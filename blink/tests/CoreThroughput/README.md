# Actual interpreter throughput

This harness measures one fixed hot integer loop through the actual upstream
`ExecuteInstruction` path. It does not run guest bytes on the host CPU, load an
ELF, start a service, or substitute an instruction implementation.

The eleven guest bytes are `48 ff c0 48 01 03 48 ff c9 75 f5`:

```asm
loop: inc rax
      add qword [rbx], rax
      dec rcx
      jnz loop
```

Each iteration executes exactly four guest instructions, including the final
untaken branch. The default measured batch has 250,000 iterations (1,000,000
instructions); five batches run in each of three fresh processes per mode.
Every process first performs the same fixed 40,000-instruction warmup. These
counts are selected before measurements and are not adjusted to improve scores.

Preparation/reset, semantic checks, printing, owner setup, and teardown are
outside the timestamp interval. The interval encloses the C dispatch loop and
its calls to `ExecuteInstruction`; loop/call overhead, runtime activity, and
scheduling pauses are included. Native timestamps use `CLOCK_MONOTONIC`;
managed timestamps use BCL `Stopwatch.GetTimestamp` and its reported frequency.
The two timestamp boundaries are authored callbacks, not guest instructions.
Warmup durations are discarded. External process wall time is recorded separately
and includes startup, warmup, checks, and shutdown; it is not an isolated startup
latency measurement.

Each batch must satisfy an exact semantic gate before its timing is accepted:
RAX=N, RCX=0, unchanged remaining general registers, RIP immediately after the
loop, the 64-bit sum N(N+1)/2, unchanged surrounding data and code bytes, unchanged
XMM storage and MXCSR, and arithmetic flags `0x44` under the explicit `0x8d5`
mask. Reserved bit 1 is checked separately. Timing cannot turn a semantic failure
into a pass.

The native executable links the pinned, unchanged interpreter archive with JIT
and linear mappings disabled. The managed executable replaces only the authored
frontend object in a qualified canonical core, preserving every retained object
hash and the exact physical header snapshot used by its producers. Authored host
bindings and the owning Host project are copied from that profile. Raw output is
never rewritten; a separate copy receives the normal postprocessor. Whole-library
AOT rooting matches the core qualification harness. This is derived linkage, not
a new translation of all retained source units.

One Machine and private owner live for one process. Batches reset that Machine
while its instruction/decode caches remain warm. Normal cleanup frees the Machine
and runs exit callbacks while host bindings remain available. Independent final
release discards private storage even if cleanup raises. No subsequent translated
core work runs after owner disposal.

Build and run small semantic checks first:

```sh
python3 blink/tests/CoreThroughput/run.py \
  --core-receipt <passing-core-execution-receipt> --prepare-only
```

Before timing, check the rejection gates using scratch copies of the prepared receipt:

```sh
python3 blink/tests/CoreThroughput/test-controls.py --prepared <prepared-throughput-receipt>
```

Once competing campaign builds have finished, run the frozen binaries:

```sh
python3 blink/tests/CoreThroughput/run.py \
  --measure-existing <prepared-throughput-receipt>
```

`--native-only` is useful during development. Such a receipt does not qualify the
managed modes. Final results require native plus raw/optimized JIT/NativeAOT.
Receipts retain every sample, per-process elapsed time, min/median/max/mean/sample
standard deviation, CPU affinity, CPU model/kernel/runtime details, host load,
source/object/tool/binary identities and commands. The measurement phase requires
the exact five prepared modes, unchanged provenance chain and executable paths,
and identical frozen artifacts before and after execution. Resolved .NET host and
installed Core runtime binary hashes must match preparation. A known-key allowlist
records tiering/PGO/JIT/GC settings, including explicitly unset values; arbitrary
environment values are not captured. Every measured child uses the
same permitted logical CPU. A fixed warmup does not establish that .NET tiered
compilation has reached steady state. Uncontrolled background load, frequency
scaling, SMT competition and ordinary run variation remain possible. Results
characterize this exact hot loop only, not general emulator throughput or a
performance-equivalence guarantee.

See [the measured Linux x64 result](RESULTS.md) for the qualified run and its scope.
