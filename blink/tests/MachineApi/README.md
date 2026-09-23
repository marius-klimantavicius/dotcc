# Public machine API qualification (P6 complete on Linux x64)

`guest.c` is an authored valid static Linux x86-64 guest for normal console,
argv/environment/cwd, file and lifecycle scenarios. It uses real guest syscalls
and also runs natively; it does not substitute for the pinned Kestrel guest.

The actual fixture passes all four JIT/NativeAOT × in-process/separate-process
forms at `artifacts/machine-api/attempt-b074atpe/receipt.json` (SHA-256
`5841d3b0e8d487e60a1dd685fd5fabf369c4a5bc6d65efef8b979cea343c2be7`).
All ten public cases and eleven native controls pass. Both process forms use
25 real workers; both in-process forms use none. The independent review verified
2,327 retained paths and all 50 worker exits. Current compiler/postprocessor
binaries were independently rechecked after the final sample run.

The actual Kestrel/public sample also passes all four forms at
`artifacts/machine-api-kestrel/attempt-cqqbx2eu/receipt.json` (SHA-256
`e29648060f862ae629d399510e8ec5ac96e99b07b2751c2e2998af80951c9ffa`):
12 Kestrel runs, 20 native HTTP comparisons and 12 binary console/live-folder
sample runs. Independent review verified 3,016 retained paths and normal cleanup.
Both gates consume delivery `translation/attempt-tfqtuor3`; generated instance
ownership, actual NativeAOT worker deployment and original authored references
are part of that recorded source closure. Windows is not qualified by
Linux runs. Filesystem path/reparse checks do not claim atomic containment
against hostile concurrent mutation by another host process.

Run only after a fresh public `instance-v1` delivery passes and the coordinator
releases the serial build/execution slot:

```sh
python3 blink/tests/MachineApi/run.py \
  --delivery-receipt /absolute/path/to/translation/receipt.json \
  --delivery-sha256 EXACT_SHA256
```

When other sessions rebuild shared tools, both runners accept `--producer-tools
/absolute/path/to/snapshot`. That directory must contain `compiler/` and
`postprocessor/` output closures. Every producer hash must still match the exact
delivery receipt; this changes the lookup location, not the binary identity
checks. The immutable tool closures replace the live producer source inventory,
which is not compiled by these consumer-only runs. All actual Blink compiled
sources and source membership remain frozen.

The runner first checks the delivery SHA, all 108 object calling conventions,
typed semantic reports, compiler identities, final/raw product manifests and
the actual consumer source/import closure. It builds this valid static ELF and
runs eleven finite Linux native witnesses for argv/env/cwd, file operations and
binary console/EOF. It then builds the ordinary solution and Probe project,
preserves the complete JIT consumer and automatically deployed worker, and
qualifies InProcess and SeparateProcess in separate attempt directories. AOT
publishes the actual consumer with the same deployment target and repeats both
modes. No worker path is supplied to either consumer. Native finite witnesses do
not substitute for the managed lifecycle, quota or stop assertions.

Owned-stream echo explicitly sets `LeaveOpen=false` and checks all three caller
streams close after preserving their bytes. A resource machine loads the ELF
from a read-only mount, leaving its private 16-byte storage quota free of ELF
import charges. Normal requests for 64 MiB anonymous mmap, additional descriptors
and a 64-byte file write must produce ENOMEM, EMFILE and ENOSPC under the explicit
32 MiB memory, eight-descriptor and 16-byte storage limits. Successful descriptor
numbers and the exact retained file prefix are checked. The same native requests
succeed without those private quotas; their outputs are intentionally different,
not treated as a same-limit native comparison.

The actual Kestrel sample has a separate final gate using its existing pinned
ELF, native producer and profile; it does not rebuild the guest. Original guest
build inputs are verified against their immutable producer archive, so changing
unrelated current repository package versions cannot redefine that old ELF:

```sh
python3 blink/tests/MachineApi/run-kestrel.py \
  --delivery-receipt /absolute/path/to/translation/receipt.json \
  --delivery-sha256 EXACT_SHA256 \
  --guest /absolute/path/to/KestrelService \
  --native-receipt /absolute/path/to/native/receipt.json \
  --profile-receipt /absolute/path/to/native-profile/receipt.json \
  --fixture-receipt /absolute/path/to/passing-machine-api/receipt.json \
  --fixture-sha256 EXACT_FIXTURE_RECEIPT_SHA256
```

This builds the ordinary ManagedConsumer solution/sample, uses app-local worker
discovery, and qualifies JIT/AOT in both modes. Each mode runs two overlapping
machines directly from a mounted ELF, HTTP stop, then same-machine restart and
cooperative stop. Five saved HTTP request/response pairs per mode are compared
against the pinned native profile. Requests match exactly; response comparison
permits only validated Date values and header order to vary.

The same runner requires the preceding generic fixture pass on the exact same
delivery. It reuses that pinned ELF and native oracle to qualify the actual
`--run` and `--run-process` examples in JIT/AOT: binary console with EOF, live
mounted file storage, and reading that file through a fresh sample machine.
It never rebuilds the fixture or Kestrel guest. The generic gate also checks
concurrent stop/run-disposal/machine-disposal and concurrent process kill calls.

Both runners retain command lines, exit codes, stdout/stderr, process-group
cleanup, copied sources, source membership, execution closures before/after,
and mode evidence. They stop on the first failure and leave a failed receipt.
The failed build/EOF/publish attempts remain preserved. Only documentation
changed after the final successful frozen-input review.
