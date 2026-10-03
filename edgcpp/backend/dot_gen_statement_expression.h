// c_gen_be.c emits GNU ({ ... }) blocks with a final stmk_stmt_expr_result.
// A typed helper preserves caller storage via explicit pointers. Only contained
// control flow is admitted here; transfers out of the enclosing function and
// lifetime-bearing blocks need the later control-flow/lifetime IR migration.
struct statement_expression_scan {
  std::vector<a_variable_ptr> captures;
  std::set<a_label_ptr> labels, targets;
};
void scan_statement_expression_body(a_statement_ptr s,statement_expression_scan &scan);
void scan_statement_expression_value(an_expr_node_ptr e,statement_expression_scan &scan) {
  if(!e)return;
  if(e->kind==enk_statement) {scan_statement_expression_body(e->variant.statement,scan);return;}
  if(e->kind!=enk_variable && e->kind!=enk_constant && e->kind!=enk_routine && e->kind!=enk_field && e->kind!=enk_operation)
    unsupported("GNU statement expression unintegrated expression/lifetime",e->kind);
  if(e->kind==enk_operation) {
    if(e->variant.operation.kind==eok_call) {
      auto callee=e->variant.operation.operands;
      if(callee && callee->kind==enk_routine && callee->variant.routine.ptr->source_corresp.name) {
        string symbol=callee->variant.routine.ptr->source_corresp.name;
        if(symbol.find("__vla_")==0 || source_jump_call_kind(e))
          unsupported("GNU statement expression VLA/nonlocal continuation");
      }
    }
    for(auto a=e->variant.operation.operands;a;a=a->next)scan_statement_expression_value(a,scan);
  }
  if(e->kind==enk_variable)expression_captures(e,scan.captures);
}
void statement_expression_capture_variable(a_variable_ptr v,statement_expression_scan &scan) {
  if(v && !virtual_types.count(v) && !global_offsets.count(v) && !tls_offsets.count(v) &&
      std::find(scan.captures.begin(),scan.captures.end(),v)==scan.captures.end())scan.captures.push_back(v);
}
void scan_statement_expression_body(a_statement_ptr s,statement_expression_scan &scan) {
  if(!s)return;
  diagnostic_scope diagnostic(s->position);
  scan_statement_expression_value(s->expr,scan);
  switch(s->kind) {
    case stmk_block:for(auto c=s->variant.block.statements;c;c=c->next)scan_statement_expression_body(c,scan);break;
    case stmk_decl:case stmk_empty:case stmk_expr:case stmk_stmt_expr_result:break;
    case stmk_init: {
      auto d=s->variant.dynamic_init;
      if(d->destructor)unsupported("GNU statement expression local destruction");
      statement_expression_capture_variable(d->variable,scan);
      if(d->kind==dik_expression)scan_statement_expression_value(d->variant.expression,scan);
      else if(d->kind!=dik_none && d->kind!=dik_constant && d->kind!=dik_zero)
        unsupported("GNU statement expression initialization");
      break;
    }
    case stmk_if:
      scan_statement_expression_body(s->variant.if_stmt.then_statement,scan);
      scan_statement_expression_body(s->variant.if_stmt.else_statement,scan);break;
    case stmk_while:case stmk_end_test_while:scan_statement_expression_body(s->variant.loop_statement,scan);break;
    case stmk_for:
      scan_statement_expression_body(s->variant.for_loop.extra_info->initialization,scan);
      scan_statement_expression_body(s->variant.for_loop.statement,scan);
      scan_statement_expression_value(s->variant.for_loop.extra_info->increment,scan);break;
    case stmk_switch:scan_statement_expression_body(s->variant.switch_stmt.body_statement,scan);break;
    case stmk_switch_case:break;
    case stmk_label:scan.labels.insert(s->variant.label.ptr);break;
    case stmk_goto:scan.targets.insert(s->variant.label.ptr);break;
    default:unsupported("GNU statement expression control flow",s->kind);
  }
}
void statement_expression_captures(an_expr_node_ptr e,std::vector<a_variable_ptr> &vars) {
  statement_expression_scan scan;scan_statement_expression_body(e->variant.statement,scan);
  for(auto v:scan.captures)if(std::find(vars.begin(),vars.end(),v)==vars.end())vars.push_back(v);
}
std::vector<bool> statement_expression_results;
string statement_expression_result(an_expr_node_ptr e) {
  if(statement_expression_results.empty() || !e)unsupported("unclaimed GNU statement result");
  return "return "+(statement_expression_results.back()?address(e):expr(e))+";";
}
string gnu_statement_expression(an_expr_node_ptr e) {
  statement_expression_scan scan;scan_statement_expression_body(e->variant.statement,scan);
  for(auto target:scan.targets)if(!scan.labels.count(target))unsupported("GNU statement expression outgoing jump");
  string id="__edg_statement_expr"+std::to_string(++expression_serial),args,params;
  auto saved_variables=expression_variables;
  for(size_t i=0;i<scan.captures.size();++i) {
    auto v=scan.captures[i];if(i){args+=", ";params+=", ";}
    args+=variable_address(v);
    string p="p"+std::to_string(i);params+=type(v->type)+"* "+p;expression_variables[v]="(*"+p+")";
  }
  if(current_function_scope && variadic_routine(current_function_scope->variant.routine.ptr->type)) {
    if(!params.empty()){params+=", ";args+=", ";}
    params+="global::Edg.Runtime.VaList* __edg_arguments";args+="__edg_arguments";
  }
  bool address=e->is_lvalue;
  auto saved_output=std::move(output);output.clear();statement_expression_results.push_back(address);
  statement(e->variant.statement);
  string body=std::move(output);output=std::move(saved_output);statement_expression_results.pop_back();
  expression_variables=std::move(saved_variables);
  helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+type(e->type)+
      (address?"*":"")+" "+id+"("+params+") {\n"+body+"}\n// dot-gen:member-end\n";
  string call=id+"("+args+")";return address?"(*("+call+"))":call;
}
