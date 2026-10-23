#!/usr/bin/env python3
"""Execute a bounded selection of original C controls; leave the full gate open.

The API-owned tests/Upstream/build.py creates the unchanged C harnesses. This
bounded executor deliberately does not start the separate Python integrations.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tests/Upstream"))
from feature_adaptations import apply as apply_features
from native_profile import validate_profile, c_flags
sys.path.insert(0, str(ROOT / "scripts"))
from translation_evidence import read_closed

parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument("--executables", type=Path, required=True)
parser.add_argument("--native-report", type=Path, required=True)
parser.add_argument("--native-equivalence", type=Path)
parser.add_argument("--output", type=Path, required=True)
parser.add_argument("--form", choices=("raw", "processed", "all"), default="raw")
parser.add_argument("--mode", choices=("jit", "aot", "all"), default="jit")
parser.add_argument("--case-set", choices=("selected16", "required-c"), default="selected16")
parser.add_argument("--case", action="append", help="Optional exact original native-control source path for a focused probe")
args = parser.parse_args()
config = json.loads((ROOT / "config/upstream-tests.json").read_text())
selected = ["tests/unit/" + name + ".c" for name in config["native_control_tests"]]
if len(selected) != 16 or len(set(selected)) != 16:
    raise RuntimeError("Expected the 16 unique frozen native controls")
all_controls=selected[:]
required_c = [case["path"] for case in config["cases"]
              if case["status"].startswith("required") and case["path"].endswith(".c")]
if args.case_set == "required-c":
    selected = required_c[:]
scope_controls = selected[:]
if args.case:
    if set(args.case)-set(selected):raise RuntimeError("Focused case is outside the declared original C case set")
    selected=[case for case in selected if case in args.case]
manifest_bytes = args.executables.read_bytes()
native_bytes = args.native_report.read_bytes()
manifest = json.loads(manifest_bytes)
native = json.loads(native_bytes)
native_rows = [r for r in native.get("upstream", []) if r["status"] == "passed"]
native_cases = {r["case"]: r for r in native_rows}
if len(native_cases) != len(native_rows):
    raise RuntimeError("Duplicate native controls in reference receipt")
campaign = Path(manifest["campaign_run"]).resolve()
campaign_receipt_path, campaign_receipt = read_closed(ROOT, campaign)
first = next(row["command"] for row in campaign_receipt["commands"] if "--emit=obj" in row["command"])
flags = c_flags(first)
native_match = validate_profile(ROOT, campaign, args.native_report, args.native_equivalence)
forms = ("raw", "processed") if args.form == "all" else (args.form,)
modes = ("jit", "aot") if args.mode == "all" else (args.mode,)
args.output = args.output.resolve()
args.output.parent.mkdir(parents=True, exist_ok=True)
native_snapshot = args.output.with_name(args.output.stem + "-native-reference.json")
native_snapshot.write_bytes(native_bytes)

def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()

required_count=sum(case["status"].startswith("required") for case in config["cases"])
report = {"scope": f"Bounded original native C controls; full {required_count}-case required upstream gate remains open",
          "status": "running", "whole_upstream_status": "open", "cases": [],
          "selected_case_count": len(selected), "case_set": args.case_set, "required_c_case_count": len(required_c),
          "unselected_native_controls": [case for case in all_controls if case not in selected],
          "unselected_scope_cases": [case for case in scope_controls if case not in selected],
          "remaining_required_cases": [x["path"] for x in config["cases"] if x["status"].startswith("required") and x["path"] not in selected],
          "native_report": str(args.native_report.resolve()),
          "native_report_sha256": hashlib.sha256(native_bytes).hexdigest(),
          "native_report_snapshot": str(native_snapshot), "native_report_status": native["status"],
          "native_scope": native["scope"], "campaign_run": str(campaign),
          "native_profile_match": native_match,
          "campaign_receipt": str(campaign_receipt_path),
          "campaign_receipt_sha256": sha(campaign_receipt_path), "declaration_flags": flags,
          "accepted_test_adaptations_sha256": sha(ROOT / "tests/Upstream/test-adaptations.json"),
          "executable_manifest": str(args.executables.resolve()),
          "executable_manifest_sha256": hashlib.sha256(manifest_bytes).hexdigest()}

def save():
    args.output.write_text(json.dumps(report, indent=2) + "\n")

save()
for case in selected:
    for form in forms:
        for mode in modes:
            row = {"case": case, "form": form, "mode": mode, "status": "failed"}
            report["cases"].append(row)
            try:
                entries = [e for e in manifest["executables"] if e["case"] == case and e["form"] == form and e["mode"] == mode]
                if len(entries) != 1 or entries[0]["status"] != "built":
                    detail=entries[0].get("error", "unrun") if len(entries)==1 else "missing or duplicate manifest entry"
                    raise RuntimeError("Exact translated harness is not built: " + detail)
                entry = entries[0]
                original, staged = ROOT / "ref/s2n-tls" / case, Path(entry["compiled_source"])
                if sha(original) != entry["original_sha256"] or sha(staged) != entry["compiled_sha256"]:
                    raise RuntimeError("Original/compiled source identity changed")
                adapted, adaptations = apply_features(case, original.read_bytes())
                if entry.get("test_adaptations", []) != adaptations:
                    raise RuntimeError("Translated test adaptation receipt differs from the accepted exact guards")
                if staged.read_bytes() != b'#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n' + adapted:
                    raise RuntimeError("Upstream assertions changed beyond the exact prelude/accepted conditional guards")
                for path, expected in entry["execution_sha256"].items():
                    if sha(path) != expected:
                        raise RuntimeError("Executable closure changed: " + path)
                for path, expected in entry.get("dependency_inputs_sha256", {}).items():
                    if sha(path) != expected:
                        raise RuntimeError("Actual emitted C/header dependency content changed: " + path)
                snapshot = entry.get("provider_snapshot")
                if snapshot:
                    for path, expected in snapshot["snapshot_inputs_sha256"].items():
                        if sha(path) != expected:
                            raise RuntimeError("Frozen additional test provider input changed: " + path)
                reference = native_cases.get(case)
                if reference is None or reference.get("assertions", 0) <= 0:
                    raise RuntimeError("Matching successful native assertion count is missing")
                if reference.get("case") != case or reference.get("original_sha256") != entry["original_sha256"]:
                    raise RuntimeError("Native/translated original source hashes differ")
                if reference.get("test_adaptations", []) != adaptations:
                    raise RuntimeError("Native/translated accepted test adaptation receipts differ")
                native_source = Path(reference["compiled_source"])
                if sha(native_source) != reference["compiled_sha256"] or native_source.read_bytes() != adapted:
                    raise RuntimeError("Native control source differs from the exact accepted original assertions")
                row.update(test_adaptations=adaptations, native_original_sha256=reference["original_sha256"],
                           native_compiled_sha256=reference["compiled_sha256"], native_binary_sha256=reference["binary_sha256"])
                if "application_execution_receipt" in entry:
                    identity_path = Path(entry["application_execution_receipt"])
                    if sha(identity_path) != entry["application_execution_receipt_sha256"]:
                        raise RuntimeError("Actual application execution identity receipt changed")
                    row.update(application_execution_receipt=str(identity_path),
                               application_execution_receipt_sha256=entry["application_execution_receipt_sha256"])
                log = args.output.parent / (Path(case).stem + "-" + form + "-" + mode + ".log")
                with log.open("w") as stream:
                    process = subprocess.run(entry["command"], cwd=ROOT / "ref/s2n-tls/tests/unit", stdout=stream, stderr=subprocess.STDOUT, timeout=180)
                text = log.read_text()
                matches = re.findall(r"PASSED\s+(\d+) tests", text)
                row.update(log=str(log), log_sha256=sha(log), exit_code=process.returncode, command=entry["command"], original_sha256=entry["original_sha256"], native_assertions=reference["assertions"])
                if process.returncode or len(matches) != 1 or int(matches[0]) <= 0:
                    raise RuntimeError("Preserved upstream assertions did not complete successfully")
                if re.search(r"\bSKIP(?:PED)?\b", text, re.I):
                    raise RuntimeError("Required original test reported a skip")
                row["assertions"] = int(matches[0])
                if row["assertions"] != row["native_assertions"]:
                    raise RuntimeError("Native/translated runtime assertion counts differ")
                row["status"] = "passed"
            except Exception as error:
                row["error"] = str(error)
            print(row["status"].upper(), case, form, mode, row.get("assertions", ""), row.get("error", ""), flush=True)
            save()
report["counts"] = {status: sum(x["status"] == status for x in report["cases"]) for status in ("passed", "failed")}
report["status"] = "bounded-passed" if report["counts"]["failed"] == 0 else "failed"
save()
raise SystemExit(0 if report["status"] == "bounded-passed" else 1)
