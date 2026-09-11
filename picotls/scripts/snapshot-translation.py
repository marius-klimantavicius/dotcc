#!/usr/bin/env python3
"""Preserve compiler-manifest-owned raw files; never edit emitted C# or delete user files."""
import hashlib
import json
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


if sys.argv[1:] == ["copy"]:
    # The CLI emits this status only after WriteCSharpFiles and the project
    # write succeed. translate.sh truncates the log each invocation. Relying on
    # this explicit status avoids filesystem timestamp resolution assumptions.
    transcript = (ROOT / "artifacts/translation/emit.log").read_text()
    pattern = r"^dotcc: wrote (\d+) C# source file\(s\) \+ " + re.escape(str(PRODUCT / PROJECT)) + r"$"
    status = re.search(pattern, transcript, re.M)
    if not status:
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
elif sys.argv[1:] == ["record"]:
    record = {"format": "picotls-translation-v1", "raw": hashes(RAW),
              "optimized": hashes(PRODUCT),
              "inputs": json.loads((ROOT / "config/inputs.json").read_text()),
              "core_sources": (ROOT / "config/core-sources.txt").read_text().splitlines(),
              "defines": (ROOT / "config/core-defines.txt").read_text().splitlines()}
    record["tool_sha256"] = {
        str(path.relative_to(ROOT.parent)): hashlib.sha256(path.read_bytes()).hexdigest()
        for path in [ROOT.parent / "DotCC/bin/Release/net10.0/dotcc.dll",
                     ROOT.parent / "DotCC/bin/Release/net10.0/DotCC.Lib.dll",
                     ROOT.parent / "DotCC.PostProcess/bin/Release/net10.0/dotcc-postprocess.dll"]}
    (ROOT / "artifacts/translation/success.json").write_text(json.dumps(record, indent=2) + "\n")
    print("Translation and in-place semantic postprocessing completed; behavior remains to be tested.")
else:
    raise SystemExit("Usage: snapshot-translation.py copy|record")
