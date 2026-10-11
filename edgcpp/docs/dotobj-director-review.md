# Director review and continuation

The director stays active during the authorized DuckDB campaign, reviews each
milestone and checks ongoing work every few minutes. A completed fixture or
commit is not the campaign stopping condition. User pause overrides continuation.

The supervisor is GPT-6.1-Sol/high; implementers are GPT-6.1-Sol/xhigh. The
director is GPT-6-Astra/high. The supervisor owns integration, builds and commits.

## Translation acceptance

Use the original portable manifest recorded in
`artifacts/review/dotobj-relational-duckdb-configured-scope-v1/inventory.txt`:
2,635 units, comprising 2,207 library units, 148 tests, 224 musl units and 56
libc++ provider units. Preserve the manifest's per-unit language modes and seven
static extensions; native extension loading remains disabled.

Proceed library first, tests next, with provider coverage separately visible.
One complete unit is a checkpoint, not DuckDB translation completion. The
immediate requested deliverable is actual C# source generated through the new
normalized SQLite semantic objects; compiling/running the entire library is a
later checkpoint. Required unresolved provider declarations are permitted in
that source deliverable, with an explicit imports manifest. Missing executable
trees, fabricated implementations and opaque semantic placeholders are not.

The frozen old raw-graph campaign remains stopped. Scale the new path through
measured bounded batches after complete-unit evidence; preserve artifacts and
report exact source coverage, failures, memory and object size.

## Decisions reviewed on 2026-10-11

- Shared construction destination: add nullable
  `initializations.destination_initialization_id`, referencing the exact
  EDG-selected master initialization, and a shared-result action kind. Existing
  initialization identity reservation handles recursive relationships. Stock
  `lower_init.c` explicitly permits a master without a variable for return and
  other complex destinations. Do not invent expression owners or reconstruct
  shared storage from source syntax. Preserve EDG's cleanup ownership.
- Placement construction: retain ordered argument references, selected
  initialization and failure cleanup, plus the stock lowering's null-guard
  decision through a narrow callback and explicit allocation field. Evaluate
  placement arguments once, use selected deallocation after construction
  failure, and preserve rethrow. Do not guess behavior from allocator names.
- In an explicit source-emission mode, unresolved providers may become required
  public partial methods and typed external-storage address accessors. Preserve
  exact selected signatures and identities; provide no fake bodies, default
  storage, guessed imports or erased calls. Retain strict provider resolution
  as the default mode.
- Ordinary minimal schema/callback extensions do not require a blocking director
  approval. The supervisor records the selected EDG fact and concrete consumer
  need, notifies the director, and continues. Fundamental changes to the compact
  relational semantic contract still require review.

## Review checks

Review actual changes and retained evidence, rather than agent status alone:

- Is EDG the semantic authority, with sufficient expression/type/lifetime facts?
- Is every added relation needed by generation/linking or requested navigation?
- Are source structure and names preserved where C# allows them?
- Is the current work removing an observed project blocker?
- Have complete real objects and generated real source counts advanced?
- Are workers active, decisions answered, and tasks reassigned after completion?

The first review found the director had ended its turn while construction
decisions were queued. Workers continued independent features, but the primary
DuckDB path remained blocked. This is corrected by active periodic reviews and
delegating ordinary in-contract decisions to the supervisor.
