// Address-taking of a POD aggregate prvalue needs storage in the calling
// routine, not in a generated expression helper's stack frame. EDG already
// gives nontrivial C++ temporaries explicit construction/cleanup storage.
std::map<an_expr_node_ptr,unsigned long long> materialization_offsets;
std::set<an_expr_node_ptr> materialization_suppressed;
void reset_materialization() {
  materialization_offsets.clear();materialization_suppressed.clear();
}
void require_materialized_location(an_expr_node_ptr e) {
  if(!e)return;
  if(e->kind==enk_operation) {
    auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;
    // Retain the containing object's identity for nested subobjects rather
    // than copying just its array member to unrelated storage.
    if(op==eok_dot_field || op==eok_parens || op==eok_lvalue_adjust) {
      require_materialized_location(a);return;
    }
    if(op==eok_indirect || op==eok_ref_indirect || op==eok_points_to_field)return;
  }
  if(e->is_lvalue || e->kind==enk_variable || !aggregate(e->type))return;
  if(materialization_offsets.count(e))return;
  auto t=skip_typerefs(e->type);
  unsigned long long alignment=alignment_of_type(t);
  if(!t->size || !alignment || alignment>4096 || (alignment & (alignment-1)))
    unsupported("aggregate prvalue storage layout");
  (void)type(t);
  frame_alignment=std::max(frame_alignment,alignment);
  frame_size=(frame_size+alignment-1)&~(alignment-1);
  materialization_offsets[e]=frame_size;frame_size+=t->size;
  if(frame_size+frame_alignment>64*1024*1024)unsupported("local frame exceeds 64 MiB");
}
void collect_materialization(an_expr_node_ptr e) {
  if(!e)return;
  if(e->kind==enk_operation) {
    auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;
    if(op==eok_array_to_pointer || op==eok_address_of || op==eok_reference_to ||
       op==eok_lvalue_adjust || op==eok_bassign)require_materialized_location(a);
    for(;a;a=a->next)collect_materialization(a);
  } else if(e->kind==enk_object_lifetime)collect_materialization(e->variant.object_lifetime.expr);
  else if(e->kind==enk_throw && e->variant.throw_info)collect_materialization(e->variant.throw_info->expr);
  else if(e->kind==enk_lowered_eh_construct && e->variant.lowered_eh.kind==leck_internal_try)
    for(auto a=e->variant.lowered_eh.variant.try_and_catch_expr;a;a=a->next)collect_materialization(a);
}
void collect_materialization(a_statement_ptr s) {
  if(!s)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){collect_materialization(selected);return;}
  collect_materialization(s->expr);
  switch(s->kind) {
    case stmk_try_block:
      collect_materialization(s->variant.try_block->statement);
      for(auto h=s->variant.try_block->handlers;h;h=h->next)collect_materialization(h->statement);
      break;
    case stmk_block:for(auto p=s->variant.block.statements;p;p=p->next)collect_materialization(p);break;
    case stmk_switch:collect_materialization(s->variant.switch_stmt.body_statement);break;
    case stmk_if:collect_materialization(s->variant.if_stmt.then_statement);collect_materialization(s->variant.if_stmt.else_statement);break;
    case stmk_while:case stmk_end_test_while:collect_materialization(s->variant.loop_statement);break;
    case stmk_for:
      collect_materialization(s->variant.for_loop.extra_info->initialization);
      collect_materialization(s->variant.for_loop.statement);
      collect_materialization(s->variant.for_loop.extra_info->increment);break;
    case stmk_init:if(s->variant.dynamic_init->kind==dik_expression)collect_materialization(s->variant.dynamic_init->variant.expression);break;
    default:break;
  }
}
void prepare_materialization(a_statement_ptr body) {
  reset_materialization();collect_materialization(body);
}
bool materialization_captures(an_expr_node_ptr e) {
  if(!e)return false;
  if(materialization_offsets.count(e))return true;
  if(e->kind==enk_operation) {
    for(auto a=e->variant.operation.operands;a;a=a->next)if(materialization_captures(a))return true;
  } else if(e->kind==enk_object_lifetime)return materialization_captures(e->variant.object_lifetime.expr);
  else if(e->kind==enk_throw && e->variant.throw_info)return materialization_captures(e->variant.throw_info->expr);
  else if(e->kind==enk_lowered_eh_construct && e->variant.lowered_eh.kind==leck_internal_try) {
    for(auto a=e->variant.lowered_eh.variant.try_and_catch_expr;a;a=a->next)if(materialization_captures(a))return true;
  }
  return false;
}
// Accept an already planned value so callers that have lifted comma/branch
// effects do not evaluate the original expression a second time for its address.
string materialized_value_location(an_expr_node_ptr e,const string &value) {
  auto found=materialization_offsets.find(e);
  if(found==materialization_offsets.end())unsupported("missing aggregate materialization slot");
  string ty=type(e->type),key="materialize:"+ty,id;
  auto helper=assignment_helpers.find(key);
  if(helper!=assignment_helpers.end())id=helper->second;
  else {
    id="__edg_materialize"+std::to_string(++serial);assignment_helpers[key]=id;
    helper_output+="// dot-gen:member-begin\nprivate static "+ty+"* "+id+"("+ty+" value, "+ty+"* storage) { *storage=value;return storage; }\n// dot-gen:member-end\n";
  }
  return "(*("+id+"("+value+", ("+ty+"*)(__edg_frame + "+std::to_string(found->second)+"))))";
}

string materialized_location(an_expr_node_ptr e) {
  if(!materialization_offsets.count(e) || materialization_suppressed.count(e))return "";
  materialization_suppressed.insert(e);
  string value=location(e);
  materialization_suppressed.erase(e);
  return materialized_value_location(e,value);
}
