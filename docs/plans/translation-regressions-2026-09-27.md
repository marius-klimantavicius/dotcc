# Existing translation campaign regression qualification — 2026-09-27

Status: **completed within the user-approved scope**. SQLite, picotls, libsmb2, Blink and Valkey passed their required gates on the final frozen compiler. MsQuic passed all 13 prerecovery suites and completed its full 576-case recovery collection; incomplete transfers remain failed or warned, including one unclassified FIN deadline. The user explicitly accepted documenting the native-reproduced decreasing-MTU limitation rather than repairing upstream MsQuic. Recovery is not claimed to pass.

This report records the requested verification of all six existing campaigns before implementation commits. Regressions found during qualification were repaired and reverified. Campaign defaults are a 262,144-byte split target with literal pooling and inline deduplication enabled; CLI defaults are unchanged. The native MsQuic exception preserves the actual failed/incomplete results and does not waive new DotCC regressions.

Final runs use the shared default hash policy unless their existing driver explicitly selects strict; both policies retain tool identities and behavioral assertions. Strict hashing is optional, not an additional acceptance gate.

Initial runs are diagnostic. A shared IPv6 constant linkage regression was found during the repository functional suite and repaired after those runs began. The final evidence below uses fresh translation and complete required observations on the repaired compiler; earlier receipts remain historical evidence only.

## Required coverage

| Campaign | Profiles and verification scope | Final result |
| --- | --- | --- |
| SQLite | `default`; all 12 declared verify suites, raw/processed consumers in JIT/NativeAOT, native and translated corpus controls | [Passed](../../sqlite/artifacts/campaign/20260927-060803-4d6c82a8/receipt.json) |
| picotls | `default`; native oracle and full qualification, raw/processed JIT/NativeAOT | [Passed](../../picotls/artifacts/campaign/20260927-060907-dca9e368/receipt.json) |
| MsQuic | `default`; full declared verification, native/managed host and ABI controls, raw/processed JIT/NativeAOT | [Complete observations; native recovery limitation retained](../../picotls/artifacts/verification-20260927-atomic-refresh/final-evidence.json) |
| libsmb2 | Required `legacy` delivery plus `async` full qualification, native oracle, upstream programs against disposable Samba, raw/processed JIT/NativeAOT | [Passed](../../libsmb2/artifacts/campaign/20260927-061008-cb8ef98f/receipt.json) |
| Blink | `threaded` and `single-thread` applicable default gates; threaded machine API, managed consumer delivery with freshly produced prerequisites, and guest threads | [All required gates passed](../../blink/artifacts/atomics-shared-regression-20260927/report.json) |
| Valkey | `managed`; full unit, native oracle and consumer verification, raw/processed JIT/NativeAOT including persistence and upstream protocol controls | [Passed after listener-shutdown repair](../../valkey/artifacts/campaign/20260927-062725-8d135d6c/receipt.json) |

## Repairs discovered during qualification

- Narrow C11 atomics: named `atomic_load`/`atomic_store` now route signed and unsigned 8/16-bit integers through the existing same-width interlocked helpers. Eight new emitted-shape cases fail before the repair and pass afterward; 19 atomic unit cases and six actual emitted functional fixtures pass, including native-matched adjacent-byte guards. [Focused evidence](../../quickjs/artifacts/narrow-atomics-regression-20260927/report.json) is retained. The shared tools were rebuilt and frozen before final campaign verification.
- Shared IPv6 constants: `in6addr_any` and `in6addr_loopback` require external linkage and consistent identity across translation units. The shared header incorrectly declared private copies. The repair also recognizes ordinary global struct initialization in repeated header declaration handling. Dedicated linked and direct multi-unit fixtures verify identity; duplicate same-unit definitions remain rejected.
- Valkey host integration: corrected C internal linkage made direct authored C# references to private `io_threads_initialized` and `io_shared_outbox` invalid. Guarded C accessors in the owning source unit expose the required state and queue pointer through stable exported functions. Live worker reconfiguration, traffic and shutdown passed in all four final cells.
- Valkey listener shutdown: a processed-AOT worker poll encountered `EBADF` after owner Stop closed a still-registered listener. Unlike epoll, select retains descriptor-set bits after close. A native differential using the pinned `ae_select.c`, `aeDeleteFileEvent` and exact listener-close body reproduces the failure deterministically. Guarded staging now unregisters ordinary and cluster listener events before close, using the existing event-loop lock; the unchanged traffic and shutdown assertions passed in all four final product cells. [Native proof](../../valkey/artifacts/listener-shutdown-native-control/report.json) is retained.
- Blink qualification integration: product harnesses needed to consume the current shared campaign receipt and current project-property source closure. Native Kestrel guest and profile prerequisites are now produced during the qualification run, with exact receipt digests and producer-chain checks. Hardcoded references to historical ignored attempt paths were removed from the proposed configuration; they are not accepted prerequisites.
- libsmb2 idempotence harness: its private product copy must preserve the generated/project-to-authored-source relative layout. The corrected temporary layout includes the actual authored host files in semantic compilation; the real postprocessor leaves every generated C# hash unchanged.
- libsmb2 upstream connection setup: an unchanged native zero-timeout loop can reject connection readiness merely by crossing a wall-clock second. An actual processed-JIT case failed after 95 ms; the exact loop reproduced the same failure in native and translated controls. The harness now supplies upstream-supported `timeout=10` equally in all variants and preserves URL query options after fixture paths. Original source bodies, assertions and failure controls are unchanged; the repaired five-variant matrix passed all 35 supported records. [Original failure and reduction](../../libsmb2/artifacts/regression-20260927/connection-failure/reduction.json) are retained.
- MsQuic qualification integration: behavior harnesses now resolve the actual framework-owned qualified closure and validate its receipt, completed delivery gates, digest, and generated products instead of comparing against an unrelated historical checkpoint.
- Valkey harnesses: the native protocol summary accepts ANSI-colored successful counts while retaining the original transcript and requiring nonzero coverage; isolated input tests clear inherited campaign policies before testing their own defaults.
- Blink native trace parser: split `exit(0 <unfinished ...>` / resumed traces reconstruct `exit(0 )`. Lexical whitespace handling now accepts that valid zero exit while preserving required clone/futex/thread/exit observations; five positive/negative regression controls and the targeted real guest gate passed.
- Portable test setup: relevant path-escape/audit tests inject symlink metadata rather than requiring privileged host link creation. The libsmb2 temporary semantic project copies authored sources with ordinary filesystem operations.

## Final tool identity

The final verification runs use freshly rebuilt shared tools after the IPv6 and narrow-atomic repairs. SQLite receipt `20260927-060803-4d6c82a8` built and recorded this frozen snapshot before all other lanes reused it:

- `DotCC.Lib.dll`: `5e7f4c566aaa7f292fb104c8800866e5dc10b5bfa8b825630b19695adef111a7`
- `dotcc.dll`: `df4cd81066da70da77ae33dc8156cc76c83ba0e698364197e7f4c99947fefaef`
- `dotcc-postprocess.dll`: `55807db51b41401288a7cadf9175f25e53dcfed5fd97c25e3305b6d4d514ad58`

The [current-product audit](../../quickjs/artifacts/six-campaign-regression-20260927/current-products.json) independently matches every framework-owned generated file to these final receipts, including both libsmb2 profiles and the current threaded Blink product.

Qualification here executes on Linux x64. Portable script fixes remove host symlink privilege requirements; they do not constitute an executed Windows campaign matrix. Platform-specific suites retain their declared platform scope.

The newly implemented QuickJS campaign was also requalified on the same final compiler: [`20260927-062444-292bece5`](../../quickjs/artifacts/campaign/20260927-062444-292bece5/receipt.json) passed the complete seven-suite/four-cell verify selection after removing the forwarding bridge, enabling upstream Atomics, and applying the audited branch-hint macro profile. It uses only the five upstream C units, exports each requested original inline exactly once, contains no `DotCC_JS_*` forwarding wrappers, and roots 3,597 translated methods for AOT. The native oracle records 36 checks, managed behavior records five harness and 16 JavaScript cases, and lifecycle records 23 cases, including actual cross-runtime Atomics wait/notify. [Fresh reproduction](../../quickjs/artifacts/reproduction-branch-hints-20260927/report.json) also passed, including rejection of a side-effecting expected-value argument. The [branch-hint audit](../../quickjs/artifacts/branch-hint-overrides-20260927/report.json) records all 426 calls eliminated across five objects. This is additional preservation evidence alongside the six requested existing campaigns.

## User-accepted MsQuic recovery limitation

The prior pre-Atomics recovery run stopped at case 228 (`raw-aot-client-ipv6-128-payload-ceiling-down`) after 192 successes and 35 existing recognized warnings. Its unexpected 15.28-second FIN deadline had zero transport errors and incomplete peer data. The failed parent remains failed. Twelve fixed diagnostic controls (six managed/native and six native/native with identical seed and proxy configuration) all reproduced incomplete transfer with transport idle error 62/1; none reproduced the original deadline. No compiler defect, changed assertion, or expanded warning classification is claimed. The user chose to document this native-reproduced limitation rather than repair upstream MsQuic. The original failure remains recorded independently of the final matrix; incomplete transfers are not described as successful recovery. [Retained diagnostic evidence](../../picotls/artifacts/verification-20260927-final/recovery-diagnostic/README.md) explains the observation limits.

The [MsQuic verification report](../../msquic/docs/verification-20260927.md) records the native limitation and exact causal limits. The final [composed evidence](../../picotls/artifacts/verification-20260927-atomic-refresh/final-evidence.json) binds canonical receipt [`061505-6ebabcee`](../../msquic/artifacts/campaign/20260927-061505-6ebabcee/receipt.json) to the [complete recovery results](../../picotls/artifacts/verification-20260927-atomic-refresh/recovery-complete/results.json). All 13 prerecovery suites passed, including 80 native/managed peer records, 88 independent peer cases and 152 CID cases. The parent was intentionally cancelled immediately before its own recovery invocation after the independent full collection had started; it remains cancelled and no duplicate recovery exchanges were run.

The final matrix executed **576/576 cases: 488 successful transfers, 87 existing-policy warnings (40 rebinding and 47 transport-idle outcomes), and one unexpected FIN deadline**. All ten positive scenarios passed across all 48 profile combinations. Its exit code is **1**; `matrix_complete=true`, `verification_passed=false`, `passed=false`, and `entire_p7_qualified=false`. The unexpected case is `optimized-aot-client-ipv6-256-payload-ceiling-down`: the managed client reached its 15.27-second FIN deadline, both endpoints closed with transport/peer errors zero, the native server retained 35,609 partial bytes, and managed resources plus proxy cleanup were clean.

An [exact final native/native control](../../picotls/artifacts/verification-20260927-atomic-refresh/native-final-comparison.json), using the same seed, proxy configuration and bound binaries, also failed the required decreasing-ceiling transfer: idle error 62/1 after 10.27 seconds, with 18,098 partial bytes. This proves a native transfer limitation, **not identical timer or packet history**. The classifier still rejects the mixed deadline; it was not converted to a known warning or success. The user-approved scope accepts documenting this limitation with those distinct observations intact.

The qualified closure and peer baseline hashes matched before and after collection; [native binary identity](../../picotls/artifacts/verification-20260927-atomic-refresh/native-control-identity.json) also preserves relevance of the prior fixed controls. [Actual link commands](../../picotls/artifacts/verification-20260927-atomic-refresh/link-flags.json) contain `--split-size=262144`, `--literal-pool` and `--deduplicate-inline`. All 27 [collector/provenance tests](../../picotls/artifacts/verification-20260927-atomic-refresh/collector-provenance-tests.log) passed, and [preservation checks](../../picotls/artifacts/verification-20260927-atomic-refresh/classifier-preservation.json) confirm unchanged classifier bytes and unchanged exchange/case/classification logic.

## Existing libsmb2 upstream limits

The unchanged [upstream qualification contract](../../libsmb2/docs/upstream-tests.md) distinguishes passing required gates from complete upstream coverage. Its final case receipt retains `complete=false`: 35 supported case/variant records passed, 60 were explicitly skipped, one unchanged native baseline failed, and its four managed counterparts were blocked. There are zero unexpected failures. They are not newly observed compiler regressions, and no assertions or source bodies were relaxed to hide them.

| Case(s) | Existing reason |
| --- | --- |
| `ntlmssp_generate_blob.c` | The pinned standalone source omits the server identity; the unchanged native decoder dereferences it and the native process fails. That native failure blocks the four managed variants instead of being counted as a pass. |
| `test_0100_ls_basic.sh` | `prog_ls` requires allocator interposition through `dlsym(RTLD_NEXT)`. |
| `test_0101_ls_basic_valgrind.sh`, `test_0201_mkdir_valgrind.sh`, `test_0211_cp_valgrind.sh`, `test_0301_cat_valgrind.sh` | Native Valgrind instrumentation has no equivalent evidence in this managed runner. |
| `test_0102_ls_basic_socket_error.sh`, `test_0103_ls_basic_valgrind_malloc_error.sh`, `test_0212_cp_valgrind_socket_error.sh`, `test_0302_cat_valgrind_socket_error.sh` | These upstream fault tests require tracing/interposition and equivalent managed hooks. |
| `test_0311_open_timeout.sh` | Requires the upstream scrambla server that deliberately leaves CREATE unanswered. |
| `test_0400_overdrawn_0202.sh` | Requires getopt state for metastat, allocator interposition, and ten-process credit-stress orchestration. |
| `test_900_dcerpc.sh` | Optional libdcerpc profile is not translated. |

The [retained final evidence](../../libsmb2/artifacts/regression-atomics-20260927/report.json) includes required legacy delivery [`061008-52b99441`](../../libsmb2/artifacts/campaign/20260927-061008-52b99441/receipt.json), 53 units per profile, seven native/JIT/AOT host-service controls, and the complete four-cell product matrix. The native oracle and each sample/lifecycle cell passed 11 dialect/signing/encryption/negative scenarios. Both forms passed their two facade-finalizer checks and whole-assembly-rooted AOT audits. The actual idempotence pass updated zero source files. The product audit retains its documented Kerberos.NET trimming/AOT warnings and host-import inventory; those warnings are not silently discarded.

## Existing Valkey protocol limits

The [final Valkey evidence](../../valkey/artifacts/atomics-shared-regression-20260927/report.json) binds the repaired receipt [`062725-8d135d6c`](../../valkey/artifacts/campaign/20260927-062725-8d135d6c/receipt.json), current generated hashes, original failing processed-AOT binary/log and exact native reduction. All four cells passed 21 host checks, 29 selected protocol cases and four RDB/AOF exchanges. The native oracle passed 20 baseline checks, all 35 protocol cases and the listener-shutdown differential; 30 Python tests passed. The original failed campaign remains failed.

The unchanged [managed validator](../../valkey/scripts/validate_managed.py) and [validation contract](../../valkey/docs/validation.md) select 29 upstream protocol tests and exclude six DEBUG PROTOCOL cases because `enable-debug-command` defaults to `no` in the admitted profile: `RESP3 attributes`, `RESP3 attributes readraw`, `RESP3 attributes on RESP2`, `test big number parsing`, `test bool parsing`, and `test verbatim str parsing`. The separate native oracle runs all 35. These exclusions were not introduced or expanded by this regression work.

## Shared repository checks

The [final Atomics-snapshot checks](../../quickjs/artifacts/repository-regressions-atomics-20260927/report.json) passed 2,828 unit tests and 731 functional tests with 1,179 existing optional/platform/oracle skips. The [retained repository checks](../../quickjs/artifacts/repository-regressions-20260927/report.json) include 103 passing postprocessor tests and 36 passing framework tests. The actual postprocessor CLI smoke and all eight project layout checks also passed. Earlier IPv6 and collectible-assembly test failures remain preserved with their repairs; the final functional run has no failures. Test isolation repairs do not change the frozen compiler/runtime product.

## Blink accepted receipt chain

The [final combined evidence](../../blink/artifacts/atomics-shared-regression-20260927/report.json) records passing single-thread applicable gates [`060845-bad26867`](../../blink/artifacts/campaign/20260927-060845-bad26867/receipt.json), threaded default verification [`061149-1bc91d34`](../../blink/artifacts/campaign/20260927-061149-1bc91d34/receipt.json), and all three specialist product gates [`061901-4b8dd1d5`](../../blink/artifacts/campaign/20260927-061901-4b8dd1d5/receipt.json). Both profiles emitted all 108 units. The threaded consumer covers raw/processed × JIT/NativeAOT; the final specialist run verifies the same generated product hashes. MachineApi adds 40 actual guest cases and 11 native controls. Actual Kestrel delivery adds four worker executions and six HTTP comparisons across JIT/AOT. Guest-thread witnesses execute native Linux and pinned native Blink, not managed threads.

The Kestrel guest and native profile were freshly produced during this final run, with receipt digests `5695447bcb3814ad0bcecb5a47067401ccff45782a4327d39a1f95d50030f1d9` and `b38af25980367d5c9f70c1cc1bd6286ea67d8b6ba29b297da1bfd1b88ec893bd`. The combined report links the exact producer chain, pinned SDK image, current authored sources, actual commands and child reports. Historical attempt paths are evidence references only, never prerequisite configuration.

The earlier pre-Atomics parent [`053102-eb1750ff`](../../blink/artifacts/campaign/20260927-053102-eb1750ff/receipt.json) remains failed because of the split-exit trace parsing defect. Its original failure and targeted repaired trace proof remain in the [earlier report](../../blink/artifacts/regression-20260927/report.json); the final full specialist run above independently passes that gate on the final compiler.

The six explicitly opt-in specialist suites (`sqlite-regression`, `picotls-regression`, `msquic-regression`, `language-regressions`, `wat-regression`, `zig-regression`) are outside Blink's declared required selection. The first three campaigns are independently verified here. Single-thread consumer/product gates are not declared supported and were not represented as executed.

## Reproduction

With the pinned source/prerequisite caches available, each canonical verification uses `--form all --mode all --rid linux-x64 --fetch never --tools reuse --jobs 2`. Build the shared tools first (`--tools build` on the initial invocation), then keep them unchanged across projects. The final receipts retain exact executed commands and tool hashes.

```text
python3 Scripts/campaign.py verify sqlite --profile default <common options>
python3 Scripts/campaign.py verify picotls --profile default <common options>
python3 Scripts/campaign.py verify msquic --profile default <common options>
python3 Scripts/campaign.py verify libsmb2 --profile async <common options>
python3 Scripts/campaign.py verify valkey --profile managed <common options>
python3 Scripts/campaign.py verify blink --profile single-thread --suite source-inputs --suite host-files --suite host-sockets --suite instance-io <common options>
python3 Scripts/campaign.py verify blink --profile threaded <common options>
python3 Scripts/campaign.py test blink --profile threaded --suite machine-api --suite consumer-delivery --suite guest-threads <common options>
```

`<common options>` means the explicit options above, not a literal command argument. The actual picotls/MsQuic run used one framework session so MsQuic reused the just-qualified picotls dependency. Run Blink single-thread qualification before its final threaded verification, then run the three product gates against that unchanged threaded product. The product gate builds current Kestrel prerequisites itself. Commands and exact prerequisite producer receipts are also retained in the per-project evidence reports.

The final MsQuic workspace intentionally retains a cancelled canonical parent at the recovery handoff. Automatic product discovery rejects that receipt; it must not be presented as a completed ordinary verify. The diagnostic collection used the explicit selector `DOTCC_MSQUIC_QUALIFIED_RECEIPT=msquic/artifacts/campaign/20260927-061505-6ebabcee/receipt.json`, after confirming completed translation, boundary/freeze gates and unchanged products. The [exact executed command](../../picotls/artifacts/verification-20260927-atomic-refresh/recovery-command.json) binds the qualified closure and retains the running-parent snapshot and final cancelled state. A normal fresh `verify msquic` keeps its ordinary fail-fast behavior.

To repeat that complete diagnostic selection from the repository root using the retained current product, choose a fresh output directory and set the explicit selector, for example with Python:

```python
import os
import subprocess
import sys

os.environ["DOTCC_MSQUIC_QUALIFIED_RECEIPT"] = (
    "msquic/artifacts/campaign/20260927-061505-6ebabcee/receipt.json"
)
subprocess.run([
    sys.executable, "msquic/scripts/test-recovery.py",
    "--variants", "raw", "optimized", "--runtimes", "jit", "aot",
    "--roles", "both", "client", "server", "--families", "ipv4", "ipv6",
    "--ciphers", "128", "256", "--seed", "7381", "--keep-going",
    "--output", "msquic/artifacts/recovery-reproduction-20260927",
], check=True)
```

`--keep-going` records every completed failed attempt and still returns nonzero for any unexpected failure. It neither converts failures to passes nor changes the existing warning classifier. Infrastructure failures without a complete retained attempt still stop the run.
