-- Resolved program graph. Semantic facts are ordinary relational rows.
-- No parser history, generic EDG field dumps, JSON, or encoded semantic blobs.
PRAGMA application_id = 1146045002;
PRAGMA user_version = 1026;
PRAGMA foreign_keys = ON;
CREATE TABLE object_info (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 format TEXT NOT NULL CHECK(format='relational-program-v1'),
 primary_source TEXT NOT NULL,
 target_abi TEXT NOT NULL,
 char_bit INTEGER NOT NULL CHECK(char_bit>0),
 little_endian INTEGER NOT NULL CHECK(little_endian IN(0,1))
) STRICT;
CREATE TABLE files (
 id INTEGER PRIMARY KEY,
 path TEXT NOT NULL UNIQUE
) STRICT;
CREATE TABLE types (
 id INTEGER PRIMARY KEY,
 kind TEXT NOT NULL,
 builtin_name TEXT,
 semantic_role TEXT CHECK(semantic_role IN('va-list','va-list-storage')),
 name TEXT NOT NULL DEFAULT '',
 link_name TEXT NOT NULL DEFAULT '',
 linkage TEXT NOT NULL DEFAULT 'none',
 size INTEGER,
 alignment INTEGER,
 pointee_type_id INTEGER REFERENCES types(id),
 element_type_id INTEGER REFERENCES types(id),
 element_count INTEGER,
 -- Selected source bound navigation; NULL kind retains fixed/incomplete facts.
 -- A dependent bound node may be NULL when EDG supplies an unknown bound.
 array_bound_kind TEXT CHECK(array_bound_kind IN('dependent','runtime')),
 array_bound_node_id INTEGER REFERENCES nodes(id),
 return_type_id INTEGER REFERENCES types(id),
 return_storage TEXT NOT NULL DEFAULT 'value' CHECK(return_storage IN('value','caller-provided')),
 object_type_id INTEGER REFERENCES types(id),
 -- Selected ABI parameter list already contains the receiver when true.
 -- NULL retains the source callable view and older objects' representation.
 parameters_include_receiver INTEGER CHECK(parameters_include_receiver IN(0,1)),
 underlying_type_id INTEGER REFERENCES types(id),
 is_const INTEGER NOT NULL DEFAULT 0 CHECK(is_const IN(0,1)),
 is_volatile INTEGER NOT NULL DEFAULT 0 CHECK(is_volatile IN(0,1)),
 is_restrict INTEGER NOT NULL DEFAULT 0 CHECK(is_restrict IN(0,1)),
 is_variadic INTEGER NOT NULL DEFAULT 0 CHECK(is_variadic IN(0,1)),
 exception_policy TEXT NOT NULL DEFAULT 'unwind' CHECK(exception_policy IN('unwind','terminate','dynamic','dependent')),
 record_kind TEXT,
 is_dependent INTEGER NOT NULL DEFAULT 0 CHECK(is_dependent IN(0,1)),
 is_incomplete INTEGER NOT NULL DEFAULT 0 CHECK(is_incomplete IN(0,1))
) STRICT;
CREATE TABLE type_parameters (
 type_id INTEGER NOT NULL REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 parameter_type_id INTEGER NOT NULL REFERENCES types(id),
 PRIMARY KEY(type_id,ordinal)
) STRICT, WITHOUT ROWID;
-- Declared dynamic exception specification, in EDG's selected list order.
-- A dynamic specification with no rows is throw(); it is distinct from noexcept.
CREATE TABLE exception_specification_types (
 function_type_id INTEGER NOT NULL REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 allowed_type_id INTEGER NOT NULL REFERENCES types(id),
 PRIMARY KEY(function_type_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE floating_types (
 type_id INTEGER PRIMARY KEY REFERENCES types(id),
 format TEXT NOT NULL,
 precision_bits INTEGER NOT NULL,
 min_exponent INTEGER NOT NULL,
 max_exponent INTEGER NOT NULL
) STRICT;
-- Exact literal value bytes in standard target-format, high-byte-first hex.
-- This scalar literal payload is not an encoded semantic record.
CREATE TABLE floating_values (
 id INTEGER PRIMARY KEY,
 type_id INTEGER NOT NULL REFERENCES types(id),
 bits_hex TEXT NOT NULL,
 UNIQUE(type_id,bits_hex)
) STRICT;
CREATE TABLE enums (
 type_id INTEGER PRIMARY KEY REFERENCES types(id),
 is_scoped INTEGER NOT NULL CHECK(is_scoped IN(0,1)),
 has_explicit_base INTEGER NOT NULL CHECK(has_explicit_base IN(0,1)),
 declared_base_type_id INTEGER REFERENCES types(id),
 is_definition INTEGER NOT NULL CHECK(is_definition IN(0,1)),
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER
) STRICT;
CREATE TABLE enum_values (
 id INTEGER PRIMARY KEY,
 enum_type_id INTEGER NOT NULL REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 name TEXT NOT NULL,
 value_text TEXT,
 expression_node_id INTEGER REFERENCES nodes(id),
 value_type_id INTEGER NOT NULL REFERENCES types(id),
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 UNIQUE(enum_type_id,ordinal)
) STRICT;
CREATE TABLE fields (
 id INTEGER PRIMARY KEY,
 owner_type_id INTEGER NOT NULL REFERENCES types(id),
 name TEXT NOT NULL,
 type_id INTEGER NOT NULL REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 byte_offset INTEGER NOT NULL,
 bit_offset INTEGER,
 bit_width INTEGER,
 bit_is_signed INTEGER CHECK(bit_is_signed IN(0,1)),
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 UNIQUE(owner_type_id,ordinal)
) STRICT;
-- EDG's actual base subobjects, including distinct indirect occurrences.
-- ordinal is EDG postorder; direct_ordinal preserves base declaration order.
CREATE TABLE base_subobjects (
 id INTEGER PRIMARY KEY,
 owner_type_id INTEGER NOT NULL REFERENCES types(id),
 base_type_id INTEGER NOT NULL REFERENCES types(id),
 declared_type_id INTEGER REFERENCES types(id),
 ordinal INTEGER NOT NULL,
 direct_ordinal INTEGER,
 byte_offset INTEGER NOT NULL,
 is_direct INTEGER NOT NULL CHECK(is_direct IN(0,1)),
 is_virtual INTEGER NOT NULL CHECK(is_virtual IN(0,1)),
 is_optimized_empty INTEGER NOT NULL CHECK(is_optimized_empty IN(0,1)),
 access TEXT CHECK(access IN('public','protected','private')),
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
 kind TEXT NOT NULL DEFAULT 'ordinary' CHECK(kind IN('ordinary','constructor','destructor')),
 body_node_id INTEGER NOT NULL REFERENCES nodes(id),
 receiver_symbol_id INTEGER REFERENCES symbols(id),
 nrvo_candidate_symbol_id INTEGER REFERENCES symbols(id),
 exception_policy TEXT NOT NULL DEFAULT 'unwind' CHECK(exception_policy IN('unwind','terminate','dynamic'))
) STRICT;
CREATE TABLE parameters (
 function_id INTEGER NOT NULL REFERENCES symbols(id),
 ordinal INTEGER NOT NULL,
 symbol_id INTEGER NOT NULL REFERENCES symbols(id),
 PRIMARY KEY(function_id,ordinal)
) STRICT, WITHOUT ROWID;
-- Source declaration of an empty function parameter pack. EDG's compiler
-- placeholder is not a runtime static variable and has no executable storage.
CREATE TABLE empty_parameter_packs (
 function_id INTEGER NOT NULL REFERENCES symbols(id),
 ordinal INTEGER NOT NULL,
 name TEXT NOT NULL,
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 PRIMARY KEY(function_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE nodes (
 id INTEGER PRIMARY KEY,
 domain TEXT NOT NULL DEFAULT 'program' CHECK(domain IN('program','template-recipe')),
 function_id INTEGER REFERENCES symbols(id),
 kind TEXT NOT NULL,
 type_id INTEGER REFERENCES types(id),
 execution_type_id INTEGER REFERENCES types(id),
 operation_type_id INTEGER REFERENCES types(id),
 original_lvalue_type_id INTEGER REFERENCES types(id),
 value_category TEXT,
 value_normalization TEXT CHECK(value_normalization IS NULL OR value_normalization='nullptr-zero'),
 operator TEXT,
 constant_text TEXT,
 symbol_id INTEGER REFERENCES symbols(id),
 field_id INTEGER REFERENCES fields(id),
 enum_value_id INTEGER REFERENCES enum_values(id),
 floating_value_id INTEGER REFERENCES floating_values(id),
 literal_object_id INTEGER REFERENCES literal_objects(id),
 literal_byte_offset INTEGER,
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 is_synthetic INTEGER NOT NULL DEFAULT 0 CHECK(is_synthetic IN(0,1)),
 source_node_id INTEGER REFERENCES nodes(id),
 initialization_id INTEGER REFERENCES initializations(id),
 lifetime_id INTEGER REFERENCES lifetimes(id),
 -- Selected control-flow references; these do not own cleanup regions.
 common_lifetime_id INTEGER REFERENCES lifetimes(id), -- goto: common lifetime
 containing_lifetime_id INTEGER REFERENCES lifetimes(id), -- label: target scope
 evaluation_order TEXT NOT NULL DEFAULT 'unspecified' CHECK(evaluation_order IN('unspecified','left-to-right','right-to-left')),
 CHECK(domain!='template-recipe' OR function_id IS NULL)
) STRICT;
CREATE TABLE edges (
 parent_id INTEGER NOT NULL REFERENCES nodes(id),
 role TEXT NOT NULL,
 ordinal INTEGER NOT NULL,
 child_id INTEGER NOT NULL REFERENCES nodes(id),
 PRIMARY KEY(parent_id,role,ordinal)
) STRICT, WITHOUT ROWID;
CREATE INDEX edges_child ON edges(child_id);
CREATE INDEX nodes_function ON nodes(function_id,id);
-- Selected class conversion facts, not a request to resolve a base path.
CREATE TABLE class_conversions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 base_subobject_id INTEGER NOT NULL REFERENCES base_subobjects(id),
 direction TEXT NOT NULL CHECK(direction IN('to-base','to-derived')),
 byte_adjustment INTEGER,
 preserves_null INTEGER NOT NULL CHECK(preserves_null IN(0,1)),
 source_representation TEXT NOT NULL CHECK(source_representation IN('pointer','reference','object')),
 result_representation TEXT NOT NULL CHECK(result_representation IN('pointer','reference','object'))
) STRICT;
CREATE TABLE global_initializers (
 symbol_id INTEGER PRIMARY KEY REFERENCES symbols(id),
 initializer_node_id INTEGER NOT NULL REFERENCES nodes(id)
) STRICT;
CREATE TABLE templates (
 id INTEGER PRIMARY KEY,
 kind TEXT NOT NULL,
 name TEXT NOT NULL,
 parameter_depth INTEGER,
 parameter_position INTEGER,
 is_parameter_pack INTEGER NOT NULL DEFAULT 0 CHECK(is_parameter_pack IN(0,1)),
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER
) STRICT;
CREATE TABLE template_instances (
 id INTEGER PRIMARY KEY,
 template_id INTEGER NOT NULL REFERENCES templates(id),
 symbol_id INTEGER REFERENCES symbols(id),
 type_id INTEGER REFERENCES types(id),
 is_dependent INTEGER NOT NULL DEFAULT 0 CHECK(is_dependent IN(0,1)),
 CHECK((symbol_id IS NULL) <> (type_id IS NULL))
) STRICT;
CREATE TABLE template_type_parameters (
 type_id INTEGER PRIMARY KEY REFERENCES types(id),
 kind TEXT NOT NULL CHECK(kind IN('parameter','member','unknown')),
 parameter_depth INTEGER,
 parameter_position INTEGER,
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 qualifier_type_id INTEGER REFERENCES types(id)
) STRICT;
CREATE TABLE template_formals (
 id INTEGER PRIMARY KEY,
 template_id INTEGER NOT NULL REFERENCES templates(id),
 ordinal INTEGER NOT NULL,
 kind TEXT NOT NULL CHECK(kind IN('type','constant','template')),
 name TEXT NOT NULL,
 type_id INTEGER REFERENCES types(id),
 parameter_template_id INTEGER REFERENCES templates(id),
 parameter_depth INTEGER,
 parameter_position INTEGER,
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 default_type_id INTEGER REFERENCES types(id),
 default_template_id INTEGER REFERENCES templates(id),
 default_value_type_id INTEGER REFERENCES types(id),
 default_value_text TEXT,
 default_floating_value_id INTEGER REFERENCES floating_values(id),
 default_expression_id INTEGER REFERENCES nodes(id),
 file_id INTEGER REFERENCES files(id),
 line INTEGER,
 column INTEGER,
 UNIQUE(template_id,ordinal)
) STRICT;
CREATE TABLE template_arguments (
 instance_id INTEGER NOT NULL REFERENCES template_instances(id),
 ordinal INTEGER NOT NULL,
 parent_ordinal INTEGER,
 kind TEXT NOT NULL,
 is_pack_expansion INTEGER NOT NULL DEFAULT 0 CHECK(is_pack_expansion IN(0,1)),
 type_id INTEGER REFERENCES types(id),
 expression_id INTEGER REFERENCES nodes(id),
 template_id INTEGER REFERENCES templates(id),
 constant_text TEXT,
 floating_value_id INTEGER REFERENCES floating_values(id),
 PRIMARY KEY(instance_id,ordinal),
 FOREIGN KEY(instance_id,parent_ordinal) REFERENCES template_arguments(instance_id,ordinal)
) STRICT, WITHOUT ROWID;

-- EDG-selected initialization and cleanup facts. Ordinals preserve the supplied
-- order; in lifetime_destructions, ordinal zero is the FIRST destruction.
CREATE TABLE lifetimes (
 id INTEGER PRIMARY KEY,
 function_id INTEGER REFERENCES symbols(id),
 kind TEXT NOT NULL,
 parent_id INTEGER REFERENCES lifetimes(id),
 owner_node_id INTEGER REFERENCES nodes(id),
 parent_initialization_id INTEGER REFERENCES initializations(id),
 sibling_ordinal INTEGER
) STRICT;
CREATE TABLE initializations (
 id INTEGER PRIMARY KEY,
 destination_initialization_id INTEGER REFERENCES initializations(id),
 function_id INTEGER REFERENCES symbols(id),
 kind TEXT NOT NULL,
 type_id INTEGER REFERENCES types(id),
 variable_symbol_id INTEGER REFERENCES symbols(id),
 constructor_symbol_id INTEGER REFERENCES symbols(id),
 destructor_symbol_id INTEGER REFERENCES symbols(id),
 lifetime_id INTEGER REFERENCES lifetimes(id),
 init_expr_lifetime_id INTEGER REFERENCES lifetimes(id),
 overlap_lifetime_id INTEGER REFERENCES lifetimes(id),
 value_node_id INTEGER REFERENCES nodes(id),
 implied_source INTEGER NOT NULL DEFAULT 0 CHECK(implied_source IN(0,1)),
 sequenced_arguments INTEGER NOT NULL DEFAULT 0 CHECK(sequenced_arguments IN(0,1)),
 value_initialization INTEGER NOT NULL DEFAULT 0 CHECK(value_initialization IN(0,1)),
 conditional INTEGER NOT NULL DEFAULT 0 CHECK(conditional IN(0,1)),
 unordered INTEGER NOT NULL DEFAULT 0 CHECK(unordered IN(0,1)),
 overlaps_inner INTEGER NOT NULL DEFAULT 0 CHECK(overlaps_inner IN(0,1)),
 partial_aggregate INTEGER NOT NULL DEFAULT 0 CHECK(partial_aggregate IN(0,1)),
 free_on_exception INTEGER NOT NULL DEFAULT 0 CHECK(free_on_exception IN(0,1)),
 follows_exec INTEGER NOT NULL DEFAULT 0 CHECK(follows_exec IN(0,1)),
 temporary_lifetime INTEGER NOT NULL DEFAULT 0 CHECK(temporary_lifetime IN(0,1)),
 is_constructor_init INTEGER NOT NULL DEFAULT 0 CHECK(is_constructor_init IN(0,1)),
 is_partially_initialized INTEGER NOT NULL DEFAULT 0 CHECK(is_partially_initialized IN(0,1)),
 static_storage INTEGER NOT NULL DEFAULT 0 CHECK(static_storage IN(0,1))
) STRICT;
CREATE INDEX lifetimes_function ON lifetimes(function_id,id);
CREATE INDEX initializations_function ON initializations(function_id,id);
CREATE TABLE initialization_arguments (
 initialization_id INTEGER NOT NULL REFERENCES initializations(id),
 ordinal INTEGER NOT NULL,
 node_id INTEGER NOT NULL REFERENCES nodes(id),
 PRIMARY KEY(initialization_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE lifetime_destructions (
 lifetime_id INTEGER NOT NULL REFERENCES lifetimes(id),
 ordinal INTEGER NOT NULL,
 initialization_id INTEGER NOT NULL REFERENCES initializations(id),
 PRIMARY KEY(lifetime_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE constructor_initializers (
 function_id INTEGER NOT NULL REFERENCES symbols(id),
 ordinal INTEGER NOT NULL,
 kind TEXT NOT NULL,
 field_id INTEGER REFERENCES fields(id),
 target_type_id INTEGER REFERENCES types(id),
 base_subobject_id INTEGER REFERENCES base_subobjects(id),
 copy_source_symbol_id INTEGER REFERENCES symbols(id),
 is_virtual INTEGER NOT NULL DEFAULT 0 CHECK(is_virtual IN(0,1)),
 initialization_id INTEGER NOT NULL REFERENCES initializations(id),
 PRIMARY KEY(function_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE try_handlers (
 try_node_id INTEGER NOT NULL REFERENCES nodes(id),
 ordinal INTEGER NOT NULL,
 catch_node_id INTEGER NOT NULL REFERENCES nodes(id),
 parameter_symbol_id INTEGER REFERENCES symbols(id),
 parameter_type_id INTEGER REFERENCES types(id),
 initialization_id INTEGER REFERENCES initializations(id),
 PRIMARY KEY(try_node_id,ordinal)
) STRICT, WITHOUT ROWID;
CREATE TABLE throws (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 type_id INTEGER REFERENCES types(id),
 initialization_id INTEGER REFERENCES initializations(id),
 destructor_symbol_id INTEGER REFERENCES symbols(id),
 is_rethrow INTEGER NOT NULL CHECK(is_rethrow IN(0,1))
) STRICT;

-- EDG-selected virtual dispatch and lifecycle writes, captured at the stock
-- lowering decision points. No override selection is performed by consumers.
CREATE TABLE virtual_calls (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 selected_symbol_id INTEGER NOT NULL REFERENCES symbols(id),
 receiver_node_id INTEGER NOT NULL REFERENCES nodes(id),
 receiver_type_id INTEGER NOT NULL REFERENCES types(id),
 callable_type_id INTEGER NOT NULL REFERENCES types(id),
 -- Physical selected entry signature, distinct from the logical source call.
 abi_callable_type_id INTEGER REFERENCES types(id),
 vptr_byte_offset INTEGER NOT NULL,
 slot_index INTEGER NOT NULL
) STRICT;
CREATE TABLE virtual_tables (
 id INTEGER PRIMARY KEY,
 owner_type_id INTEGER NOT NULL REFERENCES types(id),
 base_subobject_id INTEGER REFERENCES base_subobjects(id),
 construction_base_subobject_id INTEGER REFERENCES base_subobjects(id),
 link_name TEXT NOT NULL,
 linkage TEXT NOT NULL,
 address_point_byte_offset INTEGER NOT NULL,
 entry_size INTEGER NOT NULL,
 is_definition INTEGER NOT NULL CHECK(is_definition IN(0,1))
) STRICT;
CREATE TABLE virtual_table_entries (
 table_id INTEGER NOT NULL REFERENCES virtual_tables(id),
 slot_index INTEGER NOT NULL,
 selected_symbol_id INTEGER NOT NULL REFERENCES symbols(id),
 callable_type_id INTEGER NOT NULL REFERENCES types(id),
 receiver_type_id INTEGER NOT NULL REFERENCES types(id),
 target_symbol_id INTEGER REFERENCES symbols(id),
 entry_kind TEXT NOT NULL CHECK(entry_kind IN('ordinary','pure','deleted','null')),
 target_variant TEXT NOT NULL CHECK(target_variant IN('ordinary','base','complete','deleting')),
 abi_entry_name TEXT NOT NULL,
 abi_entry_symbol_id INTEGER REFERENCES symbols(id),
 abi_callable_type_id INTEGER REFERENCES types(id),
 this_delta INTEGER NOT NULL,
 vcall_index INTEGER NOT NULL,
 return_delta INTEGER NOT NULL,
 vbase_index INTEGER NOT NULL,
 PRIMARY KEY(table_id,slot_index)
) STRICT, WITHOUT ROWID;
CREATE TABLE vptr_writes (
 function_id INTEGER NOT NULL REFERENCES symbols(id),
 ordinal INTEGER NOT NULL,
 phase TEXT NOT NULL CHECK(phase IN('after-bases-before-members','before-body')),
 base_subobject_id INTEGER REFERENCES base_subobjects(id),
 vptr_byte_offset INTEGER,
 table_id INTEGER REFERENCES virtual_tables(id),
 source_kind TEXT NOT NULL CHECK(source_kind IN('table','construction-table')),
 construction_index INTEGER,
 PRIMARY KEY(function_id,ordinal)
) STRICT, WITHOUT ROWID;

CREATE TABLE template_value_references (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 name TEXT NOT NULL,
 parameter_depth INTEGER,
 parameter_position INTEGER,
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 qualifier_type_id INTEGER REFERENCES types(id)
) STRICT;

CREATE TABLE array_initializers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 zero_initialize INTEGER NOT NULL CHECK(zero_initialize IN(0,1))
) STRICT;
CREATE TABLE array_initializer_elements (
 initializer_node_id INTEGER NOT NULL REFERENCES nodes(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 array_index INTEGER NOT NULL CHECK(array_index>=0),
 repeat_count INTEGER NOT NULL CHECK(repeat_count>0),
 value_node_id INTEGER NOT NULL REFERENCES nodes(id),
 PRIMARY KEY(initializer_node_id,ordinal)
) STRICT, WITHOUT ROWID;

CREATE TABLE record_initializers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 zero_byte_count INTEGER NOT NULL CHECK(zero_byte_count>=0)
) STRICT;
CREATE TABLE record_initializer_elements (
 initializer_node_id INTEGER NOT NULL REFERENCES nodes(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 field_id INTEGER REFERENCES fields(id),
 base_subobject_id INTEGER REFERENCES base_subobjects(id),
 value_node_id INTEGER NOT NULL REFERENCES nodes(id),
 CHECK((field_id IS NULL) <> (base_subobject_id IS NULL)),
 PRIMARY KEY(initializer_node_id,ordinal)
) STRICT, WITHOUT ROWID;

-- Literal content is exact target character storage, not an encoded graph.
CREATE TABLE literal_objects (
 id INTEGER PRIMARY KEY,
 type_id INTEGER NOT NULL REFERENCES types(id),
 selected_symbol_id INTEGER REFERENCES symbols(id),
 byte_count INTEGER NOT NULL CHECK(byte_count>=0)
) STRICT;
CREATE TABLE literal_bytes (
 literal_id INTEGER NOT NULL REFERENCES literal_objects(id),
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 value INTEGER NOT NULL CHECK(value BETWEEN 0 AND 255),
 PRIMARY KEY(literal_id,byte_offset)
) STRICT, WITHOUT ROWID;
CREATE TABLE literal_initializers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 copy_byte_count INTEGER NOT NULL CHECK(copy_byte_count>=0),
 zero_byte_count INTEGER NOT NULL CHECK(zero_byte_count>=0)
) STRICT;

-- Selected allocation decisions. Check/size values are ordinary expression nodes.
CREATE TABLE allocations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 allocated_type_id INTEGER NOT NULL REFERENCES types(id),
 allocator_symbol_id INTEGER NOT NULL REFERENCES symbols(id),
 count_value_node_id INTEGER REFERENCES nodes(id),
 count_symbol_id INTEGER REFERENCES symbols(id),
 size_node_id INTEGER NOT NULL REFERENCES nodes(id),
 length_check_node_id INTEGER REFERENCES nodes(id),
 length_failure_symbol_id INTEGER REFERENCES symbols(id),
 result_byte_offset INTEGER NOT NULL,
 construction_null_check INTEGER CHECK(construction_null_check IN(0,1)),
 CHECK(count_symbol_id IS NULL OR count_value_node_id IS NOT NULL),
 CHECK((length_check_node_id IS NULL) = (length_failure_symbol_id IS NULL))
) STRICT;
