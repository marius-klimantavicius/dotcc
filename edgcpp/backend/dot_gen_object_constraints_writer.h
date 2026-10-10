std::set<unsigned> written_source_pack_expansions;
std::vector<string> source_constraint_argument_details,source_pack_expansions,source_pack_references;
std::vector<string> source_dependent_function_lookups,source_dependent_function_candidates,source_dependent_function_arguments;
unsigned source_constraint_expansion(unsigned id) {
  if(!id || !written_source_pack_expansions.insert(id).second)return id;
  const auto value=dot_object_source::source_constraint_graph().expansions.at(id-1);
  if(!value.error.empty())reject(value.error.c_str());
  source_pack_expansions.push_back("{\"id\":"+number(id)+",\"referenceCount\":"+std::to_string(value.references.size())+
    ",\"ellipsisSeen\":"+(value.ellipsis?"true":"false")+",\"usesOnlyEnclosingPacks\":"+(value.enclosing_only?"true":"false")+
    ",\"usesAnyEnclosingPacks\":"+(value.enclosing_any?"true":"false")+",\"isFunctionDeclarator\":"+(value.function_declarator?"true":"false")+
    ",\"isPackIndex\":"+(value.pack_index?"true":"false")+",\"parameterIsTemplateTemplate\":"+(value.template_template?"true":"false")+
    ",\"parameterSourceTypeId\":"+number(value.parameter_type?source_type(value.parameter_type):0)+position(value.location)+
    ",\"captureProof\":\"original-pack-expansion\"}");
  unsigned ordinal=0;for(const auto &reference:value.references)
    source_pack_references.push_back("{\"expansionId\":"+number(id)+",\"ordinal\":"+std::to_string(ordinal++)+
      ",\"kind\":"+quote(reference.kind)+",\"sourceTypeId\":"+number(reference.type?source_type(reference.type):0)+
      ",\"constantId\":"+number(reference.constant?source_semantic_constant(reference.constant):0)+
      ",\"templateId\":"+number(reference.templ?source_template_reference(reference.templ):0)+
      ",\"parameterDepth\":"+(reference.depth>=0?std::to_string(reference.depth):"null")+
      ",\"parameterPosition\":"+(reference.position>=0?std::to_string(reference.position):"null")+
      ",\"usesEnclosingPack\":"+(reference.enclosing?"true":"false")+position(reference.location)+"}");
  return id;
}
void source_constraint_argument_details_row(const char *owner,unsigned id,unsigned ordinal,unsigned substitution,
    const dot_object_source::SourceConstraintArgument &value) {
  source_constraint_argument_details.push_back("{\"ownerKind\":"+quote(owner)+",\"ownerId\":"+number(id)+
    ",\"argumentOrdinal\":"+std::to_string(ordinal)+",\"substitutionOrdinal\":"+std::to_string(substitution)+
    ",\"isPack\":"+(value.pack?"true":"false")+",\"isPackElement\":"+(value.pack_element?"true":"false")+
    ",\"hasPackEllipsis\":"+(value.ellipsis?"true":"false")+",\"isIntegerPack\":"+(value.integer_pack?"true":"false")+
    ",\"isExplicit\":"+(value.explicit_argument?"true":"false")+",\"isInjectedClassName\":"+(value.injected_class?"true":"false")+
    ",\"parameterIsAuto\":"+(value.auto_parameter?"true":"false")+",\"parameterIsDecltypeAuto\":"+(value.decltype_auto_parameter?"true":"false")+
    ",\"packExpansionId\":"+number(source_constraint_expansion(value.expansion))+
    ",\"substitutedTemplateId\":"+number(value.substituted_template?source_template_reference(value.substituted_template):0)+
    ",\"captureProof\":\"original-template-argument\"}");
}
// Included inside Writer. Source-only graph traversal never requests codegen
// entities for an unevaluated expression, nor supplies a fabricated runtime ABI.
std::set<unsigned> written_source_expressions,written_source_concepts,written_source_declarations;
std::vector<string> source_expressions,source_expression_edges,source_type_constraints;
std::vector<string> source_expression_declarations,source_expression_declaration_formals,source_expression_declaration_arguments,source_expression_declaration_references;
std::vector<string> source_concepts,source_concept_parameters,source_concept_uses,source_concept_arguments;
std::vector<string> source_expression_parameters,source_expression_parameter_references;
std::vector<string> source_expression_operations,source_expression_flags;
std::vector<string> source_expression_type_operands,source_expression_constants;
std::vector<string> source_expression_requirements,source_expression_bindings,source_expression_queries;
std::vector<string> source_expression_substitutions,source_expression_substitution_formals,source_expression_substitution_arguments;
std::set<unsigned> written_source_parameter_binders;
std::vector<string> source_parameter_binders,source_binder_parameters;
unsigned source_parameter_binder(unsigned id) {
  if(!id || !written_source_parameter_binders.insert(id).second)return id;
  const auto &value=dot_object_source::source_constraint_graph().binders.at(id);
  source_parameter_binders.push_back("{\"id\":"+number(id)+",\"kind\":"+quote(value.expression?"requires":"routine-prototype")+
    ",\"parentBinderId\":"+number(source_parameter_binder(value.parent))+
    ",\"requiresExpressionId\":"+number(value.expression?source_constraint_expression(value.expression):0)+
    ",\"routineSourceTypeId\":"+number(value.routine?source_type(value.routine):0)+
    ",\"outsideParameterList\":"+(value.outside?"true":"false")+
    ",\"parameterCount\":"+std::to_string(value.parameters.size())+",\"captureProof\":\"original-parameter-scope\"}");
  for(const auto &parameter:value.parameters)source_binder_parameters.push_back("{\"binderId\":"+number(id)+
    ",\"ordinal\":"+std::to_string(parameter.ordinal)+",\"sourceTypeId\":"+number(source_type(parameter.type))+
    ",\"declaredSourceTypeId\":"+number(parameter.declared_type?source_type(parameter.declared_type):0)+
    ",\"sourceName\":"+quote(parameter.name)+",\"qualifierBits\":"+std::to_string(parameter.qualifiers)+
    ",\"isPack\":"+(parameter.pack?"true":"false")+"}");
  return id;
}


string constraint_formal_row(const char *owner_name,unsigned owner,unsigned ordinal,
                             const dot_object_source::SourceTemplateFormal &formal) {
  return "{"+quote(owner_name)+":"+number(owner)+",\"ordinal\":"+std::to_string(ordinal)+
    ",\"kind\":"+quote(formal.kind)+",\"sourceName\":"+quote(formal.name)+
    ",\"typeId\":"+number(formal.type?source_type(formal.type):0)+
    ",\"constantId\":"+number(formal.constant?source_semantic_constant(formal.constant):0)+
    ",\"templateId\":"+number(formal.templ?source_template_reference(formal.templ):0)+
    ",\"defaultTypeId\":"+number(formal.default_type?source_type(formal.default_type):0)+
    ",\"defaultConstantId\":"+number(formal.default_constant?source_semantic_constant(formal.default_constant):0)+
    ",\"defaultTemplateId\":"+number(formal.default_template?source_template_reference(formal.default_template):0)+
    ",\"isPack\":"+(formal.pack?"true":"false")+
    ",\"isAbbreviated\":"+(formal.abbreviated?"true":"false")+position(formal.position)+"}";
}
std::set<a_type_ptr> written_source_class_headers,written_source_class_specializations;
std::vector<string> source_class_template_headers,source_class_template_formals,source_class_template_patterns,source_class_specializations,source_class_specialization_arguments;
string source_class_argument_row(unsigned owner,unsigned ordinal,const dot_object_source::SourceTemplateArgument &argument) {
  return "{\"sourceRecordId\":"+number(owner)+",\"ordinal\":"+std::to_string(ordinal)+",\"kind\":"+quote(argument.kind)+
    ",\"typeId\":"+number(argument.type?source_type(argument.type):0)+",\"constantId\":"+number(argument.constant?source_semantic_constant(argument.constant):0)+
    ",\"templateId\":"+number(argument.templ?source_template_reference(argument.templ):0)+",\"isPack\":"+(argument.pack?"true":"false")+"}";
}
unsigned source_class_header(a_type_ptr type) {
  auto &graph=dot_object_source::source_constraint_graph();auto entry=graph.class_template_headers.find(type);
  if(entry==graph.class_template_headers.end())reject("source class template header lacks its original capture");
  if(!written_source_class_headers.insert(type).second)return source_record_metadata(type);
  const auto value=entry->second;if(!value.error.empty())reject(value.error.c_str());
  unsigned record=source_record_metadata(type);
  unsigned primary=value.primary?source_class_header(dot_object_source::state().types.at(value.primary-1).record_type):0;
  source_class_template_headers.push_back("{\"sourceRecordId\":"+number(record)+",\"primarySourceRecordId\":"+number(primary)+
    ",\"formalCount\":"+std::to_string(value.formals.size())+",\"patternCount\":"+std::to_string(value.pattern.size())+
    ",\"constraintExpressionId\":"+number(value.constraint?source_constraint_expression(value.constraint):0)+",\"captureProof\":\"original-class-template-header\"}");
  unsigned ordinal=0;for(const auto &formal:value.formals)source_class_template_formals.push_back(constraint_formal_row("sourceRecordId",record,ordinal++,formal));
  ordinal=0;for(const auto &argument:value.pattern) {
    source_constraint_argument_details_row("class-pattern",record,ordinal,0,argument);
    source_class_template_patterns.push_back(source_class_argument_row(record,ordinal++,argument));
  }
  return record;
}
void source_class_specialization_metadata(a_type_ptr type) {
  auto &graph=dot_object_source::source_constraint_graph();
  if(graph.class_template_headers.count(type))source_class_header(type);
  auto found=graph.class_specializations.find(type);
  if(found==graph.class_specializations.end() || !written_source_class_specializations.insert(type).second)return;
  const auto value=found->second;if(!value.error.empty())reject(value.error.c_str());
  if(!value.selected)reject("source partial class instance has no selected header");
  unsigned record=source_record_metadata(type),selected=source_class_header(dot_object_source::state().types.at(value.selected-1).record_type);
  source_class_specializations.push_back("{\"sourceRecordId\":"+number(record)+",\"selectedSourceRecordId\":"+number(selected)+
    ",\"selectedArgumentCount\":"+std::to_string(value.arguments.size())+",\"captureProof\":\"original-partial-class-substitution\"}");
  unsigned ordinal=0;for(const auto &argument:value.arguments) {
    source_constraint_argument_details_row("class-selected",record,ordinal,0,argument);
    source_class_specialization_arguments.push_back(source_class_argument_row(record,ordinal++,argument));
  }
}
std::vector<string> source_declaration_operators,source_function_parameter_declarations;
std::vector<string> source_local_scopes,source_local_variable_declarations;
std::set<unsigned> written_source_local_scopes;
unsigned source_local_scope(unsigned id) {
  if(!id)reject("source local scope is absent");
  if(!written_source_local_scopes.insert(id).second)return id;
  const auto value=dot_object_source::source_constraint_graph().local_scopes.at(id-1);
  if(!value.error.empty())reject(value.error.c_str());
  source_local_scopes.push_back("{\"id\":"+number(id)+",\"routineDeclarationId\":"+number(source_constraint_declaration(value.routine))+
    ",\"parentId\":"+number(value.parent?source_local_scope(value.parent):0)+",\"kind\":"+quote(value.kind)+
    ",\"siblingOrdinal\":"+std::to_string(value.ordinal)+",\"childScopeCount\":"+std::to_string(value.child_count)+
    ",\"variableCount\":"+std::to_string(value.variable_count)+",\"captureProof\":\"original-local-scope-tree\""+position(value.position)+"}");
  return id;
}
unsigned source_constraint_declaration(unsigned captured) {
  if(!captured)reject("source expression declaration is absent");
  if(!written_source_declarations.insert(captured).second)return captured;
  const auto value=dot_object_source::source_constraint_graph().declarations.at(captured-1);
  if(!value.error.empty())reject(value.error.c_str());
  source_namespace_owner("declaration",captured,value.namespace_owner);
  source_expression_declarations.push_back("{\"id\":"+number(captured)+",\"kind\":"+quote(value.kind)+
    ",\"sourceName\":"+quote(value.name)+",\"qualifiedName\":"+quote(value.qualified_name)+
    ",\"sourceTypeId\":"+number(source_type(value.type))+
    ",\"ownerSourceTypeId\":"+number(value.owner_type?source_type(value.owner_type):0)+
    ",\"templatePrototypeDeclarationId\":"+number(value.template_prototype?source_constraint_declaration(value.template_prototype):0)+
    ",\"scopeKind\":"+quote(value.parameter_owner?"function-parameter":value.local_scope?"function-local":value.namespace_owner?"namespace":"nonlocal")+
    ",\"routineKind\":"+quote(value.routine_kind)+",\"templateOriginKind\":"+quote(value.template_origin_kind)+
    ",\"hasTemplateOrigin\":"+(value.has_template_origin?"true":"false")+
    ",\"isTemplatePrototype\":"+(value.is_template_prototype?"true":"false")+
    ",\"isConstexpr\":"+(value.is_constexpr?"true":"false")+
    ",\"originalInitKind\":"+(value.original_init_kind>=0?std::to_string(value.original_init_kind):"null")+
    ",\"originalDynamicInitKind\":"+(value.original_dynamic_init_kind>=0?std::to_string(value.original_dynamic_init_kind):"null")+
    ",\"initializerKind\":"+quote(value.initializer_kind)+
    ",\"initializerConstantId\":"+number(value.initializer_constant?source_semantic_constant(value.initializer_constant):0)+
    ",\"initializerExpressionId\":"+number(value.initializer_expression?source_constraint_expression(value.initializer_expression):0)+
    ",\"formalCount\":"+std::to_string(value.template_formals.size())+",\"argumentCount\":"+std::to_string(value.template_arguments.size())+
    ",\"definitionStatus\":\"reference-only\",\"captureProof\":\"original-expression-declaration\""+position(value.location)+"}");
  if(value.parameter_owner)source_function_parameter_declarations.push_back("{\"declarationId\":"+number(captured)+
    ",\"routineDeclarationId\":"+number(source_constraint_declaration(value.parameter_owner))+
    ",\"binderId\":"+number(source_parameter_binder(value.parameter_binder))+
    ",\"parameterOrdinal\":"+std::to_string(value.parameter_ordinal)+
    ",\"declaredSourceTypeId\":"+number(value.parameter_declared_type?source_type(value.parameter_declared_type):0)+
    ",\"captureProof\":\"original-function-parameter-list\"}");
  if(value.local_scope)source_local_variable_declarations.push_back("{\"declarationId\":"+number(captured)+
    ",\"scopeId\":"+number(source_local_scope(value.local_scope))+",\"declarationOrdinal\":"+std::to_string(value.local_ordinal)+
    ",\"declaredSourceTypeId\":"+number(value.local_declared_type?source_type(value.local_declared_type):0)+
    ",\"captureProof\":\"original-nonstatic-variable-list\"}");
  if(value.operator_kind>=0)source_declaration_operators.push_back("{\"declarationId\":"+number(captured)+",\"operatorKind\":"+std::to_string(value.operator_kind)+",\"captureProof\":\"original-operator-declaration\"}");
  unsigned ordinal=0;for(const auto &formal:value.template_formals)
    source_expression_declaration_formals.push_back(constraint_formal_row("declarationId",captured,ordinal++,formal));
  ordinal=0;for(const auto &argument:value.template_arguments) {
    source_constraint_argument_details_row("declaration",captured,ordinal,0,argument);
    string kind=argument.kind;
    source_expression_declaration_arguments.push_back("{\"declarationId\":"+number(captured)+",\"ordinal\":"+std::to_string(ordinal++)+
      ",\"kind\":"+quote(kind)+",\"sourceTypeId\":"+number(argument.type?source_type(argument.type):0)+
      ",\"constantId\":"+number(argument.constant?source_semantic_constant(argument.constant):0)+
      ",\"templateId\":"+number(argument.templ?source_template_reference(argument.templ):0)+"}");
  }
  return captured;
}
unsigned source_constraint_concept(unsigned captured) {
  if(!captured)reject("source concept reference is absent");
  if(!written_source_concepts.insert(captured).second)return captured;
  const auto value=dot_object_source::source_constraint_graph().concepts.at(captured-1);
  if(!value.error.empty())reject(value.error.c_str());
  source_namespace_owner("concept",captured,value.namespace_owner);
  source_concepts.push_back("{\"id\":"+number(captured)+",\"sourceName\":"+quote(value.name)+
    ",\"qualifiedName\":"+quote(value.qualified_name)+
    ",\"hasNamespaceOwner\":"+(value.namespace_owner?"true":"false")+
    ",\"parameterCount\":"+std::to_string(value.formals.size())+
    ",\"definitionExpressionId\":"+number(source_constraint_expression(value.definition))+
    ",\"headerConstraintExpressionId\":"+number(value.header_constraint?source_constraint_expression(value.header_constraint):0)+
    ",\"captureProof\":\"original-concept-definition\""+position(value.location)+"}");
  unsigned ordinal=0;for(const auto &formal:value.formals)
    source_concept_parameters.push_back(constraint_formal_row("conceptId",captured,ordinal++,formal));
  return captured;
}
std::vector<string> source_expression_temporaries,source_expression_temporary_flags,source_expression_allocations,source_expression_folds,source_expression_field_references;
unsigned source_constraint_expression(unsigned captured) {
  if(!captured)reject("source expression reference is absent");
  if(!written_source_expressions.insert(captured).second)return captured;
  const auto value=dot_object_source::source_constraint_graph().expressions.at(captured-1);
  if(!value.error.empty())reject(value.error.c_str());
  source_expressions.push_back("{\"id\":"+number(captured)+",\"kind\":"+quote(value.kind)+
    ",\"sourceTypeId\":"+number(value.type?source_type(value.type):0)+
    ",\"originalLvalueTypeId\":"+number(value.original_lvalue_type?source_type(value.original_lvalue_type):0)+
    ",\"isParenthesized\":"+(value.parenthesized?"true":"false")+
    ",\"isCompilerGenerated\":"+(value.compiler_generated?"true":"false")+
    ",\"isVolatileFetch\":"+(value.volatile_fetch?"true":"false")+
    ",\"isPackExpansion\":"+(value.pack_expansion?"true":"false")+
    ",\"valueCategory\":"+quote(!value.type?"none":value.lvalue?"lvalue":value.xvalue?"xvalue":"prvalue")+
    ",\"edgeCount\":"+std::to_string(value.edges.size())+",\"parameterCount\":"+std::to_string(value.parameters.size())+
    ",\"argumentCount\":"+std::to_string(value.arguments.size())+",\"substitutionCount\":"+std::to_string(value.substitutions.size())+
    ",\"captureProof\":\"original-source-expression\""+position(value.location)+"}");
  if(value.kind=="field-reference") {
    auto owner=dot_object_source::state().types.at(value.field_owner-1).record_type;
    source_record_metadata(owner);
    if(!source_field_ids.count(value.field))reject("source field reference has no original field row");
    source_expression_field_references.push_back("{\"expressionId\":"+number(captured)+",\"sourceFieldId\":"+number(source_field_ids.at(value.field))+",\"captureProof\":\"original-field-reference\"}");
  }
  if(value.kind=="fold-expression")
    source_expression_folds.push_back("{\"expressionId\":"+number(captured)+",\"operatorToken\":"+std::to_string(value.fold_token)+
      ",\"operatorSpelling\":"+quote(value.fold_operator)+",\"isLeftAssociative\":"+(value.fold_left?"true":"false")+",\"captureProof\":\"original-fold-expression\"}");
  if(value.kind=="allocation-expression") {
    const auto &allocation=value.allocation;
    source_expression_allocations.push_back("{\"expressionId\":"+number(captured)+",\"objectSourceTypeId\":"+number(source_type(allocation.type))+
      ",\"operatorDeclarationId\":"+number(allocation.routine?source_constraint_declaration(allocation.routine):0)+
      ",\"isNew\":"+(allocation.is_new?"true":"false")+",\"isDependentSelection\":"+(allocation.dependent?"true":"false")+
      ",\"isPlacement\":"+(allocation.placement?"true":"false")+",\"isAligned\":"+(allocation.aligned?"true":"false")+
      ",\"isArrayDelete\":"+(allocation.array_delete?"true":"false")+",\"isGlobal\":"+(allocation.global?"true":"false")+
      ",\"hasInitializer\":"+(allocation.has_initializer?"true":"false")+",\"isBracedInitializer\":"+(allocation.braced?"true":"false")+
      ",\"isParenthesizedAggregate\":"+(allocation.paren_aggregate?"true":"false")+",\"hasDeducibleType\":"+(allocation.deducible?"true":"false")+
      ",\"hasParenthesizedTypeId\":"+(allocation.parenthesized?"true":"false")+
      ",\"isLookupDeferred\":"+(allocation.lookup_deferred?"true":"false")+
      ",\"initializationDisposition\":\"absent-in-original\",\"rollbackDisposition\":\"absent-in-original\",\"captureProof\":\"original-new-delete-expression\"}");
  }
  if(value.kind=="temporary-initialization") {
    const auto &temp=value.temporary;
    source_expression_temporaries.push_back("{\"expressionId\":"+number(captured)+",\"initializationKind\":"+std::to_string(temp.initialization_kind)+
      ",\"spelledSourceTypeId\":"+number(temp.spelled_type?source_type(temp.spelled_type):0)+
      ",\"constructorDeclarationId\":"+number(temp.constructor?source_constraint_declaration(temp.constructor):0)+
      ",\"destructorDeclarationId\":"+number(temp.destructor?source_constraint_declaration(temp.destructor):0)+
      ",\"constantId\":"+number(temp.constant?source_semantic_constant(temp.constant):0)+
      ",\"isDependentConstructor\":"+(temp.dependent_constructor?"true":"false")+",\"lifetimeDisposition\":\"no-linked-lifetime\",\"captureProof\":\"original-expression-temporary\"}");
    for(const auto &flag:temp.flags)source_expression_temporary_flags.push_back("{\"expressionId\":"+number(captured)+",\"flag\":"+quote(flag)+"}");
  }
  std::map<string,unsigned> ordinals;
  for(const auto &edge:value.edges)source_expression_edges.push_back("{\"expressionId\":"+number(captured)+
    ",\"role\":"+quote(edge.role)+",\"ordinal\":"+std::to_string(ordinals[edge.role]++)+
    ",\"childExpressionId\":"+number(source_constraint_expression(edge.child))+"}");
  if(value.binder)source_expression_bindings.push_back("{\"expressionId\":"+number(captured)+
    ",\"lexicalBinderId\":"+number(source_parameter_binder(value.binder))+"}");
  if(value.declaration)source_expression_declaration_references.push_back("{\"expressionId\":"+number(captured)+
    ",\"declarationId\":"+number(source_constraint_declaration(value.declaration))+"}");
  if(value.kind=="concept-id") {
    source_concept_uses.push_back("{\"expressionId\":"+number(captured)+",\"conceptId\":"+
      number(source_constraint_concept(value.concept_id))+",\"isTypeConstraint\":"+(value.type_constraint?"true":"false")+"}");
    unsigned ordinal=0;for(const auto &argument:value.arguments) {
      source_constraint_argument_details_row("concept",captured,ordinal,0,argument);
      string kind=argument.kind;
      source_concept_arguments.push_back("{\"expressionId\":"+number(captured)+",\"ordinal\":"+std::to_string(ordinal++)+
        ",\"kind\":"+quote(kind)+",\"sourceTypeId\":"+number(argument.type?source_type(argument.type):0)+
        ",\"constantId\":"+number(argument.constant?source_semantic_constant(argument.constant):0)+
        ",\"templateId\":"+number(argument.templ?source_template_reference(argument.templ):0)+"}");
    }
  }
  if(value.kind=="dependent-function-reference") {
    source_dependent_function_lookups.push_back("{\"expressionId\":"+number(captured)+",\"sourceName\":"+quote(value.lookup_name)+
      ",\"qualifiedName\":"+quote(value.lookup_qualified_name)+",\"ownerSourceTypeId\":"+number(value.lookup_owner?source_type(value.lookup_owner):0)+
      ",\"conversionSourceTypeId\":"+number(value.lookup_conversion_type?source_type(value.lookup_conversion_type):0)+
      ",\"operatorKind\":"+std::to_string(value.lookup_operator)+",\"hasExplicitTemplateArguments\":"+(value.lookup_template?"true":"false")+
      ",\"isQualifiedName\":"+(value.lookup_qualified?"true":"false")+",\"hasAddressOf\":"+(value.lookup_address?"true":"false")+
      ",\"hasLookupSymbol\":"+(value.lookup_symbol?"true":"false")+",\"candidateCount\":"+std::to_string(value.lookup_candidates.size())+
      ",\"memberOfUnknownBase\":"+(value.lookup_unknown_base?"true":"false")+",\"qualifiedUnknownBaseMember\":"+(value.lookup_qualified_unknown_base?"true":"false")+
      ",\"captureProof\":\"original-dependent-function-lookup\"}");
    unsigned ordinal=0;for(auto candidate:value.lookup_candidates)
      source_dependent_function_candidates.push_back("{\"expressionId\":"+number(captured)+",\"ordinal\":"+std::to_string(ordinal++)+
        ",\"declarationId\":"+number(source_constraint_declaration(candidate))+"}");
    ordinal=0;for(const auto &argument:value.arguments) {
      source_constraint_argument_details_row("lookup",captured,ordinal,0,argument);
      source_dependent_function_arguments.push_back("{\"expressionId\":"+number(captured)+",\"ordinal\":"+std::to_string(ordinal++)+
        ",\"kind\":"+quote(argument.kind)+",\"sourceTypeId\":"+number(argument.type?source_type(argument.type):0)+
        ",\"constantId\":"+number(argument.constant?source_semantic_constant(argument.constant):0)+
        ",\"templateId\":"+number(argument.templ?source_template_reference(argument.templ):0)+"}");
    }
  }
  if(value.kind=="requires") {
    unsigned ordinal=0;for(const auto &formal:value.parameters)
      source_expression_parameters.push_back("{\"expressionId\":"+number(captured)+",\"ordinal\":"+std::to_string(ordinal++)+
        ",\"sourceTypeId\":"+number(source_type(formal.type))+
        ",\"declaredSourceTypeId\":"+number(formal.declared_type?source_type(formal.declared_type):0)+
        ",\"sourceName\":"+quote(formal.name)+",\"qualifierBits\":"+std::to_string(formal.qualifiers)+
        ",\"isPack\":"+(formal.pack?"true":"false")+"}");
    unsigned substitution_ordinal=0;for(const auto &substitution:value.substitutions) {
      string owner="\"expressionId\":"+number(captured)+",\"substitutionOrdinal\":"+std::to_string(substitution_ordinal);
      source_expression_substitutions.push_back("{"+owner+
        ",\"formalCount\":"+std::to_string(substitution.formals.size())+",\"argumentCount\":"+std::to_string(substitution.arguments.size())+
        ",\"argumentsKnownDependent\":"+(substitution.known_dependent?"true":"false")+
        ",\"argumentsKnownNondependent\":"+(substitution.known_nondependent?"true":"false")+
        ",\"adjustCoordinates\":"+(substitution.adjust_coordinates?"true":"false")+
        ",\"aliasDeductionGuide\":"+(substitution.alias_deduction_guide?"true":"false")+"}");
      unsigned formal_ordinal=0;for(const auto &formal:substitution.formals) {
        string row=constraint_formal_row("expressionId",captured,formal_ordinal++,formal);
        row.insert(1,"\"substitutionOrdinal\":"+std::to_string(substitution_ordinal)+",");
        source_expression_substitution_formals.push_back(row);
      }
      unsigned argument_ordinal=0;for(const auto &argument:substitution.arguments) {
        source_constraint_argument_details_row("substitution",captured,argument_ordinal,substitution_ordinal,argument);
        string kind=argument.kind;
        source_expression_substitution_arguments.push_back("{"+owner+",\"ordinal\":"+std::to_string(argument_ordinal++)+
          ",\"kind\":"+quote(kind)+",\"sourceTypeId\":"+number(argument.type?source_type(argument.type):0)+
          ",\"constantId\":"+number(argument.constant?source_semantic_constant(argument.constant):0)+
          ",\"templateId\":"+number(argument.templ?source_template_reference(argument.templ):0)+"}");
      }
      ++substitution_ordinal;
    }
  }
  if(value.kind=="requires")source_parameter_binder(dot_object_source::source_constraint_graph().requires_binders.at(captured));
  if(value.kind=="parameter-reference")
    source_expression_parameter_references.push_back("{\"expressionId\":"+number(captured)+
      ",\"binderId\":"+number(source_parameter_binder(value.resolved_binder))+
      ",\"parameterOrdinal\":"+std::to_string(value.parameter_number-1)+
      ",\"originalLevelsUp\":"+std::to_string(value.levels_up)+"}");
  if(value.kind=="operation" || value.kind=="builtin-operation")
    source_expression_operations.push_back("{\"expressionId\":"+number(captured)+",\"opcode\":"+std::to_string(value.opcode)+
      ",\"operationName\":"+quote(value.operation)+",\"originalTypeKind\":"+(value.operation_type_kind>=0?std::to_string(value.operation_type_kind):"null")+"}");
  for(const auto &flag:value.flags)source_expression_flags.push_back("{\"expressionId\":"+number(captured)+",\"flag\":"+quote(flag)+"}");
  if(value.kind=="compound-requirement")source_expression_requirements.push_back("{\"expressionId\":"+number(captured)+
    ",\"requiresNoexcept\":"+(value.noexcept_required?"true":"false")+"}");
  if(value.type_operand)source_expression_type_operands.push_back("{\"expressionId\":"+number(captured)+
    ",\"sourceTypeId\":"+number(source_type(value.type_operand))+"}");
  if(value.constant)source_expression_constants.push_back("{\"expressionId\":"+number(captured)+
    ",\"constantId\":"+number(source_semantic_constant(value.constant))+"}");
  if(value.kind=="sizeof" || value.kind=="alignof" || value.kind=="datasizeof")source_expression_queries.push_back("{\"expressionId\":"+
    number(captured)+",\"isStandardAlignof\":"+(value.standard_alignof?"true":"false")+"}");
  return captured;
}

static const char *source_type_operator_name(int opcode) {
  switch(opcode) {
    case 1:return "decltype";
    case 5:return "__underlying_type";
    case 6:return "typeof-expression";
    case 7:return "typeof-type";
    case 14:return "__add_lvalue_reference";
    case 15:return "__add_pointer";
    case 16:return "__add_rvalue_reference";
    case 17:return "__decay";
    case 18:return "__make_signed";
    case 19:return "__make_unsigned";
    case 20:return "__remove_all_extents";
    case 21:return "__remove_const";
    case 22:return "__remove_cv";
    case 23:return "__remove_cvref";
    case 24:return "__remove_extent";
    case 25:return "__remove_pointer";
    case 26:return "__remove_reference_t";
    case 27:return "__remove_restrict";
    case 28:return "__remove_volatile";
    case 29:return "__remove_reference";
    default:reject("source type operator has no configured semantic name");return "";
  }
}
std::vector<string> source_named_templates,source_alias_instances,source_alias_arguments,source_type_operators;
void source_type_origin(unsigned id,const dot_object_source::Type &type) {
  if(type.source_form=="template-alias") {
    const auto &value=dot_object_source::state().source_aliases.at(type.original_type);
    if(!value.error.empty())reject(value.error.c_str());
    source_alias_instances.push_back("{\"typeId\":"+number(id)+",\"templateId\":"+number(source_template_reference(value.templ))+
      ",\"isPrototype\":"+(value.prototype?"true":"false")+",\"isNonreal\":"+(value.nonreal?"true":"false")+
      ",\"isDependent\":"+(value.dependent?"true":"false")+",\"argumentCount\":"+std::to_string(value.arguments.size())+
      ",\"hasOriginalArguments\":"+(value.original_present?"true":"false")+",\"originalArgumentCount\":"+std::to_string(value.original_arguments.size())+
      ",\"minimumArguments\":"+std::to_string(value.minimum_arguments)+",\"captureProof\":\"original-alias-instance\"}");
    auto arguments=[&](const char *kind,const std::vector<dot_object_source::SourceTemplateArgument> &args) {
      unsigned ordinal=0;for(const auto &arg:args) {
        source_constraint_argument_details_row(kind,id,ordinal,0,arg);
        source_alias_arguments.push_back("{\"typeId\":"+number(id)+",\"listKind\":"+quote(kind)+",\"ordinal\":"+std::to_string(ordinal++)+
          ",\"kind\":"+quote(arg.kind)+",\"sourceTypeId\":"+number(arg.type?source_type(arg.type):0)+
          ",\"constantId\":"+number(arg.constant?source_semantic_constant(arg.constant):0)+",\"templateId\":"+number(arg.templ?source_template_reference(arg.templ):0)+"}");
      }
    };
    arguments("alias-selected",value.arguments);arguments("alias-original",value.original_arguments);
  } else if(type.source_form=="type-operator") {
    source_type_operators.push_back("{\"typeId\":"+number(id)+",\"opcode\":"+std::to_string(type.type_operator)+",\"operatorName\":"+quote(source_type_operator_name(type.type_operator))+
      ",\"expressionId\":"+number(type.operator_expression?source_constraint_expression(type.operator_expression):0)+
      ",\"operandSourceTypeId\":"+number(type.operator_type?source_type(type.operator_type):0)+
      ",\"isDependent\":"+(type.operator_dependent?"true":"false")+",\"isUnparenthesizedDecltype\":"+(type.operator_unparenthesized?"true":"false")+
      ",\"captureProof\":\"original-type-operator\"}");
  }
}
