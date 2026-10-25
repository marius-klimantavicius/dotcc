# Guest memory and bounded futex regressions

These normal syscall fixtures target three reviewed defects: anonymous
`MADV_DONTNEED` retaining written data, shared anonymous mmap reaching the host
backing failure after guest table mutation, and `FUTEX_WAKE(2)` broadcasting to
three waiters. They are static x86-64 assembly ELFs; no dynamic loader, CoreCLR,
malformed ELF corpus, resource exhaustion or custom fault injection is involved.

Run the native comparisons from the repository root:

```sh
python3 blink/tests/GuestMemoryFutex/run.py
python3 blink/tests/GuestMemoryFutex/run-queue.py
```

`run.py` verifies the cached upstream archive and extracts a private native
checkout below `blink/artifacts/guest-memory-futex/`. It builds unchanged native
Blink, observes exit 42 for both original discard and bounded-wake defects, then
uses `UpstreamGuestThreads.stage.adapt_memory_and_futex` to build the same
authored corrections with native Blink's original thread launch/exit machinery.
The immutable reference tree and shared managed delivery are untouched.

The cases compare ordinary Linux execution with the corrected native oracle:

- Discarding an anonymous private page clears it, rounds the length to pages,
  preserves neighboring pages, handles reserved/read-only/PROT_NONE mappings
  without requiring guest write access, and leaves permissions intact. Alignment,
  overflow, unknown advice and unmapped-hole errors are ordinary syscall tests.
  Linux's ENOMEM-with-mapped-portions-discarded behavior across a hole is checked.
- File-private DONTNEED explicitly returns ENOTSUP and preserves the private
  bytes in this profile; Linux's file reload behavior is independently checked.
  Unimplemented semantic advice, including DONTDUMP, returns EINVAL.
- Shared anonymous mmap returns ENOTSUP before a MAP_FIXED replacement can
  remove an existing private mapping. Repeated rejection preserves its bytes,
  and subsequent private allocation/write/unmap succeeds. Native Linux instead
  supports shared mappings, so it has a separate expected result.
- Three child threads exercise back-to-back wake counts 2, 1 and INT_MAX, then
  join via ordinary clear-TID waits. Changed-value EAGAIN, an ordinary timed
  wait, and deregistration after timeout are checked before spawning them.
  Ten corrected native runs are required.
  The existing GuestThreads TLS/shared-memory/clear-TID fixture also runs.

The profile retains its previous `FUTEX_WAKE(0) == 0` behavior. This host Linux
kernel wakes one waiting thread for count zero, so that limit is tested only
with the profile fixture, not claimed as a matching native Linux comparison.

`run-queue.py` compiles the actual authored `futex-wake.inc` with upstream Bus,
Futex and Dll definitions, natively and through a fresh focused C# translation.
JIT and NativeAOT execute selections 2+1+0, 1+1+1+0, and INT_MAX, with all three
records deliberately still registered. This deterministically checks that a
second wake cannot count a selected but not yet dequeued waiter again. It also
checks cleanup and queue reuse. This is a helper test, not execution of a guest
through a newly translated full Blink product.

## Authored implementation and staging

`src/UpstreamGuestThreads/memory-advice.inc` walks guest page tables without
faulting in pages. The conventional hints 0–3 validate their ranges; advice 4
discards supported anonymous private pages. The implementation checks backing
before mutation, preserves mapping permissions and VSS, waits for existing page
locks, replaces committed PTEs with reservations, retires cleared backing through
upstream `FreePage`, updates committed/reserved/RSS counts, and invalidates TLB
and executable caches through upstream mechanisms. File/host-mapped backing and
huge guest PTEs remain unsupported. The behavior follows the supported subset of
[Linux madvise semantics](https://man7.org/linux/man-pages/man2/madvise.2.html).

`futex-wake.inc` and `futex-wait.inc` retain the upstream bus-lock then futex-lock
order, per-address Futex allocation, page lookup, signal checkpoints and timed
polling. Individual queue records carry a selected bit. Host broadcasts notify
all host waiters, while only selected guest waiters complete because of that
notification. Unselected waiters repark against the actual deadline. Records
are heap-owned so a terminal signal unwind cannot leave stack addresses in the
queue; the existing exported ClearChildTid cleanup also removes any records
belonging to that Machine, including when its clear-TID address is zero. The
managed thread owner is unchanged and must continue invoking that cleanup.

`stage.py` records current snippet identities in frozen inputs and incorporates
them into its checked staged patch. No historical compatibility hash gates an
ordinary authored snippet. The only downstream adjustment is the manual
UpstreamMremap predecessor fingerprint and patch hunk offset; its function body
and unsupported semantics are unchanged.

## Fresh managed guest gate

The coordinator must regenerate the threaded product's `blink/syscall.c` and
`blink/memorymalloc.c` producers, relink and complete the normal clean delivery
first. A stale DLL build or object-only relink cannot incorporate these C
changes. The shared product generation/build is deliberately not run here.

After the producer completes:

```sh
python3 blink/tests/GuestMemoryFutex/run-managed.py \
  --clean-delivery <clean-delivery-pointer.json> \
  --native-receipt <passing-run.py-attempt>/receipt.json \
  --form processed --aot
```

Use `--form raw --aot` separately for raw JIT/NativeAOT. The pointer must be
accepted by `campaign_delivery.load_clean_delivery`: a completed strict fresh
threaded delivery without reused objects. The runner also checks the actual
producer C inputs contain today's authored snippets, verifies native fixture
and source identities, and builds only a private copy of the delivery/owner.
Each ELF runs in a fresh consumer process. The consumer requires normal guest
exit, joined workers, released memory and drained descriptors/operations.

Observed on Linux x64 on 2026-10-25:

- Native Linux/original Blink/corrected Blink: passed at
  `blink/artifacts/guest-memory-futex/attempt-iib9x1l9/receipt.json`.
- Focused queue native, translated JIT and translated NativeAOT: passed at
  `blink/generated/guest-memory-futex-queue/attempt-nuogff_5/receipt.json`.
- Full stop/runtime/thread/mremap staging replay and authored-input receipt
  recording: passed at `blink/artifacts/guest-memory-futex/attempt-cjbbc0qh/`.
- New managed consumer source: compile-only check against the existing DLL API
  at `blink/generated/guest-memory-futex-managed/compile-only-dhvfjp23/`;
  no full-product guest result is claimed for that compile-only attempt.
  Windows, broad Linux compatibility and performance remain unqualified.

Earlier failed harness attempts are retained in the same artifact roots. The
first native futex fixture assumed zero-count Linux wake matched the profile;
the native trace disproved that assumption, and the fixtures now distinguish
it. Focused queue attempts also retain missing test list initialization,
project-name and PTE-helper build-input failures; the passing receipt above is
the corrected run.

## Fresh shared-product result

Strict shared verification passed at
`artifacts/campaign/20261025-042309-dda77155/receipt.json`: all 108 selected
objects were freshly emitted with zero reuse, and both delivery forms built.
The actual memory, shared-mapping rejection and bounded-wake guests then passed
raw and processed JIT/NativeAOT (12 managed executions), with joined workers,
released backing and drained I/O:

- Processed: `generated/guest-memory-futex-managed/attempt-c6ug2_kc/receipt.json`.
- Raw: `generated/guest-memory-futex-managed/attempt-l0lx1bez/receipt.json`.

These runs use the corrected current C producers and the native receipt above;
they supersede the compile-only limitation for this finite regression scope.
They do not qualify unsupported shared mappings or general Linux compatibility.
