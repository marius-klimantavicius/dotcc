This bounded T4 workload passes native, generated JIT and NativeAOT execution,
including both closed-file interchange directions. The exact commands, frozen
sources, 15 measured phases and 13 observation reports are recorded in
[stateful-report.json](../../acceptance/sqlite/stateful-report.json). It uses a subset of
the existing 23 scalar/pointer APIs; it adds no SQLite exports or callbacks.

[workload.json](workload.json) defines three finite modes. `stateful` executes
40 actions against a new `stateful.db`: commit and close/reopen; establish a
reader snapshot before another connection commits; check old and new
visibility; hold a write transaction while the other connection gets BUSY;
roll back an update and an insert; checkpoint; and reopen to verify typed rows
and integrity. The failed `BEGIN IMMEDIATE` must return step code5 and finalize
code5, and must be finalized before the next action. Successful statements end
with step101 and finalize0.

The actions themselves are the connection barriers. These are two live
connections in one process, with `busy_timeout=0` and no sleeps, retries,
callbacks, thread scheduling or elapsed-time assertions. This does not prove
simultaneous cross-process lock behavior. All transactions/statements end
before the successful TRUNCATE checkpoint. The planned exact checkpoint tuple
is busy/log/checkpointed `0/0/0`; the runner records actual observations before
checking expectations, so any native disagreement is retained in an
incomplete report.

The first native attempt completed all 40 actions with the expected values,
BUSY step/finalize5, checkpoint0/0/0 and close0, but correctly failed the final
sidecar transfer guard. Its last verification connection was read-only and left
a zero-byte WAL plus a 32768-byte SHM file. That attempt remains unchanged at
`/mnt/storage/roslynobj-csharp-20261015/t4/native-stateful-1/stateful-report.json`,
SHA-256 `142b73627377401d5d263ac5dabd3db4e869d251b811155d39ad304c62e73ce2`.
The fixture now uses readwrite-without-create for all final verification opens,
while their SQL remains read-only, so SQLite owns normal last-close cleanup.
The runner still requires absent sidecars and never deletes them or ignores a
nonempty WAL. This is a fixture connection-lifetime adjustment, not a product
change or a successful original attempt.

`interchange-append` executes 13 actions: verify the transferred base rows,
commit a distinct row4, checkpoint, close, reopen and verify. The 5-action
`interchange-verify` checks the returned rows and integrity. The coordinator
must run both native-create → generated-append → native-verify and
generated-create → native-append → generated-verify, waiting for each producing
process to exit before starting its consumer. Hash and closed-state fields
alone are not a universal proof that no process has the file open.

Each invocation creates a fresh artifact directory. Interchange accepts
`--input-report` from the prior completed invocation, validates its exact
database file hash/length, expected state, workload hash, closed connection
flag and absent WAL/shared-memory/journal sidecars, then copies that file into
the receiving directory. It leaves the incoming file unchanged. File hashes
identify transferred artifacts; acceptance compares typed SQL values rather
than arbitrary database byte equality.

The native runner pins these identities before loading any SQLite code:

| Input | SHA-256 |
| --- | --- |
| `acceptance/sqlite/native-oracle-build.json` | `c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435` |
| Exact native oracle library | `39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a` |
| Unchanged source object | `e26d8937c94f5ccc9e3fca7915f53c4a31a1bb63432419a847d99394ab06bb1c` |
| Workload | `8f9cdc8262ac0c8b29f73fd6a375fd693abf0311dfc4335493b8e5c02ef75a8a` |

It loads only the receipt's absolute native oracle path using ctypes, rejects
loader overrides, and records the loaded library identities. It does not
import Python sqlite3 or search for system SQLite. Run it as a separate
native-only process; never load that application library into generated
execution.

Coordinator command, from the repository root, with an unused artifact path:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteStateful/run-native-stateful.py \
  --mode stateful --artifacts /mnt/storage/roslynobj-csharp-20261015/t4/native-stateful
```

For a native interchange receiving step, pass `--mode interchange-append` or
`--mode interchange-verify` and `--input-report /absolute/prior/stateful-report.json`.
Use the existing measured runner around each command to record process limits.
The generated `Checks.csproj` uses the actual published Generated project and
existing validated production API aliases. Build and run commands with exact
property/argument values are retained in the compact receipt. Each generated
invocation receives the same-mode native report for exact typed comparison.

The generated counterpart uses the same ordered actions and typed value
protocol: integers as decimal strings, reals as big-endian binary64 hex, text
as exact UTF-8 bytes/hex, blobs as exact bytes/hex, and explicit null values.
Every observation has `id`, `action`, `connection`; open/close adds `code`, SQL
adds `sql`, `columns`, `rows`, `prepareCode`, `stepCode`, `finalizeCode`.
Preparation must succeed and consume exactly one statement. Observations are
recorded before checking them, including expected errors.

Transfer report keys are `status` (`passed-native-stateful` or
`passed-generated-stateful`), `workload.sha256`, `databaseState` (`base` or
`interchanged`), `connectionsClosed`, and `database` with absolute `path`,
`sha256`, `bytes`. `observationsSha256` hashes the observations array as UTF-8
JSON with sorted keys and compact separators. Provider/file identities and
artifact paths are audit data, not cross-engine SQL values.

The workload's `remainingT4Areas` lists every unclosed feature area. See
[T4-STATEFUL.md](../../docs/T4-STATEFUL.md). No T4 gate closes from preparation,
a native-only pass, or one interchange direction.
