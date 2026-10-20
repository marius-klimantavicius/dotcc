"""Differentially qualify original and replaced upstream primitives, test-only."""
from __future__ import annotations
import json
import hashlib
from pathlib import Path
import shutil
from types import SimpleNamespace
import xml.etree.ElementTree as ET

from campaigns.inputs import acquire
from campaigns.recipes import flags, helper, json_file, link_flags
from campaigns.testing import forms, modes
from campaigns.translation import relocate_project


def result(transcript):
    rows = [json.loads(line) for line in transcript.splitlines() if line.startswith('{"checks":')]
    if len(rows) != 1 or set(rows[0]) != {"checks", "digest", "guard3"} or rows[0]["checks"] < 200000 or rows[0]["guard3"] is not True:
        raise RuntimeError("Incomplete primitive differential result: " + transcript[-1000:])
    return rows[0]


def run(ctx, suite):
    if ctx.options.rid != "linux-x64":
        raise RuntimeError("Primitive guard-page qualification requires executed linux-x64")
    work = ctx.work / "primitives"
    work.mkdir(parents=True)
    report = dict(status="running", native=None, rows=[], selection=[], controls=[],
                  source_sha256={str(path.relative_to(ctx.root)): hashlib.sha256(path.read_bytes()).hexdigest()
                                 for path in (ctx.root / "tests/Primitives").glob("*") if path.is_file()})
    report_path = ctx.artifacts / "primitives-report.json"
    try:
        source = ctx.sources.get("product") or acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
        staged = work / "source"
        shutil.copytree(source / "lib", staged / "lib")
        policy = json_file(ctx.root / "config/translation.json")
        test = ctx.root / "tests/Primitives/primitives.c"
        common = ["-std=" + policy["dialect"], *flags(policy["defines"], [staged / "lib/common"]),
                  *("-U" + name for name in policy["undefines"])]
        native = work / "primitives-native"
        ctx.run(["cc", "-O2", "-D_DEFAULT_SOURCE", "-DDZ_NATIVE_MAIN=1", *common, test, "-o", native], "primitives-native-build", timeout=300)
        report["native"] = result(ctx.run([native], "primitives-native-run"))
        ctx.run([native, "--probe-guard"], "primitives-native-inaccessible-control", check=False)
        native_fault = ctx.receipt["commands"][-1]
        if native_fault.get("exit_code") not in (-11, 139):
            raise RuntimeError("Native inaccessible page control did not fault with SIGSEGV")
        native_fault.update(deliberate_failure_control=True, expected_signal="SIGSEGV")
        ctx.tools()
        primitives = helper(ctx.root / "scripts/primitives.py")
        controls = helper(ctx.root / "tests/Primitives/profile_checks.py")
        report["policy_controls"] = controls.policy_checks(ctx, primitives, staged, test, common)
        for profile in ("original", "portable"):
            variant = SimpleNamespace(root=ctx.root, work=work / profile, profile=profile, policy=ctx.policy)
            variant.work.mkdir()
            object_path = variant.work / "primitives.cs"
            selection = ctx.artifacts / ("primitives-" + profile + "-overrides.jsonl")
            unit_flags = primitives.unit_flags(variant, test, staged, primitives.all_names(ctx.root))
            ctx.run(["dotnet", ctx.compiler, *common, *unit_flags, "--override-report", selection,
                     test, "--emit=obj", "-o", object_path], "primitives-" + profile + "-emit", timeout=600)
            report["selection"].append(dict(profile=profile, **primitives.validate_report(variant, selection, test, staged, primitives.all_names(ctx.root))))
            if profile == "portable":
                report["controls"] = controls.check_reports(ctx, primitives, variant, selection, test, staged, primitives.all_names(ctx.root))
            raw = variant.work / "raw/TranslatedZstdPrimitives"
            ctx.run(["dotnet", ctx.compiler, object_path, "--emit=managedlib", *link_flags("CPrimitives", "Managed.Zstd.PrimitiveTest"), "-o", raw], "primitives-" + profile + "-link", timeout=600)
            projects = list(raw.glob("*.csproj"))
            if len(projects) != 1:
                raise RuntimeError("Expected one generated primitive test project")
            primitives.configure(variant, projects[0])
            products = {"raw": projects[0]}
            if "processed" in forms(ctx):
                processed = variant.work / "processed/TranslatedZstdPrimitives"
                shutil.copytree(raw, processed)
                project = processed / projects[0].name
                relocate_project(project, raw)
                ctx.managed("build", project, "primitives-" + profile + "-semantic-build", "-m:1")
                ctx.run(["dotnet", ctx.postprocessor, project, "--in-place"], "primitives-" + profile + "-postprocess", timeout=900)
                products["processed"] = project
            runner = variant.work / "runner"
            runner.mkdir()
            project = runner / "PrimitiveTests.csproj"
            tree = ET.Element("Project", Sdk="Microsoft.NET.Sdk")
            properties = ET.SubElement(tree, "PropertyGroup")
            for name, value in {"OutputType":"Exe", "AllowUnsafeBlocks":"true", "IsAotCompatible":"true", "PrimitiveProject":"MISSING"}.items():
                ET.SubElement(properties, name).text=value
            group = ET.SubElement(tree,"ItemGroup")
            ET.SubElement(group,"ProjectReference",Include="$(PrimitiveProject)")
            ET.SubElement(group,"Compile",Include=str(ctx.root / "tests/Primitives/Program.cs"))
            ET.ElementTree(tree).write(project, encoding="unicode")
            for form in forms(ctx):
                for mode in modes(ctx):
                    label = "primitives-" + profile + "-" + form + "-" + mode
                    output = variant.work / (form + "-" + mode)
                    args = ["-p:PrimitiveProject=" + str(products[form]), "--artifacts-path", variant.work / "intermediates" / (form + "-" + mode), "-m:1", "-o", output]
                    if mode == "jit":
                        ctx.managed("build", project, label + "-build", *args)
                        command = ["dotnet", output / "PrimitiveTests.dll"]
                    else:
                        ctx.managed("publish", project, label + "-publish", *args, "-r", ctx.options.rid, "-p:PublishAot=true", timeout=1800)
                        command = [output / "PrimitiveTests"]
                    observed = result(ctx.run(command, label + "-run", timeout=120))
                    if observed != report["native"]:
                        raise RuntimeError("Native/original/optimized primitive differential mismatch: " + label)
                    ctx.run([*command, "--probe-guard"], label + "-inaccessible-control", check=False, timeout=120)
                    control = ctx.receipt["commands"][-1]
                    # .NET reports its fatal AccessViolation as SIGABRT on Linux.
                    if control.get("exit_code") not in (-11, 139, -6, 134):
                        raise RuntimeError("Managed inaccessible page control did not fault")
                    control.update(deliberate_failure_control=True, expected_signal="SIGSEGV or managed fatal AccessViolation SIGABRT")
                    report["rows"].append(dict(profile=profile, form=form, mode=mode, result=observed, status="pass"))
                    report_path.write_text(json.dumps(report, indent=2) + "\n")
        report["status"] = "pass"
    except Exception as error:
        report.update(status="fail", error=str(error))
        raise
    finally:
        report_path.write_text(json.dumps(report, indent=2) + "\n")
