# Valkey translation campaign

The [translation plan](docs/PLAN.md) follows the SQLite, picotls, MsQuic, libsmb2
and Blink campaigns. [Source review](docs/source.md) records the downloaded
Valkey 9.1.2 revision, archive checksum and implementation boundaries.

Status: implementation authorized and in progress with a coordinator and
sub-agents. Completed gates and limitations are tracked in the plan and campaign
validation ledger. The plan specifies the
`valkey/scripts/translate.sh`, `valkey/src/` and root
`valkey/ManagedConsumer.slnx` deliverables; those scripts and projects are
all required to complete the delivery.

The final generated and post-processed C# project belongs at
`valkey/generated/TranslatedValkey/TranslatedValkey.csproj`. The pipeline builds
and post-processes in isolated staging before promoting validated output there;
`generated/TranslatedValkey.Raw/` is a separate comparison artifact. Generated
code and the authored C# host use namespace `Managed.Database`. Linking enables
`--literal-pool` and `--deduplicate-inline` (proven-equivalent static inline methods).

Translation applies reviewed, hash-checked staging edits from
`config/managed-adaptations.json` by default. They connect upstream code to the
C# host, choose the select event backend, and enforce the embedding profile.
`ref/` stays unchanged. Host implementations live in `src/Host/*.cs`; C calls
use declarations in `src/Host/valkey_host.h` and typed bindings from
`config/dotcc-overrides.json`. Generated projects link the authored C# files.
`--unadapted --probe` is available for original-source diagnostics.
The embedded server is not yet qualified.

The implementation must resolve Python like `sqlite/scripts/common.sh`, fetch
verified inputs by default, support explicit `--no-fetch` with verified existing
reference sources, and commit each significant milestone locally.
