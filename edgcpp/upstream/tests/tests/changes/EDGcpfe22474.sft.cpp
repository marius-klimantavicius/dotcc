//type:fp
//options_all:--c++17
//remark:[6.1] Spurious error in substitution failure of default nontype template argument
// 4/17/20  [EDGcpfe/22474,EDGcpfe/22623]
//
// Spurious error in substitution failure of default nontype template argument
//
// In some cases, a nondependent nontype template argument for a dependent
// nontype parameter could erroneously cause a template substitution failure.
//
// This is now fixed.
template<int> struct A {
  template<typename T> using Type = T;
};
template<int N, typename A<N>::template Type<int> = 0> void f() {}
void g() {
  f<0>();  // Previously failed because the substitution of the default
}          // argument "0" for the second template parameter of f failed.
