The owned record contract now accepts32- and64-byte source alignment, in
addition to the existing1–16-byte cases. Original declarations, physical size,
field offsets, packing evidence and full alignment remain in the object. The
reader rejects inconsistent original/physical layout and insufficient storage
alignment; this is not a relaxed interpretation of alignas.

Generated CLR value types use explicit offsets and at most16-byte Pack/alignment
markers. CLR packing is not a promise of32/64-byte stack or heap alignment.
Addressable locals and materialized values use the existing rounded stack
buffers; globals and TLS use rounded pointers into pinned arrays. Their source
alignment remains32/64, including nested records and array strides. By-value
parameters whose addresses are observed receive aligned local copies. Named
packed fields retain their lower observed alignment.

Focused native and stock-C O0/O2, managed JIT and NativeAOT tests cover globals,
TLS, locals, nested arrays, packed fields, return values and by-value arguments.
Four resealed objects reject weakened storage/original/physical alignment or
incorrect member offsets. Actual libc++ abi_atomic.cpp now exports, verifies
and generates; this does not by itself verify the runtime atomic-wait provider.
The current execution/storage profile remains bounded at64-byte alignment.
