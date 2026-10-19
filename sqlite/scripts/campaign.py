"""SQLite amalgamation, guarded host adaptation, and corpus suites."""
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from pathlib import Path
from campaigns.recipes import flags, helper, json_file, lines, link_flags


def sources(root):
    for group, pin in json_file(root / "config/sources.json").items():
        yield Source(group, pin["url"], root / "ref" / pin["archive"],
                     root / "ref" / pin["directory"], Path(pin["directory"]).name,
                     pin.get("sha256"), tuple(pin.get("required", ())), group)


def prepare(ctx):
    from production_support import production_sources
    ctx.receipt["production_inputs"] = production_sources(ctx.root)
    source = ctx.sources["product"]
    identity = helper(ctx.root / "scripts/product_identity.py")
    identity.runtime_binding(ctx.repo, ctx.compiler)
    ctx.receipt["sqlite_source_inputs"] = identity.source_files(ctx.root, source)
    staged = ctx.work / "source"
    helper(ctx.root / "scripts/prepare-host-source.py").prepare(source, staged, hash_mode=ctx.options.hashes)
    definitions = lines(ctx.root / "config/defines.txt") + lines(ctx.root / "config/host-defines.txt")
    return Translation([Unit(staged / "sqlite3.c")], ["-std=c17", *flags(definitions, [staged]),
                       "--overrides-file", str(ctx.root / "config/dotcc-overrides.json"),
                       "--override-report", str(ctx.artifacts / "overrides.jsonl")],
                       link_flags("Sqlite", "Managed.Database", 262144), configure=configure_notices)


def configure_notices(ctx, project, form):
    ctx.receipt.setdefault("redistributable_notices", {})[form] = helper(
        ctx.root / "scripts/notices.py").package(ctx.root, project)


def run_corpora(ctx, suite):
    expected = {"core": ("native.sh", "native-corpus.expected"),
                "api": ("test-api-native.sh", "native-api.expected"),
                "vfs": ("test-vfs-native.sh", "native-vfs.expected"),
                "vtable": ("test-vtable-native.sh", "native-vtable.expected"),
                "allocation": ("test-allocation-native.sh", "native-allocation.expected"),
                "upstream": ("test-upstream-native.sh", "upstream-jsonb.expected"),
                "fts5": ("test-fts5-native.sh", "native-fts5.expected")}
    for name, (script, transcript) in expected.items():
        output = ctx.run(["bash", ctx.root / "scripts" / script], "native-" + name,
                         separate=True, timeout=600)
        if output != (ctx.root / "tests" / transcript).read_text():
            raise RuntimeError("Native SQLite transcript differs: " + name)
        ctx.script("test-translated.sh", name, label="translated-" + name, timeout=1800)
    return


def before_suite(ctx, suite):
    ctx.env["SQLITE_AOT"] = "1" if ctx.options.mode in ("aot", "all") else "0"
    if ctx.options.action == "verify":
        ctx.env["SQLITE_EXPECTED_RECEIPT"] = str(ctx.artifacts / "receipt.json")
    if suite.name in ("host-vfs", "layout", "threading", "endian"):
        identity = helper(ctx.root / "scripts/product_identity.py")
        expected = ctx.env.get("SQLITE_EXPECTED_RECEIPT")
        compiler = ctx.env.get("DOTCC_COMPILER")
        identity.require(bool(expected) and bool(compiler),
                         "set SQLITE_EXPECTED_RECEIPT and DOTCC_COMPILER; run translate first")
        identity.validate(ctx.root, expected, compiler)


def bind_product(ctx):
    helper(ctx.root / "scripts/product_identity.py").capture(ctx)


recipe = Recipe("sqlite", "TranslatedSqlite", ("default",),
                ("samples/ManagedConsumer/ManagedConsumer.csproj",), sources, prepare,
                suites=(Suite("consumer", ("consumer",), matrix=True),
                        Suite("corpora", (), 14400, run=run_corpora, platforms=("linux",)),
                        Suite("host-vfs", ("script", "test-host-vfs.sh"), 3600),
                        Suite("layout", ("script", "test-product-layout.sh"), 3600),
                        Suite("threading", ("script", "test-threading.sh"), 1800),
                        Suite("copied-translations", ("script", "test-copied-translations.sh"), 3600),
                        Suite("endian", ("script", "test-endian.sh"), 1800),
                        Suite("varargs-span", ("script", "test-varargs-span.sh"), 1800),
                        Suite("native-layout", ("script", "test-layout-translated.sh"), 3600),
                        Suite("function-identity", ("script", "test-function-identity-aot.sh"), 3600,
                              scope="fixed NativeAOT function identity checks"),
                        Suite("image-exchange", ("script", "test-image-exchange.sh"), 3600),
                        Suite("source-preparation", ("python", "-B", "-m", "unittest", "discover", "-s", "{root}/tests/source_preparation"))),
                before_suite=before_suite, after_translate=bind_product, consumer=Consumer(property="SqliteProject"),
                default_suites=("consumer",),
                verify_suites=("source-preparation", "varargs-span", "consumer", "corpora", "native-layout",
                               "copied-translations", "endian", "host-vfs", "layout", "threading",
                               "function-identity", "image-exchange"))
