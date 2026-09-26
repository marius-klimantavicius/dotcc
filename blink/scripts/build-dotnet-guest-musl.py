#!/usr/bin/env python3
"""One ordinary pinned-container static-musl publish and native HTTP witness."""

# Source revisions and reviewed fingerprints are data, not executable policy.
import json as _campaign_json
from pathlib import Path as _CampaignPath
_CAMPAIGN_ROOT = next(parent for parent in _CampaignPath(__file__).resolve().parents
                      if (parent / "config/source-manifest.json").is_file())
_CAMPAIGN_INPUTS = _campaign_json.loads((_CAMPAIGN_ROOT / "config/script-inputs.json").read_text())['scripts/build-dotnet-guest-musl.py']

import argparse
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
import time

ROOT = Path(__file__).resolve().parents[2]
BLINK = ROOT / "blink"
SOURCE = BLINK / "tests/DotNetService"
IMAGE_TAG = "mcr.microsoft.com/dotnet/sdk:10.0.401-alpine3.23-aot-amd64"
IMAGE_DIGEST = _CAMPAIGN_INPUTS['IMAGE_DIGEST']
IMAGE = IMAGE_TAG + "@" + IMAGE_DIGEST
SDK = "10.0.401"
RUNTIME = "10.0.12"


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def stop(process):
    if process.poll() is not None:
        return
    for sig, seconds in ((signal.SIGTERM, 3), (signal.SIGKILL, 5)):
        try:
            os.killpg(process.pid, sig)
        except ProcessLookupError:
            break
        try:
            process.wait(timeout=seconds)
            break
        except subprocess.TimeoutExpired:
            continue


def interrupted(number, frame):
    raise InterruptedError(f"received signal {number}")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--engine", choices=("podman", "docker"), default="podman")
    args = parser.parse_args()
    artifacts = BLINK / "artifacts/dotnet-guest-musl"
    artifacts.mkdir(parents=True, exist_ok=True)
    attempt = Path(tempfile.mkdtemp(prefix="attempt-", dir=artifacts))
    receipt = {"passed": False, "attempt": str(attempt), "commands": [],
               "scope": "one standard static-musl publish and native Linux HTTP; no Blink execution",
               "image": {"tag": IMAGE_TAG, "digest": IMAGE_DIGEST, "reference": IMAGE},
               "sdk": SDK, "runtime": RUNTIME, "sources": {}, "tools": {}}
    print(attempt, flush=True)
    env = os.environ.copy()
    for name in ("tmp", "source", "home", "nuget"):
        (attempt / name).mkdir()
    env["TMPDIR"] = str(attempt / "tmp")
    engine = shutil.which(args.engine)
    container = "blink-musl-" + attempt.name.removeprefix("attempt-").replace("_", "-")
    container_attempted = False

    def save():
        temporary = attempt / "receipt.tmp"
        temporary.write_text(json.dumps(receipt, indent=2) + "\n")
        temporary.replace(attempt / "receipt.json")

    def run(name, argv, timeout=60, required=True):
        log = attempt / (name + ".log")
        row = {"name": name, "argv": list(map(str, argv)), "timeout_seconds": timeout,
               "cwd": str(attempt), "log": str(log)}
        receipt["commands"].append(row)
        process = None
        started = time.monotonic()
        try:
            with log.open("wb") as output:
                process = subprocess.Popen(row["argv"], cwd=attempt, env=env,
                                           stdout=output, stderr=subprocess.STDOUT,
                                           start_new_session=True)
                row["exit_code"] = process.wait(timeout=timeout)
        except BaseException as error:
            row["error"] = f"{type(error).__name__}: {error}"
            raise
        finally:
            if process is not None:
                stop(process)
                row["final_exit_code"] = process.poll()
            row["elapsed_seconds"] = time.monotonic() - started
            if log.exists():
                row["log_sha256"] = sha(log)
            save()
        if required and row["exit_code"] != 0:
            raise RuntimeError(f"{name} failed; stop and report, no automatic retry: {log}")
        return log.read_text()

    try:
        receipt["runner_sha256"] = sha(__file__)
        shutil.copy2(__file__, attempt / "runner.py")
        if not engine:
            raise RuntimeError(f"{args.engine} is unavailable; stop and report")
        for name in (args.engine, "readelf", "strace"):
            path = shutil.which(name)
            if not path:
                raise RuntimeError(f"required tool missing: {name}; stop and report")
            receipt["tools"][name] = {"path": path, "resolved": str(Path(path).resolve()), "sha256": sha(path)}
            run(name + "-version", [path, "--version"])
        run("engine-info", [engine, "info"])
        for relative, target in (
                ("blink/tests/DotNetService/Program.cs", "source/Program.cs"),
                ("blink/tests/DotNetService/DotNetService.csproj", "source/DotNetService.csproj"),
                ("blink/tests/DotNetService/global.json", "original-global.json"),
                ("Directory.Build.props", "source/Directory.Build.props"),
                ("Directory.Packages.props", "source/Directory.Packages.props"),
                ("nuget.config", "source/nuget.config")):
            receipt["sources"][relative] = sha(ROOT / relative)
            shutil.copy2(ROOT / relative, attempt / target)
        global_json = {"sdk": {"version": SDK, "rollForward": "disable", "allowPrerelease": False}}
        (attempt / "source/global.json").write_text(json.dumps(global_json, indent=2) + "\n")
        receipt["global_json_override"] = {"original_sha256": sha(attempt / "original-global.json"),
                                           "derived_sha256": sha(attempt / "source/global.json"),
                                           "reason": "separate pinned official Alpine SDK profile", "value": global_json}
        # A fixed shell script avoids interpolating host paths or project contents.
        script = '''#!/bin/sh
set -eu
dotnet --version > /work/container-sdk.txt
test "$(cat /work/container-sdk.txt)" = "10.0.401"
dotnet --info > /work/container-dotnet-info.txt
dotnet --list-runtimes > /work/container-runtimes.txt
cat /etc/os-release > /work/container-os-release.txt
apk info -vv > /work/container-apk.txt
for tool in dotnet clang cc ld; do
  path=$(command -v "$tool")
  printf '%s %s\\n' "$tool" "$path" >> /work/container-tool-paths.txt
  sha256sum "$(readlink -f "$path")" >> /work/container-tool-hashes.txt
  "$tool" --version >> /work/container-tool-versions.txt
done
dotnet publish DotNetService.csproj -c Release -r linux-musl-x64 --self-contained true \\
  -o /work/publish -p:RuntimeFrameworkVersion=10.0.12 -p:PublishAot=true \\
  -p:StaticExecutable=true -p:PositionIndependentExecutable=false \\
  -p:InvariantGlobalization=true -p:ImportDirectoryBuildTargets=false
'''
        (attempt / "publish.sh").write_text(script)
        receipt["publish_script_sha256"] = sha(attempt / "publish.sh")
        frozen = {str(p.relative_to(attempt)): sha(p) for p in (attempt / "source").iterdir() if p.is_file()}
        frozen["publish.sh"] = sha(attempt / "publish.sh")
        frozen["runner.py"] = sha(attempt / "runner.py")
        receipt["frozen_inputs"] = frozen
        save()
        run("image-pull", [engine, "pull", IMAGE], timeout=600)
        inspect = json.loads(run("image-inspect", [engine, "image", "inspect", IMAGE]))
        if len(inspect) != 1 or inspect[0].get("Architecture") != "amd64" or inspect[0].get("Os") != "linux":
            raise RuntimeError("unexpected container image platform")
        actual = inspect[0]
        if IMAGE_DIGEST not in [actual.get("Digest")] and not any(x.endswith("@" + IMAGE_DIGEST) for x in actual.get("RepoDigests", [])):
            raise RuntimeError("container image digest mismatch")
        image_env = actual.get("Config", {}).get("Env", [])
        if f"DOTNET_SDK_VERSION={SDK}" not in image_env or f"DOTNET_VERSION={RUNTIME}" not in image_env:
            raise RuntimeError("container SDK/runtime metadata differs from pin")
        receipt["image"]["id"] = actual["Id"]
        container_attempted = True
        run("container-publish", [engine, "run", "--name", container, "--pull=never",
                                 "--volume", f"{attempt}:/work:rw", "--workdir", "/work/source",
                                 "--env", "DOTNET_CLI_HOME=/work/home", "--env", "NUGET_PACKAGES=/work/nuget",
                                 "--env", "TMPDIR=/work/tmp", "--env", "DOTNET_CLI_TELEMETRY_OPTOUT=1",
                                 IMAGE, "/bin/sh", "/work/publish.sh"], timeout=900)
        run("container-inspect", [engine, "inspect", container])
        assets = attempt / "source/obj/project.assets.json"
        parsed = json.loads(assets.read_text())
        receipt["assets_sha256"] = sha(assets)
        receipt["resolved_packages"] = sorted(parsed["libraries"])
        receipt["runtime_download_dependencies"] = parsed["project"]["frameworks"]["net10.0"].get("downloadDependencies", [])
        if f"Microsoft.DotNet.ILCompiler/{RUNTIME}" not in receipt["resolved_packages"]:
            raise RuntimeError("resolved NativeAOT compiler version differs from runtime pin")
        receipt["package_manifests"] = {str(p.relative_to(attempt)): sha(p)
                                        for p in (attempt / "nuget").rglob("*")
                                        if p.is_file() and (p.name.endswith((".nupkg.sha512", ".nupkg"))
                                                            or p.name in (".nupkg.metadata", "ilc"))}
        binary = attempt / "publish/DotNetService"
        receipt["binary"] = {"path": str(binary), "sha256": sha(binary), "size": binary.stat().st_size}
        readelf = receipt["tools"]["readelf"]["path"]
        header = run("elf-header", [readelf, "-hW", binary])
        headers = run("elf-program-headers", [readelf, "-lW", binary])
        dynamic = run("elf-dynamic", [readelf, "-dW", binary])
        run("elf-version-info", [readelf, "-VW", binary])
        receipt["elf"] = {"interpreter": re.findall(r"Requesting program interpreter: ([^\]]+)", headers),
                          "needed": re.findall(r"\(NEEDED\).*\[([^\]]+)\]", dynamic),
                          "tls_program_headers": [x.strip() for x in headers.splitlines() if re.match(r"\s*TLS\s", x)]}
        if receipt["elf"]["interpreter"] or receipt["elf"]["needed"] or re.search(r"^\s*INTERP\s", headers, re.M):
            raise RuntimeError("publish is not a static ELF; stop and report")
        if not re.search(r"Type:\s+EXEC\b", header) or not re.search(r"Machine:\s+Advanced Micro Devices X86-64", header):
            raise RuntimeError("expected non-PIE x86-64 executable; stop and report")
        receipt["static_elf_verified"] = True
        native_oracle(attempt, binary, receipt, env)
        for relative, digest in receipt["sources"].items():
            if sha(ROOT / relative) != digest:
                raise RuntimeError(f"authored source changed: {relative}")
        for relative, digest in frozen.items():
            if sha(attempt / relative) != digest:
                raise RuntimeError(f"frozen input changed: {relative}")
        for tool in receipt["tools"].values():
            if sha(tool["path"]) != tool["sha256"]:
                raise RuntimeError("host tool identity changed")
        if sha(binary) != receipt["binary"]["sha256"] or sha(__file__) != receipt["runner_sha256"]:
            raise RuntimeError("binary/runner identity changed")
        receipt["final_identities_stable"] = True
        receipt["passed"] = True
    except BaseException as error:
        receipt["error"] = f"{type(error).__name__}: {error}"
        receipt["disposition"] = "STOP and report to coordinator/root; no automatic alternate toolchain or retry"
        raise
    finally:
        if container_attempted:
            try:
                run("container-final-inspect", [engine, "inspect", container], required=False)
                run("container-remove", [engine, "rm", "--force", container], timeout=30)
            except BaseException as error:
                receipt["passed"] = False
                receipt["cleanup_error"] = f"{type(error).__name__}: {error}"
        receipt["artifacts"] = {str(p.relative_to(attempt)): sha(p) for p in attempt.iterdir()
                                if p.is_file() and p.name not in ("receipt.json", "receipt.tmp")}
        save()
        print(json.dumps({"passed": receipt["passed"], "receipt": str(attempt / "receipt.json")}), flush=True)


def native_oracle(attempt, binary, receipt, env):
    trace = attempt / "native.strace"
    command = [receipt["tools"]["strace"]["path"], "-f", "-qq", "-s", "256", "-o", str(trace), "--", str(binary), "0"]
    receipt["native_command"] = command
    captured = bytearray()
    communicated = False
    with (attempt / "native.stderr").open("wb") as errors:
        process = subprocess.Popen(command, cwd=attempt, env=env, stdout=subprocess.PIPE,
                                   stderr=errors, start_new_session=True)
        try:
            deadline = time.monotonic() + 20
            with selectors.DefaultSelector() as selector:
                selector.register(process.stdout, selectors.EVENT_READ)
                while b"\n" not in captured:
                    remaining = deadline - time.monotonic()
                    if remaining <= 0 or not selector.select(remaining):
                        raise TimeoutError("native readiness timed out")
                    chunk = os.read(process.stdout.fileno(), 4096)
                    if not chunk:
                        raise RuntimeError("native exited before readiness")
                    captured.extend(chunk)
                    if len(captured) > 4096:
                        raise RuntimeError("native readiness exceeds limit")
            match = re.fullmatch(rb"READY ([0-9]+)\n", captured)
            if not match:
                raise RuntimeError(f"invalid readiness: {bytes(captured)!r}")
            receipt["native_cases"] = []
            for method, path, body in (("GET", "/health", b"ok\n"), ("POST", "/stop", b"stopped\n")):
                request = f"{method} {path} HTTP/1.1\r\nHost: localhost\r\nContent-Length: 0\r\nConnection: close\r\n\r\n".encode("ascii")
                expected = b"HTTP/1.1 200 OK\r\nContent-Type: text/plain\r\nContent-Length: " + str(len(body)).encode() + b"\r\nConnection: close\r\n\r\n" + body
                response = bytearray()
                (attempt / (path[1:] + ".request")).write_bytes(request)
                try:
                    with socket.create_connection(("127.0.0.1", int(match[1])), timeout=5) as client:
                        client.settimeout(5)
                        client.sendall(request)
                        response_deadline = time.monotonic() + 15
                        while True:
                            remaining = response_deadline - time.monotonic()
                            if remaining <= 0:
                                raise TimeoutError("native HTTP response deadline exceeded")
                            client.settimeout(min(5, remaining))
                            chunk = client.recv(4096)
                            if not chunk:
                                break
                            response.extend(chunk)
                            if len(response) > 8192:
                                raise RuntimeError("native response exceeds limit")
                finally:
                    (attempt / (path[1:] + ".response")).write_bytes(response)
                if response != expected:
                    raise RuntimeError(f"native {path} response mismatch")
                receipt["native_cases"].append({"path": path, "passed": True,
                    "request_sha256": sha(attempt / (path[1:] + ".request")),
                    "response_sha256": sha(attempt / (path[1:] + ".response"))})
            tail, _ = process.communicate(timeout=10)
            communicated = True
            captured.extend(tail)
            receipt["native_exit_code"] = process.returncode
            if process.returncode != 0 or tail != b"STOPPED\n" or (attempt / "native.stderr").stat().st_size:
                raise RuntimeError("native shutdown/status mismatch")
        finally:
            stop(process)
            if not communicated:
                try:
                    tail, _ = process.communicate(timeout=5)
                    captured.extend(tail)
                except subprocess.TimeoutExpired:
                    receipt["native_output_drain_timeout"] = True
            receipt["native_final_exit_code"] = process.poll()
            (attempt / "native.stdout").write_bytes(captured)
            if trace.exists():
                receipt["trace_sha256"] = sha(trace)
                receipt["trace_syscalls"] = sorted(set(re.findall(r"^\d+\s+(\w+)\(", trace.read_text(), re.M)))


if __name__ == "__main__":
    signal.signal(signal.SIGTERM, interrupted)
    main()
