//type:fp
//remark:[6.7] Assertion failure in node_operands_have_correct_value_category
// 11/6/24  [EDGcpfe/27702]
//
// Assertion failure in node_operands_have_correct_value_category
//
// In some cases that use inlining, an assertion failure in
// node_operands_have_correct_value_category could occur during the inlining
// process.
struct A {
  A(const char *a) : _a((0, a)) {}
  const char *_a;
};
void g(A);
void f(char* x) {
  g(A(x));
}
