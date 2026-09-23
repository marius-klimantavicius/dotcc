# Public machine API qualification (P6, in progress)

`guest.c` is an authored valid static Linux x86-64 guest for normal console,
argv/environment/cwd, file and lifecycle scenarios. It uses real guest syscalls
and also runs natively; it does not substitute for the pinned Kestrel guest.

Source preparation is not an execution pass. The eventual receipt must bind
this fixture, the native control, current product/compiler/host sources, both
execution modes and actual JIT/NativeAOT consumers. Windows is not qualified by
Linux runs. Filesystem path/reparse checks do not claim atomic containment
against hostile concurrent mutation by another host process.

Run only after a fresh public `instance-v1` delivery passes and the coordinator
releases the serial build/execution slot:

```sh
python3 blink/tests/MachineApi/run.py \
  --delivery-receipt /absolute/path/to/translation/receipt.json \
  --delivery-sha256 EXACT_SHA256
```

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
ELF, native producer and profile; it does not rebuild the guest:

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
Current source preparation makes no claim that these gates have passed.
