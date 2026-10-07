# SQLite as the semantic package format

Recommendation: adopt a versioned SQLite container with a base node table,
node-kind/variant tables, shared literal payloads and a lazy managed reader.
Keep JSON as an inspection/interchange export.
This is a storage proposal with a measured prototype, not an implemented
replacement for the current native writer or C# reader.

The current dot-backend graph is 2,452,604,523 bytes. Native export succeeds and
all reference IDs close, but the managed reader's `File.ReadAllBytes` fails above
2 GB. Even smaller graphs require one whole `JsonDocument` and an in-memory node
dictionary. A SQLite file can be queried page by page; the essential change is
removing whole-graph materialization from `SemanticGraph`, not merely changing
the file extension. One giant JSON/BLOB row would retain the problem.

SQLite has a documented, versioned file format with page-based storage and file
capacities well beyond this graph size. A completed package should contain one
closed database file, with no required hot journal or WAL sidecar.
[SQLite file format](https://www.sqlite.org/fileformat.html).

## Measured experiment

The prototype streamed the actual current dot-backend graph into SQLite using
Python's SQLite 3.45.1. It retained each node's complete JSON record and added
indexed ID/kind/name columns; macro records, diagnostics and graph metadata have
separate tables. This does not use the newly translated C# SQLite engine.

| Measurement | Result |
| --- | ---: |
| Nodes | 3,115,613 |
| Macro invocations | 14,578 |
| Adapter diagnostics | 271,252 |
| Input JSON | 2.45 GB |
| SQLite database | 3.20 GB |
| Streaming conversion plus index construction | 42.56 s |
| Peak process RSS | 87.04 MiB |
| 10,000 random ID lookups plus JSON decoding | 0.125 s |
| Counts grouped by indexed node kind | 0.110 s |
| Largest individual JSON record | 55,272 characters |

The database passes `quick_check`. A complete ordered readback of metadata,
nodes, macros and diagnostics reproduces the input's logical record hash.
Query timings use a warm filesystem cache. Conversion includes Python JSON
parsing/reserialization and SQLite insertion; it is not a native-writer or C#
emitter benchmark. Source snapshots and a normalized reference table were not
included in the prototype.

The indexed JSON representation is about 30% larger than the existing JSON
file. Its measured benefit is bounded memory and indexed access; size reduction
would need separate normalization or encoding measurements. Do not infer that
the complete compiler pipeline will stay within the prototype's memory bound:
EDG still owns its native IL, and managed passes need deliberate working sets.

Retained evidence and reproducible experiment scripts:
`artifacts/campaign/persistent-continuation/semantic-export-storage-001/`
(`experiment.py`, `results.json`, `verify-records.py`,
`record-verification.json`, and `dot-prototype.sqlite`). A compact tracked
[evidence index](../semantic-export/evidence-storage.json) preserves the results
and script hashes.

## Normalized follow-up

The user suggested a literal pool and base/subtype node tables. A second
prototype applies that design to the same complete dot graph:

- A base `nodes` table holds identity, kind, form and edge-presence information.
- 72 node-kind tables hold scalar facts in typed columns, keyed by the same node
  ID. References and integer/boolean facts are INTEGER columns; absent fields
  stay distinct from explicit values.
- A shared pool holds 685,691 distinct text or structured payloads. Per-node
  source positions and other nested objects remain interned JSON in this first
  prototype; entire nodes do not.
- All 6,801,927 audit edges have dedicated source/ordinal/target/kind columns.
- Metadata, macro invocations and diagnostics retain the first prototype's
  representation, so their normalization is not counted as a benefit.

The result is **794,910,720 bytes (795 MB)**: about **68% smaller than the
original JSON** and **75% smaller than the JSON-per-node database**. Schema
inventory and input hashing took 41.12 seconds; conversion/index construction
then took 66.00 seconds. Peak process RSS was 114.45 MiB. This remains a Python
prototype, not a native writer benchmark. A production writer with a predefined
schema would not need the discovery pass. The database passes `quick_check`.
A complete reconstruction of all 3,115,613 nodes reproduces the input's canonical
node hash, and all metadata/macros/diagnostics match the baseline database.
That readback took 43.73 seconds with 225.74 MiB peak process RSS.

Retained scripts/results are `normalize.py`, `normalized-schema.json`,
`normalized-results.json`, `verify-normalized.py` and
`normalized-verification.json` in the same experiment directory.

This changes the recommendation: typed subtype tables and interning are worth
building into the first production SQLite schema. The per-node JSON experiment
is a useful reference implementation and comparison baseline, not the desired
long-term representation.

## Proposed package structure

Use an application ID and storage schema version in the database header, plus a
separate semantic schema version and explicit capabilities. Keep the existing
`completeForEmission` and adapter diagnostics contracts.

| Table | Purpose |
| --- | --- |
| `translation_units` | Unit identity, language/ABI, root IDs, invocation and producer identity |
| `nodes` | `(unit_id, node_id)` identity, node kind and genuinely common facts |
| Node-kind/variant tables | Typed expression, statement, type, variable, routine and constant facts, keyed by base-node identity |
| `literals` / `atoms` | Interned text, raw bytes and exact numeric payloads; no semantic object identity |
| `source_positions` | Shared structured location/expansion records |
| `node_refs` | Named reference field, explicit ordinal and target; useful for closure checks and incoming-reference queries |
| `native_edges` | Optional separate audit edges; never a substitute for named semantic facts |
| `source_blobs` | Original bytes keyed by content hash |
| `source_files` | Source/include occurrences, original paths and content-hash references |
| `macro_invocations` | Expansion identity, parent, definition, ranges and argument facts |
| `diagnostics` | Unit/node, diagnostic code and missing-adapter message |
| `producers` / `metadata` | Build provenance, hashes, schema and capability records |

IDs must be unit-qualified from the beginning. Storing multiple units does not
resolve cross-TU symbol identity, templates or ODR; that remains semantic
linking. Preserve explicit source/list order. Database row order must never be
used to infer C/C++ evaluation or declaration order.

Use primary-key/foreign-key relationships for the proposed inheritance: an
expression row extends a base node, and an operation-expression row extends
that expression with its operator and operands. A type row can similarly have
pointer, array or record details. Common flags/types/locations belong in shared
levels; variant fields belong in subtype tables. Preserve typed facts directly
instead of a universal property/value table. Unsupported variants can retain
explicit extension payloads and diagnostics without pretending to have a full
typed adapter.

Payload sharing must not merge semantic identity. Two constants or string
objects may reference identical pooled bytes and still require separate node
IDs, source locations, types, and emitted storage. Likewise, equal identifier
text does not make two declarations the same symbol. Keep exact target type and
representation in constant records; a literal-pool ID alone is insufficient.
If a fact is stored twice, define its authoritative representation and validate
the other as a derived index.

Source files belong in BLOBs, preserving exact bytes and deduplicating repeated
header snapshots. SQLite INTEGER is signed 64-bit, so arbitrary unsigned C
constants, 128-bit integers and target floating values need explicit byte/text
encodings; they must not pass through SQLite REAL. Also keep per-record sizes
bounded: database file capacity and individual string/BLOB limits are different.
[SQLite limits](https://www.sqlite.org/limits.html).

## Reader and writer changes

Introduce a storage interface supporting metadata, lookup by unit/ID, indexed
kind enumeration and reference traversal. Keep the existing semantic validation
and emitter interfaces above it. Replace whole-file parsing and the global node
dictionary with bounded caches and routine-sized working sets. Batch retrieval
of each routine's statements/expressions and reuse shared types/globals; one SQL
query per field accessor would add needless work. Decoded node/record lifetimes must
remain valid when cache entries are evicted; cached object identity is not the
semantic node identity.

The initial native writer can embed the pinned native SQLite amalgamation and
insert records with prepared statements in one transaction. Validate reference
closure and provenance, commit and close the temporary database, then publish
it by rename. Hash the final file for package integrity, and retain a canonical
logical-content hash separately: physical database bytes are not a semantic
identity contract across SQLite versions or writer layouts. The final file hash
belongs in external build/package evidence, not inside the file it hashes; an
embedded logical digest must exclude its own metadata field.

Use an established native SQLite binding for the first managed reader if needed.
The newly translated C# engine is an eventual storage provider, but its current
verified VFS stores test files in memory. Reading large real package files needs
a suitable read-only file VFS and independent validation. Making the storage
migration depend on that work would couple two distinct implementation risks.

## Suggested next implementation

1. Add an optional SQLite package writer while retaining JSON output.
2. Implement the managed store abstraction and SQLite-backed, bounded reader.
3. Require identical generated SQLite C# from both package formats; preserve all
   source/provenance hashes, diagnostics and graph closure checks.
4. Validate the actual >2 GB dot-backend package through the managed reader,
   measuring peak memory and emission/query costs.
5. Make SQLite the default package after those gates pass. Extend typed C++
   adapters independently; changing storage does not fill semantic gaps.
