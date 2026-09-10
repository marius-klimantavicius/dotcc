#!/usr/bin/env python3
"""Fetch fixed upstream inputs and verify their archives on every invocation."""
import hashlib
from pathlib import Path
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[1]
INPUTS = (
    ("sqlite-amalgamation-3530400", ROOT / "ref",
     "1e71ddf93849c6a6ecf58b827c0692073d2dd7ee40196158068f7b29f422e87d"),
    ("sqlite-src-3530400", ROOT / "ref" / "upstream-tests",
     "d18fa15aec74d8c17e1463f861095adc01b5ad190256acb4f91d22f0368d232b"),
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
