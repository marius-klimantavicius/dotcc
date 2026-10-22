"""Readonly POSIX host ABI/behavior probe; no source acquisition or compiler build.

Uses the campaign Context logger in private posixhost-owner outputs. Actual
PostgreSQL source retries are emission checks, never database acceptance.
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


def run(ctx, suite=None):
    from campaigns.recipes import helper, link_flags
    compiler = Path(ctx.compiler).resolve()
    source = Path(os.environ["PGLITE_POSIXHOST_SOURCE"]).resolve()
    if not compiler.is_file() or not (source / "src/include/pg_config.h").is_file():
        raise RuntimeError("Select an existing immutable compiler and readonly configured source")
    work, artifacts = ctx.work / "posixhost", ctx.artifacts / "posixhost"
    work.mkdir(); artifacts.mkdir()
    fixtures = ctx.root / "tests/PosixHost"
    report = dict(status="running", host_status="unrun", source_retries=[],
                  scope="real POSIX host services only; full frontend/backend and SQL acceptance remain open",
                  compiler=str(compiler), source=str(source),
                  compiler_closure_sha256={p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                                          for p in sorted(compiler.parent.iterdir()) if p.suffix in (".dll", ".json")})
    try:
        # Explicit authored headers make the exact input independent of an
        # older immutable tool's embedded headers. Runtime resources still
        # come exclusively from that tool; generated C# is never repaired.
        system_include = work / "system-include"
        system_include.mkdir()
        report["authored_headers"] = {}
        for name in ("pwd.h", "termios.h"):
            path = ctx.repo / "DotCC.Lib/include" / name
            shutil.copy2(path, system_include / name)
            report["authored_headers"][name] = hashlib.sha256(path.read_bytes()).hexdigest()
        native = work / "native"
        ctx.run([os.environ.get("CC", "cc"), "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror",
                 "-D_GNU_SOURCE", "-DDPH_NATIVE_MAIN", fixtures / "posix_host.c", "-o", native],
                "posixhost-native-build", 120)
        for label, extra in (("native", []), ("native-files", ["files"])):
            value = json.loads(ctx.run([native, "/dotcc-posixhost-" + label + "-" + ctx.run_id, *extra],
                                      "posixhost-" + label + "-run", 60))
            if extra and value["missing_error"] != 0:
                raise RuntimeError("Real files NSS control must demonstrate 0,NULL no-entry")
            _write(artifacts / (label + ".json"), value)
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, fixtures / "posix_host.c", "-std=c11", "-D_GNU_SOURCE",
                 "-I", system_include, "--emit=managedlib", *link_flags("PosixHost", "Managed.PGlite.Probes"),
                 "-o", emitted], "posixhost-emission", 180)
        setting = "-p:PosixHostProject=" + str(emitted / "emitted.csproj")
        project = fixtures / "PosixHost.csproj"
        mode = getattr(ctx.options, "mode", "all")
        for flavor in (("jit", "aot") if mode == "all" else (mode,)):
            if flavor == "jit":
                ctx.run(["dotnet", "build", project, "-c", "Release", "--nologo", "-m:1", setting,
                         "--artifacts-path", work / "jit"], "posixhost-jit-build", 180)
                command = ["dotnet", work / "jit/bin/PosixHost/release/PosixHost.dll"]
            else:
                ctx.run(["dotnet", "publish", project, "-c", "Release", "--nologo", "-m:1", setting,
                         "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot",
                         "--artifacts-path", work / "aot-artifacts"], "posixhost-aot-build", 300)
                command = [work / "aot/PosixHost"]
            for suffix, native_label, extra in (("", "native", []), ("-files", "native-files", ["files"])):
                ctx.run([*command, artifacts / (native_label + ".json"), artifacts / (flavor + suffix + ".json"), *extra],
                        "posixhost-" + flavor + suffix + "-run", 60)
        report["host_status"] = "passed-native-emitted-" + mode
        # Retry the actual callers with their original selected include/define
        # contexts, changing only the already reviewed private capability overlays.
        initializer = helper(ctx.root / "scripts/initializer.py")
        info = json.loads((source.parent / "prepared.json").read_text())
        frontend = initializer.prepare_frontend_overlay(ctx.root, source, work / "frontend-include")
        frontend_units = initializer.frontend_units(ctx.root, info, frontend["include"])
        backend = helper(ctx.root / "scripts/atomics.py").prepare_overlay(source, work / "backend-include")
        report["portable_backend"] = helper(ctx.root / "scripts/portability.py").prepare_overlay(ctx.root, backend["include"])
        manifest = json.loads((ctx.root / "config/backend-units.json").read_text())
        selected = ["src/bin/initdb/initdb.c", "src/common/sprompt.c", "src/port/path.c", "src/backend/storage/ipc/dsm_impl.c"]
        before = {name: hashlib.sha256((source / name).read_bytes()).hexdigest()
                  for name in [*selected, "src/include/pg_config.h"]}
        for index, name in enumerate(selected):
            if name != selected[-1]:
                unit = next(unit for unit in frontend_units if unit["relative_source"] == name)
                flags = ["-std=c11", *("-D" + item for item in unit["defines"])]
                for include in [str(system_include), *unit["includes"]]:
                    flags.extend(["-I", include])
            else:
                context = next(item for item in manifest["contexts"] if name in item["units"])
                flags = ["-std=c11", "-D_GNU_SOURCE", "-D__PGLITE__", *("-D" + item for item in backend["defines"])]
                flags.extend(value.replace("{native_base}", str(source.parent)) for value in context["defines"])
                for include in [system_include, backend["include"], *[source / include if include != "{cwd}" else (source / name).parent
                                                                 for include in context["includes"]]]:
                    flags.extend(["-I", include])
            ctx.run(["dotnet", compiler, source / name, *flags, "--emit=obj", "-o", work / f"retry-{index}.cs"],
                    f"posixhost-source-retry-{index}", 180, check=False)
            command = ctx.receipt["commands"][-1]
            diagnostic = Path(command["stdout"]).read_text(errors="replace")
            report["source_retries"].append(dict(source=name, exit_code=command["exit_code"],
                                                  diagnostic=diagnostic, log=command["stdout"]))
        for name, digest in before.items():
            if hashlib.sha256((source / name).read_bytes()).hexdigest() != digest:
                raise RuntimeError("Readonly source changed: " + name)
        report["source_unchanged_sha256"] = before
        failed = [item for item in report["source_retries"] if item["exit_code"] != 0 or "not resolvable" in item["diagnostic"]]
        if failed:
            report["next_source_failure"] = failed[0]
            raise RuntimeError("POSIX host gate passed; actual source closure still fails: " + failed[0]["source"])
        report["status"] = "passed-bounded-host-experiment"
        if getattr(ctx.options, "frontend", False):
            # The supervisor authorizes this private complete retry. It uses
            # the original initializer helper; no recipe/source changes here.
            os.environ["PGLITE_INITIALIZATION_PREPARATION"] = str(source.parent / "prepared.json")
            report["frontend_status"] = "running"
            try:
                report["frontend_result"] = initializer.run(ctx)
                report["frontend_status"] = "passed-frontend-build-only"
            except BaseException:
                report["frontend_status"] = "failed"
                report["next_build_failure_report"] = str(ctx.artifacts / "initialization/report.json")
                raise
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        _write(artifacts / "report.json", report)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compiler", type=Path, required=True)
    parser.add_argument("--source", type=Path, required=True)
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    parser.add_argument("--frontend", action="store_true", help="Also retry all actual frontend units and the C# build privately")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / "Scripts"))
    from campaigns.model import Recipe
    from campaigns.runner import Context
    options = SimpleNamespace(profile="embedding-native", action="posixhost-probe", hashes="warn", fetch="never",
                              restore="normal", mode=args.mode, frontend=args.frontend)
    recipe = Recipe("pglite", "TranslatedPGlite", ("embedding-native",), (), lambda root: ())
    ctx = Context(root.parent, recipe, options)
    ctx.artifacts = root / "artifacts" / ("posixhost-owner-" + ctx.run_id)
    ctx.work = root / "build" / ("posixhost-owner-" + ctx.run_id)
    ctx.env["TMPDIR"] = str(ctx.work / "tmp")
    ctx.receipt["staging"] = str(ctx.work.relative_to(root))
    ctx.compiler = args.compiler.resolve()
    os.environ["PGLITE_POSIXHOST_SOURCE"] = str(args.source.resolve())
    ctx.start()
    try:
        run(ctx)
        ctx.receipt["status"] = "passed"
    except BaseException as error:
        ctx.receipt.update(status="failed", error=str(error))
        raise
    finally:
        ctx.receipt["finished"] = time.time()
        ctx.save()


if __name__ == "__main__":
    main()
