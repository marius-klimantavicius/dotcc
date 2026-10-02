//type:fp
//options_all:--c++14
//remark:[4.10.1] Assertion failure in lower_param_ref
// 1/13/15  [EDGcpfe/15845]
//
// Assertion failure in lower_param_ref
//
// An assertion failure (in lower_param_ref) had occurred when lowering certain
// aggregates in C++14 mode.  This is now fixed.
struct B {
  int x = 37;
  int y = f();
  int f() { return x;}
};
struct A {
  B b {};
} a;
