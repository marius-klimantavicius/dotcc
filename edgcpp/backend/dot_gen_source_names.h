// Source spelling is presentation, never semantic identity. Every registry is
// keyed by its original IL entity; collisions allocate distinct C# identifiers.
struct source_name_entry {
  string kind,source,scope,domain,candidate,managed;
  a_source_position position{};
  bool has_getter=false,has_setter=false;
};
std::map<const void*,source_name_entry> source_name_entries;
std::vector<const void*> source_name_order;
std::map<string,std::set<string>> source_name_used,source_name_reserved,source_local_identifiers;
std::map<a_routine_ptr,string> source_export_names,source_function_domains;
bool source_force_qualified_references=false;
std::map<a_type_ptr,string> source_record_aliases,source_alias_contexts,source_member_domains;
std::map<const a_source_correspondence*,string> source_context_cache;
std::set<a_type_ptr> source_name_types,source_enum_emitted;
std::set<a_scope_ptr> source_name_scopes;
std::map<a_constant_ptr,a_type_ptr> source_enumerator_types;
std::set<const void*> source_mapping_written;
std::set<a_type_ptr> source_type_mapping_written;
bool source_names_initialized=false,source_library_names=false,source_enum_literal=false;
string source_mapping_output;
string constant(a_constant_ptr);

string source_hex(const string &value) {
  const char *digits="0123456789abcdef";string result;
  for(unsigned char c:value){result+=digits[c>>4];result+=digits[c&15];}
  return result.empty()?"-":result;
}
string source_identifier_component(string value) {
  if(!value.empty() && value[0]=='@')value.erase(0,1);
  return value;
}
string source_escape_identifier(const string &value,const string &kind) {
  // Contextual words such as value/get/set remain ordinary local/member names.
  // Only reserved tokens and ambiguous type declarations need escaping.
  static const std::set<string> keywords={
    "abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue","decimal","default","delegate","do","double","else","enum","event","explicit","extern","false","finally","fixed","float","for","foreach","goto","if","implicit","in","int","interface","internal","is","lock","long","namespace","new","null","object","operator","out","override","params","private","protected","public","readonly","ref","return","sbyte","sealed","short","sizeof","stackalloc","static","string","struct","switch","this","throw","true","try","typeof","uint","ulong","unchecked","unsafe","ushort","using","virtual","void","volatile","while",
    "__arglist","__makeref","__reftype","__refvalue"};
  static const std::set<string> type_keywords={"var","dynamic","nint","nuint","record","file","required","scoped","unmanaged","notnull"};
  return keywords.count(value)||((kind=="type"||kind=="enum")&&type_keywords.count(value))?"@"+value:value;
}
string source_identifier(const string &raw) {
  string result;const char *digits="0123456789abcdef";
  for(unsigned char c:raw) {
    bool alpha=(c>='a'&&c<='z')||(c>='A'&&c<='Z')||c=='_';
    if(alpha || (c>='0'&&c<='9'))result+=char(c);
    else {result+="_u";result+=digits[c>>4];result+=digits[c&15];result+='_';}
  }
  if(result.empty())result="anonymous";
  if(result=="_")result="source_"; // C# discard expressions must not bind source storage.
  if(result[0]>='0'&&result[0]<='9')result="source_"+result;
  // This namespace belongs to runtime adapters and local lowering temporaries.
  if(result.compare(0,6,"__edg_")==0)result="source"+result;
  // Legacy anonymous-entity IDs remain reserved until every lowering helper
  // uses the __edg_ namespace; a user f123 must never alias generated f123.
  if(result.size()>1 && (result[0]=='f'||result[0]=='m'||result[0]=='v')) {
    bool numbered=true;for(size_t i=1;i<result.size();++i)if(result[i]<'0'||result[i]>'9')numbered=false;
    if(numbered)result="source_"+result;
  }
  return result;
}
string source_spelling(a_source_correspondence *source) {
  if(!source || !source->name)return "";
#if NEED_NAME_MANGLING
  if(source->unnamed_entity_given_fabricated_name)return "";
  if(source->name_has_been_mangled && source->unmangled_name_or_mangled_encoding)
    return source->unmangled_name_or_mangled_encoding;
#endif
  return source->name;
}
string source_decoded_context(a_source_correspondence *source) {
#if NEED_NAME_MANGLING
  if(!source || !source->name || !source->name_has_been_mangled)return "";
  // lower_il's promotion clears source parent scopes after mangling. Reuse
  // EDG's decoder to recover the presentation context from that retained name.
  // The decoded text is never an identity key or a source of ABI decisions.
  std::vector<char> buffer(256);a_boolean error=FALSE,overflow=FALSE;sizeof_t required=0;
  for(;;) {
    decode_identifier(source->name,buffer.data(),buffer.size(),&error,&overflow,&required);
    if(!overflow)break;
    if(required>4*1024*1024 || buffer.size()>=4*1024*1024)return "";
    buffer.resize(std::max((size_t)required,buffer.size()*2));
  }
  if(error)return "";
  string decoded=buffer.data(),spelling=source_spelling(source);
  if(spelling.empty())return "";
  // decode.c renders EDG internal-linkage module qualifiers as annotations,
  // not C++ namespaces. Keep plain file-scope static names readable and let
  // entity-based collision allocation distinguish multiple translation units.
  for(const char *annotation:{"[local to ","[static from "}) {
    for(size_t pos=decoded.find(annotation);pos!=string::npos;pos=decoded.find(annotation)) {
      size_t end=decoded.find(']',pos);if(end==string::npos)break;
      ++end;if(decoded.compare(end,2,"::")==0)end+=2;
      else if(end<decoded.size()&&decoded[end]==' ')++end;
      decoded.erase(pos,end-pos);
    }
  }
  size_t selected=string::npos,fallback=string::npos;
  for(size_t pos=decoded.find(spelling);pos!=string::npos;pos=decoded.find(spelling,pos+1)) {
    if(pos<2 || decoded.compare(pos-2,2,"::"))continue;
    size_t end=pos+spelling.size();
    if(end==decoded.size()){fallback=pos;continue;}
    if(decoded[end]=='('){selected=pos;break;}
    if(decoded[end]=='<') {
      unsigned depth=0;size_t cursor=end;
      for(;cursor<decoded.size();++cursor) {
        if(decoded[cursor]=='<')++depth;
        else if(decoded[cursor]=='>' && --depth==0){++cursor;break;}
      }
      if(cursor<decoded.size() && decoded[cursor]=='('){selected=pos;break;}
      if(cursor==decoded.size())fallback=pos;
    }
  }
  if(selected==string::npos)selected=fallback;
  // Conversion operator spelling can differ between the original identifier
  // and decoded target type. Its leading operator token still identifies the
  // qualification boundary without interpreting the conversion's semantics.
  if(selected==string::npos && spelling.compare(0,8,"operator")==0) {
    auto marker=decoded.find("::operator");if(marker!=string::npos)selected=marker+2;
  }
  if(selected==string::npos)return "";
  string result=decoded.substr(0,selected-2);
  // Function-template encodings can print a return type before the qualified
  // name. Remove only whitespace outside template arguments.
  unsigned angle=0;size_t begin=0;
  for(size_t i=0;i<result.size();++i) {
    if(result[i]=='<')++angle;
    else if(result[i]=='>' && angle)--angle;
    else if(result[i]==' ' && !angle)begin=i+1;
  }
  result.erase(0,begin);
  for(size_t pos=result.find("::");pos!=string::npos;pos=result.find("::",pos+1))result.replace(pos,2,"_");
  return result;
#else
  (void)source;return "";
#endif
}
string source_scope_context(a_source_correspondence *source) {
  if(!source)return "";
  auto found=source_context_cache.find(source);if(found!=source_context_cache.end())return found->second;
  string result=source_decoded_context(source);
  if(!result.empty())return source_context_cache[source]=result;
  for(auto scope=source->parent_scope;scope;scope=scope->parent) {
    string part;
    if(scope->kind==sck_namespace && scope->variant.assoc_namespace)
      part=source_spelling(&scope->variant.assoc_namespace->source_corresp);
    else if(scope->kind==sck_class_struct_union && scope->variant.assoc_type)
      part=source_spelling(&scope->variant.assoc_type->source_corresp);
    if(!part.empty())result=part+(result.empty()?"":"_")+result;
  }
  return source_context_cache[source]=result;
}
string source_routine_spelling(a_routine_ptr routine) {
  string result=source_spelling(&routine->source_corresp);
  if(result.empty())return "";
  string context_name=source_scope_context(&routine->source_corresp);
  if(routine->special_kind==sfk_constructor)return (context_name.empty()?result:context_name)+"__ctor";
  if(routine->special_kind==sfk_destructor) {if(!result.empty()&&result[0]=='~')result.erase(0,1);return (context_name.empty()?result:context_name)+"__destroy";}
  return context_name.empty()?result:context_name+"_"+result;
}
string source_owner_name() {
  const char *owner=getenv("EDG_DOT_CLASS");
  return source_identifier_component(owner?owner:"Generated");
}
string source_routine_scope(a_routine_ptr routine) {
  if(!routine)return "global";
  auto found=source_function_domains.find(routine);
  if(found!=source_function_domains.end())return found->second;
  // Ordinals are assigned in deterministic declaration traversal. Source names
  // are metadata, never a uniqueness key for overloaded or generated routines.
  string domain="function:"+std::to_string(source_function_domains.size()+1)+":"+
    string(routine->source_corresp.name?routine->source_corresp.name:"generated");
  return source_function_domains[routine]=domain;
}
void source_register(const void *entity,a_source_correspondence *source,const string &kind,
                     const string &scope,const string &domain,const string &candidate) {
  if(!entity || candidate.empty() || source_name_entries.count(entity))return;
  source_name_entry entry;entry.kind=kind;entry.source=source_spelling(source);
  entry.scope=scope;entry.domain=domain;entry.candidate=source_identifier(candidate);
  if(source)entry.position=source->decl_position;
  source_name_entries[entity]=entry;source_name_order.push_back(entity);
  source_name_reserved[domain].insert(entry.candidate);
}
string source_allocate(const string &domain,const string &candidate,const string &kind,bool getter=false,bool setter=false) {
  auto &used=source_name_used[domain];string result=candidate;
  auto occupied=[&](const string &identifier) {
    // C# locals may shadow owner members. References qualify only when needed;
    // unrelated owner declarations must not displace a source local name.
    return used.count(identifier) || (getter&&used.count("get_"+identifier)) || (setter&&used.count("set_"+identifier));
  };
  if(occupied(result)) {
    string base=candidate+"__"+source_identifier(kind);result=base;unsigned suffix=2;
    while(occupied(result)||source_name_reserved[domain].count(result))result=base+"_"+std::to_string(suffix++);
  }
  used.insert(result);
  if(getter)used.insert("get_"+result);
  if(setter)used.insert("set_"+result);
  return source_escape_identifier(result,kind);
}
void source_write_mapping(const void *entity) {
  auto found=source_name_entries.find(entity);
  if(found==source_name_entries.end() || !source_mapping_written.insert(entity).second)return;
  auto &entry=found->second;
  a_const_char *file=nullptr,*full=nullptr;a_line_number source_line=0;a_boolean at_end=FALSE;
  if(entry.position.seq)conv_seq_to_file_and_line(entry.position.seq,&file,&full,&source_line,&at_end);
  string path=full?full:file?file:"";
  source_mapping_output+="// dot-gen:source "+entry.kind+" "+source_hex(path)+" "+std::to_string((unsigned long long)source_line)+" "+
    std::to_string((unsigned)entry.position.column)+" "+source_hex(entry.source)+" "+entry.managed+" "+source_hex(entry.scope)+"\n";
}
string source_entity_name(const void *entity,const char*) {
  auto found=source_name_entries.find(entity);if(found==source_name_entries.end())return "";
  auto &entry=found->second;
  if(entry.managed.empty())entry.managed=source_allocate(entry.domain,entry.candidate,entry.kind,entry.has_getter,entry.has_setter);
  source_write_mapping(entity);return entry.managed;
}
string source_member_marker(const void *entity) {
  auto found=source_name_entries.find(entity);if(found==source_name_entries.end())return "";
  source_write_mapping(entity);return "// dot-gen:origin "+found->second.managed;
}
void source_collect_type(a_type_ptr original);
void source_collect_variables(a_variable_ptr variable,const string &kind,const string &scope,const string &domain) {
  for(auto v=variable;v;v=v->next) {
    if(ignore_variable_in_back_end(v))continue;
    string spelling=source_spelling(&v->source_corresp);
    source_register(v,&v->source_corresp,kind,scope,domain,spelling);
    if((kind=="global"||kind=="static-local")&&source_name_entries.count(v))source_name_entries[v].has_getter=true;
    source_collect_type(v->type);
  }
}
void source_collect_scope(a_scope_ptr scope,a_routine_ptr routine) {
  if(!scope || !source_name_scopes.insert(scope).second)return;
  string function_scope=source_routine_scope(routine),lexical=function_scope+"/scope:"+std::to_string(scope->number);
  if(scope->kind==sck_function)source_collect_variables(scope->variant.routine.parameters,"parameter",lexical,function_scope);
  source_collect_variables(scope->nonstatic_variables,"local",lexical,function_scope);
  source_collect_variables(scope->variables,routine?"static-local":"global",lexical,"owner");
  for(auto label=scope->labels;label;label=label->next)
    source_register(label,&label->source_corresp,"label",lexical,function_scope+"/labels",source_spelling(&label->source_corresp));
  for(auto t=scope->types;t;t=t->next)source_collect_type(t);
  for(auto child=scope->scopes;child;child=child->next)source_collect_scope(child,routine);
}
void source_collect_type(a_type_ptr original) {
  if(!original || !source_name_types.insert(original).second)return;
  if(ignore_type_in_back_end(original))return;
  auto t=skip_typerefs(original);
  if(original->kind==tk_typeref && (t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union || (t->kind==tk_integer && t->variant.integer.enum_type))) {
    string alias=source_spelling(&original->source_corresp);
    if(!alias.empty() && !source_record_aliases.count(t)) {
      source_record_aliases[t]=alias;source_alias_contexts[t]=source_scope_context(&original->source_corresp);
    }
  }
  if(original!=t){source_collect_type(t);return;}
  if(t->kind==tk_pointer)source_collect_type(t->variant.pointer.type);
  else if(t->kind==tk_array)source_collect_type(t->variant.array.element_type);
  else if(t->kind==tk_routine) {
    source_collect_type(t->variant.routine.return_type);
    for(auto parameter=t->variant.routine.extra_info->param_type_list;parameter;parameter=parameter->next)source_collect_type(parameter->type);
  } else if(t->kind==tk_struct || t->kind==tk_class || t->kind==tk_union) {
    string spelling=source_record_aliases.count(t)?source_record_aliases[t]:source_spelling(&t->source_corresp);
    string context_name=source_alias_contexts.count(t)?source_alias_contexts[t]:source_scope_context(&t->source_corresp);
    string candidate=context_name.empty()?spelling:context_name+"_"+spelling;
    if(!nested_types && !candidate.empty())candidate=source_owner_name()+"_"+candidate;
    source_register(t,&t->source_corresp,"type","record:"+(context_name.empty()?spelling:context_name+"::"+spelling),nested_types?"owner":"namespace",candidate);
    string domain="record:"+std::to_string(source_name_types.size());
    source_member_domains[t]=domain;
    if(source_record_aliases.count(t) && source_name_entries.count(t))source_name_entries[t].source=source_record_aliases[t];
    if(!spelling.empty())source_name_used[domain].insert(source_identifier(spelling));
    if(!t->incomplete)for(auto field=t->variant.class_struct_union.field_list;field;field=field->next) {
      source_register(field,&field->source_corresp,"member","record:"+(context_name.empty()?spelling:context_name+"::"+spelling),domain,source_spelling(&field->source_corresp));
      if(field->is_bit_field && source_name_entries.count(field)) {
        source_name_entries[field].has_getter=true;source_name_entries[field].has_setter=true;
      }
      source_collect_type(field->type);
    }
  } else if(t->kind==tk_integer && t->variant.integer.enum_type) {
    string spelling=source_record_aliases.count(t)?source_record_aliases[t]:source_spelling(&t->source_corresp);
    string context_name=source_alias_contexts.count(t)?source_alias_contexts[t]:source_scope_context(&t->source_corresp),candidate=spelling;
    if(candidate.empty())candidate="AnonymousEnum_"+std::to_string(t->source_corresp.decl_position.seq);
    if(!context_name.empty())candidate=context_name+"_"+candidate;
    if(!nested_types)candidate=source_owner_name()+"_"+candidate;
    string semantic_scope="enum:"+(context_name.empty()?spelling:context_name+"::"+spelling);
    source_register(t,&t->source_corresp,"enum",semantic_scope,nested_types?"owner":"namespace",candidate);
    if(source_record_aliases.count(t) && source_name_entries.count(t))source_name_entries[t].source=spelling;
    string domain="enum:"+std::to_string(source_name_order.size());
    for(auto c=enum_constants(t);c;c=c->next) {
      source_register(c,&c->source_corresp,"enumerator",semantic_scope,domain,source_spelling(&c->source_corresp));
      source_enumerator_types[c]=t;
    }
  } else if(t->kind==tk_integer && t->variant.integer.enum_info.affiliated_type)source_collect_type(t->variant.integer.enum_info.affiliated_type);
}
void reset_source_names() {
  source_name_entries.clear();source_name_order.clear();source_name_used.clear();source_name_reserved.clear();source_local_identifiers.clear();
  source_export_names.clear();source_function_domains.clear();source_force_qualified_references=false;source_record_aliases.clear();source_alias_contexts.clear();source_context_cache.clear();source_member_domains.clear();source_name_types.clear();source_name_scopes.clear();
  source_enumerator_types.clear();source_enum_emitted.clear();source_mapping_written.clear();source_type_mapping_written.clear();
  source_mapping_output.clear();source_names_initialized=false;source_enum_literal=false;
  source_library_names=getenv("EDG_DOT_EMIT")&&string(getenv("EDG_DOT_EMIT"))=="managedlib";
}
void initialize_source_names(a_scope_ptr scope) {
  if(source_names_initialized)return;
  source_names_initialized=true;
  for(auto routine=scope->routines;routine;routine=routine->next)source_routine_scope(routine);
  for(const char *reserved:{"Run","Shutdown","ReleaseThread","Dispose","Equals","GetHashCode","GetType","ToString","Finalize","MemberwiseClone","ReferenceEquals"})
    source_name_used["owner"].insert(reserved);
  source_name_used["owner"].insert(source_owner_name());source_name_used["namespace"].insert(source_owner_name());
  // Public external functions have first choice of the source spelling. This
  // matters in C, where a function and a record tag may have the same name.
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_needed(r))continue;
    string spelling=source_routine_spelling(r);
    if(spelling.empty())continue;
    source_name_reserved["owner"].insert(source_identifier(spelling));
  }
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_needed(r))continue;
    string spelling=source_routine_spelling(r);
    if(spelling.empty())continue;
    if(source_library_names && routine_body_needed(r) && r->source_corresp.name_linkage==nlk_external && !r->compiler_generated) {
      source_export_names[r]=source_allocate("owner",source_identifier(spelling),"export");
      string base=spelling+"__body";spelling=base;unsigned suffix=2;
      while(source_name_reserved["owner"].count(source_identifier(spelling)))spelling=base+"_"+std::to_string(suffix++);
    }
    source_register(r,&r->source_corresp,"function",source_scope_context(&r->source_corresp),"owner",spelling);
  }
  // Preallocate function identities before records and global fields so direct
  // source APIs cannot be displaced by a later storage/type declaration.
  for(const void *entity:source_name_order){auto &entry=source_name_entries[entity];entry.managed=source_allocate(entry.domain,entry.candidate,entry.kind,entry.has_getter,entry.has_setter);}
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_needed(r))continue;
    source_collect_type(r->type);
    if(routine_body_needed(r))source_collect_scope(scope_for_routine_or_null(r),r);
  }
  source_collect_scope(scope,nullptr);
  // Owner members/types are fixed before local declarations. Their collision
  // domain is separate: references qualify a shadowed owner member/type.
  for(const void *entity:source_name_order) {
    auto &entry=source_name_entries[entity];
    if(entry.domain!="owner" && entry.domain!="namespace")continue;
    if(entry.managed.empty())entry.managed=source_allocate(entry.domain,entry.candidate,entry.kind,entry.has_getter,entry.has_setter);
    if(entry.kind=="type")source_name_used[source_member_domains[(a_type_ptr)entity]].insert(source_identifier_component(entry.managed));
  }
  // Bitfield properties reserve CLR accessor names before ordinary fields.
  // A source field get_ready must not collide with ready's generated getter.
  for(const void *entity:source_name_order) {
    auto &entry=source_name_entries[entity];
    if(entry.kind=="member" && entry.has_getter && entry.managed.empty())
      entry.managed=source_allocate(entry.domain,entry.candidate,entry.kind,entry.has_getter,entry.has_setter);
  }
  for(const void *entity:source_name_order) {
    auto &entry=source_name_entries[entity];
    if(entry.managed.empty())entry.managed=source_allocate(entry.domain,entry.candidate,entry.kind,entry.has_getter,entry.has_setter);
    if(entry.kind=="local" || entry.kind=="parameter")source_local_identifiers[entry.domain].insert(source_identifier_component(entry.managed));
  }
}
// Explicit helper bodies have synthetic parameters not represented by the
// originating source scope. Their references must be qualified unconditionally.
struct source_qualified_reference_guard {
  bool saved;
  source_qualified_reference_guard():saved(source_force_qualified_references){source_force_qualified_references=true;}
  ~source_qualified_reference_guard(){source_force_qualified_references=saved;}
};
bool source_reference_shadowed(const string &identifier) {
  if(source_force_qualified_references)return true;
  if(!current_function_scope)return false;
  auto found=source_local_identifiers.find(source_routine_scope(current_function_scope->variant.routine.ptr));
  return found!=source_local_identifiers.end() && found->second.count(source_identifier_component(identifier));
}
string source_qualified_owner() {
  const char *ns=getenv("EDG_DOT_NAMESPACE"),*owner=getenv("EDG_DOT_CLASS");
  return "global::"+string(ns&&*ns?string(ns)+".":"")+string(owner?owner:"Generated");
}
string source_owner_reference(const string &identifier,bool force_qualified=false) {
  if(!force_qualified && !source_reference_shadowed(identifier))return identifier;
  return (instance_mode?"this.":source_qualified_owner()+".")+identifier;
}
string source_routine_reference(a_routine_ptr routine,bool force_qualified=false) {
  return source_owner_reference(name(routine,"f"),force_qualified);
}
string source_type_reference(a_type_ptr source_type,bool force_qualified=false) {
  string identifier=source_entity_name(source_type,"type");
  if(identifier.empty() || (!force_qualified && !source_reference_shadowed(identifier)))return identifier;
  if(nested_types)return source_qualified_owner()+"."+identifier;
  const char *ns=getenv("EDG_DOT_NAMESPACE");
  return "global::"+string(ns&&*ns?string(ns)+".":"")+identifier;
}
string source_export_name(a_routine_ptr routine) {
  auto found=source_export_names.find(routine);if(found!=source_export_names.end())return found->second;
  string body=name(routine,"f");return "__edg_export_"+source_identifier_component(body);
}
string source_record_name(a_type_ptr record,const char *fallback) {
  string result=source_entity_name(record,fallback);
  if(result.empty()) {result=name(record,fallback);if(!nested_types)result=source_owner_name()+"_"+result;}
  if(source_name_entries.count(record) && source_type_mapping_written.insert(record).second) {
    auto &entry=source_name_entries[record];
    source_mapping_output+="// dot-gen:public-type "+source_hex(entry.source)+" "+result+" "+(record->incomplete?"1":"0")+"\n";
  }
  return result;
}
// Affiliated integers retain enum provenance for diagnostics only: their
// arithmetic/promotion ABI remains integral. Only actual enum types are named.
a_type_ptr source_enum_context(a_type_ptr source_type) {
  if(!source_type)return nullptr;
  auto original=skip_typerefs(source_type);
  if(original->kind!=tk_integer)return nullptr;
  return original->variant.integer.enum_type?original:original->variant.integer.enum_info.affiliated_type;
}
bool source_enum_nominal(a_type_ptr source_type) {
  if(source_enum_literal || !source_type)return false;
  auto original=skip_typerefs(source_type);
  return original->kind==tk_integer && original->variant.integer.enum_type && original->size<=8;
}
string source_enum_physical_type(a_type_ptr source_type) {
  bool saved=source_enum_literal;source_enum_literal=true;
  string result=type(source_type);source_enum_literal=saved;return result;
}
string source_enum_physical_constant(a_constant_ptr value) {
  bool saved=source_enum_literal;source_enum_literal=true;
  string result=constant(value);source_enum_literal=saved;return result;
}
void ensure_source_enum(a_type_ptr enumeration) {
  if(source_enum_literal || !enumeration)return;
  enumeration=skip_typerefs(enumeration);
  if(enumeration->kind!=tk_integer || !enumeration->variant.integer.enum_type || !source_enum_emitted.insert(enumeration).second)return;
  source_collect_type(enumeration);
  string id=source_entity_name(enumeration,"enum");if(id.empty())return;
  if(source_type_mapping_written.insert(enumeration).second) {
    auto &entry=source_name_entries[enumeration];
    source_mapping_output+="// dot-gen:public-type "+source_hex(entry.source)+" "+id+" 0\n";
  }
  source_enum_literal=true;
  string physical=type(enumeration);
  // CLR enum bases stop at64 bits. Retain wider extension enumerators as a
  // named constant container with their exact Int128/UInt128 representation.
  bool wide=enumeration->size>8;
  string declaration=wide?"public static class "+id+" {\n":"public enum "+id+" : "+physical+" {\n";
  for(auto value=enum_constants(enumeration);value;value=value->next) {
    string member=source_entity_name(value,"enumerator");
    if(member.empty())continue;
    declaration+=wide?"public static readonly "+physical+" "+member+" = "+constant(value)+";\n":member+" = "+constant(value)+",\n";
  }
  source_enum_literal=false;type_output+=declaration+"}\n";
}
string source_enum_type(a_type_ptr source_type) {
  if(!source_enum_nominal(source_type))return "";
  auto enumeration=skip_typerefs(source_type);ensure_source_enum(enumeration);
  return source_entity_name(enumeration,"enum");
}
string source_enum_constant(a_constant_ptr value) {
  if(source_enum_literal || value->kind!=ck_integer)return "";
  auto enumeration=source_enum_context(value->type);if(!enumeration)return "";
  ensure_source_enum(enumeration);
  a_constant_ptr original=nullptr;
  if(value->source_corresp.name && is_enum_constant(value))
    for(auto candidate=enum_constants(enumeration);candidate;candidate=candidate->next)
      if(candidate==value || (candidate->source_corresp.name && string(candidate->source_corresp.name)==value->source_corresp.name)){original=candidate;break;}
  if(original) {
    string member=source_type_reference(enumeration)+"."+source_entity_name(original,"enumerator");
    return source_enum_nominal(value->type)?member:"(("+type(value->type)+")"+member+")";
  }
  if(source_enum_nominal(value->type))return "(("+type(value->type)+")("+source_enum_physical_constant(value)+"))";
  return "";
}
string source_enum_case(a_constant_ptr value,a_type_ptr context_type,a_type_ptr governing_type=nullptr) {
  if(!governing_type)governing_type=context_type;
  if(!value || value->kind!=ck_integer || !context_type)return "";
  auto enumeration=source_enum_context(context_type);
  if(!enumeration)enumeration=source_enum_context(value->type);
  if(!enumeration)return "";
  ensure_source_enum(enumeration);
  for(auto candidate=enum_constants(enumeration);candidate;candidate=candidate->next) {
    if(candidate->kind!=ck_integer || cmp_integer_constants(value,candidate)!=0)continue;
    string member=source_entity_name(candidate,"enumerator");
    if(member.empty())continue;
    member=source_type_reference(enumeration)+"."+member;
    return source_enum_nominal(governing_type) && skip_typerefs(governing_type)==enumeration?
      member:"(("+type(governing_type)+")"+member+")";
  }
  if(source_enum_nominal(governing_type))return "(("+type(governing_type)+")("+source_enum_physical_constant(value)+"))";
  // A named enum-valued case can appear under a promoted integral switch.
  string named=source_enum_constant(value);
  return named.empty()?"":"(("+type(governing_type)+")"+named+")";
}
