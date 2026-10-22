"""PGlite campaign: real-source diagnostics, native controls, explicit product gates."""
import os

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


def before_suite(ctx, suite):
    if suite.name in ("boundary", "libc-build", "atomics", "portable-codegen"):
        if not hasattr(ctx, "compiler"):
            ctx.tools()
    if suite.name == "boundary" and os.environ.get("PGLITE_BOUNDARY_PREPARATION"):
        return
    if suite.name == "atomics" and os.environ.get("PGLITE_ATOMICS_SOURCE"):
        return
    if suite.name in ("boundary", "atomics"):
        helper(ctx.root / "scripts/prepare.py").prepare(ctx)


def inventory(ctx, suite):
    return helper(ctx.root / "scripts/inventory.py").run(ctx, suite)


recipe = Recipe(
    "pglite", "TranslatedPGlite", ("embedding-native", "native-diagnostic"),
    ("samples/ManagedConsumer/ManagedConsumer.csproj",), sources, prepare,
    consumer=Consumer(property="TranslatedProject"),
    suites=(Suite("portable-codegen", (), run=portable_codegen,
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
            Suite("consumer", ("consumer",), matrix=True,
                  scope="managed embedded init, SQL, transaction recovery, shutdown and reopen persistence")),
    default_suites=("consumer",),
    verify_suites=("inventory", "native-control", "libc-build", "boundary", "atomics", "portable-codegen", "consumer"),
    before_suite=before_suite,
)
