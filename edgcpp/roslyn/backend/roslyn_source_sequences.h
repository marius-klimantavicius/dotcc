/* Retained source occurrence and declaration-order projection.
   Included inside EDG namespace after canonical entity/symbol callbacks.
   Source list owners never substitute for the lexical scope of a declaration.
   Parser symbols, pending if-exists state and pragma descriptors are excluded. */

static Rid roslyn_source_entity(const a_tagged_pointer &p);
static Rid roslyn_pragma(a_pragma_ptr p);
#if GENERATE_SOURCE_SEQUENCE_LISTS
static Rid roslyn_source_entry(a_source_sequence_entry_ptr p);
static Rid roslyn_source_sublist(a_src_seq_sublist_ptr p);
static Rid roslyn_secondary_declaration(a_src_seq_secondary_decl_ptr p);
#endif

#if EXTRA_SOURCE_POSITIONS_IN_IL
static Rid roslyn_declaration_range(const a_source_range &p) {
  Rid start=roslyn_position(p.start),end=roslyn_position(p.end);
  if(!start || !end)return 0;
  auto key=std::make_pair(start,end);auto known=roslyn_ranges.find(key);
  if(known!=roslyn_ranges.end())return known->second;
  Rid id=DB.fresh("source_range");roslyn_ranges[key]=id;
  DB.put("source_range",{{"id",id},{"start_id",start},{"end_id",end},
    {"endpoint_kind_id",K("range_endpoint","inclusive")}});return id;
}
static void roslyn_declaration_positions(Rid owner,a_decl_position_supplement_ptr p,
                                         const char *entity_kind) {
  if(!p)return;
  Rid declarator=0,enum_value=0,namespace_definition=0;
  if(std::strcmp(entity_kind,"enumerator_symbol")==0)
    enum_value=roslyn_declaration_range(p->variant.enum_value_range);
  else if(std::strcmp(entity_kind,"namespace")==0)
    namespace_definition=roslyn_declaration_range(p->variant.namespace_definition_range);
  else declarator=roslyn_declaration_range(p->variant.declarator_range);
  DB.put("declaration_positions",{{"owner_id",owner},
    {"identifier_range_id",R(roslyn_declaration_range(p->identifier_range))},
    {"specifiers_range_id",R(roslyn_declaration_range(p->specifiers_range))},
    {"declarator_range_id",R(declarator)},{"enum_value_range_id",R(enum_value)},
    {"namespace_definition_range_id",R(namespace_definition)}});
  int ordinal=0;for(an_element_position_ptr e=p->extra_positions;e;e=e->next)
    DB.put("declaration_element_position",{{"owner_id",owner},{"ordinal",ordinal++},
      {"kind_id",roslyn_kind_edg_element_position(e->kind)},
      {"position_id",R(roslyn_position(e->position))}});
}
#endif

#if GENERATE_SOURCE_SEQUENCE_LISTS
static const char *roslyn_source_declaration_kind(const a_tagged_pointer &p) {
  switch(p.kind) {
    case iek_constant:return DB.existing(p.ptr,"enumerator_symbol")?"enumerator_symbol":"declaration";
    case iek_namespace:return "namespace";
    default:return "declaration";
  }
}
static Rid roslyn_secondary_declaration(a_src_seq_secondary_decl_ptr p) {
  bool fresh;Rid id=DB.claim(p,"declaration",fresh);if(!fresh)return id;
  Rid target=roslyn_source_entity(p->entity),symbol=roslyn_declared_symbol(p->entity);
  if(!target){DB.fail("A secondary declaration has no retained declared entity");return id;}
  RoslynSqlValue first,partial,generated_forward,nonautonomous,event;
  if(p->entity.kind==iek_routine || p->entity.kind==iek_variable)
    first=static_cast<int>(p->first_declaration);
  if(p->entity.kind==iek_type) {
    auto type=reinterpret_cast<a_type_ptr>(p->entity.ptr);
    if(type && (type->kind==tk_class || type->kind==tk_struct || type->kind==tk_union
        || type->kind==tk_enum || (type->kind==tk_integer && type->variant.integer.enum_type)))
      first=static_cast<int>(p->first_declaration);
  }
#if TEMPLATE_INSTANTIATIONS_IN_SOURCE_SEQUENCE_LISTS
  partial=static_cast<int>(p->is_partial_instantiation);
  generated_forward=static_cast<int>(p->compiler_generated_forward_decl);
  nonautonomous=static_cast<int>(p->originally_nonautonomous_definition);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
  event=static_cast<int>(p->is_event_interface);
#endif
  DB.put("declaration",{{"id",id},{"kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"symbol_id",symbol?symbol:target},{"lexical_scope_id",RoslynSqlValue()},
    {"source_origin_id",R(roslyn_origin(p->decl_position))},
    {"compiler_generated",generated_forward.tag?generated_forward:RoslynSqlValue(0)}});
  DB.put("secondary_declaration",{{"id",id},
    {"entity_tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"declared_type_id",R(roslyn_type(p->declared_type))},
    {"name_reference_id",R(roslyn_name_reference(p->name_reference))},
    {"declared_storage_class_id",roslyn_kind_edg_storage_class(p->declared_storage_class)},
    {"autonomous_tag_declaration",static_cast<int>(p->autonomous_tag_decl)},
    {"embeds_source_entries",static_cast<int>(p->embedded_source_sequence_entries)},
    {"is_friend",static_cast<int>(p->friend_decl)},
    {"declared_in_function_prototype",static_cast<int>(p->declared_in_func_prototype)},
    {"specialized_with_new_syntax",static_cast<int>(p->specialized_with_new_syntax)},
    {"is_first_declaration",first},{"is_partial_instantiation",partial},
    {"is_compiler_generated_forward_declaration",generated_forward},
    {"is_originally_nonautonomous_definition",nonautonomous},
    {"is_gnu_extension",static_cast<int>(p->marked_as_gnu_extension)},
    {"is_after_first_declarator",static_cast<int>(p->is_decl_after_first_in_comma_list)},
    {"has_explicit_storage_class",static_cast<int>(p->explicit_storage_class)},
    {"is_alias",static_cast<int>(p->is_alias)},{"is_event_interface",event}});
  roslyn_attributes(id,p->attributes);
#if EXTRA_SOURCE_POSITIONS_IN_IL
  roslyn_declaration_positions(id,p->decl_pos_info,roslyn_source_declaration_kind(p->entity));
#endif
  return id;
}
static Rid roslyn_source_end(a_src_seq_end_of_construct_ptr p) {
  bool fresh;Rid id=DB.claim(p,"source_end_construct",fresh);if(!fresh)return id;
  DB.put("source_end_construct",{{"id",id},
    {"target_tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"target_id",R(roslyn_source_entity(p->entity))},
    {"position_id",R(roslyn_position(p->position))}});return id;
}
static Rid roslyn_instantiation_directive(an_instantiation_directive_ptr p) {
  bool fresh;Rid id=DB.claim(p,"instantiation_directive",fresh);if(!fresh)return id;
  DB.put("instantiation_directive",{{"id",id},
    {"target_tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"target_id",R(roslyn_source_entity(p->entity))},
    {"declared_type_id",R(roslyn_type(p->declared_type))},
    {"source_origin_id",R(roslyn_origin(p->position))},
    {"do_not_instantiate",static_cast<int>(p->do_not_instantiate)}});
  roslyn_attributes(id,p->attributes);
#if EXTRA_SOURCE_POSITIONS_IN_IL
  roslyn_declaration_positions(id,p->decl_pos_info,"declaration");
#endif
  return id;
}
#if GENERATE_LINKAGE_SPEC_BLOCKS
static Rid roslyn_linkage_specification(a_linkage_spec_block_ptr p) {
  bool fresh;Rid id=DB.claim(p,"linkage_specification",fresh);if(!fresh)return id;
  DB.put("linkage_specification",{{"id",id},{"name_string_id",roslyn_constant(p->name_string)},
    {"name_linkage_id",roslyn_kind_edg_name_linkage(static_cast<a_name_linkage_kind>(p->name_linkage))},
    {"position_id",R(roslyn_position(p->position))},
    {"end_position_id",R(roslyn_position(p->end_position))}});return id;
}
#endif
#if GENERATE_MICROSOFT_IF_EXISTS_ENTRIES
static Rid roslyn_if_exists(an_ms_if_exists_ptr p) {
  bool fresh;Rid id=DB.claim(p,"microsoft_if_exists",fresh);if(!fresh)return id;
  bool begin=p->entity.ptr || p->is_this;
  DB.put("microsoft_if_exists",{{"id",id},
    {"target_tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"target_id",R(roslyn_source_entity(p->entity))},
    {"name_reference_id",R(roslyn_name_reference(p->name_reference))},
    {"position_id",R(roslyn_position(p->position))},
    {"is_if_exists",begin?RoslynSqlValue(static_cast<int>(p->is_if_exists)):RoslynSqlValue()},
    {"is_this",static_cast<int>(p->is_this)}});return id;
}
#endif
static Rid roslyn_source_entry(a_source_sequence_entry_ptr p) {
  bool fresh;Rid id=DB.claim(p,"source_entry",fresh);if(!fresh)return id;
  Rid target=roslyn_source_entity(p->entity),declaration=0;
  if(p->entity.kind==iek_src_seq_secondary_decl)declaration=target;
  else declaration=roslyn_primary_declaration(roslyn_declared_symbol(p->entity));
  DB.put("source_entry",{{"id",id},{"tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"target_id",R(target)},{"declaration_id",R(declaration)}});return id;
}
static Rid roslyn_source_sublist(a_src_seq_sublist_ptr p) {
  bool fresh;Rid id=DB.claim(p,"source_sublist",fresh);if(!fresh)return id;
  DB.put("source_sublist",{{"id",id}});
  int ordinal=0;for(a_source_sequence_entry_ptr e=p->source_sequence_list;e;e=e->next) {
    DB.put("source_sublist_entry",{{"owner_id",id},{"ordinal",ordinal++},
      {"entry_id",roslyn_source_entry(e)}});
    if(e==p->last_source_sequence_entry)break;
  }
  return id;
}
#endif

static Rid roslyn_source_entity(const a_tagged_pointer &p) {
  if(!p.ptr)return 0;
  switch(p.kind) {
    case iek_pragma:return roslyn_pragma(reinterpret_cast<a_pragma_ptr>(p.ptr));
#if RECORD_MACROS_IN_IL
    case iek_macro:return roslyn_macro(reinterpret_cast<a_macro_ptr>(p.ptr));
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
    case iek_ms_attribute:return roslyn_ms_attribute(reinterpret_cast<an_ms_attribute_ptr>(p.ptr));
#endif
#if GENERATE_SOURCE_SEQUENCE_LISTS
    case iek_source_sequence_entry:return roslyn_source_entry(reinterpret_cast<a_source_sequence_entry_ptr>(p.ptr));
    case iek_src_seq_secondary_decl:return roslyn_secondary_declaration(reinterpret_cast<a_src_seq_secondary_decl_ptr>(p.ptr));
    case iek_src_seq_end_of_construct:return roslyn_source_end(reinterpret_cast<a_src_seq_end_of_construct_ptr>(p.ptr));
    case iek_src_seq_sublist:return roslyn_source_sublist(reinterpret_cast<a_src_seq_sublist_ptr>(p.ptr));
    case iek_instantiation_directive:return roslyn_instantiation_directive(reinterpret_cast<an_instantiation_directive_ptr>(p.ptr));
#if GENERATE_LINKAGE_SPEC_BLOCKS
    case iek_linkage_spec_block:return roslyn_linkage_specification(reinterpret_cast<a_linkage_spec_block_ptr>(p.ptr));
#endif
#if GENERATE_MICROSOFT_IF_EXISTS_ENTRIES
    case iek_ms_if_exists:return roslyn_if_exists(reinterpret_cast<an_ms_if_exists_ptr>(p.ptr));
#endif
#endif
    default:return roslyn_entity(p);
  }
}

static Rid roslyn_pragma(a_pragma_ptr p) {
  bool fresh;Rid id=DB.claim(p,"pragma",fresh);if(!fresh)return id;
  Rid entry=0;int ms_operator=0;
#if GENERATE_SOURCE_SEQUENCE_LISTS
  entry=roslyn_source_entry(p->source_sequence_entry);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
  ms_operator=p->is_microsoft_pragma_operator;
#endif
  DB.put("pragma",{{"id",id},{"kind_id",roslyn_kind_edg_pragma(p->kind)},
    {"target_tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},
    {"target_id",R(roslyn_source_entity(p->entity))},
    {"source_origin_id",R(roslyn_origin(p->position))},{"source_entry_id",R(entry)},
    {"text_id",R(DB.atom(p->pragma_text))},
    {"ignore_in_back_end",static_cast<int>(p->ignore_in_back_end)},
    {"is_microsoft_operator",ms_operator}});
  switch(p->kind) {
    case pk_stdc:
      DB.put("pragma_stdc",{{"id",id},{"kind_id",roslyn_kind_edg_stdc_pragma(p->variant.stdc.kind)},
        {"value_id",roslyn_kind_edg_stdc_value(p->variant.stdc.value)}});break;
#if UPC_EXTENSIONS_ALLOWED
    case pk_upc:
      DB.put("pragma_upc",{{"id",id},{"kind_id",roslyn_kind_edg_upc_pragma(p->variant.upc.kind)},
        {"access_method_id",R(p->variant.upc.kind==upc_pk_access?roslyn_kind_edg_upc_access(p->variant.upc.value.access_method):0)},
        {"coherence_operation_id",R(p->variant.upc.kind==upc_pk_coherence?roslyn_kind_edg_upc_coherence(p->variant.upc.value.operation):0)}});break;
#endif
#if IDENT_DIRECTIVE_AND_PRAGMA
    case pk_ident_directive:
      DB.put("pragma_ident",{{"id",id},{"string_constant_id",roslyn_constant(p->variant.ident_string)}});break;
#endif
#if BACK_END_IS_CP_GEN_BE
    case pk_pack:
      DB.fail("CP-generating backend pragma alignment is outside the Roslyn backend profile");break;
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
    case pk_comment:
      DB.put("pragma_comment",{{"id",id},{"kind_id",roslyn_kind_edg_ms_pragma_comment(p->variant.comment.kind)},
        {"string_constant_id",R(roslyn_constant(p->variant.comment.str))}});break;
    case pk_conform:
      DB.put("pragma_conform",{{"id",id},{"kind_id",roslyn_kind_edg_ms_pragma_conform(p->variant.conform.kind)},
        {"is_on",static_cast<int>(p->variant.conform.on)},{"is_off",static_cast<int>(p->variant.conform.off)},
        {"is_show",static_cast<int>(p->variant.conform.show)},{"is_push",static_cast<int>(p->variant.conform.push)},
        {"is_pop",static_cast<int>(p->variant.conform.pop)},
        {"identifier_id",R(DB.atom(p->variant.conform.identifier))}});break;
    case pk_include_alias:
      DB.put("pragma_include_alias",{{"id",id},{"long_file_name_id",R(DB.atom(p->variant.include_alias.long_file_name))},
        {"short_file_name_id",R(DB.atom(p->variant.include_alias.short_file_name))}});break;
#endif
#if GNU_EXTENSIONS_ALLOWED
    case pk_gcc_immediate:case pk_gcc_next_token: {
      Rid visibility=0;
#if GNU_VISIBILITY_ATTRIBUTE_ALLOWED
      if(p->variant.gcc.kind==gcc_pk_visibility_push)
        visibility=roslyn_kind_edg_elf_visibility(p->variant.gcc.variant.visibility);
#endif
      DB.put("pragma_gcc",{{"id",id},{"kind_id",roslyn_kind_edg_gcc_pragma(p->variant.gcc.kind)},
        {"visibility_id",R(visibility)}});break;
    }
#endif
    default:break; // All other retained pragma kinds have only their common fields.
  }
  return id;
}

static void roslyn_source_sequences(a_scope_ptr p,Rid scope) {
  int ordinal=0;for(a_pragma_ptr pragma=p->pragmas;pragma;pragma=pragma->next)
    DB.put("scope_pragma",{{"owner_id",scope},{"ordinal",ordinal++},{"pragma_id",roslyn_pragma(pragma)}});
#if GENERATE_SOURCE_SEQUENCE_LISTS
  ordinal=0;for(a_source_sequence_entry_ptr e=p->source_sequence_list;e;e=e->next)
    DB.put("scope_source_entry",{{"owner_id",scope},{"ordinal",ordinal++},{"entry_id",roslyn_source_entry(e)}});
  ordinal=0;for(a_src_seq_sublist_ptr sublist=p->src_seq_sublist_list;sublist;sublist=sublist->next)
    DB.put("scope_source_sublist",{{"owner_id",scope},{"ordinal",ordinal++},{"sublist_id",roslyn_source_sublist(sublist)}});
#if GENERATE_MICROSOFT_IF_EXISTS_ENTRIES
  ordinal=0;for(an_ms_if_exists_ptr entry=p->ms_if_exists;entry;entry=entry->next)
    DB.put("scope_if_exists",{{"owner_id",scope},{"ordinal",ordinal++},{"if_exists_id",roslyn_if_exists(entry)}});
#endif
#endif
}

static void roslyn_record_declarations(a_type_ptr p,Rid record) {
#if MAINTAIN_CLASS_MEMBER_LIST
  int ordinal=0;for(an_il_entity_list_entry_ptr e=p->variant.class_struct_union.extra_info->member_declarations;
                   e;e=e->next,++ordinal) {
    bool fresh;Rid id=DB.claim(e,"record_declaration",fresh);if(!fresh)continue;
    Rid target=roslyn_source_entity(e->entity),declaration=0;
#if GENERATE_SOURCE_SEQUENCE_LISTS
    if(e->entity.kind==iek_src_seq_secondary_decl)declaration=target;
#endif
    DB.put("record_declaration",{{"id",id},{"record_type_id",record},{"ordinal",ordinal},
      {"tag_kind_id",roslyn_kind_edg_il_entry(e->entity.kind)},
      {"target_id",R(target)},{"declaration_id",R(declaration)}});
  }
#else
  (void)p;(void)record;
#endif
}
