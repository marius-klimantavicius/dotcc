"""PGlite campaign: real-source diagnostics, native controls, explicit product gates."""
import os
from dataclasses import replace

from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.recipes import flags, helper, json_file, link_flags


def sources(root):
    pin = json_file(root / "config/sources.json")["product"]
    yield Source("product", pin["url"], root / "ref" / pin["archive"],
                 root / "ref" / pin["directory"], pin.get("prefix", pin["directory"]),
                 pin.get("sha256"), tuple(pin["required"]))


def prepare(ctx):
    return helper(ctx.root / "scripts/translation.py").prepare(ctx)


def native_control(ctx, suite):
    return helper(ctx.root / "scripts/native-control.py").run(ctx, suite)


def boundary(ctx, suite):
    return helper(ctx.root / "scripts/boundary.py").run(ctx, suite)


def atomics(ctx, suite):
    return helper(ctx.root / "scripts/atomics.py").run(ctx, suite)


def libc_build(ctx, suite):
    return helper(ctx.root / "scripts/libc-build.py").run(ctx, suite)


def portable_codegen(ctx, suite):
    return helper(ctx.root / "scripts/portability.py").run(ctx, suite)


def locale(ctx, suite):
    return helper(ctx.root / "scripts/locale-probe.py").run(ctx, suite)


def calendar_timezone(ctx, suite):
    return helper(ctx.root / "scripts/calendar-timezone-probe.py").run(ctx)


def initializer(ctx, suite):
    report = helper(ctx.root / "scripts/initializer.py").run(ctx, suite)
    ctx.receipt["pglite_initializer_validation"] = report
    return report


def modules(ctx, suite):
    return helper(ctx.root / "scripts/initializer.py").run_modules(ctx, suite)


def snowball(ctx, suite):
    from pathlib import Path
    info = helper(ctx.root / "scripts/initializer.py").configured(ctx)
    artifacts = ctx.artifacts / "snowball-api"
    artifacts.mkdir()
    return helper(ctx.root / "scripts/snowball-probe.py").execution(
        ctx, ctx.compiler, Path(info["source"]), ctx.work / "snowball-api", artifacts)


def backend_host(ctx, suite):
    from pathlib import Path
    probe = helper(ctx.root / "scripts/backendhost-probe.py")
    report = dict(host=probe.run(ctx, suite), headers=probe.run_header_compat(ctx, suite))
    frontend = ctx.receipt.get("pglite_initializer_validation")
    if frontend and not isinstance(report["host"].get("durable_rename"), dict):
        report["durable_rename"] = probe.run_durable_rename(
            ctx, Path(frontend["frontend_project"]).parent.parent,
            frontend["configured_source"], ctx.options.mode)
    ctx.receipt["pglite_backend_host_validation"] = report
    return report


def semaphore(ctx, suite):
    return helper(ctx.root / "scripts/semaphore-probe.py").run(ctx, suite)


def posix_host(ctx, suite):
    return helper(ctx.root / "scripts/posixhost-probe.py").run(ctx, suite)


def consumer(ctx, suite):
    from campaigns import testing
    from campaigns.layout import Layout
    recipe, selected_form = ctx.recipe, ctx.options.form
    try:
        for form in testing.forms(ctx):
            backend = ctx.layout.project(ctx.profile, form)
            frontend = Layout(ctx.root, "TranslatedInitdb", recipe.default_profile).project(ctx.profile, form)
            settings = replace(recipe.consumer,
                arguments=("--bootstrap-prefix", str(backend.parent / "bootstrap-prefix"),
                           "--work", "{work}/{label}-database"),
                build_arguments=("-m:1", "-p:InitdbTranslatedProject=" + str(frontend)))
            ctx.recipe = replace(recipe, consumer=settings)
            ctx.options.form = form
            testing.consumer(ctx, suite)
    finally:
        ctx.recipe, ctx.options.form = recipe, selected_form


def before_suite(ctx, suite):
    if suite.name in ("boundary", "libc-build", "atomics", "portable-codegen", "locale", "calendar-timezone", "initializer", "modules", "snowball", "backend-host", "semaphore", "posix-host"):
        if not hasattr(ctx, "compiler"):
            ctx.tools()
    if suite.name == "libc-build" and "product" not in ctx.sources:
        # Acquisition stays in the coordinator's ordinary suite preparation;
        # the standalone libc implementation only reads supplied inputs.
        from campaigns.inputs import acquire
        ctx.sources["product"] = acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
    if suite.name == "boundary" and os.environ.get("PGLITE_BOUNDARY_PREPARATION"):
        return
    if suite.name == "atomics" and os.environ.get("PGLITE_ATOMICS_SOURCE"):
        return
    if suite.name == "locale":
        if os.environ.get("PGLITE_LOCALE_SOURCE"):
            ctx.env["PGLITE_LOCALE_SOURCE"] = os.environ["PGLITE_LOCALE_SOURCE"]
        else:
            info = helper(ctx.root / "scripts/prepare.py").prepare(ctx)
            ctx.env["PGLITE_LOCALE_SOURCE"] = str(info["source"])
    if suite.name in ("boundary", "atomics"):
        helper(ctx.root / "scripts/prepare.py").prepare(ctx)
    if suite.name == "posix-host":
        if os.environ.get("PGLITE_POSIXHOST_SOURCE"):
            ctx.env["PGLITE_POSIXHOST_SOURCE"] = os.environ["PGLITE_POSIXHOST_SOURCE"]
        else:
            info = helper(ctx.root / "scripts/prepare.py").prepare(ctx)
            ctx.env["PGLITE_POSIXHOST_SOURCE"] = str(info["source"])


def inventory(ctx, suite):
    return helper(ctx.root / "scripts/inventory.py").run(ctx, suite)


recipe = Recipe(
    "pglite", "TranslatedPGlite", ("embedding-native", "native-diagnostic"),
    ("samples/ManagedConsumer/ManagedConsumer.csproj", "src/Api/PGlite.csproj"), sources, prepare,
    consumer=Consumer(property="TranslatedProject"),
    suites=(Suite("backend-host", (), run=backend_host, timeout=1800,
                  scope="real process/filesystem/numeric/network backend host services in native/JIT/AOT"),
            Suite("semaphore", (), run=semaphore, timeout=1800,
                  scope="real unnamed POSIX semaphore native/emitted ABI and two-owner controls"),
            Suite("posix-host", (), run=posix_host, timeout=1800,
                  scope="real shared-memory, terminal and NSS native/JIT/AOT host controls"),
            Suite("initializer", (), run=initializer, timeout=1800,
                  scope="original initdb/frontend emission, build and translated transport contracts"),
            Suite("modules", (), run=modules, timeout=1800,
                  scope="original required PL/pgSQL/Snowball sources and static export closure"),
            Suite("snowball", (), run=snowball, timeout=1800,
                  scope="original English/Lithuanian Snowball algorithms and allocation in native/JIT/AOT"),
            Suite("locale", (), run=locale, timeout=1800,
                  scope="actual PostgreSQL locale ABI and C/C.UTF-8 host native/JIT/AOT scalar comparison"),
            Suite("calendar-timezone", (), run=calendar_timezone, timeout=1800,
                  scope="actual calendar ABI, timezone names/offsets and native/JIT/AOT comparison in five zones"),
            Suite("portable-codegen", (), run=portable_codegen,
                  scope="actual upstream software CRC32C/popcount native/emitted vectors and length/alignment cases"),
            Suite("atomics", (), run=atomics, timeout=1800,
                  scope="actual native/emitted PG C11 atomic port, bounded concurrency and full query-loop retry"),
            Suite("libc-build", (), run=libc_build,
                  scope="complete actual fork libc unit: private emission and C# build, required host closure"),
            Suite("native-control", (), run=native_control, timeout=1800,
                  scope="same-revision native SQL/transactions/persistence control; embedding parity separately reported"),
            Suite("boundary", (), run=boundary, timeout=1800,
                  scope="actual-header native/emitted layout and nonlocal-error feasibility; not SQL product qualification"),
            Suite("inventory", (), run=inventory,
                  scope="source/configuration manifest and explicit product completeness checks"),
            Suite("consumer", (), matrix=True, run=consumer,
                  scope="managed embedded init, SQL, transaction recovery, shutdown and reopen persistence")),
    default_suites=("consumer",),
    verify_suites=("inventory", "native-control", "libc-build", "initializer", "modules", "snowball", "boundary", "atomics", "semaphore", "portable-codegen", "locale", "calendar-timezone", "posix-host", "backend-host", "consumer"),
    before_suite=before_suite,
)
