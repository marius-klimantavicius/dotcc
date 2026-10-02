//type:fp
//options:--c++11:--c++17:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100

namespace minimal
{
  template<typename T, T> struct C {};
  template<typename T> using A = C<T, 0>;
  template<template<typename> class W, typename U> W<U> f(U);
  auto v = f<A>(1);
}

namespace no_packs
{
  template<typename T, T> struct C {};
  template<typename T> using A = C<T, 0>;
  template<template<typename> class W, typename U> W<U> f(U);

  auto v0 = f<A>(1);
  auto v1 = f<A, int>(1);
}

namespace decltype_no_packs
{
  template<typename T, T *>
  struct C { };

  template<typename T1, typename T2>
  using A = C< decltype((T1() + T2())), nullptr >;

  template<template<typename, typename> class W, typename U1, typename U2>
  W< U1, U2 > f(U1, U2);

  auto v0 = f<A>(1, 2L);
  auto v1 = f<A, int>(1, 2L);
  auto v2 = f<A, int, long>(1, 2L);
}

#if __cpp_fold_expressions
namespace simple_packs
{
  template<typename T, T>
  struct C { };

  template<typename ... TT>
  using A = C< decltype((TT() + ...)), 0 >;

  template<template<typename ...> class W, typename ... UU>
  W< UU ... > f();

  auto v1 = f<A, int>();
  auto v2 = f<A, int, int>();
  auto v3 = f<A, int, long, int>();
}

namespace additional_templ_param
{
  template<typename T, T>
  struct C { };

  template<typename ... TT>
  using A = C< decltype((TT() + ...)), 0 >;

  template<template<typename ...> class W, typename X, typename ... UU>
  W< UU ... > f();

  auto v1 = f<A, void, int>();
  auto v2 = f<A, void, int, int>();
  auto v3 = f<A, void, int, int, long>();
}

namespace deduce_pack
{
  template<typename T, T>
  struct C { };

  template<typename ... T>
  using A = C<decltype((T() + ...)), 0>;

  template<template<typename ...> class W, typename ... U>
  W<U ...> f(U ...);

  auto v1 = f<A>(1);
  auto v2 = f<A>(1, 2L);

  auto v1_1 = f<A, int>(1);
  auto v2_1 = f<A, int>(1, 2L);
  auto v2_2 = f<A, int, long>(1, 2L);
}
#endif
