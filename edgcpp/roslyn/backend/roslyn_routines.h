/* Retained routines and current signatures. Payload identity and union
   selection follow resident IL. No frontend fixups, original-type storage,
   symbol lookup, constraint solving or mangler bookkeeping is consulted. */
static Rid roslyn_requires_clause(a_requires_clause_ptr p) {
 bool fresh;Rid id=DB.claim(p,"requires_clause",fresh);if(!fresh)return id;
 DB.put("requires_clause",{{"id",id},{"constraint_expression_id",roslyn_expr(p->constraint,7002)},
  {"requires_position_id",R(roslyn_position(p->requires_pos))}});return id;
}

#if GNU_EXTENSIONS_ALLOWED || REDEFINE_EXTNAME_PRAGMA_ENABLED
static Rid roslyn_gnu_routine_supplement(a_gnu_routine_supplement_ptr p,a_routine_ptr owner) {
 bool fresh;Rid id=DB.claim(p,"gnu_routine_supplement",fresh);if(!fresh)return id;
 std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},
  {"section_id",R(DB.atom(p->section))},{"assembly_name_id",R(DB.atom(p->asm_name))},
  {"aliased_routine_id",R(roslyn_routine(p->aliased_routine))},
  {"inline_partner_id",R(roslyn_routine(p->inline_partner))}};
#if LOWER_IFUNC
 values.push_back({"resolver_variable_id",R(roslyn_variable(p->resolver_var))});
#endif
#if GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED
 if(owner->has_ctor_priority)values.push_back({"constructor_priority",Q(p->ctor_priority)});
 if(owner->has_dtor_priority)values.push_back({"destructor_priority",Q(p->dtor_priority)});
#endif
#if GNU_FUNCTION_MULTIVERSIONING
 values.push_back({"is_representative",static_cast<int>(p->is_representative)});
 values.push_back({"is_target_specific_version",static_cast<int>(p->is_target_specific_version)});
#if USE_X86_FUNCTION_MULTIVERSIONING
 values.push_back({"resolver_required",static_cast<int>(p->mv_resolver_required)});
#endif
 if(p->is_representative) {
  int ordinal=0;
  for(a_routine_list_entry_ptr version=p->mv_info.representative.targeted_versions;version;version=version->next)
   DB.put("routine_multiversion",{{"owner_id",id},{"ordinal",ordinal++},{"routine_id",roslyn_routine(version->routine)}});
 } else if(p->is_target_specific_version) {
  values.push_back({"representative_id",R(roslyn_routine(p->mv_info.targeted_version.representative))});
#if USE_X86_FUNCTION_MULTIVERSIONING
  a_mv_target_bitset bits=p->mv_info.targeted_version.target_bitset;
  for(int bit=0;bit<static_cast<int>(mvak_last);++bit)
   if((bits&(static_cast<a_mv_target_bitset>(1)<<bit))!=0)
    DB.put("routine_target_architecture",{{"owner_id",id},{"kind_id",roslyn_kind_edg_multiversion_arch(static_cast<a_multiversion_arch_kind>(bit))}});
#endif
 }
#endif
 DB.put_fields("gnu_routine_supplement",values);return id;
}
#endif

static void roslyn_routine_type(a_type_ptr p,Rid id) {
 auto x=p->variant.routine.extra_info;
 std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},
  {"result_type_id",roslyn_type(p->variant.routine.return_type)},
  {"this_type_id",R(roslyn_type(x->this_class))},
  {"language_linkage_id",roslyn_kind_edg_name_linkage(static_cast<a_name_linkage_kind>(x->routine_name_linkage))},
  {"ref_qualifier_id",roslyn_kind_edg_ref_qualifier(static_cast<a_ref_qualifier_kind>(x->ref_qualifiers))},
  {"exception_specification_id",R(roslyn_exception_specification(x->exception_specification))},
  {"associated_routine_id",R(roslyn_routine(x->assoc_routine))},
  {"prototype_scope_id",R(roslyn_scope(x->prototype_scope))},
  {"lint_varargs_count",static_cast<int>(x->lint_varargs_count)},
  {"argument_pragma_id",roslyn_kind_edg_pragma(x->arg_pragma)}};
#if MICROSOFT_EXTENSIONS_ALLOWED || GNU_X86_ATTRIBUTES_ALLOWED
 values.push_back({"calling_convention_id",roslyn_kind_edg_calling_convention(x->calling_convention)});
#endif
#if GNU_EXTENSIONS_ALLOWED
 values.push_back({"format_argument",x->fmt_arg});
 values.push_back({"first_format_substitution_argument",x->format_first_subst_arg});
 values.push_back({"sentinel_position",x->sentinel_pos});
#endif
 values.push_back({"is_variadic",static_cast<int>(x->has_ellipsis)});
 values.push_back({"has_prototype",static_cast<int>(x->prototyped)});
 values.push_back({"old_style_params_scanned",static_cast<int>(x->old_style_params_scanned)});
 values.push_back({"has_trailing_return_type",static_cast<int>(x->trailing_return_type)});
 values.push_back({"lint_argsused_flag",static_cast<int>(x->lint_argsused_flag)});
 values.push_back({"value_returned_by_cctor",static_cast<int>(x->value_returned_by_cctor)});
#if (DO_IL_LOWERING)
 values.push_back({"value_returned_as_parameter",static_cast<int>(x->value_returned_as_parameter)});
#endif
#if (DO_IL_LOWERING)
 values.push_back({"return_value_parameter_follows_this",static_cast<int>(x->return_value_parameter_follows_this)});
#endif
 values.push_back({"assoc_routine_is_ctor",static_cast<int>(x->assoc_routine_is_ctor)});
 values.push_back({"assoc_routine_is_dtor",static_cast<int>(x->assoc_routine_is_dtor)});
 values.push_back({"assoc_routine_is_lambda_body",static_cast<int>(x->assoc_routine_is_lambda_body)});
 values.push_back({"routine_name_linkage_is_explicit",static_cast<int>(x->routine_name_linkage_is_explicit)});
 values.push_back({"does_not_return",static_cast<int>(x->does_not_return)});
 values.push_back({"has_enable_if_attribute",static_cast<int>(x->has_enable_if_attribute)});
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"result_should_be_used",static_cast<int>(x->result_should_be_used)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_gnu_const",static_cast<int>(x->is_const)});
#endif
 values.push_back({"is_variadic_instance",static_cast<int>(x->is_variadic_instance)});
#if (MICROSOFT_EXTENSIONS_ALLOWED || GNU_X86_ATTRIBUTES_ALLOWED)
 values.push_back({"explicit_calling_convention",static_cast<int>(x->explicit_calling_convention)});
#endif
 values.push_back({"had_been_implicitly_const",static_cast<int>(x->had_been_implicitly_const)});
 values.push_back({"is_conditionally_explicit",static_cast<int>(x->is_conditionally_explicit)});
 values.push_back({"has_this_param",static_cast<int>(x->has_this_param)});
 values.push_back({"is_const",static_cast<int>((x->qualifiers&TQ_CONST)!=0)});
 values.push_back({"is_this_const",static_cast<int>((x->this_qualifiers&TQ_CONST)!=0)});
 values.push_back({"is_volatile",static_cast<int>((x->qualifiers&TQ_VOLATILE)!=0)});
 values.push_back({"is_this_volatile",static_cast<int>((x->this_qualifiers&TQ_VOLATILE)!=0)});
 values.push_back({"is_restrict",static_cast<int>((x->qualifiers&TQ_RESTRICT)!=0)});
 values.push_back({"is_this_restrict",static_cast<int>((x->this_qualifiers&TQ_RESTRICT)!=0)});
 values.push_back({"is_atomic",static_cast<int>((x->qualifiers&TQ_C11_ATOMIC)!=0)});
 values.push_back({"is_this_atomic",static_cast<int>((x->this_qualifiers&TQ_C11_ATOMIC)!=0)});
 values.push_back({"is_nullable",static_cast<int>((x->qualifiers&TQ_NULLABLE)!=0)});
 values.push_back({"is_this_nullable",static_cast<int>((x->this_qualifiers&TQ_NULLABLE)!=0)});
 values.push_back({"is_nonnull",static_cast<int>((x->qualifiers&TQ_NONNULL)!=0)});
 values.push_back({"is_this_nonnull",static_cast<int>((x->this_qualifiers&TQ_NONNULL)!=0)});
 values.push_back({"is_null_unspecified",static_cast<int>((x->qualifiers&TQ_NULL_UNSPECIFIED)!=0)});
 values.push_back({"is_this_null_unspecified",static_cast<int>((x->this_qualifiers&TQ_NULL_UNSPECIFIED)!=0)});
#if MICROSOFT_EXTENSIONS_ALLOWED
 values.push_back({"is_unaligned",static_cast<int>((x->qualifiers&TQ_UNALIGNED)!=0)});
 values.push_back({"is_this_unaligned",static_cast<int>((x->this_qualifiers&TQ_UNALIGNED)!=0)});
#endif
#if NEAR_AND_FAR_ALLOWED
 values.push_back({"is_near",static_cast<int>((x->qualifiers&TQ_NEAR)!=0)});
 values.push_back({"is_this_near",static_cast<int>((x->this_qualifiers&TQ_NEAR)!=0)});
#endif
#if NEAR_AND_FAR_ALLOWED
 values.push_back({"is_far",static_cast<int>((x->qualifiers&TQ_FAR)!=0)});
 values.push_back({"is_this_far",static_cast<int>((x->this_qualifiers&TQ_FAR)!=0)});
#endif
#if UPC_EXTENSIONS_ALLOWED
 values.push_back({"is_shared",static_cast<int>((x->qualifiers&TQ_UPC_SHARED)!=0)});
 values.push_back({"is_this_shared",static_cast<int>((x->this_qualifiers&TQ_UPC_SHARED)!=0)});
#endif
#if UPC_EXTENSIONS_ALLOWED
 values.push_back({"is_upc_strict",static_cast<int>((x->qualifiers&TQ_UPC_STRICT)!=0)});
 values.push_back({"is_this_upc_strict",static_cast<int>((x->this_qualifiers&TQ_UPC_STRICT)!=0)});
#endif
#if UPC_EXTENSIONS_ALLOWED
 values.push_back({"is_upc_relaxed",static_cast<int>((x->qualifiers&TQ_UPC_RELAXED)!=0)});
 values.push_back({"is_this_upc_relaxed",static_cast<int>((x->this_qualifiers&TQ_UPC_RELAXED)!=0)});
#endif
#if NAMED_ADDRESS_SPACES_ALLOWED
 values.push_back({"address_space",static_cast<int>(named_address_space_from_qualifier_set(x->qualifiers))});
 values.push_back({"this_address_space",static_cast<int>(named_address_space_from_qualifier_set(x->this_qualifiers))});
#endif
 DB.put_fields("routine_type",values);
 int ordinal=0;
 for(a_param_type_ptr a=x->param_type_list;a;a=a->next)
  DB.put("routine_type_parameter",{{"owner_id",id},{"ordinal",ordinal++},{"parameter_type_id",roslyn_parameter_type(a)},
   {"role_id",K("role",roslyn_parameter_role(p,a,x->assoc_routine))},
   {"base_subobject_id",R(roslyn_base(roslyn_parameter_base(p,a,x->assoc_routine)))}});
}

static Rid roslyn_routine(a_routine_ptr p) {
 bool fresh;Rid id=DB.claim(p,"routine",fresh);if(!fresh)return id;roslyn_prepare_declaration(id);
 int saved_role=roslyn_default_role;roslyn_default_role=roslyn_routine_role(p);
 a_routine_ptr saved_routine=roslyn_current_routine;roslyn_current_routine=p;
 a_type_ptr callable=skip_typerefs(p->type);
 Rid type=roslyn_type(p->type),callable_type=roslyn_type(callable);
 roslyn_symbol(id,"routine",p->source_corresp,type,p->compiler_generated,roslyn_kind_edg_storage_class(p->storage_class));
 a_scope_ptr scope=p->function_def_number!=NULL_function_def_number?scope_for_routine_or_null(p):nullptr;
 roslyn_collect_local_initializers(scope);
 roslyn_fact(id,"original_signature",DO_IL_LOWERING&&!roslyn_metadata_routine(p)?"lost":"available",
  DO_IL_LOWERING&&!roslyn_metadata_routine(p)?"Stock lowering changes the current signature; the unlowered routine type is not backend-readable.":nullptr);
 bool retained_parameters=scope!=nullptr;
 for(a_param_type_ptr a=callable->variant.routine.extra_info->param_type_list;a;a=a->next)if(a->param_num)retained_parameters=true;
 roslyn_fact(id,"source_parameters",retained_parameters||!DO_IL_LOWERING?"available":"not_retained",
  retained_parameters||!DO_IL_LOWERING?nullptr:"The current signature has no retained source-parameter association; current ABI parameters remain available.");
 Rid saved_eh=roslyn_current_eh;roslyn_current_eh=0;
 Rid root=scope?roslyn_statement(scope->assoc_block,roslyn_routine_role(p)):0;
 Rid containing=roslyn_type(callable->variant.routine.extra_info->this_class);
 if(!containing&&p->source_corresp.is_class_member&&p->source_corresp.parent_scope)
  containing=roslyn_type(p->source_corresp.parent_scope->variant.assoc_type);
 roslyn_fact(id,"body",root?"available":p->is_prototype_instantiation?"dependent":p->defined?"lost":"not_applicable",
  !root&&p->defined?"EDG records a completed definition whose body is no longer retained (for example, a trivial special member).":nullptr);
 RoslynSqlValue has_no_effect;
#if DO_IL_LOWERING
 if(p->has_no_effect)has_no_effect=RoslynSqlValue(1);
#endif
 std::vector<std::pair<const char*,RoslynSqlValue>> values={{"id",id},{"routine_type_id",callable_type},
  {"containing_type_id",R(containing)},{"body_root_id",R(root)},{"body_scope_id",R(roslyn_scope(scope))},
  {"body_state_id",K("body_state",p->is_prototype_instantiation?"dependent":root?"definition":p->defined?"discarded":"declaration_only")},
  {"has_no_effect",has_no_effect},{"is_ineligible",p->is_ineligible?RoslynSqlValue(1):RoslynSqlValue()},
  {"special_function_id",roslyn_kind_edg_special_function(p->special_kind)},
  {"primary_entry_id",R(ROSLYN_PRIMARY_ENTRY(p))},{"template_id",R(roslyn_template(p->assoc_template))},
  {"instance_id",R(p->assoc_template?roslyn_instance(p,id,p->assoc_template,p->template_arg_list,nullptr):0)},
  {"trailing_requires_clause_id",R(roslyn_requires_clause(p->trailing_requires_clause))},
  {"inherited_routine_id",R(p->is_inheriting_ctor?roslyn_routine(p->friends_or_originator.inherited_routine):0)},
  {"generating_using_id",R(roslyn_using(p->generating_using_decl,p->source_corresp.parent_scope))},
  {"fp_contract_id",roslyn_kind_edg_stdc_value(static_cast<a_stdc_pragma_value>(p->fp_contract))},
  {"fenv_access_id",roslyn_kind_edg_stdc_value(static_cast<a_stdc_pragma_value>(p->fenv_access))},
  {"cx_limited_range_id",roslyn_kind_edg_stdc_value(static_cast<a_stdc_pragma_value>(p->cx_limited_range))}};
 values.push_back({"address_taken",static_cast<int>(p->address_taken)});
 values.push_back({"is_virtual",static_cast<int>(p->is_virtual)});
 values.push_back({"overrides_base_member",static_cast<int>(p->overrides_base_member)});
 values.push_back({"is_pure",static_cast<int>(p->pure_virtual)});
 values.push_back({"is_final",static_cast<int>(p->final)});
 values.push_back({"is_override",static_cast<int>(p->override)});
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_abstract",static_cast<int>(p->abstract)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_sealed",static_cast<int>(p->sealed)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_new_member",static_cast<int>(p->new_member)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_interface_slot",static_cast<int>(p->interface_slot)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"definition_cannot_be_generated",static_cast<int>(p->definition_cannot_be_generated)});
#endif
 values.push_back({"covariant_return_virtual_override",static_cast<int>(p->covariant_return_virtual_override)});
 values.push_back({"is_inline",static_cast<int>(p->is_inline)});
 values.push_back({"is_declared_constexpr",static_cast<int>(p->is_declared_constexpr)});
 values.push_back({"is_constexpr",static_cast<int>(p->is_constexpr)});
 values.push_back({"is_consteval",static_cast<int>(p->is_consteval)});
 values.push_back({"is_constexpr_intrinsic",static_cast<int>(p->is_constexpr_intrinsic)});
 values.push_back({"is_defined",static_cast<int>(p->defined)});
 values.push_back({"is_called",static_cast<int>(p->called)});
 values.push_back({"is_explicit_constructor",static_cast<int>(p->is_explicit_constructor)});
 values.push_back({"is_explicit_conversion_function",static_cast<int>(p->is_explicit_conversion_function)});
 values.push_back({"is_trivial_default_constructor",static_cast<int>(p->is_trivial_default_constructor)});
 values.push_back({"is_trivial_copy_function",static_cast<int>(p->is_trivial_copy_function)});
 values.push_back({"is_trivial_destructor",static_cast<int>(p->is_trivial_destructor)});
 values.push_back({"is_initializer_list_ctor",static_cast<int>(p->is_initializer_list_ctor)});
 values.push_back({"is_delegating_ctor",static_cast<int>(p->is_delegating_ctor)});
 values.push_back({"is_inheriting_ctor",static_cast<int>(p->is_inheriting_ctor)});
 values.push_back({"inherits_virtually",static_cast<int>(p->inherits_virtually)});
 values.push_back({"is_deduction_guide_from_inheriting_ctor",static_cast<int>(p->is_deduction_guide_from_inheriting_ctor)});
#if (ASSIGNMENT_TO_THIS_ALLOWED)
 values.push_back({"assignment_to_this_done",static_cast<int>(p->assignment_to_this_done)});
#endif
 values.push_back({"is_template_function",static_cast<int>(p->is_template_function)});
 values.push_back({"is_specialized",static_cast<int>(p->is_specialized)});
 values.push_back({"specialized_with_old_syntax",static_cast<int>(p->specialized_with_old_syntax)});
 values.push_back({"is_prototype_instantiation",static_cast<int>(p->is_prototype_instantiation)});
 values.push_back({"never_throws",static_cast<int>(p->never_throws)});
 values.push_back({"is_in_class_specialization",static_cast<int>(p->is_in_class_specialization)});
 values.push_back({"explicit_instantiation",static_cast<int>(p->explicit_instantiation)});
 values.push_back({"class_explicitly_instantiated",static_cast<int>(p->class_explicitly_instantiated)});
 values.push_back({"explicit_do_not_instantiate",static_cast<int>(p->explicit_do_not_instantiate)});
 values.push_back({"has_nodiscard_attribute",static_cast<int>(p->has_nodiscard_attribute)});
#if (GNU_EXTENSIONS_ALLOWED || MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"never_inline",static_cast<int>(p->never_inline)});
#endif
#if (GNU_EXTENSIONS_ALLOWED || MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_pure_function",static_cast<int>(p->is_pure)});
#endif
#if (GNU_EXTENSIONS_ALLOWED || MICROSOFT_EXTENSIONS_ALLOWED) && (GNU_NAKED_ATTRIBUTE_ALLOWED || MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_naked",static_cast<int>(p->is_naked)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"declared_only_as_friend",static_cast<int>(p->declared_only_as_friend)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"explicit_extern_inline",static_cast<int>(p->explicit_extern_inline)});
#endif
#if (MICROSOFT_EXTENSIONS_ALLOWED)
 values.push_back({"is_direct_linkage_specifier_on_nondef_decl",static_cast<int>(p->direct_linkage_specifier_on_nondef_decl)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_initialization_routine",static_cast<int>(p->is_initialization_routine)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_finalization_routine",static_cast<int>(p->is_finalization_routine)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_weak",static_cast<int>(p->is_weak)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_weakref",static_cast<int>(p->is_weakref)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_gnu_alias",static_cast<int>(p->is_gnu_alias)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"is_ifunc",static_cast<int>(p->is_ifunc)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"has_gnu_used_attribute",static_cast<int>(p->has_gnu_used_attribute)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"has_gnu_abi_tag_attribute",static_cast<int>(p->has_gnu_abi_tag_attribute)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"in_gnu_abi_tag_namespace",static_cast<int>(p->in_gnu_abi_tag_namespace)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"implicit_abi_tags_added",static_cast<int>(p->implicit_abi_tags_added)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"allocates_memory",static_cast<int>(p->allocates_memory)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"no_instrument_function",static_cast<int>(p->no_instrument_function)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"no_check_memory_usage",static_cast<int>(p->no_check_memory_usage)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"always_inline",static_cast<int>(p->always_inline)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"gnu_c89_inline",static_cast<int>(p->gnu_c89_inline)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"implicit_alias",static_cast<int>(p->implicit_alias)});
#endif
#if (GNU_EXTENSIONS_ALLOWED)
 values.push_back({"has_internal_linkage_attribute",static_cast<int>(p->has_internal_linkage_attribute)});
#endif
#if (AUTOMATIC_TEMPLATE_INSTANTIATION)
 values.push_back({"can_be_instantiated",static_cast<int>(p->can_be_instantiated)});
#endif
#if (AUTOMATIC_TEMPLATE_INSTANTIATION)
 values.push_back({"do_not_instantiate",static_cast<int>(p->do_not_instantiate)});
#endif
#if (AUTOMATIC_TEMPLATE_INSTANTIATION)
 values.push_back({"instance_required",static_cast<int>(p->instance_required)});
#endif
 values.push_back({"contains_try_block",static_cast<int>(p->contains_try_block)});
 values.push_back({"contains_local_class_type",static_cast<int>(p->contains_local_class_type)});
 values.push_back({"superseded_external",static_cast<int>(p->superseded_external)});
 values.push_back({"is_defined_in_friend_decl",static_cast<int>(p->defined_in_friend_decl)});
 values.push_back({"is_defined_outside_of_parent",static_cast<int>(p->defined_outside_of_parent)});
#if (DO_IL_LOWERING && MINIMAL_INLINING)
 values.push_back({"is_inlinable",static_cast<int>(p->inlinable)});
#endif
#if (MAINTAIN_NEEDED_FLAGS)
 values.push_back({"definition_needed",static_cast<int>(p->definition_needed)});
#endif
 values.push_back({"expl_template_arg_list_used",static_cast<int>(p->expl_template_arg_list_used)});
#if (BACK_END_IS_CP_GEN_BE)
 values.push_back({"definition_has_direct_linkage_specifier",static_cast<int>(p->definition_has_direct_linkage_specifier)});
#endif
 values.push_back({"definition_for_inlining_only",static_cast<int>(p->definition_for_inlining_only)});
#if (INSTANTIATE_EXTERN_INLINE)
 values.push_back({"inline_instance_required",static_cast<int>(p->inline_instance_required)});
#endif
 values.push_back({"suppress_inline_body",static_cast<int>(p->suppress_inline_body)});
 values.push_back({"need_out_of_line_copy",static_cast<int>(p->need_out_of_line_copy)});
 values.push_back({"contains_statement_expression",static_cast<int>(p->contains_statement_expression)});
#if (IA64_ABI)
 values.push_back({"inline_in_class_definition",static_cast<int>(p->inline_in_class_definition)});
#endif
#if (DO_IL_LOWERING && IA64_ABI)
 values.push_back({"is_use_comdat",static_cast<int>(p->use_comdat)});
#endif
#if (DO_IL_LOWERING && IA64_ABI)
 values.push_back({"is_alias_entry",static_cast<int>(p->is_alias_entry)});
#endif
#if (PROMOTE_LOCAL_ENTITIES_TO_FILE_SCOPE)
 values.push_back({"statics_have_been_promoted",static_cast<int>(p->statics_have_been_promoted)});
#endif
 values.push_back({"is_lambda_body",static_cast<int>(p->is_lambda_body)});
 values.push_back({"declared_using_type_without_linkage",static_cast<int>(p->declared_using_type_without_linkage)});
 values.push_back({"is_defaulted",static_cast<int>(p->is_defaulted)});
 values.push_back({"is_deleted",static_cast<int>(p->is_deleted)});
 values.push_back({"contains_local_static_variable",static_cast<int>(p->contains_local_static_variable)});
#if (GENERATE_SOURCE_SEQUENCE_LISTS)
 values.push_back({"is_embeds_source_entries",static_cast<int>(p->embedded_source_sequence_entries)});
#endif
 values.push_back({"is_raw_literal_operator",static_cast<int>(p->is_raw_literal_operator)});
#if (USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES)
 values.push_back({"is_tls_initialization_alias",static_cast<int>(p->is_tls_init_alias)});
#endif
 values.push_back({"is_tls_initialization_routine",static_cast<int>(p->is_tls_init_routine)});
#if (GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED)
 values.push_back({"has_ctor_priority",static_cast<int>(p->has_ctor_priority)});
#endif
#if (GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED)
 values.push_back({"has_dtor_priority",static_cast<int>(p->has_dtor_priority)});
#endif
 values.push_back({"has_deducible_return_type",static_cast<int>(p->has_deducible_return_type)});
 values.push_back({"has_deduced_return_type",static_cast<int>(p->has_deduced_return_type)});
 values.push_back({"contains_generic_lambda",static_cast<int>(p->contains_generic_lambda)});
 values.push_back({"is_coroutine",static_cast<int>(p->is_coroutine)});
 values.push_back({"friend_defined_in_instantiation",static_cast<int>(p->friend_defined_in_instantiation)});
 values.push_back({"has_pass_object_size_attr",static_cast<int>(p->has_pass_object_size_attr)});
 values.push_back({"from_injected_tokens",static_cast<int>(p->from_injected_tokens)});
#if GENERATE_SOURCE_SEQUENCE_LISTS
 values.push_back({"declared_storage_class_id",roslyn_kind_edg_storage_class(p->declared_storage_class)});
 values.push_back({"declared_type_id",R(roslyn_type(p->declared_type))});
#endif
 if(p->special_kind==sfk_operator)values.push_back({"operator_name_id",roslyn_kind_edg_opname(p->variant.opname_kind)});
#if BUILTIN_FUNCTIONS_ENABLED
 if(p->special_kind==sfk_none)values.push_back({"builtin_kind_id",R(roslyn_builtin_function_kind(p->variant.builtin_function_kind))});
#endif
 if(p->special_kind==sfk_lambda_entry_point)values.push_back({"lambda_call_operator_id",R(roslyn_routine(p->variant.lambda_call_operator))});
 if(p->special_kind==sfk_deduction_guide)values.push_back({"deduction_guide_template_id",R(roslyn_template(p->variant.class_template))});
 if(p->is_constexpr_intrinsic)values.push_back({"constexpr_intrinsic",p->number.constexpr_intrinsic});
 else if(p->is_virtual&&!p->is_consteval)values.push_back({"virtual_function_number",Q(p->number.virtual_function)});
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
 values.push_back({"overriding_function_id",R(roslyn_routine(p->overriding_function_for_wrapper))});
 values.push_back({"overridden_function_id",R(roslyn_routine(p->overridden_function_for_wrapper))});
#if IA64_ABI
 values.push_back({"this_adjustment",Q(p->delta)});values.push_back({"vcall_index",Q(p->vcall_index)});
 values.push_back({"return_adjustment",Q(p->return_delta)});values.push_back({"vbase_index",Q(p->vbase_index)});
#endif
#endif
#if DO_IL_LOWERING && IA64_ABI
 values.push_back({"ctor_dtor_kind_id",roslyn_kind_edg_ctor_dtor(static_cast<a_ctor_or_dtor_kind>(p->ctor_dtor_kind))});
#endif
#if GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED && DO_IL_LOWERING
 values.push_back({"initialization_priority",Q(p->init_priority)});
#endif
#if FIXED_POINT_ALLOWED
 values.push_back({"fx_full_precision_id",roslyn_kind_edg_stdc_value(static_cast<a_stdc_pragma_value>(p->fx_full_precision))});
 values.push_back({"fx_fract_overflow_id",roslyn_kind_edg_stdc_value(static_cast<a_stdc_pragma_value>(p->fx_fract_overflow))});
 values.push_back({"fx_accum_overflow_id",roslyn_kind_edg_stdc_value(static_cast<a_stdc_pragma_value>(p->fx_accum_overflow))});
#endif
#if UPC_EXTENSIONS_ALLOWED
 values.push_back({"upc_access_method",static_cast<int>(p->upc_access_method)});
#endif
#if GNU_EXTENSIONS_ALLOWED && GNU_VISIBILITY_ATTRIBUTE_ALLOWED
 values.push_back({"elf_visibility_id",roslyn_kind_edg_elf_visibility(static_cast<an_ELF_visibility_kind>(p->ELF_visibility))});
#endif
#if BACK_END_IS_CP_GEN_BE
 values.push_back({"definition_c_name_linkage_specified",static_cast<int>(p->definition_C_name_linkage_specified)});
 values.push_back({"surrounding_name_linkage_id",roslyn_kind_edg_name_linkage(static_cast<a_name_linkage_kind>(p->surrounding_name_linkage_state))});
#endif
#if GNU_EXTENSIONS_ALLOWED || REDEFINE_EXTNAME_PRAGMA_ENABLED
 values.push_back({"gnu_supplement_id",R(roslyn_gnu_routine_supplement(p->gnu_extra_info,p))});
#endif
#if DECL_MODIFIERS_IN_USE
 roslyn_decl_modifiers(id,p->decl_modifiers);
#endif
 DB.put_fields("routine_symbol",values);
 if(!p->is_inheriting_ctor) {
  int ordinal=0;
  for(a_class_list_entry_ptr c=p->friends_or_originator.befriending_classes;c;c=c->next)
   DB.put("routine_befriending_class",{{"owner_id",id},{"ordinal",ordinal++},{"type_id",roslyn_type(c->class_type)}});
 }
#if MICROSOFT_EXTENSIONS_ALLOWED
 int override_ordinal=0;
 for(an_il_entity_list_entry_ptr e=p->overridden_functions;e;e=e->next)
  DB.put("routine_selected_override",{{"owner_id",id},{"ordinal",override_ordinal++},{"entity_id",roslyn_entity(e->entity)}});
#endif
 if(scope) {
  int ordinal=0;a_param_type_ptr formal=callable->variant.routine.extra_info->param_type_list;
  for(a_variable_ptr v=scope->variant.routine.parameters;v;v=v->next) {
   DB.put("routine_parameter",{{"owner_id",id},{"ordinal",ordinal++},{"parameter_id",roslyn_variable(v)},
    {"role_id",K("role",v->is_this_parameter?"this_parameter":roslyn_parameter_role(p->type,formal,p))},
    {"is_source_parameter",static_cast<int>(!v->is_this_parameter&&v->variant.assoc_param_type&&v->variant.assoc_param_type->param_num!=0)}});
   if(formal)formal=formal->next;
  }
 }
 roslyn_current_eh=saved_eh;roslyn_current_routine=saved_routine;roslyn_default_role=saved_role;return id;
}
