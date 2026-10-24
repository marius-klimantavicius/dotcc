#!/usr/bin/env python3
"""Reuse already linked original C harnesses for all four execution modes.

This performs no C emission/linking and makes no handwritten generated-C# edits.
The immutable campaign postprocessor alone transforms private processed trees.
"""
import argparse
from concurrent.futures import ThreadPoolExecutor
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import threading
import uuid
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.delivery import ignore_build_caches, source_files
from campaigns.translation import relocate_project
sys.path.insert(0, str(ROOT / "tests/Managed"))
from execution_identity import authored_sources, capture, hashes
from native_profile import validate_profile
sys.path.insert(0, str(ROOT / "scripts"))
from translation_evidence import read_closed
sys.path.insert(0, str(ROOT / "tests/Provider/UpstreamSurface"))
from provider_snapshot import snapshot_provider
from qualified_dependencies import deduplicate_qualified_case, storage_reserves, validate_work_root


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def tree_hashes(directory):
    return {p.relative_to(directory).as_posix(): digest(p) for p in source_files(directory)}


def raw_metadata_imports(directory):
    """Create or preserve only the exact original metadata import bundle."""
    planned = []
    for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props"):
        original = next((root / name for root in (ROOT, ROOT.parent) if (root / name).is_file()), None)
        target = directory / name
        if target.is_symlink() or target.exists() and not target.is_file():
            raise RuntimeError("Private raw metadata must be a regular file without symlinks")
        if original is None:
            if target.exists():
                raise RuntimeError("Private raw metadata has no matching original import")
            continue
        tree = ET.Element("Project")
        ET.SubElement(tree, "Import", Project=str(original))
        ET.indent(tree)
        expected = (ET.tostring(tree, encoding="unicode") + "\n").encode("utf-8")
        planned.append((target, original, expected))
    present = [target.exists() for target, _, _ in planned]
    if any(present) and not all(present):
        raise RuntimeError("Private raw metadata import bundle is incomplete")
    # Validate every existing file before creating anything. In particular,
    # never overwrite unknown properties/targets or repair altered input bytes.
    for target, original, expected in planned:
        if target.exists() and target.read_bytes() != expected:
            raise RuntimeError("Private raw metadata differs from its exact original import XML")
    result = []
    for target, original, expected in planned:
        if not target.exists():
            with target.open("xb") as stream:
                stream.write(expected)
        result.append({"path": str(target), "sha256": digest(target),
                       "import": str(original), "import_sha256": digest(original)})
    return result

class Expansion:
    def __init__(self, args):
        self.args = args
        self.work_root = validate_work_root(args.work_root) if args.work_root else None
        self.output = args.output.resolve()
        self.output.parent.mkdir(parents=True, exist_ok=True)
        self.input = json.loads(args.executables.read_text())
        self.campaign = Path(self.input["campaign_run"])
        self.campaign_receipt_path, self.campaign_receipt = read_closed(ROOT, self.campaign)
        self.lock = threading.RLock()
        inventory = json.loads((ROOT / "config/upstream-tests.json").read_text())
        selected = inventory["native_control_tests"]
        self.cases = ["tests/unit/" + name + ".c" for name in selected]
        if len(self.cases) != 16 or len(set(self.cases)) != 16:
            raise RuntimeError("Expected the exact 16 original controls")
        self.required = [r["path"] for r in inventory["cases"] if r["status"].startswith("required")]
        self.required_c = [case for case in self.required if case.endswith(".c")]
        if args.case_set == "required-c":
            self.cases = [case for case in self.required_c if not args.case or case in args.case]
        if args.case and set(args.case) - set(self.cases):
            raise RuntimeError("Expansion case is outside the declared original C case set")
        self.selected = [case for case in self.cases if not args.case or case in args.case]
        self.entries = {}
        for case in self.cases:
            rows = [r for r in self.input["executables"] if r["case"] == case and r["form"] == "raw" and r["mode"] == "jit"]
            if len(rows) != 1 or rows[0]["status"] != "built":
                raise RuntimeError("Original raw JIT harness is missing: " + case)
            self.entries[case] = rows[0]
        if args.resume:
            self.report = json.loads(self.output.read_text())
            if self.report["input_manifest_sha256"] != digest(args.executables):
                raise RuntimeError("Expansion input manifest changed")
            if self.report.get("case_set", "selected16") != args.case_set or {r["case"] for r in self.report["executables"]} != set(self.cases):
                raise RuntimeError("Resume must retain the exact declared expansion case set")
            if self.report.get("refresh_provider", False) != args.refresh_provider:
                raise RuntimeError("Resume must retain the exact provider refresh policy")
            self.work = Path(self.report["work"])
        else:
            if self.output.exists():
                raise RuntimeError("Output exists; use --resume to retain completed builds")
            run = datetime.now(timezone.utc).strftime("%Y%m%dT%H%M%SZ") + "-" + uuid.uuid4().hex[:8]
            self.work = (self.work_root or ROOT / "build") / "provider/original16-expanded" / run
            reserve = storage_reserves([self.output.parent, self.work], args.min_free_gib * 2**30)
            if not all(row["passed"] for row in reserve):
                raise RuntimeError("External/metadata filesystem is below the recorded expansion reserve: " + json.dumps(reserve))
            self.work.mkdir(parents=True, mode=0o700)
            receipt_path, receipt = self.campaign_receipt_path, self.campaign_receipt
            tools = hashes(p for p in (self.campaign / "tools").rglob("*") if p.is_file() and p.suffix in (".dll", ".json"))
            self.report = {"status": "running", "scope": "Bounded original C harness expansion; whole upstream gate remains open",
                "whole_upstream_status": "open", "campaign_run": str(self.campaign), "work": str(self.work),
                "case_set": args.case_set, "target_case_count": len(self.cases), "required_c_case_count": len(self.required_c),
                "required_upstream_case_count": len(self.required), "remaining_required_cases": [p for p in self.required if p not in self.cases],
                "input_manifest": str(args.executables.resolve()), "input_manifest_sha256": digest(args.executables),
                "campaign_receipt": str(receipt_path), "campaign_receipt_sha256": digest(receipt_path), "tool_sha256": tools,
                "product_objects_sha256": self.input["product_objects_sha256"],
                "product_staged_sources_sha256": hashes(Path(p) for p in receipt["units"]),
                "override_inputs_sha256": self.input["override_inputs_sha256"],
                "helper_sha256": digest(Path(__file__)), "snapshots": [], "commands": [],
                "executables": [dict(self.entries[case], form=form, mode=mode, status="unrun")
                    for case in self.cases for form in ("raw", "processed") for mode in ("jit", "aot")]}
            if self.work_root is not None:
                self.report.update(work_root=str(self.work_root), work_device=self.work.stat().st_dev,
                                   storage_reserves_at_allocation=reserve)
            if len(self.report["product_objects_sha256"]) != 175 or len(self.report["product_staged_sources_sha256"]) != 175:
                raise RuntimeError("Expansion requires the complete 175-unit product")
        self.postprocessor = self.campaign / "tools/DotCC.PostProcess/dotcc-postprocess.dll"
        if args.resume:
            # Existing absolute placement remains authoritative, even when a
            # later controller selects another root for future batches.
            self.work_root = Path(self.report["work_root"]) if self.report.get("work_root") else None
            if self.work_root is not None:
                validate_work_root(self.work_root)
            reserve = storage_reserves([self.output.parent, self.work], args.min_free_gib * 2**30)
            if not all(row["passed"] for row in reserve):
                raise RuntimeError("Recorded expansion filesystem is below reserve: " + json.dumps(reserve))
        self.native_report = (args.native_report or Path(self.report.get("native_report", ""))).resolve()
        if not self.native_report.is_file():
            raise RuntimeError("Provide the exact matching-profile --native-report")
        if "native_report_sha256" in self.report and digest(self.native_report) != self.report["native_report_sha256"]:
            raise RuntimeError("Frozen native comparison receipt changed")
        self.report.update(native_report=str(self.native_report), native_report_sha256=digest(self.native_report))
        self.native_equivalence = args.native_equivalence or self.report.get("native_equivalence")
        if self.native_equivalence:
            self.native_equivalence = Path(self.native_equivalence).resolve()
            self.report["native_equivalence"] = str(self.native_equivalence)
        self.artifacts = Path(self.report.get("artifacts", str(ROOT / "artifacts/provider/original16-expanded" / self.work.name)))
        self.artifacts.mkdir(parents=True, exist_ok=True)
        self.report["artifacts"] = str(self.artifacts)
        self.report.setdefault("qualifications", {})
        self.report.setdefault("case_failures", {})
        self.report.setdefault("cache_cleanup", {})
        self.report["refresh_provider"] = args.refresh_provider
        if args.refresh_provider and not args.provider_freeze:
            raise RuntimeError("Provider refresh needs its explicit reviewed source freeze")
        if args.provider_freeze:
            freeze_path = args.provider_freeze.resolve()
            freeze = json.loads(freeze_path.read_text())
            identity = {"receipt": str(freeze_path), "sha256": digest(freeze_path), "after_sha256": freeze["after_sha256"]}
            if "provider_source_freeze" in self.report and self.report["provider_source_freeze"] != identity:
                raise RuntimeError("Provider refresh cannot change its final source freeze")
            self.report["provider_source_freeze"] = identity
        if self.report["campaign_receipt_sha256"] != digest(self.campaign_receipt_path):
            raise RuntimeError("Closed translation identity changed")
        self.report.setdefault("campaign_receipt", str(self.campaign_receipt_path))
        self.report.setdefault("helper_revisions_sha256", []).append(digest(Path(__file__)))
        self.report.update(case_set=args.case_set, target_case_count=len(self.cases),
                           required_c_case_count=len(self.required_c), required_upstream_case_count=len(self.required),
                           remaining_required_cases=[p for p in self.required if p not in self.cases],
                           future_build_policy={"reuse_identity_raw_jit": True, "aot_debug_symbols": args.aot_debug_symbols})
        native = json.loads(self.native_report.read_text())
        self.report["native_profile_match"] = validate_profile(ROOT, self.campaign, self.native_report, self.native_equivalence)
        for case, entry in self.entries.items():
            matches = [r for r in native["upstream"] if r.get("case") == case and r["status"] == "passed"]
            if len(matches) != 1 or matches[0]["original_sha256"] != entry["original_sha256"]:
                raise RuntimeError("Native successful original source identity is missing: " + case)
            if matches[0].get("test_adaptations", []) != entry.get("test_adaptations", []):
                raise RuntimeError("Native/translated exact test adaptation receipts differ: " + case)
        self.verify_immutable()
        self.save()

    def verify_immutable(self):
        if "work_device" in self.report and self.work.stat().st_dev != self.report["work_device"]:
            raise RuntimeError("Recorded external work filesystem changed; preserve existing absolute paths")
        if digest(self.campaign_receipt_path) != self.report["campaign_receipt_sha256"]:
            raise RuntimeError("Closed translation receipt changed during expansion")
        for key in ("tool_sha256", "product_objects_sha256", "product_staged_sources_sha256", "override_inputs_sha256"):
            if hashes(self.report[key]) != self.report[key]:
                raise RuntimeError("Immutable expansion input changed: " + key)
        for entry in self.entries.values():
            snapshot = entry.get("provider_snapshot")
            if snapshot and hashes(snapshot["snapshot_inputs_sha256"]) != snapshot["snapshot_inputs_sha256"]:
                raise RuntimeError("Frozen additional test provider input changed: " + entry["case"])
        freeze = self.report.get("provider_source_freeze")
        if freeze and (digest(freeze["receipt"]) != freeze["sha256"] or hashes(freeze["after_sha256"]) != freeze["after_sha256"]):
            raise RuntimeError("Reviewed final provider source freeze changed")
        for row in self.report["executables"]:
            snapshot = row.get("provider_snapshot")
            if snapshot and hashes(snapshot["snapshot_inputs_sha256"]) != snapshot["snapshot_inputs_sha256"]:
                raise RuntimeError("Actual refreshed provider snapshot changed")

    def refresh_project_provider(self, case, project):
        """Change only authored project inputs; retain every generated C# byte."""
        entry = self.entries[case]
        old = entry.get("provider_snapshot")
        tree = ET.parse(project)
        references = list(tree.iter("ProjectReference"))
        expected = Path(old["project"]) if old else ROOT / "src/CryptoProvider/CryptoProvider.csproj"
        if len(references) != 1 or (project.parent / references[0].get("Include")).resolve() != expected.resolve():
            raise RuntimeError("Retained raw harness has an unexpected provider reference")
        fresh = None
        if old:
            repository = Path(old["project"]).parents[4]
            original_bindings = [ROOT.parent / Path(p).relative_to(repository) for p in old["bindings"]]
            fresh = snapshot_provider(ROOT / "tests/Provider/UpstreamSurface/CryptoProvider.Upstream.csproj",
                                      original_bindings, self.work / Path(case).stem / "provider-inputs")
            selected = Path(fresh["project"])
            bindings = dict(zip(old["bindings"], fresh["bindings"]))
            for node in tree.iter("Compile"):
                original = str((project.parent / node.get("Include")).resolve())
                if original in bindings:
                    node.set("Include", bindings[original])
        else:
            selected = ROOT / "src/CryptoProvider/CryptoProvider.csproj"
        references[0].set("Include", str(selected))
        ET.indent(tree); tree.write(project, encoding="unicode")
        for row in self.report["executables"]:
            if row["case"] != case:
                continue
            if fresh:
                row["provider_snapshot"] = fresh
            row["provider_source_freeze"] = self.report["provider_source_freeze"]
        return {"previous_provider_project": str(expected), "selected_provider_project": str(selected),
                "provider_snapshot": fresh, "previous_raw_jit_identity": entry.get("application_execution_receipt"),
                "previous_raw_jit_identity_sha256": entry.get("application_execution_receipt_sha256"),
                "raw_jit_policy": "always build and execute fresh; old-provider identity is historical"}

    def save(self):
        with self.lock:
            self.report["counts"] = {status: sum(r["status"] == status for r in self.report["executables"])
                                     for status in ("built", "failed", "unrun")}
            temporary = self.output.with_suffix(".json.tmp")
            temporary.write_text(json.dumps(self.report, indent=2) + "\n")
            temporary.replace(self.output)

    def command(self, argv, label, timeout=1200):
        log = self.work / "logs" / (label + ".log")
        log.parent.mkdir(exist_ok=True)
        if log.exists():
            log = log.with_name(log.stem + "-" + uuid.uuid4().hex[:8] + ".log")
        argv = list(map(str, argv))
        with log.open("w") as stream:
            try:
                result = subprocess.run(argv, cwd=ROOT, stdout=stream, stderr=subprocess.STDOUT, timeout=timeout)
                code = result.returncode
            except subprocess.TimeoutExpired:
                code = "timeout"
        with self.lock:
            self.report["commands"].append({"command": argv, "exit_code": code, "log": str(log)})
            self.save()
        if code != 0:
            raise RuntimeError("Private expansion command failed: " + str(log))
        return log

    def snapshot(self, case):
        entry = self.entries[case]
        for path, expected in entry["execution_sha256"].items():
            if digest(path) != expected:
                raise RuntimeError("Original harness execution closure changed: " + path)
        original = ROOT / "ref/s2n-tls" / case
        if digest(original) != entry["original_sha256"] or digest(entry["compiled_source"]) != entry["compiled_sha256"]:
            raise RuntimeError("Original C harness source changed")
        source = Path(entry["compiled_source"]).parents[2]  # private source tree
        if entry.get("project"):
            origin = Path(entry["project"]).parent
        else:
            # Reused C emission keeps its original compiled_source path. The
            # actual raw project's location comes from the recorded build,
            # rather than assuming that emission and linking share a folder.
            executable = Path(entry["command"][1] if entry["command"][0] == "dotnet" else entry["command"][0]).resolve()
            projects = set()
            for observation in self.input.get("commands", []):
                argv = observation["command"]
                if "--artifacts-path" not in argv or not executable.is_relative_to(Path(argv[argv.index("--artifacts-path") + 1]).resolve()):
                    continue
                projects.update(Path(arg).resolve().parent for arg in argv if arg.endswith(".csproj") and not arg.startswith("-"))
            if len(projects) > 1:
                raise RuntimeError("Ambiguous actual raw project in recorded build commands")
            origin = next(iter(projects)) if projects else source.parent / Path(case).stem / "raw/S2nTls"
        if not (origin / "S2nTls.csproj").is_file():
            raise RuntimeError("Existing linked raw harness project missing: " + str(origin))
        if entry.get("source_sha256") and tree_hashes(origin) != entry["source_sha256"]:
            raise RuntimeError("Actual raw application source/project/metadata no longer matches its build receipt")
        raw = self.work / Path(case).stem / "raw/S2nTls"
        recorded = next((r for r in self.report["snapshots"] if r["case"] == case), None)
        if recorded:
            raw = Path(recorded["raw_project"]).parent
            if tree_hashes(origin) != recorded["original_raw_source_sha256"] or tree_hashes(raw) != recorded["raw_source_sha256"]:
                raise RuntimeError("Frozen private raw harness changed")
            return raw
        before = tree_hashes(origin)
        origin_tree = ET.parse(origin / "S2nTls.csproj")
        identity = ROOT / "samples/ManagedConsumer/ExecutionIdentity.cs"
        if (not self.args.refresh_provider and entry.get("application_execution_receipt")
            and any("UPSTREAM_EXECUTION_IDENTITY" in (n.text or "") for n in origin_tree.iter("DefineConstants"))
            and any(n.get("Include") == str(identity) for n in origin_tree.iter("Compile"))):
            # New raw harnesses already carry actual identity. Their immutable
            # project/source tree can serve raw AOT directly, with all build
            # outputs in this expansion's private --artifacts-path. Avoid a
            # redundant 13-MiB source copy for each of the 257 original cases.
            row = {"case": case, "original_raw_project": str(origin / "S2nTls.csproj"),
                   "original_raw_source_sha256": before, "raw_project": str(origin / "S2nTls.csproj"),
                   "raw_source_sha256": before, "raw_source_strategy": "reuse immutable identity-enabled input project"}
            with self.lock:
                self.report["snapshots"].append(row)
                self.save()
            return origin
        if raw.exists():
            failed = raw.parent.with_name("raw-failed-" + uuid.uuid4().hex[:8])
            raw.parent.rename(failed)
            self.report.setdefault("retained_partial_raw_snapshots", []).append(str(failed))
            self.save()
        shutil.copytree(origin, raw, ignore=ignore_build_caches)
        if tree_hashes(raw) != before or tree_hashes(origin) != before:
            raise RuntimeError("Raw snapshot copy overlapped a source mutation")
        project = raw / "S2nTls.csproj"
        relocate_project(project, origin)
        tree = ET.parse(project)
        props = ET.SubElement(tree.getroot(), "PropertyGroup")
        ET.SubElement(props, "DefineConstants").text = "$(DefineConstants);UPSTREAM_EXECUTION_IDENTITY"
        if not any(n.get("Include") == str(identity) for n in tree.iter("Compile")):
            group = ET.SubElement(tree.getroot(), "ItemGroup")
            ET.SubElement(group, "Compile", Include=str(identity), Link="ExecutionIdentity.cs")
        ET.indent(tree)
        tree.write(project, encoding="unicode")
        metadata = raw_metadata_imports(raw) if self.work_root is not None else []
        refreshed = self.refresh_project_provider(case, project) if self.args.refresh_provider else None
        # Project XML is the sole authored change; all generated C# stays exact.
        for name, expected in before.items():
            if name.endswith(".cs") and digest(raw / name) != expected:
                raise RuntimeError("Generated C# changed while adding executable identity")
        row = {"case": case, "original_raw_project": str(origin / "S2nTls.csproj"),
               "original_raw_source_sha256": before, "raw_project": str(project),
               "raw_source_sha256": tree_hashes(raw)}
        if metadata:
            row["raw_metadata_imports"] = metadata
        if refreshed:
            row["provider_refresh"] = refreshed
        with self.lock:
            self.report["snapshots"].append(row)
            self.save()
        return raw

    def processed(self, case, raw):
        destination = self.work / Path(case).stem / "processed/S2nTls"
        row = next(r for r in self.report["snapshots"] if r["case"] == case)
        if "processed_source_sha256" in row:
            if tree_hashes(destination) != row["processed_source_sha256"]:
                raise RuntimeError("Private processed source changed")
            return destination
        if destination.exists():
            failed = destination.parent.with_name("processed-failed-" + uuid.uuid4().hex[:8])
            destination.parent.rename(failed)
            with self.lock:
                row.setdefault("retained_failed_processed_attempts", []).append(str(failed))
                self.save()
        shutil.copytree(raw, destination, ignore=ignore_build_caches)
        project = destination / "S2nTls.csproj"
        relocate_project(project, raw)
        semantic = destination.parents[1] / "semantic-artifacts"
        props = ET.Element("Project")
        ET.SubElement(props, "Import", Project=str(ROOT.parent / "Directory.Build.props"))
        group = ET.SubElement(props, "PropertyGroup")
        for key, value in {"UseArtifactsOutput": "true", "ArtifactsPath": str(semantic),
                           "RestoreLockedMode": "true", "UseSharedCompilation": "false",
                           "GeneratedMSBuildEditorConfigFile": str(destination / "obj/Release/S2nTls.GeneratedMSBuildEditorConfig.editorconfig")}.items():
            ET.SubElement(group, key).text = value
        ET.indent(props)
        ET.ElementTree(props).write(destination / "Directory.Build.props", encoding="unicode")
        (destination / "obj/Release").mkdir(parents=True)
        tree = ET.parse(project)
        linked = []
        linked_directory = destination.parents[1] / ("semantic-linked-" + uuid.uuid4().hex[:8])
        linked_directory.mkdir()
        for index, node in enumerate(tree.iter("Compile")):
            value = node.get("Include")
            if not value or "$" in value or "*" in value:
                raise RuntimeError("Unhandled authored Compile item in private harness")
            authored = (project.parent / value).resolve()
            private = linked_directory / (str(index) + "-" + authored.name)
            shutil.copyfile(authored, private)
            linked.append((node, value, private, digest(authored)))
            node.set("Include", str(private))
        for node in tree.iter("ProjectReference"):
            node.set("AdditionalProperties", "ArtifactsPath=" + str(semantic) + ";UseArtifactsOutput=true")
        ET.indent(tree)
        tree.write(project, encoding="unicode")
        authored_before = authored_sources(ROOT)
        snapshot = next(r for r in self.report["executables"] if r["case"] == case).get("provider_snapshot")
        if snapshot:
            authored_before.update(snapshot["snapshot_inputs_sha256"])
        label = Path(case).stem
        self.command(["dotnet", "build", project, "-c", "Release", "--artifacts-path", semantic,
                      "-p:RestoreLockedMode=true", "-p:UseSharedCompilation=false", "-m:1"], label + "-semantic")
        self.command(["dotnet", self.postprocessor, project, "--in-place"], label + "-postprocess", timeout=900)
        if hashes(authored_before) != authored_before:
            raise RuntimeError("Shared authored source changed during private semantic processing")
        for node, original, private, expected in linked:
            if digest(private) != expected:
                raise RuntimeError("Postprocessor transformed an authored linked source; private copy retained: " + str(private))
            node.set("Include", original)
        ET.indent(tree)
        tree.write(project, encoding="unicode")
        with self.lock:
            row.update(processed_project=str(project), semantic_linked_source_sha256={str(p): h for _, _, p, h in linked},
                       processed_source_sha256=tree_hashes(destination))
            self.save()
        return destination

    def build_case(self, case):
        rows = [r for r in self.report["executables"] if r["case"] == case]
        try:
            self.verify_immutable()
            if case in self.report["qualifications"]:
                qualification = self.report["qualifications"][case]
                if digest(qualification["receipt"]) != qualification["sha256"]:
                    raise RuntimeError("Qualified original case receipt changed")
                self.cleanup_caches(case, rows)
                return
            raw = self.snapshot(case)
            entry = self.entries[case]
            identity_path = entry.get("application_execution_receipt")
            raw_jit = next(r for r in rows if r["form"] == "raw" and r["mode"] == "jit")
            if raw_jit["status"] != "built" and identity_path and not self.args.refresh_provider:
                if digest(identity_path) != entry["application_execution_receipt_sha256"]:
                    raise RuntimeError("Existing raw JIT application identity receipt changed")
                identity = json.loads(Path(identity_path).read_text())
                if identity["actual_runtime"]["kind"] != "actual-runtime-identity" or hashes(identity["output_sha256"]) != identity["output_sha256"]:
                    raise RuntimeError("Existing raw JIT actual execution identity is invalid")
                freeze = self.report.get("provider_source_freeze")
                if freeze and (identity["product_snapshot"].get("provider_source_freeze") != freeze
                    or any(identity["authored_source_sha256"].get(p) != value for p, value in freeze["after_sha256"].items()
                           if Path(p).is_relative_to(ROOT / "src/CryptoProvider") and Path(p).suffix in (".cs", ".csproj", ".json"))):
                    raise RuntimeError("New raw application lacks the exact current provider identity")
                snapshot = next(r for r in self.report["snapshots"] if r["case"] == case)
                origin = Path(snapshot["original_raw_project"])
                original_tree = ET.parse(origin)
                if not any("UPSTREAM_EXECUTION_IDENTITY" in (n.text or "") for n in original_tree.iter("DefineConstants")):
                    raise RuntimeError("Input execution receipt lacks an identity-enabled original harness project")
                with self.lock:
                    raw_jit.update(entry, status="built", project=str(origin), source_sha256=snapshot["original_raw_source_sha256"],
                                   reused_from_input_manifest=True)
                    self.save()
                print("REUSED", case, "raw jit with actual identity", flush=True)
            for form in ("raw", "processed"):
                directory = raw if form == "raw" else self.processed(case, raw)
                for mode in ("jit", "aot"):
                    row = next(r for r in rows if r["form"] == form and r["mode"] == mode)
                    if row["status"] == "built":
                        if hashes(row["execution_sha256"]) != row["execution_sha256"]:
                            raise RuntimeError("Previously built private execution closure changed")
                        continue
                    name = Path(case).stem
                    target = self.work / Path(case).stem / form / mode
                    artifacts = target / "artifacts"
                    argv = ["dotnet", "publish" if mode == "aot" else "build", directory / "S2nTls.csproj", "-c", "Release",
                            "--artifacts-path", artifacts, "-m:1", "-p:RestoreLockedMode=true", "-p:UseSharedCompilation=false"]
                    if mode == "aot":
                        argv += ["-r", "linux-x64", "--self-contained", "true", "-p:PublishAot=true", "-o", target / "publish"]
                        if not self.args.aot_debug_symbols:
                            argv += ["-p:DebugSymbols=false", "-p:DebugType=None"]
                    authored = authored_sources(ROOT)
                    authored.update(hashes(p for p in (ROOT / "tests/Upstream").rglob("*")
                                          if p.is_file() and p.suffix in (".cs", ".json", ".h")))
                    snapshot = row.get("provider_snapshot")
                    if snapshot:
                        authored.update(snapshot["snapshot_inputs_sha256"])
                    frozen = tree_hashes(directory)
                    self.command(argv, name + "-" + form + "-" + mode)
                    if hashes(authored) != authored or tree_hashes(directory) != frozen:
                        raise RuntimeError("Private generated or shared authored source changed during application build")
                    executable = target / "publish" / name if mode == "aot" else artifacts / "bin/S2nTls/release" / (name + ".dll")
                    launch = [str(executable)] if mode == "aot" else ["dotnet", str(executable)]
                    identity_log = self.command([*launch, "--identity"], name + "-" + form + "-" + mode + "-identity", timeout=120)
                    runtime = json.loads(identity_log.read_text())
                    record = capture(launch, artifacts, runtime, authored,
                                     {"campaign_run": str(self.campaign), "core_tu_coverage": row.get("core_tu_coverage"),
                                      "reused_input_manifest_sha256": self.report["input_manifest_sha256"],
                                      "test_provider_snapshot": snapshot,
                                      "provider_source_freeze": self.report.get("provider_source_freeze")})
                    identity_path = target / "execution-identity.json"
                    identity_path.write_text(json.dumps(record, indent=2) + "\n")
                    with self.lock:
                        row.update(status="built", command=launch, project=str(directory / "S2nTls.csproj"),
                                   source_sha256=frozen, application_execution_receipt=str(identity_path),
                                   application_execution_receipt_sha256=digest(identity_path),
                                   execution_sha256=hashes(p for p in executable.parent.iterdir() if p.is_file()))
                        row.pop("error", None)
                        self.save()
                    print("BUILT", case, form, mode, flush=True)
            self.verify_immutable()
            qualification = self.artifacts / (Path(case).stem + ".json")
            self.command([sys.executable, "-B", ROOT / "tests/Provider/run-original-controls.py",
                          "--executables", self.output, "--native-report", self.native_report,
                          *(["--native-equivalence", self.native_equivalence] if self.native_equivalence else []),
                          "--case-set", self.args.case_set, "--case", case,
                          "--form", "all", "--mode", "all", "--output", qualification],
                         Path(case).stem + "-assertions", timeout=900)
            observed = json.loads(qualification.read_text())
            if observed["status"] != "bounded-passed" or observed["counts"] != {"passed": 4, "failed": 0}:
                raise RuntimeError("Original case did not pass all four exact native comparisons")
            with self.lock:
                self.report["qualifications"][case] = {"receipt": str(qualification), "sha256": digest(qualification)}
                self.report["case_failures"].pop(case, None)
                self.save()
            self.cleanup_caches(case, rows)
            print("QUALIFIED", case, "all4", flush=True)
        except Exception as error:
            with self.lock:
                self.report["qualifications"].pop(case, None)
                self.report["case_failures"][case] = str(error)
                for row in rows:
                    if row["status"] != "built":
                        row.update(status="failed", error=str(error))
                self.save()
            print("FAILED", case, str(error), flush=True)

    def cleanup_caches(self, case, rows):
        # Only a fully qualified, inactive case owns these rebuildable caches.
        protected = set()
        for row in rows:
            identity = json.loads(Path(row["application_execution_receipt"]).read_text())
            for key in ("output_sha256", "runtime_closure_sha256"):
                protected.update(Path(p).resolve() for p in identity[key])
        base = self.work / Path(case).stem
        candidates = [base / form / mode / "artifacts/obj" for form in ("raw", "processed") for mode in ("jit", "aot")]
        original_identity = self.entries[case].get("application_execution_receipt")
        if original_identity:
            # The pipeline's raw batch has finished before this case qualifies.
            # Its obj cache is separate from the retained raw JIT output.
            candidates.append(Path(original_identity).parent / "artifacts/obj")
        # AOT's prepublish managed/self-contained bin tree is a compilation
        # input cache. The executed native binary and all of its mapped/output
        # files live in publish and are protected above. Keep build-input hash
        # inventories in their receipts; remove these copies only when none
        # belongs to an actual execution/runtime closure. JIT bin stays intact.
        candidates.extend(base / form / "aot/artifacts/bin" for form in ("raw", "processed"))
        candidates.append(base / "semantic-artifacts")
        removed = []
        for candidate in candidates:
            if not candidate.is_dir():
                continue
            if any(candidate.resolve() in path.parents for path in protected):
                removed.append({"path": str(candidate), "retained": "Actual execution/runtime closure references this directory"})
                continue
            size = sum(p.stat().st_size for p in candidate.rglob("*") if p.is_file())
            shutil.rmtree(candidate)
            removed.append({"path": str(candidate), "rebuildable_bytes_removed": size})
        for row in rows:
            if hashes(row["execution_sha256"]) != row["execution_sha256"]:
                raise RuntimeError("Cache cleanup changed the actual execution closure")
        with self.lock:
            self.report["cache_cleanup"].setdefault(case, []).extend(removed)
            self.save()
        # This case's four assertion processes have exited. Keep every actual
        # output path and byte while sharing only the verified pinned BC DLL.
        qualified = self.report["qualifications"][case]
        if digest(qualified["receipt"]) != qualified["sha256"]:
            raise RuntimeError("Closed qualification changed before dependency storage cleanup")
        receipt = self.artifacts / (Path(case).stem + "-dependencies-" + uuid.uuid4().hex[:8] + ".json")
        storage = deduplicate_qualified_case(rows, qualified["receipt"], receipt, work_root=self.work_root)
        with self.lock:
            self.report.setdefault("dependency_storage", {}).setdefault(case, []).append({
                "receipt": str(receipt), "sha256": digest(receipt), "status": storage["status"],
                "allocated_bytes_released": storage["allocated_bytes_released"]})
            self.save()

    def run(self):
        if self.args.validate_only:
            self.report["status"] = "inputs-validated"
            self.save()
            print("INPUTS VALIDATED", len(self.cases), "of", len(self.required_c), "required C cases; execution remains unrun", flush=True)
            return 0
        with ThreadPoolExecutor(max_workers=self.args.jobs) as workers:
            list(workers.map(self.build_case, self.selected))
        self.verify_immutable()
        self.report["status"] = "qualified" if len(self.report["qualifications"]) == len(self.cases) else "incomplete"
        if self.report["status"] == "qualified":
            self.report["qualification_receipt"] = str(self.artifacts / "all4.json")
        self.save()
        if self.report["status"] == "qualified":
            try:
                self.final_receipt()
            except Exception as error:
                self.report.update(status="failed", final_audit_error=str(error))
                self.save()
                raise
        print("MANIFEST", self.output, flush=True)
        return 0 if all(case in self.report["qualifications"] for case in self.selected) else 1

    def final_receipt(self):
        cases = []
        for case in self.cases:
            qualified = self.report["qualifications"][case]
            if digest(qualified["receipt"]) != qualified["sha256"]:
                raise RuntimeError("Per-case original assertion receipt changed")
            evidence = json.loads(Path(qualified["receipt"]).read_text())
            if evidence["counts"] != {"passed": 4, "failed": 0}:
                raise RuntimeError("Missing a successful execution form/mode")
            cases.extend(evidence["cases"])
        for row in self.report["executables"]:
            if hashes(row["execution_sha256"]) != row["execution_sha256"]:
                raise RuntimeError("Final actual application closure changed")
            if digest(row["application_execution_receipt"]) != row["application_execution_receipt_sha256"]:
                raise RuntimeError("Final actual application identity receipt changed")
            if tree_hashes(Path(row["project"]).parent) != row["source_sha256"]:
                raise RuntimeError("Final generated source/project closure changed")
            snapshot = row.get("provider_snapshot")
            if snapshot and hashes(snapshot["snapshot_inputs_sha256"]) != snapshot["snapshot_inputs_sha256"]:
                raise RuntimeError("Final frozen additional test provider changed")
        receipt = {"status": "bounded-passed", "whole_upstream_status": "open", "case_set": self.args.case_set,
                   "scope": "All four modes of the explicit original C subset; remaining required inventory stays open",
                   "campaign_run": str(self.campaign), "selected_case_count": len(self.cases),
                   "campaign_receipt": str(self.campaign_receipt_path),
                   "campaign_receipt_sha256": self.report["campaign_receipt_sha256"],
                   "required_c_case_count": len(self.required_c), "required_upstream_case_count": len(self.required),
                   "remaining_required_cases": self.report["remaining_required_cases"], "cases": cases,
                   "counts": {"passed": len(cases), "failed": 0}, "per_case_receipts": self.report["qualifications"],
                   "executable_manifest": str(self.output), "executable_manifest_sha256": digest(self.output),
                   "native_report": str(self.native_report), "native_report_sha256": digest(self.native_report)}
        Path(self.report["qualification_receipt"]).write_text(json.dumps(receipt, indent=2) + "\n")


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--executables", type=Path, required=True)
    parser.add_argument("--work-root", type=Path, help="Private future build root replacing ROOT/build; existing resumed work remains at its recorded path")
    parser.add_argument("--min-free-gib", type=float, default=2, help="Reserve on each metadata/work filesystem before allocation or resume")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--native-report", type=Path)
    parser.add_argument("--native-equivalence", type=Path)
    parser.add_argument("--case", action="append")
    parser.add_argument("--case-set", choices=("selected16", "required-c"), default="selected16")
    parser.add_argument("--jobs", type=int, choices=range(1, 5), default=1)
    parser.add_argument("--resume", action="store_true")
    parser.add_argument("--validate-only", action="store_true", help="Validate the declared input/profile subset without emission, builds or execution")
    parser.add_argument("--aot-debug-symbols", action="store_true", help="Retain optional debug symbols in future test-only AOT builds (larger artifacts)")
    parser.add_argument("--refresh-provider", action="store_true",
                        help="Copy retained linked source projects, refresh authored provider inputs and build every application anew without C emission")
    parser.add_argument("--provider-freeze", type=Path, help="Exact reviewed source-freeze receipt required by --refresh-provider")
    return Expansion(parser.parse_args()).run()


if __name__ == "__main__":
    raise SystemExit(main())
