# Normal guest environment syscall integration

Native and raw/optimized JIT/NativeAOT pass at
`artifacts/guest-environment/attempt-emy2577e/receipt.json` (SHA256
`45d7a0080dfac1664a95c9bb269f80a3318192483396213ecb4c25ee619f7706`).
All five executions retain ten raw observation rows and agree on ten invariant
rows across two lifecycles. Final source/Host/tool/producer and binary identity
checks pass; an additional read-only check verified 381 retained log, binary,
producer and authored-source hashes. No implementation changes were required.
The preceding native-only receipt is `attempt-aeqsjnzi` (SHA256
`e746ff4a1a90cb15c02d8b423d3552ea20ca661ddcc6a74555e8c8782a66f925`).

The full run uses canonical assembly
`73428887715220efa9380f0dc8632efa11c202dbe67fe72630abda978cd4db75`,
retaining 108 producers and replacing only its frontend. Observed native clock
resolution was 1 ns; all managed forms reported the specified 100 ns quantum
with provider frequency 1,000,000,000 Hz. Native tid and timestamps remain their
actual process observations; managed tid was 73. Queried native action flags
were `0x04000004`, while the private registry returned `0x4`, as specified below.

The harness executes the actual `0f 05` instruction through pinned
`ExecuteInstruction`, using mapped guest arguments and the Linux register ABI.
It replaces only the authored frontend in an explicitly supplied canonical
109-object assembly and retains the other 108 objects and frozen host bindings.
It does not call guest `Sys*` helpers directly or supply their return values.

Two normal upstream lifecycles share one host owner. Each exercises:

- `clock_gettime` (228) for realtime and monotonic, twice each, followed by
  `clock_getres` (229). All timespecs are normalized and resolution is positive.
  Monotonic observations must not decrease. Realtime observations must lie
  within that execution's runner wall-clock bracket, allowing two seconds on
  either side. Realtime can step; a correction outside this bracket fails the
  qualification rather than being normalized away.
- `getrandom` (318), requesting 32 bytes with flags zero into a valid buffer
  crossing a page boundary. The return must be 32, access tracking must match,
  and both 16-byte canaries must survive. All returned bytes are retained. No
  nonzero, cross-call difference, entropy quality or randomness equality test
  is made. The managed consumer explicitly binds the frozen `BclHostEntropy`,
  which uses `RandomNumberGenerator.Fill`; it injects no substitute provider.
- `set_tid_address` (218) with mapped storage. The return must equal the actual
  positive `Machine.tid`, `Machine.ctid` must contain the supplied address, and
  the four initial bytes must remain unchanged. Managed identity is explicitly
  73; native uses its actual process identity. This does not qualify thread
  creation, clear-on-exit behavior or futex wakeup.
- `rt_sigaction` (13), setting Linux SIGUSR1 to IGN, querying it, restoring DFL
  and querying again, using the actual 32-byte Linux action and sigsetsize 8.
  The full guest records and upstream `System.hands` must agree. A separate
  read-only host `sigaction` query verifies the actual registration: upstream
  otherwise logs a host registration failure while returning guest success.
- `rt_sigprocmask` (14), blocking SIGUSR1, querying it, then restoring the exact
  initial guest mask. No signal is queued or delivered. Upstream propagates
  only job-control mask bits to the host; this test does not claim SIGUSR1
  changes the separate host delivery mask.

Native reference execution links the original pinned Blink archive and runs in
its own subprocess. It temporarily changes that subprocess's SIGUSR1 disposition
and restores DFL; it sends no signal. Managed execution changes only the private
registration table. Both paths require the queried host action to contain
SA_SIGINFO. The native Linux/glibc query may additionally contain SA_RESTORER;
the private registry must contain exactly SA_SIGINFO. Actual flags are retained,
and no common restorer address or asynchronous delivery behavior is claimed.

Raw clock fields, random bytes, tid, initial mask and host flags are saved in
every execution's stdout and parsed into its receipt. Exact comparison applies
to ten separate invariant lines, not the changing observations. Native monotonic
time measures system uptime; the managed provider uses elapsed time from its
owner's origin. The managed consumer also records the actual
`TimeProvider.System.TimestampFrequency`; output resolution must match the
frozen provider's 100 ns realtime quantum and monotonic maximum of 100 ns and
the rounded-up provider tick duration. Timestamp subsecond fields must be
multiples of 100 ns. These are representation contracts, not accuracy claims.

`clock_gettime` follows upstream's special dispatcher fast path. The remaining
calls exercise ordinary syscall temporary cleanup. Each call checks normal
return, expected instruction pointer and no pending/observed signals. Both
cycles release their guest pages and machine/system state. The managed owner
checks three remaining standard descriptors and a stable bounded retained
memory pool across cycles, then invokes disposal. Nothing queries disposed
translated storage; zero post-disposal allocation is not claimed.

The runner follows GuestIo provenance checks: original native archive/config,
immutable templates, all retained source/object/producer receipts, canonical
headers, frozen Host/bridge snapshots, complete compiler and postprocessor
identities, tool identities, closed logs and executed binaries before/after.
Raw generated sources are preserved separately from normal semantic
postprocessing. The complete derived library is rooted for NativeAOT.

Reproduce under the campaign's serialized build schedule:

```sh
python3 blink/tests/GuestEnvironment/run.py --native-only \
  --assembly-receipt blink/artifacts/core/objects/<qualified-key>/receipt.json
python3 blink/tests/GuestEnvironment/run.py \
  --assembly-receipt blink/artifacts/core/objects/<qualified-key>/receipt.json
```

Attempts use separate directories and `TMPDIR` under `generated/guest-environment`
and `artifacts/guest-environment`. The full run repeats native, then runs
raw/optimized JIT/NativeAOT. No shared compiler rebuild, mutable cancellation
overlay, timer/deadline/stop integration, service loop or worker API is included.
