# Campaign validation record

The P0/P1 baseline is recorded below; the subsequent authorized chained-paste
repair is recorded at the end. Compiler artifact paths contain the latest retry,
so the baseline table preserves the original outcomes rather than describing
the current contents of those logs.

## P0/P1 baseline

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
the reduced blockers and chosen source-linking strategy.

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
rebuilt before probing during P0/P1.

## Authorized P2 repair: chained token pasting

The user subsequently requested the chained `##` fix only. The shared macro
expander now consumes a complete paste chain before rescanning, preserving raw
arguments, empty operands and multi-token boundaries. No upstream or generated
source is patched. Remaining compiler/provider work is deferred.

Executed serially on the same Linux x64 environment with
`TMPDIR=picotls/artifacts/tmp/paste` for repository tests:

- New `MacroTokenPasteTests`: all 11 failed before the fix, then all 11 passed.
  Logs: `artifacts/paste-regression-before.log` and `paste-regression-after.log`.
- Full `DotCC.Tests` suite: 1,882 passed, none failed/skipped
  (`dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release --no-build
  --blame-hang-timeout 300s`); log: `artifacts/paste-unit-suite.log`.
- Functional macro fixtures: eight passed, including the new self-pointer
  callback, empty operand and final-name rescan fixture. Command:
  `dotnet test DotCC.FunctionalTests/DotCC.FunctionalTests.csproj -c Release
  --filter 'FullyQualifiedName~FixtureTests&DisplayName~macro'`;
  log: `artifacts/paste-functional.log`.
- Native C compiled and ran `DotCC.FunctionalTests/Fixtures/macro-chained-paste/main.c`
  with `cc -std=c17`; output matches the committed expected transcript.
- `sqlite/scripts/test-translated.sh core`: translated engine builds and its
  40-case core corpus matches the native baseline, ending with zero handles and
  files. Log: `artifacts/paste-sqlite-core.log`; SQLite script build/emission logs
  remain under `sqlite/artifacts/`. An obsolete generated `Program.cs` initially
  collided with the current `DotCcProgram.cs`; it was preserved as
  `artifacts/paste-sqlite-previous-Program.cs` outside the build directory before
  the successful retry. No authored SQLite file changed.
- `python3 picotls/scripts/probe-compiler.py --build-dotcc`: fresh compiler
  build passes; chained-paste reducer now preprocesses/emits successfully.
  None of the three unchanged core units retains `##`. `hpke.c` and `pembase64.c`
  preprocess cleanly; `picotls.c` still diagnoses missing `pthread.h`.
  All three advance to the callback format attribute at `picotls.h:845`.
  Overall probe exit remains 1 for the open blockers, including `posix_memalign`.

Actual translated picotls ABI/TLS behavior remains unverified. No further P2
repair or P3–P5 work is included.

## P2 GNU format-annotation repair (2026-09-11)

The generic grammar accepts the encountered diagnostic annotation without
changing the callback type. `dotnet build DotCC/DotCC.csproj -c Release` passed
with no warnings/errors; `dotnet test DotCC.Tests/DotCC.Tests.csproj -c Release
--filter FullyQualifiedName~AttributeTests` passed all 12 tests. The
`gnu-format-attribute` functional fixture passes with `callback=42`, exercising
an actual variadic callback and `va_arg` consumption. The allocation agent's
combined 34-test selection independently reran these attribute tests.
`python3 picotls/scripts/probe-compiler.py` now passes the reduced
callback-format case and all three actual files advance to the `__thread`
spelling at header line 1583. Full-core translation remains blocked.
Logs: `artifacts/attribute-build.log`, `attribute-tests.log`,
`attribute-retry.log` and `artifacts/compiler/`.

## P2 GNU thread-storage spelling (2026-09-11)

The full compiler build passes after adding `__thread` as a spelling of the
existing thread-local token. The pthread agent ran all 23 selected pthread and
ThreadLocal unit tests (9 ThreadLocal) and the `gnu-thread-local` functional
fixture successfully. The unchanged core probe now passes `pthread.h` inclusion
and the GNU storage declaration, then stops at `picotls.h:1959`'s extern volatile
function-pointer declaration. Evidence: `artifacts/thread-retry.log` and the
per-unit diagnostics under `artifacts/compiler/`.
