"""libsmb2 async host binding and separate Libc profile."""
import xml.etree.ElementTree as ET
import os
import shutil
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, json_file, link_flags


def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["archive"], root / "ref" / (pin["directory"] + ".tar.gz"),
                 root / "ref" / pin["directory"], pin["directory"], pin["sha256"], ("include/smb2/libsmb2.h",))


def configure(ctx, project, form):
    tree = ET.parse(project)
    group = ET.SubElement(tree.getroot(), "PropertyGroup")
    if ctx.profile == "legacy":
        ET.SubElement(group, "Libsmb2LegacyTransport").text = "true"
    else:
        context = ctx.work / "authored-host"
        if not context.exists():
            shutil.copytree(ctx.root / "src", context, ignore=shutil.ignore_patterns("bin", "obj"))
        ET.SubElement(group, "Libsmb2HostSourceDirectory").text = str(context)
    tree.write(project, encoding="unicode")


def finish(ctx, project, form):
    tree = ET.parse(project)
    for group in tree.getroot():
        for node in list(group):
            if node.tag == "Libsmb2HostSourceDirectory":
                node.text = "$(MSBuildThisFileDirectory)" + os.path.relpath(ctx.root / "src", project.parent)
    tree.write(project, encoding="unicode")


def validate(ctx, reports):
    if ctx.profile != "async":
        return
    events = [row for report in reports for line in report.read_text().splitlines()
              if (row := __import__('json').loads(line)).get("event") == "function-override"]
    required = {row["name"] for row in json_file(ctx.root / "config/dotcc-overrides.json")["functionOverrides"]}
    missing = required - {row["name"] for row in events}
    if missing:
        raise RuntimeError("Async host bindings did not match: " + ", ".join(sorted(missing)))
    ctx.receipt["host_bindings"] = events


def prepare(ctx):
    source = ctx.sources["product"]
    definitions = json_file(ctx.root / "config" / ("defines.json" if ctx.profile == "async" else "legacy-defines.json"))
    options = ["-std=c17", *flags(definitions, [ctx.root / "config/managed", source / "include", source / "include/smb2", source / "lib"])]
    if ctx.profile == "async":
        if "HAVE_ARC4RANDOM_BUF" not in definitions:
            raise RuntimeError("Async descriptor audit requires HAVE_ARC4RANDOM_BUF")
        options += ["--overrides-file", str(ctx.root / "config/dotcc-overrides.json")]
        for name in json_file(ctx.root / "config/emit-defines.json"):
            options += ["--emit-define", name]
    return Translation([Unit(source / name) for name in json_file(ctx.root / "config/sources.json")], options,
                       link_flags("LibSmb2", "Managed.Smb"), objects=True, configure=configure,
                       validate_objects=validate, finish=finish)


recipe = Recipe("libsmb2", "TranslatedLibsmb2", ("async", "legacy"),
                ("src/Managed/ManagedSmb.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"), sources, prepare,
                suites=(Suite("consumer", ("consumer",), matrix=True, profiles=("async",)),
                        Suite("qualification", ("script", "test.py"), 14400, platforms=("linux",), profiles=("async",),
                              required_profiles=("legacy",), scope="fixed raw/processed JIT/AOT Linux x64 qualification; requires Docker"),
                        Suite("upstream", ("script", "upstream-tests.py"), 14400, platforms=("linux",), profiles=("legacy",))),
                consumer=Consumer(property="Libsmb2GeneratedProject", arguments=("--help",)),
                default_suites=("consumer",), verify_suites=("qualification",))
