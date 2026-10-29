# Direct Zig frontend evidence

Status updated 2026-10-30; the detailed compiler observations below are historical
2026-10-29 feasibility evidence, not a completed direct Zig translation. Product
source is the pin in `config/source.json`:
`93967b7f9478e94dcbb42125c8a90f34feb40d75`. The actual product root is
`src/lib_vt.zig`; `src/terminal/c/main.zig` re-exports its complete terminal C API.
Neither attempt uses C declarations or a substitute terminal implementation.

DotCC's Zig frontend performs semantic lowering into typed IR, with lazy source
imports, generics and comptime support. Its supported subset does not yet lower
the selected complete module. The active C route uses upstream Zig's genuine
whole-engine C backend followed by DotCC and is tracked by the supervisor.

## Current phase and deferred upstream testing

On 2026-10-30, the user deferred upstream test suites, their 2,622-case matrix
and the upstream-only host-I/O probe to the direct Zig phase. All case names,
20 core partitions, specialist definitions, historical subset passes and the
cancelled expanded C-route attempt are retained in
[upstream test qualification](UPSTREAM-TESTS.md). Deferred work is not passed.

The C-route completion commit still follows final native whole-engine/boundary
controls, the four consumer rows, 24-case qualification in all four rows with
rooted NativeAOT, four factory-allocation rows, audit, the original root solution,
local warn/off reproduction with authored preservation, and affected shared and
downstream regressions. These retained gates now pass; see [C-route acceptance](C-ROUTE-ACCEPTANCE.md). After the completion commit, work
continues automatically with relevant true-upstream DotCC integration, then
actual pinned libghostty-vt Zig → DotCC translation without a C intermediate.
No upstream integration or direct-route implementation has begun. The direct
phase must establish complete exports and bodies, the full 2,622-case upstream
matrix in all four managed rows with native/C controls, and reproduction; the
small regressions and historical C-route subset passes below do not close it.

## Observed failures and repairs

| ID | Stage and actual observation | Repair/regression | Full source retry |
| --- | --- | --- | --- |
| Z1 | Parser: `lib_vt.zig:187`, trailing comma in multiline `@export` call rejected at `)` | Generic `ArgList` trailing comma grammar plus list flattening; `call-trailing-comma.zig` executes ordinary, nested, imported and builtin calls | Advances to Z2 |
| Z2 | Parser: `lib_vt.zig:502`, `builtin.is_test or switch (...)` rejected at `switch` | Put the already supported, brace-delimited switch expression in the ordinary expression cascade; `switch-operand.zig` exercises boolean operands and call arguments | Advances to Z3 |
| Z3 | Parser: `lib_vt.zig:524`, `else {}` rejected at `{` | Parse the empty block value as an RHS; generated and freshly rebuilt table regression. This is syntax support, not a claim of general block-value lowering | Root now reaches semantic analysis, Z5 |
| Z4 | Semantic lowering: `terminal/c/main.zig`, imported `lib.alloc.Allocator` falls into the module-as-runtime-value error | Resolve retained const namespace re-exports lazily; resolve exported type aliases through the source module graph. `import-type-alias.zig` exercises namespace, aggregate and primitive re-exports | Actual C API module advances to `zig type 'Encoding' not supported yet (slice)` |
| Z5 | Semantic lowering: full `lib_vt.zig` reaches `zig type 'noreturn' not supported yet (slice)` (`tinyPanicImpl` signature) | Open; no semantic substitute or success stub added | Direct whole-root translation remains incomplete |
| Z6 | Source audit: `ZigLowering.Lower` explicitly drops top-level `comptime {}` blocks, including the selected root's export-registration block | Open structural issue; even eventual emission of this root would need proof of every required C API export and translated body | No API completeness or managed execution claimed |

Logs are under `artifacts/zig-frontend-probes/`: `initial-lib-vt.log`,
`after-trailing-lib-vt.log`, `after-switch-lib-vt.log`,
`after-empty-block-lib-vt.log`, `initial-c-api.log`, `after-alias-c-api.log`.
The minimal trailing comma and import alias failures are also retained as
`trailing-before.log` and `import-alias-before.log`.

## Commands and validation

Run from the repository root; source and toolchain acquisition are owned by the
native/campaign worker. The following commands use the existing compiler and do
not write compiler build outputs:

```bash
dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=managedlib --class-name=GhosttyVt \
  libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src/lib_vt.zig \
  -o libghostty/build/zig-frontend-probes/root
dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=managedlib --class-name=GhosttyVt \
  libghostty/ref/ghostty-93967b7f9478e94dcbb42125c8a90f34feb40d75/src/terminal/c/main.zig \
  -o libghostty/build/zig-frontend-probes/c-api
python3 libghostty/tests/translation/run_zig_regressions.py \
  --compiler DotCC/bin/Release/net10.0/dotcc.dll \
  --zig libghostty/build/native-tools/zig-x86_64-linux-0.17.0/zig
```

With supervisor-coordinated ownership of root build outputs:

```bash
dotnet build DotCC/DotCC.csproj -c Release --no-restore -m:1 -v:q
dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release --no-restore \
  --filter 'FullyQualifiedName~Zig' -m:1 -v:q
```

The compiler build regenerates the Zig parser through its existing source
generator. Both generated and fresh parser tables pass the new grammar cases.
All **556 Zig unit cases pass, zero failures/skips** after the final alias repair;
see `artifacts/zig-frontend-probes/zig-tests-after-alias.log`.

All **three native/managed execution regressions pass**, each returning 42 with
identical stdout/stderr under native Zig **0.17.0** and emitted C# on .NET
**10 / SDK 10.0.112**, Linux x64. Commands, diagnostics and tool versions are
preserved in `build/zig-regressions/receipt.json`; the runner builds only its
generated fixture projects. These small regressions establish the individual
repairs and do not satisfy the whole-engine campaign acceptance gates.

No shared campaign framework files, preexisting dirty compiler changes, or
upstream source files were modified by this worker. The supervisor owns commits.

## Whole-engine C route: subsequent frontend repairs

The supervisor/native worker produced the complete scalar engine with upstream
Zig's C backend, then supplied `build/native-tools/engine-freestanding.c` for
actual DotCC retries. The source, not declarations or a native engine wrapper,
is translated using the supervised policy `config/zig-c-overrides.json`.

| ID | Observed whole-source failure | Generic repair and regression | Actual retry |
| --- | --- | --- | --- |
| C1 | C parser: `engine-freestanding.c:7853`, `typedef zig_under_align(1) uint64_t aligned__1_u64_9;` | Attributed scalar typedefs before the type, after it, and after the alias; explicit type alignment metadata keeps C compatibility. Existing layout/storage paths now retain weakened member alignment and stronger local/global alignment | Advances to C2 |
| C2 | C lexer: `engine-freestanding.c:15909`, valid C `"\?"` escape rejected | Add escaped question mark to STRING lexer; decoder already supported it. Native/managed byte regression prints `??= 63 63 61` | Advances to C3 |
| C3 | C parser: `engine-freestanding.c:19664`, `struct errunion_usize_15 (*const *t13)(...);`, followed by its qualified abstract cast at line 19716 | Compose callback pointer stars and const/volatile/restrict at their respective pointer levels; use the same construction for abstract casts. Retain the existing plain abstract callback parameter parse | Advances to C4 |
| C4 | C parser: `engine-freestanding.c:25395`, `(struct arr_3_usize_389366){{...}}.array[t6]` rejected at `.` | Move compound literals into the C postfix-expression level; existing typed initializer and temporary-array-member storage lowering now compose directly | Current authored-accessor engine advances to C5 |
| C5 | C parser: `build/abi-engine-all/engine.c:447923`, named callback parameter `void (*const a1)(...)` rejected at its name | Reuse qualified callback declarators for parameters and share the named pointer-to-array reduction to preserve existing parameter syntax | Complete 530,948-line engine parses; semantic binding reaches C6 |
| C6 | Semantic ABI assertion: `engine.c:28`, source `sizeof(long double)==16 && _Alignof(long double)==16` disagrees with the existing default 8-byte model | Explicit per-request `sourceAbi.longDouble: x87-storage16`; actual 16-byte representation, alignment and byte copies. Numeric operations fail explicitly; default remains unchanged | All source assertions retained and passed; backend reaches C7 |
| C7 | C# backend: `array member of a temporary aggregate requires a hoistable storage context` | Supervisor extended the existing lazy-effect classification to temporary aggregate array members. Selected branches use the existing statement lowering | Canonical scalar route emits the complete engine object; final scalar link/build/publication gates now pass |
| C8 | Canonical raw build: 57 CS0161 errors after the support header selected its empty `zig_unreachable()` fallback | Supervisor added a strict zero-argument, exact-empty macro rule selecting already-supported `__builtin_unreachable()`. Its existing IR/backend lowering emits a real C# throw | All 57 termination diagnostics resolved in canonical attempt `20261029-165535-e1751380` |
| C9 | Next raw build: 20 CS7013 errors from generated source names exceeding .NET metadata limits | Generic UTF-8 bounded target identifiers retain a readable prefix plus SHA256 of the complete spelling. Aggregate relocation uses the same map; semantic object identities retain raw source names | Canonical attempt `20261029-170255-7d3e0289` passes raw and processed-context builds; publication remains supervised |

The C1 regression first exposed a real emitted-storage mismatch: static layout
queries reported 10 bytes while actual managed `sizeof(record)` was 24. Keeping
typedef alignment on the field/object storage marker fixes that mismatch through
the existing explicit-layout backend. The native and emitted fixture now both
print exactly:

```text
layout 8 1 2 32 1 10 1
storage 0 0 values 8 1 7 43
```

This exercises a byte-offset unaligned 64-bit load/store, a containing record,
const/chained aliases, C `_Generic` compatibility, and 32-byte local/global
storage. The emitted functional test runs direct translation and object linking.

Validation: all **3504 DotCC unit tests pass**, including **89 focused
alignment/GNU tests**; the new C functional cases and the supervisor's Zig
functional cases pass **6/6**. No skips or failures in those runs. Evidence:
`artifacts/zig-frontend-probes/all-unit-tests-after-alignment.log`,
`alignment-unit-tests.log`, `scalar-alignment-functional.log`,
`scalar-alignment-native.stdout`, `scalar-alignment-managed.stdout`,
`question-native.stdout`, `engine-after-alignment.log`, and
`engine-after-question-escape.log`.

```bash
cc -std=c11 libghostty/tests/translation/scalar-typedef-alignment.c \
  -o libghostty/build/zig-frontend-probes/scalar-alignment-native
cc -std=c11 libghostty/tests/translation/question-escape.c \
  -o libghostty/build/zig-frontend-probes/question-native
dotnet DotCC/bin/Release/net10.0/dotcc.dll --emit=obj \
  libghostty/build/native-tools/engine-freestanding.c \
  -I libghostty/build/native-tools/zig-x86_64-linux-0.17.0/lib \
  --overrides-file libghostty/config/zig-c-overrides.json \
  -o libghostty/build/zig-frontend-probes/engine.cs
```

C3 validation: all **3510 DotCC unit cases pass**, including six new pointer
qualifier cases and prior instance-callback boundary regressions. The native
`const-callback-storage.c` fixture and two managed direct/object-link functional
cases all return zero and print `callbacks 41 42 7`. They exercise mutable outer
storage pointers, const callback pointees, abstract casts, three pointer levels,
global slots, record members and no-argument signatures. Illegal assignments
through both declared and cast const callback pointees retain the read-only
diagnostic. Logs: `all-unit-tests-after-callback.log`,
`qualified-callback-unit-tests.log`, `qualified-callback-functional.log`,
`callback-native.stdout`, and `engine-after-callback-cast.log`, all under
`artifacts/zig-frontend-probes/`.

C4 validation: all **3510 unit cases pass**; native plus direct/object-link
functional cases print exactly `postfix 42 7` (zero exit). The fixture combines
positional nested array initialization and a direct `.array[index]` chain with a
designated compound literal's direct scalar member. The requested current
accessor engine (`build/abi-engine-all/engine.c`, upstream root plus the
supervisor's primitive ABI address getter) was retried using the same header
override policy. Evidence: `all-unit-tests-after-postfix.log`,
`compound-postfix-functional.log`, `compound-postfix-native.stdout`, and
`abi-engine-after-postfix.log` under `artifacts/zig-frontend-probes/`.

C5 validation: **3516 unit cases and six related functional cases pass**. The
native callback parameter fixture prints `parameters 42 41`; both direct and
object-linked managed execution agree. Read-only callback parameters, callback
slot pointees, array parameters and prior pointer-to-array forms are covered.

## Explicit source storage ABI

The CLI accepts this strict version-1 override-profile setting, and the public
compiler API accepts the equivalent immutable `CSourceAbi` through
`CPreprocessingOptions`:

```json
{"version": 1, "sourceAbi": {"longDouble": "x87-storage16"}}
```

The default remains `binary64`. No global `CType.LongDouble` mutation occurs:
each frontend builder, including its speculative macro evaluator, receives the
request's model. Unknown public enum values are rejected. Profile identity and
`WithoutReport()` preserve the setting. Concurrent alternating default/storage
requests are tested.

`LongDouble80Storage` occupies sixteen real unmanaged bytes. Existing aligned
object and aggregate layout machinery gives locals, globals, containing records
and arrays sixteen-byte alignment. This supports raw storage and byte copies,
**not x87 numeric operations**: arithmetic, comparisons, conversions, truth tests
and long-double complex use fail with an explicit unsupported diagnostic.
The selected emitted engine names extended floating types only in its primitive
ABI assertions; this does not qualify extended floating arithmetic.

Native and managed direct/object execution agree exactly:

```text
abi 16 16 48 16 32
storage 0 0 0 0 stride 16 bytes 1 1 1 1 1
```

The byte checks retain all sixteen bytes across `memcpy`; a C value assignment
compares the significant ten bytes because native x87 padding is unspecified.
All **3537 unit cases pass**, including 21 source-ABI cases and concurrent
request isolation; both source-storage functional cases pass. The native
control is `tests/translation/source-abi-storage.c`. Evidence under
`artifacts/zig-frontend-probes/`: `all-unit-tests-after-source-abi.log`,
`source-abi-functional.log`, `source-abi-native.stdout`,
`source-abi-managed-build.log`, and `abi-engine-after-source-abi.log`.

The full engine retry uses a private copy of the supervised header profile plus
this setting, at `build/zig-frontend-probes/zig-c-source-abi-overrides.json`.
Required macro matches remain enforced. The original upstream source and all
ABI assertions remain intact. Campaign configuration integration is supervised.

## Conditional compound-literal lifetime regression

`tests/translation/conditional-compound-lifetime.c` is native-established and has
matching direct/object test cases in
`DotCC.FunctionalTests/ConditionalCompoundLifetimeExecutionTests.cs`. The native
program and both actual managed executables print exactly:

```text
conditional 40 2 42 7 1 41 8 0 1
```

The cases begin with assignment statements, exercise unselected initializer
effects and short-circuit `&&`/`||`, retain an array pointer after another
conditional, and revisit the same compound-literal occurrence through `goto`.
That last check verifies both stable object identity and reinitialization of the
saved pointer's contents within the enclosing C block.

Focused verification used the immutable compiler at
`build/campaign/20261029-164601-fc693464/tools/DotCC/dotcc.dll`. It emitted and
built only private regression projects; no root compiler build or duplicate full
engine attempt occurred. Both managed paths exited zero and their complete
stdout matches native exactly. Evidence under `artifacts/zig-frontend-probes/`:
`conditional-lifetime-native.stdout`, `conditional-lifetime-managed.stdout`,
`conditional-lifetime-object.stdout`, and their emission/link/build logs.
The repository test cases are authored for subsequent focused suite execution.

The supervisor's **normal canonical scalar pipeline** emitted the complete
42 MB engine object at
`build/campaign/20261029-164601-fc693464/objects/000-engine.cs`. This establishes
complete lowering/emission for that selected source attempt, not P2 completion:
engine/bridge linking, raw/processed builds and canonical publication are still
supervised acceptance gates. No separate review project is a campaign deliverable.

Native x87 scalar calling convention and numerical variadic use are unqualified
by the storage-only source ABI. Additional fail-loud boundary guards are deferred
until after the priority default-product attempt; no extended arithmetic support
is claimed.

## Support-header termination and bounded target names

The actual pinned `zig.h:336-341` selects an empty `zig_unreachable()` fallback
when the preprocessor does not advertise `__has_builtin`. No compiler identity
macro was forged. The strict campaign rule selects the implemented GNU builtin
only for that exact empty function-like definition; upstream algorithms and
source assertions remain intact.

A small program including the real pinned header reproduces CS0161 before the
rule and emits a literal `throw new System.Diagnostics.UnreachableException`
after it. Native C, direct translation and object translation all return zero
on valid branches. Deliberately reaching the impossible branch in both managed
executables terminates with `UnreachableException`, exit -6. Native undefined
behavior on that branch was not executed. Evidence:
`artifacts/zig-frontend-probes/zig-unreachable-execution.json`, the adjacent
before/after profiles, override report and build logs. Object emission used the
immutable `20261029-164959-22fd99b5` tool; relinking with the subsequent immutable
`20261029-165535-e1751380` runtime supplied the separately repaired arithmetic
helpers. No generated source was edited.

`tests/translation/long-target-identifiers.c` establishes native output
`long names 42 41` for identifiers exceeding 4 KB, including two functions that
differ only after their long common prefix. Five focused naming unit cases and
both direct/object execution cases pass. An additional immutable-tool probe
makes both functions static and exercises their direct calls and cached function
addresses after object-specific suffixing; both builds and executables pass
with identical native output. Evidence:
`artifacts/zig-frontend-probes/long-static-target-execution.json` and adjacent
emission/link/build logs. The repository functional regression now includes
both external and static function variants for subsequent focused execution.

Naming changes are owned by this frontend/compiler worker in `EmitHelpers.cs`,
`Backends/CSharpTarget.cs`, `AggregateIdentities.cs`, and the external-type
validator. Source names and raw aggregate identity metadata remain complete;
authored external CLR type spellings are preserved. Immediate before copies and
the exact naming-only patch are retained in `build/target-names-before/`, so the
prior SourceABI addition in `CSharpTarget.cs` can be isolated for commits. No
index mutation or compiler build was performed by this worker for these fixes.

## True upstream DotCC review

The parent reviewed `sebgod/dotcc` (the GitHub parent of the user fork), pinned
review revision `d3678b5c50f86bf65d831ee4efda6134a7894116`. Its untouched source
builds in the isolated `build/upstream-dotcc-review/source` directory; root
compiler outputs and the campaign inputs were preserved. Its default branch has
substantial later Zig import, generic and comptime work, requiring wider AST/IR
and LALR changes rather than an unreviewed merge during qualification.

Real probes still fail the Ghostty root at a type-returning `Enum` function and
the C API module at `Result`. Native Zig accepts the reduced `noreturn` case,
while upstream DotCC rejects it. More importantly, executable reductions show
upstream drops top-level comptime blocks: it accepts a required `@compileError`
and omits the name requested by `@export`, while native controls fail/export as
expected. It cannot establish direct Ghostty translation by replacing the tool.
The working genuine Zig C-backend route remains the campaign implementation.

Useful direct-Zig followup references include `5357dc3f3bfb` (reexports resolve
the owning module), `f3f520e46bd0` (cross-module generics), `2796ff2cc537`
(comptime type-returning bodies), `002f1ec9b3aa` (void/block values),
`cd3c9f03f874` (switch operand grammar), and `77c073eda` (lazy Zig conditional
hoists). These are reviewed implementation references, not integrated fixes or
qualification claims. Local shared fixes and the selected engine remain intact.
Evidence: `build/upstream-dotcc-review/probe-results.json`,
`native-controls.json`, and `build.log`; the initial CLI syntax mismatch is
retained separately from corrected probes.
