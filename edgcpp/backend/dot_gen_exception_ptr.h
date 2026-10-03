// libc++ dependent exception ABI. The public token is the native payload
// address; a private preceding word roots its shared managed storage owner.
bool exception_ptr_helpers_emitted=false;
void collect_primary_exception_types() {
  bool used=false;
  for(auto r=il_header.primary_scope->routines;r;r=r->next)
    if(routine_needed(r) && r->source_corresp.name &&
       string(r->source_corresp.name)=="__cxa_init_primary_exception") {used=true;break;}
  if(!used)return;
  // This runs before catch matchers are emitted. A type constructed by
  // make_exception_ptr need never appear in an ordinary throw expression.
  for(auto &entry:rtti_lowered_types) {
    auto t=skip_typerefs(entry.second);
    if(t->kind!=tk_void && t->kind!=tk_routine && t->kind!=tk_array && t->size)
      (void)exception_key(entry.second);
  }
}
void ensure_exception_ptr_helpers() {
  if(exception_ptr_helpers_emitted)return;
  exception_ptr_helpers_emitted=true;
  ensure_exception_helpers();
  ensure_typeinfo_helpers();
  string mod=lifecycle_mod();
  string owner=instance_mode?"this":"null";
  string receiver=instance_mode?"(("+string(getenv("EDG_DOT_CLASS"))+")owner!).":"";
  string callback="delegate* managed<"+string(instance_mode?string(getenv("EDG_DOT_CLASS"))+", ":"")+"void*,void>";
  string invoke="(("+callback+")function)("+string(instance_mode?"("+string(getenv("EDG_DOT_CLASS"))+")owner!, ":"")+"address);";
  helper_output+=R"CS(
// dot-gen:member-begin
private static void* __edg_current_primary_exception(void* holder) {
  if(__edg_handled_exception is not __edg_exception error)return null;
  error.Storage.RetainPrimary(holder);return error.Address;
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_increment_exception_refcount(void* token,void* holder) {
  if(token!=null)__edg_exception_storage.FromAddress(token).RetainPrimary(holder);
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_decrement_exception_refcount(void* token,void* holder) {
  if(token!=null)__edg_exception_storage.FromAddress(token).ReleasePrimary(holder);
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_rethrow_primary_exception(void* token) {
  if(token==null)return;
  __edg_exception error;
  try {error=new __edg_exception(__edg_exception_storage.FromAddress(token),true);}
  catch(global::System.OutOfMemoryException failure) {
    global::System.Environment.FailFast("Unable to allocate EDG exception carrier",failure);throw;
  }
  __edg_start_exception(error);throw error;
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_free_primary_exception(void* token) {
  if(token!=null)__edg_exception_storage.FromAddress(token).Abandon(1);
}
// dot-gen:member-end
)CS";
  helper_output+="// dot-gen:member-begin\nprivate static void __edg_primary_destructor(object? owner,byte* address,nint function) {\n"+
      receiver+"__edg_enter_exception_disposal();try {"+invoke+"} finally {"+receiver+"__edg_leave();}\n}\n// dot-gen:member-end\n";
  unsigned alignment=16;
  string cases;
  for(auto &entry:rtti_lowered_types) {
    auto t=skip_typerefs(entry.second);
    // typeid may name non-object types which can never be exception payloads.
    if(t->kind==tk_void || t->kind==tk_routine || t->kind==tk_array || !t->size)continue;
    unsigned required=(unsigned)alignment_of_type(t);
    if(!required || required>4096 || (required&(required-1)))unsupported("primary exception payload alignment");
    alignment=std::max(alignment,required);
    cases+="case "+std::to_string(virtual_types.at(entry.first))+": storage.InitializePrimary("+exception_key(entry.second)+",function,&__edg_primary_destructor);return token;\n";
  }
  helper_output+="// dot-gen:member-begin\nprivate "+mod+"void* __edg_allocate_primary_exception(ulong size) {\n"+
      "if(size>(ulong)nint.MaxValue-"+std::to_string(alignment+sizeof(void*))+"UL)global::System.Environment.FailFast(\"Exception payload exceeds supported native size\");\n"+
      "void* raw=null;bool leased=false;__edg_exception_storage? storage=null;try {__edg_prepare_exception_ledger();__edg_acquire_exception_lease();leased=true;raw=__edg_exception_alloc((nuint)size+"+std::to_string(alignment-1)+"+(nuint)sizeof(nint));\n"+
      "storage=new __edg_exception_storage(0,"+std::to_string(alignment)+",raw,(nuint)size,null,"+owner+",&__edg_release_exception_owner);raw=null;leased=false;storage.PreparePrimary();return storage.Address;\n"+
      "} catch(global::System.OutOfMemoryException error) {if(storage!=null)storage.Abandon(1);else {global::System.Runtime.InteropServices.NativeMemory.Free(raw);if(leased)__edg_release_exception_owner("+owner+");}global::System.Environment.FailFast(\"Unable to allocate primary exception\",error);throw;}\n}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate static void* __edg_init_primary_exception(void* token,void* type,nint function) {\n"
      "var storage=__edg_exception_storage.FromAddress(token);long id=(byte*)type-__edg_type_token_base;\n"
      "if(id>=0 && (id&7)==0)switch(id/8) {\n"+cases+"default: break;\n}\nglobal::System.Environment.FailFast(\"Unknown primary exception type_info\");return null;\n}\n// dot-gen:member-end\n";

}
string exception_ptr_call(a_routine_ptr r,an_expr_node_ptr args) {
  if(r->function_def_number!=NULL_function_def_number)return "";
  string source=r->source_corresp.name?r->source_corresp.name:"";
  if(source!="__cxa_current_primary_exception" && source!="__cxa_increment_exception_refcount" &&
     source!="__cxa_decrement_exception_refcount" && source!="__cxa_rethrow_primary_exception" &&
     source!="__cxa_allocate_exception" && source!="__cxa_free_exception" && source!="__cxa_init_primary_exception" &&
     source!="__edg_cxx_terminate" && source!="__edg_cxx_terminate_default" &&
     source!="__edg_cxx_set_terminate" && source!="__edg_cxx_get_terminate" && source!="__edg_cxx_uncaught_exceptions" &&
     source!="__edg_cxx_current_exception" && source!="__edg_cxx_increment_exception_refcount" &&
     source!="__edg_cxx_decrement_exception_refcount")return "";
  ensure_exception_ptr_helpers();
  std::vector<an_expr_node_ptr> values;for(auto p=args;p;p=p->next)values.push_back(p);
  if(source=="__edg_cxx_current_exception") {
    if(values.size()!=1)unsupported("holder current exception ABI");
    return "__edg_current_primary_exception((void*)("+expr(values[0])+"))";
  }
  if(source=="__edg_cxx_increment_exception_refcount" || source=="__edg_cxx_decrement_exception_refcount") {
    if(values.size()!=2)unsupported("holder exception reference ABI");
    string helper=source=="__edg_cxx_increment_exception_refcount"?"__edg_increment_exception_refcount":"__edg_decrement_exception_refcount";
    return helper+"((void*)("+expr(values[0])+"),(void*)("+expr(values[1])+"))";
  }
  if(source=="__cxa_current_primary_exception" || source=="__edg_cxx_terminate" || source=="__edg_cxx_terminate_default" || source=="__edg_cxx_get_terminate" || source=="__edg_cxx_uncaught_exceptions") {
    if(!values.empty())unsupported("primary exception no-argument ABI");
    if(source=="__edg_cxx_uncaught_exceptions")return "__edg_uncaught_exceptions";
    if(source=="__cxa_current_primary_exception")return "__edg_current_primary_exception(null)";
    if(source=="__edg_cxx_get_terminate")return "("+type(skip_typerefs(r->type)->variant.routine.return_type)+")global::System.Threading.Interlocked.CompareExchange(ref __edg_terminate_handler,0,0)";
    if(source=="__edg_cxx_terminate_default")return "__edg_default_terminate()";
    return "__edg_terminate()";
  }
  if(source=="__cxa_init_primary_exception") {
    if(values.size()!=3)unsupported("primary exception initialization ABI");
    return "("+type(skip_typerefs(r->type)->variant.routine.return_type)+")__edg_init_primary_exception((void*)("+expr(values[0])+"),(void*)("+expr(values[1])+"),(nint)("+expr(values[2])+"))";
  }
  if(values.size()!=1)unsupported("primary exception single-argument ABI");
  if(source=="__cxa_allocate_exception")return "__edg_allocate_primary_exception((ulong)("+expr(values[0])+"))";
  if(source=="__edg_cxx_set_terminate")return "("+type(skip_typerefs(r->type)->variant.routine.return_type)+")__edg_set_terminate((nint)("+expr(values[0])+"))";
  string helper=source=="__cxa_increment_exception_refcount"?"__edg_increment_exception_refcount":
      source=="__cxa_decrement_exception_refcount"?"__edg_decrement_exception_refcount":
      source=="__cxa_rethrow_primary_exception"?"__edg_rethrow_primary_exception":"__edg_free_primary_exception";
  return helper+"((void*)("+expr(values[0])+")"+
      string(source=="__cxa_increment_exception_refcount" || source=="__cxa_decrement_exception_refcount"?",null":"")+")";
}
