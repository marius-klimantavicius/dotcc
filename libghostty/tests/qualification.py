"""Execute exact assertion transcripts against raw/processed JIT/rooted-AOT."""
import copy
import json
import os

from campaigns.recipes import helper
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def run(ctx, suite):
    subject = copy.copy(ctx)
    subject.profile = "qualification"
    project = ctx.root / "tests/Qualification/Qualification.csproj"
    expected = "".join((ctx.root / "tests/native" / name).read_text(encoding="utf-8")
                       for name in ("expected.txt", "boundary-expected.txt", "abi-expected.txt"))
    results = []
    for form in forms(ctx):
        translated = check_product(subject, form)
        prop = "-p:QualificationProject=" + str(translated)
        for mode in modes(ctx):
            label = "qualification-" + form + "-" + mode
            output = ctx.work / label
            if mode == "jit":
                ctx.managed("build", project, label + "-build", prop, "-m:1", "-o", output)
                command = ["dotnet", output / "GhosttyVt.Qualification.dll"]
            else:
                ctx.managed("publish", project, label + "-publish", prop, "-m:1",
                            "-r", ctx.options.rid, "-p:PublishAot=true", "-o", output, timeout=3600)
                command = [output / ("GhosttyVt.Qualification" + (".exe" if os.name == "nt" else ""))]
            observed = ctx.run(command, label + "-run", timeout=suite.timeout, separate=True)
            if observed != expected:
                raise RuntimeError(label + ": actual assertion transcript differs from native controls")
            results.append(dict(form=form, mode=mode, project=str(translated), passed=24,
                                failed=0, skipped=0, whole_library_rooted=mode == "aot"))
    path = ctx.artifacts / "qualification.json"
    path.write_text(json.dumps(dict(cases=24, results=results), indent=2) + "\n", encoding="utf-8")
    ctx.receipt["qualification_report"] = str(path)


def run_upstream(ctx, suite):
    return _run_upstream(ctx, suite, core=False)


def run_upstream_core(ctx, suite):
    module = helper(ctx.root / "tests/upstream/core.py")
    parts = module.partitions(ctx.root)
    if ctx.profile.startswith("upstream-core-"):
        parts = tuple(part for part in parts if part["profile"] == ctx.profile)
    results = [_run_upstream(ctx, suite, core=True, partition=part["id"]) for part in parts]
    report = ctx.artifacts / "upstream-core-managed.json"
    report.write_text(json.dumps(dict(cases=sum(row["cases"] for row in results),
        partitions=len(parts), results=results), indent=2) + "\n", encoding="utf-8")
    ctx.receipt["upstream_core_managed_report"] = str(report)


def _run_upstream(ctx, suite, *, core, partition=None):
    subject = copy.copy(ctx)
    subject.profile = ("upstream-core-" + partition if partition else "upstream-core") if core else "upstream-render"
    project = ctx.root / "tests/Upstream/Upstream.csproj"
    if core:
        module = helper(ctx.root / "tests/upstream/core.py")
        selection = module.selection_for(ctx.root, partition)
        expected = module.expected_transcript(ctx.root, partition)
    else:
        selection = json.loads((ctx.root / "tests/upstream/selection.json").read_text(encoding="utf-8"))
        expected = (ctx.root / "tests/upstream/expected.txt").read_text(encoding="utf-8")
    count = selection["expected_cases"]
    results = []
    original = None
    cwd = None
    if core:
        pin = json.loads((ctx.root / "config/source.json").read_text(encoding="utf-8"))
        cwd = ctx.sources.get("product", ctx.root / "ref" / pin["directory"])
        fixture = cwd / "test/fuzz-libghostty/corpus/stream-cmin"
        if not fixture.is_dir():
            raise RuntimeError("Required pinned core corpus is unavailable: " + str(fixture))
    for form in forms(ctx):
        translated = check_product(subject, form)
        prop = "-p:UpstreamProject=" + str(translated)
        for mode in modes(ctx):
            label = subject.profile + "-" + form + "-" + mode
            output = ctx.work / label
            fixture_props = ["-p:CoreFixtureIo=true"] if core else []
            if mode == "jit":
                ctx.managed("build", project, label + "-build", prop, *fixture_props, "-m:1", "-o", output)
                command = ["dotnet", output / "GhosttyVt.Upstream.dll", str(count)]
            else:
                ctx.managed("publish", project, label + "-publish", prop, *fixture_props, "-m:1", "-r", ctx.options.rid,
                            "-p:PublishAot=true", "-o", output, timeout=3600)
                command = [output / ("GhosttyVt.Upstream" + (".exe" if os.name == "nt" else "")), str(count)]
            observed = ctx.run(command, label + "-run", timeout=suite.timeout, separate=True, cwd=cwd)
            summary = f"RESULT upstream-cases={count} passed={count} skipped=0 failed=0 live=0\n"
            if not observed.endswith(summary) or sum(line.startswith("PASS ")
                    for line in observed.splitlines()) != count:
                raise RuntimeError(label + ": upstream case/status inventory failed")
            if observed != expected:
                raise RuntimeError(label + ": upstream assertion transcript differs from original native control")
            if original is not None and observed != original:
                raise RuntimeError(label + ": upstream case transcript differs across forms/modes")
            original = observed
            results.append(dict(form=form, mode=mode, project=str(translated), passed=count,
                                failed=0, skipped=0, live_allocations=0, whole_library_rooted=mode == "aot"))
    path = ctx.artifacts / (subject.profile + "-managed.json")
    path.write_text(json.dumps(dict(cases=count, results=results), indent=2) + "\n", encoding="utf-8")
    ctx.receipt[subject.profile.replace("-", "_") + "_managed_report"] = str(path)
    return dict(profile=subject.profile, partition=partition, cases=count, results=results, report=str(path))
