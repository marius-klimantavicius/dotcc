//type:fp
//options_all:--gn 80300
//remark:[6.3] Spurious GNU-mode ambiguity on member overloaded with using-declaration
// 9/28/21  [EDGcpfe/24725]
//
// Spurious GNU-mode ambiguity on member overloaded with using-declaration
//
// The changes for EDGcpfe/21627 introduced a regression (in version 6.0) in some
// GNU C++ modes in situations where a member template is overloaded with a member
// projected by a using-declaration when the originating base is a template class
// and the derived class is not.  This resulted in spurious ambiguity errors.
//
// That is now fixed: The example is accepted in GNU C++11 modes with gnu_version
// >= 70000 (GCC versions corresponding to lower gnu_version values emit the same
// ambiguity errors as the front end does).
template<typename> struct B {
  template<typename T> void f(T);
};

struct D: public B<D> {
  using B<D>::f;
  template<typename T> int f(T);
} d;
auto r = d.f(42);  // Previously an ambiguity error in GNU C++11 modes.  
                   // Now okay when gnu_version >= 70000.
