"""Native and actual emitted-JIT qualification of the read-only fixture service."""
import json
import os
import shlex
import shutil


def run(ctx, suite):
    cc = shlex.split(os.environ.get("CC", "cc"))
    if not cc or not shutil.which(cc[0]):
        raise RuntimeError("The fixture I/O control requires a GNU-compatible host C compiler")
    fixture = ctx.work / "host-io-fixture"
    (fixture / "nested").mkdir(parents=True)
    (fixture / "bytes.bin").write_bytes(bytes([0, 1, 127, 128, 255, 65, 66, 0, 10]))
    (fixture / "nested/data.txt").write_bytes(b"nested")
    sources = [ctx.root / "tests/upstream" / name for name in ("host_io.c", "host_io_control.c")]
    expected = (ctx.root / "tests/upstream/host_io_expected.txt").read_text(encoding="utf-8")
    native = ctx.work / ("host-io-native" + (".exe" if os.name == "nt" else ""))
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", *sources, "-o", native],
            "host-io-native-compile")
    observed = ctx.run([native, fixture], "host-io-native-run")
    if observed != expected:
        raise RuntimeError("Native fixture I/O assertion transcript differs")
    if not hasattr(ctx, "compiler"):
        ctx.tools()
    output = ctx.work / "host-io-managed"
    ctx.run(["dotnet", ctx.compiler, *sources, "--emit=csproj", "-o", output], "host-io-emit")
    project = output / "host-io-managed.csproj"
    ctx.managed("build", project, "host-io-managed-build", "-m:1")
    observed = ctx.run(["dotnet", output / "bin/Release/net10.0/host-io-managed.dll", fixture],
                       "host-io-managed-run")
    if observed != expected:
        raise RuntimeError("Actual emitted-JIT fixture I/O assertions differ from native")
    path = ctx.artifacts / "host-io.json"
    path.write_text(json.dumps({"native": "passed", "emitted_jit": "passed",
                               "live_handles": 0, "scope": "read-only corpus service; full core matrix separate",
                               "secure_entropy": "native getrandom(flags=0) / shared BCL RandomNumberGenerator.Fill",
                               "sources": list(map(str, sources))}, indent=2) + "\n", encoding="utf-8")
    ctx.receipt["host_io_report"] = str(path)
