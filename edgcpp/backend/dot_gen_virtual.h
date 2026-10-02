// Keep EDG's table layout and lowered managed calls. Native RTTI implementation
// objects are replaced with stable opaque tags, shared with typeid.
void register_rtti_type(a_type_ptr t) {
  if(!t->typeinfo_var)return;
  auto v=t->typeinfo_var;
  auto prefix=skip_typerefs(v->type);
  while(prefix->kind==tk_class || prefix->kind==tk_struct) {
    auto f=prefix->variant.class_struct_union.field_list;
    if(!f || f->offset)break;
    if(aggregate(f->type)) { prefix=skip_typerefs(f->type);continue; }
    if(prefix->size==16 && f->next && !f->next->next && f->next->offset==8 &&
       f->source_corresp.name && string(f->source_corresp.name)=="__vptr" &&
       f->next->source_corresp.name && string(f->next->source_corresp.name)=="__name")
      rtti_user_prefix_types.insert(prefix);
    break;
  }
  if(!virtual_types.count(v)) {
    if(virtual_types.size()>=65536)unsupported("RTTI token count");
    virtual_types[v]=(unsigned)virtual_types.size();
    // A lowered member-function pointer is physically a record, but its RTTI
    // describes the source member-pointer type, not a class-inheritance graph.
    rtti_source_types[v]=get_underlying_type(t);
  }
}
void collect_virtual_scope(a_scope_ptr scope,std::map<a_variable_ptr,unsigned> &types,
                           std::set<a_variable_ptr> &tables) {
  for(auto t=scope->types;t;t=t->next) {
    register_rtti_type(t);
    if(t->kind!=tk_class && t->kind!=tk_struct)continue;
    auto info=class_type_supp(t);
    if(info->virtual_function_table_var)tables.insert(info->virtual_function_table_var);
  }
  for(auto s=scope->scopes;s;s=s->next)collect_virtual_scope(s,types,tables);
}
void collect_virtual_token_constants(a_constant_ptr c,const std::map<a_variable_ptr,unsigned> &types) {
  if(!c)return;
  if(c->kind==ck_aggregate) {
    for(auto p=c->variant.aggregate.first_constant;p;p=p->next)collect_virtual_token_constants(p,types);
  } else if(c->kind==ck_address && c->variant.address.kind==abk_variable) {
    auto found=types.find(c->variant.address.variant.variable);
    if(found!=types.end()) {
      if(c->variant.address.offset || skip_typerefs(c->type)->kind!=tk_integer || c->type->size!=8)
        unsupported("virtual RTTI slot");
      virtual_type_tokens[c]=found->second;
      collect_exception_metadata_variable(found->first);
    }
  }
}
void collect_virtual_metadata(a_scope_ptr scope) {
  auto &types=virtual_types;
  std::set<a_variable_ptr> tables;
  collect_virtual_scope(scope,types,tables);
  // Construction vtable groups are distinct from a class's main table.
  for(auto v=scope->variables;v;v=v->next)
    if(variable_needed(v) && v->vtable_defined)tables.insert(v);
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_body_needed(r))continue;
    if(r->function_def_number==NULL_function_def_number)continue;
    auto s=scope_for_routine_or_null(r);
    if(s)collect_virtual_scope(s,types,tables);
  }
  for(auto t=il_header.nontag_types_used_in_exception_or_rtti;t;t=t->next)register_rtti_type(t);
  for(auto v:tables) {
    if(v->init_kind==initk_static)collect_virtual_token_constants(v->initializer.constant,types);
  }
  // Cast targets need not occur in a vtable (e.g. a nonpolymorphic sibling).
  // Keep their native RTTI graphs out of application storage too. Casts
  // consume graph IDs; public type_info references consume opaque tokens.
  for(auto &entry:types)collect_exception_metadata_variable(entry.first);
  if(!types.empty()) {
    helper_output+="// dot-gen:member-begin\nprivate static class __edg_type_tokens {\n"
      "internal static readonly byte[] Storage = global::System.GC.AllocateArray<byte>("+
      std::to_string(types.size()*8)+", pinned: true); }\n// dot-gen:member-end\n"
      "private static byte* __edg_type_token_base => (byte*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref __edg_type_tokens.Storage[0]);\n";
  }
}
