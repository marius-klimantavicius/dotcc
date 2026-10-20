#!/usr/bin/env python3
"""Explicit test-only native EDG invocation; never imported by EdgSharp."""

import argparse
import hashlib
import json
import pathlib
import subprocess


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=pathlib.Path)
    parser.add_argument("--expect", choices=("accept", "reject"), required=True)
    parser.add_argument("--receipt", type=pathlib.Path, required=True)
    parser.add_argument("--compiler", type=pathlib.Path,
                        default=root.parent / "build/bin/cpfe")
    parser.add_argument("--preprocess", action="store_true")
    parser.add_argument("--input", type=pathlib.Path, action="append", default=[],
                        help="Additional explicit input files whose hashes belong in the receipt")
    args = parser.parse_args()
    source = args.source.resolve()
    compiler = args.compiler.resolve()
    command = [str(compiler), "--clear_flag", "use_predefined_macro_file",
               "--c++20", "--strict", "--no_code_gen"]
    if args.preprocess:
        command.append("--preprocess")
    command.append(str(source))
    result = subprocess.run(command, cwd=root, text=True, capture_output=True,
                            timeout=30, check=False)
    accepted = result.returncode == 0
    matched = accepted == (args.expect == "accept")
    receipt = {
        "purpose": "test oracle only; no production runtime dependency",
        "profile": "C++20 strict; built-in target witnessed by profile.cpp; predefined macro file disabled",
        "compiler": str(compiler), "compiler_sha256": digest(compiler),
        "source": str(source), "source_sha256": digest(source),
        "additional_inputs": [
            {"path": str(path.resolve()), "sha256": digest(path.resolve())}
            for path in args.input],
        "command": command, "cwd": str(root),
        "exit_code": result.returncode, "stdout": result.stdout,
        "stderr": result.stderr, "expected": args.expect, "matched": matched,
    }
    args.receipt.parent.mkdir(parents=True, exist_ok=True)
    args.receipt.write_text(json.dumps(receipt, indent=2) + "\n")
    print(f"{'PASS' if matched else 'FAIL'} native {args.expect}: {source.name} (exit {result.returncode})")
    return 0 if matched else 1


if __name__ == "__main__":
    raise SystemExit(main())
