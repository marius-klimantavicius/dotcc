// C-linkage declarations in different C++ namespaces can remain distinct IL
// routines. c_gen_be::dump_routine_name emits their shared lowered name and
// lets the C compiler/linker associate the declaration with its definition.
// Resolve that exact linker identity here, without modifying EDG's IL.
std::map<string,std::vector<a_routine_ptr>> routine_symbol_definitions;
std::map<a_routine_ptr,a_routine_ptr> routine_symbol_targets;

void reset_routine_symbols() {
  routine_symbol_definitions.clear();routine_symbol_targets.clear();
  linked_routine_definitions.clear();
}

string routine_linker_symbol(a_routine_ptr routine) {
  const auto &source=routine->source_corresp;
  if(source.name_linkage!=nlk_external && source.name_linkage!=nlk_cplusplus_external)return "";
#if GNU_EXTENSIONS_ALLOWED
  if(has_gnu_routine_supp(routine)) {
    auto symbol=gnu_routine_supp(routine)->asm_name;
    if(symbol && *symbol)return symbol;
  }
#endif
  if(!source.name)return "";
  if(source.name_linkage==nlk_external)return source.name;
#if NEED_NAME_MANGLING
  if(source.name_has_been_mangled)return source.name;
#endif
  return ""; // Never match unrelated C++ overloads by their source spelling.
}

a_routine_ptr canonical_routine(a_routine_ptr routine) {
  if(routine->compiler_generated ||
     (routine->function_def_number!=NULL_function_def_number && scope_for_routine_or_null(routine)))return routine;
  auto prior=routine_symbol_targets.find(routine);
  if(prior!=routine_symbol_targets.end())return prior->second;
  auto symbol=routine_linker_symbol(routine);
  auto found=routine_symbol_definitions.find(symbol);
  if(symbol.empty() || found==routine_symbol_definitions.end())return routine_symbol_targets[routine]=routine;
  diagnostic_scope diagnostic(routine->source_corresp.decl_position);
  if(found->second.size()!=1)unsupported(("multiple definitions for routine linker symbol "+symbol).c_str());
  auto definition=found->second.front();
  auto from=skip_typerefs(routine->type),to=skip_typerefs(definition->type);
  if(from->kind!=tk_routine || to->kind!=tk_routine ||
     !from->variant.routine.extra_info->prototyped || !to->variant.routine.extra_info->prototyped ||
     !types_are_strictly_compatible(from,to,0))
    unsupported(("routine linker symbol ABI mismatch: "+symbol).c_str());
  linked_routine_definitions.insert(definition);
  return routine_symbol_targets[routine]=definition;
}

void index_routine_symbols(a_scope_ptr scope) {
  for(auto routine=scope->routines;routine;routine=routine->next) {
    if(routine->compiler_generated || ignore_routine_in_back_end(routine) ||
       routine->function_def_number==NULL_function_def_number || !scope_for_routine_or_null(routine))continue;
    auto symbol=routine_linker_symbol(routine);
    if(!symbol.empty())routine_symbol_definitions[symbol].push_back(routine);
  }
  // Establish body liveness before source names, exports and provider pruning.
  for(auto routine=scope->routines;routine;routine=routine->next)
    if(routine_needed(routine))(void)canonical_routine(routine);
}
