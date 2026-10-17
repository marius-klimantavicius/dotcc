#!/usr/bin/env python3
"""Coordinator-only native/JIT/AOT receipt for named static and constant TLS owners."""
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
    parser.add_argument("--objects", type=Path, required=True, help="Previously measured genuine captures, never recaptured by this runner.")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--aot-cc", default="clang-18")
    args = parser.parse_args()
    tests = Path(__file__).resolve().parent
    project = tests.parent.parent
    repository = project.parents[1]
    fixtures = tests / "Fixtures"
    measured = project / "tools/run-measured.py"
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    objects = args.objects.resolve()
    environment = os.environ.copy()
    environment.pop("LD_LIBRARY_PATH", None)
    environment.pop("LD_PRELOAD", None)
    roots = [project / "src/RoslynObj.CSharp", project / "src/RoslynObj.CSharp.Runtime",
             repository / "edgcpp/roslyn/managed/RoslynObj", tests]

    def source_paths() -> set[Path]:
        paths = {path for folder in roots for path in folder.rglob("*") if path.is_file()
                 and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".c", ".cpp", ".h", ".md"}
                 and not any(part in {"obj", "bin", "__pycache__"} for part in path.relative_to(folder).parts)}
        paths |= {measured, repository / "edgcpp/roslyn/schema/kinds.tsv"}
        for folder in roots:
            for parent in folder.parents:
                if parent == repository.parent:
                    break
                paths |= {path for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config", "nuget.config")
                          if (path := parent / name).is_file()}
        return paths

    paths = source_paths()
    implementation = {str(path.relative_to(repository)): digest(path) for path in sorted(paths)}
    for path in sorted(paths):
        target = artifacts / "frozen-source" / path.relative_to(repository)
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(path, target)
    phases: list[dict] = []
    report: dict = {
        "gate": "dotcc-static-constant-thread-storage-owners", "status": "incomplete", "phases": phases,
        "implementation": implementation, "inputs": [], "generated": {}, "execution": {},
        "exportsReproduced": False, "nativeApplicationFallback": False, "nativeProviders": [],
        "limits": ["Direct public-reader emitter acceptance; no new CLI selection/COMDAT TLS policy or production source-splitting feature.",
                   "TLS admission is file-scope arithmetic/ordinary-pointer and recursive array storage with retained constant or initializer-free zero facts.",
                   "Dynamic TLS routines/aliases, rewritten or explicit zero initializers, class lifetimes, local guards and thread-exit cleanup remain diagnosed.",
                   "Thread state uses actual threads, not async-flow-local context; pointer checks occur while all owning threads remain alive.",
                   "Each admitted object retains a distinct aligned pinned allocation; this changes ownership presentation, not captured object layout.",
                   "Source decomposition is an acceptance-only syntax-tree split into partial declarations in reverse method order.",
                   "Native executions are independent oracles and are never linked into generated execution."]}

    def run(name: str, command: list, expected: int = 0, seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(4 * 1024**3), "--", *map(str, command)],
                       cwd=repository, env=environment, check=False)
        result = json.loads((phase / "run.json").read_text())
        phases.append(result | {"phase": name, "expectedExitCode": expected})
        if result["exitCode"] != expected or result.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected result; inspect preserved measured logs.")
        return (phase / "stdout.txt").read_text().strip()

    def tool(name: str) -> Path:
        path = Path(shutil.which(name) or name).resolve()
        if not path.is_file():
            raise RuntimeError("Missing tool: " + name)
        return path

    def source_files(folder: Path) -> dict:
        return {path.name: digest(path) for path in sorted(folder.iterdir()) if path.is_file()}

    def dependencies(path: Path, checker: str | None) -> dict:
        libraries = json.loads(path.read_text())["libraries"]
        expected = {"Generated", "RoslynObj.CSharp.Runtime"} | ({checker} if checker else set())
        if {name.split("/")[0] for name in libraries} != expected:
            raise RuntimeError("Generated execution gained an emitter/reader/native-application managed dependency.")
        return {"file": record(path), "libraries": list(libraries)}

    try:
        dotnet, gcc, gxx, aot_cc = tool(args.dotnet), tool("gcc"), tool("g++"), tool(args.aot_cc)
        report["toolchain"] = {"dotnet": record(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
            "sdkInfo": run("dotnet-info", [dotnet, "--info"]), "nativeC": record(gcc), "nativeCpp": record(gxx),
            "nativeCVersion": run("gcc-version", [gcc, "--version"]), "nativeCppVersion": run("gxx-version", [gxx, "--version"]),
            "aotNativeCompiler": record(aot_cc), "aotNativeVersion": run("aot-cc-version", [aot_cc, "--version"])}
        sdk_version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["toolchain"]["managedCompilerFiles"] = [record(sdk / name) for name in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]
        emitter = artifacts / "emitter"
        run("build-emitter", [dotnet, "build", tests / "Emit.csproj", "-c", "Release", "--nologo", "-o", emitter])
        report["build"] = [record(emitter / name) for name in ("Emit.dll", "RoslynObj.CSharp.dll", "RoslynObj.dll", "Microsoft.CodeAnalysis.dll", "Microsoft.CodeAnalysis.CSharp.dll")]
        for name in ("constant-thread", "static-first", "static-second", "unsupported-dynamic-thread", "unsupported-cleanup-thread", "unsupported-local-thread"):
            captured = objects / (name + ".roslynobj")
            extension = ".c" if name.startswith("static-") else ".cpp"
            run(name + "-public-inspection", [dotnet, emitter / "Emit.dll", "inspect", captured])
            export = objects.parent / (name + "-export") / "run.json"
            report["inputs"].append({"name": name, "source": record(fixtures / (name + extension)), "object": record(captured),
                "publicInspection": record(artifacts / "phases" / (name + "-public-inspection") / "stdout.txt"),
                "originalExportReceipt": record(export), "inspectionIsInventoryOnly": True, "sourceExportReproducedInThisRun": False})
        runtime_project = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
        for kind, checker, names in (("static", "StaticChecks", ["static-first", "static-second"]),
                                     ("thread", "ThreadChecks", ["constant-thread"])):
            native = artifacts / (kind + "-native-oracle")
            if kind == "static":
                native_command = [gcc, "-std=c17", "-O2", fixtures / "static-first.c", fixtures / "static-second.c", fixtures / "static-native.c", "-o", native]
            else:
                native_command = [gxx, "-std=c++20", "-O2", "-pthread", fixtures / "constant-thread.cpp", fixtures / "thread-native.cpp", "-o", native]
            run(kind + "-native-build", native_command)
            native_output = run(kind + "-native", [native])
            report["execution"][kind] = {"nativeOracle": record(native), "nativeStdoutSha256": hashlib.sha256(native_output.encode()).hexdigest(), "variants": {}}
            generated = artifacts / (kind + "-generated")
            run(kind + "-emit", [dotnet, emitter / "Emit.dll", "emit", generated, runtime_project,
                                *[objects / (name + ".roslynobj") for name in names]])
            source = (generated / "Generated.cs").read_text()
            if any(marker in source for marker in ("DllImport", "NativeLibrary", "RoslynObject")):
                raise RuntimeError("Generated fixture acquired an external/native application dependency.")
            for marker in ("__ProgramStorage", "__AllocateAndZeroStorage", "__PopulateConstantStorage", "__ApplyStorageAddressFixups"):
                if marker not in source:
                    raise RuntimeError("Named process owner/phase absent: " + marker)
            if kind == "thread":
                for marker in ("[ThreadStatic]", "__ThreadStorage", "__AllocateAndZeroThreadStorage", "__PopulateThreadConstants", "__ApplyThreadAddressFixups"):
                    if marker not in source:
                        raise RuntimeError("Named per-thread owner/phase absent: " + marker)
            report["generated"][kind] = {"original": source_files(generated)}
            split = artifacts / (kind + "-split-generated")
            run(kind + "-source-split", [dotnet, emitter / "Emit.dll", "split", generated, split])
            report["generated"][kind]["split"] = source_files(split)
            for variant, output in (("original", generated), ("split", split)):
                label = kind + "-" + variant
                checker_project = tests / (checker + ".csproj")
                property_arg = "-p:GeneratedProject=" + str(output / "Generated.csproj")
                checker_output = artifacts / (label + "-checker")
                run(label + "-generated-build", [dotnet, "build", output / "Generated.csproj", "-c", "Release", "--nologo"])
                run(label + "-checker-build", [dotnet, "build", checker_project, "-c", "Release", property_arg, "-o", checker_output, "--nologo"])
                if digest(checker_output / "Generated.dll") != digest(output / "bin/Release/net10.0/Generated.dll"):
                    raise RuntimeError("Checker did not load its separately built generated target binary.")
                for iteration in (1, 2):
                    if run(label + "-jit-" + str(iteration), [dotnet, checker_output / (checker + ".dll")]) != native_output:
                        raise RuntimeError(label + ": JIT differs from the full independent native observation protocol.")
                aot = artifacts / (label + "-aot")
                run(label + "-aot-publish", [dotnet, "publish", checker_project, "-c", "Release", property_arg,
                    "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc),
                    "-o", aot, "--nologo"], seconds=240)
                for iteration in (1, 2):
                    if run(label + "-aot-" + str(iteration), [aot / checker]) != native_output:
                        raise RuntimeError(label + ": NativeAOT differs from the full independent native observation protocol.")
                linked = run(label + "-aot-dependencies", ["ldd", aot / checker])
                if any(word in linked.lower() for word in ("sqlite", "roslynobj", "oracle")):
                    raise RuntimeError("NativeAOT execution gained a native application or reader dependency.")
                report["execution"][kind]["variants"][variant] = {
                    "jit": {"checker": record(checker_output / (checker + ".dll")), "generated": record(checker_output / "Generated.dll"),
                            "runtime": record(checker_output / "RoslynObj.CSharp.Runtime.dll"),
                            "dependencies": dependencies(checker_output / (checker + ".deps.json"), checker),
                            "generatedDependencies": dependencies(output / "bin/Release/net10.0/Generated.deps.json", None)},
                    "aot": {"binary": record(aot / checker), "dependencies": linked}}
        report["rejections"] = []
        for name in ("unsupported-dynamic-thread", "unsupported-cleanup-thread", "unsupported-local-thread"):
            rejected = artifacts / (name + "-generated")
            diagnostic = run(name + "-rejected", [dotnet, emitter / "Emit.dll", "emit", rejected, runtime_project, objects / (name + ".roslynobj")], expected=1)
            if "thread-initialization" not in diagnostic or rejected.exists():
                raise RuntimeError("An unsupported thread contract was not diagnosed before publication: " + name)
            report["rejections"].append({"name": name, "diagnostic": diagnostic, "outputPublished": False})
        report["status"] = "passed-static-constant-thread-storage-owners"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(digest(repository / path) != sha for path, sha in implementation.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "Compiled/attributed source set or content changed."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        (artifacts / "storage-initialization-receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(artifacts / "storage-initialization-receipt.json"))
    return 0 if report["status"] == "passed-static-constant-thread-storage-owners" else 1


if __name__ == "__main__":
    sys.exit(main())
