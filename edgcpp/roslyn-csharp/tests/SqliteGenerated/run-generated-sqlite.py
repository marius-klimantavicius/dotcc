#!/usr/bin/env python3
"""Test an existing genuine, unpublished SQLite candidate; never export or translate."""
from __future__ import annotations
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

PROJECT = Path(__file__).resolve().parents[2]
INPUT_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--candidate", type=Path, required=True)
    parser.add_argument("--candidate-receipt", type=Path, required=True)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--inventory", type=Path, default=Path("/mnt/storage/roslynobj-csharp-20261015/t0/sqlite-inventory-address-closure/program-report.json"))
    parser.add_argument("--oracle-build-receipt", type=Path, default=PROJECT / "acceptance/sqlite/native-oracle-build.json")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--cc", default="cc")
    parser.add_argument("--aot-cc", default="clang")
    for name, seconds, gib in (("build", 300, 8), ("aot", 900, 16), ("execution", 120, 4)):
        parser.add_argument("--" + name + "-seconds", type=int, default=seconds)
        parser.add_argument("--" + name + "-memory-bytes", type=int, default=gib * 1024**3)
    parser.add_argument("--log-bytes", type=int, default=64 * 1024**2)
    args = parser.parse_args()
    budgets = {name: {"seconds": getattr(args, name + "_seconds"), "memoryBytes": getattr(args, name + "_memory_bytes")}
        for name in ("build", "aot", "execution")}
    if min([args.log_bytes, *[value for budget in budgets.values() for value in budget.values()]]) <= 0:
        parser.error("All explicit budgets must be positive.")
    artifacts, candidate = args.artifacts.resolve(), args.candidate.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    phases, frozen, frozen_copies = [], {}, {}
    report = {"status": "incomplete-unpublished-candidate-tests", "role": "generated-csharp-candidate-tests-only",
        "productionPublished": False, "productionComplete": False, "wholeProviderCoverageAccepted": False,
        "nativeApplicationFallbackPermitted": False, "budgets": budgets, "logBytesPerPhase": args.log_bytes,
        "phases": phases, "limits": ["Tests an existing zero-error genuine full-object candidate; no export/retranslation/publication",
            "Bind/reset/clear smoke and unchanged five-case/43-statement initial SQL workload only",
            "Native application oracle runs only in separate processes; providers remain explicit system ABI bindings",
            "Passing these tests does not complete remaining T4 workloads or establish every provider contract"]}
    environment = os.environ.copy()
    environment.update({"DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER": "1", "MSBUILDDISABLENODEREUSE": "1"})
    for name in ("LD_LIBRARY_PATH", "LD_PRELOAD"): environment.pop(name, None)
    snapshot = artifacts / "harness-source/edgcpp/roslyn-csharp"
    repository = PROJECT.parents[1]

    def freeze(path: Path) -> None:
        path = path.resolve()
        if path not in frozen: frozen[path] = receipt(path)

    def run(name: str, command: list, budget: str = "execution") -> str:
        directory = artifacts / "phases" / name
        selected = budgets[budget]
        subprocess.run([sys.executable, str(snapshot / "tools/run-measured.py"), "--output-dir", str(directory),
            "--timeout-seconds", str(selected["seconds"]), "--memory-bytes", str(selected["memoryBytes"]),
            "--log-bytes", str(args.log_bytes), "--", *map(str, command)], cwd=repository, env=environment, check=False)
        path = directory / "run.json"
        result = json.loads(path.read_text())
        phases.append({"phase": name, "receipt": receipt(path), **result})
        if result["status"] != "passed" or result["exitCode"] != 0 or result.get("budgetViolation") is not None:
            raise RuntimeError(name + ": failed or exceeded explicit budget; preserved measured logs identify the failure.")
        return (directory / "stdout.txt").read_text().strip()

    def tool(name: str) -> Path:
        path = Path(shutil.which(name) or name).resolve()
        if not path.is_file(): raise RuntimeError("Selected tool is absent: " + name)
        freeze(path)
        return path

    def dependencies(path: Path) -> dict:
        value = json.loads(path.read_text())
        if {key.split("/")[0] for key in value["libraries"]} != {"SqliteGenerated", "Generated", "RoslynObj.CSharp.Runtime"}:
            raise RuntimeError("Generated managed dependencies gained a reader or other unexpected library.")
        return {"receipt": receipt(path), "libraries": value["libraries"]}

    def native_dependencies(name: str, path: Path) -> dict:
        linked = run(name, ["ldd", path])
        if "not found" in linked or any(word in linked.lower() for word in ("sqlite", "oracle", "roslynobj.dll", "e_sqlite")):
            raise RuntimeError("A generated binary gained a native application/reader dependency.")
        files = []
        for line in linked.splitlines():
            pieces = line.split()
            absolute = next((Path(piece) for piece in pieces if piece.startswith("/")), None)
            if absolute is not None and absolute.is_file(): files.append(receipt(absolute))
        return {"output": linked, "resolvedFiles": files, "virtualImages": "normal vDSO entries have no file receipt requirement"}

    try:
        relative_inputs = ["Directory.Build.props", "tests/SqliteGenerated/Program.cs", "tests/SqliteGenerated/SqliteGenerated.csproj",
            "tests/SqliteGenerated/SurfaceValidator.py", "tests/SqliteGenerated/run-generated-sqlite.py",
            "acceptance/sqlite/provider-plan.json", "acceptance/sqlite/sql-smoke.json",
            "acceptance/sqlite/run-native-bind-smoke.py", "acceptance/sqlite/run-native-sql.py", "tools/run-measured.py"]
        for name in relative_inputs:
            source, copied = PROJECT / name, snapshot / name
            freeze(source)
            copied.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, copied)
            frozen_copies[copied] = receipt(copied)
        candidate_receipt = args.candidate_receipt.resolve()
        freeze(candidate_receipt)
        captured = json.loads(candidate_receipt.read_text())
        if (captured.get("candidateGenerated") is not True or captured.get("errorCount") != 0
            or captured.get("productionPublished") is not False or captured.get("outputPublished") is not False
            or captured["input"]["sha256"] != INPUT_SHA256 or Path(captured["diagnosticCandidate"]).resolve() != candidate
            or (candidate / "completion.json").exists()):
            raise RuntimeError("Expected the exact unpublished, zero-error, unchanged full SQLite diagnostic candidate.")
        for item in captured["candidateFiles"]:
            path = (candidate / item["RelativePath"]).resolve()
            if not path.is_relative_to(candidate) or digest(path) != item["sha256"] or path.stat().st_size != item["bytes"]:
                raise RuntimeError("An emitted source/provider file differs from the exact candidate receipt.")
            freeze(path)
        freeze(Path(captured["input"]["path"]))
        if digest(Path(captured["input"]["path"])) != INPUT_SHA256: raise RuntimeError("The immutable SQLite object changed.")
        freeze(args.inventory)
        freeze(args.oracle_build_receipt)
        oracle_build = json.loads(args.oracle_build_receipt.read_text())
        oracle_library = Path(oracle_build["library"]["path"])
        freeze(oracle_library)
        if frozen[oracle_library.resolve()]["sha256"] != oracle_build["library"]["sha256"]:
            raise RuntimeError("Separate native oracle library differs from its build receipt.")
        report["separateNativeOracle"] = {"buildReceipt": receipt(args.oracle_build_receipt.resolve()),
            "library": receipt(oracle_library), "sourceSha256FromBuild": oracle_build["sourceSha256"],
            "compilerSha256FromBuild": oracle_build["compilerSha256"],
            "role": "previously built immutable native oracle; executed only in separate processes"}
        plan = json.loads((snapshot / "acceptance/sqlite/provider-plan.json").read_text())
        for provider in plan["providerFiles"]:
            freeze(Path(provider["path"]))
            if frozen[Path(provider["path"]).resolve()]["sha256"] != provider["sha256"]:
                raise RuntimeError("An explicit candidate system provider changed.")
        generated_project = candidate / "Generated.csproj"
        references = ET.parse(generated_project).getroot().findall(".//ProjectReference")
        if len(references) != 1: raise RuntimeError("Expected only the reader-independent target runtime project.")
        runtime = Path(references[0].attrib["Include"])
        if not runtime.is_absolute(): runtime = (candidate / runtime).resolve()
        if runtime.name != "RoslynObj.CSharp.Runtime.csproj": raise RuntimeError("Unexpected generated runtime project.")
        runtime_inputs = [path for path in runtime.parent.rglob("*") if path.is_file() and path.suffix in {".cs", ".csproj"}
            and not any(part in {"bin", "obj"} for part in path.relative_to(runtime.parent).parts)]
        runtime_props = next((parent / "Directory.Build.props" for parent in runtime.parents if (parent / "Directory.Build.props").is_file()), None)
        if runtime_props is not None: runtime_inputs.append(runtime_props)
        for path in runtime_inputs: freeze(path)
        report["candidate"] = {"receipt": receipt(candidate_receipt), "emittedFiles": captured["candidateFiles"],
            "input": captured["input"], "emittedFunctions": captured["emittedFunctions"], "runtimeInputs": [receipt(path) for path in runtime_inputs]}
        report["harnessSnapshot"] = {"directory": str(snapshot), "files": list(frozen_copies.values())}
        dotnet, cc, aot_cc = tool(args.dotnet), tool(args.cc), tool(args.aot_cc)
        for name in ("nm", "ldd"): tool(name)
        report["toolchain"] = {"dotnet": receipt(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
            "sdkInfo": run("dotnet-info", [dotnet, "--info"]), "abiCompiler": receipt(cc),
            "abiCompilerVersion": run("abi-compiler-version", [cc, "--version"]), "aotNativeCompiler": receipt(aot_cc),
            "aotNativeCompilerVersion": run("aot-native-compiler-version", [aot_cc, "--version"])}
        version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / version
        compiler_inputs = [sdk / "Roslyn/bincore/csc.dll", sdk / "MSBuild.dll"]
        linker_name = run("aot-linker-selection", [aot_cc, "-print-prog-name=ld"])
        linker = tool(linker_name)
        compiler_inputs.append(linker)
        for path in compiler_inputs: freeze(path)
        report["toolchain"]["managedCompilerAndLinker"] = [receipt(path) for path in compiler_inputs]
        aliases, api_report = artifacts / "CapturedAliases.cs", artifacts / "api-surface.json"
        run("validate-candidate-api", [sys.executable, snapshot / "tests/SqliteGenerated/SurfaceValidator.py",
            "--generated-project", generated_project, "--candidate-receipt", candidate_receipt,
            "--inventory", args.inventory.resolve(), "--aliases", aliases, "--report", api_report])
        freeze(aliases)
        freeze(api_report)
        # Rebuild the generated ABI-only helper with the attributed compiler, even
        # when an earlier candidate build left a newer incremental output behind.
        abi = candidate / "libRoslynObj.Generated.Abi.so"
        if abi.exists():
            previous = artifacts / "previous-native-helper/libRoslynObj.Generated.Abi.so"
            previous.parent.mkdir()
            shutil.copy2(abi, previous)
            report["previousNativeHelper"] = receipt(previous)
        run("build-generated-abi", [cc, "-std=c17", "-O2", "-fPIC", "-fvisibility=hidden", "-shared",
            candidate / "Generated.Abi.c", "-o", abi], "build")
        report["nativeHelper"] = {"binary": receipt(abi), "source": receipt(candidate / "Generated.Abi.c"),
            "exports": run("generated-abi-exports", ["nm", "-D", "--defined-only", abi]),
            "dependencies": native_dependencies("generated-abi-dependencies", abi)}
        freeze(abi)
        harness = snapshot / "tests/SqliteGenerated/SqliteGenerated.csproj"
        options = ["-p:GeneratedProject=" + str(generated_project), "-p:CandidateReceipt=" + str(candidate_receipt),
            "-p:SqliteApiAliases=" + str(aliases), "-p:SqliteApiReceipt=" + str(api_report),
            "-p:InventoryReport=" + str(args.inventory.resolve()), "-p:RoslynObjAbiCompiler=" + str(cc),
            "-p:UseSharedCompilation=false", "-nodeReuse:false", "--disable-build-servers", "--nologo"]
        jit, aot = artifacts / "jit", artifacts / "aot"
        run("build-generated-harness", [dotnet, "build", harness, "-c", "Release", *options, "-o", jit], "build")
        if digest(jit / abi.name) != digest(abi): raise RuntimeError("JIT native ABI helper copy changed.")
        report["jitBuild"] = {"files": [receipt(path) for path in sorted(jit.iterdir()) if path.is_file()],
            "dependencies": dependencies(jit / "SqliteGenerated.deps.json")}
        workload = snapshot / "acceptance/sqlite/sql-smoke.json"
        native_reports = {}
        for mode, runner in (("bind-smoke", "run-native-bind-smoke.py"), ("sql-smoke", "run-native-sql.py")):
            observation = artifacts / ("native-" + mode + ".json")
            run("native-" + mode, [sys.executable, snapshot / "acceptance/sqlite" / runner,
                "--build-receipt", args.oracle_build_receipt.resolve(), "--workload", workload, "--output", observation])
            native_reports[mode] = observation
        observations = []

        def execute(engine: str, mode: str, command: list) -> None:
            observation = artifacts / ("generated-" + engine + "-" + mode + ".json")
            run(engine + "-" + mode, [*command, mode, workload, observation, native_reports[mode]])
            value = json.loads(observation.read_text())
            expected = "passed-generated-bind-smoke" if mode == "bind-smoke" else "passed-generated-initial-43-statement-smoke"
            if value["status"] != expected or value["nativeComparison"] != "exact-typed-cases-equal":
                raise RuntimeError("Generated mode did not establish exact typed equality with its separate native oracle.")
            observations.append({"engine": engine, "mode": mode, "file": receipt(observation),
                "nativeObservation": receipt(native_reports[mode]), "cases": len(value["cases"]),
                "statements": sum(len(case["statements"]) for case in value["cases"]), "loadedLibraries": value["loadedLibraries"]})

        for mode in native_reports: execute("jit", mode, [dotnet, jit / "SqliteGenerated.dll"])
        run("publish-native-aot", [dotnet, "publish", harness, "-c", "Release", *options, "-r", "linux-x64",
            "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc), "-o", aot], "aot")
        if digest(aot / abi.name) != digest(abi): raise RuntimeError("NativeAOT ABI helper copy changed.")
        assets = harness.parent / "obj/project.assets.json"
        metadata = json.loads(assets.read_text())
        compiler_packages = {name: package for name, package in metadata["libraries"].items()
            if "microsoft.dotnet.ilcompiler/" in name.lower()}
        # The runtime-specific compiler is a downloadDependency, not necessarily
        # a libraries entry. Attribute the actual native ilc/JIT payload too.
        for framework in metadata["project"]["frameworks"].values():
            for dependency in framework.get("downloadDependencies", []):
                if "microsoft.dotnet.ilcompiler" not in dependency["name"].lower(): continue
                versions = [version.strip() for version in dependency["version"].strip("[]").split(",")]
                if len(versions) != 2 or versions[0] != versions[1]:
                    raise RuntimeError("Selected NativeAOT compiler download is not an exact version.")
                name = dependency["name"] + "/" + versions[0]
                compiler_packages[name] = {"type": "downloadDependency", "path": name.lower(), "selection": dependency}
        ilc = []
        for name, package in compiler_packages.items():
            root = next((Path(folder) / package["path"] for folder in metadata["packageFolders"]
                if (Path(folder) / package["path"]).is_dir()), None)
            if root is None: raise RuntimeError("Selected NativeAOT compiler package is absent.")
            selected_files = [path for path in sorted((root / "tools").rglob("*")) if path.is_file()]
            for path in selected_files: freeze(path)
            files = [receipt(path) for path in selected_files]
            ilc.append({"package": name, "assets": package, "compilerFiles": files})
        if not any(Path(file["path"]).name == "ilc" for item in ilc for file in item["compilerFiles"]):
            raise RuntimeError("Actual native ilc compiler identity could not be attributed.")
        report["toolchain"]["nativeAotCompilerPackages"] = {"identityRecordedAfterPublish": True, "packages": ilc}
        report["aotBuild"] = {"files": [receipt(path) for path in sorted(aot.iterdir()) if path.is_file()],
            "assets": receipt(assets), "dependencies": native_dependencies("aot-dependencies", aot / "SqliteGenerated")}
        for mode in native_reports: execute("aot", mode, [aot / "SqliteGenerated"])
        report["observations"] = observations
        report["apiSurface"] = receipt(api_report)
        report["status"] = "passed-unpublished-candidate-initial-sql-tests"
    except Exception as error:
        report["failure"] = str(error)
        print(str(error), file=sys.stderr)
    finally:
        changed = [str(path) for path, initial in {**frozen, **frozen_copies}.items()
            if not path.is_file() or digest(path) != initial["sha256"]]
        report["frozenInputs"] = list(frozen.values())
        report["frozenInputsUnchanged"] = not changed
        if changed:
            report["status"] = "incomplete-input-freeze-failed"
            report["changedInputs"] = changed
        report_path = artifacts / "generated-sqlite-report.json"
        report["observationAndApiReports"] = [receipt(path) for path in sorted(artifacts.glob("*.json")) if path != report_path]
        report_path.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "report": receipt(report_path)}))
    return 0 if report["status"] == "passed-unpublished-candidate-initial-sql-tests" else 1


if __name__ == "__main__":
    sys.exit(main())
