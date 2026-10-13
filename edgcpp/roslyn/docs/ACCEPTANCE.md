# Remaining acceptance work

The initial exporter→reader checkpoint is verified. Phases 3–4 remain in
progress until all M0–M5 gates and field/kind coverage rows are satisfied.
The semantic items below remain required; source locations follow the user's
2026-10-13 priority update below.

The user subsequently excluded C++/CLI support; C++/CLI and C++/CX implementation
and metadata/profile gates are outside the revised task. Native Microsoft C++
extensions remain required. This explicit scope change does not relax other
remaining semantic acceptance work.

The user explicitly made source location nice to have on 2026-10-13. Preserve
directly retained coordinates cheaply and never guess or reconstruct lowered
positions. Finish only small already-near-complete source checks; optional location
completeness and exact coordinate coverage do not block phases 3–4. Executable
semantics, types, lifetimes, semantic identity/order and ordinary API navigation
take priority. Temporary compiler construction bookkeeping is excluded; capture
actual resulting operations, tables and variables rather than recreating it.

## Native adapters and profiles

- Complete every retained common semantic field and active union arm in
  `field-coverage.tsv`, with explicit parser/cache/private/inactive exclusions.
- Complete template headers/parameters/defaults/constraints, retained prototypes
  as metadata, canonical/definition origins and primary/partial argument maps.
  Primary runtime prototype and all-template retention are both true; compile
  defaults alone do not determine this. Never dereference front-end-only symbols.
- Complete class/contextual bases/derivation alternatives/overrides, hidden ABI
  parameter roles, generated tables/thunks/alternate entries and storage partners.
  Preserve multiple overrides serving different base contexts.
- Complete initialization/aggregate/repeat/designator, temporary/binding/lifetime
  and all remaining expression/statement/scalar variants without fabricated calls.
- Complete exact builtin/special-function/attribute/storage/linkage/access/token
  discriminants and sparse used-kind catalog publication.
- Complete coroutine protocols and enabled GNU/MS/UPC/vector/fixed-point and
  other pinned-source extension families, including retained dependent metadata.
- Build and probe all remaining approved compatibility profiles: primary lowered
  native, source-semantic native Microsoft ABI, cfront/assignment-to-this and
  legacy exported templates. C++/CLI/CX and metadata imports are excluded by the
  user's subsequent instruction.
- Resolve exact ABI provenance: distinguish cfront-like from USL cfront 2.1/3.0
  compatibility and record runtime ABI flags. Microsoft pointer-member sizing
  alone must not label a full Microsoft ABI profile.
- Finish native host portability and callable-host resource integration where
  required; initial CLI publication is POSIX. Explicit CXX language configuration
  for the new backend is implemented and verified by independent compilation.

## Exception, identity and optional source evidence

- Query real conditional/unordered/array/partial-array/VLA/VTT/delegating/sized/
  aligned/placement/local-static/exception-object cleanup recipes and transitions.
  Observable array fixture now produces size 4/count 3; a zero-row fixture is not
  coverage. Multiple function epilogues require a collection.
- Keep exact configured null-region sentinel, region zero, physical supplement
  indices and flag meanings. No freed destructible descriptors or private
  original-type/signature fields are allowed.
- Distinguish physical files/includes from presumed `#line` mappings: stock
  source-file entries may represent remaps, and those are not include occurrences.
- Preserve direct source ranges/macros and existing physical/presumed correctness
  where inexpensive. Exact optional locations and intricate provenance recovery
  are not acceptance gates. Semantic declaration/member identity and actual
  execution/source declaration order remain required; no guessed positions.
  Token/trivia/paste lineage is not retained stock IL and stays unavailable.
- Named class/enum/typedef symbols must navigate to descriptors; genuine retained
  declaration occurrences project semantic nodes without invented source records.
  Scope discriminants must remain exact rather than defaulting unhandled kinds to
  block. Exhaustive mappings and corresponding navigation fixtures remain required.
- Complete value sharing across applicable exact families while retaining
  distinct occurrence, addressable storage and source identities. String units
  explicitly distinguish target bytes from target character units.
- Complete exact numeric representation/profile probes beyond the current
  verified GNU fixture. Unknown host/softfloat storage formats must not default
  to an unrelated representation.

## Reader, packaging and measurements

- Integrate new native family output through ordinary property navigation,
  not only generated detail rows. Initializers and statements are executable
  operations; metadata/callee routine definitions/types remain outside execution containment.
- Complete useful typed visitor dispatch and cast helpers for the expanded named
  operations and declarations, with actual dispatch assertions. Generic operation
  callbacks alone do not close the approved typed visitor surface.
- Finish bounded batch loading/query-count measurements on large real inputs;
  maximum page-size companion batches are capped at 900 parameters and prefetch
  is bounded. Verify cache eviction, concurrent disposal and independent TUs.
- Compare page sizes on large-TU throughput before choosing a smaller default;
  current tiny-object 1 KiB packaging materially reduces empty-table overhead.
- Measure frontend/export/reader wall time and RSS, table/index bytes, atom/value
  sharing and cold/warm traversal. Compare deterministic logical output excluding
  run-specific artifact metadata; never pointer-address-derived identities.
- Keep ledger/usage/limits/profile matrix and commits synchronized with actual
  tested coverage. No translator or cross-TU linker is part of these phases.

Verified initial fixes include true initializer execution containment, macro text
atom identities and nesting, plain source-parameter association, source logical
coordinates, EH sentinel/try links, checked SQLite operations and unique temporary
sibling publication. Broader acceptance remains open as listed above.
