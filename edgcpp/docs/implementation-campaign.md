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
