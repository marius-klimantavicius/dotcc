#!/usr/bin/env python3
"""Try actual foreign preprocessed bodies; never publish these diagnostic objects."""
import argparse
import hashlib
import json
import re
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.process import execute


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", required=True, type=Path)
    parser.add_argument("--headers-report", required=True, type=Path)
    parser.add_argument("--output", type=Path, default=ROOT / "artifacts/foreign-dotcc-probes")
    args = parser.parse_args()
    args.compiler = args.compiler.resolve()
    args.output = args.output.resolve()
    args.output.mkdir(parents=True, exist_ok=True)
    report = json.loads(args.headers_report.read_text())
    result = {"scope": "actual target-preprocessed source diagnostic emission; ABI/product/execution not qualified",
              "compiler": str(args.compiler),
              "compiler_library_sha256": hashlib.sha256((args.compiler.parent / "DotCC.Lib.dll").read_bytes()).hexdigest(),
              "headers_report": str(args.headers_report.resolve()), "units": []}
    for target in report["targets"]:
        selected = {"src/version.c", "src/win/core.c", "src/win/fs.c"} if target["backend"] == "windows" else {"src/version.c", "src/unix/core.c", "src/unix/darwin.c"}
        for unit in target["units"]:
            if unit["source"] not in selected:
                continue
            label = target["id"] + "-" + unit["source"].replace("/", "_")
            native = Path(unit["preprocessed"]).read_bytes()
            # Only native preprocessor metadata syntax changes; retain every C token.
            canonical = re.sub(rb'(?m)^#\s+(\d+)\s+("(?:\\.|[^"])*")[^\n]*',
                               lambda match: b'#line ' + match[1] + b' ' + match[2], native)
            input_path = args.output / (label + ".c")
            input_path.write_bytes(canonical)
            command = ["dotnet", str(args.compiler), "-std=c11", str(input_path), "--emit=obj", "-o", str(args.output / (label + ".cs"))]
            record = execute(command, label=label, cwd=args.output, output=args.output / (label + ".log"), timeout=60, check=False)
            result["units"].append({"target": target["id"], "source": unit["source"],
                                    "native_target": target["clang_target"], "native_preprocess_exit": unit["preprocess_exit"],
                                    "native_syntax_exit": unit["syntax_exit"], "source_sha256": unit["sha256"],
                                    "metadata_adaptation": "native cpp linemarkers canonicalized to standard #line; C tokens retained",
                                    "dotcc_default_abi": "LP64; foreign ABI equivalence unproven", **record})
            (args.output / "result.json").write_text(json.dumps(result, indent=2) + "\n")
    passed = sum(row["exit_code"] == 0 for row in result["units"])
    print(f"Actual foreign source object emission: {passed}/{len(result['units'])}; report {args.output / 'result.json'}")
    return 0 if passed == len(result["units"]) else 1


if __name__ == "__main__":
    raise SystemExit(main())
