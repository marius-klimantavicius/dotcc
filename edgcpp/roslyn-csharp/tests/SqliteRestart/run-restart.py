#!/usr/bin/env python3
"""Coordinator-only full SQLite phase cancellation, clean restart and byte comparison."""
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


INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
ARTIFACT_IDENTITY = "9097468216bba3077ff4a24753d09245"
SOURCES = {"Generated.cs", "Generated.csproj", "Generated.Abi.c", "Generated.NativeImports.json"}
RUNTIME = Path("/mnt/storage/roslynobj-csharp-20261015/t3/value-storage/snapshot-11/edgcpp/roslyn-csharp/src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj")


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "sha256": digest(path), "bytes": path.stat().st_size}


def published(folder: Path) -> tuple[dict, dict, dict]:
    completion = json.loads((folder / "completion.json").read_text())
    if (completion["complete"] is not True or completion["manifestVersion"] != 1
            or completion["translatorVersion"] != "0.1.0" or completion["runtimeVersion"] != "0.1.0"
            or completion["inputs"] != [{"ordinal": 1, "sha256": INPUT_SHA256, "artifactIdentity": ARTIFACT_IDENTITY}]):
        raise RuntimeError("Expected actual completed output for the exact unchanged SQLite input.")
    files = {}
    for item in completion["files"]:
        relative = item["relativePath"]
        path = (folder / relative).resolve()
        if relative in files or path.parent != folder.resolve():
            raise RuntimeError("Unexpected duplicate/escaping completion source path.")
        actual = receipt(path)
        if actual["sha256"] != item["sha256"] or actual["bytes"] != item["sizeBytes"]:
            raise RuntimeError("Completed source changed: " + str(path))
        files[relative] = actual
    if set(files) != SOURCES | {"program-report.json"}:
        raise RuntimeError("The selected completion must contain exactly four emitted sources and the program report.")
    program = json.loads((folder / "program-report.json").read_text())
    if not program["inventoryComplete"] or not program["translationComplete"] or program["diagnostics"]:
        raise RuntimeError("The selected publication is not a complete zero-diagnostic program.")
    return completion, program, files


def normalized_report(program: dict) -> dict:
    value = copy.deepcopy(program)
    # Output destination is explicitly the only permitted report normalization.
    del value["output"]["directory"]
    return value


def tree(folder: Path) -> dict[str, str]:
    return {str(path.relative_to(folder)): digest(path) for path in sorted(folder.rglob("*")) if path.is_file()}


def equal_bytes(first: Path, second: Path) -> bool:
    with first.open("rb") as left, second.open("rb") as right:
        while True:
            one, two = left.read(128 * 1024), right.read(128 * 1024)
            if one != two:
                return False
            if not one:
                return True


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, required=True, help="The exact actual production CLI request.")
    parser.add_argument("--baseline", type=Path, required=True, help="Actual validated CLI output directory.")
    parser.add_argument("--artifacts", type=Path, required=True, help="Fresh unused campaign directory.")
    parser.add_argument("--project-root", type=Path, default=Path(__file__).resolve().parents[2])
    parser.add_argument("--translator-project", type=Path, help="Frozen product project; default the project-root translator.")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--small-manifest", type=Path, help="Optional genuine small-object request for InventoryCompleted/EmissionCompleted only.")
    args = parser.parse_args()
    project, baseline = args.project_root.resolve(), args.baseline.resolve()
    translator = args.translator_project.resolve() if args.translator_project else project / "src/RoslynObj.CSharp/RoslynObj.CSharp.csproj"
    reader = (translator.parent / "../../../roslyn/managed/RoslynObj").resolve()
    repository = project.parents[1]
    measured = project / "tools/run-measured.py"
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    phases: list[dict] = []
    frozen_inputs: dict[Path, str] = {}
    source_roots = [translator.parent, reader, project / "tests/SqliteRestart", RUNTIME.parent]

    def source_paths() -> set[Path]:
        paths = {path for folder in source_roots for path in folder.rglob("*") if path.is_file()
                 and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".md"}
                 and not any(part in {"obj", "bin", "__pycache__"} for part in path.relative_to(folder).parts)}
        paths |= {Path(__file__).resolve(), measured, reader.parent.parent / "schema/kinds.tsv"}
        for folder in source_roots:
            for parent in folder.parents:
                paths |= {path for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config", "nuget.config")
                          if (path := parent / name).is_file()}
                if parent == repository.parent:
                    break
        return paths

    paths = source_paths()
    implementation = {str(path): digest(path) for path in sorted(paths)}
    report: dict = {"gate": "T4-bounded-repeat-generation-phase-cancellation-restart", "status": "incomplete",
        "implementation": implementation, "phases": phases, "partialStageReuse": False,
        "generatedApplicationExecution": False, "scope": [
            "one full unchanged-object BeforePublication cancellation and separate-process restart using the same request/output",
            "four emitted source files compared byte for byte to the actual CLI baseline",
            "program-report normalization removes only output.directory; completion/report/metrics differences are preserved separately",
            "existing incomplete stage is retained untouched; restart recomputes inventory/emission rather than resuming cached work",
            "earlier real phases run only if a separate genuine small-object manifest is supplied",
            "no source/object mutation, sleeps, filesystem polling, partial-stage publication or incremental-resume claim"]}
    environment = os.environ.copy()
    environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0"
    environment["MSBUILDDISABLENODEREUSE"] = "1"

    def run(name: str, command: list, seconds: int = 300, memory: int = 8 * 1024**3) -> str:
        folder = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(folder), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(memory), "--log-bytes", str(64 * 1024**2), "--", *map(str, command)],
                       cwd=repository, env=environment, check=False)
        result = json.loads((folder / "run.json").read_text())
        phases.append(result | {"phase": name, "expectedExitCode": 0})
        if result["exitCode"] != 0 or result.get("budgetViolation") is not None:
            raise RuntimeError(name + ": inspect the preserved measured phase.")
        return (folder / "stdout.txt").read_text().strip()

    def pin(path: Path) -> None:
        frozen_inputs[path.resolve()] = digest(path)

    def resolved_request(path: Path) -> dict:
        request = json.loads(path.read_text())
        def resolve(value: str) -> str:
            return str((path.resolve().parent / value).resolve())
        for item in request["inputs"]:
            item["path"] = resolve(item["path"])
            if digest(Path(item["path"])) != item["sha256"]:
                raise RuntimeError("An immutable request input differs from its pinned hash.")
            pin(Path(item["path"]))
        request["runtime"]["runtimeProjectPath"] = resolve(request["runtime"]["runtimeProjectPath"])
        for binding in request["runtime"]["bindings"]:
            binding["libraryPath"] = resolve(binding["libraryPath"])
            if digest(Path(binding["libraryPath"])) != binding["librarySha256"]:
                raise RuntimeError("A selected provider changed.")
            pin(Path(binding["libraryPath"]))
        request["output"]["directory"] = resolve(request["output"]["directory"])
        return request

    try:
        completion, baseline_program, baseline_files = published(baseline)
        for path in [args.manifest.resolve(), baseline / "completion.json", *(baseline / name for name in baseline_files)]:
            pin(path)
        request = resolved_request(args.manifest)
        if (len(request["inputs"]) != 1 or request["inputs"][0]["sha256"] != INPUT_SHA256
                or request["inputs"][0].get("artifactIdentity") != ARTIFACT_IDENTITY
                or Path(request["runtime"]["runtimeProjectPath"]) != RUNTIME
                or Path(request["output"]["directory"]) != baseline):
            raise RuntimeError("Use the exact actual CLI baseline request with its snapshot-11 runtime path.")
        for section in ("target", "runtime", "roots", "link", "budgets", "output"):
            expected = copy.deepcopy(request[section])
            if section == "roots":
                expected.setdefault("entryPoint", None)  # Public DTO default, not an edit to the request.
            if expected != baseline_program[section]:
                raise RuntimeError("The actual CLI request/report section differs: " + section)
        report["baseline"] = {"completion": receipt(baseline / "completion.json"), "manifest": receipt(args.manifest),
                              "files": baseline_files, "output": str(baseline)}
        dotnet = Path(shutil.which(args.dotnet) or args.dotnet).resolve()
        if not dotnet.is_file():
            raise RuntimeError("Missing selected dotnet tool.")
        report["toolchain"] = {"dotnet": receipt(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
                               "sdkInfo": run("dotnet-info", [dotnet, "--info"])}
        sdk_version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["toolchain"]["managedCompilerFiles"] = [receipt(sdk / name) for name in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]
        checker = artifacts / "checker"
        run("build-public-phase-harness", [dotnet, "build", project / "tests/SqliteRestart/Checks.csproj", "-c", "Release",
            "--nologo", "-p:UseSharedCompilation=false", "-p:TranslatorProject=" + str(translator), "-o", checker])
        report["translatorProject"] = receipt(translator)
        report["build"] = [receipt(checker / name) for name in ("Checks.dll", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        report["earlyPhaseChecks"] = []
        if args.small_manifest is not None:
            pin(args.small_manifest)
            small = resolved_request(args.small_manifest)
            if any(item["sha256"] == INPUT_SHA256 for item in small["inputs"]):
                raise RuntimeError("Earlier phase checks require a distinct genuine small object, not another full SQLite scan.")
            for phase in ("InventoryCompleted", "EmissionCompleted"):
                current = copy.deepcopy(small)
                current["output"]["directory"] = str(artifacts / ("small-" + phase + "-output-parent") / "translated")
                manifest = artifacts / ("small-" + phase + ".json")
                manifest.write_text(json.dumps(current, indent=2) + "\n")
                pin(manifest)
                result = artifacts / ("small-" + phase + "-result.json")
                run("small-cancel-" + phase, [dotnet, checker / "Checks.dll", "cancel", manifest, result, phase],
                    seconds=current["budgets"]["maximumElapsedSeconds"], memory=current["budgets"]["maximumWorkingSetBytes"])
                actual = json.loads(result.read_text())
                if actual["status"] != "passed-real-phase-cancellation" or actual["stagedDirectory"] is not None:
                    raise RuntimeError("Small genuine earlier-phase cancellation did not pass without staging.")
                report["earlyPhaseChecks"].append({"phase": phase, "manifest": receipt(manifest), "result": receipt(result)})
        else:
            report["earlyPhaseScope"] = "not exercised in this receipt; harness supports both phases with a separate genuine small-object manifest"
        request["output"]["directory"] = str(artifacts / "output-parent" / "translated")
        restart_manifest = artifacts / "restart-request.json"
        restart_manifest.write_text(json.dumps(request, indent=2) + "\n")
        pin(restart_manifest)
        cancelled_path = artifacts / "cancel-result.json"
        seconds, memory = request["budgets"]["maximumElapsedSeconds"], request["budgets"]["maximumWorkingSetBytes"]
        run("full-before-publication-cancellation", [dotnet, checker / "Checks.dll", "cancel", restart_manifest, cancelled_path, "BeforePublication"], seconds, memory)
        cancelled = json.loads(cancelled_path.read_text())
        if (cancelled["status"] != "passed-real-phase-cancellation" or cancelled["resultStatus"] != "cancelled"
                or cancelled["outputPublished"] or not cancelled["stagedCompletionSeen"] or cancelled["stagedCompletionSurvives"]
                or cancelled["observedPhases"] != ["InventoryCompleted", "EmissionCompleted", "BeforePublication"]):
            raise RuntimeError("The full request did not cancel at the real pre-publication boundary.")
        stage = Path(cancelled["stagedDirectory"])
        staged_before = tree(stage)
        if set(staged_before) != SOURCES | {"program-report.json"}:
            raise RuntimeError("Cancelled staging lost required files or retained completion.")
        for name in SOURCES:
            if staged_before[name] != baseline_files[name]["sha256"] or not equal_bytes(stage / name, baseline / name):
                raise RuntimeError("Cancelled emitted source differs from actual CLI baseline: " + name)
        staged_program = json.loads((stage / "program-report.json").read_text())
        if normalized_report(staged_program) != normalized_report(baseline_program):
            raise RuntimeError("Staged report differs beyond its explicitly different output directory.")
        report["cancellation"] = {"result": receipt(cancelled_path), "incompleteStage": str(stage), "files": staged_before,
                                  "realStagedCompletionObservedThenRemoved": True}
        restart_path = artifacts / "restart-result.json"
        run("full-fresh-process-restart", [dotnet, checker / "Checks.dll", "complete", restart_manifest, restart_path], seconds, memory)
        restarted = json.loads(restart_path.read_text())
        if restarted["status"] != "passed-fresh-process-restart-from-input" or restarted["manifest"] != cancelled["manifest"]:
            raise RuntimeError("Restart did not use the exact same immutable request in a new process.")
        if tree(stage) != staged_before:
            raise RuntimeError("Fresh restart altered or reused the preserved incomplete stage.")
        output = Path(request["output"]["directory"])
        restarted_completion, restarted_program, restarted_files = published(output)
        for name in SOURCES:
            if (restarted_files[name]["sha256"] != baseline_files[name]["sha256"] or restarted_files[name]["bytes"] != baseline_files[name]["bytes"]
                    or not equal_bytes(output / name, baseline / name)):
                raise RuntimeError("Restart source is not byte-identical to actual CLI output: " + name)
        if normalized_report(restarted_program) != normalized_report(baseline_program):
            raise RuntimeError("Restart report differs beyond output.directory.")
        metadata = lambda value: {key: value[key] for key in ("manifestVersion", "translatorVersion", "runtimeVersion", "complete", "inputs")}
        if metadata(restarted_completion) != metadata(completion):
            raise RuntimeError("Restart completion identity/configuration metadata changed.")
        report["restart"] = {"result": receipt(restart_path), "manifest": receipt(restart_manifest), "files": restarted_files,
            "completion": receipt(output / "completion.json"), "preservedIncompleteStageUnchanged": True}
        report["deterministicSources"] = {name: baseline_files[name]["sha256"] for name in sorted(SOURCES)}
        report["pathDependentArtifacts"] = {"onlyNormalizedReportField": "output.directory", "baselineOutput": str(baseline),
            "restartOutput": str(output), "baselineProgramReport": baseline_files["program-report.json"],
            "restartProgramReport": restarted_files["program-report.json"], "baselineCompletion": receipt(baseline / "completion.json"),
            "restartCompletion": receipt(output / "completion.json"), "metrics": "recorded independently in cancel/restart results; not deterministic source evidence"}
        report["status"] = "passed-bounded-repeat-generation-and-restart-from-input"
    except Exception as problem:
        report["failure"] = str(problem)
        print(str(problem), file=sys.stderr)
    finally:
        changed = [str(path) for path, sha in frozen_inputs.items() if not path.is_file() or digest(path) != sha]
        report["inputFreezeVerified"] = not changed
        if changed:
            report["status"] = "incomplete"
            report["inputFreezeFailure"] = changed
        report["sourceFreezeVerified"] = source_paths() == paths and all(Path(path).is_file() and digest(Path(path)) == sha for path, sha in implementation.items())
        if not report["sourceFreezeVerified"]:
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "An attributed source set/content changed."
        (artifacts / "restart-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(artifacts / "restart-receipt.json"))
    return 0 if report["status"] == "passed-bounded-repeat-generation-and-restart-from-input" else 1


if __name__ == "__main__":
    sys.exit(main())
