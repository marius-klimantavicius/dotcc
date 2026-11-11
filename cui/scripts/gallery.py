"""Translate the original gallery as a separate, core-referencing companion."""
import hashlib
import json
import os
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from campaigns.recipes import flags, helper, json_file


def companions(ctx):
    config = json_file(ctx.root / "config/gallery.json")
    source = ctx.work / "source"
    objects = ctx.work / "gallery-objects"
    objects.mkdir()
    inputs = []
    common = ["-std=c99", "-U_WIN32", "-U__APPLE__", "-DGLFW_INCLUDE_NONE", "--source-root", str(source),
              *flags(json_file(ctx.root / "config/profile.json")["defines"],
                     [source / "dotcc/include", source / "include"])]
    for index, name in enumerate(config["sources"]):
        target = objects / f"{index:03d}-{Path(name).stem}.cs"
        ctx.run(["dotnet", ctx.compiler, *common, source / name, "--emit=obj", "-o", target],
                "gallery-" + Path(name).stem, timeout=ctx.options.timeout)
        inputs.append(target)
    output = ctx.work / "raw" / config["product"]
    core = ctx.work / "raw" / ctx.recipe.product
    ctx.run(["dotnet", ctx.compiler, *inputs, "--emit=managedlib",
             *helper(ctx.root / "scripts/codegen.py").link_flags(ctx.root, config["class"], config["namespace"]),
             "--managed-reference", core / "Dotcc.ManagedSymbols.json", "-o", output],
            "gallery-link", timeout=max(600, ctx.options.timeout))
    project = output / (config["product"] + ".csproj")
    tree = ET.parse(project)
    root = tree.getroot()
    # A portable sibling reference resolves privately for semantic processing,
    # and to the matching published form after transactional relocation.
    properties = ET.SubElement(root, "PropertyGroup")
    ET.SubElement(properties, "CuiCoreProject", Condition="'$(CuiCoreProject)' == ''").text = "../TranslatedCui/TranslatedCui.csproj"
    ET.SubElement(properties, "CuiCoreProject", Condition="$([System.String]::Copy('$(MSBuildProjectDirectory)').EndsWith('.Raw'))").text = "../TranslatedCui.Raw/TranslatedCui.csproj"
    group = ET.SubElement(root, "ItemGroup")
    ET.SubElement(group, "ProjectReference", Include="$(CuiCoreProject)")
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
    notices = output / "notices"
    notices.mkdir()
    for name in ("LICENSE", "THIRD_PARTY.md"):
        shutil.copyfile(ctx.sources["product"] / name, notices / name)
    originals = output / "upstream"
    originals.mkdir()
    hashes = {}
    for name in [*config["sources"], *config["headers"]]:
        path = ctx.sources["product"] / name
        shutil.copyfile(path, originals / path.name)
        hashes[name] = hashlib.sha256(path.read_bytes()).hexdigest()
    provenance = {"source": json_file(ctx.root / "config/source.json"), "gallery": config,
                  "source_identity": dict(mode="root-relative-v1", root="staged upstream tree", separators="/"),
                  "adaptations": [change for change in ctx.receipt.get("adaptations", [])
                                  if change["file"] in config["sources"]],
                  "sha256": hashes,
                  "translation_sha256": {name: hashlib.sha256((source / name).read_bytes()).hexdigest()
                                         for name in config["sources"]}}
    (output / "upstream/provenance.json").write_text(json.dumps(provenance, indent=2) + "\n")
    ctx.receipt["gallery"] = provenance
    return {config["product"]: output}
