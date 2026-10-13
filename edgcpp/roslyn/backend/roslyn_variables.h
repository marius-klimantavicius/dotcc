/* Current EDG variable flags and discriminated associations; no synthetic calls. */
static Rid roslyn_variable(a_variable_ptr p) {
 bool fresh;Rid id=DB.claim(p,"variable",fresh);if(!fresh)return id;
 roslyn_prepare_declaration(id);Rid type=roslyn_type(p->type);
 roslyn_symbol(id,"variable",p->source_corresp,type,p->compiler_generated,roslyn_kind_edg_storage_class(p->storage_class));
 bool extension=false;
#if DECL_MODIFIERS_IN_USE
 extension=(p->decl_modifiers&DM_THREAD)!=0;
#endif
 bool standard=is_effective_thread_local(p)!=FALSE;
 std::vector<std::pair<const char*,RoslynSqlValue>> fields={{"id",id},{"type_id",type},
  {"initialization_kind_id",roslyn_kind_initializer(p->init_kind)},
  {"initializer_id",R(p->init_kind==initk_dynamic?roslyn_init(p->initializer.dynamic,7002):0)},
  {"constant_initializer_id",R(p->init_kind==initk_static&&!roslyn_projected_tables.count(p)?roslyn_constant(p->initializer.constant):0)},
  {"bound_expression_id",R(p->init_kind==initk_binding?roslyn_expr(p->initializer.bound_expr,7002):0)},
  {"is_thread_local",static_cast<int>(standard||extension)},
  {"standard_thread_local_declared",static_cast<int>(p->is_thread_local)},
  {"standard_thread_local_effective",static_cast<int>(standard)},{"extension_thread_local",static_cast<int>(extension)},
  {"instance_id",R(p->template_info&&p->template_info->assoc_template?roslyn_instance(p,id,p->template_info->assoc_template,p->template_info->template_arg_list,p->template_info->partial_spec_template_arg_list):0)},
  {"is_inline",static_cast<int>(p->is_inline)},{"is_parameter",static_cast<int>(p->is_parameter)},
  {"binding_container_id",R(p->is_struct_binding?roslyn_variable(p->variant.container):0)},
  {"is_structured_binding",static_cast<int>(p->is_struct_binding)},
  {"is_structured_binding_container",static_cast<int>(p->is_struct_binding_container)},
  {"associated_parameter_type_id",R(p->is_parameter&&!p->is_this_parameter&&!p->is_struct_binding&&!p->is_struct_binding_container?roslyn_parameter_type(p->variant.assoc_param_type):0)},
  {"explicit_alignment",Q(p->alignment)},
  {"storage_duration_id",K("storage_duration",p->is_parameter?"parameter":standard||extension?"thread":p->storage_class==sc_auto||p->storage_class==sc_register?"automatic":"static")}};
#define VARIABLE_FLAG(column,member) fields.push_back({column,static_cast<int>(p->member)})
 VARIABLE_FLAG("is_used",used);VARIABLE_FLAG("address_taken",address_taken);
 VARIABLE_FLAG("referenced_non_locally",referenced_non_locally);VARIABLE_FLAG("modified_within_try_block",modified_within_try_block);
 VARIABLE_FLAG("is_template_variable",is_template_variable);VARIABLE_FLAG("is_prototype_instantiation",is_prototype_instantiation);
 VARIABLE_FLAG("is_nonreal",is_nonreal);VARIABLE_FLAG("is_specialized",is_specialized);
 VARIABLE_FLAG("specialized_with_old_syntax",specialized_with_old_syntax);VARIABLE_FLAG("explicit_instantiation",explicit_instantiation);
 VARIABLE_FLAG("class_explicitly_instantiated",class_explicitly_instantiated);VARIABLE_FLAG("explicit_do_not_instantiate",explicit_do_not_instantiate);
 VARIABLE_FLAG("parameter_value_changed",param_value_has_been_changed);VARIABLE_FLAG("parameter_used_more_than_once",param_used_more_than_once);
 VARIABLE_FLAG("is_handler_parameter",is_handler_param);VARIABLE_FLAG("is_this_parameter",is_this_parameter);
 VARIABLE_FLAG("is_anonymous_parent_object",is_anonymous_parent_object);VARIABLE_FLAG("is_member_constant",is_member_constant);
 VARIABLE_FLAG("is_constexpr",is_constexpr);VARIABLE_FLAG("is_constinit",declared_constinit);
 VARIABLE_FLAG("suppress_inline_definition",suppress_inline_definition);VARIABLE_FLAG("superseded_external",superseded_external);
 VARIABLE_FLAG("has_variably_modified_type",has_variably_modified_type);VARIABLE_FLAG("is_vla",is_vla);
 VARIABLE_FLAG("is_compound_literal",is_compound_literal);VARIABLE_FLAG("has_explicit_initializer",has_explicit_initializer);
 VARIABLE_FLAG("has_parenthesized_initializer",has_parenthesized_initializer);VARIABLE_FLAG("has_direct_braced_initializer",has_direct_braced_initializer);
 VARIABLE_FLAG("is_uninitialized",uninitialized);VARIABLE_FLAG("declared_with_auto",declared_with_auto_type_specifier);
 VARIABLE_FLAG("declared_with_decltype_auto",declared_with_decltype_auto);VARIABLE_FLAG("declared_with_class_template_placeholder",declared_with_class_template_placeholder);
 VARIABLE_FLAG("declared_using_type_without_linkage",declared_using_type_without_linkage);VARIABLE_FLAG("is_pack",is_pack);
 VARIABLE_FLAG("is_pack_element",is_pack_element);VARIABLE_FLAG("is_enhanced_for_iterator",is_enhanced_for_iterator);
 VARIABLE_FLAG("initializer_in_class",initializer_in_class);VARIABLE_FLAG("constant_valued_recorded",constant_valued);
 VARIABLE_FLAG("is_immutable",is_immutable);VARIABLE_FLAG("extends_lifetime",extends_lifetime);
 VARIABLE_FLAG("is_template_parameter_object",is_template_param_object);VARIABLE_FLAG("is_in_class_specialization",is_in_class_specialization);
#if DO_IL_LOWERING
 VARIABLE_FLAG("initialization_rewritten_as_assignment",initialization_rewritten_as_assignment);
 VARIABLE_FLAG("promoted_local_static_initialization",promoted_local_static_init);VARIABLE_FLAG("promoted_local_static",promoted_local_static);
 VARIABLE_FLAG("is_optional_vtable",is_optional_vtable);VARIABLE_FLAG("vtable_defined",vtable_defined);VARIABLE_FLAG("lowering_generated",lowering_generated);
 fields.push_back({"comdat_group_id",R(DB.atom(p->comdat_group))});
 fields.push_back({"vla_element_count_variable_id",R(roslyn_variable(p->vla_element_count_variable))});
#endif
#if GNU_EXTENSIONS_ALLOWED
 fields.push_back({"cleanup_routine_id",R(roslyn_routine(p->cleanup_routine))});
 fields.push_back({"aliased_variable_id",R(roslyn_variable(p->aliased_variable))});
 VARIABLE_FLAG("is_weak",is_weak);VARIABLE_FLAG("is_weakref",is_weakref);VARIABLE_FLAG("is_gnu_alias",is_gnu_alias);
 VARIABLE_FLAG("has_gnu_used_attribute",has_gnu_used_attribute);VARIABLE_FLAG("has_gnu_abi_tag_attribute",has_gnu_abi_tag_attribute);
 VARIABLE_FLAG("is_not_common",is_not_common);VARIABLE_FLAG("is_common",is_common);VARIABLE_FLAG("has_internal_linkage_attribute",has_internal_linkage_attribute);
#if GNU_VISIBILITY_ATTRIBUTE_ALLOWED
 fields.push_back({"elf_visibility_kind_id",roslyn_kind_edg_elf_visibility(static_cast<an_ELF_visibility_kind>(p->ELF_visibility))});
#endif
#if GNU_INIT_PRIORITY_ATTRIBUTE_ALLOWED
 fields.push_back({"initialization_priority",static_cast<int>(p->init_priority)});
#endif
#endif
#if GNU_EXTENSIONS_ALLOWED || MICROSOFT_EXTENSIONS_ALLOWED
 VARIABLE_FLAG("has_flexible_array_initializer",has_flexible_array_initializer);
 fields.push_back({"section_id",R(DB.atom(p->section))});
#endif
#if NAMED_REGISTERS_ALLOWED
 if(p->has_named_register_storage_class)fields.push_back({"named_register_number",p->asm_name_or_reg.id});
 else
#endif
#if GNU_EXTENSIONS_ALLOWED || REDEFINE_EXTNAME_PRAGMA_ENABLED
 if(p->asm_name_is_valid)fields.push_back({"assembly_name_id",R(DB.atom(p->asm_name_or_reg.name))});
 else
#endif
#if GNU_EXTENSIONS_ALLOWED
 fields.push_back({"register_kind_id",R(roslyn_kind_edg_named_register(p->asm_name_or_reg.reg))});
#else
 { }
#endif
#if SEPARATE_ROUTINES_FOR_FILE_SCOPE_DYNAMIC_INITS
 if(!p->is_thread_local)fields.push_back({"dynamic_initialization_routine_id",R(roslyn_routine(p->init_routine.dynamic_init_routine))});
#endif
#if USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
 if(p->is_thread_local){fields.push_back({"thread_initialization_routine_id",R(roslyn_routine(p->init_routine.thread.init_routine))});fields.push_back({"thread_wrapper_routine_id",R(roslyn_routine(p->init_routine.thread.wrapper))});}
#endif
#if MAINTAIN_NEEDED_FLAGS && !GENERATE_EH_TABLES
 fields.push_back({"effective_class_typeinfo_variable_id",R(roslyn_variable(p->eff_class_typeinfo_var))});
#endif
#if GENERATE_SOURCE_SEQUENCE_LISTS
 fields.push_back({"declared_storage_class_id",roslyn_kind_edg_storage_class(p->declared_storage_class)});
 fields.push_back({"declared_type_id",R(roslyn_type(p->declared_type))});VARIABLE_FLAG("embeds_source_entries",embedded_source_sequence_entries);
#endif
#undef VARIABLE_FLAG
 DB.put_fields("variable_symbol",fields);
#if DECL_MODIFIERS_IN_USE
 roslyn_decl_modifiers(id,p->decl_modifiers);
#endif
 if(p->is_struct_binding_container){int ordinal=0;for(an_il_entity_list_entry_ptr b=p->variant.bindings;b;b=b->next){
  if(b->entity.kind!=iek_variable){DB.fail("Structured binding list contains a non-variable entry");break;}
  DB.put("variable_binding",{{"owner_id",id},{"ordinal",ordinal++},{"variable_id",roslyn_variable(reinterpret_cast<a_variable_ptr>(b->entity.ptr))}});
 }}
 return id;
}
