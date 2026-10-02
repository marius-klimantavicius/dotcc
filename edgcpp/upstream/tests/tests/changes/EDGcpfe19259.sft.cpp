//type:fp
//options_all:--c++11
//remark:[6.8] Assertion failure in lower_dynamic_init
// 5/14/25  [EDGcpfe/19259,EDGcpfe/28050]
//
// Assertion failure in lower_dynamic_init
//
// Some initializations that involved the question mark operator had produced
// an assertion error (in lower_dynamic_init) when lowered.
// with --c++11:
struct A {
  int y, z;
};
struct B {
  A a;
};
int f(int i) {
  B b{i > 2 ? A{1, i} : A{}};     // No longer fails an assertion.
  return b.a.z;
}
