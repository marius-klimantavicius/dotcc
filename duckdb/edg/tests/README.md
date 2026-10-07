# DuckDB upstream verification

`verify.py` runs the pinned DuckDB **upstream `unittest` executable and its
translated equivalents**. The SQLLogicTest parser, commands, result checks,
hashes, sorting, NULL handling, errors, loops, named connections, concurrency,
and query verification remain DuckDB's own implementation. No SQL statements
or expected results are extracted into a simplified replacement runner.

The native, translated JIT, and translated NativeAOT runners must be built from
the same upstream version, static extension configuration, and test sources.
Dynamic extension loading must be disabled in each build. Bundled dependencies
and statically linked extensions are eligible for translation and verification;
tests requiring unavailable external/loadable extensions are not counted as
passes. Autoloading is additionally disabled at test execution time.

## Building the runner

Include `reporter.cpp` as an extra source in the upstream `unittest` target and
the corresponding EDG translation. It adds `--reporter edg-json` while retaining
the upstream console diagnostics. No upstream test or runner files need edits.
The upstream CMake test target records its full translation-unit closure in
`compile_commands.json`; translating just `test/unittest.cpp` is insufficient.
The closure includes the Catch implementation, `test/sqlite/`, test helpers,
and enabled C++/C API tests, linked to the translated engine and static
extensions. Reporter control fixtures are **not** part of that target.

The additional reporter is necessary because DuckDB's bundled Catch has a
`skippedTests` counter that its XML and JUnit reporters do not expose. A skipped
SQLLogicTest can otherwise appear successful. Our reporter emits actual case
identities, assertion counts, skip counts, and a completed-run footer.

## Running

Create a JSON file containing argument arrays, with paths to the actual built
executables/assemblies (no shell interpolation):

```json
{
  "native": ["/path/to/native/test/unittest"],
  "jit": ["dotnet", "/path/to/translated/Unittest.dll"],
  "aot": ["/path/to/published/Unittest"]
}
```

Then run:

```sh
python3 duckdb/edg/tests/verify.py \
  --source /path/to/pinned/duckdb/source \
  --runners /path/to/runners.json \
  --provenance /path/to/source-manifest.json \
  --provenance /path/to/translation-receipt.json \
  --output-dir /mnt/storage/dotcc-edg-campaign-20261003/duckdb-tests-001
```

Use `--baseline-only` and a JSON file containing only `native` to establish the
native reference while translation is in progress. The resulting status is
`native-baseline-passed`, never a translation pass. The normal mode requires
all three runners; it cannot silently omit JIT or NativeAOT verification.

`initial-suite.json` explicitly selects ten upstream test cases: three C API
tests plus projection, joins, aggregation, ordering, prepared statements,
prepared-statement errors, and transactions. This is initial coverage, not the
full suite. `--suite` accepts the same schema with other upstream Catch
selectors, including groups, to expand coverage. A selector group must execute
at least one case. Review wildcard/composite selections against the retained
case inventory; the harness does not infer that every intended case exists.

`bundled-suite.json` adds seven exact upstream tests for JSON, ICU collation,
Parquet reading/compression and writing, autocomplete, TPC-H, and TPC-DS. The
benchmark-extension cases use scale factor zero to exercise schema creation
and query execution; they do not validate populated benchmark datasets or
performance. Their `require` directives must resolve to static extensions;
missing-extension skips are rejected. These selections do not imply every
bundled dependency or every extension behavior has been exercised.

Each group runs in a separate process and uses its own retained temporary data
directory. The upstream runner may itself delete its per-case scratch files;
the harness does not clean intermediate directories, generated sources, or
logs. Commands, controlled test environment, executable/provenance hashes,
test-source/data hashes, stdout/stderr, and per-case observations are retained.
An already existing output directory is rejected. Inherited `DUCKDB_TEST_*`
configuration is removed to prevent accidental skips or relaxed comparisons.
Thread limits are set to four for reproducibility, without forcing all queries
to a single thread. The upstream random seed is fixed.

Acceptance requires successful process exits, complete receipts, nonempty
assertions, no skipped or expected-failure cases, and identical case identities
and assertion totals between native/JIT/AOT. Upstream checks perform the actual
SQL result/error validation. This is not a byte-for-byte comparison of every
engine result, and it does not prove tests outside the selected corpus pass.
Supplied provenance files are hashed, not semantically audited; full translated
dependency/no-native-fallback audits remain the provider/build's responsibility.

## Reporter controls

```sh
python3 duckdb/edg/tests/check_reporter.py \
  --catch-include /path/to/pinned/duckdb/source/third_party/catch \
  --output-dir /mnt/storage/dotcc-edg-campaign-20261003/duckdb-reporter-controls-001
```

This compiles the pinned upstream Catch with the reporter and small harness
fixtures. It checks acceptance of a normal case with sections and escaped
names, and rejection of skipped, failed, expected-failure, empty, and unmatched
cases. These are **harness controls only**, not DuckDB engine or translation
verification. Their receipts explicitly distinguish that scope.

## Focused libc++ provider verification

```sh
python3 duckdb/edg/tests/libcxx_provider_verify.py \
  --acquisition /path/to/verified-libcxx-acquisition \
  --output-dir /path/to/new-provider-test --nativeaot
```

This compares a synthetic C++17 consumer against a native executable, JIT and
NativeAOT. It exercises strings, streams, threads, mutexes, regex, randomness,
exceptions, arithmetic sorting and floating formatting. C++ inputs are
preprocessed with libc++ headers; the final compilation adds musl with its own
C headers, without leaking the libc++ search path into those sources.

Add `--separate-consumer` to place the consumer in its own translation unit.
That focused proof explicitly includes `libcxx_provider_retention.cpp` in the
primary provider: an ODR-use of one constant and an explicit class instantiation
retain definitions needed by this fixture. These anchors are not installed in
the general provider and do not establish full DuckDB source merging. Both
modes reject unresolved externs; neither substitutes partial implementations.

## Pair signature correspondence regression

```sh
python3 duckdb/edg/tests/pair_signature_verify.py \
  --acquisition /path/to/fresh-libcxx-pair-acquisition \
  --composition /path/to/verified-library-composition/composition.json \
  --output-dir /path/to/new-pair-test
```

This checks the pinned vendor assertion and its original-header patch, then
preprocesses ten actual DuckDB pair types as copy-use and size-use translation
units. The unchanged stock C frontend must accept all adapted pairs and report
the expected correspondence diagnostic for the eight affected original pairs.
Each input's header origin and before/after hashes are verified. This is a
bounded semantic-merge regression; native and translated execution evidence is
recorded separately in the campaign. It does not count as a full DuckDB build.
