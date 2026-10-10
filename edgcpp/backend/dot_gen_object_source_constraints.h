static_assert(int(onk_last)==48, "source operator name profile mismatch");
static_assert(int(initk_none)==0 && int(initk_static)==1 && int(initk_dynamic)==2 && int(initk_zero)==3, "source initializer profile mismatch");
static_assert(int(dik_zero)==1 && int(dik_constant)==2 && int(dik_expression)==3, "source dynamic initializer profile mismatch");
// Staged source-only constraint capture. Included inside dot_object_source after
// capture_type/capture_semantic_constant have been declared. Never lowers EDG IL.
// Exact configured token identities, independent of execution operation IDs.
inline const char *source_fold_operator(a_token_kind token) {
  static_assert(int(tok_ampersand)==36, "source fold token profile mismatch");
  static_assert(int(tok_star)==37, "source fold token profile mismatch");
  static_assert(int(tok_plus)==38, "source fold token profile mismatch");
  static_assert(int(tok_minus)==39, "source fold token profile mismatch");
  static_assert(int(tok_divide)==42, "source fold token profile mismatch");
  static_assert(int(tok_remainder)==43, "source fold token profile mismatch");
  static_assert(int(tok_shift_left)==44, "source fold token profile mismatch");
  static_assert(int(tok_shift_right)==45, "source fold token profile mismatch");
  static_assert(int(tok_lt)==46, "source fold token profile mismatch");
  static_assert(int(tok_gt)==47, "source fold token profile mismatch");
  static_assert(int(tok_le)==48, "source fold token profile mismatch");
  static_assert(int(tok_ge)==49, "source fold token profile mismatch");
  static_assert(int(tok_eq)==50, "source fold token profile mismatch");
  static_assert(int(tok_ne)==51, "source fold token profile mismatch");
  static_assert(int(tok_excl_or)==54, "source fold token profile mismatch");
  static_assert(int(tok_or)==55, "source fold token profile mismatch");
  static_assert(int(tok_and_and)==56, "source fold token profile mismatch");
  static_assert(int(tok_or_or)==57, "source fold token profile mismatch");
  static_assert(int(tok_assign)==60, "source fold token profile mismatch");
  static_assert(int(tok_times_assign)==61, "source fold token profile mismatch");
  static_assert(int(tok_divide_assign)==62, "source fold token profile mismatch");
  static_assert(int(tok_remainder_assign)==63, "source fold token profile mismatch");
  static_assert(int(tok_plus_assign)==64, "source fold token profile mismatch");
  static_assert(int(tok_minus_assign)==65, "source fold token profile mismatch");
  static_assert(int(tok_shift_left_assign)==66, "source fold token profile mismatch");
  static_assert(int(tok_shift_right_assign)==67, "source fold token profile mismatch");
  static_assert(int(tok_and_assign)==68, "source fold token profile mismatch");
  static_assert(int(tok_excl_or_assign)==69, "source fold token profile mismatch");
  static_assert(int(tok_or_assign)==70, "source fold token profile mismatch");
  static_assert(int(tok_comma)==71, "source fold token profile mismatch");
  static_assert(int(tok_period_star)==216, "source fold token profile mismatch");
  static_assert(int(tok_arrow_star)==217, "source fold token profile mismatch");
  switch(token) {
    case tok_ampersand:return "&";
    case tok_star:return "*";
    case tok_plus:return "+";
    case tok_minus:return "-";
    case tok_divide:return "/";
    case tok_remainder:return "%";
    case tok_shift_left:return "<<";
    case tok_shift_right:return ">>";
    case tok_lt:return "<";
    case tok_gt:return ">";
    case tok_le:return "<=";
    case tok_ge:return ">=";
    case tok_eq:return "==";
    case tok_ne:return "!=";
    case tok_excl_or:return "^";
    case tok_or:return "|";
    case tok_and_and:return "&&";
    case tok_or_or:return "||";
    case tok_assign:return "=";
    case tok_times_assign:return "*=";
    case tok_divide_assign:return "/=";
    case tok_remainder_assign:return "%=";
    case tok_plus_assign:return "+=";
    case tok_minus_assign:return "-=";
    case tok_shift_left_assign:return "<<=";
    case tok_shift_right_assign:return ">>=";
    case tok_and_assign:return "&=";
    case tok_excl_or_assign:return "^=";
    case tok_or_assign:return "|=";
    case tok_comma:return ",";
    case tok_period_star:return ".*";
    case tok_arrow_star:return "->*";
    default:return nullptr;
  }
}
using SourceConstraintArgument=SourceTemplateArgument;
struct SourceConstraintPackReference : SourceTemplateArgument {
  bool enclosing=false;int depth=-1,position=-1;
  a_source_position location={};
};
struct SourceConstraintPackExpansion {
  bool ellipsis=false,enclosing_only=false,enclosing_any=false,function_declarator=false,pack_index=false,template_template=false;
  unsigned parameter_type=0;std::string error;
  a_source_position location={};std::vector<SourceConstraintPackReference> references;
};
struct SourceConstraintParameter {
  unsigned type=0,declared_type=0,ordinal=0,qualifiers=0;
  std::string name;
  bool pack=false,requires_parameter=false;
};
struct SourceConstraintEdge {std::string role;unsigned child=0;};
struct SourceConstraintSubstitution {
  bool known_dependent=false,known_nondependent=false,adjust_coordinates=false,alias_deduction_guide=false;
  std::vector<SourceTemplateFormal> formals;
  std::vector<SourceConstraintArgument> arguments;
};
struct SourceConstraintTemporary {
  int initialization_kind=-1;
  unsigned spelled_type=0,constructor=0,destructor=0,constant=0;
  bool dependent_constructor=false;
  std::vector<std::string> flags;
};
struct SourceConstraintAllocation {
  unsigned type=0,routine=0;
  bool lookup_deferred=false;
  bool is_new=false,dependent=false,placement=false,aligned=false,array_delete=false,global=false;
  bool has_initializer=false,braced=false,paren_aggregate=false,deducible=false,parenthesized=false;
};
struct SourceConstraintExpression {
  std::string kind,operation,error;
  SourceConstraintTemporary temporary;
  SourceConstraintAllocation allocation;
  unsigned type=0,original_lvalue_type=0,constant=0,type_operand=0,concept_id=0,binder=0,declaration=0;
  bool parenthesized=false,compiler_generated=false,volatile_fetch=false,pack_expansion=false;
  int fold_token=-1;bool fold_left=false;std::string fold_operator;
  a_field_ptr field=nullptr;unsigned field_owner=0,field_ordinal=0;
  unsigned parameter_number=0,levels_up=0,resolved_binder=0;
  bool standard_alignof=false;
  int opcode=-1,operation_type_kind=-1;
  std::vector<std::string> flags;
  bool lvalue=false,xvalue=false,type_constraint=false,noexcept_required=false;
  std::string lookup_name,lookup_qualified_name;
  unsigned lookup_owner=0,lookup_conversion_type=0;
  int lookup_operator=0;
  bool lookup_template=false,lookup_qualified=false,lookup_address=false,lookup_symbol=false;
  bool lookup_unknown_base=false,lookup_qualified_unknown_base=false;
  std::vector<unsigned> lookup_candidates;
  a_source_position location={};
  std::vector<SourceConstraintEdge> edges;
  std::vector<SourceConstraintParameter> parameters;
  std::vector<SourceConstraintSubstitution> substitutions;
  std::vector<SourceConstraintArgument> arguments;
};
struct SourceConstraintConcept {
  unsigned namespace_owner=0;
  std::string name,qualified_name,error;
  a_source_position location={};
  unsigned definition=0,header_constraint=0;
  std::vector<SourceTemplateFormal> formals;
};
struct SourceConstraintBinder {
  unsigned parent=0,expression=0,routine=0;
  bool outside=false;
  std::vector<SourceConstraintParameter> parameters;
};
struct SourceLocalScopeSnapshot {
  a_routine_ptr routine=nullptr;
  a_scope_ptr parent=nullptr;
  unsigned ordinal=0,child_count=0;
  std::string kind;
  a_source_position position={};
  std::vector<a_variable_ptr> variables;
};
struct SourceLocalScope {
  unsigned routine=0,parent=0,ordinal=0,child_count=0,variable_count=0;
  std::string kind,error;
  a_source_position position={};
};
struct SourceConstraintDeclaration {
  unsigned namespace_owner=0;
  std::string kind,name,qualified_name,error;
  unsigned type=0,owner_type=0,template_prototype=0;
  unsigned parameter_owner=0,parameter_binder=0,parameter_ordinal=0,parameter_declared_type=0;
  unsigned local_scope=0,local_ordinal=0,local_declared_type=0;
  int original_init_kind=-1,original_dynamic_init_kind=-1,operator_kind=-1;
  std::string initializer_kind="not-variable";
  unsigned initializer_constant=0,initializer_expression=0;
  bool has_template_origin=false,is_template_prototype=false,is_constexpr=false;
  std::string template_origin_kind="none",routine_kind="not-routine";
  a_source_position location={};
  std::vector<SourceTemplateFormal> template_formals;
  std::vector<SourceConstraintArgument> template_arguments;
};
struct SourceClassTemplateHeader {
  unsigned type=0,primary=0,constraint=0;
  std::vector<SourceTemplateFormal> formals;
  std::vector<SourceTemplateArgument> pattern;
  std::string error;
};
struct SourceClassSpecialization {
  unsigned selected=0;
  std::vector<SourceTemplateArgument> arguments;
  std::string error;
};
struct SourceRoutineTypeSubstitution {
  a_routine_ptr selected=nullptr;
  std::map<a_type_ptr,std::vector<unsigned>> roots;
  std::map<unsigned,unsigned> observed_arguments;
  unsigned declaration=0;
};
struct SourceConstraintGraph {
  std::map<unsigned,SourceRoutineTypeSubstitution> routine_substitutions;
  std::map<a_scope_ptr,SourceLocalScopeSnapshot> local_scope_snapshots;
  std::map<a_scope_ptr,unsigned> local_scope_ids;
  std::vector<SourceLocalScope> local_scopes;
  std::map<std::pair<an_expr_node_ptr,unsigned>,unsigned> expressions_by_original;
  std::map<a_template_ptr,unsigned> concepts_by_original;
  std::vector<SourceConstraintExpression> expressions;
  std::vector<SourceConstraintConcept> concepts;
  std::map<unsigned,SourceConstraintBinder> binders;
  unsigned active_binder=0;
  std::map<a_type_ptr,SourceClassTemplateHeader> class_template_headers;
  std::map<a_type_ptr,SourceClassSpecialization> class_specializations;
  std::set<a_new_delete_supplement_ptr> deferred_new_lookups;
  std::map<std::tuple<unsigned,unsigned,unsigned>,std::pair<unsigned,unsigned>> binder_pairs;
  std::map<unsigned,unsigned> requires_binders;
  std::map<unsigned,unsigned> type_operator_binders;
  std::set<a_type_ptr> active_type_contexts;
  std::map<std::pair<void*,std::string>,unsigned> declarations_by_original;
  std::vector<SourceConstraintDeclaration> declarations;
  std::set<unsigned> semantic_routine_signatures;
  std::map<a_pack_expansion_descr_ptr,unsigned> expansions_by_original;
  std::vector<SourceConstraintPackExpansion> expansions;
};
inline SourceConstraintGraph &source_constraint_graph(){static SourceConstraintGraph graph;return graph;}
// Preserve the frontend lookup decision separately from allocated type
// dependence: GNU prototype instantiation defers even `::new int`.
inline void observe_source_new_lookup(a_new_delete_supplement_ptr supplement,bool deferred) {
  auto &entries=source_constraint_graph().deferred_new_lookups;
  if(deferred)entries.insert(supplement);else entries.erase(supplement);
}
inline void observe_source_new_lookup_copy(a_new_delete_supplement_ptr original,a_new_delete_supplement_ptr copy) {
  observe_source_new_lookup(copy,source_constraint_graph().deferred_new_lookups.count(original)!=0);
}
struct SourceParameterScope {
  unsigned previous;
  explicit SourceParameterScope(unsigned binder):previous(source_constraint_graph().active_binder){source_constraint_graph().active_binder=binder;}
  ~SourceParameterScope(){source_constraint_graph().active_binder=previous;}
};
inline unsigned current_source_parameter_binder(){return source_constraint_graph().active_binder;}
inline std::pair<unsigned,unsigned> capture_parameter_binder_pair(unsigned expression,unsigned routine,unsigned parent,a_param_type_ptr parameters) {
  auto &graph=source_constraint_graph();auto key=std::make_tuple(expression,routine,parent);
  auto previous=graph.binder_pairs.find(key);if(previous!=graph.binder_pairs.end())return previous->second;
  unsigned inside=unsigned(graph.binders.size()+1),outside=inside+1;
  graph.binder_pairs.emplace(key,std::make_pair(inside,outside));
  graph.binders[inside]={parent,expression,routine,false,{}};
  graph.binders[outside]={parent,expression,routine,true,{}};
  auto &values=graph.binders.at(inside).parameters;
  unsigned ordinal=0;
  for(auto parameter=parameters;parameter;parameter=parameter->next) {
    SourceConstraintParameter value;value.ordinal=ordinal++;value.name=parameter->name?parameter->name:"";
    value.qualifiers=parameter->qualifiers;value.pack=parameter->is_parameter_pack;value.requires_parameter=parameter->is_requires_expr_param;
    values.push_back(value);
  }
  SourceParameterScope scope(inside);ordinal=0;
  for(auto parameter=parameters;parameter;parameter=parameter->next,++ordinal) {
    values[ordinal].type=capture_type(parameter->type);
    values[ordinal].declared_type=capture_type(parameter->declared_type);
  }
  graph.binders.at(outside).parameters=values;
  return {inside,outside};
}
inline unsigned capture_constraint_expression(an_expr_node_ptr,unsigned=~0U);
inline unsigned capture_constraint_concept(a_template_ptr);
inline unsigned capture_constraint_routine(a_routine_ptr);

inline unsigned capture_constraint_pack_expansion(a_pack_expansion_descr_ptr original) {
  if(!original)return 0;
  auto &graph=source_constraint_graph();auto previous=graph.expansions_by_original.find(original);
  if(previous!=graph.expansions_by_original.end())return previous->second;
  unsigned id=unsigned(graph.expansions.size()+1);graph.expansions_by_original[original]=id;graph.expansions.emplace_back();
  SourceConstraintPackExpansion value;
  value.ellipsis=original->ellipsis_seen;value.location=original->ellipsis_position;
  value.enclosing_only=original->uses_only_enclosing_packs;value.enclosing_any=original->uses_any_enclosing_packs;
  value.function_declarator=original->is_function_declarator;value.pack_index=original->is_pack_index;
  value.template_template=original->param_symbol_is_template_template;value.parameter_type=capture_type(original->param_symbol_type);
  for(auto ref=original->packs_referenced;ref;ref=ref->next) {
    SourceConstraintPackReference item;item.enclosing=ref->uses_enclosing_pack;item.location=ref->position;
    if(ref->kind!=prk_template_param)value.error="source constraint pack requires original function/binding pack ownership: kind="+std::to_string(int(ref->kind));
    else {
      auto symbol=ref->symbol?fundamental_symbol_of(ref->symbol):nullptr;
      if(ref->coordinates){item.depth=ref->coordinates->depth;item.position=ref->coordinates->position;}
      if(symbol && symbol->kind==sk_type){item.kind="type";item.type=capture_type(symbol->variant.type.ptr);}
      else if(symbol && symbol->kind==sk_constant){item.kind="constant";item.constant=capture_semantic_constant(symbol->variant.constant);}
      else if(symbol && symbol->kind==sk_class_template){item.kind="template";item.templ=capture_source_template(symbol->variant.template_info->il_template_entry);}
      else value.error="source constraint pack reference lacks its original template parameter declaration";
    }
    value.references.push_back(item);
  }
  graph.expansions[id-1]=value;return id;
}
inline SourceConstraintArgument capture_constraint_argument(a_template_arg_ptr arg,std::string &error) {
  SourceConstraintArgument result;
  result.pack=arg->is_pack;result.pack_element=arg->is_pack_element;result.ellipsis=arg->has_pack_ellipsis;
  result.integer_pack=arg->is_integer_pack;result.explicit_argument=arg->explicitly_specified;
  result.injected_class=arg->type_is_injected_class_name;result.auto_parameter=arg->param_is_auto;result.decltype_auto_parameter=arg->param_is_decltype_auto;
  result.expansion=capture_constraint_pack_expansion(arg->pack_expansion_descr);
  if(arg->is_error || arg->is_array_bound_of_unknown_type || arg->is_provisional_value)
    error="source concept argument requires original deduction/error value: kind="+std::to_string(int(arg->kind));
  else if(arg->kind==tak_start_of_pack_expansion)result.kind="pack-boundary";
  else if(arg->kind==tak_type && arg->variant.type){result.kind="type";result.type=capture_type(arg->variant.type);}
  else if(arg->kind==tak_nontype && arg->variant.constant){result.kind="constant";result.constant=capture_semantic_constant(arg->variant.constant);}
  else if(arg->kind==tak_template && arg->variant.templ.ptr) {
    result.kind="template";result.templ=capture_source_template(arg->variant.templ.ptr);
    result.substituted_template=capture_source_template(arg->variant.templ.substituted_param_template);
  } else error="source concept argument lacks its exact original semantic value: kind="+std::to_string(int(arg->kind));
  return result;
}

inline void capture_source_alias(a_type_ptr type) {
  auto &s=state();if(s.source_aliases.count(type))return;
  s.source_aliases.emplace(type,SourceAliasInstance{});
  SourceAliasInstance value;auto extra=type->variant.typeref.extra_info;
  value.prototype=type->variant.typeref.is_prototype_instantiation;
  value.nonreal=type->variant.typeref.is_nonreal;value.dependent=type->variant.typeref.is_dependent;
  if(!extra || !extra->assoc_template)value.error="alias instance has no original template declaration";
  else {
    value.templ=capture_source_template(extra->assoc_template);value.minimum_arguments=extra->min_template_arguments;
    value.original_present=extra->orig_template_arg_list!=nullptr;
    for(auto arg=extra->template_arg_list;arg;arg=arg->next)value.arguments.push_back(capture_constraint_argument(arg,value.error));
    for(auto arg=extra->orig_template_arg_list;arg;arg=arg->next)value.original_arguments.push_back(capture_constraint_argument(arg,value.error));
  }
  s.source_aliases[type]=value;
}

inline SourceTemplateFormal capture_constraint_formal(a_template_parameter_ptr param,std::string &error) {
  SourceTemplateFormal formal;
  if(!param){error="source constraint substitution lacks its original IL formal declaration";return formal;}
  formal.name=spelling(param->source_corresp);formal.position=param->source_corresp.decl_position;
  formal.pack=param->is_pack;formal.abbreviated=param->is_abbreviated;
  if(param->kind==tpk_type) {formal.kind="type";formal.type=capture_type(param->variant.type.ptr);formal.default_type=capture_type(param->variant.type.default_arg_type);}
  else if(param->kind==tpk_nontype) {formal.kind="constant";formal.constant=capture_semantic_constant(param->variant.nontype.constant);formal.default_constant=capture_semantic_constant(param->variant.nontype.default_arg_constant);}
  else if(param->kind==tpk_template) {formal.kind="template";formal.templ=capture_source_template(param->variant.templ.class_template);formal.default_template=capture_source_template(param->variant.templ.default_arg_template);}
  else error="source concept formal lacks an owned parameter family";
  return formal;
}

// Snapshot exact original scope/list identities before local type traversal can
// recursively capture decltype(variable.member). No source execution owner or
// generated parameter is invented for a block-local declaration.
inline void snapshot_source_local_scopes(a_scope_ptr scope,a_routine_ptr routine=nullptr,a_scope_ptr parent=nullptr,unsigned ordinal=0) {
  if(!scope || (scope->kind!=sck_function && scope->kind!=sck_block && scope->kind!=sck_condition))return;
  auto &snapshots=source_constraint_graph().local_scope_snapshots;
  if(snapshots.count(scope))return;
  if(scope->kind==sck_function) {routine=scope->variant.routine.ptr;parent=nullptr;ordinal=0;}
  if(!routine)return;
  SourceLocalScopeSnapshot value;value.routine=routine;value.parent=parent;value.ordinal=ordinal;
  value.kind=scope->kind==sck_function?"function":scope->kind==sck_block?"block":"condition";
  if(scope->assoc_block)value.position=scope->assoc_block->position;
  for(auto variable=scope->nonstatic_variables;variable;variable=variable->next)value.variables.push_back(variable);
  for(auto child=scope->scopes;child;child=child->next)++value.child_count;
  snapshots[scope]=value;
  unsigned child_ordinal=0;for(auto child=scope->scopes;child;child=child->next)
    snapshot_source_local_scopes(child,routine,scope,child_ordinal++);
}
inline unsigned capture_source_local_scope(a_scope_ptr scope) {
  auto &graph=source_constraint_graph();auto previous=graph.local_scope_ids.find(scope);
  if(previous!=graph.local_scope_ids.end())return previous->second;
  auto found=graph.local_scope_snapshots.find(scope);if(found==graph.local_scope_snapshots.end())return 0;
  unsigned id=unsigned(graph.local_scopes.size()+1);graph.local_scope_ids[scope]=id;graph.local_scopes.emplace_back();
  const auto original=found->second;SourceLocalScope value;value.kind=original.kind;value.position=original.position;
  value.ordinal=original.ordinal;value.child_count=original.child_count;value.variable_count=unsigned(original.variables.size());
  value.routine=capture_constraint_routine(original.routine);
  if(original.parent) {value.parent=capture_source_local_scope(original.parent);if(!value.parent)value.error="original local scope lost its parent snapshot";}
  graph.local_scopes[id-1]=value;return id;
}
inline bool capture_constraint_local_variable(a_variable_ptr variable,SourceConstraintDeclaration &value) {
  if(variable->is_parameter || variable->compiler_generated || variable->lowering_generated || !variable->source_corresp.enclosing_routine)return false;
  auto scope=variable->source_corresp.parent_scope;auto routine=variable->source_corresp.enclosing_routine;
  if(!scope || (scope->kind!=sck_function && scope->kind!=sck_block && scope->kind!=sck_condition))return false;
  auto &graph=source_constraint_graph();
  if(!graph.local_scope_snapshots.count(scope)) {
    auto type=state().type_ids.find(routine->type);
    if(type!=state().type_ids.end() && state().routine_abis.count(type->second)) {
      value.error="source local variable lacks its prelowering scope snapshot";return false;
    }
    auto root=scope;while(root && root->kind!=sck_function)root=root->parent;
    if(!root || root->variant.routine.ptr!=routine)return false;
    snapshot_source_local_scopes(root);
  }
  const auto &original=graph.local_scope_snapshots.at(scope);
  unsigned ordinal=0;while(ordinal<original.variables.size() && original.variables[ordinal]!=variable)++ordinal;
  if(original.routine!=routine || ordinal==original.variables.size()) {
    value.error="source local variable differs from its exact original scope/list membership";return false;
  }
  value.local_scope=capture_source_local_scope(scope);value.local_ordinal=ordinal;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  value.local_declared_type=capture_type(variable->declared_type);
#endif
  return value.local_scope!=0;
}
inline bool capture_constraint_function_parameter(a_variable_ptr variable,SourceConstraintDeclaration &value) {
  if(!variable->is_parameter || variable->is_this_parameter)return false;
  auto scope=variable->source_corresp.parent_scope;
  if(!scope || scope->kind!=sck_function || !scope->variant.routine.ptr)return false;
  auto routine=scope->variant.routine.ptr;auto &s=state();auto &graph=source_constraint_graph();
  auto captured=s.functions.find(routine);std::vector<a_variable_ptr> parameters;
  if(captured!=s.functions.end() && captured->second.scope_captured)parameters=captured->second.parameter_variables;
  else {
    auto type=s.type_ids.find(routine->type);
    if(type!=s.type_ids.end() && s.routine_abis.count(type->second)) {
      value.error="source function parameter lost its prelowering declaration list";return false;
    }
    for(auto parameter=scope->variant.routine.parameters;parameter;parameter=parameter->next)
      if(parameter!=scope->variant.routine.this_param_variable)parameters.push_back(parameter);
  }
  unsigned ordinal=0;while(ordinal<parameters.size() && parameters[ordinal]!=variable)++ordinal;
  if(ordinal==parameters.size())return false;
  value.parameter_ordinal=ordinal;
  value.parameter_owner=capture_constraint_routine(routine);
  unsigned type=captured!=s.functions.end()?captured->second.signature_type:capture_type(routine->type);
  auto binder=graph.binder_pairs.find(std::make_tuple(0U,type,0U));
  if(binder==graph.binder_pairs.end()) {value.error="source function parameter lacks its original routine prototype binder";return false;}
  value.parameter_binder=binder->second.second;
  const auto &original=graph.binders.at(value.parameter_binder).parameters;
  if(original.size()!=parameters.size() || value.parameter_ordinal>=original.size() ||
      original[value.parameter_ordinal].type!=value.type) {
    value.error="source function parameter differs from its exact original adjusted signature";return false;
  }
#if GENERATE_SOURCE_SEQUENCE_LISTS
  value.parameter_declared_type=capture_type(variable->declared_type);
#endif
  return true;
}
// Each partial class specialization has two distinct formal/actual maps.
// The class supplement's template_arg_list belongs to the primary template;
// partial_spec_template_arg_list belongs to the selected partial declaration.
inline unsigned capture_source_class_template_header(a_template_ptr templ,std::string &error) {
  SourceParameterScope scope(0);auto &graph=source_constraint_graph();
  if(!templ || (templ->kind!=templk_class && templ->kind!=templk_member_class) ||
      !templ->template_decl || !templ->prototype_instantiation.type || !is_immediate_class_type(templ->prototype_instantiation.type)) {
    error="selected class template lacks its exact original header/prototype";return 0;
  }
  auto prototype=templ->prototype_instantiation.type;
  auto previous=graph.class_template_headers.find(prototype);
  if(previous!=graph.class_template_headers.end())return previous->second.type?previous->second.type:capture_type(prototype);
  graph.class_template_headers[prototype]=SourceClassTemplateHeader{};
  unsigned type_id=capture_type(prototype);graph.class_template_headers[prototype].type=type_id;
  SourceClassTemplateHeader value;value.type=type_id;
  auto symbol=symbol_for(templ);
  if(!symbol || symbol->kind!=sk_class_template) {value.error="class template header lacks its original class template symbol";}
  else {
    auto primary_symbol=symbol->variant.template_info->primary_template_sym;
    if(primary_symbol) {
      if(primary_symbol==symbol || primary_symbol->kind!=sk_class_template || !primary_symbol->variant.template_info)
        value.error="partial class template has invalid original primary declaration";
      else value.primary=capture_source_class_template_header(primary_symbol->variant.template_info->il_template_entry,value.error);
    }
  }
  for(auto formal=templ->template_decl->param_list;formal;formal=formal->next)value.formals.push_back(capture_constraint_formal(formal,value.error));
  if(templ->template_decl->constraint.requires_clause)value.constraint=capture_constraint_expression(templ->template_decl->constraint.requires_clause->constraint,0);
  if(value.primary)for(auto argument=class_type_supp(prototype)->template_arg_list;argument;argument=argument->next)
    value.pattern.push_back(capture_constraint_argument(argument,value.error));
  graph.class_template_headers[prototype]=value;if(!value.error.empty())error=value.error;return type_id;
}
inline bool capture_source_class_specialization(a_type_ptr instance,a_template_ptr templ,std::string &error) {
  if(!instance || !templ || (templ->kind!=templk_class && templ->kind!=templk_member_class))return false;
  auto supplement=class_type_supp(instance);auto symbol=symbol_for(templ);
  if(!symbol || symbol->kind!=sk_class_template) {
    if(supplement->partial_spec_template_arg_list)error="partial class instance lacks original selected template symbol";
    return false;
  }
  auto primary=symbol->variant.template_info->primary_template_sym;
  if(!primary) {
    if(supplement->partial_spec_template_arg_list)error="partial class arguments have no selected partial declaration";
    return false;
  }
  auto &graph=source_constraint_graph();auto found=graph.class_specializations.find(instance);
  if(found!=graph.class_specializations.end()) {if(!found->second.error.empty())error=found->second.error;return true;}
  graph.class_specializations[instance]=SourceClassSpecialization{};
  SourceClassSpecialization value;value.selected=capture_source_class_template_header(templ,value.error);
  for(auto argument=supplement->partial_spec_template_arg_list;argument;argument=argument->next)
    value.arguments.push_back(capture_constraint_argument(argument,value.error));
  graph.class_specializations[instance]=value;if(!value.error.empty())error=value.error;return true;
}

inline void capture_constraint_declaration_scope(a_source_correspondence &source,SourceConstraintDeclaration &value) {
  value.name=spelling(source);value.qualified_name=value.name;value.location=source.decl_position;
  if(!value.parameter_owner && !value.local_scope)value.namespace_owner=capture_source_namespace_owner(source);
  if(source.is_class_member)value.owner_type=capture_type(scp_parent_class(&source));
  for(auto scope=source.parent_scope;scope;scope=scope->parent) {
    if(value.local_scope)break; // Exact original scope ancestry is serialized separately.
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;
      if(!ns)value.error="source declaration has absent original namespace";
      else if(!spelling(ns->source_corresp).empty())value.qualified_name=spelling(ns->source_corresp)+"::"+value.qualified_name;
    } else if(scope->kind==sck_function && value.parameter_owner && scope==source.parent_scope) {
      // The exact original parameter-list relation owns this local declaration.
      // Its routine retains enclosing class/namespace/template identity.
      break;
    } else if(scope->kind!=sck_file && scope->kind!=sck_class_struct_union)
      value.error="source expression declaration needs original local binder";
  }
  if(value.name.empty())value.error="source expression declaration needs original anonymous member identity";
}
inline unsigned capture_constraint_variable(a_variable_ptr variable) {
  SourceParameterScope declaration_scope(0);
  if(!variable)return 0;
  auto &graph=source_constraint_graph();auto key=std::make_pair(static_cast<void*>(variable),std::string("variable"));
  auto previous=graph.declarations_by_original.find(key);if(previous!=graph.declarations_by_original.end())return previous->second;
  unsigned id=unsigned(graph.declarations.size()+1);graph.declarations_by_original[key]=id;graph.declarations.emplace_back();
  SourceConstraintDeclaration value;value.kind="variable";value.type=capture_type(variable->type);value.is_constexpr=variable->is_constexpr;
  if(!capture_constraint_function_parameter(variable,value))capture_constraint_local_variable(variable,value);
  capture_constraint_declaration_scope(variable->source_corresp,value);
  value.original_init_kind=int(variable->init_kind);value.initializer_kind="none";
  a_constant_ptr initializer_constant=nullptr;
  if(variable->init_kind==initk_zero)value.initializer_kind="zero";
  else if(variable->init_kind==initk_static)initializer_constant=variable->initializer.constant;
  else if(variable->init_kind==initk_dynamic) {
    auto init=variable->initializer.dynamic;
    if(!init)value.error="original source variable lacks its selected dynamic initializer";
    else {
      value.original_dynamic_init_kind=int(init->kind);
      if(init->kind==dik_zero)value.initializer_kind="zero";
      else if(init->kind==dik_constant)initializer_constant=init->variant.constant.ptr;
      else if(init->kind==dik_expression) {
        value.initializer_kind="expression";value.initializer_expression=capture_constraint_expression(init->variant.expression);
      } else value.error="source expression declaration requires owned initializer family: dynamic-kind="+std::to_string(int(init->kind));
    }
  } else if(variable->init_kind!=initk_none)value.error="source expression declaration requires owned initializer kind="+std::to_string(int(variable->init_kind));
  if(initializer_constant) {
    if(initializer_constant->kind==ck_template_param && initializer_constant->variant.template_param.kind==tpck_expression) {
      value.initializer_kind="expression";
#if PROTOTYPE_INSTANTIATIONS_IN_IL
      if(initializer_constant->variant.template_param.local_expr_ref)value.error="source variable initializer requires original local expression correspondence";
      else
#endif
      value.initializer_expression=capture_constraint_expression(initializer_constant->variant.template_param.variant.expr);
    } else {value.initializer_kind="constant";value.initializer_constant=capture_semantic_constant(initializer_constant);}
  } else if(variable->init_kind==initk_static)value.error="source variable static initializer has no original constant";

  if(variable->template_info) {
    auto info=variable->template_info;auto templ=info->assoc_template;
    value.has_template_origin=true;value.is_template_prototype=variable->is_prototype_instantiation;
    value.template_origin_kind=templ && templ->kind==templk_static_data_member?"static-data-member":"variable-template";
    if(templ && templ->kind==templk_static_data_member) {
      // Static members inherit their template arguments from the original
      // enclosing class. Stock a_variable_template_info documents that both
      // argument lists and the independent template header are absent here.
      auto primary=templ->prototype_instantiation.variable;
      if(!primary || !variable->source_corresp.is_class_member || !primary->source_corresp.is_class_member ||
         info->template_arg_list || info->partial_spec_template_arg_list || templ->template_decl)
        value.error="static template member lost its original class declaration ownership";
      else value.template_prototype=primary==variable?id:capture_constraint_variable(primary);
    } else if(!templ || templ->kind!=templk_variable || !templ->template_decl || !templ->prototype_instantiation.variable)
      value.error="source variable reference lacks its exact original template declaration";
    else {
      if(templ->prototype_instantiation.variable!=variable)value.template_prototype=capture_constraint_variable(templ->prototype_instantiation.variable);
      for(auto formal=templ->template_decl->param_list;formal;formal=formal->next)value.template_formals.push_back(capture_constraint_formal(formal,value.error));
      if(info->partial_spec_template_arg_list)value.error="source variable partial specialization needs exact selected argument mapping";
      for(auto arg=info->template_arg_list;arg;arg=arg->next)value.template_arguments.push_back(capture_constraint_argument(arg,value.error));
    }
  }
  graph.declarations[id-1]=value;return id;
}
// A selected ordinary member may belong to a concrete class specialization.
// Capture its original enclosing-class substitution without changing its ABI
// or nominal record identity. Independent member-template arguments stay empty.
inline void capture_selected_member_owner(unsigned owner_id,unsigned primary_id,unsigned witness,std::string &error) {
  if(owner_id==primary_id)return;
  auto &s=state();
  auto owner=owner_id?s.types.at(owner_id-1).record_type:nullptr;
  auto primary=primary_id?s.types.at(primary_id-1).record_type:nullptr;
  if(!owner || !primary || !s.records.count(owner) || !s.records.count(primary)) {
    error="selected member lacks original enclosing class snapshots";return;
  }
  auto record=s.records.at(owner);
  if(record.dependent)return; // Already captured by original dependent class traversal.
  if(record.template_prototype) {
    if(record.template_prototype!=primary_id)error="selected members disagree on original enclosing class prototype";
    return;
  }
  auto supplement=class_type_supp(owner);auto templ=supplement?supplement->assoc_template:nullptr;
  if(!templ || (templ->kind!=templk_class && templ->kind!=templk_member_class) ||
      templ->prototype_instantiation.type!=primary || record.prototype) {
    error="selected concrete member requires exact original enclosing class substitution";return;
  }
  capture_source_class_specialization(owner,templ,record.template_error);
  // This original selected declaration caused the otherwise concrete record
  // to retain its class substitution. The expression that discovered it may
  // remain source-only/unemitted, so keep the exact witness as a graph edge.
  record.template_member_witness=witness;
  record.template_prototype=primary_id;s.records[owner]=record; // Reserve before recursive argument capture.
  for(auto argument=supplement->template_arg_list;argument;argument=argument->next)
    record.template_arguments.push_back(capture_constraint_argument(argument,record.template_error));
  if(!record.template_error.empty())error=record.template_error;
  s.records[owner]=record;
}
inline unsigned capture_constraint_routine(a_routine_ptr routine) {
  SourceParameterScope declaration_scope(0);
  if(!routine)return 0;
  auto &graph=source_constraint_graph();auto key=std::make_pair(static_cast<void*>(routine),std::string("routine"));
  auto previous=graph.declarations_by_original.find(key);if(previous!=graph.declarations_by_original.end())return previous->second;
  unsigned id=unsigned(graph.declarations.size()+1);graph.declarations_by_original[key]=id;graph.declarations.emplace_back();
  SourceConstraintDeclaration value;value.kind="routine";value.type=capture_type(routine->type);value.is_constexpr=routine->is_constexpr;
  value.routine_kind=routine->special_kind==sfk_constructor?"constructor":routine->special_kind==sfk_destructor?"destructor":routine->special_kind==sfk_operator?"operator":"function";
  if(routine->special_kind==sfk_operator)value.operator_kind=int(routine->variant.opname_kind);
  graph.semantic_routine_signatures.insert(value.type);
  capture_constraint_declaration_scope(routine->source_corresp,value);
  if(routine->assoc_template) {
    auto templ=routine->assoc_template;value.has_template_origin=true;value.is_template_prototype=routine->is_prototype_instantiation;
    value.template_origin_kind="function-template";
    if(templ->kind==templk_member_function && !templ->template_decl) {
      value.template_origin_kind="class-member-function";
      auto primary=templ->prototype_instantiation.routine;
      // il_def.h: ordinary members of class templates have no independent
      // template argument list. Their exact class owns the substitution.
      if(!primary || !routine->source_corresp.is_class_member || !primary->source_corresp.is_class_member ||
         routine->template_arg_list || primary->template_arg_list)
        value.error="source class member routine lost its original class/prototype ownership";
      else {
        value.template_prototype=primary==routine?id:capture_constraint_routine(primary);
        if(primary!=routine)capture_selected_member_owner(value.owner_type,graph.declarations.at(value.template_prototype-1).owner_type,id,value.error);
      }
    } else if((templ->kind!=templk_function && templ->kind!=templk_member_function) || !templ->template_decl || !templ->prototype_instantiation.routine)
      value.error="source routine reference lacks its exact original template declaration";
    else {
      if(templ->prototype_instantiation.routine!=routine)value.template_prototype=capture_constraint_routine(templ->prototype_instantiation.routine);
      for(auto formal=templ->template_decl->param_list;formal;formal=formal->next)value.template_formals.push_back(capture_constraint_formal(formal,value.error));
      for(auto arg=routine->template_arg_list;arg;arg=arg->next)value.template_arguments.push_back(capture_constraint_argument(arg,value.error));
    }
  }
  graph.declarations[id-1]=value;return id;
}

inline unsigned capture_constraint_concept(a_template_ptr declaration) {
  if(!declaration)return 0;
  auto &graph=source_constraint_graph();
  auto canonical=declaration->canonical_template?declaration->canonical_template:declaration;
  auto previous=graph.concepts_by_original.find(canonical);
  if(previous!=graph.concepts_by_original.end())return previous->second;
  unsigned id=unsigned(graph.concepts.size()+1);graph.concepts_by_original[canonical]=id;graph.concepts.emplace_back();
  SourceConstraintConcept value;
  auto definition=canonical->definition_template?canonical->definition_template:canonical;
  value.name=spelling(canonical->source_corresp);value.qualified_name=value.name;value.location=canonical->source_corresp.decl_position;
  value.namespace_owner=capture_source_namespace_owner(canonical->source_corresp);
  for(auto scope=canonical->source_corresp.parent_scope;scope;scope=scope->parent) {
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;
      if(!ns)value.error="source concept has absent original namespace";
      else if(!spelling(ns->source_corresp).empty())value.qualified_name=spelling(ns->source_corresp)+"::"+value.qualified_name;
    } else if(scope->kind!=sck_file)value.error="source concept requires owned nonnamespace declaration scope";
  }
  if(canonical->kind!=templk_concept || definition->kind!=templk_concept || value.name.empty() || !definition->template_decl)
    value.error="source concept lacks exact canonical definition/header";
  else {
    auto header=definition->template_decl;
    if(header->parent)value.error="source concept has unsupported enclosing template header";
    for(auto param=header->param_list;param;param=param->next) {
      value.formals.push_back(capture_constraint_formal(param,value.error));
    }
    if(header->constraint.requires_clause)value.header_constraint=capture_constraint_expression(header->constraint.requires_clause->constraint);
    value.definition=capture_constraint_expression(definition->prototype_instantiation.constraint);
    if(!value.definition)value.error="source concept definition has no original constraint expression";
  }
  graph.concepts[id-1]=value;return id;
}

inline void capture_constraint_lookup_candidates(a_symbol_ptr symbol,SourceConstraintExpression &value,std::set<a_symbol_ptr> &seen) {
  if(!symbol || !seen.insert(symbol).second)return;
  auto fundamental=fundamental_symbol_of(symbol);
  if(fundamental!=symbol){capture_constraint_lookup_candidates(fundamental,value,seen);return;}
  if(symbol->kind==sk_overloaded_function) {
    for(auto candidate=symbol->variant.overloaded_function.symbols;candidate;candidate=candidate->next)
      capture_constraint_lookup_candidates(candidate,value,seen);
  } else if(symbol->kind==sk_routine || symbol->kind==sk_member_function)
    value.lookup_candidates.push_back(capture_constraint_routine(symbol->variant.routine.ptr));
  else if(symbol->kind==sk_function_template) {
    auto templ=symbol->variant.template_info->il_template_entry;
    if(!templ || !templ->prototype_instantiation.routine)value.error="source dependent function lookup lacks original function template prototype";
    else value.lookup_candidates.push_back(capture_constraint_routine(templ->prototype_instantiation.routine));
  } else value.error="source dependent function lookup candidate requires owned symbol family: kind="+std::to_string(int(symbol->kind));
}
inline void capture_constraint_function_constant(a_constant_ptr constant,SourceConstraintExpression &value) {
  value.kind="dependent-function-reference";
  auto target=constant;
  value.lookup_template=constant->variant.template_param.kind==tpck_template_ref;
  if(value.lookup_template) {
    target=constant->variant.template_param.variant.template_ref.con;
    for(auto arg=constant->variant.template_param.variant.template_ref.arg_list;arg;arg=arg->next)
      value.arguments.push_back(capture_constraint_argument(arg,value.error));
  }
  if(!target || target->kind!=ck_template_param || target->variant.template_param.kind!=tpck_unknown_function) {
    value.error="source function-template reference lacks its original dependent function lookup";return;
  }
  const auto &parameter=target->variant.template_param;
  SourceConstraintDeclaration scope;capture_constraint_declaration_scope(target->source_corresp,scope);
  value.lookup_name=scope.name;value.lookup_qualified_name=scope.qualified_name;value.lookup_owner=scope.owner_type;
  if(!scope.error.empty())value.error=scope.error;
  value.lookup_qualified=parameter.is_qualified_name;value.lookup_address=parameter.has_address_of;
  value.lookup_unknown_base=target->source_corresp.member_of_unknown_base;value.lookup_qualified_unknown_base=target->source_corresp.qualified_unknown_base_member;
  const auto &function=parameter.variant.unknown_function;
  value.lookup_operator=int(function.opname_kind);value.lookup_conversion_type=capture_type(function.conversion_type);
  value.lookup_symbol=function.symbol!=nullptr;
  if(function.symbol){std::set<a_symbol_ptr> seen;capture_constraint_lookup_candidates(function.symbol,value,seen);}
#if MICROSOFT_EXTENSIONS_ALLOWED
  if(function.property_or_event_descr || function.special_kind!=sfk_none)value.error="source dependent accessor requires original property/event contract";
#endif
  if(value.lookup_operator || value.lookup_conversion_type) {
    if(value.error=="source expression declaration needs original anonymous member identity")value.error.clear();
  }
}
inline unsigned capture_constraint_expression(an_expr_node_ptr expression,unsigned binder) {
  if(!expression)return 0;
  auto &graph=source_constraint_graph();if(binder==~0U)binder=graph.active_binder;
  SourceParameterScope scope(binder);auto key=std::make_pair(expression,binder);
  auto previous=graph.expressions_by_original.find(key);if(previous!=graph.expressions_by_original.end())return previous->second;
  unsigned id=unsigned(graph.expressions.size()+1);graph.expressions_by_original[key]=id;graph.expressions.emplace_back();
  SourceConstraintExpression value;value.location=expression->position;
  {
    // Semantic result annotations use an existing type; they do not declare
    // the type recipe anew in the surrounding expression's lexical scope.
    // Stock cp_gen_be renders a user-written cast's destination type in
    // this lexical prototype scope; an implicit conversion merely borrows
    // its semantic result type (which may belong to another declaration).
    bool lexical_cast=expression->kind==enk_operation && !expression->compiler_generated &&
      (expression->variant.operation.kind==eok_cast || expression->variant.operation.kind==eok_ref_cast ||
       expression->variant.operation.kind==eok_bool_cast);
    bool lexical_temporary=expression->kind==enk_temp_init && expression->variant.init.dynamic_init &&
      expression->variant.init.dynamic_init->is_explicit_cast;
    SourceParameterScope result_annotation_scope(lexical_cast || lexical_temporary?binder:0);
    value.type=capture_type(expression->type);
    value.original_lvalue_type=capture_type(expression->orig_lvalue_type);
  }
  value.parenthesized=expression->is_parenthesized;value.compiler_generated=expression->compiler_generated;value.volatile_fetch=expression->volatile_fetch;
  value.lvalue=expression->is_lvalue;value.xvalue=expression->is_xvalue;value.binder=binder;
  value.pack_expansion=expression->is_pack_expansion;
  auto edge=[&](const char *role,an_expr_node_ptr child,unsigned context){value.edges.push_back({role,capture_constraint_expression(child,context)});};
  switch(expression->kind) {
    case enk_concept_id:
      value.kind="concept-id";value.type_constraint=expression->is_type_constraint;
      value.concept_id=capture_constraint_concept(expression->variant.concept_id.concept_template);
      for(auto arg=expression->variant.concept_id.args;arg;arg=arg->next)value.arguments.push_back(capture_constraint_argument(arg,value.error));
      break;
    case enk_requires: {
      value.kind="requires";
      auto frames=capture_parameter_binder_pair(id,0,binder,expression->variant.requires_expr.parameters);
      graph.requires_binders[id]=frames.second;value.parameters=graph.binders.at(frames.second).parameters;
      for(auto param=expression->variant.requires_expr.parameters;param;param=param->next)
        if(param->has_default_arg || param->default_arg_expr || !param->is_requires_expr_param)
          value.error="source requires parameter lost original parameter contract";
      for(auto req=expression->variant.requires_expr.requirements;req;req=req->next)edge("requirement",req,frames.second);
      if(requires_expr_substs) {
        const auto &substitutions=requires_expr_substs->get(expression);
        for(auto pair=substitutions.begin();pair!=substitutions.end();++pair) {
          SourceConstraintSubstitution substitution;
          substitution.known_dependent=pair->args_known_dependent;
          substitution.known_nondependent=pair->args_known_nondependent;
          substitution.adjust_coordinates=pair->adjust_coordinates;
          substitution.alias_deduction_guide=pair->alias_deduction_guide;
          for(auto param=pair->params;param;param=param->next)
            substitution.formals.push_back(capture_constraint_formal(param->il_template_parameter,value.error));
          for(auto arg=pair->args;arg;arg=arg->next)
            substitution.arguments.push_back(capture_constraint_argument(arg,value.error));
          value.substitutions.push_back(substitution);
        }
      }
      break;
    }
    case enk_compound_req: {
      value.kind="compound-requirement";value.noexcept_required=expression->variant.compound_req.is_noexcept;
      auto first=expression->variant.compound_req.expr_and_constraint;
      if(!first)value.error="compound requirement has no expression";
      else {edge("expression",first,binder);if(first->next){edge("constraint",first->next,binder);if(first->next->next)value.error="compound requirement has excess constraint nodes";}}
      break;
    }
    case enk_nested_req:value.kind="nested-requirement";edge("constraint",expression->variant.nested_req.constraint,binder);break;
    case enk_field: {
      value.kind="field-reference";value.field=expression->variant.field.ptr;
      if(!value.field) {value.error="original field reference has no selected field";break;}
      auto owner=parent_class_of(value.field);value.field_owner=capture_type(owner);
      auto record=state().records.find(owner);bool found=false;
      if(record!=state().records.end())for(const auto &field:record->second.fields) {
        if(field.il==value.field) {found=true;break;}++value.field_ordinal;
      }
      if(!found)value.error="source field reference lacks its exact original enclosing record/member";
      break;
    }
    case enk_braced_init_list:
      value.kind="braced-initializer-list";
      for(auto element=expression->variant.braced_init_list;element;element=element->next)edge("element",element,binder);
      break;
    case enk_fold: {
      value.kind="fold-expression";value.fold_token=int(expression->variant.fold.operator_token);value.fold_left=expression->variant.fold.left_associative;
      auto fold_spelling=source_fold_operator(expression->variant.fold.operator_token);
      if(!fold_spelling)value.error="original fold operator is not a C++17 fold token";
      else value.fold_operator=fold_spelling;
      // Partial substitution can retain more than two original operands.
      for(auto operand=expression->variant.fold.operands;operand;operand=operand->next)edge("operand",operand,binder);
      if(value.edges.empty())value.error="original fold expression has no operand";
      break;
    }
    case enk_new_delete: {
      value.kind="allocation-expression";
      auto supplement=expression->variant.new_delete;
      if(!supplement) {value.error="original allocation expression lacks its supplement";break;}
      auto &allocation=value.allocation;
      allocation.type=capture_type(supplement->type);allocation.routine=capture_constraint_routine(supplement->routine);
      allocation.is_new=supplement->is_new;allocation.lookup_deferred=source_constraint_graph().deferred_new_lookups.count(supplement)!=0;
      allocation.dependent=!supplement->routine && (allocation.lookup_deferred || (supplement->type && is_template_dependent_type(supplement->type)));
      allocation.placement=supplement->placement_new;allocation.aligned=supplement->aligned_version;
      allocation.array_delete=supplement->array_delete;allocation.global=supplement->global_new_or_delete;
      allocation.has_initializer=supplement->has_new_initializer;allocation.braced=supplement->new_initializer_is_brace_enclosed;
      allocation.paren_aggregate=supplement->new_initializer_is_paren_aggr_init;allocation.deducible=supplement->deducible_type;
      allocation.parenthesized=supplement->parenthesized_type_id;
      if(!allocation.type)value.error="original allocation expression lacks its object type";
      if(!allocation.routine && !allocation.dependent)value.error="source allocation requires its original folded/default operator selection";
      if(supplement->dynamic_init || supplement->freeing_of_storage_on_exception)
        value.error="source allocation requires its original initialization/rollback graph";
      for(auto arg=supplement->arg;arg;arg=arg->next)edge(allocation.is_new?"allocation-argument":"delete-operand",arg,binder);
      if(supplement->number_of_elements)edge("array-bound",supplement->number_of_elements,binder);
      break;
    }
    case enk_temp_init: {
      value.kind="temporary-initialization";
      auto dip=expression->variant.init.dynamic_init;
      if(!dip) {value.error="original source temporary lacks its selected initialization";break;}
      auto &temp=value.temporary;temp.initialization_kind=int(dip->kind);
      temp.spelled_type=capture_type(expression->variant.init.source.type);
      temp.destructor=capture_constraint_routine(dip->destructor);
      // This is an unevaluated/source-expression graph, not an execution body.
      // A real linked lifetime must get its own original source-only graph;
      // never adopt it into an unrelated emitted function or silently drop it.
      if(dip->variable || dip->lifetime || dip->init_expr_lifetime || dip->next_in_destruction_list ||
         dip->master_entry || dip->lifetime_of_overlapping_temps || dip->is_freeing_of_storage_on_exception ||
         dip->is_array_freeing || dip->is_constructor_init || dip->destruction_is_for_partially_constructed_aggregate ||
         dip->overlaps_temps_in_inner_lifetime)
        value.error="source temporary requires its original linked lifetime/master initialization graph";
      auto flag=[&](bool set,const char *name){if(set)temp.flags.push_back(name);};
      flag(dip->static_temp,"static-storage");flag(dip->follows_an_exec_statement,"after-statement");
      flag(dip->inside_conditional_expression,"conditional");flag(dip->unordered,"unordered-destruction");
      flag(dip->has_temporary_lifetime,"temporary-lifetime");flag(dip->is_explicit_cast,"explicit-cast");
      flag(dip->is_compound_literal,"compound-literal");flag(dip->is_braced_initializer,"braced");
      flag(dip->is_partially_initialized,"partial-initialization");flag(dip->is_result_for_class_rvalue_question_mark,"conditional-result");
      flag(dip->class_rvalue_initialized_through_master_entry,"master-result");flag(dip->is_result_for_comma_operator,"comma-result");
      flag(dip->is_reused_value,"reused-value");flag(dip->is_creation_of_initializer_list_object,"initializer-list-object");
      flag(dip->is_array_for_initializer_list_object,"initializer-list-array");
      flag(dip->is_top_temporary_for_constexpr_reference_param,"constexpr-reference-argument");
      switch(dip->kind) {
        case dik_none:case dik_zero:break;
        case dik_constant:case dik_nonconstant_aggregate:temp.constant=capture_semantic_constant(dip->variant.constant.ptr);break;
        case dik_expression:case dik_class_result_via_ctor:edge("initializer",dip->variant.expression,binder);break;
        case dik_constructor:
          temp.constructor=capture_constraint_routine(dip->variant.constructor.ptr);
          temp.dependent_constructor=!dip->variant.constructor.ptr && is_template_dependent_type(expression->type);
          if(!temp.constructor && !temp.dependent_constructor)value.error="source temporary constructor has no selected routine or dependent type";
          flag(dip->variant.constructor.is_copy_constructor_with_implied_source,"implied-copy-source");
          flag(dip->variant.constructor.is_implicit_copy_for_copy_initialization,"implicit-copy-initialization");
          flag(dip->variant.constructor.value_initialization,"value-initialization");
          flag(dip->variant.constructor.has_sequenced_arguments,"sequenced-arguments");
          flag(dip->variant.constructor.is_array_copy,"array-copy");
          if(dip->variant.constructor.is_copy_constructor_with_implied_source || dip->variant.constructor.is_array_copy)
            value.error="source temporary copy requires original implied source/array recipe";
          for(auto arg=dip->variant.constructor.args;arg;arg=arg->next)edge("argument",arg,binder);
          break;
        case dik_bitwise_copy:
          if(!dip->variant.bitwise_copy.source)value.error="source temporary bitwise copy requires original implied source";
          else edge("source",dip->variant.bitwise_copy.source,binder);
          break;
        default:value.error="source temporary initialization kind requires its original family: kind="+std::to_string(int(dip->kind));break;
      }
      break;
    }
    case enk_param_ref:
      value.kind="parameter-reference";value.parameter_number=expression->variant.param_ref.param_num;value.levels_up=expression->variant.param_ref.levels_up;
      if(!binder || !value.parameter_number)value.error="source parameter reference requires original enclosing binder";
      else {
        // Exact stock get_param_for_param_ref prototype-stack resolution.
        unsigned selected=binder,levels=value.levels_up;
        if(levels==0) {
          while(selected && !graph.binders.at(selected).outside)selected=graph.binders.at(selected).parent;
        } else if(!graph.binders.at(selected).outside)--levels;
        for(unsigned level=0;level<levels && selected;++level)selected=graph.binders.at(selected).parent;
        auto found=graph.binders.find(selected);
        if(found==graph.binders.end() || value.parameter_number>found->second.parameters.size())
          value.error="source parameter reference leaves its exact original parameter binder";
        else value.resolved_binder=selected;
      }
      break;
    case enk_variable:value.kind="variable-reference";value.declaration=capture_constraint_variable(expression->variant.variable.ptr);break;
    case enk_routine:
      value.kind="routine-reference";value.declaration=capture_constraint_routine(expression->variant.routine.ptr);
      if(!value.declaration)value.error="source vacuous destructor needs original name-reference contract";
      break;
    case enk_type_operand:value.kind="type-operand";value.type_operand=capture_type(expression->variant.type_operand.type);break;
    case enk_constant: {
      auto constant=node_constant(expression);
      if(constant->kind==ck_template_param && (constant->variant.template_param.kind==tpck_template_ref || constant->variant.template_param.kind==tpck_unknown_function))
        capture_constraint_function_constant(constant,value);
      else if(constant->kind==ck_template_param && constant->variant.template_param.kind==tpck_expression) {
        value.kind="expression-constant";
#if PROTOTYPE_INSTANTIATIONS_IN_IL
        if(constant->variant.template_param.local_expr_ref)value.error="source constraint constant requires an owned local expression reference";
        else
#endif
        edge("expression",constant->variant.template_param.variant.expr,binder);
      } else {value.kind="constant";value.constant=capture_semantic_constant(constant);}
      break;
    }
    case enk_sizeof_pack:
      value.kind="constant";value.constant=capture_semantic_constant_expr(expression);break;
    case enk_sizeof: case enk_alignof: case enk_datasizeof: {
      value.kind=expression->kind==enk_sizeof?"sizeof":expression->kind==enk_alignof?"alignof":"datasizeof";
      const auto &query=expression->variant.sizeof_info;value.standard_alignof=query.is_std_alignof;
      if(query.is_type)value.type_operand=capture_type(query.variant.type);
      else edge("operand",query.variant.expr,binder);
      break;
    }
    case enk_builtin_operation:
      value.kind="builtin-operation";value.opcode=int(expression->variant.builtin_operation.kind);
      value.operation=builtin_operation_names[expression->variant.builtin_operation.kind];
      for(auto operand=expression->variant.builtin_operation.operands;operand;operand=operand->next)edge("operand",operand,binder);
      break;
    case enk_operation:
      value.kind="operation";value.opcode=int(expression->variant.operation.kind);
      if(auto name=source_constraint_operation_name(expression->variant.operation.kind))value.operation=name;
      else value.error="source constraint contains an invalid original operation";
      value.operation_type_kind=int(expression->variant.operation.type_kind);
      {
        const auto &op=expression->variant.operation;
        if(op.returns_lvalue_instead_of_usual_rvalue)value.flags.push_back("returns-glvalue");
        if(op.is_reinterpret_cast)value.flags.push_back("reinterpret-cast");
        if(op.is_reinterpret_like_cast)value.flags.push_back("reinterpret-semantics");
        if(op.is_const_cast)value.flags.push_back("const-cast");
        if(op.is_reference_cast)value.flags.push_back("reference-cast");
        if(op.is_rvalue_reference_cast)value.flags.push_back("rvalue-reference-cast");
        if(op.implicit_in_member_naming)value.flags.push_back("implicit-member-naming");
        if(op.implicit_step_of_explicit_cast)value.flags.push_back("implicit-cast-step");
        if(op.is_conversion_call)value.flags.push_back("conversion-call");
        if(op.arg_dependent_lookup_suppressed_on_call)value.flags.push_back("adl-suppressed");
        if(op.call_with_qualified_function_name)value.flags.push_back("qualified-function-name");
#if BACK_END_IS_CP_GEN_BE
        if(op.only_found_through_arg_dependent_lookup)value.flags.push_back("adl-only");
        if(op.called_through_address_of_overload_set)value.flags.push_back("address-of-overload-set");
#endif
        if(op.call_uses_operator_syntax)value.flags.push_back("operator-call-syntax");
#if BACK_END_IS_C_GEN_BE
        if(op.has_deferred_ampersand)value.flags.push_back("deferred-ampersand");
#endif
#if BACK_END_IS_CP_GEN_BE
        if(op.suppress_top_level_parens)value.flags.push_back("suppress-top-level-parens");
#endif
        if(op.pointer_operand_is_second)value.flags.push_back("pointer-second");
        if(op.is_virtual_call)value.flags.push_back("virtual-call");
        if(op.eval_left_to_right)value.flags.push_back("left-to-right");
        if(op.eval_right_to_left)value.flags.push_back("right-to-left");
        if(op.is_consteval_call)value.flags.push_back("consteval-call");
#if GNU_EXTENSIONS_ALLOWED
        if(op.is_gnu_two_operand_question_mark)value.flags.push_back("gnu-omitted-middle");
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
        if(op.is_tracking_reference_cast || op.requires_runtime_cast_check || op.rewritten_property_reference_kind!=rprk_none)
          value.error="source operation requires managed extension auxiliary contract";
#endif
      }
      // Each supported opcode gets a checked role/arity contract in the reader;
      // operation-specific auxiliary metadata must be captured before export.
      for(auto operand=expression->variant.operation.operands;operand;operand=operand->next)edge("operand",operand,binder);
      break;
    default:value.error="source constraint expression family not yet owned: kind="+std::to_string(int(expression->kind));break;
  }
  graph.expressions[id-1]=value;return id;
}
