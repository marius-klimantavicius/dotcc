//type:fp
//options_all:--c++ --microsoft
//remark:[6.7] Microsoft compatibility: access checking on class-scope using-declarations
// 11/29/24 [EDGcpfe/27733]
//
// Microsoft compatibility: access checking on class-scope using-declarations
//
// The changes for [EDGcpfe/18533,EDGcpfe/21125] (in version 5.1) relaxed access
// checking in Microsoft mode on class-scope using-declarations that denote a
// single function in an indirect base class.  However, the case where the
// using-declaration denoted an overload set was not considered.  That case is now
// also accepted in Microsoft mode.
class A {
  void f();
  void f(int);
};
struct B : A { };
struct D : B {
  using B::f;  // Normally an access error, now accepted in Microsoft mode.
};
