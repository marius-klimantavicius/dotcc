#!/usr/bin/env python3
"""Fetch the fixed upstream amalgamation and verify it on every invocation."""
import hashlib
from pathlib import Path
import urllib.request
import zipfile
ROOT = Path(__file__).resolve().parents[1]
NAME = "sqlite-amalgamation-3500400"
URL = "https://www.sqlite.org/2025/" + NAME + ".zip"
SHA256 = "1d3049dd0f830a025a53105fc79fd2ab9431aea99e137809d064d8ee8356b032"
archive = ROOT / "ref" / (NAME + ".zip")
archive.parent.mkdir(exist_ok=True)
if not archive.exists():
    with urllib.request.urlopen(URL) as response:
        data = response.read()
    if hashlib.sha256(data).hexdigest() != SHA256:
        raise SystemExit("Downloaded SQLite archive checksum mismatch")
    archive.write_bytes(data)
if hashlib.sha256(archive.read_bytes()).hexdigest() != SHA256:
    raise SystemExit("Cached SQLite archive checksum mismatch")
with zipfile.ZipFile(archive) as source:
    for entry in source.infolist():
        target = (archive.parent / entry.filename).resolve()
        if not target.is_relative_to(archive.parent.resolve()):
            raise SystemExit("Unsafe archive path")
    source.extractall(archive.parent)
print(NAME + " SHA-256 verified: " + SHA256)
