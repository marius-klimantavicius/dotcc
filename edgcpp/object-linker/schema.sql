-- Fresh owned semantic graph. No prior semantic-export schema is used.
PRAGMA application_id = 1146045002; -- DOBJ
PRAGMA user_version = 1;
PRAGMA foreign_keys = ON;
BEGIN IMMEDIATE;
CREATE TABLE object_info (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 format_version INTEGER NOT NULL CHECK(format_version=1),
 profile TEXT NOT NULL CHECK(profile='scalar-control-v3'),
 target_abi TEXT NOT NULL,
 runtime_abi TEXT NOT NULL,
 unit_id TEXT NOT NULL,
 primary_source TEXT NOT NULL,
 source_sha256 TEXT NOT NULL,
 prelower_source INTEGER NOT NULL CHECK(prelower_source=0),
 macros INTEGER NOT NULL CHECK(macros=0),
 lifetimes INTEGER NOT NULL CHECK(lifetimes=0)
) STRICT;
CREATE TABLE provenance (
 ordinal INTEGER PRIMARY KEY,
 kind TEXT NOT NULL,
 value TEXT NOT NULL
) STRICT;
CREATE TABLE source_files (
 id INTEGER PRIMARY KEY CHECK(id>0),
 path TEXT NOT NULL,
 parent_id INTEGER REFERENCES source_files(id),
 available INTEGER NOT NULL CHECK(available IN (0,1)),
 sha256 TEXT,
 bytes BLOB,
 CHECK((available=1 AND sha256 IS NOT NULL AND bytes IS NOT NULL) OR
       (available=0 AND sha256 IS NULL AND bytes IS NULL))
) STRICT;
CREATE TABLE types (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL CHECK(kind IN ('void','i8','u8','i16','u16','i32','u32','i64','u64','bool'))
) STRICT;
CREATE TABLE entities (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_name TEXT NOT NULL,
 linkage TEXT NOT NULL CHECK(linkage IN ('external','internal')),
 external_key TEXT,
 definition TEXT NOT NULL CHECK(definition IN ('declaration','strong')),
 return_type_id INTEGER NOT NULL REFERENCES types(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((linkage='external' AND external_key IS NOT NULL) OR
       (linkage='internal' AND external_key IS NULL))
) STRICT;
CREATE TABLE parameters (
 entity_id INTEGER NOT NULL REFERENCES entities(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 type_id INTEGER NOT NULL REFERENCES types(id),
 PRIMARY KEY(entity_id,ordinal)
) STRICT;
CREATE TABLE functions (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 body_node_id INTEGER NOT NULL REFERENCES nodes(id)
) STRICT;
CREATE TABLE bindings (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_id INTEGER NOT NULL REFERENCES functions(entity_id),
 kind TEXT NOT NULL CHECK(kind IN ('parameter','local')),
 ordinal INTEGER,
 type_id INTEGER NOT NULL REFERENCES types(id),
 source_name TEXT NOT NULL,
 scope_node_id INTEGER NOT NULL REFERENCES nodes(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((kind='parameter' AND ordinal>=0) OR (kind='local' AND ordinal IS NULL))
) STRICT;
CREATE TABLE nodes (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_id INTEGER NOT NULL REFERENCES functions(entity_id),
 kind TEXT NOT NULL,
 type_id INTEGER REFERENCES types(id),
 entity_id INTEGER REFERENCES entities(id),
 binding_id INTEGER REFERENCES bindings(id),
 operator TEXT,
 constant_text TEXT,
 value_category TEXT,
 evaluation_order TEXT,
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0)
) STRICT;
CREATE TABLE edges (
 parent_id INTEGER NOT NULL REFERENCES nodes(id),
 role TEXT NOT NULL,
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 child_id INTEGER NOT NULL REFERENCES nodes(id),
 PRIMARY KEY(parent_id,role,ordinal)
) STRICT;
CREATE TABLE content_digest (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 algorithm TEXT NOT NULL CHECK(algorithm='dotobj-semantic-sha256-v1'),
 sha256 TEXT NOT NULL
) STRICT;
CREATE INDEX nodes_by_function ON nodes(function_id,id);
CREATE INDEX bindings_by_function ON bindings(function_id,id);
COMMIT;
