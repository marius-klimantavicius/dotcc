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
  // First bounded view keeps every surviving cleanup boundary at zero. Real
  // source cleanup regions need explicit lifetime-edge remapping before their
  // handler regions can be moved across the implicit coroutine catch.
  for(auto &entry:cleanup_regions) {
    auto &region=entry.second;auto routine=region.destructor;
    if(region.flags || region.next!=null_eh_region_number || !routine ||
       !routine->compiler_generated || !routine->source_corresp.name ||
       string(routine->source_corresp.name)!="__destroy_exception_object")
      unsupported("coroutine function-try nontrivial cleanup boundary");
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
