/* Values and occurrences are separate. Only exact typed value rows are interned. */
static Rid roslyn_subobject_path(a_subobject_path_ptr p) {
 bool fresh;Rid id=DB.claim(p,"subobject_path",fresh);if(!fresh)return id;
 DB.put("subobject_path",{{"id",id},{"kind_id",K("role",p->is_offset?"element":p->is_base_class?"base":"member")},
  {"field_id",R(!p->is_offset&&!p->is_base_class?roslyn_field(p->variant.field):0)},
  {"base_subobject_id",R(p->is_base_class?roslyn_base(p->variant.base_class):0)},
  {"element_offset",p->is_offset?RoslynSqlValue(Q(p->variant.ptr_offset)):RoslynSqlValue()},
  {"next_id",R(roslyn_subobject_path(p->next))},{"is_converted",static_cast<int>(p->is_converted)}});return id;
}
static Rid roslyn_constant(a_constant_ptr p) {
 bool fresh;Rid id=DB.claim(p,"constant",fresh);if(!fresh)return id;
 RoslynContent content(p->kind);
 switch(p->kind){
 case ck_void:break;
 case ck_integer:content.add("integer_value",{{"value_id",DB.atom(decimal_str_for_integer_constant(p).as_temp_characters())}});break;
 case ck_float:
#if C99_IL_EXTENSIONS_SUPPORTED
 case ck_imaginary:
#endif
 roslyn_float_content(content,skip_typerefs(p->type)->variant.float_kind,p->variant.float_value);break;
#if C99_IL_EXTENSIONS_SUPPORTED
 case ck_complex:{auto c=p->variant.complex_value;auto k=skip_typerefs(p->type)->variant.float_kind;content.add("complex_value",{{"real_value_id",roslyn_float_value(k,c->real)},{"imaginary_value_id",roslyn_float_value(k,c->imag)}});break;}
#endif
#if FIXED_POINT_ALLOWED
 case ck_fixed_point:{auto d=fxp_descr_for_constant(p);std::string scaled=str_for_integer_value(&p->variant.fixed_point_value,!d->is_unsigned,FALSE,p->type->size).as_temp_characters();content.add("fixed_point_value",{{"scaled_integer_id",DB.atom(scaled)},{"scale",static_cast<int>(targ_fractional_bits_for_fixed_point[d->is_unsigned][d->precision][d->is_fract_type])}});break;}
#endif
 case ck_string:{const auto &s=p->variant.string;content.add("string_value",{{"element_bits",8},{"unit_kind_id",K("string_unit_kind","target_bytes")},{"length",Q(s.length)},{"encoding_id",K("string_encoding","target_code_units")}});for(a_targ_size_t i=0;i<s.length;++i)content.add("string_unit",{{"ordinal",static_cast<sqlite3_int64>(i)},{"unit",static_cast<int>(static_cast<unsigned char>(s.value[i]))}},"owner_id");break;}
 case ck_address:{auto &a=p->variant.address;Rid base=0;switch(a.kind){
 case abk_routine:base=roslyn_routine(a.variant.routine);break;
 case abk_variable:base=roslyn_variable(a.variant.variable);break;
 case abk_constant:case abk_temporary:base=roslyn_constant(a.variant.constant);break;
 case abk_typeid:case abk_uuidof:case abk_cli_typeid:base=roslyn_type(a.variant.type);break;
 case abk_label:base=roslyn_label(a.variant.label);break;
 case abk_cli_array:case abk_param_ref:break;
 default:DB.fail("Address base payload adapter missing");break;
 }
 content.add("address_value",{{"base_kind_id",roslyn_kind_address_base(a.kind)},{"base_entity_id",R(base)},
  {"offset",Q(a.offset)},{"one_past_end",static_cast<int>(a.one_past_the_end)},
  {"subobject_path_id",R(roslyn_subobject_path(a.subobject_path))},{"is_object_reflection",static_cast<int>(a.is_object_reflection)},
  {"parameter_number",a.kind==abk_param_ref?RoslynSqlValue(static_cast<sqlite3_int64>(a.variant.param_ref.param_num)):RoslynSqlValue()}});break;}
#if DO_IL_LOWERING && GENERATE_EH_TABLES && !DO_FULL_PORTABLE_EH_LOWERING
 case ck_stack_offset:content.add("stack_offset_value",{{"variable_id",roslyn_variable(p->variant.stack_offset.variable)},{"offset",Q(p->variant.stack_offset.offset)}});break;
#endif
 case ck_ptr_to_member:{auto &m=p->variant.ptr_to_member;Rid member=m.is_function_ptr?roslyn_routine(m.variant.routine):roslyn_field(m.variant.field);auto type=skip_typerefs(p->type);content.add("member_pointer_value",{{"member_id",R(member)},{"containing_type_id",roslyn_type(type->variant.ptr_to_member.class_of_which_a_member)},{"subobject_id",R(roslyn_base(m.casting_base_class))},{"is_function_ptr",static_cast<int>(m.is_function_ptr)},{"cast_to_base",static_cast<int>(m.cast_to_base)},{"is_null",static_cast<int>(member==0)}});break;}
#if GNU_EXTENSIONS_ALLOWED
 case ck_label_difference:{auto &l=p->variant.label_difference;roslyn_constant_component(id,"from",0,l.from_address);roslyn_constant_component(id,"to",0,l.to_address);content.add("label_difference_value",{{"from_value_id",roslyn_value_of(l.from_address)},{"from_type_id",R(roslyn_type(l.from_address->type))},{"to_value_id",roslyn_value_of(l.to_address)},{"to_type_id",R(roslyn_type(l.to_address->type))},{"left_label_id",R(l.from_address->kind==ck_address&&l.from_address->variant.address.kind==abk_label?roslyn_label(l.from_address->variant.address.variant.label):0)},{"right_label_id",R(l.to_address->kind==ck_address&&l.to_address->variant.address.kind==abk_label?roslyn_label(l.to_address->variant.address.variant.label):0)}});break;}
#endif
 case ck_aggregate:{content.add("aggregate_value",{{"type_id",roslyn_type(p->type)}});int n=0;for(a_constant_ptr c=p->variant.aggregate.first_constant;c;c=c->next){roslyn_constant_component(id,"element",n,c);content.add("aggregate_element",{{"ordinal",n++},{"value_id",roslyn_value_of(c)},{"type_id",R(roslyn_type(c->type))}},"owner_id");}break;}
 case ck_init_repeat:{auto &r=p->variant.init_repeat;roslyn_constant_component(id,"repeated",0,r.constant);content.add("repeat_value",{{"value_id",roslyn_value_of(r.constant)},{"type_id",R(roslyn_type(r.constant->type))},{"count",Q(r.count)},{"multidimensional_tail_not_repeated",static_cast<int>(r.multidimensional_aggr_tail_not_repeated)}});break;}
 case ck_dynamic_init:content.add("dynamic_initialization_value",{{"initialization_id",roslyn_init(p->variant.dynamic_init.ptr,7002)}});break;
 case ck_template_param:{auto &t=p->variant.template_param;Rid expression=0,type=0,child_value=0,child_type=0;RoslynSqlValue number,depth;int opname=0,special=0;bool standard_alignof=false,unqualified=false;
 switch(t.kind){
 case tpck_param:number=static_cast<sqlite3_int64>(t.variant.coordinates.position);depth=static_cast<sqlite3_int64>(t.variant.coordinates.depth);break;
 case tpck_expression:expression=roslyn_expr(expr_node_from_tpck_expression(p),7002);break;
 case tpck_member:break;
 case tpck_unknown_function:type=roslyn_type(t.variant.unknown_function.conversion_type);opname=roslyn_kind_edg_opname(t.variant.unknown_function.opname_kind);
#if MICROSOFT_EXTENSIONS_ALLOWED
 special=roslyn_kind_edg_special_function(t.variant.unknown_function.special_kind);
#endif
 break;
 case tpck_address:case tpck_dependent_constant:{auto c=t.variant.constant;roslyn_constant_component(id,"dependent",0,c);child_value=roslyn_value_of(c);child_type=roslyn_type(c->type);break;}
 case tpck_concat_string_literals:{int n=0;for(a_constant_ptr c=t.variant.string_literal_list;c;c=c->next){roslyn_constant_component(id,"dependent",n,c);content.add("dependent_constant_element",{{"ordinal",n++},{"value_id",roslyn_value_of(c)},{"type_id",R(roslyn_type(c->type))}},"owner_id");}break;}
 case tpck_sizeof:case tpck_datasizeof:case tpck_alignof:case tpck_uuidof:case tpck_typeid:case tpck_noexcept:type=roslyn_type(t.variant.templ_sizeof.type);expression=roslyn_expr(generic_sizeof_arg_expr(p),7002);standard_alignof=t.variant.templ_sizeof.is_std_alignof;break;
 case tpck_template_ref:{auto c=t.variant.template_ref.con;roslyn_constant_component(id,"dependent",0,c);child_value=roslyn_value_of(c);child_type=roslyn_type(c->type);int n=0;for(a_template_arg_ptr a=t.variant.template_ref.arg_list;a;a=a->next)content.add("dependent_template_argument",{{"ordinal",n++},{"argument_id",roslyn_argument(a)}},"owner_id");break;}
 case tpck_destructor:type=roslyn_type(t.variant.destructor.type);unqualified=t.variant.destructor.unqualified;break;
 case tpck_integer_pack:{auto c=t.variant.bound;roslyn_constant_component(id,"dependent",0,c);child_value=roslyn_value_of(c);child_type=roslyn_type(c->type);break;}
 default:DB.fail("Dependent constant payload adapter missing");break;
 }
 content.add("dependent_value",{{"kind_id",roslyn_kind_template_constant(t.kind)},{"parameter_number",number},{"parameter_depth",depth},{"expression_id",R(expression)},{"type_id",R(type)},{"constant_value_id",R(child_value)},{"constant_type_id",R(child_type)},{"is_qualified_name",static_cast<int>(t.is_qualified_name)},{"has_address_of",static_cast<int>(t.has_address_of)},{"is_pack",static_cast<int>(t.is_pack)},{"has_generic_cast",static_cast<int>(t.has_generic_cast_for_nontype_template_param)},{"conversion_type_id",R(t.kind==tpck_unknown_function?type:0)},{"opname_kind_id",R(opname)},{"special_kind_id",R(special)},{"is_standard_alignof",static_cast<int>(standard_alignof)},{"destructor_unqualified",static_cast<int>(unqualified)}});break;}
 case ck_designator:{auto &d=p->variant.designator;Rid field=0,name=0;RoslynSqlValue index;if(d.is_field_designator){if(d.is_generic)name=DB.atom(d.variant.field_name);else field=roslyn_field(d.variant.field);}else if(d.is_generic){int n=0;for(a_constant_ptr c=d.variant.subscript;c;c=c->next){roslyn_constant_component(id,"designator",n,c);content.add("designator_subscript",{{"ordinal",n++},{"value_id",roslyn_value_of(c)},{"type_id",R(roslyn_type(c->type))}},"owner_id");}}else index=Q(d.variant.array_element);
 content.add("designator_value",{{"kind_id",K("role",d.is_field_designator?"member":"element")},{"field_id",R(field)},{"field_name_id",R(name)},{"is_generic",static_cast<int>(d.is_generic)},{"uses_direct_init_syntax",static_cast<int>(d.uses_direct_init_syntax)},{"index_value",index}});break;}
 case ck_reflection:{auto &r=p->variant.reflection;content.add("reflection_value",{{"kind_id",roslyn_kind_edg_il_entry(r.entity.kind)},{"entity_id",R(roslyn_entity(r.entity))},{"local_scope_number",Q(r.local_scope_number)}});break;}
 default:DB.fail("Constant payload adapter missing: "+Q(p->kind));break;
 }
 Rid value=content.intern();roslyn_constant_values[p]=value;
 DB.put("constant",{{"id",id},{"type_id",R(roslyn_type(p->type))},{"orig_type_id",R(roslyn_type(p->orig_type))},{"value_id",value},
  {"name_id",R(p->source_corresp.name?roslyn_name(p->source_corresp):0)},{"containing_entity_id",R(roslyn_owner(p->source_corresp))},{"source_origin_id",R(roslyn_origin(p->source_corresp.decl_position,&p->end_position))},
  {"backing_expression_id",R(roslyn_expr(p->local_expr_ref?expr_node_from_constant(p):p->expr,7002))},
  {"name_reference_id",R(p->kind==ck_ptr_to_member?roslyn_name_reference(p->variant.ptr_to_member.name_reference):0)},
  {"character_kind_id",roslyn_kind_edg_character(static_cast<a_character_kind>(p->character_kind))},
  {"implicit_cast",static_cast<int>(p->implicit_cast)},{"explicit_cast_applied",static_cast<int>(p->explicit_cast_applied)},
  {"is_reinterpret_cast",static_cast<int>(p->is_reinterpret_cast)},{"is_reinterpret_like_cast",static_cast<int>(p->is_reinterpret_like_cast)},
  {"is_simple_zero",static_cast<int>(p->is_simple_zero)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)}});
 return id;
}
