# Pinta implementation evidence

The Linux translation and owning API are implemented; full native parity and
edge-case qualification remain incomplete. Windows x64 is explicitly deferred by the user;
Linux x64 is the active qualification scope. No full P0–P6 acceptance gate is
claimed yet. The [plan](PLAN.md) defines the scope; this record separates
executed observations from authored code awaiting validation.

| Area | Observed result |
| --- | --- |
| Immutable inputs | Pinned archive, explicit 29-unit manifest, licenses and all 50 fixture hashes recorded. See [SOURCE.md](SOURCE.md). |
| Existing unit baseline | 2,216 passed, zero failed/skipped, Release, .NET SDK 10.0.111. |
| Existing functional baseline | 487 passed, one failed, 1,005 opt-in skips. The pre-change failure is `LiteralPoolTests.Pooled_bytes_survive_gc_and_are_collected_with_the_library`: assembly remains alive. |
| Shared regression checks | Updated unit suite passes 2,218 tests. Full functional rerun passes 490 tests with 1,009 opt-in skips; the baseline literal-pool unloading failure did not recur. Targeted functional selection also passes 18 checks with 51 opt-in skips. The enabled WAT execution oracle additionally passes 146 checks with zero skips; no external Zig compiler is installed. |
| Lua preservation | Regenerated all 33 interpreter units with the updated compiler. The full conformance runner exits zero and reports `final OK !!!`. |
| Chibi preservation | Regenerated all nine interpreter units. All 1,225 R7RS checks and 18 subgroups pass; output matches the saved native baseline after timing/ANSI normalization. |
| picotls preservation | Regenerated product passes its full raw/optimized JIT/NativeAOT campaign: 92 ABI checks, 2,962 provider checks, 232 upstream-vector checks, TLS scenarios and 56 peer cases per form. |
| MsQuic preservation | Regenerated the 47-unit product and extra ABI probe. All 60 native/JIT/AOT host observations, 29 public ABI observations and raw/optimized rooted product consumers pass. All 32 public stream/resumption and authentication cases pass across IPv4/IPv6 and all four managed forms. Provenance now reads actual metadata from hash-verified compiler snapshots, fixing an initial mismatch with mutable intermediate SDK metadata; the failed log and successful retry are retained. |
| SQLite preservation | Full product regenerated with the updated compiler and semantic postprocessor. Its separate managed consumer passes JIT/NativeAOT SQL/WAL, JSONB, FTS5, cached callbacks, nested SQL, GC/cleanup and UTF-16LE/BE checks. All seven independently regenerated native-reference corpora pass: core, API (also NativeAOT), VFS, virtual table, allocation, 37 upstream JSONB cases, and FTS5. Compiler hash remained unchanged throughout. |
| Original native release | All 29 core units build. Thirteen groups complete with 1,832 successful checks. The v2 group crashes after opening `globals-properties-v2.pint`; weak/encoding groups abort because their upstream registration omits SPUT's test wrapper. |
| Native ABI/text | Linux x64 measurements include actual 16-bit literals, 64-bit pointers, aggregate sizes and offsets. See [NATIVE-VALIDATION.md](NATIVE-VALIDATION.md). |
| Corrected native release/debug | Fifteen of 16 groups and 72 of 73 isolated cases complete. The globals-properties v2 case still crashes. After the heap-allocation guards, the creation sweep completes: 3,474 successful initializations and 623 rejected arena sizes, with canaries intact. Misaligned tiny-arena and stack-length-overflow checks also pass. Final string/character/weak allocation probes each retain 41 rooted integers, then return OOM status 4 with a null result. The native callback result survives confirmed object and payload relocation, with one open, 43 three-byte reads and one close. |
| Repeated execution | Native second Execute returns success while leaving a deliberately changed global unchanged: the module does not rerun. The owning facade rejects a second execution on that engine; raw behavior remains available. |
| Receipt workload | An authored fixed bytecode fixture computes quantity 3 times unit price 12.50, stores total `37.5`, and emits `Customer: Ada\nTotal: 37.5\n`. The native oracle verifies all 52 UTF-16LE output bytes and the total. Six similarly named upstream receipt/configuration fixtures are zero bytes; they are not used as successful workload evidence. |
| Actual-core translation | All 29 corrected core units emit and pass semantic postprocessing. Raw and optimized libraries build without warnings/errors. Reduced compiler fixes cover typedef/member names, pasted wide literals, C identifiers named `null`, and wide-character conversion. The native globals-property discrepancy prevents full VM parity. |
| Owning facade and consumer | Final Linux release and diagnostic profiles each pass raw/optimized × JIT/whole-library NativeAOT: 34 native ABI observations and 46 behavior checks in each form, with identical output and zero build warnings/errors. Includes Unicode/NUL/surrogate globals, receipt bytes, copied lifetime, callback returns after compaction, three-byte reads and early EOF cleanup, imports/missing imports, swapped-endian input, three allocation OOM guards, failure/reentry, creation/disposal, four concurrent engines and bounded-heap Pinta/CLR GC stress. |
| Translated upstream tests | All four Linux forms in each of the release and diagnostic profiles run 73 of 73 original test bodies and pass 2,034 assertions each. Seventy-two cases match native output exactly after removing timing/stream-order differences. Managed `globals_properties_v2` passes its 54 assertions while native crashes; this remains a differential failure, not parity success. |
| Dependency audit | Final release and diagnostic source/project closures pass the static audit with translation input/output hashes verified. Both JIT consumer dependency closures pass per profile; whole-library NativeAOT consumers publish and execute without a native Pinta dependency. |
| Windows x64 | Deferred by explicit user instruction: "ignore win x64 for now." No execution claim. |
| Workload measurements | Native and all four managed forms in each release/diagnostic profile validate 1,000 receipt executions. Timings, unmanaged budgets, CLR allocation counts, image sizes and peak process memory are recorded. Each execution includes separately timed Pinta collection/compaction on the small live heap. See [BENCHMARK.md](BENCHMARK.md). |

Initial compiler baseline logs and a JSON receipt are under
`artifacts/baseline/`. Native pristine and corrected profiles have distinct
directories under `artifacts/`; a corrected result does not replace the original
observation. Translation receipts retain compiler logs and input hashes under
`artifacts/translation/`. These generated evidence directories are ignored.

The remaining qualification gaps include extended allocator/value and bytecode
edge cases, three unqualified nonempty fixtures, and the native globals-properties
failure. Additional malformed-module/debugger investigation remains paused by an
automated safety check. Windows execution is deferred and does not block Linux.
The clean-checkout instructions are published; no independent fresh-checkout run
is claimed.
An authored test or a successfully emitted source file does not close these gaps.
The [blocker record](BLOCKERS.md) distinguishes the native discrepancy and paused
diagnostic investigation from successfully executed tests.

Cross-campaign commands and receipts are under `artifacts/regressions/`.
The retained CLI/compiler snapshot reports base revision
`c4325e4be08f1f0cd13ab3b2695c5848282a81c5`; its compiler-library SHA-256 is
`ef78ea4d8712fb583a7a38489368f2df21451082cae78e86b465ce7b9071db70`.
That binary hash remained unchanged across the regenerated campaigns. The
informational revision is a build label; source changes are represented by the
recorded source and binary hashes, and a later repository HEAD is not relabeled
as the compiler used for these tests.
