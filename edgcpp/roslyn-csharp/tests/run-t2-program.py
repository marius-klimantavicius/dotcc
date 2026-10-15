#!/usr/bin/env python3
"""Reproduce the self-contained, bounded two-unit scalar/pointer program slice.

Coordinator only: serialize genuine exports, independent native references,
public ProgramAssembly projection, production CLI, JIT and NativeAOT checks.
No native application body or native provider is linked into generated execution.
"""
from __future__ import annotations

import argparse
import copy
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def file_receipt(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def json_write(path: Path, value: object) -> None:
    path.write_text(json.dumps(value, indent=2) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--objects", type=Path, help="Reuse genuine c/cpp-unit-{a,b}.roslynobj exports.")
    parser.add_argument("--objects-receipt", type=Path, help="Original source/object receipt; defaults to OBJECTS/../input-receipt.json.")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--skip-aot", action="store_true", help="Record an explicitly incomplete JIT-only gate.")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    fixtures = project / "tests/Fixtures/T2/program-assembly"
    reader = repository / "edgcpp/roslyn/managed"
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    native = artifacts / "native"
    native.mkdir()
    objects = args.objects.resolve() if args.objects else artifacts / "export"
    if not args.objects:
        objects.mkdir()
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe")
    producer_hash = "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4"
    exporter = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    measured = project / "tools/run-measured.py"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
    projection_project = project / "tests/ProgramProjection/Projection.csproj"
    projection = projection_project.parent / "bin/Release/net10.0/Projection.dll"
    cli_project = project / "src/RoslynObj.CSharp.Cli/RoslynObj.CSharp.Cli.csproj"
    cli = cli_project.parent / "bin/Release/net10.0/roslynobj-csharp.dll"
    checker_project = project / "tests/ProgramProjection/GeneratedChecks.csproj"
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    environment.pop("LD_PRELOAD", None)

    def source_paths() -> set[Path]:
        return {path for folder in (project, reader) for path in folder.rglob("*")
                if path.is_file() and path.suffix in {".cs", ".csproj", ".props", ".targets"}
                and not any(part in {"bin", "obj"} for part in path.relative_to(folder).parts)}

    compiled_sources = source_paths()
    attribution = compiled_sources | {Path(__file__).resolve(), measured, exporter}
    source_hashes = {str(path.relative_to(repository)): digest(path) for path in sorted(attribution)}
    fixture_paths = sorted(path for path in fixtures.iterdir() if path.is_file())
    fixture_hashes = {path.name: digest(path) for path in fixture_paths}
    phases: list[dict] = []
    report: dict = {
        "gate": "T2-partial-two-unit-program", "status": "incomplete", "phases": phases,
        "nativeApplicationFallback": False, "nativeProviders": [], "maximumOpenUnits": 2,
        "boundedScaleAccepted": False, "sqliteExecutionAccepted": False,
        "implementation": source_hashes, "fixtures": [],
        "limits": ["two small open units", "self-contained fixed scalar/pointer program definitions",
                   "integer callback identity; cross-TU pointer-callback adaptation is not exercised by this receipt",
                   "no native aggregate or variadic multi-unit gateway", "no C++ ODR or full pointee equivalence check",
                   "no dynamic initialization/cleanup schedule or bounded-scale acceptance"],
    }

    def run(name: str, command: list, expected: int = 0, seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(2 * 1024**3), "--", *map(str, command)],
                       cwd=repository, env=environment, check=False)
        receipt = json.loads((phase / "run.json").read_text())
        phases.append(receipt | {"phase": name, "expectedExitCode": expected})
        if receipt["exitCode"] != expected or receipt["budgetViolation"] is not None:
            raise RuntimeError(f"{name}: exit {receipt['exitCode']}, expected {expected}; inspect phase logs.")
        return (phase / "stdout.txt").read_text().strip()

    def assembly_audit(generated: Path, expected: dict | None = None) -> dict:
        audit = json.loads((generated / "Generated.ProgramAssembly.json").read_text())
        if audit["maximumOpenUnits"] != 2 or audit["boundedScaleAccepted"]:
            raise RuntimeError("Program assembly report lost its explicit two-unit bound.")
        declarations = audit["report"]["declarations"]
        if audit["report"]["diagnostics"] or any(row["required"] and (not row["selectedDefinition"] or row["provider"] is not None) for row in declarations):
            raise RuntimeError("A required program declaration was not fulfilled by a selected program definition.")
        functions = audit["functions"]
        storage = audit["storage"]
        allocations = [row["allocationField"] for row in storage]
        addresses = [row["addressField"] for row in functions if row["addressField"] is not None]
        if any(not field for field in allocations) or len(set(allocations)) != len(storage) or len(set(addresses)) != len(addresses):
            raise RuntimeError("Selected allocation or addressed-function identity was duplicated/lost.")
        locators = {row["locator"]: row for row in [*functions, *storage]}
        if len(locators) != len(functions) + len(storage):
            raise RuntimeError("Selected definition locators are not distinct.")
        if expected is not None and audit != expected:
            raise RuntimeError("Direct and production CLI canonical selection/allocation/address audits differ.")
        if any((generated / filename).exists() for filename in ("Generated.NativeImports.json", "Generated.Abi.c")):
            raise RuntimeError("The self-contained program acquired a native application/provider dependency.")
        return audit

    def dependencies(path: Path, checker: bool = False) -> list[str]:
        libraries = json.loads(path.read_text())["libraries"]
        expected = {"Generated", "RoslynObj.CSharp.Runtime"} | ({"GeneratedChecks"} if checker else set())
        if {name.split("/")[0] for name in libraries} != expected:
            raise RuntimeError("Generated execution acquired a reader/SQLite/unapproved managed dependency.")
        return list(libraries)

    def system_dependencies(text: str) -> None:
        allowed = {"linux-vdso.so.1", "libm.so.6", "libc.so.6", "libgcc_s.so.1", "libpthread.so.0", "libdl.so.2",
                   "librt.so.1", "libstdc++.so.6", "/lib64/ld-linux-x86-64.so.2", "/lib/x86_64-linux-gnu/ld-linux-x86-64.so.2"}
        for line in text.splitlines():
            tokens = line.split()
            if tokens and tokens[0] not in allowed and line.strip() != "statically linked":
                raise RuntimeError("NativeAOT program has an unapproved native runtime dependency: " + line)

    def checker(name: str, generated: Path, expected_output: str, fixture: str) -> dict:
        output = artifacts / (name + "-checker")
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        # The checker's global -o also redirects referenced project outputs.
        # Build the selected project explicitly first to retain an independent
        # target artifact for the exact binary-copy identity check below.
        run(name + "-generated-build", [args.dotnet, "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        run(name + "-checker-build", [args.dotnet, "build", checker_project, "-c", "Release", property_arg, "-o", output, "--nologo"])
        if digest(output / "Generated.dll") != digest(generated / "bin/Release/net10.0/Generated.dll"):
            raise RuntimeError(name + ": checker did not receive the selected generated project's binary.")
        jit = run(name + "-checker-jit", [args.dotnet, output / "GeneratedChecks.dll", fixture])
        if jit != expected_output:
            raise RuntimeError(name + ": complete JIT program observations differ from native.")
        result = {"jit": jit, "managedDependencies": dependencies(output / "GeneratedChecks.deps.json", True),
                  "generatedDependencies": dependencies(output / "Generated.deps.json"),
                  "selectedProjectArtifact": file_receipt(generated / "bin/Release/net10.0/Generated.dll"),
                  "buildArtifacts": [file_receipt(output / filename) for filename in
                                     ("GeneratedChecks.dll", "Generated.dll", "Generated.deps.json", "RoslynObj.CSharp.Runtime.dll")]}
        if not args.skip_aot:
            published = artifacts / (name + "-nativeaot")
            run(name + "-aot-publish", [args.dotnet, "publish", checker_project, "-c", "Release", property_arg,
                "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", published, "--nologo"], seconds=300)
            executable = published / "GeneratedChecks"
            aot = run(name + "-aot-run", [executable, fixture])
            native_dependencies = run(name + "-aot-dependencies", ["ldd", executable])
            system_dependencies(native_dependencies)
            if aot != expected_output:
                raise RuntimeError(name + ": complete NativeAOT program observations differ from native.")
            result.update(nativeAot=aot, nativeAotDependencies=native_dependencies)
            result["buildArtifacts"].append(file_receipt(executable))
        return result

    try:
        if digest(producer) != producer_hash:
            raise RuntimeError("Frozen producer identity mismatch.")
        if args.objects:
            original_path = args.objects_receipt.resolve() if args.objects_receipt else objects.parent / "input-receipt.json"
            original = json.loads(original_path.read_text())
            original_files = original["files"]
            for path in [*(path for path in fixture_paths if path.suffix in {".c", ".cpp", ".h"}),
                         *(objects / (name + ".roslynobj") for name in ("c-unit-a", "c-unit-b", "cpp-unit-a", "cpp-unit-b"))]:
                matches = [entry for entry in original_files if Path(entry["path"]).name == path.name]
                if len(matches) != 1 or digest(path) != matches[0]["sha256"]:
                    raise RuntimeError("Reused object/source bytes differ from the original genuine export receipt: " + str(path))
            report["originalInputReceipt"] = file_receipt(original_path)
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        report["nativeCompiler"] = run("gcc-version", ["gcc", "--version"])
        report["nativeCppCompiler"] = run("g++-version", ["g++", "--version"])
        report["nativeTarget"] = run("gcc-target", ["gcc", "-dumpmachine"])
        if report["nativeTarget"] != "x86_64-linux-gnu":
            raise RuntimeError("The native reference target differs from the explicit GNU Linux AMD64 profile.")
        run("build-program-projection", [args.dotnet, "build", projection_project, "-c", "Release", "--nologo"])
        run("build-production-cli", [args.dotnet, "build", cli_project, "-c", "Release", "--nologo"])
        oracle = native / "oracle.o"
        run("build-native-oracle", ["gcc", "-std=c17", "-c", fixtures / "native-oracle.c", "-o", oracle])
        for fixture, suffix, compiler, standard, expected_result, observations in (
            ("c", ".c", "gcc", "c17", 1150, [3, 8, 7, 12, 18, 9, 1, 1, 1, 1]),
            ("cpp", ".cpp", "g++", "c++20", 1212, [4, 16, 7, 10, 6, 1, 1, 16]),
        ):
            pair = [objects / (fixture + "-unit-" + unit + ".roslynobj") for unit in ("a", "b")]
            native_objects: list[Path] = []
            inspected: list[dict] = []
            for unit, object_path in zip(("a", "b"), pair):
                name = fixture + "-unit-" + unit
                source = fixtures / (name + suffix)
                if not args.objects:
                    run(name + "-export", [sys.executable, exporter, "--gcc" if fixture == "c" else "--g++",
                        "--" + standard, "--gnu_version", "130300", source, "-o", object_path])
                native_object = native / (name + ".o")
                run(name + "-native-compile", [compiler, "-std=" + standard, "-c", source, "-o", native_object])
                native_objects.append(native_object)
                inspection = artifacts / (name + "-inspect.json")
                run(name + "-inspect", [args.dotnet, cli, "inspect", object_path, "--report", inspection])
                facts = json.loads(inspection.read_text())
                if facts["target"] != {"abi": "itanium", "addressableUnitBits": 8, "endianness": "little", "pointerBits": "64", "pointerAlignment": "8"}:
                    raise RuntimeError("Actual public target facts differ from the exercised profile.")
                inspected.append(facts)
            native_executable = native / (fixture + "-native")
            run(fixture + "-native-link", [compiler, *native_objects, oracle, "-o", native_executable])
            native_output = run(fixture + "-native-run", [native_executable])
            expected_output = f"result={expected_result}\nobservations={len(observations)}" + "".join(f"\n{index}:{value}" for index, value in enumerate(observations))
            if native_output != expected_output:
                raise RuntimeError(fixture + ": independent native result/observations differ from the fixture contract.")
            direct = artifacts / (fixture + "-direct-generated")
            run(fixture + "-direct-emit", [args.dotnet, projection, "emit", fixture, *pair, direct, runtime])
            direct_checks = json.loads((direct / "ProgramProjectionChecks.json").read_text())
            if not direct_checks["deterministic"] or direct_checks["maximumOpenUnits"] != 2:
                raise RuntimeError("Direct projection lost deterministic bounded selection checks.")
            direct_audit = assembly_audit(direct)
            if direct_audit["report"]["artifactIdentities"] != [facts["input"]["artifactIdentity"] for facts in inspected]:
                raise RuntimeError("Selected program artifact identities differ from the inspected pinned inputs.")
            direct_runtime = checker(fixture + "-direct", direct, native_output, fixture)
            manifest = {
                "manifestVersion": 1, "translatorVersion": "0.1.0", "inputs": [facts["input"] for facts in inspected],
                "target": {"abi": "itanium", "addressableUnitBits": 8, "endianness": "little", "pointerBits": 64,
                    "pointerAlignment": 8, "operatingSystem": "linux", "architecture": "x64", "libc": "glibc", "outputRid": "linux-x64"},
                "runtime": {"version": "0.1.0", "providers": [], "bindings": [], "runtimeProjectPath": str(runtime)},
                "roots": {"mode": "library", "exports": [], "retainAllDefinitions": True},
                "output": {"directory": str(artifacts / (fixture + "-cli-generated")), "projectName": "ProgramFixture",
                    "namespaceName": "Translated", "className": "Program", "namePolicy": "source-stable", "filePolicy": "declaration-size"},
                "link": {"definitionSelection": "strong-only", "deduplicateIdenticalArtifacts": False,
                    "initializationOrder": "manifest-order", "globalState": "process"},
                "budgets": {"maximumWorkingSetBytes": 2 * 1024**3, "maximumOperationsPerBody": 2_000_000,
                    "maximumOutputBytes": 64 * 1024**2, "maximumOpenUnits": 2, "maximumElapsedSeconds": 120,
                    "readerCachedRows": 4096, "readerCachedPages": 128, "readerPageSize": 128},
            }
            rejected = copy.deepcopy(manifest)
            rejected["budgets"]["maximumOpenUnits"] = 1
            rejected["output"]["directory"] = str(artifacts / (fixture + "-bound-rejected-output"))
            rejected_manifest = artifacts / (fixture + "-bound-rejected.json")
            rejected_report = artifacts / (fixture + "-bound-rejected-report.json")
            json_write(rejected_manifest, rejected)
            run(fixture + "-open-bound-rejection", [args.dotnet, cli, "translate", rejected_manifest, "--report", rejected_report], expected=1)
            rejection = json.loads(rejected_report.read_text())
            if Path(rejected["output"]["directory"]).exists() or rejection["translationComplete"] or not any(d["code"] == "ROC1203" for d in rejection["diagnostics"]):
                raise RuntimeError("A unit-count budget violation published a generated project.")
            cli_outputs = []
            completion = None
            cli_audit = None
            completed = None
            fulfilled_imports = []
            for attempt in range(2):
                generated = Path(manifest["output"]["directory"]) if attempt == 0 else artifacts / (fixture + "-cli-repeated")
                request = copy.deepcopy(manifest)
                request["output"]["directory"] = str(generated)
                request_path = artifacts / (fixture + f"-request-{attempt + 1}.json")
                report_path = artifacts / (fixture + f"-cli-report-{attempt + 1}.json")
                metrics_path = artifacts / (fixture + f"-cli-metrics-{attempt + 1}.json")
                json_write(request_path, request)
                run(fixture + f"-cli-translate-{attempt + 1}", [args.dotnet, cli, "translate", request_path, "--report", report_path, "--metrics", metrics_path])
                completed = json.loads(report_path.read_text())
                if not completed["translationComplete"] or completed["diagnostics"] or completed["budgets"]["maximumOpenUnits"] != 2 or completed["runtime"]["providers"] or completed["runtime"]["bindings"]:
                    raise RuntimeError("Production CLI did not retain the exact self-contained program contract.")
                completion = json.loads((generated / "completion.json").read_text())
                expected_inputs = [{"ordinal": index + 1, "sha256": facts["input"]["sha256"], "artifactIdentity": facts["input"]["artifactIdentity"]}
                                   for index, facts in enumerate(inspected)]
                if not completion["complete"] or completion["inputs"] != expected_inputs or any(
                        digest(generated / entry["relativePath"]) != entry["sha256"] or (generated / entry["relativePath"]).stat().st_size != entry["sizeBytes"] for entry in completion["files"]):
                    raise RuntimeError("Production CLI completion file/input hashes differ.")
                cli_audit = assembly_audit(generated, direct_audit)
                declarations = {row["locator"]: row for row in cli_audit["report"]["declarations"]}
                fulfilled_imports = []
                for required in (entry for entry in completed["imports"] if entry["required"]):
                    selected = declarations.get(required["locator"])
                    if (selected is None or not selected["selectedDefinition"] or selected["provider"] is not None
                            or selected["kind"] != required["kind"] or selected["externalName"] is None
                            or selected["externalName"]["name"] != required["linkageName"]):
                        raise RuntimeError("A required inventory import lacks the corresponding exact public program selection.")
                    fulfilled_imports.append({"locator": required["locator"], "selectedDefinition": selected["selectedDefinition"],
                                              "capturedExternalName": selected["externalName"]})
                cli_outputs.append({name: digest(generated / name) for name in ("Generated.cs", "Generated.csproj", "Generated.ProgramAssembly.json")})
            if cli_outputs[0] != cli_outputs[1]:
                raise RuntimeError("Production CLI emission is not deterministic across output locations.")
            cli_generated = Path(manifest["output"]["directory"])
            cli_runtime = checker(fixture + "-cli", cli_generated, native_output, fixture)
            report["fixtures"].append({"name": fixture, "inputs": [file_receipt(path) for path in pair],
                "nativeReference": {"output": native_output, "artifacts": [file_receipt(path) for path in [*native_objects, oracle, native_executable]]},
                "direct": {"checks": direct_checks, "runtime": direct_runtime, "assembly": direct_audit,
                    "files": [file_receipt(path) for path in sorted(direct.iterdir()) if path.is_file()]},
                "productionCli": {"runtime": cli_runtime, "assembly": cli_audit, "completion": completion,
                    "deterministicFileHashes": cli_outputs[0], "requiredInventoryImports": [entry for entry in completed["imports"] if entry["required"]],
                    "inventoryImportFulfillment": fulfilled_imports,
                    "files": [file_receipt(path) for path in sorted(cli_generated.iterdir()) if path.is_file()]},
                "openUnitBudgetRejected": True})
        if (source_paths() != compiled_sources
                or any(digest(repository / path) != checksum for path, checksum in source_hashes.items())
                or any(digest(fixtures / name) != checksum for name, checksum in fixture_hashes.items())):
            raise RuntimeError("Compiled source set/content or fixture bytes changed during the frozen acceptance run.")
        report["inputs"] = [file_receipt(path) for path in [producer, exporter, *fixture_paths]]
        report["tools"] = [file_receipt(Path(shutil.which(name) or name).resolve()) for name in (args.dotnet, "gcc", "g++")]
        report["buildArtifacts"] = [file_receipt(path) for path in [projection, projection.parent / "RoslynObj.CSharp.dll",
            projection.parent / "RoslynObj.dll", cli, cli.parent / "RoslynObj.CSharp.dll", cli.parent / "RoslynObj.dll"]]
        report["status"] = "passed-partial-two-unit-program" if not args.skip_aot else "jit-only-nativeaot-incomplete"
    except Exception as problem:
        report["error"] = str(problem)
        raise
    finally:
        json_write(artifacts / "t2-program-report.json", report)
    return 0


if __name__ == "__main__":
    sys.exit(main())
