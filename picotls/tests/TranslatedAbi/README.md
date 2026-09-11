This harness references the actual `TranslatedPicotls` project. It uses its
public `st_ptls_*` types, `sizeof`, and unsafe field-address differences to check
the 92 entries emitted by `tests/native-layout.c`. It does not reproduce the
picotls struct declarations, use reflection, or generate an offset table. The
two provider-extension structs embed actual translated headers.

Run `scripts/translate.sh`, `scripts/build-only.sh`, then
`scripts/probe-translated-abi.sh`. The ABI script also independently translates
the native header probe to obtain dotcc's own `offsetof` metadata and compares
its constants with native results. That metadata probe never changes the
product source closure. `--raw` selects the preserved pre-postprocessing product;
`PICOTLS_AOT=1` additionally publishes and executes the harness with NativeAOT.
`--metadata-only` runs the native/header metadata comparison before a complete
core translation exists; it cannot establish actual product storage correctness.

The initial harness covers the existing 92-entry provider ABI inventory; context
and handshake-property expansion, bitfield semantics, and raw/optimized TLS
behavior still require their own checks. Scripts and an unbuilt harness do not
establish translated ABI success: every stage must execute against an emitted,
buildable core before claiming a pass.

`translate.sh` rebuilds the current compiler and semantic postprocessor by
default (`--no-build-tools` opts out), fetches verified pinned sources, translates
the three selected units, snapshots raw manifest-owned files, and postprocesses
the product in place. Raw snapshots remove only stale files listed by the prior
compiler manifest. `build-only.sh` compiles the owning `src/BclProvider` project
when present and otherwise the emitted core; `--core` or `--raw` explicitly
selects the respective standalone core variant. It does not fetch, retranslate,
run tests, or publish. Translation provenance and
raw/optimized hashes are recorded only after successful postprocessing in
`artifacts/translation/success.json`.

`scripts/test-libc-dependencies.sh` independently retains the prerequisite
pthread/aligned-allocation unit, translated-fixture, native-oracle, and Linux
x64 NativeAOT results under `artifacts/libc/`. These prerequisite passes do not
stand in for the actual picotls ABI or TLS tests.
