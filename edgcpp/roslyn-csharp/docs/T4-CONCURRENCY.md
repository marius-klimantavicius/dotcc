The bounded concurrency gate passes native, generated JIT and NativeAOT against
the final readable production publication. All five measured phases and all
29 semantic observations pass; see [harness](../tests/SqliteConcurrency/README.md)
and [receipt](../acceptance/sqlite/concurrency-report.json).

Two real processes use command acknowledgments as explicit barriers. A real
writer lock causes BUSY 5 with callback argument 0; a reader keeps its old WAL
snapshot across the writer commit. Passive checkpoint observes 0/4/3 while the
reader is pinned, then TRUNCATE returns 0/0/0 after release; final rows survive
close/reopen. These exact counters are observations for this workload/provider,
not a general scheduling promise.

Shared-cache table contention returns LOCKED_SHAREDCACHE 262 with primary 6.
Unlock notification is registered before failed statement finalization; releasing
the writer notifies context 2 and a fresh retry succeeds. Callback storage stays
alive through cancellation/close and callbacks never call SQLite or unwind.
Two barrier-controlled threads use separate permitted connections for three read
rounds and join. The barrier permits overlap but does not establish simultaneous
in-engine execution on every scheduler run.

Worker resources release, cleanup/callback errors are empty and source/binary
freeze checks pass. Generated process maps exclude native SQLite. NativeAOT
explicitly pins clang 18; publish passes in 71.10 seconds with sampled process-tree
peak 3.346 GB. Short execution sampling and kernel waited RSS remain separate.
Other lock schedules, callback retry policies, deadlock cycles and cancellation
variants remain outside the finite proof.
