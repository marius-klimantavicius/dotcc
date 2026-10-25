# Futex bitset regression

`fixture.S` is an ordinary Linux x86-64 threaded program. Its default combined
path checks absolute realtime/monotonic deadlines, selective masks, ordinary
WAIT/WAKE compatibility, joins through clear-child-TID, and a queued waiter
whose word changes without a wake. The changed-word waiter must time out.

Run the complete Linux/private-native oracle with:

```sh
python3 -B blink/tests/GuestFutexBitset/run.py
```

This builds pinned upstream Blink with the authored futex snippets in a private
artifact directory using `make -j2`. To reuse a previously built native binary:

```sh
python3 -B blink/tests/GuestFutexBitset/run.py --native-blink /absolute/path/to/blink
```

The supplied binary's build provenance is external; the receipt pins its bytes.
Both modes still assemble and run the full combined fixture five times, the
existing ordinary wait profile, TLS/thread fixture, and native queue check.
`--isolated-phases` is supplementary diagnosis only and never establishes a
combined-fixture result. Every nonzero functional exit keeps the receipt failed.
Neither runner qualifies managed guest execution.

## Pinned native emulator limitation

Round 6 demonstrated a separate bug in untouched upstream commit
`f006a4fc6f9b8de9272504fdff0dbbe5ce5dc580` configured with threads enabled,
JIT disabled, and `NOLINEAR`. `TrackHostPage` reallocates `g_hostpages.p` without
synchronizing readers in `FindHostPage`. A child first-touching its stack or BSS
can move and free this table while its parent walks the old table. The parent
then faults in `FindPageTableEntry` while polling an ordinary shared word.

The original bitset fixture reaches this fault before the changed-word child's
futex wait. It also occurs in a separate process with no preceding clone round;
repeated thread creation is not required. No fixture ABI defect or authored
futex cause was demonstrated for this SIGSEGV.

`native-clone-memory.S` reduces the failure to one Linux clone, child BSS stores,
and parent polling with sched_yield. It has no futex syscalls. Reproduce with an
existing stock or authored native Blink binary:

```sh
python3 -B blink/tests/GuestFutexBitset/run-native-clone-memory.py \
  --blink /absolute/path/to/blink --runs 30
```

This runner builds only the guest ELF, retains every Linux/native run and binary
hash, and kills/reaps each process group on timeout. It exits nonzero when native
fails, including when `native_limitation_reproduced` is true. It never turns the
known native failure into a passing regression or substitutes this reproducer
for the complete bitset fixture.

Retained evidence under `blink/artifacts/coreclr-coordination/`:

- `futex-r6-lqml05sj/receipt.json`: existing-binary replay, pristine stock archive
  configure/build (`make -j2`), binary/source identities, and independent replays.
- `futex-r6-lqml05sj/realloc-interleave.log`: original changed-word guest; child
  stopped after realloc and before table-pointer publication, parent reads the
  freed table. Only debugger scheduling was used; guest memory was unchanged.
- `futex-r6-lqml05sj/stock-realloc-interleave.log`: same causal interleaving in
  untouched upstream with the futex-free guest.
- `native-clone-memory-6u9kmw7i/receipt.json`: Linux 30/30 passed; stock native
  12/30 SIGSEGV, overall failed.
- `futex-bitset-rkg2_dwk/receipt.json` and `futex-bitset-mrpkt918/receipt.json`:
  original complete and isolated native failures, retained unchanged.

A later passing batch is a result for that batch; it does not resolve this
intermittent native limitation. The production futex snippets and default
combined fixture remain unchanged by round 6. Coordinator-owned fresh managed
builds and guest validation remain necessary.

## Public syscall consumers

`run-managed.py` consumes a strict fresh producer and runs private public API
consumers in raw/processed and `InProcess`/`SeparateProcess` configurations.
`--aot` additionally publishes the host and its normal worker with NativeAOT;
all guest fixtures remain ordinary Linux x86-64 executables.

Optional native receipts add the guarded signal stack, dynamic-libc process-map,
dynamic-libc CPU identity and user segment-read fixtures. The dynamic fixtures import their exact
retained libc and interpreter bytes into the guest image. They do not mount a
host dependency directory. The runner verifies native output, receipt and ELF
hashes, freezes source and deployment closures, and checks normal guest exit,
run/machine disposal and worker termination in every selected case.

```sh
python3 -B blink/tests/GuestFutexBitset/run-managed.py \
  --clean-delivery blink/artifacts/campaign/current-threaded.json --aot --runs 1 \
  --signal-stack-receipt PATH-TO-SIGNAL-RECEIPT \
  --process-maps-receipt PATH-TO-MAPS-RECEIPT \
  --getcpu-receipt PATH-TO-GETCPU-RECEIPT \
  --segment-read-receipt PATH-TO-SEGMENT-RECEIPT
```

That requests 40 public cases. It qualifies only these syscall/CPU fixtures, not
CoreCLR or an HTTP service. `--prepare-only` verifies and retains the guest
inputs without compiling a managed consumer or running a guest in Blink.
