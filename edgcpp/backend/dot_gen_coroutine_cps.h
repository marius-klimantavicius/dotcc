// Suspension-dependent expression scheduling. All saved values/addresses live
// in the selected-allocator coroutine frame; no CLR stack address survives an
// await. Ordinary emission remains responsible for arithmetic and ABI details.
enum coroutine_value_mode {cvm_effect,cvm_value,cvm_address};
struct coroutine_expression_cache {string value,location;};
std::map<an_expr_node_ptr,coroutine_expression_cache> coroutine_expression_values;
std::map<an_expr_node_ptr,std::pair<unsigned long long,unsigned long long>> coroutine_expression_slots;
string coroutine_expression_override(an_expr_node_ptr expression,bool location) {
  auto found=coroutine_expression_values.find(expression);
  if(found==coroutine_expression_values.end())return "";
  return location?found->second.location:found->second.value;
}
bool coroutine_expression_captures(an_expr_node_ptr expression) {
  if(coroutine_expression_values.empty())return false;
  bool found=false;coroutine_walk_expression(expression,[&](an_expr_node_ptr node){found|=coroutine_expression_values.count(node)!=0;});return found;
}
bool coroutine_constant_value(an_expr_node_ptr expression) {
  if(expression->kind!=enk_constant)return false;
  auto kind=node_constant(expression)->kind;
  return kind==ck_integer || kind==ck_float;
}
bool coroutine_rhs_first(an_expr_operator_kind kind);
void coroutine_plan_expression(an_expr_node_ptr expression,coroutine_value_mode mode) {
  if(!expression || expression->kind==enk_routine || coroutine_constant_value(expression))return;
  if(mode==cvm_value && skip_typerefs(expression->type)->kind==tk_void)mode=cvm_effect;
  if(expression->kind==enk_object_lifetime){coroutine_plan_expression(expression->variant.object_lifetime.expr,mode);return;}
  if(mode==cvm_address && bitfield(expression)) {
    coroutine_plan_expression(expression->variant.operation.operands,expression->variant.operation.kind==eok_dot_field?cvm_address:cvm_value);return;
  }
  bool suspended=coroutine_has_await(expression);
  if(expression->kind==enk_operation && suspended) {
    auto kind=expression->variant.operation.kind;auto a=expression->variant.operation.operands;auto b=a?a->next:nullptr;
    if(kind==eok_comma){coroutine_plan_expression(a,cvm_effect);coroutine_plan_expression(b,mode);return;}
    if(kind==eok_parens){coroutine_plan_expression(a,mode);return;}
    if(kind==eok_cast && skip_typerefs(expression->type)->kind==tk_void){coroutine_plan_expression(a,cvm_effect);return;}
    if(kind==eok_question){coroutine_plan_expression(a,cvm_value);coroutine_plan_expression(b,mode);coroutine_plan_expression(b->next,mode);}
    else if(coroutine_rhs_first(kind)){coroutine_plan_expression(b,cvm_value);coroutine_plan_expression(a,cvm_address);}
    else if(kind==eok_dot_field || kind==eok_points_to_field)coroutine_plan_expression(a,kind==eok_dot_field?cvm_address:cvm_value);
    else if(kind==eok_address_of || kind==eok_reference_to || kind==eok_array_to_pointer || kind==eok_lvalue_adjust ||
            kind==eok_pre_incr || kind==eok_pre_decr || kind==eok_post_incr || kind==eok_post_decr)coroutine_plan_expression(a,cvm_address);
    else for(auto child=a;child;child=child->next)coroutine_plan_expression(child,cvm_value);
  } else if(expression->kind==enk_await || expression->kind==enk_yield) {
    auto ready=expression->variant.await_info.ready_resume_suspend;
    if(coroutine_has_await(ready))coroutine_plan_expression(ready,cvm_value);
  }
  if(mode==cvm_effect)return;
  if(mode==cvm_address) {
    if(expression->kind==enk_await || expression->kind==enk_yield) {
      auto ready=expression->variant.await_info.ready_resume_suspend;
      if(ready && ready->next)require_materialized_location(ready->next);
    } else require_materialized_location(expression);
  }
  auto inserted=coroutine_expression_slots.emplace(expression,std::make_pair(~0ULL,~0ULL));
  auto &offset=mode==cvm_address?inserted.first->second.second:inserted.first->second.first;
  if(offset!=~0ULL)return;
  auto storage=skip_typerefs(expression->type);
  offset=coroutine_storage(mode==cvm_address?8:storage->size,mode==cvm_address?8:alignment_of_type(storage));
}
void coroutine_plan_expression_statements(a_statement_ptr statement) {
  if(!statement)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(statement,selected)){coroutine_plan_expression_statements(selected);return;}
  bool condition=statement->kind==stmk_if || statement->kind==stmk_for || statement->kind==stmk_while || statement->kind==stmk_end_test_while || statement->kind==stmk_switch;
  if(coroutine_has_await(statement->expr))coroutine_plan_expression(statement->expr,condition?cvm_value:cvm_effect);
  switch(statement->kind) {
    case stmk_block:for(auto child=statement->variant.block.statements;child;child=child->next)coroutine_plan_expression_statements(child);break;
    case stmk_try_block:
      coroutine_plan_expression_statements(statement->variant.try_block->statement);
      for(auto handler=statement->variant.try_block->handlers;handler;handler=handler->next)coroutine_plan_expression_statements(handler->statement);
      break;
    case stmk_if:coroutine_plan_expression_statements(statement->variant.if_stmt.then_statement);coroutine_plan_expression_statements(statement->variant.if_stmt.else_statement);break;
    case stmk_for:
      coroutine_plan_expression_statements(statement->variant.for_loop.extra_info->initialization);
      if(coroutine_has_await(statement->variant.for_loop.extra_info->increment))coroutine_plan_expression(statement->variant.for_loop.extra_info->increment,cvm_effect);
      coroutine_plan_expression_statements(statement->variant.for_loop.statement);break;
    case stmk_while:case stmk_end_test_while:coroutine_plan_expression_statements(statement->variant.loop_statement);break;
    case stmk_switch:coroutine_plan_expression_statements(statement->variant.switch_stmt.body_statement);break;
    case stmk_init:
      if(statement->variant.dynamic_init->kind==dik_expression && coroutine_has_await(statement->variant.dynamic_init->variant.expression))
        coroutine_plan_expression(statement->variant.dynamic_init->variant.expression,cvm_value);
      break;
    default:break;
  }
}
void coroutine_prepare_expressions(a_statement_ptr body) {
  coroutine_expression_values.clear();coroutine_expression_slots.clear();coroutine_plan_expression_statements(body);
}
string coroutine_value_slot(an_expr_node_ptr expression,coroutine_value_mode mode) {
  auto found=coroutine_expression_slots.find(expression);
  if(found==coroutine_expression_slots.end() || (mode==cvm_address?found->second.second:found->second.first)==~0ULL)unsupported("missing coroutine expression frame slot");
  return coroutine_slot(mode==cvm_address?found->second.second:found->second.first,type(expression->type)+(mode==cvm_address?"*":""));
}
string coroutine_save_expression(an_expr_node_ptr expression,coroutine_value_mode mode,const string &value) {
  if(mode==cvm_effect){line(value);return "";}
  string slot=coroutine_value_slot(expression,mode);
  line(slot+"="+value+";");
  coroutine_expression_values[expression]=mode==cvm_address?
    coroutine_expression_cache{"","(*"+slot+")"}:coroutine_expression_cache{slot,slot};
  return mode==cvm_address?"(*"+slot+")":slot;
}
string coroutine_schedule_expression(an_expr_node_ptr expression,coroutine_value_mode mode);
string coroutine_schedule_await(an_expr_node_ptr expression,coroutine_value_mode mode);
bool coroutine_rhs_first(an_expr_operator_kind kind) {
  switch(kind) {
    case eok_bassign:case eok_assign:case eok_padd_assign:case eok_psubtract_assign:
    case eok_add_assign:case eok_subtract_assign:case eok_multiply_assign:
    case eok_divide_assign:case eok_remainder_assign:case eok_and_assign:
    case eok_or_assign:case eok_xor_assign:case eok_shiftl_assign:case eok_shiftr_assign:return true;
    default:return false;
  }
}
string coroutine_schedule_expression(an_expr_node_ptr expression,coroutine_value_mode mode) {
  if(!expression)return "";
  if(mode==cvm_value && skip_typerefs(expression->type)->kind==tk_void)mode=cvm_effect;
  if(expression->kind==enk_routine || coroutine_constant_value(expression)) {
    if(mode==cvm_effect)line(expression_effect(expression));
    return mode==cvm_effect?"":expr(expression);
  }
  if(expression->kind==enk_object_lifetime)
    return coroutine_schedule_expression(expression->variant.object_lifetime.expr,mode);
  if(expression->kind==enk_await || expression->kind==enk_yield)
    return coroutine_schedule_await(expression,mode);
  if(mode==cvm_address && bitfield(expression)) {
    auto base=expression->variant.operation.operands;
    coroutine_schedule_expression(base,expression->variant.operation.kind==eok_dot_field?cvm_address:cvm_value);
    return location(expression);
  }
  bool suspended=coroutine_has_await(expression);
  if(expression->kind==enk_operation && suspended) {
    auto kind=expression->variant.operation.kind;auto a=expression->variant.operation.operands;auto b=a?a->next:nullptr;
    if(kind==eok_parens) {
      string result=coroutine_schedule_expression(a,mode);
      if(mode!=cvm_effect)coroutine_expression_values[expression]=mode==cvm_address?coroutine_expression_cache{"",result}:coroutine_expression_cache{result,result};
      return result;
    }
    if(kind==eok_comma) {
      coroutine_schedule_expression(a,cvm_effect);
      string result=coroutine_schedule_expression(b,mode);
      if(mode!=cvm_effect)coroutine_expression_values[expression]=mode==cvm_address?coroutine_expression_cache{"",result}:coroutine_expression_cache{result,result};
      return result;
    }
    if(kind==eok_question || kind==eok_land || kind==eok_lor) {
      string id="__edg_coro_expression_"+std::to_string(++serial);
      string value=coroutine_schedule_expression(a,cvm_value);
      string test="("+value+(skip_typerefs(a->type)->kind==tk_pointer?"!=null)":"!=0)");
      auto inherited=coroutine_expression_values;
      if(kind==eok_question) {
        line("if(!"+test+")goto "+id+"_else;");
        string yes=coroutine_schedule_expression(b,mode);
        if(mode!=cvm_effect)line(coroutine_value_slot(expression,mode)+"="+(mode==cvm_address?"&("+yes+")":yes)+";");
        line("goto "+id+"_end;"+id+"_else: ;");coroutine_expression_values=inherited;
        string no=coroutine_schedule_expression(b->next,mode);
        if(mode!=cvm_effect)line(coroutine_value_slot(expression,mode)+"="+(mode==cvm_address?"&("+no+")":no)+";");
      } else {
        if(mode==cvm_address)unsupported("address of logical coroutine expression");
        line("if("+string(kind==eok_land?"!":"")+test+")goto "+id+"_short;");
        string right=coroutine_schedule_expression(b,cvm_value);
        if(mode!=cvm_effect)line(coroutine_value_slot(expression,mode)+"=unchecked(("+type(expression->type)+")("+right+(skip_typerefs(b->type)->kind==tk_pointer?"!=null":"!=0")+"?1:0));");
        line("goto "+id+"_end;"+id+"_short: ;");
        if(mode!=cvm_effect)line(coroutine_value_slot(expression,mode)+"="+string(kind==eok_land?"0":"1")+";");
      }
      line(id+"_end: ;");coroutine_expression_values=inherited;
      if(mode==cvm_effect)return "";
      string slot=coroutine_value_slot(expression,mode),result=mode==cvm_address?"(*"+slot+")":slot;
      coroutine_expression_values[expression]=mode==cvm_address?coroutine_expression_cache{"",result}:coroutine_expression_cache{result,result};
      return result;
    }
    if(kind==eok_cast && skip_typerefs(expression->type)->kind==tk_void)
      return coroutine_schedule_expression(a,cvm_effect);
    if(coroutine_rhs_first(kind)) {
      coroutine_schedule_expression(b,cvm_value);coroutine_schedule_expression(a,cvm_address);
    } else if(kind==eok_dot_field || kind==eok_points_to_field) {
      coroutine_schedule_expression(a,kind==eok_dot_field?cvm_address:cvm_value);
    } else if(kind==eok_address_of || kind==eok_reference_to || kind==eok_array_to_pointer || kind==eok_lvalue_adjust ||
              kind==eok_pre_incr || kind==eok_pre_decr || kind==eok_post_incr || kind==eok_post_decr) {
      coroutine_schedule_expression(a,cvm_address);
    } else {
      for(auto child=a;child;child=child->next)coroutine_schedule_expression(child,cvm_value);
    }
  } else if(suspended)unsupported("coroutine suspension in protected or unnormalized expression");
  string value=mode==cvm_effect?expression_effect(expression):mode==cvm_address?"&("+location(expression)+")":expr(expression);
  return coroutine_save_expression(expression,mode,value);
}
string coroutine_expression_root(an_expr_node_ptr expression,coroutine_value_mode mode) {
  auto inherited=coroutine_expression_values;
  coroutine_expression_values.clear();
  string result=coroutine_schedule_expression(expression,mode);
  coroutine_expression_values=inherited;
  return result;
}
