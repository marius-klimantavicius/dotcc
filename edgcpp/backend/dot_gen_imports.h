// Explicit import contracts for the portable LP64 libc bridge. Definitions in
// translated input always win; an unknown symbol or mismatched ABI is an error.
bool libc_enabled=false,libc_emitted=false,files_emitted=false,text_emitted=false,math_emitted=false,clock_emitted=false,environment_emitted=false,timezone_emitted=false;
std::set<a_routine_ptr> libc_routines;
std::map<string,a_routine_ptr> libc_binding_addresses;
std::map<a_routine_ptr,a_routine_ptr> libc_address_aliases;
a_routine_ptr libc_address_routine(a_routine_ptr r) {
  auto found=libc_address_aliases.find(r);
  return found==libc_address_aliases.end()?r:found->second;
}
void libc_register_routine(a_routine_ptr r,const string& symbol) {
  libc_routines.insert(r);
  auto inserted=libc_binding_addresses.emplace(symbol,r);
  libc_address_aliases[r]=inserted.first->second;
}
string libc_builtin_symbol(a_routine_ptr r) {
  // The C backend preserves these GNU intrinsic calls. Route only audited EDG
  // builtin identities, not arbitrary user spellings beginning __builtin_.
  if(!special_kind_is(r,sfk_none))return "";
  switch(r->variant.builtin_function_kind) {
    case bfk_memchr:return "memchr";
    case bfk_memcmp:return "memcmp";
    case bfk_memcpy:return "memcpy";
    case bfk_memmove:return "memmove";
    case bfk_memset:return "memset";
    case bfk_strlen:return "strlen";
    case bfk_strnlen:return "strnlen";
    case bfk_strcmp:return "strcmp";
    case bfk_strncmp:return "strncmp";
    case bfk_strcpy:return "strcpy";
    case bfk_strncpy:return "strncpy";
    case bfk_strcat:return "strcat";
    case bfk_strncat:return "strncat";
    case bfk_strchr:return "strchr";
    case bfk_strrchr:return "strrchr";
    case bfk_strstr:return "strstr";
    case bfk_strspn:return "strspn";
    case bfk_strcspn:return "strcspn";
    case bfk_strpbrk:return "strpbrk";
    case bfk_abs:return "abs";
    case bfk_labs:return "labs";
    case bfk_llabs:return "llabs";
    case bfk_fabs:return "fabs";
    case bfk_fabsf:return "fabsf";
    case bfk_fabsl:return "fabsl";
    case bfk_sqrt:return "sqrt";
    case bfk_sqrtf:return "sqrtf";
    case bfk_sqrtl:return "sqrtl";
    case bfk_copysign:return "copysign";
    case bfk_copysignf:return "copysignf";
    case bfk_copysignl:return "copysignl";
    case bfk_trunc:return "trunc";
    case bfk_truncf:return "truncf";
    case bfk_truncl:return "truncl";
    case bfk_floor:return "floor";
    case bfk_floorf:return "floorf";
    case bfk_floorl:return "floorl";
    case bfk_ceil:return "ceil";
    case bfk_ceilf:return "ceilf";
    case bfk_ceill:return "ceill";
    case bfk_roundl:return "roundl";
    default:return "";
  }
}
struct libc_binding {const char *symbol,*result,*parameters,*target;};
const libc_binding libc_bindings[]={
#define EDG_LIBC(s,r,p,t) {s,r,p,t},
#include "libc-bindings.def"
#include "libc-files-bindings.def"
#include "libc-wide-files-bindings.def"
#include "libc-text-bindings.def"
#include "libc-float-bindings.def"
#include "libc-math-bindings.def"
#include "libc-numeric-bindings.def"
#include "libc-locale-bindings.def"
#include "libc-clock-bindings.def"
#include "libc-cpu-clock-bindings.def"
#include "libc-calendar-bindings.def"
#include "libc-process-bindings.def"
#include "libc-assert-bindings.def"
#include "libc-environment-bindings.def"
#include "libc-timezone-bindings.def"
#undef EDG_LIBC
};
void reset_libc_imports() {
  libc_emitted=false;files_emitted=false;text_emitted=false;math_emitted=false;clock_emitted=false;environment_emitted=false;timezone_emitted=false;libc_routines.clear();
  libc_binding_addresses.clear();libc_address_aliases.clear();
}
string libc_thread_release_code() {return string(text_emitted?"__edg_text_value?.ReleaseThread();":"")+(libc_emitted?"__edg_libc.ReleaseThread();":"");}
string libc_disposal_code() {
  return string(files_emitted?"__edg_files_value?.Dispose();":"")+
    (text_emitted?"__edg_text_value?.Dispose();":"")+(environment_emitted?"__edg_environment_value?.Dispose();":"")+(timezone_emitted?"__edg_timezone_value?.Dispose();":"")+(libc_emitted?"__edg_libc.Dispose();":"");
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
void ensure_text();
void ensure_files() {
  if(files_emitted)return;
  ensure_libc();ensure_text();files_emitted=true;
  // Instance field initializers cannot refer to other instance fields. A lazy
  // holder also avoids opening standard streams when only memory APIs are used.
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcFiles? __edg_files_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcFiles __edg_files { get { lock(__edg_libc) { if(__edg_files_value==null)__edg_files_value=new global::Edg.Runtime.LibcFiles(__edg_libc.SetErrno,getEncoding:()=>__edg_text.MbCurMax()); return __edg_files_value; } } }\n";
}
void ensure_text() {
  if(text_emitted)return;
  ensure_libc();text_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcText? __edg_text_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcText __edg_text { get { lock(__edg_libc) { if(__edg_text_value==null)__edg_text_value=new global::Edg.Runtime.LibcText(__edg_libc.SetErrno); return __edg_text_value; } } }\n";
}
void ensure_math() {
  if(math_emitted)return;
  ensure_libc();math_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcMath? __edg_math_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcMath __edg_math { get { lock(__edg_libc) { if(__edg_math_value==null)__edg_math_value=new global::Edg.Runtime.LibcMath(__edg_libc.SetErrno); return __edg_math_value; } } }\n";
}
void ensure_clock() {
  if(clock_emitted)return;
  ensure_libc();clock_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcClock? __edg_clock_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcClock __edg_clock { get { lock(__edg_libc) { if(__edg_clock_value==null)__edg_clock_value=new global::Edg.Runtime.LibcClock(__edg_libc.SetErrno); return __edg_clock_value; } } }\n";
}
void ensure_environment() {
  if(environment_emitted)return;
  ensure_libc();environment_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcEnvironment? __edg_environment_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcEnvironment __edg_environment { get { lock(__edg_libc) { if(__edg_environment_value==null)__edg_environment_value=new global::Edg.Runtime.LibcEnvironment(); return __edg_environment_value; } } }\n";
}
void ensure_timezone() {
  if(timezone_emitted)return;
  ensure_libc();timezone_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcTimezone? __edg_timezone_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.LibcTimezone __edg_timezone { get { lock(__edg_libc) { if(__edg_timezone_value==null)__edg_timezone_value=new global::Edg.Runtime.LibcTimezone(__edg_libc.SetErrno); return __edg_timezone_value; } } }\n";
}
bool libc_timespec_pointer(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  if((t->kind!=tk_struct && t->kind!=tk_class) || t->incomplete || t->size!=16 || alignment_of_type(t)!=8 ||
     !t->source_corresp.name || string(t->source_corresp.name)!="timespec" ||
     !t->source_corresp.parent_scope || t->source_corresp.parent_scope->kind!=sck_file)return false;
  const char* field_names[]={"tv_sec","tv_nsec"};unsigned index=0;
  for(auto f=t->variant.class_struct_union.field_list;f;f=f->next,++index) {
    if(index>=2 || f->is_bit_field || !f->source_corresp.name || string(f->source_corresp.name)!=field_names[index] ||
       f->offset!=index*8 || type(f->type)!="long")return false;
  }
  return index==2;
}
bool libc_file_pointer(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer)return false;
  t=skip_typerefs(t->variant.pointer.type);
  return (t->kind==tk_struct || t->kind==tk_class) && t->incomplete &&
    t->source_corresp.name && string(t->source_corresp.name)=="__edg_FILE" &&
    t->source_corresp.parent_scope && t->source_corresp.parent_scope->kind==sck_file;
}
bool libc_locale_pointer(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  return (t->kind==tk_struct || t->kind==tk_class) && t->incomplete &&
    t->source_corresp.name && string(t->source_corresp.name)=="__edg_locale" &&
    t->source_corresp.parent_scope && t->source_corresp.parent_scope->kind==sck_file;
}
bool libc_lconv_pointer(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  if((t->kind!=tk_struct && t->kind!=tk_class) || t->incomplete || t->size!=96 || alignment_of_type(t)!=8 ||
     !t->source_corresp.name || string(t->source_corresp.name)!="lconv" ||
     !t->source_corresp.parent_scope || t->source_corresp.parent_scope->kind!=sck_file)return false;
  const char* field_names[]={"decimal_point","thousands_sep","grouping","int_curr_symbol","currency_symbol","mon_decimal_point","mon_thousands_sep","mon_grouping","positive_sign","negative_sign",
    "int_frac_digits","frac_digits","p_cs_precedes","p_sep_by_space","n_cs_precedes","n_sep_by_space","p_sign_posn","n_sign_posn","int_p_cs_precedes","int_p_sep_by_space","int_n_cs_precedes","int_n_sep_by_space","int_p_sign_posn","int_n_sign_posn"};
  unsigned index=0;
  for(auto f=t->variant.class_struct_union.field_list;f;f=f->next,++index) {
    if(index>=24 || f->is_bit_field || !f->source_corresp.name || string(f->source_corresp.name)!=field_names[index] ||
       f->offset!=(index<10?index*8:80+index-10) || type(f->type)!=(index<10?"sbyte*":"sbyte"))return false;
  }
  return index==24;
}
bool libc_fpos_pointer(a_type_ptr t) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  if((t->kind!=tk_struct && t->kind!=tk_class) || t->incomplete || t->size!=16 || alignment_of_type(t)!=8 ||
     !t->source_corresp.name || string(t->source_corresp.name)!="__edg_fpos" ||
     !t->source_corresp.parent_scope || t->source_corresp.parent_scope->kind!=sck_file)return false;
  const char* fpos_field_names[]={"offset","state","value"};unsigned index=0;
  for(auto f=t->variant.class_struct_union.field_list;f;f=f->next,++index) {
    if(index>=3 || f->is_bit_field || !f->source_corresp.name || string(f->source_corresp.name)!=fpos_field_names[index] ||
       f->offset!=(index==0?0:index==1?8:12) || type(f->type)!=(index==0?"long":"uint"))return false;
  }
  return index==3;
}
bool libc_boundary_pointer(a_type_ptr t) {return libc_fpos_pointer(t)||libc_file_pointer(t)||libc_locale_pointer(t)||libc_lconv_pointer(t)||libc_timespec_pointer(t);}
string libc_abi_type(a_type_ptr t) {
  if(libc_fpos_pointer(t))return "FPOS*";
  if(libc_timespec_pointer(t))return "TIMESPEC*";
  if(libc_file_pointer(t))return "FILE*";
  if(libc_locale_pointer(t))return "LOCALE*";
  if(libc_lconv_pointer(t))return "LCONV*";
  return type(t);
}
bool libc_va_pointer(a_type_ptr t) {
  t=skip_typerefs(t);
  if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);
  return va_tag_type(t) && t->size==24 && alignment_of_type(t)==8;
}
bool libc_format_callback(a_type_ptr t,bool wide=false) {
  t=skip_typerefs(t);if(t->kind!=tk_pointer || t->size!=8)return false;
  t=skip_typerefs(t->variant.pointer.type);if(t->kind!=tk_routine)return false;
  auto extra=t->variant.routine.extra_info;
  if(extra->has_ellipsis || !extra->prototyped || type(t->variant.routine.return_type)!="int")return false;
  auto p=extra->param_type_list;
  if(!p || type(p->type)!="void*")return false;
  p=p->next;
  if(!p || type(p->type)!=(wide?"int*":"sbyte*"))return false;
  p=p->next;
  return p && libc_va_pointer(p->type) && !p->next;
}
bool libc_format_lock_routine(a_routine_ptr r,bool reading=false,bool wide=false) {
  string symbol=wide?"__edg_with_file_wide_read_lock":reading?"__edg_with_file_read_lock":"__edg_with_file_lock";
  auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
  std::vector<a_type_ptr> parameters;
  for(auto p=extra->param_type_list;p;p=p->next)parameters.push_back(p->type);
  if(extra->has_ellipsis || !extra->prototyped || type(rt->variant.routine.return_type)!="int" ||
     parameters.size()!=5 || !libc_file_pointer(parameters[0]) ||
     !libc_format_callback(parameters[1],wide) || type(parameters[2])!="void*" ||
     type(parameters[3])!=(wide?"int*":"sbyte*") || !libc_va_pointer(parameters[4]))
    unsupported(("libc import ABI for "+symbol+": expected int(FILE*, int(*)(void*,const "+string(wide?"wchar_t":"char")+"*,va_list),void*,const "+string(wide?"wchar_t":"char")+"*,va_list) with builtin va_list").c_str());
  ensure_files();libc_register_routine(r,symbol);
  string signature;
  for(unsigned i=0;i<parameters.size();++i) {
    if(i)signature+=", ";
    signature+=type(parameters[i])+" a"+std::to_string(i);
  }
  // This is an ordinary routine wrapper: address-taking reuses the canonical
  // routine_address adapter, including its owner lifecycle lease and identity.
  helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+
    string(instance_mode?"":"static ")+"int "+name(r,"f")+"("+signature+") { return __edg_files."+string(wide?"WithFileWideReadLock":reading?"WithFileReadLock":"WithFileLock")+"((void*)a0, a1, "+
    string(instance_mode?"this, ":"")+"a2, a3, a4); }\n// dot-gen:member-end\n";
  return true;
}
bool libc_routine(a_routine_ptr r) {
  if(!libc_enabled || r->function_def_number!=NULL_function_def_number)return false;
  if(libc_routines.count(r))return true;
  string symbol=r->source_corresp.name?r->source_corresp.name:"";
  if(symbol=="__edg_with_file_lock")return libc_format_lock_routine(r);
  if(symbol=="__edg_with_file_read_lock")return libc_format_lock_routine(r,true);
  if(symbol=="__edg_with_file_wide_read_lock")return libc_format_lock_routine(r,true,true);
  string builtin_symbol=libc_builtin_symbol(r),lookup_symbol=builtin_symbol.empty()?symbol:builtin_symbol;
  for(const auto &binding:libc_bindings)if(lookup_symbol==binding.symbol) {
    auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
    string parameters,signature,args;unsigned index=0;
    for(auto p=extra->param_type_list;p;p=p->next) {
      string ty=type(p->type),arg="a"+std::to_string(index++);
      if(p!=extra->param_type_list){parameters+=",";signature+=", ";args+=", ";}
      parameters+=libc_abi_type(p->type);signature+=ty+" "+arg;args+=(libc_boundary_pointer(p->type)?"(void*)":"")+arg;
    }
    string result=type(rt->variant.routine.return_type);
    if(extra->has_ellipsis || parameters!=binding.parameters || libc_abi_type(rt->variant.routine.return_type)!=binding.result)
      unsupported(("libc import ABI for "+symbol+": expected "+binding.result+"("+binding.parameters+") but got "+result+"("+parameters+")").c_str());
    ensure_libc();if(string(binding.target).find("__edg_files.")==0)ensure_files();
    if(string(binding.target).find("__edg_text.")==0)ensure_text();
    if(string(binding.target).find("__edg_math.")==0)ensure_math();
    if(string(binding.target).find("__edg_clock.")==0)ensure_clock();
    if(string(binding.target).find("__edg_environment.")==0)ensure_environment();
    if(string(binding.target).find("__edg_timezone.")==0)ensure_timezone();
    libc_register_routine(r,binding.symbol);
    helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+name(r,"f")+"("+signature+") { "+
      (result=="void"?"":"return ")+(libc_boundary_pointer(rt->variant.routine.return_type)?"("+result+")":"")+binding.target+"("+args+"); }\n// dot-gen:member-end\n";
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
