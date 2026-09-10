#!/usr/bin/env python3
"""Fetch fixed upstream inputs and verify their archives on every invocation."""
import hashlib
from pathlib import Path
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[1]
INPUTS = (
    ("sqlite-amalgamation-3510300", ROOT / "ref",
     "acb1e6f5d832484bf6d32b681e858c38add8b2acdfd42ac5df24b8afb46552b4"),
    ("sqlite-src-3510300", ROOT / "ref" / "upstream-tests",
     "f8a67a1f5b5cae7c6d42f0994ca7bf1a4a5858868c82adc9fc1340bed5eb8cd2"),
)
for name, directory, checksum in INPUTS:
    archive = directory / (name + ".zip")
    directory.mkdir(parents=True, exist_ok=True)
    if not archive.exists():
        with urllib.request.urlopen("https://www.sqlite.org/2026/" + name + ".zip") as response:
            data = response.read()
        if hashlib.sha256(data).hexdigest() != checksum:
            raise SystemExit("Downloaded SQLite archive checksum mismatch: " + name)
        archive.write_bytes(data)
    if hashlib.sha256(archive.read_bytes()).hexdigest() != checksum:
        raise SystemExit("Cached SQLite archive checksum mismatch: " + name)
    with zipfile.ZipFile(archive) as source:
        for entry in source.infolist():
            target = (directory / entry.filename).resolve()
            if not target.is_relative_to(directory.resolve()):
                raise SystemExit("Unsafe archive path")
        source.extractall(directory)
    print(name + " SHA-256 verified: " + checksum)
