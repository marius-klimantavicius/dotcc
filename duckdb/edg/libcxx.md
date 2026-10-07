# DuckDB's translated libc++ source provider

`libcxx.py:prepare_provider(edgcpp_root, acquisition, output)` assembles LLVM
libc++ **18.1.8** from the existing pinned acquisition. It returns a manifest
with ordered translation units, include directories, preinclude headers, source hashes and the
selected C++17 or C++20 language mode. Use those inputs with `--libc musl --posix`; retain
exceptions and RTTI. The native comparison build must use the same language
standard. Never add EDG's bootstrap `backend/runtime/include` headers alongside
the real libc++ declarations.

The current assembly includes 44 implementation inputs: the existing managed
allocation, exception, atomic and thread ABI adapters; upstream allocation,
shared ownership, locale, streams, containers' out-of-line definitions,
synchronization, thread/future/chrono, string, regex, random, charconv and Ryu
implementations. All acquisition headers, configuration and implementation
files are verified before assembly. Configuration must also match the current
managed ABI configuration in the repository. An older acquisition can be
re-created with `backend/runtime/libcxx/acquire.py` using its retained archive.
The acquisition and original LLVM sources are not changed.

Apply every `preincludeHeaders` entry to every C++ application and provider TU
before its first libc++ include. The tracked `libcxx-preinclude.h` selects
upstream's `_LIBCPP_NO_ABI_TAG` opt-out and clears `_LIBCPP_ALWAYS_INLINE` after
loading `__config`. This gives implementation and consumer declarations the
same managed ABI annotations; JIT/NativeAOT still choose whether to inline.
Provider wrappers include this header before the implementation preamble.
Preprocess those wrappers before the final whole-program invocation as well: do
not forward the libc++ search path to the musl C inputs added by `dotnet.py`.
The manifest records its hash, and the stable tracked path allows preprocessing
cache identities to remain stable across provider staging directories. These
are macro configuration changes; no preprocessed declarations are rewritten.

The implementation source directory follows the configuration and public
include directories in the include search path: Ryu's private nested includes
require it. The selected external threading ABI uses `LibcxxThreads.cs`, and
`random_device`'s default `/dev/urandom` path reaches the existing BCL-backed
POSIX virtual device. Neither selects the translation host's native C++ ABI or
native libc. The portable FILE ABI uses 64-bit `long` and `off_t` on every host;
`fseeko`/`ftello` share the existing managed `Fseek`/`Ftell` implementation.

The existing core source profile, explicit string templates and explicit stream
templates form the primary provider TU. Additional implementations preserve
their upstream TU boundaries by default. `unity_provider=True` assembles all
selected library implementations in the primary TU, including Ryu; the selected
revision has no conflicting private definitions in that ordering. Private
stringizing macros from `iostream.cpp` are cleared before subsequent headers.
`primary_consumers` can include a targeted
consumer after the implementation definitions when EDG ownership requires it;
the implementation macros are cleared first. This option does not establish
that arbitrary application TUs can be combined without symbol collisions.

`duckdb-libcxx-003/syntax.json` in the retained campaign artifacts records
successful individual EDG C++20 syntax checks for all 15 generated TUs. This is
**not** merged-program translation or runtime acceptance. The module deliberately
records this limit in its manifest. Full DuckDB translation and its upstream
tests determine additional source and platform requirements; the source
profile is not a claim of complete libc++ support. `sys/uio.h` now describes the
portable `iovec` layout for bundled Snappy; merely declaring `readv`/`writev`
does not provide implementations, and actual calls must fail unresolved-import
validation until their shared runtime implementations exist.

C++17 implementation compatibility is confined to generated, hashed copies and
scoped macro spellings. Eight implementation files use `_LIBCPP_CONSTINIT` for
objects whose initializers are already constant in C++17. The arithmetic-only
sort instantiations use `less<>` and the same count-leading-zero expression
instead of the C++20 `ranges::less`/`__bit_log2` wrappers; introsort and its
partition policy are unchanged. Five private floating `to_chars` casts call
the same `__builtin_bit_cast` intrinsic that the C++20 `std::bit_cast` wrapper
calls. The original library sources and public declarations stay intact.

The all-source provider with an included synthetic consumer translates,
compiles and runs under JIT and NativeAOT in `duckdb-libcxx-probe-012/`. It
exercises strings, stringstreams, synchronized threads, regex, a random device,
floating formatting and vector bounds exceptions. The repeatable and expanded
acceptance command is:

```sh
python3 duckdb/edg/tests/libcxx_provider_verify.py \
  --acquisition PATH --output-dir NEW_PATH --nativeaot
```

Separate C++ consumer TUs remain a distinct unresolved assembly problem.
`duckdb-libcxx-merge-oracle-001/` retains identical portable preprocessed inputs
that cause both the unchanged EDG C backend and the C# backend to report the
same correspondence assertion before backend emission. Reordering the consumer
and suppressing explicit library template ownership in experimental overlays
did not resolve it; those experiments are retained outside the source recipe.
No EDG frontend semantics or correspondence checks were changed.

A later isolated proof, `duckdb-libcxx-merge-odrvalue-004/results.json`, passes
native, JIT and NativeAOT with a separate consumer and strict unresolved-symbol
validation. Its normalized inputs additionally ODR-use
`std::integral_constant<unsigned long, 2UL>::value` and explicitly instantiate
`std::__compressed_pair<int*, std::allocator<int>>` in the primary provider.
These retain definitions needed by that consumer; they are not a general
DuckDB solution and are not included in the source recipe. Existing EDG
configuration options for retaining template definitions were subsequently
tested without frontend changes.

`duckdb-uniform-headers-001/results.json` repeats the separate-TU proof with
the uniform source-level header and the expanded acceptance fixture, passing
native, JIT and NativeAOT. The two explicit retention anchors remain confined
to the separate-consumer verification fixture. An isolated compiler configuration with source-sequence
lists and non-class template-instantiation retention still rejects the
anchorless provider/consumer and the five-TU DuckDB subset; the unchanged C
backend gives the same result (`duckdb-template-retention-verify-001/`).
That configuration also requires EDG's explicit deprecated-SSI opt-in and is
not adopted. Removing the redundant pre-C++17 out-of-class `integral_constant`
member definition alone did not resolve the correspondence rejection either.

The repeatable verifier now supports `--separate-consumer`, with the two
fixture-only anchors recorded in its input hashes. Both the default primary-TU
mode and separate mode pass native/JIT/NativeAOT with the tracked header in
`duckdb-libcxx-uniform-primary-002/` and `duckdb-libcxx-uniform-separate-002/`.
Provider and consumer are preprocessed first; the final merge uses portable
C headers for its separately added musl implementation.


Standard streams need an additional source-assembly adaptation. Upstream
`iostream.cpp` defines aligned byte storage while `<iostream>` declares typed
objects in other translation units. Combining them directly gives conflicting
C++ declarations. The generated private implementation copy keeps each raw
buffer's exact size/alignment, gives it a private C-linkage name, and declares
the public stream object with an explicit assembler label naming that buffer.
Public headers and object types are unchanged; original placement construction,
ties, initialization order and exit flushing remain in upstream code. This
requires backend resolution of explicit assembler-labelled variable views onto
the matching source-defined storage. The overlay is a translation input, not a
recipe for optimized native unity compilation: GCC can assume distinct source
objects despite equal assembler labels. The unchanged EDG C backend provides
the storage-semantics comparison; native fixture expectations use the host's
unmodified standard library.

GNU `alias` attributes were also investigated, but unchanged EDG C rejects the
real multi-TU provider's target lookup. Putting the alias TU last passes a
minimal two-TU case but not three TUs; neither explicit zero initialization nor
an explicit assembler label on the raw target fixed the broad case. Retained
`duckdb-standard-streams-003/` artifacts distinguish those failures from the
assembler-labelled-view probe, which reaches the C# backend with ordinary
provider-first source order. Standard-stream verification now passes in both static and instance modes
under JIT and NativeAOT (`duckdb-standard-streams-004/results.json`). The
repeatable `tests/libcxx_standard_streams_verify.py` checks real stdin,
stdout/stderr, all eight object types, early initialization and redirected
wide-stream input/output against the native oracle. All externs resolve; no
partial runtime implementations are used.

The provider also scopes `locale.cpp`'s `<langinfo.h>` macros with push/pop,
including the include guard. This prevents an implementation-only `ERA` macro
from replacing DuckDB's `DatePartSpecifier::ERA`, while a later application
`#include <langinfo.h>` still exposes the public constants. The header and
macro list are recorded in the provider manifest.

An isolated upstream DuckDB core amalgamation plus all provider implementation
sources passes semantic preflight (`duckdb-amalgamation-002/`). Its inventory
covers 1,476 of the configured 2,207 library translation units; 731 remain,
including the static extensions, ICU, generated PEG transformer and generated
extension loader. The upstream dummy extension loader is excluded so the
configured seven-extension loader can supply the real implementation. Included
`utf8proc_data.cpp` is a textual dependency of the selected `utf8proc.cpp`, and
`allocator_jemalloc.cpp` is guarded by the disabled jemalloc option. This is an
assembly investigation, not full DuckDB execution acceptance.

The corrected `duckdb-amalgamation-004/` input preserves the native manifest's
exact source ID (`069cc9f9b5`) and RE2_ON_VALGRIND setting. The latter controls
sparse-table initialization and must not disappear when replacing individual
RE2 compilation units with the upstream amalgamation.

The broader primary-provider regression also passes with the stream storage
overlay under native/JIT/NativeAOT (`duckdb-libcxx-asm-primary-001/results.json`),
including arithmetic sort instantiations, floating formatting, string/stream
operations, regex, synchronized threads, random-device access and exceptions.
