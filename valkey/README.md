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

The implementation must resolve Python like `sqlite/scripts/common.sh`, fetch
verified inputs by default, support explicit `--no-fetch` with verified existing
reference sources, and commit each significant milestone locally.
