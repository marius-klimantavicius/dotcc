// Portable unnamed POSIX semaphores. Included after the ordinary POSIX binding
// declarations so this focused import extension does not duplicate that table.
string posix_semaphore_type(a_type_ptr source) {
  auto p=skip_typerefs(source);
  if(p->kind!=tk_pointer || p->size!=8)return type(source);
  p=skip_typerefs(p->variant.pointer.type);
  if(p->kind!=tk_struct || p->incomplete || !p->source_corresp.name)return type(source);
  string n=source_spelling(&p->source_corresp);
  if(n=="__edg_posix_semaphore") {
    auto f=p->variant.class_struct_union.field_list;
    if(p->size==8 && alignment_of_type(p)==8 && f && !f->next && !f->is_bit_field &&
       f->offset==0 && f->source_corresp.name && string(f->source_corresp.name)=="__token" &&
       type(f->type)=="ulong")return "POSIX_SEMAPHORE*";
  }
  if(n=="timespec" && p->size==16 && alignment_of_type(p)==8) {
    auto f=p->variant.class_struct_union.field_list;
    unsigned index=0;
    for(;f;f=f->next,++index) {
      if(index>=2 || f->is_bit_field || f->offset!=index*8 || type(f->type)!="long" ||
         !f->source_corresp.name || string(f->source_corresp.name)!=(index==0?"tv_sec":"tv_nsec"))return type(source);
    }
    if(index==2)return "POSIX_SEMAPHORE_TIMESPEC*";
  }
  return type(source);
}

bool posix_semaphore_routine(a_routine_ptr r,const string& symbol) {
  if(r->source_corresp.name_linkage!=nlk_external ||
     !r->source_corresp.parent_scope || r->source_corresp.parent_scope->kind!=sck_file)return false;
  const posix_binding bindings[]={
    {"sem_init","int","POSIX_SEMAPHORE*,int,uint","SemaphoreInit",false},
    {"sem_destroy","int","POSIX_SEMAPHORE*","SemaphoreDestroy",false},
    {"sem_wait","int","POSIX_SEMAPHORE*","SemaphoreWait",false},
    {"sem_trywait","int","POSIX_SEMAPHORE*","SemaphoreTryWait",false},
    {"sem_timedwait","int","POSIX_SEMAPHORE*,POSIX_SEMAPHORE_TIMESPEC*","SemaphoreTimedWait",false},
    {"sem_post","int","POSIX_SEMAPHORE*","SemaphorePost",false},
    {"sem_getvalue","int","POSIX_SEMAPHORE*,int*","SemaphoreGetValue",false},
  };
  for(const auto& binding:bindings)if(symbol==binding.symbol) {
    auto rt=skip_typerefs(r->type);auto extra=rt->variant.routine.extra_info;
    string parameters,signature,args;unsigned index=0;
    for(auto p=extra->param_type_list;p;p=p->next) {
      string ty=type(p->type),abi=posix_semaphore_type(p->type),arg="a"+std::to_string(index++);
      if(p!=extra->param_type_list){parameters+=",";signature+=", ";args+=", ";}
      parameters+=abi;signature+=ty+" "+arg;
      if(abi=="POSIX_SEMAPHORE*")args+="(ulong*)";
      if(abi=="POSIX_SEMAPHORE_TIMESPEC*")args+="(global::Edg.Runtime.PosixSemaphoreTimespec*)";
      args+=arg;
    }
    string result=type(rt->variant.routine.return_type);
    if(!extra->prototyped || extra->has_ellipsis || result!="int" || parameters!=binding.parameters)
      unsupported(("POSIX import ABI for "+symbol+": expected int("+binding.parameters+") but got "+result+"("+parameters+")").c_str());
    ensure_posix();libc_register_routine(r,binding.symbol);
    helper_output+="// dot-gen:import "+symbol+"\n// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+result+" "+name(r,"f")+"("+signature+") { return __edg_posix."+binding.target+"("+args+"); }\n// dot-gen:member-end\n";
    return true;
  }
  return false;
}
