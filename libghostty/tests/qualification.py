"""Execute exact assertion transcripts against raw/processed JIT/rooted-AOT."""
import copy
import json
import os

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
    subject = copy.copy(ctx)
    subject.profile = "upstream-render"
    project = ctx.root / "tests/Upstream/Upstream.csproj"
    selection = json.loads((ctx.root / "tests/upstream/selection.json").read_text(encoding="utf-8"))
    count = selection["expected_cases"]
    expected = (ctx.root / "tests/upstream/expected.txt").read_text(encoding="utf-8")
    results = []
    original = None
    for form in forms(ctx):
        translated = check_product(subject, form)
        prop = "-p:UpstreamProject=" + str(translated)
        for mode in modes(ctx):
            label = "upstream-" + form + "-" + mode
            output = ctx.work / label
            if mode == "jit":
                ctx.managed("build", project, label + "-build", prop, "-m:1", "-o", output)
                command = ["dotnet", output / "GhosttyVt.Upstream.dll", str(count)]
            else:
                ctx.managed("publish", project, label + "-publish", prop, "-m:1", "-r", ctx.options.rid,
                            "-p:PublishAot=true", "-o", output, timeout=3600)
                command = [output / ("GhosttyVt.Upstream" + (".exe" if os.name == "nt" else "")), str(count)]
            ctx.run(command, label + "-run", timeout=suite.timeout, separate=True)
            observed = (ctx.artifacts / (label + "-run.stdout.log")).read_text(encoding="utf-8")
            summary = f"RESULT upstream-cases={count} passed={count} skipped=0 failed=0 live=0\n"
            if not observed.endswith(summary) or sum(line.startswith("PASS terminal.c.render.test.")
                    for line in observed.splitlines()) != count:
                raise RuntimeError(label + ": upstream case/status inventory failed")
            if observed != expected:
                raise RuntimeError(label + ": upstream assertion transcript differs from original native control")
            if original is not None and observed != original:
                raise RuntimeError(label + ": upstream case transcript differs across forms/modes")
            original = observed
            results.append(dict(form=form, mode=mode, project=str(translated), passed=count,
                                failed=0, skipped=0, live_allocations=0, whole_library_rooted=mode == "aot"))
    path = ctx.artifacts / "upstream-managed.json"
    path.write_text(json.dumps(dict(cases=count, results=results), indent=2) + "\n", encoding="utf-8")
    ctx.receipt["upstream_managed_report"] = str(path)
