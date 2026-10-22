"""Real native/emitted boundary controls; never builds the shared compiler.

Framework adapter: run(ctx, suite=None). DOTCC_COMPILER must select an existing
immutable tool closure for standalone test runs (translate/verify may supply
ctx.compiler). Native tools and all outputs belong only to this experiment.
"""
import json
import os
from pathlib import Path
import shutil
import sys


def _write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n")


def _region(text, begin, end):
    if text.count(begin) != 1 or text.count(end) != 1:
        raise RuntimeError("Selected fork changed diagnostic slice context: " + begin)
    start, stop = text.index(begin), text.index(end)
    if stop <= start:
        raise RuntimeError("Invalid diagnostic slice order: " + begin)
    return text[start:stop]


def diagnostic_slices(source):
    """Unmodified function text only; no claim to compile the complete TU."""
    text = Path(source).read_text()
    return "\n".join([
        "/* Diagnostic slices of selected pglitec.c; not a delivered product. */",
        "#include <stddef.h>\n#include <sys/types.h>\n#define EMSCRIPTEN_KEEPALIVE",
        _region(text, "#define MAX_ATEXIT_FUNCS 32", "/* ========== streams functions"),
        _region(text, "typedef ssize_t (*pgl_read_t)", "int EMSCRIPTEN_KEEPALIVE pgl_fcntl"),
        _region(text, "ssize_t EMSCRIPTEN_KEEPALIVE pgl_recv", "int EMSCRIPTEN_KEEPALIVE pgl_connect"),
    ])


def _postgres_layout(ctx, compiler, fixtures, work, artifacts, report):
    info = ctx.receipt.get("pglite_preparation")
    selected = ctx.env.get("PGLITE_BOUNDARY_PREPARATION")
    if selected:
        info = json.loads(Path(selected).read_text())
    if not info:
        report["blockers"].append("Actual PostgreSQL layout needs configured headers: supply PGLITE_BOUNDARY_PREPARATION or prepare the campaign first")
        return
    include = Path(info["source"]) / "src/include"
    if not (include / "pg_config.h").is_file():
        raise RuntimeError("Missing configured PostgreSQL header: " + str(include / "pg_config.h"))
    report["postgres_preparation"] = info
    cc = os.environ.get("CC", "cc")
    flags = ["-D_GNU_SOURCE", "-D__PGLITE__", "-DDPG_NATIVE_MAIN", "-I", include]
    executable = work / "native-postgres-layout"
    ctx.run([cc, "-std=c11", *flags, fixtures / "postgres_layout.c", "-o", executable], "boundary-postgres-native-build")
    native = json.loads(ctx.run([executable], "boundary-postgres-native-run"))
    _write(artifacts / "postgres-native.json", native)
    report["passed"].append("actual configured PostgreSQL-header native layouts")
    emitted = work / "postgres-emitted"
    ctx.run(["dotnet", compiler, fixtures / "postgres_layout.c", *flags, "--runtime=c", "--emit=csproj", "-o", emitted],
            "boundary-postgres-emission", check=False)
    if ctx.receipt["commands"][-1].get("exit_code") != 0:
        report["blockers"].append("Actual PostgreSQL-header emission failed; see boundary-postgres-emission.log")
        return
    ctx.run(["dotnet", "build", emitted / "postgres-emitted.csproj", "-c", "Release", "--nologo", "-m:1",
             "--artifacts-path", work / "postgres-managed"], "boundary-postgres-managed-build")
    got = json.loads(ctx.run(["dotnet", work / "postgres-managed/bin/postgres-emitted/release/postgres-emitted.dll"],
                            "boundary-postgres-managed-run"))
    _write(artifacts / "postgres-managed.json", got)
    if got != native:
        raise RuntimeError("Actual PostgreSQL native/emitted type layout mismatch")
    report["unrun"].remove("actual PostgreSQL native/emitted layout match")
    report["passed"].append("actual PostgreSQL native/emitted layout match")


def run(ctx, suite=None):
    work = ctx.work / "boundary"
    artifacts = ctx.artifacts / "boundary"
    work.mkdir(parents=True, exist_ok=True)
    artifacts.mkdir(parents=True, exist_ok=True)
    report = {"scope": "native/emitted boundary experiments; no SQL product qualification",
              "passed": [], "blockers": [], "unrun": [
                  "actual PostgreSQL native/emitted layout match", "complete fork embedding initialization",
                  "translated SQL and error recovery", "shutdown/reopen persistence",
                  "raw/processed JIT/AOT product matrix"]}
    _write(artifacts / "report.json", report)
    try:
        if sys.platform != "linux":
            raise RuntimeError("Boundary native controls currently require Linux x64 C tools; other ABI profiles are unqualified.")
        pin = json.loads((ctx.root / "config/sources.json").read_text())["product"]
        source_root = ctx.sources.get("product", ctx.root / "ref" / pin["directory"])
        source = source_root / "pglite/src/pglitec/pglitec.c"
        if not source.is_file():
            raise RuntimeError("Missing selected fork source: " + str(source))
        report["source"] = str(source)
        report["requested_revision"] = pin["revision"]
        cc = os.environ.get("CC", "cc")
        if not shutil.which(cc):
            raise RuntimeError("Boundary native controls require a C compiler (CC or cc).")
        fixtures = ctx.root / "tests/Boundary"
        native = work / ("native-abi.exe" if sys.platform == "win32" else "native-abi")
        ctx.run([cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-DDPG_NATIVE_MAIN",
                 fixtures / "abi_fixture.c", "-o", native], "boundary-native-build")
        output = ctx.run([native], "boundary-native-run")
        native_report = json.loads(output)
        _write(artifacts / "native.json", native_report)
        report["passed"].append("minimal native LP64 layout, allocation, nested jumps, callback widths")
        upstream = work / ("native-pglitec.exe" if sys.platform == "win32" else "native-pglitec")
        ctx.run([cc, "-std=gnu11", f'-DDPG_PGLITEC_SOURCE="{source}"',
                 fixtures / "pglitec_native.c", "-o", upstream], "boundary-fork-native-build")
        ctx.run([upstream], "boundary-fork-native-run")
        report["passed"].append("actual fork native callbacks, atexit LIFO/reset, shared-memory lifecycle")
        ctx.run([upstream, "--top-jump"], "boundary-fork-native-top-jump", check=False)
        status = ctx.receipt["commands"][-1].get("exit_code")
        if status != 100:
            raise RuntimeError(f"Actual fork top jump must exit 100, observed {status!r}")
        report["passed"].append("actual fork native top-level jump uses synthetic exit 100")
        compiler = getattr(ctx, "compiler", None) or ctx.env.get("DOTCC_COMPILER")
        if not compiler or not Path(compiler).is_file():
            raise RuntimeError("Boundary emission needs an existing immutable DOTCC_COMPILER; shared compiler builds are not performed by this suite.")
        compiler = Path(compiler).resolve()
        report["compiler"] = str(compiler)
        _postgres_layout(ctx, compiler, fixtures, work, artifacts, report)
        sliced = work / "pglitec_slices.c"
        sliced.write_text(diagnostic_slices(source))
        amalgam = work / "boundary.c"
        # Portable includes; no symlinks or changes to acquired references.
        amalgam.write_text('#include "pglitec_slices.c"\n#include "abi_fixture.c"\n')
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, amalgam, "-I", fixtures, "--emit=managedlib", "--runtime=c",
                 "--nest-types", "--class-name", "Boundary", "--namespace", "Managed.PGlite.Probes",
                 "--split=size", "--split-size=262144", "--literal-pool", "--deduplicate-inline", "-o", emitted],
                "boundary-emission")
        ctx.run(["dotnet", "build", fixtures / "Boundary.csproj", "-c", "Release", "--nologo", "-m:1",
                 f"-p:BoundaryProject={emitted / 'emitted.csproj'}", "--artifacts-path", work / "managed"],
                "boundary-managed-build")
        executable = work / "managed/bin/Boundary/release/Boundary.dll"
        ctx.run(["dotnet", executable, artifacts / "native.json", artifacts / "managed.json"],
                "boundary-managed-run")
        report["passed"].append("actual emitted fixture and selected unmodified fork callback/atexit function bodies")
        # This probe must remain a required full-TU gate. Record the failure;
        # successful reduced fixtures cannot qualify this source unit.
        full_input = work / "pglitec-input.c"
        full_input.write_text('#include <stdbool.h>\n#include <stdlib.h>\n#include "' + source.as_posix() + '"\n')
        ctx.run(["dotnet", compiler, full_input, "--emit=obj", "-o", work / "pglitec.cs"],
                "boundary-fork-full-emission", check=False)
        if ctx.receipt["commands"][-1].get("exit_code") != 0:
            report["blockers"].append("Complete selected pglitec.c emission failed; see boundary-fork-full-emission.log")
        jump = work / "sigjmp.c"
        jump.write_text('#include <setjmp.h>\nint probe(void) { sigjmp_buf b; int status = sigsetjmp(b, 1); '
                        'if (status == 0) siglongjmp(b, 7); return status; }\n')
        ctx.run(["dotnet", compiler, jump, "--emit=obj", "-o", work / "sigjmp.cs"],
                "boundary-sigjmp-emission", check=False)
        if ctx.receipt["commands"][-1].get("exit_code") != 0:
            report["blockers"].append("sigjmp_buf/sigsetjmp boundary is unavailable in the selected tool profile")
        ctx.run(["dotnet", compiler, fixtures / "exit_fixture.c", "--overrides-file", fixtures / "exit-overrides.json",
                 "--override-report", artifacts / "exit-overrides.jsonl", "--emit=obj", "-o", work / "exit-override.cs"],
                "boundary-exit-override", check=False)
        if ctx.receipt["commands"][-1].get("exit_code") != 0:
            report["blockers"].append("Typed exit(int) override is rejected; see boundary-exit-override.log")
        managed_report = json.loads((artifacts / "managed.json").read_text())
        if managed_report.get("yield_records_terminal"):
            report["blockers"].append("PGlite synthetic exit 99 permanently records runtime termination")
        if not managed_report.get("timers_supported"):
            report["blockers"].append("setitimer is unsupported, including the fork's clear_setitimer shutdown call")
        if not managed_report.get("signal_query_supported"):
            report["blockers"].append("sigaction reports success without writing the requested previous action")
        if report["blockers"]:
            raise RuntimeError("PGlite boundary feasibility remains blocked: " + "; ".join(report["blockers"]))
        report["status"] = "passed-boundary-only"
    except BaseException as error:
        report["status"] = "blocked" if report["blockers"] else "failed"
        report["error"] = str(error)
        raise
    finally:
        _write(artifacts / "report.json", report)
