#!/usr/bin/env python3
"""Check authored native identity/binding; do not claim translated backend dispatch."""
import hashlib
import os
import sys


def run(ctx):
    project = ctx.root / "tests/dispatch/Dispatch.csproj"
    build = ctx.root / "build/dispatch"
    modes = ("jit", "aot") if ctx.options.mode == "all" else (ctx.options.mode or "jit",)
    results = []
    preload = None
    if sys.platform.startswith("linux"):
        preload = ctx.work / "interpose-read.so"
        ctx.run(["cc", "-shared", "-fPIC", ctx.root / "tests/dispatch/interpose-read.c",
                 "-o", preload], "dispatch-interposer-build")
    for mode in modes:
        if mode == "jit":
            ctx.managed("build", project, "dispatch-jit-build", "--artifacts-path", build)
            binary = build / "bin/Dispatch/release/Dispatch.dll"
            command = ["dotnet", binary]
        else:
            output = build / "aot" / ctx.options.rid
            ctx.managed("publish", project, "dispatch-aot-build", "--artifacts-path", build,
                        "-r", ctx.options.rid, "-p:PublishAot=true", "-o", output, timeout=900)
            binary = output / ("Dispatch.exe" if os.name == "nt" else "Dispatch")
            command = [binary]
        transcript = ctx.run(command, "dispatch-" + mode, timeout=30)
        if "dispatch-foundation=passed" not in transcript:
            raise RuntimeError("Native identity/binding success record missing")
        ambiguous = "unrun; requires Linux LD_PRELOAD"
        if preload:
            value = str(preload) + (":" + os.environ["LD_PRELOAD"] if os.environ.get("LD_PRELOAD") else "")
            ctx.run([*command, "--expect-ambiguous"], "dispatch-" + mode + "-interposed", timeout=30,
                    env={"LD_PRELOAD": value})
            ambiguous = "passed: actual interposed read provider rejected"
        results.append({"mode": mode, "status": "passed", "rid": ctx.options.rid,
                        "binary": str(binary), "sha256": hashlib.sha256(binary.read_bytes()).hexdigest(),
                        "provider": transcript.strip(), "ambiguous_provider": ambiguous})
    ctx.receipt["dispatch_foundation"] = {"scope": "authored native host only; translated backend selection unimplemented",
                                          "forms": "authored source, no raw/processed translation forms", "results": results}
    ctx.save()
