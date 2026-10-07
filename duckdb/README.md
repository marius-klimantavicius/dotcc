# DuckDB through EDG

This campaign translates DuckDB v1.5.6 and its bundled dependency closure to
unsafe C#, using the shared EDG musl/libc++ runtime. Dynamic native extension
loading, automatic installation and automatic loading are disabled. The native
reference includes the seven bundled static extensions: core_functions,
parquet, icu, json, autocomplete, tpcds and tpch.

The native reference has built successfully. All 2,207 selected library TUs pass
individual EDG C++17 semantic checks, and the entire source graph preprocesses
successfully. Portable runtime gaps have native/JIT/NativeAOT verification.
**DuckDB C# execution is not yet
verified.** Whole-program libc++/DuckDB source merging and subsequent generated
code/runtime verification remain work in progress.

## Reproduce the inputs

Run from the repository root; choose new output directories on a volume with
space for native objects and retained preprocessed C++.

```sh
python3 duckdb/edg/native.py --source-dir /path/duckdb-source \
  --build-dir /path/duckdb-native --std 17 --jobs 12
python3 duckdb/edg/portable.py --manifest /path/duckdb-native/sources.json \
  --output-dir /path/duckdb-portable
python3 duckdb/edg/translate.py \
  --portable-manifest /path/duckdb-portable/portable.json \
  --acquisition /path/verified-libcxx-acquisition \
  --output-dir /path/duckdb-translated --std c++17
```

`acquire.py` verifies the pinned archive, every regular file and symbolic link,
and rejects additional source files. `sources.py` reads the CMake target graph
and compile commands: 2,207 library TUs, 148 test TUs including the reporting
adapter, and 17 shell TUs in the initial profile. The dependency inventory
records compiled sources, header dependencies and unselected data/alternatives.
The optional jemalloc allocator is replaced by the shared NativeMemory-backed
malloc/new implementation. The external Delta extension and loadable-only demo
extension are outside this static bundled profile.

`portable.py` freezes the source, generated extension loader/configuration and
authored test reporter. Its narrow DuckDB-source adaptations recognize our
portable binary64 arithmetic target in ICU double-conversion's architecture
guard and select concurrentqueue's existing POSIX semaphore branch. They do
not advertise a host CPU or change those algorithms. The original archive
remains unchanged.

`translate.py` preprocesses every CMake TU independently, preserving defines
and header search order, then requests a complete EDG program with strict
unresolved-symbol rejection and source-split C#. Add `--target tests` to include
the upstream test runner. `--preprocess-only` stops at a clearly labelled input
preparation result. No preprocessing or frontend-only result is an execution
pass. Private libc++ source adaptations and the source-assembly limitations are
documented in [edg/libcxx.md](edg/libcxx.md).

After a complete preprocessing run, add
`--reuse-preprocessed /path/to/previous/results.json` to a new attempt with a
new output directory. The driver verifies the ordered source list, compiler,
portable-source manifest, preprocessing flags, environment, public headers,
forced headers and every retained `.ii` hash. It also checks the files named by
preprocessor line markers; time-dependent macros disable automatic reuse.
Missing or changed inputs and older receipts without these fingerprints are
rejected. A failed later translation can still supply a valid preprocessing
cache. Changes to provider implementation assembly alone do not invalidate
application preprocessing, while changes to its public header contract do.
Provider TUs are always preprocessed afresh. The final translator receives only
preprocessed provider/application inputs, so libc++ header search paths cannot
affect its separately compiled musl sources. Reuse carries no previous
translation or execution claim. Run the focused provenance checks with
`python3 duckdb/edg/tests/check_preprocessing_cache.py`.

Bundled PCG uses `__DATE__` and `__TIME__` for a compile-time seed. For a
reusable, reproducible cache, explicitly pass `--preprocessing-epoch 0` on both
the preparation and reuse commands. This selects **January 1, 1970 00:00:00
UTC**, a chosen reproducibility epoch, not the release timestamp. The driver
records GNU-mode macro overrides in every preprocessing command and checks
the clock policy as part of compatibility. The default leaves the compiler
clock unchanged and refuses reuse when these macros occur. `__TIMESTAMP__`
and embedded-file directives still disable automatic reuse. The existing
native baseline was compiled without this fixed-clock policy; matching clock
configuration remains part of eventual differential verification.

`--compiler` selects final code generation. By default it also preprocesses
the inputs. To reuse a cache while testing a newer backend, explicitly select
the cache's retained binary with `--preprocessing-compiler /path/to/cpfe-dot`
and select the new backend with `--compiler /path/to/new/cpfe-dot`. Both binaries
are snapshotted and verified independently; changing the selected preprocessing
compiler still invalidates reuse. Older complete receipts using one compiler
retain that binary as their preprocessing identity.

Both C++20 and matching C++17 native baselines have built and passed the same
17 selected upstream cases (2,848 assertions). Some bundled RE2 targets retain
their upstream C++11 override.
Portable whole-program input deliberately uses a uniform standard, currently
C++17, to avoid unrelated C++20 constrained-template merge requirements. Original
per-TU standards and the adaptation are recorded. The C++17 reference records
2,332 library/test TUs using C++17 and 23 RE2 TUs using C++11. Final acceptance still requires translated execution of those
same upstream tests.

## Verification

[edg/tests/README.md](edg/tests/README.md) describes the upstream test runner,
selected SQLLogicTest/C API tests and bundled-extension tests. The reporting
adapter records assertions and rejects skipped, empty and expected-failure
runs. It leaves DuckDB's SQL parsing and result/error comparison unchanged.
Native-only results are explicitly labelled baseline evidence; final acceptance
requires translated JIT and NativeAOT runs of the same cases.

The shared runtime prerequisites include `realpath` with symlink traversal and
translated-heap ownership, portable 64-bit `fseeko`/`ftello`, musl string
duplication, semaphores, physical-memory queries/locking/discard, filesystem
statistics, terminal sizing and scheduling queries. Linux x64 native, JIT and
NativeAOT checks pass in both static and instance modes. The Windows and macOS
runtime branches still need execution verification for this campaign.

The real libc++ implementation also passes focused native/JIT/NativeAOT checks
for strings, streams, threads, mutexes, regex, random generation, arithmetic
sorting, floating formatting and exceptions, with the consumer in its primary
TU. An isolated separate-consumer probe also passes native/JIT/NativeAOT after two
explicit source-retention anchors. The general separate-consumer assembly still
exposes an EDG correspondence assertion; identical preprocessed inputs reproduce
it in the stock C backend.
This is a source-assembly limitation, not evidence of completed DuckDB emission.
The full-library attempt currently stops at libc++ implementation/consumer
declaration correspondence, before C# emission. All dependency sources remain
selected; no unresolved runtime placeholders were used to claim an execution pass.

The [review directory](../edgcpp/artifacts/review/duckdb/README.md) currently
contains a partial `hugeint.cpp` translation for inspection, including its exact
C++ input. It has five unresolved DuckDB function contracts and has not been
built or run. It will be replaced by the complete library output when available.

All generated sources, compiler invocations, native objects, logs and failed
attempts are retained under `/mnt/storage/dotcc-edg-campaign-20261003/duckdb-*`.
