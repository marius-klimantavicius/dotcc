"""Pinta wrappers, local-context patches, and consumer profiles."""
import shutil
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, lines, link_flags


def sources(root):
    pin = json_file(root / "config/source-lock.json")
    yield Source("product", pin["archive_url"], root / "ref/upstream.tar.gz", root / "ref/upstream",
                 "pinta-" + pin["commit"], pin["archive_sha256"], ("Marius.Pinta/inc/pinta.h",),
                 files={entry["path"]: entry["sha256"] for entry in pin["files"] + pin["fixtures"]})


def prepare(ctx):
    source = ctx.work / "source"
    helper(ctx.root / "scripts/stage.py").stage(ctx.sources["product"], source,
                                              pristine=ctx.options.pristine, hashes=ctx.policy)
    wrappers = ctx.work / "wrappers"
    wrappers.mkdir()
    units = []
    for name in lines(ctx.root / "config/core-sources.txt"):
        path = wrappers / name.rsplit("/", 1)[-1]
        path.write_text('#include "pinta-preinclude.h"\n#include "' + name + '"\n')
        units.append(Unit(path))
    definitions = [value for value in lines(ctx.root / "config/core-defines.txt") if not value.startswith("PINTA_DEBUG=")]
    definitions.append("PINTA_DEBUG=" + str(int(ctx.profile == "debug")))
    return Translation(units, ["-std=c17", *flags(definitions, [ctx.root / "config", source, source / "Marius.Pinta/inc"])],
                       link_flags("Pinta", "Managed.Interpreters", literals=False))


def run_consumer(ctx, suite):
    return ctx.script("test.py", "--form", "all" if ctx.options.form == "all" else
                      ("optimized" if ctx.options.form == "processed" else "raw"),
                      "--mode", ctx.options.mode, "--profile", ctx.profile, timeout=suite.timeout)


def script_arguments(ctx, suite, script, args):
    args += ["--profile", ctx.profile]
    if script == "upstream-tests.py":
        ctx.script("native.py", "--stage", "corrected", "--profile", ctx.profile,
                   "--cases", label="upstream-native", timeout=suite.timeout)
        args += ["--mode", ctx.options.mode, "--form",
                 "optimized" if ctx.options.form == "processed" else ctx.options.form]
    elif script == "dependency-audit.py":
        args += ["--form", ctx.options.form]
    return args


recipe = Recipe("pinta", "TranslatedPinta", ("release", "debug"),
                ("src/ManagedApi/ManagedApi.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"), sources, prepare,
                script_arguments=script_arguments, suites=(Suite("consumer", (), 7200, run=run_consumer, matrix=True, platforms=("linux", "win32")),
                        Suite("upstream", ("script", "upstream-tests.py"), 7200),
                        Suite("audit", ("script", "dependency-audit.py"), 900)),
                supports_pristine=True, consumer=Consumer(property="PintaProject"),
                default_suites=("consumer",), verify_suites=("consumer", "upstream", "audit"))
