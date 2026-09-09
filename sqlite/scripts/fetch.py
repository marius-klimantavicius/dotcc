#!/usr/bin/env python3
"""Fetch fixed upstream inputs and verify their archives on every invocation."""
import hashlib
from pathlib import Path
import urllib.request
import zipfile

ROOT = Path(__file__).resolve().parents[1]
INPUTS = (
    ("sqlite-amalgamation-3500400", ROOT / "ref",
     "1d3049dd0f830a025a53105fc79fd2ab9431aea99e137809d064d8ee8356b032"),
    ("sqlite-src-3500400", ROOT / "ref" / "upstream-tests",
     "b7b4dc060f36053902fb65b344bbbed592e64b2291a26ac06fe77eec097850e9"),
)
for name, directory, checksum in INPUTS:
    archive = directory / (name + ".zip")
    directory.mkdir(parents=True, exist_ok=True)
    if not archive.exists():
        with urllib.request.urlopen("https://www.sqlite.org/2025/" + name + ".zip") as response:
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
