# Variable-length array lowering

The C# configuration enables EDG's existing `LOWER_VARIABLE_LENGTH_ARRAYS`
pass. Source dimension evaluation, saved bounds, pointer strides, scope exits,
and C++ element destruction remain EDG's responsibility. The backend consumes
its `__vla_alloc`, `__vla_dealloc`, and `__vla_dealloc_eh` ABI, following
`lower_c99.c`, `lower_eh.c`, `lib_src/vla_alloc.c`, and `lib_src/throw.c`.

Array storage uses `NativeMemory.Alloc`, with a native linked allocation record
and a thread-local head pointer. No managed allocation accompanies an array.
Normal releases can pop through a scope's first array, matching the reference
runtime's coalesced releases. Every allocating translated activation records a
mark and restores it in `finally`; this avoids the reference runtime's stack
address heuristics. EDG cleanup runs first, including reverse element destruction
on partially constructed arrays. Partial-EH handles for lowered VLA variables
use the stored array address, rather than the address of its pointer slot.

The target's minimum alignment is 16 bytes; explicit power-of-two alignments up
to 4096 are carried from lowered variables. Zero-byte arrays receive distinct
addressable native storage. Negative byte counts, allocation overflow, and OOM
terminate deliberately. This does not add C `setjmp`/`longjmp` support.

`artifacts/campaign/vla-translated-003/results.json` records ten checks:
original EDG-generated C plus C and C++ static/instance JIT and Linux-x64
NativeAOT. Fixtures cover evaluated dimensions, typedefs, multidimensional
strides, saved `sizeof`, scopes, break/continue/goto, recursion, zero bounds,
16-byte alignment, normal class destruction, escaping exceptions, and partial
construction failure. Instance hosts verify the native allocation chain returns
to its initial mark. The unchanged demonstration C runtime is compiled with
its documented `MOST_STRICT_ALIGNMENT=16` configuration for the oracle.

`artifacts/campaign/vla-alignment-translated-001/results.json` adds five checks:
native C and static/instance JIT/NativeAOT with 64-, 256-, and 4096-byte arrays,
independent thread allocation chains, coalesced releases, zero-sized arrays,
and zero managed bytes for 2,000 warmed allocation/release groups per thread.
Other operating systems and architectures, allocation-failure injection, VLAs
containing library exception handles, and allocator performance profiling remain
separate verification gates. Earlier failing runs remain with their logs.
