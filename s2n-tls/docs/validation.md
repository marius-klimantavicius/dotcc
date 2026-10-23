# Current validation

P0 and P2 passed. P1 and P3–P6 remain open. The first raw JIT TLS consumer passed; the complete raw/processed × JIT/NativeAOT Linux x64
behavioral matrix remains in progress. Native exchanges and independent
provider tests do not establish translated product acceptance.

Validated inputs/native baseline:

- `bash Scripts/campaign.sh layout s2n-tls --write` and `--check`: pass.
- Framework acquisition [20261023-200313-154db7b3](../artifacts/campaign/20261023-200313-154db7b3/receipt.json): pass after declared archive selectors exclude out-of-scope Rust symlinks. The original failed attempt is [20261023-200236-aaa53559](../artifacts/campaign/20261023-200236-aaa53559/receipt.json).
- `bash s2n-tls/scripts/fetch.sh --fetch never`: pass with 5 declared sources in [20261023-202754-eb034caa](../artifacts/campaign/20261023-202754-eb034caa/receipt.json).
- `python3 s2n-tls/scripts/source-inventory.py`: 174 C units, 36 suites, 14 SSLv3 entries, 78 applicable suite/version pairs; upstream static inventory 276 required, 286 excluded, 82 support.
- `python3 s2n-tls/scripts/native-control.py --fetch never --selection all`: [238 passed / 0 failed / 0 skipped](../artifacts/native/20261023T171931Z/report.json), using the same authored compatibility adapter as the product. Includes both independent roles and native self-talk. Sixteen selected upstream native executables report 590,537 actual assertions; the remaining required upstream cases are unrun.
- Both-role deterministic DH probes preserve pristine failure (256 bytes versus the backend's one) and adapted success (one byte) in that report. The repair is recorded separately from translation in `config/adaptations.json`.
- Invocation from `/tmp` with absolute native script path, `--fetch never --build-only`: [pass](../artifacts/native/20261023T172109Z/report.json).

Provider-only JIT/NativeAOT vectors and current binding development are recorded
in [the provider receipt](../artifacts/provider/receipt.json) and
[contract](provider-contract.md). These tests do not use the translated TLS core.
The managed provider refuses full capability initialization until the required
binding groups are implemented and tested.

Translation discovery commands use
`bash s2n-tls/scripts/translate.sh --profile compat --fetch never --tools reuse --jobs 4`.
Each selects the full source manifest, preserves references and attempts private
emission/build/publication. Failed attempts published no product:

| Attempt | Observed stopping point |
| --- | --- |
| `20261023-200417-f38218df` | Missing generated OpenSSL configuration header; native preparation now supplies it. |
| `20261023-200712-c4e037c8` | GNU deprecated header annotation rejected; compiler repair covered by focused tests. |
| `20261023-200901-18813bf0` | Thread-local aggregate string initializer rejected; structural compiler repair. |
| `20261023-201206-303fcacb` | Native-array `va_list` wrapper cannot store DotCC's ref-like cursor; guarded scoped-cursor adaptation preserves copies/cleanup. |
| `20261023-201651-379a3b64` | kTLS platform I/O requires unsupported `msghdr`; excluded kTLS now selects upstream unsupported-platform implementation. |
| `20261023-201943-edf15c76` | 156 objects emitted; implicit-outer multidimensional callback declaration rejected. Structural grammar/initializer repair in progress. |

Compiler milestone `f5b36fc6a` adds real GNU cleanup semantics, GNU deprecated
metadata and TLS string-literal aggregate initialization. Sixty-one focused unit
tests and five emitted execution fixtures pass; logs are under
`artifacts/compiler-discovery/`. Native cleanup reduction prints `7 21`, matching
translated execution. Further inline-reachability and callback-array fixes have
their own focused receipts and remain subject to complete-source retries.

Required suites: native, abi, provider, compatibility, consumer, upstream,
lifecycle, interop, rooted-aot, delivery, reproduction, shared-regressions.
Each remains open until current case-level evidence is recorded here.

Integration evidence:

- Full 175-object emission and linking passed in `20261023-203434-5650c832` and `20261023-205255-fc99db78`. The latter raw build has only nine OCSP import names and three braced `va_list` initialization errors; no missing provider aggregate types or earlier host-service names remain. The 181-entry profile and `27c3194dc` address those findings; full retry `20261023-210200-74eee32f` is in progress.
- `7aa3f97de`: static managed callback override tests pass twice (direct/object emission), sixty focused semantic override tests pass, three braced pointer runtime fixtures pass, and two actual Linux memory-residency/affected CPython mapping tests pass. Logs: `artifacts/compiler-discovery/{static-callback-overrides,static-callback-unit,braced-pointer-functional,memory-residency}.log`.
- `27c3194dc`: braced-zero variadic cursor fixture executes copied/divergent cursors and passes. `artifacts/compiler-discovery/va-list-braced-zero.log`.
- `python3 tests/CoreAbi/run.py --compiler ../DotCC/bin/Release/net10.0/dotcc.dll --mode all`: native/emitted upstream blob, stuffer, hash, cipher, policy, config and connection layouts/offsets/bitfields agree in JIT and NativeAOT. [Receipt](../artifacts/core-abi/receipt.json). This is a scoped actual-header probe, not the complete linked-product ABI gate.
- Authored process-exit registry executes reverse-order retained callbacks and registration during draining, output `main;231`, in JIT and NativeAOT. Logs: `artifacts/compiler-discovery/atexit-{jit,aot}.log`.
- Provider primitive runner now passes 42 groups in both JIT/NativeAOT. Actual emitted provider probe passes both modes with 181 exact typed declarations, actual native ASN1/GENERAL_NAME storage, digest cloning, callback identity/retention and zero live-context delta. [Probe receipt](../artifacts/provider/emitted-probe/receipt.json). Full255 capability advertisement is now backed by42 native differential groups in JIT/AOT and the actual emitted ABI/name/init/capability probe.
- Exact-profile native compilation of all175 staged units reconciles176 real external crypto symbols against181 typed declarations, zero missing. [Closure receipt](../artifacts/provider/closure/20261023-205255-fc99db78/receipt.json). This import inventory is separate from operation semantics.
- Original sixteen native-qualified upstream test C files emit unchanged except the required prelude; no assertions executed in managed code yet. [Emission receipt](../artifacts/managed-contract/upstream-native16-emission.json).
- `python3 scripts/test-prerequisites.py --fetch never`: isolated Python3.12 dependencies, private GnuTLS/Apache binaries, original Java client asset and Apache configuration validation pass. [Receipt](../artifacts/test-prerequisites/receipt.json). The independently pinned previous-release native s2n CLI uses static pinned OpenSSL1.1.1w; no native TLS/crypto dynamic import appears in its `ldd` output. These peers are test prerequisites, not product dependencies or managed qualification.

Full product build/publication milestone:

- [20261023-210200-74eee32f](../artifacts/campaign/20261023-210200-74eee32f/receipt.json) passed all175-unit emission/linking, raw build, semantic context, postprocessing, processed build and both final canonical builds/publication. No generated C# edits. This is P2 build/source closure, not TLS execution or P6.

- Actual canonical processed product/native layouts, offsets, bitfield bytes, managed function address and thread-local error identity pass JIT and NativeAOT: `artifacts/core-abi/product-first{,-aot}.json`. Full ABI runner matrix is in progress.
- Actual owning raw JIT consumer passes forced SSLv3 RC4-MD5(0004), SSLv3 3DES(000a), TLS1.3 AES128-GCM(1301), exact versions/IDs,1027 binary bytes both directions, close_notify and cleanup: [first loopbacks](../artifacts/managed/first-ready-loopbacks/report.json). Full compatibility/interop/lifecycle matrix is in progress.
- Focused shared regressions pass204 unit tests and10 functional tests, zero failures/skips: [211208-f1dd6d23](../artifacts/campaign/20261023-211208-f1dd6d23/receipt.json). The earlier211102 attempt included eight Windows-only oracle cases on Linux; the corrected selection explicitly targets the applicable emitted runtime fixtures.
