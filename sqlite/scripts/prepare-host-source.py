#!/usr/bin/env python3
"""Prepare the single guarded APPDEF adaptation; never modify reference inputs."""
import hashlib
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.identity import HashPolicy
SOURCE = ROOT / "ref" / json.loads((ROOT / "config/sources.json").read_text())["product"]["directory"]
OUTPUT = ROOT / "generated/sqlite-port"
SOURCE_SPEC = json.loads((ROOT / "config/host-source.json").read_text())
C_HASH = SOURCE_SPEC["source_sha256"]["sqlite3.c"]
H_HASH = SOURCE_SPEC["source_sha256"]["sqlite3.h"]
PATCHED_HASH = SOURCE_SPEC["adapted_sha256"]
BEFORE = b"#if SQLITE_THREADSAFE && !defined(SQLITE_MUTEX_NOOP)"
AFTER = BEFORE + b" && !defined(SQLITE_MUTEX_APPDEF)"


def prepare(source=SOURCE, output=OUTPUT, *, hash_mode="warn"):
    content = (source / "sqlite3.c").read_bytes()
    header = (source / "sqlite3.h").read_bytes()
    policy = HashPolicy(hash_mode)
    if hash_mode == "strict":
        try:
            policy.check(source / "sqlite3.c", C_HASH)
            policy.check(source / "sqlite3.h", H_HASH)
        except RuntimeError as error:
            raise ValueError(str(error)) from error
    if content.count(BEFORE) != 1:
        raise ValueError("Expected exactly one SQLite mutex selection guard")
    adapted = content.replace(BEFORE, AFTER)
    if hash_mode == "strict" and hashlib.sha256(adapted).hexdigest() != PATCHED_HASH:
        raise ValueError("Adapted sqlite3.c checksum mismatch")
    output.mkdir(parents=True, exist_ok=True)
    (output / "sqlite3.c").write_bytes(adapted)
    (output / "sqlite3.h").write_bytes(header)
    (output / "adaptation.json").write_text(json.dumps({
        "version": SOURCE_SPEC["version"], "reference_sha256": hashlib.sha256(content).hexdigest(),
        "adapted_sha256": hashlib.sha256(adapted).hexdigest(), "header_sha256": hashlib.sha256(header).hexdigest(),
        "before": BEFORE.decode(), "after": AFTER.decode(), "replacements": 1,
        "purpose": "Allow application mutex methods and preserve SQLite memory barriers under OS_OTHER",
    }, indent=2) + "\n")
    return output


if __name__ == "__main__":
    import os
    print(prepare(hash_mode=os.environ.get("DOTCC_CAMPAIGN_HASHES", "warn")))
