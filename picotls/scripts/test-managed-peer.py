#!/usr/bin/env python3
"""Validate translated picotls against native picotls and independent SslStream peers.

Run serially after translate.sh. Use --aot for the translated NativeAOT peer,
or --raw to select pre-postprocessor output. Results retain per-process logs.
"""
import argparse
import json
import os
import platform
from pathlib import Path
import shutil
import subprocess
import tempfile
import time

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--aot", action="store_true")
parser.add_argument("--raw", action="store_true")
parser.add_argument("--no-prepare", action="store_true", help="Require existing pinned source and native oracle; never fetch or run oracle preparation")
parser.add_argument("--runtime", default="linux-x64", choices=("linux-x64", "linux-arm64"))
parser.add_argument("--receipt", type=Path, help="Write public case results after complete success")
args = parser.parse_args()
if args.receipt:
    args.receipt.unlink(missing_ok=True)
host_arch = {"x86_64": "x64", "aarch64": "arm64"}.get(platform.machine())
if platform.system() != "Linux" or (args.aot and args.runtime != "linux-" + str(host_arch)):
    raise SystemExit("This native-peer runner requires Linux; NativeAOT execution requires a RID matching the host architecture")
artifacts = root / "artifacts/managed-peer"
artifacts.mkdir(parents=True, exist_ok=True)
run = Path(tempfile.mkdtemp(prefix="run-", dir=artifacts))
temporary = root / "artifacts/tmp/managed-peer"
temporary.mkdir(parents=True, exist_ok=True)
env = dict(os.environ, TMPDIR=str(temporary))


def execute(name, argv, timeout=60):
    with (run / (name + ".log")).open("w") as log:
        subprocess.run(argv, check=True, stdout=log, stderr=subprocess.STDOUT,
                       env=env, timeout=timeout)


if args.no_prepare:
    inputs = json.loads((root / "config/inputs.json").read_text())
    source = root / "ref" / inputs["picotls"]["directory"]
    if not (source / "include/picotls.h").is_file():
        raise SystemExit("Missing pinned source; prepare it with scripts/fetch.sh before product-only tests")
else:
    source = Path(subprocess.check_output([os.environ.get("PYTHON_CMD", "python3"), str(root.parent / "Scripts/campaign-reference.py"), "picotls"], text=True).strip())
oracle = root / "build/oracle"
if not (oracle / "libpicotls-openssl.a").exists():
    if args.no_prepare:
        raise SystemExit("Missing native oracle; prepare it with scripts/oracle.sh before product-only tests")
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
managed_project = root / "samples/ManagedConsumer/ManagedConsumer.csproj"
translated_project = root / ("generated/TranslatedPicotls.Raw/TranslatedPicotls.csproj" if args.raw else "generated/TranslatedPicotls/TranslatedPicotls.csproj")
if not translated_project.exists():
    raise SystemExit("Run scripts/translate.sh before managed TLS validation")
properties = ["-p:PicotlsProject=" + str(translated_project)]
execute("build-managed", ["dotnet", "build", str(managed_project), "-c", "Release", *properties], 600)
managed = ["dotnet", str(managed_project.parent / "bin/Release/net10.0/ManagedConsumer.dll")]
if args.aot:
    publish = root / ("build/managed-peer-aot-raw" if args.raw else "build/managed-peer-aot")
    execute("publish-managed", ["dotnet", "publish", str(managed_project), "-c", "Release", "-r", args.runtime,
                               "-p:PublishAot=true", *properties, "-o", str(publish)], 900)
    managed = [str(publish / "ManagedConsumer")]
credentials = run / "credentials"
untrusted = run / "untrusted"
execute("credentials", [*independent, "credentials", str(credentials)])
execute("untrusted-credentials", [*independent, "credentials", str(untrusted)])
for suffix in (".pem", ".key.pem", ".pfx"):
    shutil.copy2(untrusted / ("client-rsa" + suffix), credentials / ("client-untrusted" + suffix))

results = []


def pair(name, managed_server, oracle_kind, cipher="TLS_AES_128_GCM_SHA256", identity="server-ecdsa",
         client_identity="", require_client=False, byte_count=65537, update_key=False,
         client_credentials=credentials, expected_success=True, force_unoffered_alpn=False):
    oracle_binary = [str(native)] if oracle_kind == "native" else independent if oracle_kind == "independent" else managed
    server_binary = managed if managed_server else oracle_binary
    client_binary = oracle_binary if managed_server else managed
    ready = run / (name + ".ready.json")
    server_log = run / (name + ".server.log")
    client_log = run / (name + ".client.log")
    server_args = [*server_binary, "server", "--credentials", str(credentials),
                   "--identity", identity, "--require-client-cert", str(require_client).lower(), "--ready", str(ready)]
    client_args = [*client_binary, "client", "--credentials", str(client_credentials),
                   "--identity", client_identity, "--bytes", str(byte_count)]
    if force_unoffered_alpn:
        if managed_server or oracle_kind != "native":
            raise ValueError("malicious ALPN fixture requires a native server")
        server_args.extend(["--force-unoffered-alpn", "true"])
    for peer_args, is_managed, is_native in (
        (server_args, managed_server or oracle_kind == "managed", not managed_server and oracle_kind == "native"),
        (client_args, not managed_server or oracle_kind == "managed", managed_server and oracle_kind == "native")):
        if is_managed or is_native:
            peer_args.extend(["--cipher", cipher, "--update-key", str(update_key).lower()])
        if is_managed:
            peer_args.extend(["--revocation", "NoCheck"])
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
        if oracle_kind == "independent" and require_client:
            independent_result = client_result if managed_server else server_result
            if not independent_result["MutualAuthentication"]:
                raise RuntimeError(f"{name}: independent peer did not authenticate both sides")
    else:
        if client.returncode == 0 or server_status == 0:
            raise RuntimeError(f"{name}: both peers should fail")
        rejecting_log = server_log if require_client else client_log
        rejecting_managed = managed_server if require_client else not managed_server
        if rejecting_managed or oracle_kind == "managed":
            required_errors = ("PicotlsException:", "AuthenticationException:")
        elif oracle_kind == "native":
            required_errors = ("NativePeerError: handshake",)
        else:
            required_errors = ("AuthenticationException:",)
        if not rejecting_log.read_text().startswith(required_errors):
            raise RuntimeError(f"{name}: failure was not the expected authentication rejection")
    print(f"PASS {name}", flush=True)
    results.append({"case": name, "accepted": expected_success,
                    **({"protocol": client_result["Protocol"], "cipher": client_result["Cipher"],
                        "alpn": client_result["Alpn"], "bytes": client_result["Bytes"], "sha256": client_result["Sha256"]}
                       if expected_success else {})})


for oracle_kind in ("native", "independent"):
    for managed_server in (False, True):
        role = oracle_kind + ("-managed-server" if managed_server else "-managed-client")
        for cipher in ("TLS_AES_128_GCM_SHA256", "TLS_AES_256_GCM_SHA384"):
            for identity in ("server-rsa", "server-ecdsa"):
                pair(f"{role}-{cipher}-{identity}", managed_server, oracle_kind, cipher=cipher, identity=identity)
        pair(role + "-empty", managed_server, oracle_kind, byte_count=0)
        pair(role + "-large-key-update", managed_server, oracle_kind, byte_count=262145, update_key=True)
        pair(role + "-mutual-rsa", managed_server, oracle_kind, client_identity="client-rsa", require_client=True)
        pair(role + "-mutual-ecdsa", managed_server, oracle_kind, client_identity="client-ecdsa", require_client=True)
        pair(role + "-wrong-name", managed_server, oracle_kind, identity="server-wrong-name", expected_success=False)
        pair(role + "-expired", managed_server, oracle_kind, identity="server-expired", expected_success=False)
        pair(role + "-untrusted", managed_server, oracle_kind, client_credentials=untrusted, expected_success=False)
        pair(role + "-missing-client", managed_server, oracle_kind, require_client=True, expected_success=False)
        pair(role + "-untrusted-client", managed_server, oracle_kind, client_identity="client-untrusted", require_client=True, expected_success=False)
for cipher in ("TLS_AES_128_GCM_SHA256", "TLS_AES_256_GCM_SHA384"):
    pair("managed-both-" + cipher, True, "managed", cipher=cipher, update_key=True)
pair("reject-server-selected-unoffered-alpn", False, "native", expected_success=False, force_unoffered_alpn=True)
pair("reject-key-encipherment-only-leaf", False, "native", identity="server-key-encipherment-rsa", expected_success=False)
receipt = {"cases": results, "aot": args.aot, "raw": args.raw, "runtime": args.runtime, "evidence": str(run)}
(run / "PASS.json").write_text(json.dumps(receipt, indent=2) + "\n")
if args.receipt:
    args.receipt.parent.mkdir(parents=True, exist_ok=True)
    args.receipt.write_text(json.dumps(receipt, indent=2) + "\n")
print(f"Translated TLS interoperability passed; evidence: {run}")
