# Normal guest signal delivery

The actual Linux native fixture and all four managed forms pass this finite
signal-delivery contract using the same valid ELF.

This finite static x86-64 Linux ELF installs a SIGUSR1 handler using
`rt_sigaction` with `SA_SIGINFO | SA_RESTORER`. The handler verifies signal 10,
`SI_TKILL=-6`, and the actual sender PID and UID against its startup `getpid()`
and `getuid()` results. It executes a fixed 4,096-iteration register loop and
returns through an actual `rt_sigreturn` restorer.

The main thread first signals itself while unmasked. It then blocks SIGUSR1,
clones one child using the existing GuestThreads flags, and uses ordinary futex
handshakes to send the child one masked signal. The child proves that the signal
is pending and its handler has not run, then unblocks it. This deterministically
uses upstream `SysSigprocmask`'s recursive signal path. The child exits normally;
the parent waits for its clear-TID word and exits the group with exact output:

```
guest-signals: self=1 child=1 sender=checked pending=checked
```

Linux is the native semantic oracle. The pinned native Blink implementation
omits queued sender PID/UID and is known to differ on this contract; this runner
does not build it or mislabel that implementation as an equivalent oracle.
`run.py` reuses the existing GuestThreads process cleanup, valid ELF inspection
and strace decoder. It records the exact fixture ELF, tool/source identities,
closed command output and kernel signal notifications. No malformed images,
custom faults or signal-delivery failures are injected.

`run-managed.py` consumes that same ELF and an explicitly SHA-pinned current
public threaded delivery. It preserves all 108 product objects, shared semantic
selection provenance, raw/final sources and original authored source references
in private copies. The unchanged C# threaded owner runs the fixture in four
fresh processes: raw/optimized JIT and rooted NativeAOT. The ordinary bounds are
one million instructions, a 20-second cooperative deadline, 30-second main join
and 45-second outer process deadline. Actual success requires normal exits,
exact output, two released Machines, joined workers, quiescence and disposed IO.

Two exported assembly labels identify the handler's ordinary `gettid` syscalls
before and after its fixed loop. The native runner records their actual `nm`
addresses. At those exact instruction pointers the managed observer snapshots
the owner's completed instruction count. Each handler interval must account for
at least 12,288 instructions (4,096 add/decrement/branch iterations). These are
explicitly process-global marker counts and may include concurrent parent
handshake instructions; they are not attributed solely to the child.

Independently, each final per-thread instruction total must be at least 12,288.
The child's only non-handler loop is the release handshake. The fixture bounds
all ordinary futex retries to 32 per thread; even counting every branch and
helper instruction, the child's non-handler path stays below 2,048 instructions.
Its loop therefore cannot hide an unaccounted recursive handler behind parent
work or unlimited retry instructions. A retry-bound failure is an unsuccessful
fixture run, never a signal-delivery pass. Exact scheduling counts are not
normalized or compared with Linux.

This fixture qualifies only immediate self delivery, actual sender metadata,
cross-thread Machine notification, masked pending delivery, recursive handler
accounting, `rt_sigreturn` and normal cleanup. It does not claim pre-bind wake,
blocked-IO interruption, every futex actually blocking, arbitrary signals, or
general POSIX signal support. The separate HostIoCancellation fixture covers
the transient blocked-IO helper contract.

After source review and serial execution release:

```sh
python3 blink/tests/GuestSignals/run.py
python3 blink/tests/GuestSignals/run-managed.py \
  --native-receipt blink/artifacts/guest-signals/attempt-REPLACE/receipt.json \
  --delivery-receipt blink/artifacts/translation/attempt-REPLACE/receipt.json \
  --delivery-sha256 REPLACE
```

Failures remain recorded, including guest output, actual observations and owner
state where quiescence makes inspection safe. A failed join leaves borrowed
resources for process disposal. Source and report hashes are rechecked after
the matrix; no source receipt alone is treated as an execution pass.

The first native run passed all five commands in
`blink/artifacts/guest-signals/attempt-y8mbkxpd/receipt.json`, SHA256
`78c0dc100f4ac103e990d8f6ebfc6d68526815b9f2dab529c4019b5526306467`.
ELF SHA256 is `5f35f34a2f4e7bbc442074bc3a1e0e0351164a426ce6b4d72bfad563a090e961`.
The kernel trace records both actual sender identities, the child's masked
pending signal, its unblock, two `rt_sigreturn` calls, and normal child/group
exits with exact stdout and empty stderr. The handler marker syscall addresses
are `0x400399` and `0x4003c2`. No cleanup signals were needed.

The managed run passed all four forms in
`blink/artifacts/guest-signals-managed/attempt-wzgc590u/receipt.json`, SHA256
`ec24a1d0d30133108696e1492fbd5760400550425a4d7712a3fc1efcd2cd9c09`.
It consumes public delivery `translation/attempt-8roztrys/receipt.json`, SHA256
`348c75d3f80072a765618ee425a80dbf25fc4f624430aeccb0117fccd208fd7a`.
Every form observes two actual `rt_sigreturn` calls, exact native stdout,
empty stderr, normal child/group exits, released Machines and memory, joined
workers, owner quiescence and disposed IO. Both marker spans count 12,295 owner
instructions. Independent child instruction totals are 12,374–12,376, proving
the recursive fixed loop participates in per-thread accounting. Total guest
instruction counts are 24,811–24,815; scheduling differences are preserved.
All 11 commands completed without cleanup signals. A subsequent audit rechecked
1,246 individually pinned source, log and binary identities.
