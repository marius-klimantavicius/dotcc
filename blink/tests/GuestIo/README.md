# Normal guest file syscall integration

Native and raw/optimized JIT/NativeAOT qualification pass at
`artifacts/guest-io/attempt-tnq5itfa/receipt.json` (SHA256
`15795abe9f6ead804b60eac9c3f996807f3d4f244c10da8515b583146f3c825c`).
All four forms match the ten-line native transcript and pass the private
descriptor/file checks and bounded retained-pool checks. The run retains 108
objects from canonical assembly `14c483263fd7e92b9522e1e414ceea7ce68a770ca561c8a1b2e33928d7e6b86b`,
replacing only its frontend. Final input and execution binary identity checks
pass. Generated-code warnings remain in the build logs; no warnings were
suppressed and no shared compiler or host implementation changed.

The earlier native-only receipt is `artifacts/guest-io/attempt-q_w37qqy/receipt.json`
(SHA256 `e7c24b3d04e5a64e73e8f560cfdbf46ad8ad4243a9a6f142c221433ea17aefef`).
The initial compile-only const-array issue
is preserved in attempt-2zh_09gz; the fix makes the local array mutable to match
the upstream C API, without changing compiler flags. The harness exercises the
actual pinned `ExecuteInstruction` and Linux syscall dispatcher with the two
bytes `0f 05`. The frontend sets Linux x86-64 argument registers and inspects
the resulting state; it neither calls `Sys*` directly nor supplies syscall
results. Guest paths, payloads, iovecs and poll records occupy valid guest pages.

Each of two normal lifecycles performs:

- Create/truncate a relative private file, then transfer 128 KiB through loops
  accepting actual positive short counts. Read every byte back and check EOF.
- Duplicate its descriptor, seek through one reference and read through the
  other, proving the shared file cursor. Verify the upstream descriptors point
  to the actual `kFdCbHost` callback table.
- Execute `writev`/`readv` with a guest iovec table and payloads crossing page
  boundaries. Check every returned byte and the shared cursor.
- Poll a regular file for ordinary readable/writable readiness with timeout
  zero, then close one duplicate and continue reading through the survivor.
- Close both references, reopen and compare the complete file (including the
  vector overwrite), then close again and verify an empty upstream fd table,
  released guest mappings and cleared syscall temporary-allocation state.

The native adapter links the pinned native Blink archive and runs in a unique
empty artifact directory. The managed counterpart uses `InstanceIo`'s private
root, with no host-path fallback. Descriptor numbers and the number of short
transfers can differ; file bytes, lengths, shared-cursor relationships,
readiness flags and cleanup state must agree. No sockets, service loop, worker
API, invalid guest buffers, allocation exhaustion or forced host failures are
included. Valid-buffer tests do not prove rejection of invalid addresses.

The runner derives a complete library from an explicit canonical 109-object
assembly: 108 objects remain unchanged and only the authored frontend is
replaced. It uses the exact canonical header dependencies and frozen Host and
bridge sources, not current mutable host files. Original native inputs, source
templates, producer objects, compiler/postprocessor, copied Host sources,
logs and executed binaries are identified and checked for changes. Raw output
is retained independently of ordinary semantic postprocessing. The consumer
roots the complete derived library for NativeAOT.

The managed consumer forces compacting GC before execution, checks that only
three standard descriptors remain afterward, and verifies the resulting private
file length. The two upstream cycles share one owner; retained HostMemory pool
bytes and mapping count must stay equal after both cycles. Final disposal is
invoked after upstream state is discarded. No translated call observes the
disposed owner, so post-disposal zero counters are not claimed.

Reproduce under the campaign's serialized build schedule:

```sh
python3 blink/tests/GuestIo/run.py --native-only \
  --assembly-receipt blink/artifacts/core/objects/<qualified-key>/receipt.json
python3 blink/tests/GuestIo/run.py \
  --assembly-receipt blink/artifacts/core/objects/<qualified-key>/receipt.json
```

Each attempt supplies its own `TMPDIR`, native working directory, generated
projects and artifacts under `generated/guest-io/` and `artifacts/guest-io/`.
The full command reruns native, then raw/optimized JIT/NativeAOT, requiring the
same ten-line native transcript. It does not rebuild the shared compiler.
