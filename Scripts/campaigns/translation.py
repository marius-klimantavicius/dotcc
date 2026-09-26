"""Common direct/object emission, postprocessing, build, and publication."""
from concurrent.futures import ThreadPoolExecutor
import json
import os
from pathlib import Path
import shutil
import xml.etree.ElementTree as ET
from .delivery import promote, source_files, validate_emission
from .identity import write_json


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
        ctx.run(["dotnet", ctx.compiler, *spec.flags, *unit.flags,
                 "--override-report", report, unit.path, "--emit=obj", "-o", output], name,
                timeout=ctx.options.timeout)
        if not output.is_file():
            raise RuntimeError(f"Compiler produced no object: {unit.path}")
        return output, report

    if spec.objects or ctx.options.action == "probe":
        workers = ThreadPoolExecutor(max_workers=ctx.options.jobs)
        try:
            emitted = list(workers.map(emit, enumerate(units)))
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
    deliver(ctx, raw, spec.configure, spec.finish)


def deliver(ctx, raw, configure=None, finish=None):
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
    return project
