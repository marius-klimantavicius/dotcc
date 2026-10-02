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
  internal __edg_thread_state? Previous,Next;
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_thread_registry {
  private readonly object sync=new();
  private readonly global::System.Threading.ThreadLocal<__edg_thread_state> values;
  private __edg_thread_state? head;
  internal __edg_thread_registry() {values=new(Make,true);}
  private void Link(__edg_thread_state state) {
    state.Next=head;state.Previous=null;if(head!=null)head.Previous=state;head=state;
  }
  private __edg_thread_state Make() {
    var state=new __edg_thread_state();lock(sync)Link(state);return state;
  }
  internal __edg_thread_state Value {
    get=>values.Value!;
    set {
      var old=values.Value!;if(global::System.Object.ReferenceEquals(old,value))return;
      // Publish through ThreadLocal before changing the intrusive list: a
      // failed runtime slot update leaves the original state registered.
      values.Value=value;
      lock(sync) {
        if(old.Previous!=null)old.Previous.Next=old.Next;else head=old.Next;
        if(old.Next!=null)old.Next.Previous=old.Previous;old.Previous=null;old.Next=null;
        Link(value);
      }
    }
  }
  internal global::System.Collections.Generic.IList<__edg_thread_state> Values=>values.Values;
  internal bool Contains(void* address) {
    lock(sync)for(var state=head;state!=null;state=state.Next) {
      // Another thread can clear its storage while an unrelated holder is
      // being classified. Keep one strong snapshot of this pinned arena.
      byte[]? bytes=state.Storage;if(bytes==null || bytes.Length==0)continue;
      nuint start=(nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref bytes[0]);
      if((nuint)address>=start && (nuint)address-start<(nuint)bytes.Length)return true;
    }
    return false;
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_guard {internal int Thread;}
// dot-gen:member-end
private @MOD@readonly object __edg_sync=new object();
private @MOD@readonly global::System.Collections.Generic.List<__edg_finalizer> __edg_finalizers=new();
private @MOD@readonly global::System.Collections.Generic.Dictionary<nint,__edg_guard> __edg_guards=new();
private @MOD@readonly __edg_thread_registry __edg_threads=new();
private @MOD@int __edg_active,__edg_state,__edg_shutdown_thread,__edg_exception_leases,__edg_exception_references,__edg_thread_leases;
private @MOD@global::System.Action? __edg_abandon_exception_handles=null;
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
private @MOD@void __edg_change_exception_references(int delta) {
  lock(__edg_sync) {
    if(delta>0 && __edg_state==2)global::System.Environment.FailFast("Exception reference outlived owner");
    checked{__edg_exception_references+=delta;}
    if(__edg_exception_references<0)global::System.Environment.FailFast("Invalid external exception reference count");
    global::System.Threading.Monitor.PulseAll(__edg_sync);
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_acquire_thread_lease() {
  lock(__edg_sync) {
    if(__edg_state!=0 && !((__edg_state==1 || __edg_state==3) &&
        ((__edg_state==1 && __edg_shutdown_thread==global::System.Environment.CurrentManagedThreadId) || __edg_threads.Value!.Depth!=0)))
      throw new global::System.ObjectDisposedException("translated program");
    checked{++__edg_thread_leases;}
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_release_thread_lease() {
  lock(__edg_sync){if(--__edg_thread_leases<0)global::System.Environment.FailFast("Invalid EDG thread owner lease");global::System.Threading.Monitor.PulseAll(__edg_sync);}
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_enter_retained_thread() {
  lock(__edg_sync) {
    if(__edg_state==2 || __edg_thread_leases==0)
      global::System.Environment.FailFast("Thread outlived EDG owner storage");
    ++__edg_active;++__edg_threads.Value!.Depth;
  }
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
    catch(global::System.Exception error){/*__EDG_SHUTDOWN_TERMINATE__*/}
  }
}
// dot-gen:member-end
// dot-gen:member-begin
public @MOD@void ReleaseThread() {
  __edg_release_thread_core(false);
}
// dot-gen:member-end
// dot-gen:member-begin
private @MOD@void __edg_release_thread_core(bool retained) {
  lock(__edg_sync) {
    if(__edg_threads.Value!.Depth!=0)throw new global::System.InvalidOperationException("Cannot release an active translated thread");
    // Shutdown waits for retained workers; they must finish TLS teardown
    // without waiting for Shutdown to change state, or both sides deadlock.
    while(__edg_state==1 && !retained)global::System.Threading.Monitor.Wait(__edg_sync);
    if(retained && (__edg_state==2 || __edg_thread_leases==0))
      global::System.Environment.FailFast("Invalid EDG retained thread teardown");
    if(__edg_state==2)return;
    ++__edg_active;++__edg_threads.Value!.Depth;
  }
  try {var thread=__edg_threads.Value!;__edg_drain(thread.Finalizers);/*__EDG_LIBC_THREAD_KEYS_RELEASE__*//*__EDG_LIBC_THREAD_RELEASE__*/thread.Storage=null;thread.Depth=0;__edg_threads.Value=new __edg_thread_state();}
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
    while(__edg_active!=0 || __edg_thread_leases!=0)global::System.Threading.Monitor.Wait(__edg_sync);
    if(__edg_exception_references!=0) {
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
      while(__edg_active!=0 || __edg_thread_leases!=0)global::System.Threading.Monitor.Wait(__edg_sync);
      if(__edg_exception_references!=0)
        throw new global::System.InvalidOperationException("Dispose exceptions retained during shutdown and retry Shutdown");
      foreach(var thread in __edg_threads.Values)if(thread!=__edg_threads.Value && thread.Finalizers.Count!=0)
        throw new global::System.InvalidOperationException("ReleaseThread must run before retrying Shutdown");
    }
    __edg_abandon_exception_handles?.Invoke();
    lock(__edg_sync)if(__edg_exception_leases!=0)global::System.Environment.FailFast("Unreleased owner exception payloads");
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
  internal static global::System.Action<nuint,ulong>? RegionReleased=null;
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
  internal bool Contains(void* address) {
    nuint value=(nuint)address;
    lock(Sync){foreach(nuint p in pointers)if(value>=p && (ulong)(value-p)<All[p].Size)return true;}
    return false;
  }
  internal static bool ContainsAny(void* address) {
    nuint value=(nuint)address;
    lock(Sync){foreach(var entry in All)if(value>=entry.Key && (ulong)(value-entry.Key)<entry.Value.Size)return true;}
    return false;
  }
  internal static ulong Size(void* p) {lock(Sync)return All[(nuint)p].Size;}
  internal static void Free(void* p) {
    if(p==null)return;nuint raw;ulong size;
    lock(Sync){var entry=All[(nuint)p];All.Remove((nuint)p);entry.Owner.pointers.Remove((nuint)p);raw=entry.Raw;size=entry.Size;}
    try{RegionReleased?.Invoke((nuint)p,size);}
    finally{global::System.Runtime.InteropServices.NativeMemory.Free((void*)raw);}
  }
  internal void Dispose() {
    (nuint Address,nuint Raw,ulong Size)[] pending;
    lock(Sync) {
      pending=new (nuint,nuint,ulong)[pointers.Count];int index=0;
      foreach(nuint p in pointers){var entry=All[p];pending[index++]=(p,entry.Raw,entry.Size);}
      foreach(var entry in pending)All.Remove(entry.Address);pointers.Clear();
    }
    // Region callbacks may release another owner's payload. Never hold the
    // heap registry lock while acquiring any owner's lifecycle lock.
    foreach(var entry in pending) {
      try{RegionReleased?.Invoke(entry.Address,entry.Size);}
      finally{global::System.Runtime.InteropServices.NativeMemory.Free((void*)entry.Raw);}
    }
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
