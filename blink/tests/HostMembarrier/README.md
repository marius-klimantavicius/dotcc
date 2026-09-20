# Single-thread guest memory barrier

Native Linux and raw/optimized JIT/NativeAOT passed in
`artifacts/host-membarrier/attempt-a4b_894o/receipt.json`, SHA-256
`48a786c8bc411ad91afd27b94054e1da443f010e94971b511038632d729b6205`.
The runner exited zero; all four mode results and final identity checks passed.
An independent final review checked 524 recorded input/source/generated/log/
execution hashes, including the unchanged authored consumer/bridge compiled in
both modes. There are 15 closed command logs. All five executions printed:

```text
query subset=24
registration repeated=2
fences=2 value=52
```

C# also required exactly one successful real process-wide capability probe.
This owner
implements the Linux command subset QUERY=0, PRIVATE_EXPEDITED=8 and
REGISTER_PRIVATE_EXPEDITED=16. Query reports24, registration is idempotent, and
each expedited call executes BCL `Interlocked.MemoryBarrierProcessWide` and
increments its actual completion count. First query probes the same real BCL
operation once before advertising support, caching success/failure. Registration
also ensures capability when called before query. The two requested fences and
one successful capability fence have separate counters. Success preserves errno.
The owner is constructed and
called on the same thread; bridge binding cannot transfer it to another thread.

This contract applies only to the selected guest with one interpreter thread,
no guest fork and no concurrent guest-memory executor. The BCL fence really is
process-wide; it does not pin memory or implement guest thread ownership. Production staging
guards/owner binding are integrated separately. A future threaded profile must
qualify its thread and shared-memory lifecycle; a local-only fence must never
replace this BCL operation. The runtime's internal platform calls are part of
the BCL boundary, like those behind managed sockets; there is no authored P/Invoke.

The native oracle calls actual Linux membarrier, retaining the full captured
output while comparing only the supported query subset24. The translated C
caller queries, registers twice, and issues two real fences around volatile
stores/reads. C# additionally verifies registration, two completed commands and
one successful capability probe. This is
normal-path execution evidence; a same-thread visible store is not a proof of
cross-thread memory ordering. No injected failures or invalid-input tests run.
Unregistered calls/foreign threads return EPERM, unsupported commands/nonzero
flags EINVAL, and unbound callbacks ENODEV by source policy; those error paths
are not exercised by this bounded normal fixture. CPU id is ignored for these
commands, matching Linux when flags is zero.
An unsupported BCL implementation returns ENOTSUP and does not advertise the
subset; other BCL exceptions map to EIO, with no successful registration/fence
claimed. See [the official BCL process-wide barrier contract](https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.memorybarrierprocesswide?view=net-10.0).

After source review and explicit serial-slot release:

```sh
python3 blink/tests/HostMembarrier/run.py
```

The runner snapshots existing Release compiler/postprocessor outputs, standard
headers, the authored Host project and bridge; it never rebuilds shared tools.
It retains raw generated source unchanged and postprocesses a separate copy.
Actual native stdout must equal raw/optimized JIT/NativeAOT stdout in all four
modes. Receipts contain source/tool/log/generated/binary hashes and execution
before/after identities. Outputs live under `artifacts/host-membarrier/attempt-*`.
This small callback fixture does not claim execution of the guest SYSCALL
dispatcher or successful .NET guest startup.
