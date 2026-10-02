// Pointer dynamic_cast for EDG's Itanium layout. Read the existing RTTI
// initializers; port the traversal from lib_src/rtti.c without changing lowering.
unsigned rtti_constant_id(a_constant_ptr c) {
  if(!c || c->kind!=ck_address || c->variant.address.kind!=abk_variable || c->variant.address.offset)
    unsupported("RTTI class descriptor address");
  auto found=virtual_types.find(c->variant.address.variant.variable);
  if(found==virtual_types.end())unsupported("unregistered RTTI class descriptor");
  return found->second;
}
unsigned rtti_argument_id(an_expr_node_ptr e) {
  while(e && e->kind==enk_operation && e->variant.operation.kind==eok_cast)e=e->variant.operation.operands;
  if(!e || e->kind!=enk_constant)unsupported("dynamic_cast type descriptor argument");
  return rtti_constant_id(node_constant(e));
}
void ensure_rtti_helpers() {
  if(rtti_helpers_emitted)return;
  rtti_helpers_emitted=true;
  struct record { unsigned kind,flags; std::vector<unsigned> bases; string edges; };
  std::vector<record> records(virtual_types.size());
  for(auto &entry:virtual_types) {
    auto v=entry.first;
    auto source=rtti_source_types.at(v);
    if(source->kind!=tk_class && source->kind!=tk_struct)continue;
    if(v->init_kind!=initk_static)unsupported("external RTTI class descriptor");
    auto fields=cleanup_fields(v->initializer.constant,0);
    auto &rec=records[entry.second];rec.kind=0;rec.flags=3;
    auto add=[&](a_constant_ptr base,unsigned long long bits) {
      unsigned id=rtti_constant_id(base);rec.bases.push_back(id);
      rec.edges+="new __edg_rtti_edge("+std::to_string(id)+", unchecked((long)"+std::to_string(bits)+"UL)),\n";
    };
    if(fields.size()==2) { // __si_class_type_info
      rec.kind=1;add(fields[1],2);
    } else if(fields.size()==4) { // __vmi_class_type_info
      rec.kind=2;rec.flags=(unsigned)cleanup_integer(fields[1]);
      auto bases=cleanup_fields(fields[3],0);
      if(cleanup_integer(fields[2])!=bases.size() || (rec.flags & ~3U))unsupported("RTTI base count/flags");
      for(auto base:bases) {
        auto f=cleanup_fields(base,2);
        if(f[1]->kind!=ck_integer || type(f[1]->type)!="long")unsupported("RTTI offset/flags integer ABI");
        a_boolean overflow;
        // Virtual-base vtable offsets are negative signed longs. Preserve
        // their two's-complement bits, just as constant() does for literals.
        auto bits=unsigned_value_of_integer_constant(f[1],&overflow);
        if(overflow && !int_constant_is_signed(f[1]))unsupported("RTTI offset/flags overflow");
        if(bits & 0xfcULL)unsupported("RTTI base flags");
        add(f[0],bits);
      }
    } else if(fields.size()!=1)unsupported("RTTI class initializer shape");
  }
  // Bound recursion/path expansion in the generated walker; sharing virtual
  // bases does not remove repeated traversal paths in the native algorithm.
  auto count_paths=[&](auto &&self,unsigned id,unsigned depth)->unsigned long long {
    if(depth>128)unsupported("RTTI inheritance depth exceeds 128");
    unsigned long long n=1;
    for(auto b:records[id].bases) {
      n+=self(self,b,depth+1);
      if(n>65536)unsupported("RTTI inheritance paths exceed 65536");
    }
    return n;
  };
  string entries;
  for(unsigned i=0;i<records.size();++i) {
    (void)count_paths(count_paths,i,0);
    auto &r=records[i];
    entries+="new __edg_rtti_type("+std::to_string(r.kind)+", "+std::to_string(r.flags)+", new __edg_rtti_edge[] {"+r.edges+"}),\n";
  }
  helper_output+=R"CS(
// dot-gen:member-begin
private readonly struct __edg_rtti_edge {
  internal readonly int Type; internal readonly long Bits;
  internal __edg_rtti_edge(int type,long bits) { Type=type;Bits=bits; }
  internal bool Public => (Bits & 2)!=0;
  internal bool Virtual => (Bits & 1)!=0;
  internal byte* Address(byte* p) => p + (Virtual ? *(long*)((byte*)*(long**)p + (Bits >> 8)) : (Bits >> 8));
}
// dot-gen:member-end
// dot-gen:member-begin
private sealed class __edg_rtti_type {
  internal readonly int Kind,Flags; internal readonly __edg_rtti_edge[] Bases;
  internal __edg_rtti_type(int kind,int flags,__edg_rtti_edge[] bases) { Kind=kind;Flags=flags;Bases=bases; }
}
// dot-gen:member-end
// dot-gen:member-begin
private static class __edg_rtti_graph {
  internal static readonly __edg_rtti_type[] Types = new __edg_rtti_type[] {
)CS"+entries+R"CS(};
}
// dot-gen:member-end
// dot-gen:member-begin
private struct __edg_downcast_state {
  internal int SourceType,DestType;
  internal byte* Source,Dest,Result;
  internal bool PublicOnly,Terminate;
}
// dot-gen:member-end
// dot-gen:member-begin
private static void __edg_downcast_walk(byte* p,int type,ref __edg_downcast_state s) {
  // traverse_base_classes with tbc_downcast and tbc_post_downcast.
  if(type==s.DestType) { s.Dest=p;s.PublicOnly=true; }
  else if(s.Dest!=null && p==s.Source && type==s.SourceType) {
    if(s.Result!=null) { s.Result=null;s.Terminate=true; }
    else s.Result=s.Dest;
  }
  var info=__edg_rtti_graph.Types[type];
  foreach(var edge in info.Bases) {
    if(info.Kind==2 && s.PublicOnly && !edge.Public)continue;
    __edg_downcast_walk(edge.Address(p),edge.Type,ref s);
    if(s.Terminate)return;
  }
  if(type==s.DestType) { s.Dest=null;s.PublicOnly=false; }
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_find_public_source(byte* p,int type,byte* source,int sourceType) {
  // find_base_class_at_addr: both the address and the type must match.
  var info=__edg_rtti_graph.Types[type];
  foreach(var edge in info.Bases) {
    byte* child=edge.Address(p);
    if(child==source && edge.Type==sourceType)return edge.Public;
    if(edge.Public && __edg_find_public_source(child,edge.Type,source,sourceType))return true;
  }
  return false;
}
// dot-gen:member-end
// dot-gen:member-begin
private static bool __edg_base_conversion(byte* p,int type,int dest,int flags,
    ref byte* result,ref bool ambiguous,bool accessible,ref int virtualParent,
    ref int virtuality,ref bool resultAccessible) {
  // derived_to_base_conversion_r (nonnull-object path). Virtuality values:
  // 0 unknown, 1 nonvirtual, 2 direct virtual, as in the native runtime.
  var info=__edg_rtti_graph.Types[type];
  bool found=false;
  if(info.Kind==1) {
    var edge=info.Bases[0];
    if(edge.Type==dest) {
      if((result!=null && result!=p) || virtuality==2 || ambiguous) {
        ambiguous=true;result=null;return false;
      }
      virtuality=1;resultAccessible=accessible;result=p;return true;
    }
    found=__edg_base_conversion(p,edge.Type,dest,flags,ref result,ref ambiguous,
      accessible,ref virtualParent,ref virtuality,ref resultAccessible);
    return found && !ambiguous;
  }
  foreach(var edge in info.Bases) {
    byte* child=edge.Address(p);
    bool childAccessible=accessible && edge.Public;
    if(edge.Type==dest) {
      if((result!=null && child!=result) || (edge.Virtual && virtuality==1) || ambiguous) {
        ambiguous=true;result=null;return false;
      }
      virtuality=edge.Virtual?2:1;resultAccessible=childAccessible;result=child;found=true;
      if(resultAccessible && (edge.Virtual ? (flags & 2)==0 : (flags & 1)==0))break;
    } else {
      int parentHere=-1,virtualityHere=0;
      bool matched=__edg_base_conversion(child,edge.Type,dest,flags,ref result,ref ambiguous,
        childAccessible,ref parentHere,ref virtualityHere,ref resultAccessible);
      if(matched || ambiguous) {
        if(ambiguous)return false;
        if(parentHere<0 && edge.Virtual)parentHere=edge.Type;
        if(virtuality==0 || (virtualityHere==2 && virtuality==2) || (virtualParent>=0 && virtualParent==parentHere)) {
          virtuality=virtualityHere;virtualParent=parentHere;found=true;
          if(resultAccessible && (edge.Virtual ? (flags & 2)==0 : (flags & 1)==0))break;
        } else { ambiguous=true;return false; }
      }
    }
  }
  return found;
}
// dot-gen:member-end
// dot-gen:member-begin
private static void* __edg_dynamic_cast(void* source,int sourceType,int destType) {
  // EDG emits the null guard. This guard also makes the helper self-contained.
  if(source==null)return null;
  long* vtable=*(long**)source;
  byte* complete=(byte*)source+vtable[-2];
  long token=(byte*)vtable[-1]-__edg_type_token_base;
  if(token<0 || (token & 7)!=0 || (ulong)(token/8)>=(ulong)__edg_rtti_graph.Types.Length)
    global::System.Environment.FailFast("Foreign EDG RTTI token");
  int type=(int)(token/8);
  var state=new __edg_downcast_state { SourceType=sourceType,DestType=destType,Source=(byte*)source };
  __edg_downcast_walk(complete,type,ref state);
  if(state.Result!=null)return state.Result;
  if(type!=sourceType && !__edg_find_public_source(complete,type,(byte*)source,sourceType))return null;
  if(type==destType)return complete;
  byte* result=null;bool ambiguous=false,accessible=true;int virtualParent=-1,virtuality=0;
  bool found=__edg_base_conversion(complete,type,destType,__edg_rtti_graph.Types[type].Flags,
    ref result,ref ambiguous,true,ref virtualParent,ref virtuality,ref accessible);
  return found && accessible ? result : null;
}
// dot-gen:member-end
)CS";
}
string dynamic_cast_call(a_routine_ptr r,an_expr_node_ptr args) {
  auto rt=skip_typerefs(r->type);auto params=rt->variant.routine.extra_info->param_type_list;
  auto p=args;
  for(unsigned i=0;i<4;++i) {
    if(!params || !p || (i<3?skip_typerefs(params->type)->kind!=tk_pointer:type(params->type)!="long"))
      unsupported("dynamic_cast runtime ABI");
    params=params->next;p=p->next;
  }
  if(params || p || type(rt->variant.routine.return_type)!="void*")unsupported("dynamic_cast runtime ABI");
  unsigned source=rtti_argument_id(args->next),dest=rtti_argument_id(args->next->next);
  auto hint=args->next->next->next;
  if(hint->kind!=enk_constant || node_constant(hint)->kind!=ck_integer)unsupported("dynamic_cast hint");
  ensure_rtti_helpers();
  return "__edg_dynamic_cast("+expr(args)+", "+std::to_string(source)+", "+std::to_string(dest)+")";
}
