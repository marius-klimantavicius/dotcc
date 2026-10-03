# Direct C# review of the EDG compiler

`translate_self.py` reads the **original `cpfe` target** from
`build/compile_commands.json`: currently 75 C++14 translation units, including
the C backend, with the all-extensions build's definitions. It does not translate
the historical lowered C and does not substitute a small demonstration compiler.
The generated project uses `Edg.Compiler.EdgCompiler` and source-oriented file
grouping by default. `--split size` remains available.

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
artifacts/campaign/persistent-continuation/edg-self-review-promote-build-001/build/bin/cpfe-dot
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
