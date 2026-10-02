// EDG's supported IA64 variant puts the virtual tag in the integer this-delta,
// never in a CLR function address. Lowering owns comparisons, adjustments,
// virtual-table selection and calls; recognize its canonical record by type
// identity and verify the frozen managed layout before admitting its fields.
bool member_pointer_field(a_type_ptr record,a_field_ptr field) {
  if(!is_or_was_ptr_to_member_function_type(record))return false;
  if(!getenv("EDG_DOT_TARGET") || string(getenv("EDG_DOT_TARGET"))!="portable-lp64-binary80-v2")
    unsupported("member-function pointers require portable-lp64-binary80-v2");
#if IA64_ABI
  if(!targ_ia64_abi_use_variant_ptr_to_member_function_repr)
    unsupported("member-function pointers require the managed variant ABI");
  auto first=record->variant.class_struct_union.field_list;
  auto second=first?first->next:nullptr;
  if(record->size!=16 || alignment_of_type(record)!=8 || !first || !second || second->next ||
     first->offset!=0 || second->offset!=8 || first->is_bit_field || second->is_bit_field)
    unsupported("member-function pointer descriptor layout");
  auto address=skip_typerefs(first->type),delta=skip_typerefs(second->type);
  if(address->kind!=tk_pointer || address->size!=8 ||
     skip_typerefs(address->variant.pointer.type)->kind!=tk_routine ||
     delta->kind!=tk_integer || delta->size!=8 || delta->variant.integer.int_kind!=targ_ptrdiff_t_int_kind)
    unsupported("member-function pointer descriptor field types");
  return field==first || field==second;
#else
  unsupported("member-function pointers require IA64 structural lowering");
#endif
}
