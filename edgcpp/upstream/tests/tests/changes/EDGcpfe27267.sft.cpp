//type:fp
//options_all:--c++11
//remark:[6.7] Substitution of nested pack expansions
// 6/21/24  [EDGcpfe/27267]
//
// Substitution of nested pack expansions
//
// Previously, the front end failed to substitute into nested pack expansions
// during template argument deduction.
template<typename ...> struct C {};
template<typename, typename> struct E {};
template<typename T, typename ... Us> using A = C<E<T, Us> ...>;
template<typename ... Ts, typename ... Us, typename = C<A<Ts, Us ...> ...>>
int f(C<Ts ...>, Us ...);
int i = f(C<int>{}, 0);  // Previously a spurious error.  Now okay.
