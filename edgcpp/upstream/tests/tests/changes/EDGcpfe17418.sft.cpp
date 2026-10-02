//type:fp
//options_all:--c++11
//remark:[4.12] Abort with constexpr function template as nontype template argument
// 8/4/16   [EDGcpfe/17418]
//
// Abort with constexpr function template as nontype template argument
//
// In C++11 modes, the front end could abort with a segfault (in
// copy_template_param_expr_as_rvalue) when an invocation of a constexpr
// function function template appears as a nontype template argument and the
// return value of the function involves a template parameter of the constexpr
// function template.  This is now fixed.
template <int N> struct S { };
constexpr int g(int N) {
  return N + 1;
}
template <int N> S<g(N)> f() {   // S<g(N)> previously caused a segfault
  return {};
}
auto x = f<1>();
