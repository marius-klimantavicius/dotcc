#!/usr/bin/env python3
"""Explicit test-only native EDG invocation; never imported by EdgSharp."""

import argparse
import hashlib
import json
import pathlib
import subprocess


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def matches_expectation(expected, exit_code):
    # This Linux oracle profile follows host_envir.c exit_compilation and
    # host_envir.h RC_NORMAL=0 / RC_ERROR=2. Catastrophes, command-line errors,
    # internal aborts, signals and missing processes are not language passes.
    return exit_code == (0 if expected == "accept" else 2)


def oracle_command(compiler, source, instantiate="used", preprocess=False,
                   include_dirs=(), system_include_dirs=(), definitions=(),
                   undefinitions=(), no_line_commands=False):
    command = [str(compiler), "--clear_flag", "use_predefined_macro_file",
               "--c++20", "--strict", "--no_code_gen", "--instantiate", instantiate]
    if preprocess:
        command.append("--preprocess")
    if no_line_commands:
        command.append("--no_line_commands")
    for option, values in (("--include_directory", include_dirs),
                           ("--sys_include", system_include_dirs),
                           ("--define_macro", definitions),
                           ("--undefine_macro", undefinitions)):
        for value in values:
            command.extend([option, str(value)])
    return command + [str(source)]


def main():
    root = pathlib.Path(__file__).resolve().parents[1]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=pathlib.Path)
    parser.add_argument("--expect", choices=("accept", "reject"), required=True)
    parser.add_argument("--receipt", type=pathlib.Path, required=True)
    parser.add_argument("--compiler", type=pathlib.Path,
                        default=root.parent / "build/bin/cpfe")
    parser.add_argument("--preprocess", action="store_true")
    parser.add_argument("--no-line-commands", action="store_true",
                        help="EDG preprocessing output without line control directives")
    parser.add_argument("--include-dir", type=pathlib.Path, action="append", default=[])
    parser.add_argument("--system-include-dir", type=pathlib.Path, action="append", default=[])
    parser.add_argument("--define", action="append", default=[])
    parser.add_argument("--undefine", action="append", default=[])
    parser.add_argument("--instantiate", choices=("none", "used"), default="used",
                        help="Explicit EDG external template body policy; default used for semantic comparison")
    parser.add_argument("--input", type=pathlib.Path, action="append", default=[],
                        help="Additional explicit input files whose hashes belong in the receipt")
    args = parser.parse_args()
    source = args.source.resolve()
    compiler = args.compiler.resolve()
    command = oracle_command(compiler, source, args.instantiate, args.preprocess,
                             [path.resolve() for path in args.include_dir],
                             [path.resolve() for path in args.system_include_dir],
                             args.define, args.undefine, args.no_line_commands)
    result = subprocess.run(command, cwd=root, text=True, capture_output=True,
                            timeout=30, check=False)
    matched = matches_expectation(args.expect, result.returncode)
    receipt = {
        "purpose": "test oracle only; no production runtime dependency",
        "profile": f"C++20 strict; external template instantiation {args.instantiate}; built-in target witnessed by profile.cpp; predefined macro file disabled",
        "compiler": str(compiler), "compiler_sha256": digest(compiler),
        "source": str(source), "source_sha256": digest(source),
        "additional_inputs": [
            {"path": str(path.resolve()), "sha256": digest(path.resolve())}
            for path in args.input],
        "command": command, "cwd": str(root),
        "exit_code": result.returncode, "stdout": result.stdout,
        "expected_exit_code": 0 if args.expect == "accept" else 2,
        "stderr": result.stderr, "expected": args.expect, "matched": matched,
    }
    args.receipt.parent.mkdir(parents=True, exist_ok=True)
    args.receipt.write_text(json.dumps(receipt, indent=2) + "\n")
    print(f"{'PASS' if matched else 'FAIL'} native {args.expect}: {source.name} (exit {result.returncode})")
    return 0 if matched else 1


if __name__ == "__main__":
    raise SystemExit(main())
