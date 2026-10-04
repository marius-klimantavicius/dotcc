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

Variadic calls now use ordinary named calls and shared `Edg.Runtime.VaArg`
carriers. The current output has zero `__edg_varcall` references (previously
2,500 generated helper definitions); only needed shared runtime modules are
copied into the project. See [the span ABI](varargs-span.md) and the review's
`varargs-examples.cs.txt`. No full managed compiler execution is implied.

`translate_self.py` reads the **original `cpfe` target** from
`build/compile_commands.json`: currently 75 C++14 translation units, including
the C backend, with the all-extensions build's definitions. It does not translate
the historical lowered C and does not substitute a small demonstration compiler.
The generated project uses `Edg.Compiler.EdgCompiler` and source-oriented file
grouping by default. `--split size` remains available.

The current refresh uses `--profile musl --exceptions --unbound-externals partial`
with pinned libc++18.1.8 headers. It adds 213 selected musl/provider source units
and binds 106 imports through the existing shared managed services. There are
24 remaining external symbols: 9 C++ library/runtime symbols and 15 POSIX host
services (metadata, directories, descriptors, resource limits and mappings).
These host services also need implementations; implementing libc++ alone will
not make the compiler runnable. Shared signatures still use `Managed.Runtime`
in `Managed.Runtime.generated.cs`; signatures involving generated types remain
on `Edg.Compiler.EdgCompiler` in `Translated.Runtime.generated.cs`. Portable FILE
is opaque and the private host records do not claim native OS ABI compatibility.
The discussed common `Edg.Runtime` namespace migration remains separate. See
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

The earlier portable-header attempt stopped at `sys/types.h`. The musl profile
now supplies a private [host declaration overlay](../backend/self/README.md)
without discovering system headers. It uses EDG's supported
`USE_MMAP_FOR_MEMORY_REGIONS=0` allocation/read fallback while preserving
language extensions and portable C++/CLI metadata mapping. Binary128 settings
remain unchanged. Although a quadmath declaration header is needed for parsing,
the original host-conversion and hexadecimal-output settings leave no emitted
quadmath calls. The exact binary128 classification adapter uses an EDG builtin.

The diagnostic `--profile host` remains available with the native Linux target
and actual host headers. Neither profile establishes a runnable managed compiler
or complete cross-platform host integration. Native configuration defaults,
including target/predefined-macro-file policies, remain inputs to the port.

For the full source set, the custom backend must enable the supported EDG
configuration `PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE=1`, as the original C backend
does. Without it, alternate-entry name generation asserts before our backend is
called. A fresh original-C translation of the same 75 sources succeeds. An
isolated current C# backend build with the promotion setting passes that stage;
no EDG semantic source changes are needed.

The current investigation's isolated binary is under:

```
artifacts/campaign/persistent-continuation/self-musl-build-001/cpfe-dot
```

To request a fresh review artifact using such a compiler:

```sh
python3 edgcpp/translate_self.py --profile musl --exceptions \
  --libcxx-acquisition edgcpp/artifacts/campaign/libcxx-acquired-005 \
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

The current musl variant has 24 explicitly unbound external contracts. Its
bounded executable probe passes strict JIT and Linux-x64 NativeAOT; the separate
real-libc++ header probe verifies required missing bodies fail with CS8795.
Full compiler translation and formatting remain separate from a full C# build
or runtime verification, which have not been run for this variant.

The musl C++ probe also exposed unsafe adjacent-declaration fusion into for
headers. The emitter now follows the C backend in retaining those declarations
before the structured loop. C/C++ scope regressions and the ten-check existing
structured-control suite pass; reconstructing true source for-init declarations
requires retained source-scope provenance. See
[milestone evidence](../evidence/self-musl-20261004.json).

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
