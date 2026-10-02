# Packed bitfield emission

Generated ordinary bitfield properties now assemble and update their backing
bytes directly instead of looping over individual bits. A field uses at most
nine byte accesses even when a 64-bit value starts at a nonzero bit offset.
Writes mask only the affected bits of each byte, preserving adjacent fields.
Reads mask the declared width and apply the existing signed extension.

The emitted accesses never widen beyond the field's covered bytes, so packed
objects, unaligned fields and fields adjacent to an object boundary do not
require overreads, alignment assumptions or temporary allocations. This is an
ordinary-field optimization; volatile bitfield semantics remain a separate
coverage gate and are not inferred from this change.

`backend/tests/packed_bitfields.cpp` compares 142 signed/unsigned layouts,
widths 1 through 64, and every starting bit for the 64-bit boundary case. Each
layout uses 32 values, independent per-bit expected bytes, neighboring bit and
byte sentinels, signed reads and object copies (4,544 cases per execution).
The unchanged EDG C backend oracle plus static/instance JIT/NativeAOT pass five
checks in `artifacts/campaign/packed-bitfields-001/results.json`.
