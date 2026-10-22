"""Initial real-source attempts, with a hard guard against subset publication."""
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from campaigns.model import Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def record_object_scope(ctx, reports):
    ctx.receipt["pglite_translation"]["object_override_reports"] = [str(path) for path in reports]


def reject_incomplete(ctx):
    policy = json_file(ctx.root / "config/translation.json")
    if ctx.options.action != "probe" and not policy["complete_product_manifest"]:
        raise RuntimeError(policy["publication_blocker"])


def configure(ctx, project, form):
    if ctx.profile != "embedding-native":
        return
    # Give each form private authored sources during semantic processing.
    # finish() restores ordinary references to original authored files.
    authored = ctx.work / "authored-host" / form
    authored.mkdir(parents=True, exist_ok=True)
    tree = ET.parse(project)
    group = ET.SubElement(tree.getroot(), "ItemGroup")
    sources = [(path, "Host/" + path.name) for path in sorted((ctx.root / "src/Host").glob("*.cs"))]
    sources.extend((ctx.root / "src/Initialization" / name, "Initialization/" + name)
                   for name in ("InitializationBackend.cs", "InitializationBackend.Signals.cs",
                                "InitializationArguments.cs", "InitializationProcessTransport.cs"))
    for path, link in sources:
        for node in list(tree.getroot().iter("Compile")):
            if node.get("Link") == link:
                for parent in tree.getroot().iter():
                    if node in list(parent):
                        parent.remove(node)
                        break
        copy = authored / link
        copy.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(path, copy)
        ET.SubElement(group, "Compile", Include=str(copy), Link=link)
    ET.indent(tree, space="  ")
    tree.write(project, encoding="unicode")


def finish(ctx, project, form):
    # Permit private linking/build diagnostics against the actual full backend.
    # The framework calls finish before promotion, so incomplete products still
    # cannot replace either canonical product or its publication receipt.
    reject_incomplete(ctx)
    tree = ET.parse(project)
    for node in tree.getroot().iter("Compile"):
        link = node.get("Link", "")
        if link.startswith(("Host/", "Initialization/")):
            node.set("Include", str(ctx.root / "src" / link))
    tree.write(project, encoding="unicode")


def companions(ctx):
    initializer = helper(ctx.root / "scripts/initializer.py")
    report = initializer.run(ctx)
    source = Path(report["frontend_project"]).parent
    destination = ctx.work / "raw/TranslatedInitdb"
    shutil.copytree(source, destination, ignore=shutil.ignore_patterns("bin", "obj"))
    (destination / "frontend.csproj").rename(destination / "TranslatedInitdb.csproj")
    # Original catalogs/configuration/timezone data are delivered with each
    # backend form; initialized database contents are never a generated input.
    shutil.copytree(report["staged_bootstrap_prefix"],
                    ctx.work / "raw" / ctx.recipe.product / "bootstrap-prefix")
    ctx.receipt["pglite_initializer"] = report
    return {"TranslatedInitdb": destination}


def prepare_bootstrap_inputs(ctx, info):
    """Materialize upstream generated module sources and installed data.

    A configured backend alone lacks PL/pgSQL's parser and initdb's data
    prefix. Native preparation supplies build inputs/control evidence only;
    every product C unit is still translated and native libraries are excluded
    from the delivered bootstrap prefix.
    """
    initializer = helper(ctx.root / "scripts/initializer.py")
    selection = initializer.policy(ctx.root)

    def missing_inputs(current):
        source, prefix = Path(current["source"]), Path(current["prefix"])
        inputs = [source / name for name in selection["extra_generated_inputs"]]
        for module in selection["bootstrap_modules"].values():
            inputs.extend(source / name for name in module["sources"])
            inputs.extend(source / name for name in module.get("generated_headers", ()))
        inputs.extend(prefix / asset["path"] for asset in selection["bootstrap_assets"])
        return [str(path) for path in inputs if not path.exists()]

    missing = missing_inputs(info)
    if missing:
        info = helper(ctx.root / "scripts/prepare.py").build_native(ctx, info)
    remaining = missing_inputs(info)
    if remaining:
        raise RuntimeError("Upstream bootstrap preparation is incomplete: " + repr(remaining))
    ctx.receipt["pglite_bootstrap_preparation"] = dict(
        missing_before=missing, native_preparation_needed=bool(missing),
        product_uses_native_database=False)
    return info


def prepare(ctx):
    policy = json_file(ctx.root / "config/translation.json")
    info = helper(ctx.root / "scripts/prepare.py").prepare(ctx)
    if ctx.profile == "embedding-native":
        info = prepare_bootstrap_inputs(ctx, info)
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
        ctx.receipt["pglite_portable_codegen"] = helper(
            ctx.root / "scripts/portability.py").prepare_overlay(ctx.root, port["include"])
        ctx.receipt["pglite_signals_overlay"] = helper(
            ctx.root / "scripts/boundary.py").prepare_signal_overlay(Path(port["include"]), ctx.work)
    backend_defines = info.get("defines", []) if ctx.profile == "embedding-native" else []
    ctx.receipt["pglite_translation"] = dict(
        profile=ctx.profile, configuration=info,
        diagnostic_units=policy["diagnostic_units"], complete_product_manifest=policy["complete_product_manifest"],
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
    libc_wrapper = helper(ctx.root / "scripts/libc-input.py").prepare_input(ctx, source)
    if ctx.profile == "embedding-native" and (ctx.options.action != "probe" or ctx.options.unit):
        manifest = json_file(ctx.root / policy["backend_manifest"])
        pin = json_file(ctx.root / "config/sources.json")["product"]
        if manifest["revision"] != pin["revision"] or manifest["profile"] != ctx.profile:
            raise RuntimeError("Backend source selection needs an explicit source/profile upgrade")
        units = [Unit(libc_wrapper)]
        parser_inputs = helper(ctx.root / "scripts/portability.py").prepare_parser_units(
            ctx.root, source, ctx.work / "parser-inputs")
        ctx.receipt["pglite_parser_semantic_types"] = parser_inputs
        initializer = helper(ctx.root / "scripts/initializer.py")
        modules = initializer.backend_module_configuration(ctx.root)
        excluded = ctx.receipt["pglite_portable_codegen"]["excluded_native_codegen_units"]
        for context in manifest["contexts"]:
            for name in context["units"]:
                if name in excluded:
                    continue
                path = source / name
                unit_flags = [value.replace("{native_base}", str(source.parent))
                              for value in context["defines"]]
                unit_flags.extend(flags(includes=[path.parent if item == "{cwd}" else source / item
                                                  for item in context["includes"]]))
                unit_flags.extend(flags(ctx.receipt["pglite_portable_codegen"]["unit_defines"].get(name, ())))
                if name in parser_inputs["units"]:
                    parser = parser_inputs["units"][name]
                    unit_flags.extend(flags(parser["defines"]))
                    path = Path(parser["path"])
                if name == modules["dfmgr_source"]:
                    unit_flags.extend(flags(modules["dfmgr_defines"], modules["includes"]))
                units.append(Unit(path, tuple(unit_flags)))
        for name in ctx.receipt["pglite_portable_codegen"]["portable_sources"]:
            if not any(unit.path == source / name for unit in units):
                units.append(Unit(source / name, tuple(flags(backend_defines))))
        units.extend(initializer.backend_module_units(ctx, info, port["include"]))
        bridge = ctx.root / "src/Host/dotcc_pglite_boundary.c"
        units.append(Unit(bridge))
        ctx.receipt["pglite_translation"].update(
            backend_manifest=policy["backend_manifest"], backend_unit_count=manifest["backend_unit_count"],
            selected_unit_count=len(units),
            scope="configured original backend, portable sources and required PL/pgSQL/static provider plus libc/adapter; initializer companion is independently built")
    else:
        units = [Unit(libc_wrapper), *(Unit(source / name, tuple(flags(backend_defines)))
                 for name in policy["diagnostic_units"][1:])]
    if ctx.options.unit:
        # Keep the original upstream basename selectable for the staged wrapper.
        ctx.options.unit = ["pglitec-input.c" if item in
                            ("pglitec.c", policy["diagnostic_units"][0]) else item
                            for item in ctx.options.unit]
    options = ["-std=" + policy["dialect"], *flags(defines, includes)]
    linkage = link_flags(policy["class"], policy["namespace"])
    if ctx.profile == "embedding-native":
        options.extend(["--overrides-file", str(ctx.root / "src/Host/signal-overrides.json")])
        linkage.append("--state-context")
    return Translation(units, options, linkage, objects=True,
                       validate_objects=record_object_scope, configure=configure, finish=finish,
                       companions=companions if ctx.profile == "embedding-native" else None)
