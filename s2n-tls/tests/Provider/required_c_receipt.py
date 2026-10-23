"""Read-only validation of a completed required-C batch aggregate.

No controller, compiler, build, or original test is launched by this module.
"""
import json
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET

from native_profile import sha, validate_profile

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "tests/Upstream"))
from feature_adaptations import apply as apply_features
sys.path.insert(0, str(ROOT / "scripts"))
from translation_evidence import read_closed


def _verify_test_provider_snapshot(entry, identity, verify):
    """Validate the actual frozen extra assembly; its authored source may evolve."""
    snapshot = entry.get("provider_snapshot")
    if not snapshot:
        return
    if identity["product_snapshot"].get("test_provider_snapshot") != snapshot:
        raise RuntimeError("Application identity differs from its additional test provider snapshot")
    project = Path(snapshot["project"]).resolve()
    if project.name != "CryptoProvider.Upstream.csproj":
        raise RuntimeError("Unexpected additional test provider project")
    repository = project.parents[4]
    if not snapshot["original_inputs_sha256"] or not snapshot["snapshot_inputs_sha256"]:
        raise RuntimeError("Additional test provider lacks its original/frozen source identities")
    expected = {}
    for original, value in snapshot["original_inputs_sha256"].items():
        target = repository / Path(original).relative_to(ROOT.parent)
        expected[str(target)] = value
        # The original production assembly and dependency pin remain frozen.
        # Test-only authored additions may change for later private snapshots.
        if Path(original).is_relative_to(ROOT / "src/CryptoProvider") or Path(original) == ROOT / "config/provider.json":
            verify(original, value)
    if expected != snapshot["snapshot_inputs_sha256"]:
        raise RuntimeError("Additional test provider snapshot is not a byte-identical input copy")
    for path, value in expected.items():
        verify(path, value)
        if identity["authored_source_sha256"].get(path) != value:
            raise RuntimeError("Actual application omits a frozen provider input")
    references = {(Path(entry["project"]).parent / node.get("Include")).resolve()
                  for node in ET.parse(entry["project"]).iter("ProjectReference") if node.get("Include")}
    if references != {project}:
        raise RuntimeError("Original C harness did not select only its frozen additional provider project")
    compile_inputs = {(Path(entry["project"]).parent / node.get("Include")).resolve()
                      for node in ET.parse(entry["project"]).iter("Compile") if node.get("Include")}
    if not set(map(Path, snapshot["bindings"])).issubset(compile_inputs):
        raise RuntimeError("Original C harness omitted its frozen typed extra binding")
    for name in ("packages.lock.json", "packages.linux-x64.lock.json", "packages.aot.lock.json", "packages.aot.linux-x64.lock.json"):
        production = repository / "s2n-tls/src/CryptoProvider" / name
        if expected.get(str(project.parent / name)) != expected.get(str(production)) or str(production) not in expected:
            raise RuntimeError("Additional test provider changed a frozen production package lock")


def _validate_completed_report(report_path, native_report, native_equivalence, *, campaign_run=None):
    """Return validated ``cases`` (257 x four variants), or raise RuntimeError.

    Arguments are paths. The returned dictionary contains status, campaign_run,
    cases, counts, c_report/c_report_sha256, native_profile_match, and
    files_checked. Consumers select rows by (case, form, mode), using exact
    source paths as the native lookup key. Missing/failed/unrun aggregates are
    rejected; callers must keep those qualification gates open.
    """
    report_path, native_report = Path(report_path).resolve(), Path(native_report).resolve()
    report = json.loads(report_path.read_text())
    inventory = json.loads((ROOT / "config/upstream-tests.json").read_text())
    required = {r["path"] for r in inventory["cases"] if r["status"].startswith("required") and r["path"].endswith(".c")}
    if (report["status"] != "passed" or len(required) != 257
        or report["counts"] != {"passed": len(required), "failed": 0, "unrun": 0}
        or set(report["cases"]) != required or any(r["status"] != "passed" for r in report["cases"].values())):
        raise RuntimeError("A complete passing 257-case C aggregate is required; partial evidence cannot close the C gate")
    campaign = Path(report["campaign_run"]).resolve()
    translation_path, translation = read_closed(ROOT, campaign)
    translation_sha256 = sha(translation_path)
    if report.get("campaign_receipt_sha256", translation_sha256) != translation_sha256:
        raise RuntimeError("Completed aggregate closed translation identity changed")
    if campaign_run is not None and Path(campaign_run).resolve() != campaign:
        raise RuntimeError("Completed C aggregate belongs to a different product campaign")
    match = validate_profile(ROOT, campaign, native_report, native_equivalence)
    if report["native_profile_match"] != match:
        raise RuntimeError("Completed aggregate native equivalence identity changed")
    native = json.loads(native_report.read_text())
    controls = {r["case"]: r for r in native["upstream"] if r["status"] == "passed"}
    if set(controls) != required or len(controls) != len(native["upstream"]):
        raise RuntimeError("Native case-path inventory is incomplete or ambiguous")
    checked = {}

    def verify(path, expected):
        path = str(path)
        if path not in checked:
            checked[path] = sha(path)
        if checked[path] != expected:
            raise RuntimeError("Completed C evidence content changed: " + path)

    verify(translation_path, translation_sha256)
    verify(report["emission_manifest"], report["emission_manifest_sha256"])
    emission = json.loads(Path(report["emission_manifest"]).read_text())
    if emission["status"] != "emitted" or Path(emission["campaign_run"]).resolve() != campaign:
        raise RuntimeError("Completed aggregate emission snapshot differs")
    for key in ("tool_sha256", "product_objects_sha256"):
        for path, expected in emission[key].items():
            verify(path, expected)
    if len(emission["product_objects_sha256"]) != 175:
        raise RuntimeError("Completed C gate lacks the complete 175-object product")
    for row in emission["executables"]:
        for path, expected in ((row["object"], row["object_sha256"]), (row["compiled_source"], row["compiled_sha256"]),
                               (row["dependency_file"], row["dependency_sha256"])):
            verify(path, expected)
        for path, expected in row["dependency_inputs_sha256"].items():
            verify(path, expected)
    manifest_cache = {}
    rows = []
    variants = {(form, mode) for form in ("raw", "processed") for mode in ("jit", "aot")}
    for case in sorted(required):
        record, control = report["cases"][case], controls[case]
        qualification = record["qualification"]
        verify(qualification["receipt"], qualification["sha256"])
        checks = json.loads(Path(qualification["receipt"]).read_text())
        if (checks["counts"] != {"passed": 4, "failed": 0} or len(checks["cases"]) != 4
            or {(r["form"], r["mode"]) for r in checks["cases"]} != variants
            or checks["native_report_sha256"] != sha(native_report)):
            raise RuntimeError("Completed original assertion evidence is incomplete: " + case)
        if checks["campaign_receipt_sha256"] != translation_sha256:
            raise RuntimeError("Original assertion execution used different closed translation evidence")
        original = ROOT / "ref/s2n-tls" / case
        verify(original, control["original_sha256"])
        adapted, adaptations = apply_features(case, original.read_bytes())
        verify(control["compiled_source"], control["compiled_sha256"])
        if Path(control["compiled_source"]).read_bytes() != adapted or control.get("test_adaptations", []) != adaptations:
            raise RuntimeError("Native assertions differ from the accepted original source: " + case)
        manifest_path = record["expanded_manifest"]
        verify(manifest_path, report["final_audit"]["expanded_manifests_sha256"][manifest_path])
        if manifest_path not in manifest_cache:
            manifest_cache[manifest_path] = json.loads(Path(manifest_path).read_text())
        expanded = manifest_cache[manifest_path]
        if Path(expanded["campaign_run"]).resolve() != campaign or expanded["campaign_receipt_sha256"] != translation_sha256:
            raise RuntimeError("Retained harness campaign differs: " + case)
        for result in checks["cases"]:
            if (result["case"] != case or result["status"] != "passed" or result["exit_code"] != 0
                or result["assertions"] <= 0 or result["assertions"] != control["assertions"]
                or result["native_assertions"] != control["assertions"]):
                raise RuntimeError("Original/native assertion mismatch: " + case)
            verify(result["log"], result["log_sha256"])
            transcript = Path(result["log"]).read_text()
            if (re.findall(r"PASSED\s+(\d+) tests", transcript) != [str(control["assertions"])]
                or re.search(r"\bSKIP(?:PED)?\b", transcript, re.I)):
                raise RuntimeError("Original execution transcript contains a failure/skip or changed count: " + case)
            selected = [r for r in expanded["executables"] if (r["case"], r["form"], r["mode"]) == (case, result["form"], result["mode"])]
            if len(selected) != 1 or selected[0]["status"] != "built":
                raise RuntimeError("Missing exact retained execution variant: " + case)
            entry = selected[0]
            if entry["command"] != result["command"] or entry.get("test_adaptations", []) != adaptations:
                raise RuntimeError("Completed command/source adaptation identity changed: " + case)
            verify(entry["compiled_source"], entry["compiled_sha256"])
            if Path(entry["compiled_source"]).read_bytes() != b'#include "utils/s2n_prelude.h"\n#include "dotcc_s2n_host.h"\n' + adapted:
                raise RuntimeError("Translated original assertions changed: " + case)
            for path, expected in entry["dependency_inputs_sha256"].items():
                verify(path, expected)
            identity_path = entry["application_execution_receipt"]
            verify(identity_path, entry["application_execution_receipt_sha256"])
            identity = json.loads(Path(identity_path).read_text())
            if (identity["actual_runtime"]["kind"] != "actual-runtime-identity" or identity["command"] != result["command"]
                or identity["actual_runtime"]["dynamic_code"] != (result["mode"] == "jit")
                or result["application_execution_receipt"] != identity_path
                or result["application_execution_receipt_sha256"] != entry["application_execution_receipt_sha256"]):
                raise RuntimeError("Missing actual application/runtime identity: " + case)
            _verify_test_provider_snapshot(entry, identity, verify)
            for files in (entry["execution_sha256"], identity["output_sha256"], identity["runtime_closure_sha256"],
                          {str(Path(entry["project"]).parent / name): value for name, value in entry["source_sha256"].items()}):
                for path, expected in files.items():
                    verify(path, expected)
            # Require the current authored inputs actually used by this
            # harness/provider. Other captured owning-API/sample files do not
            # participate in these standalone original C applications.
            provider = ROOT / "src/CryptoProvider"
            provider_files = {str(p) for p in provider.rglob("*") if p.is_file()
                              and p.suffix in (".cs", ".csproj", ".props", ".targets", ".json")
                              and not {"bin", "obj"} & set(p.relative_to(provider).parts)}
            captured_provider = {p for p in identity["authored_source_sha256"] if Path(p).is_relative_to(provider)}
            if provider_files != captured_provider:
                raise RuntimeError("Current provider source inventory differs from the qualified application")
            project = Path(entry["project"])
            used = provider_files | {str((project.parent / node.get("Include")).resolve())
                for node in ET.parse(project).iter("Compile") if node.get("Include")}
            for directory in (ROOT, ROOT.parent):
                used.update(str(directory / name) for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props", "global.json", "NuGet.Config")
                            if (directory / name).is_file())
            for path in used:
                verify(path, identity["authored_source_sha256"][path])
            rows.append(dict(result, c_report=str(report_path), original_assertion_receipt=qualification["receipt"],
                             original_assertion_receipt_sha256=qualification["sha256"], executable_manifest=manifest_path))
    return {"status": "passed", "campaign_run": str(campaign), "cases": rows,
            "campaign_receipt": str(translation_path), "campaign_receipt_sha256": translation_sha256,
            "counts": {"passed": len(rows), "failed": 0}, "c_report": str(report_path),
            "c_report_sha256": sha(report_path), "native_profile_match": match, "files_checked": len(checked)}


def validate_completed_report(report_path, native_report, native_equivalence, *, campaign_run=None):
    """Deeply validate all 1028 retained C executions, without running any.

    Returns the validated dictionary described above; raises RuntimeError for
    malformed, partial, failed, stale, or content-mismatched evidence.
    """
    try:
        return _validate_completed_report(report_path, native_report, native_equivalence, campaign_run=campaign_run)
    except (OSError, ValueError, KeyError, TypeError) as error:
        raise RuntimeError("Malformed/missing completed C evidence: " + str(error)) from error
