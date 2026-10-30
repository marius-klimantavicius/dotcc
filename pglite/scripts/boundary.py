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


def prepare_signal_overlay(include, workspace):
    """Adapt only the caller's private copied headers, after atomic/CPU ports.

    The original interrupt check and ProcessInterrupts call remain byte-for-byte.
    No acquired/configured source directory may be passed as the destination.
    """
    include, workspace = Path(include).resolve(), Path(workspace).resolve()
    project = Path(__file__).resolve().parents[1]
    if not workspace.is_relative_to(project / "build") or workspace.is_relative_to(project / "build/native-control"):
        raise RuntimeError("Signal overlay requires a private campaign workspace under pglite/build")
    if not include.is_relative_to(workspace):
        raise RuntimeError("Signal overlay must be inside its private workspace")
    header = include / "miscadmin.h"
    if not header.resolve().is_relative_to(include):
        raise RuntimeError("Signal overlay header must not reference a file outside the private include copy")
    text = header.read_text()
    original = "#define CHECK_FOR_INTERRUPTS() \\\ndo { \\\n\tif (INTERRUPTS_PENDING_CONDITION()) \\\n\t\tProcessInterrupts(); \\\n} while(0)"
    if text.count(original) != 1 or "dpg_host_poll_signals" in text:
        raise RuntimeError("Selected miscadmin.h CHECK_FOR_INTERRUPTS context changed or was already adapted")
    adapted = ("extern void dpg_host_poll_signals(void);\n" + original.replace(
        "do { \\\n", "do { \\\n\tdpg_host_poll_signals(); \\\n", 1))
    header.write_text(text.replace(original, adapted, 1))
    return {"header": str(header), "hook": "dpg_host_poll_signals",
            "override": str(project / "src/Host/signal-overrides.json"),
            "original_interrupt_logic_preserved": True}


def diagnostic_slices(source):
    """Unmodified function text only; no claim to compile the complete TU."""
    text = Path(source).read_text()
    return "\n".join([
        "/* Diagnostic slices of selected pglitec.c; not a delivered product. */",
        "#include <stddef.h>\n#include <sys/types.h>\n#include <stdbool.h>\n#include <stdlib.h>\n#include <setjmp.h>\n#include <sys/time.h>\n#define EMSCRIPTEN_KEEPALIVE",
        _region(text, "volatile int is_pglite_active = 0;", "/* ========== Process handling functions"),
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


def _sigjmp_masks(ctx, compiler, fixtures, work, report):
    native = work / "native-sigjmp-mask"
    ctx.run([os.environ.get("CC", "cc"), "-std=c11", "-DDPG_NATIVE_MAIN",
             fixtures / "sigjmp_mask.c", "-o", native], "boundary-sigjmp-mask-native-build")
    ctx.run([native], "boundary-sigjmp-mask-native-run")
    report["passed"].append("native lexical sigsetjmp mask-save and no-save contracts")
    emitted = work / "sigjmp-mask-emitted"
    ctx.run(["dotnet", compiler, fixtures / "sigjmp_mask.c", "--emit=managedlib", "--runtime=c",
             "--nest-types", "--state-context", "--class-name", "Sigjmp", "--namespace", "Managed.PGlite.Probes",
             "-o", emitted], "boundary-sigjmp-mask-emission", check=False)
    if ctx.receipt["commands"][-1].get("exit_code") != 0:
        report["blockers"].append("Actual lexical sigsetjmp mask probe does not emit")
        return
    ctx.run(["dotnet", "build", fixtures / "Sigjmp.csproj", "-c", "Release", "--nologo", "-m:1",
             f"-p:BoundaryProject={emitted / 'sigjmp-mask-emitted.csproj'}",
             "--artifacts-path", work / "sigjmp-mask-managed"], "boundary-sigjmp-mask-managed-build", check=False)
    if ctx.receipt["commands"][-1].get("exit_code") != 0:
        report["blockers"].append("Actual lexical sigsetjmp/siglongjmp emitted C# does not build; see boundary-sigjmp-mask-managed-build.log")
        return
    ctx.run(["dotnet", work / "sigjmp-mask-managed/bin/Sigjmp/release/Sigjmp.dll"],
            "boundary-sigjmp-mask-managed-run", check=False)
    if ctx.receipt["commands"][-1].get("exit_code") != 0:
        report["blockers"].append("sigsetjmp(save_mask=1)/siglongjmp does not restore the guest signal mask")
    else:
        report["passed"].append("emitted lexical sigsetjmp mask-save and no-save contracts")


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
        compiler = ctx.env.get("DOTCC_COMPILER") or getattr(ctx, "compiler", None)
        if not compiler or not Path(compiler).is_file():
            raise RuntimeError("Boundary emission needs an existing immutable DOTCC_COMPILER; shared compiler builds are not performed by this suite.")
        compiler = Path(compiler).resolve()
        report["compiler"] = str(compiler)
        _postgres_layout(ctx, compiler, fixtures, work, artifacts, report)
        sliced = work / "pglitec_slices.c"
        sliced.write_text(diagnostic_slices(source))
        # Exercise the exact real-source macro adaptation independently of a
        # complete PostgreSQL backend. Only its ProcessInterrupts callee is a
        # diagnostic event observer; no core algorithm is supplied here.
        preparation = report.get("postgres_preparation")
        if not preparation:
            raise RuntimeError("Safe-point probe needs configured miscadmin.h")
        signal_include = work / "signal-include"
        signal_include.mkdir(exist_ok=True)
        shutil.copyfile(Path(preparation["source"]) / "src/include/miscadmin.h", signal_include / "miscadmin.h")
        report["signal_overlay"] = prepare_signal_overlay(signal_include, ctx.work)
        (work / "dpg_safe_point.h").write_text(_region(
            (signal_include / "miscadmin.h").read_text(), "extern void dpg_host_poll_signals(void);",
            "/* Is ProcessInterrupts()"))
        native_safe = work / "native-safe-point"
        ctx.run([cc, "-std=c11", "-DDPG_NATIVE_MAIN", "-I", work,
                 fixtures / "safe_point_fixture.c", "-o", native_safe], "boundary-safe-point-native-build")
        ctx.run([native_safe], "boundary-safe-point-native-run")
        report["passed"].append("native timer and guarded real-source CHECK_FOR_INTERRUPTS adaptation")
        amalgam = work / "boundary.c"
        # Portable includes; no symlinks or changes to acquired references.
        amalgam.write_text('#include "pglitec_slices.c"\n#define longjmp pgl_longjmp\n#include "abi_fixture.c"\n'
                           'void dpg_fixture_top_error(void) { pgl_longjmp((void*)postgresmain_sigjmp_buf, 7); }\n'
                           '#include "safe_point_fixture.c"\n')
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, amalgam, "-I", fixtures, "-I", work, "--emit=managedlib", "--runtime=c",
                 "--nest-types", "--state-context", "--class-name", "PGlite", "--namespace", "Managed.PGlite.Translated",
                 "--overrides-file", ctx.root / "src/Host/signal-overrides.json",
                 "--override-report", artifacts / "signal-overrides.jsonl",
                 "--split=size", "--split-size=262144", "--literal-pool", "--deduplicate-inline", "-o", emitted],
                "boundary-emission")
        ctx.run(["dotnet", "build", fixtures / "Boundary.csproj", "-c", "Release", "--nologo", "-m:1",
                 f"-p:BoundaryProject={fixtures / 'BoundaryProduct.csproj'}", f"-p:BoundarySource={emitted}",
                 "--artifacts-path", work / "managed"],
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
            report["unsupported_alternative"] = "Typed exit(int) override remains rejected; scoped runtime interception is used instead"
        managed_report = json.loads((artifacts / "managed.json").read_text())
        if managed_report.get("timers_supported"):
            report["passed"].append("cooperative owned ITIMER_REAL delivery and actual fork cancellation")
            report["passed"].append("timer delivered inside emitted real-source CHECK_FOR_INTERRUPTS loop before returning to host")
        else:
            report["blockers"].append("Selected compiler snapshot still routes public setitimer to ENOTSUP; supervisor must route it to GuestSetitimer and rebuild")
        report["passed"].extend([
            "repeatable owner-aware 99/100 runtime yields and real fork pgl_longjmp",
            "sigaction oldact; guest mask mechanics covered by separate runtime regressions",
            "fresh generated state contexts preserve initializers across same-process owner reopen",
            "real terminal exit followed by actual fork atexit cleanup retains callback ownership"])
        report["unrun"].append("complete backend startup/query timer-path coverage")
        report["excluded"] = ["cancellation while blocked inside a query; QUALIFICATION.md"]
        _sigjmp_masks(ctx, compiler, fixtures, work, report)
        if report["blockers"]:
            raise RuntimeError("PGlite boundary feasibility remains blocked: " + "; ".join(report["blockers"]))
        report["status"] = "passed-boundary-only"
    except BaseException as error:
        report["status"] = "blocked" if report["blockers"] else "failed"
        report["error"] = str(error)
        raise
    finally:
        _write(artifacts / "report.json", report)
