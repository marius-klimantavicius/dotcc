-- Resolved program graph. Semantic facts are ordinary relational rows.
-- No parser history, generic EDG field dumps, JSON, or encoded semantic blobs.
PRAGMA application_id = 1146045002;
PRAGMA user_version = 1001;
PRAGMA foreign_keys = ON;
CREATE TABLE object_info (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 format TEXT NOT NULL CHECK(format='relational-program-v1'),
 primary_source TEXT NOT NULL,
 target_abi TEXT NOT NULL
) STRICT;
CREATE TABLE files (
 id INTEGER PRIMARY KEY,
 path TEXT NOT NULL UNIQUE
) STRICT;
CREATE TABLE types (
 id INTEGER PRIMARY KEY,
 kind TEXT NOT NULL,
 name TEXT NOT NULL DEFAULT '',
 link_name TEXT NOT NULL DEFAULT '',
 linkage TEXT NOT NULL DEFAULT 'none',
 size INTEGER,
 alignment INTEGER,
 pointee_type_id INTEGER REFERENCES types(id),
 element_type_id INTEGER REFERENCES types(id),
 element_count INTEGER,
 return_type_id INTEGER REFERENCES types(id),
 object_type_id INTEGER REFERENCES types(id),
 underlying_type_id INTEGER REFERENCES types(id),
 is_const INTEGER NOT NULL DEFAULT 0 CHECK(is_const IN(0,1)),
 is_volatile INTEGER NOT NULL DEFAULT 0 CHECK(is_volatile IN(0,1)),
 is_restrict INTEGER NOT NULL DEFAULT 0 CHECK(is_restrict IN(0,1)),
 is_variadic INTEGER NOT NULL DEFAULT 0 CHECK(is_variadic IN(0,1)),
 record_kind TEXT,
 is_incomplete INTEGER NOT NULL DEFAULT 0 CHECK(is_incomplete IN(0,1))
) STRICT;
CREATE TABLE type_parameters (
 type_id INTEGER NOT NULL REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 parameter_type_id INTEGER NOT NULL REFERENCES types(id),
 PRIMARY KEY(type_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE fields (
 id INTEGER PRIMARY KEY,
 owner_type_id INTEGER NOT NULL REFERENCES types(id),
 name TEXT NOT NULL,
 type_id INTEGER NOT NULL REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 byte_offset INTEGER NOT NULL,
 bit_offset INTEGER,
 bit_width INTEGER,
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 UNIQUE(owner_type_id,ordinal)
) STRICT;
CREATE TABLE symbols (
 id INTEGER PRIMARY KEY,
 kind TEXT NOT NULL,
 name TEXT NOT NULL,
 link_name TEXT NOT NULL,
 linkage TEXT NOT NULL,
 type_id INTEGER NOT NULL REFERENCES types(id),
 parent_symbol_id INTEGER REFERENCES symbols(id),
 is_definition INTEGER NOT NULL DEFAULT 0 CHECK(is_definition IN(0,1)),
 definition_kind TEXT NOT NULL DEFAULT 'strong',
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER
) STRICT;
CREATE TABLE functions (
 symbol_id INTEGER PRIMARY KEY REFERENCES symbols(id),
 body_node_id INTEGER NOT NULL REFERENCES nodes(id),
 receiver_symbol_id INTEGER REFERENCES symbols(id)
) STRICT;
CREATE TABLE parameters (
 function_id INTEGER NOT NULL REFERENCES symbols(id),
 ordinal INTEGER NOT NULL,
 symbol_id INTEGER NOT NULL REFERENCES symbols(id),
 PRIMARY KEY(function_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE nodes (
 id INTEGER PRIMARY KEY,
 function_id INTEGER REFERENCES symbols(id),
 kind TEXT NOT NULL,
 type_id INTEGER REFERENCES types(id),
 execution_type_id INTEGER REFERENCES types(id),
 original_lvalue_type_id INTEGER REFERENCES types(id),
 value_category TEXT,
 operator TEXT,
 constant_text TEXT,
 symbol_id INTEGER REFERENCES symbols(id),
 field_id INTEGER REFERENCES fields(id),
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 is_synthetic INTEGER NOT NULL DEFAULT 0 CHECK(is_synthetic IN(0,1)),
 source_node_id INTEGER REFERENCES nodes(id)
) STRICT;
CREATE TABLE edges (
 parent_id INTEGER NOT NULL REFERENCES nodes(id),
 role TEXT NOT NULL,
 ordinal INTEGER NOT NULL,
 child_id INTEGER NOT NULL REFERENCES nodes(id),
 PRIMARY KEY(parent_id,role,ordinal)
) STRICT, WITHOUT ROWID;
CREATE INDEX edges_child ON edges(child_id);
CREATE TABLE global_initializers (
 symbol_id INTEGER PRIMARY KEY REFERENCES symbols(id),
 initializer_node_id INTEGER NOT NULL REFERENCES nodes(id)
) STRICT;
CREATE TABLE templates (
 id INTEGER PRIMARY KEY,
 kind TEXT NOT NULL,
 name TEXT NOT NULL,
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER
) STRICT;
CREATE TABLE template_instances (
 id INTEGER PRIMARY KEY,
 template_id INTEGER NOT NULL REFERENCES templates(id),
 symbol_id INTEGER REFERENCES symbols(id),
 type_id INTEGER REFERENCES types(id),
 CHECK((symbol_id IS NULL) <> (type_id IS NULL))
) STRICT;
CREATE TABLE template_arguments (
 instance_id INTEGER NOT NULL REFERENCES template_instances(id),
 ordinal INTEGER NOT NULL,
 kind TEXT NOT NULL,
 type_id INTEGER REFERENCES types(id),
 expression_id INTEGER REFERENCES nodes(id),
 template_id INTEGER REFERENCES templates(id),
 constant_text TEXT,
 PRIMARY KEY(instance_id,ordinal)
) STRICT, WITHOUT ROWID;
