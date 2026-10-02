//type:fp
//options_all:--c++20
//remark:[6.6] Expansion of constrained template type parameters
// 11/21/23 [EDGcpfe/26742]
//
// Expansion of constrained template type parameters
//
// Previously, the front end never considered a type-constraint to be expandable,
// even if it included a template argument list containing an unexpanded template
// parameter pack.
template<typename T, typename U>
concept C = sizeof(T) == sizeof(U);
template<typename ... T>
struct B {
  template<C<T> ... U>
  static int f(U ...);
};
auto v = B<char, int>::f('a', 2);  // Previously a spurious error.  Now okay.
