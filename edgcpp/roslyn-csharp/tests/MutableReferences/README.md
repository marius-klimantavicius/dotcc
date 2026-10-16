# Mutable-reference checker

The first immutable product campaign passed clean CLI build, production
publication and generic JIT native equality. This checker now binds the actual
published Cell representation. Its physical/JIT/NativeAOT execution and final
checker acceptance remain open until the coordinator records those results.

Build `Checks.csproj` with `GeneratedProject` set to the actual generated
`mutable-reference.cpp` project. This references its existing runtime transitively
and introduces no reader, native SQLite, native application provider or new
runtime implementation. Scalar application observations use the established
`Translated.Program.fixture` / `lifetime_*` interface.

The single run argument is the separate native observation JSON:

```text
Checks /mnt/storage/roslynobj-csharp-20261015/t5/native-mutable-reference-run-1/stdout.txt
```

The checker calls the fixture twice, compares every result, all nine scalar
values and every event against the complete native `runs` object, and requires
repeatability. The pinned native observation is result 54, values
`[8,12,5,7,0,11,23,1,0]`, events `[108,212,300,111,223,301]`, with zero overflow
and zero live protocol values. The first call uses distinct local objects; the
second passes the same local to both reference parameters. Field effects and
alias booleans are application observations, not source-level binding rules.

The event capacity is 64. `lifetime_live()` and value 8 are explicitly zero
protocol slots for trivial Cell: this checker claims no resource ownership,
constructor/destructor behavior or lifetime cleanup proof. JSON arrays use
explicit JsonNode values and output is AOT-safe without reflective DTO
serialization.

The actual first publication is
`t5/mutable-reference-cli-generated-1/Generated.cs`, under the campaign artifact
root. It declares `Translated.Cell` with explicit size 8/pack 4 and public
`first`/`second` fields at offsets 0/4. The checker uses these published names
directly, with no reflection or source-name reconstruction.

The independent second JSON line has key `mutableReferencePhysical` and fields
`cellSize`, `firstOffset`, `secondOffset`, `sequentialPrefixOffset`. It measures
complete size 8, actual field byte offsets 0/4 and natural CLR alignment 4 via
the actual Cell offset after a byte prefix in a sequential probe. The probe is
layout observation only; it invokes no application constructor/destructor and
does not add an ownership/lifetime claim.

Scope remains the exact selected nonthrowing reference helper and fixture.
No copy-in/copy-out, noalias assumption, reference return, EH, callback or
provider capability is introduced. Captured DuckDB helpers with absent
noexcept facts remain outside this admission. The existing aggregate-local
initialization path is unchanged; source capture 1 remains preserved.
