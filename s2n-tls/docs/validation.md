# Current validation

P0 passed. P1/P2 are in progress. All raw/processed × JIT/NativeAOT Linux x64
translated TLS behavioral rows remain unrun. Native exchanges and independent
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
