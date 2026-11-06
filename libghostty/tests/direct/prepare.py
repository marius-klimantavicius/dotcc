"""Prepare genuine upstream Zig inputs; never emit or consume a C engine.

Framework entry point: ``prepare_direct(ctx)``. Acquisition and every command
use the campaign Context. The returned Unit objects are source-ready inputs,
not a translated product. ``module_graph_file`` is the generic compiler input;
Only explicit root output/optimization context enters the graph; remaining
native target/options stay separate, unapplied audit data.
"""
import json
from pathlib import Path
import re
import shutil

from campaigns.model import Unit
from campaigns.recipes import helper, json_file


_MODULE_PAIRS = {"-target", "-mcpu", "-I", "-isystem", "-idirafter", "-D", "-U"}
_OPTIMIZE_MODES = dict(debug="debug", safe="safe", fast="fast", small="small",
                       Debug="debug", ReleaseSafe="safe", ReleaseFast="fast", ReleaseSmall="small")


def _optimization_context(options):
    """Select one explicit root -O; absent context leaves the legacy default.

    Preserve the actual spelling in audit metadata when normalizing older
    build-command spellings. Options of dependencies are not root settings.
    """
    selected = None
    args = iter(enumerate(options))
    for index, option in args:
        if option in _MODULE_PAIRS:
            if next(args, None) is None:
                raise RuntimeError("Missing native argument after " + option)
        elif option.startswith("-O"):
            mode = _OPTIMIZE_MODES.get(option[2:])
            if mode is None:
                raise RuntimeError("Unknown native optimization option: " + option)
            if selected is not None:
                raise RuntimeError("Ambiguous native root optimization options")
            selected = dict(native_module="root", native_option=option, native_option_index=index, native_mode=option[2:],
                            optimize_mode=mode, normalized=option[2:] != mode,
                            builtin_scope="root/shared-std")
    return selected


def _write(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def _absolute(value, cwd):
    path = Path(value)
    return str((path if path.is_absolute() else cwd / path).resolve())


def native_modules(argv, cwd):
    """Decode ordered native -M modules and the --dep scope preceding each.

    Fail on unknown arguments rather than silently dropping module semantics.
    Global linker/cache options are returned independently. This is audit data,
    not a claim that DotCC implements native codegen/target switches.
    """
    cwd = Path(cwd).resolve()
    if len(argv) < 2 or argv[1] != "build-lib":
        raise RuntimeError("Expected the actual native build-lib invocation")
    modules, dependencies, options, global_options = [], [], [], []
    global_pairs = {"--cache-dir", "--global-cache-dir", "--build-root", "--name", "--zig-lib-dir"}
    global_flags = {"-static", "-dynamic", "-fcompiler-rt", "-fno-compiler-rt", "-fubsan-rt", "-fno-ubsan-rt"}
    args = iter(map(str, argv[2:]))

    def following(flag):
        value = next(args, None)
        if value is None:
            raise RuntimeError("Missing native argument after " + flag)
        return value

    for arg in args:
        if arg == "--dep":
            value = following(arg)
            alias, _, target = value.partition("=")
            if not alias or ("=" in value and not target):
                raise RuntimeError("Invalid native dependency: " + value)
            dependencies.append({"name": alias, "module": target or alias})
        elif arg.startswith("-M"):
            name, equal, root = arg[2:].partition("=")
            if not name or not equal or not root:
                raise RuntimeError("Invalid native module: " + arg)
            modules.append(dict(name=name, root=_absolute(root, cwd),
                                dependencies=dependencies, options=options))
            dependencies, options = [], []
        elif arg in global_pairs:
            global_options.extend((arg, following(arg)))
        elif arg in global_flags or arg.startswith("-femit-bin="):
            global_options.append(arg)
        elif arg in _MODULE_PAIRS:
            options.extend((arg, following(arg)))
        elif (re.fullmatch(r"-O(?:debug|fast|safe|small|Debug|ReleaseFast|ReleaseSafe|ReleaseSmall)", arg)
              or arg in {"-fPIC", "-fno-PIC", "-lc"}
              or arg.startswith(("-I", "-D", "-U", "-mcpu=", "-m", "-f", "-l"))):
            options.append(arg)
        else:
            raise RuntimeError("Unclassified native closure argument: " + arg)
    if dependencies or options:
        raise RuntimeError("Native module options/dependencies have no following -M module")
    names = [row["name"] for row in modules]
    if not names or names[0] != "root" or len(names) != len(set(names)):
        raise RuntimeError("Expected a unique native root module followed by unique dependencies")
    for row in modules:
        aliases = [dep["name"] for dep in row["dependencies"]]
        if len(aliases) != len(set(aliases)):
            raise RuntimeError("Duplicate dependency alias in module " + row["name"])
        for dep in row["dependencies"]:
            if dep["module"] not in names:
                raise RuntimeError("Missing native dependency module: " + dep["module"])
        if not Path(row["root"]).is_file():
            raise RuntimeError("Missing native module root: " + row["root"])
    return modules, global_options


def _native(ctx, work):
    """Use existing acquisition/version/verbose helpers, with private caches.

    _prepare_native chooses a shared global cache when hashing is enabled.
    This small native-build-only entry instead keeps both caches in ctx.work
    without changing the caller's hash policy or acquisition metadata.
    """
    native = helper(ctx.root / "tests/native/run.py")
    source, zig = native._source(ctx).resolve(), native._zig(ctx)
    config = json_file(ctx.root / "config/translation.json")
    pin = json_file(ctx.root / "config/source.json")
    work.mkdir(parents=True, exist_ok=True)
    prefix, cache, global_cache = work / "install", work / "zig-cache", work / "zig-global-cache"
    cmd = [zig, "build", *config["native_build_arguments"], "--prefix", prefix,
           "--cache-dir", cache, "-j2", "--verbose",
           "-Dversion-string=" + pin["upstream_version"],
           "-Dlib-version-string=" + pin["library_version"]]
    if ctx.options.fetch == "never":
        cmd += ["--system", source / "zig-pkg"]
        cmd += ["-fno-sys=" + name for name in pin["build_system_integrations"]]
    _write(work / "native-build-command.json", list(map(str, cmd)))
    log = ctx.run(cmd, "direct-native-upstream-build", timeout=1800, cwd=source,
                  env={"ZIG_GLOBAL_CACHE_DIR": str(global_cache)})
    argv = native._engine_command(log, zig, source)
    _write(work / "native-engine-command.json", argv)
    libraries = list((prefix / "lib").glob("libghostty-vt.a"))
    if not libraries:
        libraries = list((prefix / "lib").glob("*ghostty-vt*.lib"))
    if len(libraries) != 1:
        raise RuntimeError("Expected one independently built original native library")
    return dict(upstream=source, zig=zig, argv=argv, command=list(map(str, cmd)),
                native_library=libraries[0])


def stage_actual(root, upstream, work, relative_root):
    """Copy src, then append the existing metadata-only ABI import to actual.zig.

    The original root guard and all TopComptime exports remain byte-for-byte.
    The entry is the actual lib_vt contents, not a wrapper importing lib_vt.
    """
    original = upstream / relative_root
    data = original.read_bytes()
    if (data.count(b"const lib = @This();") != 1
            or data.count(b'if (@import("root") == lib) {') != 1
            or b"dotcc_ghostty_api.zig" in data):
        raise RuntimeError("Unexpected upstream actual-root/export guard for ABI extension")
    staged = work / "source" / "src"
    if staged.exists():
        raise RuntimeError("Direct source staging already exists; use a fresh Context work directory")
    shutil.copytree(upstream / "src", staged)
    adapter = helper(root / "scripts/abi.py").generate(root, work / "abi")
    shutil.copyfile(adapter / "dotcc_ghostty_api.zig", staged / "dotcc_ghostty_api.zig")
    entry = staged / "actual.zig"
    appendix = (b"\n// Authored ABI address accessor; upstream exports above are preserved.\n"
                b'comptime { _ = @import("dotcc_ghostty_api.zig"); }\n')
    if entry.exists():
        raise RuntimeError("Upstream source already owns actual.zig")
    entry.write_bytes(data + appendix)
    if original.read_bytes() != data or entry.read_bytes() != data + appendix:
        raise RuntimeError("Actual root append-only staging invariant failed")
    return entry, adapter, dict(original_bytes=len(data), appended_bytes=len(appendix),
                                original_root=str(original), guard_preserved=True,
                                original_prefix_equal=True)


def _validate_options(config, modules):
    by_name = {row["name"]: row for row in modules}
    build = Path(by_name["build_options"]["root"]).read_text(encoding="utf-8")
    terminal = Path(by_name["terminal_options"]["root"]).read_text(encoding="utf-8")

    def boolean(text, name, expected):
        matches = re.findall(r"^pub const " + re.escape(name) + r": bool = (true|false);$", text, re.M)
        if matches != [str(expected).lower()]:
            raise RuntimeError("Native generated option differs from selected profile: " + name)

    boolean(build, "simd", False)
    boolean(terminal, "simd", False)
    boolean(terminal, "c_abi", True)
    for name in config["features"]:
        boolean(terminal, name, True)
    for name in config["excluded_features"]:
        boolean(terminal, {"tmux": "tmux_control_mode"}.get(name, name), False)
    return [{"name": name, "root": by_name[name]["root"]}
            for name in ("build_options", "terminal_options")]


def prepare_direct(ctx):
    """Return actual.zig Unit, separate C ABI Unit, and exact module manifests.

    The strict module graph can be passed to --zig-modules-file once that generic
    CLI seam exists. Target/builtin parity remains an explicit integration need.
    No compiler invocation, build of managed products, or publication occurs.
    """
    previous = getattr(ctx, "_libghostty_direct", None)
    if previous is not None:
        return previous
    config = json_file(ctx.root / "config/translation.json")
    pin = json_file(ctx.root / "config/source.json")
    if not isinstance(config["aggregate_data"], bool) or config["maximum_factory_parameters"] != 2:
        raise RuntimeError("Unexpected aggregate-data/factory contract")
    work = (ctx.work / "direct").resolve()
    native = _native(ctx, work / "native")
    modules, globals_ = native_modules(native["argv"], native["upstream"])
    original_root = (native["upstream"] / config["zig_root"]).resolve()
    if Path(modules[0]["root"]) != original_root:
        raise RuntimeError("Native engine root differs from the configured actual Zig root")
    optimization = _optimization_context(modules[0]["options"])
    build_options = _validate_options(config, modules)
    entry, adapter, staging = stage_actual(ctx.root, native["upstream"], work, config["zig_root"])
    graph_modules = []
    for row in modules:
        path = Path(row["root"])
        if row["name"] == "root":
            path = entry
        elif path.is_relative_to(native["upstream"] / "src"):
            path = entry.parent / path.relative_to(native["upstream"] / "src")
        graph_modules.append(dict(name=row["name"], root=str(path), dependencies=row["dependencies"]))
    lib = (native["zig"].parent / "lib").resolve()
    # native_modules requires build-lib. Keep this source compilation context
    # independent of DotCC's intermediate object emission mode.
    graph = dict(schema_version=1, root_module="root", zig_lib_dir=str(lib),
                 output_mode="Lib", modules=graph_modules)
    if optimization is not None:
        graph["optimize_mode"] = optimization["optimize_mode"]
    graph_file = work / "module-graph.json"
    _write(graph_file, graph)
    includes = (lib, native["upstream"] / "include", adapter)
    roots = {row["name"]: row["root"] for row in graph_modules}
    native_probe = ["-M" + arg[2:].partition("=")[0] + "=" + roots[arg[2:].partition("=")[0]]
                    if arg.startswith("-M") else arg for arg in native["argv"]
                    if not arg.startswith("-femit-bin=")]
    # A replay must not write the default archive beside the acquired source.
    native_probe.append("-fno-emit-bin")
    unit = Unit(entry)
    boundary = Unit(ctx.root / config["bridge"], ("-DDOTCC_GHOSTTY_INDIRECT_ABI=1",))
    audit = dict(schema_version=1, status="source-ready", source_revision=pin["revision"],
                 original_root=str(original_root), actual_root=str(entry), staging=staging,
                 module_graph_file=str(graph_file), native_modules=modules, native_global_options=globals_,
                 native_build_command=native["command"], native_engine_command=native["argv"],
                 native_cwd=str(native["upstream"]), direct_native_command=native_probe,
                 native_library=str(native["native_library"]),
                 build_options=build_options, includes=list(map(str, includes)),
                 native_target=(modules[0]["options"][modules[0]["options"].index("-target") + 1]
                                if "-target" in modules[0]["options"] else "native"),
                 c_backend_comparison_target=config.get("c_target"), compiler_target_applied=False,
                 compiler_output_mode="Lib", compiler_output_mode_applied=True,
                 compiler_optimize_mode=graph.get("optimize_mode"),
                 compiler_optimization_applied=optimization is not None,
                 compiler_optimization_context=optimization,
                 unapplied_native_module_options=[dict(name=row["name"], options=[
                     option for index, option in enumerate(row["options"])
                     if not (row["name"] == "root" and optimization is not None
                             and index == optimization["native_option_index"])]) for row in modules],
                 compiler_link_libc_applied=False, compiler_zig_version_applied=False,
                 aggregate_data=config["aggregate_data"], maximum_factory_parameters=2,
                 class_name="GhosttyVt", namespace="Managed", engine_language="zig",
                 generated_c_engine=False, units=[str(unit.path), str(boundary.path)],
                 boundary_flags=list(boundary.flags),
                 integration_needs=["generic per-request named-module mapping (--zig-modules-file)",
                                    "actual TopComptime exports and referenced function body closure",
                                    "native target, link_libc and Zig version builtin context parity",
                                    "per-module optimization contexts beyond root/shared-std and native safety/codegen semantics"],
                 upstream_test_matrix="unchanged inventory; required in direct phase, not executed here")
    manifest_file = work / "direct-preparation.json"
    _write(manifest_file, audit)
    ctx.receipt["libghostty_direct_preparation"] = dict(manifest=str(manifest_file),
                                                       status="source-ready", product_translated=False)
    result = dict(source=entry, unit=unit, boundary_unit=boundary, units=[unit, boundary],
                  upstream=native["upstream"], zig=native["zig"], includes=includes,
                  adapter=adapter, module_graph=graph, module_graph_file=graph_file,
                  manifest_file=manifest_file, native=native, native_modules=modules,
                  build_options=build_options, aggregate_data=config["aggregate_data"],
                  maximum_factory_parameters=2)
    ctx._libghostty_direct = result
    return result
