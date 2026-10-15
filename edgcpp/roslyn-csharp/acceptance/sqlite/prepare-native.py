#!/usr/bin/env python3
"""Build the pinned full-feature SQLite as a separate native test oracle only."""
from __future__ import annotations

import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import sys


SOURCE_SHA256 = "2da161e310ef0aba3465f22a4d12b8a43723236f0942fa76e8aac523c3317243"
OBJECT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
DEFINITIONS = [
    "SQLITE_THREADSAFE=1", "SQLITE_ENABLE_FTS4=1", "SQLITE_ENABLE_FTS5=1",
    "SQLITE_ENABLE_RTREE=1", "SQLITE_ENABLE_GEOPOLY=1", "SQLITE_ENABLE_SESSION=1",
    "SQLITE_ENABLE_PREUPDATE_HOOK=1", "SQLITE_ENABLE_RBU=1", "SQLITE_ENABLE_DBSTAT_VTAB=1",
    "SQLITE_ENABLE_DBPAGE_VTAB=1", "SQLITE_ENABLE_STMTVTAB=1", "SQLITE_ENABLE_BYTECODE_VTAB=1",
    "SQLITE_ENABLE_MATH_FUNCTIONS=1", "SQLITE_ENABLE_COLUMN_METADATA=1", "SQLITE_ENABLE_STAT4=1",
    "SQLITE_ENABLE_SNAPSHOT=1", "SQLITE_ENABLE_CARRAY=1", "SQLITE_ENABLE_NORMALIZE=1",
    "SQLITE_ENABLE_UNLOCK_NOTIFY=1",
]


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--attempt", type=Path, required=True, help="New evidence/output directory.")
    parser.add_argument("--input-root", type=Path, default=Path("/mnt/storage/roslynobj-sqlite-20261014"))
    parser.add_argument("--compiler", default="/usr/bin/gcc")
    args = parser.parse_args()
    input_root = args.input_root.resolve()
    source = input_root / "source/sqlite-amalgamation-3540000/sqlite3.c"
    object_path = input_root / "export/sqlite3.roslynobj"
    recipe_path = input_root / "export/recipe.json"
    recipe = json.loads(recipe_path.read_text())
    if digest(source) != SOURCE_SHA256 or digest(object_path) != OBJECT_SHA256:
        raise RuntimeError("Pinned SQLite source/object identity mismatch.")
    flags = ["-D" + value for value in DEFINITIONS]
    if recipe["feature_flags"] != flags or recipe["source_sha256"] != SOURCE_SHA256:
        raise RuntimeError("Recipe differs from the approved full-feature configuration.")
    attempt = args.attempt.resolve()
    attempt.mkdir(parents=True, exist_ok=False)
    library = attempt / "libsqlite3-native-oracle.so"
    command = [args.compiler, "-std=gnu17", *flags, "-fPIC", "-shared", str(source),
               "-o", str(library), "-lm", "-ldl", "-lpthread"]
    measured = Path(__file__).resolve().parents[2] / "tools/run-measured.py"
    report = {
        "status": "incomplete", "role": "separate-native-test-oracle-only",
        "source": str(source), "sourceSha256": SOURCE_SHA256,
        "object": str(object_path), "objectSha256": OBJECT_SHA256,
        "recipeSha256": digest(recipe_path), "definitions": DEFINITIONS,
        "command": command, "applicationNativeFallbackPermitted": False,
        "buildOnlyFlags": ["-fPIC", "-shared"],
        "nativeLibraries": ["libm", "libdl", "libpthread", "libc"],
    }
    try:
        version = subprocess.run([args.compiler, "--version"], check=True, text=True, capture_output=True)
        report["compilerVersion"] = version.stdout
        compiler_path = Path(args.compiler).resolve()
        if compiler_path.is_file():
            report["compilerSha256"] = digest(compiler_path)
        subprocess.run([sys.executable, str(measured), "--output-dir", str(attempt / "build"),
                        "--timeout-seconds", "600", "--memory-bytes", str(4 * 1024**3), "--", *command], check=True)
        report["library"] = {"path": str(library), "sha256": digest(library), "bytes": library.stat().st_size}
        report["status"] = "built-not-executed"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (attempt / "native-oracle-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
