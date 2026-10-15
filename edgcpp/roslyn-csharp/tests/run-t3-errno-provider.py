#!/usr/bin/env python3
"""Reproduce the bounded Linux x64 logical-errno provider on a genuine object.

Coordinator only: exports/builds are serialized with the other acceptance runs.
The native reference is an independent oracle. The interference helper is not an
application provider; generated native code contains ABI/error/TLS forwarding.
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

ARTIFACT_ROOT = Path("/mnt/storage/roslynobj-csharp-20261015")
PRODUCER_SHA256 = "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4"
EXPORT_RECIPE_SHA256 = "5245f79e783a4821ad1c5f76dda3926ba8501dd8aeb272d5de4d6158624f6991"
FIXTURE_SHA256 = "4e87daf838fc51b1322d39379b4e48b7c89462bf6ab10cfb0c4f8ba2a30ed1a9"
REUSED_OBJECT_SHA256 = "4947423ad150b67d75c1860118642c5973bededf3dc324295aaf8675f3168979"
LIBC_SHA256 = "3a15d66867d83762c7f2f1e37359cb8f6c5743edb369c65285cb0b1c4f7498bf"
AUDIT_SHA256 = "c41c292517d8451118a92d598f4bc02bd5993f92706e1483870a05fb7022262a"
SQLITE_SHA256 = "e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c"
EXPECTED_OBSERVATIONS = [1201, 1201, -1, 20, 1, 555, 777, 777, 444, 333, 2, 9,
                         0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 901, 1]
EXPECTED_TERMINALS = {"ordinary-int32-call-argument": 10, "int32-local-initializer-value": 3,
                      "int32-equality-value": 7, "int32-assignment-rhs-value": 5,
                      "int32-inequality-value": 2, "int32-return-value": 1}
EXPECTED_IMPORTS = {"__errno_location", "open", "getpid", "qsort", "pthread_create", "pthread_join",
                    "pthread_mutexattr_init", "pthread_mutexattr_settype", "pthread_mutex_init",
                    "pthread_mutex_lock", "pthread_mutex_trylock", "pthread_mutex_unlock",
                    "pthread_mutex_destroy", "pthread_mutexattr_destroy"}


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def exported_functions(output: str) -> set[str]:
    return {fields[-1] for line in output.splitlines() if len(fields := line.split()) >= 3
            and fields[-2] in {"T", "W"}}


def implementation_paths(project: Path, fixtures: Path, measured: Path) -> list[Path]:
    paths = {p for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
             and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)}
    paths.update(p for p in fixtures.iterdir() if p.is_file())
    paths.update([project / "Directory.Build.props", Path(__file__).resolve(), measured])
    return sorted(paths)


def audit_selected_errno_use(path: Path) -> dict:
    if digest(path) != AUDIT_SHA256:
        raise RuntimeError("Expected the immutable, refined public errno-use audit2.")
    audit = json.loads(path.read_text())
    if (audit["input"]["sha256"] != SQLITE_SHA256 or audit["declaration"]["linkageName"] != "__errno_location"
            or audit["callCount"] != 28 or audit["referenceCount"] != 28 or audit["noncalleeReferences"] != 0
            or audit["terminalCounts"] != EXPECTED_TERMINALS or audit["initializerRootOverlapOccurrences"] != 3
            or audit["bodyPlusExplicitInitializerRootOccurrences"] != 31
            or not audit["everyCallImmediatelyProducesSigned32Prvalue"]
            or not audit["everyDereferenceHasWhitelistedTerminalRead"]
            or not audit["selectedPatternHasNoPointerEscape"] or audit["dereferenceAddressTakingObserved"]):
        raise RuntimeError("The exact retained errno terminal-use pattern changed.")
    return {"receipt": receipt(path), "input": audit["input"], "uniqueCalls": 28, "uniqueCalleeReferences": 28,
            "terminalCounts": audit["terminalCounts"], "initializerRootOverlapOccurrences": 3,
            "bodyPlusExplicitInitializerRootOccurrences": 31, "scope": audit["scope"],
            "occurrenceExplanation": audit["occurrenceExplanation"],
            "proofSource": "Immutable public reader audit; no source parsing or general alias-analysis claim"}


def audit_imports(generated: Path, libc: Path) -> dict:
    imports = json.loads((generated / "Generated.NativeImports.json").read_text())
    entries = imports["imports"]
    if (imports["nativeErrorProtocol"] != "LinuxX64LogicalErrno" or len(entries) != 14
            or {i["linkageName"] for i in entries} != EXPECTED_IMPORTS
            or any(i["library"] != str(libc) or i["entryPoint"] != i["linkageName"]
                   or i["callingConvention"] != "cdecl" or i["aggregateGateway"]
                   or i["isVariadic"] != (i["linkageName"] == "open")
                   or i["logicalErrnoProvider"] != (i["linkageName"] == "__errno_location") for i in entries)):
        raise RuntimeError("Native imports exceeded the explicit fixture provider contract.")
    contract = imports["generatedAbi"]
    if (contract["logicalErrno"]["protocol"] != "linux-x64-logical-errno-v1"
            or contract["logicalErrno"]["entry"] != "__roslynobj_application_errno_location"
            or {e["linkageName"] for e in contract["hostEntries"]} != {"inner_compare", "outer_compare", "worker"}
            or len(contract["hostEntries"]) != 3 or len(contract["hostOutboundShapes"]) != 14
            or contract["entries"] or contract["aggregateEntries"] or contract["aggregateLayouts"]
            or contract["aggregateOutboundShapes"] or len(contract["outboundShapes"]) != 1
            or contract["outboundShapes"][0]["fixedParameterCount"] != 2
            or contract["outboundShapes"][0]["arguments"] != ["sbyte*", "int"]):
        raise RuntimeError("Generated native forwarding exceeded the exercised scalar/pointer/open shapes.")
    return imports


def audit_canonical_entries(generated: Path, contract: dict) -> dict:
    text = (generated / "Generated.cs").read_text()
    addresses = re.findall(r'private static readonly nint (\w+) = NativeLibrary.GetExport\((\w+), "([^"]+)"\);', text)
    expected = (EXPECTED_IMPORTS - {"__errno_location"}) | {contract["logicalErrno"]["entry"]} | {
        e["entry"] for e in contract["hostEntries"]}
    if {symbol for _, _, symbol in addresses} != expected or len(addresses) != len(expected):
        raise RuntimeError("A first-use imported address was duplicated or escaped the logical provider.")
    fields = {symbol: field for field, _, symbol in addresses}
    for storage, symbol in (("errno_address", contract["logicalErrno"]["entry"]), ("process_id", "getpid")):
        expected_fixup = f'*(nint*)((byte*)global::Translated.Program.{storage}) = global::Translated.Program.{fields[symbol]};'
        if text.count(expected_fixup) != 1:
            raise RuntimeError("The imported-address-first global fixup lost its canonical entry.")
    if '__errno_location")' in text or 'EntryPoint = "__errno_location"' in text:
        raise RuntimeError("Generated application code exposed the raw libc errno accessor.")
    return {"firstUseConstantAddresses": ["errno_address", "process_id"],
            "exactlyOneCanonicalFieldPerEntry": True, "rawLibcErrnoAddressExposed": False,
            "logicalAccessor": contract["logicalErrno"], "fields": [
                {"field": field, "libraryField": library, "entryPoint": symbol} for field, library, symbol in addresses]}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--object", type=Path, help="Reuse the genuine fixture export instead of exporting again")
    parser.add_argument("--export-recipe", type=Path, default=ARTIFACT_ROOT / "t3/providers/export-errno-thread-2/run.json")
    parser.add_argument("--errno-use-audit", type=Path, default=ARTIFACT_ROOT / "t3/errno-public-use-audit-2.json")
    parser.add_argument("--libc", type=Path, default=Path("/usr/lib/x86_64-linux-gnu/libc.so.6"))
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--cc", default="cc")
    parser.add_argument("--skip-aot", action="store_true")
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    fixtures = project / "tests/Fixtures/T3/providers"
    measured = project / "tools/run-measured.py"
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    native = artifacts / "native"
    native.mkdir()
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer") / (PRODUCER_SHA256 + "-roslyn-cpfe")
    driver = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
    emitter_project = project / "tests/NativeProviders/Emit.csproj"
    checker_project = project / "tests/NativeProviders/ErrnoChecks.csproj"
    libc = args.libc.resolve()
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    phases: list[dict] = []
    implementation = [{"path": str(p.relative_to(project)), "sha256": digest(p)}
                      for p in implementation_paths(project, fixtures, measured)]
    report = {"gate": "T3-partial-linux-x64-logical-errno", "status": "incomplete", "phases": phases,
              "nativeApplicationFallback": False, "implementation": implementation,
              "limits": ["Explicit Linux x64/glibc logical errno substitution; not raw libc slot/address identity",
                         "This receipt exercises fourteen exact imports, not all 88 SQLite native requirements",
                         "Incoming scalar/pointer qsort/pthread callbacks and two-fixed-argument open gateway only",
                         "Protocol code also protects enabled unknown variadic/aggregate gateways; this fixture does not establish those combinations",
                         "dlerror snapshot/read-clear, loader/buffer ownership and the remaining native contracts are open",
                         "No general errno alias analysis, nonlocal jumps, cancellation or native C++ exception propagation claim"]}

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
        if digest(producer) != PRODUCER_SHA256 or digest(libc) != LIBC_SHA256:
            raise RuntimeError("The frozen producer or explicit libc provider identity changed.")
        report["errnoUseAudit"] = audit_selected_errno_use(args.errno_use_audit.resolve())
        if digest(args.export_recipe) != EXPORT_RECIPE_SHA256 or digest(fixtures / "errno-thread.c") != FIXTURE_SHA256:
            raise RuntimeError("The frozen export recipe/application fixture identity changed.")
        recipe = json.loads(args.export_recipe.read_text())
        command = recipe["command"]
        if (recipe["exitCode"] != 0 or recipe.get("budgetViolation") is not None or len(command) < 4
                or command[-2] != "-o" or Path(command[-3]).name != "errno-thread.c"
                or Path(command[1]).name != "roslyn-cpfe.py"
                or any("SQLITE" in flag or "timing" in flag for flag in command[2:-3])):
            raise RuntimeError("Expected the successful, SQLite-define-free frozen host export recipe.")
        captured = args.object.resolve() if args.object else artifacts / "errno-thread.roslynobj"
        export_command = [sys.executable, str(driver), *command[2:-3], str(fixtures / "errno-thread.c"), "-o", str(captured)]
        report["export"] = {"recipe": receipt(args.export_recipe.resolve()), "frozenHostFlags": command[2:-3],
                            "command": export_command, "reusedObject": args.object is not None,
                            "scope": "Recorded GNU 13.3 host predefines/include paths; no SQLite defines or timing flags"}
        report["inputs"] = [receipt(producer), receipt(driver), receipt(libc)] + [receipt(p) for p in sorted(fixtures.iterdir()) if p.is_file()]
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        gcc = Path(shutil.which("gcc") or "gcc").resolve()
        cc = Path(shutil.which(args.cc) or args.cc).resolve()
        report["nativeCompilers"] = [{"role": "independent native oracle/helper", "executable": receipt(gcc),
                                      "version": run("gcc-version", [str(gcc), "--version"])},
                                     {"role": "generated ABI gateway", "executable": receipt(cc),
                                      "version": run("abi-compiler-version", [str(cc), "--version"])}]
        if args.object is None:
            run("export-errno-thread", export_command)
        if args.object is not None and digest(captured) != REUSED_OBJECT_SHA256:
            raise RuntimeError("Object reuse requires the exact genuine fixture exported by the frozen recipe.")
        report["object"] = receipt(captured)
        oracle = native / "errno-thread"
        run("native-oracle-build", [str(gcc), "-std=c17", "-pthread", str(fixtures / "errno-thread.c"),
                                    str(fixtures / "native-oracle.c"), "-o", str(oracle)])
        native_output = run("native-oracle-run", [str(oracle)])
        lines = native_output.splitlines()
        expected_native = ["result=6223", "observations=24"] + [f"{i}:{value}" for i, value in enumerate(EXPECTED_OBSERVATIONS)]
        if lines != expected_native:
            raise RuntimeError("Independent native result/observation sequence changed.")
        helper = native / "libraw-errno.so"
        run("interference-helper-build", [str(gcc), "-std=c17", "-fPIC", "-shared",
                                          str(fixtures / "native-raw-errno.c"), "-o", str(helper)])
        helper_exports = run("interference-helper-exports", ["nm", "-D", "--defined-only", str(helper)])
        if exported_functions(helper_exports) != {"raw_errno_write", "raw_errno_read", "raw_errno_address"}:
            raise RuntimeError("Raw-interference helper exceeded its explicit native test scope.")
        report["nativeOracle"] = {"source": receipt(fixtures / "errno-thread.c"), "harness": receipt(fixtures / "native-oracle.c"),
                                  "binary": receipt(oracle), "output": native_output, "role": "Separate native semantic oracle; never a generated dependency"}
        report["interferenceHelper"] = {"source": receipt(fixtures / "native-raw-errno.c"), "binary": receipt(helper),
                                        "exports": helper_exports, "role": "Checker-only raw TLS interference; no application binding/body"}
        emitter_dir = artifacts / "emitter"
        run("build-emitter", [args.dotnet, "build", str(emitter_project), "-c", "Release", "-o", str(emitter_dir), "--nologo"])
        generated, repeated = artifacts / "generated", artifacts / "repeated"
        emitter_args = [args.dotnet, str(emitter_dir / "Emit.dll"), str(captured)]
        counts = run("emit", [*emitter_args, str(generated), str(runtime), str(libc)])
        run("repeat-emit", [*emitter_args, str(repeated), str(runtime), str(libc)])
        files = sorted(p.name for p in generated.iterdir())
        if files != sorted(p.name for p in repeated.iterdir()) or any(digest(generated / p) != digest(repeated / p) for p in files):
            raise RuntimeError("Emission was not deterministic.")
        report["emission"] = {"counts": json.loads(counts), "deterministic": True,
                              "files": [receipt(generated / name) for name in files]}
        imports = audit_imports(generated, libc)
        contract = imports["generatedAbi"]
        report["nativeImports"] = imports
        report["canonicalAddressAudit"] = audit_canonical_entries(generated, contract)
        c_source = (generated / "Generated.Abi.c").read_text()
        layouts = {"pthread_mutexattr_t": (4, 4), "pthread_mutex_t": (40, 8), "pthread_t": (8, 8), "pthread_attr_t": (56, 8)}
        for token, (size, alignment) in layouts.items():
            for check in (f"_Static_assert(sizeof({token}) == {size},", f"_Static_assert(_Alignof({token}) == {alignment},"):
                if check not in c_source:
                    raise RuntimeError("A captured native provider storage layout check was omitted.")
        report["selectedNativeLayouts"] = {"sizeAlignment": {k: {"bytes": v[0], "alignmentBytes": v[1]} for k, v in layouts.items()},
            "compiledChecks": [line for line in c_source.splitlines() if line.startswith("_Static_assert")],
            "scope": "Retained layouts/member offsets checked against native headers; generated recursive-mutex/thread workflow exercises actual storage"}
        checker_dir = artifacts / "checker"
        build_options = ["-p:GeneratedProject=" + str(generated / "Generated.csproj"), "-p:RoslynObjAbiCompiler=" + str(cc)]
        run("checker-build", [args.dotnet, "build", str(checker_project), "-c", "Release", *build_options, "-o", str(checker_dir), "--nologo"])
        expected_output = "result=6223\nobservations=" + ",".join(map(str, EXPECTED_OBSERVATIONS)) + "\nlogical-errno-interference=passed\nnested-qsort-foreign-pthread=passed"
        jit = run("checker-jit", [args.dotnet, str(checker_dir / "ErrnoChecks.dll"), str(helper)])
        if jit != expected_output:
            raise RuntimeError("Generated JIT errno/callback/thread observations changed.")
        deps = json.loads((checker_dir / "ErrnoChecks.deps.json").read_text())["libraries"]
        if {key.split("/")[0] for key in deps} != {"ErrnoChecks", "Generated", "RoslynObj.CSharp.Runtime"}:
            raise RuntimeError("Generated workload acquired a reader/SQLite or other unapproved managed dependency.")
        abi = generated / "libRoslynObj.Generated.Abi.so"
        copied = checker_dir / abi.name
        if digest(abi) != digest(copied):
            raise RuntimeError("The generated native ABI output copy changed.")
        abi_exports = run("generated-abi-exports", ["nm", "-D", "--defined-only", str(abi)])
        expected_exports = {contract["logicalErrno"]["entry"]} | {
            entry[field] for entry in contract["hostEntries"] for field in ("entry", "registration")} | {
            entry["entry"] for entry in contract["hostOutboundShapes"] + contract["outboundShapes"]}
        if exported_functions(abi_exports) != expected_exports:
            raise RuntimeError("Generated native exports exceeded exact TLS/error/ABI forwarding.")
        abi_dependencies = run("generated-abi-dependencies", ["ldd", str(abi)])
        if "sqlite" in abi_dependencies.lower() or "libraw-errno" in abi_dependencies:
            raise RuntimeError("Generated native ABI acquired a test-oracle/application dependency.")
        report["generatedAbi"] = {"source": receipt(generated / "Generated.Abi.c"), "binary": receipt(abi),
            "outputCopy": receipt(copied), "exports": abi_exports, "dependencies": abi_dependencies,
            "role": "Concrete typed call forwarding, native logical TLS and immediate native errno synchronization; no application algorithms"}
        built = [emitter_dir / name for name in ("Emit.dll", "Emit.deps.json", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        built += [checker_dir / name for name in ("ErrnoChecks.dll", "Generated.dll", "RoslynObj.CSharp.Runtime.dll", "ErrnoChecks.deps.json")]
        aot_output = None
        if not args.skip_aot:
            published = artifacts / "nativeaot"
            run("checker-publish", [args.dotnet, "publish", str(checker_project), "-c", "Release", *build_options,
                                    "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(published), "--nologo"], 300)
            aot_output = run("checker-aot", [str(published / "ErrnoChecks"), str(helper)])
            if aot_output != expected_output or digest(published / abi.name) != digest(abi):
                raise RuntimeError("NativeAOT errno/callback/thread observations or ABI copy changed.")
            report["generatedAbi"]["publishedCopy"] = receipt(published / abi.name)
            report["nativeAotDependencies"] = run("nativeaot-dependencies", ["ldd", str(published / "ErrnoChecks")])
            built.append(published / "ErrnoChecks")
        report["execution"] = {"nativeOutput": native_output, "jitOutput": jit, "nativeAotOutput": aot_output,
                               "applicationInvocationsPerChecker": 2, "observationsPerInvocation": 24,
                               "additionalChecks": ["logical/raw TLS address distinction", "stable direct/indirect logical accessor",
                                   "application pointer stores", "unrelated raw overwrite and CLR allocation/GC",
                                   "success leaving errno unchanged", "immediate failure snapshot", "nested qsort error propagation",
                                   "foreign pthread initial value, TLS separation and join completion"]}
        report["buildArtifacts"] = [receipt(p) for p in built]
        current = implementation_paths(project, fixtures, measured)
        if [str(p.relative_to(project)) for p in current] != [entry["path"] for entry in implementation] or any(
                digest(project / entry["path"]) != entry["sha256"] for entry in implementation):
            raise RuntimeError("Source set changed during acceptance; build attribution is incomplete.")
        if digest(libc) != LIBC_SHA256 or digest(args.errno_use_audit.resolve()) != AUDIT_SHA256:
            raise RuntimeError("The native provider/public use audit changed during acceptance.")
        report["status"] = "passed-partial-contract" if not args.skip_aot else "jit-only-nativeaot-incomplete"
    except Exception as error:
        report["error"] = str(error)
        raise
    finally:
        (artifacts / "t3-errno-provider-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
