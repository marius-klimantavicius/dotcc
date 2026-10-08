bool array_helpers_emitted = false;
void ensure_terminate_helpers();
string array_callback_type() {
  return "delegate* managed<"+string(instance_mode?string(getenv("EDG_DOT_CLASS"))+", ":"")+"void*, void>";
}
void ensure_array_helpers() {
  if(array_helpers_emitted)return;
  array_helpers_emitted=true;
  ensure_terminate_helpers();
  // One canonical C# template is also consumed by the owned-object runtime
  // binder. Only callback owner/signature and transfer policies are substituted.
  string code=R"CS(
// dot-gen:member-begin
private @MOD@bool __edg_array_cleanup_failure(global::System.Exception error) {
  if(!(@CONTROL_TRANSFER_EXCLUSION@))return false;
  __edg_terminate_runtime(error,"Array destructor threw during unwinding");
  return false;
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_vec_cleanup(void* data, ulong count, ulong stride, @CALLBACK@ dtor) {
  try { if (dtor != null) while (count != 0) { --count; dtor(@OWNER@(byte*)data + count * stride); } }
  catch (global::System.Exception error) when (__edg_array_cleanup_failure(error)) { throw; }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_vec_dtor(void* data, ulong count, ulong stride, @CALLBACK@ dtor) {
  try { if (dtor != null) while (count != 0) { --count; dtor(@OWNER@(byte*)data + count * stride); } }
  catch (global::System.Exception jump_check) when (@SOURCE_JUMP_EXCLUSION@) { __edg_vec_cleanup(data, count, stride, dtor); throw; }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_vec_ctor(void* data, ulong count, ulong stride, @CALLBACK@ ctor, @CALLBACK@ dtor) {
  ulong done = 0;
  try { if (ctor != null) for (; done < count; ++done) ctor(@OWNER@(byte*)data + done * stride); }
  catch (global::System.Exception jump_check) when (@SOURCE_JUMP_EXCLUSION@) { __edg_vec_cleanup(data, done, stride, dtor); throw; }
}
// dot-gen:member-end
)CS";
  auto substitute=[&](const string &token,const string &value) {
    size_t at=0;
    while((at=code.find(token,at))!=string::npos) {code.replace(at,token.size(),value);at+=value.size();}
  };
  substitute("@MOD@",instance_mode?"":"static ");
  substitute("@CALLBACK@",array_callback_type());
  substitute("@OWNER@",instance_mode?"this, ":"");
  substitute("@CONTROL_TRANSFER_EXCLUSION@",control_transfer_exclusion("error"));
  substitute("@SOURCE_JUMP_EXCLUSION@",source_jump_exclusion("jump_check"));
  helper_output+=code;
}
// Match lib_src/vec_newdel.c: cookie immediately before data, reverse
// destruction, completed-prefix cleanup and deallocation on exceptions.
std::set<string> array_allocation_helpers;
string array_allocation_call(a_routine_ptr r,an_expr_node_ptr args) {
  string source=r->source_corresp.name?r->source_corresp.name:"";
  bool create=source=="__cxa_vec_new" || source=="__cxa_vec_new2" || source=="__cxa_vec_new3";
  bool destroy=source=="__cxa_vec_delete" || source=="__cxa_vec_delete2" || source=="__cxa_vec_delete3";
  if(!r->compiler_generated || r->function_def_number!=NULL_function_def_number || (!create && !destroy))return "";
  ensure_array_helpers();
  bool supplied=source.back()=='2'||source.back()=='3',sized=source.back()=='3';
  int count=0;string call="__edg"+source+"(";
  for(auto a=args;a;a=a->next){if(count++)call+=", ";call+=expr(a);}call+=")";
  if(count!=(create?(supplied?7:5):(supplied?5:4)))unsupported("array allocation helper ABI");
  if(array_allocation_helpers.insert(source).second) {
    string owner=instance_mode?"this, ":"",fp=array_callback_type(),mod=instance_mode?"":"static ";
    string alloc="allocator("+owner+"size)",release="deallocator("+owner+"raw"+string(sized?", size":"")+")";
    if(!supplied) {
      a_routine_ptr new_array=nullptr,delete_array=nullptr;
      for(auto p=il_header.primary_scope->routines;p;p=p->next)if(p->source_corresp.name && p->function_def_number!=NULL_function_def_number) {
        string n=p->source_corresp.name;if(n=="_Znam")new_array=p;if(n=="_ZdaPv")delete_array=p;
      }
      if(!delete_array || (create && !new_array))unsupported("missing array allocation provider");
      if(create)alloc=source_routine_reference(new_array,true)+"(size)";
      release=source_routine_reference(delete_array,true)+"(raw)";
    }
    string signature=create?"void* __edg"+source+"(ulong count, ulong stride, ulong prefix, "+fp+" ctor, "+fp+" dtor":
        "void __edg"+source+"(void* data, ulong stride, ulong prefix, "+fp+" dtor";
    if(supplied) {
      string owner_type=instance_mode?string(getenv("EDG_DOT_CLASS"))+", ":"";
      if(create)signature+=", delegate* managed<"+owner_type+"ulong, void*> allocator";
      signature+=", delegate* managed<"+owner_type+"void*, "+string(sized?"ulong, ":"")+"void> deallocator";
    }
    string code="// dot-gen:member-begin\nprivate "+mod+signature+") {\n";
    if(create) {
      a_routine_ptr bad=nullptr;
      // lib_src/vec_newdel.c delegates overflow to __cxa_throw_bad_array_new_length.
      // A translated libc++ ABI provider can define that entry directly; retain
      // the explicit bootstrap helper for the existing edg-exceptions provider.
      for(auto p=il_header.primary_scope->routines;p;p=p->next)if(p->source_corresp.name &&
          p->function_def_number!=NULL_function_def_number) {
        string n=p->source_corresp.name;
        if(n=="__cxa_throw_bad_array_new_length" || (n=="__edg_dot_throw_bad_array_new_length" && !bad))bad=p;
      }
      if(!bad)unsupported("missing bad_array_new_length provider");
      auto bad_type=skip_typerefs(bad->type);
      if(bad_type->variant.routine.extra_info->param_type_list ||
         bad_type->variant.routine.extra_info->has_ellipsis ||
         skip_typerefs(bad_type->variant.routine.return_type)->kind!=tk_void)
        unsupported("bad_array_new_length provider ABI");
      code+="if (stride != 0 && count > (ulong.MaxValue-prefix)/stride) "+source_routine_reference(bad,true)+"();\n"
          "ulong bytes=count*stride; ulong size=(bytes==0?1UL:bytes)+prefix;\n"
          "void* raw="+alloc+"; if(raw==null)return null; void* data=(byte*)raw+prefix;\n"
          "if(prefix!=0)*((ulong*)data-1)=count;\n"
          "try {__edg_vec_ctor(data,count,stride,ctor,dtor);return data;}\n"
          "catch (global::System.Exception jump_check) when ("+source_jump_exclusion("jump_check")+") {try {"+release+";}catch(global::System.Exception error) when ("+control_transfer_exclusion("error")+"){__edg_terminate_runtime(error,\"Array deallocator threw during unwinding\");}throw;}\n";
    } else {
      code+="if(data==null)return;void* raw=(byte*)data-prefix;ulong count=prefix==0?0:*((ulong*)data-1);\n"
          "ulong size=count*stride+prefix;try{__edg_vec_dtor(data,count,stride,dtor);}finally{"+release+";}\n";
    }
    helper_output+=code+"}\n// dot-gen:member-end\n";
  }
  return call;
}
