"""Generate enum-name adapters against the pinned EDG header, keeping guards."""
import pathlib,re,sys
s=re.sub(r'/\*.*?\*/','',pathlib.Path(sys.argv[1]).read_text(),flags=re.S)
out=[]
for t in ['a_type_kind','a_statement_kind','a_scope_kind','a_dynamic_init_kind','a_constant_repr_kind','a_templ_arg_kind','a_template_kind','an_integer_kind','an_object_lifetime_kind','an_expr_operator_kind','an_init_kind','a_special_function_kind']:
 m=re.search(r'enum '+t+r'\s*:[^{]+\{(.*?)\};',s,re.S)
 if not m: raise ValueError(t)
 out.append('static const char *name_of('+t+' x) { switch(x) {')
 for line in m[1].splitlines():
  line=line.strip()
  if line.startswith('#'):out.append(line);continue
  for v in line.split(','):
   v=v.strip()
   if '=' in v:continue
   if re.fullmatch(r'\w+',v):out.append('case '+v+': return "'+v+'";')
 out.append('default: return "unknown"; }}')
path=pathlib.Path(sys.argv[2]);value='\n'.join(out)+'\n'
if not path.exists() or path.read_text()!=value:path.write_text(value)
