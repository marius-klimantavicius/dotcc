#!/usr/bin/env python3
"""Reproduce typed value/place/effect sequencing on genuine retained graphs.

Coordinator only. The original aggregate.cpp producer/native disagreement stays
an excluded comparison artifact; its retained schedule is never forced to GCC.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import sys


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--objects", type=Path)
    parser.add_argument("--rejection-object", type=Path)
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--skip-aot", action="store_true")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    fixtures = project / "tests/Fixtures/T2/sequencing"
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    native_dir = artifacts / "native"
    native_dir.mkdir()
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe")
    producer_sha256 = "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4"
    if digest(producer) != producer_sha256:
        raise RuntimeError("Frozen producer identity mismatch.")
    driver = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    measured = project / "tools/run-measured.py"
    runner_project = project / "tests/Projection/Projection.csproj"
    runner = runner_project.parent / "bin/Release/net10.0/Projection.dll"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
    checker = project / "tests/Sequencing/GeneratedChecks.csproj"
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    phases: list[dict] = []

    def run(name: str, command: list[str], seconds: int = 120, expected_exit: int = 0) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(2 * 1024**3), "--", *command], cwd=repository, env=environment, check=False)
        recorded = json.loads((phase / "run.json").read_text())
        phases.append(recorded | {"phase": name, "expectedExitCode": expected_exit})
        if recorded["exitCode"] != expected_exit or recorded.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected execution result; see measured phase receipt.")
        return (phase / "stdout.txt").read_text().strip()

    paths = [p for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
             and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)]
    paths.extend([project / "Directory.Build.props", Path(__file__).resolve(), measured])
    implementation = [{"path": str(p.relative_to(project)), "sha256": digest(p)} for p in sorted(set(paths))]
    report: dict = {"gate": "T2-partial-typed-sequencing", "status": "incomplete", "phases": phases,
                    "nativeApplicationFallback": False, "producerSha256": producer_sha256,
                    "implementation": implementation, "fixtures": [],
                    "limits": ["single-unit sequencing fixtures; bounded multi-unit proof is separate",
                               "nonvolatile nonatomic places; hidden lifetime/EH obligations rejected",
                               "planned va_* combinations remain unsupported",
                               "conditional bitfield lvalues, partial/differing-representation aggregate copies unsupported",
                               "general goto/switch and large-method execution remain open",
                               "original aggregate.cpp discrepancy excluded without schedule rewriting"]}
    try:
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        gcc = Path(shutil.which("gcc") or "gcc").resolve()
        gpp = Path(shutil.which("g++") or "g++").resolve()
        report["nativeCompilers"] = [{"executable": receipt(gcc), "version": run("gcc-version", [str(gcc), "--version"])},
                                     {"executable": receipt(gpp), "version": run("gpp-version", [str(gpp), "--version"])}]
        run("build-projection", [args.dotnet, "build", str(runner_project), "-c", "Release", "--nologo"])
        oracle = native_dir / "oracle.o"
        run("build-native-oracle", [str(gcc), "-std=c17", "-c", str(fixtures / "native-oracle.c"), "-o", str(oracle)])

        def object_path(name: str, source: Path, cpp: bool) -> Path:
            target = args.objects.resolve() / (name + ".roslynobj") if args.objects else artifacts / (name + ".roslynobj")
            if not args.objects:
                run(name + "-export", [sys.executable, str(driver), "--g++" if cpp else "--gcc", "--c++20" if cpp else "--c17", "--gnu_version", "130300", str(source), "-o", str(target)])
            return target

        for name, file, value_count, expected in (
            ("branches", "branches.c", 1, 41), ("places", "places.cpp", 11, 15),
            ("callee", "callee.cpp", 1, 18), ("aggregate-snapshot", "aggregate-snapshot.cpp", 6, 248),
        ):
            source = fixtures / file
            cpp = source.suffix == ".cpp"
            captured = object_path(name, source, cpp)
            native = native_dir / name
            run(name + "-native-build", [str(gpp if cpp else gcc), "-std=c++20" if cpp else "-std=c17", str(source), str(oracle), "-o", str(native)])
            native_output = run(name + "-native-run", [str(native), str(value_count)])
            if not native_output.startswith("result=" + str(expected) + "\n"):
                raise RuntimeError(name + ": independent native fixture result changed.")
            run(name + "-public-operations", [args.dotnet, str(runner), "inspect", str(captured)])
            run(name + "-public-storage", [args.dotnet, str(runner), "inspect-storage", str(captured)])
            generated = artifacts / (name + "-generated")
            repeated = artifacts / (name + "-repeated")
            counts = run(name + "-emit", [args.dotnet, str(runner), "emit", str(captured), str(generated), str(runtime)])
            run(name + "-repeat-emit", [args.dotnet, str(runner), "emit", str(captured), str(repeated), str(runtime)])
            files = sorted(p.name for p in generated.iterdir())
            if files != sorted(p.name for p in repeated.iterdir()) or any(digest(generated / p) != digest(repeated / p) for p in files):
                raise RuntimeError(name + ": emission was not deterministic.")
            if any(p in files for p in ("Generated.NativeImports.json", "Generated.Abi.c")):
                raise RuntimeError("Pure managed sequencing unexpectedly acquired a native workload dependency.")
            output = artifacts / (name + "-checker")
            option = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
            run(name + "-checker-build", [args.dotnet, "build", str(checker), "-c", "Release", option, "-o", str(output), "--nologo"])
            jit = run(name + "-checker-jit", [args.dotnet, str(output / "GeneratedChecks.dll"), name])
            deps = json.loads((output / "GeneratedChecks.deps.json").read_text())["libraries"]
            if any(p.split("/")[0] not in {"GeneratedChecks", "Generated", "RoslynObj.CSharp.Runtime"} for p in deps):
                raise RuntimeError("Sequencing workload acquired reader/SQLite or another unapproved dependency.")
            built = [output / "GeneratedChecks.dll", output / "Generated.dll", output / "RoslynObj.CSharp.Runtime.dll", output / "GeneratedChecks.deps.json", native]
            aot = None
            aot_dependencies = None
            if not args.skip_aot:
                published = artifacts / (name + "-nativeaot")
                run(name + "-checker-publish", [args.dotnet, "publish", str(checker), "-c", "Release", option, "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(published), "--nologo"], 300)
                aot = run(name + "-checker-aot", [str(published / "GeneratedChecks"), name])
                aot_dependencies = run(name + "-aot-dependencies", ["ldd", str(published / "GeneratedChecks")])
                built.append(published / "GeneratedChecks")
            if jit != native_output or aot is not None and aot != native_output:
                raise RuntimeError(name + ": event/value trace differs from the independent native oracle.")
            report["fixtures"].append({"name": name, "source": receipt(source), "input": receipt(captured), "nativeOutput": native_output,
                "jitOutput": jit, "nativeAotOutput": aot, "checkerInvocations": 2, "emissionCounts": counts,
                "generatedDependencies": list(deps), "nativeAotSystemDependencies": aot_dependencies, "deterministic": True,
                "generatedFiles": [receipt(generated / p) for p in files], "builtArtifacts": [receipt(p) for p in built]})

        # Preserve the disagreement as independent evidence, never an accepted
        # fixture whose operations may be adjusted to match a separate compiler.
        original_source = fixtures / "aggregate.cpp"
        original_object = object_path("aggregate", original_source, True)
        original_native = native_dir / "aggregate-excluded"
        run("aggregate-excluded-native-build", [str(gpp), "-std=c++20", str(original_source), str(oracle), "-o", str(original_native)])
        original_output = run("aggregate-excluded-native-run", [str(original_native), "6"])
        run("aggregate-excluded-public-operations", [args.dotnet, str(runner), "inspect", str(original_object)])
        report["excludedDifferential"] = {"source": receipt(original_source), "input": receipt(original_object), "native": receipt(original_native),
            "nativeOutput": original_output, "generatedExecutionAccepted": False,
            "retainedGraphPrediction": 248, "predictionKind": "inference from explicit retained comma/local-copy ordering; see public operation dump",
            "policy": "preserve selected graph; no schedule rewrite to reproduce native600"}
        if not original_output.startswith("result=600\n"):
            raise RuntimeError("Excluded original aggregate native comparison changed; the recorded disagreement needs review.")

        rejection_source = project / "tests/Fixtures/T2/abi-admission/over-aligned-variadic.c"
        rejection_object = args.rejection_object.resolve() if args.rejection_object else artifacts / "over-aligned-variadic.roslynobj"
        if args.rejection_object is None:
            run("over-aligned-export", [sys.executable, str(driver), "--gcc", "--c17", "--gnu_version", "130300", str(rejection_source), "-o", str(rejection_object)])
        facts = json.loads(run("over-aligned-public-facts", [args.dotnet, str(runner), "verify-abi-rejection", str(rejection_object)]))
        rejected_output = artifacts / "must-not-publish"
        rejection = run("over-aligned-rejection", [args.dotnet, str(runner), "emit", str(rejection_object), str(rejected_output), str(runtime)], expected_exit=1)
        if rejected_output.exists() or "CSHARP2100" not in rejection or "natural size and alignment" not in rejection:
            raise RuntimeError("The effective over-aligned prefix was not rejected before publication.")
        report["rejectionProof"] = {"source": receipt(rejection_source), "input": receipt(rejection_object), "publicFacts": facts,
                                    "diagnostics": rejection, "published": False}
        report["inputs"] = [receipt(p) for p in [producer, driver, rejection_source, *sorted(p for p in fixtures.iterdir() if p.is_file())]]
        report["buildArtifacts"] = [receipt(p) for p in [runner, runner.parent / "RoslynObj.CSharp.dll", runner.parent / "RoslynObj.dll",
                                     runtime.parent / "bin/Release/net10.0/RoslynObj.CSharp.Runtime.dll"]]
        current_paths = {str(p.relative_to(project)) for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
                         and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)}
        expected_paths = {p["path"] for p in implementation if Path(p["path"]).suffix in {".cs", ".csproj"}}
        if current_paths != expected_paths or any(digest(project / p["path"]) != p["sha256"] for p in implementation):
            raise RuntimeError("Implementation source set changed during acceptance; build attribution is incomplete.")
        report["status"] = "passed-partial-contracts" if not args.skip_aot else "jit-only-nativeaot-incomplete"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (artifacts / "t2-sequencing-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
