#!/usr/bin/env python3
"""Reproduce bounded semantic assembly from genuine independent translation units.

The campaign coordinator runs this serial build/export driver. It does not emit
or execute translated multi-unit C#, and it establishes no scale acceptance.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys

PRODUCER_SHA256 = "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4"
PRODUCER = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/" + PRODUCER_SHA256 + "-roslyn-cpfe")
C_FIXTURES = ("unit-a", "unit-b", "duplicate-a", "duplicate-b", "category-a", "category-b",
              "unresolved", "weak", "common", "alias", "ifunc", "abi-a", "abi-b")
CPP_FIXTURES = ("names-a", "names-b", "comdat")


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def file_receipt(path: Path) -> dict:
    return {"path": str(path), "bytes": path.stat().st_size, "sha256": digest(path)}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--objects", type=Path, help="Reuse immutable genuine objects instead of exporting fresh inputs.")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--compiler", type=Path, default=PRODUCER, help="Same pinned producer bytes at an alternate location.")
    args = parser.parse_args()
    fixtures = Path(__file__).resolve().parent
    project = fixtures.parents[1]
    repository = project.parents[1]
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    producer = args.compiler.resolve()
    if digest(producer) != PRODUCER_SHA256:
        raise RuntimeError("Frozen producer identity mismatch.")
    exporter = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    measured = project / "tools/run-measured.py"
    runner_project = fixtures / "Assembly.csproj"
    runner = fixtures / "bin/Release/net10.0/Assembly.dll"
    objects = args.objects.resolve() if args.objects else artifacts / "export"
    if not args.objects:
        objects.mkdir()
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    phases: list[dict] = []
    def implementation_paths() -> list[Path]:
        return sorted(path for directory in (project / "src/RoslynObj.CSharp", repository / "edgcpp/roslyn/managed/RoslynObj", fixtures)
                      for path in directory.rglob("*") if path.is_file() and path.suffix in {".cs", ".csproj", ".c", ".cpp", ".py"}
                      and not {"bin", "obj"}.intersection(path.relative_to(directory).parts))
    discovered_sources = implementation_paths()
    source_paths = discovered_sources + [project / "Directory.Build.props", repository / "Directory.Build.props",
                         repository / "Directory.Packages.props", repository / "edgcpp/roslyn/managed/Directory.Build.props",
                         repository / "edgcpp/roslyn/managed/Directory.Packages.props", repository / "edgcpp/roslyn/schema/kinds.tsv",
                         exporter, measured, Path(__file__).resolve()]
    report = {"gate": "T2-program-assembly-partial", "status": "incomplete", "semanticAssemblyOnly": True,
              "generatedMultiUnitExecution": False, "scaleAcceptance": False, "maximumOpenUnits": 2,
              "producer": file_receipt(producer), "reusedGenuineObjects": bool(args.objects), "phases": phases,
              "sources": [file_receipt(path) for path in sorted(set(source_paths))], "inputs": []}

    def run(name: str, command: list[str], seconds: int = 120, memory: int = 2 * 1024**3) -> str:
        phase = artifacts / "phases" / name
        invocation = [sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                      "--memory-bytes", str(memory), "--", *command]
        subprocess.run(invocation, cwd=repository, env=environment, check=True)
        receipt = json.loads((phase / "run.json").read_text())
        phases.append(receipt | {"phase": name})
        return (phase / "stdout.txt").read_text().strip()

    try:
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        for compiler in ("gcc", "g++"):
            executable = Path(shutil.which(compiler) or compiler).resolve()
            report[compiler] = file_receipt(executable) | {"version": run(compiler + "-version", [str(executable), "--version"])}
        for names, extension, flags in ((C_FIXTURES, ".c", ["--gcc", "--c17", "--gnu_version", "130300"]),
                                        (CPP_FIXTURES, ".cpp", ["--g++", "--c++20", "--gnu_version", "130300"])):
            for name in names:
                source = fixtures / (name + extension)
                target = objects / (name + ".roslynobj")
                command = [sys.executable, str(exporter), *flags, str(source), "-o", str(target)]
                if not args.objects:
                    run(name + "-export", command)
                report["inputs"].append({"name": name, "source": file_receipt(source), "object": file_receipt(target),
                                          "exportCommand": command, "exportReexecuted": not bool(args.objects)})
        run("build-assembly-harness", [args.dotnet, "build", str(runner_project), "-c", "Release", "--nologo"], 300, 4 * 1024**3)
        native_c = artifacts / "assembly-native"
        run("native-c-build", ["gcc", "-std=c17", str(fixtures / "unit-a.c"), str(fixtures / "unit-b.c"),
                               str(fixtures / "native-main.c"), "-o", str(native_c)])
        c_output = run("native-c-run", [str(native_c)])
        native_cpp = artifacts / "assembly-cpp-native"
        run("native-cpp-build", ["g++", "-std=c++20", str(fixtures / "names-a.cpp"), str(fixtures / "names-b.cpp"),
                                 str(fixtures / "native-cpp-main.cpp"), "-o", str(native_cpp)])
        cpp_output = run("native-cpp-run", [str(native_cpp)])
        if c_output != "32" or cpp_output != "75 8":
            raise RuntimeError("Independent native cross-unit oracle observations differ from the fixed fixture contracts.")
        assembly_report = artifacts / "program-assembly.json"
        report["harnessOutput"] = run("assembly-harness", [args.dotnet, str(runner), str(objects), str(assembly_report)])
        report["nativeObservations"] = {"c": c_output, "cpp": cpp_output}
        report["programAssemblyReport"] = file_receipt(assembly_report)
        report["builtArtifacts"] = [file_receipt(path) for path in (runner, runner.parent / "RoslynObj.CSharp.dll",
            runner.parent / "RoslynObj.dll", runner.parent / "Assembly.deps.json", native_c, native_cpp)]
        if any(digest(Path(source["path"])) != source["sha256"] for source in report["sources"]):
            raise RuntimeError("Build input sources changed during acceptance; attribution is incomplete.")
        if implementation_paths() != discovered_sources:
            raise RuntimeError("The build input source set changed during acceptance; attribution is incomplete.")
        if any(digest(Path(item["object"]["path"])) != item["object"]["sha256"] for item in report["inputs"]):
            raise RuntimeError("Immutable input bytes changed during acceptance.")
        report["status"] = "passed-semantic-assembly-only"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (artifacts / "assembly-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
