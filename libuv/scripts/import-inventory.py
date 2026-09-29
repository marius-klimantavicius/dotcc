#!/usr/bin/env python3
"""Record native archive imports and source lookup candidates, without inventing ABI contracts."""
import argparse
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--archive", type=Path, default=ROOT / "build/native-baseline/libuv/libuv.a")
    parser.add_argument("--output", type=Path, default=ROOT / "config/native-imports.json")
    args = parser.parse_args()
    pin = json.loads((ROOT / "config/sources.json").read_text())["product"]
    source = ROOT / "ref" / pin["directory"]
    undefined = subprocess.check_output(["nm", "-A", "-u", str(args.archive)], text=True)
    definitions = subprocess.check_output(["nm", "-g", "--defined-only", str(args.archive)], text=True)
    defined = {line.split()[-1] for line in definitions.splitlines() if len(line.split()) >= 3}
    names = sorted({line.split()[-1] for line in undefined.splitlines() if line.strip()} - defined)
    if any(name.startswith("uv_") for name in names):
        raise SystemExit("Incomplete native libuv archive: unresolved uv_* symbols")
    files = sorted(path for path in (source / "src").rglob("*") if path.suffix in (".c", ".h"))
    callsites = {name: [] for name in names}
    pattern = re.compile(r"\b(?:" + "|".join(map(re.escape, names)) + r")\b")
    dynamic = []
    for path in files:
        text = path.read_text()
        for number, line in enumerate(text.splitlines(), 1):
            for name in set(pattern.findall(line)):
                callsites[name].append({"path": path.relative_to(source).as_posix(), "line": number, "text": line.strip()})
        for match in re.finditer(r'\b(GetProcAddress|dlsym)\s*\(\s*([^,]+),\s*"([^"]+)"', text):
            dynamic.append({"symbol": match[3], "lookup": match[1], "library_expression": match[2].strip(),
                            "path": path.relative_to(source).as_posix(), "line": text[:match.start()].count("\n") + 1,
                            "status": "candidate; conditional availability/fallback/signature unaudited"})
    rows = []
    for name in names:
        rows.append({"symbol": name, "backend": "linux-glibc-x64",
                     "evidence": "native static archive unresolved symbol after removing archive definitions",
                     "object_files": sorted({line.split(":")[-2] for line in undefined.splitlines() if line.split()[-1] == name}),
                     "callsite_candidates": callsites[name],
                     "kind": "source name observed; ABI audit pending" if callsites[name] else "compiler/libc expansion or external data; inspect native object",
                     "library": None, "signature": None, "calling_convention": None,
                     "required_or_optional": "unresolved", "error_channel": "unresolved", "ownership": "unresolved", "binding": "unimplemented"})
    record = {"schema_version": 1, "source_revision": pin["revision"],
              "scope": "observed Linux glibc native archive externs and direct string-named dynamic lookups across all sources; not complete all-platform import closure",
              "native_archive": str(args.archive.relative_to(ROOT)), "symbols": rows, "dynamic_lookup_candidates": dynamic,
              "unresolved": ["every foreign backend static import set", "macro-generated dynamic lookups including FSEvents",
                             "Winsock extension GUID dispatch", "symbol versions and errno/TLS/data imports",
                             "full signatures, calling conventions, ownership and fallback contracts"]}
    args.output.write_text(json.dumps(record, indent=2) + "\n")
    print(f"Recorded {len(rows)} native externs and {len(dynamic)} dynamic lookup candidates in {args.output}")


if __name__ == "__main__":
    main()
