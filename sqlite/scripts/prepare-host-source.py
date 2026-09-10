#!/usr/bin/env python3
"""Prepare the single guarded APPDEF adaptation; never modify reference inputs."""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / "ref/sqlite-amalgamation-3530400"
OUTPUT = ROOT / "generated/sqlite-port"
C_HASH = "b1dd5d74ec7f29055a6684fa06fb3c2f6821c87dd38f9a458dfd2e8a1db28189"
H_HASH = "919e7f2e8ed1d8f56ac17b412b8971c76aa5d1a879752cc6058f75e7d5910e1d"
PATCHED_HASH = "497cc4d6de14a548553a1508c57a8b88bcf02dfe28735e1b7771d281212b77e1"
BEFORE = b"#if SQLITE_THREADSAFE && !defined(SQLITE_MUTEX_NOOP)"
AFTER = BEFORE + b" && !defined(SQLITE_MUTEX_APPDEF)"


def prepare(source=SOURCE, output=OUTPUT):
    content = (source / "sqlite3.c").read_bytes()
    header = (source / "sqlite3.h").read_bytes()
    if hashlib.sha256(content).hexdigest() != C_HASH:
        raise ValueError("Reference sqlite3.c checksum mismatch; review the port adaptation")
    if hashlib.sha256(header).hexdigest() != H_HASH:
        raise ValueError("Reference sqlite3.h checksum mismatch")
    if content.count(BEFORE) != 1:
        raise ValueError("Expected exactly one SQLite mutex selection guard")
    adapted = content.replace(BEFORE, AFTER)
    if hashlib.sha256(adapted).hexdigest() != PATCHED_HASH:
        raise ValueError("Adapted sqlite3.c checksum mismatch")
    output.mkdir(parents=True, exist_ok=True)
    (output / "sqlite3.c").write_bytes(adapted)
    (output / "sqlite3.h").write_bytes(header)
    (output / "adaptation.json").write_text(json.dumps({
        "version": "3.53.4", "reference_sha256": C_HASH,
        "adapted_sha256": PATCHED_HASH, "header_sha256": H_HASH,
        "before": BEFORE.decode(), "after": AFTER.decode(), "replacements": 1,
        "purpose": "Allow application mutex methods and preserve SQLite memory barriers under OS_OTHER",
    }, indent=2) + "\n")
    return output


if __name__ == "__main__":
    print(prepare())
