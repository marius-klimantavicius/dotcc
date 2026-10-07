// Equality for the emitted representation of lowered static initializer values.
// Do not call compare_constants here: its address case follows frontend-only
// trans_unit_corresp links even with CC_STRICTLY_IDENTICAL. fe_wrapup frees the
// frontend region before invoking the backend (il_def.h documents that lifetime).
// c_gen_be.c::dump_initializer_part consumes explicit ck_init_repeat entries
// without making frontend correspondence queries. Our additional compression
// must use only surviving lowered types, values and relocation entity identity.
bool same_lowered_initializer(a_constant_ptr left,a_constant_ptr right) {
  if(!left || !right)return left==right;
  if(left->kind!=right->kind || left->type!=right->type)return false;
  switch(left->kind) {
    case ck_integer:
      return cmp_integer_constants(left,right)==0;
    case ck_float: {
      if(!left->type)return false;
      auto t=skip_typerefs(left->type);
      if(t->kind!=tk_float)return false;
      // Unlike arithmetic equality, preserve negative zero and NaN payloads.
      return fp_same_representation(t->variant.float_kind,&left->variant.float_value,
                                    &right->variant.float_value);
    }
    case ck_string: {
      auto &a=left->variant.string;auto &b=right->variant.string;
      if(a.length!=b.length)return false;
#if DO_IL_LOWERING && ASSIGN_STRING_LITERAL_SEQUENCE_NUMBERS
      if(a.sequence_number!=b.sequence_number)return false;
#endif
      for(a_targ_size_t i=0;i<a.length;++i)if(a.value[i]!=b.value[i])return false;
      return true;
    }
    case ck_address: {
      auto &a=left->variant.address;auto &b=right->variant.address;
      if(a.kind!=b.kind || a.offset!=b.offset ||
         a.is_object_reflection!=b.is_object_reflection ||
         a.one_past_the_end!=b.one_past_the_end)return false;
      switch(a.kind) {
        case abk_variable:return a.variant.variable==b.variant.variable;
        case abk_routine:return a.variant.routine==b.variant.routine;
        // Equal contents do not establish the identity of addressed storage.
        case abk_constant:return a.variant.constant==b.variant.constant;
        default:return false;
      }
    }
    case ck_aggregate: {
      if(left->variant.aggregate.has_dynamic_init_component ||
         right->variant.aggregate.has_dynamic_init_component)return false;
      auto a=left->variant.aggregate.first_constant;
      auto b=right->variant.aggregate.first_constant;
      for(;a && b;a=a->next,b=b->next)if(!same_lowered_initializer(a,b))return false;
      return !a && !b;
    }
    case ck_designator: {
      auto &a=left->variant.designator;auto &b=right->variant.designator;
      if(a.is_generic || b.is_generic || a.is_field_designator!=b.is_field_designator)return false;
      return a.is_field_designator?a.variant.field==b.variant.field:
                                  a.variant.array_element==b.variant.array_element;
    }
    case ck_init_repeat: {
      auto &a=left->variant.init_repeat;auto &b=right->variant.init_repeat;
      return a.count==b.count &&
             a.multidimensional_aggr_tail_not_repeated==b.multidimensional_aggr_tail_not_repeated &&
             same_lowered_initializer(a.constant,b.constant);
    }
    // Unlowered/dynamic/unsupported values must retain their individual entries.
    default:return false;
  }
}
