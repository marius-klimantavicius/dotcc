//type:fp
//options_all:--c++20
//remark:[6.8] Spurious "atomic constraint depends on itself" error
// 3/18/25  [EDGcpfe/26547,EDGcpfe/27691]
//
// Spurious "atomic constraint depends on itself" error
//
// In some cases, the front end previously issued a spurious "atomic constraint
// depends on itself" error during constraint checking.
// --c++20:
template<typename T> auto f(T t) { t.g(); }
template<typename T> requires
  requires (T t) { f(t); }  // Previously a spurious error.  Now okay.
using A = T;
template<typename> struct C { void g() { A<C> a; } };
A<C<int>> a;
