#!/usr/bin/env python3
"""Audit dotcc's own offsetof metadata; emitted storage is checked by TranslatedAbi."""
import argparse
import base64
from pathlib import Path
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("native", type=Path)
parser.add_argument("emitted_probe", type=Path)
args = parser.parse_args()
native = dict(line.split("=", 1) for line in args.native.read_text().splitlines())
source = args.emitted_probe.read_text()
requests = {}
for block in re.findall(r"/\* dotcc-layout-v1\n(.*?)end-dotcc-layout \*/", source, re.S):
    for line in block.splitlines():
        parts = line.split("\t")
        if parts[0] != "request":
            continue
        _, helper, aggregate, member, offset = parts
        aggregate = base64.b64decode(aggregate, validate=True).decode()
        member = base64.b64decode(member, validate=True).decode()
        name = aggregate[3:] if aggregate.startswith("st_ptls_") else "struct " + aggregate
        key = name + "." + member
        constants = re.search(r"internal static class " + re.escape(helper) + r"\s*\{(.*?)\}", source, re.S)
        if not constants:
            raise SystemExit(f"Missing compiler-emitted layout constants for {key}")
        size = re.search(r"\bSize = (\d+)", constants[1])
        alignment = re.search(r"\bAlignment = (\d+)", constants[1])
        if not size or not alignment:
            raise SystemExit(f"Incomplete compiler-emitted layout constants for {key}")
        value = (int(offset), int(size[1]), int(alignment[1]))
        if key in requests and requests[key] != value:
            raise SystemExit(f"Conflicting compiler metadata for {key}")
        requests[key] = value

count = 0
for key, expected in native.items():
    name, member = key.rsplit(".", 1)
    if member in ("size", "align"):
        continue
    count += 1
    target = (int(expected), int(native[name + ".size"]), int(native[name + ".align"]))
    if requests.get(key) != target:
        raise SystemExit(f"Metadata {key}: native offset/size/alignment={target}, dotcc={requests.get(key)}")
if not count:
    raise SystemExit("Native layout inventory was empty")
print(f"PASS {count} compiler-generated offsetof/size/alignment contracts; actual product storage is checked separately")
