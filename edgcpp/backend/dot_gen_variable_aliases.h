// Resolved GNU aliases are alternate typed views of one object. Like the
// stock C backend's alias attribute, they allocate/initialize no storage.
std::map<a_variable_ptr,a_variable_ptr> variable_alias_targets;
std::map<a_variable_ptr,unsigned long long> variable_alias_alignments;
std::map<string,std::vector<a_variable_ptr>> variable_symbol_definitions;

bool register_bound_variable(a_variable_ptr variable_ptr) {
#if NAMED_REGISTERS_ALLOWED
  if(variable_ptr->has_named_register_storage_class)return true;
#endif
#if GNU_EXTENSIONS_ALLOWED
  return !variable_ptr->asm_name_is_valid && variable_ptr->asm_name_or_reg.reg!=anr_invalid;
#else
  return false;
#endif
}

bool explicit_variable_symbol(a_variable_ptr variable_ptr) {
  if(register_bound_variable(variable_ptr))return false;
#if GNU_EXTENSIONS_ALLOWED
  return variable_ptr->asm_name_is_valid && variable_ptr->asm_name_or_reg.name &&
         *variable_ptr->asm_name_or_reg.name;
#else
  return false;
#endif
}

string variable_linker_symbol(a_variable_ptr variable_ptr) {
  if(register_bound_variable(variable_ptr))return "";
#if GNU_EXTENSIONS_ALLOWED
  if(explicit_variable_symbol(variable_ptr))return variable_ptr->asm_name_or_reg.name;
#endif
  auto &source=variable_ptr->source_corresp;
  if(!source.name)return "";
  if(source.name_linkage==nlk_external)return source.name;
#if NEED_NAME_MANGLING
  if(source.name_has_been_mangled)return source.name;
#endif
  return ""; // Never guess linkage from a pretty/unmangled C++ name.
}

bool variable_symbol_view(a_variable_ptr variable_ptr) {
  return variable_ptr->storage_class==sc_extern && explicit_variable_symbol(variable_ptr);
}

bool variable_alias(a_variable_ptr variable_ptr) {
#if GNU_EXTENSIONS_ALLOWED
  return variable_ptr->is_gnu_alias || variable_ptr->is_weakref || variable_ptr->aliased_variable;
#else
  return false;
#endif
}

bool mapped_variable_alias(a_variable_ptr variable_ptr) {
  return variable_alias_targets.count(variable_ptr) &&
         (global_offsets.count(variable_ptr) || tls_offsets.count(variable_ptr));
}

void reset_variable_aliases() {
  variable_alias_targets.clear();variable_alias_alignments.clear();
  variable_symbol_definitions.clear();
}

void index_variable_symbols(a_variable_ptr first) {
  for(auto variable_ptr=first;variable_ptr;variable_ptr=variable_ptr->next) {
    if(variable_ptr->storage_class==sc_extern || variable_alias(variable_ptr) ||
       ignore_variable_in_back_end(variable_ptr))continue;
    string symbol=variable_linker_symbol(variable_ptr);
    if(!symbol.empty())variable_symbol_definitions[symbol].push_back(variable_ptr);
  }
}

a_variable_ptr variable_symbol_target(a_variable_ptr view) {
  if(!variable_symbol_view(view))return nullptr;
  string symbol=variable_linker_symbol(view);
  auto found=variable_symbol_definitions.find(symbol);
  if(found==variable_symbol_definitions.end())return nullptr;
  if(found->second.size()!=1) {
    diagnostic_scope diagnostic(view->source_corresp.decl_position);
    unsupported(("multiple storage definitions for variable asm symbol "+symbol).c_str());
  }
  return found->second.front();
}

a_variable_ptr resolve_variable_alias(a_variable_ptr alias,std::set<a_variable_ptr>& visiting) {
  auto found=variable_alias_targets.find(alias);
  if(found!=variable_alias_targets.end())return found->second;
  if(!variable_alias(alias) && !variable_symbol_view(alias))return alias;
  diagnostic_scope diagnostic(alias->source_corresp.decl_position);
  if(!visiting.insert(alias).second)unsupported("cyclic GNU variable alias");
#if GNU_EXTENSIONS_ALLOWED
  auto target=alias->aliased_variable;
#else
  a_variable_ptr target=nullptr;
#endif
  if(!target && variable_symbol_view(alias)) {
    target=variable_symbol_target(alias);
    if(!target)unsupported("unresolved variable asm symbol storage");
  }
  if(!target)unsupported("unresolved GNU variable alias");
  target=resolve_variable_alias(target,visiting);
  visiting.erase(alias);
  if(target->storage_class==sc_extern)unsupported("GNU variable alias target has no defined storage");
  if(thread_storage(alias)!=thread_storage(target))unsupported("GNU variable alias TLS mismatch");
  if(exception_metadata.count(target))unsupported("GNU variable alias to native exception metadata");
  if(alias->init_kind!=initk_none && alias->init_kind!=initk_zero)
    unsupported("GNU variable alias with its own initializer");
  auto view=skip_typerefs(alias->type),storage=skip_typerefs(target->type);
  if(!view->size || view->size>storage->size)unsupported("GNU variable alias view exceeds target storage");
  unsigned long long alignment=std::max<a_targ_alignment>(alignment_of_type(alias->type),alias->alignment);
  if(!alignment || (alignment&(alignment-1)) || alignment>4096)unsupported("GNU variable alias alignment");
  // Stronger minimum alignment for an alternate view is safe for the same
  // standalone object; its extent and initialization still come from target.
  auto &required=variable_alias_alignments[target];required=std::max(required,alignment);
  (void)type(alias->type);
  variable_alias_targets[alias]=target;
  return target;
}

void collect_variable_aliases(a_variable_ptr first,std::vector<a_variable_ptr>& storage) {
  for(auto alias=first;alias;alias=alias->next) {
    if(!variable_needed(alias) || (!variable_alias(alias) && !variable_symbol_view(alias)))continue;
    std::set<a_variable_ptr> visiting;
    auto target=resolve_variable_alias(alias,visiting);
    // An alias may be the only reference that requires its target. Allocate
    // the resolved definition even if it did not receive an ordinary use bit.
    if(std::find(storage.begin(),storage.end(),target)==storage.end())storage.push_back(target);
  }
}

void map_variable_alias_storage() {
  for(auto entry:variable_alias_targets) {
    auto alias=entry.first,target=entry.second;
    auto global=global_offsets.find(target),tls=tls_offsets.find(target);
    if(global!=global_offsets.end())global_offsets[alias]=global->second;
    else if(tls!=tls_offsets.end())tls_offsets[alias]=tls->second;
    else unsupported("GNU variable alias target storage was not allocated");
  }
}
