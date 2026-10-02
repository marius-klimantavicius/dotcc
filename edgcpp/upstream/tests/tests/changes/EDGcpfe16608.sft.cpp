//type:fp
//options_all:--microsoft
//remark:[4.11] Assertion failure with unevaluated expressions in lambdas
// 11/15/15 [EDGcpfe/16608]
//
// Assertion failure with unevaluated expressions in lambdas
//
// As a result of the changes for EDGcpfe/16292 et al., an assertion failure
// (in lower_expr_full) had been triggered when a local variable of an enclosing
// routine appeared in an unevaluated context within a lambda.  The assertion
// failure has been corrected, but back ends should be prepared to handle the case
// where a variable with automatic storage is referred to from another scope (both
// will have the same memory region).
void f(int n) {
  auto lm = []{ __assume(n); };
}
