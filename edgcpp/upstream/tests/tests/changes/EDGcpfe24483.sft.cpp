//type:fp
//options_all:--c++20
//remark:[6.4] User-defined string literal operator and class template argument deduction
// 10/5/22  [EDGcpfe/24483,EDGcpfe/24620]
//
// User-defined string literal operator and class template argument deduction
//
// The addition in C++20 of nontype template parameters of class type (see the
// changes for EDGcpfe/20033 etc.) also introduced a new user-defined string
// literal operator template form.  The front end's initial implementation,
// however, failed to handle class template argument deduction for that form.
//
// Previously, this example elicited a few errors, including a diagnostic about
// the template parameter list of operator""_fstr.  Now this example is accepted.
using S = decltype(sizeof(42));
template<S N> struct FixedStr {
  char str[N] = {};
  constexpr FixedStr(char const (&arr)[N]) {
    for (size_t i = 0; i < N; ++i) str[i] = arr[i];
  }
};
template<FixedStr str> constexpr auto operator""_fstr() {
  return str;
}
constexpr auto s = "abc"_fstr;
