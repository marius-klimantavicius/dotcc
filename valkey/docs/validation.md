# Valkey execution ledger

Implementation began on 2026-09-24. Linux x64 is the only execution host used
so far. The complete translated library, owning API, sample and integration
consumer build. The initial managed profile passes raw/processed JIT/NativeAOT
execution; broader command/scripting coverage and other platforms remain open.

## Input and pipeline infrastructure

`python3 -m unittest discover -s valkey/tests` passes 30 tests: 12 acquisition
checks, nine pipeline checks, seven managed-adaptation checks and two notice
checks. These run the real campaign orchestration
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

Broader P4/P5 command and scripting coverage, recovery/performance campaigns,
other-platform execution and clean-checkout reproduction remain open. Do not
interpret source audits or native checks as translated execution passes. The
root `ManagedConsumer.slnx`, API and sample build against the real generated
library; the initial runtime matrix now passes as recorded below.


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
generated API. The later final-product matrix below qualifies the initial
running profile.

## Complete C# library build and first startup

`artifacts/translation/attempt-k9tw85p0/result.json` records all 164 units emitted,
linked into `Managed.Database.ValkeyCore`, compiled raw, post-processed, compiled
again and promoted to `generated/TranslatedValkey`. The final-path build passed.
Authored C# host sources remain relative linked files under `src/Host`.
Raw, processed and final builds each reported zero errors and 116 warnings.
The root consumer solution subsequently built with zero warnings/errors.

Literal pooling was enabled for that run. The pipeline now also passes
`--deduplicate-inline` at link time; all nine pipeline tests pass with both
options required. Full regeneration with both options passed in
`attempt-rj1nminy`: 164 units, 85 generated C# files, raw and processed builds,
post-processing and final-path promotion (88 warnings, zero errors).

The first real sample execution (`artifacts/managed-smoke/rdb-1f1k99e9`) failed
while constructing the generated owner: its globals structure exceeds the CLR
array-element size limit. Pinned byte backing with typed references fixed this,
with source/object, owner/TLS isolation, GC, native and NativeAOT regressions.
No readiness, command
or persistence pass is claimed from this attempt.

The real integration harness in `tests/ManagedHostTests` builds with zero
warnings/errors. `scripts/validate-managed.sh` executes it and can add the pinned
native RESP comparison, external-server Tcl protocol suite and a whole-assembly
NativeAOT gate. Runtime results are recorded below.

The newer frozen compiler/runtime regression snapshot
`artifacts/verification/runtime-expanded-haejvtoa` passed 2,706 unit checks.
Its functional run passed 691 cases, failed nine and skipped 1,151 optional
oracles. The failures reduced to two name collisions (the BCL `Path` helper and
a volatile runtime helper); both have been repaired and passed focused managed
and native checks. This is not a claim that the full functional suite was rerun.

## Running JIT and NativeAOT server evidence

`artifacts/reductions/startup-3077111h` relinks the complete real 164-object closure
with literal pooling, inline deduplication and the repaired storage runtime.
It references the actual C# host/API/sample sources. Authenticated RDB and AOF
samples pass, including two owners, binary values, lists/hashes, transactions,
Lua, save/reload and joined cleanup. The preceding diagnostic correctly exposed
a failed SAVE: directory handles now report `EISDIR`, a platform limitation
explicitly accepted by unmodified upstream `fsyncFileDir`. Directory durability
is not claimed (see [persistence.md](persistence.md)).

The integration harness passed ten cases in both JIT and whole-assembly-rooted
NativeAOT, covering owner/authentication isolation, RESP2/RESP3, fragmented large
binary requests, collections/transactions/TTL, Lua cache/functions, capability
guards, failed SAVE recovery, RDB reload, AOF replay and repeated cleanup/rebind.
`integration-retry.json` and `aot-integration.json` identify the actual execution
mode and each case. `exchange-result.json` records all eight additional
native↔managed RDB/AOF exchange gates across JIT/AOT, with native integrity
checks and data/type/absolute-expiry/function assertions.

Pinned Tcl `unit/protocol` executed against that managed endpoint and passed
29 checks, zero failures (`protocol-final/receipt.json`). Six DEBUG PROTOCOL
tests are explicitly excluded because DEBUG is outside the admitted profile.
The ordinary HELLO availability-zone test passes; its setter is per-owner and
is admitted by the C# configuration policy. The external test launcher cannot
start a substitute native server.

The native/actual-assembly ABI probe passed 133 checks across 21 types, including
Valkey object bitfield bytes, packed SDS, dict, client/server and Lua layouts.
131 match exactly. Two documented internal event-loop differences derive from
the managed pthread mutex handle; see [ABI evidence](../tests/abi/README.md).

The subsequent frozen full regression run `runtime-storage-r2filo4c` passed all
709 functional cases, with 1,157 optional oracle cases skipped. 2,734 unit tests
passed and one failed because its negative `var z` substring assertion also
matched the unrelated runtime local `var zone`. The assertion now targets the
complete declaration and its focused rerun passes.

These diagnostic runtime passes precede the final generated-product matrix.

## Final-path delivery matrix

`./valkey/scripts/verify.sh --no-build-tools` passed with default fetching enabled.
It ran 30 acquisition/pipeline/notice checks, rebuilt the native control, emitted
all 164 units, linked with both pooling/deduplication options, post-processed,
built/promoted final output, built the real root solution and executed the matrix.
The translation receipt is `artifacts/translation/attempt-lshtv_ga/result.json`.

| Product | JIT integration | Whole-assembly NativeAOT integration | Pinned Tcl protocol | Native↔managed RDB/AOF exchange |
| --- | --- | --- | --- | --- |
| Raw (`run-qel76rfs`) | 16 passed | 16 passed | 29 passed, 6 explicit DEBUG exclusions | 4 directions/formats in each execution mode passed |
| Processed (`run-52d_lfqb`) | 16 passed | 16 passed | 29 passed, 6 explicit DEBUG exclusions | 4 directions/formats in each execution mode passed |

These run directories are under `artifacts/managed-validation/`. They include
commands/logs, assembly hashes, per-case results, actual execution modes,
persistence file hashes and native integrity checks. The native AOF checker
misclassifies this pin's `VALKEY`-signature RDB base files; the validator instead
checks strict manifest structure, runs native check-rdb on bases (requiring a
checksum pass), checks incremental AOF segments, and requires real replay.

The integration cases additionally qualify background lazyfree work using its
completed counter, pending-job drain and shutdown joins. EverySecond AOF uses
owner-bound persistence snapshots to require the published fsynced offset to
catch up with acknowledged writes, then reloads 256 values. Command-family
assertions run against both managed and pinned native servers, covering numeric
and bit operations, collection transitions, stream groups, HLL, geo, SCAN,
WATCH conflicts and ACL command/key denials. This remains representative
coverage, not an exhaustive pass of all 307 required inventory entries.

The actual sample project passed authenticated RDB and AOF restart under JIT
and NativeAOT (`artifacts/managed-smoke/delivery-o8suqatu/result.json`). Its
isolated build outputs prevent interference with the raw/processed test builds.
`UPSTREAM-NOTICES.txt` accompanies generated, build and publish outputs and
preserves 259 source/license notice sections with original source hashes.

The final assembly passes the 133-case ABI probe with only the two documented
pthread-handle differences (`artifacts/abi/final-product/receipt.json`). A second
postprocessor pass leaves all 85 generated C# hashes unchanged and does not
touch authored originals (`artifacts/postprocess-idempotence/check-kzayl3t2`).
Default-fetch and explicit-no-fetch generation both pass; temporary staging paths
enter internal unit-name hashes, so byte-identical C# across staging roots is
not claimed. The outputs have equivalent tested behavior.

Measured raw/processed NativeAOT publications use standard system/BCL libraries;
no separate native Valkey/Lua backend was linked or observed during live PING/Lua
execution. Import, loaded-image, child-process and source evidence is recorded in
`artifacts/native-dependencies/run-lxgb9a22/summary.json`, with the limits of each
check stated. This is not an exhaustive proof about every future execution path.

Freshly translating the existing SQLite core and VFS corpora with the current
compiler passes both exact-output comparisons
(`artifacts/regressions/sqlite/receipt.json`). Other campaigns' entire delivery
matrices were not rerun. No Windows, power-loss, exhaustive upstream scripting,
full-command inventory or performance-parity claim is made.

## Occupied-port lifecycle regression

A final failed-startup case exposed a shared socket mismatch after the initial
matrix above: .NET's Linux `ReuseAddress` mapping enabled both native reuse
options, allowing two owners to listen on the same port. Independent native C
and BCL probes establish the difference in
`artifacts/reductions/socket-reuse-abi/receipt.json`. The shared runtime now uses
BCL raw socket options on Linux to keep `SO_REUSEADDR` and `SO_REUSEPORT`
independent. Port sharing requires the explicit latter option; unsupported
non-Linux reuseport returns an error.

The repair passes 44 affected socket/ownership/termination unit checks, six
focused reuse-option checks, four functional socket fixtures and four native
GCC comparisons. The new fixture is explicitly Linux-only. A fresh 164-object
diagnostic relink passes all 17 integration cases, including occupied-port
failure, queued-snapshot failure, cleanup without quarantine and surviving peer
owners (`artifacts/reductions/failed-startup-mata4cmu/managed.json`). The final
generation/matrix is being repeated with this runtime repair.
