// Validate operand orders supported by C# emission. Cleanup liveness and
// predecessor links come directly from EDG region tables.
void check_operand_order(an_expr_node_ptr e) {
  if(!e)return;
  if(e->kind==enk_lowered_eh_construct && e->variant.lowered_eh.kind==leck_internal_try)
    for(auto a=e->variant.lowered_eh.variant.try_and_catch_expr;a;a=a->next)check_operand_order(a);
  if(e->kind==enk_throw && e->variant.throw_info)check_operand_order(e->variant.throw_info->expr);
  if(e->kind==enk_object_lifetime) {
    check_operand_order(e->variant.object_lifetime.expr);
  } else if(e->kind==enk_operation) {
    auto op=e->variant.operation.kind;
    std::vector<an_expr_node_ptr> operands;
    for(auto a=e->variant.operation.operands;a;a=a->next)operands.push_back(a);
    bool rhs_first=false;
    switch(op) {
      case eok_bassign: case eok_assign: case eok_padd_assign: case eok_psubtract_assign:
      case eok_add_assign: case eok_subtract_assign: case eok_multiply_assign:
      case eok_divide_assign: case eok_remainder_assign: case eok_and_assign:
      case eok_or_assign: case eok_xor_assign: case eok_shiftl_assign: case eok_shiftr_assign: rhs_first=true; break;
      default: break;
    }
    if(rhs_first)std::reverse(operands.begin(),operands.end());
    else if(e->variant.operation.eval_right_to_left && operands.size()>1)
      unsupported("right-to-left lowered operation",op);
    for(auto a:operands)check_operand_order(a);
  }
}
void check_operand_order(a_statement_ptr s) {
  if(!s)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){check_operand_order(selected);return;}
  if(s->kind!=stmk_end_test_while && s->kind!=stmk_for)check_operand_order(s->expr);
  switch(s->kind) {
    case stmk_try_block:
      check_operand_order(s->variant.try_block->statement);
      for(auto h=s->variant.try_block->handlers;h;h=h->next)check_operand_order(h->statement);
      break;
    case stmk_block:
      for(auto p=s->variant.block.statements;p;p=p->next)check_operand_order(p);
      break;
    case stmk_switch:check_operand_order(s->variant.switch_stmt.body_statement);break;
    case stmk_if:
      check_operand_order(s->variant.if_stmt.then_statement);
      check_operand_order(s->variant.if_stmt.else_statement);break;
    case stmk_while:check_operand_order(s->variant.loop_statement);break;
    case stmk_end_test_while:
      check_operand_order(s->variant.loop_statement);check_operand_order(s->expr);break;
    case stmk_for:
      check_operand_order(s->variant.for_loop.extra_info->initialization);
      check_operand_order(s->expr);
      check_operand_order(s->variant.for_loop.statement);
      check_operand_order(s->variant.for_loop.extra_info->increment);break;
    case stmk_init:
      if(s->variant.dynamic_init->kind==dik_expression)
        check_operand_order(s->variant.dynamic_init->variant.expression);
      break;
    default:break;
  }
}
