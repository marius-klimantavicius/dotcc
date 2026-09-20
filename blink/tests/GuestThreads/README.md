# Guest thread lifecycle

Native Linux and the separately built pinned threaded Blink passed the valid fixture. The same valid fixture also passed raw/optimized JIT and rooted NativeAOT through the C# threaded owner. This qualifies the bounded two-thread lifecycle below, not arbitrary threading or .NET runtime startup.

`fixture.S` is a static x86-64 Linux ELF with no C runtime or interpreter dependency. It sets the main thread's FS base to private initialized storage, then creates one child with the observed musl flags `0x7d0f00`, its own 64 KiB stack and FS storage, and a shared parent-TID/clear-child-TID word. The child verifies its stack and TLS, obtains its actual TID, updates shared memory, and announces readiness. The parent verifies the child TID, isolated main TLS, child TLS update, shared value, and stack sentinel before releasing the child. The child uses ordinary `exit(0)`; the parent waits for the cleared TID and uses `exit_group(0)`.

The readiness and release words use `FUTEX_WAIT_PRIVATE` / `FUTEX_WAKE_PRIVATE`. The clear-TID wait deliberately uses **non-private `FUTEX_WAIT`**, matching Linux's clear-TID wake. Each wait is attempted even if the word has already changed. A changed-value `EAGAIN` or interrupted wait causes the caller to recheck the word. There are no timing sleeps, injected failures, invalid accesses, or claims that a particular wait necessarily blocked. Assertion failures exit with a distinct nonzero status.

Both native executions must return zero, emit no stderr, and write exactly:

```text
guest-threads: tls=isolated shared=42 tid=cleared
```

The write loop handles positive short transfers. This is an explicit-FS test, not a PT_TLS loader test, musl startup test, robust-futex test, or broad threading qualification.

After review and serial execution release:

```sh
python3 blink/tests/GuestThreads/run.py
```

The runner reads the checksum-pinned Blink archive already in `blink/ref`; it does not fetch anything. Each attempt under `blink/artifacts/guest-threads/` extracts a private source tree and builds a separate native Blink. Its configure arguments are the pinned baseline arguments with exactly `--disable-threads` replaced by `--enable-threads`. JIT and fork remain disabled; the native binary runs with the baseline `-jm` arguments. It never runs upstream `make check`, rebuilds the managed compiler, or replaces existing native artifacts. The original archive members and existing baseline trees are checked after completion.

Each receipt retains the exact assembly/linker/runner inputs, source-manifest and archive identities, native configure files, compiler/binutils/make/strace/Python identities, command arguments, timeouts, closed stdout/stderr/trace hashes, ELF program headers, and execution binary hashes before and after both runs. Native Linux trace checks require the single clone's named flags, two observed TIDs, both private handshake operations, the non-private clear-TID wait, and normal child/group exits. Split strace calls are reassembled by TID; the full original trace remains available. The separately built Blink trace observes **host** syscalls, while the guest assembly's assertions and exact output establish its guest semantics. Native pthread implementation details are not claimed equivalent to the managed ABI.

The build uses the existing host toolchain and its dependencies; this is not a hermetic build. Commands have individual deadlines and owned process groups. On interruption or timeout the runner sends TERM, waits three seconds, then KILL and waits five seconds, retaining the outcome. It does not claim bounded cleanup of kernel-uninterruptible processes. A successful run requires normal group termination without cleanup signals. Failed attempts are retained and are never automatically retried.

## Same-owner managed matrix

After native qualification and a separately reviewed threaded delivery pass:

```sh
python3 blink/tests/GuestThreads/run-managed.py \
  --delivery-receipt blink/artifacts/threaded-delivery/attempt-REPLACE/receipt.json \
  --native-receipt blink/artifacts/guest-threads/attempt-REPLACE/receipt.json
```

This runner reuses the **exact native-qualified ELF**, without reassembling it. Each of raw JIT, raw rooted NativeAOT, optimized JIT, and optimized rooted NativeAOT runs in a fresh process. The private raw library is the delivery's frozen raw snapshot; the optimized library preserves its original `src` references in an explicitly copied repository-shaped closure. The same unmodified `Managed.Emulation.ThreadedExecution` source snapshot owns every run. No C execution loop or generated-source repair is introduced.

`Program.cs` creates private executable image/IO state, a 20-second cooperative deadline, and a dedicated main owner thread. It calls `ThreadedGuestExecution.Run` with a one-million-instruction ceiling and joins within 30 seconds. It requires normal completion before these safety bounds: no stop reason, signal, halt, observer overflow, or notification failure. It checks two distinct positive guest TIDs, one `ThreadExit` and one `GroupExit`, released Machines, all workers joined, `MemoryReleased`, and `IsQuiescent`. The actual clone arguments/return TID, three futex operations, and exit syscalls are observed through the owner's callback. The callback records at most 4,096 rows and the test reads them only after joins. Timings, addresses, instruction counts, TIDs, and wake counts are observations, not native/managed equality claims.

Private stdout/stderr must match the native references exactly; only that stdout is copied to the managed process stdout. A separate explicit `Utf8JsonWriter` report is NativeAOT-safe. On a failed qualification, a `passed=false` report retains the exception, observed progress/stop state, and synchronized guest output bytes (also saved as binary sidecars). Completed result and bounded syscall observations are included only after the main join and owner quiescence; otherwise they are explicitly unavailable. The runner hashes these diagnostics even when the process returns nonzero. IO and stop disposal occur only after the main thread joined and the owner became quiescent; otherwise the process must be discarded without freeing resources potentially still in use. The report distinguishes retained memory before release from successful release and does not invent post-disposal zero memory measurements.

Managed receipts bind the native/delivery/108-object producer chain, compiler identity, original and copied authored closures, immutable raw and final generated sources, owner/test sources, SDK/runtime binaries, closed command artifacts, reports, and execution binaries before/after. Four reports and exact output checks are mandatory. The outer process command deadline is 45 seconds; build/publish deadlines are separate. Shared host SDK/NuGet dependencies remain in use, so this is not a fully hermetic environment.

## Observed native result

`blink/artifacts/guest-threads/attempt-b_de41tq/receipt.json` passed both references, with receipt SHA256 `32b9cc19b5b9ca72574ee0a9a8c34852d3c155c44935357e7022268096427c82`. The exact static ELF SHA256 is `08bdd3a9bb68a53cfca1ab89479b2006ee55d3cea88754a9f11dbf56c899658e`; the separately built threaded Blink SHA256 is `f7e06af3a5adfb71492d6b92d2bf6339034ce5c1006d44aec733bf1be6a0a7b7`.

Both executions returned zero with the expected 50 stdout bytes and no stderr. Linux recorded one successful clone, the two distinct TIDs, a readiness wait returning zero, a release wait returning ordinary `EAGAIN`, a clear-TID wait returning zero, and normal child/group exits. This records the actual scheduling outcome, not a claim that both handshake waits blocked. The native Blink host trace is retained separately. All 14 commands completed normally without cleanup signals; 60 receipt input/log/execution identities were independently rechecked. The runner also verified original archive members and existing native baseline trees unchanged. The subsequent managed result is recorded below.

## Observed managed result

`blink/artifacts/guest-threads-managed/attempt-xh97uv94/receipt.json` passed all four modes; receipt SHA256 `1043a108e58bbed55ac8080bc10867b62dd8c7c4910b266aaa8e0eb79c584096`. It consumes threaded delivery `attempt-fxbxjsio` (SHA256 `0c1422e85054902e4caff93e8e6e9ff07096b90fdbdc65c262fb0136a493f5de`) and the exact native ELF above. All four processes produced the same 50 stdout bytes and empty stderr, with two distinct positive guest TIDs, normal child/group exits, both Machines released, all workers joined, memory release completed, and owner quiescence.

Raw/optimized JIT each observed 148 completed instructions; raw/optimized NativeAOT each observed 146. Scheduling-dependent wait paths are deliberately not normalized to one instruction count. Every mode reported two retained mappings and 270,550 charged bytes before shared-owner release; successful release is reported separately, without a post-disposal getter. The final audit rechecked 1,174 source/tree/log/binary identities, including 947 individually pinned inputs. All 11 commands exited normally with no cleanup signals.

The earlier `attempt-l08x_1yc` is preserved as a **harness preparation failure**, receipt SHA256 `a6126932fdafe7e3b88fc4fc2d6c255dd36bd993eccb037f05c41861b777969c`. Its raw build succeeded, but the strict process-group check found a surviving build descendant and sent TERM; no guest ran. The reviewed runner correction adds `--disable-build-servers`, `-p:UseSharedCompilation=false`, and recorded `MSBUILDDISABLENODEREUSE=1`. The successful retry keeps the original strict process-group rule for every command and does not weaken guest results or alter the fixture/product.
