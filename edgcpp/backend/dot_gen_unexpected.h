// Legacy dynamic exception specifications. Preserve the distinction made in
// lower_eh.c: noexcept is an unwind destination; throw(T...) selects unexpected
// only if the original type is outside the permitted list.
bool unexpected_helpers_emitted=false;
a_routine_ptr bad_exception_factory=nullptr;
a_type_ptr bad_exception_type=nullptr;
a_type_ptr dynamic_exception_type(a_type_ptr t) {
  auto plain=skip_typerefs(t);
  if(plain->kind==tk_pointer && plain->variant.pointer.is_reference)t=plain->variant.pointer.type;
  return t;
}
bool dynamic_exception_spec(a_routine_ptr r) {
  auto spec=skip_typerefs(r->type)->variant.routine.extra_info->exception_specification;
  return spec && !spec->throw_any && !spec->is_noexcept;
}
void collect_dynamic_specification(a_routine_ptr r) {
  if(r->source_corresp.name && string(r->source_corresp.name)=="__edg_cxx_throw_bad_exception" &&
     r->function_def_number!=NULL_function_def_number) {
    auto rt=skip_typerefs(r->type);auto p=rt->variant.routine.extra_info->param_type_list;
    if(!p || p->next || skip_typerefs(p->type)->kind!=tk_pointer ||
       skip_typerefs(rt->variant.routine.return_type)->kind!=tk_void)
      unsupported("bad_exception provider ABI");
    bad_exception_factory=r;bad_exception_type=skip_typerefs(p->type)->variant.pointer.type;
    (void)exception_key(bad_exception_type);collect_exception_typeinfo(bad_exception_type);
  }
  if(!dynamic_exception_spec(r))return;
  auto spec=skip_typerefs(r->type)->variant.routine.extra_info->exception_specification;
  if(spec->indeterminate || spec->arg_cached || spec->copy_from_prototype)
    unsupported("unresolved dynamic exception specification");
  for(auto p=spec->variant.exception_specification_type_list;p;p=p->next) {
    if(p->is_pack_expansion)unsupported("unexpanded dynamic exception specification");
    (void)exception_key(dynamic_exception_type(p->type));collect_exception_typeinfo(dynamic_exception_type(p->type));
  }
}
void ensure_unexpected_helpers() {
  if(unexpected_helpers_emitted)return;
  unexpected_helpers_emitted=true;ensure_terminate_helpers();
  string mod=instance_mode?"":"static ";
  string callback="delegate* managed<"+string(instance_mode?string(getenv("EDG_DOT_CLASS"))+", ":"")+"void>";
  string invoke="(("+callback+")handler)("+string(instance_mode?"this":"")+");";
  helper_output+="private "+mod+"nint __edg_unexpected_handler;\n";
  helper_output+="// dot-gen:member-begin\nprivate "+mod+"nint __edg_set_unexpected(nint handler) {return global::System.Threading.Interlocked.Exchange(ref __edg_unexpected_handler,handler);}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\n[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]\nprivate "+mod+"void __edg_unexpected() {\n"
      "nint handler=global::System.Threading.Interlocked.CompareExchange(ref __edg_unexpected_handler,0,0);\n"
      "if(handler!=0) {"+invoke+"}\n__edg_terminate();\n}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate "+mod+"bool __edg_unexpected_cleanup_failure(global::System.Exception error) {\n"+
      "if(!("+control_transfer_exclusion("error")+"))return false;\n"+
      "__edg_terminate_runtime(error,\"Destructor threw during unexpected replacement\");return false;\n}\n// dot-gen:member-end\n";
}
string unexpected_call(a_routine_ptr r,an_expr_node_ptr args) {
  if(r->function_def_number!=NULL_function_def_number)return "";
  string source=r->source_corresp.name?r->source_corresp.name:"";
  if(source!="__edg_cxx_set_unexpected" && source!="__edg_cxx_get_unexpected" &&
     source!="__edg_cxx_unexpected" && source!="__edg_cxx_unexpected_default")return "";
  ensure_unexpected_helpers();
  if(source=="__edg_cxx_set_unexpected") {
    if(!args || args->next)unsupported("set_unexpected ABI");
    return "("+type(skip_typerefs(r->type)->variant.routine.return_type)+")__edg_set_unexpected((nint)("+expr(args)+"))";
  }
  if(args)unsupported("unexpected no-argument ABI");
  if(source=="__edg_cxx_get_unexpected")return "("+type(skip_typerefs(r->type)->variant.routine.return_type)+")global::System.Threading.Interlocked.CompareExchange(ref __edg_unexpected_handler,0,0)";
  return source=="__edg_cxx_unexpected_default"?"__edg_terminate()":"__edg_unexpected()";
}
string dynamic_exception_match(an_exception_specification_ptr spec,const string &error) {
  string matches;
  for(auto p=spec->variant.exception_specification_type_list;p;p=p->next) {
    if(!matches.empty())matches+=" || ";
    matches+=exception_match("((__edg_exception)"+error+").Type",dynamic_exception_type(p->type));
  }
  return matches.empty()?"false":"("+error+" is __edg_exception && ("+matches+"))";
}
string dynamic_exception_boundary(a_routine_ptr r) {
  ensure_unexpected_helpers();
  auto spec=skip_typerefs(r->type)->variant.routine.extra_info->exception_specification;
  string error="__edg_spec_error",replacement="__edg_spec_replacement";
  bool bad_allowed=false;
  if(bad_exception_type)for(auto p=spec->variant.exception_specification_type_list;p;p=p->next)
    if(exception_adjustment(exception_descriptor(bad_exception_type),exception_descriptor(dynamic_exception_type(p->type)))!=-1)bad_allowed=true;
  string code="} catch(global::System.Exception "+error+") when (__edg_begin_exception("+error+") && !("+dynamic_exception_match(spec,error)+")) {\n"
      "var __edg_spec_frame=__edg_create_catch_frame("+error+",0,0,false);__edg_spec_frame.MarkCaught();\n"
      "try {\ntry {__edg_unexpected();throw new global::System.InvalidProgramException(\"unexpected returned\");}\ncatch(global::System.Exception "+replacement+") when (__edg_begin_exception("+replacement+")) {\n"
      "var __edg_replacement_frame=__edg_create_catch_frame("+replacement+",0,0,false);__edg_replacement_frame.MarkCaught();\ntry {\n"
      "if("+dynamic_exception_match(spec,replacement)+")__edg_rethrow();\n";
  if(bad_allowed)code+=name(bad_exception_factory,"f")+"(null);\n";
  code+="__edg_terminate_runtime("+replacement+",\"Unexpected handler threw a forbidden exception\");throw;\n"
      "} finally {try {__edg_replacement_frame.Leave();}catch(global::System.Exception __edg_unexpected_cleanup_error) when(__edg_unexpected_cleanup_failure(__edg_unexpected_cleanup_error)){throw;}}\n"
      "}\n} finally {try {__edg_spec_frame.Leave();}catch(global::System.Exception __edg_unexpected_cleanup_error) when(__edg_unexpected_cleanup_failure(__edg_unexpected_cleanup_error)){throw;}}\n}\n";
  return code;
}
