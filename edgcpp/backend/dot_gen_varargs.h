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
  string id="__edg_varcall"+std::to_string(++serial),parameters=function_pointer_type(rt)+" callback";
  string invoke="callback("+string(instance_mode?"this, ":""),call=id+"("+expr(target);
  auto parameter=rt->variant.routine.extra_info->param_type_list;
  unsigned index=0;unsigned long long bytes=0,alignment=8;
  string writes;
  for(auto a=args;a;a=a->next) {
    string arg="a"+std::to_string(index++),ty=type(a->type);
    parameters+=", "+ty+" "+arg;call+=", "+expr(a);
    if(parameter) {invoke+=arg+", ";parameter=parameter->next;}
    else {
      auto t=skip_typerefs(a->type);unsigned long long align=alignment_of_type(t);
      if(!t->size || !align || align>4096 || (align&(align-1)))unsupported("variadic argument layout");
      alignment=std::max(alignment,align);bytes=(bytes+align-1)&~(align-1);
      writes+="*("+ty+"*)(data+"+std::to_string(bytes)+") = "+arg+";\n";bytes+=t->size;
      if(bytes>64*1024*1024)unsupported("variadic pack exceeds 64 MiB");
    }
  }
  if(parameter)unsupported("missing fixed variadic arguments");
  bool heap=bytes+alignment>65536;
  string result=type(rt->variant.routine.return_type);
  string body="byte* raw="+(heap?"(byte*)global::System.Runtime.InteropServices.NativeMemory.Alloc("+std::to_string(bytes+alignment)+")":"stackalloc byte["+std::to_string(bytes+alignment)+"]")+";\n";
  if(heap)body+="if(raw==null)throw new global::System.OutOfMemoryException();\ntry {\n";
  body+="byte* data=(byte*)(((nuint)raw+"+std::to_string(alignment-1)+") & ~(nuint)"+std::to_string(alignment-1)+");\n"+writes;
  body+=va_list_type+" pack=new "+va_list_type+" { Begin=data, Next=data, End=data+"+std::to_string(bytes)+" };\n";
  body+=(result=="void"?"":"return ")+invoke+"&pack);\n";
  if(heap)body+="} finally {global::System.Runtime.InteropServices.NativeMemory.Free(raw);}\n";
  helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+id+"("+parameters+") {\n"+body+"}\n// dot-gen:member-end\n";
  return call+")";
}
