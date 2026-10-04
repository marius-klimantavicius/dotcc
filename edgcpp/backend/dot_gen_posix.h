// Optional portable POSIX import boundary. Record layouts are checked before
// casting to shared runtime records; native host records never enter the IL.
bool posix_enabled=false,posix_emitted=false;
void ensure_posix() {
  if(posix_emitted)return;
  ensure_libc();posix_emitted=true;
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"global::Edg.Runtime.PosixContext? __edg_posix_value;\n";
  helper_output+="private "+mod+"global::Edg.Runtime.PosixContext __edg_posix { get { lock(__edg_libc) { if(__edg_posix_value==null)__edg_posix_value=new global::Edg.Runtime.PosixContext(__edg_libc.SetErrno); return __edg_posix_value; } } }\n";
}
string posix_type(a_type_ptr t) {
  auto p=skip_typerefs(t);
  if(p->kind!=tk_pointer || p->size!=8)return type(t);
  p=skip_typerefs(p->variant.pointer.type);
  if(p->kind!=tk_struct || p->incomplete || !p->source_corresp.name)return type(t);
  string n=p->source_corresp.name;
  if(n!="stat" && n!="timeval")return type(t);
  const char* stat_names[]={"st_dev","st_ino","st_mode","st_nlink","st_uid","st_gid","st_size","st_atime","st_mtime","st_ctime","st_rdev","st_blksize","st_blocks"};
  const char* stat_types[]={"ulong","ulong","uint","ulong","uint","uint","long","long","long","long","ulong","long","long"};
  unsigned stat_offsets[]={0,8,16,24,32,36,40,48,56,64,72,80,88};
  unsigned index=0,count=n=="stat"?13:2;
  if(p->size!=(n=="stat"?96:16) || alignment_of_type(p)!=8)return type(t);
  for(auto f=p->variant.class_struct_union.field_list;f;f=f->next,++index) {
    if(index>=count || f->is_bit_field || !f->source_corresp.name)return type(t);
    string expected=n=="stat"?stat_names[index]:(index==0?"tv_sec":"tv_usec");
    if(string(f->source_corresp.name)!=expected || type(f->type)!=(n=="stat"?stat_types[index]:"long") ||
       f->offset!=(n=="stat"?stat_offsets[index]:index*8))return type(t);
  }
  return index==count?(n=="stat"?"POSIX_STAT*":"POSIX_TIMEVAL*"):type(t);
}
struct posix_binding { const char* symbol;const char* result;const char* parameters;const char* target;bool variadic; };
const posix_binding posix_bindings[]={
  {"open","int","sbyte*,int","OpenVariadic",true},
  {"fcntl","int","int,int","FcntlVariadic",true},
  {"close","int","int","Close",false},
  {"read","long","int,void*,ulong","Read",false},
  {"write","long","int,void*,ulong","Write",false},
  {"pread","long","int,void*,ulong,long","Pread",false},
  {"pwrite","long","int,void*,ulong,long","Pwrite",false},
  {"lseek","long","int,long,int","Lseek",false},
  {"ftruncate","int","int,long","Ftruncate",false},
  {"fsync","int","int","Fsync",false},
  {"fdatasync","int","int","Fdatasync",false},
  {"access","int","sbyte*,int","Access",false},
  {"getcwd","sbyte*","sbyte*,ulong","Getcwd",false},
  {"unlink","int","sbyte*","Unlink",false},
  {"mkdir","int","sbyte*,uint","Mkdir",false},
  {"rmdir","int","sbyte*","Rmdir",false},
  {"readlink","long","sbyte*,sbyte*,ulong","Readlink",false},
  {"fchmod","int","int,uint","Fchmod",false},
  {"fchown","int","int,uint,uint","Fchown",false},
  {"geteuid","uint","","Geteuid",false},
  {"stat","int","sbyte*,POSIX_STAT*","Stat",false},
  {"fstat","int","int,POSIX_STAT*","Fstat",false},
  {"lstat","int","sbyte*,POSIX_STAT*","Lstat",false},
  {"mmap","void*","void*,ulong,int,int,int,long","Mmap",false},
  {"munmap","int","void*,ulong","Munmap",false},
  {"gettimeofday","int","POSIX_TIMEVAL*,void*","Gettimeofday",false},
  {"utimes","int","sbyte*,POSIX_TIMEVAL*","Utimes",false},
  {"sysconf","long","int","Sysconf",false},
  {"getpid","int","","Getpid",false},
  {"sleep","uint","uint","Sleep",false},
  {"usleep","int","uint","Usleep",false},
};
bool posix_routine(a_routine_ptr r,const string& symbol) {
  if(!posix_enabled)return false;
  // A class/namespace method named open/getpid is not the global POSIX API.
  if(r->source_corresp.name_linkage!=nlk_external ||
     !r->source_corresp.parent_scope || r->source_corresp.parent_scope->kind!=sck_file)return false;
  for(const auto& binding:posix_bindings)if(symbol==binding.symbol) {
    auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
    string parameters,signature,args;unsigned index=0;
    for(auto p=extra->param_type_list;p;p=p->next) {
      string ty=type(p->type),abi=posix_type(p->type),arg="a"+std::to_string(index++);
      if(p!=extra->param_type_list){parameters+=",";signature+=", ";args+=", ";}
      parameters+=abi;signature+=ty+" "+arg;
      if(abi=="POSIX_STAT*")args+="(global::Edg.Runtime.PosixStat*)";
      if(abi=="POSIX_TIMEVAL*")args+="(global::Edg.Runtime.PosixTimeval*)";
      if((symbol=="mmap" || symbol=="munmap") && index==2)args+="(nuint)";
      args+=arg;
    }
    string result=type(rt->variant.routine.return_type);
    if(!extra->prototyped || extra->has_ellipsis!=binding.variadic || parameters!=binding.parameters || result!=binding.result)
      unsupported(("POSIX import ABI for "+symbol+": expected "+binding.result+"("+binding.parameters+") but got "+result+"("+parameters+")").c_str());
    if(binding.variadic) {
      signature+=", params global::System.ReadOnlySpan<global::Edg.Runtime.VaArg> __edg_varargs";
      args+=", __edg_varargs";
    }
    ensure_posix();libc_register_routine(r,binding.symbol);
    helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+name(r,"f")+"("+signature+") { return __edg_posix."+binding.target+"("+args+"); }\n// dot-gen:member-end\n";
    return true;
  }
  return false;
}
