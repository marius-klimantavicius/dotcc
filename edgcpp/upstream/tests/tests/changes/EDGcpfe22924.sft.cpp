//type:fp
//options_all:--c++20
//remark:[6.1] C++20 comparison rewrites
// 6/9/20   [EDGcpfe/22924]
//
// C++20 comparison rewrites
//
// A number of problems involve C++20 comparison rewrites are now fixed.  That
// includes cases like the following:
//
// A different problem occurred when a nondependent rewritten comparison occurred
// in a template instantiation.
#include <compare> 
struct X {
  auto operator<=>(int x) const { return 42 <=> x; }
} x;
template<typename> auto cmp = 1 <=> x;
auto r = cmp<void>;  // Previously triggered a spurious error.  Now okay.
