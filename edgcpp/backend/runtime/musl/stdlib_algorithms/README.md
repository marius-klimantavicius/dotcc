# Portable integer and search/sort algorithms

Pinned musl 1.2.6 contributes eleven public functions: abs/labs/llabs/imaxabs,
div/ldiv/lldiv/imaxdiv, bsearch, qsort and qsort_r. Smoothsort uses O(1) scratch
space, has O(n log n) worst-case comparison complexity, and needs no allocation.
The portable qsort_r ABI is comparator(a,b,context), with context as the final
function argument. It is never bound to the platform C library's qsort_r.

The original sort algorithm and its Lynn Ochs MIT notice remain intact. The
atomic.h adaptation retains only musl's generic count-trailing-zero helpers;
no atomic operations or platform assembly are needed. Other adaptations add
explicit C linkage/internal declarations, rename the C++ keyword `try`, cast
void* explicitly, and replace the public weak alias with an ordinary wrapper.

The sources use existing translated managed function pointers and explicit
owner propagation, including function-pointer/void* round trips in musl's
ordinary qsort adapter. Division by zero, unrepresentable quotient, and abs of
a type's minimum value remain C undefined behavior, not supported extensions.

`source.json`, originals/ and patches/ retain exact provenance. Regenerate using
backend/tests/vendor_musl_stdlib_algorithms.py. The focused verifier retains
native original/adapted and all generated artifacts; see docs/libc-stdlib-algorithms.md.
