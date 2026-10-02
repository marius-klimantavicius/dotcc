//type:fp
//options:--c++17 -A:--c++20 -A:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++17 --microsoft_version 1936:--ms_c++20 --microsoft_version 1950

// Note: Other compilers don't actually support the full semantics yet, but we
// still want to test that the feature works in their modes.

namespace minimal
{
  template<typename T = char>
  struct C {
    C(const char *);
  };
  template<template<typename T = int> class TT>
  void f() {
    TT t = "";  // Previously deduced as C<char>, now deduced as C<int>
  }
  template void f<C>();
}

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace std_example
{
  template<typename ... Ts>
  struct Y {
    Y();
    Y(Ts ...);
  };

  template<template<typename T = char> class X>
  void f() {
    X x;                        // OK, deduces Y<char>
    X x0{};                     // OK, deduces Y<char>
    X x1{1};                    // OK, deduces Y<int>
    //X x2{1, 2};               // error: cannot deduce X<T> from Y<int, int>

    static_assert(is_same_v<decltype(x), Y<char>>);
    static_assert(is_same_v<decltype(x0), Y<char>>);
    static_assert(is_same_v<decltype(x1), Y<int>>);
  }

  template void f<Y>();
}

namespace basic_example
{
  template<typename T>
  struct C
  {
    C(int, int);

    C(T);
  };

  template<typename T>
  C(T, int) -> C<T>;

  template<template<typename> class TT>
  void f()
  {
    TT tt1(1);
    static_assert(is_same_v<decltype(tt1), C<int>>);

    TT tt1c = 2;
    static_assert(is_same_v<decltype(tt1c), C<int>>);

    TT tt2(1, 2);
    static_assert(is_same_v<decltype(tt2), C<int>>);
  }

  template void f<C>();
}

#if __cpp_deduction_guides >= 201907L
namespace aggregate_ctad_arg
{
  template<typename T>
  struct C
  {
    T t;
  };

  template<template<typename> class TT>
  void f()
  {
    TT tt1{1};
    static_assert(is_same_v<decltype(tt1), C<int>>);

    TT tt1p(1);
    static_assert(is_same_v<decltype(tt1p), C<int>>);
  }

  template void f<C>();


  template<template<typename> class TT>
  struct D
  {
    template<typename T>
    using A = TT<T>;

    constexpr int f()
    {
      A a{ 1 };
      static_assert(is_same_v<decltype(a), C<int>>);

      return a.t;
    }
  };

  static_assert(D<C>().f() == 1);
}
#endif

#if __cpp_deduction_guides >= 201907L
namespace alias_ctad_arg
{
  template<typename T>
  struct C
  {
    C(int, int);

    C(T);
  };

  template<typename T>
  C(T, int) -> C<T>;

  template<typename T>
  using A = C<T>;

  template<template<typename> class TT>
  void f()
  {
    TT tt1(1);
    static_assert(is_same_v<decltype(tt1), C<int>>);

    TT tt1c = 2;
    static_assert(is_same_v<decltype(tt1c), C<int>>);

    TT tt2(1, 2);
    static_assert(is_same_v<decltype(tt2), C<int>>);
  }

  template void f<A>();
}
#endif

namespace template_param_lookup
{
  namespace ns
  {
    template<typename>
    struct is_iterator {
      static constexpr bool value = true;
    };

    template<bool, typename>
    struct enable_if
    { };

    template<typename T>
    struct enable_if<true, T> {
      using type = T;
    };

    template<bool B, typename T>
    using enable_if_t = typename enable_if<B, T>::type;

    template<typename T>
    struct C
    {
      C(T*, T*) {}
    };

    template<class Iter, enable_if_t<is_iterator<Iter>::value, int> = 0>
    C(Iter, Iter) -> C<const char>;
  }

  template<template <class...> class TT>
  void f()
  {
    TT t("", "");
  }

  template void f<ns::C>();
}
