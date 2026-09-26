#!/usr/bin/env python3
"""Compatibility entrypoint: translation.py ACTION [OPTIONS]."""
from pathlib import Path
import sys
sys.path.insert(0, str(Path(__file__).resolve().parents[2] / "Scripts"))
from campaigns.cli import main
if __name__ == "__main__":
    raise SystemExit(main([sys.argv[1] if len(sys.argv) > 1 else "translate", "pinta", *sys.argv[2:]]))
