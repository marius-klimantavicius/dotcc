/* Actual retained spelling form; no lookup or reconstructed qualification. */
static Rid roslyn_qualifier(a_name_qualifier_ptr p) {
 bool fresh;Rid id=DB.claim(p,"name_qualifier",fresh);if(!fresh)return id;
 Rid target=p->is_class?roslyn_type(p->qualifier.class_type):roslyn_namespace(p->qualifier.namespace_ptr);
 DB.put("name_qualifier",{{"id",id},{"previous_qualifier_id",R(roslyn_qualifier(p->previous_qualifier))},
  {"target_id",R(target)},{"name_id",R(DB.atom(p->name))},{"is_class",static_cast<int>(p->is_class)}});return id;
}
static Rid roslyn_name_reference(a_name_reference_ptr p) {
 bool fresh;Rid id=DB.claim(p,"name_reference",fresh);if(!fresh)return id;
 int primary=0;Rid property=0;
#if MICROSOFT_EXTENSIONS_ALLOWED && !DO_IL_LOWERING
 if((p->special_kind==sfk_property_get||p->special_kind==sfk_property_set)&&p->variant.property_or_event_descr&&p->variant.property_or_event_descr->kind==pek_declspec_property)property=roslyn_field(p->variant.property_or_event_descr->variant.field);
#endif
#if GENERATE_SOURCE_SEQUENCE_LISTS
 primary=p->used_in_primary_declarator;
#endif
 DB.put("name_reference",{{"id",id},{"kind_id",roslyn_kind_edg_special_function(p->special_kind)},{"property_id",R(property)},
  {"destructor_type_id",R(p->special_kind==sfk_none?roslyn_type(p->variant.destructor_type):0)},
  {"qualifier_id",R(roslyn_qualifier(p->qualifier))},{"num_template_arguments",Q(p->num_template_arguments)},
  {"is_global_qualified_name",static_cast<int>(p->is_global_qualified_name)},
  {"is_template_id",static_cast<int>(p->is_template_id)},{"is_super_qualified",static_cast<int>(p->is_super_qualified)},
  {"is_decltype_qualified",static_cast<int>(p->is_decltype_qualified)},
  {"from_prototype_instantiation",static_cast<int>(p->from_prototype_instantiation)},
  {"used_in_primary_declarator",primary}});
 int ordinal=0;for(a_template_arg_ptr a=p->orig_template_arg_list;a;a=a->next)
  DB.put("original_template_argument",{{"owner_id",id},{"ordinal",ordinal++},{"argument_id",roslyn_argument(a)}});
 return id;
}
