The managed qualification executable references `Managed.Zstd`, which references
the selected actual generated project through `ZstdProject`. No native zstd
library or alternate codec is a managed backend. The qualification executable
roots the complete translated assembly for NativeAOT; the separate archive
consumer exercises ordinary trimming and project-reference delivery.

| Suite | Assertions |
| --- | --- |
| `abi` | 12 native/emitted upstream stream-layout observations, actual upstream struct storage roundtrip, version/error contract |
| `behavior` | 30 cases: sizes through block boundaries, levels through 22, all nine strategies, LDM, incremental flush/end, stream copies, unknown size, concatenated/skippable frames, standard/raw dictionaries, training, malformed/truncated/checksum errors and output bounds |
| `lifecycle` | Seven cases: repeated reset/dispose, error recovery, concurrent independent contexts, active GC during calls, nested ambient runtime restoration, SafeHandle finalization and validation |
| `interop` | Six managed checks plus three exact comparisons: bidirectional native frame/dictionary exchange, native decoding of managed unknown-size streaming frames, identical compression and training bytes for matching settings/corpus |
| `upstream` | Six groups: three complete bounds/parameter routines, exact fuzzer compression and dictionary-training/use excerpts, exact zstream streaming/skip/dictionary/reuse excerpts; original CHECK macros and actual upstream data generator; native and translated execution |
| `consumer` | Plain and trained-dictionary streaming archives, four restored files including Unicode names and empty content, exact filename inventory and SHA-256 equality |
| `audit` | Canonical source ownership, complete algorithm representatives, no required-symbol native fallback, exact license/notice delivery, managed dependency/import metadata, actual rooted AOT machine code and ordinary consumer/harness ELF dependencies |

```bash
DOTCC_CAMPAIGN_COMMAND_LOCKS=production bash zstd/scripts/test.sh \
  --form all --mode all --suite abi --suite consumer --suite behavior \
  --suite lifecycle --suite interop --suite upstream --suite audit
```

`audit` requires consumer execution and the rooted managed harness from the same
invocation. Native and managed deliberate assertion failures are required
controls: their actual exit code 1 remains in the receipt together with explicit
expected-exit metadata. Other missing, skipped or failed required cases fail the
suite. JSON case reports, matrix rows, native transcripts and input/output files
remain in the current campaign artifact directory, including failure paths.

The upstream suite runs original selected routines and assertion excerpts, not
analogous managed roundtrips. Its report records original line ranges, observed
source/body hashes, original subcase labels and concrete exclusions. The full
zstream driver unconditionally requests two compression workers and cannot run
unmodified against the selected single-thread product; selected exact streaming
assertions run with an authored fixture/cleanup wrapper. The full randomized
fuzzer, shell/CLI tests, allocation-scale stress, experimental sequence-producer
cases and MT/legacy-specific suites are not reported as passes. Authored behavior
cases are reported in `behavior`.

Declaration-only build probes under ignored `build/declaration-only` contain
throw-only signatures for early syntax checking. They must never be executed or
treated as a product, runtime test, interoperability result or qualification.
