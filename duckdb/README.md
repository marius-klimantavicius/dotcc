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
not advertise a host CPU or change those algorithms. Correspondence
adaptations also rename the unused private `NumericCast` struct and select the
existing `pair<string, LogicalType>` and `pair<string, Value>` copy-assignment
signatures in unevaluated expressions. Bundled fmt spells two existing enum-to-integer conversions
explicitly in its argument-store constants. The public `NumericCast` function
template, enum types, packing/layout and libc++ algorithms remain unchanged. The original archive and earlier frozen trees remain intact;
[the source-assembly notes](edg/libcxx.md) record the reduced proofs and limits.

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

## Core amalgamation and full source composition

`edg/amalgamate.py` invokes the pinned upstream amalgamation generator and
records the exact configured partition: **1,476 core TUs plus 731 remaining
TUs**. All seven static extensions, their dependencies and the real generated
extension loader remain selected. The upstream dummy loader is excluded; the
manifest explicitly classifies the two additional textual implementation
inputs, records native macro differences by source index, and enables the
native RE2 initialization flag `RE2_ON_VALGRIND`. It preserves the exact native
source ID `069cc9f9b5` and release `v1.5.6`.

```sh
python3 -B duckdb/edg/amalgamate.py \
  --portable-manifest /path/duckdb-portable/portable.json \
  --preprocessed-receipt /path/verified-epoch-zero/results.json \
  --acquisition /path/verified-libcxx-acquisition \
  --output-dir /path/duckdb-composition \
  --preprocessing-epoch 0 --preprocess
```

Raw upstream output remains in `upstream-raw/`. A recorded two-expression
generator overlay gives newly emitted `#line` directives absolute staged
source paths, matching the retained TUs' `__FILE__` values; existing source
directives remain unchanged. This matters because PCG's seed includes the file
name as well as the build date/time. The recipe preprocesses the real libc++
provider and core together using the retained preprocessing compiler and the
same explicit epoch. It produces `composition.json` with source/output hashes,
line mappings, macro deviations and an ordered **732-input** program manifest.
It performs no whole-program merge or execution verification.

The remaining `.ii` files are verified as previously built program inputs,
including their original receipt, dependency hashes and preprocessing flags.
The manifest records inventory differences and makes no fresh cache-hit claim;
the stricter `translate.py --reuse-preprocessed` check remains unchanged.
`--unity-inventory` can attach the recorded
[`remaining-unity-recipes.json`](/mnt/storage/dotcc-edg-campaign-20261003/duckdb-amalgamation-002/remaining-unity-recipes.json)
investigation as provenance. The later verified fallback uses the actual native
CMake unity files: 29 complete groups cover 557 remaining TUs, with 174 kept
separate. Together with the core, that is 204 inputs and 516 MiB of preprocessed
source. A mixed group containing already-amalgamated core sources is excluded;
all 2,207 original library TUs still occur exactly once. Upstream `--extended`
adds public headers, not extension implementations.

The tracked unity recipe derives complete groups from a fresh native CMake
inventory and verifies them against the portable source graph. Keep the same
acquired source directory used for the base composition:

```sh
python3 -B duckdb/edg/native.py --source-dir /path/duckdb-source \
  --build-dir /path/duckdb-native-unity --std 17 --configure-only --unity
python3 -B duckdb/edg/unity.py \
  --composition /path/duckdb-composition/composition.json \
  --unity-manifest /path/duckdb-native-unity/sources.json \
  --output-dir /path/duckdb-unity-composition --preprocess
```

Without `--preprocess`, the unity tool emits an `inventory-only` receipt and
planned inputs. Only `preprocessed-composition` receipts are ready for the
merged translator. Fourteen focused checks cover coverage, input changes,
flag/include-order mismatches, and real EDG preprocessing of a tiny group.

Additional source groups are an explicit option, separate from the upstream
CMake groups. Pass `--extra-groups /path/approval-evidence.json` and
`--extra-groups-sha256 SHA256` to the same command. The recipe checks exact
ordered membership, source hashes, compiler options and the native comparison
observations. An inventory-only experimental plan cannot be preprocessed.
The default remains the upstream groups only.

An audited manifest can coalesce complete existing upstream groups only through
`replacesUpstreamGroups` receipts and an exact `sourceTargets` map. Partial or
duplicate replacement is rejected. Ordered prior receipts bind the cumulative
native archives and actual link. Physical-line parsing preserves C++ comment
semantics; the default upstream grouping remains unchanged.

Additional manifests and their checksums may be repeated in order. A composed
native comparison must bind the prior approval, unchanged archive members and
actual link inputs. The verified Parquet follow-up combines 17 implementation
files while keeping `column_reader.cpp` separate. Together with ICU and JSON,
this produces 148 inputs covering all 2,207 selected TUs exactly once. Native
comparison matches 179 fully executed cases and 14,615 assertions per binary.
Two dependency skips and one upstream `mode skip` file remain explicitly outside
acceptance; a reported passed case with zero assertions is not coverage.

The current optional profile combines the 13 ICU extension implementations and
29 JSON implementations into two groups, reducing the input count from 204 to
164 with all 2,207 original library TUs still represented exactly once. Native
comparison rebuilt only these extension objects, retained all other baseline
objects, and passed 115 upstream cases with 12,438 assertions identically. One
additional `read_json.test` case reached 291 assertions before a matching
`require httpfs` skip in both binaries; it is excluded from the fully executed
acceptance count. All seven selected static extensions remain linked. This
supports the explicit grouping experiment; translated execution and its memory
savings must still be measured. See the retained
[native comparison evidence](/mnt/storage/dotcc-edg-campaign-20261003/duckdb-native-extra-unity-002/approval-evidence.json).

The initial 732-input merge was stopped by the coordinator at about 64 GiB RSS,
28 minutes into the run. It also reported two declaration-correspondence
errors, now covered by narrowly verified source adaptations.
The unity fallback has passed preprocessing; full translation and execution
remain outstanding. See the [input-scaling receipt](../edgcpp/evidence/duckdb-input-scaling-20261007.json).

The full merge's memory use belongs to the native EDG frontend, before C#
emission or .NET compilation. EDG keeps separate IL trees and symbol-table
state for the input translation units until final instantiation/merging; it
then removes duplicate entities. Repeated DuckDB/libc++ headers therefore
contribute repeated semantic state even after preprocessing. The corrected
204-input attempt reached 54.8 GiB compiler RSS at ICU before a private-class
correspondence error stopped it. This is a process measurement, not an
allocation-category profile. Further unity grouping must check C++ lookup,
internal storage and initialization behavior, not just preprocessing size.
The stock `--remove_unneeded_entities` option is not a transparent remedy:
it prunes after parsing and can remove public pointer-only type layouts.

The [lowered-IL checkpoint workflow](../edgcpp/backend/README.md#optional-lowered-il-checkpoints)
is available for the next full merge. It preserves a completed frontend result
if later C# emission fails, allowing a compatible backend update to retry
without repeating template instantiation.

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
declaration correspondence in the separate-consumer assembly; the combined-core
assembly has progressed beyond this limitation. Its latest 164-input full run
stopped at the existing 72 GiB compiler RSS guard after 77 minutes, during
deferred template instantiation, without frontend errors or a usable checkpoint.
The 148-input Parquet attempt stopped after 70.8 minutes on an explicit
`pair<string, Value>` declaration-correspondence diagnostic, at 58.85 GiB peak
compiler RSS, below the memory guard. No usable checkpoint was produced.
A separately prepared 129-input core-functions fallback preserves all 2,207
original TUs and all seven extensions. Its native comparison passes 215 fully
executed upstream cases and 18,701 assertions; three matched partial/nonexecuting
observations are excluded. The reviewed grouping recipe is now adopted, but the prepared fallback run is
superseded until the source-signature issue is resolved in a fresh staging tree.
It is not a translated execution pass.
See [fallback evidence](../edgcpp/evidence/duckdb-core-unity-fallback-20261007.json).
All dependency sources remain selected; no unresolved runtime placeholders were used to claim
an execution pass.

The [review directory](../edgcpp/artifacts/review/duckdb/README.md) currently
contains a partial `hugeint.cpp` translation for inspection, including its exact
C++ input. It has five unresolved DuckDB function contracts and has not been
built or run. It will be replaced by the complete library output when available.

All generated sources, compiler invocations, native objects, logs and failed
attempts are retained under `/mnt/storage/dotcc-edg-campaign-20261003/duckdb-*`.
