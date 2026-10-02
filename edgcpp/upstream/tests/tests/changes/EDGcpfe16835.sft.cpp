//type:fp
//options_all:--c++11
//remark:[4.12] Parameter packs in attribute arguments
// 6/6/16   [EDGcpfe/16835,EDGcpfe/17222]
//
// Parameter packs in attribute arguments
//
// The changes for EDGcpfe/15940 added the mechanism for variadic parameter
// packs in attribute (or alignas) arguments, but the parameter pack expansion
// was never performed.  That has now been fixed.
template <class... T> struct alignas(T...) A {};
template <int... T> struct alignas(T...) B {};
void f() {
  static_assert(alignof(A<char, double, int>) == alignof(double), "");
  static_assert(alignof(B<1, 8, 4>) == 8, "");
}
