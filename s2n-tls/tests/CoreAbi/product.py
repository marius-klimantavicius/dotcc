"""Exercise canonical product layouts/address identity, not a separate emitter probe."""
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def run(ctx, suite, native=None):
    project = ctx.root / "tests/CoreAbi/Product/ProductAbi.csproj"
    native = native or ctx.root / "artifacts/core-abi/native.log"
    if not native.is_file():
        raise RuntimeError("Matching native layout observations missing")
    for form in forms(ctx):
        product = check_product(ctx, form)
        for mode in modes(ctx):
            label = "product-abi-" + form + "-" + mode
            output = ctx.work / label
            properties = ["-p:S2nTlsProject=" + str(product), "--artifacts-path", output / "artifacts", "-m:1"]
            if mode == "jit":
                ctx.managed("build", project, label + "-build", *properties, timeout=1200)
                command = ["dotnet", output / "artifacts/bin/ProductAbi/release/ProductAbi.dll"]
            else:
                ctx.managed("publish", project, label + "-build", *properties, "-r", ctx.options.rid,
                            "-p:PublishAot=true", "-o", output / "publish", timeout=1800)
                command = [output / "publish/ProductAbi"]
            ctx.run([*command, native, ctx.artifacts / (label + ".json")], label + "-run", timeout=120)
