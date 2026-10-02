//type:fp
//options_all:--c++14
//remark:[6.8] Substitution failure for use of enclosing "this" in generic lambda declarator
// 4/7/25   [EDGcpfe/27961]
//
// Substitution failure for use of enclosing "this" in generic lambda declarator
//
// Previously, a (possibly implicit) reference to an enclosing "this" pointer
// appearing in an unevaluated context of a generic lambda declarator resulted in
// a substitution failure.
struct A {
  template<typename T> void g(T);
  void f() {
    [] (auto i) -> decltype(g(i)) { } (1);  // Previously a spurious error.
  }                                         // Now okay.
};
