// Use EDG's supported VLA lowering and its exact runtime entry signatures.
bool function_vla_used=false;
string vla_call(a_routine_ptr r,an_expr_node_ptr args) {
  if(!r || r->function_def_number!=NULL_function_def_number || !r->compiler_generated || !r->source_corresp.name)return "";
  string source=r->source_corresp.name;
  if(source!="__vla_alloc" && source!="__vla_dealloc" && source!="__vla_dealloc_eh")return "";
  auto rt=skip_typerefs(r->type);auto p=rt->variant.routine.extra_info->param_type_list;
  bool allocation=source=="__vla_alloc";
  if(type(rt->variant.routine.return_type)!="void" || !p || type(p->type)!="void*" ||
     rt->variant.routine.extra_info->has_ellipsis ||
     (allocation?(!p->next || p->next->next || type(p->next->type)!="long"):p->next!=nullptr) ||
     !args || (allocation?(!args->next || args->next->next):args->next!=nullptr))
    unsupported("lowered VLA runtime ABI");
  if(allocation) {
    if(!current_function_scope)unsupported("VLA outside function activation");
    function_vla_used=true;
    unsigned long long alignment=16;
    auto slot=args;
    while(slot && slot->kind==enk_operation && (slot->variant.operation.kind==eok_cast || slot->variant.operation.kind==eok_address_of))
      slot=slot->variant.operation.operands;
    if(slot && slot->kind==enk_variable)alignment=std::max(alignment,(unsigned long long)node_variable(slot)->alignment);
    if(alignment>4096 || (alignment&(alignment-1)))unsupported("VLA alignment");
    return "global::Edg.Runtime.Vla.Allocate("+expr(args)+", "+expr(args->next)+", "+std::to_string(alignment)+"U)";
  }
  return "global::Edg.Runtime.Vla.Release("+string(source=="__vla_dealloc"?"*(void**)(":"(")+expr(args)+"))";
}
