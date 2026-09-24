# Campaign validation record

## Compiler refresh (2026-09-24)

Fresh translation with dotcc at `1abb6a813ee4a8e8880a103abc8558f3b09dbf85`
passes the complete Linux x64 raw/optimized × JIT/NativeAOT matrix. The native
oracle also passes (two files, 24 tests). Each translated form passes the
copied-source consumer, 92 ABI checks, 2,962 provider checks, eight upstream
utility cases / 232 checks, TLS tests, and 56 independent peer cases (224 peer
executions total). Provider checks include the 124 real-handshake allocation
boundaries and two ticket-clone failure paths. The dependency audit reports
zero violations and zero missing prerequisites.

Commands, run serially from the repository root:

```sh
bash picotls/scripts/oracle.sh
bash picotls/scripts/translate.sh
bash picotls/scripts/test.sh --all --aot --runtime linux-x64
python3 picotls/scripts/audit-product.py
```

Evidence: `artifacts/tests/PASS.json`, `artifacts/tests/run-_d0wplxf/`, and
`artifacts/dependencies/report.json`. The matrix result SHA-256 is
`fef113e6d922a32c1b04360db36ac2e7c03f63d60b65297de5cbf9c56db1739c`;
the translation SHA-256 is
`9f0e2962ed629b1223333bb2882c529dd3465bfc10423613eb8987d3c1a23138`.
The combined rerun's commands and compiler source hashes are retained under
`../msquic/artifacts/reverify-20260924/`. MsQuic transport results are recorded
separately in [its verification report](../../msquic/docs/verification-20260924.md).
SQLite and other execution platforms were not rerun in this campaign.

## Final result (2026-09-11)

**The unchanged pinned picotls core and BCL-only product pass the complete Linux
x64 raw/optimized × JIT/NativeAOT matrix.** All public results match. The product
dependency audit passes with zero violations and zero missing inputs. Windows,
macOS and arm64 execution targets were unavailable; those targets remain
unverified and the corresponding plan gates remain open.

| Variant | Actual ABI | Provider checks | Upstream utility cases/checks | TLS assertions | Independent peer cases |
| --- | ---: | ---: | ---: | ---: | ---: |
| Raw JIT | 92 | 2033 | 8 / 232 | 9967 | 56 |
| Raw NativeAOT | 92 | 2033 | 8 / 232 | 9961 | 56 |
| Optimized JIT | 92 | 2033 | 8 / 232 | 9961 | 56 |
| Optimized NativeAOT | 92 | 2033 | 8 / 232 | 9965 | 56 |

Each provider run includes all 124 measured real-handshake allocation boundaries
and two actual ticket-clone failure paths. The TLS suite includes authenticated
CertificateVerify/Finished corruption, name/trust/expiry/EKU/key-usage failures,
fragmentation/HRR/key update/close, ticket rotation/expiry/resumption, mTLS ticket
reauthentication, bounded malformed input and GC/concurrency checks. Peer cases
cover native picotls and independent BCL SslStream in both roles, totaling 224
executions. Only the TLS assertion counter varies with fresh ECDSA signature and
fragment lengths; the driver preserves raw logs and normalizes that one counter.
Scenario outcomes, negotiated capabilities and payload results still match.

Reproduce from `picotls/`, serially:

```sh
./scripts/fetch.sh
./scripts/oracle.sh
./scripts/translate.sh
./scripts/build-only.sh
dotnet build ManagedConsumer/ManagedConsumer.slnx -c Release
./scripts/test.sh --all --aot --runtime linux-x64
python3 scripts/audit-product.py
```

The final run reused the already verified native oracle and final rebuilt
compiler/postprocessor via `translate.sh --no-build-tools`. Evidence:

- Translation/raw snapshot/postprocessing: `artifacts/translation/final-regeneration.log`
  and `artifacts/translation/success.json`.
- Matrix: `artifacts/tests/PASS.json`, `artifacts/tests/run-lqwhjcso/` and
  `artifacts/translation/final-matrix.log` (exit zero).
- Independent peer directories, in table order: `artifacts/managed-peer/run-2hehen7q/`,
  `run-cxo72dtm/`, `run-xbcjdrwj/`, and `run-n3_zcd6s/`.
- Dependency inventory: `artifacts/dependencies/report.json` and
  `artifacts/translation/final-dependency-audit.log` (zero violations/missing inputs).
- Consumer solution: `artifacts/translation/final-consumer-solution-build.log`;
  all three projects build, zero errors. Current matrix and nested peer publish
  logs have zero IL/trim/AOT warnings; ordinary CS8981 lowercase native-type
  naming warnings remain.
- Shared regressions: 2017 units, 380 functional fixtures (967 opt-in oracle
  skips), 64 postprocessor and 31 analyzer checks (one optional oracle skip).
  The complete SQLite JIT/NativeAOT campaign also passes; details and exact
  preserved-artifact records are below.

The receipt binds input, result and translation hashes. Its results SHA-256 is
`a1e99c9ac9ecca40d0165deabc66f8cb2f76827fc8bbb6b27cc33d1f5ea776a3`.
See [provider contracts](crypto-provider.md), [upstream case coverage](upstream-tests.md),
[dependency audit scope](dependencies.md), and [usage](usage.md) for capability
boundaries. The remaining sections preserve historical checkpoints; their
then-pending statements are superseded by this final result.

## Integrated Linux x64 JIT results (2026-09-11)

The historical baseline sections below preserve their original outcomes. Current
unchanged-core translation, nine-file source-linking and semantic postprocessing
pass (`artifacts/translation/emission-fixes-retry.log`, `success.json`). The real
optimized generated product and BCL provider build successfully.

| Executed check | Result | Evidence under `artifacts/` |
| --- | --- | --- |
| Actual emitted type ABI | All 92 native size/alignment/offset comparisons pass | `translated-abi/optimized/`, product `TranslatedAbi` execution |
| Actual provider vectors | 321 checks pass, including symmetric/AEAD/HKDF, P256, DER/PSS, certificate and direct allocation/lifetime/ticket tests | `translation/provider-vectors.log` |
| Direct upstream utility ports | Eight cases, 232 checks pass | `translation/upstream-vectors.log` |
| Translated ↔ translated TLS | 9669 assertions pass: both suites/identities, HRR, mTLS, fragmentation, authenticated CertificateVerify/Finished corruption, record/authentication failures, tickets, stress and eight concurrent connections | `translation/` TLS execution log |
| Translated ↔ native and SslStream processes | All 56 scenarios pass in both roles, including key update, mTLS, authentication failures, bad key usage and malicious unoffered ALPN | `managed-peer/run-r9nxk7zy/`, `translation/managed-peer-first.json` |

The TLS assertion count includes fragmentation pump iterations and may vary with
fresh ECDSA signature/record lengths. The driver retains raw logs and compares all
public output except that single count. Test scenario identities and peer receipts
remain exact. Raw/optimized equivalence, integrated NativeAOT, real-handshake
allocation-failure sweeps and final broad compiler/SQLite regressions remain
pending at this checkpoint. Windows, macOS and other architectures are unrun.

## Expanded provider and upstream TLS checkpoint

The optimized Linux x64 JIT rerun passes 2033 provider checks. Its bounded
real-handshake fault sweep measures and injects all 124 provider allocation
boundaries, including two actual ticket-issuance hash-clone failures; first
exception identity, unpublished outputs, connection closure and post-GC handle/key
baselines pass. This covers provider wrapper allocation sites, not arbitrary BCL
or core `malloc` failures. Explicit intermediate-chain positive/negative cases
also pass. Evidence: `artifacts/translation/provider-expanded.log`.

The expanded TLS suite passes 9959 assertions, including direct upstream cipher
selection, exact fragment callbacks/overflow, six legacy packets and GREASE
resumption, mTLS ticket fallback with fresh certificate authentication, and UTF-8
exporter-label boundaries. Evidence: `artifacts/translation/tls-expanded.log`.
The actual optimized TLS NativeAOT executable also passes (9963 assertions,
with the expected variable fragment count). Publish emitted no IL/trim/AOT
warnings; three ordinary CS8981 lowercase native-type naming warnings remain.
Evidence: `artifacts/translation/tls-aot-first-build.log`, `tls-aot-first.log` and
`build/tls-aot-first/`. The final raw/optimized full-suite matrix still awaits
fresh translation after the shared ABI-profile investigation below.

## Final generic compiler and preserved-port regression result

After `846bc84` completes the measured GNU bit-field placement repair and
`6607b11` records the independently measured native profile, the complete final
regression driver exits zero. It passes 2017 compiler/runtime units, 380
functional fixtures (967 explicitly opt-in oracle skips), 64 semantic
postprocessor tests and 31 analyzer tests (one optional analyzer oracle skip).
The entire SQLite regression campaign also passes, including all eight native
and seven translated corpora, actual/threaded ABI storage, owning consumer,
threading/abort/GC checks, independent-process VFS/crash/WAL behavior, source and
object function-pointer identity and bidirectional native/managed image exchange.
Applicable JIT/NativeAOT executions pass with no IL/trim/AOT warnings in their
current publish logs. SQLite is regression coverage for the shared compiler and
libc changes; it is not a picotls product dependency.

Exact added-postprocessor orchestration is retained at
`artifacts/translation/sqlite-verify-with-postprocess.sh`; output is
`artifacts/translation/sqlite-final-regression.log`, with individual logs under
`sqlite/artifacts/campaign-*`. Eleven obsolete generated `Program.cs` files are
preserved unchanged with their original/destination paths and SHA-256 hashes in
`artifacts/sqlite-stale-generated/preserved-files.json`. Fresh picotls translation
and postprocessing then run with the final compiler so the forthcoming full
raw/optimized JIT/NativeAOT matrix uses current embedded runtime and provenance.

## P0/P1 baseline

Executed on 2026-09-11 on the existing `sqlite` branch. P0 is complete; P1
capability, inventory, native/C# mirror, handle-lifetime and contract work is
complete, with **actual translated ABI validation still blocked**. P2 was not
started. All authored changes are confined to `picotls/`; dotcc and SQLite source
are unchanged. Builds/tests ran serially using campaign-specific `TMPDIR` paths.

## P0/P1 reproduction (historical)

Run from `picotls/` in this order; do not run builds/tests concurrently:

```sh
./scripts/fetch.sh
./scripts/oracle.sh
./scripts/probe-boundaries.sh
python3 scripts/probe-compiler.py --build-dotcc
# The original P1 run returned 1; current repaired compiler probes pass.
python3 scripts/inventory.py
```

The native scripts resolve their own locations; Python scripts resolve paths from
`__file__`. Oracle and boundary scripts fetch/verify inputs automatically. The
compiler and symbol inventories use those established inputs/artifacts. The
historical P1 compiler failures are retained in the baseline below; current
results are recorded above. Inspect `results.json` for diagnostics. Future compiler
changes must rerun these exact upstream files and appropriate generic regressions.

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

## Historical P1 limits and next boundary

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

## P2 extern volatile callbacks (2026-09-11)

The generic extern declarator/storage repair and volatile pointer-global backing
storage fix pass all 12 `ExternFnPtrTests`/`VolatileTests`. The `extern-fnptr`
multi-unit fixture and native `cc -std=c17` both print `callback=42 table=80`
and `updated=42`, covering declaration sharing, callback arrays and fenced
callback reassignment. Logs are `artifacts/extern-callback-{build,unit,functional,retry}.log`.
The same actual core source advances to `static inline const char *` at header
line 2012; this is the next open grammar boundary, not a core translation pass.

## P2 qualified specifiers and inline enum types (2026-09-11)

Forty-three qualifier/const-check/volatile/thread-local unit tests pass after the
specifier fix; twelve enum/qualifier selected tests pass after adding inline enum
types. Both new functional fixtures match native C: `inline-qualified-return`
prints `hello 42`, and `inline-enum` prints `phase=7 done=8 mode=2 value=42`.
The unchanged public header now emits in hpke/pembase64 objects. Main-core parsing
advanced through the nested enum member and currently stops at the pointer
qualifier in a later declarator (`picotls.c:672`). Logs are
`artifacts/inline-qualified-{build,unit,retry}.log` and
`artifacts/inline-enum-{build,unit,functional,retry}.log`.

## Current libc dependency validation (2026-09-11)

`picotls/scripts/test-libc-dependencies.sh` passes 53 selected unit tests, both
pthread/posix functional fixtures, both native C oracles and current Linux x64
NativeAOT executables under normal and debug/scan heaps. Retained evidence is
`artifacts/libc/PASS.txt` plus each stage's log. No trim/AOT diagnostic is present;
the pthread generated project has one ordinary CS0649 warning for a global field
assigned through a pointer. BCL-only implementation and Linux execution are
established; other platforms have not run.

## P2 const pointer tails (2026-09-11)

Twenty-four selected `ConstPointerTailTests`/multi-declarator/const-check unit
tests pass. The native and translated `const-pointer-tail` fixture both print
`count=5 value=9`, verifying the endpoint loop and independent inner/outer pointer
qualification. Main-core parsing advances to its macro-generated static local
designated initializer at line 988. Logs:
`artifacts/const-tail-{build,unit,functional,retry}.log`.

## P2 static local designated initialization (2026-09-11)

The static designated initializer regression passes in the memory agent's
32-test combined unit selection and four-fixture selection. Its fixture checks
repeated calls to independently named static objects, retained increments and
zero-initialized unspecified fields. The unchanged main core now parses through
line 4430, where a comma-list of braced aggregate initializers is the next blocker.
`hpke.c` and `pembase64.c` object emission now have empty stderr after correcting
readonly libc prototypes. This remains emission evidence, not a compiled product.

## Final shared-regression checkpoint (2026-09-11)

`SQLITE_AOT=1 sqlite/scripts/verify.sh` rebuilt the entire Release solution with
zero warnings/errors, then passed all 2010 compiler/runtime units and all 379
ordinary functional fixtures; 965 optional external-oracle cases were explicitly
skipped by the default test profile. Evidence: `sqlite/artifacts/campaign-repository.log`.
The broad run found and repaired two issues with existing focused coverage:

- The Zig test-block test searched the entire emitted runtime for the substring
  `addition`, which legitimate allocation comments now contain. Its assertion
  checks the quoted test-name literal instead; focused and complete units pass
  (`34f3caa`).
- The new pthread clock helper used an unqualified `DateTime` name, which an
  existing C type in `runtime-datetime-collision` shadows. Both references now
  use `global::System.DateTime`; the complete unit/functional rerun passes
  (`48a9191`).

The subsequent SQLite varargs project initially found a pre-manifest generated
`sqlite/generated/VarargsSpanFixture/Program.cs` from the previous campaign beside
its newly manifest-owned `DotCcLib.cs`. Its compiler-generated contents were
preserved unchanged at
`picotls/artifacts/sqlite-stale-generated/VarargsSpanFixture/Program.cs`; no
source or generated contents were patched. The saved driver
`picotls/artifacts/translation/sqlite-verify-after-repository.sh` resumes the
original verification steps with only the already-passed repository build/tests
omitted. Its source, exact command and logs remain in artifacts. Remaining SQLite
stages and postprocessor suites are still running at this checkpoint.

The resumed SQLite native core/API/VFS/vtable/allocation/upstream/FTS5/layout
oracles pass, as does the 15-case span-varargs corpus under JIT and NativeAOT.
The subsequent metadata gate reports four differences in `WhereInfo` and
`WhereLoop`. Its historical native oracle deliberately uses `-mms-bitfields`;
the picotls repair introduces GNU ordinary-member tail-byte reuse. Existing
SQLite documentation also records earlier default-GNU differences for
`ExprList_item`, `VdbeCursor` and `Parse`. The next step is a complete native-GNU
comparison and a reduced shared-layout repair where required, preserving the
actual picotls ABI. Neither the expected outputs nor the oracle flags have been
changed to hide these differences. Remaining SQLite stages are paused at this
gate while that profile investigation runs.

## 2026-09-12 — Isolated PicoTls sources

The translation recipe now selects `--nest-types --runtime=c --class-name PicoTls`.
Both raw and optimized output contain nine manifest-owned `PicoTls*.cs` files,
with no global using directives or global-usings sidecar. Regeneration removed
the old manifest-owned `Picotls.*` files. Provider and direct consumers import
the nested API types locally.

`./scripts/test.sh --all --aot --runtime linux-x64` passes in
`artifacts/tests/run-_ofyv5ic`. All four variants pass the new copied-source
consumer (implicit usings disabled, colliding consumer names, nested allocation
and callback invocation), 92 actual ABI checks, provider/failure vectors, upstream
ports, TLS tests, and 224 peer executions. The dependency audit also passes with
zero violations and zero missing prerequisites; its report is
`artifacts/dependencies-picotls-rename.json`.

`python3 ../msquic/scripts/test-tls-feasibility.py` also passes the direct QUIC
handshake-message probes for raw/optimized × JIT/NativeAOT. Its receipt is
`../msquic/artifacts/tls-feasibility-run.json`.

This change uses existing compiler nesting support; it changes no shared compiler
or libc implementation. These results qualify Linux x64; they do not establish
additional platform support or requalify concurrent MsQuic changes.
