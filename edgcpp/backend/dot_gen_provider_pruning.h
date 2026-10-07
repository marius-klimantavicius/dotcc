// Provider-only dead definition elimination. This does not change EDG needed
// flags: the C backend quite correctly retains externally visible definitions.
// The driver explicitly opts selected implementation files into library linking
// semantics. All other emitted definitions (including public header inlines)
// remain roots, even if no other translated function calls them.
bool provider_pruning_enabled=false;
std::set<a_routine_ptr> provider_prunable_routines,provider_live_routines;
std::set<a_variable_ptr> provider_prunable_variables,provider_live_variables;
std::set<const void*> provider_walk_seen;
std::vector<std::pair<char*,an_il_entry_kind>> provider_walk_pending;

bool provider_entity_kept(a_routine_ptr r) {
  return !provider_pruning_enabled || !provider_prunable_routines.count(r) || provider_live_routines.count(r);
}
bool provider_entity_kept(a_variable_ptr v) {
  return !provider_pruning_enabled || !provider_prunable_variables.count(v) || provider_live_variables.count(v);
}
void reset_provider_pruning() {
  provider_pruning_enabled=false;
  provider_prunable_routines.clear();provider_live_routines.clear();
  provider_prunable_variables.clear();provider_live_variables.clear();
  provider_walk_seen.clear();provider_walk_pending.clear();
}
string provider_position_file(const a_source_position &position) {
  if(!position.seq)return "";
  a_const_char *file=nullptr,*full=nullptr;a_line_number line=0;a_boolean at_end=FALSE;
  conv_seq_to_file_and_line(position.seq,&file,&full,&line,&at_end);
  return full?full:file?file:"";
}
void provider_queue(char *entry,an_il_entry_kind kind) {
  if(entry && !is_string_entry_kind(kind) && !provider_walk_seen.count(entry))
    provider_walk_pending.emplace_back(entry,kind);
}
char *provider_reference(char *entry,an_il_entry_kind kind) {
  // Secondary references include calls, function-pointer constants, variable
  // addresses, cleanup functions, exception metadata and lowered aliases.
  // Scope backlinks describe ownership rather than execution; following one
  // would pull the complete translation unit back into the live set.
  if(kind!=iek_scope)provider_queue(entry,kind);
  return entry; // Identity remap: never mutate the IL or frontend flags.
}
a_boolean provider_walk_stop(char *entry,an_il_entry_kind kind) {
  if(kind==iek_scope && ((a_scope_ptr)entry)->kind==sck_file)return TRUE;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  // Source-order metadata is not an executable dependency and can link to
  // unrelated declarations. Executable statements and lowered metadata are
  // traversed by the ordinary EDG walker independently of this list.
  if(kind==iek_source_sequence_entry)return TRUE;
#endif
  if(!provider_walk_seen.insert(entry).second)return TRUE;
  if(kind==iek_routine) {
    auto routine=(a_routine_ptr)entry;
    provider_live_routines.insert(routine);
    // GNU builtins can bind to a translated libc definition only during C#
    // emission (libc_builtin_definition), so that edge is absent from EDG IL.
    string builtin=libc_enabled?libc_builtin_symbol(routine):"";
    if(!builtin.empty())for(auto target=il_header.primary_scope->routines;target;target=target->next)
      if(target->source_corresp.name && string(target->source_corresp.name)==builtin &&
         target->source_corresp.name_linkage==nlk_external &&
         target->function_def_number!=NULL_function_def_number)
        provider_queue((char*)target,iek_routine);
    if(routine_body_needed(routine)) {
      auto body=scope_for_routine_or_null(routine);
      if(body)provider_queue((char*)body,iek_scope);
    }
  } else if(kind==iek_variable)provider_live_variables.insert((a_variable_ptr)entry);
  return FALSE;
}
void collect_provider_reachability(a_scope_ptr scope) {
  const char *manifest=getenv("EDG_DOT_PRUNABLE_SOURCES");
  if(!manifest || !*manifest)return;
  FILE *input=fopen(manifest,"rb");
  if(!input)unsupported("cannot read provider pruning source manifest");
  std::set<string> files;string path;
  for(int ch;(ch=fgetc(input))!=EOF;) {
    if(ch=='\n'){if(!path.empty())files.insert(path);path.clear();}
    else if(ch!='\r')path+=(char)ch;
  }
  if(ferror(input)){fclose(input);unsupported("cannot read provider pruning source manifest");}
  fclose(input);if(!path.empty())files.insert(path);
  if(files.empty())unsupported("empty provider pruning source manifest");
  // Classify definitions before turning on the filter. Body positions are
  // essential: routine declarations commonly originate in public libc headers.
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_body_needed(r))continue;
    auto body=scope_for_routine_or_null(r);
    string file=body && body->assoc_block?provider_position_file(body->assoc_block->position):"";
    if(files.count(file) && !r->is_initialization_routine && !r->is_finalization_routine && !r->is_tls_init_routine &&
       !(r->source_corresp.name && string(r->source_corresp.name).find(IL_LOWERING_INIT_ROUTINE_PREFIX)==0) &&
       // terminate_source_abort also synthesizes an edge during emission.
       !(r->source_corresp.name && string(r->source_corresp.name)=="abort") &&
       !coroutine_required(r))
      provider_prunable_routines.insert(r);
    else provider_queue((char*)r,iek_routine);
  }
  for(auto v=scope->variables;v;v=v->next) {
    if(!variable_needed(v))continue;
    // Unknown/header origins are conservative roots. Dynamic initializers
    // remain roots because their side effects need not be referenced elsewhere.
    if(files.count(provider_position_file(v->source_corresp.decl_position)) &&
       v->storage_class!=sc_extern && v->init_kind!=initk_dynamic && !coroutine_required(v))
      provider_prunable_variables.insert(v);
    else provider_queue((char*)v,iek_variable);
  }
  // Keep filtering off until closure is complete, so routine_body_needed uses
  // the original C backend liveness contract while visiting dependencies.
  for(size_t index=0;index<provider_walk_pending.size();++index) {
    auto item=provider_walk_pending[index];
    walk_il_subtree(nullptr,nullptr,provider_reference,nullptr,provider_walk_stop,FALSE,item.first,item.second);
  }
  provider_pruning_enabled=true;
  provider_walk_pending.clear();provider_walk_seen.clear();
}
string provider_pruning_report() {
  size_t routines=0,variables=0;
  for(auto r:provider_prunable_routines)if(provider_live_routines.count(r))++routines;
  for(auto v:provider_prunable_variables)if(provider_live_variables.count(v))++variables;
  return "// dot-gen:provider-pruning {\"mode\":\"reachable\",\"candidateRoutines\":"+std::to_string(provider_prunable_routines.size())+
    ",\"retainedRoutines\":"+std::to_string(routines)+",\"candidateVariables\":"+std::to_string(provider_prunable_variables.size())+
    ",\"retainedVariables\":"+std::to_string(variables)+"}";
}
