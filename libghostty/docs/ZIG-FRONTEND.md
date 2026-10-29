# Direct Zig frontend evidence

Updated 2026-10-29. These are compiler feasibility findings, not a completed
libghostty translation. Product source is the pin in `config/source.json`:
`93967b7f9478e94dcbb42125c8a90f34feb40d75`. The actual product root is
`src/lib_vt.zig`; `src/terminal/c/main.zig` re-exports its complete terminal C API.
Neither attempt uses C declarations or a substitute terminal implementation.

DotCC's Zig frontend performs semantic lowering into typed IR, with lazy source
imports, generics and comptime support. Its supported subset does not yet lower
the selected complete module. The campaign also investigates upstream Zig's
whole-engine C backend followed by DotCC; that route is tracked by the supervisor.

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
| C5 | C parser: `build/abi-engine-all/engine.c:447923`, named callback parameter `void (*const a1)(...)` rejected at its name | Open; reuse qualified named callback declarators in parameter position | Latest whole-engine attempt remains incomplete |

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
