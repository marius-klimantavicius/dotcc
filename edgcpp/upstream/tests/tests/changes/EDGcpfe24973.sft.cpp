//type:fp
//options_all:--c++14
//remark:[6.5] Assertion failure in lower_ctor_init
// 12/21/22 [EDGcpfe/24973]
//
// Assertion failure in lower_ctor_init
//
// In some cases, an assertion failure (in lower_ctor_init) had occurred when
// initializing certain fields in C++14 mode.
struct B {
  B() {}
  int v;
};
struct D : B {};
struct A {
  D d1{};
  D d2{};
};
A a;
