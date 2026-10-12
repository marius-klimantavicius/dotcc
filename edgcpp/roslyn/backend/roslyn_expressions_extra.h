/* Retained residual expression payloads. Included inside the EDG namespace.
   Authority: pinned il_def.h:14677-14804,15410-15785,17932-18178.
   Parser caches, lambda declaration parse states and pending capture symbols
   are deliberately not dereferenced. */

static Rid roslyn_token_sequence(a_token_sequence_ptr p) {
  bool fresh;Rid id=DB.claim(p,"token_sequence",fresh);if(!fresh)return id;
  DB.put("token_sequence",{{"id",id}});
  int ordinal=0;
  for(a_token_sequence_entry_ptr token=p->tokens;token;token=token->next)
    DB.put("token_sequence_token",{{"owner_id",id},{"ordinal",ordinal++},
      {"kind_id",roslyn_kind_token(token->token_kind)},
      {"spelling_id",R(DB.atom(token->spelling))},
      {"source_position_id",R(roslyn_position(token->position))}});
  return id;
}

static Rid roslyn_lambda(a_lambda_ptr p) {
  bool fresh;Rid id=DB.claim(p,"lambda_definition",fresh);if(!fresh)return id;
  Rid capture_end=0,mutable_position=0,constexpr_position=0;
#if EXTRA_SOURCE_POSITIONS_IN_IL
  capture_end=roslyn_position(p->capture_end_position);
  mutable_position=roslyn_position(p->mutable_position);
  constexpr_position=roslyn_position(p->constexpr_position);
#endif
  DB.put("lambda_definition",{{"id",id},{"closure_type_id",roslyn_type(p->closure_class)},
    {"call_operator_id",R(roslyn_routine(p->lambda_routine))},
    {"source_origin_id",R(roslyn_origin(p->start_position))},
    {"capture_end_position_id",R(capture_end)},{"mutable_position_id",R(mutable_position)},
    {"constexpr_position_id",R(constexpr_position)},
    {"is_generic",static_cast<int>(p->is_generic)},{"is_mutable",static_cast<int>(p->is_mutable)},
    {"constexpr_specified",static_cast<int>(p->constexpr_specified)},
    {"consteval_specified",static_cast<int>(p->consteval_specified)},
    {"has_capture_default",static_cast<int>(p->has_capture_default)},
    {"default_is_reference",static_cast<int>(p->default_is_by_reference)},
    {"explicit_return_type",static_cast<int>(p->explicit_return_type)},
    {"has_parameter_declaration",static_cast<int>(p->has_parameter_decl)},
    {"has_template_parameter_list",static_cast<int>(p->has_template_param_list)}});
  int ordinal=0;
  for(a_lambda_capture_ptr capture=p->capture_list;capture;capture=capture->next) {
    Rid variable=0,initialization=0,indirect_field=0,source_field=0;
    if(capture->is_init_capture)initialization=roslyn_init(capture->captured.initializer,7002);
    else if(capture->is_indirect_init_capture)indirect_field=roslyn_field(capture->captured.init_capture_field);
    else variable=roslyn_variable(capture->captured.variable);
    if(!capture->is_init_capture && !capture->field_pending)
      source_field=roslyn_field(capture->capture_info.source_closure_field);
    bool is_this=capture->is_param_ref_capture || (!capture->is_init_capture
      && !capture->is_indirect_init_capture && capture->captured.variable
      && capture->captured.variable->is_this_parameter);
    const char *kind=capture->is_init_capture?"initialization":
      is_this?(capture->capture_by_reference?"this":"star_this"):
      capture->capture_by_reference?"reference":"copy";
    Rid origin=0;
#if EXTRA_SOURCE_POSITIONS_IN_IL
    origin=roslyn_origin(capture->position,&capture->end_position);
#else
    origin=roslyn_origin(capture->position);
#endif
    DB.put("lambda_capture",{{"owner_id",id},{"ordinal",ordinal++},{"kind_id",K("capture_kind",kind)},
      {"variable_id",R(variable)},{"field_id",R(roslyn_field(capture->closure_field))},
      {"initialization_id",R(initialization)},{"source_closure_field_id",R(source_field)},
      {"indirect_capture_field_id",R(indirect_field)},{"source_origin_id",R(origin)},
      {"is_pack",static_cast<int>(capture->is_pack_expansion)},
      {"is_init_capture",static_cast<int>(capture->is_init_capture)},
      {"is_indirect_init_capture",static_cast<int>(capture->is_indirect_init_capture)},
      {"is_parameter_reference",static_cast<int>(capture->is_param_ref_capture)},
      {"capture_by_reference",static_cast<int>(capture->capture_by_reference)},
      {"is_implicit",static_cast<int>(capture->is_implicit)},
      {"is_pack_element",static_cast<int>(capture->is_pack_element)},
      {"direct_initialization",static_cast<int>(capture->direct_init)},
      {"parenthesized_initialization",static_cast<int>(capture->parenthesized_init)},
      {"const_capture",static_cast<int>(capture->const_capture)}});
  }
  return id;
}

static bool roslyn_extra_expression(an_expr_node_ptr p,int role,Rid id,int kind) {
  switch(p->kind) {
    case enk_lambda: {
      Rid initialization=roslyn_init(p->variant.init.dynamic_init,role);
      DB.put("lambda_expression",{{"id",id},{"lambda_id",roslyn_lambda(p->variant.init.source.lambda)},
        {"initialization_id",R(initialization)}});
      roslyn_child(id,"initialization",0,initialization);
      return true;
    }
    case enk_new_delete: {
      auto supplement=p->variant.new_delete;
      Rid initialization=roslyn_init(supplement->dynamic_init,role);
      Rid exceptional=roslyn_init(supplement->freeing_of_storage_on_exception,7002);
      Rid count=roslyn_expr(supplement->number_of_elements,role);
      Rid operand=supplement->is_new?0:roslyn_expr(supplement->arg,role);
      a_type_ptr allocated=supplement->type?skip_typerefs(supplement->type):nullptr;
      DB.put("allocation_expression",{{"id",id},{"allocated_type_id",R(roslyn_type(supplement->type))},
        {"allocator_id",R(supplement->is_new?roslyn_routine(supplement->routine):0)},
        {"deallocator_id",R(!supplement->is_new?roslyn_routine(supplement->routine):0)},
        {"initialization_id",R(initialization)},{"exception_freeing_id",R(exceptional)},
        {"array_count_id",R(count)},{"operand_id",R(operand)},
        {"is_delete",static_cast<int>(!supplement->is_new)},
        {"is_array",static_cast<int>(supplement->array_delete || (supplement->is_new && allocated && allocated->kind==tk_array))},
        {"is_global",static_cast<int>(supplement->global_new_or_delete)},
        {"is_placement",static_cast<int>(supplement->placement_new)},
        {"is_aligned",static_cast<int>(supplement->aligned_version)},
        {"has_initializer",static_cast<int>(supplement->has_new_initializer)},
        {"brace_initializer",static_cast<int>(supplement->new_initializer_is_brace_enclosed)},
        {"paren_aggregate_initializer",static_cast<int>(supplement->new_initializer_is_paren_aggr_init)},
        {"deducible_type",static_cast<int>(supplement->deducible_type)},
        {"parenthesized_type",static_cast<int>(supplement->parenthesized_type_id)}});
      if(supplement->is_new) {
        int ordinal=0;
        for(an_expr_node_ptr argument=supplement->arg;argument;argument=argument->next) {
          Rid expression=roslyn_expr(argument,role);
          DB.put("allocation_argument",{{"owner_id",id},{"ordinal",ordinal},{"expression_id",expression}});
          roslyn_child(id,"argument",ordinal++,expression);
        }
      }
      roslyn_child(id,"operand",0,operand);roslyn_child(id,"operand",1,count);
      roslyn_child(id,"initialization",0,initialization);roslyn_reference(id,"cleanup",0,exceptional);
      return true;
    }
#if MICROSOFT_EXTENSIONS_ALLOWED
    case enk_gcnew: {
      auto supplement=p->variant.gcnew_info;Rid initialization=roslyn_init(supplement->dynamic_init,role);
      DB.put("allocation_expression",{{"id",id},{"allocated_type_id",R(roslyn_type(supplement->type))},
        {"initialization_id",R(initialization)},{"is_gcnew",1},
        {"is_array",static_cast<int>(supplement->is_cli_array)},
        {"has_initializer",static_cast<int>(supplement->has_new_initializer)}});
      int ordinal=0;
      for(an_expr_node_ptr length=supplement->cli_array_dimension_lengths;length;length=length->next) {
        Rid expression=roslyn_expr(length,role);
        DB.put("allocation_argument",{{"owner_id",id},{"ordinal",ordinal},{"expression_id",expression}});
        roslyn_child(id,"argument",ordinal++,expression);
      }
      roslyn_child(id,"initialization",0,initialization);return true;
    }
#endif
    case enk_typeid: {
      auto &info=p->variant.typeid_info;an_expr_node_ptr type=info.type_with_opt_expr;
      an_expr_node_ptr operand=type?type->next:nullptr;
      Rid expression=roslyn_expr(operand,info.is_dynamic?role:7002);
      DB.put("type_query_expression",{{"id",id},{"query_kind_id",kind},
        {"operand_type_id",R(type?roslyn_type(type->variant.type_operand.type):0)},
        {"operand_expression_id",R(expression)},{"is_dynamic",static_cast<int>(info.is_dynamic)}});
      roslyn_reference(id,"metadata",0,roslyn_expr(type,7002));
      if(info.is_dynamic)roslyn_child(id,"operand",0,expression);
      else roslyn_reference(id,"metadata",1,expression);
      return true;
    }
    case enk_c11_generic: {
      Rid selected=roslyn_expr(p->variant.c11_generic.result,role);
      DB.put("builtin_expression",{{"id",id},{"kind_id",kind},{"selected_expression_id",R(selected)}});
      roslyn_expr_operands(id,p->variant.c11_generic.operands,7002,false);
      roslyn_child(id,"result",0,selected);return true;
    }
#if BUILTIN_FUNCTIONS_ENABLED
    case enk_builtin_choose_expr: {
      auto &info=p->variant.builtin_choose_expr;
      an_expr_node_ptr first=info.operands?info.operands->next:nullptr;
      an_expr_node_ptr selected=info.choose_first?first:(first?first->next:nullptr);
      Rid result=roslyn_expr(selected,role);
      DB.put("builtin_expression",{{"id",id},{"kind_id",kind},{"selected_expression_id",R(result)}});
      roslyn_expr_operands(id,info.operands,7002,false);roslyn_child(id,"result",0,result);return true;
    }
#endif
    case enk_await:case enk_yield: {
      auto &info=p->variant.await_info;an_expr_node_ptr ready=info.ready_resume_suspend;
      an_expr_node_ptr resume=ready?ready->next:nullptr;an_expr_node_ptr suspend=resume?resume->next:nullptr;
      Rid operand=roslyn_expr(info.operand,role),ready_id=roslyn_expr(ready,role);
      Rid resume_id=roslyn_expr(resume,role),suspend_id=roslyn_expr(suspend,role);
      DB.put("coroutine_operation",{{"id",id},{"kind_id",kind},{"operand_id",R(operand)},
        {"ready_id",R(ready_id)},{"resume_id",R(resume_id)},{"suspend_id",R(suspend_id)}});
      roslyn_child(id,"operand",0,operand);roslyn_child(id,"ready",0,ready_id);
      roslyn_child(id,"suspend",0,suspend_id);roslyn_child(id,"resume",0,resume_id);return true;
    }
    case enk_sizeof_pack: {
      auto &info=p->variant.sizeof_pack;Rid type=0,templ=0,expression=0;
      if(info.is_template_template)templ=roslyn_template(info.variant.templ);
      else if(info.is_type)type=roslyn_type(info.variant.type);
      else expression=roslyn_expr(info.variant.expr,7002);
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"pack_type_id",R(type)},
        {"pack_template_id",R(templ)},{"wrapped_expression_id",R(expression)}});
      roslyn_reference(id,"metadata",0,expression);return true;
    }
    case enk_fold: {
      auto &info=p->variant.fold;
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},
        {"fold_operator_id",roslyn_kind_token(info.operator_token)},
        {"fold_left_associative",static_cast<int>(info.left_associative)}});
      roslyn_expr_operands(id,info.operands,7002,false);return true;
    }
    case enk_pack_index: {
      Rid expression=roslyn_expr(p->variant.pack_index.expr,7002);
      Rid index=roslyn_expr(p->variant.pack_index.index_expr,7002);
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},
        {"wrapped_expression_id",R(expression)},{"pack_index_id",R(index)}});
      roslyn_reference(id,"metadata",0,expression);roslyn_reference(id,"metadata",1,index);return true;
    }
    case enk_concept_id: {
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},
        {"template_id",R(roslyn_template(p->variant.concept_id.concept_template))}});
      int ordinal=0;
      for(a_template_arg_ptr arg=p->variant.concept_id.args;arg;) {
        DB.put("expression_template_argument",{{"owner_id",id},{"ordinal",ordinal++},{"argument_id",roslyn_argument(arg)}});
        bool pack=arg->kind==tak_start_of_pack_expansion;arg=arg->next;
        if(pack)while(arg && arg->is_pack_element)arg=arg->next;
      }
      return true;
    }
    case enk_requires: {
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind}});
      int ordinal=0;
      for(a_param_type_ptr parameter=p->variant.requires_expr.parameters;parameter;parameter=parameter->next)
        DB.put("requirement_parameter",{{"owner_id",id},{"ordinal",ordinal++},{"parameter_type_id",roslyn_parameter_type(parameter)}});
      ordinal=0;
      for(an_expr_node_ptr requirement=p->variant.requires_expr.requirements;requirement;requirement=requirement->next) {
        Rid expression=roslyn_expr(requirement,7002);
        DB.put("dependent_requirement",{{"owner_id",id},{"ordinal",ordinal++},{"expression_id",expression}});
      }
      return true;
    }
    case enk_compound_req: {
      Rid expression=roslyn_expr(p->variant.compound_req.expr_and_constraint,7002);
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"wrapped_expression_id",R(expression)},
        {"noexcept_required",static_cast<int>(p->variant.compound_req.is_noexcept)}});
      roslyn_expr_operands(id,p->variant.compound_req.expr_and_constraint,7002,false);return true;
    }
    case enk_nested_req: {
      Rid expression=roslyn_expr(p->variant.nested_req.constraint,7002);
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"wrapped_expression_id",R(expression)}});
      roslyn_reference(id,"metadata",0,expression);return true;
    }
    case enk_braced_init_list:
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind}});
      roslyn_expr_operands(id,p->variant.braced_init_list,7002,false);return true;
    case enk_initializer: {
      Rid initialization=roslyn_init(p->variant.initializer.dyn_init,7002);
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"initialization_id",R(initialization)}});
      roslyn_reference(id,"metadata",0,initialization);return true;
    }
    case enk_const_eval_deferred: {
      auto &info=p->variant.const_eval_deferred;Rid expression=roslyn_expr(info.wrapped,7002);
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},{"wrapped_expression_id",R(expression)},
        {"reattempt_default_argument",static_cast<int>(info.reattempt_state.default_arg)},
        {"reattempt_default_member_initializer",static_cast<int>(info.reattempt_state.default_mem_init)}});
      roslyn_reference(id,"metadata",0,expression);return true;
    }
    case enk_template_name:
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},
        {"template_id",R(roslyn_template(p->variant.template_name))}});return true;
    case enk_token_sequence:
      DB.put("dependent_expression",{{"id",id},{"kind_id",kind},
        {"token_sequence_id",R(roslyn_token_sequence(p->variant.token_sequence.tokens))}});
      roslyn_expr_operands(id,p->variant.token_sequence.interpolations,7002,false);return true;
    default:return false;
  }
}
