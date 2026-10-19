# ABI and host-service progress (Owner B, 2026-10-17)

## Owner B34 — actual worker/callback boundary and logical stack ABI

The generic stack service is scoped to the EXACT existing RuntimeContext.
Both creation routes admit/retain before allocating or starting a thread and use
max(logical requested size, scoped minimum). The unrelated1 MiB default and all
logical pthread attribute validation/roundtrips remain unchanged. Larger requests
are preserved; no hidden count/aggregate cap exists. Resource errors retain
EAGAIN and rollback removes registry/admission/context retention. Actual allocation/
Thread constructor/Start exhaustion was not induced; lease rollback invariants
are tested and cleanup paths source reviewed.

Retiring a root scope keeps its policy installed through admitted workers,
descendants, detached exit and TLS destructors until drain; a replacement cannot
race them. The original null legacy binding is preserved, including raw heap/
process-cwd/termination semantics. Explicit-instance creation selects its supplied
owner under unrelated ambient bindings. ExecutionContext flow is suppressed
around Thread.Start, including the already-suppressed case. No host monitor spans
Python/GIL/callback/join. Static program context changes reject before interpreter
entry; same-context Python generations remain valid.

The new legacy regression fails normally against held d9 source and passes with
PthreadLib4ca64034…;49 affected current-Libc tests pass with zero skips. Independent
target-header compilation retains logical pthread_attr_t4 bytes; Linux native
pthread_attr_t is56 bytes. Native attrs and ordinary creation controls pass.
Current-Libc JIT/AOT both measure64 MiB physical stacks for null/default/explicit
16 KiB logical attributes on both routes;65 MiB remains65 MiB. Actual regenerated
RAW JIT Python worker/default/explicit32 KiB/descendant/shutdown callbacks also
measure64 MiB; synchronous owner callbacks remain on the256 KiB external caller.
Logical-vs-physical managed-backend semantics deliberately differ from native
physical attribute equality.

One executor plus2 workers requests192 MiB virtual stack reservation; plus8
requests576 MiB, for worker requests no larger than64 MiB. Larger requests add
their actual sizes; guards/runtime/commit overhead are additional. Ordinary Linux
VmSize/RSS are recorded alongside actual callback bounds, without an exhaustion
reproduction or a claim that process VmSize equals the requested stacks.
[Own actual callback/accounting observations](../artifacts/owner-b-r34-20261018/actual-jit-same-owner-owner.json),
[revised shared source/input proof](../artifacts/owner-b-r34-20261018/revised-shared-actual-input-proof.json)
and [actual facade/library inputs](../artifacts/owner-b-r34-20261018/actual-jit-same-owner-input-proof.json)
identify exercised bytes. Initial d9/V19 remains known bad and its green controls
incomplete; corrected V20 uses normal capture/regeneration. Saved B32 JSON case
now passes unchanged in focused JIT with interpreter reuse. Actual RAW NativeAOT ordinary contracts and the same unchanged JSON case/reuse
also pass, with177 exact ILC references and no second Libc assembly. Full RAW
JIT completes4867 unique IDs:4751 passes,12 IO failures,101 required skips and
3 XFAIL, zero errors/fatal/unrun.4759 pass events include8 repeated IDs.
[Terminal counts](../artifacts/owner-b-r34-20261018/full-required-journal-counts.json)
and [actual AOT proof](../artifacts/owner-b-r34-20261018/actual-aot-initial-input-proof.json)
retain the distinction. Final both-form ABI/new193/D10/rooted/trimmed/delivery/
audit/P6 remain open.


B34 hands the genuine safe execution lane to B35 for the12 newly exposed
ftruncate/offset failures; that nonowned runtime path was reported without B34
editing it. [Direct safe release](../artifacts/owner-b-r34-20261018/serial-slot-release-to-b35.json)
records37 terminal PIDs, no active/queued work and three independent free checks.
Initial d9 legacy-binding and first-harness AV failures remain retained; no
algorithm, recursion limit, required method/decorator/exclusion or generated
source/resource was patched.64 MiB is measured for these paths, not a universal
guarantee for arbitrary inputs or the still-open complete matrix.

## Owner B32 — actual regenerated stat metadata passes; full ABI remains open

The independent actual V18 consumer binds115 compiled source documents across
entry/facade/library, their own PE/PDB identities and the exact B31 stat source
section. Its library SHA256 `a9997b05…`/MVID
`5b4b8cc0-7ea0-4e36-89e4-11b9bb682ea2` is distinct from A28's canonical PE.
Minimal actual layout observations retain LP64/unsigned wchar16, PyConfig448,
stdlib_dir336 and module_search_paths320. This is not a rerun of the full ABI38
or final D10/ownership/thread matrix. Two ordinary owning lifecycles pass.

The exact unchanged R30 fixture now passes native and actual py_compile plus
SourcelessFileLoader42. Independently read physical modes0600/0644/0664, uid/gid1000
and sizes match both transcripts; the pyc is physically0664. Original first-ID
and prior38 error replays all pass, including their full required-journal outcomes.
The native fixture is a valid narrow oracle; no full native importlib seed pass
is claimed for the existing native profile with missing helper modules.

The immediately run4867-required raw JIT suite naturally SIGABRTs-6 after241.618s
at the unchanged JSON highly-nested-object decoding case. Final journal totals:
3804 unique started,3803 completed,3712 passed, zero errors,88 required skips,
3 required XFAIL and1064 without completed outcome. No stack/limit/source-case
adjustment or JSON diagnostic ran. See [B32 evidence](../artifacts/owner-b-r32-20261018/handoff.json)
and [current blocker](../artifacts/owner-b-r32-20261018/current-blocker.json).
Final raw/processed JIT/AOT for new193/full286, full ABI/D10/GC/TLS/ownership/
thread/lifecycle/rooted/trimmed/delivery/audit and P6 remain open; processedV16
and embedded193oldV11 retain their historical scope.

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

## Owner B30 — minimal new RAW after-controls prepared, UNRUN

The exact original deleted-cwd C fixture remains byte-identical and is selected
for fresh native and V17 RAW JIT NULL/ENOENT/absolute-recovery controls. The new
ordinary owning consumer measures actual selected PyConfig/basic target layouts,
version and PE identities, executes B29 startup/frozen checks without modifying
configuration, and repeats finalization. Its preparation uses no native Python,
shared runtime/header/compiler edit or hidden inspector build.

Execution follows A26's direct new RAW receipt and safe serial handoff. This is
early discovery preparation; full ABI38, expanded primitive IL/RVA/allocation,
GC/TLS/owner, processed/AOT and final-product gates remain required. The
[source preparation](../artifacts/owner-b-r30-20261018/source-preparation.json)
records exact inputs and the immediate full required-suite sequence.

## Owner B29 — focused cwd and real PyConfig boundary controls

Current `getcwd` source is repaired with named exception mapping, GNU allocation
and LP64 capacity rules. The same C fixture passes native 47 checks and current
Libc JIT/NativeAOT 47 process + 47 virtual-owner checks, plus 18 owner/GC/thread/
disposal assertions per mode; old-source failures remain retained. Virtual cwd
remains path-based: external rename/deletion returns ENOENT until absolute chdir
recovery, while process cwd follows rename. No new native service or ownership
framework is introduced; B23/B24 behavior outside this method is preserved.

Real selected C headers report PyConfig size448, stdlib_dir offset336 and unsigned
16-bit wchar. Actual typed fields in all four raw/processed test/embedded inputs
agree on all14 measured values; no guessed offset or memory patch is used.
The authored Host sets stdlib_dir through PyConfig_SetString with distinct owned
wide storage and passes exact-pin native two-lifecycle/callback/error controls.
Unchanged V16/V11 PEs separately pass typed API configuration, owned-copy/GC/
clear, non-ASCII relocation, ordinary failure and reinitialization controls.
Four unchanged frozen-finder IDs pass twice per V16 form with the full delivered
stdlib selected. This does not qualify the new Host or runtime in an interpreter.

[The B29 handoff](owner-b-handoff-r29.md) binds every focused consumer/reference
PE/PDB/source and current-Libc AOT input/object/executable. The initial partial-
stdlib test-root mismatch is preserved; corrected controls stay separate.
Normal immutable recapture, unchanged-C emitted matrix, actual new-profile/owning
proof and suite retry remain required. Full ABI/D10/matrix/P6 and B28's actual
skip/XFAIL/adjudication state remain open; no canonical publication occurred.

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

## Owner B27 — native listen constant and current-runtime backlog contract

Native and actual repaired shared headers agree: `SOMAXCONN=4096`, type int,
CPython default minimum128, socklen_t4 and sockaddr16; repeated inclusion passes.
The unchanged A23 reduction retains native PASS/selected-header RED/after PASS;
immutable V15 remains RED. All28 native/selected-header/direct current-Libc
JIT/AOT cases observe real kernel queue limits and TCP traffic/closure; four
wrong-byte controls reject. All23 unchanged focused network unit cases pass.
Each actual entry/reference PE, matching PDB/physical Compile input and every
AOT reference/object/executable is separately bound, including real Listen IL.
[B27 handoff](owner-b-handoff-r27.md) records source-ready hashes and release.

No runtime/layout/compiler/policy or actual CPython C input changes. New immutable
tools, emitted SAME-C direct/object raw/processed JIT/AOT, actual socket unit and
whole286 remain next-owner work. Full product ABI/D10/thread/GC/lifecycle/owning/
rooted/delivery/audit/upstream/P6 gates remain open; no historical pass transfers.

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

## Owner B25 — actual C typed runtime function-address mismatch reduced

The unchanged native resolver header/layout/function-pointer control passes.
V14 emits it with77 byte-exact runtime sections, but C# compilation fails CS0407
on hostent* versus runtime void* at the generated function resolver. Both
direct/object hostent and independent protoent reductions reproduce the same
boundary failure. A narrow same-C direct-call raw-JIT control matches native
`typed-host 2 4 127.0.0.1 h_errno=99 errno=123`, with its actual PE/MVID/PDB/source
binding. Source/resource equality supplies no function-pointer or matrix pass.

[B25 evidence and release](owner-b-handoff-r25.md) preserve all original fixtures
and failures. No runtime/header/compiler fix is made here; A21 investigates the
generic boundary in a separate coordinated round. Other emitted cells, product
ABI/D10/thread/lifecycle/upstream/rooted/delivery and P6 stay open.

## Owner B23 — native directory errno and synchronous SIGINT source controls pass

[Source-ready evidence](../artifacts/owner-b-r23-20261018/source-ready.json) binds
only PosixFsLib.cs, Libc.cs and ProcessSignalLib.cs repairs. Native and current
Libc JIT/AOT agree on13 directory cases per mode, including ENOTDIR20 for file
ancestors/trailing slash/.., ENOENT2, EACCES13, ELOOP40 and ENAMETOOLONG36.
Kernel directory handles close before owned snapshot publication and on errors.

The host Linux x64 sigaction ABI is independently measured: size152, handler0,
128-byte mask8, flags136, restorer144. It is separate from DotCC's public target
signal struct. Only64 mask bits are defined by the kernel; glibc query output's
reserved mask tail/padding is unspecified. The saved whole host struct is passed
unchanged on restoration; all defined fields and thread-mask bits roundtrip in
native/JIT/AOT blocked-SIGINT subprocesses, retaining pending delivery exactly once.
Unblocked C default terminates by actual SIGINT before return in all three modes.
Installed/ignore/error/reentrant comparisons also match native. Both managed modes
pass67 owner/thread/errno/in-flight/cleanup assertions and real OS exactly-once
callbacks;42 affected runtime unit tests pass without skips. Each actual Libc
PE/MVID/PDB and the actual AOT Libc reference/object/executable has its own binding.

The original AOT default timing failure remains RED historical evidence; the
repair does not use a delay, process-exit substitute or private System.Native API.
Exactly one host subscription remains under the disposition gate; callbacks run
outside it with existing RetainLease/Enter lifetime semantics. Explicit C default
is a process-wide terminal request; independent third-party OS registrations do
not share that gate and can race with save/restore. Unsupported host default ABI
reports ENOTSUP rather than guessing a layout. signal()'s pre-existing managed
handler backend remains SIGINT-only; unsupported registrations report SIG_ERR/EINVAL.

This does not qualify V13 emitted runtime or canonical193/286. Fresh normal tool
capture, unchanged-C emitted direct/object JIT/AOT, socket/resolver closure, actual
profile regeneration and all per-consumer D10/ABI/thread/lifecycle/upstream/P6
checks remain mandatory. Required skip/native/configuration adjudication is open.

## Owner B22 — fresh hostname ABI passes; required execution exposes two runtime defects

Both current A17 hostname forms pass fresh actual-source/PE/PDB/D10 review:
1,522 constant properties/942,677 exact RVA bytes/newarr0,1,962 caches/9,068
uses/454 pins and nine concrete lifetime exceptions. Both measured interpreter/
runtime layouts match all eight fresh native target-header observations.
Public ABI compares24 common fields, three deliberate wchar differences and
38 assertions over two actual lifecycles per form. Full warm allocation/GC and
four-cell interpreter qualification remain separate and unrun.

The actual owning upstream input receives its own AST/IL/RVA/cache/lifetime
review and21 rejecting controls, then passes real PID423287/hostname bootstrap.
All23 seeds discover the frozen5,134 IDs/5,142 occurrences. The failed raw-JIT
attempt executes1,999 unique required cases:1,995 pass, one error, one failure,
one required skip and one interruption;2,868 IDs remain unrun. Original journal,
tracebacks and exit134/outer1 are retained. Details: [API progress](api-progress.md).

The first error reaches upstream stub_socket selected by absent HAVE_SOCKET,
while the existing typed shared socket service is compiled. Real loopback
native/direct/object JIT/AOT controls each pass25 assertions and wrong-byte
expectations reject. This is an A-owned source-selection finding, with no policy
change or network/asyncio product capability claim.

The actual directory errno failure is independently RED in all four unchanged-C
emitted cells: opendir(file), opendir(file/child), opendir(file/) return ENOENT2;
native returns ENOTDIR20. Missing-child ENOENT and real directory iteration/close
controls pass. Existing PosixFsLib's Directory.Exists false branch is identified.
A native installed SIGINT handler runs before raise returns; all four emitted
cells instead return with delivery0/exit39, matching the actual generator's late
interrupt ordering problem. Exact native counterparts of all three failing or
interrupted required cases pass. These are frozen-runtime defects, awaiting
coordinated repair/capture; no shared source/header byte changes occur in B22.

Twelve reduction inputs bind actual PEs/MVIDs, ILC responses/references/roots,
native objects and executables;30 wrong-identity/empty-reference controls reject.
All six generated routes independently match76 current shared runtime sections.
Independent actual full-product semantic comparison covers26,508 method bodies
and1,618 RVA fields/2,332,915 bytes. The actual report remains failed.
Required UTF-8 skip/native marker adjudication, other upstream cells, full
ABI/runtime/thread/lifecycle/owning/rooted/delivery/audit/data/GC matrices,
new193 and P6 remain open. No source, required ID or decorator is waived.

## Owner B21 — fresh GETPID layout/ABI passes; hostname service already works

The new GETPID profile is independently bound to receipt035815-1ef722f0 and
its actual source/options/header/assets/75 registrations/runtime/tool closure.
Both canonical forms execute all eight native target measurements:
194816/10488/283080/88264/wchar2/callbacks10464/10472/10480. Both byte-identical
reference consumers pass24 public native fields, three explicit native-wchar4/
target-unsigned2 observations and38 real ABI operations over two lifecycles.
Fresh D10/source/PE/cache/lifetime review finds1,521 properties/942,622 RVA bytes/
newarr0,6,253 regions/1,962 caches/9,068 uses/454 pins/nine concrete exceptions.
All-property allocation/GC/static-identity and four-cell interpretation remain
separate unrun gates. Full details: [API progress](api-progress.md).

After two retained pre-execution harness-control failures, the exact041846
consumer passes fresh strict guards/eight negatives in the supplemental actual
resume and executes GETPID twice equal to real ProcessId398939. Actual PE
b5d96737e264e1109b5fc78c68aa7ac0ce1772cf73287d46577dc404dd3f64d5 /
MVID6e3fcc75-d40b-4f4c-ab7a-c79aed526e42 matches26,505 canonical method bodies
and1,617 RVA fields/2,332,829 bytes semantically. Discovery reaches22/23 seeds,
4,996 unique IDs/5,004 occurrences, then test_builtin/platform._node blocks on
absent socket.gethostname. Executed0/4,867 required IDs unrun/no case journal.
Original framework receipts remain failed and immutable; no pass is invented.

[Hostname controls](../artifacts/owner-b-r21-20261018/gethostname-controls/results.json)
pass51 native and51 in each direct/object JIT/AOT cell, with two genuine workers,
actual host identity, typed int(char*,size_t), truncation/NUL/sentinel/wide-capacity
and errno checks. Five wrong-host controls and ten PE/MVID/ILC-reference controls
reject. Both raw emissions match all76 current runtime sections. Actual service
signature0002080F050B and Dns.GetHostName call are present even in the actual
upstream input; its guarded CPython wrapper is absent. Same-pin native fallback
imports test_builtin with gethostname and reproduces the identical AttributeError
without it. The original wrong uppercase fallback expectation remains retained.
No shared ABI/implementation defect is reproduced and no runtime/header/policy
mutation is made. A15 owns independent selected-feature review after the genuine
B21 release/grant. Runtime164/compiler318/root tools/canonical193286 are preserved;
no old proof qualifies future policy/product bytes. Full matrix/P6 remain open.

## Owner B20 — executed new286 layout/ABI; getpid service passes, API absent

The new V13 test profile has fresh source/PE-bound D10 evidence, documented in
[API progress](api-progress.md) and `artifacts/owner-b-r20-20261018/`.
Both canonical PEs execute the eight actual target-layout observations:
interpreter194816/warnings10488/runtime283080/main88264/wchar2 and callback
offsets10464/10472/10480. All match a freshly compiled native oracle using this
publication's real target headers and compiler identity. Both JIT public ABI
consumers also pass24 common native fields, the explicit three wchar differences,
and38 operations over two actual initialization/finalization lifecycles each.

The early owning raw-JIT upstream input receives its own fresh D10/source/PE/
MVID/IL/RVA/cache/lifetime proof and three rejecting identity controls, then fails
on absent `os.getpid`. Eight seeds discover1,191 IDs;15 seeds block; assertions0;
all4,867 required IDs remain unrun. Original nonzero execution/report/tracebacks
are preserved. This is an A-owned HAVE_GETPID source/API selection gap, without
an observed V13 compiler or shared runtime defect. No feature is enabled by B.

The exact unchanged shared getpid service passes native and four direct/object
JIT/AOT controls:54 checks/two genuine workers in each. Typed int(void), real
positive host PID, stability, worker context/return and independent errno state
pass; dynamically wrong expected PIDs reject in all five executions. Actual
getpid IL calls System.Environment.get_ProcessId; each JIT/ILC PE/MVID, response,
all references, roots, native object and executable is bound, with ten rejecting
identity/reference controls. Both raw routes match all76 current runtime sections.
No runtime164/header/compiler/policy mutation or process support is introduced.

The actual owning availability diagnostic binds the failed upstream input bytes,
confirms missing getpid/getppid/getgroups and executes new formatter/status/wait
contracts correctly (epoch string, exitcode42, EPERM; child execution0). Native
ECHILD versus target EPERM remains deliberate. B20 releases after this bounded
group; whole-profile AOT/ABI/thread/lifecycle/rooted/delivery/allocation/GC and
remaining upstream qualification, new193 and P6 stay open. No old proof transfers.

## Owner B19 — same-C LP64/calendar and wait-status after-proof passes

[B19 exact-input results](../artifacts/owner-b-r19-20261018/v13-after-results.json)
prove the unchanged B18 C controls with exact V13: both native controls plus16
raw/processed × direct/object × JIT/AOT cells pass. Original source hashes and
assertions are retained. Strftime yields19 for ordinary and2147483713-wide
capacities,0 for tight/empty cases, with correct bytes/NUL/sentinels. Direct
runtime calls cast literal capacities to their C ulong formal while the
legitimate managed int overload remains. Wait stopped0x137f/0x7f and continued
0xffff are not signaled; exit/signal/status decoders and WNOHANG pass. Native
wait/waitpid reject ECHILD; target rejects EPERM, preserving status305419896.

The233 direct-runtime assertions pass in each JIT/AOT mode, including actual
typed function-address invocation, capacities through ulong.MaxValue, legacy
negative/ordinary capacities, insufficient-buffer nonwrites, null/error cases,
errno and sentinel preservation. All27 actual-source focused units pass/zero
skips. Full recursive tool/source copies and all151 actual CLR resources are
independently verified; each raw output matches all76 runtime sections. Every
actual consumer PE/MVID, ILC rsp/reference/root/native object/executable is
bound, with independent input copies and40 rejecting identity/reference controls.
Actual shared-runtime PDB/source checksums cover all83 runtime source files.

All164 frozen runtime/header files, exact V13 tools, root tools, canonical193/286
and unrelated changes remain unchanged. B's source-only harness and documentation
are the authored changes. Original before failures/CS0266 and first upstream
bootstrap report remain retained. The genuine
[RELEASE-ALL](../artifacts/owner-b-r19-20261018/serial-slot-release-all.json)
returns the lane for separately granted A11 policy/native controls. No feature
enable, product regeneration, interpreter/whole-profile ABI/D10 or P6 result is
inferred. All4867 required upstream IDs remain unexecuted; fresh193/286 and
their complete actual-input qualification remain required.

## Owner B18 — actual286 bootstrap gate; LP64/calendar and wait-status repair

Full scope and retained failures: [API progress](api-progress.md), evidence
`artifacts/owner-b-r18-20261018`. Fresh193 strict preflight passes before mutation;
fresh286 independently binds source/receipt/actual PE/MVID/1511 RVA getters and
6239 regions/1960 caches/9052 uses/454 pins/9 reviewed lifetimes in each form.
Actual286 L16 edges37, rather than193's36. The first raw-JIT owning upstream
input is strongly reviewed before execution, then receipt023127-d2d83fe8 fails
all23 seed imports on absent time.strftime/os.waitpid. Zero assertions execute;
all4867 required IDs stay unexecuted, with complete original evidence retained.

Actual native-pass/emitted-fail reductions establish the authorized three-file
repair. `strftime` now has size_t/ulong capacity/result and an exact typed
function address; legacy managed int calls remain valid. Capacity2147483713
previously narrowed and returned0 for a19-byte result. The wait header restores
signed-char WIFSIGNALED semantics: stopped0x137f/0x7f and continued0xffff are not
termination signals. No wait/process algorithm is replaced: wait/waitpid still
return-1/EPERM without touching status. Native no-child ECHILD is separate.

Current shared Libc direct JIT/AOT each pass233 boundary/function-pointer/
wide-capacity/NUL/sentinel/error/errno/rejection assertions. Original-source
focused runtime units pass27/zero skips without compiler build; the stale-root
compiler dependency failure of the ordinary project is preserved separately.
Real GCC old-header failure24/current-header pass0 and same-pin native bootstrap
controls pass. Only CalendarLib.cs/time.h/sys/wait.h change runtime164; other161
inputs stay frozen. SOURCE-READY precedes A10's fresh normal immutable capture;
B independently verifies the V12 dependency/source handoff and all151 actual
runtime/header resources. The identical wait C fixture now passes native and
direct/object JIT/AOT (four cells), preserving EPERM rejection and status bytes.
The identical strftime C fixture passes native19/wide19, but its emitted JIT
build fails CS0266: literal capacities64/4/1 bind the preserved managed int
overload instead of the C size_t/ulong formal. No strftime emitted after run is
claimed; the other three cells are unrun behind this compiler blocker. A typed
external-override wrapper diagnostic passes and narrows the defect to direct
runtime calls. Original build/driver failures and the diagnostic expectation
correction remain. Frozen sources were not altered. Full regenerated193/286/
ABI/upstream and P6 remain open; no inherited V11-after or interpreter pass.

## Owner B17 — complete V9 actual-input binding; next lane returned to A9

Evidence/work: `owner-b-r17-20261018`; full commands and scope are in
[API progress](api-progress.md). All20 distinct actual consumer input PEs now pass
the unchanged strict compiled/address/lifetime readers, including the12 pending
ABI/behavior/rooted/delivery-contract reviews. Each binds actual SHA/MVID,
IL/RVA/source/cache/pin and all9 concrete remaining lifetime exceptions; each
independent semantic-byte comparison passes. All58 required negative outcomes
remain:18 different-PE canonical-only proofs,20 wrong MVIDs and20 empty lifetime
reviews reject. No interpreter group was rebuilt or native backend substituted.
Canonical source-bound allocation/GC probes retain their separate data-only scope.

Only the private prepared wrapper's positional-project selector needed repair;
its original failure and20-command reduction are preserved. Runtime164,
compiler318, the actual canonical V9 receipt/source/PEs, existing strict readers
and A8's separately published286 remain unchanged. No runtime/header/API/Host
change, compiler build, regeneration, staging or commit occurred.

B's new profile-aware audit helper and upstream hook require explicit new-profile
source/receipt/lifetime evidence, inspect the actual response-selected AOT input
and retain wrong-MVID/empty-review controls before upstream assertions. Reporting
now checkpoints completed groups and remaining required IDs.92 audit and23
reporting/harness controls pass/zero skips. Actual B17 new286 readiness/D10/ABI/
upstream qualification is **unrun**, deferred at the explicit A9 slot request;
A8's own helper/fork passes confer no broader B qualification. No actual compiler
blocker was observed; unexecuted new-profile gates remain unknown.

The group completed normally and all new build queues paused. B independently
verified all34,712 copied files/8,285,479,634 bytes in A9's complete V9 backup,
with separate inodes and exact bindings for canonical product and all20 inputs.
B's newly produced proof bytes are copied too. `v9-explicit-input-handoff.json`
and `serial-slot-release-all.json` bind source/PE/receipt/evidence backups, frozen
inputs, process state and independent three-lock release. No active proof was
retargeted; future historical review must bind actual copied bytes. A9 needs
coordinator verification plus explicit grant; B queues remain paused until return.
Fresh new193/new286 matrices, frozen upstream/native-outcome adjudication,
production/provider/AOT/downstream/reproduction and all P0–P6 remain open.

## Owner B16 — actual delivery input audit; remaining reviews yielded to A8

Evidence/work: `owner-b-r16-20261018`; exact assignment and commands are in
[API progress](api-progress.md). A7 release and all three locks were verified.
All164 frozen runtime inputs and V9 canonical receipt/source/PE/MVIDs remain
unchanged; no runtime/header/Host/API/compiler mutation or product regeneration.

Actual receipt `20261018-011626-7c6fa428` passes raw/processed JIT/NativeAOT
relocated useful delivery (two cycles and normal/three missing-asset scenarios)
and all four ordinary-delivery audits. The hook now audits each actual ILC input
rather than inherit JIT PE evidence for AOT. Response/entry/native object/output
bindings, actual metadata/IL/RVA and fresh unchanged semantic analysis bind all
four useful delivery PEs. Each has4,632 regions/1,465 caches/8,024 uses/383 pins/
9 concrete lifetime exceptions and1,233 assembly-data getters/897,429 exact bytes/
1,338 calls/newarr0. Source-bound warmed allocation/GC controls remain explicit.
Strict auditor guards and all existing controls are unchanged;85 controls pass,
zero skips. Original failures and canonical-only rejections remain retained.

Twenty actual consumer inputs are pinned:8 held exact-byte strong proofs,
**12 pending strong reviews**. The retrospective review command is prepared but
**never launched**: coordinator requested the next A8 slot, so the current
command finished normally and all new queues stopped. Four delivery/audit passes
do not close the full matrix, upstream or P0–P6. B15's overwritten historical raw
ordinary-AOT response is not invented; freshly bound isolated delivery AOT
artifacts provide replacement useful-consumer executions.

A8's concurrent preparation-policy edits are retained separately. Actual audit
readers use unchanged manifests and retained V9 C/config/source/PE evidence;
they do not invoke the changed preparation/profile entrypoints. Boundary hashes,
precise dependency scope and diffs are durable. HAVE_FORK/layout/thread/callback
changes inherit no V9 proof. Durable `serial-slot-release-all.json` supplies
commands/results/pending work and genuine process/three-lock observations for
coordinator/A8 handoff. Fresh B round after explicit slot return; final tools,
full286/upstream/native adjudication/downstream/reproduction remain open.

## Owner B15 — actual interpreter AOT/ABI and PE binding; slot returned

Evidence `artifacts/owner-b-r15-20261018`; assignment
`cpython-impl-v9-aot-qualification-b15-20261018`. All164 frozen runtime inputs,
V9 canonical source/PE/MVIDs and receipt233444 remain unchanged. No service,
header, Host, API or compiler edit; no regeneration/commit/nested agent.

Actual004135-66a38985 passes raw/processed NativeAOT ABI38 assertions/two cycles,
24 common layouts and the explicit native wchar4/target unsigned2 difference.
Both forms also pass behavior/thread/lifecycle5/2/2 groups per cycle/two cycles
with real ownership/cancellation/trace/error/reuse/drain checks. Both whole-library
and facade-rooted AOT runs pass all9 groups/two cycles. Normally trimmed useful
AOT consumers004820-cdd32be2 pass both forms/two cycles under outer shared flock.

Isolated outputs change actual PE/MVIDs; the unchanged gate rejects canonical-only
proof. Full semantic-byte comparison of the actual ABI ILC inputs matches all
method bodies/EH/locals and1,281 RVA fields/1,637,980 bytes per form. Fresh actual
IL/RVA and unchanged analyzer results bind4,632 regions,1,465 caches/8,024 uses,
383 pins and9 source-span lifetime exceptions. Wrong-MVID and empty-exception
controls reject.76 existing audit controls pass; no strict guard was softened.
B's narrow test-hook repair records the original rejection and freshly analyzes
the actual PE, retaining the lifetime explanations only with the same IL edges.

The current proof group finished normally before A7's requested compiler slot.
Durable `serial-slot-release-all.json` records no B command active/queued and
independent shared/run/native lock observations. Delivery/complete four-cell audit
and remaining actual consumer-input reviews are unrun/pending explicit slot return.
Full286/upstream, native marker/encoding policy, final-tool193/downstream and P6
remain open. Details and exact receipts/commands are in the API ledger/artifacts.

## Owner B round 14 — kernel address reuse demonstrated; runtime unchanged

Assignment `cpython-impl-mapping-qualification-b14-20261018`; actual T3 task ID
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-mapping-qualification-b14-20261018`.
Local date 2026-10-18; requested model/configuration confirmed. Private artifacts
and work: `owner-b-r14-20261018`. All164 frozen R13 inputs match initially.
The unchanged original test fails1/1, with full TRX/syscalls preserved. strace
proves owner disposal munmap=0 followed by CLR `/memfd:doublemapper` allocation
at the exact address before mincore0. The assertion measured address permanence,
not original storage lifetime. `mapping-diagnosis.json` binds the original
failure and four decisive syscall lines. No runtime/header/API mutation needed;
all164 frozen R13 inputs remain unchanged.

The corrected test observes a unique live file's kernel mapping identity before/
after disposal while retaining anonymous partial/foreign-owner controls. One new
ordinary/direct lifecycle control proves retained-storage detection, close/file
lifetime, partial/overlapping holes, active recursive bindings, foreign/ambient
owners, idempotency and actual shared bytes. Native19 independent kernel checks
and actual shared-libc direct JIT/NativeAOT pass; affected ordinary units41/41,
zero skipped. No private registry/counter oracle or permanent-address assumption.

Independent strong unchanged D10 preflight passes both actual canonical V9 PEs:
expected receipt/index/source/PE/MVID binding,4,632 regions/1,465 tracked caches/
8,024 uses/383 pins/9 concrete lifetime exceptions, no unresolved group. Fresh
metadata confirms L0/L1636/L320/PinAndRoot3 edges; payload IL/RVA/warmed JIT/AOT
and semantic evidence qualify their own data-probe scope. Short fresh JIT
ABI/useful-consumer/runtime lane is active; A6 preserves V9 canonical products.
A fresh review of actual new useful-consumer PE copies also passes both forms;
all9 lifetime exceptions and exact helper edges are bound in
`independent-consumer-pe-review.json`. Fresh useful JIT consumers pass both
forms/two cycles in002105-dfd6562f. ABI attempts preserve002224's private-wrapper
environment failure and002307's authored recursive-source-glob failure. B fixed
only Abi.csproj to explicitly compile its actual Program.cs; all separate probe
projects and product sources remain. Fixed-project ABI002515-6c265612 passes
both JIT forms:38 assertions/two cycles each,24 common actual native layouts and
explicit native wchar4/target unsigned2. Fresh runtime003012-c176f7bd passes raw/
processed JIT behavior/thread/lifecycle:5/2/2 groups per cycle/two cycles plus
owning value/error/callback/trace/cancellation/reuse/worker-drain assertions.

Coordinator's serial-slot steering defers all additional AOT/rooted/delivery/
audit commands until A6 complete286 finishes and explicitly returns the slot.
B's short JIT wrapper completed normally. **Durable RELEASE-ALL** is in
`serial-slot-release-all.json`, with actual receipt/source/PE pins, runtime164
freeze, process/lock observations, retained failures and no owned/queued command.
No expensive matrix has been started. A6 may take the complete286 slot while V9
canonical products stay frozen. Qualification resumes only after explicit A6
completion/slot return. A6 retains compiler/profile/publication ownership.
Remaining four-cell/whole-rooted/trimmed-consumer-AOT/delivery/audit/upstream/
final-tool/downstream/reproduction and all P0–P6 acceptance remain open. No commit.

R12 guide/helper readiness and R13 shared secure-erasure closure are completed.
Final service READY/RELEASE-ALL is recorded; product/D10/P0–P6 remain open.
No commit made.

## Owner B round 13 — explicit_bzero ABI/service READY; RELEASE-ALL

Assignment OWNER B ROUND13; runtime session
`01a14b77-4ff7-7241-aaae-0108af8b1d51` (no supplied T3 delegate task ID).
Full source/repro/results/handoff: [api-progress.md](api-progress.md), artifacts
`owner-b-r13-20261017/final-ready-release.json` and `r13-summary.json`.
A exclusively owns compiler/config/scripts/NativeOracle/whole-profile products.
B built no compiler and changed no profile macro. All B commands completed,
none queued: **final READY + explicit RELEASE-ALL** for A's whole286/final193.
A may select actual upstream `HAVE_EXPLICIT_BZERO=1`; no upstream source changes.

- `DotCC.Libc/SecureZeroMemoryLib.cs` SHA256
  `0ba69773d63fcadfd77493bd6df1e732b77cc662b3e6a6870698042b2485b9fb`.
- `DotCC.Lib/include/string.h` SHA256
  `498320a477829c6eb90c446953a88767fdfb3371bf4274788f30150d0eeb27bb`.

Ordinary header ABI is **void explicit_bzero(void *, size_t)**. Actual direct
and emitted Release metadata signature0002010F010B confirms void result,
writable pointer and LP64 unsigned64 count. No P/Invoke/alternate native service
implements it: the shared BCL ZeroMemory contract retains stores for buffers
never read again. Checked complete native extents precede all stores; null with
zero length is accepted, errno unchanged, writable ownership/pin required for
whole call, int.MaxValue chunks cover wide lengths without narrowing or
retaining a managed pointer. No out-of-range or C object-lifetime extension is
claimed. No D10 writable destination allocation/identity is removed.

Native before branch30 passes; V6 missing shared declaration fails2. Final V8
normal embedded resources pass native/JIT/AOT30 ordinary and35 wide assertions
each. The wide extent is2,147,483,712 bytes, crosses the real span boundary,
fully scans zero bytes and retains both guard bytes. Exact static/heap/stack/
unaligned/zero-length/control-flow/errno cases pass. Direct shared JIT40/AOT40
include a fixed managed owner across forced compacting GC, two workers, checked
invalid ranges, actual void function pointer and10,000 warmed allocation-free
erasures. New/affected units63/63 and ordinary emitted fixture1/1 pass,zero skips.
These are shared-service checks, not interpreter/ABI/D3 qualification.

Unchanged same-pin BLAKE2b/s reference C sources pass native/JIT/AOT50 each,
two variants/eight vectors plus streaming/copy/reset/errors/state-erasure.
All21 actual BLAKE2 inputs match the pinned archive. Separate ordinary object
emission/link retains both algorithms. Exact failed280-blake2module source
replays successfully with the single additional HAVE_EXPLICIT_BZERO flag and
new tool/output/report paths. Full286 is A-owned and remains unqualified.

`non-elision-proof.json` binds source/actual PEs/MVIDs/call graph and optimized
JIT/AOT dead-key/key-burn disassembly. Conditional dead stack-buffer erasure
calls survive both code generators; BCL ZeroMemory reaches real zero stores.
Both actual keyed algorithm init paths retain erasure after last block use.
Native unfortified optimized control calls actual glibc explicit_bzero;
original optimized oracle uses its checked form. The
[documented BCL contract](https://learn.microsoft.com/dotnet/api/system.security.cryptography.cryptographicoperations.zeromemory)
and [10.0.12 implementation](https://github.com/dotnet/runtime/blob/v10.0.12/src/libraries/System.Security.Cryptography/src/System/Security/Cryptography/CryptographicOperations.cs)
support non-elision; no new constant-time/broader security claim. AOT requires
only libc/libm/loader. Initial V7 unqualified BCL-name embedding failure is
preserved and fixed in shared source via full qualification/fresh V8 resources;
no generated C# repair. Other initial harness failures remain in the API ledger.

A's immutable V8 handoff SHAa7de7f560341b4ad9254aadbb5264d86b16491b85f0730fb1cce6dada310f363
binds all314 preserved/current source copies with no drift.162 original R10
inputs remain exact; string.h is the sole original exception and the new
runtime file makes164 inputs. Service edits held flock --close against A's
snapshot; all builds/emission/execution used per-command shared/native locking.
No idle-held/bypassed/deleted/nested lock or native engine/product backend.
All25 unrelated dirty inputs/six frozen upstream JSON/PLAN/usage/HEAD remain
unchanged. No commit, staging, push, cleanup, worktree or nested agent.

Final tools remain provisional. Full193/286 regeneration, final D10 static/
scalar address/cache/semantic exceptions plus exact payload IL/RVA/allocation/
identity, fresh all-four-cell ABI/runtime/trimmed/rooted/delivery/audit and
frozen upstream assertions remain open. Explicit final product READY/run.lock
release precedes short JIT ABI/useful consumer and expensive cells. Native
required marker/UTF8/default-encoding adjudication, stable-tool production/
provider/AOT/affected regressions and reproduction/P0–P6 remain required.

## Owner B round 12 — guide and source/argv/helper readiness

Assignment OWNER B ROUND12; runtime session/thread
`01a14b6c-dfdd-7100-8b51-6a578f38328c` (no supplied T3 delegate task ID).
Exact commands, scope and handoff are in [api-progress.md](api-progress.md),
with evidence under `artifacts/owner-b-r12-20261017`.
[usage.md](usage.md) now supplies the required P5 owning guide: actual sample/
facade calls, ordinary raw/processed references, isolated packaged startup,
exact JSON, lifetime/GIL/worker/disposal/error/text/cancellation contracts,
relocation, trimmed/rooted AOT and target limits.13 links and9 copied sample
statements are checked; no novel C# example or product execution was introduced.

B corrected only stale native-helper snapshot metadata that described universal
Py_BUILD_CORE flags. New read-only `tests/Audit/helper_profile.py` checks actual
manifest/source hashes, ordered argv versus recorded flags, source/header API
guards, all static registrations and compiled initializer membership/PE identity.
It preserves _testcapi's individual source opt-ins and does not assume every
helper is core/public/limited. This readiness tool does not execute an interpreter
or replace A's real preprocessing/compilation and existing full qualification.
A owns recipe integration and profile/compiler flags.

Fresh independent audit controls pass76/zero skips; upstream reporting controls
pass21/zero skips. Actual failed286 receipt224316 review intentionally exits1:
75 registrations;62 role units (29 public/5 source opt-ins/23 limited/5 internal);
one failed/two cancelled/59 unrun commands; compiled profile absent. Its two
inherited-core public-guard observations are retained, not normalized. New
negative controls reject dropped source/guards, order/redefinition errors,
command-forced opt-ins, erased internal role, missing compiled initializer,
wrong-product PE and changed PE bytes. These are audit-envelope tests, not
emitted ABI/layout, upstream assertion or interpreter results.

Current completed first profile failure remains224316's public _testcapi guard.
A's provisional C031 V6 handoff binds all163 R10 inputs/no drift and explicitly
says toolsFinalStable:false; strict286 retry225558-3de551e6 is A-owned. No new
final193 READY/release or published286 pointer was received. B runs no shared/
dotnet/emission/AOT/consumer command or competing build. OldV3 static data proof
and two unresolved address groups remain distinct from fresh final qualification.

Next actual lane: explicit A READY/run.lock release, pin final receipt/all owned
source files/PEs before and after; reject incomplete address/site/exception proof
before short JIT ABI/useful consumer and the fresh four-cell/runtime/trimmed/
rooted/delivery/D10 allocation+identity matrix. Published286 is required for all
frozen in-interpreter assertions/duplicate occurrence counts. Native required
skip/xfail/UTF8 policy adjudication, stable-tool production/provider/AOT/affected
regressions and reproduction remain open. No runtime/header/Host/API/frozen
inventory mutation, staging, cleanup, commit, push or nested agent.

Preservation: `artifacts/owner-b-r12-20261017/preservation-after.json` binds
all163 R10 inputs,168 protected runtime/header/Host/API/sample inputs, all25
unrelated dirty campaign inputs, six frozen upstream JSON files, PLAN and HEAD
unchanged. `docs/plans/summary.md` is also unchanged. A's concurrent
`DotCC/Program.cs` edit is preserved. Product pointers remain unchanged, with
no published test-profile pointer. Final index: `r12-summary.json`. No B shared
command is queued; the independent R12 deliverables are complete.

## Owner B round 11 — source/PE/cache audit binding completed; final artifact pending

Task `cpython-impl-c030-profile-qualification-r11-20261017`; exact task ID and
current commands/evidence are in [api-progress.md](api-progress.md).
Artifacts/work: `owner-b-r11-20261017`. No runtime/header/Host/API mutation;
initial312-path snapshot verifies all163 R10 inputs unchanged.

B reproduced and repaired a qualification-envelope defect: source hashes plus
a qualified label/empty unresolved list previously passed without semantic
analysis, and a matching run ID did not require the same receipt bytes. The
gate now binds the actual analyzer/report/source inventory/Release PE/MVID,
validates exact UTF-16 source ranges, recomputes static groups and independently
enumerates conversions/cache definitions/uses/pins. Nonempty tracked-ref helper
and actual PE ref-byte field061005/Get IL must agree with the independent
MetadataAudit reader; remaining conversions need concrete source-bound semantic
exceptions. Rejection controls provide audit evidence only, no ABI/layout or
interpreter pass. Actual final owner/type/alias/projection/dominance/lifetime
review and native/direct/object/GC controls remain required.

Auditor59 methods/zero skips and40 malformed analysis/site subcases pass.
Actual C030's zero legacy byref declarations/1463 lazy caches exposed a B data
inventory incompatibility; independently counting real cache declarations now
accepts the new representation while false declared counts fail. The final
semantic/address gate remains mandatory. MetadataAudit builds in isolated
outputs under nonblocking shared flock, zero errors/two existing VectorIo
warnings. Both preserved V3 PE inspections independently confirm field061005
and the same42-byte Get IL hash8a73d2c2a3dc0afd76027e71ce519b8b76667256e637532bcf4dd9880183c28b.

Actual read-only V3 source/PE/RVA data checks pass both forms:1233 properties,
897429 exact bytes,1338 getter calls andzero allocating payload sites.
`v3-static-data-review.json` binds these preserved copies and actual PEs;
`actual-negative-report-consumption.json` rejects both actual old1567-group and
V3 two-group failed semantic reports despite deliberately contradictory wrappers.
No interpreter, warmed-allocation, writable-destination identity or final ABI
gate is inferred. Preserved v5 failures concern only newly checked marker-fixture
inventory counts; corrected support records pass without changing runtime tests.

Actual provisional V3 `20261017-222803-2283f397` reports4635 sites/4632 regions/
1463 cache bindings,9 remaining conversions andzero unknown/errors; it still
fails two groups (PyOS_Readline pointer-slot and memoryobject const-bool). Its
raw PE SHA81b823a4244d2ef1ebf54d37f6f39b54d6e8dbef10467e292676560b7d172a62,
MVID6cda07d2-3010-4662-afc0-2219f2e94e2e, ref-byte field061005 and42-byte Get
IL are source-bound observations, not a final ABI/runtime certification.
A has supplied no final READY/release; B queues no product execution. The old
210915 D10 address failure remains, and its prior four-cell passes are distinct.
Full286/frozen upstream, native marker/default-encoding adjudication, final-tool
production/provider/AOT/affected regressions, reproduction and P0–P6 stay open.

V4 immutable tools bind313 inputs/no drift/all163 frozen R10 hashes exact;
handoff SHA4384ac807b1ab00b35da98682931ca3148b92be05e330eef412c0e3664f53338.
A's focused34/native/object/static/context JIT/AOT controls do not qualify the
interpreter. A owns full286 followed by final193 after profile findings. B's next
actual execution awaits explicit final READY and run.lock/shared-lock release,
then final source/PE/address proof before short JIT ABI/useful consumer and the
complete fresh four-cell/trimmed/rooted/delivery/audit matrix. No shared operation
or old-product consumer is queued; no final-tool downstream regeneration began.

Full286 V4 attempt224316-589a5659 passes fcntlmodule then fails actual public
_testcapimodule at inherited `Py_BUILD_CORE` (#error,exit2); two sibling objects
are cancelled130 and outputs{}. A owns flag selection/retry, with286 units
retained. There is no new B runtime defect or published helper profile.
R11 index: `artifacts/owner-b-r11-20261017/r11-summary.json`. Final preservation
verifies163 R10 inputs,228 frozen runtime/header/Host/API/non-Audit-test inputs,
25 unrelated dirty inputs and HEAD unchanged. Independent B audit work is ready
for handoff while final product/profile qualification awaits A's release.

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

## Owner B round 9 — real Linux x64 record locks READY; native/JIT/AOT pass

Task `cpython-impl-fcntl-profile-r9-20261017`, exact task ID and commands in
[api-progress.md](api-progress.md). Evidence: `artifacts/owner-b-r9-20261017`.
A retains exclusive C030/compiler/config/full193/full286/publication ownership.

Actual native-header geometry: `struct flock` size32/alignment8; l_type/l_whence
signed16 at0/2; l_start/l_len signed64 off_t at8/16; l_pid signed32 at24.
GETLK/SETLK/SETLKW=5/6/7; RDLCK/WRLCK/UNLCK=0/1/2. Shared header uses actual
sys/types typedefs. Initial same-C native62 +independent peer29 assertions pass;
old tools fail incomplete-flock emission exit2. Expanded native77 +peer41
checks/21 phases cover GETLK output/PID, read/write/conflict/downgrade, split/
partial unlock, zero-to-future-EOF, signed lengths and offsets beyond5GiB,
SEEK_SET/CUR/END without moving file position, close-any-descriptor process
release, descriptor reuse and true blocking SETLKW. Whole-file flock separately
tests sharing, conflicts, conversion, independent record locks, blocking and
open-description close semantics. A's source-complete V2 provisional tools now
pass the identical C control in **emitted JIT and NativeAOT**,77 C assertions +41
native-peer checks each/21 phases. Actual emitted-header geometry matches native;
no handwritten managed struct establishes the ABI.

Shared service passes the original C pointer to Linux fcntl, allowing kernel
EFAULT instead of dereferencing invalid pointers in managed code. It borrows/
retains the original FileStream SafeFileHandle, synchronizes logical position
for SEEK_CUR, and does not dup/close or hold descriptor-table locks while
blocked. Close racing a retained call completes OS closure after return.
No signal/success/lock stub; OS errno/deadlock/blocking behavior remains actual.
Process-associated locks intentionally share across runtime contexts/threads.
Non-Linux-x64 hosts or backings without a host file descriptor return ENOTSUP.

Existing flock's success-noop is removed. Direct/first emitted controls exposed
FileStream path-open's implicit shared flock; POSIX open on Linux x64 now adopts
the actual native-open owning handle (existing O_NOFOLLOW mechanism) for all
its flags, without hidden advisory locks. The unmodified stdio/mkstemp path-based
backings retain an explicitly recorded implicit-sharing residual; no qualification
of their equivalence to native lock-free open is claimed.

Actual shared Libc direct JIT78 / NativeAOT78 and affected unit72/72 pass,
including five new xunit methods, zero skipped; current85-path source snapshot
unchanged. Native OFD observation checks
classic process locks, real flock, context ownership, close/dispose, invalid
integer/pointer buffers, large addresses, position/bytes and real blocked-call
handle retention/concurrent close/table availability. First A provisional
emitted JIT passes14 record phases then fails whole-file flock errno11 with its
older embedded open. Failure/source/tool evidence remains in `provisional-v1`;
no AOT pass or final compiler/product qualification is inferred for that snapshot.
The repaired V2 snapshot above passes every emit/build/JIT/AOT stage; both
blocking APIs stay blocked at least300ms until independent native-owner release.
Ordinary emitted fixture17 assertions passes one discovered test/zero skips.
Actual same-pin unmodified fcntlmodule-only diagnostic also emits0 with original
source/config/flags, under the shared lock; no full286/profile qualification or
publication is inferred. No B compiler build.

Candidate exact source hashes: `candidate-services-hashes.json`; xunit TRX/
commands and preserved harness failures are in the API ledger. All five candidate
hashes match A V2; all25 unrelated tracked inputs and HEAD are unchanged.158 prior
B paths remain unchanged; only four intended existing files and new RecordLockLib
change the prior162 closure. Direct/emitted AOT ELF imports are libc/libm/loader.
Unchanged D10 auditor50 mechanism controls pass while final address qualification
stays required FAILED. Six final-tool downstream command dry runs pass; actual
regeneration waits A's stable-tool/provider handoff. All shared
operations use per-command flock --close, no nested/idle-held lock. Frozen IDs/
decorators/exclusions, old193 receipts, final D10 hard-failure and all unowned
inputs remain preserved. No commit/push/staging/reset/cleanup/worktree/nested
agent. Final interpreter/full286/upstream/all4 cells/rooted/delivery/audit/
reproduction and final-tool downstream production/affected regressions remain
open; these focused services do not close P1/D3 or P0–P6.

## Owner B round 8 — actual matrix passes; final D10 address gate fails

Task `cpython-impl-d10-delivery-matrix-r8-20261017`; evidence under
`artifacts/owner-b-r8-20261017`. A remains sole compiler/config/generator/product
owner. B changed only tests and these two progress ledgers; no compiler/Libc/
header/Host/API/NativeOracle implementation change, commit or nested agent.

**Required final D10 address qualification FAILS both forms**, exit1 in
`d10-required-address-qualification-command.json`. A's concrete review leaves
conservative call/label limits and additional safe static-storage reuse open;
2736 residual base mentions are not all proven necessary. B's audit now requires
a qualified receipt/source-bound safe-site review, with no generic macro/call
exemption. Exact failure/next action: `d10-final-address-qualification.json`.
A owns any further lowering repair/full193 regeneration/fresh qualification.

Explicit product-ready handoff selected strict all193 receipt210915-401ea00d,
SHA0bf316d6b7076c40cf4471f310bb918f7c4e95a73998ca84e08c0896d36bd1b4.
Shared-lock initial/final snapshots match all262 owned files/form. Current
receipt-specific results (all tools provisional; no P1/D3/campaign closure):

- **Actual ABI all4 cells pass**: JIT212126-eac09a77; AOT213332-6e74a923.
  Every cell executes38 assertions/two cycles,24 common native layouts, real
  refcounts/immortals/slots/varargs/vectorcall/callback signatures. Native wchar4
  versus target unsigned wchar2 remains explicit, with no false equality.
- All behavior/thread/lifecycle cells pass: JIT212308-83222824, AOT213332.
  Each has5/2/2 Python groups per cycle, two cycles, plus owning reference/error/
  trace/cancellation/reuse/callback/worker-drain assertions. The JIT receipt's
  later filename-classifier delivery failure remains failed as a whole.
- Ordinary useful consumers pass four cells: A JIT211651-b11d262c and B
  AOT213151-2f0c807e. AOT is normally trimmed, serialized by outer shared flock
  from unrelated /tmp; two cycles/six callbacks/exact JSON/text/errors each.
- Whole TranslatedCPython **and facade** rooted AOT passes both forms in213332,
  all9 Python groups/two cycles/form plus actual owning gates.
- Corrected JIT delivery212800-1f2b9d4a and AOT delivery213332 pass all4 cells:
  188 assets/notices, relocated paths with spaces, poisoned environment/empty
  PATH, real module/builtin origins/no engine/extensions and normal/three fresh
  missing-asset error processes. Only the B filename classifier/absolute flock
  resolution changed; no interpreter assertion or generated C# was repaired.

Data/dependency audit214231-78a0d9bb passes both forms/modes:193 source/object/
link/build closure,70 generated sources each,46 builtin registrations/188 assets,
actual facade/sample/product references and31 shared-runtime-matched native
imports in each product. Actual AOT ELF needs only libm/libc/the ELF loader;
resolved runtime-pack/license notices pass. This audit invocation preceded B's
final-address-gate addition and establishes that narrower scope. The separate
required final D10 gate above fails; full audit/P2/P6 acceptance remains open.

Actual compiled D10 proof:1233 named payload getters/897429 exact RVA bytes /
1338 call sites per form, zero temporary newarr/maxgetterIL16. Raw MVID
 dc77a44f-52ed-41ea-b26a-05807e8994e6, processed
 d5aa3e65-eaa7-4125-a980-f66a8042d3e1. Receipt/csproj/ownership/source/PE byte
identities are tied together; byte-identical ordinary-reference PE copies are
accepted. Owned mutable setvbuf/pipe byte buffers remain and are explained.
A's91f0ec6c handoff binds all four actual-product JIT/AOT allocation probes:
all1233 properties/10000 warmed copies each/zero allocation/exact hashes,322
retained compiler/runtime sources, semantic30/507/4 reports and exact native
stdout. Counts overlap rather than sum. Known compiler baseline/initial AOT
reflection-root probe failures remain preserved. No NativeOracle edit by B.
Warmed payload measurements exclude required writable destinations and owned
pinned literal-pool startup allocations; their identity/lifetime remains.

Support validation:50 auditor rejection controls,3 failed-upstream-report
retention controls and two real allocating/cached-array PE rejection controls.
These prove harness behavior only. Required upstream nonzero exits now retain
original counts/duplicate occurrences/full report hash/first nonpass in campaign
receipts; no skip/xfail/pass normalization, frozen-ID or decorator change.

R8 summary/proof/first-failure/preservation records remain under its artifact
root. All162 R7 validated runtime/Host/API/header/QuickJS inputs and25 unrelated
tracked dirty inputs are unchanged; original HEAD remains bb2f9b1f13. All shared
operations used flock --close; no outer lock enclosed specialist hooks. Explicit
RELEASE-ALL followed the final address failure and unchanged-product pin;
B has no shared operation queued.

A's subsequent isolated full286 receipt `20261017-214714-ed846112` **FAILED**
actual `Modules/fcntlmodule.c`, exit2: local `l` has incomplete aggregate `flock`.
The original log/command/source/tool hashes are retained in
`full286-fcntl-blocker.json`; no unit/header/config drop or new publication was
made. Shared `fcntl.h`/real-lock repair needs a coordinator-assigned runtime lane;
B remains within this round's Audit/Delivery scope. A's C030 compiler work and
any changed tools require fresh193/product/matrix qualification.210915 evidence
remains receipt-specific and does not qualify the failed286 profile.

Still required: complete final-storage CSE review/repair and stable tools with
new193 qualification if changed; full286 actual upstream4cells and unresolved
native required skip/xfail/UTF8 policy; final-tool whole production regressions
with proper provider closure and affected Blink/libsmb2; full reproduction and
P0–P6. Interim successful foundations do not qualify all interpreter requirements.

## Owner B round 7 — shared inputs and interim translated runtime matrix validated

**Serialization qualification open:** generic common consumer publish/run lacks
the required shared flock. Original175737 successful AOT consumer commands are
preserved. B stopped its corrective raw publish while still waiting on A's D10
flock, before dotnet start (exit-15/38.27s); exact process/queue evidence remains.
Locked consumer qualification is deferred to the new explicit D10 product to
avoid a queued command silently consuming replaced sources. ABI/runtime/rooted
hooks explicitly lock each command. A owns the common framework path; no B
compiler/recipe edit or lock bypass.

**A snapshot ready:** artifacts/owner-b-r7-20261017/final-runtime-hashes.json
verifies all162 current paths. It includes FileLib
`d02583f37b3380f1cc9dac01ba555b97771f5254291b074b0dcff27a524d21fb`
and math.h
`7fcb6473ce51271cc834f2cbc8212c12ad49de4b32971a81a2ab38ae14fb1845`.
Earlier162 hashes are retained verbatim in final-runtime-hashes-stdio-complete.json.
No B compiler build/core emission/publication, nested agent or commit. R6 failed
at MODEL CAPACITY; every partial source/failed artifact remains preserved.

Fresh integrated hook receipt175127-c121cf50 now passes **both JIT forms** for
ABI/behavior/threads/lifecycle against203708. ABI38 real assertions/two cycles /
24 common native layouts per form; target unsigned wchar2 stays explicit.
Behavior5 groups, threads2, lifecycle2 per cycle, two cycles per cell; all pass,
including formerly failing IEEE math. Owning cancellation/trace/lifetimes/drain
assertions run. integrated-jit-summary.json records exact paths/counts. Both
ordinary NativeAOT forms and both whole-library/facade-rooted NativeAOT forms
also pass, with exact receipts below. D10 remains A's
exclusive work and will require all-new product/matrix results. No P1/D3 closure.

Stdio old-association flushing and failed-reopen closure pass **35 native /35
emitted JIT /35 emitted NativeAOT**, exact stdout. Direct actual current-libc
JIT/AOT each pass45 (borrowed buffers, same-path truncation, missing/invalid-mode
closure, descriptor reuse/stale handles, old flush-error reset and input/access
reset). Existing direct-libc file/stdio/error/ownership/formatting/service units
pass65/65, no skips, resolved current runtime DLL equality recorded. Native /
failed-before / final commands and source versions remain immutable.

New actual CPython behavior executes all9 groups in both earlier195909 JIT
forms: **8 pass /1 fail**, required stdlib's NaN/infinity assertion. Read-only
actual emitted inspection isolates an int cast around a signed HUGE_VAL
conditional. An independent C reduction passes natively and fails emitted (int
width/finite value/NaN loss). Shared math.h now defines typed IEEE constant
expressions for HUGE_VAL/F/L, INFINITY and NAN. Normal current-header includes
pass16 native/JIT/AOT assertions for types, signs, storage, NaNs and promotions,
exact stdout. No compiler/generated-source change. Explicit source include
identity is in math-header-inputs.json; all other runtime/header resources match
A's built compiler library. No old-product interpreter fix pass is inferred.

**Actual CPython195909 thread and lifecycle JIT cells pass in both forms**, each
two full cycles, with two required Python fixture groups selected per cycle.
Real worker/TLS/condition/managed callbacks, errors/trace preservation, loop/wait
cancellation/reuse, owner/value rejection, callback/active-wait drain and
finalization execute through actual product exports. Exact cells/inputs are in
cpython-jit-thread-lifecycle-summary.json and cpython-behavior-pinned-inputs.json.
A's ABI201502 independently passes38 real assertions/two cycles and24 common
layout fields per JIT form; native wchar4 versus target unsigned wchar2 remains
explicit. B did not duplicate ABI. AOT/P1/D3/full matrix remain open.

The authored fixture's unnecessary argparse CLI dependency is separated from
its explicit embedding function. Fixtures now import normally from the API's
configured application path; native -I CLI explicitly configures that same
fixture directory. All9 groups/assertions remain, native CLI/direct equality
passes (5 behavior/4 unselected); old failed receipts/fixture versions remain.
A independently closes real already-selected importlib.util/_abc dependencies
in its owning inventory, no source drops or product test-only APIs.

QuickJS actual-source NUL repair passes163 JIT/163 NativeAOT boundary assertions,
including real JS_Eval and poisoned buffers/WTF-8. JIT owning consumer/ABI/
behavior/lifecycle/upstream passes both forms (receipt172743-2b08b2f5); AOT owning
consumer/behavior/lifecycle/upstream passes both forms (173510-a55dcf19). These
are **historical-engine-runtime baseline preservation with the marshaller fix**;
no new-libc production qualification or AOT whole-library ABI claim. Final-tool
regeneration/runtime/AOT for all production references and affected Blink/libsmb2
remain required. r7-summary.json records exact receipts/counts/remaining gates.

A published all193 receipt203708-d34d1ee6 with C027, physical-size guard, these
final runtime/IEEE inputs and complete cache;186 Python/188 total assets add
only observed eager importlib files. Existing195909 stays provenance-distinct.
The completed runtime matrix below exercises203708; A's required D10 work
still needs fresh full193/full286 regeneration and new matrix/preservation.

### R7 standard regression verification complete

Both new ordinary-discovery functional fixtures pass **2/2, zero skipped**:
16-check typed IEEE constants and35-check freopen errors. The private test
snapshot uses the existing compiled functional harness, A's rebuilt CLI compiler
and freshly built actual libc dependency; every runtime/header resource matches
current source. No B compiler/harness build. Exact2-test TRX counters, source /
DLL identities and commands are in standard-functional-results and
standard-functional-snapshot-identity.json. All162 final source hashes remain
stable; completed interim product runtime results are recorded below.

### Integrated203708 ordinary and rooted NativeAOT PASS; final handoff

Receipt `20261017-175737-1ecb6a1a` passes both NativeAOT forms for ABI,
ordinary trimmed owning consumer, behavior, threads and lifecycle. ABI38 real
assertions/2 cycles/24 common fields per cell retain the explicit wchar ABI
difference; each consumer has2 owning cycles/6 callbacks/exact output. Selected
Python groups pass5 behavior/2 threads/2 lifecycle per cycle,2 cycles each, plus
real wrapper cancellation, trace, ownership, error and worker/active-wait drain.

Actual whole TranslatedCPython **and Managed.CPython facade** rooted NativeAOT
passes raw receipt `20261017-175516-1b21790c` and processed
`20261017-180236-b8744b01`: all9 groups each cycle,2 cycles/form, real wrapper
gates. Raw publication53.14s/run1.26s; processed publication43.14s/run command
208.77s includes shared-flock queue time. No old always-throw AOT pass is inferred.
All B commands completed with normal campaign lock release, no bypass/deletion.

integrated-matrix-summary.json binds203708 receipt hash,262 owned generated
files/form and all162 current runtime sources; it verifies4 consumer cells,
4 ABI cells,12 runtime cells and2 rooted forms from actual reports/stdout.
JIT consumer evidence is coordinator receipt204543-7bb548af, without duplication.
Exact AOT/per-cycle reports and separate summaries remain in the R7 artifacts.

The162-path final-runtime-hashes.json is unchanged and ready for A's D10 tools.
Latest coordinator reports provisional C028/C029,30 focused semantic passes
and2,977 compiler passes/same malloc-text baseline failure; shared functional
507/507, literal pool14/14 and reduced actual JIT/AOT allocation5 properties /
70,010 RVA bytes /10,000 zero-allocation iterations also pass. Strict full193
receipt210915-401ea00d is active with359 inputs/all B162 matching; full-product
allocation/IL evidence and full286 closure remain A-owned requirements. B must
consume source/receipt/PE-bound generated_code_evidence.py/GeneratedDataAudit
JSONs in final audit and rerun the final product matrix, rooted AOT, delivery /
relocation/audit/reproduction and required upstream outcomes. Frozen required
skips/xfails and production final-tool regeneration/runtime/AOT (plus affected
Blink/libsmb2) remain open. Interim203708 passes do not qualify D10, D3 or P1.

## Owner B round 6 — historical partial work; MODEL CAPACITY failure

Current evidence: `artifacts/owner-b-r6-20261017`; B task
`cpython-impl-runtime-regressions-r6-20261017`, with A R5 retaining exclusive
compiler/config/core regeneration/publication. Exact full task ID and commands
are at the top of [api-progress.md](api-progress.md).

All four production canonical products embed historical libc source; an
ordinary rebuild cannot qualify the new FILE/mmap/passwd/locale/math services.
Fresh pinned historical SQLite consumer/threading/HostVfs controls pass.
QuickJS both-form JIT consumer and ABI controls pass; processed behavior has
one intermittent `ReferenceError: 'u' is not defined` failure. Its unchanged
rebuilt binary and retained historical binary pass one repeat each. Keep that
failure visible and distinct from candidate libc evidence. Final regenerated
production suites and AOT plus affected Blink/libsmb2 checks remain open.

A native `setvbuf`/`freopen` ordering regression passes `old=A new=B`; the
current-libc pre-fix harness awaits the shared build lock. No corresponding
runtime repair/pass is claimed yet. All queued builds are serialized with A's
active C026 retry; no compiler rebuild/core emission by B.

Actual CPython still selects receipt `20261017-193052-824be3ee`. Its 24 common
layout matches remain separate from the initializer failure before runtime
assertions. No new actual interpreter/AOT pass; no repeat of the known
always-throw AOT initializer. Short JIT on A's explicitly repaired receipt is
the next owning ABI/facade execution gate. Frozen upstream blockers and unsigned
16-bit wchar remain unchanged; P0–P6 unpassed, no commit or nested agent.

## Recovery R5 — actual raw-build shared-service closure

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


R4 cancellation after the PC restart preserved all partial work. B task ID:
`node:delegated-task:command%3Amcp%3A7e24ebe0-a118-47e4-9d06-9641c99216e9%3Adelegate-task%3Acpython-impl-libc-restart-r5-20261017`.
A owns compiler/config/full-193 regeneration; B owns these shared services and
focused tests. No commit, staging, reset/cleanup, worktree or nested agent.

The actual R4 raw diagnostics contain **54 unresolved shared-symbol references
and eight passwd member accesses** in the B handoff. Services/macros are now
shared implementations/header declarations, without emitted-source repair:
FILE adoption and buffering, IEEE math, clock resolution, real mmap/munmap,
O_NOFOLLOW, timestamps, NSS passwd/identity and locale/text conversions. The
complete live integration/macro handoff and exact commands are at the top of
[api-progress.md](api-progress.md). mmap was previously an explicit ENOTSUP;
implementing munmap alone would not have enabled CPython's real pymalloc arena
path. Both are now actual OS mappings, including partial intervals and owner
cleanup, with file descriptors translated through the shared table.

Unsigned **16-bit wchar remains explicit**. No glibc wide-character function
is called. C/POSIX ASCII and C.UTF-8 conversion preserve byte/unit lengths,
NUL termination, measurement, non-BMP surrogate pairs and complete-scalar short
writes; malformed UTF-8 and lone surrogates return `(size_t)-1`/EILSEQ. Native
controls use ordinary Linux wchar4 and assert the deliberately different unit
counts separately. This is not an assertion of equal wide layouts.

Native/header/runtime passwd layout is **48 bytes, alignment 8**, pointer
fields at **0/8/24/32/40**, unsigned uid/gid at **16/20**. Real NSS `_r` bindings
fill reusable per-runtime/per-worker storage; the public APIs remain getpwuid/
getpwnam, with borrowed record/string lifetimes through the next same-thread
lookup or owner disposal. Actual UID/EUID/GID/EGID and all seven passwd fields
match the native control. Native NSS ENOENT remains a real error rather than an
invented success. Two real workers verify independent records/errno.

FILE checks cover adoption without dup/truncation, failed-adoption ownership,
append behavior, full/line/unbuffered writes, caller-buffer lifetime, UTF-8
formatted output, close/flush failures and stale-handle invalidation after fd
reuse. Shared standard-stream backing policy remains explicit. Native utimbuf
is the LP64 pair of signed time_t values (16 bytes, offsets 0/8); the OS updates
both atomically. stat's atime now reflects real access time, independently of
mtime; no broader stat conformance is inferred. CLOCK_REALTIME resolution
matches the existing BCL reading's 100 ns increments, and CLOCK_MONOTONIC uses
the rounded Stopwatch period. MAP_FIXED and signal-handler dispatch/pause fail
ENOTSUP. Other host ABIs fail services they cannot implement.

Fresh final C024-snapshot controls: `services-c024-final` has **99 native assertions and 102
each in emitted JIT/NativeAOT**, all stages exit 0. **33 binary64 vector values
match native at 0 ULP** in both cells; special controls include NaN, signed zero,
subnormals, infinity, fused cancellation/rounding and range errors. BCL math
uses the fixed nearest/default rounding contract; erf/erfc bind only Linux libm.
Both anonymous and descriptor-backed mapping bytes survive their required
lifetimes, and partial unmap intervals retain neighboring bytes. Unit mincore
control confirms owner disposal really unmaps a retained arena.

Repository final ordinary-reference service selection passes **190/190**, including
15 R5 cases, zero skipped; tested/runtime DLL equality was captured while holding
the shared flock. Earlier affected emitted selection passed 9/9. The unchanged
required local callback array now separately passes native/JIT/NativeAOT with
A's validated C024 tools: `array-c024-{commands,comparison}.json`; all seven
stages exit 0 and expected stdout matches. The original source is byte-identical
to `fnptr-array-source.c`. Its old-tool parser failure and superseded scalar
variation remain historical, without workaround/compiler/generated-source edits.
Counts from earlier variants overlap rather than sum.
Current evidence/private output roots are `artifacts/owner-b-r5-20261017` and
`build/owner-b-r5-20261017`; exact commands and retained failures are linked in
the API ledger. All shared commands hold `flock --close`; native controls use
its separate lock and no native interpreter/helper profile is rebuilt.

HAVE_LANGINFO_H also activates real `_locale` day/month/format constants. The
new shared header declares all **54 C-formatting items plus CODESET**. Explicit
Linux nl_item values and every returned byte now pass a fresh native/emitted
JIT/NativeAOT comparison (`services-c024-final`, 99/102/102 assertions, both C and C.UTF-8).
C conventions retain immutable borrowed storage across C.UTF-8 LC_CTYPE changes;
C CRNCYSTR is the real `"-"` prefix, not an invented empty value. No locale query
is made to return unsupported/empty success in place of its selected semantics.
Unknown selectors explicitly set ENOTSUP. The additional unit case passes in the 190-case final selection. Refreshed
shared metadata reads 110 types/1,454 methods/4,838 IL edges/31 imports; its
libm services are explicitly classified, and 21 auditor controls pass. These
are shared-runtime inspections, not CPython artifact audits. The unchanged callback-array
function-pointer regression passes with final C024 tools as described above. A's earlier 193-unit retry
`20261017-191623-134a3c10` reached only these 43 missing constants with its older
header snapshot; B retains that raw failure. A's latest receipt
`20261017-193052-824be3ee` now passes both actual canonical builds/publication.
B has not run a competing core.

Actual-product ABI runner/handoff is now authored in `tests/Abi/Abi.csproj`,
`Program.cs` and `campaign_hooks.py`. It references the existing real assembly,
reads actual emitted layouts (including anonymous-union refcount storage), and
calls actual refcount/type/slot/varargs/vectorcall/Host exports; no mirror C
structs or native backend. The hook compares native wchar4 and target unsigned
wchar16 explicitly and refuses missing products/oracles/incomplete reports.
A has replaced the recipe's pending ABI binding with
`tests/Abi/campaign_hooks.py:run_suite`. The raw ABI build passes after correcting its authored exception-pointer
cast; layout comparison passes 24 common fields. Actual raw operations and
the useful consumer fail at the emitted class initializer before initialization.
Both Behavior JIT forms and raw actual AOT now confirm the initializer
failure; no B command remains active.

Shared controls, the 24-field actual raw layout comparison and product builds
remain distinct from complete interpreter qualification. No successful translated
refcount/GC/GIL/lifecycle/facade execution, four-cell interpreter matrix,
rooted/trimmed AOT, canonical product or P0–P6 acceptance is claimed. Required
SQLite/QuickJS/picotls/MsQuic final-tool regeneration and runtime/AOT, plus
affected Blink/libsmb2 checks, remain open. Frozen upstream IDs and their native
UTF-8/unconditional-marker contradiction are unchanged pending adjudication.

## Round 4 — real shared POSIX process CPU accounting (ready for A retry)

A's real `posixmodule.c` requires `struct tms`/`times`; neither existed in shared
headers/runtime. The normal native C fixture passes while pre-repair DotCC
emission exits **2** with missing `sys/times.h` and incomplete `tms`. The new
shared header declares **four signed 64-bit clock_t fields**, size **32**, align
**8**, offsets **0/8/16/24** from actual C `sizeof`/`_Alignof`/`offsetof` controls.
Runtime `times(void*)` uses the real Linux LP64 OS call directly, including
waited-child counters; its rate is the actual libc `sysconf(_SC_CLK_TCK)` query.
The selector is **2**, independent of existing `CLOCKS_PER_SEC=1000`/`clock()`.
Other host ABIs fail with `ENOTSUP`, touching no output, rather than inventing
CPU accounting. Existing query cases and `clock` remain unchanged.

Native control, separate real waited-child control, emitted JIT and NativeAOT
pass with CPU-work advancement, nonnegative self/child counters, elapsed-tick
ordering/rate, output bounds, null-buffer elapsed query, success errno
preservation and invalid-sysconf `EINVAL`. Seven final build/run commands exit
**0** (`times-validation-commands.json`). This repairs only a host execution
service; all CPython algorithms and `os.times_impl` remain actual upstream C.
A must enable `HAVE_SYS_TIMES_H` and perform the whole 193-unit retry; no actual
interpreter or P1/D3 gate is closed by this probe.

The first native development probe passed an invalid pointer `(struct tms*)1`
and crashed **SIGSEGV** inside glibc `__times` (`sysdeps/unix/sysv/linux/times.c:44`,
verified with gdb). That is invalid C storage, not a required passing error
case or an oracle transcript to imitate. The final valid-input fixture retains
null-buffer behavior and real sysconf error checks, and makes no arbitrary
pointer-safety claim. Historical `times-native-run.log`/command remain preserved.

Focused repository regressions pass **14 unit / 1 emitted functional / zero
skipped** (all exit 0; existing functional-project analyzer warnings remain).
**Both `HAVE_SYS_TIMES_H=1` and `HAVE_SYSCONF=1` are required.** The previous
target policy omitted the latter; actual `Python/fileutils.c:3032–3043` otherwise
chooses the bogus 60 Hz fallback while this host's real rate is 100 Hz. Header
completion alone would therefore leave `os.times` numerically wrong.
This actual shared runtime change adds final-tool regeneration/runtime/AOT obligations for SQLite,
QuickJS, picotls and MsQuic plus affected Blink/libsmb2, before P6. Earlier source
probes/historical outputs do not satisfy those obligations. B has not edited
A's configuration/compiler/recipe or native product trees, and has not emitted
a competing core or native engine. Exact commands and current investigation
evidence are under `artifacts/owner-b-r4-20261017`.

A acknowledged the shared-service readiness and enabled both target macros.
The in-flight actual full-source retries, 286-unit separate test profile and
publication remain A-owned; B has performed no duplicate CPython core emission.
`tests/Delivery` now provides actual-artifact framework audit/relocation and
Behavior/thread/lifecycle hooks. `tests/Boundary/RootedBoundary.csproj` roots the
entire translated/facade assemblies in AOT and executes the same real lifecycle
harness; the sample remains normally trimmed. No real product is yet present,
so these actual interpreter ABI/lifecycle/consumer cells are explicitly unrun.

Actual standalone shared-runtime metadata inspection reads 105 types / 1,391
methods / 4,584 IL edges / 19 native imports, including the newly required
`times`/`sysconf` OS services. All imports are reported against that real basis;
unused shared loader definitions remain visible, and product/facade calls into
dynamic loading or process engines are rejected. Actual Host names derive from
the C header; signatures/constants derive from actual compiled product metadata
when available, with direct facade builds as the type check. No Python layout
mirror or new native engine dependency was added.

19 auditor rejection controls, 18 existing reporter controls and 24 authored
AST/XML checks pass. NativeAOT notice target resolution passes with the actual
target pack. These are harness/shared-service results only; no fully rooted
CPython AOT build or product audit has passed. Exact native required outcomes,
the UTF-8 configuration contradiction and director decision are in the API
ledger. Both source/decorator markers and frozen required IDs remain intact.

## Round 3 — ordinary trace semantics, helper profile and remaining ABI gates

The actual Host had an observable defect: `dcp_operation_begin` always installed
its private trace, even for an ordinary owning call with no cancellable token.
A real native assertion (`PyThreadState_Get()->c_tracefunc == NULL` at ordinary
entry) fails with **SIGABRT / exit -6** before the repair. The C definition/header
now take **`dcp_operation_begin(void *context, int can_cancel)`**; authored
`TranslatedExports.Operation` passes `CancellationToken.CanBeCanceled ? 1 : 0`.
Normal operations track entry but preserve the active Python trace and any
trace changes made by their code. Explicitly cancellable operations retain the
previous temporary-hook/registration-drain contract. A must reemit the Host and
facade together; old one-argument outputs are stale.

Fresh exact-pin native boundary controls build with `-Wall -Wextra -Werror` and
pass **two complete cycles**, ordinary trace absence/preservation and real user
trace events, all prior real streams/errors/refcounts/callback/worker checks,
nonfatal bad-startup status, daemon C-wait drain and finalization flush failure.
Loop/wait interruption measures **0.050148 / 0.050398 / 0.050129 / 0.050398 s**,
all below two seconds, retaining real frames and same-interpreter reuse. Native
requester threads join before operation reset/context destruction. No lock spans
a join; roots remain through finalization. No shared libc/header change.

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -I cpython/build/upstream-profile/owner-b-r3-20261017/build -I cpython/build/upstream-profile/owner-b-r3-20261017/source/Include -I cpython/src/Host cpython/src/Host/dotcc_cpython_host.c cpython/tests/Embedding/native_boundary_control.c cpython/build/upstream-profile/owner-b-r3-20261017/build/libpython3.13.a cpython/build/upstream-profile/owner-b-r3-20261017/build/Modules/_hacl/libHacl_Hash_SHA2.a -lm -ldl -lpthread -o cpython/build/upstream-profile/owner-b-r3-20261017/boundary-trace-fixed
cpython/build/upstream-profile/owner-b-r3-20261017/boundary-trace-fixed /home/marius/p/dotcc/cpython/build/upstream-profile/owner-b-r3-20261017/source/Lib /home/marius/p/dotcc/cpython/samples/ManagedConsumer/app
cpython/build/upstream-profile/owner-b-r3-20261017/boundary-trace-fixed /home/marius/p/dotcc/cpython/build/upstream-profile/owner-b-r3-20261017/source/Lib /home/marius/p/dotcc/cpython/samples/ManagedConsumer/app --bad-startup
```

All three final commands exit **0**. Failed pre-repair controls and final argv/
CWD/exits remain in `artifacts/owner-b-r3-20261017/validation-commands.json`.
Logs: `boundary-before-trace-{build,run}.log` and
`boundary-trace-fixed-{build,run,bad-startup}.log`. The command transcript records
absolute paths/private CWD; commands above are root-relative equivalents.
These execute real native CPython and C callbacks, not translated/managed Python.

Limited real-header Host emission and managed-library link both exit **0** with
actual 16-bit product config and current tool hashes captured under the shared
lock. Untouched generated output exports
`public static unsafe void dcp_operation_begin(void* context, int can_cancel)`.
`host-emission-commands.json` contains exact argv/lock/logs. No whole-core
emission, emitted-source repair, replacement interpreter or success stub.
The separate-profile product prerequisite exits **1**, as required while
`generated/profiles/upstream-tests-static/TranslatedCPython/TranslatedCPython.csproj`
is absent. Managed Upstream/facade execution and all real interpreter JIT/AOT
ABI/layout/static-identity gates remain **blocked/unrun**. Source integration
is ready; neither a C# compile of the facade nor an interpreter pass is claimed.

The private upstream native helper profile now builds **30 static modules /
94 actual C sources** from the already acquired pin. Manifest, exact Setup
rows/flags, actual Makefile closure, all PyInit symbols/static registrations,
bootstrap generator identity and **28 generated assets** are in
`build/upstream-profile/owner-b-r3-20261017/helper-sources.json`. Native audit
finds only libc/libm ELF dependencies, no dynamic libpython and no built `.so`
extensions. The native profile retains normal **32-bit wchar**, GIL and pymalloc.
Its platform config/sysconfig data are **not** DotCC's 16-bit ABI configuration.
A must derive the separate translated test profile from its real product config.
No test API enters delivered product selection by this native build.

Full case selection/evidence and A recipe/asset handoff are at the top of
[api-progress.md](api-progress.md). Frozen inventory covers **5,134 IDs / 4,867
required / 267 individual exclusions**; native selected execution covers **4,875
occurrences** with **4,868 passed, 4 required skips, 3 expected failures**, exit
**1**. Seven required native outcomes remain blockers and no translated
qualification is inferred. A/coordinator still owns product/compiler closure,
production regressions, four-cell execution and P0–P6 acceptance.

## Round 2 — actual owning C boundary and managed function-pointer ABI

Authored `src/Host/dotcc_cpython_host.{c,h}` is an ordinary translation input,
not an application-owned native dependency. It invokes real CPython APIs for
configuration, stream capture, raised exceptions, METH_O capsule-backed methods,
thread-state save/restore, worker shutdown and finalization. CPython retains all
interpreter algorithms/GC/GIL. Native linking below is solely a **control**.
`src/Managed.CPython/TranslatedExports.cs` directly consumes the actual generated
class; object pointers use its nested `_object` type. Ordinary object/numeric/
Unicode APIs and exported refcount inlines/constants require no forwarding C
wrappers. Detailed A integration instructions are near the top of
[api-progress.md](api-progress.md).

All fresh work/logs below are under `build/owner-b-r2-20261017` and
`artifacts/owner-b-r2-20261017`. CWD `/home/marius/p/dotcc`. Exact argv, CWD,
exit codes, expected failures and logs are persisted in
`artifacts/owner-b-r2-20261017/validation-commands.json` and `thunk-commands.json`.
Every shared compiler/dotnet/publish invocation held the shared flock; native
builds held the native flock. No duplicate native interpreter build, acquisition,
full selected-manifest translation, publication or shared implementation repair.

### Native-real boundary control (no managed/translated CPython pass)

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -I cpython/build/native/build -I cpython/build/native/source/Include -I cpython/src/Host cpython/src/Host/dotcc_cpython_host.c cpython/tests/Embedding/native_boundary_control.c cpython/build/native/build/libpython3.13.a cpython/build/native/build/Modules/_hacl/libHacl_Hash_SHA2.a -lm -ldl -lpthread -o cpython/build/owner-b-r2-20261017/native-boundary-control
timeout 25s cpython/build/owner-b-r2-20261017/native-boundary-control /home/marius/p/dotcc/cpython/build/native/source/Lib /home/marius/p/dotcc/cpython/samples/ManagedConsumer/app
timeout 15s cpython/build/owner-b-r2-20261017/native-boundary-control /home/marius/p/dotcc/cpython/build/native/source/Lib /home/marius/p/dotcc/cpython/samples/ManagedConsumer/app --bad-startup
```

All exit **0**. Control asserts exact 3.13.15 version, two isolated startup/
shutdown cycles; real PyStatus Error returned without process termination;
conservative rejection of reuse after partial initialization; real refcounts;
Unicode code points including NUL/non-BMP/lone surrogate; stable METH_O storage
and argument conversion/overflow; real Python worker callbacks; stdout/stderr/
stdin ownership; NUL-bearing stream and error message lengths; exact error type,
message and real traceback frames; SystemExit containment; visible formatting
failure retaining obtainable frames; ordinary reuse; and Py_FinalizeEx `-1`
for a real stream-flush error after completed finalization.

Real cooperative cancellation interrupts an infinite Python loop and
`host.wait_ms(30000)` in each cycle. Measured **0.050152, 0.050405, 0.050146,
0.050397 s** against a **<2 s** assertion. Same interpreter then evaluates 42.
The pre-existing real Python trace is restored and a precancelled operation
never runs its body. Wait uses upstream timed locks with GIL released between
polls (at most 10 ms). Cancellation token writes use C11 integer atomics without
GIL. Cancellation request threads are joined before resetting/freeing context.
Harness timeout is only a failure guard, never a passing interruption result.

The final lifecycle review added an active-C-wait drain: CPython's normal
shutdown omits daemon joins. The boundary counts its active waits under GIL,
rejects new waits after ordinary shutdown and releases GIL while draining
existing ones before context destruction. The native control leaves a daemon
inside a real 200 ms host wait and verifies that shutdown drains that wait,
then explicitly joins its remaining Python frame. This does not redefine
CPython's general daemon semantics or claim all daemon threads are joined.

The first native development run exposed an error-capture defect: trace-hook
KeyboardInterrupt lacked `cancel_control.py` in its traceback. The boundary now
calls actual `PyTraceBack_Here(frame)` on the interrupted real frame; final
control asserts that frame. It does not synthesize or normalize traceback text.
Earlier unsuccessful development runs are superseded by the explicitly named
`*-final` logs, not counted as passes.

Logs: `native-boundary-build-final.log`, `native-boundary-run-final.log`,
`native-boundary-bad-startup.log`. Ordinary native ABI still has 32-bit wchar.
No equality with the selected 16-bit product config is inferred from this run.

### Actual managed thunk through emitted C, JIT/NativeAOT

`tests/Embedding/managed_callback_probe.c` includes the actual Host callback
typedef and invokes the authored `CallbackThunks.Int64` by a real emitted C
function pointer. Its two pthread workers both enter managed code concurrently
and join before roots are removed. This is a **service/ABI probe without an
interpreter**, and cannot prove CPython GIL callbacks.

Normal emission uses `--emit=managedlib --runtime=c --nest-types --class-name
CallbackProbe --namespace Managed.CPython.Probes -std=c11 -I cpython/src/Host`.
`ManagedThunks.csproj` references that actual generated project with the explicit
`CallbackProbeProject` property and original authored root/thunk/text sources.
Fresh command transcript is `thunk-commands.json` (entire transcript ran inside
the shared flock; tool DLL hashes were captured under that same lock).

**21 assertions pass JIT and NativeAOT**, emitted project/build/publish/run exit
**0**, no warnings/errors. Cases include real new managed worker identities,
root retention during GC, successful values, overflow, expired token,
throwing/invalid exception.Message, NUL diagnostic escaping, complete UTF-8
truncation with a capacity sentinel, and zero/one-byte error capacities. The
actual managed signature is
`delegate*<long,long,long*,byte*,ulong,int>`; no dynamic delegate thunk or
unmanaged calling-convention conversion is used. This reaches the actual static
method through emitted C in AOT; it is not a mock exported implementation.

### Limited actual-header Host emission/link (not a product build)

The final Host source emits with `-std=c11 -DPy_BUILD_CORE
-DUSE_COMPUTED_GOTOS=0`, config at
`build/campaign/20261017-170042-b03542bd/config` (actual 16-bit wchar policy),
actual native source Include/internal headers and original `src/Host`.
Object emission and managed-library link both exit **0**. Link uses normal
campaign flags plus `--export-inline Py_INCREF --export-inline Py_DECREF
--export-enum PyUnicode_4BYTE_KIND`. Untouched output confirms public host APIs,
`CPython._object*`, public refcount inline methods and
`public const int PyUnicode_4BYTE_KIND = 4`.

The standalone generated build exits **1** with **204 distinct CS0103**
diagnostics for 106 missing real core symbols. It contains no current CS0306
diagnostic; that observation
does not substitute for A's pointer-atomic regression/full-source retry.
`host-diagnostic-errors.json` retains each error. No core symbols were stubbed
and no generated C# was edited. Full core link, facade compilation, actual
translated layouts/operations/callbacks and all four interpreter cells remain
**blocked/unrun**. Logs: `host-final-emit-validated.log`,
`host-final-link-validated.log`, `host-standalone-diagnostic-build.log`,
`host-tool-identity.log`.

Fresh support-only Contracts JIT/AOT also pass **32 + 3** assertions per runtime.
The actual product-prerequisite check exits **1** (canonical product absent).
See API ledger for the required Unicode fixture failure and blocked upstream
helper profile. No shared libc/C header implementation changed; production
regressions for A's shared compiler work remain with A/coordinator. This is
substantive boundary progress, not P1/P3 or campaign completion.

## Prior round evidence (historical)

## Inputs and artifact ownership

The pin/source/native toolchain are supplied by A under
`ref/cpython-4061bc4c35f7c26f25264666d4ba083b93d2f6f9` and `build/native`.
`tests/Abi/cpython_layout.c` includes actual `Python.h`; all sizes/offsets come
from real upstream types. No mirror structs or implementation transplant.

All commands below use CWD `/home/marius/p/dotcc`,
`probe_work=cpython/build/owner-b-20261017`,
`probe_logs=cpython/artifacts/owner-b-20261017`.
Shared compiler/dotnet/publication commands take
`/tmp/dotcc-cpython-shared-build.lock`; native builds take
`/tmp/dotcc-cpython-native-build.lock`. The ordinary Linux controls and controlled
16-bit header-only controls are distinct executables. B acquired no source,
rebuilt no native interpreter/compiler, and published no campaign product.

## Real pthread/TLS/atomic and 16-bit wide service probe

`libc_capabilities.c` asserts two real workers and distinct thread identities,
mutex/condition wait and broadcast, thread-local values with two destructor
calls, once initialization, 4,000 atomic increments, successful/failed CAS and
join return values. Emitted mode also asserts unsigned 16-bit wchar, a non-BMP
surrogate pair counted as two code units, NUL termination and length-bearing
wide copying past an embedded NUL. It exercises the existing shared runtime
through normal `--runtime=c` emission, without a private replacement libc.

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -pthread cpython/tests/Abi/libc_capabilities.c -o "$probe_work/libc-native"
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -fshort-wchar -DDOTCC_EXPECT_SHORT_WCHAR -DDOTCC_NATIVE_SHORT_WCHAR -pthread cpython/tests/Abi/libc_capabilities.c -o "$probe_work/libc-native-matching"
timeout 15s "$probe_work/libc-native"
timeout 15s "$probe_work/libc-native-matching"
flock /tmp/dotcc-cpython-shared-build.lock dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=csproj --runtime=c -std=c11 -DDOTCC_EXPECT_SHORT_WCHAR cpython/tests/Abi/libc_capabilities.c -o "$probe_work/libc-emitted"
flock /tmp/dotcc-cpython-shared-build.lock dotnet build "$probe_work/libc-emitted/libc-emitted.csproj" -c Release --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/libc-products --disable-build-servers -m:1
timeout 20s dotnet "$probe_work/libc-products/bin/libc-emitted/release/libc-emitted.dll"
flock /tmp/dotcc-cpython-shared-build.lock dotnet publish "$probe_work/libc-emitted/libc-emitted.csproj" -c Release -r linux-x64 -p:PublishAot=true -p:StripSymbols=true --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/libc-aot --disable-build-servers -m:1 -o /home/marius/p/dotcc/cpython/build/owner-b-20261017/libc-aot-publish
timeout 20s "$probe_work/libc-aot-publish/libc-emitted"
```

All exit **0**; managed builds report no warnings/errors. All four executions
return exactly
`{"threads":2,"increments":4000,"tls_destructors":2,"once":1,"cas":42}`.
Logs: `libc-native-{build,run}.log`, `libc-native-matching-{build,run}.log`,
`libc-emit.log`, `libc-emitted-{build,run}.log`, `libc-aot-{build,run}.log`.
The matching-native wide check uses a local length loop and **never** calls
glibc's 32-bit-wchar functions. The wide service calls themselves execute in
the emitted probe. No CPython GIL or interpreter workers are exercised here.
There is no processed-form or whole-product qualification claim.

## Actual CPython header layouts

Ordinary native and controlled matching-native probes both build/run. To create
the latter's private config, the one occurrence of
`#define SIZEOF_WCHAR_T 4` in A's native `pyconfig.h` was checked and replaced
with `#define SIZEOF_WCHAR_T 2` in
`build/owner-b-20261017/matching-include/pyconfig.h`. No acquired/staged source or
A-owned configuration was edited. Only this header-only executable uses
`-fshort-wchar`; it does not link to native libpython or call wide libc routines.

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -I cpython/build/native/build -I cpython/build/native/source/Include cpython/tests/Abi/cpython_layout.c -o "$probe_work/cpython-layout-native"
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -fshort-wchar -I "$probe_work/matching-include" -I cpython/build/native/source/Include cpython/tests/Abi/cpython_layout.c -o "$probe_work/cpython-layout-matching"
"$probe_work/cpython-layout-native" > "$probe_logs/cpython-layout-native.json"
"$probe_work/cpython-layout-matching" > "$probe_logs/cpython-layout-matching.json"
python3 cpython/tests/Abi/compare_layouts.py --native "$probe_logs/cpython-layout-native.json" --candidate "$probe_logs/cpython-layout-matching.json" --candidate-kind matching-native --report "$probe_logs/cpython-layout-comparison.json"
```

All exit **0**. Comparison asserts **24 common fields** equal and checks the
deliberate wchar difference independently: native size/config 4, signed;
matching size/config 2, unsigned. Actual common observations include
`PyObject=16/align8`, `ob_refcnt=0`, `ob_type=8`, `PyVarObject=24`, `ob_size=16`,
`PyTypeObject=416`, `tp_repr=88`, `tp_call=128`, `PyStatus=32`,
`PyWideStringList=16`, `PyPreConfig=40`, `PyConfig=448`, search-path field 320.
These values are native observations, not manually authored managed layouts.
Logs: `cpython-layout-native-build-final.log`,
`cpython-layout-matching-build.log`, and the three JSON files above.

The first DotCC actual-header attempt, using the native config, exited **2**
at `cpython/pyhash.h:38:17`, unexpected `const`, parser state 638. Retrying with
A's actual product `SIZEOF_WCHAR_T=2` configuration reproduced the same error.
Logs: `cpython-layout-emit.log`, `cpython-layout-product-config-emit.log`;
tool identity: `header-tool-sha256.txt`. No source/header erasure or emitted C#
repair was attempted. A owns the observed compiler defect and repair.

After A's concurrent compiler progress, the independently reduced qualifier
scenario `cpython_const_slot.c` emits, builds and runs (native and emitted JIT,
all exit 0). The actual header probe now **emits** with the selected product
configuration. Fresh commands:

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror cpython/tests/Abi/cpython_const_slot.c -o "$probe_work/cpython-const-slot-native"
flock /tmp/dotcc-cpython-native-build.lock timeout 15s "$probe_work/cpython-const-slot-native"
flock /tmp/dotcc-cpython-shared-build.lock dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=csproj --runtime=c -std=c11 cpython/tests/Abi/cpython_const_slot.c -o "$probe_work/cpython-const-slot-emitted"
flock /tmp/dotcc-cpython-shared-build.lock dotnet build "$probe_work/cpython-const-slot-emitted/cpython-const-slot-emitted.csproj" -c Release --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/const-slot-products --disable-build-servers -m:1
timeout 20s dotnet "$probe_work/const-slot-products/bin/cpython-const-slot-emitted/release/cpython-const-slot-emitted.dll"
flock /tmp/dotcc-cpython-shared-build.lock timeout 120s dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=csproj --runtime=c -std=c11 -I cpython/build/campaign/20261017-165243-7c6548ac/config -I cpython/build/native/source/Include cpython/tests/Abi/cpython_layout.c -o "$probe_work/cpython-layout-product-config-retry"
flock /tmp/dotcc-cpython-shared-build.lock dotnet build "$probe_work/cpython-layout-product-config-retry/cpython-layout-product-config-retry.csproj" -c Release --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/cpython-layout-products --disable-build-servers -m:1
```

Tool DLL hashes were captured while holding the same flock immediately before
the last header emission (`header-retry-tool-sha256.txt`). The generated C# is
untouched. The standalone build exits **1** with 73 errors. Most are expected
missing real-core dependencies of exported header inlines (`PyMutex_Lock`,
`PyLong_Type`, `_Py_Dealloc`, monitoring APIs, etc.); do not stub them to build the
probe. Two are a distinct compiler defect: **CS0306** at generated lines 810 and
936, where actual `_Py_atomic_compare_exchange_ptr`/`_Py_atomic_exchange_ptr`
lower to `Atomic.CompareExchange`/`Atomic.Exchange` with a `void*` generic type
argument. They must use pointer-width integer storage in compiler lowering.
The header probe has **not executed**, so no translated layout equality is claimed.
Logs: `cpython-const-slot-{native-build,native-run,emit,emitted-build,emitted-run}.log`,
`cpython-layout-product-config-retry-emit.log`, `cpython-layout-emitted-build.log`.

The pointer problem is independently reproduced by the final
`tests/Abi/cpython_pointer_atomics.c`. It exercises real C11 pointer CAS success,
CAS failure (including expected-value update), and exchange through opaque
`void*` parameters matching `pyatomic_std.h`, with GNU intrinsic controls.
The native build/run exit **0**. DotCC emission exits **0**, but the untouched
generated project build exits **1** with exactly **two CS0306 errors** at lines
76/82. The earlier GNU-only control built successfully; the failure is
specifically the standard C11 pointer path, not the GNU lowering or shared
`Atomic<nint>` service. This is a compiler-owner handoff, not a libc repair.
The relevant existing lowering is the `atomic_exchange`/compare-exchange cases
in `DotCC.Lib/Backends/CSharpBackend.cs` around lines 3052–3056; the GNU path
already reinterprets pointer storage as `nint` in `CSharpBackend.GnuIntrinsics.cs`.

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror cpython/tests/Abi/cpython_pointer_atomics.c -o "$probe_work/cpython-pointer-atomics-native"
timeout 15s "$probe_work/cpython-pointer-atomics-native"
flock /tmp/dotcc-cpython-shared-build.lock dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=csproj --runtime=c -std=c11 cpython/tests/Abi/cpython_pointer_atomics.c -o "$probe_work/cpython-pointer-atomics-std-emitted"
flock /tmp/dotcc-cpython-shared-build.lock dotnet build "$probe_work/cpython-pointer-atomics-std-emitted/cpython-pointer-atomics-std-emitted.csproj" -c Release --artifacts-path /home/marius/p/dotcc/cpython/build/owner-b-20261017/pointer-atomics-std-products --disable-build-servers -m:1
```

Logs: `cpython-pointer-atomics-native-std-{build,run}.log`,
`cpython-pointer-atomics-std-emit.log`,
`cpython-pointer-atomics-std-emitted-build.log`. The first GNU-only trial's
sources/logs are retained under `cpython-pointer-atomics-emitted` and
`cpython-pointer-atomics-emitted-build.log`; its narrower build success does not
supersede the final native-passing C11 failure.

## Real refcounts, immortals, slots, vectorcall and varargs (native only)

`cpython_operations.c` initializes isolated exact-pin native CPython using an
explicit stdlib path, checks new/incremented/decremented references, immortal
singleton stability and type identity, calls a real type's `tp_repr` slot,
exercises `Py_BuildValue`/`PyArg_ParseTuple` mixed scalar/pointer/size varargs with
embedded NUL, invokes an actual `METH_O` callable through vectorcall, releases
everything and finalizes. The complete cycle repeats twice.

The initial link omitted the pin's separate HACL archive and failed with SHA2
undefined references. Native Makefile `LOCALMODLIBS` declares that archive;
the corrected probe link succeeds. This was a harness link-closure correction,
not a libc/compiler defect or a source adaptation.

```bash
flock /tmp/dotcc-cpython-native-build.lock cc -std=c11 -Wall -Wextra -Werror -DCP_B_ABI_MAIN -I cpython/build/native/build -I cpython/build/native/source/Include cpython/tests/Abi/cpython_operations.c cpython/build/native/build/libpython3.13.a cpython/build/native/build/Modules/_hacl/libHacl_Hash_SHA2.a -lm -ldl -lpthread -o "$probe_work/cpython-operations-native"
timeout 30s "$probe_work/cpython-operations-native" /home/marius/p/dotcc/cpython/build/native/source/Lib
```

Both exit **0**; the transcript is exactly
`{"cycles":2,"refcounts":true,"immortals":true,"slots":true,"varargs":true,"vectorcall":true}`.
Logs: `cpython-operations-native-build.log` (original failing command omitted
only the HACL archive), `cpython-operations-native-build-retry.log`,
`cpython-operations-native-run.log`. This test-only C callable is not evidence
of managed callbacks or translated CPython execution; its translated core link
and execution remain pending A's real product.

## Remaining gates and preservation

Still required: actual translated core/static identity/refcounts/slots/varargs,
managed callback thunks and `PyStatus`, initialization assets, full GIL/worker
shutdown, cooperative interpreter interruption, and raw/processed JIT/NativeAOT
execution. Matching-native controls and emitted service probes cannot close
those gates. API and fixture evidence is in [api-progress.md](api-progress.md).

No independently reproduced shared-libc defect was found, so B changed **no**
`DotCC.Libc` or supplied C header implementation. No new production downstream
regeneration is triggered by a B shared-code repair. SQLite, QuickJS, picotls
and MsQuic regression/qualification remain required for A's actual shared
compiler changes and P6; no inherited downstream result is claimed here.
Unrelated checkout changes, acquired source, A's NativeOracle/scripts/config and
all generated C# are preserved. A owns the whole-manifest compiler retry.


## Owner B35 — generic ftruncate position repair, focused proof only

The ftruncate method/doc alone in PosixFsLib.cs now preserves offsets during
shrink, extend and unchanged-length calls, including sparse offsets above2 GiB.
It uses the registered regular file's owning handle for POSIX ftruncate, retaining
native errno and avoiding a seek/restore race. C stdio callers explicitly flush
before descriptor access. B23 opendir/B29 getcwd and every other PosixFs byte,
B34 facade/pthread pins, stat, headers and descriptor/buffering services remain
unchanged. Windows RandomAccess and other POSIX ABIs remain unrun.

The independent native C control passes136 checks. The faithful current-Libc
fixture fails32/147 before repair and passes147/147 in both JIT and NativeAOT
after repair; all30 new/affected unit cases pass with zero skips (seven original
unit failures retained). Native readonly/pipe/socket/negative EINVAL, max-length
EFBIG and closed/invalid EBADF results are compared exactly. Own entry/Libc
PE/PDB/Compile inputs and176 ILC references/object/executable bind.
[Detailed source-ready handoff](owner-b-handoff-r35.md) retains the native
harness-boundary correction, original small source, commands and preservation.

[Direct genuine safe release](../artifacts/owner-b-r35-20261018/serial-slot-release-all.json)
records14 terminal PIDs,41.898 command seconds, three independent free checks
and no active/queued work. A30/supervisor received source pin19e0d945… for one
future normal capture/SAME-C emitted controls/RAW286jobs8, then actual unchanged12
and full4867 retries. No actual product was regenerated or proof retargeted.
B34's unique4751PASS/12FAIL/101requiredSKIP/3XFAIL remains the prior full result;
4759 is its pass-event count, including eight duplicates. Required nonpasses,
final both-form matrix and campaign acceptance remain OPEN.


## Owner B36 — V21 full RAW JIT has no unexpected failures; markers remain required

All12 original IO failures pass both focused and in the immediately following
unchanged full4867 run;8actual Python resize/write/read scenarios match B35 C's
native expectations, including sparse offsets above2GiB. A30's correct same-C
JIT/AOT proof is reused. Own library20b60ea7…/MVID2184752b-db8c-47b3-8eec-65e13b7f547d
has97 actual PDB documents and remains distinct from canonicald50031a5…;
entry/facade/library total115 documents bind their own actual Compile bytes.

Full RAW JIT completes322.367s with4867 unique started/completed:4763 unique PASS,
101required SKIP,3required XFAIL, zero failures/errors/fatal/unrun.4771 PASS events
include8repeated IDs. All12 previous failures are resolved with B35's captured
runtime repair. B36 changes no case, decorator, selection or shared implementation. Required markers remain
nonpasses and the ordinary suite correctly exits1.

Remaining categories are85bytecode startup skips,3UTF8 startup skips,10missing
pipe exposures,1missing symlink exposure,2unconditional upstream skips and3
upstream XFAIL. Fresh exact native availability takes3.169s: descriptor2reproduce
XFAIL;102case loads block on _socket15/binascii2/_testmultiphase72/
_posixsubprocess13. Actual native Setup.local disables those modules and
configure uses --disable-test-modules; a qualification-native closure needs
separate coordinated work. No replacement Python or native build was used.

[Detailed B36 handoff](owner-b-handoff-r36.md) and
[case-level conditions/next controls](../artifacts/owner-b-r36-20261018/required-marker-source-inventory.json)
record every original source/decorator, native availability and activation gap.
HAVE_PIPE cannot be enabled on source evidence alone: pipe slots have no Stream,
fstat rejects null Stream with EBADF and FileIO rejects EBADF. The new unchanged-C
pipe metadata reduction is SOURCE_ONLY/UNRUN, coordinated for A30's next safe phase.
No B34/B35 frozen stat/API/runtime or policy was changed. No environment setting
can run the2unconditional skips unchanged; completion adjudication stays explicit.

[Direct genuine safe return](../artifacts/owner-b-r36-20261018/serial-slot-release-to-a30.json)
records5terminal PIDs absent,3independent free locks and no active/queued work.
B36 is source/reporting/docs-only. Old B34/B35 history is preserved by this append.
All104 required nonpasses and final new193/processed/D10/both-form JIT/AOT/ABI/
owning/thread/lifecycle/rooted/trimmed/delivery/audit/downstream/reproduction/P6
remain OPEN; this actual RAW JIT result closes no campaign milestone.

## B37 — full same-pin native control and honest marker witnesses

A separate ordinary upstream native build preserves the old disabled-module
generator. Offline archive/source verification binds the same 3.13.15 commit;
configure (13.237s) and bounded make -j8 (16.544s) retain `libpython3.13.a`. All
75 selected registrations import before discovery. All 23 seeds reproduce exactly
5,134 unique IDs / 5,142 occurrences / 4,867 frozen required IDs, with no ID delta.
The native loader and 74 genuine shared modules are control-only; the translated
static closure is unchanged. Native wchar width is four bytes; the target remains
unsigned two-byte wchar. Optional native dbm/tk/readline absences waive no case.

All 104 original marker cases now execute with unchanged source/method/decorator
bindings, including the namespace-zip expected failure. Controlled C.UTF-8,
`-I -X utf8=0` gives 99 PASS, two SKIP and three XFAIL; mode 1 gives 96 PASS,
five SKIP and three XFAIL. Both have zero FAIL/ERROR/UNRUN. Tests use a writable
consumer-owned Lib/DSO copy. The first by-name harness failure is retained; the
corrected driver uses the unchanged runner's discovery, load_tests and module
fixtures. Native observations do not add translated passes.

[Every witness and unresolved policy](../artifacts/owner-b-r37-20261018/native-marker-adjudication.json)
preserves the final five required nonpasses: two unconditional upstream skips
and three expected failures. Their completion-policy question remains unanswered;
no native-parity exception, waiver, decorator or frozen-ID change is authorized.

After B38's exact validated source release, common `HAVE_PIPE=1` is selected.
[Ordered source/header audit](../artifacts/owner-b-r37-20261018/ordered-profile-header-audit-after-pipe.json)
binds every 193/286 unit and Clinic selector; no upstream core layout header
contains either feature guard. Five pure controls reject common/per-unit split
and glued D/U overrides, defined-zero service flags, wrong headers and incomplete
symlink/readlink/lstat closure. `HAVE_SYMLINK` remains off pending the independent
B39 repair: the unchanged C fixture passes 17 native checks but immutable V21
legacy and explicit-owner JIT each expose two ENOENT mapping defects. New emitted
pipe/symlink/Host and actual CPython qualification require the combined capture
and whole-unit retry; all final matrix/D10/P6 gates remain open.

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
