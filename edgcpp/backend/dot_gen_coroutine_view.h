// Backend-owned statement views. Never splice or rewrite EDG statement lists,
// try supplements, cleanup regions, object lifetimes, or needed flags.
std::map<a_statement_ptr,std::vector<a_statement_ptr>> coroutine_block_views;
std::vector<a_statement_ptr> coroutine_block_children(a_statement_ptr block) {
  auto found=coroutine_block_views.find(block);
  if(found!=coroutine_block_views.end())return found->second;
  std::vector<a_statement_ptr> result;
  for(auto child=block->variant.block.statements;child;child=child->next)result.push_back(child);
  return result;
}
void coroutine_flatten_blocks(a_statement_ptr node,std::vector<a_statement_ptr> &result) {
  if(node->kind==stmk_block) {
    for(auto child=node->variant.block.statements;child;child=child->next)coroutine_flatten_blocks(child,result);
  } else result.push_back(node);
}
bool coroutine_view_contains(an_expr_node_ptr expression,an_expr_node_ptr needle) {
  if(!expression)return false;
  if(expression==needle)return true;
  if(expression->kind==enk_operation) {
    for(auto child=expression->variant.operation.operands;child;child=child->next)
      if(coroutine_view_contains(child,needle))return true;
  } else if(expression->kind==enk_object_lifetime)
    return coroutine_view_contains(expression->variant.object_lifetime.expr,needle);
  return false;
}
bool coroutine_view_handler_call(a_statement_ptr node,an_expr_node_ptr needle) {
  if(coroutine_view_contains(node->expr,needle))return true;
  if(node->kind==stmk_block) {
    for(auto child=node->variant.block.statements;child;child=child->next)
      if(coroutine_view_handler_call(child,needle))return true;
  }
  return false;
}
bool coroutine_view_epilogue(a_statement_ptr node,a_try_supplement_ptr block) {
  return node->kind==stmk_expr && node->expr && node->expr->kind==enk_lowered_eh_construct &&
    node->expr->variant.lowered_eh.kind==leck_try_epilogue &&
    node->expr->variant.lowered_eh.variant.epilogue_try_block==block;
}
void coroutine_prepare_function_try_view(a_statement_ptr root,a_coroutine_descr_ptr descriptor) {
  coroutine_block_views.clear();
  std::vector<a_statement_ptr> outer;coroutine_flatten_blocks(root,outer);
  a_statement_ptr source=nullptr,epilogue=nullptr;
  for(auto node:outer)if(node->kind==stmk_try_block && node->variant.try_block->is_function_try_block) {
    if(source)unsupported("multiple coroutine source function-try blocks");
    source=node;
  }
  if(!source)return;
  // Keep the source/implicit catch boundary at zero. A catch parameter may
  // have its exact retained destructor region linked to that handler's own
  // release entry (lower_eh.c::begin_catch_clause inserts this entry first).
  // Other source lifetime regions still require a separately proven view.
  std::map<a_variable_ptr,std::pair<a_routine_ptr,unsigned long long>> parameters;
  for(auto handler=source->variant.try_block->handlers;handler;handler=handler->next) {
    if(!handler->parameter || !handler->dynamic_init || !handler->dynamic_init->destructor)continue;
    std::vector<a_statement_ptr> statements;coroutine_flatten_blocks(handler->statement,statements);
    if(statements.empty())unsupported("coroutine function-try catch-parameter cleanup provenance");
    auto first=statements.front();
    if(first->kind!=stmk_expr || !first->expr || first->expr->kind!=enk_lowered_eh_construct ||
       first->expr->variant.lowered_eh.kind!=leck_cleanup_state)
      unsupported("coroutine function-try catch-parameter cleanup provenance");
    auto release_index=first->expr->variant.lowered_eh.variant.cleanup_region_number;
    auto release=cleanup_regions.find(release_index);
    if(release==cleanup_regions.end() || release->second.flags ||
       release->second.next!=null_eh_region_number || !release->second.destructor ||
       !release->second.destructor->compiler_generated || !release->second.destructor->source_corresp.name ||
       string(release->second.destructor->source_corresp.name)!="__destroy_exception_object")
      unsupported("coroutine function-try catch-parameter release provenance");
    parameters[handler->parameter]={handler->dynamic_init->destructor,release_index};
  }
  for(auto &entry:cleanup_regions) {
    auto &region=entry.second;auto routine=region.destructor;
    bool release=!region.flags && region.next==null_eh_region_number && routine &&
      routine->compiler_generated && routine->source_corresp.name &&
      string(routine->source_corresp.name)=="__destroy_exception_object";
    if(release)continue;
    bool parameter=false;
    if(!region.flags && region.handle && region.handle->kind==ck_stack_offset &&
       region.handle->variant.stack_offset.offset==0) {
      auto found=parameters.find(region.handle->variant.stack_offset.variable);
      parameter=found!=parameters.end() && region.destructor==found->second.first &&
        region.next==found->second.second;
    }
    if(!parameter)unsupported("coroutine function-try nontrivial cleanup boundary");
  }
  for(auto node:outer)if(coroutine_view_epilogue(node,source->variant.try_block)) {
    if(epilogue)unsupported("multiple coroutine source try epilogues");
    epilogue=node;
  }
  if(!epilogue || source->variant.try_block->statement->kind!=stmk_block)
    unsupported("coroutine function-try source-body boundary");
  std::vector<a_statement_ptr> replacement;
  coroutine_flatten_blocks(source->variant.try_block->statement,replacement);
  a_statement_ptr implicit=nullptr;
  for(auto node:replacement)if(node->kind==stmk_try_block) {
    bool match=false;
    for(auto handler=node->variant.try_block->handlers;handler;handler=handler->next)
      match|=coroutine_view_handler_call(handler->statement,descriptor->unhandled_exception_call);
    if(match) {
      if(implicit)unsupported("multiple coroutine implicit handlers");
      implicit=node;
    }
  }
  if(!implicit || implicit->variant.try_block->statement->kind!=stmk_block)
    unsupported("coroutine function-try implicit-handler boundary");
  std::vector<a_statement_ptr> body;
  coroutine_flatten_blocks(implicit->variant.try_block->statement,body);
  if(body.size()<2 || body.front()->kind!=stmk_expr || body.front()->expr!=descriptor->initial_suspend_call)
    unsupported("coroutine function-try initial-await boundary");
  auto initial=body.front();body.erase(body.begin());
  std::vector<a_statement_ptr> replacement_root;
  for(auto node:outer) {
    if(node==source)replacement_root.insert(replacement_root.end(),replacement.begin(),replacement.end());
    else if(node!=epilogue)replacement_root.push_back(node);
  }
  coroutine_block_views[root]=replacement_root;
  coroutine_block_views[source->variant.try_block->statement]=body;
  coroutine_block_views[implicit->variant.try_block->statement]={initial,source,epilogue};
}
