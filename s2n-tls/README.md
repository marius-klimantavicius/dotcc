# s2n-tls translated to C#

This campaign translates the actual pinned s2n-tls C engine with DotCC. The
managed cryptographic boundary uses .NET and pinned BouncyCastle primitives;
the delivered product does not wrap native s2n/libcrypto or another TLS engine.

The default owning configuration enables SSLv3 through TLS1.3 and all36 pinned
negotiable suites, including RC4 and3DES. Applications can narrow its policy.
Certificate validation remains enabled unless an explicit upstream control
disables a specific check.

Raw and processed libraries are published at
`generated/TranslatedS2nTls.Raw/TranslatedS2nTls.csproj` and
`generated/TranslatedS2nTls/TranslatedS2nTls.csproj`. The owning API is
`src/Managed.S2nTls/Managed.S2nTls.csproj`; the root solution includes the product,
provider, owning API and ordinary consumer. See [usage](docs/usage.md).

```bash
bash s2n-tls/scripts/fetch.sh
bash s2n-tls/scripts/translate.sh --tools build --fetch never
python3 s2n-tls/scripts/native-control.py --fetch never --selection all
python3 s2n-tls/scripts/test-prerequisites.py --fetch missing
bash s2n-tls/scripts/test.sh --suite consumer --form all --mode all
bash s2n-tls/scripts/verify.sh --tools reuse --fetch never --form all --mode all
```

Python3.12, .NET10, a Linuxx64 C/CMake toolchain and NativeAOT prerequisites are
required for the complete qualification. Test-only pinned native peers and
isolated Python/OS packages are separate from ordinary managed delivery.
Sources/pins, adaptations, manifests and notice policy live in `config/`.
Reference sources remain separate from staged adaptations and emitted C#.

Current evidence: complete175-unit raw/processed builds and publication pass.
The repaired provider passes49 native differential groups and emitted ABI checks
in JIT and NativeAOT. The earlier0158 provider snapshot passed1,076 application
rows and12 copied/rooted delivery rows; these require fresh qualification against
the RSA repair. All18 native Python comparator families pass22,186 applicable
original cases. Complete translated upstream C/Python qualification and integrated
acceptance remain in progress. [Validation](docs/validation.md),
[plan](docs/PLAN.md) and [blocker ledger](docs/blockers.md) identify exact snapshots
and open gates.
