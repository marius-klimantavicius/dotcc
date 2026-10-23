"""Focused regression gates for shared compiler/runtime changes in this campaign."""
import json
import re


def run(ctx, suite):
    groups = (
        ("DotCC.Tests", "FullyQualifiedName~GnuCleanupTests|FullyQualifiedName~AttributeTests|FullyQualifiedName~ThreadLocalTests|FullyQualifiedName~AggregateArrayDimensionsTests|FullyQualifiedName~InlineFunctionTests|FullyQualifiedName~InlineExportBindingTests|FullyQualifiedName~SemanticFunctionOverrideTests|FullyQualifiedName~FunctionOverrideDirectCallTests|FullyQualifiedName~VaListLifetimeTests|FullyQualifiedName~VarargTests|FullyQualifiedName~MemoryResidencyTests|FullyQualifiedName~CPythonR14MappingTests"),
        ("DotCC.FunctionalTests", "FullyQualifiedName~StaticCallbackOverrideTests|FullyQualifiedName~InlineReachabilityTests|(FullyQualifiedName~FixtureTests.Fixture_emits_csharp_runnable_with_matching_stdout&(DisplayName~gnu-cleanup|DisplayName~c11-thread-local-initializers|DisplayName~braced-pointer-initializer|DisplayName~va-list-braced-zero|DisplayName~implicit-outer))"),
    )
    rows = []
    for project, selection in groups:
        transcript = ctx.run(["dotnet", "test", ctx.repo / project / (project + ".csproj"), "-c", "Release",
                              "--filter", selection, "--logger", "console;verbosity=minimal"],
                             "shared-" + project, timeout=1800)
        match = re.search(r"Failed:\s*(\d+), Passed:\s*(\d+), Skipped:\s*(\d+), Total:\s*(\d+)", transcript)
        if not match or int(match[1]) or int(match[3]) or int(match[4]) == 0:
            raise RuntimeError("Missing/nonpassing focused test result: " + project)
        rows.append({"project": project, "filter": selection, "passed": int(match[2]), "skipped": int(match[3])})
    (ctx.artifacts / "shared-regressions.json").write_text(json.dumps({"status": "passed", "rows": rows}, indent=2) + "\n")
