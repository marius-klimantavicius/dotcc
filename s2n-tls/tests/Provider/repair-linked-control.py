#!/usr/bin/env python3
"""Rebuild one failed retained raw harness with authored test-provider bindings.

This never emits/links C or edits generated C#. A failed row becomes built only
after a real fresh build and actual runtime identity capture. Original assertion
qualification belongs to the separate four-mode expander/executor.
"""
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import shutil
import subprocess
import sys
import uuid
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.delivery import ignore_build_caches, source_files
from campaigns.translation import relocate_project
sys.path.insert(0, str(ROOT / "tests/Managed"))
from execution_identity import authored_sources, capture, hashes
sys.path.insert(0, str(ROOT / "tests/Provider/UpstreamSurface"))
from provider_snapshot import snapshot_provider


def tree_hashes(directory):
    return {p.relative_to(directory).as_posix(): value for p, value in
            ((p, hashes([p])[str(p)]) for p in source_files(directory))}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--executables", type=Path, required=True)
    parser.add_argument("--emission-manifest", type=Path, required=True)
    parser.add_argument("--case", required=True)
    parser.add_argument("--raw-project", type=Path, required=True)
    parser.add_argument("--provider-project", type=Path, required=True)
    parser.add_argument("--provider-binding", type=Path, action="append", required=True)
    parser.add_argument("--provider-freeze", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    args = parser.parse_args()
    if args.output.exists():
        raise RuntimeError("Preserve the existing repair manifest; choose a fresh output")
    manifest = json.loads(args.executables.read_text())
    emissions = json.loads(args.emission_manifest.read_text())
    freeze = json.loads(args.provider_freeze.read_text())
    rows = [r for r in manifest["executables"] if (r["case"], r["form"], r["mode"]) == (args.case, "raw", "jit")]
    units = [r for r in emissions["executables"] if r["case"] == args.case]
    if len(rows) != 1 or rows[0]["status"] != "failed" or len(units) != 1 or units[0]["status"] != "emitted":
        raise RuntimeError("Repair requires one actually failed raw build and its exact successful emitted unit")
    failed, unit = rows[0], units[0]
    if any(manifest[key] != emissions[key] for key in ("campaign_run", "tool_sha256", "product_objects_sha256")):
        raise RuntimeError("Failed linked harness and emitted unit use different campaign inputs")
    reused = failed["reused_emission"]
    if (reused["manifest"]["path"] != str(args.emission_manifest.resolve())
        or reused["manifest"]["sha256"] != hashes([args.emission_manifest])[str(args.emission_manifest)]
        or reused["original_object"] != unit["object"] or reused["object_sha256"] != unit["object_sha256"]):
        raise RuntimeError("Failed build lacks exact immutable emitted-unit provenance")
    original = args.raw_project.resolve()
    failed_commands = [r for r in manifest["commands"] if r["command"][:2] == ["dotnet", "build"]
                       and str(original) in r["command"] and r["exit_code"] != 0]
    if len(failed_commands) != 1 or failed_commands[0]["log"] not in failed.get("error", ""):
        raise RuntimeError("Retained raw project does not match the actual failed command/log")
    immutable = {**manifest["tool_sha256"], **manifest["product_objects_sha256"], **manifest["override_inputs_sha256"],
                 **unit["dependency_inputs_sha256"], unit["object"]: unit["object_sha256"],
                 unit["compiled_source"]: unit["compiled_sha256"], unit["dependency_file"]: unit["dependency_sha256"],
                 str(ROOT / "ref/s2n-tls" / args.case): unit["original_sha256"], **freeze["after_sha256"]}
    immutable.update(hashes([args.executables.resolve(), args.emission_manifest.resolve(), args.provider_freeze.resolve(), Path(__file__).resolve()]))
    if hashes(immutable) != immutable:
        raise RuntimeError("Retained/final-source repair input changed")
    before = tree_hashes(original.parent)
    stamp = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
    work = ROOT / "build/provider/linked-control-repair" / stamp
    artifacts = ROOT / "artifacts/provider/linked-control-repair" / stamp
    artifacts.mkdir(parents=True)
    raw = work / Path(args.case).stem / "raw/S2nTls"
    shutil.copytree(original.parent, raw, ignore=ignore_build_caches)
    if tree_hashes(raw) != before or tree_hashes(original.parent) != before:
        raise RuntimeError("Failed raw source snapshot overlapped a mutation")
    project = raw / original.name
    relocate_project(project, original.parent)
    tree = ET.parse(project)
    references = list(tree.iter("ProjectReference"))
    if len(references) != 1 or Path(references[0].get("Include")).resolve() != ROOT / "src/CryptoProvider/CryptoProvider.csproj":
        raise RuntimeError("Bounded repair expects the observed production-reference failure")
    snapshot = snapshot_provider(args.provider_project, args.provider_binding, work / "provider-inputs")
    references[0].set("Include", snapshot["project"])
    group = ET.SubElement(tree.getroot(), "ItemGroup")
    for index, binding in enumerate(snapshot["bindings"]):
        ET.SubElement(group, "Compile", Include=binding, Link="TestProviderBinding-" + str(index) + "-" + Path(binding).name)
    ET.indent(tree); tree.write(project, encoding="unicode")
    for name, expected in before.items():
        if name.endswith(".cs") and hashes([raw / name])[str(raw / name)] != expected:
            raise RuntimeError("Repair changed generated C# bytes")
    authored = authored_sources(ROOT)
    authored.update(hashes(p for p in (ROOT / "tests/Upstream").rglob("*") if p.is_file() and p.suffix in (".cs", ".json", ".h")))
    authored.update(snapshot["snapshot_inputs_sha256"])
    frozen = {**immutable, **authored}
    row = dict(failed, status="unrun", project=str(project), provider_snapshot=snapshot)
    repaired = dict(manifest, status="failed", commands=[], executables=[row],
                    evidence_kind="fresh-raw-build-from-retained-failed-linked-project",
                    retained_failed_manifest={"path": str(args.executables.resolve()), "sha256": hashes([args.executables])[str(args.executables)]},
                    retained_failed_command=failed_commands[0],
                    source_snapshot={"original_project": str(original), "original_source_sha256": before,
                                     "private_project": str(project), "private_source_sha256": tree_hashes(raw)},
                    provider_source_freeze={"receipt": str(args.provider_freeze.resolve()), "sha256": hashes([args.provider_freeze])[str(args.provider_freeze)], "after_sha256": freeze["after_sha256"]})

    def run(argv, label):
        argv = list(map(str, argv)); log = artifacts / (label + ".log")
        with log.open("w") as output:
            result = subprocess.run(argv, cwd=ROOT, stdout=output, stderr=subprocess.STDOUT, timeout=1200)
        repaired["commands"].append({"command": argv, "exit_code": result.returncode, "log": str(log)})
        if result.returncode:
            raise RuntimeError("Actual retained-project build failed: " + str(log))
        if hashes(frozen) != frozen or tree_hashes(original.parent) != before:
            raise RuntimeError("Frozen/live inputs changed during actual retained-project build")
        return log

    try:
        target = work / Path(args.case).stem / "raw/jit"
        run(["dotnet", "build", project, "-c", "Release", "--artifacts-path", target / "artifacts", "-m:1",
             "-p:RestoreLockedMode=true", "-p:UseSharedCompilation=false"], "raw-build")
        executable = target / "artifacts/bin/S2nTls/release" / (Path(args.case).stem + ".dll")
        launch = ["dotnet", str(executable)]
        runtime = json.loads(run([*launch, "--identity"], "raw-identity").read_text())
        identity = capture(launch, target / "artifacts", runtime, authored,
                           {"campaign_run": manifest["campaign_run"], "core_tu_coverage": failed["core_tu_coverage"], "test_provider_snapshot": snapshot})
        identity_path = target / "execution-identity.json"
        identity_path.write_text(json.dumps(identity, indent=2) + "\n")
        row.update(status="built", command=launch, application_execution_receipt=str(identity_path),
                   application_execution_receipt_sha256=hashes([identity_path])[str(identity_path)],
                   execution_sha256=hashes(p for p in executable.parent.iterdir() if p.is_file()))
        for key in ("compiled_source", "compiled_sha256", "original_sha256", "dependency_file", "dependency_sha256", "dependency_inputs_sha256"):
            row[key] = unit[key]
        row.pop("error", None)
        repaired["status"] = "built"
    except Exception as error:
        row.update(status="failed", error=str(error))
        repaired["error"] = str(error)
    args.output.parent.mkdir(parents=True, exist_ok=True)
    args.output.write_text(json.dumps(repaired, indent=2) + "\n")
    print("RETAINED LINKED REPAIR", args.output, repaired["status"], flush=True)
    return 0 if repaired["status"] == "built" else 1


if __name__ == "__main__":
    raise SystemExit(main())
