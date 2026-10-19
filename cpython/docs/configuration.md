# Effective profile and ABI

## A24 V16 capture and published socket policy

The policy remains byte-identical to A23/A22 (common SOCKET/STRFTIME/GETPID,
twelve reviewed test-only socket dependencies). B27's sys/socket.h SOMAXCONN4096
is the sole V15-to-V16 source/resource delta. Native and emitted default backlog
is128. Receipt `20261018-080530-377217d8` publishes all286 unchanged ordered units and
2329 assets per form with these exact tools. Embedded193 remains oldV11.
Actual PE/PDB/source/options and readiness evidence: [A24 handoff](../artifacts/owner-a-r24-20261018/a24-handoff.json).
Full runtime/ABI/D10 and upstream qualification remain open.


## A22: complete test-only socket dependency family selected

After B26's genuine V15 service proof/release, `upstream-tests-static` now selects
exactly HAVE_BIND, HAVE_LISTEN, HAVE_CONNECT, HAVE_ACCEPT, HAVE_GETSOCKNAME,
HAVE_GETPEERNAME, HAVE_GETADDRINFO, HAVE_NETDB_H, HAVE_ADDRINFO,
HAVE_SOCKADDR_STORAGE, HAVE_GETNAMEINFO and HAVE_GETHOSTBYNAME, each1.
Common HAVE_SOCKET=1 remains. One effective policy binds generated pyconfig,
target sysconfig and every ordered unit option;41 pure controls pass.

No HAVE_SOCKETPAIR, HAVE_ACCEPT4, HAVE_GETHOSTBYADDR, HAVE_GETSERVBYPORT,
HAVE_GETHOSTBYNAME_R, HAVE_GAI_STRERROR, HAVE_HSTRERROR or ENABLE_IPV6 is selected.
Embedded193 has no _socket unit/builtin or sysconfig asset. Source counts,
registrations and asset counts remain193/286,46/75 and188/2329; only test
sysconfig gains the twelve values. No forced header or upstream edit is used.

Fresh native/actual-emitted resolver layouts, unchanged socketunit before/after
emissions and16 real C resolver/self-pipe JIT/AOT cells pass. Native core layout
controls use actual DotCC headers, unsigned16 wchar and the existing syntax-only
preamble; glibc32-wchar/pthread layouts are not substituted. The owned native
oracle adds actual stdio/TLS-errno declarations after its retained initial RED.
Actual full regenerated product layout/ABI/D10 and required upstream behavior
remain open. Canonical193/286 are unchanged. See
[A22 evidence](../artifacts/owner-a-r22-20261018/a22-handoff.json); earlier sections
below retain their historical policy/evidence scopes.


## A19: narrow D4 resolver host dependency

The coordinator approved private host libc getnameinfo/gethostbyname_r calls
inside shared DotCC.Libc, extending its existing OS P/Invoke pattern (including
PasswdLib's reentrant NSS bridge). Native NSS canonical names/aliases,
NI_NOFQDN, TCP versus UDP service names, partial writes and error behavior cannot
be faithfully supplied by BCL DNS alone. This is a shared execution service
under D4. No native Python engine, application-owned shim or second libc is
introduced; public C gethostbyname remains nonreentrant and no public _r API or
HAVE_GETHOSTBYNAME_R is selected.

B24 owns the actual header/runtime implementation and its Linux x64 host ABI
proof. Its design uses host errno plus getaddrinfo/freeaddrinfo, copies returned
addrinfo into the existing tracked heap, and keeps borrowed hostent storage on
the existing runtime thread/context with deterministic cleanup. Actual size_t,
field order, ERANGE/result pointers, invalidation, independent threads/contexts
and JIT/NativeAOT controls are required. See the
[host dependency ledger](../config/host-contract.json). Actual delivered dynamic
dependencies, libc/NSS/service configuration, loader behavior and dependency/
license notices remain P6 audit work; no source-only dependency closure is claimed.

The twelve test-only values remain PRIVATE; embedded still selects no _socket
unit or sysconfig asset. A19's private source binding preserves193/286 units,
46/75 registrations and188/2329 assets, with only twelve test sysconfig additions.
No live policy change or new tool capture occurs. The initial local-header GCC
RED mixes declaration gaps and DotCC-header dialect errors; native ABI/service
observations and current-runtime qualification remain separate evidence.

## A18: required common socket flag; dependent family remains private

The explicit user requirement “We also should enable HAVE_SOCKET if upstream
does that” is implemented as common HAVE_SOCKET=1. The exact d3678b5c reference
selects it; this fork's own header/service supplies the typed API. Embedded193
has no _socket unit/builtin, and no module is added. Test286 already selects it.
Both headers gain the value; only the test sysconfig asset changes. Embedded188
assets remain unchanged and contain no sysconfig.41 live pure tests pass.

The six guarded fallback methods are HAVE_BIND, HAVE_LISTEN, HAVE_CONNECT,
HAVE_ACCEPT, HAVE_GETSOCKNAME and HAVE_GETPEERNAME. Their private candidate also
needs HAVE_GETADDRINFO, HAVE_NETDB_H, HAVE_ADDRINFO, HAVE_SOCKADDR_STORAGE,
HAVE_GETNAMEINFO and HAVE_GETHOSTBYNAME. All twelve dependent flags are absent
live. The common service remains protected even in units where no socket code
is selected; all13 names reject common/unit split/glued D/U overrides, defined0,
incomplete candidate families and divergent headers.10 private-candidate policy
controls pass; they do not establish missing service/header support.

The exact source path matters: HAVE_GETNAMEINFO skips the incompatible included
getnameinfo.c fallback, but also selects the real Python wrapper that calls
sock_decode_hostname. That helper's guard needs the chosen ordinary
gethostbyname contract, including hostent and real h_errno. No gethostbyaddr,
getservbyport, servent or reentrant family is needed under the complete chosen
modern path. Shared addrinfo uses uint addrlen and addr before canon pointer;
upstream fallback differs. Native/emitted field-by-field layouts are mandatory.
No SOCKETPAIR/ACCEPT4, forced header, upstream edit or generated repair is used.

B24 owns missing resolver/header/owner-thread lifetime implementation and native
RED/current-Libc proof. A18's [accepted handoff](../artifacts/owner-a-r18-20261018/socket-resolver-ownership-handoff.json),
[h_errno/candidate addendum](../artifacts/owner-a-r18-20261018/socket-resolver-handoff-addendum.json)
and [fixture plan](../artifacts/owner-a-r18-20261018/socket-native-emitted-fixture-plan.json)
record exact guards, ABI/flags/errors/ownership and meaningful controls. B23's
verified repairs must accompany resolver inputs in the next immutable capture.
Capture, generation, actual full-profile layout/ABI/D10 and self-pipe/unchanged
required-suite retry require SOURCE-READY, release and coordinated grants.
Common SOCKET source-only guard results are not asyncio acceptance. The separate
UTF-8 required skip remains unresolved, never waived. Full D10/P0–P6 remain.

## A17: hostname whole286 published; fresh B22 qualification required

The explicit separate900s grant completes strict/reuse/jobs2/both-form generation
with both exact immutable V13 tools. Receipt
[`20261018-045653-2ea433b1`](../artifacts/campaign/20261018-045653-2ea433b1/receipt.json)
passes all286 unchanged ordered C units, linkage, raw build, private semantic
context/postprocessing, processed build and both final publication builds:
298 commands exit0 in 569.133s. Only the separate test
profile selects HAVE_GETHOSTNAME=1. Common GETPID/STRFTIME, test-only fork/wait,
undefined WCSFTIME and explicit process rejection remain the accepted policy.

Both actual published PEs, their independent copies, matching portable PDB IDs
and all PDB document checksums bind to the exact compiled source bytes. Raw is
SHA256 `0792fac4f287cf66622f972b898f796fa93fff9206678f561288e1570d9d8852`, MVID `89b5295a-b16e-46e5-8cea-c1ea123fd340`;
processed is SHA256 `72e2bcc384ab892afc952f72ab673e2c8a070aca653e7aebf9647d52a240efc8`, MVID `2b9d0999-3d70-4185-8dac-232e081f7b2a`.
[Publication evidence](../artifacts/owner-a-r17-20261018/whole286-actual-publication.json)
records every ordered source/option/D-U/object, all263 upstream Include headers,
uniform complete layout-object signatures (158 interpreter,
158 runtime), and separately retained incomplete declarations.
These signatures do not execute product sizeof/offset or establish ABI equality.
All75 registrations retain73 non-NULL initializers and two special NULL entries.
Each form has2329 assets; their sole byte delta is the actual target sysconfig
HAVE_GETHOSTNAME=1 addition. Original GETPID-only bytes remain independently held.

[Compiled readiness](../artifacts/owner-a-r17-20261018/new286-compiled-readiness.json)
uses the existing immutable metadata reader and a private explicit hostname-stage
adapter of the existing read-only feature audit. Both forms contain73 nonempty
initializer bodies, the real source-owned socket_gethostname/METH_NOARGS binding,
audit/GIL/error/decoding calls and shared typed gethostname→Dns.GetHostName IL.
GETPID's actual source/service edges are checked anew. All24 wrong-identity,
deleted/empty initializer, deleted hostname/service/DNS/pointer and GETPID controls
reject. Raw output matches all76 current runtime sections. These are static
readiness observations; no product execution result is inferred from metadata.

Before generation, A17 rehashes54363 current original/copy pairs and39804 B21
historical copies, recording A16's changed historical originals explicitly.
The extended backup preserves60133 files/5169734155 bytes and verifies
five canonical/actual PE restore pairs. A16's16 actual service bindings, full
failed B21 framework consumers/resume report/logs and A14's deferred drivers
are retained without proof retargeting. Compiler318/runtime164/postprocessor15,
root/V13 tools, embedded193, HEAD/index and25 unrelated diffs remain preserved.

Fresh B22 must independently prove the new profile and each actual consumer/ILC
input: sourceAST/PE/MVID/IL/RVA/cache-def-use/pins/concrete GC lifetime closure,
native/product layout and ABI, then the early actual upstream retry. B21's
GETPID-only proofs remain scoped to their original bytes. All4867 required
assertions are still UNRUN; no case journal or repaired import pass is claimed.
Full warm data/allocation/GC/static identity and interpreter/thread/lifecycle/
owning/rooted/trimmed/delivery/audit/upstream four-cell gates remain open.
Embedded193 stays oldV11; A14's new193/currentV13 D10 plan is separately ungranted.
Production P6/current-runtime/provider/reproduction work remains deferred.
P0 stays in progress and P1–P6 open. The
[A17 handoff](../artifacts/owner-a-r17-20261018/a17-handoff.json) supplies the actual
command, preservation, prepared B22 controls and genuine RELEASE-ALL evidence.

## A16 live test-only hostname policy; actual products remain unchanged

The exact A15 candidate is installed: only upstream-tests-static gains
HAVE_GETHOSTNAME=1. Common GETPID/STRFTIME and the existing fork/wait/status
contract stay unchanged; embedded has no selected socket unit and its header
and188 assets remain identical. All units now reject selected/unselected
hostname D/U overrides, split/glued spellings, zero values and header divergence.
All31 policy tests pass; the retained old resolver fails the new contract.

Source-only preparation keeps193/286 ordered source bytes and46/75 registrations;
test2329 assets change only by sysconfig HAVE_GETHOSTNAME=1. Native and object
before/after layouts agree in both profiles. Actual unchanged socketmodule emits
select its existing hand-written wrapper/documentation/method-table branch.
Eight hostname and eight prior-service raw/processed JIT/AOT cells pass fresh
typed controls. These measurements do not qualify regenerated products.

No HAVE_UNAME/GETPPID/GETGROUPS/KILL/SETHOSTNAME/GETADDRINFO/WCSFTIME selection,
shared header/runtime/compiler change, native configure reuse or process support
is added. Current286 is still GETPID-only;193 is still V11. New whole286 needs
its own grant and fresh actual-input D10/layout/ABI/runtime/upstream proof.
See [A16 controls](../artifacts/owner-a-r16-20261018/hostname-policy-native-emitted-controls.json).

## A15 private hostname selection; live policy remains unchanged

The candidate adds `HAVE_GETHOSTNAME=1` only to `upstream-tests-static` because
that profile alone selects `_socket` for unchanged required support imports.
Embedded has no socket unit/builtin and gains no feature/header/sysconfig change.
The common GETPID/STRFTIME choices and test-only fork/wait/status contract remain.
All selected and unselected units reject common/unit split/glued D/U overrides;
the uniform resolver guard also rejects defined0, which would select #ifdef code.
Exact header agreement remains mandatory. These new tests are prepared, UNRUN.

Actual source effects are socket_gethostname, its documentation bytes and the
METH_NOARGS method entry. The wrapper is hand-written, not a Clinic method. It
audits socket.gethostname, releases/reacquires the GIL around the existing shared
service with char[1024]/capacity1023, maps failure through set_error, sets the
final NUL and decodes with PyUnicode_DecodeFSDefault. The C unsigned-long length
matches the existing CLR ulong formal; B21 proves that service's real host query,
truncation/NUL/sentinels/wide capacity/errno in its stated native/four-cell scope.
No new tool/resource capture is indicated by current evidence.

No core/header/layout branch uses this feature. Fresh native target-header and
emitted object comparisons remain required; source absence is not an executed
layout pass. Only test sysconfig is expected to add HAVE_GETHOSTNAME=1; actual
per-path asset deltas and old bytes must be measured/preserved. The resolver and
effective policy bindings change even where embedded header bytes do not.
No HAVE_UNAME/GETPPID/GETGROUPS/KILL/SETHOSTNAME/GETADDRINFO/WCSFTIME selection,
native configure reuse, upstream edit or product-scope expansion is included.
See [source classification](../artifacts/owner-a-r15-20261018/hostname-feature-classification.json).
The narrow480-second request is ungranted; whole286 and193 publications require
their own grants and fresh actual source/PE/D10/ABI/runtime qualification.

## A13: whole286 GETPID published; fresh B21 qualification required

The separately authorized strict/reuse/jobs2/both-form generation uses both exact
immutable V13 compiler and postprocessor paths. Receipt
[`20261018-035815-1ef722f0`](../artifacts/campaign/20261018-035815-1ef722f0/receipt.json)
passes all286 ordered unchanged C units, linkage, raw/context/postprocessing,
processed and final publication builds: 298 commands exit0 in
565.837s. Common HAVE_GETPID=1 is now compiled into this separate
test profile; common strftime and test-only fork/wait policy remain accepted,
WCSFTIME stays undefined and process creation remains rejected. Embedded193
remains unchanged on V11; no new193 or P6 generation ran.

The actual raw PE is SHA256 `70399489db6d39825cd9baa60b5588b11d726cbc1505ba2c53b6796e2f1eed82`, MVID `9a10b646-e148-427e-9afe-f1011fb89312`;
processed is SHA256 `0563478af7c36b99fc68dc748003d64a41f45d6fabd5d551bbdd96e9272fd644`, MVID `e5bca8b1-c282-4ed0-a0e2-443138c08cde`.
[Publication evidence](../artifacts/owner-a-r13-20261018/whole286-actual-publication.json)
binds every actual source/ordered D/U/object, all263 upstream Include headers,
uniform complete interpreter/runtime definitions and separately retained incomplete
declarations. All75 registrations retain73 non-NULL initializers and the two
special NULL entries. Both forms contain2329 verified assets. Only the target
sysconfig bytes change: HAVE_GETPID is added with value1; original bytes and
hashes remain independently copied, without a same-all-assets claim.

[Compiled readiness](../artifacts/owner-a-r13-20261018/new286-compiled-readiness.json)
uses an existing immutable metadata reader on both actual published PEs. It
checks75 registrations/73 nonempty initializer bodies, exact private-source
owners of os_getpid and os_getpid_impl, and actual IL call edges through the
upstream PyLong_FromPid macro's PyLong_FromLong binding and the unchanged
getpid/Environment.ProcessId service. Ten wrong-identity/missing-initializer/
missing-getpid controls reject. These are actual compiled readiness observations;
new product sizeof/offset execution, full D10 and ABI/interpreter qualification
remain UNRUN. Field presence, build success and old B20 proofs supply no pass.

Preflight rehashes 66580 existing independent backup files and
adds 17650 files, preserving 7621797885 bytes in total.
All four prior canonical PE SHA/MVID restoration pairs verify. B20's historical
seal log was captured empty before its own final output; its later849 bytes are
bound separately, while original historical bytes/proof paths remain untouched.
Compiler318/runtime164, root/V13 tools, unrelated dirty work, HEAD and index
are preserved. A13's handoff returns RELEASE-ALL with no active/queued builds
and independent shared/native/run-lock checks. Fresh B21 must bind each actual
new consumer/ILC PE and execute native/layout/ABI controls and the early actual
upstream retry. All4867 required assertions remain unrun; full four-cell D10,
runtime/thread/lifecycle/owning/rooted/trimmed/delivery/audit/upstream gates,
separate193 after B's blocker assessment and production P6 remain required.
P0 stays in progress; P1-P6 remain open. See the
[A13 handoff](../artifacts/owner-a-r13-20261018/a13-handoff.json).

## A12: common getpid policy proven; regeneration and upstream retry pending

B20's actual V13 upstream receipt `20261018-033841-b2749980` discovers eight of
23 seeds/1,191 IDs, then fails15 imports at `test.support.os_helper:22` because
`os.getpid` is absent. No assertions execute; all4,867 required IDs remain unrun.
The original report/tracebacks/nonzero exits are preserved. B20 independently
proves the existing getpid service natively and in direct/object JIT/AOT:54
checks, two genuine workers and five wrong-PID rejections. No runtime repair
or compiler defect is inferred.

After B20's verified RELEASE-ALL and a separate eight-minute coordinator grant,
A12 enables only common `HAVE_GETPID=1` and protects it uniformly against common
and per-unit D/U overrides, defined-zero flags and header disagreement. The
existing common formatter/test-only fork+wait policy stays intact; no getppid,
getgroups, uname, kill, wide formatter or process-creation capability is enabled.
The actual selected effects are os.getpid/Clinic, PID in random's existing
fallback seed and fork warning; no upstream algorithm or required case changes.

[Actual controls](../artifacts/owner-a-r12-20261018/getpid-policy-native-emitted-controls.json)
pass46 commands:23 pure policy tests, exact source-only193/286 preparation,
46/75 builtins and188/2329 assets. Only the test sysconfig GETPID bytes change.
All eight native target-header layout values agree before/after per profile;
emitted interpreter/runtime signatures agree. The unchanged posix/random units
emit before and after with the actual newly selected definitions/calls. All
8 profile × raw/processed × JIT/AOT service cells pass typed getpid/errno,
strftime/wait/status and LP64/unsigned16 controls. Actual PEs/MVIDs, ILC responses,
all references/roots/native objects/executables have independent byte copies;
20 identity/reference negative controls reject. Both raw service forms match
all76 current runtime sections. These are header/service/source controls, not
regenerated full-product ABI, D10 or interpreter qualification.

A read-only AST audit follows195 syntactic modules from the23 seeds and records
161 import-time bindings, keeping9,267 deferred functions and optional paths
separate. B20's actual os/time callable inventory identifies getpid as the sole
missing unconditional callable in that inventory. Dynamic imports/calls and
actual discovery remain required; optional absences are not enabled wholesale.
The original conservative AST report, stale-V11 source-preparation comparison
and recursive optimized-source inspection error are retained with corrections.

Before edits,66,580 independent full-byte copies preserve6,519,461,666 bytes of
current193/286/source/assets/bin/obj, private campaign inputs, all receipts,
failed actual B18/B20 consumers, B19/B20 proofs and tool closures. Every original
and restored byte and all four canonical PE SHA/MVID pairs verify. Compiler318,
runtime164, root/V13 tools, canonical products and unrelated dirty work stay
unchanged. Canonical193 is still its V11 receipt; canonical286 is still A11's
V13 receipt without GETPID. Both need separately granted regeneration and fresh
actual input proofs. The prepared old193 command plan is deferred behind the
actual bootstrap blocker. P0 remains in progress; P1–P6 are open.

## A11: consistent policy and new V13 whole286; independent qualification open

After B19's verified SAME-C V13 proof and the coordinator's explicit bounded
slot, the exact accepted policy now enables common `HAVE_STRFTIME=1` and only
`upstream-tests-static` adds `HAVE_WAITPID=1`/`HAVE_SYS_WAIT_H=1`. The existing
test-only `HAVE_FORK=1` remains uniform. `HAVE_WCSFTIME` stays undefined. Wait
and waitpid still reject with -1/EPERM and preserve status; no process backend,
required source, frozen ID, decorator or exclusion changed.

[A11 controls](../artifacts/owner-a-r11-20261018/policy-native-emitted-controls.json)
bind fresh source-only preparation of all ordered193/286 units,46/75 builtins
and188/2329 assets to the exact policy/header/every-unit D/U/sysconfig. All8
import-policy and7 fork-policy tests pass, including common/unit split/glued
overrides, absent services, header mismatch, partial wait and defined-zero flags.
Three actual unchanged upstream units (time, posix and _testcapi) emit with V13;
before-policy controls omit the newly selected formatter/wait/status definitions,
and after-controls retain the actual method-table/Clinic bindings.

Actual native controls use target headers/unsigned16 wchar/DotCC identity. All8
layout observations agree before/after within each profile: embedded interpreter
194792/runtime283056, test interpreter194816/runtime283080, main offset88264;
test callback offsets10464/10472/10480. Emitted interpreter/runtime object
signatures also agree. These are fresh native/header/object observations;
**compiled regenerated product layout is still unrun**. Standalone header-only
managed build failed on88 unresolved real core symbols; the unlinked-header
attempt is retained and no implementation was replaced to make it build.

Separate actual shared-service probes pass all8 raw/processed × JIT/AOT ×
profile cells, with exact formatter19, LP64/unsigned16 widths, typed function
pointers, wait decoders, EPERM and untouched status. Each actual PE/MVID and
ILC response/reference/root/native-object/executable binding is independently
copied;20 wrong-MVID/bytes/empty-reference controls reject. Both raw emissions
match all76 current runtime sections. The original source-scan rejection of
upstream platform-only undef HAVE_WCSFTIME and missing project-local restore
assets for postprocessing are retained as harness failures. The two inert wide
undefs never enable a service. Compiler318/runtime164/V13 tools are unchanged.

The separately granted whole286 strict/reuse/jobs2/both-form publication passes
in570.8s, receipt `20261018-031856-ab7e6848` (SHA256
`7de2f7b1e05ef232ed30f4aab1524c08768ea12c58fe6780a82d8690a746c85b`).
All286 ordered actual C bytes and75 registrations remain unchanged; both forms
have2329 assets, with only target sysconfig bytes changed from the old policy.
All emission/link/raw/context/postprocess/processed/final publication builds pass.
Both exact V13 tools are selected explicitly. The shared runner now supports
`DOTCC_POSTPROCESSOR` alongside `DOTCC_COMPILER`;47 framework controls pass,
including3 selected-tool controls (the original runner fails2 of those3).
No root tool or runtime/compiler source is replaced or rebuilt.

Fresh read-only inspection binds both actual published PE SHA/MVIDs, all286
ordered options/sources,263 unchanged upstream headers and158 uniform complete
interpreter/runtime definitions per form. The118 interpreter and1 runtime
incomplete tags are preserved as declarations, without layout claims. All75
compiled registrations are ready (73 non-NULL initializer implementations), and
10 newly selected formatter/wait/status methods have actual nonempty IL bound to
their exact source/object owners. Eight wrong-identity/missing-method controls
reject. [New publication](../artifacts/owner-a-r11-20261018/whole286-actual-publication.json)
and [compiled readiness](../artifacts/owner-a-r11-20261018/new286-compiled-readiness.json)
are artifact inspections, not interpreter or D10 qualification.

Before publication,43384 independent full-byte copies preserve4266961503 bytes
of current193/286/source/assets/PE/receipts, failed B18 consumer/JIT/ILC trees,
B19 evidence/inputs and exact V13/root closures. Every source/restored file and
all4 original/restored canonical PE SHA/MVID pairs are independently verified.
Old proof paths are not silently retargeted. A9/A10 historical archives remain.

Embedded193 still identifies its unchanged V11 receipt/bytes and needs a separate
new193 grant. Actual new-profile sizeof/offset execution, fresh complete D10 and
every consumer-input proof, early actual upstream retry, full interpreter/ABI/
thread/lifecycle/rooted/delivery/audit matrices and downstream P6 remain required.
All4867 required upstream IDs remain unexecuted; P0 stays in progress, P1–P6 open.
No B-owned edit, source-case repair, required-test execution or P6 run occurred.


`embedded-gil` retains release layouts, pymalloc, cyclic GC, real pthread/TLS,
CPython GIL scheduling and switch dispatch. No `HAVE_PTHREAD_STUBS`,
`Py_GIL_DISABLED`, `WITH_MIMALLOC`, experimental JIT, dynamic extension loading
or native perf executable trampoline is advertised. These choices do not yet
prove translated execution or close P1.

DotCC currently rejects `-std=gnu11`; the recipe uses `-std=c11`, the tool's
supported C11 mode, and exercises required extensions on actual upstream inputs.
This difference is recorded in `config/profile.json`; no source feature is erased
to hide the dialect limitation.

`config/pyconfig-policy.json` defines the target configuration explicitly from
this fork's supplied C headers and service declarations, without copying native
Linux configure capabilities. ABI: LP64, little endian, pointers/long 8 bytes,
int 4, unsigned wchar 2; long double uses DotCC's 8-byte model. Shared pthread
handles follow DotCC's ABI. Configuration observations still need actual-header
emission/layout/operation probes; ordinary native Linux wchar and long double
must not be falsely compared as identical layouts.

Native generator configure options live in `config/profile.json`. It is GIL
backed with pymalloc and switch dispatch, without mimalloc/shared libpython or
ensurepip. Its selected static modules match the setup inventory; normal Linux
native dynamic loading remains available to the native tool, outside the product.

Common flags and ordered includes are recorded in translation receipts:
configuration; staged pin Include; staged internal headers; staged source root;
then DotCC supplied C headers. Product C units get `Py_BUILD_CORE`; module units receive
`Py_BUILD_CORE_BUILTIN`. Reviewed helper rows retain their own ordered core,
builtin, ordinary-extension or limited-API flags, including explicit `-U`
operations. The helper auditor checks those roles against actual emitted inputs. Getpath gets explicit prefix/version/platlibdir
constants, and SHA2 gets its upstream HACL include directory. Each manifest row
records per-unit flags. No system Python/system C include directory is supplied.

Shared `link_flags("CPython", "Managed.Interpreters")` selects the existing object
format/linker, 256 KiB splitting, literal pooling and inline deduplication.
Assembly: `TranslatedCPython`. Generated projects link B's original Host sources;
the facade/consumer use `CPythonProject`. Receipt arguments and unit paths are the
source of truth for each attempt, including failed ones.

The round-2 source set adds the required Unicode-name implementation through
ordinary upstream static-module configuration. `config/embedding.json` adds B's
completed authored Host C unit and real `Py_INCREF`, `Py_DECREF`, and
`PyUnicode_4BYTE_KIND` link exports. All original upstream units remain. The Host
C source is emitted from its authored path with the ordinary target pyconfig;
it is never linked as a native product backend. Limited generated output confirms
nested `_object`, refcount signatures and enum value 4; a complete library/ABI
build and actual interpreter execution remain blocked.


Recovery round 4 selects explicit portable HACL byte-order predefines in
`profile.json`. Real SHA224/256/384/512 native and emitted JIT vectors and
unaligned endian helpers pass; runtime timing properties remain unclaimed.
The target pyconfig now supplies `PLATFORM="linux"`,
`MULTIARCH="dotcc-linux-x64-wchar16"`, `ABIFLAGS=""`. Without PLATFORM the real
upstream getplatform.c returns "unknown"; this metadata describes the selected
target rather than inheriting native configure's ABI identity. Actual runtime
sys/config observations remain required.

After B's real times/sysconf service passed native/JIT/AOT controls, target
`HAVE_SYS_TIMES_H`/`HAVE_SYSCONF` select the proper shared header and tick query.
No private libc or source workaround is present. The next whole manifest
retests actual fileutils/posix integration and all original units.

`upstream-tests-static` retains all **193** actual product units/Host and adds
a reviewed profile-specific `HAVE_FORK=1` selection. It then extends the source
table with **93** deduplicated real helper sources (**94** declared helper
sources include the already-required unicodedata), for **286** selected units.
Its separately generated upstream config.c has **75** entries. The recipe uses
B's reviewed same-pin Setup rows, without invoking or depending on the native
helper build. Separate test-only content contains pin Lib/test support/data and
stdlib dependencies plus generated target sysconfig values. Structural staging
asserts **2,329 test assets**, while shipped embedded-gil remains **186** assets.
A8 strict receipt `20261018-013343-0e127aa3` publishes both actual286 forms;
source/role/75-initializer and actual-control evidence is recorded separately.
This does not qualify the frozen upstream suite. No native pyconfig/sysconfig platform values are copied.

## Restart round 5 integration

The target uses `_Py_NO_RETURN=_Noreturn`, the compiler's supported C11 marker,
and `_PYTHONFRAMEWORK=""` for this Linux static product. Actual fatal calls
retain their effects and explicit nonreturning control-flow lowering; no dummy
return is appended.

B's R5 native/JIT/AOT controls justify clock resolution, UTF-16 multibyte
conversion, copysign/erf/erfc, utime, passwd fields and real identity-query
feature macros. Public reentrant passwd APIs remain unadvertised. Locale
conversion is C/POSIX ASCII or explicit C.UTF-8, never glibc wchar4.
`HAVE_LANGINFO_H` requires the actual `_locale` DAY/ABDAY/MON/ABMON and
format/AM/PM table as well as CODESET. B's complete shared C/POSIX/C.UTF-8
54-query service now supplies this surface, with native/JIT/AOT comparisons;
whole receipt `20261017-193052-824be3ee` builds both forms with it. The earlier
43-diagnostic raw failure remains in `20261017-191623-134a3c10`. No source branch
was disabled to hide that failure. No target config is copied from native helper
metadata. Actual CPython runtime configuration remains unobserved because its
static initializer currently fails before startup.

## Profile-specific fork hook selection

`pyconfig.py` resolves the common policy plus the selected profile overlay before
any source/helper preparation. The same effective object generates `pyconfig.h`,
sysconfig and the receipt binding. `HAVE_FORK` is a source/layout selection for
the test profile's actual lifecycle algorithms; process creation is rejected by
the shared runtime. It exposes `os.fork`, `os.register_at_fork`, thread-lock
reinitialization methods and conditional helper branches. Required IDs and
exclusions do not change. See [the exact contract and controls](fork-hook-profile.md).

A unit cannot override the selected layout feature with either spelling of
`-D`/`-U`. All286 effective ordered options, source bytes, header hash, source
guards, layout-sensitive object signatures, target sysconfig and actual compiled
initializers must agree before frozen tests. The A18 common SOCKET addition changes the current embedded target header;
embedded module/assets scope stays fixed. Normal native pthread layouts and GCC-specific packing are
not DotCC layout oracles; use actual target headers and compiler identity.


## A23 CPY-P004: all286 emit/link; raw build lacks SOMAXCONN

All61 framework controls (including14 new fake/private-lock cases),26 adapter
controls and41 policy controls pass. The narrow opt-in command hook is live;
native cache-state locks avoid nested acquisition. Actual wrapped argv/PIDs,
timeouts and errors retain the existing process-tree cleanup protocol.

Authorized V15 strict/reuse/jobs2/both-form receipt `20261018-072953-2b81c724`
finishes in486.990s: all286 unchanged ordered C units emit and link;
291 commands pass. Command292, raw-build, exits1 with two CS0103 diagnostics at
`CPython.00078.cs:95`: `SOMAXCONN` is unresolved in unchanged `sock_listen`.
The selected shared `sys/socket.h` does not define it. Upstream's separate
module-constant fallback5 does not define the unguarded listen-default expression.
This is a shared socket-header constant/binding gap; no generic compiler/runtime
cause is established. The source-only reduction is UNRUN. Exact diagnostics,
source/object/generated excerpts and a next-owner regression brief are in the
[first blocker](../artifacts/owner-a-r23-20261018/first-actual-blocker.json) and
[A23 handoff](../artifacts/owner-a-r23-20261018/a23-handoff.json). No generated
repair, source cut, compiler/runtime/header edit or automatic retry occurs.

Static bindings retain75 registrations (73 non-NULL/two legitimate special NULL),
2329 staged assets and all77 current raw runtime sections/84 source resources.
Both canonical products/pointers and four current PE/MVID identities are unchanged;
no new product PE, postprocessing, processed build or publication exists. The
director owns authorized redundant-intermediate cleanup. Unique failed source,
objects, generated C#/project and logs remain at this receipt's work/artifact roots;
deleted redundant held copies are not recreated or treated as preservation failures.

Next: reviewed header contract and native/emitted regression controls in a fresh
slot, normal immutable recapture if the header changes, SAME-C proof and a new
whole286 retry. Fresh B27 consumer/ABI/layout proof and EARLY actual required-suite
retry follow a successfully published profile. B22 remains1999 executed/1995
passed/four nonpasses/2868 unrun. Expanded2026-10-18 D10 typed-table and every
supported primitive-class requirement is OPEN authorized implementation work for
the next compiler round. Historical byte-only proofs cannot close it. P0 remains
in progress; P1–P6/new193/four-cell/interpreter/thread/GC/lifecycle/owning/rooted/
trimmed/delivery/audit/current-runtime production P6 qualification remains open.
