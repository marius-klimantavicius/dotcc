"""Execute generic Zig frontend regressions against native Zig and emitted C#.

Builds only the emitted fixture projects; the compiler must already be built.
These cases verify frontend repairs and do not establish libghostty translation.
"""

from __future__ import annotations

import argparse
import json
import os
from pathlib import Path
import subprocess
import sys


def main() -> int:
    root = Path(__file__).resolve().parents[3]
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--zig", type=Path, required=True)
    parser.add_argument("--output", type=Path,
                        default=root / "libghostty/build/zig-regressions")
    args = parser.parse_args()
    compiler, zig, output = (p.resolve() for p in
                             (args.compiler, args.zig, args.output))
    output.mkdir(parents=True, exist_ok=True)
    steps: list[dict] = []
    passed = 0

    def run(argv: list[str], expected: int = 0) -> subprocess.CompletedProcess:
        result = subprocess.run(argv, cwd=root, text=True, capture_output=True,
                                timeout=180)
        step = {"argv": argv, "expected_exit": expected,
                "exit": result.returncode, "stdout": result.stdout,
                "stderr": result.stderr}
        steps.append(step)
        if result.returncode != expected:
            raise RuntimeError(json.dumps(step, indent=2))
        return result

    try:
        run(["dotnet", "--version"])
        run([str(zig), "version"])
        for case in ("call-trailing-comma", "switch-operand", "import-type-alias"):
            source = Path(__file__).parent / f"{case}.zig"
            binary = output / (case + (".exe" if os.name == "nt" else ""))
            run([str(zig), "build-exe", str(source), f"-femit-bin={binary}"])
            native = run([str(binary)], expected=42)
            managed = output / f"{case}-managed"
            run(["dotnet", str(compiler), str(source), "--emit=csproj",
                 "-o", str(managed)])
            project = managed / f"{managed.name}.csproj"
            run(["dotnet", "build", str(project), "-c", "Release", "-m:1", "-v:q"])
            dll = managed / "bin/Release/net10.0" / f"{managed.name}.dll"
            translated = run(["dotnet", str(dll)], expected=42)
            if (native.stdout, native.stderr) != (translated.stdout, translated.stderr):
                raise RuntimeError(f"{case}: native and managed streams differ")
            passed += 1
            print(f"PASS {case}: native/managed exit 42, identical streams")
    except (OSError, RuntimeError, subprocess.TimeoutExpired) as exc:
        print(str(exc), file=sys.stderr)
        return 1
    finally:
        (output / "receipt.json").write_text(json.dumps(
            {"passed": passed, "required": 3, "steps": steps}, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
