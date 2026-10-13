/* Values and occurrences are separate. Only exact typed value rows are interned. */
#if FIXED_POINT_ALLOWED
static a_fixed_point_type_descr roslyn_fixed_point_descriptor(a_type_ptr type) {
 a_type_ptr effective=skip_typerefs(type);
 if(effective->kind==tk_fixed_point)return effective->variant.fixed_point;
 /* Stock lowering changes the cached canonical fixed-point descriptor into
    a typeref in place.  Existing public cache entries retain its identity;
    their exact descriptor keys remain valid after frontend wrapup. */
 if(in_front_end||curr_translation_unit!=nullptr||!is_primary_translation_unit){
  DB.fail("Fixed-point cache query requires completed primary frontend wrapup");
  return make_fixed_point_type_descr(fpp_default,FALSE,FALSE,FALSE);
 }
 for(int precision=fpp_short;precision<fpp_last;++precision)
  for(int is_unsigned=0;is_unsigned!=2;++is_unsigned)
   for(int is_fract=0;is_fract!=2;++is_fract)
    for(int saturating=0;saturating!=2;++saturating){
     auto descriptor=make_fixed_point_type_descr(static_cast<a_fixed_point_precision>(precision),is_unsigned,is_fract,saturating);
     if(!fixed_point_type_used_in_primary_IL(descriptor))continue;
     a_type_ptr cached=fixed_point_type(descriptor);
     for(a_type_ptr current=type;current;current=current->kind==tk_typeref?current->variant.typeref.type:nullptr)
      if(current==cached)return descriptor;
    }
 DB.fail("Retained fixed-point constant has no exact canonical descriptor association");
 return make_fixed_point_type_descr(fpp_default,FALSE,FALSE,FALSE);
}
#endif
static a_float_kind roslyn_constant_float_kind(a_constant_ptr constant) {
 a_type_ptr effective=skip_typerefs(constant->type);
 if(effective->kind==tk_float||effective->kind==tk_imaginary||effective->kind==tk_complex)
  return effective->variant.float_kind;
#if C99_IL_EXTENSIONS_SUPPORTED && DO_IL_LOWERING
 if(in_front_end||curr_translation_unit!=nullptr||!is_primary_translation_unit){
  DB.fail("Complex cache query requires completed primary frontend wrapup");return fk_float;
 }
 for(int kind=fk_float;kind<fk_last;++kind){
  auto float_kind=static_cast<a_float_kind>(kind);a_type_ptr cached=nullptr;
  if(constant->kind==ck_complex&&complex_type_used_in_primary_IL(float_kind))cached=complex_type(float_kind);
#if LOWER_COMPLEX
  if(constant->kind==ck_imaginary&&imaginary_type_used_in_primary_IL(float_kind))cached=imaginary_type(float_kind);
#endif
  if(!cached)continue;
  for(a_type_ptr current=constant->type;current;current=current->kind==tk_typeref?current->variant.typeref.type:nullptr)
   if(current==cached)return float_kind;
 }
#endif
 DB.fail("Retained floating constant has no exact type-kind association");return fk_float;
}
static Rid roslyn_subobject_path(a_subobject_path_ptr p) {
 bool fresh;Rid id=DB.claim(p,"subobject_path",fresh);if(!fresh)return id;
 DB.put("subobject_path",{{"id",id},{"kind_id",K("role",p->is_offset?"element":p->is_base_class?"base":"member")},
  {"field_id",R(!p->is_offset&&!p->is_base_class?roslyn_field(p->variant.field):0)},
  {"base_subobject_id",R(p->is_base_class?roslyn_base(p->variant.base_class):0)},
  {"element_offset",p->is_offset?RoslynSqlValue(Q(p->variant.ptr_offset)):RoslynSqlValue()},
  {"next_id",R(roslyn_subobject_path(p->next))},{"is_converted",static_cast<int>(p->is_converted)}});return id;
}
static Rid roslyn_constant_origin(a_constant_ptr constant) {
 Rid point=roslyn_origin(constant->source_corresp.decl_position);
#if EXTRA_SOURCE_POSITIONS_IN_IL
 Rid range=roslyn_origin(constant->source_corresp.decl_position,&constant->end_position);
 return range!=point?range:0;
#else
 return 0;
#endif
}
static Rid roslyn_constant(a_constant_ptr p) {
 bool fresh;Rid id=DB.claim(p,"constant",fresh);if(!fresh)return id;
 roslyn_attach_correspondence(id,p->source_corresp);
 RoslynContent content(p->kind);
 switch(p->kind){
 case ck_void:break;
 case ck_integer:content.add("integer_value",{{"value_id",DB.atom(decimal_str_for_integer_constant(p).as_temp_characters())}});break;
#if UPC_EXTENSIONS_ALLOWED
 case ck_upc_threads:content.add("upc_threads_value",{{"multiplier_id",DB.atom(decimal_str_for_integer_constant(p).as_temp_characters())}});break;
 case ck_upc_mythread:break;
#endif
 case ck_float:
#if C99_IL_EXTENSIONS_SUPPORTED
 case ck_imaginary:
#endif
 roslyn_float_content(content,roslyn_constant_float_kind(p),p->variant.float_value);break;
#if C99_IL_EXTENSIONS_SUPPORTED
 case ck_complex:{auto c=p->variant.complex_value;auto k=roslyn_constant_float_kind(p);content.add("complex_value",{{"real_value_id",roslyn_float_value(k,c->real)},{"imaginary_value_id",roslyn_float_value(k,c->imag)}});break;}
#endif
#if FIXED_POINT_ALLOWED
 case ck_fixed_point:{auto d=roslyn_fixed_point_descriptor(p->type);std::string scaled=str_for_integer_value(&p->variant.fixed_point_value,!d.is_unsigned,FALSE,targ_sizeof_fixed_point[d.is_unsigned][d.precision][d.is_fract_type]).as_temp_characters();content.add("fixed_point_value",{{"scaled_integer_id",DB.atom(scaled)},{"scale",static_cast<int>(targ_fractional_bits_for_fixed_point[d.is_unsigned][d.precision][d.is_fract_type])}});break;}
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
 case ck_ptr_to_member:{auto &m=p->variant.ptr_to_member;Rid member=m.is_function_ptr?roslyn_routine(m.variant.routine):roslyn_field(m.variant.field);auto type=skip_typerefs(p->type);if(type->kind!=tk_ptr_to_member&&p->orig_type){auto original=skip_typerefs(p->orig_type);if(original->kind==tk_ptr_to_member)type=original;}Rid containing=type->kind==tk_ptr_to_member?roslyn_type(type->variant.ptr_to_member.class_of_which_a_member):0;content.add("member_pointer_value",{{"member_id",R(member)},{"containing_type_id",R(containing)},{"subobject_id",R(roslyn_base(m.casting_base_class))},{"is_function_ptr",static_cast<int>(m.is_function_ptr)},{"cast_to_base",static_cast<int>(m.cast_to_base)},{"is_null",static_cast<int>(member==0)}});break;}
#if GNU_EXTENSIONS_ALLOWED
 case ck_label_difference:{auto &l=p->variant.label_difference;roslyn_constant_component(id,"from",0,l.from_address);roslyn_constant_component(id,"to",0,l.to_address);content.add("label_difference_value",{{"from_value_id",roslyn_value_of(l.from_address)},{"from_type_id",R(roslyn_type(l.from_address->type))},{"to_value_id",roslyn_value_of(l.to_address)},{"to_type_id",R(roslyn_type(l.to_address->type))},{"left_label_id",R(l.to_address->kind==ck_address&&l.to_address->variant.address.kind==abk_label?roslyn_label(l.to_address->variant.address.variant.label):0)},{"right_label_id",R(l.from_address->kind==ck_address&&l.from_address->variant.address.kind==abk_label?roslyn_label(l.from_address->variant.address.variant.label):0)}});break;}
#endif
 case ck_aggregate:{content.add("aggregate_value",{{"type_id",roslyn_type(p->type)}});int n=0;for(a_constant_ptr c=p->variant.aggregate.first_constant;c;c=c->next){roslyn_constant_component(id,"element",n,c);content.add("aggregate_element",{{"ordinal",n++},{"value_id",roslyn_value_of(c)},{"type_id",R(roslyn_type(c->type))}},"owner_id");}break;}
 case ck_init_repeat:{auto &r=p->variant.init_repeat;roslyn_constant_component(id,"repeated",0,r.constant);content.add("repeat_value",{{"value_id",roslyn_value_of(r.constant)},{"type_id",R(roslyn_type(r.constant->type))},{"count",Q(r.count)},{"multidimensional_tail_not_repeated",static_cast<int>(r.multidimensional_aggr_tail_not_repeated)}});break;}
 case ck_dynamic_init:content.add("dynamic_initialization_value",{{"initialization_id",roslyn_init(p->variant.dynamic_init.ptr,7002)}});break;
 case ck_template_param:{auto &t=p->variant.template_param;Rid expression=0,type=0,child_value=0,child_type=0;RoslynSqlValue number,depth;int opname=0,special=0;bool standard_alignof=false,unqualified=false;
 switch(t.kind){
 case tpck_param:number=static_cast<sqlite3_int64>(t.variant.coordinates.position);depth=static_cast<sqlite3_int64>(t.variant.coordinates.depth);break;
 case tpck_expression:{an_expr_node_ptr expr=t.variant.expr;
#if PROTOTYPE_INSTANTIATIONS_IN_IL
 if(!expr&&t.local_expr_ref)expr=roslyn_unique_local_expression(p,lerk_tpl_param_expr);
#endif
 expression=roslyn_expr(expr,7002);break;}
 case tpck_member:break;
 case tpck_unknown_function:type=roslyn_type(t.variant.unknown_function.conversion_type);opname=roslyn_kind_edg_opname(t.variant.unknown_function.opname_kind);
#if MICROSOFT_EXTENSIONS_ALLOWED
 special=roslyn_kind_edg_special_function(t.variant.unknown_function.special_kind);
#endif
 break;
 case tpck_address:case tpck_dependent_constant:{auto c=t.variant.constant;roslyn_constant_component(id,"dependent",0,c);child_value=roslyn_value_of(c);child_type=roslyn_type(c->type);break;}
 case tpck_concat_string_literals:{int n=0;for(a_constant_ptr c=t.variant.string_literal_list;c;c=c->next){roslyn_constant_component(id,"dependent",n,c);content.add("dependent_constant_element",{{"ordinal",n++},{"value_id",roslyn_value_of(c)},{"type_id",R(roslyn_type(c->type))}},"owner_id");}break;}
 case tpck_sizeof:case tpck_datasizeof:case tpck_alignof:case tpck_uuidof:case tpck_typeid:case tpck_noexcept:{type=roslyn_type(t.variant.templ_sizeof.type);an_expr_node_ptr expr=t.variant.templ_sizeof.expr;
#if PROTOTYPE_INSTANTIATIONS_IN_IL
 if(!expr&&t.local_expr_ref)expr=roslyn_unique_local_expression(p,lerk_generic_sizeof);
#endif
 expression=roslyn_expr(expr,7002);standard_alignof=t.variant.templ_sizeof.is_std_alignof;break;}
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
 Rid storage=0;
#if DO_IL_LOWERING && !BACK_END_IS_C_GEN_BE && !IL_SHOULD_BE_WRITTEN_TO_FILE
 storage=roslyn_variable(p->assoc_var);
#endif
 std::vector<std::pair<const char*,RoslynSqlValue>> facts={{"id",id},{"type_id",R(roslyn_type(p->type))},{"orig_type_id",R(roslyn_type(p->orig_type))},{"value_id",value},
  {"associated_storage_variable_id",R(storage)},
  {"source_origin_id",R(roslyn_constant_origin(p))},
  {"backing_expression_id",R(roslyn_expr(p->expr?p->expr:p->local_expr_ref?roslyn_unique_local_expression(p,lerk_constant_expr):nullptr,7002))},
  {"name_reference_id",R(p->kind==ck_ptr_to_member?roslyn_name_reference(p->variant.ptr_to_member.name_reference):0)},
  {"character_kind_id",roslyn_kind_edg_character(static_cast<a_character_kind>(p->character_kind))},
  {"implicit_cast",static_cast<int>(p->implicit_cast)},{"explicit_cast_applied",static_cast<int>(p->explicit_cast_applied)},
  {"is_reinterpret_cast",static_cast<int>(p->is_reinterpret_cast)},{"is_reinterpret_like_cast",static_cast<int>(p->is_reinterpret_like_cast)},
  {"is_simple_zero",static_cast<int>(p->is_simple_zero)},{"is_pack_expansion",static_cast<int>(p->is_pack_expansion)}};
#define CONSTANT_FLAG(field) facts.push_back({#field,static_cast<int>(p->field)})
 CONSTANT_FLAG(non_arithmetic);CONSTANT_FLAG(null_pointer_constant_ruled_out);CONSTANT_FLAG(nullptr_keyword);
 CONSTANT_FLAG(explicit_braces_on_aggregate);CONSTANT_FLAG(explicit_parentheses_on_aggregate);CONSTANT_FLAG(from_undefined_preproc_id);
 CONSTANT_FLAG(uses_designated_initializers);CONSTANT_FLAG(is_named_constant_definition);CONSTANT_FLAG(partial_aggr_value);
 CONSTANT_FLAG(is_partially_initialized);CONSTANT_FLAG(implicit_aggr_element);CONSTANT_FLAG(is_compound_literal);
 CONSTANT_FLAG(is_result_of_constexpr_call);CONSTANT_FLAG(is_generic_initializer);CONSTANT_FLAG(constant_for_base_class);
 CONSTANT_FLAG(constant_for_base_class_from_constexpr_folding);CONSTANT_FLAG(folded_statement_expression);
 CONSTANT_FLAG(formed_from_promoted_storage);CONSTANT_FLAG(local_expr_ref);
#if GNU_EXTENSIONS_ALLOWED
 CONSTANT_FLAG(null_keyword);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED
 CONSTANT_FLAG(native_nullptr_keyword);CONSTANT_FLAG(ptr_to_mem_constant_construct);
#endif
#if MICROSOFT_EXTENSIONS_ALLOWED || GNU_EXTENSIONS_ALLOWED
 CONSTANT_FLAG(flexible_array_initializer);
#endif
#if DO_IL_LOWERING
 CONSTANT_FLAG(initializes_empty_object);CONSTANT_FLAG(is_implicit_initialization);CONSTANT_FLAG(vptr_has_been_lowered);
 if(p->initializes_empty_object&&(p->kind==ck_aggregate||p->kind==ck_dynamic_init)){
  const auto &member=p->kind==ck_aggregate?p->variant.aggregate.field_or_base:p->variant.dynamic_init.field_or_base;
  facts.push_back({p->constant_for_base_class?"initialized_base_subobject_id":"initialized_field_id",R(p->constant_for_base_class?roslyn_base(member.base):roslyn_field(member.field))});
 }
#endif
#undef CONSTANT_FLAG
 if(p->kind==ck_string){const auto &s=p->variant.string;
  if(s.literal_kind!=SCLK_NOT_A_LITERAL){const char *prefix=nullptr;switch(literal_encoding_prefix(s.literal_kind)){
   case SCLK_ORDINARY_LITERAL:prefix="ordinary";break;case SCLK_UTF8_LITERAL:prefix="utf8";break;
   case SCLK_CHAR16_T_LITERAL:prefix="utf16";break;case SCLK_CHAR32_T_LITERAL:prefix="utf32";break;
   case SCLK_WIDE_LITERAL:prefix="wide";break;default:DB.fail("Unknown retained literal encoding prefix");break;
  }
  if(prefix)facts.push_back({"literal_encoding_prefix_id",K("literal_encoding_prefix",prefix)});
  facts.push_back({"is_raw_string",static_cast<int>((s.literal_kind&SCLK_RAW_STRING_LITERAL)!=0)});
  facts.push_back({"is_string_literal",static_cast<int>((s.literal_kind&SCLK_STRING_LITERAL)!=0)});
  facts.push_back({"is_function_name_literal",static_cast<int>((s.literal_kind&SCLK_FUNCTION_NAME)!=0)});
  }
  facts.push_back({"contains_function_name_token",static_cast<int>(s.func_name_tok)});facts.push_back({"is_embed_expansion",static_cast<int>(s.embed_expansion)});
#if DO_IL_LOWERING && ASSIGN_STRING_LITERAL_SEQUENCE_NUMBERS
  facts.push_back({"string_sequence_number",Q(s.sequence_number)});
#endif
#if PRESERVE_EMBED_DIRECTIVE_WHEN_OPTIMIZED
  if(s.embed_expansion)facts.push_back({"embed_directive_id",R(DB.atom(s.embed_directive))});
#endif
 }
 if(p->kind==ck_aggregate){facts.push_back({"has_dynamic_init_component",static_cast<int>(p->variant.aggregate.has_dynamic_init_component)});facts.push_back({"added_const_for_template_param",static_cast<int>(p->variant.aggregate.added_const_for_template_param)});}
#if PROTOTYPE_INSTANTIATIONS_IN_IL
 if(p->kind==ck_template_param)facts.push_back({"template_local_expr_ref",static_cast<int>(p->variant.template_param.local_expr_ref)});
#endif
 DB.put_fields("constant",facts);
 return id;
}
