#!/usr/bin/env python3
"""Build raw and processed managed libghostty directly from its Zig graph.

All derived files stay in disposable storage. This prepares reviewable source
products; publication and behavioral qualification are separate operations.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import time
import xml.etree.ElementTree as ET

sys.dont_write_bytecode = True
from prepare_direct import prepare


def notices(root, project, zig):
    pin = json.loads((root / "config/source.json").read_text())
    sources = {item["name"]: root / "ref" / item["directory"]
               for item in pin.get("license_sources", [])}
    sources["product"] = root / "ref" / pin["directory"]
    sections = []
    for item in pin["license_inputs"]:
        if not item.get("distribute"):
            continue
        path = (root.parent / item["repository_path"] if "repository_path" in item else
                zig.parent / item["toolchain_path"] if "toolchain_path" in item else
                sources[item.get("source", "product")] / item["path"])
        sections.append(item["name"] + "\n" + path.read_text())
    (project.parent / "THIRD-PARTY-NOTICES").write_text("\n\n".join(sections) + "\n")
    tree = ET.parse(project)
    group = ET.SubElement(tree.getroot(), "ItemGroup")
    ET.SubElement(group, "None", Update="THIRD-PARTY-NOTICES",
                  TargetPath="GhosttyVt.THIRD-PARTY-NOTICES",
                  CopyToOutputDirectory="PreserveNewest", CopyToPublishDirectory="PreserveNewest")
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--zig", type=Path, required=True)
    parser.add_argument("--work", type=Path, required=True)
    parser.add_argument("--evidence", type=Path, required=True)
    parser.add_argument("--graph", type=Path, help="Existing actual upstream source graph")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    work, evidence, compiler, zig = (p.resolve() for p in
                                    (args.work, args.evidence, args.compiler, args.zig))
    if not work.is_relative_to(Path("/mnt/storage/dotcc-libghostty-disposable")):
        parser.error("--work must be under /mnt/storage/dotcc-libghostty-disposable")
    work.mkdir(parents=True, exist_ok=True)
    evidence.mkdir(parents=True, exist_ok=True)
    temp = work / "tmp"
    temp.mkdir(exist_ok=True)
    env = dict(os.environ, TMPDIR=str(temp))
    records = []

    def run(stage, command):
        command = list(map(str, command))
        log = work / (stage + ".log")
        started = time.monotonic()
        with log.open("w") as output:
            result = subprocess.run(command, cwd=root.parent, env=env,
                                    stdout=output, stderr=subprocess.STDOUT)
        records.append(dict(stage=stage, command=command, exit=result.returncode,
                            seconds=time.monotonic() - started, log=str(log)))
        (evidence / "stages.json").write_text(json.dumps(records, indent=2) + "\n")
        print(stage, result.returncode, flush=True)
        if result.returncode:
            raise SystemExit(result.returncode)

    graph = (args.graph.resolve() if args.graph else
             prepare(root, zig, work / "native", evidence / "native"))
    run("engine", ["python3", root / "scripts/retry_direct.py", "--compiler", compiler,
                   "--graph", graph, "--work", work / "engine", "--evidence", evidence / "engine"])
    pin = json.loads((root / "config/source.json").read_text())
    boundary = work / "boundary.csobj"
    run("boundary", ["dotnet", compiler, root / "src/Boundary/ghostty_boundary.c", "--emit=obj",
                     "--aggregate-data", "-std=c11", "-I" + str(root / "ref" / pin["directory"] / "include"),
                     "-o", boundary])
    raw = work / "raw/TranslatedGhosttyVt"
    run("link", ["dotnet", compiler, work / "engine/engine.csobj", boundary,
                 "--emit=managedlib", "--nest-types", "--runtime=auto", "--class-name", "GhosttyVt",
                 "--namespace", "Managed", "--split=size", "--split-size=262144", "--literal-pool",
                 "--deduplicate-inline", "--prune-unused-inline", "--export-enum", "DOTCC_STYLE_*", "-o", raw])
    project = raw / "TranslatedGhosttyVt.csproj"
    notices(root, project, zig)
    run("raw-build", ["dotnet", "build", project, "-c", "Release", "--nologo"])
    processed = work / "processed/TranslatedGhosttyVt"
    # Replace only this script's derived tree, never sources or accepted products.
    if processed.exists():
        shutil.rmtree(processed)
    shutil.copytree(raw, processed, ignore=shutil.ignore_patterns("bin", "obj"))
    tool = work / "postprocessor"
    run("postprocessor-build", ["dotnet", "build", root.parent / "DotCC.PostProcess/DotCC.PostProcess.csproj",
                                "-c", "Release", "-p:PublishAot=false", "-p:UseArtifactsOutput=true",
                                "-p:ArtifactsPath=" + str(tool), "--nologo"])
    processed_project = processed / project.name
    run("processed-restore", ["dotnet", "restore", processed_project, "--nologo"])
    run("postprocess", ["dotnet", tool / "bin/DotCC.PostProcess/release/dotcc-postprocess.dll",
                        processed_project, "--in-place"])
    run("processed-build", ["dotnet", "build", processed_project, "-c", "Release", "--nologo"])
    for directory in (raw, processed):
        owned = sorted(p.relative_to(directory).as_posix() for p in directory.iterdir() if p.is_file())
        (directory / ".campaign-files.json").write_text(json.dumps(owned, indent=2) + "\n")
    (evidence / "products.json").write_text(json.dumps(dict(raw=str(project),
        processed=str(processed_project), graph=str(graph), delivery_qualified=False), indent=2) + "\n")
    print(project)
    print(processed_project)


if __name__ == "__main__":
    main()
