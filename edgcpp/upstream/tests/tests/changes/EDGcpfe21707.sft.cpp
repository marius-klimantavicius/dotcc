//type:fp
//options_all:--c++17
//remark:[6.2] Spurious SFINAE failure on lookup for member selection (regression)
// 11/4/20  [EDGcpfe/21707,EDGcpfe/22796,EDGcpfe/23311,EDGcpfe/23528]
//
// Spurious SFINAE failure on lookup for member selection (regression)
//
// The changes for EDGcpfe/20899 (introduced in version 5.1) fixed some spurious
// SFINAE failures, but they accidentally introduced new ones in other complex
// situations.
//
// That regression is now fixed.
template<typename...> struct S { void f(S); };
template<typename... Ts>
  void g(S<Ts...> &l, S<Ts...> &r) noexcept(noexcept(l.f(r)));
void (*p)(S<int>&, S<int>&) = g<int>;  // Previously a spurious error.
                                       // Now okay.
