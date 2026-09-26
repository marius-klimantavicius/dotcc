"""Suite selection and execution against explicitly selected library artifacts."""
from pathlib import Path
import sys
from .translation import check_product


def select(recipe, options):
    names = options.suite or (recipe.verify_suites if options.action == "verify" else recipe.default_suites)
    suites = {suite.name: suite for suite in recipe.suites}
    if names == ["all"] or names == ("all",):
        names = list(suites)
    unknown = set(names) - suites.keys()
    if unknown:
        raise ValueError("Unknown suites: " + ", ".join(sorted(unknown)))
    selected = [suites[name] for name in dict.fromkeys(names)]
    for suite in selected:
        if suite.platforms and sys.platform not in suite.platforms:
            raise ValueError(f"{recipe.name}/{suite.name} supports {suite.platforms}, not {sys.platform}")
        profile = options.profile or recipe.default_profile
        if suite.profiles and profile not in suite.profiles:
            raise ValueError(f"{suite.name} does not support profile {profile}")
    return selected


def forms(ctx):
    return ("raw", "processed") if ctx.options.form == "all" else (ctx.options.form,)


def modes(ctx):
    return ("jit", "aot") if ctx.options.mode == "all" else (ctx.options.mode,)


def consumer(ctx, suite):
    settings = ctx.recipe.consumer
    project = ctx.root / settings.project
    if settings.build_solution:
        ctx.managed("build", ctx.layout.solution, "consumer-solution-build")
    for form in forms(ctx):
        product = check_product(ctx, form)
        prop = f"-p:{settings.property}={product}"
        for mode in modes(ctx):
            label = f"consumer-{form}-{mode}"
            output = ctx.work / label
            if mode == "jit":
                ctx.managed("build", project, label + "-build", prop, *settings.build_arguments, "-o", output)
                # Read AssemblyName rather than assuming the csproj basename.
                import xml.etree.ElementTree as ET
                assembly = ET.parse(project).findtext(".//AssemblyName") or project.stem
                command = ["dotnet", output / (assembly + ".dll")]
            else:
                ctx.managed("publish", project, label + "-publish", prop, "-r", ctx.options.rid,
                            "-p:PublishAot=true", *settings.build_arguments, "-o", output, timeout=1800)
                import xml.etree.ElementTree as ET
                assembly = ET.parse(project).findtext(".//AssemblyName") or project.stem
                command = [output / (assembly + (".exe" if sys.platform == "win32" else ""))]
            command += [argument.format(root=ctx.root, work=ctx.work, label=label)
                        for argument in settings.arguments]
            ctx.run(command, label + "-run", timeout=120, separate=True)


def run_suite(ctx, suite):
    ctx.env["DOTCC_CAMPAIGN_EXISTING"] = "1"
    ctx.receipt.setdefault("suites", []).append({"name": suite.name, "scope": suite.scope,
                                                "matrix": suite.matrix})
    if ctx.recipe.before_suite:
        ctx.recipe.before_suite(ctx, suite)
    if suite.run:
        return suite.run(ctx, suite)
    kind, *parts = suite.command
    if kind == "consumer":
        return consumer(ctx, suite)
    if kind == "script":
        script, *args = parts
        if ctx.recipe.script_arguments:
            args = ctx.recipe.script_arguments(ctx, suite, script, args)
        return ctx.script(script, *args, label=suite.name, timeout=suite.timeout)
    command = [sys.executable if kind == "python" else kind,
               *(part.replace("{root}", str(ctx.root)) for part in parts)]
    return ctx.run(command, suite.name, suite.timeout)
