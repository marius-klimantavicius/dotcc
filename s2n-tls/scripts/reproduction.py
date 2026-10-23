"""Reproduction gates using shared tasks under the caller's campaign lock."""
import copy
from dataclasses import replace
import hashlib
import json
import os
from pathlib import Path
import tempfile
import xml.etree.ElementTree as ET

from campaigns.cli import task_plan
from campaigns.delivery import source_files
from campaigns.model import Unit
from campaigns.runner import Context


def fingerprint(ctx):
    paths = [path for form in ("raw", "processed")
             for path in source_files(ctx.layout.directory(ctx.profile, form))]
    paths.append(ctx.root / "artifacts/campaign" / ("current-" + ctx.profile + ".json"))
    return {str(path): hashlib.sha256(path.read_bytes()).hexdigest() for path in paths}


def run(ctx, suite):
    if any(not ctx.layout.project(ctx.profile, form).is_file() for form in ("raw", "processed")):
        raise RuntimeError("Reproduction requires both actual published product forms")
    # Exercise full publication in an isolated destination profile with exactly
    # compat's sources/options. Its name changes only the output destination.
    # A private identical authored host lets edit-pickup run while independently
    # qualified applications continue using their frozen production sources.
    profile = ctx.profile + "-reproduction-" + ctx.run_id
    private_host = ctx.work / "authored/S2nHost.cs"
    private_host.parent.mkdir(parents=True)
    original_host = ctx.root / "src/Boundary/S2nHost.cs"
    private_host.write_bytes(original_host.read_bytes())
    authored_before = {str(path): hashlib.sha256(path.read_bytes()).hexdigest()
                       for path in source_files(ctx.root / "src")}
    delivered_before = fingerprint(ctx)
    original_prepare = ctx.recipe.prepare

    def reproduction_prepare(child):
        translation = original_prepare(child)
        original_configure = translation.configure

        def configure(context, project, form):
            if original_configure:
                original_configure(context, project, form)
            tree = ET.parse(project)
            nodes = [node for node in tree.iter("Compile") if node.get("Link") == "S2nHost.cs"]
            if len(nodes) != 1:
                raise RuntimeError("Expected the real authored host reference exactly once")
            actual = (project.parent / nodes[0].get("Include")).resolve()
            if actual not in (original_host.resolve(), private_host.resolve()):
                raise RuntimeError("Unexpected authored host reference in reproduction product")
            nodes[0].set("Include", os.path.relpath(private_host, project.parent))
            ET.indent(tree, space="  ")
            tree.write(project, encoding="unicode")
            context.receipt.setdefault("reproduction_authored_host", {})[form] = {
                "original": str(original_host), "private": str(private_host),
                "sha256": hashlib.sha256(private_host.read_bytes()).hexdigest()}

        translation.configure = configure
        return translation

    recipe = replace(ctx.recipe, profiles=(*ctx.recipe.profiles, profile), prepare=reproduction_prepare)
    report = {"status": "running", "scope": "shared framework regeneration, offline/cwd/provenance, authored source and failed publication",
              "destination_only_profile": profile, "source_profile": ctx.profile,
              "authored_before_sha256": authored_before, "delivered_before_sha256": delivered_before,
              "authored_snapshot": {"original": str(original_host), "private": str(private_host),
                                    "sha256": hashlib.sha256(private_host.read_bytes()).hexdigest()}, "attempts": []}
    destination = ctx.artifacts / "reproduction.json"

    def save():
        destination.write_text(json.dumps(report, indent=2) + "\n")

    def attempt(policy, selected_recipe=recipe, expect_failure=False):
        options = copy.copy(ctx.options)
        options.action, options.form, options.fetch, options.tools = "translate", "all", "never", "reuse"
        options.hashes, options.suite, options.unit = policy, None, None
        options.profile = profile
        child = Context(ctx.repo, selected_recipe, options, ctx.session)
        tasks = task_plan(child, ())
        child.start()
        child.receipt["planned_tasks"] = [{"name": task.name, "status": "pending"} for task in tasks]
        error = None
        try:
            for task in tasks:
                child.task(task.name, task.action)
        except Exception as caught:
            error = caught
        child.finish(error)
        report["attempts"].append({"receipt": str(child.artifacts / "receipt.json"), "expected_failure": expect_failure,
                                    "status": child.receipt["status"]})
        save()
        if (error is not None) != expect_failure:
            raise RuntimeError("Unexpected reproduction outcome: " + str(child.artifacts / "receipt.json")) from error
        return child

    save()
    original_cwd = Path.cwd()
    try:
        with tempfile.TemporaryDirectory(prefix="dotcc-s2n-reproduction-") as temporary:
            os.chdir(temporary)
            for policy in ("warn", "off"):
                product = attempt(policy)
            before = fingerprint(product)

            def fail_prepare(child):
                translation = reproduction_prepare(child)
                invalid = child.work / "intentional-invalid.c"
                invalid.write_text("#error deliberate reproduction failure before publication\n")
                translation.units.insert(0, Unit(invalid))
                return translation

            attempt("warn", replace(recipe, prepare=fail_prepare), expect_failure=True)
            if fingerprint(product) != before:
                raise RuntimeError("Failed emission changed the previously published product or current pointer")
            report["failed_publication_preserved"] = True
            host = private_host
            original = host.read_bytes()
            generated_before = fingerprint(product)
            probe = Path(temporary) / "AuthoredProbe"
            probe.mkdir()
            try:
                host.write_bytes(original + b'\npublic static unsafe partial class S2nTls { public static int ReproductionAuthoredProbe() => 731; }\n')
                (probe / "AuthoredProbe.csproj").write_text(
                    '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
                    '<OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertyGroup>'
                    '<ItemGroup><ProjectReference Include="' + str(product.layout.project(profile)) + '" /></ItemGroup></Project>')
                (probe / "Program.cs").write_text('if (Managed.Security.S2n.S2nTls.ReproductionAuthoredProbe() != 731) throw new System.Exception("Authored edit not picked up");')
                ctx.run(["dotnet", "run", "--project", probe / "AuthoredProbe.csproj", "-c", "Release"], "authored-edit-pickup", timeout=1200)
                if fingerprint(product) != generated_before:
                    raise RuntimeError("Authored rebuild unexpectedly changed generated source/publication files")
                report["authored_edit_without_regeneration"] = True
            finally:
                host.write_bytes(original)
                ctx.managed("build", product.layout.project(profile), "authored-edit-restored", timeout=1200)
        if fingerprint(ctx) != delivered_before:
            raise RuntimeError("Isolated reproduction changed the qualified delivery or current pointer")
        authored_after = {str(path): hashlib.sha256(path.read_bytes()).hexdigest()
                          for path in source_files(ctx.root / "src")}
        if authored_before != authored_after:
            raise RuntimeError("Regeneration changed the original authored source tree")
        report["qualified_delivery_unchanged"] = True
        report["original_authored_sources_preserved"] = True
    finally:
        os.chdir(original_cwd)
        save()
    report["status"] = "passed"
    save()
