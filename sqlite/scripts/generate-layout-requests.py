#!/usr/bin/env python3
"""Extract every active offsetof request from dotcc's full preprocessed SQLite.

The native probe computes values from unchanged C; this file contains no offsets.
Reject unexpected syntax instead of silently dropping an active request.
"""
import argparse
from pathlib import Path
import re

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("preprocessed", type=Path)
parser.add_argument("output", type=Path)
args = parser.parse_args()
source = args.preprocessed.read_text()
requests = set()
for match in re.finditer(r"\boffsetof\s*\(", source):
    end = source.find(")", match.end())
    if end < 0:
        raise SystemExit("Unterminated offsetof request")
    request = source[match.end():end]
    parsed = re.fullmatch(
        r"\s*((?:(?:struct|union)\s+)?[A-Za-z_]\w*)\s*,\s*"
        r"([A-Za-z_]\w*(?:\s*(?:\.\s*[A-Za-z_]\w*|\[\s*\d+\s*\]))*)\s*",
        request)
    if not parsed:
        raise SystemExit("Unhandled active offsetof syntax: " + request)
    aggregate = " ".join(parsed.group(1).split())
    member = re.sub(r"\s+", "", parsed.group(2))
    requests.add((aggregate, member))
if not requests:
    raise SystemExit("No active offsetof requests found")
args.output.parent.mkdir(parents=True, exist_ok=True)
args.output.write_text("/* Generated from all active offsetof requests. */\n" +
                       "".join(f"REQUIRED({aggregate}, {member});\n"
                               for aggregate, member in sorted(requests)))
print(f"Generated {len(requests)} distinct active offsetof layout probes")
