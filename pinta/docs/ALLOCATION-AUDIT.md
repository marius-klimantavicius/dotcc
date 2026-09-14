# Direct core allocation audit

Read-only review of the pinned 29-unit manifest found 21 direct calls to
`pinta_core_alloc`, excluding its definition. Seventeen immediately check for a
NULL result before initialization. `decimal.c:842` returns the allocation result
unchanged; the ordinary `pinta_lib_decimal_alloc` wrapper checks it. Three
allocation sites initialize a potentially NULL object:

| Original location | Function | First unchecked use |
| --- | --- | --- |
| `string.c:548` | `pinta_string_alloc_object_value` | `pinta_string_set_length` |
| `string.c:1950` | `pinta_char_alloc_object_value` (non-ASCII path) | `pinta_char_set_value` |
| `weak.c:124` | `pinta_weak_alloc_object_value` | `pinta_weak_set_target` |

The matching public `pinta_lib_*` wrappers already translate a NULL allocation
result into `PINTA_EXCEPTION_OUT_OF_MEMORY`. Adding an immediate NULL return at
these three sites would preserve that existing contract; no arithmetic, object,
or GC algorithm change is required. These three NULL guards are approved and applied as two hash-checked staged
patches (`string-allocation-safety.patch` and `weak-allocation-safety.patch`).
The native probe `native-value-exhaustion.c` retains uncached integer objects in
a stable 128-entry native root frame until its 1,024-byte heap is exhausted,
then calls each affected public wrapper and requires status 4 and an unchanged
NULL result. Run `native-boundaries.py --value-exhaustion-only`. Native execution
retained 41 objects and passed all three status/result checks. The separate
managed consumer repeated those checks successfully in raw/optimized JIT and
NativeAOT, in both release and diagnostic profiles. Receipts are under
`artifacts/native-boundaries-corrected` and `artifacts/managed`.

This bounded audit does not qualify all allocation sizing arithmetic or all
indirect allocation callers. Creation-time native-arena allocations are reviewed
and exercised separately in `NATIVE-VALIDATION.md`.

`pinta_lib_string_alloc_value` retains supplied external character storage,
whereas `pinta_lib_string_alloc_copy` copies characters into Pinta-owned storage.
The callback-return oracle uses `alloc_copy` and checks relocation of both the
returned object and its UTF-16 payload across compaction, matching owning managed
`ReturnString` semantics.
