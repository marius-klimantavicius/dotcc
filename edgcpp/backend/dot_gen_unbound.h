// Explicit external contracts: throwing review declarations or required C#
// partial implementations. Normal translation rejects symbols without bindings.
bool unbound_declarations=false;
bool unbound_partial=false;
std::set<a_routine_ptr> unbound_routines;
std::set<a_variable_ptr> unbound_variables;
std::map<const void*,string> unbound_runtime_names;
string unbound_runtime_contracts,unbound_local_runtime_contracts;
bool unbound_runtime_field_emitted=false;

void reset_unbound_runtime() {
  unbound_runtime_names.clear();unbound_runtime_contracts.clear();unbound_local_runtime_contracts.clear();unbound_runtime_field_emitted=false;
}
string unbound_runtime_name(const void *entity,const string &candidate,const string &kind) {
  auto found=unbound_runtime_names.find(entity);
  if(found!=unbound_runtime_names.end())return found->second;
  return unbound_runtime_names[entity]=source_allocate("external-runtime",candidate,kind);
}
void initialize_unbound_runtime_names(a_scope_ptr scope) {
  if(!unbound_partial)return;
  // The provider identity must not depend on the translated owner's name or
  // its member collisions. Allocate imports in a separate deterministic domain.
  for(const char *reserved:{"Runtime","Owner","get_Owner","Equals","GetHashCode","GetType","ToString","Finalize","MemberwiseClone","ReferenceEquals"})
    source_name_used["external-runtime"].insert(reserved);
  std::vector<std::pair<string,a_routine_ptr>> routines;
  for(auto routine=scope->routines;routine;routine=routine->next) {
    if(!routine_needed(routine) || routine->function_def_number!=NULL_function_def_number)continue;
    string spelling=source_routine_spelling(routine);
    if(spelling.empty())continue;
    string candidate=source_identifier(spelling);
    source_name_reserved["external-runtime"].insert(candidate);
    routines.push_back({routine->source_corresp.name?routine->source_corresp.name:spelling,routine});
  }
  std::stable_sort(routines.begin(),routines.end(),[](const std::pair<string,a_routine_ptr> &left,const std::pair<string,a_routine_ptr> &right){return left.first<right.first;});
  for(auto &entry:routines)unbound_runtime_name(entry.second,source_identifier(source_routine_spelling(entry.second)),"external");
  for(const void *entity:source_name_order) {
    auto &entry=source_name_entries[entity];
    if(entry.kind!="global")continue;
    string candidate="__edg_unbound_address_"+source_identifier(entry.source);
    source_name_reserved["external-runtime"].insert(candidate);
  }
  for(const void *entity:source_name_order) {
    auto &entry=source_name_entries[entity];
    if(entry.kind=="global")unbound_runtime_name(entity,"__edg_unbound_address_"+source_identifier(entry.source),"storage");
  }
}
void ensure_unbound_runtime_field() {
  if(unbound_runtime_field_emitted)return;
  unbound_runtime_field_emitted=true;
  helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+
    "readonly global::Managed.Runtime __edg_runtime"+string(instance_mode?";":" = new global::Managed.Runtime();")+"\n// dot-gen:member-end\n";
}
string unbound_contract_type(a_type_ptr original) {
  // First use the authoritative renderer to validate and materialize its ABI
  // types. Then qualify references for a different, fixed namespace/class.
  string rendered=type(original);
  auto t=skip_typerefs(original);
  if(t->kind==tk_pointer) {
    auto pointee=skip_typerefs(t->variant.pointer.type);
    if(pointee->kind==tk_routine) {
      auto extra=pointee->variant.routine.extra_info;
      string result="delegate* managed<";
      if(instance_mode)result+=source_qualified_owner()+", ";
      for(auto parameter=extra->param_type_list;parameter;parameter=parameter->next)
        result+=unbound_contract_type(parameter->type)+", ";
      if(extra->has_ellipsis)result+="global::Edg.Runtime.VaList*, ";
      return result+unbound_contract_type(pointee->variant.routine.return_type)+">";
    }
    return unbound_contract_type(t->variant.pointer.type)+"*";
  }
  // Runtime ABI types (va_list, binary80, binary128, etc.) are already qualified.
  if(rendered.compare(0,8,"global::")==0)return rendered;
  if(aggregate(t) || source_enum_nominal(t)) {
    if(nested_types)return source_qualified_owner()+"."+rendered;
    const char *ns=getenv("EDG_DOT_NAMESPACE");
    return "global::"+string(ns&&*ns?string(ns)+".":"")+rendered;
  }
  return rendered;
}
bool unbound_project_type(a_type_ptr original) {
  // Classify the actual managed representation, not a native tag spelling or
  // typedef. Pinned runtime ABI records such as va_list are shared even though
  // their native IL is aggregate; generated records/enums remain project-local.
  string rendered=type(original);
  auto t=skip_typerefs(original);
  if(t->kind==tk_pointer) {
    auto pointee=skip_typerefs(t->variant.pointer.type);
    if(pointee->kind==tk_routine) {
      // Instance callbacks carry the translated owner as their hidden first
      // argument. Keep that implementation on the owner so `this` is available.
      if(instance_mode)return true;
      auto extra=pointee->variant.routine.extra_info;
      if(unbound_project_type(pointee->variant.routine.return_type))return true;
      for(auto parameter=extra->param_type_list;parameter;parameter=parameter->next)
        if(unbound_project_type(parameter->type))return true;
      return false;
    }
    return unbound_project_type(t->variant.pointer.type);
  }
  if(rendered.compare(0,8,"global::")==0)return false;
  return aggregate(t) || source_enum_nominal(t);
}

void emit_unbound_runtime_contracts() {
  if(!unbound_partial)return;
  output+="// dot-gen:runtime-begin\nnamespace Managed;\npublic unsafe partial class Runtime {\n";
  output+="internal object? Owner { get; }\ninternal Runtime(object? owner = null) { Owner = owner; }\n";
  output+=unbound_runtime_contracts;
  output+="}\n// dot-gen:runtime-end\n";
  if(!unbound_local_runtime_contracts.empty()) {
    output+="// dot-gen:local-runtime-begin\n";
    const char *ns=getenv("EDG_DOT_NAMESPACE"),*owner=getenv("EDG_DOT_CLASS");
    if(ns&&*ns)output+="namespace "+string(ns)+";\n";
    output+="public "+string(instance_mode?"sealed":"static")+" unsafe partial class "+string(owner)+" {\n";
    output+=unbound_local_runtime_contracts;
    output+="}\n// dot-gen:local-runtime-end\n";
  }
}

bool unbound_routine(a_routine_ptr routine) {
  if(!unbound_declarations)return false;
  if(!unbound_routines.insert(routine).second)return true;
  source_register(routine,&routine->source_corresp,"function",
    source_scope_context(&routine->source_corresp),"owner",source_routine_spelling(routine));
  auto rt=skip_typerefs(routine->type);
  bool local_contract=unbound_partial && unbound_project_type(rt->variant.routine.return_type);
  if(unbound_partial)for(auto parameter=rt->variant.routine.extra_info->param_type_list;parameter;parameter=parameter->next)
    local_contract|=unbound_project_type(parameter->type);
  string id=name(routine,"f"),parameters,contract_parameters,arguments;
  unsigned index=0;
  for(auto p=rt->variant.routine.extra_info->param_type_list;p;p=p->next) {
    if(!parameters.empty()){parameters+=", ";contract_parameters+=", ";arguments+=", ";}
    string argument="arg"+std::to_string(index++);
    parameters+=type(p->type)+" "+argument;
    if(unbound_partial && !local_contract)contract_parameters+=unbound_contract_type(p->type)+" "+argument;
    arguments+=argument;
  }
  if(rt->variant.routine.extra_info->has_ellipsis) {
    string separator=parameters.empty()?"":", ";
    parameters+=separator+"global::Edg.Runtime.VaList* __edg_arguments";
    contract_parameters+=separator+"global::Edg.Runtime.VaList* __edg_arguments";
    arguments+=separator+"__edg_arguments";
  }
  string symbol=routine->source_corresp.name?routine->source_corresp.name:"<unnamed>";
  string return_type=type(rt->variant.routine.return_type);
  if(local_contract) {
    helper_output+="// dot-gen:unbound-local function "+source_hex(symbol)+" "+id+" "+id+"\n";
    unbound_local_runtime_contracts+="// dot-gen:runtime-origin "+id+"\n"+
      "// Project-specific signature; implement in another translated-owner partial file.\n"+
      "private "+string(instance_mode?"":"static ")+"partial "+return_type+" "+id+"("+parameters+");\n";
  } else if(unbound_partial) {
    ensure_unbound_runtime_field();
    string spelling=source_routine_spelling(routine);
    string runtime_id=unbound_runtime_name(routine,source_identifier(spelling.empty()?symbol:spelling),"external");
    helper_output+="// dot-gen:unbound function "+source_hex(symbol)+" "+id+" "+runtime_id+"\n";
    unbound_runtime_contracts+="// dot-gen:runtime-origin "+id+"\n"+
      "// External function; implementation required in another Managed.Runtime partial file.\n"+
      "internal partial "+unbound_contract_type(rt->variant.routine.return_type)+" "+runtime_id+"("+contract_parameters+");\n";
    helper_output+="// dot-gen:member-begin\n"+source_member_marker(routine)+"\nprivate "+string(instance_mode?"":"static ")+return_type+" "+id+
      "("+parameters+") => __edg_runtime."+runtime_id+"("+arguments+");\n// dot-gen:member-end\n";
  } else {
    helper_output+="// dot-gen:unbound function "+source_hex(symbol)+" "+id+"\n";
    helper_output+="// dot-gen:member-begin\n"+source_member_marker(routine)+
      "\n// Unbound external declaration; review-only output.\nprivate "+string(instance_mode?"":"static ")+return_type+" "+id+"("+parameters+
      ") { throw new global::System.PlatformNotSupportedException(\"Unbound external function in review-only translation: "+source_hex(symbol)+" (UTF-8 hex)\"); }\n// dot-gen:member-end\n";
  }
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
    if(unbound_partial && unbound_project_type(variable_ptr->type)) {
      helper_output+="// dot-gen:unbound-local variable "+source_hex(symbol)+" "+id+" "+getter+"\n";
      unbound_local_runtime_contracts+="// dot-gen:runtime-origin "+id+"\n"+
        "// Project-specific external storage; supply a stable address with source lifetime and alignment.\n"+
        "private "+string(instance_mode?"":"static ")+"partial "+ty+"* "+getter+"();\n";
    } else if(unbound_partial) {
      ensure_unbound_runtime_field();
      string runtime_getter=unbound_runtime_name(variable_ptr,"__edg_unbound_address_"+source_identifier(source_spelling(&variable_ptr->source_corresp)),"storage");
      helper_output+="// dot-gen:unbound variable "+source_hex(symbol)+" "+id+" "+runtime_getter+"\n";
      unbound_runtime_contracts+="// dot-gen:runtime-origin "+id+"\n"+
        "// External storage; supply a stable address with the source lifetime and alignment.\n"+
        "internal partial "+unbound_contract_type(variable_ptr->type)+"* "+runtime_getter+"();\n";
      helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+ty+"* "+getter+"() => __edg_runtime."+runtime_getter+"();\n// dot-gen:member-end\n";
    } else {
      helper_output+="// dot-gen:unbound variable "+source_hex(symbol)+" "+id+"\n";
      helper_output+="// dot-gen:member-begin\n// Unbound external declaration; review-only output.\n"+
        string("private ")+string(instance_mode?"":"static ")+ty+"* "+getter+"() { throw new global::System.PlatformNotSupportedException(\"Unbound external variable in review-only translation: "+source_hex(symbol)+" (UTF-8 hex)\"); }\n// dot-gen:member-end\n";
    }
    helper_output+="// dot-gen:member-begin\n"+source_member_marker(variable_ptr)+"\nprivate "+string(instance_mode?"":"static ")+"ref "+ty+" "+id+" => ref *"+getter+"();\n// dot-gen:member-end\n";
  }
  return source_owner_reference(getter)+"()";
}
