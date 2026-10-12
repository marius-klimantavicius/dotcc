#!/usr/bin/env python3
"""Generate schema/kind includes mechanically from the shared contract."""
import argparse,csv,pathlib,re,subprocess
root=pathlib.Path(__file__).resolve().parents[1]
p=argparse.ArgumentParser();p.add_argument('--upstream',type=pathlib.Path,default=pathlib.Path('/tmp/roslynobj-edg-upstream-20261012'));args=p.parse_args()
pin='89e67e07c7f6d0fc38622f9778d8e79f40deaa0f'
if subprocess.check_output(['git','-C',str(args.upstream),'rev-parse','HEAD'],text=True).strip()!=pin:raise SystemExit('Upstream does not match required pin')
up=args.upstream/'src/il_def.h'
rows=list(csv.DictReader((root/'schema/kinds.tsv').open(),delimiter='\t'))
lookup={(r['family'],r['name']):int(r['id']) for r in rows}
out=['/* Generated from schema/kinds.tsv and pristine EDG enums. */']
out.append('static const struct { const char *family; const char *name; int id; } roslyn_catalog[] = {')
out.extend('{"'+r['family']+'","'+r['name']+'",'+r['id']+'},' for r in sorted(rows,key=lambda r:(r['family'],r['name'])))
out.append('};')
src=re.sub(r'/\*.*?\*/',lambda m:'\n'*m.group(0).count('\n'),up.read_text(),flags=re.S)
families={'a_statement_kind':'statement','an_expr_node_kind':'expression','an_expr_operator_kind':'operator','a_builtin_operation_kind':'builtin','a_constant_repr_kind':'constant','an_address_base_kind':'address_base','a_type_kind':'type','a_dynamic_init_kind':'dynamic_init','a_constructor_init_kind':'constructor_init','a_templ_arg_kind':'template_argument','a_template_kind':'template','an_object_lifetime_kind':'lifetime','a_lowered_eh_construct_kind':'lowered_eh','a_template_param_constant_kind':'template_constant','a_template_param_type_kind':'template_type','a_for_each_pattern_kind':'for_each_pattern','a_template_parameter_kind':'template_parameter','an_init_kind':'initializer','an_integer_kind':'edg_integer_kind','a_float_kind':'edg_float_kind','a_special_function_kind':'edg_special_function','a_name_linkage_kind':'edg_name_linkage','a_storage_class':'edg_storage_class','an_access_specifier':'edg_access','a_calling_convention':'edg_calling_convention','a_typeref_kind':'edg_typeref','an_attribute_kind':'edg_attribute','an_attribute_arg_kind':'edg_attribute_argument','an_attribute_family':'edg_attribute_family','an_attribute_location':'edg_attribute_location','a_builtin_function_kind_tag':'edg_builtin_function','a_token_kind':'token','a_builtin_user_function_kind':'edg_builtin_user_function','a_scope_kind':'edg_scope','a_ref_qualifier_kind':'edg_ref_qualifier','a_fixed_point_precision':'edg_fixed_precision','a_character_kind':'edg_character','an_opname_kind':'edg_opname','an_il_entry_kind':'edg_il_entry','an_asm_operand_constraint_kind':'edg_asm_constraint','an_asm_operand_modifier':'edg_asm_modifier','a_named_register':'edg_named_register','a_module_kind':'edg_module','a_module_file_kind':'edg_module_file','a_rewritten_property_reference_kind':'rewritten_property'}
for typ,fam in families.items():
 enum_src=src
 if typ=='a_builtin_user_function_kind': enum_src=re.sub(r'/\*.*?\*/',lambda m:'\n'*m.group(0).count('\n'),up.with_name('sys_predef.h').read_text(),flags=re.S)
 if typ=='a_builtin_function_kind_tag': enum_src=re.sub(r'/\*.*?\*/',lambda m:'\n'*m.group(0).count('\n'),up.with_name('builtin_kinds.h').read_text(),flags=re.S)
 m=re.search(r'enum '+typ+r'[^\{]*\{(.*?)\};',enum_src,re.S)
 if not m:raise SystemExit('Missing enum '+typ)
 guard={'a_lowered_eh_construct_kind':'DO_IL_LOWERING && !DO_FULL_PORTABLE_EH_LOWERING','a_fixed_point_precision':'FIXED_POINT_ALLOWED','an_asm_operand_constraint_kind':'GNU_EXTENSIONS_ALLOWED && !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS','an_asm_operand_modifier':'GNU_EXTENSIONS_ALLOWED && !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS','a_named_register':'GNU_EXTENSIONS_ALLOWED','a_rewritten_property_reference_kind':'MICROSOFT_EXTENSIONS_ALLOWED'}.get(typ)
 if guard:out.append('#if '+guard)
 out.append('static int roslyn_kind_'+fam+'('+typ+' value) { switch(value) {')
 for line in m.group(1).splitlines():
  if line.lstrip().startswith('#'):out.append(line)
  else:
   for e,alias in re.findall(r'\b([A-Za-z]+_[A-Za-z_0-9]+)\s*(=\s*[^,\n]+)?',line):
    if e in ('fk_first_extended_type','bufk_first'):continue
    if alias and not re.fullmatch(r'=\s*(?:[0-9]+|0x[0-9a-fA-F]+)',alias) and e!='fk_std_bfloat16':continue
    if (fam,e) not in lookup:
     if e.endswith('_last'):out.append('case '+e+': return 0;');continue
     raise SystemExit('Missing catalog '+fam+' '+e)
    out.append('case '+e+': return '+str(lookup[(fam,e)])+';')
 out.append('default: return 0; } }')
 if guard:out.append('#endif')
(root/'backend/roslyn_kinds.h').write_text('\n'.join(line.rstrip() for line in out)+'\n')
(root/'backend/roslyn_schema.h').write_text((root/'schema/schema.inc').read_text())
