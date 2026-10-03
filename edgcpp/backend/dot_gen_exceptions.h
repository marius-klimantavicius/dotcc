// Scalar, class, ordinary pointer and member-pointer exceptions.
// EDG owns construction/catch copying; the carrier owns final destruction.
#include "dot_gen_exception_identity.h"
bool exception_helpers_emitted=false;
string pending_exception;
std::map<a_handler_ptr,string> catch_frames;
std::map<a_try_supplement_ptr,string> try_frames;
string exception_locals;
std::map<a_type_ptr,unsigned> exception_classes;
std::map<a_routine_ptr,string> exception_destructors;
struct exception_type {
  unsigned id;
  string leaf;
  a_type_ptr record=nullptr;
  std::vector<unsigned> qualifiers; // Outermost pointee first; no top-level cv.
  unsigned member_kind=0; // 2=data member, 3=member function, at the leaf.
  bool nothrow=false;
};
std::map<string,exception_type> exception_types;
std::set<string> exception_matchers;
string exception_leaf(a_type_ptr t) {
  // Lowering represents nullptr_t as a void* typeref. Retain its C++ identity.
  if(is_or_was_nullptr_type(t))return "N";
  t=skip_typerefs(t);
  if(t->kind==tk_void)return "V";
  if(t->kind==tk_float)return "F"+std::to_string((unsigned)t->variant.float_kind);
  if(t->kind==tk_class || t->kind==tk_struct || t->kind==tk_union ||
     (t->kind==tk_integer && t->variant.integer.enum_type)) {
    (void)type(t);
    auto found=exception_classes.find(t);
    if(found==exception_classes.end())found=exception_classes.emplace(t,65536+exception_classes.size()).first;
    return "T"+std::to_string(found->second);
  }
  if(t->kind!=tk_integer)
    unsupported("exception leaf type");
  (void)type(t);
  auto &i=t->variant.integer;
  // C# representation alone conflates e.g. long/long long and char/signed char.
  return "I"+std::to_string((unsigned)i.int_kind | (i.bool_type<<8) |
      (i.wchar_t_type<<9) | (i.char8_t_type<<10) | (i.char16_t_type<<11) | (i.char32_t_type<<12));
}
const exception_type &exception_descriptor(a_type_ptr t) {
  (void)type(t); // Validate the payload representation before assigning its identity.
  exception_type result{};
  string signature;
  while(!is_or_was_nullptr_type(t) && skip_typerefs(t)->kind==tk_pointer) {
    auto p=skip_typerefs(t);
    if(p->variant.pointer.is_reference)unsupported("reference exception payload");
    t=p->variant.pointer.type;
    unsigned q=get_type_qualifiers(t);
    result.qualifiers.push_back(q);
    signature+="P"+std::to_string(q)+":";
  }
  result.leaf=extended_exception_leaf(t,result.nothrow,result.member_kind);
  if(result.leaf.empty())result.leaf=exception_leaf(t);
  auto leaf=skip_typerefs(t);
  if(!result.member_kind && (leaf->kind==tk_class || leaf->kind==tk_struct))result.record=leaf;
  signature+=result.leaf+(result.nothrow?"!":"");
  auto found=exception_types.find(signature);
  if(found==exception_types.end()) {
    if(exception_types.size()>=0x7fffffffU)unsupported("exception type ID overflow");
    result.id=1+(unsigned)exception_types.size();
    found=exception_types.emplace(signature,result).first;
  }
  return found->second;
}
string exception_key(a_type_ptr t) { return std::to_string(exception_descriptor(t).id); }
int exception_adjustment(const exception_type &src,const exception_type &dst) {
  if(src.id==dst.id)return 0;
  if(src.qualifiers.empty() && src.leaf=="N" &&
      (!dst.qualifiers.empty() || dst.member_kind))return 0;
  if(src.leaf==dst.leaf && src.qualifiers.size()==dst.qualifiers.size()) {
    bool previous_const=true,okay=true;
    // throw.c treats noexcept as a separate flag for single-level function
    // pointers and top-level member pointers, after comparing source RTTI.
    if(src.qualifiers.size()<=1 && dst.nothrow && !src.nothrow)okay=false;
    for(size_t i=0;i<src.qualifiers.size();++i) {
      unsigned s=src.qualifiers[i],d=dst.qualifiers[i];
      if((s & ~d) || ((d & ~s) && !previous_const))okay=false;
      previous_const &= (d & TQ_CONST)!=0;
    }
    if(okay)return 0;
  }
  // EDG throw.c checks the single-level flags here. Multi-level qualifiers
  // reside in ptr_flags, and do not participate in its void* fallback.
  unsigned source_flags=src.qualifiers.size()==1?src.qualifiers[0]:0;
  if(!src.qualifiers.empty() && dst.leaf=="V" && dst.qualifiers.size()==1 &&
     !(source_flags & ~dst.qualifiers[0]))return 0;
  // throw.c permits public, unambiguous derived-to-base conversion for class
  // objects and single-level pointers, never for Derived** -> Base**.
  if(src.record && dst.record && src.qualifiers.size()==dst.qualifiers.size() &&
     src.qualifiers.size()<=1 &&
     (src.qualifiers.empty() || !(src.qualifiers[0] & ~dst.qualifiers[0]))) {
    for(auto b=src.record->variant.class_struct_union.extra_info->base_classes;b;b=b->next)
      if(skip_typerefs(b->type)==dst.record && !b->ambiguous && b->has_public_derivation) {
        // The payload is a complete object of src.record, so EDG's resolved
        // offset also describes its shared virtual-base subobject.
        if(b->offset>src.record->size)unsupported("exception base offset");
        return (int)b->offset;
      }
  }
  return -1;
}
string exception_adjust(string value,a_type_ptr t) {
  const auto &dst=exception_descriptor(t);
  string id="__edg_adjust"+std::to_string(dst.id);
  if(exception_matchers.insert(id).second) {
    string test;
    // The complete TU has been prescanned, including throws in other functions.
    for(const auto &entry:exception_types) {
      int adjustment=exception_adjustment(entry.second,dst);
      if(adjustment>=0)test+="case "+std::to_string(entry.second.id)+": return "+std::to_string(adjustment)+";\n";
    }
    helper_output+="// dot-gen:member-begin\nprivate static int "+id+"(int type) { switch(type) {\n"+test+"default: return -1; } }\n// dot-gen:member-end\n";
  }
  return id+"("+value+")";
}
string exception_match(string value,a_type_ptr t) { return "("+exception_adjust(value,t)+" >= 0)"; }
string exception_catch_frame(string error,a_type_ptr t) {
  unsigned kind=0;
  string adjustment="0",null_conversion="false";
  if(t) {
    const auto &d=exception_descriptor(t);
    kind=d.qualifiers.empty()?d.member_kind:1;
    adjustment=exception_adjust("((__edg_exception)"+error+").Type",t);
    auto null_type=exception_types.find("N");
    if(kind && null_type!=exception_types.end())
      null_conversion="((__edg_exception)"+error+").Type == "+std::to_string(null_type->second.id);
  }
  return "__edg_create_catch_frame("+error+", "+std::to_string(kind)+", "+adjustment+", "+null_conversion+")";
}
// Native typeinfo storage is replaced by explicit IDs. Exclude its initializer
// dependency graph, including type-name strings and the runtime RTTI vtable.
void collect_exception_metadata_variable(a_variable_ptr v);
void collect_exception_metadata_constant(a_constant_ptr c) {
  if(!c)return;
  if(c->kind==ck_aggregate)
    for(auto p=c->variant.aggregate.first_constant;p;p=p->next)collect_exception_metadata_constant(p);
  else if(c->kind==ck_address && c->variant.address.kind==abk_variable)
    collect_exception_metadata_variable(c->variant.address.variant.variable);
}
void collect_exception_metadata_variable(a_variable_ptr v) {
  if(!v || !exception_metadata.insert(v).second)return;
  if(v->init_kind==initk_static)collect_exception_metadata_constant(v->initializer.constant);
}
void collect_exception_typeinfo(a_type_ptr t) {
  collect_exception_metadata_variable(skip_typerefs(t)->typeinfo_var);
  // Like lower_eh.c::eff_type_for_typeinfo, pointer throws use the leaf's
  // RTTI. This matters when no typed handler independently supplies its root.
  while(!is_or_was_nullptr_type(t) && skip_typerefs(t)->kind==tk_pointer)
    t=skip_typerefs(t)->variant.pointer.type;
  collect_exception_metadata_variable(get_underlying_type(t)->typeinfo_var);
  bool nothrow=false;
  collect_exception_metadata_variable(exception_identity_type(t,nothrow)->typeinfo_var);
}
void collect_exception_expression(an_expr_node_ptr e) {
  if(!e)return;
  if(e->kind==enk_lowered_eh_construct && e->variant.lowered_eh.kind==leck_internal_try)
    for(auto a=e->variant.lowered_eh.variant.try_and_catch_expr;a;a=a->next)collect_exception_expression(a);
  if(e->kind==enk_throw && e->variant.throw_info) {
    auto t=e->variant.throw_info;
    (void)exception_key(t->type);
    collect_exception_typeinfo(t->type);
    collect_exception_expression(t->expr);
  } else if(e->kind==enk_operation) {
    for(auto p=e->variant.operation.operands;p;p=p->next)collect_exception_expression(p);
  } else if(e->kind==enk_object_lifetime)collect_exception_expression(e->variant.object_lifetime.expr);
}
a_type_ptr catch_type(a_handler_ptr h) {
  auto t=h->parameter->type;
  auto p=skip_typerefs(t);
  if(p->kind==tk_pointer && p->variant.pointer.is_reference)t=p->variant.pointer.type;
  (void)exception_key(t);
  return t;
}
void collect_exception_metadata(a_statement_ptr s) {
  if(!s)return;
  a_statement_ptr selected=nullptr;
  if(selected_runtime_branch(s,selected)){collect_exception_metadata(selected);return;}
  collect_exception_expression(s->expr);
  switch(s->kind) {
    case stmk_try_block: {
      auto t=s->variant.try_block;
      // EDG lower_try_block places constructor/destructor subobject wrappers
      // inside this protected body and supplies implicit handler rethrows.
      // Emit these through the ordinary region-table try/catch path.
      collect_exception_metadata(t->statement);
      for(auto h=t->handlers;h;h=h->next) {
        if(h->parameter) { (void)catch_type(h); collect_exception_metadata_variable(h->typeinfo_var); }
        collect_exception_metadata(h->statement);
      }
      break;
    }
    case stmk_block:for(auto p=s->variant.block.statements;p;p=p->next)collect_exception_metadata(p);break;
    case stmk_switch:collect_exception_metadata(s->variant.switch_stmt.body_statement);break;
    case stmk_if:collect_exception_metadata(s->variant.if_stmt.then_statement);collect_exception_metadata(s->variant.if_stmt.else_statement);break;
    case stmk_while:case stmk_end_test_while:collect_exception_metadata(s->variant.loop_statement);break;
    case stmk_for:collect_exception_metadata(s->variant.for_loop.extra_info->initialization);collect_exception_metadata(s->variant.for_loop.statement);collect_exception_expression(s->variant.for_loop.extra_info->increment);break;
    case stmk_init:if(s->variant.dynamic_init->kind==dik_expression)collect_exception_expression(s->variant.dynamic_init->variant.expression);break;
    default:break;
  }
}
void ensure_exception_helpers() {
  if(exception_helpers_emitted)return;
  exception_helpers_emitted=true;
  ensure_lifecycle();
  helper_output+=R"CS(
// dot-gen:member-begin
private sealed class __edg_exception_storage {
  internal int Type;
  internal __edg_exception_storage? Previous,Next,AllPrevious,AllNext;
  internal readonly nuint Size;
  private int internalHandles;
  private nint storage;
  private nint handle;
  private int pendingPrimary;
  private nint primaryDestructor;
  private delegate* managed<object?,byte*,nint,void> invokePrimaryDestructor;
  private readonly nuint mask;
  private readonly delegate* managed<object?, byte*, void> destructor;
  private readonly delegate* managed<object?, void> releaseOwner;
  private object? owner;
  internal bool Complete;
  private int uses = 1; // Active throws and persistent exception handles.
  internal byte* Address => (byte*)(((nuint)storage + (nuint)sizeof(nint) + mask) & ~mask);
  internal __edg_exception_storage(int type, int alignment, void* storage,nuint size,
      delegate* managed<object?, byte*, void> dtor, object? owner,
      delegate* managed<object?, void> releaseOwner) {
    Type=type;Size=size;mask=(nuint)(alignment-1);destructor=dtor;this.owner=owner;
    this.releaseOwner=releaseOwner;
    // Allocate the handle before taking ownership of raw bytes: constructor
    // OOM leaves the factory responsible, and this partial finalizer sees zero.
    handle=global::System.Runtime.InteropServices.GCHandle.ToIntPtr(
        global::System.Runtime.InteropServices.GCHandle.Alloc(this));
    this.storage=(nint)storage;*((nint*)Address-1)=handle;
    try {__edg_register_exception_storage(owner,this);}
    catch {this.storage=0;global::System.Runtime.InteropServices.GCHandle.FromIntPtr(handle).Free();handle=0;throw;}
  }
  internal static __edg_exception_storage FromAddress(void* address) {
    return (__edg_exception_storage)global::System.Runtime.InteropServices.GCHandle.FromIntPtr(*((nint*)address-1)).Target!;
  }
  internal void PreparePrimary() {pendingPrimary=1;}
  internal void InitializePrimary(int type,nint dtor,delegate* managed<object?,byte*,nint,void> invoke) {
    Type=type;primaryDestructor=dtor;invokePrimaryDestructor=invoke;
  }
  internal void RetainPrimary(void* holder) {
    // The allocation's construction reference becomes its first handle only
    // after libc++ successfully placement-constructs the object.
    if(global::System.Threading.Interlocked.Exchange(ref pendingPrimary,0)!=0)Complete=true;
    else Retain();
    bool tracked=false;
    try {
      if(__edg_owned_exception_holder(owner,holder)) {
        if(global::System.Threading.Interlocked.Increment(ref internalHandles)<=0)
          global::System.Environment.FailFast("Invalid internal exception handle retain");
      } else {
        __edg_track_foreign_exception_holder(holder,this);tracked=true;
        __edg_change_exception_owner_references(owner,1);
      }
    } catch(global::System.OutOfMemoryException error) {
      if(tracked)__edg_untrack_foreign_exception_holder(holder,this);
      // Bookkeeping failure must not leave a retained reference or escape a
      // foreign CLR exception from libc++'s noexcept handle operation.
      Abandon(1);
      global::System.Environment.FailFast("Unable to retain C++ exception handle",error);throw;
    }
  }
  internal void ReleasePrimary(void* holder) {
    bool local=__edg_owned_exception_holder(owner,holder);object? held=owner;
    if(local && global::System.Threading.Interlocked.Decrement(ref internalHandles)<0)
      global::System.Environment.FailFast("Invalid internal exception handle count");
    if(!local)__edg_untrack_foreign_exception_holder(holder,this);
    try {Release();}finally{if(!local)__edg_change_exception_owner_references(held,-1);}
  }
  internal void AddWrapper(bool retain) {
    __edg_change_exception_owner_references(owner,1);if(retain)Retain();
  }
  internal void ReleaseWrapper() {
    object? held=owner;
    try {Release();}finally{__edg_change_exception_owner_references(held,-1);}
  }
  internal void AbandonWrappers(int count) {
    object? held=owner;
    try {Abandon(count);}finally{__edg_change_exception_owner_references(held,-count);}
  }
  internal void AbandonForeignHandles(int count) {
    object? held=owner;
    try {Abandon(count);}finally{__edg_change_exception_owner_references(held,-count);}
  }
  internal void AbandonOwnedHandles() {
    int count=global::System.Threading.Interlocked.Exchange(ref internalHandles,0)+
        global::System.Threading.Interlocked.Exchange(ref pendingPrimary,0);
    if(count<=0 || global::System.Threading.Volatile.Read(ref uses)!=count)
      global::System.Environment.FailFast("Unclassified exception payload reference at shutdown");
    Abandon(count);
  }
  ~__edg_exception_storage() { FreeStorage(); }
  private void FreeStorage() {
    byte* address=Address;
    nint raw=global::System.Threading.Interlocked.Exchange(ref storage,0);
    if(raw==0)return;
    __edg_unregister_exception_storage(owner,this);
    try {
      try {__edg_abandon_exception_region((nuint)address,(ulong)Size);}
      finally {global::System.Runtime.InteropServices.NativeMemory.Free((void*)raw);}
    } finally {
      nint token=global::System.Threading.Interlocked.Exchange(ref handle,0);
      if(token!=0)global::System.Runtime.InteropServices.GCHandle.FromIntPtr(token).Free();
      object? held=owner;owner=null;releaseOwner(held);
    }
  }
  internal void Retain() {
    if(global::System.Threading.Interlocked.Increment(ref uses)<=1)
      global::System.Environment.FailFast("Invalid EDG exception retain");
  }
  internal void Release() {
    int count=global::System.Threading.Interlocked.Decrement(ref uses);
    if(count<0)global::System.Environment.FailFast("Invalid EDG exception use count");
    if(count!=0)return;
    try {
      if(Complete) {
        if(destructor!=null)destructor(owner,Address);
        else if(primaryDestructor!=0)invokePrimaryDestructor(owner,Address,primaryDestructor);
      }
    }
    finally { FreeStorage();global::System.GC.SuppressFinalize(this);global::System.GC.KeepAlive(this); }
  }
  internal void Abandon(int count) {
    int remaining=global::System.Threading.Interlocked.Add(ref uses,-count);
    if(remaining<0)global::System.Environment.FailFast("Invalid abandoned EDG exception use count");
    // A surviving exception_ptr still owns normal final destruction. Only
    // abandonment of the final reference skips user code on this thread.
    if(remaining==0){FreeStorage();global::System.GC.SuppressFinalize(this);}
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_exception : global::System.Exception, global::System.IDisposable {
  internal readonly __edg_exception_storage Storage;
  internal int Type => Storage.Type;
  internal byte* Address => Storage.Address;
  internal bool Started=false;
  internal int CountedThread=0;
  internal bool Complete {get=>Storage.Complete;set=>Storage.Complete=value;}
  private int disposeRequested,uses=1;
  internal __edg_exception(__edg_exception_storage storage,bool retain) {
    storage.AddWrapper(retain);Storage=storage;
  }
  // Each exception_ptr rethrow has an independent host disposal boundary.
  // Bare C++ rethrow retains this wrapper while the previous catch releases.
  ~__edg_exception() {Storage?.AbandonWrappers(global::System.Threading.Interlocked.Exchange(ref uses,0));}
  public void Dispose() {
    if(global::System.Threading.Interlocked.Exchange(ref disposeRequested,1)==0)Release();
  }
  internal void Retain() {
    Storage.AddWrapper(true);
    if(global::System.Threading.Interlocked.Increment(ref uses)<=1)
      global::System.Environment.FailFast("Invalid EDG exception wrapper retain");
  }
  internal void Release() {
    int remaining=global::System.Threading.Interlocked.Decrement(ref uses);
    if(remaining<0)global::System.Environment.FailFast("Invalid EDG exception wrapper use count");
    if(remaining==0) {
      global::System.Threading.Interlocked.Exchange(ref disposeRequested,1);
      global::System.GC.SuppressFinalize(this);
    }
    try{Storage.ReleaseWrapper();}finally{global::System.GC.KeepAlive(this);}
  }
}
// dot-gen:member-end
[global::System.ThreadStatic] private static int __edg_uncaught_count;
// dot-gen:member-begin
private static bool __edg_begin_exception(global::System.Exception error) {
  if(error is __edg_exception carrier) {
    int thread=global::System.Environment.CurrentManagedThreadId;
    int previous=global::System.Threading.Interlocked.CompareExchange(ref carrier.CountedThread,thread,0);
    if(previous==0)checked{++__edg_uncaught_count;}
    else if(previous!=thread)global::System.Environment.FailFast("Concurrent host reuse of a C++ throw carrier; rethrow an exception_ptr instead");
  }
  return true;
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_end_exception(global::System.Exception error) {
  if(error is not __edg_exception carrier)return;
  int thread=global::System.Threading.Volatile.Read(ref carrier.CountedThread);
  if(thread==0)return;
  if(thread!=global::System.Environment.CurrentManagedThreadId)
    global::System.Environment.FailFast("C++ uncaught marker ended on a different thread");
  global::System.Threading.Volatile.Write(ref carrier.CountedThread,0);
  if(--__edg_uncaught_count<0)global::System.Environment.FailFast("Invalid C++ uncaught exception count");
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_start_exception(__edg_exception carrier) {
  carrier.Started=true;__edg_begin_exception(carrier);
}
// dot-gen:member-end
[global::System.ThreadStatic] private static __edg_catch_frame? __edg_current_catch;
private static global::System.Exception? __edg_active_exception {
  get {var frame=__edg_current_catch;while(frame!=null && !frame.Active)frame=frame.parent;return frame?.Error;}
}
private static global::System.Exception? __edg_handled_exception {
  get {var frame=__edg_current_catch;while(frame!=null && (!frame.Active || !frame.Caught))frame=frame.parent;return frame?.Error;}
}
// dot-gen:member-begin
private sealed class __edg_catch_frame {
  internal readonly global::System.Exception Error;
  internal readonly __edg_catch_frame? parent;
  private readonly nuint* pointerStorage;
  private readonly int offset;
  internal byte* Address => pointerStorage != null
    ? (byte*)pointerStorage
    : ((__edg_exception)Error).Address + offset;
  internal bool Active = true;
  internal bool Caught = false;
  internal __edg_catch_frame(global::System.Exception error, int pointerKind, int adjustment, bool nullConversion) {
    // Like throw.c's pointer_buffer, mutations through T*& affect this catch
    // only. Rethrow starts from the original payload, not this adjusted copy.
    if (pointerKind==1 || nullConversion) {
      // nullptr -> member function needs the whole two-word descriptor. A
      // data-member null uses -1, unlike ordinary and function-member nulls.
      pointerStorage = (nuint*)__edg_exception_alloc((nuint)(pointerKind==3?16:8));
      if(nullConversion) {
        pointerStorage[0]=pointerKind==2?nuint.MaxValue:0;
        if(pointerKind==3)pointerStorage[1]=0;
      } else {
        pointerStorage[0] = *(nuint*)((__edg_exception)error).Address;
        if (pointerStorage[0] != 0) pointerStorage[0] += (nuint)adjustment;
      }
    }
    offset = adjustment;
    Error = error; parent = __edg_current_catch; __edg_current_catch = this;
  }
  internal void MarkCaught() {Caught=true;__edg_end_exception(Error);}
  internal void Leave() {
    if (!Active) return;
    if (!global::System.Object.ReferenceEquals(__edg_current_catch, this))
      global::System.Environment.FailFast("Invalid EDG catch stack");
    Active = false;
    // As in EDG's __free_thrown_object, destruction precedes popping the entry.
    try { if (Error is __edg_exception payload) payload.Release(); }
    finally { __edg_current_catch = parent; global::System.Runtime.InteropServices.NativeMemory.Free(pointerStorage); global::System.GC.KeepAlive(this); }
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_rethrow() {
  var error = __edg_active_exception;
  if (error == null) __edg_terminate_runtime(null,"C++ rethrow without active exception");
  if (error is __edg_exception payload) payload.Retain();
  __edg_begin_exception(error!);
  global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error!).Throw();
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_throw_filter(__edg_exception payload, global::System.Exception error) {
  if (payload.Started && !global::System.Object.ReferenceEquals(payload, error))
    __edg_terminate_runtime(error,"Exception escaped EDG throw copy");
  return !global::System.Object.ReferenceEquals(payload, error);
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_catch_filter(__edg_catch_frame frame, global::System.Exception error) {
  if (!frame.Caught)
    __edg_terminate_runtime(error,"Exception escaped catch parameter initialization");
  // Bookkeeping must not stop CLR first-pass search before a termination
  // boundary. Handler cleanup happens in finally during the second pass.
  return false;
}
// dot-gen:member-end
// dot-gen:member-begin
private static __edg_catch_frame __edg_create_catch_frame(global::System.Exception error,int pointerKind,int adjustment,bool nullConversion) {
  try {return new __edg_catch_frame(error,pointerKind,adjustment,nullConversion);}
  catch(global::System.OutOfMemoryException failure) {
    global::System.Environment.FailFast("Unable to allocate EDG exception storage",failure);throw;
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static void* __edg_exception_alloc(nuint size) {
  try {
    void* result=global::System.Runtime.InteropServices.NativeMemory.Alloc(size);
    if(result==null)global::System.Environment.FailFast("Unable to allocate EDG exception storage");
    return result;
  } catch(global::System.OutOfMemoryException error) {
    global::System.Environment.FailFast("Unable to allocate EDG exception storage",error);throw;
  }
}
// dot-gen:member-end
)CS";
  if(source_jumps_used)for(const string &signature:std::vector<string>{
      "private static bool __edg_begin_exception(global::System.Exception error) {\n",
      "private static bool __edg_throw_filter(__edg_exception payload, global::System.Exception error) {\n",
      "private static bool __edg_catch_filter(__edg_catch_frame frame, global::System.Exception error) {\n"}) {
    auto pos=helper_output.find(signature);
    if(pos==string::npos)unsupported("missing source-jump EH filter");
    helper_output.insert(pos+signature.size(),"  if(error is global::Edg.Runtime.NonlocalJump.Transfer)return false;\n");
  }
  if(c11_thread_exit_used)for(const auto &entry:std::vector<std::pair<string,string>>{
      {"private static bool __edg_begin_exception(global::System.Exception error) {\n","false"},
      {"private static bool __edg_throw_filter(__edg_exception payload, global::System.Exception error) {\n","true"},
      {"private static bool __edg_catch_filter(__edg_catch_frame frame, global::System.Exception error) {\n","false"}}) {
    auto pos=helper_output.find(entry.first);
    if(pos==string::npos)unsupported("missing C11 thread-exit EH filter");
    helper_output.insert(pos+entry.first.size(),"  if(error is global::Edg.Runtime.CThreadExitTransfer){"+string(entry.second=="true"?"__edg_end_exception(payload);":"")+"return "+entry.second+";}\n");
  }
  if(instance_mode)for(const string &signature:std::vector<string>{"void __edg_rethrow(","bool __edg_throw_filter(","bool __edg_catch_filter("}) {
    string from="private static "+signature,to="private "+signature;
    auto pos=helper_output.find(from);if(pos==string::npos)unsupported("missing owner-aware EH helper");
    helper_output.replace(pos,from.size(),to);
  }
  string receiver=instance_mode?"(("+string(getenv("EDG_DOT_CLASS"))+")owner!).":"";
  string mod=instance_mode?"":"static ";
  helper_output+="private "+mod+"__edg_exception_storage? __edg_exception_head=null;\n";
  helper_output+="// dot-gen:member-begin\nprivate "+mod+"void __edg_prepare_exception_ledger() {lock(__edg_sync){if(__edg_abandon_exception_handles==null)__edg_abandon_exception_handles=__edg_abandon_owned_exception_handles;if(__edg_heap_owner.RegionReleased==null)__edg_heap_owner.RegionReleased=__edg_abandon_exception_region;}}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate static void __edg_change_exception_owner_references(object? owner,int delta) {"+receiver+"__edg_change_exception_references(delta);}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate static void __edg_register_exception_storage(object? owner,__edg_exception_storage storage) {\nlock("+receiver+"__edg_sync) {storage.Next="+receiver+"__edg_exception_head;if(storage.Next!=null)storage.Next.Previous=storage;"+receiver+"__edg_exception_head=storage;}\nlock(__edg_exception_regions_sync){storage.AllNext=__edg_all_exception_payloads;if(storage.AllNext!=null)storage.AllNext.AllPrevious=storage;__edg_all_exception_payloads=storage;}\n}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate static void __edg_unregister_exception_storage(object? owner,__edg_exception_storage storage) {\nlock("+receiver+"__edg_sync) {if(storage.Previous!=null)storage.Previous.Next=storage.Next;else "+receiver+"__edg_exception_head=storage.Next;if(storage.Next!=null)storage.Next.Previous=storage.Previous;storage.Previous=null;storage.Next=null;}\nlock(__edg_exception_regions_sync){if(storage.AllPrevious!=null)storage.AllPrevious.AllNext=storage.AllNext;else __edg_all_exception_payloads=storage.AllNext;if(storage.AllNext!=null)storage.AllNext.AllPrevious=storage.AllPrevious;storage.AllPrevious=null;storage.AllNext=null;}\n}\n// dot-gen:member-end\n";
  helper_output+=R"CS(
// dot-gen:member-begin
private static readonly object __edg_exception_regions_sync=new();
private static __edg_exception_storage? __edg_all_exception_payloads=null;
private static readonly global::System.Collections.Generic.Dictionary<(nuint Holder,__edg_exception_storage Target),int> __edg_foreign_exception_handles=new();
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_track_foreign_exception_holder(void* address,__edg_exception_storage target) {
  if(address==null)return;
  bool region=__edg_heap_owner.ContainsAny(address);
  lock(__edg_exception_regions_sync) {
    if(!region)for(var storage=__edg_all_exception_payloads;storage!=null;storage=storage.AllNext)
      if((nuint)address>=(nuint)storage.Address && (nuint)address-(nuint)storage.Address<storage.Size){region=true;break;}
    if(!region)return; // Stack/global/TLS holders have ordinary C++ teardown.
    var key=((nuint)address,target);
    __edg_foreign_exception_handles.TryGetValue(key,out int count);
    __edg_foreign_exception_handles[key]=checked(count+1);
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_untrack_foreign_exception_holder(void* address,__edg_exception_storage target) {
  if(address==null)return;
  lock(__edg_exception_regions_sync) {
    var key=((nuint)address,target);
    if(__edg_foreign_exception_handles.TryGetValue(key,out int count)) {
      if(count==1)__edg_foreign_exception_handles.Remove(key);else __edg_foreign_exception_handles[key]=count-1;
    }
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_abandon_exception_region(nuint address,ulong size) {
  for(;;) {
    (nuint Holder,__edg_exception_storage Target) key=default;int count=0;
    lock(__edg_exception_regions_sync) {
      foreach(var entry in __edg_foreign_exception_handles)
        if(entry.Key.Holder>=address && (ulong)(entry.Key.Holder-address)<size){key=entry.Key;count=entry.Value;break;}
      if(count==0)return;
      __edg_foreign_exception_handles.Remove(key);
    }
    // Remove first, then release outside the registry lock. Recursive foreign
    // payload cleanup is finite and must not call translated destructors.
    key.Target!.AbandonForeignHandles(count);
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_in_exception_arena(byte[] bytes,void* address) {
  if(bytes.Length==0)return false;
  nuint start=(nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref bytes[0]);
  return (nuint)address>=start && (nuint)address-start<(nuint)bytes.Length;
}
// dot-gen:member-end
)CS";
  helper_output+="// dot-gen:member-begin\nprivate static bool __edg_owned_exception_holder(object? owner,void* address) {\nif(address==null)return false;lock("+receiver+"__edg_sync) {\n"+
      "if(__edg_in_exception_arena("+receiver+"__edg_storage,address))return true;\n"+
      "if("+receiver+"__edg_threads.Contains(address))return true;\n"+
      "for(var payload="+receiver+"__edg_exception_head;payload!=null;payload=payload.Next)if((nuint)address>=(nuint)payload.Address && (nuint)address-(nuint)payload.Address<payload.Size)return true;\n"+
      "if("+receiver+"__edg_heap.Contains(address))return true;\nreturn false;}\n}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate "+mod+"void __edg_abandon_owned_exception_handles() {for(;;){__edg_exception_storage? storage;lock(__edg_sync)storage=__edg_exception_head;if(storage==null)return;storage.AbandonOwnedHandles();}}\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate static void __edg_release_exception_owner(object? owner) { "+receiver+"__edg_release_exception_lease(); }\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate static __edg_exception __edg_create_exception(int type,int size,int alignment,delegate* managed<object?,byte*,void> dtor,object? owner) {\n"
      "void* raw=null;bool leased=false;__edg_exception_storage? storage=null;try { "+receiver+"__edg_prepare_exception_ledger();"+receiver+"__edg_acquire_exception_lease();leased=true;\n"
      "raw=__edg_exception_alloc((nuint)size+(nuint)alignment-1+(nuint)sizeof(nint));storage=new __edg_exception_storage(type,alignment,raw,(nuint)size,dtor,owner,&__edg_release_exception_owner);raw=null;leased=false;return new __edg_exception(storage,false);\n"
      "} catch(global::System.OutOfMemoryException error) {if(storage!=null)storage.Abandon(1);else {global::System.Runtime.InteropServices.NativeMemory.Free(raw);if(leased)__edg_release_exception_owner(owner);}global::System.Environment.FailFast(\"Unable to allocate EDG exception storage\",error);throw;}\n}\n// dot-gen:member-end\n";
  ensure_terminate_helpers();
}
string exception_destructor(a_routine_ptr r) {
  if(!r)return "null";
  // Full portable lower_throw selects cdk_complete. Partial lowering retains
  // the primary destructor, which can instead be a VTT-taking subobject entry.
  if(r->ctor_dtor_kind!=cdk_complete) {
    auto primary=r->primary_ctor_or_dtor?r->primary_ctor_or_dtor:r;
    a_routine_ptr complete=nullptr;
    for(auto p=il_header.primary_scope->routines;p;p=p->next)
      if(p->ctor_dtor_kind==cdk_complete && (p==primary || p->primary_ctor_or_dtor==primary)) { complete=p; break; }
    if(!complete)unsupported("missing complete-object exception destructor");
    r=complete;
  }
  auto found=exception_destructors.find(r);
  if(found!=exception_destructors.end())return "&"+found->second;
  if(r->function_def_number==NULL_function_def_number)unsupported("external exception destructor");
  auto rt=skip_typerefs(r->type);
  auto p=rt->variant.routine.extra_info->param_type_list;
  if(!p || p->next || skip_typerefs(p->type)->kind!=tk_pointer ||
     skip_typerefs(rt->variant.routine.return_type)->kind!=tk_void)unsupported("exception destructor ABI");
  string id="__edg_exception_dtor"+std::to_string(exception_destructors.size());
  exception_destructors[r]=id;
  string receiver=instance_mode?"(("+string(getenv("EDG_DOT_CLASS"))+")owner!).":"";
  helper_output+="// dot-gen:member-begin\nprivate static void "+id+"(object? owner, byte* address) {\n"+
      receiver+"__edg_enter_exception_disposal(); try { "+receiver+name(r,"f")+
      "(("+type(p->type)+")address); } finally { "+receiver+"__edg_leave(); } }\n// dot-gen:member-end\n";
  return "&"+id;
}
string exception_address(an_expr_node_ptr e) {
  ensure_exception_helpers();
  string address;
  if(e->variant.lowered_eh.kind==leck_caught_object_address)
    address="__edg_current_catch!.Address";
  else if(e->variant.lowered_eh.kind==leck_thrown_object_address && !pending_exception.empty())
    address=pending_exception+".Address";
  else unsupported("exception address",e->variant.lowered_eh.kind);
  return "(("+type(e->type)+")"+address+")";
}
string expression_effect(an_expr_node_ptr e);
string throw_effect(an_expr_node_ptr e) {
  ensure_exception_helpers();
  auto t=e->variant.throw_info;
  if(!t)return "__edg_rethrow();\n";
  string key=exception_key(t->type);
  auto payload_type=skip_typerefs(t->type);
  auto alignment=alignment_of_type(payload_type);
  if(!alignment || alignment>4096 || (alignment & (alignment-1)))unsupported("exception payload alignment");
  string dtor=exception_destructor(t->destructor);
  string saved=pending_exception;
  string id="__edg_thrown"+std::to_string(++serial);
  pending_exception=id;
  string init=expression_effect(t->expr);
  pending_exception=saved;
  // Keep the carrier rooted throughout initializer helpers and a failing
  // initializer. Successful throws transfer that root to CLR exception handling.
  return "{ var "+id+" = __edg_create_exception("+key+", "+std::to_string(payload_type->size)+", "+
      std::to_string(alignment)+", "+dtor+", "+(instance_mode?"this":"null")+");\ntry {\n"+init+
      id+".Complete = true; __edg_start_exception("+id+"); throw "+id+";\n} catch (global::System.Exception initialization_error) when (__edg_throw_filter("+id+", initialization_error)) {\n"
      "if (!global::System.Object.ReferenceEquals(initialization_error, "+id+")) {\n"+
      id+".Release(); } throw;\n} finally { global::System.GC.KeepAlive("+id+"); } }\n";
}
