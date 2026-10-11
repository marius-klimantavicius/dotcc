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

## Active review record (UTC, 2026-10-11)

| Time | Evidence inspected | Decision or correction |
| --- | --- | --- |
| 16:27 | Current initialization schema and stock `lower_init.c` master destinations | Approved shared-destination FK and placement facts; removed ordinary-extension approval bottleneck; fixed full inventory and original language modes. |
| 16:30 | Managed `Unit`, `LinkedProgram` and function-body loading | Catalogs remain fully resident. Measure real-unit and bounded-batch memory before increasing the link batch; do not divert the current exporter work. |
| 16:31 | Shared-result native patch and managed destination emitter | One FK and bound destination are sufficient. Structured branches construct into selected storage; binding restoration and cleanup ownership reviewed. |
| 16:33 | Compiler entry point, invocation environment and legacy capture gate | New relational runs clear legacy mode variables and bypass the old raw-graph emitter. |
| 16:36 | Shared-results-v1 generated source and retained test/build evidence | Direct caller-result construction and local cleanup match the intended projection; self-address and failure tests pass native/C#/NativeAOT. Real hugeint probe advances to placement construction. |
| 16:42 | Placement emitter, stock catch-region construction and actual allocator source | Require exact cleanup ownership and protected-action references. Check actual source wrappers rather than testing only a convenient synthetic expression. |
| 16:48 | Actual hugeint placement-copy failure and exception hash-table jump | Fold the existing selected record-copy action into the placement milestone; retain source jumps instead of rewriting all control flow. |
| 16:56 | Placement-v1 generated allocator/initialization/catch sequence | Saved arguments, guard and selected protected action match the intended representation; native/C#/NativeAOT and related regressions pass in `4e27fb690`. |
| 16:58 | EDG label lifetime definitions and exporter `attach_lifetime` | A jump's common lifetime is a reference, not a new cleanup-region owner. Store it separately; do not overwrite lexical lifetime ownership. |
| 17:07 | Vacuous-destruction lowering and managed discarded locations | Preserve operand evaluation without loading/copying an unused record; optional destructor-name operand is navigation. Corrected the dot-field receiver lookup to ordered operand zero. |
| 17:15 | Stock runtime array zeroing and nonvirtual record delete | Project the selected byte-zero action and saved extent rather than deriving scalar zero representations. Record exact complete-destructor entry, pointer reuse and null guard; do not invent a destruction `finally` absent from this selected path. |
| 17:22 | Allocation/deallocation emitters and new fixture source | Existing schema suffices. Byte count and delete pointer are saved once; selected guard and member destruction precede the exact sized deallocator. Fixture tests pointer mutation during destruction, null paths and allocation-length failure. |
| 17:23 | Real exception-v12 and hugeint-v46 probe reports | Exception advances to the lambda blocker shared with checksum. Correct the observed array-count correspondence, then prioritize closure facts selected by EDG. Complete real-object count remains zero. |
| 17:30 | Allocation-actions-v3 generated source and verification | Saved byte extent and source pointer, selected null guards, complete destruction and sized deallocator reviewed. Native/C#/NativeAOT and related regressions pass; milestone `8e08ed5a0`. |
| 17:33 | Lambda patch and stock `lower_lambda` | Reuse the front end's closure type and aggregate initialization. No capture-analysis machinery, schema extension or new lowering callback needed. Generic call-operator prototypes must remain distinct from selected executable instantiations. |
| 17:36 | Local session database and latest agent turn records | Verified both implementers use `gpt-6.1-sol` with `xhigh`, and supervisor uses `gpt-6.1-sol` with `high`. Director runtime identifies `gpt-6-astra` with `high`; old agent path names do not change configuration. |
| 17:44 | Real exception-v13/checksum-v7 and file-scope lowering | Both pass lambda expressions and reach final startup-list capture. Prioritize this shared boundary over virtual delete. Dynamic startup order/guards are distinct from static data initialization; use EDG's selection. |
| 17:51 | Lambda-v3 evidence, checksum-v8 diagnostic and stock lambda entry generation | Captured values/references reuse selected aggregate actions. Startup diagnostics must exclude stock-ignored prototypes before claiming executable work. `_FUN` receives its body in `define_default_version_of_routine`; capture the selected generated call instead of synthesizing forwarding in C#. |
| 18:00 | Routine-address projection and checksum-v9 declaration failure | Reduced one selected routine-address constant from three synthetic nodes to one typed node. Preserve dynamic exception specifications as ordered type metadata, independently of executable policy support. |
| 18:10 | Lambda-v6 generated wrapper, dynamic-spec proof and member-pointer patch | Export exact scope-owned wrapper storage without inferring declarations from reads. Dynamic-spec declarations pass native/C#/NativeAOT with defined unsupported bodies rejected explicitly. Member-pointer owner/member references reuse existing type columns; retain source owner alias and reject unsupported nonzero modifiers. Complete real objects remain zero. |
| 18:25 | Array-bound patch, stock local-expression lookup and checksum-v12/exception-v15 reports | Two nullable type columns retain selected source bounds without re-evaluation. Stock lookup uses the recorded enclosing routine; missing required local correspondence remains an explicit exporter boundary. Both real probes advance to the same source `sizeof...` expression. Keep that projection small and move the managed worker onto required external declarations for source emission. Complete real objects remain zero. |
| 18:33 | Staged array/query metadata, array-bound-metadata-v2 SQL summary and import emitter patch | Fixed/zero/incomplete counts and selected dependent bounds remain distinct in schema 1023 (44 tables). Native/C#/NativeAOT proof passes; checksum-v13 advances to a source type-trait builtin. Batch direct source builtin projections without evaluating traits. Required-provider mode preserves signatures and link identities; verify function pointers and stable external-storage addresses, and keep unused source declarations out of imports. |
| 18:37 | Required-providers-v1 generated declarations and native/C#/NativeAOT reports | Function/global/RTTI address tests pass, but a generated global property consumes `get_imported_value`, renaming the C function to `get_imported_value__2`. Require original callable names to take priority over generated storage members, and add an exact-name fixture assertion before committing this milestone. |
| 18:47 | Required-providers-v2 source and real bundled ICU utf_impl-v1 object, timing and node 139 | Exact callable name is now preserved; provider/builtin milestone committed as `c68043962`. First complete configured-scope bundled object: 503,808 bytes, 14 functions, 1,652 nodes; full export 0.09 s / 17,920 KiB peak RSS. Core objects remain zero. C# stops at a plain-local nested assignment inside a short-circuit condition; prioritize direct named assignment preserving that conditional evaluation over independent bitfield breadth. |
| 18:52 | ICU generated-v2 complete source and required-import manifest | All 14 recorded definitions emit, with local assignments still inside short-circuit conditions and seven required provider declarations. Generation alone passes; compilation/execution is not claimed. Real source also exposes avoidable discarded-local compound-assignment helpers (`c &= 7`); direct the managed worker to retain ordinary named compound statements where exact selected types/order allow them. Native query milestone is `34d97152a`; dependent enum values next use an expression FK rather than forced integer text. |
| 19:03 | ICU generated-v3, source-variable patch and enum reference paths | `2b6db2560` emits ordinary local bitwise statements and removes unused helpers; real ICU source retains all 14 definitions. Nonreal variables use source-only declaration/initializer nodes and are excluded from executable symbol resolution. Caught the constant-path early return bypassing dependent enum identity attachment; native confirmed and added exact-ID verification before final acceptance build. No heuristic name/value matching. |
