"""Whitespace-only Roslyn finishing, isolated from generated runtime dependencies."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import time


def format_sources(root, sources):
    project = root / "backend/formatter/Edg.SourceFormatter.csproj"
    inputs = [project, project.with_name("Program.cs")]
    sdk = subprocess.run(["dotnet", "--version"], check=True, capture_output=True, text=True).stdout.strip()
    digest = hashlib.sha256(sdk.encode())
    for path in inputs:
        digest.update(path.name.encode())
        digest.update(path.read_bytes())
    identity = digest.hexdigest()
    cache = root / "artifacts/source-formatter" / identity
    cache.mkdir(parents=True, exist_ok=True)
    stamp = cache / "ready.json"
    lock = cache / "building"
    deadline = time.monotonic() + 180
    while not stamp.exists():
        try:
            lock.mkdir()
        except FileExistsError:
            if time.monotonic() >= deadline:
                raise RuntimeError("timed out waiting for source formatter build: " + str(cache))
            time.sleep(0.1)
            continue
        try:
            if stamp.exists():
                break
            command = ["dotnet", "build", str(project), "-c", "Release", "--nologo", "-v:q",
                       "-o", str(cache / "lib"), "-p:BaseIntermediateOutputPath=" + str(cache / "obj") + os.sep]
            result = subprocess.run(command, capture_output=True, text=True)
            (cache / "build.stdout").write_text(result.stdout)
            (cache / "build.stderr").write_text(result.stderr)
            (cache / "build-command.json").write_text(json.dumps(command, indent=2) + "\n")
            if result.returncode:
                raise RuntimeError("source formatter build failed; see " + str(cache / "build.stdout"))
            stamp.write_text(json.dumps({"sdk": sdk, "sourceSha256": identity}) + "\n")
        finally:
            lock.rmdir()
    tool = cache / "lib/Edg.SourceFormatter.dll"
    result = subprocess.run(["dotnet", str(tool)], input=json.dumps(sources), capture_output=True, text=True)
    if result.returncode:
        raise RuntimeError("source formatting failed:\n" + result.stdout + result.stderr)
    response = json.loads(result.stdout)
    if set(response["files"]) != set(sources) or response["diagnostics"]:
        raise RuntimeError("source formatter returned unexpected files or diagnostics")
    for name, value in response["files"].items():
        if isinstance(value, list) and all(isinstance(chunk, str) for chunk in value):
            response["files"][name] = "".join(value)
        elif not isinstance(value, str):
            raise RuntimeError("source formatter returned an invalid file value: " + name)
    return response["files"], {"mode": "roslyn-whitespace-v2", "sdk": sdk, "sourceSha256": identity,
                               "toolSha256": hashlib.sha256(tool.read_bytes()).hexdigest()}
