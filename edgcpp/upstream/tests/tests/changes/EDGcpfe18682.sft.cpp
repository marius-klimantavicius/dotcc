//type:fp
//options_all:--c++11
//remark:[5.0] Abort in scan_template_argument_constant_expression
// 11/13/17 [EDGcpfe/18682]
//
// Abort in scan_template_argument_constant_expression
//
// The changes for EDGcpfe/17888 (which unified C++11 and C++14 constexpr
// evaluation) introduced a regression in version 4.13 when scanning certain
// nontype template arguments.
//
// Specifically, it triggered an internal error due to an unexpected backing
// expression (in scan_template_argument_constant_expression).  That is now fixed.
template<int N> struct S {
  constexpr S(): a() {}
  int a[N];
};
template <int N> struct X {
  int a[N+1];
};
X<S<1>().a[0]> x;  // Previously triggered an internal error.
