// Expression helpers take explicit pointers to caller-owned storage: no closure,
// no copy of an lvalue, and no movement of a branch's effects outside the branch.
string cleanup_slot = "__edg_cleanup_state";
unsigned expression_serial = 0;
string expr(an_expr_node_ptr e);
string condition(an_expr_node_ptr e);
string expression_effect(an_expr_node_ptr e);
void statement_expression_captures(an_expr_node_ptr e,std::vector<a_variable_ptr> &vars);
void expression_captures(an_expr_node_ptr e, std::vector<a_variable_ptr> &vars) {
  if (!e) return;
  if(coroutine_descriptive_effect(e))return;
  if(e->kind==enk_statement){statement_expression_captures(e,vars);return;}
  if (e->kind == enk_variable) {
    auto v=node_variable(e);
    if (!virtual_types.count(v) && !global_offsets.count(v) && !tls_offsets.count(v) &&
        std::find(vars.begin(),vars.end(),v)==vars.end()) vars.push_back(v);
  } else if(e->kind==enk_lowered_eh_construct && e->variant.lowered_eh.kind==leck_internal_try) {
    for(auto a=e->variant.lowered_eh.variant.try_and_catch_expr;a;a=a->next)expression_captures(a,vars);
    auto capture=[&](a_constant_ptr c){if(c && c->kind==ck_stack_offset){auto v=c->variant.stack_offset.variable;if(v && !global_offsets.count(v) && !tls_offsets.count(v) && std::find(vars.begin(),vars.end(),v)==vars.end())vars.push_back(v);}};
    for(auto &entry:cleanup_regions){auto &r=entry.second;if(coroutine_descriptive_region(entry.first))continue;capture(r.handle);capture(r.condition);capture(r.vtt);}
    for(auto &entry:cleanup_arrays)for(auto c:entry)capture(c);
    if(cleanup_this && std::find(vars.begin(),vars.end(),cleanup_this)==vars.end())vars.push_back(cleanup_this);
  } else if (e->kind == enk_operation) {
    for(auto a=e->variant.operation.operands;a;a=a->next) expression_captures(a,vars);
  } else if(e->kind == enk_object_lifetime) expression_captures(e->variant.object_lifetime.expr,vars);
  else if(e->kind == enk_throw && e->variant.throw_info) expression_captures(e->variant.throw_info->expr,vars);
  else if(e->kind == enk_result_of_overriding_function) {
    for(auto p=current_function_scope->variant.routine.parameters;p;p=p->next)
      if(std::find(vars.begin(),vars.end(),p)==vars.end())vars.push_back(p);
  }
}
string sequence_result(an_expr_node_ptr e, bool address_result) {
  if(e->kind==enk_object_lifetime) return sequence_result(e->variant.object_lifetime.expr,address_result);
  if(e->kind==enk_operation && e->variant.operation.kind==eok_comma) {
    auto a=e->variant.operation.operands;
    return expression_effect(a)+sequence_result(a->next,address_result);
  }
  if(skip_typerefs(e->type)->kind==tk_void) return expression_effect(e);
  return "return " + (address_result ? address(e) : expr(e)) + ";\n";
}
string sequence_expression(an_expr_node_ptr e) {
  auto saved=expression_variables;
  string saved_slot=cleanup_slot;
  std::vector<a_variable_ptr> vars;
  expression_captures(e,vars);
  string id="__edg_expr"+std::to_string(++expression_serial);
  string args="&("+cleanup_slot+")", parameters="int* __edg_state";
  if(materialization_captures(e) || coroutine_expression_captures(e)) {
    args+=", __edg_frame";
    parameters+=", byte* __edg_frame";
  }
  if(current_function_scope && variadic_routine(current_function_scope->variant.routine.ptr->type)) {
    args+=", __edg_arguments";
    parameters+=", global::Edg.Runtime.VaList* __edg_arguments";
  }
  if(!pending_exception.empty()) {
    args+=", "+pending_exception;
    parameters+=", __edg_exception "+pending_exception;
  }
  for(size_t i=0;i<vars.size();++i) {
    auto v=vars[i];
    args+=", "+variable_address(v);
    string param="p"+std::to_string(i);
    parameters+=", "+type(v->type)+"* "+param;
    expression_variables[v]="(*"+param+")";
  }
  cleanup_cursor_used=true;
  cleanup_slot="(*__edg_state)";
  bool address=e->is_lvalue;
  string result;{ source_qualified_reference_guard references;result=sequence_result(e,address); }
  expression_variables=saved;
  cleanup_slot=saved_slot;
  helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+
      type(e->type)+(address?"*":"")+" "+id+"("+parameters+") {\n"+result+"}\n// dot-gen:member-end\n";
  string call=id+"("+args+")";
  return address?"(*("+call+"))":call;
}
string expression_effect(an_expr_node_ptr e) {
  if(!e)return "";
  if(coroutine_descriptive_effect(e))return "";
  // c_gen_be -> il_to_str::form_constant renders retained ck_void as (void)0.
  if(e->kind==enk_constant && node_constant(e)->kind==ck_void) {
    if(skip_typerefs(e->type)->kind!=tk_void)unsupported("void constant type");
    return "";
  }
  if(e->kind==enk_throw)return throw_effect(e);
  if(e->kind==enk_object_lifetime) return expression_effect(e->variant.object_lifetime.expr);
  if(e->kind==enk_lowered_eh_construct) {
    auto &eh=e->variant.lowered_eh;
    if(eh.kind==leck_cleanup_state)
      return cleanup_transition(eh.variant.cleanup_region_number,cleanup_slot);
    if(eh.kind==leck_unreachable_cleanup_state)return "";
    if(eh.kind==leck_exception_started && !pending_exception.empty())return "__edg_start_exception("+pending_exception+");\n";
    if(eh.kind==leck_internal_try) {
      cleanup_cursor_used=true;
      string id="__edg_internal_boundary"+std::to_string(++expression_serial);
      auto a=eh.variant.try_and_catch_expr;
      string attempt=expression_effect(a),failure=expression_effect(a->next);
      string unwind=cleanup_unwind(id,cleanup_slot);
      return "{int "+id+" = "+cleanup_slot+"; try {\n"+attempt+"} catch (global::System.Exception jump_check) when ("+source_jump_exclusion("jump_check")+") {\n"+unwind+failure+"throw;\n}}\n";
    }
    unsupported("EH expression transition",eh.kind);
  }
  if(e->kind==enk_operation) {
    auto op=e->variant.operation.kind;
    auto a=e->variant.operation.operands;
    if(op==eok_comma)return expression_effect(a)+expression_effect(a->next);
    if(op==eok_cast && skip_typerefs(e->type)->kind==tk_void)return expression_effect(a);
    if(op==eok_question && skip_typerefs(e->type)->kind==tk_void)
      return "if "+condition(a)+" {\n"+expression_effect(a->next)+"} else {\n"+
             expression_effect(a->next->next)+"}\n";
  }
  return (skip_typerefs(e->type)->kind==tk_void?"":"_ = ")+expr(e)+";\n";
}
