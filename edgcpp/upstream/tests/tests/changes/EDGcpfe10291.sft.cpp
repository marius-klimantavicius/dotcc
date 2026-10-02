//type:fp
//remark:[4.2] Assertion failure when lowering certain pointer to member function comparisons
// 12/7/09  [EDGcpfe/10291]
//
// Assertion failure when lowering certain pointer to member function comparisons
//
// Lowering of a pointer to member function comparison expression in the file
// scope where one of the operands is a pointer to member function constant and
// the constant also appears earlier in the expression had caused an assertion
// failure (in repr_for_ptr_to_member_function_constant in lower_il.c) and is now
// fixed.
struct A {
  void f(void) {}
};
void (A::*p)(void);
bool x = (&A::f, (&A::f == p));
