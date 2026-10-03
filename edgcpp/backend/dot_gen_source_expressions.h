// Source-oriented plans are consumed at statement/control-flow boundaries.
// Existing ABI/EH/atomic/volatile adapters remain responsible for those special
// contracts. No prefix is injected by expr(): a condition's work must stay in
// its branch or iteration, and nested argument work must retain sequencing.
struct source_planned_node {
  string prefix,value,place,pointer;
  source_planned_node(string p="",string v="",string l="",string a=""):
      prefix(std::move(p)),value(std::move(v)),place(std::move(l)),pointer(std::move(a)){}
};
struct source_expression_binding { string value,place,pointer; };
std::map<an_expr_node_ptr,source_expression_binding> source_expression_bindings;
string source_expression_override(an_expr_node_ptr e,bool address) {
  auto found=source_expression_bindings.find(e);
  return found==source_expression_bindings.end()?"":address?found->second.place:found->second.value;
}
string source_expression_address_override(an_expr_node_ptr e) {
  auto found=source_expression_bindings.find(e);
  return found==source_expression_bindings.end()?"":found->second.pointer;
}
struct source_expression_binding_scope {
  std::map<an_expr_node_ptr,source_expression_binding> saved;
  source_expression_binding_scope():saved(source_expression_bindings){}
  ~source_expression_binding_scope(){source_expression_bindings=std::move(saved);}
};
string source_planned_address(an_expr_node_ptr e,const source_planned_node &plan) {
  if(!plan.pointer.empty())return plan.pointer;
  source_expression_binding_scope bindings;
  source_expression_bindings[e]={plan.value,plan.place.empty()?plan.value:plan.place,""};
  return address(e);
}
string source_temporary(const char *purpose){return string("__edg_")+purpose+std::to_string(++serial);}
const char *source_assignment_token(an_expr_operator_kind op) {
  switch(op) {
#define SOURCE_ASSIGN(kind,text) case kind:return text
    SOURCE_ASSIGN(eok_assign,"=");SOURCE_ASSIGN(eok_bassign,"=");
    SOURCE_ASSIGN(eok_add_assign,"+=");SOURCE_ASSIGN(eok_padd_assign,"+=");
    SOURCE_ASSIGN(eok_subtract_assign,"-=");SOURCE_ASSIGN(eok_psubtract_assign,"-=");
    SOURCE_ASSIGN(eok_multiply_assign,"*=");SOURCE_ASSIGN(eok_divide_assign,"/=");
    SOURCE_ASSIGN(eok_remainder_assign,"%=");SOURCE_ASSIGN(eok_and_assign,"&=");
    SOURCE_ASSIGN(eok_or_assign,"|=");SOURCE_ASSIGN(eok_xor_assign,"^=");
    SOURCE_ASSIGN(eok_shiftl_assign,"<<=");SOURCE_ASSIGN(eok_shiftr_assign,">>=");
#undef SOURCE_ASSIGN
    default:return nullptr;
  }
}
bool source_increment(an_expr_operator_kind op) {
  return op==eok_pre_incr || op==eok_pre_decr || op==eok_post_incr || op==eok_post_decr;
}
bool source_simple_value(an_expr_node_ptr e) {
  if((get_type_qualifiers(e->type)&TQ_VOLATILE) || c11_atomic_type(e->type) || volatile_bitfield(e))return false;
  if(e->kind==enk_constant || e->kind==enk_variable || e->kind==enk_routine)return true;
  if(e->kind!=enk_operation)return false;
  auto op=e->variant.operation.kind;
  if(source_assignment_token(op) || source_increment(op) || op==eok_call || op==eok_comma)return false;
  for(auto a=e->variant.operation.operands;a;a=a->next) {
    if(a->kind==enk_field)continue;
    if(!source_simple_value(a))return false;
  }
  return true;
}
bool source_stable_place(an_expr_node_ptr e) {
  if(e->kind==enk_variable)return true;
  if(e->kind!=enk_operation)return false;
  auto op=e->variant.operation.kind;
  return (op==eok_dot_field || op==eok_parens)?source_stable_place(e->variant.operation.operands):false;
}
string source_bitfield_normalize(a_field_ptr field,string value) {
  if(is_bool_type(field->type))return "("+type(field->type)+")("+value+" != 0 ? 1 : 0)";
  value="unchecked((ulong)("+type(field->type)+")("+value+"))";
  if(field->bit_size<64)value="("+value+" & "+source_bit_mask(field->bit_size)+")";
  if(field->bit_field_is_signed)value="((long)("+value+" << "+std::to_string(64-field->bit_size)+") >> "+std::to_string(64-field->bit_size)+")";
  return "unchecked(("+type(field->type)+")("+value+"))";
}
// C# compound assignment requires an implicit RHS-to-target conversion,
// except its integral shift count. C integer/float compound assignments do not.
bool source_implicit_conversion(an_expr_node_ptr value,a_type_ptr target) {
  string from=type(value->type),to=type(target);
  if(from==to)return true;
  auto source_type=skip_typerefs(value->type),target_type=skip_typerefs(target);
  if(source_type->kind==tk_pointer && target_type->kind==tk_pointer)
    return skip_typerefs(target_type->variant.pointer.type)->kind==tk_void;
  if(from.find("global::")!=string::npos || to.find("global::")!=string::npos)return false;
  if(target_type->kind==tk_float)
    return source_type->kind==tk_integer || (source_type->kind==tk_float && source_type->size<=target_type->size);
  if(source_type->kind!=tk_integer || target_type->kind!=tk_integer)return false;
  bool source_signed=int_type_is_signed(source_type),target_signed=int_type_is_signed(target_type);
  if(source_signed==target_signed && source_type->size<=target_type->size)return true;
  if(!source_signed && target_signed && source_type->size<target_type->size)return true;
  // C# permits a representable int constant to an otherwise narrower or
  // unsigned type. Keep the common source form `flags->mode += 2` intact.
  if(from=="int" && value->kind==enk_constant && node_constant(value)->kind==ck_integer) {
    a_boolean overflow;auto integer=value_of_integer_constant(node_constant(value),&overflow);
    if(!overflow && target_type->size<=8) {
      unsigned bits=(unsigned)target_type->size*8;
      if(target_signed)return bits==64 || (integer>=-(1LL<<(bits-1)) && integer<=(1LL<<(bits-1))-1);
      return integer>=0 && (bits==64 || (unsigned long long)integer<=((1ULL<<bits)-1));
    }
  }
  return false;
}
a_field_ptr source_bitfield_lvalue(an_expr_node_ptr e) {
  auto field=bitfield(e);if(field)return field;
  if(e->kind!=enk_operation)return nullptr;
  auto op=e->variant.operation.kind;
  if(source_assignment_token(op) || op==eok_pre_incr || op==eok_pre_decr || op==eok_parens)
    return source_bitfield_lvalue(e->variant.operation.operands);
  return nullptr;
}
source_planned_node source_plan_node(an_expr_node_ptr e,bool discard=false,bool want_place=false);
source_planned_node source_plan_mutation(an_expr_node_ptr e,bool discard,bool want_place) {
  auto op=e->variant.operation.kind;auto lhs=e->variant.operation.operands;auto rhs=lhs->next;
  bool increment=source_increment(op),post=op==eok_post_incr || op==eok_post_decr;
  string token=increment?((op==eok_pre_incr || op==eok_post_incr)?"+=":"-="):source_assignment_token(op);
  auto field=source_bitfield_lvalue(lhs);auto direct_field=bitfield(lhs);
  // These paths retain their already verified memory-order and byte-access
  // protocols. Their semantic localization is separate from ordinary syntax.
  if(c11_atomic_type(lhs->type) || (get_type_qualifiers(lhs->type)&TQ_VOLATILE) || (direct_field && volatile_bitfield(lhs)))return {"",expr(e),e->is_lvalue?location(e):""};
  auto right=increment?source_planned_node{"","1",""}:source_plan_node(rhs);
  string prefix=right.prefix;
  bool direct_address=source_stable_place(lhs) || (!want_place && (increment || source_simple_value(rhs)) && source_simple_value(lhs));
  if(!increment && (!direct_address || (token!="=" && !source_simple_value(rhs)))) {
    string temp=source_temporary("assignment_rhs");prefix+=type(rhs->type)+" "+temp+" = "+right.value+";\n";right.value=temp;
  }
  source_planned_node left;
  if(!direct_field){left=source_plan_node(lhs,false,true);prefix+=left.prefix;}
  string place=direct_field?location(lhs):(left.place.empty()?left.value:left.place);
  string pointer=left.pointer;
  // Capture the containing record rather than taking an illegal property
  // address. This also makes nested C++ bitfield-lvalue assignments possible.
  if(!direct_address || (field && (!discard || want_place))) {
    if(direct_field) {
      auto base=lhs->variant.operation.operands;
      bool receiver_is_pointer=lhs->variant.operation.kind==eok_points_to_field;
      auto receiver=source_plan_node(base,false,!receiver_is_pointer);prefix+=receiver.prefix;
      string temp=source_temporary("bitfield_receiver");
      auto object=receiver_is_pointer?skip_typerefs(base->type)->variant.pointer.type:base->type;
      prefix+=type(object)+"* "+temp+" = "+(receiver_is_pointer?receiver.value:source_planned_address(base,receiver))+";\n";
      place=temp+"->"+name(field,"m");
    } else if(!field) {
      string temp=source_temporary("assignment_target");prefix+=type(lhs->type)+"* "+temp+" = "+source_planned_address(lhs,left)+";\n";place="(*"+temp+")";pointer=temp;
    }
  }
  bool exact_compound=true;
  if(!increment) {
    bool shift=token=="<<=" || token==">>=";
    bool pointer_arithmetic=skip_typerefs(lhs->type)->kind==tk_pointer && (token=="+=" || token=="-=");
    if(shift) {
      if(type(rhs->type)!="int")right.value="unchecked((int)("+right.value+"))";
    }
    // Pointer +=/-= takes an integral displacement, not a value convertible
    // to the pointer type. C# typed pointer arithmetic supplies EDG's element
    // stride; casting the address to an integer here would lose that stride.
    else if(!pointer_arithmetic && !source_implicit_conversion(rhs,lhs->type)) {
      if(token=="=")right.value=convert(lhs->type,right.value);
      else exact_compound=false;
    }
  }
  if(field && (token=="/=" || token=="%="))exact_compound=exact_compound && (type(node_type_after_integral_promotion(lhs))==type(field->type) || type(field->type)=="int");
  if(discard && !want_place && (token=="=" || exact_compound)) {
    string value=increment?(post?place+(token=="+="?"++":"--"):(token=="+="?"++":"--")+place):place+" "+token+" "+right.value;
    return {prefix,value,place,pointer};
  }
  if(!field && !want_place && exact_compound) {
    string value=increment?(post?place+(token=="+="?"++":"--"):(token=="+="?"++":"--")+place):place+" "+token+" "+right.value;
    return {prefix,"("+value+")",place,pointer};
  }
  string old;
  if(post){old=source_temporary("bitfield_previous");prefix+=type(lhs->type)+" "+old+" = "+place+";\n";}
  string value=right.value;
  if(token!="=") {
    string operation=token.substr(0,token.size()-1);
    if(skip_typerefs(lhs->type)->kind==tk_pointer) {
      value=place+" "+operation+" ("+right.value+")";
    } else {
      auto promoted=field?node_type_after_integral_promotion(lhs):lhs->type;
      string calc=(operation=="<<" || operation==">>")?type(promoted):arithmetic_type(promoted,increment?promoted:rhs->type);
      value="(("+calc+")("+place+")) "+operation+" (("+((operation=="<<" || operation==">>")?"int":calc)+")("+right.value+"))";
    }
  }
  value=field?source_bitfield_normalize(field,value):convert(lhs->type,value);
  string stored=source_temporary(field?"bitfield_value":"assigned_value");
  prefix+=type(lhs->type)+" "+stored+" = "+value+";\n"+place+" = "+stored+";\n";
  return {prefix,discard && !want_place?"":post?old:stored,place,pointer};
}
source_planned_node source_plan_node(an_expr_node_ptr e,bool discard,bool want_place) {
  if(!e)return {};
  if(e->kind==enk_constant && node_constant(e)->kind==ck_void)return {};
  // Like location(), consume public type_info views before descending into
  // their synthetic native RTTI variable, which is intentionally not storage.
  string metadata=typeinfo_expression(e);
  if(!metadata.empty())return {"",metadata,metadata,"&("+metadata+")"};
  if(e->kind!=enk_operation)return {"",expr(e),e->is_lvalue?location(e):"",e->is_lvalue?address(e):""};
  auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;auto b=a?a->next:nullptr;
  if(source_assignment_token(op) || source_increment(op))return source_plan_mutation(e,discard,want_place);
  if(e->variant.operation.eval_right_to_left && b)unsupported("right-to-left lowered operation",op);
  if(op==eok_call && a && a->kind==enk_routine && a->variant.routine.ptr->compiler_generated)
    return {"",expr(e),e->is_lvalue?location(e):"",e->is_lvalue?address(e):""};
  if(op==eok_comma) {
    auto effect=plan_source_effect(a);auto tail=source_plan_node(b,discard,want_place);tail.prefix=effect.statements+tail.prefix;return tail;
  }
  if(op==eok_cast && skip_typerefs(e->type)->kind==tk_void) {
    auto effect=plan_source_effect(a);return {effect.statements,"",""};
  }
  if(op==eok_land || op==eok_lor || op==eok_question) {
    // Conditional lvalue bitfields cannot be represented by ref/pointer cells;
    // retain the established IL-specific adapter until localized branching is
    // requested by a mutation plan that can carry both property alternatives.
    if(want_place && e->is_lvalue)return {"",expr(e),location(e)};
    auto test=plan_source_condition(a);
    auto yes=op==eok_question?source_plan_node(b,discard):source_planned_node{};
    auto no=op==eok_question?source_plan_node(b->next,discard):source_planned_node{};
    if(op!=eok_question) {
      auto other=plan_source_condition(b);
      if(other.prefix.empty())return {test.prefix,"(("+test.value+(op==eok_land?" && ":" || ")+other.value+") ? 1 : 0)",""};
      string temp=source_temporary("condition");string prefix=test.prefix+"bool "+temp+" = "+test.value+";\nif ("+(op==eok_land?temp:"!"+temp)+") {\n"+other.prefix+temp+" = "+other.value+";\n}\n";
      return {prefix,"("+temp+" ? 1 : 0)",""};
    }
    if(skip_typerefs(e->type)->kind==tk_void)return {test.prefix+"if ("+test.value+") {\n"+yes.prefix+(yes.value.empty()?"":yes.value+";\n")+"} else {\n"+no.prefix+(no.value.empty()?"":no.value+";\n")+"}\n","",""};
    if(yes.prefix.empty() && no.prefix.empty())return {test.prefix,"("+test.value+" ? "+yes.value+" : "+no.value+")",""};
    string temp=source_temporary("conditional_value");
    return {test.prefix+type(e->type)+" "+temp+";\nif ("+test.value+") {\n"+yes.prefix+temp+" = "+yes.value+";\n} else {\n"+no.prefix+temp+" = "+no.value+";\n}\n",temp,""};
  }
  // Preserve special lowered lifetime/EH/intrinsic semantics in existing
  // emitters, but replace ordinary child values with their localized plans.
  source_expression_binding_scope bindings;
  std::vector<std::pair<an_expr_node_ptr,source_planned_node>> children;
  for(auto child=a;child;child=child->next) {
    if(child->kind==enk_field || child->kind==enk_routine)continue;
    bool place=child==a && (op==eok_address_of || op==eok_reference_to || op==eok_array_to_pointer || op==eok_dot_field || op==eok_lvalue_adjust || (op==eok_parens && want_place));
    children.push_back({child,source_plan_node(child,false,place)});
  }
  string prefix;
  for(size_t i=0;i<children.size();++i) {
    auto child=children[i].first;auto value=children[i].second;prefix+=value.prefix;
    bool later=false;for(size_t j=i+1;j<children.size();++j)later|=!children[j].second.prefix.empty();
    bool place=child==a && (op==eok_address_of || op==eok_reference_to || op==eok_array_to_pointer || op==eok_dot_field || op==eok_lvalue_adjust || (op==eok_parens && want_place));
    if(later) {
      string temp=source_temporary(place?"operand_address":"operand_value");
      if(place){prefix+=type(child->type)+"* "+temp+" = "+source_planned_address(child,value)+";\n";value.place="(*"+temp+")";value.value=value.place;value.pointer=temp;}
      else {prefix+=type(child->type)+" "+temp+" = "+value.value+";\n";value.value=temp;value.place=temp;value.pointer="";}
    }
    source_expression_bindings[child]={value.value,value.place.empty()?value.value:value.place,value.pointer};
  }
  return {prefix,expr(e),e->is_lvalue?location(e):"",e->is_lvalue && !bitfield(e)?address(e):""};
}
source_expression_plan plan_source_value(an_expr_node_ptr e) {auto p=source_plan_node(e);return {p.prefix,p.value};}
source_expression_plan plan_source_condition(an_expr_node_ptr e) {
  if(!e)return {};
  if(e && e->kind==enk_operation) {
    auto op=e->variant.operation.kind;auto a=e->variant.operation.operands;auto b=a?a->next:nullptr;
    if(op==eok_land || op==eok_lor) {
      auto left=plan_source_condition(a),right=plan_source_condition(b);
      if(right.prefix.empty())return {left.prefix,"("+left.value+(op==eok_land?" && ":" || ")+right.value+")"};
      string temp=source_temporary("condition");
      return {left.prefix+"bool "+temp+" = "+left.value+";\nif ("+(op==eok_land?temp:"!"+temp)+") {\n"+right.prefix+temp+" = "+right.value+";\n}\n",temp};
    }
    if(op==eok_not){auto p=plan_source_condition(a);p.value="!("+p.value+")";return p;}
    if(op==eok_cast || op==eok_parens) {
      if(op==eok_parens || is_bool_type(e->type))return plan_source_condition(a);
    }
    const char *token=op==eok_eq?"==":op==eok_ne?"!=":op==eok_lt?"<":op==eok_le?"<=":op==eok_gt?">":op==eok_ge?">=":nullptr;
    if(token && !function_pointer(a->type)) {
      auto left=plan_source_value(a),right=plan_source_value(b);string prefix=left.prefix;
      if(!right.prefix.empty()){string temp=source_temporary("comparison_left");prefix+=type(a->type)+" "+temp+" = "+left.value+";\n";left.value=temp;}
      auto at=skip_typerefs(a->type),bt=skip_typerefs(b->type);
      if((at->kind==tk_integer || at->kind==tk_float) && (bt->kind==tk_integer || bt->kind==tk_float)) {
        auto promoted_a=at->kind==tk_integer?node_type_after_integral_promotion(a):a->type;
        auto promoted_b=bt->kind==tk_integer?node_type_after_integral_promotion(b):b->type;
        string common=arithmetic_type(promoted_a,promoted_b);
        if(type(a->type)!=common)left.value="unchecked(("+common+")("+left.value+"))";
        if(type(b->type)!=common)right.value="unchecked(("+common+")("+right.value+"))";
      }
      return {prefix+right.prefix,left.value+" "+token+" "+right.value};
    }
  }
  auto p=plan_source_value(e);return {p.prefix,"("+p.value+(skip_typerefs(e->type)->kind==tk_pointer?" != null)":" != 0)")};
}
source_effect_plan plan_source_effect(an_expr_node_ptr e) {
  if(!e)return {};
  if(e->kind==enk_constant && node_constant(e)->kind==ck_void)return {};
  if(e->kind!=enk_operation)return {expression_effect(e),""};
  if(e->variant.operation.kind==eok_comma) {
    auto first=e->variant.operation.operands;
    auto left=plan_source_effect(first),right=plan_source_effect(first->next);
    string header;
    if((left.statements.empty() || !left.header.empty()) &&
       (right.statements.empty() || !right.header.empty())) {
      header=left.header;
      if(!right.header.empty())header+=(header.empty()?"":", ")+right.header;
    }
    return {left.statements+right.statements,header};
  }
  auto p=source_plan_node(e,true);auto op=e->variant.operation.kind;
  if(p.value.empty())return {p.prefix,""};
  bool direct=(source_assignment_token(op) || source_increment(op) || skip_typerefs(e->type)->kind==tk_void) && p.value.compare(0,10,"unchecked(")!=0;
  string statement=(direct?"":"_ = ")+p.value;
  return {p.prefix+statement+";\n",p.prefix.empty()?statement:""};
}
