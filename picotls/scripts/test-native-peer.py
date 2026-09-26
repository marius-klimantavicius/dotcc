#!/usr/bin/env python3
"""Validate native picotls/OpenSSL against independent SslStream peers.

Run serially. This establishes test-oracle interoperability, not managed TLS.
Use --aot-peer to run the SslStream side as a Linux x64 NativeAOT executable.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import time

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--aot-peer", action="store_true")
args = parser.parse_args()
artifacts = root / "artifacts/native-peer"
artifacts.mkdir(parents=True, exist_ok=True)
run = Path(tempfile.mkdtemp(prefix="run-", dir=artifacts))
temporary = root / "artifacts/tmp/native-peer"
temporary.mkdir(parents=True, exist_ok=True)
env = dict(os.environ, TMPDIR=str(temporary))


def execute(name, argv, timeout=60):
    with (run / (name + ".log")).open("w") as log:
        subprocess.run(argv, check=True, stdout=log, stderr=subprocess.STDOUT,
                       env=env, timeout=timeout)


source = Path(subprocess.check_output([os.environ.get("PYTHON_CMD", "python3"), str(root.parent / "Scripts/campaign-reference.py"), "picotls"], text=True).strip())
oracle = root / "build/oracle"
if not (oracle / "libpicotls-openssl.a").exists():
    execute("oracle", [str(root / "scripts/oracle.sh")], 600)
native = root / "build/native-peer/NativePeer"
native.parent.mkdir(parents=True, exist_ok=True)
execute("build-native", ["cc", "-std=c17", "-O2", "-g", "-Wall", "-Wextra", "-Werror",
                         "-DPTLS_HAVE_LOG=0", "-DPICOTLS_USE_DTRACE=0", "-isystem", str(source / "include"),
                         str(root / "tests/NativePeer/main.c"), "-L", str(oracle),
                         "-lpicotls-openssl", "-lpicotls-core", "-lssl", "-lcrypto", "-pthread", "-o", str(native)], 120)
project = root / "tests/IndependentPeer/IndependentPeer.csproj"
execute("build-independent", ["dotnet", "build", str(project), "-c", "Release"], 600)
independent = ["dotnet", str(project.parent / "bin/Release/net10.0/IndependentPeer.dll")]
if args.aot_peer:
    publish = root / "build/independent-peer-aot"
    execute("publish-independent", ["dotnet", "publish", str(project), "-c", "Release", "-r", "linux-x64",
                                    "-p:PublishAot=true", "-o", str(publish)], 600)
    independent = [str(publish / "IndependentPeer")]
credentials = run / "credentials"
untrusted = run / "untrusted"
execute("credentials", [*independent, "credentials", str(credentials)])
execute("untrusted-credentials", [*independent, "credentials", str(untrusted)])
for suffix in (".pem", ".key.pem", ".pfx"):
    shutil.copy2(untrusted / ("client-rsa" + suffix), credentials / ("client-untrusted" + suffix))


def pair(name, native_server, cipher="TLS_AES_128_GCM_SHA256", identity="server-ecdsa",
         client_identity="", require_client=False, byte_count=65537, update_key=False,
         client_credentials=credentials, expected_success=True):
    server_binary = [str(native)] if native_server else independent
    client_binary = independent if native_server else [str(native)]
    ready = run / (name + ".ready.json")
    server_log = run / (name + ".server.log")
    client_log = run / (name + ".client.log")
    server_args = [*server_binary, "server", "--credentials", str(credentials),
                   "--identity", identity, "--require-client-cert", str(require_client).lower(), "--ready", str(ready)]
    client_args = [*client_binary, "client", "--credentials", str(client_credentials),
                   "--identity", client_identity, "--bytes", str(byte_count)]
    native_args = server_args if native_server else client_args
    native_args.extend(["--cipher", cipher, "--update-key", str(update_key).lower()])
    with server_log.open("w") as output:
        server = subprocess.Popen(server_args, stdout=output, stderr=subprocess.STDOUT, env=env)
        try:
            until = time.monotonic() + 15
            while not ready.exists():
                if server.poll() is not None:
                    raise RuntimeError(f"{name}: server failed before ready; see {server_log}")
                if time.monotonic() >= until:
                    raise TimeoutError(f"{name}: server readiness timeout")
                time.sleep(0.05)
            port = json.loads(ready.read_text())["Port"]
            with client_log.open("w") as client_output:
                client = subprocess.run([*client_args, "--port", str(port)], stdout=client_output,
                                        stderr=subprocess.STDOUT, env=env, timeout=40)
            server_status = server.wait(timeout=40)
        finally:
            if server.poll() is None:
                server.kill()
                server.wait()
    if expected_success:
        if client.returncode != 0 or server_status != 0:
            raise RuntimeError(f"{name}: peer failed; see {server_log} and {client_log}")
        client_result = json.loads(client_log.read_text().strip().splitlines()[-1])
        server_result = json.loads(server_log.read_text().strip().splitlines()[-1])
        for key in ("Protocol", "Cipher", "Alpn", "Bytes", "Sha256"):
            if client_result[key] != server_result[key]:
                raise RuntimeError(f"{name}: peers disagree on {key}")
        if client_result["Bytes"] != byte_count or client_result["Cipher"] != cipher:
            raise RuntimeError(f"{name}: incorrect payload/cipher")
        independent_result = client_result if native_server else server_result
        if require_client and not independent_result["MutualAuthentication"]:
            raise RuntimeError(f"{name}: missing mutual authentication")
    else:
        if client.returncode == 0 or server_status == 0:
            raise RuntimeError(f"{name}: both peers should fail")
        rejecting_log = server_log if require_client else client_log
        rejecting_native = native_server if require_client else not native_server
        required_error = "NativePeerError: handshake" if rejecting_native else "AuthenticationException:"
        if not rejecting_log.read_text().startswith(required_error):
            raise RuntimeError(f"{name}: failure was not the expected authentication rejection")
    print(f"PASS {name}", flush=True)


for native_server in (False, True):
    role = "native-server" if native_server else "native-client"
    for cipher in ("TLS_AES_128_GCM_SHA256", "TLS_AES_256_GCM_SHA384"):
        for identity in ("server-rsa", "server-ecdsa"):
            pair(f"{role}-{cipher}-{identity}", native_server, cipher=cipher, identity=identity)
    pair(role + "-empty", native_server, byte_count=0)
    pair(role + "-large-key-update", native_server, byte_count=262145, update_key=True)
    pair(role + "-mutual-rsa", native_server, client_identity="client-rsa", require_client=True)
    pair(role + "-mutual-ecdsa", native_server, client_identity="client-ecdsa", require_client=True)
    pair(role + "-wrong-name", native_server, identity="server-wrong-name", expected_success=False)
    pair(role + "-expired", native_server, identity="server-expired", expected_success=False)
    pair(role + "-untrusted", native_server, client_credentials=untrusted, expected_success=False)
    pair(role + "-missing-client", native_server, require_client=True, expected_success=False)
    pair(role + "-untrusted-client", native_server, client_identity="client-untrusted",
         require_client=True, expected_success=False)
print(f"Native/independent oracle validation passed; evidence: {run}")
