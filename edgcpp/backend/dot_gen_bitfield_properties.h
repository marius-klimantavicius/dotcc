// EDG offsets/bit widths are authoritative. Like dotcc's PackMappedBitFields,
// tile only occupied bytes, then expose source-named scalar properties. Unlike
// widened access to a nominal allocation unit, no tile crosses the object or
// an ordinary field. Volatile operations retain their separate byte protocol.
string source_bit_mask(unsigned width) {
  return width==64?"18446744073709551615UL":std::to_string((1ULL<<width)-1ULL)+"UL";
}
string source_bitfield_members(a_type_ptr record) {
  struct tile { unsigned long long offset; unsigned bytes; string id; };
  string result;
  for(auto first=record->variant.class_struct_union.field_list;first;) {
    if(!first->is_bit_field || !first->bit_size){first=first->next;continue;}
    auto end=first;unsigned long long low=first->offset,high=low;
    for(;end && end->is_bit_field && end->bit_size;end=end->next) {
      auto ft=skip_typerefs(end->type);
      if(ft->kind!=tk_integer || end->bit_size>64 || end->offset_bit_remainder>7)
        unsupported("bitfield representation");
      low=std::min<unsigned long long>(low,end->offset);
      high=std::max<unsigned long long>(high,end->offset+((unsigned)end->offset_bit_remainder+(unsigned)end->bit_size+7U)/8U);
    }
    if(high>record->size)unsupported("bitfield outside record");
    std::vector<tile> tiles;
    string group=name(first,"m");if(!group.empty() && group[0]=='@')group.erase(0,1);
    for(auto offset=low;offset<high;) {
      unsigned bytes=8;
      while(bytes>1 && (bytes>alignment_of_type(record) || offset%bytes || offset+bytes>high))bytes/=2;
      string storage=bytes==8?"ulong":bytes==4?"uint":bytes==2?"ushort":"byte";
      string id="__edg_bits_"+group+"_"+std::to_string(offset);
      tiles.push_back({offset,bytes,id});
      result+="[global::System.Runtime.InteropServices.FieldOffset("+std::to_string(offset)+")] private "+storage+" "+id+";\n";
      offset+=bytes;
    }
    for(auto field=first;field!=end;field=field->next) {
      unsigned long long begin=field->offset*8+field->offset_bit_remainder,finish=begin+field->bit_size;
      string read,write;
      for(auto &part:tiles) {
        auto start=std::max(begin,part.offset*8),stop=std::min(finish,(part.offset+part.bytes)*8);
        if(start>=stop)continue;
        unsigned bits=(unsigned)(stop-start),storage_shift=(unsigned)(start-part.offset*8),value_shift=(unsigned)(start-begin);
        string mask=source_bit_mask(bits),piece="(((ulong)"+part.id+" >> "+std::to_string(storage_shift)+") & "+mask+")";
        if(value_shift)piece="("+piece+" << "+std::to_string(value_shift)+")";
        if(!read.empty())read+=" | ";
        read+=piece;
        string storage=part.bytes==8?"ulong":part.bytes==4?"uint":part.bytes==2?"ushort":"byte";
        string value=is_bool_type(field->type)?"(value != 0 ? 1UL : 0UL)":"unchecked((ulong)value)";
        write+=part.id+" = unchecked(("+storage+")(((ulong)"+part.id+" & ~("+mask+" << "+std::to_string(storage_shift)+")) | ((("+value+" >> "+std::to_string(value_shift)+") & "+mask+") << "+std::to_string(storage_shift)+"))); ";
      }
      if(field->bit_field_is_signed && !is_bool_type(field->type))
        read="((long)(("+read+") << "+std::to_string(64-field->bit_size)+") >> "+std::to_string(64-field->bit_size)+")";
      result+="public "+type(field->type)+" "+name(field,"m")+" { get => unchecked(("+type(field->type)+")("+read+")); set { "+write+"} }\n";
    }
    first=end;
  }
  return result;
}
