"""Logged native/original-C qualification of the actual full Zydis source profile.

The companion callback emits test-only C using the *same product object files*.
Managed build/execution happens only when the parent invokes the campaign suite.
"""
from __future__ import annotations

import argparse
from concurrent.futures import ThreadPoolExecutor, as_completed
import hashlib
import json
import os
from pathlib import Path
import platform
import re
import shlex
import sys

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT.parent / "Scripts"))
from campaigns.delivery import validate_emission
from campaigns.layout import Layout
from campaigns.recipes import helper, link_flags, load
from campaigns.testing import forms, modes
from campaigns.translation import check_product, object_report_flags

COMPANION = "TranslatedZydisQualification"
GROUPS = {0: "abi", 1: "controls", 2: "upstream-info", 3: "upstream-encoder", 4: "upstream-absolute"}
EXPECTED = {
    0: ["abi"],
    1: ["version-features", "decode-format-tokenize-modes", "formatter-hook-context-lifetimes",
        "ordinary-errors", "encode-convert-roundtrip-absolute", "nop-fill", "utilities-register-context-segments"],
    2: ["upstream-info"], 3: ["upstream-encoder-reencoder"], 4: ["upstream-absolute"],
}


def _configuration(ctx):
    recipe = helper(ctx.root / "scripts/campaign.py")
    selected = recipe.native_configuration(ctx.root, ctx.sources or None)
    ctx.sources.update(selected["sources"])
    return selected


def _harness(ctx):
    return helper(ctx.root / "tests/harness.py")


def _digest(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def _inputs(ctx, selected):
    """Current content identity, independent of historical provenance hash policy."""
    result = {}
    for name, source in selected["sources"].items():
        for directory in ("include", "src", "tests", "tools") if name == "zydis" else ("include", "src/API"):
            for path in (source / directory).rglob("*"):
                # Include generated .inc lookup tables and any other regular
                # dependency here. A suffix allowlist can miss bytes consumed
                # by #include and falsely accept stale translated tables.
                if path.is_file():
                    result[name + "/" + path.relative_to(source).as_posix()] = _digest(path)
    for path in (ctx.root / "tests/qualification.c", ctx.root / "tests/harness.py",
                 ctx.root / "config/translation.json"):
        result["authored/" + path.relative_to(ctx.root).as_posix()] = _digest(path)
    return result


def companions(ctx):
    """Translation.companions callback. Does no tool/managed builds itself.

    Called after normal product object emission and linking; reuse those exact
    core objects instead of independently selecting/replacing library code.
    """
    selected = _configuration(ctx)
    units = _harness(ctx).test_units(ctx, ctx.sources["zydis"])
    core = [ctx.work / "objects" / f"{i:03d}-{path.stem}.cs" for i, path in enumerate(selected["units"])]
    if not all(path.is_file() for path in core):
        raise RuntimeError("Qualification companion requires all exact current product objects")
    objects = ctx.work / "qualification-objects"
    objects.mkdir(parents=True, exist_ok=True)

    def emit(row):
        i, unit = row
        label = f"qualification-emit-{i:03d}-{unit.path.stem}"
        output = objects / (label + ".cs")
        report = ctx.artifacts / (label + "-overrides.jsonl")
        ctx.run(["dotnet", ctx.compiler, *object_report_flags([*selected["flags"], *unit.flags], report),
                 unit.path, "--emit=obj", "-o", output], label, timeout=ctx.options.timeout)
        if not output.is_file():
            raise RuntimeError("Missing qualification object: " + str(unit.path))
        return output

    with ThreadPoolExecutor(max_workers=ctx.options.jobs) as workers:
        pending = {workers.submit(emit, row): row[0] for row in enumerate(units)}
        emitted = [None] * len(units)
        try:
            for future in as_completed(pending):
                emitted[pending[future]] = future.result()
        except BaseException:
            ctx.cancellation.set()
            for future in pending:
                future.cancel()
            raise
    raw = ctx.work / "raw" / COMPANION
    exports = json.loads((ctx.root / "config/translation.json").read_text())["exportEnums"]
    ctx.run(["dotnet", ctx.compiler, *core, *emitted, "--emit=managedlib",
             *link_flags("ZydisQualification", "Managed.Zydis.Tests"),
             *(part for pattern in exports for part in ("--export-enum", pattern)), "-o", raw],
            "qualification-link", timeout=max(600, ctx.options.timeout))
    validate_emission(raw, COMPANION)
    (raw / "zydis-test-inputs.json").write_text(json.dumps(_inputs(ctx, selected), indent=2) + "\n")
    ctx.receipt["zydis_qualification_companion"] = dict(
        product=COMPANION, core_objects=[str(path) for path in core],
        test_units=[str(unit.path) for unit in units],
        fixture_inventory=str(ctx.work / "zydis-qualification-fixtures/inventory.json"))
    ctx.save()
    return {COMPANION: raw}


def _target(ctx):
    if (sys.platform != "linux" or ctx.options.rid != "linux-x64" or
            platform.machine().lower() not in ("x86_64", "amd64")):
        raise RuntimeError("Current Zydis exact native/emitted ABI qualification requires execution on linux-x64")


def _cases(transcript):
    pattern = re.compile(rb"^@@ZQ CASE ([a-zA-Z0-9_./+-]+) BEGIN\n(.*?)^@@ZQ CASE \1 END (-?\d+)\n", re.M | re.S)
    result = {}
    for match in pattern.finditer(transcript):
        name = match[1].decode("ascii")
        if name in result:
            raise RuntimeError("Duplicate upstream result: " + name)
        result[name] = (match[2], int(match[3]))
    # An unfinished/crashed case cannot disappear merely because its END marker
    # was never printed. Require every marker to belong to one complete case.
    if transcript.count(b"@@ZQ CASE ") != len(result) * 2:
        raise RuntimeError("Malformed or incomplete upstream case transcript")
    return result


def _validate(ctx, group, transcript, inventory, label):
    passed = re.findall(rb"^CASE PASS (.+)$", transcript, re.M)
    if passed != [name.encode() for name in EXPECTED[group]] or b"CHECK FAILED" in transcript:
        raise RuntimeError(f"Incomplete/failed {label} assertion inventory: {passed!r}")
    kinds = {2: {"info"}, 3: {"encoder", "reencoder"}, 4: {"absolute"}}.get(group)
    if not kinds:
        return [dict(id=name, status="passed") for name in EXPECTED[group]]
    wanted = [row for row in inventory if row["kind"] in kinds]
    actual = _cases(transcript)
    if list(actual) != [row["id"] for row in wanted]:
        raise RuntimeError(f"{label} upstream inventory differs: {list(actual)}")
    results = []
    failed = []
    for row in wanted:
        content, status = actual[row["id"]]
        ok = status == 0
        if row["kind"] == "info":
            # This is upstream's documented sole normalization. All statuses,
            # whitespace, instruction metadata, text, tokens and bytes remain.
            expected = Path(row["expected"]).read_bytes().replace(b"\r\n", b"\n")
            expected_exit = getattr(ctx, "_zydis_info_exits", {}).get(row["id"], 0)
            ok = ((status & 255) == expected_exit and
                  (status == 0 or (not expected and (status & 0x80000000))) and
                  content.replace(b"\r\n", b"\n") == expected)
        results.append(dict(**row, c_return=status, zyan_status=f"{status & 0xFFFFFFFF:08X}",
                            status="passed" if ok else "failed"))
        if not ok:
            failed.append(row["id"])
    report = ctx.artifacts / (label + "-cases.json")
    report.write_text(json.dumps(results, indent=2) + "\n")
    if failed:
        raise RuntimeError(f"{label}: {len(failed)} upstream cases failed: {failed}; see {report}")
    return results


def _execute(ctx, command, group, label, inventory, native_abi=None):
    ctx.run([*command, str(group), *([native_abi] if group == 0 and native_abi else [])], label,
            timeout=600, separate=True, env={"NO_COLOR": "1", "FORCE_COLOR": ""})
    path = ctx.artifacts / (label + ".log")
    errors = path.with_suffix(".stderr").read_bytes()
    expected_errors = getattr(ctx, "_zydis_info_stderr", b"") if group == 2 else b""
    if errors != expected_errors:
        raise RuntimeError(f"Unexpected qualification stderr: {path.with_suffix('.stderr')}")
    data = path.read_bytes()
    result = _validate(ctx, group, data, inventory, label)
    return path, result


def _failure_control(ctx, command, label):
    data = ctx.run([*command, "5"], label, check=False, separate=True)
    record = next(row for row in reversed(ctx.receipt["commands"]) if row["label"] == label)
    record.update(expected_exit_code=1, deliberate_failure_control=True)
    ctx.save()
    if record.get("exit_code") != 1 or "CHECK FAILED" not in data or "deliberate failure control" not in data:
        raise RuntimeError("Qualification assertion failure did not propagate as exit 1: " + label)


def native(ctx):
    _target(ctx)
    if hasattr(ctx, "_zydis_native"):
        return ctx._zydis_native
    selected = _configuration(ctx)
    harness = _harness(ctx)
    units = [*[(path, ()) for path in selected["units"]],
             *[(unit.path, unit.flags) for unit in harness.test_units(ctx, ctx.sources["zydis"])],
             (ctx.root / "tests/native_main.c", ())]
    output = ctx.work / "zydis-native"
    output.mkdir(parents=True, exist_ok=True)
    compiler = shlex.split(os.environ.get("CC", "cc"))
    if not compiler:
        raise RuntimeError("Empty CC")
    ctx.run([*compiler, "--version"], "zydis-native-compiler")

    def compile_unit(row):
        i, (path, flags) = row
        label = f"zydis-native-{i:03d}-{path.stem}"
        obj = output / f"{i:03d}-{path.stem}.o"
        ctx.run([*compiler, *selected["flags"], *flags, "-O2", "-c", path, "-o", obj], label, timeout=600)
        if not obj.is_file():
            raise RuntimeError("Missing native object: " + str(obj))
        return obj

    with ThreadPoolExecutor(max_workers=2) as workers:
        objects = list(workers.map(compile_unit, enumerate(units)))
    binary = output / "zydis-native"
    ctx.run([*compiler, *objects, "-o", binary], "zydis-native-link")
    inventory = harness.inventory(ctx.sources["zydis"])
    report = dict(status="running", binary=str(binary), configuration={
        "units": [str(path) for path in selected["units"]], "flags": selected["flags"],
        "test_units": [str(path) for path, _ in units[len(selected["units"]):]], "jobs": 2},
        groups=[], cases=[])
    destination = ctx.artifacts / "zydis-native-report.json"
    try:
        # The upstream Python regression ignores tool status. Rejection cases
        # have empty snapshots. Verify those against a separately compiled,
        # unrenamed original CLI instead of assuming every fixture returns 0.
        original = output / "original-zydis-info"
        original_object = output / "original-zydis-info.o"
        ctx.run([*compiler, *selected["flags"], "-O2", "-c", ctx.sources["zydis"] / "tools/ZydisInfo.c",
                 "-o", original_object], "zydis-native-original-info-compile")
        tool_objects = [obj for obj in objects if obj.name.endswith(("-ZydisToolsShared.o", "-Terminal.o"))]
        ctx.run([*compiler, *objects[:len(selected["units"])], *tool_objects, original_object, "-o", original],
                "zydis-native-original-info-link")
        ctx._zydis_info_exits, errors = {}, []
        report["original_rejection_controls"] = []
        for row in inventory:
            if row["kind"] != "info" or Path(row["expected"]).read_bytes():
                continue
            label = "zydis-original-" + row["id"].replace("/", "-")
            ctx.run([original, *row["arguments"]], label, check=False, separate=True,
                    env={"NO_COLOR": "1", "FORCE_COLOR": ""})
            command = next(item for item in reversed(ctx.receipt["commands"]) if item["label"] == label)
            exit_code = command.get("exit_code")
            if not isinstance(exit_code, int) or exit_code < 0 or exit_code > 255:
                raise RuntimeError("Original upstream CLI crashed/failed to execute: " + row["id"])
            command.update(expected_exit_code=exit_code, upstream_rejection_control=True)
            ctx._zydis_info_exits[row["id"]] = exit_code
            if (ctx.artifacts / (label + ".log")).read_bytes() != Path(row["expected"]).read_bytes():
                raise RuntimeError("Original upstream rejecting snapshot differs: " + row["id"])
            error = (ctx.artifacts / (label + ".stderr")).read_bytes()
            errors.append(error)
            report["original_rejection_controls"].append(dict(id=row["id"], exit_code=exit_code,
                                                               stderr=error.decode("utf-8")))
        ctx._zydis_info_stderr = b"".join(errors)
        ctx.save()
        _failure_control(ctx, [binary], "zydis-native-failure-control")
        paths = {}
        for group, name in GROUPS.items():
            label = "zydis-native-run-" + name
            path, cases = _execute(ctx, [binary], group, label, inventory)
            paths[group] = path
            report["groups"].append(dict(group=group, name=name, transcript=str(path), status="passed"))
            report["cases"].extend(cases)
        report["status"] = "passed"
        report["inputs"] = _inputs(ctx, selected)
        report["passed"] = len(report["cases"])
        report["upstream_passed"] = len(inventory)
        report["failed"] = report["skipped"] = 0
    except BaseException as error:
        report.update(status="failed", error=str(error))
        raise
    finally:
        destination.write_text(json.dumps(report, indent=2) + "\n")
        ctx.receipt["zydis_native_report"] = str(destination)
        ctx.save()
    ctx._zydis_native = binary, paths, report
    return ctx._zydis_native


def _managed(ctx, form, mode, rooted=False):
    check_product(ctx, form)
    layout = Layout(ctx.root, COMPANION, ctx.recipe.default_profile)
    product = layout.project(ctx.profile, form)
    if not product.is_file():
        raise RuntimeError("Missing translated original-C qualification companion: " + str(product))
    current = _inputs(ctx, _configuration(ctx))
    manifest = product.parent / "zydis-test-inputs.json"
    if not manifest.is_file() or json.loads(manifest.read_text()) != current:
        raise RuntimeError("Qualification companion source/config/fixture identity differs from current native oracle; regenerate")
    cache = getattr(ctx, "_zydis_managed_commands", {})
    key = (str(product), form, mode, rooted)
    if key in cache:
        return cache[key]
    label = f"qualification-{form}-{mode}" + ("-rooted" if rooted else "")
    output = ctx.work / label
    args = ["-p:TranslatedProject=" + str(product), "--artifacts-path", output / "artifacts", "-m:1", "-o", output / "bin"]
    if rooted:
        args.append("-p:RootQualificationLibrary=true")
    project = ctx.root / "tests/Qualification/Qualification.csproj"
    if mode == "jit":
        ctx.managed("build", project, label + "-build", *args, timeout=1800)
        command = ["dotnet", output / "bin/Zydis.Qualification.dll"]
    else:
        ctx.managed("publish", project, label + "-publish", *args, "-r", ctx.options.rid,
                    "-p:PublishAot=true", timeout=3600)
        command = [output / "bin/Zydis.Qualification"]
    _failure_control(ctx, command, label + "-failure-control")
    cache[key] = command, label
    ctx._zydis_managed_commands = cache
    return cache[key]


def run(ctx, suite):
    if suite.name == "audit":
        return helper(ctx.root / "scripts/audit.py").run(ctx, suite)
    _, expected, report = native(ctx)
    if suite.name == "native":
        return
    groups = {"abi": [0], "qualification": [1], "upstream": [2, 3, 4], "rooted-aot": [0, 1, 2, 3, 4]}.get(suite.name)
    if groups is None:
        raise ValueError("Unknown required Zydis suite: " + suite.name)
    inventory = _harness(ctx).inventory(ctx.sources["zydis"])
    results = []
    destination = ctx.artifacts / ("zydis-" + suite.name + "-report.json")
    summary = dict(status="running", native_report=ctx.receipt["zydis_native_report"], cells=results)
    try:
        for form in forms(ctx):
            for mode in ("aot",) if suite.name == "rooted-aot" else modes(ctx):
                command, label = _managed(ctx, form, mode, rooted=suite.name == "rooted-aot")
                for group in groups:
                    name = f"{label}-run-{GROUPS[group]}"
                    path, cases = _execute(ctx, command, group, name, inventory, native_abi=expected[0])
                    same = path.read_bytes() == expected[group].read_bytes()
                    row = dict(form=form, mode=mode, group=group, status="passed" if same else "failed",
                               comparison="exact transcript bytes", native=str(expected[group]),
                               managed=str(path), passed=len(cases), failed=0 if same else 1,
                               skipped=0, cases=cases)
                    results.append(row)
                    if not same:
                        raise RuntimeError("Actual translated C differs from native: " + str(path))
        summary["status"] = "passed"
    except BaseException as error:
        summary.update(status="failed", error=str(error))
        raise
    finally:
        destination.write_text(json.dumps(summary, indent=2) + "\n")
        ctx.receipt.setdefault("zydis_qualification_reports", {})[suite.name] = str(destination)
        ctx.save()


def main():
    parser = argparse.ArgumentParser(__doc__)
    parser.add_argument("--native", action="store_true", required=True,
                        help="run native controls only, never builds managed tools or libraries")
    parser.add_argument("--work", type=Path, required=True)
    args = parser.parse_args()
    from campaigns.runner import Context
    from types import SimpleNamespace
    options = SimpleNamespace(action="test", profile="default", form="all", mode="all", rid="linux-x64",
                              suite=["native"], hashes="warn", fetch="never", tools="reuse", restore="none", fast=False, jobs=2)
    ctx = Context(ROOT.parent, load(ROOT.parent, "zydis"), options)
    ctx.work = args.work.resolve()
    ctx.artifacts = ctx.work / "logs"
    ctx.receipt["staging"] = str(ctx.work)
    ctx.receipt["scope"] = "native controls only; managed execution pending parent integration"
    ctx.start()
    try:
        native(ctx)
    except BaseException as error:
        ctx.receipt.update(status="failed", error=str(error))
        ctx.save()
        raise
    ctx.receipt["status"] = "passed"
    ctx.save()
    print("PASS native qualification: " + str(ctx.artifacts / "zydis-native-report.json"))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
