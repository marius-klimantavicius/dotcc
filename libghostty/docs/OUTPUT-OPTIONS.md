# Generated namespace and aggregate initializers

Status updated 2026-10-30. Measurements and receipts below record historical
C-route output evidence; current retained C P6 gates pass in `001153-ab4e8deb`. The user
deferred upstream test suites, their 2,622-case matrix and upstream-only host-I/O
probe to direct Zig translation. They remain required in that later phase and
are not counted as passed; see [upstream test qualification](UPSTREAM-TESTS.md).

The generated class is `Managed.GhosttyVt`. The owning `Terminal`,
`TerminalCell`, `TerminalPosition` and `TerminalTextStyle` types use namespace
`Managed`; project, assembly and folder names are unchanged.

```csharp
using Managed;
using var terminal = new Terminal(80, 24);
terminal.Write("Hello\r\nworld");
Console.WriteLine(terminal.Snapshot());
```

`config/translation.json:aggregate_data=true` enables `--aggregate-data` during
source/object emission for every profile. The flag is not passed when linking
existing objects. The shared `--literal-pool` link default remains enabled.
Generated C# is never edited manually.

Measured canonical scalar source bytes:

| Output | Before | After |
| --- | ---: | ---: |
| Main `GhosttyVt.cs` (both forms) | 11,586,042 | 9,314,264 |
| All raw C# | 29,186,041 | 26,865,847 |
| All processed C# | 29,363,698 | 27,043,504 |

Each form has 58 C# files and 12 aggregate-data sites, copying 193,924 logical
bytes in 52 chunks. This is a combined namespace and aggregate-data comparison;
the reduction is not attributed solely to aggregate data. The compiler retains
normal typed initialization when proof is unavailable. Examples include a
12,512-byte table of pointer-bearing slices and a 16-byte record below the
1,024-byte threshold. Endian/size guards and copies into the original mutable
C storage preserve the selected LP64 little-endian layout.

Evidence: `artifacts/user-output-options-baseline.json`,
`artifacts/user-output-options-size-report.json`, and
`artifacts/useroptsanalysis/aggregate-data.json`. Canonical regeneration receipt:
`artifacts/campaign/20261029-173744-b79eaedb/receipt.json`. The root solution and
owning consumer pass raw/processed JIT and NativeAOT after the change
(`174224-0297ff72`; `artifacts/user-output-options-root-build.log`). The separate
24-case qualification profile also passes all four rows with these choices
(`174734-04d23ea5`). These historical passes do not close final C-route reproduction.

## Bounded expression factories

The later factory repair retains both output choices above. Canonical normal run
`181517-e418fbb7` uses immutable span payloads for constant inline arrays,
including proven named records/bitfields, and typed value argument packs for
runtime values. Helpers take at most two parameters (Create takes zero or one),
compared with the former maximum of 8,192. Whole initializer values evaluate in
order; primitive floating payloads use typed C arithmetic at each operation.
Native reductions and four Release direct/object execution cases cover those
semantics, bit patterns, zero fill, callbacks and storage lifetime.

| Source bytes | Before factory repair | After factory repair |
| --- | ---: | ---: |
| Main file, both forms | 9,314,264 | 8,910,123 |
| All raw C# | 26,865,847 | 26,267,155 |
| All processed C# | 27,043,504 | 26,444,812 |

Each form now has 57 C# files. Actual Release PE audits find 93 helper types,
186 helper methods, maximum arity two and maximum helper IL size 77 bytes.
Across the product's payload pool, 3,277 byte/ushort/uint/ulong getters directly
reference matching RVA data and contain no `newarr`; their logical payloads
sum to 249,295 bytes. All 92 zero-argument value factories and every getter
allocate zero bytes in the warmed measurements. The one argument-taking
factory receives IL/callgraph inspection and execution-fixture coverage.

Acceptance reports: `artifacts/aggregate-factories-{raw,processed}-accepted.json`.
The standalone audit first warms the complete getter set, then repeats the full
strict zero-byte measurement with default process tiering. Earlier one-pass
reports captured one-time runtime/JIT initialization at varying getters and are
retained; the threshold is not relaxed. Type/reflection/delegate setup and the
full warmup are explicit exclusions. This establishes RVA and executed JIT
allocation behavior, separately from terminal/NativeAOT qualification.

The corrected owning consumer passes raw/processed JIT and NativeAOT in
`181936-f36c22c9`; the root solution also builds. The regenerated 24-case differential profile passes all four rows, including
whole-library-rooted NativeAOT (`182713-753e56db`). Final C-route P6 acceptance
is accepted for the retained C scope; upstream matrix acceptance is deferred to direct Zig.

The source-identical private allocation diagnostic additionally passes both
forms under JIT and whole-assembly-rooted NativeAOT. Each control executes all
3,277 getters and 92 constant factories with exactly zero measured allocations;
positive controls detect allocations and returned-byte checksums match the PE
report. The original products remain unchanged and contain no diagnostic
entrypoint. See `tests/FactoryAllocation/RESULTS.md` and its retained receipt.
These are source-identical private executables, separately identified from the
canonical library binaries and their actual terminal consumers.

The normal recipe now declares `factory-allocation` as a required verification
suite, creating current PE/RVA reports rather than requiring an ignored
historical artifact. Its standard CLI raw/JIT integration passes in
`184722-f2b92a93`; standalone controls already cover all four rows. Final retained C verification `001153-ab4e8deb` passes the complete suite against current artifacts.
