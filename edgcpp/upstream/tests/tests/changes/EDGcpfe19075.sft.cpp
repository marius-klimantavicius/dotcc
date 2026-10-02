//type:fp
//remark:[5.0] Abort in lowering on use of __FUNCDNAME__ in generic lambda
// 1/30/18  [EDGcpfe/19075]
//
// Abort in lowering on use of __FUNCDNAME__ in generic lambda
//
// In Microsoft C++ modes that perform lowering, the front end previously aborted
// with an internal error (in function call_operator_function_type_for_lambda in
// lower_name.c) on uses of __FUNCDNAME__ in a generic lambda.
//
// This is now fixed.
auto g = [](auto a) {
  char const *n = __FUNCDNAME__;  // Previously triggered an internal error.
};
