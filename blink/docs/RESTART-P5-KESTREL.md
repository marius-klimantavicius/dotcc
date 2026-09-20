# P5 Kestrel restart handoff — stopped by user

The user requested saving state and stopping because the session was approaching
limits. No further implementation, build or guest run is authorized until resume.
No compiler/build/guest processes were active when this handoff was written.
Both workers acknowledged stop. P5 remains open; P6 has not started.

## Workspace and preservation

Work directly on branch `sqlite` in `/home/marius/p/dotcc`. Last implementation
commit is `51948ad` (bounded memory and activation failure evidence), preceded by
`7451cce` (2,056 CPU comparisons). The handoff commit changes documentation only.
Incomplete source work remains deliberately uncommitted and must be reviewed,
not blindly restored over later changes.

An immutable backup of all 25 pending authored files, a binary working-tree
diff and hashes is at `blink/artifacts/attempt-7jhw__6l/`. Its `manifest.json`
SHA-256 is `70479212118c9b205bf20d13789ea363915ddeb291e4783585d16da95552fe62`.
The new untracked `src/Managed.Emulation.Host/HostSignalWake.cs` is included in
the `files/` backup; it is not included in ordinary `git diff`.

Preserve the user's three patches in the repository root:
`0001-Fix-fixed-address-storage-for-generated-globals.patch`,
`partial_blink.patch`, and `patch1.patch`. Their hashes are recorded in the
snapshot. Preserve libsmb2, prior receipts, raw generated snapshots, and existing
recovery worktrees. Do not create a duplicate implementation branch.

## Current observed result

The actual static-musl ASP.NET Core/Kestrel NativeAOT guest was built successfully
on the first standard pinned container attempt. The difficult-musl-build stop
condition did not trigger. This is genuine Kestrel, not the older raw-socket C#
fixture or NativeAOT compilation of the emulator host.

The corrected SIMD mask behavior passes all 514 selected cases in all four
managed forms (2,056 comparisons). The old 64 MiB guest address-space ceiling
then blocked Gate-thread allocation. A selected **128 MiB coupled AS/DATA and
backing ceiling**, keeping the default 64 MiB and guest GC limit 16 MiB, reaches
Kestrel READY and passes health, large and fragmented HTTP requests.

That run still fails: guest thread 262146 requests `tkill(262150,35)`, receives
`-95` (EOPNOTSUPP), and aborts itself with signal 6. Missing-route and normal-stop
requests do not complete. The client eventually reaches its 60-second deadline;
that is distinct from the earlier guest abort. All nine guest Machines and
workers release. There are no mmap ENOMEM results in this 128 MiB run.

Do not increase deadlines or instruction limits to hide this failure. The
current bounds remain 100 million instructions, 60 seconds and 16 workers.

## Exact evidence

| Evidence | Receipt under `blink/artifacts/` | SHA-256 |
|---|---|---|
| Real Kestrel musl producer | `kestrel-guest-musl/attempt-o5jvvf7t/receipt.json` | `7bc07c1e8d01dd3d326fdbb436473ff0b2b8dcaf2910aea6fffebdaa7b119865` |
| Native five-case HTTP profile | `kestrel-native-profile/attempt-zphi57zq/receipt.json` | `e1e3c2ecf4c929f6f13d0f4937757cdc0dc82ee2b55d2c76d1fd88c4ec7db01a` |
| Last published product | `translation/attempt-tf_6yqk6/receipt.json` | `92763476d1719166912ecbf2d5ca31cabb1feb512cfcef278a288319980524a5` |
| CPU all-four pass | `cpu-conformance-managed/attempt-jt41ulk6/receipt.json` | `c62ea727bde462c938a91a67c6c3f4ae0ef69bff6c3bb28be1fe0bb06e10a7b2` |
| Partial Kestrel 128 MiB run | `kestrel-guest-execution/attempt-uq52p1wf/receipt.json` | `20727ac9628d261882dfd21e9efe3aee271c1b80cb7b820c93bb0876960cb04d` |

Guest ELF: `kestrel-guest-musl/attempt-o5jvvf7t/publish/KestrelService`, SHA-256
`ef6f1433794a42fe32b0fed4851bf88dd0631cd6a836550c6effca79d9e9a3ac`.
The six selected environment variables and native response rules are recorded
in [P5-KESTREL-RUNTIME.md](P5-KESTREL-RUNTIME.md). Preserve exact values and
compare actual HTTP; only validated Date values may differ.

The last generated product has 108 producers, direct references to original
`src`, literal pooling and inline deduplication (one IsJitDisabled definition,
zero Libc.L calls). It does **not** include the pending signal integration.
Live source references have now changed, so rebuilding it is a new, unqualified
combination. Do not reuse an old receipt as proof of current live sources.

## Pending signal implementation and ownership

All following changes are source-only: no compilation, public regeneration or
guest run has used the new signal boundary.

- Coordinator: `IHostGuestThreads.cs`, `HostGuestThreadsBridge.cs`, and
  `ThreadedGuestExecution.cs`. Ordinary and recursive handler execution share
  `ExecuteOne` accounting/tracing/stop checks. Nested dispatch returns through
  upstream syscall cleanup on stop. Per-worker metadata retains actual tkill
  PID/UID, preserves first pending sender, handles immediate self delivery
  separately, discards ignored/default delivery, and clears on release. Wake
  helpers bind per worker and dispose after quiescence.
- `/root/blink_coordinator/kestrel_guest`: `UpstreamGuestThreads` stage/patch/docs,
  `Host/include/host-guest-threads.h`, `UpstreamMremap` stage/patch/docs and
  `scripts/stage-threaded-core.py`. Six narrow callbacks hand execution/wake/
  metadata to C#, while upstream frame construction, mask handling, signal
  selection, recursion scaffolding and rt_sigreturn remain. `signal.c` joins
  syscall.c and memorymalloc.c in the threaded source replacements.
- `/root/managedconsumer_verify`: new `HostSignalWake.cs`, six IO/network/message/
  readiness/epoll/sleep bridges, and `tests/HostIoCancellation` Program/probe/runner.
  Request marks a generation and queues cancellation outside caller locks;
  per-operation leases retain their assigned wake; Checkpoint acknowledges
  immediately before upstream ConsumeSignal. Successful partial I/O survives.
  Ordinary permanent cancellation remains 125; transient wake maps to EINTR.
  Existing epoll and sleep EINTR cancellation exceptions remain unchanged.
- The same verifier owns four separate prepared sample files:
  `ManagedConsumer/{Program.cs,README.md}` and
  `tests/ManagedConsumerDelivery/{run.py,README.md}`. They select the real guest,
  six environment values and 128 MiB profile, with actual native HTTP semantics
  and explicit delivery provenance. They have not built or run.

Why these changes are necessary:

1. Upstream already queues guest signal 35, but its host pthread wake was rejected.
   Actual target state at enqueue is not proven; its final trace is a futex wait.
   Futex checks interruption every 50 ms; blocked IO needs a real transient wake.
2. Actual ELF handler `0x463b20` checks siginfo sender PID against guest getpid.
   Upstream's zero-filled sender field fails that check. The new metadata is
   taken from the actual guest sender, not a fixture-specific handler bypass.
3. Existing SignalActor dispatches recursively outside the C# budget. The new
   actor callback keeps execution in the same authored C# accounting path.
4. NewMachine initially stores its creator's pthread identity, until the child
   dispatcher replaces it. The latest wake callback therefore targets the
   actual owned Machine, including a child that has not bound yet.

The latest sixth-callback source state has **not** repeated the complete staging
verification. Prior five-callback source-only verification is
`threaded-signal-staging/attempt-b7j2eo2x/verification.json`, SHA-256
`4793aa58c0241f9f3b47cdd699f2daa1c623e219db3008984515e3e417fa43c0`.
Its successful zero-fuzz patch checks do not qualify the later Machine-target
wake edit. Current in-memory staged hashes reported by its worker are syscall
`60b42ebabd8c4312f5ce98d29fd71b8aee69a8c3297b94f71fa10bd490dc6012`,
signal `05ed4528a60ac33f0c27b5a3ed66e184f3f8416209e677c388f54399c4758a7e`,
and downstream Mremap syscall
`8eecd697e5f1e4b2a04a68d90fa7596b7802aaa5a6a378b56121fc2fe67c21a8`.

## Minimal resume sequence

1. Read this handoff, PLAN and current diff. Keep the incomplete edits and inspect
   the immutable source snapshot if anything changed. Reestablish serial build
   ownership; do not duplicate workers or jobs.
2. Fix the known harness race: blocked `TransientWake` currently calls Request
   twice sequentially, allowing a checkpoint between them to interrupt its next
   read legitimately. Use one request there; deterministic repeated-request
   coverage already exists before registration. Update its README for 32 normal
   scenarios (previously 22); no new scenario has run.
3. Independently review helper races/lifetime, all six callbacks, metadata and
   nested stop cleanup. Repeat fresh threaded and Mremap staging plus zero-fuzz
   patch checks. The source-only verification recipe/logs are retained in the
   prior staging attempt. Unified patch context blank lines intentionally contain
   one space; do not corrupt checked patches to silence whitespace diagnostics.
4. Run `python3 blink/tests/HostIoCancellation/run.py` serially: native ABI smoke,
   then raw/optimized JIT/NativeAOT normal cancellation/wake contracts. Correct
   actual defects, preserve failed receipts, and commit meaningful progress.
5. Run `blink/scripts/translate.sh --offline --profile threaded` after source
   freeze. It revalidates compiler/profile identities and publishes a new raw
   snapshot plus final direct-source project. Do not hand-edit generated C#.
6. Run `python3 blink/tests/KestrelGuestExecution/run.py` with required
   `--delivery-receipt NEW_RECEIPT --delivery-sha256 NEW_SHA`,
   `--native-receipt blink/artifacts/kestrel-guest-musl/attempt-o5jvvf7t/receipt.json`
   and `--profile-receipt blink/artifacts/kestrel-native-profile/attempt-zphi57zq/receipt.json`.
   Require all five real requests and ordinary stop/cleanup before promoting.
7. Only after that succeeds, review/run the prepared KestrelWorkerInstances
   all-four matrix with the same receipt arguments: simultaneous private
   instances, real HTTP, restart, cooperative stop and idle deadline. The committed
   runner is currently source-only. Then run the actual ManagedConsumerDelivery
   solution/sample JIT and NativeAOT; it additionally requires `--guest` pointing
   to the real guest ELF above. Commit/update exact P5 gates, then stop; no P6.

No current tool rejection prevents this work. Historical restrictions and failed
attempts remain evidence, not invented permanent bans. Preserve the ordinary
test scope: no new custom fault injection or invalid/malformed ELF tests.
