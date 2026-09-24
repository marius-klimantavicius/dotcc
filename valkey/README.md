# Valkey translation campaign

The [translation plan](docs/PLAN.md) follows the SQLite, picotls, MsQuic, libsmb2
and Blink campaigns. [Source review](docs/source.md) records the downloaded
Valkey 9.1.2 revision, archive checksum and implementation boundaries.

Status: the initial Linux x64 managed profile passes raw/processed JIT and
NativeAOT execution, protocol tests and native persistence exchange. Broader
command/upstream coverage and unrun platforms remain tracked in the
[validation ledger](docs/validation.md). `valkey/scripts/translate.sh` emits and builds the library;
`valkey/ManagedConsumer.slnx` includes it, the owning API under `valkey/src/`,
the sample and the integration consumer.

The final generated and post-processed C# project belongs at
`valkey/generated/TranslatedValkey/TranslatedValkey.csproj`. The pipeline builds
and post-processes in isolated staging before promoting validated output there;
`generated/TranslatedValkey.Raw/` is a separate comparison artifact. Generated
code and the authored C# host use namespace `Managed.Database`. Linking enables
`--literal-pool` and `--deduplicate-inline` (proven-equivalent static inline methods).

Translation applies reviewed, hash-checked staging edits from
`config/managed-adaptations.json` by default. They connect upstream code to the
C# host and enforce the embedding profile; portable build flags select the
upstream select event backend.
`ref/` stays unchanged. Host implementations live in `src/Host/*.cs`; C calls
use declarations in `src/Host/valkey_host.h` and typed bindings from
`config/dotcc-overrides.json`. Generated projects link the authored C# files.
`--unadapted --probe` is available for original-source diagnostics.
The qualified scope and exclusions are recorded per test; this is not complete
qualification of every upstream Valkey feature.

See [generation and usage](docs/usage.md) for build, sample and validation commands.

The implementation must resolve Python like `sqlite/scripts/common.sh`, fetch
verified inputs by default, support explicit `--no-fetch` with verified existing
reference sources, and commit each significant milestone locally.
