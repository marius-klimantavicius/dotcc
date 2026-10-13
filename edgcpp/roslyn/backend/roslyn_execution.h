/* Retained executable expressions, statements and exact auxiliary descriptors. */
static Rid roslyn_switch_case(a_switch_case_entry_ptr p,int role) {
 bool fresh;Rid id=DB.claim(p,"switch_case_entry",fresh);if(!fresh)return id;
 Rid high=0;
#if GNU_EXTENSIONS_ALLOWED
 high=roslyn_constant(p->range_end);
#endif
 DB.put("switch_case_entry",{{"id",id},{"statement_id",R(roslyn_statement(p->stmt,role))},{"low_constant_id",R(roslyn_constant(p->case_value))},{"high_constant_id",R(high)},{"reachable_by_fall_through",static_cast<int>(p->reachable_by_fall_through)},{"source_origin_id",R(roslyn_origin(p->position))}});
 return id;
}
static Rid roslyn_switch_descriptor(a_switch_stmt_descr_ptr p,int role) {
 bool fresh;Rid id=DB.claim(p,"switch_descriptor",fresh);if(!fresh)return id;
 DB.put("switch_descriptor",{{"id",id},{"default_case_id",R(roslyn_switch_case(p->default_case,role))},{"has_sorted_cases",static_cast<int>(p->sorted_cases!=nullptr)}});
 int ordinal=0;for(a_switch_case_entry_ptr c=p->cases;c;c=c->next)DB.put("switch_descriptor_case",{{"owner_id",id},{"ordinal",ordinal++},{"case_id",roslyn_switch_case(c,role)}});
 ordinal=0;for(a_switch_case_entry_ptr c=p->sorted_cases;c;c=c->next_on_sorted_list)DB.put("switch_descriptor_sorted_case",{{"owner_id",id},{"ordinal",ordinal++},{"case_id",roslyn_switch_case(c,role)}});
 return id;
}
/* Source VLA computations follow resident dimensions, copies and typedef
   boundaries; no mutating lowering helper or frontend total-size cache. */
static bool roslyn_source_vla_array(a_type_ptr type) {
 std::set<a_type_ptr> seen;
 while(type&&seen.insert(type).second){
  if(type->kind==tk_typeref){type=type->variant.typeref.type;continue;}
  return type->kind==tk_array&&(type->variant.array.is_vla||type->variant.array.is_variable_size_array);
 }
 return false;
}
static void roslyn_vla_children(Rid owner,a_type_ptr type,int role) {
#if !DO_IL_LOWERING
 std::set<a_type_ptr> seen_types;std::set<a_vla_dimension_ptr> seen_dimensions;int ordinal=0;
 while(type&&seen_types.insert(type).second){
  if(type->kind==tk_typeref){if(typeref_is_typedef(type))break;type=type->variant.typeref.type;continue;}
  if(type->kind==tk_pointer){type=type->variant.pointer.type;continue;}
  if(type->kind==tk_routine){type=type->variant.routine.return_type;continue;}
  if(type->kind!=tk_array)break;
  if(type->variant.array.is_vla&&type->variant.array.has_assoc_vla_dimension){
   a_vla_dimension_ptr dimension=roslyn_type_vla_dimension(type);std::set<a_vla_dimension_ptr> copies;
   while(dimension&&dimension->original_dimension&&copies.insert(dimension).second)dimension=dimension->original_dimension;
   if(dimension&&!dimension->has_size_statement&&!dimension->in_prototype_scope&&dimension->dimension_expr&&seen_dimensions.insert(dimension).second){
    Rid expression=roslyn_expr(dimension->dimension_expr,role);
    DB.put("expression_vla_dimension",{{"owner_id",owner},{"ordinal",ordinal},{"dimension_id",roslyn_vla_dimension(dimension)}});
    roslyn_child(owner,"dimension",ordinal++,expression);
   }
  }
  type=type->variant.array.element_type;
 }
#endif
}
static void roslyn_expr_operands(Rid owner,an_expr_node_ptr list,int role,bool evaluated=true,bool callee_only=false) {int n=0;for(an_expr_node_ptr a=list;a;a=a->next){bool operand_evaluated=evaluated&&(!callee_only||n==0);Rid child=roslyn_expr(a,callee_only&&n>0?7002:role);DB.put("expression_operand",{{"owner_id",owner},{"role_id",K("role","operand")},{"ordinal",n},{"expression_id",child}});if(operand_evaluated)roslyn_child(owner,"operand",n,child);else roslyn_reference(owner,"metadata",n,child);++n;}}
#include "roslyn_expressions_extra.h"
static Rid roslyn_vla_dimension(a_vla_dimension_ptr p) {
 bool fresh;Rid id=DB.claim(p,"vla_dimension",fresh);if(!fresh)return id;
 Rid variable=0;
#if DO_IL_LOWERING && !LOWER_VARIABLE_LENGTH_ARRAYS
 variable=roslyn_variable(p->dimension_variable);
#endif
 DB.put("vla_dimension",{{"id",id},{"type_id",R(roslyn_type(p->type))},{"expression_id",R(roslyn_expr(p->dimension_expr,7002))},{"original_dimension_id",R(roslyn_vla_dimension(p->original_dimension))},{"dimension_variable_id",R(variable)},{"in_prototype_scope",static_cast<int>(p->in_prototype_scope)},{"has_size_statement",static_cast<int>(p->has_size_statement)},{"source_position_id",R(roslyn_position(p->position))}});
 return id;
}
static void roslyn_coroutine(a_coroutine_descr_ptr p,Rid id,int role) {
 if(!p){DB.fail("Coroutine statement has no retained descriptor");return;}
 if(p->error_descr){DB.fail("Invalid coroutine descriptor reached successful export");return;}
 Rid initial=roslyn_expr(p->initial_suspend_call,role),final=roslyn_expr(p->final_suspend_call,role),exception=roslyn_expr(p->unhandled_exception_call,role),result=roslyn_expr(p->get_return_object_call,role),failure=roslyn_expr(p->alloc_failure_gro_call,role);
 DB.put("coroutine",{{"id",id},{"routine_id",roslyn_routine(roslyn_current_routine)},{"traits_type_id",R(roslyn_type(p->traits))},{"handle_id",R(roslyn_variable(p->handle))},{"promise_id",R(roslyn_variable(p->promise))},{"initial_await_resume_id",R(roslyn_variable(p->init_await_resume))},{"this_parameter_copy_id",R(roslyn_variable(p->this_param_copy))},{"final_suspend_label_id",R(roslyn_label(p->final_suspend_label))},{"initial_suspend_id",R(initial)},{"final_suspend_id",R(final)},{"exception_id",R(exception)},{"return_object_id",R(result)},{"allocation_failure_call_id",R(failure)},{"allocator_id",R(roslyn_routine(p->new_routine))},{"deallocator_id",R(roslyn_routine(p->delete_routine))},{"source_position_id",R(roslyn_position(p->position))},{"has_return_void",static_cast<int>(p->has_return_void)},{"body_generated",static_cast<int>(p->body_generated)},{"error_descriptor",static_cast<int>(p->error_descr)}});
 int ordinal=0;for(a_variable_ptr v=p->parameter_copies;v;v=v->next,++ordinal)if(v->is_parameter&&v!=p->this_param_copy)DB.put("coroutine_parameter_copy",{{"owner_id",id},{"ordinal",ordinal},{"copy_id",roslyn_variable(v)},{"initialization_id",R(v->init_kind==initk_dynamic?roslyn_init(v->initializer.dynamic,7002):0)}});
 roslyn_child(id,"initialization",0,initial);roslyn_child(id,"result",0,result);roslyn_child(id,"cleanup",0,final);roslyn_child(id,"else",0,exception);roslyn_child(id,"else",1,failure);
}
static Rid roslyn_asm(an_asm_entry_ptr p) {
 bool fresh;Rid id=DB.claim(p,"asm_entry",fresh);if(!fresh)return id;
 int is_volatile=0,is_goto=0,gnu_form=0,has_volatile=0,has_inline=0;RoslynSqlValue count;
#if GNU_EXTENSIONS_ALLOWED
 is_volatile=p->is_volatile;is_goto=p->is_asm_goto;gnu_form=p->gnu_asm_form;has_volatile=p->has_volatile_keyword;has_inline=p->has_inline_keyword;
#if !RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
 count=Q(p->number_of_constraints);
#endif
#endif
 DB.put("asm_entry",{{"id",id},{"asm_string_id",roslyn_constant(p->asm_string)},{"gnu_asm_form",gnu_form},{"is_volatile",is_volatile},{"has_volatile_keyword",has_volatile},{"has_inline_keyword",has_inline},{"is_goto",is_goto},{"number_of_constraints",count}});roslyn_attach_correspondence(id,p->source_corresp);
#if GNU_EXTENSIONS_ALLOWED
 int ordinal=0;for(an_asm_operand_ptr a=p->operands;a;a=a->next){
 RoslynSqlValue constraint,modifier;const char *kind;
#if RECORD_RAW_ASM_OPERAND_DESCRIPTIONS
 constraint=R(DB.atom(a->constraints_string));kind=a->is_output_operand?"output":"input";
#else
 modifier=roslyn_kind_edg_asm_modifier(a->modifiers);kind=a->modifiers==aom_input?"input":a->modifiers==aom_modify?"inout":"output";
 int n=0;for(an_asm_operand_constraint_ptr c=a->constraints;c;c=c->next){Rid condition=0;
#if GNU_X86_ASM_EXTENSIONS_ALLOWED
 if(c->kind==aoc_cc)condition=DB.atom(c->cond_code);
#endif
 DB.put("asm_operand_constraint",{{"owner_id",id},{"operand_ordinal",ordinal},{"ordinal",n++},{"kind_id",roslyn_kind_edg_asm_constraint(c->kind)},{"condition_code_id",R(condition)}});}
#endif
 Rid expression=roslyn_expr(a->expression,7002);DB.put("asm_operand",{{"owner_id",id},{"ordinal",ordinal},{"kind_id",K("asm_operand_kind",kind)},{"name_id",R(DB.atom(a->name))},{"constraint_id",constraint},{"modifier_kind_id",modifier},{"expression_id",expression},{"source_position_id",R(roslyn_position(a->position))}});++ordinal;
 }
 ordinal=0;for(a_named_register_list_ptr c=p->clobbers;c;c=c->next)DB.put("asm_clobber",{{"owner_id",id},{"ordinal",ordinal++},{"register_kind_id",roslyn_kind_edg_named_register(c->reg)}});
 ordinal=0;for(a_label_list_ptr l=p->labels;l;l=l->next)DB.put("asm_label",{{"owner_id",id},{"ordinal",ordinal++},{"label_id",roslyn_label(l->label)}});
#endif
 return id;
}
static Rid roslyn_expr(an_expr_node_ptr p,int role) {
 if(!DO_IL_LOWERING&&role==7001)role=7003;bool fresh;Rid id=DB.claim(p,"expression",fresh);if(!fresh){DB.promote_execution(id,role);return id;}role=DB.semantic_role(id,role);int saved_role=roslyn_default_role;roslyn_default_role=role;
 if(p->kind==enk_error||p->kind==enk_reclaimed||p->kind==enk_last){DB.fail("Invalid EDG expression reached");roslyn_default_role=saved_role;return id;}
 int kind=roslyn_kind_expression(p->kind);Rid origin=p->kind==enk_token_sequence?roslyn_origin(p->position):roslyn_origin(p->position,&p->expr_range.end,&p->expr_range.start);DB.put("semantic_node",{{"id",id},{"kind_id",kind},{"source_origin_id",R(origin)},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"representation_role_id",role}});
 RoslynSqlValue normalized;
#if DO_IL_LOWERING
 normalized=RoslynSqlValue(static_cast<int>(p->is_non_normalized_boolean_controlling_expr));
#endif
 DB.put("expression",{{"id",id},{"type_id",roslyn_type(p->type)},{"value_category_id",K("value_category",p->is_lvalue?"lvalue":p->is_xvalue?"xvalue":"prvalue")},{"original_lvalue_type_id",R(roslyn_type(p->orig_lvalue_type))},{"result_unused",static_cast<int>(p->result_is_not_used)},{"initialization_guard",static_cast<int>(p->is_initialization_guard)},{"generated_default_argument",static_cast<int>(p->generated_default_arg)},{"volatile_fetch",static_cast<int>(p->volatile_fetch)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},{"is_parenthesized",static_cast<int>(p->is_parenthesized)},{"is_static_cast",static_cast<int>(p->is_static_cast)},{"is_functional_cast",static_cast<int>(p->is_functional_notation_cast)},{"is_brace_cast",static_cast<int>(p->is_brace_notation_cast)},{"is_objectless_member_reference",static_cast<int>(p->is_objectless_nonstatic_data_mem_ref)},{"is_gnu_extension",static_cast<int>(p->marked_as_gnu_extension)},{"type_definition_needed",static_cast<int>(p->type_definition_needed)},{"boolean_not_normalized",normalized},{"is_type_constraint",static_cast<int>(p->is_type_constraint)},{"was_lvalue_temp_initializer",static_cast<int>(p->was_lvalue_temp_initializer)},{"is_safe_cast",static_cast<int>(p->is_safe_cast)}});
 switch(p->kind){
 case enk_operation:{auto &o=p->variant.operation;Rid property_kind=0;
 bool unevaluated_operands=o.kind==eok_noexcept,unevaluated_arguments=false;
#if MICROSOFT_EXTENSIONS_ALLOWED
 unevaluated_operands=unevaluated_operands||o.kind==eok_assume;
#endif
#if GNU_EXTENSIONS_ALLOWED
 /* Stock intrinsic identity determines these pseudo-call arguments; routine
    names and unrelated pure/const builtin categories are not consulted. */
 unevaluated_arguments=is_call_to_builtin_function(p,(a_builtin_function_kind)bfk_assume)||
  is_call_to_builtin_function(p,(a_builtin_function_kind)bfk_constant_p)||
  is_call_to_builtin_function(p,(a_builtin_function_kind)bfk_classify_type);
#endif
 switch(o.kind){case eok_cast:case eok_lvalue_cast:case eok_ref_cast:case eok_lvalue_adjust:case eok_class_rvalue_adjust:case eok_base_class_cast:case eok_derived_class_cast:case eok_pm_base_class_cast:case eok_pm_derived_class_cast:case eok_dynamic_cast:case eok_ref_dynamic_cast:case eok_bool_cast:roslyn_vla_children(id,p->type,role);break;default:break;}
#if MICROSOFT_EXTENSIONS_ALLOWED
 property_kind=roslyn_kind_rewritten_property(static_cast<a_rewritten_property_reference_kind>(o.rewritten_property_reference_kind));
#endif
 DB.put("operator_expression",{{"id",id},{"operator_kind_id",roslyn_kind_operator(o.kind)},{"operation_type_kind_id",roslyn_kind_type(o.type_kind)},{"eval_left_to_right",static_cast<int>(o.eval_left_to_right)},{"eval_right_to_left",static_cast<int>(o.eval_right_to_left)},{"pointer_operand_is_second",static_cast<int>(o.pointer_operand_is_second)},{"is_reinterpret_like_cast",static_cast<int>(o.is_reinterpret_like_cast)},{"requires_runtime_cast_check",static_cast<int>(o.requires_runtime_cast_check)},{"returns_lvalue",static_cast<int>(o.returns_lvalue_instead_of_usual_rvalue)},{"is_reinterpret_cast",static_cast<int>(o.is_reinterpret_cast)},{"is_reference_cast",static_cast<int>(o.is_reference_cast)},{"is_rvalue_reference_cast",static_cast<int>(o.is_rvalue_reference_cast)},{"is_const_cast",static_cast<int>(o.is_const_cast)},{"is_tracking_reference_cast",static_cast<int>(o.is_tracking_reference_cast)},{"is_gnu_two_operand_conditional",static_cast<int>(o.is_gnu_two_operand_question_mark)},{"call_uses_operator_syntax",static_cast<int>(o.call_uses_operator_syntax)},{"implicit_member_naming",static_cast<int>(o.implicit_in_member_naming)},{"implicit_explicit_cast_step",static_cast<int>(o.implicit_step_of_explicit_cast)},{"is_conversion_call",static_cast<int>(o.is_conversion_call)},{"argument_dependent_lookup_suppressed",static_cast<int>(o.arg_dependent_lookup_suppressed_on_call)},{"qualified_function_name",static_cast<int>(o.call_with_qualified_function_name)},{"retained_virtual_call",static_cast<int>(o.is_virtual_call)},{"rewritten_property_kind_id",R(property_kind)}});roslyn_expr_operands(id,o.operands,unevaluated_operands?7002:role,!unevaluated_operands,unevaluated_arguments);
 if(o.kind==eok_call||o.kind==eok_dot_member_call||o.kind==eok_points_to_member_call||o.kind==eok_dot_pm_call||o.kind==eok_points_to_pm_call){
 an_expr_node_ptr callee=o.operands,target_expression=callee;
 if(callee&&callee->kind==enk_operation&&(callee->variant.operation.kind==eok_dot_static||callee->variant.operation.kind==eok_points_to_static)){auto selector=callee->variant.operation.operands;target_expression=selector?selector->next:nullptr;}
 a_routine_ptr selected=target_expression&&target_expression->kind==enk_routine?target_expression->variant.routine.ptr:nullptr;
 Rid target=roslyn_routine(selected);a_type_ptr signature=selected?skip_typerefs(selected->type):roslyn_call_type(callee);
 a_param_type_ptr formal=signature?signature->variant.routine.extra_info->param_type_list:nullptr;Rid receiver=0,returned=0;
 bool member=o.kind==eok_dot_member_call||o.kind==eok_points_to_member_call;
 bool member_pointer=o.kind==eok_dot_pm_call||o.kind==eok_points_to_pm_call;
 an_expr_node_ptr arguments=callee?callee->next:nullptr;if(member||member_pointer){receiver=roslyn_expr(arguments,role);arguments=arguments?arguments->next:nullptr;}
 int n=0;for(an_expr_node_ptr a=arguments;a;a=a->next){const char *argrole=roslyn_parameter_role(signature,formal,selected);Rid aid=roslyn_expr(a,unevaluated_arguments?7002:role);
 if(std::strcmp(argrole,"this_parameter")==0)receiver=aid;if(std::strcmp(argrole,"return_parameter")==0)returned=aid;
 DB.put("call_argument",{{"owner_id",id},{"ordinal",n++},{"expression_id",aid},{"parameter_type_id",R(roslyn_parameter_type(formal))},{"role_id",K("role",argrole)}});if(formal)formal=formal->next;}
 DB.put("call_expression",{{"id",id},{"callee_id",R(roslyn_expr(callee,role))},{"target_routine_id",R(target)},{"function_type_id",R(roslyn_type(signature))},{"receiver_id",R(receiver)},{"return_slot_id",R(returned)},{"source_target_routine_id",R(!DO_IL_LOWERING?target:0)},{"source_dispatch_kind_id",R(!DO_IL_LOWERING?K("source_dispatch_kind",member_pointer?"member_pointer":member&&o.is_virtual_call?"virtual":"non_virtual"):0)},{"call_kind_id",K("call_kind",member_pointer?"member_pointer":member?"member":target?"direct":"indirect")}});
 roslyn_fact(id,"source_target",DO_IL_LOWERING?"lost":target?"available":"not_applicable",DO_IL_LOWERING?"Stock lowering rewrites calls; a current callee is not a retained source-selected target.":target?nullptr:"The source call uses an indirect or member-pointer target.");
 roslyn_fact(id,"source_dispatch",DO_IL_LOWERING?"lost":"available",DO_IL_LOWERING?"Stock lowering rewrites member and virtual dispatch; the current call form is retained separately.":nullptr);
 }
 break;}
 case enk_constant:DB.put("constant_expression",{{"id",id},{"constant_id",roslyn_constant(p->variant.constant.ptr)},{"name_reference_id",R(roslyn_name_reference(p->variant.constant.name_reference))}});break;
 case enk_variable:DB.put("reference_expression",{{"id",id},{"target_id",roslyn_variable(p->variant.variable.ptr)},{"reference_kind_id",K("reference_kind","variable")},{"name_reference_id",R(roslyn_name_reference(p->variant.variable.name_reference))}});break;
 case enk_routine:DB.put("reference_expression",{{"id",id},{"target_id",R(roslyn_routine(p->variant.routine.ptr))},{"reference_kind_id",K("reference_kind","routine")},{"name_reference_id",R(roslyn_name_reference(p->variant.routine.name_reference))}});break;
 case enk_field:DB.put("reference_expression",{{"id",id},{"target_id",roslyn_field(p->variant.field.ptr)},{"reference_kind_id",K("reference_kind","field")},{"name_reference_id",R(roslyn_name_reference(p->variant.field.name_reference))}});break;
 case enk_object_lifetime:{auto &l=p->variant.object_lifetime;Rid ex=roslyn_expr(l.expr,role);DB.put("lifetime_expression",{{"id",id},{"expression_id",R(ex)},{"lifetime_id",R(roslyn_lifetime(l.ptr))}});roslyn_child(id,"body",0,ex);break;}
 case enk_temp_init:roslyn_vla_children(id,p->type,role);DB.put("temporary_expression",{{"id",id},{"initialization_id",roslyn_init(p->variant.init.dynamic_init,role)},{"source_type_id",R(roslyn_type(p->variant.init.source.type))}});roslyn_child(id,"initialization",0,roslyn_init(p->variant.init.dynamic_init,role));break;
 case enk_statement:{Rid body=roslyn_statement(p->variant.statement,role);DB.put("statement_expression",{{"id",id},{"statement_id",body}});roslyn_child(id,"body",0,body);break;}
 case enk_reuse_value:DB.put("reuse_expression",{{"id",id},{"initialization_id",roslyn_init(p->variant.reused_value_init,7002)}});break;
 case enk_builtin_operation:DB.put("builtin_expression",{{"id",id},{"kind_id",roslyn_kind_builtin(p->variant.builtin_operation.kind)}});roslyn_expr_operands(id,p->variant.builtin_operation.operands,role);break;
 case enk_type_operand:DB.put("reference_expression",{{"id",id},{"target_id",R(roslyn_type(p->variant.type_operand.type))},{"reference_kind_id",K("reference_kind","type")},{"name_reference_id",R(roslyn_name_reference(p->variant.type_operand.name_reference))}});break;
#if VLA_DEALLOCATIONS_IN_IL
 case enk_vla_dealloc:DB.put("vla_operation",{{"id",id},{"variable_id",roslyn_variable(p->variant.vla_variable)},{"type_id",roslyn_type(p->variant.vla_variable->type)}});break;
#endif
 case enk_param_ref:DB.put("parameter_reference_expression",{{"id",id},{"parameter_number",static_cast<int>(p->variant.param_ref.param_num)},{"parameter_depth",static_cast<int>(p->variant.param_ref.levels_up)},{"is_this",static_cast<int>(p->variant.param_ref.param_num==0)}});break;
 case enk_throw:{auto t=p->variant.throw_info;Rid ex=t?ER(t,expr,roslyn_expr):0;Rid init=t?roslyn_init(t->dynamic_init,DO_IL_LOWERING?7002:role):0;DB.put("throw_expression",{{"id",id},{"operand_id",R(ex)},{"thrown_type_id",R(t?roslyn_type(t->type):0)},{"initialization_id",R(init)},{"destructor_id",R(t?roslyn_routine(t->destructor):0)},{"is_rethrow",static_cast<int>(!t)}});
#if !ABI_CHANGES_FOR_RTTI
 if(t){int ordinal=0;for(an_accessible_base_class_ptr base=t->accessible_base_classes;base;base=base->next)DB.put("throw_accessible_base",{{"owner_id",id},{"ordinal",ordinal++},{"subobject_id",roslyn_base(base->base_class)}});}
#endif
 roslyn_child(id,"operand",1,ex);if(!DO_IL_LOWERING)roslyn_child(id,"initialization",0,init);break;}
 case enk_condition:{auto t=p->variant.condition;Rid ex=roslyn_expr(t->expr,role);DB.put("condition_expression",{{"id",id},{"expression_id",R(ex)},{"initialization_id",R(roslyn_statement(t->initialization,role))},{"dynamic_initialization_id",R(roslyn_init(t->dynamic_init,role))},{"variable_id",R(t->dynamic_init?roslyn_variable(t->dynamic_init->variable):0)},{"scope_id",R(roslyn_scope(t->scope))}});roslyn_child(id,"condition",0,ex);roslyn_child(id,"initialization",0,roslyn_statement(t->initialization,role));if(!DO_IL_LOWERING)roslyn_child(id,"initialization",1,roslyn_init(t->dynamic_init,role));break;}
 case enk_sizeof:case enk_alignof:case enk_datasizeof:{auto &q=p->variant.sizeof_info;bool evaluated=false,dynamic=false;
#if !DO_IL_LOWERING
 if(p->kind==enk_sizeof)dynamic=roslyn_source_vla_array(q.is_type?q.variant.type:q.variant.expr->type);
 evaluated=dynamic&&!q.is_type;
#endif
 Rid ex=q.is_type?0:roslyn_expr(q.variant.expr,evaluated?role:7002);DB.put("type_query_expression",{{"id",id},{"query_kind_id",kind},{"operand_type_id",R(q.is_type?roslyn_type(q.variant.type):0)},{"operand_expression_id",R(ex)},{"is_standard_alignof",static_cast<int>(q.is_std_alignof)},{"is_dynamic",static_cast<int>(dynamic)}});
 if(evaluated)roslyn_child(id,"operand",0,ex);else roslyn_reference(id,"metadata",0,ex);
 if(p->kind==enk_sizeof)roslyn_vla_children(id,q.is_type?q.variant.type:q.variant.expr->type,role);break;}
 case enk_address_of_ellipsis:break;
#if DO_IL_LOWERING && ABI_CHANGES_FOR_COVARIANT_VIRTUAL_FUNC_RETURN
 case enk_result_of_overriding_function:break;
#endif
 #if DO_IL_LOWERING && !DO_FULL_PORTABLE_EH_LOWERING
 case enk_lowered_eh_construct:{auto &e=p->variant.lowered_eh;Rid eh=roslyn_current_eh,region=0;std::string state;Rid routine=0,handler=0,try_id=0,te=0,ce=0,initialization=0;
 switch(e.kind){case leck_function_prologue:eh=roslyn_eh(e.variant.prologue_info,id);break;case leck_cleanup_state:case leck_unreachable_cleanup_state:{
#if GENERATE_EH_TABLES
 auto n=e.variant.cleanup_region_number;state=Q(n);auto found=roslyn_regions.find({eh,static_cast<unsigned long>(n)});if(found!=roslyn_regions.end())region=found->second;
#else
 initialization=roslyn_init(e.variant.cleanup_ptr,7002);
#endif
 DB.put("cleanup_state_operation",{{"id",id},{"region_id",R(region)},{"state_number",state.empty()?RoslynSqlValue():RoslynSqlValue(state)},{"is_unreachable",static_cast<int>(e.kind==leck_unreachable_cleanup_state)}});break;}
#if !GENERATE_EH_TABLES
 case leck_initialization_completed:initialization=roslyn_init(e.variant.dynamic_init,7002);break;
#endif
 case leck_function_epilogue:routine=roslyn_routine(e.variant.epilogue_routine);break;case leck_caught_object_address:handler=roslyn_handler(e.variant.caught_object_handler);break;case leck_catch_epilogue:handler=roslyn_handler(e.variant.epilogue_handler);break;case leck_try_epilogue:try_id=roslyn_try_descriptor(e.variant.epilogue_try_block);break;case leck_internal_try:te=roslyn_expr(e.variant.try_and_catch_expr,role);ce=roslyn_expr(e.variant.try_and_catch_expr->next,role);roslyn_child(id,"then",0,te);roslyn_child(id,"else",0,ce);break;default:break;}
 DB.put("lowered_eh_operation",{{"id",id},{"kind_id",roslyn_kind_lowered_eh(e.kind)},{"function_eh_id",R(eh)},{"region_id",R(region)},{"state_number",state.empty()?RoslynSqlValue():RoslynSqlValue(state)},{"routine_id",R(routine)},{"handler_id",R(handler)},{"try_block_id",R(try_id)},{"try_expression_id",R(te)},{"catch_expression_id",R(ce)},{"initialization_id",R(initialization)}});break;}
 #endif
 default:if(!roslyn_extra_expression(p,role,id,kind))DB.fail("Expression payload adapter missing: "+Q(p->kind));break;
 }
 roslyn_default_role=saved_role;return id;
}
static Rid roslyn_statement(a_statement_ptr p,int role) {
 if(!DO_IL_LOWERING&&role==7001)role=7003;bool fresh;Rid id=DB.claim(p,"statement",fresh);if(!fresh){DB.promote_execution(id,role);return id;}role=DB.semantic_role(id,role);int saved_role=roslyn_default_role;roslyn_default_role=role;int kind=roslyn_kind_statement(p->kind);
 DB.put("semantic_node",{{"id",id},{"kind_id",kind},{"source_origin_id",R(roslyn_origin(p->position,&p->end_position))},{"compiler_generated",static_cast<int>(p->compiler_generated)},{"representation_role_id",role}});
 roslyn_attributes(id,p->attributes);
 RoslynSqlValue lowering_generated,boilerplate;
#if DO_IL_LOWERING
 lowering_generated=RoslynSqlValue(static_cast<int>(p->lowering_generated));boilerplate=RoslynSqlValue(static_cast<int>(p->is_lowering_boilerplate));
#endif
 Rid source_entry=0;
#if GENERATE_SOURCE_SEQUENCE_LISTS
 source_entry=roslyn_source_entry(p->source_sequence_entry);
#endif
 Rid expression=roslyn_expr(p->expr,role);DB.put("statement",{{"id",id},{"kind_id",kind},{"expression_id",R(expression)},{"parent_id",R(roslyn_statement(p->parent,role))},{"is_guard",static_cast<int>(p->is_initialization_guard)},{"lowering_generated",lowering_generated},{"has_associated_pragma",static_cast<int>(p->has_associated_pragma)},{"is_lowering_boilerplate",boilerplate},{"is_fallthrough_statement",static_cast<int>(p->is_fallthrough_statement)},{"is_likely",static_cast<int>(p->is_likely)},{"is_unlikely",static_cast<int>(p->is_unlikely)},{"source_sequence_entry_id",R(source_entry)}});roslyn_child(id,"operand",0,expression);
 switch(p->kind){
 case stmk_expr:case stmk_empty:case stmk_coroutine_return:break;
#if GNU_EXTENSIONS_ALLOWED
 case stmk_assigned_goto:DB.put("jump_statement",{{"id",id},{"kind_id",K("jump_kind","assigned_goto")},{"value_id",R(expression)}});break;
#endif
 case stmk_coroutine:roslyn_coroutine(p->variant.coroutine.descr,id,role);break;
 case stmk_asm:{auto entry=p->variant.asm_entry;DB.put("asm_statement",{{"id",id},{"entry_id",roslyn_asm(entry)}});
#if GNU_EXTENSIONS_ALLOWED
 int ordinal=0;for(an_asm_operand_ptr a=entry->operands;a;a=a->next)roslyn_child(id,"argument",ordinal++,roslyn_expr(a->expression,role));
#endif
 break;}
#if ASM_FUNCTION_ALLOWED
 case stmk_asm_func_body:DB.put("asm_statement",{{"id",id},{"text_id",DB.atom(p->variant.asm_func_body)}});break;
#endif
 case stmk_constexpr_if:{auto s=p->variant.constexpr_if;Rid then=roslyn_statement(s->then_statement,role),els=roslyn_statement(s->else_statement,role);DB.put("if_statement",{{"id",id},{"then_id",R(then)},{"else_id",R(els)},{"condition_id",R(expression)},{"selection_known",static_cast<int>(s->value_known)},{"selected_then",s->value_known?RoslynSqlValue(static_cast<int>(s->value)):RoslynSqlValue()}});roslyn_child(id,"then",0,then);roslyn_child(id,"else",0,els);break;}
 case stmk_range_based_for:{auto &s=p->variant.range_based_for_loop;auto t=s.extra_info;Rid body=roslyn_statement(s.statement,role),init=roslyn_statement(t->initialization,role),test=roslyn_expr(t->ne_call_expr,role),increment=roslyn_expr(t->incr_call_expr,role);DB.put("range_for",{{"id",id},{"body_id",R(body)},{"initialization_id",R(init)},{"range_variable_id",R(roslyn_variable(t->range))},{"begin_variable_id",R(roslyn_variable(t->begin))},{"end_variable_id",R(roslyn_variable(t->end))},{"iterator_variable_id",R(roslyn_variable(t->iterator))},{"comparison_id",R(test)},{"increment_id",R(increment)},{"scope_id",R(roslyn_scope(t->range_based_for_scope))},{"iterator_scope_id",R(roslyn_scope(t->iterator_scope))},{"use_await",static_cast<int>(t->use_await)}});roslyn_child(id,"initialization",0,init);int ordinal=1;for(a_variable_ptr v:{t->range,t->begin,t->end,t->iterator})if(v&&v->init_kind==initk_dynamic)roslyn_child(id,"initialization",ordinal++,roslyn_init(v->initializer.dynamic,role));roslyn_child(id,"body",0,body);roslyn_child(id,"condition",0,test);roslyn_child(id,"increment",0,increment);break;}
 case stmk_set_vla_size:{auto dimension=p->variant.vla_dimension;DB.put("vla_operation",{{"id",id},{"type_id",R(roslyn_type(dimension->type))},{"dimension_id",roslyn_vla_dimension(dimension)}});roslyn_child(id,"operand",1,roslyn_expr(dimension->dimension_expr,role));break;}
 case stmk_vla_decl:{auto &v=p->variant.vla;DB.put("vla_operation",{{"id",id},{"variable_id",R(v.is_typedef_decl?0:roslyn_variable(v.variant.variable))},{"type_id",R(roslyn_type(v.is_typedef_decl?v.variant.typedef_type:v.variant.variable->type))},{"is_typedef_declaration",static_cast<int>(v.is_typedef_decl)}});break;}
 case stmk_stmt_expr_result:{Rid init=roslyn_init(p->variant.stmt_expr_result.dynamic_init,role);if(init)DB.put("initialization_statement",{{"id",id},{"initialization_id",init}});roslyn_child(id,"initialization",0,init);break;}
 case stmk_block:{auto info=p->variant.block.extra_info;DB.put("block_statement",{{"id",id},{"descriptor_id",R(roslyn_block_descriptor(info))}});int n=0;for(a_statement_ptr c=p->variant.block.statements;c;c=c->next){Rid child=roslyn_statement(c,role);DB.put("block_item",{{"owner_id",id},{"ordinal",n},{"statement_id",child}});roslyn_child(id,"statement",n++,child);}break;}
 case stmk_if:case stmk_if_consteval:case stmk_if_not_consteval:{auto &s=p->variant.if_stmt;Rid then=roslyn_statement(s.then_statement,role),els=roslyn_statement(s.else_statement,role);DB.put("if_statement",{{"id",id},{"then_id",R(then)},{"else_id",R(els)},{"condition_id",R(expression)}});roslyn_child(id,"then",0,then);roslyn_child(id,"else",0,els);break;}
 case stmk_while:case stmk_end_test_while:{Rid body=roslyn_statement(p->variant.loop_statement,role);DB.put("loop_statement",{{"id",id},{"body_id",R(body)},{"condition_id",R(expression)}});roslyn_child(id,"body",0,body);break;}
 #if UPC_EXTENSIONS_ALLOWED
 case stmk_upc_notify:case stmk_upc_wait:case stmk_upc_barrier:case stmk_upc_fence:break;
 case stmk_upc_forall:
#endif
 case stmk_for:{auto &s=p->variant.for_loop;Rid body=roslyn_statement(s.statement,role),init=s.extra_info?roslyn_statement(s.extra_info->initialization,role):0,inc=s.extra_info?roslyn_expr(s.extra_info->increment,role):0,affinity=0;
#if UPC_EXTENSIONS_ALLOWED
 affinity=s.extra_info?roslyn_expr(s.extra_info->affinity,role):0;
#endif
 DB.put("loop_statement",{{"id",id},{"body_id",R(body)},{"initialization_id",R(init)},{"condition_id",R(expression)},{"increment_id",R(inc)},{"affinity_id",R(affinity)},{"init_scope_id",R(s.extra_info?roslyn_scope(s.extra_info->for_init_scope):0)}});roslyn_child(id,"initialization",0,init);roslyn_child(id,"body",0,body);roslyn_child(id,"increment",0,inc);roslyn_child(id,"affinity",0,affinity);break;}
 case stmk_return:case stmk_goto:{Rid init=p->kind==stmk_return?roslyn_init(p->variant.return_dynamic_init,role):0;Rid label=p->kind==stmk_goto?roslyn_label(p->variant.label.ptr):0;DB.put("jump_statement",{{"id",id},{"kind_id",K("jump_kind",p->kind==stmk_return?"return":p->variant.label.ptr->break_label?"break":p->variant.label.ptr->continue_label?"continue":"goto")},{"value_id",R(expression)},{"label_id",R(label)},{"initialization_id",R(init)},{"common_lifetime_id",R(p->kind==stmk_goto?roslyn_lifetime(p->variant.label.lifetime):0)}});roslyn_child(id,"initialization",0,init);break;}
 case stmk_label:DB.put("label_statement",{{"id",id},{"label_id",roslyn_label(p->variant.label.ptr)}});break;
 case stmk_switch:{auto &s=p->variant.switch_stmt;Rid body=roslyn_statement(s.body_statement,role);DB.put("switch_statement",{{"id",id},{"body_id",R(body)},{"selector_id",R(expression)},{"descriptor_id",roslyn_switch_descriptor(s.extra_info,role)}});roslyn_child(id,"body",0,body);break;}
 #if MICROSOFT_EXTENSIONS_ALLOWED
 case stmk_for_each:{auto &s=p->variant.for_each_loop;auto t=s.extra_info;bool resolved_native=t->kind==sfepk_stl_pattern||t->kind==sfepk_array_pattern;if(t->kind!=sfepk_none&&!resolved_native){DB.fail("For-each managed dialect pattern is excluded");break;}
 a_variable_ptr iterator=t->uses_prev_decl_iterator?t->iterator.prev_decl.variable:t->iterator.variable;
 a_field_ptr field=t->uses_prev_decl_iterator?t->iterator.prev_decl.field:nullptr;
 a_variable_ptr end_variable=resolved_native?t->variant.stl_array_pattern.end_variable:nullptr;
 Rid body=roslyn_statement(s.statement,role),assignment=roslyn_expr(t->uses_prev_decl_iterator?t->iterator.prev_decl.assign_expr:nullptr,role),test=resolved_native?roslyn_expr(t->variant.stl_array_pattern.ne_call_expr,role):0,increment=resolved_native?roslyn_expr(t->variant.stl_array_pattern.incr_call_expr,role):0;
 DB.put("for_each_statement",{{"id",id},{"body_id",R(body)},{"collection_id",R(expression)},{"pattern_kind_id",roslyn_kind_for_each_pattern(t->kind)},{"iterator_id",R(roslyn_variable(iterator))},{"uses_previous_declaration_iterator",static_cast<int>(t->uses_prev_decl_iterator)},{"previous_field_id",R(roslyn_field(field))},{"assignment_id",R(assignment)},{"collection_reference_id",R(roslyn_variable(t->collection_expr_ref))},{"temporary_variable_id",R(roslyn_variable(t->temporary_variable))},{"end_variable_id",R(roslyn_variable(end_variable))},{"test_id",R(test)},{"increment_id",R(increment)},{"scope_id",R(roslyn_scope(t->for_each_scope))},{"iterator_scope_id",R(roslyn_scope(t->iterator_scope))}});
 int ordinal=0;for(a_variable_ptr v:{t->collection_expr_ref,t->temporary_variable,end_variable,t->uses_prev_decl_iterator?nullptr:iterator})if(v&&v->init_kind==initk_dynamic)roslyn_child(id,"initialization",ordinal++,roslyn_init(v->initializer.dynamic,role));roslyn_child(id,"body",0,body);roslyn_child(id,"condition",0,test);roslyn_child(id,"increment",0,increment);roslyn_child(id,"operand",1,assignment);break;}
#endif
 case stmk_switch_case:DB.put("switch_case_statement",{{"id",id},{"entry_id",roslyn_switch_case(p->variant.switch_case.extra_info,role)},{"switch_statement_id",roslyn_statement(p->variant.switch_case.switch_statement,role)}});break;
 case stmk_init:{Rid init=roslyn_init(p->variant.dynamic_init,role);DB.put("initialization_statement",{{"id",id},{"initialization_id",init}});roslyn_reference(id,"initialization",0,init);roslyn_child(id,"initialization",0,init);break;}
 case stmk_try_block:{auto supplement=p->variant.try_block;Rid descriptor=roslyn_try_descriptor(supplement),body=roslyn_statement(supplement->statement,role);DB.put("try_statement",{{"id",id},{"descriptor_id",descriptor}});roslyn_child(id,"body",0,body);int ordinal=0;for(a_handler_ptr h=supplement->handlers;h;h=h->next,++ordinal){Rid handler=roslyn_handler(h);if(!DO_IL_LOWERING)roslyn_child(id,"initialization",ordinal,roslyn_init(h->dynamic_init,role));roslyn_reference(id,"member",ordinal,handler);roslyn_child(id,"else",ordinal,roslyn_statement(h->statement,role));}break;}
 case stmk_microsoft_try:{auto s=p->variant.microsoft_try;Rid body=roslyn_statement(s->guarded_statement,role),filter=roslyn_expr(s->except_expr,role),cleanup=roslyn_statement(s->cleanup_statement,role);DB.put("microsoft_try_statement",{{"id",id},{"body_id",body},{"filter_id",R(filter)},{"handler_id",R(filter?cleanup:0)},{"finally_id",R(!filter?cleanup:0)},{"is_finally",static_cast<int>(!filter)},{"except_or_finally_position_id",R(roslyn_position(s->except_or_finally_position))}});roslyn_child(id,"body",0,body);roslyn_child(id,"else",0,cleanup);roslyn_child(id,"condition",0,filter);break;}
 case stmk_decl:{DB.put("declaration_statement",{{"id",id},{"has_static_or_thread_variable",static_cast<int>(p->variant.decl.has_static_or_thread_variable)}});int n=0;for(an_il_entity_list_entry_ptr e=p->variant.decl.entities;e;e=e->next){Rid entity=roslyn_entity(e->entity);DB.put("declaration_statement_entity",{{"owner_id",id},{"ordinal",n},{"entity_id",entity}});roslyn_reference(id,"member",n++,entity);
#if !DO_IL_LOWERING
 if(e->entity.kind==iek_variable){auto variable=reinterpret_cast<a_variable_ptr>(e->entity.ptr);if(variable->init_kind==initk_dynamic)roslyn_child(id,"initialization",n-1,roslyn_init(variable->initializer.dynamic,role));}
#endif
 }break;}
 default:DB.fail("Statement payload adapter missing: "+Q(p->kind));break;
 }
 roslyn_default_role=saved_role;return id;
}
