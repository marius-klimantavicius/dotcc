/* Retained class, base and field descriptors; target-byte layout quantities. */
static std::map<a_field_ptr,a_type_ptr> roslyn_field_records;
static void roslyn_register_record_fields(a_type_ptr p) {
 if(!p || (p->kind!=tk_class&&p->kind!=tk_struct&&p->kind!=tk_union))return;
 for(a_field_ptr f=p->variant.class_struct_union.field_list;f;f=f->next)roslyn_field_records.emplace(f,p);
}
static bool roslyn_record_has_layout(a_type_ptr p) {
 if(!p || (p->kind!=tk_class&&p->kind!=tk_struct&&p->kind!=tk_union))return false;
 auto &record=p->variant.class_struct_union;
 return !is_incomplete_type(p)&&!roslyn_type_is_dependent(p)&&!record.is_prototype_instantiation&&!record.is_ms_instantiated_nonreal_class;
}
static bool roslyn_resolved_field_width(a_field_ptr p) {
 return p->is_bit_field&&p->bit_size_constant&&p->bit_size_constant->kind==ck_integer;
}
#if DECL_MODIFIERS_IN_USE
static void roslyn_decl_modifiers(Rid owner,a_decl_modifier_set modifiers) {
 for(int n=0;n<static_cast<int>(dmt_last);++n)
  if((modifiers&(static_cast<a_decl_modifier_set>(1)<<n))!=0)
   DB.put("owner_decl_modifier",{{"owner_id",owner},{"kind_id",roslyn_kind_edg_decl_modifier(static_cast<a_decl_modifier>(n))}});
}
#endif
static Rid roslyn_field(a_field_ptr p,a_type_ptr record) {
 bool fresh;Rid id=DB.claim(p,"field",fresh);if(!fresh)return id;
 if(!record){auto known=roslyn_field_records.find(p);if(known!=roslyn_field_records.end())record=known->second;}
 if(!record){a_scope_ptr parent=roslyn_source_scope(p->source_corresp);if(parent&&parent->kind==sck_class_struct_union)record=parent->variant.assoc_type;}
 if(!record){DB.fail("Retained field has no exact containing record association");return id;}
 roslyn_prepare_declaration(id);Rid type=roslyn_type(p->type);roslyn_symbol(id,"field",p->source_corresp,type,p->compiler_generated);
 std::vector<std::pair<const char*,RoslynSqlValue>> facts={{"id",id},{"record_type_id",roslyn_type(record)},{"type_id",type},{"is_bit_field",static_cast<int>(p->is_bit_field)},{"is_mutable",static_cast<int>(p->is_mutable)},{"is_generated_vptr",RoslynSqlValue()},{"default_initialization_id",R(roslyn_init(p->initializer,7002))},{"bit_size_constant_id",R(roslyn_constant(p->bit_size_constant))},{"bit_width_expression_id",R(roslyn_expr(p->bit_size_constant_expr_in_local_expr_node_ref?roslyn_local_expression(p,lerk_bit_field_width):p->bit_size_constant?p->bit_size_constant->expr:nullptr,7002))},{"declared_bit_size",roslyn_resolved_field_width(p)?RoslynSqlValue(Q(p->declared_bit_size)):RoslynSqlValue()},{"explicit_alignment",p->alignment?RoslynSqlValue(Q(p->alignment)):RoslynSqlValue()},{"bit_field_is_signed",p->is_bit_field?RoslynSqlValue(static_cast<int>(p->bit_field_is_signed)):RoslynSqlValue()}};
#define FIELD_FLAG(field) facts.push_back({#field,static_cast<int>(p->field)})
 FIELD_FLAG(is_anonymous_parent_object);FIELD_FLAG(is_init_capture);FIELD_FLAG(is_captured_this);FIELD_FLAG(is_captured_pack_element);FIELD_FLAG(has_initializer);FIELD_FLAG(init_is_ctor_dependent);FIELD_FLAG(has_direct_braced_initializer);FIELD_FLAG(has_nonconstant_initializer);FIELD_FLAG(has_no_unique_address_attribute);FIELD_FLAG(is_optimized_empty_class);FIELD_FLAG(bit_size_constant_expr_in_local_expr_node_ref);
#if GNU_EXTENSIONS_ALLOWED
 FIELD_FLAG(is_packed);FIELD_FLAG(vla_treated_as_zero_length_array);
#endif
#if DO_IL_LOWERING
 FIELD_FLAG(class_subobject_with_tail_padding);facts.push_back({"is_generated_base",static_cast<int>(p->is_lowered_base_class)});
#endif
#undef FIELD_FLAG
 DB.put_fields("field_symbol",facts);
#if MICROSOFT_EXTENSIONS_ALLOWED
 if(p->property_or_event_descr){auto property=p->property_or_event_descr;if(property->kind==pek_declspec_property)DB.put("field_property",{{"id",id},{"get_name_id",R(DB.atom(property->get_routine.name))},{"put_name_id",R(DB.atom(property->set_routine.name))}});}
#endif
 int ordinal=0;for(an_il_entity_list_entry_ptr e=p->entities_defined_in_initializer;e;e=e->next)
  DB.put("field_initializer_entity",{{"owner_id",id},{"ordinal",ordinal++},{"entity_id",roslyn_entity(e->entity)}});
 return id;
}
static Rid roslyn_base(a_base_class_ptr p) {
 bool fresh;Rid id=DB.claim(p,"base_subobject",fresh);if(!fresh)return id;
 bool layout=roslyn_record_has_layout(p->derived_class),has_offset=layout;
#if IA64_ABI
 has_offset=has_offset&&p->offset_is_set;
#endif
 std::vector<std::pair<const char*,RoslynSqlValue>> facts={{"id",id},{"most_derived_type_id",roslyn_type(p->derived_class)},{"base_type_id",roslyn_type(p->type)},{"original_type_id",R(roslyn_type(p->orig_type))},{"offset",has_offset?RoslynSqlValue(Q(p->offset)):RoslynSqlValue()},{"is_virtual",static_cast<int>(p->is_virtual)},{"is_direct",static_cast<int>(p->direct)},{"direct_base_number",static_cast<int>(p->direct_base_number)}};
#define BASE_FLAG(field) facts.push_back({#field,static_cast<int>(p->field)})
 BASE_FLAG(ambiguous);BASE_FLAG(shares_virtual_function_info);BASE_FLAG(ignore_during_dependent_lookup);BASE_FLAG(is_optimized_empty_base);BASE_FLAG(is_pack_expansion);BASE_FLAG(has_public_derivation);
#if CFRONT_OBJECT_CODE_COMPATIBILITY
 BASE_FLAG(complete_subobject);if(p->is_virtual)facts.push_back({"data_section_base_subobject_id",R(roslyn_base(p->data_section_base_class))});
#endif
#undef BASE_FLAG
#if IA64_ABI
 facts.push_back({"primary_base_subobject_id",R(roslyn_base(p->primary_base_class))});
#else
 if(p->is_virtual){bool pointer_set=layout;
#if CFRONT_OBJECT_CODE_COMPATIBILITY
 pointer_set=pointer_set&&p->pointer_offset_is_set;
#endif
 facts.push_back({"virtual_base_pointer_offset",pointer_set?RoslynSqlValue(Q(p->pointer_offset)):RoslynSqlValue()});facts.push_back({"virtual_base_pointer_base_subobject_id",R(roslyn_base(p->pointer_base_class))});}
#endif
#if DO_IL_LOWERING
#if IA64_ABI
 facts.push_back({"virtual_function_table_index",Q(p->virtual_function_table_offset)});
 if(p->is_virtual)facts.push_back({"virtual_base_offset_index",layout?RoslynSqlValue(Q(p->vbase_offset_index)):RoslynSqlValue()});
#else
#if !IL_SHOULD_BE_WRITTEN_TO_FILE && !BACK_END_IS_C_GEN_BE
 facts.push_back({"virtual_function_table_variable_id",R(roslyn_variable(p->virtual_function_table_var))});
#endif
#endif
#if ABI_CHANGES_FOR_CONSTRUCTION_VTBLS
 facts.push_back({"construction_table_index",Q(p->index_in_construction_vtbl_array)});
 if(IA64_ABI||!p->is_virtual)facts.push_back({"construction_subarray_index",Q(p->base_subarray_index_in_construction_vtbl_array)});
#endif
#endif
 DB.put_fields("base_subobject",facts);roslyn_attributes(id,p->attributes);
 int path_number=0;for(a_base_class_derivation_ptr d=p->derivation;d;d=d->next){bool df;Rid deriv=DB.claim(d,"base_derivation",df);if(!df){++path_number;continue;}int count=0;for(a_derivation_step_ptr step=d->path;step;step=step->next){DB.put("base_derivation_step",{{"owner_id",deriv},{"ordinal",count++},{"subobject_id",roslyn_base(step->base_class)}});if(step==d->path_tail)break;}DB.put("base_derivation",{{"id",deriv},{"base_subobject_id",id},{"ordinal",path_number++},{"is_direct",static_cast<int>(d->direct)},{"is_preferred",static_cast<int>(d->preferred)},{"access_id",roslyn_kind_edg_access(static_cast<an_access_specifier>(d->access))},{"path_tail_ordinal",count?RoslynSqlValue(count-1):RoslynSqlValue()}});}
 if(!p->is_pack_expansion){int ordinal=0;for(an_overriding_virtual_function_ptr o=p->variant.overriding_virtual_functions;o;o=o->next){Rid routine=roslyn_routine(o->overriding_function);DB.put("override_relation",{{"owner_id",routine},{"ordinal",roslyn_override_ordinals[routine]++},{"base_ordinal",ordinal++},{"overridden_id",roslyn_routine(o->primary_function)},{"subobject_id",id},{"overriding_subobject_id",R(roslyn_base(o->base_class))},{"return_adjustment_subobject_id",R(roslyn_base(o->return_adjustment_base_class))}});}}
 return id;
}
static void roslyn_record_type(a_type_ptr p,Rid id) {
 bool incomplete=is_incomplete_type(p)!=FALSE,layout=roslyn_record_has_layout(p);
auto &r=p->variant.class_struct_union;auto x=r.extra_info;std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},{"scope_id",R(roslyn_scope(r.extra_info->assoc_scope))},{"definition_state_id",K("definition_state",incomplete?"incomplete":"complete")},{"is_abstract",static_cast<int>(r.abstract)},{"is_empty",static_cast<int>(r.is_empty_class)},{"subobject_partner_id",R(LR(r.extra_info,subobject_partner,roslyn_type))},{"instance_id",R(r.extra_info->assoc_template?roslyn_instance(p,id,r.extra_info->assoc_template,r.extra_info->template_arg_list,r.extra_info->partial_spec_template_arg_list):0)}};
 values.push_back({"is_final",static_cast<int>(r.final)});
 values.push_back({"any_const_member",static_cast<int>(r.any_const_member)});
 values.push_back({"any_volatile_member",static_cast<int>(r.any_volatile_member)});
 values.push_back({"any_mutable_member",static_cast<int>(r.any_mutable_member)});
 values.push_back({"any_virtual_base_classes",static_cast<int>(r.any_virtual_base_classes)});
 values.push_back({"any_virtual_functions",static_cast<int>(r.any_virtual_functions)});
 values.push_back({"any_pure_virtual_functions",static_cast<int>(r.any_pure_virtual_functions)});
 values.push_back({"any_virtual_functions_including_bases",static_cast<int>(r.any_virtual_functions_including_in_base_classes)});
 values.push_back({"nested_class_defined_outside_parent",static_cast<int>(r.nested_class_defined_outside_of_parent)});
 values.push_back({"originally_unnamed",static_cast<int>(r.originally_unnamed)});
 values.push_back({"is_nonstandard_anonymous_union",static_cast<int>(r.is_nonstd_anonymous_union_type)});
 values.push_back({"is_template_class",static_cast<int>(r.is_template_class)});
 values.push_back({"is_nonreal_class",static_cast<int>(r.is_nonreal_class)});
 values.push_back({"is_ms_instantiated_nonreal_class",static_cast<int>(r.is_ms_instantiated_nonreal_class)});
 values.push_back({"is_prototype_instantiation",static_cast<int>(r.is_prototype_instantiation)});
 values.push_back({"is_specialized",static_cast<int>(r.is_specialized)});
 values.push_back({"specialized_with_old_syntax",static_cast<int>(r.specialized_with_old_syntax)});
 values.push_back({"is_in_class_specialization",static_cast<int>(r.is_in_class_specialization)});
 values.push_back({"explicitly_instantiated",static_cast<int>(r.explicitly_instantiated)});
 values.push_back({"do_not_instantiate",static_cast<int>(r.do_not_instantiate)});
 values.push_back({"is_proxy_class",static_cast<int>(r.proxy_class)});
 values.push_back({"no_proper_data",static_cast<int>(r.no_proper_data)});
 values.push_back({"has_zero_init_component",static_cast<int>(r.has_zero_init_component)});
 values.push_back({"has_pointer_component",static_cast<int>(r.has_pointer_component)});
 values.push_back({"contains_flexible_array_member",static_cast<int>(r.contains_flexible_array_member)});
 values.push_back({"has_operator_ampersand",static_cast<int>(r.has_operator_ampersand)});
 values.push_back({"copy_assignment_declaration_suppressed",static_cast<int>(r.copy_assignment_decl_suppressed)});
 values.push_back({"copy_constructor_declaration_suppressed",static_cast<int>(r.copy_ctor_decl_suppressed)});
 values.push_back({"default_constructor_declaration_suppressed",static_cast<int>(r.default_ctor_decl_suppressed)});
 values.push_back({"destructor_declaration_suppressed",static_cast<int>(r.dtor_decl_suppressed)});
 values.push_back({"maximum_member_alignment",Q(r.max_member_alignment)});
#if GNU_EXTENSIONS_ALLOWED
 values.push_back({"is_transparent",static_cast<int>(r.is_transparent)});
 values.push_back({"is_packed",static_cast<int>(r.is_packed)});
 values.push_back({"has_internal_linkage_attribute",static_cast<int>(r.has_internal_linkage_attribute)});
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 values.push_back({"is_interface",static_cast<int>(r.is_interface)});
 values.push_back({"is_interface_like",static_cast<int>(r.is_interface_like)});
 values.push_back({"is_sealed",static_cast<int>(r.sealed)});
#endif

#define RECORD_FLAG(field) values.push_back({#field,static_cast<int>(x->field)})
 values.push_back({"is_lambda",static_cast<int>(x->is_lambda_closure_class)});
 RECORD_FLAG(is_generic_lambda_closure_class);RECORD_FLAG(has_lambda_conversion_function);RECORD_FLAG(is_initializer_list);RECORD_FLAG(has_initializer_list_ctor);RECORD_FLAG(has_anonymous_union_member);RECORD_FLAG(defined_in_variable_initializer);RECORD_FLAG(defined_in_field_initializer);RECORD_FLAG(named_in_inline_template_directive);RECORD_FLAG(is_va_list_tag);RECORD_FLAG(defined_in_parent_class);RECORD_FLAG(has_nodiscard_attribute);RECORD_FLAG(has_field_initializer);
#if DO_IL_LOWERING
 RECORD_FLAG(compiler_generated);RECORD_FLAG(has_subobject_type);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 RECORD_FLAG(inheritance_kind_is_explicit);RECORD_FLAG(has_coclass_attribute);RECORD_FLAG(has_explicitly_aligned_subobject);
 values.push_back({"uuid_string_id",R(DB.atom(x->uuid_string))});values.push_back({"original_tag_kind_id",roslyn_kind_type(x->orig_type_kind)});values.push_back({"inheritance_kind_id",roslyn_kind_edg_inheritance(x->inheritance_kind)});
#endif
#undef RECORD_FLAG
#define RECORD_CACHED(field) values.push_back({#field,x->field||x->field##_cached?RoslynSqlValue(static_cast<int>(x->field)):RoslynSqlValue()})
 RECORD_CACHED(contains_error);RECORD_CACHED(contains_local_type);RECORD_CACHED(contains_unnamed_namespace_type);
#undef RECORD_CACHED
 values.push_back({"does_not_contain_parentless_lambda_in_default_argument",x->does_not_contain_parentless_lambda_in_default_argument?RoslynSqlValue(1):RoslynSqlValue()});values.push_back({"does_not_contain_deprecated_or_unavailable_type",x->does_not_contain_deprecated_or_unavailable_type?RoslynSqlValue(1):RoslynSqlValue()});
 values.push_back({"virtual_function_info_base_subobject_id",R(roslyn_base(x->virtual_function_info_base_class))});values.push_back({"virtual_function_info_offset",layout&&r.any_virtual_functions?RoslynSqlValue(Q(x->virtual_function_info_offset)):RoslynSqlValue()});values.push_back({"highest_virtual_function_number",Q(x->highest_virtual_function_number)});
 values.push_back({"anonymous_union_kind_id",roslyn_kind_edg_anonymous_union(x->anonymous_union_kind)});values.push_back({"anonymous_union_field_id",R(x->anonymous_union_kind==auk_field?roslyn_field(x->anonymous_union_field):0)});values.push_back({"minimum_template_arguments",x->min_template_arguments});values.push_back({"proxy_of_type_id",R(roslyn_type(x->proxy_of_type))});
 Rid lambda_parent=0;if(x->is_lambda_closure_class){if(x->defined_in_variable_initializer)lambda_parent=roslyn_variable(x->lambda_parent.variable);else if(x->defined_in_field_initializer)lambda_parent=roslyn_field(x->lambda_parent.field);else lambda_parent=roslyn_routine(x->lambda_parent.routine);}values.push_back({"lambda_parent_entity_id",R(lambda_parent)});
#if MAINTAIN_NEEDED_FLAGS
 values.push_back({"definition_needed",static_cast<int>(r.definition_needed)});
#endif
#if GNU_EXTENSIONS_ALLOWED && GNU_VISIBILITY_ATTRIBUTE_ALLOWED
 values.push_back({"elf_visibility_id",roslyn_kind_edg_elf_visibility(static_cast<an_ELF_visibility_kind>(x->ELF_visibility))});
#endif
#if NEAR_AND_FAR_ALLOWED
 values.push_back({"is_near",static_cast<int>((x->qualifiers&TQ_NEAR)!=0)});values.push_back({"is_far",static_cast<int>((x->qualifiers&TQ_FAR)!=0)});
#endif
#if IA64_ABI
 values.push_back({"primary_base_subobject_id",R(roslyn_base(x->primary_base_class))});
#endif
#if DO_IL_LOWERING && !IL_SHOULD_BE_WRITTEN_TO_FILE && !BACK_END_IS_C_GEN_BE
 values.push_back({"virtual_function_table_variable_id",R(roslyn_variable(x->virtual_function_table_var))});
#if IA64_ABI
 values.push_back({"virtual_table_table_variable_id",R(roslyn_variable(x->virtual_table_table_var))});
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 values.push_back({"uuid_variable_id",R(roslyn_variable(x->uuid_variable))});
#endif
#endif
#if DO_IL_LOWERING && IA64_ABI
 values.push_back({"next_negative_virtual_table_index",layout&&x->virtual_function_table_var?RoslynSqlValue(Q(x->next_negative_virtual_table_index)):RoslynSqlValue()});values.push_back({"first_vcall_offset_index",x->first_vcall_offset_index?RoslynSqlValue(Q(x->first_vcall_offset_index)):RoslynSqlValue()});
 int vcall_ordinal=0;for(a_vcall_offset_entry_ptr e=x->vcall_offsets;e;e=e->next)DB.put("record_vcall_offset",{{"owner_id",id},{"ordinal",vcall_ordinal++},{"routine_id",roslyn_routine(e->routine)},{"base_subobject_id",R(roslyn_base(e->base_class))},{"vtable_index",Q(e->vcall_offset_index)},{"is_primary",static_cast<int>(e->is_primary)}});
#endif
#if NEW_CAN_BE_FOLDED_INTO_CTOR
 values.push_back({"allocation_routine_id",R(roslyn_routine(x->assoc_operator_new_routine))});
#endif
#if DELETE_CAN_BE_FOLDED_INTO_DTOR
 values.push_back({"deallocation_routine_id",R(roslyn_routine(x->assoc_operator_delete_routine))});
#endif
 DB.put_fields("record_type",values);roslyn_record_declarations(p,id);
#if DECL_MODIFIERS_IN_USE
 roslyn_decl_modifiers(id,x->decl_modifiers);
#endif
 bool nonvirtual_alignment=layout&&!C_mode();
#if DO_IL_LOWERING
 nonvirtual_alignment=nonvirtual_alignment&&!x->compiler_generated;
#endif
 Rid layout_id=DB.fresh("record_layout");DB.put("record_layout",{{"id",layout_id},{"type_id",id},{"is_complete",static_cast<int>(layout)},{"nonvirtual_size",layout&&!C_mode()?RoslynSqlValue(Q(x->size_without_virtual_base_classes)):RoslynSqlValue()},{"nonvirtual_alignment",nonvirtual_alignment?RoslynSqlValue(Q(x->alignment_without_virtual_base_classes)):RoslynSqlValue()}});
 int ordinal=0;for(a_field_ptr f=r.field_list;f;f=f->next){bool offset=layout;
#if IA64_ABI
 offset=offset&&f->offset_is_set;
#endif
 bool width=layout&&roslyn_resolved_field_width(f)&&!roslyn_type_is_dependent(f->type)&&skip_typerefs(f->type)->kind==tk_integer;
 std::vector<std::pair<const char*,RoslynSqlValue>> field={{"owner_id",layout_id},{"ordinal",ordinal++},{"field_id",roslyn_field(f,p)},{"offset",offset?RoslynSqlValue(Q(f->offset)):RoslynSqlValue()},{"bit_offset",offset?RoslynSqlValue(Q(f->offset_bit_remainder)):RoslynSqlValue()},{"bit_width",width?RoslynSqlValue(Q(f->bit_size)):RoslynSqlValue()}};
#if RECORD_BIT_FIELD_CONTAINER_OFFSETS_IN_IL
 field.push_back({"container_offset",offset&&targ_microsoft_bit_field_allocation&&f->is_bit_field?RoslynSqlValue(Q(f->offset_in_container)):RoslynSqlValue()});
#endif
 DB.put_fields("field_layout",field);}
 ordinal=0;for(a_base_class_ptr b=x->base_classes;b;b=b->next)DB.put("record_base_postorder",{{"owner_id",id},{"ordinal",ordinal++},{"target_id",roslyn_base(b)}});
#if IA64_ABI
 ordinal=0;for(a_base_class_ptr b=x->preorder_base_classes;b;b=b->next_preorder)DB.put("record_base_preorder",{{"owner_id",id},{"ordinal",ordinal++},{"target_id",roslyn_base(b)}});
#endif
 ordinal=0;for(a_base_class_ptr b=x->direct_base_classes;b;b=b->next_direct)DB.put("direct_base",{{"owner_id",id},{"ordinal",ordinal++},{"base_type_id",roslyn_type(b->type)},{"access_id",R(b->derivation?roslyn_kind_edg_access(static_cast<an_access_specifier>(b->derivation->access)):0)},{"is_virtual",static_cast<int>(b->is_virtual)},{"subobject_id",roslyn_base(b)}});
 ordinal=0;for(a_class_list_entry_ptr e=x->befriending_classes;e;e=e->next)DB.put("record_befriending_class",{{"owner_id",id},{"ordinal",ordinal++},{"target_id",roslyn_type(e->class_type)}});
 ordinal=0;for(an_il_entity_list_entry_ptr e=x->friends;e;e=e->next)DB.put("record_friend",{{"owner_id",id},{"ordinal",ordinal++},{"target_id",roslyn_entity(e->entity)}});
#if DO_IL_LOWERING && PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE
 ordinal=0;for(a_type_ptr t=x->promoted_local_types;t;t=t->next)DB.put("record_promoted_type",{{"owner_id",id},{"ordinal",ordinal++},{"target_id",roslyn_type(t)}});
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 ordinal=0;for(an_event_interface_ptr e=x->event_interfaces;e;e=e->next)DB.put("record_event_interface",{{"owner_id",id},{"ordinal",ordinal++},{"target_id",roslyn_type(e->interface_type)},{"source_origin_id",R(roslyn_origin(e->pos))}});
#endif
}
