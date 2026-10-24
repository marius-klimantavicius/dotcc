"""Audit preserved all-four assertion evidence and stage optional package storage.

Read-only with respect to every historical input. No application, provider,
native oracle, compiler, expander or campaign suite is executed. Staged DLLs
are package copies, never labelled as freshly executed applications.
"""
import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
sys.path.insert(0, str(ROOT / "tests/Provider"))
from campaigns.delivery import source_files
from managed_test_storage import (audit_build_caches, contained_output, optional_storage,
                                  require_storage_reserve, validate_output_root, verify_files)
from qualified_dependencies import DLL, dependency_store, replace_identical, store_member


def digest(path):
    with Path(path).open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def audit(manifest, qualification, previous_audit, case, output_root, output_roots):
    inputs = [manifest, qualification, previous_audit]
    immutable = {str(p): digest(p) for p in inputs}
    verify_files(immutable)
    prior = json.loads(previous_audit.read_text())
    if (prior["unchanged_expander_manifest"] != str(manifest) or
            prior["unchanged_expander_manifest_sha256"] != immutable[str(manifest)] or
            prior["actual_qualification"] != str(qualification) or
            prior["actual_qualification_sha256"] != immutable[str(qualification)]):
        raise RuntimeError("Previous independent audit does not bind these exact historical inputs")
    data = json.loads(manifest.read_text())
    proof = json.loads(qualification.read_text())
    variants = {(form, mode) for form in ("raw", "processed") for mode in ("jit", "aot")}
    rows = [row for row in data["executables"] if row["case"] == case]
    if (proof["status"] != "bounded-passed" or proof["counts"] != {"passed": 4, "failed": 0} or
            len(rows) != 4 or {(r["form"], r["mode"]) for r in rows} != variants or
            any(r["status"] != "built" for r in rows) or len(proof["cases"]) != 4 or
            {(r["form"], r["mode"]) for r in proof["cases"]} != variants):
        raise RuntimeError("Expected one exact preserved all-four qualification")
    originals = dict(immutable)

    def bind(files):
        for name, value in files.items():
            if name in originals and originals[name] != value:
                raise RuntimeError("Conflicting evidence hash: " + name)
            originals[name] = value

    bind(prior["checked_sha256"])
    cache_inputs, dependencies, results = {}, [], []
    source_trees = []
    for row in rows:
        found = [r for r in proof["cases"] if (r["form"], r["mode"]) == (row["form"], row["mode"])]
        result = found[0]
        if (result["case"] != case or result["status"] != "passed" or result["exit_code"] != 0 or
                result["assertions"] != result["native_assertions"] or result["assertions"] <= 0 or
                result["command"] != row["command"] or
                result["application_execution_receipt"] != row["application_execution_receipt"] or
                result["application_execution_receipt_sha256"] != row["application_execution_receipt_sha256"]):
            raise RuntimeError("Preserved assertions do not match the actual executable row")
        identity_path = Path(row["application_execution_receipt"])
        bind({str(identity_path): row["application_execution_receipt_sha256"], result["log"]: result["log_sha256"]})
        verify_files(originals)
        identity = json.loads(identity_path.read_text())
        if identity["command"] != row["command"]:
            raise RuntimeError("Actual application identity command differs")
        counts = re.findall(r"\bPASSED\s+(\d+) tests\s*$", Path(result["log"]).read_text(), re.M)
        if counts != [str(result["assertions"])]:
            raise RuntimeError("Original assertion transcript differs")
        bind(row["execution_sha256"])
        for field in ("output_sha256", "runtime_closure_sha256", "authored_source_sha256"):
            bind(identity[field])
        bind({identity["executable"]: identity["executable_sha256"]})
        for name, value in identity["managed_build_closure_sha256"].items():
            if name in cache_inputs and cache_inputs[name] != value:
                raise RuntimeError("Conflicting historical build input")
            cache_inputs[name] = value
        project_root = Path(row["project"]).parent
        bind({str(project_root / p): h for p, h in row["source_sha256"].items()})
        source_trees.append((project_root, row["source_sha256"]))
        if row["mode"] == "jit":
            paths = [Path(p) for p in identity["output_sha256"] if Path(p).name == DLL]
            if len(paths) != 1:
                raise RuntimeError("Expected one actual JIT package dependency")
            dependencies.append((row["form"], paths[0], identity["output_sha256"][str(paths[0])]))
        results.append(dict(form=row["form"], mode=row["mode"], assertions=result["assertions"],
                            native_assertions=result["native_assertions"], log=result["log"],
                            command=result["command"], execution="preserved; not rerun"))
    if len({h for _, _, h in dependencies}) != 1:
        raise RuntimeError("Actual JIT dependency bytes differ across forms")

    def verify_originals():
        verify_files(originals)
        for directory, expected in source_trees:
            actual = {p.relative_to(directory).as_posix(): digest(p) for p in source_files(directory)}
            if actual != expected:
                raise RuntimeError("Preserved source tree changed: " + str(directory))

    verify_originals()
    caches = audit_build_caches(cache_inputs, data.get("cache_cleanup", {}).get(case, []), required=originals)
    output_root = validate_output_root(output_root, protected_roots=(ROOT.parent, *(p.parent for p in inputs)))
    if not output_roots:
        raise RuntimeError("Explicit dependency-output-root is required for new staging")
    output_roots = [validate_output_root(p, protected_roots=(ROOT.parent,)) for p in output_roots]
    for path in output_roots:
        contained_output(path, output_root)
    reserves = require_storage_reserve([ROOT, output_root, *output_roots])
    output_root.mkdir(parents=True, exist_ok=False, mode=0o700)
    staged, package_rows = {}, []
    report = dict(status="failed", scope="Preserved assertion audit and new package-storage staging only",
                  historical_manifest_status=data["status"], historical_case_failures=data.get("case_failures", {}),
                  canonical_adoption=False, executions_launched=0, assertions=results,
                  preserved_sha256=originals, build_cache_inventory=caches,
                  dependency_output_roots=list(map(str, output_roots)), storage_reserves_before=reserves,
                  infrastructure_sha256={str(p): digest(p) for p in
                      (Path(__file__).resolve(), ROOT.parent / "Scripts/managed_test_storage.py",
                       ROOT / "tests/Provider/qualified_dependencies.py")})

    def verify():
        verify_originals()
        verify_files(staged)

    try:
        # Fresh copies have separate inodes from the retained actual outputs.
        # Only these package copies may be optimized by the storage caller.
        for form, original, expected in dependencies:
            destination = output_roots[0] / form / DLL
            contained_output(destination, output_roots[0])
            store = dependency_store(destination, output_roots=output_roots)
            destination.parent.mkdir(parents=True)
            shutil.copyfile(original, destination)
            staged[str(destination)] = expected
            package_rows.append(dict(form=form, original=str(original), path=str(destination), sha256=expected,
                                     store=str(store), role="unexecuted package dependency copy"))
        verify()

        def optimize():
            replacements, stores = [], {}
            for row in package_rows:
                store = dependency_store(Path(row["path"]), output_roots=output_roots)
                if store not in stores:
                    member, created = store_member(Path(row["original"]), row["sha256"], store=store)
                    stores[store] = dict(path=str(member), sha256=row["sha256"], created=created,
                                        created_allocated_bytes=member.stat().st_blocks * 512 if created else 0)
                replaced = replace_identical(Path(row["path"]), Path(stores[store]["path"]), row["sha256"])
                replaced["role"] = row["role"]
                replacements.append(replaced)
            gross = sum(r["allocated_bytes_released"] for r in replacements)
            cost = sum(r["created_allocated_bytes"] for r in stores.values())
            return dict(status="passed", replacements=replacements, stores=list(stores.values()),
                        allocated_bytes_released=gross, newly_allocated_store_bytes=cost,
                        net_allocated_bytes_released=gross - cost)

        report["optional_storage"] = optional_storage(optimize, verify=verify)
        report.update(status="passed-preserved-evidence", staged_package_files=package_rows,
                      preserved_files_verified=len(originals),
                      storage_reserves_after=require_storage_reserve([ROOT, output_root]))
        verify()
    except BaseException as error:
        report["error"] = str(error)
        raise
    finally:
        (output_root / "receipt.json").write_text(json.dumps(report, indent=2) + "\n")
    return report


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--manifest", type=Path, required=True)
    parser.add_argument("--qualification", type=Path, required=True)
    parser.add_argument("--previous-audit", type=Path, required=True)
    parser.add_argument("--case", required=True)
    parser.add_argument("--output-root", type=Path, required=True)
    parser.add_argument("--dependency-output-root", type=Path, action="append", required=True)
    args = parser.parse_args()
    audit(args.manifest, args.qualification, args.previous_audit, args.case, args.output_root, args.dependency_output_root)
    print("PRESERVED BUILD AUDIT", args.output_root / "receipt.json")


if __name__ == "__main__":
    main()
