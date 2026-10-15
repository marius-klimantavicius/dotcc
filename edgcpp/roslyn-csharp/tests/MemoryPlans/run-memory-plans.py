#!/usr/bin/env python3
"""Coordinator-only bounded pointer/shift/volatile/planned-vararg receipt."""
from __future__ import annotations

import argparse
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


def record(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--aot-cc", default="clang-18")
    parser.add_argument("--skip-aot", action="store_true", help="Record an explicitly incomplete JIT-only run.")
    parser.add_argument("--objects", type=Path, help="Reuse these immutable genuine objects; this run will not claim to reproduce their export.")
    args = parser.parse_args()
    tests = Path(__file__).resolve().parent.parent
    project = tests.parent
    repository = project.parents[1]
    fixtures = tests / "Fixtures/T3/memory-plans"
    measured = project / "tools/run-measured.py"
    exporter = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe")
    if digest(producer) != "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4":
        raise RuntimeError("The selected frozen genuine producer changed.")
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    objects = args.objects.resolve() if args.objects else artifacts / "export"
    if args.objects is None:
        objects.mkdir()
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    environment.pop("LD_PRELOAD", None)
    source_roots = [project / "src/RoslynObj.CSharp", project / "src/RoslynObj.CSharp.Runtime",
                    repository / "edgcpp/roslyn/managed/RoslynObj", tests / "Projection", tests / "MemoryPlans", fixtures]

    def source_paths() -> set[Path]:
        paths = {path for folder in source_roots for path in folder.rglob("*") if path.is_file()
                 and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".c", ".h", ".md"}
                 and not any(part in {"obj", "bin", "__pycache__"} for part in path.relative_to(folder).parts)}
        paths |= {measured, exporter, repository / "edgcpp/roslyn/schema/kinds.tsv"}
        for folder in source_roots:
            for parent in folder.parents:
                if parent == repository.parent:
                    break
                paths |= {path for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config", "nuget.config")
                          if (path := parent / name).is_file()}
        return paths

    paths = source_paths()
    implementation = {str(path.relative_to(repository)): digest(path) for path in sorted(paths)}
    phases: list[dict] = []
    report: dict = {
        "gate": "T3-partial-pointer-shift-volatile-planned-vararg", "status": "incomplete", "phases": phases,
        "implementation": implementation, "producer": record(producer), "inputs": [],
        "exportsReproduced": args.objects is None, "nativeApplicationFallback": False, "nativeProviders": [],
        "limits": ["Linux x64; ordinary 8/8 data-pointer compound offsets are captured signed32",
                   "pointer scaling uses the immediate selected element, including pointer8 for T**; no source-name type reconstruction",
                   "compound shifts exercise unsigned64/signed32; target runtime rejects counts outside the captured operand width",
                   "recorded unsigned16/unsigned32 volatile primitive fetches and volatile-qualified stores only; natural runtime address alignment required",
                   "address-only qualified pointer/record views are not value loads; no volatile64/aggregate/bitfield/atomic-qualified object execution",
                   "planned calls reuse accepted scalar SysV variadic packs; selected branch prefixes remain branch-local; planned va_* cursor operators remain unsupported",
                   "native application is an independent oracle only; no SQLite or general language/provider completion claim",
                   "direct emitter proof; production CLI admission is not established by this receipt"]}

    def run(name: str, command: list, expected: int = 0, seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(4 * 1024**3), "--", *map(str, command)],
                       cwd=repository, env=environment, check=False)
        result = json.loads((phase / "run.json").read_text())
        phases.append(result | {"phase": name, "expectedExitCode": expected})
        if result["exitCode"] != expected or result.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected result; inspect the preserved measured phase logs.")
        return (phase / "stdout.txt").read_text().strip()

    def tool(name: str) -> Path:
        result = Path(shutil.which(name) or name).resolve()
        if not result.is_file():
            raise RuntimeError("Missing selected tool: " + name)
        return result

    def dependencies(path: Path, checker: bool) -> dict:
        libraries = json.loads(path.read_text())["libraries"]
        expected = {"Generated", "RoslynObj.CSharp.Runtime"} | ({"Checks"} if checker else set())
        if {name.split("/")[0] for name in libraries} != expected:
            raise RuntimeError("Generated execution gained a reader/native-application managed dependency.")
        return {"file": record(path), "libraries": list(libraries)}

    def generated_sources(folder: Path) -> dict:
        files = {path.name: digest(path) for path in folder.iterdir() if path.is_file()}
        if set(files) != {"Generated.cs", "Generated.csproj"}:
            raise RuntimeError("This self-contained memory fixture gained a native gateway or unexpected generated file.")
        source = (folder / "Generated.cs").read_text()
        for forbidden in ("DllImport", "NativeLibrary", "RoslynObject"):
            if forbidden in source:
                raise RuntimeError("Generated execution acquired a native import or reader dependency.")
        for method in ("LoadUInt16", "LoadUInt32", "StoreUInt16", "StoreUInt32"):
            if "TargetVolatileMemory." + method not in source:
                raise RuntimeError("An exercised volatile family did not activate: " + method)
        for marker in ("TargetArithmetic.ShiftLeft", "TargetArithmetic.ShiftRight", "__VariadicCall_"):
            if marker not in source:
                raise RuntimeError("An exercised target shift/planned pack did not activate: " + marker)
        return files

    try:
        dotnet, gcc, aot_cc = tool(args.dotnet), tool("gcc"), tool(args.aot_cc)
        report["toolchain"] = {"dotnet": record(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
            "sdkInfo": run("dotnet-info", [dotnet, "--info"]), "nativeCompiler": record(gcc),
            "nativeVersion": run("gcc-version", [gcc, "--version"]), "aotNativeCompiler": record(aot_cc),
            "aotNativeVersion": run("aot-cc-version", [aot_cc, "--version"])}
        sdk_version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["toolchain"]["managedCompilerFiles"] = [record(sdk / name) for name in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]
        projection = artifacts / "projection"
        run("build-projection", [dotnet, "build", tests / "Projection/Projection.csproj", "-c", "Release", "--nologo", "-o", projection])
        report["build"] = [record(projection / name) for name in ("Projection.dll", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        for name in ("operations", "unsupported-volatile64"):
            source, captured = fixtures / (name + ".c"), objects / (name + ".roslynobj")
            if args.objects is None:
                run(name + "-export", [sys.executable, exporter, "--gcc", "--c17", "--gnu_version", "130300", source, "-o", captured])
            inspection = run(name + "-public-inspection", [dotnet, projection / "Projection.dll", "inspect", captured])
            inspection_path = artifacts / "phases" / (name + "-public-inspection") / "stdout.txt"
            if "FUNCTION fixture state=definition" not in inspection:
                raise RuntimeError("A genuine input has no retained fixture body.")
            if name == "operations":
                for kind in ("eok_padd_assign", "eok_psubtract_assign", "eok_shiftl_assign", "eok_shiftr_assign", "eok_question", "eok_comma"):
                    if kind not in inspection:
                        raise RuntimeError("A retained focused operation did not activate: " + kind)
            report["inputs"].append({"name": name, "source": record(source), "object": record(captured),
                "publicInspection": record(inspection_path), "inspectionIsInventoryOnly": True,
                "sourceExportReproducedInThisRun": args.objects is None})
        native = artifacts / "native-oracle"
        run("native-reference-build", [gcc, "-std=c17", "-O2", fixtures / "operations.c", fixtures / "native-oracle.c", "-o", native])
        native_output = run("native-reference", [native])
        values = [5, 3, 2, 2, 2, 1, 2, 1, 25769803776, 6, 1, 1, 65530, 0x12345678, 17, 23, 1, 24, 17, 19, 4, 1234, 12, 8]
        expected_output = "\n".join(["result=24", "observations=24", *[f"{i}:{value}" for i, value in enumerate(values)],
                                      "events=4", *[f"event{i}:{i + 1}" for i in range(4)]])
        if native_output != expected_output:
            raise RuntimeError("The independent native oracle differs from the defined exact observation protocol.")
        report["nativeReference"] = {"binary": record(native), "stdoutSha256": hashlib.sha256(native_output.encode()).hexdigest(),
            "role": "independent oracle only; never linked into generated execution"}
        runtime_project = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
        generated, repeated = artifacts / "generated", artifacts / "repeated"
        for label, output in (("first", generated), ("repeat", repeated)):
            run("emit-" + label, [dotnet, projection / "Projection.dll", "emit", objects / "operations.roslynobj", output, runtime_project])
        report["generatedSources"] = generated_sources(generated)
        if generated_sources(repeated) != report["generatedSources"]:
            raise RuntimeError("Repeated memory-plan emission is not deterministic.")
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        checker = artifacts / "checker"
        run("generated-build", [dotnet, "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        run("checker-build", [dotnet, "build", tests / "MemoryPlans/Checks.csproj", "-c", "Release", property_arg, "-o", checker, "--nologo"])
        if digest(checker / "Generated.dll") != digest(generated / "bin/Release/net10.0/Generated.dll"):
            raise RuntimeError("Checker did not load the separately built generated target binary.")
        report["jit"] = {"checker": record(checker / "Checks.dll"), "generated": record(checker / "Generated.dll"),
            "runtime": record(checker / "RoslynObj.CSharp.Runtime.dll"), "dependencies": dependencies(checker / "Checks.deps.json", True),
            "generatedDependencies": dependencies(generated / "bin/Release/net10.0/Generated.deps.json", False)}
        for iteration in (1, 2):
            if run("checker-jit-" + str(iteration), [dotnet, checker / "Checks.dll"]) != native_output:
                raise RuntimeError("Generated JIT observations differ from the full native reference.")
        if not args.skip_aot:
            aot = artifacts / "aot"
            run("checker-aot-publish", [dotnet, "publish", tests / "MemoryPlans/Checks.csproj", "-c", "Release", property_arg,
                "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc),
                "-o", aot, "--nologo"], seconds=240)
            for iteration in (1, 2):
                if run("checker-aot-" + str(iteration), [aot / "Checks"]) != native_output:
                    raise RuntimeError("Generated NativeAOT observations differ from the full native reference.")
            linked = run("checker-aot-dependencies", ["ldd", aot / "Checks"])
            if any(word in linked.lower() for word in ("sqlite", "roslynobj", "memory-plan", "oracle")):
                raise RuntimeError("NativeAOT execution gained a native application or reader dependency.")
            report["aot"] = {"binary": record(aot / "Checks"), "dependencies": linked}
        rejected = artifacts / "unsupported-volatile64-generated"
        diagnostic = run("unsupported-volatile64-rejected", [dotnet, projection / "Projection.dll", "emit",
            objects / "unsupported-volatile64.roslynobj", rejected, runtime_project], expected=1)
        if "volatile" not in diagnostic.lower() or rejected.exists():
            raise RuntimeError("The unproved volatile64 variant was not diagnosed before output publication.")
        report["rejection"] = {"name": "unsupported-volatile64", "diagnostic": diagnostic, "outputPublished": False}
        report["status"] = "incomplete-jit-only" if args.skip_aot else "passed-bounded-memory-plan-slice"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(digest(repository / path) != sha for path, sha in implementation.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "Compiled/attributed source set or content changed."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        (artifacts / "memory-plans-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(artifacts / "memory-plans-receipt.json"))
    return 0 if report["status"] == "passed-bounded-memory-plan-slice" else 1


if __name__ == "__main__":
    sys.exit(main())
