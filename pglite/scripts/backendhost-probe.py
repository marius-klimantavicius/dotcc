"""Actual native/emitted backend host contracts, using existing immutable tools.

No source acquisition, shared compiler build, whole-backend emission or product
publication. The standalone command writes only backendhost-owner private paths.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import shutil
import sys
import time
from types import SimpleNamespace


def _write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n")


def run_durable_rename(ctx, objects, source, mode="all"):
    """Relink existing complete original frontend objects with current runtime.

    This is a host-boundary diagnostic, not fresh frontend regeneration or an
    initialization/SQL acceptance claim. No C unit is emitted by this function.
    """
    from campaigns.recipes import link_flags

    objects, source = Path(objects).resolve(), Path(source).resolve()
    selected = sorted(objects.glob("[0-9][0-9][0-9]-*.cs"))
    selected.extend(objects / name for name in ("libc.cs", "adapter.cs"))
    if len(selected) < 75 or not all(path.is_file() for path in selected):
        raise RuntimeError("Supply an existing complete original frontend object closure")
    work, artifacts = ctx.work / "durable-rename", ctx.artifacts / "durable-rename"
    work.mkdir(); artifacts.mkdir()
    report = dict(status="running", scope="original complete frontend durable rename; no initialization/SQL claim",
                  source=str(source), objects=str(objects), compiler=str(ctx.compiler),
                  object_sha256={path.name: hashlib.sha256(path.read_bytes()).hexdigest() for path in selected})
    try:
        fixtures = ctx.root / "tests/BackendHost"
        native = work / "native"
        ctx.run([os.environ.get("CC", "cc"), "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror",
                 fixtures / "durable_rename_native.c", "-Wl,--start-group", source / "src/common/libpgcommon.a",
                 source / "src/port/libpgport.a", source.parent / "pglitec.o", "-Wl,--end-group", "-lm", "-o", native],
                "durable-rename-native-build", 120)
        directory = work / "native-directory"; directory.mkdir()
        ctx.run([native, directory], "durable-rename-native-run", 60)
        output = work / "frontend"
        ctx.run(["dotnet", ctx.compiler, *selected, "--emit=managedlib", "--state-context",
                 *link_flags("Initdb", "Managed.PGlite.Initialization.Translated"), "-o", output], "durable-rename-resource-link", 180)
        setting = "-p:InitdbProject=" + str(output / "frontend.csproj")
        project = fixtures / "DurableRename.csproj"
        for flavor in (("jit", "aot") if mode == "all" else (mode,)):
            if flavor == "jit":
                ctx.managed("build", project, "durable-rename-jit-build", "-m:1", setting, "--artifacts-path", work / "jit")
                command = ["dotnet", work / "jit/bin/DurableRename/release/DurableRename.dll"]
            else:
                ctx.managed("publish", project, "durable-rename-aot-build", "-m:1", setting, "-r", "linux-x64", "-p:PublishAot=true",
                            "-o", work / "aot", "--artifacts-path", work / "aot-artifacts")
                command = [work / "aot/DurableRename"]
            # -yy identifies each fsync's actual backing. Retain this independent
            # evidence when strace is installed; it is not a test prerequisite.
            tracer = shutil.which("strace")
            if tracer:
                trace = artifacts / (flavor + "-syscalls.log")
                ctx.run([tracer, "-f", "-yy", "-e", "trace=fsync,rename", "-o", trace,
                         *command, work / (flavor + "-directories")], "durable-rename-" + flavor + "-run", 120)
                report[flavor + "_syscalls"] = str(trace)
            else:
                ctx.run([*command, work / (flavor + "-directories")], "durable-rename-" + flavor + "-run", 120)
        report["status"] = "passed-native-original-emitted-" + mode
        ctx.receipt.setdefault("outputs", {})["durable_rename"] = str(artifacts / "report.json")
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        _write(artifacts / "report.json", report)


def run_header_compat(ctx, suite=None):
    """Qualify raw-build header gaps without repeating the existing host gate."""
    from campaigns.recipes import link_flags

    mode = getattr(ctx.options, "mode", "all")
    work, artifacts = ctx.work / "backendhost-header", ctx.artifacts / "backendhost-header"
    work.mkdir(); artifacts.mkdir()
    fixtures = ctx.root / "tests/BackendHost"
    headers = work / "include"
    report = dict(status="running", compiler=str(ctx.compiler), mode=mode,
                  scope="raw191828 missing header declarations and remapped C prototypes; no backend execution claim",
                  headers_sha256={}, fixture_sha256={p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in [fixtures / name for name in ("header_compat.c", "header_compat_impl.c", "HeaderCompatProgram.cs", "HeaderCompat.csproj")]},
                  compiler_closure_sha256={p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in sorted(Path(ctx.compiler).parent.iterdir()) if p.suffix in (".dll", ".json")})
    try:
        for name in ("signal.h", "netinet/in.h", "sys/shm.h"):
            source, target = ctx.repo / "DotCC.Lib/include" / name, headers / name
            target.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(source, target)
            report["headers_sha256"][name] = hashlib.sha256(source.read_bytes()).hexdigest()
        native = work / "native"
        sources = [fixtures / name for name in ("header_compat.c", "header_compat_impl.c")]
        flags = ["-std=c11", "-D__PGLITE__", "-Dshmctl=pgl_shmctl", "-Dshmat=pgl_shmat"]
        ctx.run([os.environ.get("CC", "cc"), *flags, "-Wall", "-Wextra", "-Werror", "-DDPH_NATIVE_MAIN",
                 *sources, "-o", native], "backendhost-header-native-build", 120)
        observations = ctx.run([native], "backendhost-header-native-run", 60)
        expected = artifacts / "native-observations.txt"
        expected.write_text(observations)
        report["native_observations"] = [int(value) for value in observations.split()]
        if len(report["native_observations"]) != 5:
            raise RuntimeError("Incomplete native header observation list")
        objects = []
        for index, source in enumerate(sources):
            output = work / ("unit-" + str(index) + ".cs")
            ctx.run(["dotnet", ctx.compiler, source, *flags, "-I", headers, "--emit=obj", "-o", output],
                    "backendhost-header-emission-" + str(index), 120)
            objects.append(output)
        emitted = work / "fixture"
        ctx.run(["dotnet", ctx.compiler, *objects, "--emit=managedlib", "--state-context",
                 *link_flags("BackendHostHeaderFixture", "Managed.PGlite.Probes"), "-o", emitted],
                "backendhost-header-link", 120)
        project = fixtures / "HeaderCompat.csproj"
        setting = "-p:BackendHostHeaderProject=" + str(emitted / "fixture.csproj")
        for flavor in (("jit", "aot") if mode == "all" else (mode,)):
            if flavor == "jit":
                ctx.managed("build", project, "backendhost-header-jit-build", "-m:1", setting, "--artifacts-path", work / "jit")
                command = ["dotnet", work / "jit/bin/HeaderCompat/release/HeaderCompat.dll"]
            else:
                ctx.managed("publish", project, "backendhost-header-aot-build", "-m:1", setting, "-r", "linux-x64", "-p:PublishAot=true",
                            "-o", work / "aot", "--artifacts-path", work / "aot-artifacts")
                command = [work / "aot/HeaderCompat"]
            ctx.run([*command, expected], "backendhost-header-" + flavor + "-run", 120)
        report["status"] = "passed-native-emitted-" + mode
        ctx.receipt.setdefault("outputs", {})["backendhost_header"] = str(artifacts / "report.json")
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        _write(artifacts / "report.json", report)


def run(ctx, suite=None):
    from campaigns.recipes import link_flags

    compiler = Path(ctx.compiler).resolve()
    if not compiler.is_file():
        raise RuntimeError("Select an existing immutable compiler")
    mode = getattr(ctx.options, "mode", "all")
    work, artifacts = ctx.work / "backendhost", ctx.artifacts / "backendhost"
    work.mkdir(); artifacts.mkdir()
    fixtures = ctx.root / "tests/BackendHost"
    report = dict(status="running", compiler=str(compiler), scope="backend host services only; complete SQL/product acceptance remains open",
                  mode=mode, compiler_closure_sha256={p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                  for p in sorted(compiler.parent.iterdir()) if p.suffix in (".dll", ".json")})
    try:
        headers = work / "include"
        headers.mkdir()
        report["headers_sha256"] = {}
        for name in ("fcntl.h", "unistd.h", "stdio.h", "stdlib.h", "ctype.h", "wctype.h", "math.h", "ifaddrs.h", "execinfo.h"):
            source = ctx.repo / "DotCC.Lib/include" / name
            shutil.copy2(source, headers / name)
            report["headers_sha256"][name] = hashlib.sha256(source.read_bytes()).hexdigest()
        report["runtime_sha256"] = {p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                                    for p in sorted((ctx.repo / "DotCC.Libc").glob("PgliteBackend*Lib.cs"))}
        native = work / "native"
        ctx.run([os.environ.get("CC", "cc"), "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror",
                 "-DDPH_NATIVE_MAIN", fixtures / "backend_host.c", "-lm", "-o", native], "backendhost-native-build", 120)
        native_directory = work / "native-directory"
        native_directory.mkdir()
        observations = ctx.run([native, native_directory], "backendhost-native-run", 60)
        native_observations = artifacts / "native-observations.txt"
        native_observations.write_text(observations)
        values = [int(value) for value in observations.split()]
        if len(values) != 18:
            raise RuntimeError("Incomplete native backend-host observation list")
        report["native_observations"] = values
        emitted = work / "fixture"
        ctx.run(["dotnet", compiler, fixtures / "backend_host.c", "-std=c11", "-I", headers,
                 "--emit=managedlib", *link_flags("BackendHostFixture", "Managed.PGlite.Probes"), "--state-context",
                 "-o", emitted], "backendhost-emission", 180)
        project = fixtures / "Emitted.csproj"
        setting = "-p:BackendHostProject=" + str(emitted / "fixture.csproj")
        for flavor in (("jit", "aot") if mode == "all" else (mode,)):
            if flavor == "jit":
                ctx.managed("build", project, "backendhost-jit-build", "-m:1", setting, "--artifacts-path", work / "jit")
                command = ["dotnet", work / "jit/bin/Emitted/release/Emitted.dll"]
            else:
                ctx.managed("publish", project, "backendhost-aot-build", "-m:1", setting, "-r", "linux-x64", "-p:PublishAot=true",
                            "-o", work / "aot", "--artifacts-path", work / "aot-artifacts")
                command = [work / "aot/Emitted"]
            ctx.run([*command, native_observations, work / (flavor + "-directories")], "backendhost-" + flavor + "-run", 120)
        frontend_objects = getattr(ctx.options, "frontend_objects", None) or os.environ.get("PGLITE_BACKENDHOST_FRONTEND_OBJECTS")
        source = getattr(ctx.options, "source", None) or os.environ.get("PGLITE_BACKENDHOST_SOURCE")
        if bool(frontend_objects) != bool(source):
            raise RuntimeError("Original durable rename requires both frontend objects and configured source")
        if frontend_objects:
            report["durable_rename"] = run_durable_rename(ctx, frontend_objects, source, mode)
        else:
            report["durable_rename"] = "unrun; select complete existing frontend objects and readonly configured source"
        report["status"] = "passed-native-emitted-" + mode
        ctx.receipt.setdefault("outputs", {})["backendhost"] = str(artifacts / "report.json")
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        _write(artifacts / "report.json", report)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    parser.add_argument("--frontend-objects", type=Path, help="Existing complete frontend object closure for original durable-rename gate")
    parser.add_argument("--source", type=Path, help="Readonly configured source containing native control archives")
    parser.add_argument("--headers-only", action="store_true", help="Run only the focused raw-build header compatibility gate")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / "Scripts"))
    from campaigns.model import Recipe
    from campaigns.runner import Context

    options = SimpleNamespace(profile="embedding-native", action="backendhost-probe", hashes="warn", fetch="never", restore="normal", mode=args.mode,
                              frontend_objects=args.frontend_objects, source=args.source)
    recipe = Recipe("pglite", "TranslatedPGlite", ("embedding-native",), (), lambda root: ())
    ctx = Context(root.parent, recipe, options)
    ctx.artifacts = root / "artifacts" / ("backendhost-owner-" + ctx.run_id)
    ctx.work = root / "build" / ("backendhost-owner-" + ctx.run_id)
    ctx.env["TMPDIR"] = str(ctx.work / "tmp")
    ctx.receipt["staging"] = str(ctx.work.relative_to(root))
    ctx.compiler = args.compiler.resolve()
    ctx.start()
    try:
        (run_header_compat if args.headers_only else run)(ctx)
        ctx.receipt["status"] = "passed"
    except BaseException as error:
        ctx.receipt.update(status="failed", error=str(error))
        raise
    finally:
        # Do not change the normal campaign's latest-attempt pointer.
        ctx.receipt["finished"] = time.time()
        ctx.save()


if __name__ == "__main__":
    main()
