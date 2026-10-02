//type:cp
//options:--c++20:--ms_c++20:--c++20 --gn 120100

namespace minimal
{
  using int_t = int;
  template<typename> struct A {
    template<int_t N> requires (N != 0)
    friend int f(A);
  };
  int i = f<1>(A<int>());
}

namespace no_typedef
{
  template<typename> struct A
  {
    template<int N> requires (N != 0)
    friend int f(A);
  };

  int i = f<1>(A<int>());
}

namespace no_typedef_use_as_arg
{
  template<int I>
  struct B
  {
    static constexpr int value = I;
  };

  template<typename> struct A
  {
    template<int N> requires (B<N>::value != 0)
    friend int f(A);
  };

  int i = f<1>(A<int>());
}

namespace typedef_use_as_arg
{
  using int_t = int;

  template<int I>
  struct B
  {
    static constexpr int value = I;
  };

  template<typename> struct A
  {
    template<int_t N> requires (B<N>::value != 0)
    friend int f(A);
  };

  int i = f<1>(A<int>());
}

namespace type_param
{
  template<typename> struct A
  {
    template<typename T> requires (sizeof(T) == 1)
    friend int f(A);
  };

  int i = f<char>(A<int>());
}

namespace tmpl_param
{
  template<typename T> struct C
  {
    static T s;
  };

  template<typename> struct A
  {
    template<template<typename> class TT> requires (sizeof(TT<char>::s) == 1)
    friend int f(A);
  };

  int i = f<C>(A<int>());
}

namespace trailing_requires_clause
{
  using int_t = int;

  template<typename> struct A
  {
    template<int_t N>
    friend int f(A) requires (N != 0);
  };

  int i = f<1>(A<int>());
}
