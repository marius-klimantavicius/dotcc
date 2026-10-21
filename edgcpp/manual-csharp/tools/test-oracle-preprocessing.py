#!/usr/bin/env python3
"""Explicit native-only PP contracts; no production/frontend dependency.

Text comparisons below preserve token boundaries and stringized whitespace.
They witness this pinned EDG profile, not a claim of whole-standard coverage.
"""

import argparse
import json
import pathlib
import subprocess
import sys


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--receipt-prefix", required=True)
    args = parser.parse_args()
    expected_expansion = [
        "prescan 7", "inert F(7)", "alias 8", "tail 9", "expanded_empty",
        "nonempty 1", "pasted token", "empty_paste right left",
        'stringized "a + b"', "adjacent + +", "adjacent_less < <",
        "spliced 10", "comment (x) x(11)", "physical_line 28 + 28",
        "short_circuit yes", "conditional yes"]
    cases = [
        ("m1-expansion-contract", "accept", [], expected_expansion),
        ("m1-include-tail", "accept", ["m1-include-tail.h"], ["F", "(7)"]),
        ("m1-multiline-origin", "accept", [],
         ["direct 6", "wrapped 9", "nested 7", "logical 82"]),
        ("m1-lazy-arguments", "accept", [],
         ["unused 7", 'stringized "BAD(1)"', 'vaopt "" "a"']),
        ("m1-guarded-reentry", "accept", ["m1-guarded-reentry.h"], None),
        ("m1-line-include", "accept", ["m1-guarded-reentry.h"], None),
        ("m1-header-closes-root", "reject", ["m1-header-closes-root.h"], None),
        ("m1-header-opens-root", "reject", ["m1-header-opens-root.h"], None)]
    for name, expected, headers, expected_lines in cases:
        receipt = pathlib.Path(args.receipt_prefix + "-" + name + ".json")
        command = [sys.executable, str(root / "tools/oracle.py"),
                   str(root / "tools/oracle-inputs" / (name + ".cpp")),
                   "--expect", expected, "--receipt", str(receipt)]
        for header in headers:
            command += ["--input", str(root / "tools/oracle-inputs" / header)]
        if expected_lines is not None:
            command += ["--preprocess", "--no-line-commands"]
        result = subprocess.run(command, cwd=root, check=False)
        if result.returncode:
            return result.returncode
        data = json.loads((root / receipt).read_text())
        if expected_lines is not None:
            actual = [line.strip() for line in data["stdout"].splitlines() if line.strip()]
            if actual != expected_lines:
                print(f"FAIL {name} preprocessing text: {actual!r}", file=sys.stderr)
                return 1
    print(f"PASS {len(cases)} native preprocessing contracts, including exact text witnesses")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
