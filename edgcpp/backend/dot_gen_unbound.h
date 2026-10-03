// Explicit external contracts: throwing review declarations or required C#
// partial implementations. Normal translation rejects symbols without bindings.
bool unbound_declarations=false;
bool unbound_partial=false;
std::set<a_routine_ptr> unbound_routines;
std::set<a_variable_ptr> unbound_variables;

bool unbound_routine(a_routine_ptr routine) {
  if(!unbound_declarations)return false;
  if(!unbound_routines.insert(routine).second)return true;
  source_register(routine,&routine->source_corresp,"function",
    source_scope_context(&routine->source_corresp),"owner",source_routine_spelling(routine));
  auto rt=skip_typerefs(routine->type);
  string id=name(routine,"f"),parameters;
  unsigned index=0;
  for(auto p=rt->variant.routine.extra_info->param_type_list;p;p=p->next) {
    if(!parameters.empty())parameters+=", ";
    parameters+=type(p->type)+" arg"+std::to_string(index++);
  }
  if(rt->variant.routine.extra_info->has_ellipsis)
    parameters+=string(parameters.empty()?"":", ")+"global::Edg.Runtime.VaList* __edg_arguments";
  string symbol=routine->source_corresp.name?routine->source_corresp.name:"<unnamed>";
  helper_output+="// dot-gen:unbound function "+source_hex(symbol)+" "+id+"\n";
  helper_output+="// dot-gen:member-begin\n"+source_member_marker(routine)+
    (unbound_partial?"\n// External function; implementation required in another partial class file.\n":"\n// Unbound external declaration; review-only output.\n")+
    "private "+string(instance_mode?"":"static ")+(unbound_partial?"partial ":"")+
    type(rt->variant.routine.return_type)+" "+id+"("+parameters+
    (unbound_partial?");\n":") { throw new global::System.PlatformNotSupportedException(\"Unbound external function in review-only translation: "+source_hex(symbol)+" (UTF-8 hex)\"); }\n")+
    "// dot-gen:member-end\n";
  return true;
}

string unbound_variable_address(a_variable_ptr variable_ptr) {
  if(!unbound_declarations)return "";
  source_register(variable_ptr,&variable_ptr->source_corresp,"global",
    source_scope_context(&variable_ptr->source_corresp),"owner",source_spelling(&variable_ptr->source_corresp));
  auto entry=source_name_entries.find(variable_ptr);
  if(entry!=source_name_entries.end())entry->second.has_getter=true;
  string id=name(variable_ptr,"v"),getter="__edg_unbound_address_"+source_identifier_component(id);
  if(unbound_variables.insert(variable_ptr).second) {
    string ty=type(variable_ptr->type);
    string symbol=variable_ptr->source_corresp.name?variable_ptr->source_corresp.name:"<unnamed>";
    helper_output+="// dot-gen:unbound variable "+source_hex(symbol)+" "+id+"\n";
    helper_output+="// dot-gen:member-begin\n"+
      string(unbound_partial?"// External storage; implementation must supply a stable address with the source lifetime and alignment.\n":"// Unbound external declaration; review-only output.\n")+
      "private "+string(instance_mode?"":"static ")+(unbound_partial?"partial ":"")+ty+"* "+getter+"()"+
      (unbound_partial?";\n":" { throw new global::System.PlatformNotSupportedException(\"Unbound external variable in review-only translation: "+source_hex(symbol)+" (UTF-8 hex)\"); }\n")+
      "// dot-gen:member-end\n";
    helper_output+="// dot-gen:member-begin\n"+source_member_marker(variable_ptr)+"\nprivate "+string(instance_mode?"":"static ")+"ref "+ty+" "+id+" => ref *"+getter+"();\n// dot-gen:member-end\n";
  }
  return source_owner_reference(getter)+"()";
}
