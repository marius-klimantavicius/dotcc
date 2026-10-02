//type:fp
//options_all:--c++14 --gnu_version=70500
//remark:[6.5] GNU C++ compatibility: Closure types and literal types
// 1/9/23   [EDGcpfe/22395]
//
// GNU C++ compatibility: Closure types and literal types
//
// C++17 introduced "constexpr lambdas", and along with that feature closure types
// that are literal types.  However, GCC treated closure types as literal types
// by default until GCC 8.x.
//
// The front end now emulates that behavior.
constexpr auto lm = [](int i) { return i; };
  // Now accepted in GNU C++14 mode when gnu_version < 80000.
