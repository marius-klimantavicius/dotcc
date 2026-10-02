//type:fp
//options_all:--c++14
//remark:[6.5] Incorrect move from lvalue for class with trivial copy constructor
// 3/6/23   [EDGcpfe/25426]
//
// Incorrect move from lvalue for class with trivial copy constructor
//
// For a class with a trivial copy constructor, a functional notation cast with a
// braced init list of an lvalue of the class type would incorrectly call the move
// constructor on that lvalue.
struct C {
  constexpr C() = default;
  constexpr C(const C &) = default;
  constexpr C(C &&c) { c.moved = true; }
  bool moved = false;
};
constexpr bool f(C c) {
  C{c};                              // Previously called C::C(C&&).
  return !c.moved;
}
static_assert(f({}), "Unexpected");  // Previously failed.  Now okay.
