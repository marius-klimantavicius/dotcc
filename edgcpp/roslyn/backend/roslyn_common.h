/* Canonical retained entity correspondence and common scope/symbol adapters. */
static a_scope_ptr roslyn_source_scope(a_source_correspondence &c) {if(c.parent_scope)return c.parent_scope;auto found=roslyn_scope_parents.find(&c);return found==roslyn_scope_parents.end()?nullptr:found->second;}
static Rid roslyn_source_correspondence(a_source_correspondence &c) {
 bool fresh;Rid id=DB.claim(&c,"source_correspondence",fresh);if(!fresh)return id;
 std::vector<std::pair<const char*,RoslynSqlValue>> fields={{"id",id},{"current_name_id",R(DB.atom(c.name))},
  {"parent_scope_id",R(roslyn_scope(roslyn_source_scope(c)))},{"enclosing_routine_id",R(roslyn_routine(c.enclosing_routine))},
  {"is_imported_from_module",static_cast<int>(c.module_entity!=nullptr)},
  {"source_origin_id",R(roslyn_origin(c.decl_position))},
  {"access_id",roslyn_kind_edg_access(static_cast<an_access_specifier>(c.access))},
  {"linkage_id",roslyn_kind_edg_name_linkage(static_cast<a_name_linkage_kind>(c.name_linkage))}};
#define CORRESPONDENCE_FLAG(field) fields.push_back({#field,static_cast<int>(c.field)})
 CORRESPONDENCE_FLAG(referenced);CORRESPONDENCE_FLAG(is_local_to_function);CORRESPONDENCE_FLAG(is_class_member);
 CORRESPONDENCE_FLAG(copied_from_secondary_trans_unit);CORRESPONDENCE_FLAG(same_name_as_external_entity_in_secondary_trans_unit);
 CORRESPONDENCE_FLAG(member_of_unknown_base);CORRESPONDENCE_FLAG(qualified_unknown_base_member);
 CORRESPONDENCE_FLAG(is_deprecated_or_unavailable);CORRESPONDENCE_FLAG(externalized);CORRESPONDENCE_FLAG(maybe_unused);
#if MAINTAIN_NEEDED_FLAGS
 CORRESPONDENCE_FLAG(needed);
#endif
#if NEED_NAME_MANGLING
 fields.push_back({"unmangled_name_or_mangled_encoding_id",R(DB.atom(c.unmangled_name_or_mangled_encoding))});
 CORRESPONDENCE_FLAG(name_has_been_mangled);CORRESPONDENCE_FLAG(unnamed_entity_given_fabricated_name);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 CORRESPONDENCE_FLAG(member_of_unknown_super);CORRESPONDENCE_FLAG(microsoft_identifier_used);
#endif
#if GENERATE_SOURCE_SEQUENCE_LISTS
 CORRESPONDENCE_FLAG(is_decl_after_first_in_comma_list);CORRESPONDENCE_FLAG(marked_as_gnu_extension);
 fields.push_back({"source_sequence_entry_id",R(roslyn_source_entry(c.source_sequence_entry))});
#endif
#undef CORRESPONDENCE_FLAG
 DB.put_fields("source_correspondence",fields);roslyn_attributes(id,c.attributes);return id;
}
static void roslyn_attach_correspondence(Rid owner,a_source_correspondence &c) {
 if(!owner)return;auto inserted=roslyn_correspondence_owners.emplace(owner,&c);
 if(!inserted.second){if(inserted.first->second!=&c)DB.fail("One retained entity has two source correspondence identities");return;}
 Rid descriptor=roslyn_source_correspondence(c);
 DB.put("entity_source_correspondence",{{"owner_id",owner},{"source_correspondence_id",descriptor}});
}
static Rid roslyn_prepare_declaration(Rid symbol,bool declare=true) {
 if(!declare)return 0;auto known=roslyn_primary_declarations.find(symbol);if(known!=roslyn_primary_declarations.end())return known->second;
 Rid declaration=DB.fresh("declaration");roslyn_primary_declarations[symbol]=declaration;return declaration;
}
static void roslyn_register_parameter_placeholder(const void *descriptor,Rid parameter) {
 if(!descriptor)return;
 auto &bindings=roslyn_parameter_placeholders[descriptor];bindings.insert(parameter);
 Rid symbol=bindings.size()==1?*bindings.begin():0;
 Rid type=DB.existing(descriptor,"type");if(type)DB.set_type_symbol(type,symbol);
 Rid templ=DB.existing(descriptor,"template");if(templ)DB.set_template_symbol(templ,symbol);
}
static void roslyn_symbol(Rid id,const char *kind,a_source_correspondence &c,Rid type,int generated=0,int storage=0,bool attrs=true,bool declare=true) {
 Rid declaration=roslyn_prepare_declaration(id,declare);
 DB.put("symbol",{{"id",id},{"kind_id",K("entity",kind)},{"type_id",R(type)},{"compiler_generated",generated},{"storage_id",R(storage)}});
 // Attributes belong to the actual correspondence, shared by all views.
 (void)attrs;roslyn_attach_correspondence(id,c);
 roslyn_fact(id,"source",roslyn_has_position(c.decl_position)?"available":"absent",roslyn_has_position(c.decl_position)?nullptr:"The retained declaration has no source sequence position.");
 if(!declare)return;
 roslyn_attach_correspondence(declaration,c);
 DB.put("declaration",{{"id",declaration},{"kind_id",K("entity",kind)},{"symbol_id",id},{"compiler_generated",generated}});
#if EXTRA_SOURCE_POSITIONS_IN_IL
 roslyn_declaration_positions(declaration,c.decl_pos_info,kind);
#endif
}
static Rid roslyn_using(a_using_decl_ptr p,a_scope_ptr scope=nullptr) {
 bool fresh;Rid id=DB.claim(p,"using",fresh);if(!p)return 0;
 auto &owners=roslyn_using_scopes[p];if(scope)owners.insert(scope);
 Rid lexical_scope=owners.size()==1?roslyn_scope(*owners.begin()):0;
 if(!fresh){DB.set_using_scope(id,lexical_scope);return id;}
 std::vector<std::pair<const char*,RoslynSqlValue>> fields={{"id",id},{"target_id",R(roslyn_entity(p->entity))},
  {"lexical_scope_id",R(lexical_scope)},{"source_origin_id",R(roslyn_origin(p->position))},
  {"is_directive",static_cast<int>(p->is_using_directive)},{"is_class_member",static_cast<int>(p->is_class_member)},
  {"is_inheriting_constructor",static_cast<int>(p->is_inheriting_ctor)},{"is_hidden",static_cast<int>(p->hidden)},
  {"compiler_generated",static_cast<int>(p->compiler_generated)},{"is_inline_namespace",static_cast<int>(p->inline_namespace)},
  {"is_strong",static_cast<int>(p->strong)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},
  {"is_representative",static_cast<int>(p->is_representative)},{"is_using_enum",static_cast<int>(p->is_using_enum)},
  {"is_enumerator",static_cast<int>(p->is_enumerator)}};
 if(!p->is_using_directive){
  if(p->is_class_member)fields.push_back({"adjusted_access_id",roslyn_kind_edg_access(p->access)});
  if(p->is_class_member||p->is_enumerator)fields.push_back({"qualifier_type_id",R(roslyn_type(p->qualifier.class_type))});
  else {fields.push_back({"qualifier_namespace_id",R(roslyn_namespace(p->qualifier.namespace_ptr))});fields.push_back({"is_global_qualified",static_cast<int>(p->qualifier.namespace_ptr==nullptr)});}
 }
#if GENERATE_SOURCE_SEQUENCE_LISTS
 fields.push_back({"next_in_set_id",R(roslyn_using(p->next_in_set))});
#endif
 DB.put_fields("using_declaration",fields);roslyn_attributes(id,p->attributes);return id;
}
static Rid roslyn_scope(a_scope_ptr p) {
 roslyn_collect_local_initializers(p);
 bool fresh;Rid id=DB.claim(p,"scope",fresh);if(!fresh)return id;
 int saved_role=roslyn_default_role;a_routine_ptr saved_routine=roslyn_current_routine;
 a_routine_ptr owning_routine=roslyn_scope_routine(p);if(owning_routine){roslyn_current_routine=owning_routine;roslyn_default_role=roslyn_routine_role(owning_routine);}
 Rid assoc=0;if(p->kind==sck_function)assoc=roslyn_routine(p->variant.routine.ptr);else if(p->kind==sck_namespace)assoc=roslyn_namespace(p->variant.assoc_namespace);else if(p->kind==sck_class_struct_union||p->kind==sck_class_reactivation||p->kind==sck_func_prototype||p->kind==sck_enum)assoc=roslyn_type(p->variant.assoc_type);else if(p->kind==sck_condition)assoc=roslyn_statement(p->variant.assoc_statement);else if(p->kind==sck_block)assoc=roslyn_handler(p->variant.assoc_handler);
 DB.put("scope",{{"id",id},{"kind_id",roslyn_kind_edg_scope(p->kind)},{"parent_id",R(roslyn_scope(roslyn_scope_parent(p)))},{"associated_entity_id",R(assoc)},{"lifetime_id",R(roslyn_lifetime(p->lifetime))},{"function_static_lifetime_id",R(p->kind==sck_function?roslyn_lifetime(p->variant.routine.lifetime_of_local_static_vars):0)},
  {"is_constexpr_routine",static_cast<int>(p->is_constexpr_routine)},{"is_stmt_expr_block",static_cast<int>(p->is_stmt_expr_block)},{"is_placeholder_scope",static_cast<int>(p->is_placeholder_scope)},
  {"this_parameter_id",R(p->kind==sck_function?roslyn_variable(p->variant.routine.this_param_variable):0)},{"nrvo_variable_id",R(p->kind==sck_function?roslyn_variable(p->variant.routine.return_value_variable):0)}});
 int n=0;
#define MEMBER(target) DB.put("scope_member",{{"owner_id",id},{"ordinal",n++},{"member_id",target}})
 for(a_type_ptr t=p->types;t;t=t->next){MEMBER(roslyn_type(t));}
 for(a_variable_ptr v=p->variables;v;v=v->next){MEMBER(roslyn_variable(v));}
 for(a_variable_ptr v=p->nonstatic_variables;v;v=v->next){MEMBER(roslyn_variable(v));}
 for(a_routine_ptr r=p->routines;r;r=r->next){MEMBER(roslyn_routine(r));}
 for(a_constant_ptr c=p->constants;c;c=c->next){MEMBER(roslyn_constant(c));}
 for(a_namespace_ptr ns=p->namespaces;ns;ns=ns->next){MEMBER(roslyn_namespace(ns));}
 for(a_label_ptr l=p->labels;l;l=l->next){MEMBER(roslyn_label(l));}
 for(a_template_ptr t=p->templates;t;t=t->next){MEMBER(roslyn_template(t));}
 for(a_using_decl_ptr u=p->using_declarations;u;u=u->next){MEMBER(roslyn_using(u,p));}
 for(a_using_decl_ptr u=p->using_directives;u;u=u->next){MEMBER(roslyn_using(u,p));}
 for(an_asm_entry_ptr a=p->asm_entries;a;a=a->next){MEMBER(roslyn_asm(a));}
 for(a_vla_dimension_ptr d=p->vla_dimensions;d;d=d->next){MEMBER(roslyn_vla_dimension(d));}
#undef MEMBER
 int child_ordinal=0;for(a_scope_ptr s=p->scopes;s;s=s->next)DB.put("scope_child",{{"owner_id",id},{"ordinal",child_ordinal++},{"child_id",roslyn_scope(s)}});
 int local_static_ordinal=0;for(a_local_static_variable_init_ptr i=p->local_static_variable_inits;i;i=i->next)
  DB.put("scope_local_static_initialization",{{"owner_id",id},{"ordinal",local_static_ordinal++},{"initialization_id",roslyn_local_static_initialization(i)}});
 int constructor_ordinal=0;for(a_constructor_init_ptr i=p->kind==sck_function?p->variant.routine.constructor_inits:nullptr;i;i=i->next){
  DB.put("scope_constructor_initializer",{{"owner_id",id},{"ordinal",constructor_ordinal},{"initializer_id",roslyn_constructor_initializer(i)}});
#if !DO_IL_LOWERING
  if(p->kind==sck_function&&p->assoc_block){Rid body=roslyn_statement(p->assoc_block,roslyn_default_role);roslyn_child(body,"initialization",constructor_ordinal,roslyn_init(i->initializer,roslyn_default_role));roslyn_child(body,"operand",constructor_ordinal,roslyn_expr(i->source_expr,roslyn_default_role));}
#endif
  ++constructor_ordinal;
 }

 int init_ordinal=0;for(a_dynamic_init_ptr i=p->dynamic_inits;i;i=i->next)DB.put("scope_initialization",{{"owner_id",id},{"ordinal",init_ordinal++},{"initialization_id",roslyn_init(i)}});
 if(p->assoc_block)roslyn_statement(p->assoc_block);
 roslyn_scope_expression_references(p,id);
 roslyn_source_sequences(p,id);
#if MICROSOFT_EXTENSIONS_ALLOWED
 roslyn_ms_attributes(id,p->ms_attributes);
#endif
 roslyn_default_role=saved_role;roslyn_current_routine=saved_routine;return id;
}
static Rid roslyn_namespace(a_namespace_ptr p) {
 bool fresh;Rid id=DB.claim(p,"namespace",fresh);if(!fresh)return id;roslyn_symbol(id,"namespace",p->source_corresp,0);
 std::vector<std::pair<const char*,RoslynSqlValue>> fields={{"id",id},{"scope_id",R(p->is_namespace_alias?0:roslyn_scope(p->variant.assoc_scope))},{"alias_target_id",R(p->is_namespace_alias?roslyn_namespace(p->variant.assoc_namespace):0)},{"is_inline",static_cast<int>(p->is_inline)},{"is_anonymous",static_cast<int>(unmangled_name_of(&p->source_corresp)==nullptr)},
  {"has_internal_linkage",static_cast<int>(p->has_internal_linkage)},{"named_in_strong_using",static_cast<int>(p->named_in_strong_using)},{"is_std",static_cast<int>(p->is_std)},{"is_namespace_alias",static_cast<int>(p->is_namespace_alias)}};
#if MICROSOFT_EXTENSIONS_ALLOWED
 fields.push_back({"proxy_class_id",R(roslyn_type(p->proxy_class))});
#endif
#if GNU_EXTENSIONS_ALLOWED
 fields.push_back({"has_gnu_abi_tag_attribute",static_cast<int>(p->has_gnu_abi_tag_attribute)});
#endif
 DB.put_fields("namespace_symbol",fields);return id;
}
static Rid roslyn_label(a_label_ptr p) {
 bool fresh;Rid id=DB.claim(p,"label",fresh);if(!fresh)return id;roslyn_symbol(id,"label",p->source_corresp,0,unmangled_name_of(&p->source_corresp)==nullptr);
 std::vector<std::pair<const char*,RoslynSqlValue>> fields={{"id",id},{"statement_id",R(roslyn_statement(p->exec_stmt))},{"lifetime_id",R(p->exec_stmt&&p->exec_stmt->kind==stmk_label?roslyn_lifetime(p->exec_stmt->variant.label.lifetime):0)},{"is_break",static_cast<int>(p->break_label)},{"is_continue",static_cast<int>(p->continue_label)},
  {"reachable_by_fall_through",static_cast<int>(p->reachable_by_fall_through)},{"is_switch_break",static_cast<int>(p->switch_break_label)},{"is_likely",static_cast<int>(p->is_likely)},{"is_unlikely",static_cast<int>(p->is_unlikely)}};
#if GNU_EXTENSIONS_ALLOWED
 fields.push_back({"address_taken",static_cast<int>(p->address_taken)});fields.push_back({"locally_declared",static_cast<int>(p->locally_declared)});
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 fields.push_back({"is_leave",static_cast<int>(p->leave_label)});fields.push_back({"microsoft_try_depth",Q(p->num_microsoft_trys_inside_of)});
#endif
 DB.put_fields("label_symbol",fields);return id;
}
static void roslyn_reconcile_using_scopes() {
 // Finalize only already exported rows from actual list memberships.  A
 // recursive target traversal may have discovered a second owner after the
 // original using row was claimed; its outer insert must not choose one.
 for(const auto &entry:roslyn_using_scopes){Rid id=DB.existing(entry.first,"using");if(!id)continue;
  Rid scope=entry.second.size()==1?DB.existing(*entry.second.begin(),"scope"):0;
  DB.set_using_scope(id,scope);
 }
}
