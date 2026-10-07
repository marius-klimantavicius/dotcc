-- edg-semantic-sqlite/1. Node identities are (unit_id,node_id).
-- Graph semantic schema remains edg-semantic/1. Reader selects one unit;
-- multiple units do not imply semantic linking.
PRAGMA application_id = 1162102611; -- ASCII EDGS
PRAGMA user_version = 1;
CREATE TABLE units(unit_id INTEGER PRIMARY KEY);
CREATE TABLE graph_metadata(unit_id INTEGER NOT NULL, key TEXT NOT NULL, value_json TEXT NOT NULL, PRIMARY KEY(unit_id,key)) WITHOUT ROWID;
CREATE TABLE node_kinds(kind_id INTEGER PRIMARY KEY, name TEXT NOT NULL UNIQUE, table_name TEXT NOT NULL UNIQUE);
CREATE TABLE node_fields(kind_id INTEGER NOT NULL, ordinal INTEGER NOT NULL, name TEXT NOT NULL, value_kind TEXT NOT NULL, PRIMARY KEY(kind_id,name), UNIQUE(kind_id,ordinal)) WITHOUT ROWID;
-- text payloads are decoded JSON strings encoded as UTF-8; json payloads are
-- complete JSON object/array values. Pool equality shares bytes, never nodes.
CREATE TABLE literals(literal_id INTEGER PRIMARY KEY, encoding TEXT NOT NULL CHECK(encoding IN ('text','json')), payload_hash INTEGER NOT NULL, payload BLOB NOT NULL);
-- Hash is an acceleration hint only: compare exact bytes within each bucket.
CREATE INDEX literals_lookup ON literals(encoding,payload_hash);
CREATE TABLE nodes(unit_id INTEGER NOT NULL, node_id INTEGER NOT NULL, kind_id INTEGER NOT NULL, form_literal_id INTEGER, edge_count INTEGER, PRIMARY KEY(unit_id,node_id)) WITHOUT ROWID;
CREATE INDEX nodes_kind ON nodes(unit_id,kind_id,node_id);
-- Each node_<kind> table has (unit_id,node_id) composite PK, followed by
-- named scalar columns. Its exact field names/types are in node_fields.
-- value_kind: integer|boolean|text|json|null. INTEGER columns use BLOB decimal
-- ASCII for integers outside signed64. text/json columns are literal IDs.
-- SQL NULL = absent field; value_kind=null with stored 0 = explicit JSON null.
CREATE TABLE native_edges(unit_id INTEGER NOT NULL, source_id INTEGER NOT NULL, ordinal INTEGER NOT NULL, target_id INTEGER NOT NULL, target_kind_id INTEGER NOT NULL, PRIMARY KEY(unit_id,source_id,ordinal)) WITHOUT ROWID;
CREATE TABLE macros(unit_id INTEGER NOT NULL, invocation_id INTEGER NOT NULL, parent_index INTEGER NOT NULL, definition_id INTEGER NOT NULL, start_literal_id INTEGER NOT NULL, end_literal_id INTEGER NOT NULL, arguments_literal_id INTEGER NOT NULL, PRIMARY KEY(unit_id,invocation_id)) WITHOUT ROWID;
CREATE TABLE diagnostics(unit_id INTEGER NOT NULL, ordinal INTEGER NOT NULL, node_id INTEGER NOT NULL, code_literal_id INTEGER NOT NULL, message_literal_id INTEGER NOT NULL, PRIMARY KEY(unit_id,ordinal)) WITHOUT ROWID;
-- The wrapper completes these tables after native export. Native writer creates
-- them empty. A database lacking package_info.manifest is not a complete package.
CREATE TABLE source_blobs(sha256 TEXT PRIMARY KEY, bytes BLOB NOT NULL) WITHOUT ROWID;
CREATE TABLE source_files(unit_id INTEGER NOT NULL, node_id INTEGER NOT NULL, original_path TEXT NOT NULL, content_sha256 TEXT, available INTEGER NOT NULL, byte_length INTEGER, PRIMARY KEY(unit_id,node_id)) WITHOUT ROWID;
CREATE TABLE package_info(key TEXT PRIMARY KEY, value_json TEXT NOT NULL) WITHOUT ROWID;
