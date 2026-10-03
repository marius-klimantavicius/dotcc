// Source-facing statement view. EDG's lowered IL remains authoritative; this
// view only chooses legal C# scopes and preserves C/C++ transfer targets.
struct source_control_region { a_statement_ptr branch, owner; };
std::set<a_statement_ptr> source_flat_controls;
std::map<a_label_ptr,std::vector<source_control_region>> source_label_regions;
std::vector<std::pair<a_label_ptr,std::vector<source_control_region>>> source_goto_regions;
std::set<a_variable_ptr> source_lexical_variables, source_declared_variables;
std::map<a_statement_ptr,a_dynamic_init_ptr> source_for_initializers;
bool source_lexical_safe=false;
struct source_control_context { a_label_ptr break_target=nullptr, continue_target=nullptr; bool loop=false; };
std::vector<source_control_context> source_control_stack;
bool source_initializer_mentions(an_expr_node_ptr e,a_variable_ptr v) {
  if(!e)return false;
  if(e->kind==enk_variable)return node_variable(e)==v;
  if(e->kind==enk_constant) {auto c=node_constant(e);return c->kind==ck_address && c->variant.address.kind==abk_variable && c->variant.address.variant.variable==v;}
  if(e->kind==enk_operation)for(auto p=e->variant.operation.operands;p;p=p->next)if(source_initializer_mentions(p,v))return true;
  if(e->kind==enk_object_lifetime)return source_initializer_mentions(e->variant.object_lifetime.expr,v);
  return false;
}
void source_control_scan(a_statement_ptr s,std::vector<source_control_region> regions) {
  if(!s)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){source_control_scan(selected,regions);return;}
  auto child=[&](a_statement_ptr node,a_statement_ptr owner) {
    auto nested=regions;if(node)nested.push_back({node,owner});source_control_scan(node,nested);
  };
  switch(s->kind) {
    case stmk_label:source_label_regions[s->variant.label.ptr]=regions;break;
    case stmk_goto:
      source_goto_regions.push_back({s->variant.label.ptr,regions});
      break;
    case stmk_init: {
      auto d=s->variant.dynamic_init;auto v=d->variable;
      if(v && v->source_corresp.name && !v->lowering_generated && !aggregate(v->type) &&
         !local_offsets.count(v) && !global_offsets.count(v) && !tls_offsets.count(v) &&
         (d->kind==dik_expression || d->kind==dik_constant || d->kind==dik_zero) &&
         !(d->kind==dik_expression && source_initializer_mentions(d->variant.expression,v))) {
        source_lexical_variables.insert(v);
        if(s->next && s->next->kind==stmk_for && !s->next->variant.for_loop.extra_info->initialization)
          source_for_initializers[s->next]=d;
      }
      break;
    }
    case stmk_block:
      if(s!=current_function_scope->assoc_block && s->variant.block.extra_info->assoc_scope && !s->variant.block.extra_info->implicit_scope_not_allowed)regions.push_back({s,s});
      for(auto p=s->variant.block.statements;p;p=p->next)source_control_scan(p,regions);
      break;
    case stmk_if:child(s->variant.if_stmt.then_statement,s);child(s->variant.if_stmt.else_statement,s);break;
    case stmk_for:
      source_control_scan(s->variant.for_loop.extra_info->initialization,regions);
      child(s->variant.for_loop.statement,s);break;
    case stmk_while:case stmk_end_test_while:child(s->variant.loop_statement,s);break;
    case stmk_switch:child(s->variant.switch_stmt.body_statement,s);break;
    case stmk_try_block:
      source_lexical_safe=false;
      source_control_scan(s->variant.try_block->statement,regions);
      for(auto h=s->variant.try_block->handlers;h;h=h->next)source_control_scan(h->statement,regions);
      break;
    default:break;
  }
}
void prepare_source_control(a_statement_ptr body) {
  source_flat_controls.clear();source_label_regions.clear();source_goto_regions.clear();
  source_lexical_variables.clear();source_declared_variables.clear();source_for_initializers.clear();source_control_stack.clear();
  source_lexical_safe=source_jump_sites.empty() && cleanup_regions.empty();
  source_control_scan(body,{});
  for(auto &jump:source_goto_regions) {
    auto target=source_label_regions.find(jump.first);if(target==source_label_regions.end())continue;
    for(auto &region:target->second) {
      bool inside=false;for(auto &origin:jump.second)if(origin.branch==region.branch){inside=true;break;}
      if(!inside)source_flat_controls.insert(region.owner);
    }
  }
  if(!source_lexical_safe){source_lexical_variables.clear();source_for_initializers.clear();}
}
bool source_structured(a_statement_ptr s) {return source_jump_sites.empty() && !source_flat_controls.count(s);}
a_label_ptr source_loop_continue(a_statement_ptr s) {
  if(!s)return nullptr;
  if(s->kind==stmk_label && s->variant.label.ptr->continue_label)return s->variant.label.ptr;
  if(s->kind==stmk_block)for(auto p=s->variant.block.statements;p;p=p->next)if(auto l=source_loop_continue(p))return l;
  if(s->kind==stmk_if) {
    if(auto l=source_loop_continue(s->variant.if_stmt.then_statement))return l;
    return source_loop_continue(s->variant.if_stmt.else_statement);
  }
  return nullptr;
}
source_control_context source_context(a_statement_ptr s,a_statement_ptr body,bool loop) {
  source_control_context c;c.loop=loop;
  if(s->next && s->next->kind==stmk_label && s->next->variant.label.ptr->break_label)c.break_target=s->next->variant.label.ptr;
  if(loop)c.continue_target=source_loop_continue(body);
  return c;
}
string source_transfer(a_label_ptr label) {
  for(auto it=source_control_stack.rbegin();it!=source_control_stack.rend();++it) {
    if(it->break_target==label)return "break;";
    break;
  }
  for(auto it=source_control_stack.rbegin();it!=source_control_stack.rend();++it) {
    if(!it->loop)continue;
    if(it->continue_target==label)return "continue;";
    break;
  }
  return "goto "+name(label,"label")+";";
}
// Switch labels nested in control constructs (e.g. Duff's device) require the
// original dispatch form. A C# switch may otherwise preserve source sections.
bool source_switch_items(a_statement_ptr s,std::vector<a_statement_ptr> &items) {
  if(!s)return true;
  if(s->kind==stmk_block) {
    for(auto p=s->variant.block.statements;p;p=p->next)if(!source_switch_items(p,items))return false;
    return true;
  }
  if(s->kind==stmk_switch_case){items.push_back(s);return true;}
  auto contains_case=[&](auto &&self,a_statement_ptr node)->bool {
    if(!node || node->kind==stmk_switch)return false;
    if(node->kind==stmk_switch_case)return true;
    bool nested=false;
    if(node->kind==stmk_block){for(auto p=node->variant.block.statements;p;p=p->next)nested=self(self,p)||nested;}
    if(node->kind==stmk_if)nested=self(self,node->variant.if_stmt.then_statement)|self(self,node->variant.if_stmt.else_statement);
    if(node->kind==stmk_for)nested=self(self,node->variant.for_loop.statement);
    if(node->kind==stmk_while || node->kind==stmk_end_test_while)nested=self(self,node->variant.loop_statement);
    if(nested)source_flat_controls.insert(node);
    return nested;
  };
  if(contains_case(contains_case,s)){source_flat_controls.insert(s);return false;}
  items.push_back(s);return true;
}
string source_initializer_value(a_dynamic_init_ptr d) {
  if(d->kind==dik_zero)return "default";
  if(d->kind==dik_constant)return enum_boundary_value(d->variable->type,d->variant.constant.ptr->type,constant(d->variant.constant.ptr));
  auto p=plan_source_value(d->variant.expression);output+=p.prefix;return enum_boundary_value(d->variable->type,d->variant.expression->type,p.value);
}

bool source_statement_terminates(a_statement_ptr s) {
  if(!s)return false;
  if(s->kind==stmk_return || s->kind==stmk_goto)return true;
  if(s->kind==stmk_block) {
    a_statement_ptr last=nullptr;
    for(auto p=s->variant.block.statements;p;p=p->next)if(p->kind!=stmk_decl && p->kind!=stmk_empty)last=p;
    return source_statement_terminates(last);
  }
  if(s->kind==stmk_if)return source_statement_terminates(s->variant.if_stmt.then_statement) && source_statement_terminates(s->variant.if_stmt.else_statement);
  return false;
}

a_type_ptr source_switch_enum_context(an_expr_node_ptr expression) {
  if(!expression)return nullptr;
  auto t=skip_typerefs(expression->type);
  if(t->kind==tk_integer && (t->variant.integer.enum_type || t->variant.integer.enum_info.affiliated_type))return t;
  if(expression->kind==enk_operation && (expression->variant.operation.kind==eok_cast || expression->variant.operation.kind==eok_parens))
    return source_switch_enum_context(expression->variant.operation.operands);
  return expression->type;
}
