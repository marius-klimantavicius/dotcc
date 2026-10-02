//type:fp
//options:--c++14:--c++20:--ms_c++20:--c++20 --g++
//options_all:-tused

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

namespace sfinae
{
  template<class T, class U> struct A
  { };

  template<class T, class U> using AA = typename A<T, U>::type;

  template<class T> struct B
  {
    template<class U> using A = typename A<T, U>::type;
  };

  template <class T, class U> typename A<T, U>::type f(T, U, short);
  template <class T, class U> typename B<T>::template A<U> f(T, U, int);
  template <class T, class U> AA<T, U> f(T, U, char);
  template <class T, class U> void f(T, U, long);

  static_assert(is_same_v<decltype(f('a', 2L, 3)), void>, "passes");
}

namespace sfinae_no_failure
{
  template<class T, class U> struct A
  {
    using type = U;
  };

  template<class T, class U> using AA = typename A<T, U>::type;

  template<class T> struct B
  {
    template<class U> using A = typename A<T, U>::type;
  };

  template <class T, class U> typename A<T, U>::type f(T, U, short);
  template <class T, class U> typename B<T>::template A<U> f(T, U, int);
  template <class T, class U> AA<T, U> f(T, U, char);
  template <class T, class U> void f(T, U, long);

  static_assert(is_same_v<decltype(f('a', short(2), short(3))), short>, "passes");
  static_assert(is_same_v<decltype(f('a', 2, 3)), int>, "passes");
  static_assert(is_same_v<decltype(f('a', 'b', 'c')), char>, "passes");
  static_assert(is_same_v<decltype(f('a', 2L, 3L)), void>, "passes");
}

namespace deduced_pack
{
  template<class ... t> struct A {};

  template<class t>
  struct P
  {
    template<class ...vt> using A = A<vt *...>;
  };

  template<class t, class ... vt>
  using AA = A<vt *...>;

  template<class p1, class ...vt1>
  void g(typename P<p1>::template A<vt1 ...> v1)
  { }

  void foo()
  {
    g<int, int>(A<int *>{});
  }
}

namespace minimal
{
  template<class T> struct B {
    template<class U>
    using C = typename U::type;
  };
  template<class T, class U> typename B<T>::template C<U> f(T, U, int);
  template<class T, class U> int f(T, U, long);
  int i = f(1, 2, 3);
}

namespace PR
{
  template <template <class> class Fn1>
  struct q {
    template <class T1>
    using f = Fn1<T1>;
  };

  template <class T2>
  T2 _decay(const T2&);

  template <class T3>
  using decay_t = decltype(PR::_decay(T3{}));

  template <class T4>
  typename T4::template f<int> test(float);

  int t = test<q<decay_t>>(0.0f);
}

namespace friend_names
{
  template <typename T>
  struct S
  {
    S() {}

    friend inline void foo(S &s) { }

    template<typename U>
    friend inline void tmpl_foo(S &s, U u) { }
  };

  template<typename T>
  void bar()
  {
    S<int> s;

    foo(s);

    tmpl_foo(s, 1);
  }

  void baz()
  {
    bar<void>();
  }
}

namespace alias
{
  template<template <class> class Fn1>
  struct C
  {
    template<class T1>
    using f = Fn1<T1>;
  };

  template<class T2>
  T2 g(const T2&);

  template<class T3>
  using A = decltype((g)(T3{}));

  template<class T4>
  typename T4::template f<int> h();

  void foo()
  {
    h<C<A>>();
  }
}

namespace templ_func
{
  template<template <class> class Fn1>
  struct C
  {
    template <class T1>
    static Fn1<T1> f();
  };

  template <class T2>
  T2 g(const T2&);

  template <class T3>
  using A = decltype((g)(T3{}));

  template <class T4>
  decltype(T4::template f<int>()) h();

  void foo()
  {
    h<C<A>>();
  }
}
