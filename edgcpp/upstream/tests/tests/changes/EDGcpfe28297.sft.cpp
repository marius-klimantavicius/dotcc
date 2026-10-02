//type:fp
//options_all:--gn 999999 --c++20
//remark:[6.8] Spurious error on some fold-expressions
// 8/29/25  [EDGcpfe/28297]
//
// Spurious error on some fold-expressions
//
// This previously triggered an error in the instantiation of the fold expression.
// That is now fixed.
template<int ... Ns> struct S {
  static auto f() { return (1 + Ns + ...); }
};
int r = S<1, 2, 3, 4>::f();
