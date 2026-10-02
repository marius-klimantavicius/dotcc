//type:fp
//options_all:--gnu_version 80000
//remark:[6.1] Spurious error on GNU-mode braced list conversion in template definition
// 4/17/20  [EDGcpfe/20436,EDGcpfe/22620]
//
// Spurious error on GNU-mode braced list conversion in template definition
//
// In GNU C++ mode, the front end sometimes issued a spurious error on the
// conversion of a braced list to a class type, when that conversion appears
// in a template definition and the braced list omits some nested braces.
//
// That issue is now fixed.
struct A { A() {} };
struct B { A x, y; };
struct C { B b; };
template<typename> struct D {
  D() {
    A a;
    C{a, a};  // Omits the braces for C::b.  Previously a spurious error
  }           // in GNU C++ mode.  Now okay.
};
D<int> di;
