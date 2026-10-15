#!/usr/bin/env python3
"""Coordinator-only bounded recorded intrinsic/native/JIT/NativeAOT receipt.

Generated execution has no native application or provider fallback. The native
application is an independent reference, never linked into generated output.
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


def record(path: Path) -> dict:
    return {"path": str(path), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--aot-cc", default="clang-18")
    parser.add_argument("--skip-aot", action="store_true", help="Record an explicitly incomplete JIT-only receipt.")
    args = parser.parse_args()
    tests = Path(__file__).resolve().parent.parent
    project = tests.parent
    repository = project.parents[1]
    fixtures = tests / "Fixtures/T3/intrinsics"
    reader = repository / "edgcpp/roslyn/managed/RoslynObj"
    measured = project / "tools/run-measured.py"
    exporter = repository / "edgcpp/roslyn/build/roslyn-cpfe.py"
    producer = Path("/mnt/storage/roslynobj-duckdb-20261014/export/major2/producer/cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4-roslyn-cpfe")
    producer_hash = "cf619053cbedabca46da3eab3e8278de43fa4a25ef770a56572454abf9ff25d4"
    if digest(producer) != producer_hash:
        raise RuntimeError("The frozen genuine producer identity changed.")
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    objects = artifacts / "export"
    objects.mkdir()
    environment = os.environ.copy()
    environment["ROSLYNOBJ_COMPILER"] = str(producer)
    environment.pop("LD_LIBRARY_PATH", None)
    environment.pop("LD_PRELOAD", None)
    source_roots = [project / "src/RoslynObj.CSharp", project / "src/RoslynObj.CSharp.Runtime", reader,
                    tests / "Projection", tests / "IntrinsicFacts", tests / "Intrinsics", fixtures]

    def source_paths() -> set[Path]:
        paths = {path for folder in source_roots for path in folder.rglob("*")
                 if path.is_file() and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".c", ".h", ".md"}
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
        "gate": "T3-partial-fifteen-recorded-intrinsic-families", "status": "incomplete", "phases": phases,
        "implementation": implementation, "producer": record(producer), "inputs": [], "rejections": [],
        "nativeApplicationFallback": False, "nativeProviders": [], "nativeErrorProtocol": "None; no native imports",
        "limits": ["Linux x64 natural primitive addresses; runtime checks actual target alignment",
                   "direct public-object emission proof; production CLI intrinsic admission is not established by this receipt",
                   "captured constant relaxed memory-order0 only; stronger sequentially consistent implementation",
                   "atomic load1/2/4 and store1/2/4/8; full sync fence; bswap16/32/64; defined nonzero clzll",
                   "signed64 + signed64 -> signed64 output overflow only; bool8 result normalized",
                   "no mixed/unsigned/narrow/128-bit overflow, other atomic orders, atomic-qualified storage or intrinsic addresses",
                   "finite1000value cross-thread flag/payload experiment is not a general native thread/provider implementation",
                   "no SQLite execution or complete language/provider acceptance"],
    }

    def run(name: str, command: list, expected: int = 0, seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(4 * 1024**3), "--", *map(str, command)],
                       cwd=repository, env=environment, check=False)
        result = json.loads((phase / "run.json").read_text())
        phases.append(result | {"phase": name, "expectedExitCode": expected})
        if result["exitCode"] != expected or result.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected result; inspect preserved measured phase logs.")
        return (phase / "stdout.txt").read_text().strip()

    def tool(name: str) -> Path:
        path = Path(shutil.which(name) or name).resolve()
        if not path.is_file():
            raise RuntimeError("Missing selected tool: " + name)
        return path

    def deps(path: Path, checker: bool) -> dict:
        libraries = json.loads(path.read_text())["libraries"]
        expected = {"Generated", "RoslynObj.CSharp.Runtime"} | ({"Checks"} if checker else set())
        if {name.split("/")[0] for name in libraries} != expected:
            raise RuntimeError("Generated execution gained a reader/SQLite/application managed dependency.")
        return {"file": record(path), "libraries": list(libraries)}

    def generated_sources(folder: Path) -> dict:
        files = {path.name: digest(path) for path in folder.iterdir() if path.is_file()}
        if set(files) != {"Generated.cs", "Generated.csproj"}:
            raise RuntimeError("Intrinsic-only output gained a native import/gateway or unexpected file.")
        text = (folder / "Generated.cs").read_text()
        if "DllImport" in text or "NativeLibrary" in text:
            raise RuntimeError("Recorded intrinsic acquired a native symbol fallback.")
        for method in ("AtomicLoad1", "AtomicLoad2", "AtomicLoad4", "AtomicStore1", "AtomicStore2", "AtomicStore4", "AtomicStore8",
                       "Synchronize", "ByteSwap16", "ByteSwap32", "ByteSwap64", "CountLeadingZeros64", "AddOverflow64", "SubtractOverflow64", "MultiplyOverflow64"):
            if "TargetIntrinsics." + method not in text:
                raise RuntimeError("A required recorded intrinsic family did not activate: " + method)
        return files

    try:
        dotnet, gcc, aot_cc = tool(args.dotnet), tool("gcc"), tool(args.aot_cc)
        report["toolchain"] = {"dotnet": record(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
            "sdkInfo": run("dotnet-info", [dotnet, "--info"]), "nativeCompiler": record(gcc), "nativeVersion": run("gcc-version", [gcc, "--version"]),
            "aotNativeCompiler": record(aot_cc), "aotNativeVersion": run("aot-cc-version", [aot_cc, "--version"])}
        sdk_version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["toolchain"]["managedCompilerFiles"] = [record(sdk / name) for name in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]
        runtime_project = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
        projection_bin = artifacts / "projection"
        probe_bin = artifacts / "public-probe"
        run("build-projection", [dotnet, "build", tests / "Projection/Projection.csproj", "-c", "Release", "--nologo", "-o", projection_bin])
        run("build-public-probe", [dotnet, "build", tests / "IntrinsicFacts/IntrinsicFacts.csproj", "-c", "Release", "--nologo", "-o", probe_bin])
        report["build"] = [record(projection_bin / name) for name in ("Projection.dll", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        report["build"].append(record(probe_bin / "IntrinsicFacts.dll"))
        facts_by_name = {}
        for name in ("operations", "unsupported-order", "unsupported-overflow"):
            source, captured = fixtures / (name + ".c"), objects / (name + ".roslynobj")
            run(name + "-export", [sys.executable, exporter, "--gcc", "--c17", "--gnu_version", "130300",
                                   "-D__ATOMIC_ACQUIRE=2", "-D__ATOMIC_RELAXED=0", source, "-o", captured])
            facts_path = artifacts / (name + "-public-facts.json")
            run(name + "-public-facts", [dotnet, probe_bin / "IntrinsicFacts.dll", captured, facts_path])
            facts_by_name[name] = json.loads(facts_path.read_text())
            report["inputs"].append({"name": name, "source": record(source), "object": record(captured), "publicFacts": record(facts_path),
                "inventoryOnly": True, "artifact": facts_by_name[name]["Artifact"]})
        expected_builtins = {"bfk_" + name for name in ("atomic_load_1", "atomic_load_2", "atomic_load_4", "atomic_store_1", "atomic_store_2", "atomic_store_4", "atomic_store_8",
            "sync_synchronize", "bswap16", "bswap32", "bswap64", "clzll", "add_overflow", "sub_overflow", "mul_overflow")}
        positive_calls = facts_by_name["operations"]["Calls"]
        if {call["Builtin"] for call in positive_calls} != expected_builtins or any(not call["TargetsAgree"] for call in positive_calls):
            raise RuntimeError("Genuine positive input did not expose all15 matching recorded builtin families.")
        for call in positive_calls:
            if "atomic" in call["Builtin"]:
                order = call["Arguments"][-1]["Facts"]["ConversionOrParenthesesChain"]
                if [part["Integer"] for part in order if part["Integer"] is not None] != ["0"]:
                    raise RuntimeError("Positive captured atomic order differs from the admitted relaxed0 input.")
        native = artifacts / "native-oracle"
        run("native-reference-build", [gcc, "-std=c17", "-O2", "-pthread", fixtures / "operations.c", fixtures / "native-oracle.c", "-o", native])
        native_output = run("native-reference", [native])
        lines = native_output.splitlines()
        if lines[:2] != ["result=34", "observations=34"] or len(lines) != 39 or lines[-3:] != ["trace=1", "handoffs=1000", "handoff-sum=500500"]:
            raise RuntimeError("The native positive reference observation protocol changed.")
        if [line.split(":", 1)[0] for line in lines[2:36]] != [str(i) for i in range(34)]:
            raise RuntimeError("Native exact observation indexing changed.")
        report["nativeReference"] = {"binary": record(native), "stdoutSha256": hashlib.sha256(native_output.encode()).hexdigest(), "role": "independent oracle only; never linked into generated execution"}
        generated, repeated = artifacts / "generated", artifacts / "repeated"
        for label, output in (("first", generated), ("repeat", repeated)):
            run("emit-" + label, [dotnet, projection_bin / "Projection.dll", "emit", objects / "operations.roslynobj", output, runtime_project])
        sources = generated_sources(generated)
        if generated_sources(repeated) != sources:
            raise RuntimeError("Repeated intrinsic emission is not deterministic.")
        report["generatedSources"] = sources
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        checker = artifacts / "checker"
        run("generated-build", [dotnet, "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        run("checker-build", [dotnet, "build", tests / "Intrinsics/Checks.csproj", "-c", "Release", property_arg, "-o", checker, "--nologo"])
        if digest(checker / "Generated.dll") != digest(generated / "bin/Release/net10.0/Generated.dll"):
            raise RuntimeError("Checker did not load the separately built generated target binary.")
        report["jit"] = {"checker": record(checker / "Checks.dll"), "generated": record(checker / "Generated.dll"),
            "runtime": record(checker / "RoslynObj.CSharp.Runtime.dll"), "dependencies": deps(checker / "Checks.deps.json", True),
            "generatedDependencies": deps(generated / "bin/Release/net10.0/Generated.deps.json", False)}
        for iteration in (1, 2):
            if run("checker-jit-" + str(iteration), [dotnet, checker / "Checks.dll"]) != native_output:
                raise RuntimeError("Generated JIT observations differ from the complete native reference.")
        if not args.skip_aot:
            aot = artifacts / "aot"
            run("checker-aot-publish", [dotnet, "publish", tests / "Intrinsics/Checks.csproj", "-c", "Release", property_arg,
                "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc), "-o", aot, "--nologo"], seconds=240)
            for iteration in (1, 2):
                if run("checker-aot-" + str(iteration), [aot / "Checks"]) != native_output:
                    raise RuntimeError("Generated NativeAOT observations differ from the complete native reference.")
            linked = run("checker-aot-dependencies", ["ldd", aot / "Checks"])
            if any(word in linked.lower() for word in ("sqlite", "roslynobj", "intrinsic", "oracle")):
                raise RuntimeError("NativeAOT execution gained a native application/reader/provider dependency.")
            report["aot"] = {"binary": record(aot / "Checks"), "dependencies": linked}
        for name, expected_text in (("unsupported-order", "captured constant relaxed atomic order"),
                                     ("unsupported-overflow", "scalar width/sign/effective alignment")):
            if not facts_by_name[name]["Calls"]:
                raise RuntimeError(name + ": rejection feature did not activate in the genuine public input.")
            output = artifacts / (name + "-generated")
            diagnostic = run(name + "-rejected", [dotnet, projection_bin / "Projection.dll", "emit", objects / (name + ".roslynobj"), output, runtime_project], expected=1)
            if expected_text not in diagnostic or output.exists():
                raise RuntimeError(name + ": unsupported feature was not diagnosed before output publication.")
            report["rejections"].append({"name": name, "diagnostic": diagnostic, "outputPublished": False})
        report["status"] = "incomplete-jit-only" if args.skip_aot else "passed-bounded-recorded-intrinsic-slice"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(digest(repository / path) != sha for path, sha in implementation.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "Compiled source set or attributed source content changed."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        (artifacts / "intrinsics-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(artifacts / "intrinsics-receipt.json"))
    return 0 if report["status"] == "passed-bounded-recorded-intrinsic-slice" else 1


if __name__ == "__main__":
    sys.exit(main())
