The bounded T4 restart preparation lives in
[SqliteRestart](../tests/SqliteRestart/README.md). Execution evidence remains
pending. It addresses the acceptance requirement for repeat generation from
identical immutable input and cancellation/restart at a real phase boundary.

The existing token polling and CLI Ctrl-C handler provide cancellation but no
external deterministic phase signal. The new public synchronous observer
reports completed inventory, completed emission and the real pre-publication
boundary. It preserves the prior `Translate` overload. The full SQLite test
cancels only at `BeforePublication`, after observing the actual staged
completion and before publication. The existing cleanup must remove that
completion while preserving incomplete files. Earlier phases are exercised
only with a separate genuine small fixture when requested.

Restart is a new process calling the normal pipeline with the exact same
request/output path. It recomputes from the unchanged object and providers,
leaves prior incomplete staging untouched, and publishes normally. No
incremental resume API or safe partial-stage reuse exists, and this procedure
does not invent one. The cancellation/result report must say incomplete even
when preserved source was already completely emitted.

Four emitted files must remain byte-identical to the actual CLI baseline:
`Generated.cs`, `Generated.csproj`, `Generated.Abi.c` and
`Generated.NativeImports.json`. The snapshot-11 runtime path, complete pinned
input, explicit provider contract, all definitions, target and budgets stay
fixed. Program reports differ in their output destination; that field alone
may be removed for exact report comparison. Completion report hashes and
resource metrics are separate recorded data, never normalized into a source
determinism claim.

Dedicated output parents isolate real staging observations from unrelated
artifacts. There are no sleeps, scheduling guesses, object mutations, raw
database reads or alternative semantic consumers. Every completed file hash
is verified; input/provider and source stability are checked at the end. The
full measured process supplies cancellation timing/peak memory because the
current cancelled result inherits inventory metrics.

This is one cancellation plus one clean restart on the full input, with
optional early-phase checks on a small input. It establishes no application
workload, enabled-feature, whole-provider or incremental-recovery acceptance
without its own corresponding evidence.


The frozen full-object campaign now passes all eight measured phases at
`t4/restart-reproduction-1/restart-receipt.json`, SHA256
`640fa5ddeae10f431150c156431c56e35da3958cb60ee03927ce7cc244429366`.
[Committed receipt](../acceptance/sqlite/restart-report.json). Both small genuine
phase checks pass. Full BeforePublication cancellation passes in 423.77 seconds
with sampled process-tree peak 2.447 GB; its real staged completion is observed
then removed, incomplete files retained and requested output unpublished. The
fresh process recomputes the identical request in 430.44 seconds, sampled peak
2.443 GB, and publishes atomically. All four emitted sources compare byte for
byte with the actual CLI baseline. Reports compare after removing only
output.directory; completion/metrics/path differences remain independently
recorded. The previous publication, immutable inputs and preserved incomplete
stage remain unchanged. Source and input freeze checks pass. Root independently
reviewed this finite evidence. This is restart from input, not reuse of stages.
