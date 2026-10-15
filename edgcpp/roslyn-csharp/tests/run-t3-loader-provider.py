#!/usr/bin/env python3
"""Reproduce the bounded glibc loader snapshot/read-clear provider. Coordinator only."""
from __future__ import annotations
import argparse
import json
import os
from pathlib import Path
import re
import runpy
import shutil
import subprocess
import sys

PROJECT = Path(__file__).resolve().parents[1]
SHARED_DRIVER = PROJECT / "tests/run-t3-errno-provider.py"
SHARED = runpy.run_path(str(SHARED_DRIVER))
digest, receipt, exported_functions = (SHARED[name] for name in ("digest", "receipt", "exported_functions"))
ARTIFACT_ROOT = Path("/mnt/storage/roslynobj-csharp-20261015")
OBJECT_SHA256 = "6d1642b64eb629a1bd6896b8ce5847e556565deebdd89f16c18850d503210d1c"
SOURCE_SHA256 = "457bb537ea17b206d9052084df6617bccfb637d4a2b76fd0837400c167eb9187"
AUDIT_SHA256 = "4a648ed17fdcb1fe7b3389bc2c942df6e839e9106b1827192a74fe285e3f4c0a"
LOADER_NAMES = {"dlopen", "dlsym", "dlclose", "dlerror"}
BASELINE = "\n".join(["pending-error-after-successful-dlopen=0", "dlerror-read-clear=0",
    "pending-error-after-successful-dlsym=0", "pending-error-after-successful-dlclose=0",
    "null-symbol-value=1", "null-symbol-error=0", "borrowed-message-copy=1", "read-clear-next=0", "dlerror-errno=2721"])
CHECKER_OUTPUT = "result=17\nobservations=" + ",".join(["1"] * 17) + "\nloader-snapshot-read-clear-lifetime=passed\nloader-canonical-and-unknown-indirect=passed"


def source_paths(project: Path, fixtures: Path, measured: Path) -> list[Path]:
    return sorted(set(SHARED["implementation_paths"](project, fixtures, measured)) | {Path(__file__).resolve(), SHARED_DRIVER})


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--object", type=Path)
    parser.add_argument("--export-recipe", type=Path, default=ARTIFACT_ROOT / "t3/providers/export-errno-thread-2/run.json")
    parser.add_argument("--public-use-audit", type=Path, default=ARTIFACT_ROOT / "t3/providers/sqlite-loader-use-audit-1.json")
    parser.add_argument("--libc", type=Path, default=Path("/usr/lib/x86_64-linux-gnu/libc.so.6"))
    parser.add_argument("--cc", default="cc")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--skip-aot", action="store_true")
    args = parser.parse_args()
    project, repository = PROJECT, PROJECT.parents[1]
    fixtures = project / "tests/Fixtures/T3/providers"
    measured = project / "tools/run-measured.py"
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    native = artifacts / "native"
    native.mkdir()
    libc = args.libc.resolve()
    producer_sha = SHARED["PRODUCER_SHA256"]
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer") / (producer_sha + "-roslyn-cpfe")
    export_driver = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
    emitter_project = project / "tests/NativeProviders/LoaderEmit.csproj"
    checker_project = project / "tests/NativeProviders/LoaderChecks.csproj"
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    phases: list[dict] = []
    implementation = [{"path": str(p.relative_to(project)), "sha256": digest(p)} for p in source_paths(project, fixtures, measured)]
    report = {"gate": "T3-partial-selected-glibc-loader", "status": "incomplete", "phases": phases,
        "nativeApplicationFallback": False, "implementation": implementation,
        "limits": ["Five exact selected libc imports; not all 88 SQLite provider contracts",
            "Ordinary captured Linux x64 scalar/pointer loader signatures only",
            "Selected registered loader/error addresses share the proven provider contract; no arbitrary loader interposition",
            "Read-clear preserves returned bytes until next selected loader call; native pthread destructor owns cleanup",
            "This fixture does not prove incoming raw loader-error state from arbitrary foreign native application bodies",
            "No SQLite execution, native extension algorithm fallback, cancellation or native C++ exception propagation claim"]}

    def run(name: str, command: list[str], seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
            "--memory-bytes", str(2 * 1024**3), "--", *command], cwd=repository, env=environment, check=False)
        recorded = json.loads((phase / "run.json").read_text())
        phases.append(recorded | {"phase": name, "expectedExitCode": 0})
        if recorded["exitCode"] != 0 or recorded.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected execution result; see measured phase receipt.")
        return (phase / "stdout.txt").read_text().strip()

    try:
        if digest(producer) != producer_sha or digest(libc) != SHARED["LIBC_SHA256"]:
            raise RuntimeError("Frozen producer/explicit libc identity changed.")
        if digest(args.export_recipe) != SHARED["EXPORT_RECIPE_SHA256"] or digest(fixtures / "loader-state.c") != SOURCE_SHA256:
            raise RuntimeError("Frozen host recipe/application fixture identity changed.")
        if digest(args.public_use_audit) != AUDIT_SHA256:
            raise RuntimeError("Expected the immutable actual public SQLite loader-use audit.")
        public = json.loads(args.public_use_audit.read_text())
        if (public["Input"]["Sha256"] != SHARED["SQLITE_SHA256"] or {s["Entry"] for s in public["Signatures"]} != LOADER_NAMES
            or len(public["Uses"]) != 3 or any(not use["CalleeOnly"] for use in public["Uses"])
            or [c["Entry"] for c in public["AddressConstants"]] != ["dlsym"]):
            raise RuntimeError("The selected loader-use pattern changed.")
        report["publicUseAudit"] = {"receipt": receipt(args.public_use_audit.resolve()), "input": public["Input"],
            "signatures": public["Signatures"], "referenceUses": public["Uses"], "addressConstants": public["AddressConstants"],
            "scope": "unixDlError casts dlerror result to local const-byte pointer, tests it, then passes it to translated sqlite3_snprintf before leaving mutex; no general alias-analysis claim"}
        recipe = json.loads(args.export_recipe.read_text())
        flags = recipe["command"][2:-3]
        if recipe["exitCode"] != 0 or recipe.get("budgetViolation") is not None or any("SQLITE" in flag or "timing" in flag for flag in flags):
            raise RuntimeError("Expected the successful SQLite-define-free frozen host flags.")
        captured = args.object.resolve() if args.object else artifacts / "loader-state.roslynobj"
        command = [sys.executable, str(export_driver), *flags, str(fixtures / "loader-state.c"), "-o", str(captured)]
        report["export"] = {"recipe": receipt(args.export_recipe.resolve()), "frozenHostFlags": flags, "command": command, "reusedObject": args.object is not None}
        report["inputs"] = [receipt(producer), receipt(export_driver), receipt(libc)] + [receipt(p) for p in sorted(fixtures.iterdir()) if p.is_file()]
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        gcc = Path(shutil.which("gcc") or "gcc").resolve()
        cc = Path(shutil.which(args.cc) or args.cc).resolve()
        report["nativeCompilers"] = [{"role": "native oracle/helper", "binary": receipt(gcc), "version": run("gcc-version", [str(gcc), "--version"])},
            {"role": "generated ABI provider", "binary": receipt(cc), "version": run("abi-compiler-version", [str(cc), "--version"])}]
        if args.object is None:
            run("export-loader-state", command)
        elif digest(captured) != OBJECT_SHA256:
            raise RuntimeError("Object reuse requires the exact genuine frozen fixture export.")
        report["object"] = receipt(captured)
        zero = native / "libloader-zero.so"
        run("zero-symbol-build", [str(gcc), "-std=c17", "-fPIC", "-shared", str(fixtures / "native-loader-zero.c"), "-o", str(zero)])
        zero_exports = run("zero-symbol-exports", ["nm", "-D", "--defined-only", str(zero)])
        if [line.split()[-1] for line in zero_exports.splitlines()] != ["native_loader_zero"] or zero_exports.split()[-2] != "A":
            raise RuntimeError("The ABI-only provider must expose exactly one absolute zero-valued symbol.")
        baseline = native / "loader-baseline"
        run("native-baseline-build", [str(gcc), "-std=c17", str(fixtures / "loader-baseline.c"), "-ldl", "-o", str(baseline)])
        baseline_output = run("native-baseline-run", [str(baseline), str(zero)])
        if baseline_output != BASELINE:
            raise RuntimeError("The selected glibc pending/read-clear/NULL-value/errno contract changed.")
        oracle = native / "loader-state"
        run("native-application-build", [str(gcc), "-std=c17", str(fixtures / "loader-state.c"), str(fixtures / "loader-native-oracle.c"), "-ldl", "-o", str(oracle)])
        native_output = run("native-application-run", [str(oracle), str(zero)])
        if native_output != "result=17\n" + "\n".join(f"{i}:1" for i in range(17)):
            raise RuntimeError("Independent native loader application observations changed.")
        helper = native / "libraw-loader.so"
        run("raw-interference-build", [str(gcc), "-std=c17", "-fPIC", "-shared", str(fixtures / "native-loader-interference.c"), "-ldl", "-o", str(helper)])
        helper_exports = run("raw-interference-exports", ["nm", "-D", "--defined-only", str(helper)])
        if exported_functions(helper_exports) != {"native_loader_raw_fail"}:
            raise RuntimeError("Checker-only raw loader interference exceeded its explicit scope.")
        report["nativeEvidence"] = {"baseline": {"binary": receipt(baseline), "output": baseline_output},
            "oracle": {"binary": receipt(oracle), "output": native_output, "role": "Independent native application oracle; never generated dependency"},
            "zeroSymbolProvider": {"binary": receipt(zero), "source": receipt(fixtures / "native-loader-zero.c"), "exports": zero_exports,
                "role": "ABI-only absolute NULL-valued symbol; no executable native application body"},
            "interferenceHelper": {"binary": receipt(helper), "source": receipt(fixtures / "native-loader-interference.c"), "exports": helper_exports,
                "role": "Checker-only raw loader interference; no application binding"}}
        emitter = artifacts / "emitter"
        run("emitter-build", [args.dotnet, "build", str(emitter_project), "-c", "Release", "-o", str(emitter), "--nologo"])
        generated, repeated = artifacts / "generated", artifacts / "repeated"
        emit_args = [args.dotnet, str(emitter / "LoaderEmit.dll"), str(captured)]
        counts = run("emit", [*emit_args, str(generated), str(runtime), str(libc)])
        run("repeat-emit", [*emit_args, str(repeated), str(runtime), str(libc)])
        files = sorted(p.name for p in generated.iterdir())
        if files != sorted(p.name for p in repeated.iterdir()) or any(digest(generated / name) != digest(repeated / name) for name in files):
            raise RuntimeError("Emission was not deterministic.")
        imports = json.loads((generated / "Generated.NativeImports.json").read_text())
        if (imports["nativeErrorProtocol"] != "LinuxX64LogicalErrno" or len(imports["imports"]) != 5
            or {i["linkageName"] for i in imports["imports"]} != LOADER_NAMES | {"__errno_location"}
            or any(i["library"] != str(libc) or i["entryPoint"] != i["linkageName"] or i["isVariadic"] or i["aggregateGateway"]
                or i["callingConvention"] != "cdecl" or i["logicalErrnoProvider"] != (i["linkageName"] == "__errno_location")
                or i["logicalDlerrorProvider"] != (i["linkageName"] == "dlerror") for i in imports["imports"])):
            raise RuntimeError("Application native imports exceeded the exact selected system provider contract.")
        contract = imports["generatedAbi"]
        if (contract["logicalDlerror"]["protocol"] != "linux-x64-glibc-logical-dlerror-v1"
            or len(contract["loaderProviders"]) != 4 or {p["linkageName"] for p in contract["loaderProviders"]} != LOADER_NAMES
            or any(contract[name] for name in ("entries", "aggregateEntries", "aggregateLayouts", "aggregateOutboundShapes", "outboundShapes", "hostEntries"))):
            raise RuntimeError("Native forwarding exceeded the exact loader fixture slice.")
        canonical = re.findall(r'private static readonly nint (\w+) = NativeLibrary.GetExport\((\w+), "([^"]+)"\);', (generated / "Generated.cs").read_text())
        canonical_names = {"dlopen", "dlsym", "dlclose", contract["logicalErrno"]["entry"], contract["logicalDlerror"]["entry"]}
        if len(canonical) != 5 or {entry for _, _, entry in canonical} != canonical_names:
            raise RuntimeError("Canonical first-use address fields duplicated or exposed raw dlerror.")
        c_source = (generated / "Generated.Abi.c").read_text()
        if re.search(r'(?<![\w])dlerror\s*\(', c_source) or "((char *(*)(void))__roslynobj_loader_error)()" not in c_source:
            raise RuntimeError("Loader capture bypassed the exact registered error provider.")
        report["emission"] = {"counts": json.loads(counts), "deterministic": True, "files": [receipt(generated / name) for name in files],
            "nativeImports": imports, "canonicalFields": canonical, "rawErrorProvider": "registered explicitly bound address; no linked-name fallback"}
        options = ["-p:GeneratedProject=" + str(generated / "Generated.csproj"), "-p:RoslynObjAbiCompiler=" + str(cc)]
        checker = artifacts / "checker"
        run("checker-build", [args.dotnet, "build", str(checker_project), "-c", "Release", *options, "-o", str(checker), "--nologo"])
        check_args = [str(zero), str(helper), str(libc)]
        jit = run("checker-jit", [args.dotnet, str(checker / "LoaderChecks.dll"), *check_args])
        if jit != CHECKER_OUTPUT:
            raise RuntimeError("Generated JIT loader observations/interference/identity changed.")
        deps = json.loads((checker / "LoaderChecks.deps.json").read_text())["libraries"]
        if {key.split("/")[0] for key in deps} != {"LoaderChecks", "Generated", "RoslynObj.CSharp.Runtime"}:
            raise RuntimeError("Generated workload acquired reader/SQLite or other unapproved managed dependencies.")
        abi = generated / "libRoslynObj.Generated.Abi.so"
        if digest(abi) != digest(checker / abi.name):
            raise RuntimeError("Generated ABI build output copy changed.")
        exports = run("generated-abi-exports", ["nm", "-D", "--defined-only", str(abi)])
        expected_exports = {contract["logicalErrno"]["entry"], contract["logicalDlerror"]["entry"], contract["logicalDlerror"]["registration"]} | {s["entry"] for s in contract["hostOutboundShapes"]}
        if exported_functions(exports) != expected_exports:
            raise RuntimeError("Generated ABI exports exceeded concrete error/TLS/call forwarding.")
        dependencies = run("generated-abi-dependencies", ["ldd", str(abi)])
        if "sqlite" in dependencies.lower() or "raw-loader" in dependencies:
            raise RuntimeError("Generated ABI acquired an application-oracle dependency.")
        report["generatedAbi"] = {"source": receipt(generated / "Generated.Abi.c"), "binary": receipt(abi),
            "outputCopy": receipt(checker / abi.name), "exports": exports, "dependencies": dependencies}
        built = [emitter / name for name in ("LoaderEmit.dll", "LoaderEmit.deps.json", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        built += [checker / name for name in ("LoaderChecks.dll", "LoaderChecks.deps.json", "Generated.dll", "RoslynObj.CSharp.Runtime.dll")]
        aot = None
        if not args.skip_aot:
            published = artifacts / "nativeaot"
            run("checker-publish", [args.dotnet, "publish", str(checker_project), "-c", "Release", *options, "-r", "linux-x64",
                "--self-contained", "true", "-p:PublishAot=true", "-o", str(published), "--nologo"], 300)
            aot = run("checker-aot", [str(published / "LoaderChecks"), *check_args])
            if aot != CHECKER_OUTPUT or digest(published / abi.name) != digest(abi):
                raise RuntimeError("NativeAOT loader observations/interference or ABI copy changed.")
            report["generatedAbi"]["publishedCopy"] = receipt(published / abi.name)
            report["nativeAotDependencies"] = run("nativeaot-dependencies", ["ldd", str(published / "LoaderChecks")])
            built.append(published / "LoaderChecks")
        report["execution"] = {"jitOutput": jit, "nativeAotOutput": aot, "applicationInvocationsPerChecker": 2,
            "observationsPerInvocation": 17, "generatedDependencies": list(deps),
            "checks": ["success clears unread errors", "NULL value without error", "read-clear", "borrowed bytes survive read-clear/raw/GC",
                "direct/canonical function-address identity", "known selected provider address through unknown indirect gateway", "errno bookkeeping"]}
        report["buildArtifacts"] = [receipt(path) for path in built]
        if ([str(path.relative_to(project)) for path in source_paths(project, fixtures, measured)] != [entry["path"] for entry in implementation]
            or any(digest(project / entry["path"]) != entry["sha256"] for entry in implementation)):
            raise RuntimeError("Source set changed during acceptance; build attribution is incomplete.")
        report["status"] = "passed-partial-contract" if not args.skip_aot else "jit-only-nativeaot-incomplete"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (artifacts / "t3-loader-provider-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
