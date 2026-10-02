//type:fp
//options:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace reduced_example
{
  template<template <class...> class _Fn, typename... _Args>
  struct Eval
  {
    using type = _Fn<_Args...>;
  };

  template<template <class...> class _Fn, typename... _Args>
  using eval_t = typename Eval<_Fn, _Args...>::type;

  template<typename T, typename>
  using F = T;

  template<typename, typename>
  concept X = true;

  template<typename>
  struct S
  {
    struct C
    {
      template<typename T, X<decltype(eval_t<F, int, T>())> U>
      void connect(T, U);
    };
  };

  void f(S<int>::C s)
  {
    s.connect(1, 2);
  }
}

namespace further_reduced
{
  template<typename T1, typename T2>
  struct B
  {
    using type = T1;
  };

  template<typename ... Ts>
  using A = typename B<Ts ...>::type;

  template<typename, typename>
  concept X = true;

  template<typename>
  struct S
  {
    struct C
    {
      template<typename T, X<A<T, T>> U>
      void f(T, U);
    };
  };

  void f(S<int>::C c)
  {
    c.f(1, 2);
  }
}

namespace non_pack
{
  template<typename T1, typename T2>
  struct B
  {
    using type = T1;
  };

  template<typename, typename>
  concept X = true;

  template<typename V>
  struct S
  {
    struct C
    {
      template<typename T, X<typename B<T, T>::type> U>
      void f(T, U);

      template<typename T, X<typename B<V, V>::type> U>
      void g(T, U);
    };
  };

  void f(S<int>::C c)
  {
    c.f(1, 2);
    c.g(1, 2);
  }
}
