#!/usr/bin/env python3
"""Build and execute a byte-preserved copied managed source delivery closure."""
from __future__ import annotations

import argparse
import datetime
import hashlib
import json
import os
from pathlib import Path
import shutil
import sys
import time
import xml.etree.ElementTree as ET

REPO = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(REPO / "Scripts"))
from production_support import subprocess


def sha(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def save(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--receipt", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    options = parser.parse_args()
    receipt = json.loads(options.receipt.read_text())
    required = {"translation", *("test:" + name for name in ("native", "primitives", "abi", "consumer", "behavior", "lifecycle", "interop", "upstream", "audit"))}
    if receipt["status"] != "passed" or required - {row["name"] for row in receipt["tasks"] if row["status"] == "passed"}:
        raise RuntimeError("Copied delivery requires the complete successful optimized verification receipt")
    if options.output.exists():
        raise RuntimeError("Refusing to reuse copied delivery outputs")
    output = options.output.resolve(); output.mkdir(parents=True)
    tree = output / "tree"; tree.mkdir()
    report = dict(status="running", receipt=str(options.receipt.resolve()), receipt_sha256=sha(options.receipt),
                  source_tree=str(tree), files={}, commands=[], cells=[],
                  scope="Exact copied generated and authored source closure; no ref tree, compiler tools or original workspace source references")
    report_path = output / "report.json"

    def execute(label, command, timeout=600):
        log = output / (label + ".log")
        row = dict(label=label, command=list(map(str, command)), status="running", log=str(log),
                   started=datetime.datetime.now(datetime.timezone.utc).isoformat())
        report["commands"].append(row); save(report_path, report)
        start = time.monotonic()
        try:
            with log.open("w") as stream:
                result = subprocess.run(command, cwd=tree, stdout=stream, stderr=subprocess.STDOUT, timeout=timeout, check=False)
            row.update(actual_command=result.args, exit_code=result.returncode, status="passed" if result.returncode == 0 else "failed")
            if result.returncode:
                raise RuntimeError(label + " failed: " + str(log) + "\n" + log.read_text()[-4000:])
            return log.read_text()
        except BaseException as error:
            row.update(status="failed", error=repr(error)); raise
        finally:
            row.update(elapsed_seconds=time.monotonic() - start, log_sha256=sha(log)); save(report_path, report)

    def copy(source):
        if not source.is_file() or source.is_symlink():
            raise RuntimeError("Missing or symlinked source: " + str(source))
        relative = source.relative_to(REPO)
        destination = tree / relative
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(source, destination)
        if sha(source) != sha(destination):
            raise RuntimeError("Copied source differs: " + str(source))
        report["files"][relative.as_posix()] = dict(original=str(source), sha256=sha(source), bytes=source.stat().st_size)
        destination.chmod(destination.stat().st_mode & ~0o222)

    try:
        for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "nuget.config", "global.json"):
            if (REPO / name).is_file():
                copy(REPO / name)
        copy(REPO / "zstd/ManagedConsumer.slnx")
        for directory in ("zstd/src/Managed.Zstd", "zstd/samples/ManagedConsumer"):
            for file in sorted((REPO / directory).iterdir()):
                if file.suffix in (".cs", ".csproj"):
                    copy(file)
        copy(REPO / "zstd/src/Optimization/Primitives.cs")
        for form, entry in receipt["outputs"].items():
            project = Path(entry["project"])
            for name, expected in entry["hashes"].items():
                file = project.parent / name
                if not file.resolve().is_relative_to(project.parent) or sha(file) != expected:
                    raise RuntimeError("Verified generated input changed: " + str(file))
                copy(file)
        # An escaping original absolute link fails before any build can consume it.
        for project in tree.rglob("*.csproj"):
            for node in ET.parse(project).iter("Compile"):
                if include := node.get("Include"):
                    linked = (project.parent / include).resolve()
                    if not linked.is_relative_to(tree) or not linked.is_file():
                        raise RuntimeError("Copied product source link escapes delivery: " + str(linked))
        save(output / "source-manifest.json", report["files"])
        save(report_path, report)
        for form in ("raw", "processed"):
            product = tree / Path(receipt["outputs"][form]["project"]).relative_to(REPO)
            api = tree / "zstd/src/Managed.Zstd/Managed.Zstd.csproj"
            consumer = tree / "zstd/samples/ManagedConsumer/ManagedConsumer.csproj"
            artifacts, binaries = output / "build" / form, output / "bin" / form
            arguments = ["-p:ZstdProject=" + str(product), "--artifacts-path", artifacts]
            execute(form + "-build", ["dotnet", "build", consumer, "-c", "Release", "--nologo", "-m:1", *arguments, "-o", binaries])
            row = dict(form=form, status="running", project=str(product), evaluations=[])
            report["cells"].append(row)
            for role, project in (("consumer", consumer), ("api", api), ("product", product)):
                text = execute(form + "-" + role + "-evaluated-inputs", ["dotnet", "msbuild", project, "-nologo",
                    "-getItem:Compile,ProjectReference", "-p:Configuration=Release", "-p:ZstdProject=" + str(product), "-p:ArtifactsPath=" + str(artifacts)])
                evaluated = json.loads(text)
                files = []
                for kind in ("Compile", "ProjectReference"):
                    for item in evaluated["Items"][kind]:
                        path = Path(item["FullPath"]).resolve()
                        if not path.is_relative_to(tree) or not path.is_file():
                            raise RuntimeError("Evaluated source/project escapes copied delivery: " + str(path))
                        relative = path.relative_to(tree).as_posix()
                        if relative not in report["files"] or sha(path) != report["files"][relative]["sha256"]:
                            raise RuntimeError("Unmanifested copied source/project: " + str(path))
                        files.append(dict(kind=kind, path=str(path), sha256=sha(path)))
                row["evaluations"].append(dict(role=role, files=files))
            for name in ("LICENSE", "COPYING", "THIRD-PARTY-NOTICES"):
                delivered = binaries / ("Zstd." + name)
                if not delivered.is_file() or delivered.read_bytes() != (product.parent / name).read_bytes():
                    raise RuntimeError("Copied consumer notice missing or changed: " + name)
            executable = binaries / "ManagedConsumer.dll"
            row.update(executable=str(executable), executable_sha256=sha(executable))
            execute(form + "-consumer-run", ["dotnet", executable, "--self-test", output / "work" / form])
            row["status"] = "passed"; save(report_path, report)
        observed = {path.relative_to(tree).as_posix() for path in tree.rglob("*") if path.is_file()}
        if observed != set(report["files"]):
            raise RuntimeError("Build wrote unmanifested files into copied source delivery")
        for name, entry in report["files"].items():
            if sha(tree / name) != entry["sha256"]:
                raise RuntimeError("Copied source changed during build: " + name)
        report["status"] = "passed"
    except BaseException as error:
        report.update(status="failed", error=repr(error)); raise
    finally:
        save(report_path, report)
    print(report_path)


if __name__ == "__main__":
    main()
