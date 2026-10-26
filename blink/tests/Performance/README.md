# Bounded interpreter measurements

`Bench.csproj` references an explicitly selected, frozen assembly directory.
It never regenerates Blink or selects an old library implicitly. Supply all
three matching assemblies: `TranslatedBlink.dll`, `Managed.Emulation.Host.dll`
and `Managed.Emulation.ThreadedExecution.dll`.

Build outside a timing run, using the repository production build locks:

```sh
python3 Scripts/production_support.py dotnet build \
  blink/tests/Performance/Bench.csproj -c Release \
  -p:BenchLibrary=/absolute/path/to/frozen/lib -o /absolute/path/to/bench
```

Run each command serially in an owned, externally bounded process group:

```sh
dotnet /absolute/path/to/bench/Bench.dll loop guest.elf result.json 6
dotnet /absolute/path/to/bench/Bench.dll host closure.json result.json 6
dotnet /absolute/path/to/bench/Bench.dll prefix closure.json result.json 6
dotnet /absolute/path/to/bench/Bench.dll service closure.json result.json 1
```

The caller records guest, producer, compiler, source and executed closure hashes
before and after execution, host SDK/runtime, tiering, external bounds and
process-group cleanup. `DOTNET_TieredCompilation=0` is a diagnostic setting;
default-tiering measurements and NativeAOT hosts are separate observations.
Publish this same harness with `PublishAot=true` to measure a NativeAOT **host**.
That does not change the guest publish mode.

`loop.S` supplies three arithmetic controls, each with ten million iterations
and a checked normal exit. Assemble it with `-DREGISTER`, `-DSTACK` or `-DFILE`,
then link each object with `ld -static -T ../GuestExecutionStop/fixture.ld`.
Use separate assembly and link steps, as in the retained receipts; a compiler
driver's implicit build-ID section is outside that fixture linker script.

Import/hash checking is outside the execution interval. Results include wall
and process CPU time, allocations, peak process working set, exact completed
instructions, retained owner accounting, stop reason and joined/released state.
Peak working set is process-wide and may include prior trials. Trial zero is
marked warmup; a single service run remains a single observation. Timing values
are observations, with no timing assertions. Scalar per-thread termination, IP,
signal and first-trap diagnostics are retained on failure.

`host` runs the retained CoreCLR `--info` scenario. `prefix` runs precisely
2,000,000 completed instructions of service startup and requires Budget plus
complete cleanup. Both consume the normal CoreCLR image closure schema from
`tests/CoreClrRuntime`. The prefix does not claim readiness or service success.
An optional closure `Executable` selects a native guest instead of `/dotnet/dotnet`.

`service` uses the unchanged `GuestKestrelMinimal`/`CoreClrReadyToRun` middleware
contract: three health requests, two echo requests, then normal HTTP stop. It
requires exact bodies and stdout. Readiness and each request record completed
instruction snapshots, wall and process CPU time. Readiness is sampled after
observing stdout at ten-millisecond intervals, so it is an upper-bound snapshot,
not an exact instruction at the write. Request deltas include concurrent guest
workers and response observation. The finite service bound is 180 seconds and
150 million instructions. Optional final arguments select a service deadline
(1–7,200 seconds) and budget (at most three billion). CoreCLR services may exceed
the defaults; failures remain failures.

Non-loop runs retain 256 MiB backing, explicit 4 GiB virtual space, the default
64 MiB memory-file sublimit, 512 MiB storage/image, 8,192 nodes, 1,024 descriptors
and 64 guest workers. Loops use 32 MiB backing, 40 million instructions and a
60-second deadline. Tracing is disabled in every measurement.

`semantics.S` is a small valid static ELF control for overflow/carry/zero flags,
the ordered scalar SIMD comparison-mask regression, and unaligned vector loads
across a guest page boundary. Assemble/link with the existing
`GuestExecutionStop/fixture.ld`, then run native Linux and native Blink controls
before executing it as a `loop` in each selected translated host/form.
For the retained performance run, native Linux and all baseline/candidate
managed forms pass. The pinned native Blink exits 1 on this SIMD mask control;
that unsuccessful comparison is retained and is not used as the oracle for
the managed control.

`../HostExecutionStop` checks public cancellation propagation and monotonic
requested/budget/deadline state. Its optional `--measure` prints isolated
null-timeout Reason-read wall/CPU samples; `StopSource` can select a frozen
baseline source for a matched A/B. Whole guest budget, cancellation and recursive
signal checks remain separate execution-owner gates.
