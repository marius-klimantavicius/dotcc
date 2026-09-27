# Translate QuickJS with DotCC

Status: P0–P6 passed with Atomics enabled, direct upstream inline exports and value-only branch-hint overrides.
Campaign: `quickjs/`. Updated: 2026-09-27.

This plan adopts [translation baseline v2](../../docs/plans/translation-baseline.md)
and the [shared framework](../../docs/campaigns.md). Project decisions and gates
are below; common repair and execution methodology stays in the baseline.
Current user instructions govern implementation, commits, branch work, and
delegation. The user authorized coordinated implementation on 2026-09-26.

## Outcome and scope

Translate the original Bellard QuickJS parser, bytecode compiler/interpreter,
objects, reference counting/cycle collection, BigInt, number conversion, Unicode,
and regular-expression implementations into `TranslatedQuickJs`. Deliver an owning
.NET API over the translated implementation using the shared runtime and BCL host
services. A native QuickJS binary is a test oracle only.

The separate consumer accepts `{"items":[2,3,5]}`, evaluates JavaScript that maps
each item through a managed `hostDouble` callback, and returns exactly
`{"values":[4,6,10],"sum":20}`. An in-memory ES module provides the summation
function. A Promise continuation produces the result through explicit job
draining. The consumer asserts the result, callback count, module resolution,
and clean disposal, then repeats with a fresh runtime.

| Scope | Features/APIs/workloads |
| --- | --- |
| Required for completion | Source evaluation, functions/closures, objects/arrays, exceptions, JSON, strings/Unicode, Number/BigInt, RegExp, Map/Set, typed arrays/ArrayBuffer/SharedArrayBuffer, Atomics operations and wait/notify, in-memory ES modules, Promise jobs; owning values/contexts/runtimes, managed callbacks, memory limit, safe recursion rejection and cooperative interruption. Preserve other reachable core algorithms and qualify the selected upstream cases. |
| Excluded from this product | `quickjs-libc.c`, delivered `qjs`/REPL and `qjsc`, `std`/`os` shell modules, native dynamic modules, filesystem/network/process APIs, workers, public bytecode import/export, browser/Node APIs, Intl; these expand the embedding or ABI contract. No full ECMAScript conformance claim. |
| Later extensions | Trusted same-version bytecode interchange, external Test262 qualification, OS module/event-loop integration, workers, Windows/macOS/arm64, performance campaigns. |
| Departures from the baseline | None for implementation/acceptance. Implementation uses the shared campaign framework; all required gates pass in the final receipt linked from `validation.md`. |

## Decisions and feasibility work

The following choices are implemented and validated by native controls, actual
translated execution and distribution audits. Evidence is linked in `validation.md`.

| ID | Decision | Choice and reason | Status; resolving evidence/gate |
| --- | --- | --- | --- |
| D1 | Source and dependencies | Official `2026-06-04` release, not QuickJS-NG or Micro QuickJS; exact archive/hash in `config/source.json`. Bundled test inputs use the same pin. No external product libraries beyond repository runtime/BCL. | Validated; pinned native controls pass 36/36 records. |
| D2 | Source/configuration closure | Five upstream units in `config/core-sources.txt`; header-inline embedding APIs are selected with `--export-inline` under their original C names; `embedded` profile in `config/profile.json`. GNU C11, LP64/unsigned-char headers, `_GNU_SOURCE`, metadata-derived `CONFIG_VERSION`; switch dispatch through `config/dotcc-overrides.json`, upstream `CONFIG_ATOMICS` remains enabled. | Five-unit translation, original-name inline exports and Atomics qualification pass in all four cells. |
| D3 | ABI and encodings | Linux x64 little-endian LP64: 32-bit int, 64-bit long/pointers/size_t, unsigned 8-bit plain char. The actual non-NaN-boxed `JSValue` is 16 bytes with tag offset 8; aggregate passing/returns, short BigInts and function pointers are probed. Explicit UTF-8 lengths at C boundary; preserve JavaScript UTF-16 code units including lone surrogates. | Validated; actual ABI and string/aggregate/callback tests pass in all four cells. |
| D4 | Host/dependency policy | Stable unmanaged allocations through shared runtime/BCL, exact allocator accounting, managed callbacks, clocks/timezone, math/libc bindings and interrupt service. No native QuickJS, alternate JS engine or project-specific platform import. | Validated; allocator/stack/math tests and actual PE/ELF dependency audit pass. |
| D5 | State/concurrency | One active entry per runtime, multiple contexts per runtime, independent runtimes may execute on separate threads. Reject cross-runtime values, concurrent same-runtime entry, and callback reentry into its active runtime. Drain active callbacks before disposal. | Validated; 23 lifecycle cases per cell cover ownership, class IDs, limits and concurrency. |
| D6 | Translation/delivery | Object emission/linking for five units; generated class `QuickJs` in `Managed.Interpreters`, assembly `TranslatedQuickJs`. Shared campaign defaults: 256 KB splitting (`--split=size --split-size=262144`), `--literal-pool` and `--deduplicate-inline`; standalone CLI defaults unchanged. Authored host sources linked from `src/Host/`; owning project `src/Managed.QuickJs/Managed.QuickJs.csproj`. | Passed with campaign defaults in final receipt `20260927-062444-292bece5`; 14 generated C# files per product. |
| D7 | Controls | Native C embedding harness using the same five units, ABI and effective adaptations; preserve stock native comparison for dispatch/adaptation checks. Assertions enabled; deliberate failing case proves harness failure propagation. | Validated; stock/embedded native controls and deliberate failure controls pass. |
| D8 | Execution matrix | `embedded` × raw/processed × JIT/NativeAOT × Linux x64, executing all four cells. Rooted AOT boundary harness plus normally trimmed separate consumer. | Passed in every required cell; other targets remain outside acceptance. |
| D9 | Tests | Selected bundled engine tests, ABI/host contracts, core features, ordinary errors/lifetimes, two independent runtimes and bounded cancellation tests. External Test262, custom fuzzing, exhaustive fault injection and extended stress excluded from initial acceptance. | Passed; 76 selected upstream functions plus cyclic module, authored and boundary suites. |

Source and profile metadata are under `config/`; the dispatch macro override is
declared in `config/dotcc-overrides.json`; retained reports prove it selects the
full engine switch branch during preprocessing. Text adaptations are recorded in
`config/adaptations.json` with unique local anchors and reasons. Actual translation preserves the upstream CONFIG_ATOMICS definition; the only
text adaptation is the native-oracle-only dispatch selection. Keep revision and
checksum literals out of scripts. Default hash policy
is `warn`, `off` is supported, and strict reproduction is optional. Local usable
edited sources remain valid inputs; report observed provenance honestly.

## Source, host and ownership contracts

The inspected upstream `Makefile` includes `quickjs-libc.o` in its library object
list. This embedding deliberately selects `quickjs.c`, `dtoa.c`, `libregexp.c`,
`libunicode.c`, and `cutils.c` and audits the resulting unresolved imports. Use the
archive's checked-in atom/opcode/Unicode headers. Unicode regeneration and its
extras archive, generated REPL bytecode, `qjsc`, `unicode_gen`, and `run-test262`
are not product prerequisites. Preserve upstream LICENSE and notices when
publishing generated products. See [source evidence](source.md).

Effective headers, GNU extensions, per-unit flags and target macros are fixed
by the recipe. Object linking preserves translation-unit-local storage and avoids
static-name collisions. Observed compiler/runtime failures were reduced, repaired
and regression-tested; [blockers.md](blockers.md) records the evidence. The
experiments below remain the validation contract for future changes.

| Boundary | Translated responsibility | Host/reused dependency | Ownership, errors and validation |
| --- | --- | --- | --- |
| Allocation and GC | QuickJS reference counts, cycle detection, object layout and memory-limit decisions | `JS_NewRuntime2` allocator callbacks; stable unmanaged storage and usable-size accounting | Pair realloc/free with the allocating owner; zero-size/overflow/failure semantics and alignment match C. Preserve old allocation on realloc failure. Check allocations return to baseline after cycles, exceptions and disposal. Default allocator imports must also resolve or be explicitly bound. |
| Values, strings and buffers | `JS_DupValue`/`JS_FreeValue`, atoms, string and ArrayBuffer semantics | Owning .NET handles; length-aware conversions; copy buffers by default | Document every consuming versus borrowing API, especially property setters; release `JS_ToCStringLen` storage with `JS_FreeCString`. No movable pointer escapes a call. Test embedded NUL, non-BMP text, lone surrogates, detached buffers and double disposal. |
| Managed callbacks | Function invocation, JS exception values, module resolution | Rooted context tokens and stable AOT-compatible thunks for `JSCFunction`, allocator, loader and interrupt signatures | Verify aggregate returns and canonical pointer identity; catch managed exceptions and translate them to JS errors. Release tokens only after callbacks stop. Arguments are borrowed; returned ownership is explicit. |
| Modules and jobs | Parse/link/evaluate modules; Promise reaction queue | In-memory source resolver; explicit `JS_ExecutePendingJob` loop and rejection reporting | No implicit host I/O or event loop. Unknown module throws; cache/normalize names per runtime, retain source buffers for the documented call lifetime, bound job drain and surface failures. |
| Time, numbers and randomness | Date semantics, numeric conversions, BigInt, PRNG algorithm | Shared libc/BCL math, clock and local-time services; controlled inputs in tests | UTC test environment; exact deterministic conversions and stated tolerances for platform libm results. Audit PRNG seeding; no cryptographic-randomness promise for `Math.random`. |
| Limits, shutdown and instances | Interrupt polls and exception unwind; context/runtime cleanup | Cancellation token checked by `JS_SetInterruptHandler`, callback drain and stack budget guard | Budget checks also cover parsing, module recursion, RegExp and job execution. Stop active work before freeing values, contexts, runtime and callback roots. Prove two in-process runtimes have separate state without a global execution lock. |

Allowed adaptations select existing configuration branches and bind host services
through extension points or typed overrides. For dispatch, use DotCC's
`--override-macro DIRECT_DISPATCH=0`, or the guarded equivalent in
[`config/dotcc-overrides.json`](../config/dotcc-overrides.json). Unlike ordinary
`-D`, a [macro override](../../docs/macro-overrides.md) replaces each active source
definition. The profile requires an object-like definition with original body
`1` and at least one match. Apply it to the `quickjs.c` object invocation only:
the other units do not define this macro and would fail `requireMatch`. Retain
`--override-report` evidence and check that preprocessing selects the switch
dispatcher. Do not combine a same-name `-D` with the override.

QuickJS branch hints use guarded macro overrides as well. The only upstream
`__builtin_expect` occurrences are the `likely`, `unlikely`, `js_likely` and
`js_unlikely` definitions, with literal expected values `1` or `0`. Their
replacement is `(!!(x))`: the builtin's first value is parenthesized and evaluated
once, and its effect-free expected argument is omitted. The non-GNU `(x)`
alternatives remain unchanged. Apply the shared hint rules to all five source
units; keep required dispatch and host-function guards on `quickjs.c` only.
The generated-source audit rejects surviving calls to `__builtin_expect`.

`CONFIG_ATOMICS` remains defined and enabled; no override or staged adaptation
disables it. Never define `__EMSCRIPTEN__` to select dispatch branches because
it also disables Atomics and stack checking. For a native compiler without DotCC overrides,
use a guarded staged dispatch selection in the matching native control and
compare it with stock native dispatch. Preserve acquired references. No emitted
C# edits or replacement interpreter. Any upstream bug repair gets its own record
and native reproducer.

`native-switch-dispatch` in `config/adaptations.json` is **tests only**: it is
applied only by the native oracle, whose C compiler does not implement DotCC's
macro override option. It is **not used for actual translation**. Actual object
emission keeps upstream dispatch definitions and applies the validated override.

`CONFIG_ATOMICS` remains enabled as requested. It supplies JavaScript `Atomics`,
pthread-backed wait/notify and the upstream class-ID mutex. The facade retains
upstream's default `can_block=false`; low-level embedders may explicitly enable
blocking with `JS_SetCanBlock`. Qualification covers every integer width,
shared-buffer lifetimes, bounded waits, cross-runtime notify and cleanup. An
observed narrow C11 load/store lowering gap was repaired in the shared compiler
using its existing same-width atomic runtime primitives. Native and translated
controls cover signed and unsigned 8/16-bit loads, stores and read-modify-write
operations with adjacent-memory guards.

| Risk | Smallest useful experiment | Observable success/failure | Dependent gate |
| --- | --- | --- | --- |
| Tagged aggregates and callbacks | Emit actual `quickjs.h` types and calls to real value constructors, getters, callback dispatch and frees; native sizeof/alignof/offset probes | Payload/tag bytes and returned semantics agree, including NaN, signed zero, pointers, short/heap BigInts; do not substitute mirror structs | P1/P2 |
| BigInt and number lowering | Translate actual arithmetic/conversion paths exercising `__int128`, carries, shifts, overflow and dtoa rounding | Exact integer/round-trip results against native across signed boundaries and short-to-heap transitions | P1/P2 |
| Dispatch, stack allocation and recursion | Emit real evaluator with upstream switch branch; probe `alloca`, parser recursion and native frame-address assumptions in JIT/AOT | Correct control flow and bounded JS exception before managed stack exhaustion; no acceptance with stack checks returning a constant | P1/P3 |
| Collection and accounting | Cycles, retained callback values, buffer release and memory-limit failure through actual engine | Correct finalization and no stale pointers, allocator mismatch or surviving owned allocation after teardown | P1/P3 |
| Shared state | Audit globals/function statics and `js_class_id_alloc`; concurrently exercise two runtimes and register host classes | Stable IDs and isolated state, callbacks and exceptions. Use the upstream Atomics-enabled class-ID mutex; no whole-engine lock | P1/P3 |
| Cancellation | Interrupt an infinite JS loop and bound recursive/RegExp work and Promise job drain | Documented error and cleanup within the harness deadline; timeout/crash is failure. Identify noninterruptible native/BCL host calls | P3/P4 |

The stack service uses a no-inline native-stack-pointer helper, real runtime
stack limits, requested-allocation space plus a 32 KiB reserve, and managed stack
availability checks. The facade defaults to a 256 KiB budget and permits 64–512
KiB. Parser, module, JavaScript recursion and RegExp/job interruption tests pass
in JIT and NativeAOT. See `api.md` for limits and callback cancellation behavior.
Unsafe translated code does not establish an isolation boundary for hostile scripts.

## Framework integration and deliverables

- `scripts/campaign.py` exports `Recipe("quickjs", "TranslatedQuickJs", ...)`,
  profile `embedded`, source metadata reader, project-only staging hooks, and suites.
- Final and raw products: `generated/TranslatedQuickJs/TranslatedQuickJs.csproj`
  and `generated/TranslatedQuickJs.Raw/TranslatedQuickJs.csproj`. No alternate
  profile is required; diagnostics cannot replace the default product.
- Root solution: `ManagedConsumer.slnx`, including processed product, owning API
  and `samples/ManagedConsumer/ManagedConsumer.csproj`. Consumer and facade use
  `QuickJsProject` to select the product form; the recipe declares that property.
- `src/Host/` holds host files linked into generated projects from their original
  paths; `src/Managed.QuickJs/` references the generated product. Authored edits must
  take effect on rebuild without regeneration. No copied authored implementation.
- `tests/NativeOracle/`, `tests/Abi/`, `tests/Behavior/`, `tests/Upstream/` and
  `tests/fixtures/` hold independent harnesses and fixed workloads. None becomes
  a library entrypoint. Full library rooting belongs to the ABI AOT harness.
- Prerequisites: repository .NET 10/C# 14 toolchain, Python 3.11+, Bash; native C
  compiler/libm and NativeAOT prerequisites on Linux x64. Source archive provides
  engine tests. No JS package manager, external peer or native JS runtime in delivery.

Campaign scripts must operate on Windows without administrator privileges or
Developer Mode. Staging and temporary verification use ordinary copies, without
host symlink requirements. This orchestration requirement does not claim Windows
QuickJS runtime qualification; the selected native/execution target remains Linux
x64 and its platform-specific prerequisites stay explicit.

After supplying the recipe and authored consumer, run the shared `layout --write`
to generate the solution and `{common,fetch,translate,build,test,verify,probe}.sh`.
Do not create another Python resolver, fetcher, publisher or central registry.
Use shared preparation/emission/raw-build/postprocess/processed-build/transactional
publication. A failed attempt preserves the previous valid product. Native
control builds belong to suites, not ordinary translation prerequisites.

## Milestones and evidence

Commit each significant milestone after its required checks pass, with focused
changes and recorded evidence. QuickJS regeneration and full verification with
the campaign defaults, direct inline exports and enabled Atomics passed in
`20260927-062444-292bece5`, including native controls, all four execution cells,
lifecycle checks and artifact audits.

| Phase | Concrete exit gate | Status | Evidence |
| --- | --- | --- | --- |
| P0 — Scope/native baseline | Freeze source/profile/case inventory; recipe and canonical layout; native JSON workflow and deliberately failing control; first real full-manifest translation attempt | Passed | Recipe/layout/fetch created; native controls 36/36; actual full source attempts recorded in `docs/validation.md`. |
| P1 — Boundary feasibility | Emitted/native aggregate ABI, arithmetic, callbacks, allocation, stack guard and state probes; decide every required host import | Passed | Actual native/emitted ABI, allocator and rooted AOT boundary reports; D2–D6 validated. |
| P2 — Complete translation | All five upstream units and selected upstream inline exports link, raw then processed products build; required imports resolved; no handwritten emitted repairs | Passed | Framework receipts and symbol/dependency audit; preserve authored paths. |
| P3 — Useful execution | JSON/module/callback/Promise workload through real engine; safe limits/errors and full cleanup; independent runtimes | Passed | `consumer`, `behavior`, `lifecycle` reports across required modes. |
| P4 — Qualification | Required upstream cases and complete feature/error/lifetime matrix pass with case counts; no unclassified oracle crashes | Passed | `upstream`, `abi`, `behavior`, `lifecycle`; record all exclusions. |
| P5 — Consumer delivery | Root solution and ordinary project references; owning API documented; separate consumer runs in all four cells including trimmed AOT | Passed | `consumer` plus dependency audit and usage instructions. |
| P6 — Acceptance | Final-tool regeneration, all required suites/matrix, offline-source reproduction and affected shared regressions | Passed | Current receipts linked from `docs/validation.md`; no required blocked/unrun rows. |

Dependencies: P0 → P1 → P2 → P3 → P4 → P5 → P6. API/harness design can
proceed during P1/P2, but runtime acceptance requires the translated engine.
Track observed failures in [blockers.md](blockers.md) with classification,
reproducer, native expectation, regression, fix, and full-source retry. Keep
suspected risks and unrelated repository baseline failures separate.

## Validation selection

These are the declared recipe suites; current execution status is recorded in `docs/validation.md`. “Matrix” means
`embedded` × Linux x64 × raw/processed × JIT/NativeAOT. All required rows execute
in every cell unless the row explicitly describes native or shared-tool work.

| Behavior/gate | Suite and oracle/assertion | Target | Selection/prerequisite |
| --- | --- | --- | --- |
| ABI and host boundary | `abi`: actual native/emitted layouts, aggregate calls, rooted thunks, allocator and string contracts; whole-library AOT root | Matrix plus native control | Required; native compiler |
| Useful workflow | `consumer`: exact JSON, three callback calls, module identity, settled Promise and clean repeat run | Matrix | Required; ordinary project references |
| Engine features | `behavior`: closures, exceptions, Unicode/surrogates, numeric/BigInt edges, RegExp, Map/Set, typed arrays, cyclic/failed modules, job and rejection ordering | Matrix plus native control | Required; fixed fixtures and explicit assertions |
| Errors and lifetimes | `lifecycle`: syntax/type errors, missing modules, callback exceptions, memory limits, recursion, cancellation, value ownership, two runtimes and repeated disposal | Matrix | Required; bounded harness deadlines |
| Bundled cases | `upstream`: inventory in `docs/upstream-tests.md`, native and translated assertion outcomes and counts | Matrix plus native control | Required selected cases; same archive tests |
| Delivery dependencies | `audit`: assembly references/imports, generated ownership, licenses, no native QuickJS/backend, both product forms and consumer dependency manifests | Both forms and JIT/AOT artifacts | Required; final products |
| Bytecode/file interoperability | No suite selected | None | Excluded; public bytecode is outside scope, and version-coupled bytecode is not an untrusted input format |
| External conformance/fuzz/stress | No suite selected | None | Excluded from initial acceptance; external Test262 pin and harness would require a separate decision |
| Shared regressions | Focused `DotCC.Tests`, emitted `DotCC.FunctionalTests/Fixtures`, runtime/postprocessor tests and affected campaigns chosen by changed semantics | Relevant repository targets | Required when shared code changes; no blanket historical campaign prerequisite |

`default_suites`: `consumer`. `verify_suites`: `native`, `abi`, `consumer`,
`behavior`, `lifecycle`, `upstream`, `audit`. `verify` must honor the requested
matrix; acceptance commands explicitly request `--form all --mode all`.

Compare deterministic JSON, integers, bytes, layout and statuses exactly. Use
`Object.is`/bit patterns where signed zero or NaN needs special handling; compare
exception class and stable message fragments, not host-specific stack paths.
Run Date tests with fixed timestamps and UTC; nondeterministic clock/random tests
assert explicit range/order invariants, never arbitrary transcript stripping.
Set per-case libm tolerances only where native platform variation warrants them.
Preserve unnormalized logs and per-case pass/fail/skip totals. An oracle crash,
silently skipped assertion, pending Promise or harness timeout is not a pass.
Performance thresholds and benchmarks are not part of this plan.

## Reproduction commands and acceptance

Implemented commands from the repository root. Current execution receipts and
qualification scope are recorded in `docs/validation.md`.

```bash
bash Scripts/campaign.sh layout quickjs --write
bash Scripts/campaign.sh layout quickjs --check
bash quickjs/scripts/fetch.sh --fetch missing
bash quickjs/scripts/translate.sh --fetch never
dotnet build quickjs/ManagedConsumer.slnx -c Release
bash quickjs/scripts/test.sh --suite consumer --form all --mode all --rid linux-x64
bash quickjs/scripts/verify.sh --fetch never --form all --mode all --rid linux-x64
```

Test invocation by absolute script path from an unrelated directory. Demonstrate
`--fetch never` with usable local trees/cached archives and `--hashes warn`/`off`
with legitimate provenance drift; exact adaptation guards still apply. NuGet
restore is separate: document `--restore normal|locked|none` and cache prerequisites.
Repeat from clean campaign-owned outputs without deleting unrelated files;
verify that regeneration preserves authored files and picks up authored edits.

Final evidence belongs in `docs/validation.md`, linking
`artifacts/campaign/<run-id>/receipt.json` and case-level specialist reports.
P0–P6 pass in final receipt `20260927-062444-292bece5`. All required suites,
four execution cells, actual artifact audits and offline reproduction gates pass.
See `validation.md` for receipts. Other platforms, external Test262,
shell/worker features and bytecode delivery are excluded; no failing or blocked
required test can be reclassified as optional to close acceptance.

The completion conditions are satisfied by the final evidence: real useful
workflow, required features, all four execution cells, owning consumer, actual
distribution audit and reproduction gates. Changes must continue to satisfy them.

## Follow-ups

**`__js_rc` deduplication — completed.** The recipe omitted the opt-in
`--deduplicate-inline` linker flag. Both object definitions already had the
same supported fingerprint, no dependencies and no observed function address;
there was no compiler equivalence-proof failure. The recipe now enables the
existing option, without changing shared compiler/linker behavior.

Fresh raw and processed products each contain one unmangled `__js_rc` definition,
enforced by the product audit. A focused managed regression executes the same
header-pointer arithmetic through two translation units and checks the returned
pointer and shared allocation; existing callback-address and mutable-state
checks remain in that regression. All six direct/object managed cases and 33
inline unit tests pass. The full Linux x64 raw/processed × JIT/NativeAOT matrix
passes again in `20260927-062444-292bece5`.

The [investigation evidence](../artifacts/inline-deduplication/report.json)
retains the original object metadata, omitted flag, focused test logs and final
deduplication audit. This resolves the previously deferred user-requested item.

**Header-inline exports — completed.** The facade directly calls the five upstream
embedding inlines selected by `--export-inline` in `config/profile.json`. The
forwarding `DotCC_JS_*` functions and their extra C translation unit are removed.
The final source and rooted-AOT audits validate the original-name APIs, and all
four execution cells pass in `20260927-062444-292bece5`.
