//type:fp
//remark:[4.13] Conversion functions and template arguments
// 12/6/16  [EDGcpfe/16988]
//
// Conversion functions and template arguments
//
// When constexpr is enabled, the front end now considers conversion functions to
// match nontype template arguments to their parameters.
template<int N> int g() { return N; }
struct X {
  constexpr operator int() const { return 42; }
};
constexpr X x{};
int r = g<x>();  // Previously an error.  Now okay.
