"""Compile the complete actual fork libc unit as a private feasibility library."""
from campaigns.inputs import acquire
from campaigns.recipes import link_flags


def run(ctx, suite=None):
    source = acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
    wrapper = ctx.work / "full-pglitec.c"
    wrapper.write_text('#include <stdbool.h>\n#include <stdlib.h>\n#include "'
                       + (source / 'pglite/src/pglitec/pglitec.c').as_posix() + '"\n')
    output = ctx.work / "full-pglitec"
    ctx.receipt["scope"] = "complete pglitec feasibility build only; no SQL core/product publication"
    ctx.run(["dotnet", ctx.compiler, "-std=c11", wrapper, "--emit=managedlib",
             *link_flags("FullLibcControl", "Managed.PGlite.Probes"), "-o", output],
            "full-pglitec-emission")
    ctx.managed("build", output / "full-pglitec.csproj", "full-pglitec-build")
