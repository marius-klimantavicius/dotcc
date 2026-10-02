//type:fp
//options_all:--c++14 -tused --gn 60000
//remark:[4.14] Spurious constexpr initialization error with class type subobject
// 3/16/17  [EDGcpfe/18068]
//
// Spurious constexpr initialization error with class type subobject
//
// In some cases, the front end failed to evaluate a constexpr constructor
// claiming an uninitialized class type subobject.
//
// In C++11 mode (but not C++14 mode), this was a regression introduced by
// version 4.13 of the front end.  Now fixed.
struct A {
  constexpr A(int i = 0) : i(i) {}
  int i;
};
struct B {
  constexpr B(A a = A{}) : a(a) {}
  A a;
};
struct C {
  constexpr C() {}
  B b;
};
C constexpr c;  // Previously an error in some configurations.  Now okay.
