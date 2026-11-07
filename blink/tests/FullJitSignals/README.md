# Focused Full-JIT Linux signal controls

These static Linux x64 guests test deliberate UD2/XGETBV illegal instructions
and guest-unmasked SSE divide-by-zero. The virtual CPU profile advertises neither
XSAVE nor OSXSAVE. The SSE fixture requires no optional host instruction set.

`context.c` installs a real `SA_SIGINFO | SA_RESTORER` SIGILL action. Assembly
sets known RAX/R12/RSP, arithmetic flags, and MXCSR before the fault. The handler
requires Linux x86 `ILL_ILLOPN=2`, `si_addr=RIP=fault_site`, trap 6/error 0,
unchanged registers, architectural parity flags, valid CS and saved MXCSR. It
edits saved RIP to an explicit fallback and saved RAX/flags, changes its live
MXCSR, then returns through syscall 15. The fallback checks restored state. Two
rounds prove that the signal mask was restored as well.

`caught-simd` uses DIVSS 1/+0 with guest divide-by-zero unmasked. Its handler
requires SIGFPE/FPE_FLTDIV=3, vector19/error0, faulting RIP, committed MXCSR
divide status and unchanged XMM destination. `caught-simd-recursive` runs the
same two recovery rounds from a SIGUSR1 handler. Both restore the saved MXCSR
through real rt_sigreturn. Native-kernel controls establish these expectations;
managed qualification requires a fresh producer exporting vector19.

`caught-ud2-recursive` blocks SIGUSR1, queues it to the same TID and unblocks it
with `rt_sigprocmask`. Its SIGUSR1 handler runs both UD2 recovery rounds. In Blink
this deterministically selects `SysSigprocmask -> DeliverSignalRecursively ->
SignalActor`, then nested synchronous SIGILL. No second thread or timing race is
needed. `uncaught-ud2` emits a fixed marker and must terminate by SIGILL. The
native runner disables core dumps for its own process groups.

`cpu-policy.S` emits eight raw CPUID result rows. The managed consumer checks the
declared virtual baseline and absent optional advertisements, including
XSAVE/OSXSAVE/AVX. `host-capabilities.c` records actual host CPUID and executes
XGETBV only when the actual OSXSAVE bit permits it. This evidence is explicitly
distinct from virtual policy; no simulated capability mask is used.

Build/run the small native controls in a new task-storage directory:

```sh
TMPDIR=/mnt/storage/blink-full-jit-20261027/tmp \
  python3 -B blink/tests/FullJitSignals/run.py \
  --scratch /mnt/storage/blink-full-jit-20261027/cpu-signals/new-native-run
```

The runner refuses existing scratch directories or storage outside this task.
All guest binaries, logs, symbols, manifests and temporary directories stay
there. It verifies the original signal-source hashes and six adapter tests,
builds seven guests, runs direct/nested caught UD2 and SIMD plus uncaught UD2 against the
actual Linux kernel, and records every command, hash, exit and process cleanup.
XGETBV is guest-only because the actual host may support it. Native CPUID is not
compared to the deliberately different virtual CPU.

The parent owns fresh strict C production, raw/processed closure creation and
CoreCLR/NativeAOT builds. Add `FullJitSignals.csproj` to that frozen build and
deploy its matching worker using the existing consumer targets. No generated C#
is edited by this suite. Invoke each already-built consumer with:

```sh
dotnet /path/to/frozen/FullJitSignals.dll /path/to/fixtures /path/to/results.json
/path/to/nativeaot/FullJitSignals /path/to/fixtures /path/to/results.json
```

The fixture directory must include the runner's `manifest.json`. The consumer
hash-checks each ELF and checks 30 observations: five fixtures, both public
execution modes, and Interpreter/Hybrid/Full. It requires matching reason,
status, signal, output and completed-instruction counts; the uncaught result
also requires exact diagnostic fault address and signal code. Full must report
positive inline-plus-helper work, zero interpreter instructions and attempts,
and `Mode=Full`. All rows require released resources, an empty native cache and
a reaped separate-process worker. Failed observations are saved before checks.
Run the same executable on a real Windows host for Windows evidence; a Linux
pass or an artificial capability mask does not qualify that host.

Optionally pass `--consumer /path/to/already-built/FullJitSignals.dll` to the
Python runner to perform the same managed matrix after its native controls.
This option does not build a .NET project. Managed build/qualification results
remain separate from native-oracle and adapter-guard passes.
