//type:fn
//options:--c++14:--c++20:--c++20 --gn 130100:--c++20 --clang_version 160000
//options_all:-tused

namespace incompatible_templ_templ_parameters
{
  template<int>
  struct X { };


  template<typename T, template<T> class C>
  int f();

  auto x1 = f<int, X>();
  auto x2 = f<long, X>();       // error


  template<typename T, template<T> class C>
  struct D { };

  D<int, X> d1;
  D<long, X> d2;                // error


  template<typename T, template<T> class C>
  int v;

  auto y1 = v<int, X>;
  auto y2 = v<long, X>;         // error
}

#ifdef __cpp_nontype_template_parameter_auto
namespace different_auto_args
{
  template<auto &>
  struct X { };


  template<template<auto *> class>
  int f();

  auto x = f<X>();              // error, although commonly accepted, but not emulated


  template<template<auto *> class>
  struct D { };

  D<X> d;                       // error, although commonly accepted, but not emulated


  template<template<auto *> class>
  int v;

  auto y = v<X>;                // error, although commonly accepted, but not emulated
}
#endif
