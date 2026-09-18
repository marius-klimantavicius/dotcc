#!/usr/bin/env python3
"""Preserve compiler-manifest-owned raw files; never edit emitted C# or delete user files."""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
PRODUCT = ROOT / "generated/TranslatedPicotls"
RAW = ROOT / "generated/TranslatedPicotlsRaw"
MANIFEST = "Dotcc.SourceFiles.txt"
PROJECT = "TranslatedPicotls.csproj"


def files(directory, required=True):
    manifest = directory / MANIFEST
    if not manifest.exists() and not required:
        return []
    names = manifest.read_text().splitlines()
    if not names or len(names) != len(set(names)):
        raise SystemExit(f"Missing/duplicate generated files in {manifest}")
    for name in names:
        if not re.fullmatch(r"[A-Za-z_][A-Za-z_0-9.]*\.cs", name) or ".." in name:
            raise SystemExit(f"Unsafe generated filename in {manifest}: {name!r}")
        if (directory / name).is_symlink():
            raise SystemExit(f"Generated source must not be a symlink: {directory / name}")
    return names


def hashes(directory):
    return {name: hashlib.sha256((directory / name).read_bytes()).hexdigest()
            for name in sorted(files(directory) + [MANIFEST, PROJECT])}


def input_state():
    config = ROOT / "config"
    inputs = json.loads((config / "inputs.json").read_text())
    source = ROOT / "ref" / inputs["picotls"]["directory"]
    core_sources = (config / "core-sources.txt").read_text().splitlines()
    host_sources = []
    for line in (config / "host-sources.txt").read_text().splitlines():
        name = line.strip()
        if not name or name.startswith("#"):
            continue
        path = ROOT / name
        path.resolve().relative_to(ROOT)
        host_sources.append({"path": name, "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})
    return {"inputs": inputs,
            "core_wrappers": json.loads((config / "core-wrappers.json").read_text()),
            "core_sources": core_sources,
            "core_source_sha256": {name: hashlib.sha256((source / name).read_bytes()).hexdigest()
                                   for name in core_sources if name.strip() and not name.startswith("#")},
            "upstream_header_sha256": {str(path.relative_to(source)): hashlib.sha256(path.read_bytes()).hexdigest()
                                       for path in sorted((source / "include").rglob("*.h"))},
            "defines": (config / "core-defines.txt").read_text().splitlines(),
            "host_sources": host_sources}


if sys.argv[1:] == ["copy"]:
    # The CLI emits this status only after WriteCSharpFiles and the project
    # write succeed. translate.sh truncates the log each invocation. Relying on
    # this explicit status avoids filesystem timestamp resolution assumptions.
    transcript = (ROOT / "artifacts/translation/emit.log").read_text()
    pattern = r"^dotcc: wrote (\d+) C# source file\(s\) \+ (.+)$"
    status = re.search(pattern, transcript, re.M)
    if not status or Path(status[2]).resolve() != (PRODUCT / PROJECT).resolve():
        raise SystemExit("Translation produced no successful project-write status; refusing to snapshot stale output")
    current = files(PRODUCT)
    if len(current) != int(status[1]):
        raise SystemExit("Compiler status and generated source manifest disagree")
    previous = files(RAW, required=False)
    RAW.mkdir(parents=True, exist_ok=True)
    # Cleanup is limited to the previous compiler manifest, matching dotcc.
    for name in set(previous) - set(current):
        (RAW / name).unlink(missing_ok=True)
    for name in current + [MANIFEST, PROJECT]:
        if (RAW / name).is_symlink():
            raise SystemExit(f"Refusing to overwrite symlink: {RAW / name}")
        shutil.copyfile(PRODUCT / name, RAW / name)
    print(f"Preserved {len(current)} raw generated sources in {RAW}")
elif sys.argv[1:] == ["inputs"]:
    (ROOT / "artifacts/translation/input-state.json").write_text(json.dumps(input_state(), indent=2) + "\n")
elif sys.argv[1:] == ["record"]:
    inputs = input_state()
    if inputs != json.loads((ROOT / "artifacts/translation/input-state.json").read_text()):
        raise SystemExit("Translation inputs changed during emission/postprocessing; rerun translation")
    record = {"format": "picotls-translation-v1", "raw": hashes(RAW),
              "optimized": hashes(PRODUCT), **inputs}
    compiler = Path(os.environ.get("DOTCC_COMPILER", ROOT.parent / "DotCC/bin/Release/net10.0/dotcc.dll")).resolve()
    record["tool_sha256"] = {
        os.path.relpath(path, ROOT.parent): hashlib.sha256(path.read_bytes()).hexdigest()
        for path in [compiler, compiler.parent / "DotCC.Lib.dll",
                     ROOT.parent / "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"]}
    (ROOT / "artifacts/translation/success.json").write_text(json.dumps(record, indent=2) + "\n")
    print("Translation and in-place semantic postprocessing completed; behavior remains to be tested.")
else:
    raise SystemExit("Usage: snapshot-translation.py inputs|copy|record")
