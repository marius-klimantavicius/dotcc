"""Real positive/negative native TLS exchanges for the labelled PSK CLI peer."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import signal
import socket
import subprocess
import threading
import time

ROOT = Path(__file__).resolve().parents[1]


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


class Endpoint:
    def __init__(self, command, directory, name):
        self.command = list(map(str, command))
        self.process = subprocess.Popen(self.command, cwd=ROOT / "ref/s2n-tls",
            env={**os.environ, "S2N_DONT_MLOCK": "1"}, stdin=subprocess.PIPE,
            stdout=subprocess.PIPE, stderr=subprocess.PIPE, start_new_session=True)
        self.data = {"stdout": bytearray(), "stderr": bytearray()}
        self.threads = []
        self.directory, self.name = directory, name
        for stream in self.data:
            thread = threading.Thread(target=self.read, args=(stream,), daemon=True)
            thread.start()
            self.threads.append(thread)

    def read(self, stream):
        pipe = getattr(self.process, stream)
        while chunk := os.read(pipe.fileno(), 65536):
            self.data[stream].extend(chunk)

    def wait_for(self, marker, timeout=12):
        deadline = time.monotonic() + timeout
        while time.monotonic() < deadline:
            if marker in self.data["stdout"] or marker in self.data["stderr"]:
                return
            if self.process.poll() is not None:
                break
            time.sleep(0.01)
        raise RuntimeError(self.name + " did not produce " + repr(marker))

    def send(self, data):
        self.process.stdin.write(data)
        self.process.stdin.flush()

    def finish(self):
        if not self.process.stdin.closed:
            self.process.stdin.close()
        try:
            code = self.process.wait(timeout=8)
        except subprocess.TimeoutExpired:
            os.killpg(self.process.pid, signal.SIGTERM)
            try:
                self.process.wait(timeout=3)
            except subprocess.TimeoutExpired:
                os.killpg(self.process.pid, signal.SIGKILL)
                self.process.wait()
            code = "drain-timeout"
        for thread in self.threads:
            thread.join(timeout=2)
        paths = {}
        for stream, data in self.data.items():
            path = self.directory / (self.name + "." + stream)
            path.write_bytes(data)
            paths[stream] = {"path": str(path), "sha256": digest(path)}
        return {"command": self.command, "pid": self.process.pid, "exit_code": code,
                "logs": paths, "drained": self.process.poll() is not None}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--peer-receipt", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    peer_receipt = args.peer_receipt.resolve()
    peer = json.loads(peer_receipt.read_text())
    if peer["status"] != "passed":
        raise RuntimeError("Derived peer build did not pass")
    binary = Path(peer["derived"]) / "apps/openssl"
    native = ROOT / "build/native/s2n-adapted/bin"
    inputs = {**peer["inputs"], **peer["outputs"], str(peer_receipt): digest(peer_receipt),
              str(Path(__file__).resolve()): digest(Path(__file__)),
              **{str(native / name): digest(native / name) for name in ("s2nc", "s2nd")},
              **{str(ROOT / "build/native/credentials" / name): digest(ROOT / "build/native/credentials" / name)
                 for name in ("rsa.pem", "rsa.key")}}
    for name, expected in inputs.items():
        if digest(Path(name)) != expected:
            raise RuntimeError("PSK proof input changed: " + name)
    output = args.output.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {"status": "running", "scope": __doc__, "inputs": inputs,
              "derived_peer_build": str(peer_receipt), "cases": []}
    secret = "00112233445566778899aabbccddeeff" * 2
    identity = "dotcc-native-proof-identity"
    configs = [(role, cipher, variant) for role in ("client", "server")
               for cipher in ("TLS_AES_128_GCM_SHA256", "TLS_AES_256_GCM_SHA384")
               for variant in ("positive", "wrong-secret", "wrong-identity")]
    # Bind before launch only to select an unused port, never connect to consume
    # a test server's sole accept. Bounded private diagnostic range, not full runs.
    next_port = 62000
    try:
        for role, cipher, variant in configs:
            while True:
                with socket.socket() as reservation:
                    try:
                        reservation.bind(("127.0.0.1", next_port))
                    except OSError:
                        next_port += 1
                        continue
                port = next_port
                next_port += 1
                break
            name = role + "-" + cipher + "-" + variant
            directory = output / name
            directory.mkdir()
            algorithm = "SHA384" if "SHA384" in cipher else "SHA256"
            client_secret = "deadbeef" if variant == "wrong-secret" else secret
            client_identity = "wrong-identity" if variant == "wrong-identity" else identity
            ssl_options = ["-tls1_3", "-ciphersuites", cipher]
            if role == "client":
                server_command = [native / "s2nd", "--max-conns=1", "--self-service-blinding",
                    "-c", "test_all_tls13", "-T", "--psk", identity + "," + secret + "," + algorithm,
                    "--cert", ROOT / "build/native/credentials/rsa.pem", "--key", ROOT / "build/native/credentials/rsa.key",
                    "127.0.0.1", str(port)]
                client_command = [binary, "s_client", "-connect", "127.0.0.1:" + str(port),
                    "-quiet", "-no_ign_eof", "-psk_identity", client_identity, "-psk", client_secret, *ssl_options]
                ready = b"Listening on"
            else:
                server_command = [binary, "s_server", "-accept", "127.0.0.1:" + str(port),
                    "-naccept", "1", "-nocert", "-rev", "-psk_identity", identity, "-psk", secret, *ssl_options]
                client_command = [native / "s2nc", "-e", "-T", "--insecure", "-c", "test_all_tls13",
                    "--psk", client_identity + "," + client_secret + "," + algorithm, "127.0.0.1", str(port)]
                ready = b"ACCEPT"
            row = {"name": name, "derived_role": role, "cipher": cipher, "variant": variant, "status": "running"}
            endpoints = []
            try:
                server = Endpoint(server_command, directory, "server"); endpoints.append(server)
                server.wait_for(ready)
                client = Endpoint(client_command, directory, "client"); endpoints.append(client)
                full_fallback = role == "client" and variant == "wrong-identity"
                if variant == "positive" or full_fallback:
                    s2n = server if role == "client" else client
                    if not full_fallback:
                        s2n.wait_for(("Negotiated PSK identity: " + identity).encode())
                        row["psk_identity_verified"] = True
                    s2n.wait_for(cipher.encode())
                    message = b"actual-native-psk-payload-0123456789\n"
                    client.send(message)
                    if role == "client":
                        server.wait_for(message)
                        response = b"actual-native-server-response-9876543210\n"
                        server.send(response)
                    else:
                        response = message.rstrip(b"\n")[::-1] + b"\n"
                    client.wait_for(response)
                    row["payload_verified"] = True
                    if full_fallback:
                        if b"Negotiated PSK identity:" in s2n.data["stdout"]:
                            raise RuntimeError("Unknown identity unexpectedly selected a PSK")
                        row["full_handshake_fallback"] = True
                else:
                    client.process.stdin.close()
                    client.process.wait(timeout=12)
                    if client.process.returncode == 0:
                        raise RuntimeError("Invalid PSK unexpectedly succeeded")
                    if role == "server":
                        client.wait_for(b"Failed to negotiate")
                    else:
                        server.wait_for(b"PSK binder did not match the expected value")
                    row["rejected"] = True
                row["status"] = "passed"
            except BaseException as error:
                row["status"] = "failed"
                row["error"] = str(error)
            finally:
                row["endpoints"] = [endpoint.finish() for endpoint in reversed(endpoints)]
                if (variant == "positive" or (role == "client" and variant == "wrong-identity")) and any(item["exit_code"] != 0 for item in row["endpoints"]):
                    row["status"] = "failed"
                    row.setdefault("error", "Positive endpoint did not exit cleanly")
                report["cases"].append(row)
                (output / "report.json").write_text(json.dumps(report, indent=2) + "\n")
        for name, expected in inputs.items():
            if digest(Path(name)) != expected:
                raise RuntimeError("PSK proof changed input: " + name)
        report["passed"] = sum(row["status"] == "passed" for row in report["cases"])
        report["failed"] = len(report["cases"]) - report["passed"]
        report["status"] = "passed" if report["failed"] == 0 else "failed"
    finally:
        (output / "report.json").write_text(json.dumps(report, indent=2) + "\n")
    print(output / "report.json")
    if report["status"] != "passed":
        raise SystemExit(1)


if __name__ == "__main__":
    main()
