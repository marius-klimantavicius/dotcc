//type:fn
//options_all:--c++20
//remark:[6.1] Spurious 'expected a ")"' error during template instantiation
// 1/7/20   [EDGcpfe/22179]
//
// Spurious 'expected a ")"' error during template instantiation
//
// In certain instances the front end would issue a spurious 'expected a ")"'
// error during template instantiation where C++20 parenthesized aggregate
// initialization was used.
//
// This is now fixed.
template <typename T>
int func() noexcept(T(0));
template <typename T>
struct A {
  using type = decltype(func<T>()); // The 6.1 change removed a spurious
};                                  // 'expected a ")"' error.  Later versions
                                    // diagnose too many initializer values.
A<A<int>>::type foo;
