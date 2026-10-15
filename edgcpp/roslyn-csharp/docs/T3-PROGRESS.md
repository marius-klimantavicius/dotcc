# SQLite implementation progress

The unchanged full-feature object still has no complete generated project or SQL
execution. The first full library-emission diagnostic pass reported 596 first
blockers in 270.91 seconds. After the bounded control/intrinsic/provider changes,
the second reports 313 in 298.35 seconds. The third reports 57 in 340.62
seconds, within the fixed 900-second/8-GiB budget. These are first failures per body/representation, not exhaustive counts
or a completion percentage. All three attempts publish no generated output.

[Pass 1](../acceptance/sqlite/emission-diagnostics/pass-1.json) uses candidate
system bindings with the original error protocol. [Pass 2](../acceptance/sqlite/emission-diagnostics/pass-2.json)
explicitly selects `LinuxX64LogicalErrno` and the bounded recorded-intrinsic
implementation. All 88 system bindings remain candidates from the export audit;
the focused provider proof does not establish every SQLite provider contract.
Pass 2 SHA256 is
`e3d4933ab793aa7d9f70703ef4e0b856fe19498c778f7de3b837e3a4384ea0b6`.
The measured runner records exit 1 for diagnostic rejection, with no resource
violation. It is an expected incomplete-translation outcome.

[Pass 3](../acceptance/sqlite/emission-diagnostics/pass-3.json), SHA256
`907eb2dbc06f72d1d4f49caf4ba749c60ab0a1effe8028b112fca560c5853dd3`,
uses immutable snapshot 3. It records 57 first blockers, exit 1 and no budget
violation (about 1.02 GB sampled process-tree RSS).

| Pass 3 family | First blockers | Next ownership |
| --- | ---: | --- |
| Compound pointer offset/count representation | 30 | Planner worker; actual signed/unsigned natural widths |
| Volatile representation/access | 20 | Planner worker and coordinator pure representation/local-init hooks |
| Recipe-free static strings and zero record | 5 | Storage/provider worker; captured constant/zero facts |
| Compound shift operand width | 2 | Planner worker; actual unsigned32/64 and signed32/64 counts |

Goto/switch, missing builtin binding and opaque-record value-layout families no
longer appear in pass 2. Opaque pointer views do not demand complete pointee
storage. Public storage facts further narrow the remaining work: 132 static
locals have one `initk_static` recipe and no dynamic initializer/cleanup; the
inventory has no TLS. SQLite's incomplete array fields are actual flexible tails
at captured offsets, including Fts5Sorter.aIdx at offset 28 within its 32-byte
header padding. They do not add a fabricated CLR element field. Its two unsigned 128-bit locals have size and
alignment 16, value arithmetic/casts/shifts and no retained address-taking;
addressable storage needs separate alignment handling.

The completed bounded proofs are:

- [Statements](../acceptance/statements/report.json): native/JIT/NativeAOT result
  23 and all 23 events, twice per checker. Captured targets preserve nested and
  cross-arm entry, skipped initialization, switch fallthrough/default/no-match/
  range, for increment and end-tested continue. Only entered scopes flatten;
  ordinary loops and conditionals remain structured. C++ cleanup stays guarded.
- [Recorded intrinsics](../acceptance/intrinsics/report.json): 27 expected phase
  outcomes, 34 exact observations, typed comma and 1000 ordered handoffs totaling
  500500, JIT/AOT twice. Seven relaxed atomic forms, fence, swaps/clz and signed
  64-bit overflow are admitted from actual public facts. Two valid unsupported
  inputs reject before output. This standalone proof has only Generated and
  Runtime dependencies. The [production CLI proof](../acceptance/intrinsics/cli-report.json)
  now passes 18 expected outcomes: all 15 required imports select recorded
  runtime lowering, exact completion hashes, the existing checker in JIT/AOT,
  and both valid unsupported inputs reject before publication.
- [Logical errno/provider](../acceptance/errno-provider/report.json): 17 phases,
  native/JIT/AOT 6223 and all 24 observations, canonical direct/indirect accessor,
  deliberate raw-TLS interference/GC, nested qsort and foreign pthread, twice.
  Exact ABI-only exports, native layout assertions, compiler/provider/build
  hashes, deterministic output and copied-library identities pass. See
  [the explicit contract and limits](T3-ERRNO-PROVIDER.md).

- [Memory plans](../acceptance/memory-plans/report.json): 21 expected outcomes,
  all 24 observations and four events in JIT/AOT; immediate pointer strides,
  signed32 offsets, unsigned64 shifts, unsigned16/32 volatile accesses and
  branch-local planned variadic calls. Broader widths remain separate work.
- [Static values, tails and unsigned128](../acceptance/value-storage/report.json):
  native/JIT/AOT static state 306/313 with all 15 observations, allocated tails
  with 12 observations and physical header/offset/stride checks, eight exact
  unsigned128 values, and actual C aggregate-prvalue reset result 4. Repeated
  generated files match. Addressable unsigned128 rejects through the CLI before
  output. The earlier manual 406/413 prediction was a checker arithmetic error;
  the exact native observations govern this comparison.
- [Selected glibc loader](../acceptance/loader-provider/report.json): 21 phases,
  native/JIT/AOT 17 observations, canonical/unknown-indirect paths, copied error
  lifetime, read-clear and raw loader interference. It uses registered provider
  addresses consistently. See [storage and loader limits](T3-STORAGE-LOADER.md).

Pass 3 exposed four direct static string recipes and one zero-initialized record,
plus natural pointer/shift operand widths, signed32 and binary64 volatile values
and local initializers. Narrow fixes and activation fixtures are subsequent
working changes, outside the frozen receipts above. The matching loader TLS
pointer is also being cleared before destructor release; native TLS-destructor
reentry remains unexercised. No complete SQLite source/build/SQL claim is made.

Full artifacts remain under `/mnt/storage/roslynobj-csharp-20261015/t3`.
The initial discarded-void-cast build failure, intrinsic argument-role rejection,
missing fixture header/macro invocations and earlier exploratory proofs remain
preserved. No producer/core changes, SQLite re-export, feature reduction or
native application fallback was used.
