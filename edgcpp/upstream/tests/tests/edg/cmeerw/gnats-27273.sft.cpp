//type:fp
//options:--c++17:--c++20:--c++17 --gn 140100:--c++20 --gn 140100:--ms_c++20 --microsoft_version 1938
//options_all:-w

namespace minimal
{
  namespace ns {
    template<typename> struct D {};
    template<typename T, typename = D<T>> struct C {};
  }
  template<template<typename> typename W, typename T>
  W<T> g();
  auto v = g<ns::C, int>();
}

namespace simple_test
{
  namespace ns
  {
    template<typename>
    struct D
    { };

    template<typename T, typename U = D<T>>
    struct C
    { };
  }

  template<template<typename> typename W, typename T>
  W<T> g();

  void f()
  {
    g<ns::C, int>();
  }
}

namespace template_in_outer_ns
{
  template<typename>
  struct D
  { };

  namespace ns
  {
    template<typename>
    struct D
    { };

    template<typename T, typename U = D<T>>
    struct C
    { };
  }

  template<template<typename> typename W, typename T>
  W<T> g();

  void f()
  {
    g<ns::C, int>();
  }
}

namespace packs
{
  template<template<typename...> class T>
  struct A
  {
    T<int> t;
  };

  template<typename T, typename U = void>
  struct B
  {
    T t = 1;
    U *u = static_cast<void *>(0);
  };

  void f()
  {
    A<B> a;
    a.t.t = 2;
    a.t.u = static_cast<void *>(0);
  }
}
