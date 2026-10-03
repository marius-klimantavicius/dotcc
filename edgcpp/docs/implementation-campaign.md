# Full implementation campaign

This ledger tracks the user-authorized coordinator and implementation workers.
The complete contract remains [the backend plan](dot-gen-be-plan.md). A passing
increment is not completion of the plan. All verification inputs during the
campaign are synthetic or EDG tests; real-world acceptance resumes only after
all implementation gates are met. Preserve intermediates and per-run evidence.

## Ownership and current work

| Owner | Work | State |
| --- | --- | --- |
| Root | Embed upstream, portable target, multi-TU, atomics and libc++ audit | Embedding/portable ABI/atomics complete; libc++ thread integration in progress |
| Coordinator | Integration, import ABI, driver, lifecycle/heap adapters, ledger, builds/commits | In progress |
| Core language worker | Function-try-blocks, then member-pointer/full EH coverage | In progress |
| Libc worker | Audited BCL libc bridge, portable header subset, synthetic conformance | In progress |

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
- [ ] Only after all above: real-world project translation and validation.

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
