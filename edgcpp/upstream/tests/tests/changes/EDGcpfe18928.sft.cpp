//type:fp
//options_all:--c++11
//remark:[5.0] Failure to handle brace-notation cast failure as SFINAE
// 11/14/17 [EDGcpfe/18928]
//
// Failure to handle brace-notation cast failure as SFINAE
//
// In some cases, substituting a brace-notation cast with template arguments that
// make that case invalid produced an immediate error instead of a (SFINAE)
// deduction failure.
//
// Previously this triggered an error while substituting A{declval<Us>()...}.
// That is now fixed: The corresponding test template is discarded and the
// ellipsis version is selected instead.
template <typename T> T&& declval();
struct Y { static constexpr bool value = true; };
struct N { static constexpr bool value = false; };
struct A { A() {} };
struct B {};
template<typename T> struct H {
  template<typename... Us>
    static decltype(static_cast<void>(A{declval<Us>()...}), Y{}) test(int);
  template<typename...>
    static N test(...);
  using type = decltype(test<T>(0));
};
static_assert(!H<B>::type::value, "");
