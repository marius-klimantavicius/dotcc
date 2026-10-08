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
bool posix_timespec_record(a_type_ptr t) {
  t=skip_typerefs(t);
  if(t->kind!=tk_struct || t->incomplete || t->size!=16 || alignment_of_type(t)!=8 ||
     !t->source_corresp.name || string(t->source_corresp.name)!="timespec")return false;
  unsigned index=0;
  for(auto f=t->variant.class_struct_union.field_list;f;f=f->next,++index)
    if(index>=2 || f->is_bit_field || !f->source_corresp.name ||
       string(f->source_corresp.name)!=(index==0?"tv_sec":"tv_nsec") ||
       f->offset!=index*8 || type(f->type)!="long")return false;
  return index==2;
}
string posix_type(a_type_ptr t) {
  if(libc_file_pointer(t))return "FILE*";
  auto p=skip_typerefs(t);
  if(p->kind!=tk_pointer || p->size!=8)return type(t);
  p=skip_typerefs(p->variant.pointer.type);
  // A C header may be included inside a C++ namespace (EDG does this for
  // dirent.h). Its extern-C function still has the portable ABI. Validate the
  // record itself rather than requiring its tag to live in the global scope.
  if(p->kind!=tk_struct || !p->source_corresp.name)return type(t);
  string n=source_spelling(&p->source_corresp);
  if(n=="__edg_DIR" && p->incomplete)return "POSIX_DIR*";
  if(p->incomplete)return type(t);
  if(n=="sockaddr") {
    auto family=p->variant.class_struct_union.field_list;
    if(p->size!=16 || alignment_of_type(p)!=2 || !family || family->is_bit_field ||
       !family->source_corresp.name || string(family->source_corresp.name)!="sa_family" ||
       family->offset!=0 || type(family->type)!="ushort")return type(t);
    auto data=family->next;
    if(!data || data->next || data->is_bit_field || !data->source_corresp.name ||
       string(data->source_corresp.name)!="sa_data" || data->offset!=2)return type(t);
    auto array=skip_typerefs(data->type);
    if(array->kind!=tk_array || array->size!=14 ||
       type(array->variant.array.element_type)!="sbyte")return type(t);
    return "POSIX_SOCKADDR*";
  }
  if(n=="rlimit") {
    auto f=p->variant.class_struct_union.field_list;
    if(p->size!=16 || alignment_of_type(p)!=8)return type(t);
    for(unsigned i=0;i<2;++i) {
      if(!f || f->is_bit_field || !f->source_corresp.name ||
         string(f->source_corresp.name)!=(i==0?"rlim_cur":"rlim_max") ||
         f->offset!=i*8 || type(f->type)!="ulong")return type(t);
      f=f->next;
    }
    return f?type(t):"POSIX_RLIMIT*";
  }
  if(n=="dirent") {
    auto f=p->variant.class_struct_union.field_list;
    if(p->size!=4096 || alignment_of_type(p)!=1 || !f || f->next || f->is_bit_field ||
       f->offset!=0 || !f->source_corresp.name || string(f->source_corresp.name)!="d_name")return type(t);
    auto a=skip_typerefs(f->type);
    return a->kind==tk_array && a->size==4096 && type(a->variant.array.element_type)=="sbyte"?"POSIX_DIRENT*":type(t);
  }
  if(n=="statvfs") {
    const char* fields[]={"f_bsize","f_frsize","f_blocks","f_bfree","f_bavail","f_files","f_ffree","f_favail","f_fsid","f_flag","f_namemax"};
    if(p->size!=88 || alignment_of_type(p)!=8)return type(t);
    auto f=p->variant.class_struct_union.field_list;
    for(unsigned i=0;i<11;++i) {
      if(!f || f->is_bit_field || !f->source_corresp.name || string(f->source_corresp.name)!=fields[i] || f->offset!=i*8 || type(f->type)!="ulong")return type(t);
      f=f->next;
    }
    return f?type(t):"POSIX_STATVFS*";
  }
  if(n!="stat" && n!="timeval")return type(t);
  const char* stat_names[]={"st_dev","st_ino","st_mode","st_nlink","st_uid","st_gid","st_size","st_atim","st_mtim","st_ctim","st_rdev","st_blksize","st_blocks"};
  const char* stat_types[]={"ulong","ulong","uint","ulong","uint","uint","long","timespec","timespec","timespec","ulong","long","long"};
  unsigned stat_offsets[]={0,8,16,24,32,36,40,48,64,80,96,104,112};
  unsigned index=0,count=n=="stat"?13:2;
  if(p->size!=(n=="stat"?120:16) || alignment_of_type(p)!=8)return type(t);
  for(auto f=p->variant.class_struct_union.field_list;f;f=f->next,++index) {
    if(index>=count || f->is_bit_field || !f->source_corresp.name)return type(t);
    string expected=n=="stat"?stat_names[index]:(index==0?"tv_sec":"tv_usec");
    bool valid_type=n=="stat" && index>=7 && index<=9?posix_timespec_record(f->type):
        type(f->type)==(n=="stat"?stat_types[index]:"long");
    if(string(f->source_corresp.name)!=expected || !valid_type ||
       f->offset!=(n=="stat"?stat_offsets[index]:index*8))return type(t);
  }
  return index==count?(n=="stat"?"POSIX_STAT*":"POSIX_TIMEVAL*"):type(t);
}
struct posix_binding { const char* symbol;const char* result;const char* parameters;const char* target;bool variadic; };
const posix_binding posix_bindings[]={
  {"socket","int","int,int,int","Socket",false},
  {"bind","int","int,POSIX_SOCKADDR*,uint","Bind",false},
  {"listen","int","int,int","Listen",false},
  {"accept","int","int,POSIX_SOCKADDR*,uint*","Accept",false},
  {"connect","int","int,POSIX_SOCKADDR*,uint","Connect",false},
  {"getsockname","int","int,POSIX_SOCKADDR*,uint*","Getsockname",false},
  {"shutdown","int","int,int","Shutdown",false},
  {"open","int","sbyte*,int","OpenVariadic",true},
  {"openat","int","int,sbyte*,int","OpenAtVariadic",true},
  {"fcntl","int","int,int","FcntlVariadic",true},
  {"ioctl","int","int,ulong","IoctlVariadic",true},
  {"close","int","int","Close",false},
  {"read","long","int,void*,ulong","Read",false},
  {"write","long","int,void*,ulong","Write",false},
  {"pread","long","int,void*,ulong,long","Pread",false},
  {"pwrite","long","int,void*,ulong,long","Pwrite",false},
  {"lseek","long","int,long,int","Lseek",false},
  {"ftruncate","int","int,long","Ftruncate",false},
  {"truncate","int","sbyte*,long","Truncate",false},
  {"pathconf","long","sbyte*,int","Pathconf",false},
  {"fsync","int","int","Fsync",false},
  {"fdatasync","int","int","Fdatasync",false},
  {"access","int","sbyte*,int","Access",false},
  {"getcwd","sbyte*","sbyte*,ulong","Getcwd",false},
  {"realpath","sbyte*","sbyte*,sbyte*","Realpath",false},
  {"unlink","int","sbyte*","Unlink",false},
  {"unlinkat","int","int,sbyte*,int","UnlinkAt",false},
  {"link","int","sbyte*,sbyte*","Link",false},
  {"symlink","int","sbyte*,sbyte*","Symlink",false},
  {"mkdir","int","sbyte*,uint","Mkdir",false},
  {"rmdir","int","sbyte*","Rmdir",false},
  {"readlink","long","sbyte*,sbyte*,ulong","Readlink",false},
  {"fchmod","int","int,uint","Fchmod",false},
  {"fchown","int","int,uint,uint","Fchown",false},
  {"geteuid","uint","","Geteuid",false},
  {"stat","int","sbyte*,POSIX_STAT*","Stat",false},
  {"fstat","int","int,POSIX_STAT*","Fstat",false},
  {"lstat","int","sbyte*,POSIX_STAT*","Lstat",false},
  {"statvfs","int","sbyte*,POSIX_STATVFS*","Statvfs",false},
  {"mmap","void*","void*,ulong,int,int,int,long","Mmap",false},
  {"munmap","int","void*,ulong","Munmap",false},
  {"mlock","int","void*,ulong","Mlock",false},
  {"munlock","int","void*,ulong","Munlock",false},
  {"madvise","int","void*,ulong,int","Madvise",false},
  {"gettimeofday","int","POSIX_TIMEVAL*,void*","Gettimeofday",false},
  {"utimes","int","sbyte*,POSIX_TIMEVAL*","Utimes",false},
  {"sysconf","long","int","Sysconf",false},
  {"getpid","int","","Getpid",false},
  {"sched_getcpu","int","","SchedulerGetCpu",false},
  {"sched_yield","int","","SchedulerYield",false},
  {"sleep","uint","uint","Sleep",false},
  {"usleep","int","uint","Usleep",false},
  {"chdir","int","sbyte*","Chdir",false},
  {"mkstemp","int","sbyte*","Mkstemp",false},
  {"isatty","int","int","Isatty",false},
  {"fdopen","FILE*","int,sbyte*","Fdopen",false},
  {"fileno","int","FILE*","Fileno",false},
  {"opendir","POSIX_DIR*","sbyte*","Opendir",false},
  {"fdopendir","POSIX_DIR*","int","Fdopendir",false},
  {"readdir","POSIX_DIRENT*","POSIX_DIR*","Readdir",false},
  {"closedir","int","POSIX_DIR*","Closedir",false},
  {"getrlimit","int","int,POSIX_RLIMIT*","Getrlimit",false},
  {"setrlimit","int","int,POSIX_RLIMIT*","Setrlimit",false},
};
#include "dot_gen_semaphore.h"
bool posix_routine(a_routine_ptr r,const string& symbol) {
  if(!posix_enabled)return false;
  if(posix_semaphore_routine(r,symbol))return true;
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
      if(abi=="POSIX_SOCKADDR*")args+="(void*)";
      if(abi=="POSIX_STAT*")args+="(global::Edg.Runtime.PosixStat*)";
      if(abi=="POSIX_TIMEVAL*")args+="(global::Edg.Runtime.PosixTimeval*)";
      if(abi=="POSIX_RLIMIT*")args+="(global::Edg.Runtime.PosixRlimit*)";
      if(abi=="POSIX_STATVFS*")args+="(global::Edg.Runtime.PosixStatvfs*)";
      if((symbol=="mmap" || symbol=="munmap") && index==2)args+="(nuint)";
      args+=arg;
    }
    string result=type(rt->variant.routine.return_type),result_abi=posix_type(rt->variant.routine.return_type);
    if(!extra->prototyped || extra->has_ellipsis!=binding.variadic || parameters!=binding.parameters || result_abi!=binding.result)
      unsupported(("POSIX import ABI for "+symbol+": expected "+binding.result+"("+binding.parameters+") but got "+result+"("+parameters+")").c_str());
    if(binding.variadic) {
      signature+=", params global::System.ReadOnlySpan<global::Edg.Runtime.VaArg> __edg_varargs";
      args+=", __edg_varargs";
    }
    ensure_posix();libc_register_routine(r,binding.symbol);
    // realpath(NULL destination) must allocate from the translated heap so
    // ordinary translated free() retains its ownership and alignment rules.
    if(symbol=="realpath")args+=", size => (nint)__edg_malloc(size)";
    bool files=symbol=="fdopen" || symbol=="fileno";
    if(files)ensure_files();
    if(symbol=="ioctl")ensure_files();
    bool opaque_result=result_abi=="FILE*" || result_abi=="POSIX_DIR*" || result_abi=="POSIX_DIRENT*";
    helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+name(r,"f")+"("+signature+") { "+string(symbol=="ioctl"?"_ = __edg_files; ":"")+"return "+string(opaque_result?"("+result+")":"")+(files?"__edg_files.":"__edg_posix.")+binding.target+"("+args+"); }\n// dot-gen:member-end\n";
    return true;
  }
  return false;
}
