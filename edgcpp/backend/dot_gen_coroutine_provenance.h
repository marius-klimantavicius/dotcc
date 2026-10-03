// EDG retains the source operand for rendering and executable await triples.
// lower_expr lowers both trees. Only triple-owned temporaries execute here.
// Keep the IL unchanged and prove every discarded temporary use by identity.
std::set<a_variable_ptr> coroutine_descriptive_variables;
std::set<an_expr_node_ptr> coroutine_descriptive_destructors;
std::set<unsigned long long> coroutine_descriptive_regions;
bool coroutine_descriptive_effect(an_expr_node_ptr expression) {
  return current_coroutine && coroutine_descriptive_destructors.count(expression)!=0;
}
bool coroutine_descriptive_region(unsigned long long region) {
  return current_coroutine && coroutine_descriptive_regions.count(region)!=0;
}
a_variable_ptr coroutine_destructor_address(an_expr_node_ptr expression) {
  if(!expression || expression->kind!=enk_operation)return nullptr;
  auto kind=expression->variant.operation.kind;auto child=expression->variant.operation.operands;
  if(!child || child->next)return nullptr;
  if(kind==eok_cast || kind==eok_parens)return coroutine_destructor_address(child);
  if(kind==eok_address_of && child->kind==enk_variable)return node_variable(child);
  return nullptr;
}
bool coroutine_cleanup_transition_at_end(an_expr_node_ptr expression,unsigned long long &region) {
  if(!expression)return false;
  if(expression->kind==enk_object_lifetime)return coroutine_cleanup_transition_at_end(expression->variant.object_lifetime.expr,region);
  if(expression->kind==enk_operation && expression->variant.operation.kind==eok_comma)
    return coroutine_cleanup_transition_at_end(expression->variant.operation.operands->next,region);
  if(expression->kind!=enk_lowered_eh_construct || expression->variant.lowered_eh.kind!=leck_cleanup_state)return false;
  region=expression->variant.lowered_eh.variant.cleanup_region_number;return true;
}
void coroutine_match_descriptive_destructor(an_expr_node_ptr call,unsigned long long predecessor) {
  if(!call || call->kind!=enk_operation || call->variant.operation.kind!=eok_call)return;
  auto routine=call->variant.operation.operands;auto argument=routine?routine->next:nullptr;
  if(!routine || routine->kind!=enk_routine || !argument || argument->next)return;
  auto variable=coroutine_destructor_address(argument);if(!variable)return;
  bool descriptive=false,executable=false;
  for(auto &entry:cleanup_regions) {
    auto &region=entry.second;
    if(region.next==predecessor && region.destructor==routine->variant.routine.ptr && region.handle && region.handle->kind==ck_stack_offset &&
       region.handle->variant.stack_offset.variable==variable && region.handle->variant.stack_offset.offset==0 && region.flags==0) {
      if(coroutine_descriptive_regions.count(entry.first))descriptive=true;else executable=true;
    }
  }
  if(descriptive && executable)unsupported("ambiguous coroutine temporary destruction provenance");
  if(descriptive)coroutine_descriptive_destructors.insert(call);
}
void coroutine_collect_descriptive_destructor(an_expr_node_ptr expression) {
  if(expression->kind!=enk_operation || expression->variant.operation.kind!=eok_comma)return;
  auto first=expression->variant.operation.operands;auto call=first?first->next:nullptr;
  unsigned long long predecessor;
  if(coroutine_cleanup_transition_at_end(first,predecessor))coroutine_match_descriptive_destructor(call,predecessor);
}
void coroutine_validate_descriptive_expression(an_expr_node_ptr expression) {
  if(!expression || coroutine_descriptive_effect(expression))return;
  if(expression->kind==enk_variable && coroutine_descriptive_variables.count(node_variable(expression))) {
    if(getenv("EDG_DOT_TRACE"))fprintf(stderr,"COROUTINE unexpected descriptive variable=%p\n",(void*)node_variable(expression));
    unsupported("descriptive coroutine temporary escapes executable provenance");
  }
  if(expression->kind==enk_operation) {
    for(auto child=expression->variant.operation.operands;child;child=child->next)coroutine_validate_descriptive_expression(child);
  } else if(expression->kind==enk_await || expression->kind==enk_yield) {
    for(auto child=expression->variant.await_info.ready_resume_suspend;child;child=child->next)coroutine_validate_descriptive_expression(child);
  } else if(expression->kind==enk_object_lifetime)coroutine_validate_descriptive_expression(expression->variant.object_lifetime.expr);
  else if(expression->kind==enk_throw && expression->variant.throw_info)coroutine_validate_descriptive_expression(expression->variant.throw_info->expr);
  else if(expression->kind==enk_lowered_eh_construct && expression->variant.lowered_eh.kind==leck_internal_try)
    for(auto child=expression->variant.lowered_eh.variant.try_and_catch_expr;child;child=child->next)coroutine_validate_descriptive_expression(child);
}
void coroutine_validate_descriptive_statement(a_statement_ptr statement) {
  if(!statement)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(statement,selected)){coroutine_validate_descriptive_statement(selected);return;}
  if(statement->kind==stmk_init && coroutine_descriptive_variables.count(statement->variant.dynamic_init->variable))return;
  coroutine_validate_descriptive_expression(statement->expr);
  switch(statement->kind) {
    case stmk_block:
      // Full-expression statements use comma pairs; initialization cleanups
      // use adjacent expression statements with the same EDG state protocol.
      for(auto child=statement->variant.block.statements;child;child=child->next) {
        unsigned long long predecessor;
        if(child->kind==stmk_expr && child->next && child->next->kind==stmk_expr &&
           coroutine_cleanup_transition_at_end(child->expr,predecessor))
          coroutine_match_descriptive_destructor(child->next->expr,predecessor);
      }
      for(auto child=statement->variant.block.statements;child;child=child->next)coroutine_validate_descriptive_statement(child);
      break;
    case stmk_try_block:
      coroutine_validate_descriptive_statement(statement->variant.try_block->statement);
      for(auto handler=statement->variant.try_block->handlers;handler;handler=handler->next) {coroutine_validate_descriptive_statement(handler->statement);}
      break;
    case stmk_if:coroutine_validate_descriptive_statement(statement->variant.if_stmt.then_statement);coroutine_validate_descriptive_statement(statement->variant.if_stmt.else_statement);break;
    case stmk_for:
      coroutine_validate_descriptive_statement(statement->variant.for_loop.extra_info->initialization);
      coroutine_validate_descriptive_expression(statement->variant.for_loop.extra_info->increment);
      coroutine_validate_descriptive_statement(statement->variant.for_loop.statement);break;
    case stmk_while:case stmk_end_test_while:coroutine_validate_descriptive_statement(statement->variant.loop_statement);break;
    case stmk_switch:coroutine_validate_descriptive_statement(statement->variant.switch_stmt.body_statement);break;
    case stmk_init:if(statement->variant.dynamic_init->kind==dik_expression)coroutine_validate_descriptive_expression(statement->variant.dynamic_init->variant.expression);break;
    default:break;
  }
}
void coroutine_prepare_descriptive_operands(a_statement_ptr body) {
  coroutine_descriptive_variables.clear();coroutine_descriptive_destructors.clear();coroutine_descriptive_regions.clear();
  std::set<a_variable_ptr> executable;
  std::set<unsigned long long> descriptive_regions,executable_regions;
  auto region_marker=[](an_expr_node_ptr node,std::set<unsigned long long> &regions) {
    if(node->kind==enk_lowered_eh_construct && node->variant.lowered_eh.kind==leck_cleanup_state)
      regions.insert(node->variant.lowered_eh.variant.cleanup_region_number);
  };
  coroutine_walk_statement(body,[&](an_expr_node_ptr node){
    if(node->kind!=enk_await && node->kind!=enk_yield)return;
    coroutine_walk_expression(node->variant.await_info.operand,[&](an_expr_node_ptr child){
      region_marker(child,descriptive_regions);
      if(child->kind==enk_variable) {
        auto variable=node_variable(child);
        // lower_il.c::make_temporary sets lowering_generated, independently
        // of the front-end compiler_generated flag. User variables never qualify.
        if((variable->compiler_generated || variable->lowering_generated) && variable->storage_class==sc_auto && !variable->is_parameter)
          coroutine_descriptive_variables.insert(variable);
      }
    });
    for(auto triple=node->variant.await_info.ready_resume_suspend;triple;triple=triple->next)
      coroutine_walk_expression(triple,[&](an_expr_node_ptr child){region_marker(child,executable_regions);if(child->kind==enk_variable)executable.insert(node_variable(child));});
  });
  // Storage may be reused by a later executable await. Region identities
  // describe distinct lifetimes even when their stack-offset variable is shared.
  for(auto number:descriptive_regions) {
    if(executable_regions.count(number))continue;
    auto found=cleanup_regions.find(number);if(found==cleanup_regions.end())continue;
    auto handle=found->second.handle;
    if(handle && handle->kind==ck_stack_offset && coroutine_descriptive_variables.count(handle->variant.stack_offset.variable)) {
      if(found->second.flags!=0)unsupported("descriptive coroutine cleanup region flags");
      if(handle->variant.stack_offset.offset!=0)unsupported("descriptive coroutine cleanup subobject offset");
      coroutine_descriptive_regions.insert(number);
    }
  }
  for(auto variable:executable)coroutine_descriptive_variables.erase(variable);
  coroutine_walk_statement(body,[](an_expr_node_ptr node){coroutine_collect_descriptive_destructor(node);});
  if(getenv("EDG_DOT_TRACE")) {
    for(auto &entry:cleanup_regions) {
      auto handle=entry.second.handle;
      fprintf(stderr,"COROUTINE provenance region=%llu parent=%llu original=%d executable=%d ignored=%d variable=%p\n",entry.first,entry.second.next,(int)descriptive_regions.count(entry.first),(int)executable_regions.count(entry.first),(int)coroutine_descriptive_regions.count(entry.first),handle && handle->kind==ck_stack_offset?(void*)handle->variant.stack_offset.variable:nullptr);
    }
    fprintf(stderr,"COROUTINE provenance ignored-variables=%zu ignored-destructors=%zu\n",coroutine_descriptive_variables.size(),coroutine_descriptive_destructors.size());
  }
  coroutine_validate_descriptive_statement(body);
  cleanup_capture_variables.erase(std::remove_if(cleanup_capture_variables.begin(),cleanup_capture_variables.end(),
    [](a_variable_ptr variable){return coroutine_descriptive_variables.count(variable)!=0;}),cleanup_capture_variables.end());
}
