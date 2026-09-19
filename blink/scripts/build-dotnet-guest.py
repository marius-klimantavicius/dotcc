#!/usr/bin/env python3
"""Publish and natively qualify the genuine C# NativeAOT guest (no emulator)."""
import hashlib
import json
import os
from pathlib import Path
import re
import selectors
import shutil
import signal
import socket
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parents[2]
CAMPAIGN = ROOT / "blink"
SOURCE = CAMPAIGN / "tests/DotNetService"


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def terminate(process):
    if process.poll() is None:
        try:
            os.killpg(process.pid, signal.SIGTERM)
        except ProcessLookupError:
            pass
        try:
            process.wait(timeout=2)
        except subprocess.TimeoutExpired:
            try:
                os.killpg(process.pid, signal.SIGKILL)
            except ProcessLookupError:
                pass
            process.wait(timeout=5)


def main():
    artifacts = CAMPAIGN / "artifacts/dotnet-guest"
    artifacts.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    receipt = {"passed": False, "scope": "native linux-x64 .NET NativeAOT guest; no emulated execution",
               "attempt": str(attempt), "commands": [], "sources": {}}
    print(attempt, flush=True)
    env = os.environ.copy()
    (attempt / "tmp").mkdir()
    env["TMPDIR"] = str(attempt / "tmp")
    project = attempt / "source"
    project.mkdir()

    def run(name, command, timeout=60, cwd=project):
        log = attempt / (name + ".log")
        item = {"name": name, "argv": list(map(str, command)), "cwd": str(cwd)}
        receipt["commands"].append(item)
        with log.open("wb") as output:
            process = subprocess.Popen(item["argv"], cwd=cwd, env=env, stdout=output,
                                       stderr=subprocess.STDOUT, start_new_session=True)
            try:
                item["exit_code"] = process.wait(timeout=timeout)
            finally:
                terminate(process)
                item["log_sha256"] = sha(log)
        if item["exit_code"] != 0:
            raise RuntimeError(f"{name} failed: {log}")
        return log.read_text()

    try:
        for name in ("Program.cs", "DotNetService.csproj", "global.json"):
            shutil.copy2(SOURCE / name, project / name)
            receipt["sources"][str((SOURCE / name).relative_to(ROOT))] = sha(SOURCE / name)
        # Freeze inherited build configuration explicitly, including the NuGet source map.
        for name in ("Directory.Build.props", "Directory.Packages.props", "nuget.config"):
            shutil.copy2(ROOT / name, project / name)
            receipt["sources"][name] = sha(ROOT / name)
        receipt["runner_sha256"] = sha(__file__)
        receipt["sdk_version"] = run("sdk-version", ["dotnet", "--version"]).strip()
        if receipt["sdk_version"] != "10.0.111":
            raise RuntimeError("unexpected SDK")
        run("dotnet-info", ["dotnet", "--info"])
        receipt["tools"] = {}
        for name in ("dotnet", "gcc", "ld", "readelf", "strace"):
            path = shutil.which(name)
            if path:
                receipt["tools"][name] = {"path": path, "resolved": str(Path(path).resolve()), "sha256": sha(path)}
                run(name + "-version", [path, "--version"])
        publish = attempt / "publish"
        run("publish", ["dotnet", "publish", "DotNetService.csproj", "-c", "Release",
                        "-r", "linux-x64", "--self-contained", "true", "-o", str(publish),
                        "-p:CppCompilerAndLinker=" + receipt["tools"]["gcc"]["path"],
                        "-p:ImportDirectoryBuildTargets=false"], timeout=600)
        assets = project / "obj/project.assets.json"
        receipt["assets_sha256"] = sha(assets)
        receipt["resolved_packages"] = sorted(json.loads(assets.read_text())["libraries"])
        receipt["runtime_download_dependencies"] = json.loads(assets.read_text())["project"]["frameworks"]["net10.0"]["downloadDependencies"]
        binary = publish / "DotNetService"
        receipt["binary"] = {"path": str(binary), "sha256": sha(binary), "size": binary.stat().st_size}
        headers = run("elf-program-headers", ["readelf", "-lW", str(binary)])
        dynamic = run("elf-dynamic", ["readelf", "-dW", str(binary)])
        run("elf-header", ["readelf", "-hW", str(binary)])
        run("elf-version-info", ["readelf", "-VW", str(binary)])
        receipt["elf"] = {"interpreter": re.findall(r"Requesting program interpreter: ([^\]]+)", headers),
                          "needed": re.findall(r"\(NEEDED\).*\[([^\]]+)\]", dynamic),
                          "tls_program_headers": [line.strip() for line in headers.splitlines() if re.match(r"\s*TLS\s", line)]}
        print(json.dumps(receipt["elf"]), flush=True)
        trace = attempt / "native.strace"
        command = [str(binary), "0"]
        if "strace" in receipt["tools"]:
            command = [receipt["tools"]["strace"]["path"], "-f", "-qq", "-s", "256", "-o", str(trace), "--"] + command
        receipt["native_command"] = command
        with (attempt / "native.stderr").open("wb") as errors:
            process = subprocess.Popen(command, cwd=attempt, env=env, stdout=subprocess.PIPE,
                                       stderr=errors, start_new_session=True)
            try:
                with selectors.DefaultSelector() as selector:
                    selector.register(process.stdout, selectors.EVENT_READ)
                    if not selector.select(20):
                        raise TimeoutError("native readiness timed out")
                    ready = process.stdout.readline()
                match = re.fullmatch(rb"READY ([0-9]+)\n", ready)
                if not match:
                    raise RuntimeError(f"invalid readiness: {ready!r}")
                port = int(match[1])
                receipt["native_cases"] = []
                for method, path, body in (("GET", "/health", b"ok\n"), ("POST", "/stop", b"stopped\n")):
                    request = f"{method} {path} HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\nConnection: close\r\n\r\n".encode("ascii")
                    expected = b"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: " + str(len(body)).encode() + b"\r\nConnection: close\r\n\r\n" + body
                    response = bytearray()
                    with socket.create_connection(("127.0.0.1", port), timeout=5) as client:
                        client.settimeout(5)
                        client.sendall(request)
                        while True:
                            chunk = client.recv(4096)
                            if not chunk:
                                break
                            response.extend(chunk)
                            if len(response) > 8192:
                                raise RuntimeError("native response exceeds limit")
                    name = path[1:]
                    (attempt / (name + ".request")).write_bytes(request)
                    (attempt / (name + ".response")).write_bytes(response)
                    if response != expected:
                        raise RuntimeError(f"native {path} response mismatch")
                    receipt["native_cases"].append({"path": path, "request_sha256": sha(attempt / (name + ".request")),
                                                    "response_sha256": sha(attempt / (name + ".response")), "passed": True})
                tail, _ = process.communicate(timeout=10)
                (attempt / "native.stdout").write_bytes(ready + tail)
                receipt["native_exit_code"] = process.returncode
                if process.returncode != 0 or tail != b"STOPPED\n" or (attempt / "native.stderr").stat().st_size:
                    raise RuntimeError("native shutdown/status mismatch")
            finally:
                terminate(process)
        if trace.exists():
            receipt["trace_sha256"] = sha(trace)
            receipt["trace_syscalls"] = sorted(set(re.findall(r"^\d+\s+([a-zA-Z0-9_]+)\(", trace.read_text(), re.M)))
        if sha(binary) != receipt["binary"]["sha256"] or sha(__file__) != receipt["runner_sha256"]:
            raise RuntimeError("binary or runner changed during qualification")
        for name, digest in receipt["sources"].items():
            if sha(ROOT / name) != digest:
                raise RuntimeError(f"source changed: {name}")
        receipt["passed"] = True
    except BaseException as error:
        receipt["error"] = f"{type(error).__name__}: {error}"
        raise
    finally:
        receipt["artifacts"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.iterdir() if p.is_file() and p.name != "receipt.json"}
        (attempt / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
        print(json.dumps({"passed": receipt["passed"], "receipt": str(attempt / "receipt.json")}), flush=True)


if __name__ == "__main__":
    main()
