"""Readonly-source POSIX locale host qualification; no compiler/product builds.

run(ctx, suite=None) uses the shared framework command and receipt logger.
The standalone entry point also uses Context, without changing campaign pointers.
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
    compiler = Path(getattr(ctx, "compiler", None) or os.environ["DOTCC_COMPILER"]).resolve()
    source = Path(ctx.env.get("PGLITE_LOCALE_SOURCE") or os.environ["PGLITE_LOCALE_SOURCE"]).resolve()
    if not compiler.is_file() or not (source / "src/include/pg_config.h").is_file():
        raise RuntimeError("Select existing immutable compiler and readonly configured source")
    work, artifacts = ctx.work / "locale", ctx.artifacts / "locale"
    work.mkdir(); artifacts.mkdir()
    original = {name: hashlib.sha256((source / "src/include" / name).read_bytes()).hexdigest()
                for name in ("pg_config.h", "port/atomics.h", "storage/s_lock.h", "utils/pg_locale.h")}
    report = {
        "scope": "POSIX C/C.UTF-8 locale host services and actual PG header layout; managed SQL unqualified",
        "status": "running", "compiler": str(compiler), "source": str(source),
        "compiler_closure_sha256": {p.name: hashlib.sha256(p.read_bytes()).hexdigest()
                                    for p in sorted(compiler.parent.iterdir()) if p.suffix in (".dll", ".json")},
        "locale_status": "unrun", "source_retries": [],
    }
    try:
        port = helper(ctx.root / "scripts/atomics.py").prepare_overlay(source, work / "include")
        report["portable_codegen"] = helper(ctx.root / "scripts/portability.py").prepare_overlay(ctx.root, port["include"])
        # Exact authored-header copy permits discovery with an allocated older
        # immutable compiler; all other system headers still come from that
        # compiler. Final integrated snapshots embed this same real header.
        system_include = work / "system-include"
        system_include.mkdir()
        wctype = ctx.repo / "DotCC.Lib/include/wctype.h"
        shutil.copy2(wctype, system_include / "wctype.h")
        report["authored_wctype_sha256"] = hashlib.sha256(wctype.read_bytes()).hexdigest()
        flags = ["-std=c11", "-D_GNU_SOURCE", "-D__PGLITE__",
                 *["-D" + define for define in port["defines"]], "-I", port["include"], "-I", source,
                 "-I", system_include]
        fixtures = ctx.root / "tests/Locale"
        native = work / "native"
        ctx.run([os.environ.get("CC", "cc"), "-std=gnu11", "-O2", "-Wall", "-Wextra", "-Werror",
                 "-D_GNU_SOURCE", "-D__PGLITE__", "-DDPL_NATIVE_MAIN", "-I", source / "src/include",
                 fixtures / "pg_locale.c", "-o", native], "locale-native-build", 120)
        native_result = json.loads(ctx.run([native], "locale-native-run", 60))
        _write(artifacts / "native.json", native_result)
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, fixtures / "pg_locale.c", *flags, "--emit=managedlib",
                 *link_flags("Locale", "Managed.PGlite.Probes"), "-o", emitted], "locale-emission", 180)
        project = fixtures / "Locale.csproj"
        project_setting = "-p:LocaleProject=" + str(emitted / "emitted.csproj")
        mode = getattr(ctx.options, "mode", "all")
        if mode in ("all", "jit"):
            ctx.run(["dotnet", "build", project, "-c", "Release", "--nologo", "-m:1", project_setting,
                     "--artifacts-path", work / "jit"], "locale-jit-build", 180)
            ctx.run(["dotnet", work / "jit/bin/Locale/release/Locale.dll", artifacts / "native.json",
                     artifacts / "jit.json"], "locale-jit-run", 60)
        if mode in ("all", "aot"):
            ctx.run(["dotnet", "publish", project, "-c", "Release", "--nologo", "-m:1", project_setting,
                     "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot",
                     "--artifacts-path", work / "aot-artifacts"], "locale-aot-build", 300)
            ctx.run([work / "aot/Locale", artifacts / "native.json", artifacts / "aot.json"], "locale-aot-run", 60)
        report["locale_status"] = "passed-native-emitted-" + mode
        manifest = json.loads((ctx.root / "config/backend-units.json").read_text())
        # Retry the original failing TU plus the actual libc-locale callers.
        selected = ["src/backend/access/hash/hashfunc.c", "src/backend/utils/adt/pg_locale_libc.c",
                    "src/backend/utils/adt/pg_locale.c", "src/port/chklocale.c", "src/port/pg_localeconv_r.c"]
        for index, name in enumerate(selected):
            context = next(item for item in manifest["contexts"] if name in item["units"])
            unit_flags = [value.replace("{native_base}", str(source.parent)) for value in context["defines"]]
            for include in context["includes"]:
                unit_flags.extend(["-I", (source / name).parent if include == "{cwd}" else source / include])
            ctx.run(["dotnet", compiler, source / name, *flags, *unit_flags, "--emit=obj", "-o", work / f"retry-{index}.cs"],
                    f"locale-source-retry-{index}", 180, check=False)
            command = ctx.receipt["commands"][-1]
            diagnostic = Path(command["stdout"]).read_text(errors="replace")
            report["source_retries"].append({"source": name, "exit_code": command["exit_code"],
                                              "diagnostic": diagnostic, "log": command["stdout"]})
        failures = [item for item in report["source_retries"] if item["exit_code"] != 0 or "not resolvable" in item["diagnostic"]]
        report["source_headers_unchanged"] = original
        for name, digest in original.items():
            if hashlib.sha256((source / "src/include" / name).read_bytes()).hexdigest() != digest:
                raise RuntimeError("Readonly source changed: " + name)
        if failures:
            report["next_source_failure"] = failures[0]
            raise RuntimeError("Locale qualification passed; actual-source closure remains incomplete: " + failures[0]["source"])
        report["status"] = "passed-experiment-only"
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
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / "Scripts"))
    from campaigns.model import Recipe
    from campaigns.runner import Context
    options = SimpleNamespace(profile="embedding-native", action="locale-probe", hashes="warn", fetch="never",
                              restore="normal", mode=args.mode)
    recipe = Recipe("pglite", "TranslatedPGlite", ("embedding-native",), (), lambda root: ())
    ctx = Context(root.parent, recipe, options)
    # Prefixes and isolated outputs avoid every supervisor-owned campaign pointer.
    ctx.artifacts = root / "artifacts" / ("locale-owner-" + ctx.run_id)
    ctx.work = root / "build" / ("locale-owner-" + ctx.run_id)
    ctx.env["TMPDIR"] = str(ctx.work / "tmp")
    ctx.receipt["staging"] = str(ctx.work.relative_to(root))
    ctx.compiler = args.compiler.resolve()
    os.environ["PGLITE_LOCALE_SOURCE"] = str(args.source.resolve())
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
