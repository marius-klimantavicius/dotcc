/* Typed tagged roots and retained declarations. No generic payload storage. */
static Rid roslyn_entity(const a_tagged_pointer &p);
static Rid roslyn_using(a_using_decl_ptr p,a_scope_ptr scope=nullptr) {
 bool fresh;Rid id=DB.claim(p,"using",fresh);if(!fresh)return id;
 DB.put("using_declaration",{{"id",id},{"target_id",R(roslyn_entity(p->entity))},
  {"lexical_scope_id",R(roslyn_scope(scope))},{"source_origin_id",R(roslyn_origin(p->position))},
  {"is_directive",static_cast<int>(p->is_using_directive)},{"is_class_member",static_cast<int>(p->is_class_member)},
  {"is_inheriting_constructor",static_cast<int>(p->is_inheriting_ctor)},{"is_hidden",static_cast<int>(p->hidden)},
  {"compiler_generated",static_cast<int>(p->compiler_generated)},{"is_inline_namespace",static_cast<int>(p->inline_namespace)},
  {"is_strong",static_cast<int>(p->strong)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},
  {"is_representative",static_cast<int>(p->is_representative)},{"is_using_enum",static_cast<int>(p->is_using_enum)}});
 roslyn_attributes(id,p->attributes);return id;
}
static Rid roslyn_static_assertion(a_static_assertion_ptr p) {
 bool fresh;Rid id=DB.claim(p,"static_assertion",fresh);if(!fresh)return id;
 DB.put("static_assertion",{{"id",id},{"condition_id",roslyn_constant(p->condition)},
  {"message_id",R(roslyn_constant(p->string_literal))},{"source_origin_id",R(roslyn_origin(p->position))}});return id;
}
static Rid roslyn_entity(const a_tagged_pointer &p) {
 if(!p.ptr)return 0;
 switch(p.kind){
 case iek_constant:return roslyn_constant(reinterpret_cast<a_constant_ptr>(p.ptr));
 case iek_type:return roslyn_type(reinterpret_cast<a_type_ptr>(p.ptr));
 case iek_variable:return roslyn_variable(reinterpret_cast<a_variable_ptr>(p.ptr));
 case iek_field:return roslyn_field(reinterpret_cast<a_field_ptr>(p.ptr));
 case iek_routine:return roslyn_routine(reinterpret_cast<a_routine_ptr>(p.ptr));
 case iek_label:return roslyn_label(reinterpret_cast<a_label_ptr>(p.ptr));
 case iek_expr_node:return roslyn_expr(reinterpret_cast<an_expr_node_ptr>(p.ptr));
 case iek_statement:return roslyn_statement(reinterpret_cast<a_statement_ptr>(p.ptr));
 case iek_scope:return roslyn_scope(reinterpret_cast<a_scope_ptr>(p.ptr));
 case iek_namespace:return roslyn_namespace(reinterpret_cast<a_namespace_ptr>(p.ptr));
 case iek_template:return roslyn_template(reinterpret_cast<a_template_ptr>(p.ptr));
 case iek_dynamic_init:return roslyn_init(reinterpret_cast<a_dynamic_init_ptr>(p.ptr));
 case iek_using_decl:return roslyn_using(reinterpret_cast<a_using_decl_ptr>(p.ptr));
 case iek_static_assertion:return roslyn_static_assertion(reinterpret_cast<a_static_assertion_ptr>(p.ptr));
 case iek_module_import_decl:return roslyn_module_import(reinterpret_cast<a_module_import_decl_ptr>(p.ptr));
 case iek_asm_entry:return roslyn_asm(reinterpret_cast<an_asm_entry_ptr>(p.ptr));
 case iek_vla_dimension:return roslyn_vla_dimension(reinterpret_cast<a_vla_dimension_ptr>(p.ptr));
 case iek_base_class:return roslyn_base(reinterpret_cast<a_base_class_ptr>(p.ptr));
 default:DB.fail("Tagged semantic entity adapter missing: "+Q(p.kind));return 0;
 }
}
static void roslyn_header_roots() {
 int ordinal=0;for(a_module_import_decl_ptr p=il_header.imported_modules;p;p=p->next)DB.put("translation_unit_module_import",{{"owner_id",roslyn_unit},{"ordinal",ordinal++},{"import_id",roslyn_module_import(p)}});
 ordinal=0;
 for(a_memory_region_number region=1;region<=highest_used_region_number;++region)
  for(a_scope_ptr s=il_header.region_scope_entry[region];s;s=s->next)
   DB.put("translation_unit_root",{{"owner_id",roslyn_unit},{"kind_id",K("root_kind","resident_region")},{"ordinal",ordinal++},{"entity_id",roslyn_scope(s)}});
 ordinal=0;for(a_type_ptr t=il_header.nontag_types_used_in_exception_or_rtti;t;t=t->next)
  DB.put("translation_unit_root",{{"owner_id",roslyn_unit},{"kind_id",K("root_kind","nontag_type")},{"ordinal",ordinal++},{"entity_id",roslyn_type(t)}});
 ordinal=0;for(an_il_entity_list_entry_ptr e=il_header.file_scope_statements;e;e=e->next)
  DB.put("translation_unit_root",{{"owner_id",roslyn_unit},{"kind_id",K("root_kind","file_scope_statement")},{"ordinal",ordinal++},{"entity_id",roslyn_entity(e->entity)}});
#if SCOPE_ORPHANED_LIST_PROCESSING_NEEDED
 ordinal=0;for(a_scope_orphaned_list_header_ptr h=il_header.scope_orphaned_list_headers;h;h=h->next){
  bool fresh;Rid id=DB.claim(h,"orphan_scope",fresh);if(!fresh)continue;
  DB.put("orphan_scope",{{"id",id},{"translation_unit_id",roslyn_unit},{"routine_id",R(roslyn_routine(h->assoc_routine))},{"scope_number",Q(h->scope_number)}});
  DB.put("translation_unit_root",{{"owner_id",roslyn_unit},{"kind_id",K("root_kind","scope_orphan")},{"ordinal",ordinal++},{"entity_id",id}});
  int n=0;for(a_type_ptr t=h->orphaned_types;t;t=t->next)DB.put("orphan_scope_member",{{"owner_id",id},{"kind_id",K("entity","type")},{"ordinal",n++},{"entity_id",roslyn_type(t)}});
  n=0;for(a_variable_ptr v=h->orphaned_variables;v;v=v->next)DB.put("orphan_scope_member",{{"owner_id",id},{"kind_id",K("entity","variable")},{"ordinal",n++},{"entity_id",roslyn_variable(v)}});
  n=0;for(a_namespace_ptr ns=h->orphaned_namespaces;ns;ns=ns->next)DB.put("orphan_scope_member",{{"owner_id",id},{"kind_id",K("entity","namespace")},{"ordinal",n++},{"entity_id",roslyn_namespace(ns)}});
 }
#endif
#if SEPARATE_ROUTINES_FOR_FILE_SCOPE_DYNAMIC_INITS
 ordinal=0;for(a_routine_list_entry_ptr r=il_header.file_scope_dynamic_init_routines;r;r=r->next)
  DB.put("file_scope_dynamic_init_routine",{{"owner_id",roslyn_unit},{"ordinal",ordinal++},{"routine_id",roslyn_routine(r->routine)},{"is_thread_local",0}});
#if !USE_LAZY_INITIALIZATION_FOR_THREAD_LOCAL_VARIABLES
 for(a_routine_list_entry_ptr r=il_header.thread_local_dynamic_init_routines;r;r=r->next)
  DB.put("file_scope_dynamic_init_routine",{{"owner_id",roslyn_unit},{"ordinal",ordinal++},{"routine_id",roslyn_routine(r->routine)},{"is_thread_local",1}});
#endif
#endif
}
