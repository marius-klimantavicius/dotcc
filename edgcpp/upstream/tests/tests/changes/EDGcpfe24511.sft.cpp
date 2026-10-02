//type:fp
//options_all:--gn 999999
//remark:[6.7] GNU compatibility: Comparison of integer vector types
// 3/14/24  [EDGcpfe/24511,EDGcpfe/25768,EDGcpfe/26155,EDGcpfe/26885,
//           EDGcpfe/26911,EDGcpfe/27099]
//
// GNU compatibility: Comparison of integer vector types
//
// Previously this elicited an error complaining that the return value of the test
// function does not match its return type.  That is now fixed.  (The exact
// behavior of GCC is somewhat surprising, and earlier attempts at emulating it
// relied on incorrect conclusions.  The emulation now produces a result type for
// vector comparisons that is distinct from vector types that can be declared by
// the programmer directly.)
using u32x8 [[gnu::vector_size(32)]] = unsigned int;
u32x8 test(u32x8 x) {
  return (x & 0x7FFFFFFF) == 0;  // Previously an error.  Now okay.
}
