#!/usr/bin/env python3
"""Reproduce the partial T2 storage/callback contracts; varargs remain open.

Coordinator only: serializes genuine exports, independent native oracle builds,
ordinary C# projection, deterministic emission, JIT and NativeAOT execution.
Native workload dependencies contain only the named ABI/observation adapters.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def file_receipt(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--objects", type=Path, help="Reuse immutable genuine storage/callbacks/aggregate-copy objects.")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--skip-aot", action="store_true")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    fixtures = project / "tests/Fixtures/T2"
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
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment["LD_LIBRARY_PATH"] = str(native_dir) + (":" + environment["LD_LIBRARY_PATH"] if environment.get("LD_LIBRARY_PATH") else "")
    phases: list[dict] = []

    def run(name: str, command: list[str], seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(2 * 1024**3), "--", *command], cwd=repository, env=environment, check=True)
        phases.append(json.loads((phase / "run.json").read_text()) | {"phase": name})
        return (phase / "stdout.txt").read_text().strip()

    implementation_paths = [p for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
                            and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)]
    implementation_paths.extend([project / "Directory.Build.props", Path(__file__).resolve(), measured])
    implementation = [{"path": str(p.relative_to(project)), "sha256": digest(p)} for p in sorted(set(implementation_paths))]
    fixture_files = sorted(p for folder in (fixtures / "storage", fixtures / "function-addresses") for p in folder.iterdir() if p.is_file())
    report: dict = {"gate": "T2-partial-storage-callbacks", "status": "incomplete", "phases": phases,
                    "nativeApplicationFallback": False, "producerSha256": producer_sha256,
                    "implementation": implementation, "fixtures": [],
                    "limits": ["single translation unit", "nonvolatile nonatomic byte-coordinate bitfields",
                               "scalar/pointer Cdecl native callbacks; aggregate native ABI unaccepted",
                               "variadic calls and native variadic entry remain unaccepted in this receipt",
                               "effectful comma/conditional/right-to-left plans remain unaccepted in this receipt"]}
    try:
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        report["nativeCompiler"] = run("gcc-version", ["gcc", "--version"])
        run("build-projection", [args.dotnet, "build", str(runner_project), "-c", "Release", "--nologo"])
        callbacks_provider = native_dir / "libcallbacks-adapter.so"
        layout_provider = native_dir / "libstorage-layout-adapter.so"
        run("build-callback-provider", ["gcc", "-std=c17", "-shared", "-fPIC", str(fixtures / "function-addresses/native-adapters.c"), "-o", str(callbacks_provider)])
        run("build-layout-observer", ["gcc", "-std=c17", "-shared", "-fPIC", str(fixtures / "storage/native-layout-adapter.c"), "-o", str(layout_provider)])
        binding_names = ["native_reset_events", "native_mark_event", "native_event_count", "native_event_at",
                         "native_echo_callback", "native_invoke_callback", "native_callback_provider"]
        bindings = artifacts / "callbacks-bindings.json"
        bindings.write_text(json.dumps([{"LinkageName": name, "LibraryPath": str(callbacks_provider), "EntryPoint": name} for name in binding_names], indent=2) + "\n")
        for name, relative_source, native_sources, expected in (
            ("storage", "storage/storage.c", ["storage/storage.c", "storage/native-oracle.c"], "147423"),
            ("aggregate-copy", "storage/aggregate-copy.c", ["storage/aggregate-copy.c", "storage/native-copy-oracle.c"], "45"),
            ("callbacks", "function-addresses/callbacks.c", ["function-addresses/callbacks.c", "function-addresses/native-adapters.c", "function-addresses/native-oracle.c"], "751"),
        ):
            source = fixtures / relative_source
            object_path = args.objects.resolve() / (name + ".roslynobj") if args.objects else artifacts / (name + ".roslynobj")
            if not args.objects:
                run(name + "-export", [sys.executable, str(driver), "--gcc", "--c17", "--gnu_version", "130300", str(source), "-o", str(object_path)])
            native = native_dir / name
            run(name + "-native-build", ["gcc", "-std=c17", *[str(fixtures / p) for p in native_sources], "-o", str(native)])
            native_output = run(name + "-native-run", [str(native)])
            match = re.search(r"(?:^|\n)(?:result=)?(-?\d+)(?:\n|$)", native_output)
            if match is None or match.group(1) != expected:
                raise RuntimeError(f"{name}: native fixture contract changed: {native_output}")
            public_model = run(name + "-public-operations", [args.dotnet, str(runner), "inspect", str(object_path)])
            public_storage = run(name + "-public-storage", [args.dotnet, str(runner), "inspect-storage", str(object_path)])
            generated = artifacts / (name + "-generated")
            repeated = artifacts / (name + "-repeated")
            binding_args = [str(bindings)] if name == "callbacks" else []
            emission = run(name + "-emit", [args.dotnet, str(runner), "emit", str(object_path), str(generated), str(runtime), *binding_args])
            run(name + "-repeat-emit", [args.dotnet, str(runner), "emit", str(object_path), str(repeated), str(runtime), *binding_args])
            generated_files = sorted(p.name for p in generated.iterdir())
            if generated_files != sorted(p.name for p in repeated.iterdir()) or any(digest(generated / p) != digest(repeated / p) for p in generated_files):
                raise RuntimeError(f"{name}: emission was not deterministic.")
            import_report = None
            if name == "callbacks":
                import_report = json.loads((generated / "Generated.NativeImports.json").read_text())
                imports = import_report["imports"]
                if len(imports) != len(binding_names) or {p["linkageName"] for p in imports} != set(binding_names) or any(
                        p["library"] != str(callbacks_provider) or p["entryPoint"] != p["linkageName"] or p["callingConvention"] != "cdecl" for p in imports):
                    raise RuntimeError("Callback native imports exceeded the exact ABI adapter bindings.")
            elif "Generated.NativeImports.json" in generated_files:
                raise RuntimeError("Storage application unexpectedly imported native application execution.")
            run(name + "-jit-build", [args.dotnet, "build", str(generated / "Generated.csproj"), "-c", "Release", "--nologo"])
            app = generated / "bin/Release/net10.0/Generated.dll"
            jit = run(name + "-jit-run", [args.dotnet, str(app)])
            deps = json.loads((app.parent / "Generated.deps.json").read_text())["libraries"]
            if any(p.split("/")[0] not in {"Generated", "RoslynObj.CSharp.Runtime"} for p in deps):
                raise RuntimeError("Generated workload acquired a reader/SQLite or other unapproved dependency.")
            aot = None
            aot_dependencies = None
            built = [app, app.parent / "Generated.deps.json", app.parent / "RoslynObj.CSharp.Runtime.dll", native]
            if not args.skip_aot:
                published = generated / "nativeaot"
                run(name + "-aot-publish", [args.dotnet, "publish", str(generated / "Generated.csproj"), "-c", "Release", "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(published), "--nologo"], 300)
                aot = run(name + "-aot-run", [str(published / "Generated")])
                aot_dependencies = run(name + "-aot-dependencies", ["ldd", str(published / "Generated")])
                built.append(published / "Generated")
            if jit != expected or aot is not None and aot != expected:
                raise RuntimeError(f"{name}: generated execution differs from the native oracle.")
            checks: dict = {}
            if name in {"storage", "callbacks"}:
                checker_name = "Storage" if name == "storage" else "Callbacks"
                checker = project / "tests" / checker_name / (checker_name + ".csproj")
                checker_build = artifacts / (name + "-checker")
                property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
                run(name + "-checker-build", [args.dotnet, "build", str(checker), "-c", "Release", property_arg, "-o", str(checker_build), "--nologo"])
                checked_jit = run(name + "-checker-jit", [args.dotnet, str(checker_build / (checker_name + ".dll"))])
                checked_aot = None
                built.append(checker_build / (checker_name + ".dll"))
                if not args.skip_aot:
                    checker_aot = artifacts / (name + "-checker-nativeaot")
                    run(name + "-checker-aot-publish", [args.dotnet, "publish", str(checker), "-c", "Release", property_arg, "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(checker_aot), "--nologo"], 300)
                    checked_aot = run(name + "-checker-aot-run", [str(checker_aot / checker_name)])
                    built.append(checker_aot / checker_name)
                required = native_output if name == "callbacks" else native_output + "\nphysical-layout=passed"
                if checked_jit != required or checked_aot is not None and checked_aot != required:
                    raise RuntimeError(f"{name}: physical layout or callback observations differ.")
                checks = {"jit": checked_jit, "nativeAot": checked_aot}
            report["fixtures"].append({"name": name, "input": file_receipt(object_path), "source": file_receipt(source),
                "nativeOutput": native_output, "jitOutput": jit, "nativeAotOutput": aot, "checks": checks,
                "emissionCounts": emission, "publicOperationDumpSha256": hashlib.sha256((public_model + "\n").encode()).hexdigest(),
                "publicStorageDumpSha256": hashlib.sha256((public_storage + "\n").encode()).hexdigest(),
                "generatedDependencies": list(deps), "nativeAotSystemDependencies": aot_dependencies,
                "nativeImportAudit": import_report,
                "deterministic": True, "generatedFiles": [file_receipt(generated / p) for p in generated_files],
                "builtArtifacts": [file_receipt(p) for p in built]})
        report["nativeProviders"] = [{"binary": file_receipt(callbacks_provider), "source": file_receipt(fixtures / "function-addresses/native-adapters.c"),
                                      "role": "native test observation, identity echo, invocation and identity callback only"},
                                     {"binary": file_receipt(layout_provider), "source": file_receipt(fixtures / "storage/native-layout-adapter.c"),
                                      "role": "physical byte/layout observations only"}]
        report["inputs"] = [file_receipt(p) for p in [producer, driver, *fixture_files]]
        report["buildArtifacts"] = [file_receipt(p) for p in [runner, runner.parent / "RoslynObj.CSharp.dll", runner.parent / "RoslynObj.dll",
                                     runtime.parent / "bin/Release/net10.0/RoslynObj.CSharp.Runtime.dll"]]
        current_paths = {str(p.relative_to(project)) for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
                         and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)}
        expected_paths = {p["path"] for p in implementation if Path(p["path"]).suffix in {".cs", ".csproj"}}
        if current_paths != expected_paths or any(digest(project / p["path"]) != p["sha256"] for p in implementation):
            raise RuntimeError("Implementation sources changed during acceptance; build attribution is incomplete.")
        report["status"] = "passed-partial-contracts" if not args.skip_aot else "jit-only-nativeaot-incomplete"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (artifacts / "t2-storage-callbacks-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
