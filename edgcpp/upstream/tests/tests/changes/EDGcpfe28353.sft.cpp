//type:fp
//options_all:--c++20 --gn 120300
//remark:[6.8] Abort on overloaded default constructor
// 7/22/25  [EDGcpfe/28353]
//
// Abort on overloaded default constructor
//
// Depending on the configuration, either or both of the variable declarations
// could trigger assertion failures.  That is now fixed.
template <typename... Ts> struct S {
  constexpr S() = default;
  constexpr explicit S(Ts...) noexcept requires (sizeof...(Ts) != 0) {}
};
constexpr S s1;
constexpr S s2{};
