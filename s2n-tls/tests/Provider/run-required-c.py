#!/usr/bin/env python3
"""Persistently qualify all required original C cases in bounded private batches.

Reuse only verified emissions from the exact new campaign. Each raw JIT harness
is linked/built once, then the owned expander qualifies the remaining modes.
"""
import argparse
import copy
import fcntl
import json
from pathlib import Path
import shutil
import subprocess
import sys
import time

from native_profile import validate_profile, sha

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "scripts"))
from translation_evidence import read_closed
from required_c_receipt import _verify_test_provider_snapshot, require_passed_native
from qualified_dependencies import storage_reserves, validate_work_root


def read(path):
    return json.loads(Path(path).read_text())


class RetainedRawRequired(RuntimeError):
    """A deliberate placement boundary; no new raw builder was launched."""


class Pipeline:
    def __init__(self, args):
        self.args = args
        self.output = args.output.resolve(); self.output.mkdir(parents=True, exist_ok=True)
        self.work_root = validate_work_root(args.work_root) if args.work_root else None
        self.upstream_builder = args.upstream_builder.resolve() if args.upstream_builder else None
        self.upstream_builder_inputs = self.validate_upstream_builder() if self.upstream_builder else None
        self.lease = (self.output / "controller.lock").open("a+")
        fcntl.flock(self.lease, fcntl.LOCK_EX | fcntl.LOCK_NB)
        self.progress_path = self.output / "progress.json"
        self.campaign = args.campaign_run.resolve()
        self.translation_path, self.translation_record = read_closed(ROOT, self.campaign)
        self.emission = read(args.emission_manifest)
        self.inventory = read(ROOT / "config/upstream-tests.json")
        self.required = [r["path"] for r in self.inventory["cases"] if r["status"].startswith("required") and r["path"].endswith(".c")]
        observed = self.emission["executables"]
        if (self.emission["status"] != "emitted" or Path(self.emission["campaign_run"]).resolve() != self.campaign
            or len(observed) != len(self.required) or {r["case"] for r in observed} != set(self.required)
            or any(r["status"] != "emitted" for r in observed)):
            raise RuntimeError("Pipeline needs the complete new required-C emission manifest, without missing/duplicate cases")
        require_passed_native(read(args.native_report))
        match = validate_profile(ROOT, self.campaign, args.native_report, args.native_equivalence)
        if self.progress_path.exists():
            if not args.resume:
                raise RuntimeError("Persistent progress exists; use --resume")
            self.progress = read(self.progress_path)
            if (self.progress["emission_manifest_sha256"] != sha(args.emission_manifest)
                or Path(self.progress["campaign_run"]) != self.campaign):
                raise RuntimeError("Resume cannot change the immutable campaign/emission inputs")
        else:
            batches = [self.required[i:i + args.batch_size] for i in range(0, len(self.required), args.batch_size)]
            self.progress = {"status": "running", "scope": "All required original C assertions; Python and other campaign gates remain separate",
                "whole_campaign_status": "open", "campaign_run": str(self.campaign), "required_c_cases": len(self.required),
                "emission_manifest": str(args.emission_manifest.resolve()), "emission_manifest_sha256": sha(args.emission_manifest),
                "native_profile_match": match, "commands": [], "cases": {case: {"status": "unrun"} for case in self.required},
                "batches": [{"index": index, "cases": cases, "status": "unrun"} for index, cases in enumerate(batches)]}
        self.support = self.output / "support-reuse.json"
        self.provider_freeze = None
        if args.provider_freeze:
            path = args.provider_freeze.resolve()
            frozen = read(path)
            self.provider_freeze = {"receipt": str(path), "sha256": sha(path), "after_sha256": frozen["after_sha256"]}
        if args.refresh_provider and not self.provider_freeze:
            raise RuntimeError("Source-aware controller refresh needs an explicit --provider-freeze")
        self.linked_reuse = {}
        for path in args.reuse_linked_manifest:
            manifest = read(path)
            self.validate_raw_inputs(manifest)
            for entry in manifest["executables"]:
                if entry["form"] == "raw" and entry["mode"] == "jit" and entry["status"] == "built":
                    if entry["case"] in self.linked_reuse:
                        raise RuntimeError("Ambiguous retained linked raw harness selection: " + entry["case"])
                    self.linked_reuse[entry["case"]] = (path.resolve(), manifest, entry)
        if self.progress.get("campaign_receipt_sha256", sha(self.translation_path)) != sha(self.translation_path):
            raise RuntimeError("Resume cannot change the closed translation receipt")
        self.progress.setdefault("campaign_receipt", str(self.translation_path))
        self.progress.setdefault("campaign_receipt_sha256", sha(self.translation_path))
        self.verify_emissions()
        self.save()

    def verify_emissions(self):
        if sha(self.translation_path) != self.progress["campaign_receipt_sha256"]:
            raise RuntimeError("Closed translation evidence changed")
        if self.provider_freeze:
            if sha(self.provider_freeze["receipt"]) != self.provider_freeze["sha256"]:
                raise RuntimeError("Reviewed provider freeze receipt changed")
            for path, expected in self.provider_freeze["after_sha256"].items():
                if sha(path) != expected:
                    raise RuntimeError("Reviewed provider source freeze changed: " + path)
        for key in ("tool_sha256", "product_objects_sha256"):
            for path, expected in self.emission[key].items():
                if sha(path) != expected:
                    raise RuntimeError("Immutable new emission input changed: " + path)
        for row in self.emission["executables"]:
            for path, expected in ((row["object"], row["object_sha256"]), (row["compiled_source"], row["compiled_sha256"]),
                                   (row["dependency_file"], row["dependency_sha256"])):
                if sha(path) != expected:
                    raise RuntimeError("Frozen emitted harness input changed: " + path)
            for path, expected in row["dependency_inputs_sha256"].items():
                if sha(path) != expected:
                    raise RuntimeError("Frozen actual emitted header/source dependency changed: " + path)

    def save(self):
        self.progress["counts"] = {status: sum(r["status"] == status for r in self.progress["cases"].values())
                                   for status in ("passed", "failed", "unrun")}
        self.progress["updated_unix"] = time.time()
        temporary = self.progress_path.with_suffix(".json.tmp")
        temporary.write_text(json.dumps(self.progress, indent=2) + "\n"); temporary.replace(self.progress_path)

    def validate_upstream_builder(self):
        """Accept only an explicitly selected, preserved placement derivation."""
        path = self.upstream_builder
        if self.work_root is None or path.name != "build.py" or path == ROOT / "tests/Upstream/build.py" or path.is_symlink():
            raise RuntimeError("A private derived builder needs its explicit external work root")
        receipt = path.with_name("derivation.json")
        data = read(receipt)
        if data.get("schema") != "dotcc-private-upstream-placement-v1" or data["canonical_builder"] != str(ROOT / "tests/Upstream/build.py"):
            raise RuntimeError("Unrecognized private builder derivation")
        expected = dict(data["inputs_sha256"])
        expected[str(receipt)] = sha(receipt)
        if expected.get(str(path)) != sha(path) or sha(data["canonical_builder"]) != data["canonical_builder_sha256"]:
            raise RuntimeError("Private builder differs from its reviewed source derivation")
        for source, value in expected.items():
            if sha(source) != value:
                raise RuntimeError("Private builder dependency changed: " + source)
        return expected

    def command(self, command, directory, label):
        log = directory / (label + ".log")
        if log.exists():
            log = directory / (label + "-" + str(time.time_ns()) + ".log")
        command = list(map(str, command))
        with log.open("w") as stream:
            result = subprocess.run(command, cwd=ROOT.parent, stdout=stream, stderr=subprocess.STDOUT)
        self.progress["commands"].append({"command": command, "exit_code": result.returncode, "log": str(log)})
        self.save()
        return result.returncode

    def add_support(self, manifest_path):
        manifest = read(manifest_path)
        data = read(self.support) if self.support.exists() else {
            "scope": "Exact previously emitted whole original support TUs only; no execution claim",
            "campaign_run": str(self.campaign), "tool_sha256": self.emission["tool_sha256"],
            "product_objects_sha256": self.emission["product_objects_sha256"], "support_emissions": [], "provenance": []}
        keyed = {}
        for row in [*data["support_emissions"], *manifest.get("support_emissions", [])]:
            if row.get("status") != "emitted":
                continue
            source = Path(row["source"])
            stage = next(p for p in source.parents if p.name == "source")
            relative = source.relative_to(stage).as_posix()
            if relative in keyed and keyed[relative]["original_sha256"] != row["original_sha256"]:
                raise RuntimeError("Same-campaign support TU source identity changed")
            keyed.setdefault(relative, row)
        data["support_emissions"] = list(keyed.values())
        provenance = {"path": str(manifest_path), "sha256": sha(manifest_path)}
        if provenance not in data["provenance"]:
            data["provenance"].append(provenance)
        self.support.write_text(json.dumps(data, indent=2) + "\n")

    def validate_raw_inputs(self, manifest):
        if (Path(manifest["campaign_run"]).resolve() != self.campaign
            or manifest["tool_sha256"] != self.emission["tool_sha256"]
            or manifest["product_objects_sha256"] != self.emission["product_objects_sha256"]):
            raise RuntimeError("Retained linked harness must use the exact campaign/tool/175-object closure")

    def prepare_provider_refresh(self):
        if not self.provider_freeze:
            return
        previous = self.progress.get("provider_source_freeze")
        if previous == self.provider_freeze:
            return
        has_old_results = any(r["status"] != "unrun" for r in self.progress["cases"].values())
        if has_old_results and not self.args.refresh_provider:
            raise RuntimeError("Existing results have a different provider source; use --refresh-provider to retain and requalify them")
        if has_old_results:
            archive = self.output / ("historical-provider-progress-" + str(time.time_ns()) + ".json")
            shutil.copyfile(self.progress_path, archive)
            self.progress.setdefault("historical_provider_progress", []).append({"path": str(archive), "sha256": sha(archive),
                "counts": dict(self.progress["counts"]), "provider_source_freeze": previous})
            for case, record in list(self.progress["cases"].items()):
                if record["status"] == "unrun":
                    continue
                self.progress["cases"][case] = {"status": "unrun", "stage": "awaiting-final-provider-refresh",
                                                "retained_prior_attempts": [copy.deepcopy(record)]}
            for batch in self.progress["batches"]:
                if batch.get("raw_manifest"):
                    batch["provider_refresh_from_active_cases"] = batch.get("active_cases", batch["cases"])
                    batch["active_cases"] = list(batch["cases"])
                    batch["provider_refresh_from_raw_manifest"] = batch["raw_manifest"]
                    batch["provider_refresh_attempt"] = batch.get("provider_refresh_attempt", 0) + 1
                    batch["provider_refresh_from_batch_status"] = batch["status"]
                    batch["status"] = "refresh-retained"
        self.progress["provider_source_freeze"] = self.provider_freeze
        self.save()

    def retained_raw_manifest(self, batch, requested, destination):
        original_path = Path(batch["provider_refresh_from_raw_manifest"])
        original = read(original_path); self.validate_raw_inputs(original)
        combined = dict(original, executables=[], commands=[], evidence_kind="retained-linked-harnesses-for-provider-refresh",
                        retained_input_manifests=[], retained_case_selections=[], provider_source_freeze=self.provider_freeze)
        seen = set()
        for case in requested:
            if case in self.linked_reuse:
                path, manifest, entry = self.linked_reuse[case]
                selection = "explicit retained linked manifest"
            else:
                # An adopted repair can live outside the last batch's raw
                # manifest, or replace its failed entry. Retain the latest
                # case's own provenance across every source-freeze refresh.
                prior = self.progress["cases"][case].get("retained_prior_attempts", [])
                paths = {Path(record["raw_manifest"]).resolve() for record in prior if record.get("raw_manifest")}
                if len(paths) > 1:
                    raise RuntimeError("Ambiguous prior raw provenance for source refresh: " + case)
                path = next(iter(paths)) if paths else original_path
                manifest = read(path) if path != original_path else original
                self.validate_raw_inputs(manifest)
                selection = "latest retained case provenance" if paths else "retained batch manifest"
                found = [r for r in manifest["executables"] if (r["case"], r["form"], r["mode"]) == (case, "raw", "jit")]
                if len(found) != 1:
                    raise RuntimeError("Missing exact retained raw harness for source refresh: " + case)
                entry = found[0]
            entry = dict(entry)
            if entry["status"] == "built" and not entry.get("project"):
                executable = Path(entry["command"][1] if entry["command"][0] == "dotnet" else entry["command"][0]).resolve()
                projects = set()
                for command in manifest["commands"]:
                    argv = command["command"]
                    if "--artifacts-path" not in argv or not executable.is_relative_to(Path(argv[argv.index("--artifacts-path") + 1]).resolve()):
                        continue
                    projects.update(str(Path(p).resolve()) for p in argv if p.endswith(".csproj") and not p.startswith("-"))
                if len(projects) != 1:
                    raise RuntimeError("Retained raw source project is missing or ambiguous: " + case)
                entry["project"] = next(iter(projects))
            combined["executables"].append(entry)
            combined["retained_case_selections"].append({"case": case, "manifest": str(path),
                "manifest_sha256": sha(path), "selection": selection})
            if str(path) not in seen:
                seen.add(str(path)); combined["commands"].extend(manifest["commands"])
                combined["retained_input_manifests"].append({"path": str(path), "sha256": sha(path)})
        combined["status"] = "built" if all(r["status"] == "built" for r in combined["executables"]) else "failed"
        destination.write_text(json.dumps(combined, indent=2) + "\n")

    def batch(self, row):
        requested = [case for case in row.get("active_cases", row["cases"]) if self.progress["cases"][case]["status"] != "passed"]
        if row["status"] in ("failed", "controller-error") and self.args.retry_failed:
            requested = [case for case in row["cases"] if self.progress["cases"][case]["status"] != "passed"]
            row["retry_attempt"] = row.get("retry_attempt", 0) + 1
        row["active_cases"] = requested
        suffix = "-retry-" + str(row["retry_attempt"]) if row.get("retry_attempt") else ""
        if row.get("provider_refresh_attempt"):
            suffix += "-provider-refresh-" + str(row["provider_refresh_attempt"])
        directory = self.output / ("batch-" + str(row["index"]).zfill(3) + suffix); directory.mkdir(exist_ok=True)
        raw = directory / "raw-executables.json"
        if not requested:
            row["status"] = "passed"; self.save(); return
        if not raw.exists() and row.get("provider_refresh_from_raw_manifest"):
            self.retained_raw_manifest(row, requested, raw)
            row["raw_manifest"] = str(raw); self.save()
        if not raw.exists() and self.args.retained_raw_only:
            raise RetainedRawRequired("Retained raw provenance exhausted; review external raw-builder allocation before continuing")
        if not raw.exists():
            if self.work_root is not None and self.upstream_builder is None:
                raise RetainedRawRequired("External newraw allocation needs the explicit reviewed private builder")
            builder = self.upstream_builder or ROOT / "tests/Upstream/build.py"
            argv = [sys.executable, "-B", builder, "--campaign-run", self.campaign,
                    "--reuse-emission", self.args.emission_manifest.resolve(), "--form", "raw", "--mode", "jit", "--output", raw]
            if self.upstream_builder is not None:
                if self.validate_upstream_builder() != self.upstream_builder_inputs:
                    raise RuntimeError("Private builder identity changed since controller admission")
                argv += ["--work-root", self.work_root, "--min-free-gib", self.args.min_free_gib]
                if self.provider_freeze:
                    argv += ["--provider-freeze", self.provider_freeze["receipt"]]
                row["private_build_driver_inputs_sha256"] = self.upstream_builder_inputs
            if self.support.exists():
                argv += ["--reuse-support-manifest", self.support]
            if self.args.provider_policy:
                source_policy = self.args.provider_policy.resolve()
                frozen_policy = directory / "provider-policy.json"
                before = sha(source_policy)
                if frozen_policy.exists():
                    policy = row.get("provider_policy")
                    if not policy or policy["snapshot"] != str(frozen_policy) or sha(frozen_policy) != policy["snapshot_sha256"]:
                        raise RuntimeError("Interrupted batch policy snapshot is missing its exact retained identity")
                else:
                    shutil.copyfile(source_policy, frozen_policy)
                    if sha(frozen_policy) != before or sha(source_policy) != before:
                        raise RuntimeError("Authored provider policy changed during batch snapshot")
                    row["provider_policy"] = {"original": str(source_policy), "original_before_sha256": before,
                                              "snapshot": str(frozen_policy), "snapshot_sha256": before}
                policy_observation = {"original": str(source_policy), "before_sha256": before}
                row["provider_policy"].setdefault("build_observations", []).append(policy_observation)
                argv += ["--provider-policy", frozen_policy]
            for case in requested:
                argv += ["--case", case]
            row.update(status="building-raw", raw_manifest=str(raw)); self.save()
            self.command(argv, directory, "raw-build")
            if self.upstream_builder is not None and self.validate_upstream_builder() != self.upstream_builder_inputs:
                raise RuntimeError("Private builder closure changed during raw build")
            if self.args.provider_policy:
                policy = row["provider_policy"]
                policy_observation["after_sha256"] = sha(source_policy)
                if len(policy["build_observations"]) == 1:
                    policy["original_after_sha256"] = policy_observation["after_sha256"]
                self.save()
                if policy_observation["after_sha256"] != before or sha(policy["snapshot"]) != policy["snapshot_sha256"]:
                    raise RuntimeError("Original/frozen provider policy changed during raw batch build")
        if row.get("provider_policy"):
            policy = row["provider_policy"]
            if sha(policy["snapshot"]) != policy["snapshot_sha256"]:
                raise RuntimeError("Retained batch provider policy changed")
        if not raw.exists():
            row.update(status="failed", error="Raw builder produced no reviewable manifest")
            for case in requested:
                self.progress["cases"][case].update(status="failed", stage="raw-build", error=row["error"])
            self.save(); return
        self.add_support(raw)
        manifest = read(raw)
        if Path(manifest["campaign_run"]).resolve() != self.campaign:
            raise RuntimeError("Raw batch used a different complete campaign")
        built = []
        for case in requested:
            found = [r for r in manifest["executables"] if r["case"] == case and r["form"] == "raw" and r["mode"] == "jit"]
            if len(found) == 1 and found[0]["status"] == "built":
                built.append(case)
            else:
                self.progress["cases"][case].update(status="failed", stage="raw-build", raw_manifest=str(raw),
                    error=found[0].get("error", "Missing exact built harness") if len(found) == 1 else "Missing/duplicate raw harness")
        if built:
            expanded = directory / "expanded-executables.json"
            argv = [sys.executable, "-B", ROOT / "tests/Provider/expand-original-controls.py", "--executables", raw,
                    "--output", expanded, "--native-report", self.args.native_report.resolve(),
                    "--case-set", "required-c", "--jobs", self.args.jobs]
            if self.work_root is not None:
                argv += ["--work-root", self.work_root, "--min-free-gib", self.args.min_free_gib]
            if self.args.native_equivalence:
                argv += ["--native-equivalence", self.args.native_equivalence.resolve()]
            if self.provider_freeze:
                argv += ["--provider-freeze", self.provider_freeze["receipt"]]
            if row.get("provider_refresh_from_raw_manifest"):
                argv += ["--refresh-provider"]
            if expanded.exists():
                argv.append("--resume")
            for case in built:
                argv += ["--case", case]
            row.update(status="expanding-and-executing", expanded_manifest=str(expanded)); self.save()
            self.command(argv, directory, "all4")
            evidence = read(expanded) if expanded.exists() else {"qualifications": {}, "executables": []}
            if evidence.get("final_audit_error"):
                raise RuntimeError("Expansion final closure audit failed: " + evidence["final_audit_error"])
            for case in built:
                record = evidence.get("qualifications", {}).get(case)
                if record:
                    checks = read(record["receipt"])
                    if sha(record["receipt"]) != record["sha256"] or checks["counts"] != {"passed": 4, "failed": 0}:
                        raise RuntimeError("Qualified case receipt identity/count changed")
                    variants = [r for r in evidence["executables"] if r["case"] == case]
                    if len(variants) != 4 or any(r["status"] != "built" for r in variants):
                        raise RuntimeError("Qualified case lacks four actual application variants")
                    for variant in variants:
                        files = {**variant["execution_sha256"], variant["application_execution_receipt"]: variant["application_execution_receipt_sha256"]}
                        files.update({str(Path(variant["project"]).parent / name): value for name, value in variant["source_sha256"].items()})
                        for path, expected in files.items():
                            if sha(path) != expected:
                                raise RuntimeError("Qualified actual source/execution/identity closure changed: " + path)
                    self.progress["cases"][case].update(status="passed", original_assertions_all4=checks["cases"],
                        qualification=record, raw_manifest=str(raw), expanded_manifest=str(expanded),
                        provider_source_freeze_sha256=self.provider_freeze["sha256"] if self.provider_freeze else None)
                else:
                    errors = sorted({r.get("error", "Missing all-four-mode qualification") for r in evidence["executables"] if r["case"] == case})
                    if case in evidence.get("case_failures", {}):
                        errors.append(evidence["case_failures"][case])
                    self.progress["cases"][case].update(status="failed", stage="expansion-or-execution", errors=errors,
                        raw_manifest=str(raw), expanded_manifest=str(expanded))
        row["status"] = "passed" if all(self.progress["cases"][case]["status"] == "passed" for case in row["cases"]) else "failed"
        self.save(); print("BATCH", row["index"], row["status"], self.progress["counts"], flush=True)

    def final_audit(self):
        # Recheck retained results from earlier batches and resumed controllers.
        # Shared runtime files are hashed once for this audit, while each
        # application's source/output/identity receipt remains independently
        # associated with its four exact original assertion executions.
        checked = {}

        def verify(path, expected):
            path = str(path)
            if path not in checked:
                checked[path] = sha(path)
            if checked[path] != expected:
                raise RuntimeError("Retained source/execution evidence changed: " + path)

        manifests = {}
        for case, record in self.progress["cases"].items():
            if record["status"] != "passed":
                continue
            try:
                qualified = record["qualification"]
                verify(qualified["receipt"], qualified["sha256"])
                assertions = read(qualified["receipt"])
                expected_variants = {(form, mode) for form in ("raw", "processed") for mode in ("jit", "aot")}
                if (assertions["counts"] != {"passed": 4, "failed": 0}
                    or len(assertions["cases"]) != 4
                    or {(r["form"], r["mode"]) for r in assertions["cases"]} != expected_variants
                    or any(r["case"] != case or r["status"] != "passed" or r["assertions"] != r["native_assertions"] for r in assertions["cases"])):
                    raise RuntimeError("Retained exact original/native assertion evidence is incomplete")
                path = record["expanded_manifest"]
                if path not in manifests:
                    manifests[path] = read(path)
                variants = [r for r in manifests[path]["executables"] if r["case"] == case]
                if len(variants) != 4 or {(r["form"], r["mode"]) for r in variants} != expected_variants:
                    raise RuntimeError("Retained four-mode execution manifest is incomplete")
                for variant in variants:
                    verify(variant["application_execution_receipt"], variant["application_execution_receipt_sha256"])
                    identity = read(variant["application_execution_receipt"])
                    _verify_test_provider_snapshot(variant, identity, verify)
                    if self.provider_freeze:
                        captured = identity["authored_source_sha256"]
                        for source, expected in self.provider_freeze["after_sha256"].items():
                            path = Path(source)
                            if path.is_relative_to(ROOT / "src/CryptoProvider") and path.suffix in (".cs", ".csproj", ".json"):
                                if captured.get(source) != expected:
                                    raise RuntimeError("Old-provider actual execution identity cannot qualify final sources: " + source)
                    files = {**variant["execution_sha256"], **identity["output_sha256"], **identity["runtime_closure_sha256"]}
                    files.update({str(Path(variant["project"]).parent / name): value for name, value in variant["source_sha256"].items()})
                    for source, expected in files.items():
                        verify(source, expected)
            except Exception as error:
                record.update(status="failed", stage="final-retained-evidence-audit", error=str(error))
        self.progress["final_audit"] = {"files_checked": len(checked), "expanded_manifests_sha256": {p: sha(p) for p in manifests}}
        self.save()

    def adopt_expanded(self, path):
        """Adopt a qualified private repair only while holding the controller lease."""
        path = path.resolve()
        expanded = read(path)
        if (Path(expanded["campaign_run"]).resolve() != self.campaign
            or expanded.get("case_set") != "required-c"
            or expanded["native_report_sha256"] != sha(self.args.native_report)
            or expanded["native_profile_match"] != self.progress["native_profile_match"]
            or expanded["campaign_receipt_sha256"] != sha(self.translation_path)):
            raise RuntimeError("Repair adoption needs the exact campaign/native/closed-translation identities")
        if self.provider_freeze and expanded.get("provider_source_freeze") != self.provider_freeze:
            raise RuntimeError("Repair adoption cannot reuse an old-provider qualification")
        controls = {r["case"]: r for r in read(self.args.native_report)["upstream"] if r["status"] == "passed"}
        expected = {(form, mode) for form in ("raw", "processed") for mode in ("jit", "aot")}
        pending = {}
        for case, qualification in expanded.get("qualifications", {}).items():
            if case not in self.progress["cases"] or self.progress["cases"][case]["status"] == "passed":
                continue
            if sha(qualification["receipt"]) != qualification["sha256"]:
                raise RuntimeError("Repair assertion receipt changed")
            assertions = read(qualification["receipt"])
            variants = [r for r in expanded["executables"] if r["case"] == case]
            if (assertions["counts"] != {"passed": 4, "failed": 0} or len(assertions["cases"]) != 4
                or len(variants) != 4 or {(r["form"], r["mode"]) for r in variants} != expected
                or {(r["form"], r["mode"]) for r in assertions["cases"]} != expected
                or any(r["status"] != "built" for r in variants)
                or any(r["case"] != case or r["status"] != "passed" or r["assertions"] != controls[case]["assertions"]
                       or r["native_assertions"] != controls[case]["assertions"] for r in assertions["cases"])):
                raise RuntimeError("Repair lacks four exact original/native assertion executions: " + case)
            for result in assertions["cases"]:
                if sha(result["log"]) != result["log_sha256"]:
                    raise RuntimeError("Repair assertion transcript changed")
            prior = copy.deepcopy(self.progress["cases"][case])
            pending[case] = {"status": "passed", "original_assertions_all4": assertions["cases"],
                             "qualification": qualification, "raw_manifest": expanded["input_manifest"],
                             "expanded_manifest": str(path), "retained_prior_attempts": [prior],
                             "provider_source_freeze_sha256": self.provider_freeze["sha256"] if self.provider_freeze else None}
        self.progress["cases"].update(pending)
        self.final_audit()
        rejected = [case for case in pending if self.progress["cases"][case]["status"] != "passed"]
        self.progress.setdefault("adopted_expansions", []).append({"path": str(path), "sha256": sha(path),
            "cases": list(pending), "rejected_after_actual_closure_audit": rejected})
        for batch in self.progress["batches"]:
            if all(self.progress["cases"][case]["status"] == "passed" for case in batch["cases"]):
                batch["status"] = "passed"
        self.save()
        if rejected:
            raise RuntimeError("Repair failed actual retained closure audit: " + ", ".join(rejected))
        print("ADOPTED", len(pending), "qualified original C repairs from", path, flush=True)

    def run(self):
        if self.args.validate_only:
            self.progress["status"] = "inputs-validated"
            self.save()
            print("PIPELINE INPUTS VALIDATED;", len(self.required), "required C cases remain unrun", flush=True)
            return 0
        self.prepare_provider_refresh()
        for path in self.args.adopt_expanded:
            self.adopt_expanded(path)
        self.progress["status"] = "running"; self.save()
        for row in self.progress["batches"]:
            if row["status"] == "passed" or (row["status"] == "failed" and not self.args.retry_failed):
                continue
            locations = [self.output]
            if self.work_root is not None:
                locations.append(self.work_root)
            if row.get("expanded_manifest") and Path(row["expanded_manifest"]).is_file():
                locations.append(Path(read(row["expanded_manifest"])["work"]))
            reserves = storage_reserves(locations, self.args.min_free_gib * 2**30)
            self.progress.setdefault("storage_reserve_observations", []).append({"batch": row["index"], "checks": reserves,
                "future_work_root": str(self.work_root) if self.work_root is not None else None})
            if not all(check["passed"] for check in reserves):
                self.progress.update(status="resource-limit", resource_error="Free storage is below the recorded batch reserve; actual execution closures retained")
                self.save(); return 1
            try:
                self.batch(row)
            except RetainedRawRequired as error:
                self.progress.update(status="awaiting-retained-raw", placement_error=str(error))
                row.update(status="awaiting-retained-raw", placement_error=str(error)); self.save()
                print("STOPPED before new raw builder; retained qualification/progress preserved", flush=True)
                return 1
            except Exception as error:
                self.progress.update(status="controller-error", controller_error=str(error))
                row.update(status="controller-error", error=str(error)); self.save()
                raise
            if (self.output / "pause-after-batch").exists():
                self.progress["status"] = "paused-between-batches"; self.save()
                print("PAUSED between inactive batches", flush=True); return 1
        self.final_audit()
        self.progress["status"] = "passed" if self.progress["counts"]["passed"] == len(self.required) else "failed"
        self.verify_emissions(); self.save()
        print("PROGRESS", self.progress_path, self.progress["counts"], flush=True)
        return 0 if self.progress["status"] == "passed" else 1


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--campaign-run", type=Path, required=True)
    parser.add_argument("--upstream-builder", type=Path, help="Explicit immutable reviewed private placement derivation; default remains canonical")
    parser.add_argument("--work-root", type=Path, help="Private future expansion root replacing ROOT/build; canonical builder and old results stay unchanged")
    parser.add_argument("--retained-raw-only", action="store_true", help="Stop before launching any new raw builder once retained linked provenance is exhausted")
    parser.add_argument("--emission-manifest", type=Path, required=True)
    parser.add_argument("--native-report", type=Path, required=True)
    parser.add_argument("--native-equivalence", type=Path,
                        help="Required only for an independently checked equivalent native report from a different campaign")
    parser.add_argument("--provider-policy", type=Path,
                        help="Freeze an additional original-test provider selection catalogue separately for each raw batch")
    parser.add_argument("--adopt-expanded", type=Path, action="append", default=[],
                        help="Adopt retained all-four-mode private repairs under the exclusive controller lease, preserving failed attempts")
    parser.add_argument("--refresh-provider", action="store_true", help="Retain historical old-provider progress and requalify linked projects against the final provider")
    parser.add_argument("--provider-freeze", type=Path, help="Immutable reviewed final provider source-freeze receipt")
    parser.add_argument("--reuse-linked-manifest", type=Path, action="append", default=[],
                        help="Exact retained raw harness manifest for a previously repaired boundary; no old executable identity is reused")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--batch-size", type=int, choices=range(1, 9), default=4)
    parser.add_argument("--jobs", type=int, choices=range(1, 5), default=2)
    parser.add_argument("--resume", action="store_true")
    parser.add_argument("--retry-failed", action="store_true", help="Create new private attempts for failed cases while preserving prior actual execution closures")
    parser.add_argument("--min-free-gib", type=float, default=2)
    parser.add_argument("--validate-only", action="store_true")
    return Pipeline(parser.parse_args()).run()


if __name__ == "__main__":
    raise SystemExit(main())
