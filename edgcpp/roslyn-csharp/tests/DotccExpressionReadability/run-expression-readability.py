#!/usr/bin/env python3
"""Coordinator-only native/JIT/NativeAOT acceptance for typed expression rendering."""
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
    parser.add_argument("--objects", type=Path, help="Reuse immutable genuine captures; do not claim a reproduced export.")
    parser.add_argument("--skip-aot", action="store_true", help="Record an explicitly incomplete JIT-only run.")
    args = parser.parse_args()
    test = Path(__file__).resolve().parent
    tests, project = test.parent, test.parent.parent
    repository = project.parents[1]
    fixtures = tests / "Fixtures/DotccExpressionReadability"
    runtime = project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj"
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
    roots = [project / "src/RoslynObj.CSharp", project / "src/RoslynObj.CSharp.Runtime",
             repository / "edgcpp/roslyn/managed/RoslynObj", tests / "Projection", test, fixtures]

    def source_paths() -> set[Path]:
        paths = {path for root in roots for path in root.rglob("*") if path.is_file()
                 and path.suffix in {".cs", ".csproj", ".props", ".targets", ".py", ".c", ".md"}
                 and not any(part in {"obj", "bin", "__pycache__"} for part in path.relative_to(root).parts)}
        paths |= {measured, exporter, repository / "edgcpp/roslyn/schema/kinds.tsv"}
        for root in roots:
            for parent in root.parents:
                if parent == repository.parent:
                    break
                paths |= {path for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config", "nuget.config")
                          if (path := parent / name).is_file()}
        return paths

    paths = source_paths()
    implementation = {str(path.relative_to(repository)): digest(path) for path in sorted(paths)}
    phases: list[dict] = []
    report: dict = {"gate": "DotCC-typed-expression-readability", "status": "incomplete", "phases": phases,
        "implementation": implementation, "producer": record(producer), "exportsReproduced": args.objects is None,
        "nativeApplicationFallback": False, "nativeProviders": [],
        "limits": ["Linux x64 C17 fixture through the public serialized RoslynObj reader and direct emitter",
                   "30 source observations and six sequenced events; no claim of broader C++ or provider closure",
                   "19 intrinsic-type renderer probes additionally compile with checked arithmetic enabled",
                   "the native application is an independent oracle and is never linked into generated execution"]}

    def run(name: str, command: list, seconds: int = 120) -> str:
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(4 * 1024**3), "--", *map(str, command)],
                       cwd=repository, env=environment, check=False)
        result = json.loads((phase / "run.json").read_text())
        phases.append(result | {"phase": name, "expectedExitCode": 0})
        if result["exitCode"] != 0 or result.get("budgetViolation") is not None:
            raise RuntimeError(name + ": unexpected result; inspect the preserved measured phase logs.")
        return (phase / "stdout.txt").read_text().strip()

    def tool(name: str) -> Path:
        path = Path(shutil.which(name) or name).resolve()
        if not path.is_file():
            raise RuntimeError("Missing tool: " + name)
        return path

    def dependencies(path: Path, checker: bool) -> dict:
        libraries = list(json.loads(path.read_text())["libraries"])
        expected = {"Generated", "RoslynObj.CSharp.Runtime"} | ({"Checks"} if checker else set())
        if {name.split("/")[0] for name in libraries} != expected:
            raise RuntimeError("Generated execution acquired a reader or native-application dependency.")
        return {"file": record(path), "libraries": libraries}

    def generated_sources(folder: Path) -> dict:
        files = {path.name: digest(path) for path in folder.iterdir() if path.is_file()}
        if set(files) != {"Generated.cs", "Generated.csproj"}:
            raise RuntimeError("The self-contained fixture acquired an unexpected generated file.")
        source = (folder / "Generated.cs").read_text()
        if any(marker in source for marker in ("DllImport", "NativeLibrary", "RoslynObject")):
            raise RuntimeError("Generated execution acquired a native import or reader dependency.")

        def method_body(name: str) -> str:
            match = re.search(r"\b" + name + r"\([^;{}]*\)\s*\{", source)
            if match is None:
                raise RuntimeError("Missing representative emitted method: " + name)
            depth = 1
            for index in range(match.end(), len(source)):
                if source[index] == "{":
                    depth += 1
                elif source[index] == "}":
                    depth -= 1
                    if depth == 0:
                        return source[match.end():index].strip()
            raise RuntimeError("Unclosed representative emitted method: " + name)

        samples = {}
        for name in ("precedence_add", "grouping_sub", "grouping_div", "floating_right", "floating_left", "unary_negative", "pointer_value", "target_shift"):
            body = method_body(name)
            samples[name] = body
            if name in {"precedence_add", "grouping_sub", "grouping_div", "floating_right", "floating_left"} and "(unchecked(" in body:
                raise RuntimeError("A typed arithmetic sample retained redundant nested unchecked wrappers.")
        if "checked((int)" not in samples["target_shift"] or "TargetArithmetic.ShiftLeft" not in samples["target_shift"]:
            raise RuntimeError("The selected shift/count conversion lost its explicit target policy.")
        samples["loop_sum"] = method_body("loop_sum")
        if "for (" not in samples["loop_sum"]:
            raise RuntimeError("Preferred scalar rendering lowered an ordinary for loop.")
        report["representativeEmittedBodies"] = samples
        return files

    try:
        dotnet, gcc, aot_cc = tool(args.dotnet), tool("gcc"), tool(args.aot_cc)
        report["toolchain"] = {"dotnet": record(dotnet), "sdkVersion": run("dotnet-version", [dotnet, "--version"]),
            "sdkInfo": run("dotnet-info", [dotnet, "--info"]), "nativeCompiler": record(gcc),
            "nativeVersion": run("gcc-version", [gcc, "--version"]), "aotCompiler": record(aot_cc),
            "aotVersion": run("aot-cc-version", [aot_cc, "--version"])}
        sdk_version = report["toolchain"]["sdkVersion"]
        sdk_line = next(line for line in run("dotnet-sdks", [dotnet, "--list-sdks"]).splitlines() if line.split()[0] == sdk_version)
        sdk = Path(sdk_line.split("[", 1)[1].removesuffix("]")) / sdk_version
        report["toolchain"]["managedCompilerFiles"] = [record(sdk / name) for name in ("Roslyn/bincore/csc.dll", "MSBuild.dll")]
        projection = artifacts / "projection"
        run("build-projection", [dotnet, "build", tests / "Projection/Projection.csproj", "-c", "Release", "--nologo", "-o", projection])
        report["build"] = [record(projection / name) for name in ("Projection.dll", "RoslynObj.CSharp.dll", "RoslynObj.dll")]
        captured = objects / "expressions.roslynobj"
        if args.objects is None:
            run("expressions-export", [sys.executable, exporter, "--gcc", "--c17", "--gnu_version", "130300",
                fixtures / "expressions.c", "-o", captured])
        inspection = run("public-inspection", [dotnet, projection / "Projection.dll", "inspect", captured])
        for marker in ("FUNCTION fixture state=definition", "eok_subtract", "eok_divide", "eok_shiftl", "eok_land", "eok_lor", "eok_question", "eok_comma"):
            if marker not in inspection:
                raise RuntimeError("A focused retained operation did not activate: " + marker)
        report["input"] = {"source": record(fixtures / "expressions.c"), "object": record(captured),
            "inspection": record(artifacts / "phases/public-inspection/stdout.txt"), "inspectionIsInventoryOnly": True}
        oracle = artifacts / "native-oracle"
        run("native-build", [gcc, "-std=c17", "-O2", "-ffp-contract=off", fixtures / "expressions.c", fixtures / "native-oracle.c", "-o", oracle])
        native = run("native-run", [oracle])
        values = [14, 23, 15, 0, 13, 15, 1, 0, -7, 7, 1, 0, 0, 1, 0, 1, 30, 20, 96, 23, 17, 0, 1, 19, 7, 5, 6, 123456, 96, 10]
        expected = "\n".join(["result=30", "observations=30", *[f"{index}:{value}" for index, value in enumerate(values)],
                              "events=6", *[f"event{index}:{index + 1}" for index in range(6)]])
        if native != expected:
            raise RuntimeError("The independent native oracle differs from the defined observation protocol.")
        report["nativeReference"] = {"binary": record(oracle), "stdoutSha256": hashlib.sha256(native.encode()).hexdigest()}
        generated, repeated = artifacts / "generated", artifacts / "repeated"
        for name, output in (("first", generated), ("repeat", repeated)):
            run("emit-" + name, [dotnet, projection / "Projection.dll", "emit", captured, output, runtime])
        report["generatedSources"] = generated_sources(generated)
        if generated_sources(repeated) != report["generatedSources"]:
            raise RuntimeError("Repeated emission is not deterministic.")
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        checker = artifacts / "checker"
        run("generated-build", [dotnet, "build", generated / "Generated.csproj", "-c", "Release", "--nologo"])
        run("checker-build", [dotnet, "build", test / "Checks.csproj", "-c", "Release", property_arg, "-o", checker, "--nologo"])
        if digest(checker / "Generated.dll") != digest(generated / "bin/Release/net10.0/Generated.dll"):
            raise RuntimeError("The checker did not load the separately built generated target binary.")
        report["jit"] = {"checker": record(checker / "Checks.dll"), "generated": record(checker / "Generated.dll"),
            "runtime": record(checker / "RoslynObj.CSharp.Runtime.dll"), "dependencies": dependencies(checker / "Checks.deps.json", True),
            "generatedDependencies": dependencies(generated / "bin/Release/net10.0/Generated.deps.json", False)}
        for iteration in (1, 2):
            if run("checker-jit-" + str(iteration), [dotnet, checker / "Checks.dll"]) != native:
                raise RuntimeError("JIT observations differ from the complete native reference.")
        emitter_checks, probes = artifacts / "emitter-checks", artifacts / "syntax-probes"
        run("emitter-checks-build", [dotnet, "build", test / "EmitterChecks.csproj", "-c", "Release", "--nologo", "-o", emitter_checks])
        if run("emitter-checks", [dotnet, emitter_checks / "EmitterChecks.dll", probes, runtime]) != "rendererChecks=19":
            raise RuntimeError("The syntax renderer probes did not complete.")
        run("syntax-probes-build", [dotnet, "build", probes / "Probes.csproj", "-c", "Release", "--nologo"])
        if run("syntax-probes-jit", [dotnet, probes / "bin/Release/net10.0/Probes.dll"]) != "rendererChecks=19":
            raise RuntimeError("Compiled syntax probe behavior changed.")
        report["syntaxProbes"] = {"count": 19, "checkedProject": record(probes / "Probes.csproj"), "source": record(probes / "Program.cs")}
        if not args.skip_aot:
            aot = artifacts / "aot"
            run("checker-aot-publish", [dotnet, "publish", test / "Checks.csproj", "-c", "Release", property_arg,
                "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-p:CppCompilerAndLinker=" + str(aot_cc),
                "-o", aot, "--nologo"], seconds=240)
            for iteration in (1, 2):
                if run("checker-aot-" + str(iteration), [aot / "Checks"]) != native:
                    raise RuntimeError("NativeAOT observations differ from the complete native reference.")
            linked = run("checker-aot-dependencies", ["ldd", aot / "Checks"])
            if any(word in linked.lower() for word in ("roslynobj", "oracle", "expressions")):
                raise RuntimeError("NativeAOT execution gained a native application or reader dependency.")
            report["aot"] = {"binary": record(aot / "Checks"), "dependencies": linked}
        report["status"] = "incomplete-jit-only" if args.skip_aot else "passed-bounded-expression-readability"
    except Exception as exception:
        report["failure"] = str(exception)
        print(str(exception), file=sys.stderr)
    finally:
        if source_paths() != paths or any(digest(repository / path) != sha for path, sha in implementation.items()):
            report["status"] = "incomplete"
            report["sourceFreezeFailure"] = "The attributed source set or content changed."
        report["sourceFreezeVerified"] = "sourceFreezeFailure" not in report
        receipt = artifacts / "expression-readability-receipt.json"
        receipt.write_text(json.dumps(report, indent=2) + "\n")
    print(report["status"] + ": " + str(receipt))
    return 0 if report["status"] == "passed-bounded-expression-readability" else 1


if __name__ == "__main__":
    sys.exit(main())
