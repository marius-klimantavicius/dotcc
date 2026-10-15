This fixture supplies three finite time, randomness and allocation contracts
for the unchanged full-feature SQLite object. The separate pinned native
oracle passed all three cases. Generated JIT/NativeAOT execution is a separate
gate on the new readable CLI publication. It compares contract observations,
while nondeterministic timestamps, random bytes, addresses and usable sizes
remain independent audits.

[workload.json](workload.json) SHA-256 is
`33b6ea1f6d7dd02833c339f3d0e4a6612a75449b1c2be1b6e9b710e216f80502`.
The native report is
`/mnt/storage/roslynobj-csharp-20261015/t4/native-provider-contracts-1/provider-contracts-report.json`,
SHA-256 `97f678f2eb0b91fc580767362aa0e0d8150fd7b3734dacc1cad834e0bc5995b1`.
The measured run is
`/mnt/storage/roslynobj-csharp-20261015/t4/native-provider-contracts-run-1/run.json`,
SHA-256 `8c9d3d8400086770f9e7353d08d32c7bbcf964c91abd542aa5911e29944c4e34`.
It exited zero in 0.101 seconds; waited process RSS was20,709,376 bytes.
The short-run 100 ms process-tree sample misses startup peaks and must not
be presented as the process's maximum memory.

The three cases are:

1. Prepare `SELECT unixepoch('now'),julianday('now')`, then sample host realtime
   immediately before and after its single ROW step. Require integer/finite
   real columns, both values inside the endpoint window with the documented
   two-millisecond rounding tolerance, and consistency between the statement's
   two time values. Endpoint minimum/maximum handles either observed order;
   no wall-clock monotonicity is assumed. This finite observation does not
   establish behavior under arbitrary clock adjustments between the samples.
2. Call `sqlite3_randomness(64,interior)` on a native buffer with16-byte guards
   before and after the requested extent. Require unchanged guards, completed
   call and the observed64-byte extent. Record exact bytes and whether they
   differ from the initialized interior only as audit data. No random vector
   is forbidden, and no distribution, entropy or security property is claimed.
3. Explicitly initialize once, call malloc64(32), write/read32 known bytes,
   grow with realloc64(96), require the original32 bytes preserved and both
   msize results at least their request, then write/read the64-byte new tail.
   Clear ownership before the one free call. Failed positive growth retains
   the original owner for cleanup. No old pointer is read after successful
   realloc, and msize is checked before accessing each requested extent.

The pinned source's unixCurrentTimeInt64 uses gettimeofday and truncates to
milliseconds; unixepoch drops fractional seconds, while julianday divides
the statement's millisecond Julian count by86,400,000. The chosen0.002-second
tolerance covers that millisecond truncation and the binary64 Julian/epoch
conversion at the observed epoch. It is not an allowance for arbitrary time
skew. Source locations and scope are in
[T4-PROVIDER-CONTRACTS.md](../../docs/T4-PROVIDER-CONTRACTS.md).

[run-native.py](run-native.py) loads only the absolute library selected by
the pinned [build receipt](../../acceptance/sqlite/native-oracle-build.json),
with receipt SHA-256
`c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435`
and library SHA-256
`39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a`.
It has no Python sqlite3 import or system fallback. [Program.cs](Program.cs)
instead calls the captured generated APIs statically, rejects a loaded native
SQLite application library, and uses explicit JSON nodes for NativeAOT.
It requires the successful pinned native report and compares only the exact
three `cases` objects. Both harnesses record actual direct API invocation
counts. Those counts do not instrument internal calls or prove invocation
of every selected native provider export.

[SurfaceValidator.py](SurfaceValidator.py) replays the unchanged production23
validator from its recorded frozen path, then validates the six additional
selected definitions, ordinary roles, captured scalar/pointer dimensions and
actual generated declarations: initialize, randomness, malloc64, realloc64,
msize and free. int is4/4, uint64 is8/8 and void* is8/8. Signature validation
and the selection of88 native providers are distinct from runtime invocation.

Coordinator preparation against the accepted readable publication, with fresh
alias/report paths:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteProviderContracts/SurfaceValidator.py \
  --generated-project /mnt/storage/roslynobj-csharp-20261015/t4/sqlite-readable-generated-1/Generated.csproj \
  --candidate-receipt /mnt/storage/roslynobj-csharp-20261015/t4/sqlite-readable-generated-1/completion.json \
  --base-report /mnt/storage/roslynobj-csharp-20261015/t4/sqlite-readable-production-run-1/api-surface.json \
  --aliases /absolute/new/CapturedAliases.cs --report /absolute/new/api-surface.json
```

Build [Checks.csproj](Checks.csproj) with those `GeneratedProject`,
`CandidateReceipt`, `SqliteApiAliases` and `SqliteApiReceipt` properties.
`SqliteSurfaceValidator` may select the separately frozen validator path;
the prebuild target repeats its `--verify-only` check. Publish the same
project for linux-x64 NativeAOT with the accepted clang-18 toolchain.
The coordinator schedules all builds and executions.

Run `dotnet Checks.dll WORKLOAD_JSON NEW_ARTIFACT_DIRECTORY SEPARATE_NATIVE_REPORT`,
or the AOT executable with the same arguments, in a fresh process. Each result
includes provider identities, closure/release checks and input/report hashes.
The output is `provider-contracts-report.json`; success requires all16 finite
contract booleans and exact comparable step/finalize/close results. Zero-size
allocations, every allocator refusal schedule and randomness quality are
outside this fixture. Existing heap-failure, pager-fault, errno, loader and
thread proofs remain separate.


Coordinator verification now passes native, JIT and NativeAOT: all five measured
phases, exact semantic comparison, immutable sources/pins and generated runtime
dependency separation. See [the compact receipt](../../acceptance/sqlite/provider-contracts-report.json).
The final publication is source 13; broad unexercised variants remain explicit
in the receipt and current T4 ledger.
