# Integer ABI: immediate source-derived contract

Status: source/configuration inspection; no native execution or build performed.
Whole-source hashes are in `README.md`; a reproducible manifest will pin the
full inspected input set. Exact ABI measurements remain a supervisor gate.

The operative include path is `fe_common.h` → `basic_hdrs.h` → `basics.h` →
`defines.h` → `build/src/includes/cmake_defines.h` (`USE_CMAKE_DEFINES`), then
`host_envir.h` / `targ_def.h`. Target selection at runtime is a separate layer.

1. `cmake_defines.h:149` sets `INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=0`.
   `targ_def.h:1601–1679` therefore supplies the actual storage type.
2. `an_int_value_part` is `unsigned short` (`targ_def.h:1611`), with
   `MAX_UINT_VALUE_PART=0xffff`, `SIGN_BIT_INT_VALUE_PART=0x8000`.
3. `INT128_EXTENSIONS_ALLOWED=1` and the `TARG_SIZEOF_INT128` default 16 make
   `TARG_SIZEOF_LARGEST_INTEGER` and `MAX_SIZEOF_LARGEST_INTEGER` 16.
   `FIXED_POINT_ALLOWED=1` also participates in representation-size selection;
   it must not be silently removed. No unsuffixed generated override replaces
   the defaults in `targ_def.h:1278–1455`: maximum long accum size is 8,
   maximum long fract size is 4, so the selected fixed-point maximum is 8.
4. `INTEGER_VALUE_REPRESENTATION_SIZE` takes the maximum integer/fixed-point
   size. With the inspected 16-byte integer maximum and default fixed-point
   sizes ≤8, it is 16. `INTERNAL_TARG_CHAR_BIT=8`, host `CHAR_BIT=8` and a
   two-byte short yield `INT_VALUE_PARTS_PER_INTEGER_VALUE=8` and
   `BITS_IN_AN_INTEGER_VALUE=128`.
5. Storage is exactly `struct an_integer_value { unsigned short part[8]; }`.
   It is an embedded array with value-copy semantics, not a reference to shared
   mutable array storage. Derived Linux x86_64 size/alignment: 16/2; confirm
   by the supervisor's native layout probe. `part[0]` holds the most significant
   part (`sign_of`, comparison and signed/unsigned setters). This part order
   is independent of host byte endianness (recorded little-endian).
6. `a_host_large_integer=a_ptrdiff=ptrdiff_t` and
   `a_host_large_unsigned=sizeof_t=size_t` (`targ_def.h:1627–1631`,
   `basics.h:533–554`). Under the recorded LP64 host ABI these are signed and
   unsigned 64-bit integers. Generated scalar-representation type macros do
   not override these typedefs in this branch.
7. `USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=1` selects `a_boolean=bool` and
   `a_byte_boolean=bool` (`basics.h:213–217`), not `int`. Distinguish native
   storage from C# interop default bool marshaling; a fresh managed in-process
   port need not use P/Invoke.
8. Native integer promotion is material: a 16-bit part promotes to signed
   32-bit `int` in expressions unless an explicit cast changes it. The work
   types in multiplication, carry propagation and masking must follow each
   routine's casts. Do not replace them with a uniform arithmetic type.
9. Preserve explicit `signed_shift_right` in `const_ints.h:37–40` (unsigned
   host-large shift, conditional sign-fill mask, final signed cast), and the
   unchecked narrowing conversions observed in each routine. Native signed
   overflow or out-of-range shifts are unresolved platform-sensitive cases,
   not permission to repair or invent semantics.
10. Setters/read-modify-write operations take pointers to existing storage.
    Preserve `op_1==op_2` alias behavior and traversal direction. Reference
    setter overloads copy parts in ascending order; integer decomposition
    setters traverse from the last part downward. No allocation, normalization,
    canonicalization, diagnostics or cache may be introduced into leaf paths.

The independent source trace confirms the implementation worker's announced
`fixed ushort part[8]` / C# `long` / `ulong` / `bool` contract for this pinned
host profile. This is agreement about the source-defined representation, not
approval of managed implementation bytes (not reviewed by this research task).

Preserve post-mutation alias reads in add/subtract: their multipart overflow
expressions inspect `sign_of(*op_2)` after writing `op_1`'s parts. Caching the
second sign early would change same-storage behavior. Also preserve the private
division helper's ordered writes in the smaller-dividend branch; its quotient
can alias `orig_op_1`. Do not repair or reorder either observation.

The supervisor should confirm the exact chosen native target and record native
`sizeof`/`alignof`/offsets and arithmetic boundary behavior. The source-defined
128-bit storage remains required even when a strict C++20 runtime invocation
does not permit extension syntax.
