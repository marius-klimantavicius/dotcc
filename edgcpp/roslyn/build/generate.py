#!/usr/bin/env python3
"""Generate schema/kind includes mechanically from the shared contract."""
import csv,pathlib,re
root=pathlib.Path(__file__).resolve().parents[1]
up=pathlib.Path('/tmp/roslynobj-edg-upstream-20261012/src/il_def.h')
rows=list(csv.DictReader((root/'schema/kinds.tsv').open(),delimiter='\t'))
lookup={(r['family'],r['name']):int(r['id']) for r in rows}
out=['/* Generated from schema/kinds.tsv and pristine EDG enums. */']
out.append('static const char *roslyn_catalog_sql = R"ROS('+''.join("INSERT INTO kind VALUES ("+r['id']+",'"+r['family']+"','"+r['name']+"');\n" for r in rows)+')ROS";')
out.append('static const struct { const char *family; const char *name; int id; } roslyn_catalog[] = {')
out.extend('{"'+r['family']+'","'+r['name']+'",'+r['id']+'},' for r in rows)
out.append('};')
src=re.sub(r'/\*.*?\*/',lambda m:'\n'*m.group(0).count('\n'),up.read_text(),flags=re.S)
families={'a_statement_kind':'statement','an_expr_node_kind':'expression','an_expr_operator_kind':'operator','a_builtin_operation_kind':'builtin','a_constant_repr_kind':'constant','an_address_base_kind':'address_base','a_type_kind':'type','a_dynamic_init_kind':'dynamic_init','a_constructor_init_kind':'constructor_init','a_templ_arg_kind':'template_argument','a_template_kind':'template','an_object_lifetime_kind':'lifetime','a_lowered_eh_construct_kind':'lowered_eh','a_template_param_constant_kind':'template_constant','a_template_param_type_kind':'template_type','a_for_each_pattern_kind':'for_each_pattern','a_template_parameter_kind':'template_parameter','an_init_kind':'initializer','an_integer_kind':'edg_integer_kind','a_float_kind':'edg_float_kind'}
for typ,fam in families.items():
 m=re.search(r'enum '+typ+r'[^\{]*\{(.*?)\};',src,re.S)
 if not m:raise SystemExit('Missing enum '+typ)
 out.append('static int roslyn_kind_'+fam+'('+typ+' value) { switch(value) {')
 for line in m.group(1).splitlines():
  if line.lstrip().startswith('#'):out.append(line)
  else:
   for e,alias in re.findall(r'\b([a-z]+k_[A-Za-z_0-9]+)\s*(=\s*\w+)?',line):
    if alias:continue
    if (fam,e) not in lookup:raise SystemExit('Missing catalog '+fam+' '+e)
    out.append('case '+e+': return '+str(lookup[(fam,e)])+';')
 out.append('default: return 0; } }')
(root/'backend/roslyn_kinds.h').write_text('\n'.join(out)+'\n')
(root/'backend/roslyn_schema.h').write_text((root/'schema/schema.inc').read_text())
