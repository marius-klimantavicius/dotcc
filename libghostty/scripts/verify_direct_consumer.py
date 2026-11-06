#!/usr/bin/env python3
"""Run the unchanged owning consumer against both direct source products."""
import argparse
import json
import os
from pathlib import Path
import subprocess
import time


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--raw-project", type=Path, required=True)
    parser.add_argument("--processed-project", type=Path, required=True)
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--evidence", type=Path, required=True)
    parser.add_argument("--rid", default="linux-x64")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    work, evidence = args.work.resolve(), args.evidence.resolve()
    if not work.is_relative_to(Path("/mnt/storage/dotcc-libghostty-disposable")):
        parser.error("--work must be under /mnt/storage/dotcc-libghostty-disposable")
    work.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    temp = work / "tmp"
    temp.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temp))
    # The authored consumer source is compiled unchanged. Root the full translated
    # assembly in NativeAOT, including methods outside this consumer's call graph.
    project = work / "OwningConsumer.csproj"
    project.write_text('''<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup>
<OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework>
<AssemblyName>GhosttyVt.DirectConsumer</AssemblyName><IsAotCompatible>true</IsAotCompatible>
<InvariantGlobalization>true</InvariantGlobalization><ImplicitUsings>enable</ImplicitUsings>
<EnableDefaultCompileItems>false</EnableDefaultCompileItems></PropertyGroup><ItemGroup>
<Compile Include="''' + str(root / "samples/ManagedConsumer/Program.cs") + '''"/>
<ProjectReference Include="''' + str(root / "src/Managed.GhosttyVt/Managed.GhosttyVt.csproj") + '''"
 AdditionalProperties="TranslatedProject=$(TranslatedProject)"/>
<TrimmerRootAssembly Include="TranslatedGhosttyVt" Condition="'$(PublishAot)' == 'true'"/>
</ItemGroup></Project>''')
    records, results = [], []

    def run(stage, command):
        command = list(map(str, command))
        started = time.monotonic()
        result = subprocess.run(command, cwd=root.parent, env=env, capture_output=True, text=True)
        log = work / (stage + ".log")
        log.write_text(result.stdout + result.stderr)
        records.append(dict(stage=stage, command=command, exit=result.returncode,
                            seconds=time.monotonic() - started, log=str(log)))
        (evidence / "stages.json").write_text(json.dumps(records, indent=2) + "\n")
        print(stage, result.returncode, flush=True)
        if result.returncode:
            raise SystemExit(result.returncode)
        return result

    expected = "PASS owning terminal transcript, UTF-8 chunking, screens, resize/reset, concurrent isolation and disposal\n"
    for form, product in (("raw", args.raw_project.resolve()), ("processed", args.processed_project.resolve())):
        for mode in ("jit", "aot"):
            label = form + "-" + mode
            output = work / label
            command = ["dotnet", "publish" if mode == "aot" else "build", project,
                       "-c", "Release", "-p:PublishAot=" + str(mode == "aot").lower(),
                       "-p:TranslatedProject=" + str(product), "-p:UseArtifactsOutput=true",
                       "-p:ArtifactsPath=" + str(work / (label + "-build")), "-o", output, "--nologo"]
            if mode == "aot":
                command += ["-r", args.rid]
            run(label + "-build", command)
            executable = output / "GhosttyVt.DirectConsumer"
            result = run(label + "-run", [executable] if mode == "aot" else ["dotnet", str(executable) + ".dll"])
            if result.stdout != expected or result.stderr:
                raise RuntimeError(label + ": owning assertion transcript differs")
            results.append(dict(form=form, mode=mode, passed=True, whole_library_rooted=mode == "aot"))
            (evidence / "consumer.json").write_text(json.dumps(dict(results=results,
                complete=len(results) == 4), indent=2) + "\n")


if __name__ == "__main__":
    main()
