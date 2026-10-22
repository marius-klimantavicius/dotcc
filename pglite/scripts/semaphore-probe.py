"""Native/emitted unnamed-semaphore ABI and behavior using immutable tools."""
from pathlib import Path


def run(ctx, suite=None):
    from campaigns.recipes import link_flags
    work = ctx.work / "semaphore"
    work.mkdir()
    fixture = ctx.root / "tests/Semaphore/probe.c"
    native = work / "native"
    ctx.run(["cc", "-std=c11", "-Wall", "-Wextra", "-Werror",
             "-DDPG_SEMAPHORE_NATIVE", fixture, "-o", native], "semaphore-native-build")
    ctx.run([native], "semaphore-native-run")
    output = work / "fixture"
    ctx.run(["dotnet", ctx.compiler, fixture, "--emit=managedlib", "--state-context",
             *link_flags("SemaphoreProbe", "Managed.PGlite.Probes"), "-o", output], "semaphore-emission")
    project = ctx.root / "tests/Semaphore/Semaphore.csproj"
    setting = "-p:SemaphoreProject=" + str(output / "fixture.csproj")
    mode = getattr(ctx.options, "mode", "all")
    for flavor in (("jit", "aot") if mode == "all" else (mode,)):
        if flavor == "jit":
            ctx.managed("build", project, "semaphore-jit-build", setting, "-m:1",
                        "--artifacts-path", work / "jit")
            command = ["dotnet", work / "jit/bin/Semaphore/release/Semaphore.dll"]
        else:
            ctx.managed("publish", project, "semaphore-aot-build", setting, "-m:1",
                        "-r", "linux-x64", "-p:PublishAot=true", "--artifacts-path", work / "aot-artifacts",
                        "-o", work / "aot")
            command = [work / "aot/Semaphore"]
        ctx.run(command, "semaphore-" + flavor + "-run")
    ctx.receipt["pglite_semaphore"] = dict(status="passed", modes=mode,
        scope="real POSIX semaphore ABI/count/wait controls in two generated owners; SQL separate")
