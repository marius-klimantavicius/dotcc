The [SqliteProviderContracts fixture](../tests/SqliteProviderContracts/README.md)
adds exactly three finite contracts: SQL wall-clock bounds, guarded randomness
output, and allocation growth with preserved bytes. The independent pinned
native oracle passed. Generated JIT/NativeAOT comparison on the accepted new
readable CLI publication remains a separate execution gate at preparation.
No native SQLite application body serves generated calls.

Frozen native evidence:

- Workload SHA-256:
  `33b6ea1f6d7dd02833c339f3d0e4a6612a75449b1c2be1b6e9b710e216f80502`.
- `/mnt/storage/roslynobj-csharp-20261015/t4/native-provider-contracts-1/provider-contracts-report.json`,
  SHA-256 `97f678f2eb0b91fc580767362aa0e0d8150fd7b3734dacc1cad834e0bc5995b1`.
- `/mnt/storage/roslynobj-csharp-20261015/t4/native-provider-contracts-run-1/run.json`,
  SHA-256 `8c9d3d8400086770f9e7353d08d32c7bbcf964c91abd542aa5911e29944c4e34`.

Time is measured by preparing first, sampling host realtime immediately around
the single ROW step, then reading the two results. The pinned source around
sqlite3.c47827 constructs Julian milliseconds from gettimeofday, truncating
microseconds. sqlite3StmtCurrentTime around94389 caches the first time for a
statement; juliandayFunc around26975 divides by86,400,000 and unixepochFunc
around26993 drops fractional seconds. The two-millisecond tolerance explicitly
covers millisecond truncation and the binary64 Julian-to-epoch round trip at
this epoch. Both endpoint orders are supported through minimum/maximum bounds;
an arbitrary intermediate clock adjustment is not proven safe by that rule.
Actual endpoint nanoseconds, epoch decimal and Julian binary64 bits are audit
data. Only type, bounded-window and same-statement consistency booleans compare
between engines. No sleeps or scheduling assertion is used.

Randomness writes64 requested bytes inside a96-byte native allocation with
16-byte guards. The pinned header around3369 specifies that randomness stores
N bytes in the caller's buffer. The test checks guard preservation, extent and
call completion, and records all64 bytes independently. It neither rejects a
particular possible vector nor infers statistical quality, entropy, security
or a particular underlying native-provider call from the observed output.

Allocation uses the selected malloc64/realloc64/msize/free APIs described in
the pinned header around3274–3339. It checks at least the requested usable
extent before reading/writing, known initial32-byte contents, preserved prefix
after growth to96, and known new64-byte tail contents. The successful native
usable sizes were32/96; those sizes, addresses and whether realloc moved are
audits, not required cross-engine equality. Successful growth invalidates the
old pointer, while refused positive growth leaves the original owner for
cleanup. Ownership is cleared before exactly one free. Zero-size behavior and
the separate hard-heap refusal scenario are not added to this three-case slice.

The new validator uses the accepted public inventory plus actual generated
declarations for six APIs, and replays the existing production23 validator
against this publication's completion and API receipts. Their retained
signatures are initialize()->int, randomness(int,void*)->void,
malloc64(uint64)->void*, realloc64(void*,uint64)->void*, msize(void*)->uint64,
and free(void*)->void. Each has ordinary prototype parameters, no variadic or
hidden return roles, and the captured default external C calling convention.
Natural integer32 is4/4, unsigned64 and ordinary pointers are8/8.

Explicit direct API invocation counts record calls actually made by each
harness. They do not enumerate internal calls or equate88 selected native
exports with88 observed invocations. Existing allocation refusal, injected
pager IOERR, logical errno/TLS, loader, callbacks and threading receipts supply
their separate bounded contracts. The remaining provider/API/concurrency
variants in [the T4 ledger](T4-PROGRESS.md) stay explicit.


The frozen native/JIT/NativeAOT gate now passes all five measured phases against
the final readable source-13 publication. [Compact receipt](../acceptance/sqlite/provider-contracts-report.json)
pins source, workload, API, compiler, native oracle and loaded-library identities.
Both generated modes match all three finite contract objects and 16 invariants. Wall-clock endpoints/Julian rounding, requested random-buffer extent/guards and allocator grow/preserve/msize/free pass. Timestamps, random bytes and addresses remain separate audits; direct SQLite API counts do not imply every native import ran.
Generated dependencies contain only Generated, Runtime and the test harness;
process maps exclude native SQLite. NativeAOT explicitly pins clang 18. This is
finite configured Linux/x64 evidence, not an exhaustive failure, tokenizer or
provider-variant claim.
