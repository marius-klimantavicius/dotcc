// The C backend preserves C bit-field declarations/accesses (dump_field_list).
// This target defines volatile bit-fields as ordered accesses to their exact
// occupied bytes. It promises neither native MMIO container widths nor atomic
// read/modify/write operations. No access may extend outside the EDG layout.
bool volatile_bitfield(an_expr_node_ptr e) {
  auto field=bitfield(e);if(!field)return false;
  auto base=e->variant.operation.operands;
  auto object=e->variant.operation.kind==eok_points_to_field?
      skip_typerefs(base->type)->variant.pointer.type:base->type;
  // EDG's lvalue-to-rvalue conversion can clear expression qualification while
  // retaining the qualified field and/or containing object type.
  return ((get_type_qualifiers(e->type)|get_type_qualifiers(field->type)|
           get_type_qualifiers(object))&TQ_VOLATILE)!=0;
}
string volatile_bitfield_value(a_field_ptr field,const string &bits) {
  string value=bits;
  if(field->bit_size<64)value="("+value+" & "+std::to_string((1ULL<<field->bit_size)-1ULL)+"UL)";
  // EDG's configured C++ bool uses a signed storage cell, but a bool bit-field
  // still has the values zero and one rather than a signed one-bit range.
  if(!is_bool_type(field->type) && int_type_is_signed(skip_typerefs(field->type)))
    value="((long)("+value+" << "+std::to_string(64-field->bit_size)+") >> "+std::to_string(64-field->bit_size)+")";
  return "unchecked(("+type(field->type)+")("+value+"))";
}
string volatile_bitfield_address(an_expr_node_ptr e) {
  auto field=bitfield(e);auto base=e->variant.operation.operands;
  string storage_address=e->variant.operation.kind==eok_points_to_field?expr(base):address(base);
  return "((byte*)("+storage_address+") + "+std::to_string(field->offset)+")";
}
string volatile_bitfield_access(an_expr_node_ptr e,an_expr_node_ptr rhs,
                               const string &op,bool post=false) {
  auto field=bitfield(e);auto ft=skip_typerefs(field->type);
  unsigned width=(unsigned)field->bit_size,remainder=(unsigned)field->offset_bit_remainder;
  if(ft->kind!=tk_integer || !width || width>64 || remainder>7)
    unsupported("volatile bitfield representation");
  unsigned count=(remainder+width+7)/8;
  string ty=type(e->type),rt=rhs?type(rhs->type):ty;
  // Source field names are scoped presentation, never a layout identity.
  string key="volatile-bits:"+std::to_string(width)+":"+std::to_string(remainder)+":"+
      std::to_string(field->bit_field_is_signed)+":"+std::to_string(is_bool_type(field->type))+":"+
      type(field->type)+":"+ty+":"+rt+":"+op+(post?":post":"");
  string id;auto found=assignment_helpers.find(key);
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_volatile_bits"+std::to_string(++serial);assignment_helpers[key]=id;
    bool read=op!="=";string body;
    if(read) {
      for(unsigned i=0;i<count;++i)
        body+="byte b"+std::to_string(i)+"=global::System.Threading.Volatile.Read(ref p["+std::to_string(i)+"]);";
      string bits="((ulong)b0 >> "+std::to_string(remainder)+")";
      for(unsigned i=1;i<count;++i)bits+=" | ((ulong)b"+std::to_string(i)+" << "+std::to_string(i*8-remainder)+")";
      body+="ulong bits="+bits+";"+ty+" old="+volatile_bitfield_value(field,"bits")+";";
    }
    if(op.empty())body+="return old;";
    else {
      // Use EDG's configured bit-field promotion rules, including narrow
      // unsigned fields that promote to signed int. Shift counts do not take
      // part in the left operand's arithmetic conversion.
      auto promoted=source_promoted_type(e);
      string calc=(op=="<<" || op==">>")?type(promoted):
          arithmetic_type(promoted,rhs?rhs->type:promoted);
      string value=op=="="?"rhs":"(("+calc+")old) "+op+" "+
        ((op=="<<" || op==">>")?"(int)rhs":"(("+calc+")rhs)");
      body+=(read?"bits=":"ulong bits=")+string("unchecked((ulong)(")+type(field->type)+")("+value+"));";
      body+=ty+" stored="+volatile_bitfield_value(field,"bits")+";";
      for(unsigned i=0;i<count;++i) {
        int shift=(int)i*8-(int)remainder;
        unsigned low=shift<0?(unsigned)-shift:0,high=std::min(8U,remainder+width-i*8);
        unsigned mask=((1U<<(high-low))-1U)<<low;
        string fragment=shift<0?"(bits << "+std::to_string(-shift)+")":"(bits >> "+std::to_string(shift)+")";
        string merged="("+fragment+" & "+std::to_string(mask)+"UL)";
        if(mask!=255) {
          string prior=read?"b"+std::to_string(i):"global::System.Threading.Volatile.Read(ref p["+std::to_string(i)+"])";
          merged="(((ulong)"+prior+" & ~"+std::to_string(mask)+"UL) | "+merged+")";
        }
        body+="global::System.Threading.Volatile.Write(ref p["+std::to_string(i)+"], unchecked((byte)"+merged+"));";
      }
      body+="return "+string(post?"old":"stored")+";";
    }
    // Keep a discarded read observable even when the caller ignores its value.
    helper_output+="// dot-gen:member-begin\n[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]\nprivate static "+ty+" "+id+"("+(op.empty()?"":rt+" rhs, ")+"byte* p) {"+body+"}\n// dot-gen:member-end\n";
  }
  return id+"("+(op.empty()?"":(rhs?expr(rhs):"1")+", ")+volatile_bitfield_address(e)+")";
}
