static const char roslynobj_schema_contract_sha256[] = "e49f0f888402b4fedd4e40cbd18ec9580fb859f67ac3d539c60975437ce98eaa";
static const char roslynobj_schema_sql[] = R"roslynsql(-- RoslynObj 1.0: normalized scalar and typed relationship contract.
-- SQLite 3.37+ required. Exact target quantities use canonical decimal TEXT.
-- Extension kind catalogs are persisted only when referenced.
PRAGMA foreign_keys=ON;
PRAGMA user_version=1000;
CREATE TABLE kind (id INTEGER PRIMARY KEY, family TEXT NOT NULL, name TEXT NOT NULL, UNIQUE(family,name)) STRICT;

CREATE TABLE kind_disposition (
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  state TEXT NOT NULL,
  canonical_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (kind_id)
) STRICT;

CREATE TABLE text_atom (
  id INTEGER PRIMARY KEY,
  value TEXT NOT NULL COLLATE BINARY UNIQUE
) STRICT;

CREATE TABLE entity (
  id INTEGER PRIMARY KEY,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE object_header (
  id INTEGER PRIMARY KEY,
  format_major INTEGER NOT NULL,
  format_minor INTEGER NOT NULL,
  artifact_id TEXT NOT NULL,
  producer_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  revision_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  edg_version_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  capture_profile_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_profile_id INTEGER REFERENCES target_profile(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  main_source_file_id INTEGER REFERENCES source_file(id) DEFERRABLE INITIALLY DEFERRED,
  translation_unit_id INTEGER REFERENCES translation_unit(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  complete INTEGER NOT NULL CHECK(complete=1)
) STRICT;

CREATE TABLE target_profile (
  id INTEGER PRIMARY KEY,
  abi_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  addressable_unit_bits INTEGER NOT NULL,
  little_endian INTEGER NOT NULL,
  pointer_bits INTEGER NOT NULL,
  pointer_alignment TEXT NOT NULL
) STRICT;

CREATE TABLE translation_unit (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  global_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  primary_input_id INTEGER REFERENCES source_file(id) DEFERRABLE INITIALLY DEFERRED,
  language_mode_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  entry_point_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE translation_unit_root (
  owner_id INTEGER REFERENCES translation_unit(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, kind_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE file_scope_dynamic_init_routine (
  owner_id INTEGER REFERENCES translation_unit(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_thread_local INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE orphan_scope (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  translation_unit_id INTEGER REFERENCES translation_unit(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  scope_number TEXT NOT NULL
) STRICT;

CREATE TABLE orphan_scope_member (
  owner_id INTEGER REFERENCES orphan_scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, kind_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE build_capability (
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  state_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  value_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  required INTEGER NOT NULL DEFAULT 0,
  enabled INTEGER,
  PRIMARY KEY (name_id)
) STRICT;

CREATE TABLE invocation_argument (
  ordinal INTEGER NOT NULL,
  text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (ordinal)
) STRICT;

CREATE TABLE fact_availability (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  fact_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  state_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  reason_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, fact_id)
) WITHOUT ROWID, STRICT;

CREATE TABLE scope (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parent_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  associated_entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  function_static_lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  is_constexpr_routine INTEGER NOT NULL,
  is_stmt_expr_block INTEGER NOT NULL,
  is_placeholder_scope INTEGER NOT NULL,
  this_parameter_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  nrvo_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE scope_member (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  member_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE scope_initialization (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  storage_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  compiler_generated INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE type_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE declaration (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  symbol_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  lexical_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  compiler_generated INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE namespace_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  alias_target_id INTEGER REFERENCES namespace_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_inline INTEGER NOT NULL DEFAULT 0,
  is_anonymous INTEGER NOT NULL DEFAULT 0,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  proxy_class_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  has_internal_linkage INTEGER NOT NULL,
  named_in_strong_using INTEGER NOT NULL,
  is_std INTEGER NOT NULL,
  has_gnu_abi_tag_attribute INTEGER,
  is_namespace_alias INTEGER NOT NULL
) STRICT;

CREATE TABLE routine_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  routine_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  containing_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  body_root_id INTEGER REFERENCES semantic_node(id) DEFERRABLE INITIALLY DEFERRED,
  body_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  body_state_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  special_function_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  builtin_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  instance_id INTEGER REFERENCES template_instance(id) DEFERRABLE INITIALLY DEFERRED,
  primary_entry_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_virtual INTEGER NOT NULL DEFAULT 0,
  is_pure INTEGER NOT NULL DEFAULT 0,
  is_final INTEGER NOT NULL DEFAULT 0,
  is_inline INTEGER NOT NULL DEFAULT 0,
  is_constexpr INTEGER NOT NULL DEFAULT 0,
  is_consteval INTEGER NOT NULL DEFAULT 0,
  is_deleted INTEGER NOT NULL DEFAULT 0,
  is_defaulted INTEGER NOT NULL DEFAULT 0,
  is_tls_initialization_routine INTEGER NOT NULL DEFAULT 0,
  is_tls_initialization_alias INTEGER NOT NULL DEFAULT 0,
  is_defined INTEGER NOT NULL DEFAULT 0,
  has_no_effect INTEGER,
  is_ineligible INTEGER,
  address_taken INTEGER NOT NULL DEFAULT 0,
  overrides_base_member INTEGER NOT NULL DEFAULT 0,
  is_override INTEGER NOT NULL DEFAULT 0,
  is_abstract INTEGER,
  is_sealed INTEGER,
  is_new_member INTEGER,
  is_interface_slot INTEGER,
  definition_cannot_be_generated INTEGER,
  covariant_return_virtual_override INTEGER NOT NULL DEFAULT 0,
  is_declared_constexpr INTEGER NOT NULL DEFAULT 0,
  is_constexpr_intrinsic INTEGER NOT NULL DEFAULT 0,
  is_called INTEGER NOT NULL DEFAULT 0,
  is_explicit_constructor INTEGER NOT NULL DEFAULT 0,
  is_explicit_conversion_function INTEGER NOT NULL DEFAULT 0,
  is_trivial_default_constructor INTEGER NOT NULL DEFAULT 0,
  is_trivial_copy_function INTEGER NOT NULL DEFAULT 0,
  is_trivial_destructor INTEGER NOT NULL DEFAULT 0,
  is_initializer_list_ctor INTEGER NOT NULL DEFAULT 0,
  is_delegating_ctor INTEGER NOT NULL DEFAULT 0,
  is_inheriting_ctor INTEGER NOT NULL DEFAULT 0,
  inherits_virtually INTEGER NOT NULL DEFAULT 0,
  is_deduction_guide_from_inheriting_ctor INTEGER NOT NULL DEFAULT 0,
  assignment_to_this_done INTEGER,
  is_template_function INTEGER NOT NULL DEFAULT 0,
  is_specialized INTEGER NOT NULL DEFAULT 0,
  specialized_with_old_syntax INTEGER NOT NULL DEFAULT 0,
  is_prototype_instantiation INTEGER NOT NULL DEFAULT 0,
  never_throws INTEGER NOT NULL DEFAULT 0,
  is_in_class_specialization INTEGER NOT NULL DEFAULT 0,
  explicit_instantiation INTEGER NOT NULL DEFAULT 0,
  class_explicitly_instantiated INTEGER NOT NULL DEFAULT 0,
  explicit_do_not_instantiate INTEGER NOT NULL DEFAULT 0,
  has_nodiscard_attribute INTEGER NOT NULL DEFAULT 0,
  never_inline INTEGER,
  is_naked INTEGER,
  declared_only_as_friend INTEGER,
  explicit_extern_inline INTEGER,
  is_direct_linkage_specifier_on_nondef_decl INTEGER,
  is_initialization_routine INTEGER,
  is_finalization_routine INTEGER,
  is_weak INTEGER,
  is_weakref INTEGER,
  is_gnu_alias INTEGER,
  is_ifunc INTEGER,
  has_gnu_used_attribute INTEGER,
  has_gnu_abi_tag_attribute INTEGER,
  in_gnu_abi_tag_namespace INTEGER,
  implicit_abi_tags_added INTEGER,
  allocates_memory INTEGER,
  no_instrument_function INTEGER,
  no_check_memory_usage INTEGER,
  always_inline INTEGER,
  gnu_c89_inline INTEGER,
  implicit_alias INTEGER,
  has_internal_linkage_attribute INTEGER,
  can_be_instantiated INTEGER,
  do_not_instantiate INTEGER,
  instance_required INTEGER,
  contains_try_block INTEGER NOT NULL DEFAULT 0,
  contains_local_class_type INTEGER NOT NULL DEFAULT 0,
  superseded_external INTEGER NOT NULL DEFAULT 0,
  is_defined_in_friend_decl INTEGER NOT NULL DEFAULT 0,
  is_defined_outside_of_parent INTEGER NOT NULL DEFAULT 0,
  is_inlinable INTEGER,
  definition_needed INTEGER,
  expl_template_arg_list_used INTEGER NOT NULL DEFAULT 0,
  definition_has_direct_linkage_specifier INTEGER,
  definition_for_inlining_only INTEGER NOT NULL DEFAULT 0,
  inline_instance_required INTEGER,
  suppress_inline_body INTEGER NOT NULL DEFAULT 0,
  need_out_of_line_copy INTEGER NOT NULL DEFAULT 0,
  contains_statement_expression INTEGER NOT NULL DEFAULT 0,
  inline_in_class_definition INTEGER,
  is_use_comdat INTEGER,
  is_alias_entry INTEGER,
  statics_have_been_promoted INTEGER,
  is_lambda_body INTEGER NOT NULL DEFAULT 0,
  declared_using_type_without_linkage INTEGER NOT NULL DEFAULT 0,
  contains_local_static_variable INTEGER NOT NULL DEFAULT 0,
  is_embeds_source_entries INTEGER,
  is_raw_literal_operator INTEGER NOT NULL DEFAULT 0,
  has_ctor_priority INTEGER,
  has_dtor_priority INTEGER,
  has_deducible_return_type INTEGER NOT NULL DEFAULT 0,
  has_deduced_return_type INTEGER NOT NULL DEFAULT 0,
  contains_generic_lambda INTEGER NOT NULL DEFAULT 0,
  is_coroutine INTEGER NOT NULL DEFAULT 0,
  friend_defined_in_instantiation INTEGER NOT NULL DEFAULT 0,
  has_pass_object_size_attr INTEGER NOT NULL DEFAULT 0,
  from_injected_tokens INTEGER NOT NULL DEFAULT 0,
  is_pure_function INTEGER,
  declared_storage_class_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  declared_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  operator_name_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  lambda_call_operator_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  deduction_guide_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_function_number TEXT,
  constexpr_intrinsic INTEGER,
  trailing_requires_clause_id INTEGER REFERENCES requires_clause(id) DEFERRABLE INITIALLY DEFERRED,
  inherited_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  generating_using_id INTEGER REFERENCES using_declaration(id) DEFERRABLE INITIALLY DEFERRED,
  overriding_function_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  overridden_function_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  this_adjustment TEXT,
  vcall_index TEXT,
  return_adjustment TEXT,
  vbase_index TEXT,
  ctor_dtor_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_priority TEXT,
  fp_contract_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  fenv_access_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  cx_limited_range_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  fx_full_precision_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  fx_fract_overflow_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  fx_accum_overflow_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  upc_access_method INTEGER,
  elf_visibility_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  surrounding_name_linkage_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  definition_c_name_linkage_specified INTEGER,
  gnu_supplement_id INTEGER REFERENCES gnu_routine_supplement(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE routine_parameter (
  owner_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  parameter_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_source_parameter INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE variable_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  initialization_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  initializer_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  constant_initializer_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  storage_duration_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  is_thread_local INTEGER NOT NULL DEFAULT 0,
  is_inline INTEGER NOT NULL DEFAULT 0,
  is_parameter INTEGER NOT NULL DEFAULT 0,
  binding_container_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  instance_id INTEGER REFERENCES template_instance(id) DEFERRABLE INITIALLY DEFERRED,
  cleanup_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_structured_binding INTEGER NOT NULL DEFAULT 0,
  is_structured_binding_container INTEGER NOT NULL DEFAULT 0,
  bound_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  standard_thread_local_declared INTEGER NOT NULL DEFAULT 0,
  standard_thread_local_effective INTEGER NOT NULL DEFAULT 0,
  extension_thread_local INTEGER NOT NULL DEFAULT 0,
  assembly_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  named_register_number INTEGER,
  register_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  dynamic_initialization_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  thread_initialization_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  thread_wrapper_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_rewritten_as_assignment INTEGER NOT NULL DEFAULT 0,
  lowering_generated INTEGER NOT NULL DEFAULT 0,
  promoted_local_static INTEGER NOT NULL DEFAULT 0,
  promoted_local_static_initialization INTEGER NOT NULL DEFAULT 0,
  extends_lifetime INTEGER NOT NULL DEFAULT 0,
  is_uninitialized INTEGER NOT NULL DEFAULT 0,
  has_flexible_array_initializer INTEGER NOT NULL DEFAULT 0,
  is_immutable INTEGER NOT NULL DEFAULT 0,
  is_constexpr INTEGER NOT NULL DEFAULT 0,
  is_constinit INTEGER NOT NULL DEFAULT 0,
  is_pack INTEGER NOT NULL DEFAULT 0,
  is_pack_element INTEGER NOT NULL DEFAULT 0,
  associated_parameter_type_id INTEGER REFERENCES parameter_type(id) DEFERRABLE INITIALLY DEFERRED,
  explicit_alignment TEXT,
  initialization_priority INTEGER,
  section_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  comdat_group_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  aliased_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  vla_element_count_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  effective_class_typeinfo_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_used INTEGER NOT NULL DEFAULT 0,
  address_taken INTEGER NOT NULL DEFAULT 0,
  referenced_non_locally INTEGER NOT NULL DEFAULT 0,
  modified_within_try_block INTEGER NOT NULL DEFAULT 0,
  is_template_variable INTEGER NOT NULL DEFAULT 0,
  is_prototype_instantiation INTEGER NOT NULL DEFAULT 0,
  is_nonreal INTEGER NOT NULL DEFAULT 0,
  is_specialized INTEGER NOT NULL DEFAULT 0,
  specialized_with_old_syntax INTEGER NOT NULL DEFAULT 0,
  explicit_instantiation INTEGER NOT NULL DEFAULT 0,
  class_explicitly_instantiated INTEGER NOT NULL DEFAULT 0,
  explicit_do_not_instantiate INTEGER NOT NULL DEFAULT 0,
  parameter_value_changed INTEGER NOT NULL DEFAULT 0,
  parameter_used_more_than_once INTEGER NOT NULL DEFAULT 0,
  is_handler_parameter INTEGER NOT NULL DEFAULT 0,
  is_this_parameter INTEGER NOT NULL DEFAULT 0,
  is_anonymous_parent_object INTEGER NOT NULL DEFAULT 0,
  is_member_constant INTEGER NOT NULL DEFAULT 0,
  suppress_inline_definition INTEGER NOT NULL DEFAULT 0,
  superseded_external INTEGER NOT NULL DEFAULT 0,
  has_variably_modified_type INTEGER NOT NULL DEFAULT 0,
  is_vla INTEGER NOT NULL DEFAULT 0,
  is_compound_literal INTEGER NOT NULL DEFAULT 0,
  has_explicit_initializer INTEGER NOT NULL DEFAULT 0,
  has_parenthesized_initializer INTEGER NOT NULL DEFAULT 0,
  has_direct_braced_initializer INTEGER NOT NULL DEFAULT 0,
  declared_with_auto INTEGER NOT NULL DEFAULT 0,
  declared_with_decltype_auto INTEGER NOT NULL DEFAULT 0,
  declared_with_class_template_placeholder INTEGER NOT NULL DEFAULT 0,
  declared_using_type_without_linkage INTEGER NOT NULL DEFAULT 0,
  is_enhanced_for_iterator INTEGER NOT NULL DEFAULT 0,
  initializer_in_class INTEGER NOT NULL DEFAULT 0,
  constant_valued_recorded INTEGER NOT NULL DEFAULT 0,
  is_template_parameter_object INTEGER NOT NULL DEFAULT 0,
  is_in_class_specialization INTEGER NOT NULL DEFAULT 0,
  is_optional_vtable INTEGER NOT NULL DEFAULT 0,
  vtable_defined INTEGER NOT NULL DEFAULT 0,
  is_weak INTEGER NOT NULL DEFAULT 0,
  is_weakref INTEGER NOT NULL DEFAULT 0,
  is_gnu_alias INTEGER NOT NULL DEFAULT 0,
  has_gnu_used_attribute INTEGER NOT NULL DEFAULT 0,
  has_gnu_abi_tag_attribute INTEGER NOT NULL DEFAULT 0,
  is_not_common INTEGER NOT NULL DEFAULT 0,
  is_common INTEGER NOT NULL DEFAULT 0,
  has_internal_linkage_attribute INTEGER NOT NULL DEFAULT 0,
  elf_visibility_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  declared_storage_class_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  declared_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  embeds_source_entries INTEGER NOT NULL DEFAULT 0,
  can_be_instantiated INTEGER,
  do_not_instantiate INTEGER,
  instance_required INTEGER,
  parameter_used_as_lvalue INTEGER,
  inline_instance_required INTEGER,
  is_temp_for_unmodified_inlined_parameter INTEGER,
  is_temp_for_constructor_this_inlined_parameter INTEGER
) STRICT;

CREATE TABLE variable_binding (
  owner_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE field_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  record_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  default_initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  is_mutable INTEGER NOT NULL DEFAULT 0,
  is_bit_field INTEGER NOT NULL DEFAULT 0,
  is_generated_base INTEGER NOT NULL DEFAULT 0,
  is_generated_vptr INTEGER,
  bit_size_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  bit_width_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  declared_bit_size TEXT,
  is_anonymous_parent_object INTEGER NOT NULL DEFAULT 0,
  is_init_capture INTEGER NOT NULL DEFAULT 0,
  is_captured_this INTEGER NOT NULL DEFAULT 0,
  is_captured_pack_element INTEGER NOT NULL DEFAULT 0,
  has_initializer INTEGER NOT NULL DEFAULT 0,
  init_is_ctor_dependent INTEGER NOT NULL DEFAULT 0,
  has_direct_braced_initializer INTEGER NOT NULL DEFAULT 0,
  has_nonconstant_initializer INTEGER NOT NULL DEFAULT 0,
  has_no_unique_address_attribute INTEGER NOT NULL DEFAULT 0,
  is_optimized_empty_class INTEGER NOT NULL DEFAULT 0,
  bit_size_constant_expr_in_local_expr_node_ref INTEGER NOT NULL DEFAULT 0,
  is_packed INTEGER,
  vla_treated_as_zero_length_array INTEGER,
  class_subobject_with_tail_padding INTEGER,
  bit_field_is_signed INTEGER,
  explicit_alignment TEXT
) STRICT;

CREATE TABLE field_property (
  id INTEGER PRIMARY KEY REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  get_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  put_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE label_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  statement_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  is_break INTEGER NOT NULL DEFAULT 0,
  is_continue INTEGER NOT NULL DEFAULT 0,
  is_leave INTEGER,
  reachable_by_fall_through INTEGER NOT NULL,
  is_switch_break INTEGER NOT NULL,
  address_taken INTEGER,
  locally_declared INTEGER,
  is_likely INTEGER NOT NULL,
  is_unlikely INTEGER NOT NULL,
  microsoft_try_depth TEXT
) STRICT;

CREATE TABLE enum_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  underlying_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_scoped INTEGER NOT NULL DEFAULT 0,
  explicit_underlying_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  has_explicit_base INTEGER NOT NULL DEFAULT 0,
  underlying_type_should_use_unsigned INTEGER NOT NULL DEFAULT 0,
  is_packed INTEGER NOT NULL DEFAULT 0,
  originally_unnamed INTEGER NOT NULL DEFAULT 0,
  is_template_enum INTEGER NOT NULL DEFAULT 0,
  is_prototype_instantiation INTEGER NOT NULL DEFAULT 0,
  is_nonreal INTEGER NOT NULL DEFAULT 0,
  is_specialized INTEGER NOT NULL DEFAULT 0,
  is_ms_instantiated_nonreal_enum INTEGER NOT NULL DEFAULT 0,
  elf_visibility_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  enumerator_list_seen INTEGER NOT NULL DEFAULT 0,
  enumerator_list_complete INTEGER NOT NULL DEFAULT 0,
  has_nodiscard INTEGER NOT NULL DEFAULT 0,
  base_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  uuid_string_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  uuid_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE enumerator (
  owner_id INTEGER REFERENCES enum_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  symbol_id INTEGER REFERENCES symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE alias_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  underlying_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE using_declaration (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  lexical_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  is_directive INTEGER NOT NULL DEFAULT 0,
  is_class_member INTEGER NOT NULL DEFAULT 0,
  is_inheriting_constructor INTEGER NOT NULL DEFAULT 0,
  is_hidden INTEGER NOT NULL DEFAULT 0,
  compiler_generated INTEGER NOT NULL DEFAULT 0,
  is_inline_namespace INTEGER NOT NULL DEFAULT 0,
  is_strong INTEGER NOT NULL DEFAULT 0,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  is_representative INTEGER NOT NULL DEFAULT 0,
  is_using_enum INTEGER NOT NULL DEFAULT 0,
  is_enumerator INTEGER NOT NULL,
  adjusted_access_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  qualifier_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  qualifier_namespace_id INTEGER REFERENCES namespace_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_global_qualified INTEGER,
  next_in_set_id INTEGER REFERENCES using_declaration(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE static_assertion (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  condition_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  message_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  symbol_id INTEGER REFERENCES symbol(id) DEFERRABLE INITIALLY DEFERRED,
  size TEXT NOT NULL,
  alignment TEXT NOT NULL,
  descriptor_size TEXT,
  descriptor_alignment TEXT,
  incomplete INTEGER NOT NULL,
  is_dependent INTEGER NOT NULL DEFAULT 0,
  is_atomic INTEGER NOT NULL DEFAULT 0,
  descriptor_incomplete INTEGER,
  used_in_exception_or_rtti INTEGER NOT NULL DEFAULT 0,
  declared_in_function_prototype INTEGER NOT NULL DEFAULT 0,
  is_tag_redefinition INTEGER NOT NULL DEFAULT 0,
  alignment_set_explicitly INTEGER NOT NULL DEFAULT 0,
  may_alias INTEGER NOT NULL DEFAULT 0,
  has_microsoft_w64_specifier INTEGER NOT NULL DEFAULT 0,
  is_microsoft_intrinsic INTEGER NOT NULL DEFAULT 0,
  autonomous_primary_tag_declaration INTEGER NOT NULL DEFAULT 0,
  is_builtin_va_list INTEGER NOT NULL DEFAULT 0,
  is_builtin_va_list_from_cstdarg INTEGER NOT NULL DEFAULT 0,
  has_gnu_abi_tag_attribute INTEGER NOT NULL DEFAULT 0,
  in_gnu_abi_tag_namespace INTEGER NOT NULL DEFAULT 0,
  use_cfront_transitional_nested_type_name_mangling INTEGER NOT NULL DEFAULT 0,
  typeinfo_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  size_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE arithmetic_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  arithmetic_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  edg_integer_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  edg_float_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  edg_fixed_precision_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  bits INTEGER,
  signed INTEGER,
  precision INTEGER,
  radix INTEGER,
  scale INTEGER,
  is_bool INTEGER NOT NULL DEFAULT 0,
  is_wchar INTEGER NOT NULL DEFAULT 0,
  is_char8 INTEGER NOT NULL DEFAULT 0,
  is_char16 INTEGER NOT NULL DEFAULT 0,
  is_char32 INTEGER NOT NULL DEFAULT 0,
  is_microsoft_sized_integer INTEGER NOT NULL DEFAULT 0,
  explicitly_signed INTEGER NOT NULL DEFAULT 0,
  affiliated_enum_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_fract_type INTEGER NOT NULL DEFAULT 0,
  is_saturating INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE qualified_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  underlying_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_const INTEGER NOT NULL DEFAULT 0,
  is_volatile INTEGER NOT NULL DEFAULT 0,
  is_restrict INTEGER NOT NULL DEFAULT 0,
  address_space INTEGER,
  is_shared INTEGER NOT NULL DEFAULT 0,
  shared_block_size TEXT,
  is_atomic INTEGER NOT NULL DEFAULT 0,
  is_nullable INTEGER NOT NULL DEFAULT 0,
  is_nonnull INTEGER NOT NULL DEFAULT 0,
  is_unaligned INTEGER NOT NULL DEFAULT 0,
  is_near INTEGER NOT NULL DEFAULT 0,
  is_far INTEGER NOT NULL DEFAULT 0,
  is_upc_strict INTEGER NOT NULL DEFAULT 0,
  is_upc_relaxed INTEGER NOT NULL DEFAULT 0,
  is_null_unspecified INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE pointer_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  pointee_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_reference INTEGER NOT NULL DEFAULT 0,
  is_rvalue_reference INTEGER NOT NULL DEFAULT 0,
  is_handle INTEGER NOT NULL DEFAULT 0,
  is_tracking_reference INTEGER NOT NULL DEFAULT 0,
  is_interior INTEGER NOT NULL DEFAULT 0,
  is_pinned INTEGER NOT NULL DEFAULT 0,
  is_ptr32 INTEGER NOT NULL DEFAULT 0,
  is_ptr64 INTEGER NOT NULL DEFAULT 0,
  is_sptr INTEGER NOT NULL DEFAULT 0,
  is_uptr INTEGER NOT NULL DEFAULT 0,
  based_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE member_pointer_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  declaring_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  member_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  original_declaring_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_ptr32 INTEGER NOT NULL DEFAULT 0,
  is_ptr64 INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE array_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  element_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  bound_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  bound TEXT,
  bound_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  vla_dimension_id INTEGER REFERENCES vla_dimension(id) DEFERRABLE INITIALLY DEFERRED,
  bound_is_zero INTEGER NOT NULL DEFAULT 0,
  is_template_dependent INTEGER NOT NULL DEFAULT 0,
  is_variable INTEGER NOT NULL DEFAULT 0,
  is_vla INTEGER NOT NULL DEFAULT 0,
  has_vla_dimension INTEGER NOT NULL DEFAULT 0,
  is_static INTEGER NOT NULL DEFAULT 0,
  is_threads_dimension INTEGER NOT NULL DEFAULT 0,
  bound_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  dependent_bound_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  is_const INTEGER NOT NULL DEFAULT 0,
  is_volatile INTEGER NOT NULL DEFAULT 0,
  is_restrict INTEGER NOT NULL DEFAULT 0,
  is_atomic INTEGER NOT NULL DEFAULT 0,
  is_nullable INTEGER NOT NULL DEFAULT 0,
  is_nonnull INTEGER NOT NULL DEFAULT 0,
  is_unaligned INTEGER NOT NULL DEFAULT 0,
  is_near INTEGER NOT NULL DEFAULT 0,
  is_far INTEGER NOT NULL DEFAULT 0,
  is_shared INTEGER NOT NULL DEFAULT 0,
  is_upc_strict INTEGER NOT NULL DEFAULT 0,
  is_upc_relaxed INTEGER NOT NULL DEFAULT 0,
  dependent_bound_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  address_space INTEGER,
  is_null_unspecified INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE routine_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  result_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  this_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  calling_convention_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  language_linkage_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  exception_specification_id INTEGER REFERENCES exception_specification(id) DEFERRABLE INITIALLY DEFERRED,
  is_variadic INTEGER NOT NULL DEFAULT 0,
  is_const INTEGER NOT NULL DEFAULT 0,
  is_volatile INTEGER NOT NULL DEFAULT 0,
  ref_qualifier_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  has_prototype INTEGER NOT NULL DEFAULT 1,
  old_style_params_scanned INTEGER NOT NULL DEFAULT 0,
  has_trailing_return_type INTEGER NOT NULL DEFAULT 0,
  lint_argsused_flag INTEGER NOT NULL DEFAULT 0,
  value_returned_by_cctor INTEGER NOT NULL DEFAULT 0,
  value_returned_as_parameter INTEGER,
  return_value_parameter_follows_this INTEGER,
  assoc_routine_is_ctor INTEGER NOT NULL DEFAULT 0,
  assoc_routine_is_dtor INTEGER NOT NULL DEFAULT 0,
  assoc_routine_is_lambda_body INTEGER NOT NULL DEFAULT 0,
  routine_name_linkage_is_explicit INTEGER NOT NULL DEFAULT 0,
  does_not_return INTEGER NOT NULL DEFAULT 0,
  has_enable_if_attribute INTEGER NOT NULL DEFAULT 0,
  result_should_be_used INTEGER,
  is_gnu_const INTEGER,
  is_variadic_instance INTEGER NOT NULL DEFAULT 0,
  explicit_calling_convention INTEGER,
  had_been_implicitly_const INTEGER NOT NULL DEFAULT 0,
  is_conditionally_explicit INTEGER NOT NULL DEFAULT 0,
  has_this_param INTEGER NOT NULL DEFAULT 0,
  associated_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  prototype_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  lint_varargs_count INTEGER NOT NULL DEFAULT -1,
  argument_pragma_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  format_argument INTEGER,
  first_format_substitution_argument INTEGER,
  sentinel_position INTEGER,
  is_restrict INTEGER NOT NULL DEFAULT 0,
  is_atomic INTEGER NOT NULL DEFAULT 0,
  is_nullable INTEGER NOT NULL DEFAULT 0,
  is_nonnull INTEGER NOT NULL DEFAULT 0,
  is_null_unspecified INTEGER NOT NULL DEFAULT 0,
  is_unaligned INTEGER NOT NULL DEFAULT 0,
  is_near INTEGER NOT NULL DEFAULT 0,
  is_far INTEGER NOT NULL DEFAULT 0,
  is_shared INTEGER NOT NULL DEFAULT 0,
  is_upc_strict INTEGER NOT NULL DEFAULT 0,
  is_upc_relaxed INTEGER NOT NULL DEFAULT 0,
  address_space INTEGER,
  is_this_const INTEGER NOT NULL DEFAULT 0,
  is_this_volatile INTEGER NOT NULL DEFAULT 0,
  is_this_restrict INTEGER NOT NULL DEFAULT 0,
  is_this_atomic INTEGER NOT NULL DEFAULT 0,
  is_this_nullable INTEGER NOT NULL DEFAULT 0,
  is_this_nonnull INTEGER NOT NULL DEFAULT 0,
  is_this_null_unspecified INTEGER NOT NULL DEFAULT 0,
  is_this_unaligned INTEGER NOT NULL DEFAULT 0,
  is_this_near INTEGER NOT NULL DEFAULT 0,
  is_this_far INTEGER NOT NULL DEFAULT 0,
  is_this_shared INTEGER NOT NULL DEFAULT 0,
  is_this_upc_strict INTEGER NOT NULL DEFAULT 0,
  is_this_upc_relaxed INTEGER NOT NULL DEFAULT 0,
  this_address_space INTEGER
) STRICT;

CREATE TABLE parameter_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  declared_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  parameter_number INTEGER NOT NULL,
  default_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  has_name_conflict INTEGER NOT NULL DEFAULT 0,
  passed_via_copy_constructor INTEGER NOT NULL DEFAULT 0,
  has_default_argument INTEGER NOT NULL DEFAULT 0,
  default_argument_in_class INTEGER NOT NULL DEFAULT 0,
  has_unevaluated_template_default INTEGER NOT NULL DEFAULT 0,
  type_involves_deduced_template_parameter INTEGER NOT NULL DEFAULT 0,
  type_involves_template_parameter INTEGER NOT NULL DEFAULT 0,
  is_parameter_pack INTEGER NOT NULL DEFAULT 0,
  is_pack_element INTEGER NOT NULL DEFAULT 0,
  was_nontrailing_pack INTEGER NOT NULL DEFAULT 0,
  is_auto_parameter INTEGER NOT NULL DEFAULT 0,
  duplicate_name INTEGER NOT NULL DEFAULT 0,
  move_constructor_or_assignment_parameter INTEGER NOT NULL DEFAULT 0,
  copy_or_move_constructor_parameter INTEGER NOT NULL DEFAULT 0,
  is_requires_expression_parameter INTEGER NOT NULL DEFAULT 0,
  is_explicit_this INTEGER NOT NULL DEFAULT 0,
  is_transparent INTEGER NOT NULL DEFAULT 0,
  is_nonnull INTEGER NOT NULL DEFAULT 0,
  is_cli_parameter_array INTEGER NOT NULL DEFAULT 0,
  is_const INTEGER NOT NULL DEFAULT 0,
  is_volatile INTEGER NOT NULL DEFAULT 0,
  is_restrict INTEGER NOT NULL DEFAULT 0,
  is_atomic INTEGER NOT NULL DEFAULT 0,
  is_nullable INTEGER NOT NULL DEFAULT 0,
  is_nonnull_qualified INTEGER NOT NULL DEFAULT 0,
  is_null_unspecified INTEGER NOT NULL DEFAULT 0,
  is_unaligned INTEGER NOT NULL DEFAULT 0,
  is_near INTEGER NOT NULL DEFAULT 0,
  is_far INTEGER NOT NULL DEFAULT 0,
  is_upc_shared INTEGER NOT NULL DEFAULT 0,
  is_upc_strict INTEGER NOT NULL DEFAULT 0,
  is_upc_relaxed INTEGER NOT NULL DEFAULT 0,
  named_address_space INTEGER
) STRICT;

CREATE TABLE parameter_default_entity (
  owner_id INTEGER REFERENCES parameter_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE routine_type_parameter (
  owner_id INTEGER REFERENCES routine_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  parameter_type_id INTEGER REFERENCES parameter_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  definition_state_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  instance_id INTEGER REFERENCES template_instance(id) DEFERRABLE INITIALLY DEFERRED,
  subobject_partner_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_lambda INTEGER NOT NULL DEFAULT 0,
  is_empty INTEGER NOT NULL DEFAULT 0,
  is_abstract INTEGER NOT NULL DEFAULT 0,
  is_final INTEGER NOT NULL DEFAULT 0,
  any_const_member INTEGER NOT NULL DEFAULT 0,
  any_volatile_member INTEGER NOT NULL DEFAULT 0,
  any_mutable_member INTEGER NOT NULL DEFAULT 0,
  any_virtual_base_classes INTEGER NOT NULL DEFAULT 0,
  any_virtual_functions INTEGER NOT NULL DEFAULT 0,
  any_pure_virtual_functions INTEGER NOT NULL DEFAULT 0,
  any_virtual_functions_including_bases INTEGER NOT NULL DEFAULT 0,
  nested_class_defined_outside_parent INTEGER NOT NULL DEFAULT 0,
  originally_unnamed INTEGER NOT NULL DEFAULT 0,
  is_nonstandard_anonymous_union INTEGER NOT NULL DEFAULT 0,
  is_template_class INTEGER NOT NULL DEFAULT 0,
  is_nonreal_class INTEGER NOT NULL DEFAULT 0,
  is_ms_instantiated_nonreal_class INTEGER NOT NULL DEFAULT 0,
  is_prototype_instantiation INTEGER NOT NULL DEFAULT 0,
  is_specialized INTEGER NOT NULL DEFAULT 0,
  specialized_with_old_syntax INTEGER NOT NULL DEFAULT 0,
  is_in_class_specialization INTEGER NOT NULL DEFAULT 0,
  explicitly_instantiated INTEGER NOT NULL DEFAULT 0,
  do_not_instantiate INTEGER NOT NULL DEFAULT 0,
  is_proxy_class INTEGER NOT NULL DEFAULT 0,
  no_proper_data INTEGER NOT NULL DEFAULT 0,
  has_zero_init_component INTEGER NOT NULL DEFAULT 0,
  has_pointer_component INTEGER NOT NULL DEFAULT 0,
  contains_flexible_array_member INTEGER NOT NULL DEFAULT 0,
  is_transparent INTEGER NOT NULL DEFAULT 0,
  is_packed INTEGER NOT NULL DEFAULT 0,
  has_internal_linkage_attribute INTEGER NOT NULL DEFAULT 0,
  has_operator_ampersand INTEGER NOT NULL DEFAULT 0,
  copy_assignment_declaration_suppressed INTEGER NOT NULL DEFAULT 0,
  copy_constructor_declaration_suppressed INTEGER NOT NULL DEFAULT 0,
  default_constructor_declaration_suppressed INTEGER NOT NULL DEFAULT 0,
  destructor_declaration_suppressed INTEGER NOT NULL DEFAULT 0,
  maximum_member_alignment TEXT,
  is_interface INTEGER NOT NULL DEFAULT 0,
  is_interface_like INTEGER NOT NULL DEFAULT 0,
  is_sealed INTEGER NOT NULL DEFAULT 0,
  compiler_generated INTEGER,
  has_subobject_type INTEGER,
  is_generic_lambda_closure_class INTEGER NOT NULL DEFAULT 0,
  has_lambda_conversion_function INTEGER NOT NULL DEFAULT 0,
  is_initializer_list INTEGER NOT NULL DEFAULT 0,
  has_initializer_list_ctor INTEGER NOT NULL DEFAULT 0,
  has_anonymous_union_member INTEGER NOT NULL DEFAULT 0,
  defined_in_variable_initializer INTEGER NOT NULL DEFAULT 0,
  defined_in_field_initializer INTEGER NOT NULL DEFAULT 0,
  named_in_inline_template_directive INTEGER NOT NULL DEFAULT 0,
  is_va_list_tag INTEGER NOT NULL DEFAULT 0,
  defined_in_parent_class INTEGER NOT NULL DEFAULT 0,
  has_nodiscard_attribute INTEGER NOT NULL DEFAULT 0,
  has_field_initializer INTEGER NOT NULL DEFAULT 0,
  contains_error INTEGER,
  contains_local_type INTEGER,
  contains_unnamed_namespace_type INTEGER,
  does_not_contain_parentless_lambda_in_default_argument INTEGER,
  does_not_contain_deprecated_or_unavailable_type INTEGER,
  inheritance_kind_is_explicit INTEGER,
  has_coclass_attribute INTEGER,
  has_explicitly_aligned_subobject INTEGER,
  definition_needed INTEGER,
  primary_base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_function_info_base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_function_info_offset TEXT,
  highest_virtual_function_number TEXT,
  next_negative_virtual_table_index TEXT,
  first_vcall_offset_index TEXT,
  virtual_function_table_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_table_table_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  uuid_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  uuid_string_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  original_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  inheritance_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  elf_visibility_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  anonymous_union_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  anonymous_union_field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  allocation_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  deallocation_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  minimum_template_arguments INTEGER NOT NULL DEFAULT -1,
  lambda_parent_entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  proxy_of_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_near INTEGER,
  is_far INTEGER
) STRICT;

CREATE TABLE type_reference (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  underlying_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  reference_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  decltype_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  instance_id INTEGER REFERENCES template_instance(id) DEFERRABLE INITIALLY DEFERRED,
  is_predeclared INTEGER NOT NULL DEFAULT 0,
  has_variably_modified_type INTEGER NOT NULL DEFAULT 0,
  is_lowered_variably_modified_type INTEGER NOT NULL DEFAULT 0,
  decltype_expression_not_parenthesized INTEGER NOT NULL DEFAULT 0,
  is_dependent_type_operator INTEGER NOT NULL DEFAULT 0,
  is_nonreal INTEGER NOT NULL DEFAULT 0,
  is_dependent INTEGER NOT NULL DEFAULT 0,
  is_prototype_instantiation INTEGER NOT NULL DEFAULT 0,
  is_lowered_complex_type INTEGER NOT NULL DEFAULT 0,
  embeds_source_entries INTEGER NOT NULL DEFAULT 0,
  added_to_record_name INTEGER NOT NULL DEFAULT 0,
  has_typename_prefix INTEGER NOT NULL DEFAULT 0,
  is_global_qualified_name INTEGER NOT NULL DEFAULT 0,
  is_intrinsic_member INTEGER NOT NULL DEFAULT 0,
  explicit_memory_attribute_made_implicit INTEGER NOT NULL DEFAULT 0,
  name_qualifier_id INTEGER REFERENCES name_qualifier(id) DEFERRABLE INITIALLY DEFERRED,
  proxy_class_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  operator_type_argument_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  minimum_template_arguments INTEGER,
  type_id_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE vector_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  element_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  element_count TEXT,
  vector_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  size_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  is_boolean INTEGER NOT NULL DEFAULT 0,
  tuple_elements INTEGER,
  length_multiplier INTEGER,
  edg_vector_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE dependent_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  parameter_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  parameter_number INTEGER,
  parameter_depth INTEGER,
  parent_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  constraint_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  bit_width_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  is_pack INTEGER NOT NULL DEFAULT 0,
  is_auto_parameter INTEGER NOT NULL DEFAULT 0,
  is_decltype_auto INTEGER NOT NULL DEFAULT 0,
  originally_class_template_parameter INTEGER NOT NULL DEFAULT 0,
  is_unsigned_bit_precise_integer INTEGER NOT NULL DEFAULT 0,
  proxy_class_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE managed_type (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  managed_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  underlying_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  rank INTEGER,
  assembly_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE record_layout (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  nonvirtual_size TEXT,
  nonvirtual_alignment TEXT,
  is_complete INTEGER NOT NULL
) STRICT;

CREATE TABLE field_layout (
  owner_id INTEGER REFERENCES record_layout(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  offset TEXT,
  bit_offset TEXT,
  bit_width TEXT,
  container_offset TEXT,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE direct_base (
  owner_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  base_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  access_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  is_virtual INTEGER NOT NULL DEFAULT 0,
  subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE base_subobject (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  most_derived_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  base_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  offset TEXT,
  is_virtual INTEGER NOT NULL DEFAULT 0,
  is_direct INTEGER NOT NULL DEFAULT 0,
  ambiguous INTEGER NOT NULL DEFAULT 0,
  shares_virtual_function_info INTEGER NOT NULL DEFAULT 0,
  ignore_during_dependent_lookup INTEGER NOT NULL DEFAULT 0,
  is_optimized_empty_base INTEGER NOT NULL DEFAULT 0,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  has_public_derivation INTEGER NOT NULL DEFAULT 0,
  complete_subobject INTEGER,
  original_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  direct_base_number INTEGER NOT NULL DEFAULT 0,
  primary_base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  data_section_base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_base_pointer_offset TEXT,
  virtual_base_pointer_base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_function_table_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  virtual_function_table_index TEXT,
  construction_table_index TEXT,
  construction_subarray_index TEXT,
  virtual_base_offset_index TEXT
) STRICT;

CREATE TABLE base_derivation (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  is_direct INTEGER NOT NULL DEFAULT 0,
  is_preferred INTEGER NOT NULL DEFAULT 0,
  access_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  path_tail_ordinal INTEGER NOT NULL
) STRICT;

CREATE TABLE base_derivation_step (
  owner_id INTEGER REFERENCES base_derivation(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE override_relation (
  owner_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  overridden_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  overriding_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  return_adjustment_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  base_ordinal INTEGER,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE semantic_node (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  compiler_generated INTEGER NOT NULL DEFAULT 0,
  representation_role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE structural_child (
  owner_id INTEGER REFERENCES semantic_node(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  child_id INTEGER REFERENCES semantic_node(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, role_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE semantic_reference (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, role_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  value_category_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  original_lvalue_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  result_unused INTEGER NOT NULL DEFAULT 0,
  initialization_guard INTEGER NOT NULL DEFAULT 0,
  generated_default_argument INTEGER NOT NULL DEFAULT 0,
  volatile_fetch INTEGER NOT NULL DEFAULT 0,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  is_parenthesized INTEGER NOT NULL DEFAULT 0,
  is_static_cast INTEGER NOT NULL DEFAULT 0,
  is_functional_cast INTEGER NOT NULL DEFAULT 0,
  is_brace_cast INTEGER NOT NULL DEFAULT 0,
  is_safe_cast INTEGER NOT NULL DEFAULT 0,
  is_cli_typeid INTEGER NOT NULL DEFAULT 0,
  is_cli_param_array_element INTEGER NOT NULL DEFAULT 0,
  is_objectless_member_reference INTEGER NOT NULL DEFAULT 0,
  is_gnu_extension INTEGER NOT NULL DEFAULT 0,
  type_definition_needed INTEGER NOT NULL DEFAULT 0,
  boolean_not_normalized INTEGER,
  is_type_constraint INTEGER NOT NULL DEFAULT 0,
  was_lvalue_temp_initializer INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  parent_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  is_guard INTEGER NOT NULL DEFAULT 0,
  lowering_generated INTEGER,
  has_associated_pragma INTEGER NOT NULL DEFAULT 0,
  is_lowering_boilerplate INTEGER,
  is_fallthrough_statement INTEGER NOT NULL DEFAULT 0,
  is_likely INTEGER NOT NULL DEFAULT 0,
  is_unlikely INTEGER NOT NULL DEFAULT 0,
  source_sequence_entry_id INTEGER REFERENCES source_entry(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE expression_operand (
  owner_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, role_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE operator_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  operator_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  operation_type_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  eval_left_to_right INTEGER NOT NULL DEFAULT 0,
  eval_right_to_left INTEGER NOT NULL DEFAULT 0,
  pointer_operand_is_second INTEGER NOT NULL DEFAULT 0,
  is_reinterpret_like_cast INTEGER NOT NULL DEFAULT 0,
  requires_runtime_cast_check INTEGER NOT NULL DEFAULT 0,
  returns_lvalue INTEGER NOT NULL DEFAULT 0,
  is_reinterpret_cast INTEGER NOT NULL DEFAULT 0,
  is_reference_cast INTEGER NOT NULL DEFAULT 0,
  is_rvalue_reference_cast INTEGER NOT NULL DEFAULT 0,
  is_const_cast INTEGER NOT NULL DEFAULT 0,
  is_tracking_reference_cast INTEGER NOT NULL DEFAULT 0,
  is_gnu_two_operand_conditional INTEGER NOT NULL DEFAULT 0,
  call_uses_operator_syntax INTEGER NOT NULL DEFAULT 0,
  implicit_member_naming INTEGER NOT NULL DEFAULT 0,
  implicit_explicit_cast_step INTEGER NOT NULL DEFAULT 0,
  is_conversion_call INTEGER NOT NULL DEFAULT 0,
  argument_dependent_lookup_suppressed INTEGER NOT NULL DEFAULT 0,
  qualified_function_name INTEGER NOT NULL DEFAULT 0,
  retained_virtual_call INTEGER NOT NULL DEFAULT 0,
  rewritten_property_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE call_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  callee_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  source_target_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  receiver_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  function_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  call_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_dispatch_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  return_slot_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE call_argument (
  owner_id INTEGER REFERENCES call_expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parameter_type_id INTEGER REFERENCES parameter_type(id) DEFERRABLE INITIALLY DEFERRED,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE reference_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  reference_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  name_reference_id INTEGER REFERENCES name_reference(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE name_reference (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  property_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  destructor_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  qualifier_id INTEGER REFERENCES name_qualifier(id) DEFERRABLE INITIALLY DEFERRED,
  num_template_arguments TEXT,
  is_global_qualified_name INTEGER NOT NULL DEFAULT 0,
  is_template_id INTEGER NOT NULL DEFAULT 0,
  is_super_qualified INTEGER NOT NULL DEFAULT 0,
  is_decltype_qualified INTEGER NOT NULL DEFAULT 0,
  from_prototype_instantiation INTEGER NOT NULL DEFAULT 0,
  used_in_primary_declarator INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE name_qualifier (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  previous_qualifier_id INTEGER REFERENCES name_qualifier(id) DEFERRABLE INITIALLY DEFERRED,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  is_class INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE original_template_argument (
  owner_id INTEGER REFERENCES name_reference(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE constant_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  name_reference_id INTEGER REFERENCES name_reference(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE block_descriptor (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  final_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  end_reachable INTEGER NOT NULL DEFAULT 0,
  is_statement_expression INTEGER NOT NULL DEFAULT 0,
  implicit_scope_not_allowed INTEGER NOT NULL DEFAULT 0,
  upc_access_method_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE block_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  descriptor_id INTEGER REFERENCES block_descriptor(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE declaration_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  has_static_or_thread_variable INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE declaration_statement_entity (
  owner_id INTEGER REFERENCES declaration_statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE initialization_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE block_item (
  owner_id INTEGER REFERENCES block_statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  statement_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE if_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  then_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  else_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  condition_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  selection_known INTEGER,
  selected_then INTEGER
) STRICT;

CREATE TABLE loop_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  condition_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  increment_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  init_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  affinity_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE range_for (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  range_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  begin_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  end_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  iterator_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  comparison_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  increment_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  iterator_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  use_await INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE for_each_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  collection_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  pattern_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  iterator_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  test_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  increment_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  uses_previous_declaration_iterator INTEGER NOT NULL DEFAULT 0,
  previous_field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  assignment_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  collection_reference_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  temporary_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  end_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  iterator_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE switch_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  selector_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  descriptor_id INTEGER REFERENCES switch_descriptor(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE switch_descriptor (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  default_case_id INTEGER REFERENCES switch_case_entry(id) DEFERRABLE INITIALLY DEFERRED,
  has_sorted_cases INTEGER NOT NULL
) STRICT;

CREATE TABLE switch_case_entry (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  statement_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  low_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  high_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  reachable_by_fall_through INTEGER NOT NULL,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE switch_descriptor_case (
  owner_id INTEGER REFERENCES switch_descriptor(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  case_id INTEGER REFERENCES switch_case_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE switch_descriptor_sorted_case (
  owner_id INTEGER REFERENCES switch_descriptor(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  case_id INTEGER REFERENCES switch_case_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE switch_case_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  entry_id INTEGER REFERENCES switch_case_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  switch_statement_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE jump_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  value_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  label_id INTEGER REFERENCES label_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  common_lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE label_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  label_id INTEGER REFERENCES label_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE try_descriptor (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_function_try INTEGER NOT NULL DEFAULT 0,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE try_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  descriptor_id INTEGER REFERENCES try_descriptor(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE try_handler (
  owner_id INTEGER REFERENCES try_descriptor(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  handler_id INTEGER REFERENCES handler(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE handler (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  caught_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  parameter_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  parameter_initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  typeinfo_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE microsoft_try_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  body_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  filter_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  handler_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  finally_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  except_or_finally_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  is_finally INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE throw_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  operand_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  thrown_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  destructor_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_rethrow INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE throw_accessible_base (
  owner_id INTEGER REFERENCES throw_expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE type_query_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  query_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  operand_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  operand_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  is_dynamic INTEGER NOT NULL DEFAULT 0,
  is_standard_alignof INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE temporary_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  source_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE condition_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED,
  dynamic_initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE lifetime_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE statement_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  statement_id INTEGER REFERENCES statement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE reuse_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE builtin_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  selected_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE parameter_reference_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  parameter_number INTEGER NOT NULL,
  parameter_depth INTEGER NOT NULL,
  is_this INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE dependent_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  wrapped_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  noexcept_required INTEGER,
  pack_index_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  pack_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  pack_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  fold_operator_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  fold_left_associative INTEGER,
  token_sequence_id INTEGER REFERENCES token_sequence(id) DEFERRABLE INITIALLY DEFERRED,
  reattempt_default_argument INTEGER,
  reattempt_default_member_initializer INTEGER
) STRICT;

CREATE TABLE requirement_parameter (
  owner_id INTEGER REFERENCES dependent_expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  parameter_type_id INTEGER REFERENCES parameter_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE expression_template_argument (
  owner_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE token_sequence (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE token_sequence_token (
  owner_id INTEGER REFERENCES token_sequence(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  spelling_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE dependent_requirement (
  owner_id INTEGER REFERENCES dependent_expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE allocation_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  allocated_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  allocator_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  deallocator_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  array_count_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  operand_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  is_delete INTEGER NOT NULL DEFAULT 0,
  is_array INTEGER NOT NULL DEFAULT 0,
  is_global INTEGER NOT NULL DEFAULT 0,
  is_gcnew INTEGER NOT NULL DEFAULT 0,
  is_placement INTEGER NOT NULL DEFAULT 0,
  is_aligned INTEGER NOT NULL DEFAULT 0,
  has_initializer INTEGER NOT NULL DEFAULT 0,
  brace_initializer INTEGER NOT NULL DEFAULT 0,
  paren_aggregate_initializer INTEGER NOT NULL DEFAULT 0,
  deducible_type INTEGER NOT NULL DEFAULT 0,
  parenthesized_type INTEGER NOT NULL DEFAULT 0,
  exception_freeing_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE allocation_argument (
  owner_id INTEGER REFERENCES allocation_expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE lambda_definition (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  closure_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  call_operator_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  capture_end_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  mutable_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  constexpr_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  is_generic INTEGER NOT NULL DEFAULT 0,
  is_mutable INTEGER NOT NULL DEFAULT 0,
  constexpr_specified INTEGER NOT NULL DEFAULT 0,
  consteval_specified INTEGER NOT NULL DEFAULT 0,
  has_capture_default INTEGER NOT NULL DEFAULT 0,
  default_is_reference INTEGER NOT NULL DEFAULT 0,
  explicit_return_type INTEGER NOT NULL DEFAULT 0,
  has_parameter_declaration INTEGER NOT NULL DEFAULT 0,
  has_template_parameter_list INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE lambda_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  lambda_id INTEGER REFERENCES lambda_definition(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE lambda_capture (
  owner_id INTEGER REFERENCES lambda_definition(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  source_closure_field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  indirect_capture_field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  is_pack INTEGER NOT NULL DEFAULT 0,
  is_init_capture INTEGER NOT NULL DEFAULT 0,
  is_indirect_init_capture INTEGER NOT NULL DEFAULT 0,
  is_parameter_reference INTEGER NOT NULL DEFAULT 0,
  capture_by_reference INTEGER NOT NULL DEFAULT 0,
  is_implicit INTEGER NOT NULL DEFAULT 0,
  is_pack_element INTEGER NOT NULL DEFAULT 0,
  direct_initialization INTEGER NOT NULL DEFAULT 0,
  parenthesized_initialization INTEGER NOT NULL DEFAULT 0,
  const_capture INTEGER NOT NULL DEFAULT 0,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE coroutine (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  promise_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  handle_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initial_suspend_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  final_suspend_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  return_object_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  exception_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  traits_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  initial_await_resume_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  this_parameter_copy_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  final_suspend_label_id INTEGER REFERENCES label_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  allocation_failure_call_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  allocator_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  deallocator_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  error_descriptor INTEGER NOT NULL DEFAULT 0,
  has_return_void INTEGER NOT NULL DEFAULT 0,
  body_generated INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE coroutine_parameter_copy (
  owner_id INTEGER REFERENCES coroutine(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  copy_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE coroutine_operation (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  operand_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  ready_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  suspend_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  resume_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE vla_dimension (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  original_dimension_id INTEGER REFERENCES vla_dimension(id) DEFERRABLE INITIALLY DEFERRED,
  dimension_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  in_prototype_scope INTEGER NOT NULL DEFAULT 0,
  has_size_statement INTEGER NOT NULL DEFAULT 0,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE vla_operation (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  dimension_id INTEGER REFERENCES vla_dimension(id) DEFERRABLE INITIALLY DEFERRED,
  is_typedef_declaration INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE asm_entry (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  asm_string_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  gnu_asm_form INTEGER NOT NULL DEFAULT 0,
  is_volatile INTEGER NOT NULL DEFAULT 0,
  has_volatile_keyword INTEGER NOT NULL DEFAULT 0,
  has_inline_keyword INTEGER NOT NULL DEFAULT 0,
  is_goto INTEGER NOT NULL DEFAULT 0,
  number_of_constraints TEXT
) STRICT;

CREATE TABLE asm_statement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  entry_id INTEGER REFERENCES asm_entry(id) DEFERRABLE INITIALLY DEFERRED,
  text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE asm_operand (
  owner_id INTEGER REFERENCES asm_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  constraint_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  modifier_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE asm_operand_constraint (
  owner_id INTEGER REFERENCES asm_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  operand_ordinal INTEGER NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  condition_code_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, operand_ordinal, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE asm_clobber (
  owner_id INTEGER REFERENCES asm_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  register_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE asm_label (
  owner_id INTEGER REFERENCES asm_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  label_id INTEGER REFERENCES label_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE owner_attribute (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  attribute_id INTEGER REFERENCES attribute(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE attribute (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  family_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  location_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  namespace_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  group_id INTEGER REFERENCES attribute_group(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  on_primary_declaration INTEGER NOT NULL DEFAULT 0,
  transforms_type_specifier INTEGER NOT NULL DEFAULT 0,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  is_standard_gcc INTEGER NOT NULL DEFAULT 0,
  is_implicit_abi_tag INTEGER NOT NULL DEFAULT 0,
  namespace_from_using INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE attribute_group (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE attribute_argument_link (
  owner_id INTEGER REFERENCES attribute(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES attribute_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE attribute_argument (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  token_text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  token_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  is_local_expression_reference INTEGER NOT NULL DEFAULT 0,
  scoped_expression_id INTEGER REFERENCES scoped_expression(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE constant (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  orig_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  backing_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  name_reference_id INTEGER REFERENCES name_reference(id) DEFERRABLE INITIALLY DEFERRED,
  character_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  implicit_cast INTEGER NOT NULL DEFAULT 0,
  explicit_cast_applied INTEGER NOT NULL DEFAULT 0,
  is_reinterpret_cast INTEGER NOT NULL DEFAULT 0,
  is_reinterpret_like_cast INTEGER NOT NULL DEFAULT 0,
  is_simple_zero INTEGER NOT NULL DEFAULT 0,
  associated_storage_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  non_arithmetic INTEGER NOT NULL DEFAULT 0,
  null_pointer_constant_ruled_out INTEGER NOT NULL DEFAULT 0,
  nullptr_keyword INTEGER NOT NULL DEFAULT 0,
  explicit_braces_on_aggregate INTEGER NOT NULL DEFAULT 0,
  explicit_parentheses_on_aggregate INTEGER NOT NULL DEFAULT 0,
  from_undefined_preproc_id INTEGER NOT NULL DEFAULT 0,
  uses_designated_initializers INTEGER NOT NULL DEFAULT 0,
  is_named_constant_definition INTEGER NOT NULL DEFAULT 0,
  partial_aggr_value INTEGER NOT NULL DEFAULT 0,
  is_partially_initialized INTEGER NOT NULL DEFAULT 0,
  implicit_aggr_element INTEGER NOT NULL DEFAULT 0,
  is_compound_literal INTEGER NOT NULL DEFAULT 0,
  is_result_of_constexpr_call INTEGER NOT NULL DEFAULT 0,
  is_generic_initializer INTEGER NOT NULL DEFAULT 0,
  constant_for_base_class INTEGER NOT NULL DEFAULT 0,
  constant_for_base_class_from_constexpr_folding INTEGER NOT NULL DEFAULT 0,
  folded_statement_expression INTEGER NOT NULL DEFAULT 0,
  formed_from_promoted_storage INTEGER NOT NULL DEFAULT 0,
  local_expr_ref INTEGER NOT NULL DEFAULT 0,
  null_keyword INTEGER,
  native_nullptr_keyword INTEGER,
  ptr_to_mem_constant_construct INTEGER,
  flexible_array_initializer INTEGER,
  initializes_empty_object INTEGER,
  is_implicit_initialization INTEGER,
  literal_encoding_prefix_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  is_raw_string INTEGER,
  is_string_literal INTEGER,
  is_function_name_literal INTEGER,
  contains_function_name_token INTEGER,
  is_embed_expansion INTEGER,
  string_sequence_number TEXT,
  embed_directive_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  has_dynamic_init_component INTEGER,
  added_const_for_template_param INTEGER,
  initialized_field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initialized_base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  template_local_expr_ref INTEGER,
  vptr_has_been_lowered INTEGER
) STRICT;

CREATE TABLE constant_value (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE constant_component (
  owner_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, role_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE integer_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  value_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE floating_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  class_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  negative INTEGER NOT NULL,
  radix INTEGER NOT NULL,
  significand_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  exponent INTEGER,
  nan_payload_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  nan_quiet INTEGER,
  representation_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE fixed_point_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  scaled_integer_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  scale INTEGER NOT NULL
) STRICT;

CREATE TABLE complex_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  real_value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  imaginary_value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE string_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  element_bits INTEGER NOT NULL,
  length TEXT NOT NULL,
  encoding_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  unit_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE string_unit (
  owner_id INTEGER REFERENCES string_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  unit INTEGER NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE address_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  base_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  base_entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  offset TEXT NOT NULL,
  one_past_end INTEGER NOT NULL DEFAULT 0,
  subobject_path_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  is_object_reflection INTEGER NOT NULL DEFAULT 0,
  parameter_number INTEGER
) STRICT;

CREATE TABLE stack_offset_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  offset TEXT NOT NULL
) STRICT;

CREATE TABLE member_pointer_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  member_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  containing_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  is_function_ptr INTEGER NOT NULL DEFAULT 0,
  cast_to_base INTEGER NOT NULL DEFAULT 0,
  is_null INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE label_difference_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  left_label_id INTEGER REFERENCES label_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  right_label_id INTEGER REFERENCES label_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  from_value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  from_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  to_value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  to_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE aggregate_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE aggregate_element (
  owner_id INTEGER REFERENCES aggregate_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE repeat_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  count TEXT NOT NULL,
  multidimensional_tail_not_repeated INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE designator_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  field_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  is_generic INTEGER NOT NULL DEFAULT 0,
  uses_direct_init_syntax INTEGER NOT NULL DEFAULT 0,
  index_value TEXT
) STRICT;

CREATE TABLE designator_subscript (
  owner_id INTEGER REFERENCES designator_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE dynamic_initialization_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE dependent_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parameter_number INTEGER,
  parameter_depth INTEGER,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  constant_value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  constant_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_qualified_name INTEGER NOT NULL DEFAULT 0,
  has_address_of INTEGER NOT NULL DEFAULT 0,
  is_pack INTEGER NOT NULL DEFAULT 0,
  has_generic_cast INTEGER NOT NULL DEFAULT 0,
  conversion_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  opname_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  special_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  is_standard_alignof INTEGER NOT NULL DEFAULT 0,
  destructor_unqualified INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE dependent_constant_element (
  owner_id INTEGER REFERENCES dependent_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  value_id INTEGER REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE dependent_template_argument (
  owner_id INTEGER REFERENCES dependent_value(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE reflection_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  local_scope_number TEXT
) STRICT;

CREATE TABLE subobject_path (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  element_offset TEXT,
  next_id INTEGER REFERENCES subobject_path(id) DEFERRABLE INITIALLY DEFERRED,
  is_converted INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE initialization (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  lambda_id INTEGER REFERENCES lambda_definition(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  constructor_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  destructor_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  init_expression_lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  master_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  next_destruction_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  overlapping_lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  source_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  is_unordered INTEGER NOT NULL DEFAULT 0,
  is_conditional INTEGER NOT NULL DEFAULT 0,
  is_overlapping INTEGER NOT NULL DEFAULT 0,
  is_class_result_master INTEGER NOT NULL DEFAULT 0,
  is_static_temporary INTEGER NOT NULL DEFAULT 0,
  is_partial_aggregate INTEGER NOT NULL DEFAULT 0,
  is_partial_initialization INTEGER NOT NULL DEFAULT 0,
  follows_executable_statement INTEGER NOT NULL DEFAULT 0,
  has_temporary_lifetime INTEGER NOT NULL DEFAULT 0,
  is_constructor_initialization INTEGER NOT NULL DEFAULT 0,
  is_conditional_guard INTEGER NOT NULL DEFAULT 0,
  is_vla_deallocation INTEGER NOT NULL DEFAULT 0,
  creates_initializer_list INTEGER NOT NULL DEFAULT 0,
  is_initializer_list_array INTEGER NOT NULL DEFAULT 0,
  is_reused_value INTEGER NOT NULL DEFAULT 0,
  is_question_result INTEGER NOT NULL DEFAULT 0,
  is_comma_result INTEGER NOT NULL DEFAULT 0,
  constructor_copy_implied_source INTEGER NOT NULL DEFAULT 0,
  constructor_implicit_copy INTEGER NOT NULL DEFAULT 0,
  constructor_value_initialization INTEGER NOT NULL DEFAULT 0,
  constructor_sequenced_arguments INTEGER NOT NULL DEFAULT 0,
  constructor_array_copy INTEGER NOT NULL DEFAULT 0,
  constant_nonconstant INTEGER NOT NULL DEFAULT 0,
  frees_storage INTEGER NOT NULL DEFAULT 0,
  frees_array INTEGER NOT NULL DEFAULT 0,
  is_explicit_cast INTEGER NOT NULL DEFAULT 0,
  is_braced INTEGER NOT NULL DEFAULT 0,
  is_compound_literal INTEGER NOT NULL DEFAULT 0,
  is_freeing_of_exception_object INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE initialization_operand (
  owner_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, role_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE lifetime (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parent_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED,
  bound_entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  parent_checkpoint_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  has_implicit_child INTEGER NOT NULL DEFAULT 0,
  has_block_after_label_child INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE lifetime_child (
  owner_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  child_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE lifetime_cleanup (
  owner_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE exception_specification (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  noexcept_argument_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  copy_from_prototype_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  argument_cached INTEGER NOT NULL DEFAULT 0,
  from_attribute INTEGER NOT NULL DEFAULT 0,
  compiler_generated INTEGER NOT NULL DEFAULT 0,
  throw_any INTEGER NOT NULL DEFAULT 0,
  noexcept_effective INTEGER,
  is_dependent INTEGER NOT NULL DEFAULT 0,
  is_indeterminate INTEGER NOT NULL DEFAULT 0,
  copy_from_prototype INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE exception_specification_type (
  owner_id INTEGER REFERENCES exception_specification(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_redundant INTEGER NOT NULL DEFAULT 0,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE function_eh (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  prologue_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  region_table_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  array_table_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  initial_state TEXT,
  null_state TEXT
) STRICT;

CREATE TABLE eh_table_projection (
  owner_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  function_eh_id INTEGER REFERENCES function_eh(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id)
) STRICT;

CREATE TABLE cleanup_region (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  function_eh_id INTEGER REFERENCES function_eh(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  next_region_id INTEGER REFERENCES cleanup_region(id) DEFERRABLE INITIALLY DEFERRED,
  condition_id INTEGER REFERENCES cleanup_condition(id) DEFERRABLE INITIALLY DEFERRED,
  is_supplement INTEGER NOT NULL DEFAULT 0,
  supplement_owner_id INTEGER REFERENCES cleanup_region(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE cleanup_action (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  region_id INTEGER REFERENCES cleanup_region(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  storage_id INTEGER REFERENCES cleanup_storage_location(id) DEFERRABLE INITIALLY DEFERRED,
  array_id INTEGER REFERENCES cleanup_array(id) DEFERRABLE INITIALLY DEFERRED,
  abi_detail_id INTEGER REFERENCES cleanup_abi_detail(id) DEFERRABLE INITIALLY DEFERRED,
  is_array INTEGER NOT NULL DEFAULT 0,
  is_indirect INTEGER NOT NULL DEFAULT 0,
  is_conditional INTEGER NOT NULL DEFAULT 0,
  is_unordered INTEGER,
  is_base_subobject INTEGER NOT NULL DEFAULT 0,
  is_vla INTEGER NOT NULL DEFAULT 0,
  is_deallocation INTEGER NOT NULL DEFAULT 0,
  is_local_static_guard INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE cleanup_storage_location (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  base_entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  offset TEXT NOT NULL,
  is_indirect INTEGER NOT NULL DEFAULT 0,
  uses_this_parameter_offset INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE cleanup_array (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  function_eh_id INTEGER REFERENCES function_eh(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  handle_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  element_size TEXT NOT NULL,
  element_count TEXT NOT NULL,
  is_nonarray_size INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE cleanup_condition (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  storage_id INTEGER REFERENCES cleanup_storage_location(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  comparison_value TEXT
) STRICT;

CREATE TABLE cleanup_abi_detail (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  vtt_location_id INTEGER REFERENCES cleanup_storage_location(id) DEFERRABLE INITIALLY DEFERRED,
  is_delegating INTEGER,
  vla_count_location_id INTEGER REFERENCES cleanup_storage_location(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE lowered_eh_operation (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  function_eh_id INTEGER REFERENCES function_eh(id) DEFERRABLE INITIALLY DEFERRED,
  region_id INTEGER REFERENCES cleanup_region(id) DEFERRABLE INITIALLY DEFERRED,
  state_number TEXT,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  handler_id INTEGER REFERENCES handler(id) DEFERRABLE INITIALLY DEFERRED,
  try_block_id INTEGER REFERENCES try_descriptor(id) DEFERRABLE INITIALLY DEFERRED,
  try_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  catch_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE cleanup_state_operation (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  region_id INTEGER REFERENCES cleanup_region(id) DEFERRABLE INITIALLY DEFERRED,
  state_number TEXT,
  is_unreachable INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE template_definition (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  pattern_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  constraint_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  canonical_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  definition_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  prototype_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  is_exported INTEGER NOT NULL DEFAULT 0,
  ignore_export INTEGER NOT NULL DEFAULT 0,
  is_pack INTEGER NOT NULL DEFAULT 0,
  is_friend_template INTEGER NOT NULL DEFAULT 0,
  parameter_number INTEGER,
  parameter_depth INTEGER,
  definition_text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  export_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  definition_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  symbol_id INTEGER REFERENCES symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE template_header (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  parent_id INTEGER REFERENCES template_header(id) DEFERRABLE INITIALLY DEFERRED,
  scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  is_generic INTEGER NOT NULL DEFAULT 0,
  requires_clause_id INTEGER REFERENCES requires_clause(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE template_parameter (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  header_id INTEGER REFERENCES template_header(id) DEFERRABLE INITIALLY DEFERRED,
  ordinal INTEGER NOT NULL,
  depth INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_pack INTEGER NOT NULL DEFAULT 0,
  is_abbreviated INTEGER NOT NULL DEFAULT 0,
  placeholder_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  placeholder_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  placeholder_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  default_argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED,
  constraint_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE template_generic_constraint_clause (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  header_id INTEGER REFERENCES template_header(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  parameter_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  where_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  colon_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE template_generic_constraint (
  owner_id INTEGER REFERENCES template_generic_constraint_clause(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  is_implicit INTEGER NOT NULL DEFAULT 0,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE template_argument (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  is_pack INTEGER NOT NULL DEFAULT 0,
  is_explicit INTEGER NOT NULL DEFAULT 0,
  is_pack_element INTEGER NOT NULL DEFAULT 0,
  is_ellipsis INTEGER NOT NULL DEFAULT 0,
  is_integer_pack INTEGER NOT NULL DEFAULT 0,
  is_injected_name INTEGER NOT NULL DEFAULT 0,
  is_provisional INTEGER NOT NULL DEFAULT 0,
  is_error INTEGER NOT NULL DEFAULT 0,
  is_unknown_array_bound INTEGER NOT NULL DEFAULT 0,
  deduced_array_bound TEXT
) STRICT;

CREATE TABLE template_argument_pack_element (
  owner_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE template_instance (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  result_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED,
  selected_specialization_id INTEGER REFERENCES template_specialization(id) DEFERRABLE INITIALLY DEFERRED,
  state_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  is_dependent INTEGER NOT NULL DEFAULT 0,
  is_prototype INTEGER NOT NULL DEFAULT 0,
  is_suppressed INTEGER NOT NULL DEFAULT 0,
  is_explicit INTEGER NOT NULL DEFAULT 0,
  is_specialized INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE template_instance_argument (
  owner_id INTEGER REFERENCES template_instance(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  role_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, role_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE template_specialization (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  primary_template_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE source_file (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  path_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  origin_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE include_occurrence (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  file_id INTEGER REFERENCES source_file(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parent_id INTEGER REFERENCES include_occurrence(id) DEFERRABLE INITIALLY DEFERRED,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  ordinal INTEGER NOT NULL,
  display_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  name_as_written_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  first_sequence TEXT,
  last_sequence TEXT,
  first_line TEXT,
  module_id INTEGER REFERENCES module(id) DEFERRABLE INITIALLY DEFERRED,
  is_system INTEGER NOT NULL DEFAULT 0,
  uses_angle_brackets INTEGER NOT NULL DEFAULT 0,
  is_include_file INTEGER NOT NULL DEFAULT 0,
  is_preinclude INTEGER NOT NULL DEFAULT 0,
  is_macros_only INTEGER NOT NULL DEFAULT 0,
  is_top_level INTEGER NOT NULL DEFAULT 0,
  is_top_level_from_pch INTEGER NOT NULL DEFAULT 0,
  is_implicit_include INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE line_directive (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  presumed_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  first_sequence TEXT NOT NULL,
  last_sequence TEXT NOT NULL,
  first_line TEXT NOT NULL,
  include_id INTEGER REFERENCES include_occurrence(id) DEFERRABLE INITIALLY DEFERRED,
  parent_id INTEGER REFERENCES line_directive(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE source_descriptor_child (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  child_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE source_mapping_segment (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  first_sequence TEXT NOT NULL,
  last_sequence TEXT NOT NULL,
  file_id INTEGER REFERENCES source_file(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  include_id INTEGER REFERENCES include_occurrence(id) DEFERRABLE INITIALLY DEFERRED,
  line_directive_id INTEGER REFERENCES line_directive(id) DEFERRABLE INITIALLY DEFERRED,
  logical_line TEXT NOT NULL,
  physical_line TEXT,
  presumed_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  ends_at_end_of_source INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE source_position (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  sequence TEXT NOT NULL,
  column INTEGER NOT NULL,
  original_sequence TEXT,
  original_column INTEGER,
  macro_invocation_id INTEGER REFERENCES macro_invocation(id) DEFERRABLE INITIALLY DEFERRED,
  file_id INTEGER REFERENCES source_file(id) DEFERRABLE INITIALLY DEFERRED,
  presumed_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  logical_line TEXT,
  physical_line TEXT,
  include_id INTEGER REFERENCES include_occurrence(id) DEFERRABLE INITIALLY DEFERRED,
  mapping_segment_id INTEGER REFERENCES source_mapping_segment(id) DEFERRABLE INITIALLY DEFERRED,
  spelling_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  at_end_of_source INTEGER NOT NULL DEFAULT 0,
  origin_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE source_range (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  start_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  end_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  endpoint_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE source_origin (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE macro_definition (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  replacement_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_function_like INTEGER NOT NULL DEFAULT 0,
  is_predefined INTEGER NOT NULL DEFAULT 0,
  is_undef INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE macro_invocation (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  definition_id INTEGER REFERENCES macro_definition(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parent_id INTEGER REFERENCES macro_invocation(id) DEFERRABLE INITIALLY DEFERRED,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  end_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  argument_text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  ordinal INTEGER NOT NULL
) STRICT;

CREATE TABLE module (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  resolved_file_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  file_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  contains_unsupported_constructs INTEGER NOT NULL DEFAULT 0,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  resolved_header_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  unit_id INTEGER REFERENCES module(id) DEFERRABLE INITIALLY DEFERRED,
  uses_angle_brackets INTEGER NOT NULL DEFAULT 0,
  suppress_macro_export INTEGER NOT NULL DEFAULT 0,
  is_internal_partition INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE module_import (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  module_id INTEGER REFERENCES module(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  module_name_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  implementation_unit_importing_self INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE translation_unit_module_import (
  owner_id INTEGER REFERENCES translation_unit(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  import_id INTEGER REFERENCES module_import(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE source_entry (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  declaration_id INTEGER REFERENCES declaration(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE scope_source_entry (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entry_id INTEGER REFERENCES source_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE source_sublist (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE source_sublist_entry (
  owner_id INTEGER REFERENCES source_sublist(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entry_id INTEGER REFERENCES source_entry(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE scope_source_sublist (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  sublist_id INTEGER REFERENCES source_sublist(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE secondary_declaration (
  id INTEGER PRIMARY KEY REFERENCES declaration(id) DEFERRABLE INITIALLY DEFERRED,
  entity_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  declared_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  name_reference_id INTEGER REFERENCES name_reference(id) DEFERRABLE INITIALLY DEFERRED,
  declared_storage_class_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  autonomous_tag_declaration INTEGER NOT NULL DEFAULT 0,
  embeds_source_entries INTEGER NOT NULL DEFAULT 0,
  is_friend INTEGER NOT NULL DEFAULT 0,
  declared_in_function_prototype INTEGER NOT NULL DEFAULT 0,
  specialized_with_new_syntax INTEGER NOT NULL DEFAULT 0,
  is_first_declaration INTEGER,
  is_partial_instantiation INTEGER,
  is_compiler_generated_forward_declaration INTEGER,
  is_originally_nonautonomous_definition INTEGER,
  is_gnu_extension INTEGER NOT NULL DEFAULT 0,
  is_after_first_declarator INTEGER NOT NULL DEFAULT 0,
  has_explicit_storage_class INTEGER NOT NULL DEFAULT 0,
  is_alias INTEGER NOT NULL DEFAULT 0,
  is_event_interface INTEGER
) STRICT;

CREATE TABLE declaration_positions (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  identifier_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  specifiers_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  declarator_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  enum_value_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  namespace_definition_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id)
) STRICT;

CREATE TABLE declaration_element_position (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE source_end_construct (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  target_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE instantiation_directive (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  target_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  declared_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  do_not_instantiate INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE linkage_specification (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  name_string_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  name_linkage_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  end_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE microsoft_if_exists (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  target_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  name_reference_id INTEGER REFERENCES name_reference(id) DEFERRABLE INITIALLY DEFERRED,
  position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  is_if_exists INTEGER,
  is_this INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE record_declaration (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  record_type_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  declaration_id INTEGER REFERENCES declaration(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE pragma (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  source_entry_id INTEGER REFERENCES source_entry(id) DEFERRABLE INITIALLY DEFERRED,
  text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  ignore_in_back_end INTEGER NOT NULL DEFAULT 0,
  is_microsoft_operator INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE scope_pragma (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  pragma_id INTEGER REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE scope_if_exists (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  if_exists_id INTEGER REFERENCES microsoft_if_exists(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE pragma_stdc (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  value_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE pragma_upc (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  access_method_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED,
  coherence_operation_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE pragma_ident (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  string_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE pragma_comment (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  string_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE pragma_conform (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_on INTEGER NOT NULL DEFAULT 0,
  is_off INTEGER NOT NULL DEFAULT 0,
  is_show INTEGER NOT NULL DEFAULT 0,
  is_push INTEGER NOT NULL DEFAULT 0,
  is_pop INTEGER NOT NULL DEFAULT 0,
  identifier_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE pragma_include_alias (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  long_file_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  short_file_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE pragma_gcc (
  id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  visibility_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE typeref_original_template_argument (
  owner_id INTEGER REFERENCES type_reference(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES template_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE ms_attribute (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  target_tag_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  source_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED,
  source_entry_id INTEGER REFERENCES source_entry(id) DEFERRABLE INITIALLY DEFERRED,
  next_in_block_id INTEGER REFERENCES ms_attribute(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE ms_attribute_argument (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  parameter_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  integer_value TEXT,
  boolean_value INTEGER,
  enumeration_ordinal INTEGER,
  string_constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  other_text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  uuid_text_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE owner_ms_attribute (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  attribute_id INTEGER REFERENCES ms_attribute(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE ms_attribute_argument_link (
  owner_id INTEGER REFERENCES ms_attribute(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  argument_id INTEGER REFERENCES ms_attribute_argument(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE ms_attribute_target (
  owner_id INTEGER REFERENCES ms_attribute(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE scoped_expression (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE local_expression_reference (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  referrer_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE template_symbol (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  descriptor_id INTEGER REFERENCES template_definition(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE local_static_initialization (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  initialization_kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  constant_id INTEGER REFERENCES constant(id) DEFERRABLE INITIALLY DEFERRED,
  lifetime_id INTEGER REFERENCES lifetime(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE scope_local_static_initialization (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  initialization_id INTEGER REFERENCES local_static_initialization(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE constructor_initializer (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  field_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  initialization_id INTEGER REFERENCES initialization(id) DEFERRABLE INITIALLY DEFERRED,
  source_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED,
  compiler_generated INTEGER NOT NULL,
  is_pack_expansion INTEGER NOT NULL DEFAULT 0,
  is_braced INTEGER NOT NULL DEFAULT 0,
  uses_field_initializer INTEGER NOT NULL DEFAULT 0,
  original_type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED,
  source_range_id INTEGER REFERENCES source_range(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE scope_constructor_initializer (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  initializer_id INTEGER REFERENCES constructor_initializer(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE data_member_spec (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  requested_alignment TEXT NOT NULL,
  bit_width TEXT NOT NULL,
  no_unique_address INTEGER NOT NULL DEFAULT 0
) STRICT;

CREATE TABLE requires_clause (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  constraint_expression_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  requires_position_id INTEGER REFERENCES source_position(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE gnu_routine_supplement (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  section_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  assembly_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  aliased_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  inline_partner_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  resolver_variable_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  constructor_priority TEXT,
  destructor_priority TEXT,
  is_representative INTEGER,
  is_target_specific_version INTEGER,
  resolver_required INTEGER,
  representative_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE routine_selected_override (
  owner_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE routine_befriending_class (
  owner_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  type_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE routine_multiversion (
  owner_id INTEGER REFERENCES gnu_routine_supplement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_base_postorder (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_base_preorder (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_befriending_class (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_friend (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_promoted_type (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE field_initializer_entity (
  owner_id INTEGER REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE owner_decl_modifier (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, kind_id)
) WITHOUT ROWID, STRICT;

CREATE TABLE routine_target_architecture (
  owner_id INTEGER REFERENCES gnu_routine_supplement(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  kind_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, kind_id)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_event_interface (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  target_id INTEGER REFERENCES type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE record_vcall_offset (
  owner_id INTEGER REFERENCES record_type(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  base_subobject_id INTEGER REFERENCES base_subobject(id) DEFERRABLE INITIALLY DEFERRED,
  vtable_index TEXT NOT NULL,
  is_primary INTEGER NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE source_correspondence (
  id INTEGER PRIMARY KEY REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED,
  current_name_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  unmangled_name_or_mangled_encoding_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED,
  parent_scope_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED,
  enclosing_routine_id INTEGER REFERENCES routine_symbol(id) DEFERRABLE INITIALLY DEFERRED,
  is_imported_from_module INTEGER NOT NULL,
  source_origin_id INTEGER REFERENCES source_origin(id) DEFERRABLE INITIALLY DEFERRED,
  access_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  referenced INTEGER NOT NULL,
  needed INTEGER,
  linkage_id INTEGER REFERENCES kind(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  is_local_to_function INTEGER NOT NULL,
  is_class_member INTEGER NOT NULL,
  name_has_been_mangled INTEGER,
  unnamed_entity_given_fabricated_name INTEGER,
  is_decl_after_first_in_comma_list INTEGER,
  copied_from_secondary_trans_unit INTEGER NOT NULL,
  same_name_as_external_entity_in_secondary_trans_unit INTEGER NOT NULL,
  member_of_unknown_base INTEGER NOT NULL,
  qualified_unknown_base_member INTEGER NOT NULL,
  member_of_unknown_super INTEGER,
  microsoft_identifier_used INTEGER,
  marked_as_gnu_extension INTEGER,
  is_deprecated_or_unavailable INTEGER NOT NULL,
  externalized INTEGER NOT NULL,
  maybe_unused INTEGER NOT NULL,
  source_sequence_entry_id INTEGER REFERENCES source_entry(id) DEFERRABLE INITIALLY DEFERRED
) STRICT;

CREATE TABLE entity_source_correspondence (
  owner_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  source_correspondence_id INTEGER REFERENCES source_correspondence(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id)
) STRICT;

CREATE TABLE scope_child (
  owner_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  child_id INTEGER REFERENCES scope(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE upc_threads_value (
  id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED,
  multiplier_id INTEGER REFERENCES text_atom(id) DEFERRABLE INITIALLY DEFERRED NOT NULL
) STRICT;

CREATE TABLE expression_vla_dimension (
  owner_id INTEGER REFERENCES expression(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  dimension_id INTEGER REFERENCES vla_dimension(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;

CREATE TABLE variable_initializer_entity (
  owner_id INTEGER REFERENCES variable_symbol(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  ordinal INTEGER NOT NULL,
  entity_id INTEGER REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED NOT NULL,
  PRIMARY KEY (owner_id, ordinal)
) WITHOUT ROWID, STRICT;
INSERT INTO kind VALUES(1001,'entity','translation_unit');
INSERT INTO kind VALUES(1002,'entity','name');
INSERT INTO kind VALUES(1003,'entity','scope');
INSERT INTO kind VALUES(1004,'entity','symbol');
INSERT INTO kind VALUES(1005,'entity','namespace');
INSERT INTO kind VALUES(1006,'entity','routine');
INSERT INTO kind VALUES(1007,'entity','variable');
INSERT INTO kind VALUES(1008,'entity','field');
INSERT INTO kind VALUES(1009,'entity','label');
INSERT INTO kind VALUES(1010,'entity','enum');
INSERT INTO kind VALUES(1011,'entity','alias');
INSERT INTO kind VALUES(1012,'entity','using');
INSERT INTO kind VALUES(1013,'entity','type');
INSERT INTO kind VALUES(1014,'entity','declaration');
INSERT INTO kind VALUES(1015,'entity','expression');
INSERT INTO kind VALUES(1016,'entity','statement');
INSERT INTO kind VALUES(1017,'entity','initialization');
INSERT INTO kind VALUES(1018,'entity','lifetime');
INSERT INTO kind VALUES(1019,'entity','temporary');
INSERT INTO kind VALUES(1020,'entity','base_subobject');
INSERT INTO kind VALUES(1021,'entity','template');
INSERT INTO kind VALUES(1022,'entity','template_parameter');
INSERT INTO kind VALUES(1023,'entity','template_argument');
INSERT INTO kind VALUES(1024,'entity','template_instance');
INSERT INTO kind VALUES(1025,'entity','template_specialization');
INSERT INTO kind VALUES(1026,'entity','source_file');
INSERT INTO kind VALUES(1027,'entity','include_occurrence');
INSERT INTO kind VALUES(1028,'entity','source_mapping');
INSERT INTO kind VALUES(1029,'entity','source_position');
INSERT INTO kind VALUES(1030,'entity','source_range');
INSERT INTO kind VALUES(1031,'entity','source_origin');
INSERT INTO kind VALUES(1032,'entity','macro_definition');
INSERT INTO kind VALUES(1033,'entity','macro_invocation');
INSERT INTO kind VALUES(1034,'entity','source_attachment');
INSERT INTO kind VALUES(1035,'entity','attribute');
INSERT INTO kind VALUES(1036,'entity','subobject_path');
INSERT INTO kind VALUES(1037,'entity','name_reference');
INSERT INTO kind VALUES(1038,'entity','exception_specification');
INSERT INTO kind VALUES(1039,'entity','virtual_table');
INSERT INTO kind VALUES(1040,'entity','record_layout');
INSERT INTO kind VALUES(1041,'entity','vla_dimension');
INSERT INTO kind VALUES(1042,'entity','coroutine');
INSERT INTO kind VALUES(1043,'entity','function_eh');
INSERT INTO kind VALUES(1044,'entity','cleanup_region');
INSERT INTO kind VALUES(1045,'entity','cleanup_action');
INSERT INTO kind VALUES(1046,'entity','cleanup_storage');
INSERT INTO kind VALUES(1047,'entity','cleanup_array');
INSERT INTO kind VALUES(1048,'entity','cleanup_condition');
INSERT INTO kind VALUES(1049,'entity','cleanup_abi');
INSERT INTO kind VALUES(2001,'capture_profile','native_lowered');
INSERT INTO kind VALUES(2002,'capture_profile','source_semantic');
INSERT INTO kind VALUES(2003,'capture_profile','cfront_lowered');
INSERT INTO kind VALUES(2004,'capture_profile','exported_templates_lowered');
INSERT INTO kind VALUES(3001,'abi','itanium');
INSERT INTO kind VALUES(3002,'abi','cfront');
INSERT INTO kind VALUES(3003,'abi','microsoft');
INSERT INTO kind VALUES(4001,'availability','available');
INSERT INTO kind VALUES(4002,'availability','absent');
INSERT INTO kind VALUES(4003,'availability','not_applicable');
INSERT INTO kind VALUES(4004,'availability','dependent');
INSERT INTO kind VALUES(4005,'availability','disabled');
INSERT INTO kind VALUES(4006,'availability','lost');
INSERT INTO kind VALUES(4007,'availability','not_retained');
INSERT INTO kind VALUES(5001,'fact','source');
INSERT INTO kind VALUES(5002,'fact','source_target');
INSERT INTO kind VALUES(5003,'fact','source_dispatch');
INSERT INTO kind VALUES(5004,'fact','source_parameters');
INSERT INTO kind VALUES(5005,'fact','original_type');
INSERT INTO kind VALUES(5006,'fact','lowering_origin');
INSERT INTO kind VALUES(5007,'fact','template_origin');
INSERT INTO kind VALUES(5008,'fact','declaration_sequence');
INSERT INTO kind VALUES(5009,'fact','macro_tree');
INSERT INTO kind VALUES(5010,'fact','macro_arguments');
INSERT INTO kind VALUES(5011,'fact','tokens');
INSERT INTO kind VALUES(5012,'fact','trivia');
INSERT INTO kind VALUES(5013,'fact','lifetime_details');
INSERT INTO kind VALUES(6001,'role','operand');
INSERT INTO kind VALUES(6002,'role','callee');
INSERT INTO kind VALUES(6003,'role','argument');
INSERT INTO kind VALUES(6004,'role','receiver');
INSERT INTO kind VALUES(6005,'role','return_slot');
INSERT INTO kind VALUES(6006,'role','condition');
INSERT INTO kind VALUES(6007,'role','then');
INSERT INTO kind VALUES(6008,'role','else');
INSERT INTO kind VALUES(6009,'role','body');
INSERT INTO kind VALUES(6010,'role','initialization');
INSERT INTO kind VALUES(6011,'role','increment');
INSERT INTO kind VALUES(6012,'role','statement');
INSERT INTO kind VALUES(6013,'role','result');
INSERT INTO kind VALUES(6014,'role','metadata');
INSERT INTO kind VALUES(6015,'role','backing_expression');
INSERT INTO kind VALUES(6016,'role','primary_template');
INSERT INTO kind VALUES(6017,'role','selected_partial_specialization');
INSERT INTO kind VALUES(6018,'role','type_operand');
INSERT INTO kind VALUES(6019,'role','ready');
INSERT INTO kind VALUES(6020,'role','suspend');
INSERT INTO kind VALUES(6021,'role','resume');
INSERT INTO kind VALUES(6022,'role','yield');
INSERT INTO kind VALUES(6023,'role','return_value');
INSERT INTO kind VALUES(6024,'role','affinity');
INSERT INTO kind VALUES(6025,'role','cleanup');
INSERT INTO kind VALUES(6026,'role','normal');
INSERT INTO kind VALUES(6027,'role','execution');
INSERT INTO kind VALUES(6028,'role','source_parameter');
INSERT INTO kind VALUES(6029,'role','this_parameter');
INSERT INTO kind VALUES(6030,'role','return_parameter');
INSERT INTO kind VALUES(6031,'role','vtt_parameter');
INSERT INTO kind VALUES(6032,'role','abi_parameter');
INSERT INTO kind VALUES(6033,'role','ordinary_parameter');
INSERT INTO kind VALUES(6034,'role','member');
INSERT INTO kind VALUES(6035,'role','base');
INSERT INTO kind VALUES(6036,'role','element');
INSERT INTO kind VALUES(6037,'role','constructor');
INSERT INTO kind VALUES(6038,'role','destructor');
INSERT INTO kind VALUES(6039,'role','allocator');
INSERT INTO kind VALUES(6040,'role','deallocator');
INSERT INTO kind VALUES(6041,'role','constraint');
INSERT INTO kind VALUES(7001,'representation_role','execution');
INSERT INTO kind VALUES(7002,'representation_role','metadata');
INSERT INTO kind VALUES(7003,'representation_role','source_semantic');
INSERT INTO kind VALUES(8001,'value_category','prvalue');
INSERT INTO kind VALUES(8002,'value_category','lvalue');
INSERT INTO kind VALUES(8003,'value_category','xvalue');
INSERT INTO kind VALUES(9001,'call_kind','direct');
INSERT INTO kind VALUES(9002,'call_kind','indirect');
INSERT INTO kind VALUES(9003,'call_kind','member');
INSERT INTO kind VALUES(9004,'call_kind','member_pointer');
INSERT INTO kind VALUES(10001,'float_class','finite');
INSERT INTO kind VALUES(10002,'float_class','zero');
INSERT INTO kind VALUES(10003,'float_class','infinity');
INSERT INTO kind VALUES(10004,'float_class','nan');
INSERT INTO kind VALUES(11001,'string_encoding','target_code_units');
INSERT INTO kind VALUES(11002,'string_encoding','utf8');
INSERT INTO kind VALUES(11003,'string_encoding','ascii');
INSERT INTO kind VALUES(11004,'string_encoding','utf16');
INSERT INTO kind VALUES(11005,'string_encoding','utf32');
INSERT INTO kind VALUES(11006,'string_encoding','unknown');
INSERT INTO kind VALUES(12001,'name_category','named');
INSERT INTO kind VALUES(12002,'name_category','anonymous');
INSERT INTO kind VALUES(12003,'name_category','unnamed');
INSERT INTO kind VALUES(12004,'name_category','generated');
INSERT INTO kind VALUES(13001,'body_state','definition');
INSERT INTO kind VALUES(13002,'body_state','declaration_only');
INSERT INTO kind VALUES(13003,'body_state','dependent');
INSERT INTO kind VALUES(13004,'body_state','discarded');
INSERT INTO kind VALUES(13005,'body_state','unavailable');
INSERT INTO kind VALUES(14001,'definition_state','complete');
INSERT INTO kind VALUES(14002,'definition_state','incomplete');
INSERT INTO kind VALUES(14003,'definition_state','dependent');
INSERT INTO kind VALUES(15001,'bound_kind','constant');
INSERT INTO kind VALUES(15002,'bound_kind','incomplete');
INSERT INTO kind VALUES(15003,'bound_kind','zero');
INSERT INTO kind VALUES(15004,'bound_kind','variable');
INSERT INTO kind VALUES(15005,'bound_kind','dependent');
INSERT INTO kind VALUES(16001,'reference_kind','variable');
INSERT INTO kind VALUES(16002,'reference_kind','routine');
INSERT INTO kind VALUES(16003,'reference_kind','field');
INSERT INTO kind VALUES(16004,'reference_kind','label');
INSERT INTO kind VALUES(16005,'reference_kind','constant');
INSERT INTO kind VALUES(16006,'reference_kind','type');
INSERT INTO kind VALUES(16007,'reference_kind','builtin');
INSERT INTO kind VALUES(16008,'reference_kind','property');
INSERT INTO kind VALUES(16009,'reference_kind','destructor');
INSERT INTO kind VALUES(17001,'jump_kind','return');
INSERT INTO kind VALUES(17002,'jump_kind','goto');
INSERT INTO kind VALUES(17003,'jump_kind','break');
INSERT INTO kind VALUES(17004,'jump_kind','continue');
INSERT INTO kind VALUES(17005,'jump_kind','leave');
INSERT INTO kind VALUES(17006,'jump_kind','assigned_goto');
INSERT INTO kind VALUES(18001,'cleanup_kind','destructor');
INSERT INTO kind VALUES(18002,'cleanup_kind','deallocation');
INSERT INTO kind VALUES(18003,'cleanup_kind','guard');
INSERT INTO kind VALUES(18004,'cleanup_kind','exception_object');
INSERT INTO kind VALUES(18005,'cleanup_kind','special');
INSERT INTO kind VALUES(19001,'storage_location_kind','direct_variable');
INSERT INTO kind VALUES(19002,'storage_location_kind','indirect_variable');
INSERT INTO kind VALUES(19003,'storage_location_kind','receiver_relative');
INSERT INTO kind VALUES(19004,'storage_location_kind','static_address');
INSERT INTO kind VALUES(20001,'condition_kind','constructed');
INSERT INTO kind VALUES(20002,'condition_kind','nonzero');
INSERT INTO kind VALUES(20003,'condition_kind','state');
INSERT INTO kind VALUES(21001,'source_origin_kind','ordinary');
INSERT INTO kind VALUES(21002,'source_origin_kind','predefined');
INSERT INTO kind VALUES(21003,'source_origin_kind','command_line');
INSERT INTO kind VALUES(21004,'source_origin_kind','unknown');
INSERT INTO kind VALUES(21005,'source_origin_kind','synthesized');
INSERT INTO kind VALUES(21006,'source_origin_kind','macro');
INSERT INTO kind VALUES(22001,'range_endpoint','inclusive');
INSERT INTO kind VALUES(22002,'range_endpoint','last_character');
INSERT INTO kind VALUES(22003,'range_endpoint','hypothetical');
INSERT INTO kind VALUES(23001,'specialization_kind','explicit');
INSERT INTO kind VALUES(23002,'specialization_kind','partial');
INSERT INTO kind VALUES(23003,'specialization_kind','implicit');
INSERT INTO kind VALUES(24001,'type_relationship','declared');
INSERT INTO kind VALUES(24002,'type_relationship','storage');
INSERT INTO kind VALUES(24003,'type_relationship','subobject_partner');
INSERT INTO kind VALUES(24004,'type_relationship','retained_original');
INSERT INTO kind VALUES(25001,'abi_routine_kind','primary');
INSERT INTO kind VALUES(25002,'abi_routine_kind','alternate_complete');
INSERT INTO kind VALUES(25003,'abi_routine_kind','alternate_subobject');
INSERT INTO kind VALUES(25004,'abi_routine_kind','deleting_destructor');
INSERT INTO kind VALUES(25005,'abi_routine_kind','thunk');
INSERT INTO kind VALUES(25006,'abi_routine_kind','wrapper');
INSERT INTO kind VALUES(25007,'abi_routine_kind','target');
INSERT INTO kind VALUES(26001,'template_argument_role','primary');
INSERT INTO kind VALUES(26002,'template_argument_role','selected_partial');
INSERT INTO kind VALUES(27001,'instantiation_state','concrete');
INSERT INTO kind VALUES(27002,'instantiation_state','dependent');
INSERT INTO kind VALUES(27003,'instantiation_state','external_definition');
INSERT INTO kind VALUES(27004,'instantiation_state','suppressed');
INSERT INTO kind VALUES(28001,'asm_operand_kind','input');
INSERT INTO kind VALUES(28002,'asm_operand_kind','output');
INSERT INTO kind VALUES(28003,'asm_operand_kind','inout');
INSERT INTO kind VALUES(29001,'scope_kind','file');
INSERT INTO kind VALUES(29002,'scope_kind','namespace');
INSERT INTO kind VALUES(29003,'scope_kind','class');
INSERT INTO kind VALUES(29004,'scope_kind','function');
INSERT INTO kind VALUES(29005,'scope_kind','block');
INSERT INTO kind VALUES(29006,'scope_kind','prototype');
INSERT INTO kind VALUES(29007,'scope_kind','template');
INSERT INTO kind VALUES(30001,'linkage','none');
INSERT INTO kind VALUES(30002,'linkage','internal');
INSERT INTO kind VALUES(30003,'linkage','external');
INSERT INTO kind VALUES(30004,'linkage','c');
INSERT INTO kind VALUES(30005,'linkage','cpp');
INSERT INTO kind VALUES(31001,'storage_duration','automatic');
INSERT INTO kind VALUES(31002,'storage_duration','static');
INSERT INTO kind VALUES(31003,'storage_duration','thread');
INSERT INTO kind VALUES(31004,'storage_duration','allocated');
INSERT INTO kind VALUES(31005,'storage_duration','parameter');
INSERT INTO kind VALUES(32001,'access','public');
INSERT INTO kind VALUES(32002,'access','protected');
INSERT INTO kind VALUES(32003,'access','private');
INSERT INTO kind VALUES(32004,'access','none');
INSERT INTO kind VALUES(33001,'ref_qualifier','none');
INSERT INTO kind VALUES(33002,'ref_qualifier','lvalue');
INSERT INTO kind VALUES(33003,'ref_qualifier','rvalue');
INSERT INTO kind VALUES(34001,'calling_convention','default');
INSERT INTO kind VALUES(34002,'calling_convention','cdecl');
INSERT INTO kind VALUES(34003,'calling_convention','stdcall');
INSERT INTO kind VALUES(34004,'calling_convention','fastcall');
INSERT INTO kind VALUES(34005,'calling_convention','thiscall');
INSERT INTO kind VALUES(34006,'calling_convention','vectorcall');
INSERT INTO kind VALUES(35001,'type_reference_kind','typedef');
INSERT INTO kind VALUES(35002,'type_reference_kind','qualified');
INSERT INTO kind VALUES(35003,'type_reference_kind','decltype');
INSERT INTO kind VALUES(35004,'type_reference_kind','template');
INSERT INTO kind VALUES(35005,'type_reference_kind','alias');
INSERT INTO kind VALUES(36001,'exception_specification_kind','none');
INSERT INTO kind VALUES(36002,'exception_specification_kind','dynamic');
INSERT INTO kind VALUES(36003,'exception_specification_kind','noexcept');
INSERT INTO kind VALUES(36004,'exception_specification_kind','dependent');
INSERT INTO kind VALUES(37001,'vector_kind','fixed');
INSERT INTO kind VALUES(37002,'vector_kind','scalable');
INSERT INTO kind VALUES(37003,'vector_kind','riscv');
INSERT INTO kind VALUES(38001,'virtual_table_kind','vtable');
INSERT INTO kind VALUES(38002,'virtual_table_kind','vtt');
INSERT INTO kind VALUES(38003,'virtual_table_kind','rtti');
INSERT INTO kind VALUES(39001,'attribute_kind','gnu');
INSERT INTO kind VALUES(39002,'attribute_kind','microsoft');
INSERT INTO kind VALUES(39003,'attribute_kind','standard');
INSERT INTO kind VALUES(40001,'capture_kind','copy');
INSERT INTO kind VALUES(40002,'capture_kind','reference');
INSERT INTO kind VALUES(40003,'capture_kind','this');
INSERT INTO kind VALUES(40004,'capture_kind','star_this');
INSERT INTO kind VALUES(40005,'capture_kind','initialization');
INSERT INTO kind VALUES(41001,'managed_kind','cli');
INSERT INTO kind VALUES(41002,'managed_kind','cx');
INSERT INTO kind VALUES(41003,'managed_kind','handle');
INSERT INTO kind VALUES(41004,'managed_kind','tracking_reference');
INSERT INTO kind VALUES(41005,'managed_kind','property');
INSERT INTO kind VALUES(41006,'managed_kind','event');
INSERT INTO kind VALUES(41007,'managed_kind','delegate');
INSERT INTO kind VALUES(41008,'managed_kind','array');
INSERT INTO kind VALUES(42001,'source_attachment_kind','frozen_compilation_input');
INSERT INTO kind VALUES(42002,'source_attachment_kind','post_compilation_reread');
INSERT INTO kind VALUES(42003,'source_attachment_kind','external');
INSERT INTO kind VALUES(43001,'lifetime_transition_kind','enter');
INSERT INTO kind VALUES(43002,'lifetime_transition_kind','exit');
INSERT INTO kind VALUES(43003,'lifetime_transition_kind','jump');
INSERT INTO kind VALUES(43004,'lifetime_transition_kind','return');
INSERT INTO kind VALUES(43005,'lifetime_transition_kind','throw');
INSERT INTO kind VALUES(44001,'special_function_kind','ordinary');
INSERT INTO kind VALUES(44002,'special_function_kind','constructor');
INSERT INTO kind VALUES(44003,'special_function_kind','destructor');
INSERT INTO kind VALUES(44004,'special_function_kind','conversion');
INSERT INTO kind VALUES(44005,'special_function_kind','operator');
INSERT INTO kind VALUES(44006,'special_function_kind','thunk');
INSERT INTO kind VALUES(44007,'special_function_kind','wrapper');
INSERT INTO kind VALUES(45001,'float_representation','binary16');
INSERT INTO kind VALUES(45002,'float_representation','bfloat16');
INSERT INTO kind VALUES(45003,'float_representation','binary32');
INSERT INTO kind VALUES(45004,'float_representation','binary64');
INSERT INTO kind VALUES(45005,'float_representation','binary80');
INSERT INTO kind VALUES(45006,'float_representation','binary128');
INSERT INTO kind VALUES(45007,'float_representation','ibm_double_double');
INSERT INTO kind VALUES(46001,'arithmetic_kind','signed_integer');
INSERT INTO kind VALUES(46002,'arithmetic_kind','unsigned_integer');
INSERT INTO kind VALUES(46003,'arithmetic_kind','boolean');
INSERT INTO kind VALUES(46004,'arithmetic_kind','character');
INSERT INTO kind VALUES(46005,'arithmetic_kind','floating');
INSERT INTO kind VALUES(46006,'arithmetic_kind','fixed_point');
INSERT INTO kind VALUES(46007,'arithmetic_kind','complex');
INSERT INTO kind VALUES(46008,'arithmetic_kind','imaginary');
INSERT INTO kind VALUES(100001,'statement','stmk_expr');
INSERT INTO kind VALUES(100002,'statement','stmk_if');
INSERT INTO kind VALUES(100003,'statement','stmk_constexpr_if');
INSERT INTO kind VALUES(100004,'statement','stmk_if_consteval');
INSERT INTO kind VALUES(100005,'statement','stmk_if_not_consteval');
INSERT INTO kind VALUES(100006,'statement','stmk_while');
INSERT INTO kind VALUES(100007,'statement','stmk_goto');
INSERT INTO kind VALUES(100008,'statement','stmk_label');
INSERT INTO kind VALUES(100009,'statement','stmk_return');
INSERT INTO kind VALUES(100010,'statement','stmk_coroutine');
INSERT INTO kind VALUES(100011,'statement','stmk_coroutine_return');
INSERT INTO kind VALUES(100012,'statement','stmk_block');
INSERT INTO kind VALUES(100013,'statement','stmk_end_test_while');
INSERT INTO kind VALUES(100014,'statement','stmk_for');
INSERT INTO kind VALUES(100015,'statement','stmk_range_based_for');
INSERT INTO kind VALUES(100016,'statement','stmk_for_each');
INSERT INTO kind VALUES(100017,'statement','stmk_switch_case');
INSERT INTO kind VALUES(100018,'statement','stmk_switch');
INSERT INTO kind VALUES(100019,'statement','stmk_init');
INSERT INTO kind VALUES(100020,'statement','stmk_asm');
INSERT INTO kind VALUES(100021,'statement','stmk_asm_func_body');
INSERT INTO kind VALUES(100022,'statement','stmk_try_block');
INSERT INTO kind VALUES(100023,'statement','stmk_microsoft_try');
INSERT INTO kind VALUES(100024,'statement','stmk_decl');
INSERT INTO kind VALUES(100025,'statement','stmk_set_vla_size');
INSERT INTO kind VALUES(100026,'statement','stmk_vla_decl');
INSERT INTO kind VALUES(100027,'statement','stmk_upc_notify');
INSERT INTO kind VALUES(100028,'statement','stmk_upc_wait');
INSERT INTO kind VALUES(100029,'statement','stmk_upc_barrier');
INSERT INTO kind VALUES(100030,'statement','stmk_upc_fence');
INSERT INTO kind VALUES(100031,'statement','stmk_upc_forall');
INSERT INTO kind VALUES(100032,'statement','stmk_assigned_goto');
INSERT INTO kind VALUES(100033,'statement','stmk_empty');
INSERT INTO kind VALUES(100034,'statement','stmk_stmt_expr_result');
INSERT INTO kind VALUES(100035,'statement','stmk_last');
INSERT INTO kind VALUES(101001,'expression','enk_routine');
INSERT INTO kind VALUES(101002,'expression','enk_error');
INSERT INTO kind VALUES(101003,'expression','enk_reclaimed');
INSERT INTO kind VALUES(101004,'expression','enk_last');
INSERT INTO kind VALUES(101005,'expression','enk_operation');
INSERT INTO kind VALUES(101006,'expression','enk_constant');
INSERT INTO kind VALUES(101007,'expression','enk_variable');
INSERT INTO kind VALUES(101008,'expression','enk_field');
INSERT INTO kind VALUES(101009,'expression','enk_temp_init');
INSERT INTO kind VALUES(101010,'expression','enk_lambda');
INSERT INTO kind VALUES(101011,'expression','enk_new_delete');
INSERT INTO kind VALUES(101012,'expression','enk_gcnew');
INSERT INTO kind VALUES(101013,'expression','enk_throw');
INSERT INTO kind VALUES(101014,'expression','enk_condition');
INSERT INTO kind VALUES(101015,'expression','enk_object_lifetime');
INSERT INTO kind VALUES(101016,'expression','enk_typeid');
INSERT INTO kind VALUES(101017,'expression','enk_sizeof');
INSERT INTO kind VALUES(101018,'expression','enk_alignof');
INSERT INTO kind VALUES(101019,'expression','enk_datasizeof');
INSERT INTO kind VALUES(101020,'expression','enk_address_of_ellipsis');
INSERT INTO kind VALUES(101021,'expression','enk_statement');
INSERT INTO kind VALUES(101022,'expression','enk_reuse_value');
INSERT INTO kind VALUES(101023,'expression','enk_vla_dealloc');
INSERT INTO kind VALUES(101024,'expression','enk_type_operand');
INSERT INTO kind VALUES(101025,'expression','enk_builtin_operation');
INSERT INTO kind VALUES(101026,'expression','enk_param_ref');
INSERT INTO kind VALUES(101027,'expression','enk_c11_generic');
INSERT INTO kind VALUES(101028,'expression','enk_builtin_choose_expr');
INSERT INTO kind VALUES(101029,'expression','enk_yield');
INSERT INTO kind VALUES(101030,'expression','enk_await');
INSERT INTO kind VALUES(101031,'expression','enk_runtime_sizeof');
INSERT INTO kind VALUES(101032,'expression','enk_sizeof_pack');
INSERT INTO kind VALUES(101033,'expression','enk_braced_init_list');
INSERT INTO kind VALUES(101034,'expression','enk_fold');
INSERT INTO kind VALUES(101035,'expression','enk_initializer');
INSERT INTO kind VALUES(101036,'expression','enk_concept_id');
INSERT INTO kind VALUES(101037,'expression','enk_requires');
INSERT INTO kind VALUES(101038,'expression','enk_compound_req');
INSERT INTO kind VALUES(101039,'expression','enk_nested_req');
INSERT INTO kind VALUES(101040,'expression','enk_const_eval_deferred');
INSERT INTO kind VALUES(101041,'expression','enk_template_name');
INSERT INTO kind VALUES(101042,'expression','enk_token_sequence');
INSERT INTO kind VALUES(101043,'expression','enk_pack_index');
INSERT INTO kind VALUES(101044,'expression','enk_lowered_eh_construct');
INSERT INTO kind VALUES(101045,'expression','enk_result_of_overriding_function');
INSERT INTO kind VALUES(102001,'operator','eok_assign');
INSERT INTO kind VALUES(102002,'operator','eok_bassign');
INSERT INTO kind VALUES(102003,'operator','eok_call');
INSERT INTO kind VALUES(102005,'operator','eok_address_of');
INSERT INTO kind VALUES(102006,'operator','eok_reference_to');
INSERT INTO kind VALUES(102007,'operator','eok_handle_to');
INSERT INTO kind VALUES(102008,'operator','eok_indirect');
INSERT INTO kind VALUES(102009,'operator','eok_ref_indirect');
INSERT INTO kind VALUES(102010,'operator','eok_cast');
INSERT INTO kind VALUES(102011,'operator','eok_lvalue_cast');
INSERT INTO kind VALUES(102012,'operator','eok_ref_cast');
INSERT INTO kind VALUES(102013,'operator','eok_lvalue_adjust');
INSERT INTO kind VALUES(102014,'operator','eok_class_rvalue_adjust');
INSERT INTO kind VALUES(102015,'operator','eok_box');
INSERT INTO kind VALUES(102016,'operator','eok_handle_to_box');
INSERT INTO kind VALUES(102017,'operator','eok_unbox');
INSERT INTO kind VALUES(102018,'operator','eok_unbox_lvalue');
INSERT INTO kind VALUES(102019,'operator','eok_base_class_cast');
INSERT INTO kind VALUES(102020,'operator','eok_derived_class_cast');
INSERT INTO kind VALUES(102021,'operator','eok_pm_base_class_cast');
INSERT INTO kind VALUES(102022,'operator','eok_pm_derived_class_cast');
INSERT INTO kind VALUES(102023,'operator','eok_dynamic_cast');
INSERT INTO kind VALUES(102024,'operator','eok_ref_dynamic_cast');
INSERT INTO kind VALUES(102025,'operator','eok_bool_cast');
INSERT INTO kind VALUES(102026,'operator','eok_array_to_pointer');
INSERT INTO kind VALUES(102027,'operator','eok_dot_vacuous_destructor_call');
INSERT INTO kind VALUES(102028,'operator','eok_points_to_vacuous_destructor_call');
INSERT INTO kind VALUES(102029,'operator','eok_assume');
INSERT INTO kind VALUES(102030,'operator','eok_noexcept');
INSERT INTO kind VALUES(102031,'operator','eok_parens');
INSERT INTO kind VALUES(102032,'operator','eok_negate');
INSERT INTO kind VALUES(102033,'operator','eok_unary_plus');
INSERT INTO kind VALUES(102034,'operator','eok_complement');
INSERT INTO kind VALUES(102035,'operator','eok_not');
INSERT INTO kind VALUES(102036,'operator','eok_vector_not');
INSERT INTO kind VALUES(102037,'operator','eok_vector_fill');
INSERT INTO kind VALUES(102038,'operator','eok_xconj');
INSERT INTO kind VALUES(102039,'operator','eok_real_part');
INSERT INTO kind VALUES(102040,'operator','eok_imag_part');
INSERT INTO kind VALUES(102041,'operator','eok_post_incr');
INSERT INTO kind VALUES(102042,'operator','eok_post_decr');
INSERT INTO kind VALUES(102043,'operator','eok_pre_incr');
INSERT INTO kind VALUES(102044,'operator','eok_pre_decr');
INSERT INTO kind VALUES(102045,'operator','eok_add');
INSERT INTO kind VALUES(102046,'operator','eok_subtract');
INSERT INTO kind VALUES(102047,'operator','eok_multiply');
INSERT INTO kind VALUES(102048,'operator','eok_divide');
INSERT INTO kind VALUES(102049,'operator','eok_remainder');
INSERT INTO kind VALUES(102050,'operator','eok_jmultiply');
INSERT INTO kind VALUES(102051,'operator','eok_jdivide');
INSERT INTO kind VALUES(102052,'operator','eok_fjadd');
INSERT INTO kind VALUES(102053,'operator','eok_jfadd');
INSERT INTO kind VALUES(102054,'operator','eok_fjsubtract');
INSERT INTO kind VALUES(102055,'operator','eok_jfsubtract');
INSERT INTO kind VALUES(102056,'operator','eok_padd');
INSERT INTO kind VALUES(102057,'operator','eok_psubtract');
INSERT INTO kind VALUES(102058,'operator','eok_pdiff');
INSERT INTO kind VALUES(102059,'operator','eok_shiftl');
INSERT INTO kind VALUES(102060,'operator','eok_shiftr');
INSERT INTO kind VALUES(102061,'operator','eok_and');
INSERT INTO kind VALUES(102062,'operator','eok_or');
INSERT INTO kind VALUES(102063,'operator','eok_xor');
INSERT INTO kind VALUES(102064,'operator','eok_eq');
INSERT INTO kind VALUES(102065,'operator','eok_ne');
INSERT INTO kind VALUES(102066,'operator','eok_gt');
INSERT INTO kind VALUES(102067,'operator','eok_lt');
INSERT INTO kind VALUES(102068,'operator','eok_ge');
INSERT INTO kind VALUES(102069,'operator','eok_le');
INSERT INTO kind VALUES(102070,'operator','eok_spaceship');
INSERT INTO kind VALUES(102071,'operator','eok_vector_eq');
INSERT INTO kind VALUES(102072,'operator','eok_vector_ne');
INSERT INTO kind VALUES(102073,'operator','eok_vector_gt');
INSERT INTO kind VALUES(102074,'operator','eok_vector_lt');
INSERT INTO kind VALUES(102075,'operator','eok_vector_ge');
INSERT INTO kind VALUES(102076,'operator','eok_vector_le');
INSERT INTO kind VALUES(102077,'operator','eok_gnu_min');
INSERT INTO kind VALUES(102078,'operator','eok_gnu_max');
INSERT INTO kind VALUES(102079,'operator','eok_add_assign');
INSERT INTO kind VALUES(102080,'operator','eok_subtract_assign');
INSERT INTO kind VALUES(102081,'operator','eok_multiply_assign');
INSERT INTO kind VALUES(102082,'operator','eok_divide_assign');
INSERT INTO kind VALUES(102083,'operator','eok_remainder_assign');
INSERT INTO kind VALUES(102084,'operator','eok_shiftl_assign');
INSERT INTO kind VALUES(102085,'operator','eok_shiftr_assign');
INSERT INTO kind VALUES(102086,'operator','eok_and_assign');
INSERT INTO kind VALUES(102087,'operator','eok_or_assign');
INSERT INTO kind VALUES(102088,'operator','eok_xor_assign');
INSERT INTO kind VALUES(102089,'operator','eok_padd_assign');
INSERT INTO kind VALUES(102090,'operator','eok_psubtract_assign');
INSERT INTO kind VALUES(102091,'operator','eok_land');
INSERT INTO kind VALUES(102092,'operator','eok_lor');
INSERT INTO kind VALUES(102093,'operator','eok_vector_land');
INSERT INTO kind VALUES(102094,'operator','eok_vector_lor');
INSERT INTO kind VALUES(102095,'operator','eok_comma');
INSERT INTO kind VALUES(102096,'operator','eok_subscript');
INSERT INTO kind VALUES(102097,'operator','eok_vector_subscript');
INSERT INTO kind VALUES(102098,'operator','eok_dot_field');
INSERT INTO kind VALUES(102099,'operator','eok_points_to_field');
INSERT INTO kind VALUES(102100,'operator','eok_pm_field');
INSERT INTO kind VALUES(102101,'operator','eok_pm_points_to_field');
INSERT INTO kind VALUES(102102,'operator','eok_dot_pm_func_ptr');
INSERT INTO kind VALUES(102103,'operator','eok_points_to_pm_func_ptr');
INSERT INTO kind VALUES(102104,'operator','eok_dot_static');
INSERT INTO kind VALUES(102105,'operator','eok_points_to_static');
INSERT INTO kind VALUES(102106,'operator','eok_virtual_function_ptr');
INSERT INTO kind VALUES(102107,'operator','eok_question');
INSERT INTO kind VALUES(102108,'operator','eok_vector_question');
INSERT INTO kind VALUES(102109,'operator','eok_dot_member_call');
INSERT INTO kind VALUES(102110,'operator','eok_points_to_member_call');
INSERT INTO kind VALUES(102111,'operator','eok_dot_pm_call');
INSERT INTO kind VALUES(102112,'operator','eok_points_to_pm_call');
INSERT INTO kind VALUES(102113,'operator','eok_cli_subscript');
INSERT INTO kind VALUES(102114,'operator','eok_va_start');
INSERT INTO kind VALUES(102115,'operator','eok_va_arg');
INSERT INTO kind VALUES(102116,'operator','eok_va_end');
INSERT INTO kind VALUES(102117,'operator','eok_va_copy');
INSERT INTO kind VALUES(102118,'operator','eok_va_start_single_operand');
INSERT INTO kind VALUES(102119,'operator','eok_lvalue');
INSERT INTO kind VALUES(102120,'operator','eok_await');
INSERT INTO kind VALUES(102121,'operator','eok_yield');
INSERT INTO kind VALUES(102122,'operator','eok_splice');
INSERT INTO kind VALUES(102123,'operator','eok_error');
INSERT INTO kind VALUES(102124,'operator','eok_last');
INSERT INTO kind VALUES(103001,'builtin','bok_offsetof');
INSERT INTO kind VALUES(103002,'builtin','bok_has_assign');
INSERT INTO kind VALUES(103003,'builtin','bok_has_copy');
INSERT INTO kind VALUES(103004,'builtin','bok_has_nothrow_assign');
INSERT INTO kind VALUES(103005,'builtin','bok_has_nothrow_constructor');
INSERT INTO kind VALUES(103006,'builtin','bok_has_nothrow_copy');
INSERT INTO kind VALUES(103007,'builtin','bok_has_trivial_assign');
INSERT INTO kind VALUES(103008,'builtin','bok_has_trivial_constructor');
INSERT INTO kind VALUES(103009,'builtin','bok_has_trivial_copy');
INSERT INTO kind VALUES(103010,'builtin','bok_has_trivial_destructor');
INSERT INTO kind VALUES(103011,'builtin','bok_has_user_destructor');
INSERT INTO kind VALUES(103012,'builtin','bok_has_virtual_destructor');
INSERT INTO kind VALUES(103013,'builtin','bok_is_abstract');
INSERT INTO kind VALUES(103014,'builtin','bok_is_base_of');
INSERT INTO kind VALUES(103015,'builtin','bok_is_class');
INSERT INTO kind VALUES(103016,'builtin','bok_is_convertible_to');
INSERT INTO kind VALUES(103017,'builtin','bok_is_empty');
INSERT INTO kind VALUES(103018,'builtin','bok_is_enum');
INSERT INTO kind VALUES(103019,'builtin','bok_is_pod');
INSERT INTO kind VALUES(103020,'builtin','bok_is_polymorphic');
INSERT INTO kind VALUES(103021,'builtin','bok_is_union');
INSERT INTO kind VALUES(103022,'builtin','bok_types_compatible');
INSERT INTO kind VALUES(103023,'builtin','bok_intaddr');
INSERT INTO kind VALUES(103024,'builtin','bok_is_trivial');
INSERT INTO kind VALUES(103025,'builtin','bok_is_standard_layout');
INSERT INTO kind VALUES(103026,'builtin','bok_is_trivially_copyable');
INSERT INTO kind VALUES(103027,'builtin','bok_is_literal_type');
INSERT INTO kind VALUES(103028,'builtin','bok_has_trivial_move_constructor');
INSERT INTO kind VALUES(103029,'builtin','bok_has_trivial_move_assign');
INSERT INTO kind VALUES(103030,'builtin','bok_has_nothrow_move_assign');
INSERT INTO kind VALUES(103031,'builtin','bok_is_constructible');
INSERT INTO kind VALUES(103032,'builtin','bok_is_nothrow_constructible');
INSERT INTO kind VALUES(103033,'builtin','bok_has_finalizer');
INSERT INTO kind VALUES(103034,'builtin','bok_is_delegate');
INSERT INTO kind VALUES(103035,'builtin','bok_is_interface_class');
INSERT INTO kind VALUES(103036,'builtin','bok_is_ref_array');
INSERT INTO kind VALUES(103037,'builtin','bok_is_ref_class');
INSERT INTO kind VALUES(103038,'builtin','bok_is_sealed');
INSERT INTO kind VALUES(103039,'builtin','bok_is_simple_value_class');
INSERT INTO kind VALUES(103040,'builtin','bok_is_value_class');
INSERT INTO kind VALUES(103041,'builtin','bok_is_final');
INSERT INTO kind VALUES(103042,'builtin','bok_is_trivially_constructible');
INSERT INTO kind VALUES(103043,'builtin','bok_is_destructible');
INSERT INTO kind VALUES(103044,'builtin','bok_is_nothrow_destructible');
INSERT INTO kind VALUES(103045,'builtin','bok_is_trivially_destructible');
INSERT INTO kind VALUES(103046,'builtin','bok_is_nothrow_assignable');
INSERT INTO kind VALUES(103047,'builtin','bok_is_trivially_assignable');
INSERT INTO kind VALUES(103048,'builtin','bok_builtin_shuffle');
INSERT INTO kind VALUES(103049,'builtin','bok_builtin_complex');
INSERT INTO kind VALUES(103050,'builtin','bok_is_valid_winrt_type');
INSERT INTO kind VALUES(103051,'builtin','bok_is_win_class');
INSERT INTO kind VALUES(103052,'builtin','bok_is_win_interface');
INSERT INTO kind VALUES(103053,'builtin','bok_builtin_shufflevector');
INSERT INTO kind VALUES(103054,'builtin','bok_builtin_convertvector');
INSERT INTO kind VALUES(103055,'builtin','bok_is_assignable');
INSERT INTO kind VALUES(103056,'builtin','bok_is_trivially_copy_assignable');
INSERT INTO kind VALUES(103057,'builtin','bok_is_assignable_no_precondition_check');
INSERT INTO kind VALUES(103058,'builtin','bok_builtin_addressof');
INSERT INTO kind VALUES(103059,'builtin','bok_has_unique_object_representations');
INSERT INTO kind VALUES(103060,'builtin','bok_is_aggregate');
INSERT INTO kind VALUES(103061,'builtin','bok_reference_binds_to_temporary');
INSERT INTO kind VALUES(103062,'builtin','bok_is_same');
INSERT INTO kind VALUES(103063,'builtin','bok_is_same_as');
INSERT INTO kind VALUES(103064,'builtin','bok_is_function');
INSERT INTO kind VALUES(103065,'builtin','bok_builtin_has_attribute');
INSERT INTO kind VALUES(103066,'builtin','bok_builtin_bit_cast');
INSERT INTO kind VALUES(103067,'builtin','bok_is_layout_compatible');
INSERT INTO kind VALUES(103068,'builtin','bok_is_pointer_interconvertible_base_of');
INSERT INTO kind VALUES(103069,'builtin','bok_is_pointer_interconvertible_with_class');
INSERT INTO kind VALUES(103070,'builtin','bok_builtin_is_pointer_interconvertible_with_class');
INSERT INTO kind VALUES(103071,'builtin','bok_is_corresponding_member');
INSERT INTO kind VALUES(103072,'builtin','bok_builtin_is_corresponding_member');
INSERT INTO kind VALUES(103073,'builtin','bok_edg_is_deducible');
INSERT INTO kind VALUES(103074,'builtin','bok_is_array');
INSERT INTO kind VALUES(103075,'builtin','bok_array_rank');
INSERT INTO kind VALUES(103076,'builtin','bok_array_extent');
INSERT INTO kind VALUES(103077,'builtin','bok_is_arithmetic');
INSERT INTO kind VALUES(103078,'builtin','bok_is_complete_type');
INSERT INTO kind VALUES(103079,'builtin','bok_is_compound');
INSERT INTO kind VALUES(103080,'builtin','bok_is_const');
INSERT INTO kind VALUES(103081,'builtin','bok_is_floating_point');
INSERT INTO kind VALUES(103082,'builtin','bok_is_fundamental');
INSERT INTO kind VALUES(103083,'builtin','bok_is_integral');
INSERT INTO kind VALUES(103084,'builtin','bok_is_lvalue_reference');
INSERT INTO kind VALUES(103085,'builtin','bok_is_member_function_pointer');
INSERT INTO kind VALUES(103086,'builtin','bok_is_member_object_pointer');
INSERT INTO kind VALUES(103087,'builtin','bok_is_member_pointer');
INSERT INTO kind VALUES(103088,'builtin','bok_is_object');
INSERT INTO kind VALUES(103089,'builtin','bok_is_pointer');
INSERT INTO kind VALUES(103090,'builtin','bok_is_reference');
INSERT INTO kind VALUES(103091,'builtin','bok_is_rvalue_reference');
INSERT INTO kind VALUES(103092,'builtin','bok_is_scalar');
INSERT INTO kind VALUES(103093,'builtin','bok_is_signed');
INSERT INTO kind VALUES(103094,'builtin','bok_is_unsigned');
INSERT INTO kind VALUES(103095,'builtin','bok_is_void');
INSERT INTO kind VALUES(103096,'builtin','bok_is_volatile');
INSERT INTO kind VALUES(103097,'builtin','bok_is_bounded_array');
INSERT INTO kind VALUES(103098,'builtin','bok_is_unbounded_array');
INSERT INTO kind VALUES(103099,'builtin','bok_is_referenceable');
INSERT INTO kind VALUES(103100,'builtin','bok_is_nothrow_convertible');
INSERT INTO kind VALUES(103101,'builtin','bok_reference_constructs_from_temporary');
INSERT INTO kind VALUES(103102,'builtin','bok_reference_converts_from_temporary');
INSERT INTO kind VALUES(103103,'builtin','bok_is_convertible');
INSERT INTO kind VALUES(103104,'builtin','bok_is_trivially_equality_comparable');
INSERT INTO kind VALUES(103105,'builtin','bok_is_scoped_enum');
INSERT INTO kind VALUES(103106,'builtin','bok_is_trivially_relocatable');
INSERT INTO kind VALUES(103107,'builtin','bok_is_invocable');
INSERT INTO kind VALUES(103108,'builtin','bok_is_nothrow_invocable');
INSERT INTO kind VALUES(103109,'builtin','bok_is_bitwise_cloneable');
INSERT INTO kind VALUES(103110,'builtin','bok_builtin_is_virtual_base_of');
INSERT INTO kind VALUES(103111,'builtin','bok_builtin_is_implicit_lifetime');
INSERT INTO kind VALUES(103112,'builtin','bok_builtin_lt_synthesizes_from_spaceship');
INSERT INTO kind VALUES(103113,'builtin','bok_builtin_gt_synthesizes_from_spaceship');
INSERT INTO kind VALUES(103114,'builtin','bok_builtin_le_synthesizes_from_spaceship');
INSERT INTO kind VALUES(103115,'builtin','bok_builtin_ge_synthesizes_from_spaceship');
INSERT INTO kind VALUES(103116,'builtin','bok_builtin_is_structural');
INSERT INTO kind VALUES(103117,'builtin','bok_last');
INSERT INTO kind VALUES(104001,'constant','ck_stack_offset');
INSERT INTO kind VALUES(104002,'constant','ck_error');
INSERT INTO kind VALUES(104003,'constant','ck_last');
INSERT INTO kind VALUES(104004,'constant','ck_integer');
INSERT INTO kind VALUES(104005,'constant','ck_fixed_point');
INSERT INTO kind VALUES(104006,'constant','ck_string');
INSERT INTO kind VALUES(104007,'constant','ck_float');
INSERT INTO kind VALUES(104008,'constant','ck_complex');
INSERT INTO kind VALUES(104009,'constant','ck_imaginary');
INSERT INTO kind VALUES(104010,'constant','ck_address');
INSERT INTO kind VALUES(104011,'constant','ck_ptr_to_member');
INSERT INTO kind VALUES(104012,'constant','ck_label_difference');
INSERT INTO kind VALUES(104013,'constant','ck_dynamic_init');
INSERT INTO kind VALUES(104014,'constant','ck_aggregate');
INSERT INTO kind VALUES(104015,'constant','ck_init_repeat');
INSERT INTO kind VALUES(104016,'constant','ck_designator');
INSERT INTO kind VALUES(104017,'constant','ck_upc_threads');
INSERT INTO kind VALUES(104018,'constant','ck_upc_mythread');
INSERT INTO kind VALUES(104019,'constant','ck_void');
INSERT INTO kind VALUES(104020,'constant','ck_template_param');
INSERT INTO kind VALUES(104021,'constant','ck_reflection');
INSERT INTO kind VALUES(105001,'address_base','abk_routine');
INSERT INTO kind VALUES(105002,'address_base','abk_variable');
INSERT INTO kind VALUES(105003,'address_base','abk_constant');
INSERT INTO kind VALUES(105004,'address_base','abk_temporary');
INSERT INTO kind VALUES(105005,'address_base','abk_uuidof');
INSERT INTO kind VALUES(105006,'address_base','abk_typeid');
INSERT INTO kind VALUES(105007,'address_base','abk_cli_typeid');
INSERT INTO kind VALUES(105008,'address_base','abk_cli_array');
INSERT INTO kind VALUES(105009,'address_base','abk_label');
INSERT INTO kind VALUES(105010,'address_base','abk_param_ref');
INSERT INTO kind VALUES(105011,'address_base','abk_last');
INSERT INTO kind VALUES(106001,'type','tk_class');
INSERT INTO kind VALUES(106002,'type','tk_struct');
INSERT INTO kind VALUES(106003,'type','tk_error');
INSERT INTO kind VALUES(106004,'type','tk_unknown');
INSERT INTO kind VALUES(106005,'type','tk_void');
INSERT INTO kind VALUES(106006,'type','tk_integer');
INSERT INTO kind VALUES(106007,'type','tk_fixed_point');
INSERT INTO kind VALUES(106008,'type','tk_float');
INSERT INTO kind VALUES(106009,'type','tk_imaginary');
INSERT INTO kind VALUES(106010,'type','tk_complex');
INSERT INTO kind VALUES(106011,'type','tk_pointer');
INSERT INTO kind VALUES(106012,'type','tk_routine');
INSERT INTO kind VALUES(106013,'type','tk_array');
INSERT INTO kind VALUES(106014,'type','tk_union');
INSERT INTO kind VALUES(106015,'type','tk_typeref');
INSERT INTO kind VALUES(106016,'type','tk_ptr_to_member');
INSERT INTO kind VALUES(106017,'type','tk_vector');
INSERT INTO kind VALUES(106018,'type','tk_scalable_vector');
INSERT INTO kind VALUES(106019,'type','tk_scalable_vector_count');
INSERT INTO kind VALUES(106020,'type','tk_riscv_vector');
INSERT INTO kind VALUES(106021,'type','tk_mfp8');
INSERT INTO kind VALUES(106022,'type','tk_float8e4m3');
INSERT INTO kind VALUES(106023,'type','tk_float8e5m2');
INSERT INTO kind VALUES(106024,'type','tk_nullptr');
INSERT INTO kind VALUES(106025,'type','tk_enum');
INSERT INTO kind VALUES(106026,'type','tk_template_param');
INSERT INTO kind VALUES(106027,'type','tk_reflection');
INSERT INTO kind VALUES(107001,'dynamic_init','dik_none');
INSERT INTO kind VALUES(107002,'dynamic_init','dik_zero');
INSERT INTO kind VALUES(107003,'dynamic_init','dik_constant');
INSERT INTO kind VALUES(107004,'dynamic_init','dik_expression');
INSERT INTO kind VALUES(107005,'dynamic_init','dik_class_result_via_ctor');
INSERT INTO kind VALUES(107006,'dynamic_init','dik_constructor');
INSERT INTO kind VALUES(107007,'dynamic_init','dik_nonconstant_aggregate');
INSERT INTO kind VALUES(107008,'dynamic_init','dik_bitwise_copy');
INSERT INTO kind VALUES(107009,'dynamic_init','dik_lambda');
INSERT INTO kind VALUES(107010,'dynamic_init','dik_call_returning_class_via_cctor');
INSERT INTO kind VALUES(108001,'constructor_init','cik_virtual_base_class');
INSERT INTO kind VALUES(108002,'constructor_init','cik_direct_base_class');
INSERT INTO kind VALUES(108003,'constructor_init','cik_field');
INSERT INTO kind VALUES(108004,'constructor_init','cik_delegation');
INSERT INTO kind VALUES(109001,'template_argument','tak_type');
INSERT INTO kind VALUES(109002,'template_argument','tak_nontype');
INSERT INTO kind VALUES(109003,'template_argument','tak_template');
INSERT INTO kind VALUES(109004,'template_argument','tak_start_of_pack_expansion');
INSERT INTO kind VALUES(110001,'template','templk_none');
INSERT INTO kind VALUES(110002,'template','templk_class');
INSERT INTO kind VALUES(110003,'template','templk_function');
INSERT INTO kind VALUES(110004,'template','templk_variable');
INSERT INTO kind VALUES(110005,'template','templk_member_function');
INSERT INTO kind VALUES(110006,'template','templk_static_data_member');
INSERT INTO kind VALUES(110007,'template','templk_member_class');
INSERT INTO kind VALUES(110008,'template','templk_member_enum');
INSERT INTO kind VALUES(110009,'template','templk_template_template_param');
INSERT INTO kind VALUES(110010,'template','templk_concept');
INSERT INTO kind VALUES(111001,'lifetime','olk_global_static');
INSERT INTO kind VALUES(111002,'lifetime','olk_block');
INSERT INTO kind VALUES(111003,'lifetime','olk_block_after_label');
INSERT INTO kind VALUES(111004,'lifetime','olk_function_static');
INSERT INTO kind VALUES(111005,'lifetime','olk_expr_temporary');
INSERT INTO kind VALUES(111006,'lifetime','olk_try_block');
INSERT INTO kind VALUES(112001,'lowered_eh','leck_function_prologue');
INSERT INTO kind VALUES(112002,'lowered_eh','leck_cleanup_state');
INSERT INTO kind VALUES(112003,'lowered_eh','leck_initialization_completed');
INSERT INTO kind VALUES(112004,'lowered_eh','leck_caught_object_address');
INSERT INTO kind VALUES(112005,'lowered_eh','leck_thrown_object_address');
INSERT INTO kind VALUES(112006,'lowered_eh','leck_unreachable_cleanup_state');
INSERT INTO kind VALUES(112007,'lowered_eh','leck_function_epilogue');
INSERT INTO kind VALUES(112008,'lowered_eh','leck_catch_epilogue');
INSERT INTO kind VALUES(112009,'lowered_eh','leck_try_epilogue');
INSERT INTO kind VALUES(112010,'lowered_eh','leck_exception_caught');
INSERT INTO kind VALUES(112011,'lowered_eh','leck_exception_started');
INSERT INTO kind VALUES(112012,'lowered_eh','leck_internal_try');
INSERT INTO kind VALUES(113001,'template_constant','tpck_unknown_function');
INSERT INTO kind VALUES(113002,'template_constant','tpck_param');
INSERT INTO kind VALUES(113003,'template_constant','tpck_expression');
INSERT INTO kind VALUES(113004,'template_constant','tpck_member');
INSERT INTO kind VALUES(113005,'template_constant','tpck_address');
INSERT INTO kind VALUES(113006,'template_constant','tpck_sizeof');
INSERT INTO kind VALUES(113007,'template_constant','tpck_datasizeof');
INSERT INTO kind VALUES(113008,'template_constant','tpck_alignof');
INSERT INTO kind VALUES(113009,'template_constant','tpck_uuidof');
INSERT INTO kind VALUES(113010,'template_constant','tpck_typeid');
INSERT INTO kind VALUES(113011,'template_constant','tpck_noexcept');
INSERT INTO kind VALUES(113012,'template_constant','tpck_template_ref');
INSERT INTO kind VALUES(113013,'template_constant','tpck_integer_pack');
INSERT INTO kind VALUES(113014,'template_constant','tpck_destructor');
INSERT INTO kind VALUES(113015,'template_constant','tpck_dependent_constant');
INSERT INTO kind VALUES(113016,'template_constant','tpck_concat_string_literals');
INSERT INTO kind VALUES(114001,'template_type','tptk_param');
INSERT INTO kind VALUES(114002,'template_type','tptk_member');
INSERT INTO kind VALUES(114003,'template_type','tptk_unknown');
INSERT INTO kind VALUES(114004,'template_type','tptk_bit_precise_int');
INSERT INTO kind VALUES(115001,'for_each_pattern','sfepk_none');
INSERT INTO kind VALUES(115002,'for_each_pattern','sfepk_stl_pattern');
INSERT INTO kind VALUES(115003,'for_each_pattern','sfepk_cli_pattern');
INSERT INTO kind VALUES(115004,'for_each_pattern','sfepk_cli_array_pattern');
INSERT INTO kind VALUES(115005,'for_each_pattern','sfepk_array_pattern');
INSERT INTO kind VALUES(116001,'template_parameter','tpk_error');
INSERT INTO kind VALUES(116002,'template_parameter','tpk_type');
INSERT INTO kind VALUES(116003,'template_parameter','tpk_nontype');
INSERT INTO kind VALUES(116004,'template_parameter','tpk_template');
INSERT INTO kind VALUES(117001,'initializer','initk_none');
INSERT INTO kind VALUES(117002,'initializer','initk_static');
INSERT INTO kind VALUES(117003,'initializer','initk_dynamic');
INSERT INTO kind VALUES(117004,'initializer','initk_zero');
INSERT INTO kind VALUES(117005,'initializer','initk_function_local');
INSERT INTO kind VALUES(117006,'initializer','initk_binding');
INSERT INTO kind VALUES(118001,'local_expression_reference','lerk_none');
INSERT INTO kind VALUES(118002,'local_expression_reference','lerk_generic_sizeof');
INSERT INTO kind VALUES(118003,'local_expression_reference','lerk_tpl_param_expr');
INSERT INTO kind VALUES(118004,'local_expression_reference','lerk_array_bound');
INSERT INTO kind VALUES(118005,'local_expression_reference','lerk_dep_array_bound');
INSERT INTO kind VALUES(118006,'local_expression_reference','lerk_decltype');
INSERT INTO kind VALUES(118007,'local_expression_reference','lerk_bit_field_width');
INSERT INTO kind VALUES(118008,'local_expression_reference','lerk_constant_expr');
INSERT INTO kind VALUES(118009,'local_expression_reference','lerk_scoped_expr');
INSERT INTO kind VALUES(200001,'edg_integer_kind','ik_char');
INSERT INTO kind VALUES(200002,'edg_integer_kind','ik_signed_char');
INSERT INTO kind VALUES(200003,'edg_integer_kind','ik_unsigned_char');
INSERT INTO kind VALUES(200004,'edg_integer_kind','ik_short');
INSERT INTO kind VALUES(200005,'edg_integer_kind','ik_unsigned_short');
INSERT INTO kind VALUES(200006,'edg_integer_kind','ik_int');
INSERT INTO kind VALUES(200007,'edg_integer_kind','ik_unsigned_int');
INSERT INTO kind VALUES(200008,'edg_integer_kind','ik_long');
INSERT INTO kind VALUES(200009,'edg_integer_kind','ik_unsigned_long');
INSERT INTO kind VALUES(200010,'edg_integer_kind','ik_long_long');
INSERT INTO kind VALUES(200011,'edg_integer_kind','ik_unsigned_long_long');
INSERT INTO kind VALUES(200012,'edg_integer_kind','ik_int128');
INSERT INTO kind VALUES(200013,'edg_integer_kind','ik_unsigned_int128');
INSERT INTO kind VALUES(200014,'edg_integer_kind','ik_bit_precise');
INSERT INTO kind VALUES(200015,'edg_integer_kind','ik_unsigned_bit_precise');
INSERT INTO kind VALUES(200016,'edg_integer_kind','ik_last');
INSERT INTO kind VALUES(200017,'edg_integer_kind','ik_none');
INSERT INTO kind VALUES(201001,'edg_float_kind','fk_float16');
INSERT INTO kind VALUES(201002,'edg_float_kind','fk_fp16');
INSERT INTO kind VALUES(201003,'edg_float_kind','fk_float');
INSERT INTO kind VALUES(201004,'edg_float_kind','fk_float32x');
INSERT INTO kind VALUES(201005,'edg_float_kind','fk_double');
INSERT INTO kind VALUES(201006,'edg_float_kind','fk_float64x');
INSERT INTO kind VALUES(201007,'edg_float_kind','fk_long_double');
INSERT INTO kind VALUES(201008,'edg_float_kind','fk_float80');
INSERT INTO kind VALUES(201009,'edg_float_kind','fk_float128');
INSERT INTO kind VALUES(201010,'edg_float_kind','fk_first_extended_type');
INSERT INTO kind VALUES(201011,'edg_float_kind','fk_std_bfloat16');
INSERT INTO kind VALUES(201012,'edg_float_kind','fk_std_float16');
INSERT INTO kind VALUES(201013,'edg_float_kind','fk_std_float32');
INSERT INTO kind VALUES(201014,'edg_float_kind','fk_std_float64');
INSERT INTO kind VALUES(201015,'edg_float_kind','fk_std_float128');
INSERT INTO kind VALUES(201016,'edg_float_kind','fk_last');
INSERT INTO kind VALUES(202001,'edg_fixed_precision','fpp_short');
INSERT INTO kind VALUES(202002,'edg_fixed_precision','fpp_default');
INSERT INTO kind VALUES(202003,'edg_fixed_precision','fpp_long');
INSERT INTO kind VALUES(202004,'edg_fixed_precision','fpp_last');
INSERT INTO kind VALUES(203001,'rewritten_property','rprk_none');
INSERT INTO kind VALUES(203002,'rewritten_property','rprk_compound_assignment');
INSERT INTO kind VALUES(203003,'rewritten_property','rprk_pre_incr_decr');
INSERT INTO kind VALUES(203004,'rewritten_property','rprk_post_incr_decr');
INSERT INTO kind VALUES(203005,'rewritten_property','rprk_comma_discard_first');
INSERT INTO kind VALUES(203006,'rewritten_property','rprk_comma_discard_second');
INSERT INTO kind VALUES(1050,'entity','constant');
INSERT INTO kind VALUES(1051,'entity','constant_value');
INSERT INTO kind VALUES(1052,'entity','handler');
INSERT INTO kind VALUES(1053,'entity','template_header');
INSERT INTO kind VALUES(1054,'entity','lambda_definition');
INSERT INTO kind VALUES(47001,'eh_table_kind','region');
INSERT INTO kind VALUES(47002,'eh_table_kind','array');
INSERT INTO kind VALUES(1055,'entity','base_derivation');
INSERT INTO kind VALUES(48001,'string_unit_kind','target_bytes');
INSERT INTO kind VALUES(48002,'string_unit_kind','target_characters');
INSERT INTO kind VALUES(1056,'entity','attribute_group');
INSERT INTO kind VALUES(1057,'entity','generic_constraint_clause');
INSERT INTO kind VALUES(1058,'entity','type_symbol');
INSERT INTO kind VALUES(1059,'entity','static_assertion');
INSERT INTO kind VALUES(1060,'entity','name_qualifier');
INSERT INTO kind VALUES(1061,'entity','orphan_scope');
INSERT INTO kind VALUES(49001,'root_kind','resident_region');
INSERT INTO kind VALUES(49002,'root_kind','scope_orphan');
INSERT INTO kind VALUES(49003,'root_kind','nontag_type');
INSERT INTO kind VALUES(49004,'root_kind','file_scope_statement');
INSERT INTO kind VALUES(49005,'root_kind','file_initializer');
INSERT INTO kind VALUES(49006,'root_kind','retained_declaration');
INSERT INTO kind VALUES(50001,'constant_component_role','element');
INSERT INTO kind VALUES(50002,'constant_component_role','repeated');
INSERT INTO kind VALUES(50003,'constant_component_role','dependent');
INSERT INTO kind VALUES(50004,'constant_component_role','designator');
INSERT INTO kind VALUES(50005,'constant_component_role','from');
INSERT INTO kind VALUES(50006,'constant_component_role','to');
INSERT INTO kind VALUES(23004,'specialization_kind','retained_specialization');
INSERT INTO kind VALUES(1062,'entity','token_sequence');
INSERT INTO kind VALUES(1063,'entity','parameter_type');
INSERT INTO kind VALUES(6042,'role','virtual_base_parameter');
INSERT INTO kind VALUES(6043,'role','destructor_control_parameter');
INSERT INTO kind VALUES(1064,'entity','enumerator_symbol');
INSERT INTO kind VALUES(3004,'abi','cfront_like');
INSERT INTO kind VALUES(51001,'source_dispatch_kind','non_virtual');
INSERT INTO kind VALUES(51002,'source_dispatch_kind','virtual');
INSERT INTO kind VALUES(51003,'source_dispatch_kind','member_pointer');
INSERT INTO kind VALUES(5014,'fact','original_signature');
INSERT INTO kind VALUES(5015,'fact','size');
INSERT INTO kind VALUES(5016,'fact','alignment');
INSERT INTO kind VALUES(5017,'fact','layout');
INSERT INTO kind VALUES(1065,'entity','module');
INSERT INTO kind VALUES(1066,'entity','module_import');
INSERT INTO kind VALUES(1067,'entity','asm_entry');
INSERT INTO kind VALUES(1068,'entity','line_directive');
INSERT INTO kind VALUES(1069,'entity','source_entry');
INSERT INTO kind VALUES(1070,'entity','source_sublist');
INSERT INTO kind VALUES(1071,'entity','source_end_construct');
INSERT INTO kind VALUES(1072,'entity','instantiation_directive');
INSERT INTO kind VALUES(1073,'entity','linkage_specification');
INSERT INTO kind VALUES(1074,'entity','microsoft_if_exists');
INSERT INTO kind VALUES(1075,'entity','record_declaration');
INSERT INTO kind VALUES(1076,'entity','pragma');
INSERT INTO kind VALUES(1077,'entity','ms_attribute');
INSERT INTO kind VALUES(1078,'entity','ms_attribute_argument');
INSERT INTO kind VALUES(1079,'entity','scoped_expression');
INSERT INTO kind VALUES(1080,'entity','template_symbol');
INSERT INTO kind VALUES(21007,'source_origin_kind','preinclude');
INSERT INTO kind VALUES(19005,'storage_location_kind','no_object');
INSERT INTO kind VALUES(1081,'entity','attribute_argument');
INSERT INTO kind VALUES(1082,'entity','block_descriptor');
INSERT INTO kind VALUES(1083,'entity','try_descriptor');
INSERT INTO kind VALUES(1084,'entity','local_static_initialization');
INSERT INTO kind VALUES(1085,'entity','constructor_initializer');
INSERT INTO kind VALUES(1086,'entity','data_member_spec');
INSERT INTO kind VALUES(19006,'storage_location_kind','thrown_object_relative');
INSERT INTO kind_disposition VALUES(1001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1025,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1028,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1029,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1030,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1031,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1032,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1033,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1034,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1035,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1036,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1037,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1038,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1039,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1040,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1041,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1042,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1043,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1044,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1045,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1046,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1047,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1048,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1049,'semantic',NULL);
INSERT INTO kind_disposition VALUES(2001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(2002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(2003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(2004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(3001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(3002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(3003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(4007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6025,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6028,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6029,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6030,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6031,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6032,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6033,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6034,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6035,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6036,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6037,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6038,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6039,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6040,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6041,'semantic',NULL);
INSERT INTO kind_disposition VALUES(7001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(7002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(7003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(8001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(8002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(8003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(9001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(9002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(9003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(9004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(10001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(10002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(10003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(10004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(11001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(11002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(11003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(11004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(11005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(11006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(12001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(12002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(12003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(12004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(13001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(13002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(13003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(13004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(13005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(14001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(14002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(14003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(15001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(15002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(15003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(15004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(15005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(16009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(17001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(17002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(17003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(17004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(17005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(17006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(18001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(18002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(18003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(18004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(18005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(19001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(19002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(19003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(19004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(20001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(20002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(20003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(22001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(22002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(22003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(23001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(23002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(23003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(24001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(24002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(24003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(24004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(25007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(26001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(26002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(27001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(27002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(27003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(27004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(28001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(28002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(28003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(29007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(30001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(30002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(30003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(30004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(30005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(31001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(31002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(31003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(31004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(31005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(32001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(32002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(32003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(32004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(33001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(33002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(33003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(34001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(34002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(34003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(34004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(34005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(34006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(35001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(35002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(35003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(35004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(35005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(36001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(36002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(36003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(36004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(37001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(37002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(37003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(38001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(38002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(38003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(39001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(39002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(39003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(40001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(40002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(40003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(40004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(40005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(41008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(42001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(42002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(42003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(43001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(43002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(43003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(43004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(43005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(44007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(45007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(46008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100025,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100028,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100029,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100030,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100031,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100032,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100033,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100034,'semantic',NULL);
INSERT INTO kind_disposition VALUES(100035,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(101001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101002,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(101003,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(101004,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(101005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101025,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101028,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101029,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101030,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101031,'alias',101017);
INSERT INTO kind_disposition VALUES(101032,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101033,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101034,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101035,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101036,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101037,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101038,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101039,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101040,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101041,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101042,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101043,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101044,'semantic',NULL);
INSERT INTO kind_disposition VALUES(101045,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102025,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102028,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102029,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102030,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102031,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102032,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102033,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102034,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102035,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102036,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102037,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102038,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102039,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102040,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102041,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102042,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102043,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102044,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102045,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102046,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102047,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102048,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102049,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102050,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102051,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102052,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102053,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102054,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102055,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102056,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102057,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102058,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102059,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102060,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102061,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102062,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102063,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102064,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102065,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102066,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102067,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102068,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102069,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102070,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102071,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102072,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102073,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102074,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102075,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102076,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102077,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102078,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102079,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102080,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102081,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102082,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102083,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102084,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102085,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102086,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102087,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102088,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102089,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102090,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102091,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102092,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102093,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102094,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102095,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102096,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102097,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102098,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102099,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102100,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102101,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102102,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102103,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102104,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102105,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102106,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102107,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102108,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102109,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102110,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102111,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102112,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102113,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102114,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102115,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102116,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102117,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102118,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102119,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102120,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102121,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102122,'semantic',NULL);
INSERT INTO kind_disposition VALUES(102123,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(102124,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(103001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103025,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103028,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103029,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103030,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103031,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103032,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103033,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103034,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103035,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103036,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103037,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103038,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103039,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103040,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103041,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103042,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103043,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103044,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103045,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103046,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103047,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103048,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103049,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103050,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103051,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103052,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103053,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103054,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103055,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103056,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103057,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103058,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103059,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103060,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103061,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103062,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103063,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103064,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103065,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103066,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103067,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103068,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103069,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103070,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103071,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103072,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103073,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103074,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103075,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103076,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103077,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103078,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103079,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103080,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103081,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103082,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103083,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103084,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103085,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103086,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103087,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103088,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103089,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103090,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103091,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103092,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103093,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103094,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103095,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103096,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103097,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103098,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103099,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103100,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103101,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103102,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103103,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103104,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103105,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103106,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103107,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103108,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103109,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103110,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103111,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103112,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103113,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103114,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103115,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103116,'semantic',NULL);
INSERT INTO kind_disposition VALUES(103117,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(104001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104002,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(104003,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(104004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(104021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(105011,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(106001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106003,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(106004,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(106005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106018,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106019,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106020,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106021,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106022,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106023,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106024,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106025,'alias',106006);
INSERT INTO kind_disposition VALUES(106026,'semantic',NULL);
INSERT INTO kind_disposition VALUES(106027,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(107010,'alias',107005);
INSERT INTO kind_disposition VALUES(108001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(108002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(108003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(108004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(109001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(109002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(109003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(109004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(110010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(111001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(111002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(111003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(111004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(111005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(111006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(112012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(113016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(114001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(114002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(114003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(114004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(115001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(115002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(115003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(115004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(115005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(116001,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(116002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(116003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(116004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(117001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(117002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(117003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(117004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(117005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(117006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118001,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(118002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(118009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(200016,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(200017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201008,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201009,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201010,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201011,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201012,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201013,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(201016,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(202001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(202002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(202003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(202004,'invalid_concrete_node',NULL);
INSERT INTO kind_disposition VALUES(203001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(203002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(203003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(203004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(203005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(203006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1050,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1051,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1052,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1053,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1054,'semantic',NULL);
INSERT INTO kind_disposition VALUES(47001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(47002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1055,'semantic',NULL);
INSERT INTO kind_disposition VALUES(48001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(48002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1056,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1057,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1058,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1059,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1060,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1061,'semantic',NULL);
INSERT INTO kind_disposition VALUES(49001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(49002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(49003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(49004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(49005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(49006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(50001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(50002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(50003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(50004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(50005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(50006,'semantic',NULL);
INSERT INTO kind_disposition VALUES(23004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1062,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1063,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6042,'semantic',NULL);
INSERT INTO kind_disposition VALUES(6043,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1064,'semantic',NULL);
INSERT INTO kind_disposition VALUES(3004,'semantic',NULL);
INSERT INTO kind_disposition VALUES(51001,'semantic',NULL);
INSERT INTO kind_disposition VALUES(51002,'semantic',NULL);
INSERT INTO kind_disposition VALUES(51003,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5014,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5015,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5016,'semantic',NULL);
INSERT INTO kind_disposition VALUES(5017,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1065,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1066,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1067,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1068,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1069,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1070,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1071,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1072,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1073,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1074,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1075,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1076,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1077,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1078,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1079,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1080,'semantic',NULL);
INSERT INTO kind_disposition VALUES(21007,'semantic',NULL);
INSERT INTO kind_disposition VALUES(19005,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1081,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1082,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1083,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1084,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1085,'semantic',NULL);
INSERT INTO kind_disposition VALUES(1086,'semantic',NULL);
INSERT INTO kind_disposition VALUES(19006,'semantic',NULL);
CREATE INDEX declaration_symbol ON declaration(symbol_id);
CREATE INDEX structural_parent ON structural_child(child_id);
CREATE INDEX semantic_reference_target ON semantic_reference(target_id);
CREATE INDEX macro_parent ON macro_invocation(parent_id,ordinal);
CREATE INDEX cleanup_function ON cleanup_region(function_eh_id,ordinal);
CREATE INDEX template_instance_template ON template_instance(template_id);
CREATE INDEX lifetime_binding ON lifetime(bound_entity_id);
CREATE INDEX constant_component_target ON constant_component(constant_id);
CREATE INDEX reference_expression_target ON reference_expression(target_id);
CREATE INDEX enumerator_constant ON enumerator(constant_id);
CREATE INDEX enumerator_symbol ON enumerator(symbol_id);
CREATE INDEX source_correspondence_current_name ON source_correspondence(current_name_id);
CREATE INDEX source_correspondence_auxiliary_name ON source_correspondence(unmangled_name_or_mangled_encoding_id);
CREATE INDEX correspondence_entities ON entity_source_correspondence(source_correspondence_id);
CREATE INDEX source_correspondence_parent ON source_correspondence(parent_scope_id);
CREATE INDEX source_correspondence_routine ON source_correspondence(enclosing_routine_id);
)roslynsql";
