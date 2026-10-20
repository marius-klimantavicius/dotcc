The managed qualification executable references `Managed.Zstd`, which references
the selected actual generated project through `ZstdProject`. No native zstd
library or alternate codec is a managed backend. The qualification executable
roots the complete translated assembly for NativeAOT; the separate archive
consumer exercises ordinary trimming and project-reference delivery.

| Suite | Assertions |
| --- | --- |
| `primitives` | Original native and original/replaced translated primitive parity, exact per-unit typed selection, alignment/sentinel checks, three-byte inaccessible-page checks and deliberate fault controls; raw/processed JIT/AOT |
| `abi` | 12 native/emitted upstream stream-layout observations, actual upstream struct storage roundtrip, version/error contract |
| `behavior` | 30 cases: sizes through block boundaries, levels through 22, all nine strategies, LDM, incremental flush/end, stream copies, unknown size, concatenated/skippable frames, standard/raw dictionaries, training, malformed/truncated/checksum errors and output bounds |
| `lifecycle` | Seven cases: repeated reset/dispose, error recovery, concurrent independent contexts, active GC during calls, nested ambient runtime restoration, SafeHandle finalization and validation |
| `interop` | Six managed checks plus three exact comparisons: bidirectional native frame/dictionary exchange, native decoding of managed unknown-size streaming frames, identical compression and training bytes for matching settings/corpus |
| `upstream` | Six groups: three complete bounds/parameter routines, exact fuzzer compression and dictionary-training/use excerpts, exact zstream streaming/skip/dictionary/reuse excerpts; original CHECK macros and actual upstream data generator; native and translated execution |
| `consumer` | Plain and trained-dictionary streaming archives, four restored files including Unicode names and empty content, exact filename inventory and SHA-256 equality |
| `audit` | Canonical source ownership, two exact-width LE24 adapters, 747 intrinsic and 48 managed selections with complete typed provenance, actual BCL operations in algorithm caller IL, no required-symbol native fallback, exact license/notice delivery, managed dependency/import metadata, actual rooted AOT machine code and ordinary consumer/harness ELF dependencies |

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

The portable profile maps 40 primitive contracts to closed compiler intrinsics.
Only `MEM_readLE24` and `MEM_writeLE24` use the authored adapter, each touching
exactly three bytes. The audit decodes actual algorithm method IL in raw and
processed assemblies, including generic BCL calls and endian field reads. It
scans every translated method for direct calls to retained intrinsic wrappers,
while allowing wrapper
addresses for function-pointer identity. Wrapper bodies never satisfy caller
evidence. Private audit-tool fixtures prove wrapper-only rejection, forbidden
calls from algorithm and other helper methods, and the allowed address case;
these fixtures are not product or runtime qualification.

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

After a complete successful optimized verification run, copied-source delivery
can be checked with fresh outputs:

```sh
DOTCC_CAMPAIGN_COMMAND_LOCKS=production python3 zstd/tests/delivery.py \
  --receipt zstd/artifacts/campaign/RUN/receipt.json \
  --output zstd/artifacts/campaign/RUN/copied-delivery
```

This copies only the generated products, owning API, sample, original authored
primitive source and root MSBuild metadata. It preserves every source byte,
rejects links to the original workspace, evaluates actual compiled source and
project references, builds each form in private intermediates, checks delivered
notices and runs both archive consumer self-tests. It retains its source hash
manifest, exact commands, evaluations, outputs and failure evidence. The copied
source tree contains no upstream reference source or compiler tools.


Recorded `production_inputs` and generated-output digests bind copied files to
that successful receipt. Files absent from historical receipt inputs are marked
explicitly unbound, while still checked for byte preservation and source closure.
Private stale-receipt controls exercise the actual copy path without changing
campaign sources:

```sh
python3 zstd/tests/delivery_control.py --output zstd/artifacts/delivery-control-repeat
```
