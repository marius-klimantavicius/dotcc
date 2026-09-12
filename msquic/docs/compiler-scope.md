# MsQuic: initial compiler scope

Date: 2026-09-12. Historical baseline before phase implementation: no
compiler/libc changes were made during this experiment, and nothing was committed.

## Source and experiment

The current upstream `main` was resolved through GitHub's commits API to
[`80a065112426bce68c1da42d026478d3e40fd45e`](https://github.com/microsoft/msquic/commit/80a065112426bce68c1da42d026478d3e40fd45e),
dated 2026-09-09, "Fix OpenSSL TLS handshake record processing (#6309)".
Its version header says 2.7.0. This is a development snapshot, not the plan's
v2.6.1 release baseline.

The archive and unchanged extracted tree are in
`ref/msquic-80a065112426bce68c1da42d026478d3e40fd45e*`.
[source.json](../config/source.json) records the exact URL and SHA-256;
`scripts/fetch.py` verifies the archive and every archived source file.
Git submodule contents were not fetched: these probes need neither native TLS
providers nor GoogleTest/CLOG implementations.

The selection is all **39 C files in `src/core/CMakeLists.txt`**, plus the five
platform candidates `crypt.c`, `hashtable.c`, `pcp.c`, `platform_worker.c`, and
`toeplitz.c`: **44 translation units, 51,437 C source lines**, excluding headers.
This deliberately surveys the full core source list, including optional code;
it does not establish the final managed product's feature/source closure.

Environment: Linux x64, .NET SDK 10.0.111, GCC 13.3.0; dotcc commit
`428348c896f99a0b7aef0ad15d63bb41f50a0640`. The compiler Release build passed with
zero warnings/errors. Binary hashes are in `artifacts/environment.json`.

Defines: `CX_PLATFORM_LINUX=1`, `__linux__=1`, `_GNU_SOURCE=1`, `NDEBUG=1`,
`QUIC_BUILD_STATIC=1`, `QUIC_EVENTS_STUB=1`, `QUIC_LOGS_STUB=1`.
The Linux identity macro only selects upstream headers for this experiment;
it does not give the generated library a Linux datapath implementation.

## Results

| Check | Result |
| --- | --- |
| GCC syntax check of all 44 original files, GNU C17 + `-fms-extensions` | 44/44 accepted, no diagnostics |
| dotcc original source, configured Linux defines | 0/44 emitted; every unit reaches the shared function-typedef parser failure |
| dotcc diagnostic source copy | 44/44 object fragments emitted |
| Link diagnostic objects as a managed library | Project emitted successfully |
| Roslyn Release build | Fails with 8,182 errors and 6 warnings |
| Runtime, transport, TLS, NativeAOT | Not reached |

GCC also accepts all 44 mechanically adjusted files when using native system
headers. The diagnostic declarations used with dotcc are not a working PAL or
a validated ABI. Emission is a useful measure of how far parsing/binding gets,
not evidence of transport correctness.

The final Roslyn errors group into 7,939 unresolved-name diagnostics (123
distinct names), 239 missing-member accesses, two missing-member initializers,
and two out-of-range constant conversions. These are repeated across emitted
inline helpers and call sites, not 8,182 independent defects. The 241 member
errors match the anonymous-aggregate issue; the two constant errors match the
high-bit reproducer. Host services dominate the unresolved names.

## Independently reduced compiler gaps

Each linked example is accepted by GCC with the selected GNU/MS-extension
profile. Small emission/build receipts live in `artifacts/reproducers/`.

| Area | Evidence and impact |
| --- | --- |
| Function-type typedef declarations | Both [`typedef void (Callback)(int)`](../probes/parenthesized-function-typedef.c) and [the unparenthesized form](../probes/function-typedef.c) fail parsing. Upstream uses these throughout TLS/datapath/API callbacks, including declarations of functions through typedef names. |
| GNU attribute forms and positions | [`struct __attribute__((aligned(16)))`](../probes/struct-attribute.c) and [`__attribute__((noinline, noreturn))`](../probes/function-attributes.c) fail parsing. `no_instrument_function` and `always_inline` also obstruct the common headers. Existing support for other attribute forms does not cover these. |
| Empty file-scope declarations | [`;` after a declaration](../probes/empty-declaration.c) fails. Upstream's C `DEFINE_ENUM_FLAG_OPERATORS` expands to nothing and its static-assert macro supplies its own semicolon, leaving this syntax. |
| Multi-character integer constants | [`'CIUQ'`](../probes/multichar-pool-tag.c) fails lexing. Allocation tags use this spelling extensively. The target needs an explicit implementation-defined packing policy, not a Unicode character interpretation. |
| Nested GNU variadic macros | [Logging-shaped reproducer](../probes/nested-variadic.c) expands arguments into the wrong positions and leaves `__FILE__` unexpanded. GCC produces `sink(format, "key", 42, file, line)`; dotcc produces malformed token sequences. Both preprocessor outputs are saved. |
| MS anonymous members | Both [typedef-based](../probes/anonymous-typedef-member.c) and [named-tag](../probes/anonymous-tag-member.c) anonymous members fail parsing. This is an actual upstream `-fms-extensions` dependency, used to embed base handle/datapath fields. |
| Runtime aggregate initialization | [`struct timespec ts = {0, 0}`](../probes/timespec-initializer.c) fails binding with "aggregate initializer for unknown struct/union 'timespec'". The runtime owns this type, but the initializer binder lacks its field model. |
| Nested anonymous member promotion | [Anonymous union containing an anonymous bitfield struct](../probes/nested-anonymous-bitfields.c) emits, then fails C# compilation: promoted members such as `Type` and `LEN` are absent on the outer emitted struct. This occurs in actual QUIC frame and packet headers. |
| GNU intrinsics | [`__sync_add_and_fetch`, `__atomic_load_n`, `__builtin_bswap32`](../probes/gnu-intrinsics.c) emit as unresolved C# identifiers. The full header uses more `__sync_*` operations and all 16/32/64-bit swaps. Lower them to the existing atomic/runtime model or explicitly adapt their host boundary, with correct memory ordering. |
| Nonstandard include extensions | [`#include "version.inc"`](../probes/include-nonstandard-extension.c) is warned as unresolved although the file exists beside the input; emission nevertheless exits zero, then C# compilation fails. Upstream's `msquic.ver` has the same discovery problem. The include scanner currently only registers `.h` and `.c`. |
| Requested member alignment | [`alignas(16)` member probe](../probes/member-alignment.c) **compiles and runs incorrectly**: GCC prints size/alignment `16 16`, dotcc prints `1 1`. MsQuic uses aligned locks/events/pool structures. The existing documented constraint-checking-only behavior is insufficient for these layouts. |
| High-bit integer/enum conversions | [Receive-mask and send-flag examples](../probes/high-bit-conversions.c) emit C# casts that fail `CS0221`: `0x8000000000000000UL` to `long`, and `0x80000000U` to an enum. Preserve the selected C conversion semantics using the appropriate unchecked representation. |

Callback aggregate initialization was also checked in single-file and separately
compiled two-file controls. Both controls build. Two callback errors in the first
full diagnostic build were traced to the probe's initial typedef substitution,
which mistakenly turned function declarations into pointer variables. The probe
now expands those declarations to explicit function prototypes. They are not
counted as an independently demonstrated compiler defect.

These are **12 grouped gaps**, not an estimate of 12 patches. GNU attributes,
anonymous aggregate storage/access, callback type semantics, and atomics each
cross multiple compiler stages. More runtime or optimizer defects may be hidden
behind the current build failures.

## Separate platform and integration work

The baseline also warns about missing `netdb.h`, `netinet/ip.h`, `sys/syscall.h`,
`sys/epoll.h`, and `sys/eventfd.h`. The probe supplies declaration-only headers
and a few extra pthread declarations. These do not implement their functions.

Most unresolved names belong to the intended host boundary: allocation/asserts,
time/processors, reference/rundown operations, worker execution, UDP/datapath,
storage, TLS, entropy, and cryptography. Missing POSIX constants and services
include error codes, `CLOCK_MONOTONIC`, `in6addr_loopback`, `strnlen`, pthread
rwlocks, and condition-variable clock selection.

The plan calls for a BCL CxPlat layer and translated picotls. Native epoll/TLS
implementations are therefore not automatically compiler features to port.
No picotls bridge, UDP host, credentials, scheduling adapter, native TLS backend,
or application-owned native imports were added by this experiment.

## Diagnostic accommodations

`scripts/probe.py --stage syntax` creates `artifacts/probe-source/` from the
reference tree. `artifacts/probe-source.patch` records every source difference.
The reproducible transformations are:

1. Replace function-type typedefs with pointer typedefs, adjust pointer uses,
   and expand bare function declarations to explicit prototypes.
2. Expose the version header under a `.h` name in the diagnostic tree.
3. Remove unsupported aligned-struct attributes; replace the bugcheck attribute
   with `_Noreturn`; omit instrumentation/inlining hints.
4. Make tracing macros no-ops, including argument evaluation, to bypass their
   independently reproduced macro-expansion failure.
5. Remove the empty file-scope declarations left by enum/static-assert macros.
6. Convert ASCII allocation tags to GCC-equivalent numeric constants.
7. Expand the selected typedef/tag anonymous members to C11 anonymous bodies.
8. Replace the timespec positional initializer with explicit member stores.

`config/probe-headers/` contains the declaration-only system-header additions.
Its epoll struct has no enforced packing, and pthread rwlocks use an opaque
probe handle. **Neither these declarations nor the adjusted sources may be
used to claim layout, concurrency, or runtime correctness.** Generated C# was
never hand-edited. Compiler and libc sources remain unchanged.

## Reproduction and evidence

Run the commands in [README.md](../README.md). For a quick iteration:

```sh
python3 msquic/scripts/probe.py --stage syntax --units frame crypt
python3 msquic/scripts/probe.py --stage syntax --units crypt --preprocess
```

The full 44-unit syntax probe must be rerun before `build-probe.py`; the selected
unit command replaces that stage's result file. The scripts record individual
commands as JSON argument arrays and keep full logs.

| Evidence | Path under `msquic/` |
| --- | --- |
| Source integrity and toolchain | `config/source.json`, `ref/snapshot.json`, `artifacts/environment.json` |
| Original native syntax control | `artifacts/native-syntax.command.json`, `artifacts/native-syntax.log` |
| Adjusted-source native syntax control | `artifacts/native-diagnostic-syntax.command.json`, `artifacts/native-diagnostic-syntax.log` |
| Baseline unit results | `artifacts/baseline/results.json` |
| Adjusted unit results and exact invocations | `artifacts/syntax/results.json`, `artifacts/syntax/*.command.json` |
| All diagnostic modifications | `artifacts/probe-source.patch`, `config/probe-headers/` |
| Managed library build | `artifacts/scope-library-results.json`, `artifacts/scope-library-build.log` |
| Categorized Roslyn diagnostics | `artifacts/scope-library-errors.json` |
| Reduced parser/preprocessor results | `artifacts/reproducers/results.json`, `*.gcc.i`, `*.dotcc.i` |
| Reduced C# build/runtime results | `artifacts/reproducers/build-results.json`, `*.build.log` |

The next compiler work should establish correct preprocessing/declarations,
then anonymous aggregate access, intrinsic lowering, constant conversions, and
actual storage/alignment. The fact that all adjusted units emit is encouraging
for the breadth of existing C support, but it does not bound the later runtime,
NativeAOT, TLS, or transport-validation effort.
