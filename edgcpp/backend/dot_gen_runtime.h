// The optional bootstrap is C++ code, so EDG supplies its object and EH IL.
// Raw allocation/free, RTTI names and compiler throw entries need bridges.
string runtime_provider_call(a_routine_ptr r,an_expr_node_ptr args) {
  if(r->function_def_number!=NULL_function_def_number)return "";
  string source=r->source_corresp.name?r->source_corresp.name:"";
  auto rt=skip_typerefs(r->type);auto params=rt->variant.routine.extra_info->param_type_list;
  if(r->compiler_generated && (source=="__cxa_bad_cast" || source=="__cxa_bad_typeid" ||
      source=="__cxa_throw_bad_array_new_length")) {
    // lower_il.c creates prototyped ABI calls after source declarations. The C
    // backend leaves their external symbols for its linker; bind the exact
    // translated C-linkage definition here before considering the bootstrap.
    if(args || params || rt->variant.routine.extra_info->has_ellipsis ||
       skip_typerefs(rt->variant.routine.return_type)->kind!=tk_void)
      unsupported("compiler throw-entry ABI");
    a_routine_ptr definition=nullptr;
    for(auto p=il_header.primary_scope->routines;p;p=p->next) {
      if(!p->source_corresp.name || string(p->source_corresp.name)!=source ||
         p->source_corresp.name_linkage!=nlk_external ||
         p->function_def_number==NULL_function_def_number || !scope_for_routine_or_null(p))continue;
      auto pt=skip_typerefs(p->type);
      if(pt->variant.routine.extra_info->param_type_list || pt->variant.routine.extra_info->has_ellipsis ||
         skip_typerefs(pt->variant.routine.return_type)->kind!=tk_void || !routine_body_needed(p))
        unsupported("translated compiler throw-entry ABI or unavailable body");
      if(definition && definition!=p)unsupported("multiple translated compiler throw-entry definitions");
      definition=p;
    }
    if(definition)return source_routine_reference(definition)+"()";
  }
  if(!runtime_exceptions)return "";
  if((source=="__cxa_bad_cast" || source=="__cxa_bad_typeid" || source=="__cxa_throw_bad_array_new_length") && r->compiler_generated) {
    string provider=source=="__cxa_bad_cast"?"__edg_dot_throw_bad_cast":source=="__cxa_bad_typeid"?"__edg_dot_throw_bad_typeid":"__edg_dot_throw_bad_array_new_length";
    if(args || params || type(rt->variant.routine.return_type)!="void")unsupported("RTTI throw runtime ABI");
    for(auto p=il_header.primary_scope->routines;p;p=p->next) {
      if(p->source_corresp.name && string(p->source_corresp.name)==provider &&
         p->function_def_number!=NULL_function_def_number) {
        auto pt=skip_typerefs(p->type);
        if(pt->variant.routine.extra_info->param_type_list || type(pt->variant.routine.return_type)!="void")
          unsupported("RTTI throw provider ABI");
        return source_routine_reference(p)+"()";
      }
    }
    unsupported("missing C++ RTTI throw provider");
  }
  if(source=="__edg_dot_type_name") {
    if(!params || params->next || !args || args->next || type(params->type)!="void*" ||
       type(rt->variant.routine.return_type)!="sbyte*")unsupported("type_info name provider ABI");
    ensure_typeinfo_helpers();
    return "__edg_type_name("+expr(args)+")";
  }
  if(source=="__edg_dot_set_handler" || source=="__edg_dot_get_handler") {
    ensure_lifecycle();
    string value=source=="__edg_dot_set_handler"?
      "global::System.Threading.Interlocked.Exchange(ref __edg_new_handler, (nint)("+expr(args)+"))":
      "global::System.Threading.Volatile.Read(ref __edg_new_handler)";
    return "(("+type(rt->variant.routine.return_type)+")("+value+"))";
  }
  if(source!="__edg_dot_allocate" && source!="__edg_dot_allocate_aligned" && source!="__edg_dot_free")return "";
  ensure_lifecycle();
  if(source=="__edg_dot_free")return "__edg_heap_owner.Free("+expr(args)+")";
  bool aligned=source=="__edg_dot_allocate_aligned";
  if(!params || !args || type(params->type)!="ulong" ||
     type(rt->variant.routine.return_type)!="void*" ||
     (aligned?(!params->next || params->next->next || !args->next || args->next->next):(params->next || args->next)))
    unsupported("bootstrap allocation ABI");
  return "__edg_heap.Allocate("+expr(args)+", "+(aligned?expr(args->next):"16UL")+")";
}
