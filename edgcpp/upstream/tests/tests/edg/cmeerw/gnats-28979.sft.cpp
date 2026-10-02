//type:fp
//options:--c++14:--c++20:--c++20 --gn 160100:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  using uint = unsigned;
  template<uint, int = 0> void f();
  auto l = [](auto) {
    constexpr uint U = 1 + 1;
    return [&](auto v) {
      f<U, sizeof v>();
      f<U>();
    };
  };
}

#if __cpp_generic_lambdas >= 201707
namespace minimal_cpp20
{
  using uint = unsigned;
  template<uint, int> void f1();
  template<uint> void f2();
  auto f(auto) {
    constexpr uint U = 1 + 1;
    return [&]<int I>() {
      f1<U, I>();
      f2<U>();
    };
  }
}

namespace slightly_longer
{
  using S = unsigned long;

  template<S, bool>
  void f1() {}

  template<S>
  void f2() {}

  template <typename F>
  auto g(F f) {
    return f;
  }

  void f() {
    [&]<typename V = int>() {
      constexpr S B = 1 + 1;
      auto f = [&]<bool X>() {
        g(f1<B, X>);
        g(f2<B>);
      };
      f.template operator()<true>();
    }();
  }
}
#endif
