#include "dot_gen_integer_intrinsics.h"
// GNU floating classification builtins have special, non-promoted argument
// lists. Handle them before the ordinary variadic ABI, as c_gen_be preserves
// these as compiler intrinsics instead of materializing a C va_list.
string floating_intrinsic_call(an_expr_node_ptr call,a_routine_ptr routine,an_expr_node_ptr first) {
  auto builtin=routine->variant.builtin_function_kind;
  string body;unsigned expected=1;
  switch(builtin) {
    case bfk_isnan:case bfk_isnanf:case bfk_isnanl:body="global::Edg.Runtime.LibcFloat.Classify(a0)==0?1:0";break;
    case bfk_isfinite:body="global::Edg.Runtime.LibcFloat.IsFinite(a0)";break;
    case bfk_isnormal:body="global::Edg.Runtime.LibcFloat.Classify(a0)==4?1:0";break;
    case bfk_issubnormal:body="global::Edg.Runtime.LibcFloat.Classify(a0)==3?1:0";break;
    case bfk_isinf:case bfk_isinff:case bfk_isinfl:body="global::Edg.Runtime.LibcFloat.Classify(a0)==1?1:0";break;
    case bfk_isinf_sign:body="global::Edg.Runtime.LibcFloat.Classify(a0)==1?(global::Edg.Runtime.LibcFloat.SignBit(a0)!=0?-1:1):0";break;
    case bfk_signbit:case bfk_signbitf:case bfk_signbitl:body="global::Edg.Runtime.LibcFloat.SignBit(a0)";break;
    case bfk_isgreater:expected=2;body="a0>a1?1:0";break;
    case bfk_isgreaterequal:expected=2;body="a0>=a1?1:0";break;
    case bfk_isless:expected=2;body="a0<a1?1:0";break;
    case bfk_islessequal:expected=2;body="a0<=a1?1:0";break;
    case bfk_islessgreater:expected=2;body="a0<a1 || a0>a1?1:0";break;
    case bfk_isunordered:expected=2;body="global::Edg.Runtime.LibcFloat.Classify(a0)==0 || global::Edg.Runtime.LibcFloat.Classify(a1)==0?1:0";break;
    case bfk_fpclassify:expected=6;body="global::Edg.Runtime.LibcFloat.Classify(a5) switch {0=>a0,1=>a1,2=>a4,3=>a3,4=>a2,_=>a0}";break;
    default:return "";
  }
  unsigned count=0;int floating_rank=0;string signature,args,key="floating-builtin:"+std::to_string((unsigned)builtin);
  for(auto a=first;a;a=a->next,++count) {
    string ty=type(a->type);
    if(builtin==bfk_fpclassify && count<5) {
      if(ty!="int")unsupported("floating classifier selector ABI");
    } else {
      int rank=ty=="global::System.Half"?1:ty=="float"?2:ty=="double"?3:ty=="global::Edg.Runtime.Float80"?4:ty=="global::Edg.Runtime.Float128"?5:0;
      auto operand_type=get_underlying_type(a->type);
      if(!rank && !(expected==2 && (operand_type->kind==tk_integer || operand_type->kind==tk_enum)))
        unsupported("floating builtin operand type");
      floating_rank=std::max(floating_rank,rank);
    }
    if(count){signature+=", ";args+=", ";}
    signature+=ty+" a"+std::to_string(count);args+=expr(a);key+=":"+ty;
  }
  if(count!=expected || type(call->type)!="int" || !floating_rank)unsupported("floating builtin signature");
  // GCC's comparison predicates apply the usual arithmetic conversion even
  // though their special ellipsis argument list retains each operand type.
  // At least one operand is floating; integer operands convert to that common
  // floating type, and every operand is still evaluated once by the call.
  string preamble;
  if(expected==2) {
    string common=floating_rank==5?"global::Edg.Runtime.Float128":floating_rank==4?"global::Edg.Runtime.Float80":floating_rank==3?"double":floating_rank==2?"float":"global::System.Half";
    preamble=common+" c0=("+common+")a0,c1=("+common+")a1;";
    for(const string &id:{string("a0"),string("a1")}) {
      std::size_t position=0;
      while((position=body.find(id,position))!=string::npos){body[position]='c';position+=2;}
    }
  }
  if(floating_rank==1 || floating_rank==5) {
    string from="global::Edg.Runtime.LibcFloat",to="global::Edg.Runtime.ExtendedFloat";
    size_t position=0;while((position=body.find(from,position))!=string::npos){body.replace(position,from.size(),to);position+=to.size();}
  }
  auto found=assignment_helpers.find(key);string id;
  if(found!=assignment_helpers.end())id=found->second;
  else {
    id="__edg_float_builtin"+std::to_string(++serial);assignment_helpers[key]=id;
    helper_output+="// dot-gen:member-begin\nprivate static int "+id+"("+signature+") {"+preamble+"return "+body+";}\n// dot-gen:member-end\n";
  }
  return id+"("+args+")";
}

// Match c_gen_be.c::dump_intrinsic_call. Constant evaluation has already
// happened in EDG; a residual runtime query is false, and laundering preserves
// the storage address (the managed backend does not cache C++ object values).
string intrinsic_call(an_expr_node_ptr call) {
  auto routine_expression=call->variant.operation.operands;
  if(!routine_expression || routine_expression->kind!=enk_routine)return "";
  auto routine=routine_expression->variant.routine.ptr;
  if(!special_kind_is(routine,sfk_none))return "";
  auto first=routine_expression->next;
  string coroutine=coroutine_intrinsic(call,routine,first);
  if(!coroutine.empty())return coroutine;
  string integer=integer_intrinsic_call(call,routine,first);
  if(!integer.empty())return integer;
  string floating=floating_intrinsic_call(call,routine,first);
  if(!floating.empty())return floating;
  switch(routine->variant.builtin_function_kind) {
    case bfk_unreachable: {
      // c_gen_be preserves this intrinsic for the native C compiler. Reaching
      // it is undefined; use a non-returning managed boundary, not a C++ throw.
      if(first || skip_typerefs(call->type)->kind!=tk_void)unsupported("unreachable builtin arguments");
      string key="intrinsic:unreachable";
      if(!assignment_helpers.count(key)) {
        assignment_helpers[key]="__edg_unreachable";
        helper_output+="// dot-gen:member-begin\n[global::System.Diagnostics.CodeAnalysis.DoesNotReturn]\nprivate static void __edg_unreachable() {global::System.Environment.FailFast(\"Reached __builtin_unreachable\");}\n// dot-gen:member-end\n";
      }
      return "__edg_unreachable()";
    }
    case bfk_constant_p:
      // expr.c folds proven constants. A surviving function-scope query is
      // optimization-dependent (the C backend delegates it to GCC). Choosing
      // conservative false is valid; its operand must never be evaluated.
      if(!first || first->next)unsupported("constant-p builtin arguments");
      return "(("+type(call->type)+")0)";
    case bfk_is_constant_evaluated:
      if(first)unsupported("constant-evaluation builtin arguments");
      return "(("+type(call->type)+")0)";
    case bufk_launder:
      if(!first || first->next)unsupported("launder builtin arguments");
      return "("+expr(first)+")";
    default:return "";
  }
}
