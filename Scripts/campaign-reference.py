#!/usr/bin/env python3
"""Path-only acquisition interface for specialist/native harnesses."""
import argparse
from pathlib import Path
from campaigns.compat import references
from campaigns.recipes import discover

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("project", choices=discover(Path(__file__).resolve().parent.parent))
parser.add_argument("--no-fetch", "--offline", action="store_true")
args = parser.parse_args()
paths = references(Path(__file__).resolve().parent.parent / args.project, args.no_fetch)
print(next(iter(paths.values())))
