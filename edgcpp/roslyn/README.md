# EDG RoslynObj

RoslynObj exports the authoritative EDG intermediate language into a normalized
SQLite object and reads it through an immutable .NET object model. Each object
contains one independent translation unit. The library does not parse C++, link
translation units or depend on Microsoft.CodeAnalysis.

The user excluded C++/CLI and C++/CX support from the revised task. Native
Microsoft C++ extensions remain in scope.

Phases 3–4/M0–M5 are accepted at initial public format 1.0: 251 tables and 39,145
stable kind IDs. Fourteen strict pinned profiles, 1,524 verified required field
rows with none pending, three original upstream positive tests and a practical
1.24M-entity real C++ translation unit have scoped acceptance. Two selected stock module-partition
input limits and optional-source losses remain explicit; this does not claim all
catalog values were activated or a future translator is implemented. See the
[implementation ledger](docs/IMPLEMENTATION.md), [accepted evidence](docs/ACCEPTANCE.md)
and [profiles](docs/PROFILES.md).

The current producer emits format 2.0 with fingerprint
`3e818d17da55433edff5f5c9b035c58110e953371c3c6fa09cb99d0f6f84b5f6`.
It preserves actual null most-derived type pointers on retained prototype base
placeholders. The reader explicitly admits this contract and the published 1.0
fingerprint `4bb94e5feda94e85fe4b6fe4e8eb5f3494f3b112d14a56dc71e6b069861a2559`.
Numeric versions alone do not admit earlier development objects. Complete snapshots
and supported enabled required capabilities remain required;
[format policy](docs/FORMAT.md) documents exact admission and identity scope.
There is no migration or forward-compatibility promise.

[DuckDB core-library generation](docs/DUCKDB.md) is accepted for all 164 configured
core inputs: 97.735 GiB of independent objects, each admitted by the reader, with
no core failures or unmapped semantic flags. The user's final scope excludes tests
and separate extension/dependency targets.

[SQLite amalgamation generation](docs/SQLITE.md) is accepted for untouched SQLite
3.54.0 with JSON/JSONB, FTS3/4/5, WAL and the documented optional modules enabled.
One 283.77 MiB object passes reader admission and bounded feature navigation.

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
facts on their actual owners and have genuine public acceptance. The field worksheet records the exact owner proof and explicit dispositions.
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
