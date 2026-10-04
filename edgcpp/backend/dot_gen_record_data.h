// C's aggregate initializer order is defined by EDG's lowered constants and
// next_non_empty_initializable_field, as in c_gen_be::dump_initializer_part.
// Use C# value-type initializers only where every field has an ordinary
// managed representation. Struct fields must not overlap; a union initializer
// selects exactly one member. Other layouts retain direct stores.
bool source_plain_record(a_type_ptr original) {
  auto t=skip_typerefs(original);
  if((t->kind!=tk_struct && t->kind!=tk_class && t->kind!=tk_union) || !t->size)return false;
  bool union_type=t->kind==tk_union;
  unsigned long long end=0;
  for(auto field=t->variant.class_struct_union.field_list;field;field=field->next) {
    auto ft=skip_typerefs(field->type);
    if(field->is_bit_field || field->is_lowered_base_class || field->compiler_generated ||
       ft->kind==tk_array || !ft->size || (!union_type && field->offset<end))return false;
    if((ft->kind==tk_struct || ft->kind==tk_class || ft->kind==tk_union) && !source_plain_record(ft))return false;
    if(ft->kind!=tk_integer && ft->kind!=tk_float && ft->kind!=tk_pointer &&
       ft->kind!=tk_struct && ft->kind!=tk_class && ft->kind!=tk_union)return false;
    if(field->offset>t->size || ft->size>t->size-field->offset)return false;
    end=field->offset+ft->size;
  }
  return true;
}
string source_record_scalar(a_type_ptr target,a_constant_ptr value) {
  auto t=skip_typerefs(target),vt=skip_typerefs(value->type);
  // C# permits int constant conversion to narrow integer fields only when the
  // value fits. Keep named enums and all other explicit conversions intact.
  if(t->kind==tk_integer && !t->variant.integer.enum_type &&
     vt->kind==tk_integer && !vt->variant.integer.enum_type && value->kind==ck_integer) {
    string ty=type(t);long long low=0,high=0;
    if(ty=="sbyte"){low=-128;high=127;}
    else if(ty=="byte"){low=0;high=255;}
    else if(ty=="short"){low=-32768;high=32767;}
    else if(ty=="ushort"){low=0;high=65535;}
    else return enum_boundary_value(target,value->type,constant(value));
    a_boolean overflow=FALSE;
    auto number=value_of_integer_constant(value,&overflow);
    if(!overflow && number>=low && number<=high)return std::to_string((long long)number);
  }
  return enum_boundary_value(target,value->type,constant(value));
}
string source_record_initializer(a_type_ptr original,a_constant_ptr value,const string &indent="") {
  auto t=skip_typerefs(original);
  if(value->kind!=ck_aggregate || !source_plain_record(t))return "";
  string result="new()\n"+indent+"{\n";
  auto field=next_non_empty_initializable_field(t->variant.class_struct_union.field_list);
  for(auto item=value->variant.aggregate.first_constant;item;item=item->next) {
    if(item->kind==ck_designator) {
      // With LOWER_DESIGNATED_INITIALIZERS, the C backend retains designators
      // only to choose a union member. The following constant supplies its
      // value. Do not infer the member from constant type or overlapping offset.
      if(t->kind!=tk_union || item->variant.designator.is_generic ||
         !item->variant.designator.is_field_designator)return "";
      auto selected=item->variant.designator.variant.field;
      bool found=false;
      for(auto candidate=t->variant.class_struct_union.field_list;candidate;candidate=candidate->next)
        if(candidate==selected){found=true;break;}
      if(!found || !item->next || item->next->kind==ck_designator)return "";
      field=selected;item=item->next;
    }
    if(!field || item->kind==ck_init_repeat)return "";
    // A single value-type construction must never overwrite another union
    // member selected earlier in the initializer. Preserve the store fallback
    // for any extended/multiple-designator representation.
    if(t->kind==tk_union && item->next)return "";
    auto ft=skip_typerefs(field->type);
    string expression;
    if(ft->kind==tk_struct || ft->kind==tk_class || ft->kind==tk_union) {
      expression=source_record_initializer(ft,item,indent+"    ");
      if(expression.empty())return "";
    } else {
      if(item->kind==ck_aggregate || item->kind==ck_string)return "";
      expression=source_record_scalar(field->type,item);
    }
    result+=indent+"    "+name(field,"m")+" = "+expression+",\n";
    field=next_non_empty_initializable_field(field->next);
  }
  return result+indent+"}";
}
string source_global_initializer(a_variable_ptr variable_ptr,a_constant_ptr value) {
  auto t=skip_typerefs(variable_ptr->type);
  // Name the final aggregate storage once, including structs that contain
  // tables. Keep array element pointers so pointer-cell stores avoid .Value;
  // records use a pointer to the original record and named member accesses.
  if(!aggregate(t) || !t->size ||
     (value->kind!=ck_aggregate && value->kind!=ck_string))
    return initializer(variable(variable_ptr),variable_ptr->type,value);
  bool array=t->kind==tk_array;
  auto pointed_type=array?t->variant.array.element_type:variable_ptr->type;
  string domain=emitting_tls_initializer?"tls-initializer-locals":"global-initializer-locals";
  auto &reserved=source_name_used[domain];
  // Constructor startup catches and TLS allocation already use these locals.
  reserved.insert("error");reserved.insert("p");reserved.insert("storage");
  string alias=source_allocate(domain,source_identifier_component(name(variable_ptr,"v")),"table");
  auto &offsets=emitting_tls_initializer?tls_offsets:global_offsets;
  auto allocated=offsets.find(variable_ptr);
  if(allocated==offsets.end())unsupported("initializer aggregate without allocated storage");
  string address=string(emitting_tls_initializer?"p":"__edg_base")+" + "+std::to_string(allocated->second);
  // Aliases never create copies. Relocations still use final global/TLS storage.
  initializer_chunk_scope chunks(type(pointed_type)+"* "+alias+
      (emitting_tls_initializer?", byte* p":""),
      alias+(emitting_tls_initializer?", p":""),true);
  return "var "+alias+" = ("+type(pointed_type)+"*)("+address+");\n"+
    (array?initializer(variable(variable_ptr),variable_ptr->type,value,alias,alias):
           initializer("*"+alias,variable_ptr->type,value,alias,"",alias));
}
