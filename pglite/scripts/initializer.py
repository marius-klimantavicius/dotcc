"""Readonly source closure and actual translated initdb discovery.

Framework hooks only: no acquisition, make, compiler builds, native database
execution, publication, or edits to the configured native stage. The supervisor
owns recipe integration. Source/capability selection is reviewable owned JSON.
"""
import hashlib
import json
import os
from pathlib import Path
import re
import shutil

from campaigns.model import Unit
from campaigns.recipes import helper, json_file, link_flags


def policy(root):
    return json_file(Path(root) / "src/Initialization/frontend-closure.json")


def configured(ctx):
    selected = os.environ.get("PGLITE_INITIALIZATION_PREPARATION")
    if selected:
        info = json_file(Path(selected))
    else:
        candidates = sorted((ctx.root / "build/native-control").glob("*/prepared.json"),
                            key=lambda path: path.stat().st_mtime_ns, reverse=True)
        pin = json_file(ctx.root / "config/sources.json")["product"]
        matching = [json_file(path) for path in candidates
                    if json_file(path).get("requested_revision") == pin["revision"]
                    and json_file(path).get("profile") == "embedding-native"]
        if not matching:
            raise RuntimeError("Supervisor must provide an existing configured PGlite stage")
        info = matching[0]
    expected = policy(ctx.root)
    if info.get("requested_revision") != expected["revision"] or info.get("profile") != expected["profile"]:
        raise RuntimeError("Initializer metadata requires a reviewed source/profile upgrade")
    return info


def prepare_frontend_overlay(root, source, destination):
    """Frontend-only guarded config correction; never change backend inputs.

    Returns the exact before/after macro contract and ordered flags. PostgreSQL
    supplies the real option-parser algorithms in src/port/getopt*.c.
    """
    source, destination = Path(source).resolve(), Path(destination).resolve()
    if destination == source or source in destination.parents or destination in source.parents:
        raise RuntimeError("Initializer overlay must be independent of configured source")
    selection = policy(root)
    original = source / "src/include"
    header = (original / "pg_config.h").read_text()
    for macro, (before, _after) in selection["frontend_configuration"].items():
        pattern = rf"^#define {re.escape(macro)} {re.escape(before)}$"
        if len(re.findall(pattern, header, flags=re.MULTILINE)) != 1:
            count = len(re.findall(pattern, header, flags=re.MULTILINE))
            raise RuntimeError(f"Frontend capability expects exactly one {macro}={before}, got {count}")
    shutil.copytree(original, destination, symlinks=False)
    shared = helper(Path(root) / "scripts/portability.py").prepare_overlay(root, destination)
    staged = (destination / "pg_config.h").read_text()
    for macro, (_before, after) in selection["frontend_configuration"].items():
        expected = f"/* #undef {macro} */" if after is None else f"#define {macro} {after}"
        if staged.splitlines().count(expected) != 1:
            raise RuntimeError("Shared portability selection does not satisfy frontend contract: " + macro)
    return dict(include=str(destination), configuration=selection["frontend_configuration"],
                shared_portability=shared,
                source_mutated=False, frontend_only=True,
                original_sha256=hashlib.sha256(header.encode()).hexdigest(),
                staged_sha256=hashlib.sha256(staged.encode()).hexdigest())


def frontend_units(root, info, include):
    """Exact selected per-unit source, ordered includes, and defines.

    Frontend common variants stay separate from backend objects. All libpq
    callers use the same private encoding names within this separate library.
    Raw native contexts and both link-map inventories remain in the metadata.
    """
    source = Path(info["source"])
    result = []
    selected_units = list(policy(root)["units"])
    selected_paths = {unit["source"] for unit in selected_units}
    for path in json_file(Path(root) / "config/portable-codegen.json")["portable_sources"]:
        if path not in selected_paths:
            selected_units.append(dict(source=path, role="upstream-portable-fallback",
                                       defines=["FRONTEND", "USE_PRIVATE_ENCODING_FUNCS", "JSONAPI_USE_PQEXPBUFFER"],
                                       includes=["{cwd}", "src/include", "src/interfaces/libpq", "src/port", "src/common"]))
            selected_paths.add(path)
    for selected in selected_units:
        path = source / selected["source"]
        if not path.is_file():
            raise RuntimeError("Missing actual initializer source: " + str(path))
        includes = [str(include), *(str(path.parent if item == "{cwd}" else source / item)
                                   for item in selected["includes"])]
        defines = ["_GNU_SOURCE", *info["defines"], *selected["defines"]]
        result.append(dict(source=str(path), relative_source=selected["source"],
                           includes=includes, defines=list(dict.fromkeys(defines)), role=selected["role"]))
    return result


def backend_module_configuration(root):
    """Per-dfmgr remaps only; preserve its original module checks and cache.

    Apply these defines to src/backend/utils/fmgr/dfmgr.c, not to the provider
    or PL/pgSQL sources. All module units must share the backend state context.
    """
    return dict(includes=[str(Path(root) / "src/Initialization")],
                dfmgr_source="src/backend/utils/fmgr/dfmgr.c",
                dfmgr_defines=[name + "=dpg_static_" + name
                               for name in ("dlopen", "dlsym", "dlclose", "dlerror")],
                register="dpg_static_register(path, dpg_plpgsql_module())",
                module_relative_path="lib/postgresql/plpgsql",
                registration_helper="InitializationBackend.RegisterModules(bootstrapPrefix)",
                modules=[dict(name="plpgsql", provider="dpg_plpgsql_module", path="lib/postgresql/plpgsql"),
                         dict(name="dict_snowball", provider="dpg_snowball_module", path="lib/postgresql/dict_snowball")],
                same_backend_context=True)


def backend_module_units(ctx, info, include):
    """Original PL/pgSQL/Snowball plus forwarding catalogs/static provider.

    The caller supplies its already reviewed backend include overlay. No
    acquisition, configure/make, compilation or shared-output mutation occurs.
    """
    source, include = Path(info["source"]), Path(include)
    modules = policy(ctx.root)["bootstrap_modules"]
    work = ctx.work / "initialization/module-inputs"
    work.mkdir(parents=True, exist_ok=True)
    flags = [*("-D" + item for item in info["defines"]), "-I", str(include),
             "-I", str(source / "src/pl/plpgsql/src"),
             "-I", str(ctx.root / "src/Initialization")]
    units = []
    for module_name, selection in modules.items():
        selected_flags = [*flags, *(part for name in selection.get("extra_include_paths", [])
                                   for part in ("-I", str(source / name)))]
        for name in selection["sources"]:
            original = source / name
            if not original.is_file():
                raise RuntimeError("Missing original " + module_name + " source: " + str(original))
            path = original
            if original.name == "pl_handler.c":
                text = original.read_text()
                if text.count("PG_MODULE_MAGIC_EXT(") != 1 or text.count("_PG_init(void)") != 1:
                    raise RuntimeError("PL/pgSQL export wrapper requires reviewed magic/initializer declarations")
                path = work / "pl_handler-input.c"
                path.write_text('#include "postgres.h"\n#include "fmgr.h"\n'
                                '#undef PG_MAGIC_FUNCTION_NAME\n#define PG_MAGIC_FUNCTION_NAME dpg_plpgsql_magic\n'
                                '#define _PG_init dpg_plpgsql_init\n#include "' + original.as_posix() + '"\n')
            elif original.name == "dict_snowball.c":
                text = original.read_text()
                if text.count("PG_MODULE_MAGIC_EXT(") != 1 or "_PG_init(" in text:
                    raise RuntimeError("Snowball export wrapper requires reviewed original magic and no initializer")
                path = work / "dict_snowball-input.c"
                path.write_text('#include "postgres.h"\n#include "fmgr.h"\n'
                                '#undef PG_MAGIC_FUNCTION_NAME\n#define PG_MAGIC_FUNCTION_NAME dpg_snowball_magic\n'
                                '#include "' + original.as_posix() + '"\n')
            units.append(Unit(path, tuple(selected_flags)))
    for name in ("dotcc_plpgsql_module.c", "dotcc_snowball_module.c", "dotcc_static_modules.c"):
        units.append(Unit(ctx.root / "src/Initialization" / name, tuple(flags)))
    return units


def stage_bootstrap_data(root, info, destination):
    """Copy the exact upstream data closure into a fresh private prefix.

    Native engine/module libraries are never copied. The PL/pgSQL marker is an
    authored static-provider manifest used for dfmgr's real file/path checks;
    the host must register the corresponding original translated exports.
    """
    source, destination = Path(info["prefix"]).resolve(), Path(destination).resolve()
    if destination == source or source in destination.parents or destination in source.parents:
        raise RuntimeError("Bootstrap data destination must be independent of the native prefix")
    if destination.exists():
        raise RuntimeError("Bootstrap data destination must be fresh")
    entries = []
    for selected in policy(root)["bootstrap_assets"]:
        relative = Path(selected["path"])
        if relative.is_absolute() or ".." in relative.parts:
            raise RuntimeError("Invalid bootstrap data path: " + str(relative))
        path = source / relative
        if not path.exists():
            raise RuntimeError("Missing bootstrap data: " + str(path))
        files = sorted(path.rglob("*")) if path.is_dir() else [path]
        for item in files:
            if item.is_file():
                entries.append((item, item.relative_to(source)))
    # Validate all required source paths before creating any destination.
    destination.mkdir(parents=True)
    manifest = []
    for item, relative in entries:
        output = destination / relative
        output.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(item, output)
        manifest.append(dict(path=relative.as_posix(), bytes=output.stat().st_size,
                             sha256=hashlib.sha256(output.read_bytes()).hexdigest(), kind="upstream-bootstrap-data"))
    manifest.extend(helper(Path(root) / "scripts/notices.py").stage(root, info, destination))
    for name, module in policy(root)["bootstrap_modules"].items():
        descriptor = Path(root) / "src/Initialization" / (name + ".module.json")
        relative = module["descriptor"]["installed_relative_path"]
        output = destination / relative
        output.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(descriptor, output)
        manifest.append(dict(path=relative, bytes=output.stat().st_size,
                             sha256=hashlib.sha256(output.read_bytes()).hexdigest(), kind="translated-static-module-manifest"))
    # Original find_my_exec/find_other_exec validate regular executable paths
    # before invoking the fork's in-process version callback. These descriptors
    # identify managed entrypoints; they contain no shell/native executable.
    for name, entry in (("initdb", "Managed.PGlite.Initialization.Translated.Initdb.main"),
                        ("postgres", "Managed.PGlite.Translated.PGlite.main")):
        relative = "bin/" + name
        output = destination / relative
        output.parent.mkdir(parents=True, exist_ok=True)
        _write(output, dict(kind="translated-entrypoint-descriptor", entry=entry,
                            native_executable=False))
        if os.name != "nt":
            output.chmod(0o700)
        manifest.append(dict(path=relative, bytes=output.stat().st_size,
                             sha256=hashlib.sha256(output.read_bytes()).hexdigest(),
                             kind="translated-entrypoint-descriptor"))
    return dict(prefix=str(destination), files=manifest, native_libraries_copied=False,
                module_execution_qualified=False)


def run_contracts(ctx, info, include):
    """Native/emitted execution of original getopt algorithms and a static
    symbol-provider boundary. Not a PostgreSQL initialization claim."""
    work = ctx.work / "initialization/contracts"
    work.mkdir(parents=True, exist_ok=True)
    source, include = Path(info["source"]), Path(include)
    tests, authored = ctx.root / "tests/Initialization", ctx.root / "src/Initialization"
    options = [source / "src/port/getopt.c", source / "src/port/getopt_long.c",
               source / "src/port/snprintf.c", source / "src/port/strerror.c", tests / "getopt_probe.c"]
    modules = [authored / "dotcc_static_modules.c", tests / "static_module_probe.c"]
    inputs = options + modules
    flags = ["-std=c11", "-D_GNU_SOURCE", "-DFRONTEND", "-I", include, "-I", authored]
    # Use already-built native formatting support, whose GNU strerror_r ABI
    # matches native libc. The emitted formatter below uses DotCC's POSIX ABI.
    ctx.run(["cc", *flags, "-DDPG_NATIVE_MAIN", *options[:2], options[-1],
             source / "src/port/libpgport.a", "-lm", "-o", work / "native-getopt"], "initdb-getopt-native-build")
    ctx.run([work / "native-getopt"], "initdb-getopt-native-run")
    ctx.run(["cc", *flags, "-DDPG_NATIVE_MAIN", *modules, "-o", work / "native-static-modules"], "initdb-static-native-build")
    ctx.run([work / "native-static-modules"], "initdb-static-native-run")
    objects = []
    for index, path in enumerate(inputs):
        obj = work / f"{index}.cs"
        ctx.run(["dotnet", ctx.compiler, path, *flags, "--emit=obj", "-o", obj], f"initdb-contract-unit-{index}")
        objects.append(obj)
    output = work / "getopt"
    ctx.run(["dotnet", ctx.compiler, *objects, "--emit=managedlib", "--state-context",
             *link_flags("OptionProbe", "Managed.PGlite.Tests"), "-o", output], "initdb-contract-link")
    ctx.managed("build", tests / "GetoptTests.csproj", "initdb-contract-build", "-m:1",
                "-p:GetoptProject=" + str(output / "getopt.csproj"), "--artifacts-path", work / "managed")
    ctx.run(["dotnet", work / "managed/bin/GetoptTests/release/GetoptTests.dll"], "initdb-contract-run")


def run_frontend_entrypoints(ctx, project, *, aot=False):
    """Execute original option paths through the concrete owning adapter.

    No backend replacement is supplied. Any backend invocation by these
    option-only cases fails explicitly; this never claims catalog bootstrap.
    """
    work = ctx.work / "initialization/frontend-entrypoints"
    project = Path(project).resolve()
    tests = ctx.root / "tests/Initialization/FrontendTests.csproj"
    properties = ["-m:1", "-p:InitdbProject=" + str(project),
                  "--artifacts-path", work / "managed"]
    ctx.managed("build", tests, "initdb-original-entrypoints-build", *properties)
    ctx.run(["dotnet", work / "managed/bin/FrontendTests/release/FrontendTests.dll"],
            "initdb-original-entrypoints-run")
    if aot:
        ctx.managed("publish", tests, "initdb-original-entrypoints-aot-publish", *properties,
                    "-r", "linux-x64", "-p:PublishAot=true", "-o", work / "aot")
        ctx.run([work / "aot/FrontendTests"], "initdb-original-entrypoints-aot-run")


def run_actual_initialization(ctx, frontend_project, backend_project, prefix, data_directory=None):
    """Build the owning harness and invoke original initdb with real backend.

    backend_project must include the complete backend, module units and authored
    backend adapter. Missing implementations fail at build/run; no native or
    byte-fixture fallback is possible. The caller owns campaign acceptance.
    """
    work = ctx.work / "initialization/execution"
    work.mkdir(parents=True, exist_ok=True)
    data = Path(data_directory) if data_directory else work / "pgdata"
    if data.exists():
        raise RuntimeError("Actual initialization requires a fresh private data directory")
    artifacts = ctx.artifacts / "initialization/execution"
    artifacts.mkdir(parents=True, exist_ok=True)
    ctx.managed("build", ctx.root / "tests/Initialization/Initialize.csproj",
                "initdb-actual-invocation-build", "-m:1",
                "-p:InitdbProject=" + str(Path(frontend_project).resolve()),
                "-p:BackendProject=" + str(Path(backend_project).resolve()),
                "--artifacts-path", work / "managed")
    ctx.run(["dotnet", work / "managed/bin/Initialize/release/Initialize.dll",
             Path(prefix).resolve(), data.resolve(), artifacts.resolve()], "initdb-actual-invocation-run", 900)
    return dict(data_directory=str(data.resolve()), bootstrap_prefix=str(Path(prefix).resolve()),
                observations=str(artifacts / "backend-invocations.json"),
                scope="original translated initdb/bootstrap/single-user invocation; SQL product qualification remains required")


def run(ctx, suite=None):
    """Attempt every real frontend unit, retaining failures instead of claiming
    object emission is a working initializer. A failed closure gate raises after
    all independent unit attempts; the framework receipt stays failed.
    """
    work, artifacts = ctx.work / "initialization", ctx.artifacts / "initialization"
    work.mkdir(parents=True, exist_ok=True)
    artifacts.mkdir(parents=True, exist_ok=True)
    report = dict(scope="original initdb/frontend/libpq closure diagnostics; no product or SQL qualification",
                  passed=[], failed=[], unrun=["translated initdb execution", "bootstrap", "post-bootstrap SQL",
                                               "PL/pgSQL registration", "JIT/AOT product matrix"])
    try:
        if ctx.profile != "embedding-native":
            raise RuntimeError("Initializer discovery requires embedding-native LP64")
        info = configured(ctx)
        source = Path(info["source"])
        compiler = Path(getattr(ctx, "compiler", None) or os.environ["DOTCC_COMPILER"]).resolve()
        if not compiler.is_file():
            raise RuntimeError("Select an immutable compiler; initializer never builds tools")
        report.update(compiler=str(compiler), compiler_sha256=hashlib.sha256(compiler.read_bytes()).hexdigest(),
                      configured_source=str(source), requested_revision=info["requested_revision"])
        original_hash = hashlib.sha256((source / "src/include/pg_config.h").read_bytes()).hexdigest()
        overlay = prepare_frontend_overlay(ctx.root, source, work / "include")
        report["frontend_configuration"] = overlay
        try:
            run_contracts(ctx, info, work / "include")
            report["contracts"] = "passed native/emitted original option parsing, owner globals and static symbol transport"
        except Exception as error:
            # A shared compiler/runtime failure in an independent contract
            # must not prevent discovery against the actual source closure.
            report["contracts"] = "failed"
            report["contract_error"] = str(error)
        units = frontend_units(ctx.root, info, work / "include")
        _write(artifacts / "frontend-units.json", units)
        data = stage_bootstrap_data(ctx.root, info, work / "bootstrap-prefix")
        _write(artifacts / "bootstrap-data.json", data)
        report["staged_bootstrap_prefix"] = data["prefix"]
        assets = []
        for asset in policy(ctx.root)["bootstrap_assets"]:
            path = Path(info["prefix"]) / asset["path"]
            if not path.exists():
                raise RuntimeError("Missing bootstrap input: " + str(path))
            assets.append(dict(asset, observed_path=str(path)))
        report["bootstrap_assets"] = assets
        objects = []
        for index, unit in enumerate(units):
            output = work / f"{index:03d}-{Path(unit['source']).stem}.cs"
            flags = ["-std=c11", *("-D" + item for item in unit["defines"])]
            for include in unit["includes"]:
                flags.extend(["-I", include])
            label = f"initdb-unit-{index:03d}-{Path(unit['source']).stem}"
            ctx.run(["dotnet", compiler, unit["source"], *flags, "--emit=obj", "-o", output],
                    label, 90, check=False)
            command = ctx.receipt["commands"][-1]
            observation = dict(source=unit["relative_source"], exit_code=command.get("exit_code"),
                               log=command.get("stdout"))
            if command.get("exit_code") == 0:
                report["passed"].append(observation)
                objects.append(output)
            else:
                report["failed"].append(observation)
        if hashlib.sha256((source / "src/include/pg_config.h").read_bytes()).hexdigest() != original_hash:
            raise RuntimeError("Readonly configured source changed during initializer discovery")
        if report["failed"]:
            raise RuntimeError(f"Actual initializer closure: {len(report['failed'])}/{len(units)} units failed; see initialization/report.json")
        # Complete actual libc is separately compiled without interception
        # remaps, exactly as required by the fork's build contract.
        wrapper = helper(ctx.root / "scripts/libc-input.py").prepare_input(ctx, source)
        for label, path in (("libc", wrapper), ("adapter", ctx.root / "src/Initialization/dotcc_pglite_initdb.c")):
            obj = work / (label + ".cs")
            ctx.run(["dotnet", compiler, path, "-std=c11", "--emit=obj", "-o", obj], "initdb-" + label)
            objects.append(obj)
        output = work / "frontend"
        ctx.run(["dotnet", compiler, *objects, "--emit=managedlib", "--state-context",
                 *link_flags("Initdb", "Managed.PGlite.Initialization.Translated"), "-o", output], "initdb-link")
        ctx.managed("build", output / "frontend.csproj", "initdb-managed-build", "-m:1",
                    "--artifacts-path", work / "managed")
        run_frontend_entrypoints(ctx, output / "frontend.csproj")
        if report["contracts"] == "failed":
            raise RuntimeError("Frontend builds but required option-parser/static-module contracts failed")
        report["frontend_project"] = str(output / "frontend.csproj")
        report["original_main_options"] = "passed help, version, failed-option, exit/stdio and fresh-owner cleanup"
        report["status"] = "passed-frontend-build-and-original-option-entrypoints"
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        _write(artifacts / "report.json", report)


def _write(path, value):
    path.write_text(json.dumps(value, indent=2) + "\n")


def run_modules(ctx, suite=None):
    """Native compilation and actual emission of required PL/pgSQL/Snowball
    units and their static catalogs/provider. Linking/execution requires the real
    complete backend, owned by the supervisor; a module object is not bootstrap.
    """
    info = configured(ctx)
    source = Path(info["source"])
    work, artifacts = ctx.work / "plpgsql", ctx.artifacts / "plpgsql"
    work.mkdir(parents=True, exist_ok=True)
    artifacts.mkdir(parents=True, exist_ok=True)
    port = helper(ctx.root / "scripts/atomics.py").prepare_overlay(source, work / "include")
    shared = helper(ctx.root / "scripts/portability.py").prepare_overlay(ctx.root, port["include"])
    report = dict(scope="original required PL/pgSQL/Snowball source/static-loader closure; no module execution or bootstrap claim",
                  overlay=port, portability=shared, units=[], status="running")
    compiler = Path(getattr(ctx, "compiler", None) or os.environ["DOTCC_COMPILER"]).resolve()
    try:
        units = backend_module_units(ctx, info, work / "include")
        report["backend_integration"] = backend_module_configuration(ctx.root)
        for index, unit in enumerate(units):
            path = unit.path
            flags = ["-std=c11", "-D_GNU_SOURCE", *("-D" + item for item in port["defines"]), *unit.flags]
            ctx.run(["cc", *flags, "-c", path, "-o", work / f"{index}.o"], f"plpgsql-native-{index}", 120, check=False)
            native = ctx.receipt["commands"][-1]
            observation = dict(source=str(path), flags=list(map(str, flags)),
                               native_exit_code=native.get("exit_code"), native_log=native.get("stdout"))
            ctx.run(["dotnet", compiler, path, *flags, "--emit=obj", "-o", work / f"{index}.cs"],
                    f"plpgsql-emission-{index}", 120, check=False)
            emitted = ctx.receipt["commands"][-1]
            observation.update(emission_exit_code=emitted.get("exit_code"), emission_log=emitted.get("stdout"))
            report["units"].append(observation)
        if any(item["native_exit_code"] != 0 or item["emission_exit_code"] != 0 for item in report["units"]):
            raise RuntimeError("Required bundled module source/static export closure remains blocked; see plpgsql/report.json")
        report["status"] = "passed-native-compilation-and-emission-only"
        return report
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        _write(artifacts / "report.json", report)
