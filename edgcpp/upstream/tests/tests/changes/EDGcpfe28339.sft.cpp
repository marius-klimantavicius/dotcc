//type:fp
//options_all:-w --clang_v 999999
//remark:Deduction of Clang "ext_vector" types
// 5/27/26  [EDGcpfe/28339,EDGcpfe/28819]
//
// Deduction of Clang "ext_vector" types
//
// Previously, this resulted in a spurious error due to deduction failure.
// There were multiple causes for that failure (including incorrect handling
// of the result of v == v, which is a "boolean vector"): All the causes are
// now fixed and the example is now accepted.
template<typename T, unsigned long N>
  using V __attribute((ext_vector_type(N))) = T;
template<typename T, unsigned long N> int f(V<T, N>);
V<short, 8> v;
int r = f(v == v);  // Previously an error.  Now okay.
