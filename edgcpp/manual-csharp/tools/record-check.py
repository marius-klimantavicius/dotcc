#!/usr/bin/env python3
"""Run one explicitly selected validation command and retain its exact receipt."""

import argparse
import hashlib
import json
import pathlib
import subprocess
import time


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--receipt", type=pathlib.Path, required=True)
    parser.add_argument("--timeout", type=int, default=300)
    parser.add_argument("--source", type=pathlib.Path, action="append", default=[],
                        help="Additional explicitly checked tool/source path to fingerprint")
    parser.add_argument("command", nargs=argparse.REMAINDER)
    args = parser.parse_args()
    command = args.command[1:] if args.command[:1] == ["--"] else args.command
    if not command:
        parser.error("An explicit validation command is required")
    sources = {}
    for directory in ("src", "tests", "fixtures"):
        for path in sorted((root / directory).rglob("*")):
            if path.is_file() and not {"bin", "obj"}.intersection(path.relative_to(root).parts):
                sources[str(path.relative_to(root))] = hashlib.sha256(path.read_bytes()).hexdigest()
    for path in args.source:
        path = (root / path).resolve()
        sources[str(path.relative_to(root))] = hashlib.sha256(path.read_bytes()).hexdigest()
    started = time.monotonic()
    result = subprocess.run(command, cwd=root, text=True, capture_output=True,
                            timeout=args.timeout, check=False)
    receipt = {"command": command, "cwd": str(root), "exit_code": result.returncode,
               "duration_seconds": round(time.monotonic() - started, 3),
               "stdout": result.stdout, "stderr": result.stderr,
               "source_sha256": sources}
    args.receipt.parent.mkdir(parents=True, exist_ok=True)
    args.receipt.write_text(json.dumps(receipt, indent=2) + "\n")
    print(result.stdout, end="")
    print(result.stderr, end="")
    return result.returncode


if __name__ == "__main__":
    raise SystemExit(main())
