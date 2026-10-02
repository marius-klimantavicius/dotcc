//type:fp
//options_all:--c++11
//remark:[6.5] Inaccessible member function during substitution
// 4/27/23  [EDGcpfe/19798,EDGcpfe/19950,EDGcpfe/20260,EDGcpfe/23304,
//           EDGcpfe/24614,EDGcpfe/25463,EDGcpfe/25624,EDGcpfe/25804,
//           EDGcpfe/26282]
//
// Inaccessible member function during substitution
//
// Previously, when a member with a dependent name resolved to a using declaration
// during substitution, accessibility of the using declaration was not considered.
// This could result in substitution failure when the underlying function was not
// accessible.
struct B {
  int f();
};
template<typename T>
struct D : private T {
  using T::f;
};
template<typename T>
auto g(D<T> d) -> decltype(d.f());
int i = g(D<B>());  // Previously a spurious error.  Now okay.
