-- Fresh owned semantic graph. No prior semantic-export schema is used.
PRAGMA application_id = 1146045002; -- DOBJ
PRAGMA user_version = 1;
PRAGMA foreign_keys = ON;
BEGIN IMMEDIATE;
CREATE TABLE object_info (
 singleton INTEGER PRIMARY KEY CHECK(singleton=1),
 format_version INTEGER NOT NULL CHECK(format_version=1),
 profile TEXT NOT NULL CHECK(profile='scalar-control-v64'),
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
-- EDG owns the meaning of this graph. These tables enforce its serialization
-- structure, without resolving source-language recipes or lexical binders.
CREATE TABLE edg_graph_nodes (
 id INTEGER PRIMARY KEY CHECK(id>0),
 entry_kind INTEGER NOT NULL CHECK(entry_kind>=0),
 entry_kind_name TEXT NOT NULL CHECK(length(entry_kind_name)>0),
 capture_phase TEXT NOT NULL CHECK(capture_phase='frontend-observation'),
 UNIQUE(id,entry_kind)
) STRICT;
CREATE TABLE edg_graph_fields (
 node_id INTEGER NOT NULL REFERENCES edg_graph_nodes(id),
 name TEXT NOT NULL CHECK(length(name)>0),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 value_kind TEXT NOT NULL CHECK(value_kind IN ('signed','unsigned','boolean','text','bytes','reference')),
 value TEXT,
 target_id INTEGER,
 target_entry_kind INTEGER CHECK(target_entry_kind>=0),
 PRIMARY KEY(node_id,name,ordinal),
 FOREIGN KEY(target_id,target_entry_kind) REFERENCES edg_graph_nodes(id,entry_kind),
 CHECK((value_kind='reference' AND value IS NULL AND target_entry_kind IS NOT NULL) OR
       (value_kind<>'reference' AND value IS NOT NULL AND target_id IS NULL AND target_entry_kind IS NULL)),
 CHECK(value_kind<>'boolean' OR value IN ('0','1')),
 CHECK(value_kind<>'bytes' OR (length(value)%2=0 AND value NOT GLOB '*[^0-9a-f]*'))
) STRICT;
CREATE TABLE edg_graph_roots (
 node_id INTEGER NOT NULL REFERENCES edg_graph_nodes(id),
 context_node_id INTEGER REFERENCES edg_graph_nodes(id),
 role TEXT NOT NULL CHECK(length(role)>0),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 PRIMARY KEY(role,ordinal)
) STRICT;
CREATE TABLE edg_graph_coverage (
 entry_kind INTEGER NOT NULL CHECK(entry_kind>=0),
 field TEXT NOT NULL CHECK(length(field)>0),
 disposition TEXT NOT NULL CHECK(disposition IN ('retained','frontend-private','gap','configuration')),
 detail TEXT NOT NULL,
 PRIMARY KEY(entry_kind,field)
) STRICT;
CREATE TABLE source_files (
 id INTEGER PRIMARY KEY CHECK(id>0),
 path TEXT NOT NULL,
 parent_id INTEGER REFERENCES source_files(id),
 available INTEGER NOT NULL CHECK(available IN (0,1)),
 sha256 TEXT,
 bytes BLOB,
 codec TEXT CHECK(codec IN ('raw','zlib')),
 decoded_byte_length INTEGER CHECK(decoded_byte_length>=0 AND decoded_byte_length<=2147483647),
 CHECK((available=1 AND sha256 IS NOT NULL AND bytes IS NOT NULL AND codec IS NOT NULL AND decoded_byte_length IS NOT NULL) OR
       (available=0 AND sha256 IS NULL AND bytes IS NULL AND codec IS NULL AND decoded_byte_length IS NULL)),
 CHECK(codec IS NULL OR (codec='raw' AND length(bytes)=decoded_byte_length) OR
       (codec='zlib' AND length(bytes)>=6 AND length(bytes)<decoded_byte_length))
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
 exception_policy TEXT NOT NULL CHECK(exception_policy IN ('unwind','terminate','dynamic'))
) STRICT;
CREATE TABLE routine_parameters (
 type_id INTEGER NOT NULL REFERENCES routine_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 parameter_type_id INTEGER NOT NULL REFERENCES types(id),
 PRIMARY KEY(type_id,ordinal)
) STRICT;
CREATE TABLE source_routine_types (
 declarator_context_count INTEGER NOT NULL CHECK(declarator_context_count>=0),
 has_borrowed_arguments INTEGER NOT NULL CHECK(has_borrowed_arguments IN (0,1)),
 prototype_origin TEXT NOT NULL DEFAULT 'original' CHECK(prototype_origin IN('original','lambda-conversion-copy')),
 exception_policy_origin TEXT NOT NULL CHECK(exception_policy_origin IN ('captured','generated-resolution','implicit-destructor-resolution')),
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 return_type_id INTEGER NOT NULL REFERENCES source_types(id),
 variadic INTEGER NOT NULL CHECK(variadic IN (0,1)),
 exception_policy TEXT NOT NULL CHECK(exception_policy IN ('unwind','terminate','dynamic')),
 parameter_mode TEXT NOT NULL CHECK(parameter_mode IN ('prototype','unspecified')),
 object_type_id INTEGER REFERENCES source_types(id),
 object_is_const INTEGER NOT NULL CHECK(object_is_const IN (0,1)),
 object_is_volatile INTEGER NOT NULL CHECK(object_is_volatile IN (0,1)),
 object_ref_qualifier TEXT NOT NULL CHECK(object_ref_qualifier IN ('none','lvalue','rvalue')),
 CHECK(object_type_id IS NOT NULL OR
       (object_is_const=0 AND object_is_volatile=0 AND object_ref_qualifier='none'))
) STRICT;
CREATE TABLE source_routine_exception_resolutions (
 source_routine_type_id INTEGER PRIMARY KEY REFERENCES source_routine_types(type_id),
 final_source_routine_type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 source_function_entity_id INTEGER REFERENCES source_functions(entity_id),
 initial_policy TEXT NOT NULL CHECK(initial_policy IN ('unwind','terminate','dynamic','unsupported-dynamic')),
 resolved_policy TEXT NOT NULL CHECK(resolved_policy IN ('unwind','terminate','dynamic')),
 initial_present INTEGER NOT NULL CHECK(initial_present IN (0,1)),
 initial_indeterminate INTEGER NOT NULL CHECK(initial_indeterminate IN (0,1)),
 initial_arg_cached INTEGER NOT NULL CHECK(initial_arg_cached IN (0,1)),
 initial_copy_from_prototype INTEGER NOT NULL CHECK(initial_copy_from_prototype IN (0,1)),
 initial_is_noexcept INTEGER NOT NULL CHECK(initial_is_noexcept IN (0,1)),
 initial_throw_any INTEGER NOT NULL CHECK(initial_throw_any IN (0,1)),
 final_present INTEGER NOT NULL CHECK(final_present IN (0,1)),
 final_indeterminate INTEGER NOT NULL CHECK(final_indeterminate IN (0,1)),
 final_arg_cached INTEGER NOT NULL CHECK(final_arg_cached IN (0,1)),
 final_copy_from_prototype INTEGER NOT NULL CHECK(final_copy_from_prototype IN (0,1)),
 final_is_noexcept INTEGER NOT NULL CHECK(final_is_noexcept IN (0,1)),
 final_throw_any INTEGER NOT NULL CHECK(final_throw_any IN (0,1)),
 is_defaulted INTEGER NOT NULL CHECK(is_defaulted IN (0,1)),
 is_compiler_generated INTEGER NOT NULL CHECK(is_compiler_generated IN (0,1)),
 is_inheriting_constructor INTEGER NOT NULL CHECK(is_inheriting_constructor IN (0,1)),
 special_member_kind TEXT NOT NULL CHECK(special_member_kind IN ('constructor','destructor','assignment','equality','inequality','less','greater','less-equal','greater-equal','three-way-comparison')),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('observed-generated-exception-specification','observed-implicit-destructor-exception-specification'))
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
 parameter_role TEXT NOT NULL CHECK(parameter_role IN ('source','indirect-source','this','result-buffer','vtt')),
 source_ordinal INTEGER CHECK(source_ordinal>=0),
 CHECK((parameter_role IN ('source','indirect-source') AND source_ordinal IS NOT NULL) OR
       (parameter_role IN ('this','result-buffer','vtt') AND source_ordinal IS NULL)),
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
 definition TEXT NOT NULL CHECK(definition IN ('declaration','strong','linkonce','weak')),
 return_type_id INTEGER NOT NULL REFERENCES types(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 routine_type_id INTEGER NOT NULL REFERENCES routine_types(type_id),
 is_weak INTEGER NOT NULL CHECK(is_weak IN (0,1)),
 CHECK((is_weak=1 AND linkage='external' AND definition IN ('weak','declaration')) OR
       (is_weak=0 AND definition!='weak')),
 CHECK((linkage='external' AND external_key IS NOT NULL) OR
       (linkage='internal' AND external_key IS NULL))
) STRICT;
CREATE TABLE parameters (
 entity_id INTEGER NOT NULL REFERENCES entities(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 type_id INTEGER NOT NULL REFERENCES types(id),
 parameter_role TEXT NOT NULL CHECK(parameter_role IN ('source','indirect-source','this','result-buffer','vtt','forwarded-source','static-object','initialization-object','entity-destination','entity-count','entity-model')),
 source_ordinal INTEGER CHECK(source_ordinal>=0),
 CHECK((parameter_role IN ('source','indirect-source','forwarded-source') AND source_ordinal IS NOT NULL) OR
       (parameter_role IN ('this','result-buffer','vtt','static-object','initialization-object','entity-destination','entity-count','entity-model') AND source_ordinal IS NULL)),
 UNIQUE(entity_id,source_ordinal),
 PRIMARY KEY(entity_id,ordinal)
) STRICT;
CREATE TABLE abi_routine_origins (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 primary_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 entry_kind TEXT NOT NULL CHECK(entry_kind IN ('complete','subobject','deleting','delegation','constructor-default-adapter')),
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
CREATE TABLE compile_time_selections (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 condition_value INTEGER NOT NULL CHECK(condition_value IN (0,1)),
 has_else INTEGER NOT NULL CHECK(has_else IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-constexpr-selection')
) STRICT;
CREATE TABLE thunk_wrappers (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 target_entity_id INTEGER NOT NULL REFERENCES entities(id),
 overridden_entity_id INTEGER NOT NULL REFERENCES entities(id),
 primary_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 this_delta INTEGER NOT NULL,
 vcall_index INTEGER NOT NULL,
 return_delta INTEGER NOT NULL,
 vbase_index INTEGER NOT NULL,
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-wrapper-creation'),
 CHECK(entity_id<>target_entity_id AND entity_id<>overridden_entity_id AND entity_id<>primary_entity_id)
) STRICT;
CREATE TABLE wrapper_calls (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 wrapper_entity_id INTEGER NOT NULL REFERENCES thunk_wrappers(entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-wrapper-forwarding')
) STRICT;
CREATE TABLE lambda_entry_origins (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 target_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 closure_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 entry_source_routine_type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-lambda-static-entry'),
 CHECK(entity_id<>target_entity_id)
) STRICT;
CREATE TABLE lambda_entry_calls (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 entry_entity_id INTEGER NOT NULL REFERENCES lambda_entry_origins(entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-lambda-entry-forwarding')
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
CREATE TABLE source_initializers (
 id INTEGER PRIMARY KEY CHECK(id>0),
 storage_id INTEGER REFERENCES storages(id),
 source_name TEXT NOT NULL,
 execution_disposition TEXT NOT NULL CHECK(execution_disposition IN ('emitted','stock-not-emitted','source-template')),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_ordinal INTEGER CHECK(source_ordinal>=0),
 source_field_id INTEGER REFERENCES source_record_fields(id),
 kind TEXT NOT NULL CHECK(kind IN ('file','local-static','field-default')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0), column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK((execution_disposition='emitted')=(storage_id IS NOT NULL)),
 CHECK((kind='field-default')=(source_field_id IS NOT NULL)),
 CHECK((kind='field-default')=(execution_disposition='source-template'))
) STRICT;
CREATE TABLE source_field_initializer_destructions (
 source_initializer_id INTEGER PRIMARY KEY REFERENCES source_initializers(id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 selected_routine_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering')
) STRICT;
CREATE TABLE field_initializer_executions (
 source_initializer_id INTEGER NOT NULL REFERENCES source_initializers(id),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 constructor_ordinal INTEGER NOT NULL CHECK(constructor_ordinal>=0),
 execution_disposition TEXT NOT NULL CHECK(execution_disposition IN ('copied','stock-no-effect')),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('observed-field-initializer-copy','observed-no-effect-field-initializer')),
 PRIMARY KEY(source_initializer_id,function_entity_id),
 UNIQUE(function_entity_id,constructor_ordinal)
) STRICT;
CREATE TABLE field_initializer_destruction_elisions (
 source_initializer_id INTEGER NOT NULL REFERENCES source_field_initializer_destructions(source_initializer_id),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 kind TEXT NOT NULL CHECK(kind='stock-no-effect'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-no-effect-destruction-removal'),
 PRIMARY KEY(source_initializer_id,function_entity_id)
) STRICT;
CREATE TABLE field_initializer_partial_destruction_executions (
 source_static_obligation_id INTEGER NOT NULL REFERENCES source_static_destructions(id),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 kind TEXT NOT NULL CHECK(kind IN ('cleanup-action','stock-terminal-no-throw','adopted-automatic')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-field-partial-disposition'),
 PRIMARY KEY(source_static_obligation_id,function_entity_id)
) STRICT;
CREATE TABLE source_static_lifetimes (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_initializer_id INTEGER REFERENCES source_initializers(id),
 parent_lifetime_id INTEGER REFERENCES source_static_lifetimes(id),
 parent_obligation_id INTEGER REFERENCES source_static_destructions(id),
 sibling_ordinal INTEGER CHECK(sibling_ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('global-static','function-static','block','block-after-label','expression-temporary','try-block')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0), column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering')
) STRICT;
CREATE TABLE source_static_destructions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 lifetime_id INTEGER NOT NULL REFERENCES source_static_lifetimes(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 storage_id INTEGER REFERENCES storages(id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 selected_routine_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 source_initializer_id INTEGER REFERENCES source_initializers(id),
 kind TEXT NOT NULL CHECK(kind IN ('persistent','partial-aggregate','automatic','allocation')),
 is_array_allocation INTEGER CHECK(is_array_allocation IN (0,1)),
 domain TEXT NOT NULL CHECK(domain IN ('process','thread')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0), column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK((kind='allocation')=(is_array_allocation IS NOT NULL)),
 UNIQUE(lifetime_id,ordinal)
) STRICT;
CREATE TABLE initializer_lifetime_executions (
 source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 source_static_lifetime_id INTEGER NOT NULL REFERENCES source_static_lifetimes(id),
 source_initializer_id INTEGER NOT NULL REFERENCES source_initializers(id),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-initializer-lifetime-adoption'),
 PRIMARY KEY(source_lifetime_id,function_entity_id)
) STRICT;
CREATE TABLE static_destruction_registrations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_obligation_id INTEGER NOT NULL REFERENCES source_static_destructions(id),
 source_initializer_id INTEGER NOT NULL REFERENCES source_initializers(id),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 callback_entity_id INTEGER NOT NULL REFERENCES entities(id),
 object_node_id INTEGER NOT NULL REFERENCES nodes(id),
 domain TEXT NOT NULL CHECK(domain IN ('process','thread')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-static-destruction-registration')
) STRICT;
CREATE TABLE tls_routine_origins (
 entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 kind TEXT NOT NULL CHECK(kind IN ('variable-wrapper','initializer-alias','empty-initializer')),
 source_storage_id INTEGER REFERENCES storages(id),
 target_entity_id INTEGER REFERENCES entities(id),
 guard_kind TEXT NOT NULL CHECK(guard_kind IN ('none','always','weak-address')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-tls-variable-factory')
) STRICT;
CREATE TABLE generated_array_initializers (
 function_entity_id INTEGER PRIMARY KEY REFERENCES entities(id),
 owner_function_entity_id INTEGER NOT NULL REFERENCES entities(id),
 source_allocation_obligation_id INTEGER NOT NULL REFERENCES source_allocation_obligations(id),
 source_element_type_id INTEGER NOT NULL REFERENCES source_types(id),
 element_type_id INTEGER NOT NULL REFERENCES types(id),
 constructor_entity_id INTEGER REFERENCES entities(id),
 object_parameter_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 model_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 model_initialization_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 model_copy_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 constructor_call_node_id INTEGER UNIQUE REFERENCES nodes(id),
 model_lowered_init_kind INTEGER NOT NULL CHECK(model_lowered_init_kind BETWEEN 0 AND 5),
 model_site_kind TEXT NOT NULL CHECK(model_site_kind IN ('scope-entry','dynamic-init','lowered-assignment')),
 model_zero_storage INTEGER NOT NULL CHECK(model_zero_storage IN (0,1)),
 model_partially_initialized INTEGER NOT NULL CHECK(model_partially_initialized IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-array-zero-initializer-factory'),
 CHECK((constructor_entity_id IS NULL)=(constructor_call_node_id IS NULL)),
 CHECK(object_parameter_binding_id<>model_binding_id)
) STRICT;
CREATE TABLE generated_static_destructors (
 function_entity_id INTEGER PRIMARY KEY REFERENCES functions(entity_id),
 source_obligation_id INTEGER NOT NULL REFERENCES source_static_destructions(id),
 parameter_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 storage_id INTEGER NOT NULL REFERENCES storages(id),
 element_type_id INTEGER REFERENCES types(id),
 element_count INTEGER CHECK(element_count>0),
 element_size INTEGER CHECK(element_size>0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-static-destructor-body'),
 CHECK((element_type_id IS NULL)=(element_count IS NULL) AND (element_count IS NULL)=(element_size IS NULL))
) STRICT;
CREATE TABLE static_destruction_dispositions (
 source_obligation_id INTEGER PRIMARY KEY REFERENCES source_static_destructions(id),
 kind TEXT NOT NULL CHECK(kind IN ('registered','stock-proven-no-effect','stock-not-emitted','cleanup-action','stock-terminal-no-throw','source-template','adopted-automatic')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-static-destruction-disposition')
) STRICT;
CREATE TABLE source_lifetimes (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_initializer_id INTEGER REFERENCES source_initializers(id),
 parent_lifetime_id INTEGER REFERENCES source_lifetimes(id),
 sibling_ordinal INTEGER NOT NULL CHECK(sibling_ordinal>=0),
 parent_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 parent_allocation_obligation_id INTEGER REFERENCES source_allocation_obligations(id),
 kind TEXT NOT NULL CHECK(kind IN ('block','block-after-label','expression-temporary','try-block')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK((function_entity_id IS NULL)!=(source_initializer_id IS NULL)),
 CHECK(parent_obligation_id IS NULL OR parent_allocation_obligation_id IS NULL),
 UNIQUE(function_entity_id,parent_lifetime_id,sibling_ordinal),
 UNIQUE(source_initializer_id,parent_lifetime_id,sibling_ordinal)
) STRICT;
CREATE TABLE source_destruction_obligations (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_initializer_id INTEGER REFERENCES source_initializers(id),
 source_static_obligation_id INTEGER REFERENCES source_static_destructions(id),
 lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 destructor_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 is_partial_aggregate INTEGER NOT NULL CHECK(is_partial_aggregate IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK((function_entity_id IS NULL)!=(source_initializer_id IS NULL)),
 UNIQUE(lifetime_id,ordinal)
) STRICT;
CREATE TABLE destruction_elisions (
 source_obligation_id INTEGER NOT NULL REFERENCES source_destruction_obligations(id),
 function_entity_id INTEGER NOT NULL REFERENCES entities(id),
 kind TEXT NOT NULL CHECK(kind IN ('terminal-no-throw','stock-no-effect')),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('observed-terminal-partial-elision','observed-no-effect-destruction-removal')),
 PRIMARY KEY(source_obligation_id,function_entity_id)
) STRICT;
CREATE TABLE source_array_deletions (
 source_obligation_id INTEGER PRIMARY KEY REFERENCES source_destruction_obligations(id),
 deallocator_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-array-delete-lowering')
) STRICT;
CREATE TABLE array_deletion_operations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_obligation_id INTEGER NOT NULL REFERENCES source_array_deletions(source_obligation_id),
 destructor_entity_id INTEGER NOT NULL REFERENCES entities(id),
 deallocator_entity_id INTEGER NOT NULL REFERENCES entities(id),
 element_type_id INTEGER NOT NULL REFERENCES types(id),
 padding_size INTEGER NOT NULL CHECK(padding_size>=0),
 helper_kind TEXT NOT NULL CHECK(helper_kind IN ('ia64-vector-delete2','ia64-vector-delete3')),
 callback_kind TEXT NOT NULL CHECK(callback_kind IN ('direct','virtual-cookie-guarded')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-array-delete-runtime')
) STRICT;
CREATE TABLE array_deletion_virtual_callbacks (
 call_node_id INTEGER PRIMARY KEY REFERENCES array_deletion_operations(node_id),
 callback_node_id INTEGER NOT NULL REFERENCES nodes(id),
 vtable_index INTEGER NOT NULL CHECK(vtable_index>=0),
 cookie_size INTEGER NOT NULL CHECK(cookie_size>0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-array-delete-virtual-dispatch')
) STRICT;
CREATE TABLE source_array_allocation_elements (
 source_allocation_obligation_id INTEGER PRIMARY KEY REFERENCES source_allocation_obligations(id),
 source_destruction_obligation_id INTEGER NOT NULL UNIQUE REFERENCES source_destruction_obligations(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering')
) STRICT;
CREATE TABLE array_allocation_operations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_allocation_obligation_id INTEGER NOT NULL REFERENCES source_allocation_obligations(id),
 source_destruction_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 allocator_entity_id INTEGER NOT NULL REFERENCES entities(id),
 deallocator_entity_id INTEGER NOT NULL REFERENCES entities(id),
 constructor_entity_id INTEGER REFERENCES entities(id),
 destructor_entity_id INTEGER REFERENCES entities(id),
 element_type_id INTEGER NOT NULL REFERENCES types(id),
 padding_size INTEGER NOT NULL CHECK(padding_size>=0),
 helper_kind TEXT NOT NULL CHECK(helper_kind IN ('ia64-vector-new2','ia64-vector-new3')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-array-new-runtime')
) STRICT;
CREATE TABLE source_uninitialized_allocations (
 source_allocation_obligation_id INTEGER PRIMARY KEY REFERENCES source_allocation_obligations(id),
 allocator_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 is_placement INTEGER NOT NULL CHECK(is_placement IN (0,1)),
 has_initializer INTEGER NOT NULL CHECK(has_initializer IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-new-without-initialization')
) STRICT;
CREATE TABLE allocation_elisions (
 source_allocation_obligation_id INTEGER NOT NULL REFERENCES source_allocation_obligations(id),
 function_entity_id INTEGER NOT NULL REFERENCES entities(id),
 kind TEXT NOT NULL CHECK(kind IN ('empty-initialization','no-initialization','dynamic-array-zero-initialization')),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('observed-empty-allocation-initialization','original-new-without-initialization','observed-dynamic-array-zero-initialization')),
 PRIMARY KEY(source_allocation_obligation_id,function_entity_id)
) STRICT;
CREATE TABLE source_allocation_obligations (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_initializer_id INTEGER REFERENCES source_initializers(id),
 source_static_obligation_id INTEGER REFERENCES source_static_destructions(id),
 lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 deallocator_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 allocated_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 is_array INTEGER NOT NULL CHECK(is_array IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK((function_entity_id IS NULL)!=(source_initializer_id IS NULL)),
 UNIQUE(lifetime_id,ordinal)
) STRICT;
CREATE TABLE internal_try_expressions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_allocation_obligation_id INTEGER REFERENCES source_allocation_obligations(id),
 cleanup_policy TEXT NOT NULL CHECK(cleanup_policy='unwind-to-entry-before-handler'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-internal-try')
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
CREATE TABLE virtual_base_expressions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 complete_base_type_id INTEGER NOT NULL REFERENCES record_types(type_id)
) STRICT;
CREATE TABLE virtual_base_subobjects (
 record_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 complete_base_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 physical_view_type_id INTEGER REFERENCES record_types(type_id),
 byte_offset INTEGER NOT NULL CHECK(byte_offset>=0),
 extent INTEGER NOT NULL CHECK(extent>=0),
 field_id INTEGER UNIQUE REFERENCES record_fields(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-virtual-base-placement'),
 PRIMARY KEY(record_type_id,complete_base_type_id)
) STRICT;
CREATE TABLE normal_destructions (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 source_static_obligation_id INTEGER REFERENCES source_static_destructions(id),
 source_field_initializer_id INTEGER REFERENCES source_field_initializer_destructions(source_initializer_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-normal-destruction'),
 CHECK((source_obligation_id IS NOT NULL)+(source_static_obligation_id IS NOT NULL)+(source_field_initializer_id IS NOT NULL)=1)
) STRICT;
CREATE TABLE cleanup_actions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 source_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 source_static_obligation_id INTEGER REFERENCES source_static_destructions(id),
 source_field_initializer_id INTEGER REFERENCES source_field_initializer_destructions(source_initializer_id),
 callback_entity_id INTEGER NOT NULL REFERENCES entities(id),
 target_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-region-entry'),
 CHECK((source_obligation_id IS NOT NULL)+(source_static_obligation_id IS NOT NULL)+(source_field_initializer_id IS NOT NULL)=1)
) STRICT;
CREATE TABLE array_runtime_operations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 source_static_obligation_id INTEGER REFERENCES source_static_destructions(id),
 source_field_initializer_id INTEGER REFERENCES source_field_initializer_destructions(source_initializer_id),
 callback_entity_id INTEGER NOT NULL REFERENCES entities(id),
 operation_kind TEXT NOT NULL CHECK(operation_kind IN ('construct','destroy')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-array-runtime'),
 CHECK((source_obligation_id IS NOT NULL)+(source_static_obligation_id IS NOT NULL)+(source_field_initializer_id IS NOT NULL)=1)
) STRICT;
CREATE TABLE cleanup_arrays (
 action_id INTEGER PRIMARY KEY REFERENCES cleanup_actions(id),
 element_type_id INTEGER NOT NULL REFERENCES types(id),
 element_count INTEGER NOT NULL CHECK(element_count>0),
 element_size INTEGER NOT NULL CHECK(element_size>0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-array-region')
) STRICT;
CREATE TABLE cleanup_vtt_arguments (
 action_id INTEGER PRIMARY KEY REFERENCES cleanup_actions(id),
 binding_id INTEGER NOT NULL REFERENCES bindings(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-subobject-vtt')
) STRICT;
CREATE TABLE cleanup_conditions (
 action_id INTEGER PRIMARY KEY REFERENCES cleanup_actions(id),
 flag_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-conditional-cleanup-flag')
) STRICT;
CREATE TABLE allocation_cleanup_actions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 source_allocation_obligation_id INTEGER NOT NULL REFERENCES source_allocation_obligations(id),
 callback_entity_id INTEGER NOT NULL REFERENCES entities(id),
 target_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 allocation_size INTEGER CHECK(allocation_size>0),
 condition_binding_id INTEGER REFERENCES bindings(id),
 null_policy TEXT NOT NULL CHECK(null_policy='skip-null'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-allocation-region')
) STRICT;
CREATE TABLE local_static_guards (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES entities(id),
 storage_id INTEGER NOT NULL UNIQUE REFERENCES storages(id),
 guard_storage_id INTEGER NOT NULL UNIQUE REFERENCES storages(id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 guard_policy TEXT NOT NULL CHECK(guard_policy IN ('ia64-acquire-release','thread-local-byte')),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0), column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-local-static-guard')
) STRICT;
CREATE TABLE guard_operations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 guard_id INTEGER NOT NULL REFERENCES local_static_guards(id),
 kind TEXT NOT NULL CHECK(kind IN ('probe','acquire','release','thread-publish')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-local-static-guard-operation')
) STRICT;
CREATE TABLE guard_cleanup_actions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 guard_id INTEGER NOT NULL REFERENCES local_static_guards(id),
 target_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-guard-region')
) STRICT;
CREATE TABLE cleanup_states (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 action_id INTEGER UNIQUE REFERENCES cleanup_actions(id),
 next_state_id INTEGER REFERENCES cleanup_states(id),
 handler_node_id INTEGER UNIQUE REFERENCES source_handlers(node_id),
 allocation_action_id INTEGER UNIQUE REFERENCES allocation_cleanup_actions(id),
 guard_action_id INTEGER UNIQUE REFERENCES guard_cleanup_actions(id),
 CHECK((action_id IS NOT NULL)+(handler_node_id IS NOT NULL)+(allocation_action_id IS NOT NULL)+(guard_action_id IS NOT NULL)=1)
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
 source_initializer_id INTEGER NOT NULL REFERENCES source_initializers(id),
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
 binding_id INTEGER REFERENCES bindings(id),
 kind TEXT NOT NULL CHECK(kind IN ('declaration','zero-initialization','cleanup-frame')),
 capture_proof TEXT NOT NULL,
 CHECK((kind='cleanup-frame' AND binding_id IS NULL AND capture_proof='observed-function-cleanup-frame') OR
       (kind!='cleanup-frame' AND binding_id IS NOT NULL AND capture_proof='observed-scope-binding')),
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
 has_namespace_owner INTEGER NOT NULL CHECK(has_namespace_owner IN(0,1)),
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_form TEXT NOT NULL DEFAULT 'plain' CHECK(source_form IN('plain','template-alias','type-operator')),
 kind TEXT NOT NULL,
 source_name TEXT NOT NULL,
 underlying_type_id INTEGER REFERENCES source_types(id),
 execution_type_id INTEGER REFERENCES types(id),
 is_const INTEGER NOT NULL CHECK(is_const IN (0,1)),
 is_volatile INTEGER NOT NULL CHECK(is_volatile IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 element_count INTEGER CHECK(element_count>=0),
 is_restrict INTEGER NOT NULL CHECK(is_restrict IN (0,1)),
 is_atomic INTEGER NOT NULL CHECK(is_atomic IN (0,1)),
 array_bound_kind TEXT CHECK(array_bound_kind IN ('constant','unspecified','dependent')),
 CHECK((kind='array' AND array_bound_kind IS NOT NULL AND
        ((array_bound_kind='constant' AND element_count IS NOT NULL) OR (array_bound_kind<>'constant' AND element_count IS NULL))) OR
       (kind<>'array' AND array_bound_kind IS NULL))
) STRICT;
CREATE TABLE source_type_qualifiers (
 type_id INTEGER NOT NULL REFERENCES source_types(id),
 name TEXT NOT NULL,
 PRIMARY KEY(type_id,name)
) STRICT;
CREATE TABLE source_nested_type_declarations (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 owner_type_id INTEGER NOT NULL REFERENCES source_types(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-nested-declaration'),
 CHECK(type_id<>owner_type_id)
) STRICT;
CREATE TABLE source_dependent_types (
 has_type_constraint INTEGER NOT NULL CHECK(has_type_constraint IN(0,1)),
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 kind TEXT NOT NULL CHECK(kind IN ('parameter','member','unknown','auto-placeholder')),
 owner_type_id INTEGER REFERENCES source_types(id),
 original_nested_type_id INTEGER REFERENCES source_nested_type_declarations(type_id),
 nesting_depth INTEGER CHECK(nesting_depth>=-1),
 parameter_position INTEGER CHECK(parameter_position>0),
 is_pack INTEGER NOT NULL CHECK(is_pack IN (0,1)),
 identity_key TEXT NOT NULL,
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK(has_type_constraint=0 OR kind IN ('parameter','auto-placeholder')),
 CHECK(kind='member' OR original_nested_type_id IS NULL),
 CHECK((kind='auto-placeholder' AND owner_type_id IS NULL AND nesting_depth=-1 AND parameter_position IN(1,2) AND is_pack=0 AND identity_key='') OR
       (kind='parameter' AND owner_type_id IS NULL AND nesting_depth IS NOT NULL AND nesting_depth>=0 AND parameter_position IS NOT NULL AND identity_key!='') OR
       (kind='member' AND owner_type_id IS NOT NULL AND nesting_depth IS NULL AND parameter_position IS NULL AND is_pack=0 AND identity_key='') OR
       (kind='unknown' AND owner_type_id IS NULL AND nesting_depth IS NULL AND parameter_position IS NULL AND is_pack=0 AND identity_key=''))
) STRICT;
CREATE TABLE source_enum_types (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 identity_key TEXT NOT NULL,
 underlying_type_id INTEGER NOT NULL REFERENCES source_types(id),
 is_scoped INTEGER NOT NULL CHECK(is_scoped IN (0,1)),
 is_complete INTEGER NOT NULL CHECK(is_complete IN (0,1)),
 enumerator_list_complete INTEGER NOT NULL CHECK(enumerator_list_complete IN (0,1)),
 has_explicit_base INTEGER NOT NULL CHECK(has_explicit_base IN (0,1)),
 is_template INTEGER NOT NULL CHECK(is_template IN (0,1)),
 owner_type_id INTEGER REFERENCES source_types(id),
 source_occurrence INTEGER NOT NULL CHECK(source_occurrence>=0),
 enumerator_count INTEGER NOT NULL CHECK(enumerator_count>=0),
 is_nonreal INTEGER NOT NULL CHECK(is_nonreal IN(0,1)),
 is_prototype INTEGER NOT NULL CHECK(is_prototype IN(0,1)),
 is_specialized INTEGER NOT NULL CHECK(is_specialized IN(0,1)),
 originally_unnamed INTEGER NOT NULL CHECK(originally_unnamed IN(0,1)),
 underlying_origin TEXT NOT NULL CHECK(underlying_origin IN('declared','inferred','prototype-placeholder'))
) STRICT;
CREATE TABLE source_enumerators (
 constant_id INTEGER REFERENCES source_semantic_constants(id),
 type_id INTEGER NOT NULL REFERENCES source_enum_types(type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_name TEXT NOT NULL,
 value_text TEXT NOT NULL,
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((constant_id IS NULL AND value_text!='') OR (constant_id IS NOT NULL AND value_text='')),
 PRIMARY KEY(type_id,ordinal)
) STRICT;
CREATE TABLE source_member_pointer_types (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 owner_type_id INTEGER NOT NULL REFERENCES source_types(id),
 member_type_id INTEGER NOT NULL REFERENCES source_types(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN ('observed-source-type','observed-member-pointer-lowering'))
) STRICT;
CREATE TABLE source_record_owners (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_types(id),
 owner_source_record_id INTEGER NOT NULL REFERENCES source_record_types(id),
 owner_field_id INTEGER NOT NULL REFERENCES source_record_fields(id),
 kind TEXT NOT NULL CHECK(kind='anonymous-member'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-member-declaration'),
 UNIQUE(owner_source_record_id,owner_field_id),
 CHECK(source_record_id<>owner_source_record_id)
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
 layout_known INTEGER NOT NULL CHECK(layout_known IN (0,1)),
 is_dependent INTEGER NOT NULL CHECK(is_dependent IN (0,1)),
 is_prototype_instantiation INTEGER NOT NULL CHECK(is_prototype_instantiation IN (0,1)),
 CHECK(is_complete=1 OR (layout_known=0 AND is_trivially_copyable IS NULL)),
 CHECK((layout_known=1 AND is_complete=1 AND is_dependent=0 AND native_alignment>0) OR
       (layout_known=0 AND native_size=0 AND native_alignment=0)),
 CHECK(is_trivially_copyable IS NULL OR layout_known=1),
 CHECK(is_dependent=0 OR layout_known=0),
 CHECK(is_prototype_instantiation=0 OR is_dependent=1)
) STRICT;
-- Source-only template arguments and dependent extents never imply storage.
CREATE TABLE source_semantic_constants (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL CHECK(kind IN ('integer','parameter','operation','expression','member','type-operand','builtin-trait','sizeof','alignof','datasizeof','source-expression','aggregate','sizeof-pack')),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_name TEXT NOT NULL,
 value_text TEXT NOT NULL,
 parameter_depth INTEGER,
 parameter_position INTEGER,
 is_pack INTEGER NOT NULL CHECK(is_pack IN (0,1)),
 operator_name TEXT NOT NULL,
 owner_source_type_id INTEGER REFERENCES source_types(id),
 operand_source_type_id INTEGER REFERENCES source_types(id),
 builtin_operator_kind INTEGER CHECK(builtin_operator_kind>=0),
 is_standard_alignof INTEGER NOT NULL CHECK(is_standard_alignof IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((kind='parameter' AND parameter_depth>=0 AND parameter_position>0 AND source_file_id IS NOT NULL AND line>0) OR
       (kind<>'parameter' AND parameter_depth IS NULL AND parameter_position IS NULL AND is_pack=0)),
 CHECK((kind='integer' AND length(value_text)>0) OR (kind<>'integer' AND value_text='')),
 CHECK((kind IN ('operation','builtin-trait') AND length(operator_name)>0) OR (kind NOT IN ('operation','builtin-trait') AND operator_name='')),
 CHECK((kind='builtin-trait')=(builtin_operator_kind IS NOT NULL)),
 CHECK(operand_source_type_id IS NULL OR kind IN ('type-operand','sizeof','alignof','datasizeof')),
 CHECK(kind='alignof' OR is_standard_alignof=0),
 CHECK((kind='member' AND owner_source_type_id IS NOT NULL) OR (kind<>'member' AND owner_source_type_id IS NULL))
) STRICT;
CREATE TABLE source_semantic_constant_operands (
 constant_id INTEGER NOT NULL REFERENCES source_semantic_constants(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 operand_id INTEGER NOT NULL REFERENCES source_semantic_constants(id),
 PRIMARY KEY(constant_id,ordinal)
) STRICT;
CREATE TABLE source_array_bounds (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),
 constant_id INTEGER NOT NULL REFERENCES source_semantic_constants(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-dependent-bound')
) STRICT;
CREATE TABLE source_templates (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL CHECK(kind IN('parameter','alias')),
 source_name TEXT NOT NULL,
 parameter_depth INTEGER CHECK(parameter_depth>=0),
 parameter_position INTEGER CHECK(parameter_position>0),
 is_pack INTEGER NOT NULL CHECK(is_pack IN (0,1)),
 parameter_count INTEGER NOT NULL CHECK(parameter_count>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN('original-template-parameter','original-alias-template')),
 source_file_id INTEGER NOT NULL REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>0),
 column INTEGER NOT NULL CHECK(column>=0),
 CHECK((kind='parameter' AND parameter_depth IS NOT NULL AND parameter_position IS NOT NULL AND capture_proof='original-template-parameter') OR (kind='alias' AND parameter_depth IS NULL AND parameter_position IS NULL AND is_pack=0 AND capture_proof='original-alias-template'))
) STRICT;
CREATE TABLE source_template_parameters (
 source_template_id INTEGER NOT NULL REFERENCES source_templates(id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template')),
 source_name TEXT NOT NULL,
 type_id INTEGER REFERENCES source_types(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 default_type_id INTEGER REFERENCES source_types(id),
 default_constant_id INTEGER REFERENCES source_semantic_constants(id),
 default_template_id INTEGER REFERENCES source_templates(id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN (0,1)),
 is_abbreviated INTEGER NOT NULL CHECK(is_abbreviated IN (0,1)),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 PRIMARY KEY(source_template_id,ordinal),
 CHECK((kind='type' AND type_id IS NOT NULL AND constant_id IS NULL AND template_id IS NULL AND default_constant_id IS NULL AND default_template_id IS NULL) OR
       (kind='constant' AND type_id IS NULL AND constant_id IS NOT NULL AND template_id IS NULL AND default_type_id IS NULL AND default_template_id IS NULL) OR
       (kind='template' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NOT NULL AND default_type_id IS NULL AND default_constant_id IS NULL)),
 CHECK(is_pack=0 OR (default_type_id IS NULL AND default_constant_id IS NULL AND default_template_id IS NULL))
) STRICT;
CREATE TABLE source_record_instantiations (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_types(id),
 prototype_source_record_id INTEGER REFERENCES source_record_types(id),
 resolution_kind TEXT NOT NULL CHECK(resolution_kind IN ('class-template','dependent-member-template','template-parameter')),
 source_template_id INTEGER REFERENCES source_templates(id),
 member_owner_source_type_id INTEGER REFERENCES source_types(id),
 member_of_unknown_base INTEGER NOT NULL CHECK(member_of_unknown_base IN (0,1)),
 qualified_unknown_base_member INTEGER NOT NULL CHECK(qualified_unknown_base_member IN (0,1)),
 has_selected_partial_specialization INTEGER NOT NULL CHECK(has_selected_partial_specialization IN(0,1)),
 capture_proof TEXT NOT NULL,
 CHECK((resolution_kind='class-template' AND prototype_source_record_id IS NOT NULL AND source_template_id IS NULL AND member_owner_source_type_id IS NULL AND
        member_of_unknown_base=0 AND qualified_unknown_base_member=0 AND capture_proof='original-template-arguments') OR
       (resolution_kind='dependent-member-template' AND prototype_source_record_id IS NULL AND source_template_id IS NULL AND member_owner_source_type_id IS NOT NULL AND
        capture_proof='original-nonreal-member-template') OR
       (resolution_kind='template-parameter' AND prototype_source_record_id IS NULL AND source_template_id IS NOT NULL AND member_owner_source_type_id IS NULL AND
        member_of_unknown_base=0 AND qualified_unknown_base_member=0 AND capture_proof='original-template-parameter-instantiation')),
 CHECK(source_record_id<>prototype_source_record_id),
 CHECK(qualified_unknown_base_member=0 OR member_of_unknown_base=1)
) STRICT;
CREATE TABLE source_template_arguments (
 source_record_id INTEGER NOT NULL REFERENCES source_record_instantiations(source_record_id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template','pack-boundary')),
 type_id INTEGER REFERENCES source_types(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 PRIMARY KEY(source_record_id,ordinal),
 CHECK((kind='type' AND type_id IS NOT NULL AND constant_id IS NULL AND template_id IS NULL) OR
       (kind='constant' AND type_id IS NULL AND constant_id IS NOT NULL AND template_id IS NULL) OR
       (kind='template' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NOT NULL) OR
       (kind='pack-boundary' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NULL))
) STRICT;
CREATE TABLE source_record_operations (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_types(id),
 bitwise_construction_allowed INTEGER NOT NULL CHECK(bitwise_construction_allowed IN (0,1)),
 bitwise_assignment_allowed INTEGER NOT NULL CHECK(bitwise_assignment_allowed IN (0,1)),
 requires_return_storage INTEGER NOT NULL CHECK(requires_return_storage IN (0,1)),
 requires_parameter_storage INTEGER NOT NULL CHECK(requires_parameter_storage IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-class-prelowering')
) STRICT;
CREATE TABLE virtual_base_adjustments (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 operand_node_id INTEGER NOT NULL REFERENCES nodes(id),
 source_node_id INTEGER NOT NULL REFERENCES nodes(id),
 lookup_source_node_id INTEGER NOT NULL REFERENCES nodes(id),
 derived_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 complete_base_type_id INTEGER NOT NULL REFERENCES record_types(type_id),
 offset_node_id INTEGER NOT NULL REFERENCES nodes(id),
 vtable_index INTEGER NOT NULL,
 is_complete_object INTEGER NOT NULL CHECK(is_complete_object=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-virtual-base-adjustment')
) STRICT;
CREATE TABLE record_storage_initializations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 binding_id INTEGER NOT NULL REFERENCES bindings(id),
 source_declaration_id INTEGER NOT NULL REFERENCES source_declarations(id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_init_kind INTEGER NOT NULL CHECK(source_init_kind BETWEEN 0 AND 5),
 lowered_init_kind INTEGER NOT NULL CHECK(lowered_init_kind BETWEEN 0 AND 5),
 site_kind TEXT NOT NULL CHECK(site_kind IN ('scope-entry','dynamic-init','lowered-assignment')),
 zero_storage INTEGER NOT NULL CHECK(zero_storage IN (0,1)),
 partially_initialized INTEGER NOT NULL CHECK(partially_initialized IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-storage-initialization'),
 CHECK(zero_storage=(lowered_init_kind=3 OR (lowered_init_kind=2 AND partially_initialized=1))),
 CHECK(partially_initialized=0 OR lowered_init_kind=2),
 CHECK(site_kind<>'scope-entry' OR lowered_init_kind=3)
) STRICT;
CREATE TABLE source_record_temporaries (
 id INTEGER PRIMARY KEY CHECK(id>0),
 function_entity_id INTEGER REFERENCES source_functions(entity_id),
 source_initializer_id INTEGER REFERENCES source_initializers(id),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_ordinal INTEGER NOT NULL CHECK(source_ordinal>=0),
 dynamic_init_kind INTEGER NOT NULL CHECK(dynamic_init_kind BETWEEN 0 AND 8),
 source_destruction_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),
 column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='before-lowering'),
 CHECK((function_entity_id IS NULL)<>(source_initializer_id IS NULL))
) STRICT;
CREATE TABLE record_temporary_initializations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 binding_id INTEGER NOT NULL REFERENCES bindings(id),
 source_temporary_id INTEGER NOT NULL REFERENCES source_record_temporaries(id),
 lowered_init_kind INTEGER NOT NULL CHECK(lowered_init_kind BETWEEN 0 AND 5),
 site_kind TEXT NOT NULL CHECK(site_kind IN ('scope-entry','dynamic-init','lowered-assignment')),
 zero_storage INTEGER NOT NULL CHECK(zero_storage IN (0,1)),
 partially_initialized INTEGER NOT NULL CHECK(partially_initialized IN (0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-temporary-storage-initialization'),
 CHECK(zero_storage=(lowered_init_kind=3 OR (lowered_init_kind=2 AND partially_initialized=1))),
 CHECK(partially_initialized=0 OR lowered_init_kind=2),
 CHECK(site_kind<>'scope-entry' OR lowered_init_kind=3)
) STRICT;
CREATE TABLE record_constant_initializations (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 source_record_id INTEGER NOT NULL REFERENCES source_record_types(id),
 storage_id INTEGER NOT NULL REFERENCES storages(id),
 initialization_kind TEXT NOT NULL CHECK(initialization_kind IN ('zero','constant')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-pooled-record-initialization')
) STRICT;
CREATE TABLE record_storage_copies (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 target_node_id INTEGER NOT NULL REFERENCES nodes(id),
 value_node_id INTEGER NOT NULL REFERENCES nodes(id),
 source_record_id INTEGER NOT NULL REFERENCES source_record_types(id),
 byte_count INTEGER NOT NULL CHECK(byte_count>0),
 kind TEXT NOT NULL CHECK(kind IN ('evaluation-save','class-assignment','initializer-storage','bitwise-initialization','array-zero-model','entity-model')),
 capture_proof TEXT NOT NULL,
 CHECK((kind='evaluation-save' AND capture_proof='observed-pre-execution-save') OR
       (kind='class-assignment' AND capture_proof='observed-class-assignment-lowering') OR
       (kind='initializer-storage' AND capture_proof='observed-initialization-storage') OR
       (kind='bitwise-initialization' AND capture_proof='observed-bitwise-initialization') OR
       (kind='array-zero-model' AND capture_proof='observed-array-zero-model-copy') OR
       (kind='entity-model' AND capture_proof='observed-entity-model-copy'))
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
 alignment INTEGER NOT NULL CHECK(alignment IN (0,1,2,4,8,16,32,64)),
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
 field_role TEXT NOT NULL CHECK(field_role IN ('source','vptr','abi','abi-code-address','base','virtual-base')),
 source_base_id INTEGER REFERENCES source_record_bases(id),
 observed_alignment INTEGER CHECK(observed_alignment>0 AND (observed_alignment & (observed_alignment-1))=0),
 CHECK(field_role<>'source' OR bit_width IS NOT NULL OR observed_alignment IS NOT NULL),
 CHECK(bit_width IS NULL OR observed_alignment IS NULL),
 CHECK((field_role='source' AND source_field_id IS NOT NULL AND source_type_id IS NOT NULL) OR
       (field_role IN ('vptr','abi','abi-code-address','base','virtual-base') AND source_field_id IS NULL AND source_type_id IS NULL)),
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
 constraint_text TEXT NOT NULL CHECK(constraint_text IN ('m','rm','mr','=m','+m')),
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
 CHECK((direction='input' AND constraint_text IN ('m','rm','mr')) OR (direction='output' AND constraint_text='=m') OR (direction='inout' AND constraint_text='+m')),
 CHECK((extent_kind='fixed' AND fixed_element_count IS NOT NULL AND extent_binding_id IS NULL AND capture_proof='observed-asm-place') OR
       (extent_kind='vla-total' AND fixed_element_count IS NULL AND extent_binding_id IS NOT NULL AND capture_proof='observed-asm-vla-total')),
 CHECK(constraint_text NOT IN ('rm','mr') OR source_shape='object'),
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
 try_kind TEXT NOT NULL CHECK(try_kind IN ('ordinary','constructor','destructor')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-source-try')
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

CREATE TABLE deferred_function_declarations (
 entity_id INTEGER PRIMARY KEY REFERENCES source_functions(entity_id),
 source_routine_type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-incomplete-declaration')
) STRICT;

CREATE TABLE deferred_routine_abis (
 routine_type_id INTEGER NOT NULL REFERENCES routine_types(type_id),
 source_routine_type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 reason TEXT NOT NULL CHECK(reason='incomplete-value-type'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-incomplete-routine-lowering'),
 PRIMARY KEY(routine_type_id,source_routine_type_id)
) STRICT;

-- Owned source-only constraint graph; staged for the next profile.
CREATE TABLE source_concepts (
 has_namespace_owner INTEGER NOT NULL CHECK(has_namespace_owner IN(0,1)),
 id INTEGER PRIMARY KEY CHECK(id>0),
 source_name TEXT NOT NULL CHECK(length(source_name)>0),
 qualified_name TEXT NOT NULL CHECK(length(qualified_name)>0),
 parameter_count INTEGER NOT NULL CHECK(parameter_count>0),
 definition_expression_id INTEGER NOT NULL REFERENCES source_expressions(id),
 header_constraint_expression_id INTEGER REFERENCES source_expressions(id),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-concept-definition')
) STRICT;
CREATE TABLE source_concept_parameters (
 concept_id INTEGER NOT NULL REFERENCES source_concepts(id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template')),
 source_name TEXT NOT NULL,
 type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 default_type_id INTEGER REFERENCES source_types(id),default_constant_id INTEGER REFERENCES source_semantic_constants(id),
 default_template_id INTEGER REFERENCES source_templates(id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),is_abbreviated INTEGER NOT NULL CHECK(is_abbreviated IN(0,1)),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 PRIMARY KEY(concept_id,ordinal)
) STRICT;
CREATE TABLE source_expressions (
 id INTEGER PRIMARY KEY CHECK(id>0),
 kind TEXT NOT NULL CHECK(kind IN('concept-id','requires','compound-requirement','nested-requirement','parameter-reference','variable-reference','routine-reference','dependent-function-reference','type-operand','constant','expression-constant','sizeof','alignof','datasizeof','builtin-operation','operation','temporary-initialization','allocation-expression','braced-initializer-list','fold-expression','field-reference')),
 edge_count INTEGER NOT NULL CHECK(edge_count>=0),
 parameter_count INTEGER NOT NULL CHECK(parameter_count>=0),
 argument_count INTEGER NOT NULL CHECK(argument_count>=0),
 substitution_count INTEGER NOT NULL CHECK(substitution_count>=0),
 original_lvalue_type_id INTEGER REFERENCES source_types(id),
 is_parenthesized INTEGER NOT NULL CHECK(is_parenthesized IN(0,1)),
 is_compiler_generated INTEGER NOT NULL CHECK(is_compiler_generated IN(0,1)),
 is_volatile_fetch INTEGER NOT NULL CHECK(is_volatile_fetch IN(0,1)),
 is_pack_expansion INTEGER NOT NULL CHECK(is_pack_expansion IN(0,1)),
 source_type_id INTEGER REFERENCES source_types(id),
 value_category TEXT NOT NULL CHECK(value_category IN('none','lvalue','xvalue','prvalue')),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-source-expression')
) STRICT;
CREATE TABLE source_expression_edges (
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),role TEXT NOT NULL,ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 child_expression_id INTEGER NOT NULL REFERENCES source_expressions(id),
 PRIMARY KEY(expression_id,role,ordinal)
) STRICT;
CREATE TABLE source_type_constraints (
 source_type_id INTEGER PRIMARY KEY REFERENCES source_dependent_types(type_id),
 constraint_expression_id INTEGER NOT NULL REFERENCES source_expressions(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-type-constraint')
) STRICT;
CREATE TABLE source_concept_uses (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 concept_id INTEGER NOT NULL REFERENCES source_concepts(id),
 is_type_constraint INTEGER NOT NULL CHECK(is_type_constraint IN(0,1))
) STRICT;
CREATE TABLE source_concept_arguments (
 expression_id INTEGER NOT NULL REFERENCES source_concept_uses(expression_id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN('type','constant','template','pack-boundary')),
 source_type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),PRIMARY KEY(expression_id,ordinal)
) STRICT;
CREATE TABLE source_expression_parameters (
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),declared_source_type_id INTEGER REFERENCES source_types(id),
 source_name TEXT NOT NULL,qualifier_bits INTEGER NOT NULL CHECK(qualifier_bits>=0),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 PRIMARY KEY(expression_id,ordinal)
) STRICT;
CREATE TABLE source_expression_parameter_references (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 binder_id INTEGER REFERENCES source_parameter_binders(id),
 parameter_number INTEGER NOT NULL CHECK(parameter_number>=0),
 original_levels_up INTEGER NOT NULL CHECK(original_levels_up>=0)
) STRICT;
CREATE TABLE source_expression_operations (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 opcode INTEGER NOT NULL CHECK(opcode>=0),operation_name TEXT NOT NULL,
 original_type_kind INTEGER CHECK(original_type_kind>=0)
) STRICT;
CREATE TABLE source_expression_flags (
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),flag TEXT NOT NULL,
 PRIMARY KEY(expression_id,flag)
) STRICT;
CREATE TABLE source_expression_type_operands (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),source_type_id INTEGER NOT NULL REFERENCES source_types(id)
) STRICT;
CREATE TABLE source_expression_constants (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),constant_id INTEGER NOT NULL REFERENCES source_semantic_constants(id)
) STRICT;
-- Exact delayed substitution maps and semantic-only declaration references.

CREATE TABLE source_expression_bindings (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 lexical_binder_id INTEGER NOT NULL REFERENCES source_parameter_binders(id)
) STRICT;
CREATE TABLE source_expression_requirements (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),requires_noexcept INTEGER NOT NULL CHECK(requires_noexcept IN(0,1))
) STRICT;
CREATE TABLE source_expression_queries (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),is_standard_alignof INTEGER NOT NULL CHECK(is_standard_alignof IN(0,1))
) STRICT;
CREATE TABLE source_expression_substitutions (
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),substitution_ordinal INTEGER NOT NULL CHECK(substitution_ordinal>=0),
 formal_count INTEGER NOT NULL CHECK(formal_count>=0),argument_count INTEGER NOT NULL CHECK(argument_count>=0),
 arguments_known_dependent INTEGER NOT NULL CHECK(arguments_known_dependent IN(0,1)),
 arguments_known_nondependent INTEGER NOT NULL CHECK(arguments_known_nondependent IN(0,1)),
 adjust_coordinates INTEGER NOT NULL CHECK(adjust_coordinates IN(0,1)),alias_deduction_guide INTEGER NOT NULL CHECK(alias_deduction_guide IN(0,1)),
 PRIMARY KEY(expression_id,substitution_ordinal)
) STRICT;

CREATE TABLE source_expression_substitution_formals (
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),substitution_ordinal INTEGER NOT NULL,ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template')),
 source_name TEXT NOT NULL,
 type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 default_type_id INTEGER REFERENCES source_types(id),default_constant_id INTEGER REFERENCES source_semantic_constants(id),
 default_template_id INTEGER REFERENCES source_templates(id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),is_abbreviated INTEGER NOT NULL CHECK(is_abbreviated IN(0,1)),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 PRIMARY KEY(expression_id,substitution_ordinal,ordinal),
 FOREIGN KEY(expression_id,substitution_ordinal) REFERENCES source_expression_substitutions(expression_id,substitution_ordinal)
) STRICT;

CREATE TABLE source_expression_substitution_arguments (
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),substitution_ordinal INTEGER NOT NULL,ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN('type','constant','template','pack-boundary')),
 source_type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),PRIMARY KEY(expression_id,substitution_ordinal,ordinal),
 FOREIGN KEY(expression_id,substitution_ordinal) REFERENCES source_expression_substitutions(expression_id,substitution_ordinal)
) STRICT;

CREATE TABLE source_expression_declarations (
 id INTEGER PRIMARY KEY CHECK(id>0),kind TEXT NOT NULL CHECK(kind IN('variable','routine')),
 source_name TEXT NOT NULL,qualified_name TEXT NOT NULL,source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 owner_source_type_id INTEGER REFERENCES source_types(id),
 template_prototype_declaration_id INTEGER REFERENCES source_expression_declarations(id),
 template_origin_kind TEXT NOT NULL CHECK(template_origin_kind IN('none','variable-template','function-template','static-data-member','class-member-function')),
 routine_kind TEXT NOT NULL CHECK(routine_kind IN('not-routine','function','constructor','destructor','operator')),
 scope_kind TEXT NOT NULL CHECK(scope_kind IN('nonlocal','function-parameter','function-local','namespace')),
 has_template_origin INTEGER NOT NULL CHECK(has_template_origin IN(0,1)),is_template_prototype INTEGER NOT NULL CHECK(is_template_prototype IN(0,1)),
 is_constexpr INTEGER NOT NULL CHECK(is_constexpr IN(0,1)),
 formal_count INTEGER NOT NULL CHECK(formal_count>=0),argument_count INTEGER NOT NULL CHECK(argument_count>=0),
 original_init_kind INTEGER CHECK(original_init_kind BETWEEN 0 AND 5),original_dynamic_init_kind INTEGER CHECK(original_dynamic_init_kind>=0),
 initializer_kind TEXT NOT NULL CHECK(initializer_kind IN('not-variable','none','zero','constant','expression')),
 initializer_constant_id INTEGER REFERENCES source_semantic_constants(id),initializer_expression_id INTEGER REFERENCES source_expressions(id),
 definition_status TEXT NOT NULL CHECK(definition_status='reference-only'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-expression-declaration'),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL
) STRICT;
CREATE TABLE source_expression_declaration_references (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id)
) STRICT;

CREATE TABLE source_expression_declaration_formals (
 declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template')),
 source_name TEXT NOT NULL,
 type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 default_type_id INTEGER REFERENCES source_types(id),default_constant_id INTEGER REFERENCES source_semantic_constants(id),
 default_template_id INTEGER REFERENCES source_templates(id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),is_abbreviated INTEGER NOT NULL CHECK(is_abbreviated IN(0,1)),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 PRIMARY KEY(declaration_id,ordinal)
) STRICT;

CREATE TABLE source_expression_declaration_arguments (
 declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN('type','constant','template','pack-boundary')),
 source_type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),PRIMARY KEY(declaration_id,ordinal)
) STRICT;

CREATE TABLE source_constraint_argument_details (
 owner_kind TEXT NOT NULL CHECK(owner_kind IN('class','concept','declaration','substitution','lookup','alias-selected','alias-original','class-pattern','class-selected')),owner_id INTEGER NOT NULL CHECK(owner_id>0),
 substitution_ordinal INTEGER NOT NULL CHECK(substitution_ordinal>=0),argument_ordinal INTEGER NOT NULL CHECK(argument_ordinal>=0),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),is_pack_element INTEGER NOT NULL CHECK(is_pack_element IN(0,1)),
 has_pack_ellipsis INTEGER NOT NULL CHECK(has_pack_ellipsis IN(0,1)),is_integer_pack INTEGER NOT NULL CHECK(is_integer_pack IN(0,1)),
 is_explicit INTEGER NOT NULL CHECK(is_explicit IN(0,1)),is_injected_class_name INTEGER NOT NULL CHECK(is_injected_class_name IN(0,1)),
 parameter_is_auto INTEGER NOT NULL CHECK(parameter_is_auto IN(0,1)),parameter_is_decltype_auto INTEGER NOT NULL CHECK(parameter_is_decltype_auto IN(0,1)),
 pack_expansion_id INTEGER REFERENCES source_pack_expansions(id),substituted_template_id INTEGER REFERENCES source_templates(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-template-argument'),
 PRIMARY KEY(owner_kind,owner_id,substitution_ordinal,argument_ordinal)
) STRICT;
CREATE TABLE source_pack_expansions (
 id INTEGER PRIMARY KEY CHECK(id>0),reference_count INTEGER NOT NULL CHECK(reference_count>=0),
 ellipsis_seen INTEGER NOT NULL CHECK(ellipsis_seen IN(0,1)),uses_only_enclosing_packs INTEGER NOT NULL CHECK(uses_only_enclosing_packs IN(0,1)),
 uses_any_enclosing_packs INTEGER NOT NULL CHECK(uses_any_enclosing_packs IN(0,1)),is_function_declarator INTEGER NOT NULL CHECK(is_function_declarator IN(0,1)),
 is_pack_index INTEGER NOT NULL CHECK(is_pack_index IN(0,1)),parameter_is_template_template INTEGER NOT NULL CHECK(parameter_is_template_template IN(0,1)),
 parameter_source_type_id INTEGER REFERENCES source_types(id),source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-pack-expansion')
) STRICT;
CREATE TABLE source_pack_references (
 expansion_id INTEGER NOT NULL REFERENCES source_pack_expansions(id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN('type','constant','template')),source_type_id INTEGER REFERENCES source_types(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),template_id INTEGER REFERENCES source_templates(id),
 parameter_depth INTEGER CHECK(parameter_depth>=0),parameter_position INTEGER CHECK(parameter_position>0),
 uses_enclosing_pack INTEGER NOT NULL CHECK(uses_enclosing_pack IN(0,1)),source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 PRIMARY KEY(expansion_id,ordinal)
) STRICT;
CREATE TABLE source_dependent_function_lookups (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),source_name TEXT NOT NULL,qualified_name TEXT NOT NULL,
 owner_source_type_id INTEGER REFERENCES source_types(id),conversion_source_type_id INTEGER REFERENCES source_types(id),operator_kind INTEGER NOT NULL CHECK(operator_kind BETWEEN 0 AND 47),
 has_explicit_template_arguments INTEGER NOT NULL CHECK(has_explicit_template_arguments IN(0,1)),is_qualified_name INTEGER NOT NULL CHECK(is_qualified_name IN(0,1)),
 has_address_of INTEGER NOT NULL CHECK(has_address_of IN(0,1)),has_lookup_symbol INTEGER NOT NULL CHECK(has_lookup_symbol IN(0,1)),candidate_count INTEGER NOT NULL CHECK(candidate_count>=0),
 member_of_unknown_base INTEGER NOT NULL CHECK(member_of_unknown_base IN(0,1)),qualified_unknown_base_member INTEGER NOT NULL CHECK(qualified_unknown_base_member IN(0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-dependent-function-lookup')
) STRICT;
CREATE TABLE source_dependent_function_candidates (
 expression_id INTEGER NOT NULL REFERENCES source_dependent_function_lookups(expression_id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id),PRIMARY KEY(expression_id,ordinal)
) STRICT;
CREATE TABLE source_dependent_function_arguments (
 expression_id INTEGER NOT NULL REFERENCES source_dependent_function_lookups(expression_id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN('type','constant','template','pack-boundary')),source_type_id INTEGER REFERENCES source_types(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),template_id INTEGER REFERENCES source_templates(id),PRIMARY KEY(expression_id,ordinal)
) STRICT;

-- The stock default-constructor adapter copies accepted call-site default
-- arguments into a generated expression lifetime. These are execution
-- adoptions of original source identities, never replacement source owners.
CREATE TABLE default_argument_evaluations (
 function_entity_id INTEGER PRIMARY KEY REFERENCES functions(entity_id),
 owner_function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 source_function_entity_id INTEGER NOT NULL REFERENCES source_functions(entity_id),
 target_entity_id INTEGER NOT NULL REFERENCES entities(id),
 forwarding_call_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 root_source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-default-argument-copy')
) STRICT;
CREATE TABLE default_argument_lifetime_executions (
 function_entity_id INTEGER NOT NULL REFERENCES default_argument_evaluations(function_entity_id),
 source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 copied_parent_source_lifetime_id INTEGER REFERENCES source_lifetimes(id),
 copied_sibling_ordinal INTEGER CHECK(copied_sibling_ordinal>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-default-argument-copy'),
 PRIMARY KEY(function_entity_id,source_lifetime_id),
 CHECK((copied_parent_source_lifetime_id IS NULL)=(copied_sibling_ordinal IS NULL))
) STRICT;
CREATE TABLE default_argument_obligation_executions (
 function_entity_id INTEGER NOT NULL REFERENCES default_argument_evaluations(function_entity_id),
 source_obligation_id INTEGER REFERENCES source_destruction_obligations(id),
 source_allocation_obligation_id INTEGER REFERENCES source_allocation_obligations(id),
 source_lifetime_id INTEGER NOT NULL REFERENCES source_lifetimes(id),
 copied_ordinal INTEGER NOT NULL CHECK(copied_ordinal>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-default-argument-copy'),
 PRIMARY KEY(function_entity_id,source_lifetime_id,copied_ordinal),
 UNIQUE(function_entity_id,source_obligation_id),
 UNIQUE(function_entity_id,source_allocation_obligation_id),
 CHECK((source_obligation_id IS NULL)<>(source_allocation_obligation_id IS NULL))
) STRICT;
CREATE TABLE default_argument_temporary_executions (
 function_entity_id INTEGER NOT NULL REFERENCES default_argument_evaluations(function_entity_id),
 source_temporary_id INTEGER NOT NULL REFERENCES source_record_temporaries(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-default-argument-copy'),
 PRIMARY KEY(function_entity_id,source_temporary_id)
) STRICT;

CREATE TABLE source_semantic_constant_expressions (
 constant_id INTEGER PRIMARY KEY REFERENCES source_semantic_constants(id),
 expression_id INTEGER NOT NULL REFERENCES source_expressions(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-edg-expression')
) STRICT;
CREATE TABLE source_aggregate_constants (
 constant_id INTEGER PRIMARY KEY REFERENCES source_semantic_constants(id),
 element_count INTEGER NOT NULL CHECK(element_count>=0),
 original_type_id INTEGER REFERENCES source_types(id),
 backing_expression_id INTEGER REFERENCES source_expressions(id),
 flag_bits INTEGER NOT NULL CHECK(flag_bits BETWEEN 0 AND 65535),
 added_template_const INTEGER NOT NULL CHECK(added_template_const IN(0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-aggregate-constant')
) STRICT;

CREATE TABLE source_dynamic_exception_specifications (
 source_routine_type_id INTEGER PRIMARY KEY REFERENCES source_routine_types(type_id),
 kind TEXT NOT NULL CHECK(kind='throw-list'),
 allowed_type_count INTEGER NOT NULL CHECK(allowed_type_count>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-resolved-source-specification')
) STRICT;
CREATE TABLE source_dynamic_exception_types (
 source_routine_type_id INTEGER NOT NULL REFERENCES source_dynamic_exception_specifications(source_routine_type_id),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 declared_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 match_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 PRIMARY KEY(source_routine_type_id,ordinal)
) STRICT;
CREATE TABLE function_dynamic_exception_boundaries (
 function_entity_id INTEGER PRIMARY KEY REFERENCES functions(entity_id),
 source_routine_type_id INTEGER NOT NULL REFERENCES source_dynamic_exception_specifications(source_routine_type_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-throw-spec-function-boundary')
) STRICT;

CREATE TABLE source_named_templates (
 has_namespace_owner INTEGER NOT NULL CHECK(has_namespace_owner IN(0,1)),
 template_id INTEGER PRIMARY KEY REFERENCES source_templates(id),qualified_name TEXT NOT NULL,
 owner_source_type_id INTEGER REFERENCES source_types(id),prototype_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 constraint_expression_id INTEGER REFERENCES source_expressions(id),capture_proof TEXT NOT NULL CHECK(capture_proof='original-alias-template')
) STRICT;
CREATE TABLE source_alias_instances (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),template_id INTEGER NOT NULL REFERENCES source_templates(id),
 is_prototype INTEGER NOT NULL CHECK(is_prototype IN(0,1)),is_nonreal INTEGER NOT NULL CHECK(is_nonreal IN(0,1)),
 is_dependent INTEGER NOT NULL CHECK(is_dependent IN(0,1)),argument_count INTEGER NOT NULL CHECK(argument_count>=0),
 has_original_arguments INTEGER NOT NULL CHECK(has_original_arguments IN(0,1)),original_argument_count INTEGER NOT NULL CHECK(original_argument_count>=0),
 minimum_arguments INTEGER NOT NULL CHECK(minimum_arguments>=-1),capture_proof TEXT NOT NULL CHECK(capture_proof='original-alias-instance')
) STRICT;
CREATE TABLE source_alias_arguments (
 type_id INTEGER NOT NULL REFERENCES source_alias_instances(type_id),list_kind TEXT NOT NULL CHECK(list_kind IN('alias-selected','alias-original')),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),kind TEXT NOT NULL CHECK(kind IN('type','constant','template','pack-boundary')),
 source_type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),template_id INTEGER REFERENCES source_templates(id),
 PRIMARY KEY(type_id,list_kind,ordinal)
) STRICT;
CREATE TABLE source_type_operators (
 type_id INTEGER PRIMARY KEY REFERENCES source_types(id),opcode INTEGER NOT NULL,
 expression_id INTEGER REFERENCES source_expressions(id),operand_source_type_id INTEGER REFERENCES source_types(id),
 is_dependent INTEGER NOT NULL CHECK(is_dependent IN(0,1)),is_unparenthesized_decltype INTEGER NOT NULL CHECK(is_unparenthesized_decltype IN(0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-type-operator'),operator_name TEXT NOT NULL,CHECK((expression_id IS NULL)!=(operand_source_type_id IS NULL))
) STRICT;

CREATE TABLE source_expression_temporaries (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),initialization_kind INTEGER NOT NULL CHECK(initialization_kind BETWEEN 0 AND 7),
 spelled_source_type_id INTEGER REFERENCES source_types(id),constructor_declaration_id INTEGER REFERENCES source_expression_declarations(id),
 destructor_declaration_id INTEGER REFERENCES source_expression_declarations(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 is_dependent_constructor INTEGER NOT NULL CHECK(is_dependent_constructor IN(0,1)),
 lifetime_disposition TEXT NOT NULL CHECK(lifetime_disposition='no-linked-lifetime'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-expression-temporary')
) STRICT;
CREATE TABLE source_expression_temporary_flags (
 expression_id INTEGER NOT NULL REFERENCES source_expression_temporaries(expression_id),flag TEXT NOT NULL,
 PRIMARY KEY(expression_id,flag)
) STRICT;

CREATE TABLE source_parameter_binders (
 id INTEGER PRIMARY KEY,kind TEXT NOT NULL CHECK(kind IN('requires','routine-prototype')),
 parent_binder_id INTEGER REFERENCES source_parameter_binders(id),
 requires_expression_id INTEGER REFERENCES source_expressions(id),
 routine_source_type_id INTEGER REFERENCES source_routine_types(type_id),
 outside_parameter_list INTEGER NOT NULL CHECK(outside_parameter_list IN(0,1)),
 parameter_count INTEGER NOT NULL CHECK(parameter_count>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-parameter-scope'),
 CHECK((kind='requires' AND requires_expression_id IS NOT NULL AND routine_source_type_id IS NULL) OR
       (kind='routine-prototype' AND requires_expression_id IS NULL AND routine_source_type_id IS NOT NULL))
) STRICT;
CREATE TABLE source_binder_parameters (
 binder_id INTEGER NOT NULL REFERENCES source_parameter_binders(id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),declared_source_type_id INTEGER REFERENCES source_types(id),
 source_name TEXT NOT NULL,qualifier_bits INTEGER NOT NULL CHECK(qualifier_bits>=0),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),PRIMARY KEY(binder_id,ordinal)
) STRICT;

CREATE TABLE source_expression_allocations (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 object_source_type_id INTEGER NOT NULL REFERENCES source_types(id),operator_declaration_id INTEGER REFERENCES source_expression_declarations(id),
 is_new INTEGER NOT NULL CHECK(is_new IN(0,1)),
 is_dependent_selection INTEGER NOT NULL CHECK(is_dependent_selection IN(0,1)),
 is_placement INTEGER NOT NULL CHECK(is_placement IN(0,1)),
 is_aligned INTEGER NOT NULL CHECK(is_aligned IN(0,1)),
 is_array_delete INTEGER NOT NULL CHECK(is_array_delete IN(0,1)),
 is_global INTEGER NOT NULL CHECK(is_global IN(0,1)),
 has_initializer INTEGER NOT NULL CHECK(has_initializer IN(0,1)),
 is_braced_initializer INTEGER NOT NULL CHECK(is_braced_initializer IN(0,1)),
 is_parenthesized_aggregate INTEGER NOT NULL CHECK(is_parenthesized_aggregate IN(0,1)),
 has_deducible_type INTEGER NOT NULL CHECK(has_deducible_type IN(0,1)),
 has_parenthesized_type_id INTEGER NOT NULL CHECK(has_parenthesized_type_id IN(0,1)),
 initialization_disposition TEXT NOT NULL CHECK(initialization_disposition IN ('absent-in-original','retained-in-edg-graph')),
 rollback_disposition TEXT NOT NULL CHECK(rollback_disposition IN ('absent-in-original','retained-in-edg-graph')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-new-delete-expression'),
 is_lookup_deferred INTEGER NOT NULL CHECK(is_lookup_deferred IN(0,1))
) STRICT;
CREATE TABLE source_declaration_operators (
 declaration_id INTEGER PRIMARY KEY REFERENCES source_expression_declarations(id),operator_kind INTEGER NOT NULL CHECK(operator_kind BETWEEN 1 AND 47),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-operator-declaration')
) STRICT;

CREATE TABLE source_routine_prototype_origins (
 routine_source_type_id INTEGER PRIMARY KEY REFERENCES source_routine_types(type_id),
 original_routine_source_type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 original_binder_id INTEGER NOT NULL REFERENCES source_parameter_binders(id),
 kind TEXT NOT NULL CHECK(kind='lambda-conversion-copy'),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-lambda-conversion-prototype'),
 CHECK(routine_source_type_id<>original_routine_source_type_id)
) STRICT;

CREATE TABLE source_expression_folds (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 operator_token INTEGER NOT NULL CHECK(operator_token>=0),operator_spelling TEXT NOT NULL,
 is_left_associative INTEGER NOT NULL CHECK(is_left_associative IN(0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-fold-expression')
) STRICT;

CREATE TABLE source_expression_field_references (
 expression_id INTEGER PRIMARY KEY REFERENCES source_expressions(id),
 source_field_id INTEGER NOT NULL REFERENCES source_record_fields(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-field-reference')
) STRICT;

CREATE TABLE source_function_parameter_declarations (
 declaration_id INTEGER PRIMARY KEY REFERENCES source_expression_declarations(id),
 routine_declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id),
 binder_id INTEGER NOT NULL REFERENCES source_parameter_binders(id),
 parameter_ordinal INTEGER NOT NULL CHECK(parameter_ordinal>=0),
 declared_source_type_id INTEGER REFERENCES source_types(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-function-parameter-list'),
 FOREIGN KEY(binder_id,parameter_ordinal) REFERENCES source_binder_parameters(binder_id,ordinal),
 UNIQUE(routine_declaration_id,parameter_ordinal)
) STRICT;

CREATE TABLE source_class_template_headers (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_types(id),
 primary_source_record_id INTEGER REFERENCES source_class_template_headers(source_record_id),
 formal_count INTEGER NOT NULL CHECK(formal_count>=0),pattern_count INTEGER NOT NULL CHECK(pattern_count>=0),
 constraint_expression_id INTEGER REFERENCES source_expressions(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-class-template-header'),
 CHECK(primary_source_record_id IS NULL OR primary_source_record_id!=source_record_id)
) STRICT;
CREATE TABLE source_class_specializations (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_instantiations(source_record_id),
 selected_source_record_id INTEGER NOT NULL REFERENCES source_class_template_headers(source_record_id),
 selected_argument_count INTEGER NOT NULL CHECK(selected_argument_count>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-partial-class-substitution')
) STRICT;

CREATE TABLE source_class_template_formals (
 source_record_id INTEGER NOT NULL REFERENCES source_class_template_headers(source_record_id),ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template')),
 source_name TEXT NOT NULL,
 type_id INTEGER REFERENCES source_types(id),constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 default_type_id INTEGER REFERENCES source_types(id),default_constant_id INTEGER REFERENCES source_semantic_constants(id),
 default_template_id INTEGER REFERENCES source_templates(id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),is_abbreviated INTEGER NOT NULL CHECK(is_abbreviated IN(0,1)),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 PRIMARY KEY(source_record_id,ordinal)
) STRICT;

CREATE TABLE source_class_template_patterns (
 source_record_id INTEGER NOT NULL REFERENCES source_class_template_headers(source_record_id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template','pack-boundary')),
 type_id INTEGER REFERENCES source_types(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 PRIMARY KEY(source_record_id,ordinal),
 CHECK((kind='type' AND type_id IS NOT NULL AND constant_id IS NULL AND template_id IS NULL) OR
       (kind='constant' AND type_id IS NULL AND constant_id IS NOT NULL AND template_id IS NULL) OR
       (kind='template' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NOT NULL) OR
       (kind='pack-boundary' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NULL))
) STRICT;

CREATE TABLE source_class_specialization_arguments (
 source_record_id INTEGER NOT NULL REFERENCES source_class_specializations(source_record_id),
 is_pack INTEGER NOT NULL CHECK(is_pack IN(0,1)),
 ordinal INTEGER NOT NULL CHECK(ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN ('type','constant','template','pack-boundary')),
 type_id INTEGER REFERENCES source_types(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),
 template_id INTEGER REFERENCES source_templates(id),
 PRIMARY KEY(source_record_id,ordinal),
 CHECK((kind='type' AND type_id IS NOT NULL AND constant_id IS NULL AND template_id IS NULL) OR
       (kind='constant' AND type_id IS NULL AND constant_id IS NOT NULL AND template_id IS NULL) OR
       (kind='template' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NOT NULL) OR
       (kind='pack-boundary' AND type_id IS NULL AND constant_id IS NULL AND template_id IS NULL))
) STRICT;

CREATE TABLE source_enum_increments (
 operation_constant_id INTEGER PRIMARY KEY REFERENCES source_semantic_constants(id),
 previous_constant_id INTEGER NOT NULL UNIQUE REFERENCES source_semantic_constants(id),
 one_constant_id INTEGER NOT NULL REFERENCES source_semantic_constants(id),
 original_source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 did_force_dependent_type INTEGER NOT NULL CHECK(did_force_dependent_type IN(0,1)),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-dependent-enumerator-increment')
) STRICT;

-- Original unevaluated sizeof...(pack), preserving the exact operand category.
CREATE TABLE source_pack_sizes (
 constant_id INTEGER PRIMARY KEY REFERENCES source_semantic_constants(id),
 is_type INTEGER NOT NULL CHECK(is_type IN(0,1)),
 is_template_template INTEGER NOT NULL CHECK(is_template_template IN(0,1)),
 source_type_id INTEGER REFERENCES source_types(id),
 expression_id INTEGER REFERENCES source_expressions(id),
 template_id INTEGER REFERENCES source_templates(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-sizeof-pack'),
 CHECK((is_template_template=1 AND template_id IS NOT NULL AND source_type_id IS NULL AND expression_id IS NULL) OR
       (is_template_template=0 AND is_type=1 AND source_type_id IS NOT NULL AND template_id IS NULL AND expression_id IS NULL) OR
       (is_template_template=0 AND is_type=0 AND expression_id IS NOT NULL AND source_type_id IS NULL AND template_id IS NULL))
) STRICT;

-- A nonreal EDG lookup class representing an original parameter/decltype.
CREATE TABLE source_proxy_types (
 source_record_id INTEGER PRIMARY KEY REFERENCES source_record_types(id),
 original_type_id INTEGER NOT NULL REFERENCES source_types(id),
 source_occurrence INTEGER NOT NULL CHECK(source_occurrence>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-class-proxy-of-type')
) STRICT;

CREATE TABLE generated_entity_initializers (
 function_entity_id INTEGER PRIMARY KEY REFERENCES functions(entity_id),
 owner_function_entity_id INTEGER NOT NULL REFERENCES functions(entity_id),
 source_allocation_obligation_id INTEGER NOT NULL REFERENCES source_allocation_obligations(id),
 source_element_type_id INTEGER NOT NULL REFERENCES source_types(id),
 element_type_id INTEGER NOT NULL REFERENCES types(id),
 destination_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 count_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 model_binding_id INTEGER NOT NULL REFERENCES bindings(id),
 copy_node_id INTEGER NOT NULL UNIQUE REFERENCES nodes(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-counted-model-initializer-factory')
) STRICT;
CREATE TABLE generated_entity_initializer_calls (
 node_id INTEGER PRIMARY KEY REFERENCES nodes(id),
 helper_entity_id INTEGER NOT NULL UNIQUE REFERENCES generated_entity_initializers(function_entity_id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='observed-entity-initializer-call')
) STRICT;

CREATE TABLE source_namespaces (
 id INTEGER PRIMARY KEY,parent_id INTEGER REFERENCES source_namespaces(id),
 source_name TEXT NOT NULL,is_unnamed INTEGER NOT NULL CHECK(is_unnamed IN(0,1)),
 is_inline INTEGER NOT NULL CHECK(is_inline IN(0,1)),has_internal_linkage INTEGER NOT NULL CHECK(has_internal_linkage IN(0,1)),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL CHECK(line>=0),column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-namespace-declaration'),
 CHECK((is_unnamed=1)=(source_name='')),CHECK(parent_id IS NULL OR parent_id!=id)
) STRICT;
CREATE TABLE source_namespace_owners (
 owner_kind TEXT NOT NULL CHECK(owner_kind IN('type','declaration','concept','template')),
 owner_id INTEGER NOT NULL CHECK(owner_id>0),namespace_id INTEGER NOT NULL REFERENCES source_namespaces(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-enclosing-namespace'),PRIMARY KEY(owner_kind,owner_id)
) STRICT;

CREATE TABLE source_local_scopes (
 id INTEGER PRIMARY KEY,
 routine_declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id),
 parent_id INTEGER REFERENCES source_local_scopes(id),
 kind TEXT NOT NULL CHECK(kind IN('function','block','condition')),
 sibling_ordinal INTEGER NOT NULL CHECK(sibling_ordinal>=0),
 child_scope_count INTEGER NOT NULL CHECK(child_scope_count>=0),variable_count INTEGER NOT NULL CHECK(variable_count>=0),
 source_file_id INTEGER REFERENCES source_files(id),line INTEGER NOT NULL,column INTEGER NOT NULL,
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-local-scope-tree'),
 CHECK((kind='function')=(parent_id IS NULL)),CHECK(parent_id IS NULL OR parent_id!=id)
) STRICT;
CREATE TABLE source_local_variable_declarations (
 declaration_id INTEGER PRIMARY KEY REFERENCES source_expression_declarations(id),
 scope_id INTEGER NOT NULL REFERENCES source_local_scopes(id),
 declaration_ordinal INTEGER NOT NULL CHECK(declaration_ordinal>=0),
 declared_source_type_id INTEGER REFERENCES source_types(id),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-nonstatic-variable-list'),
 UNIQUE(scope_id,declaration_ordinal)
) STRICT;

CREATE TABLE source_bitfield_widths (
 source_field_id INTEGER PRIMARY KEY REFERENCES source_record_fields(id),
 constant_id INTEGER REFERENCES source_semantic_constants(id),
 declared_width INTEGER CHECK(declared_width>=0),
 width_kind TEXT NOT NULL CHECK(width_kind IN ('evaluated','dependent')),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-field-width'),
 CHECK((width_kind='dependent')=(declared_width IS NULL)),
 CHECK(width_kind<>'dependent' OR constant_id IS NOT NULL)
) STRICT;

CREATE TABLE source_routine_type_substitutions (
 routine_source_type_id INTEGER PRIMARY KEY REFERENCES source_routine_types(type_id),
 selected_source_declaration_id INTEGER NOT NULL REFERENCES source_expression_declarations(id),
 argument_owner_kind TEXT NOT NULL CHECK(argument_owner_kind IN ('function-template','class-template','class-partial')),
 borrowed_argument_count INTEGER NOT NULL CHECK(borrowed_argument_count>0),
 capture_proof TEXT NOT NULL CHECK(capture_proof='original-routine-type-argument-substitution')
) STRICT;
CREATE TABLE source_routine_borrowed_arguments (
 routine_source_type_id INTEGER NOT NULL REFERENCES source_routine_type_substitutions(routine_source_type_id),
 argument_ordinal INTEGER NOT NULL CHECK(argument_ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 PRIMARY KEY(routine_source_type_id,argument_ordinal)
) STRICT;

-- Original parser-factory witnesses for anonymous routine signatures. These
-- are source metadata only; no executable calling convention is implied.
CREATE TABLE source_declarator_contexts (
 id INTEGER PRIMARY KEY CHECK(id>0),
 routine_source_type_id INTEGER NOT NULL REFERENCES source_routine_types(type_id),
 scope_ordinal INTEGER NOT NULL CHECK(scope_ordinal>=0),
 kind TEXT NOT NULL CHECK(kind IN('routine','class','class-partial','observed')),
 selected_source_declaration_id INTEGER REFERENCES source_expression_declarations(id),
 source_record_id INTEGER REFERENCES source_record_types(id),
 header_source_record_id INTEGER REFERENCES source_class_template_headers(source_record_id),
 argument_count INTEGER NOT NULL CHECK(argument_count>=0),
 borrowed_argument_count INTEGER NOT NULL CHECK(borrowed_argument_count>=0 AND borrowed_argument_count<=argument_count),
 source_file_id INTEGER REFERENCES source_files(id),
 line INTEGER NOT NULL CHECK(line>=0),column INTEGER NOT NULL CHECK(column>=0),
 capture_proof TEXT NOT NULL CHECK(capture_proof IN('observed-routine-declarator-instantiation-context','observed-function-instantiation-declarator-context')),
 CHECK(capture_proof<>'observed-function-instantiation-declarator-context' OR kind IN('routine','observed')),
 UNIQUE(routine_source_type_id,scope_ordinal),
 CHECK((kind='routine' AND selected_source_declaration_id IS NOT NULL AND source_record_id IS NULL AND header_source_record_id IS NULL) OR
       (kind IN('class','class-partial') AND selected_source_declaration_id IS NULL AND source_record_id IS NOT NULL AND header_source_record_id IS NOT NULL) OR
       kind='observed')
) STRICT;
CREATE TABLE source_declarator_borrowed_arguments (
 context_id INTEGER NOT NULL REFERENCES source_declarator_contexts(id),
 argument_ordinal INTEGER NOT NULL CHECK(argument_ordinal>=0),
 source_type_id INTEGER NOT NULL REFERENCES source_types(id),
 PRIMARY KEY(context_id,argument_ordinal)
) STRICT;
