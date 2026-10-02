// Integer, enum, object pointer and class exceptions.
// EDG owns construction/catch copying; the carrier owns final destruction.
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
};
std::map<string,exception_type> exception_types;
std::set<string> exception_matchers;
string exception_leaf(a_type_ptr t) {
  if(is_or_was_ptr_to_data_member_type(t) || is_or_was_ptr_to_member_function_type(t))
    unsupported("member pointer exception type");
  // Lowering represents nullptr_t as a void* typeref. Retain its C++ identity.
  if(is_or_was_nullptr_type(t))return "N";
  t=skip_typerefs(t);
  if(t->kind==tk_void)return "V";
  if(t->kind==tk_class || t->kind==tk_struct || t->kind==tk_union ||
     (t->kind==tk_integer && t->variant.integer.enum_type)) {
    (void)type(t);
    auto found=exception_classes.find(t);
    if(found==exception_classes.end())found=exception_classes.emplace(t,65536+exception_classes.size()).first;
    return "T"+std::to_string(found->second);
  }
  if(t->kind!=tk_integer)
    unsupported("exception leaf type (integer, enum, void, nullptr or class required)");
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
  result.leaf=exception_leaf(t);
  auto leaf=skip_typerefs(t);
  if(leaf->kind==tk_class || leaf->kind==tk_struct)result.record=leaf;
  signature+=result.leaf;
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
  if(src.qualifiers.empty() && src.leaf=="N" && !dst.qualifiers.empty())return 0;
  if(src.leaf==dst.leaf && src.qualifiers.size()==dst.qualifiers.size()) {
    bool previous_const=true,okay=true;
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
  collect_exception_expression(s->expr);
  switch(s->kind) {
    case stmk_try_block: {
      auto t=s->variant.try_block;
      if(t->is_function_try_block)unsupported("function try block");
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
  helper_output+=R"CS(
// dot-gen:member-begin
private sealed class __edg_exception : global::System.Exception {
  internal readonly int Type;
  private readonly byte[] storage;
  private readonly nuint mask;
  private readonly delegate* managed<object?, byte*, void> destructor;
  private readonly object? owner;
  internal bool Started = false, Complete = false;
  private bool destroyed;
  private int uses = 1; // The original throw; each rethrow adds one use.
  internal byte* Address => (byte*)(((nuint)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref storage[0]) + mask) & ~mask);
  internal __edg_exception(int type, int size, int alignment,
      delegate* managed<object?, byte*, void> dtor, object? owner) {
    Type = type; mask = (nuint)(alignment - 1); destructor = dtor; this.owner = owner;
    storage = global::System.GC.AllocateArray<byte>(size + alignment - 1, pinned: true);
  }
  internal void Retain() { checked { ++uses; } }
  internal void Release() {
    if (--uses < 0) global::System.Environment.FailFast("Invalid EDG exception use count");
    if (uses != 0 || destroyed) return;
    destroyed = true; // A throwing destructor must never be invoked twice.
    try { if (Complete && destructor != null) destructor(owner, Address); }
    finally { global::System.GC.KeepAlive(this); }
  }
}
// dot-gen:member-end
[global::System.ThreadStatic] private static __edg_catch_frame? __edg_current_catch;
private static global::System.Exception? __edg_active_exception => __edg_current_catch?.Error;
// dot-gen:member-begin
private sealed class __edg_catch_frame {
  internal readonly global::System.Exception Error;
  private readonly __edg_catch_frame? parent;
  private readonly nuint[]? pointerStorage;
  private readonly int offset;
  internal byte* Address => pointerStorage != null
    ? (byte*)global::System.Runtime.CompilerServices.Unsafe.AsPointer(ref pointerStorage[0])
    : ((__edg_exception)Error).Address + offset;
  internal bool Active = true;
  internal bool Caught = false;
  internal __edg_catch_frame(global::System.Exception error, bool pointerCatch, int adjustment) {
    // Like throw.c's pointer_buffer, mutations through T*& affect this catch
    // only. Rethrow starts from the original payload, not this adjusted copy.
    if (pointerCatch) {
      pointerStorage = global::System.GC.AllocateArray<nuint>(1, pinned: true);
      pointerStorage[0] = *(nuint*)((__edg_exception)error).Address;
      if (pointerStorage[0] != 0) pointerStorage[0] += (nuint)adjustment;
    }
    offset = adjustment;
    Error = error; parent = __edg_current_catch; __edg_current_catch = this;
  }
  internal void Leave() {
    if (!Active) return;
    if (!global::System.Object.ReferenceEquals(__edg_current_catch, this))
      global::System.Environment.FailFast("Invalid EDG catch stack");
    Active = false;
    // As in EDG's __free_thrown_object, destruction precedes popping the entry.
    try { if (Error is __edg_exception payload) payload.Release(); }
    finally { __edg_current_catch = parent; global::System.GC.KeepAlive(this); }
  }
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_rethrow() {
  var error = __edg_active_exception;
  if (error == null) global::System.Environment.FailFast("C++ rethrow without active exception");
  if (error is __edg_exception payload) payload.Retain();
  global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error!).Throw();
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_throw_filter(__edg_exception payload, global::System.Exception error) {
  if (payload.Started && !global::System.Object.ReferenceEquals(payload, error))
    global::System.Environment.FailFast("Exception escaped EDG throw copy", error);
  return true;
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_catch_filter(__edg_catch_frame frame, global::System.Exception error) {
  if (!frame.Caught)
    global::System.Environment.FailFast("Exception escaped catch parameter initialization", error);
  return true;
}
// dot-gen:member-end
)CS";
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
  helper_output+="// dot-gen:member-begin\nprivate static void "+id+"(object? owner, byte* address) {\n"+
      (instance_mode?"(("+string(getenv("EDG_DOT_CLASS"))+")owner!).":"")+name(r,"f")+
      "(("+type(p->type)+")address); }\n// dot-gen:member-end\n";
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
  return "{ var "+id+" = new __edg_exception("+key+", "+std::to_string(payload_type->size)+", "+
      std::to_string(alignment)+", "+dtor+", "+(instance_mode?"this":"null")+");\ntry {\n"+init+
      id+".Complete = true; throw "+id+";\n} catch (global::System.Exception initialization_error) when (__edg_throw_filter("+id+", initialization_error)) {\n"
      "if (!global::System.Object.ReferenceEquals(initialization_error, "+id+")) {\n"+
      id+".Release(); } throw;\n} finally { global::System.GC.KeepAlive("+id+"); } }\n";
}
