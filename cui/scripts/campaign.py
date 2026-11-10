"""One CUI translation with runtime Windows/X11 selection through managed GLFW."""
import json
import os
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, lines, link_flags


def run_suite(ctx, suite):
    runners = helper(ctx.root / "scripts/suites.py")
    return (runners.abi if suite.name == "abi" else runners.audit if suite.name == "audit"
            else runners.qualification)(ctx, suite)


def sources(root):
    pin = json_file(root / "config/source.json")
    inputs = Path(os.environ["CUI_WORK_ROOT"]) / "inputs" if "CUI_WORK_ROOT" in os.environ else root / "ref"
    directory = Path(os.environ["CUI_SOURCE_ROOT"]) if "CUI_SOURCE_ROOT" in os.environ else inputs / pin["directory"]
    yield Source("product", pin["url"], directory.parent / pin["archive"], directory,
                 pin["directory"], required=tuple(pin["required"]))


def prepare(ctx):
    source = ctx.work / "source"
    ctx.receipt["adaptations"] = helper(ctx.root / "scripts/stage.py").stage(ctx.sources["product"], source)
    common = json_file(ctx.root / "config/host-overrides.json")
    glfw = json_file(ctx.root / "config/glfw-overrides.json")
    merged = ctx.work / "glfw-overrides.json"
    merged.write_text(json.dumps({"version": 1, "functionOverrides": [*common["functionOverrides"], *glfw["functionOverrides"]]}))
    units = [Unit(source / name, ("--overrides-file", str(merged if name.endswith("cui_app_glfw.c") else ctx.root / "config/host-overrides.json")))
             for name in lines(ctx.root / "config/core-sources.txt")]
    return Translation(units,
                       ["-std=c99", "-U_WIN32", "-U__APPLE__", "-DGLFW_INCLUDE_NONE",
                        *flags(json_file(ctx.root / "config/profile.json")["defines"], [ctx.root / "config/include", source / "include"])],
                       [*link_flags("Cui", "Managed.Cui"), "--export-enum", "CUI_*"],
                       objects=True, configure=configure, finish=finish)


def configure(ctx, project, form):
    tree = ET.parse(project)
    root = tree.getroot()
    group = ET.SubElement(root, "ItemGroup")
    for path in sorted((ctx.root / "src/Host").glob("*.cs")):
        selected = path
        if form == "processed":
            selected = ctx.work / "postprocess-host" / path.name
            selected.parent.mkdir(exist_ok=True)
            shutil.copyfile(path, selected)
        existing = next((node for node in root.iter("Compile") if node.get("Link") == "Host/" + path.name), None)
        if existing is None:
            existing = ET.SubElement(group, "Compile", Link="Host/" + path.name)
        existing.set("Include", os.path.relpath(selected, project.parent))
    reference = ctx.root / "src/Glfw/Glfw.csproj"
    if not any(Path(node.get("Include", "")).name == "Glfw.csproj" for node in root.iter("ProjectReference")):
        ET.SubElement(group, "ProjectReference", Include=os.path.relpath(reference, project.parent))
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
    notices = project.parent / "notices"
    notices.mkdir(exist_ok=True)
    source = ctx.sources["product"]
    for name in ("LICENSE", "THIRD_PARTY.md"):
        shutil.copyfile(source / name, notices / name)
    for path in (source / "assets/fonts").glob("LICENSE*"):
        shutil.copyfile(path, notices / path.name)
    shutil.copyfile(ctx.root / "src/Glfw/LICENSE.md", notices / "GLFW-LICENSE.md")


def finish(ctx, project, form):
    # Semantic processing uses private host copies; delivered projects always
    # reference the authored files, so ordinary builds pick up host fixes.
    tree = ET.parse(project)
    for node in tree.getroot().iter("Compile"):
        link = node.get("Link", "")
        if link.startswith("Host/"):
            node.set("Include", os.path.relpath(ctx.root / "src/Host" / Path(link).name, project.parent))
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def validate_options(options):
    if options.action in ("list", "layout"):
        return
    configured = os.environ.get("CUI_WORK_ROOT")
    if not configured:
        raise ValueError("Set CUI_WORK_ROOT to a disposable directory on external storage before running CUI campaign commands")
    work = Path(configured).resolve()
    if work.is_relative_to(Path(__file__).resolve().parents[2]):
        raise ValueError("CUI_WORK_ROOT must be outside the repository")


recipe = Recipe(
    "cui", "TranslatedCui", ("desktop",),
    ("src/Glfw/Glfw.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
    sources, prepare,
    consumer=Consumer(property="CuiProject", arguments=("--smoke",), build_arguments=("-m:1",)),
    suites=(Suite("consumer", ("consumer",), matrix=True, scope="translated OpenGL widgets, readback and desktop lifecycle"),
            Suite("native", (), run=run_suite, scope="matching native GLFW selected deterministic upstream suites", timeout=3600),
            Suite("abi", (), run=run_suite, matrix=True, scope="GLFW callbacks, GL execution, platform ABI and guarded adaptations", timeout=3600),
            Suite("upstream", (), run=run_suite, matrix=True, scope="original upstream C suites, bidi and trusted Arabic shaping oracle", timeout=7200),
            Suite("render", (), run=run_suite, matrix=True, scope="five image formats and exact native/software pixels", timeout=7200),
            Suite("lifecycle", (), run=run_suite, matrix=True, scope="offscreen allocation failures and cleanup cycles", timeout=7200),
            Suite("audit", (), run=run_suite, matrix=True, scope="canonical source/reference and native dependency closure")),
    default_suites=("consumer",), verify_suites=("native", "consumer", "abi", "upstream", "render", "lifecycle", "audit"),
    validate_options=validate_options,
    work_root=Path(os.environ["CUI_WORK_ROOT"]) if "CUI_WORK_ROOT" in os.environ else None,
)
