# Selected definitions and composed scalar calls

The [compact receipt](../acceptance/duckdb/selected-composition-report.json) records
77 expected phase outcomes: 69 successes, two expected termination signals and six
intended rejections. No unexpected outcomes or budget violations occur in this
final campaign; earlier compile/publication/bookkeeping attempts and the bounded
dependency-probe timeout remain separately preserved. Product 263 files, current
checkers and all input hashes verify. Receipt SHA-256:
`5471ddec7603f2570f01b281472d4a8442d9bd444243aa1409d1665f9f625579`.

This milestone verifies bounded explicit managed definitions across supplied
translation units, direct class-exception construction, and a composed borrowed
member/pointer/scalar call chain. Public EDG identities and retained operations
remain authoritative. Default all-definition output and selected-definition
output keep separate completion scopes.

The constructor candidate's native, JIT and NativeAOT executions each repeat
result 24, values `[7,0,17,0,3,2,2,1,0]` and all 30 events. Each invocation creates
three payload allocations, commits two, destroys the two completed payloads and
releases one distinct failed pending parent without its payload destructor.
Captured temporary/local cleanup under the propagating child's selected handler
precedes pending parent storage release. Completed payload destruction precedes
its storage free. Complete layout 16/8 and occupied extent 12/8 remain distinct.
Evidence: `t5/class-construction-cli-generated-2`, its frozen checker, and
`t5/native-class-construction-run-1/stdout.txt`.

The composed scalar fixture passes the complete native/JIT/NativeAOT matrix:
two input tuples, four pointer/branch configurations and two invocations each,
with all 24 values and events compared. Results are `201,5,206,206` for 5/201 and
`255,128,383,383` for 128/255. Physical checks measure borrowed owner/input views
56/8, owner payload offset 24, complete proxy 24/8 with selector offset 16,
carrier/base offsets, alignment and strides. Captured proxy nonvirtual extent
17/8 is not substituted for its complete size. Actual getters, both Boolean
conversions, pointer delegation and composed scalar bodies execute fresh
null/first/second/null transitions, including unsigned-byte reads at input offset
24. Direct versus skipped/base guard effects, address identities, pointer cookies,
full-width untouched fields and entire guarded buffers agree. Generated raw
layout bytes and native live C++ objects are explicitly different representations;
no C++ construction, virtual execution or ownership claim follows. Evidence:
`t5/composed-scalar-generated-2`, `t5/composed-scalar-preparation-1/provenance.json`
and `t5/native-composed-scalar-run-1/stdout.txt`.

Independent selected two-unit C and C++ publications pass JIT and NativeAOT,
returning 1150 and 113111 with their exact observations. Declaration-to-definition
bindings use current-open identities, exact captured linkage and selected-boundary
ABI checks. This does not establish general ODR resolution or link the DuckDB
object catalog. The focused 42-phase regression also preserves earlier emitted
source bytes, supported owner profiles and four genuine unsupported cases. The
selected core getter retains its prior source and has zero ambient candidates.
The genuine read-only-write case remains rejected and unpublished.

Actual core293 startup is conservatively rejected with ROC1212 naming its retained
`__sti__` routine. Its 14 constructors, 14 `__cxa_atexit` registrations and one Slot
constructor are real obligations; the residual `dik_none` descriptor is not their
schedule. No startup or teardown is erased. Next work is the actual captured
startup/atexit schedule together with the real guard's remaining constructor,
string, JSON and library closure. Broad T5, full Vector/DuckDB execution, arbitrary
methods, native unwinding and general NoThrow/cache admission remain open. Scalar
composition adds no Runtime/provider substitution; the separate pending owner
extension does not widen those boundaries.
