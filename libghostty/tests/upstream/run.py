"""Shared-campaign hooks for unchanged upstream render-state test assertions."""
import json
import os
from pathlib import Path
import re
import shlex
import shutil


def _json(path):
    return json.loads(Path(path).read_text(encoding="utf-8"))


def _write(path, data):
    Path(path).write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")


def _inputs(ctx):
    from campaigns.inputs import acquire
    declared = {item.name: item for item in ctx.recipe.sources(ctx.root)}
    for name in ("product", "zig"):
        if name not in ctx.sources:
            ctx.sources[name] = acquire(declared[name], ctx.policy, ctx.options.fetch)
    zig = Path(ctx.sources["zig"]) / ("zig.exe" if os.name == "nt" else "zig")
    return Path(ctx.sources["product"]).resolve(), zig.resolve()


def _stage_test_std(ctx, original, destination):
    # The acquired compiler is immutable. This private copy only changes the
    # test runner's CPU-count execution service, retaining allocator algorithms.
    shutil.copytree(original, destination)
    policy = _json(ctx.root / "config/toolchain.json")
    for item in policy.get("test_adaptations", []):
        path = destination / item["file"]
        original_text = path.read_text(encoding="utf-8")
        if original_text.count(item["anchor"]) != 1:
            raise RuntimeError(f"Missing/ambiguous test-only toolchain adaptation: {item['file']}")
        path.write_text(original_text.replace(item["anchor"], item["replacement"]), encoding="utf-8")


def prepare_tests(ctx):
    """Return the genuine test C unit and host services for a diagnostic product."""
    previous = getattr(ctx, "_libghostty_upstream_render", None)
    if previous is not None:
        return previous
    source, zig = _inputs(ctx)
    selection = _json(ctx.root / "tests/upstream/selection.json")
    policy = _json(ctx.root / "config/translation.json")
    pin = _json(ctx.root / "config/source.json")
    toolchain = _json(ctx.root / "config/toolchain.json")
    version = ctx.run([zig, "version"], "upstream-zig-version").strip()
    if version != toolchain["version"]:
        raise RuntimeError(f"Wrong Zig compiler: {version}")
    work = (ctx.work / "upstream-render").resolve()
    work.mkdir(parents=True, exist_ok=True)
    native_flags = [arg for arg in policy["native_build_arguments"] if not arg.startswith("-Doptimize=")]
    cmd = [zig, "build", "test-lib-vt-build", *native_flags,
           "-Doptimize=" + selection["optimization"], "-Demit-test-exe=true",
           "-Dtest-filter=" + selection["test_filter"],
           "-Dversion-string=" + pin["upstream_version"],
           "-Dlib-version-string=" + pin["library_version"],
           "--prefix", work / "install", "--cache-dir", work / "zig-cache", "-j2", "--verbose"]
    if ctx.options.fetch == "never":
        cmd += ["--system", source / "zig-pkg"]
        cmd += ["-fno-sys=" + name for name in pin["build_system_integrations"]]
    env = {"ZIG_GLOBAL_CACHE_DIR": str(ctx.root / "build/native-tools/zig-global-cache")}
    log = ctx.run(cmd, "upstream-render-native-build", timeout=1800, cwd=source, env=env)
    candidates = [shlex.split(line.removeprefix("info(verbose): ")) for line in log.splitlines()
                  if "zig test " in line and "--name ghostty-vt-c-test " in line]
    if len(candidates) != 1:
        raise RuntimeError("Expected one actual upstream C API test compile command")
    native_argv = [arg for arg in candidates[0] if arg not in ("--listen=-", "-fPIC")]
    runner = work / "runner.zig"
    shutil.copyfile(ctx.root / "tests/upstream/runner.zig", runner)
    excluded = ",\n".join(json.dumps(item["name"]) for item in selection.get("excluded_cases", []))
    (work / "selection.zig").write_text(
        "pub const prefix = " + json.dumps(selection["test_filter"]) + ";\n"
        + "pub const excluded = [_][]const u8{\n" + excluded + "\n};\n", encoding="utf-8")
    native_argv += ["--test-runner", str(runner), "--test-no-exec"]
    staged_std = work / "zig-lib"
    _stage_test_std(ctx, zig.parent / "lib", staged_std)
    output = work / "upstream-render.c"
    argv = list(native_argv)
    idx = next(i for i, arg in enumerate(argv) if arg.startswith("-Mroot="))
    argv[idx:idx] = ["-target", selection["target"]]
    idx = argv.index("--zig-lib-dir")
    argv[idx + 1] = str(staged_std)
    argv += ["-fno-entry", "-ofmt=c", "-femit-bin=" + str(output)]
    _write(work / "native-build-command.json", list(map(str, cmd)))
    _write(work / "native-test-command.json", native_argv)
    _write(work / "emit-test-c-command.json", argv)
    ctx.run(argv, "upstream-render-emit-c", timeout=1800, cwd=source)
    if not output.is_file() or output.stat().st_size == 0:
        raise RuntimeError("Zig emitted no actual test C product")
    if "__asm" in output.read_text(encoding="utf-8"):
        raise RuntimeError("Freestanding upstream test C unexpectedly contains inline assembly")
    result = {"source": output, "includes": (staged_std, source / "include"),
              "host": ctx.root / "tests/upstream/host.c", "case_count": selection["expected_cases"],
              "upstream": source, "zig": zig, "work": work, "argv": argv,
              "native_argv": native_argv, "selection": selection}
    _write(work / "test-closure.json", {
        "source": str(output), "host": str(result["host"]),
        "case_count": result["case_count"], "selection": selection,
        "argv": argv, "native_argv": native_argv,
        "toolchain_adaptations": toolchain.get("test_adaptations", []),
        "bytes": output.stat().st_size,
    })
    ctx._libghostty_upstream_render = result
    return result


def run(ctx, suite):
    """Native Zig and native C execution, with exact per-case comparison."""
    tests = prepare_tests(ctx)
    work = tests["work"]
    cc = shlex.split(os.environ.get("CC", "cc"))
    if not cc or not shutil.which(cc[0]):
        raise RuntimeError("Upstream native tests require a GNU-compatible host C compiler")
    host_obj = work / "host-no-main.o"
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-DDOTCC_UPSTREAM_HOST_NO_MAIN",
             "-c", tests["host"], "-o", host_obj], "upstream-render-host-compile")
    native = work / "upstream-render-native"
    argv = [*tests["native_argv"], "-lc", str(host_obj), "-femit-bin=" + str(native)]
    ctx.run(argv, "upstream-render-original-compile", timeout=1800, cwd=tests["upstream"])
    original = ctx.run([native], "upstream-render-original-run", timeout=suite.timeout, separate=True)
    obj, executable = work / "upstream-render.o", work / "upstream-render-c"
    ctx.run([*cc, "-std=gnu11", "-O0", "-w", "-I", tests["includes"][0],
             "-c", tests["source"], "-o", obj], "upstream-render-c-compile", timeout=1800)
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", tests["host"], obj,
             "-lm", "-o", executable], "upstream-render-c-link")
    emitted = ctx.run([executable], "upstream-render-c-run", timeout=suite.timeout, separate=True)
    # separate=True preserves stderr progress/diagnostics in its own log;
    # Context.run returns stdout. RUN lines are never normalized into passes.
    expected_count = tests["case_count"]
    summary = f"RESULT upstream-cases={expected_count} passed={expected_count} skipped=0 failed=0 live=0\n"
    for name, transcript in (("original", original), ("c", emitted)):
        if not transcript.endswith(summary) or len(re.findall(r"^PASS terminal\.c\.render\.test\.", transcript, re.M)) != expected_count:
            raise RuntimeError(f"{name} upstream-render case inventory/status did not pass exactly {expected_count} cases")
    if original != emitted:
        raise RuntimeError("Original Zig and generated C upstream render transcripts differ")
    expected = (ctx.root / "tests/upstream/expected.txt").read_text(encoding="utf-8")
    if original != expected:
        raise RuntimeError("Accepted upstream-render transcript differs from expected.txt")
    _write(ctx.artifacts / "upstream-render.json", {
        "case_count": expected_count, "passed": expected_count, "skipped": 0, "failed": 0,
        "live_allocations": 0, "original_equals_c": True, "source": str(tests["source"]),
    })
    return emitted
