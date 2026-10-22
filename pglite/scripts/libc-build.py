"""Compile the complete actual fork libc unit, without acquiring shared inputs."""
from campaigns.recipes import helper, link_flags


def run(ctx, suite=None):
    selection = next(ctx.recipe.sources(ctx.root))
    source = ctx.sources.get("product", selection.directory)
    missing = [name for name in selection.required if not (source / name).is_file()]
    if missing:
        raise RuntimeError("Supervisor must prepare the readonly product reference: " + ", ".join(missing))
    wrapper = helper(ctx.root / "scripts/libc-input.py").prepare_input(ctx, source)
    output = ctx.work / "full-pglitec"
    ctx.receipt["scope"] = "complete pglitec feasibility build only; no SQL core/product publication"
    ctx.run(["dotnet", ctx.compiler, "-std=c11", wrapper, "--emit=managedlib",
             *link_flags("FullLibcControl", "Managed.PGlite.Probes"), "-o", output],
            "full-pglitec-emission")
    ctx.managed("build", output / "full-pglitec.csproj", "full-pglitec-build", "-m:1",
                "--artifacts-path", ctx.work / "libc-managed")
    fixture = ctx.work / "bootstrap-fixture"
    ctx.run(["dotnet", ctx.compiler, ctx.root / "tests/Initialization/transport_fixture.c",
             "-std=c11", "--emit=managedlib",
             *link_flags("BootstrapFixture", "Managed.PGlite.Tests"), "-o", fixture],
            "initdb-transport-fixture-emission")
    tests = ctx.root / "tests/Initialization/ProcessServices.csproj"
    ctx.managed("build", tests, "full-pglitec-contracts-build", "-m:1",
                "-p:FullLibcProject=" + str(output / "full-pglitec.csproj"),
                "-p:BootstrapFixtureProject=" + str(fixture / "bootstrap-fixture.csproj"),
                "--artifacts-path", ctx.work / "libc-contracts")
    ctx.run(["dotnet", ctx.work / "libc-contracts/bin/ProcessServices/release/ProcessServices.dll"],
            "full-pglitec-contracts-run")
