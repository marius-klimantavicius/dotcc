// Compatible EDG array types can be distinct IL objects. C# array-storage
// structs need one nominal identity in every use site (including returns and
// conditional expressions), rather than casts inserted only at calls.
std::map<a_type_ptr,unsigned> storage_record_ids;
std::map<string,string> array_storage_types;
string storage_type_key(a_type_ptr source) {
  auto t=skip_typerefs(source);
  switch(t->kind) {
    case tk_class:case tk_struct:case tk_union: {
      auto found=storage_record_ids.find(t);
      if(found==storage_record_ids.end())found=storage_record_ids.emplace(t,(unsigned)storage_record_ids.size()+1).first;
      return "record:"+std::to_string(found->second);
    }
    case tk_pointer:return "pointer("+storage_type_key(t->variant.pointer.type)+")";
    case tk_array:
      if(t->variant.array.is_variable_size_array || t->variant.array.is_vla || t->variant.array.is_template_dependent_size_array)
        unsupported("dynamic array storage identity");
      return "array:"+std::to_string(t->variant.array.variant.number_of_elements)+"("+storage_type_key(t->variant.array.element_type)+")";
    case tk_routine: {
      string key="function("+storage_type_key(t->variant.routine.return_type);
      for(auto p=t->variant.routine.extra_info->param_type_list;p;p=p->next)key+=","+storage_type_key(p->type);
      return key+(t->variant.routine.extra_info->has_ellipsis?",...)":")");
    }
    default:return type(t);
  }
}
