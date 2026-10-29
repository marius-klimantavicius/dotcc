"""Current-product PE/RVA and source-identical JIT/NativeAOT allocation gates."""
import json
import sys

from campaigns.recipes import json_file
from campaigns.testing import forms, modes
from campaigns.translation import check_product


def run(ctx, suite):
    policy = json_file(ctx.root / "config/translation.json")
    bound = policy["maximum_factory_parameters"]
    if not isinstance(bound, int) or isinstance(bound, bool) or bound < 0:
        raise RuntimeError("maximum_factory_parameters must be a nonnegative integer")
    audit_project = ctx.root / "tests/AggregateFactoryAudit/AggregateFactoryAudit.csproj"
    audit_outputs = ctx.work / "factory-pe-audit"
    ctx.managed("build", audit_project, "factory-pe-audit-build", "-m:1",
                "--artifacts-path", audit_outputs)
    audit_tool = audit_outputs / "bin/AggregateFactoryAudit/release/GhosttyVt.AggregateFactoryAudit.dll"
    if not audit_tool.is_file():
        raise RuntimeError("Missing built factory PE auditor: " + str(audit_tool))
    # Historical measurements are useful comparison evidence, never mandatory
    # provenance. Reproduction works with only the checked-in configuration.
    baseline = ctx.artifacts / "factory-comparison-baseline.json"
    baseline.write_text(json.dumps(dict(
        historical_measurements="See docs/OUTPUT-OPTIONS.md; not an acceptance input",
        maximum_factory_parameters=bound), indent=2) + "\n", encoding="utf-8")
    selected_forms = forms(ctx)
    command = [sys.executable, ctx.root / "tests/FactoryAllocation/run.py",
               "--work", ctx.work / "factory-allocation", "--rid", ctx.options.rid,
               "--forms", *selected_forms,
               "--modes", *("nativeaot" if mode == "aot" else mode for mode in modes(ctx))]
    for form in selected_forms:
        project = check_product(ctx, form)
        ctx.managed("build", project, "factory-" + form + "-product-build", "-m:1")
        assembly = project.parent / "bin/Release/net10.0/TranslatedGhosttyVt.dll"
        report = ctx.artifacts / ("factory-" + form + "-pe.json")
        ctx.run(["dotnet", audit_tool, assembly, project.parent, baseline, report, str(bound)],
                "factory-" + form + "-pe-audit", timeout=suite.timeout)
        command.extend(["--" + form + "-project", project, "--" + form + "-audit", report])
    ctx.run(command, "factory-allocation-matrix", timeout=suite.timeout)
    report = ctx.work / "factory-allocation/receipt.json"
    data = json.loads(report.read_text(encoding="utf-8"))
    if data.get("status") != "pass":
        raise RuntimeError("Source-identical factory allocation matrix did not pass")
    ctx.receipt["factory_allocation_report"] = str(report)
    summary = ctx.artifacts / "factory-allocation.json"
    summary.write_text(json.dumps(data, indent=2) + "\n", encoding="utf-8")
