//type:fp
//options:--c++11:--c++20:--c++11 --gn 60400:--c++11 --gn 70100:--c++20 --gn 130100:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename>
  struct A { };

  template<template<typename, typename...> class C, typename T, typename ... U>
  int f(C<T, U...>);

  int i = f(A<int>());
}

namespace pr_13464
{
  struct B { };

  template<class, class ...>
  struct B2 : B { };

  template<class>
  struct B1 : B { };

  template<template<class, class ...> class ... T, class U, class ... V>
  static constexpr int f(T<U, V...> ...)
  {
    return 0;
  }

  static constexpr int f(B, B)
  {
    return 1;
  }

  static_assert(f(B1<int>(), B2<int, int>()) == 1, "");
}

namespace pr_17335
{
  template<class...> using A = void;
  template<class> struct B { };

  template<template <class...> class T> struct C
  {
    template<class, class = A<>>
    struct CC { };

    template<template<class...> class U, class... Ts>
    struct CC<U<Ts...>, A<T<Ts..., float> > > { };
  };

  C<B>::CC<B<int>> cc;          // gcc 6.4 complains about incomplete type here (not emulated)
}

namespace pr_16668_ordering
{
  template<typename> struct D
  { };

  template<template<typename> class C, typename T>
  constexpr int f(C<T>)
  { return 1; }

  template<template<typename ...> class C, typename T, typename ... U>
  constexpr int f(C<T, U ...>)
  { return 0; }

  static_assert(f<D, int>(D<int>{}) == 1, "");
  static_assert(f(D<int>{}) == 1, ""); // ambiguous with clang (not emulated)
}

namespace pr_16668_matching
{
  struct B { };

  template<typename> struct D : B
  { };

  template<template<typename ...> class C, typename T, typename ... U>
  constexpr int f(C<T, U ...>)
  { return 1; }

  template<template<typename> class = D, typename = int>
  constexpr int f(B)
  { return 0; }

  constexpr int result_with_initial_subst =
#if defined(_MSC_VER) || defined(__clang__) || (defined(__GNUC__) && __GNUC__ < 7)
    0;
#else
    1;
#endif

  static_assert(f(D<int>{}) == 1, "");

  static_assert(f<D, int>(D<int>{}) == result_with_initial_subst, "");
}
