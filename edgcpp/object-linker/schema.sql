-- Fresh owned semantic graph. No prior semantic-export schema is used.
PRAGMA application_id = 1146045002; -- DOBJ
PRAGMA user_version = 1;
PRAGMA foreign_keys = ON;
BEGIN IMMEDIATE;
CREATE TABLE object_info (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 format_version INTEGER NOT NULL CHECK(format_version=1),
 profile TEXT NOT NULL CHECK(profile='scalar-control-v34'),
 target_abi TEXT NOT NULL,
 runtime_abi TEXT NOT NULL,
 unit_id TEXT NOT NULL,
 primary_source TEXT NOT NULL,
 source_sha256 TEXT NOT NULL,
 prelower_source INTEGER NOT NULL CHECK(prelower_source=0),
 macros INTEGER NOT NULL CHECK(macros=0),
 lifetimes INTEGER NOT NULL CHECK(lifetimes=0),
 source_declarations INTEGER NOT NULL CHECK(source_declarations=1),
 macro_definitions INTEGER NOT NULL CHECK(macro_definitions=1),
 source_language TEXT NOT NULL CHECK(source_language IN ('c','c++'))
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
 kind TEXT NOT NULL CHECK(kind IN ('void','i8','u8','i16','u16','i32','u32','i64','u64','i128','u128','f32','f64','f80','bool','pointer','array','record','routine','va-list')),
 pointee_type_id INTEGER REFERENCES types(id),
 element_type_id INTEGER REFERENCES types(id),
 element_count INTEGER,
 CHECK((kind='pointer' AND pointee_type_id IS NOT NULL) OR
       (kind<>'pointer' AND pointee_type_id IS NULL)),
 CHECK((kind='array' AND element_type_id IS NOT NULL AND (element_count IS NULL OR element_count>=0)) OR
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
 parameter_mode TEXT NOT NULL CHECK(parameter_mode IN ('prototype','unspecified')),
 object_type_id INTEGER REFERENCES source_types(id),
 object_is_const INTEGER NOT NULL CHECK(object_is_const IN (0,1)),
 object_is_volatile INTEGER NOT NULL CHECK(object_is_volatile IN (0,1)),
 object_ref_qualifier TEXT NOT NULL CHECK(object_ref_qualifier IN ('none','lvalue','rvalue')),
 CHECK(object_type_id IS NOT NULL OR
       (object_is_const=0 AND object_is_volatile=0 AND object_ref_qualifier='none'))
) STRICT;
CREATE TABLE source_routine_parameters (
 type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 parameter_type_id INTEGER NOT NULL REFERENCES source_types(id),
 PRIMARY KEY(type_id,ordinal)
) STRICT;
CREATE TABLE source_routine_abis (
 source_routine_type_id INTEGER PRIMARY KEY REFERENCES source_routine_types(type_id),
 return_passing TEXT NOT NULL CHECK(return_passing IN ('direct','result-buffer')),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('source-c-signature','observed-routine-lowering','observed-runtime-signature'))
) STRICT;
CREATE TABLE source_routine_abi_parameters (
 source_routine_type_id INTEGER NOT NULL REFERENCES source_routine_abis(source_routine_type_id),
 execution_ordinal INTEGER NOT NULL CHECK(execution_ordinal>=0),
 parameter_role TEXT NOT NULL CHECK(parameter_role IN ('source','indirect-source','this','result-buffer')),
 source_ordinal INTEGER CHECK(source_ordinal>=0),
 CHECK((parameter_role IN ('source','indirect-source') AND source_ordinal IS NOT NULL) OR
       (parameter_role IN ('this','result-buffer') AND source_ordinal IS NULL)),
 UNIQUE(source_routine_type_id,source_ordinal),
 PRIMARY KEY(source_routine_type_id,execution_ordinal)
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
 parameter_role TEXT NOT NULL CHECK(parameter_role IN ('source','indirect-source','this','result-buffer','forwarded-source')),
 source_ordinal INTEGER CHECK(source_ordinal>=0),
 CHECK((parameter_role IN ('source','indirect-source','forwarded-source') AND source_ordinal IS NOT NULL) OR
       (parameter_role IN ('this','result-buffer') AND source_ordinal IS NULL)),
 UNIQUE(entity_id,source_ordinal),
 PRIMARY KEY(entity_id,ordinal)
) STRICT;
CREATE TABLE abi_routine_origins (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 primary_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 entry_kind TEXT NOT NULL CHECK(entry_kind IN ('complete','subobject','deleting','constructor-default-adapter')),
 is_alias_entry INTEGER NOT NULL CHECK(is_alias_entry IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('primary-ctor-dtor-link','observed-default-wrapper-creation')),
 target_entity_id INTEGER REFERENCES entities(id),
 CHECK(entity_id<>primary_entity_id),
 CHECK((entry_kind='constructor-default-adapter' AND capture_proof='observed-default-wrapper-creation' AND is_alias_entry=0 AND target_entity_id IS NOT NULL AND target_entity_id<>entity_id) OR
       (entry_kind<>'constructor-default-adapter' AND capture_proof='primary-ctor-dtor-link' AND target_entity_id IS NULL))
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
CREATE TABLE source_lifetimes (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 parent_lifetime_id INTEGER REFERENCES source_lifetimes(id),
 sibling_ordinal INTEGER NOT NULL CHECK(sibling_ordinal>=0),
 parent_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 kind TEXT NOT NULL CHECK(kind IN ('block','block-after-label','expression-temporary','try-block')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 UNIQUE(function_entity_id,parent_lifetime_id,sibling_ordinal)
) STRICT;
CREATE TABLE source_destruction_obligations (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 destructor_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 UNIQUE(lifetime_id,ordinal)
) STRICT;
CREATE TABLE expression_lifetimes (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-lifetime-wrapper')
) STRICT;
CREATE TABLE base_subobject_expressions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_base_id INTEGER NOT NULL REFERENCES source_record_bases(id)
) STRICT;
CREATE TABLE normal_destructions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_obligation_id INTEGER NOT NULL REFERENCES source_destruction_obligations(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-normal-destruction')
) STRICT;
CREATE TABLE cleanup_actions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 source_obligation_id INTEGER NOT NULL REFERENCES source_destruction_obligations(id),
 callback_entity_id INTEGER NOT NULL REFERENCES entities(id),
 target_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-region-entry')
) STRICT;
CREATE TABLE cleanup_conditions (
 action_id INTEGER PRIMARY KEY REFERENCES cleanup_actions(id),
 flag_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-conditional-cleanup-flag')
) STRICT;
CREATE TABLE cleanup_states (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 action_id INTEGER UNIQUE REFERENCES cleanup_actions(id),
 next_state_id INTEGER REFERENCES cleanup_states(id),
 handler_node_id INTEGER UNIQUE REFERENCES source_handlers(node_id),
 CHECK((action_id IS NULL) != (handler_node_id IS NULL))
) STRICT;
CREATE TABLE cleanup_transitions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 state_id INTEGER REFERENCES cleanup_states(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-cleanup-state'),
 is_unreachable INTEGER NOT NULL CHECK(is_unreachable IN (0,1))
) STRICT;
CREATE TABLE cleanup_frame_markers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 kind TEXT NOT NULL CHECK(kind IN ('prologue','epilogue')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-cleanup-frame')
) STRICT;
CREATE TABLE generated_routine_origins (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 kind TEXT NOT NULL CHECK(kind='file-scope-initializer'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-file-scope-init-creation'),
 failure_policy TEXT NOT NULL CHECK(failure_policy='terminate')
) STRICT;
CREATE TABLE dynamic_initializers (
 id INTEGER PRIMARY KEY CHECK(id>0),
 action_id INTEGER NOT NULL REFERENCES initialization_actions(id),
 storage_id INTEGER NOT NULL UNIQUE REFERENCES storages(id),
 source_ordinal INTEGER NOT NULL UNIQUE CHECK(source_ordinal>=0),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-file-scope-init-lowering'),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 UNIQUE(action_id,ordinal)
) STRICT;
CREATE TABLE dynamic_initializer_statements (
 initializer_id INTEGER NOT NULL REFERENCES dynamic_initializers(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 PRIMARY KEY(initializer_id,ordinal)
) STRICT;
CREATE TABLE generated_routine_scaffolds (
 entity_id INTEGER NOT NULL REFERENCES generated_routine_origins(entity_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 binding_id INTEGER NOT NULL REFERENCES bindings(id),
 kind TEXT NOT NULL CHECK(kind IN ('declaration','zero-initialization')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-scope-binding'),
 PRIMARY KEY(entity_id,ordinal)
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
 CHECK(is_synthetic=0 OR kind IN ('local','parameter')),
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
CREATE TABLE source_record_types (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_type_id INTEGER NOT NULL UNIQUE REFERENCES source_types(id),
 identity_key TEXT NOT NULL,
 source_name TEXT NOT NULL,
 record_kind TEXT NOT NULL CHECK(record_kind IN ('struct','union','class')),
 is_complete INTEGER NOT NULL CHECK(is_complete IN (0,1)),
 native_size INTEGER NOT NULL CHECK(native_size>=0),
 native_alignment INTEGER NOT NULL CHECK(native_alignment>=0),
 is_trivially_copyable INTEGER CHECK(is_trivially_copyable IN (0,1)),
 capture_phase TEXT NOT NULL CHECK(capture_phase IN ('before-lowering','before-class-prelowering')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 is_packed INTEGER NOT NULL CHECK(is_packed IN (0,1)),
 max_member_alignment INTEGER NOT NULL CHECK(max_member_alignment>=0 AND (max_member_alignment & (max_member_alignment-1))=0),
 explicit_alignment INTEGER NOT NULL CHECK(explicit_alignment>=0 AND (explicit_alignment & (explicit_alignment-1))=0),
 CHECK((is_complete=0 AND native_size=0 AND native_alignment=0 AND is_trivially_copyable IS NULL) OR
       (is_complete=1 AND native_alignment>0 AND is_trivially_copyable IS NOT NULL))
) STRICT;
CREATE TABLE source_record_operations (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_types(id),
 bitwise_construction_allowed INTEGER NOT NULL CHECK(bitwise_construction_allowed IN (0,1)),
 bitwise_assignment_allowed INTEGER NOT NULL CHECK(bitwise_assignment_allowed IN (0,1)),
 requires_return_storage INTEGER NOT NULL CHECK(requires_return_storage IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-class-prelowering')
) STRICT;
CREATE TABLE record_value_transfers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 kind TEXT NOT NULL CHECK(kind IN ('construction','temporary')),
 capture_proof TEXT NOT NULL,
 CHECK((kind='construction' AND capture_proof='observed-dynamic-init') OR
       (kind='temporary' AND capture_proof='observed-temporary-assignment'))
) STRICT;
CREATE TABLE source_record_fields (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_record_id INTEGER NOT NULL REFERENCES source_record_types(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_name TEXT NOT NULL,
 is_mutable INTEGER NOT NULL CHECK(is_mutable IN (0,1)),
 has_no_unique_address INTEGER NOT NULL CHECK(has_no_unique_address IN (0,1)),
 bit_width INTEGER CHECK(bit_width>=0),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 has_initializer INTEGER NOT NULL CHECK(has_initializer IN (0,1)),
 is_packed INTEGER NOT NULL CHECK(is_packed IN (0,1)),
 explicit_alignment INTEGER NOT NULL CHECK(explicit_alignment>=0 AND (explicit_alignment & (explicit_alignment-1))=0),
 UNIQUE(source_record_id,ordinal)
) STRICT;
CREATE TABLE source_record_bases (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_record_id INTEGER NOT NULL REFERENCES source_record_types(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 base_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 access_kind TEXT NOT NULL CHECK(access_kind IN ('public','protected','private')),
 is_virtual INTEGER NOT NULL CHECK(is_virtual IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 UNIQUE(source_record_id,ordinal)
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
 source_record_id INTEGER REFERENCES source_record_types(id),
 representation_kind TEXT NOT NULL CHECK(representation_kind IN ('complete','abi','subobject')),
 value_copy_kind TEXT NOT NULL CHECK(value_copy_kind IN ('ordinary','explicit-only')),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('source-snapshot','observed-class-lowering','observed-abi-creation','observed-subobject-creation')),
 complete_type_id INTEGER REFERENCES record_types(type_id),
 CHECK((representation_kind='complete' AND source_record_id IS NOT NULL AND capture_proof IN ('source-snapshot','observed-class-lowering')) OR
       (representation_kind='abi' AND source_record_id IS NULL AND capture_proof='observed-abi-creation') OR
       (representation_kind='subobject' AND source_record_id IS NOT NULL AND capture_proof='observed-subobject-creation' AND value_copy_kind='explicit-only')),
 CHECK((representation_kind='subobject' AND complete_type_id IS NOT NULL AND complete_type_id<>type_id AND size>0) OR
       (representation_kind<>'subobject' AND complete_type_id IS NULL)),
 CHECK((size=0 AND alignment=0) OR (size>0 AND alignment>0))
) STRICT;
CREATE TABLE record_fields (
 id INTEGER PRIMARY KEY CHECK(id>0),
 record_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 type_id INTEGER NOT NULL REFERENCES types(id),
 source_name TEXT NOT NULL,
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 source_type_id INTEGER REFERENCES source_types(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 bit_offset INTEGER CHECK(bit_offset>=0 AND bit_offset<8),
 bit_width INTEGER CHECK(bit_width>0 AND bit_width<=64),
 bit_signed INTEGER CHECK(bit_signed IN (0,1)),
 source_field_id INTEGER REFERENCES source_record_fields(id),
 field_role TEXT NOT NULL CHECK(field_role IN ('source','vptr','abi','base')),
 source_base_id INTEGER REFERENCES source_record_bases(id),
 observed_alignment INTEGER CHECK(observed_alignment>0 AND (observed_alignment & (observed_alignment-1))=0),
 CHECK(field_role<>'source' OR bit_width IS NOT NULL OR observed_alignment IS NOT NULL),
 CHECK(bit_width IS NULL OR observed_alignment IS NULL),
 CHECK((field_role='source' AND source_field_id IS NOT NULL AND source_type_id IS NOT NULL) OR
       (field_role IN ('vptr','abi','base') AND source_field_id IS NULL AND source_type_id IS NULL)),
 CHECK((field_role='base' AND source_base_id IS NOT NULL AND bit_offset IS NULL) OR
       (field_role<>'base' AND source_base_id IS NULL)),
 CHECK((bit_offset IS NULL AND bit_width IS NULL AND bit_signed IS NULL) OR
       (bit_offset IS NOT NULL AND bit_width IS NOT NULL AND bit_signed IS NOT NULL))
) STRICT;
CREATE TABLE record_base_subobjects (
 record_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 source_base_id INTEGER NOT NULL REFERENCES source_record_bases(id),
 complete_base_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 physical_view_type_id INTEGER REFERENCES record_types(type_id),
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 extent INTEGER NOT NULL CHECK(extent>=0),
 field_id INTEGER REFERENCES record_fields(id),
 PRIMARY KEY(record_type_id,source_base_id),
 UNIQUE(field_id),
 CHECK((extent=0 AND physical_view_type_id IS NULL AND field_id IS NULL) OR
       (extent>0 AND physical_view_type_id IS NOT NULL AND field_id IS NOT NULL))
) STRICT;
CREATE TABLE source_functions (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 return_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 capture_phase TEXT NOT NULL,
 routine_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 function_kind TEXT NOT NULL CHECK(function_kind IN ('ordinary','constructor','destructor','runtime'))
) STRICT;
CREATE TABLE source_parameters (
 entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 has_default_argument INTEGER NOT NULL CHECK(has_default_argument IN (0,1)),
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
CREATE TABLE memory_barriers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 is_volatile INTEGER NOT NULL CHECK(is_volatile IN (0,1)),
 has_volatile_keyword INTEGER NOT NULL CHECK(has_volatile_keyword IN (0,1)),
 has_inline_keyword INTEGER NOT NULL CHECK(has_inline_keyword IN (0,1)),
 clobbers_memory INTEGER NOT NULL CHECK(clobbers_memory IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-empty-gnu-asm'),
 CHECK(has_volatile_keyword=0 OR is_volatile=1)
) STRICT;
CREATE TABLE memory_barrier_operands (
 node_id INTEGER NOT NULL REFERENCES memory_barriers(node_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 direction TEXT NOT NULL CHECK(direction IN ('input','output','inout')),
 constraint_text TEXT NOT NULL CHECK(constraint_text IN ('m','=m','+m')),
 source_name TEXT NOT NULL,
 source_element_type_id INTEGER NOT NULL REFERENCES source_types(id),
 element_size INTEGER NOT NULL CHECK(element_size>0),
 source_shape TEXT NOT NULL CHECK(source_shape IN ('object','fixed-array','vla-array')),
 extent_kind TEXT NOT NULL CHECK(extent_kind IN ('fixed','vla-total')),
 fixed_element_count INTEGER CHECK(fixed_element_count>=0),
 extent_binding_id INTEGER REFERENCES bindings(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('observed-asm-place','observed-asm-vla-total')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 PRIMARY KEY(node_id,ordinal),
 CHECK((direction='input' AND constraint_text='m') OR (direction='output' AND constraint_text='=m') OR (direction='inout' AND constraint_text='+m')),
 CHECK((extent_kind='fixed' AND fixed_element_count IS NOT NULL AND extent_binding_id IS NULL AND capture_proof='observed-asm-place') OR
       (extent_kind='vla-total' AND fixed_element_count IS NULL AND extent_binding_id IS NOT NULL AND capture_proof='observed-asm-vla-total')),
 CHECK((source_shape='vla-array')=(extent_kind='vla-total')),
 CHECK(source_shape<>'object' OR fixed_element_count=1)
) STRICT;
CREATE TABLE memory_barrier_dimensions (
 node_id INTEGER NOT NULL,
 operand_ordinal INTEGER NOT NULL,
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('fixed','vla-total')),
 fixed_count INTEGER CHECK(fixed_count>=0),
 extent_binding_id INTEGER REFERENCES bindings(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 PRIMARY KEY(node_id,operand_ordinal,ordinal),
 FOREIGN KEY(node_id,operand_ordinal) REFERENCES memory_barrier_operands(node_id,ordinal),
 CHECK((kind='fixed' AND fixed_count IS NOT NULL AND extent_binding_id IS NULL) OR
       (kind='vla-total' AND fixed_count IS NULL AND extent_binding_id IS NOT NULL))
) STRICT;

CREATE TABLE content_digest (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 algorithm TEXT NOT NULL CHECK(algorithm='dotobj-semantic-sha256-v1'),
 sha256 TEXT NOT NULL
) STRICT;
CREATE INDEX source_declarations_by_function ON source_declarations(function_entity_id,id);
CREATE INDEX nodes_by_function ON nodes(function_id,id);
CREATE INDEX bindings_by_function ON bindings(function_id,id);
CREATE TABLE exception_types (
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 leaf_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 payload_type_id INTEGER NOT NULL REFERENCES types(id),
 payload_size INTEGER NOT NULL CHECK(payload_size>0),
 payload_alignment INTEGER NOT NULL CHECK(payload_alignment>0),
 source_destructor_entity_id INTEGER REFERENCES source_functions(entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-throw-lowering')
) STRICT;
CREATE TABLE exception_base_conversions (
 exception_type_id INTEGER NOT NULL REFERENCES exception_types(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 base_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 is_public INTEGER NOT NULL CHECK(is_public IN (0,1)),
 is_ambiguous INTEGER NOT NULL CHECK(is_ambiguous IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-complete-base-layout'),
 PRIMARY KEY(exception_type_id,ordinal)
) STRICT;
CREATE TABLE throw_expressions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 exception_type_id INTEGER NOT NULL REFERENCES exception_types(id),
 destructor_entity_id INTEGER REFERENCES entities(id),
 initialization_kind TEXT NOT NULL CHECK(initialization_kind IN ('empty','expression')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-throw-lowering')
) STRICT;
CREATE TABLE throw_initialization_boundaries (
 throw_node_id INTEGER PRIMARY KEY REFERENCES throw_expressions(node_id),
 cleanup_policy TEXT NOT NULL CHECK(cleanup_policy='unwind-to-entry-before-release'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-throw-initializer')
) STRICT;
CREATE TABLE throw_cleanup_transitions (
 node_id INTEGER PRIMARY KEY REFERENCES cleanup_transitions(node_id),
 throw_node_id INTEGER NOT NULL REFERENCES throw_initialization_boundaries(throw_node_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-throw-initializer-transition')
) STRICT;
CREATE TABLE throw_object_addresses (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 throw_node_id INTEGER NOT NULL REFERENCES throw_expressions(node_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-thrown-object-address')
) STRICT;
CREATE TABLE cleanup_payload_targets (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 action_id INTEGER NOT NULL UNIQUE REFERENCES cleanup_actions(id),
 throw_node_id INTEGER NOT NULL REFERENCES throw_initialization_boundaries(throw_node_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-thrown-object-position')
) STRICT;
CREATE TABLE throw_payload_subobjects (
 source_obligation_id INTEGER PRIMARY KEY REFERENCES source_destruction_obligations(id),
 throw_node_id INTEGER NOT NULL REFERENCES throw_initialization_boundaries(throw_node_id),
 completion_policy TEXT NOT NULL CHECK(completion_policy='complete-payload-destructor'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-payload-subobject-init')
) STRICT;
CREATE TABLE throw_payload_paths (
 source_obligation_id INTEGER NOT NULL REFERENCES throw_payload_subobjects(source_obligation_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 field_id INTEGER REFERENCES record_fields(id),
 source_base_id INTEGER REFERENCES source_record_bases(id),
 PRIMARY KEY(source_obligation_id,ordinal),
 CHECK((field_id IS NULL) != (source_base_id IS NULL))
) STRICT;
CREATE TABLE throw_markers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 throw_node_id INTEGER NOT NULL REFERENCES throw_expressions(node_id),
 kind TEXT NOT NULL CHECK(kind='started'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-throw-start')
) STRICT;
CREATE TABLE rethrow_expressions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-rethrow')
) STRICT;
CREATE TABLE source_tries (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-ordinary-try')
) STRICT;
CREATE TABLE source_handlers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 try_node_id INTEGER NOT NULL REFERENCES source_tries(node_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 match_kind TEXT NOT NULL CHECK(match_kind IN ('ellipsis','typed')),
 source_type_id INTEGER REFERENCES source_types(id),
 match_type_id INTEGER REFERENCES source_types(id),
 parameter_binding_id INTEGER UNIQUE REFERENCES bindings(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-source-handler'),
 UNIQUE(try_node_id,ordinal),
 CHECK((match_kind='ellipsis' AND source_type_id IS NULL AND match_type_id IS NULL AND parameter_binding_id IS NULL)
    OR (match_kind='typed' AND source_type_id IS NOT NULL AND match_type_id IS NOT NULL AND parameter_binding_id IS NOT NULL))
) STRICT;
CREATE TABLE handler_markers (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 owner_node_id INTEGER NOT NULL REFERENCES nodes(id),
 kind TEXT NOT NULL CHECK(kind IN ('caught','handler-exit','try-exit')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-handler-marker')
) STRICT;

CREATE TABLE caught_object_addresses (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 handler_node_id INTEGER NOT NULL REFERENCES source_handlers(node_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-caught-object-address')
) STRICT;

COMMIT;
