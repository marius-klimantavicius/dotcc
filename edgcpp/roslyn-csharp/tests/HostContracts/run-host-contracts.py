#!/usr/bin/env python3
"""Coordinator-only finite Linux x64 errno/TLS/pthread JIT + NativeAOT probe.

No reader, translated application, SQLite, or product host bridge is involved.
Actual errno observations are retained independently for each run; they are
never treated as a universal managed-transition preservation contract.
"""
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


def file_record(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def write_json(path: Path, value: object) -> None:
    path.write_text(json.dumps(value, indent=2) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--cc", default="gcc")
    parser.add_argument("--aot-cc", default="clang")
    parser.add_argument("--skip-aot", action="store_true", help="Record an incomplete JIT-only receipt.")
    args = parser.parse_args()
    probe = Path(__file__).resolve().parent
    project = probe.parents[1]
    repository = project.parents[1]
    fixtures = project / "tests/Fixtures/HostContracts"
    measured = project / "tools/run-measured.py"
    artifacts = args.artifacts.resolve()
    if artifacts.is_relative_to(probe) or artifacts.is_relative_to(fixtures):
        raise ValueError("Put artifacts outside the owned source directories.")
    artifacts.mkdir(parents=True, exist_ok=False)
    environment = os.environ.copy()
    removed_environment = [name for name in ("LD_PRELOAD", "LD_LIBRARY_PATH") if name in environment]
    for name in removed_environment:
        del environment[name]

    def source_paths() -> set[Path]:
        paths = {path for folder in (probe, fixtures) for path in folder.rglob("*")
                 if path.is_file() and path.suffix in {".cs", ".csproj", ".c", ".h", ".py", ".md", ".props", ".targets"}
                 and not any(part in {"bin", "obj", "__pycache__"} for part in path.relative_to(folder).parts)}
        paths |= {project / "Directory.Build.props", measured}
        paths |= {path for parent in (project, project.parent, repository) for name in
                  ("global.json", "NuGet.Config", "nuget.config", "Directory.Build.props", "Directory.Build.targets")
                  if (path := parent / name).is_file()}
        return paths

    paths = source_paths()
    source_hashes = {str(path.relative_to(repository)): digest(path) for path in sorted(paths)}
    phases: list[dict] = []
    report: dict = {
        "gate": "T3-preparation-finite-host-contract-probe", "status": "incomplete", "phases": phases,
        "implementation": source_hashes, "removedEnvironmentVariables": removed_environment,
        "productHostBridgeImplemented": False, "readerDependency": False,
        "nativeApplicationBodies": False, "sqliteDependency": False,
        "limits": ["finite observations on the actual Linux x64 host and selected module hashes only",
                   "raw PInvoke managed-later errno has no paired immediate native snapshot for that same invocation",
                   "separate native open invocation supplies its own immediate errno baseline",
                   "protected experiment restores errno only inside its native scope around nested callbacks",
                   "managed allocation, GC, UCO and reentry observations do not establish universal preservation",
                   "one joined foreign pthread per run; no asynchronous callback lifetime or general thread model claim",
                   "opaque pthread mutex objects stay native; only native sizes/constants and return protocols are observed",
                   "no product host provider, TLS, mutex, errno or SQLite execution implementation accepted"],
        "runs": [],
    }

    def run(name: str, command: list, seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase),
                        "--timeout-seconds", str(seconds), "--memory-bytes", str(4 * 1024**3),
                        "--", *map(str, command)], cwd=repository, env=environment, check=False)
        recorded = json.loads((phase / "run.json").read_text())
        phases.append(recorded | {"phase": name, "expectedExitCode": 0})
        if recorded["exitCode"] != 0 or recorded.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected execution result; inspect measured phase logs.")
        return (phase / "stdout.txt").read_text().strip()

    def executable(value: str) -> Path:
        path = Path(shutil.which(value) or value).resolve()
        if not path.is_file():
            raise RuntimeError("Missing tool: " + value)
        return path

    def dependency_audit(name: str, binary: Path) -> dict:
        dynamic = run(name + "-dynamic", ["readelf", "-d", binary])
        loaded = run(name + "-ldd", ["ldd", binary])
        if "sqlite" in dynamic.lower() or "sqlite" in loaded.lower() or "roslynobj" in loaded.lower():
            raise RuntimeError(name + ": probe acquired a reader/application/SQLite native dependency.")
        library_paths = sorted({Path(token).resolve() for line in loaded.splitlines() for token in line.split()
                                if token.startswith("/") and Path(token).is_file()})
        return {"elfDynamic": dynamic, "ldd": loaded, "resolvedFiles": [file_record(path) for path in library_paths]}

    expected_exports = {
        "hc_abi_count", "hc_abi_name", "hc_abi_value", "hc_tls_address", "hc_tls_read", "hc_tls_write",
        "hc_set_errno", "hc_fail_capture", "hc_report_count", "hc_report_name", "hc_protected_probe",
        "hc_reentry_probe", "hc_thread_probe", "hc_mutex_probe", "hc_provider_count", "hc_provider_symbol",
        "hc_provider_address", "hc_provider_path", "hc_provider_base",
    }
    expected_providers = {
        "hc_abi_count", "open", "__errno_location", "pthread_create", "pthread_join", "pthread_mutexattr_init",
        "pthread_mutexattr_settype", "pthread_mutex_init", "pthread_mutex_lock", "pthread_mutex_trylock",
        "pthread_mutex_unlock", "pthread_mutex_destroy", "pthread_mutexattr_destroy", "calloc", "free", "dladdr",
    }
    essential_checks = {
        "loaded_errno_symbol_matches_native_provider", "loaded_open_symbol_matches_native_provider",
        "separate_native_immediate_errno_is_ENOTDIR", "protected_errno_restored_inside_native_scope",
        "protected_outer_and_nested_completed_once", "pthread_create_return_zero", "pthread_join_return_zero",
        "pthread_callback_acquired_completion", "pthread_errno_tls_distinct_from_main",
        "pthread_marker_tls_distinct_from_main", "foreign-pthread_outer_and_nested_completed_once",
        "mutex_trylock_held_returns_EBUSY", "mutex_errorcheck_lock_held_returns_EDEADLK", "mutex_unlock_unowned_returns_EPERM",
    }
    selected_provider_facts: list | None = None
    selected_abi: dict | None = None
    selected_check_names: set[str] | None = None

    def observe(name: str, command: list, dynamic_code: bool, helper: Path) -> None:
        nonlocal selected_provider_facts, selected_abi, selected_check_names
        target = artifacts / (name + "-report.json")
        run(name, [*command, helper, target])
        observation = json.loads(target.read_text())
        checks = observation["checks"]
        check_names = {check["name"] for check in checks}
        if (observation["schema"] != "roslynobj-host-contract-probe-v1" or not observation["passed"]
                or observation["dynamicCodeSupported"] != dynamic_code
                or not essential_checks <= check_names or len(check_names) != len(checks)
                or not all(check["passed"] for check in checks)):
            raise RuntimeError(name + ": complete named probe invariants or execution mode did not pass.")
        if Path(observation["nativeHelper"]) != helper or observation["nativeHelperSha256"] != digest(helper):
            raise RuntimeError(name + ": native helper identity differs from the selected binary.")
        providers = observation["providers"]
        if len(providers) != len(expected_providers) or {row["symbol"] for row in providers} != expected_providers:
            raise RuntimeError(name + ": selected native provider symbol audit is incomplete.")
        for row in providers:
            if digest(Path(row["path"])) != row["sha256"] or int(row["address"], 16) == 0 or int(row["moduleBase"], 16) == 0:
                raise RuntimeError(name + ": an actually loaded provider has no matching file/address evidence.")
        provider_facts = sorted((row["symbol"], row["path"], row["sha256"]) for row in providers)
        if selected_provider_facts is None:
            selected_provider_facts, selected_abi, selected_check_names = provider_facts, observation["nativeAbi"], check_names
        elif provider_facts != selected_provider_facts or observation["nativeAbi"] != selected_abi or check_names != selected_check_names:
            raise RuntimeError(name + ": selected host/provider/ABI/check contract changed between runs.")
        direct = observation["directUnprotected"]
        protected = observation["protectedNativeSnapshotRestore"]
        report["runs"].append({
            "name": name, "report": file_record(target), "checksPassed": len(checks), "dynamicCodeSupported": dynamic_code,
            "rawPInvokeManagedLaterErrnoObservations": {
                key: direct[key]["errnoLater"] for key in
                ("managedLater", "managedAfterAllocationAndGc", "managedAfterSeparateNativeReentry")},
            "separateNativeInvocation": direct["separateNativeInvocation"],
            "protectedNativeScope": {
                "immediateErrno": protected["native_errno_immediate"],
                "afterCallbackErrno": protected["native_errno_after_callback"],
                "restoredErrno": protected["native_errno_restored"],
                "callbackChangedErrnoObserved": protected["native_errno_after_callback"] != protected["native_errno_immediate"],
                "managedLaterErrnoObservation": observation["protectedManagedLater"]["errnoLater"]},
            "observationEqualityAcrossManagedTransitionsRequired": False,
        })

    try:
        dotnet = executable(args.dotnet)
        cc = executable(args.cc)
        report["dotnet"] = {"executable": file_record(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
                            "info": run("dotnet-info", [dotnet, "--info"])}
        report["cCompiler"] = {"executable": file_record(cc), "version": run("cc-version", [cc, "--version"]), "components": []}
        for component in ("cc1", "collect2", "as", "ld"):
            resolved = run("cc-component-" + component, [cc, "-print-prog-name=" + component])
            report["cCompiler"]["components"].append({"name": component, **file_record(executable(resolved))})
        sdk_lines = run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines()
        sdk_version = report["dotnet"]["sdkVersion"]
        sdk_line = next(line for line in sdk_lines if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["dotnet"]["compilerFiles"] = [file_record(sdk / path) for path in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]

        helper = artifacts / "libhost-contracts-probe.so"
        run("native-helper-build", [cc, "-std=c17", "-O2", "-g", "-fPIC", "-shared", "-Wall", "-Wextra", "-Werror",
                                    "-pthread", fixtures / "host-contracts.c", "-ldl", "-o", helper])
        exports = run("native-helper-exports", ["nm", "-D", "--defined-only", helper])
        actual_exports = {line.split()[-1] for line in exports.splitlines() if len(line.split()) >= 3 and line.split()[-2] in {"T", "W"}}
        if actual_exports != expected_exports:
            raise RuntimeError("Native helper exports differ from the explicit ABI observation/forwarding surface.")
        report["nativeHelper"] = {"binary": file_record(helper), "source": file_record(fixtures / "host-contracts.c"),
                                  "exports": exports, "dependencies": dependency_audit("native-helper", helper)}
        project_file = probe / "HostContracts.csproj"
        jit = artifacts / "jit"
        run("jit-build", [dotnet, "build", project_file, "-c", "Release", "--nologo", "-o", jit])
        managed_dependencies = json.loads((jit / "HostContracts.deps.json").read_text())["libraries"]
        if {name.split("/")[0] for name in managed_dependencies} != {"HostContracts"}:
            raise RuntimeError("The independent host probe acquired a reader/product/application managed dependency.")
        report["jit"] = {"binary": file_record(jit / "HostContracts.dll"), "dependencies": file_record(jit / "HostContracts.deps.json"),
                         "runtimeConfig": file_record(jit / "HostContracts.runtimeconfig.json")}
        observe("jit-first", [dotnet, jit / "HostContracts.dll"], True, helper)
        observe("jit-repeat", [dotnet, jit / "HostContracts.dll"], True, helper)
        if not args.skip_aot:
            aot = artifacts / "aot"
            aot_cc = executable(args.aot_cc)
            aot_ld = executable(run("aot-linker-path", [aot_cc, "-print-prog-name=ld"]))
            report["aotNativeCompiler"] = {"executable": file_record(aot_cc),
                "version": run("aot-cc-version", [aot_cc, "--version"]), "linker": file_record(aot_ld)}
            run("aot-publish", [dotnet, "publish", project_file, "-c", "Release", "-r", "linux-x64", "--self-contained", "true",
                                "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc), "--nologo", "-o", aot], seconds=240)
            report["aot"] = {"binary": file_record(aot / "HostContracts"), "dependencies": dependency_audit("aot", aot / "HostContracts")}
            assets_path = probe / "obj/project.assets.json"
            assets = json.loads(assets_path.read_text())
            report["aot"]["restoreAssets"] = file_record(assets_path)
            ilc_paths = {name: value["path"] for name, value in assets["libraries"].items() if "ilcompiler" in name.lower()}
            for framework in assets["project"]["frameworks"].values():
                for dependency in framework.get("downloadDependencies", []):
                    if "ilcompiler" not in dependency["name"].lower():
                        continue
                    version = dependency["version"].strip("[]").split(",")
                    if len(version) != 2 or version[0].strip() != version[1].strip():
                        raise RuntimeError("NativeAOT compiler download is not pinned to one exact package version.")
                    exact = version[0].strip()
                    ilc_paths[dependency["name"] + "/" + exact] = dependency["name"].lower() + "/" + exact
            if not any(name.lower().startswith("runtime.linux-x64.microsoft.dotnet.ilcompiler/") for name in ilc_paths):
                raise RuntimeError("NativeAOT restore assets did not expose the selected compiler package.")
            report["aot"]["compilerPackages"] = []
            for name, relative in sorted(ilc_paths.items()):
                package = next(Path(folder) / relative for folder in assets["packageFolders"]
                               if (Path(folder) / relative).is_dir())
                tools = sorted(path for path in (package / "tools").rglob("*") if path.is_file())
                report["aot"]["compilerPackages"].append({"name": name, "path": str(package), "toolFiles": [file_record(path) for path in tools]})
            observe("aot-first", [aot / "HostContracts"], False, helper)
            observe("aot-repeat", [aot / "HostContracts"], False, helper)
        report["selectedProviderFiles"] = [{"symbol": symbol, "path": path, "sha256": sha} for symbol, path, sha in selected_provider_facts or []]
        report["status"] = "incomplete-jit-only" if args.skip_aot else "passed-finite-host-probe"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(digest(repository / path) != sha for path, sha in source_hashes.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "Owned source set/content changed during the receipt."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        write_json(artifacts / "host-contracts-receipt.json", report)
    print(report["status"] + ": " + str(artifacts / "host-contracts-receipt.json"))
    return 0 if report["status"] == "passed-finite-host-probe" else 1


if __name__ == "__main__":
    sys.exit(main())
