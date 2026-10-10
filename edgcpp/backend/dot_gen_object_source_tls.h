// Observations of stock TLS variable-wrapper factories. Included at the end of
// dot_object_source; no source IL or lowering decisions are changed.
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
struct TlsVariableFactory {
  a_variable_ptr variable=nullptr;
  a_routine_ptr initializer=nullptr,wrapper=nullptr;
  a_scope_ptr wrapper_scope=nullptr;
  bool guarded=false,wrapper_complete=false;
  std::string error;
};
struct TlsFactoryGraph {
  std::map<a_variable_ptr,TlsVariableFactory> variables;
  std::map<a_routine_ptr,a_variable_ptr> initializer_variables,wrapper_variables;
  a_routine_ptr empty_initializer=nullptr;
  a_scope_ptr empty_scope=nullptr;
  std::string error;
};
inline TlsFactoryGraph &tls_factory_graph(){static TlsFactoryGraph graph;return graph;}
inline void observe_tls_variable_initializer(a_variable_ptr variable) {
  if(!enabled())return;
  capture_variable(variable);
  auto &graph=tls_factory_graph();auto &value=graph.variables[variable];
  value.variable=variable;value.initializer=variable->init_routine.thread.init_routine;
  if(!is_effective_thread_local(variable) || !value.initializer || !value.initializer->is_tls_init_alias ||
     !state().variables.count(variable)) {
    value.error="TLS variable initializer lacks original source storage or factory alias [thread="+
      std::to_string(is_effective_thread_local(variable))+",initializer="+std::to_string(value.initializer!=nullptr)+
      ",alias="+std::to_string(value.initializer && value.initializer->is_tls_init_alias)+
      ",source-storage="+std::to_string(state().variables.count(variable))+"]";return;
  }
  auto added=graph.initializer_variables.emplace(value.initializer,variable);
  if(!added.second && added.first->second!=variable)value.error="TLS initializer aliases unrelated source variables";
}
inline void observe_tls_variable_wrapper_created(a_variable_ptr variable) {
  if(!enabled())return;
  capture_variable(variable);
  auto &graph=tls_factory_graph();auto &value=graph.variables[variable];
  value.variable=variable;value.wrapper=variable->init_routine.thread.wrapper;
  if(!value.wrapper || !graph.wrapper_variables.emplace(value.wrapper,variable).second)
    value.error="TLS wrapper differs from its unique creation factory";
}
inline void observe_tls_variable_wrapper(a_variable_ptr variable,a_scope_ptr scope,a_boolean guarded) {
  if(!enabled())return;
  observe_tls_variable_initializer(variable);
  auto &graph=tls_factory_graph();auto &value=graph.variables[variable];
  value.wrapper=variable->init_routine.thread.wrapper;value.wrapper_scope=scope;value.guarded=guarded;
  if(!value.wrapper || scope->variant.routine.ptr!=value.wrapper || value.wrapper_complete)
    value.error="TLS wrapper differs from its unique observed factory scope";
  value.wrapper_complete=true;
  auto added=graph.wrapper_variables.emplace(value.wrapper,variable);
  if(!added.second && added.first->second!=variable)value.error="TLS wrapper aliases unrelated source variables";
}
inline void observe_empty_tls_initializer(a_routine_ptr routine,a_scope_ptr scope) {
  if(!enabled())return;
  auto &graph=tls_factory_graph();
  if(graph.empty_initializer || !routine->is_tls_init_routine || scope->variant.routine.ptr!=routine)
    graph.error="empty TLS initializer differs from its unique stock factory";
  graph.empty_initializer=routine;graph.empty_scope=scope;
}
#endif

inline bool is_observed_tls_routine(a_routine_ptr routine) {
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
  const auto &graph=tls_factory_graph();
  return graph.wrapper_variables.count(routine) || graph.initializer_variables.count(routine) ||
    (routine && routine==graph.empty_initializer);
#else
  return false;
#endif
}
