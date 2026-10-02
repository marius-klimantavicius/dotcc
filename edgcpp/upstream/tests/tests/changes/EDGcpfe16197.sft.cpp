//type:fp
//options_all:--c++11
//remark:[4.10.1] Assertion failure in constant_value_at_address with indirect base member
// 5/12/15  [EDGcpfe/16197]
//
// Assertion failure in constant_value_at_address with indirect base member
//
// An attempt to access an indirect base class member in a derived class,
// where intermediate base classes have no direct members, previously aborted
// with an assertion failure in constant_value_at_address.  This is now fixed.
struct A {
  constexpr A(float r) : v(r) { }
  float v;
};
struct B : A {
  constexpr B(float r) : A(r) { }
};
struct C : B {
  constexpr C(float r) : B(r) { }
};
struct D {
  constexpr D(const C& c) : w{c.v} { }  // Previously aborted
  float w;
};
void f() {
  D d(C(11));
}
