#!/usr/bin/env python3
"""Generate the shared relational contract and stable catalog, never IL data."""
from pathlib import Path
import re
import sqlite3

HERE = Path(__file__).resolve().parent
DESIGN = HERE.parent.parent / 'roslyn-design' / 'semantic-inventory.md'
tables = []
def table(name, columns, key='id', entity=False):
    cols = []
    if key == 'id':
        cols.append('id INTEGER PRIMARY KEY' + (' REFERENCES entity(id) DEFERRABLE INITIALLY DEFERRED' if entity else ''))
    for line in columns.strip().splitlines():
        if not line.strip(): continue
        name_, typ, *rest = line.split()
        spec = ' '.join(rest)
        if typ.startswith('@'):
            typ, spec = 'INTEGER', 'REFERENCES ' + typ[1:] + '(id) DEFERRABLE INITIALLY DEFERRED ' + spec
        cols.append(f'{name_} {typ} {spec}'.rstrip())
    if key != 'id': cols.append('PRIMARY KEY (' + key + ')')
    storage = ' WITHOUT ROWID, STRICT;' if ',' in key else ' STRICT;'
    tables.append('CREATE TABLE ' + name + ' (\n  ' + ',\n  '.join(cols) + '\n)' + storage)

def entity(name, columns): table(name, columns, entity=True)
def relation(name, columns, key='owner_id, ordinal'): table(name, columns, key)

def append_columns(name, columns):
    """Add reviewed fields to an existing typed family without allocating IDs."""
    table(name, columns)
    extension = tables.pop().split('\n', 1)[1].rsplit('\n)', 1)[0]
    extension = extension.split('\n', 1)[1]  # Drop the temporary id column.
    for index, definition in enumerate(tables):
        if definition.startswith('CREATE TABLE ' + name + ' ('):
            existing = set(re.findall(r'^  (\w+) ', definition, re.M))
            added = set(re.findall(r'^  (\w+) ', extension, re.M))
            assert not existing & added, (name, existing & added)
            tables[index] = definition.replace('\n) STRICT;', ',\n' + extension + '\n) STRICT;')
            return
    raise ValueError('Unknown typed table ' + name)
def value_table(name, columns):
    table(name, columns)
    tables[-1] = tables[-1].replace('id INTEGER PRIMARY KEY', 'id INTEGER PRIMARY KEY REFERENCES constant_value(id) DEFERRABLE INITIALLY DEFERRED', 1)

tables.append('CREATE TABLE kind (id INTEGER PRIMARY KEY, family TEXT NOT NULL, name TEXT NOT NULL, UNIQUE(family,name)) STRICT;')
relation('kind_disposition', 'kind_id @kind NOT NULL\nstate TEXT NOT NULL\ncanonical_kind_id @kind', 'kind_id')
table('text_atom', 'value TEXT NOT NULL COLLATE BINARY UNIQUE')
table('entity', 'kind_id @kind NOT NULL')
table('object_header', '''format_major INTEGER NOT NULL
format_minor INTEGER NOT NULL
artifact_id TEXT NOT NULL
producer_id @text_atom NOT NULL
revision_id @text_atom NOT NULL
edg_version_id @text_atom NOT NULL
capture_profile_id @kind NOT NULL
target_profile_id @target_profile NOT NULL
main_source_file_id @source_file
translation_unit_id @translation_unit NOT NULL
complete INTEGER NOT NULL CHECK(complete=1)''')
table('target_profile', '''abi_id @kind NOT NULL
addressable_unit_bits INTEGER NOT NULL
little_endian INTEGER NOT NULL
pointer_bits INTEGER NOT NULL
pointer_alignment TEXT NOT NULL
description_id @text_atom''')
entity('translation_unit', 'global_scope_id @scope NOT NULL\nprimary_input_id @source_file\nlanguage_mode_id @text_atom NOT NULL\nentry_point_id @routine_symbol')
relation('translation_unit_root', 'owner_id @translation_unit NOT NULL\nkind_id @kind NOT NULL\nordinal INTEGER NOT NULL\nentity_id @entity NOT NULL', 'owner_id, kind_id, ordinal')
relation('file_scope_dynamic_init_routine', 'owner_id @translation_unit NOT NULL\nordinal INTEGER NOT NULL\nroutine_id @routine_symbol NOT NULL\nis_thread_local INTEGER NOT NULL DEFAULT 0')
entity('orphan_scope', 'translation_unit_id @translation_unit NOT NULL\nroutine_id @routine_symbol\nscope_number TEXT NOT NULL')
relation('orphan_scope_member', 'owner_id @orphan_scope NOT NULL\nkind_id @kind NOT NULL\nordinal INTEGER NOT NULL\nentity_id @entity NOT NULL', 'owner_id, kind_id, ordinal')
relation('build_capability', 'name_id @text_atom NOT NULL\nstate_id @kind NOT NULL\nvalue_id @text_atom\nrequired INTEGER NOT NULL DEFAULT 0\nenabled INTEGER', 'name_id')
relation('invocation_argument', 'ordinal INTEGER NOT NULL\ntext_id @text_atom NOT NULL', 'ordinal')
relation('fact_availability', 'owner_id @entity NOT NULL\nfact_id @kind NOT NULL\nstate_id @kind NOT NULL\nreason_id @text_atom', 'owner_id, fact_id')
entity('name', 'spelling_id @text_atom\ncategory_id @kind NOT NULL')
relation('qualified_name_part', 'owner_id @name NOT NULL\nordinal INTEGER NOT NULL\nname_id @name NOT NULL')
entity('scope', 'kind_id @kind NOT NULL\nparent_id @scope\nassociated_entity_id @entity\nlifetime_id @lifetime')
relation('scope_member', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\nmember_id @entity NOT NULL')
relation('scope_initialization', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\ninitialization_id @initialization NOT NULL')
entity('symbol', '''kind_id @kind NOT NULL
name_id @name
containing_entity_id @entity
type_id @type
source_origin_id @source_origin
linkage_id @kind
storage_id @kind
access_id @kind
compiler_generated INTEGER NOT NULL DEFAULT 0''')
entity('type_symbol', 'type_id @type NOT NULL')
entity('declaration', 'kind_id @kind NOT NULL\nsymbol_id @entity NOT NULL\nlexical_scope_id @scope\nsource_origin_id @source_origin\ncompiler_generated INTEGER NOT NULL DEFAULT 0\nimplicit INTEGER')
relation('declaration_sequence', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\ndeclaration_id @declaration NOT NULL')
entity('namespace_symbol', 'alias_target_id @namespace_symbol\nis_inline INTEGER NOT NULL DEFAULT 0\nis_anonymous INTEGER NOT NULL DEFAULT 0\nscope_id @scope')
entity('routine_symbol', '''routine_type_id @type NOT NULL
containing_type_id @type
body_root_id @semantic_node
body_scope_id @scope
body_state_id @kind NOT NULL
special_function_id @kind
builtin_kind_id @kind
runtime_kind_id @kind
template_id @template_definition
instance_id @template_instance
primary_entry_id @routine_symbol
is_virtual INTEGER NOT NULL DEFAULT 0
is_pure INTEGER NOT NULL DEFAULT 0
is_final INTEGER NOT NULL DEFAULT 0
is_inline INTEGER NOT NULL DEFAULT 0
is_constexpr INTEGER NOT NULL DEFAULT 0
is_consteval INTEGER NOT NULL DEFAULT 0
is_deleted INTEGER NOT NULL DEFAULT 0
is_defaulted INTEGER NOT NULL DEFAULT 0
is_tls_initialization_routine INTEGER NOT NULL DEFAULT 0
is_tls_initialization_alias INTEGER NOT NULL DEFAULT 0''')
relation('routine_parameter', 'owner_id @routine_symbol NOT NULL\nordinal INTEGER NOT NULL\nparameter_id @variable_symbol NOT NULL\nrole_id @kind NOT NULL\ndefault_initialization_id @initialization\nis_source_parameter INTEGER NOT NULL DEFAULT 0')
entity('variable_symbol', '''type_id @type NOT NULL
initialization_kind_id @kind NOT NULL
initializer_id @initialization
constant_initializer_id @constant
lifetime_id @lifetime
storage_duration_id @kind
is_thread_local INTEGER NOT NULL DEFAULT 0
is_inline INTEGER NOT NULL DEFAULT 0
is_parameter INTEGER NOT NULL DEFAULT 0
parameter_type_ordinal INTEGER
binding_container_id @variable_symbol
instance_id @template_instance''')
relation('variable_binding', 'owner_id @variable_symbol NOT NULL\nordinal INTEGER NOT NULL\nvariable_id @variable_symbol NOT NULL')
entity('field_symbol', '''record_type_id @type NOT NULL
type_id @type NOT NULL
default_initialization_id @initialization
is_mutable INTEGER NOT NULL DEFAULT 0
is_bit_field INTEGER NOT NULL DEFAULT 0
is_generated_base INTEGER NOT NULL DEFAULT 0
is_generated_vptr INTEGER NOT NULL DEFAULT 0''')
table('field_property', 'get_name_id @text_atom\nput_name_id @text_atom')
tables[-1] = tables[-1].replace('id INTEGER PRIMARY KEY', 'id INTEGER PRIMARY KEY REFERENCES field_symbol(id) DEFERRABLE INITIALLY DEFERRED', 1)
entity('label_symbol', 'statement_id @statement\nlifetime_id @lifetime\nis_break INTEGER NOT NULL DEFAULT 0\nis_continue INTEGER NOT NULL DEFAULT 0\nis_leave INTEGER NOT NULL DEFAULT 0')
entity('enum_symbol', 'underlying_type_id @type\nis_scoped INTEGER NOT NULL DEFAULT 0')
relation('enumerator', 'owner_id @enum_symbol NOT NULL\nordinal INTEGER NOT NULL\nsymbol_id @symbol NOT NULL\nconstant_id @constant NOT NULL')
entity('alias_symbol', 'target_id @entity\nunderlying_type_id @type')
entity('using_declaration', '''target_id @entity
lexical_scope_id @scope
source_origin_id @source_origin
is_directive INTEGER NOT NULL DEFAULT 0
is_class_member INTEGER NOT NULL DEFAULT 0
is_inheriting_constructor INTEGER NOT NULL DEFAULT 0
is_hidden INTEGER NOT NULL DEFAULT 0
compiler_generated INTEGER NOT NULL DEFAULT 0
is_inline_namespace INTEGER NOT NULL DEFAULT 0
is_strong INTEGER NOT NULL DEFAULT 0
is_pack_expansion INTEGER NOT NULL DEFAULT 0
is_representative INTEGER NOT NULL DEFAULT 0
is_using_enum INTEGER NOT NULL DEFAULT 0''')
entity('static_assertion', 'condition_id @constant NOT NULL\nmessage_id @constant\nsource_origin_id @source_origin')
entity('type', '''kind_id @kind NOT NULL
name_id @name
symbol_id @type_symbol
source_origin_id @source_origin
size TEXT NOT NULL
alignment TEXT NOT NULL
descriptor_size TEXT
descriptor_alignment TEXT
incomplete INTEGER NOT NULL
canonical_type_id @type
declared_type_id @type
is_dependent INTEGER NOT NULL DEFAULT 0
is_atomic INTEGER NOT NULL DEFAULT 0''')
entity('arithmetic_type', 'arithmetic_kind_id @kind NOT NULL\nedg_integer_kind_id @kind\nedg_float_kind_id @kind\nedg_fixed_precision_id @kind\nbits INTEGER\nsigned INTEGER\nprecision INTEGER\nradix INTEGER\nscale INTEGER\nis_bool INTEGER NOT NULL DEFAULT 0\nis_wchar INTEGER NOT NULL DEFAULT 0\nis_char8 INTEGER NOT NULL DEFAULT 0\nis_char16 INTEGER NOT NULL DEFAULT 0\nis_char32 INTEGER NOT NULL DEFAULT 0\nis_microsoft_sized_integer INTEGER NOT NULL DEFAULT 0')
entity('qualified_type', 'underlying_type_id @type NOT NULL\nis_const INTEGER NOT NULL DEFAULT 0\nis_volatile INTEGER NOT NULL DEFAULT 0\nis_restrict INTEGER NOT NULL DEFAULT 0\naddress_space INTEGER\nis_shared INTEGER NOT NULL DEFAULT 0\nshared_block_size TEXT')
entity('pointer_type', 'pointee_type_id @type NOT NULL\nis_reference INTEGER NOT NULL DEFAULT 0\nis_rvalue_reference INTEGER NOT NULL DEFAULT 0\nis_handle INTEGER NOT NULL DEFAULT 0\nis_tracking_reference INTEGER NOT NULL DEFAULT 0\nis_interior INTEGER NOT NULL DEFAULT 0\nis_pinned INTEGER NOT NULL DEFAULT 0\nis_ptr32 INTEGER NOT NULL DEFAULT 0\nis_ptr64 INTEGER NOT NULL DEFAULT 0\nis_sptr INTEGER NOT NULL DEFAULT 0\nis_uptr INTEGER NOT NULL DEFAULT 0\nbased_variable_id @variable_symbol')
entity('member_pointer_type', 'declaring_type_id @type NOT NULL\nmember_type_id @type NOT NULL')
entity('array_type', 'element_type_id @type NOT NULL\nbound_kind_id @kind NOT NULL\nbound TEXT\nbound_expression_id @expression\nvla_dimension_id @vla_dimension\nbound_is_zero INTEGER NOT NULL DEFAULT 0')
entity('routine_type', '''result_type_id @type NOT NULL
this_type_id @type
calling_convention_id @kind
language_linkage_id @kind
exception_specification_id @exception_specification
is_variadic INTEGER NOT NULL DEFAULT 0
is_const INTEGER NOT NULL DEFAULT 0
is_volatile INTEGER NOT NULL DEFAULT 0
ref_qualifier_id @kind
has_prototype INTEGER NOT NULL DEFAULT 1''')
entity('parameter_type', '''type_id @type NOT NULL
declared_type_id @type
name_id @text_atom
source_origin_id @source_origin
parameter_number INTEGER NOT NULL
default_expression_id @expression
has_name_conflict INTEGER NOT NULL DEFAULT 0
passed_via_copy_constructor INTEGER NOT NULL DEFAULT 0
has_default_argument INTEGER NOT NULL DEFAULT 0
default_argument_in_class INTEGER NOT NULL DEFAULT 0
has_unevaluated_template_default INTEGER NOT NULL DEFAULT 0
type_involves_deduced_template_parameter INTEGER NOT NULL DEFAULT 0
type_involves_template_parameter INTEGER NOT NULL DEFAULT 0
is_parameter_pack INTEGER NOT NULL DEFAULT 0
is_pack_element INTEGER NOT NULL DEFAULT 0
was_nontrailing_pack INTEGER NOT NULL DEFAULT 0
is_auto_parameter INTEGER NOT NULL DEFAULT 0
duplicate_name INTEGER NOT NULL DEFAULT 0
move_constructor_or_assignment_parameter INTEGER NOT NULL DEFAULT 0
copy_or_move_constructor_parameter INTEGER NOT NULL DEFAULT 0
is_requires_expression_parameter INTEGER NOT NULL DEFAULT 0
is_explicit_this INTEGER NOT NULL DEFAULT 0
is_transparent INTEGER NOT NULL DEFAULT 0
is_nonnull INTEGER NOT NULL DEFAULT 0
is_cli_parameter_array INTEGER NOT NULL DEFAULT 0
is_const INTEGER NOT NULL DEFAULT 0
is_volatile INTEGER NOT NULL DEFAULT 0
is_restrict INTEGER NOT NULL DEFAULT 0
is_atomic INTEGER NOT NULL DEFAULT 0
is_nullable INTEGER NOT NULL DEFAULT 0
is_nonnull_qualified INTEGER NOT NULL DEFAULT 0
is_null_unspecified INTEGER NOT NULL DEFAULT 0
is_unaligned INTEGER NOT NULL DEFAULT 0
is_near INTEGER NOT NULL DEFAULT 0
is_far INTEGER NOT NULL DEFAULT 0
is_upc_shared INTEGER NOT NULL DEFAULT 0
is_upc_strict INTEGER NOT NULL DEFAULT 0
is_upc_relaxed INTEGER NOT NULL DEFAULT 0
named_address_space INTEGER''')
relation('parameter_default_entity', 'owner_id @parameter_type NOT NULL\nordinal INTEGER NOT NULL\nentity_id @entity NOT NULL')
relation('routine_type_parameter', 'owner_id @routine_type NOT NULL\nordinal INTEGER NOT NULL\nparameter_type_id @parameter_type NOT NULL\nrole_id @kind NOT NULL\nbase_subobject_id @base_subobject')
entity('record_type', 'scope_id @scope\ndefinition_state_id @kind NOT NULL\ninstance_id @template_instance\nsubobject_partner_id @type\nis_lambda INTEGER NOT NULL DEFAULT 0\nis_empty INTEGER NOT NULL DEFAULT 0\nis_abstract INTEGER NOT NULL DEFAULT 0')
entity('type_reference', 'underlying_type_id @type NOT NULL\nreference_kind_id @kind NOT NULL\ntypedef_symbol_id @entity\ndecltype_expression_id @expression\ninstance_id @template_instance')
entity('vector_type', 'element_type_id @type NOT NULL\nelement_count TEXT\nvector_kind_id @kind NOT NULL\nscalable_count_type_id @type\nsize_constant_id @constant\nis_boolean INTEGER NOT NULL DEFAULT 0\ntuple_elements INTEGER\nlength_multiplier INTEGER')
entity('dependent_type', 'parameter_kind_id @kind\nparameter_number INTEGER\nparameter_depth INTEGER\nparent_type_id @type\nexpression_id @expression\nconstraint_id @expression\nbit_width_constant_id @constant')
entity('managed_type', 'managed_kind_id @kind NOT NULL\nunderlying_type_id @type\nrank INTEGER\nassembly_name_id @text_atom')
entity('record_layout', 'type_id @type NOT NULL\nnonvirtual_size TEXT\nnonvirtual_alignment TEXT\nis_complete INTEGER NOT NULL')
relation('field_layout', 'owner_id @record_layout NOT NULL\nordinal INTEGER NOT NULL\nfield_id @field_symbol NOT NULL\noffset TEXT NOT NULL\nbit_offset TEXT\nbit_width TEXT\ncontainer_type_id @type')
relation('direct_base', 'owner_id @type NOT NULL\nordinal INTEGER NOT NULL\nbase_type_id @type NOT NULL\naccess_id @kind\nis_virtual INTEGER NOT NULL DEFAULT 0\nsubobject_id @base_subobject')
entity('base_subobject', 'most_derived_type_id @type NOT NULL\nbase_type_id @type NOT NULL\noffset TEXT\nis_virtual INTEGER NOT NULL DEFAULT 0\nis_direct INTEGER NOT NULL DEFAULT 0\nparent_id @base_subobject\nbase_field_id @field_symbol')
entity('base_derivation', 'base_subobject_id @base_subobject NOT NULL\nordinal INTEGER NOT NULL\nis_direct INTEGER NOT NULL DEFAULT 0\nis_preferred INTEGER NOT NULL DEFAULT 0\naccess_id @kind\npath_tail_ordinal INTEGER NOT NULL')
relation('base_derivation_step', 'owner_id @base_derivation NOT NULL\nordinal INTEGER NOT NULL\nsubobject_id @base_subobject NOT NULL')
relation('virtual_member', 'owner_id @type NOT NULL\nordinal INTEGER NOT NULL\nroutine_id @routine_symbol NOT NULL\nis_inherited INTEGER NOT NULL DEFAULT 0\nis_pure INTEGER NOT NULL DEFAULT 0\nis_final INTEGER NOT NULL DEFAULT 0\nslot TEXT')
relation('override_relation', 'owner_id @routine_symbol NOT NULL\nordinal INTEGER NOT NULL\noverridden_id @routine_symbol NOT NULL\nsubobject_id @base_subobject\noverriding_subobject_id @base_subobject\nreturn_adjustment_subobject_id @base_subobject\nadjustment TEXT\nreturn_adjustment TEXT')
relation('lowered_type_relationship', 'owner_id @type NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\ntarget_id @type NOT NULL')
relation('abi_routine_relationship', 'owner_id @routine_symbol NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\ntarget_id @routine_symbol NOT NULL\nadjustment TEXT\nreturn_adjustment TEXT')
entity('virtual_table', 'variable_id @variable_symbol NOT NULL\nrecord_type_id @type\nkind_id @kind NOT NULL')
relation('virtual_table_entry', 'owner_id @virtual_table NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\nroutine_id @routine_symbol\nentity_id @entity\nadjustment TEXT\nconstant_id @constant')
entity('semantic_node', 'kind_id @kind NOT NULL\nsource_origin_id @source_origin\ncompiler_generated INTEGER NOT NULL DEFAULT 0\nsynthesis_kind_id @kind\nrepresentation_role_id @kind NOT NULL')
relation('structural_child', 'owner_id @semantic_node NOT NULL\nrole_id @kind NOT NULL\nordinal INTEGER NOT NULL\nchild_id @semantic_node NOT NULL', 'owner_id, role_id, ordinal')
relation('semantic_reference', 'owner_id @entity NOT NULL\nrole_id @kind NOT NULL\nordinal INTEGER NOT NULL\ntarget_id @entity NOT NULL', 'owner_id, role_id, ordinal')
entity('expression', '''type_id @type NOT NULL
value_category_id @kind NOT NULL
original_lvalue_type_id @type
result_unused INTEGER NOT NULL DEFAULT 0
initialization_guard INTEGER NOT NULL DEFAULT 0
generated_default_argument INTEGER NOT NULL DEFAULT 0
volatile_fetch INTEGER NOT NULL DEFAULT 0
is_pack_expansion INTEGER NOT NULL DEFAULT 0
is_parenthesized INTEGER NOT NULL DEFAULT 0
is_static_cast INTEGER NOT NULL DEFAULT 0
is_functional_cast INTEGER NOT NULL DEFAULT 0
is_brace_cast INTEGER NOT NULL DEFAULT 0
is_safe_cast INTEGER NOT NULL DEFAULT 0
is_cli_typeid INTEGER NOT NULL DEFAULT 0
is_cli_param_array_element INTEGER NOT NULL DEFAULT 0
is_objectless_member_reference INTEGER NOT NULL DEFAULT 0
is_gnu_extension INTEGER NOT NULL DEFAULT 0
type_definition_needed INTEGER NOT NULL DEFAULT 0
boolean_not_normalized INTEGER NOT NULL DEFAULT 0''')
entity('statement', 'kind_id @kind NOT NULL\nexpression_id @expression\nparent_id @statement\nlifetime_id @lifetime\nis_guard INTEGER NOT NULL DEFAULT 0\nis_declared_constexpr INTEGER NOT NULL DEFAULT 0\nlowering_generated INTEGER NOT NULL DEFAULT 0\nhas_associated_pragma INTEGER NOT NULL DEFAULT 0\nis_lowering_boilerplate INTEGER NOT NULL DEFAULT 0\nis_fallthrough_statement INTEGER NOT NULL DEFAULT 0\nis_likely INTEGER NOT NULL DEFAULT 0\nis_unlikely INTEGER NOT NULL DEFAULT 0')
relation('expression_operand', 'owner_id @expression NOT NULL\nrole_id @kind NOT NULL\nordinal INTEGER NOT NULL\nexpression_id @expression NOT NULL', 'owner_id, role_id, ordinal')
entity('operator_expression', '''operator_kind_id @kind NOT NULL
operation_type_kind_id @kind
eval_left_to_right INTEGER NOT NULL DEFAULT 0
eval_right_to_left INTEGER NOT NULL DEFAULT 0
pointer_operand_is_second INTEGER NOT NULL DEFAULT 0
is_reinterpret_like_cast INTEGER NOT NULL DEFAULT 0
requires_runtime_cast_check INTEGER NOT NULL DEFAULT 0
returns_lvalue INTEGER NOT NULL DEFAULT 0
is_reinterpret_cast INTEGER NOT NULL DEFAULT 0
is_reference_cast INTEGER NOT NULL DEFAULT 0
is_rvalue_reference_cast INTEGER NOT NULL DEFAULT 0
is_const_cast INTEGER NOT NULL DEFAULT 0
is_tracking_reference_cast INTEGER NOT NULL DEFAULT 0
is_gnu_two_operand_conditional INTEGER NOT NULL DEFAULT 0
call_uses_operator_syntax INTEGER NOT NULL DEFAULT 0
implicit_member_naming INTEGER NOT NULL DEFAULT 0
implicit_explicit_cast_step INTEGER NOT NULL DEFAULT 0
is_conversion_call INTEGER NOT NULL DEFAULT 0
argument_dependent_lookup_suppressed INTEGER NOT NULL DEFAULT 0
qualified_function_name INTEGER NOT NULL DEFAULT 0
retained_virtual_call INTEGER NOT NULL DEFAULT 0
rewritten_property_kind_id @kind''')
entity('call_expression', 'callee_id @expression NOT NULL\ntarget_routine_id @routine_symbol\nsource_target_routine_id @routine_symbol\nreceiver_id @expression\nfunction_type_id @type\ncall_kind_id @kind NOT NULL\nsource_dispatch_kind_id @kind\nreturn_slot_id @expression')
relation('call_argument', 'owner_id @call_expression NOT NULL\nordinal INTEGER NOT NULL\nexpression_id @expression NOT NULL\nparameter_id @variable_symbol\nrole_id @kind')
entity('reference_expression', 'target_id @entity\nreference_kind_id @kind NOT NULL\nname_reference_id @name_reference\nsubobject_id @base_subobject')
entity('name_reference', '''kind_id @kind
builtin_kind_id @kind
property_id @field_symbol
destructor_type_id @type
qualifier_id @name_qualifier
num_template_arguments TEXT
is_global_qualified_name INTEGER NOT NULL DEFAULT 0
is_template_id INTEGER NOT NULL DEFAULT 0
is_super_qualified INTEGER NOT NULL DEFAULT 0
is_decltype_qualified INTEGER NOT NULL DEFAULT 0
from_prototype_instantiation INTEGER NOT NULL DEFAULT 0
used_in_primary_declarator INTEGER NOT NULL DEFAULT 0''')
entity('name_qualifier', 'previous_qualifier_id @name_qualifier\ntarget_id @entity\nname_id @text_atom\nis_class INTEGER NOT NULL DEFAULT 0')
relation('original_template_argument', 'owner_id @name_reference NOT NULL\nordinal INTEGER NOT NULL\nargument_id @template_argument NOT NULL')
entity('constant_expression', 'constant_id @constant NOT NULL\nname_reference_id @name_reference')
entity('conversion_expression', 'operand_id @expression NOT NULL\nsource_type_id @type\ntarget_type_id @type NOT NULL\nkind_id @kind NOT NULL\nroutine_id @routine_symbol\nbase_path_id @entity')
entity('block_descriptor', 'scope_id @scope\nlifetime_id @lifetime\nfinal_position_id @source_position\nend_reachable INTEGER NOT NULL DEFAULT 0\nis_statement_expression INTEGER NOT NULL DEFAULT 0\nimplicit_scope_not_allowed INTEGER NOT NULL DEFAULT 0\nupc_access_method_id @kind')
entity('block_statement', 'descriptor_id @block_descriptor')
entity('declaration_statement', 'has_static_or_thread_variable INTEGER NOT NULL DEFAULT 0')
relation('declaration_statement_entity', 'owner_id @declaration_statement NOT NULL\nordinal INTEGER NOT NULL\nentity_id @entity NOT NULL')
entity('initialization_statement', 'initialization_id @initialization NOT NULL')
relation('block_item', 'owner_id @block_statement NOT NULL\nordinal INTEGER NOT NULL\nstatement_id @statement NOT NULL')
entity('if_statement', 'then_id @statement\nelse_id @statement\ncondition_id @expression\ninitialization_id @statement\nselection_known INTEGER\nselected_then INTEGER')
entity('loop_statement', 'body_id @statement\ninitialization_id @statement\ncondition_id @expression\nincrement_id @expression\ninit_scope_id @scope\ntest_position_id @source_position\naffinity_id @expression')
entity('range_for', 'body_id @statement\nrange_variable_id @variable_symbol\nbegin_variable_id @variable_symbol\nend_variable_id @variable_symbol\niterator_variable_id @variable_symbol\ncomparison_id @expression\nincrement_id @expression\ninitialization_id @statement\nscope_id @scope\niterator_scope_id @scope\nuse_await INTEGER NOT NULL DEFAULT 0')
entity('for_each_statement', 'body_id @statement\ncollection_id @expression\npattern_kind_id @kind NOT NULL\niterator_id @variable_symbol\ninitialization_id @expression\ntest_id @expression\nincrement_id @expression\nuses_previous_declaration_iterator INTEGER NOT NULL DEFAULT 0\nprevious_field_id @field_symbol\nassignment_id @expression\ncollection_reference_id @variable_symbol\ntemporary_variable_id @variable_symbol\nend_variable_id @variable_symbol\nscope_id @scope\niterator_scope_id @scope')
entity('switch_statement', 'body_id @statement\nselector_id @expression\ndefault_label_id @label_symbol')
relation('switch_case', 'owner_id @switch_statement NOT NULL\nordinal INTEGER NOT NULL\nstatement_id @statement\nlabel_id @label_symbol\nlow_constant_id @constant\nhigh_constant_id @constant\nis_default INTEGER NOT NULL DEFAULT 0')
entity('jump_statement', 'kind_id @kind NOT NULL\nvalue_id @expression\nlabel_id @label_symbol\ninitialization_id @initialization\nsource_lifetime_id @lifetime\ncommon_lifetime_id @lifetime')
entity('label_statement', 'label_id @label_symbol NOT NULL\nstatement_id @statement')
entity('try_descriptor', 'body_id @statement NOT NULL\nis_function_try INTEGER NOT NULL DEFAULT 0\nlifetime_id @lifetime\nfinally_statement_id @statement')
entity('try_statement', 'descriptor_id @try_descriptor NOT NULL')
relation('try_handler', 'owner_id @try_descriptor NOT NULL\nordinal INTEGER NOT NULL\nhandler_id @handler NOT NULL')
entity('handler', 'body_id @statement NOT NULL\ncaught_type_id @type\nparameter_id @variable_symbol\nparameter_initialization_id @initialization\ntypeinfo_id @variable_symbol\nsource_position_id @source_position')
entity('microsoft_try_statement', 'body_id @statement NOT NULL\nfilter_id @expression\nhandler_id @statement\nfinally_id @statement\nexcept_or_finally_position_id @source_position\nis_finally INTEGER NOT NULL DEFAULT 0')
entity('throw_expression', 'operand_id @expression\nthrown_type_id @type\ninitialization_id @initialization\ndestructor_id @routine_symbol\nis_rethrow INTEGER NOT NULL DEFAULT 0')
relation('throw_accessible_base', 'owner_id @throw_expression NOT NULL\nordinal INTEGER NOT NULL\nsubobject_id @base_subobject NOT NULL')
entity('type_query_expression', 'query_kind_id @kind NOT NULL\noperand_type_id @type\noperand_expression_id @expression\nis_dynamic INTEGER NOT NULL DEFAULT 0\nis_standard_alignof INTEGER NOT NULL DEFAULT 0\nvla_dimension_id @vla_dimension')
entity('temporary_expression', 'initialization_id @initialization\ntemporary_id @temporary\nsource_type_id @type')
entity('condition_expression', 'expression_id @expression\ninitialization_id @statement\ndynamic_initialization_id @initialization\nscope_id @scope\nvariable_id @variable_symbol')
entity('lifetime_expression', 'expression_id @expression NOT NULL\nlifetime_id @lifetime')
entity('statement_expression', 'statement_id @statement NOT NULL\nresult_id @expression')
entity('reuse_expression', 'initialization_id @initialization NOT NULL')
entity('builtin_expression', 'kind_id @kind NOT NULL\nselected_expression_id @expression\nresult_constant_id @constant')
entity('parameter_reference_expression', 'parameter_number INTEGER NOT NULL\nparameter_depth INTEGER NOT NULL\nis_this INTEGER NOT NULL DEFAULT 0')
entity('dependent_expression', 'kind_id @kind NOT NULL\nwrapped_expression_id @expression\ntemplate_id @template_definition\ninitialization_id @initialization\nparameter_scope_id @scope\nnoexcept_required INTEGER\nis_dependent INTEGER NOT NULL DEFAULT 1\npack_index_id @expression\npack_type_id @type\npack_template_id @template_definition\nfold_operator_id @kind\nfold_left_associative INTEGER\ntoken_sequence_id @token_sequence\nreattempt_default_argument INTEGER\nreattempt_default_member_initializer INTEGER')
relation('requirement_parameter', 'owner_id @dependent_expression NOT NULL\nordinal INTEGER NOT NULL\nparameter_type_id @parameter_type NOT NULL')
relation('expression_template_argument', 'owner_id @expression NOT NULL\nordinal INTEGER NOT NULL\nargument_id @template_argument NOT NULL')
entity('token_sequence', '')
relation('token_sequence_token', 'owner_id @token_sequence NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\nspelling_id @text_atom\nsource_position_id @source_position')
relation('dependent_requirement', 'owner_id @dependent_expression NOT NULL\nordinal INTEGER NOT NULL\nexpression_id @expression NOT NULL')
entity('allocation_expression', '''allocated_type_id @type
allocator_id @routine_symbol
deallocator_id @routine_symbol
constructor_id @routine_symbol
destructor_id @routine_symbol
initialization_id @initialization
array_count_id @expression
size_expression_id @expression
operand_id @expression
is_delete INTEGER NOT NULL DEFAULT 0
is_array INTEGER NOT NULL DEFAULT 0
is_global INTEGER NOT NULL DEFAULT 0
is_gcnew INTEGER NOT NULL DEFAULT 0
is_placement INTEGER NOT NULL DEFAULT 0
is_aligned INTEGER NOT NULL DEFAULT 0
has_initializer INTEGER NOT NULL DEFAULT 0
brace_initializer INTEGER NOT NULL DEFAULT 0
paren_aggregate_initializer INTEGER NOT NULL DEFAULT 0
deducible_type INTEGER NOT NULL DEFAULT 0
parenthesized_type INTEGER NOT NULL DEFAULT 0
exception_freeing_id @initialization''')
relation('allocation_argument', 'owner_id @allocation_expression NOT NULL\nordinal INTEGER NOT NULL\nexpression_id @expression NOT NULL')
entity('lambda_definition', '''closure_type_id @type NOT NULL
call_operator_id @routine_symbol
source_origin_id @source_origin
capture_end_position_id @source_position
mutable_position_id @source_position
constexpr_position_id @source_position
is_generic INTEGER NOT NULL DEFAULT 0
is_mutable INTEGER NOT NULL DEFAULT 0
constexpr_specified INTEGER NOT NULL DEFAULT 0
consteval_specified INTEGER NOT NULL DEFAULT 0
has_capture_default INTEGER NOT NULL DEFAULT 0
default_is_reference INTEGER NOT NULL DEFAULT 0
explicit_return_type INTEGER NOT NULL DEFAULT 0
has_parameter_declaration INTEGER NOT NULL DEFAULT 0
has_template_parameter_list INTEGER NOT NULL DEFAULT 0''')
entity('lambda_expression', 'lambda_id @lambda_definition NOT NULL\ninitialization_id @initialization')
relation('lambda_capture', '''owner_id @lambda_definition NOT NULL
ordinal INTEGER NOT NULL
kind_id @kind NOT NULL
variable_id @variable_symbol
field_id @field_symbol
initialization_id @initialization
source_closure_field_id @field_symbol
indirect_capture_field_id @field_symbol
source_origin_id @source_origin
is_pack INTEGER NOT NULL DEFAULT 0
is_init_capture INTEGER NOT NULL DEFAULT 0
is_indirect_init_capture INTEGER NOT NULL DEFAULT 0
is_parameter_reference INTEGER NOT NULL DEFAULT 0
capture_by_reference INTEGER NOT NULL DEFAULT 0
is_implicit INTEGER NOT NULL DEFAULT 0
is_pack_element INTEGER NOT NULL DEFAULT 0
direct_initialization INTEGER NOT NULL DEFAULT 0
parenthesized_initialization INTEGER NOT NULL DEFAULT 0
const_capture INTEGER NOT NULL DEFAULT 0''')
entity('coroutine', '''routine_id @routine_symbol NOT NULL
body_id @statement
promise_id @variable_symbol
handle_id @variable_symbol
allocation_id @expression
deallocation_id @expression
initial_suspend_id @expression
final_suspend_id @expression
return_object_id @expression
exception_id @expression
traits_type_id @type
initial_await_resume_id @variable_symbol
this_parameter_copy_id @variable_symbol
final_suspend_label_id @label_symbol
allocation_failure_call_id @expression
allocator_id @routine_symbol
deallocator_id @routine_symbol
source_position_id @source_position
error_descriptor INTEGER NOT NULL DEFAULT 0
has_return_void INTEGER NOT NULL DEFAULT 0
body_generated INTEGER NOT NULL DEFAULT 0''')
relation('coroutine_parameter_copy', 'owner_id @coroutine NOT NULL\nordinal INTEGER NOT NULL\nparameter_id @variable_symbol\ncopy_id @variable_symbol NOT NULL\ninitialization_id @initialization')
entity('coroutine_operation', 'kind_id @kind NOT NULL\noperand_id @expression\nready_id @expression\nsuspend_id @expression\nresume_id @expression\nprotocol_call_id @expression')
entity('vla_dimension', 'type_id @type\nexpression_id @expression\noriginal_dimension_id @vla_dimension\ndimension_variable_id @variable_symbol\ncount_variable_id @variable_symbol\nevaluation_statement_id @statement\nin_prototype_scope INTEGER NOT NULL DEFAULT 0\nhas_size_statement INTEGER NOT NULL DEFAULT 0\nsource_position_id @source_position')
entity('vla_operation', 'variable_id @variable_symbol\ntype_id @type\ndimension_id @vla_dimension\nis_typedef_declaration INTEGER NOT NULL DEFAULT 0')
entity('asm_entry', 'asm_string_id @constant NOT NULL\nsource_origin_id @source_origin\ngnu_asm_form INTEGER NOT NULL DEFAULT 0\nis_volatile INTEGER NOT NULL DEFAULT 0\nhas_volatile_keyword INTEGER NOT NULL DEFAULT 0\nhas_inline_keyword INTEGER NOT NULL DEFAULT 0\nis_goto INTEGER NOT NULL DEFAULT 0\nnumber_of_constraints TEXT')
entity('asm_statement', 'entry_id @asm_entry\ntext_id @text_atom')
relation('asm_operand', 'owner_id @asm_entry NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\nname_id @text_atom\nconstraint_id @text_atom\nmodifier_kind_id @kind\nexpression_id @expression NOT NULL\nsource_position_id @source_position')
relation('asm_operand_constraint', 'owner_id @asm_entry NOT NULL\noperand_ordinal INTEGER NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\ncondition_code_id @text_atom', 'owner_id, operand_ordinal, ordinal')
relation('asm_clobber', 'owner_id @asm_entry NOT NULL\nordinal INTEGER NOT NULL\ntext_id @text_atom\nregister_kind_id @kind')
relation('asm_label', 'owner_id @asm_entry NOT NULL\nordinal INTEGER NOT NULL\nlabel_id @label_symbol NOT NULL')
relation('owner_attribute', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\nattribute_id @attribute NOT NULL')
entity('attribute', '''kind_id @kind NOT NULL
family_id @kind
location_id @kind
name_id @text_atom
namespace_name_id @text_atom
group_id @attribute_group
source_origin_id @source_origin
on_primary_declaration INTEGER NOT NULL DEFAULT 0
transforms_type_specifier INTEGER NOT NULL DEFAULT 0
is_pack_expansion INTEGER NOT NULL DEFAULT 0
is_standard_gcc INTEGER NOT NULL DEFAULT 0
is_implicit_abi_tag INTEGER NOT NULL DEFAULT 0
namespace_from_using INTEGER NOT NULL DEFAULT 0
string_argument_id @text_atom
integer_argument TEXT
type_argument_id @type
expression_argument_id @expression
entity_argument_id @entity''')
entity('attribute_group', 'source_origin_id @source_origin\nnamespace_name_id @text_atom')
relation('attribute_argument_link', 'owner_id @attribute NOT NULL\nordinal INTEGER NOT NULL\nargument_id @attribute_argument NOT NULL')
entity('attribute_argument', '''kind_id @kind NOT NULL
text_id @text_atom
integer_value TEXT
type_id @type
expression_id @expression
entity_id @entity
constant_id @constant
token_text_id @text_atom
token_kind_id @kind
is_pack_expansion INTEGER NOT NULL DEFAULT 0
source_origin_id @source_origin''')
entity('constant', '''type_id @type
name_id @name
containing_entity_id @entity
orig_type_id @type
value_id @constant_value NOT NULL
source_origin_id @source_origin
backing_expression_id @expression
name_reference_id @name_reference
character_kind_id @kind
is_pack_expansion INTEGER NOT NULL DEFAULT 0
is_literal INTEGER NOT NULL DEFAULT 0
implicit_cast INTEGER NOT NULL DEFAULT 0
explicit_cast_applied INTEGER NOT NULL DEFAULT 0
is_reinterpret_cast INTEGER NOT NULL DEFAULT 0
is_reinterpret_like_cast INTEGER NOT NULL DEFAULT 0
is_simple_zero INTEGER NOT NULL DEFAULT 0''')
entity('constant_value', 'kind_id @kind NOT NULL')
relation('constant_component', 'owner_id @constant NOT NULL\nrole_id @kind NOT NULL\nordinal INTEGER NOT NULL\nconstant_id @constant NOT NULL', 'owner_id, role_id, ordinal')
value_table('integer_value', 'value_id @text_atom NOT NULL')
value_table('floating_value', 'class_id @kind NOT NULL\nnegative INTEGER NOT NULL\nradix INTEGER NOT NULL\nsignificand_id @text_atom\nexponent INTEGER\nnan_payload_id @text_atom\nnan_quiet INTEGER\nrepresentation_kind_id @kind')
value_table('fixed_point_value', 'scaled_integer_id @text_atom NOT NULL\nscale INTEGER NOT NULL')
value_table('complex_value', 'real_value_id @constant_value\nimaginary_value_id @constant_value NOT NULL')
value_table('string_value', 'element_bits INTEGER NOT NULL\nlength TEXT NOT NULL\nencoding_id @kind NOT NULL\nunit_kind_id @kind NOT NULL\ntext_id @text_atom')
relation('string_unit', 'owner_id @string_value NOT NULL\nordinal INTEGER NOT NULL\nunit INTEGER NOT NULL')
value_table('address_value', 'base_kind_id @kind NOT NULL\nbase_entity_id @entity\noffset TEXT NOT NULL\none_past_end INTEGER NOT NULL DEFAULT 0\nsubobject_path_id @entity\nis_object_reflection INTEGER NOT NULL DEFAULT 0\nparameter_number INTEGER')
value_table('stack_offset_value', 'variable_id @variable_symbol NOT NULL\noffset TEXT NOT NULL')
value_table('member_pointer_value', 'member_id @entity\ncontaining_type_id @type\nadjustment TEXT\nsubobject_id @base_subobject\nis_function_ptr INTEGER NOT NULL DEFAULT 0\ncast_to_base INTEGER NOT NULL DEFAULT 0\nis_null INTEGER NOT NULL DEFAULT 0')
value_table('label_difference_value', 'left_label_id @label_symbol\nright_label_id @label_symbol\nfrom_value_id @constant_value NOT NULL\nfrom_type_id @type\nto_value_id @constant_value NOT NULL\nto_type_id @type\noffset TEXT')
value_table('aggregate_value', 'type_id @type NOT NULL')
relation('aggregate_element', 'owner_id @aggregate_value NOT NULL\nordinal INTEGER NOT NULL\nvalue_id @constant_value NOT NULL\ntype_id @type\nfield_id @field_symbol\ninitialization_id @initialization')
value_table('repeat_value', 'value_id @constant_value NOT NULL\ntype_id @type\ncount TEXT NOT NULL\nmultidimensional_tail_not_repeated INTEGER NOT NULL DEFAULT 0')
value_table('designator_value', 'kind_id @kind NOT NULL\nfield_id @field_symbol\nfield_name_id @text_atom\nis_generic INTEGER NOT NULL DEFAULT 0\nuses_direct_init_syntax INTEGER NOT NULL DEFAULT 0\nindex_value TEXT\nrange_end TEXT\nvalue_id @constant_value\ntype_id @type')
relation('designator_subscript', 'owner_id @designator_value NOT NULL\nordinal INTEGER NOT NULL\nvalue_id @constant_value NOT NULL\ntype_id @type')
value_table('dynamic_initialization_value', 'initialization_id @initialization NOT NULL')
value_table('dependent_value', '''kind_id @kind NOT NULL
parameter_number INTEGER
parameter_depth INTEGER
expression_id @expression
type_id @type
entity_id @entity
constant_value_id @constant_value
constant_type_id @type
is_qualified_name INTEGER NOT NULL DEFAULT 0
has_address_of INTEGER NOT NULL DEFAULT 0
is_pack INTEGER NOT NULL DEFAULT 0
has_generic_cast INTEGER NOT NULL DEFAULT 0
conversion_type_id @type
opname_kind_id @kind
special_kind_id @kind
is_standard_alignof INTEGER NOT NULL DEFAULT 0
destructor_unqualified INTEGER NOT NULL DEFAULT 0''')
relation('dependent_constant_element', 'owner_id @dependent_value NOT NULL\nordinal INTEGER NOT NULL\nvalue_id @constant_value NOT NULL\ntype_id @type')
relation('dependent_template_argument', 'owner_id @dependent_value NOT NULL\nordinal INTEGER NOT NULL\nargument_id @template_argument NOT NULL')
value_table('reflection_value', 'kind_id @kind NOT NULL\nentity_id @entity\nowner_routine_id @routine_symbol\nlocal_scope_number TEXT')
entity('subobject_path', 'kind_id @kind NOT NULL\nfield_id @field_symbol\nbase_subobject_id @base_subobject\nelement_offset TEXT\nnext_id @subobject_path\nis_converted INTEGER NOT NULL DEFAULT 0')
entity('initialization', '''kind_id @kind NOT NULL
target_id @entity
type_id @type
lambda_id @lambda_definition
expression_id @expression
constant_id @constant
constructor_id @routine_symbol
destructor_id @routine_symbol
lifetime_id @lifetime
init_expression_lifetime_id @lifetime
master_id @initialization
next_destruction_id @initialization
overlapping_lifetime_id @lifetime
source_id @expression
condition_variable_id @variable_symbol
is_unordered INTEGER NOT NULL DEFAULT 0
is_conditional INTEGER NOT NULL DEFAULT 0
is_overlapping INTEGER NOT NULL DEFAULT 0
is_array INTEGER NOT NULL DEFAULT 0
is_elided INTEGER NOT NULL DEFAULT 0
is_class_result_master INTEGER NOT NULL DEFAULT 0
is_static_temporary INTEGER NOT NULL DEFAULT 0
is_partial_aggregate INTEGER NOT NULL DEFAULT 0
is_partial_initialization INTEGER NOT NULL DEFAULT 0
follows_executable_statement INTEGER NOT NULL DEFAULT 0
has_temporary_lifetime INTEGER NOT NULL DEFAULT 0
is_constructor_initialization INTEGER NOT NULL DEFAULT 0
is_conditional_guard INTEGER NOT NULL DEFAULT 0
is_vla_deallocation INTEGER NOT NULL DEFAULT 0
creates_initializer_list INTEGER NOT NULL DEFAULT 0
is_initializer_list_array INTEGER NOT NULL DEFAULT 0
is_reused_value INTEGER NOT NULL DEFAULT 0
is_question_result INTEGER NOT NULL DEFAULT 0
is_comma_result INTEGER NOT NULL DEFAULT 0
constructor_copy_implied_source INTEGER NOT NULL DEFAULT 0
constructor_implicit_copy INTEGER NOT NULL DEFAULT 0
constructor_value_initialization INTEGER NOT NULL DEFAULT 0
constructor_sequenced_arguments INTEGER NOT NULL DEFAULT 0
constructor_array_copy INTEGER NOT NULL DEFAULT 0
constant_nonconstant INTEGER NOT NULL DEFAULT 0
frees_storage INTEGER NOT NULL DEFAULT 0
frees_array INTEGER NOT NULL DEFAULT 0
is_explicit_cast INTEGER NOT NULL DEFAULT 0
is_braced INTEGER NOT NULL DEFAULT 0
is_compound_literal INTEGER NOT NULL DEFAULT 0
guard_variable_id @variable_symbol
vla_dimension_id @vla_dimension''')
relation('initialization_operand', 'owner_id @initialization NOT NULL\nrole_id @kind NOT NULL\nordinal INTEGER NOT NULL\ninitialization_id @initialization\nexpression_id @expression\nfield_id @field_symbol\nbase_subobject_id @base_subobject', 'owner_id, role_id, ordinal')
entity('temporary', 'type_id @type NOT NULL\nvariable_id @variable_symbol\ninitialization_id @initialization\nlifetime_id @lifetime\nextended_lifetime_id @lifetime')
entity('lifetime', 'kind_id @kind NOT NULL\nparent_id @lifetime\nbound_entity_id @entity\nparent_checkpoint_id @initialization\nhas_implicit_child INTEGER NOT NULL DEFAULT 0\nhas_block_after_label_child INTEGER NOT NULL DEFAULT 0')
relation('lifetime_child', 'owner_id @lifetime NOT NULL\nordinal INTEGER NOT NULL\nchild_id @lifetime NOT NULL')
relation('lifetime_cleanup', 'owner_id @lifetime NOT NULL\nordinal INTEGER NOT NULL\ninitialization_id @initialization NOT NULL')
relation('lifetime_transition', 'owner_id @semantic_node NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\nsource_id @lifetime\ntarget_id @lifetime')
relation('construction_dependency', 'owner_id @initialization NOT NULL\nordinal INTEGER NOT NULL\ndependency_id @initialization NOT NULL\ncondition_id @variable_symbol')
relation('static_initialization', 'owner_id @translation_unit NOT NULL\nordinal INTEGER NOT NULL\nvariable_id @variable_symbol NOT NULL\ninitialization_kind_id @kind NOT NULL\ninitialization_id @initialization\nconstant_id @constant\ninit_routine_id @routine_symbol\ntermination_routine_id @routine_symbol\nis_thread_local INTEGER NOT NULL DEFAULT 0')
entity('exception_specification', 'kind_id @kind NOT NULL\nnoexcept_argument_id @constant\nnoexcept_expression_id @expression\ncopy_from_prototype_routine_id @routine_symbol\nsource_origin_id @source_origin\nargument_cached INTEGER NOT NULL DEFAULT 0\nfrom_attribute INTEGER NOT NULL DEFAULT 0\ncompiler_generated INTEGER NOT NULL DEFAULT 0\nthrow_any INTEGER NOT NULL DEFAULT 0\nnoexcept_effective INTEGER\nis_dependent INTEGER NOT NULL DEFAULT 0\nis_indeterminate INTEGER NOT NULL DEFAULT 0')
relation('exception_specification_type', 'owner_id @exception_specification NOT NULL\nordinal INTEGER NOT NULL\ntype_id @type NOT NULL\nis_redundant INTEGER NOT NULL DEFAULT 0\nis_pack_expansion INTEGER NOT NULL DEFAULT 0\nsource_position_id @source_position')
entity('function_eh', 'routine_id @routine_symbol NOT NULL\nprologue_id @expression\nepilogue_id @expression\nregion_table_id @variable_symbol\narray_table_id @variable_symbol\ninitial_state TEXT NOT NULL\nnull_state TEXT NOT NULL')
relation('eh_table_projection', 'owner_id @variable_symbol NOT NULL\nkind_id @kind NOT NULL\nfunction_eh_id @function_eh NOT NULL', 'owner_id')
entity('cleanup_region', 'function_eh_id @function_eh NOT NULL\nordinal INTEGER NOT NULL\nnext_region_id @cleanup_region\ncondition_id @cleanup_condition\nis_supplement INTEGER NOT NULL DEFAULT 0\nsupplement_owner_id @cleanup_region')
entity('cleanup_action', '''region_id @cleanup_region NOT NULL
ordinal INTEGER NOT NULL
kind_id @kind NOT NULL
routine_id @routine_symbol
storage_id @cleanup_storage_location
array_id @cleanup_array
abi_detail_id @cleanup_abi_detail
is_array INTEGER NOT NULL DEFAULT 0
is_indirect INTEGER NOT NULL DEFAULT 0
is_conditional INTEGER NOT NULL DEFAULT 0
is_unordered INTEGER
is_base_subobject INTEGER NOT NULL DEFAULT 0
is_vla INTEGER NOT NULL DEFAULT 0
is_deallocation INTEGER NOT NULL DEFAULT 0
is_local_static_guard INTEGER NOT NULL DEFAULT 0''')
entity('cleanup_storage_location', 'kind_id @kind NOT NULL\nvariable_id @variable_symbol\nbase_entity_id @entity\noffset TEXT NOT NULL')
entity('cleanup_array', 'function_eh_id @function_eh NOT NULL\nordinal INTEGER NOT NULL\nhandle_constant_id @constant NOT NULL\nelement_size TEXT NOT NULL\nelement_count TEXT NOT NULL\nis_nonarray_size INTEGER NOT NULL DEFAULT 0')
entity('cleanup_condition', 'storage_id @cleanup_storage_location NOT NULL\nkind_id @kind NOT NULL\ncomparison_value TEXT')
entity('cleanup_abi_detail', 'vtt_location_id @cleanup_storage_location\nis_delegating INTEGER\nvla_count_location_id @cleanup_storage_location')
entity('lowered_eh_operation', 'kind_id @kind NOT NULL\nfunction_eh_id @function_eh\nregion_id @cleanup_region\nstate_number TEXT\nroutine_id @routine_symbol\nobject_type_id @type\nhandler_id @handler\ntry_block_id @try_descriptor\ntry_expression_id @expression\ncatch_expression_id @expression\ninitialization_id @initialization')
entity('cleanup_state_operation', 'region_id @cleanup_region\nstate_number TEXT NOT NULL\nis_unreachable INTEGER NOT NULL DEFAULT 0')
relation('eh_handler_region', 'owner_id @handler NOT NULL\nordinal INTEGER NOT NULL\nregion_id @cleanup_region NOT NULL\nend_region_id @cleanup_region')
entity('template_definition', '''kind_id @kind NOT NULL
pattern_id @entity
constraint_id @expression
canonical_template_id @template_definition
definition_template_id @template_definition
prototype_template_id @template_definition
is_exported INTEGER NOT NULL DEFAULT 0
ignore_export INTEGER NOT NULL DEFAULT 0
is_pack INTEGER NOT NULL DEFAULT 0
is_friend_template INTEGER NOT NULL DEFAULT 0
parameter_number INTEGER
parameter_depth INTEGER
definition_text_id @text_atom
export_position_id @source_position
definition_range_id @source_range''')
entity('template_header', 'template_id @template_definition\nparent_id @template_header\nscope_id @scope\nsource_origin_id @source_origin\nis_generic INTEGER NOT NULL DEFAULT 0\nconstraint_id @expression\nrequires_position_id @source_position')
entity('template_parameter', 'template_id @template_definition\nheader_id @template_header\nordinal INTEGER NOT NULL\ndepth INTEGER NOT NULL\nkind_id @kind NOT NULL\nis_pack INTEGER NOT NULL DEFAULT 0\nis_abbreviated INTEGER NOT NULL DEFAULT 0\nplaceholder_type_id @type\nplaceholder_constant_id @constant\nplaceholder_template_id @template_definition\ndefault_argument_id @template_argument\nconstraint_id @expression')
entity('template_generic_constraint_clause', 'header_id @template_header NOT NULL\nordinal INTEGER NOT NULL\nparameter_type_id @type NOT NULL\ntype_position_id @source_position\nwhere_position_id @source_position\ncolon_position_id @source_position')
relation('template_generic_constraint', 'owner_id @template_generic_constraint_clause NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\ntype_id @type\nis_implicit INTEGER NOT NULL DEFAULT 0\nsource_position_id @source_position')
entity('template_argument', '''kind_id @kind NOT NULL
type_id @type
constant_id @constant
expression_id @expression
template_id @template_definition
is_pack INTEGER NOT NULL DEFAULT 0
is_explicit INTEGER NOT NULL DEFAULT 0
is_pack_element INTEGER NOT NULL DEFAULT 0
is_ellipsis INTEGER NOT NULL DEFAULT 0
is_integer_pack INTEGER NOT NULL DEFAULT 0
is_injected_name INTEGER NOT NULL DEFAULT 0
is_provisional INTEGER NOT NULL DEFAULT 0
is_error INTEGER NOT NULL DEFAULT 0
is_unknown_array_bound INTEGER NOT NULL DEFAULT 0
deduced_array_bound TEXT''')
relation('template_argument_pack_element', 'owner_id @template_argument NOT NULL\nordinal INTEGER NOT NULL\nargument_id @template_argument NOT NULL')
entity('template_instance', 'result_id @entity NOT NULL\ntemplate_id @template_definition\nselected_specialization_id @template_specialization\nstate_id @kind NOT NULL\nsource_position_id @source_position\nis_dependent INTEGER NOT NULL DEFAULT 0\nis_prototype INTEGER NOT NULL DEFAULT 0\nis_suppressed INTEGER NOT NULL DEFAULT 0\nis_explicit INTEGER NOT NULL DEFAULT 0\nis_specialized INTEGER NOT NULL DEFAULT 0')
relation('template_instance_argument', 'owner_id @template_instance NOT NULL\nrole_id @kind NOT NULL\nordinal INTEGER NOT NULL\nargument_id @template_argument NOT NULL', 'owner_id, role_id, ordinal')
entity('template_specialization', 'primary_template_id @template_definition NOT NULL\nentity_id @entity NOT NULL\nkind_id @kind NOT NULL\nconstraint_id @expression')
relation('constraint_metadata', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\nexpression_id @expression\nresult INTEGER\nis_dependent INTEGER NOT NULL DEFAULT 0')
relation('synthesized_origin', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\ncausing_entity_id @entity NOT NULL')
entity('source_file', 'path_id @text_atom NOT NULL\norigin_kind_id @kind NOT NULL\nverified_digest_id @text_atom')
entity('include_occurrence', '''file_id @source_file NOT NULL
parent_id @include_occurrence
position_id @source_position
ordinal INTEGER NOT NULL
display_name_id @text_atom
name_as_written_id @text_atom
first_sequence TEXT
last_sequence TEXT
first_line TEXT
module_id @module
is_system INTEGER NOT NULL DEFAULT 0
uses_angle_brackets INTEGER NOT NULL DEFAULT 0
is_include_file INTEGER NOT NULL DEFAULT 0
is_preinclude INTEGER NOT NULL DEFAULT 0
is_macros_only INTEGER NOT NULL DEFAULT 0
is_top_level INTEGER NOT NULL DEFAULT 0
is_top_level_from_pch INTEGER NOT NULL DEFAULT 0
is_implicit_include INTEGER NOT NULL DEFAULT 0''')
entity('line_directive', '''presumed_name_id @text_atom NOT NULL
first_sequence TEXT NOT NULL
last_sequence TEXT NOT NULL
first_line TEXT NOT NULL
include_id @include_occurrence
parent_id @line_directive''')
relation('source_descriptor_child', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\nchild_id @entity NOT NULL')
entity('source_mapping_segment', 'first_sequence TEXT NOT NULL\nlast_sequence TEXT NOT NULL\nfile_id @source_file NOT NULL\ninclude_id @include_occurrence\nline_directive_id @line_directive\nlogical_line TEXT NOT NULL\nphysical_line TEXT\npresumed_name_id @text_atom\nends_at_end_of_source INTEGER NOT NULL DEFAULT 0')
entity('source_position', 'sequence TEXT NOT NULL\ncolumn INTEGER NOT NULL\noriginal_sequence TEXT\noriginal_column INTEGER\nmacro_invocation_id @macro_invocation\nfile_id @source_file\npresumed_name_id @text_atom\nlogical_line TEXT\nphysical_line TEXT\ninclude_id @include_occurrence\nmapping_segment_id @source_mapping_segment\nspelling_position_id @source_position\nat_end_of_source INTEGER NOT NULL DEFAULT 0\norigin_kind_id @kind NOT NULL')
entity('source_range', 'start_id @source_position NOT NULL\nend_id @source_position NOT NULL\nendpoint_kind_id @kind NOT NULL')
entity('source_origin', 'position_id @source_position\nrange_id @source_range\nspelling_range_id @source_range\nexpansion_range_id @source_range\nkind_id @kind NOT NULL')
entity('macro_definition', 'name_id @text_atom NOT NULL\ntext_id @text_atom NOT NULL\nposition_id @source_position\nreplacement_range_id @source_range\nkind_id @kind NOT NULL\nis_function_like INTEGER NOT NULL DEFAULT 0\nis_predefined INTEGER NOT NULL DEFAULT 0\nis_undef INTEGER NOT NULL DEFAULT 0')
entity('macro_invocation', 'definition_id @macro_definition NOT NULL\nparent_id @macro_invocation\nposition_id @source_position\nend_position_id @source_position\nargument_text_id @text_atom\nordinal INTEGER NOT NULL')
entity('source_attachment', 'file_id @source_file NOT NULL\nuri_id @text_atom NOT NULL\nencoding_id @text_atom\ndigest_id @text_atom\nacquisition_kind_id @kind NOT NULL')
entity('module', 'kind_id @kind NOT NULL\nresolved_file_id @text_atom\nfile_kind_id @kind NOT NULL\ncontains_unsupported_constructs INTEGER NOT NULL DEFAULT 0\nname_id @text_atom\nresolved_header_id @text_atom\nunit_id @module\nuses_angle_brackets INTEGER NOT NULL DEFAULT 0\nsuppress_macro_export INTEGER NOT NULL DEFAULT 0\nis_internal_partition INTEGER NOT NULL DEFAULT 0')
entity('module_import', 'module_id @module NOT NULL\nsource_position_id @source_position\nmodule_name_position_id @source_position\nimplementation_unit_importing_self INTEGER NOT NULL DEFAULT 0')
relation('translation_unit_module_import', 'owner_id @translation_unit NOT NULL\nordinal INTEGER NOT NULL\nimport_id @module_import NOT NULL')

# Retained source occurrences are independent from nominal symbols and execution.
entity('source_entry', 'tag_kind_id @kind NOT NULL\ntarget_id @entity\ndeclaration_id @declaration')
relation('scope_source_entry', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\nentry_id @source_entry NOT NULL')
entity('source_sublist', '')
relation('source_sublist_entry', 'owner_id @source_sublist NOT NULL\nordinal INTEGER NOT NULL\nentry_id @source_entry NOT NULL')
relation('scope_source_sublist', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\nsublist_id @source_sublist NOT NULL')
table('secondary_declaration', '''entity_tag_kind_id @kind NOT NULL
declared_type_id @type
name_reference_id @name_reference
declared_storage_class_id @kind NOT NULL
autonomous_tag_declaration INTEGER NOT NULL DEFAULT 0
embeds_source_entries INTEGER NOT NULL DEFAULT 0
is_friend INTEGER NOT NULL DEFAULT 0
declared_in_function_prototype INTEGER NOT NULL DEFAULT 0
specialized_with_new_syntax INTEGER NOT NULL DEFAULT 0
is_first_declaration INTEGER
is_partial_instantiation INTEGER
is_compiler_generated_forward_declaration INTEGER
is_originally_nonautonomous_definition INTEGER
is_gnu_extension INTEGER NOT NULL DEFAULT 0
is_after_first_declarator INTEGER NOT NULL DEFAULT 0
has_explicit_storage_class INTEGER NOT NULL DEFAULT 0
is_alias INTEGER NOT NULL DEFAULT 0
is_event_interface INTEGER''')
tables[-1] = tables[-1].replace('id INTEGER PRIMARY KEY', 'id INTEGER PRIMARY KEY REFERENCES declaration(id) DEFERRABLE INITIALLY DEFERRED', 1)
relation('declaration_positions', 'owner_id @entity NOT NULL\nidentifier_range_id @source_range\nspecifiers_range_id @source_range\ndeclarator_range_id @source_range\nenum_value_range_id @source_range\nnamespace_definition_range_id @source_range', 'owner_id')
relation('declaration_element_position', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\nposition_id @source_position')
entity('source_end_construct', 'target_tag_kind_id @kind NOT NULL\ntarget_id @entity\nposition_id @source_position')
entity('instantiation_directive', 'target_tag_kind_id @kind NOT NULL\ntarget_id @entity\ndeclared_type_id @type\nsource_origin_id @source_origin\ndo_not_instantiate INTEGER NOT NULL DEFAULT 0')
entity('linkage_specification', 'name_string_id @constant NOT NULL\nname_linkage_id @kind NOT NULL\nposition_id @source_position\nend_position_id @source_position')
entity('microsoft_if_exists', 'target_tag_kind_id @kind NOT NULL\ntarget_id @entity\nname_reference_id @name_reference\nposition_id @source_position\nis_if_exists INTEGER\nis_this INTEGER NOT NULL DEFAULT 0')
entity('record_declaration', 'record_type_id @record_type NOT NULL\nordinal INTEGER NOT NULL\ntag_kind_id @kind NOT NULL\ntarget_id @entity\ndeclaration_id @declaration')
entity('pragma', 'kind_id @kind NOT NULL\ntarget_tag_kind_id @kind NOT NULL\ntarget_id @entity\nsource_origin_id @source_origin\nsource_entry_id @source_entry\ntext_id @text_atom\nignore_in_back_end INTEGER NOT NULL DEFAULT 0\nis_microsoft_operator INTEGER NOT NULL DEFAULT 0')
relation('scope_pragma', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\npragma_id @pragma NOT NULL')
relation('scope_if_exists', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\nif_exists_id @microsoft_if_exists NOT NULL')
for name, columns in {
    'pragma_stdc': 'kind_id @kind NOT NULL\nvalue_id @kind NOT NULL',
    'pragma_upc': 'kind_id @kind NOT NULL\naccess_method_id @kind\ncoherence_operation_id @kind',
    'pragma_ident': 'string_constant_id @constant NOT NULL',
    'pragma_comment': 'kind_id @kind NOT NULL\nstring_constant_id @constant',
    'pragma_conform': 'kind_id @kind NOT NULL\nis_on INTEGER NOT NULL DEFAULT 0\nis_off INTEGER NOT NULL DEFAULT 0\nis_show INTEGER NOT NULL DEFAULT 0\nis_push INTEGER NOT NULL DEFAULT 0\nis_pop INTEGER NOT NULL DEFAULT 0\nidentifier_id @text_atom',
    'pragma_include_alias': 'long_file_name_id @text_atom\nshort_file_name_id @text_atom',
    'pragma_gcc': 'kind_id @kind NOT NULL\nvisibility_id @kind',
}.items():
    table(name, columns)
    tables[-1] = tables[-1].replace('id INTEGER PRIMARY KEY', 'id INTEGER PRIMARY KEY REFERENCES pragma(id) DEFERRABLE INITIALLY DEFERRED', 1)

append_columns('symbol', 'is_imported_from_module INTEGER NOT NULL DEFAULT 0')
append_columns('type', '''descriptor_incomplete INTEGER DEFAULT 0
used_in_exception_or_rtti INTEGER NOT NULL DEFAULT 0
declared_in_function_prototype INTEGER NOT NULL DEFAULT 0
is_tag_redefinition INTEGER NOT NULL DEFAULT 0
alignment_set_explicitly INTEGER NOT NULL DEFAULT 0
may_alias INTEGER NOT NULL DEFAULT 0
has_microsoft_w64_specifier INTEGER NOT NULL DEFAULT 0
is_microsoft_intrinsic INTEGER NOT NULL DEFAULT 0
autonomous_primary_tag_declaration INTEGER NOT NULL DEFAULT 0
is_builtin_va_list INTEGER NOT NULL DEFAULT 0
is_builtin_va_list_from_cstdarg INTEGER NOT NULL DEFAULT 0
has_gnu_abi_tag_attribute INTEGER NOT NULL DEFAULT 0
in_gnu_abi_tag_namespace INTEGER NOT NULL DEFAULT 0
use_cfront_transitional_nested_type_name_mangling INTEGER NOT NULL DEFAULT 0
typeinfo_variable_id @variable_symbol''')
append_columns('enum_symbol', '''explicit_underlying_type_id @type
has_explicit_base INTEGER NOT NULL DEFAULT 0
underlying_type_should_use_unsigned INTEGER NOT NULL DEFAULT 0
is_packed INTEGER NOT NULL DEFAULT 0
originally_unnamed INTEGER NOT NULL DEFAULT 0
is_template_enum INTEGER NOT NULL DEFAULT 0
is_prototype_instantiation INTEGER NOT NULL DEFAULT 0
is_nonreal INTEGER NOT NULL DEFAULT 0
is_specialized INTEGER NOT NULL DEFAULT 0
is_ms_instantiated_nonreal_enum INTEGER NOT NULL DEFAULT 0
elf_visibility_id @kind
enumerator_list_seen INTEGER NOT NULL DEFAULT 0
enumerator_list_complete INTEGER NOT NULL DEFAULT 0
has_nodiscard INTEGER NOT NULL DEFAULT 0
base_position_id @source_position
template_id @template_definition
uuid_string_id @text_atom
uuid_variable_id @variable_symbol''')
append_columns('arithmetic_type', 'explicitly_signed INTEGER NOT NULL DEFAULT 0\naffiliated_enum_type_id @type\nis_fract_type INTEGER NOT NULL DEFAULT 0\nis_saturating INTEGER NOT NULL DEFAULT 0')
append_columns('qualified_type', 'is_atomic INTEGER NOT NULL DEFAULT 0\nis_nullable INTEGER NOT NULL DEFAULT 0\nis_nonnull INTEGER NOT NULL DEFAULT 0\nis_unaligned INTEGER NOT NULL DEFAULT 0\nis_near INTEGER NOT NULL DEFAULT 0\nis_far INTEGER NOT NULL DEFAULT 0\nis_upc_strict INTEGER NOT NULL DEFAULT 0\nis_upc_relaxed INTEGER NOT NULL DEFAULT 0')
append_columns('array_type', '''is_template_dependent INTEGER NOT NULL DEFAULT 0
is_variable INTEGER NOT NULL DEFAULT 0
is_vla INTEGER NOT NULL DEFAULT 0
has_vla_dimension INTEGER NOT NULL DEFAULT 0
is_static INTEGER NOT NULL DEFAULT 0
is_threads_dimension INTEGER NOT NULL DEFAULT 0
bound_constant_id @constant
dependent_bound_constant_id @constant
is_const INTEGER NOT NULL DEFAULT 0
is_volatile INTEGER NOT NULL DEFAULT 0
is_restrict INTEGER NOT NULL DEFAULT 0
is_atomic INTEGER NOT NULL DEFAULT 0
is_nullable INTEGER NOT NULL DEFAULT 0
is_nonnull INTEGER NOT NULL DEFAULT 0
is_unaligned INTEGER NOT NULL DEFAULT 0
is_near INTEGER NOT NULL DEFAULT 0
is_far INTEGER NOT NULL DEFAULT 0
is_shared INTEGER NOT NULL DEFAULT 0
is_upc_strict INTEGER NOT NULL DEFAULT 0
is_upc_relaxed INTEGER NOT NULL DEFAULT 0''')
append_columns('member_pointer_type', 'original_declaring_type_id @type\nis_ptr32 INTEGER NOT NULL DEFAULT 0\nis_ptr64 INTEGER NOT NULL DEFAULT 0')
append_columns('dependent_type', 'is_pack INTEGER NOT NULL DEFAULT 0\nis_auto_parameter INTEGER NOT NULL DEFAULT 0\nis_decltype_auto INTEGER NOT NULL DEFAULT 0\noriginally_class_template_parameter INTEGER NOT NULL DEFAULT 0\nis_unsigned_bit_precise_integer INTEGER NOT NULL DEFAULT 0\nproxy_class_id @type')
append_columns('vector_type', 'edg_vector_kind_id @kind')
append_columns('type_reference', '''is_predeclared INTEGER NOT NULL DEFAULT 0
has_variably_modified_type INTEGER NOT NULL DEFAULT 0
is_lowered_variably_modified_type INTEGER NOT NULL DEFAULT 0
decltype_expression_not_parenthesized INTEGER NOT NULL DEFAULT 0
is_dependent_type_operator INTEGER NOT NULL DEFAULT 0
is_nonreal INTEGER NOT NULL DEFAULT 0
is_dependent INTEGER NOT NULL DEFAULT 0
is_prototype_instantiation INTEGER NOT NULL DEFAULT 0
is_lowered_complex_type INTEGER NOT NULL DEFAULT 0
embeds_source_entries INTEGER NOT NULL DEFAULT 0
added_to_record_name INTEGER NOT NULL DEFAULT 0
has_typename_prefix INTEGER NOT NULL DEFAULT 0
is_global_qualified_name INTEGER NOT NULL DEFAULT 0
is_intrinsic_member INTEGER NOT NULL DEFAULT 0
explicit_memory_attribute_made_implicit INTEGER NOT NULL DEFAULT 0
name_qualifier_id @name_qualifier
proxy_class_id @type
operator_type_argument_id @type
minimum_template_arguments INTEGER
type_id_range_id @source_range''')
relation('typeref_original_template_argument', 'owner_id @type_reference NOT NULL\nordinal INTEGER NOT NULL\nargument_id @template_argument NOT NULL')
append_columns('record_type', '''is_final INTEGER NOT NULL DEFAULT 0
any_const_member INTEGER NOT NULL DEFAULT 0
any_volatile_member INTEGER NOT NULL DEFAULT 0
any_mutable_member INTEGER NOT NULL DEFAULT 0
any_virtual_base_classes INTEGER NOT NULL DEFAULT 0
any_virtual_functions INTEGER NOT NULL DEFAULT 0
any_pure_virtual_functions INTEGER NOT NULL DEFAULT 0
any_virtual_functions_including_bases INTEGER NOT NULL DEFAULT 0
nested_class_defined_outside_parent INTEGER NOT NULL DEFAULT 0
originally_unnamed INTEGER NOT NULL DEFAULT 0
is_nonstandard_anonymous_union INTEGER NOT NULL DEFAULT 0
is_template_class INTEGER NOT NULL DEFAULT 0
is_nonreal_class INTEGER NOT NULL DEFAULT 0
is_ms_instantiated_nonreal_class INTEGER NOT NULL DEFAULT 0
is_prototype_instantiation INTEGER NOT NULL DEFAULT 0
is_specialized INTEGER NOT NULL DEFAULT 0
specialized_with_old_syntax INTEGER NOT NULL DEFAULT 0
is_in_class_specialization INTEGER NOT NULL DEFAULT 0
explicitly_instantiated INTEGER NOT NULL DEFAULT 0
do_not_instantiate INTEGER NOT NULL DEFAULT 0
is_proxy_class INTEGER NOT NULL DEFAULT 0
no_proper_data INTEGER NOT NULL DEFAULT 0
has_zero_init_component INTEGER NOT NULL DEFAULT 0
has_pointer_component INTEGER NOT NULL DEFAULT 0
contains_flexible_array_member INTEGER NOT NULL DEFAULT 0
is_transparent INTEGER NOT NULL DEFAULT 0
is_packed INTEGER NOT NULL DEFAULT 0
has_internal_linkage_attribute INTEGER NOT NULL DEFAULT 0
has_operator_ampersand INTEGER NOT NULL DEFAULT 0
copy_assignment_declaration_suppressed INTEGER NOT NULL DEFAULT 0
copy_constructor_declaration_suppressed INTEGER NOT NULL DEFAULT 0
default_constructor_declaration_suppressed INTEGER NOT NULL DEFAULT 0
destructor_declaration_suppressed INTEGER NOT NULL DEFAULT 0
maximum_member_alignment TEXT
is_interface INTEGER NOT NULL DEFAULT 0
is_interface_like INTEGER NOT NULL DEFAULT 0
is_sealed INTEGER NOT NULL DEFAULT 0''')
entity('ms_attribute', '''kind_id @kind NOT NULL
name_id @text_atom
text_id @text_atom
target_tag_kind_id @kind NOT NULL
target_id @entity
source_position_id @source_position
source_entry_id @source_entry
next_in_block_id @ms_attribute''')
entity('ms_attribute_argument', '''kind_id @kind NOT NULL
parameter_name_id @text_atom
integer_value TEXT
boolean_value INTEGER
enumeration_ordinal INTEGER
string_constant_id @constant
other_text_id @text_atom
uuid_text_id @text_atom''')
relation('owner_ms_attribute', 'owner_id @entity NOT NULL\nordinal INTEGER NOT NULL\nattribute_id @ms_attribute NOT NULL')
relation('ms_attribute_argument_link', 'owner_id @ms_attribute NOT NULL\nordinal INTEGER NOT NULL\nargument_id @ms_attribute_argument NOT NULL')
relation('ms_attribute_target', 'owner_id @ms_attribute NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL')
entity('scoped_expression', 'expression_id @expression NOT NULL\nsource_origin_id @source_origin')
relation('local_expression_reference', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\nkind_id @kind NOT NULL\nreferrer_id @entity NOT NULL\nexpression_id @expression NOT NULL')
append_columns('array_type', 'dependent_bound_expression_id @expression\naddress_space INTEGER\nis_null_unspecified INTEGER NOT NULL DEFAULT 0')
append_columns('qualified_type', 'is_null_unspecified INTEGER NOT NULL DEFAULT 0')
append_columns('field_symbol', 'bit_size_constant_id @constant\nbit_width_expression_id @expression\ndeclared_bit_size TEXT')
entity('template_symbol', 'descriptor_id @template_definition NOT NULL')
append_columns('template_definition', 'symbol_id @symbol')
append_columns('initialization', 'is_freeing_of_exception_object INTEGER NOT NULL DEFAULT 0')

append_columns('attribute_argument', 'is_local_expression_reference INTEGER NOT NULL DEFAULT 0\nscoped_expression_id @scoped_expression')
append_columns('constant', 'associated_storage_variable_id @variable_symbol')
append_columns('scope', 'function_static_lifetime_id @lifetime')
append_columns('cleanup_storage_location', 'is_indirect INTEGER NOT NULL DEFAULT 0\nuses_this_parameter_offset INTEGER NOT NULL DEFAULT 0')
entity('local_static_initialization', 'variable_id @variable_symbol NOT NULL\ninitialization_kind_id @kind NOT NULL\ninitialization_id @initialization\nconstant_id @constant\nlifetime_id @lifetime')
relation('scope_local_static_initialization', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\ninitialization_id @local_static_initialization NOT NULL')
entity('constructor_initializer', 'kind_id @kind NOT NULL\nfield_id @field_symbol\nbase_subobject_id @base_subobject\ninitialization_id @initialization\nsource_expression_id @expression\ncompiler_generated INTEGER NOT NULL\nis_pack_expansion INTEGER NOT NULL DEFAULT 0\nis_braced INTEGER NOT NULL DEFAULT 0\nuses_field_initializer INTEGER NOT NULL DEFAULT 0\noriginal_type_id @type\nsource_range_id @source_range')
relation('scope_constructor_initializer', 'owner_id @scope NOT NULL\nordinal INTEGER NOT NULL\ninitializer_id @constructor_initializer NOT NULL')
entity('data_member_spec', 'type_id @type NOT NULL\nname_id @text_atom\nrequested_alignment TEXT NOT NULL\nbit_width TEXT NOT NULL\nno_unique_address INTEGER NOT NULL DEFAULT 0')
append_columns('variable_symbol', '''cleanup_routine_id @routine_symbol
is_structured_binding INTEGER NOT NULL DEFAULT 0
is_structured_binding_container INTEGER NOT NULL DEFAULT 0
bound_expression_id @expression
standard_thread_local_declared INTEGER NOT NULL DEFAULT 0
standard_thread_local_effective INTEGER NOT NULL DEFAULT 0
extension_thread_local INTEGER NOT NULL DEFAULT 0
assembly_name_id @text_atom
named_register_number INTEGER
register_kind_id @kind
dynamic_initialization_routine_id @routine_symbol
thread_initialization_routine_id @routine_symbol
thread_wrapper_routine_id @routine_symbol
initialization_rewritten_as_assignment INTEGER NOT NULL DEFAULT 0
lowering_generated INTEGER NOT NULL DEFAULT 0
promoted_local_static INTEGER NOT NULL DEFAULT 0
promoted_local_static_initialization INTEGER NOT NULL DEFAULT 0
extends_lifetime INTEGER NOT NULL DEFAULT 0
is_uninitialized INTEGER NOT NULL DEFAULT 0
has_flexible_array_initializer INTEGER NOT NULL DEFAULT 0
is_immutable INTEGER NOT NULL DEFAULT 0
is_constexpr INTEGER NOT NULL DEFAULT 0
is_constinit INTEGER NOT NULL DEFAULT 0
is_pack INTEGER NOT NULL DEFAULT 0
is_pack_element INTEGER NOT NULL DEFAULT 0''')
append_columns('routine_symbol', 'is_defined INTEGER NOT NULL DEFAULT 0\nhas_no_effect INTEGER\nis_ineligible INTEGER')

append_columns('variable_symbol', '''associated_parameter_type_id @parameter_type
explicit_alignment TEXT
initialization_priority INTEGER
section_id @text_atom
comdat_group_id @text_atom
aliased_variable_id @variable_symbol
vla_element_count_variable_id @variable_symbol
effective_class_typeinfo_variable_id @variable_symbol
is_used INTEGER NOT NULL DEFAULT 0
address_taken INTEGER NOT NULL DEFAULT 0
referenced_non_locally INTEGER NOT NULL DEFAULT 0
modified_within_try_block INTEGER NOT NULL DEFAULT 0
is_template_variable INTEGER NOT NULL DEFAULT 0
is_prototype_instantiation INTEGER NOT NULL DEFAULT 0
is_nonreal INTEGER NOT NULL DEFAULT 0
is_specialized INTEGER NOT NULL DEFAULT 0
specialized_with_old_syntax INTEGER NOT NULL DEFAULT 0
explicit_instantiation INTEGER NOT NULL DEFAULT 0
class_explicitly_instantiated INTEGER NOT NULL DEFAULT 0
explicit_do_not_instantiate INTEGER NOT NULL DEFAULT 0
parameter_value_changed INTEGER NOT NULL DEFAULT 0
parameter_used_more_than_once INTEGER NOT NULL DEFAULT 0
is_handler_parameter INTEGER NOT NULL DEFAULT 0
is_this_parameter INTEGER NOT NULL DEFAULT 0
is_anonymous_parent_object INTEGER NOT NULL DEFAULT 0
is_member_constant INTEGER NOT NULL DEFAULT 0
suppress_inline_definition INTEGER NOT NULL DEFAULT 0
superseded_external INTEGER NOT NULL DEFAULT 0
has_variably_modified_type INTEGER NOT NULL DEFAULT 0
is_vla INTEGER NOT NULL DEFAULT 0
is_compound_literal INTEGER NOT NULL DEFAULT 0
has_explicit_initializer INTEGER NOT NULL DEFAULT 0
has_parenthesized_initializer INTEGER NOT NULL DEFAULT 0
has_direct_braced_initializer INTEGER NOT NULL DEFAULT 0
declared_with_auto INTEGER NOT NULL DEFAULT 0
declared_with_decltype_auto INTEGER NOT NULL DEFAULT 0
declared_with_class_template_placeholder INTEGER NOT NULL DEFAULT 0
declared_using_type_without_linkage INTEGER NOT NULL DEFAULT 0
is_enhanced_for_iterator INTEGER NOT NULL DEFAULT 0
initializer_in_class INTEGER NOT NULL DEFAULT 0
constant_valued_recorded INTEGER NOT NULL DEFAULT 0
is_template_parameter_object INTEGER NOT NULL DEFAULT 0
is_in_class_specialization INTEGER NOT NULL DEFAULT 0
is_optional_vtable INTEGER NOT NULL DEFAULT 0
vtable_defined INTEGER NOT NULL DEFAULT 0
is_weak INTEGER NOT NULL DEFAULT 0
is_weakref INTEGER NOT NULL DEFAULT 0
is_gnu_alias INTEGER NOT NULL DEFAULT 0
has_gnu_used_attribute INTEGER NOT NULL DEFAULT 0
has_gnu_abi_tag_attribute INTEGER NOT NULL DEFAULT 0
is_not_common INTEGER NOT NULL DEFAULT 0
is_common INTEGER NOT NULL DEFAULT 0
has_internal_linkage_attribute INTEGER NOT NULL DEFAULT 0
elf_visibility_kind_id @kind
declared_storage_class_id @kind
declared_type_id @type
embeds_source_entries INTEGER NOT NULL DEFAULT 0''')



# Optional differing raw descriptor state has no default factual value.
for index, definition in enumerate(tables):
    definition = definition.replace('descriptor_incomplete INTEGER DEFAULT 0', 'descriptor_incomplete INTEGER')
    definition = definition.replace('symbol_id INTEGER REFERENCES type_symbol(id)', 'symbol_id INTEGER REFERENCES symbol(id)')
    if definition.startswith('CREATE TABLE field_layout ('):
        definition = definition.replace('offset TEXT NOT NULL', 'offset TEXT')
    tables[index] = definition

custom = {
 'entity': 'translation_unit name scope symbol namespace routine variable field label enum alias using type declaration expression statement initialization lifetime temporary base_subobject template template_parameter template_argument template_instance template_specialization source_file include_occurrence source_mapping source_position source_range source_origin macro_definition macro_invocation source_attachment attribute subobject_path name_reference exception_specification virtual_table record_layout vla_dimension coroutine function_eh cleanup_region cleanup_action cleanup_storage cleanup_array cleanup_condition cleanup_abi',
 'capture_profile': 'native_lowered source_semantic cfront_lowered exported_templates_lowered',
 'abi': 'itanium cfront microsoft',
 'availability': 'available absent not_applicable dependent disabled lost not_retained',
 'fact': 'source source_target source_dispatch source_parameters original_type lowering_origin template_origin declaration_sequence macro_tree macro_arguments tokens trivia lifetime_details',
 'role': 'operand callee argument receiver return_slot condition then else body initialization increment statement result metadata backing_expression primary_template selected_partial_specialization type_operand ready suspend resume yield return_value affinity cleanup normal execution source_parameter this_parameter return_parameter vtt_parameter abi_parameter ordinary_parameter member base element constructor destructor allocator deallocator constraint',
 'representation_role': 'execution metadata source_semantic',
 'value_category': 'prvalue lvalue xvalue',
 'call_kind': 'direct indirect member member_pointer',
 'float_class': 'finite zero infinity nan',
 'string_encoding': 'target_code_units utf8 ascii utf16 utf32 unknown',
 'name_category': 'named anonymous unnamed generated',
 'body_state': 'definition declaration_only dependent discarded unavailable',
 'definition_state': 'complete incomplete dependent',
 'bound_kind': 'constant incomplete zero variable dependent',
 'reference_kind': 'variable routine field label constant type builtin property destructor',
 'jump_kind': 'return goto break continue leave assigned_goto',
 'cleanup_kind': 'destructor deallocation guard exception_object special',
 'storage_location_kind': 'direct_variable indirect_variable receiver_relative static_address',
 'condition_kind': 'constructed nonzero state',
 'source_origin_kind': 'ordinary predefined command_line unknown synthesized macro',
 'range_endpoint': 'inclusive last_character hypothetical',
 'specialization_kind': 'explicit partial implicit',
 'type_relationship': 'declared storage subobject_partner retained_original',
 'abi_routine_kind': 'primary alternate_complete alternate_subobject deleting_destructor thunk wrapper target',
 'template_argument_role': 'primary selected_partial',
 'instantiation_state': 'concrete dependent external_definition suppressed',
 'asm_operand_kind': 'input output inout',
 'scope_kind': 'file namespace class function block prototype template',
 'linkage': 'none internal external c cpp',
 'storage_duration': 'automatic static thread allocated parameter',
 'access': 'public protected private none',
 'ref_qualifier': 'none lvalue rvalue',
 'calling_convention': 'default cdecl stdcall fastcall thiscall vectorcall',
 'type_reference_kind': 'typedef qualified decltype template alias',
 'exception_specification_kind': 'none dynamic noexcept dependent',
 'vector_kind': 'fixed scalable riscv',
 'virtual_table_kind': 'vtable vtt rtti',
 'attribute_kind': 'gnu microsoft standard',
 'capture_kind': 'copy reference this star_this initialization',
 'managed_kind': 'cli cx handle tracking_reference property event delegate array',
 'source_attachment_kind': 'frozen_compilation_input post_compilation_reread external',
 'lifetime_transition_kind': 'enter exit jump return throw',
 'special_function_kind': 'ordinary constructor destructor conversion operator thunk wrapper',
 'float_representation': 'binary16 bfloat16 binary32 binary64 binary80 binary128 ibm_double_double',
 'arithmetic_kind': 'signed_integer unsigned_integer boolean character floating fixed_point complex imaginary',
}
rows=[]
for i,(family,names) in enumerate(custom.items(),1):
    for j,name in enumerate(names.split(),1): rows.append((i*1000+j,family,name))
prefixes={'stmk_':'statement','enk_':'expression','eok_':'operator','bok_':'builtin','ck_':'constant','abk_':'address_base','tk_':'type','dik_':'dynamic_init','cik_':'constructor_init','tak_':'template_argument','templk_':'template','olk_':'lifetime','leck_':'lowered_eh','tpck_':'template_constant','tptk_':'template_type','sfepk_':'for_each_pattern','tpk_':'template_parameter','initk_':'initializer','lerk_':'local_expression_reference'}
text=DESIGN.read_text()
for i,(prefix,family) in enumerate(prefixes.items(),100):
    names=list(dict.fromkeys(re.findall(r'`('+prefix+r'[A-Za-z0-9_]+)`',text)))
    for j,name in enumerate(names,1): rows.append((i*1000+j,family,name))
# The explicit checked-in TSV is authoritative. Bootstrap extraction never runs
# once this exists: prose edits cannot renumber the persisted format.
catalog_path = HERE/'kinds.tsv'
if catalog_path.exists():
    rows = [(int(id_),family,name) for id_,family,name in
            (line.split('\t') for line in catalog_path.read_text().splitlines()[1:] if line)]
    assert len({row[0] for row in rows}) == len(rows), 'duplicate stable ID'
    assert len({row[1:] for row in rows}) == len(rows), 'duplicate kind name'
else:
    catalog=''.join(f'{id_}\t{family}\t{name}\n' for id_,family,name in rows)
    catalog_path.write_text('id\tfamily\tname\n'+catalog)
# Large extension catalogs are compiled/source-owned, and persisted on demand.
# Only the small original foundation is embedded in every object. The native
# writer copies exact stable rows for every actually referenced extension kind.
embedded_rows=[row for row in rows if row[0]<300000]
sql_parts=['-- RoslynObj 1.0: normalized scalar and typed relationship contract.\n-- SQLite 3.37+ required. Exact target quantities use canonical decimal TEXT.\n-- Extension kind catalogs are persisted only when referenced.\nPRAGMA foreign_keys=ON;\nPRAGMA user_version=1000;\n'+ '\n\n'.join(tables)+'\n']
for id_,family,name in embedded_rows:
    sql_parts.append(f"INSERT INTO kind VALUES({id_},'{family}','{name}');\n")
kind_ids={(family,name):id_ for id_,family,name in rows}
aliases={'enk_runtime_sizeof':('expression','enk_sizeof'), 'tk_enum':('type','tk_integer'), 'dik_call_returning_class_via_cctor':('dynamic_init','dik_class_result_via_ctor')}
for id_,family,name in embedded_rows:
    invalid = name.endswith('_last') or name in ('tk_error','tk_unknown','enk_error','enk_reclaimed','eok_error','ck_error','tpk_error','lerk_none')
    # Disposition is about concrete graph nodes. tk_unknown remains valid as an
    # auxiliary operator type discriminator; consumers must respect field role.
    state='invalid_concrete_node' if invalid else 'alias' if name in aliases else 'semantic'
    canonical=kind_ids.get(aliases[name]) if name in aliases else None
    sql_parts.append(f"INSERT INTO kind_disposition VALUES({id_},'{state}',{canonical if canonical else 'NULL'});\n")
sql_parts.append('CREATE INDEX symbol_name ON symbol(name_id);\nCREATE INDEX symbol_owner ON symbol(containing_entity_id);\nCREATE INDEX declaration_symbol ON declaration(symbol_id);\nCREATE INDEX structural_parent ON structural_child(child_id);\nCREATE INDEX semantic_reference_target ON semantic_reference(target_id);\nCREATE INDEX macro_parent ON macro_invocation(parent_id,ordinal);\nCREATE INDEX cleanup_function ON cleanup_region(function_eh_id,ordinal);\nCREATE INDEX template_instance_template ON template_instance(template_id);\nCREATE INDEX type_name ON type(name_id);\nCREATE INDEX lifetime_binding ON lifetime(bound_entity_id);\n')
sql_parts.append('CREATE INDEX constant_component_target ON constant_component(constant_id);\n')
sql_parts.append('CREATE INDEX constant_name ON constant(name_id);\n')
sql_parts.append('CREATE INDEX constant_owner ON constant(containing_entity_id);\nCREATE INDEX reference_expression_target ON reference_expression(target_id);\n')
sql_parts.append('CREATE INDEX enumerator_constant ON enumerator(constant_id);\nCREATE INDEX enumerator_symbol ON enumerator(symbol_id);\n')
sql=''.join(sql_parts)
# Execute actual DDL before publication; this tests storage syntax only.
connection=sqlite3.connect(':memory:')
connection.executescript(sql)
connection.close()
(HERE/'roslynobj.sql').write_text(sql)
(HERE/'schema.inc').write_text('static const char roslynobj_schema_sql[] = R"roslynsql('+sql+')roslynsql";\n')
print(f'{len(tables)} tables, {len(rows)} stable kinds')
