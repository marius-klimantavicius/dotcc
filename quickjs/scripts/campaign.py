"""QuickJS embedded engine source selection and executable campaign recipe."""
import sys
import json
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, lines, link_flags


def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin["directory"], pin["directory"], pin["sha256"],
                 tuple(pin["required"]))


def prepare(ctx):
    pin = json_file(ctx.root / "config/source.json")
    source = ctx.work / "source"
    ctx.receipt["adaptations"] = helper(ctx.root / "scripts/stage.py").stage(ctx.sources["product"], source)
    overrides = ctx.root / "config/dotcc-overrides.json"
    # Auxiliary units share cutils.h's branch hints, but do not define the
    # required engine dispatch or host functions. Keep their guards separate.
    hints = ctx.work / "branch-hint-overrides.json"
    hint_names = {"likely", "unlikely", "js_likely", "js_unlikely"}
    hints.write_text(json.dumps({"version": 1, "macroOverrides": [
        rule for rule in json_file(overrides)["macroOverrides"] if rule["name"] in hint_names
    ]}, indent=2) + "\n")
    units = [Unit(source / name, ("--overrides-file", str(overrides if name == "quickjs.c" else hints)))
             for name in lines(ctx.root / "config/core-sources.txt")]
    profile = json_file(ctx.root / "config/profile.json")
    definitions = [*profile["defines"], 'CONFIG_VERSION="' + pin["version"] + '"']
    return Translation(units, ["-std=" + profile["dotcc_standard"], *flags(definitions, [ctx.root / "config", source])],
                       [*link_flags("QuickJs", "Managed.Interpreters"),
                        *(flag for name in profile["inline_exports"] for flag in ("--export-inline", name))],
                       objects=True, configure=configure, validate_objects=validate_objects)


def validate_objects(ctx, reports):
    records = [json.loads(line) for path in reports for line in path.read_text().splitlines() if line]
    selected = [row for row in records if row.get("event") == "selected" and row.get("name") == "DIRECT_DISPATCH"]
    expanded = [row for row in records if row.get("event") == "summary" and row.get("name") == "DIRECT_DISPATCH"]
    if not selected or any(row.get("effective") != "0" for row in selected) or not any(int(row.get("expansions", 0)) > 0 for row in expanded):
        raise RuntimeError("DIRECT_DISPATCH override must select and expand the switch branch")
    hints = [row for row in records if row.get("event") == "selected"
             and row.get("name") in {"likely", "unlikely", "js_likely", "js_unlikely"}]
    if len([row for row in hints if row["name"] in {"likely", "unlikely"}]) != 2 * len(reports):
        raise RuntimeError("Every source unit must override both cutils branch hints")
    if any(row.get("effective") != "(!!(x))" for row in hints):
        raise RuntimeError("Branch hints must retain only their parenthesized first value")
    ctx.receipt["override_evidence"] = {"reports": [str(path) for path in reports],
                                        "branch_hints": hints,
                                        "dispatch": selected + expanded,
                                        "functions": [row for row in records if row.get("event") == "function-override"]}


def configure(ctx, project, form):
    ctx.receipt['upstream_notices'] = helper(ctx.root / 'scripts/notices.py').validate(ctx.sources['product'])
    for name in ("LICENSE", "VERSION"):
        shutil.copyfile(ctx.sources["product"] / name, project.parent / name)
    shutil.copyfile(ctx.root / 'THIRD-PARTY-NOTICES', project.parent / 'THIRD-PARTY-NOTICES')
    tree = ET.parse(project)
    notices = None
    for name in ('LICENSE', 'VERSION', 'THIRD-PARTY-NOTICES'):
        node = next((item for item in tree.iter('None') if item.get('Update') == name), None)
        if node is None:
            if notices is None:
                notices = ET.SubElement(tree.getroot(), 'ItemGroup')
            node = ET.SubElement(notices, 'None', Update=name)
        node.set('TargetPath', 'QuickJs.' + name)
        node.set('CopyToOutputDirectory', 'PreserveNewest')
        node.set('CopyToPublishDirectory', 'PreserveNewest')
    group = None
    for path in sorted((ctx.root / "src/Host").glob("*.cs")):
        if not any(node.get("Include") == str(path) for node in tree.iter("Compile")):
            if group is None:
                group = ET.SubElement(tree.getroot(), "ItemGroup")
            ET.SubElement(group, "Compile", Include=str(path), Link="Host/" + path.name)
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def run_native(ctx, suite):
    from campaigns.inputs import acquire
    pin = next(sources(ctx.root))
    source = ctx.sources.get("product") or acquire(pin, ctx.policy, ctx.options.fetch)
    return ctx.run([sys.executable, ctx.root / "tests/NativeOracle/run.py", "--source", source,
                    "--output", ctx.artifacts / "native"], "native", timeout=suite.timeout)


def run_managed(ctx, suite):
    return helper(ctx.root / "scripts/test.py").run(ctx, suite)


def run_consumer(ctx, suite):
    return helper(ctx.root / "scripts/test.py").run_consumer(ctx, suite)


def run_audit(ctx, suite):
    return helper(ctx.root / "scripts/audit.py").run(ctx, suite)



recipe = Recipe(
    "quickjs", "TranslatedQuickJs", ("embedded",),
    ("src/Managed.QuickJs/Managed.QuickJs.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
    sources, prepare, consumer=Consumer(property="QuickJsProject"),
    suites=(Suite("native", (), run=run_native, scope="native embedding, ABI, dispatch and failure controls"),
            Suite("consumer", (), run=run_consumer, matrix=True),
            Suite("abi", (), run=run_managed, matrix=True),
            Suite("behavior", (), run=run_managed, matrix=True),
            Suite("lifecycle", (), run=run_managed, matrix=True),
            Suite("upstream", (), run=run_managed, matrix=True),
            Suite("audit", (), run=run_audit)),
    default_suites=("consumer",),
    verify_suites=("native", "abi", "consumer", "behavior", "lifecycle", "upstream", "audit"),
)
