/* Current ABI signature roles, from retained type/parameter discriminants. */
static a_base_class_ptr roslyn_parameter_base(a_type_ptr type,a_param_type_ptr parameter,a_routine_ptr routine) {
#if DO_IL_LOWERING && !IA64_ABI
 if(type&&parameter&&routine&&routine->special_kind==sfk_constructor){type=skip_typerefs(type);auto x=type->variant.routine.extra_info;if(visited_yet(type)&&x->this_class){auto argument=x->param_type_list;if(argument)argument=argument->next;if(x->value_returned_as_parameter&&argument)argument=argument->next;for(a_base_class_ptr b=x->this_class->variant.class_struct_union.extra_info->base_classes;b;b=b->next)if(b->is_virtual){if(argument==parameter)return b;if(argument)argument=argument->next;}}}
#else
 (void)type;(void)parameter;(void)routine;
#endif
 return nullptr;
}
static const char *roslyn_parameter_role(a_type_ptr type,a_param_type_ptr parameter,a_routine_ptr routine=nullptr) {
 if(!type||!parameter)return "argument";
 type=skip_typerefs(type);if(type->kind!=tk_routine)return "argument";
 auto x=type->variant.routine.extra_info;
#if DO_IL_LOWERING
 if(visited_yet(type)) {
  a_param_type_ptr receiver=nullptr,returned=nullptr;
  if(x->this_class){receiver=x->param_type_list;if(x->value_returned_as_parameter&&!x->return_value_parameter_follows_this&&receiver)receiver=receiver->next;}
  if(x->value_returned_as_parameter){returned=x->param_type_list;if(x->return_value_parameter_follows_this&&returned)returned=returned->next;}
  if(parameter==receiver)return "this_parameter";
  if(parameter==returned)return "return_parameter";
#if !IA64_ABI
  if(roslyn_parameter_base(type,parameter,routine))return "virtual_base_parameter";
  if(routine&&routine->special_kind==sfk_destructor){auto control=x->param_type_list;while(control&&(control==receiver||control==returned))control=control->next;if(parameter==control)return "destructor_control_parameter";}
#endif
#if IA64_ABI
  if(routine&&x->this_class&&(routine->special_kind==sfk_constructor||routine->special_kind==sfk_destructor)&&
     (routine->ctor_dtor_kind==cdk_subobject||routine->ctor_dtor_kind==cdk_delegation)&&
     x->this_class->variant.class_struct_union.any_virtual_base_classes){
   a_param_type_ptr vtt=x->param_type_list;
   while(vtt&&(vtt==receiver||vtt==returned))vtt=vtt->next;
   if(parameter==vtt)return "vtt_parameter";
  }
#endif
 }
#endif
 return parameter->param_num?"ordinary_parameter":"abi_parameter";
}
static a_type_ptr roslyn_call_type(an_expr_node_ptr callee) {
 if(!callee||!callee->type)return nullptr;
 a_type_ptr t=skip_typerefs(callee->type);
 if(t->kind==tk_pointer)t=skip_typerefs(t->variant.pointer.type);
 if(t->kind==tk_ptr_to_member)t=skip_typerefs(t->variant.ptr_to_member.type);
 return t->kind==tk_routine?t:nullptr;
}
static Rid roslyn_parameter_type(a_param_type_ptr p) {
 bool fresh;Rid id=DB.claim(p,"parameter_type",fresh);if(!fresh)return id;
 int transparent=0,nonnull=0,cli_array=0,unaligned=0,near_flag=0,far_flag=0,upc_shared=0,upc_strict=0,upc_relaxed=0;
 RoslynSqlValue address_space;
#if GNU_EXTENSIONS_ALLOWED
 transparent=p->is_transparent;nonnull=p->nonnull;
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 cli_array=p->is_cli_param_array;unaligned=(p->qualifiers&TQ_UNALIGNED)!=0;
#endif
#if NEAR_AND_FAR_ALLOWED
 near_flag=(p->qualifiers&TQ_NEAR)!=0;far_flag=(p->qualifiers&TQ_FAR)!=0;
#endif
#if UPC_EXTENSIONS_ALLOWED
 upc_shared=(p->qualifiers&TQ_UPC_SHARED)!=0;upc_strict=(p->qualifiers&TQ_UPC_STRICT)!=0;upc_relaxed=(p->qualifiers&TQ_UPC_RELAXED)!=0;
#endif
#if NAMED_ADDRESS_SPACES_ALLOWED
 address_space=static_cast<int>(named_address_space_from_qualifier_set(p->qualifiers));
#endif
 DB.put("parameter_type",{{"id",id},{"type_id",roslyn_type(p->type)},{"declared_type_id",R(roslyn_type(p->declared_type))},
  {"name_id",R(DB.atom(p->name))},{"parameter_number",static_cast<sqlite3_int64>(p->param_num)},
  {"default_expression_id",R(roslyn_expr(p->default_arg_expr,7002))},{"has_name_conflict",static_cast<int>(p->has_name_conflict)},
  {"passed_via_copy_constructor",static_cast<int>(p->passed_via_copy_constructor)},
  {"has_default_argument",static_cast<int>(p->has_default_arg)},{"default_argument_in_class",static_cast<int>(p->default_arg_appeared_in_class_definition)},
  {"has_unevaluated_template_default",static_cast<int>(p->has_unevaluated_template_default)},
  {"type_involves_deduced_template_parameter",static_cast<int>(p->type_involves_deduced_template_param)},
  {"type_involves_template_parameter",static_cast<int>(p->type_involves_template_param)},
  {"is_parameter_pack",static_cast<int>(p->is_parameter_pack)},{"is_pack_element",static_cast<int>(p->is_pack_element)},
  {"was_nontrailing_pack",static_cast<int>(p->was_nontrailing_pack)},{"is_auto_parameter",static_cast<int>(p->is_auto_param)},
  {"duplicate_name",static_cast<int>(p->duplicate_name)},{"move_constructor_or_assignment_parameter",static_cast<int>(p->move_ctor_or_assign_parameter)},
  {"copy_or_move_constructor_parameter",static_cast<int>(p->copy_or_move_ctor_parameter)},
  {"is_requires_expression_parameter",static_cast<int>(p->is_requires_expr_param)},{"is_explicit_this",static_cast<int>(p->is_explicit_this)},
  {"is_transparent",transparent},{"is_nonnull",nonnull},{"is_cli_parameter_array",cli_array},
  {"is_const",static_cast<int>((p->qualifiers&TQ_CONST)!=0)},{"is_volatile",static_cast<int>((p->qualifiers&TQ_VOLATILE)!=0)},
  {"is_restrict",static_cast<int>((p->qualifiers&TQ_RESTRICT)!=0)},{"is_atomic",static_cast<int>((p->qualifiers&TQ_C11_ATOMIC)!=0)},
  {"is_nullable",static_cast<int>((p->qualifiers&TQ_NULLABLE)!=0)},{"is_nonnull_qualified",static_cast<int>((p->qualifiers&TQ_NONNULL)!=0)},
  {"is_null_unspecified",static_cast<int>((p->qualifiers&TQ_NULL_UNSPECIFIED)!=0)},
  {"is_unaligned",unaligned},{"is_near",near_flag},{"is_far",far_flag},{"is_upc_shared",upc_shared},
  {"is_upc_strict",upc_strict},{"is_upc_relaxed",upc_relaxed},{"named_address_space",address_space}});
 int ordinal=0;for(an_il_entity_list_entry_ptr e=p->entities_defined_in_default_arg;e;e=e->next)
  DB.put("parameter_default_entity",{{"owner_id",id},{"ordinal",ordinal++},{"entity_id",roslyn_entity(e->entity)}});
 roslyn_attributes(id,p->attributes);return id;
}
