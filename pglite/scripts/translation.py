"""Initial real-source attempts, with a hard guard against subset publication."""
from pathlib import Path
from campaigns.model import Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def reject_incomplete(ctx, reports):
    policy = json_file(ctx.root / "config/translation.json")
    if ctx.options.action != "probe" and not policy["complete_product_manifest"]:
        raise RuntimeError(policy["publication_blocker"])


def prepare(ctx):
    policy = json_file(ctx.root / "config/translation.json")
    info = helper(ctx.root / "scripts/prepare.py").prepare(ctx)
    source = Path(info["source"])
    build = Path(info["build"])
    includes = info.get("include_dirs", info.get("include", [build / "src/include", source / "src/include"]))
    defines = list(policy["diagnostic_defines"])
    if ctx.profile == "embedding-native":
        port = helper(ctx.root / "scripts/atomics.py").prepare_overlay(
            source, ctx.work / "atomics-include")
        includes = [port["include"], *includes]
        defines.extend(port["defines"])
        ctx.receipt["pglite_atomics_overlay"] = port
    backend_defines = info.get("defines", []) if ctx.profile == "embedding-native" else []
    ctx.receipt["pglite_translation"] = dict(
        profile=ctx.profile, configuration=info,
        diagnostic_units=policy["diagnostic_units"], complete_product_manifest=False,
        embedding_parity=False,
        scope="real fork source diagnostics using native generated headers; not wasm32 profile qualification")
    # Paths in specialist configuration are serialized in the ordinary receipt.
    def serializable(value):
        if isinstance(value, Path):
            return str(value)
        if isinstance(value, dict):
            return {key: serializable(item) for key, item in value.items()}
        if isinstance(value, (list, tuple)):
            return [serializable(item) for item in value]
        return value
    ctx.receipt["pglite_translation"]["configuration"] = serializable(info)
    # The upstream script compiles pglitec before adding interception remaps.
    # Its native compiler flags force stdbool/stdlib includes: use an ordinary
    # staged include wrapper, preserving the acquired translation unit.
    libc_wrapper = ctx.work / "pglitec-input.c"
    libc_wrapper.write_text('#include <stdbool.h>\n#include <stdlib.h>\n#include "'
                            + (source / policy["diagnostic_units"][0]).as_posix() + '"\n')
    if ctx.options.action != "probe" and ctx.profile == "embedding-native":
        manifest = json_file(ctx.root / policy["backend_manifest"])
        pin = json_file(ctx.root / "config/sources.json")["product"]
        if manifest["revision"] != pin["revision"] or manifest["profile"] != ctx.profile:
            raise RuntimeError("Backend source selection needs an explicit source/profile upgrade")
        units = [Unit(libc_wrapper)]
        for context in manifest["contexts"]:
            for name in context["units"]:
                path = source / name
                unit_flags = [value.replace("{native_base}", str(source.parent))
                              for value in context["defines"]]
                unit_flags.extend(flags(includes=[path.parent if item == "{cwd}" else source / item
                                                  for item in context["includes"]]))
                units.append(Unit(path, tuple(unit_flags)))
        bridge = ctx.root / "src/Host/dotcc_pglite_boundary.c"
        units.append(Unit(bridge))
        ctx.receipt["pglite_translation"].update(
            backend_manifest=policy["backend_manifest"], backend_unit_count=manifest["backend_unit_count"],
            selected_unit_count=len(units),
            scope="whole observed native backend link closure plus libc/adapter; initdb/product closure incomplete")
    else:
        units = [Unit(libc_wrapper), *(Unit(source / name, tuple(flags(backend_defines)))
                 for name in policy["diagnostic_units"][1:])]
    if ctx.options.unit:
        # Keep the original upstream basename selectable for the staged wrapper.
        ctx.options.unit = ["pglitec-input.c" if item in
                            ("pglitec.c", policy["diagnostic_units"][0]) else item
                            for item in ctx.options.unit]
    return Translation(units, ["-std=" + policy["dialect"], *flags(defines, includes)],
                       link_flags(policy["class"], policy["namespace"]), objects=True,
                       validate_objects=reject_incomplete)
