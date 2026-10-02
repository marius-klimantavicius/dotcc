//type:fp
//options_all:--c++14
//remark:[6.3] Constant-evaluation of address comparisons
// 8/10/21  [EDGcpfe/24266]
//
// Constant-evaluation of address comparisons
//
// The constexpr interpreter previously sometimes misevaluated address comparisons
// of certain values with distinct types.
//
// That is now fixed.
constexpr bool g(int *x, int const *y) { return x == y; }
static_assert(g(nullptr,nullptr), "");  // Previously failed.  Now okay.
