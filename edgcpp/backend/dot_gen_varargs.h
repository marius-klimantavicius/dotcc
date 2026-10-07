// Portable argument packs. The C backend emits va_* builtins; this backend
// consumes the same retained operators with an explicit managed calling ABI.
const string va_list_type="global::Edg.Runtime.VaList";
bool va_tag_type(a_type_ptr t) {
  t=skip_typerefs(t);
  return (t->kind==tk_struct || t->kind==tk_class) && class_type_supp(t)->is_va_list_tag;
}
bool variadic_routine(a_type_ptr t) {
  t=skip_typerefs(t);
  if(t->kind==tk_pointer)t=skip_typerefs(t->variant.pointer.type);
  return t->kind==tk_routine && t->variant.routine.extra_info->has_ellipsis;
}
string va_list_address(an_expr_node_ptr e) {
  auto t=skip_typerefs(e->type);
  return "("+va_list_type+"*)("+(t->kind==tk_pointer?expr(e):address(e))+")";
}
string va_operation(an_expr_node_ptr e) {
  auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;
  if(op==eok_va_start || op==eok_va_start_single_operand) {
    if(!current_function_scope || !variadic_routine(current_function_scope->variant.routine.ptr->type))
      unsupported("va_start outside variadic routine");
    return va_list_type+".Start("+va_list_address(a)+", __edg_arguments)";
  }
  if(op==eok_va_copy)return va_list_type+".Copy("+va_list_address(a)+", "+va_list_address(a->next)+")";
  if(op==eok_va_end)return va_list_type+".EndList("+va_list_address(a)+")";
  if(op==eok_va_arg) {
    auto t=skip_typerefs(e->type);auto alignment=alignment_of_type(t);
    if(!t->size || !alignment || alignment>4096 || (alignment&(alignment-1)))unsupported("va_arg layout");
    return "(*("+type(t)+"*)"+va_list_type+".Take("+va_list_address(a)+", "+std::to_string(t->size)+"UL, "+std::to_string(alignment)+"UL))";
  }
  return "";
}
string variadic_call(an_expr_node_ptr target,an_expr_node_ptr args) {
  auto rt=skip_typerefs(target->type);
  if(rt->kind==tk_pointer)rt=skip_typerefs(rt->variant.pointer.type);
  if(!variadic_routine(rt))return "";
  bool direct=target->kind==enk_routine;
  string call;
  if(direct) {
    auto routine=canonical_routine(target->variant.routine.ptr);
    if((routine->function_def_number==NULL_function_def_number || !scope_for_routine_or_null(routine)) &&
       !libc_routine(routine) && !thread_routine(routine) && !process_routine(routine) && !unbound_routine(routine))
      unsupported(("external variadic call: "+string(routine->source_corresp.name?routine->source_corresp.name:"<unnamed>")).c_str());
    // The same audited import can have several native declarations. Reuse its
    // canonical generated method just as routine_address does for callbacks.
    routine=libc_address_routine(routine);
    call=source_routine_reference(routine)+"(";
  } else call=expr(target)+"(";
  bool separator=!direct && instance_mode;
  if(separator)call+="this";
  auto parameter=rt->variant.routine.extra_info->param_type_list;
  string carriers;
  unsigned long long bytes=0;
  for(auto argument=args;argument;argument=argument->next) {
    if(parameter) {
      if(separator)call+=", ";
      call+=enum_boundary_value(parameter->type,argument->type,expr(argument));
      separator=true;parameter=parameter->next;
      continue;
    }
    // c_gen_be::dump_call consumes EDG's argument types after the front end's
    // default promotions. Do not infer promotions from C# literal syntax.
    auto t=skip_typerefs(argument->type);
    unsigned long long align=alignment_of_type(t);
    if(!t->size || !align || align>4096 || (align&(align-1)))unsupported("variadic argument layout");
    bytes=(bytes+align-1)&~(align-1);bytes+=t->size;
    if(bytes>64*1024*1024)unsupported("variadic pack exceeds 64 MiB");
    string value=expr(argument),carrier,ty=type(argument->type);
    string physical=source_enum_nominal(argument->type)?source_enum_physical_type(argument->type):ty;
    bool implicit_scalar=((physical=="int" || physical=="uint") && t->size==4 && align==4) ||
      ((physical=="long" || physical=="ulong" || physical=="double") && t->size==8 && align==8);
    if(t->kind==tk_pointer) {
      if(t->size!=8 || align!=8)unsupported("variadic pointer layout");
      carrier="((void*)("+value+"))";
    } else if(implicit_scalar) {
      // The carrier overload sees the same promoted scalar type as the native
      // ABI. Named enums need an explicit physical conversion in C#.
      carrier=physical==ty?value:"unchecked(("+physical+")("+value+"))";
    } else {
      value=enum_boundary_value(argument->type,argument->type,value);
      carrier="global::Edg.Runtime.VaArg.From<"+ty+">("+value+", "+
        std::to_string(t->size)+", "+std::to_string(align)+")";
    }
    // Factory calls snapshot each by-value operand at its evaluation point,
    // before any later operand can mutate the original aggregate storage.
    if(!carriers.empty())carriers+=", ";
    carriers+=carrier;
  }
  if(parameter)unsupported("missing fixed variadic arguments");
  if(direct) {
    if(!carriers.empty())call+=string(separator?", ":"")+carriers;
  } else {
    // Function-pointer signatures cannot carry a params modifier.
    call+=string(separator?", ":"")+"["+carriers+"]";
  }
  return call+")";
}
string variadic_entry_prologue() {
  return "int __edg_va_stack_bytes = global::Edg.Runtime.VaPack.GetStackBytes(__edg_varargs);\n"
    "byte* __edg_va_stack = stackalloc byte[__edg_va_stack_bytes];\n"
    "using var __edg_va_pack = new global::Edg.Runtime.VaPack(__edg_varargs, __edg_va_stack, __edg_va_stack_bytes);\n"
    "global::Edg.Runtime.VaList __edg_arguments_value = __edg_va_pack.Value;\n"
    "global::Edg.Runtime.VaList* __edg_arguments = &__edg_arguments_value;\n";
}
