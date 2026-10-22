"""Common direct/object emission, postprocessing, build, and publication."""
from concurrent.futures import ThreadPoolExecutor, as_completed
import json
import os
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from .delivery import promote, source_files, validate_emission
from .identity import write_json
from .layout import Layout


def relocate_project(project, origin):
    """Keep relative authored links correct when a generated tree moves."""
    tree = ET.parse(project)
    changed = False
    for node in tree.iter():
        for attribute in ("Include", "Project"):
            value = node.get(attribute)
            if value and value.startswith("..") and "$" not in value and "*" not in value:
                target = (origin / value).resolve()
                node.set(attribute, os.path.relpath(target, project.parent))
                changed = True
    if changed:
        ET.indent(tree, space="  ")
        tree.write(project, encoding="unicode")


def object_report_flags(flags, report):
    """Replace a recipe's direct-emission report with the per-object report."""
    result = []
    arguments = iter(flags)
    for argument in arguments:
        if argument == "--override-report":
            if next(arguments, None) is None:
                raise ValueError("--override-report requires a path")
        elif not str(argument).startswith("--override-report="):
            result.append(argument)
    return [*result, "--override-report", report]


def translate(ctx):
    spec = ctx.recipe.prepare(ctx)
    if not spec.units or len({str(unit.path) for unit in spec.units}) != len(spec.units):
        raise RuntimeError("Empty or duplicate translation units")
    for unit in spec.units:
        if not unit.path.is_file():
            raise RuntimeError(f"Missing translation unit: {unit.path}")
    if ctx.options.unit:
        chosen = set(ctx.options.unit)
        units = [unit for unit in spec.units if str(unit.path) in chosen or unit.path.name in chosen
                 or any(str(unit.path).endswith('/' + name) for name in chosen)]
        if not units or any(not any(str(unit.path).endswith(name) for unit in units) for name in chosen):
            raise RuntimeError("Unknown probe unit")
    else:
        units = spec.units
    ctx.receipt["units"] = [str(unit.path) for unit in units]
    ctx.receipt["input_hashes"] = ctx.policy.manifest([unit.path for unit in units], ctx.root)
    objects = ctx.work / "objects"
    objects.mkdir()
    reports = []

    def emit(row):
        index, unit = row
        name = f"{index:03d}-{unit.path.stem}"
        output = objects / (name + ".cs")
        report = ctx.artifacts / (name + "-overrides.jsonl")
        ctx.run(["dotnet", ctx.compiler, *object_report_flags([*spec.flags, *unit.flags], report),
                 unit.path, "--emit=obj", "-o", output], name,
                timeout=ctx.options.timeout)
        if not output.is_file():
            raise RuntimeError(f"Compiler produced no object: {unit.path}")
        return output, report

    if spec.objects or ctx.options.action == "probe":
        workers = ThreadPoolExecutor(max_workers=ctx.options.jobs)
        try:
            pending = {workers.submit(emit, row): row[0] for row in enumerate(units)}
            emitted = [None] * len(units)
            # Observe any failure immediately, even if an earlier ordered unit
            # is still running. Keep the successful link's original unit order.
            for future in as_completed(pending):
                emitted[pending[future]] = future.result()
        except BaseException:
            ctx.cancellation.set()
            raise
        finally:
            workers.shutdown(wait=True, cancel_futures=True)
        inputs, reports = map(list, zip(*emitted))
        if spec.validate_objects:
            spec.validate_objects(ctx, reports)
        if ctx.options.action == "probe":
            ctx.receipt["scope"] = "object emission only; no product published"
            return
        flags = []
    else:
        inputs, flags = [unit.path for unit in units], spec.flags
    raw = ctx.work / "raw" / ctx.recipe.product
    ctx.run(["dotnet", ctx.compiler, *flags, *inputs, "--emit=managedlib",
             *spec.link_flags, "-o", raw], "link", timeout=max(600, ctx.options.timeout))
    validate_emission(raw, ctx.recipe.product)
    companions = spec.companions(ctx) if spec.companions else {}
    deliver(ctx, raw, spec.configure, spec.finish, companions)


def deliver(ctx, raw, configure=None, finish=None, companions=None):
    if companions:
        return deliver_companions(ctx, raw, configure, finish, companions)
    product = ctx.work / "processed" / ctx.recipe.product
    project_name = ctx.recipe.product + ".csproj"
    if configure:
        configure(ctx, raw / project_name, "raw")
    ctx.managed("build", raw / project_name, "raw-build")
    forms = ["raw"] if ctx.options.form == "raw" else ["raw", "processed"]
    trees = {"raw": raw}
    if "processed" in forms:
        shutil.copytree(raw, product, ignore=shutil.ignore_patterns("bin", "obj"))
        relocate_project(product / project_name, raw)
        if configure:
            configure(ctx, product / project_name, "processed")
        ctx.restore(product / project_name, "postprocess-restore")
        # Referenced private host projects need evaluated metadata at this path.
        ctx.managed("build", product / project_name, "semantic-context-build")
        ctx.run(["dotnet", ctx.postprocessor, product / project_name, "--in-place"], "postprocess", 900)
        if finish:
            finish(ctx, product / project_name, "processed")
        ctx.managed("build", product / project_name, "processed-build")
        trees["processed"] = product
    if finish:
        finish(ctx, raw / project_name, "raw")
    pairs = []
    for form, directory in trees.items():
        for path in sorted(directory.rglob("*"), key=lambda p: len(p.parts), reverse=True):
            if path.is_dir() and path.name in ("bin", "obj"):
                shutil.rmtree(path)
        names = [str(p.relative_to(directory)) for p in source_files(directory)]
        write_json(directory / ".campaign-files.json", names)
        pairs.append((directory, ctx.layout.directory(ctx.profile, form)))

    def validate():
        for form, origin in trees.items():
            project = ctx.layout.project(ctx.profile, form)
            relocate_project(project, origin)
            if finish:
                finish(ctx, project, form)
            ctx.managed("build", project, "final-" + form + "-build")

    def publication():
        for form in forms:
            directory = ctx.layout.directory(ctx.profile, form)
            ctx.receipt["outputs"][form] = dict(project=str(ctx.layout.project(ctx.profile, form)),
                                                hashes=ctx.policy.manifest(source_files(directory), directory))
        ctx.receipt["scope"] = "translation and project builds; behavioral tests separate"
        ctx.save()
        return {"receipt": str(ctx.artifacts / "receipt.json"), "outputs": ctx.receipt["outputs"]}

    promote(pairs, ctx.root / "build/campaign/promotion.json", validate,
            {ctx.root / "artifacts/campaign" / ("current-" + ctx.profile + ".json"): publication},
            legacy_owned=ctx.recipe.legacy_owned)


def deliver_companions(ctx, raw, configure, finish, companions):
    """Build every form before one atomic promotion of related libraries.

    Recipes prepare the independently translated inputs; this framework owns
    processing, project relocation, builds, rollback and publication for all.
    Main-product hooks apply only to the main product's authored sources.
    """
    if ctx.recipe.product in companions:
        raise ValueError("A companion cannot replace the main product")
    products = {ctx.recipe.product: raw, **companions}
    forms = ["raw"] if ctx.options.form == "raw" else ["raw", "processed"]
    trees, layouts = {}, {}
    origins = set()
    # Reject the entire set before configure hooks or a build can mutate any
    # member. A malformed later companion must leave the main input untouched.
    for name, directory in products.items():
        if not name or any(c not in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-" for c in name):
            raise ValueError("Invalid companion product: " + name)
        origin = Path(directory).resolve()
        if origin in origins:
            raise ValueError("Related products must have distinct source directories")
        origins.add(origin)
        validate_emission(Path(directory), name)
    for name, directory in products.items():
        layout = Layout(ctx.root, name, ctx.recipe.default_profile)
        layouts[name] = layout
        directory = Path(directory)
        project = directory / (name + ".csproj")
        main = name == ctx.recipe.product
        label = "" if main else name + "-"
        if main and configure:
            configure(ctx, project, "raw")
        ctx.managed("build", project, label + "raw-build")
        trees[(name, "raw")] = directory
        if "processed" in forms:
            processed = ctx.work / "processed" / name
            shutil.copytree(directory, processed, ignore=shutil.ignore_patterns("bin", "obj"))
            processed_project = processed / project.name
            relocate_project(processed_project, directory)
            if main and configure:
                configure(ctx, processed_project, "processed")
            ctx.restore(processed_project, label + "postprocess-restore")
            ctx.managed("build", processed_project, label + "semantic-context-build")
            ctx.run(["dotnet", ctx.postprocessor, processed_project, "--in-place"], label + "postprocess", 900)
            if main and finish:
                finish(ctx, processed_project, "processed")
            ctx.managed("build", processed_project, label + "processed-build")
            trees[(name, "processed")] = processed
        if main and finish:
            finish(ctx, project, "raw")
    pairs = []
    for (name, form), directory in trees.items():
        for path in sorted(directory.rglob("*"), key=lambda p: len(p.parts), reverse=True):
            if path.is_dir() and path.name in ("bin", "obj"):
                shutil.rmtree(path)
        write_json(directory / ".campaign-files.json", [str(p.relative_to(directory)) for p in source_files(directory)])
        pairs.append((directory, layouts[name].directory(ctx.profile, form)))

    def validate():
        for (name, form), origin in trees.items():
            project = layouts[name].project(ctx.profile, form)
            relocate_project(project, origin)
            if name == ctx.recipe.product and finish:
                finish(ctx, project, form)
            ctx.managed("build", project, "final-" + name + "-" + form + "-build")

    def publication():
        for form in forms:
            def record(name):
                directory = layouts[name].directory(ctx.profile, form)
                return dict(project=str(layouts[name].project(ctx.profile, form)),
                            hashes=ctx.policy.manifest(source_files(directory), directory))
            ctx.receipt["outputs"][form] = {**record(ctx.recipe.product),
                "companions": {name: record(name) for name in companions}}
        ctx.receipt["scope"] = "related translated libraries and project builds; behavioral tests separate"
        ctx.save()
        return {"receipt": str(ctx.artifacts / "receipt.json"), "outputs": ctx.receipt["outputs"]}

    promote(pairs, ctx.root / "build/campaign/promotion.json", validate,
            {ctx.root / "artifacts/campaign" / ("current-" + ctx.profile + ".json"): publication},
            legacy_owned=ctx.recipe.legacy_owned)


def check_product(ctx, form):
    project = ctx.layout.project(ctx.profile, form)
    if not project.is_file():
        raise RuntimeError(f"Missing library {project}; run translate first")
    ctx.receipt.setdefault("tested_artifacts", {})[form] = str(project)
    if ctx.policy.mode == "off":
        return project
    current = ctx.root / "artifacts/campaign" / ("current-" + ctx.profile + ".json")
    if not current.exists():
        ctx.policy.issue(f"No framework translation receipt for {project}; using existing output")
        return project
    try:
        record = json.loads(current.read_text()).get("outputs", {}).get(form, {})
    except (ValueError, AttributeError) as error:
        ctx.policy.issue(f"Unreadable optional provenance {current}: {error}")
        return project
    ctx.receipt["translation_receipt"] = str(current)
    hashes = record.get("hashes", {})
    if not hashes:
        ctx.policy.issue(f"No recorded output hashes for {project}")
    for name, expected in hashes.items():
        path = project.parent / name
        if not path.resolve().is_relative_to(project.parent.resolve()):
            raise RuntimeError(f"Unsafe receipt path: {name}")
        if path.is_file():
            ctx.policy.check(path, expected)
        else:
            ctx.policy.issue(f"Previously recorded output missing: {path}")
    for name, companion in record.get("companions", {}).items():
        if not name or any(c not in "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-" for c in name):
            raise RuntimeError("Invalid companion receipt product")
        companion_project = Layout(ctx.root, name, ctx.recipe.default_profile).project(ctx.profile, form)
        if companion.get("project") != str(companion_project):
            raise RuntimeError("Companion receipt project differs from selected layout")
        if not companion_project.is_file():
            raise RuntimeError(f"Missing related library {companion_project}; run translate first")
        companion_hashes = companion.get("hashes", {})
        if not companion_hashes:
            ctx.policy.issue(f"No recorded output hashes for related library {companion_project}")
        for relative, expected in companion_hashes.items():
            path = companion_project.parent / relative
            if not path.resolve().is_relative_to(companion_project.parent.resolve()):
                raise RuntimeError(f"Unsafe companion receipt path: {relative}")
            if path.is_file():
                ctx.policy.check(path, expected)
            else:
                ctx.policy.issue(f"Previously recorded companion output missing: {path}")
    return project
