//type:fp
//remark:[4.1] Assertion failure when return value optimization variable used as an rvalue
// 6/10/09  [EDGcpfe/9884]
//
// Assertion failure when return value optimization variable used as an rvalue
//
// When DO_RETURN_VALUE_OPTIMIZATION_IN_LOWERING is TRUE and a function to
// which the return value optimization applies is lowered, if it contains
// a use of the return variable as an rvalue in the function, the
// type of the expression will not match the type of the substituted temporary
// variable.  In C generating back end configurations, this could lead to an
// internal error ("check_type_of_variable_node: enk_variable has wrong type").
// This is a 4.0 regression and is now fixed.
struct A {
  ~A();
};
void g(A x);
A f(void) {
  A x;
  g(x);
  return x;
}
