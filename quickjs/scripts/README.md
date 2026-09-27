# Campaign integration

`campaign.py` declares the embedded source profile, guarded staging, authored
host links, consumer property and native/managed suites. Shared shell helpers
reuse `Scripts/campaigns` acquisition, tools, builds, postprocessing, receipts
and transactional publication.

```bash
bash Scripts/campaign.sh layout quickjs --check
bash quickjs/scripts/translate.sh --fetch never
bash quickjs/scripts/test.sh --suite native
bash quickjs/scripts/verify.sh --fetch never --rid linux-x64
```

`stage.py` preserves acquired references and checks every adaptation's local
anchor. `test.py` supplies managed case/matrix execution using shared policy.
`audit.py` checks delivered source ownership, host links, notices and dependencies.
No script result establishes more coverage than its actual passing assertions;
see [validation](../docs/validation.md) for the current state.

Use the .NET 10 SDK, a C compiler and Linux NativeAOT prerequisites (the system
linker and development libraries used by the SDK). Native controls additionally
link the system math and POSIX thread libraries. Source fetch policy is separate from NuGet restore:
`--fetch never` uses local source trees/cached archives; `--hashes warn|off|strict`
controls provenance checks. `--restore normal` permits ordinary package restore,
`--restore locked` requests NuGet locked mode and requires compatible lock files,
and `--restore none`
requires already-restored assets for each requested form/mode/RID. The default
qualification matrix is Linux x64, raw/processed, JIT/NativeAOT.
