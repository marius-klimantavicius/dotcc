# C++ coroutine backend

Status: the first source counter passes static/instance JIT and NativeAOT,
with the exact native lifetime trace. The complete coroutine protocol remains
a full-plan language gate; the scoped boundaries below are still open.

## Retained EDG semantics

`func_def.c:1274` expands the coroutine into parameter copies, promise
construction, initial suspend, user body, the unhandled-exception catch and
final suspend. `body_generated` identifies this semantic expansion. It does
not mean that a resumable target frame already exists.

`lower_il.c:17293` retains `enk_await`/`enk_yield` and lowers their three
ready/resume/suspend expressions. The ready expression incorporates evaluation
of the awaiter, including `await_transform` and `operator co_await` where needed.
The separately retained original operand is descriptive and must not be
evaluated again. The initial resume expression already sets EDG's
initial-await-resume-called variable immediately before invoking await_resume
(`expr.c:49061`). Preserve those expressions and EDG cleanup regions.

The descriptor also supplies the chosen new/delete routines, allocation-failure
return expression and converted get_return_object expression. These require
explicit backend emission. `c_gen_be.c:10089` ignores the descriptor statement;
`c_gen_be.c:7093` emits retained co_await expressions. Its output is useful for
ordering and layout inspection, not an executable lowered-C coroutine oracle.

## Frame and dispatch contract

The compiler reserves one selected-allocator allocation containing the runtime
header, promise backpointer, aligned promise, copied parameters, suspension-live
locals and temporaries, cleanup cursor and continuation state. No pointer to a
resuming method's stack may survive suspension. Frame storage uses native
allocation; it is not a managed object graph or a pinned array per coroutine.

The factory invokes the selected allocation function, follows the explicit
allocation-failure path, executes parameter copies and promise construction,
obtains the return object once, and starts the initial suspension protocol.
Construction failures release exactly the successfully constructed objects and
the matching allocation. A coroutine that finishes without suspending at final
suspend may destroy its frame before the factory returns, so the factory must
not read that frame after starting execution.

Resume preserves source control flow and structured EH boundaries. Every
suspension publishes the continuation and cleanup state before await_suspend.
The void/bool/handle return forms respectively suspend, conditionally resume,
or transfer to another coroutine. Await_suspend may resume or destroy the frame
it was given; neither the dispatcher nor runtime may read that frame after a
successful suspending callback. Await_resume executes once on each resumed path.
Final suspension marks done before invoking its suspend callback. Destruction
uses the retained cleanup cursor and selected delete routine.

`runtime/Coroutine.cs` implements an iterative resume loop for symmetric handle
transfer, avoiding recursive CLR frames. Its header contains managed function
pointers and unmanaged fields only. Promise lookup/from_promise uses the reserved
prefix backpointer, without an address dictionary. Source no-op handles require the pinned libc++ ABI overlay described below;
complete source protocol coverage remains an integration gate.

## Owner and allocation lifetime

Suspended frames are raw handles under the existing owner-lifetime contract:
the caller keeps the originating owner alive and does not dispose it while
resume/destroy uses are intended. Active operations take normal owner-entry
leases. A suspended frame is not an active-call lease, which would prevent
ordinary global owning-task destructors from running during Shutdown.

An intrusive registry roots the owner with one GCHandle while it has frames.
Normal destroy detaches the frame after source destruction and before selected
deallocation. Shutdown drains source TLS/global destructors first, then abandons
remaining frame registrations before disposing its allocation arena. Abandonment
releases managed rooting and never invents promise destruction or an operator
delete call for a leaked raw frame. Physical memory follows its actual allocator:
ordinary owner heap allocations are reclaimed by heap teardown; a custom pool
retains its own allocation contract. Handles are invalid after owner disposal.

## Evidence

`backend/tests/libcxx/coroutine_counter.cpp` passes native GCC and Clang with an
exact 19-event allocation/promise/initial/yield/final/destruction trace. Commands
are retained in `artifacts/campaign/coroutine-counter-audit-20261003-032100`;
that directory's first EDG attempt failed because the host multiarch include
was missing. The corrected original-C semantic output is retained separately
in `coroutine-counter-audit-20261003-032130/oracle.c`.

Eight standalone JIT/NativeAOT checks pass in
`artifacts/campaign/coroutine-runtime-20261003-032500/results.json`. They cover
promise roundtrips, state/done/destroy, 10,000 allocation-free symmetric
transfers, self-destruction during a callback, owner rooting/release, abandonment
without source destruction, and invalid/completed frame rejection. These checks
establish the transport component; they do not establish translated coroutine
support or the complete promise/await/EH protocol.

## Source-emitter bring-up boundaries

The first source candidate supports statement-position await/yield and retained
structured control flow with caller-frame storage. Value-producing awaits,
awaits embedded in conditions, placement/allocation-failure adapters, exported
factory wrappers, and coroutine/nonlocal-jump lifetime integration are explicit
diagnostics. This is an implementation checkpoint, not complete coroutine
support. The fixture also contains an unused value-await coroutine to ensure
unused descriptors cannot expand reachability.

The next EH acceptance must distinguish initial-await failures from exceptions
escaping `promise.unhandled_exception()`. The latter leaves the coroutine
suspended at its final suspend point; promise destruction and deallocation occur
when its handle is destroyed, rather than as ordinary function unwinding.
See [the coroutine definition rules](https://eel.is/c++draft/dcl.fct.def.coroutine#14).
Native GCC and Clang agree on that ordering in
`artifacts/campaign/coroutine-unhandled-native-20261003-040234/results.json`.
The subsequent protocol matrix below implements and accepts this boundary.

`coroutine-protocol-native-20261003-040142/results.json` retains the broader
native-only protocol audit. Initial ready/suspend exceptions with an owning
nontrivial return object differ between the two host compilers; that observation
is not an EDG defect claim or a substitute for the language/retained-IL analysis.

Five first-source checks pass in
`artifacts/campaign/coroutine-counter-source-20261003-040856/results.json`: native
GCC/Clang O0/O2 plus generated static/instance JIT/NativeAOT. Strict warning checks
pass with split generated files. The nineteen-event trace covers the selected
allocator, promise, indirect factory call, initial/yield/final suspension, saved
loop local, promise access and selected destruction/deallocation. An unused
value-await coroutine does not become a reachability root.

The fixture calls the factory through a volatile function pointer so native
allocation elision cannot remove the allocator events being compared. The first
Clang O2 observation, where that permitted optimization occurred, remains in
`coroutine-counter-source-20261003-040810`; the subsequent packaging assertion
correction is retained in `coroutine-counter-source-20261003-040841`. Neither
observation required altering EDG or generated output.

The next source protocol matrix passes 33 checks in
`coroutine-protocol-source-20261003-041353/progress.json`: GCC/Clang agree on eight
traces, each matching generated static/instance JIT/NativeAOT. They cover ready
true, bool suspend false/true, exceptions in user suspend/resume and initial
resume, a throwing unhandled-exception callback, and early destruction.
The unhandled-exception boundary now preserves final-suspended promise lifetime.
Its exact source/compiler checkpoint is
`coroutine-protocol-checkpoint-20261003-041723/hashes.json`.

The same run stopped before executing its no-op fixture because pinned libc++
selected the GNU fallback's native two-function-pointer frame. The explicit
configuration overlay `runtime/libcxx/config/__coroutine/noop_coroutine_handle.h`
retains the one-pointer public handle and original license, replacing only that
native representation with private `__edg_coro_noop()` ABI access. Exact upstream,
overlay and patch hashes are in `config/edg-overlays.json`; source acceptance of
this overlay now passes the focused matrix below. Existing acquisitions are retained unchanged.

The no-op runtime component independently passes eight JIT/NativeAOT checks in
`coroutine-noop-runtime-20261003-041151/results.json`. Its immortal process-global
frame uses a pinned array, supports erased resume/destroy and promise roundtrips,
and remains stable through GC. It has no source allocation-owner lifetime.


The combined no-op/protocol/counter matrix passes 43 checks in
`coroutine-noop-source-20261003-042521/results.json`, using fresh acquisition
`libcxx-coroutine-noop-20261003-042124`. Acquisition validates the pinned original
header and configuration replacement hashes and records its overlay manifest.
The runtime and generated code use an actual managed callback frame, rather than
reinterpreting the native libc++ fallback.

The final guard/failure checkpoint passes 16 checks in
`coroutine-guard-initial-20261003-043554/results.json`. It rejects a needed legacy
native no-op factory before publishing C#, permits unused legacy inline frame
initialization, and repeats static/instance JIT/NativeAOT counter/no-op behavior.
Five additional failure scenarios verify allocation-function throws, promise
construction, get_return_object, initial await_ready and initial await_suspend.
The latter use a trivial returned handle, yielding identical GCC/Clang O0/O2
traces without the earlier owning-return-object disagreement.

Value-producing awaits, suspended condition expressions, allocation-failure
return hooks, placement allocators, exported factories and function-try
coroutine factories remain open. `coroutine_value_await.cpp` is a native-only
next-slice fixture; it is not translated acceptance evidence.
