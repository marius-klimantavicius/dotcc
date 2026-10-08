-- Fresh owned semantic graph. No prior semantic-export schema is used.
PRAGMA application_id = 1146045002; -- DOBJ
PRAGMA user_version = 1;
PRAGMA foreign_keys = ON;
BEGIN IMMEDIATE;
CREATE TABLE object_info (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 format_version INTEGER NOT NULL CHECK(format_version=1),
 profile TEXT NOT NULL CHECK(profile='scalar-control-v14'),
 target_abi TEXT NOT NULL,
 runtime_abi TEXT NOT NULL,
 unit_id TEXT NOT NULL,
 primary_source TEXT NOT NULL,
 source_sha256 TEXT NOT NULL,
 prelower_source INTEGER NOT NULL CHECK(prelower_source=0),
 macros INTEGER NOT NULL CHECK(macros=0),
 lifetimes INTEGER NOT NULL CHECK(lifetimes=0),
 source_declarations INTEGER NOT NULL CHECK(source_declarations=1),
 macro_definitions INTEGER NOT NULL CHECK(macro_definitions=1)
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
 kind TEXT NOT NULL CHECK(kind IN ('void','i8','u8','i16','u16','i32','u32','i64','u64','f32','f64','f80','bool','pointer','array','record','routine','va-list')),
 pointee_type_id INTEGER REFERENCES types(id),
 element_type_id INTEGER REFERENCES types(id),
 element_count INTEGER,
 CHECK((kind='pointer' AND pointee_type_id IS NOT NULL) OR
       (kind<>'pointer' AND pointee_type_id IS NULL)),
 CHECK((kind='array' AND element_type_id IS NOT NULL AND element_count IS NOT NULL AND element_count>=0) OR
       (kind<>'array' AND element_type_id IS NULL AND element_count IS NULL))
) STRICT;
CREATE TABLE routine_types (
 type_id INTEGER PRIMARY KEY REFERENCES types(id),
 return_type_id INTEGER NOT NULL REFERENCES types(id),
 variadic INTEGER NOT NULL CHECK(variadic IN (0,1)),
 exception_policy TEXT NOT NULL CHECK(exception_policy IN ('unwind','terminate'))
) STRICT;
CREATE TABLE routine_parameters (
 type_id INTEGER NOT NULL REFERENCES routine_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 parameter_type_id INTEGER NOT NULL REFERENCES types(id),
 PRIMARY KEY(type_id,ordinal)
) STRICT;
CREATE TABLE source_routine_types (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 return_type_id INTEGER NOT NULL REFERENCES source_types(id),
 variadic INTEGER NOT NULL CHECK(variadic IN (0,1)),
 exception_policy TEXT NOT NULL CHECK(exception_policy IN ('unwind','terminate')),
 parameter_mode TEXT NOT NULL CHECK(parameter_mode IN ('prototype','unspecified'))
) STRICT;
CREATE TABLE source_routine_parameters (
 type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 parameter_type_id INTEGER NOT NULL REFERENCES source_types(id),
 PRIMARY KEY(type_id,ordinal)
) STRICT;
CREATE TABLE storages (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_name TEXT NOT NULL,
 linkage TEXT NOT NULL CHECK(linkage IN ('external','internal')),
 external_key TEXT,
 definition TEXT NOT NULL CHECK(definition IN ('declaration','strong','linkonce')),
 type_id INTEGER NOT NULL REFERENCES types(id),
 source_type_id INTEGER REFERENCES source_types(id),
 is_thread_local INTEGER NOT NULL CHECK(is_thread_local IN (0,1)),
 is_const INTEGER NOT NULL CHECK(is_const IN (0,1)),
 alignment INTEGER NOT NULL CHECK((alignment>0 AND (alignment & (alignment-1))=0) OR (alignment=0 AND definition='declaration')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((linkage='external' AND external_key IS NOT NULL) OR
       (linkage='internal' AND external_key IS NULL))
) STRICT;
CREATE TABLE storage_data (
 storage_id INTEGER NOT NULL REFERENCES storages(id),
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 bytes BLOB NOT NULL CHECK(length(bytes)>0),
 PRIMARY KEY(storage_id,byte_offset)
) STRICT;
CREATE TABLE relocations (
 storage_id INTEGER NOT NULL REFERENCES storages(id),
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 target_storage_id INTEGER REFERENCES storages(id),
 target_entity_id INTEGER REFERENCES entities(id),
 addend INTEGER NOT NULL,
 PRIMARY KEY(storage_id,byte_offset),
 CHECK((target_storage_id IS NOT NULL AND target_entity_id IS NULL) OR
       (target_storage_id IS NULL AND target_entity_id IS NOT NULL))
) STRICT;
CREATE TABLE entities (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_name TEXT NOT NULL,
 linkage TEXT NOT NULL CHECK(linkage IN ('external','internal')),
 external_key TEXT,
 definition TEXT NOT NULL CHECK(definition IN ('declaration','strong','linkonce')),
 return_type_id INTEGER NOT NULL REFERENCES types(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 routine_type_id INTEGER NOT NULL REFERENCES routine_types(type_id),
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
CREATE TABLE initialization_actions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 entity_id INTEGER NOT NULL REFERENCES entities(id),
 phase TEXT NOT NULL CHECK(phase='startup'),
 priority INTEGER NOT NULL CHECK(priority>=0 AND priority<=65535),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 UNIQUE(phase,entity_id),
 UNIQUE(phase,ordinal)
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
 is_synthetic INTEGER NOT NULL CHECK(is_synthetic IN (0,1)),
 CHECK(is_synthetic=0 OR kind='local'),
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
 column INTEGER NOT NULL CHECK(column>=0),
 field_id INTEGER REFERENCES record_fields(id),
 storage_id INTEGER REFERENCES storages(id)
) STRICT;
CREATE TABLE edges (
 parent_id INTEGER NOT NULL REFERENCES nodes(id),
 role TEXT NOT NULL,
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 child_id INTEGER NOT NULL REFERENCES nodes(id),
 PRIMARY KEY(parent_id,role,ordinal)
) STRICT;
CREATE TABLE source_types (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL,
 source_name TEXT NOT NULL,
 underlying_type_id INTEGER REFERENCES source_types(id),
 execution_type_id INTEGER NOT NULL REFERENCES types(id),
 is_const INTEGER NOT NULL CHECK(is_const IN (0,1)),
 is_volatile INTEGER NOT NULL CHECK(is_volatile IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 element_count INTEGER CHECK(element_count>=0)
) STRICT;
CREATE TABLE record_types (
 type_id INTEGER PRIMARY KEY REFERENCES types(id),
 identity_key TEXT NOT NULL,
 source_name TEXT NOT NULL,
 size INTEGER NOT NULL CHECK(size>=0 AND size<=2147483647),
 alignment INTEGER NOT NULL CHECK(alignment IN (0,1,2,4,8,16)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 layout_kind TEXT NOT NULL CHECK(layout_kind IN ('struct','union','class')),
 CHECK((size=0 AND alignment=0) OR (size>0 AND alignment>0))
) STRICT;
CREATE TABLE record_fields (
 id INTEGER PRIMARY KEY CHECK(id>0),
 record_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 type_id INTEGER NOT NULL REFERENCES types(id),
 source_name TEXT NOT NULL,
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 bit_offset INTEGER CHECK(bit_offset>=0 AND bit_offset<8),
 bit_width INTEGER CHECK(bit_width>0 AND bit_width<=64),
 bit_signed INTEGER CHECK(bit_signed IN (0,1)),
 CHECK((bit_offset IS NULL AND bit_width IS NULL AND bit_signed IS NULL) OR
       (bit_offset IS NOT NULL AND bit_width IS NOT NULL AND bit_signed IS NOT NULL))
) STRICT;
CREATE TABLE source_functions (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 return_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 capture_phase TEXT NOT NULL,
 routine_source_type_id INTEGER NOT NULL REFERENCES source_types(id)
) STRICT;
CREATE TABLE source_parameters (
 entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 PRIMARY KEY(entity_id,ordinal)
) STRICT;
CREATE TABLE source_declarations (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL CHECK(kind IN ('function','parameter','local')),
 source_name TEXT NOT NULL,
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 entity_id INTEGER REFERENCES entities(id),
 binding_id INTEGER REFERENCES bindings(id),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((kind='function' AND entity_id IS NOT NULL AND binding_id IS NULL AND function_entity_id IS NULL) OR
       (kind IN ('parameter','local') AND entity_id IS NULL AND binding_id IS NOT NULL AND function_entity_id IS NOT NULL))
) STRICT;
CREATE TABLE macro_definitions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 name TEXT NOT NULL,
 form TEXT NOT NULL CHECK(form IN ('object','function')),
 variadic INTEGER NOT NULL CHECK(variadic IN (0,1)),
 dynamic INTEGER NOT NULL CHECK(dynamic IN (0,1)),
 definition_text TEXT NOT NULL,
 origin TEXT NOT NULL CHECK(origin IN ('source','command-line','predefined')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0)
) STRICT;
CREATE TABLE macro_parameters (
 definition_id INTEGER NOT NULL REFERENCES macro_definitions(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 name TEXT NOT NULL,
 PRIMARY KEY(definition_id,ordinal)
) STRICT;
CREATE TABLE macro_events (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL CHECK(kind IN ('define','undef','push','pop')),
 name TEXT NOT NULL,
 definition_id INTEGER REFERENCES macro_definitions(id),
 origin TEXT NOT NULL CHECK(origin IN ('source','command-line','predefined')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((kind='define' AND definition_id IS NOT NULL) OR
       (kind='undef' AND definition_id IS NULL) OR kind IN ('push','pop'))
) STRICT;
CREATE TABLE content_digest (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 algorithm TEXT NOT NULL CHECK(algorithm='dotobj-semantic-sha256-v1'),
 sha256 TEXT NOT NULL
) STRICT;
CREATE INDEX source_declarations_by_function ON source_declarations(function_entity_id,id);
CREATE INDEX nodes_by_function ON nodes(function_id,id);
CREATE INDEX bindings_by_function ON bindings(function_id,id);
COMMIT;
