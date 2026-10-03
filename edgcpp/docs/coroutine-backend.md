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

Suspension-dependent scalar expressions now pass ten focused checks in
`coroutine-cps-values-20261003-045934/results.json`, with 38 counter/protocol
regression checks in `coroutine-cps-regression-20261003-050031/results.json`.
The emitter schedules values and lvalue addresses into the selected-allocator
frame, preserves assignment RHS-before-LHS ordering and short-circuit branches,
and supports awaited initialization, conditional expressions and loop conditions.
Cached reads bypass a second volatile/atomic read; saved addresses continue to
use ordinary access semantics when their value is subsequently needed. Expression
helpers explicitly receive the frame pointer. No helper-local stack address is
retained across suspension.

`coroutine_value_await.cpp` agrees with GCC/Clang O0/O2; both static and instance
C# pass JIT/NativeAOT with strict warning checks. `coroutine_saved_value.cpp`
adds a C++17 sequenced shift whose left-hand volatile read must survive suspension
and mutation on the right. Clang O0/O2 and generated JIT/NativeAOT agree. The
native GCC observation is retained separately in
`coroutine-value-native-20261003-045734`: its event sequence agrees but its
computed result differs, so that fixture uses Clang as its executable oracle.
No EDG source or semantic rule was changed. Exact accepted input/helper/compiler
hashes are in `coroutine-cps-checkpoint-20261003-045934/hashes.json`.

Pointer and POD aggregate await results, nontrivial awaiter construction,
full-expression cleanup, and early destruction now pass 74 additional checks in
`coroutine-provenance-final-20261003-053258/results.json`. The eleven construction
and seven lifetime traces agree with GCC/Clang, then pass static/instance
JIT/NativeAOT with strict warnings. They include four constructor failures and
destruction at each suspension point. The exact helper/input/compiler checkpoint
is `coroutine-provenance-checkpoint-20261003-053314/hashes.json`.

EDG retains both the original operand and ready/resume/suspend expressions:
`expr.c::add_await_to_operand` copies the source expression, `lower_il.c:17293`
lowers both representations, and `c_gen_be.c:7093` renders the original operand.
Our resumable emitter executes the triples. Backend-owned provenance filtering
therefore omits only original-operand cleanup lifetimes proven absent from those
triples. It uses actual region identities, destructor routine/object identity,
and the adjacent cleanup-state transition. This handles both comma expressions
and adjacent statement pairs. A temporary variable may be reused by a later
executed await, so variable identity alone is insufficient. Such storage is
retained; only the descriptive lifetime's actions are omitted. Unshared unused
storage and its capture arguments are omitted too. EDG trees, needed flags and
region tables remain unchanged. Unrecognized references or cleanup forms are
diagnosed rather than discarded speculatively.

The initial failures and diagnosis remain in `coroutine-value-lifetime-*`,
`coroutine-provenance-20261003-051616`, and
`coroutine-provenance-trace-20261003-052823`. The latter records original and
executed region identities for reused storage. The `int&`-returning awaiter
assignment probe was rejected by both EDG frontends on identical preprocessed
input before backend emission; its evidence is
`coroutine-value-lifetime-20261003-050219/original-c-preprocessed2.log`. The
accepted pointer-returning equivalent does not claim reference-result coverage.

Reference-result source acceptance, protected-expression suspension, conditional
or array descriptive cleanup forms, complex pre-frame allocation-failure expressions,
default-argument descriptor calls, advanced placement conversions, exported factories and
function-try coroutine factories remain open.


Selected size-only nothrow coroutine allocators now invoke the retained
allocation-failure return hook before allocating frame metadata or constructing
the promise. A noncopyable return object is initialized directly into the caller's
hidden return destination; POD returns and exceptions thrown by the hook are also
supported. Branch-local cleanup bookkeeping exists only when the emitted
expression requires it. No per-failure heap or managed storage is introduced.

The noncopyable fixture passes five native/static/instance JIT/NativeAOT checks in
`coroutine-allocation-failure-20261003-054438/progress.json`; that run subsequently
stopped on the separate default-argument probe below. The POD fixture passes five
checks in `coroutine-allocation-pod-20261003-055107/results.json`, including a
throwing hook. Both use native GCC/Clang O0/O2 event traces and strict C# warnings.
Exact source/runtime/compiler hashes for the initial allocation implementation
are in `coroutine-allocation-checkpoint-20261003-054439/hashes.json`.

Provenance matching additionally requires zero offset for a direct local-object
destructor and rejects any destructor/object/predecessor key shared between an
executed and descriptive cleanup region. Construction/lifetime regression passes
74 checks in `coroutine-allocation-regression-20261003-054507/results.json`.

A valid hook with a default parameter remains an explicit unsupported gate.
`symbol_tbl.c::select_coroutine_new_delete` selects such a routine, but the
retained descriptor in this probe contains no actual argument. `lower_il.c`
clears `a_param_type::default_arg_expr` (9539–9551 and 10328–10334). The original
C backend preserves coroutine syntax for a downstream C++ compiler, which can
supply the source default; our resumable backend cannot reconstruct its side
effects from the lowered routine type. The original failing probe and generated
C# remain in `coroutine-allocation-failure-20261003-054438/pod`. Its unchanged
source is `tests/libcxx/coroutine_allocation_failure_default.cpp`; native
GCC/Clang O0/O2 verify its default side effect and throwing-hook behavior.
The emitter now diagnoses a missing required descriptor actual instead of
publishing a C# call with missing arguments. Supporting this form requires a
backend-owned prelower semantic capture path; EDG sources remain unchanged.

The default-argument guard passes three focused checks in
`coroutine-allocation-default-guard-20261003-055413/results.json`: the unchanged
source passes native GCC/Clang O0/O2, and both generated ownership modes reject
it before C# publication. That directory's `hashes.json` and `checkpoint/`
retain the final guard helper, fixtures, verifiers and compiler. This diagnostic
is remaining-feature evidence, not acceptance of default-argument translation.

A bounded placement-allocation adapter now calls the selected allocator with
exact scalar/pointer values and directly bound explicit reference parameters.
It uses the incoming factory parameters before copying them into the coroutine
frame. Hidden result parameters are excluded. Matching uses retained C++ type
identity and reference flags, not merely equal C# storage types. Class copies,
user-defined conversions, extra defaults, variadics and implicit-object placement
remain separately diagnosed. Canonical global nothrow allocation is supported below.

The reference and value fixtures pass ten native/static/instance JIT/NativeAOT
checks in `coroutine-placement-20261003-060229/progress.json`. They cover reference
identity and mutation, scalar/pointer arguments, reference-to-value copies,
allocator throws, and normal promise/frame destruction. The subsequent member
probe in that run stopped before backend emission. The compiler, helpers and
input hashes are retained alongside it. Conversion rejection and ordinary
size-only counter/allocation-failure JIT regressions pass nine checks in
`coroutine-placement-gates-regression-20261003-060414/results.json`.

Two valid native probes remain frontend source-acceptance gates: binding a
mutable reference allocator argument to a by-value coroutine parameter, and
binding an implicit object to an `Engine&` placement parameter. Both pass native
GCC/Clang O0/O2, but both EDG frontends reject their selected allocator/promise
constructor calls before backend emission. Identical preprocessed-input evidence
is retained in `coroutine-placement-20261003-055857/original-c-frontend.log` and
`coroutine-placement-20261003-060229/member-original-c-frontend.log`. Their
source fixtures are retained as `coroutine_placement_parameter_lvalues.cpp` and
`coroutine_placement_member.cpp`; they are not translated-feature acceptance.
The accepted explicit-reference fixture does not claim either missing form.

The final placement guard checkpoint passes nine scoped JIT/native/diagnostic
checks in `coroutine-placement-final-20261003-060706/results.json`; its
`hashes.json` records the final helper and compiler. Both positive ownership
modes still pass after making unsupported implicit-object placement explicit,
and the class-conversion probe is rejected before C# publication. The earlier
ten-check matrix provides the corresponding NativeAOT acceptance.


Canonical global nothrow coroutine allocation now passes ten focused checks in
`coroutine-global-nothrow-20261003-061733/results.json`. The emitter validates the
fixed LP64 Itanium allocator ABI together with EDG's semantic nothrow-parameter
predicate, locates the actual source-defined `std::nothrow` object with matching
qualified type, and passes that object's address. The early backend-owned
requirement set retains the object even when no ordinary source expression
references it. EDG needed flags are not modified and no substitute tag object
is synthesized. A missing provider definition is diagnosed before publication.

The fixture uses pinned libc++18 `src/new_helpers.cpp`, the existing exception
class provider, and original `src/typeinfo.cpp`. `provider.json` records their
hashes. A synthetic replacement nothrow allocator checks pointer identity against
`&std::nothrow`, failure without promise construction, and successful normal
lifetime; a second variant omits the explicit source reference to exercise the
requirement inventory. Both variants pass static/instance JIT and NativeAOT with
strict warnings. Native GCC/Clang O0/O2 agree on the identity/lifetime trace.
`hashes.json`, `checkpoint/`, and `runtime-hashes.json` retain the final compiler,
helpers, inputs and generated runtime evidence. Custom member allocation taking
a nothrow tag remains a placement-conversion gate.
