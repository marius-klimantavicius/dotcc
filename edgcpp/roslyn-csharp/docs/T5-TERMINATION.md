# Default termination at a generated noexcept boundary

Status: the bounded default-only noexcept slice passes production CLI and
JIT/NativeAOT from immutable `t5/termination-source-1` (212 files). All 38
expected outcomes pass: 32 normal successes, two intended SIGABRT outcomes and
four unsupported-case rejections. The [receipt](../acceptance/lifetimes/termination-report.json)
separates native observations from selected EDG provider-policy evidence.
The first slice addresses an explicit noexcept boundary inside library output;
absence of a handler inside an arbitrary library function is not a program
termination boundary.

The genuine header-free candidate is
[noexcept-termination.cpp](../tests/Fixtures/T5/noexcept-termination.cpp), SHA-256
`b725930729d0d5028b6ea9c0cce3c816c187f251c0390c1291be55952850b3cf`.
It exports `int fixture(int* buffer, int raising)` with one initial integer-7
throw and one source typed try. A tracked outer object 1 precedes that try.
The explicitly noexcept boundary owns tracked object 3 and calls a helper
owning tracked object 2. Constructors, destructors and the event helper are
noexcept. All application calls and mapped memory writes remain generated;
there are no source native calls, callbacks, allocation expressions, custom
termination handlers or signal handlers.

## Borrowed observation storage and process modes

Each invocation runs in a fresh subprocess. Its parent creates a shared file
mapping containing 68 zero-initialized target integers, and the child harness
passes its address to the source fixture. The capture selects ordinary
`int* buffer` and `int raising` parameters, with signed integer storage 4/4 and
pointer storage 8/8. The mapped protocol therefore occupies exactly 272 bytes.

| Integer slot | Meaning |
| --- | --- |
| 0 | Number of recorded events, bounded 0 through 64 |
| 1 | Overflow flag |
| 2 | Live tracked objects, updated by actual constructors/destructors |
| 3 | Source result marker: initially 0; normal completion writes 1; source catch writes negative payload |
| 4 through 67 | Ordered event values |

The fixture does not reset live count or reconstruct observations after a
failure. The parent reads the mapping only after the child exits. This process
barrier avoids observation callbacks during unwinding or changes to abort/signal
semantics. Core-file limits belong to the isolated harness setup. Return status,
actual abnormal-exit signal, mapped values and logs are recorded separately;
runtime-specific stderr text is not a cross-engine semantic comparison.

Mode `raising=0` uses the same bodies and returns normally. Its purpose is to
observe ordinary cleanup and validate the map protocol. Nonzero mode takes the
sole throw and must terminate at the noexcept boundary. Source catch event 707,
post-helper boundary event 801, normal-return event 900 and outer destructor
event 301 must not occur after that termination decision. The initial source
does not encode a predicted event array as test authority.

## Captured execution and cleanup facts

The unchanged native-lowered input is
`/mnt/storage/roslynobj-csharp-20261015/t5/capture/noexcept-termination.roslynobj`,
SHA-256 `a8b29c975a6caeeae547a77b4202a3750cb6876aecec4a188eb19bffa06a776e`.
Public facts are in `t5/noexcept-termination-facts-6.json`, SHA-256
`2c26b1a1df00159be507c6980b76246920ad573a34c3b241fba784fd035373e4`;
the public inspect report has SHA-256
`d1c532e2ff17c95fb5beef11a66f9f2ab13d44f979e5513a33d23f36402016c9`.
These reports retain the frozen producer revision below, C++17, the itanium
target and `native_lowered` capture profile. The labels below identify public
probe entities and occurrences; catalog identities are not an execution order.

The sole initial throw is operation 91 in helper function 14. Its type is the
same signed integer type 367, size/alignment 4/4; operand 92 initializes the
retained thrown-object address 95 with 7. There is no rethrow. Boundary function
17 has `Signature.NoexceptEffective=true`; its selected parameter is ordinary
integer `raising`. The complete constructor and destructor entries 16 and 15
are noexcept and receive the captured `this_parameter`; the constructor also
receives one ordinary integer. Their independent subobject entries 22 and 23
remain separate bodies. Record 374 has size/alignment 4/4 and its `id` field is
at offset 0.

| Function | Actual cleanup region | Selected storage/action | Relevant retained occurrences |
| --- | --- | --- | --- |
| Helper 14 | Region 117, ordinal 0 to null | Direct variable 114 (`leaf`), offset 0; destructor 15 | Constructor call 74; state 80 selects region 117; note 507; throw 91 |
| Noexcept boundary 17 | Region 196, ordinal 0 to null | Direct variable 193 (`at_boundary`), offset 0; destructor 15 | Constructor call 161; state 167 selects region 196; note 800; call 173 selects helper 14 |
| Fixture 18 | Region 311, ordinal 0 to null | Direct variable 304 (`outer`), offset 0; destructor 15 | Constructor call 206; state 212 selects region 311 before typed try 213 |

Those storage descriptors have no indirect/this-relative flags, array counts,
conditions or ABI argument recipes. The helper and boundary start in null state
65535 and each has its own actual prologue, region table and epilogue. Their
normal paths explicitly reset to null before destructor calls 105 and 184.
Exceptional projection must use the active region action, rather than replay
those normal calls. The associated destructor initialization recipes 430 and
435 name the corresponding variables and lifetimes but have no executable
expression or operands; recipe associations do not add an execution root.

The outer try 213 has descriptor 439 and handler 437, catching the identical
integer type 367 into variable 306. Its saved boundary is region 311/state 0.
The caught-object copy, caught marker and catch epilogue are actual retained
occurrences 218, 224 and 247. Region 312 leads to 311 and contains the no-object
exception-release action. These handler paths remain in the capture but must
not activate when first-pass search selects the inner noexcept destination.

## Selected stock provider policy

The authoritative pristine revision is
`89e67e07c7f6d0fc38622f9778d8e79f40deaa0f` at
`/tmp/roslynobj-edg-upstream-20261012`; sparse runtime files were read through
that revision's Git objects. No native C++ runtime is compiled or linked as a
generated provider by this preparation.

`lib_src/config.h:370–378` documents the policy and defaults
`UNWIND_STACK_BEFORE_CALLING_TERMINATE` to TRUE under `#ifndef`. Header SHA-256
is `d02ab957e68d757ae4a45040af2ca32cc467a8cf2abaad0b89cf01de039da9ff`.
`lib_src/CMakeLists.txt:53–55` loads the cpprtlib macro configuration. The
pristine linux-gcc and linux-gcc-release configurations import
`default/cpprtlib`, whose entries are only USE_STDINT_HEADER and
SYSTEM_RUNTIME_HAS_IA64_THREAD_ATEXIT=0; they do not override this flag. That
default configuration's SHA-256 is
`c62d324e2ef281c13ea164ce5493543a80e20586ad347fe45a837c1e6ada4aeb`.

TRUE is the deliberately selected stock provider policy for this finite slice,
not a captured producer capability or a claim about every EDG runtime build.
The consumer must not silently choose FALSE to reproduce GCC's independent
termination behavior. Any native difference in permitted unwind behavior is
retained and explained against the selected policy.

## First-pass destination and exact cleanup depth

Exact `lib_src/throw.c` SHA-256 is
`27ed40c95c4d0f06df87d4e5969cec74c8adab774f108e2746c4cbfe64d83225`.
At 1389–1397, first-pass search selects the nearest `ehsek_noexcept` as its
destination and stops before a matching source catch in the caller. With the
selected TRUE flag, the early termination branch at 1415–1424 is inactive.
Pass 2 at 1425–1486 traverses entries strictly before the selected destination.
Each function entry performs current-region-to-null cleanup and restores the
saved caller region. At 1615–1621 it terminates at the noexcept destination.

The stock portable prologue at `lower_eh.c:4918–4985` pushes the exception-
specification entry first, then the function cleanup entry at 5017.
Consequently the cleanup frame of a boundary-local object precedes its
noexcept destination. This illustrates the stock runtime ordering; it does
not change the candidate's native-lowered capture into portable IL. The actual
capture establishes its NoexceptEffective fact, decoded regions, selected
destructors and retained state/call occurrences as recorded above.

For this three-object candidate, the selected contract therefore requires
cleanup of helper-local object 2 and boundary-local object 3, stopping before
the caller's outer object 1 or typed handler. Their expected mapped termination
prefix is `101,103,800,102,507,302,303`, live count 1 and untouched result marker
0. This contract-derived generated expectation is now proved below,
not an alleged GCC observation.

The independent GCC observation is preserved in
`t5/noexcept-native-observations-1.json`, SHA-256
`36d16fb77a8412ee682394d9d8322bfa7f740d3769d5183eeb78271af8d78bcd`.
Normal mode exits 0 with events
`101,103,800,102,507,508,302,801,303,900,301`, count 11, overflow 0, live 0 and
result marker 1. Raising mode exits with signal SIGABRT (process result -6),
events `101,103,800,102,507`, count 5, overflow 0, live 3 and result marker 0.
Unused event slots remain zero. Core dump limits are 0 for the isolated probe.
GCC therefore supplies the normal-path comparison and an explicitly different
raising observation; it does not override the selected EDG unwind-TRUE policy.

For comparison, flag FALSE would take 1415–1424 before pass 2 because no normal
typed handler was selected, even when a noexcept destination exists. Uncaught
propagation under TRUE instead has a null destination, traverses every EH entry
and terminates at 1487–1501. That separate obligation requires a genuine
executable/program-entry contract and remains outside this library fixture.

## Default abort scope and required implementation boundary

`lib_src/eh_util.c:205–215` calls std::terminate and aborts if it returns.
Its 40–59 detects recursive termination, invokes the configured termination
routine and supplies the default abort wrapper. The same file's 62–81 exposes
set/get_terminate, which this slice does not admit. File SHA-256 is
`89e972e767609a0a80dfec3af3bec6ea18dca39dfa70fb8d890ab4e0b2f3075b`.
`lib_src/error.c:91–107` performs optional diagnostics/flush then aborts; SHA-256
is `6df0567ffc9cf820b4195380b4a12bd470e2bdf63f48ae168756f04df649ee12`.

The scope is default nonreturning abort with no installed custom handler.
The approved Linux/.NET host mapping is `Environment.FailFast`, identified by
`ManagedCppTerminationPolicy.UnwindBeforeTerminateDefaultFailFastV1 = 1`.
Its actual JIT/NativeAOT SIGABRT and mapped prefix are now proved below.
An unhandled CLR exception or a source catch followed by late termination is
not this contract. No fake source handler
selection, caught transition or exception-owner release may be invented for
the termination destination. Application destruction still follows actual EDG
actions; neither GC/finalizers nor arbitrary native callbacks perform it.

The additive reader-independent runtime API is:

```csharp
bool SelectNoexceptBoundary(object boundaryToken, ManagedCppTerminationPolicy policy);
void RequireSelectedDestination();
[DoesNotReturn]
void TerminateNoexceptBoundary(object boundaryToken, ManagedCppTerminationPolicy policy);
```

The first-pass filter records a distinct terminal destination on the original
committed carrier, checking the exact boundary token and supported policy.
It does not select a typed handler or increment typed Selected/Caught counters.
The generic assertion checks destination selection before actual encoded unwind
actions; existing `RequireSelectedHandler` retains its narrower contract.
The terminal call checks the same token/policy and a prior cleanup assertion,
then calls FailFast with the exact public `DefaultNoexceptTerminationMarker`:

```text
RoslynObj.CSharp.ManagedCppException:default-noexcept-termination:unwind-true:failfast-v1
```

There is no fake caught transition or exception-payload free. The terminating
process still owns its payload; OS process teardown is not counted as a normal
runtime release. Existing integer-handler/rethrow APIs and their diagnostic DTOs
are preserved.

The new `tests/TerminationLifetime` child opens an existing exact 272-byte map,
requires all 68 slots initially zero and retains `AcquirePointer` for the whole
direct generated fixture call. Mode 0 checks the ordinary result/protocol and
prints explicit AOT-safe JSON; mode 1 returning is a failure. The coordinator's
parent harness must separately verify SIGABRT, the exact marker, the complete
normal and selected-policy event arrays, untouched unused slots and actual
emitted policy evidence. No custom signal handler or native observation callback
is installed. The frozen child/runtime now pass both execution modes.

## Verified production and process evidence

Production output `t5/termination-cli-generated-1` publishes all eight bodies
with one exact semantic runtime identity selected under
`managed-cpp-int32-noexcept-unwind-true-failfast-owner-v1`. The generated methods
pass the explicit policy enum; the actual program report states selected TRUE,
default FailFast/SIGABRT mapping, version and exact marker as provider policy.
No native abort symbol/import or native application body is invented.

Both JIT and NativeAOT normal children exit 0 and match the complete native
11-event map, result marker 1 and live count 0. Both raising children exit -6
(SIGABRT), include the exact versioned FailFast marker, and leave mapped events
`101,103,800,102,507,302,303`, live count 1, result marker 0, overflow 0 and all
unused slots zero. Marker and map are required together because an accidental
unhandled CLR error could also abort. Host stack/diagnostic text is recorded
separately. The caller's catch 707 and outer destructor 301 never execute on
this path. GCC's five-event raising observation remains an explicit policy
differential; no native/generated raising equality is claimed.

The terminal filter wraps the boundary's real observer/finally. First-pass
selection precedes the leaf and boundary cleanup chains; the terminal catch
then validates its exact token/policy and prior cleanup before FailFast. No
source handler transition or physical payload free is fabricated at abort.

All five prior normal/int-EH/constructor/array/rethrow CLI/JIT/NativeAOT
regressions pass with byte-identical generated application sources. Repeat
source/project and normalized report bytes agree; all completion hashes,
11 retained inputs and the 212-file freeze verify. All four existing genuine
unsupported candidates remain unpublished. JIT dependencies are Checks,
Generated and Runtime only; NativeAOT has ordinary system dependencies. No
builder/translator remains active. Core limit 0 is isolated test setup only.

Receipt SHA-256:
`4924dac4bfa200d7d7a30f7ddac5830eb08dbb23ec60ce00be3e7139bc8ba11a`.
The pre-implementation CLI rejection remains preserved separately. There were
no unexpected final-campaign outcomes or budget failures.

Producer admission also excludes dynamic throw specifications: actual
`roslyn_type_support.h:4–7` populates NoexceptEffective only for resolved
`p->is_noexcept`; dynamic specifications keep null. Pristine il_def.h:7130–7132
defines that bit as the C++11 noexcept form. This is retained producer meaning,
not a consumer reconstruction of throw()/unexpected semantics.

Custom handlers,
returning/throwing termination callbacks, destructor throws, program-uncaught
termination, class payloads and native unwinding remain separate unsupported
forms. The existing rethrow checker remains unchanged.
