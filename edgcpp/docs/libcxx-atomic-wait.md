# libc++ atomic wait/wake boundary

`backend/runtime/libcxx/abi_atomic.cpp` adapts LLVM libc++ 18.1.8 `src/atomic.cpp`
under its retained Apache-2.0 WITH LLVM-exception license. The contention table,
sequence counters, reference counts, hash selection and memory orders remain
the original implementation. Platform wait/wake functions are replaced, and
the private synchronization table uses a shared storage accessor; platform
headers and unused polling includes are removed. The portable target
uses an aligned 64-bit `__cxx_contention_t` regardless of the execution OS.

Two checked C imports call `LibcxxAtomicWait.Wait64` and `Wake64`. A third,
`Table`, supplies a lazily allocated, pinned 16-KiB table aligned to 64 bytes.
C++ assertions check the original entry size, alignment and both field offsets.
Unlike application globals, this private address-indexed synchronization table
must be shared across engine instances: otherwise a generic wait in one engine
would monitor a different sequence counter from a notification in another.
Zero-initialized storage preserves the original constexpr initializers. The BCL runtime
uses 256 fixed monitor buckets. Waiting reads the contention word with
`Interlocked.Read` while holding the same monitor used for notification, then
parks with `Monitor.Wait`. This closes the comparison/parking race. Waking uses
`PulseAll`, including for the original `notify_one` path: unrelated addresses
can share a bucket, so waking an arbitrary single thread could lose progress
for the notified address. Every awakened waiter checks its own word again.

The monitor buckets retain no addresses or owners and create no per-call buffers.
Global/TLS contention storage remains pinned; heap contention storage uses
native allocation. The caller must keep an atomic object alive while it is
being used, as required by C++. Normal generated entry/thread leases protect
the containing engine lifetime. No extra libc++ thread-service instance is
allocated just to use these two stateless imports. The fixed table is shared
across generated owners in one runtime assembly, allowing explicitly shared
native storage to synchronize between owners. Interoperation with independently
generated runtime assemblies remains a separate linking contract.

The standalone synthetic test passes JIT and Linux-x64 NativeAOT in
`artifacts/campaign/libcxx-atomic-wait-runtime-001/`: 3,072 per-address handshakes,
deliberately colliding hash buckets, and notification before waiting. Generated
imports pass four further static/instance JIT/NativeAOT checks in
`libcxx-atomic-wait-translated-002/`: 256 handshakes per mode/runtime, native and
pinned global storage, compacting GC, cross-owner notification and independent
owner globals. The adapted libc++ contention algorithm additionally passes
four checks in `libcxx-atomic-protocol-translated-002/`: 4,608 handshakes per
mode/runtime with generic hash collisions, cross-owner shared native addresses,
direct 64-bit monitoring, same-address notification to multiple waiters and GC.
This component is compiled as one explicitly combined translation unit;
separate-TU linking is still an open provider gate. The shared pinned table's
alignment, zero initialization and GC stability pass standalone JIT/NativeAOT
in `libcxx-atomic-wait-runtime-002/`. The complete libc++ atomic API is a separate acceptance step;
other operating systems/architectures have not yet been executed.
