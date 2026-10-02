//type:fp
//options:--c++20

namespace non_dependent
{
  template<typename T>
  struct C
  { };

  static_assert( __edg_is_deducible(C, C<int>));
  static_assert( __edg_is_deducible(C, C<int *>));
  static_assert(!__edg_is_deducible(C, int));

  template<typename T>
  using A = C<T *>;

  template<typename T>
  using P = T *;

  static_assert( __edg_is_deducible(A, C<int *>));
  static_assert(!__edg_is_deducible(A, C<int>));
  static_assert(!__edg_is_deducible(A, int));

  static_assert( __edg_is_deducible(P, int *));
  static_assert(!__edg_is_deducible(P, int));
}

namespace use_default_arg
{
  template<int I, int J>
  struct C
  { };

  template<int I, int J = I + 1>
  using A = C<I, +J>;

  static_assert(!__edg_is_deducible(A, C<1, 1>));
  static_assert( __edg_is_deducible(A, C<1, 2>));
}

namespace empty_pack
{
  template<class ... TT>
  using A = int;

  static_assert( __edg_is_deducible(A, int));
}

namespace dependent
{
  template<typename T>
  struct C
  { };

  template<template<typename> class X, typename T>
  constexpr bool is_deducible_v = __edg_is_deducible(X, T);

  static_assert( is_deducible_v<C, C<int>>);
  static_assert(!is_deducible_v<C, int>);

  struct Outer
  {
    template<typename T>
    struct C
    { };
  };

  template<template<typename> class X, typename T>
  struct Y
  {
    char arr1[ __edg_is_deducible(X, C<int>)];
    char arr2[!__edg_is_deducible(X, int)];
    char arr3[!__edg_is_deducible(T::template C, C<int>)];
    char arr4[!__edg_is_deducible(T::template C, int)];
  };

  template struct Y<C, Outer>;
}

namespace use_concept
{
  template<typename T>
  struct C
  { };

  struct B
  {
    template<typename T>
    struct C
    { };
  };

  template<template<typename> class X, typename T>
  concept is_deducible = __edg_is_deducible(X, T);

  template<template<typename> class X, typename Y, typename T>
  struct Z
  {
    static constexpr int foo(...)
    { return 0; }

    static constexpr int foo(int) requires is_deducible<X, T>
    { return 1; }

    static constexpr int foo(int) requires is_deducible<Y::template C, T>
    { return 2; }
  };

  static_assert(Z<C, B, C<int>>::foo(0) == 1);
  static_assert(Z<C, B, B::C<int>>::foo(0) == 2);
  static_assert(Z<C, void, int>::foo(0) == 0);
}

namespace use_concept_nested_template_name
{
  template<typename T>
  struct C
  { };

  struct A
  { };

  struct B
  {
    template<typename T>
    struct C
    { };
  };

  template<typename S, typename T>
  concept is_C_deducible = __edg_is_deducible(S::template C, T);

  template<typename Y, typename T>
  struct Z
  {
    static constexpr int foo(...)
    { return 0; }

    static constexpr int foo(int) requires is_C_deducible<Y, T>
    { return 1; }
  };

  static_assert(Z<B, C<int>>::foo(0) == 0);
  static_assert(Z<B, B::C<int>>::foo(0) == 1);
  static_assert(Z<A, C<int>>::foo(0) == 0);
  static_assert(Z<A, B::C<int>>::foo(0) == 0);
  static_assert(Z<void, int>::foo(0) == 0);
}

namespace compare_expressions
{
  template<typename T, typename U>
  struct C
  {
    void foo(decltype(__edg_is_deducible(T::template X, U)));
    void foo(decltype(__edg_is_deducible(T::template Y, U)));
  };
}
