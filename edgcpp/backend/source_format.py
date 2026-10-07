"""Whitespace-only Roslyn finishing, isolated from generated runtime dependencies."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import time


def formatter_tool(root):
    project = root / "backend/formatter/Edg.SourceFormatter.csproj"
    inputs = [project, *sorted(project.parent.glob("*.cs"))]
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
    return tool, {"sdk": sdk, "sourceSha256": identity,
                  "toolSha256": hashlib.sha256(tool.read_bytes()).hexdigest()}


def shard_methods(root, source, owner, *, threshold=62000, target=61000):
    tool, provenance = formatter_tool(root)
    request = {"source": source, "owner": owner, "threshold": threshold, "target": target}
    result = subprocess.run(["dotnet", str(tool), "--shard-methods"],
                            input=json.dumps(request), capture_output=True, text=True)
    if result.returncode:
        raise RuntimeError("generated method sharding failed:\n" + result.stderr)
    response = json.loads(result.stdout)
    if isinstance(response.get("source"), list) and all(isinstance(p, str) for p in response["source"]):
        response["source"] = "".join(response["source"])
    if not isinstance(response.get("source"), str) or not isinstance(response.get("metadata"), dict):
        raise RuntimeError("method sharder returned an invalid response")
    return response["source"], dict(response["metadata"], **provenance)


def format_sources(root, sources):
    tool, provenance = formatter_tool(root)
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
    return response["files"], {"mode": "roslyn-whitespace-v2", **provenance}
