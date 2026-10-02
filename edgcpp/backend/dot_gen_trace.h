// Optional diagnostic trace of the retained, structurally lowered lifetime IL.
// Included inside the backend's anonymous namespace; IDs are deterministic.
string trace_id(const void *p, const char *prefix) { return p ? name(p, prefix) : "null"; }
void trace_lifetime(an_object_lifetime_ptr l, int depth) {
  if (!l) return;
  fprintf(stderr, "%*slifetime %s kind=%d parent=%s\n", depth, "", trace_id(l,"L").c_str(),
          l->kind, trace_id(l->parent_lifetime,"L").c_str());
  for (auto d=l->destructions; d; d=d->next_in_destruction_list)
    fprintf(stderr, "%*s destruction %s kind=%d variable=%s destructor=%s partial=%d ctor=%d descriptor=%s flags=%d,%d,%d,%d\n", depth,"",
            trace_id(d,"D").c_str(),d->kind,trace_id(d->variable,"v").c_str(),
            d->destructor ? d->destructor->source_corresp.name : "null",
            d->destruction_is_for_partially_constructed_aggregate,d->is_constructor_init,
            d->destructible_entity_descr ? "present" : "absent", d->is_freeing_of_storage_on_exception,
            d->is_guard_var_for_local_static_var_init,d->is_vla_deallocation,d->unordered);
  for(auto child=l->child_lifetime;child;child=child->next) trace_lifetime(child,depth+2);
}
void trace_expr(an_expr_node_ptr e,int depth) {
  if (!e) return;
  fprintf(stderr,"%*sexpr kind=%d",depth,"",e->kind);
  if(e->kind==enk_lowered_eh_construct) {
    auto &eh=e->variant.lowered_eh;
    fprintf(stderr," EH=%d",eh.kind);
    if(eh.kind==leck_cleanup_state || eh.kind==leck_unreachable_cleanup_state)
      fprintf(stderr," cleanup=%lu",(unsigned long)eh.variant.cleanup_region_number);
  } else if(e->kind==enk_routine) fprintf(stderr," routine=%s",e->variant.routine.ptr->source_corresp.name);
  else if(e->kind==enk_variable) fprintf(stderr," variable=%s",trace_id(node_variable(e),"v").c_str());
  else if(e->kind==enk_operation) fprintf(stderr," op=%d order=%s",e->variant.operation.kind,
      e->variant.operation.eval_right_to_left?"rtl":(e->variant.operation.eval_left_to_right?"ltr":"canonical"));
  fprintf(stderr,"\n");
  if(e->kind==enk_operation)
    for(auto a=e->variant.operation.operands;a;a=a->next) trace_expr(a,depth+2);
  if(e->kind==enk_throw && e->variant.throw_info)trace_expr(e->variant.throw_info->expr,depth+2);
  if(e->kind==enk_object_lifetime) {
    trace_lifetime(e->variant.object_lifetime.ptr,depth+2);
    trace_expr(e->variant.object_lifetime.expr,depth+2);
  }
}
void trace_statement(a_statement_ptr s,int depth) {
  if(!s)return;
  fprintf(stderr,"%*sstatement kind=%d\n",depth,"",s->kind);
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){trace_statement(selected,depth+2);return;}
  trace_expr(s->expr,depth+2);
  switch(s->kind) {
    case stmk_try_block:
      trace_statement(s->variant.try_block->statement,depth+2);
      for(auto h=s->variant.try_block->handlers;h;h=h->next)trace_statement(h->statement,depth+2);
      break;
    case stmk_block:
      trace_lifetime(s->variant.block.extra_info->lifetime,depth+2);
      for(auto p=s->variant.block.statements;p;p=p->next)trace_statement(p,depth+2);
      break;
    case stmk_switch:trace_statement(s->variant.switch_stmt.body_statement,depth+2);break;
    case stmk_if: trace_statement(s->variant.if_stmt.then_statement,depth+2);trace_statement(s->variant.if_stmt.else_statement,depth+2);break;
    case stmk_while: case stmk_end_test_while:trace_statement(s->variant.loop_statement,depth+2);break;
    case stmk_for:trace_statement(s->variant.for_loop.extra_info->initialization,depth+2);trace_expr(s->variant.for_loop.extra_info->increment,depth+2);trace_statement(s->variant.for_loop.statement,depth+2);break;
    default:break;
  }
}
void trace_program(a_scope_ptr scope) {
  for(auto r=scope->routines;r;r=r->next) {
    if(!routine_body_needed(r))continue;
    if(r->function_def_number==NULL_function_def_number)continue;
    auto s=scope_for_routine_or_null(r);if(!s)continue;
    fprintf(stderr,"routine %s\n",r->source_corresp.name);
    trace_lifetime(s->lifetime,2);
    trace_statement(s->assoc_block,2);
  }
  names.clear(); serial=0;
}
