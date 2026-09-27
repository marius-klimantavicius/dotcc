# Authored implementation

- `Host/`: tracked allocator and stack services linked from their original C#
  paths. The facade calls selected upstream header inlines directly through
  `--export-inline`; their names are listed in `config/profile.json`.
- `Managed.QuickJs/`: owning runtime/context/value API with ordinary references
  selected through `QuickJsProject`.

See [API](../docs/api.md), [plan](../docs/PLAN.md) and current
[validation](../docs/validation.md). Keep authored sources out of `generated/`.
