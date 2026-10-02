//type:fn
//options:--c++11:--c++11 --g++:--c++11 --microsoft;fp
//options_all:-tused

namespace lookup
{
  template <typename T>
  struct S;

  void foo(int)
  { }

  template<typename T>
  void bar(S<int> &s)
  {
    foo(s);
  }

  void foo(S<int> &s)
  { }

  template <typename T>
  struct S
  {
    S() {}
    friend void foo(S &s);
  };

  void baz(S<int> &s)
  {
    bar<void>(s);
  }
}

namespace friend_templ_func
{
  template<template <class> class Fn1>
  struct C
  {
    template<class T1>
    friend Fn1<T1> f(C, T1);
  };

  template<class T2>
  T2 g(const T2&);

  template<class T3>
  using A = decltype((g)(T3{}));

  template<class T4>
  decltype(f(T4{}, 1)) h();

  void foo()
  {
    h<C<A>>();                  // known issue in g++ mode
  }
}
