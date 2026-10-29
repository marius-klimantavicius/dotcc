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
upstream source files were modified by this work. No commits were made.
