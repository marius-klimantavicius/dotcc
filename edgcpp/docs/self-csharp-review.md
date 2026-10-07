# EDG translated to C#

`translate_self.py` defaults to the **original `cpfe` target** from
`build/compile_commands.json`: 75 C++14 translation units, including the original
C backend, with the all-extensions build definitions. `--program cpfe-dot`
selects the 77-source compiler with our C# backend instead, using C++17 for its
pinned libc++ string implementation. Neither mode uses the historical lowered C.
The generated owner is `Edg.Compiler.EdgCompiler`; shared runtime sources use
`Edg.Runtime`.

The current review emission builds with zero C# warnings/errors and passes all
18 compiler acceptance cases plus three original EDG source-file parser checks
under both JIT and Linux-x64 NativeAOT. Windows .NET/Wine passes 11 compiler
checks; native Windows/macOS execution is unverified. The review aliases point
to this working project and its 36.6 MiB NativeAOT executable. Earlier failed
outputs and diagnostics are retained. [Full evidence](../evidence/self-execution-20261004.json).

## C# backend bootstrap

To include `dot_gen_be` itself, add `--program cpfe-dot --profile musl` to the
translation command below. This dependency closure additionally uses actual
pinned libc++ `string.cpp` and `stdexcept.cpp`. The provider includes the backend
as its final source so the primary translation unit owns the explicit string
template definitions and their consumer; the remaining compiler files retain
their separate translation units and file-local names.

Self-translation pins `__DATE__` and `__TIME__` from `--source-date-epoch`
(default `0`, UTC). `frontend-invocation.json` records the exact frontend
arguments, working directory, and EDG environment before execution. Reuse this
invocation for the bootstrap comparison: creating a fresh musl/provider snapshot
would change source paths in the emitted C#.

`backend/tests/self_bootstrap_verify.py` takes that invocation, a native compiler,
and `--translated-command-json` containing a command-prefix array such as
`["/absolute/published/Generated"]`. It executes two native baselines and the
translated compiler, retaining binary stdout/stderr, hashes, exit status and
first-difference context. No identifiers, paths, timestamps or whitespace are
normalized. `--native-only` runs just the baseline. The existing review aliases
remain on the verified original `cpfe` build until the new bootstrap passes.

## Build and run

Build the canonical backend, then translate using the pinned libc++ acquisition:

```sh
EDG_BUILD_TARGET=cpfe-dot bash edgcpp/build.sh
python3 edgcpp/translate_self.py --profile musl \
  --libcxx-acquisition edgcpp/artifacts/campaign/libcxx-acquired-005 \
  --output-dir /path/to/new-self-output --build
```

The default is an executable project with no unresolved externs. The musl profile
requires exceptions for its actual libc++ allocation/exception implementation;
the original compiler's no-RTTI setting is preserved. `--emit managedlib` remains
an explicit export-everything review option, but the full compiler's extra export
wrappers exceed the CLR's per-type method limit. Use the executable mode for the
full running compiler. File splitting alone does not split the owner type.

Packaging now budgets CLR methods and accessors. Above 62,000 slots,
`--method-sharding auto` (the default) moves eligible static synthetic helpers
into nested classes to bring the original owner below 61,001 slots. Source
functions, APIs, storage and instance methods retain their owner. Calls remain
direct, with qualified references that support multiple owners in one assembly.
If too few safe helpers exist, packaging reports the limitation. The diagnostic
option `--method-sharding none` retains the original layout even when it exceeds
the CLR limit. This packaging step does not change raw bootstrap comparison
bytes. Oversized expanded template identifiers are bounded in the backend;
full original names and scopes remain in the source map.

```sh
dotnet /path/to/new-self-output/managed/bin/Release/net10.0/Generated.dll --version
dotnet /path/to/new-self-output/managed/bin/Release/net10.0/Generated.dll \
  --clear_flag use_predefined_macro_file --no_code_gen --c++20 example.cpp
```

The second command is suitable for a standalone input with explicitly supplied
include paths. Normal predefined-header operation requires `EDG_BASE` pointing
to the matching EDG base directory, such as
`edgcpp/upstream/bases/docker/dev-env/gcc`. Native target/predefined-header
policies remain inputs to the compiler port. With that base and the macro file
enabled, the Linux/x64/type/width/version macro probes match native EDG exactly.
Disabling the file selects a different implicit macro environment because the
portable host build omits native OS macros; supply explicit input target facts
when parsing portable headers.

The ready review executable is:

```sh
edgcpp/artifacts/review/edg/nativeaot/Generated --version
```

EDG exits through its normal `exit`/`abort` paths. Run compilations as separate
processes: the public `Run(string[])` bridge is not a reusable in-process compiler
service. Expected exit codes are success/warnings `0`, source errors `2`, and
command-line or catastrophic errors `4`.

For a bounded Linux-x64 NativeAOT build:

```sh
python3 edgcpp/build_managed.py /path/to/new-self-output/managed/Generated.csproj \
  --output-dir /path/to/new-aot-build --publish-aot --warnings-as-errors
```

`build_managed.py` records commands, logs, elapsed time and sampled process-tree
RSS. Its default limits are a 40 GiB managed heap, 48 GiB process-tree RSS, and
four logical processors. Create `OUTPUT_DIR/cancel.txt` with a reason to stop an
active build while retaining its diagnostics. SDK analyzers are off for these
large initial builds; `--analyzers` enables them. NativeAOT compiler warnings are
errors when `--warnings-as-errors` is selected.

## Runtime and configuration

The musl profile uses selected musl implementations plus the shared portable
POSIX/BCL services. The [minimal libc++ provider](../backend/self/libcxx.md)
supplies actual upstream allocation, new-handler, exception classes and RTTI
support needed by this compiler. There are no throwing placeholders for missing
EDG imports. This dependency closure does not constitute complete libc++.

The self-specific [profile](../backend/self/profile.py) explicitly records the
portable LP64 host's pointer/IL-prefix alignment and preserves the original
compiler's x86-64 output target, half-precision support and multibyte buffer
bound. It does not inject native OS/CPU macros into generic portable headers.
The output target is distinct from the OS/CPU executing the managed compiler.

`USE_MMAP_FOR_MEMORY_REGIONS=0` selects EDG's supported malloc/read fallback for
compiler memory and module storage. Portable C++/CLI metadata mapping remains
enabled and uses the shared POSIX provider. CLI/CX acceptance still needs their
platform metadata; missing-metadata diagnostic tests do not establish complete
CLI/CX program support. Windows finite CPU limits report `ENOTSUP`; Unix limits
apply to the process. See [POSIX/stdio ownership](posix-stdio.md).

The custom backend explicitly enables EDG's supported
`PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE` and `MAINTAIN_NEEDED_FLAGS` settings, as
needed for alternate-entry placement and merged inline definitions. It keeps
unused IL available for source/lifetime provenance. No EDG semantic fix is
involved in these configuration selections.

## Review and verification

The review entry point is
[`artifacts/review/edg/README.md`](../artifacts/review/edg/README.md).
Generated source retains ordinary names, locals, named record members and
structured control flow where the languages permit it. Constant tables use
source-named pointer aliases, object initializers and embedded typed spans;
large initialization is split into bounded methods while preserving order.
UTF-8 literals have previews. Variadic calls use ordinary named calls and shared
`Edg.Runtime.VaArg` spans, with no `__edg_varcall` helpers.

Exception-enabled output records construction progress and performs cleanup in
`finally`; empty noexcept functions omit redundant catch guards. Some lowered
control flow and expression helpers remain where required by C/C++ semantics.
Source grouping is the default, with `--split size`, `function`, and `none`
also available.

Run the native-versus-translated execution corpus with:

```sh
python3 edgcpp/backend/tests/self_execution_verify.py \
  --output-dir /path/to/new-verification \
  --managed-command dotnet /path/to/new-self-output/managed/bin/Release/net10.0/Generated.dll
```

For NativeAOT, pass the published executable as `--managed-command`. The runner
compares parsing, preprocessing, diagnostics and extension modes with native EDG,
and compiles and executes generated C from both C11 and C++20 inputs. Generated
C text equality is informational: EDG's temporary names can depend on IL
addresses, even between two native runs. Diagnostics and executed program
behavior must match. Real-header host-profile and merged-inline reductions
remain separate permanent regression tests.

`backend/tests/self_source_verify.py` additionally parses original `cfe.c`,
`types.c`, and `ifc_modules_read.c` with the same explicit portable input macros
and headers for both compilers. Pass `--profile-results` with the self report,
`--output-dir` with a new directory, and `--managed-command` as above. All three
match native diagnostics and exit codes in JIT and NativeAOT. In the recorded
single-run AOT checks, they took 0.689/0.975/2.241 seconds, versus native EDG's
0.166/0.221/0.596 seconds. These are observed timings, not a controlled benchmark.

The full Release C# build took 392 seconds and NativeAOT publication 424 seconds
(including its separate RID-specific C# compilation), with peak process-tree
RSS below 18 GiB. SDK analyzers were disabled; C# and NativeAOT compiler warnings
were errors. The emitted owner has 57,803 methods, below the CLR limit.

The self report records original source hashes, native definitions, configuration,
header search paths, exact commands, compiler hash and runtime/license provenance.
`raw.cs` is retained before formatting and packaging. A failed translation has
no complete generated project. `dotnet.py --raw-input PATH` can replay packaging
of a retained raw file, but does not establish native translation provenance;
keep its original report. The driver rejects unbound externs by default;
`--unbound-externals partial` or `declarations` is an explicit diagnostic/review
mode and does not supply working implementations.
