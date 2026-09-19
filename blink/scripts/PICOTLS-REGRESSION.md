# Scoped picotls regression with frozen compiler tools

This wrapper prepares the pinned native oracle and fresh translated products,
then qualifies a selected normal/upstream-native subset:

```sh
python3 blink/scripts/test-picotls-regression.py --cache "$PWD/picotls/ref"
```

`--prepare-only` stops after native upstream tests and regeneration. Resume that
exact unfinished preparation using `--resume <attempt-directory>` with the same
runner, compiler, postprocessor and tracked picotls inputs. Once any matrix
command starts, `--resume` refuses that attempt so failed matrix logs cannot be
overwritten; retry with a new attempt instead. Both preparation and full-run
success require the final identity check. Existing checksum
archive checks and `translate.sh --no-build-tools --no-fetch` are retained.
The wrapper never rebuilds generic tools. SIGTERM, timeout and keyboard interruption terminate and drain
the currently owned command process group. No injected timeout tests are added. It overrides `DOTCC_COMPILER` to the
current repository Release binary and gives orchestration a dedicated temporary
directory. Run serially: campaign projects/native outputs and the peer runner's
own temporary directory are shared build products.

The selected matrix is:

- `CopiedConsumer`: actual emitted sources compiled into an independent consumer,
  raw/optimized JIT and NativeAOT.
- `TranslatedAbi`: 92 fresh native size/alignment/member-address observations,
  compared in each of those four modes.
- 55 normal/authentication peer cases per mode, 220 case results total, against
  native picotls, independent SslStream, and the managed endpoint pair.

Ordinary wrong-name, expired/untrusted certificates, missing client credentials,
and incompatible authentication settings remain valid error-handling tests.
Exactly one peer case is excluded: the authored native server deliberately
selecting an ALPN the client never offered. The wrapper checks the Python AST
contains exactly the reviewed top-level call with the exact arguments, removes
only that statement, and verifies the remaining AST is identical. The original
source is preserved; its derived runner lives directly under `picotls/generated`
at the same root-relative depth as `scripts/test-managed-peer.py`. Original and
derived hashes, removed source text, reason, exact selected case names and
per-mode peer receipts are recorded. Existing peer receipts retain process logs
and public outcomes, but do not capture each peer binary's hash immediately
before and after execution. The final archival file inventory does not supply
that missing per-execution proof. No tracked picotls source or generated C#
is patched.

Three mixed targets are excluded as whole suites: `ProviderVectors` contains
custom allocation injection, `TlsTests` contains altered/crafted records and
handshake corruption, and `UpstreamVectors` adds truncated QUIC prefixes absent
from the pinned upstream case. No runtime switch silently re-enables them.
The native oracle still runs the existing pinned `test-openssl.t` and
`test-minicrypto.t` suites, with upstream revision/archive provenance.

Success requires exact suite/mode and peer-name coverage, matching deterministic
outputs across variants, stable binaries during direct execution, unchanged raw
and optimized product sources, stable peer adaptation and native ABI reference,
and unchanged compiler/postprocessor/authored sources. All commands and output
hashes are retained. Existing full-campaign `PASS.json` and dependency-audit
results are not copied or promoted into this scoped result.

The scoped wrapper passed the pinned native oracle, fresh translation, all
8 selected suite/mode results and all 220 normal/authentication peer cases at
`blink/artifacts/picotls-regression/attempt-fx7grn1c/receipt.json`, SHA256
`8d2a80a7a1bfb6c57f09427b94f4043b83430b6e1e2e9ef4e90babe4109a360e`.
Exact case names and modes, source/adaptation/raw-product hashes, and final tool
and tracked-source identities all passed. Original peer runner SHA256:
`175a0e455ce07e32f7bca8d9dc1fbc72d5da40f389f3819461b3f912c7b7ed83`;
derived normal runner SHA256:
`263eb003255412d70595b080193a27e8aa9ace26f656c887ae08d5fd9cb1afd1`.
No shared compiler rebuild, tracked picotls edit or excluded test execution
occurred. Historical full-suite results remain historical and are not promoted
into this scoped qualification; the peer binary-hash limitation above remains.
