#!/usr/bin/env python3
"""Explicit native-only oracle controls; never part of default managed tests.

These controls fail if external template body instantiation silently returns to
EDG's default `none` mode. Each invocation retains a normal oracle receipt.
"""

import argparse
import pathlib
import subprocess
import sys


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--receipt-prefix", required=True)
    parser.add_argument("--compiler", type=pathlib.Path)
    args = parser.parse_args()
    cases = [("m4-selected-false-assert", "reject"),
             ("m4-unused-false-assert", "accept"),
             ("m4-selected-invalid-body", "reject"),
             ("m4-losing-template", "accept")]
    for name, expected in cases:
        command = [sys.executable, str(root / "tools/oracle.py"),
                   str(root / "tools/oracle-inputs" / (name + ".cpp")),
                   "--expect", expected, "--receipt", args.receipt_prefix + "-" + name + ".json"]
        if args.compiler:
            command.extend(["--compiler", str(args.compiler.resolve())])
        result = subprocess.run(command, cwd=root, check=False)
        if result.returncode:
            return result.returncode
    print(f"PASS {len(cases)} explicit native oracle integration controls")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
