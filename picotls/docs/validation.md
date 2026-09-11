# P0/P1 validation record

Executed on 2026-09-11 on the existing `sqlite` branch. P0 is complete; P1
capability, inventory, native/C# mirror, handle-lifetime and contract work is
complete, with **actual translated ABI validation still blocked**. P2 was not
started. All authored changes are confined to `picotls/`; dotcc and SQLite source
are unchanged. Builds/tests ran serially using campaign-specific `TMPDIR` paths.

## Reproduce

Run from `picotls/` in this order; do not run builds/tests concurrently:

```sh
./scripts/fetch.sh
./scripts/oracle.sh
./scripts/probe-boundaries.sh
python3 scripts/probe-compiler.py --build-dotcc
# The previous command currently returns 1 because recorded blockers remain.
python3 scripts/inventory.py
```

The native scripts resolve their own locations; Python scripts resolve paths from
`__file__`. Oracle and boundary scripts fetch/verify inputs automatically. The
compiler and symbol inventories use those established inputs/artifacts. A failed
compiler probe is expected at this stage; inspect `results.json` rather than
ignoring all failures. A future compiler change must rerun these exact upstream
files, as well as appropriate generic regressions.

| Check | Result | Evidence under `artifacts/` |
| --- | --- | --- |
| Frozen source + picotest | Original hashes verified; upstream HEAD remains the recorded parent; rerun preserves/compares original contents | `provenance/`, manifest in `config/inputs.json` |
| Native oracle | Debug/assertions enabled; both selected upstream binaries pass, 24 top-level TAP groups, zero failures | `oracle/tests.log`, `build.log`, `configure.log`, `compile_commands.json` |
| Optional dependency closure | Brotli absent; dtrace/fusion/AEGIS/MbedTLS/fuzzer disabled; OpenSSL host version recorded | `oracle/compile_commands.json`, `environment.txt`, `native-dependencies.txt` |
| Fresh dotcc Release build | Pass, no warnings/errors | `compiler/build.log` |
| Separate real-core preprocessing | All three commands return 0 but retain invalid `##`; core also diagnoses missing pthread header; harness flags blocked | `compiler/*-preprocess.i`, `*-preprocess.stderr`, `results.json` |
| Separate real-core object emission | All three fail with exit 2 at public-header callback macro | `compiler/*-object.stderr`, `results.json` |
| Reduced chained paste | Native compile/run pass; dotcc object emission fails as expected | `compiler/chained-paste-*` |
| Reduced pthread include | Native compile/run pass; dotcc preprocessing/object emission print missing-include diagnostics despite exit 0 | `compiler/pthread-include-*` |
| Reduced aligned allocation | Native alignment/free test passes; dotcc object emission succeeds but generated C# build fails `CS0103` for `posix_memalign` | `compiler/posix-memalign-*` |
| Core native symbol inventory | 85 definitions, 23 external dependencies after internal resolution; no backend crypto imports | `compiler/inventory.json`, `native-defined.txt`, `native-undefined.txt` |
| Native/C# mirror ABI | 92 size/alignment/offset comparisons pass | `p1/boundaries/native-layout.txt`, `managed.log` |
| BCL capability/ownership | All probes pass: hashes/GC handles, appended AEAD ownership, AES-GCM, raw P-256, DER/PSS, X.509/name/leaf-signature positive and negative checks | `p1/boundaries/managed.log` |

Final bounded-script replays of the native oracle and boundary tests also pass.
`compiler/results.json` includes the repository commit used for its final probe,
SHA-256 values of the CLI/frontend/libc assemblies present beside the CLI, exact
commands and return codes. Logs/builds/generated files remain ignored; authored
reproducers and scripts are committed. Shell syntax and Git whitespace checks
pass for campaign changes.

The tested environment is SDK 10.0.111 / runtime 10.0.11, GCC 13.3.0, CMake 3.28.3,
and Ubuntu-packaged OpenSSL 3.0.13 on Zorin OS 18.1 Linux x64. Archive pins are
immutable; these system toolchain/backend versions are recorded environmental
dependencies, not newly downloaded immutable packages. See
[source.md](source.md) and [configuration.md](configuration.md).

## Limits and next boundary

No translated core parses, links or runs yet. Hand-authored ABI mirrors are
standalone feasibility evidence; actual emitted types/layout metadata, complete
`ptls_context_t`, handshake properties and bitfield behavior require P2. P1's
ABI milestone remains open for that reason. [blockers.md](blockers.md) contains
the three reduced blockers and chosen source-linking strategy.

The native oracle covers in-process upstream C tests, including OpenSSL and
bundled minicrypto cross-backend scenarios. Three upstream minicrypto capability
skips are documented in configuration; Perl socket tests and public ECH tests
were not selected. Native backend algorithms beyond the initial BCL profile do
not imply managed support.

The BCL tests are not an integrated provider or TLS interoperability test. They
do not establish NativeAOT, Windows/macOS/arm64, revocation policy, TLS-level
authentication, ticket protection, concurrent connection safety, or cross-allocator
ownership. Those remain P3–P5. No broad compiler or SQLite regression suite was
run because no shared implementation changed; the repository compiler itself was
rebuilt before probing. Continue with P2 only when the user requests it.
