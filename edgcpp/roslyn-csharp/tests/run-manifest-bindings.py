#!/usr/bin/env python3
"""Exercise the production CLI with the genuine, already accepted callback input."""
from __future__ import annotations
import argparse
import copy
import hashlib
import json
import os
from pathlib import Path
import subprocess
import sys


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--artifacts", type=Path, required=True)
    parser.add_argument("--object", type=Path, required=True)
    args = parser.parse_args()
    project = Path(__file__).resolve().parents[1]
    repository = project.parents[1]
    artifacts = args.artifacts.resolve()
    artifacts.mkdir(parents=True, exist_ok=False)
    measured = project / "tools/run-measured.py"
    sources = sorted(p for p in project.rglob("*") if p.is_file() and p.suffix in {".cs", ".csproj"}
                     and not any(part in {"bin", "obj"} for part in p.relative_to(project).parts))
    sources += [project / "Directory.Build.props", Path(__file__), measured]
    hashes = {str(p.relative_to(project)): digest(p) for p in sources}
    report = {"status": "incomplete", "scope": "production manifest/CLI binding integration, existing callback semantics",
              "nativeApplicationFallback": False, "implementation": hashes, "phases": []}
    environment = os.environ.copy()
    native = artifacts / "native"
    native.mkdir()
    environment["LD_LIBRARY_PATH"] = str(native)  # Explicit checker-only observer provider search.
    def run(name: str, command, expected: int = 0, seconds: int = 120):
        phase = artifacts / "phases" / name
        subprocess.run([sys.executable, str(measured), "--output-dir", str(phase), "--timeout-seconds", str(seconds),
                        "--memory-bytes", str(2 * 1024**3), "--", *map(str, command)], cwd=repository, env=environment)
        result = json.loads((phase / "run.json").read_text())
        report["phases"].append(result | {"phase": name})
        if result["exitCode"] != expected or result["budgetViolation"] is not None:
            raise RuntimeError(f"{name} returned {result['exitCode']}, expected {expected}.")
        return (phase / "stdout.txt").read_text().strip()
    cli_project = project / "src/RoslynObj.CSharp.Cli/RoslynObj.CSharp.Cli.csproj"
    cli = cli_project.parent / "bin/Release/net10.0/roslynobj-csharp.dll"
    try:
        run("build-cli", ["dotnet", "build", cli_project, "-c", "Release", "--nologo"])
        inspected = artifacts / "inspected.json"
        run("inspect", ["dotnet", cli, "inspect", args.object.resolve(), "--report", inspected])
        provider = native / "libcallbacks-adapter.so"
        provider_source = project / "tests/Fixtures/T2/function-addresses/native-adapters.c"
        run("build-provider", ["gcc", "-std=c17", "-shared", "-fPIC", provider_source, "-o", provider])
        request = json.loads((project / "acceptance/sqlite/input.json").read_text())
        request["inputs"] = [json.loads(inspected.read_text())["input"]]
        request["runtime"]["runtimeProjectPath"] = str(project / "src/RoslynObj.CSharp.Runtime/RoslynObj.CSharp.Runtime.csproj")
        request["output"].update(directory=str(artifacts / "generated"), namespaceName="Translated", className="Program", projectName="Callbacks")
        request["budgets"].update(maximumWorkingSetBytes=2 * 1024**3, maximumElapsedSeconds=120, maximumOutputBytes=64 * 1024**2)
        manifest = artifacts / "request.json"
        manifest.write_text(json.dumps(request, indent=2) + "\n")
        inventory = artifacts / "inventory.json"
        run("inventory", ["dotnet", cli, "inventory", manifest, "--report", inventory])
        imports = [p for p in json.loads(inventory.read_text())["imports"] if p["required"]]
        expected_names = {"native_reset_events", "native_mark_event", "native_event_count", "native_event_at",
                          "native_echo_callback", "native_invoke_callback", "native_callback_provider"}
        if len(imports) != 7 or {p["linkageName"] for p in imports} != expected_names:
            raise RuntimeError("Genuine callback input required imports differ from the accepted seven-provider contract.")
        request["runtime"].update(providers=["callback-observer-adapter"], bindings=[{
            "linkageName": p["linkageName"], "kind": "function", "provider": "callback-observer-adapter",
            "symbol": p["linkageName"], "libraryPath": str(provider), "librarySha256": digest(provider),
            "signature": p["signature"], "callingConvention": p["callingConvention"],
            "ownership": "borrowed synchronous scalar/pointer arguments; no retained managed ownership",
            "errorContract": "fixture adapter has no errno/result error protocol",
            "threadContract": "single-thread fixture observations only",
            "callbackContract": "synchronous Cdecl callback reentry; no exception crossing"
        } for p in imports])
        for name in ("signature-mismatch", "missing-ownership", "provider-hash-mismatch", "missing-binding"):
            rejected = copy.deepcopy(request)
            rejected["output"]["directory"] = str(artifacts / (name + "-output"))
            binding = rejected["runtime"]["bindings"][0]
            if name == "signature-mismatch": binding["signature"] += " mismatched"
            elif name == "missing-ownership": binding["ownership"] = ""
            elif name == "provider-hash-mismatch": binding["librarySha256"] = "0" * 64
            else: rejected["runtime"]["bindings"].pop()
            rejected_path = artifacts / (name + ".json")
            rejected_path.write_text(json.dumps(rejected, indent=2) + "\n")
            diagnostic_report = artifacts / (name + "-report.json")
            run(name, ["dotnet", cli, "translate", rejected_path, "--report", diagnostic_report], expected=1)
            diagnostic = json.loads(diagnostic_report.read_text())
            if Path(rejected["output"]["directory"]).exists() or diagnostic["translationComplete"] or not any(p["code"] == "ROC1301" for p in diagnostic["diagnostics"]):
                raise RuntimeError("An unsupported/mismatched binding published an output project.")
        manifest.write_text(json.dumps(request, indent=2) + "\n")
        completed_report = artifacts / "program-report.json"
        run("translate", ["dotnet", cli, "translate", manifest, "--report", completed_report])
        completed = json.loads(completed_report.read_text())
        generated = Path(request["output"]["directory"])
        if not completed["translationComplete"] or completed["runtime"]["bindings"] != request["runtime"]["bindings"]:
            raise RuntimeError("Completed report lost the exact provider dependency contract.")
        completion = json.loads((generated / "completion.json").read_text())
        if not completion["complete"] or any(digest(generated / p["relativePath"]) != p["sha256"] for p in completion["files"]):
            raise RuntimeError("Completion manifest file hashes differ.")
        checker = project / "tests/Callbacks/Callbacks.csproj"
        property_arg = "-p:GeneratedProject=" + str(generated / "Generated.csproj")
        checker_output = artifacts / "checker"
        run("checker-build", ["dotnet", "build", checker, "-c", "Release", property_arg, "-o", checker_output, "--nologo"])
        jit = run("checker-jit", ["dotnet", checker_output / "Callbacks.dll"])
        aot_output = artifacts / "checker-nativeaot"
        run("checker-publish", ["dotnet", "publish", checker, "-c", "Release", property_arg, "-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", aot_output, "--nologo"], seconds=300)
        aot = run("checker-aot", [aot_output / "Callbacks"])
        expected_output = "result=751\nevents=7\n0:21\n1:11\n2:21\n3:31\n4:21\n5:11\n6:41"
        if jit != expected_output or aot != expected_output:
            raise RuntimeError("Production CLI callback execution differs from the accepted native observations.")
        deps = json.loads((checker_output / "Generated.deps.json").read_text())["libraries"]
        if {p.split("/")[0] for p in deps} != {"Generated", "RoslynObj.CSharp.Runtime"}:
            raise RuntimeError("Generated library acquired an unexpected dependency.")
        if any(digest(project / p) != checksum for p, checksum in hashes.items()):
            raise RuntimeError("Implementation inputs changed during the frozen acceptance run.")
        report.update(status="passed-production-cli-bindings", jitOutput=jit, nativeAotOutput=aot,
                      runtimeDependencies=list(deps), provider={"path": str(provider), "sha256": digest(provider),
                      "source": str(provider_source), "sourceSha256": digest(provider_source)},
                      input={"path": str(args.object.resolve()), "sha256": digest(args.object)},
                      completion=completion, nativeImports=json.loads((generated / "Generated.NativeImports.json").read_text()),
                      buildArtifacts=[{"path": str(p), "sha256": digest(p)} for p in [cli, cli.parent / "RoslynObj.CSharp.dll", cli.parent / "RoslynObj.dll", aot_output / "Callbacks"]])
    except Exception as problem:
        report["error"] = str(problem)
        raise
    finally:
        (artifacts / "manifest-binding-report.json").write_text(json.dumps(report, indent=2) + "\n")
    return 0


if __name__ == "__main__":
    sys.exit(main())
