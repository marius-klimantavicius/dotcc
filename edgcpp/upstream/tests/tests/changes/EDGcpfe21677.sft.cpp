//type:fp
//options_all:--gn 80000 -tused
//remark:[6.2] Constexpr interpreter aborts when copying capture of "this"
// 8/26/20  [EDGcpfe/21677,EDGcpfe/23263]
//
// Constexpr interpreter aborts when copying capture of "this"
//
// In some relatively complex situations, the constexpr interpreter could abort
// (dereferencing a null pointer) when evaluating the copy of a closure for a
// lambda that captures a "this" pointer.
//
// That problem is now fixed.
template<typename T> struct S {
  S(S const&) = default;
  S(S&) = default;
};
template<typename F> void g(F) {}
struct X {
  template<typename F> void eval(F f){
    g(f);
  }
};
struct Z {
  void f(S<int> p) {
    x.eval([this, p]{});  // Previously triggered an abort.
  }
  X x;
};
