# picotls with dotcc

[Translation plan](docs/PLAN.md) · [Pinned upstream source](docs/source.md) ·
[Validation](docs/validation.md) · [Compiler blockers](docs/blockers.md)

The unchanged pinned TLS 1.3 core runs with a BCL-only crypto provider and an
owning C# API. Linux x64 passes the complete raw/optimized × JIT/NativeAOT matrix,
including actual ABI/provider/TLS checks and 224 independent native
picotls/SslStream peer executions. The product dependency audit passes.
Windows, macOS and additional architectures remain unverified.

See [build and API usage](docs/usage.md), the [provider contract](docs/crypto-provider.md),
and [case-level upstream coverage](docs/upstream-tests.md). The selected profile
supports P256, both AES-GCM TLS suites, ECDSA/RSA-PSS authentication, explicit
certificate trust/name checks, ALPN/SNI, key updates and protected session tickets.
Early data and follow-up algorithms remain disabled.

Run these commands serially from this directory (scripts also resolve paths when
invoked elsewhere):

```sh
./scripts/fetch.sh
./scripts/oracle.sh
./scripts/translate.sh
./scripts/build-only.sh
./scripts/test.sh --all --aot --runtime linux-x64
```

The full test driver checks both generated variants, NativeAOT and independent
peer interoperability before writing a success receipt. All recipes use isolated
campaign temporary directories for builds/tests. Downloads, generated
files, native builds and logs remain ignored under `ref/`, `generated/`, `build/`
and `artifacts/`. Local commits stay on the `sqlite` branch; nothing is pushed.
