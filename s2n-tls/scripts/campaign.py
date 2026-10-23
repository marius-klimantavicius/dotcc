"""s2n-tls source-to-managed campaign; policy lives in JSON metadata."""
from pathlib import Path
import os
import shutil
import xml.etree.ElementTree as ET

from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def sources(root):
    for name, pin in json_file(root / "config/inputs.json").items():
        yield Source(name, pin["url"], root / "ref" / pin["archive"],
                     root / "ref" / pin["directory"], pin.get("prefix", pin["directory"]),
                     pin.get("sha256"), tuple(pin["required"]), pin["group"],
                     archive_paths=tuple(pin.get("archive_paths", ())))


def prepare(ctx):
    policy = json_file(ctx.root / "config/translation.json")
    manifest = json_file(ctx.root / "config/source-manifest.json")
    reference = ctx.sources["s2n-tls"]
    stage = ctx.work / "source"
    shutil.copytree(reference, stage)
    for adaptation in json_file(ctx.root / "config/adaptations.json")["replacements"]:
        path = stage / adaptation["path"]
        text = path.read_text()
        if text.count(adaptation["old"]) != 1:
            raise RuntimeError("Adaptation requires unique context: " + adaptation["path"] + ": " + adaptation["reason"])
        path.write_text(text.replace(adaptation["old"], adaptation["new"]))
    units = manifest["units"]
    if len(units) != policy["required_upstream_units"] or len(set(units)) != len(units):
        raise RuntimeError("The complete unique upstream source manifest is required")
    for name in units:
        path = stage / name
        if Path(name).is_absolute() or ".." in Path(name).parts or not path.is_file():
            raise RuntimeError("Invalid or missing selected source: " + name)
        # CMake force-includes this header. Materialize that input in the staged
        # TU because DotCC's CLI has no force-include option; never edit ref/.
        path.write_text('#include "utils/s2n_prelude.h"\n' + path.read_text())
    headers = ctx.work / "include"
    for header in policy.get("staged_headers", ()):
        destination = headers / header["destination"]
        destination.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(ctx.root / header["source"], destination)
    include_paths = [stage, stage / "api", ctx.root / "src/Boundary", headers]
    provider = json_file(ctx.root / "config/provider.json")
    for name in policy["include_directories"]:
        include_paths.append(ctx.root / name)
    include_paths += [ctx.root / name for name in provider["include_directories"]]
    options = ["-std=" + policy["dialect"], *flags([*policy["defines"], *provider["compiler_definitions"]], include_paths)]
    for name in policy.get("emit_defines", ()):
        options += ["--emit-define", name]
    overrides = ctx.root / "config/dotcc-overrides.json"
    if overrides.exists():
        options += ["--overrides-file", str(overrides)]
    provider_overrides = ctx.root / provider["override_file"]
    if provider_overrides.exists():
        override_snapshot = ctx.work / "provider-overrides.json"
        shutil.copyfile(provider_overrides, override_snapshot)
        options += ["--overrides-file", str(override_snapshot)]
    ctx.receipt["s2n_configuration"] = policy
    ctx.receipt["source_manifest"] = manifest
    return Translation([*[Unit(stage / name) for name in units], Unit(ctx.root / "src/Boundary/dotcc_s2n_compat.c")], options,
                       [*link_flags("S2nTls", "Managed.Security.S2n"), "--prune-unused-inline"], objects=True, configure=configure)


def configure(ctx, project, form):
    provider = json_file(ctx.root / "config/provider.json")
    tree = ET.parse(project)
    group = ET.SubElement(tree.getroot(), "ItemGroup")
    reference = os.path.relpath(ctx.root / provider["project_reference"], project.parent)
    if not any(node.get("Include") == reference for node in tree.iter("ProjectReference")):
        ET.SubElement(group, "ProjectReference", Include=reference)
    for source in provider["authored_bindings"]:
        if not any(node.get("Link") == Path(source).name for node in tree.iter("Compile")):
            ET.SubElement(group, "Compile", Include=os.path.relpath(ctx.root / source, project.parent), Link=Path(source).name)
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def run_suite(ctx, suite):
    return helper(ctx.root / "scripts/qualification.py").run(ctx, suite)


SUITES = ("native", "abi", "provider", "compatibility", "consumer", "upstream",
          "lifecycle", "interop", "rooted-aot", "delivery", "reproduction", "shared-regressions")

recipe = Recipe(
    "s2n-tls", "TranslatedS2nTls", ("compat",),
    ("samples/ManagedConsumer/ManagedConsumer.csproj",), sources, prepare,
    consumer=Consumer(property="S2nTlsProject"),
    suites=tuple(Suite(name, (), run=run_suite, matrix=name != "native",
                       scope="s2n-tls " + name + " acceptance") for name in SUITES),
    default_suites=("consumer",), verify_suites=SUITES,
)
