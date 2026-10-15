#!/usr/bin/env python3
"""Reproduce primitive SysV variadics with managed bodies and concrete ABI glue.

Coordinator only. Serializes genuine export/reuse, independent native reference,
ordinary C# emission, JIT/NativeAOT, physical cursors and native observations.
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
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--skip-aot", action="store_true")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    fixtures = project / "tests/Fixtures/T2/variadics"
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
    # Own generated ABI loading must work from the assembly directory. Test-only
    # adapter discovery is enabled solely for the separate observation checkers.
    environment.pop("LD_LIBRARY_PATH", None)
    checker_environment = environment | {"LD_LIBRARY_PATH": str(native_dir)}
    phases: list[dict] = []

    def run(name: str, command: list[str], seconds: int = 120, checker: bool = False) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(2 * 1024**3), "--", *command], cwd=repository,
                       env=checker_environment if checker else environment, check=True)
        phases.append(json.loads((phase / "run.json").read_text()) | {"phase": name, "testAdapterSearchPath": str(native_dir) if checker else None})
        return (phase / "stdout.txt").read_text().strip()

    implementation_paths = [p for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
                            and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)]
    implementation_paths.extend([project / "Directory.Build.props", Path(__file__).resolve(), measured])
    implementation = [{"path": str(p.relative_to(project)), "sha256": digest(p)} for p in sorted(set(implementation_paths))]
    report: dict = {"gate": "T2-partial-primitive-sysv-variadics", "status": "incomplete", "phases": phases,
                    "nativeApplicationFallback": False, "producerSha256": producer_sha256,
                    "implementation": implementation, "fixtures": [],
                    "limits": ["single translation unit; scalar/pointer Cdecl SysV AMD64 ABI only",
                               "already-promoted actual argument representations; no semantic promotion replay",
                               "va_arg prvalue only; lvalue/xvalue cursor slots rejected",
                               "aggregate/vector/extended scalar variadic ABI unsupported",
                               "native C++ exception propagation unsupported",
                               "effectful right-to-left/comma/conditional integration remains a separate gate"]}
    try:
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        gcc = Path(shutil.which("gcc") or "gcc").resolve()
        cc = Path(shutil.which("cc") or "cc").resolve()
        report["nativeCompilers"] = [{"executable": receipt(gcc), "version": run("gcc-version", [str(gcc), "--version"])},
                                     {"executable": receipt(cc), "version": run("cc-version", [str(cc), "--version"])}]
        run("build-projection", [args.dotnet, "build", str(runner_project), "-c", "Release", "--nologo"])
        providers = {
            "addressed": (native_dir / "libvariadic-adapter.so", "native-call-adapters.c",
                          ["native_echo_variadic", "native_call_integer_variadic", "native_call_double_variadic"]),
            "outbound": (native_dir / "liboutbound-adapter.so", "native-outbound-adapters.c",
                         ["native_capture_variadic", "native_outbound_provider", "native_outbound_echo", "native_captured_integer", "native_captured_double"]),
        }
        for name, (library, source, names) in providers.items():
            run(name + "-provider-build", [str(gcc), "-std=c17", "-shared", "-fPIC", str(fixtures / source), "-o", str(library)])
            (artifacts / (name + "-bindings.json")).write_text(json.dumps([
                {"LinkageName": symbol, "LibraryPath": str(library), "EntryPoint": symbol} for symbol in names], indent=2) + "\n")
        for name, application_source, reference_sources, expected in (
            ("variadics", "variadics.c", ["variadics.c", "native-oracle.c"], "1870"),
            ("interleaved", "interleaved.c", ["interleaved.c", "native-interleaved-oracle.c"], "314"),
            ("addressed", "addressed.c", ["addressed.c", "native-call-adapters.c", "native-entry-adapter.c", "native-addressed-oracle.c"], "216"),
            ("outbound", "outbound.c", ["outbound.c", "native-outbound-adapters.c", "native-outbound-oracle.c"], "627"),
        ):
            source = fixtures / application_source
            object_path = args.objects.resolve() / (name + ".roslynobj") if args.objects else artifacts / (name + ".roslynobj")
            if not args.objects:
                run(name + "-export", [sys.executable, str(driver), "--gcc", "--c17", "--gnu_version", "130300", str(source), "-o", str(object_path)])
            native = native_dir / name
            run(name + "-native-build", [str(gcc), "-std=c17", *[str(fixtures / p) for p in reference_sources], "-o", str(native)])
            native_output = run(name + "-native-run", [str(native)])
            matched = re.search(r"(?:^|\n)(?:result=)?(-?\d+)(?:\n|$)", native_output)
            if matched is None or matched.group(1) != expected:
                raise RuntimeError(name + ": independent native fixture contract changed.")
            run(name + "-public-operations", [args.dotnet, str(runner), "inspect", str(object_path)])
            run(name + "-public-storage", [args.dotnet, str(runner), "inspect-storage", str(object_path)])
            generated = artifacts / (name + "-generated")
            repeated = artifacts / (name + "-repeated")
            bindings = [str(artifacts / (name + "-bindings.json"))] if name in providers else []
            counts = run(name + "-emit", [args.dotnet, str(runner), "emit", str(object_path), str(generated), str(runtime), *bindings])
            run(name + "-repeat-emit", [args.dotnet, str(runner), "emit", str(object_path), str(repeated), str(runtime), *bindings])
            files = sorted(p.name for p in generated.iterdir())
            if files != sorted(p.name for p in repeated.iterdir()) or any(digest(generated / p) != digest(repeated / p) for p in files):
                raise RuntimeError(name + ": emission was not deterministic.")
            imports = None
            if name in providers:
                library, _, symbols = providers[name]
                imports = json.loads((generated / "Generated.NativeImports.json").read_text())
                captured = imports["imports"]
                if len(captured) != len(symbols) or {p["linkageName"] for p in captured} != set(symbols) or any(
                        p["library"] != str(library) or p["entryPoint"] != p["linkageName"] or p["callingConvention"] != "cdecl" for p in captured):
                    raise RuntimeError(name + ": native imports exceeded the exact ABI fixture provider.")
                if name == "outbound" and [p["linkageName"] for p in captured if p["isVariadic"]] != ["native_capture_variadic"]:
                    raise RuntimeError("The native variadic binding lost its captured current signature.")
            elif "Generated.NativeImports.json" in files:
                raise RuntimeError("An internal-only variadic fixture unexpectedly acquired a native provider.")
            run(name + "-jit-build", [args.dotnet, "build", str(generated / "Generated.csproj"), "-c", "Release", "-p:RoslynObjAbiCompiler=" + str(cc), "--nologo"])
            app = generated / "bin/Release/net10.0/Generated.dll"
            jit = run(name + "-jit-run", [args.dotnet, str(app)])
            deps = json.loads((app.parent / "Generated.deps.json").read_text())["libraries"]
            if any(p.split("/")[0] not in {"Generated", "RoslynObj.CSharp.Runtime"} for p in deps):
                raise RuntimeError("Generated workload acquired a reader/SQLite or other unapproved managed dependency.")
            built = [app, app.parent / "Generated.deps.json", app.parent / "RoslynObj.CSharp.Runtime.dll", native]
            abi = None
            if imports is not None and imports["generatedAbi"] is not None:
                abi_source = generated / "Generated.Abi.c"
                abi_library = generated / "libRoslynObj.Generated.Abi.so"
                abi_copy = app.parent / abi_library.name
                if digest(abi_library) != digest(abi_copy):
                    raise RuntimeError("Generated ABI library was not copied exactly to the application output.")
                exports = run(name + "-abi-exports", ["nm", "-D", "--defined-only", str(abi_library)])
                actual_exports = {line.split()[-1] for line in exports.splitlines() if len(line.split()) >= 3 and line.split()[-2] in {"T", "W"}}
                abi_contract = imports["generatedAbi"]
                expected_exports = {entry[field] for entry in abi_contract["entries"] for field in ("entry", "registration")}
                expected_exports |= {entry["entry"] for entry in abi_contract["outboundShapes"]}
                if actual_exports != expected_exports:
                    raise RuntimeError("Generated native ABI exports exceeded the concrete forwarding contract.")
                abi = {"source": receipt(abi_source), "binary": receipt(abi_library), "outputCopy": receipt(abi_copy),
                       "exports": exports, "dependencies": run(name + "-abi-dependencies", ["ldd", str(abi_library)]), "contract": abi_contract}
                built.extend([abi_library, abi_copy])
            aot = None
            aot_dependencies = None
            if not args.skip_aot:
                published = generated / "nativeaot"
                run(name + "-aot-publish", [args.dotnet, "publish", str(generated / "Generated.csproj"), "-c", "Release", "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:RoslynObjAbiCompiler=" + str(cc), "-o", str(published), "--nologo"], 300)
                aot = run(name + "-aot-run", [str(published / "Generated")])
                aot_dependencies = run(name + "-aot-dependencies", ["ldd", str(published / "Generated")])
                built.append(published / "Generated")
                if abi is not None:
                    copied = published / "libRoslynObj.Generated.Abi.so"
                    if digest(copied) != abi["binary"]["sha256"]:
                        raise RuntimeError("NativeAOT publication lost or changed the concrete generated ABI provider.")
                    abi["publishedCopy"] = receipt(copied)
                    built.append(copied)
            if jit != expected or aot is not None and aot != expected:
                raise RuntimeError(name + ": generated workload differs from independent native execution.")
            checks: dict = {}
            if name in providers:
                checker_name = "Addressed" if name == "addressed" else "Outbound"
                checker = project / "tests/Variadics" / (checker_name + ".csproj")
                checker_build = artifacts / (name + "-checker")
                options = ["-p:GeneratedProject=" + str(generated / "Generated.csproj"), "-p:RoslynObjAbiCompiler=" + str(cc)]
                run(name + "-checker-build", [args.dotnet, "build", str(checker), "-c", "Release", *options, "-o", str(checker_build), "--nologo"])
                checked_jit = run(name + "-checker-jit", [args.dotnet, str(checker_build / (checker_name + ".dll"))], checker=True)
                checked_aot = None
                built.append(checker_build / (checker_name + ".dll"))
                if not args.skip_aot:
                    checker_aot = artifacts / (name + "-checker-nativeaot")
                    run(name + "-checker-aot-publish", [args.dotnet, "publish", str(checker), "-c", "Release", *options, "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(checker_aot), "--nologo"], 300)
                    checked_aot = run(name + "-checker-aot-run", [str(checker_aot / checker_name)], checker=True)
                    built.append(checker_aot / checker_name)
                required = native_output + ("\nphysical-cursor=passed" if name == "addressed" else "")
                if checked_jit != required or checked_aot is not None and checked_aot != required:
                    raise RuntimeError(name + ": native entry identity/cursor/argument observations changed.")
                checks = {"jit": checked_jit, "nativeAot": checked_aot}
            report["fixtures"].append({"name": name, "input": receipt(object_path), "source": receipt(source), "nativeOutput": native_output,
                "jitOutput": jit, "nativeAotOutput": aot, "checks": checks, "emissionCounts": counts, "nativeImports": imports, "generatedAbi": abi,
                "generatedDependencies": list(deps), "nativeAotSystemDependencies": aot_dependencies, "deterministic": True,
                "generatedFiles": [receipt(generated / p) for p in files], "builtArtifacts": [receipt(p) for p in built]})
        report["nativeProviders"] = [{"binary": receipt(library), "source": receipt(fixtures / source),
                                      "role": "native ABI test invocation/identity or argument observation only"} for library, source, _ in providers.values()]
        report["inputs"] = [receipt(p) for p in [producer, driver, *sorted(p for p in fixtures.iterdir() if p.is_file())]]
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
        (artifacts / "t2-variadics-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
