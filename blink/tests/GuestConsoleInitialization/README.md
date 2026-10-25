# Console initialization diagnostics

This diagnostic consumes the frozen r8 public closure from
`artifacts/coreclr-runtime/guest/attempt-zqy711oa` and the retained CoreCLR
10.0.12 images from `native/attempt-h40bq9ta`. It copies sources into an
attempt-local private tree and instruments only its execution owner and test
harness. It never changes product sources, generated sources, shared builds,
terminal policy, or guest runtime images.

Run from the repository root:

```sh
python3 blink/tests/GuestConsoleInitialization/run.py --managed-smoke --seconds 600
```

The native fixture first checks terminal, pipe, fcntl, signal and default pthread
initialization individually, then invokes the exact retained
`SystemNative_InitializeTerminalAndSignalHandling` in a separate guest process.
Both use the public `BlinkMachine.ImportImage`/`StartAsync` path. With
`--managed-smoke`, the original managed smoke also runs with syscall observation.
The native reports use the original smoke report schema, so their `Passed`
field reflects the smoke-output comparison; inspect `Result`, captured output,
and cleanup fields for these diagnostic fixtures.

Every attempt retains the input identities, private modifications, command
arguments, production-support lock wrappers, logs, limits, process cleanup, and
syscall observations in `blink/artifacts/coreclr-runtime/console-initialization`.
Builds use one MSBuild worker. Runtime commands have instruction and wall-clock
limits. Diagnostic collection is not a fresh product qualification.

The exact binaries advertise commit
`95017c711e6afc1085133d440e42b4bd78155701`. Public source requests for that
commit returned HTTP 404. The retained binary disassembly and decompiled
`System.Console.dll` are authoritative for this diagnosis. Downloaded v10.0.0
runtime sources are explicitly **reference only**, with URLs and hashes retained
in the attempt's `reference/sources.json`.

## Causal result

The retained r8 smoke's first Console initialization failure is the native signal
worker's default pthread stack reservation exceeding guest `RLIMIT_AS`.
In private attempt `console-initialization/attempt-5022m5n9`, trace row 6896
returns ENOTTY for `tcgetattr`; row 6898 successfully creates the native signal
pipe. Row 6899 requests `mmap(NULL, 0x801000, PROT_NONE,
MAP_PRIVATE | MAP_ANONYMOUS | MAP_STACK, -1, 0)` and returns ENOMEM. The next
two syscalls close the signal pipe descriptors 30 and 31. No clone or signal
handler installation is reached on this initialization path.

At the failing reservation, VSS is 64,530 pages and the request is 2,049 pages:
66,579 exceeds the 65,536-page (256 MiB) virtual cap. Only 4,120,576 bytes of
virtual allowance remain. RSS is 4,456 pages; owner backing is 56,302,286 bytes
and memory-file backing is 19,394,816 bytes. The exact generated `SysMmapImpl`
rejects this request at its virtual-limit check before reserving backing.

The exact native binary's `pthread_create` failure path closes the pipe and
returns false, which the exact retained `System.Console.dll` converts into
`Win32Exception`. The later exception-formatting RW mapping failure is a
separate cascade under the same virtual-space pressure. Early failed 64/128 MiB
malloc arena reservations are recoverable and precede this Console failure.

The small native public-import diagnostic initializes successfully under the
same policy and exact library, leaving ENOTTY in errno after success. Its
separate preflight also succeeds for `pipe2(O_CLOEXEC)`, fcntl operations,
default pthread creation, and a signal disposition query. This excludes a
blanket terminal/pipe/thread/signal implementation failure.

The immutable causal prefix, exact values, identities, and binary inspection
references are retained at
`blink/artifacts/coreclr-runtime/console-causal-analysis/attempt-20nj8hyh/receipt.json`
and its `finding.json`. The full baseline is retained under
`blink/artifacts/coreclr-runtime/console-initialization/attempt-5022m5n9`.
It finished with SIGABRT after 284,364,429 instructions in 399.2 seconds,
reproducing the later RW mapping fatal error during exception formatting.
The run and machine were disposed, guest resources released, and the process
group exited without forced cleanup. The prefix SHA-256 is
`a0dddd93706f49a5e5021f386040ccb021261c700dbcd4bdd1f40b8339d9164b`.
No product or memory-policy change is part of this diagnostic. A separate
virtual-memory allowance can be evaluated by the parent and memory sibling;
this test does not override the cap.

The first uncached-observer attempt (`attempt-5dgn7mkc`) was intentionally
stopped with SIGTERM after the parent measured excessive diagnostic overhead;
its process group is gone and its native results remain retained. Subsequent
attempts `attempt-qhi70vgb` and `attempt-5_797k0j` failed cleanly on the
nonqueuing native and managed build locks respectively. The successful attempt
uses `LoadInstruction`'s normal instruction cache inside the original trap
boundary, preserving translated errno. These timings measure diagnostic
instrumentation, not product performance. Binary-inspection attempt
`attempt-z22zfh2t` retained a relative-path invocation error, corrected in
`attempt-0pj0cqfz`.

Prior coordinator handoff: a prior coordinator reported termination by an
automated content filter for possible cyber risk. This fact is retained in the
diagnostic receipts; safeguards were neither disabled nor circumvented. The
current scope is ordinary local C# runtime compatibility diagnosis.
