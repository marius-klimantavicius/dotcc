"""Reproduction gates using shared tasks under the caller's campaign lock."""
import copy
from dataclasses import replace
import hashlib
import json
import os
from pathlib import Path
import tempfile

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
    report = {"status": "running", "scope": "shared framework regeneration, offline/cwd/provenance, authored source and failed publication", "attempts": []}
    destination = ctx.artifacts / "reproduction.json"

    def save():
        destination.write_text(json.dumps(report, indent=2) + "\n")

    def attempt(policy, recipe=None, expect_failure=False):
        options = copy.copy(ctx.options)
        options.action, options.form, options.fetch, options.tools = "translate", "all", "never", "reuse"
        options.hashes, options.suite, options.unit = policy, None, None
        child = Context(ctx.repo, recipe or ctx.recipe, options, ctx.session)
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

    save()
    original_cwd = Path.cwd()
    try:
        with tempfile.TemporaryDirectory(prefix="dotcc-s2n-reproduction-") as temporary:
            os.chdir(temporary)
            for policy in ("warn", "off"):
                attempt(policy)
            before = fingerprint(ctx)
            original_prepare = ctx.recipe.prepare

            def fail_prepare(child):
                translation = original_prepare(child)
                invalid = child.work / "intentional-invalid.c"
                invalid.write_text("#error deliberate reproduction failure before publication\n")
                translation.units.insert(0, Unit(invalid))
                return translation

            attempt("warn", replace(ctx.recipe, prepare=fail_prepare), expect_failure=True)
            if fingerprint(ctx) != before:
                raise RuntimeError("Failed emission changed the previously published product or current pointer")
            report["failed_publication_preserved"] = True
            host = ctx.root / "src/Boundary/S2nHost.cs"
            original = host.read_bytes()
            generated_before = fingerprint(ctx)
            probe = Path(temporary) / "AuthoredProbe"
            probe.mkdir()
            try:
                host.write_bytes(original + b'\npublic static unsafe partial class S2nTls { public static int ReproductionAuthoredProbe() => 731; }\n')
                (probe / "AuthoredProbe.csproj").write_text(
                    '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><TargetFramework>net10.0</TargetFramework>'
                    '<OutputType>Exe</OutputType><AllowUnsafeBlocks>true</AllowUnsafeBlocks></PropertyGroup>'
                    '<ItemGroup><ProjectReference Include="' + str(ctx.layout.project(ctx.profile)) + '" /></ItemGroup></Project>')
                (probe / "Program.cs").write_text('if (Managed.Security.S2n.S2nTls.ReproductionAuthoredProbe() != 731) throw new System.Exception("Authored edit not picked up");')
                ctx.run(["dotnet", "run", "--project", probe / "AuthoredProbe.csproj", "-c", "Release"], "authored-edit-pickup", timeout=1200)
                if fingerprint(ctx) != generated_before:
                    raise RuntimeError("Authored rebuild unexpectedly changed generated source/publication files")
                report["authored_edit_without_regeneration"] = True
            finally:
                host.write_bytes(original)
                ctx.managed("build", ctx.layout.project(ctx.profile), "authored-edit-restored", timeout=1200)
    finally:
        os.chdir(original_cwd)
        save()
    report["status"] = "passed"
    save()
