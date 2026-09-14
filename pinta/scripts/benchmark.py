#!/usr/bin/env python3
"""Measure identical receipt inputs with native and owning managed engines, serially.

Exploratory workload timings including explicit collection/compaction, not a performance gate.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import platform
import subprocess
import sys
import time

ROOT = Path(__file__).resolve().parents[1]


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--iterations", type=int, default=1000)
    parser.add_argument("--mode", choices=["all", "jit", "aot"], default="all")
    parser.add_argument("--profile", choices=["release", "debug"], default="release")
    args = parser.parse_args()
    if not 1 <= args.iterations <= 10000:
        parser.error("iterations must be between 1 and 10000")
    if sys.platform != "linux" or platform.machine().lower() not in ("x86_64", "amd64"):
        parser.error("Native benchmark currently requires Linux x64")
    subprocess.run([sys.executable, ROOT / "scripts/dependency-audit.py", "--profile", args.profile], check=True)
    variant = Path() if args.profile == "release" else Path("debug")
    artifacts = ROOT / "artifacts/benchmark" / variant
    artifacts.mkdir(parents=True, exist_ok=True)
    output = ROOT / "build/benchmark" / variant
    output.mkdir(parents=True, exist_ok=True)
    fixture = ROOT / "tests/fixtures/receipt.pint"
    source = ROOT / "generated/native-input"
    core = [source / name for name in (ROOT / "config/core-sources.txt").read_text().splitlines()]
    inputs = core + list((ROOT / "src/ManagedApi").glob("*.cs")) + list((ROOT / "tests/Benchmark").glob("*.*"))
    inputs += [Path(__file__).resolve(), ROOT / "tests/native-benchmark.c", ROOT / "src/native-adapter.c", fixture]
    inputs += [ROOT / "config/source-lock.json", ROOT / "config/patches.json", ROOT / "config/native-preinclude.h"]
    snapshot = {str(path.relative_to(ROOT)): sha(path) for path in sorted(inputs)}
    receipt = {"platform": platform.platform(), "profile": args.profile, "iterations": args.iterations,
               "input_sha256": snapshot, "runs": [], "passed": False,
               "limitations": ["Single fixed workload; timings are exploratory and machine dependent.",
                   "Native shares one immutable module buffer; managed facade clones its module dictionary per engine.",
                   "Native memory/host allocator costs and CLR allocations are reported separately.",
                   "Five warmups precede measurement; JIT tiered compilation is disabled.",
                   "One explicit Pinta collection with compaction follows each execution; this is a small live heap, not a GC stress benchmark.",
                   "Native output copy is freed during dispose; managed output arrays are reclaimed by CLR GC.",
                   "Peak working set covers the entire process including runtime startup and warmups."]}
    env = dict(os.environ, DOTNET_TieredCompilation="0")

    def run(label, command, execution=False):
        command = list(map(str, command))
        print(label, flush=True)
        log = artifacts / (label + ".log")
        start = time.monotonic()
        with log.open("w") as stream:
            result = subprocess.run(command, stdout=stream, stderr=subprocess.STDOUT,
                                    cwd=ROOT.parent, env=env, timeout=900 if not execution else 120)
        item = {"label": label, "command": command, "exit": result.returncode,
                "wall_seconds": time.monotonic() - start, "log_sha256": sha(log)}
        receipt["runs"].append(item)
        if result.returncode:
            raise RuntimeError(f"{label} failed: {log}\n{log.read_text()[-4000:]}")
        if execution:
            observations = {}
            for line in log.read_text().splitlines():
                key, value = line.split("=", 1)
                observations[key] = float(value) if key.endswith("_seconds") else int(value)
            if observations.get("validated") != 1 or observations.get("iterations") != args.iterations:
                raise RuntimeError(f"Invalid benchmark observations: {log}")
            if observations.get("output_bytes") != 52:
                raise RuntimeError("Receipt output length mismatch")
            item["observations"] = observations
        return item

    try:
        native = output / "native-benchmark"
        run("native-build", [os.environ.get("CC", "cc"), "-std=c11", "-D_POSIX_C_SOURCE=200809L",
            "-fshort-wchar", "-funsigned-char", "-fno-strict-aliasing",
            "-O2" if args.profile == "release" else "-O0",
            "-DPINTA_DEBUG=" + ("0" if args.profile == "release" else "1"),
            "-include", ROOT / "config/native-preinclude.h", "-I", source / "Marius.Pinta/inc",
            "-I", source / "Marius.Pinta/tests", *core, ROOT / "src/native-adapter.c",
            ROOT / "tests/native-benchmark.c", "-o", native])
        native_run = run("native-run", [native, fixture, args.iterations], execution=True)
        native_run["binary"] = {"sha256": sha(native), "bytes": native.stat().st_size}
        modes = ["jit", "aot"] if args.mode == "all" else [args.mode]
        for form in ["raw", "optimized"]:
            library = ROOT / "generated" / variant / ("TranslatedPintaRaw" if form == "raw" else "TranslatedPinta") / "TranslatedPinta.csproj"
            for mode in modes:
                directory = output / form / mode
                command = ["dotnet", "build" if mode == "jit" else "publish", ROOT / "tests/Benchmark/Benchmark.csproj",
                    "-c", "Release", "--nologo", "--artifacts-path", directory / "intermediate",
                    "-o", directory / "app", f"-p:PintaProject={library}"]
                if mode == "aot":
                    command += ["-r", "linux-x64", "-p:PublishAot=true"]
                run(f"{form}-{mode}-build", command)
                binary = directory / "app" / ("Benchmark.dll" if mode == "jit" else "Benchmark")
                invocation = ["dotnet", binary] if mode == "jit" else [binary]
                item = run(f"{form}-{mode}-run", [*invocation, fixture, args.iterations], execution=True)
                item["binary"] = {"sha256": sha(binary), "bytes": binary.stat().st_size}
        if snapshot != {name: sha(ROOT / name) for name in snapshot}:
            raise RuntimeError("Benchmark inputs changed during measurement; rerun")
        receipt["passed"] = True
    finally:
        (artifacts / "receipt.json").write_text(json.dumps(receipt, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
