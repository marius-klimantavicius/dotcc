"""Pinned portable Zstandard core, ordinary managed delivery and native controls."""
import shutil
import sys
import xml.etree.ElementTree as ET

from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin["directory"], pin["directory"],
                 pin["sha256"], tuple(pin["required"]))


def prepare(ctx):
    from production_support import production_sources
    ctx.receipt["production_inputs"] = production_sources(ctx.root)
    policy = json_file(ctx.root / "config/translation.json")
    upstream = ctx.sources["product"]
    observed = {path.relative_to(upstream).as_posix()
                for group in policy["source_groups"] for path in (upstream / group).glob("*.c")}
    if observed != set(policy["units"]):
        raise RuntimeError("Zstandard source manifest differs from selected core: "
                           f"unlisted={sorted(observed - set(policy['units']))}, "
                           f"missing={sorted(set(policy['units']) - observed)}")
    source = ctx.work / "source"
    shutil.copytree(upstream / "lib", source / "lib")
    for name in ("LICENSE", "COPYING"):
        shutil.copyfile(upstream / name, source / name)
    bridge = ctx.root / policy["bridge"]
    primitives = helper(ctx.root / "scripts/primitives.py")
    inventory = primitives.inventory(ctx.root)["expected_units"]
    if set(inventory) != {*policy["units"], policy["bridge"]}:
        raise RuntimeError("Primitive inventory must cover exactly the selected source units")
    units = [Unit(source / name, primitives.unit_flags(ctx, source / name, source, inventory[name]))
             for name in policy["units"]]
    # A diagnostic object probe can run while the authored embedding bridge is
    # being developed. A deliverable always requires the complete boundary.
    if bridge.is_file():
        units.append(Unit(bridge, primitives.unit_flags(ctx, bridge, source, inventory[policy["bridge"]])))
    elif ctx.options.action != "probe":
        raise RuntimeError(f"Missing authored Zstandard boundary: {bridge}")
    ctx.receipt["zstd_selection"] = {
        "metadata": str(ctx.root / "config/source.json"),
        "configuration": str(ctx.root / "config/translation.json"),
        "source": str(upstream), "staged_source": str(source),
        "upstream_units": policy["units"], "bridge": str(bridge),
        "adaptations": [], "multithread": False, "legacy_frames": False,
    }
    return Translation(units, ["-std=" + policy["dialect"],
                       *flags(policy["defines"], [source / "lib", source / "lib/common",
                                                  ctx.root / "src/Native"]),
                       *("-U" + name for name in policy["undefines"])],
                       [*link_flags("Zstd", "Managed.Zstd.Native"), "--deduplicate-static",
                        *(flag for pattern in policy["export_enums"] for flag in ("--export-enum", pattern))], objects=True,
                       configure=configure, validate_objects=primitives.validate_objects)


def configure(ctx, project, form):
    """Retain license texts with each delivered form without authored copies."""
    tree = ET.parse(project)
    group = None
    upstream = ctx.sources["product"]
    notices = []
    for relative in ("lib/common/xxhash.c", "lib/common/xxhash.h", "lib/dictBuilder/divsufsort.c"):
        text = (upstream / relative).read_text()
        if not text.startswith("/*") or "*/" not in text:
            raise RuntimeError(f"Missing upstream source notice: {relative}")
        notices.append(relative + "\n" + text[:text.index("*/") + 2])
    (project.parent / "THIRD-PARTY-NOTICES").write_text("\n\n".join(notices) + "\n")
    for name in ("LICENSE", "COPYING", "THIRD-PARTY-NOTICES"):
        if name != "THIRD-PARTY-NOTICES":
            shutil.copyfile(upstream / name, project.parent / name)
        node = next((item for item in tree.iter("None") if item.get("Update") == name), None)
        if node is None:
            if group is None:
                group = ET.SubElement(tree.getroot(), "ItemGroup")
            node = ET.SubElement(group, "None", Update=name)
        node.set("TargetPath", "Zstd." + name)
        node.set("CopyToOutputDirectory", "PreserveNewest")
        node.set("CopyToPublishDirectory", "PreserveNewest")
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")
    helper(ctx.root / "scripts/primitives.py").configure(ctx, project)


def run_native(ctx, suite):
    from campaigns.inputs import acquire
    source = ctx.sources.get("product") or acquire(next(sources(ctx.root)), ctx.policy, ctx.options.fetch)
    return ctx.run([sys.executable, ctx.root / "tests/NativeOracle/run.py",
                    "--source", source, "--output", ctx.artifacts / "native"],
                   "native", timeout=suite.timeout)


def run_managed(ctx, suite):
    return helper(ctx.root / "tests/managed_qualification.py").run(ctx, suite)


def run_consumer(ctx, suite):
    return helper(ctx.root / "tests/managed_qualification.py").run_consumer(ctx, suite)


def run_audit(ctx, suite):
    return helper(ctx.root / "tests/audit.py").run(ctx, suite)


def run_primitives(ctx, suite):
    return helper(ctx.root / "tests/Primitives/run.py").run(ctx, suite)


recipe = Recipe(
    "zstd", "TranslatedZstd", ("portable", "original"),
    ("src/Managed.Zstd/Managed.Zstd.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
    sources, prepare,
    consumer=Consumer(property="ZstdProject", arguments=("--self-test", "{work}/consumer-{label}")),
    suites=(Suite("native", (), run=run_native,
                  scope="exact-source portable native ABI, compression, streaming, dictionary and error controls"),
            Suite("primitives", (), run=run_primitives, timeout=7200,
                  scope="original native and original/replaced translated byte and bit helper parity, including guard pages"),
            Suite("consumer", (), run=run_consumer, matrix=True,
                  scope="owning API multi-file streaming consumer"),
            *(Suite(name, (), run=run_managed, matrix=True)
              for name in ("abi", "behavior", "lifecycle", "interop", "upstream")),
            Suite("audit", (), run=run_audit,
                  scope="selected managed artifact closure and native-backend exclusion")),
    default_suites=("consumer",),
    verify_suites=("native", "primitives", "abi", "consumer", "behavior", "lifecycle", "interop", "upstream", "audit"),
)
