#!/usr/bin/env python3
"""Compatibility entrypoint for shared verification."""
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Scripts"))
from campaigns.cli import main
raise SystemExit(main(["verify", "libsmb2", *sys.argv[1:]]))
