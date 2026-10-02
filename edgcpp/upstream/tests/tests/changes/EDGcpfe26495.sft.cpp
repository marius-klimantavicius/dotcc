//type:fp
//options_all:--c++20
//remark:[6.6] Inherited constructor only called in a constraint
// 7/21/23  [EDGcpfe/26495]
//
// Inherited constructor only called in a constraint
//
// Previously, this triggered a spurious error claiming the constraint couldn't
// succeed because the inherited constructor D::B(int) lacks a definition.  That
// is now fixed.
struct B {
  unsigned u;
  constexpr B(unsigned x): u{x} {}
};
struct D: B {
  using B::B;
};
template<typename T> concept C = static_cast<T>(1).u == 1;
template<C T> void g(T) {}
void f(D x) {
  g(x);
}
