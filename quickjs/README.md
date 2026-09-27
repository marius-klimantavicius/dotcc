# QuickJS translation campaign

Implemented on Linux x64: raw/processed × JIT/NativeAOT.
The Atomics-enabled profile and direct upstream inline exports pass all gates.
See [validation](docs/validation.md) for receipts and scope.

[Plan](docs/PLAN.md) · [Source decisions](docs/source.md) ·
[Blockers](docs/blockers.md) · [Upstream tests](docs/upstream-tests.md)

The product translates Bellard QuickJS into `TranslatedQuickJs`, with an owning
.NET API and a separate JSON/module/managed-callback/Promise consumer.

```bash
bash quickjs/scripts/fetch.sh
bash quickjs/scripts/translate.sh --fetch never
dotnet build quickjs/ManagedConsumer.slnx -c Release
bash quickjs/scripts/test.sh --suite consumer --form all --mode all
bash quickjs/scripts/verify.sh --fetch never --rid linux-x64
```

The source pin lives in `config/source.json`. `ref/`, `generated/`, `build/` and
`artifacts/` are ignored source/build/evidence directories. Authored host code is
linked from `src/Host`; API and consumer use ordinary project references.

[Third-party notices](THIRD-PARTY-NOTICES) preserve the archive license and
individual source notices. Builds and publishes deliver them as
`QuickJs.THIRD-PARTY-NOTICES`, alongside `QuickJs.LICENSE` and `QuickJs.VERSION`.

This unsafe embedding does not provide an isolation boundary for hostile scripts.
