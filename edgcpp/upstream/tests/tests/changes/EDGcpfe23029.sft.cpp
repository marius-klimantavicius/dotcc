//type:fp
//options_all:--c++14
//remark:[6.3] Inheriting copy/move constructors with default arguments
// 6/17/21  [EDGcpfe/23029,EDGcpfe/24157,EDGcpfe/24262]
//
// Inheriting copy/move constructors with default arguments
//
// The changes to the implementation of inheriting constructors for
// EDGcpfe/17687 (in version 6.1) introduced a regression for cases in which a
// class copy/move constructor that has a default argument is inherited by a
// derived class.  This is now fixed.
struct B {
  B(const B&, int = 0) { }
};
class D : B {
  using B::B;
  void f(const D &d) {
    D x(d, 1);  // Previously an error, "no instance of constructor matches"
  }
};
