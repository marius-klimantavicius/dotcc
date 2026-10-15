#!/usr/bin/env python3
"""Prepare an explicit retain-all library manifest; bindings remain candidates.

Reads only accepted public report/plan JSON and pinned files. Does not translate,
execute SQLite, resolve arbitrary exports, change source, or create completion.
"""
from __future__ import annotations
import argparse
import copy
import hashlib
import json
from pathlib import Path

ALIASES = {"__USER_LABEL_PREFIX__" + name: name for name in (
    "open64", "stat64", "fstat64", "ftruncate64", "fcntl64", "pread64", "pwrite64", "mmap64", "lstat64")}


def digest(path: Path) -> str:
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def receipt(path: Path) -> dict:
    return {"path": str(path.resolve()), "sha256": digest(path), "bytes": path.stat().st_size}


def main() -> int:
    here = Path(__file__).resolve().parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, default=here / "input.json")
    parser.add_argument("--provider-plan", type=Path, default=here / "provider-plan.json")
    parser.add_argument("--inventory", type=Path, default=Path("/mnt/storage/roslynobj-csharp-20261015/t0/sqlite-inventory-address-closure/program-report.json"))
    parser.add_argument("--runtime-project", type=Path, help="Select an explicit frozen target runtime project for reproducible builds.")
    parser.add_argument("--manifest", type=Path, required=True)
    parser.add_argument("--report", type=Path, required=True)
    parser.add_argument("--output-directory", type=Path, required=True)
    args = parser.parse_args()
    for path in (args.manifest, args.report):
        if path.exists(): parser.error("Choose fresh manifest/report paths; preparation does not overwrite evidence.")
    plan = json.loads(args.provider_plan.read_text())
    inventory = json.loads(args.inventory.read_text())
    original = json.loads(args.input.read_text())
    if digest(args.inventory) != plan["acceptedInventoryReportSha256"] or not inventory["inventoryComplete"]:
        raise RuntimeError("Inventory differs from the accepted public plan/report identity.")
    audit_path = Path(plan["sourceAudit"]["path"])
    if digest(audit_path) != plan["sourceAudit"]["sha256"]:
        raise RuntimeError("Provider plan's accepted import audit changed.")
    if plan["explicitAliases"] != ALIASES or (plan["requiredImports"], plan["resolvedNativeExports"], plan["targetIntrinsics"]) != (103, 88, 15):
        raise RuntimeError("The explicitly reviewed import/alias scope changed.")
    if len(original["inputs"]) != 1 or len(inventory["inputs"]) != 1:
        raise RuntimeError("This candidate requires the one unchanged full SQLite object.")
    source_input, observed_input = original["inputs"][0], inventory["inputs"][0]
    object_path = Path(source_input["path"])
    if not object_path.is_absolute(): object_path = (args.input.resolve().parent / object_path).resolve()
    if (digest(object_path) != source_input["sha256"] or observed_input["sha256"] != source_input["sha256"]
        or observed_input["artifactIdentity"] != source_input["artifactIdentity"]):
        raise RuntimeError("The immutable full SQLite input differs from the accepted artifact.")
    required = {(item["kind"], item["linkageName"]): item for item in inventory["imports"] if item["required"]}
    functions = {item["locator"]: item for item in inventory["functions"]}
    providers = {item["path"]: item for item in plan["providerFiles"]}
    provider_receipts = []
    for path, recorded in providers.items():
        actual = receipt(Path(path))
        if actual["sha256"] != recorded["sha256"] or actual["bytes"] != recorded["bytes"]:
            raise RuntimeError("Pinned system provider changed: " + path)
        provider_receipts.append(actual)
    bindings, decisions, intrinsics = [], [], []
    seen = set()
    for item in plan["imports"]:
        captured = item["capturedImport"]
        key = (captured["kind"], captured["linkageName"])
        if key in seen or key not in required: raise RuntimeError("Repeated/unrequired planned import.")
        seen.add(key)
        actual = required[key]
        for field in ("locator", "name", "linkageName", "signature", "callingConvention", "directCalls", "addressReferences", "addressExposed"):
            if actual[field] != captured[field]: raise RuntimeError("Planned import differs from public inventory: " + str(key))
        selected = functions[actual["locator"]]
        if selected["signature"] != item["selectedSignature"]:
            raise RuntimeError("Selected signature differs from accepted provider plan.")
        if item["candidateProviderGroup"] == "target-intrinsic":
            if selected["builtin"] != item["recordedBuiltin"] or item.get("nativeExportLookup") is not False:
                raise RuntimeError("A recorded intrinsic acquired a native fallback.")
            intrinsics.append({"linkageName": actual["linkageName"], "builtin": selected["builtin"],
                "signature": actual["signature"], "nativeBinding": False, "executionAcceptance": "separate intrinsic evidence"})
            continue
        linkage, entry = actual["linkageName"], item["hostEntryPoint"]
        expected_entry = ALIASES.get(linkage, linkage)
        if (entry != expected_entry or item["explicitProviderAlias"] != (linkage in ALIASES)
            or not item["exportResolved"] or captured["kind"] != "function" or selected["builtin"] is not None):
            raise RuntimeError("Unreviewed alias/provider or intrinsic replacement: " + str(key))
        path = item["requestedProviderPath"]
        if path not in providers: raise RuntimeError("Requested provider has no pinned system-file receipt.")
        contract = item["contracts"]
        if any(not contract[key].strip() for key in ("ownership", "errors", "threads", "callbacks")):
            raise RuntimeError("A candidate provider lacks an explicit contract.")
        binding = {"linkageName": linkage, "kind": "function", "provider": item["candidateProviderGroup"],
            "symbol": entry, "libraryPath": path, "librarySha256": providers[path]["sha256"],
            "signature": actual["signature"], "callingConvention": actual["callingConvention"],
            "ownership": contract["ownership"], "errorContract": contract["errors"],
            "threadContract": contract["threads"], "callbackContract": contract["callbacks"]}
        bindings.append(binding)
        decisions.append({"binding": binding, "selectedSignature": item["selectedSignature"],
            "alias": linkage in ALIASES, "plannedResolvedTarget": item["actualProvider"],
            "status": "candidate-binding-not-whole-program-runtime-accepted", "runtimeAbiExercisedByThisTool": False})
    if seen != set(required) or len(bindings) != 88 or len(intrinsics) != 15:
        raise RuntimeError("Candidate reconciliation did not account for exactly all 103 requirements.")
    candidate = copy.deepcopy(original)
    candidate["inputs"][0]["path"] = str(object_path)
    runtime_path = args.runtime_project.resolve() if args.runtime_project is not None else Path(candidate["runtime"]["runtimeProjectPath"])
    if not runtime_path.is_absolute(): runtime_path = (args.input.resolve().parent / runtime_path).resolve()
    if not runtime_path.is_file(): raise RuntimeError("Runtime project is absent.")
    candidate["runtime"].update({"providers": sorted({binding["provider"] for binding in bindings}), "bindings": bindings,
        "runtimeProjectPath": str(runtime_path), "nativeErrorProtocol": "linuxX64LogicalErrno"})
    candidate["roots"] = {"mode": "library", "exports": [], "retainAllDefinitions": True}
    candidate["output"].update({"directory": str(args.output_directory.resolve()), "namespaceName": "Translated", "className": "Program"})
    report = {"status": "prepared-candidate-manifest", "generatedExecution": False, "translationComplete": False,
        "wholeProviderCoverageAccepted": False, "nativeApplicationFallbackPermitted": False,
        "sourceManifest": receipt(args.input), "acceptedInventory": receipt(args.inventory), "providerPlan": receipt(args.provider_plan),
        "publicImportAudit": receipt(audit_path), "object": receipt(object_path), "tool": receipt(Path(__file__)),
        "targetRuntimeProject": receipt(runtime_path),
        "providerFiles": provider_receipts, "bindingCount": 88, "unboundRecordedIntrinsicCount": 15,
        "explicitAliases": ALIASES, "bindings": decisions, "intrinsics": intrinsics,
        "limits": ["Export presence/contract preservation is configuration preparation, not actual 88-provider ABI/error/thread/lifetime execution evidence",
            "Candidate is retain-all library translation of the unchanged full object; no SQLite source/body/native application fallback",
            "Normal libc IFUNC/vDSO resolved targets retain their requested pinned libc binding; no artificial hash requirement for virtual images",
            "Actual generated API surface, native dependencies, SQL differential execution and remaining T4 areas need separate receipts"]}
    args.manifest.parent.mkdir(parents=True, exist_ok=True)
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.manifest.write_text(json.dumps(candidate, indent=2) + "\n")
    report["candidateManifest"] = receipt(args.manifest)
    args.report.write_text(json.dumps(report, indent=2) + "\n")
    print(json.dumps({"status": report["status"], "bindings": len(bindings), "unboundIntrinsics": len(intrinsics), "manifest": str(args.manifest)}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
