// The C backend preserves _Atomic-qualified accesses for the native compiler.
// Managed storage uses the unqualified layout; every lvalue conversion and
// mutation routes through the shared AtomicOps synchronization domain.
bool c11_atomic_type(a_type_ptr t) {return (get_type_qualifiers(t)&TQ_C11_ATOMIC)!=0;}
string c11_atomic_read(an_expr_node_ptr e,const string &address) {
  string ty=type(e->type),key="c11-load:"+ty,id;
  auto found=assignment_helpers.find(key);
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_atomic_load"+std::to_string(++serial);assignment_helpers[key]=id;
    helper_output+="// dot-gen:member-begin\nprivate static "+ty+" "+id+"("+ty+"* p) { "+ty+
      " value=default; global::Edg.Runtime.AtomicOps.LoadBytes("+std::to_string(skip_typerefs(e->type)->size)+
      "UL,p,&value,5);return value;}\n// dot-gen:member-end\n";
  }
  return id+"("+address+")";
}
string c11_atomic_mutation(an_expr_node_ptr e) {
  if(e->kind!=enk_operation)return "";
  auto a=e->variant.operation.operands;auto b=a?a->next:nullptr;
  if(!a || !c11_atomic_type(a->type))return "";
  string operation;bool post=false,unit=false;
  switch(e->variant.operation.kind) {
    case eok_assign:case eok_bassign:operation="=";break;
    case eok_pre_incr:case eok_post_incr:operation="+";unit=true;post=e->variant.operation.kind==eok_post_incr;break;
    case eok_pre_decr:case eok_post_decr:operation="-";unit=true;post=e->variant.operation.kind==eok_post_decr;break;
    case eok_add_assign:case eok_padd_assign:operation="+";break;
    case eok_subtract_assign:case eok_psubtract_assign:operation="-";break;
    case eok_multiply_assign:operation="*";break;
    case eok_divide_assign:operation="/";break;
    case eok_remainder_assign:operation="%";break;
    case eok_and_assign:operation="&";break;
    case eok_or_assign:operation="|";break;
    case eok_xor_assign:operation="^";break;
    case eok_shiftl_assign:operation="<<";break;
    case eok_shiftr_assign:operation=">>";break;
    default:return "";
  }
  if(!a->is_lvalue || (!unit && !b))unsupported("C11 atomic mutation operands");
  string ty=type(a->type),rt=unit?"int":type(b->type);
  string key="c11-mutate:"+ty+":"+rt+":"+operation+(post?":post":""),id;
  auto found=assignment_helpers.find(key);
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_atomic_mutate"+std::to_string(++serial);assignment_helpers[key]=id;
    string bytes=std::to_string(skip_typerefs(a->type)->size)+"UL";
    string code="// dot-gen:member-begin\nprivate static "+ty+" "+id+"("+rt+" rhs,"+ty+"* p) { ";
    if(operation=="=")code+=ty+" value=unchecked(("+ty+")rhs);global::Edg.Runtime.AtomicOps.StoreBytes("+bytes+",p,&value,5);return value;";
    else {
      string calculation;
      if(skip_typerefs(a->type)->kind==tk_pointer)calculation="old "+operation+" rhs";
      else {
        string promoted=arithmetic_type(a->type,unit?integer_type(ik_int):b->type);
        calculation="(("+promoted+")old) "+operation+" "+((operation=="<<"||operation==">>")?"(int)rhs":"(("+promoted+")rhs)");
      }
      code+=ty+" old=default;global::Edg.Runtime.AtomicOps.LoadBytes("+bytes+",p,&old,5);for(;;){ "+ty+
        " value=unchecked(("+ty+")("+calculation+"));if(global::Edg.Runtime.AtomicOps.CompareExchangeBytes("+bytes+
        ",p,&old,&value,false,5,5))return "+(post?"old":"value")+";}";
    }
    helper_output+=code+"}\n// dot-gen:member-end\n";
  }
  return id+"("+(unit?"1":expr(b))+", "+address(a)+")";
}
