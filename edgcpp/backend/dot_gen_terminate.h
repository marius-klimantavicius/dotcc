// Shared C++ terminate state. Each generated owner has its own configured
// handler; recursion and the runtime uncaught-count mask belong to the thread.
bool terminate_helpers_emitted=false;
void ensure_exception_helpers();
// Resolve the same source C symbol that the EDG runtime's abort call links to.
// Never emit a call to a pruned body or accept a name-only ABI match.
string terminate_source_abort() {
  if(!libc_enabled)return "";
  a_routine_ptr definition=nullptr;
  for(auto candidate=il_header.primary_scope->routines;candidate;candidate=candidate->next) {
    if(!candidate->source_corresp.name || string(candidate->source_corresp.name)!="abort" ||
       candidate->source_corresp.name_linkage!=nlk_external ||
       candidate->function_def_number==NULL_function_def_number)continue;
    if(definition && definition!=candidate)unsupported("multiple source abort definitions for default terminate");
    auto routine_type=skip_typerefs(candidate->type);
    auto details=routine_type->variant.routine.extra_info;
    if(skip_typerefs(routine_type->variant.routine.return_type)->kind!=tk_void ||
       details->param_type_list || details->has_ellipsis || !details->prototyped ||
       !routine_body_needed(candidate) || !scope_for_routine_or_null(candidate))
      unsupported("source abort ABI or unavailable body for default terminate");
    definition=candidate;
  }
  return definition?source_routine_reference(definition,true)+"()":"";
}
void ensure_terminate_helpers() {
  if(terminate_helpers_emitted)return;
  terminate_helpers_emitted=true;
  ensure_exception_helpers();
  string mod=instance_mode?"":"static ";
  string callback="delegate* managed<"+string(instance_mode?string(getenv("EDG_DOT_CLASS"))+", ":"")+"void>";
  string invoke="(("+callback+")handler)("+string(instance_mode?"this":"")+");";
  helper_output+="// dot-gen:termination owner-v1\n";
  string source_abort=terminate_source_abort();
  helper_output+="// dot-gen:member-begin\n[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]\nprivate "+mod+"void __edg_default_terminate() {\n";
  if(!source_abort.empty()) {
    helper_output+="try {"+source_abort+";} catch(global::System.Exception failure) when(__edg_default_abort_failure(failure)) {throw;}";
  }
  helper_output+="global::System.Environment.FailFast(\"C++ default terminate\");\n}\n// dot-gen:member-end\n";

  if(!source_abort.empty()) {
    // Fatal in the CLR first-pass filter, before abort/handler stack cleanup.
    helper_output+="// dot-gen:member-begin\nprivate static bool __edg_default_abort_failure(global::System.Exception failure) {"+
      string(source_jumps_used?"if(failure is global::Edg.Runtime.NonlocalJump.Transfer)global::System.Environment.FailFast(\"Nonlocal jump from C++ terminate handler\",failure);":"")+
      string(c11_thread_exit_used?"if(failure is global::Edg.Runtime.CThreadExitTransfer)global::System.Environment.FailFast(\"Thread exit from C++ terminate handler\",failure);":"")+
      "global::System.Environment.FailFast(\"Default C++ terminate abort failed\",failure);return false;}\n// dot-gen:member-end\n";
  }
  helper_output+="private "+mod+"nint __edg_terminate_handler;\n";
  helper_output+="[global::System.ThreadStatic] private static bool __edg_terminate_entered;\n";
  helper_output+="[global::System.ThreadStatic] private static bool __edg_runtime_terminating;\n";
  helper_output+="private static int __edg_uncaught_exceptions => __edg_runtime_terminating?0:__edg_uncaught_count;\n";
  helper_output+="// dot-gen:member-begin\nprivate "+mod+"nint __edg_set_terminate(nint handler) {return global::System.Threading.Interlocked.Exchange(ref __edg_terminate_handler,handler);}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\n[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]\nprivate "+mod+"void __edg_terminate_core(bool runtime,global::System.Exception? error,string reason) {\n"
    "if(__edg_terminate_entered)global::System.Environment.FailFast(\"Recursive C++ terminate\",error);\n"
    "__edg_terminate_entered=true;if(runtime)__edg_runtime_terminating=true;\n"
    "nint handler=global::System.Threading.Interlocked.CompareExchange(ref __edg_terminate_handler,0,0);\n"
    "if(runtime && error!=null) {var frame=__edg_create_catch_frame(error,0,0,false);frame.MarkCaught();}\n"
    "if(handler==0)__edg_default_terminate();\n"
    "if(handler!=0) {\n"
    "try {"+invoke+"} catch(global::System.Exception failure) {"+
    string(source_jumps_used?"if(failure is global::Edg.Runtime.NonlocalJump.Transfer)global::System.Environment.FailFast(\"Nonlocal jump from C++ terminate handler\",failure);":"")+
    string(c11_thread_exit_used?"if(failure is global::Edg.Runtime.CThreadExitTransfer)global::System.Environment.FailFast(\"Thread exit from C++ terminate handler\",failure);":"")+
    "global::System.Environment.FailFast(\"C++ terminate handler threw\",failure);}\n"
    "}\nglobal::System.Environment.FailFast(reason,error);\n}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\n[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]\nprivate "+mod+"void __edg_terminate() {__edg_terminate_core(false,null,\"C++ terminate\");}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\n[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]\nprivate "+mod+"void __edg_terminate_runtime(global::System.Exception? error,string reason) {"+
    string(source_jumps_used?"if(error is global::Edg.Runtime.NonlocalJump.Transfer)global::System.Environment.FailFast(\"Nonlocal jump escaped translated runtime boundary\",error);":"")+
    string(c11_thread_exit_used?"if(error is global::Edg.Runtime.CThreadExitTransfer)global::System.Environment.FailFast(\"Thread exit escaped translated thread boundary\",error);":"")+
    "__edg_terminate_core(true,error,reason);}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\ninternal "+mod+"void __EdgUnhandledProgram(global::System.Exception error) {__edg_terminate_runtime(error,\"Exception escaped translated main\");}\n// dot-gen:member-end\n";
}
