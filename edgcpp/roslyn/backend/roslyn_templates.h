/* Retained template IL projection. Included inside the EDG namespace.
   Front-end symbols, rescans, substitution worklists and token caches are excluded.
   Field authority: pinned upstream il_def.h:7683-7842,17150-17441. */

static a_template_ptr roslyn_header_template(a_template_decl_ptr header,
                                            a_template_ptr context) {
  if (!header) return nullptr;
  if (context && context->template_decl==header) return context;
  std::vector<a_template_ptr> pending;
  std::map<a_template_ptr,bool> seen;
  if (context) pending.push_back(context);
  for (a_scope_ptr scope=header->scope;scope;scope=scope->parent)
    for (a_template_ptr t=scope->templates;t;t=t->next) pending.push_back(t);
  for (a_template_ptr t=il_header.primary_scope->templates;t;t=t->next)
    pending.push_back(t);
  while (!pending.empty()) {
    a_template_ptr t=pending.back();pending.pop_back();
    if (!t || !seen.emplace(t,true).second) continue;
    if (t->template_decl==header) return t;
    if (t->canonical_template) pending.push_back(t->canonical_template);
    if (t->definition_template) pending.push_back(t->definition_template);
    if (t->prototype_template) pending.push_back(t->prototype_template);
    if (t->source_corresp.enclosing_routine)
      pending.push_back(t->source_corresp.enclosing_routine->assoc_template);
    for (a_scope_ptr scope=t->source_corresp.parent_scope;scope;scope=scope->parent) {
      if (scope->kind==sck_function)
        pending.push_back(scope->variant.routine.ptr->assoc_template);
      if (scope->kind==sck_class_struct_union) {
        a_type_ptr type=scope->variant.assoc_type;
        if (type && (type->kind==tk_class || type->kind==tk_struct || type->kind==tk_union))
          pending.push_back(type->variant.class_struct_union.extra_info->assoc_template);
      }
    }
  }
  return nullptr;
}

static Rid roslyn_default_template_argument(a_templ_arg_kind kind,Rid value) {
  if (!value) return 0;
  Rid id=DB.fresh("template_argument");
  DB.put("template_argument",{{"id",id},{"kind_id",roslyn_kind_template_argument(kind)},
    {"type_id",R(kind==tak_type?value:0)},{"constant_id",R(kind==tak_nontype?value:0)},
    {"template_id",R(kind==tak_template?value:0)}});
  return id;
}

static Rid roslyn_template_header(a_template_decl_ptr p,a_template_ptr context) {
  bool fresh;Rid id=DB.claim(p,"template_header",fresh);if(!fresh)return id;
  a_template_ptr owner=roslyn_header_template(p,context);
  Rid owner_id=roslyn_template(owner);
  Rid parent=roslyn_template_header(p->parent,owner?owner:context);
  bool generic=false;
#if MICROSOFT_EXTENSIONS_ALLOWED
  generic=p->is_generic;
#endif
  Rid constraint=0,requires_position=0;
  if(!generic && p->constraint.requires_clause) {
    constraint=roslyn_expr(p->constraint.requires_clause->constraint,7002);
    requires_position=roslyn_position(p->constraint.requires_clause->requires_pos);
  }
  DB.put("template_header",{{"id",id},{"template_id",R(owner_id)},{"parent_id",R(parent)},
    {"scope_id",R(roslyn_scope(p->scope))},{"source_origin_id",R(roslyn_origin(p->template_pos))},
    {"is_generic",static_cast<int>(generic)},{"constraint_id",R(constraint)},
    {"requires_position_id",R(requires_position)}});
#if MICROSOFT_EXTENSIONS_ALLOWED
  if(generic) {
    int clause_ordinal=0;
    for(a_generic_constraint_clause_ptr clause=p->constraint.where_clauses;clause;clause=clause->next,++clause_ordinal) {
      bool clause_fresh;Rid clause_id=DB.claim(clause,"generic_constraint_clause",clause_fresh);
      if(!clause_fresh)continue;
      Rid where=0,colon=0;
#if EXTRA_SOURCE_POSITIONS_IN_IL
      where=roslyn_position(clause->where_position);colon=roslyn_position(clause->colon_position);
#endif
      DB.put("template_generic_constraint_clause",{{"id",clause_id},{"header_id",id},
        {"ordinal",clause_ordinal},{"parameter_type_id",roslyn_type(clause->type)},
        {"type_position_id",R(roslyn_position(clause->type_position))},
        {"where_position_id",R(where)},{"colon_position_id",R(colon)}});
      int item_ordinal=0;
      for(a_generic_constraint_ptr item=clause->constraints;item;item=item->next) {
        const char *kind=nullptr;
        switch(item->kind) {
          case gck_none:kind="gck_none";break;case gck_type:kind="gck_type";break;
          case gck_naked_type_param:kind="gck_naked_type_param";break;
          case gck_ref_class:kind="gck_ref_class";break;case gck_value_class:kind="gck_value_class";break;
          case gck_gcnew:kind="gck_gcnew";break;case gck_fail:kind="gck_fail";break;
        }
        if(!kind){DB.fail("Unhandled retained generic constraint kind");continue;}
        DB.put("template_generic_constraint",{{"owner_id",clause_id},{"ordinal",item_ordinal++},
          {"kind_id",K("generic_constraint",kind)},
          {"type_id",R(item->kind==gck_type || item->kind==gck_naked_type_param?roslyn_type(item->type):0)},
          {"is_implicit",static_cast<int>(item->implicit_constraint)},
          {"source_position_id",R(roslyn_position(item->position))}});
      }
    }
  }
#endif
  int ordinal=0;
  for(a_template_parameter_ptr parameter=p->param_list;parameter;parameter=parameter->next,++ordinal) {
    bool parameter_fresh;Rid parameter_id=DB.claim(parameter,"template_parameter",parameter_fresh);
    if(!parameter_fresh)continue;
    Rid type=0,constant=0,templ=0,default_argument=0,constraint=0,symbol_type=0;
    int depth=0;
    switch(parameter->kind) {
      case tpk_type:
        type=roslyn_type(parameter->variant.type.ptr);
        symbol_type=type;
        default_argument=roslyn_default_template_argument(tak_type,roslyn_type(parameter->variant.type.default_arg_type));
        if(parameter->variant.type.ptr && parameter->variant.type.ptr->kind==tk_template_param) {
          auto supplement=parameter->variant.type.ptr->variant.template_param.extra_info;
          depth=supplement->coordinates.depth;
          if(depth!=CLASS_TEMPLATE_PLACEHOLDER_NESTING_DEPTH && depth!=BIT_PRECISE_INT_NESTING_DEPTH)
            constraint=roslyn_expr(supplement->constraint.type_constraint,7002);
        }
        break;
      case tpk_nontype:
        constant=roslyn_constant(parameter->variant.nontype.constant);
        symbol_type=roslyn_type(parameter->variant.nontype.constant?parameter->variant.nontype.constant->type:nullptr);
        default_argument=roslyn_default_template_argument(tak_nontype,roslyn_constant(parameter->variant.nontype.default_arg_constant));
        if(parameter->variant.nontype.constant && parameter->variant.nontype.constant->kind==ck_template_param
            && parameter->variant.nontype.constant->variant.template_param.kind==tpck_param)
          depth=parameter->variant.nontype.constant->variant.template_param.variant.coordinates.depth;
        break;
      case tpk_template:
        templ=roslyn_template(parameter->variant.templ.class_template);
        default_argument=roslyn_default_template_argument(tak_template,roslyn_template(parameter->variant.templ.default_arg_template));
        if(parameter->variant.templ.class_template)depth=parameter->variant.templ.class_template->coordinates.depth;
        break;
      case tpk_error:DB.fail("Error template parameter in successful retained IL");break;
    }
    roslyn_symbol(parameter_id,"template_parameter",parameter->source_corresp,symbol_type,
                  0,0,true,!parameter->is_abbreviated);
    DB.put("template_parameter",{{"id",parameter_id},{"template_id",R(owner_id)},{"header_id",id},
      {"ordinal",ordinal},{"depth",depth},{"kind_id",roslyn_kind_template_parameter(parameter->kind)},
      {"is_pack",static_cast<int>(parameter->is_pack)},
      {"is_abbreviated",static_cast<int>(parameter->is_abbreviated)},
      {"placeholder_type_id",R(type)},{"placeholder_constant_id",R(constant)},
      {"placeholder_template_id",R(templ)},{"default_argument_id",R(default_argument)},
      {"constraint_id",R(constraint)}});
  }
  return id;
}

static Rid roslyn_template(a_template_ptr p) {
  bool fresh;Rid id=DB.claim(p,"template",fresh);if(!fresh)return id;
  // Claim the header before following prototype bodies, whose members can point back.
  roslyn_template_header(p->template_decl,p);
  Rid pattern=0,constraint=0,symbol_type=0;
  switch(p->kind) {
    case templk_function:case templk_member_function:
      pattern=roslyn_routine(p->prototype_instantiation.routine);
      symbol_type=roslyn_type(p->prototype_instantiation.routine?p->prototype_instantiation.routine->type:nullptr);break;
    case templk_class:case templk_member_class:case templk_member_enum:
      pattern=roslyn_type(p->prototype_instantiation.type);symbol_type=pattern;break;
    case templk_variable:case templk_static_data_member:
      pattern=roslyn_variable(p->prototype_instantiation.variable);
      symbol_type=roslyn_type(p->prototype_instantiation.variable?p->prototype_instantiation.variable->type:nullptr);break;
    case templk_concept:
      constraint=roslyn_expr(p->prototype_instantiation.constraint,7002);pattern=constraint;
      symbol_type=roslyn_type(p->prototype_instantiation.constraint?p->prototype_instantiation.constraint->type:nullptr);break;
    case templk_none:case templk_template_template_param:break;
  }
  roslyn_symbol(id,"template",p->source_corresp,symbol_type);
  Rid export_position=0,definition_range=0,definition_text=0;
#if RECORD_TEMPLATE_STRINGS
  definition_text=DB.atom(p->text);
#endif
#if EXTRA_SOURCE_POSITIONS_IN_IL
  export_position=roslyn_position(p->export_position);
  if(p->definition_range.start.seq || p->definition_range.start.column) {
    definition_range=DB.fresh("source_range");
    DB.put("source_range",{{"id",definition_range},{"start_id",roslyn_position(p->definition_range.start)},
      {"end_id",roslyn_position(p->definition_range.end)},{"endpoint_kind_id",K("range_endpoint","inclusive")}});
  }
#endif
  DB.put("template_definition",{{"id",id},{"kind_id",roslyn_kind_template(p->kind)},
    {"pattern_id",R(pattern)},{"constraint_id",R(constraint)},
    {"canonical_template_id",R(roslyn_template(p->canonical_template))},
    {"definition_template_id",R(roslyn_template(p->definition_template))},
    {"prototype_template_id",R(roslyn_template(p->prototype_template))},
    {"is_exported",static_cast<int>(p->is_exported)},{"ignore_export",static_cast<int>(p->ignore_export)},
    {"is_pack",static_cast<int>(p->is_pack)},{"is_friend_template",static_cast<int>(p->is_friend_template)},
    {"parameter_number",p->kind==templk_template_template_param?RoslynSqlValue(static_cast<sqlite3_int64>(p->coordinates.position)):RoslynSqlValue()},
    {"parameter_depth",p->kind==templk_template_template_param?RoslynSqlValue(p->coordinates.depth):RoslynSqlValue()},
    {"definition_text_id",R(definition_text)},
    {"export_position_id",R(export_position)},{"definition_range_id",R(definition_range)}});
  return id;
}

static Rid roslyn_argument(a_template_arg_ptr p) {
  bool fresh;Rid id=DB.claim(p,"template_argument",fresh);if(!fresh)return id;
  Rid type=0,constant=0,templ=0;
  switch(p->kind) {
    case tak_type:type=roslyn_type(p->variant.type);break;
    case tak_nontype:
      if(!p->is_array_bound_of_unknown_type)constant=roslyn_constant(p->variant.constant);
      break;
    case tak_template:templ=roslyn_template(p->variant.templ.ptr);break;
    case tak_start_of_pack_expansion:break;
  }
  DB.put("template_argument",{{"id",id},{"kind_id",roslyn_kind_template_argument(p->kind)},
    {"type_id",R(type)},{"constant_id",R(constant)},{"template_id",R(templ)},
    {"is_pack",static_cast<int>(p->is_pack || p->kind==tak_start_of_pack_expansion)},
    {"is_explicit",static_cast<int>(p->explicitly_specified)},
    {"is_pack_element",static_cast<int>(p->is_pack_element)},
    {"is_ellipsis",static_cast<int>(p->has_pack_ellipsis)},
    {"is_integer_pack",static_cast<int>(p->is_integer_pack)},
    {"is_injected_name",static_cast<int>(p->type_is_injected_class_name)},
    {"is_provisional",static_cast<int>(p->is_provisional_value)},
    {"is_error",static_cast<int>(p->is_error)},
    {"is_unknown_array_bound",static_cast<int>(p->is_array_bound_of_unknown_type)},
    {"deduced_array_bound",p->kind==tak_nontype && p->is_array_bound_of_unknown_type
        ? RoslynSqlValue(Q(p->variant.integer_value)) : RoslynSqlValue()}});
  if(p->kind==tak_start_of_pack_expansion) {
    int ordinal=0;
    for(a_template_arg_ptr element=p->next;element && element->is_pack_element;element=element->next)
      DB.put("template_argument_pack_element",{{"owner_id",id},{"ordinal",ordinal++},{"argument_id",roslyn_argument(element)}});
  }
  return id;
}

static void roslyn_template_arguments(Rid instance,const char *role,a_template_arg_ptr arguments) {
  int ordinal=0;
  for(a_template_arg_ptr argument=arguments;argument;) {
    DB.put("template_instance_argument",{{"owner_id",instance},{"role_id",K("template_argument_role",role)},
      {"ordinal",ordinal++},{"argument_id",roslyn_argument(argument)}});
    bool pack=argument->kind==tak_start_of_pack_expansion;
    argument=argument->next;
    if(pack)while(argument && argument->is_pack_element)argument=argument->next;
  }
}

static Rid roslyn_instance(const void *pointer,Rid result,a_template_ptr templ,
                           a_template_arg_ptr primary,a_template_arg_ptr partial) {
  bool fresh;Rid id=DB.claim(pointer,"template_instance",fresh);if(!fresh)return id;
  bool dependent=false,prototype=false,suppressed=false,explicit_instance=false,specialized=false;
  a_source_correspondence_ptr source=nullptr;
  if(DB.existing(pointer,"routine")) {
    a_routine_ptr routine=static_cast<a_routine_ptr>(const_cast<void*>(pointer));
    source=&routine->source_corresp;
    prototype=routine->is_prototype_instantiation;dependent=prototype;
    suppressed=routine->explicit_do_not_instantiate;
    explicit_instance=routine->explicit_instantiation;specialized=routine->is_specialized;
  } else if(DB.existing(pointer,"variable")) {
    a_variable_ptr variable=static_cast<a_variable_ptr>(const_cast<void*>(pointer));
    source=&variable->source_corresp;
    prototype=variable->is_prototype_instantiation;dependent=prototype || variable->is_nonreal;
    suppressed=variable->explicit_do_not_instantiate;
    explicit_instance=variable->explicit_instantiation;specialized=variable->is_specialized;
  } else if(DB.existing(pointer,"type")) {
    a_type_ptr type=static_cast<a_type_ptr>(const_cast<void*>(pointer));source=&type->source_corresp;
    if(type->kind==tk_class || type->kind==tk_struct || type->kind==tk_union) {
      prototype=type->variant.class_struct_union.is_prototype_instantiation;
      dependent=prototype || type->variant.class_struct_union.is_nonreal_class;
      suppressed=type->variant.class_struct_union.do_not_instantiate;
      explicit_instance=type->variant.class_struct_union.explicitly_instantiated;
      specialized=type->variant.class_struct_union.is_specialized;
    } else if(type->kind==tk_typeref) {
      prototype=type->variant.typeref.is_prototype_instantiation;
      dependent=type->variant.typeref.is_dependent || type->variant.typeref.is_nonreal;
    } else if(type->kind==tk_integer && type->variant.integer.enum_type) {
      prototype=type->variant.integer.is_prototype_instantiation;
      dependent=prototype || type->variant.integer.is_nonreal;
      specialized=type->variant.integer.is_specialized;
    }
  }
  Rid template_id=roslyn_template(templ),specialization=0;
  // The retained flag also covers do-not-instantiate directives. Its exact
  // relationship is recorded without assigning explicit-source syntax.
  if(specialized && template_id) {
    specialization=DB.fresh("template_specialization");
    DB.put("template_specialization",{{"id",specialization},{"primary_template_id",template_id},
      {"entity_id",result},{"kind_id",K("specialization_kind","retained_specialization")}});
  }
  DB.put("template_instance",{{"id",id},{"result_id",result},{"template_id",R(template_id)},
    {"selected_specialization_id",R(specialization)},
    {"state_id",K("instantiation_state",dependent?"dependent":suppressed?"suppressed":"concrete")},
    {"source_position_id",R(source?roslyn_position(source->decl_position):0)},
    {"is_dependent",static_cast<int>(dependent)},{"is_prototype",static_cast<int>(prototype)},
    {"is_suppressed",static_cast<int>(suppressed)},{"is_explicit",static_cast<int>(explicit_instance)},
    {"is_specialized",static_cast<int>(specialized)}});
  roslyn_template_arguments(id,"primary",primary);
  roslyn_template_arguments(id,"selected_partial",partial);
  return id;
}
