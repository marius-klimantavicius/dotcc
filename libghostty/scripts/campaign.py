"""Pinned libghostty-vt: genuine whole-engine Zig C emission into DotCC."""
import os
import platform
import shutil
import xml.etree.ElementTree as ET

from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin["directory"], pin["directory"],
                 pin.get("sha256"), tuple(pin["required"]))
    for item in pin.get("license_sources", []):
        yield Source(item["name"], item["url"], root / "ref" / item["archive"],
                     root / "ref" / item["directory"], item["prefix"],
                     item.get("sha256"), tuple(item["required"]),
                     archive_paths=tuple(item.get("archive_paths", [])))
    tool = json_file(root / "config/toolchain.json")
    arch = {"AMD64": "x86_64", "arm64": "aarch64"}.get(platform.machine(), platform.machine())
    system = {"Darwin": "macos", "Windows": "windows", "Linux": "linux"}.get(platform.system())
    name = f"{arch}-{system}"
    if name not in tool["hosts"]:
        raise RuntimeError(f"No pinned Zig host toolchain for {name}")
    pin = tool["hosts"][name]
    base = root / tool["directory"]
    yield Source("zig", pin["url"], base / pin["archive"], base / pin["directory"],
                 pin["directory"], pin.get("sha256"),
                 ("zig.exe" if os.name == "nt" else "zig", "lib/zig.h"))


def prepare(ctx):
    policy = json_file(ctx.root / "config/translation.json")
    engine = helper(ctx.root / "tests/native/run.py").prepare_engine(ctx)
    profile = ctx.root / "config/zig-c-overrides.json"
    ctx.receipt["libghostty_selection"] = {
        "policy": str(ctx.root / "config/translation.json"),
        "upstream_zig_root": policy["zig_root"],
        "generated_c": str(engine["source"]), "target": engine["target"],
        "engine_command": engine["argv"],
        "abi_adapter": policy["bridge"], "native_backend_at_runtime": False,
    }
    # Every engine algorithm is in the genuine Zig-produced C unit. The second
    # unit adapts only the ABI and supplies the public allocator extension point.
    units = [Unit(engine["source"], ("--overrides-file", str(profile))),
             Unit(ctx.root / policy["bridge"])]
    return Translation(units, ["-std=" + policy["dialect"], *flags(includes=engine["includes"])],
                       [*link_flags("GhosttyVt", "Managed.GhosttyVt.Native"), "--prune-unused-inline",
                        "--export-enum", "DOTCC_STYLE_*"],
                       objects=True, configure=configure)


def configure(ctx, project, form):
    pin = json_file(ctx.root / "config/source.json")
    notices = []
    for item in pin["license_inputs"]:
        if not item.get("distribute"):
            continue
        source = (ctx.sources["zig"] / item["toolchain_path"] if "toolchain_path" in item
                  else ctx.sources[item.get("source", "product")] / item["path"])
        notices.append(item["name"] + "\n" + source.read_text(encoding="utf-8"))
    name = "THIRD-PARTY-NOTICES"
    (project.parent / name).write_text("\n\n".join(notices) + "\n", encoding="utf-8")
    tree = ET.parse(project)
    node = next((item for item in tree.iter("None") if item.get("Update") == name), None)
    if node is None:
        group = ET.SubElement(tree.getroot(), "ItemGroup")
        node = ET.SubElement(group, "None", Update=name)
    node.set("TargetPath", "GhosttyVt." + name)
    node.set("CopyToOutputDirectory", "PreserveNewest")
    node.set("CopyToPublishDirectory", "PreserveNewest")
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def run_native(ctx, suite):
    return helper(ctx.root / "tests/native/run.py").run(ctx, suite)


recipe = Recipe(
    "libghostty", "TranslatedGhosttyVt", ("scalar",),
    ("src/Managed.GhosttyVt/Managed.GhosttyVt.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
    sources, prepare,
    consumer=Consumer(property="TranslatedProject"),
    suites=(Suite("native", (), run=run_native, timeout=3600,
                  scope="pinned Zig native and genuine whole-engine C, 16 exact assertion cases"),
            Suite("consumer", ("consumer",), matrix=True,
                  scope="owning translated terminal transcript, split UTF-8, screens, resize/reset, independent instances")),
    default_suites=("consumer",),
    # Expanded qualification declarations are added with their actual harnesses;
    # this current selection does not establish P4/P6 acceptance by itself.
    verify_suites=("native", "consumer"),
)
