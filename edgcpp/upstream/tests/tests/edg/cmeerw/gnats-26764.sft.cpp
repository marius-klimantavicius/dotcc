//type:fp
//options:--c++17:--c++20:--c++20 --gn 130100:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1936
//options_all:-tused

// see P0522R0, P1616R1, and CWG150
// temp.arg.template/3 "A template-argument matches a template
// template-parameter P when P is at least as specialized as the
// template-argument A. In this comparison, if P is unconstrained, the
// constraints on A are not considered."

namespace minimal
{
  template<int> class A;
  template<template<auto> class P> int f();
  int i = f<A>();
}

namespace all_auto_deduced
{
  template<auto I, template<auto> class C>
  int f(C<I> c);

  template<auto I>
  struct X { };

  int i = f(X<1>());
  int j = f<1, X>(X<1>());
}

namespace int_params_deduced
{
  template<int I, template<int> class C>
  int f(C<I> c);

  template<auto I>
  struct X { };

  int i = f(X<1>());
  int j = f<1, X>(X<1>());
}

namespace more_specialized
{
  template<auto>
  struct X { };


  template<template<int> class>
  int f();

  auto x = f<X>();


  template<template<int> class>
  struct D { };

  D<X> d;


  template<template<int> class>
  int v;

  auto y = v<X>;
}

#if __cpp_concepts
namespace constrained_arg
{
  template<typename>
  concept C = true;

  template<C auto>
  struct X { };


  template<template<auto> class>
  int f();

  auto x = f<X>();


  template<template<auto> class>
  struct D { };

  D<X> d;


  template<template<auto> class>
  int v;

  auto y = v<X>;
}

namespace constrained_param
{
  template<typename>
  concept C = true;

  template<auto>
  struct X { };


  template<template<C auto> class>
  int f();

  auto x = f<X>();


  template<template<C auto> class>
  struct D { };

  D<X> d;


  template<template<C auto> class>
  int v;

  auto y = v<X>;
}

namespace more_constrained_param
{
  template<typename>
  concept C = true;

  template<typename T>
  concept C2 = C<T> && true;

  template<C auto>
  struct X { };


  template<template<C2 auto> class>
  int f();

  auto x = f<X>();


  template<template<C2 auto> class>
  struct D { };

  D<X> d;


  template<template<C2 auto> class>
  int v;

  auto y = v<X>;
}
#endif

namespace more_specialized_auto_ptr_arg
{
  template<auto>
  struct X { };


  template<template<auto *> class>
  int f();

  auto x = f<X>();


  template<template<auto *> class>
  struct D { };

  D<X> d;


  template<template<auto *> class>
  int v;

  auto y = v<X>;
}
