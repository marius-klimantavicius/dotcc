// libc++'s portable external thread ABI. Fixed C entry points are ordinary
// imported routines, so canonical address adapters continue to own call leases.
bool thread_services_emitted=false;
bool c11_thread_exit_used=false;
std::set<a_routine_ptr> thread_routines;
void reset_thread_imports() {thread_services_emitted=false;c11_thread_exit_used=false;thread_routines.clear();}
void collect_c11_thread_imports(a_scope_ptr scope) {
  for(auto routine=scope->routines;routine;routine=routine->next)
    if(routine_needed(routine) && routine->function_def_number==NULL_function_def_number &&
       routine->source_corresp.name && string(routine->source_corresp.name)=="__edg_c11_thread_exit")
      c11_thread_exit_used=true;
}
string thread_owner_type() {return instance_mode?string(getenv("EDG_DOT_CLASS")):"__edg_thread_owner";}
string thread_runtime_type() {return "global::Edg.Runtime.LibcxxThreads<"+thread_owner_type()+">";}
string thread_finalizer_catch_code() {
  return c11_thread_exit_used && thread_services_emitted?
    "catch(global::Edg.Runtime.CThreadExitTransfer exit) when(__edg_thread_services_value!=null && __edg_thread_services_value.AcceptExitResult(exit)) {}":"";
}
string thread_keys_release_code() {return thread_services_emitted?"__edg_thread_services_value?.ReleaseThread();":"";}
string thread_disposal_code() {return thread_services_emitted?"__edg_thread_services_value?.Dispose();":"";}
void ensure_thread_services() {
  if(thread_services_emitted)return;
  thread_services_emitted=true;ensure_lifecycle();
  string mod=instance_mode?"":"static ",owner_type=thread_owner_type(),runtime=thread_runtime_type();
  if(!instance_mode)helper_output+="// dot-gen:member-begin\nprivate sealed class __edg_thread_owner {}\n// dot-gen:member-end\nprivate static readonly __edg_thread_owner __edg_thread_owner_value=new();\n";
  helper_output+="private "+mod+runtime+"? __edg_thread_services_value;\n";
  string prefix=instance_mode?"owner.":"";
  for(const auto &hook:std::vector<std::pair<string,string>>{
      {"retain","__edg_acquire_thread_lease()"},{"release","__edg_release_thread_lease()"},
      {"enter","__edg_enter_retained_thread()"},{"leave","__edg_leave()"},
      {"tls","__edg_release_thread_core(true)"}}) {
    helper_output+="// dot-gen:member-begin\nprivate static void __edg_thread_"+hook.first+"("+owner_type+" owner) { "+prefix+hook.second+"; }\n// dot-gen:member-end\n";
  }
  // This follows the current backend termination boundary. Once a translated
  // std::terminate handler provider exists, route this adapter through it too.
  helper_output+="// dot-gen:member-begin\nprivate static void __edg_thread_terminate("+owner_type+" owner, global::System.Exception error) { /*__EDG_THREAD_TERMINATE__*/ }\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate "+mod+runtime+" __edg_thread_services { get { lock(__edg_sync) { if(__edg_thread_services_value==null) __edg_thread_services_value=new "+runtime+"("+
    string(instance_mode?"this":"__edg_thread_owner_value")+", &__edg_thread_retain, &__edg_thread_release, &__edg_thread_enter, &__edg_thread_leave, &__edg_thread_tls, &__edg_thread_terminate); return __edg_thread_services_value; } } }\n// dot-gen:member-end\n";
}
bool thread_timespec_pointer(a_type_ptr t) {
  t=skip_typerefs(t);
  if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  if(t->kind!=tk_struct || t->incomplete || t->size!=16 || alignment_of_type(t)!=8 ||
     !t->source_corresp.name || string(t->source_corresp.name)!="timespec")return false;
  auto f=t->variant.class_struct_union.field_list;
  if(!f || f->is_bit_field || f->offset!=0 || type(f->type)!="long" || !f->source_corresp.name || string(f->source_corresp.name)!="tv_sec")return false;
  f=f->next;
  return f && !f->next && !f->is_bit_field && f->offset==8 && type(f->type)=="long" &&
    f->source_corresp.name && string(f->source_corresp.name)=="tv_nsec";
}
string thread_abi_type(a_type_ptr t) {
  if(thread_timespec_pointer(t))return "timespec*";
  auto raw=skip_typerefs(t);
  if(raw->kind==tk_pointer && raw->size==8) {
    auto function=skip_typerefs(raw->variant.pointer.type);
    if(function->kind==tk_routine) {
      auto extra=function->variant.routine.extra_info;
      if(extra->has_ellipsis || !extra->prototyped)return "invalid-callback";
      string signature=type(function->variant.routine.return_type)+"(";
      for(auto p=extra->param_type_list;p;p=p->next) {
        if(p!=extra->param_type_list)signature+=",";
        signature+=type(p->type);
      }
      return signature+")";
    }
  }
  return type(t);
}
bool thread_routine(a_routine_ptr r) {
  if(!libc_enabled || r->function_def_number!=NULL_function_def_number)return false;
  if(thread_routines.count(r))return true;
  string symbol=r->source_corresp.name?r->source_corresp.name:"";
  if(symbol.compare(0,13,"__edg_libcpp_")!=0 && symbol.compare(0,10,"__edg_c11_")!=0)return false;
  struct binding {const char *symbol,*result,*parameters,*expression;};
  const binding bindings[]={
#define EDG_C11_THREAD(s,r,p,e) {s,r,p,e},
#include "libc-c11-threads-bindings.def"
#undef EDG_C11_THREAD
    {"__edg_libcpp_atomic_table","void*","","global::Edg.Runtime.LibcxxAtomicWait.Table()"},
    {"__edg_libcpp_atomic_wait64","void","void*,long","global::Edg.Runtime.LibcxxAtomicWait.Wait64(a0,a1)"},
    {"__edg_libcpp_atomic_wake64","void","void*","global::Edg.Runtime.LibcxxAtomicWait.Wake64(a0)"},
    {"__edg_libcpp_mutex_lock","int","ulong*,int","__edg_thread_services.MutexLock(a0,a1!=0)"},
    {"__edg_libcpp_mutex_trylock","int","ulong*,int","(__edg_thread_services.MutexTryLock(a0,a1!=0)?1:0)"},
    {"__edg_libcpp_mutex_unlock","int","ulong*,int","__edg_thread_services.MutexUnlock(a0,a1!=0)"},
    {"__edg_libcpp_mutex_destroy","int","ulong*,int","__edg_thread_services.MutexDestroy(a0,a1!=0)"},
    {"__edg_libcpp_recursive_mutex_init","int","ulong*","__edg_thread_services.RecursiveMutexInit(a0)"},
    {"__edg_libcpp_condvar_signal","int","ulong*,int","__edg_thread_services.ConditionSignal(a0,a1!=0)"},
    {"__edg_libcpp_condvar_wait","int","ulong*,ulong*,timespec*","__edg_thread_services.ConditionWait(a0,a1,(@RUNTIME@.Timespec*)a2)"},
    {"__edg_libcpp_condvar_destroy","int","ulong*","__edg_thread_services.ConditionDestroy(a0)"},
    {"__edg_libcpp_execute_once","int","ulong*,void()","__edg_thread_services.ExecuteOnce(a0,a1)"},
    {"__edg_libcpp_thread_create","int","ulong*,void*(void*),void*","__edg_thread_services.ThreadCreate(a0,a1,a2)"},
    {"__edg_libcpp_thread_current_id","ulong","","@RUNTIME@.CurrentThreadId()"},
    {"__edg_libcpp_thread_get_id","ulong","ulong*","__edg_thread_services.ThreadGetId(a0)"},
    {"__edg_libcpp_thread_join","int","ulong*","__edg_thread_services.ThreadJoin(a0)"},
    {"__edg_libcpp_thread_detach","int","ulong*","__edg_thread_services.ThreadDetach(a0)"},
    {"__edg_libcpp_thread_yield","void","","@RUNTIME@.ThreadYield()"},
    {"__edg_libcpp_thread_sleep_for","void","long","@RUNTIME@.ThreadSleepFor(a0)"},
    {"__edg_libcpp_tls_create","int","ulong*,void(void*)","__edg_thread_services.TlsCreate(a0,a1)"},
    {"__edg_libcpp_tls_get","void*","ulong","__edg_thread_services.TlsGet(a0)"},
    {"__edg_libcpp_tls_set","int","ulong,void*","__edg_thread_services.TlsSet(a0,a1)"},
  };
  for(const auto &binding:bindings)if(symbol==binding.symbol) {
    auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
    string parameters,signature;unsigned index=0;
    for(auto p=extra->param_type_list;p;p=p->next) {
      if(index){parameters+=",";signature+=", ";}
      parameters+=thread_abi_type(p->type);signature+=type(p->type)+" a"+std::to_string(index++);
    }
    string result=type(rt->variant.routine.return_type);
    if(extra->has_ellipsis || !extra->prototyped || parameters!=binding.parameters || result!=binding.result)
      unsupported(("thread import ABI for "+symbol+": expected "+binding.result+"("+binding.parameters+") but got "+result+"("+parameters+")").c_str());
    // Stateless address-wait buckets neither retain owners nor need thread/TLS
    // resources. Ordinary generated entry leases still protect caller storage.
    if(symbol!="__edg_libcpp_atomic_wait64" && symbol!="__edg_libcpp_atomic_wake64" && symbol!="__edg_libcpp_atomic_table")ensure_thread_services();
    thread_routines.insert(r);
    string expression=binding.expression,runtime=thread_runtime_type();
    for(size_t at=expression.find("@RUNTIME@");at!=string::npos;at=expression.find("@RUNTIME@",at+runtime.size()))expression.replace(at,9,runtime);
    helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+name(r,"f")+"("+signature+") { "+
      (result=="void"?"":"return ")+expression+"; }\n// dot-gen:member-end\n";
    return true;
  }
  return false;
}
