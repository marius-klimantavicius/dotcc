// Public type_info operations use the same opaque addresses as vtable RTTI
// slots. The native ABI's metadata class hierarchy is not materialized.
bool user_typeinfo(a_type_ptr t) {
  return rtti_user_prefix_types.count(skip_typerefs(t)) ||
    (type_of_type_info && skip_typerefs(t)==skip_typerefs(type_of_type_info));
}
string typeinfo_constant(a_constant_ptr c) {
  if(c->kind!=ck_address || c->variant.address.kind!=abk_variable)return "";
  auto found=virtual_types.find(c->variant.address.variant.variable);
  if(found==virtual_types.end())return "";
  auto t=skip_typerefs(c->type);
  if(c->variant.address.offset || t->kind!=tk_pointer || !user_typeinfo(t->variant.pointer.type))
    unsupported("opaque RTTI address outside type_info reference");
  return "(("+type(c->type)+")(__edg_type_token_base + "+std::to_string(8*found->second)+"))";
}
a_variable_ptr typeinfo_root(an_expr_node_ptr e) {
  if(e->kind==enk_variable)return virtual_types.count(node_variable(e))?node_variable(e):nullptr;
  if(e->kind!=enk_operation)return nullptr;
  auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;
  if(op==eok_lvalue_adjust)return typeinfo_root(a);
  if(op==eok_dot_field) {
    auto f=node_field(a->next);
    if(f->offset==0 && aggregate(f->type) &&
       f==skip_typerefs(a->type)->variant.class_struct_union.field_list)return typeinfo_root(a);
  }
  return nullptr;
}
string typeinfo_expression(an_expr_node_ptr e) {
  if(!user_typeinfo(e->type))return "";
  auto v=typeinfo_root(e);
  if(!v)return "";
  return "(*("+type(e->type)+"*)(__edg_type_token_base + "+std::to_string(8*virtual_types.at(v))+"))";
}
string typeinfo_name_bytes(a_variable_ptr v) {
  if(v->init_kind==initk_static) {
    // lower_eh.c::define_typeinfo_var nests its two-field user type_info
    // prefix inside the appropriate ABI metadata class initializer.
    auto c=v->initializer.constant;
    while(c && c->kind==ck_aggregate) {
      auto fields=cleanup_fields(c,0);
      if(fields.size()==2 && fields[0]->kind==ck_address) {
        auto n=fields[1];
        if(n->kind!=ck_address || n->variant.address.offset || n->variant.address.kind!=abk_variable)
          unsupported("RTTI name address");
        auto name_var=n->variant.address.variant.variable;
        if(name_var->init_kind!=initk_static || name_var->initializer.constant->kind!=ck_string)
          unsupported("RTTI name initializer");
        auto str=name_var->initializer.constant;
        string bytes(str->variant.string.value,str->variant.string.length);
        if(bytes.empty() || bytes.back()!=0)unsupported("RTTI name terminator");
        return bytes;
      }
      c=fields.empty()?nullptr:fields[0];
    }
    unsupported("RTTI user prefix");
  }
  // EDG leaves built-in RTTI to the native library. Its Itanium symbol
  // suffix is precisely the mangled type name (lower_eh.c::make_typeinfo_name).
  // Match typeinfo_is_defined_in_runtime; never supply unresolved class RTTI.
  auto t=rtti_source_types.at(v);
  bool builtin=is_void_type(t) || is_integral_type(t) || is_real_floating_type(t) || is_or_was_nullptr_type(t);
  if(is_pointer_type(t)) {
    auto p=type_pointed_to(t);auto q=get_type_qualifiers(p);
    builtin=(q==TQ_NONE || q==TQ_CONST) && (is_void_type(p) || is_integral_type(p) || is_real_floating_type(p));
  }
  string symbol=v->source_corresp.name?v->source_corresp.name:"";
  if(!builtin || !v->source_corresp.name_has_been_mangled || symbol.compare(0,4,"_ZTI") || symbol.size()==4)
    unsupported("unresolved runtime type_info definition");
  return symbol.substr(4)+string(1,'\0');
}
void ensure_typeinfo_helpers() {
  if(typeinfo_helpers_emitted)return;
  typeinfo_helpers_emitted=true;
  // A separate lazy holder avoids depending on application-global/literal
  // initializer order. Names and tokens remain rooted across every owner GC.
  string bytes;std::vector<unsigned> offsets(virtual_types.size());
  std::map<string,unsigned> name_offsets;
  // Walk by ID, not address-ordered map iteration, for deterministic output.
  std::vector<a_variable_ptr> vars(virtual_types.size());
  for(auto &entry:virtual_types)vars[entry.second]=entry.first;
  for(unsigned i=0;i<vars.size();++i) {
    string value=typeinfo_name_bytes(vars[i]);
    auto found=name_offsets.find(value);
    if(found==name_offsets.end()) {
      if(bytes.size()+value.size()>4*1024*1024)unsupported("RTTI names exceed 4 MiB");
      offsets[i]=(unsigned)bytes.size();name_offsets[value]=offsets[i];bytes+=value;
    } else offsets[i]=found->second;
  }
  string data,entries;
  for(char b:bytes)data+=std::to_string((unsigned char)b)+",";
  for(auto offset:offsets)entries+=std::to_string(offset)+",";
  helper_output+=R"CS(
// dot-gen:member-begin
private static class __edg_type_names {
  internal static readonly int[] Offsets = [)CS"+entries+R"CS(];
  internal static readonly byte[] Bytes = Make();
  private static byte[] Make() {
    global::System.ReadOnlySpan<byte> data = [)CS"+data+R"CS(];
    byte[] bytes=global::System.GC.AllocateArray<byte>(data.Length,pinned:true);
    data.CopyTo(bytes);return bytes;
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static sbyte* __edg_type_name(void* value) {
  long offset=(byte*)value-__edg_type_token_base;
  if(offset<0 || (offset & 7)!=0 || (ulong)(offset/8)>=(ulong)__edg_type_names.Offsets.Length)
    global::System.Environment.FailFast("Foreign EDG type_info token");
  return (sbyte*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref __edg_type_names.Bytes[__edg_type_names.Offsets[(int)(offset/8)]]);
}
// dot-gen:member-end
)CS";
}
