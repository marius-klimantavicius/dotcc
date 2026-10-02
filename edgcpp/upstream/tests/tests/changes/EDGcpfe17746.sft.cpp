//type:fp
//remark:[4.13] User-defined literal value errors
// 11/16/16  [EDGcpfe/17746]
//
// User-defined literal value errors
//
// The front end contained a bug causing the literal operator templates to be
// passed incorrect template arguments in some situations.
//
// This test case should be accepted, but was previously handled as if 2_F were
// replaced by 0_F, causing the final assertion to fail.  This is now fixed.
template<typename, typename> struct Eq {
  static constexpr bool value = false;
};
template<typename T> struct Eq<T, T> {
  static constexpr bool value = true;
};
template<int N> struct F {};
template<char D> struct D2I {
  static constexpr int value = D-'0';
};
template<char... Ds> F<D2I<Ds...>::value> operator "" _F();
template <typename T> void f(T);
struct C {
  static void unused() { f<F<0>>(0_F); }
};
static_assert(Eq<F<2>, decltype(2_F) >::value, "Unexpected!");
