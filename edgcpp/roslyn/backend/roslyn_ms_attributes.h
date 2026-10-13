/* Native Microsoft bracket attributes; custom CLI attributes are outside scope. */
#if MICROSOFT_EXTENSIONS_ALLOWED
static Rid roslyn_ms_attribute_argument(an_ms_attribute_arg_ptr p) {
 bool fresh;Rid id=DB.claim(p,"ms_attribute_argument",fresh);if(!fresh)return id;
 RoslynSqlValue integer,boolean,enumeration;Rid string=0,other=0,uuid=0;
 switch(p->kind){
 case msaak_integer:integer=Q(p->variant.integer_value);break;
 case msaak_boolean:boolean=static_cast<int>(p->variant.bool_value);break;
 case msaak_string:string=roslyn_constant(p->variant.string_constant);break;
 case msaak_uuid:uuid=DB.atom(p->variant.uuid_string);break;
 case msaak_enumeration:enumeration=p->variant.enum_value;break;
 case msaak_other:other=DB.atom(p->variant.other_string);break;
 default:DB.fail("Invalid native Microsoft attribute argument");break;
 }
 DB.put("ms_attribute_argument",{{"id",id},{"kind_id",roslyn_kind_edg_ms_attribute_argument(p->kind)},{"parameter_name_id",R(DB.atom(p->param_name))},{"integer_value",integer},{"boolean_value",boolean},{"enumeration_ordinal",enumeration},{"string_constant_id",R(string)},{"other_text_id",R(other)},{"uuid_text_id",R(uuid)}});return id;
}
static Rid roslyn_ms_attribute(an_ms_attribute_ptr p) {
 bool fresh;Rid id=DB.claim(p,"ms_attribute",fresh);if(!fresh)return id;
 if(p->kind==msak_custom){DB.fail("Custom CLI attribute is outside the supported native profile");return id;}
 Rid entry=0;
#if GENERATE_SOURCE_SEQUENCE_LISTS
 entry=roslyn_source_entry(p->source_sequence_entry);
#endif
 DB.put("ms_attribute",{{"id",id},{"kind_id",roslyn_kind_edg_ms_attribute(p->kind)},{"name_id",R(DB.atom(p->variant.info.name))},{"text_id",R(DB.atom(p->variant.info.string))},{"target_tag_kind_id",roslyn_kind_edg_il_entry(p->entity.kind)},{"target_id",R(roslyn_entity(p->entity))},{"source_position_id",R(roslyn_position(p->position))},{"source_entry_id",R(entry)},{"next_in_block_id",R(roslyn_ms_attribute(p->next_in_block))}});
 static const char *targets[]={"msat_none","msat_assembly","msat_module","msat_class","msat_struct","msat_union","msat_enum","msat_constructor","msat_method","msat_property","msat_field","msat_event","msat_interface","msat_parameter","msat_delegate","msat_returnvalue","msat_genericparameter","msat_typedef","msat_variable","msat_routine","msat_interfaceimpl"};
 unsigned known=0;int ordinal=0;for(unsigned bit=0;bit<21;++bit){unsigned flag=1u<<bit;known|=flag;if(p->target&flag)DB.put("ms_attribute_target",{{"owner_id",id},{"ordinal",ordinal++},{"kind_id",K("edg_ms_attribute_target",targets[bit])}});}if(p->target&~known)DB.fail("Unknown native Microsoft attribute target bit");
 ordinal=0;for(an_ms_attribute_arg_ptr argument=p->variant.info.arg_list;argument;argument=argument->next)DB.put("ms_attribute_argument_link",{{"owner_id",id},{"ordinal",ordinal++},{"argument_id",roslyn_ms_attribute_argument(argument)}});
 return id;
}
static void roslyn_ms_attributes(Rid owner,an_ms_attribute_ptr first) {
 int ordinal=0;for(an_ms_attribute_ptr p=first;p;p=p->next)DB.put("owner_ms_attribute",{{"owner_id",owner},{"ordinal",ordinal++},{"attribute_id",roslyn_ms_attribute(p)}});
}
#endif
