//type:fp
//options_all:--gnu=40802 --c++11
//remark:[4.10.1] C++-generating back end: default value of
// 4/21/15  [EDGcpfe/16133]
//
// C++-generating back end: default value of
// KEEP_TEMPLATE_ARG_EXPR_THAT_CAUSES_INSTANTIATION
//
// In configurations with PROTOTYPE_INSTANTIATIONS_IN_IL set to TRUE and
// KEEP_TEMPLATE_ARG_EXPR_THAT_CAUSES_INSTANTIATION set to FALSE, in some
// cases the C++-generating back end could generate incorrect code when a
// non-type template argument expression is folded.  To avoid this problem,
// the default setting of KEEP_TEMPLATE_ARG_EXPR_THAT_CAUSES_INSTANTIATION has
// been changed to TRUE when PROTOTYPE_INSTANTIATIONS_IN_IL is TRUE, allowing
// the original form of the argument to be preserved.
// --gnu_version=40802 --c++11:
template <typename T, unsigned N> struct A {
  T m[N];
  constexpr const T &operator[](unsigned i) const {
    return m[i];
  }
};

template <typename T, class U, class M>
struct B;

template <typename T, T... Vs,
          template <T...> class U,
          unsigned... Is,
          template <unsigned...> class M>
struct B<T, U<Vs...>, M<Is...>> {
  static constexpr A<T, sizeof...(Vs)> mVs = {Vs...};
  using type = U<mVs[Is]...>;
  // Generated as U<{Vs}[Is]...> with
  // KEEP_TEMPLATE_ARG_EXPR_THAT_CAUSES_INSTANTIATION set to FALSE
};
