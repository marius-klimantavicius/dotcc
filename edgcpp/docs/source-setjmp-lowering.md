# Source setjmp/longjmp implementation contract

Status: generated source translation passes the initial controlling/standalone
continuation subset in C and C++, including C++ handler bypass and owner reentry.
Protected-region targets, VLA-containing target functions and extension forms
remain explicit follow-up gates. This is not full source nonlocal-jump coverage.

## Existing implementations consulted

`upstream/src/c_gen_be.c:6704` emits ordinary `eok_call` nodes through
`dump_call`; there is no source `setjmp` continuation transform. Its native C
target supplies returns-twice behavior. Separately, `lower_eh.c:5392` constructs
setjmp comparisons for fully portable C++ EH, with the configured target buffer
described in `targ_def.h:5265`. Our retained C++ EH continues to use CLR
unwinding; source nonlocal jumps must not enable that portable EH lowering.

Dotcc's `DotCC.Libc/SetJmpLib.cs` provides numeric unmanaged buffer identities
and a tagged managed exception. `DotCC.Lib/Ir/IrBuilder.cs:2679` claims supported
syntactic tails and `DotCC.Lib/Backends/CSharpBackend.cs:1037` emits a restart
label with the captured jump value. We retain the tagged transport idea, while
making the target live for its complete function activation. A valid target
can outlive the lexical block containing its call, and multiple buffers armed
at one syntactic site can remain usable. Merely wrapping an if/switch's body
would not implement those cases.

`backend/tests/setjmp_control.c` verifies eleven source cases: a target after its
lexical block, multiple buffers armed by a loop at one site, repeated jumps,
discarded setjmp result, a side-effecting buffer expression evaluated once,
recursive jumps to an ancestor, while/do/for guards, reversed comparison and
buffer rearming after goto. All surviving modified locals are volatile.
Direct GCC/Clang pass at O0/O2 in
`artifacts/campaign/setjmp-control-oracles-20261003-023410`; the original EDG C
output passes GCC/Clang in
`artifacts/campaign/setjmp-control-oracles-20261003-023428/results.json`.
The first attempt's missing host multiarch include is retained, not counted as
original-C acceptance. Native headers are confined to these oracle commands.
The expanded eleven-case fixture passes all six native compilation variants in
`artifacts/campaign/setjmp-control11-oracles-20261003-024520/results.json`, with
the exact input copied into that evidence directory.

## Runtime and buffer ABI

`backend/runtime/NonlocalJump.cs` is independent of libc/native EH. Its public
API is:

```csharp
NonlocalJump.Enter(Activation* activation, uint siteCount);
NonlocalJump.Leave(Activation* activation);
int NonlocalJump.Arm(Activation* activation, void* buffer, uint site);
NonlocalJump.LongJump(void* buffer, int value); // never returns
bool NonlocalJump.Matches(Transfer error, Activation* activation);
```

`Buffer` is 16 bytes: a 64-bit activation identity, a 32-bit positive site
number and a reserved zero word. The future portable header exposes an array
of one corresponding C structure as `jmp_buf`; it stores no CLR reference or
native program counter. An activation record lives in the caller's existing
unmanaged frame and links into a thread-local active-frame chain. A process-local
monotonic identity distinguishes recursive calls and later reuse of the same
stack address. Enter/arm/leave allocate no managed or native heap storage.

Each call to setjmp stores the same current activation identity plus that
call site's continuation. Multiple buffers at one site remain valid; rearming
one buffer replaces only that buffer's target. Longjmp copies the descriptor,
validates that its activation is still active on this thread, and throws a
sealed `Transfer` carrying identity, site and value (zero becomes one). Only
the rare jump allocates a managed exception. Allocation failure terminates
explicitly. Unarmed, stale, cross-thread and out-of-range targets fail closed.

The activation chain holds no managed owner. Existing generated entry leases
keep the active owner alive; resumptions stay in the same function activation.
Cross-owner calls within a generated runtime can target an active ancestor.
Shared runtime identity across separately generated assemblies is still part
of the open cross-assembly ABI gate, and must not be claimed from this helper.

Twelve standalone JIT/NativeAOT transport checks pass in
`artifacts/campaign/setjmp-runtime-20261003-023759/results.json`, including zero
managed bytes for 20,000 warmed activation/arming cycles. This establishes the
runtime component. Generated acceptance is recorded below separately.

## Required compiler continuation transform

`dot_gen_setjmp.h` supplies recognition, continuation inventory and emission
helpers; the central emitter supplies statement and envelope hooks. Recognize the portable
header's reserved private setjmp declaration by declaration identity/signature,
not text matching arbitrary user calls. All unclaimed occurrences must produce
a diagnostic. Never emit a runtime setjmp stub that always returns zero.

The routine prepass inventories sites, then constructs a control-flow graph
covering the **whole function**, including nested blocks, if/switch/loops,
break/continue, labels and gotos. Locals and temporary slots remain in the same
caller frame across resume. Initial site execution evaluates its buffer once,
arms it and supplies zero; the continuation receives the saved jump value and
does not reevaluate that buffer expression or preceding side effects.

The dispatcher has this lifetime placement:

```csharp
// Existing frame allocation and routine entry precede this envelope.
NonlocalJump.Enter(&activation, siteCount);
try {
    for (;;) {
        try {
            // Dispatch PC; evaluate normal branches and resume sites.
            // Normal return exits this entire envelope.
        } catch (NonlocalJump.Transfer jump)
          when (NonlocalJump.Matches(jump, &activation)) {
            pc = jump.Site;
            setjmpResult = jump.Value;
        }
    }
} finally {
    NonlocalJump.Leave(&activation);
}
// Existing routine cleanup envelope surrounds the dispatcher, rather than
// running each time a jump resumes this same activation.
```

`pc` selects an internal continuation ID rather than assuming a source label
can legally enter a CLR protected region. Normal code without setjmp retains
the existing emitter. A transfer aimed at an ancestor propagates past this
routine's dispatcher and unregisters this activation normally.

First integration should accept standard C controlling/standalone contexts and
diagnose unsupported expression shapes and lifetime regions. Assignment-style
uses accepted as extensions may follow as an explicit extension, rather than
being represented as part of strict C semantics. C++ handlers and nontrivial
lifetime regions require the following adapters before broader acceptance.

## Cleanup and EH integration requirements

1. Every translated C++ handler filter must exclude `NonlocalJump.Transfer`,
   including catch-all. It is control transport, not a foreign CLR exception.
   `noexcept`, dynamic-specification, throw-copy, catch-copy, cleanup-failure,
   array-secondary-failure, and thread/global final termination filters must
   likewise bypass it or reject an invalid external boundary explicitly.
2. Transport does not alter C++ uncaught counts, create a catch frame or become
   capturable by `current_exception`. Routine bookkeeping needs a distinct
   nonlocal-transfer flag; treating it as ordinary unwind would run source
   cleanup not prescribed for a nonlocal jump.
3. CLR resource/owner-entry `finally` blocks still run in discarded frames.
   Source C cleanup extensions must be audited separately. C++ jumps that
   would skip nontrivial destruction are not silently accepted as correct
   destructor unwinding; unsupported cases initially diagnose/fail closed.
4. VLA allocations in discarded activations retain the runtime's existing
   `Vla.Restore` reclamation. Resuming the same activation requires a saved VLA
   mark per armed environment if variable storage was created since setjmp.
   That mark is not yet in the 16-byte core buffer: initial integration must
   reject such regions or deliberately version/extend the buffer before
   claiming support. Do not restore the routine-entry mark while surviving
   variables still refer to allocations made before the setjmp.
5. Jumping out of an active C++ handler needs release of its catch-frame
   bookkeeping; jumping back into a handler must preserve its caught payload
   and lifetime context. A top-level dispatcher alone cannot do the latter.
   Initially reject setjmp targets inside these protected regions. This is a
   remaining full-plan gate, not a permanent exclusion of the feature.
6. A native/unmanaged callback boundary must never let this managed transfer
   escape as native longjmp. Managed callbacks obey the explicit CLR finally
   interoperation contract. A transfer without a live translated target is a
   runtime error, never a raw native jump or stale stack-pointer dereference.

## Acceptance sequence

Fourteen source-continuation checks pass in
`artifacts/campaign/setjmp-control-20261003-025248/results.json`: eleven C
control-flow cases compiled with native GCC/Clang and the original EDG C
backend, plus static/instance generated C and trivial C++ JIT/NativeAOT. Four
negative cases verify explicit diagnostics for assignment/return expression
forms and protected/VLA target regions. The generated program never delegates
returns-twice behavior to a native function or silently returns zero.

Six further mixed-C++ checks pass in
`artifacts/campaign/setjmp-cpp-20261003-025515/results.json`. The generated
matrix repeatedly bypasses catch-all, noexcept and dynamic-specification
handlers, exits a scalar C++ handler, and jumps back through a managed callback
that entered another generated owner. Forced GC does not invalidate the buffer;
managed callback finally blocks run, exited catch frames release their payloads,
and both owners shut down cleanly. These checks cover static/instance JIT/AOT.

The native observations are deliberately distinct from the generated ownership
contract. Installed GCC/Clang leave `current_exception()` non-null after raw
longjmp out of catch(int); the exact observation is retained in
`artifacts/campaign/setjmp-cpp-oracles-20261003-025012/observations.json`.
The generated contract retires that exited catch frame and restores null.
This follows our reading of the draft's automatic-destructor restriction,
handler exit rule, and current-exception definition; it is not presented as a
change to EDG. See [nonlocal-jump restrictions](https://eel.is/c++draft/csetjmp.syn#2),
[active handlers](https://eel.is/c++draft/except.handle#9), and
[current_exception](https://eel.is/c++draft/propagation#9).

The original generic C runtime has no persistent exception_ptr provider and a
raw source jump bypasses restoration of its explicit EH stack entries. Its
isolated oracle checks destination and handler bypass, without making
post-jump library-EH queries. The oracle header uses its configured long[25]
buffer spelling, separate from our portable 16-byte descriptor. A retained
failed Clang attempt accidentally linked native libstdc++ through the clang++
driver; exit-time stream flushing then queried interposed EDG EH state.
`setjmp-cpp-20261003-025426/clang-exit-backtrace.txt` records that diagnosis.
The accepted original-C oracle uses the C clang driver and does not link a
native C++ runtime. No EDG source or generated C was patched.

First verify the standalone transport, including allocation-free enter/arming,
recursive target selection, multiple buffers/site, repeated values, invalid
target rejection and JIT/NativeAOT. Then translate `setjmp_control.c`, adding
all supported control-statement contexts, function-wide target liveness,
buffer alias/rearm, unchanged and volatile locals, recursion and owner reentry.
Add EH exclusion tests through catch-all/noexcept and explicit diagnostics for
unsupported lifetime regions. Finally extend VLA and C++ protected-region
continuations with exact native differential traces. No real-world project
acceptance is authorized at this stage.

## Pending VLA continuation ABI version 2

The candidate in
`artifacts/campaign/setjmp-vla-candidate-20261003-030003` extends the descriptor
to 32 bytes: activation identity, site, version word 2, VLA allocation head and
allocation generation. The public header advertises
`__EDG_DOT_JMP_BUF_ABI_VERSION=2`. Promotion requires updating the driver's
manifest to version 2/bufferSize 32 and making `NonlocalJump` depend on `Vla`.
The accepted canonical compiler remains on version 1 until that promotion and
translated verification finish.

Arming captures the VLA allocation chain without allocating. Longjmp validates
the saved head and generation by walking live allocations, without dereferencing
the saved address. Intervening function finally blocks restore their ordinary
entry marks. At the destination, `NonlocalJump.Resume` validates again and
restores the exact saved mark before the dispatcher enters the continuation.
This preserves allocations and jump buffers that existed at setjmp while
reclaiming later allocations in the destination activation. A generation stamp
also rejects reuse of an old allocation address.

Ten standalone JIT/NativeAOT candidate checks pass in
`artifacts/campaign/setjmp-vla-runtime-20261003-030221/results.json`. They cover
before/after allocation, repeated jumps, caller/callee restoration, a jump buffer
inside surviving VLA storage, allocation-free capture, and invalid checkpoint
rejection at throw and resume. This is runtime proof, not yet translated-source
acceptance. `backend/tests/setjmp_vla.c` and its verifier add source coverage,
including a variably modified pointer that requires no VLA allocation.

The C rule is broader than allocation liveness: jumping back after leaving the
scope of an identifier with variably modified type is undefined, even if that
identifier was a pointer or typedef and allocated no array. EDG lowers away
some such scope markers (`lower_il.c:9820`, `lower_il.c:21185`); an allocation
registry cannot diagnose every invalid VM-scope target. We diagnose expired
allocated-array checkpoints and do not claim universal detection of source UB.
Valid VM-pointer programs need no extra runtime allocation. Likewise, reading
a jump buffer through an already dangling source pointer remains source UB.
See the longjmp constraints in [WG14 N3220, §7.13.2.1](https://www.open-std.org/jtc1/sc22/wg14/www/docs/n3220.pdf).

Native GCC/Clang at O0/O2 pass the initial VLA source fixture in
`artifacts/campaign/setjmp-vla-oracles-20261003-030554`. The original generated C
has a distinct demonstration-runtime observation: after a jump, another VLA
allocation in the same activation can reclaim an array that must survive the
jump. `lib_src/vla_alloc.c:134` uses allocation-time stack-frame markers to find
dead allocations. The surviving array and later temporary share a marker; the
native jump does not restore an allocation checkpoint, and the heuristic then
pops both. The retained debugger trace
`setjmp-vla-runtime-audit-20261003-031100/markers.log` shows the surviving value
changing from 41 to 17 when the temporary reuses its storage. Rebuilding the
unmodified runtime at O0/O2 with documented alignment 16 preserves this
observation (`setjmp-vla-oracles-configured-20261003-031000/observations.json`).
No EDG source or generated C was changed. This case uses native source execution
as its defined-behavior oracle; the managed adapter uses explicit checkpoints
instead of inferring CLR frame lifetimes from stack addresses.
