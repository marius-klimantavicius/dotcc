// Source nonlocal-jump continuations. Ordinary EDG statement emission already
// flattens blocks/loops to labels, so a dispatcher can resume inside that same
// CLR try without crossing a protected-region boundary.
bool source_jumps_used=false;
std::map<an_expr_node_ptr,unsigned> source_jump_sites;
std::map<an_expr_node_ptr,an_expr_node_ptr> source_jump_contexts;
std::set<an_expr_node_ptr> source_jump_emitted;
bool source_jump_protected=false,source_jump_variable_storage=false;

int source_jump_routine(a_routine_ptr r) {
  if(!r || r->function_def_number!=NULL_function_def_number || !r->source_corresp.name)return 0;
  string symbol=r->source_corresp.name;
  int kind=symbol=="__edg_c_setjmp"?1:symbol=="__edg_c_longjmp"?2:0;
  if(!kind)return 0;
  auto rt=skip_typerefs(r->type);auto p=rt->variant.routine.extra_info->param_type_list;
  if(type(rt->variant.routine.return_type)!=(kind==1?"int":"void") ||
     !p || type(p->type)!="void*" || rt->variant.routine.extra_info->has_ellipsis ||
     (kind==1?p->next!=nullptr:(!p->next || p->next->next || type(p->next->type)!="int")))
    unsupported("source nonlocal jump provider ABI");
  return kind;
}
int source_jump_call_kind(an_expr_node_ptr e) {
  if(!e || e->kind!=enk_operation || e->variant.operation.kind!=eok_call)return 0;
  auto f=e->variant.operation.operands;
  return f && f->kind==enk_routine?source_jump_routine(f->variant.routine.ptr):0;
}
void scan_source_jumps(an_expr_node_ptr e,std::vector<an_expr_node_ptr> &sites) {
  if(!e)return;
  if(e->kind==enk_operation) {
    int kind=source_jump_call_kind(e);
    if(kind) {source_jumps_used=true;if(kind==1)sites.push_back(e);}
    auto a=e->variant.operation.operands;
    if(e->variant.operation.kind==eok_call && a && a->kind==enk_routine) {
      auto r=a->variant.routine.ptr;
      if(r->compiler_generated && r->source_corresp.name && string(r->source_corresp.name)=="__vla_alloc")
        source_jump_variable_storage=true;
    }
    for(;a;a=a->next)scan_source_jumps(a,sites);
  } else if(e->kind==enk_object_lifetime)scan_source_jumps(e->variant.object_lifetime.expr,sites);
  else if(e->kind==enk_throw && e->variant.throw_info)scan_source_jumps(e->variant.throw_info->expr,sites);
  else if(e->kind==enk_lowered_eh_construct && e->variant.lowered_eh.kind==leck_internal_try) {
    source_jump_protected=true;
    for(auto a=e->variant.lowered_eh.variant.try_and_catch_expr;a;a=a->next)scan_source_jumps(a,sites);
  }
}
an_expr_node_ptr source_jump_strip(an_expr_node_ptr e) {
  while(e && e->kind==enk_operation && (e->variant.operation.kind==eok_parens || e->variant.operation.kind==eok_cast))
    e=e->variant.operation.operands;
  return e;
}
bool source_jump_shape(an_expr_node_ptr e,an_expr_node_ptr site,bool controlling) {
  e=source_jump_strip(e);
  if(e==site)return true;
  if(!controlling || !e || e->kind!=enk_operation)return false;
  auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;
  if(op==eok_not)return source_jump_strip(a)==site;
  if(op==eok_eq || op==eok_ne || op==eok_lt || op==eok_le || op==eok_gt || op==eok_ge) {
    auto b=a?a->next:nullptr;
    a=source_jump_strip(a);b=source_jump_strip(b);
    return (a==site && b && b->kind==enk_constant && node_constant(b)->kind==ck_integer) ||
           (b==site && a && a->kind==enk_constant && node_constant(a)->kind==ck_integer);
  }
  return false;
}
void collect_source_jump_expression(an_expr_node_ptr e,bool allowed,bool controlling,bool prepare) {
  std::vector<an_expr_node_ptr> sites;scan_source_jumps(e,sites);
  if(!prepare || sites.empty())return;
  if(!allowed || sites.size()!=1 || !source_jump_shape(e,sites.front(),controlling))
    unsupported("setjmp requires a supported controlling or standalone expression");
  auto site=sites.front();
  if(!source_jump_sites.count(site)) {
    unsigned id=(unsigned)source_jump_sites.size()+1;source_jump_sites.emplace(site,id);
  }
  source_jump_contexts[e]=site;
}
void collect_source_jump_statements(a_statement_ptr s,bool prepare) {
  if(!s)return;
  diagnostic_scope diagnostic(s->position);
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){collect_source_jump_statements(selected,prepare);return;}
  bool controlling=s->kind==stmk_if || s->kind==stmk_switch || s->kind==stmk_while || s->kind==stmk_end_test_while || s->kind==stmk_for;
  collect_source_jump_expression(s->expr,controlling || s->kind==stmk_expr,controlling,prepare);
  switch(s->kind) {
    case stmk_try_block:
      source_jump_protected=true;
      collect_source_jump_statements(s->variant.try_block->statement,prepare);
      for(auto h=s->variant.try_block->handlers;h;h=h->next)collect_source_jump_statements(h->statement,prepare);
      break;
    case stmk_block:for(auto p=s->variant.block.statements;p;p=p->next)collect_source_jump_statements(p,prepare);break;
    case stmk_switch:collect_source_jump_statements(s->variant.switch_stmt.body_statement,prepare);break;
    case stmk_if:
      collect_source_jump_statements(s->variant.if_stmt.then_statement,prepare);
      collect_source_jump_statements(s->variant.if_stmt.else_statement,prepare);break;
    case stmk_while:case stmk_end_test_while:collect_source_jump_statements(s->variant.loop_statement,prepare);break;
    case stmk_for:
      collect_source_jump_statements(s->variant.for_loop.extra_info->initialization,prepare);
      collect_source_jump_statements(s->variant.for_loop.statement,prepare);
      collect_source_jump_expression(s->variant.for_loop.extra_info->increment,false,false,prepare);break;
    case stmk_init:
      if(s->variant.dynamic_init->kind==dik_expression)
        collect_source_jump_expression(s->variant.dynamic_init->variant.expression,false,false,prepare);
      break;
    default:break;
  }
}
void prepare_source_jumps(a_statement_ptr body) {
  source_jump_sites.clear();source_jump_contexts.clear();source_jump_emitted.clear();
  source_jump_protected=false;source_jump_variable_storage=false;
  collect_source_jump_statements(body,true);
  if(!source_jump_sites.empty() && (source_jump_protected || !cleanup_regions.empty()))
    unsupported("setjmp continuation through C++ protected/lifetime regions");
  if(!source_jump_sites.empty() && source_jump_variable_storage)
    unsupported("setjmp continuation with variable-length storage");
}
string source_jump_prefix(an_expr_node_ptr root) {
  auto found=source_jump_contexts.find(root);
  if(found==source_jump_contexts.end())return "";
  auto call=found->second;auto arg=call->variant.operation.operands->next;
  if(!arg || arg->next)unsupported("setjmp argument ABI");
  auto site=source_jump_sites.at(call);source_jump_emitted.insert(call);
  return "__edg_jump_result = global::Edg.Runtime.NonlocalJump.Arm(&__edg_jump_activation, (void*)("+expr(arg)+"), "+std::to_string(site)+"U);\n"
         "__edg_jump_site"+std::to_string(site)+": ;\n";
}
string source_jump_expression(an_expr_node_ptr e) {
  int kind=source_jump_call_kind(e);
  if(!kind)return "";
  if(kind==1) {
    if(!source_jump_sites.count(e) || !source_jump_emitted.count(e))unsupported("unclaimed setjmp continuation");
    return "__edg_jump_result";
  }
  auto a=e->variant.operation.operands->next;
  if(!a || !a->next || a->next->next)unsupported("longjmp argument ABI");
  return "global::Edg.Runtime.NonlocalJump.LongJump((void*)("+expr(a)+"), "+expr(a->next)+")";
}
string source_jump_dispatch_prefix() {
  if(source_jump_sites.empty())return "";
  string code="global::Edg.Runtime.NonlocalJump.Activation __edg_jump_activation = default;\nint __edg_jump_pc=0, __edg_jump_result=0;\n"
    "global::Edg.Runtime.NonlocalJump.Enter(&__edg_jump_activation, "+std::to_string(source_jump_sites.size())+"U);\n"
    "try { for (;;) { try { switch (__edg_jump_pc) { case 0: break;\n";
  for(auto &site:source_jump_sites)code+="case "+std::to_string(site.second)+": goto __edg_jump_site"+std::to_string(site.second)+";\n";
  return code+"default: throw new global::System.InvalidProgramException(\"Invalid nonlocal continuation\"); }\n";
}
string source_jump_dispatch_suffix() {
  if(source_jump_sites.empty())return "";
  if(source_jump_emitted.size()!=source_jump_sites.size())unsupported("unemitted setjmp continuation");
  return string(function_returns_void?"return;\n":"throw new global::System.InvalidProgramException(\"Function fell through nonlocal continuation\");\n")+
    "} catch (global::Edg.Runtime.NonlocalJump.Transfer __edg_jump) when (global::Edg.Runtime.NonlocalJump.Matches(__edg_jump, &__edg_jump_activation)) {\n"
    "__edg_jump_pc=(int)__edg_jump.Site;__edg_jump_result=__edg_jump.Value;\n"
    "} } } finally {global::Edg.Runtime.NonlocalJump.Leave(&__edg_jump_activation);}\n";
}
string source_jump_exclusion(const string &error) {
  return source_jumps_used?"!("+error+" is global::Edg.Runtime.NonlocalJump.Transfer)":error+" != null";
}
