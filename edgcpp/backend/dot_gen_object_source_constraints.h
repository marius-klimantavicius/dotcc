static_assert(int(onk_last)==48, "source operator name profile mismatch");
static_assert(int(initk_none)==0 && int(initk_static)==1 && int(initk_dynamic)==2 && int(initk_zero)==3, "source initializer profile mismatch");
static_assert(int(dik_zero)==1 && int(dik_constant)==2 && int(dik_expression)==3, "source dynamic initializer profile mismatch");
// Staged source-only constraint capture. Included inside dot_object_source after
// capture_type/capture_semantic_constant have been declared. Never lowers EDG IL.
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
struct SourceConstraintExpression {
  std::string kind,operation,error;
  unsigned type=0,original_lvalue_type=0,constant=0,type_operand=0,concept_id=0,binder=0,declaration=0;
  bool parenthesized=false,compiler_generated=false,volatile_fetch=false;
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
  std::string name,qualified_name,error;
  a_source_position location={};
  unsigned definition=0,header_constraint=0;
  std::vector<SourceTemplateFormal> formals;
};
struct SourceConstraintBinder {unsigned parent=0;std::vector<SourceConstraintParameter> parameters;};
struct SourceConstraintDeclaration {
  std::string kind,name,qualified_name,error;
  unsigned type=0,owner_type=0,template_prototype=0;
  int original_init_kind=-1,original_dynamic_init_kind=-1;
  std::string initializer_kind="not-variable";
  unsigned initializer_constant=0,initializer_expression=0;
  bool has_template_origin=false,is_template_prototype=false,is_constexpr=false;
  a_source_position location={};
  std::vector<SourceTemplateFormal> template_formals;
  std::vector<SourceConstraintArgument> template_arguments;
};
struct SourceConstraintGraph {
  std::map<std::pair<an_expr_node_ptr,unsigned>,unsigned> expressions_by_original;
  std::map<a_template_ptr,unsigned> concepts_by_original;
  std::vector<SourceConstraintExpression> expressions;
  std::vector<SourceConstraintConcept> concepts;
  std::map<unsigned,SourceConstraintBinder> binders;
  std::map<std::pair<void*,std::string>,unsigned> declarations_by_original;
  std::vector<SourceConstraintDeclaration> declarations;
  std::set<unsigned> semantic_routine_signatures;
  std::map<a_pack_expansion_descr_ptr,unsigned> expansions_by_original;
  std::vector<SourceConstraintPackExpansion> expansions;
};
inline SourceConstraintGraph &source_constraint_graph(){static SourceConstraintGraph graph;return graph;}
inline unsigned capture_constraint_expression(an_expr_node_ptr,unsigned=0);
inline unsigned capture_constraint_concept(a_template_ptr);

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

inline void capture_constraint_declaration_scope(a_source_correspondence &source,SourceConstraintDeclaration &value) {
  value.name=spelling(source);value.qualified_name=value.name;value.location=source.decl_position;
  if(source.is_class_member)value.owner_type=capture_type(scp_parent_class(&source));
  for(auto scope=source.parent_scope;scope;scope=scope->parent) {
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;
      if(!ns || ns->has_internal_linkage || spelling(ns->source_corresp).empty())value.error="source expression declaration needs original unnamed namespace identity";
      else value.qualified_name=spelling(ns->source_corresp)+"::"+value.qualified_name;
    } else if(scope->kind!=sck_file && scope->kind!=sck_class_struct_union)
      value.error="source expression declaration needs original local binder";
  }
  if(value.name.empty())value.error="source expression declaration needs original anonymous member identity";
}
inline unsigned capture_constraint_variable(a_variable_ptr variable) {
  if(!variable)return 0;
  auto &graph=source_constraint_graph();auto key=std::make_pair(static_cast<void*>(variable),std::string("variable"));
  auto previous=graph.declarations_by_original.find(key);if(previous!=graph.declarations_by_original.end())return previous->second;
  unsigned id=unsigned(graph.declarations.size()+1);graph.declarations_by_original[key]=id;graph.declarations.emplace_back();
  SourceConstraintDeclaration value;value.kind="variable";value.type=capture_type(variable->type);value.is_constexpr=variable->is_constexpr;
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
    if(!templ || (templ->kind!=templk_variable && templ->kind!=templk_static_data_member) || !templ->template_decl || !templ->prototype_instantiation.variable)
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
inline unsigned capture_constraint_routine(a_routine_ptr routine) {
  if(!routine)return 0;
  auto &graph=source_constraint_graph();auto key=std::make_pair(static_cast<void*>(routine),std::string("routine"));
  auto previous=graph.declarations_by_original.find(key);if(previous!=graph.declarations_by_original.end())return previous->second;
  unsigned id=unsigned(graph.declarations.size()+1);graph.declarations_by_original[key]=id;graph.declarations.emplace_back();
  SourceConstraintDeclaration value;value.kind="routine";value.type=capture_type(routine->type);value.is_constexpr=routine->is_constexpr;
  graph.semantic_routine_signatures.insert(value.type);
  capture_constraint_declaration_scope(routine->source_corresp,value);
  if(routine->assoc_template) {
    auto templ=routine->assoc_template;value.has_template_origin=true;value.is_template_prototype=routine->is_prototype_instantiation;
    if((templ->kind!=templk_function && templ->kind!=templk_member_function) || !templ->template_decl || !templ->prototype_instantiation.routine)
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
  for(auto scope=canonical->source_corresp.parent_scope;scope;scope=scope->parent) {
    if(scope->kind==sck_namespace) {
      auto ns=scope->variant.assoc_namespace;
      if(!ns || ns->has_internal_linkage || spelling(ns->source_corresp).empty())value.error="source concept requires owned unnamed namespace identity";
      else value.qualified_name=spelling(ns->source_corresp)+"::"+value.qualified_name;
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
  auto &graph=source_constraint_graph();auto key=std::make_pair(expression,binder);
  auto previous=graph.expressions_by_original.find(key);if(previous!=graph.expressions_by_original.end())return previous->second;
  unsigned id=unsigned(graph.expressions.size()+1);graph.expressions_by_original[key]=id;graph.expressions.emplace_back();
  SourceConstraintExpression value;value.type=capture_type(expression->type);value.location=expression->position;
  value.original_lvalue_type=capture_type(expression->orig_lvalue_type);
  value.parenthesized=expression->is_parenthesized;value.compiler_generated=expression->compiler_generated;value.volatile_fetch=expression->volatile_fetch;
  value.lvalue=expression->is_lvalue;value.xvalue=expression->is_xvalue;value.binder=binder;
  auto edge=[&](const char *role,an_expr_node_ptr child,unsigned context){value.edges.push_back({role,capture_constraint_expression(child,context)});};
  switch(expression->kind) {
    case enk_concept_id:
      value.kind="concept-id";value.type_constraint=expression->is_type_constraint;
      value.concept_id=capture_constraint_concept(expression->variant.concept_id.concept_template);
      for(auto arg=expression->variant.concept_id.args;arg;arg=arg->next)value.arguments.push_back(capture_constraint_argument(arg,value.error));
      break;
    case enk_requires: {
      value.kind="requires";unsigned ordinal=0;
      for(auto param=expression->variant.requires_expr.parameters;param;param=param->next) {
        SourceConstraintParameter formal;formal.type=capture_type(param->type);formal.declared_type=capture_type(param->declared_type);
        formal.ordinal=ordinal++;formal.qualifiers=param->qualifiers;formal.name=param->name?param->name:"";
        formal.pack=param->is_parameter_pack;formal.requires_parameter=param->is_requires_expr_param;
        if(param->has_default_arg || param->default_arg_expr || !formal.requires_parameter)value.error="source requires parameter lost original parameter contract";
        value.parameters.push_back(formal);
      }
      graph.binders[id]={binder,value.parameters};
      for(auto req=expression->variant.requires_expr.requirements;req;req=req->next)edge("requirement",req,id);
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
    case enk_param_ref:
      value.kind="parameter-reference";value.parameter_number=expression->variant.param_ref.param_num;value.levels_up=expression->variant.param_ref.levels_up;
      if(!binder || !value.parameter_number)value.error="source parameter reference requires original enclosing binder";
      else {
        // Requires bodies are outside their parameter list, matching
        // il_to_str.c::get_param_for_param_ref; levels_up traverses binders.
        unsigned selected=binder;
        for(unsigned level=0;level<value.levels_up && selected;level++) {
          auto found=graph.binders.find(selected);
          if(found==graph.binders.end()){selected=0;break;}
          selected=found->second.parent;
        }
        auto found=graph.binders.find(selected);
        if(found==graph.binders.end() || value.parameter_number>found->second.parameters.size())
          value.error="source parameter reference leaves its exact original requires binder";
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
