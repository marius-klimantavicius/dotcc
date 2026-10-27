"""Full Zydis library recipe; native controls reuse the same source policy."""
import json
import os
from pathlib import Path
import re
import shutil
import xml.etree.ElementTree as ET

from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def sources(root):
    pins = json_file(root / "config/inputs.json")
    inventory = json_file(root / "config/source-inventory.json")["inputs"]
    for name in ("zydis", "zycore"):
        pin = pins[name]
        yield Source(
            name, pin["url"], root / "ref" / pin["archive"],
            root / "ref" / pin["directory"], pin["prefix"], pin.get("sha256"),
            tuple(pin["required"]), pin["group"],
            files={row["path"]: row["sha256"] for row in inventory[name]["files"]},
            archive_paths=tuple(pin.get("archivePaths", ())),
        )


def validate_source_manifest(root, source_paths):
    """Check structural closure in every hash mode; edited usable trees are valid."""
    policy = json_file(root / "config/translation.json")
    zydis = source_paths["zydis"]
    selected = policy["units"]["zydis"]
    observed = {path.relative_to(zydis).as_posix()
                for path in (zydis / "src").rglob("*.c")}
    declared = set(re.findall(r'"(src/[^"\n]+\.c)"',
                              (zydis / "CMakeLists.txt").read_text()))
    if len(selected) != len(set(selected)) or set(selected) != observed or set(selected) != declared:
        raise RuntimeError("Full Zydis source manifest differs from library sources/CMake: "
                           f"selected={sorted(selected)}, observed={sorted(observed)}, "
                           f"cmake={sorted(declared)}")
    inventory = json_file(root / "config/source-inventory.json")["inputs"]
    required = {"zydis": [row["path"] for row in inventory["zydis"]["files"]
                           if row["path"].startswith(("include/", "src/"))],
                "zycore": policy["zycoreClosure"]["headers"]}
    for name, unit_names in policy["units"].items():
        if len(unit_names) != len(set(unit_names)):
            raise RuntimeError(f"Duplicate {name} translation units")
        required[name].extend(unit_names)
    for notice in policy["notices"]:
        required[notice["source"]].append(notice["path"])
    for name, names in required.items():
        base = source_paths[name].resolve()
        for relative in names:
            path = base / relative
            if not path.resolve().is_relative_to(base) or not path.is_file():
                raise RuntimeError(f"Missing/unsafe required library input: {path}")
    return policy


def native_configuration(root, source_paths=None):
    """Read-only native/qualification hook: units + flags are the actual profile.

    source_paths may be ctx.sources; otherwise the acquired metadata paths are
    used. Native runners append their own test adapters and output options.
    DotCC-only export/link flags are deliberately separate from native flags.
    """
    root = Path(root).resolve()
    if source_paths is None:
        source_paths = {source.name: source.directory for source in sources(root)}
    source_paths = {name: Path(path) for name, path in source_paths.items()}
    policy = validate_source_manifest(root, source_paths)
    includes = [source_paths[row["source"]] / row["path"]
                for row in policy["includeDirectories"]]
    units = [source_paths[name] / relative
             for name, names in policy["units"].items() for relative in names]
    cflags = ["-std=" + policy["dialect"], *flags(policy["defines"], includes),
              *("-U" + name for name in policy["undefines"])]
    return {"profile": policy["profile"], "sources": source_paths, "units": units,
            "flags": cflags, "includes": includes, "defines": policy["defines"],
            "undefines": policy["undefines"], "adaptations": policy["adaptations"]}


def prepare(ctx):
    policy = json_file(ctx.root / "config/translation.json")
    selected = native_configuration(ctx.root, ctx.sources)
    # References need no staged changes: no text patches, generated headers or
    # optional CMake backend detection are used by this full library profile.
    if policy["adaptations"]:
        raise RuntimeError("No guarded Zydis source adaptation has been implemented")
    ctx.receipt["zydis_selection"] = {
        "configuration": str(ctx.root / "config/translation.json"),
        "source_inventory": str(ctx.root / "config/source-inventory.json"),
        "sources": {name: str(path) for name, path in selected["sources"].items()},
        "units": {name: names for name, names in policy["units"].items()},
        "native_flags": selected["flags"], "features": policy["features"],
        "zycore_closure": policy["zycoreClosure"], "adaptations": [],
        "upstream_tools_in_product": False,
    }
    return Translation(
        [Unit(path) for path in selected["units"]],
        [*selected["flags"], *(part for pattern in policy["emitDefines"]
                               for part in ("--emit-define", pattern))],
        [*link_flags(policy["className"], policy["namespace"]),
         *(part for pattern in policy["exportEnums"] for part in ("--export-enum", pattern))],
        objects=True, configure=configure, companions=qualification_companions,
    )


def configure(ctx, project, form):
    """Link original authored API files and carry both upstream license texts."""
    policy = json_file(ctx.root / "config/translation.json")
    api = sorted({path for pattern in policy["authoredCompileGlobs"]
                  for path in ctx.root.glob(pattern) if path.is_file()})
    if not api:
        raise RuntimeError("Missing authored Zydis API: " + ", ".join(policy["authoredCompileGlobs"]))
    tree = ET.parse(project)
    group = next((node for node in tree.getroot().findall("ItemGroup")
                  if node.get("Label") == "ZydisAuthoredApi"), None)
    if group is None:
        group = ET.SubElement(tree.getroot(), "ItemGroup", Label="ZydisAuthoredApi")
    for path in api:
        link = "Authored/" + path.relative_to(ctx.root / "src").as_posix()
        nodes = [node for node in tree.iter("Compile") if node.get("Link") == link]
        if len(nodes) > 1:
            raise RuntimeError("Duplicate authored API link: " + link)
        node = nodes[0] if nodes else ET.SubElement(group, "Compile", Link=link)
        node.set("Include", os.path.relpath(path, project.parent))
    notices = []
    for row in policy["notices"]:
        target = row["target"]
        shutil.copyfile(ctx.sources[row["source"]] / row["path"], project.parent / target)
        node = next((item for item in tree.iter("None") if item.get("Update") == target), None)
        if node is None:
            node = ET.SubElement(group, "None", Update=target)
        node.set("CopyToOutputDirectory", "PreserveNewest")
        node.set("CopyToPublishDirectory", "PreserveNewest")
        notices.append(target)
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
    ctx.receipt.setdefault("zydis_delivery", {})[form] = {
        "authored_sources": [str(path.relative_to(ctx.root)) for path in api],
        "licenses": notices,
    }


def run_inventory(ctx, suite):
    selected = native_configuration(ctx.root, ctx.sources or None)
    result = {"units": [str(path) for path in selected["units"]],
              "flags": selected["flags"], "adaptations": selected["adaptations"]}
    output = ctx.artifacts / "source-manifest.json"
    output.write_text(json.dumps(result, indent=2) + "\n")
    ctx.receipt["zydis_source_manifest"] = str(output)


def run_qualification(ctx, suite):
    if suite.name == "audit":
        return helper(ctx.root / "scripts/audit.py").run(ctx, suite)
    policy = json_file(ctx.root / "config/translation.json")["tests"]
    path = ctx.root / policy["helper"]
    if not path.is_file():
        raise RuntimeError(f"Required {suite.name} runner is missing: {path}")
    runner = getattr(helper(path), policy["suiteEntrypoint"], None)
    if runner is None:
        raise RuntimeError(f"Required {suite.name} runner has no {policy['suiteEntrypoint']}(ctx, suite): {path}")
    return runner(ctx, suite)


def qualification_companions(ctx):
    return helper(ctx.root / "scripts/qualification.py").companions(ctx)


def run_managed_boundary(ctx, suite):
    """Qualify callbacks and owning spans on the delivered product itself."""
    expected = ["formatter-intel", "formatter-att", "formatter-status-failure",
                "formatter-contained-exception", "owning-snapshots-and-slices"]
    rows = []
    report = ctx.artifacts / "zydis-managed-boundary-report.json"
    summary = {"status": "running", "cells": rows}
    try:
        for form in forms(ctx):
            product = check_product(ctx, form)
            for mode in modes(ctx):
                label = f"managed-boundary-{form}-{mode}"
                output = ctx.work / label
                project = ctx.root / "tests/ManagedBoundary/ManagedBoundary.csproj"
                args = ["-p:TranslatedProject=" + str(product), "-m:1",
                        "--artifacts-path", output / "artifacts", "-o", output / "bin"]
                if mode == "aot":
                    ctx.managed("publish", project, label + "-publish", *args,
                                "-r", ctx.options.rid, "-p:PublishAot=true",
                                "-p:RootTranslatedLibrary=true", timeout=3600)
                    command = [output / "bin/Zydis.ManagedBoundary"]
                else:
                    ctx.managed("build", project, label + "-build", *args, timeout=1800)
                    command = ["dotnet", output / "bin/Zydis.ManagedBoundary.dll"]
                transcript = ctx.run(command, label + "-run", separate=True)
                actual = [line[5:] for line in transcript.splitlines() if line.startswith("PASS ")]
                if actual != [*expected, "managed-boundary cases=5"]:
                    raise RuntimeError("Incomplete managed boundary assertions: " + repr(actual))
                stderr = ctx.artifacts / (label + "-run.stderr")
                if stderr.read_bytes():
                    raise RuntimeError("Unexpected managed boundary stderr: " + str(stderr))
                rows.append(dict(form=form, mode=mode, product=str(product),
                                 rooted=mode == "aot", status="passed", passed=5,
                                 failed=0, skipped=0,
                                 transcript=str(ctx.artifacts / (label + "-run.log"))))
        summary["status"] = "passed"
    except BaseException as error:
        summary.update(status="failed", error=str(error))
        raise
    finally:
        report.write_text(json.dumps(summary, indent=2) + "\n")
        ctx.receipt["zydis_managed_boundary_report"] = str(report)
        ctx.save()


recipe = Recipe(
    "zydis", "TranslatedZydis", ("default",),
    ("samples/ManagedConsumer/ManagedConsumer.csproj",), sources, prepare,
    consumer=Consumer(property="TranslatedProject"),
    suites=(
        Suite("inventory", (), run=run_inventory,
              scope="complete full-feature library source/header/table/license closure; no emitted execution"),
        Suite("consumer", ("consumer",), matrix=True,
              scope="owning decode/format/encode workflow through ordinary project reference"),
        Suite("native", (), run=run_qualification, platforms=("linux",),
              scope="same-profile native assertions, actual ABI and differential reference transcripts"),
        *(Suite(name, (), run=run_qualification, matrix=True, platforms=("linux",))
          for name in ("abi", "qualification", "upstream", "rooted-aot")),
        Suite("managed-boundary", (), run=run_managed_boundary, matrix=True, platforms=("linux",),
              scope="actual delivered-product managed callbacks, rooting, exception containment and owning slices"),
        Suite("audit", (), run=run_qualification,
              scope="actual artifact dependency closure and exclusion of native Zydis backends/test adapters"),
    ),
    default_suites=("consumer",),
    verify_suites=("inventory", "native", "abi", "qualification", "upstream", "rooted-aot", "managed-boundary", "consumer", "audit"),
)
