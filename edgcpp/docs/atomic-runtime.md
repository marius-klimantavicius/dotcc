# Translated atomic operations

The first atomic milestone implements EDG's GNU `__atomic_*` lowering through
`dot_gen_atomics.h` and `AtomicOps.cs`. It supports scalar widths 1, 2, 4, 8 and
16, generic object load/store/exchange/compare-exchange, arithmetic and bitwise
read-modify-write, flags, fences and dynamic lock-free queries. The legacy
`__sync_synchronize` fence is included; other legacy `__sync_*` operations and
C11 `_Atomic` qualified lvalue operations remain separate work.

The original C backend removes the leading size argument from generic atomic
calls before emitting GCC source builtins (`c_gen_be.c`,
`sfk_gnu_atomic_generic_function`). The C# runtime instead consumes that size.
Both paths preserve EDG's operation selection and argument evaluation.
EDG lowers C++ `bool` to target `char` and C `_Bool` to unsigned char; emitted
predicate values follow the actual lowered type rather than assuming a byte.

All supported memory orders currently execute with sequentially consistent
synchronization. Strong compare-exchange also implements weak compare-exchange,
which permits but does not require spurious failure. Compare-exchange compares
object representations, preserves the expected value on success, and updates
it with the observed representation on failure. Invalid order combinations are
rejected before touching storage.

Naturally aligned 1/2/4/8-byte objects use same-width .NET `Interlocked`
operations on x64 and Arm64. Larger or unaligned objects use a shared, striped
address lock; typed and generic operations select the same synchronization for
the same object. Generic exchange saves the old representation on the stack up
to 512 bytes and in temporary native storage above that threshold. Native
temporaries are freed in `finally`. Atomic storage remains in the caller's
native frame/heap or pinned global/TLS arena. Typed managed-host byrefs are
pinned throughout an operation, including lock acquisition.

These guarantees apply within one translated runtime assembly. Independently
generated runtime assemblies need a shared synchronization provider before
exchanging objects that use lock fallback. Native code cannot concurrently use
its own atomic implementation on such objects without an explicit adapter.
An atomic object must have its required alignment; overlapping atomics with
different object boundaries are not a supported synchronization contract.

EDG folds constant lock-free queries in the frontend: widths 1/2/4/8 may fold
to true, and `always_lock_free` for 16 folds to false. Runtime queries use width
and alignment. The executable platform matrix must validate consistency on
each supported architecture before broader platform claims. Current execution
evidence covers Linux x64; Arm64 paths require execution evidence.

## Evidence and remaining work

`backend/tests/atomic_builtins_verify.py` checks C and C++ input against the
original EDG C output compiled by GCC and an independent Clang compilation of
the original synthetic fixture. Clang does not recognize the size-suffixed GCC
builtins in EDG's emitted C, so that output is not modified to accommodate it.
The translated checks exercise static and independent instance owners, JIT and
NativeAOT, all scalar operations, 3/16/600-byte objects, and concurrent calls.
All 12 checks passed in `artifacts/campaign/atomic-builtins-003/`.

`backend/tests/atomic_runtime_verify.py` additionally verifies representation
comparisons (including NaNs and signed zero), wraparound, failed-CAS expected
updates, adjacent-byte guards, contention, unaligned storage, moving managed
GC, mixed typed/raw operations, and absence of torn generic object reads.
JIT and NativeAOT passed in `artifacts/campaign/atomic-runtime-003/`.

These results do not complete the atomic plan. Remaining work includes
standard-library atomic headers and wait/notify integration, C11 qualified
loads/stores, the remaining legacy builtins, complete platform execution,
cross-module synchronization, and measured optimization of common operations.
The current conservative ordering is correct for the tested operations but is
not a claim of optimal throughput for relaxed loads or contention.
