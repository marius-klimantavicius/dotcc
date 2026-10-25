# Guarded guest alternate signal stack

`python3 -B blink/tests/GuestSignalStack/run.py --native-source <configured-threaded-native-tree>`
builds a valid static x86-64 ELF and runs it on Linux and on private native Blink
executables. Native compilation
is serial and bounded. Shared native output, translated C#, public consumers and
the CoreClrRuntime harness are not changed or built by this runner.

The existing native tree must provide its configured headers, `o/blink/blink.a`
and `o/blink/blink.o`, with threads enabled for the ordinary `tkill` syscall.
For this checkout the retained GuestThreads witness supplies that input at
`blink/artifacts/guest-threads/attempt-d60bbksy/native/blink-f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580`.
The shared default native profile disables threads and omits `tkill`; it cannot
serve as a delivery witness. The runner copies the selected native inputs and
never reconfigures or rebuilds their original tree.

The guest maps 16 KiB read/write, protects its first 4 KiB with `PROT_NONE`, and
registers the entire 16 KiB descriptor with `sigaltstack`. It installs an ordinary
SIGUSR1 `SA_ONSTACK | SA_SIGINFO | SA_RESTORER` handler and sends itself two
signals. Each handler checks that its actual stack pointer lies in the writable
12 KiB region and that querying `sigaltstack` reports `SS_ONSTACK`, the original
base and the full registered size. Each actual `rt_sigreturn` restores the main
stack and a register deliberately changed by the handler. The guest then disables
the alternate stack, restores the default signal action, unmaps the allocation,
prints exact output and exits normally:

```
guest-signal-stack: guard=4096 registered=16384 onstack=2 returned=2 disabled=1
```

The Linux trace records protection, registration, both deliveries, both returns
and normal cleanup. The same valid workload exits at registration (code 24) in
private native Blink compiled with the original syscall source, then passes with
the authored `scripts/signal_bindings.py` adaptation. Both use the same existing
native archive and configuration, copied into the attempt. The runner checks
that every other syscall function is byte-identical and that upstream
`DeliverSignal` still validates the actual frame through `CopyToUserWrite`.
This is a successful lifecycle regression, with no malformed ELF or injected
faults. It does not qualify nested signals, autodisarm, stack exhaustion or all
Linux alternate-stack behavior.

The registration change follows Linux's descriptor-only registration in
[`do_sigaltstack`](https://kernel.googlesource.com/pub/scm/linux/kernel/git/torvalds/linux.git/+/refs/tags/v6.1/kernel/signal.c#4173).
Existing descriptor access, flag, minimum-size and active-stack checks remain.
Frame write validation is independent of registration and remains in delivery.

Attempt receipts under `blink/artifacts/guest-signal-stack/attempt-*` retain exact
source, tool, native archive/configuration and ELF hashes, commands, stdout/stderr,
Linux trace and process-group cleanup. The parent's fresh producer must run this
same ELF through managed Blink; this runner makes no managed execution claim.

The retained CoreCLR fresh attempt `coreclr-runtime/guest/attempt-nu5voo4t`
records a successful 16 KiB RW mapping, protection of its first 4 KiB as NONE,
then `sigaltstack=-EFAULT` in smoke observations 3478–3480 (zero-based). The
enclosing integration receipt is
`coreclr-integration/coreclr-second-fresh-od7wm0p_/receipt.json`.
The parent reports that the prior coordinator stopped after an automated content
filter cited possible cyber risk; that stop remains historical evidence and no
safeguards are disabled or circumvented by this compatibility work.
