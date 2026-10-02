// Explicit import contracts for the portable LP64 libc bridge. Definitions in
// translated input always win; an unknown symbol or mismatched ABI is an error.
bool libc_enabled=false,libc_emitted=false,files_emitted=false,text_emitted=false;
std::set<a_routine_ptr> libc_routines;
struct libc_binding {const char *symbol,*result,*parameters,*target;};
const libc_binding libc_bindings[]={
#define EDG_LIBC(s,r,p,t) {s,r,p,t},
#include "libc-bindings.def"
#include "libc-files-bindings.def"
#include "libc-text-bindings.def"
#include "libc-float-bindings.def"
#undef EDG_LIBC
};
void reset_libc_imports() {
  libc_emitted=false;files_emitted=false;text_emitted=false;libc_routines.clear();
}
string libc_thread_release_code() {return libc_emitted?"__edg_libc.ReleaseThread();":"";}
string libc_disposal_code() {
  return string(files_emitted?"__edg_files_value?.Dispose();":"")+
    (text_emitted?"__edg_text_value?.Dispose();":"")+(libc_emitted?"__edg_libc.Dispose();":"");
}
void ensure_lifecycle();
void ensure_libc() {
  if(libc_emitted)return;
  libc_emitted=true;ensure_lifecycle();
  string code=R"CS(
private @MOD@readonly global::Edg.Runtime.LibcContext __edg_libc=new();
// dot-gen:member-begin
private @MOD@void* __edg_malloc(ulong n) {
  void* p=__edg_heap.Allocate(n,16);if(p==null)__edg_libc.SetErrno(12);return p;
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_free(void* p) {__edg_heap_owner.Free(p);}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void* __edg_calloc(ulong count,ulong size) {
  if(size!=0 && count>ulong.MaxValue/size){__edg_libc.SetErrno(12);return null;}
  ulong n=count*size;void* p=__edg_malloc(n);
  if(p!=null)global::System.Runtime.InteropServices.NativeMemory.Clear(p,(nuint)n);return p;
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void* __edg_realloc(void* old,ulong size) {
  if(old==null)return __edg_malloc(size);
  if(size==0){__edg_free(old);return null;}
  ulong previous=__edg_heap_owner.Size(old);void* p=__edg_malloc(size);
  if(p==null)return null;
  global::System.Runtime.InteropServices.NativeMemory.Copy(old,p,(nuint)global::System.Math.Min(previous,size));
  __edg_free(old);return p;
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void* __edg_aligned_alloc(ulong alignment,ulong size) {
  if(alignment==0 || (alignment&(alignment-1))!=0 || size%alignment!=0){__edg_libc.SetErrno(22);return null;}
  void* p=__edg_heap.Allocate(size,alignment);if(p==null)__edg_libc.SetErrno(12);return p;
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@int __edg_posix_memalign(void** result,ulong alignment,ulong size) {
  if(alignment<8 || (alignment&(alignment-1))!=0)return 22;
  void* p=__edg_heap.Allocate(size,alignment);if(p==null)return 12;*result=p;return 0;
}
// dot-gen:member-end
)CS";
  string mod=instance_mode?"":"static ";
  for(size_t pos=code.find("@MOD@");pos!=string::npos;pos=code.find("@MOD@",pos+mod.size()))code.replace(pos,5,mod);
  helper_output+=code;
}
void ensure_files() {
  if(files_emitted)return;
  ensure_libc();files_emitted=true;
  // Instance field initializers cannot refer to other instance fields. A lazy
  // holder also avoids opening standard streams when only memory APIs are used.
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcFiles? __edg_files_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcFiles __edg_files { get { lock(__edg_libc) { if(__edg_files_value==null)__edg_files_value=new global::Edg.Runtime.LibcFiles(__edg_libc.SetErrno); return __edg_files_value; } } }\n";
}
void ensure_text() {
  if(text_emitted)return;
  ensure_libc();text_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcText? __edg_text_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcText __edg_text { get { lock(__edg_libc) { if(__edg_text_value==null)__edg_text_value=new global::Edg.Runtime.LibcText(__edg_libc.SetErrno); return __edg_text_value; } } }\n";
}
bool libc_file_pointer(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer)return false;
  t=skip_typerefs(t->variant.pointer.type);
  return (t->kind==tk_struct || t->kind==tk_class) && t->incomplete &&
    t->source_corresp.name && string(t->source_corresp.name)=="__edg_FILE" &&
    t->source_corresp.parent_scope && t->source_corresp.parent_scope->kind==sck_file;
}
string libc_abi_type(a_type_ptr t) {return libc_file_pointer(t)?"FILE*":type(t);}
bool libc_va_pointer(a_type_ptr t) {
  t=skip_typerefs(t);
  if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  return va_tag_type(t) && t->size==24 && alignment_of_type(t)==8;
}
bool libc_format_callback(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);if(t->kind!=tk_routine)return false;
  auto extra=t->variant.routine.extra_info;
  if(extra->has_ellipsis || !extra->prototyped || type(t->variant.routine.return_type)!="int")return false;
  auto p=extra->param_type_list;
  if(!p || type(p->type)!="void*")return false;
  p=p->next;
  if(!p || type(p->type)!="sbyte*")return false;
  p=p->next;
  return p && libc_va_pointer(p->type) && !p->next;
}
bool libc_format_lock_routine(a_routine_ptr r) {
  auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
  std::vector<a_type_ptr> parameters;
  for(auto p=extra->param_type_list;p;p=p->next)parameters.push_back(p->type);
  if(extra->has_ellipsis || !extra->prototyped || type(rt->variant.routine.return_type)!="int" ||
     parameters.size()!=5 || !libc_file_pointer(parameters[0]) ||
     !libc_format_callback(parameters[1]) || type(parameters[2])!="void*" ||
     type(parameters[3])!="sbyte*" || !libc_va_pointer(parameters[4]))
    unsupported("libc import ABI for __edg_with_file_lock: expected int(FILE*, int(*)(void*,const char*,va_list),void*,const char*,va_list) with builtin va_list");
  ensure_files();libc_routines.insert(r);
  string signature;
  for(unsigned i=0;i<parameters.size();++i) {
    if(i)signature+=", ";
    signature+=type(parameters[i])+" a"+std::to_string(i);
  }
  // This is an ordinary routine wrapper: address-taking reuses the canonical
  // routine_address adapter, including its owner lifecycle lease and identity.
  helper_output+="// dot-gen:import __edg_with_file_lock\n// dot-gen:member-begin\nprivate "+
    string(instance_mode?"":"static ")+"int "+name(r,"f")+"("+signature+") { return __edg_files.WithFileLock((void*)a0, a1, "+
    string(instance_mode?"this, ":"")+"a2, a3, a4); }\n// dot-gen:member-end\n";
  return true;
}
bool libc_routine(a_routine_ptr r) {
  if(!libc_enabled || r->function_def_number!=NULL_function_def_number)return false;
  if(libc_routines.count(r))return true;
  string symbol=r->source_corresp.name?r->source_corresp.name:"";
  if(symbol=="__edg_with_file_lock")return libc_format_lock_routine(r);
  for(const auto &binding:libc_bindings)if(symbol==binding.symbol) {
    auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
    string parameters,signature,args;unsigned index=0;
    for(auto p=extra->param_type_list;p;p=p->next) {
      string ty=type(p->type),arg="a"+std::to_string(index++);
      if(p!=extra->param_type_list){parameters+=",";signature+=", ";args+=", ";}
      parameters+=libc_abi_type(p->type);signature+=ty+" "+arg;args+=(libc_file_pointer(p->type)?"(void*)":"")+arg;
    }
    string result=type(rt->variant.routine.return_type);
    if(extra->has_ellipsis || parameters!=binding.parameters || libc_abi_type(rt->variant.routine.return_type)!=binding.result)
      unsupported(("libc import ABI for "+symbol+": expected "+binding.result+"("+binding.parameters+") but got "+result+"("+parameters+")").c_str());
    ensure_libc();if(string(binding.target).find("__edg_files.")==0)ensure_files();
    if(string(binding.target).find("__edg_text.")==0)ensure_text();
    libc_routines.insert(r);
    helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+name(r,"f")+"("+signature+") { "+
      (result=="void"?"":"return ")+(libc_file_pointer(rt->variant.routine.return_type)?"("+result+")":"")+binding.target+"("+args+"); }\n// dot-gen:member-end\n";
    return true;
  }
  return false;
}
string libc_variable(a_variable_ptr v) {
  if(!libc_enabled || !v->source_corresp.name)return "";
  string symbol=v->source_corresp.name;
  if(symbol=="stdin" || symbol=="stdout" || symbol=="stderr") {
    if(!libc_file_pointer(v->type))unsupported("libc standard stream ABI (opaque FILE* required)");
    ensure_files();string member=symbol=="stdin"?"StdinLocation":symbol=="stdout"?"StdoutLocation":"StderrLocation";
    return "(*("+type(v->type)+"*)__edg_files."+member+")";
  }
  if(symbol!="errno")return "";
  if(type(v->type)!="int")unsupported("libc errno ABI (int required)");
  ensure_libc();return "(*__edg_libc.ErrnoLocation())";
}
