/* Exact retained descriptor fields and audited stock effective type queries. */
static void roslyn_qualifier_fields(std::vector<std::pair<const char*,RoslynSqlValue>> &fields,a_type_qualifier_set qualifiers) {
 fields.push_back({"is_const",static_cast<int>((qualifiers&TQ_CONST)!=0)});fields.push_back({"is_volatile",static_cast<int>((qualifiers&TQ_VOLATILE)!=0)});fields.push_back({"is_restrict",static_cast<int>((qualifiers&TQ_RESTRICT)!=0)});fields.push_back({"is_atomic",static_cast<int>((qualifiers&TQ_C11_ATOMIC)!=0)});fields.push_back({"is_nullable",static_cast<int>((qualifiers&TQ_NULLABLE)!=0)});fields.push_back({"is_nonnull",static_cast<int>((qualifiers&TQ_NONNULL)!=0)});fields.push_back({"is_null_unspecified",static_cast<int>((qualifiers&TQ_NULL_UNSPECIFIED)!=0)});
#if MICROSOFT_EXTENSIONS_ALLOWED
 fields.push_back({"is_unaligned",static_cast<int>((qualifiers&TQ_UNALIGNED)!=0)});
#endif
#if NEAR_AND_FAR_ALLOWED
 fields.push_back({"is_near",static_cast<int>((qualifiers&TQ_NEAR)!=0)});fields.push_back({"is_far",static_cast<int>((qualifiers&TQ_FAR)!=0)});
#endif
#if UPC_EXTENSIONS_ALLOWED
 fields.push_back({"is_shared",static_cast<int>((qualifiers&TQ_UPC_SHARED)!=0)});fields.push_back({"is_upc_strict",static_cast<int>((qualifiers&TQ_UPC_STRICT)!=0)});fields.push_back({"is_upc_relaxed",static_cast<int>((qualifiers&TQ_UPC_RELAXED)!=0)});
#endif
#if NAMED_ADDRESS_SPACES_ALLOWED
 fields.push_back({"address_space",static_cast<int>(named_address_space_from_qualifier_set(qualifiers))});
#endif
}
static Rid roslyn_type(a_type_ptr p) {
 bool fresh;Rid id=DB.claim(p,"type",fresh);if(!fresh)return id;
 int kind=roslyn_kind_type(p->kind);if(!kind)DB.fail("Unmapped EDG type kind");
 bool dependent=roslyn_type_is_dependent(p),incomplete=is_incomplete_type(p)!=FALSE;
 a_targ_size_t effective_size=size_of_type(p);a_targ_alignment effective_alignment=alignment_of_type(p);
 std::vector<std::pair<const char*,RoslynSqlValue>> facts={{"id",id},{"kind_id",kind},{"symbol_id",R(roslyn_type_declaration(p,id))},{"name_id",roslyn_name(p->source_corresp)},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position))},{"size",Q(effective_size)},{"alignment",Q(effective_alignment)},{"descriptor_size",p->size!=effective_size?RoslynSqlValue(Q(p->size)):RoslynSqlValue()},{"descriptor_alignment",p->alignment!=effective_alignment?RoslynSqlValue(Q(p->alignment)):RoslynSqlValue()},{"incomplete",static_cast<int>(incomplete)},{"descriptor_incomplete",p->incomplete!=incomplete?RoslynSqlValue(static_cast<int>(p->incomplete)):RoslynSqlValue()},{"is_dependent",static_cast<int>(dependent)},{"is_atomic",static_cast<int>(is_c11_atomic_qualified_type(p))},{"used_in_exception_or_rtti",static_cast<int>(p->used_in_exception_or_rtti)},{"declared_in_function_prototype",static_cast<int>(p->declared_in_function_prototype)},{"is_tag_redefinition",static_cast<int>(p->is_tag_redefinition)},{"alignment_set_explicitly",static_cast<int>(p->alignment_set_explicitly)},{"is_builtin_va_list",static_cast<int>(p->is_builtin_va_list)},{"is_builtin_va_list_from_cstdarg",static_cast<int>(p->is_builtin_va_list_from_cstdarg)}};
#if GNU_EXTENSIONS_ALLOWED
 facts.push_back({"may_alias",static_cast<int>(p->may_alias)});facts.push_back({"has_gnu_abi_tag_attribute",static_cast<int>(p->has_gnu_abi_tag_attribute)});facts.push_back({"in_gnu_abi_tag_namespace",static_cast<int>(p->in_gnu_abi_tag_namespace)});
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 facts.push_back({"has_microsoft_w64_specifier",static_cast<int>(p->has_microsoft_w64_specifier)});facts.push_back({"is_microsoft_intrinsic",static_cast<int>(p->is_microsoft_intrinsic)});
#endif
#if GENERATE_SOURCE_SEQUENCE_LISTS
 facts.push_back({"autonomous_primary_tag_declaration",static_cast<int>(p->autonomous_primary_tag_decl)});
#endif
#if CFRONT_2_1_OBJECT_CODE_COMPATIBILITY
 facts.push_back({"use_cfront_transitional_nested_type_name_mangling",static_cast<int>(p->use_cfront_transitional_nested_type_name_mangling)});
#endif
#if DO_IL_LOWERING
 facts.push_back({"typeinfo_variable_id",R(roslyn_variable(p->typeinfo_var))});
#endif
 DB.put_fields("type",facts);
 roslyn_attributes(id,p->source_corresp.attributes);
 a_type_ptr effective_type=skip_typerefs(p);
 bool scalable=effective_type->kind==tk_scalable_vector||effective_type->kind==tk_scalable_vector_count||effective_type->kind==tk_riscv_vector;
 bool runtime_size=(effective_type->kind==tk_array&&effective_type->variant.array.is_variable_size_array)||scalable;
 bool no_object_size=effective_type->kind==tk_void||effective_type->kind==tk_routine;
 const char *size_state=dependent?"dependent":incomplete?"absent":runtime_size||no_object_size?"not_applicable":"available";
 const char *alignment_state=dependent?"dependent":incomplete?"absent":no_object_size||scalable?"not_applicable":"available";
 roslyn_fact(id,"size",size_state,dependent?"A placeholder or instantiation-dependent descriptor does not specify a concrete object size.":incomplete?"The retained type is incomplete.":runtime_size?"The effective descriptor has runtime or scalable size; retained dimension facts are available separately.":no_object_size?"The descriptor is not an object type.":nullptr);
 roslyn_fact(id,"alignment",alignment_state,dependent?"A placeholder or instantiation-dependent descriptor does not specify concrete alignment.":incomplete?"The retained type is incomplete.":scalable?"A scalable descriptor does not specify fixed object alignment.":no_object_size?"The descriptor is not an object type.":nullptr);
 bool record=p->kind==tk_class||p->kind==tk_struct||p->kind==tk_union;
 roslyn_fact(id,"layout",!record?"not_applicable":dependent?"dependent":incomplete?"absent":"available",!record?"The descriptor is not a record type.":dependent?"The retained record is instantiation-dependent.":incomplete?"The retained record is incomplete.":nullptr);
 switch(p->kind){
 case tk_void:case tk_nullptr:case tk_reflection:break;
 case tk_integer: DB.put("arithmetic_type",{{"id",id},{"arithmetic_kind_id",K("arithmetic_kind",p->variant.integer.bool_type?"boolean":is_signed_integral_type(p)?"signed_integer":"unsigned_integer")},{"bits",static_cast<int>(p->size*targ_char_bit)},{"signed",static_cast<int>(is_signed_integral_type(p))},{"edg_integer_kind_id",roslyn_kind_edg_integer_kind(p->variant.integer.int_kind)},{"precision",is_bit_precise_kind(p->variant.integer.int_kind)&&p->variant.integer.extra_info?RoslynSqlValue(Q(p->variant.integer.extra_info->bit_width)):RoslynSqlValue()},{"explicitly_signed",static_cast<int>(p->variant.integer.explicitly_signed)},{"affiliated_enum_type_id",R(!p->variant.integer.enum_type?roslyn_type(p->variant.integer.enum_info.affiliated_type):0)},{"is_bool",static_cast<int>(p->variant.integer.bool_type)},{"is_wchar",static_cast<int>(p->variant.integer.wchar_t_type)},{"is_char8",static_cast<int>(p->variant.integer.char8_t_type)},{"is_char16",static_cast<int>(p->variant.integer.char16_t_type)},{"is_char32",static_cast<int>(p->variant.integer.char32_t_type)},{"is_microsoft_sized_integer",static_cast<int>(p->variant.integer.microsoft_sized_int_type)}});break;
#if FIXED_POINT_ALLOWED
 case tk_fixed_point:{auto &fixed=p->variant.fixed_point;DB.put("arithmetic_type",{{"id",id},{"arithmetic_kind_id",K("arithmetic_kind","fixed_point")},{"edg_fixed_precision_id",roslyn_kind_edg_fixed_precision(fixed.precision)},{"bits",static_cast<int>(p->size*targ_char_bit)},{"signed",static_cast<int>(!fixed.is_unsigned)},{"radix",2},{"scale",static_cast<int>(targ_fractional_bits_for_fixed_point[fixed.is_unsigned][fixed.precision][fixed.is_fract_type])},{"is_fract_type",static_cast<int>(fixed.is_fract_type)},{"is_saturating",static_cast<int>(fixed.saturating)}});break;}
#endif
 case tk_float:case tk_imaginary:case tk_complex:DB.put("arithmetic_type",{{"id",id},{"arithmetic_kind_id",K("arithmetic_kind",p->kind==tk_float?"floating":p->kind==tk_complex?"complex":"imaginary")},{"edg_float_kind_id",roslyn_kind_edg_float_kind(p->variant.float_kind)},{"bits",static_cast<int>(p->size*targ_char_bit)}});break;
 case tk_pointer:{auto &t=p->variant.pointer;Rid base=0;int ptr32=0,ptr64=0,sptr=0,uptr=0;
#if MICROSOFT_EXTENSIONS_ALLOWED
 base=roslyn_variable(t.base_variable);ptr32=(t.modifiers&PM_PTR32)!=0;ptr64=(t.modifiers&PM_PTR64)!=0;sptr=(t.modifiers&PM_SPTR)!=0;uptr=(t.modifiers&PM_UPTR)!=0;
#endif
 DB.put("pointer_type",{{"id",id},{"pointee_type_id",roslyn_type(t.type)},{"is_reference",static_cast<int>(t.is_reference)},{"is_rvalue_reference",static_cast<int>(t.is_rvalue_reference)},{"based_variable_id",R(base)},{"is_ptr32",ptr32},{"is_ptr64",ptr64},{"is_sptr",sptr},{"is_uptr",uptr}});break;}
 case tk_array:{auto &array=p->variant.array;Rid bound=roslyn_constant(array.bound_constant),dependent_bound=array.is_template_dependent_size_array?roslyn_constant(array.variant.element_count_constant):0;
 an_expr_node_ptr bound_expr=array.constant_bound_expr_in_local_expr_node_ref?roslyn_local_expression(p,lerk_array_bound):array.bound_constant?array.bound_constant->expr:nullptr;
 an_expr_node_ptr dependent_expr=array.dep_constant_bound_expr_in_local_expr_node_ref?roslyn_local_expression(p,lerk_dep_array_bound):nullptr;
 std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},{"element_type_id",roslyn_type(array.element_type)},{"bound_kind_id",K("bound_kind",array.is_variable_size_array?"variable":array.is_template_dependent_size_array?"dependent":array.bound_is_zero?"zero":incomplete?"incomplete":"constant")},{"bound",array.is_variable_size_array||array.is_template_dependent_size_array?RoslynSqlValue():RoslynSqlValue(Q(array.variant.number_of_elements))},{"bound_constant_id",R(bound)},{"dependent_bound_constant_id",R(dependent_bound)},{"bound_expression_id",R(roslyn_expr(bound_expr,7002))},{"dependent_bound_expression_id",R(roslyn_expr(dependent_expr,7002))},{"vla_dimension_id",R(roslyn_vla_dimension(roslyn_type_vla_dimension(p)))},{"bound_is_zero",static_cast<int>(array.bound_is_zero)},{"is_template_dependent",static_cast<int>(array.is_template_dependent_size_array)},{"is_variable",static_cast<int>(array.is_variable_size_array)},{"is_vla",static_cast<int>(array.is_vla)},{"has_vla_dimension",static_cast<int>(array.has_assoc_vla_dimension)},{"is_static",static_cast<int>(array.is_static)}};
#if UPC_EXTENSIONS_ALLOWED
 values.push_back({"is_threads_dimension",static_cast<int>(array.is_threads_dimension)});
#endif
 roslyn_qualifier_fields(values,static_cast<a_type_qualifier_set>(array.qualifiers));DB.put_fields("array_type",values);break;}
 case tk_routine:{auto x=p->variant.routine.extra_info;Rid convention=0;
#if MICROSOFT_EXTENSIONS_ALLOWED || GNU_X86_ATTRIBUTES_ALLOWED
 convention=roslyn_kind_edg_calling_convention(x->calling_convention);
#endif
 DB.put("routine_type",{{"id",id},{"result_type_id",roslyn_type(p->variant.routine.return_type)},{"this_type_id",R(roslyn_type(x->this_class))},{"is_variadic",static_cast<int>(x->has_ellipsis)},{"has_prototype",static_cast<int>(x->prototyped)},{"calling_convention_id",R(convention)},{"language_linkage_id",roslyn_kind_edg_name_linkage(static_cast<a_name_linkage_kind>(x->routine_name_linkage))},{"is_const",static_cast<int>((x->qualifiers&TQ_CONST)!=0)},{"is_volatile",static_cast<int>((x->qualifiers&TQ_VOLATILE)!=0)},{"ref_qualifier_id",roslyn_kind_edg_ref_qualifier(static_cast<a_ref_qualifier_kind>(x->ref_qualifiers))},{"exception_specification_id",R(roslyn_exception_specification(x->exception_specification))}});int n=0;for(a_param_type_ptr a=x->param_type_list;a;a=a->next)DB.put("routine_type_parameter",{{"owner_id",id},{"ordinal",n++},{"parameter_type_id",roslyn_parameter_type(a)},{"role_id",K("role",roslyn_parameter_role(p,a,x->assoc_routine))},{"base_subobject_id",R(roslyn_base(roslyn_parameter_base(p,a,x->assoc_routine)))}});break;}
 case tk_class:case tk_struct:case tk_union:{auto &r=p->variant.class_struct_union;std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},{"scope_id",R(roslyn_scope(r.extra_info->assoc_scope))},{"definition_state_id",K("definition_state",incomplete?"incomplete":"complete")},{"is_abstract",static_cast<int>(r.abstract)},{"is_empty",static_cast<int>(r.is_empty_class)},{"subobject_partner_id",R(LR(r.extra_info,subobject_partner,roslyn_type))},{"instance_id",R(r.extra_info->assoc_template?roslyn_instance(p,id,r.extra_info->assoc_template,r.extra_info->template_arg_list,r.extra_info->partial_spec_template_arg_list):0)}};
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
 DB.put_fields("record_type",values);roslyn_record_declarations(p,id);Rid layout=DB.fresh("record_layout");DB.put("record_layout",{{"id",layout},{"type_id",id},{"is_complete",static_cast<int>(!incomplete&&!dependent)},{"nonvirtual_size",!incomplete&&!dependent?RoslynSqlValue(Q(r.extra_info->size_without_virtual_base_classes)):RoslynSqlValue()},{"nonvirtual_alignment",!incomplete&&!dependent?RoslynSqlValue(Q(r.extra_info->alignment_without_virtual_base_classes)):RoslynSqlValue()}});int n=0;for(a_field_ptr f=r.field_list;f;f=f->next)DB.put("field_layout",{{"owner_id",layout},{"ordinal",n++},{"field_id",roslyn_field(f,p)},{"offset",!incomplete&&!dependent?RoslynSqlValue(Q(f->offset)):RoslynSqlValue()},{"bit_offset",!incomplete&&!dependent?RoslynSqlValue(Q(f->offset_bit_remainder)):RoslynSqlValue()},{"bit_width",!incomplete&&!dependent?RoslynSqlValue(Q(f->bit_size)):RoslynSqlValue()}});for(a_base_class_ptr b=r.extra_info->base_classes;b;b=b->next)roslyn_base(b);n=0;for(a_base_class_ptr b=r.extra_info->direct_base_classes;b;b=b->next_direct)DB.put("direct_base",{{"owner_id",id},{"ordinal",n++},{"base_type_id",roslyn_type(b->type)},{"access_id",R(b->derivation?roslyn_kind_edg_access(b->derivation->access):0)},{"is_virtual",static_cast<int>(b->is_virtual)},{"subobject_id",roslyn_base(b)}});break;}
 case tk_typeref:{auto &reference=p->variant.typeref;auto supplement=reference.extra_info;std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},{"underlying_type_id",roslyn_type(reference.type)},{"reference_kind_id",roslyn_kind_edg_typeref(reference.kind)},{"decltype_expression_id",R(supplement?roslyn_expr(decltype_arg(p),7002):0)},{"instance_id",R(supplement&&supplement->assoc_template?roslyn_instance(p,id,supplement->assoc_template,supplement->template_arg_list,supplement->orig_template_arg_list):0)}};
 values.push_back({"is_predeclared",static_cast<int>(reference.predeclared)});
 values.push_back({"has_variably_modified_type",static_cast<int>(reference.has_variably_modified_type)});
 values.push_back({"decltype_expression_not_parenthesized",static_cast<int>(reference.decltype_expr_not_parenthesized)});
 values.push_back({"is_dependent_type_operator",static_cast<int>(reference.is_dependent_type_operator)});
 values.push_back({"is_nonreal",static_cast<int>(reference.is_nonreal)});
 values.push_back({"is_dependent",static_cast<int>(reference.is_dependent)});
 values.push_back({"is_prototype_instantiation",static_cast<int>(reference.is_prototype_instantiation)});
 values.push_back({"embeds_source_entries",static_cast<int>(reference.embedded_source_sequence_entries)});
 values.push_back({"added_to_record_name",static_cast<int>(reference.added_to_record_name)});
 values.push_back({"has_typename_prefix",static_cast<int>(reference.has_typename_prefix)});
 values.push_back({"is_global_qualified_name",static_cast<int>(reference.is_global_qualified_name)});
 values.push_back({"is_intrinsic_member",static_cast<int>(reference.is_intrinsic_member)});
#if LOWER_VARIABLE_LENGTH_ARRAYS
 values.push_back({"is_lowered_variably_modified_type",static_cast<int>(reference.is_lowered_variably_modified_type)});
#endif
#if C99_IL_EXTENSIONS_SUPPORTED && LOWER_COMPLEX
 values.push_back({"is_lowered_complex_type",static_cast<int>(reference.is_lowered_complex_type)});
#endif
#if NEAR_AND_FAR_ALLOWED
 values.push_back({"explicit_memory_attribute_made_implicit",static_cast<int>(reference.explicit_memory_attribute_made_implicit)});
#endif
 if(supplement){values.push_back({"proxy_class_id",R(roslyn_type(supplement->proxy_class))});values.push_back({"operator_type_argument_id",R(roslyn_type(supplement->operator_type_arg))});values.push_back({"minimum_template_arguments",static_cast<int>(supplement->min_template_arguments)});
#if DEFAULT_RECORD_FORM_OF_NAME_REFERENCE
 values.push_back({"name_qualifier_id",R(roslyn_qualifier(supplement->name_qualifier))});
#endif
#if EXTRA_SOURCE_POSITIONS_IN_IL
 values.push_back({"type_id_range_id",R(roslyn_range(supplement->type_id_range))});
#endif
 }
 DB.put_fields("type_reference",values);std::vector<std::pair<const char*,RoslynSqlValue>> qualifiers={{"id",id},{"underlying_type_id",roslyn_type(reference.type)}};roslyn_qualifier_fields(qualifiers,static_cast<a_type_qualifier_set>(reference.qualifiers));
#if UPC_EXTENSIONS_ALLOWED
 if(supplement)qualifiers.push_back({"shared_block_size",Q(supplement->upc_block_size)});
#endif
 DB.put_fields("qualified_type",qualifiers);break;}
 case tk_ptr_to_member:{auto &member=p->variant.ptr_to_member;int ptr32=0,ptr64=0;
#if MICROSOFT_EXTENSIONS_ALLOWED
 ptr32=(member.modifiers&PM_PTR32)!=0;ptr64=(member.modifiers&PM_PTR64)!=0;
#endif
 DB.put("member_pointer_type",{{"id",id},{"declaring_type_id",roslyn_type(member.class_of_which_a_member)},{"original_declaring_type_id",R(roslyn_type(member.orig_class_of_which_a_member))},{"member_type_id",roslyn_type(member.type)},{"is_ptr32",ptr32},{"is_ptr64",ptr64}});break;}
 case tk_template_param:{auto &d=p->variant.template_param;auto x=d.extra_info;DB.put("dependent_type",{{"id",id},{"parameter_kind_id",roslyn_kind_template_type(d.kind)},{"parameter_number",x?static_cast<int>(x->coordinates.position):0},{"parameter_depth",x?static_cast<int>(x->coordinates.depth):0},{"parent_type_id",R(x?roslyn_type(x->orig_nested_type):0)},{"constraint_id",R(x&&x->coordinates.depth!=CLASS_TEMPLATE_PLACEHOLDER_NESTING_DEPTH&&x->coordinates.depth!=BIT_PRECISE_INT_NESTING_DEPTH?roslyn_expr(x->constraint.type_constraint,7002):0)},{"bit_width_constant_id",R(x&&x->coordinates.depth==BIT_PRECISE_INT_NESTING_DEPTH?roslyn_constant(x->constraint.bit_width_constant):0)},{"is_pack",static_cast<int>(d.is_pack)},{"is_auto_parameter",static_cast<int>(d.is_auto_param)},{"is_decltype_auto",static_cast<int>(d.is_decltype_auto)},{"originally_class_template_parameter",static_cast<int>(d.originally_class_template_param)},{"is_unsigned_bit_precise_integer",static_cast<int>(d.is_unsigned_bit_precise_int)},{"proxy_class_id",R(x?roslyn_type(x->class_type):0)}});break;}
 case tk_vector:DB.put("vector_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.vector.element_type)},{"vector_kind_id",K("vector_kind","fixed")},{"edg_vector_kind_id",roslyn_kind_edg_vector_kind(p->variant.vector.kind)},{"size_constant_id",R(roslyn_constant(p->variant.vector.size_constant))},{"is_boolean",static_cast<int>(p->variant.vector.is_boolean_vector)}});break;
 case tk_scalable_vector:DB.put("vector_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.scalable_vector.element_type)},{"vector_kind_id",K("vector_kind","scalable")},{"tuple_elements",static_cast<int>(p->variant.scalable_vector.tuple_elements)}});break;
 case tk_riscv_vector:DB.put("vector_type",{{"id",id},{"element_type_id",roslyn_type(p->variant.riscv_vector.element_type)},{"vector_kind_id",K("vector_kind","riscv")},{"tuple_elements",static_cast<int>(p->variant.riscv_vector.tuple_elements)},{"length_multiplier",static_cast<int>(p->variant.riscv_vector.length_multiplier)}});break;
 case tk_scalable_vector_count:case tk_mfp8:case tk_float8e4m3:case tk_float8e5m2:break;
 default:DB.fail("Type payload adapter missing: "+Q(p->kind));break;
 }
 return id;
}
