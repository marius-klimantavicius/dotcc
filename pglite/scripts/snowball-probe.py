"""Readonly Snowball module compile/emission and original API execution control.

Uses initializer.backend_module_units for the product source/wrapper/flags, and
recovers the exact backend include context from a supplied normal receipt.
Does not acquire sources, configure, build shared tools, publish, or link the
backend. The independent four-source API fixture uses upstream libstemmer's
ordinary allocation interface, not PostgreSQL's palloc macro header.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import sys
import time
from types import SimpleNamespace


def write_json(path, value):
    Path(path).write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n")


def digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def makefile_sources(source):
    """Resolve the actual OBJS assignments without invoking mutating make."""
    directory = Path(source) / "src/backend/snowball"
    text = (directory / "Makefile").read_text().replace("\\\n", " ")
    objects = []
    for line in text.splitlines():
        match = re.fullmatch(r"OBJS\s*(\+?=)\s*(.*)", line)
        if not match:
            continue
        if match[1] == "=":
            objects = []
        for word in match[2].split():
            if word == "$(WIN32RES)":
                continue  # No Windows resource source in the Linux profile.
            if not re.fullmatch(r"[A-Za-z0-9_]+\.o", word):
                raise RuntimeError("Unreviewed Snowball Makefile object: " + word)
            objects.append(word)
    if not objects or len(objects) != len(set(objects)):
        raise RuntimeError("Missing or duplicate Snowball Makefile objects")
    sources = []
    for name in objects:
        candidates = [path for path in (directory / (name[:-2] + ".c"),
                                       directory / "libstemmer" / (name[:-2] + ".c")) if path.is_file()]
        if len(candidates) != 1:
            raise RuntimeError("Snowball object must resolve to one real source: " + name)
        sources.append(candidates[0].relative_to(source).as_posix())
    return sources


def backend_context(receipt):
    """Keep the exact common options and overlay identity of a normal attempt.

    Both a real backend command and its static catalog command are required;
    their common prefix excludes all per-unit flags and output/report paths.
    No include directories are copied or replaced.
    """
    source = Path(receipt["pglite_preparation"]["source"])
    commands = [item["command"] for item in receipt["commands"]
                if item.get("exit_code") == 0 and "--emit=obj" in item["command"]]
    backend = next((argv for argv in commands
                    if any(str(part).startswith(str(source / "src/backend") + "/")
                           and str(part).endswith(".c") for part in argv)), None)
    module = next((argv for argv in commands
                   if any(Path(part).name == "dotcc_plpgsql_module.c" for part in argv)), None)
    if backend is None or module is None:
        raise RuntimeError("Normal receipt must retain successful backend and static-module object commands")
    flags = []
    for left, right in zip(backend[2:], module[2:]):
        if left != right:
            break
        flags.append(left)
    # A per-unit include may share its -I token while its value differs.
    # Do not accidentally consume the first appended per-unit flag as a path.
    if flags and flags[-1] in ("-I", "-D", "-isystem", "--overrides-file"):
        flags.pop()
    include = Path(receipt["pglite_atomics_overlay"]["include"])
    if str(include) not in flags or "--emit=obj" in flags or not include.is_dir():
        raise RuntimeError("Cannot recover the exact existing coherent backend include context")
    for macro in ("__PGLITE__", "PGLITE_DOTCC_C11_ATOMICS", "PGLITE_DOTCC_C11_SPINLOCKS"):
        if "-D" + macro not in flags:
            raise RuntimeError("Normal backend context is missing " + macro)
    return flags, include


def case_header(selection):
    languages = {"english": 0, "lithuanian": 1}
    entries, arrays = [], []
    for index, item in enumerate(selection["cases"]):
        if item["language"] not in languages:
            raise RuntimeError("Unreviewed execution-control language")
        values = []
        for name in ("input", "expected"):
            data = item[name].encode("utf-8")
            symbol = f"dsb_{name}_{index}"
            arrays.append("static const symbol " + symbol + "[] = {" +
                          ",".join(str(value) for value in (*data, 0)) + "};")
            values.extend((symbol, str(len(data))))
        entries.append("{" + ",".join((str(languages[item["language"]]), *values)) + "}")
    if not entries or selection["rounds"] < 2 or selection["allocation_cycles"] < 2:
        raise RuntimeError("Execution control requires cases, repeated resets and allocation cycles")
    return ("\n".join(arrays) + "\nstatic const struct dsb_case dsb_cases[] = {\n" +
            ",\n".join(entries) + "\n};\n" +
            f"#define DSB_CASE_COUNT {len(entries)}\n#define DSB_ROUNDS {selection['rounds']}\n" +
            f"#define DSB_ALLOCATION_CYCLES {selection['allocation_cycles']}\n")


def execution(ctx, compiler, source, work, artifacts):
    from campaigns.recipes import link_flags
    fixtures = ctx.root / "tests/Snowball"
    selection = json.loads((fixtures / "cases.json").read_text())
    work.mkdir()
    (work / "cases.h").write_text(case_header(selection))
    sources = [source / "src/backend/snowball/libstemmer" / name for name in
               ("api.c", "utilities.c", "stem_UTF_8_english.c", "stem_UTF_8_lithuanian.c")]
    # Header-first forwarding wrappers expose the original standalone API.
    # header.h is the upstream Snowball header, not PostgreSQL's replacement.
    # DotCC does not implement the native compiler's -include option.
    wrappers = []
    for path in sources:
        wrapper = work / (path.stem + "-input.c")
        wrapper.write_text('#include <stdlib.h>\n#include <string.h>\n#include "' + path.as_posix() + '"\n')
        wrappers.append(wrapper)
    flags = ["-std=c11", "-D_GNU_SOURCE",
             "-I", str(source / "src/include/snowball/libstemmer"), "-I", str(work)]
    report = dict(scope="four original Snowball sources with upstream allocation interface; no dictionary/SQL execution",
                  sources=[str(path) for path in sources],
                  source_sha256={str(path): digest(path) for path in sources},
                  wrappers={str(path): {"text": path.read_text(), "sha256": digest(path)} for path in wrappers},
                  flags=flags, cases=selection,
                  modes={}, status="running")
    try:
        native = work / "native"
        fixture = fixtures / "probe.c"
        ctx.run([os.environ.get("CC", "cc"), *flags, "-Wall", "-Wextra", "-Werror",
                 "-DDSB_NATIVE_MAIN", *wrappers, fixture, "-o", native],
                "snowball-api-native-build", 120)
        expected = ""
        for owner in range(2):
            actual = ctx.run([native], f"snowball-api-native-owner-{owner}", 60)
            (artifacts / f"native-owner-{owner}.txt").write_text(actual)
            expected += actual
        report["native"] = "passed-two-fresh-process-controls"
        # Emit the original source includes as independent objects, then strict-link
        # only these four sources plus the authored assertion fixture.
        objects = []
        for index, path in enumerate([*wrappers, fixture]):
            output = work / f"{index}-{path.stem}.cs"
            ctx.run(["dotnet", compiler, *flags, path, "--emit=obj", "-o", output],
                    f"snowball-api-object-{index}", 180)
            objects.append(output)
        emitted = work / "emitted"
        ctx.run(["dotnet", compiler, *objects, "--emit=managedlib", "--state-context",
                 *link_flags("SnowballProbe", "Managed.PGlite.Probes"), "-o", emitted],
                "snowball-api-link", 180)
        project = fixtures / "Snowball.csproj"
        setting = "-p:SnowballProject=" + str(emitted / "emitted.csproj")
        mode = getattr(ctx.options, "mode", "all")
        if mode not in ("jit", "aot", "all"):
            raise RuntimeError("Snowball execution mode must be jit, aot or all")
        failures = []
        for flavor in (("jit", "aot") if mode == "all" else (mode,)):
            try:
                if flavor == "jit":
                    ctx.managed("build", project, "snowball-api-jit-build", setting, "-m:1",
                                "--artifacts-path", work / "jit")
                    command = ["dotnet", work / "jit/bin/Snowball/release/Snowball.dll"]
                else:
                    ctx.managed("publish", project, "snowball-api-aot-publish", setting, "-m:1",
                                "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot",
                                "--artifacts-path", work / "aot-artifacts")
                    command = [work / "aot/Snowball"]
                actual = ctx.run(command, "snowball-api-" + flavor + "-run", 120,
                                 env={"DOTCC_DEBUG_HEAP": "1", "DOTCC_DEBUG_HEAP_SCAN": "1"})
                (artifacts / (flavor + ".txt")).write_text(actual)
                if actual != expected:
                    raise RuntimeError("Original Snowball native/emitted transcript mismatch: " + flavor)
                report["modes"][flavor] = "passed-exact-native-comparison-two-fresh-generated-owners-debug-heap"
            except Exception as error:
                report["modes"][flavor] = {"status": "failed", "error": str(error)}
                failures.append(flavor)
        if failures:
            raise RuntimeError("Snowball execution failed: " + ", ".join(failures))
        report["status"] = "passed-bounded-control"
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        write_json(artifacts / "execution.json", report)


def run(ctx, suite=None):
    from campaigns.recipes import helper
    compiler = Path(getattr(ctx, "compiler", None) or os.environ["DOTCC_COMPILER"]).resolve()
    if not compiler.is_file():
        raise RuntimeError("Select an existing immutable compiler snapshot")
    receipt_path = getattr(ctx, "snowball_receipt", None) or ctx.env.get("PGLITE_SNOWBALL_RECEIPT")
    receipt = json.loads(Path(receipt_path).read_text()) if receipt_path else ctx.receipt
    info = receipt["pglite_preparation"]
    source = Path(info["source"]).resolve()
    common_flags, include = backend_context(receipt)
    initializer = helper(ctx.root / "scripts/initializer.py")
    module = initializer.policy(ctx.root)["bootstrap_modules"]["dict_snowball"]
    actual = makefile_sources(source)
    declared = module["sources"]
    if len(declared) != len(set(declared)) or set(actual) != set(declared):
        raise RuntimeError("Initializer Snowball source inventory differs from actual upstream Makefile")
    work, artifacts = ctx.work / "snowball", ctx.artifacts / "snowball"
    work.mkdir()
    artifacts.mkdir()
    all_units = initializer.backend_module_units(ctx, info, include)
    original_paths = {source / relative for relative in declared}
    units = [unit for unit in all_units if unit.path in original_paths
             or unit.path.name == "dict_snowball-input.c"]
    selected_sources = [module["handler_wrapper"]["source"] if unit.path.name == "dict_snowball-input.c"
                        else unit.path.relative_to(source).as_posix() for unit in units]
    if sorted(selected_sources) != sorted(declared):
        raise RuntimeError("Initializer helper must provide exactly the declared original Snowball closure")
    wrapper = next(unit.path for unit in units if unit.path.name == "dict_snowball-input.c")
    original_dict = source / module["handler_wrapper"]["source"]
    expected_wrapper = ('#include "postgres.h"\n#include "fmgr.h"\n'
                        '#undef PG_MAGIC_FUNCTION_NAME\n#define PG_MAGIC_FUNCTION_NAME dpg_snowball_magic\n'
                        '#include "' + original_dict.as_posix() + '"\n')
    if wrapper.read_text() != expected_wrapper:
        raise RuntimeError("Snowball dictionary wrapper must rename only its original magic function")
    watched = {source / relative for relative in actual}
    watched.update((source / "src/include/snowball").rglob("*.h"))
    watched.update((source / "src/backend/snowball/Makefile", source / "src/include/pg_config.h"))
    watched.update(include.rglob("*.h"))
    before = {str(path): digest(path) for path in sorted(watched)}
    profile = json.loads((ctx.root / "config/profile.json").read_text())["profiles"][info["profile"]]
    native_flags = [*profile["cflags"], "-D_GNU_SOURCE", *("-D" + item for item in info["defines"]),
                    "-I", str(source / "src/include/snowball"),
                    "-I", str(source / "src/include/snowball/libstemmer"),
                    *(part for path in info["include_dirs"] for part in ("-I", str(path))),
                    "-Werror=implicit-function-declaration", "-Werror=incompatible-pointer-types"]
    report = dict(status="running", scope="Snowball original module compilation/emission and independent original API controls; campaign remains incomplete",
                  receipt=str(receipt_path) if receipt_path else "current context", source=str(source),
                  compiler=str(compiler), coherent_include=str(include), common_flags=common_flags,
                  makefile_sources=actual, declared_sources=declared,
                  source_unit_count=len(actual), stemmer_unit_count=sum(Path(item).name.startswith("stem_") for item in actual),
                  wrapper=str(wrapper), wrapper_sha256=digest(wrapper), native_flags=native_flags,
                  compiler_closure_sha256={path.name: digest(path) for path in sorted(compiler.parent.iterdir())
                                           if path.suffix in (".dll", ".json")},
                  watched_inputs_sha256=before, units=[], failures=[])
    ctx.receipt["pglite_snowball"] = {"report": str(artifacts / "report.json"), "status": "running"}
    write_json(artifacts / "inventory.json", {key: report[key] for key in
               ("source", "makefile_sources", "declared_sources", "source_unit_count", "stemmer_unit_count",
                "coherent_include", "common_flags", "native_flags", "wrapper", "wrapper_sha256")})
    try:
        for index, unit in enumerate(units):
            relative = module["handler_wrapper"]["source"] if unit.path == wrapper else unit.path.relative_to(source).as_posix()
            observation = dict(source=relative, input=str(unit.path), sha256=digest(unit.path),
                               emission_flags=[*common_flags, *unit.flags])
            for kind in ("native", "emission"):
                output = work / (f"{index:02d}-" + unit.path.stem + (".o" if kind == "native" else ".cs"))
                command = ([os.environ.get("CC", "cc"), *native_flags, "-c", unit.path, "-o", output]
                           if kind == "native" else
                           ["dotnet", compiler, *common_flags, *unit.flags, unit.path,
                            "--emit=obj", "-o", output])
                label = f"snowball-{kind}-{index:02d}-{unit.path.stem}"
                ctx.run(command, label, 180, check=False)
                record = ctx.receipt["commands"][-1]
                result = dict(exit_code=record.get("exit_code"), log=record.get("stdout"), output=str(output))
                if record.get("exit_code") != 0 or not output.is_file():
                    report["failures"].append(dict(kind=kind, source=relative, **result))
                else:
                    result["sha256"] = digest(output)
                observation[kind] = result
            report["units"].append(observation)
            write_json(artifacts / "report.json", report)
        # An actual module source failure must not hide independently available
        # execution controls. Their failures also leave their own original logs.
        try:
            report["execution"] = execution(ctx, compiler, source, work / "api", artifacts)
        except Exception as error:
            report["failures"].append(dict(kind="execution", error=str(error),
                                            report=str(artifacts / "execution.json")))
        after = {str(path): digest(path) for path in sorted(watched)}
        report["watched_inputs_unchanged"] = before == after
        report["compiler_unchanged"] = report["compiler_closure_sha256"] == {
            path.name: digest(path) for path in sorted(compiler.parent.iterdir()) if path.suffix in (".dll", ".json")}
        if not report["watched_inputs_unchanged"] or not report["compiler_unchanged"]:
            report["failures"].append(dict(kind="input-integrity", error="Readonly watched inputs changed during control"))
        report["native_passed"] = sum(item["native"]["exit_code"] == 0 for item in report["units"])
        report["emission_passed"] = sum(item["emission"]["exit_code"] == 0 for item in report["units"])
        if report["failures"]:
            raise RuntimeError("Snowball bounded controls failed; see " + str(artifacts / "report.json"))
        report["status"] = "passed-bounded-controls-only"
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        write_json(artifacts / "report.json", report)
        ctx.receipt["pglite_snowball"]["status"] = report["status"]
        ctx.save()


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--receipt", type=Path, required=True, help="Normal receipt with original coherent backend/module commands")
    parser.add_argument("--compiler", type=Path, required=True, help="Existing immutable compiler, never rebuilt")
    parser.add_argument("--mode", choices=("jit", "aot", "all"), default="all")
    parser.add_argument("--api-only", action="store_true", help="Retry only the independent API fixture; no module work")
    args = parser.parse_args()
    root = Path(__file__).resolve().parents[1]
    sys.path.insert(0, str(root.parent / "Scripts"))
    from campaigns.model import Recipe
    from campaigns.runner import Context
    options = SimpleNamespace(profile="embedding-native", action="snowball-probe", hashes="warn",
                              fetch="never", restore="normal", mode=args.mode)
    recipe = Recipe("pglite", "TranslatedPGlite", ("embedding-native",), (), lambda root: ())
    ctx = Context(root.parent, recipe, options)
    ctx.compiler = args.compiler.resolve()
    ctx.snowball_receipt = args.receipt.resolve()
    ctx.start()
    try:
        if args.api_only:
            receipt = json.loads(ctx.snowball_receipt.read_text())
            artifacts = ctx.artifacts / "snowball"
            artifacts.mkdir()
            execution(ctx, ctx.compiler, Path(receipt["pglite_preparation"]["source"]),
                      ctx.work / "snowball-api", artifacts)
        else:
            run(ctx)
        ctx.receipt["status"] = "passed"
    except BaseException as error:
        ctx.receipt.update(status="failed", error=str(error))
        raise
    finally:
        # Do not mutate the shared campaign latest-attempt pointer.
        ctx.receipt["finished"] = time.time()
        ctx.save()


if __name__ == "__main__":
    main()
