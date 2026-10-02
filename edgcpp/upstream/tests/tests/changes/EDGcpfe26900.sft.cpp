//type:fp
//options_all:--microsoft_v 1936 --ms_c++17 --no_ms_permissive
//remark:[6.7] Microsoft bugs compatibility: &(X::Y) pointer-to-member formation
// 12/20/23 [EDGcpfe/26900]
//
// Microsoft bugs compatibility: &(X::Y) pointer-to-member formation
//
// The static assertion should fail, because the partial specialization of Cond
// fails substitution since &(T::f) with T=S is not valid (&T::f would create a
// pointer-to-member constant, but the parentheses are not permitted).  MSVC,
// however, does accept the case and the front end now emulates that behavior in
// Microsoft bugs mode.
template<typename> using Void = void;
struct S {
  int& f();
};
template<typename, typename = void> constexpr bool Cond = false;
template<typename T> constexpr bool Cond<T, Void<decltype(&(T::f))>> = true;
static_assert(Cond<S>);
