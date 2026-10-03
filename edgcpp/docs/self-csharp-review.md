# Direct C# review of the EDG compiler

`translate_self.py` reads the **original `cpfe` target** from
`build/compile_commands.json`: currently 75 C++14 translation units, including
the C backend, with the all-extensions build's definitions. It does not translate
the historical lowered C and does not substitute a small demonstration compiler.
The generated project uses `Edg.Compiler.EdgCompiler` and source-oriented file
grouping by default. `--split size` remains available.
The completed review checkpoint is available through
[`artifacts/review/edg/README.md`](../artifacts/review/edg/README.md): 155 C# files,
279,023,677 source bytes, 41,890 exported wrappers, and 82 explicitly unbound
external declarations. All 75 original translation units were emitted and the
complete output passed Roslyn syntax checking and formatting. This is review
coverage, not a claim of a runnable managed compiler or finished readability:
some source loops still use `goto`, and some assignments retain helper calls.
The optional strict Release build was stopped after about 326 seconds when
Roslyn reached 78.2 GiB resident memory, leaving 5.65 GiB available on the host.
It had emitted neither compiler diagnostics nor an assembly. Full semantic
compilation therefore remains unverified; the retained resource-limit record
distinguishes this controlled stop from a passing build or a compiler error.
The runner also preserves native `-fno-exceptions` and `-fno-rtti` through the
driver's explicit `--no-exceptions --no-rtti` flags. Ordinary driver invocations
continue to enable both features by default.

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
artifacts/campaign/persistent-continuation/edg-self-review-linear-build-002/cpfe-dot
```

To request a fresh review artifact using such a compiler:

```sh
python3 edgcpp/translate_self.py --profile host \
  --compiler /path/to/promotion-enabled/cpfe-dot \
  --unbound-externals declarations \
  --output-dir /path/to/new-output
```

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
