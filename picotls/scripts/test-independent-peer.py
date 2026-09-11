#!/usr/bin/env python3
"""Build and validate the independent SslStream TLS 1.3 oracle, serially.

This validates the oracle itself, not translated picotls. --aot also publishes
and executes the same process-level positive and negative checks with NativeAOT.
"""
import argparse
import json
import os
from pathlib import Path
import subprocess
import tempfile
import time

root = Path(__file__).resolve().parents[1]
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--aot", action="store_true")
args = parser.parse_args()
project = root / "tests/IndependentPeer/IndependentPeer.csproj"
artifacts = root / "artifacts/independent-peer"
artifacts.mkdir(parents=True, exist_ok=True)
run = Path(tempfile.mkdtemp(prefix="run-", dir=artifacts))
tmp = root / "artifacts/tmp/independent-peer"
tmp.mkdir(parents=True, exist_ok=True)
env = dict(os.environ, TMPDIR=str(tmp))


def command(name, argv, timeout=60):
    with (run / (name + ".log")).open("w") as log:
        subprocess.run(argv, check=True, stdout=log, stderr=subprocess.STDOUT,
                       env=env, timeout=timeout)


command("build", ["dotnet", "build", str(project), "-c", "Release"], 600)
managed = ["dotnet", str(project.parent / "bin/Release/net10.0/IndependentPeer.dll")]
credentials = run / "credentials"
untrusted = run / "untrusted"
command("credentials", [*managed, "credentials", str(credentials)])
command("untrusted-credentials", [*managed, "credentials", str(untrusted)])


def pair(executable, name, server_identity="server-ecdsa", client_identity="",
         byte_count=65537, client_credentials=credentials, require_client=False,
         expected_success=True):
    ready = run / (name + ".ready.json")
    server_log = run / (name + ".server.log")
    client_log = run / (name + ".client.log")
    server_args = [*executable, "server", "--credentials", str(credentials),
                   "--identity", server_identity, "--ready", str(ready),
                   "--require-client-cert", str(require_client).lower()]
    with server_log.open("w") as output:
        server = subprocess.Popen(server_args, stdout=output, stderr=subprocess.STDOUT, env=env)
        try:
            until = time.monotonic() + 15
            while not ready.exists():
                if server.poll() is not None:
                    raise RuntimeError(f"{name}: server stopped before ready; see {server_log}")
                if time.monotonic() >= until:
                    raise TimeoutError(f"{name}: server not ready")
                time.sleep(0.05)
            port = json.loads(ready.read_text())["Port"]
            with client_log.open("w") as client_output:
                client = subprocess.run([*executable, "client", "--credentials", str(client_credentials),
                                         "--port", str(port), "--bytes", str(byte_count),
                                         "--identity", client_identity], stdout=client_output,
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
        for key in ("Protocol", "Cipher", "Alpn", "Bytes", "Sha256", "MutualAuthentication"):
            if client_result[key] != server_result[key]:
                raise RuntimeError(f"{name}: peers disagree about {key}")
        if client_result["Bytes"] != byte_count or client_result["Protocol"] != "Tls13":
            raise RuntimeError(f"{name}: incorrect payload or protocol")
        if require_client and not client_result["MutualAuthentication"]:
            raise RuntimeError(f"{name}: mutual authentication required")
    else:
        if client.returncode == 0 or server_status == 0:
            raise RuntimeError(f"{name}: expected both peers to reject authentication")
        rejecting_log = server_log if require_client else client_log
        if not rejecting_log.read_text().startswith("AuthenticationException:"):
            raise RuntimeError(f"{name}: expected authentication rejection, not an unrelated process failure")
    print(f"PASS {name}", flush=True)


def suite(executable, mode):
    pair(executable, mode + "-ecdsa-empty", byte_count=0)
    pair(executable, mode + "-rsa-fragmented", server_identity="server-rsa")
    pair(executable, mode + "-ecdsa-large", byte_count=262145)
    pair(executable, mode + "-mutual-rsa", client_identity="client-rsa", require_client=True)
    pair(executable, mode + "-mutual-ecdsa", server_identity="server-rsa",
         client_identity="client-ecdsa", require_client=True)
    pair(executable, mode + "-wrong-name", server_identity="server-wrong-name", expected_success=False)
    pair(executable, mode + "-expired", server_identity="server-expired", expected_success=False)
    pair(executable, mode + "-untrusted", client_credentials=untrusted, expected_success=False)
    pair(executable, mode + "-missing-client-certificate", require_client=True, expected_success=False)


suite(managed, "jit")
if args.aot:
    publish = root / "build/independent-peer-aot"
    command("publish-aot", ["dotnet", "publish", str(project), "-c", "Release",
                            "-r", "linux-x64", "-p:PublishAot=true", "-o", str(publish)], 600)
    suite([str(publish / "IndependentPeer")], "aot")
print(f"Independent peer validation passed; evidence: {run}")
