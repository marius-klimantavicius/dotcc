//type:fp
//options_all:--c++20
//remark:[6.8] Matching of partial specialization with a placeholder for a deduced class type
// 7/24/25  [EDGcpfe/27654,EDGcpfe/28350]
//
// Matching of partial specialization with a placeholder for a deduced class type
//
// Previously, the front end failed to match a partial specialization declared
// with a placeholder for a deduced class type.
template<int>
struct S {
  char c;
};
template<typename T, S s> struct N;
template<typename T, S s> struct N<T *, s> { };
N<int *, S<0>{'a'}> n;  // Previously a spurious error.  Now okay.
