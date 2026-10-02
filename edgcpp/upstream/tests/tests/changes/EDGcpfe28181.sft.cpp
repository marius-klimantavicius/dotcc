//type:fp
//options_all:--c++20
//remark:[6.8] Abort on unnamed function parameter pack
// 5/19/25  [EDGcpfe/28181]
//
// Abort on unnamed function parameter pack
//
// Previously, the expansion of a function parameter pack in a template
// declaration that includes another unnamed function parameter pack could result
// in an abort due to a null pointer indirection.
int g(int);
template<typename ... T>
int f(T ...) requires
  requires (T ... t) { g(t ...); };  // Previously aborted, now okay.
int i = f(1);
