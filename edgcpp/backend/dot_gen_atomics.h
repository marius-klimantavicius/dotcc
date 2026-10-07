// Preserve EDG's builtin lowering. c_gen_be.c removes the leading size for
// sfk_gnu_atomic_generic_function to restore GCC's source-level builtin;
// our runtime consumes that explicit size instead.
string atomic_call(a_routine_ptr routine, an_expr_node_ptr first) {
  if(routine->function_def_number!=NULL_function_def_number || !routine->compiler_generated ||
     !routine->source_corresp.name)return "";
  string source=routine->source_corresp.name;
  bool legacy=source.compare(0,7,"__sync_")==0;
  if(source.compare(0,9,"__atomic_")!=0 && !legacy)return "";
  auto rt=skip_typerefs(routine->type);
  auto extra=rt->variant.routine.extra_info;
  std::vector<string> arguments,parameters;
  for(auto a=first;a;a=a->next)arguments.push_back(expr(a));
  for(auto p=extra->param_type_list;p;p=p->next)parameters.push_back(type(p->type));
  string result=type(rt->variant.routine.return_type);
  // EDG lowers C++ bool to target char, while C _Bool uses unsigned char.
  string boolean_type=type(source_integer_view(BOOL_INT_KIND));
  auto signature=[&](string expected_result,std::vector<string> expected) {
    if(extra->has_ellipsis || result!=expected_result || parameters!=expected || arguments.size()!=expected.size())
      unsupported(("atomic builtin ABI: "+source).c_str());
  };
  string prefix="global::Edg.Runtime.AtomicOps.";
  auto joined=[&](size_t begin=0) {
    string value;
    for(size_t i=begin;i<arguments.size();++i){if(i!=begin)value+=", ";value+=arguments[i];}
    return value;
  };
  auto truth=[](string value){return "(("+value+") != 0)";};
  auto boolean=[&](string value){return "(("+boolean_type+")("+value+" ? 1 : 0))";};
  if(source=="__sync_synchronize") {
    signature("void",{});return prefix+"ThreadFence(5)";
  }
  if(source=="__atomic_thread_fence" || source=="__atomic_signal_fence") {
    signature("void",{"int"});
    return prefix+(source=="__atomic_thread_fence"?"ThreadFence(":"SignalFence(")+joined()+")";
  }
  if(source=="__atomic_is_lock_free" || source=="__atomic_always_lock_free") {
    signature(boolean_type,{"ulong","void*"});
    return boolean(prefix+"IsLockFree((nuint)("+arguments[0]+"), "+arguments[1]+")");
  }
  if(source=="__atomic_test_and_set" || source=="__atomic_clear") {
    signature(source=="__atomic_clear"?"void":boolean_type,{"void*","int"});
    string call=prefix+(source=="__atomic_clear"?"Clear(":"TestAndSet(")+"ref *(byte*)("+arguments[0]+"), "+arguments[1]+")";
    return source=="__atomic_clear"?call:boolean(call);
  }
  if(source=="__atomic_load" || source=="__atomic_store") {
    signature("void",{"ulong","void*","void*","int"});
    return prefix+(source=="__atomic_load"?"LoadBytes(":"StoreBytes(")+joined()+")";
  }
  if(source=="__atomic_exchange") {
    signature("void",{"ulong","void*","void*","void*","int"});
    return prefix+"ExchangeBytes("+joined()+")";
  }
  if(source=="__atomic_compare_exchange") {
    signature(boolean_type,{"ulong","void*","void*","void*",boolean_type,"int","int"});
    arguments[4]=truth(arguments[4]);
    return boolean(prefix+"CompareExchangeBytes("+joined()+")");
  }
  auto split=source.find_last_of('_');
  string width=source.substr(split+1),value_type;
  if(width=="1")value_type="byte";
  else if(width=="2")value_type="ushort";
  else if(width=="4")value_type="uint";
  else if(width=="8")value_type="ulong";
  else if(width=="16")value_type="global::System.UInt128";
  else return "";
  size_t prefix_size=legacy?7:9;
  string operation=source.substr(prefix_size,split-prefix_size);
  string address=arguments.empty()?"":"ref *("+value_type+"*)("+arguments[0]+")";
  if(legacy) {
    // Like c_gen_be, consume EDG's concrete size-suffixed GNU builtins.
    // Reuse the same storage primitives as __atomic so both families share
    // synchronization, including the unaligned/128-bit lock fallback.
    if(operation=="val_compare_and_swap" || operation=="bool_compare_and_swap") {
      bool returns_bool=operation=="bool_compare_and_swap";
      signature(returns_bool?boolean_type:value_type,{"void*",value_type,value_type});
      string call=prefix+(returns_bool?"CompareExchangeMatches(":"CompareExchangeValue(")+
          address+", "+arguments[1]+", "+arguments[2]+")";
      return returns_bool?boolean(call):call;
    }
    if(operation=="lock_test_and_set") {
      signature(value_type,{"void*",value_type});
      return prefix+"Exchange("+address+", "+arguments[1]+", 5)";
    }
    if(operation=="lock_release") {
      signature("void",{"void*"});
      return prefix+"Store("+address+", ("+value_type+")0, 5)";
    }
    const std::map<string,string> legacy_operations={
      {"fetch_and_add","FetchAdd"},{"add_and_fetch","AddFetch"},
      {"fetch_and_sub","FetchSub"},{"sub_and_fetch","SubFetch"},
      {"fetch_and_and","FetchAnd"},{"and_and_fetch","AndFetch"},
      {"fetch_and_or","FetchOr"},{"or_and_fetch","OrFetch"},
      {"fetch_and_xor","FetchXor"},{"xor_and_fetch","XorFetch"},
      {"fetch_and_nand","FetchNand"},{"nand_and_fetch","NandFetch"},
    };
    auto method=legacy_operations.find(operation);
    if(method==legacy_operations.end())return "";
    signature(value_type,{"void*",value_type});
    return prefix+method->second+"("+address+", "+arguments[1]+", 5)";
  }
  if(operation=="load") {
    signature(value_type,{"void*","int"});
    return prefix+"Load("+address+", "+arguments[1]+")";
  }
  if(operation=="store" || operation=="exchange") {
    signature(operation=="store"?"void":value_type,{"void*",value_type,"int"});
    return prefix+(operation=="store"?"Store(":"Exchange(")+address+", "+arguments[1]+", "+arguments[2]+")";
  }
  if(operation=="compare_exchange") {
    signature(boolean_type,{"void*","void*",value_type,boolean_type,"int","int"});
    return boolean(prefix+"CompareExchange("+address+", ref *("+value_type+"*)("+arguments[1]+"), "+
        arguments[2]+", "+truth(arguments[3])+", "+arguments[4]+", "+arguments[5]+")");
  }
  const std::map<string,string> operations={
    {"fetch_add","FetchAdd"},{"add_fetch","AddFetch"},
    {"fetch_sub","FetchSub"},{"sub_fetch","SubFetch"},
    {"fetch_and","FetchAnd"},{"and_fetch","AndFetch"},
    {"fetch_or","FetchOr"},{"or_fetch","OrFetch"},
    {"fetch_xor","FetchXor"},{"xor_fetch","XorFetch"},
    {"fetch_nand","FetchNand"},{"nand_fetch","NandFetch"},
  };
  auto method=operations.find(operation);
  if(method==operations.end())return "";
  signature(value_type,{"void*",value_type,"int"});
  return prefix+method->second+"("+address+", "+arguments[1]+", "+arguments[2]+")";
}
