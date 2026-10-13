#!/usr/bin/env python3
"""Mechanical immutable property bindings for the one supervisor-owned DDL."""
from pathlib import Path
import re
import sys

root = Path(__file__).resolve().parents[2]
sql = (root / 'schema/roslynobj.sql').read_text()
output = root / 'managed/RoslynObj/SchemaDetails.g.cs'
blocks = re.findall(r'CREATE TABLE (\w+) \(\n(.*?)\n\) (?:WITHOUT ROWID, )?STRICT;', sql, re.S)

def pascal(value):
    return ''.join(piece.title() for piece in value.split('_'))

models = {
    'entity':('SemanticEntity','Entity'), 'type':('CppType','Type'),
    'record_type':('RecordType','RecordType'),
    'symbol':('Symbol','Symbol'), 'routine_symbol':('FunctionSymbol','Function'),
    'variable_symbol':('VariableSymbol','Variable'), 'field_symbol':('FieldSymbol','Field'),
    'namespace_symbol':('NamespaceSymbol','Namespace'), 'label_symbol':('LabelSymbol','Label'),
    'semantic_node':('SemanticNode','Node'), 'expression':('ExpressionOperation','Expression'),
    'statement':('StatementOperation','Statement'), 'translation_unit':('TranslationUnit','Unit'),
    'declaration':('Declaration','Declaration'), 'scope':('Scope','Scope'),
    'constant':('Constant','Constant'), 'constant_value':('ConstantValue','Value'),
    'initialization':('Initialization','Initialization'), 'lifetime':('Lifetime','Lifetime'),
    'template_definition':('TemplateDescriptor','Template'), 'template_symbol':('TemplateSymbol','TemplateSymbol'), 'template_instance':('TemplateInstantiation','Instance'),
    'template_argument':('TemplateArgument','Argument'),
    'template_header':('TemplateHeader','Header'), 'template_parameter':('TemplateParameter','TemplateParameter'),
    'template_generic_constraint_clause':('GenericConstraintClause','GenericClause'),
    'source_origin':('SourceInfo','Source'), 'source_position':('SourcePosition','Position'),
    'source_range':('SourceRange','Range'), 'source_file':('SourceFile','File'),
    'macro_definition':('MacroDefinition','MacroDefinition'), 'macro_invocation':('MacroInvocation','MacroInvocation'),
    'cleanup_region':('CleanupRegion','Region'), 'cleanup_action':('CleanupAction','Action'),
    'cleanup_storage_location':('CleanupStorageLocation','Storage'), 'cleanup_array':('CleanupArray','Array'),
    'cleanup_condition':('CleanupCondition','Condition'), 'cleanup_abi_detail':('CleanupAbiDetail','AbiDetail'),
    'record_layout':('RecordLayout','Layout'), 'type_symbol':('TypeSymbol','TypeSymbol'),
    'handler':('ExceptionHandler','Handler'),
    'parameter_type':('ParameterType','ParameterType'), 'token_sequence':('TokenSequence','TokenSequence'),
    'lambda_definition':('LambdaDefinition','LambdaDefinition'),
    'asm_entry':('AssemblyDefinition','AssemblyDefinition'),
    'module':('CppModule','Module'), 'module_import':('ModuleImport','ModuleImport'),
    'include_occurrence':('IncludeOccurrence','Include'), 'line_directive':('LineDirective','LineDirective'),
    'source_mapping_segment':('SourceMappingSegment','Mapping'),
    'source_entry':('SourceEntry','SourceEntry'), 'source_sublist':('SourceSublist','SourceSublist'),
    'record_declaration':('RecordDeclaration','RecordDeclaration'),
    'source_end_construct':('SourceEndConstruct','SourceEnd'),
    'instantiation_directive':('InstantiationDirective','InstantiationDirective'),
    'linkage_specification':('LinkageSpecification','LinkageSpecification'),
    'microsoft_if_exists':('MicrosoftIfExists','IfExists'), 'pragma':('PragmaDirective','Pragma'),
    'ms_attribute':('MicrosoftAttribute','MicrosoftAttribute'), 'ms_attribute_argument':('MicrosoftAttributeArgument','MicrosoftAttributeArgument'),
    'scoped_expression':('ScopedExpression','ScopedExpression'),
    'attribute':('Attribute','Attribute'), 'attribute_argument':('AttributeArgument','AttributeArgument'),
    'attribute_group':('AttributeGroup','AttributeGroup'),
    'data_member_spec':('DataMemberSpecification','DataMemberSpecification'),
    'requires_clause':('RequiresClause','RequiresClause'),
    'exception_specification':('ExceptionSpecification','ExceptionSpecification'),
    'switch_descriptor':('SwitchDescriptor','SwitchDescriptor'), 'switch_case_entry':('SwitchCaseEntry','SwitchCase'),
    'vla_dimension':('VlaDimension','VlaDimension'),
    'subobject_path':('SubobjectPath','SubobjectPath'),
    'source_correspondence':('SourceCorrespondence','Correspondence'),
    'name_reference':('NameReference','NameReference'), 'name_qualifier':('NameQualifier','NameQualifier'),
    'using_declaration':('UsingDeclaration','Using'),
    'gnu_routine_supplement':('GnuRoutineSupplement','GnuRoutineSupplement'),
    'block_descriptor':('BlockDescriptor','BlockDescriptor'), 'try_descriptor':('TryDescriptor','TryDescriptor'),
    'local_static_initialization':('LocalStaticInitialization','LocalStaticInitialization'),
    'constructor_initializer':('ConstructorInitializer','ConstructorInitializer'),
    'enum_symbol':('EnumSymbol','Enum'), 'alias_symbol':('AliasSymbol','Alias'),
    'template_specialization':('TemplateSpecialization','Specialization'),
    'base_subobject':('BaseSubobject','Subobject'), 'base_derivation':('BaseDerivation','Derivation')
}
booleans = {'compiler_generated','negative','nan_quiet','signed','incomplete','implicit','result_unused','initialization_guard',
    'generated_default_argument','volatile_fetch','type_definition_needed','boolean_not_normalized',
    'selection_known','selected_then','one_past_end','frees_storage','frees_array','throw_any',
    'noexcept_effective','noexcept_required','result','required','enabled','passed_indirectly','complete',
    'fold_left_associative','reattempt_default_argument','reattempt_default_member_initializer','gnu_asm_form','error_descriptor',
    'contains_unsupported_constructs','implementation_unit_importing_self','descriptor_incomplete','boolean_value','no_unique_address',
    'contains_error','contains_local_type','contains_unnamed_namespace_type',
    'does_not_contain_parentless_lambda_in_default_argument','does_not_contain_deprecated_or_unavailable_type'}
# Direct retained flags whose nullable profile columns do not use is_/has_.
booleans.update({'vla_treated_as_zero_length_array','class_subobject_with_tail_padding','bit_field_is_signed',
    'definition_cannot_be_generated','assignment_to_this_done','never_throws','never_inline','declared_only_as_friend',
    'explicit_extern_inline','in_gnu_abi_tag_namespace','implicit_abi_tags_added','allocates_memory',
    'no_instrument_function','no_check_memory_usage','always_inline','gnu_c89_inline','implicit_alias',
    'can_be_instantiated','do_not_instantiate','instance_required','definition_needed',
    'definition_has_direct_linkage_specifier','inline_instance_required','inline_in_class_definition',
    'statics_have_been_promoted','definition_c_name_linkage_specified','result_should_be_used',
    'explicit_calling_convention','resolver_required','old_style_params_scanned','lint_argsused_flag',
    'value_returned_by_cctor','value_returned_as_parameter','return_value_parameter_follows_this',
    'assoc_routine_is_ctor','assoc_routine_is_dtor','assoc_routine_is_lambda_body',
    'routine_name_linkage_is_explicit','does_not_return','had_been_implicitly_const','has_this_param',
    'complete_subobject','reachable_by_fall_through','lowering_generated',
    'null_keyword','native_nullptr_keyword','ptr_to_mem_constant_construct','flexible_array_initializer',
    'initializes_empty_object','contains_function_name_token','added_const_for_template_param',
    'template_local_expr_ref','vptr_has_been_lowered',
    'referenced','needed','name_has_been_mangled','unnamed_entity_given_fabricated_name',
    'copied_from_secondary_trans_unit','same_name_as_external_entity_in_secondary_trans_unit',
    'member_of_unknown_base','qualified_unknown_base_member','member_of_unknown_super',
    'microsoft_identifier_used','marked_as_gnu_extension','externalized','maybe_unused',
    'named_in_strong_using','address_taken','locally_declared','parameter_used_as_lvalue'})
numeric_text = {'scope_number','local_scope_number','num_template_arguments','state_number','initial_state','null_state','size','alignment','offset','bound','length','count','adjustment','return_adjustment',
    'slot','index_value','range_end','element_offset','comparison_value','delegation_argument',
    'sequence','original_sequence','first_sequence','last_sequence','first_line','logical_line','physical_line',
    'integer_argument','integer_value','pointer_alignment','shared_block_size','number_of_constraints'}
numeric_text.update({'virtual_function_number','this_adjustment','vcall_index','return_adjustment',
    'vbase_index','initialization_priority','constructor_priority','destructor_priority',
    'virtual_function_table_index','construction_table_index','construction_subarray_index',
    'virtual_base_offset_index','highest_virtual_function_number','next_negative_virtual_table_index',
    'first_vcall_offset_index','vtable_index','string_sequence_number','microsoft_try_depth'})
numeric_atom = {('integer_value','value_id'),('upc_threads_value','multiplier_id'),('floating_value','significand_id'),
    ('floating_value','nan_payload_id'),('fixed_point_value','scaled_integer_id')}
lines = ['// Generated from schema/roslynobj.sql by managed/tools/generate_bindings.py. Do not edit.',
    '#nullable enable', 'using System.Numerics;', 'namespace RoslynObj;', '']
coverage = []
for table, block in blocks:
    if table in ('kind','text_atom'): continue
    cls = pascal(table)+'Details'
    lines += [f'public sealed class {cls} : SemanticDetail', '{',
        f'    internal {cls}(RoslynObject owner, Row row) : base(owner, row) {{ }}']
    for raw in block.splitlines():
        m = re.match(r'  (\w+) (INTEGER|TEXT)(.*)', raw)
        if not m: continue
        col,sqltype,tail=m.groups()
        if col=='id': continue
        nullable='NOT NULL' not in tail and 'PRIMARY KEY' not in tail
        ref=re.search(r'REFERENCES (\w+)\(id\)',tail)
        prop=pascal(col[:-3] if ref and col.endswith('_id') else col)
        if prop==cls:prop+='Value'
        if ref:
            target=ref.group(1)
            if target=='text_atom':
                typ='BigInteger' if (table,col) in numeric_atom else 'string'
                expr=f'Object.Text(Data.OptionalLong("{col}"))'
                if typ=='BigInteger':expr=f'Number({expr})'
                elif not nullable:expr+='!'
            elif target=='kind':
                typ='SemanticKind'
                expr=f'Object.Kind(Data.Long("{col}"))'
            else:
                typ,method=models.get(target,(pascal(target)+'Details',None))
                if method: factory=f'Object.{method}(value)'
                else:factory=f'Object.Detail("{target}", value, r => new {typ}(Object, r))'
                expr=f'Object.Link(Data.OptionalLong("{col}"), value => {factory})'
                if not nullable:expr+='!'
        elif sqltype=='TEXT':
            isnumeric=col in numeric_text or col.endswith(('_size','_alignment','_offset','_count','_width'))
            typ='BigInteger' if isnumeric else 'string'
            expr=f'Number(Data.OptionalText("{col}"))' if isnumeric else f'Data.OptionalText("{col}")'
            if not nullable:
                expr = f'RequiredNumber(Data.OptionalText("{col}"))' if isnumeric else expr+'!'
        else:
            typ='bool' if col.startswith(('is_','has_')) or col in booleans or ('DEFAULT 0' in tail and col!='direct_base_number') else 'long'
            expr=f'Data.{"Bool" if typ=="bool" else "Long"}("{col}")'
        if ref and ref.group(1)=='kind' and nullable:
            expr=f'Object.Link(Data.OptionalLong("{col}"), value => Object.Kind(value))'
        if typ=='BigInteger' and ref and not nullable:expr=f'RequiredNumber(Object.Text(Data.OptionalLong("{col}")))'
        if nullable:
            typ+='?'
            if not ref and sqltype=='INTEGER':expr=f'Data.{"OptionalBool" if typ=="bool?" else "OptionalLong"}("{col}")'
        lines.append(f'    public {typ} {prop} => {expr};')
        coverage.append(f'{table}\t{col}\t{cls}.{prop}\t{typ}')
    if re.search(r'^  id INTEGER',block,re.M):
        for child,childblock in blocks:
            cols={m.group(1):m.group(2) for m in re.finditer(r'^  (\w+) (INTEGER|TEXT)',childblock,re.M)}
            ownercol=None
            for candidate in ('owner_id','template_id','header_id','base_subobject_id','try_id','region_id','function_eh_id'):
                if re.search(rf'^  {candidate} INTEGER REFERENCES {table}\(id\)',childblock,re.M):
                    ownercol=candidate
                    break
            if ownercol is None:continue
            childcls=pascal(child)+'Details'
            prop=pascal(child)+'Rows'
            ordering=','.join(c for c in ('role_id','depth','ordinal','id') if c in cols)
            if not ordering:continue
            hydration=f', "{child}"' if 'id' in cols else ''
            lines.append(f'    public IReadOnlyList<{childcls}> {prop} => Object.List("SELECT * FROM {child} WHERE {ownercol}=$owner ORDER BY {ordering}", Data.Long("id"), r => new {childcls}(Object, r){hydration});')
    lines+=['}','']
generated='\n'.join(lines)+'\n'
matrix='table\tcolumn\tmanaged_property\ttype\n'+'\n'.join(coverage)+'\n'
matrix_path=root/'managed/field-bindings.tsv'
if '--check' in sys.argv:
    if not output.exists() or output.read_text()!=generated or not matrix_path.exists() or matrix_path.read_text()!=matrix:
        raise SystemExit('Managed bindings differ from the shared DDL. Run managed/tools/generate_bindings.py.')
else:
    output.write_text(generated)
    matrix_path.write_text(matrix)
print(f'Generated {len(blocks)-1} table bindings and {len(coverage)} typed properties.')
