#!/usr/bin/env python3
"""Coordinator-only typed va_arg and volatile-formal classification receipt."""
from __future__ import annotations

import argparse
from collections import Counter
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
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--objects", type=Path, help="Reuse these immutable genuine objects; this run will not claim to reproduce their export.")
    parser.add_argument("--operator-facts", type=Path, help="Record the existing public SQLite blocker-facts basis without scanning the object again.")
    args = parser.parse_args()
    project = args.project_root.resolve()
    tests = project / "tests"
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
                    repository / "edgcpp/roslyn/managed/RoslynObj", tests / "Projection", tests / "LastFour", tests / "OperatorFacts"]

    def source_paths() -> set[Path]:
        paths = {path for folder in source_roots for path in folder.rglob("*") if path.is_file()
                 and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".c", ".h", ".md"}
                 and not any(part in {"obj", "bin", "__pycache__"} for part in path.relative_to(folder).parts)}
        paths |= {Path(__file__).resolve(), measured, exporter, repository / "edgcpp/roslyn/schema/kinds.tsv"}
        paths |= {fixtures / name for name in ("last-four.c", "native-last-four-oracle.c")}
        if args.operator_facts is not None:
            paths.add(args.operator_facts.resolve())
        for folder in source_roots:
            for parent in folder.parents:
                if parent == repository.parent:
                    break
                paths |= {path for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config", "nuget.config")
                          if (path := parent / name).is_file()}
        return paths

    paths = source_paths()
    implementation = {str(path): digest(path) for path in sorted(paths)}
    phases: list[dict] = []
    input_hashes: dict[Path, str] = {}
    report: dict = {
        "gate": "T3-partial-typed-va-arg-volatile-formal", "status": "incomplete", "phases": phases,
        "implementation": implementation, "producer": record(producer), "inputs": [],
        "exportsReproduced": args.objects is None, "nativeApplicationFallback": False, "nativeProviders": [],
        "limits": ["typed scalar-prvalue va_arg only, using the existing SysV scalar cursor adapter and exact selected tag24/8 layout",
                   "the actual cursor occurrence is captured once; lvalue/address va_arg and planned va_start/copy/end remain unsupported",
                   "signed32/unsigned32/binary64/pointer results exercise the actual remaining SQLite cursor families without adding scalar admission",
                   "selected branch cursor updates remain branch-local; six exact events and one cursor selector call",
                   "top-level volatile binary64 formal uses ordinary generated call selection plus recorded internal volatile fetch",
                   "source classifier traversal does not admit native aggregate/volatile execution or remove the general execution guard",
                   "native application is an independent oracle only; no full SQLite execution or production CLI completion claim"]}

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
        for method in ("LoadInt32", "LoadDouble", "StoreInt32", "StoreDouble"):
            if "TargetVolatileMemory." + method not in source:
                raise RuntimeError("An exercised volatile family did not activate: " + method)
        for marker in ("SysVVariadic.NextInt32", "SysVVariadic.NextUInt32", "SysVVariadic.NextDouble", "SysVVariadic.NextPointer"):
            if marker not in source:
                raise RuntimeError("An exercised selected cursor adapter did not activate: " + marker)
        return files

    try:
        if args.operator_facts is not None:
            fact_path = args.operator_facts.resolve()
            facts = json.loads(fact_path.read_text())
            if facts["InventoryOnly"] is not True or facts["ExecutionOrderAccepted"] is not False or facts["ReachabilityAccepted"] is not False:
                raise RuntimeError("The recorded blocker-facts basis must be public inventory only.")
            report["operatorFactsBasis"] = {"file": record(fact_path), "artifact": facts["Artifact"],
                "selectedBodyCount": len(facts["Functions"]), "representationDiagnosticCount": len(facts["RepresentationDiagnostics"])}
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
        public_probe = artifacts / "public-probe"
        run("build-public-probe", [dotnet, "build", tests / "OperatorFacts/OperatorFacts.csproj", "-c", "Release", "--nologo", "-o", public_probe])
        report["build"].append(record(public_probe / "OperatorFacts.dll"))
        for name in ("last-four",):
            source, captured = fixtures / (name + ".c"), objects / (name + ".roslynobj")
            if args.objects is None:
                run(name + "-export", [sys.executable, exporter, "--gcc", "--c17", "--gnu_version", "130300", source, "-o", captured])
            inspection = run(name + "-public-inspection", [dotnet, projection / "Projection.dll", "inspect", captured])
            inspection_path = artifacts / "phases" / (name + "-public-inspection") / "stdout.txt"
            if "FUNCTION fixture state=definition" not in inspection:
                raise RuntimeError("A genuine input has no retained fixture body.")
            if name == "last-four":
                for kind in ("eok_va_arg", "eok_va_start", "eok_va_end", "eok_comma", "eok_question"):
                    if kind not in inspection:
                        raise RuntimeError("A retained focused operation did not activate: " + kind)
            report["inputs"].append({"name": name, "source": record(source), "object": record(captured),
                "publicInspection": record(inspection_path), "inspectionIsInventoryOnly": True,
                "sourceExportReproducedInThisRun": args.objects is None})
            input_hashes[captured] = digest(captured)
        public_facts_path = artifacts / "fixture-public-facts.json"
        run("fixture-public-facts", [dotnet, public_probe / "OperatorFacts.dll", objects / "last-four.roslynobj", public_facts_path,
                                     "--names", "consume,qualified_formal,ordinary_caller"])
        fixture_facts = json.loads(public_facts_path.read_text())
        if fixture_facts["InventoryOnly"] is not True or fixture_facts["ExecutionOrderAccepted"] is not False:
            raise RuntimeError("Public fixture graph inspection must remain inventory only.")
        functions = {function["CurrentName"]: function for function in fixture_facts["Functions"]}
        formal = functions["qualified_formal"]
        def volatile_type(type_facts: dict) -> bool:
            return any((wrapper["Qualifiers"] or {}).get("IsVolatile") is True for wrapper in type_facts["Wrappers"])
        parameters = formal["SignatureParameters"]
        if len(parameters) != 1 or parameters[0]["Ordinal"] != 0 or parameters[0]["Role"] != "ordinary_parameter" or not volatile_type(parameters[0]["Type"]):
            raise RuntimeError("The selected genuine formal did not retain its top-level volatile qualification.")
        if not any(expression["VolatileFetch"] and expression["Type"]["Arithmetic"]["Kind"] == "floating" and
                   expression["Type"]["Arithmetic"]["Bits"] == 64 for expression in formal["VolatileBodyGraphExpressions"]):
            raise RuntimeError("The selected volatile formal did not retain an actual binary64 fetch.")
        calls = functions["ordinary_caller"]["VolatileRelatedCalls"]
        if len(calls) != 1 or calls[0]["TargetName"] != "qualified_formal" or not volatile_type(calls[0]["Parameters"][0]["Type"]) or volatile_type(calls[0]["Arguments"][0]["Value"]["Type"]):
            raise RuntimeError("The ordinary actual argument/qualified formal classifier activation changed.")
        cursor_operations = [row["Operation"] for row in functions["consume"]["CursorOperations"] if row["Operation"]["Kind"] == "eok_va_arg"]
        result_families = Counter()
        for operation in cursor_operations:
            if operation["ValueCategory"] != "prvalue" or len(operation["Operands"]) != 1:
                raise RuntimeError("The exercised va_arg category/retained cursor shape changed.")
            cursor = operation["Operands"][0]["Type"]
            tag = cursor["Pointee"]
            if cursor["Effective"]["Size"] != "8" or cursor["Effective"]["Alignment"] != "8" or not tag["BuiltinVaListTag"] or tag["Raw"]["Size"] != "24" or tag["Raw"]["Alignment"] != "8":
                raise RuntimeError("The exercised va_arg cursor is not the selected24/8 builtin tag pointer.")
            if [field["Offset"] for field in tag["VaListFields"]] != ["0", "4", "8", "16"]:
                raise RuntimeError("The exercised cursor field coordinates changed.")
            result = operation["Result"]
            arithmetic = result["Arithmetic"]
            family = "pointer" if result["Raw"]["Kind"] == "tk_pointer" else (
                "double" if arithmetic["Kind"] == "floating" and arithmetic["Bits"] == 64 else
                "int" if arithmetic["Bits"] == 32 and arithmetic["Signed"] is True else
                "uint" if arithmetic["Bits"] == 32 and arithmetic["Signed"] is False else "unsupported")
            result_families[family] += 1
        if result_families != Counter({"int": 4, "uint": 1, "double": 1, "pointer": 2}):
            raise RuntimeError("The focused genuine typed cursor families changed.")
        report["publicFixtureFacts"] = {"file": record(public_facts_path), "artifact": fixture_facts["Artifact"],
            "cursorGraphIdentityFamilies": dict(result_families), "inventoryOnly": True}
        native = artifacts / "native-oracle"
        run("native-reference-build", [gcc, "-std=c17", "-O2", fixtures / "last-four.c", fixtures / "native-last-four-oracle.c", "-o", native])
        native_output = run("native-reference", [native])
        values = [7, 2147483649, 6, 11, 23, 13, 1, 6, 123456, 55, 8, 1, 24]
        expected_output = "\n".join(["result=13", "observations=13", *[f"{i}:{value}" for i, value in enumerate(values)], "events=6", *[f"event{i}:{i + 1}" for i in range(6)]])
        if native_output != expected_output:
            raise RuntimeError("The independent native oracle differs from the defined exact observation protocol.")
        report["nativeReference"] = {"binary": record(native), "stdoutSha256": hashlib.sha256(native_output.encode()).hexdigest(),
            "role": "independent oracle only; never linked into generated execution"}
        runtime_project = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
        generated, repeated = artifacts / "generated", artifacts / "repeated"
        for label, output in (("first", generated), ("repeat", repeated)):
            run("emit-" + label, [dotnet, projection / "Projection.dll", "emit", objects / "last-four.roslynobj", output, runtime_project])
        report["generatedSources"] = generated_sources(generated)
        if generated_sources(repeated) != report["generatedSources"]:
            raise RuntimeError("Repeated memory-plan emission is not deterministic.")
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        checker = artifacts / "checker"
        run("generated-build", [dotnet, "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        run("checker-build", [dotnet, "build", tests / "LastFour/Checks.csproj", "-c", "Release", property_arg, "-o", checker, "--nologo"])
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
            run("checker-aot-publish", [dotnet, "publish", tests / "LastFour/Checks.csproj", "-c", "Release", property_arg,
                "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc),
                "-o", aot, "--nologo"], seconds=240)
            for iteration in (1, 2):
                if run("checker-aot-" + str(iteration), [aot / "Checks"]) != native_output:
                    raise RuntimeError("Generated NativeAOT observations differ from the full native reference.")
            linked = run("checker-aot-dependencies", ["ldd", aot / "Checks"])
            if any(word in linked.lower() for word in ("sqlite", "roslynobj", "memory-plan", "oracle")):
                raise RuntimeError("NativeAOT execution gained a native application or reader dependency.")
            report["aot"] = {"binary": record(aot / "Checks"), "dependencies": linked}
        report["status"] = "incomplete-jit-only" if args.skip_aot else "passed-bounded-typed-va-arg-volatile-formal"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(not Path(path).is_file() or digest(Path(path)) != sha for path, sha in implementation.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "Compiled/attributed source set or content changed."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        if any(not path.is_file() or digest(path) != sha for path, sha in input_hashes.items()):
            report["status"] = "incomplete"
            report["inputFreezeFailure"] = "A selected genuine object changed."
        report["inputFreezeVerified"] = "inputFreezeFailure" not in report
        (artifacts / "last-four-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(artifacts / "last-four-receipt.json"))
    return 0 if report["status"] == "passed-bounded-typed-va-arg-volatile-formal" else 1


if __name__ == "__main__":
    sys.exit(main())
