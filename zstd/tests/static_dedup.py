#!/usr/bin/env python3
"""Capture and compare receipt-bound zstd source/body/binary sizes without builds."""
from __future__ import annotations

import argparse
from collections import Counter
import datetime
import hashlib
import json
from pathlib import Path
import re
import subprocess

REPO = Path(__file__).resolve().parents[2]
METHOD = re.compile(
    r"^    public static (?:unsafe )?[^\n=;{}]+? (?P<name>[A-Za-z_][A-Za-z0-9_]*)"
    r"\([^\n]*\)\n    \{(?P<body>.*?)^    \}", re.MULTILINE | re.DOTALL)
UNIT = re.compile(r"__unit_[A-F0-9]+")
HASH = re.compile(r"ZSTD_hash(?:[3-8](?:PtrS?)?|Ptr(?:Salted)?)\Z")
REQUIRED = {"translation", *("test:" + name for name in (
    "native", "primitives", "abi", "consumer", "behavior", "lifecycle", "interop", "upstream", "audit"))}


def digest(path):
    with path.open("rb") as stream:
        return hashlib.file_digest(stream, "sha256").hexdigest()


def file_record(path, expected=None, retained=None):
    path = path.resolve()
    actual = digest(path)
    if expected is not None and actual != expected:
        raise RuntimeError("Receipt-bound file changed: " + str(path))
    row = dict(path=str(path), bytes=path.stat().st_size, sha256=actual)
    if retained is not None and retained.is_file():
        if digest(retained) != actual:
            raise RuntimeError("Retained copy differs: " + str(retained))
        row["retained_path"] = str(retained.resolve())
    return row


def source_hashes(receipt):
    return {name.split("/source/", 1)[-1]: value for name, value in receipt["input_hashes"].items()}


def capture(receipt_path, source_tree=None):
    receipt_path = receipt_path.resolve()
    receipt = json.loads(receipt_path.read_text())
    passed = {row["name"] for row in receipt["tasks"] if row["status"] == "passed"}
    if receipt["status"] != "passed" or REQUIRED - passed:
        raise RuntimeError("A complete successful zstd verification receipt is required")
    if receipt["project"] != "zstd" or receipt["profile"] != "portable":
        raise RuntimeError("The portable zstd product is required")
    if receipt["selection"]["form"] != "all" or receipt["selection"]["mode"] != "all":
        raise RuntimeError("Both source forms and both execution modes are required")
    result = dict(schema_version=1, status="passed", created_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),
                  scope="Exact receipt-bound generated sources and qualified managed harness binaries; no timing or speed claim",
                  git_head=subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=REPO, text=True).strip(),
                  receipt=file_record(receipt_path), selection=receipt["selection"], tools=receipt["tools"],
                  source_tree=str(source_tree.resolve()) if source_tree else None,
                  generator=file_record(Path(__file__)), source_input_hashes=source_hashes(receipt),
                  production_inputs=receipt["production_inputs"], unit_count=len(receipt["units"]),
                  primitive_selection={key: receipt["primitive_overrides"][key] for key in ("selections", "target_kinds")},
                  method_scope="Public static method bodies in Zstd.[0-9]*.cs; excludes primary Zstd.cs runtime/types/globals",
                  body_normalization="Remove __unit_HEX suffixes only for diagnostic grouping; exact file hashes remain unnormalized",
                  forms={})
    retained_tree = receipt_path.parent / "copied-delivery/tree"
    for form in ("raw", "processed"):
        entry = receipt["outputs"][form]
        directory = Path(entry["project"]).parent
        files, methods, families = {}, [], {}
        for name, expected in sorted(entry["hashes"].items()):
            published = directory / name
            relative = published.relative_to(REPO)
            path = source_tree / relative if source_tree else published
            files[name] = file_record(path, expected, retained_tree / relative)
            if re.fullmatch(r"Zstd\.[0-9]+\.cs", name):
                text = path.read_text()
                for match in METHOD.finditer(text):
                    methods.append(match["name"])
                    family = UNIT.sub("", match["name"])
                    if HASH.fullmatch(family):
                        normalized = UNIT.sub("", match["body"])
                        row = families.setdefault(family, dict(declarations=0, normalized_bodies=Counter()))
                        row["declarations"] += 1
                        row["normalized_bodies"][hashlib.sha256(normalized.encode()).hexdigest()] += 1
        if not methods or len(set(methods)) != len(methods):
            raise RuntimeError("Missing or duplicate generated method declarations: " + form)
        required_hashes = {"ZSTD_hash" + str(n) + "Ptr" for n in range(4, 9)}
        if required_hashes - families.keys():
            raise RuntimeError("Missing selected hash helper declarations: " + form)
        sources = {name: value for name, value in files.items() if name.endswith(".cs")}
        split = {name: value for name, value in sources.items() if re.fullmatch(r"Zstd\.[0-9]+\.cs", name)}
        cells = {}
        for harness in receipt["zstd_managed_harnesses"]:
            if harness["form"] != form:
                continue
            mode = harness["mode"]
            if mode in cells or not harness["whole_library_rooted"]:
                raise RuntimeError("Duplicate or unrooted managed qualification cell")
            cells[mode] = dict(harness=file_record(Path(harness["executable"]), harness["executable_sha256"]))
            if mode == "jit":
                cells[mode]["product_assembly"] = file_record(Path(harness["output"]) / "TranslatedZstd.dll")
            else:
                cells[mode]["method_map"] = file_record(Path(harness["aot_map"]), harness["aot_map_sha256"])
        if set(cells) != {"jit", "aot"}:
            raise RuntimeError("Missing qualified binary cell: " + form)
        result["forms"][form] = dict(files=files, source_files=len(sources), source_bytes=sum(r["bytes"] for r in sources.values()),
            translated_function_files=len(split), translated_function_source_bytes=sum(r["bytes"] for r in split.values()),
            method_declarations=len(methods), unit_suffixed_declarations=sum(bool(UNIT.search(name)) for name in methods),
            hash_helpers=families, cells=cells)
    return result


def verify_previous(snapshot):
    """Use preserved delivery source bytes after canonical products are replaced."""
    rows = [snapshot["receipt"]]
    for form in snapshot["forms"].values():
        rows.extend(form["files"].values())
        for cell in form["cells"].values():
            rows.extend(cell.values())
    for row in rows:
        path = Path(row.get("retained_path", row["path"]))
        if digest(path) != row["sha256"]:
            raise RuntimeError("Baseline evidence changed: " + str(path))
    return len(rows)


def compare(before, after):
    result = dict(status="passed", baseline_files_rehashed=verify_previous(before), forms={})
    for name in ("unit_count", "primitive_selection", "source_input_hashes"):
        if before[name] != after[name]:
            raise RuntimeError("Source/primitive selection changed: " + name)
    recipe = str(REPO / "zstd/scripts/campaign.py")
    original_inputs = {name: value for name, value in before["production_inputs"].items() if name != recipe}
    final_inputs = {name: value for name, value in after["production_inputs"].items() if name != recipe}
    result["production_input_changes"] = sorted(name for name in original_inputs.keys() | final_inputs.keys()
                                               if original_inputs.get(name) != final_inputs.get(name))
    if result["production_input_changes"]:
        raise RuntimeError("Production inputs changed beyond the zstd recipe: " + repr(result["production_input_changes"]))
    for form in ("raw", "processed"):
        old, new = before["forms"][form], after["forms"][form]
        changes = {}
        for field in ("source_files", "source_bytes", "translated_function_files", "translated_function_source_bytes",
                      "method_declarations", "unit_suffixed_declarations"):
            changes[field] = dict(before=old[field], after=new[field], delta=new[field] - old[field])
        changes["hash_helpers"] = {name: dict(before=old["hash_helpers"][name]["declarations"],
            after=new["hash_helpers"].get(name, {}).get("declarations", 0)) for name in old["hash_helpers"]}
        for name in ("ZSTD_hash" + str(n) + "Ptr" for n in range(4, 9)):
            if old["hash_helpers"][name]["declarations"] <= 1 or new["hash_helpers"][name]["declarations"] != 1:
                raise RuntimeError("Selected pointer hash helper did not collapse: " + form + ":" + name)
        for name in ("method_declarations", "translated_function_source_bytes"):
            if new[name] >= old[name]:
                raise RuntimeError("Static deduplication did not reduce " + form + ":" + name)
        changes["binaries"] = {}
        for mode, field in (("jit", "product_assembly"), ("aot", "harness")):
            a, b = old["cells"][mode][field]["bytes"], new["cells"][mode][field]["bytes"]
            changes["binaries"][mode] = dict(before=a, after=b, delta=b - a)
        result["forms"][form] = changes
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--receipt", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--baseline", type=Path)
    parser.add_argument("--source-tree", type=Path,
                        help="Exact copied-delivery tree for an older receipt whose canonical products have been replaced")
    options = parser.parse_args()
    if options.output.exists():
        raise RuntimeError("Refusing to overwrite evidence: " + str(options.output))
    report = capture(options.receipt, options.source_tree)
    if options.baseline:
        baseline = json.loads(options.baseline.read_text())
        report["baseline"] = file_record(options.baseline)
        report["comparison"] = compare(baseline, report)
    options.output.parent.mkdir(parents=True, exist_ok=True)
    with options.output.open("x") as stream:
        json.dump(report, stream, indent=2)
        stream.write("\n")
    print(options.output)


if __name__ == "__main__":
    main()
