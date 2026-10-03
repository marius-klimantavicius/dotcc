# Managed libc++ exception ABI

Status: persistent exception handles, owner classification, foreign-region
abandonment and allocation-free capture have focused JIT/NativeAOT acceptance.
Uncaught counts and managed escape markers also have focused acceptance.
Owner-aware termination and console/lifetime boundary routing also have focused
acceptance; this document does not claim complete EH coverage.

The provider uses the pinned LLVM 18.1.8 public declarations and an adapted
`src/support/runtime/exception_pointer_cxxabi.ipp`; only lifetime-reference hooks
gain the holder address, preserving public layout and retain-before-release order. The adapter is
`backend/runtime/libcxx/abi_exceptions.cpp`; its include paths must contain the
acquisition's configuration, libc++ headers and `src` directory, followed by
portable C headers. It must not include the EDG bootstrap exception provider.
Base exception and allocation exception definitions belong to `abi_core.cpp`.
The fixture also compiles original libc++ `src/typeinfo.cpp`, which supplies the
`type_info` key function and vtable.

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
A non-elided exception-object copy therefore observes count 1 in the original
EDG C runtime; installed native GCC/Clang runtimes report 0 for that same copy.
Catch-parameter copies observe 1 in all three. The backend follows EDG's explicit
marker; this difference is recorded in
`artifacts/campaign/uncaught-marker-audit-20261003-010916/results.json` and has
separate direct-source and C-lowered test expectations.

`current_exception` must skip a handler whose catch parameter is still being
constructed: it sees an enclosing fully entered handler, or returns null. Bare
rethrow follows EDG's active-handler lookup separately. The dedicated count and
managed-boundary fixtures pass nineteen checks in
`artifacts/campaign/exception-counts-20261003-013058/results.json`: three native/EDG
oracle checks and static/instance JIT/NativeAOT for ordinary counts, non-elided
copy markers, managed export/reinjection and public `Run` handoff.

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

Foreign CLR exceptions do not contribute to the C++ uncaught count and return
null through `current_exception`. Public exported methods and `Run` finish
translated cleanup before handing an exception to their managed host, then end
the uncaught marker. Ordinary translated function-pointer trampolines are not
host boundaries: translated callers can catch through them. Concurrent rethrow
uses fresh `exception_ptr` wrappers; a managed host must not concurrently reuse
a single escaped carrier.

The combined foreign-boundary matrix passes twenty static/instance
JIT/NativeAOT checks in
`artifacts/campaign/foreign-clr-boundaries-20261003-023012/results.json`.
It preserves the exact CLR object across repeated catch-all/bare rethrows,
restores an enclosing C++ `current_exception` after a nested foreign handler,
captures an empty `nested_exception` inside a foreign handler, and supports
allowed C++/`bad_exception` replacements through dynamic specifications.
Foreign `noexcept` escapes, secondary foreign throws during C++ cleanup,
empty nested rethrows and forbidden unexpected replacements invoke the active
owner's terminate handler with zero C++ count and no fabricated C++ payload.
The secondary-throw case terminates during first-pass search before further
destructor cleanup. These are managed interoperation assertions, not native
exception ABI compatibility claims.

Remaining gates include separate-assembly
identity/ownership and complete libc++ integration.

## Termination acceptance

`dot_gen_terminate.h` supplies a per-owner handler and per-thread recursion guard.
Runtime-initiated termination masks the uncaught count to zero, matching EDG;
explicit termination during unwinding preserves the current count. A runtime
termination handler can capture the triggering exception through
`current_exception`. A returning or throwing handler terminates the process,
and recursive termination aborts without repeatedly invoking the handler.

Noexcept violations unwind to their boundary before the handler runs. A second
exception escaping cleanup, throw-copy construction or catch-parameter copying
terminates during first-pass search. Array cleanup requires this same first-pass
rule for a secondary destructor exception, while its first failing destructor
unwinds normally. The original C array oracle is retained at
`artifacts/campaign/terminate-array-oracle-20261003-014527/results.json`.

Core handler behavior passes 58 checks (18 original C GCC/Clang checks and 40
static/instance JIT/NativeAOT checks), recorded in
`artifacts/campaign/exception-terminate-20261003-014814/progress.json`. That run
subsequently hit an unrelated libc import gap in the boundary fixture, so its
overall results file correctly remains failed.

Console/main handoff, first-pass worker termination, owner shutdown, secondary
array destruction and global initializer failure pass 20 further checks in
`artifacts/campaign/exception-terminate-boundaries-20261003-015047/results.json`.
The console entry point calls the owner termination adapter after `Run` has
finished C++ cleanup; managed-library callers retain the explicit host exception
boundary. Raw runtime invariants and storage-allocation failures still use
FailFast directly; they are not silently reclassified as C++ termination.

The focused fixture uses synthetic new/delete adapters and nonvariadic output;
it does not establish complete libc++ allocation or libc import coverage.
Initial `printf` and `getenv` translation failures remain retained evidence for
the independent libc import work.

## Legacy dynamic exception specifications

The backend follows `lower_eh.c`'s distinction between `noexcept` and legacy
`throw(T...)`: a dynamic specification matches the escaping type against its
permitted list before selecting `unexpected`. The function's ordinary cleanup
runs first. The unexpected handler sees the original exception as caught, can
rethrow it, or replace it with a permitted exception. A forbidden replacement
becomes `std::bad_exception` when that type (or its public base) is permitted;
otherwise the active owner's terminate handler runs. An empty `throw()` remains
a dynamic specification in the C++14 fixture. Handler return calls terminate.

`dot_gen_unexpected.h` emits the boundary and typed matchers. The libc++ provider
exposes `set_unexpected`, `get_unexpected` and `unexpected` only in the relevant
legacy/opt-in header modes. A private factory's typed `std::bad_exception*`
parameter supplies the exact public class identity; the backend does not guess
it from symbol spellings. Original and replacement catch frames release payload
references in `finally`, with first-pass termination for throwing destructors.

Six positive checks cover direct GCC/Clang, original EDG C GCC/Clang, and generated
static/instance JIT/NativeAOT. Cases include reference specifications, derived
objects, pointer qualification, data-member pointers, array/function adjustment,
nested specifications, replacement and `bad_exception` substitution:
`artifacts/campaign/dynamic-specs-20261003-022100/results.json`.
Twenty-eight fatal checks cover empty specifications, returning/forbidden
handlers, throwing original/replacement destructors, owner isolation and handler
`current_exception`, including the comparable original EDG C traces:
`artifacts/campaign/dynamic-specs-fatal-20261003-022320/results.json`.

The original generic EDG C runtime provides useful ordering evidence but does
not provide persistent `exception_ptr` ownership. Its throw-specification stack
entry has no original-payload cleanup action (`throw.c`, the pass-two
`ehsek_throw_spec` case). In the retained observational fixture, replacement of
an original class exception therefore does not invoke that original destructor.
Installed GCC/Clang invoke it. No EDG source is changed: the managed runtime
retains its explicit payload ownership contract and releases the original when
its final active/retained reference ends, as required for C++ exception-object
lifetime. Consequently the original-payload destructor case uses the managed
lifetime contract and direct C++ evidence, rather than asserting equivalence to
that generic runtime observation. Replacement-payload secondary-throw ordering
does match the EDG C oracle; native GCC/Clang differ in the permitted amount of
cleanup before termination. All observations are preserved in
`artifacts/campaign/dynamic-fatal-oracles-20261003-022032`.

Explicit persistent-handle retention of both original and replacement payloads,
including successful `bad_exception` substitution, passes five additional
direct GCC/Clang and static/instance JIT/NativeAOT checks in
`artifacts/campaign/dynamic-specs-lifetime-20261003-022606/results.json`.
The fixture verifies exactly one final destructor per payload, immediate cleanup
of an unretained original before the replacement catch, and rethrow of a saved
original after the replacement has been substituted with `bad_exception`.

## Owner-local handles and shutdown classification

Separate external-reference and payload-liveness counters allow owner-local
global handles to survive until their own destructors run, while escaped
carriers and cross-owner handles prevent premature shutdown.

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

The payload/wrapper refactor and native-header GCHandle storage pass the existing
five storage/host-ownership checks. Local libc++ handles pass five checks: native
GCC/Clang source semantics and static/instance JIT/NativeAOT, including nested
exceptions, failed placement-copy capture, over-alignment and member-pointer
payloads. Evidence:
`artifacts/campaign/libcxx-handles-20261003-003345/results.json`.

Owner-local/global/TLS/heap, normal cross-owner ownership, concurrent rethrow
wrappers and explicit host disposal pass fourteen focused checks in
`artifacts/campaign/exception-owners-20261003-004103/results.json`.

## Foreign regions and allocation behavior

The foreign-region follow-up records only handles physically inside another
owner's heap or exception payload. Region free/abandonment removes these records
and abandons their managed references without invoking translated destructors.
Ordinary own-owner and stack captures use counters rather than reference-record
allocations; the foreign-region side dictionary can grow. Bookkeeping OOM rolls
back the newly acquired payload reference, then terminates explicitly. Callback
execution is outside heap, reference-registry and owner locks. A cycle of live
cross-owner references causes deterministic Shutdown rejection until C++ code
explicitly breaks a reference; disposal never force-frees a live target.

TLS arenas are registered in an intrusive list. Capture scans that list without
allocating a `ThreadLocal.Values` snapshot; rare whole-owner shutdown still uses
the snapshot. Replacement/removal updates the registry, and lookup retains the
storage reference while reading its address. Heap-region lookup remains linear
in owned allocations; a nonallocating logarithmic range index is an explicit
efficiency task.

Foreign region cleanup/cycles, balanced bookkeeping OOM and 20,000 warmed
`current_exception`/copy/assignment iterations with **zero managed bytes** pass
ten checks across static/instance JIT/NativeAOT in
`artifacts/campaign/exception-capture-20261003-011804/results.json`. The old TLS
snapshot path allocated 10,240,000 bytes in the retained negative control
`artifacts/campaign/exception-capture-negative-control-20261003-010427`.
Retained-thread creation, owner shutdown and C++/runtime TLS destructor ordering
pass six regression checks in
`artifacts/campaign/exception-tls-threads-20261003-011929/results.json`.
