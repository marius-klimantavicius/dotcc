This finite T4 workload checks actual hard-heap allocation refusal and the
pinned SQLite pager's documented commit-fault hook, followed by recovery. The
native oracle passed both cases. Generated JIT/NativeAOT execution is a separate
gate against the forthcoming readable production CLI publication; these test
sources do not claim that result.

[workload.json](workload.json) has SHA-256
`f1cfaffcc508d27de1c7aedca10744b15a04300d06a25911e70666533a83d6bf`.
[run-native.py](run-native.py) loads only the absolute library from the pinned
[native build receipt](../../acceptance/sqlite/native-oracle-build.json):
receipt SHA-256
`c5d7e700950971126a1f6aa735fc205850d9563bca482b815715ed1353feb435`,
library SHA-256
`39d00f19c6778ab90a51bdfdb8e6f13db6d733482112e8587c0e30eccb284c6a`.
The independent process imports no Python sqlite3 module and has no system
SQLite fallback. The generated counterpart calls captured `Api` methods
statically and rejects any loaded native SQLite application library.

The successful native observations are
`/mnt/storage/roslynobj-csharp-20261015/t4/native-failures-1/failures-report.json`
(SHA-256
`9389da20b10cb2670577adcbb8054f63627a2640a9c8b50097c68b4155770905`).
The measured run is
`/mnt/storage/roslynobj-csharp-20261015/t4/native-failures-run-1/run.json`
(SHA-256
`4014c7f76608d779302a4900bf9f043178372d21aa393ec9ff3433ebab6050a6`).
It exited zero in 0.201 seconds; sampled process-tree RSS was 21,327,872 bytes.
The 100 ms sampling interval can miss short peaks.

The allocation case prepares the one-MiB randomblob statement before setting
the hard limit to live SQLite memory plus 65,536 bytes. It requires an actual
step result of NOMEM7, copies primary/extended error7 and `out of memory`
immediately, restores the previous limit in `finally`, then finalizes once
with result7. Native autocommit remains0. An explicit ROLLBACK restores row
`(1,10)`, and a subsequent valid insert produces `(1,10),(2,20)`. Memory used,
requested/observed cap and limit restoration are recorded separately from the
cross-engine case comparison; allocator accounting need not be equal.

The I/O case uses a fresh DELETE-journal file. An unarmed FAULT_INSTALL9 call
invokes callback0 once and returns0. Only the first armed callback400 returns1,
activating the retained `sqlite3PagerCommitPhaseOne` IOERR path. COMMIT step and
finalize each return10, with copied primary/extended10 and `disk I/O error`.
Native autocommit is then1; there is no explicit rollback. Row `(1,30)` remains,
a valid transaction writes31, and close/reopen plus integrity_check confirm
persistence. This is injected pager-commit IOERR evidence. Actual OS/device
failures, other VFS sites and broader failure schedules remain separate.

The Cdecl `int(int)` callback catches errors and never throws through native
code. Arming covers only the intended COMMIT step. It is disarmed/uninstalled
unconditionally before finalization; statement ownership is cleared before
each finalizer. Every connection closes, the generated/native reports retain
distinct close results, and normal SQLite close must leave no journal/WAL/SHM
sidecars. The harness never deletes sidecars to satisfy that check.

[SurfaceValidator.py](SurfaceValidator.py) replays the unchanged production23
validator from its recorded frozen path. It then validates seven selected
failure API signatures, actual generated declarations, the captured int(int)
fault callback, builtin va_list types, and the existing runtime pack layout.
The generated `sqlite3_test_control` projection accepts one fixed int plus a
`SysVVaList*`. The stack pack uses a16-byte-aligned register-save area, fixed
GP offset8, FP offset48, and the callback pointer in GP slot8. Captured tag and
runtime physical coordinates must both be size24/alignment8 with fields at
0/4/8/16. No ABI constants are inferred from C# pointer spelling.

Coordinator preparation, after the new readable CLI publication and its base
production23 API validation exist, choosing fresh paths:

```sh
python3 edgcpp/roslyn-csharp/tests/SqliteFailures/SurfaceValidator.py \
  --generated-project /absolute/readable-cli/Generated.csproj \
  --candidate-receipt /absolute/readable-cli/completion.json \
  --base-report /absolute/readable-production/api-surface.json \
  --aliases /absolute/new/CapturedAliases.cs --report /absolute/new/api-surface.json
```

Build [Checks.csproj](Checks.csproj) with those `GeneratedProject`,
`CandidateReceipt`, `SqliteApiAliases` and `SqliteApiReceipt` properties.
`SqliteSurfaceValidator` may select the separately frozen validator path.
The prebuild target repeats its verification. Publish the same harness for
linux-x64 NativeAOT using the accepted clang-18 toolchain. The coordinator
schedules all builds and executions.

Run `dotnet Checks.dll WORKLOAD_JSON NEW_ARTIFACT_DIRECTORY SEPARATE_NATIVE_REPORT`,
or the NativeAOT `Checks` with the same arguments, in a fresh process. Each
generated report compares the exact native `cases` array, including separate
step/finalize results, immediate errors, observed autocommit and rollback
actions. SQL values use decimal integers and exact UTF-8 bytes. File bytes,
paths and memory accounting are audits, not cross-engine equality conditions.
The result records loaded providers, file hashes, limit/callback cleanup and
closed connections. See [T4-FAILURES.md](../../docs/T4-FAILURES.md) for scope.


Coordinator verification now passes native, JIT and NativeAOT: all five measured
phases, exact semantic comparison, immutable sources/pins and generated runtime
dependency separation. See [the compact receipt](../../acceptance/sqlite/failures-report.json).
The final publication is source 13; broad unexercised variants remain explicit
in the receipt and current T4 ledger.
