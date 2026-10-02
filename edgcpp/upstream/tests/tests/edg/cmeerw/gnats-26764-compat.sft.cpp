//type:fp
//options:--c++20:--c++20 --gn 130100:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936
//options_all:-tused

namespace int_arg_auto_params_deduced
{
  template<auto I, template<auto> class C>
  int f(C<I> c);

  template<int I>
  struct X { };

  int i = f(X<1>());            // accepted by gcc
  int j = f<1, X>(X<1>());      // accepted by gcc
}

namespace not_at_least_as_specialized
{
  template<int>
  struct X { };


  template<template<auto> class>
  int f();

  auto x = f<X>();              // accepted by gcc


  template<template<auto> class>
  struct D { };

  D<X> d;                       // accepted by gcc


  template<template<auto> class>
  int v;

  auto y = v<X>;                // accepted by gcc
}
