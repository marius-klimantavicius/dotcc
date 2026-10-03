// c_gen_be leaves GNU overflow builtins to the native compiler. The generic
// forms operate on mathematical integers, then store the result modulo the
// destination width and report whether that destination represents it exactly.
// Preserve each actual operand's signedness; usual arithmetic conversions are
// NOT applied between the two generic input arguments.
string overflow_intrinsic_call(an_expr_node_ptr call,a_routine_ptr routine,an_expr_node_ptr first) {
  char operation=0;
  switch(routine->variant.builtin_function_kind) {
    case bfk_add_overflow:case bfk_sadd_overflow:case bfk_saddl_overflow:case bfk_saddll_overflow:
    case bfk_uadd_overflow:case bfk_uaddl_overflow:case bfk_uaddll_overflow:operation='+';break;
    case bfk_sub_overflow:case bfk_ssub_overflow:case bfk_ssubl_overflow:case bfk_ssubll_overflow:
    case bfk_usub_overflow:case bfk_usubl_overflow:case bfk_usubll_overflow:operation='-';break;
    case bfk_mul_overflow:case bfk_smul_overflow:case bfk_smull_overflow:case bfk_smulll_overflow:
    case bfk_umul_overflow:case bfk_umull_overflow:case bfk_umulll_overflow:operation='*';break;
    default:return "";
  }
  if(!first || !first->next || !first->next->next || first->next->next->next)
    unsupported("overflow builtin argument count");
  auto second=first->next,destination_node=second->next;
  auto a=skip_typerefs(first->type),b=skip_typerefs(second->type),p=skip_typerefs(destination_node->type);
  if(p->kind!=tk_pointer)unsupported("overflow builtin output pointer");
  auto qualified=p->variant.pointer.type,t=skip_typerefs(qualified);
  if(a->kind!=tk_integer || b->kind!=tk_integer || t->kind!=tk_integer ||
     !a->size || a->size>16 || !b->size || b->size>16 || !t->size || t->size>16 ||
     (get_type_qualifiers(qualified)&(TQ_CONST|TQ_C11_ATOMIC)))
    unsupported("overflow builtin integer representation");
  bool a_signed=int_type_is_signed(a),b_signed=int_type_is_signed(b),target_signed=int_type_is_signed(t);
  bool vol=(get_type_qualifiers(qualified)&TQ_VOLATILE)!=0;
  string at=type(a),bt=type(b),tt=type(t),rt=type(call->type);
  string key="overflow:"+string(1,operation)+":"+at+":"+bt+":"+tt+":"+rt+(vol?":volatile":"");
  auto found=assignment_helpers.find(key);string id;
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_overflow"+std::to_string(++serial);assignment_helpers[key]=id;
    string body,I="global::System.Int128",U="global::System.UInt128";
    unsigned bits=(unsigned)t->size*8;
    string umax=bits==128?U+".MaxValue":"((("+U+")1 << "+std::to_string(bits)+")-1)";
    string limit="(("+U+")1 << "+std::to_string(bits-1)+")";
    string smax=bits==128?I+".MaxValue":"((("+I+")1 << "+std::to_string(bits-1)+")-1)";
    string smin=bits==128?I+".MinValue":"(-(("+I+")1 << "+std::to_string(bits-1)+"))";
    if(at==bt && bt==tt && (tt=="int" || tt=="uint" || tt=="long" || tt=="ulong") && operation!='*') {
      // SQLite's signed-64 add/sub path: ordinary machine arithmetic and sign
      // tests, without wide arithmetic, managed exceptions, or allocations.
      body=tt+" value=unchecked(a"+operation+"b);bool overflow=";
      if(target_signed)body+=operation=='+'?"((a^value)&(b^value))<0;":"((a^b)&(a^value))<0;";
      else body+=operation=='+'?"value<a;":"a<b;";
    } else if(a->size<=8 && b->size<=8) {
      // Int128 holds every <=64-bit sum/difference and any such product with
      // a signed operand. Only unsigned64*unsigned64 requires UInt128.
      bool positive=operation=='*' && !a_signed && !b_signed;
      string wide=positive?U:I;
      body=wide+" exact=("+wide+")a"+operation+"("+wide+")b;"+tt+" value=unchecked(("+tt+")exact);bool overflow=";
      if(positive)body+="exact > "+(target_signed?"("+U+")("+smax+")":umax)+";";
      else if(target_signed)body+="exact < "+smin+" || exact > "+smax+";";
      else body+="exact < 0 || unchecked(("+U+")exact) > "+umax+";";
    } else {
      // 128-bit operands need up to129 sum bits or256 product bits. Carry /
      // division detects the discarded high magnitude without BigInteger.
      // Low128 bits are sufficient for every supported destination store.
      body="bool an="+string(a_signed?"a<0":"false")+",bn="+string(b_signed?"b<0":"false")+";";
      body+=U+" x="+(a_signed?"(an?unchecked(("+U+")(-("+I+")a)):("+U+")a)":"("+U+")a")+";";
      body+=U+" y="+(b_signed?"(bn?unchecked(("+U+")(-("+I+")b)):("+U+")b)":"("+U+")b")+";";
      body+="bool negative,wide=false;"+U+" magnitude;";
      if(operation=='*')body+="negative=an!=bn;wide=x!=0 && y>"+U+".MaxValue/x;magnitude=unchecked(x*y);";
      else {
        if(operation=='-')body+="bn=!bn;";
        body+="if(an==bn){negative=an;magnitude=unchecked(x+y);wide=magnitude<x;}"
              "else if(x>=y){negative=an;magnitude=x-y;}else{negative=bn;magnitude=y-x;}";
      }
      body+=tt+" value=unchecked(("+tt+")(negative?unchecked(("+U+")0-magnitude):magnitude));bool overflow=wide || ";
      if(target_signed)body+="magnitude > (negative?"+limit+":"+limit+"-1);";
      else body+="(negative && magnitude!=0) || magnitude>"+umax+";";
    }
    if(vol)body+="global::System.Threading.Thread.MemoryBarrier();";
    body+="*destination=value;";
    if(vol)body+="global::System.Threading.Thread.MemoryBarrier();";
    body+="return ("+rt+")(overflow?1:0);";
    helper_output+="// dot-gen:member-begin\n[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]\nprivate static "+rt+" "+id+"("+at+" a,"+bt+" b,"+tt+"* destination) {"+body+"}\n// dot-gen:member-end\n";
  }
  return id+"("+expr(first)+","+expr(second)+","+expr(destination_node)+")";
}
