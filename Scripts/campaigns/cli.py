"""Command-line policy and dependency planning for discovered campaigns."""
import argparse
import copy
import json
import os
from pathlib import Path
import platform
import signal
import sys
from .delivery import lock, recover
from .inputs import acquire
from .layout import Layout, check, scaffold
from .model import Task, ordered_tasks
from .recipes import discover, load
from .runner import Context
from .testing import select, run_suite, forms
from .translation import translate, check_product

REPO = Path(__file__).resolve().parents[2]


def parser():
    result = argparse.ArgumentParser(description="Shared DotCC translation campaigns")
    result.add_argument("action", choices=("list", "layout", "fetch", "translate", "build", "test", "verify", "probe"))
    result.add_argument("project", nargs="?", choices=(*discover(REPO), "all"), default="all")
    result.add_argument("--profile")
    result.add_argument("--fetch", choices=("missing", "never"))
    result.add_argument("--no-fetch", "--offline", dest="fetch", action="store_const", const="never")
    result.add_argument("--hashes", choices=("off", "warn", "strict"), default=os.environ.get("DOTCC_CAMPAIGN_HASHES", "warn"))
    result.add_argument("--tools", choices=("build", "reuse"))
    result.add_argument("--no-build-tools", dest="tools", action="store_const", const="reuse")
    result.add_argument("--restore", choices=("normal", "locked", "none"), default=os.environ.get("DOTCC_CAMPAIGN_RESTORE", "normal"))
    result.add_argument("--jobs", type=int, default=4)
    result.add_argument("--timeout", type=int, default=600)
    result.add_argument("--form", choices=("raw", "processed", "all"))
    result.add_argument("--raw", "--no-postprocess", dest="form", action="store_const", const="raw")
    result.add_argument("--all", dest="form", action="store_const", const="all")
    result.add_argument("--mode", choices=("jit", "aot", "all"))
    result.add_argument("--aot", dest="mode", action="store_const", const="all")
    machine = "arm64" if platform.machine().lower() in ("aarch64", "arm64") else "x64"
    system = "win" if sys.platform == "win32" else "osx" if sys.platform == "darwin" else "linux"
    result.add_argument("--rid", "--runtime", default=system + "-" + machine)
    result.add_argument("--suite", action="append")
    result.add_argument("--suites", action="store_true", help="list available suites")
    result.add_argument("--group", action="append", help="source group (product, tests, or all)")
    result.add_argument("--unit", action="append")
    result.add_argument("--probe", action="store_true", help="compatibility alias for the probe command")
    result.add_argument("--managed-profile", action="store_true", help="compatibility alias for the default managed profile")
    result.add_argument("--pristine", "--unadapted", action="store_true")
    result.add_argument("--fast", action="store_true", help="omit qualification gates if supported by the recipe")
    result.add_argument("--with", dest="with_suites", choices=("repository",), action="append", default=[])
    result.add_argument("--dry-run", action="store_true")
    layout = result.add_mutually_exclusive_group()
    layout.add_argument("--check", action="store_true")
    layout.add_argument("--write", action="store_true")
    return result


def options_for(options, recipe):
    args = copy.copy(options)
    args.profile = args.profile or recipe.default_profile
    if args.profile not in recipe.profiles:
        raise ValueError(f"{recipe.name}: profile must be one of {recipe.profiles}")
    args.fetch = args.fetch or os.environ.get("DOTCC_CAMPAIGN_FETCH") or ("missing" if args.action in ("fetch", "translate", "verify") else "never")
    args.tools = args.tools or ("build" if args.action in ("translate", "verify") else "reuse")
    args.form = args.form or ("all" if args.action in ("translate", "verify") else "processed")
    args.mode = args.mode or ("all" if args.action == "verify" else "jit")
    if not 1 <= args.jobs <= 16 or args.timeout <= 0:
        raise ValueError("--jobs must be 1..16 and --timeout must be positive")
    if (args.unit or args.pristine) and args.action != "probe":
        raise ValueError("--unit and --pristine/--unadapted require probe")
    if args.pristine and not recipe.supports_pristine:
        raise ValueError(f"{recipe.name} does not support pristine probing")
    if args.fast and (not recipe.supports_fast or args.action != "translate"):
        raise ValueError(f"{recipe.name}: --fast requires a recipe that supports fast translation")
    if args.write and args.action != "layout":
        raise ValueError("--write requires layout")
    if recipe.validate_options:
        recipe.validate_options(args)
    return args


def source_selection(ctx):
    sources = list(ctx.recipe.sources(ctx.root))
    groups = ctx.options.group or (["all"] if ctx.options.action in ("fetch", "verify") else ["product"])
    unknown = set(groups) - {"all", *(source.group for source in sources)}
    if unknown:
        raise ValueError("Unknown source groups: " + ", ".join(sorted(unknown)))
    return [source for source in sources if "all" in groups or source.group in groups]


def task_plan(ctx, suites):
    args = ctx.options
    tasks = []
    def inputs():
        for source in source_selection(ctx):
            ctx.sources[source.name] = acquire(source, ctx.policy, args.fetch)
            ctx.receipt.setdefault("requested_sources", {})[source.name] = {
                "url": source.url, "archive": str(source.archive), "sha256": source.sha256,
                "group": source.group, "directory": str(source.directory)}
        ctx.receipt["sources"] = {name: str(path) for name, path in ctx.sources.items()}
    if args.action in ("fetch", "translate", "verify", "probe"):
        tasks.append(Task("inputs", inputs))
    if args.action in ("translate", "verify", "probe"):
        tasks.append(Task("tools", ctx.tools, ("inputs",)))
        def emission():
            if ctx.recipe.before_translate:
                ctx.recipe.before_translate(ctx)
            if ctx.recipe.translate:
                ctx.recipe.translate(ctx)
            else:
                translate(ctx)
            if ctx.recipe.after_translate:
                ctx.recipe.after_translate(ctx)
        tasks.append(Task("translation", emission, ("tools",)))
    if args.action == "build":
        def build():
            for form in forms(ctx):
                project = check_product(ctx, form)
                ctx.managed("build", project, "build-" + form)
            if args.form != "raw" and ctx.profile == ctx.recipe.default_profile:
                ctx.managed("build", ctx.layout.solution, "consumer-solution-build")
        tasks.append(Task("build", build))
    previous = "translation" if args.action == "verify" else None
    for suite in suites:
        tasks.append(Task("test:" + suite.name, lambda suite=suite: run_suite(ctx, suite),
                          (previous,) if previous else ()))
        previous = "test:" + suite.name
    return ordered_tasks(tasks)


def run_project(recipe, args, session):
    ctx = Context(REPO, recipe, args, session)
    suites = select(recipe, args) if args.action in ("test", "verify") else []
    for profile in dict.fromkeys(profile for suite in suites for profile in suite.required_profiles):
        if args.action == "verify":
            if ("delivery", recipe.name, profile) not in session:
                child = copy.copy(args)
                child.action, child.profile, child.suite = "translate", profile, None
                child.form = "all"
                run_project(recipe, child, session)
        elif not args.dry_run:
            for form in ("raw", "processed"):
                if not ctx.layout.project(profile, form).is_file():
                    raise RuntimeError(f"Missing {recipe.name} {profile}/{form}; translate --profile {profile} first or use verify")
    dependencies = list(dict.fromkeys(dependency for suite in suites for dependency in suite.dependencies))
    if args.action in ("build", "test", "verify"):
        dependencies = list(dict.fromkeys([*recipe.dependencies, *dependencies]))
    for name in dependencies:
        dependency = load(REPO, name)
        dependency_layout = Layout(REPO / name, dependency.product, dependency.default_profile)
        if args.action == "verify":
            key = ("delivery", name, dependency.default_profile)
            if key not in session:
                child = copy.copy(args)
                child.action, child.profile, child.suite = "translate", dependency.default_profile, None
                child.fast = False
                run_project(dependency, child, session)
        elif not args.dry_run:
            for form in forms(ctx):
                if not dependency_layout.project(form=form).is_file():
                    raise RuntimeError(f"Missing {name} {form} dependency; translate {name} first or use verify")
    tasks = task_plan(ctx, suites)
    issues = check(ctx.layout, recipe)
    if args.dry_run:
        print(json.dumps({"project": recipe.name, "action": args.action, "profile": ctx.profile,
                          "hash_policy": args.hashes, "fetch": args.fetch, "tools": args.tools,
                          "tasks": [{"name": task.name, "dependencies": task.dependencies} for task in tasks],
                          "suites": [{"name": suite.name, "scope": suite.scope, "matrix": suite.matrix,
                                      "required_profiles": suite.required_profiles} for suite in suites],
                          "sources": [{"name": s.name, "directory": str(s.directory), "archive": str(s.archive)}
                                      for s in source_selection(ctx)],
                          "output": str(ctx.layout.project(ctx.profile)), "layout_issues": issues}, indent=2))
        return
    if issues:
        raise RuntimeError("; ".join(issues) + f"; run bash Scripts/campaign.sh layout {recipe.name} --write")
    with lock(ctx.root / "build/campaign/run.lock"):
        recover(ctx.root / "build/campaign/promotion.json")
        ctx.start()
        ctx.receipt["planned_tasks"] = [{"name": task.name, "status": "pending"} for task in tasks]
        try:
            for task in tasks:
                ctx.task(task.name, task.action)
        except BaseException as error:
            ctx.finish(error)
            raise
        ctx.finish()
        if args.action in ("translate", "verify"):
            session.add(("delivery", recipe.name, ctx.profile))


def main(argv=None):
    cli = parser()
    options = cli.parse_args(argv)
    if options.probe:
        if options.action != "translate":
            cli.error("--probe is a translate compatibility option")
        options.action = "probe"
    if options.with_suites and options.action != "verify":
        cli.error("--with repository requires verify")
    projects = discover(REPO) if options.project == "all" else (options.project,)
    session = set()
    try:
        recipes = [(load(REPO, name)) for name in projects]
        selections = [(recipe, options_for(options, recipe)) for recipe in recipes]
        # Validate selection for every project before the first mutation.
        for recipe, args in selections:
            if args.action in ("test", "verify"):
                select(recipe, args)
        for recipe, args in selections:
            layout = Layout(REPO / recipe.name, recipe.product, recipe.default_profile)
            if args.action == "list":
                print(f"{recipe.name}: {recipe.product}; profiles={','.join(recipe.profiles)}")
                if args.suites:
                    for suite in recipe.suites:
                        print(f"  {suite.name}: {suite.scope}; dependencies={','.join(suite.dependencies) or 'none'}")
                for issue in check(layout, recipe):
                    print("  layout: " + issue)
            elif args.action == "layout":
                if args.write and not args.dry_run:
                    scaffold(layout, recipe)
                issues = check(layout, recipe)
                print(recipe.name + ": " + ("; ".join(issues) if issues else "layout OK"))
                if issues and not args.dry_run:
                    raise RuntimeError("Layout is incomplete")
            else:
                run_project(recipe, args, session)
        if options.with_suites and not options.dry_run:
            if options.action != "verify":
                raise ValueError("--with repository requires verify")
            recipe, args = selections[-1]
            ctx = Context(REPO, recipe, args, session)
            ctx.start()
            try:
                for name in ("DotCC.Tests", "DotCC.FunctionalTests", "DotCC.PostProcess.Tests"):
                    ctx.managed("test", REPO / name / (name + ".csproj"), name, timeout=3600)
            except BaseException as error:
                ctx.finish(error)
                raise
            ctx.finish()
        return 0
    except (ValueError, RuntimeError, OSError) as error:
        print(f"campaign: {error}", file=sys.stderr)
        return 1
    except KeyboardInterrupt:
        print("campaign: cancelled", file=sys.stderr)
        return 130


def _terminated(signum, frame):
    raise KeyboardInterrupt


signal.signal(signal.SIGTERM, _terminated)
