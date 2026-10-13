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
    'contains_unsupported_constructs','implementation_unit_importing_self','descriptor_incomplete','boolean_value','no_unique_address'}
numeric_text = {'scope_number','local_scope_number','num_template_arguments','deduced_array_bound','state_number','initial_state','null_state','size','alignment','offset','bound','length','count','adjustment','return_adjustment',
    'slot','index_value','range_end','element_offset','comparison_value','delegation_argument',
    'sequence','original_sequence','first_sequence','last_sequence','first_line','logical_line','physical_line',
    'integer_argument','integer_value','pointer_alignment','shared_block_size','number_of_constraints'}
numeric_atom = {('integer_value','value_id'),('floating_value','significand_id'),
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
            typ='bool' if col.startswith(('is_','has_')) or col in booleans or 'DEFAULT 0' in tail else 'long'
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
