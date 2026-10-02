//type:fn
//options:--c++20
//options_all:--display_error_number

namespace std_err_example
{
  template<typename ... Ts>
  struct Y {
    Y();
    Y(Ts ...);
  };

  template<template<typename T = char> class X>
  void f() {
    X x2{1, 2};               // error: cannot deduce X<T> from Y<int, int>
  }

  template void f<Y>();
}

namespace cwg3003
{
  template <typename T> struct A { A(T); };

  template <typename T, template <typename> class TT = A>
  using Alias = TT<T>;

  template <typename T>
  using Alias2 = Alias<T>;

  void h() { Alias2 a(42); }    // error: cannot deduce
  void h2() { Alias a(42); }    // error: cannot deduce
}

namespace constrained_guide
{
  template<typename T>
  struct C
  {
    C(T) requires (sizeof(T) == 0);
  };

  template<typename T>
  struct D
  {
    D();
  };

  template<typename T> requires (sizeof(T) == 0)
  D(T) -> D<T>;

  D d(1);                       // error: cannot deduce

  template<template<typename> class TT>
  void f()
  {
    TT t(1);                    // error: cannot deduce
  }

  template void f<C>();
  template void f<D>();
}

namespace names_in_diagnostics
{
  template<typename>
  struct C
  {
    C(int);
  };

  explicit C(int) -> C<int>;

  template<template<typename> class TT>
  struct B
  {
    static inline TT t1 = {1};
  };

  template<typename T>
  using A = C<T>;

  B<C> b;                       // error: cannot use "explicit" deduction guide
  A a = { 3 };                  // error: cannot use "explicit" deduction guide
  C c = { 2 };                  // error: cannot use "explicit" deduction guide
}
