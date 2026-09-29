#!/usr/bin/env python3
"""Native-control suite, executed through shared campaign logging/process support."""
from pathlib import Path
from campaigns.compat import references


def run(ctx):
    source = references(ctx.root, ctx.options.fetch == "never")["product"]
    build = ctx.root / "build/native-baseline"
    ctx.run(["cmake", "-S", ctx.root / "tests/native", "-B", build,
             "-DLIBUV_SOURCE=" + str(source), "-DCMAKE_BUILD_TYPE=Release",
             "-DCMAKE_EXPORT_COMPILE_COMMANDS=ON"], "native-configure")
    ctx.run(["cmake", "--build", build, "--config", "Release", "-j", str(ctx.options.jobs)],
            "native-build", timeout=900)
    suffix = ".exe" if __import__("os").name == "nt" else ""
    candidates = [build / ("libuv-native-workflow" + suffix), build / "Release" / ("libuv-native-workflow" + suffix)]
    binary = next((path for path in candidates if path.is_file()), None)
    if binary is None:
        raise RuntimeError("Native workflow executable missing")
    ctx.run([binary], "native-workflow", timeout=30, cwd=ctx.work)
    layout = build / "libuv-native-layout"
    if layout.is_file():
        output = ctx.run([layout], "native-layout", timeout=30)
        measurements = dict(line.split("=", 1) for line in output.splitlines() if "=" in line)
        from campaigns.identity import write_json
        write_json(ctx.artifacts / "native-layout.json",
                   {"scope": "native-only; emitted comparison pending",
                    "measurements": {key: int(value) for key, value in measurements.items()}})
    ctx.receipt["native_baseline"] = {"source": str(source), "workflow": "passed",
                                      "managed_qualification": False}
