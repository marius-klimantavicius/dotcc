"""Run actual owning-API assertions against selected published zstd artifacts."""
from __future__ import annotations

import json
import hashlib
import platform
from pathlib import Path
import shutil
import sys

from campaigns.inputs import acquire
from campaigns.recipes import helper
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def require_target(ctx):
    if ctx.options.rid != "linux-x64" or sys.platform != "linux" or platform.machine().lower() not in ("x86_64", "amd64"):
        raise RuntimeError("Zstd managed qualification currently requires execution on linux-x64")


def run_consumer(ctx, suite):
    require_target(ctx)
    from copy import copy
    from dataclasses import replace
    from campaigns.testing import consumer
    original_options, original_recipe = ctx.options, ctx.recipe
    cells = [(form, mode) for form in forms(ctx) for mode in modes(ctx)]
    try:
        for form, mode in cells:
            selected = copy(original_options)
            selected.form, selected.mode = form, mode
            arguments = (*original_recipe.consumer.build_arguments, "-m:1", "--artifacts-path",
                         str(ctx.work / "consumer-intermediates" / (form + "-" + mode)))
            ctx.options = selected
            ctx.recipe = replace(original_recipe, consumer=replace(original_recipe.consumer, build_arguments=arguments))
            consumer(ctx, suite)
    finally:
        ctx.options, ctx.recipe = original_options, original_recipe


def oracle(ctx):
    if hasattr(ctx, "_zstd_native_oracle"):
        return ctx._zstd_native_oracle
    source = ctx.sources.get("product") or acquire(next(ctx.recipe.sources(ctx.root)), ctx.policy, ctx.options.fetch)
    module = helper(ctx.root / "tests/NativeOracle/run.py")
    binary = module.build(source, ctx.work / "managed-native-oracle",
                          execute=lambda label, command, **kw: ctx.run(command, "managed-native-" + label, **kw))
    transcript = ctx.run([binary, "self-test"], "managed-native-self-test")
    actual = [line.removeprefix("CASE PASS ") for line in transcript.splitlines() if line.startswith("CASE PASS ")]
    if actual != list(module.CASES):
        raise RuntimeError("Native oracle case inventory differs: " + repr(actual))
    # The native failure control proves its CHECK mechanism is active in Release.
    ctx.run([binary, "fail-control"], "managed-native-fail-control", check=False)
    control = ctx.receipt["commands"][-1]
    control.update(expected_exit_code=1, deliberate_failure_control=True)
    ctx.save()
    if control.get("exit_code") != 1:
        raise RuntimeError("Native oracle failure control did not fail with exit code 1")
    ctx._zstd_native_oracle = binary
    return binary


def managed(ctx, form, mode):
    product = check_product(ctx, form)
    cache = getattr(ctx, "_zstd_managed_programs", {})
    key = (str(product), mode)
    if key in cache:
        return cache[key]
    project = ctx.root / "tests/ManagedTests/ManagedTests.csproj"
    label = "managed-tests-" + form + "-" + mode
    output = ctx.work / label
    intermediates = ctx.work / "managed-intermediates" / (form + "-" + mode + "-rooted")
    common = ["-p:ZstdProject=" + str(product), "-m:1", "-o", output, "--artifacts-path", intermediates]
    if mode == "jit":
        ctx.managed("build", project, label + "-build", *common)
        command = ["dotnet", output / "ManagedTests.dll"]
    else:
        ctx.managed("publish", project, label + "-publish", *common,
                    "-r", ctx.options.rid, "-p:PublishAot=true", "-p:IlcGenerateMapFile=true", timeout=1800)
        command = [output / ("ManagedTests.exe" if sys.platform == "win32" else "ManagedTests")]
    executable = Path(command[1] if mode == "jit" else command[0])
    control_work = ctx.artifacts / (label + "-failure-control")
    ctx.run([*command, "failure-control", control_work], label + "-failure-control", check=False)
    control = ctx.receipt["commands"][-1]
    control.update(expected_exit_code=1, deliberate_failure_control=True)
    details = json.loads((control_work / "failure-control-report.json").read_text())
    if control.get("exit_code") != 1 or details["failed"] != 1 or details["passed"] != 0:
        raise RuntimeError("Managed assertion failure did not propagate correctly")
    evidence = dict(form=form, mode=mode, whole_library_rooted=True, product=str(product),
                    output=str(output), executable=str(executable),
                    executable_sha256=hashlib.sha256(executable.read_bytes()).hexdigest(),
                    intermediates=str(intermediates))
    if mode == "aot":
        maps = list(intermediates.rglob("ManagedTests.map.xml"))
        if len(maps) != 1:
            raise RuntimeError("Expected one rooted NativeAOT method map: " + repr(maps))
        retained = ctx.artifacts / (label + ".map.xml")
        shutil.copy2(maps[0], retained)
        evidence.update(aot_map=str(retained), aot_map_sha256=hashlib.sha256(retained.read_bytes()).hexdigest())
    ctx.receipt.setdefault("zstd_managed_harnesses", []).append(evidence)
    ctx.save()
    cache[key] = (command, output)
    ctx._zstd_managed_programs = cache
    return command, output


def _interop(ctx, command, binary, work, label):
    work.mkdir(parents=True, exist_ok=True)
    ctx.run([*command, "fixtures", work], label + "-managed-fixtures", timeout=300)
    ctx.run([binary, "compress", work / "payload.bin", work / "native.zst"], label + "-native-encode")
    ctx.run([binary, "train", work / "samples.bin", work / "sizes.bin", work / "native.dict"], label + "-native-train", timeout=300)
    ctx.run([binary, "compress", work / "payload.bin", work / "native-dict.zst", work / "native.dict"], label + "-native-dict-encode")
    for frame, decoded, dictionary in (
        ("managed.zst", "native-decoded.bin", None),
        ("managed-dict.zst", "native-dict-decoded.bin", "managed.dict"),
        ("managed-stream.zst", "native-stream-decoded.bin", None),
    ):
        ctx.run([binary, "decompress", work / frame, work / decoded,
                 *([work / dictionary] if dictionary else [])], label + "-decode-" + frame)
    ctx.run([*command, "interop", work], label + "-run", timeout=300, separate=True)
    ctx.run([binary, "decompress", work / "managed-native-dict.zst", work / "native-cross-decoded.bin", work / "native.dict"], label + "-cross-dictionary-decode")
    comparisons = (
        ("deterministic-frame-bytes", "managed.zst", "native.zst"),
        ("deterministic-training-bytes", "managed.dict", "native.dict"),
        ("native-dictionary-managed-frame-native-decoded", "payload.bin", "native-cross-decoded.bin"),
    )
    results = []
    for name, left, right in comparisons:
        equal = (work / left).read_bytes() == (work / right).read_bytes()
        results.append(dict(name=name, status="passed" if equal else "failed", left=left, right=right))
    (work / "interop-comparison-report.json").write_text(json.dumps(dict(cases=results), indent=2) + "\n")
    if any(item["status"] != "passed" for item in results):
        raise RuntimeError("Native/managed deterministic interchange comparison failed")


def run(ctx, suite):
    require_target(ctx)
    if suite.name == "upstream":
        # Exact extracted upstream routines have their own translated test unit.
        return helper(ctx.root / "scripts/upstream.py").run(ctx, suite)
    binary = oracle(ctx) if suite.name in ("abi", "interop") else None
    rows = []
    report = ctx.artifacts / (suite.name + "-matrix.json")
    try:
        for form in forms(ctx):
            for mode in modes(ctx):
                label = suite.name + "-" + form + "-" + mode
                work = ctx.artifacts / label
                work.mkdir(parents=True, exist_ok=True)
                row = dict(form=form, mode=mode, rid=ctx.options.rid,
                           status="running", report=str(work / (suite.name + "-report.json")))
                rows.append(row)
                report.write_text(json.dumps(rows, indent=2) + "\n")
                try:
                    command, _ = managed(ctx, form, mode)
                    if suite.name == "abi":
                        observation = ctx.run([binary, "abi"], label + "-native")
                        values = json.loads(observation)
                        if len(values) != 12:
                            raise RuntimeError("Incomplete native ABI inventory")
                        (work / "native-abi.json").write_text(json.dumps(values) + "\n")
                    if suite.name == "interop":
                        _interop(ctx, command, binary, work, label)
                    else:
                        ctx.run([*command, suite.name, work], label + "-run", timeout=300, separate=True)
                    details = json.loads((work / (suite.name + "-report.json")).read_text())
                    expected = {"abi": 3, "behavior": 30, "lifecycle": 7, "interop": 6}[suite.name]
                    if details["failed"] or details["passed"] != expected or details["skipped"]:
                        raise RuntimeError("Required managed cases failed, missing or skipped")
                    row.update(status="passed", passed=details["passed"], failed=0, skipped=0)
                except BaseException as error:
                    row.update(status="failed", error=str(error))
                    raise
                finally:
                    report.write_text(json.dumps(rows, indent=2) + "\n")
    finally:
        ctx.receipt.setdefault("zstd_qualification", {})[suite.name] = str(report)
        ctx.save()
