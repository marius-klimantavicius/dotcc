// Source-named ref views preserve the existing pinned/native arena layout.
// They do not own storage, create GC objects, or change initialization order.
std::set<a_variable_ptr> source_global_accessors,source_frame_aliases;
std::map<a_variable_ptr,string> source_incoming_parameters;
void reset_source_storage() {
  source_global_accessors.clear();source_frame_aliases.clear();source_incoming_parameters.clear();
}
string source_storage_address(a_variable_ptr variable_ptr) {
  auto local=local_offsets.find(variable_ptr);
  if(local!=local_offsets.end())return "(("+type(variable_ptr->type)+"*)(__edg_frame+"+std::to_string(local->second)+"))";
  auto tls=tls_offsets.find(variable_ptr);
  if(tls!=tls_offsets.end())return "(("+type(variable_ptr->type)+"*)("+string(emitting_tls_initializer?"p":"__edg_tls_base")+"+"+std::to_string(tls->second)+"))";
  auto global=global_offsets.find(variable_ptr);
  if(global!=global_offsets.end())return "(("+type(variable_ptr->type)+"*)(__edg_base+"+std::to_string(global->second)+"))";
  return "";
}
string source_variable_override(a_variable_ptr variable_ptr) {
  if(emitting_tls_initializer && tls_offsets.count(variable_ptr))return "";
  if(source_global_accessors.count(variable_ptr)) {
    return source_owner_reference(name(variable_ptr,"v"));
  }
  if(source_frame_aliases.count(variable_ptr))return name(variable_ptr,"v");
  return "";
}
void emit_source_global_accessors() {
  // Traverse the source registry, not pointer-ordered arena maps: declaration
  // order and suffix selection must be reproducible across compiler processes.
  for(const void *entity:source_name_order) {
    auto &entry=source_name_entries[entity];
    if(entry.kind!="global" && entry.kind!="static-local")continue;
    auto variable_ptr=(a_variable_ptr)entity;
    if(!global_offsets.count(variable_ptr) && !tls_offsets.count(variable_ptr))continue;
    string address=source_storage_address(variable_ptr);
    if(address.empty() || !source_global_accessors.insert(variable_ptr).second)continue;
    helper_output+="// dot-gen:member-begin\n"+source_member_marker(variable_ptr)+"\nprivate "+string(instance_mode?"":"static ")+"ref "+type(variable_ptr->type)+" "+name(variable_ptr,"v")+" => ref *"+address+";\n// dot-gen:member-end\n";
  }
}
void reset_source_frame_aliases(){source_frame_aliases.clear();source_incoming_parameters.clear();}
string source_parameter_name(a_variable_ptr parameter) {
  string result=name(parameter,"v");
  if(!local_offsets.count(parameter))return result;
  auto found=source_incoming_parameters.find(parameter);if(found!=source_incoming_parameters.end())return found->second;
  auto entry=source_name_entries.find(parameter);
  string domain=entry==source_name_entries.end()?"generated-parameters":entry->second.domain;
  result=source_allocate(domain,source_identifier_component(result)+"__incoming","parameter");
  source_incoming_parameters[parameter]=result;return result;
}
string source_frame_alias_for(a_variable_ptr variable_ptr) {
  if(!local_offsets.count(variable_ptr) || !source_name_entries.count(variable_ptr) || !source_frame_aliases.insert(variable_ptr).second)return "";
  return "ref "+type(variable_ptr->type)+" "+name(variable_ptr,"v")+" = ref *"+source_storage_address(variable_ptr)+";\n";
}
string source_frame_alias_declarations(a_scope_ptr scope) {
  if(!scope)return "";
  string result;
  if(scope->kind==sck_function)for(auto p=scope->variant.routine.parameters;p;p=p->next)result+=source_frame_alias_for(p);
  for(auto v=scope->nonstatic_variables;v;v=v->next)result+=source_frame_alias_for(v);
  for(auto child=scope->scopes;child;child=child->next)result+=source_frame_alias_declarations(child);
  return result;
}
