# Integer utilities and search/sort

The selected musl 1.2.6 component supplies abs/labs/llabs/imaxabs,
div/ldiv/lldiv/imaxdiv, bsearch, qsort and qsort_r. Existing managed abs imports
remain available to `--libc dotcc`; selected source definitions take precedence
when supplied. No new managed runtime bindings are required.

The source archive, copyright and SHA256 pin are the same as the numeric and
wide components. Original files, deterministic source generator, replayable
patches and hashes are retained in `backend/runtime/musl/stdlib_algorithms`.
[Patch replay results](../artifacts/campaign/libc-stdlib-algorithms-patch-replay-20261003-041430/results.json)
cover all twelve source/header adaptations.

## Callback ABI and storage

Musl smoothsort uses fixed stack scratch: a 256-byte chunk buffer plus bounded
pointer/Leonardo-number arrays. It allocates neither managed nor native heap
scratch. Emitted local storage uses stackalloc; only global/TLS storage remains
pinned. Complexity is O(n log n) worst case and near linear for mostly sorted
input. Dotcc's current StdlibLib was consulted: its qsort uses insertion sort
and per-call NativeMemory scratch, while its instance overload explicitly
threads the owner through comparator calls. The selected implementation keeps
musl's algorithm and our existing explicit-owner function pointer convention.

`qsort_r` has the GNU/POSIX-style signature
`void qsort_r(void*, size_t, size_t, int (*)(const void*, const void*, void*), void*)`.
The comparator's context is last. This is a portable translated API; it does
not import any host qsort_r variant. Ordinary qsort retains musl's callback
adapter and function-pointer/void-pointer round trip. C++ exceptions can unwind
through the translated algorithm without a native call boundary.

EDG's C backend was consulted for function-pointer parameter/type handling
(c_gen_be.c's routine declaration path); no special callback lowering was
needed and no EDG change was made. The C++ source adjustments are limited to
explicit linkage/declarations, `try` identifier renaming, a void-pointer cast,
and replacing weak alias machinery with a public wrapper. Generic musl ctz
fallbacks are retained with their original zero-input behavior, without atomic
or OS dependencies. Division by zero, minimum-value/-1 division and minimum
signed absolute value are C undefined behavior and excluded from acceptance.

## Acceptance

Original and adapted native algorithms produce identical digests. All eight
C/C++ × static/instance × JIT/NativeAOT configurations pass:
[focused results](../artifacts/campaign/libc-stdlib-algorithms-20261003-041130/results.json).
Tests include integer boundaries and aggregate returns, exhaustive small signed
division, function addresses, empty inputs, repeated/missing binary-search keys,
ascending/reverse/equal/duplicate/organ-pipe/random patterns, record widths
1/3/8/257/1025, context and recursive comparator calls, owner isolation, and C++
callback exceptions. One initial synthetic comparison-budget assertion counted
recursive sorting on every comparison; the fixture now performs one deliberate
recursive sort per outer call. The original failure remains retained.

The tested architecture is Linux x64 under the portable LP64/binary80 profile.
No Windows/macOS execution or complete libc coverage is implied. Registration
and full-suite hooks are coordinated separately; see
[the integration contract](libc-selected-test-integration.md).
