# Full implementation campaign

This ledger tracks the user-authorized coordinator and implementation workers.
The broader design remains [the backend plan](dot-gen-be-plan.md). On
2026-10-03 the user prioritized real-project translation and verification,
starting with SQLite, and deferred exhaustive C++/libc++ coverage. Project
failures now drive new implementation. A passing project is not completion of
the broader plan. Preserve generated sources and diagnostic evidence; root may
clean inactive compiled temporaries under the later explicit user authorization.

## Ownership and current work

| Owner | Work | State |
| --- | --- | --- |
| Root | User-facing review and source-correspondence acceptance assistance | Supplied readability fixtures, source layout helper and direct named SQLite consumer |
| Coordinator | Control/declarations, integration, driver grouping/navigation, builds/commits and project evidence | Readability14, control10, layout6 and revised SQLite20 accepted; next source-fidelity increment under test |
| Core language worker | Source identifiers/types and collision mappings | Shadow-aware names14, GNU statement10 and source-typed enums12 accepted |
| Expression worker | Direct expressions, localized evaluation adjustments and named bitfield properties |52 unique focused checks accepted; comma loops and pointer scaling verified |

Compiler preparation/build and Git mutations are serialized through the
coordinator. Canonical emitter sources stay in `backend/`. Workers use disjoint
files; generated overlays are refreshed only at integration boundaries.

## Completion gates

- [ ] Owned typed representation, verifier, source diagnostics and complete IL coverage.
- [ ] Versioned portable target ABI, predefined macros and headers independent of translation host.
- [ ] Complete supported C++ language, member pointers, initializer forms, atomics and extended types.
- [ ] Complete EH runtime: function-try-blocks, exception_ptr, nested exceptions, uncaught counts, matching and termination.
- [ ] NativeMemory ownership for short-lived exception/catch storage; allocation efficiency audit.
- [ ] Portable variadic ABI and compiler-assisted setjmp/longjmp.
- [ ] External function/data imports, canonical addresses and native callback adapters.
- [ ] Multi-TU semantic assembly, ODR/type identities, storage/relocations, startup and cross-module exceptions/RTTI.
- [ ] Complete libc state and services: allocation, strings, formatting, stdio, files, errno, locale, environment, time, threads, math/fenv.
- [ ] Pinned musl feasibility slice with measured adaptation and explicit provider decision.
- [ ] Pinned libc++ port including out-of-line code and all planned library families.
- [ ] Independent owner state, TLS/thread exit and disposal across every runtime subsystem.
- [ ] Windows/Linux/macOS x64/Arm64 JIT and NativeAOT validation, runtime platform selection.
- [ ] Efficiency/scalability measurements, reproducible manifests and final supported-feature audit.
- [x] Pre-readability SQLite baseline: six full-profile native/reference/JIT/NativeAOT corpora and two independent-owner checks (20 project checks).
- [ ] Current priority: source names throughout bodies/types, structured statements/declarations, direct expressions/named bitfields, source navigation/splitting, and fresh SQLite JIT/NativeAOT/direct named API acceptance. Exhaustive unrelated language/library gates remain deferred.

## Verified milestones

Baseline commit `a63ae45c` embeds all 111061 original upstream tracked files and
the backend in parent Git. Original history is retained under ignored
`artifacts/embedding/upstream-history.bundle`; no nested Git remains.

Baseline evidence predates this campaign: 175 full regression checks plus ten
focused checks, recorded in `evidence/dot-gen-backend.json`. Those results prove
the documented subset only. New milestone results and commits will be recorded
here with retained artifact paths.

First integration milestone `f98f512fb` (portable target, imports, multi-TU and function-try blocks):

- `artifacts/campaign/libc-imports-final-20261002-214539/results.json`: seven
  checks, 24 C-oracle cases, static/instance JIT/NativeAOT, header and ABI probes.
- `artifacts/campaign/multitu-acceptance-001/results.json`: ten checks, C and
  C++ semantic merging, GCC/Clang C oracle, four NativeAOT configurations.
- `artifacts/campaign/function-try-shared-20261002-220203/results.json`: nine
  checks, 32 ordinary traces and three fatal traces in static/instance JIT/AOT.
- `artifacts/campaign/portable-target-acceptance-001/results.json`: nine profile
  checks covering macros, scalar/object layout, byte order and binary80.
- `artifacts/campaign/milestone-runtime-portable-20261002/results.json`: all
  209 combined regression checks pass, including NativeAOT.

First-pass exception search now uses false-returning bookkeeping filters. An
initial duplicated cleanup-switch implementation caused pathological NativeAOT
compile times; both interrupted runs and logs are retained. A single cold
per-routine decoder preserves semantics and reduces the scalar exception fixture
from over 11000 lines for its main routine alone to 2960 total generated lines.
The focused scalar exception suite passes all nine checks in 111.5 seconds.

Before the first integration commit, targeted checks additionally cover the
default portable profile, disabled implicit host headers and EDG's existing
ignore-variable predicate for dependent template entities. The full runtime
profile remains experimental; no Windows/macOS/Arm64 execution claim is made.

## Initial increment

Implement strict signature-checked `--libc dotcc` imports, including callable
external addresses and addressable errno. Reuse the generated NativeMemory heap
for C allocation and C++ new/delete. Runtime services are BCL-backed; pinning is
reserved for owner/thread errno storage, globals, TLS and existing immutable
storage. Add constructor/destructor/ordinary function-try-block differential
tests using the unchanged EDG C backend as reference.

Final first-increment supplements pass: 17 runtime/header checks at
`artifacts/campaign/default-portable-runtime/results.json`, two include-path
checks at `artifacts/campaign/portable-includes-001/results.json`, and five
variable-template checks at `artifacts/campaign/template-entities-acceptance-003/results.json`.
C++ whole-program translation now requests EDG `--instantiate used`, and the
portable profile is the driver default. See
`evidence/milestone-runtime-portable.json` for the source hashes and remaining gates.

## Second increment: native exception storage, varargs, files and atomics

The second increment passes the combined regression suite from an isolated
source/compiler snapshot at `artifacts/campaign/milestone-native-snapshot`.
Commit `5cc5d52d1` records 250 combined checks and 594 successful commands;
`evidence/milestone-native-runtime.json` hashes the exact committed inputs.
The initial run preserved 555 successful command results before a strict
nullable-field warning; the corrected field is covered by the successful
complete rerun. Snapshot hashes, command logs, generated projects and all
failure evidence are retained. Focused evidence, all synthetic:

- `artifacts/campaign/exception-storage-20261002-223655/results.json`: five
  checks with GCC/Clang C-oracle agreement and static/instance JIT/NativeAOT.
  Payloads and catch adjustment cells use native memory. Explicit carrier
  disposal runs translated destructors once; finalization releases bytes only.
  Live carriers retain owners. Shutdown preserves storage while deferred
  carrier cleanup runs, including cross-thread disposal and callback reentry.
- `artifacts/campaign/libc-files-integrated-20261002-224609/results.json`:
  seven checks, 25 strict FILE imports, 11 native traces, owner-specific
  addressable standard streams and fpos, buffering, errors and concurrent
  append. Linux execution is verified; Windows/macOS branches still require
  execution on those systems.
- `artifacts/campaign/varargs-004/results.json`: ten C/C++ checks for explicit
  typed packs, default promotions, binary80/128-bit/aggregate arguments,
  va_copy/end, forwarding, recursion, indirect calls and EH. Static/instance
  JIT/NativeAOT agree with original EDG C. Packs exceeding 64 KiB use temporary
  NativeMemory; the test passes a 70 KiB pack. Arbitrary aggregate-size limits
  remain a separate unresolved representation gate.
- `artifacts/campaign/atomic-runtime-003`: isolated atomic helpers pass
  JIT/NativeAOT; `artifacts/campaign/atomic-builtins-003/results.json` adds
  twelve passing emitted C/C++ checks, including concurrent calls.

Selected musl printf source, license, provenance, replayable adaptation and
private sink are retained under `backend/runtime/musl/printf`. Its 109 native
synthetic cases pass GCC/Clang, but translated C# formatter acceptance is not
yet established. New bit-level floating helpers have separate native-vector
evidence; errno/fenv and directed arithmetic are not inferred from that proof.

Next integrated gates are member-pointer variant ABI and extended pointer EH,
formatter dependencies/translation, and libc++ component bring-up.
The complete campaign checklist above remains open.

## Third increment: member pointers, language modes and library foundations

Commit `043b1cca8` records the third increment: 332 combined regression checks and 816 recorded
commands from `artifacts/campaign/milestone-member-library-snapshot`. The source
and compiler snapshot remains independent of subsequent work:

- Version-two target profile selects EDG's supported integer-tagged member
  function pointer representation, without assumptions about CLR pointer bits.
  The 48-case descriptor fixture passes five checks, including static/instance
  JIT/NativeAOT and C++17 sequencing. Forty extended pointer exception cases
  cover exact member/function/array/noexcept identities, adjusted nullptr
  buffers, rethrows and native-storage cleanup.
- Needed entity and needed definition predicates follow the original C backend,
  avoiding dependent pack placeholders and unused header-inline bodies.
- Runtime constant-evaluation queries and launder follow the C backend; ten
  constexpr/template-pack checks pass. New explicit language modes and
  constexpr/consteval statement selection pass 21 focused checks, including unsigned negation and C17/C23/C++20/23/26 modes.
- Libc++18 acquisition is reproducible and its first synthetic traits consumer
  passes JIT/NativeAOT. Other seeds expose concrete remaining library/API gaps.
- Selected musl formatting passes nine emitted-program checks (25 original EDG C cases; C/C++ × static/instance × JIT/NativeAOT), at `artifacts/campaign/musl-printf-translated-20261002-235819/results.json`. Eleven malformed import contracts are rejected before C# emission.
- BCL thread services pass six generated/native checks covering retained worker leases, global-destructor thread creation, TLS/key cleanup order and independent owners. A supplementary four JIT/NativeAOT checks verify first-pass fatal-thread termination.
- Aggregate-prvalue caller-frame materialization passes 16 differential cases under static/instance JIT/NativeAOT, including nested/conditional/comma returns, simultaneous temporaries, loop reuse, forced GC and cleanup.
- Member-pointer RTTI adds five passing checks at `artifacts/campaign/member-typeinfo-20261002-232630/results.json`; the semantic descriptor retains member-class/cv/noexcept identities rather than confusing its physical record layout with class RTTI.

The exact milestone inputs are hashed in `evidence/milestone-member-library.json`.
Main arguments, additional math, compact large-array storage and the libc++
exception-pointer adapter are subsequent work and are not included in this snapshot.
New standalone runtime or native proofs do not substitute for emitted-program acceptance.

The third run exposed a harness retention limitation after acceptance: three
new modules reused `static`/`instance` project directories. Driver source
history and command logs remain available, but some build outputs and oracle
paths could be overwritten by later modules. Every module now receives its
own fresh suite directory; the next full run retains complete independent
module artifacts. This does not alter the recorded successful executions.

## Fourth increment: numeric libraries, storage and exception handles

Commit `10ea8248e` records the immutable `milestone-numeric-owners-snapshot`,
which passes 439 checks and 1,069
recorded commands, including static/instance JIT and Linux-x64 NativeAOT.
Every suite retains separate artifacts, and completed checks are checkpointed.
The earlier `milestone-numeric-arrays-snapshot` failure remains preserved:
known-token type-info destruction had reached a runtime guard instead of its
required translation rejection. The field bridge now preserves that rejection;
the original negative tests were not relaxed.

This increment includes main arguments; selected math and musl integer/floating
conversion; compact large/repeated arrays; anonymous records; public type-info
fields; residual void/floating intrinsics; source diagnostics; bounded locale
objects, allocating formatting and POSIX strerror_r. `--libc musl` selects
thirteen pinned source translation units plus audited BCL services, recording
retained inputs and hashes. It remains selected libc coverage, not complete libc.

Libc++ local exception handles, make/nested exceptions, normal owner/global/TLS/
heap ownership, concurrent rethrows and explicit host disposal are included.
Foreign containing-region abandonment, uncaught counts and implicit termination
integration remain subsequent gates. The original libc++ new/memory/locale
source providers and production separate-TU linking are not accepted by the
component handle fixtures. Source diagnostics do not constitute an owned IR.

Exact inputs and evidence are recorded in `evidence/milestone-numeric-owners.json`.
Newer packed-bitfield optimization, foreign-region reference bookkeeping,
atomic waits, narrow classification/collation and BCL clocks have focused
results but are not part of this immutable snapshot. VLA lowering is undergoing
verification. Full library/API coverage, platform execution and performance
closure remain open; no real-world project acceptance has been run.

## Fifth increment: wide libc and remaining lifetime boundaries

The immutable `milestone-wide-lifetimes-snapshot-004` passes the complete
337-check JIT suite. It carries forward 51 completed checks from snapshot003
with identical compiler/runtime hashes, then reruns the incomplete class suite
and every subsequent suite. A fatal child exceeded the old 15-second test
budget; the same executable completed with the expected trace and termination
in 13.50 seconds during isolated reproduction. The continuation changes only
that timeout to 60 seconds. Earlier failed snapshots and all commands remain.

Two earlier integration findings are resolved: the EH-disabled test host is
regenerated when it replaces the normal compiler output, and optimized bitfield
loads/stores widen bytes before shifts/masks so strict C# warning checks pass.
No EDG lowering changes or diagnostic suppression were used.

This increment adds native VLA storage with scoped and exceptional cleanup,
partial C++ array destruction, explicit 64/256/4096-byte alignment, restrict
qualifier acceptance, retained nonfirst-union initializer designators, and
bytewise packed-field operations. Exception handles cover foreign containing
regions, balanced bookkeeping OOM, allocation-free TLS classification and
uncaught/current_exception behavior across catch construction and managed
handoff. Configured implicit termination remains the next increment.

The automatic musl provider now selects 49 pinned translation units, adding
wide classification/case/string/memory/collation algorithms. All 1,114,112
Unicode values match the pinned native oracle. BCL clocks, narrow classification,
known memory/string intrinsic aliases and shared atomic wait services have
focused acceptance. Source-backed libc++ atomic contention preserves the
original algorithm while sharing only its private synchronization table across
owners; application globals remain instanced.

The evidence index `evidence/milestone-wide-lifetimes.json` records 170 focused
checks, including targeted regressions and Linux-x64 NativeAOT. This is not a
second full NativeAOT run or 170 disjoint language features. Multibyte/calendar
providers, configured termination, dynamic exception specifications and the
owned-type graph prototype remain separate subsequent work. Full libc/libc++,
separate-TU assembly, performance closure and other platform executions remain
open. No real-world project acceptance was run.

## Sixth increment in progress: configured termination and portable library breadth

The next accepted component set adds configured owner-specific termination
(78 focused checks plus 50 targeted regressions), dynamic exception specification
positive and fatal cases (six positive, 28 fatal and five persistent-lifetime native/JIT/NativeAOT checks), and compatible array storage
(ten checks). The opt-in physical type graph passes six checks including
adversarial validation and byte-identical C# emission. It remains physical types
only; owned operations, lifetimes and separate-TU linking are still required.

Selected musl packaging now includes 97 translation units. Multibyte, wide
numeric, UTC calendar, wide calendar and standard utility algorithms each pass
nine component checks; automatic packaging passes eight checks at
`artifacts/campaign/musl-provider-calendar-algorithms-001/results.json`.
The source-backed sort retains musl's smoothsort algorithm and native allocation
policy; multibyte hidden states are owner/thread scoped. Calendar acceptance
covers UTC/civil arithmetic and formatting, not local zones or DST.

`checkpoint-calendar-termination-001` preserves canonical inputs and a separate
exact installed compiler overlay. New environment and scanf drafts in its
canonical tree are explicitly not accepted by that checkpoint. Local-time/environment integration
and scanf remain active work.
Full libc/libc++, architecture/platform execution, performance closure and the
owned program IR are not complete. All intermediates remain retained.

The sixth focused evidence index records 245 checks, including targeted
regressions; it is not a new complete-suite run. Exact staged inputs are saved
in `evidence/milestone-calendar-termination.json`. Environment/scanf work stays
outside this increment.

## Seventh increment in progress: environment, scanning and nonlocal control

Commit `d7083852a` records the sixth increment above (245 focused checks,
237 files). Subsequent source-backed environment and scanf components bring
automatic musl packaging to 105 translation units. The environment snapshot
runs at implementation startup priority0, before user constructors, and mutations
stay owner-local. Scanner acceptance retains original musl parsing, typed
variadic cursors, shared numeric lookahead and managed FILE locking. Independent
packaging matrices pass eight checks each for environment103 and scanner105.

Binary128 and binary16 basic language operations pass ten native/JIT/AOT checks
at `artifacts/campaign/extended-float-translated-001/results.json`. Software
quadruple arithmetic reuses audited dotcc code with direct destination rounding
and exact integer/binary80 adapters; BigInteger allocation/performance and full
floating-environment semantics remain pending. Foreign CLR boundaries pass
20 focused checks without backend changes.

Source setjmp ABI v1 (16-byte buffer) now passes 14 control-flow checks and six
C++ boundary checks, including static/instance JIT/AOT; activation dispatch skips
translated C++ catches, noexcept and dynamic-specification handling while CLR
finally blocks balance host resources. Target functions containing protected
regions or VLA allocation remain excluded in this first accepted slice. A
32-byte VLA checkpoint candidate is held for the next increment.
Wide FILE orientation, captured encoding and version2 16-byte fpos storage now
pass nine generated/native checks and sixteen strict ABI rejection checks;
byte FILE, formatter and scanner regressions also pass. Wscanf remains a
separate candidate. All intermediates
remain retained; no application-level acceptance has run.


Timezone integration now has nine component checks at
`artifacts/campaign/libc-timezone-translated-003/results.json`, plus standalone
runtime, exact patch replay and reproducible pinned IANA2026e bundle evidence.
The musl algorithms retain integer full-range civil/DST/POSIX-rule behavior;
BCL discovers default identifiers and reads explicit paths. Automatic packaging
now includes112 translation units and passes all eight packaging JIT/AOT checks
at `artifacts/campaign/musl-provider-timezone-001/results.json`.
Unmappable custom OS timezone defaults remain an explicit portability boundary.

The C backend requires `LOWER_CLASS_RVALUE_ADJUST=1`. Enabling that same
supported configuration materializes qualifier-adjusted class rvalues before
our emitter, addressing const aggregate copies and constexpr class arrays
observed in the synthetic libc++ chrono source closure. No EDG semantic source
was changed; five focused native/generated JIT/AOT checks pass at
`artifacts/campaign/class-rvalue-adjust-translated-001/results.json`.


The eighth working slice, outside the frozen seventh snapshot, has accepted
VLA-aware source-jump ABI v2: 22 standalone transport/checkpoint checks and28
translated/native checks at `setjmp-v2-runtime-20261003-031620` and
`setjmp-v2-source-20261003-031800`. The32-byte buffer restores only post-setjmp
allocations while preserving surviving arrays and rejects expired allocated
checkpoints. Managed cleanup remains balanced; broader protected targets and
undefined VM-only scope diagnostics remain explicit limits.

Empty zero-bound aggregate members now consume their empty initializer without
writing a nonexistent CLR field. Five differential checks pass at
`zero-bound-padding-translated-001`, covering globals/TLS/local copies and adjacent
storage. With this adaptation the explicit single-TU synthetic libc++ array
consumer and original core/locale/thread/future/chrono source closure passes
translation, strict C# build, JIT and NativeAOT in `libcxx-core-seeds-038`.
This does not establish separate-TU library assembly or whole-library coverage.


Commit `6de065787` records the seventh increment:154 files and202 focused checks.
Its exact frozen snapshot has now completed the full JIT suite:530 checks at
`artifacts/campaign/full-jit-streams-timezone-001/results.json`, indexed by
`evidence/full-jit-streams-timezone.json`. The broad run intentionally uses its
immutable pre-correction inputs; the committed DF80 byte-escape correction has
four independent original-musl/runtime and nine translated focused checks.
Subsequent VLA v2, zero-bound padding and wscanf changes are separately verified;
this is not a broad-run claim for those later bytes. All intermediates remain.


The eighth slice also has selected wide scanf (nine component, eight allocation
failure and eight signature checks), standard CPU clock (nine native/generated
checks), four C compatibility headers (nine), implicit C11 atomics (five),
public stdatomic (eight), and contained GNU statement expressions (ten positive,
four rejection checks). Selected musl packaging114 passes eight JIT/AOT checks.
The atomic adapter uses existing shared synchronization for typed/raw accesses;
bitwise compare-exchange preserves NaN payloads and signed zero, pointer updates
scale correctly, and warmed integer operations allocate no managed memory.

Actual C-linkage definitions for compiler-generated RTTI throw entries now bind
before the optional bootstrap, matching the C backend's external linker model.
Five focused differential checks pass at `compiler-throw-entries-translated-003`.
The synthetic libc++ source040 allocation, algorithms and container consumers
all pass translation, strict build, JIT and NativeAOT; their pinned single-TU
source recipe is being made reproducible. This remains component acceptance,
not full C++/libc++ completion or separate-TU library linking.

Commit `f985b8c17` records the eighth verified increment: 100 files and 152
focused checks indexed by `evidence/milestone-c11-library.json`. Its exact
snapshot excludes subsequent coroutine source emission and C assertion work.
The source-provider recipe is now pinned by ordered source hashes.

The next working slice has three passing libc++ owner checks in
`artifacts/campaign/libcxx-core-owners-001/results.json`: a native source oracle
and translated instance JIT/NativeAOT. Two owners retain independent vector,
string, tree/hash/shared ownership, `new_handler`, and per-owner/per-thread TLS;
one owner continues operating after the other is disposed. This remains an
explicit single-TU component, not production separate-TU library assembly.
Coroutine source emission has its first five-check counter acceptance at
`artifacts/campaign/coroutine-counter-source-20261003-040856/results.json`;
exact checkpoint bytes are retained separately. Exception protocol and broader
continuation forms are still being implemented and are not counted as complete.

The ninth working slice adds selected musl strptime and case-insensitive string
comparison: ten original/adapted native, sanitizer and generated C/C++
static/instance JIT/AOT checks pass at `libc-strptime-translated-002`. Source
adaptations bound musl's unknown-timezone scan and checked year arithmetic;
no EDG semantics changed. Three packaged source units bring the provider to117,
with eight automatic-packaging checks at `musl-provider-strptime-001` and two
exact source patch-replay checks at `string-case-strptime-patches-001`.

The authored C11 facade now brings selected packaging to118 units. Ten native
and generated thread checks, 13 strict ABI checks and six standalone/runtime
regression checks cover worker exit, copy-operand exception cleanup, nested
arrays, noexcept/catch boundaries, TLS and repeated TSS cleanup. Packaging118
passes eight checks at `musl-provider-c11-threads-001`. Main/foreign-host exit
and process/embedding termination remain separate implementation gates.

The ninth checkpoint freezes initial coroutine source emission (counter,
await-protocol and creation-failure traces), an explicit libc++ no-op handle
adaptation, C11 worker threads, assert, strptime/string comparison and libc++
owner isolation. Its evidence index records 125 focused checks; these include
Linux-x64 JIT and NativeAOT, native differential oracles and strict ABI checks.
The first complete JIT attempt passed 450 checks before a verification-harness
duplicate source: wscanf was supplied both explicitly and through its now
registered provider. That failed run is retained; no runtime regression is
inferred from that frontend duplicate-definition diagnostic. Value-producing
co_await continuations and process/owner termination remain the next work.

The frozen ninth regression now passes all650 JIT checks:450 before the harness
duplicate-source interruption and200 in the recorded continuation with the same
compiler/runtime. The two corrected wide-scanning verifiers compare source
bytes and use the registered provider once. Evidence is indexed separately in
`evidence/full-jit-coroutines-threads.json`; this is not an all-platform claim.

The next accepted increment adds process-wide normal/quick/immediate exit and
initial value-producing coroutine continuations. Seventy-seven focused checks
cover static/instance JIT and NativeAOT:77 is the combined process29 and CPS48
acceptance count, not complete language/library coverage. Executable main return
uses normal process termination; managed Run still returns for explicit host
cleanup. Live source workers are not joined or given freed owner storage during
process exit. Source coroutine values/addresses that survive suspension reside
in the coroutine frame. Initial-main-thread thrd_exit and richer lifetime forms
remain separate gates. Evidence:milestone-process-coroutine-values.json.

The selected PRNG component adds eleven original musl algorithm units and
raises automatic packaging to 129. Its 18 checks cover exact streams, explicit
state switching, owner isolation, forced GC and concurrent BSD random access
under JIT/NativeAOT, plus packaging and exact source-patch replay. Source state
and algorithms remain translated C; only contention yield reaches BCL services.

Executable initial C11 thread exit now uses logical join completion. The last
live source thread performs normal status-zero process termination on that
thread, preserving its identity for callbacks. Forty-two focused checks cover
initial joins, descendants, detach, cleanup ordering, runtime regressions and
existing process-exit behavior under JIT/NativeAOT. Managed Run keeps host
control; arbitrary foreign-host exit and owner-local termination remain open.
The documented C++ initial-thread cleanup policy is tested explicitly rather
than claimed identical to observed glibc behavior.

Selected binary32/64/80 remainder and decomposition now translate six original
musl algorithms, with explicit errno adaptation for the advertised profile.
Eighteen focused checks cover exact native/generated bits, signed zero, NaN
classification, output pointers, owner/TLS errno, packaging and patch replay.
Automatic source packaging reaches135 units; floating environment support and
the remainder of libm remain open.

Audited GNU libc builtins now bind matching translated C definitions before
BCL fallback, preserving the source provider for remainder/decomposition and
existing abs families. Nine native/generated static/instance JIT/AOT checks
cover exact result/output pointers and errno; frexp demonstrates retained BCL
fallback when no source definition is present. Name guessing is not used and
function ABI/body availability is checked before emitting the adapter.

Coroutine temporary provenance and wide formatting form the next151-check
checkpoint. Coroutine122 checks cover construction failures, early destruction,
POD/pointer await results and previous protocols; exact executable region
identity distinguishes retained descriptive operand lifetimes, including reused
storage. No EDG IL or flags are changed. Wide-format29 checks include native
and generated formatting, sanitizers, strict ABI, runtime locking, patch replay
and automatic packaging. Documented musl pointer-format/count adaptations avoid
undefined native source behavior; selected provider count reaches138.

Binary80 comparisons now normalize integer significands and compare exponents
without allocating BigInteger values. Thirteen focused checks cover 18,400
native comparison vectors, arbitrary finite-encoding reference comparisons,
one million allocation-free operator calls, and existing binary80 math/source
regressions under JIT/NativeAOT. Other arithmetic and floating-environment gates
remain open; see float80-comparison.md and its evidence index.

Selected musl fmin/fmax/fdim families add nine source units, bringing the
automatic provider to 147. Eighteen checks cover original/adapted native
semantics, all C/C++ static/instance JIT/NativeAOT modes, overflow errno,
owner/TLS isolation, allocation-free binary80 extrema, packaging and exact
patch replay. Full libm/fenv remains open; see libc-math-extrema.md.

Coroutine allocation-failure return handling passes ten focused native/JIT/AOT
checks for noncopyable and plain-value return objects, plus 74 cleanup-provenance
regressions. Three additional checks preserve a valid default-argument fixture
as an explicit early diagnostic: lowered metadata no longer contains the
required source expression. This remaining feature needs prelower semantic
capture; no EDG behavior is changed. See milestone-coroutine-allocation-failure.json.

The frozen coroutine-lifetime/wide-format checkpoint passed the complete752-check
synthetic JIT regression suite. All1134 snapshot file hashes were revalidated
after completion. This is evidence for that exact checkpoint; later focused
comparison, math-extrema and allocation-failure changes have separate evidence.
NativeAOT scope remains the focused matrices, not a claimed full-suite AOT run.

Stdio management adds freopen, tmpfile, tmpnam, rewind, setbuf and an authored
perror source facade. Twenty-seven native/generated, ABI, runtime and packaging
checks cover FILE identity, queued operations and disposal races, mode changes,
wide stderr preservation, immediate-exit temporary cleanup and collectible
owner-pinned name storage. Source provider reaches148 units. Other platforms
and broader POSIX/file/process coverage remain open.

Float/double round now use BCL ties-away-from-zero overloads and audited GNU
builtin aliases, completing those widths alongside existing binary80 roundl.
Sixteen focused native/generated JIT/NativeAOT and ABI checks cover adjacent
halfway inputs, signed zero, subnormals, large exact integers, infinities/NaNs,
errno preservation and function-address identity. Fenv-dependent rounding APIs
remain unsupported. Older exception narrative was reconciled with the accepted
exception_ptr and pointer-descriptor status; current table remains authoritative.

Bounded coroutine placement allocation passes28 focused native/JIT/AOT,
regression and diagnostic checks. Exact scalar/pointer/explicit-reference
arguments use incoming factory storage before frame copies. Class conversions,
defaults and implicit-object forms are not approximated. Two native-valid
source forms reproduce rejection in original EDG C processing, retained as
frontend gates without changing EDG. Global nothrow allocation is next.

Standard global nothrow coroutine allocation now passes the exact address of
the source-defined std::nothrow object. A backend-owned requirement set retains
that object even without an explicit source reference; missing providers reject
before publication. Ten native/static/instance JIT/NativeAOT and diagnostic
checks use pinned libc++ new_helpers.cpp and allocator identity probes.

Narrow/wide scanf now compute each decimal width digit before adding it,
avoiding a proven intermediate signed overflow for valid widths through INT_MAX.
Original UBSan failures, corrected native/source JIT/NativeAOT acceptance and
patch replay are retained. A separate246-record audit confirms the selected
ISO byte-counted wide-destination contract;160 glibc differences remain
observations. Twenty-six checks are indexed, including expected negatives and
native corpus regressions. Widths beyond INT_MAX remain explicit work.

Owner environment locale selection and fixed-width binary80 conversions pass30
focused checks: locale20, conversion2 and shared provider packaging8. The
provider now contains185 source units. Empty locale names resolve translated
LC_ALL/category/LANG without reading live host state, with transactional category
updates and explicit C/POSIX/C.UTF-8 limits. Float80 has no BigInteger paths;
36,000 native conversion records,100,000 independent narrowing cases and checked
integer boundary grids agree, while one million warmed conversion groups allocate
zero managed bytes in JIT and NativeAOT. Other extended math helpers, dynamic
fenv and execution on other platforms remain open. See float80-conversions.md,
libc-locale-environment.md and milestone-locale-float80-conversions.json.

The bounded volatile-bitfield task passes ten checks: original EDG C/C++
oracles plus static/instance JIT/NativeAOT. Accesses honor qualifiers retained on
the field or containing object after rvalue conversion; exact byte loads/stores
preserve neighbors, configured integer promotion, boolean values and one-time
receiver/RHS evaluation. Native MMIO container widths and atomicity are not
promised. An analogous preexisting nonvolatile C++ bool:1 sign-extraction path
remains a candidate gap to validate when project-driven work needs it.

SQLite's checked-in full profile first reached missing exp, then the explicit
no-math diagnostic profile reached GNU checked-overflow builtins. Add/subtract/
multiply builtins now pass ten C/C++ native/original-C/static/instance JIT/AOT
checks, including generic mixed sign/width arithmetic through128bits, destination
writes on overflow and alias/evaluation behavior. The interim SQLite library
strict-builds and its core executable matches the native transcript. Full-profile
math and project NativeAOT/owner acceptance are still separate pending steps.

The unchanged pinned SQLite amalgamation now passes its full configured core
corpus under strict generated C# JIT and NativeAOT. Both6,337-byte transcripts
match the fresh native run and checked-in reference. The optional SQL math
extension remains enabled;28 selected musl double math units bring the provider
to213. Nine math component checks plus three project core checks are indexed in
`evidence/sqlite-core-math-milestone.json`. This establishes the test-only memory
VFS/THREADSAFE=0 profile on Linux x64, not native SQLite OS VFS or complete
project coverage. Additional corpora and independent-owner checks are underway.

Two independent generated SqliteCore instances also pass strict JIT/NativeAOT
consumer checks with the full feature profile: separate databases/transactions,
forced collection, sequential thread handoff, one owner's shutdown while the
other continues, and replacement-owner initialization. THREADSAFE=0 does not
permit concurrent calls into one engine. Evidence:sqlite-project-owners.json.


## Revised source-correspondence implementation (in progress)

The user requested a change in emission strategy after inspecting the working
SQLite output. Original names now drive backend-owned symbol mappings;
structured statement views retain EDG control-flow targets, and expression
plans keep necessary sequencing/conversion temporaries at their source boundary.
EDG IL and its layout/lifetime semantics remain unchanged. The implementation
also adds named global/TLS ref views over the existing pinned arenas and named
frame aliases, keeping ordinary accesses separate from explicit address paths.

First integration evidence is provisional and predates subsequent corrections:
`persistent-continuation/source-control-translated-001` passes ten original-C
and generated JIT/NativeAOT checks for C/C++ Duff dispatch, jumps entering loops
and branches, switch fallthrough and nested break/continue. The first expression
matrix passed ten checks. Broader naming/multi-TU/packed/volatile regressions
then exposed address-provenance, special RTTI-call operand handling, narrow
compound conversions and a helper cache keyed by presentation field name.
These are C# backend integration issues; fixes do not change EDG lowering.
At that point there was no revised-emitter SQLite acceptance or completed
readability claim.

The subsequent integrated milestone now passes 14 source-correspondence checks,
ten structured-control checks and six complete driver layout/navigation checks.
The layout matrix covers all four split modes, keyword namespace/class names,
instance NativeAOT, exact generated line mappings and preservation of superseded
generation inputs. Source-less split helpers explicitly enable nullable context,
which Roslyn otherwise suppresses for the `.generated.cs` filename convention.

Revised SQLite core output matches all 6337 native/reference bytes under JIT and
NativeAOT. A separate consumer declares `SqliteCore.sqlite3*` and
`SqliteCore.sqlite3_stmt*`, calls the original public API names directly, and
passes independent-owner/transaction/GC checks in both runtimes. These five
project checks are fresh evidence for the revised emitter. The other five
corpora subsequently passed all15 native/reference/JIT/NativeAOT checks in
`sqlite-readable-additional-001`, bringing revised project acceptance to20.
Their older passes are not substituted. Commit `a00394edb` records the first
verified source-correspondence implementation increment. A parser regression
exposed by this rerun was fixed locally in the expression planner: pointer
compound arithmetic now retains the element stride instead of adding raw bytes.
Expanded C/C++ expression tests cover consumed/discarded pointer compounds,
struct/array strides, negative displacements and single evaluation.

See `evidence/source-correspondence-milestone.json`,
`evidence/source-expression-components.json` and
`evidence/source-naming-components.json` for exact results, hashes and snapshot
distinctions. This is an accepted increment, not completion of source fidelity:
enum types still use physical integers in declarations, some parameters/locals
carry unnecessary collision suffixes, and comma-increment loops still fall back
to labels. These are the next bounded source-correspondence tasks.

Build010 completes the latter two items: locals and parameters preserve their
spelling unless their own C# scope conflicts; actual shadowed owner references
are qualified. Source routine domains are distinct and deterministic. Synthetic
helper bodies explicitly qualify their source calls/references, including GNU
statement expressions, cleanup, adapters and exception destructors. Comma
increment expressions remain in `for` headers when every operand admits a legal
statement-expression form; prefixes retain the semantic fallback. Fixed source
array-variable accesses now use named InlineArray views without changing their
explicit address path. Exact-int shift operands no longer receive redundant
casts.

The refinement passes naming/shadow14, GNU statement10, expressions10, arrays10,
RTTI12 and function-try9 checks, plus fresh SQLite core3 including NativeAOT.
`docs/source-correspondence-review.md` reviews four real SQLite implementations
against saved original bodies. See `evidence/source-correspondence-refinements.json`
and `evidence/source-shadow-components.json`. Source-typed enum declarations
remain the next bounded requirement; broader standards/platform gates stay open.

Build012 retains CLR-representable enum types in declarations, fields, globals,
TLS, pointer/callback signatures and public APIs. Named members remain visible;
explicit conversions preserve EDG's integral promotions, scalar boundaries,
conditional results, bitfield truncation and pointer element strides. Unnamed
negative enum constants use valid parenthesized C# casts. Affiliated/promoted
integer nodes remain physical integers. Wider-than-64-bit enum extensions retain
the documented named constant container because CLR enums cannot represent that
underlying type.

The enum matrix passes 12 C/C++ native/original-C, JIT/NativeAOT and direct typed
API checks. The existing pointer/enum exception matrix passes nine checks with 48
native traces, forced GC, owner crossing and thread isolation; source review14
and SQLite core3 also pass. The final frozen012 project run covers all six
corpora and both named-API and lifecycle-owner consumers. Source/type evidence
is `evidence/source-enum-components.json`; broader standards/platform gates are
not closed by this source-correspondence work.

Final frozen012 acceptance is complete: all six SQLite corpora pass native
reference/JIT/NativeAOT comparisons (18 checks); direct original-name API and
independent-owner lifecycle consumers each pass JIT/NativeAOT (four more).
All945 implementation input hashes are unchanged across these22 project checks.
The final naming14 and layout6 regression matrix also passes012. Four final
SQLite implementation bodies match the manually reviewed010 bodies byte for
byte; the reviewed source index points to final generated lines.

Implementation milestones are `a00394edb` (names/structured expressions/storage),
`9c15c35f3` (local names, arrays and comma loops), and `d84359051` (nominal enums).
Final durable evidence is `evidence/sqlite-source-correspondence-final.json` and
`evidence/source-type-final-regressions.json`. The bounded revised-emission
increment is accepted for supported inputs. Broader language/library coverage,
production OS VFS/shared SQLite engine concurrency and Windows/macOS/Arm64
execution remain explicit backlog gates. The separately requested semantic-tree
export investigation does not change this implementation or authorize migration.

The complete pre-readability SQLite baseline contains20 unique project checks:
core/API/VFS/vtable/allocation/FTS5 each match fresh native and checked-in
transcripts under JIT and NativeAOT, and the two-owner consumer passes both
runtimes. The1402-file frozen-input comparison found only three test-file edits,
with no implementation/compiler/runtime/provider drift. Consolidated durable
evidence is `evidence/sqlite-project-baseline.json`. This baseline explicitly
predates the revised source-correspondence emitter.

### Review refresh and semantic export checkpoint (2026-10-03)

The second explicitly authorized cleanup removed disposable compiled outputs
from inactive tests, reclaiming 84.09 GiB while retaining sources, IL, logs,
provenance, current compilers and active work. See
`evidence/temporary-cleanup-20261003-second.json`.

Fresh full-feature SQLite translation is available through
`artifacts/review/sqlite/REVIEW.md`, with size-split functions, original API
names and an updated source index. All 74 emitted C# files carry the requested
`// <auto-generated />` marker and explicit nullable context; the refreshed
strict Release build passes. This repeats generation/compilation, not the prior
runtime acceptance matrix. Test-memory VFS and THREADSAFE=0 limits remain.
See `evidence/sqlite-review-output.json`.

The user subsequently authorized implementation of the semantic exporter.
The isolated native exporter and managed reader now form a verified first
checkpoint: unlowered C/C++ graphs, original source bytes, source and macro
provenance, resolved overload/template identities and cleanup facts. All 20
reader checks pass under both JIT and NativeAOT on Linux x64. This does not
replace the current emitter: unsupported adapters remain explicit diagnostics,
and strict emission rejects the deliberately incomplete packages. Commands,
evidence and remaining work are recorded in
`docs/semantic-export-implementation.md` and `semantic-export/README.md`.

The 2026-10-07 continuation adds an isolated managed C# emitter for the bounded
`c-integer-structured/1` profile. Native adapters now close the metadata gaps in
the positive C fixtures and expose qualifiers, cleanup/alignment facts and full
64-bit constants. Generated source retains names and structured control flow;
226 native-C observations match JIT and NativeAOT. Profile rejection remains
independent of the deliberately false graph-wide completeness flag. This does
not migrate SQLite or implement C++ lifetime lowering. The checkpoint, exact
validation evidence and next project-driven steps are recorded in
`docs/semantic-export-c-emission.md`.

The next semantic-export milestone audits the actual 77-TU `cpfe-dot` source
set, including its dot backend, and implements a separate C# project handler
for the configured SQLite amalgamation. SQLite-driven native adapters eliminate
its 7,906 baseline adapter diagnostics. All 3,165 engine/VFS routines compile;
43 native-C observations match JIT and NativeAOT, with the C memory VFS itself
translated. The process-static, THREADSAFE=0 memory-VFS scope and C++ adapter
backlog remain explicit. See `docs/semantic-export-self-audit.md`,
`docs/semantic-export-sqlite.md` and their semantic-export evidence indexes.

Direct C# emission of the original 75-TU EDG compiler has now succeeded, using
the native C++14/no-exceptions/no-RTTI build profile and the C backend's supported
local-entity promotion configuration in an isolated compiler. The formatted
review package contains 155 C# files and explicitly records 82 unbound external
declarations. These declarations retain source names and throw when accessed;
they are review placeholders, not portable host bindings or a runnable selfhost.
See `docs/self-csharp-review.md` for the final review location and build status.
The strict full C# build was stopped after Roslyn reached 78.2 GiB RSS and host
memory became scarce. No compiler diagnostics or assembly were produced before
the stop, so semantic compilation remains unverified. The terminated build's
zero exit code is not accepted as success; retained resource evidence records
the incomplete check.

The large project exposed scaling limits in our emission/packaging tools.
Marker substitution and export-origin lookup now avoid repeated whole-output
or whole-symbol-table scans. SQLite's 74 C# files remain byte-identical after
these optimizations; source-index equivalence accounts only for timestamped
musl snapshot paths. Roslyn formatting preserves exact tokens/trivia within
huge synthetic initialization blocks and numeric collections while formatting
source methods normally. Large formatter responses use Unicode-safe chunks.
Bulk/token-identity and JIT/NativeAOT checks, chunk-boundary tests, and the six
existing formatter checks pass. Native raw output is retained; explicit replay
is marked packaging-only and never attributed to an unused compiler binary.

The requested exception-enabled self-review is now separate at
`artifacts/review/edg-exceptions/README.md`. All 75 original translation units
emit and pass Roslyn syntax checking/formatting with `--exceptions --no-rtti`:
155 C# files, 284,931,624 bytes and 84 explicitly unbound external symbols.
`translate_self.py --exceptions` overrides the native database setting and
records native/effective flags independently. Linked `token_to_string` excerpts
show successful-construction state tracking, a false-returning exception filter,
function-level `finally` and destructor cleanup helper. The previous review is
retained. No full C# build, managed-compiler runtime or NativeAOT execution is
claimed; see `evidence/self-review-exceptions-20261003.json`.

### Storage readability, macro APIs and external runtime contracts (2026-10-03)

Ordinary aggregates/arrays now use named typed locals when managed layout
supplies the native alignment. Explicit alignment, large native heap frames
and coroutine storage retain their necessary fallbacks. Static byte/string
initialization uses constant-span bulk copies, selecting exact UTF-8 literals
or numeric bytes; literal-pool pointers receive safe bounded textual previews.
Generated empty/bare-return noexcept bodies no longer receive catch/termination
wrappers. Bodies with synthesized member destruction still do.

`--export-macros REGEX` adds opt-in final-macro-snapshot APIs: EDG-folded integral
constants become C# const fields; other expressions become typed properties;
function-like macros require `--macro-signatures` native type contracts. The
implementation checks include-wrapper preprocessing equivalence and preserves
the adapters and provenance. A real SQLite test exposed a greedy managed
signature parser; a balanced parameter scanner and lifecycle-boundary regression
now cover that case. Arbitrary textual macro semantics are not claimed.

`--unbound-externals partial` emits required partial methods and external-storage
address hooks. `--runtime-source` includes C# providers in the same assembly.
Missing implementations fail compilation; existing bound runtime imports and
the separate throwing review mode retain their behavior. This implements the
binding mechanism, not every outstanding project import.

Focused native/JIT/NativeAOT tests cover bytes, local layout and unwinding,
empty noexcept functions, macros and partial runtime implementations. The
canonical compiler was rebuilt and its local/noexcept checks also pass.
SQLite's full-feature core corpus matches fresh native output under JIT and
NativeAOT. The new `artifacts/review/sqlite-readable` library exports 28 actual
constants plus the version-pointer property; strict build and a direct managed
consumer pass JIT and NativeAOT. These remain Linux-x64 tests with the test VFS
and THREADSAFE=0 profile.

The fresh full 75-TU exception-enabled EDG review is at
`artifacts/review/edg-readable-exceptions`: 155 C# files, 242,687,604 bytes,
14.83% smaller than the preceding exception review. Raw frames fall from 7,484
to 1,285 and noexcept guards from 2,519 to 1,929. Emission/syntax/formatting pass;
full managed compiler build/runtime remain unverified, with 84 unbound symbols.
Earlier review aliases and all intermediate artifacts remain intact.

Details: `docs/source-storage-readability.md`, `docs/macro-exports.md`,
`docs/partial-externals.md`. Evidence: `evidence/source-storage-readability-20261003.json`,
`evidence/macro-exports-20261003.json`, `evidence/sqlite-readable-macros-20261003.json`,
and `evidence/edg-readable-exceptions-20261003.json`.

### EDG review replacement (2026-10-04)

Retranslated all 75 original TUs with the current backend, exceptions enabled
and `--unbound-externals partial`. Native emission, Roslyn syntax validation and
formatting pass. The 84 unresolved symbols are now required partial-method
contracts, including external-storage address hooks. At the user's direction,
the existing `artifacts/review/edg`, `edg-exceptions` and
`edg-readable-exceptions` aliases all point to the refreshed output. The README
links named-local unwinding, empty destructor and extern-contract excerpts.
Full managed compilation/runtime remain unverified; no implementations of the
84 contracts are implied. Evidence: `evidence/edg-self-refresh-20261004.json`.

### Embedded primitive initializer tables (2026-10-04)

Numeric arrays now use private static `ReadOnlySpan<T>` properties and bulk
`CopyTo` initialization. This covers signed/unsigned 16-, 32- and 64-bit data,
nominal enums, Half, float and double, including multidimensional rows and
primitive array fields. Floating values use primitive bit-pattern data and a
typed `MemoryMarshal.Cast` view; no decimal round-trip or per-element floating
conversion is introduced. Data properties are deduplicated and chunked at 4,096
elements. Long zero runs retain default storage rather than embedded zero blobs.
Pointer relocations, record initialization and extended 128-bit/80-bit scalar
representations retain their existing paths.

The new primitive fixture matches GCC/Clang under static and instance JIT and
NativeAOT, including NaN payloads, signed zero, chunk boundaries, zero gaps with
nonzero suffixes, TLS and owner isolation. PE inspection verifies 29 initializer
getters backed by 24 RVA fields, exact uint payload bytes and no heap-allocation
instructions or unaudited calls. The default compiler is rebuilt and passes the
same checks. SQLite's full-feature core corpus still matches native under JIT
and NativeAOT. See `evidence/primitive-data-20261004.json` and
`docs/source-storage-readability.md`.

The existing EDG review aliases now contain a fresh full exception-enabled
translation with these properties: 155 C# files, 243,496,119 bytes and 3,458
primitive initializer properties (including backing-data/typed-view pairs).
The README links a real uint property and its copy site. Emission and syntax/
format validation pass; full managed compiler build/runtime remain unverified,
and its 84 extern partial contracts still need implementations. See
`evidence/edg-primitive-data-review-20261004.json`.

### Original managed export names (2026-10-04)

Every named wrapper emitted by managed-library mode now receives its
source-resolved name, including internal/static helpers and named synthesized
C++ routines. The implementation receives a `__body` suffix. External APIs take
priority over same-spelled internal functions from other translation units;
keywords and unavoidable collisions retain the existing C# naming rules.
Executable body names are unchanged. Lifetime and exception boundaries remain
on public wrappers, and translated internal calls still target the bodies.

Seven focused checks pass, including direct original-name calls in static and
instance JIT/NativeAOT consumers, cross-TU priority, body-name collisions, source
splitting, C++ overloads/synthesized members and executable naming. The canonical
compiler also passes the existing 14 source-name checks. Details:
`docs/export-names.md`; evidence: `evidence/export-names-20261004.json`.

### Compact record tables and EDG review refresh (2026-10-04)

Ordinary global/TLS record tables now use source-named typed pointer aliases and
one target-typed object initializer per row. Named fields retain trailing commas,
plain nested records can nest initializers, and representable narrow integer
constants omit casts. Unions, bitfields, array members, lowered bases and
potentially overlapping layouts retain the existing path. Omitted fields and
elements remain zero; address relocations target final arena storage. Literal
pointers use an aggressively inlined `__edg_literal` helper with the same pool,
UTF-8 comments and byte-offset semantics; redundant zero offsets disappear.

New record and existing byte fixtures both pass GCC/Clang comparison plus
static/instance JIT and NativeAOT, including self-referencing table pointers,
TLS aliases named p/storage, owner/thread isolation and forced GC. SQLite core
also matches native output under JIT and NativeAOT. The canonical compiler is
rebuilt and the existing 14 source-name checks pass. Evidence:
`evidence/record-initializers-20261004.json`; implementation details:
`docs/source-storage-readability.md`.

All existing EDG review aliases now point to a fresh full exception-enabled
75-TU translation: 155 C# files, 190,188,359 bytes, 21.89% smaller than the prior
243,496,119-byte review. It includes 97 record-table aliases, 198,167 indexed
object initializers and all 3,458 primitive initializer properties. All 41,900
exports use source-resolved names rather than the __edg_export_ prefix. The
README links the user's exact builtin_riscv_vector_table[15643] row, named-local
cleanup and required extern contracts. Emission, syntax validation and formatting
pass. Full managed compiler compilation/runtime remain unverified; 84 external
partial contracts still require implementations. Evidence:
`evidence/edg-record-initializers-review-20261004.json`.

### Named aliases for all initialized array tables (2026-10-04)

Extended initializer pointer aliases from eligible plain-record arrays to all
nonempty constant-initialized global/TLS arrays, including static-local backing
arrays. Data and function-pointer tables now assign directly through the alias
without inline-array cell `.Value` wrappers. Primitive/string bulk copies and
nested/union/bitfield/array-member initialization retain their existing semantics
while using the named destination. A byte-span clear handles native sizes,
padding and omitted entries without invalid `Span<pointer>` type arguments.
Constructor/TLS helper locals `error`, `p` and `storage` are reserved against
alias collisions.

The new fixture checks 17 alias cases, native GCC/Clang parity, pointer and
function-pointer values, repeated/null tails, forward/cyclic relocations,
dynamic startup, TLS, owner isolation and forced GC. Static/instance JIT and
NativeAOT pass. Existing record and primitive-table suites also pass, including
the primitive getter PE-data audit. SQLite core retains native parity under JIT
and NativeAOT. The canonical compiler is rebuilt. Evidence:
`evidence/table-aliases-20261004.json`.

All three existing EDG review aliases now show the requested
`builtin_type_strings[5924] = __edg_literal(...)` form beneath a single
`sbyte**` table declaration. The refreshed 75-TU exception-enabled review has
155 C# files, 188,334,585 bytes and 11,468 named array aliases: another 1,853,774
bytes smaller than the prior record-table review. Emission, Roslyn syntax
validation and formatting pass. Full compiler managed compilation/runtime
remain unverified, with 84 external partial contracts still unimplemented.
Evidence: `evidence/edg-table-aliases-review-20261004.json`.

### Fixed Managed.Runtime contracts (2026-10-04)

Unbound partial functions/storage hooks now belong to fixed `Managed.Runtime`,
independent of translated namespace, class and split mode. Source-named owner
methods forward to a runtime instance: one for a static owner, one per object in
instance mode. Contracts have fully qualified generated ABI types and a separate
name-allocation domain. `Runtime.Owner` preserves context for incoming instance
callbacks. Partial mode emits the runtime shell even when no imports are used.

`--runtime-source` accepts reusable implementation supersets. A small Roslyn
contract/provider pass adds missing defining declarations for unused methods;
it supports aliases, escaped namespace identifiers and callback types, and keeps
signature errors visible to the compiler. Provider code does not need namespace
or class rewriting. Generated record/enum aliases and mode-specific callback
adapters remain project-specific. See [provider documentation](partial-externals.md).

Verification: 12 focused checks passed, including unchanged common provider
bytes across different names/modes and no-import output, required-body failures,
records/enums/pointers, native allocation, forced-GC stable storage and independent
owner callbacks. Four hosts passed strict JIT and NativeAOT. Default rejection
and throwing-declaration regression tests passed; unformatted emission also
built strictly. Both the isolated and canonical native compilers built.
[Evidence](../evidence/managed-runtime-20261004.json) records hashes and artifacts.

The existing EDG review aliases now show all 75 original translation units with
84 contracts in `Managed.Runtime.generated.cs`: 156 C# files, 188,347,480 bytes.
Native emission, syntax validation and formatting passed. Full managed EDG
compilation/execution remain unverified and its required contracts are still
unimplemented. [Review evidence](../evidence/edg-managed-runtime-review-20261004.json).

### Shared versus translated-owner extern contracts (2026-10-04)

Partial extern placement now follows the emitted signature recursively.
Primitive/BCL/shared-runtime signatures remain in `Managed.Runtime.generated.cs`.
Generated records, enums, opaque types and callbacks containing generated types
use private partial methods on the original translated owner, in
`Translated.Runtime.generated.cs`. Instance callbacks are owner-local because
their hidden ABI parameter names the generated class. Local providers can use
`this` and generated types directly. Manifest/source navigation identify both
owners and files; all split modes and unformatted output retain this separation.

The host EDG FILE is a complete 216-byte glibc record, so its externs remain local.
No broad record-to-void-pointer mapping was added. The existing portable FILE ABI
already uses typed local adapters around shared `LibcFiles` opaque handles;
portable headers or an explicit compatible adapter are needed to reuse that
approach with host-header translations. See [FILE policy](partial-externals.md#file-and-other-platform-records).

Both native compiler builds passed. All 12 focused checks passed, with four JIT
and NativeAOT hosts covering primitive provider reuse, records/enums, opaque
handles, recursively typed callbacks, owner isolation and placement metadata.
Default rejection/throwing stubs and strict `--format none` builds also passed.
[Verification evidence](../evidence/contract-split-20261004.json).

EDG was refreshed from all 75 original translation units: 52 shared contracts and
32 owner-local contracts, including FILE, platform structures and C++ ABI types.
The shared file contains no generated EdgCompiler types. There are 154 C# files
(188,342,153 bytes). Full managed EDG compilation/execution remains unverified.
[EDG review evidence](../evidence/edg-contract-split-review-20261004.json).

SQLite was retranslated at the user's request; both existing SQLite review
aliases now point to the new size-split instance library. It preserves 29 macro
exports, 213 musl source units and the test memory VFS profile. Strict compilation
and the existing macro/version/open-close consumer pass JIT and Linux-x64
NativeAOT. This refresh does not rerun the broader SQL corpus or establish a
production OS VFS/threading profile. [SQLite evidence](../evidence/sqlite-contract-split-review-20261004.json).

### Nested tables and selected-union initializers (2026-10-04)

Named initializer aliases now cover nonempty struct/union globals and TLS, in
addition to top-level arrays. A record containing tables receives one pointer to
its final arena storage; initialization uses named member accesses such as
`(__log_data->tab)[80]`, without repeating the arena cast. Bulk array data retains
its existing destination/stride semantics and aliases do not create copies.

The object-initializer eligibility check now permits ordinary union layouts when
EDG identifies exactly one initialized member. It follows the first initializable
field or a validated lowered union designator, as the C backend does. SQLite's
`aAlterTableFuncs` rows therefore use one `new()` value with a nested
`u = new() { pHash = ..., }`. Trailing commas, constant bit patterns, pointer
relocations and omitted-field zero values are preserved. Bitfields, array-member
layouts, lowered bases and unsupported multiple union selections retain their
existing fallback. See [storage readability](source-storage-readability.md).

The expanded table-alias and existing record-initializer suites pass against
GCC/Clang under static/instance JIT and NativeAOT. Coverage includes nested tables,
selected function/data-pointer union members, omitted fields, exact negative-zero
bits, zero padding, owner/TLS isolation and forced GC. SQLite core output also
matches its native reference under JIT and NativeAOT. Both native backend builds
pass. SQLite's existing review aliases are refreshed with the requested examples;
full translated EDG was not regenerated for this SQLite-focused change.
[Verification and review evidence](../evidence/nested-tables-20261004.json).

### Shared span varargs without per-call helpers (2026-10-04)

Named variadic methods now take `params ReadOnlySpan<Edg.Runtime.VaArg>`.
Direct calls retain the routine name and promoted scalar arguments; indirect
calls pass an explicit span collection. The shared authored `VaArg`, `VaPack`
and `VaList` modules contain the runtime implementation. Generated bodies pack
once on entry, with stack storage up to 64 KiB and NativeMemory above that;
using-based disposal covers return and unwinding. Explicit va_list forwarding
keeps the existing cursor ABI. Values larger than 16 bytes use an unpinned
snapshot at argument evaluation to preserve values and avoid leaks when later
arguments throw. [ABI and provider migration](varargs-span.md).

Both native compiler builds pass. Verification includes 25 shared runtime checks
under JIT/NativeAOT (zero measured managed allocation for 10,000 scalar calls),
C/C++ GCC/Clang/original-EDG oracles and static/instance JIT/NativeAOT, shared
external partial providers, and a strict VaArg-only declarations-mode project.
SQLite core again matches its native reference; the library consumer passes
JIT/NativeAOT including exported macros, rename-column and math checks.

Both existing review alias sets are refreshed. SQLite has zero varcall helpers
(previously 699), and 408,611 fewer translated-source bytes. The complete 75-TU
EDG emission has zero varcall helpers (previously 2,500); syntax, formatting and
packaging pass. Full managed EDG build/execution remains unverified, as do other
OS/architecture runs. No broad libc/C++ completeness claim follows from these
checks. [Verification and review evidence](../evidence/varargs-span-20261004.json).

### EDG self-translation with musl (2026-10-04)

`translate_self.py --profile musl --libcxx-acquisition PATH` now translates all
75 original compiler source units with 213 selected musl/provider units and
the pinned libc++18 headers. The acquisition's header hashes are verified;
no native system headers or native C++ implementation are supplied. A private
portable declaration overlay covers host metadata, directories, descriptors,
resource limits and mappings. EDG's supported allocation/read fallback replaces
memory-region mmap; language extensions and C++/CLI metadata remain enabled.
Binary128 classification uses an exact builtin adapter for libc++'s missing
extended-type overload. Original conversion settings leave quadmath declarations
unreferenced, so there is no quadmath runtime dependency in this output.

The refreshed existing EDG review aliases and new `artifacts/review/edg-musl`
contain 410 C# files (187,177,227 bytes), 106 managed libc bindings and no varcall
helpers. Required external contracts drop from 84 to 24: 9 C++ library/runtime
symbols and 15 POSIX host services. These remain real required partial bodies;
the complete translated compiler is not yet runnable. The discussed common
Edg.Runtime namespace migration remains separate from this provider change.

The C++ musl probe exposed a source-control emitter bug: adjacency alone was
used to move preceding local declarations into for headers, hiding variables
used after the loop. Our emitter now retains those declarations before the
structured loop, as the C backend does. No EDG source/semantic change was made.
Safe restoration of true for-init headers needs original scope provenance.

Seven bounded profile checks pass, including strict JIT/Linux-x64 NativeAOT,
formatting/parsing, binary80/binary128, allocation, exceptions, real libc++
required-contract diagnostics and native/C# C/C++ loop scope probes. The existing
ten-check structured-control suite also passes. Full self emission, Roslyn
syntax validation, formatting and packaging pass; full managed compiler build
and execution remain unverified. [Evidence](../evidence/self-musl-20261004.json)
and [review guide](self-csharp-review.md).

### Read-only atomic loads exposed by SQLite WAL (2026-10-04)

The real-file SQLite process campaign found an access violation when loading a
read-only WAL index: `AtomicOps.Load` used compare-exchange, which writes even
when the stored bits are unchanged. Aligned 1/2/4/8-byte loads now use read-only
acquire operations, with full fences for sequential consistency; the existing
unaligned/larger address-lock fallback reads without writing. EDG was unchanged.

The pre-fix failure is retained. OS-enforced read-only mapping tests pass strict
JIT/Linux-x64 NativeAOT for every legal load order, zero/nonzero contents,
aligned/unaligned typed and raw objects. Existing atomic conformance/contention
and original-EDG-C versus static/instance C11 JIT/AOT checks also pass.
[Evidence](../evidence/atomic-readonly-20261004.json).

### Filesystem-backed, threadsafe SQLite host (2026-10-04)

`sqlite_host.py` now prepares the pinned SQLite APPDEF adaptation, translates
SQLite plus 213 selected musl/provider units, supplies all four host partial
contracts, and verifies the resulting reusable library. It preserves public
API names, exports 490 object macros and retains size-split source. THREADSAFE=1
uses real BCL mutexes; disk temp, WAL and database mmap are enabled. The adapter
reuses the authored dotcc VFS/platform sources with owner-first callback thunks,
owner-specific registration/mutex/TLS state and pinned callback-table storage.
Static mutex identities survive shutdown/reinitialization.

Strict JIT/Linux-x64 NativeAOT builds and all 11 in-process test groups pass.
The full independent-process campaign passes under both execution modes with
native↔translated and translated↔translated pairs: locks, forced-death recovery,
WAL snapshots/checkpoints, readonly indexes/media, hardlinks and symlink paths.
The first campaign's readonly atomic failure and its regression evidence are
retained. A separate eight-caller heap/errno/TLS/shutdown fixture also passes
JIT/AOT. Review aliases now point to the host profile.

This is OS_OTHER host integration, not an implementation of the POSIX layer.
The user's OS_UNIX suggestion was tested: portable declarations allow upstream
Unix VFS emission, leaving 29 POSIX externs plus two APPDEF hooks. Descriptor,
metadata, fcntl and mapping services remain to be implemented for that route.
Windows/macOS execution and native extension loading are not claimed.
[Usage and limits](sqlite-host.md), [Unix analysis](sqlite-posix.md),
[evidence](../evidence/sqlite-host-20261004.json).

### Upstream Unix SQLite and reusable POSIX runtime (2026-10-04)

`dotnet.py --posix` now adds the fixed portable Unix headers, checked import
bindings, shared `Edg.Runtime.PosixContext` source closure and shutdown cleanup.
The selected descriptor, path/metadata, locking, mapping and time surface is
implemented. `open`/`fcntl` consume shared variadic spans without call helpers.
Wrong record layouts are rejected before pointer conversion. Static and instance
translation match native behavior and independent portable C layout oracles;
JIT/AOT lifecycle checks verify descriptors/mappings are released.

`sqlite_host.py --profile unix` retains SQLite's upstream Unix VFS and supplies
its BCL APPDEF mutex/barrier hooks and runtime database-mmap capability hook. Final `sqlite-posix-005` strict builds,
all 11 in-process groups and native↔translated/translated↔translated process
campaigns pass under JIT and Linux-x64 NativeAOT. This includes disk temp,
WAL/mmap, readonly media/recovery, forced process termination, checkpoint races,
hardlinks and symlinks. Review aliases now point to the Unix emission; the prior
OS_OTHER profile remains at `sqlite-host`.

Review exposed a traditional-process-lock hazard: unrelated BCL closes or
separately copied runtime assemblies could invalidate/bypass ownership. Unix
now uses per-owner/inode OFD anchors with atomic kernel conversion, safe readonly
anchor migration and POSIX close-any-owner-descriptor semantics. The reproduced
failure and passing JIT/AOT edge tests are retained. Darwin OFD APIs are private
and fail explicitly if unavailable. Windows uses a same-account cross-session
advisory registry with atomic range conversions and stale-owner recovery;
registry algorithms pass independent-process JIT/AOT tests on Linux. Windows
native SQLite/other-account interoperability is not provided. Windows .NET 10
under Wine 11 passes 83 Windows-path runtime checks, the cross-process registry
suite, all 11 SQLite groups and translated peer contention/crash recovery.
Actual Windows/macOS execution remains unverified.

This is the configured SQLite POSIX surface, not complete POSIX/musl. General
pthread/process/descriptor-stdio APIs and unsupported mapping cases remain
explicit. [Runtime and platform contract](sqlite-posix.md),
[evidence](../evidence/sqlite-posix-20261004.json).

Wine exposed Windows `ERROR_USER_MAPPED_FILE` during Unix-VFS VACUUM. An
independent BCL probe confirms that shrinking fails with a live mapping and
succeeds after unmapping; growing remains supported. SQLite now selects ordinary database reads on Windows
by clamping mmap limits during initialization; WAL shared-memory mappings remain
active. The Unix mapping path remains enabled. Tests check the explicit Windows
fallback and run the same snapshot, growth, checkpoint and VACUUM scenarios.

Final Wine campaign `posix-wine-003` also validates a 5,015-frame WAL with two
32 KiB shared-memory regions, an independent reader, blocked checkpoint during
a snapshot and successful truncate checkpoint after release. Native Windows
and Windows NativeAOT remain unverified. Reusable Wine runner and full commands,
settings, runtime archive provenance and peer events are retained.

### Bounded initialization for full compiler compilation (2026-10-04)

Large static/TLS aggregate tables now initialize through ordered bounded helper
methods, passing the original typed table pointers. Startup calls still follow
all static stores. Native/static/instance JIT/NativeAOT tests validate complete
tables, nested arrays, relocations, TLS, owner isolation and GC. Existing table
alias readability tests pass. A full-size 7.7 MB embedded literal pool compiles
within 6.03 GiB measured process-tree RSS; its representation stays unchanged.
`build_managed.py` adds bounded compiler memory/time monitoring. This milestone
does not yet establish full managed EDG compilation or execution.
[Evidence](../evidence/initializer-chunks-20261004.json).

### EDG self runtime dependencies (2026-10-04)

The shared POSIX runtime now supplies directory enumeration, secure temporary
creation, terminal detection, virtual cwd changes and CPU-limit operations.
An optional descriptor provider unifies libc FILE and POSIX ownership: fopen,
fdopen, fileno, mapping and close share one descriptor registry. Stream buffers
flush before POSIX disposal. Portable records support C headers included inside
a C++ namespace by inspecting their retained source names and actual layouts.
Native/translated C/C++14 static/instance JIT/NativeAOT and Wine tests pass; the
full SQLite Unix campaign remains green. Windows finite CPU limits return
ENOTSUP; Unix limits apply to the host process.

A minimal pinned libc++18.1.8 C++14 provider supplies real allocation, new-handler,
exception classes, RTTI and managed exception ABI definitions. Audited private
header/constinit adaptations avoid unused library dependencies. Focused native,
JIT and NativeAOT tests cover allocation failures, handlers, RAII and exceptions.
The full EDG translation/build is the next acceptance step, not implied by these
component passes. [Evidence](../evidence/self-runtime-dependencies-20261004.json),
[descriptor ownership](posix-stdio.md), [libc++ provider](../backend/self/libcxx.md).

### Whole-program function retention and enum indexing (2026-10-04)

The first full managed compiler build completed within 16.9 GiB measured RSS,
revealing missing inline-member references and enum-index conversions. A two-TU
reduction showed that the custom backend's default disabled needed-flag tracking:
the source `referenced` bit alone did not retain every merged inline definition.
The supported configuration now enables `MAINTAIN_NEEDED_FLAGS`, retains unused
IL for source/lifetime provenance, and explicitly enables local-entity promotion
as the C backend does. The canonical build now carries the promotion setting
previously supplied only by the isolated self-review build.

Named enum subscripts now use EDG's integral promotion in pointer indexing and
address capture, including reversed subscripts. Native C/C++ and original EDG
C-backend oracles, static/instance JIT/NativeAOT reductions, large initializer
regressions and the minimal libc++ provider all pass with the rebuilt compiler.
Full compiler regeneration and execution remain the next acceptance step.
[Evidence](../evidence/self-emitter-reachability-20261004.json).

### Explicit portable EDG host configuration (2026-10-04)

Full self emission `edg-self-runtime-005` has no unresolved externs and builds
with zero warnings/errors in 397 seconds at 17.84 GiB measured RSS. Its 57,802
owner methods fit the CLR limit, and `--version` runs successfully. Parsing then
exposed EDG's host-alignment guard: absent native CPU macros had selected the
four-byte pointer default despite the portable eight-byte representation.

The self profile now sets six audited EDG configuration facts explicitly,
preserving the original x86-64 output target without implying a native host OS.
Comparing all configuration dumps found only these corrections, the intentional
mmap/PCH fallback settings, and an equivalent integer expression. Real EDG
headers reproduce the old failure and pass pointer/prefix/constant layouts,
binary80/binary128 storage and Half arithmetic under JIT/NativeAOT with the new
profile. Full regeneration is required before claiming parser acceptance.
The 18-case execution runner includes half/bfloat frontend coverage; both
versions of the failing full output and all logs remain retained.
[Evidence](../evidence/self-host-profile-20261004.json).

### Runnable EDG self-translation (2026-10-04)

`edg-self-runtime-006` translates all 75 original C++14 `cpfe` units, including
the C backend, with selected musl and real minimal libc++ implementations.
There are zero unresolved externs. Strict C# and Linux-x64 NativeAOT builds pass;
the 38,391,792-byte native executable needs no .NET installation. Its 57,803
owner methods fit the CLR limit. Build peaks stayed below 18 GiB measured RSS.
SDK analyzers were disabled for the full builds; C# and ILCompiler warnings were
errors. All intermediates and prior failures remain retained.

Both JIT and NativeAOT pass 18 native-oracle cases covering parsing,
preprocessing, diagnostics, dialects/extensions, templates, concepts, reflection,
half/bfloat arithmetic and coroutine input. Both C11 and C++20 C-backend outputs
are compiled and executed successfully. CLI/CX cases verify missing metadata
diagnostics, not complete platform-metadata support. Three original compiler
files (`cfe.c`, `types.c`, `ifc_modules_read.c`) parse with exact native parity
under a common explicit portable input profile. Normal EDG_BASE macro-file
preprocessing also matches native output in JIT and NativeAOT.

Windows .NET under Wine passes 11 actual compiler checks, including generated
C compilation/execution. Native Windows, Windows NativeAOT, macOS and ARM64
execution remain unverified. The full SQLite Unix JIT/AOT regression also passes
with the corrected backend configuration. EDG review aliases now point to the
working project; `nativeaot/Generated` exposes the published executable.

This completes the defined original-compiler self-translation acceptance. It
does not translate the added C# backend itself or imply complete libc++/POSIX
coverage. `Run` retains EDG's process-exit behavior; use a subprocess per compile.
[Usage and source review](self-csharp-review.md),
[evidence and measured timings](../evidence/self-execution-20261004.json).

### C# backend bootstrap dependency closure (2026-10-07)

The new self-host target includes `dot_gen_be`. Its libc++ provider adds pinned
upstream `string.cpp` and `stdexcept.cpp`, with the backend included after those
definitions in the primary translation unit. The other compiler files remain
separate. C++17 satisfies upstream string's `to_chars` requirement.

A focused real-provider fixture passes the native C++ oracle, strict translation
with no unresolved imports, JIT and Linux-x64 NativeAOT. It covers static
map/set/string lifetimes, vector/stable_sort, allocated string edits/moves,
integer formatting limits and typed range/length exceptions. The original EDG
C backend also accepts and lowers the combined provider/consumer. This is
dependency verification, not yet full compiler bootstrap acceptance.
[Evidence](../evidence/self-dot-libcxx-20261007.json).

`translate_self.py --program cpfe-dot` now selects the 77-source target and
records a fixed UTC build timestamp and exact frontend invocation. Its first
full native emission succeeds with no unresolved imports: 263,127,055 raw bytes
packaged into 576 C# files. `self_bootstrap_verify.py` compares raw binary output
without normalization and establishes native repeatability before comparing
the translated compiler. Full generated-compiler build and execution remain
pending at this emission milestone.
[Translation evidence](../evidence/self-dot-translation-20261007.json).

Three full native emissions are byte-identical (SHA-256
`215ac18729872a0c3bc279aa13b5cc54f6f8e91e3f7d0793cc3cadc32e125c85`).
The first C# build exposes 40 CS7013 diagnostics for oversized expanded template
names. Names over 900 encoded bytes now retain a readable prefix/suffix and a
stable full-spelling hash; the collision allocator and full source maps remain
in use. Ordinary names are unchanged. Nine focused checks pass, including a
native oracle, deliberate compact-name collisions, static/instance JIT and
NativeAOT, and exact raw repeatability. The full compiler is being regenerated;
its large owner's method-count limit is handled separately in packaging.
[Name-limit evidence](../evidence/source-name-limits-20261007.json).

The Roslyn packaging tool now moves eligible static synthetic helpers to nested
classes when an owner approaches the CLR method limit. The first full output's
conservative budget falls from 74,990 to 61,000, with 10,000 and 3,990 helpers in
two nested classes. Public/source methods and storage stay in their original
owner; static initialization and instance ownership are preserved. Calls remain
direct and structurally qualified, so separate translated owners do not import
conflicting helper names. Focused split-file, static/instance, two-owner,
function-pointer and initialization tests pass under JIT/NativeAOT; all six
existing formatter regressions also pass. The driver enables this automatically
and records counts/provenance in `methodSharding`.
[Method-limit evidence](../evidence/method-shards-20261007.json).

Full packaging exposed Roslyn parser list-builder reuse after the multi-million
element literal pool: methods appended after it repeatedly cleared an array of
5,382,648 references. Helper classes now precede the existing owner declarations,
keeping literal construction last without reordering any storage/constructor.
The focused static/instance JIT/NativeAOT checks pass again, and full packaging
finishes in 170.968 seconds. The regenerated 260,596,266-byte raw output is
identical across native repeats (SHA-256
`d50d43f9835b962d665b27acc49297392cc582e52497cbc9af0195124bc4bff6`).
Its main owner budgets 61,000 slots after moving 13,995 helpers. The first stuck
formatter's heap/stack evidence and raw inputs remain retained.
[Placement evidence](../evidence/method-shard-placement-20261007.json).

The bootstrap harness can reuse a verified native-only baseline, checking exact
arguments/environment/cwd, current native executable hash, and all retained
output hashes, sizes and actual byte equality before starting the translated
compiler. Eighteen acceptance/tamper-rejection checks pass. Source/include inputs
must remain frozen between stages, as with the original sequential replay.
[Baseline reuse evidence](../evidence/bootstrap-baseline-reuse-20261007.json).

The backend-inclusive `edg-dot-bootstrap-002` compiler now builds with zero C#
warnings/errors in 476.311 seconds (19,536,396,288-byte peak process-tree RSS).
It loads and reports EDG 7.0 with the pinned build timestamp. Its first C++
virtual/exception fixture emits exactly the native backend's 55,870 bytes,
including identical stderr and exit status (SHA-256
`87b1ec82f8840ada6a62b57c6b0442bfa0c18c17002ba4493d6986a2ee502ee2`).
Frozen compiler/backend/header/original-source hashes were rechecked before
starting the full 290-input bootstrap. Full bootstrap and NativeAOT acceptance
remain pending at this runnable-compiler milestone.
[Runnable compiler evidence](../evidence/self-dot-execution-20261007.json).

The first full translated bootstrap completes in 317.259 seconds with identical
178,058-byte diagnostics but non-identical C#: native 260,596,266 bytes versus
translated 260,596,181 bytes. Investigation found a real backend bug: repetition
compression called `compare_constants`, whose address comparison can follow
frontend-only correspondence records after `fe_wrapup` frees that region. Two
distinct relocation rows were incorrectly merged, shifting many generated IDs.
The original EDG implementation and C backend confirm the lifetime contract;
no EDG source change is needed.

Compression now uses a backend-local comparison of surviving lowered types,
representations and direct relocation identities. Six focused checks pass:
native two-TU behavior, static/instance JIT and NativeAOT, valid same-entity
compression, distinct globals/offsets/function addresses, signed zero/unions,
and a direct comparator with poisoned correspondence pointers. Corrected full
bootstrap is in progress. The earlier compiler also builds under NativeAOT and
passes the small byte-identical emission fixture; this does not validate its
known incorrect full initializer output.
[Initializer identity evidence](../evidence/initializer-identity-20261007.json).

The corrected `edg-dot-bootstrap-003` completes the requested full experiment.
Native EDG translates the 77-source backend-inclusive compiler plus its provider
inputs; the resulting C# builds and publishes with Linux-x64 NativeAOT in
537.398 seconds, with warnings treated as errors (analyzers disabled) and a
19,242,668,032-byte peak process-tree RSS. Both JIT and NativeAOT versions emit
byte-identical C# for the small virtual-function/exception fixture. The NativeAOT
compiler then retranslates all 290 direct inputs in 310.941 seconds, exiting 0.
Frozen source/include hashes are unchanged before and after the experiment.

**The full raw output is not byte-identical.** Two native runs produce
260,609,296 bytes (SHA-256
`3a4328f7da3600d7eb410d609107aef9bb0d65cb712eaaff6175e341498defb4`);
the translated compiler produces 260,609,297 bytes (SHA-256
`94303be81bef61c8e2c687a1f8ee4d122f8191aca6b3a924769860fe50c9a5b0`).
All 178,058 diagnostic bytes match. The one-byte length difference does not
describe the number of changed bytes. The comparison harness correctly reports
failure, with no normalization or packaging applied to either raw stream.

Both full outputs now preserve the formerly incorrect `saved_vars` rows 7/8 as
separate relocations to offsets 6690344/6690352. Both have 3,627,849 lines and
identical multisets of source-map/public-type records. Remaining differences
include synthetic helper/type discovery order, source-map ordering and literal
packing. Independent small reductions demonstrate stateful emitter calls in
C++ string-concatenation expressions with unspecified operand evaluation order;
native and translated evaluation orders can legally produce different IDs and
literal layouts. Both outputs of the temporary/conditional-string reduction
build and execute. This explains demonstrated differences, not a proof that
every remaining difference in the full program is semantically harmless.

The second-generation full C# is retained but has not been built or executed.
Review aliases remain on the previously verified original compiler. A future
byte-stable emitter needs explicit sequencing of stateful emission/discovery
calls; that is separate from changing EDG language semantics or normalizing the
comparison. [Full bootstrap evidence](../evidence/self-dot-bootstrap-20261007.json).

## Picotls real-project translation (2026-10-07)

The next target reuses the existing immutable picotls revision
`3598470df01264da85157025ed10db0f7e103790` and BCL crypto provider from the dotcc
campaign. `picotls/edg/translate.py` verifies every archived upstream file and
translates the unchanged core with the existing authored host/QUIC boundaries,
musl and portable POSIX, preserving `Managed.Security.PicoTls` and public API
names. EDG output stays separate from the existing dotcc port.

The first missing dependency was an unconditional pthread header include.
Logging is disabled in the selected profile, so no pthread calls are required;
the new header provides declarations and unresolved calls are explicitly
rejected. Portable socket/address headers and a strict BCL-span `inet_pton`
implementation support the core's actual IPv4/IPv6 name detection. The parser
matches native libc for 1,186 inputs under JIT and NativeAOT, including malformed
addresses, unchanged destination storage and owner/thread errno. Translated
static/instance binding tests and unbound-pthread rejection also pass.

All three original core C files pass a native/JIT/NativeAOT exact-output fixture
covering aligned buffers, ASN.1 serialization, QUIC integer boundaries and
truncations, base64, memory callbacks, escaping, IP recognition and real-file
PEM decoding. The managed-library translation with host boundaries also builds
with warnings treated as errors and no unbound externs. Provider integration,
ABI and TLS handshake verification remain pending at this milestone.
[Core evidence](../evidence/picotls-core-20261007.json).

The final picotls library (`picotls-edg-004`) and shared BCL provider now pass
JIT and Linux-x64 NativeAOT acceptance. Each mode passes 92 native ABI checks,
232 upstream vector checks, 2,962 provider checks (including 124 allocation-failure
boundaries and two ticket-clone failures), and 9,963 TLS assertions. Both modes
also pass all 56 independent-peer cases against native picotls/OpenSSL and
.NET SslStream: 112 final peer executions total. These include both client/server
roles, RSA/ECDSA authentication, AES-128/256-GCM, mutual authentication, large
records, key updates and certificate/name/ALPN rejection. Generated and authored
source integrity checks cover 312 files after execution.

Provider integration retains the existing implementation as its single source:
an auditable recipe adapts C tag/typedef spellings, signed-char boundary casts,
inline-array addresses and one anonymous-union field reference in authored
callers. Generated C# is linked unchanged. A partial owner bridge uses the
translated allocation registry for exchanged buffers; process-lifetime names
use pinned arrays. The peer runner accepts explicit JIT/AOT command prefixes
without rebuilding or replacing the existing dotcc port. No additional backend
language fix was needed for this target.

Review sources are available through `artifacts/review/picotls/`. The selected
existing TLS 1.3 profile is verified on Linux x64; optional logging/DTrace,
other platforms, TLS 1.2, 0-RTT and full QUIC transport are not covered by this
acceptance. [Port/build instructions](../../picotls/edg/README.md) and
[TLS acceptance evidence](../evidence/picotls-tls-20261007.json).

Picotls and the generic musl driver now default to provider-only reachability
pruning. `--libc-pruning all` explicitly retains the complete provider. An exact
manifest of selected musl implementation files defines which
entities are eligible; all emitted project definitions remain roots. EDG's IL
walker follows direct references, function-pointer constants, variable/TLS
initializers and cleanup metadata, with explicit edges for backend-resolved
builtins and conservative startup/TLS/finalizer roots. EDG needed flags and
upstream semantics are unchanged. This is not a general C++ archive linker.

The picotls result shrinks from 286 to 72 generated C# files and 4,452,485 to
3,299,933 bytes. It retains 20/449 provider routines and 4/66 provider variables;
unused `c32rtomb` disappears, while all 102 public picotls names and 240 project
function names remain. Fourteen C/C++ static/instance/default compatibility
checks pass, along with an actual musl C++ two-owner JIT/AOT fixture and a used
`c32rtomb` UTF-8 test. The trimmed picotls library passes the full ABI, upstream,
provider and TLS JIT/AOT suites plus all 56 independent-peer cases per mode.

`artifacts/review/picotls/` points to the trimmed output. Every provider input is
still parsed; this change reduces generated output. A referenced authored runtime
is a smaller next step than separately compiled musl, which needs a stable
owner/allocation/TLS/callback/type ABI. Neither shared assembly is implemented
here. [Pruning evidence](../evidence/picotls-pruning-20261007.json).

### Constant macro probe cleanup (2026-10-07)

Macro exports already promoted EDG-folded integral values to public C# constants,
but retained the private probe body and its lifecycle boundary wrapper.
Packaging now removes both when no other emitted member references them, and
removes their source-navigation entries. Nonconstant properties and function
macros retain their adapters; raw output and native probes remain inspectable.

The refreshed picotls review has all 17 selected constants and removes 34 unused
methods, reducing packaged C# from 3,299,933 bytes / 72 files to 3,288,178 bytes /
71 files. Nine macro checks pass, including C/static and C++/instance JIT and
NativeAOT consumers. The regenerated library passes strict compilation and the
picotls ABI, upstream, provider and TLS suites in JIT and Linux-x64 NativeAOT.
Independent peers were not rerun for this cleanup.
[Evidence](../evidence/macro-probe-cleanup-20261007.json).


## Semantic export: normalized SQLite packages (2026-10-07)

The native semantic exporter now writes SQLite directly. Base nodes, typed
node-kind tables, shared literal payloads and ordered audit edges replace the
whole-graph JSON file. Source snapshots and exact producer metadata are embedded
in the sealed database. Compact hash buckets compare exact bytes before
interning, preserving distinct semantic identities and exact integer payloads.
The C# consumer uses indexed access and bounded caches, with legacy JSON input
compatibility and NativeAOT support through the platform SQLite library.

The SQLite + memory-VFS package reproduces all 3,171 prior generated C#/project
files byte-for-byte. Native/JIT/NativeAOT execution matches all 43 project and
67 expression observations. The actual dot-backend graph is validated through
the database reader without whole-file JSON materialization. Storage corruption,
rehashed malformed records, exact payloads, hash collisions and legacy reader/
emitter behavior are covered by retained regression results.

This changes storage, not C++ semantic coverage or cross-unit linking. The prior
77-unit EDG audit remains evidence for that prior producer; this migration's
large production fixtures are SQLite and the actual dot backend.
[Storage contract and measurements](semantic-export-storage.md),
[tracked evidence](../semantic-export/evidence-sqlite-storage.json).


## Semantic export: typed native SQLite writer (2026-10-07)

Removed the native per-record JSON serialization/parser bridge. Typed values
now carry scalar fields into SQLite bindings, with typed position/range and
macro records and explicit ordered edges. Final structured literal/metadata
JSON encoding remains in storage schema v1; no native record parsing remains.
The schema, semantic facts and C# consumer API remain compatible.

Full streaming comparisons preserve all 1,205,132 SQLite + memory-VFS nodes and
3,144,305 dot nodes, plus macros, diagnostics, metadata and source snapshots.
Only the three explicitly identified frontend clock records differ per package.
The dot baseline and typed export use the same frozen current source tree.
All 3,171 generated SQLite C#/project files match the earlier native/JIT/NativeAOT
execution baseline. Both reader modes pass 58 storage checks, and NativeAOT
validates the complete new dot package. The native regression adds exact signed/
unsigned boundaries, duplicate field ordering and escaped typed positions.
Review links now expose the new matching database and generated C# artifacts.
[Typed writer evidence](../semantic-export/evidence-typed-writer.json).

## MsQuic with translated picotls (2026-10-07)

The EDG port translates the existing dotcc campaign's 43 unchanged upstream C
units and four managed-host adapter/portable units, plus one explicit managed
API root unit. It links unchanged generated C# to copies of the existing authored
BCL host, packet crypto, picotls adapter and owning QUIC API, with recorded ABI
spelling adaptations. Both MsQuic and picotls cores are EDG-generated. PAL buffers
and objects use tracked 16-byte-aligned NativeMemory allocations; source globals
and existing managed asynchronous buffer ownership keep their separate lifetimes.

Prerequisites added Microsoft anonymous struct syntax selection, legacy GNU
`__sync_*` atomics on the shared atomic runtime, and pinned musl byte-order/address
formatting helpers. Backend fixes consume retained struct/array designators,
preserve materialized aggregate slots when naming locals, and prevent repeated
comma effects when planning aggregate addresses. Stock C-backend comparisons and
native/JIT/AOT regression results are retained; no frontend changes were made.
One broader const-folded designator discrepancy was observed identically in stock
C and C# and is documented in the backend evidence without claiming resolution.

Final MsQuic `011` and picotls `002` pass strict builds. Acceptance covers 65
native ABI observations and 33 allocator checks per runtime, configuration,
IPv4/IPv6 stream/lifetime, AES-128/256 key-update/DATAGRAM and resumption controls
in JIT and NativeAOT. All 16 independent aioquic role/family/cipher/runtime cases
pass with payload, FIN, authentication and ownership checks. The regenerated
picotls passes its standalone ABI/upstream/provider/TLS JIT/AOT suites. Final
provenance and dependency audits pass. Linux x64 is executed; broader dotcc
qualification and other OS/architecture runs are not claimed.

[Port recipes and review links](../../msquic/edg/README.md),
[final evidence](../evidence/msquic-20261007.json),
[prerequisites](../evidence/msquic-prerequisites-20261007.json),
[backend fixes](../evidence/msquic-backend-20261007.json).

## DuckDB source and runtime bring-up (2026-10-07)

Pinned DuckDB v1.5.6 with all seven bundled static extensions and their source
dependencies. Dynamic extension loading, autoloading and installation are off;
the optional native allocator uses the shared NativeMemory-backed allocation
contract instead. The native C++20 reference builds and passes 17 selected
upstream cases (2,848 assertions) covering C API/SQL and bundled extensions.
The unchanged upstream SQLLogicTest runner and a small reporting adapter reject
skips, empty runs and expected failures as acceptance results.

Shared prerequisites now include pinned musl string duplication, realpath,
64-bit stdio offsets, process-local unnamed semaphores, physical memory queries,
memory locking/discard, filesystem statistics, terminal windows and scheduler
queries. Focused native/JIT/NativeAOT checks pass on Linux x64, including instance
ownership, real kernel locks and zero-fill discard. Other execution platforms
remain unverified. Unsupported cross-process semaphore and nonportable discard
cases return explicit errors.

The generic source planner now honors EDG's right-to-left operator-function call
ordering, following the stock C backend. Ten native/stock-C traces match static
and instance JIT/AOT. A broad real libc++ implementation profile passes native,
JIT and NativeAOT with streams, regex, threading, random generation, arithmetic
sorts, floating charconv and typed exceptions, using a C++17 private-source
compatibility overlay and one primary implementation/consumer TU.

Full DuckDB C# execution is not yet established. Separately merged libc++
consumers reach a correspondence assertion also reproduced by the stock C
backend on identical preprocessed inputs. Assembly alternatives are being
investigated without editing the frontend. The source-review link currently
contains a partial Hugeint.cpp translation with five unresolved DuckDB contracts,
not a runnable full database. Input preparation retains all source, C++/C#,
commands, compiler snapshots and failure evidence.

[DuckDB recipes](../../duckdb/README.md),
[first runtime evidence](../evidence/duckdb-runtime-prerequisites-20261007.json).

### DuckDB source assembly follow-up

The real libc++ provider now uses one tracked header configuration across its
implementation and consumers. Native ABI tags are disabled through libc++'s
provided opt-out, and .NET chooses inlining. Preprocessing C++ inputs before
the final merge prevents libc++ C-header wrappers from leaking into the musl
implementation. Repeatable primary-TU and separate-consumer checks pass native,
JIT and NativeAOT. The separate fixture still requires two explicit definition
retention anchors; these are not installed in the general DuckDB provider.

An isolated build using EDG's existing source-sequence/template-retention
configuration did not resolve the general correspondence failure, including
with the stock C backend. That configuration is not adopted. Upstream DuckDB
amalgamation is being tested as an alternative source assembly.

[Source assembly evidence](../evidence/duckdb-source-assembly-20261007.json).

The C++17 native reference now passes the same 17 upstream cases and 2,848
assertions as the earlier C++20 build (RE2 retains its explicit C++11 override).
The translation driver supports verified reuse of completed application
preprocessing, including source/flag/header/environment/compiler fingerprints
and retained-input hashes. A later code-generation failure does not discard a
valid preprocessing result. Provider inputs are preprocessed afresh, and the
final invocation keeps libc++ include paths away from the musl C sources.
Fresh/reuse CLI checks pass on a real DuckDB TU plus the real provider; this is
input-pipeline evidence, not DuckDB execution.

Full-library preprocessing and cache reuse now pass for all 2,207 selected
library TUs plus the freshly staged libc++ provider. The cache verifies 5,333
application dependencies and 972 provider dependencies. Eighteen focused
provenance checks pass. An explicit preprocessing compiler can remain frozen
while the separately recorded code-generation compiler changes, so backend
iterations need not repeat preprocessing.

PCG consumes compiler date/time macros; reproducible preprocessing therefore
uses an explicit epoch (the verified run uses epoch zero). This is a build
policy, not DuckDB's release date, and the existing native baseline did not use
that clock. Volatile inputs without a fixed clock remain ineligible for reuse.
These receipts establish complete input preparation, not generated execution.

[Complete preprocessing evidence](../evidence/duckdb-preprocessing-20261007.json).

### DuckDB libc++ storage views

Resolved GNU variable aliases and explicit assembler-labelled extern views now
share the canonical object's global/TLS allocation and initialization. Provider
pruning retains storage reached through an assembler symbol. GNU `__thread`
storage follows the stock C backend's modifier handling. Unsupported hardware
register bindings reject explicitly. Twenty-six focused verification groups
pass, including native/stock-C comparisons, static/instance JIT and NativeAOT.

The private libc++ stream implementation uses explicit storage views to retain
upstream construction, initialization order and flushing when assembled with
its consumers. All eight standard streams pass native comparisons in static
and instance JIT/AOT; the broader provider regression also passes. The scoped
locale implementation macros no longer replace DuckDB's `ERA` enum member.
No frontend semantics changed. GNU alias multi-TU constraints and optimized
native unity assumptions remain documented limitations, not resolved features.

This milestone is verified on Linux x64. Full DuckDB C# build/execution remains
outstanding; the core amalgamation emission is a separate investigation.

[Storage-view and stream evidence](../evidence/duckdb-storage-views-20261007.json).

The tracked DuckDB amalgamation recipe now records the full configured closure:
1,476 core TUs represented by the upstream amalgamation, plus 731 retained
preprocessed TUs, with all seven static extensions and their generated loader.
Three independent assemblies produce identical raw and absolute-line C++/header
bytes. Combined core/provider preprocessing succeeds with the fixed epoch and
source paths matching the retained inputs. Macro visibility differences remain
explicit in the composition receipt; no whole-program or execution pass is
claimed by this input-only validation.

A native macro probe establishes the fixed-clock and source-prefix mapping
needed to match PCG's parsed constants. The helper is unused in pinned DuckDB;
the existing native behavioral baseline retains its original clock provenance.

[Complete composition evidence](../evidence/duckdb-composition-20261007.json).

The first whole-core emission reached the C# backend after full template
instantiation and lowering, then rejected `duckdb_fsst_export` despite its
definition being present. A reduced case reproduced the difference from the
stock C backend: separate namespace declarations with C linkage share one
linker identity. Our backend now associates exact external symbols with their
source definitions, checks routine-type compatibility, and uses the canonical
definition for calls and function pointers. Pruning retains the linked body,
and redundant declarations do not consume its original managed name.

Native, stock-C, static/instance JIT/AOT linkage checks pass, including aggregate
returns, variadics, pointer identity and invalid-binding rejection. The broader
libc++ provider regression also passes. The driver can record stock compiler
timing diagnostics; enabling them produces identical raw C# in its smoke check.
Full DuckDB composition and execution still require the next attempt.

[Routine-linkage evidence](../evidence/duckdb-routine-linkage-20261007.json).

The 732-input full composition was stopped by the coordinator after 28 minutes
at about 64 GiB resident memory, still early in the remaining input list. The
automatic memory guard did not trigger. Inputs, resource samples and diagnostics
are retained. Two declaration-correspondence diagnostics (`NumericCast` and a
libc++ `pair` assignment) require separate source/configuration investigation;
the resource stop does not establish an EDG correctness failure.

An exact upstream-unity fallback is prepared: 29 complete native unity groups
cover 557 remaining TUs, with 174 kept separate and the original 1,476-TU core.
This reduces the final input count to 204 and preprocessed bytes from 1.52 GiB
to 516 MiB. All 2,207 original library TUs occur exactly once; all seven static
extensions remain included. Preprocessing passes, but the full merge has not
run and no full DuckDB C# execution is claimed.

[Input-scaling evidence](../evidence/duckdb-input-scaling-20261007.json).

### Lowered-IL checkpoint and replay for DuckDB retries

The driver now supports optional stock-IL writer/reader tools and verified
checkpoints. A completed frontend/lowering result survives backend failure;
compatible backend-only updates can emit again without parsing or instantiating
the program. Source/header hashes, target/configuration, semantic options,
emission environment, provider/pruning and macro-export metadata are verified.
This binary cache is host/configuration-specific, not the semantic-tree export.

Our emitter's primitive factories depended on frontend-only caches. Read-only
backend primitive views now preserve the stock promotion rules without those
allocations; alignment and target/emulation context are retained. No frontend
semantics changed. Twenty-five replay checks pass, including 16 old-compiler
raw-output parity pairs and C/C++ primitive JIT/AOT execution. The rebuilt main
compiler matches 22 retained raw outputs. Existing C11, GNU atomic and source
expression suites pass. A real 223-TU musl/macro checkpoint that failed emission
now succeeds through a corrected compatible reader without reparsing.

The existing C++20 enum-suite predefined-macro failure reproduces with both the
old and new compiler and remains a separate limitation. This milestone verifies
checkpoint infrastructure and focused execution, not full DuckDB translation.

[Checkpoint/replay evidence](../evidence/duckdb-il-replay-20261007.json).

The verified unity fallback now has a tracked recipe, `duckdb/edg/unity.py`,
with an explicit native `--unity --configure-only` inventory option. It checks
the acquired/staged source mapping, exact group include order and frontend
flags, generated code, source coverage and retained input hashes. Mixed core/
remaining groups are kept separate. Fourteen focused checks pass, including
real EDG preprocessing; a full retained-source inventory confirms the expected
204 planned inputs. This does not claim a full semantic merge or execution.

[Unity-recipe evidence](../evidence/duckdb-unity-recipe-20261007.json).

### DuckDB source correspondence adaptations

New portable trees now rename only the unused private `NumericCast` struct; a
complete token audit preserves all 640 calls to the public function template
and rejects replacement-name collisions. A second adaptation uses unevaluated
`sizeof`/`static_assert` to select the existing `pair<string, LogicalType>` copy
assignment consistently across TUs. It adds no runtime code and leaves libc++
and EDG unchanged. Reduced native and current stock-C emission/build/run checks
pass. Actual DuckDB headers pass native syntax and semantic-merge checks.

The broader header reduction's later stock-C `read_memory_region` failure is
retained separately and is not described as a lowering pass. Earlier frozen
source trees remain intact; fresh preprocessing uses a newly staged tree.

[Source-adaptation evidence](../evidence/duckdb-correspondence-adaptations-20261007.json).

Fresh preprocessing with both correspondence adaptations passes for every
selected library TU, with strict unchanged-input verification. The tracked
unity recipe produces the corrected 204-input composition, preserving exact
2,207-source coverage and all seven static extensions. The real-header pair
reduction also writes/replays successfully through our checkpoint tools, with
raw output identical to direct emission; its partial extern contracts still
make this emission-only evidence. The full corrected checkpoint attempt is
recorded separately in `duckdb-complete-checkpoint-001`.

[Corrected-input evidence](../evidence/duckdb-corrected-inputs-20261007.json).

The first corrected checkpoint attempt was stopped after five bundled-fmt
correspondence diagnostics in the first unity group. The first error appeared
at about 220 seconds; the coordinator stopped the run at 285 seconds, with
12.2 GiB peak compiler RSS. No memory guard fired. Earlier `NumericCast` and
`LogicalType` pair diagnostics did not reappear before this stop. Inputs remain
unchanged; the writer removed its incomplete IL on error, so no usable full
checkpoint or generated DuckDB C# is claimed. The fmt source-assembly mismatch
is the next reduced investigation.

[Corrected checkpoint attempt](../evidence/duckdb-checkpoint-attempt-20261007.json).

The fmt diagnostics reduce to unnamed-enum constants in dependent expressions.
Portable staging now spells two existing integral conversions explicitly; enum
declarations/types, packing boundary, values and layouts remain unchanged.
Native original/adapted and actual fmt single-TU stock-C execution agree for
0, 1, 11, 12 and 13 arguments. A tiny two-TU reduction emits, compiles and runs
through stock C; actual fmt two-TU semantic merge passes. The later stock-C
reader failure persists after rebuilding all stock compiler objects and remains
a separate limitation. Our checkpoint writer/reader handles the same fmt
inputs successfully and matches direct C# emission byte-for-byte (one partial
provider symbol, so this is emission-only evidence).

[Fmt adaptation evidence](../evidence/duckdb-fmt-adaptation-20261007.json).

The next full attempt passed core-functions and Parquet, then stopped on two
different implementation-only `duckdb::ICUCalendarSub` class definitions in the
ICU date-add and date-subtract source files. The first diagnostic appeared at
46.5 minutes; peak compiler RSS was 54.8 GiB. The new launcher automatically
stopped its isolated process group on that explicit error; the memory guard
did not fire. All corrected input hashes remained unchanged and all 2,207 TUs
remain selected. No usable full checkpoint or full generated C# resulted.
The next source adaptation will disambiguate the private class names while
preserving the public ICU registrations and algorithms.

[ICU checkpoint attempt](../evidence/duckdb-icu-checkpoint-attempt-20261007.json).

A separate stock `--remove_unneeded_entities` investigation rules it out as a
transparent memory fallback. It runs after all input parsing and can discard
the layout of a public type used only behind a pointer. The reduced program
still executes, but its generated public struct becomes empty. The option
remains disabled; passing execution alone would miss this API regression.

[Memory-pruning investigation](../evidence/duckdb-memory-pruning-20261007.json).

Portable staging now renames the interval-subtraction helper to
`ICUCalendarIntervalSub`, changing exactly seven code identifiers. The original
diagnostic literal, `date_sub` helper, public entry points and algorithms are
unchanged. Eighteen command checks reproduce the original rejection with the
stock C backend and verify the corrected actual-source semantic merge. Reduced
native and stock-C programs execute both distinct helper operations. A lexical
audit of 317 extension C++ files found no other conflicting source-defined
namespace/class-scope type names; headers and macro expansion are outside that
audit's scope. Full translated DuckDB execution remains outstanding.

[ICU adaptation evidence](../evidence/duckdb-icu-adaptation-20261007.json).

Fresh preprocessing with the ICU adaptation passes all 2,207 selected TUs and
strict unchanged-input checks. Core/provider composition 007 is ready. A
bounded native experiment groups the 13 ICU extension sources and 29 JSON
sources into two TUs, preserving source order and compiler flags. Only those
extension objects are replaced in copies of the native archives; all other
objects and the seven static extensions remain linked. The baseline inputs
remain unchanged. Native symbol sets match except for the two expected private
ICU specialization renames.

The candidate and baseline match on 115 fully executed upstream cases and
12,438 assertions each, with no failures or skips in that acceptance subset.
One additional JSON reader test matches through 291 assertions, then skips its
`require httpfs` suffix in both binaries; it remains explicitly partial and is
excluded from acceptance counts. Header inspection found an internal constant
null-string sentinel whose address can coalesce under unity; inspected uses
consume its bytes, with no observed pointer-identity dependency. This remains
a source-build experiment supported by native tests, not proof of universal
unity equivalence, EDG memory savings or translated execution.

[Additional unity native evidence](../evidence/duckdb-extra-unity-native-20261007.json).

The first composition check after native tests correctly rejected 32 generated
test-data files added under the acquired source directory. No original source
changed. The complete generated directory was moved into retained test artifacts
and its hashes verified; the acquisition once again matches its full manifest.
The failed composition attempt and the preservation receipt remain available.

The optional tracked recipe now requires a pinned additional-group manifest and
validates native observations, actual compiler standard/options, exact tested
wrapper include order, source hashes, successful command artifacts and input
coverage. The upstream-only default is unchanged. Ten new controls and fourteen
existing unity checks pass, as does independent review. The resulting corrected
composition has 164 inputs (31 groups plus core and 132 separate TUs), exactly
2,207 original TUs, and 358,571,518 preprocessed input bytes. All before/after
hashes remain unchanged. The next full frontend run will measure memory use.

The unchanged stock C frontend also accepts the two newly grouped ICU/JSON
inputs together with used-template instantiation enabled. This targeted
semantic merge passed in 36.6 seconds; it does not claim lowering or execution.

The native test harness now starts each runner in a separate artifact working
directory. Upstream caches `TEMP_DIR` before argument parsing and refreshes it
when switching to `--test-dir`; this ensures the requested artifact temporary
path is used. Full source-file/link snapshots now detect additions anywhere
in the acquisition. The actual JSON dates test passes 195 assertions without
skips, writes all eight generated JSON files into artifacts, and leaves all
14,536 pinned acquisition entries unchanged. No upstream code changed.

[Test temporary-directory evidence](../evidence/duckdb-test-tempdir-20261007.json).

While the 164-input full attempt continues unchanged, a Parquet fallback has
been validated natively. Eighteen standalone files cannot all be combined:
repeated inclusion of the bundled LZ4 header fails native and EDG syntax.
Keeping `column_reader.cpp` separate permits a 17-file group with unchanged
source-origin preprocessing. A native variant retaining all other Parquet
archive members and the tested ICU/JSON groups has identical strong symbols
and matches 179 fully executed upstream cases (14,615 assertions). Three
additional observations match but are excluded: two require-httpfs partial/
skipped cases and an upstream `mode skip` file with zero assertions, despite
the runner reporting a passed case and zero skips. Exact acquisition and
baseline inventories remain unchanged. This fallback is not yet adopted.

[Parquet fallback evidence](../evidence/duckdb-parquet-unity-fallback-20261007.json).

The 164-input attempt subsequently reached its unchanged 72 GiB RSS guard after
4,631 seconds. It had read all inputs and reached deferred storage-compression
template instantiation without frontend errors. It did not finish a usable IL
checkpoint or generate the full C# library. A separate core-only diagnostic had
already stopped at its own host-available-memory guard after 3,344 seconds; its
partial IL also remains unusable. All artifacts are retained. These are process
memory measurements, not an allocation-category profile.

[Resource-stop evidence](../evidence/duckdb-memory-stops-20261007.json).

The verified Parquet fallback is now adopted through ordered additional-group
receipts. Validation binds the reused ICU/JSON archives and their exact members,
baseline and actual link command. An explicitly excluded `mode skip` observation
requires pinned source and initial directive evidence, both matching raw modes,
and zero assertions; it never contributes accepted coverage. Seventeen focused
checks and fourteen original checks pass. Root reviewed the isolated recipe;
an additional agent review could not run because its model was at capacity.

Composition006 passes all 32 group preprocessings and preserves all 2,207 source
TUs exactly once as 148 inputs: combined core, 32 groups and 115 separate TUs.
Frozen inputs remain unchanged. Preprocessed input bytes fall from 358,571,518
to 287,907,570; this is not a measured compiler-memory saving. Full checkpoint
attempt004 is running with the same compiler, feature profile and memory guard.
No translated DuckDB build or execution is claimed.


A further fallback combines 114 core-functions sources currently represented by
20 upstream groups. Native and stock-EDG syntax probes pass; optimized native
code generation completes in 112.4 seconds below 3.5 GiB sampled process-group
RSS. Every source-origin token sequence and original source hash matches. The
native archive replacement removes exactly 114 members, retains the three
standalone extension objects and adds one tested combined object, while reusing
the exact accepted ICU/JSON/Parquet archives and all seven static extensions.
Strong global symbols are unchanged. Baseline and candidate match on 215 fully
executed upstream cases and 18,701 assertions each. The same three explicit
partial/nonexecuting observations remain outside acceptance counts.

The isolated recipe permits only explicitly pinned complete upstream-group
replacement and exact per-source target mappings. It binds the ordered prior
native receipts and their cumulative archive/link evidence. 26 focused controls
and 14 original recipe checks pass. Safe standalone line comments are accepted
only for audited native wrappers; line-splicing hazards remain rejected and
the upstream CMake wrapper parser stays strict. Actual receipt validation and
fresh preprocessing produce 129 inputs (core, 13 groups and 115 separate TUs),
covering all 2,207 sources exactly once with unchanged frozen inputs. Input bytes
fall to 215,232,430; this is not yet a compiler-memory measurement. The candidate
remains isolated and unadopted while full attempt 004 runs unchanged.

[Core-functions fallback evidence](../evidence/duckdb-core-unity-fallback-20261007.json).


Independent review found and corrected a wrapper-parser edge case: Python's
`splitlines()` treats VT/FF and Unicode separators as new lines, although those
characters do not end C++ `//` comments. The native candidate uses ordinary LF,
so its compiled bytes and native observations are unaffected. Candidate002
uses physical newlines and passes 27 focused controls (including hidden-include
rejection and CRLF acceptance), 14 existing controls, actual evidence validation
and fresh 129-input preprocessing. The reviewer approved the corrected recipe
with no remaining concrete findings. Candidate001 and its artifacts remain
intact. Attempt 005 is prepared against the reviewed composition but has not
been launched; attempt 004 continues unchanged.


A read-only upstream-runner audit identifies 148 additional C++17 translation
units (147 upstream sources and the reporter), making 2,355 configured sources
with the library. Upstream CMake groups 107 test sources into 26 wrappers and
keeps 41 separate. Preserve those wrappers and Catch counter/registration
boundaries; do not concatenate separately preprocessed test inputs. The current
portable test metadata retains an older standard/build-directory profile, so
future runner preparation must explicitly bind its C++17 and test/build/data
roots. The native-selected persistence/Catch sources also expose real
`fork`/`kill` and POSIX signal requirements. This audit changes no source and
adds no stubs or test exclusions; these are explicit future runner gates.
A library-only checkpoint cannot append the new test semantics through replay.

[Upstream runner assembly audit](../evidence/duckdb-runner-assembly-audit-20261007.json).


Full attempt004 stopped after 4,245 seconds on a frontend declaration-correspondence
error for `std::pair<string, Value>::operator=`, triggered by map concatenation's
converting `emplace_back`. Peak compiler RSS was 58.85 GiB; neither memory nor
time stopped the run. Before/after input hashes match and no usable checkpoint
exists. Prepared005 contains the same uncorrected headers and is explicitly
superseded, not launched. Actual Value cases are being reduced against stock C;
no EDG semantic change or diagnostic suppression is authorized.

With the active run finished, the independently reviewed complete-group
replacement recipe is now canonical. Its 27 focused controls and 14 original
checks pass at canonical paths. The source-target/ordered receipt contract and
physical-line parser correction are preserved; optional grouping never drops
configured sources. A new stage is required before another full invocation.

[Attempt004 diagnostic](../evidence/duckdb-checkpoint004-diagnostic-20261007.json).


The Value pair mismatch is reproduced in four actual-header two-TU cases using
the unchanged stock C frontend: copy assignment paired with size-only use,
default construction, converting construction, and vector emplace. Selecting
the existing copy-assignment signature in an unevaluated expression after the
complete Value declaration makes all four semantic merges pass. Original and
anchored native Value copy/self-copy/default/converting/emplace/layout probes
agree. A reduced nontrivial Value-like pair also emits, builds and runs through
the stock C backend. Actual-header C lowering separately reaches the previously
classified IL-read assertion; it is not counted as C execution acceptance.

The narrow adaptation is integrated into portable source preparation without
changing EDG or libc++. A fresh immutable stage and full preprocessing run are
being prepared; old input sets and superseded005 remain preserved.

Canonical pair verifier passes all 34 commands against the fresh stage,
including the existing LogicalType and real stock-C execution control.
[Value signature evidence](../evidence/duckdb-value-pair-adaptation-20261007.json).


Fresh Value-adapted preprocessing passes all 2,207 library sources and all
13 selected unity groups. Complete coverage is unchanged: 129 program inputs,
215,313,546 bytes, seven static extensions, every configured source exactly once.
A byte-hash comparison of both 14,535-file staged trees finds only the expected
Value header change. Full attempt006 is running with the same 72-GiB guard;
005 remains superseded and unlaunched. No usable checkpoint or translated
DuckDB execution is claimed.

[Fresh complete preparation](../evidence/duckdb-value-full-preparation-20261007.json).

Independent stage review verifies the complete normalized 2,355-unit/options/
extension/dependency metadata and all 2,433 C/C++ source-file hashes unchanged.
Only the expected Value header differs, matching the proved overlay exactly.


The isolated upstream-runner composition prototype passes 21 inventory and
negative controls. It retains the 129 library inputs and adds all 148 selected
test sources through 26 original upstream wrappers plus 41 standalone inputs:
196 inputs covering 2,355 source identities exactly once, provider once. Catch
implementation and reporter remain separate. This is inventory evidence only.

The next isolated preprocessing phase uses C++17/epoch zero, the frozen staged
source/data root and generated headers, an explicit runner build-directory
macro, and future separate writable native/JIT/AOT test directories. Existing
native baseline path/clock differences stay explicit; no matched-profile or
execution claim is implied. Real POSIX persistence/signal requirements remain.

[Runner composition inventory](../evidence/duckdb-runner-composition-inventory-20261007.json).

Runner preprocessing002 passes the first 60 inputs, then stops at the real
remote-optimizer test's missing `netdb.h`; six later inputs remain unattempted.
Post-failure full-stage, retained-library, dependency and fingerprint integrity
checks pass. That test also requires socket I/O, process creation/signals and
a native loadable optimizer demo. These remain explicit service/acceptance
gaps; no empty headers, fake-success imports or test pruning are introduced.
The active library checkpoint006 remains independent and unchanged.

The read-only runner service audit confirms six fork sites, preprocessed Catch
fatal-signal/alternate-stack handlers, and an unhidden remote-optimizer case
loading the native demo disabled by the selected static-extension profile.
BCL socket/resolver services are feasible; they cannot solve arbitrary managed
fork or make that native-demo case eligible. Exact source locations and current
service candidates are retained in the linked evidence. A bounded BCL socket
prototype may proceed in isolated artifacts while full006 inputs stay frozen.


Full006 stops after 3,867.5 seconds on another exact pair copy-signature
correspondence diagnostic, now `pair<string,string>` in JSON MultiFileFunction
construction. The Value case does not recur. Peak compiler RSS is 46.33 GiB;
neither time nor memory stopped the run. All before/after input hashes match.
No usable checkpoint or generated full library exists. The next work reduces
this concrete signature and broadens bounded pair preflight without modifying
EDG semantics or weakening diagnostics.

[Attempt006 diagnostic](../evidence/duckdb-checkpoint006-diagnostic-20261007.json).


The JSON pair reduction generalizes to eight nontrivial pairs in a ten-type
stock-C preflight. A named-alias-only rewrite does not resolve it. A single
unevaluated assertion selecting the existing copy-assignment signature resolves
all ten while preserving the original declaration/body/noexcept/layout. This
is a pinned libc++ vendor source adaptation; EDG remains unchanged.

Independent safety review found no candidate-only difference in the targeted
GCC/Clang C++17/20 matrix. Eight original/adapted native executable comparisons
are byte-identical; stock-C C++17 emission and execution pass. Focused two-TU
translated static/instance JIT and NativeAOT also pass. C++20's existing
predefined-macro conflict is retained as an explicit pre-source limitation.
Finite tests do not establish every possible template instantiation.

Fresh vendor acquisition and complete library/provider preprocessing now use
one identical adapted pair definition. No old preprocessed input is reused in
this new full-program composition. Full DuckDB emission and execution remain
unaccepted until the subsequent checkpoint/backend/project gates pass.

The reusable canonical pair verifier passes all 60 commands against the fresh
acquisition: two unaffected original passes, eight expected original
correspondence diagnostics, and ten adapted passes. Exact patch, complete
acquisition inventory, include order and per-input header origins are checked;
mixed-header provenance is rejected and before/after inputs are unchanged.
[Pair adaptation evidence](../evidence/duckdb-pair-signature-adaptation-20261007.json).

Fresh pipeline002 passes all 2,207 sources, provider and unity preprocessing
with unchanged fingerprints. The first preparation is retained as a provenance
rejection: the new verifier filename appeared in an inventoried directory during
that run, despite all source preprocessing succeeding. No rejected cache was
reused and no check was weakened. Final composition has 129 inputs, all seven
extensions and exact source coverage. An independent line-marker audit verifies
all 38 pair-containing inputs use only the same adapted vendor header.

Full checkpoint007 is now running with tools002 and the unchanged 72-GiB
compiler RSS guard. It has no elapsed-time cutoff. Driver/backend/provider
inputs are frozen; no full checkpoint or translated execution is claimed yet.


During frozen full007, isolated BCL socket imports and pinned musl address
parsers pass native/JIT/NativeAOT checks and independent review. A minimal
24-file bundle also passes registered-provider packaging tests in a separate
repository snapshot (224 selected musl TUs), existing network checks and the
93-check descriptor regression under both JIT and AOT. It remains unapplied
to canonical files until the active writer releases its freeze. Linux x64 is
the executed platform; parser weak-symbol/interposition scope stays explicit.

The isolated upstream-runner retry now passes 65 of 67 preprocessing inputs,
including the former netdb failure, then stops at termcolor's real host-platform
guard. All 148 test sources remain present; the last input is unattempted.
Reference library inputs are not claimed mergeable with changed header
inventories. Fork, fatal signals and native-demo loading remain separate gaps;
Catch's handler-disable configuration is reviewed only, not silently selected.
[Isolated network implementation evidence](../evidence/duckdb-socket-candidate-20261008.json).


The isolated termcolor managed-target source adapter now passes exact native
pipe/PTY comparisons and 14 staging controls, with independent review. It
preserves all manipulators and forcing and defines no global host-OS macro.
It remains unapplied while full007 runs. The runtime supplementary probe uses
the independently inventoried socket-bundle snapshot: native/JIT/AOT pipe
bytes match, but raw PTY parity fails because .NET emits a seven-byte keypad
initialization prefix on first write. This failure is retained; no filtering
or successful interactive-Windows claim is made. No translated C++ termcolor
or complete 67-input runner acceptance is claimed.
[Termcolor source-adaptation evidence](../evidence/duckdb-termcolor-adaptation-20261008.json).


Full checkpoint007 stopped after 4,489.8 seconds on the unchanged EDG
`check_correspondences` assertion at trans_copy.c:962: "primary entry should
not be canonical". Peak compiler RSS was 50.24 GiB; this was an explicit
diagnostic stop, not a memory/time guard. All input hashes remain unchanged.
The previous JSON pair correspondence diagnostic did not recur, but no usable
checkpoint, full C# output or execution was produced. Next work is a bounded
source/configuration reduction and original-C-backend comparison; EDG semantic
changes or suppressing this assertion are excluded. Queued source/runtime
bundles remain isolated while reproduction scheduling is established.
[Full007 diagnostic evidence](../evidence/duckdb-checkpoint007-diagnostic-20261008.json).


Isolated runner preprocessing005 now passes all 67 inputs (26 original
wrappers and 41 standalone inputs), covering all 148 test source identities
exactly once. Independent review verifies the sole termcolor header delta and
all dependency/clock/compiler/source receipts. Catch configuration is unchanged
in this preprocessing result. Old 129 library inputs remain reference-only and
nonmergeable under the new stage/header fingerprint; no semantic or execution
claim is made. Separately, a native Catch reporting profile disabling POSIX
signal and Windows SEH handlers preserves all 5,486 ordered registrations and
five selected cases with 797 assertions each. It is not canonically selected
and does not resolve fork or native-demo loading.
[Runner prerequisite evidence](../evidence/duckdb-runner-preprocessing-20261008.json).


An existing two-input case reproduces assertion962 under stock C and both
normal/diagnostic writers in roughly six seconds. Built-in DEBUG output names
`integral_constant<size_t, 2>::value`, whose primary entry lacks a definition.
This does not yet identify the full007 entity. A separate diagnostic full run
is active at `duckdb-correspondence-debug-full-001`, using unchanged source
and supported DEBUG=1 with the original constexpr-depth limit preserved. No
normal-reader compatibility or checkpoint acceptance is claimed. Generic
header retention candidates were rejected after valid class-valued template
uses exposed changed overload/initializer behavior. Only a restricted, isolated
integral/enum candidate is under investigation; canonical inputs remain frozen.
[Correspondence reduction evidence](../evidence/duckdb-correspondence-reduction-20261008.json).


The completed diagnostic compiler log identifies the full007 correspondence
entity as `pcg_detail::no_stream<uint64_t>::is_mcg`: the primary TU has no
definition, while secondary `unity-0012.ii` has a definition. This is distinct
from the earlier `integral_constant` small reduction. The compiler log ends
with assertion962 and compilation aborted. No compiler process remains.

The launcher did not finalize its original receipt; that receipt is preserved
and recovery records the exit code, exact finish time and launcher interruption
cause as unknown. All inventoried inputs are unchanged. No checkpoint or full
C# output is accepted. The next step is an exact PCG stock-C reduction, before
another full attempt. The isolated integral overlay remains unapplied, and
the compressed-pair address-retention experiment is explicitly rejected after
changing the validity of ordinary Clang18-valid source.
[Exact PCG diagnostic evidence](../evidence/duckdb-pcg-correspondence-20261008.json).


With full007 and its diagnostic writer terminal, the previously reviewed
termcolor staging adapter is now canonical. Thirteen canonical controls pass
against the exact pinned header, including drift and alias rejection. Existing
staged trees and preprocessed inputs are preserved; a fresh complete stage is
required for the next composition. This does not resolve interactive terminal
output differences or count as translated runner execution.
[Termcolor integration](../evidence/duckdb-termcolor-integration-20261008.json).


The reviewed 24-file socket/parser bundle is now integrated after the writer
freeze ended. The canonical compiler rebuild and all three focused suites pass:
55 registered socket/parser commands with C/C++ static/instance JIT and AOT,
existing native-differential network checks, and 93 descriptor checks under
each managed execution mode. The selected musl provider now contains 224 TUs.
The Linux x64 evidence does not establish other-platform execution, DNS
resolution, fork or fatal-signal support.

A fresh adoption audit checked all 24 baseline/payload files and 2,484 retained
evidence hashes. These header/runtime changes require new preprocessing and
checkpoint provenance; no previous DuckDB .ii is relabeled or reused as current.
[Canonical socket integration](../evidence/duckdb-socket-integration-20261008.json).

Fresh checkpoint writer/reader tools003 are built from the integrated backend.
All 25 replay checks pass, including NativeAOT. These establish tool
compatibility for the next attempt; no full DuckDB checkpoint yet exists.
[Fresh checkpoint tools](../evidence/duckdb-checkpoint-tools003-20261008.json).
