// Process-only libc termination. Include after dot_gen_threads.h so the exact
// typed callback validator is available. Owner embedding mode remains disabled.
bool process_exit_helpers_emitted=false;
std::set<a_routine_ptr> process_exit_routines;
void reset_process_exit_imports() {
  process_exit_helpers_emitted=false;
  process_exit_routines.clear();
}
void ensure_process_exit() {
  if(process_exit_helpers_emitted)return;
  process_exit_helpers_emitted=true;
  ensure_lifecycle();
  string process_code=R"CS(
// dot-gen:process-exit process-v1
private @MOD@int __edg_process_exit_started;
private @MOD@readonly global::System.Collections.Generic.List<__edg_finalizer> __edg_quick_finalizers=new();
// dot-gen:member-begin
private @MOD@int __edg_register_exit(nint function,bool quick) {
  if(function==0)return 1;
  try {
    lock(__edg_sync) {
      if(__edg_state==2)return 1;
      (quick?__edg_quick_finalizers:__edg_finalizers).Add(new __edg_finalizer(function,0,true));
      return 0;
    }
  } catch(global::System.OutOfMemoryException) {return 1;}
}
// dot-gen:member-end
// dot-gen:member-begin
[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
private @MOD@void __edg_process_exit(int status,bool quick) {
  if(global::System.Threading.Interlocked.CompareExchange(ref __edg_process_exit_started,1,0)!=0)
    global::System.Environment.FailFast("Repeated translated process termination request");
  if(quick) {
    __edg_drain(__edg_quick_finalizers);
  } else {
    __edg_drain(__edg_threads.Value!.Finalizers);
    __edg_drain(__edg_finalizers);
    /*__EDG_PROCESS_FLUSH__*/
  }
  // Never call Shutdown here: other source threads may still hold pointers,
  // locks and leases. The process primitive terminates all of them together.
  global::Edg.Runtime.LibcProcessExit.Exit(status);
}
// dot-gen:member-end
// dot-gen:member-begin
[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]
public @MOD@void __EdgProcessExit(int status) {__edg_process_exit(status,false);}
// dot-gen:member-end
)CS";
  string process_modifier=instance_mode?"":"static ";
  for(size_t process_at=process_code.find("@MOD@");process_at!=string::npos;process_at=process_code.find("@MOD@",process_at+process_modifier.size()))
    process_code.replace(process_at,5,process_modifier);
  helper_output+=process_code;
}
bool process_routine(a_routine_ptr process_routine_ptr) {
  if(!libc_enabled || process_routine_ptr->function_def_number!=NULL_function_def_number)return false;
  if(process_exit_routines.count(process_routine_ptr))return true;
  string process_symbol=process_routine_ptr->source_corresp.name?process_routine_ptr->source_corresp.name:"";
  struct process_binding {const char *symbol,*result,*parameters,*expression;};
  const process_binding process_bindings[]={
#define EDG_PROCESS_EXIT(s,r,p,e) {s,r,p,e},
#include "libc-process-exit-bindings.def"
#undef EDG_PROCESS_EXIT
  };
  for(const auto &process_binding_entry:process_bindings)if(process_symbol==process_binding_entry.symbol) {
    auto process_type=skip_typerefs(process_routine_ptr->type);
    auto process_extra=process_type->variant.routine.extra_info;
    string process_parameters,process_signature;
    unsigned process_index=0;
    for(auto process_parameter=process_extra->param_type_list;process_parameter;process_parameter=process_parameter->next) {
      if(process_index){process_parameters+=",";process_signature+=", ";}
      process_parameters+=thread_abi_type(process_parameter->type);
      process_signature+=type(process_parameter->type)+" a"+std::to_string(process_index++);
    }
    string process_result=type(process_type->variant.routine.return_type);
    if(process_extra->has_ellipsis || !process_extra->prototyped ||
       process_parameters!=process_binding_entry.parameters || process_result!=process_binding_entry.result)
      unsupported(("process import ABI for "+process_symbol+": expected "+process_binding_entry.result+"("+process_binding_entry.parameters+") but got "+process_result+"("+process_parameters+")").c_str());
    ensure_process_exit();
    process_exit_routines.insert(process_routine_ptr);
    libc_register_routine(process_routine_ptr,process_symbol);
    helper_output+="// dot-gen:import "+process_symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+
      process_result+" "+name(process_routine_ptr,"f")+"("+process_signature+") { "+
      string(process_result=="void"?"":"return ")+process_binding_entry.expression+"; }\n// dot-gen:member-end\n";
    return true;
  }
  return false;
}
