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
    if ctx.profile == "upstream-render":
        tests = helper(ctx.root / "tests/upstream/run.py").prepare_tests(ctx)
        ctx.receipt["upstream_test_closure"] = {
            "source": str(tests["source"]), "argv": tests["argv"],
            "case_count": tests["case_count"], "host": str(tests["host"]),
        }
        return Translation([
            Unit(tests["source"], ("--overrides-file", str(ctx.root / "config/zig-c-overrides.json"))),
            Unit(tests["host"], ("-DDOTCC_UPSTREAM_HOST_NO_MAIN=1",))],
            ["-std=" + policy["dialect"], *flags(includes=tests["includes"])],
            [*link_flags("GhosttyVt", "Managed.GhosttyVt.Native"), "--prune-unused-inline"],
            objects=True, configure=configure)
    engine = helper(ctx.root / "tests/native/run.py").prepare_engine(ctx)
    contents = engine["source"].read_text(encoding="utf-8")
    forbidden_aliases = ("zig_mangled", "zig_e_zig_windows_teb", "zig_e_zig_windows_peb")
    if any(name in contents for name in forbidden_aliases):
        raise RuntimeError("Generated engine now requires a suppressed support-header symbol alias")
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
             Unit(ctx.root / policy["bridge"], ("-DDOTCC_GHOSTTY_INDIRECT_ABI=1",))]
    if ctx.profile == "qualification":
        staged = ctx.work / "qualification-sources"
        staged.mkdir()
        for filename, entry in (("oracle.c", "dotcc_oracle_main"),
                                ("boundary_control.c", "dotcc_boundary_main"),
                                ("abi_layout.c", "dotcc_abi_main")):
            text = (ctx.root / "tests/native" / filename).read_text(encoding="utf-8")
            anchor = "int main(void)"
            if text.count(anchor) != 1:
                raise RuntimeError("Ambiguous qualification entrypoint: " + filename)
            target = staged / filename
            target.write_text(text.replace(anchor, "int " + entry + "(void)"), encoding="utf-8")
            units.append(Unit(target, ("-DDOTCC_GHOSTTY_INDIRECT_ABI=1",)))
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


def run_qualification(ctx, suite):
    return helper(ctx.root / "tests/qualification.py").run(ctx, suite)


def run_upstream(ctx, suite):
    return helper(ctx.root / "tests/qualification.py").run_upstream(ctx, suite)


def run_upstream_native(ctx, suite):
    return helper(ctx.root / "tests/upstream/run.py").run(ctx, suite)


def run_audit(ctx, suite):
    return helper(ctx.root / "tests/audit.py").run(ctx, suite)


def run_host_io(ctx, suite):
    return helper(ctx.root / "tests/upstream/host_io_probe.py").run(ctx, suite)


recipe = Recipe(
    "libghostty", "TranslatedGhosttyVt", ("scalar", "qualification", "upstream-render"),
    ("src/Managed.GhosttyVt/Managed.GhosttyVt.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
    sources, prepare,
    consumer=Consumer(property="TranslatedProject"),
    suites=(Suite("host-io", (), run=run_host_io,
                  scope="native/emitted-JIT read-only corpus directory, positional binary I/O, errors and cleanup"),
            Suite("native", (), run=run_native, timeout=3600,
                  scope="pinned Zig native and genuine whole-engine C, 16 exact assertion cases"),
            Suite("consumer", ("consumer",), matrix=True,
                  scope="owning translated terminal transcript, split UTF-8, screens, resize/reset, independent instances"),
            Suite("qualification", (), run=run_qualification, matrix=True,
                  required_profiles=("qualification",),
                  scope="24 native differential, physical ABI layout and allocator boundary cases, rooted AOT"),
            Suite("upstream-native", (), run=run_upstream_native, timeout=3600,
                  scope="applicable unchanged upstream render assertions, original native vs genuine C, with allocator checks"),
            Suite("upstream", (), run=run_upstream, matrix=True,
                  required_profiles=("upstream-render",),
                  scope="applicable unchanged upstream render assertions, raw/processed JIT/rooted-AOT"),
            Suite("audit", (), run=run_audit, profiles=("scalar",),
                  scope="retained upstream export inventory, native-backend exclusion, notices and delivery boundaries")),
    default_suites=("consumer",),
    # Expanded qualification declarations are added with their actual harnesses;
    # this current selection does not establish P4/P6 acceptance by itself.
    verify_suites=("host-io", "native", "consumer", "qualification", "upstream-native", "upstream", "audit"),
)
