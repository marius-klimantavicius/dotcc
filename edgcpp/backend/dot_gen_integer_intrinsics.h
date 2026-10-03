// The original C backend leaves these calls to the native compiler. Preserve
// each builtin's declared width and evaluate its operand exactly once.
string integer_intrinsic_call(an_expr_node_ptr call,a_routine_ptr routine,an_expr_node_ptr first) {
  unsigned width=0;string operation;bool byte_swap=false;
  switch(routine->variant.builtin_function_kind) {
    case bfk_clz:width=4;operation="LeadingZeroCount";break;
    case bfk_clzl:case bfk_clzll:width=8;operation="LeadingZeroCount";break;
    case bfk_ctz:width=4;operation="TrailingZeroCount";break;
    case bfk_ctzl:case bfk_ctzll:width=8;operation="TrailingZeroCount";break;
    case bfk_popcount:width=4;operation="PopCount";break;
    case bfk_popcountl:case bfk_popcountll:width=8;operation="PopCount";break;
    case bfk_parity:width=4;operation="parity";break;
    case bfk_parityl:case bfk_parityll:width=8;operation="parity";break;
    case bfk_ffs:width=4;operation="ffs";break;
    case bfk_ffsl:case bfk_ffsll:width=8;operation="ffs";break;
    case bfk_clrsb:width=4;operation="clrsb";break;
    case bfk_clrsbl:case bfk_clrsbll:width=8;operation="clrsb";break;
    case bfk_bswap16:width=2;byte_swap=true;break;
    case bfk_bswap32:width=4;byte_swap=true;break;
    case bfk_bswap64:width=8;byte_swap=true;break;
    case bfk_bswap128:width=16;byte_swap=true;break;
    default:return "";
  }
  if(!first || first->next || skip_typerefs(first->type)->kind!=tk_integer ||
     skip_typerefs(first->type)->size!=width ||
     (!byte_swap && type(call->type)!="int") ||
     (byte_swap && skip_typerefs(call->type)->size!=width))unsupported("integer builtin signature");
  string ty=type(first->type),result=type(call->type),u=width==2?"ushort":width==4?"uint":width==8?"ulong":"global::System.UInt128";
  string bits="unchecked(("+u+")value)",body,prefix="global::System.Numerics.BitOperations.";
  if(byte_swap) {
    string reverse="global::System.Buffers.Binary.BinaryPrimitives.ReverseEndianness";
    body=width==16?"(((global::System.UInt128)"+reverse+"((ulong)value)<<64)|"+reverse+"((ulong)(value>>64)))":reverse+"("+bits+")";
  } else if(operation=="parity")body="("+prefix+"PopCount("+bits+")&1)";
  else if(operation=="ffs")body="value==0?0:"+prefix+"TrailingZeroCount("+bits+")+1";
  else if(operation=="clrsb") {
    string signed_type=width==4?"int":"long",shift=std::to_string(width*8-1);
    body=prefix+"LeadingZeroCount(unchecked(("+u+")(("+signed_type+")value ^ (("+signed_type+")value>>"+shift+"))))-1";
  } else body=prefix+operation+"("+bits+")";
  string key="integer-builtin:"+std::to_string((unsigned)routine->variant.builtin_function_kind)+":"+ty;
  auto found=assignment_helpers.find(key);string id;
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_integer_builtin"+std::to_string(++serial);assignment_helpers[key]=id;
    helper_output+="// dot-gen:member-begin\nprivate static "+result+" "+id+"("+ty+" value) {return unchecked(("+result+")("+body+"));}\n// dot-gen:member-end\n";
  }
  return id+"("+expr(first)+")";
}
