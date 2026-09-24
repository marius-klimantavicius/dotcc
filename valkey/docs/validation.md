# Valkey execution ledger

Implementation began on 2026-09-24. Linux x64 is the only execution host used
so far. No translated Valkey server, managed consumer, JIT product or NativeAOT
product has passed a runtime gate yet.

## Input and pipeline infrastructure

`python3 -m unittest discover -s valkey/tests` passes 20 tests: 12 acquisition
checks and eight pipeline checks. These run the real campaign orchestration
against isolated test sources and an explicitly test-only dotnet executable.
They establish acquisition forwarding, source immutability, deterministic
generators, interpreter selection, failure preservation and publication rollback;
they do not establish C translation or C# compilation correctness.

The real `scripts/fetch.sh` default and `--no-fetch` paths both verified the
pinned archive and all 1,922 reference files. The checked-in trusted manifest
supports archive-free reuse, and tests block network calls in no-fetch mode.
See [inputs.md](inputs.md). Shell syntax and Python compilation checks pass.

## Native control

`./valkey/scripts/oracle.sh --no-fetch` built Valkey 9.1.2 with GCC 13.3.0,
GNU Make 4.3 and Python 3.12.3. The receipt is
`artifacts/native/receipt.json`; source closure and configuration are recorded in
[configuration.md](configuration.md). The server identifies the pinned commit,
not the dotcc checkout's Git state. Reference files remain unchanged.

- 20 baseline checks passed: binary/pipelined/fragmented RESP, representative data
  structures, Lua extensions, transactions, RESP3, foreground RDB save/reload,
  startup AOF append/replay and native CLI connection.
- The pinned `unit/protocol` Tcl suite passed 35 assertions with zero failures.
- The actual server link selected 164 C units and 141 transitive project
  dependencies. This is native evidence, not proof that managed host services work.

## Compiler baseline and initial full-source attempt

`dotnet build dotcc.sln -c Release --nologo` passed with zero errors and 31
warnings. `dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release --no-build
--nologo` passed 2,441 tests, zero failures/skips, in 4m12s. Logs are under
`artifacts/baseline/`. The full functional suite after the first header repair
passed 620 tests with 1,083 optional oracle checks skipped in 3m41s. Skipped
oracles are not execution passes.

`./valkey/scripts/translate.sh --no-fetch --no-build-tools --probe` attempted all
164 units using a frozen compiler snapshot. In
`artifacts/translation/attempt-0o6qsy8t/result.json`, 58 emitted objects and 106
failed. Each unit has source/tool/configuration hashes, its command and a log.
The prior `attempt-re83cscr` records a pipeline option error (`--runtime=c` at
object emission instead of link time); it is not a source-compatibility result.
The corrected driver retains that first attempt instead of overwriting evidence.

Independent object translations now use four workers by default (`--jobs 1..16`)
while receipts retain manifest order. All failures are collected before linking
is considered; a failed or partial run cannot publish a product. The same 20
pipeline/acquisition checks pass after this change.

The full default pipeline is implemented through linking, raw build,
post-processing, final build and failure-safe promotion, but currently stops on
real translation errors. It has not published a successful product.
`--probe` only diagnoses object emission, returns nonzero for failed units and
never publishes partial output. See [blockers.md](blockers.md).

## Remaining execution gates

P1 ABI/host feasibility, P2 complete library emission/build and P3–P9 server,
command, scripting, persistence, consumer and delivery gates remain open. Do not
interpret source audits or native checks as translated execution passes. The
root `ManagedConsumer.slnx` will be created with real projects, not empty targets.
