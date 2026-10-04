// Primitive initializer tables live in assembly data, not per-call arrays.
// Floating tables use their exact EDG bit patterns; a typed view avoids decimal
// round-trips, NaN canonicalization and loss of negative zero.
std::map<string,string> primitive_data_properties;
string primitive_data_property(const string &storage_type,const string &view_type,const string &values) {
  string key=storage_type+":"+view_type+":"+values;
  auto found=primitive_data_properties.find(key);
  if(found!=primitive_data_properties.end())return found->second;
  string id="__edg_initializer"+std::to_string(++serial);
  string backing=storage_type==view_type?id:id+"_bits";
  helper_output+="// dot-gen:member-begin\nprivate static global::System.ReadOnlySpan<"+storage_type+"> "+backing+" => ["+values+"];\n// dot-gen:member-end\n";
  if(backing!=id)
    helper_output+="// dot-gen:member-begin\nprivate static global::System.ReadOnlySpan<"+view_type+"> "+id+
      " => global::System.Runtime.InteropServices.MemoryMarshal.Cast<"+storage_type+", "+view_type+">("+backing+");\n// dot-gen:member-end\n";
  primitive_data_properties[key]=id;return id;
}
bool primitive_array_copy(a_type_ptr array,a_constant_ptr aggregate_value,const string &address,string &result) {
  if(array->kind!=tk_array)return false;
  auto element=skip_typerefs(array->variant.array.element_type);
  bool integer=element->kind==tk_integer && element->size>=2 && element->size<=8;
  bool floating=element->kind==tk_float && (element->size==2 || element->size==4 || element->size==8);
  if(!integer && !floating)return false;
  string view_type=type(element),storage_type=integer?source_enum_physical_type(element):
      element->size==2?"ushort":element->size==4?"uint":"ulong";
  if(integer && storage_type!="short" && storage_type!="ushort" && storage_type!="int" &&
     storage_type!="uint" && storage_type!="long" && storage_type!="ulong")return false;
  if(floating && view_type!="float" && view_type!="double" && view_type!="global::System.Half")return false;
  struct run {string value;unsigned long long count;bool zero;};
  std::vector<run> runs;unsigned long long total=0;
  for(auto item=aggregate_value->variant.aggregate.first_constant;item;item=item->next) {
    auto value=item;unsigned long long count=1;
    if(item->kind==ck_init_repeat){value=item->variant.init_repeat.constant;count=item->variant.init_repeat.count;}
    if((integer && value->kind!=ck_integer) || (floating && value->kind!=ck_float) ||
       skip_typerefs(value->type)->size!=element->size)return false;
    if(total>array->variant.array.variant.number_of_elements ||
       count>array->variant.array.variant.number_of_elements-total)unsupported("primitive array initializer bounds");
    string text;bool zero;
    if(integer) {
      text=source_enum_physical_constant(value);
      a_boolean overflow=FALSE;
      zero=unsigned_value_of_integer_constant(value,&overflow)==0 && !overflow;
    } else {
      auto value_type=skip_typerefs(value->type);
      string bits=fp_to_hex_string(value_type->variant.float_kind,&value->variant.float_value).as_temp_characters();
      if(bits.size()!=element->size*2)return false;
      text="0x"+bits+(element->size==8?"UL":element->size==4?"U":"");
      zero=bits.find_first_not_of('0')==string::npos;
    }
    total+=count;
    if(!runs.empty() && runs.back().value==text)runs.back().count+=count;
    else runs.push_back({text,count,zero});
  }
  // The caller has already zeroed the destination, including omitted elements.
  // Do not embed an enormous all-zero designated/repeated range in metadata.
  const unsigned long long chunk_limit=4096;
  unsigned long long offset=0,chunk_start=0,chunk_count=0;string values;
  auto flush=[&]() {
    if(!chunk_count)return;
    string property=primitive_data_property(storage_type,view_type,values);
    result+=property+".CopyTo(new global::System.Span<"+view_type+">(("+view_type+"*)("+address+")"+
      (chunk_start?" + "+std::to_string(chunk_start):"")+", "+property+".Length));\n";
    values.clear();chunk_count=0;
  };
  for(auto &run_value:runs) {
    if(run_value.zero && run_value.count>=chunk_limit) {flush();offset+=run_value.count;continue;}
    for(unsigned long long index=0;index<run_value.count;++index) {
      if(!chunk_count)chunk_start=offset;
      if(chunk_count)values+=", ";
      values+=run_value.value;++chunk_count;++offset;
      if(chunk_count==chunk_limit)flush();
    }
  }
  flush();return true;
}
