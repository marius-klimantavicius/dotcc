// EDG emits the initializer bodies, guard calls and destructor registrations.
// This layer supplies their managed owner/thread lifetime and synchronization.
bool lifecycle_emitted=false;
std::vector<a_variable_ptr> local_static_storage;
std::vector<a_routine_ptr> startup_routines;
string lifecycle_mod() {return instance_mode?"":"static ";}
void ensure_lifecycle() {
  if(lifecycle_emitted)return;
  lifecycle_emitted=true;
  string mod=lifecycle_mod();
  string callback="delegate* managed<"+string(instance_mode?string(getenv("EDG_DOT_CLASS"))+", ":"")+"void*, void>";
  string invoke="(("+callback+")entry.Function)("+string(instance_mode?"this, ":"")+"(void*)entry.Argument);";
  string code=R"CS(
// dot-gen:member-begin
private readonly struct __edg_finalizer {
  internal readonly nint Function,Argument;
  internal __edg_finalizer(nint fn,nint arg) {Function=fn;Argument=arg;}
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_thread_state {
  internal readonly int Thread=global::System.Environment.CurrentManagedThreadId;
  internal readonly global::System.Collections.Generic.List<__edg_finalizer> Finalizers=new();
  internal byte[]? Storage;
  internal int Depth;
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_guard {internal int Thread;}
// dot-gen:member-end
private @MOD@readonly object __edg_sync=new object();
private @MOD@readonly global::System.Collections.Generic.List<__edg_finalizer> __edg_finalizers=new();
private @MOD@readonly global::System.Collections.Generic.Dictionary<nint,__edg_guard> __edg_guards=new();
private @MOD@readonly global::System.Threading.ThreadLocal<__edg_thread_state> __edg_threads=new(()=>new __edg_thread_state(),true);
private @MOD@int __edg_active,__edg_state,__edg_shutdown_thread,__edg_exception_leases;
private @MOD@nint __edg_new_handler;
// dot-gen:member-begin
private @MOD@void __edg_enter() {
  lock(__edg_sync) {
    if(__edg_state!=0 && !((__edg_state==1 || __edg_state==3) && ((__edg_state==1 && __edg_shutdown_thread==global::System.Environment.CurrentManagedThreadId) || __edg_threads.Value!.Depth!=0)))
      throw new global::System.ObjectDisposedException("translated program");
    ++__edg_active;++__edg_threads.Value!.Depth;
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_enter_exception_disposal() {
  lock(__edg_sync) {
    if(__edg_state==2)global::System.Environment.FailFast("Exception outlived EDG owner storage");
    ++__edg_active;++__edg_threads.Value!.Depth;
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_leave() {lock(__edg_sync){--__edg_threads.Value!.Depth;--__edg_active;global::System.Threading.Monitor.PulseAll(__edg_sync);}}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_acquire_exception_lease() {
  lock(__edg_sync){if(__edg_state==2)throw new global::System.ObjectDisposedException("translated program");checked{++__edg_exception_leases;}}
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_release_exception_lease() {
  lock(__edg_sync){if(--__edg_exception_leases<0)global::System.Environment.FailFast("Invalid EDG exception owner lease");global::System.Threading.Monitor.PulseAll(__edg_sync);}
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@int __edg_register(nint fn,nint arg,bool tls) {
  lock(__edg_sync) {
    if(__edg_state==2)throw new global::System.ObjectDisposedException("translated program");
    (tls?__edg_threads.Value!.Finalizers:__edg_finalizers).Add(new __edg_finalizer(fn,arg));return 0;
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_drain(global::System.Collections.Generic.List<__edg_finalizer> list) {
  for(;;) {
    __edg_finalizer entry;
    lock(__edg_sync){if(list.Count==0)return;entry=list[list.Count-1];list.RemoveAt(list.Count-1);}
    try {@INVOKE@}
    catch(global::System.Exception error){global::System.Environment.FailFast("Destructor threw during shutdown",error);}
  }
}
// dot-gen:member-end
// dot-gen:member-begin
public @MOD@void ReleaseThread() {
  lock(__edg_sync) {
    if(__edg_threads.Value!.Depth!=0)throw new global::System.InvalidOperationException("Cannot release an active translated thread");
    while(__edg_state==1)global::System.Threading.Monitor.Wait(__edg_sync);
    if(__edg_state==2)return;
    ++__edg_active;++__edg_threads.Value!.Depth;
  }
  try {var thread=__edg_threads.Value!;__edg_drain(thread.Finalizers);/*__EDG_LIBC_THREAD_RELEASE__*/thread.Storage=null;__edg_threads.Value=new __edg_thread_state();}
  finally {lock(__edg_sync){--__edg_active;global::System.Threading.Monitor.PulseAll(__edg_sync);}}
}
// dot-gen:member-end
// dot-gen:member-begin
public @MOD@void Shutdown() {
  lock(__edg_sync) {
    if(__edg_state==2)return;
    if(__edg_threads.Value!.Depth!=0)throw new global::System.InvalidOperationException("Cannot shut down from an active translated call");
    if(__edg_state==1) {
      if(__edg_shutdown_thread==global::System.Environment.CurrentManagedThreadId)return;
      while(__edg_state==1)global::System.Threading.Monitor.Wait(__edg_sync);if(__edg_state==2)return;
    }
    bool deferred=__edg_state==3;
    __edg_state=1;__edg_shutdown_thread=global::System.Environment.CurrentManagedThreadId;
    while(__edg_active!=0)global::System.Threading.Monitor.Wait(__edg_sync);
    if(__edg_exception_leases!=0) {
      __edg_state=deferred?3:0;global::System.Threading.Monitor.PulseAll(__edg_sync);
      throw new global::System.InvalidOperationException("Dispose escaped C++ exception carriers before Shutdown");
    }
    foreach(var thread in __edg_threads.Values)if(thread!=__edg_threads.Value && thread.Finalizers.Count!=0) {
      __edg_state=deferred?3:0;global::System.Threading.Monitor.PulseAll(__edg_sync);
      throw new global::System.InvalidOperationException("ReleaseThread must run on each owning thread before Shutdown");
    }
  }
  bool closed=false;
  try {
    __edg_drain(__edg_threads.Value!.Finalizers);__edg_drain(__edg_finalizers);
    lock(__edg_sync) {
      // Destructors can call hosts that retain a fresh carrier. Keep storage
      // alive but ordinary entry closed until its owner explicitly releases it.
      while(__edg_active!=0)global::System.Threading.Monitor.Wait(__edg_sync);
      if(__edg_exception_leases!=0)
        throw new global::System.InvalidOperationException("Dispose exceptions retained during shutdown and retry Shutdown");
      foreach(var thread in __edg_threads.Values)if(thread!=__edg_threads.Value && thread.Finalizers.Count!=0)
        throw new global::System.InvalidOperationException("ReleaseThread must run before retrying Shutdown");
    }
    /*__EDG_LIBC_DISPOSE__*/__edg_heap.Dispose();__edg_storage=[];foreach(var thread in __edg_threads.Values)thread.Storage=null;__edg_guards.Clear();closed=true;
  }
  finally {lock(__edg_sync){__edg_state=closed?2:3;if(closed)__edg_new_handler=0;global::System.Threading.Monitor.PulseAll(__edg_sync);}}
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@int __edg_guard_acquire(byte* address) {
  __edg_guard guard;lock(__edg_sync){if(!__edg_guards.TryGetValue((nint)address,out guard!))__edg_guards.Add((nint)address,guard=new __edg_guard());}
  global::System.Threading.Monitor.Enter(guard);
  if(global::System.Threading.Volatile.Read(ref *address)!=0){global::System.Threading.Monitor.Exit(guard);return 0;}
  if(guard.Thread!=0)global::System.Environment.FailFast("Recursive local-static initialization");
  guard.Thread=global::System.Environment.CurrentManagedThreadId;return 1;
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_guard_finish(byte* address,bool success) {
  __edg_guard guard;lock(__edg_sync)guard=__edg_guards[(nint)address];
  global::System.Threading.Volatile.Write(ref *address,(byte)(success?1:0));guard.Thread=0;
  global::System.Threading.Monitor.Exit(guard);
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_heap_owner {
  private static readonly object Sync=new object();
  private static readonly global::System.Collections.Generic.Dictionary<nuint,(__edg_heap_owner Owner,nuint Raw,ulong Size)> All=new();
  private readonly global::System.Collections.Generic.HashSet<nuint> pointers=new();
  internal void* Allocate(ulong size,ulong alignment) {
    if(alignment<16)alignment=16;
    if((alignment&(alignment-1))!=0 || alignment>(ulong)nint.MaxValue || size>(ulong)nint.MaxValue-alignment)return null;
    void* raw=null;
    try {
      raw=global::System.Runtime.InteropServices.NativeMemory.Alloc((nuint)(size+alignment));
      if(raw==null)return null;
      nuint result=((nuint)raw+(nuint)alignment-1)&~((nuint)alignment-1);
      lock(Sync){pointers.Add(result);try{All.Add(result,(this,(nuint)raw,size));}catch{pointers.Remove(result);throw;}}
      return (void*)result;
    } catch(global::System.OutOfMemoryException){global::System.Runtime.InteropServices.NativeMemory.Free(raw);return null;}
  }
  internal static ulong Size(void* p) {lock(Sync)return All[(nuint)p].Size;}
  internal static void Free(void* p) {
    if(p==null)return;nuint raw;
    lock(Sync){var entry=All[(nuint)p];All.Remove((nuint)p);entry.Owner.pointers.Remove((nuint)p);raw=entry.Raw;}
    global::System.Runtime.InteropServices.NativeMemory.Free((void*)raw);
  }
  internal void Dispose() {
    lock(Sync){foreach(nuint p in pointers){global::System.Runtime.InteropServices.NativeMemory.Free((void*)All[p].Raw);All.Remove(p);}pointers.Clear();}
  }
}
// dot-gen:member-end
private @MOD@readonly __edg_heap_owner __edg_heap=new();
)CS";
  for(auto replacement:std::vector<std::pair<string,string>>{{"@MOD@",mod},{"@INVOKE@",invoke}})
    for(size_t pos=code.find(replacement.first);pos!=string::npos;pos=code.find(replacement.first,pos+replacement.second.size()))
      code.replace(pos,replacement.first.size(),replacement.second);
  if(instance_mode)code+="// dot-gen:member-begin\npublic void Dispose() { Shutdown(); }\n// dot-gen:member-end\n";
  helper_output+=code;
}
string lifecycle_call(a_routine_ptr r,an_expr_node_ptr args) {
  if(r->function_def_number!=NULL_function_def_number)return "";
  string source=r->source_corresp.name?r->source_corresp.name:"";
  if(r->is_tls_init_alias) {
    for(auto p=il_header.primary_scope->routines;p;p=p->next)if(p->is_tls_init_routine)return name(p,"f")+"()";
    unsupported("external TLS initializer");
  }
  if(source=="__cxa_guard_acquire" || source=="__cxa_guard_release" || source=="__cxa_guard_abort") {
    if(!args || args->next)unsupported("guard runtime ABI");
    ensure_lifecycle();
    string address="(byte*)("+expr(args)+")";
    return source=="__cxa_guard_acquire"?"__edg_guard_acquire("+address+")":
        "__edg_guard_finish("+address+", "+string(source=="__cxa_guard_release"?"true":"false")+")";
  }
  if(source=="__cxa_atexit" || source=="__cxa_thread_atexit") {
    if(!args || !args->next || !args->next->next || args->next->next->next)unsupported("atexit runtime ABI");
    ensure_lifecycle();
    return "__edg_register((nint)("+expr(args)+"), (nint)("+expr(args->next)+"), "+
        string(source=="__cxa_thread_atexit"?"true":"false")+")";
  }
  return "";
}
