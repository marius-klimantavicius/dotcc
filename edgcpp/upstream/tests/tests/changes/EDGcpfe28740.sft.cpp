//type:fp
//options_all:--c++11
// 8/26/26  [EDGcpfe/28740]
//
// Substituting explicitly-specified template arguments with trailing empty pack
//
// Previously, a trailing empty pack in an explicitly-specified template argument
// list could result in a spurious substitution failure.
// --c++11:
struct C {
  template<typename> static int f();
};
template<typename T, typename ... Us>
auto f(T, Us ...) -> decltype(C::f<T, Us ...>());
int i = f(1);
