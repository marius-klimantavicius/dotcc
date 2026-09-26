#!/usr/bin/env python3
"""Acquire campaign inputs with the shared optional hash policy."""
import argparse
from pathlib import Path
import sys
ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.compat import references
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--offline", "--no-fetch", action="store_true")
args = parser.parse_args()
for path in references(ROOT, args.offline).values():
    print(path)
