# EDG RoslynObj

RoslynObj exports the authoritative EDG intermediate language into a normalized
SQLite object and reads it through an immutable .NET object model. Each object
contains one independent translation unit. The library does not parse C++, link
translation units or depend on Microsoft.CodeAnalysis.

The user excluded C++/CLI and C++/CX support from the revised task. Native
Microsoft C++ extensions remain in scope.

Phases 3–4 are approved and in progress. The current251-table format passes
4,437 reader assertions across211 distinct objects and14 strict pinned profiles.
Full M0–M5 acceptance is tracked in [the implementation ledger](docs/IMPLEMENTATION.md),
[remaining acceptance](docs/ACCEPTANCE.md), and [compatibility profiles](docs/PROFILES.md).
There are 1,503 verified field rows and 25 required proof rows still open, with two
explicit selected-stock module-input limitations; table or
kind presence alone does not establish complete semantic-family coverage. The latest
bounded dependent-constant gate passes 94 assertions on two fresh objects;
the format remains unchanged.

The build uses a clean EDG 7.0 checkout pinned at
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f`. The default materialized checkout is
`/tmp/roslynobj-edg-upstream-20261012`; staging and generated artifacts live under
`edgcpp/artifacts/roslynobj`. The preparer verifies core source against the pin and
overlays only build configuration and the new backend. Existing modified EDG
sources in the workspace are preserved.

From the repository root:

```sh
python3 edgcpp/roslyn/build/build.py --jobs 8
python3 edgcpp/roslyn/build/roslyn-cpfe.py --c++20 input.cpp -o input.roslynobj
dotnet build edgcpp/roslyn/managed/RoslynObj/RoslynObj.csproj
dotnet run --project edgcpp/roslyn/managed/RoslynObj.Tests/RoslynObj.Tests.csproj -- input.roslynobj
```

The native producer requires SQLite 3.37 or newer. The managed project targets
.NET 10/C# 14 and pins Microsoft.Data.Sqlite 10.0.12. The wrapper forwards EDG
arguments and maps `-o` to atomic object publication. `ROSLYNOBJ_COMPILER` selects
an independently built compatibility-profile compiler.

Ordinary property navigation covers functions, current ABI parameters, types,
layouts, executable bodies, exact constants, templates, partial exception cleanup
recipes, source positions and macro associations. See
[the managed usage guide](managed/README.md). Optional retained relationships are
null or empty. The captured profile documents lowering losses such as unavailable
original virtual-call targets. Scoped availability rows distinguish retained, dependent, disabled and lost
facts on their actual owners and have genuine public acceptance. Remaining owner-field proof is tracked separately.
Lost source associations are not reconstructed.

The shared contract lives in `schema/generate.py` and the authoritative append-only
`schema/kinds.tsv`. Regenerate with `python3 edgcpp/roslyn/schema/generate.py` and
`python3 edgcpp/roslyn/managed/tools/generate_bindings.py`. SQL stores typed scalar
columns and ordered foreign-key relationships, with exact target integers as
decimal text. Source occurrences, shared value content and addressable storage
remain distinct identities.

Integration helpers operate on valid compilations and snapshots:

```sh
python3 edgcpp/roslyn/tests/inspect.py input.roslynobj
python3 edgcpp/roslyn/tests/publication.py
python3 edgcpp/roslyn/tests/measure_packaging.py input.roslynobj --output-directory /tmp/roslynobj-pages
```

The field and kind worksheets record extraction, API, fixture and guarded
exclusion evidence. No translator project is included in these phases.
