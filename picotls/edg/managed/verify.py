#!/usr/bin/env python3
"""Build and execute the shared BCL provider against EDG translated picotls."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import sys
import time

HERE = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--core", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--native-layout", type=Path, required=True)
    parser.add_argument("--aot", action="store_true")
    parser.add_argument("--runtime", default="linux-x64")
    args = parser.parse_args()
    output = args.output_dir.resolve()
    output.mkdir(parents=True, exist_ok=False)
    report = {"status": "running", "commands": [], "checks": [], "runtime": args.runtime}
    def save():
        (output / "results.json").write_text(json.dumps(report, indent=2) + "\n")
    def run(label, command, timeout=900):
        command = list(map(str, command))
        entry = {"label": label, "command": command}
        report["commands"].append(entry)
        save()
        start = time.monotonic()
        with (output / (label + ".stdout")).open("wb") as stdout, (output / (label + ".stderr")).open("wb") as stderr:
            result = subprocess.run(command, stdout=stdout, stderr=stderr, timeout=timeout,
                                    env=dict(os.environ, DOTNET_PROCESSOR_COUNT="4"))
        entry.update(exitCode=result.returncode, seconds=round(time.monotonic() - start, 3))
        save()
        print(label + ": exit " + str(result.returncode), flush=True)
        if result.returncode:
            raise RuntimeError(label + " failed; see retained logs")
    try:
        prepared = output / "projects"
        run("prepare", [sys.executable, HERE.parent / "provider.py", "--core", args.core.resolve(), "--output", prepared])
        manifest = json.loads((prepared / "provider-manifest.json").read_text())
        names = ["TranslatedAbi", "UpstreamVectors", "ProviderVectors", "TlsTests", "ManagedConsumer"]
        for name in names:
            project = Path(manifest["projects"][name])
            run("jit-build-" + name, ["dotnet", "build", project, "-c", "Release", "--nologo", "-p:TreatWarningsAsErrors=true"])
            command = ["dotnet", project.parent / "bin/Release/net10.0" / (name + ".dll")]
            if name == "ManagedConsumer":
                (output / "consumer-jit-command.json").write_text(json.dumps(list(map(str, command))) + "\n")
            else:
                if name == "TranslatedAbi": command.append(args.native_layout.resolve())
                if name == "TlsTests": command.append(output / "tls-jit-credentials")
                run("jit-run-" + name, command, timeout=600)
                report["checks"].append(name + " JIT")
            if args.aot:
                published = output / ("aot-" + name)
                run("aot-build-" + name, ["dotnet", "publish", project, "-c", "Release", "-r", args.runtime,
                    "-p:PublishAot=true", "-p:TreatWarningsAsErrors=true", "-p:IlcTreatWarningsAsErrors=true", "-o", published], timeout=1800)
                command = [published / name]
                if name == "ManagedConsumer":
                    (output / "consumer-aot-command.json").write_text(json.dumps(list(map(str, command))) + "\n")
                else:
                    if name == "TranslatedAbi": command.append(args.native_layout.resolve())
                    if name == "TlsTests": command.append(output / "tls-aot-credentials")
                    run("aot-run-" + name, command, timeout=600)
                    report["checks"].append(name + " NativeAOT")
        report["status"] = "passed"
    except Exception as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        save()


if __name__ == "__main__":
    main()
