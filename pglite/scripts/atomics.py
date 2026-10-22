"""Actual-PostgreSQL C11 atomic/spinlock ports; framework run(ctx, suite).

No preparation of the native-control stage, source fetching, shared tool build,
product publication, or standalone framework runner. The parent recipe consumes
prepare_overlay()'s ordered include/define contract.
"""
import hashlib
import json
import os
from pathlib import Path
import shutil


PORT_DEFINE = "PGLITE_DOTCC_C11_ATOMICS"
SPINLOCK_DEFINE = "PGLITE_DOTCC_C11_SPINLOCKS"


def _write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n")


def _replace(text, before, after):
    if text.count(before) != 1:
        raise RuntimeError("PostgreSQL atomics port selection context changed: " + before)
    return text.replace(before, after, 1)


def prepare_overlay(source, destination):
    """Copy configured PG headers, inserting guarded atomic/spinlock selections.

    `source` is the already configured source root, never a writable target.
    Return an include directory to place BEFORE the original src/include plus
    the explicit port define. Copy the entire include closure because quoted
    includes in original c.h/postgres.h would otherwise bypass a partial overlay.
    `destination` must be a fresh private directory (typically ctx.work/atomics).
    All upstream generic wrappers/contention algorithms remain unchanged.
    """
    source, destination = Path(source).resolve(), Path(destination).resolve()
    if destination == source or source in destination.parents or destination in source.parents:
        raise RuntimeError("Atomics overlay must be independent of the configured source")
    original = source / "src/include"
    for name in ("pg_config.h", "c.h", "port/atomics.h", "port/atomics/generic.h", "storage/s_lock.h"):
        if not (original / name).is_file():
            raise RuntimeError("Missing configured PostgreSQL header: " + str(original / name))
    text = (original / "port/atomics.h").read_text()
    arch = '#if defined(__arm__) || defined(__arm) || defined(__aarch64__)\n#include "port/atomics/arch-arm.h"'
    text = _replace(text, arch,
                    '#if defined(' + PORT_DEFINE + ')\n#include "port/atomics/generic-dotcc-c11.h"\n'
                    '#elif defined(__arm__) || defined(__arm) || defined(__aarch64__)\n'
                    '#include "port/atomics/arch-arm.h"')
    compiler = '#if defined(__GNUC__) || defined(__INTEL_COMPILER)\n#include "port/atomics/generic-gcc.h"'
    text = _replace(text, compiler,
                    '#if defined(' + PORT_DEFINE + ')\n/* C11 compiler port selected above. */\n'
                    '#elif defined(__GNUC__) || defined(__INTEL_COMPILER)\n'
                    '#include "port/atomics/generic-gcc.h"')
    spinlock = (original / "storage/s_lock.h").read_text()
    spinlock = _replace(spinlock, '#if defined(__GNUC__) || defined(__INTEL_COMPILER)\n',
                        '#if defined(' + SPINLOCK_DEFINE + ')\n'
                        '#include "storage/s_lock-dotcc-c11.h"\n'
                        '#elif defined(__GNUC__) || defined(__INTEL_COMPILER)\n')
    ports = Path(__file__).resolve().parents[1] / "src/Atomics"
    port = ports / "generic-dotcc-c11.h"
    spin_port = ports / "s_lock-dotcc-c11.h"
    # Check every context and authored input before creating the destination.
    for path in (port, spin_port):
        if not path.is_file():
            raise RuntimeError("Missing authored compiler port: " + str(path))
    shutil.copytree(original, destination, symlinks=False)
    (destination / "port/atomics.h").write_text(text)
    (destination / "storage/s_lock.h").write_text(spinlock)
    shutil.copy2(port, destination / "port/atomics/generic-dotcc-c11.h")
    shutil.copy2(spin_port, destination / "storage/s_lock-dotcc-c11.h")
    # The selections are the only modified upstream headers. Record original and
    # staged hashes for parent review; hashes here are evidence, not pin policy.
    manifest = {
        "source": str(source), "include": str(destination), "defines": [PORT_DEFINE, SPINLOCK_DEFINE],
        "original_atomics_sha256": hashlib.sha256((original / "port/atomics.h").read_bytes()).hexdigest(),
        "staged_atomics_sha256": hashlib.sha256(text.encode()).hexdigest(),
        "port_sha256": hashlib.sha256(port.read_bytes()).hexdigest(),
        "original_spinlock_sha256": hashlib.sha256((original / "storage/s_lock.h").read_bytes()).hexdigest(),
        "staged_spinlock_sha256": hashlib.sha256(spinlock.encode()).hexdigest(),
        "spinlock_port_sha256": hashlib.sha256(spin_port.read_bytes()).hexdigest(),
        "source_mutated": False, "selection_hunks": 3,
        "spinlock_generic_unchanged": (original / "storage/s_lock.h").read_text().split(
            "/* Blow up if we didn't have any way to do spinlocks */", 1)[1]
            == spinlock.split("/* Blow up if we didn't have any way to do spinlocks */", 1)[1],
        "generic_wrappers_unchanged": (original / "port/atomics/generic.h").read_bytes()
                                    == (destination / "port/atomics/generic.h").read_bytes(),
    }
    return manifest


def _configured_source(ctx, pin):
    selected = os.environ.get("PGLITE_ATOMICS_SOURCE")
    if selected:
        return Path(selected).resolve()
    prepared = ctx.receipt.get("pglite_preparation", {})
    if prepared.get("source"):
        return Path(prepared["source"]).resolve()
    candidates = []
    for receipt in (ctx.root / "build/native-control").glob("*/prepared.json"):
        info = json.loads(receipt.read_text())
        if info.get("requested_revision") == pin["revision"] and info.get("profile") == "embedding-native":
            candidates.append((receipt.stat().st_mtime_ns, Path(info["source"])))
    if not candidates:
        raise RuntimeError("No configured exact-revision embedding-native source; set PGLITE_ATOMICS_SOURCE to an existing configured stage")
    return max(candidates)[1].resolve()


def _spinlocks(ctx, source, port, compiler, work, artifacts, report):
    fixtures = ctx.root / "tests/Atomics"
    native_results = {}
    cc = os.environ.get("CC", "cc")
    for label, include, defines in (
            ("original", source / "src/include", []),
            ("c11", Path(port["include"]), ["-D" + value for value in port["defines"]])):
        native = work / ("native-spinlocks-" + label)
        ctx.run([cc, "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror", "-pthread",
                 "-D_GNU_SOURCE", "-D__PGLITE__", "-DDPGS_NATIVE_MAIN", *defines,
                 "-I", include, "-I", source, fixtures / "pg_spinlocks.c",
                 source / "src/port/libpgport.a", "-lm", "-o", native],
                "spinlocks-native-" + label + "-build", 120)
        output = ctx.run([native], "spinlocks-native-" + label + "-run", 60)
        native_results[label] = json.loads(output.splitlines()[-1])
        _write(artifacts / ("spinlocks-native-" + label + ".json"), native_results[label])
        _check_stuck(ctx, [native, "--stuck"], "spinlocks-native-" + label + "-stuck")
    if native_results["original"] != native_results["c11"]:
        raise RuntimeError("Native original/C11 spinlock reports differ")
    report["passed"].append("actual native original/C11 slock_t/TAS and upstream s_lock contention/delay/timeout")
    emitted = work / "spinlocks-emitted"
    ctx.run(["dotnet", compiler, fixtures / "pg_spinlocks.c", "-std=c11", "-D_GNU_SOURCE", "-D__PGLITE__",
             *["-D" + value for value in port["defines"]], "-I", port["include"], "-I", source,
             "--emit=managedlib", "--runtime=c", "--nest-types", "--class-name", "Spinlocks",
             "--namespace", "Managed.PGlite.Probes", "--split=size", "--split-size=262144",
             "--literal-pool", "--deduplicate-inline", "-o", emitted], "spinlocks-emission", 180)
    ctx.run(["dotnet", "build", fixtures / "Spinlocks.csproj", "-c", "Release", "--nologo", "-m:1",
             "-p:SpinlocksProject=" + str(emitted / "spinlocks-emitted.csproj"),
             "--artifacts-path", work / "spinlocks-managed"], "spinlocks-managed-build", 180)
    executable = work / "spinlocks-managed/bin/Spinlocks/release/Spinlocks.dll"
    ctx.run(["dotnet", executable, artifacts / "spinlocks-native-original.json",
             artifacts / "spinlocks-managed.json"], "spinlocks-managed-run", 60)
    if json.loads((artifacts / "spinlocks-managed.json").read_text()) != native_results["original"]:
        raise RuntimeError("Actual emitted/native spinlock reports differ")
    _check_stuck(ctx, ["dotnet", executable, "--stuck"], "spinlocks-managed-stuck")
    report["spinlock_status"] = "passed-native-and-emitted-jit"
    report["passed"].append("actual emitted one-byte TAS, 20,000 increments, 64 forced upstream S_LOCK sleeps, seeded backoff and stuck-lock failure")


def _check_stuck(ctx, command, label):
    output = ctx.run(command, label, 15, check=False)
    if ctx.receipt["commands"][-1].get("exit_code") != 1 or "Stuck spinlock detected" not in output:
        raise RuntimeError("Actual upstream stuck-spinlock path must diagnose and exit 1: " + label)


def run(ctx, suite=None):
    """Native original/C11 ports vs actual emitted C# JIT, then postgres.c retry.

    Every subprocess goes through the shared ctx.run logger/cancellation adapter.
    No changes to the shared native stage or compiler outputs. A blocked complete
    postgres.c attempt remains a failing gate even if smaller probes pass.
    """
    work, artifacts = ctx.work / "atomics", ctx.artifacts / "atomics"
    work.mkdir(parents=True, exist_ok=True)
    artifacts.mkdir(parents=True, exist_ok=True)
    report = {
        "scope": "native/emitted PG C11 atomics and one-byte spinlocks with actual upstream contention; no SQL/product qualification",
        "passed": [], "blockers": [],
        "unrun": ["translated SQL", "full product closure", "product raw/processed JIT/AOT qualification", "cross-process shared-memory atomics"],
    }
    try:
        if ctx.profile != "embedding-native":
            raise RuntimeError("Atomics port is bounded to embedding-native Linux LP64 x64")
        pin = json.loads((ctx.root / "config/sources.json").read_text())["product"]
        source = _configured_source(ctx, pin)
        report.update(requested_revision=pin["revision"], configured_source=str(source))
        source_hashes = {name: hashlib.sha256((source / "src/include" / name).read_bytes()).hexdigest()
                         for name in ("port/atomics.h", "storage/s_lock.h", "pg_config.h")}
        port = prepare_overlay(source, work / "include")
        # Same independently owned upstream software-codegen selection as the
        # integrated recipe; this helper mutates only our private pg_config.h.
        from campaigns.recipes import helper
        report["portable_codegen"] = helper(ctx.root / "scripts/portability.py").prepare_overlay(ctx.root, port["include"])
        _write(artifacts / "overlay.json", port)
        ctx.receipt["pglite_atomics_overlay"] = port
        ctx.save()
        fixtures = ctx.root / "tests/Atomics"
        ctx.run(["python3", "-B", fixtures / "test_overlay.py"], "atomics-overlay-checks", 60,
                env={"PGLITE_ATOMICS_SOURCE": str(source), "PGLITE_ATOMICS_TEST_WORK": str(work)})
        cc = os.environ.get("CC", "cc")
        if not shutil.which(cc):
            raise RuntimeError("Atomics native control needs an installed C compiler")
        compiler = getattr(ctx, "compiler", None) or os.environ.get("DOTCC_COMPILER")
        if not compiler or not Path(compiler).is_file():
            raise RuntimeError("Select an existing immutable DOTCC_COMPILER; this suite never builds compiler tools")
        compiler = Path(compiler).resolve()
        report["compiler"] = str(compiler)
        report["compiler_sha256"] = hashlib.sha256(compiler.read_bytes()).hexdigest()
        report["compiler_closure_sha256"] = {
            path.name: hashlib.sha256(path.read_bytes()).hexdigest()
            for path in sorted(compiler.parent.iterdir()) if path.suffix in (".dll", ".json")
        }
        native_results = {}
        for label, include, defines in (
                ("original", source / "src/include", []),
                ("c11", Path(port["include"]), ["-D" + value for value in port["defines"]])):
            native = work / ("native-" + label)
            ctx.run([cc, "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror", "-pthread",
                     "-D_GNU_SOURCE", "-D__PGLITE__", "-DDPGA_NATIVE_MAIN", *defines,
                     "-I", include, fixtures / "pg_atomics.c", "-o", native],
                    "atomics-native-" + label + "-build", 120)
            output = ctx.run([native], "atomics-native-" + label + "-run", 60)
            native_results[label] = json.loads(output)
            _write(artifacts / ("native-" + label + ".json"), native_results[label])
            report["passed"].append("actual PG " + label + " native behavior/layout, four workers and fence handshakes")
        if native_results["original"] != native_results["c11"]:
            raise RuntimeError("Native original/C11 port reports differ")
        flags = ["-std=c11", "-D_GNU_SOURCE", "-D__PGLITE__", *["-D" + value for value in port["defines"]],
                 "-I", port["include"]]
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, fixtures / "pg_atomics.c", *flags, "--emit=managedlib", "--runtime=c",
                 "--nest-types", "--class-name", "Atomics", "--namespace", "Managed.PGlite.Probes",
                 "--split=size", "--split-size=262144", "--literal-pool", "--deduplicate-inline", "-o", emitted],
                "atomics-emission", 180)
        ctx.run(["dotnet", "build", fixtures / "Atomics.csproj", "-c", "Release", "--nologo", "-m:1",
                 "-p:AtomicsProject=" + str(emitted / "emitted.csproj"), "--artifacts-path", work / "managed"],
                "atomics-managed-build", 180)
        ctx.run(["dotnet", work / "managed/bin/Atomics/release/Atomics.dll",
                 artifacts / "native-original.json", artifacts / "managed.json"], "atomics-managed-run", 60)
        if json.loads((artifacts / "managed.json").read_text()) != native_results["original"]:
            raise RuntimeError("Actual emitted C# / native reports differ")
        report["passed"].append("actual emitted C# PG wrappers equal both native controls, including bounded concurrency and fences")
        report["atomic_status"] = "passed-native-and-emitted-jit"
        _spinlocks(ctx, source, port, compiler, work, artifacts, report)
        # A complete real TU diagnostic, not part of any publishable manifest.
        policy = json.loads((ctx.root / "config/profile.json").read_text())["profiles"]["embedding-native"]
        overrides = ["-D" + value for value in policy["overrides"]]
        ctx.run(["dotnet", compiler, source / "src/backend/tcop/postgres.c", *flags, *overrides,
                 "--emit=obj", "-o", work / "postgres.cs"], "atomics-postgres-full-emission", 180, check=False)
        command = ctx.receipt["commands"][-1]
        report["postgres_emission"] = {name: command.get(name) for name in ("exit_code", "status", "stdout", "seconds")}
        log = Path(command["stdout"]).read_text(errors="replace")
        report["postgres_diagnostic"] = log.strip()
        if command.get("exit_code") != 0:
            report["blockers"].append("Complete postgres.c cannot emit: see atomics-postgres-full-emission.log; supervisor owns compiler/source closure and full retry")
        else:
            report["passed"].append("complete actual postgres.c object emission with the private C11 port")
        # DotCC currently reports unresolved includes without a failing exit
        # status. Keep actual object-emission success separate from closure.
        missing = [line for line in log.splitlines() if "#include" in line and "not resolvable" in line]
        if missing:
            report["postgres_unresolved_includes"] = missing
            report["blockers"].append("postgres.c required include closure remains incomplete despite the compiler exit status; see postgres_unresolved_includes")
        for name, digest in source_hashes.items():
            if hashlib.sha256((source / "src/include" / name).read_bytes()).hexdigest() != digest:
                raise RuntimeError("Configured source changed during independent experiment: " + name)
        report["source_headers_unchanged"] = source_hashes
        if report["blockers"]:
            raise RuntimeError("; ".join(report["blockers"]))
        report["status"] = "passed-experiment-only"
        return report
    except BaseException as error:
        report["status"] = "blocked" if report["blockers"] else "failed"
        report["error"] = str(error)
        raise
    finally:
        _write(artifacts / "report.json", report)
