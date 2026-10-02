# Managed libc++ exception ABI

Status: implementation in progress. Member-pointer and extended scalar/pointer
EH acceptance is complete; persistent exception handles and the boundary/count
work below are not yet accepted.

The provider uses the pinned LLVM 18.1.8 public declarations and an adapted
`src/support/runtime/exception_pointer_cxxabi.ipp`; only lifetime-reference hooks
gain the holder address, preserving public layout and retain-before-release order. The adapter is
`backend/runtime/libcxx/abi_exceptions.cpp`; its include paths must contain the
acquisition's configuration, libc++ headers and `src` directory, followed by
portable C headers. It must not include the EDG bootstrap exception provider.
Base exception classes, allocation exceptions and public `type_info` definitions
belong to the separate `abi_core.cpp` provider.

## Payload ownership

A native payload owner contains source type identity, aligned bytes, completion
state, destructor and originating-owner lease. A CLR exception wrapper owns an
active throw reference. Bare C++ rethrow retains that wrapper while the previous
handler releases its reference. `exception_ptr` retains the payload independently
and creates a fresh CLR wrapper for each rethrow. This permits concurrent C++
rethrows and independently idempotent managed-host `IDisposable` boundaries.

The final normal reference release runs the translated destructor once and
releases the native block and originating-owner lease in `finally`, including
when destruction itself throws. Abandoning a CLR wrapper on the finalizer thread
only drops its active references. If a persistent handle survives, it still owns
normal final destruction. If abandonment removes the final reference, only raw
storage is released; translated user code never runs on the finalizer thread.

The dependent-exception implementation uses the aligned payload address as its
opaque token. A private word before the payload holds a normal, unpinned
`GCHandle` to its managed owner. This supplies constant-time lookup without
allocating during `current_exception` or handle copies. Allocation/free must
cover exactly one native block and one handle, with no strong back-reference
from payload owner to CLR throw wrapper.

The four libc++ dependent-exception hooks are:

| Hook | Required behavior |
| --- | --- |
| `__cxa_current_primary_exception` | Retain the active C++ payload and return its token; zero outside a C++ handler or for a foreign CLR exception. |
| `__cxa_increment_exception_refcount` | Retain a nonzero token; zero is a no-op. |
| `__cxa_decrement_exception_refcount` | Release a nonzero token; destroy exactly on the final normal reference. |
| `__cxa_rethrow_primary_exception` | Create a fresh managed throw wrapper retaining the payload; zero returns so libc++ calls `terminate`. |

Pinned libc++ also selects its optimized `make_exception_ptr` implementation.
This needs `__cxa_allocate_exception`, `__cxa_init_primary_exception` and
`__cxa_free_exception`, including placement-constructor failure and the fallback
capture of that constructor's exception. Allocation has a size-only interface;
the target must precompute an alignment sufficient for every supported type
that can reach the allocator in this generated program. It must not silently
misalign over-aligned payloads or pretend this ABI can cross separately generated
assemblies without an adapter.

## Uncaught markers and termination

EDG's `lib_src/throw.c` links its throw-processing marker at
`__exception_started`, after evaluation and before the exception copy. It removes
the marker at `__exception_caught`, after catch-parameter initialization. Its
`eh_util.c::uncaught_exceptions` counts those markers. The managed implementation
must preserve those points, including nested throws and bare/persistent rethrows.

An exported boundary must finish translated unwinding before handing a carrier
to a managed host. It then marks the throw as handled by that boundary. A carrier
propagated from a host callback back into translated frames must regain its
uncaught marker during CLR first-pass search, before translated cleanup executes.
Managed tests must cover host catch/dispose/rethrow, nested callback entry,
repeated rethrows and transfer through persistent handles across threads.

Termination handlers are independent per generated owner. Explicit termination,
implicit termination and a returning/throwing handler must all terminate the
process. EDG reports zero uncaught exceptions when termination was initiated by
its runtime; preserve that observable behavior where the original runtime is the
oracle. Do not replace handler execution with an unconditional early FailFast.

The existing foreign/uncaught-boundary policy remains an explicit open gate until
these tests pass. Other open gates include dynamic exception specifications,
separate-assembly identity/ownership and complete libc++ integration.

## Owner-local handles and planned shutdown classification

Persistent-handle shutdown acceptance is still pending. The current storage-only
lease guard correctly rejects escaped carriers but also rejects a global
`exception_ptr` before its own destructor can release it. It must be replaced by
separate externally retained-reference and payload-liveness accounting before
owner-local global handles are supported.

An internal holder belongs to the originating owner's global arena, TLS arena,
tracked C/C++ heap, or another payload allocated by that owner. The last region
matters for `nested_exception`: its captured handle is physically inside an
exception payload. A holder in a different owner's region retains the original
owner externally. Local stack holders conservatively retain externally until
scope cleanup, which finishes before shutdown's active-call wait completes.
Raw host pointers into owner storage borrow that owner's lifetime; they do not
implicitly turn contained C++ objects into independently owned managed handles.

The adapter classifies holder addresses in the out-of-line copy constructor,
assignment and destructor, plus current/native capture. Pinned libc++ 18.1.8's
non-Microsoft `exception_ptr` has no inline move constructor, move assignment or
swap that bypasses these operations. Its inline default/null constructors create
empty handles; comparisons and bool conversion do not transfer ownership.
Generic move/swap, container relocation and enclosing nontrivial object copies
therefore use the copy hooks. Acceptance must include stack-to-global and
heap/member transfers, global-to-stack copies, swap and self-assignment rather
than relying only on simple handle copies.

Shutdown must reject escaped wrappers and externally retained handles before
running global destructors, preserving globals needed by later payload
destruction. Normal global/TLS/container teardown then releases internal handles.
Handles left in leaked owner heap objects are abandoned with that owner's raw
heap bytes; this matches the existing heap-owner contract, which does not invent
C++ destructor calls for leaked objects. Abandonment never runs translated
payload destructors on finalizer threads. A payload ledger must account for
internal payload-to-payload references and must release each native block and
GCHandle exactly once.

## Focused primary-handle bring-up

`backend/tests/libcxx/exception_handles.cpp` tests local current/copy/rethrow,
mutable shared payloads, `make_exception_ptr`, throwing placement copies,
`nested_exception`, 64-byte payload alignment and member-pointer payloads. Its
synthetic deleting-destructor adapter only supplies `operator delete` via libc
`free`; it is not acceptance of original libc++ `new.cpp` or a substitute for
that separate provider test.

Installed native GCC/Clang runtimes fail the fixture's 64-byte exception-address
assertion; Clang also diagnoses that its target runtime supports only 16-byte
exception alignment. The direct source semantic oracle disables only that
alignment assertion, while translated C# retains it. Native limitations and
initial failed traces are preserved in the campaign directories, not presented
as successful generated-backend acceptance.

The local libc++ primary-handle fixture now passes five checks: native GCC/Clang
source semantics and static/instance JIT/NativeAOT, including nested exceptions,
failed placement-copy capture, over-alignment and member-pointer payloads. See
`artifacts/campaign/libcxx-handles-20261003-003345/results.json`. The owner-aware ledger/holder classification has the dedicated acceptance
evidence below. Heap-region lookup is currently linear in owned allocations;
a nonallocating logarithmic range index remains an explicit efficiency task.

Owner-local/global/TLS/heap, normal cross-owner ownership, concurrent rethrow
wrappers and explicit host disposal pass fourteen focused checks in
`artifacts/campaign/exception-owners-20261003-004103/results.json`. This is not
complete EH acceptance: uncaught counts, implicit termination integration and
foreign containing-region abandonment remain separate gates.
