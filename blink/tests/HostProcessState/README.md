# Shared process registration state

Native and raw/optimized JIT/NativeAOT qualification passed in
`artifacts/host-process-state/attempt-o9f8h7kl/receipt.json` (SHA256
`24e8ae53f5ae1a9b472c27d13797b5234f6fc6e0d2d63cb2203a6f5c40b5210d`).
All five executions printed exactly
`shared disposition; callbacks BCA once; clean end`. Source/tool snapshots,
generated sources, runtime binaries and command logs were reverified after the
matrix. This qualifies the normal shared registry scenario, not guest thread
execution or the complete threaded profile.

The `BLINK_MANAGED_GUEST_THREADS` branch of HostSignalActions and
HostExitCallbacks uses process-private registries protected by real pthread
mutexes. The translated form uses the existing BCL pthread implementation and
managed scalar handle ABI; the native oracle uses ordinary native pthread
objects. The disabled profile retains its TLS storage and existing behavior.

The owner calls both Begin functions once before starting workers. Workers may
read/update dispositions and register callbacks. After all workers join, the
owner runs callbacks while their dependencies remain valid and then ends both
registries. End is not a worker detach operation. The internal static mutex
handles remain allocated until the containing worker process exits; this is a
bounded pair of handles, not a reusable multi-instance registry API.

The exit registry unlocks before each callback and relocks afterward, preserving
its existing phase/generation checks. Registration from a callback works, and a
callback unwind cannot strand the mutex. As before, an escaping callback leaves
the run in its running phase until owner teardown. The changes add no callback
delivery or C execution lifecycle framework. Lock failures terminate explicitly;
they cannot yield a success result with unprotected storage.

`run.py` qualifies one normal native scenario and raw/optimized
JIT/NativeAOT equivalents. Two real threads share a disposition: the first sets
handler A and registers callback A; the second observes A, installs B and
registers B; the first then observes B. After joins the main owner runs B, whose
registration of C yields exact B,C,A order. A second Run invokes nothing; End
clears counters. Successful operations preserve distinct thread errno values.
The managed consumer uses dedicated C# Threads, not the intentionally blocked
generic pthread_create execution path. Its joins precede registry teardown.
The compiler's include registry gives later include directories priority, so
the runner lists generic headers first and the threaded overlay last. The
managed probe asserts both reviewed header markers and the scalar pthread ABI;
its separate fixture config is not a complete guest core configuration.

Native module calls resolve to the authored private signal registry compiled
against native signal records; this is not a test of OS signal delivery. No
custom invalid inputs or injected callback failures are included. Existing
disabled-profile registration/callback suites remain separate regressions.
