#!/usr/bin/env python3
"""Reproduce T1 with genuine exports, separate native oracles, JIT and NativeAOT.

Run through the campaign coordinator: this script serializes all builds/exports.
It refuses to reuse an evidence directory. No C/C++ source is an emitter input.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--objects", type=Path, help="Reuse immutable genuine arithmetic/resolved-template.roslynobj inputs.")
    parser.add_argument("--skip-aot", action="store_true", help="Leaves the NativeAOT acceptance gate incomplete.")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    fixtures = project / "tests/Fixtures/T1"
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe")
    producer_hash = "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4"
    if digest(producer) != producer_hash:
        raise RuntimeError("Frozen producer identity mismatch.")
    driver = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
    runner_project = project / "tests/Projection/Projection.csproj"
    runner = runner_project.parent / "bin/Release/net10.0/Projection.dll"
    measured = project / "tools/run-measured.py"
    phases: list[dict] = []
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)

    def run(name: str, command: list[str], seconds: int = 120, memory: int = 2 * 1024**3) -> str:
        phase = artifacts / "phases" / name
        invocation = [sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds), "--memory-bytes", str(memory), "--", *command]
        subprocess.run(invocation, cwd=repository, env=environment, check=True)
        receipt = json.loads((phase / "run.json").read_text())
        phases.append(receipt | {"phase": name})
        return (phase / "stdout.txt").read_text().strip()

    report: dict = {"gate": "T1", "status": "incomplete", "producerSha256": producer_hash, "phases": phases,
                    "nativeApplicationFallback": False, "generatedRuntimeProvider": "managed-target-operations",
                    "applicationNativeImports": [], "fixtures": []}
    implementation_paths = [path for path in project.rglob("*")
                            if path.is_file() and path.suffix in {".cs", ".csproj"}
                            and not any(part in {"bin", "obj"} for part in path.relative_to(project).parts)]
    implementation_paths.extend([project / "Directory.Build.props", Path(__file__).resolve(), measured])
    report["implementation"] = [{"path": str(path.relative_to(project)), "sha256": digest(path)} for path in sorted(set(implementation_paths))]
    try:
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        run("build-projection", [args.dotnet, "build", str(runner_project), "-c", "Release", "--nologo"])
        harness = artifacts / "native-harness.o"
        run("build-native-harness", ["gcc", "-std=c17", "-c", str(fixtures / "native-harness.c"), "-o", str(harness)])
        for name, source_name, profile, compiler in (
            ("arithmetic", "arithmetic.c", ["--gcc", "--c17", "--gnu_version", "130300"], "gcc"),
            ("resolved-template", "resolved-template.cpp", ["--g++", "--c++20", "--gnu_version", "130300"], "g++"),
        ):
            source = fixtures / source_name
            object_path = args.objects.resolve() / (name + ".roslynobj") if args.objects else artifacts / (name + ".roslynobj")
            if not args.objects:
                run(name + "-export", [sys.executable, str(driver), *profile, str(source), "-o", str(object_path)])
            native = artifacts / (name + "-native")
            run(name + "-native-build", [compiler, "-std=c17" if compiler == "gcc" else "-std=c++20", str(source), str(harness), "-o", str(native)])
            native_output = run(name + "-native-run", [str(native)])
            model_text = run(name + "-public-model", [args.dotnet, str(runner), "verify", str(object_path), name])
            model = json.loads(model_text)
            generated = artifacts / (name + "-generated")
            run(name + "-emit", [args.dotnet, str(runner), "emit", str(object_path), str(generated), str(runtime)])
            repeated = artifacts / (name + "-repeated")
            run(name + "-repeat-emit", [args.dotnet, str(runner), "emit", str(object_path), str(repeated), str(runtime)])
            files = sorted(p.name for p in generated.iterdir())
            if any(digest(generated / file) != digest(repeated / file) for file in files):
                raise RuntimeError("Repeated emission was not deterministic.")
            run(name + "-jit-build", [args.dotnet, "build", str(generated / "Generated.csproj"), "-c", "Release", "--nologo"])
            app = generated / "bin/Release/net10.0/Generated.dll"
            jit_output = run(name + "-jit-run", [args.dotnet, str(app)])
            deps = json.loads((app.parent / "Generated.deps.json").read_text())["libraries"]
            if any(library.split("/")[0] not in {"Generated", "RoslynObj.CSharp.Runtime"} for library in deps):
                raise RuntimeError("Generated application acquired an unapproved runtime dependency.")
            aot_output = None
            native_runtime_dependencies = None
            if not args.skip_aot:
                published = generated / "nativeaot"
                run(name + "-aot-publish", [args.dotnet, "publish", str(generated / "Generated.csproj"), "-c", "Release", "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(published), "--nologo"], seconds=300)
                aot_output = run(name + "-aot-run", [str(published / "Generated")])
                native_runtime_dependencies = run(name + "-aot-system-dependencies", ["ldd", str(published / "Generated")])
            if jit_output != native_output or (aot_output is not None and aot_output != native_output):
                raise RuntimeError(f"{name} differential execution failed.")
            built_applications = [app, app.parent / "RoslynObj.CSharp.Runtime.dll", app.parent / "Generated.deps.json"]
            if not args.skip_aot:
                built_applications.append(generated / "nativeaot/Generated")
            report["fixtures"].append({"name": name, "sourceSha256": digest(source), "objectPath": str(object_path), "objectSha256": digest(object_path),
                "objectBytes": object_path.stat().st_size, "publicModel": model, "nativeOutput": native_output,
                "jitOutput": jit_output, "nativeAotOutput": aot_output, "generatedDependencies": list(deps),
                "nativeAotSystemDependencies": native_runtime_dependencies,
                "builtArtifacts": [{"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size} for path in built_applications],
                "deterministic": True, "generatedFiles": [{"path": str(generated / file), "sha256": digest(generated / file), "bytes": (generated / file).stat().st_size} for file in files]})
        report["inputs"] = [{"path": str(path), "sha256": digest(path)} for path in [producer, driver, fixtures / "arithmetic.c", fixtures / "resolved-template.cpp", fixtures / "native-harness.c"]]
        build_artifacts = [runner, runner.parent / "RoslynObj.CSharp.dll", runner.parent / "RoslynObj.dll",
                           runtime.parent / "bin/Release/net10.0/RoslynObj.CSharp.Runtime.dll"]
        report["buildArtifacts"] = [{"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size} for path in build_artifacts]
        if any(digest(project / source["path"]) != source["sha256"] for source in report["implementation"]):
            raise RuntimeError("Implementation sources changed during acceptance; the build attribution is incomplete.")
        report["status"] = "passed" if not args.skip_aot else "jit-only-nativeaot-incomplete"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (artifacts / "t1-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
