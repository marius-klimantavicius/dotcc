"""MsQuic staged closure and explicit qualification gates."""
from pathlib import Path
from campaigns.model import Consumer, Recipe, Source, Suite, Translation, Unit
from campaigns.testing import forms
from campaigns.recipes import flags, json_file, lines, link_flags


def sources(root):
    pin = json_file(root / "config/source.json")
    yield Source("product", pin["url"], root / "ref" / pin["archive"], root / "ref" / pin["directory"],
                 pin["directory"], pin["sha256"], ("src/core/CMakeLists.txt", "src/inc/msquic.h"))


def gates(ctx):
    if not ctx.options.fast and ctx.options.action != "probe":
        ctx.script("test-host-contract.py", "--no-fetch", timeout=7200)
        ctx.script("test-abi.py", "--groups", "public", "--no-fetch", timeout=3600)


def prepare(ctx):
    ctx.script("generate-host-contract.py")
    ctx.script("stage-product.py", "--no-fetch")
    stage = ctx.root / "build/product-source"
    manifest = json_file(stage / "manifest.json")
    options = [*flags(manifest["defines"], [stage / name for name in ("system", "src/inc", "src/core", "src/platform", "host")]
                     + [ctx.root / "tests/Abi", ctx.root / "tests/HostContract"]),
               "--emit-define", "QUIC_STATUS_*", "--overrides-file", str(ctx.root / "config/dotcc-overrides.json")]
    exports = lines(ctx.root / "config/inline-exports.txt")
    if not exports or len(set(exports)) != len(exports):
        raise RuntimeError("Empty or duplicate inline export selectors")
    return Translation([Unit(stage / name) for name in manifest["units"]], options,
                       [*link_flags("MsQuic", "Managed.Transport"), "--deduplicate-inline",
                       *(part for name in exports for part in ("--export-inline", name))], objects=True)


def finish(ctx):
    if not ctx.options.fast and ctx.options.action != "probe":
        ctx.script("build-product.py", "--existing", label="product-boundary", timeout=7200)
        closure = ctx.artifacts / "qualified-closure.json"
        ctx.script("freeze-product.py", "--without-sqlite", "--output", closure,
                   label="freeze-closure", timeout=1800)
        ctx.receipt["qualification"] = str(closure)


names = ("platform-host", "packet-crypto", "tls-adapter", "datapath-host", "managed-peer",
         "managed-api", "public-consumer", "managed-net-quic", "malformed-corpus", "endpoint-controls",
         "injected-host", "managed-independent", "cid-rotation", "recovery")
def script_arguments(ctx, suite, script, args):
    if ctx.options.mode == "jit" and script != "test-packet-crypto.py":
        args.append("--jit-only")
    if script not in ("test-packet-crypto.py", "test-managed-net-quic.py"):
        args += ["--variants", *("raw" if form == "raw" else "optimized" for form in forms(ctx))]
    return args


def validate_options(args):
    if args.action in ("translate", "verify") and not args.fast and args.form != "all":
        raise ValueError("MsQuic qualification needs both forms; use --form all or translate --fast")


recipe = Recipe("msquic", "TranslatedMsQuic", ("default",),
                ("src/BclHost/BclHost.csproj", "src/ManagedApi/ManagedApi.csproj", "samples/ManagedConsumer/ManagedConsumer.csproj"),
                sources, prepare,
                script_arguments=script_arguments, suites=(Suite("consumer", ("consumer",), matrix=True),
                        *(Suite(name, (("script", "test-managed-independent.py", "--rotate-cid", "--output",
                                       str(Path(__file__).resolve().parents[1] / 'artifacts/cid-rotation'))
                                      if name == "cid-rotation" else
                                      ("script", "test-" + name + ".py", *(("--all",) if name == "managed-api" else ()))),
                                14400, platforms=("linux",),
                                dependencies=("picotls",) if name in ("tls-adapter", "managed-peer", "managed-independent") else ()) for name in names)),
                dependencies=("picotls",), supports_fast=True, validate_options=validate_options,
                consumer=Consumer(property="MsQuicProject", arguments=("certificates", "{work}/{label}-certificates"),
                                  build_arguments=("-m:1", "-p:BuildInParallel=false")),
                default_suites=("consumer",), verify_suites=names, before_translate=gates, after_translate=finish)
