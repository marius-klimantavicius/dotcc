//type:fp
//options_all:--c++20
//remark:[6.6] Non-type template parameter of typedef type in friend function template
// 7/18/23  [EDGcpfe/26467]
//
// Non-type template parameter of typedef type in friend function template
// constraint
//
// Previously, the front end failed to check the satisfaction of a constraint on a
// friend function template involving a non-type template parameter declared with
// a typedef type.
using int_t = int;
template<typename> struct A {
  template<int_t N> requires (N != 0)
  friend int f(A);
};
int i = f<1>(A<int>());  // Previously a spurious error.  Now okay.
