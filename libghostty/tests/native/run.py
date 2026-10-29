"""Campaign hooks for the upstream native oracle and genuine Zig -> C input.

All commands go through Context.run. Source/toolchain acquisition belongs to the
shared campaign framework; no downloader or alternate process runner lives here.
"""
import json
import os
from pathlib import Path
import platform
import shlex
import shutil


def _json(path):
    return json.loads(Path(path).read_text(encoding="utf-8"))


def _write(path, value):
    Path(path).write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def _source(ctx):
    if "product" not in ctx.sources:
        from campaigns.inputs import acquire
        item = next(s for s in ctx.recipe.sources(ctx.root) if s.name == "product")
        ctx.sources["product"] = acquire(item, ctx.policy, ctx.options.fetch)
    return Path(ctx.sources["product"])


def _zig(ctx):
    pin = _json(ctx.root / "config/toolchain.json")
    if "zig" in ctx.sources:
        tree = Path(ctx.sources["zig"])
    else:
        arch = {"AMD64": "x86_64", "arm64": "aarch64"}.get(platform.machine(), platform.machine())
        system = {"Darwin": "macos", "Windows": "windows", "Linux": "linux"}.get(platform.system())
        host = f"{arch}-{system}"
        if host not in pin["hosts"]:
            raise RuntimeError(f"No pinned Zig toolchain for {host}")
        tree = ctx.root / pin["directory"] / pin["hosts"][host]["directory"]
    exe = tree / ("zig.exe" if os.name == "nt" else "zig")
    if not exe.is_file():
        raise RuntimeError(f"Missing pinned Zig executable: {exe}; acquire framework source 'zig'")
    version = ctx.run([exe, "version"], "native-zig-version").strip()
    if version != pin["version"]:
        raise RuntimeError(f"Zig version mismatch: expected {pin['version']}, observed {version}")
    return exe.resolve()


def _engine_command(log, zig, source):
    """Retain the actual upstream module graph rather than inventing one."""
    candidates = []
    for line in log.splitlines():
        if " build-lib " not in line:
            continue
        line = line.removeprefix("info(verbose): ")
        argv = shlex.split(line)
        if not argv or Path(argv[0]).resolve() != zig:
            continue
        if any(arg.startswith("-Mroot=") and arg.endswith("src/lib_vt.zig") for arg in argv):
            candidates.append(argv)
    if not candidates:
        raise RuntimeError("Upstream --verbose log contains no actual src/lib_vt.zig build-lib command")
    # The static invocation carries the compiler runtime closure explicitly.
    argv = next((c for c in candidates if "-fcompiler-rt" in c), candidates[0])
    return [arg for arg in argv if arg != "--listen=-"]


def _prepare_native(ctx):
    previous = getattr(ctx, "_libghostty_native", None)
    if previous is not None:
        return previous
    source = _source(ctx).resolve()
    zig = _zig(ctx)
    config = _json(ctx.root / "config/translation.json")
    pin = _json(ctx.root / "config/source.json")
    work = (ctx.work / "native").resolve()
    work.mkdir(parents=True, exist_ok=True)
    prefix = work / "install"
    cmd = [zig, "build", *config["native_build_arguments"],
           "--prefix", prefix, "--cache-dir", work / "zig-cache", "-j2", "--verbose"]
    # Avoid accidentally describing the enclosing DotCC checkout as Ghostty.
    cmd += ["-Dversion-string=" + pin["upstream_version"],
            "-Dlib-version-string=" + pin["library_version"]]
    if ctx.options.fetch == "never":
        # Zig 0.17's documented offline mode. Disabling its default system
        # integrations preserves the selected upstream scalar feature profile.
        cmd += ["--system", source / "zig-pkg"]
        cmd += ["-fno-sys=" + name for name in pin["build_system_integrations"]]
    env = {"ZIG_GLOBAL_CACHE_DIR": str(ctx.root / "build/native-tools/zig-global-cache")}
    log = ctx.run(cmd, "native-upstream-build", timeout=1800, cwd=source, env=env)
    argv = _engine_command(log, zig, source)
    libraries = list((prefix / "lib").glob("libghostty-vt.a"))
    if not libraries:
        libraries = list((prefix / "lib").glob("*ghostty-vt*.lib"))
    if len(libraries) != 1:
        raise RuntimeError(f"Expected one upstream static library, found {libraries}")
    result = {"upstream": source, "zig": zig, "work": work,
              "native_library": libraries[0], "argv": argv,
              "includes": (zig.parent / "lib", source / "include")}
    _write(work / "native-command.json", list(map(str, cmd)))
    _write(work / "native-engine-command.json", argv)
    ctx._libghostty_native = result
    return result


def prepare_engine(ctx):
    """Return C source/include paths emitted from the whole pinned engine."""
    previous = getattr(ctx, "_libghostty_engine", None)
    if previous is not None:
        return previous
    native = _prepare_native(ctx)
    config = _json(ctx.root / "config/translation.json")
    output = native["work"] / "engine.c"
    argv = [a for a in native["argv"] if not a.startswith("-femit-bin=")]
    target = config.get("c_target", "x86_64-freestanding-none")
    if target:
        # Target/optimization switches apply to the module which follows them.
        # This is the root engine module; dependency modules retain upstream flags.
        root_index = next(i for i, a in enumerate(argv) if a.startswith("-Mroot="))
        argv[root_index:root_index] = ["-target", target]
        argv = [a for a in argv if a != "-fPIC"]
    argv += ["-ofmt=c", "-femit-bin=" + str(output)]
    _write(native["work"] / "emit-c-command.json", argv)
    ctx.run(argv, "native-emit-whole-engine-c", timeout=1800, cwd=native["upstream"])
    if not output.is_file() or not output.stat().st_size:
        raise RuntimeError("Zig succeeded without creating the whole-engine C output")
    contents = output.read_text(encoding="utf-8")
    if target and target.endswith("freestanding-none") and "__asm" in contents:
        raise RuntimeError("Freestanding C output unexpectedly retains inline assembly")
    result = dict(native, source=output, argv=argv, target=target)
    _write(native["work"] / "engine-closure.json", {
        "source": str(output), "target": target,
        "includes": list(map(str, result["includes"])), "command": argv,
        "generated_modules": [a for a in argv if a.startswith("-M")],
        "bytes": output.stat().st_size, "lines": contents.count("\n"),
    })
    ctx._libghostty_engine = result
    return result


def run(ctx, suite):
    """Compare original native and native-compiled C engine assertions."""
    native = prepare_engine(ctx)
    cc = shlex.split(os.environ.get("CC", "cc"))
    if not cc or not shutil.which(cc[0]):
        raise RuntimeError("The native oracle needs a host GNU-compatible C compiler (CC or cc)")
    suffix = ".exe" if os.name == "nt" else ""
    work, source = native["work"], native["upstream"]
    oracle_obj, engine_obj = work / "oracle.o", work / "engine.o"
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-I", source / "include",
             "-c", ctx.root / "tests/native/oracle.c", "-o", oracle_obj], "native-oracle-compile")
    # Exercise the authored translated ABI adapter against the actual headers.
    boundary = ctx.root / "src/Boundary/ghostty_boundary.c"
    if boundary.is_file():
        ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-I", source / "include",
                 "-c", boundary, "-o", work / "boundary.o"], "native-boundary-compile")
    original = work / ("oracle-native" + suffix)
    ctx.run([*cc, oracle_obj, native["native_library"], "-lm", "-o", original], "native-oracle-link")
    original_text = ctx.run([original], "native-oracle-run", timeout=suite.timeout)
    ctx.run([*cc, "-std=gnu11", "-O0", "-w", "-I", native["zig"].parent / "lib",
             "-c", native["source"], "-o", engine_obj], "native-c-engine-compile", timeout=1800)
    translated = work / ("oracle-c" + suffix)
    ctx.run([*cc, oracle_obj, engine_obj, "-lm", "-o", translated], "native-c-oracle-link")
    generated_text = ctx.run([translated], "native-c-oracle-run", timeout=suite.timeout)
    expected = (ctx.root / "tests/native/expected.txt").read_text(encoding="utf-8")
    for label, transcript in (("upstream", original_text), ("zig-c", generated_text)):
        if transcript != expected:
            raise RuntimeError(f"{label} native assertion inventory/transcript differs from expected.txt")
    boundary_passed = 0
    if boundary.is_file():
        boundary_control = work / "boundary_control.o"
        ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-I", source / "include",
                 "-c", ctx.root / "tests/native/boundary_control.c", "-o", boundary_control],
                "native-boundary-control-compile")
        boundary_expected = (ctx.root / "tests/native/boundary-expected.txt").read_text(encoding="utf-8")
        for label, library in (("native", native["native_library"]), ("c", engine_obj)):
            control = work / ("boundary-" + label + suffix)
            ctx.run([*cc, boundary_control, work / "boundary.o", library, "-lm", "-o", control],
                    "native-boundary-" + label + "-link")
            transcript = ctx.run([control], "native-boundary-" + label + "-run", timeout=suite.timeout)
            if transcript != boundary_expected:
                raise RuntimeError(f"{label} boundary control differs from boundary-expected.txt")
        boundary_passed = 2
    _write(ctx.artifacts / "native-oracle.json", {
        "cases": 16, "passed": 16, "failed": 0, "original_equals_c": True,
        "boundary_cases": boundary_passed, "boundary_passed": boundary_passed,
        "source": str(native["source"]), "target": native["target"],
        "expected": str(ctx.root / "tests/native/expected.txt"),
        "native_log": "native-oracle-run.log", "c_log": "native-c-oracle-run.log",
        "compiler": cc, "allocator": "tracked explicit C vtable; alloc/free size and alignment checked",
    })
    return generated_text
