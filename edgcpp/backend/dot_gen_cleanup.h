// Consume EDG's partial-EH region tables at translation time. The generated
// program uses CLR unwinding; no native EH tables, stack offsets or setjmp.
#if !GENERATE_EH_TABLES || DO_FULL_PORTABLE_EH_LOWERING
#error "The C# cleanup emitter requires EDG partial EH tables."
#endif
bool cleanup_cursor_used=false;
a_variable_ptr cleanup_region_var=nullptr, cleanup_array_var=nullptr, cleanup_this=nullptr;
std::set<unsigned long long> cleanup_references;
std::map<a_variable_ptr,a_constant_ptr> cleanup_initializers;
struct cleanup_region {
  a_routine_ptr destructor;
  a_constant_ptr handle;
  unsigned long long next, flags;
  a_constant_ptr condition;
  a_constant_ptr vtt;
  a_constant_ptr vla_count;
  unsigned long long vtt_flags;
};
std::map<unsigned long long,cleanup_region> cleanup_regions;
std::vector<std::vector<a_constant_ptr>> cleanup_arrays;
std::vector<a_variable_ptr> cleanup_capture_variables;
a_routine_ptr cleanup_routine=nullptr;
void capture_cleanup_handle(a_constant_ptr c) {
  if(!c || c->kind!=ck_stack_offset)return;
  auto v=c->variant.stack_offset.variable;
  if(v && !global_offsets.count(v) && !tls_offsets.count(v) &&
     std::find(cleanup_capture_variables.begin(),cleanup_capture_variables.end(),v)==cleanup_capture_variables.end())
    cleanup_capture_variables.push_back(v);
}
unsigned long long cleanup_integer(a_constant_ptr c) {
  if(!c || c->kind!=ck_integer)unsupported("non-integer cleanup table field");
  a_boolean overflow;
  auto result=unsigned_value_of_integer_constant(c,&overflow);
  if(overflow)unsupported("overflow in cleanup table field");
  return result;
}
std::vector<a_constant_ptr> cleanup_fields(a_constant_ptr c,size_t count) {
  if(!c || c->kind!=ck_aggregate)unsupported("cleanup table aggregate");
  std::vector<a_constant_ptr> result;
  for(auto p=c->variant.aggregate.first_constant;p;p=p->next)result.push_back(p);
  if(count && result.size()!=count)unsupported("cleanup table field count");
  return result;
}
std::vector<a_constant_ptr> cleanup_table(a_variable_ptr v) {
  if(!v)return {};
  auto found=cleanup_initializers.find(v);
  if(found==cleanup_initializers.end())unsupported("missing cleanup table initializer");
  return cleanup_fields(found->second,0);
}
void find_cleanup_prologue(a_statement_ptr s) {
  if(!s)return;
  if(s->kind==stmk_expr && s->expr && s->expr->kind==enk_lowered_eh_construct &&
     s->expr->variant.lowered_eh.kind==leck_function_prologue) {
    auto p=s->expr->variant.lowered_eh.variant.prologue_info;
    if(cleanup_region_var || cleanup_array_var)unsupported("multiple cleanup prologues");
    cleanup_region_var=p->region_table; cleanup_array_var=p->array_table;
  }
  if(s->kind==stmk_block)
    for(auto p=s->variant.block.statements;p;p=p->next)find_cleanup_prologue(p);
}
void collect_cleanup_initializers(a_scope_ptr scope) {
  for(auto p=scope->local_static_variable_inits;p;p=p->next) {
    if(p->variable==cleanup_region_var || p->variable==cleanup_array_var) {
      if(p->init_kind!=initk_static)unsupported("dynamic cleanup table initializer");
      cleanup_initializers[p->variable]=p->initializer.constant;
    }
  }
  for(auto child=scope->scopes;child;child=child->next)collect_cleanup_initializers(child);
}
void prepare_cleanup(a_scope_ptr scope) {
  cleanup_cursor_used=false; cleanup_region_var=nullptr; cleanup_array_var=nullptr; cleanup_this=nullptr;
  cleanup_regions.clear(); cleanup_arrays.clear(); cleanup_references.clear();
  cleanup_capture_variables.clear();cleanup_routine=scope->variant.routine.ptr;
  find_cleanup_prologue(scope->assoc_block);
  cleanup_initializers.clear(); collect_cleanup_initializers(scope);
  for(auto p=scope->variant.routine.parameters;p;p=p->next)
    if(p->is_this_parameter)cleanup_this=p;
  for(auto c:cleanup_table(cleanup_array_var))cleanup_arrays.push_back(cleanup_fields(c,3));
  auto entries=cleanup_table(cleanup_region_var);
  if(entries.size()>4096)unsupported("more than 4096 EH table entries");
  for(size_t i=0;i<entries.size();++i) {
    auto f=cleanup_fields(entries[i],4);
    auto dtor=f[0];
    auto flags=cleanup_integer(f[3]);
    bool guard=(flags & 0x80)!=0;
    if(!guard && (dtor->kind!=ck_address || dtor->variant.address.kind!=abk_routine ||
       dtor->variant.address.offset))unsupported("cleanup destructor constant");
    cleanup_region r={guard?nullptr:dtor->variant.address.variant.routine,f[1],cleanup_integer(f[2]),flags,nullptr,nullptr,nullptr,0};
    // 0x40 means VLA with ARRAY, otherwise base subobject. 0x20 is a
    // VTT supplement only for a non-array base-subobject entry.
    if((r.flags & ~0xffULL) || ((r.flags & 0x20) && (!(r.flags & 0x40) || (r.flags & 8))) ||
       ((r.flags & 4) && (r.flags & (0x20|0x40))))
      unsupported("cleanup region flags",(int)r.flags);
    auto index=i;
    if(r.flags & 2) {
      if(++i>=entries.size())unsupported("missing cleanup condition entry");
      auto condition=cleanup_fields(entries[i],4);
      if(cleanup_integer(condition[3])!=0)unsupported("cleanup condition flags");
      r.condition=condition[1];
    }
    if(r.flags & 0x20) {
      if(++i>=entries.size())unsupported("missing cleanup VTT supplement");
      auto supplement=cleanup_fields(entries[i],4);
      r.vtt=supplement[1];r.vtt_flags=cleanup_integer(supplement[3]);
      if(r.vtt_flags & ~1ULL)unsupported("cleanup VTT flags");
    }
    if((r.flags & 0x48)==0x48) {
      if(++i>=entries.size())unsupported("missing VLA element count supplement");
      auto supplement=cleanup_fields(entries[i],4);
      if(cleanup_integer(supplement[3])!=0)unsupported("VLA count supplement flags");
      r.vla_count=supplement[1];
    }
    cleanup_regions[index]=r;
  }
  for(auto &entry:cleanup_regions) {
    auto &r=entry.second;
    capture_cleanup_handle(r.handle);capture_cleanup_handle(r.condition);capture_cleanup_handle(r.vtt);capture_cleanup_handle(r.vla_count);
    if((r.flags & 0x10) && cleanup_this &&
       std::find(cleanup_capture_variables.begin(),cleanup_capture_variables.end(),cleanup_this)==cleanup_capture_variables.end())
      cleanup_capture_variables.push_back(cleanup_this);
  }
  for(auto &entry:cleanup_arrays)for(auto c:entry)capture_cleanup_handle(c);
  for(auto &entry:cleanup_regions) {
    std::set<unsigned long long> visited;
    for(auto index=entry.first;index!=null_eh_region_number;) {
      if(!visited.insert(index).second)unsupported("cyclic cleanup region chain");
      auto found=cleanup_regions.find(index);
      if(found==cleanup_regions.end())unsupported("invalid cleanup region link");
      index=found->second.next;
    }
  }
}
unsigned long long cleanup_state(unsigned long long region) {
  return region==null_eh_region_number?0:region+1;
}
string cleanup_transition(unsigned long long region,const string &slot) {
  cleanup_cursor_used=true;
  cleanup_references.insert(region);
  return slot+" = "+std::to_string(cleanup_state(region))+";\n";
}
string cleanup_handle(a_constant_ptr c,unsigned long long flags) {
  if(c && c->kind==ck_address && c->variant.address.kind==abk_variable) {
    string address="((byte*)("+constant(c)+"))";
    return (flags & 1)?"(*(byte**)"+address+")":address;
  }
  if(flags & 0x10) {
    if(!cleanup_this || (flags & 1))unsupported("cleanup this handle");
    return "((byte*)"+variable(cleanup_this)+" + "+std::to_string(cleanup_integer(c))+"UL)";
  }
  if(!c || c->kind!=ck_stack_offset || !c->variant.stack_offset.variable)
    unsupported("cleanup variable handle");
  auto &p=c->variant.stack_offset;
  string base=variable(p.variable);
  // A lowered VLA variable is already the array-storage pointer. EDG's C
  // backend initializes its object-address table through
  // make_address_of_init_entity_node, which applies this VLA indirection.
  if(!(flags & 1) && !p.variable->is_vla)base="&("+base+")";
  return "((byte*)("+base+") + "+std::to_string(p.offset)+"UL)";
}
void ensure_exception_helpers();
void ensure_terminate_helpers();
void ensure_cleanup_failure_filter() {
  ensure_terminate_helpers();
  const string name="__edg_cleanup_failure";
  if(helper_output.find("bool "+name+"(")!=string::npos)return;
  // throw.c pass 1 terminates upon reaching a throw-processing marker,
  // before pass 2 unwinds locals in the escaping destructor. A CLR catch
  // body is too late: use its first-pass filter to retain that ordering.
  helper_output+="// dot-gen:member-begin\nprivate static bool __edg_begin_unwind(global::System.Exception error, ref bool unwinding) { if("+string(c11_thread_exit_used?"error is global::Edg.Runtime.CThreadExitTransfer || ":"")+"__edg_begin_exception(error)) unwinding = true; return false; }\n// dot-gen:member-end\n";
  helper_output+="// dot-gen:member-begin\nprivate "+string(instance_mode?"":"static ")+"bool "+name+
      "(global::System.Exception error) { if(!("+control_transfer_exclusion("error")+"))return false; __edg_terminate_runtime(error,\"Destructor threw during unwinding\"); return false; }\n// dot-gen:member-end\n";
}
bool coroutine_descriptive_region(unsigned long long region);
string emit_cleanup() {
  ensure_cleanup_failure_filter();
  for(auto n:cleanup_references)
    if(n!=null_eh_region_number && !cleanup_regions.count(n))unsupported("unknown cleanup state");
  string result;
  for(auto &entry:cleanup_regions) {
    auto &region=entry.second;
    auto r=region.destructor;
    if(coroutine_descriptive_region(entry.first)) {
      result+="case "+std::to_string(entry.first+1)+": __edg_cleanup_state = "+std::to_string(cleanup_state(region.next))+"; break;\n";
      continue;
    }
    if(region.flags & 0x80) {
      ensure_lifecycle();
      result+="case "+std::to_string(entry.first+1)+": __edg_cleanup_state = "+std::to_string(cleanup_state(region.next))+"; __edg_guard_finish("+cleanup_handle(region.handle,region.flags)+", false); break;\n";
      continue;
    }
    if(r && r->compiler_generated && r->source_corresp.name &&
       string(r->source_corresp.name)=="__destroy_exception_object") {
      // Release this handler's use after its locals and catch parameter.
      // Rethrows retain the payload; the final release calls its destructor.
      if(region.flags)unsupported("exception object cleanup flags");
      result+="case "+std::to_string(entry.first+1)+": __edg_cleanup_state = "+
          std::to_string(cleanup_state(region.next))+"; __edg_current_catch!.Leave(); break;\n";
      continue;
    }
    bool vla_deallocation=r && r->compiler_generated && r->source_corresp.name &&
        string(r->source_corresp.name)=="__vla_dealloc_eh";
    if(!r || (r->function_def_number==NULL_function_def_number && !vla_deallocation && !unbound_routine(r)))unsupported("external cleanup destructor");
    auto rt=skip_typerefs(r->type);
    auto params=rt->variant.routine.extra_info->param_type_list;
    bool sized_delete=(region.flags & 12)==12;
    if(!params || skip_typerefs(rt->variant.routine.return_type)->kind!=tk_void ||
       ((region.vtt || sized_delete)?(!params->next || params->next->next ||
         (sized_delete?type(params->next->type)!="ulong":skip_typerefs(params->next->type)->kind!=tk_pointer)):params->next!=nullptr))
      unsupported("destructor ABI");
    string destruction;
    if(vla_deallocation) {
      if((region.flags & ~3ULL)!=4 || type(params->type)!="void*")unsupported("VLA deallocation cleanup ABI");
      destruction="global::Edg.Runtime.Vla.Release("+cleanup_handle(region.handle,region.flags)+");";
    } else if(region.flags & 4) {
      string address,size_arg;
      if(sized_delete) {
        auto index=cleanup_integer(region.handle);
        if(index>=cleanup_arrays.size())unsupported("sized delete array table index");
        auto &a=cleanup_arrays[index];
        address=cleanup_handle(a[0],region.flags);
        size_arg=", "+std::to_string(cleanup_integer(a[1]))+"UL";
      } else address=cleanup_handle(region.handle,region.flags);
      // throw.c uses the array supplement's element_size for sized delete,
      // and does not call either form for a null allocation address.
      destruction="{ byte* allocation = "+address+"; if(allocation != null) "+source_routine_reference(r,true)+
          "(("+type(params->type)+")allocation"+size_arg+"); }";
    } else if(region.flags & 8) {
      auto index=cleanup_integer(region.handle);
      if(index>=cleanup_arrays.size())unsupported("cleanup array index");
      auto &a=cleanup_arrays[index];
      auto size=cleanup_integer(a[1]);
      string count;
      if(region.vla_count)count="(*(ulong*)"+cleanup_handle(region.vla_count,0)+")";
      else {
        auto value=cleanup_integer(a[2]);
        if(value>64*1024*1024 || (size && value>64*1024*1024/size))unsupported("excessive cleanup array");
        count=std::to_string(value)+"UL";
      }
      if(!size || size>64*1024*1024)unsupported("cleanup array element size");
      ensure_array_helpers();
      destruction="__edg_vec_cleanup("+cleanup_handle(a[0],region.flags)+", "+count+
          ", "+std::to_string(size)+"UL, ("+array_callback_type()+")"+routine_address(r)+");";
    } else {
      destruction=source_routine_reference(r,true)+"(("+type(params->type)+")"+cleanup_handle(region.handle,region.flags);
      if(region.vtt)destruction+=", ("+type(params->next->type)+")"+cleanup_handle(region.vtt,region.vtt_flags);
      destruction+=");";
    }
    if(region.condition) {
      string flag="(*(int*)"+cleanup_handle(region.condition,0)+")";
      destruction="if ("+flag+" != 0) { "+flag+" = 0; "+destruction+" }";
    }
    result+="case "+std::to_string(entry.first+1)+": __edg_cleanup_state = "+
            std::to_string(cleanup_state(region.next))+";\n"+destruction+" break;\n";
  }
  return result;
}
string cleanup_unwind(const string &boundary,const string &slot="__edg_cleanup_state") {
  string call="__edg_cleanup_"+source_identifier_component(name(cleanup_routine,"f"))+"("+boundary+", &("+slot+")";
  for(auto v:cleanup_capture_variables)call+=", "+variable_address(v);
  return call+");\n";
}
void emit_cleanup_helper() {
  // One cold decoder per function. Repeating a full switch in each CLR
  // finally makes native compilation grow dramatically with nested handlers.
  // Pass addresses, not captured locals: sequence-expression helpers have
  // their own mapping, and destructors must always observe caller storage.
  auto saved=expression_variables;
  string parameters="int boundary, int* state";
  for(size_t i=0;i<cleanup_capture_variables.size();++i) {
    auto v=cleanup_capture_variables[i];string p="p"+std::to_string(i);
    parameters+=", "+type(v->type)+"* "+p;
    expression_variables[v]="(*"+p+")";
  }
  string cases;{ source_qualified_reference_guard references;cases=emit_cleanup(); }
  expression_variables=saved;
  const string marker="__edg_cleanup_state",replacement="(*state)";
  for(size_t pos=cases.find(marker);pos!=string::npos;pos=cases.find(marker,pos+replacement.size()))
    cases.replace(pos,marker.size(),replacement);
  helper_output+="// dot-gen:member-begin\n[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]\nprivate "+
      string(instance_mode?"":"static ")+"void __edg_cleanup_"+source_identifier_component(name(cleanup_routine,"f"))+"("+parameters+") {\n"
      "try { while (*state != boundary) { switch (*state) {\n"+cases+
      "default: global::System.Environment.FailFast(\"Invalid EDG cleanup boundary\"); break;\n"
      "} } } catch (global::System.Exception error) when (__edg_cleanup_failure(error)) { throw; }\n}\n// dot-gen:member-end\n";
}
