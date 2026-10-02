//type:fp
//options_all:--c++17
//remark:[6.4] Assertion failure in lower_dynamic_init for nested lambdas
// 7/12/22  [EDGcpfe/25420]
//
// Assertion failure in lower_dynamic_init for nested lambdas
//
// In configurations that use lowering, the front end had aborted with an
// assertion failure in lower_dynamic_init when (1) a lambda is nested within
// another lambda, (2) the inner lambda captures a constexpr variable and a
// non-constexpr variable, (3) the outer lambda is a generic lambda, and (4) the
// outer lambda is invoked.  Now fixed.
void f(int, int);
void g() {
  int a = 3;
  constexpr int b = 4;
  auto lam = [=](auto) {
                          [=](){ f(a, b); };
                       };
  lam(2);
}
