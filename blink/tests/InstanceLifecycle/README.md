# Controller subprocess qualification

Run `python3 blink/tests/InstanceLifecycle/run.py`. The runner records source
hashes, commands, logs and the AOT binary hash for JIT/NativeAOT executions.

The fixture executable launches separate copies of itself speaking the actual
bounded protocol. Cases cover captured guest-like result payloads, graceful and
forced stop, deadline, worker crash, oversized incoming frame, diagnostic flood,
two simultaneous process owners, copied configuration, outgoing-frame preflight
and a root that exits while a child holds redirected channels. That deliberately
created child is independently killed by the test after checking bounded drain;
this does not claim cleanup of arbitrary detached worker descendants.

No guest instructions are executed. Passing these checks qualifies controller
mechanics; the service worker and actual HTTP lifecycle gate remain open.
