// Included after the ordinary statement emitter. This backend-owned inventory
// adds descriptor-only references; it never changes EDG needed flags or IL.
std::set<a_routine_ptr> coroutine_required_routines;
std::set<a_variable_ptr> coroutine_required_variables;
bool coroutine_required(a_routine_ptr routine) {return coroutine_required_routines.count(routine)!=0;}
bool coroutine_required(a_variable_ptr variable) {return coroutine_required_variables.count(variable)!=0;}

template<class Visit> void coroutine_walk_expression(an_expr_node_ptr expression,const Visit &visit) {
  if(!expression)return;
  visit(expression);
  if(expression->kind==enk_await || expression->kind==enk_yield) {
    for(auto child=expression->variant.await_info.ready_resume_suspend;child;child=child->next)
      coroutine_walk_expression(child,visit);
  } else if(expression->kind==enk_operation) {
    for(auto child=expression->variant.operation.operands;child;child=child->next)
      coroutine_walk_expression(child,visit);
  } else if(expression->kind==enk_object_lifetime)
    coroutine_walk_expression(expression->variant.object_lifetime.expr,visit);
  else if(expression->kind==enk_throw && expression->variant.throw_info)
    coroutine_walk_expression(expression->variant.throw_info->expr,visit);
  else if(expression->kind==enk_lowered_eh_construct && expression->variant.lowered_eh.kind==leck_internal_try)
    for(auto child=expression->variant.lowered_eh.variant.try_and_catch_expr;child;child=child->next)
      coroutine_walk_expression(child,visit);
}
template<class Visit> void coroutine_walk_statement(a_statement_ptr statement,const Visit &visit) {
  if(!statement)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(statement,selected)){coroutine_walk_statement(selected,visit);return;}
  coroutine_walk_expression(statement->expr,visit);
  switch(statement->kind) {
    case stmk_block:for(auto child=statement->variant.block.statements;child;child=child->next)coroutine_walk_statement(child,visit);break;
    case stmk_try_block:
      coroutine_walk_statement(statement->variant.try_block->statement,visit);
      for(auto handler=statement->variant.try_block->handlers;handler;handler=handler->next)coroutine_walk_statement(handler->statement,visit);
      break;
    case stmk_if:coroutine_walk_statement(statement->variant.if_stmt.then_statement,visit);coroutine_walk_statement(statement->variant.if_stmt.else_statement,visit);break;
    case stmk_for:
      coroutine_walk_statement(statement->variant.for_loop.extra_info->initialization,visit);
      coroutine_walk_expression(statement->variant.for_loop.extra_info->increment,visit);
      coroutine_walk_statement(statement->variant.for_loop.statement,visit);break;
    case stmk_while:case stmk_end_test_while:coroutine_walk_statement(statement->variant.loop_statement,visit);break;
    case stmk_switch:coroutine_walk_statement(statement->variant.switch_stmt.body_statement,visit);break;
    case stmk_init:if(statement->variant.dynamic_init->kind==dik_expression)coroutine_walk_expression(statement->variant.dynamic_init->variant.expression,visit);break;
    default:break;
  }
}
void collect_coroutine_requirements(a_scope_ptr scope) {
  coroutine_required_routines.clear();coroutine_required_variables.clear();
  bool native_noop_frame=false;
  for(auto variable=scope->variables;variable;variable=variable->next) {
    if(!variable_needed(variable) || !variable->source_corresp.name)continue;
    string symbol=variable->source_corresp.name;
    if(symbol.find("__noop_coroutine_frame_")!=string::npos && symbol.find("_ZNSt")!=string::npos)
      native_noop_frame=true;
  }
  std::vector<a_routine_ptr> pending;
  auto require=[&](a_routine_ptr routine) {
    if(routine && coroutine_required_routines.insert(routine).second)pending.push_back(routine);
  };
  auto expression=[&](an_expr_node_ptr node) {
    if(node->kind==enk_routine)require(node->variant.routine.ptr);
    else if(node->kind==enk_variable)coroutine_required_variables.insert(node_variable(node));
    else if(node->kind==enk_constant) {
      auto constant=node_constant(node);
      if(constant->kind==ck_address && constant->variant.address.kind==abk_routine)
        require(constant->variant.address.variant.routine);
    }
  };
  for(auto routine=scope->routines;routine;routine=routine->next) {
    if(!routine_body_needed(routine))continue;
    auto body=scope_for_routine_or_null(routine);if(!body)continue;
    auto descriptor=coroutine_descriptor(body->assoc_block);if(!descriptor)continue;
    require(descriptor->new_routine);require(descriptor->delete_routine);
    coroutine_walk_expression(descriptor->get_return_object_call,expression);
    coroutine_walk_expression(descriptor->alloc_failure_gro_call,expression);
    coroutine_walk_statement(body->assoc_block,expression);
  }
  for(size_t i=0;i<pending.size();++i) {
    auto routine=pending[i];
    if(routine->function_def_number==NULL_function_def_number)continue;
    auto body=scope_for_routine_or_null(routine);if(body)coroutine_walk_statement(body->assoc_block,expression);
  }
  // The inline native frame can require static initialization even when its
  // private constructor is never used. Only the public noop factory exposes
  // that incompatible address; do not reject harmless header inclusion.
  if(native_noop_frame)for(auto routine=scope->routines;routine;routine=routine->next) {
    if(!routine_body_needed(routine) || !routine->source_corresp.name)continue;
    string symbol=routine->source_corresp.name;
    if(symbol.find("14noop_coroutine")!=string::npos && symbol.find("_ZNSt")!=string::npos)
      unsupported("native libc++ noop coroutine frame requires managed ABI configuration overlay");
  }
}
void collect_coroutine_metadata(a_scope_ptr scope) {
  auto descriptor=coroutine_descriptor(scope->assoc_block);if(!descriptor)return;
  auto collect=[](an_expr_node_ptr node){collect_exception_expression(node);};
  coroutine_walk_expression(descriptor->get_return_object_call,collect);
  coroutine_walk_expression(descriptor->alloc_failure_gro_call,collect);
  coroutine_walk_statement(scope->assoc_block,collect);
}

bool coroutine_helpers_emitted=false;
void ensure_coroutine_final_filter();
void ensure_coroutine_helpers() {
  if(coroutine_helpers_emitted)return;
  coroutine_helpers_emitted=true;ensure_lifecycle();ensure_coroutine_final_filter();
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.Coroutine.Registry? __edg_coroutine_registry;\n"
    "// dot-gen:member-begin\nprivate "+mod+"global::Edg.Runtime.Coroutine.Registry __edg_coroutines { get { lock(__edg_sync) { if(__edg_coroutine_registry==null)__edg_coroutine_registry=new("+
    string(instance_mode?"this":"null")+");return __edg_coroutine_registry; } } }\n// dot-gen:member-end\n";
}
void ensure_coroutine_final_filter() {
  helper_output+="// dot-gen:member-begin\nprivate static bool __edg_coroutine_final_filter(global::System.Exception error, global::Edg.Runtime.Coroutine.Frame* frame, ref bool finalException) {if("+control_transfer_exclusion("error")+"){finalException=true;frame->Resume=null;}return false;}\n// dot-gen:member-end\n";
}
string coroutine_disposal_code() {return coroutine_helpers_emitted?"__edg_coroutine_registry?.Abandon();":"";}
string coroutine_intrinsic(an_expr_node_ptr call,a_routine_ptr routine,an_expr_node_ptr first) {
  if(routine->source_corresp.name && string(routine->source_corresp.name)=="__edg_coro_noop") {
    auto result=skip_typerefs(call->type);
    if(routine->function_def_number!=NULL_function_def_number || first || result->kind!=tk_pointer ||
       skip_typerefs(result->variant.pointer.type)->kind!=tk_void)
      unsupported("private noop coroutine runtime ABI");
    ensure_coroutine_helpers();return "global::Edg.Runtime.Coroutine.Noop()";
  }
  string operation;
  switch(routine->variant.builtin_function_kind) {
    case bfk_coro_noop:
      if(first)unsupported("noop coroutine builtin ABI");
      ensure_coroutine_helpers();return "global::Edg.Runtime.Coroutine.Noop()";
    case bfk_coro_resume:operation="Resume";break;
    case bfk_coro_destroy:operation="Destroy";break;
    case bfk_coro_done:operation="Done";break;
    case bfk_coro_promise: {
      auto alignment=first?first->next:nullptr;auto direction=alignment?alignment->next:nullptr;
      if(!direction || direction->next)unsupported("coroutine promise builtin ABI");
      ensure_coroutine_helpers();
      return "global::Edg.Runtime.Coroutine.Promise((void*)("+expr(first)+"), (nuint)("+expr(alignment)+"), "+condition(direction)+")";
    }
    default:return "";
  }
  if(!first || first->next)unsupported("coroutine handle builtin ABI");
  ensure_coroutine_helpers();
  string result="global::Edg.Runtime.Coroutine."+operation+"((void*)("+expr(first)+"))";
  return operation=="Done"?"(("+type(call->type)+")("+result+" ? 1 : 0))":result;
}

struct coroutine_await_site {unsigned state;bool initial,final;};
std::map<an_expr_node_ptr,coroutine_await_site> coroutine_await_sites;
std::map<a_try_supplement_ptr,std::pair<unsigned long long,unsigned long long>> coroutine_try_offsets;
a_coroutine_descr_ptr current_coroutine=nullptr;
a_routine_ptr current_coroutine_routine=nullptr;
a_statement_ptr coroutine_initial_try=nullptr;
unsigned long long coroutine_storage(unsigned long long size,unsigned long long alignment) {
  if(!size || !alignment || alignment>4096 || (alignment&(alignment-1)))unsupported("coroutine frame storage layout");
  frame_alignment=std::max(frame_alignment,alignment);frame_size=(frame_size+alignment-1)&~(alignment-1);
  auto result=frame_size;frame_size+=size;
  if(frame_size+frame_alignment>64*1024*1024)unsupported("coroutine frame exceeds 64 MiB");
  return result;
}
void coroutine_frame_variable(a_variable_ptr variable) {
  if(!variable || local_offsets.count(variable) || global_offsets.count(variable) || tls_offsets.count(variable) ||
     variable==cleanup_region_var || variable==cleanup_array_var || exception_metadata.count(variable) ||
     constant_templates.count(variable) || variable->storage_class==sc_extern)return;
  auto storage=skip_typerefs(variable->type);(void)type(storage);
  local_offsets[variable]=coroutine_storage(storage->size,std::max<a_targ_alignment>(alignment_of_type(storage),variable->alignment));
}
void coroutine_frame_scope(a_scope_ptr scope) {
  for(auto variable=scope->nonstatic_variables;variable;variable=variable->next)
    if(variable_needed(variable))coroutine_frame_variable(variable);
  for(auto child=scope->scopes;child;child=child->next)coroutine_frame_scope(child);
}
bool coroutine_has_await(an_expr_node_ptr expression) {
  bool found=false;coroutine_walk_expression(expression,[&](an_expr_node_ptr node){found|=node->kind==enk_await || node->kind==enk_yield;});return found;
}
std::vector<an_expr_node_ptr> coroutine_awaits(a_statement_ptr statement) {
  std::vector<an_expr_node_ptr> result;
  coroutine_walk_statement(statement,[&](an_expr_node_ptr node){if(node->kind==enk_await || node->kind==enk_yield)result.push_back(node);});
  return result;
}
string coroutine_slot(unsigned long long offset,const string &type) {
  return "(*("+type+"*)(__edg_frame + "+std::to_string(offset)+"UL))";
}
string coroutine_try_entry(a_try_supplement_ptr statement) {return name(statement,"__edg_coro_try")+"_entry";}
string coroutine_resume_label(an_expr_node_ptr expression) {return "__edg_coro_resume_"+std::to_string(coroutine_await_sites.at(expression).state);}
void coroutine_dispatch_routes(a_statement_ptr statement,std::map<unsigned,string> &routes) {
  if(!statement)return;
  if(statement->kind==stmk_try_block) {
    for(auto await:coroutine_awaits(statement))routes[coroutine_await_sites.at(await).state]=coroutine_try_entry(statement->variant.try_block);
    return;
  }
  auto visit=[&](an_expr_node_ptr expression) {
    if(expression->kind==enk_await || expression->kind==enk_yield)
      routes[coroutine_await_sites.at(expression).state]=coroutine_resume_label(expression);
  };
  coroutine_walk_expression(statement->expr,visit);
  switch(statement->kind) {
    case stmk_block:for(auto child=statement->variant.block.statements;child;child=child->next)coroutine_dispatch_routes(child,routes);break;
    case stmk_if:coroutine_dispatch_routes(statement->variant.if_stmt.then_statement,routes);coroutine_dispatch_routes(statement->variant.if_stmt.else_statement,routes);break;
    case stmk_for:
      coroutine_dispatch_routes(statement->variant.for_loop.extra_info->initialization,routes);
      coroutine_walk_expression(statement->variant.for_loop.extra_info->increment,visit);
      coroutine_dispatch_routes(statement->variant.for_loop.statement,routes);break;
    case stmk_while:case stmk_end_test_while:coroutine_dispatch_routes(statement->variant.loop_statement,routes);break;
    case stmk_switch:coroutine_dispatch_routes(statement->variant.switch_stmt.body_statement,routes);break;
    case stmk_init:if(statement->variant.dynamic_init->kind==dik_expression)coroutine_walk_expression(statement->variant.dynamic_init->variant.expression,visit);break;
    default:break;
  }
}
void coroutine_dispatch(const std::map<unsigned,string> &routes) {
  line("switch(__edg_coro->State) {");
  for(auto &route:routes)line("case "+std::to_string(route.first)+": goto "+route.second+";");
  line("case 0: break;default: global::System.Environment.FailFast(\"Invalid coroutine continuation\");break;}\n");
}
void coroutine_effect(an_expr_node_ptr expression) {
  if(!expression)return;
  if(expression==current_coroutine->unhandled_exception_call) {
    line("try {"+expression_effect(expression)+"} catch(global::System.Exception error) when(__edg_coroutine_final_filter(error,__edg_coro,ref __edg_final_exception)){throw;}");
    return;
  }
  bool semantic_call=false;
  coroutine_walk_expression(expression,[&](an_expr_node_ptr node){semantic_call|=node==current_coroutine->unhandled_exception_call;});
  if(!semantic_call && !coroutine_has_await(expression)){line(expression_effect(expression));return;}
  if(expression->kind==enk_object_lifetime){coroutine_effect(expression->variant.object_lifetime.expr);return;}
  if(expression->kind==enk_operation && expression->variant.operation.kind==eok_comma) {
    auto first=expression->variant.operation.operands;coroutine_effect(first);coroutine_effect(first->next);return;
  }
  if(expression->kind!=enk_await && expression->kind!=enk_yield)
    unsupported("coroutine suspension inside an unnormalized value expression");
  auto ready=expression->variant.await_info.ready_resume_suspend;
  auto resume=ready?ready->next:nullptr;auto suspend=resume?resume->next:nullptr;
  if(!suspend || suspend->next || coroutine_has_await(ready) || coroutine_has_await(resume) || coroutine_has_await(suspend))
    unsupported("nested coroutine await protocol");
  auto site=coroutine_await_sites.at(expression);
  line("if (!"+condition(ready)+") {");
  line("__edg_coro->State="+std::to_string(site.state)+";__edg_coro->Cleanup=__edg_cleanup_state;");
  if(site.final)line("__edg_coro->Resume=null;");
  auto suspend_type=skip_typerefs(suspend->type);
  if(suspend_type->kind==tk_void) {
    line(expression_effect(suspend));line("return null;");
  } else if(suspend_type->kind==tk_integer && suspend_type->variant.integer.bool_type) {
    line("if "+condition(suspend)+" return null;");
  } else if(aggregate(suspend_type) && suspend_type->size==8) {
    string temporary="__edg_transfer_"+std::to_string(site.state);
    line(type(suspend->type)+" "+temporary+"="+expr(suspend)+";");
    line("return *(void**)&"+temporary+";");
  } else unsupported("coroutine await_suspend result ABI");
  line("}");line(coroutine_resume_label(expression)+": ;");line("__edg_coro->State=0;");
  line(expression_effect(resume));
}
void coroutine_statement(a_statement_ptr statement);
void coroutine_try(a_statement_ptr statement) {
  auto block=statement->variant.try_block;
  auto offsets=coroutine_try_offsets.at(block);
  string id=name(block,"__edg_coro_try"),active=coroutine_slot(offsets.first,"byte"),boundary=coroutine_slot(offsets.second,"int");
  // EDG's epilogue is emitted by the ordinary statement helper and assigns a
  // bool. Use a frame bool field only for this backend's private region flag.
  active=coroutine_slot(offsets.first,"bool");try_frames[block]=active;
  line(coroutine_try_entry(block)+": ;");
  line("if(__edg_coro->State==0){"+active+"=true;"+boundary+"=__edg_cleanup_state;} try {");
  std::map<unsigned,string> routes;coroutine_dispatch_routes(block->statement,routes);coroutine_dispatch(routes);
  coroutine_statement(block->statement);
  string matches;bool catches_all=false;
  for(auto handler=block->handlers;handler;handler=handler->next) {
    if(!handler->parameter){catches_all=true;break;}
    if(!matches.empty())matches+=" || ";
    matches+=exception_match("((__edg_exception)"+id+"_error).Type",catch_type(handler));
  }
  matches=catches_all?"true":"("+id+"_error is __edg_exception && ("+matches+"))";
  line("} catch(global::System.Exception "+id+"_error) when("+active+" && __edg_begin_exception("+id+"_error) && "+matches+") {");
  line(cleanup_unwind(boundary));bool first=true,ellipsis=false;
  for(auto handler=block->handlers;handler;handler=handler->next) {
    if(!coroutine_awaits(handler->statement).empty())unsupported("coroutine suspension in handler");
    string test=handler->parameter?id+"_error is __edg_exception "+id+"_typed"+name(handler,"h")+" && "+
      exception_match(id+"_typed"+name(handler,"h")+".Type",catch_type(handler)):"true";
    line(string(first?"if(":"else if(")+test+") {");first=false;ellipsis|=!handler->parameter;
    string frame=id+"_frame"+name(handler,"h");catch_frames[handler]=frame;
    line("var "+frame+"="+exception_catch_frame(id+"_error",handler->parameter?catch_type(handler):nullptr)+";try {");
    coroutine_statement(handler->statement);
    line("} catch(global::System.Exception handler_error) when(__edg_catch_filter("+frame+",handler_error)){throw;}finally{if("+frame+".Active){"+cleanup_unwind(boundary)+"}"+frame+".Leave();}}");
    catch_frames.erase(handler);
  }
  if(!ellipsis)line("else {throw;}");
  line("}");
}
void coroutine_statement(a_statement_ptr node) {
  if(!node)return;
  diagnostic_scope diagnostic(node->position);
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(node,selected)){coroutine_statement(selected);return;}
  if(node->kind==stmk_coroutine)return;
  if(node->kind==stmk_coroutine_return){coroutine_effect(node->expr);return;}
  if(node->kind==stmk_block){for(auto child=node->variant.block.statements;child;child=child->next)coroutine_statement(child);return;}
  if(node->kind==stmk_try_block){coroutine_try(node);return;}
  bool structured=node->kind==stmk_if || node->kind==stmk_for || node->kind==stmk_while || node->kind==stmk_end_test_while || node->kind==stmk_switch;
  bool semantic_call=false;
  coroutine_walk_expression(node->expr,[&](an_expr_node_ptr expression){semantic_call|=expression==current_coroutine->unhandled_exception_call;});
  if(!structured && !semantic_call && coroutine_awaits(node).empty()){statement(node);return;}
  switch(node->kind) {
    case stmk_expr:coroutine_effect(node->expr);break;
    case stmk_if: {
      if(coroutine_has_await(node->expr))unsupported("coroutine condition suspension");
      string id=name(node,"__edg_if");line("if (!"+condition(node->expr)+") goto "+id+"_else;");
      coroutine_statement(node->variant.if_stmt.then_statement);line("goto "+id+"_end;");
      line(id+"_else: ;");coroutine_statement(node->variant.if_stmt.else_statement);line(id+"_end: ;");break;
    }
    case stmk_for: {
      auto loop=node->variant.for_loop.extra_info;
      if(coroutine_has_await(node->expr) || coroutine_has_await(loop->increment))unsupported("coroutine loop-control suspension");
      coroutine_statement(loop->initialization);string id=name(node,"__edg_for");line(id+"_head: ;");
      if(node->expr)line("if (!"+condition(node->expr)+") goto "+id+"_end;");
      coroutine_statement(node->variant.for_loop.statement);line(expression_effect(loop->increment));line("goto "+id+"_head;");
      if(node->expr)line(id+"_end: ;");
      break;
    }
    case stmk_while:case stmk_end_test_while: {
      if(coroutine_has_await(node->expr))unsupported("coroutine loop-condition suspension");
      string id=name(node,"__edg_loop");line(id+"_head: ;");
      if(node->kind==stmk_while)line("if (!"+condition(node->expr)+") goto "+id+"_end;");
      coroutine_statement(node->variant.loop_statement);
      line(node->kind==stmk_while?"goto "+id+"_head;":"if "+condition(node->expr)+" goto "+id+"_head;");
      if(node->kind==stmk_while)line(id+"_end: ;");
      break;
    }
    case stmk_switch: {
      if(coroutine_has_await(node->expr))unsupported("coroutine switch-condition suspension");
      string id=name(node,"__edg_switch");line(type(node->expr->type)+" "+id+"_value="+expr(node->expr)+";");
      auto extra=node->variant.switch_stmt.extra_info;
      for(auto entry=extra->cases;entry;entry=entry->next) {
        if(!entry->case_value)continue;
        string test=id+"_value == "+constant(entry->case_value);
        if(entry->range_end)test=id+"_value >= "+constant(entry->case_value)+" && "+id+"_value <= "+constant(entry->range_end);
        line("if("+test+") goto "+name(entry,"__edg_case")+";");
      }
      line("goto "+(extra->default_case?name(extra->default_case,"__edg_case"):id+"_end")+";");
      coroutine_statement(node->variant.switch_stmt.body_statement);if(!extra->default_case)line(id+"_end: ;");break;
    }
    default:unsupported("coroutine statement needing continuation lowering",node->kind);
  }
}
void coroutine_plan_tries(a_statement_ptr node) {
  if(!node)return;
  switch(node->kind) {
    case stmk_try_block: {
      auto block=node->variant.try_block;
      auto active=coroutine_storage(1,1),boundary=coroutine_storage(4,4);
      coroutine_try_offsets[block]={active,boundary};
      coroutine_plan_tries(block->statement);
      for(auto handler=block->handlers;handler;handler=handler->next)coroutine_plan_tries(handler->statement);
      break;
    }
    case stmk_block:for(auto child=node->variant.block.statements;child;child=child->next)coroutine_plan_tries(child);break;
    case stmk_if:coroutine_plan_tries(node->variant.if_stmt.then_statement);coroutine_plan_tries(node->variant.if_stmt.else_statement);break;
    case stmk_for:coroutine_plan_tries(node->variant.for_loop.extra_info->initialization);coroutine_plan_tries(node->variant.for_loop.statement);break;
    case stmk_while:case stmk_end_test_while:coroutine_plan_tries(node->variant.loop_statement);break;
    case stmk_switch:coroutine_plan_tries(node->variant.switch_stmt.body_statement);break;
    default:break;
  }
}
a_variable_ptr coroutine_return_storage(an_expr_node_ptr expression,a_type_ptr expected) {
  if(!expression)return nullptr;
  if(expression->kind==enk_object_lifetime)return coroutine_return_storage(expression->variant.object_lifetime.expr,expected);
  if(expression->kind!=enk_operation)return nullptr;
  auto first=expression->variant.operation.operands;
  if(expression->variant.operation.kind==eok_comma)return coroutine_return_storage(first->next,expected);
  if(expression->variant.operation.kind!=eok_call || !first || first->kind!=enk_routine)return nullptr;
  auto routine=first->variant.routine.ptr;auto extra=skip_typerefs(routine->type)->variant.routine.extra_info;
  auto destination=first->next;
  if(extra->value_returned_as_parameter) {
    if(extra->has_this_param && extra->return_value_parameter_follows_this)destination=destination->next;
  } else if(!special_kind_is(routine,sfk_constructor))return nullptr;
  while(destination && destination->kind==enk_operation &&
        (destination->variant.operation.kind==eok_cast || destination->variant.operation.kind==eok_parens))
    destination=destination->variant.operation.operands;
  if(!destination || destination->kind!=enk_operation || destination->variant.operation.kind!=eok_address_of)return nullptr;
  auto object=destination->variant.operation.operands;
  if(!object || object->kind!=enk_variable || type(object->type)!=type(expected))return nullptr;
  return node_variable(object);
}
string coroutine_delete_call(a_coroutine_descr_ptr descriptor,const string &address,const string &size) {
  auto routine=descriptor->delete_routine;if(!routine)unsupported("coroutine without selected delete");
  auto parameters=skip_typerefs(routine->type)->variant.routine.extra_info->param_type_list;
  if(!parameters || (parameters->next && parameters->next->next))unsupported("coroutine deallocator parameters");
  string result=name(routine,"f")+"(("+type(parameters->type)+")"+address;
  if(parameters->next)result+=", ("+type(parameters->next->type)+")"+size;
  return result+");";
}
void emit_coroutine(a_routine_ptr routine,a_scope_ptr scope,a_coroutine_descr_ptr descriptor) {
  diagnostic_scope diagnostic(routine->source_corresp.decl_position);
  if(source_jumps_used)unsupported("coroutine/nonlocal-jump lifetime integration");
  check_operand_order(scope->assoc_block);
  check_operand_order(descriptor->get_return_object_call);
  check_operand_order(descriptor->alloc_failure_gro_call);
  coroutine_walk_statement(scope->assoc_block,[](an_expr_node_ptr node) {
    if(source_jump_call_kind(node))unsupported("coroutine/nonlocal-jump lifetime integration");
    if(node->kind==enk_await || node->kind==enk_yield)
      for(auto operation=node->variant.await_info.ready_resume_suspend;operation;operation=operation->next)
        check_operand_order(operation);
  });
  ensure_coroutine_helpers();ensure_exception_helpers();ensure_cleanup_failure_filter();
  if(!descriptor->body_generated || !descriptor->promise || !descriptor->handle || !descriptor->init_await_resume)
    unsupported("incomplete coroutine descriptor");
  if(scope->assoc_block->kind!=stmk_block)unsupported("coroutine function-try frame entry");
  current_coroutine=descriptor;current_coroutine_routine=routine;
  coroutine_await_sites.clear();coroutine_try_offsets.clear();coroutine_initial_try=nullptr;
  local_offsets.clear();frame_size=0;frame_alignment=8;reset_materialization();
  coroutine_frame_variable(descriptor->promise);
  coroutine_frame_variable(descriptor->handle);coroutine_frame_variable(descriptor->init_await_resume);
  for(auto parameter=scope->variant.routine.parameters;parameter;parameter=parameter->next)coroutine_frame_variable(parameter);
  coroutine_frame_scope(scope);
  bool unhandled_call_found=false;
  auto inventory=[&](an_expr_node_ptr node) {
    unhandled_call_found|=node==descriptor->unhandled_exception_call;
    if(node->kind==enk_variable)coroutine_frame_variable(node_variable(node));
    if(node->kind==enk_await || node->kind==enk_yield) {
      if(!coroutine_await_sites.count(node)) {
        unsigned state=(unsigned)coroutine_await_sites.size()+1;
        coroutine_await_sites[node]={state,false,false};
      }
      for(auto operation=node->variant.await_info.ready_resume_suspend;operation;operation=operation->next)
        collect_materialization(operation);
    }
  };
  coroutine_walk_statement(scope->assoc_block,inventory);
  if(!unhandled_call_found)unsupported("coroutine unhandled-exception semantic boundary");
  coroutine_walk_expression(descriptor->get_return_object_call,inventory);
  coroutine_walk_expression(descriptor->alloc_failure_gro_call,inventory);
  collect_materialization(scope->assoc_block);collect_materialization(descriptor->get_return_object_call);collect_materialization(descriptor->alloc_failure_gro_call);
  coroutine_walk_expression(descriptor->initial_suspend_call,[&](an_expr_node_ptr node){if(coroutine_await_sites.count(node))coroutine_await_sites[node].initial=true;});
  coroutine_walk_expression(descriptor->final_suspend_call,[&](an_expr_node_ptr node){if(coroutine_await_sites.count(node))coroutine_await_sites[node].final=true;});
  for(auto node=scope->assoc_block->variant.block.statements;node;node=node->next) {
    if(node->kind!=stmk_try_block)continue;
    for(auto await:coroutine_awaits(node))if(coroutine_await_sites.at(await).initial)coroutine_initial_try=node;
  }
  if(!coroutine_initial_try)unsupported("coroutine initial suspension region");
  coroutine_plan_tries(scope->assoc_block);
  for(auto variable:cleanup_capture_variables)coroutine_frame_variable(variable);
  if(skip_typerefs(descriptor->handle->type)->size!=8)unsupported("coroutine handle representation");
  auto rt=skip_typerefs(routine->type);auto extra=rt->variant.routine.extra_info;
  auto specification=extra->exception_specification;
  if(specification && specification->indeterminate)unsupported("indeterminate coroutine exception specification");
  bool no_throw=specification && !specification->throw_any && specification->is_noexcept;
  if(dynamic_exception_spec(routine))unsupported("coroutine dynamic exception specification");
  auto parameters=scope->variant.routine.parameters;
  a_variable_ptr result_parameter=nullptr;
  if(extra->value_returned_as_parameter)result_parameter=extra->has_this_param && extra->return_value_parameter_follows_this?parameters->next:parameters;
  a_variable_ptr result_storage=nullptr;
  if(result_parameter) {
    auto result_type=skip_typerefs(result_parameter->type)->variant.pointer.type;
    result_storage=coroutine_return_storage(descriptor->get_return_object_call,result_type);
    if(!result_storage)unsupported("coroutine converted return-object destination");
  }
  string id=name(routine,"f"),resume_id="__edg_coro_resume_"+id,destroy_id="__edg_coro_destroy_"+id;
  string mod=instance_mode?"":"static ",owner=instance_mode?"owner.":"";
  string signature="private "+mod+type(rt->variant.routine.return_type)+" "+id+"(",arguments;
  for(auto parameter=parameters;parameter;parameter=parameter->next) {
    if(parameter!=parameters){signature+=", ";arguments+=", ";}
    signature+=type(parameter->type)+" "+name(parameter,"v");arguments+=name(parameter,"v");
  }
  signature+=")";
  string bytes="((nuint)sizeof(global::Edg.Runtime.Coroutine.Frame) + (nuint)"+std::to_string(8+frame_alignment-1+frame_size)+"UL)";
  auto allocator=descriptor->new_routine;
  auto allocation_parameters=allocator?skip_typerefs(allocator->type)->variant.routine.extra_info->param_type_list:nullptr;
  if(!allocation_parameters || allocation_parameters->next)unsupported("coroutine placement/nothrow allocator adapter");
  line("// dot-gen:member-begin");line(signature+" {");
  if(no_throw)line("try {");
  line("nuint __edg_size="+bytes+";void* __edg_allocation="+name(allocator,"f")+"(("+type(allocation_parameters->type)+")__edg_size);");
  if(descriptor->alloc_failure_gro_call)unsupported("coroutine allocation-failure return adapter");
  line("if(__edg_allocation==null)global::System.Environment.FailFast(\"Throwing coroutine allocator returned null\");");
  line("byte* __edg_frame=(byte*)(((nuint)__edg_allocation+(nuint)sizeof(global::Edg.Runtime.Coroutine.Frame)+"+std::to_string(8+frame_alignment-1)+"UL)&~(nuint)"+std::to_string(frame_alignment-1)+"UL);");
  line("global::System.Runtime.InteropServices.NativeMemory.Clear(__edg_frame,(nuint)"+std::to_string(frame_size)+"UL);");
  line("var __edg_coro=(global::Edg.Runtime.Coroutine.Frame*)__edg_allocation;bool __edg_registered=false,__edg_created=false,__edg_started=false,__edg_unwinding=false;int __edg_cleanup_state=0;");
  line("try {__edg_coroutines.Attach(__edg_coro,__edg_frame,__edg_size,&"+resume_id+",&"+destroy_id+");__edg_registered=true;");
  for(auto parameter=parameters;parameter;parameter=parameter->next)line(variable(parameter)+"="+name(parameter,"v")+";");
  line("*(void**)&("+variable(descriptor->handle)+")=__edg_coro;");
  for(auto node=scope->assoc_block->variant.block.statements;node!=coroutine_initial_try;node=node->next)
    if(node->kind!=stmk_coroutine)statement(node);
  auto saved_variables=expression_variables;
  if(result_storage)expression_variables[result_storage]="(*"+name(result_parameter,"v")+")";
  string result;
  if(skip_typerefs(rt->variant.routine.return_type)->kind==tk_void)line(expression_effect(descriptor->get_return_object_call));
  else {result="__edg_result";line(type(rt->variant.routine.return_type)+" "+result+"="+expr(descriptor->get_return_object_call)+";");}
  expression_variables=saved_variables;
  line("__edg_coro->Cleanup=__edg_cleanup_state;__edg_started=true;global::Edg.Runtime.Coroutine.Resume(__edg_coro);__edg_created=true;");
  line(result.empty()?"return;":"return "+result+";");
  line("} catch(global::System.Exception error) when(__edg_begin_unwind(error,ref __edg_unwinding)){throw;}finally {if(!__edg_created && !(__edg_started && __edg_coro->Resume==null)){");
  line("if(__edg_registered){if(__edg_started)__edg_cleanup_state=__edg_coro->Cleanup;"+cleanup_unwind("0")+"global::Edg.Runtime.Coroutine.Detach(__edg_coro);}");
  line(coroutine_delete_call(descriptor,"__edg_allocation","__edg_size"));line("}}");
  if(no_throw)line("} catch(global::System.Exception error) when("+control_transfer_exclusion("error")+"){__edg_terminate_runtime(error,\"Exception escaped noexcept coroutine factory\");throw;}");
  line("}");line("// dot-gen:member-end");

  line("// dot-gen:member-begin");line("private static void* "+resume_id+"(void* address) {");
  if(instance_mode)line(string(getenv("EDG_DOT_CLASS"))+" owner=("+getenv("EDG_DOT_CLASS")+")global::Edg.Runtime.Coroutine.Owner(address)!;");
  line(owner+"__edg_enter();try{return "+owner+resume_id+"_body((global::Edg.Runtime.Coroutine.Frame*)address);}finally{"+owner+"__edg_leave();}");
  line("}\n// dot-gen:member-end");line("// dot-gen:member-begin");
  line("private "+mod+"void* "+resume_id+"_body(global::Edg.Runtime.Coroutine.Frame* __edg_coro){");
  line("byte* __edg_frame=(byte*)__edg_coro->Promise;int __edg_cleanup_state=__edg_coro->Cleanup;bool __edg_unwinding=false,__edg_final_exception=false;try {");
  std::map<unsigned,string> routes;
  for(auto node=coroutine_initial_try;node;node=node->next)coroutine_dispatch_routes(node,routes);
  coroutine_dispatch(routes);
  for(auto node=coroutine_initial_try;node;node=node->next)coroutine_statement(node);
  line("__edg_coro->Cleanup=__edg_cleanup_state;global::Edg.Runtime.Coroutine.Destroy(__edg_coro);return null;");
  line("} catch(global::System.Exception error) when(!__edg_final_exception && __edg_begin_unwind(error,ref __edg_unwinding)){throw;}finally{if(__edg_unwinding){"+cleanup_unwind("0")+"}if(__edg_unwinding || __edg_final_exception)__edg_coro->Cleanup=__edg_cleanup_state;}}");
  line("// dot-gen:member-end");

  line("// dot-gen:member-begin");line("private static void "+destroy_id+"(void* address) {");
  if(instance_mode)line(string(getenv("EDG_DOT_CLASS"))+" owner=("+getenv("EDG_DOT_CLASS")+")global::Edg.Runtime.Coroutine.Owner(address)!;");
  line(owner+"__edg_enter();try{"+owner+destroy_id+"_body((global::Edg.Runtime.Coroutine.Frame*)address);}finally{"+owner+"__edg_leave();}");
  line("}\n// dot-gen:member-end\n// dot-gen:member-begin");
  line("private "+mod+"void "+destroy_id+"_body(global::Edg.Runtime.Coroutine.Frame* __edg_coro){");
  line("byte* __edg_frame=(byte*)__edg_coro->Promise;nuint __edg_size=__edg_coro->AllocationSize;int __edg_cleanup_state=__edg_coro->Cleanup;bool __edg_unwinding=false;try{");
  line("try{while(__edg_cleanup_state!=0){switch(__edg_cleanup_state){"+emit_cleanup()+"default:global::System.Environment.FailFast(\"Invalid coroutine destruction state\");break;}}}");
  line("catch(global::System.Exception error)when(__edg_begin_unwind(error,ref __edg_unwinding)){throw;}finally{if(__edg_unwinding){"+cleanup_unwind("0")+"}}");
  line("}finally{global::Edg.Runtime.Coroutine.Detach(__edg_coro);"+coroutine_delete_call(descriptor,"__edg_coro","__edg_size")+"}}");
  line("// dot-gen:member-end");emit_cleanup_helper();
  if(function_vla_used)unsupported("coroutine variable-length storage lifetime");
  if(library_mode)unsupported("exported coroutine factory wrapper");
  current_coroutine=nullptr;current_coroutine_routine=nullptr;
}
