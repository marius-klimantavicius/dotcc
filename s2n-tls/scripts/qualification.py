"""Connect shared campaign selection to specialist acceptance runners."""
import sys
from campaigns.recipes import helper


def run(ctx, suite):
    if suite.name == "native":
        ctx.run([sys.executable, ctx.root / "scripts/native-control.py", "--fetch", "never"], "native", timeout=7200)
        return
    if suite.name in ("consumer", "compatibility", "interop", "lifecycle", "upstream", "delivery", "rooted-aot"):
        ctx.run([sys.executable, ctx.root / "scripts/managed-qualification.py", "--suite", suite.name,
                 "--form", ctx.options.form, "--mode", ctx.options.mode, "--rid", ctx.options.rid,
                 "--restore", ctx.options.restore, "--output", ctx.artifacts / suite.name],
                suite.name, timeout=14400)
        return
    if suite.name == "provider":
        ctx.run([sys.executable, ctx.root / "tests/Provider/run.py", "--fetch", "never",
                 "--hashes", ctx.options.hashes, "--mode", ctx.options.mode,
                 "--artifacts", ctx.artifacts / "provider"], "provider-primitives", timeout=3600)
        # Primitive qualification alone does not exercise the translated upstream
        # hash/HMAC/HKDF/PRF/cipher callers. The applicable original-C cases must
        # also run; their missing executable inventory is a failure.
        ctx.run([sys.executable, ctx.root / "scripts/managed-qualification.py", "--suite", "upstream",
                 "--form", ctx.options.form, "--mode", ctx.options.mode, "--rid", ctx.options.rid,
                 "--restore", ctx.options.restore, "--output", ctx.artifacts / "provider-upstream"],
                "provider-upstream", timeout=14400)
        return
    if suite.name == "abi":
        ctx.tools()
        for script, label in (("tests/CoreAbi/run.py", "core-abi"),
                              ("tests/Abi/run-provider-probe.py", "provider-abi")):
            ctx.run([sys.executable, ctx.root / script, "--compiler", ctx.compiler,
                     "--mode", ctx.options.mode], label, timeout=3600)
        helper(ctx.root / "tests/CoreAbi/product.py").run(ctx, suite)
        return
    if suite.name == "reproduction":
        return helper(ctx.root / "scripts/reproduction.py").run(ctx, suite)
    if suite.name == "shared-regressions":
        return helper(ctx.root / "scripts/shared-regressions.py").run(ctx, suite)
    raise RuntimeError("Required s2n-tls suite has not passed implementation: " + suite.name)
