//type:fp
//options_all:--c++14
// 8/28/26  [EDGcpfe/25498]
//
// Using unknown references in constant expressions
//
// The standardization committee's paper P2280R4 relaxes the rules for constant
// expressions so that a reference whose referent is not known to the constant
// evaluator, such as a reference parameter of the function being compiled, can
// be used in a constant expression as long as the result does not depend on the
// object the reference is bound to.  The front end has now been updated
// accordingly.
//
// The same holds for class member access through the "this" pointer:
using Sz = decltype(sizeof(0));
template<class T, Sz N> constexpr Sz f(T (&)[N]) {
  return N;
}
struct S { enum { e = 42 }; };
struct X { static constexpr Sz r() { return 3; } };
void g(char (&arr)[8], S &s, X &x) {
  constexpr auto n = f(arr);
  constexpr int h = s.e;
  static_assert(x.r() == 3, "");
}

struct Outer {
  struct Inner { constexpr int f() const { return 42; } } inside;
  void check() {
    static_assert(inside.f() == 42, "");
  }
};
