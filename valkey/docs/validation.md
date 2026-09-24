# Valkey execution ledger

Implementation began on 2026-09-24. Linux x64 is the only execution host used
so far. The complete translated library, owning API, sample and integration
consumer build. Managed server execution and NativeAOT product gates remain open.

## Input and pipeline infrastructure

`python3 -m unittest discover -s valkey/tests` passes 28 tests: 12 acquisition
checks, nine pipeline checks and seven managed-adaptation checks. These run the real campaign orchestration
against isolated test sources and an explicitly test-only dotnet executable.
They establish acquisition forwarding, source immutability, deterministic
generators, interpreter selection, failure preservation and publication rollback;
they do not establish complete C translation or C# compilation correctness.
The adapter checks also compile and execute the C symbol resolver against test
callbacks, verifying signatures and failure behavior, not Lua execution.

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
post-processing, final build and failure-safe promotion. It subsequently passed
the complete build pipeline in `attempt-k9tw85p0` (see the milestone below).
`--probe` only diagnoses object emission, returns nonzero for failed units and
never publishes partial output. See [blockers.md](blockers.md).

## Remaining execution gates

Full object emission reached 164/164 units in `attempt-pacp2mye`, followed by
166/166 in the earlier C-host profile (`attempt-gkmsc4bi`). The latter reached
link-time external callback checks after header resolution and module opaque
type fixes. Product host code has since moved to authored C# with declarations
and typed overrides; the resulting product now links and builds successfully.

The frozen regression snapshot in
`artifacts/verification/runtime-ownership-eqoa3ltj` completed with 2,655 unit
passes/four failures and 678 functional passes/eight failures/1,139 optional
oracle skips. All twelve failures were traced to Zig stderr typing, global
pointer increments, and assertions affected by prior ABI/diagnostic changes.
The repaired build passed 412 focused unit checks and 12 functional checks,
including calc/json-pretty execution, Zig stderr, global pointers and literal
pool behavior. See `artifacts/reductions/regression-repairs/`. This does not claim
a subsequent full-suite run.

Owner-scoped C random generators passed 32 focused new/existing unit checks.
Acquisition, immutable staging and pipeline orchestration passed 28 Python
checks, including explicit no-fetch reuse, namespace selection, authored host
links and failed-build rollback. Compiler/runtime reductions and native control
checks do not establish translated Valkey execution.

P1 ABI/host feasibility, P2 NativeAOT and P3–P9 server, command, scripting,
persistence, consumer execution and delivery gates remain open. Do not
interpret source audits or native checks as translated execution passes. The
root `ManagedConsumer.slnx`, API and sample build against the real generated
library; their runtime qualification remains open.


## C# host wiring and reduced adaptations

The C# host profile emits all 164 units in `attempt-9aq_edkg`. Its first link
invocation incorrectly passed the source-only override option; the driver now
applies overrides only at object emission, where typed bindings are persisted.
The 28 pipeline/acquisition checks pass with that correction. A diagnostic
166-object link of the preceding native-host snapshot succeeded and exposed
additional generated C# errors; it is not the product and was not published.

The product now needs four adapted files and 13 exact replacements. Native
compilation of the common dispatch hook passed, and a real unmodified `ae.c`
translation selected `select` (`artifacts/reductions/reduced-profile/receipt.json`).
C# host and consumer sources are authored and being checked against the actual
generated API. They are not yet a qualified running managed Valkey server.

## Complete C# library build and first startup

`artifacts/translation/attempt-k9tw85p0/result.json` records all 164 units emitted,
linked into `Managed.Database.ValkeyCore`, compiled raw, post-processed, compiled
again and promoted to `generated/TranslatedValkey`. The final-path build passed.
Authored C# host sources remain relative linked files under `src/Host`.
Raw, processed and final builds each reported zero errors and 116 warnings.
The root consumer solution subsequently built with zero warnings/errors.

Literal pooling was enabled for that run. The pipeline now also passes
`--deduplicate-inline` at link time; all nine pipeline tests pass with both
options required. Full regeneration with the new option remains pending.

The first real sample execution (`artifacts/managed-smoke/rdb-1f1k99e9`) failed
while constructing the generated owner: its globals structure exceeds the CLR
array-element size limit. Pinned byte backing with typed references is being
qualified in the shared compiler before retrying startup. No readiness, command
or persistence pass is claimed from this attempt.

The real integration harness in `tests/ManagedHostTests` builds with zero
warnings/errors. `scripts/validate-managed.sh` executes it and can add the pinned
native RESP comparison, external-server Tcl protocol suite and a whole-assembly
NativeAOT gate. Its runtime checks are pending successful startup.

The newer frozen compiler/runtime regression snapshot
`artifacts/verification/runtime-expanded-haejvtoa` passed 2,706 unit checks.
Its functional run passed 691 cases, failed nine and skipped 1,151 optional
oracles. The failures reduced to two name collisions (the BCL `Path` helper and
a volatile runtime helper); both have been repaired and passed focused managed
and native checks. This is not a claim that the full functional suite was rerun.
