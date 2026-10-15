#!/usr/bin/env python3
"""Reproduce separately bounded native aggregate gateways on genuine objects.

Coordinator only. Native libraries contain ABI identity/invocation/observation
adapters; all captured application transform bodies execute as generated C#.
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
    fixtures = project / "tests/Fixtures/T2/aggregate-abi"
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
    probe_project = project / "tests/Sequencing/Probe.csproj"
    probe = probe_project.parent / "bin/Release/net10.0/Probe.dll"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    phases: list[dict] = []

    def run(name: str, command: list[str], seconds: int = 120, expected_exit: int = 0) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(2 * 1024**3), "--", *command], cwd=repository, env=environment, check=False)
        recorded = json.loads((phase / "run.json").read_text())
        phases.append(recorded | {"phase": name, "expectedExitCode": expected_exit})
        if recorded["exitCode"] != expected_exit or recorded.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected execution result; see measured phase receipt.")
        return (phase / "stdout.txt").read_text().strip()

    paths = [p for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
             and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts)]
    paths.extend([project / "Directory.Build.props", Path(__file__).resolve(), measured])
    implementation = [{"path": str(p.relative_to(project)), "sha256": digest(p)} for p in sorted(set(paths))]
    report: dict = {"gate": "T2-partial-concrete-native-aggregate-gateways", "status": "incomplete", "phases": phases,
        "nativeApplicationFallback": False, "producerSha256": producer_sha256, "implementation": implementation,
        "fixtures": [], "rejectionProofs": [],
        "limits": ["single-unit fixed Cdecl Linux x64 R(R) / R(R (*)(R), R) signatures; six exact primitive layouts only",
                   "native compiler owns host register/stack/hidden-return placement; no generic aggregate classifier",
                   "EDG retained hidden receiver/result/copy-constructor roles remain unsupported",
                   "atomic/volatile/address-space/UPC, bitfield, nested/array and over-aligned aggregate boundaries unsupported",
                   "multi-unit aggregate boundary views, aggregate varargs and native C++ exception propagation unsupported"]}
    try:
        report["dotnetSdk"] = run("dotnet-version", [args.dotnet, "--version"])
        gcc = Path(shutil.which("gcc") or "gcc").resolve()
        cc = Path(shutil.which("cc") or "cc").resolve()
        report["nativeCompilers"] = [{"executable": receipt(gcc), "version": run("gcc-version", [str(gcc), "--version"])},
                                     {"executable": receipt(cc), "version": run("cc-version", [str(cc), "--version"])}]
        run("build-projection", [args.dotnet, "build", str(runner_project), "-c", "Release", "--nologo"])
        run("build-public-probe", [args.dotnet, "build", str(probe_project), "-c", "Release", "--nologo"])
        provider = native_dir / "libaggregate-abi-adapter.so"
        run("provider-build", [str(gcc), "-std=c17", "-shared", "-fPIC", str(fixtures / "native-adapters.c"), "-o", str(provider)])
        provider_exports = run("provider-exports", ["nm", "-D", "--defined-only", str(provider)])
        actual_provider_exports = {line.split()[-1] for line in provider_exports.splitlines() if len(line.split()) >= 3 and line.split()[-2] in {"T", "W"}}
        expected_provider_exports = {"native_" + prefix + "_" + suffix for prefix in ("integer", "sse", "mixed", "memory", "packed", "unionword")
            for suffix in ("echo", "invoke", "provider", "echo_callback", "bits" if prefix == "unionword" else "field")}
        if actual_provider_exports != expected_provider_exports:
            raise RuntimeError("Native fixture provider exports exceeded its explicit ABI-only adapters.")
        report["nativeProvider"] = {"binary": receipt(provider), "source": receipt(fixtures / "native-adapters.c"),
            "exports": provider_exports,
            "role": "ABI-only identity, pointer identity, callback invocation and physical storage observation"}

        def object_path(name: str) -> Path:
            target = args.objects.resolve() / (name + ".roslynobj") if args.objects else artifacts / (name + ".roslynobj")
            if not args.objects:
                run(name + "-export", [sys.executable, str(driver), "--gcc", "--c17", "--gnu_version", "130300", str(fixtures / (name + ".c")), "-o", str(target)])
            return target

        for name, prefix, shape, expected in (
            ("integer", "integer", "integer-pair", 36), ("sse", "sse", "sse-pair", 94),
            ("mixed", "mixed", "mixed-pair", 76), ("memory", "memory", "memory-triple", 161),
            ("packed", "packed", "packed-byte-double", 364), ("union", "unionword", "union-uint64-double", 11),
        ):
            source = fixtures / (name + ".c")
            captured = object_path(name)
            native = native_dir / name
            run(name + "-native-build", [str(gcc), "-std=c17", str(source), str(fixtures / "native-adapters.c"), str(fixtures / "native-oracle.c"), "-o", str(native)])
            native_output = run(name + "-native-run", [str(native)])
            lines = native_output.splitlines()
            if lines[0] != "result=" + str(expected):
                raise RuntimeError(name + ": independent native fixture result changed.")
            count = int(lines[1].split("=", 1)[1])
            observations = [int(line.split(":", 1)[1]) for line in lines[2:]]
            if len(observations) != count or [line.split(":", 1)[0] for line in lines[2:]] != [str(i) for i in range(count)]:
                raise RuntimeError(name + ": native observation indexing changed.")
            facts = json.loads(run(name + "-public-facts", [args.dotnet, str(probe), str(captured)]))
            transform = next(f for f in facts["Signatures"] if f["Name"] == "transform")
            if not transform["HasPrototype"] or transform["HasThisParameter"] or transform["ReturnsValueAsParameter"] or transform["ReturnsValueViaCopyConstructor"] or any(
                    p["Role"] != "ordinary_parameter" or p["PassedViaCopyConstructor"] for p in transform["SelectedSignatureParameters"]):
                raise RuntimeError(name + ": selected execution signature no longer has ordinary value roles.")
            if any(call["HasReturnSlot"] or call["HasReceiver"] for f in facts["Functions"] for call in f["Calls"]) or any(f["Temporaries"] for f in facts["Functions"]):
                raise RuntimeError(name + ": selected call graph acquired an unaccepted hidden slot/temporary.")
            run(name + "-public-storage", [args.dotnet, str(runner), "inspect-storage", str(captured)])
            bindings = artifacts / (name + "-bindings.json")
            symbols = ["native_" + prefix + "_" + suffix for suffix in ("echo", "invoke", "provider", "echo_callback", "bits" if name == "union" else "field")]
            bindings.write_text(json.dumps([{"LinkageName": symbol, "LibraryPath": str(provider), "EntryPoint": symbol} for symbol in symbols], indent=2) + "\n")
            generated, repeated = artifacts / (name + "-generated"), artifacts / (name + "-repeated")
            counts = run(name + "-emit", [args.dotnet, str(runner), "emit", str(captured), str(generated), str(runtime), str(bindings)])
            run(name + "-repeat-emit", [args.dotnet, str(runner), "emit", str(captured), str(repeated), str(runtime), str(bindings)])
            files = sorted(p.name for p in generated.iterdir())
            if files != sorted(p.name for p in repeated.iterdir()) or any(digest(generated / p) != digest(repeated / p) for p in files):
                raise RuntimeError(name + ": emission was not deterministic.")
            imports = json.loads((generated / "Generated.NativeImports.json").read_text())
            captured_imports = imports["imports"]
            if len(captured_imports) != len(symbols) or {i["linkageName"] for i in captured_imports} != set(symbols) or any(
                    i["library"] != str(provider) or i["entryPoint"] != i["linkageName"] or i["callingConvention"] != "cdecl" or i["isVariadic"] or
                    i["aggregateGateway"] != (i["linkageName"] in {"native_" + prefix + "_echo", "native_" + prefix + "_invoke"}) for i in captured_imports):
                raise RuntimeError(name + ": native imports exceeded the exact ABI fixture provider.")
            contract = imports["generatedAbi"]
            if contract["entries"] or contract["outboundShapes"] or len(contract["aggregateEntries"]) != 1 or len(contract["aggregateLayouts"]) != 1 or contract["aggregateLayouts"][0]["shape"] != shape:
                raise RuntimeError(name + ": generated native forwarding exceeded the concrete aggregate case.")
            if any(s["selectedReturnSlot"] or s["selectedCopyConstructorReturn"] or any(r != "ordinary_parameter" for r in s["parameterRoles"]) for s in contract["aggregateOutboundShapes"]):
                raise RuntimeError(name + ": generated forwarding changed selected execution roles.")
            checker_name = "IntegerChecks" if name == "integer" else "ShapeChecks"
            checker = project / "tests/AggregateAbi" / (checker_name + ".csproj")
            options = ["-p:GeneratedProject=" + str(generated / "Generated.csproj"), "-p:RoslynObjAbiCompiler=" + str(cc)]
            if name != "integer": options.append("-p:AggregateShape=" + name.upper())
            output = artifacts / (name + "-checker")
            run(name + "-checker-build", [args.dotnet, "build", str(checker), "-c", "Release", *options, "-o", str(output), "--nologo"])
            jit = run(name + "-checker-jit", [args.dotnet, str(output / (checker_name + ".dll")), str(provider)])
            expected_output = "result=" + str(expected) + "\nobservations=" + ",".join(map(str, observations)) + "\ncanonical-identity=passed\nphysical-layout=passed"
            if jit != expected_output: raise RuntimeError(name + ": generated JIT observations/physical checks changed.")
            deps = json.loads((output / (checker_name + ".deps.json")).read_text())["libraries"]
            if any(p.split("/")[0] not in {checker_name, "Generated", "RoslynObj.CSharp.Runtime"} for p in deps):
                raise RuntimeError("Generated workload acquired a reader/SQLite or other unapproved managed dependency.")
            abi_library = generated / "libRoslynObj.Generated.Abi.so"
            output_copy = output / abi_library.name
            if digest(abi_library) != digest(output_copy): raise RuntimeError("Generated ABI output copy changed.")
            exports = run(name + "-abi-exports", ["nm", "-D", "--defined-only", str(abi_library)])
            actual_exports = {line.split()[-1] for line in exports.splitlines() if len(line.split()) >= 3 and line.split()[-2] in {"T", "W"}}
            expected_exports = {e[field] for e in contract["aggregateEntries"] for field in ("entry", "registration")} | {e["entry"] for e in contract["aggregateOutboundShapes"]}
            if actual_exports != expected_exports: raise RuntimeError("Generated native ABI exports exceeded the concrete forwarding contract.")
            abi = {"source": receipt(generated / "Generated.Abi.c"), "binary": receipt(abi_library), "outputCopy": receipt(output_copy), "exports": exports,
                   "dependencies": run(name + "-abi-dependencies", ["ldd", str(abi_library)]), "contract": contract}
            built = [output / (checker_name + ".dll"), output / "Generated.dll", output / "RoslynObj.CSharp.Runtime.dll", output / (checker_name + ".deps.json"), native]
            aot, aot_dependencies = None, None
            if not args.skip_aot:
                published = artifacts / (name + "-nativeaot")
                run(name + "-checker-publish", [args.dotnet, "publish", str(checker), "-c", "Release", *options, "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", str(published), "--nologo"], 300)
                aot = run(name + "-checker-aot", [str(published / checker_name), str(provider)])
                aot_dependencies = run(name + "-aot-dependencies", ["ldd", str(published / checker_name)])
                if aot != expected_output: raise RuntimeError(name + ": NativeAOT observations/physical checks changed.")
                copied = published / abi_library.name
                if digest(copied) != digest(abi_library): raise RuntimeError("NativeAOT ABI provider copy changed.")
                abi["publishedCopy"] = receipt(copied)
                built.append(published / checker_name)
            report["fixtures"].append({"name": name, "source": receipt(source), "input": receipt(captured), "selectedSignature": transform,
                "nativeOutput": native_output, "jitOutput": jit, "nativeAotOutput": aot, "checkerInvocations": 2, "emissionCounts": counts,
                "generatedDependencies": list(deps), "nativeAotSystemDependencies": aot_dependencies, "deterministic": True, "nativeImports": imports,
                "generatedAbi": abi, "generatedFiles": [receipt(generated / p) for p in files], "builtArtifacts": [receipt(p) for p in built]})

        for name, adapter_source, symbol, expected, message in (
            ("overaligned", "native-negative-adapters.c", "native_overaligned_echo", 7, "alignment at most eight"),
            ("bitfields", "native-negative-adapters.c", "native_bitfields_echo", -2, "does not support bitfields"),
            ("atomic-field", "native-atomic-adapter.c", "native_atomic_echo", 11, "Atomic/UPC aggregate ABI storage"),
        ):
            source, captured = fixtures / (name + ".c"), object_path(name)
            native, library = native_dir / name, native_dir / ("lib" + name + "-adapter.so")
            run(name + "-native-build", [str(gcc), "-std=c17", str(source), str(fixtures / adapter_source), str(fixtures / "native-oracle.c"), "-o", str(native)])
            native_output = run(name + "-native-run", [str(native)])
            if not native_output.startswith("result=" + str(expected) + "\n"): raise RuntimeError(name + ": native rejection candidate changed.")
            run(name + "-provider-build", [str(gcc), "-std=c17", "-shared", "-fPIC", str(fixtures / adapter_source), "-o", str(library)])
            bindings = artifacts / (name + "-bindings.json")
            bindings.write_text(json.dumps([{"LinkageName": symbol, "LibraryPath": str(library), "EntryPoint": symbol}], indent=2) + "\n")
            rejected = artifacts / (name + "-must-not-publish")
            diagnostics = run(name + "-rejection", [args.dotnet, str(runner), "emit", str(captured), str(rejected), str(runtime), str(bindings)], expected_exit=1)
            if rejected.exists() or message not in diagnostics: raise RuntimeError(name + ": unaccepted representation was not rejected before publication.")
            report["rejectionProofs"].append({"source": receipt(source), "input": receipt(captured), "native": receipt(native),
                "provider": receipt(library), "nativeOutput": native_output, "diagnostics": diagnostics, "published": False})
        report["inputs"] = [receipt(p) for p in [producer, driver, *sorted(p for p in fixtures.iterdir() if p.is_file())]]
        report["buildArtifacts"] = [receipt(p) for p in [runner, runner.parent / "RoslynObj.CSharp.dll", runner.parent / "RoslynObj.dll",
            probe, runtime.parent / "bin/Release/net10.0/RoslynObj.CSharp.Runtime.dll"]]
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
        (artifacts / "t2-aggregate-abi-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
