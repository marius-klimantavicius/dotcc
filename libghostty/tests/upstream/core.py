"""Normal campaign hooks for the complete selected upstream-core diagnostic.

The product contains genuine upstream tests and algorithms. Only test execution
services and authored runner code are adapted; acquired inputs stay unchanged.
"""
import json
import os
from pathlib import Path
import platform
import re
import shlex
import shutil


def _read(path):
    return json.loads(Path(path).read_text(encoding="utf-8"))


def _write(path, value):
    Path(path).write_text(json.dumps(value, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")


def _base_selection(root):
    selection = _read(Path(root) / "tests/upstream/core-selection.json")
    names = selection["case_names"]
    if not names or len(names) != selection["expected_cases"] or len(set(names)) != len(names):
        raise RuntimeError("Core selection must contain the exact unique expected case inventory")
    return selection


def _candidate_names(root, selection, filename="core-expected.txt"):
    lines = (Path(root) / "tests/upstream" / filename).read_text(encoding="utf-8").splitlines()
    count = selection["expected_cases"]
    summary = f"RESULT upstream-cases={count} passed={count} skipped=0 failed=0 live=0"
    if not lines or lines[-1] != summary or any(not line.startswith("PASS ") for line in lines[:-1]):
        raise RuntimeError("Invalid original-native candidate core transcript")
    names = [line[5:] for line in lines[:-1]]
    if len(names) != count or len(set(names)) != count or set(names) != set(selection["case_names"]):
        raise RuntimeError("Candidate core transcript differs from the exact required selection")
    return names


def partitions(root):
    """Read/validate the exact deterministic partition metadata; performs no tool calls."""
    root = Path(root)
    base = _base_selection(root)
    metadata = _read(root / "tests/upstream/core-partitions.json")
    if metadata["schema_version"] != 1 or metadata["selection"] != "core-selection.json":
        raise RuntimeError("Unsupported core partition metadata")
    if metadata["expected_cases"] != base["expected_cases"] or metadata["case_limit"] < 1:
        raise RuntimeError("Core partition inventory/size differs from its parent selection")
    scalar_features = _read(root / "config/translation.json")["features"]
    if metadata["scalar_features"] != scalar_features or "glyph_protocol" in scalar_features:
        raise RuntimeError("Core scalar partitions must use exactly the delivered scalar features")
    order = _candidate_names(root, base, metadata["expected_transcript"])
    required = set(base["case_names"])
    groups = _read(root / "tests/upstream" / metadata["groups"])["groups"]
    group_names = {group["id"]: set(group["case_names"]) for group in groups}
    workflow = metadata["workflow_groups"]
    if len(group_names) != len(groups) or len(set(workflow)) != len(workflow) or set(workflow) != set(group_names):
        raise RuntimeError("Partition workflow must include every inventory group exactly once")
    glyph = metadata["glyph_case"]
    if glyph not in required:
        raise RuntimeError("The isolated glyph assertion is not a required core case")
    canonical = []
    limit = metadata["case_limit"]
    for group in workflow:
        names = [name for name in order if name in group_names[group] and name != glyph]
        for start in range(0, len(names), limit):
            subset = names[start:start + limit]
            identifier = f"{group}-{start // limit + 1:02d}"
            canonical.append(dict(id=identifier, group=group, profile="upstream-core-" + identifier,
                                  expected_cases=len(subset), native_features=scalar_features, case_names=subset))
    canonical.append(dict(id="glyph-reset", group="glyph-reset", profile="upstream-core-glyph-reset",
                          expected_cases=1, native_features=[*scalar_features, "glyph_protocol"], case_names=[glyph]))
    declared = metadata["partitions"]
    if len(declared) != metadata["expected_partitions"] or declared != canonical:
        raise RuntimeError("Core partitions differ from the deterministic workflow/native-order slices")
    names = [name for part in declared for name in part["case_names"]]
    if len(names) != len(set(names)) or len(names) != len(required) or set(names) != required:
        raise RuntimeError("Core partition union must cover every required case exactly once")
    return tuple(declared)


def selection_for(root, partition=None):
    """Resolve a partition ID (or None for the original monolithic diagnostic)."""
    selection = _base_selection(root)
    selection = dict(selection, partition_id=None, work_name="upstream-core")
    if partition is None:
        return selection
    if not isinstance(partition, str) or not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", partition):
        raise RuntimeError("Core partition must be a declared metadata ID")
    selected = next((part for part in partitions(root) if part["id"] == partition), None)
    if selected is None:
        raise RuntimeError("Unknown core partition: " + partition)
    return dict(selection, profile=selected["profile"], partition_id=partition,
                work_name=selected["profile"], expected_cases=selected["expected_cases"],
                native_features=selected["native_features"], case_names=selected["case_names"])


def expected_transcript(root, partition=None):
    """Filter the original-native candidate in its original order; no pass claim."""
    base = _base_selection(root)
    selected = selection_for(root, partition)
    names = set(selected["case_names"])
    ordered = [name for name in _candidate_names(root, base) if name in names]
    count = selected["expected_cases"]
    if len(ordered) != count:
        raise RuntimeError("Partition candidate transcript has the wrong case count")
    return "".join("PASS " + name + "\n" for name in ordered) + (
        f"RESULT upstream-cases={count} passed={count} skipped=0 failed=0 live=0\n")


def _adapt(directory, items):
    for item in items:
        path = directory / item["file"]
        text = path.read_text(encoding="utf-8")
        if text.count(item["anchor"]) != 1:
            raise RuntimeError(f"Missing/ambiguous core service adaptation: {item['file']}")
        path.write_text(text.replace(item["anchor"], item["replacement"]), encoding="utf-8")


def _filters(selection):
    # Zig 0.17's response parser mishandles apostrophes inside quoted arguments.
    # Exact direct argv works on Linux. Short module filters avoid OS argv limits
    # elsewhere; the runtime selector still enforces every exact selected name.
    if os.name != "nt" and platform.system() != "Darwin":
        return list(selection["case_names"]), "full-name-substrings-with-exact-runtime-selection"
    modules = sorted({name.split(".test.")[0] for name in selection["case_names"]})
    result = []
    for module in modules:
        if module == "terminal.compress.lz4_differential":
            # Keep the ordinary light case without compiling opt-in exhaustive
            # environment-dependent code into this diagnostic profile.
            result += [name for name in selection["case_names"] if name.startswith(module + ".test.")]
        else:
            result.append(module + ".test.")
    return result, "short-module-filters-with-exact-runtime-selection"


def prepare_core(ctx, partition=None):
    """Prepare genuine upstream test C for one metadata ID, or the full selection."""
    selection = selection_for(ctx.root, partition)
    cache = getattr(ctx, "_libghostty_upstream_cores", {})
    previous = cache.get(partition)
    if previous is None and partition is None:
        previous = getattr(ctx, "_libghostty_upstream_core", None)
    if previous is not None:
        return previous
    expected = expected_transcript(ctx.root, partition)
    log_prefix = "core" if partition is None else "core-" + partition
    label = lambda suffix: log_prefix + "-" + suffix
    from campaigns.inputs import acquire
    declared = {item.name: item for item in ctx.recipe.sources(ctx.root)}
    for name in ("product", "zig"):
        if name not in ctx.sources:
            ctx.sources[name] = acquire(declared[name], ctx.policy, ctx.options.fetch)
    source = Path(ctx.sources["product"]).resolve()
    zig = (Path(ctx.sources["zig"]) / ("zig.exe" if os.name == "nt" else "zig")).resolve()
    tool = _read(ctx.root / "config/toolchain.json")
    version = ctx.run([zig, "version"], label("zig-version")).strip()
    if version != tool["version"]:
        raise RuntimeError(f"Wrong pinned Zig version: {version}")
    pin = _read(ctx.root / "config/source.json")
    config = _read(ctx.root / "config/translation.json")
    adaptations = _read(ctx.root / "tests/upstream/core-adaptations.json")
    names = selection["case_names"]
    work = (ctx.work / selection["work_name"]).resolve()
    work.mkdir(parents=True, exist_ok=True)
    expected_path = work / "expected.txt"
    expected_path.write_text(expected, encoding="utf-8")
    native_flags = [arg for arg in config["native_build_arguments"]
                    if not arg.startswith(("-Doptimize=", "-Dvt-features="))]
    features = "-all," + ",".join("+" + feature.replace("_", "-") for feature in selection["native_features"])
    command = [zig, "build", "test-lib-vt-build", *native_flags,
               "-Doptimize=" + selection["optimization"], "-Dvt-features=" + features, "-Demit-test-exe=true",
               "-Dtest-filter=" + (names[0] if partition is not None else "terminal.c.render.test."),
               "-Dversion-string=" + pin["upstream_version"],
               "-Dlib-version-string=" + pin["library_version"],
               "--prefix", work / "install", "--cache-dir", work / "zig-cache", "-j2", "--verbose"]
    if ctx.options.fetch == "never":
        command += ["--system", source / "zig-pkg"]
        command += ["-fno-sys=" + name for name in pin["build_system_integrations"]]
    log = ctx.run(command, label("native-module-graph"), timeout=1800, cwd=source,
                  env={"ZIG_GLOBAL_CACHE_DIR": str(ctx.root / "build/native-tools/zig-global-cache")})
    candidates = [shlex.split(line.removeprefix("info(verbose): ")) for line in log.splitlines()
                  if "zig test " in line and "--name ghostty-vt-c-test " in line]
    if len(candidates) != 1:
        raise RuntimeError("No unique actual upstream C API test module compile command")
    captured = candidates[0]
    native = []
    index = 0
    while index < len(captured):
        if captured[index] == "--test-filter":
            index += 2
        else:
            if captured[index] not in ("--listen=-", "-fPIC"):
                native.append(captured[index])
            index += 1
    filters, strategy = _filters(selection)
    for value in filters:
        native += ["--test-filter", value]
    root_index = next(index for index, arg in enumerate(native) if arg.startswith("-Mroot="))
    native[root_index:root_index] = ["-fsingle-threaded"]
    for name in ("core-runner.zig", "core-io.zig", "core-page.zig"):
        shutil.copyfile(ctx.root / "tests/upstream" / name, work / name)
    # Zig strings accept actual UTF-8; JSON's \uXXXX escapes are not Zig syntax.
    (work / "selection.zig").write_text("pub const names = [_][]const u8{\n" +
        ",\n".join(json.dumps(name, ensure_ascii=False) for name in names) + "\n};\n", encoding="utf-8")
    native += ["--test-runner", str(work / "core-runner.zig"), "--test-no-exec", "-lc"]
    staged_source = work / "staged"
    shutil.copytree(source / "src", staged_source / "src")
    _adapt(staged_source, adaptations["source"])
    staged_std = work / "zig-lib"
    shutil.copytree(zig.parent / "lib", staged_std)
    _adapt(staged_std, tool.get("test_adaptations", []))
    _adapt(staged_std, adaptations["toolchain"])
    emitted = list(native)
    emitted[1] = "test-obj"
    emitted = [("-Mroot=" + str(staged_source / "src/lib_vt.zig"))
               if arg.startswith("-Mroot=") else arg for arg in emitted]
    root_index = next(index for index, arg in enumerate(emitted) if arg.startswith("-Mroot="))
    emitted[root_index:root_index] = ["-target", selection["target"]]
    emitted[emitted.index("--zig-lib-dir") + 1] = str(staged_std)
    output = work / "upstream-core.c"
    emitted += ["-fno-entry", "-ofmt=c", "-femit-bin=" + str(output)]
    _write(work / "native-module-build.json", list(map(str, command)))
    _write(work / "native-argv.json", native)
    _write(work / "emit-argv.json", emitted)
    ctx.run(emitted, label("emit-c"), timeout=1800, cwd=source)
    if not output.is_file() or output.stat().st_size == 0:
        raise RuntimeError("Zig did not emit the complete selected upstream-core test product")
    with output.open(encoding="utf-8") as stream:
        if any("__asm" in line for line in stream):
            raise RuntimeError("Freestanding core test C unexpectedly contains inline assembly")
    from campaigns.recipes import helper
    helper(ctx.root / "tests/upstream/host_contract.py").check(
        ctx, output, (staged_std, source / "include"), label("host-contract"))
    result = {"source": output, "includes": (staged_std, source / "include"),
              "host": ctx.root / "tests/upstream/host.c", "host_io": ctx.root / "tests/upstream/host_io.c",
              "driver": ctx.root / "tests/upstream/core-host.c", "case_count": selection["expected_cases"],
              "upstream": source, "work": work, "zig": zig, "argv": emitted,
              "native_argv": native, "selection": selection, "partition": partition,
              "expected": expected_path, "expected_transcript": expected, "log_prefix": log_prefix}
    _write(work / "core-closure.json", {"source": str(output), "case_count": result["case_count"],
           "partition": partition, "native_features": selection["native_features"],
           "expected": str(expected_path), "expected_status": "candidate; native/C qualification pending",
           "filter_strategy": strategy, "argv": emitted, "native_argv": native,
           "adaptations": adaptations, "bytes": output.stat().st_size,
           "capabilities": ["tracked aligned allocation", "zeroed page allocation", "read-only corpus I/O",
                            "secure entropy", "hosted C allocator", "sequential execution"]})
    cache[partition] = result
    ctx._libghostty_upstream_cores = cache
    if partition is None:
        ctx._libghostty_upstream_core = result
    return result


def run(ctx, suite, partition=None):
    tests = prepare_core(ctx, partition)
    label = lambda suffix: tests["log_prefix"] + "-" + suffix
    work = tests["work"]
    cc = shlex.split(os.environ.get("CC", "cc"))
    if not cc or not shutil.which(cc[0]):
        raise RuntimeError("Upstream core native controls require a GNU-compatible host C compiler")
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-DDOTCC_UPSTREAM_HOST_NO_MAIN",
             "-c", tests["host"], "-o", work / "host.o"], label("host-compile"))
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-c", tests["host_io"],
             "-o", work / "host_io.o"], label("host-io-compile"))
    native = work / "upstream-core-native"
    ctx.run([*tests["native_argv"], str(work / "host.o"), str(work / "host_io.o"), "-femit-bin=" + str(native)],
            label("original-compile"), timeout=1800, cwd=tests["upstream"])
    original = ctx.run([native], label("original-run"), timeout=suite.timeout, cwd=tests["upstream"], separate=True)
    obj, executable = work / "upstream-core.o", work / "upstream-core-c"
    ctx.run([*cc, "-std=gnu11", "-O0", "-w", "-I", tests["includes"][0], "-c", tests["source"], "-o", obj],
            label("c-compile"), timeout=1800)
    ctx.run([*cc, "-std=c11", "-Wall", "-Wextra", "-Werror", "-DDOTCC_UPSTREAM_HOST_NO_MAIN",
             tests["host"], tests["host_io"], tests["driver"], obj, "-lm", "-o", executable], label("c-link"))
    generated = ctx.run([executable], label("c-run"), timeout=suite.timeout, cwd=tests["upstream"], separate=True)
    count = tests["case_count"]
    expected_summary = f"RESULT upstream-cases={count} passed={count} skipped=0 failed=0 live=0\n"
    for label, transcript in (("original", original), ("C", generated)):
        if not transcript.endswith(expected_summary) or transcript.count("\nPASS ") + int(transcript.startswith("PASS ")) != count:
            raise RuntimeError(f"{label} core did not pass exactly {count} required cases with zero skips/failures")
    if original != generated:
        raise RuntimeError("Original and generated C core test transcripts differ")
    expected = tests["expected_transcript"]
    if original != expected:
        raise RuntimeError("Core native/C output differs from the original-native candidate transcript")
    artifact = "upstream-core" + ("-" + partition if partition is not None else "") + ".json"
    _write(ctx.artifacts / artifact, {"cases": count, "passed": count, "skipped": 0, "failed": 0,
           "tracked_live_allocations": 0, "fixture_handles_checked_after_every_case": True,
           "original_equals_c": True, "partition": partition, "native_features": tests["selection"]["native_features"],
           "expected": str(tests["expected"])})
    return generated
