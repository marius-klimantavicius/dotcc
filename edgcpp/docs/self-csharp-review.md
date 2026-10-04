# Direct C# review of the EDG compiler

The current exception-enabled review is at
[`artifacts/review/edg/README.md`](../artifacts/review/edg/README.md).
It uses named ordinary locals, bulk byte initialization, literal previews and
omits empty noexcept guards. Primitive numeric tables use static typed span
properties backed by assembly data. All constant-initialized global/TLS arrays use source-named pointer aliases,
including pointer tables with direct indexed assignments. Plain records use
named-field object initializers with trailing commas. Named export
wrappers retain their source-resolved names; implementation bodies use `__body`.
The existing EDG review aliases are refreshed in place; their README and manifest
describe the current output.

`translate_self.py` reads the **original `cpfe` target** from
`build/compile_commands.json`: currently 75 C++14 translation units, including
the C backend, with the all-extensions build's definitions. It does not translate
the historical lowered C and does not substitute a small demonstration compiler.
The generated project uses `Edg.Compiler.EdgCompiler` and source-oriented file
grouping by default. `--split size` remains available.

The current refresh uses `--exceptions --unbound-externals partial`: 84 external
symbols require runtime implementations. Shared signatures use `Managed.Runtime`
in `Managed.Runtime.generated.cs`; signatures involving generated types remain
on `Edg.Compiler.EdgCompiler` in `Translated.Runtime.generated.cs`. This includes
the host FILE and other platform record layouts. See
[`partial-externals.md`](partial-externals.md) for `--runtime-source` providers.
These are required compile-time contracts, not throwing review stubs. Full
managed compilation/runtime remain unverified. An earlier no-exceptions build
was stopped when Roslyn reached 78.2 GiB resident memory without producing an
assembly; that controlled stop was not a successful build. Some source loops
still use `goto`, and some assignments retain helper calls.

By default, the runner preserves native `-fno-exceptions` and `-fno-rtti` through
the driver's explicit `--no-exceptions --no-rtti` flags. `--exceptions` and
`--no-exceptions` override exception handling for review; the report records both
the native and effective feature settings. Ordinary driver invocations continue
to enable both features by default.

The portable-header attempt currently stops at the unconditional `sys/types.h`
include in `host_envir.h`. The diagnostic `--profile host` uses the native Linux
LP64 target and actual host headers. This profile is for reviewing translated
bodies; it does not establish portable libc/OS bindings or a runnable managed
compiler. The native EDG configuration uses OS services such as memory mapping
that still need a managed host implementation.

For the full source set, the custom backend must enable the supported EDG
configuration `PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE=1`, as the original C backend
does. Without it, alternate-entry name generation asserts before our backend is
called. A fresh original-C translation of the same 75 sources succeeds. An
isolated current C# backend build with the promotion setting passes that stage;
no EDG semantic source changes are needed.

The current investigation's isolated binary is under:

```
artifacts/campaign/persistent-continuation/contract-split-build-001/cpfe-dot
```

To request a fresh review artifact using such a compiler:

```sh
python3 edgcpp/translate_self.py --profile host --exceptions \
  --compiler /path/to/promotion-enabled/cpfe-dot \
  --unbound-externals partial \
  --output-dir /path/to/new-output
```

`--exceptions` generates stack-unwinding paths. The review guide
[`artifacts/review/edg-exceptions/README.md`](../artifacts/review/edg-exceptions/README.md)
links the generated `token_to_string` function, its cleanup helper and the
exception filter, alongside the original C++. Construction advances a cleanup
state; normal destruction clears
it. A function-level `finally` invokes the cleanup helper during unwinding.
The helper clears the state before calling the destructor and terminates if a
destructor throws during cleanup. The exception filter marks unwinding but
returns false, leaving exception propagation to the CLR.

This variant retains the host-header review limitations and has 84 explicitly
unbound external declarations. Translation and formatting are separate from a
full C# build or runtime verification; those remain unverified for this variant.

The driver records the selected compiler path and its SHA-256 captured before
invocation. The self-translation report additionally records original source
hashes, native build definitions, generated configuration hash, header search
paths, exact commands, exit codes, and logs. A nonzero translation exit means
there is **no complete generated project** in that output directory.
The runner retains `raw.cs` before formatting or packaging, using the driver's
optional `--raw-output` argument. Raw files must be new; they are never silently
overwritten. This preserves the full backend result even if later tooling fails.
For such a retained result, `dotnet.py --raw-input /path/to/raw.cs` explicitly
replays only packaging, with the same language, target, layout, and output options.
Its manifest records the raw input path/hash and `packagingOnly: true`; `compiler`
is null because replay does not invoke a compiler or independently verify the
raw file's native provenance. Keep the original self-translation report beside
the replayed project. `--raw-input` and `--raw-output` are mutually exclusive.

The formatter keeps very large synthetic storage constructors and numeric
literal collections compact internally, preserving their exact tokens and
formatting source functions normally. This avoids quadratic Roslyn token
navigation over millions of synthetic initializer siblings. Large formatted
files travel through JSON as ordered string chunks; chunk boundaries preserve
UTF-16 surrogate pairs, and the Python driver rejoins the exact text before
source splitting and mapping.

## Explicit unbound declarations

Normal translation rejects external functions or variables that have no binding.
`--unbound-externals declarations` is an explicit review option: typed external
function bodies and external-storage accessors throw
`PlatformNotSupportedException`. It records every such symbol in
`dot-gen.json.unboundExternals`. Function pointer calls use the same throwing
declaration. Reading, writing, or taking the address of missing storage cannot
silently manufacture a zero-initialized object.

This mode preserves reviewable function bodies while making the missing host
boundary explicit. It does **not** implement those imports, mask unsupported
language operations, or establish that the resulting program runs.

The focused `backend/tests/unbound_externals_verify.py` check verifies default
rejection plus static/instance JIT and NativeAOT throwing behavior. A separate
five-by-900-KiB string fixture verifies that lifting the old 4-MiB literal-pool
limit to the existing 64-MiB arena budget preserves JIT and NativeAOT data reads.
Literal bytes still live in their own pinned array, separate from global/TLS
arenas; only the checked capacity limit changed.
