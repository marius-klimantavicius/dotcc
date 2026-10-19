# Owning API progress (Owner B, 2026-10-17)

## Owner B34 — private execution/worker contract implemented; actual RAW JIT/NativeAOT controls pass

The facade keeps the initializing caller as sole external owner and executes
initialization, restore/save, evaluation/import/call/conversion, INCREF/DECREF,
error/output capture, join/finalize/destroy on one private64 MiB thread. Requests
are bounded to one outstanding operation; span inputs are copied and completion
retains arguments through callback roundtrips. ExceptionDispatchInfo preserves
original managed exception stacks. The existing cancellation bridge/registration
drain and trace-hook restoration remain intact. Waiting never abandons Python.

Executor-origin application callbacks run synchronously on the waiting caller
through one narrow mailbox; genuine Python workers invoke callbacks directly.
No arbitrary SynchronizationContext pumping or host monitor spans translated
work, callbacks, GIL release or joins. Reentry/foreign access reject before queueing;
live values reject disposal before stopping, permitting release/retry. Roots
remain through worker join/finalization. Uncertain startup/boundary failures keep
the process reservation and context/policy retention; no finalizer enters Python.

Actual correctedV20 RAW JIT and NativeAOT controls each pass:225 assertions/two small256 KiB caller
generations,114 ThreadPool-origin assertions,30 legacy assertions/two separate
process-lifetime generations and2 uncertain-startup reservation assertions.
All owning APIs, callbacks/worker stack bounds, error frames/output, cancellation/
trace restoration/reuse, live-reference disposal retry and shutdown/atexit
callbacks execute through the real facade. The EXACT unchanged required JSON
highly-nested-object decoding method passes with no skip/XFAIL and same-interpreter
reuse. This qualifies that focused JIT regression, not every stack path.

A static translated program's Libc context spans its entire program lifetime.
The initial harness disposed a DIFFERENT program context between Python runtime
generations and aborted with an AccessViolation; its original log cannot establish
which initialization failed. [Original failure and preserved small inputs](../artifacts/owner-b-r34-20261018/first-actual-blocker.json)
remain RED. Global arrays/pins and main allocator state outlive Py_Finalize.
The facade now rejects a different context or null/explicit binding mode BEFORE
translated entry, including after the original program owner is disposed.
One same context supports repeated Python generations; arbitrary owner switches
for this static VM are unsupported. No permanent lease or second context is added.

[Actual own consumer/input proof](../artifacts/owner-b-r34-20261018/actual-jit-same-owner-input-proof.json)
binds entry/facade/library separately from canonicalV20 and old96d7/a9997 products.
[JSON case log](../artifacts/owner-b-r34-20261018/actual-json-initial-json-case.log)
and [revised shared readiness](../artifacts/owner-b-r34-20261018/revised-shared-source-ready.json)
retain scope and limits. Initial d9 controls missed the null legacy binding;
V19 remains known bad. Corrected shared49/49 and current-Libc JIT/AOT bounds pass.
The full unchanged RAW JIT suite completes all4867 unique IDs:4751 unique
passes,12 IO failures,101 required skips and3 required XFAIL; zero errors, fatal
or unrun IDs. The runner has4875 occurrences:4759 pass events include8 repeated
IDs. None of the116 unique nonpasses is a success. The exact JSON case passes
in this full run too. NativeAOT focused controls and JSON/reuse pass; own177 ILC
references contain no second Libc assembly. Final both-form/new193/full-matrix/
D10/ABI/rooted/trimmed/delivery/audit/downstream/P0–P6 remain open.
[Full unique/event counts](../artifacts/owner-b-r34-20261018/full-required-journal-counts.json),
[actual AOT inputs](../artifacts/owner-b-r34-20261018/actual-aot-initial-input-proof.json)
and [B34 handoff](../artifacts/owner-b-r34-20261018/handoff.json) retain exact scope.


B34 hands the genuine safe execution lane to B35 for the12 newly exposed
ftruncate/offset failures; that nonowned runtime path was reported without B34
editing it. [Direct safe release](../artifacts/owner-b-r34-20261018/serial-slot-release-to-b35.json)
records37 terminal PIDs, no active/queued work and three independent free checks.
Initial d9 legacy-binding and first-harness AV failures remain retained; no
algorithm, recursion limit, required method/decorator/exclusion or generated
source/resource was patched.64 MiB is measured for these paths, not a universal
guarantee for arbitrary inputs or the still-open complete matrix.

## Owner B32 — actual V18 metadata pass; required RAW JIT recursion fatal

The new ordinary owning consumer independently binds its own entry, facade and
library PE/PDB/Compile inputs, including the exact B31 `PosixLib.cs` section.
Its library is SHA256 `a9997b05…`, MVID `5b4b8cc0-7ea0-4e36-89e4-11b9bb682ea2`;
A28's canonical `96d7a662…` remains a separate artifact. Two startup/finalization
cycles pass two callbacks, eight original frozen-finder executions and deleted-cwd
recovery. The exact unchanged R30 pyc fixture passes native and actual execution:
physical/reported0600/0644/0664, uid/gid1000 and real bytecode loading of42 agree.
The original first importlib failure and all38 previous errors pass unchanged,
both in focused replay and the immediately following full required journal.

All23 seeds discover5134 unique IDs/5142 occurrences, zero blocked, selecting
all4867 required IDs. The full raw JIT process naturally aborts-6 after241.618s
with managed stack overflow in
`test.test_json.test_recursion.TestCRecursion.test_highly_nested_objects_decoding`.
The authoritative journal records3804 unique started/3803 completed/3712 passed,
zero errors,88 required skips and3 required XFAIL;1064 lack completed outcomes
(1063 never started, one fatal). Its earlier ordinary report is not terminal
case-count evidence. No recursion diagnostic, caller-stack override, limit/test/
decorator/exclusion change or runtime/API/Host repair was attempted.

[B32 handoff and exact inputs](../artifacts/owner-b-r32-20261018/handoff.json),
[journal counts](../artifacts/owner-b-r32-20261018/required-suite-journal-counts.json)
and [current fatal](../artifacts/owner-b-r32-20261018/current-blocker.json) retain
the results. Production stack-safety resolution and required skip/XFAIL
adjudication remain open. Processed stays stale V16 and embedded193 oldV11;
all final four-cell/D10/ABI/ownership/thread/lifecycle/rooted/trimmed/delivery/
audit/downstream/current-tool P0–P6 gates remain mandatory and unclosed.

## Owner B31 — generic metadata repair SOURCE-READY; actual retry pending

The saved B30 pyc reduction identifies false shared stat metadata: direct-created
files physically have0600/0644/0664 and uid/gid1000 while old FillStat reports
zero permissions/identity. Unchanged importlib then creates bytecode physically
0200 from that reported mode; its loader correctly returns EACCES. The generic
Linux x64 repair maps real host144-byte metadata into the unchanged **96-byte**
target stat ABI, preserves actual owner handles for fstat and implements final-link
lstat semantics. Independent native/header/actual-binary measurements correct the
inherited104-byte size description; no shared header or creation-mode edit occurs.

Original604 checks fail266 before repair and pass604 after it. Final expanded
current-Libc controls pass685 each in JIT/NativeAOT;35 metadata/file/path/owner
unit tests pass with zero skips. PE/PDB and actual176-reference ILC input bindings
are retained. These results do not qualify an interpreter with the new runtime.
B31 released all three locks and its lane directly to A28/supervisor for focused
compiler validation, one normal capture/raw286 retry, then the same unchanged
pyc fixture and early actual required-suite execution. See
[B31 source-ready handoff](owner-b-handoff-r31.md).

B30's actual raw journal remains3804 started/3803 completed:3674 passes,38 errors,
88 required skips and3 required XFAIL;1064 required cases lack completed outcomes.
Its natural JSON recursion stack overflow and all final raw/processed JIT/AOT,
owning/ABI/thread/lifecycle/rooted/delivery/audit/new193/P6 gates remain open.
V17 products are frozen, processed is stale V16 and embedded193 old V11.

## Owner B30 — new RAW startup and required-suite source preparation

The separate [RAW discovery entry](../tests/RawDiscoveryR30/Program.cs) calls the
actual owning API for two startup/finalization cycles, unchanged B29 frozen-path
assertions and the four original finder IDs twice, then invokes the unchanged
full inventory runner in a fresh process. It does not configure PyConfig itself.
The [prepared driver](../tests/RawDiscoveryR30/run.py) binds its actual entry,
facade and library PE/PDB/source inputs before execution and retains durable
case-journal counts even after an abort. Final acceptance guards stay unchanged.

All actual B30 commands remain UNRUN until A26 directly supplies its new strict
RAW286 receipt and safe heavy release after P1's performance work. No build,
native, emission, campaign or build-lock queue is started during preparation.
The frozen inventory remains5134 unique/5142 occurrences,4867 required/267
excluded across23 seeds. V16 processed and oldV11 embedded evidence remain
separate; all final D10, four-cell, owning/ABI/thread/lifecycle/delivery/audit and
P6 gates stay open. See [source preparation](../artifacts/owner-b-r30-20261018/source-preparation.json).

## Owner B29 — cwd and owning startup fixes are SOURCE-READY

B29 changes only `DotCC.Libc/PosixFsLib.cs` and the authored Host C startup.
`getcwd` contains named filesystem exceptions, preserves NULL/ENOENT after cwd
deletion, implements GNU requested-size allocation and zero-size rules, and
writes only the actual path length for LP64 capacities. The existing path-based
virtual owner cwd now rejects a vanished path; it does not acquire inode or
process-cwd ownership. Existing allocator, thread and disposal contracts remain.
The owning Host gives `PyConfig.stdlib_dir` its own wide copy of the same selected
stdlib root already appended to `module_search_paths`, before initialization.
Temporary raw storage, copied PyStatus and `PyConfig_Clear` retain their owners.

The 156.902-second focused group retains old-source REDs and one corrected
harness failure. Identical C checks pass natively (47) and through current Libc
callbacks in JIT/NativeAOT (47 process + 47 virtual-owner checks per mode), with
zero escaped exceptions and 18 additional owner/GC/thread/disposal assertions.
The actual changed Host linked to the exact-pin native base passes two selected
roots, two lifecycles/two callbacks per root, and an ordinary returned error.
Four separate typed PyConfig consumers use unchanged raw/processed V16 test and
historical V11 embedded PEs: each passes real-layout, relocated non-ASCII root,
owned-copy/GC/clear, error-status and two-lifecycle controls. The four unchanged
frozen-finder IDs pass twice in each V16 form with the full delivered root.
These are focused configuration controls, not a full required-suite retry.

The repaired Host is **not** in those frozen PEs; new runtime and owning startup
execution await normal capture/emission. Fresh entry/reference PE/PDB/source and
current-Libc ILC proofs are retained; 25 additional strict negatives run after
execution and are labeled accordingly. A partial relocated tree initially loaded
`test.support` from a different root; that unmodified failure is preserved, then
relocation and original-case controls are run separately with correct inputs.
See [B29 handoff](owner-b-handoff-r29.md). No compiler/header/policy, frozen case,
required skip/XFAIL, generated output, main plan/ledger or canonical product is
changed. B28 journal counts and all expanded D10/P0–P6 gates remain open.

## Owner B28 — fresh V16 proof; raw-JIT progress and native-confirmed blockers

Both published forms pass fresh eight-value target-header layout, 24 common
public ABI fields plus three explicit wchar differences, and exactly38 actual
ABI assertions/two lifecycles. Their own source/AST/address/cache/IL/RVA reviews
find6259 regions,1963 caches/9070 uses,454 pins and nine concrete lifetime
exceptions per form.1533 byte getters cover944670 exact bytes/1706 calls with
getter newarr0. These are safety/legacy-byte preflights; expanded D10 primitive,
readability and full allocation acceptance remain OPEN.

Actual raw-JIT receipt `20261018-083246-e8989539` discovers all23 seeds,
5134 unique IDs/5142 occurrences and selects the unchanged4867 required/267
excluded IDs. Its own actual consumer PE/MVID/PDB/project/source/IL/RVA/lifetime
proof passes before execution; wrong identity/mapping/reference/socket-edge
controls reject. The journal retains2609 passes, one required UTF-8 skip, two
required expected failures and20 failed-subtest events across four IDs:
2616 unique completed cases, one started without outcome and2251 required IDs
without a completed outcome. The last ordinary report snapshot ends earlier at
2440 executed/2437 passed; it is preserved without overwriting its counts.
The prior top-level-await, ENOTDIR and signal/yield-from cases now pass.

The process naturally aborts134 at importlib `test_deleted_cwd`: shared getcwd
lets FileNotFoundException escape. Fresh native returns NULL/ENOENT2; the exact
actual suite PE reduction reproduces the escaping exception. Native embedding
also reproduces missing frozen filenames when PyConfig.stdlib_dir is omitted;
setting that field corrects the native reduction, while the actual PE retains
None. Both unchanged descriptor expectedFailure methods reproduce natively.
Native UTF-8 modes1/0 confirm the skip guard/configuration distinction, without
an actual candidate-mode case pass or a waiver. No runtime/header/compiler,
source/case/decorator/exclusion or feature-policy repair is made.

The first framework prerequisite unexpectedly rebuilt MetadataAudit, and existing
specialist commands initially lacked nonblocking -n. Both deviations and exact
original argv remain retained; focused adapters now require the explicit pinned
reader and nonblocking locks. V16 tools/capture copies/resources/products remain
frozen, independent of A25's authorized source-only compiler edits.92 audit and23
reporting controls pass. See the [B28 handoff](../artifacts/owner-b-r28-20261018/b28-final-handoff.json)
for actual inputs, native REDs, journal/snapshot distinctions and genuine release.
Other expensive cells remain unqueued. All four-cell/owning/thread/GC/lifecycle/
rooted/trimmed/delivery/audit, new193, expanded D10 and final production/reproduction
P6 gates remain open; P0 is in progress and P1–P6 remain open.

## Owner B27 — SOMAXCONN shared-header source ready

The sole production edit defines the native Linux integer `SOMAXCONN=4096` in
`sys/socket.h`. The original A23 reduction fails before and compiles afterward;
native and shared-header type/guard/layout controls agree on default128. All28
bounded native/selected-header/direct current-Libc JIT/AOT TCP cases pass with
kernel-observed queue caps, real echo, errors and endpoint closure. Four wrong
payloads reject;23 unchanged socket/readiness/reuse unit cases pass, zero skips.
Actual entry/library PE/PDB/source/ILC inputs and service IL are independently
bound. See [B27 handoff](owner-b-handoff-r27.md).

V15 still embeds the old header; no new tool or CPython product pass is claimed.
A24 normal capture and SAME-C direct/object raw/processed JIT/AOT plus unchanged
socket-unit controls precede whole286. A22's twelve test-only dependent flags
are live; canonical193/286 and B22's1999 executions/1995 passes/four nonpasses/
2868 unrun remain unchanged. Expanded D10 and all remaining P0–P6 gates stay open.

## Owner B26 — V15 unchanged C full service after-controls pass

All48 fresh direct/object × raw/processed × JIT/NativeAOT cells pass, with nine
fresh native controls. Each family has eight cells: the original resolver,
47-case addrinfo, three previous native-oracle cases, original B22 directory
and synchronous SIGINT reductions, and independent protoent typed-return fixture.
Full native stdout matches exactly; no transcript normalization is used. Resolver
execution covers actual19 layout observations, NI constants/59 buffer/error/service
cases, NSS aliases, TCP512 exec/UDP512 biff, IPv6 scope, typed hostent lookup,
h_errno read/write and addrinfo pointer graphs/free. Directory controls return
ENOTDIR20 for file/ancestor/slash and ENOENT2 for a missing child. SIGINT delivers
once before raise returns. All12 raw routes contain77 byte-exact runtime sections.
A21's exact header8 remains retained context; it is not counted as B26 execution.

Every actual JIT PE and ILC-selected entry PE has its own SHA/MVID, matching PDB,
actual compiled project/explicit PathMap/unique physical Compile input/checksum
proof. All ILC references, roots, native objects and executables are separately
bound. Actual IL decodes one static function-address cache per symbol, exact typed
resolver signatures and host/thread/cleanup call edges.22 wrong identity/mapping/
source/cache/signature/cleanup/reference/native-output inspections reject.
[The B26 handoff](owner-b-handoff-r26.md) gives exact reports and genuine release.

No C fixture, runtime, header, compiler, live policy, profile or canonical product
changed. Existing B23/B24 worker/GC/disposal/fault controls remain historical;
these emitted service controls do not establish full CPython D10, interpreter,
ABI/thread/GC/lifecycle/owning/rooted/trimmed/delivery/audit/upstream/P6 acceptance.
B22's1999 executions/1995 passes and four nonpasses,2868 unrun, frozen4867 required
IDs and267 exclusions remain unchanged. P0 is in progress; P1–P6 stay open.

## Owner B25 — V14 unchanged header exposes typed function-pointer compiler blocker

Independent A20/B23/B24R2 bindings and all eight original native controls pass.
The unchanged resolver header emits with77 exact runtime sections, then its first
raw-JIT build fails CS0407: C's hostent-returning function pointer resolves to
the shared CLR void*-returning gethostbyname method. Native-backed hostent and
independent protoent/getprotobyname reductions reproduce the mismatch in direct
and object routes. The same hostent fixture's direct-call control passes actual
raw JIT with native-exact address fields and h_errno99/errno123. Its own PE/MVID/
PDB/source/IL binding is retained; it does not qualify the blocked matrix.

[B25 handoff](owner-b-handoff-r25.md) retains the actual RED and release. A21 owns
the newly coordinated generic compiler investigation; no runtime/header/original
C/policy/product repair occurred in B25. Processed/AOT, emitted B23 controls,
profile products, full D10/runtime/upstream matrices and P6 remain open.

## Owner B23 — shared directory and raise sources ready; products unchanged

The authorized three-path repair is [SOURCE-READY](../artifacts/owner-b-r23-20261018/source-ready.json).
Linux x64 directory snapshots now use real host opendir/readdir with immediate
native errno and unconditional host-handle closure. Native/JIT/AOT each pass13
file/ancestor/slash/.., symlink/dangling, missing, access, loop, length and positive
iteration cases. Owned DIR tokens, rewind/close and disposal retain the existing
shared ownership model.

SIGINT registration now has one process-wide disposition/subscription, an
installing-context RetainLease and a separate Enter binding for each callback.
raise invokes the installed handler synchronously on the raising thread, with
no second OS signal; ignore, prior-disposition returns, error, reinstallation,
reentrant deferral and caller-context/errno restoration are exercised. Replacement
can retire a registration while an in-flight callback retains its owner.
Explicit C SIG_DFL uses the independently measured152-byte Linux x64 host
sigaction, saves/restores its complete representation on return, and selects real
kernel default before host raise. Native/JIT/AOT terminate by SIGINT before any
return marker. Blocked-signal controls prove defined action fields, all64 kernel
mask bits, unchanged thread mask and exactly-once pending delivery after restore.
Reserved sigset tail/padding are unspecified native output, not comparable state.

[Final controls](../artifacts/owner-b-r23-20261018/controls-r3/current-libc-results.json)
record71 commands/60 native comparisons,67 ownership assertions in each managed
mode, one real OS event after ten installs per mode and42 runtime unit passes,
zero failures/skips. Actual Libc PEs/PDB source checks and AOT response/reference/
object/executable bindings are [independent](../artifacts/owner-b-r23-20261018/actual-current-libc-inputs.json).
The original AOT default-return RED and native reserved-mask-tail control failure
remain preserved. Read-only upstream d3678b5c has the same two original defects;
no implementation was imported.

These are shared source controls. V13 resources and canonical193/286 still contain
the old runtime; no emitted after-proof or CPython repair pass is inferred.
Normal tool capture, unchanged-C direct/object JIT/AOT retries, socket/resolver
closure, both profile regenerations and fresh per-consumer D10/ABI/full runtime/
upstream/required UTF-8-skip/P6 gates remain open. No IDs, decorators or exclusions
change. SIGINT is process-wide; explicit default is terminal, and independent
third-party raw signal installation can race with host-action save/restore.

## Owner B22 — actual required cases execute; socket, directory and signal blockers reduced

Evidence/work: `owner-b-r22-20261018`. B independently verifies A17's fifteen
handoff references, 61 sealed source/evidence references, all286 current C/object
bindings and263 headers, exact V13 compiler318/runtime164 inputs and151 actual
resource bytes, both published PE/MVID/PDB/source closures,75 registrations
(73 nonempty initializers/two special NULL entries) and2,329 assets per form.
Two stale-PDB controls reject. Embedded193 remains the old V11 product.

Both new hostname forms have fresh source/IL/RVA/cache/lifetime proof:
1,522 constant properties/942,677 exact bytes/newarr0;6,253 static regions,
1,962 caches/9,068 uses,454 pins and nine individually bound lifetime exceptions.
Both execute all eight fresh native target layout measurements,24 common public
ABI fields, three explicit wchar differences and38 ABI assertions over two
lifecycles. Twenty-eight hostname/initializer/call-owner metadata negatives
reject; native real-hostname/wrong-host controls pass. All-property warmed
allocation/GC/static identity and complete interpreter matrices remain unrun.

The early actual raw-JIT receipt
[`20261018-051653-bc11d636`](../artifacts/campaign/20261018-051653-bc11d636/receipt.json)
builds a new owning consumer and independently audits its actual PE:
SHA256 `1169cf2a18333f228ff5623bb4b0f1af90dba8da1aa13bf4cdbfc4b5b88479be`,
MVID `0aead94f-dfbc-4774-848a-5f8b2fd24c3f`. Fresh actual AST/cache/lifetime,
IL/RVA and21 rejecting canonical-only/wrong-identity/empty-exception/initializer/
GETPID/hostname-call controls precede real execution. Independent semantic byte
comparison matches26,508 method bodies and1,618 RVA fields/2,332,915 bytes;
canonical metadata is never substituted for this different actual PE.

Real entry checks GETPID twice against ProcessId423287 and hostname twice
against the actual BCL host `marius-devtop`. Discovery completes all23 seeds,
5,134 unique IDs/5,142 occurrences, retaining all eight loader duplicates.
The real journal starts with GrammarTests.test_additive_ops passing.
[The actual suite result](../artifacts/owner-b-r22-20261018/actual-required-suite-result.json)
records1,999 unique executions:1,995 passed, one failure, one error, one required
skip and one interruption;2,868 required IDs remain unrun. The selected module
groups contained2,009 occurrences; ten not reached in the interrupted generator
group are not counted as executed. The command finishes naturally after173.529s,
exit134/outer1, without B timeout, cancellation or an external interrupt.

The first error is required BuiltinTest.test_compile_top_level_await. Its
asyncio self-pipe reaches socket._fallback_socketpair and upstream stub_socket:
undefined HAVE_SOCKET selects ENOTSUP, although the existing shared BCL socket
service is present. Actual source/IL in
[the A-owned selection handoff](../artifacts/owner-b-r22-20261018/priority-socket-source-selection.json)
proves that edge. Native and direct/object JIT/AOT real local TCP controls each
pass25 assertions; five wrong received-byte controls reject. No feature is enabled.

ExceptionTests.test_errno_ENOTDIR fails because os.listdir(regular_file) gets
ENOENT2 instead of ENOTDIR20. The unchanged C reduction proves native ENOTDIR
for a file, file/child and file/, and ENOENT for a missing child. All four emitted
direct/object JIT/AOT cells reproduce three wrong errnos while real-directory
and missing-entry controls pass. PosixFsLib.opendir returns ENOENT whenever
Directory.Exists is false. Runtime repair remains pending coordinated freeze
change; no runtime/header/tool capture or product regeneration occurs in B22.

SignalAndYieldFromTest.test_raise_and_yield_from returns FAILED rather than
PASSED, then a late KeyboardInterrupt interrupts formatting that assertion.
The untouched _testcapi helper calls raise(SIGINT) before generator.send.
Same-C native installed-handler execution completes before raise returns; all
four emitted direct/object JIT/AOT reductions instead observe delivery0 before
return and exit39. All three actual failing/interrupted required cases pass on
the same-pin native helper interpreter. The initial wrong native executable,
which lacks required helpers, remains a separate retained oracle-selection error.
Thirty reduction PE/MVID/reference negatives reject; every JIT/ILC entry, reference,
root, native object and executable is bound, with all76 runtime sections verified
in each of six generated routes. No RED result is called a product pass.

BuiltinTest.test_open_default_encoding remains a required skip under UTF-8 mode.
No upstream source/decorator/ID/exclusion, skip or expected-failure classification
changes. B22 returns RELEASE-ALL after sealing actual byte inputs. Directory and
signal repairs plus A-owned socket selection are priority before new193/P6 work.
Remaining upstream cells/full raw retry, all-property data/GC/static-identity,
interpreter/thread/lifecycle/owning/rooted/trimmed/delivery/audit matrices, separate
new193 and P6 remain open. P0 is in progress; P1–P6 stay unpassed.

## Owner B21 — GETPID executes; actual discovery reaches hostname binding gap

Evidence/work: `owner-b-r21-20261018`. B independently rehashes the A13 handoff,
1,207 publication references, complete V13 closures/318 compiler inputs/164
runtime inputs, all286 ordered unchanged C inputs/options,263 headers,75
registrations (73 non-NULL/two legitimate special NULL entries),2,329 assets and
both current published PE SHA/MVIDs. Embedded193 stays on V11; no result transfers.

Both new GETPID forms pass fresh actual-source/PE IL/RVA and semantic/cache/
lifetime review:1,521 named properties/942,622 exact bytes/newarr0;6,253 regions,
1,962 caches/9,068 uses,454 pins and nine concrete lifetime exceptions each.
Lifetime call closure is L0/L1637/L320/PinAndRoot3; missing pinned/new borrowed
callers reject. Both canonical PEs execute all eight freshly built native target
layout values. Both also pass24 common native public fields, three deliberate
wchar differences and38 ABI operations over two real lifecycles. These are JIT
layout/ABI and source/data reviews; all-property warmed/GC/static-identity and
the complete interpreter matrix remain unrun for these new bytes.

Framework attempts041629-3ca24474 and041846-2fbb4ad8 both stop before an Upstream
run command at a B-added negative-control exception-class identity mistake.
The first correction touched a neighbouring catch; both failed envelopes and
exact executed source variants remain. The precise catch is now corrected with
the exact expected error preserved. No product failure/test execution is inferred.
The already-built041846 consumer is retained without a third rebuild/analyzer.
`actual-resume/execution.json` binds the immutable build receipt and exact same
current source/actual PE/MVID/address report, fresh metadata/IL/RVA, unchanged
strict qualification and eight rejecting wrong-MVID/bytes/empty/canonical-only/
GETPID-call-edge controls before executing the full frozen entrypoint. Unexpected
exceptions still propagate. Original receipts are immutable.

[Actual translated execution](../artifacts/owner-b-r21-20261018/actual-resume/execution.json)
prints `GETPID bootstrap passed:398939`: two owning API calls equal the real
Environment.ProcessId. Actual input SHA256
`b5d96737e264e1109b5fc78c68aa7ac0ce1772cf73287d46577dc404dd3f64d5`,
MVID `6e3fcc75-d40b-4f4c-ab7a-c79aed526e42`, has its own full source/IL/RVA/AST/
cache/lifetime proof. Independent semantic comparison matches26,505 method
bodies and1,617 RVA fields/2,332,829 bytes, allowing only MVID/RVA-location
metadata differences. This is actual executable entry, not build intent.

Discovery now passes22 of23 seeds, finding4,996 unique IDs/5,004 occurrences.
The sole blocked seed is test.test_builtin: class line1724 calls platform.system;
its absent-os.uname fallback reaches platform._node:698 and raises AttributeError
because socket.gethostname is absent. The actual command exits134, outer driver1.
No required case starts; assertions0/all4,867 required IDs remain unrun and no
case journal is fabricated. The complete report/traceback/nonzero exits are in
[the blocker handoff](../artifacts/owner-b-r21-20261018/first-actual-upstream-blocker.json).
No source, decorator, frozen ID, exclusion or skip/xfail classification changes.

The existing unchanged gethostname shared service independently passes native
and V13 direct/object JIT/NativeAOT:51 checks each, two real workers, typed
int(char*,size_t), actual /proc host identity, zero/tight/exact/NUL/sentinels,
LP64 wide capacity and thread-local errno. Wrong hostname rejects in all five
runs. Actual PEs/MVIDs/ILC references/roots/native objects/executables are bound;
ten wrong-identity/reference controls reject. Both raw routes match76 current
runtime source sections. Actual service IL calls System.Net.Dns.GetHostName;
the CPython wrapper is absent under unselected HAVE_GETHOSTNAME. No shared
service ABI or missing-implementation defect is reproduced; no runtime repair,
header/policy edit, os.uname bypass or network/process capability enable occurs.

The same-pin native oracle, with only os.uname absent in its disposable process,
executes the real fallback and imports test.test_builtin. Removing gethostname
reproduces the exact AttributeError and exits22. The native fallback returns
sys.platform (`linux`), not ordinary uname.system (`Linux`); the original wrong
uppercase expectation and failed oracle log remain with this correction.
A15 owns independent feature-policy/source/layout controls after coordinator
grant. B21 seals exact input copies and RELEASE-ALL at the completed boundary;
all remaining upstream cells, full D10/interpreter/thread/lifecycle/rooted/
trimmed/delivery/audit matrices, new193 and production P6 stay open.

## Owner B20 — fresh V13 profile gates pass; actual upstream stops at getpid

Evidence/work: `owner-b-r20-20261018`. B independently verifies A11's sealed
handoff, all112 evidence bindings, exact V13 tool closures/318 inputs and both
current publications. Embedded193 remains V11 and receives no new qualification.
The new286 receipt is `20261018-031856-ab7e6848`; all286 ordered C inputs,
effective policy/header/D/U,263 headers,75 registrations (73 non-NULL and two
legitimate special NULL entries) and2,329 assets are freshly checked per form.

Fresh actual raw/processed D10 proof has6,253 regions,1,962 caches/9,068 uses,
454 pins and nine individually reviewed lifetime exceptions per form. There are
1,520 named properties/942,568 exact RVA bytes/1,693 getter calls/newarr0 and
zero unresolved pure sites, unknown bindings or semantic errors. Actual lifetime
edges are L0/L1637/L320/PinAndRoot3; missing pinned callers/new borrowed callers
reject. Source-bound warmed allocation, GC/static-identity execution and the
complete interpreter matrix remain separate and unrun for these new bytes.

Both actual published PEs execute all eight layout observations against a fresh
native oracle built from this receipt's target headers. Both also pass24 common
public ABI fields, the explicit three native-wchar4/target-unsigned2 differences,
and38 real refcount/immortal/slot/varargs/vectorcall assertions over two lifecycles.
These JIT reference consumers load byte-identical canonical PEs; no field-presence
or old-profile evidence is substituted for measurement.

The early actual owning raw-JIT upstream retry
[`20261018-033841-b2749980`](../artifacts/campaign/20261018-033841-b2749980/receipt.json)
fails after136.2 seconds. Its actual PE is SHA256
`dc50d14851fa3229a5517df854e06e8792221a67fef2960760518aecc1c912eb`, MVID
`5d0c3c5c-5125-4529-9693-be9f943b2167`. Fresh source/IL/RVA/AST/cache/lifetime
review precedes execution; canonical-only different-PE, wrong-MVID and empty
exception controls reject. Independent exact semantic comparison covers26,501
method bodies and1,616 RVA fields/2,332,759 bytes, allowing only MVID/RVA-location
metadata differences. Eight of23 seed modules discover1,191 IDs; all15 blocked
modules end at absent `os.getpid`, first in `test.support.os_helper:22` during
`test.test_grammar` import. Executed assertions0; all4,867 required IDs remain
unrun. Discovery fails before the runner opens a case journal; its complete
report, selected/unexecuted IDs, tracebacks, exit134 and outer exit1 are retained.
No subset, decorator, exclusion, expected-failure or skip reclassification occurs.

Native and exact-V13 direct/object JIT/NativeAOT getpid controls each pass54
checks with two genuine workers. A typed int(void) function address returns the
positive real host PID independently supplied by the launcher across exec;
repeated identity, worker context/return and per-thread errno preservation pass.
Five dynamically wrong expected-PID controls reject. Every actual PE/MVID,
ILC entry/reference/root/object/executable is bound; ten wrong-MVID/bytes/empty
reference controls reject. Both raw routes match all76 selected runtime sections.
The existing shared service and header are unchanged. Actual source/Clinic guards
select `os.getpid` only with absent HAVE_GETPID: A12 owns that policy gap.

An owning diagnostic reuses the exact failed upstream product/facade bytes. It
records actual os/time callable inventories, confirms getpid/getppid/getgroups
absence, and executes formatter epoch bytes, wait exitcode42 and waitpid EPERM.
Other missing methods are observations, not feature-selection authorization.
Child execution remains0. All92 audit and23 reporting controls pass/zero skips.
The bounded group now returns its lane with a substantive handoff and independent
three-lock release. No runtime/header/compiler/policy/product change, canonical
regeneration, old193 matrix, commit, staging or nested agent. Fresh remaining
upstream cells, allocation/GC/static identity, full ABI/thread/lifecycle/rooted/
trimmed/delivery/audit matrices, final193 and production P6 remain open.

## Owner B19 — unchanged-C V13 service proof passes; lane released for A11

[B19 results](../artifacts/owner-b-r19-20261018/v13-after-results.json) repeat
the SAME original `strftime_profile_r18.c` and `wait_profile_r18.c`, retaining
their B18 hashes and all C assertions. Both native controls and all **16**
raw/processed × direct/object × JIT/NativeAOT service cells pass. Strftime
matches native exactly: ordinary19, wide19, tight0, empty0, failures0. Literal
capacities64/4/1 now select the real size_t/ulong boundary; the legacy managed
int overload remains. Wait status decoders correctly distinguish exited,
signaled, stopped and continued values. Wait/waitpid still return-1/EPERM with
status305419896 untouched; native no-child ECHILD remains explicitly different.

Independent checks bind the exact V13 driver/library/postprocessor PEs and
MVIDs, complete recursive dependency closures,318 compiler inputs,164 frozen
runtime/header inputs and independent A10 source copies. All151 actual CLR
resources are decoded and compared byte-for-byte (83 runtime,68 headers), and
all76 selected runtime sections match in each raw direct/object emission.
Processed cells use ordinary semantic snapshots of those unchanged raw projects.
Every actual JIT/ILC-selected PE has SHA/MVID/entry binding; AOT bindings retain
the real response, all selected references, roots, native object and executable.
Independent copies preserve executed inputs. Forty wrong-MVID/wrong-byte/empty
ILC-reference controls reject. The actual shared-runtime PDBs bind all83 source
documents/checksums rather than borrowing another assembly's metadata.

The existing real-source compatibility harness again passes **233 assertions
in each JIT/AOT mode**, including typed function pointers, legacy calls,
ulong-wide capacities, bounds/NUL/errno/sentinels and wait rejection. All **27**
focused actual-source runtime units pass with zero failures/skips. Original V11
runtime failures, V12 CS0266, diagnostic-driver failures and the first actual
upstream report remain unchanged and independently copied; none is reclassified.

Only B's harness/evidence and progress documentation changed. Runtime164,
compiler/tool captures, root tools, canonical193/286 and unrelated dirty work
remain frozen. The [release](../artifacts/owner-b-r19-20261018/serial-slot-release-all.json)
records completed commands, no active/queued B work and three independent lock
acquires/releases. A10 is terminal; A11 requires coordinator verification and
its own explicit bounded policy/native-control grant, followed by separate286
and193 regeneration slots. B enabled no feature policy and regenerated no
product. All4867 frozen required upstream IDs remain unexecuted. New193/286
actual bootstrap, ABI/D10/thread/lifecycle/full matrix/rooted/delivery/audit,
production/provider/reproduction and P0–P6 gates remain open.

## Owner B18 — fresh profile proof; actual bootstrap failure and bounded service repair

Evidence/work: `owner-b-r18-20261018`. Entry independently verifies all199 A9
artifact bindings, both current193/286 publications,607 preserved paths and
runtime164. Fresh embedded193 strict canonical/source/PE/D10/46-builtin preflight
passes both forms, with old-V9/wrong-MVID/empty-exception rejection. This is
pre-mutation evidence; no fresh embedded interpreter matrix has run.

Fresh286 independently passes both-form source/config/header/helper readiness
(all286 units,75 initializers,263 headers), actual PE/MVID IL/RVA proof and full
address/lifetime review. Each form has6239 regions/1960 caches/9052 uses/454 pins/
9 concrete exceptions and1511 constant properties/940261 exact bytes/newarr0.
Actual L16 incoming edges are37, with L0/L320/PinAndRoot3; no193 exception/proof
was inherited. Dropped pinned-call/new borrowed-span controls reject.

The first actual owning raw-JIT upstream receipt
`20261018-023127-d2d83fe8` fails normally after129.3 seconds. Its exact consumer
PE receives fresh IL/RVA/address/cache/lifetime review and wrong-MVID/empty-review
controls before execution. All23 seed imports then block: `test.support` needs
absent `time.strftime`, while asyncio/subprocess needs absent `os.waitpid` and
the wait-status surface. Discovered/executed0; all4867 required IDs remain
unexecuted. The complete report, original traceback and nonzero exit remain;
there is no translated unittest pass or case/decorator/exclusion change.

Native/current-immutable-V11 emitted controls independently reproduce two shared
defects: strftime capacity2147483713 yields native19/emitted0 (exit23), and
stopped0x137f/0x7f plus continued0xffff wrongly satisfy WIFSIGNALED (exit24).
Both wait services already reject-1, preserving status; native ECHILD differs
explicitly from managed EPERM. Coordinator-authorized repair changes exactly
CalendarLib.cs/time.h/sys/wait.h: real LP64 ulong count/capacity, correct size_t
prototype and signed-char status discrimination. Existing managed int callers,
format bytes/NUL/bounds/errno and unsupported-child rejection remain.

After repair, actual shared Libc direct JIT/NativeAOT each pass233 assertions,
and27 actual-source focused unit cases pass/zero skips. GCC with the preserved
old shared wait header fails24; the identical control with the repaired header
passes0. Same-pin native Python confirms bootstrap imports and rejected stopped/
continued status conversions. The ordinary full test-project attempt cannot
compile against the stale root compiler DLL (AggregateIdentity absent); its
failure is retained. The focused unit project compiles the original runtime
test files and new regression without a compiler dependency/build. Initial
native fixture format-warning failure is also retained.

Runtime mutation is exactly the three coordinated files; the other161 frozen
inputs remain unchanged. Sources are frozen SOURCE-READY for A10's normal fresh
immutable tool/header-resource capture. B independently verifies A10's V12 full
dependency/source handoff and extracts all151 actual length-prefixed resources
(83 runtime/68 headers), byte-equal to the frozen sources. With those resources,
the unchanged wait C reduction passes native and all four direct/object JIT/AOT
cells, including stopped/continued classification and -1/EPERM/status ownership.
The unchanged strftime reduction passes native (ordinary19/wide19), but its first
fresh emitted JIT build fails CS0266 at capacities64/4/1: emitted direct calls
select the retained managed int overload despite the C size_t formal/result.
The strftime emitted executions and other three cells remain unrun. No generated
source or frozen runtime repair was changed to bypass this compiler blocker.

A separate external-override overload diagnostic passes native/emitted: its
generated typed ulong wrapper selects the correct overload. This bounds the
observed defect to direct runtime calls; the private diagnostic driver's wrong
expected exit is retained with explicit adjudication. The wait driver's original
post-pass report-reading error is retained too; exact successful commands were
reused, with no repeated test or generated-source edit. V11 remains before
evidence only. A10 owns the compiler defect and consistent feature-policy
selection. No canonical regeneration/publication, compiler build, resource patch,
staging or commit by B. New193/new286/frozen assertions, allocation/GC/ABI/full
matrix/rooted/delivery/audit, downstream and P0–P6 remain open; prior proofs do
not qualify the changed runtime/header/configuration.

## Owner B17 — all20 V9 actual inputs reviewed; RELEASE-ALL for A9

Evidence/work: `owner-b-r17-20261018`. The active retrospective group completed
normally: **20/20 distinct actual JIT/ILC input PEs** pass fresh strict review,
including all **12 previously pending** inputs and revalidation of the8 held
proofs. The original useful-consumer/ABI/behavior/rooted/delivery executions are
retained at their own receipts; no completed interpreter group was rebuilt.
`consumer-input-review-index.json` links every exact-input report and command.

Every input binds its actual PE SHA/MVID, metadata, initializer IL/RVA bytes,
source spans, tracked address/cache uses, pins and complete lifetime closure.
Each retains1,233 assembly-data properties/897,429 exact RVA bytes/1,338 getter
calls/newarr0 and4,632 regions/1,465 caches/8,024 uses/383 pins/9 concrete lifetime
exceptions. Independent semantic-byte comparison covers each input. The58
negative outcomes are preserved:18 different-PE canonical-only proof rejections,
20 wrong actual MVID rejections and20 empty lifetime-exception rejections.
Source-bound canonical warmed allocation/GC probes retain their data-only scope;
no new193/new286 or final-tool qualification is inherited.

The never-launched B16 prepared wrapper had one real selector bug: it counted
`-p:CPythonProject=...csproj` as a positional consumer project. B17 retained the
original failure/script and reduced it against all20 actual build commands;
its private copy now selects the single positional project. The strict auditor,
consumer evidence reader and immutable analyzers are unchanged. No product,
runtime/header/Host/API/compiler source was repaired or regenerated.

B authored `tests/Audit/profile_evidence.py` and seven publication-binding
controls for the separate286 profile. The existing upstream hook now requires
that profile's explicit receipt/source/lifetime index and freshly reviews every
actual JIT/ILC input before assertions. Upstream reporting checkpoints preserve
completed groups and required-unexecuted IDs if a later operation fails; the
per-case journal still identifies the in-flight case. **92 audit controls** and
**23 upstream harness controls** pass/zero skips; these are harness controls,
not translated upstream assertions.

At the coordinator's A9 slot request, no new long queue was launched. B17's
`profile_preflight.py` remains prepared/unrun, as do its new286 D10/ABI/upstream
cells. A8's published286/helper/fork controls retain their own scope. No actual
compiler-invalidating blocker was observed in B17's executed group; the unrun
new-profile gates remain unknown, not passes. Frozen required skips/xfails,
encoding adjudication, all new-profile upstream cells and P0–P6 remain open.

B independently verified every byte and independent inode of A9's34,712-file,
8,285,479,634-byte V9 backup, including canonical sources/bin/obj/assets, complete
V9 translation and A5 evidence/tool/probe trees, and all20 consumer input trees.
B also copies its newly completed proof artifacts. `v9-explicit-input-handoff.json`
binds those backups and actual inputs. Active reviews used original paths and
are finished; future historical revalidation must bind the copied byte set,
never merely substitute a path. `serial-slot-release-all.json` records runtime164,
compiler318, canonical V9/286 preservation, owned processes/queues and independent
three-lock release. A9 publication still requires coordinator verification and
explicit grant. B build queues stay paused until explicit slot return; final
production/provider/AOT/downstream/reproduction and campaign acceptance stay open.

## Owner B16 — four-cell V9 delivery/audit passed; lane returned for A8

Assignment `cpython-impl-v9-delivery-audit-b16-20261018`; evidence and private
work: `owner-b-r16-20261018`. Entry verified A7 release SHA
`361140dbbf66804177dd8df8fba90693fa63d46e3faa71edf7022c5697999577`,
all three locks,588 preservation paths,164 frozen runtime inputs and the exact
V9 receipt/source/PE/MVID pins. No compiler build, canonical regeneration,
publication, runtime/header/Host/API change, staging, commit or nested agent.

Receipt `20261018-011626-7c6fa428` passes **all four delivery cells** and **all
four ordinary-delivery audit cells**. Every delivery runs the useful workflow
for two cycles outside the repository with an empty executable PATH and poisoned
ambient Python paths; normal and missing-stdlib/json/application contract
scenarios pass. Assets, notices, static46 builtins, full193 source/build closure,
managed/native dependencies and exports are audited against retained V9 inputs.
The exact command, environment, logs and individual reports are in
`delivery-audit-command.json` and the framework receipt.

B repaired a real audit coverage gap: the old hook checked actual JIT PE/D10
proof but only assets/ELF dependencies for AOT. The existing hook now independently
selects each actual ILC response's product PE and binds the response, entry
assembly, native object and executable; it runs the existing IL/RVA and immutable
semantic analyzer for those exact bytes. JIT/AOT evidence has distinct paths.
All strict reader guards remain unchanged.85 audit controls pass/zero skips
(76 existing plus9 meaningful input-selection controls). The retained initial
new-test failure used a different dynamically imported exception class; stable
RuntimeError assertions retain exact rejection diagnostics.

The four fresh useful delivery inputs pass1,233 payload properties/897,429 exact
RVA bytes/1,338 getter calls/no getter newarr,4,632 regions/1,465 caches/8,024
uses/383 pins/9 concrete lifetime exceptions. Canonical-only semantic reviews
for different PEs reject before fresh analysis. The existing canonical warmed
JIT/AOT allocation/GC evidence retains its source-bound data-only scope; it is
not relabeled interpreter execution. Actual relocated AOT execution is separate.

`remaining-consumer-input-review.json` pins **20 actual inputs**:8 have held
exact-byte strong proofs (B14 useful JIT2, B15 ABI AOT2 and B16 delivery-consumer4),
while **12 other inputs still need strong review** (B14 ABI/behavior JIT4,
B15 behavior/rooted AOT4 and B16 delivery-contract4). The prepared
`build/owner-b-r16-20261018/review_inputs.py` was **never launched** when the
coordinator requested A8's next bounded slot. No complete V9 matrix is claimed.
B15 ordinary AOT004820 reused one project-local response: only processed remains.
Its executions are preserved; no missing raw response is invented. The new
isolated useful delivery AOT cells provide fresh, fully bound replacements.

Concurrent A8 authoring changed `pyconfig-policy.json`, `prepare.py` and
`test_profile.py`. Entry copies, before/after hashes and diffs are retained in
`a8-concurrent-policy-drift.json`. Read-only dependency review establishes that
this action=test selected only delivery/audit, invoked no translation/helper
preparation, and used unchanged source/sources/embedding/stdlib manifests plus
retained V9 private C/config/source/PE inputs. The standalone D10 inventory does
not import those changed preparation entrypoints. `v9-audit-policy-boundary.json`
records this scope, not a profile/layout equivalence assumption. Any HAVE_FORK
change needs fresh layout/thread/callback/source/PE qualification.

The active command completed normally before stopping all new queues.
`serial-slot-release-all.json` records actual results/pending work, commands,
source/PE/runtime pins, process state and genuine independent three-lock release.
A8's <=15min reuse-V11 probe slot remains subject to coordinator grant; whole286
and new193 are separate. Resume remaining B qualification in a fresh round only
after explicit slot return. No compiled full286 profile exists; CPY-P002,
frozen upstream/native skip/xfail/encoding adjudication, final stable-tool193,
production/provider/AOT/downstream/reproduction and P0–P6 remain open.

## Owner B15 — V9 NativeAOT execution; serial slot returned to A7

Assignment `cpython-impl-v9-aot-qualification-b15-20261018`, codex / gpt-6.1-sol /
xhigh; evidence `artifacts/owner-b-r15-20261018`. Entry verified A6 RELEASE-ALL,
all three locks,564 preserved paths,164 frozen R13 inputs and the exact V9
233444 receipt/canonical source/PE/MVID pins. No runtime/API/compiler/generated
source mutation or regeneration; no commit, staging, push or nested agent.

Fresh004135-66a38985 passes both-form interpreter NativeAOT ABI (38 assertions /
two cycles /24 common native layouts, explicit wchar4 versus unsigned2), behavior,
threads and lifecycle (5/2/2 Python groups per cycle /two cycles plus owning error,
trace/cancellation/reuse/reference/callback/drain assertions). Both whole-library
and facade-rooted AOT runs pass all9 groups/two cycles; actual ILC response files
contain both assembly roots. Normally trimmed useful AOT consumers pass both
forms/two cycles in004820-cdd32be2 under the required outer shared flock.

Private build paths produce different PE/MVIDs. Canonical-only D10 proof correctly
rejects those actual ABI ILC inputs. Independent comparison proves raw19,207 /
processed19,205 exact IL/EH/locals bodies and all1,281 RVA fields/1,637,980 exact
bytes; metadata differs only in MVID/FieldRVA locations. Fresh actual-PE IL/RVA and
unchanged semantic-analyzer runs pass the strict gate in both forms:4,632 regions,
1,465 caches/8,024 uses/383 pins/9 concrete lifetime exceptions. Wrong MVID and
empty exception reviews are rejected. The canonical source-bound warmed probes
retain their explicit data-only scope; they are not interpreter AOT results.

B changed only the test audit hook and added `tests/Audit/consumer_evidence.py`:
fresh analysis binds the actual consumer PE, preserving exact source-span reasons
only when actual lifetime IL edges also match. No auditor guard changed.76 audit
controls pass; the new path passes both real input controls above. Initial private
recorder status mismatch and every original/expected rejection remain retained.

At the coordinator's A7 slot request, the current proof group completed normally.
`serial-slot-release-all.json` records actual completed/pending work, pins and
genuine lock/process observations. No B build is queued. Four-cell delivery and
complete audit have **not run**; remaining per-consumer PE review also stays open.
Resume only on explicit slot return. Full286 remains unpublished/link-blocked;
frozen upstream/native-marker policy, final-tool193/downstream/reproduction and
P0–P6 remain open. B14's completed JIT evidence is preserved without repetition.

## Owner B round 14 — mapping disposal diagnosis and independent V9 qualification

Assignment `cpython-impl-mapping-qualification-b14-20261018`; actual T3 task ID
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-mapping-qualification-b14-20261018`.
Local date 2026-10-18; codex / gpt-6.1-sol / xhigh confirmed by live thread
configuration. Private work/evidence: `owner-b-r14-20261018`.
All164 R13 runtime inputs match before work; original HEAD remains bb2f9b1.
A6 owns compiler/config/scripts/NativeOracle/full193/full286/publication.
No nested agents, commits or competing compiler build.

Primary diagnosis is complete: [mapping-diagnosis.json](../artifacts/owner-b-r14-20261018/mapping-diagnosis.json)
preserves the original isolated1/1 failure, source copies, TRX and full strace.
At lines17342/17367/17370/17381 the retained anonymous mapping is created,
successfully unmapped at owner disposal, replaced at the exact address by the
CLR's `/memfd:doublemapper`, then observed by mincore0. The old assertion
required an unmapped address to remain unused. No service leak is established.

The corrected ordinary test retains anonymous partial/foreign-owner assertions
and identifies retained file storage in kernel `/proc/self/maps` before/after
owner disposal. A unique live backing file prevents unrelated address reuse
from satisfying the retained-storage observation. New R14 lifecycle controls
cover descriptor close, surviving partial intervals, overlapping holes, active
recursive binding disposal rejection, foreign owners, ambient-owner isolation,
idempotent disposal and shared backing bytes. No private registry/counter oracle,
skip, process reset, test shim or runtime mutation. Native kernel control19
assertions also demonstrates mincore0 on deliberately reused storage while the
original file mapping is absent. Actual current-libc direct JIT/NativeAOT pass;
affected ordinary units41/41, zero skipped. Commands and results are under the
round's private artifacts; all164 R13 runtime inputs remain unchanged.

Independent unchanged strong B preflight passes both released V9 forms, binding
the expected233444 receipt and evidence-index SHA, actual canonical Release PEs/
MVIDs, all source spans,1,465 tracked caches/8,024 uses,4,632 storage regions,
383 pins and all9 concrete remaining lifetime exceptions. Fresh metadata confirms
L=0/L16=36/L32=0/PinAndRoot=3 actual edges per form. Named payload IL/RVA and
source-bound JIT/AOT warmed-allocation/semantic evidence also qualify; these data
probes do not execute the full interpreter. `independent-v9-preflight.json`
records the exact scope. Independent fresh metadata from the actual new useful
consumer PE copies also passes every unchanged strong gate for both forms:
`independent-consumer-pe-review.json`. All9 source-span-bound lifetime exceptions
were reviewed; the three distinct pinned storage owners, pinned destination
callers, changed-base L16/L32 misses and tracked ref-byte helper retain their
concrete ownership requirements. Borrowed L helpers have zero actual PE edges;
no lifetime guarantee for arbitrary passed spans is inferred.

Fresh useful-consumer JIT receipt002105-dfd6562f passes both forms/two cycles
with exact JSON/three callbacks/return/output/traceback/finalization. Runtime
sources and both actual PE hashes/MVIDs remain V9. The first ABI attempt002224
fails before compilation because B's private process wrapper supplied only
environment overrides; inherited environment is restored in its corrected copy.
The next002307 preserves a real authored harness build failure: Abi.csproj's
recursive source glob included the R9/R10/R13/R14 separate service-project
entrypoints. The B-owned project now explicitly compiles its own Program.cs;
separate service projects and every193 product unit remain unchanged. Before/
after source hashes are in `abi-project-source-fix.json`. Fixed-project ABI
receipt002515-6c265612 passes raw and processed JIT:38 assertions/two cycles per
cell, all24 common native layout observations, actual refcount/immortal/slot/
varargs/vectorcall behavior; native wchar4 versus target unsigned2 stays explicit.
Fresh behavior/thread/lifecycle JIT receipt003012-c176f7bd passes both forms,
two cycles each:5/2/2 Python groups per cycle plus owning value/error/callback/
trace/cancellation/reuse/worker-drain checks. All executed assertions ran on the
translated interpreter. These are fresh V9 results, with no runtime mutation.

**Serial-slot RELEASE-ALL:** the in-flight short JIT wrapper completed normally;
no B shared/native/dotnet/compiler/emission/publication command remains running
or queued. `serial-slot-release-all.json` durably binds commands, completed and
retained failed receipts, expected V9 receipt/source/actual PE pins, all164
unchanged R13 inputs, process/lock observations and outstanding gates. A6 may
take the campaign/shared slot for complete286; canonical embedded193 remains V9.
The prepared AOT/rooted/delivery/audit script has **not** been launched and will
remain unrun until explicit A6 completion/slot return. Consumer NativeAOT,
remaining four-cell/runtime/rooted/delivery/audit, published286 frozen upstream
execution, final-tool193/downstream/reproduction and all P0–P6 gates remain open.
No historical matrix pass is inherited; no commit, push or nested agent.

R12 guide/helper readiness and R13 shared secure-erasure closure are completed.
Final service READY/RELEASE-ALL is recorded; product/D10/P0–P6 remain open.
No commit made.

## Owner B round 13 — shared secure erasure READY; RELEASE-ALL

Assignment **OWNER B ROUND13**; runtime session
`01a14b77-4ff7-7241-aaae-0108af8b1d51` (no supplied T3 delegate task ID).
Evidence/work: `artifacts/owner-b-r13-20261017`, `build/owner-b-r13-20261017`.
R12 guide/helper-role work is completed. A retains sole compiler/linker/
postprocessor/config/scripts/NativeOracle/full193/full286/publication ownership.
B built no compiler, changed no profile macro, and ran no old-product matrix.

**Final service READY and explicit RELEASE-ALL:** all B shared/native commands
have completed; none is queued. A may now select the existing upstream
`HAVE_EXPLICIT_BZERO=1` branch and retry whole286/final193 with its normally
captured V8 resources. Final hashes:

- `DotCC.Libc/SecureZeroMemoryLib.cs`:
  `0ba69773d63fcadfd77493bd6df1e732b77cc662b3e6a6870698042b2485b9fb`.
- `DotCC.Lib/include/string.h`:
  `498320a477829c6eb90c446953a88767fdfb3371bf4274788f30150d0eeb27bb`.

`final-ready-release.json`, `source-bindings-final.json` and
`final-runtime-hashes-r13.json` bind the source/commands/results to A's immutable
V8 handoff SHA `a7de7f560341b4ad9254aadbb5264d86b16491b85f0730fb1cce6dada310f363`.
All314 preserved source copies and current inputs match; tools remain provisional.
The source-freeze exception is exactly this service/header:162 of163 original
R10 inputs remain unchanged, string.h is the one intended change, and the new
service makes164 runtime/header/Host/API inputs. No other frozen source changed.

Actual all286 receipt225558-3de551e6 first fails280-blake2module at unmodified
`impl/blake2-impl.h:156`'s fallback asm;281/282 cancelled, no publication.
The native valid branch passes30 checks while V6 lacks the shared declaration
(requireMatch fails, exit2). The implementation adds ordinary
`void explicit_bzero(void *, size_t)` and routes exact writable ranges through
BCL `CryptographicOperations.ZeroMemory`. It validates the complete native
range before any store, accepts null/zero length, preserves errno, and splits
LP64 counts into int.MaxValue spans without narrowing or pointer escape.
Unmanaged storage or the caller's pin must remain live through the call.
This changes no hash engine, upstream C/asm, allocation destination or D10 rule.

Fresh final results (overlapping controls are not summed):

- Same unchanged upstream header branch: native/emitted JIT/emitted NativeAOT
  each30 ordinary assertions and35 in the wide run. Wide clears exactly
  **2,147,483,712 bytes** (int.MaxValue+65), fully scans them and checks both
  guards. Stack/heap/static/unaligned/zero/end/control-flow/errno checks pass.
- Direct actual shared runtime: **40 assertions each JIT and NativeAOT**;
  fixed managed storage survives forced compacting collections, two workers
  retain separate buffers/errno, impossible/wrapping ranges reject before
  stores, actual void/ulong function pointer works, and10,000 warmed erasures
  allocate zero bytes. The ordinary unit selection passes **63/63, zero skips**
  (five new methods plus memory/string/allocation/ownership regressions).
- Ordinary functional-discovery fixture: **1/1, zero skips**,30 semantic C
  assertions, existing harness plus actual V8 compiler resources; no B tool
  build or generated-code repair.
- Same-pin unchanged `blake2b-ref.c` and `blake2s-ref.c`: **50 assertions each
  native/JIT/NativeAOT**,two algorithms/eight native-oracle digest vectors,
  empty/abc/200-byte/keyed inputs, streaming/state copy/reset/errors and state
  erasure. Normal separate object emission/linking retains both real engines.
  All21 BLAKE2 source files match the exact-pin archive (`actual-source-pin.json`).
- Exact failed280-blake2module command now emits0. Only immutable CLI/output/
  report destinations and `-DHAVE_EXPLICIT_BZERO=1` differ. Original source,
  include order and ordered -D/-U roles remain; this is one actual-unit replay,
  **not** a whole286/profile/runtime pass.

`non-elision-proof.json` binds actual Release PE/MVID/signatures/call graph and
optimized JIT/NativeAOT disassembly. Dead stack keys are never read after erasure;
both code generators retain the conditional erasure call. Actual service IL has
void/pointer/ulong signature0002010F010B and calls BCL ZeroMemory, whose actual
compiled path calls Span.Clear and real zero stores. Both unchanged algorithms'
key-init paths retain erasure in optimized AOT, even after their key block's last
use. Native disassembly verifies real glibc explicit_bzero (separate unfortified
control; original optimized oracle uses glibc's checked form). This is erasure
non-elision evidence, not a new constant-time or broader security claim.
The [BCL contract](https://learn.microsoft.com/dotnet/api/system.security.cryptography.cryptographicoperations.zeromemory)
and [10.0.12 implementation](https://github.com/dotnet/runtime/blob/v10.0.12/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/CryptographicOperations.cs)
explain its NoInlining/NoOptimization protection; post-read tests alone are not
used as non-elision proof. AOT ELF dependencies remain libc/libm/ELF loader.

Retained failures: the initial before-control recorder used the wrong log-key
and was corrected in before-v2; initial BLAKE2 native harness needed an explicit
uint8_t pointer cast under -Werror. V7's service embedded build fails because
runtime splicing strips imports; source-v2 fully qualifies the BCL call, and A's
fresh V8 resolves it normally. No emitted C# was edited. The initial direct log
destinations were reused for the source-v240-check rerun, so source-ready-v1's
command digest is historical, not asserted current; source-ready-v2 and final
hash bindings identify the actual current evidence. Every failed V7/before/
BLAKE2 attempt remains separate. Existing VectorIo warnings remain visible.

Reproduce into fresh output leaves (compiler acquisition/build stays with A):

```bash
python3 -B cpython/tests/Abi/validate_secure_zero_r13.py --embedded-runtime --work cpython/build/owner-b-r13-repro --artifacts cpython/artifacts/owner-b-r13-repro --tools cpython/build/owner-a-r5-20261017/c031-r13-v8-tools --blake2 cpython/build/campaign/20261017-225558-3de551e6/source/Modules/_blake2/impl
python3 -B cpython/tests/Abi/validate_blake2_secure_zero_r13.py --work cpython/build/owner-b-r13-blake2-repro --artifacts cpython/artifacts/owner-b-r13-blake2-repro --tools cpython/build/owner-a-r5-20261017/c031-r13-v8-tools --source cpython/build/campaign/20261017-225558-3de551e6/source --failed-receipt cpython/artifacts/campaign/20261017-225558-3de551e6/receipt.json
```

These specialist helpers lock each native/shared command separately; do not
add an outer lock. Exact current commands are in final-v8/commands.json,
blake2-v8/commands.json, extra-commands.json, proof-commands.json and
ordinary-functional-command.json. No interpreter/native-helper rebuild,
publication, staging, commit, push, reset/cleanup, worktree or nested agent.

Preservation-after.json confirms all25 unrelated dirty inputs, six frozen
upstream JSON files, PLAN, usage guide and HEAD unchanged. Concurrent A compiler/
configuration/recipe/proof work remains preserved. `r13-summary.json` indexes
the actual evidence. Full193/286, final D10 address/cache/exception and payload
IL/RVA/warmed allocation/identity proof, fresh four-cell ABI/behavior/thread/
lifecycle/trimmed/rooted/delivery/audit and actual frozen upstream assertions
remain open. B awaits explicit final product READY/run.lock release; short JIT
ABI/useful consumer precedes expensive cells. Native marker/default-encoding/
UTF8 adjudication, final stable-tool SQLite/QuickJS/picotls/MsQuic/provider/AOT/
affected Blink/libsmb2 and reproduction/P0–P6 acceptance remain required.

## Owner B round 12 — owning guide and actual helper-role readiness

Assignment: **OWNER B ROUND12**. Runtime session/thread ID
`01a14b6c-dfdd-7100-8b51-6a578f38328c`; no T3 delegate task ID was supplied
in this brief, so none is invented. Evidence: `artifacts/owner-b-r12-20261017`.
A retains sole compiler/linker/postprocessor/config/scripts/NativeOracle/profile
publication ownership. B changes only `docs/usage.md`, these two progress
ledgers, B's native-helper snapshot description and independent Audit helpers.
Runtime/header/Host/API and frozen IDs/decorators/exclusions remain unchanged.

Required P5 guide: [usage.md](usage.md). It documents actual facade signatures,
ordinary project references and `CPythonProject` selection, build/publish and
asset relocation, exact JSON workflow, owner-thread/value/callback lifetime,
GIL/worker limits, explicit disposal/reinitialization, errors/tracebacks/capture,
non-BMP/NUL/surrogates and catchable cooperative KeyboardInterrupt. The bounded
10ms host-wait polling scope is distinct from arbitrary blocking calls/shutdown.
Trimmed versus whole-library/facade-rooted AOT, selected target/exclusions and
receipt-specific provisional status are explicit. All9 substantive C# excerpt
lines come from the unchanged ordinary sample;13 local links resolve.
No novel C# example, sample/API edit or documentation-triggered build was added.

The readiness review found one stale B native-helper snapshot description:
`common_translation_contract.defines` described blanket Py_BUILD_CORE inheritance.
`tests/Upstream/prepare_profile.py` now records per-unit ordered options and
public/source-opt-in/limited/internal roles; historical native manifests remain
unchanged. Native flags are oracle evidence, not target configuration.

New read-only `tests/Audit/helper_profile.py` consumes an actual receipt's module/
source manifest and hashes, actual ordered compiler argv (including split -D/-U),
real C/header guard text and actual static table. It preserves source-declared
internal opt-ins within _testcapi and requires compiled PE/initializer inspection
before reporting readiness. It does not preprocess, emit, run Python, replace
the semantic compiler, or run upstream assertions. The separate helper checks
are not yet wired into A-owned recipe hooks; A can invoke the CLI on a published
profile with MetadataAudit output. Full qualification still uses the existing
complete product/D10/runtime/upstream gates.

Exact independent commands/results are under this round's artifact directory:

- `audit-controls-final-v2-command.json`:76 audit methods pass, zero skips
  (59 existing plus17 helper-role/registration controls). These include command
  order/redefinition, public-core inheritance, erased internal roles, source
  opt-ins, cancellation/unrun, source/guard/unit drops, missing compiled
  initializer, wrong-product PE and changed PE rejection.
- `upstream-report-controls-command.json`:21 report controls pass, zero skips;
  required skips/xfails and duplicate occurrences remain failures/original counts.
- `actual-failed-profile-review-v2-command.json`:intentional exit1 on actual
  failed286 receipt224316; source/receipt-bound report
  `actual-224316-helper-review-v2.json` remains **not ready**. It retains62 role
  units (29 public,5 source opt-ins,23 limited,5 internal),75 registrations,
  one failed/two cancelled/59 unrun role-unit commands and no compiled profile.
  Receipt SHA522b2af77f86b72d7864e844132334db40181bea34e66f9c6ed5ee10b2d72247.
  No failure or cancelled/unrun row is relabeled passed.
- `guide-source-review.json`:local links/sample excerpt checked read-only;
  this is not a compilation/runtime result.

Last completed actual profile failure remains224316's public _testcapi guard.
During this round A produced provisional C031 V6 tools with ordered macro
undefinition and started strict286 retry225558-3de551e6. Handoff
`c031-r10-v6-tools-handoff.json` says `toolsFinalStable:false`, inputDrift[],
all163 R10 hashes bound. No final193 READY/run.lock release or published
test-profile pointer was received. Canonical embedded pointer at entry remains
V3 receipt222803; B runs no old-product matrix and queues no shared command.

Next: A completes the full286/final193 retries and supplies explicit READY/
release plus exact receipt/source/PE/site analysis. B first reviews every pure
address group/remaining concrete exception and final D10 payload IL/RVA/root
bindings, then runs short JIT ABI/useful consumer before expensive all-four-cell
ABI/behavior/threads/lifecycle, trimmed consumer, rooted AOT and delivery/audit.
Consumer-only outer flock follows run.lock release; specialist hooks retain
per-command locking. Warmed allocations/writable identity must bind new PEs.
Actual frozen upstream assertions require the published286 profile; native
UTF8/default-encoding/marker contradictions stay for director adjudication.
P6 whole SQLite/QuickJS/picotls/MsQuic regeneration with matching provider/AOT,
affected Blink/libsmb2 and reproduction remain pending final stable tools.

Preservation: `artifacts/owner-b-r12-20261017/preservation-after.json` binds
all163 R10 inputs,168 protected runtime/header/Host/API/sample inputs, all25
unrelated dirty campaign inputs, six frozen upstream JSON files, PLAN and HEAD
unchanged. `docs/plans/summary.md` is also unchanged. A's concurrent
`DotCC/Program.cs` edit is preserved. Product pointers remain unchanged, with
no published test-profile pointer. Final index: `r12-summary.json`. No B shared
command is queued; the independent R12 deliverables are complete.

## Owner B round 11 — actual address evidence binding; final publication pending

Task `cpython-impl-c030-profile-qualification-r11-20261017`; task ID
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-c030-profile-qualification-r11-20261017`.
Evidence/work: `artifacts/owner-b-r11-20261017`, `build/owner-b-r11-20261017`.
A exclusively owns compiler/NativeOracle/config/scripts/profile/publication.
B edits only assigned Audit tests/reader and these two progress ledgers. R10
runtime/header/Host/API inputs remain frozen; no competing compiler/profile
worker, cleanup, staging, commit, push or nested agent.

The previous B address gate had two reproduced false acceptances: a qualified
label with exact source hashes and an empty unresolved list but no actual
analysis; and a matching run ID with a different receipt SHA. Original failing
controls are retained in `address-binding-before.log`. B now requires actual
semantic-report provenance, exact receipt/source/inventory/PE/MVID identity,
UTF-16 source spans (including CRLF/non-BMP), cache declarations/getters/uses,
actual tracked-ref helper and independently inspected PE field/Get IL. Source
conversions/cache declarations/uses/pins are independently enumerated; storage
counters are recomputed, including repeated loop acquisitions and mixed
direct/cache sites. Every remaining conversion requires a source-bound concrete
exception; macro/call/label blanket waivers and an empty analysis cannot pass.
These are evidence checks, not new interpreter assertions or a replacement
semantic compiler. Actual final analysis and runtime qualification remain open.

Fresh auditor controls pass59 methods/zero skips, including40 malformed
analysis/site subcases. The retained v5 support-fixture failure followed the new
actual lazy-cache counter check: six marker fixtures declared zero despite
containing one generic cache. Correcting those support records passes; no runtime
assertion changed. The final count excludes a workspace-dependent historical
report test, moved to an explicit artifact check so a missing ignored report
cannot become a skipped repository test.

Actual V3 exposed another B harness incompatibility: C030 has zero legacy
`ref T __dotcc_address = ref ...` declarations and1463 lazy tracked caches.
The data gate now independently counts cache declarations, accepts this
representation and rejects a claimed count without source sites. The required
semantic/address/allocation gates remain separate.

Isolated MetadataAudit build passes under nonblocking per-command shared flock,
zero errors/two unchanged VectorIo warnings (`metadata-build-command.json`).
Read-only inspection of **A's preserved V3 PEs**, never loading/executing
CPython, confirms both forms' actual ref-byte field061005 and Get signature/
42-byte IL hash8a73d2c2a3dc0afd76027e71ce519b8b76667256e637532bcf4dd9880183c28b.
`v3-independent-metadata-commands.json` binds inputs before/after and reports.
`v3-static-data-review.json` independently passes actual preserved source/
receipt/Release PE/RVA bindings in both forms:1233 properties/897429 bytes,
1338 getter calls,zero allocating payload sites,1463 caches/383 pins/691 runtime
base mentions. Existing immutable RVA tooling was reused; B rebuilt neither
compiler nor NativeOracle and ran no product consumer. No warmed allocation,
writable-object identity, JIT/AOT interpreter or final qualification is inferred.

`actual-negative-report-consumption.json` consumes both retained actual failed
semantic reports against their receipt/PE identities. Contradictory wrappers
declaring qualified/empty unresolved cannot turn either result green: old210915
retains1567 groups and V3 retains2. This is actual-report rejection, not a rerun
of the historical analyzer or interpreter. Final analysis must also provide
bound symbol/reference declaration spans and analyzer cwd, so relative invocation
paths resolve correctly when verification runs from an unrelated directory.

A's provisional actual product `20261017-222803-2283f397` has receipt SHA
`fded45aea557db1f716b5904ccbe176f146133672efe599cc835563d7391b6f1`.
Its actual raw analysis `c030-v3-raw-address-analysis-r4.json` reports4635 bound
sites/4632 regions/1463 caches,9 remaining conversions,zero semantic errors and
unknown bindings, but **two unresolved pure groups**: three pointer-slot
addresses in PyOS_Readline and two const-bool addresses in memoryobject.unpack_cmp.
The report/PE/source identity is provisional, separate from210915 and the next
scalar/nint extension. No READY/run.lock release was supplied; B runs no product
consumer or old matrix while A regenerates. Frozen upstream IDs/decorators/
exclusions/default-encoding adjudication remain unchanged. Final stable-tool
production/provider/AOT/affected regressions and reproduction remain required.

A's scalar/nint extension passes focused34 including exact native const-bool
memcmp, plus separate object-linked static/context JIT/AOT GC/alias/owner controls.
Immutable V4 handoff `c030-r10-v4-tools-handoff.json`, SHA
4384ac807b1ab00b35da98682931ca3148b92be05e330eef412c0e3664f53338,
captures313 sources with drift[] and all163 R10 hashes exact. A owns V4 full286,
then final193 after profile findings. No final READY or lock release exists.
Next B action is final receipt/source/PE/address review, then short JIT ABI/useful
consumer before fresh all-four-cell ABI/behavior/threads/lifecycle, trimmed
consumers, both rooted AOT forms, all delivery cells and complete audit.
Consumer uses outer shared flock only after campaign run.lock release; specialist
hooks retain per-command locks without nesting. Frozen upstream assertions run
only after A publishes that actual full286 profile; no placeholder/test API is
shipped. Final-tool downstream plan remains unexecuted.

Actual V4 full286 receipt `20261017-224316-589a5659` retains286 selected units,
passes fcntlmodule, then fails203-_testcapimodule exit2:
`_testcapi must test the public Python C API, not the internal C API`.
Two in-flight objects are cancelled (exit130); outputs remain{}. This is A's
profile-helper `Py_BUILD_CORE` flag selection, not a new B runtime defect.
A owns the per-unit correction and complete retry; no source/unit is dropped.
No published full286 profile exists for B to execute. Full R11 evidence and
exact commands: `artifacts/owner-b-r11-20261017/r11-summary.json`.
`preservation-after.json` verifies all163 R10 inputs,228 frozen runtime/header/
Host/API/non-Audit-test inputs,all25 unrelated dirty inputs and HEAD unchanged.
The independent B audit phase is ready for handoff; no B shared command is
queued, and final product qualification remains pending A's explicit release.

## Owner B round 10 — stdio opens and temporary files READY; lane released

Task `cpython-impl-file-lock-closure-r10-20261017` (task ID
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-file-lock-closure-r10-20261017`).
Evidence/work: `artifacts/owner-b-r10-20261017`, `build/owner-b-r10-20261017`.
Full index: `r10-summary.json`; exact source/tool bindings: `source-bindings.json`.
A exclusively owns compiler/config/scripts/publication/full193/full286/C030/
NativeOracle. No B compiler build, full profile translation or native-helper build.

BEFORE same-C native/emitted and actual DotCC.Libc C# controls reproduce the
contract mismatch: the independent native peer's LOCK_EX|LOCK_NB succeeds for
native fopen/freopen/mkstemp, but fails EAGAIN11 for each managed/emitted open
until close. `before/comparison.json` retains commands, source/tool hashes,
binaries and individual transcripts. This is an observed failure, not cleanup.

Shared FileLib/FileDurabilityLib/PosixFsLib reuse original native open and owning
SafeFileHandle on Linux x64. No path reopen, hidden advisory lock, per-call dup/
close, synthetic fd sent to the OS, private libc or new native import. Kernel
flags/errno, atomic exclusive creation/umask, private0600 templates, append/update
access, original handles, FILE identity/borrowed buffers, fdopen ownership and
portable FileStream fallback remain explicit. Existing tmpfile is unchanged;
these path-based probes do not qualify its anonymous-file equivalence.

**Final same-C native, emitted JIT and emitted NativeAOT each pass43 assertions /
11 phases**, using exact immutable `c030-r10-v3-tools`: `final-v3-nofollow`.
Independent native-peer controls prove opens under an existing exclusive flock,
no implicit sharing flock, retained caller locks after another open/close,
release on owning close, separate classic fcntl close-any-descriptor behavior,
append offsets/a+ reads, exclusive nontruncation, symlink/O_NOFOLLOW ELOOP,
0666&umask0027=0640 creation, unique0600 templates and fdopen/error ownership.
The preceding narrower39-assert final-v3 and native31 preflight remain retained.
Four lock observations pass per native/direct/emitted JIT/AOT probe. No product
fork/subprocess support is inferred from the independent native test observer.

Direct actual DotCC.Libc **JIT46 / NativeAOT46** assertions pass; native-peer
observations pass both. Expanded affected runtime **77/77**, zero skipped,
includes new3 xunit methods and existing R9/R5/filesystem/wchar/stdio/owner cases.
Earlier43/43 overlaps rather than adds to77. Fresh current shared-runtime stdio
preservation **JIT45 / AOT45** also passes flush-before-reopen, borrowed buffers,
failed reopen/close/reuse/error-reset and stdout ownership. These are focused
runtime controls; rebuilding historical campaign products cannot qualify them.
Direct/emitted/preservation AOT ELF depends only on libc/libm/the ELF loader.

Initial affected run retains42 passed /1 failure: FileOwnershipTests assumed
implicit BCL fopen sharing. The same ownership/disposal check now explicitly
requests real flock before asserting conflict/release; no assertion is weakened.
`unit.log`, initial TRX and `unit-aot-commands.json` preserve that failure;
`unit-aot-v2-commands.json`/`affected-commands.json` retain corrected passes.
All native/shared operations use per-command flock --close without nested locks.

`source-bindings.json` independently verifies all313 immutable source copies,
A tool binaries/handoff and the exact three candidate runtime hashes.
`final-runtime-hashes-r10.json` binds all163 R9 inputs:160 unchanged, only3
intended runtime changes. Preservation checks show231/234 prior B-owned inputs
unchanged, all25 unrelated dirty tracked inputs unchanged, frozen inventory/
267 exclusions/duplicates/decorators/auditor unchanged and original HEAD
bb2f9b1f1399ca1f6ac39c7c42f964796d7dd183. One existing ownership test, new R10 tests
and these2 progress ledgers are the remaining B edits. No commit/staging/push/
reset/cleanup/worktree/nested agent; concurrent PLAN/compiler edits preserved.

A received final READY hashes and explicit **RELEASE-ALL** after every B shared/
native command completed: `release-all.json`. A acknowledged exact input hashes
and started full193 strict all-form with immutable V3/tools reuse/jobs2; full286
follows independently. No next actual286 blocker or new193 READY/run.lock-free
release exists yet. Tools remain provisional. B has no old-product interpreter
command queued, repeated no210915 matrix and did not soften its required failed
final-address gate. New actual typed-owner/projection/static-storage/site/IL/
PE review must precede fresh matrix/rooted/delivery/audit. Full frozen upstream,
final stable-tool production/provider/AOT/affected regressions and reproduction
remain required; the final-tool downstream plan stays unexecuted.

## Owner B round 9 — real record-lock ABI/services READY for A full-profile retry

Task `cpython-impl-fcntl-profile-r9-20261017` (task ID
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-fcntl-profile-r9-20261017`).
Evidence/work roots: `artifacts/owner-b-r9-20261017` and
`build/owner-b-r9-20261017`. A retains exclusive C030/compiler/config/full193/
full286/publication ownership. B performs no compiler build or profile translation.

The real old-tool full286 failure remains R8's214714 `fcntlmodule.c` incomplete
aggregate. Independent same-C BEFORE control passes62 assertions +29 native
peer checks/14 phases, then old immutable tools fail emission with incomplete
`flock`, exit2. Original sources/transcripts/commands/hashes remain in `before`
and `before-v1-*`. No helper/source/config drop or generated C# repair.

Shared `fcntl.h` now declares Linux x64 LP64 `struct flock`: signed short type/
whence, signed64 off_t start/length, signed32 pid_t; size32/alignment8, offsets
0/2/8/16/24. Commands GETLK/SETLK/SETLKW=5/6/7 and RDLCK/WRLCK/UNLCK=0/1/2
match actual system-header observations. `RecordLockLib.cs` borrows the real
FileStream SafeFileHandle with DangerousAddRef/Release, without dup/close or
passing a synthetic descriptor to the kernel. It synchronizes the logical
FileStream position to the same host descriptor for SEEK_CUR. The kernel handles
ranges, PID/conflicts, EFAULT, signed/large offsets, unlock and genuine blocking.
No descriptor-table lock is held during SETLKW. A racing close is deferred by
the retained handle until the in-flight call returns. Other host ABIs/backings
fail ENOTSUP; classic locks remain process-owned across threads/runtime contexts.

The separate existing `flock` success stub is replaced by real Linux flock(2),
with open-description ownership. Direct controls exposed .NET path-open's
implicit shared flock. Linux x64 POSIX `open` now uses the existing native-open/
owning-SafeFileHandle path even without O_NOFOLLOW, without hidden sharing locks.
Native flags/mode/errno are preserved. Existing stdio fopen/mkstemp path-based
FileStream implicit locking is a separately observed residual; this repair does
not qualify those as lock-free native opens or change their source/tests.

**Current results:** direct actual DotCC.Libc JIT78 / NativeAOT78 assertions and
affected runtime selection72/72 (including the five new xunit methods), zero
skipped. Controls include pointer
widths and invalid integer addresses, descriptor-owner isolation, process lock
cleanup, changing file offsets/bytes, native OFD observation, no per-call dup/
close, retained blocking/concurrent close/table freedom, and real flock conflicts.
Current85-path direct-source snapshot is unchanged. Expanded same-C native
control passes77 assertions +41 independent-process checks/21 phases, including
both blocking APIs held for at least300ms before independent-owner release.

A's first immutable provisional C030/R9 CLI emits/builds this C control. Its
14 record phases pass, then exclusive flock fails errno11 because that snapshot
still embeds the earlier path-based open. `provisional-v1` retains the actual
failure. No expensive AOT follows that failing JIT. A's second immutable
`c030-r9-v2-tools` snapshot binds all313 source inputs with drift[] and the exact
five candidate hashes. **Native, emitted JIT and emitted NativeAOT each pass77
C assertions +41 independent native-process checks/21 phases** in `final-v2`.
Both real blocking calls remain blocked for at least300ms before peer release.
All emit/build/publish/run stages exit0; actual C header geometry matches native.
The ordinary `cpython-r9-record-locks` emitted fixture passes17 assertions,
one discovered test/zero skips, with V2 compiler resources and fresh actual libc.

Actual unchanged same-pin `Modules/fcntlmodule.c` now emits exit0 using the
original214714 source/config/flags and A's first immutable provisional tools;
only tool/output/report destinations changed. `actual-fcntlmodule-v1-*` binds
that command/source/tool identity. This is one real module diagnostic, not a
full286/profile/link/build/runtime pass. A owns the next full193/full286 retries.
All these results remain shared-service evidence, never interpreter cases.

Stable candidate five-path hashes: `candidate-services-hashes.json`. Direct
commands/results: `direct-jit-v3-commands.json`, `unit-and-aot-commands.json`,
`unit-results/r9-record-locks.trx`, `direct-source-inputs.json`. Preserved first
failures include an authored test's active-binding disposal mistake (corrected)
and a private command recorder's incorrect dict slicing; neither is a runtime
qualification failure. A's earlier new-file MSBuild glob race is separately
preserved by A. Existing VectorIo warnings remain visible.
An additional replay of the already-tested direct consumer's managed AOT-input
DLL timed out after30s while A's full shared-functional run held the shared
lock; its blank log/exit124 remain in `final-direct-jit-command.json`. It supplies
no runtime result and does not replace the completed direct/emitted controls.

Final five hashes remain unchanged;158/162 prior B inputs are unchanged, with
only the four intended existing service/header changes plus new RecordLockLib.
All25 unrelated tracked inputs and HEAD remain unchanged. `preservation-after.json`
records these checks. Actual direct/emitted AOT ELF dependencies are libc/libm/
loader only. The unchanged D10 auditor's50 mechanism controls pass; this does
not qualify the final address analysis or turn R8's required failure green.
`future-downstream-qualification-plan.json` records exact final-tool command
vectors/profiles/provider order and six successful dry runs; no downstream
regeneration was queued. Serial shared/native locking and existing specialist
hook ownership remain execution prerequisites, with no nested shared lock.
All B shared commands are now complete; no old-product consumer/matrix work is
queued. A's C030 address-review plan remains design-only until actual new193
source/receipt/PE/site analysis and semantic-exception proof exist. B will
validate those concrete bindings without treating empty lists as proof.

R8 RELEASE-ALL and all old product evidence remain distinct. B has not repeated
old210915 matrix cells, altered final D10 review JSON, or softened the required
address gate. New193/286 products, C030 final-address proof, full upstream frozen
assertions/counts and unresolved native skip/xfail/UTF8 policy remain required.
Final-tool production SQLite/QuickJS/picotls/MsQuic regeneration/runtime/AOT with
provider closure and affected Blink/libsmb2, reproduction and P0–P6 remain open.
No staging/commit/push, cleanup/reset/worktree/branch replacement or nested agent.

## Owner B round 8 — matrix/data checks pass; required final address gate fails

Task `cpython-impl-d10-delivery-matrix-r8-20261017`; evidence/work roots
`artifacts/owner-b-r8-20261017` and `build/owner-b-r8-20261017`. A retains sole
compiler/config/generator/publication ownership. B changed only assigned tests
and these two progress ledgers. No compiler/NativeOracle/runtime/header/Host/API
source change, commit, staging, worktree, cleanup or nested agent this round.

**Final D10 address qualification remains REQUIRED FAILED.** A's
`d10-address-reuse-review.json` explicitly leaves conservative all-call and
labeled-block limits open, with further safe static-storage reuse possible.
The reduction21237→2736 runtime-base mentions does not prove every remaining
repetition necessary. B added `d10_address_qualification` to the actual audit
hook, requiring a receipt/source-bound qualified review with no unresolved pure
sites. Read-only execution against the completed real audit reports exits1 for
both forms; exact failures/next owner are in
`d10-final-address-qualification.json`. A owns the final-storage/dominance repair
and any resulting full193 regeneration/new matrix. No blanket macro/call
exemption, compiler repair or unsafe pin contract was introduced by B.

A explicitly released strict all193 D10 product `20261017-210915-401ea00d`,
receipt SHA `0bf316d6b7076c40cf4471f310bb918f7c4e95a73998ca84e08c0896d36bd1b4`,
after useful JIT consumers passed `211651-b11d262c`. Shared-lock before/after
snapshots match **all262 owned files per form** exactly; tools remain provisional.
`r8-summary.json` consolidates the original receipts and required open gates.

- ABI: both JIT forms pass `212126-eac09a77`; both AOT forms pass
  `213332-6e74a923`. Every cell executes38 assertions/two cycles and24 common
  native layouts, with native wchar4 versus unsigned target wchar2 explicit.
- Behavior/threads/lifecycle: both JIT forms pass their cases in
  `212308-83222824`; both AOT forms pass `213332-6e74a923`. Each cell has two
  cycles,5/2/2 passing Python groups respectively, plus actual owning error,
  trace/cancellation/reuse/reference/callback/worker-drain assertions.
- Useful consumers: A's211651 supplies both JIT cells. Both ordinary trimmed AOT
  consumers pass `213151-2f0c807e`, invoked from unrelated /tmp under the required
  outer shared flock after run.lock release. Every consumer has two cycles/six
  callbacks/exact JSON and text/error/lifetime assertions. The generic common
  consumer hook still needs A-owned serialization support; the outer lock makes
  these particular commands valid and does not rewrite R7's earlier observations.
- Whole translated-library **and facade** rooted AOT passes both forms in
  `213332-6e74a923`, all9 Python groups/two cycles each, with actual wrapper gates.
- Delivery: both JIT forms pass `212800-1f2b9d4a`; both AOT forms pass
  `213332-6e74a923`. All188 assets/notices, outside-checkout paths with spaces,
  empty PATH/poisoned Python settings, delivered stdlib/app/builtin origins,
  no native engine/extensions and normal/three missing-asset fresh processes pass.

Actual data/dependency audit `214231-78a0d9bb` passes both forms/JIT+AOT:
all193 source/object/link/build closure,70 generated sources per form, original
Host links,46 compiled builtin registrations,188 assets, actual assembly/import/
ELF dependencies and resolved runtime-pack notices. This invocation preceded
B's final-address gate addition; its passing status establishes these narrower
checks. The final required address gate above **fails**, so neither this receipt
nor the matrix closes the complete audit/P2/P6 requirement. Its original index
and receipt are preserved; `d10-final-evidence-index.json` adds the separate
address-review binding without rewriting the earlier data evidence.

D10 data proof binds source/csproj/ownership/receipt to actual PE SHA/MVID and
A's IL/RVA reader:1233 properties,897429 bytes,1338 getter call sites per form,
zero allocating constant AsSpan byte payloads and zero getter newarr/maxIL16.
Raw MVID dc77a44f-52ed-41ea-b26a-05807e8994e6; processed
MVID d5aa3e65-eaa7-4125-a980-f66a8042d3e1. All1233 properties in each of four
actual-product JIT/AOT probes pass10000 warmed copies/exact hashes/zero allocation.
These warmed payload measurements exclude required writable destinations and
owned pinned literal-pool startup allocations; their identity/lifetime remains.
Required mutable setvbuf and per-pipe byte buffers are inventoried separately.
Tracked addresses830/pinned chunks344 and concrete typed-IR/lifetime/branch
examples are evidence of the implemented subset, not complete remaining-site
qualification.

A's91f0ec6c evidence index verifies all probe/program/project/binary/PE hashes,
322 retained compiler/runtime sources, semantic30/507/4 TRX results and exact
native stdout, without summing overlapping tests. The unchanged known compiler
malloc-text failure and initial AOT reflection-root harness failures remain A's
recorded failures. No NativeOracle/script/compiler source was edited by B.

Support checks: **50 auditor controls**, **3 upstream receipt-retention controls**
and two actual invalid-PE rejection controls pass their intended contracts;
they are not interpreter cases. Required upstream skip/xfail/duplicate counts
now survive nonzero interpreter exits in the campaign receipt, with the original
full case report/hash and first nonpass preserved. Frozen IDs/decorators/selection
are unchanged; the separate actual286 product remains A-owned/unpublished here.

After RELEASE-ALL, A's isolated full286 attempt
`20261017-214714-ed846112` **FAILED** actual `Modules/fcntlmodule.c`: local object
`l` requires a complete type, incomplete aggregate `flock` (exit2).
`201-fcntlmodule.log` and the exact compiler command/source/tool hashes are bound
in `full286-fcntl-blocker.json`. No unit/header/config drop or publication was
made; the selected210915 product remains separate. This round's Audit/Delivery
scope does not authorize repairing the shared `fcntl.h`/real-lock contract;
the coordinator must assign a runtime lane. A continues C030 final-static-storage
repair. Any changed tools require fresh193/product/matrix qualification.

Retained first failures:212308's delivery stage falsely matched the managed
product DLL names as native Python (precise filename fix plus genuine Python DLL
controls); initial filename-control setup error; overly strict physical-PE-path
comparison corrected to byte-identical-copy binding. The relocated command now
resolves flock before PATH poisoning. All original failure transcripts and the
corrected passes remain. No generated C# or interpreter assertion was changed.

All shared operations used `flock --close`; specialist hooks ran without an
outer shared lock. B sent explicit **RELEASE-ALL** after final audit/gate and the
last unchanged-product pin, allowing A's isolated full286 attempt. No B shared
build/emission/publication remains queued. All162 R7 validated runtime/Host/API/
header/QuickJS inputs and25 unrelated tracked dirty inputs remain unchanged;
HEAD remains bb2f9b1f1399ca1f6ac39c7c42f964796d7dd183.

Still required: complete D10 address reuse, final/stable tools and any fresh193
qualification after changes; full286/actual frozen upstream matrix and unresolved
required native skip/xfail/UTF8 policy; final-tool whole SQLite/QuickJS/picotls/
MsQuic regeneration/runtime/AOT with matching provider plus affected Blink/
libsmb2; complete reproduction and P0–P6 acceptance. These foundation passes do
not qualify the full campaign, D3/P1 or every required interpreter gate.

## Owner B round 7 — validated shared inputs and interim193 runtime matrix complete

**Final recording check found a serialization defect:** the common framework
consumer hook does not wrap dotnet publish/run with the shared flock. B's
175737 AOT consumer commands passed without that required serialization; those
original records remain unchanged. ABI/runtime/rooted hooks explicitly use
flock. The corrective raw consumer command was stopped with SIGTERM while
waiting on A's active D10 lock, before acquisition or any dotnet child; exit-15 /
38.27s and process/argv evidence remain in consumer-correction-queue-stop.json.
It performed no build. Locked consumer qualification is deferred to D10 so a
queued command cannot silently consume the replacement canonical product.
A owns Scripts/campaigns/testing.py; its generic consumer commands require an
explicit shared-lock wrapper on future calls (without nesting already-locked
specialist hooks). No B shared-framework/recipe edit.
After the coordinator signals final publication and run.lock release, run the
consumer-only suite under an outer shared `flock --close`; run the already-locked
ABI/runtime/delivery/audit/rooted hooks separately. Never put an outer shared
lock around those specialist hooks, which would attempt to reacquire it.

**A's next snapshot:** `artifacts/owner-b-r7-20261017/final-runtime-hashes.json`
verifies all162 current runtime/header/Host/facade/QuickJS source paths. A
independently rechecked every hash and published strict integrated all193
receipt203708-d34d1ee6 with C027, physical-size guard and complete cache.
All162 paths remain unchanged after the completed runtime matrix. Tools are
**not final/stable**: A exclusively owns required D10 compiler work and its next
full regeneration. Earlier195909 evidence remains separate. Inventory adds real eager
importlib/_abc.py and importlib/util.py, with no source drops:186 Python/188 assets.

- FileLib SHA-256: `d02583f37b3380f1cc9dac01ba555b97771f5254291b074b0dcff27a524d21fb`.
- math.h SHA-256: `7fcb6473ce51271cc834f2cbc8212c12ad49de4b32971a81a2ab38ae14fb1845`.
- QuickJsText SHA-256: `188a911e67a534dc441d4ff4a8df0d9a205cdf380f854327a4cfa424bbe760ae`.

The earlier validated162-path set before IEEE repair remains verbatim in
`final-runtime-hashes-stdio-complete.json`; it is historical, not current math.h.
R6 terminated at MODEL CAPACITY after partial edits. R7 recovered those edits
and actual per-command results; task label owner-b-r7-20261017 (delegated task ID
was not supplied). Private evidence/work roots are artifacts/owner-b-r7-20261017
and build/owner-b-r7-20261017. A retains compiler/config/core ownership. No B
compiler build, core emission/publication, staging/commit, cleanup or nested agent.

### Integrated strict product handoff accepted

A's integrated receipt `20261017-203708-d34d1ee6` passes all193 emit/link,
raw/private-processed/final canonical builds and publication, with all162 R7
source hashes and186 Python/188 total assets. Both-form actual useful JIT
consumer `20261017-204543-7bb548af` passes two cycles each. B completed fresh
ABI/behavior/threads/lifecycle hooks in both JIT and NativeAOT forms, ordinary
NativeAOT consumers and both whole-library rooted NativeAOT runs against that
explicit product. Current inputs remain interim: A exclusively owns required D10 scoped
address reuse/named RVA constant spans; another full regeneration and fresh
matrix/preservation will be required afterward. No B core emission/publication
or duplicate old195909 ABI run. NativeAOT followed relevant integrated JIT success.

### Integrated203708 actual JIT gates PASS

Fresh hook receipt `20261017-175127-c121cf50` **passes ABI, behavior, threads and
lifecycle in both JIT forms**, all against integrated203708. ABI:38 assertions /
two cycles /24 common layout fields per form, explicit wchar difference. Each
behavior cell has5 passing required groups per cycle; each thread/lifecycle cell
has2, two cycles each. All nine required Python groups execute across suites;
no failure/required skip in these foundation fixtures. Owning cancellation,
user trace, callback/worker drain, error/reference ownership and finalization
assertions also execute. The previously failed IEEE stdlib assertion now passes
in the actual interpreter. Exact summaries/individual reports are in
artifacts/owner-b-r7-20261017/integrated-jit-summary.json. Earlier195909 failures
and passes stay distinct. This is interim before D10, not P1/D3/full qualification.

B subsequently passed ordinary and rooted NativeAOT on this repaired initializer,
as detailed below. Old always-throw product failures remain preserved separately.

### Final focused repairs and controls

**QuickJS pre-existing owning-boundary defect:** its fast non-surrogate UTF-8
encoder omitted the required trailing NUL. Only that branch now stores zero;
byte counts/WTF-8/NUL/surrogate behavior is preserved. Actual production source
passes163 assertions each in JIT/NativeAOT: exact byte counts/data, poisoned stack
and NativeMemory storage/guards, forced marshaller allocation, ASCII/empty/
multibyte/non-BMP/both lone surrogates/NUL/long strings and real JS_Eval results.
Original source/sentinel count8/terminator7f/argv and all failed R6 runs remain.
Both-form JIT owning consumer/ABI/behavior/lifecycle/upstream passes QuickJS
receipt `20261017-172743-2b08b2f5`; both-form AOT owning consumer/behavior/lifecycle/
upstream passes `20261017-173510-a55dcf19`. Per form: ABI JIT5 harness groups;
behavior5 groups/16 JS cases; lifecycle23 groups; upstream6 files/76 JS cases and
the unchanged explicit event-loop exclusion. These use the **historical embedded
engine runtime plus actual owning marshaller fix**, and are baseline preservation,
not new-libc/final-tool production qualification. Whole-library AOT ABI is not
claimed by these runs. First queued QuickJS command was deliberately stopped
with SIGTERM before lock acquisition to prioritize final stdio controls; exit-15
and exact queue evidence remain immutable.

**stdio old-association/failure semantics:** R6's partial after binary now really
executes old=A/new=B. Final freopen drains buffered bytes and closes old backing
before opening/truncating replacement, including byte-only writes without a
Writer. Missing path/invalid mode close the old descriptor with NULL/errno;
stale FILE aliases cannot access a reused descriptor. Success resets input/error/
access state; old flush failures never redirect bytes or poison new indicators.
Native C35 checks and identical emitted JIT35/AOT35 pass with exact native stdout.
Direct actual current-libc JIT/AOT each pass45, including caller buffers,
same-path truncation, error closure, descriptor reuse, /dev/full and state reset.
Existing direct-libc file/stdio/ownership/error/formatting/R5 service units pass
65/65, zero skipped, with resolved fresh libc DLL identity matching direct harness.
Original R6 old-association failure and expanded before-run invalid-mode failure
(exit134) remain. Existing VectorIo warnings stay visible. No wholesale rewrite.

**IEEE math header defect from real CPython behavior:** earlier product emits
`retval = ((int)(negate ? -HUGE_VAL : HUGE_VAL))` in actual _Py_parse_inf_or_nan.
Shared math.h lacked typed C definitions. The native conditional/sizeof reduction
passes; same-source emitted code builds then exits1, reporting finite instead
of infinite results, NaN loss and HUGE_VAL width4. Header now defines typed IEEE
constant expressions for HUGE_VAL/F/L, INFINITY and NAN. Normal source includes
(no -D substitutions in final controls) pass16 native/JIT/AOT assertions each:
exact types, signs, conditional promotions, NaNs and static storage; stdout
matches exactly, long-double ABI differences are checked honestly. No compiler
or generated-source repair. Before source and all failures/candidate/final logs
remain; `math-header-inputs.json` records the exact actual include override.

Focused emission selects A's already built FunctionalTests compiler library
and existing CLI in a private provisional regression snapshot, with all original
runtime/header resources verified byte-for-byte. Only the final math.h control
uses its explicit normal include override after that resource snapshot. This
avoids duplicate B compiler builds and does not declare final campaign tools.
The byte-identical validated16-check IEEE and35-check stdio C controls now also
live in normal functional discovery under Fixtures/cpython-r7-math-constants
and Fixtures/cpython-r7-stdio-reopen-errors, each with exact native stdout.
standard-functional-regressions.json verifies their identity; the next ordinary
functional build will include them. No additional compiler/source implementation
is introduced by these regressions.

Runtime/header hashes, tool identity, exact argv/cwd/timeouts/exits and unit TRX
are in final-runtime-hashes.json, runtime-control-tool-identity.json,
file-unit-snapshot-identity.json and all *-command.json/logs.

### Actual existing-product runtime evidence and next integrated cells

Product `20261017-195909-d7f8cc82` is explicitly pinned. Coordinator's raw JIT
useful consumer `20261017-200920-a72d4f43` passes two cycles/six callbacks. A's
ABI `20261017-201502-a09f0b71` passes both JIT forms,38 real operations/two cycles
and24 common fields per cell, retaining native wchar4 versus unsigned wchar2.
B did not repeat completed ABI cells; P1/D3/AOT/full matrix remain open.

B's first campaign attempt is preflight blocked by A's run.lock, not runtime
failure; no bypass/deletion. After release, `20261017-172108-54071092` builds both
JIT forms, then exposes authored fixture's unnecessary argparse dependency.
The explicit embedding entry now takes suite/work/report directly, deferring
CLI parsing to main. Test modules import normally from the explicit application
path; native -I CLI supplies that same fixture directory. All9 groups/assertions
and selection remain unchanged. Native CLI/direct equivalence passes5 behavior /
4 unselected and rejects unknown suites; import guard proves no argparse entry.
A independently closes real already-selected inspect/pathlib/importlib.util eager
dependencies in the product inventory; no test-only product APIs or scope change.

Private unchanged actual-product harness/DLL snapshots plus refreshed authored
fixture content execute all9 groups in both JIT forms, cycle0: **8 pass/1 fail**.
The required stdlib group still fails its real nan/inf assertion on old195909;
that failure remains failed pending new whole regeneration with corrected math.h.
No manual DLL core replacement or generated-source edit. Original failed output/
fixture versions remain, and cpython-behavior-pinned-inputs.json plus
cpython-behavior-fixture-retry-inputs.json identify exact bytes.

**Actual thread and lifecycle JIT suites pass in raw and processed**, each two
full owning init/finalize cycles, two required fixture groups per cycle. Real
Python worker/TLS/condition/managed callbacks, ordinary errors, user trace
preservation, loop/wait/pre-cancel interruption and reuse, value/owner rejection,
callback/active-wait shutdown drain and finalization all execute. Exact cases/
commands/identities are in cpython-jit-thread-lifecycle-summary.json. These
passed existing-product cells need fresh integrated-product execution; they do
not qualify old behavior, CPython AOT or all upstream cases.

P0 in progress/P1–P6 remain open. Frozen5,134 IDs/4,867 required/267 individual
exclusions and required native skips/xfails are unchanged. Separate286 helper
profile, final all193 product and all4 cells/rooted AOT, required upstream outcomes,
relocation/audit/reproduction and final-tool SQLite/QuickJS/picotls/MsQuic whole
regeneration+required runtime/AOT with affected Blink/libsmb2 stay required.
Historical product rebuilds are baseline only. r7-summary.json retains exact
controls/receipts/remaining gates. HEAD and25 unrelated tracked dirty inputs
are verified preserved; A-owned concurrent changes are excluded from that check.

### R7 standard regression verification complete

Both new ordinary-discovery functional fixtures pass **2/2, zero skipped**:
16-check typed IEEE constants and35-check freopen errors. The private test
snapshot uses the existing compiled functional harness, A's rebuilt CLI compiler
and freshly built actual libc dependency; every runtime/header resource matches
current source. No B compiler/harness build. Exact2-test TRX counters, source /
DLL identities and commands are in standard-functional-results and
standard-functional-snapshot-identity.json. All162 final source hashes remain
stable; the completed interim product matrix is recorded below.

### Integrated203708 all NativeAOT cells and both rooted forms PASS

Receipt `20261017-175737-1ecb6a1a` passes both forms of ABI, ordinary trimmed
owning consumer, behavior, threads and lifecycle NativeAOT (299.11s total command).
Each ABI cell passes38 real operation assertions over2 cycles and24 common
native layout fields, with native wchar4 versus target unsigned wchar2 explicit.
Each consumer passes2 cycles /6 callbacks with exact JSON and return/output /
traceback/finalization. Each behavior cell passes5 Python groups per cycle;
thread/lifecycle cells pass2 each,2 cycles per cell. Wrapper ownership, error,
trace, cancellation/reuse and callback/active-wait shutdown drain assertions run.

Whole TranslatedCPython **and facade** rooted NativeAOT also passes both forms:
raw receipt `20261017-175516-1b21790c`, processed `20261017-180236-b8744b01`.
All9 Python groups pass in each of2 owning cycles per form, with the same real
wrapper lifetime/thread/cancellation gates. Raw publication53.14s/run1.26s;
processed publication43.14s/run command208.77s includes shared-flock waiting,
so the latter is not a pure interpreter timing. All campaign commands completed
normally and released run.lock; no bypass, deletion or competing core publication.

`artifacts/owner-b-r7-20261017/integrated-matrix-summary.json` verifies all4
consumer cells,4 ABI cells,12 selected runtime cells and2 rooted forms directly
from receipts, exact stdout and all per-cycle case reports. It binds product
receipt SHA-256, all262 owned output files per form and unchanged162 runtime
source hashes. Separate AOT and processed-rooted summaries retain exact paths.
The JIT consumer receipt is A-owned; B did not duplicate those completed runs.

### Next A/B handoff; final qualification remains open

No shared runtime/header source changed after A's203708 snapshot. Reuse the
exact final-runtime-hashes.json and the three hashes above for A's next tools.
Latest coordinator handoff reports provisional C028 named static ReadOnlySpan
payload properties/C029 scoped fixed/byref address reuse,30 semantic cases and
2,977 compiler passes with the same known malloc-text baseline failure. Shared
functional507/507, literal pool14/14 and focused JIT/AOT allocation5 properties /
70,010 RVA bytes /10,000 warmed zero-allocation iterations pass in A's evidence.
These are reduced controls, not final CPython allocation qualification. A's
strict full193 receipt210915-401ea00d is active with359 captured inputs; all162
B inputs match. A owns full193/full286 regeneration and final-product evidence;
these interim runtime results do not qualify D10. A's source/receipt-bound
scripts/generated_code_evidence.py and NativeOracle/GeneratedDataAudit evidence
must be consumed by B's final audit once produced, checking source/receipt/PE
identity rather than inheriting a structural or allocation pass.

After A supplies the new explicit product and releases campaign run.lock, B
must rerun the runtime/ABI/consumer/rooted cells, delivery/relocation/audit /
reproduction and required upstream profile. Frozen5,134 IDs/4,867 required /
267 individual exclusions and required native skips/xfails remain unchanged;
no test exclusion or product-scope reduction occurred. Final-tool whole
SQLite/QuickJS/picotls/MsQuic regeneration with required runtime/AOT and affected
Blink/libsmb2 checks remains mandatory. Historical embedded-runtime production
checks above are baseline preservation only. No P1/D3/P0–P6 closure claim.

R7 changed paths/results/open gates are in r7-summary.json and r7-owned-paths.json.
Original HEAD, unrelated tracked dirty inputs, failed runs, old manifests and
R6 capacity-failed partial work are preserved. No staging, commit, reset/cleanup,
worktree, nested agent or child-review continuation message.

## Owner B round 6 — historical partial work; MODEL CAPACITY failure

B task: `node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-runtime-regressions-r6-20261017`.
A's active compiler/core owner remains `cpython-impl-raw-compiler-restart-r5-20261017`.
B has not rebuilt a compiler or emitted/published another CPython core. Evidence
root: `artifacts/owner-b-r6-20261017`; private work: `build/owner-b-r6-20261017`.

**Preservation provenance:** SQLite, QuickJS, picotls and MsQuic canonical
products embed their historical runtime sources. All lack the new passwd and
strict multibyte implementations. Ordinary rebuilds therefore provide baseline
controls, not evidence for the candidate libc. Exact product pointers and source
identities are retained in `products-before.json`; tracked dirty inputs and B
sources are recorded in `preservation-before.json`. Final-tool regeneration and
required runtime/AOT qualification of all four production campaigns, with
affected Blink/libsmb2 checks, remain required before P6. A has been notified.

Fresh pinned SQLite historical consumer, threading and HostVfs binaries pass;
the latter includes real SQL/JSONB/FTS5, WAL snapshots, mapping lifetimes and
cleanup. Original binaries were copied to private task-owned snapshots before
execution. QuickJS's two JIT useful consumers and actual ABI suites pass, but
processed behavior fails once with `ReferenceError: 'u' is not defined` before
the promise-order fixture completes. One repeat of the same rebuilt binary and
the retained historical binary pass. This is an unresolved intermittent
existing-product failure, not a candidate-runtime result; the failed receipt
`20261017-165821-50156470` remains failed. Lifecycle/upstream were not reached.

Independent review found a concrete `setvbuf`/`freopen` ordering concern:
byte-only buffered output has no Writer, so replacing the backing can redirect
pending old bytes into the new file. The uniquely named native C fixture passes
`old=A new=B`. The current-libc harness is queued under the shared lock; no fix
or emitted pass is claimed yet. picotls's both-form JIT qualification is also
queued behind A's full C026 retry. Commands and all wait/failure outcomes are
retained per command; these queues do not hold idle locks.

Current CPython selection remains `20261017-193052-824be3ee`: actual raw useful
consumer, Behavior and ABI operations fail in `CPython..cctor()` with
`InvalidProgramException`, before interpreter assertions. The prior raw AOT
"will always throw" result is retained; no repeated costly AOT attempt. B will
consume an explicitly selected repaired A product through short JIT first.
Frozen 5,134 IDs / 4,867 required / 267 individual exclusions and required
native skips/xfails/UTF-8 contradiction remain unchanged pending adjudication.
P0 in progress; P1–P6 open. No commit, staging, push, reset, cleanup, worktree or
nested agent.

## Recovery R5 — shared services READY; actual runtime blocked in initializer

**ACTUAL RUNTIME BLOCKER FOR A:** published raw useful consumer compiles, then
aborts at `dcp_create`: `CPython..cctor()` throws `InvalidProgramException`
(consumer receipt `20261017-194232-9f0184d4`, `consumer-raw-jit-run.stderr`).
The direct real-export ABI runner independently reproduces that initializer
failure before its first operation assertion. Its actual raw-assembly layout
control passes **24 common fields** against fresh native observations, retaining
wchar4 versus unsigned wchar16. Fresh native ABI operations pass two cycles.
`artifacts/owner-b-r5-20261017/runtime-first-failure.json` preserves the handoff.
A owns the emitted initializer repair; B owns no compiler/generated-source edit.
A has wired `tests/Abi/campaign_hooks.py:run_suite` into the real campaign
recipe. No interpreter runtime/ABI gate pass is claimed.
Actual raw NativeAOT publication exited **0 in 337.96 s**, but ILC explicitly
reports `CPython..cctor()` **will always throw: Invalid IL or CLR metadata**.
The produced native binary confirms `InvalidProgramException` before its first
ABI operation assertion. Both raw JIT/AOT layout comparisons pass 24 common
fields only. Both raw and processed Behavior JIT builds pass, then fail at the
same initializer before any Python fixture executes (receipt
`20261017-194354-7e526066`). A is repairing initializer partitioning; B has not
regenerated/published another core. Exact ILC/run results and measured initializer
metadata are in `runtime-first-failure.json` and `actual-raw-cctor.json`.


**All R4 shared-symbol gaps and A's 43 new langinfo constants are implemented.**
`langinfo.h` now exposes **54 actual C-formatting queries plus CODESET**; their
numeric IDs and returned bytes match native libc. Keep `HAVE_LANGINFO_H`.
A has now emitted/linked all 193 and built/published canonical raw and processed
products with final C016–C024 tools: passed receipt
`artifacts/campaign/20261017-193052-824be3ee/receipt.json` (raw 36.48 s, zero
errors/three visible warnings). This closes the observed raw missing-symbol
errors; it does not qualify interpreter execution. Fresh final-snapshot service evidence:
`artifacts/owner-b-r5-20261017/services-c024-final/{commands,comparison,tool-identity,input}.json`.
B has run no competing core translation or product publisher. R4 was cancelled
after the restart, retaining all partial code/logs. R5 task:
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-libc-restart-r5-20261017`.
Thread `01a14a91-66b2-7642-9f4f-283ec23f4020`.

Shared closure: fdopen/setbuf/setvbuf, copysign/nextafter/fma/exp2/erf/erfc,
clock_getres, real mmap/munmap, atomic O_NOFOLLOW open, utime, complete Linux
passwd/getpwuid/getpwnam, actual uid/euid/gid/egid queries, strict mbstowcs/
wcstombs, nl_langinfo and all required locale constants. Real native bindings
remain in shared DotCC.Libc; no native Python backend/private libc/shim or
emitted C# repair. mmap's prior ENOTSUP is replaced by actual anonymous and
file-backed mapping with owner/partial-unmap cleanup. The independent stat
atime correction is required to observe distinct utime inputs.

**A owns configuration:** declarations/services support HAVE_CLOCK_GETRES,
HAVE_MBSTOWCS, HAVE_WCSTOMBS, HAVE_COPYSIGN, HAVE_ERF, HAVE_ERFC,
HAVE_LANGINFO_H, HAVE_UTIME_H, HAVE_STRUCT_PASSWD_PW_PASSWD/PW_GECOS and
identity getters. Review each against actual target behavior. Keep HAVE_MMAP,
HAVE_SYS_TIMES_H and HAVE_SYSCONF. Public passwd APIs are non-_r; do not enable
HAVE_GETPWUID_R, HAVE_GETPWNAM_R or HAVE_GETPWENT. Internal native _r calls
provide thread-safe storage, not additional public C APIs. The R5 raw receipt
`20261017-191623-134a3c10` failed only on the 43 constants with its older
CODESET-only tool snapshot; **all 43 are now declared and all 54 query values
passed native/JIT/AOT comparison** (`raw-service-closure.json`). The fresh complete receipt above records that build closure; actual runtime
gates remain open.

ABI/ownership details are in [abi-progress.md](abi-progress.md). wchar stays
unsigned UTF-16 with explicit ASCII C/POSIX or C.UTF-8 selection per runtime;
strict conversion, lengths/NUL/surrogates/errno are real. No native wchar4 call.
Passwd is 48-byte LP64 storage, separate per runtime/worker, borrowed until the
next same-thread lookup/disposal. fdopen adopts without dup/truncation, checks
access/append, and closes/invalidate handles before fd reuse. Full/line/
unbuffered output, caller buffers, UTF-8 formatted output, errors and cleanup
are exercised. Clock resolution describes its BCL source, not native precision.
MAP_FIXED and signal dispatch/pause fail ENOTSUP. Math uses BCL IEEE primitives
under the existing nearest-rounding contract; erf/erfc bind Linux libm.

Current focused result: **99 native assertions; 102 each in emitted JIT and
NativeAOT**, with two actual NSS/TLS workers, files/errors/ownership, anonymous/
file-backed/partial mappings, UTF-16/invalid text, IEEE edge cases and lifecycle.
**33 math values match at 0 ULP**, all uid/passwd observations match, and all
54 locale-query IDs/bytes match exactly in both C and C.UTF-8. Every control stage exits 0; existing
VectorIo CS0649 warnings remain visible. Earlier 93/96/96 and 97/100/100 runs
are preserved independently and never summed. One false native C-locale
CRNCYSTR assumption was corrected to actual `"-"` before final validation;
isolated native observations are recorded separately.

Repository final ordinary-reference selection passes **190/190**, zero skips,
including 15 R5 cases and the new locale-table lifetime/unsupported-selector
control. Tested-runtime DLL equality is captured under the parent flock in
`host-unit-locale-final-identity.json`. Earlier new/affected emitted selection
passed 9/9 before adding the callback-array function-address control; those
counts cannot qualify the now-expanded fixture.

**The unchanged required callback-array fixture now passes native/JIT/NativeAOT.**
A supplied validated C024 grammar tools after its 16 native/direct/object-link
controls passed. B rebuilt/snapshotted normally under the shared lock, then ran
`build/owner-b-r5-20261017/validate_array_c024.py`. The fixture matches retained
`fnptr-array-source.c` byte for byte; `array-c024-{commands,comparison}.json`
records all seven stages exiting 0 and exact expected stdout. No declarator
workaround/compiler edit or generated repair. Older stale-tool failure and the
superseded scalar variation remain separate, never counted as array proof.

**Actual ABI/facade/consumer execution is now active against canonical products.**
`tests/Abi/Abi.csproj` directly references the selected actual assembly;
`Program.cs` observes emitted types and invokes real Host/refcount/type/slot/
varargs/vectorcall exports through two initialization/finalization cycles.
`tests/Abi/campaign_hooks.py:run_suite` is B's real-export gate; A has
replaced `abi`'s pending binding with this hook in the campaign recipe.
It consumes existing products, uses the existing exact-pin native oracle only
for controls, records explicit wchar4 versus unsigned wchar16 differences, and
holds the required locks. It never emits/publishes a core. The raw ABI build passes (zero warnings/errors after correcting its authored
exception-pointer cast); its 24 common layout observations match fresh native.
Real operations and the useful consumer fail at the emitted class initializer.
Both Behavior JIT forms and raw NativeAOT now confirm the initializer
failure; all B test/build processes have finished.
Exact ownership/commands and the initializer metadata are in
`product-controls-intent.json`, `runtime-first-failure.json`,
`actual-consumer-raw-jit-command.json` and `actual-raw-cctor.json`.
The actual initializer contains 1,221,175 IL bytes, 183 locals and max-stack 784;
these are measurements, not a diagnosed CLR cause. Both canonical forms pass
actual source/closure/186-asset audits; the raw assembly has 5,068 types,
16,962 methods, 93,054 IL edges, 31 shared-runtime-matched imports and all 28
required Host/refcount exports. Nested-runtime auditor controls pass 24/24.
No full delivery/runtime/audit gate completion is inferred.

Refreshed actual shared-runtime metadata reads **110 types, 1,454 methods,
4,838 IL call edges and 31 native imports** (`shared-runtime-locale.metadata.json`);
21 auditor controls pass. libm imports are now explicitly classified as Linux
math services. The executed service AOT binary needs libc/libm plus the ELF
loader, with no Python engine dependency. These inspect/test the shared service
artifact, not a CPython product or fully rooted interpreter AOT build.

```bash
python3 -B cpython/tests/Abi/validate_host_services.py --work cpython/build/owner-b-r5-20261017/services-reproduce --artifacts cpython/artifacts/owner-b-r5-20261017/services-reproduce
flock --close /tmp/dotcc-cpython-shared-build.lock dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release -p:UseLocalLalrCc=false --disable-build-servers -m:1 --filter 'FullyQualifiedName~CPythonR5HostServicesTests'
flock --close /tmp/dotcc-cpython-shared-build.lock dotnet test DotCC.FunctionalTests/DotCC.FunctionalTests.csproj -c Release -p:UseLocalLalrCc=false --disable-build-servers -m:1 --filter 'FullyQualifiedName~FixtureTests&DisplayName~cpython-r5-services'
```

The Linux-x64 control uses existing campaign process/identity support, a private
compiler dependency snapshot and required shared/native locks. Choose fresh
output leaves. Preservation and exact owned paths are in `preservation.json`
and `owned-paths.json` (42 paths including the actual ABI runner/hook and
auditor updates). No commit/staging/reset/cleanup/worktree/
delegation or native interpreter/helper rebuild. Frozen IDs and all old artifacts
remain. P0 in progress/P1–P6 unpassed: actual core/ABI/D3/facade/runtime/delivery/
rooted-AOT and four-cell gates remain open. SQLite/QuickJS/picotls/MsQuic final-
tool regeneration/runtime/AOT and affected Blink/libsmb2 checks remain required
before P6; service assertions/source probes cannot satisfy those obligations.

## A/director integration — Owner B round 4

**A: shared process-times header/service READY for the full 193-unit retry.**
`DotCC.Lib/include/sys/times.h` now declares the actual LP64 four-`clock_t`
`struct tms` and `times`. `unistd.h` supplies Linux `_SC_CLK_TCK=2`;
`DotCC.Libc/ProcessTimesLib.cs` forwards Linux LP64 `times` and the matching
`sysconf` tick query to libc, preserving real self/waited-child counters and
elapsed ticks. Other host ABIs return `ENOTSUP`; no zero-success fallback.
`PosixQueryLib.sysconf` integrates that selector without changing existing ones.
**A has enabled both `HAVE_SYS_TIMES_H=1` and `HAVE_SYSCONF=1`** in the reviewed
target policy and owns the **all-193** retry. Retain both: exact-pin
`Python/fileutils.c:3032` otherwise makes `_Py_GetTicksPerSecond` take its bogus
**60 Hz** fallback instead of the real `_SC_CLK_TCK` rate. Enabling just the
times header would silently mis-scale `os.times` (this host returns **100 Hz**).

Fresh same-input pre-repair emission failed (missing header/incomplete `tms`),
native valid-input control passed, repaired emission/JIT/NativeAOT all pass.
Native waited-child CPU control also passes. All seven final build/run stages
exit **0**; JIT build has **0 warnings/errors**. Commands/logs:
`artifacts/owner-b-r4-20261017/times-validation-commands.json` and
`times-valid-before-commands.json`. Shared builds/emission/publish use
`flock --close /tmp/dotcc-cpython-shared-build.lock`; native builds use the
native lock. Repository-focused regressions now pass **14 unit + 1 emitted
functional, zero skipped** (`times-repository-commands.json`). This is shared
service evidence, not actual CPython execution or full downstream qualification.
Final SQLite/QuickJS/picotls/MsQuic regeneration/runtime/AOT and affected
Blink/libsmb2 checks are now also required for this B shared-runtime change.

The unchanged frozen inventory has a **configuration contradiction**. The real
Host sets `PyPreConfig.utf8_mode=1`; native `-I` alone leaves it 0. Native
`-I -X utf8` resolves both LocaleCodec surrogatepass outcomes, but now the
required `BuiltinTest.test_open_default_encoding`,
`CTextIOWrapperTest.test_default_encoding` and
`PyTextIOWrapperTest.test_default_encoding` explicitly skip because UTF-8 mode is
enabled. Fresh entire inventory: **4,875 occurrences / 4,867 unique**, **4,867
passed / 5 required skips / 3 required expected failures**, **0 ordinary
errors/failures/subtest failures/unexecuted IDs**, exit **1**. This is no gate
improvement or reclassification. See
`artifacts/owner-b-r4-20261017/native-required-blockers-final.json`.

Five original outcomes are unconditional upstream markers: two descriptor
`expectedFailure`s, missing-directory namespace ZIP `expectedFailure`,
unentered ZIP-resource `skip`, and private `_recursion_count` on the Condition
wrapper `skip`. An algorithm repair alone cannot make those marked cases pass:
the reporter must reject an unexpected success, and unconditional skips never
execute their body. No permission to change decorators or scope is inferred.
Director/user adjudication is needed; until then these remain REQUIRED blockers.
`tests/Upstream/investigate_blockers.py` supplies six small native semantic
groups with all seven original source/decorator records. Both native launch
configurations are recorded without changing upstream code, resources or IDs.

B delivered `tests/Delivery/campaign_hooks.py` hooks `run_delivery`, `run_audit`,
`run_boundary`, `run_behavior`; A wired them into the actual recipe alongside
the separate upstream profile. All managed builds and product executions use
`flock --close`; the upstream hook now uses the same support. They consume
existing selected products only; no competing core emission/publication or
native acquisition. Missing real products still block **all seven** specialist
suite checks (`product-prerequisites.json` / private framework receipt).

Delivery builds the **ordinary sample without whole-library roots**, copies it
outside the repository to paths with spaces and runs both useful-workflow
cycles from an unrelated directory. An empty executable PATH, poisoned
PYTHONHOME/PYTHONPATH/PYTHONUSERBASE/startup and PYTHONUTF8=0 test actual isolation.
The separate real `Delivery` entrypoint checks exact search paths, generated
builtin registrations, delivered module origins, no loaded extension/engine,
and missing encodings/json/application diagnostics in fresh processes. These
are **integration-ready/uncompiled/unexecuted against CPython**, not passes.

`tests/Boundary/RootedBoundary.csproj` roots **all TranslatedCPython and
Managed.CPython methods** and runs existing Behavior/lifecycle `all` in NativeAOT
for both forms. Normal consumer trimming is independent. The actual-output
auditor checks generated/ownership/asset manifests, original authored links,
all actual C unit/object/link/build stages, frozen/config inputs, actual static
module rows and compiled initializers/Host exports, assembly/deps references,
IL calls, native imports and AOT ELF dependencies. Its reports retain raw/
processed/form/mode/artifact identities and unknown dependencies fail visibly.
Signatures come from actual compiled metadata and export names from the real
Host header/config, with no mirror Python structs or native engine backend.

The facade now carries the repository MIT notice through ordinary references.
`DeliveryNotices.targets`, imported by the sample/Delivery/rooted projects,
copies the installed .NET MIT text plus the **actually resolved NativeAOT target
pack's** third-party notices. Actual SDK target evaluation passes and identifies
`microsoft.netcore.app.runtime.nativeaot.linux-x64/10.0.12`; no historical
runtime pack is assumed. The first evaluation's incorrect ResolvedRuntimePack
assumption failed, was repaired to use target/host ILCompiler packs, and both
logs remain. Actual product publication/copy/relocation is still unrun.

Fresh support validation: **19 audit rejection controls / 18 reporter controls**
pass. Actual metadata inspection of current shared DotCC.Libc reads **105 types,
1,391 methods, 4,584 IL call edges, 19 native imports**, with the five dynamic
service definitions retained/classified. This inspects the shared runtime, not
a CPython product. **24 authored Python/XML syntax checks** and frozen-selection
preservation pass. No fake or general substitute product establishes behavior.

### Native outcome classification and source-repair handoff

`blockers-{default,utf8}-source-validated.json` records six semantic groups plus all **ten**
original/dependent case sources/decorators. A separate fresh native **3/3**
default-encoding control passes with UTF-8 mode off, where those tests apply;
the full mode-on run executes their real upstream skips. This is a configuration
matrix finding, not a combined passing inventory or permission to alter IDs.
The source investigator now unwraps unittest decorators before attributing
files (and asserts `Lib/test/` origins). Earlier reports incorrectly paired a
skip wrapper's `Lib/unittest/case.py` filename with the original method's lines;
the new source-validated reports supersede those metadata paths. Original
upstream inventory discovery already unwraps them correctly. This correction
changes no executed callable, frozen selection or full-run outcome.

- LocaleCodec uses real `_testinternalcapi.EncodeLocaleEx/DecodeLocaleEx`
  (`Modules/_testinternalcapi.c:579–665`) with `current_locale=0`. Actual
  `Python/fileutils.c:594–637, 785–825` chooses UTF-8 filesystem conversion only
  when forced or preconfig UTF-8 mode is enabled; ordinary current-locale
  conversion rejects surrogatepass. Both helpers pass their exact surrogate
  bytes/ordinals with mode 1. Helpers/resources are present; no codec algorithm
  repair is needed for the selected Host configuration.
- Descriptor reproducers independently show class calls ignoring incompatible
  `__new__` assignments, while direct `__new__` calls reject them, and restored
  `object.__new__` continuing to reject valid init arguments after deletions.
  Repair target for A, **if an upstream repair is adjudicated**:
  `Objects/typeobject.c`'s `update_one_slot` special-case around **10665–10688**
  preserves `type->tp_new` instead of resolving the assigned wrapper's bound
  allocator. A candidate must derive the wrapper owner from the real
  PyCFunction and retain `tp_new_wrapper`'s subtype/static-base safety checks
  (**9096–9128**) before selecting its actual allocator, otherwise use the
  generic slot. It must restore the actual object allocator when valid and
  retain parent override/deletion propagation. This is a **proposed repair
  boundary, not an applied or validated patch**; do not globally relax
  `object_new`'s argument checks. Native concrete before/after-deletion values
  and TypeErrors are preserved in the semantic reports.
- Namespace ZIP reproducer creates real archives with identical module bytes,
  differing only by the explicit directory entry: absent entry gives
  ModuleNotFoundError, present entry imports value 42. Actual
  `Lib/zipimport.py:303–309` checks only literal directory membership. A minimal
  proposed upstream repair would infer a namespace directory from child-entry
  prefixes, retaining explicit-directory precedence and archive path handling;
  this is unimplemented, requires its own native tests/guarded A adaptation,
  and cannot solve the expectedFailure-marker gate on its own.
- ZIP `as_file`'s unentered context retains a bound reader and real live
  ZipFile handle; entering it releases that reader and correctly extracts and
  removes the temporary resource. Actual `importlib/resources/_common.py:122–170`
  explains this lifetime. The skipped test is unconditional, not missing data
  or a Linux failure to delete open files. Linux unlink behavior is not
  substituted for a Windows handle-release claim.
- Condition intentionally exports acquire/release and selected private methods,
  but not RLock's private `_recursion_count` (`Lib/threading.py:281–302`). Real
  RLock and Condition's underlying lock both observe recursion depth two; the
  inherited wrapper test (`test_threading.py:2132`) unconditionally skips.

No upstream source, decorator or assertion was changed. Required skips and
xfail/unexpected-success remain failures. Under these constraints a fully green
inventory is irreducible without director/user policy adjudication; all affected
gates remain open while independent implementation proceeds.

### Round 4 commands, artifacts and open gates

CWD `/home/marius/p/dotcc`; private work
`cpython/build/upstream-profile/owner-b-r4-20261017`, evidence
`cpython/artifacts/owner-b-r4-20261017`. Exact argv/CWD/exit/timeouts are retained
in `native-investigation-commands.json`, `times-valid-before-commands.json`,
`times-validation-commands.json`, `times-repository-commands.json`,
`metadata-final-commands.json`, `support-validation-commands.json`,
`audit-rejection-final-command.json`, `aot-runtime-notice-final-command.json`,
`blocker-source-validation-commands.json`, and `native-default-encoding-command.json`.
The first unsafe native development
probe and first notice evaluation remain failed historical evidence.

```bash
cpython/build/upstream-profile/owner-b-r3-20261017/build/python -I -X utf8 cpython/tests/Upstream/run_inventory.py --implementation native-oracle --inventory cpython/tests/Upstream/inventory.frozen.json --report cpython/artifacts/owner-b-r4-20261017/native-frozen-utf8.json
python3 -B -m unittest discover -s cpython/tests/Audit -v
cpython/build/upstream-profile/owner-b-r3-20261017/build/python -I cpython/tests/Upstream/test_reporting.py
flock --close /tmp/dotcc-cpython-shared-build.lock dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release -p:UseLocalLalrCc=false --disable-build-servers -m:1 --filter 'FullyQualifiedName~ProcessTimesTests|FullyQualifiedName~PosixQueryTests|FullyQualifiedName~LibcTimeInttypesTests'
flock --close /tmp/dotcc-cpython-shared-build.lock dotnet test DotCC.FunctionalTests/DotCC.FunctionalTests.csproj -c Release -p:UseLocalLalrCc=false --disable-build-servers -m:1 --filter 'FullyQualifiedName~FixtureTests&DisplayName~process-times'
```

The full native command exits **1**; the four subsequent commands exit **0**.
For exact native compiler, emitted-project JIT/AOT build/publish/run commands,
use the times command ledger (native `-Wall -Wextra -Werror`, normal `--runtime=c`
emission, no generated C# repair). A's acknowledged in-flight continuation owns
the actual 193/286-unit retries and profile macros; B duplicates neither.

Next: A completes core/profile publication; these wired hooks then build/run
the facade/useful sample/Behavior/Upstream/normal-trimmed and rooted AOT boundary
against **those** artifacts, with exact cell identities. Canonical products are
currently absent. P1/D3, actual ABI/static slots/varargs, all interpreter cells,
P0–P6 acceptance and final shared downstream regeneration remain unpassed.
This round has no commit, staging, cleanup/reset, worktree, branch replacement,
push, source transplant or nested agent. All previous logs and frozen IDs remain.

## A integration contract — Owner B round 3

**Regenerate the authored Host and facade together:**
`dcp_operation_begin(void *context, int can_cancel)` now has a second argument.
`TranslatedExports.Operation` passes `CancellationToken.CanBeCanceled` as 0/1.
Ordinary entry preserves the real Python trace; explicitly cancellable entry
owns/restores its temporary trace. The previous unconditional trace was
reproduced as a native assertion failure, then repaired. Direct updated Host
emission/link succeeds; the facade/Upstream entrypoint still awaits A's core.

**Separate test profile is ready for A integration.**
`tests/Upstream/helper-profile.json` selects **30 real static modules / 94 C
sources** (including required `unicodedata`, `_socket`, `_posixsubprocess`,
`select`, `fcntl`, `binascii`, `_csv`, three `_test*capi` families, `_testbuffer`,
real static multi/single-phase import fixtures, CJK codecs and hashes).
`build/upstream-profile/owner-b-r3-20261017/helper-sources.json` contains exact
same-pin source paths/hashes, upstream Setup paths/lines/rows, actual native
flags/dependencies, expanded Makefile closure, and **28 generated assets**.
Native ELF needs only libc/libm; no shared extension or dynamic libpython was
built. Native-only platform services do not silently enter product selection.

A must add profile `upstream-tests-static`, retaining the **actual product
parser/compiler/evaluator/object model/GC/GIL/Host and 16-bit ABI**, union and
deduplicate those source rows, generate its separate static `Modules/config.c`,
and stage the pin's `Lib/test` support/data plus unittest/doctest dependencies.
Produce test-profile sysconfig data from the real reviewed DotCC config; the
native 32-bit-wchar pyconfig/sysconfig/getpath artifacts are oracle/generator
evidence, not the translated platform configuration. Keep every test-only API
out of the shipped product. No core emission or compiler repair was duplicated.

Use `tests/Upstream/Upstream.csproj` with explicit `CPythonProject`. It calls
`run_inventory.run_embedded` through the actual owning API, serially **inside**
the translated interpreter. `tests/Upstream/campaign_hooks.py:run_suite` is an
integration-ready framework hook using `ctx.managed`, `ctx.run`, ordinary
product/provenance checks and forms/modes. It consumes separately published
profile libraries; it never emits or runs native Python. Add the profile to the
recipe and declare upstream `verify_profiles=('upstream-tests-static',)` with
this hook and a bounded suite timeout (900 seconds); source/asset preparation
and publication stay in A's existing framework hooks. All four profile cells
remain blocked/unrun until those real artifacts exist. Missing assets fail.

**Frozen selection is complete, native qualification is not passing.**
`tests/Upstream/inventory.frozen.json` classifies all **5,134 unique IDs** across
all **23** seed modules: **4,867 required / 267 individual exclusions**. All 18
mandatory floor IDs remain required. Every exclusion has exact upstream path,
source evidence, unsupported feature/resource and affected contract; no missing
C helper is excluded. The sole platform-only module placeholder is the Windows
registry finder (`Lib/test/test_importlib/test_windows.py`, missing `winreg` on
Linux). It is explicit rather than a missing-helper blanket exclusion.
The reporter preserves eight repeated load_tests/doctest occurrences in original
upstream order, for **5,142 total discovered occurrences**. Fresh execution loads
one seed immediately before running it and unloads newly imported test modules
using the pin's serial regrtest lifetime policy; it starts no regrtest process.

The native selected inventory executes **4,875 occurrences / 4,867 unique IDs**:
**4,868 passing occurrences, 4 required skips, 3 required expected failures,
0 errors/ordinary failures/failed subtests, 0 unexecuted required IDs**. Exit is
**1**, correctly. Seven cases stay REQUIRED and block the native/qualification
gate: two LocaleCodec surrogatepass skips; descriptor bad-new/restored-new
expected failures; importlib's zip-as-file unsupported skip and namespace ZIP
expected failure; ConditionAsRLock's recursion-count skip. Full tracebacks and
exact IDs are in `artifacts/owner-b-r3-20261017/native-required-blockers.json`.
No expected outcome is normalized to success or silently excluded.

`prepare_profile.prepare(ctx, private_base)` uses ordinary copies of the already
acquired pin, campaign process support, native lock and `make -j2`. It acquires
nothing and edits no A cache/manifests. The first real build failed because the
minimal freezer omitted real `_Py_Get_Getpath_CodeObject` needed by static
`_testinternalcapi`. The recorded correction copies A's existing **exact-pin
freezer** into private tools and overrides only Makefile bootstrap-generator
variables. Real getpath/core/helper sources then link; no success stub, omitted
helper assertion or native backend is introduced. `--resume` continues owned
private outputs; `--snapshot-only` collects actual Makefile/generated evidence.

B edited only assigned Host/facade/tests and these two progress ledgers. A's
compiler/config/scripts/other docs/native trees and unrelated edits remain
preserved. Native Python remains a discovery/control/generation tool. No product,
P0–P6, four-cell, AOT interpreter, delivery or shared regression pass is claimed.

## Round 3 exact commands and evidence

CWD `/home/marius/p/dotcc`. Private base
`cpython/build/upstream-profile/owner-b-r3-20261017`; logs
`cpython/artifacts/owner-b-r3-20261017`. Process argv/CWD/timeouts/exits/logs are in
`preparation-commands.json`, `validation-commands.json`,
`final-validation-commands.json`, `host-emission-commands.json` and
`native-audit-commands.json`. Native builds hold
`/tmp/dotcc-cpython-native-build.lock`, always `make -j2`; the limited Host
emit/link and prerequisite check hold the shared build lock. No build lock is
held while private native assertions execute. No nested agent, commit, staging,
reset/cleanup, worktree/branch replacement, push or upstream DotCC transplant.

```bash
python3 -B cpython/tests/Upstream/prepare_profile.py --work cpython/build/upstream-profile/owner-b-r3-20261017 --artifacts cpython/artifacts/owner-b-r3-20261017
python3 -B cpython/tests/Upstream/prepare_profile.py --resume --work cpython/build/upstream-profile/owner-b-r3-20261017 --artifacts cpython/artifacts/owner-b-r3-20261017
python3 -B cpython/tests/Upstream/prepare_profile.py --snapshot-only --work cpython/build/upstream-profile/owner-b-r3-20261017 --artifacts cpython/artifacts/owner-b-r3-20261017
cpython/build/upstream-profile/owner-b-r3-20261017/build/python -I cpython/tests/Upstream/classify_inventory.py --seed cpython/tests/Upstream/inventory.seed.json --output cpython/tests/Upstream/inventory.candidate.json --discovery cpython/artifacts/owner-b-r3-20261017/classification-resources-final.json
cpython/build/upstream-profile/owner-b-r3-20261017/build/python -I cpython/tests/Upstream/test_reporting.py
cpython/build/upstream-profile/owner-b-r3-20261017/build/python -I cpython/tests/Upstream/run_inventory.py --implementation native-oracle --inventory cpython/tests/Upstream/inventory.frozen.json --report cpython/artifacts/owner-b-r3-20261017/native-frozen-inventory.json
```

The historical first preparation invocation failed (retained `helper-build.log`); the
recorded resumed builds succeed. For a fresh reproduction, choose a new private
work leaf; preparation deliberately refuses to replace the existing tree. The
successful final reporter has **18 controls**,
including required skip/expected-failure, assertion/error/subtest, class
setup/teardown, interrupted partial evidence, stale/unclassified inventory,
hidden module-helper skip, doctest identity, and every duplicate assertion
occurrence. These controls test the reporter, not the interpreter. The helper
native behavior fixture now passes **5 groups, 0 failed, 4 unselected**, including
the previously failing named-Unicode grammar assertion. Upstream frozen native
execution remains failing for the seven required outcomes above.

The first broad native run (`native-candidate-all.json`) remains evidence:
4,943 occurrences, 4,855 passed, 13 errors, 34 failed subtests, 70 required skips,
3 expected failures, exit 1. It exposed `_csv` and stale JSON exception classes
from preloading every module. `_csv` was built from its actual source; fresh
per-seed execution fixes the harness ordering. The targeted JSON control then
passes **182 occurrences**. Earlier 4,134-ID discovery was incomplete because
unittest hid missing `_testmultiphase` behind module skips. The reporter now
blocks that; real static fixture modules expose 1,000 more importlib IDs. All
historical failures/counts are retained and superseded only by named later runs.

No B shared-libc/header repair was required. Production SQLite/QuickJS/MsQuic/
picotls and affected Blink/libsmb2 final regressions remain A/coordinator/P6
work for A's compiler changes; B inherits no pass. The next action is A's real
separate-profile/core build with the updated Host signature, followed by the
managed hook across raw/processed JIT/NativeAOT. The seven native blockers must
be resolved or explicitly adjudicated without silently changing this selection.

## A handoff — Owner B round 2

- Compiler reproducer remains `tests/Abi/cpython_pointer_atomics.c`: native C11
  pointer CAS/exchange passes; prior generated C# failed CS0306. A owns repair
  and the whole-manifest retry. B will not edit compiler/config/scripts.
- Add **authored** `src/Host/dotcc_cpython_host.c` as an ordinary product C unit,
  with existing product config/includes plus `src/Host` (header is alongside it).
  Never native-link this boundary into the delivered product. All `dcp_*`
  external definitions must be public static exports on
  `Managed.Interpreters.CPython`; exact signatures and fresh probe evidence
  are recorded below. This boundary owns real PyConfig/streams/errors/capsules,
  uses upstream API calls, and replaces no interpreter algorithm.
- Recipe integration must append
  `Unit(ctx.root / 'src/Host/dotcc_cpython_host.c')` to selected units using
  their existing common flags (plus `-I <campaign>/src/Host` if needed), then
  add the three link exports below. Record this as an authored input alongside
  the pinned Makefile-derived closure; do not pretend the Host file came from
  the upstream archive or omit its provenance from the product source manifest.
- Add link flags **`--export-inline Py_INCREF --export-inline Py_DECREF
  --export-enum PyUnicode_4BYTE_KIND`**. The adapter uses actual nested
  `CPython._object*`, direct object/numeric/Unicode APIs, and this exported enum.
  Do not add forwarding C wrappers or generated-source repairs for these APIs.
  The ordinary boundary source also needs the real core `_PyEval_SetTrace`
  symbol (exact-pin declaration matches `Include/internal/pycore_ceval.h:24`).
  Its int result preserves audit/monitor failure reporting, unlike the public
  void `PyEval_SetTrace`, which reports failures as unraisable exceptions.
- Keep `src/Managed.CPython/TranslatedExports.cs` in the facade and select the
  actual product with `CPythonProject`. No stub/native fallback is permitted.
- **`unicodedata` is needed by the product's named-Unicode-escape language
  contract**, not just discovery. Add its actual `Modules/unicodedata.c` and
  built-in registration/generated data through A's reviewed manifests/native
  controls. The new required fixture currently fails instead of hiding this.
- Separate test profile remains required: `_socket`, `_posixsubprocess`,
  `_testcapi`, `_testlimitedcapi`, `_testinternalcapi`. Exact Setup rows and **65
  source paths** for those modules plus `unicodedata`, blocked import chains,
  and **18 mandatory source-declared IDs** are in
  `tests/Upstream/helper-contract.json`. Seed selects that mandatory floor;
  full classification is still open, with zero exclusions. Import support in a
  test profile does not promise subprocess/network execution in the product.
  These are identified requirements, not a proven complete helper/import
  closure; rediscovery may expose further required dependencies.

Round 2 results follow; this section is an integration request,
not evidence of completed translation or a passed product gate.

## Round 2 implementation and evidence

`src/Host/dotcc_cpython_host.{c,h}` owns real isolated PyPreConfig/PyConfig,
explicit search paths, UTF-8 startup and copied nonfatal PyStatus. Successful
startup saves the real owner thread state and releases GIL. A failed partial
initialization is conservatively **not reusable**; only completed finalization
releases that reservation. Host stdout/stderr and their double-underscore
counterparts are real `io.StringIO`; stdin is explicit empty input/EOF.
Captured streams and exception fields are length-bearing (including embedded
NUL). Diagnostics escape lone surrogates as visible UTF-8 backslash text;
value conversion separately preserves actual ordinal values when requested.
Exception formatting failures remain visible and retain frames that did format.

`TranslatedExports.cs` now implements every abstract export with direct static
calls to the generated class. No native backend, reflected lookup, fake product,
success stub, or manually repaired output exists. It is **not yet compiled or
executed with the full translated core**. Limited actual object/link output
confirms public host signatures, nested object types, refcount inline exports,
and `public const int PyUnicode_4BYTE_KIND = 4`. Product prerequisite still
fails because the canonical translated project is unavailable.

Actual C METH_O callable storage is capsule-owned, including stable method/name
storage. `CallbackThunks.Int64` is an address-taken static managed function with
the emitted signature `delegate*<long,long,long*,byte*,ulong,int>`; using an
unmanaged thunk convention here would be incorrect. It contains managed
exceptions, writes a bounded valid UTF-8 error (including safe truncation and
bad exception-message fallback), and returns failure for C to set RuntimeError.
Real CPython performs argument conversion and constructs a new integer result.
Managed roots persist through worker shutdown and finalization; native capsule
destruction never releases managed tokens prematurely.

Execute/Evaluate/Call have cooperative CancellationToken brackets. Registration
drains before clearing the atomic cancellation flag, restoring the prior trace,
or freeing context. A real CPython trace hook polls the flag; `PyTraceBack_Here`
retains the actual interrupted frame (the first native control exposed that a
trace-hook error otherwise had no Python frame). The explicit `host.wait_ms`
uses real timed CPython lock waits, releases/reacquires GIL, and polls at at most
10 ms intervals. This is the supported interruptible blocking API; arbitrary
blocking C extensions/time.sleep are not claimed interruptible. Cancellation
raises real KeyboardInterrupt; a Python program may catch it. No evaluator,
thread abort, external kill or harness timeout substitutes for interruption.

Owner disposal still joins through CPython's `threading._shutdown` with no host
monitor, then finalizes before releasing roots. Because upstream shutdown omits
daemon joins, the boundary also drains **active C host waits**, releasing GIL
between polls, and rejects new host waits after ordinary shutdown. This prevents
freeing the context while a daemon still uses it; it retains CPython's ordinary
daemon semantics. Added real-product Behavior
assertions cover cancellation/reuse, precancelled entry, output on failure,
NUL-bearing error messages and callbacks pending during shutdown. They remain
uncompiled/unexecuted until A provides the actual core. The consumer's default
stdlib path now matches A's packaged `cpython/stdlib` target.

Fresh results, CWD `/home/marius/p/dotcc`, work
`cpython/build/owner-b-r2-20261017`, logs
`cpython/artifacts/owner-b-r2-20261017`:

- Native C boundary: `cc -std=c11 -Wall -Wextra -Werror` against A's existing
  exact-pin libpython plus HACL archive, then actual execution: **exit 0**.
  Two initialization cycles, NUL/non-BMP/surrogate stream policy, real error
  frames/formatting failures/SystemExit containment, refcounts/Unicode,
  callback conversion/overflow/failure, two worker callbacks per cycle,
  infinite-loop and 30-second wait interruption, restored prior trace,
  same-interpreter reuse, and finalization flush error (`-1`) all asserted.
  Four cancellations took **0.050152/0.050405/0.050146/0.050397 seconds**, each
  required below 2 seconds. Separate bad-startup control returns **0** after
  asserting a real nonfatal initialization Error and unsafe-reuse rejection.
  **These are native C callback/control results, not managed CPython callbacks.**
- Actual emitted `managed_callback_probe.c` + authored static thunk:
  **21 assertions pass in JIT and NativeAOT**, including two real translated
  pthread workers concurrently entering managed code, GC while rooted,
  overflow/expired-token/malformed-message containment and bounded UTF-8.
  Builds/emission exit **0**, no warnings/errors. **This is emitted-C/managed
  ABI/service evidence, not CPython interpreter execution.**
- Fresh Contracts JIT/AOT: **32 + 3** ownership/text/root/uncertain-state
  assertions in each runtime; builds/runs exit **0**, no warnings/errors.
- Final authored Host source with A's 16-bit config: limited object emission
  and managed-library link both exit **0**. Untouched standalone generated
  build exits **1**, **204 distinct CS0103 diagnostics** for 106 missing real core
  symbols (PyMutex_Lock, PyLong_Type, etc.). No CS0306 appears in this fresh
  diagnostic. This does not replace A's pointer-atomic regression/full retry.
  No stub core was supplied, and no interpreter/layout execution is claimed.
- Required behavior fixture now reports **4 passed, 1 failed, 4 unselected**,
  exit **1**: `language.named_unicode_grammar` exposes missing `unicodedata`.
  No assertion was removed to retain the prior green subset.
- Reporter tests: **7 pass**, exit **0**. Fresh upstream discovery exits **1**:
  **1,296 discovered, 15 blocked modules, 18 configured mandatory IDs,
  0 executed, 0 excluded**. Missing import diagnostics remain preserved.
- Actual product-prerequisite check exits **1**, correctly reporting missing
  translated product. All raw/processed CPython JIT/AOT, consumer, fully rooted
  interpreter AOT, delivery and qualification gates remain **blocked/unrun**.

Exact argv/cwd/exit/expected-exit/logs are in
`artifacts/owner-b-r2-20261017/validation-commands.json`; thunk command details
are in `artifacts/owner-b-r2-20261017/thunk-commands.json`. Build commands all
held `/tmp/dotcc-cpython-shared-build.lock`; native builds held
`/tmp/dotcc-cpython-native-build.lock`. No interpreter acquisition, native
interpreter rebuild, competing own build, full-manifest attempt or product
publication was performed by B. Logs remain private; the one-off validation
transcript under build is not another campaign runner.

Next actionable task: **A completes the real selected-core build with the Host
unit, three link exports and Unicode module; coordinator runs the facade/consumer
and Behavior harness in each required cell**. Build the separate real helper
profile, rediscover, classify every case and freeze the upstream inventory.
Required helper skips stay failures. B made no shared-libc/header repair, so
production downstream regeneration is for A's actual compiler changes/P6;
no inherited downstream pass is claimed. No commits, staging, reset, cleanup,
worktree, branch replacement, source transplant or nested agent was used.

Round 2 changed **21 authored paths (9 new, 12 modified)**; exact paths and
before/after hashes are in `artifacts/owner-b-r2-20261017/owned-paths.json`.
`preservation.json`, `initial-status.txt`, `final-status.txt` and tracked-diff
snapshots retain the shared-checkout evidence. Head remains
`bb2f9b1f1399ca1f6ac39c7c42f964796d7dd183`. B edited no A-owned path, compiler,
shared libc or supplied C header; A's concurrent compiler/config/campaign
progress and pre-existing unrelated changes remain preserved. Fresh
`git diff --check`, owned-text whitespace checks and **7 Python AST syntax
checks** pass; these are preservation/syntax results, not interpreter tests.

## Prior round evidence (historical; superseded implementation notes)

### Authored projects and actual binding prerequisite

Both `src/Managed.CPython/Managed.CPython.csproj` and
`samples/ManagedConsumer/ManagedConsumer.csproj` expose `CPythonProject`, defaulting
to the canonical processed product. The consumer has ordinary references to the
facade and selected product; its application package copies to build/publish
output. A can scaffold the root solution around these existing projects.

`PythonRuntime.Translated.cs` calls `TranslatedExports.Create()`. That adapter is
**absent** until generated signatures are available and verified. The product
project deliberately fails its prerequisite target while the product/binding is
missing. It supplies no mock, native libpython backend, success stub, reflected
export lookup, or repaired generated C#. `CPythonExports` is an internal abstract
binding contract, inaccessible as an application-provided backend. Its methods
have no implementations yet. Ordinary facade/consumer/Behavior builds and their
execution are **unverified/blocked**, including every raw/processed JIT/AOT cell.

`tests/Contracts` compiles the authored support/contracts while excluding
`PythonRuntime.Translated.cs`. It cannot initialize an interpreter. Its JIT/AOT
passes below test bookkeeping, text and managed callback roots only.

### Implemented ownership and boundary contracts

- One process runtime reservation and initializing owner thread. Owner entry
  restores its saved CPython thread state/GIL; exit saves state/releases GIL.
  Foreign-thread and reentrant owning calls are rejected before CPython entry.
  Python workers are real interpreter threads and may call the constrained
  `Func<long,long>` callbacks. Callbacks cannot reenter the owning facade.
- `PythonValue` owns a new reference; clone increments it. No raw pointers,
  borrowed references or stolen-reference operations escape the facade. All
  values must be explicitly released before finalization. Runtime/value types
  have **no finalizers** calling into Python. Forgotten values visibly block
  shutdown, rather than permit a finalizer to touch a dead interpreter.
- Shutdown checks outstanding values, restores the owner GIL, joins Python
  workers through CPython's real shutdown path, then finalizes on the owner.
  No host monitor is held across entry, callbacks or join. Callback roots remain
  until finalization returns. Failed joins release the still-live GIL and retain
  the process reservation; uncertain restore/save/finalize state prevents reuse.
  `Py_FinalizeEx` stream-flush failure remains an error even after finalization.
- Callback registrations use monotonically unique tokens in an explicit strong
  root registry. Managed exceptions become failure results for the future C
  thunk, which must set Python `RuntimeError`/return NULL. Tokens expire only
  after drain; active removal is rejected. Actual `METH_O` capsule conversion,
  AOT-compatible static thunk signatures and Python error mapping remain unbound.
- UTF-8 conversion is strict and length-bearing, preserving embedded NUL.
  Unicode value conversion uses explicit ordinal values for CPython's 4-byte-kind
  API, preserving non-BMP characters. Lone UTF-16 surrogates reject by default;
  explicit `Preserve` round-trips their ordinal values without replacement.
  NUL-terminated source/name/path operations reject NUL before interpreter entry.
  CPython Unicode storage is never cast to/from .NET string/wchar storage.
- `PythonOptions` requires existing explicit stdlib/application directories.
  The binding must use isolated `PyPreConfig`/`PyConfig`, UTF-8, no environment/
  user-site/signal-handler startup, and the **16-bit** config ABI. It must copy
  `PyStatus` function/message before clearing config, return Exit as a managed
  exception/status instead of terminating the process, and capture real error
  type/message/traceback frames while releasing temporary Python references.

These contracts are authored code, not proof of actual interpreter behavior.
Cooperative interpreter cancellation, general callback signatures, and a fully
bound host stream/error/traceback implementation remain required work.

### Consumer and required harness foundations

`samples/ManagedConsumer` runs two intended initialization cycles. Each reads
exact `{"items":[2,3,5]}` bytes, imports `campaign_app`, invokes managed `host.double`
three times, checks return 20 and exact output
`{"sum":20,"values":[4,6,10]}`, checks captured workflow output, exercises both
host-owned stream captures, checks caught and host-propagated traceback frames,
recovers after the error, round-trips non-BMP/NUL/surrogates, releases values and
finalizes. The Python workload uses real `json`, `io` and `contextlib` operations.
It has not run through the unbound facade.

`tests/Behavior` is a separate intended real-product harness with the same
project property. It registers actual managed double/failing callbacks, invokes
`tests/fixtures/run_required.py` through length-bearing Python arguments, checks
callback counts, outstanding-reference shutdown rejection, clone/dispose,
foreign owner-thread/dead-runtime rejection, and two cycles. It is uncompiled
and unexecuted pending the adapter.

Fixtures contain eight explicit groups covering objects/closures/descriptors,
big integers, Unicode/bytes/containers, cyclic GC/weakrefs, generators/coroutines,
exceptions/tracebacks, specialization/deoptimization, every required stdlib name,
filesystem operations, real worker threads/TLS/contextvars/conditions, managed
callbacks, missing source/import/file, recursion and buffer exports. The runner
records passed/failed/unselected rows, retains tracebacks, rejects wrong versions
and `-O`, and explicitly lists unimplemented required embedding gates. A passing
fixture subset cannot close the broader lifecycle/cancellation campaign gate.

`tests/Upstream/run_inventory.py` runs serial unittest inside the selected
interpreter. Required skips/expected failures, missing helpers, stale IDs,
unclassified discovered cases and empty inventories fail qualification. Every
exclusion requires upstream path, reason and affected contract. Its seed is
explicitly **not frozen** and currently selects/excludes zero cases.

### Prior round validation and commands

CWD `/home/marius/p/dotcc`. `probe_work=cpython/build/owner-b-20261017`;
`probe_logs=cpython/artifacts/owner-b-20261017`. Build flags and outputs below are
the actual commands; all build invocations held the shared flock.

```bash
flock /tmp/dotcc-cpython-shared-build.lock dotnet build cpython/tests/Contracts/Contracts.csproj -c Release --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/contracts --disable-build-servers -m:1
dotnet "$probe_work/contracts/bin/Contracts/release/Contracts.dll"
dotnet "$probe_work/contracts/bin/Contracts/release/Contracts.dll" --faulted-owner
flock /tmp/dotcc-cpython-shared-build.lock dotnet publish cpython/tests/Contracts/Contracts.csproj -c Release -r linux-x64 -p:PublishAot=true -p:StripSymbols=true --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/contracts-aot --disable-build-servers -m:1 -o /home/marius/p/dotcc/cpython/build/owner-b-20261017/contracts-aot-publish
timeout 20s "$probe_work/contracts-aot-publish/Contracts"
timeout 20s "$probe_work/contracts-aot-publish/Contracts" --faulted-owner
```

Both builds exit 0 with no warnings/errors. Both runtimes pass **32** text/
ownership/root/drain assertions and **3** separate uncertain-runtime reservation
assertions. Logs: `contracts-build-final.log`, `contracts-run-final.log`,
`contracts-faulted-owner.log`, `contracts-aot-build-final.log`,
`contracts-aot-run-final.log`, `contracts-aot-faulted-owner.log`. Earlier 31-check
logs are retained but superseded by these final runs.

```bash
flock /tmp/dotcc-cpython-native-build.lock cpython/build/native/build/python -I cpython/tests/fixtures/run_required.py --suite behavior --work cpython/build/owner-b-20261017/behavior-native-final --report cpython/artifacts/owner-b-20261017/behavior-native-final.json
flock /tmp/dotcc-cpython-native-build.lock cpython/build/native/build/python -I -c 'import importlib.util,pathlib,json; p=pathlib.Path("cpython/tests/fixtures/thread_lifecycle.py"); s=importlib.util.spec_from_file_location("cp_b_threads",p); m=importlib.util.module_from_spec(s); s.loader.exec_module(m); m.workers(); m.ordinary_errors(pathlib.Path("cpython/build/owner-b-20261017/behavior-native-work")); print(json.dumps({"threads.workers_tls_condition":"passed","lifecycle.ordinary_errors":"passed","scope":"native only; callback and translated lifecycle gates unrun"}))'
flock /tmp/dotcc-cpython-native-build.lock cpython/build/native/build/python -I cpython/tests/Upstream/test_reporting.py
flock /tmp/dotcc-cpython-native-build.lock cpython/build/native/build/python -I cpython/tests/Upstream/run_inventory.py --discover --implementation native-oracle --inventory cpython/tests/Upstream/inventory.seed.json --report cpython/artifacts/owner-b-20261017/upstream-discovery.json
```

The first command exits 0: **4 passed, 0 failed, 4 unselected**. The second exits
0 for the two non-callback thread/lifecycle groups. The third exits 0 with **7
reporting tests**, including deliberate failed assertions, failed subtests and
required skips. These synthetic reporter outcomes do not qualify interpreter
semantics. The fourth exits **1**: **1,296 discovered IDs, 15 blocked modules,
0 selected/executed/excluded**. Raw errors name missing `_socket`,
`_posixsubprocess`, and `unicodedata` imports (including parser `\N` dependencies).
The report preserves each module's complete traceback. Required test helpers and
test-only import dependencies need a separate test-profile decision; they are
not silently excluded or added to the shipped product by this runner.

Logs/reports: `behavior-native-final.{log,json}`, `threads-lifecycle-native.log`,
`upstream-reporting-tests.log`, `upstream-discovery.{log,json}`. Exact-pin native
Python is A's existing oracle/generator, never the product execution backend.
Seven owned Python files pass `ast.parse` (`python-syntax.txt`); `git diff --check`
passes. Neither check is interpreter or consumer qualification.

The product prerequisite was explicitly checked with
`flock /tmp/dotcc-cpython-shared-build.lock dotnet msbuild cpython/src/Managed.CPython/Managed.CPython.csproj -t:RequireTranslatedCPythonBinding -nologo`:
exit **1**, with the expected real-product-unavailable diagnostic
(`product-binding-prerequisite.log`). This is a negative prerequisite check,
not a consumer build or passing interpreter test.

### Prior round preservation and handoff

Changed-path inventory: `artifacts/owner-b-20261017/owned-paths.json` (30 authored
paths). Ancestor/owned-directory searches found no `AGENTS.md`. Existing PLAN,
compiler and unrelated campaign changes are preserved. B did not edit config,
scripts, solution, `tests/NativeOracle`, compiler files or the main ledger, and
made no shared-libc change, source acquisition, native interpreter build, full
translation/publication, reset, cleanup or commit. Native probes and private
outputs are separate from A's source/build products. Production downstream
regeneration remains A/coordinator work; B changed no shared implementation.

Next: A fixes/retries C11 pointer atomic CAS/exchange lowering using the
native-passing `tests/Abi/cpython_pointer_atomics.c` and actual-header evidence
in [abi-progress.md](abi-progress.md), then retries the real whole manifest with
the selected 16-bit configuration. Provide generated symbols/type locations/inline exports
and product asset paths. Then implement `TranslatedExports` using **direct actual
translated exports**, verify header ABI/status/callback signatures, and build/run
the owning consumer and Behavior harness in every required matrix cell. Preserve
the remaining cancellation, lifecycle, helper-inventory and delivery gates as
open. This round is a handoff milestone, not campaign completion.

## B37 — explicit isolated startup options, native controls complete

`PythonOptions` now adds init-only `WriteBytecode` (default false) and `Utf8Mode`
(default true). The constructor and directory validation are unchanged. The
additive C ABI is `dcp_initialize_configured(void*, const char*, const char*, int,
int)`, with 0/1 validation before preinitialization. Existing `dcp_initialize`
retains defaults (0,1). `TranslatedExports.Initialize` passes these values at its
existing executor entry after the B34 captured program-owner guard; executor,
context leases, nullable binding, callbacks, cancellation, drain and value ownership
are unchanged. PyPreConfig/PyConfig receive real options before imports/discovery.

The actual authored Host linked to the new exact-pin native archive passes four
invalid-input clean/nonfatal controls and six initialization generations covering
defaults, all four combinations and defaults again. Real flags and fixture-cache
files match; isolated startup ignores hostile Python environment variables. This
is native boundary evidence. The new owning startup and full qualification
harnesses remain unrun until normal whole-closure regeneration exposes the new
export; old generated libraries cannot compile the current facade. No generated
repair or alternate backend is permitted.

[ABI/source snapshot](../artifacts/owner-b-r37-20261018/startup-api-abi-source-ready.json)
and [native commands](../artifacts/owner-b-r37-20261018/native-remaining-commands-suite-loaders.json)
retain the exact scope. Existing B34 owning/thread/callback/lifecycle controls
remain required in the final matrix.

B37's additional ordinary-suite wiring selects the same explicit qualification
options in `tests/Upstream/Program.cs` before discovery. Two temporary owning
PythonValues assert the actual bytecode/UTF-8 flags and are disposed before
bootstrap/runner imports. The existing framework hook still passes the fresh
consumer-owned target stdlib/app copy; runner, inventory, failure handling and
product defaults are unchanged.
[Additive ordinary-suite seal](../artifacts/owner-b-r37-20261018/ordinary-upstream-startup-source-ready.json)
retains the original ten-source startup seal separately. Actual new-product
execution remains pending the combined regeneration.

B37 final feature review selects common `HAVE_SYMLINK=1` after B39's validated
Linux-x64 source `4d9c60fa…`: native 361, current-Libc JIT/AOT 180 each, 21 units
with zero skips, and matching legacy/owner errno/result/extent observations. The
original immutable-V21 RED remains retained. Existing shared libc OS primitives
`symlink`/`readlink` provide the private Linux boundary; no native Python backend
is introduced. Other-host fallback remains unqualified. Both PIPE and SYMLINK
selectors pass the final ordered 193/286-unit/Clinic/header audit and five pure
guard controls. The final [B37 source seal](../artifacts/owner-b-r37-20261018/source-ready.json)
requires ONE combined A31 capture and fresh same-C/actual-product tests; no
CPython pass or completion-policy waiver follows from these service controls.


## B40 — current actual startup, raw counts, and explicit acceptance semantics

The actual V22 consumer binds its own library `0ff85f9f…` / MVID
`e016dffa-a975-4813-b651-6aa0c13ac835`, 98 library/14 facade/four entry
Compile/PDB documents, separately from canonical `cc68e84b…`. Four invalid
boolean controls and six direct Host generations match the exact native
transcript; six owning generations pass all options/defaults/callback/cache
checks with one static-program context. The unchanged original104 mode-0
comparison is 99 PASS/2 SKIP/3 XFAIL with all native/source bindings matching.

The first unchanged full4867 RAW JIT run completes 4859 unique PASS, four SKIP,
three XFAIL and one ERROR; zero FAIL/fatal/unrun. Its 4867 PASS events include
eight repeated loader IDs. The original failed report/exit remains preserved.
Native-faithful isolated embedding retains C unless the consumer selects its
locale before preinit. Explicit and legacy actual owners match native: explicit
consumer C.UTF-8 fixes the Unicode-directory reduction. Both surrogatepass cases
still SKIP natively/actually in mode0, and both PASS in supported mode1 with
unchanged methods and matched actual/native source/configuration bindings.

The user explicitly accepts only five identity-bound native exceptions (two
unconditional SKIPs/three XFAILs). The coordinator separately authorizes exact
two-case supported configuration coverage; mode0 skips are retained, and no
other skip/error/unexpected success/fatal/unrun or wrong source/PE is covered.
Eighteen focused acceptance controls pass. Revised ordinary Upstream and R37
qualification startup sources select C.UTF-8 on the existing program owner before
preinit; separate reference-only entries compile/bind to the exact actual PE.
The final correctly-started primary full run must still execute the metadata case
PASS. Public defaults and Host/runtime/header algorithms are unchanged.

[Exact evidence and remaining gates](../artifacts/owner-b-r40-20261018/final-handoff.json),
[five-exception manifest](../tests/ActualQualificationR40/five-exceptions.json),
[integrated gate](../tests/ActualQualificationR40/acceptance.py) and
[retained first-full decision](../artifacts/owner-b-r40-20261018/first-full-integrated-acceptance.json)
keep raw counts separate from acceptance. New193/both286 forms/JIT+AOT/full
D10/address/lifetime/ABI/thread/GC/lifecycle/rooted/trimmed/delivery/audit/downstream/
reproduction/P0–P6 remain required. No final matrix pass is claimed.
