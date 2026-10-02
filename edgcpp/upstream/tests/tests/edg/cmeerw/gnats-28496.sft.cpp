//type:fp
//options:--c++17:--c++17 --gn 150200:--c++17 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename T>
  auto f(T t) -> decltype(t(0));
  template<int>
  struct C {
    template<typename>
    static constexpr auto l = [] (auto) { return f([] (auto) { return 0; }); };
  };
  int i = C<1>::l<int>(0);
}

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace separate_lambdas
{
  template<typename T>
  auto f(T t) -> decltype(t(0));

  template<int>
  struct C {
    template<typename>
    static constexpr auto l1 = [] (auto) { return 0; };

    template<typename T>
    static constexpr auto l2 = [] (auto) { return f(l1<T>); };
  };
  int i = C<1>::l2<int>(0);
}

namespace non_generic_lambdas
{
  template<typename T>
  auto f(T t) -> decltype(t(0));

  template<int>
  struct C {
    template<typename>
    static constexpr auto l1 = [] (int) { return 0; };

    template<typename T>
    static constexpr auto l2 = [] (int) { return f(l1<T>); };
  };
  int i = C<1>::l2<int>(0);
}

namespace member_variable_template
{
  template<typename T>
  auto f(T t) -> decltype(t(0));

  template<int N>
  struct value {
    static constexpr int n = N;
  };

  template<int N>
  struct C {
    template<typename>
    static constexpr auto l = [] (auto) {
      return f([] (auto) { return value<N>{}; });
    };
  };

  static_assert(is_same_v<decltype(C<2>::l<int>(0)), value<2>>);
  static_assert(is_same_v<decltype(C<1>::l<int>(0)), value<1>>);
  static_assert(is_same_v<decltype(C<2>::l<char>(0)), value<2>>);
}

namespace namespace_variable_template
{
  template<typename T>
  auto f(T t) -> decltype(t(0));

  template<int N>
  struct value {
    static constexpr int n = N;
  };

  template<int, typename>
  constexpr auto l = [] (auto) {
    return f([] (auto) { return value<0>{}; });
  };

  static_assert(is_same_v<decltype(l<2, value<2>>(0)), value<0>>);
  static_assert(is_same_v<decltype(l<1, value<1>>(0)), value<0>>);
  static_assert(is_same_v<decltype(l<2, value<2> *>(0)), value<0>>);
}
