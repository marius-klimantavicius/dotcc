//type:fp
//options_all:--gn 70300 --c++11
//remark:[5.1] __builtin_offsetof in constexpr evaluations
// 12/10/18 [EDGcpfe/20554]
//
// __builtin_offsetof in constexpr evaluations
//
// The evaluation of __builtin_offsetof expressions in the constexpr interpreter
// did not correctly handle array subscripting.
//
// That is now fixed.
struct S {
  int x[2];
};
constexpr int g(int i) {
  return (int)__builtin_offsetof(S, x[i]);
};
static_assert(g(1) == sizeof(int));  // Previously failed.  Now okay.
