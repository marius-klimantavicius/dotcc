# Portable C compatibility and atomic headers

The four compatibility headers `stdbool.h`, `stdalign.h`, `stdnoreturn.h` and
`iso646.h` use pinned musl 1.2.6 definitions. Original bytes, hashes, license and
patches are retained in `backend/runtime/musl/c_headers`. The only adaptation
removes `features.h` from stdnoreturn: the portable C11 frontend already supports
_Noreturn. C++ preserves its existing bool/alignment/alternative-token keywords.

[Native portable-header plus all eight generated C/C++ static/instance JIT/AOT
checks](../artifacts/campaign/libc-c-compat-20261003-060900/results.json) cover
booleans, every alternative token, global/local 32-byte alignment and a noreturn
function address. Linux x64 is the executed platform.

The public C11 `stdatomic.h` facade is now accepted. Musl supplies no stdatomic
header because it relies on the compiler's header. Our compiler facade uses GNU
atomic builtins and the existing AtomicOps runtime; it preserves _Atomic storage
typing. Generic scratch values use an unevaluated comma rvalue to remove atomic/cv
qualifiers. Pointer fetch arithmetic explicitly scales by element size; GNU
fetch builtins alone count bytes. Flag and fence wrappers have ordinary function
addresses. Lock-free macros conservatively report sometimes; atomic_is_lock_free
uses the runtime architecture/alignment check. The current lock-free primitive
architecture contract is x64/arm64; broader architecture atomic_flag guarantees
remain an explicit porting gate.

Original EDG C output preserves _Atomic loads, stores and compound assignments.
The C# backend now maps those operations to shared atomic byte/CAS helpers before
erasing the managed storage qualifier. Pointer RMW scales correctly, NaN/negative
zero comparisons preserve representation, and aggregate operations preserve exact
width. Generic public macros use contained GNU statement expressions emitted as
helpers with explicit caller-storage pointers, avoiding closure allocation.

The header is a C11/C17 API. Under C++17 it is a harmless compatibility include;
C++ atomic APIs remain supplied by the C++ library. This does not implement the
C++23 stdatomic namespace additions. Invalid source memory-order combinations do
not gain fabricated behavior. Current runtime orders conservatively use stronger
sequential consistency. Native atomic ABI interoperability and arbitrary runtime
assemblies sharing atomic storage are outside this managed program ABI.

Evidence:

- [Direct implicit operations, contention, adjacent-byte guards and zero integer allocation](../artifacts/campaign/libc-c11-implicit-20261003-062300/results.json).
- [Public APIs, aggregate/NaN CAS, pointer scaling, single evaluation, function addresses, weak-CAS retry, threads, owner isolation and zero public integer allocation](../artifacts/campaign/libc-stdatomic-translated-20261003-063400/results.json).
- [Four compatibility header original hashes and installed-copy checks](../artifacts/campaign/libc-c-headers-provenance-20261003-062800/results.json).

The public matrix contains two native/EDG-C oracles, four C static/instance
JIT/AOT configurations and two C++17 include checks. Linux x64 is the executed
JIT/AOT target; this is focused acceptance, not proof of every C11 memory-model
execution. Public C threads and the broader unfinished libc ledger remain open.
