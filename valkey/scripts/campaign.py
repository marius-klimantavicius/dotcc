"""Valkey generators, managed embedding adaptations, and notice delivery."""
import os
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.testing import forms
from campaigns.recipes import flags, helper, json_file, link_flags


def sources(root):
    pin = json_file(root / "config/source.json")
    files = json_file(root / "config" / pin["file_manifest"])["files"]
    yield Source("product", pin["url"], root / "ref" / pin["archive"], root / "ref" / pin["directory"],
                 pin["directory"], pin["archive_sha256"], ("src/server.c", "src/server.h"),
                 files={name: row["sha256"] for name, row in files.items()})


def host_links(ctx, project, sources):
    tree = ET.parse(project)
    for group in list(tree.getroot()):
        if group.get("Label") == "ValkeyHost":
            tree.getroot().remove(group)
    group = ET.SubElement(tree.getroot(), "ItemGroup", Label="ValkeyHost")
    for source in sources:
        ET.SubElement(group, "Compile", Include=os.path.relpath(source, project.parent), Link="Host/" + source.name)
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def prepare(ctx):
    pipeline = helper(ctx.root / "scripts/pipeline.py")
    pin = json_file(ctx.root / "config/source.json")
    ctx.receipt["inputs"] = {"commit": pin["commit"], "source_root": str(ctx.sources["product"])}

    def run(command, log, receipt, **kwargs):
        ctx.run(command, log.stem, cwd=kwargs.get("cwd"))

    source = pipeline.stage_source(ctx.sources["product"], ctx.work / "source", ctx.receipt,
                                   ctx.artifacts, managed_profile=not ctx.options.pristine,
                                   runner=run, policy=ctx.policy)
    records = json_file(ctx.root / "config/sources.json")["sources"]
    options = ["-std=c11", "--instance-methods"]
    if not ctx.options.pristine:
        records += json_file(ctx.root / "config/managed-adaptations.json")["extra_sources"]
        options += ["--overrides-file", str(ctx.root / "config/dotcc-overrides.json")]
    units = [Unit(source / row["path"], tuple(flags(row.get("defines", []),
             [source / name for name in row.get("include_dirs", [])]))) for row in records]
    authored = sorted((ctx.root / "src/Host").glob("*.cs"))
    context = ctx.work / "authored-host"
    context.mkdir()
    for path in authored:
        shutil.copy2(path, context / path.name)

    def configure(ctx, project, form):
        helper(ctx.root / "scripts/notices.py").write_notices(ctx.sources["product"], ctx.root / "config", project, ctx.policy)
        host_links(ctx, project, sorted(context.glob("*.cs")))

    def finish(ctx, project, form):
        host_links(ctx, project, authored)

    return Translation(units, options, [*link_flags("ValkeyCore", "Managed.Database", 262144),
                       "--instance-methods", "--deduplicate-inline"], objects=True,
                       configure=configure, finish=finish)


def run_consumer(ctx, suite):
    for form in forms(ctx):
        args = ["--variant", form, "--runtime", ctx.options.rid]
        if ctx.options.mode in ("aot", "all"):
            args.append("--aot")
        if ctx.options.action == "verify":
            args += ["--native-compare", "--persistence-exchange", "--upstream-protocol"]
        ctx.script("validate_managed.py", *args, label="managed-" + form, timeout=suite.timeout)
    return


def script_arguments(ctx, suite, script, args):
    if script == "oracle.py" and ctx.options.fetch == "never":
        args.append("--no-fetch")
    return args


recipe = Recipe("valkey", "TranslatedValkey", ("managed",),
                ("src/Managed.Valkey/Managed.Valkey.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
                sources, prepare,
                script_arguments=script_arguments, suites=(Suite("consumer", (), 14400, run=run_consumer, matrix=True, platforms=("linux",)),
                        Suite("oracle", ("script", "oracle.py"), 3600, platforms=("linux",)),
                        Suite("unit", ("python", "-m", "unittest", "discover", "-s", "{root}/tests"))),
                supports_pristine=True, consumer=Consumer(property="TranslatedValkeyProject"),
                default_suites=("consumer",), verify_suites=("unit", "oracle", "consumer"))
