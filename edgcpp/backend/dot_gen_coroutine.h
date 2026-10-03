// Coroutine frame lowering consumes EDG's semantic body and await triples.
// body_generated denotes the promise/body/catch expansion, not a resumable
// target frame (c_gen_be.c still emits co_await for these expression nodes).
a_coroutine_descr_ptr coroutine_descriptor(a_statement_ptr statement) {
  if(!statement)return nullptr;
  if(statement->kind==stmk_coroutine)return statement->variant.coroutine.descr;
  if(statement->kind==stmk_try_block)
    return coroutine_descriptor(statement->variant.try_block->statement);
  if(statement->kind==stmk_block)
    for(auto child=statement->variant.block.statements;child;child=child->next) {
      auto descriptor=coroutine_descriptor(child);if(descriptor)return descriptor;
    }
  return nullptr;
}
void trace_coroutine_expression(an_expr_node_ptr expression,int depth) {
  if(!expression)return;
  fprintf(stderr,"%*scoroutine expression %s kind=%d type=%d size=%llu lvalue=%d\n",depth,"",
    trace_id(expression,"E").c_str(),expression->kind,skip_typerefs(expression->type)->kind,
    (unsigned long long)skip_typerefs(expression->type)->size,expression->is_lvalue);
  if(expression->kind==enk_await || expression->kind==enk_yield) {
    fprintf(stderr,"%*soriginal operand (not a separate evaluation):\n",depth+2,"");
    trace_coroutine_expression(expression->variant.await_info.operand,depth+4);
    const char* role[]={"ready","resume","suspend"};unsigned i=0;
    for(auto child=expression->variant.await_info.ready_resume_suspend;child;child=child->next) {
      fprintf(stderr,"%*s%s:\n",depth+2,"",i<3?role[i]:"extra");++i;
      trace_coroutine_expression(child,depth+4);
    }
  } else if(expression->kind==enk_operation) {
    fprintf(stderr,"%*soperator=%d\n",depth+2,"",expression->variant.operation.kind);
    for(auto child=expression->variant.operation.operands;child;child=child->next)
      trace_coroutine_expression(child,depth+2);
  } else if(expression->kind==enk_variable) {
    auto variable=node_variable(expression);
    fprintf(stderr,"%*svariable=%s name=%s\n",depth+2,"",trace_id(variable,"v").c_str(),variable->source_corresp.name?variable->source_corresp.name:"<generated>");
  } else if(expression->kind==enk_routine) {
    auto routine=expression->variant.routine.ptr;
    fprintf(stderr,"%*sroutine=%s definition=%lu needed=%d\n",depth+2,"",routine->source_corresp.name?routine->source_corresp.name:"<generated>",(unsigned long)routine->function_def_number,routine_body_needed(routine));
  } else if(expression->kind==enk_object_lifetime)
    trace_coroutine_expression(expression->variant.object_lifetime.expr,depth+2);
  else if(expression->kind==enk_lowered_eh_construct) {
    fprintf(stderr,"%*sEH=%d\n",depth+2,"",expression->variant.lowered_eh.kind);
    if(expression->variant.lowered_eh.kind==leck_internal_try)
      for(auto child=expression->variant.lowered_eh.variant.try_and_catch_expr;child;child=child->next)
        trace_coroutine_expression(child,depth+2);
  }
}
void trace_coroutine_statements(a_statement_ptr statement,int depth) {
  if(!statement)return;
  fprintf(stderr,"%*scoroutine statement=%d\n",depth,"",statement->kind);
  trace_coroutine_expression(statement->expr,depth+2);
  switch(statement->kind) {
    case stmk_block:for(auto child=statement->variant.block.statements;child;child=child->next)trace_coroutine_statements(child,depth+2);break;
    case stmk_try_block:
      trace_coroutine_statements(statement->variant.try_block->statement,depth+2);
      for(auto handler=statement->variant.try_block->handlers;handler;handler=handler->next)trace_coroutine_statements(handler->statement,depth+2);
      break;
    case stmk_if:trace_coroutine_statements(statement->variant.if_stmt.then_statement,depth+2);trace_coroutine_statements(statement->variant.if_stmt.else_statement,depth+2);break;
    case stmk_for:
      trace_coroutine_statements(statement->variant.for_loop.extra_info->initialization,depth+2);
      trace_coroutine_expression(statement->variant.for_loop.extra_info->increment,depth+2);
      trace_coroutine_statements(statement->variant.for_loop.statement,depth+2);break;
    case stmk_while:case stmk_end_test_while:trace_coroutine_statements(statement->variant.loop_statement,depth+2);break;
    case stmk_switch:trace_coroutine_statements(statement->variant.switch_stmt.body_statement,depth+2);break;
    case stmk_init:if(statement->variant.dynamic_init->kind==dik_expression)trace_coroutine_expression(statement->variant.dynamic_init->variant.expression,depth+2);break;
    default:break;
  }
}
void trace_coroutines(a_scope_ptr scope) {
  for(auto routine=scope->routines;routine;routine=routine->next) {
    if(!routine_body_needed(routine))continue;
    auto body=scope_for_routine_or_null(routine);if(!body)continue;
    auto descriptor=coroutine_descriptor(body->assoc_block);if(!descriptor)continue;
    fprintf(stderr,"COROUTINE %s body_generated=%d\n",routine->source_corresp.name?routine->source_corresp.name:"<unnamed>",descriptor->body_generated);
    for(auto variable:{descriptor->promise,descriptor->handle,descriptor->init_await_resume})
      fprintf(stderr," descriptor-variable=%s needed=%d size=%llu\n",trace_id(variable,"v").c_str(),variable?variable_needed(variable):0,variable?(unsigned long long)skip_typerefs(variable->type)->size:0);
    for(auto selected:{descriptor->new_routine,descriptor->delete_routine})
      fprintf(stderr," allocation-routine=%s definition=%lu needed=%d\n",selected && selected->source_corresp.name?selected->source_corresp.name:"<null>",selected?(unsigned long)selected->function_def_number:0,selected?routine_body_needed(selected):0);
    fprintf(stderr," get-return-object:\n");trace_coroutine_expression(descriptor->get_return_object_call,2);
    fprintf(stderr," allocation-failure-return:\n");trace_coroutine_expression(descriptor->alloc_failure_gro_call,2);
    trace_coroutine_statements(body->assoc_block,2);
  }
  names.clear();serial=0;
}
