# Actual generated-data proof

The original three-argument auditor still accepts the historical byte manifest.
Schema 2 adds fixed-width byte/ushort/uint/ulong backings, typed views, exact
RVA field extents and a transitive copy/helper audit. It rejects unexpected
getter conversions, temporary arrays, equivalent array construction and
unreviewed external calls. Required writable C allocation boundaries and
concrete pinned literal pools appear separately in the report.

Prepare a schema-2 manifest with `cpython/scripts/generated_code_evidence.py
--typed`. Supply the actual source directory, translation receipt, product
project, owner, literal holder and exact reference PE. `--probe` creates an
authored executable referencing that PE directly, so it cannot rebuild or
replace the product. Preparation is a source inventory; it is not acceptance.

Run the built auditor with `actual.dll inventory.json report.json`. Build the
prepared probe in Release for JIT and NativeAOT, then run it with an output JSON
path and the native-table transcript. It binds closed typed delegates before
measurement, verifies scalar widths and exact bytes, and measures 10,000 copies
after warming. Destination allocation and required literal-pool startup are
outside the measurement. Reports use `Utf8JsonWriter` and require no reflective
JSON serialization.

`product_tables.c` reads five actual symbols from the same-pin native CPython
archive without initializing an interpreter. `ProductTables.cs.txt` checks
those exact C tables in the selected managed product, distinct writable storage
and addresses across compacting collection. It does not establish native-byte
parity for other tables, scalar classes absent from the product, or complete
interpreter behavior.

Every execution needs its own actual PE/MVID, matching PDB and Compile inputs.
For AOT, bind the response-selected entry and every reference, roots, native
object and executable. A matching project name does not permit retargeting a
proof to a different consumer PE. The A29 command plans and
`artifacts/owner-a-r29-20261018/actual-data-proof.json` provide a historical raw286
example bound to its original `96d7…` PE. Coordinate the shared lane before any
build or execution.

The campaign audit now has a strict schema-2 consumer in
`tests/Audit/typed_data.py`. New reports require `auditRevision: 3`: every backing
and view is enumerated from source and the actual PE; transitive local calls,
owned helpers and implicit helper type initializers bind to independent metadata
tokens and IL hashes. Temporary arrays, reference/array/delegate constructors,
boxing, indirect payload calls and unreviewed external construction fail.
Required C destinations and the concrete byte-pool/L16 pin/root boundaries stay
visible. The existing byte, address, nine lifetime and semantic gates remain.

The tracked address cache's managed `calli` is bounded separately: its actual
type must be byref-like with the sole ref-byte field, and each reached `Get`
call must immediately consume a local static `ldftn` acquire target with the
matching zero-argument ref-T signature. The auditor walks that acquire body and
its local callees. Exact-shaped cache controls whose acquire helper allocates an
array or object reject; an unrelated indirect helper also rejects. A static
data subgate pass still requires the existing complete address/lifetime gate
bound to the same actual PE/source before a campaign audit cell can pass.

Each form's `DOTCC_CPYTHON_GENERATED_EVIDENCE` entry supplies `typed_data` as a
path/hash pair, or `typed_data_by_pe` keyed by actual library SHA256. Its proof has
`schemaVersion: 2`, `product`, `inventory`, `probeInputs`, `nativeOracle`, `jit`
and `aot`. Each execution binds the actual product and probe PE/PDB/project,
Compile inputs and PathMap. AOT additionally binds the actual response, every
selected reference (bytes/MVID), entry, object, roots and executable. A canonical
or source-equivalent PE cannot substitute for the selected consumer library.
Missing evidence remains a required failure; the gate runs no replacement probe.

`tests/Audit/prepare_typed_controls.py <private-output>` prepares small real-IL
positive/negative controls without building or locking. They cover hidden
transitive and owned-helper arrays, equivalent construction, reference/array
constructors, helper static caches, wrong views, changed RVA bits/extents and
omitted backing/view entries. Pure gate/PDB/source controls are available with
`python3 -B -m unittest discover -s cpython/tests/Audit`. Synthetic unit fixtures
are rejection controls, never product qualification.

A29's `96d7…` proof keeps its original bytes and scope. The changed gate needs
fresh actual reports. New193, processed forms, missing scalar/bit/native offset
cases and the complete ownership/lifetime/interpreter matrix remain required.

A30 ran 17 real IL controls (two accepted, 15 rejected), including exact-shaped
tracked-cache acquire helpers that allocate arrays or objects. The final static
report and independent metadata bind the original V20 `7b01…` PE; the normal
V21 publisher retains its PE/PDB and all 98 source documents. The new V21
`d500…` PE has fresh publication/source binding, SAME-C ftruncate JIT/AOT and
strict schema-2 warmed typed-data JIT/AOT proof: all 3,849 backings and 22 views,
10,000 warm copies each, zero payload allocation, five exact native tables and
all 176 actual ILC references. Ten fresh actual-input controls reject stale or
wrong evidence, including the absent mandatory address/lifetime review. The
whole campaign audit cell remains open until the same actual PE/source passes
that complete review and semantic qualification. See
`artifacts/owner-a-r30-20261018/final-handoff.json` for exact tested-byte
identities and remaining scope. Pure Audit controls cover execution
record/report drift, typed widths/bits, warm allocation thresholds, direct PE
references and native table closure; their synthetic inputs never qualify a
product.
