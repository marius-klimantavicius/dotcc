# Guest thread churn and capacity

This separate normal static Linux x86-64 ELF creates and joins 96 sequential
children using the actual clone parent-TID publication and shared clear-TID futex
wake. The managed owner runs it with `maximumWorkers: 2`, including main. A second
case holds two children behind futex gates with a limit of three, requires the
next clone to return `EAGAIN`, releases the children and completes another 96
children. There is no custom fault injection or malformed ELF input.

Clear-TID is an exit handshake, preceding host Machine/binding cleanup. A later
clone may still return `EAGAIN` until cleanup, host-thread join and synchronization
resource disposal finish. The fixture retries that ordinary transient result with
one-millisecond guest nanosleeps, at most 5,000 retries per creation. Futex gates
control ready/release and clear-TID progress; scheduling does not depend on a fixed
sleep to make a child finish. Each managed owner has a 20-second deadline, a
30-second main-thread join bound and a 40-second outer process timeout.

Additional managed cases cover main `exit(7)` before a child `exit(0)` (the child
waits for main's clear-TID, all workers join, final ordinary exit status wins), cancellation at startup, cancellation at
the first published clone, and cancellation after both workers reach a futex
handshake. Every successful case verifies full thread result history, instruction
accounting, Machine release, worker joins, backing release, IO drain, and rejection
of a second `Run` on the same owner. Failed quiescence retains borrowed IO/stop
resources until the private process is terminated.

The runner first assembles and links the fixture in a new ignored artifact
directory and runs that exact ELF directly on native Linux. The native capacity
argument skips the managed-only cap rejection (Linux has no per-owner
`maximumWorkers` policy); it follows the same two-held-child/release/churn protocol.
Only the managed capacity case establishes `EAGAIN` and recovery. Native execution
is recorded separately and never substitutes for a managed run.

From the repository root, after the coordinator finishes a fresh strict shared
threaded delivery:

```bash
python3 blink/tests/GuestThreadChurn/run.py \
  --delivery-receipt /absolute/path/to/shared/receipt.json \
  --delivery-sha256 "$(sha256sum /absolute/path/to/shared/receipt.json | cut -d' ' -f1)" \
  --form processed --aot
```

Use `--form raw` for the raw delivery too. The runner requires the exact receipt
SHA and `load_clean_delivery` validation: completed strict delivery, all selected
objects freshly produced without reuse/relink, current staged sources, and matching
authored inputs. It snapshots the product source closure and current owner into a
private build tree. Builds/publishes never write the shared delivery. `--aot` runs
all six cases under JIT and NativeAOT; without it only JIT is qualified. The final
receipt pins the fixture, image, owner, selected sources, binaries and reports.

Independent native witness and explicit diagnostic use of an existing DLL:

```bash
python3 blink/tests/GuestThreadChurn/run.py --native-only
python3 blink/tests/GuestThreadChurn/run.py \
  --host-library blink/generated/TranslatedBlink/bin/Release/net10.0 --aot
```

The second command compiles today's authored owner against private copies of the
selected existing translated/host DLLs. Its receipt explicitly says
`scope: host-owner-only` and `fresh_delivery: false`, even when all tests pass.
It provides no qualification of newly staged C corrections. These focused tests
do not establish whole-product/consumer qualification or other host platforms.

## Fresh shared-product result

Both delivery forms passed all six scenarios in JIT and NativeAOT, 24 managed
executions, after strict shared producer
`artifacts/campaign/20261025-042309-dda77155/receipt.json` emitted all 108 selected
objects with zero reuse:

- Processed: `artifacts/guest-thread-churn/attempt-an0tf9ju/receipt.json`.
- Raw: `artifacts/guest-thread-churn/attempt-l2nz_6m_/receipt.json`.

Each form also passed the three native witnesses. The earlier host-owner-only
receipt `artifacts/guest-thread-churn/attempt-iahja6tr/receipt.json` used existing
DLLs and supplies no C-producer qualification. The exact old-owner baseline at
`artifacts/guest-thread-churn/baseline-o2s2ghcz/receipt.json` admitted one child,
then exhausted 5,000 ordinary `EAGAIN` retries and exited 13 on the same final ELF.
