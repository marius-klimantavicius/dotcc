#!/usr/bin/env python3
"""Coordinator-only production CLI integration proof for existing intrinsic inputs.

Reuses the accepted genuine inputs and checker. It neither exports inputs nor
builds native application bodies. This is not a complete SQLite execution gate.
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


def record(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--objects", type=Path, required=True)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--input-receipt", type=Path, help="Accepted existing-input receipt; defaults to project acceptance/intrinsics/report.json.")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--aot-cc", default="clang-18")
    args = parser.parse_args()
    project = args.project_root.resolve()
    repository = project.parents[1]
    objects = args.objects.resolve()
    tests = project / "tests"
    input_receipt = (args.input_receipt or project / "acceptance/intrinsics/report.json").resolve()
    prior = json.loads(input_receipt.read_text())
    if prior.get("status") != "passed-bounded-recorded-intrinsic-slice" or not prior.get("sourceFreezeVerified"):
        raise RuntimeError("An accepted frozen intrinsic input receipt is required.")
    prior_inputs = {item["name"]: item for item in prior["inputs"]}
    names = ("operations", "unsupported-order", "unsupported-overflow")
    for name in names:
        if digest(objects / (name + ".roslynobj")) != prior_inputs[name]["object"]["sha256"]:
            raise RuntimeError("An existing genuine intrinsic object differs from its accepted receipt: " + name)
        if digest(tests / "Fixtures/T3/intrinsics" / (name + ".c")) != prior_inputs[name]["source"]["sha256"]:
            raise RuntimeError("The selected snapshot source differs from the accepted input source: " + name)
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    measured = project / "tools/run-measured.py"
    template_path = project / "acceptance/sqlite/input.json"
    template = json.loads(template_path.read_text())
    environment = os.environ.copy()
    environment.pop("LD_LIBRARY_PATH", None)
    environment.pop("LD_PRELOAD", None)
    roots = [project / "src/RoslynObj.CSharp", project / "src/RoslynObj.CSharp.Cli", project / "src/RoslynObj.CSharp.Runtime",
             repository / "edgcpp/roslyn/managed/RoslynObj", tests / "Intrinsics"]

    def source_paths() -> set[Path]:
        paths = {path for folder in roots for path in folder.rglob("*") if path.is_file()
                 and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".md"}
                 and not any(part in {"obj", "bin", "__pycache__"} for part in path.relative_to(folder).parts)}
        paths |= {Path(__file__).resolve(), measured, template_path, input_receipt, repository / "edgcpp/roslyn/schema/kinds.tsv"}
        paths |= {tests / "Fixtures/T3/intrinsics" / (name + ".c") for name in names}
        for folder in roots:
            for parent in folder.parents:
                if parent == repository.parent:
                    break
                paths |= {path for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config", "nuget.config")
                          if (path := parent / name).is_file()}
        return paths

    paths = source_paths()
    implementation = {str(path): digest(path) for path in sorted(paths)}
    phases: list[dict] = []
    report: dict = {"gate": "T3-production-cli-recorded-intrinsic-integration", "status": "incomplete", "phases": phases,
        "projectRoot": str(project), "implementation": implementation, "existingInputReceipt": record(input_receipt),
        "producerProvenance": prior["producer"], "inputs": [], "exportsPerformed": False, "nativeApplicationBodiesBuilt": False,
        "nativeApplicationFallback": False, "nativeBindings": [], "nativeProviders": [],
        "limits": ["production CLI inventory/admission/translation integration of the existing fifteen-family bounded intrinsic slice only",
                   "no new intrinsic signature, memory-order, width or native provider admission",
                   "JIT and NativeAOT run the accepted exact34observations/trace/1000handoffs checker once each",
                   "genuine acquire-order and unsigned32-overflow variants remain unsupported before output publication",
                   "no full SQLite translation/execution, complete language support or general concurrency claim"]}

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

    try:
        dotnet, aot_cc = tool(args.dotnet), tool(args.aot_cc)
        report["toolchain"] = {"dotnet": record(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
            "sdkInfo": run("dotnet-info", [dotnet, "--info"]), "aotNativeCompiler": record(aot_cc),
            "aotNativeVersion": run("aot-cc-version", [aot_cc, "--version"])}
        sdk_version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["toolchain"]["managedCompilerFiles"] = [record(sdk / name) for name in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]
        cli = artifacts / "cli"
        run("build-production-cli", [dotnet, "build", project / "src/RoslynObj.CSharp.Cli/RoslynObj.CSharp.Cli.csproj",
                                    "-c", "Release", "--nologo", "-o", cli])
        report["build"] = [record(cli / name) for name in ("roslynobj-csharp.dll", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        requests = {}
        for name in names:
            captured = objects / (name + ".roslynobj")
            inspection_path = artifacts / (name + "-inspection.json")
            run(name + "-inspect", [dotnet, cli / "roslynobj-csharp.dll", "inspect", captured, "--report", inspection_path])
            inspection = json.loads(inspection_path.read_text())
            actual = inspection["input"]
            if actual["sha256"] != digest(captured) or actual["artifactIdentity"] != prior_inputs[name]["artifact"]:
                raise RuntimeError("CLI inspection lost accepted exact input identity: " + name)
            for field in ("producer", "edgRevision", "edgVersion", "captureProfile"):
                if actual[field] != template["inputs"][0][field]:
                    raise RuntimeError("CLI inspection differs from the selected producer/capture contract: " + field)
            for field in ("abi", "addressableUnitBits", "endianness", "pointerBits", "pointerAlignment"):
                if str(inspection["target"][field]) != str(template["target"][field]):
                    raise RuntimeError("CLI inspection differs from the selected source target: " + field)
            if inspection["languageMode"] != "c201710":
                raise RuntimeError("The accepted fixture is no longer captured GNU C17.")
            request = copy.deepcopy(template)
            request["inputs"] = [actual]
            request["runtime"].update({"providers": [], "bindings": [], "nativeErrorProtocol": "none",
                "runtimeProjectPath": str(project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj")})
            request["roots"] = {"mode": "library", "exports": [], "retainAllDefinitions": True}
            request["output"].update({"directory": str(artifacts / (name + "-generated")), "projectName": "IntrinsicCliFixture",
                                      "namespaceName": "Translated", "className": "Program"})
            request["budgets"].update({"maximumWorkingSetBytes": 4 * 1024**3, "maximumOutputBytes": 64 * 1024**2,
                                       "maximumOpenUnits": 1, "maximumElapsedSeconds": 120})
            request_path = artifacts / (name + "-request.json")
            request_path.write_text(json.dumps(request, indent=2) + "\n")
            requests[name] = (request, request_path)
            report["inputs"].append({"name": name, "object": record(captured), "publicInspection": record(inspection_path),
                                     "request": record(request_path), "artifact": actual["artifactIdentity"]})
        request, request_path = requests["operations"]
        inventory_path, metrics_path = artifacts / "inventory-report.json", artifacts / "inventory-metrics.json"
        output = run("production-inventory", [dotnet, cli / "roslynobj-csharp.dll", "inventory", request_path,
            "--report", inventory_path, "--metrics", metrics_path])
        inventory = json.loads(inventory_path.read_text())
        if not output.startswith("InventoryComplete:") or not inventory["inventoryComplete"] or inventory["translationComplete"] or inventory["diagnostics"]:
            raise RuntimeError("Production inventory did not complete cleanly before translation.")
        report["inventory"] = {"report": record(inventory_path), "metrics": record(metrics_path)}
        translation_path, translation_metrics = artifacts / "translation-report.json", artifacts / "translation-metrics.json"
        output = run("production-translate", [dotnet, cli / "roslynobj-csharp.dll", "translate", request_path,
            "--report", translation_path, "--metrics", translation_metrics])
        completed = json.loads(translation_path.read_text())
        if not output.startswith("TranslationComplete:") or not completed["inventoryComplete"] or not completed["translationComplete"] or completed["diagnostics"]:
            raise RuntimeError("Production translation did not complete cleanly.")
        required = [item for item in completed["imports"] if item["required"]]
        functions = {function["locator"]: function for function in completed["functions"]}
        actual_builtins = {functions[item["locator"]]["builtin"] for item in required}
        expected_builtins = {"bfk_" + name for name in ("atomic_load_1", "atomic_load_2", "atomic_load_4", "atomic_store_1", "atomic_store_2", "atomic_store_4", "atomic_store_8",
            "sync_synchronize", "bswap16", "bswap32", "bswap64", "clzll", "add_overflow", "sub_overflow", "mul_overflow")}
        if actual_builtins != expected_builtins or len(required) != 15 or any(item["kind"] != "function" or
            item["provider"] != "RoslynObj.CSharp.Runtime.TargetIntrinsics" or item["bindingStatus"] != "recorded-intrinsic-lowering-selected" for item in required):
            raise RuntimeError("The completed report did not select all15 recorded intrinsic imports through the runtime lowering provider.")
        if completed["runtime"]["providers"] or completed["runtime"]["bindings"] or completed["roots"]["exports"]:
            raise RuntimeError("The production intrinsic request gained native bindings/providers/exports.")
        generated = Path(request["output"]["directory"])
        files = {path.name: record(path) for path in generated.iterdir() if path.is_file()}
        if set(files) != {"Generated.cs", "Generated.csproj", "program-report.json", "completion.json"}:
            raise RuntimeError("The production intrinsic project gained a native binding/export/gateway file.")
        source = (generated / "Generated.cs").read_text()
        if any(marker in source for marker in ("DllImport", "NativeLibrary", "RoslynObject")):
            raise RuntimeError("The production intrinsic generated body gained a native/reader fallback.")
        completion = json.loads((generated / "completion.json").read_text())
        if completion["complete"] is not True or completion["inputs"][0]["sha256"] != request["inputs"][0]["sha256"]:
            raise RuntimeError("The atomic publication completion lost its selected input identity.")
        if {item["relativePath"] for item in completion["files"]} != set(files) - {"completion.json"}:
            raise RuntimeError("The completion does not cover the exact emitted file set.")
        for item in completion["files"]:
            target = generated / item["relativePath"]
            if digest(target) != item["sha256"] or target.stat().st_size != item["sizeBytes"]:
                raise RuntimeError("An emitted file differs from its completion hash/length.")
        if digest(generated / "program-report.json") != digest(translation_path):
            raise RuntimeError("The embedded and explicit completion reports differ.")
        report["translation"] = {"report": record(translation_path), "metrics": record(translation_metrics),
            "requiredIntrinsicImports": required, "builtinSet": sorted(actual_builtins), "generatedFiles": files}
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        checker = artifacts / "checker"
        run("generated-build", [dotnet, "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        run("checker-build", [dotnet, "build", tests / "Intrinsics/Checks.csproj", "-c", "Release", property_arg, "--nologo", "-o", checker])
        if digest(checker / "Generated.dll") != digest(generated / "bin/Release/net10.0/Generated.dll"):
            raise RuntimeError("The accepted checker did not use the separately built production generated project.")
        report["jit"] = {"checker": record(checker / "Checks.dll"), "generated": record(checker / "Generated.dll"),
            "runtime": record(checker / "RoslynObj.CSharp.Runtime.dll"), "dependencies": dependencies(checker / "Checks.deps.json", True),
            "generatedDependencies": dependencies(generated / "bin/Release/net10.0/Generated.deps.json", False)}
        expected_output_hash = prior["nativeReference"]["stdoutSha256"]
        jit_output = run("production-checker-jit", [dotnet, checker / "Checks.dll"])
        if hashlib.sha256(jit_output.encode()).hexdigest() != expected_output_hash:
            raise RuntimeError("Production JIT exact34observations/trace/1000handoffs differ from the accepted independent native oracle.")
        aot = artifacts / "aot"
        run("checker-aot-publish", [dotnet, "publish", tests / "Intrinsics/Checks.csproj", "-c", "Release", property_arg,
            "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc),
            "--nologo", "-o", aot], seconds=240)
        aot_output = run("production-checker-aot", [aot / "Checks"])
        if hashlib.sha256(aot_output.encode()).hexdigest() != expected_output_hash:
            raise RuntimeError("Production NativeAOT exact34observations/trace/1000handoffs differ from the accepted independent native oracle.")
        linked = run("checker-aot-dependencies", ["ldd", aot / "Checks"])
        if any(marker in linked.lower() for marker in ("sqlite", "roslynobj", "intrinsic", "oracle")):
            raise RuntimeError("Production NativeAOT gained a native application/reader/intrinsic provider dependency.")
        report["aot"] = {"binary": record(aot / "Checks"), "dependencies": linked}
        report["exactObservationProtocolSha256"] = expected_output_hash
        report["rejections"] = []
        for name, message in (("unsupported-order", "captured constant relaxed atomic order"),
                              ("unsupported-overflow", "scalar width/sign/effective alignment")):
            negative, negative_path = requests[name]
            negative_report = artifacts / (name + "-report.json")
            run(name + "-translate-rejected", [dotnet, cli / "roslynobj-csharp.dll", "translate", negative_path, "--report", negative_report], expected=1)
            rejected = json.loads(negative_report.read_text())
            if not rejected["inventoryComplete"] or rejected["translationComplete"] or not any(message in item["message"] for item in rejected["diagnostics"]):
                raise RuntimeError("A valid unsupported intrinsic input did not fail at bounded lowering: " + name)
            negative_output = Path(negative["output"]["directory"])
            if negative_output.exists() or list(negative_output.parent.glob(negative_output.name + ".incomplete-*/completion.json")):
                raise RuntimeError("An unsupported intrinsic input published a complete/generated project: " + name)
            report["rejections"].append({"name": name, "report": record(negative_report), "outputPublished": False})
        report["status"] = "passed-production-cli-intrinsic-integration"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(not Path(path).is_file() or digest(Path(path)) != sha for path, sha in implementation.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "The selected compiled/attributed source set or content changed."
        if any(digest(objects / (name + ".roslynobj")) != prior_inputs[name]["object"]["sha256"] for name in names):
            report["status"] = "incomplete"
            report["inputFreezeFailure"] = "A reused genuine object changed."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        report["inputFreezeVerified"] = "inputFreezeFailure" not in report
        (artifacts / "cli-intrinsics-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(artifacts / "cli-intrinsics-receipt.json"))
    return 0 if report["status"] == "passed-production-cli-intrinsic-integration" else 1


if __name__ == "__main__":
    sys.exit(main())
