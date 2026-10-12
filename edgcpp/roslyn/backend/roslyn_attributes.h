/* Backend-visible retained attribute syntax. No front-end assoc_info payload. */
static Rid roslyn_attribute_group(an_attribute_group_ptr p) {
 bool fresh;Rid id=DB.claim(p,"attribute_group",fresh);if(!fresh)return id;
#if EXTRA_SOURCE_POSITIONS_IN_IL
 Rid origin=roslyn_origin(p->position,&p->end_position);
#else
 Rid origin=roslyn_origin(p->position);
#endif
 DB.put("attribute_group",{{"id",id},{"source_origin_id",R(origin)}});return id;
}
static void roslyn_attributes(Rid owner,an_attribute_ptr first) {
 int ordinal=0;
 for(an_attribute_ptr p=first;p;p=p->next,++ordinal){
  bool fresh;Rid id=DB.claim(p,"attribute",fresh);if(!fresh)continue;
#if EXTRA_SOURCE_POSITIONS_IN_IL
  Rid origin=roslyn_origin(p->position,&p->end_position);
#else
  Rid origin=roslyn_origin(p->position);
#endif
  int implicit_abi=0;
#if GNU_EXTENSIONS_ALLOWED
  implicit_abi=p->is_implicit_abi_tag_attribute;
#endif
  DB.put("attribute",{{"id",id},{"owner_id",owner},{"ordinal",ordinal},
   {"kind_id",roslyn_kind_edg_attribute(p->kind)},{"family_id",roslyn_kind_edg_attribute_family(p->family)},
   {"location_id",roslyn_kind_edg_attribute_location(p->syntactic_location)},
   {"name_id",R(DB.atom(p->name))},{"namespace_name_id",R(DB.atom(p->namespace_name))},
   {"group_id",R(roslyn_attribute_group(p->group))},{"source_origin_id",R(origin)},
   {"on_primary_declaration",static_cast<int>(p->on_primary_declaration)},
   {"transforms_type_specifier",static_cast<int>(p->transforms_type_specifier)},
   {"is_pack_expansion",static_cast<int>(p->is_pack_expansion)},
   {"is_standard_gcc",static_cast<int>(p->is_std_gcc_attribute)},
   {"is_implicit_abi_tag",implicit_abi},{"namespace_from_using",static_cast<int>(p->namespace_from_using)}});
  int argument_ordinal=0;
  for(an_attribute_arg_ptr a=p->arguments;a;a=a->next,++argument_ordinal){
   Rid constant=0,type=0,expression=0,text=0;int token=0;
   switch(a->kind){
   case aak_empty:break;
   case aak_token:case aak_raw_token:text=DB.atom(a->variant.token);token=roslyn_kind_token(a->token_kind);break;
   case aak_constant:constant=roslyn_constant(a->variant.constant);break;
   case aak_type:type=roslyn_type(a->variant.type);break;
   case aak_expression:expression=roslyn_expr(expr_node_from_attribute_arg(a),7002);break;
   default:DB.fail("Attribute argument payload adapter missing");break;
   }
#if EXTRA_SOURCE_POSITIONS_IN_IL
   Rid argument_origin=roslyn_origin(a->position,&a->end_position);
#else
   Rid argument_origin=roslyn_origin(a->position);
#endif
   DB.put("attribute_argument",{{"owner_id",id},{"ordinal",argument_ordinal},
    {"kind_id",roslyn_kind_edg_attribute_argument(a->kind)},{"constant_id",R(constant)},
    {"type_id",R(type)},{"expression_id",R(expression)},{"token_text_id",R(text)},
    {"token_kind_id",R(token)},{"is_pack_expansion",static_cast<int>(a->is_pack_expansion)},
    {"source_origin_id",R(argument_origin)}});
  }
 }
}
