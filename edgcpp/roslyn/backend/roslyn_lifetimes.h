/* Canonical retained wrappers and lifetime bindings, independent of visits. */
static Rid roslyn_block_descriptor(a_block_ptr p) {
 bool fresh;Rid id=DB.claim(p,"block_descriptor",fresh);if(!fresh)return id;
 Rid access=0;
#if UPC_EXTENSIONS_ALLOWED
 access=roslyn_kind_edg_upc_access(p->upc_access_method);
#endif
 DB.put("block_descriptor",{{"id",id},{"scope_id",R(roslyn_scope(p->assoc_scope))},
  {"lifetime_id",R(roslyn_lifetime(p->lifetime))},{"final_position_id",R(roslyn_position(p->final_position))},
  {"end_reachable",static_cast<int>(p->end_of_block_reachable)},
  {"is_statement_expression",static_cast<int>(p->is_statement_expression)},
  {"implicit_scope_not_allowed",static_cast<int>(p->implicit_scope_not_allowed)},
  {"upc_access_method_id",R(access)}});return id;
}
static Rid roslyn_handler(a_handler_ptr p) {
 bool fresh;Rid id=DB.claim(p,"handler",fresh);if(!fresh)return id;
 DB.put("handler",{{"id",id},{"body_id",roslyn_statement(p->statement,7002)},
  {"caught_type_id",R(p->parameter?roslyn_type(p->parameter->type):0)},
  {"parameter_id",R(roslyn_variable(p->parameter))},
  {"parameter_initialization_id",R(roslyn_init(p->dynamic_init,7002))},
  {"typeinfo_id",R(ER(p,typeinfo_var,roslyn_variable))}});return id;
}
static Rid roslyn_try_descriptor(a_try_supplement_ptr p) {
 bool fresh;Rid id=DB.claim(p,"try_descriptor",fresh);if(!fresh)return id;
 DB.put("try_descriptor",{{"id",id},{"body_id",roslyn_statement(p->statement,7002)},
  {"is_function_try",static_cast<int>(p->is_function_try_block)},
  {"lifetime_id",R(roslyn_lifetime(p->lifetime))}});
 int ordinal=0;for(a_handler_ptr h=p->handlers;h;h=h->next)
  DB.put("try_handler",{{"owner_id",id},{"ordinal",ordinal++},{"handler_id",roslyn_handler(h)}});
 return id;
}
static Rid roslyn_local_static_initialization(a_local_static_variable_init_ptr p) {
 bool fresh;Rid id=DB.claim(p,"local_static_initialization",fresh);if(!fresh)return id;
 DB.put("local_static_initialization",{{"id",id},{"variable_id",roslyn_variable(p->variable)},
  {"initialization_kind_id",roslyn_kind_initializer(p->init_kind)},
  {"initialization_id",R(p->init_kind==initk_dynamic?roslyn_init(p->initializer.dynamic,7002):0)},
  {"constant_id",R(p->init_kind==initk_static&&!roslyn_projected_tables.count(p->variable)?roslyn_constant(p->initializer.constant):0)},
  {"lifetime_id",R(roslyn_lifetime(p->lifetime))}});return id;
}
static Rid roslyn_constructor_initializer(a_constructor_init_ptr p) {
 bool fresh;Rid id=DB.claim(p,"constructor_initializer",fresh);if(!fresh)return id;
 Rid field=0,base=0,range=0;
 switch(p->kind){case cik_virtual_base_class:case cik_direct_base_class:base=roslyn_base(p->variant.base_class);break;
 case cik_field:field=roslyn_field(p->variant.field);break;case cik_delegation:break;
 default:DB.fail("Invalid constructor initializer kind");break;}
#if EXTRA_SOURCE_POSITIONS_IN_IL
 range=roslyn_range(p->ctor_init_range);
#endif
 DB.put("constructor_initializer",{{"id",id},{"kind_id",roslyn_kind_constructor_init(p->kind)},
  {"field_id",R(field)},{"base_subobject_id",R(base)},
  {"initialization_id",R(roslyn_init(p->initializer,7002))},
  {"source_expression_id",R(roslyn_expr(p->source_expr,7002))},
  {"compiler_generated",static_cast<int>(p->compiler_generated)},
  {"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},{"is_braced",static_cast<int>(p->is_braced)},
  {"uses_field_initializer",static_cast<int>(p->use_field_initializer)},
  {"original_type_id",R(roslyn_type(p->orig_type))},{"source_range_id",R(range)}});return id;
}
static Rid roslyn_lifetime(an_object_lifetime_ptr p) {
 bool fresh;Rid id=DB.claim(p,"lifetime",fresh);if(!fresh)return id;
 DB.put("lifetime",{{"id",id},{"kind_id",roslyn_kind_lifetime(p->kind)},
  {"parent_id",R(roslyn_lifetime(p->parent_lifetime))},{"bound_entity_id",R(roslyn_entity(p->entity))},
  {"parent_checkpoint_id",R(roslyn_init(p->parent_destruction_sublist,7002))},
  {"has_implicit_child",static_cast<int>(p->has_implicit_child)},
  {"has_block_after_label_child",static_cast<int>(p->has_block_after_label_child_lifetime)}});
 int ordinal=0;for(a_dynamic_init_ptr i=p->destructions;i;i=i->next_in_destruction_list)
  DB.put("lifetime_cleanup",{{"owner_id",id},{"ordinal",ordinal++},{"initialization_id",roslyn_init(i,7002)}});
 ordinal=0;for(an_object_lifetime_ptr l=p->child_lifetime;l;l=l->next)
  DB.put("lifetime_child",{{"owner_id",id},{"ordinal",ordinal++},{"child_id",roslyn_lifetime(l)}});
 return id;
}
