"""Required, physical-source typed primitive selection for core and test units."""
from __future__ import annotations
import hashlib
import json
import os
from pathlib import Path
import re
import xml.etree.ElementTree as ET

ADAPTER = "src/Optimization/Primitives.cs"


def inventory(root):
    return json.loads((root / "config/primitive-inventory.json").read_text())


def all_names(root):
    return sorted(inventory(root)["contracts"])


def check_headers(ctx, source):
    # Historical hashes are optional provenance; exact typed selection and the
    # differential tests establish compatibility. Honor the campaign policy.
    for name, expected in inventory(ctx.root)["header_sha256"].items():
        ctx.policy.check(source / name, expected)


def unit_flags(ctx, unit, source, names):
    """Stage exact per-invocation rules. Test-only TUs must specify expected names.

    source is the directory containing lib/, unit the actual physical C file.
    portable uses replacements; original supplies no profile. validate_report
    must still be called on every resulting object report, including empty sets.
    """
    if ctx.profile == "original":
        return ()
    names = sorted(names)
    rules = json.loads((ctx.root / "config/primitive-overrides.json").read_text())["functionOverrides"]
    by_name = {rule["name"]: rule for rule in rules}
    if len(by_name) != 42 or set(by_name) != set(all_names(ctx.root)) or not set(names) <= set(by_name):
        raise RuntimeError("Primitive rule inventory differs")
    check_headers(ctx, source)
    selected = []
    for name in names:
        rule = dict(by_name[name])
        rule["translationUnit"] = str(unit.resolve())
        rule["declarationFile"] = str((source / "lib/common" / Path(rule["declarationFile"]).name).resolve())
        selected.append(rule)
    directory = ctx.work / "primitive-profiles"
    directory.mkdir(exist_ok=True)
    path = directory / (unit.stem + "-" + hashlib.sha256(str(unit.resolve()).encode()).hexdigest()[:12] + ".json")
    path.write_text(json.dumps(dict(version=1, functionOverrides=selected), indent=2) + "\n")
    return ("--overrides-file", str(path))


def validate_report(ctx, report, unit, source, names):
    expected = set() if ctx.profile == "original" else set(names)
    rows = [json.loads(line) for line in report.read_text().splitlines() if line]
    selected = [row for row in rows if row.get("event") == "function-override"]
    actual = [row["name"] for row in selected]
    if len(actual) != len(expected) or set(actual) != expected or any(row.get("event") == "function-override-unmatched" for row in rows):
        raise RuntimeError(f"Primitive selection differs for {unit}: expected={sorted(expected)}, actual={actual}")
    contracts = inventory(ctx.root)["contracts"]
    rules = {rule["name"]: rule for rule in json.loads((ctx.root / "config/primitive-overrides.json").read_text())["functionOverrides"]}
    for row in selected:
        rule = rules[row["name"]]
        header = source / "lib/common" / Path(rule["declarationFile"]).name
        origin = re.fullmatch(r"(.+):functionOverrides\[(\d+)\]", row.get("origin", ""))
        if origin is None:
            raise RuntimeError("Missing primitive rule provenance: " + str(row))
        profile = json.loads(Path(origin[1]).read_text())
        selected_rule = profile["functionOverrides"][int(origin[2])]
        if (row["signature"] != contracts[row["name"]]
                or row["target"] != "managedMethod:" + rule["target"]["method"]
                or Path(row["translationUnit"]).resolve() != unit.resolve()
                or Path(row["declarationFile"]).resolve() != header.resolve()
                or row.get("matches") != "1"
                or selected_rule != dict(rule, translationUnit=str(unit.resolve()), declarationFile=str(header.resolve()))):
            raise RuntimeError("Primitive selection provenance differs: " + str(row))
    return dict(unit=str(unit), report=str(report), expected=sorted(expected), selected=selected)


def validate_objects(ctx, reports):
    source = ctx.work / "source"
    expected = inventory(ctx.root)["expected_units"]
    policy = json.loads((ctx.root / "config/translation.json").read_text())
    by_path = {str((source / name).resolve()): names for name, names in expected.items() if name in policy["units"]}
    by_path[str((ctx.root / policy["bridge"]).resolve())] = expected[policy["bridge"]]
    units = [Path(name) for name in ctx.receipt["units"]]
    if len(units) != len(reports):
        raise RuntimeError("Primitive per-object reports incomplete")
    records = [validate_report(ctx, report, unit, source, by_path[str(unit.resolve())])
               for unit, report in zip(units, reports)]
    ctx.receipt["primitive_overrides"] = dict(profile=ctx.profile, rules=str(ctx.root / "config/primitive-overrides.json"),
        inventory=str(ctx.root / "config/primitive-inventory.json"), adapter=str(ctx.root / ADAPTER) if ctx.profile != "original" else None,
        units=records, selections=sum(len(row["selected"]) for row in records))


def configure(ctx, project):
    """Link the one original authored file before semantic processing and builds."""
    if ctx.profile == "original":
        return
    path = ctx.root / ADAPTER
    tree = ET.parse(project)
    nodes = [node for node in tree.iter("Compile") if node.get("Include")
             and (project.parent / node.get("Include")).resolve() == path.resolve()]
    if len(nodes) > 1:
        raise RuntimeError("Duplicate primitive adapter source link")
    if not nodes:
        group = ET.SubElement(tree.getroot(), "ItemGroup")
        ET.SubElement(group, "Compile", Include=os.path.relpath(path, project.parent).replace(os.sep, "/"), Link="Optimization/Primitives.cs")
    else:
        nodes[0].set("Include", os.path.relpath(path, project.parent).replace(os.sep, "/"))
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
