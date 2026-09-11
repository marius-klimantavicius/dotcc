#!/usr/bin/env python3
"""Capture P1 real-source diagnostics without repairing or rewriting upstream C.

Exit 1 means a compiler boundary remains blocked, including diagnostics from a
dotcc invocation that returned zero. All commands and exit statuses are retained.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shlex
import subprocess

root = Path(__file__).resolve().parents[1]
repo = root.parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--build-dotcc", action="store_true")
args = parser.parse_args()
out = root / "artifacts/compiler"
out.mkdir(parents=True, exist_ok=True)
tmp = root / "artifacts/tmp/compiler"
tmp.mkdir(parents=True, exist_ok=True)
env = dict(os.environ, TMPDIR=str(tmp))
compiler = repo / "DotCC/bin/Release/net10.0/dotcc.dll"
if args.build_dotcc:
    with (out / "build.log").open("w") as log:
        subprocess.run(["dotnet", "build", str(repo / "DotCC/DotCC.csproj"),
                        "-c", "Release"], env=env, stdout=log,
                       stderr=subprocess.STDOUT, check=True)
if not compiler.exists():
    parser.error("build dotcc first or pass --build-dotcc")
inputs = json.loads((root / "config/inputs.json").read_text())
source = root / "ref" / inputs["picotls"]["directory"]
defines = ["-D" + line.strip() for line in
           (root / "config/core-defines.txt").read_text().splitlines()
           if line.strip() and not line.lstrip().startswith("#")]
sources = [line.strip() for line in
           (root / "config/core-sources.txt").read_text().splitlines()
           if line.strip() and not line.lstrip().startswith("#")]
base = ["dotnet", str(compiler), "-std=c17", *defines, "-I", str(source / "include")]
results = []

def run(name, command, *, preprocess=False, expected_native=False, build_status=False):
    stdout = out / (name + (".i" if preprocess else ".stdout"))
    stderr = out / (name + ".stderr")
    try:
        with stdout.open("w") as output, stderr.open("w") as errors:
            proc = subprocess.run(command, cwd=root, env=env, stdout=output,
                                  stderr=errors, timeout=120)
        status = proc.returncode
    except subprocess.TimeoutExpired:
        status = 124
    diagnostics = stderr.read_text()
    diagnostic_lines = diagnostics.splitlines()
    if build_status:
        # --emit=build reports its successful write/build on stderr too. Only
        # these known status messages are informational; retain all warnings,
        # errors and unexpected text as blockers even when dotcc exits zero.
        diagnostic_lines = [line for line in diagnostic_lines if not re.fullmatch(
            r"dotcc: wrote [0-9]+ C# source file\(s\) \+ .+\.csproj|dotcc: OK\. dotnet .+\.dll \[args\]", line)]
    pasted_tokens = preprocess and "##" in stdout.read_text()
    blocked = status != 0 or any(line.strip() for line in diagnostic_lines) or pasted_tokens
    results.append(dict(name=name, command=shlex.join(command), exit_code=status,
                        stderr=str(stderr.relative_to(root)),
                        stdout=str(stdout.relative_to(root)),
                        unexpanded_paste=pasted_tokens, blocked=blocked,
                        expected_native=expected_native))
    print(f"{name}: {'BLOCKED' if blocked else 'PASS'} (exit {status})")
    return not blocked

for relative in sources:
    unit = Path(relative).stem
    run(unit + "-preprocess", [*base, "-E", str(source / relative)], preprocess=True)
    target = out / (unit + ".cs")
    target.unlink(missing_ok=True)
    run(unit + "-object", [*base, "--emit=obj", str(source / relative), "-o", str(target)])

for case in sorted((root / "tests/compiler-blockers").glob("*.c")):
    native = out / (case.stem + "-native")
    native.unlink(missing_ok=True)
    if run(case.stem + "-native-build", ["cc", "-std=c17", str(case), "-o", str(native)],
           expected_native=True):
        run(case.stem + "-native-run", [str(native)], expected_native=True)
    run(case.stem + "-preprocess", [*base, "-E", str(case)], preprocess=True)
    target = out / (case.stem + ".cs")
    target.unlink(missing_ok=True)
    run(case.stem + "-object", [*base, "--emit=obj", str(case), "-o", str(target)])
    if case.stem == "posix-memalign":
        run(case.stem + "-build", [*base, "--emit=build", str(case), "-o",
                                    str(out / "posix-memalign-build")], build_status=True)

assemblies = [compiler, compiler.with_name("DotCC.Lib.dll"), compiler.with_name("DotCC.Libc.dll")]
report = dict(compiler_assembly_sha256={path.name: hashlib.sha256(path.read_bytes()).hexdigest()
                                      for path in assemblies if path.exists()},
              repository_commit=subprocess.check_output(
                  ["git", "-C", str(repo), "rev-parse", "HEAD"], text=True).strip(),
              revision=inputs["picotls"]["revision"], results=results)
(out / "results.json").write_text(json.dumps(report, indent=2) + "\n")
raise SystemExit(int(any(item["blocked"] for item in results)))
