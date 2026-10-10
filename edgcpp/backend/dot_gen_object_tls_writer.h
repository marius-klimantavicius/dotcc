// Staged members of Writer: only stock wrappers, imported/empty-target aliases,
// and the empty TU initializer. Dynamic TU initializer bodies remain distinct.
std::vector<string> tls_routine_origins;
bool tls_alias_definition(a_routine_ptr routine) {
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
  const auto &graph=dot_object_source::tls_factory_graph();
  return graph.initializer_variables.count(routine) &&
    (routine->storage_class==sc_static || routine->storage_class==sc_unspecified);
#else
  return false;
#endif
}
a_routine_ptr tls_alias_target(a_routine_ptr routine) {
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
  auto &graph=dot_object_source::tls_factory_graph();
  if(!tls_alias_definition(routine))return nullptr;
  if(!graph.error.empty())reject(graph.error.c_str());
  if(!graph.empty_initializer || !graph.empty_scope)
    reject("defined TLS alias requires owned dynamic TU initialization contract");
  for(const auto &entry:dot_object_source::state().generated_initializers)
    if(entry.second.is_thread_local)reject("TLS alias has dynamic TU initialization rather than observed empty target");
  return graph.empty_initializer;
#else
  return nullptr;
#endif
}
bool write_tls_routine_origin(a_routine_ptr r,unsigned id,a_type_ptr t,a_scope_ptr body) {
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
  auto &graph=dot_object_source::tls_factory_graph();
  auto wrapper=graph.wrapper_variables.find(r),alias=graph.initializer_variables.find(r);
  bool empty=r==graph.empty_initializer;
  if(wrapper==graph.wrapper_variables.end() && alias==graph.initializer_variables.end() && !empty)return false;
  auto extra=t->variant.routine.extra_info;
  if(extra->param_type_list || extra->has_ellipsis || !extra->prototyped || extra->explicit_calling_convention ||
     (body && body->variant.routine.parameters) || dot_object_source::state().functions.count(r))
    reject((string("TLS generated routine differs from its observed parameterless factory ABI [name=")+
      dot_object_source::spelling(r->source_corresp)+",parameters="+std::to_string(extra->param_type_list!=nullptr)+
      ",ellipsis="+std::to_string(extra->has_ellipsis)+",prototyped="+std::to_string(extra->prototyped)+
      ",calling-convention="+std::to_string(extra->explicit_calling_convention)+
      ",body-parameters="+std::to_string(body && body->variant.routine.parameters)+
      ",source-function="+std::to_string(dot_object_source::state().functions.count(r))+"]").c_str());
  a_variable_ptr variable=nullptr;a_routine_ptr target=nullptr;string kind,guard="none";
  if(empty) {
    if(!graph.error.empty())reject(graph.error.c_str());
    if(!body || body!=graph.empty_scope || !r->is_tls_init_routine || r->storage_class!=sc_static ||
       type_id(t->variant.routine.return_type)!=1)reject("empty TLS initializer lacks exact factory scope/ABI");
    kind="empty-initializer";
  } else {
    variable=wrapper!=graph.wrapper_variables.end()?wrapper->second:alias->second;
    const auto &origin=graph.variables.at(variable);
    if(!origin.error.empty())reject(origin.error.c_str());
    if(!variable->is_thread_local || !dot_object_source::state().variables.count(variable) ||
       variable->init_routine.thread.init_routine!=origin.initializer)
      reject("TLS factory lost original source variable identity");
    if(wrapper!=graph.wrapper_variables.end()) {
      if(!origin.wrapper_complete || origin.wrapper!=r || variable->init_routine.thread.wrapper!=r ||
         !body || body!=origin.wrapper_scope || type_id(t->variant.routine.return_type)!=pointer_type(type_id(variable->type)))
        reject("TLS wrapper differs from observed source storage pointer ABI/body");
      kind="variable-wrapper";guard=origin.guarded?"weak-address":"always";target=origin.initializer;
    } else {
      if(body || !r->is_tls_init_alias || type_id(t->variant.routine.return_type)!=1)
        reject("TLS variable initializer differs from observed bodyless void alias");
      kind="initializer-alias";target=tls_alias_target(r);
    }
  }
  unsigned source_storage=variable?storage(variable):0,target_id=target?routine(target):0;
  routine_abi_slots[r]={};
  tls_routine_origins.push_back("{\"entityId\":"+number(id)+",\"kind\":"+quote(kind)+
    ",\"sourceStorageId\":"+number(source_storage)+",\"targetEntityId\":"+number(target_id)+
    ",\"guardKind\":"+quote(guard)+",\"captureProof\":\"observed-tls-variable-factory\"}");
  return true;
#else
  return false;
#endif
}
bool write_tls_alias_body(a_routine_ptr r) {
  auto target=tls_alias_target(r);if(!target)return false;
  function=routine_ids.at(r);current_routine=r;
  auto position=r->source_corresp.decl_position;
  unsigned block=node("block",0,position),call=node("call",1,position,routine(target),0,"","","value","left-to-right");
  unsigned expression_statement=node("expr-stmt",0,position),ret=node("return",0,position);
  edge(expression_statement,"value",0,call);edge(block,"statement",0,expression_statement);edge(block,"statement",1,ret);
  functions.push_back("{\"entityId\":"+number(function)+",\"bodyNodeId\":"+number(block)+"}");
  return true;
}
