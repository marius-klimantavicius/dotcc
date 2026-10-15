# Bounded storage and callback execution

T2 remains open. This checkpoint proves genuine single-TU storage and
scalar/pointer Cdecl callback behavior in JIT and NativeAOT. It uses the frozen
producer and public reader; native adapters contain observation or ABI forwarding
only, never application function bodies.

The [immutable report](../acceptance/storage-callbacks/report.json) passes all
46 phases. Its SHA256 is
`56d93baaf5b696c0d1a78976bc1d62991eca90d69ebcc13a1d8e31cd5094b2f5`.
The original artifact directory is
`/mnt/storage/roslynobj-csharp-20261015/t2/storage-callback-reproduction`.
All 37 implementation inputs were frozen for the entire run. Source, object,
producer, generated files, build artifacts and provider hashes are in the receipt.
Repeated emission produces identical files. The relevant
[T1 regression](../acceptance/storage-callbacks/t1-regression.json) passes all
23 phases with native/JIT/NativeAOT results 66/38.

| Fixture | Native/JIT/NativeAOT | Additional executed proof |
| --- | --- | --- |
| storage | 147423 | All 32 native observations; actual emitted record sizes/offsets, address alignment, packed/bitfield bytes through native observers, array stride, exact embedded-NUL bytes, cyclic/one-past fixups and pinned address stability after GC |
| aggregate-copy | 45 | Dynamic aggregate copy, local partial-initialization zero fill and self-copy |
| callbacks | 751 | Exact event sequence 21,11,21,31,21,11,41; canonical repeated addresses, pointer/integer round trip, pointers in record/array/global, native echo/reentry and unknown provider branch |

Storage uses stable aligned pinned owners, captured layout, allocation before
constant writes and address fixups. Address.ByteOffset is already the total
captured displacement; subobject paths are not added again. Bitfields use the
captured byte/bit coordinates, width and signedness. Local partial aggregate
initialization clears the complete selected storage first. Application functions
remain ordinary C# methods. Address-exposed functions have concrete static
UnmanagedCallersOnly wrappers; known canonical addresses dispatch to managed
bodies and unknown addresses use the explicitly tested native branch.

The callback import audit contains exactly seven bindings to the test adapter:
`native_reset_events`, `native_mark_event`, `native_event_count`,
`native_event_at`, `native_echo_callback`, `native_invoke_callback`, and
`native_callback_provider`. The storage observer is used only by the physical
checker. Generated workload dependencies contain only Generated and
RoslynObj.CSharp.Runtime, with no reader/SQLite dependency. Provider source and
binary hashes, generated import reports and NativeAOT system dependencies are
recorded. No native SQLite provider is involved.

Each phase freezes 2 GiB RSS and 64 MiB log budgets; ordinary phases allow 120
seconds and NativeAOT publishes 300 seconds. The receipt records elapsed time,
waited process high-water RSS and sampled live process-tree RSS separately.
Sampling can miss short peaks and double-count shared pages; tiny sampled values
are not claimed as runtime peak memory. These small fixtures are not a scale gate.

Reproduce with an unused artifact directory:

```sh
python3 edgcpp/roslyn-csharp/tests/run-t2-storage-callbacks.py \
  --objects /mnt/storage/roslynobj-csharp-20261015/t2/export \
  --artifacts /mnt/storage/roslynobj-csharp-20261015/t2/NEW-ATTEMPT
```

The driver requires a complete source freeze, including new files compiled by
default project globs. Native reference processes are separate from generated
workloads. The fixture-only harness prints a selected result; it is not production
entry semantics.

Remaining limits include generated multi-unit execution, native aggregate-by-value
ABI, hidden ABI roles, general scalar ABI alignment validation, variadics,
volatile/atomic storage, TLS/dynamic global initialization, broader floating
behavior, inheritance/EH and effectful sequencing. Native callbacks do not promise
C++ exception propagation across unmanaged boundaries. Typed expression planning
and SysV cursor primitives compile in this checkpoint but are unintegrated and
unaccepted; existing effect guards remain. No SQLite translation or SQL execution
is claimed.
