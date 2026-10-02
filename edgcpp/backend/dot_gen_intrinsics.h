// Match c_gen_be.c::dump_intrinsic_call. Constant evaluation has already
// happened in EDG; a residual runtime query is false, and laundering preserves
// the storage address (the managed backend does not cache C++ object values).
string intrinsic_call(an_expr_node_ptr call) {
  auto routine_expression=call->variant.operation.operands;
  if(!routine_expression || routine_expression->kind!=enk_routine)return "";
  auto routine=routine_expression->variant.routine.ptr;
  if(!special_kind_is(routine,sfk_none))return "";
  auto first=routine_expression->next;
  switch(routine->variant.builtin_function_kind) {
    case bfk_is_constant_evaluated:
      if(first)unsupported("constant-evaluation builtin arguments");
      return "(("+type(call->type)+")0)";
    case bufk_launder:
      if(!first || first->next)unsupported("launder builtin arguments");
      return "("+expr(first)+")";
    default:return "";
  }
}
