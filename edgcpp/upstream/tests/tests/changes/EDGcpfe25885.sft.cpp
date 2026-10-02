//type:fp
//options_all:--c++20 --microsoft_v 1930
//remark:[6.8] Deduction of an auto template parameter from an array bound
// 7/7/25   [EDGcpfe/25885,EDGcpfe/27661,EDGcpfe/28296]
//
// Deduction of an auto template parameter from an array bound
//
// In some cases, an auto template parameter was not correctly deduced from an
// array bound in a reference parameter.
//
// Here, deduction of N from the length of "x" previously failed.  That is now
// fixed.
template<auto N> struct S {
  static constexpr auto size = N;
  constexpr S(const char* str) {}
};
template<auto N> S(const char(&)[N]) -> S<N>;
template<S> int g();  // Class template argument deduction.
int r = g<"x">();
