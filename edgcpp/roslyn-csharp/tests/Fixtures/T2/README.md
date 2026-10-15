# T2 genuine fixtures

These application sources are genuine frozen-EDG export inputs. Files named
`native-oracle.c` or `native-*-oracle.c` belong only to independent reference
processes. Generated workloads never link those application sources or oracle
objects. The public reader is the emitter's only semantic input.

`../../run-t2-storage-callbacks.py` reproduces the currently exercised partial
storage/callback contracts. It either exports the three C sources with the frozen
producer or reuses immutable `.roslynobj` files through `--objects`. Every
export/build/run is serialized and measured. The receipt records source, object,
translator, build, provider and generated-file hashes, deterministic repeated
emission, native/JIT/NativeAOT outputs and runtime dependencies. This partial
receipt does not close T2 or the SQLite execution gate.

| Fixture | Application input | Independent observations |
|---|---|---|
| Storage | `storage/storage.c` | 32 semantic observations; actual emitted sizes, offsets, alignment, bitfield bytes, packed field bytes, array strides, cycles, embedded NUL and stable addresses after GC |
| Dynamic aggregate copy | `storage/aggregate-copy.c` | Runtime source copy, record assignment, exact self-copy through alias, partial local record-array zero-fill |
| Function addresses | `function-addresses/callbacks.c` | Address equality/distinction, casts, table and array storage, native echo, unknown native identity provider, managed/native reentry and exact event order |
| Internal variadics | `variadics/variadics.c` | Captured promotions, GP/SSE overflow, explicit cursor forwarding, independent `va_copy` cursors; unaccepted until generated execution passes |
| Addressed variadics | `variadics/addressed.c` | Canonical entry echo, direct/indirect calls, native integer and double overflow into fixed bridge; unaccepted until generated execution passes |
| C++ final linkage | `cpp-linkage/unit-a.cpp`, `unit-b.cpp` | Captured overloaded final names and distinct same-spelling TU-local functions; no source lookup or remangling |

The only native providers used by the partial storage/callback runner are:

- `storage/native-layout-adapter.c`: reads bytes, fields and pointer cycles from
  generated storage; it contains no application execution body.
- `function-addresses/native-adapters.c`: records test events, echoes addresses,
  invokes supplied callbacks and supplies a native identity callback. It contains
  no translated application's implementation.

`variadics/native-entry-adapter.c` is a separate candidate ABI adapter, excluded
from the accepted partial runner. Its concrete fixed prefix uses the C compiler's
`va_start`, synchronously calls a registered fixed-signature bridge and ends the
borrowed cursor. Its callback owns all application execution. The independent
native oracle registers an independently compiled source body only inside the
reference process. That reference bridge is never a generated provider.

The captured GNU Linux AMD64 `__va_list_tag` has size24, alignment8, unsigned
32-bit cursor offsets at bytes0/4 and pointer fields at bytes8/16. Public `va_*`
operands already contain the selected array decay. These facts must be validated
before using the SysV cursor adapter; names alone do not establish representation.
Acceptance still needs interleaved GP/SSE exhaustion, exhausted fixed-prefix
occupancy, independent copies and borrowed native cursors. Extended scalars,
vectors and aggregate native ABI classification remain unsupported.

The current partial emitter also rejects volatile/atomic storage, richer dynamic
initialization, native aggregate calls and value-used bitfield updates until their
separate execution contracts pass. Comma, conditional effects and right-to-left
effects require the typed value/place/effect planner and genuine sequencing
fixtures; string expressions alone do not establish that contract.
