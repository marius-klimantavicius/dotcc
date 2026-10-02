//type:fp
//remark:[4.12] User-defined conversions in nontype template arguments
// 6/7/16   [EDGcpfe/17002]
//
// User-defined conversions in nontype template arguments
//
// The front end previously sometimes failed to consider constexpr user-defined
// conversion functions when matching a nontype template argument to its
// corresponding parameter.
//
// This is now fixed.
struct B {
  constexpr operator bool() const { return true; }
};
template<bool> struct F;
template<> struct F<true> {
  using Type = float;
};
template<int> struct C {
  static constexpr B cond = B{};
};
template<int N>
typename F<C<N>::cond>::Type f();
void g() {
  f<0>();  // Previously triggered a spurious error because the conversion
}          // of C<0>::cond to bool was not considered.
