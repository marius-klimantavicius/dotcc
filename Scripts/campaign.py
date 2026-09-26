#!/usr/bin/env python3
"""Shared entrypoint; Bash callers discover Python in campaign-common.sh."""
import sys

if sys.version_info < (3, 11):
    raise SystemExit("Python 3.11 or newer is required")

from campaigns.cli import main

if __name__ == "__main__":
    raise SystemExit(main())
