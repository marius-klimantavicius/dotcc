//type:fp
//options:--c++17:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename T, T>
  struct C { };
  template<typename T, template<typename, T> class TT>
  struct D { };
  template<typename T, template<typename, T> class TT1>
  void f(D<T, TT1>) { }
  void g() { f(D<int, C>{}); }
}

namespace minimal_warning
{
  template<typename T, T>
  struct C { };
  template<template<typename T, T> class TT, typename T>
  struct D { };
  D<C, int> d;
}

namespace deduction
{
  template<typename T, T val>
  struct S {};

  template<template<typename T, T> class X, typename T>
  struct W {};

  template<template<typename T, T> class X, typename T>
  bool f(const W<X, T>&) { return true; }

  bool b = f(W<S, int>{});
}

namespace implicit_deduction_guide
{
  template<typename T, T> struct S { };
  template<typename T, template<typename, T> class TT> struct W { };

  template<auto...> struct X {};
  template<template<typename T, T> class> struct Y {};

  template<typename A> struct C {
    template<typename U, U V, template<typename T, T> class Q>
    C(A, U, X<V>, Y<Q>);
  };

  C c(1, 2, X<0>{}, Y<S>{});
}

namespace deduced_single_param
{
  template<typename T, T> struct S { };
  template<typename T, template<typename, T> class TT> struct W { };

  template<typename T, template<typename, T> typename TT>
  bool f(const W<T, TT>&) { return true; }

  bool b = f(W<int, S>{});
}

namespace deduced_two_params
{
  template<typename T, T> struct S { };
  template<typename T, template<typename, T> class TT> struct W { };

  template<typename T, template<typename, T> typename TT1,
      template<typename, T> typename TT2>
  bool f(const W<T, TT1> &, const W<T, TT2> &) { return true; }

  bool b = f(W<int, S>{}, W<int, S>{});
}

namespace explicit_args
{
  template<typename T, T> struct S { };
  template<typename T, template<typename, T> class TT> struct W { };

  template<typename T, template<typename, T> typename TT>
  bool f(const W<T, TT>&) { return true; }

  bool b = f<int, S>(W<int, S>{});
}

namespace deduced_long {
  template<typename T, T> struct S { };
  template<typename T, template<typename, T> class TT> struct W { };

  template<typename T, template<typename, T> typename TT>
  bool f(const W<T, TT>&) { return true; }

  bool b = f(W<long, S>{});
}

namespace default_arg
{
  template<typename T, T> struct S { };
  template<typename T, template<typename, T> class TT> struct W { };

  template<typename T, template<typename, T> typename TT = S>
  bool f(const W<T, TT>&) { return true; }

  bool b = f<int>(W<int, S>{});
}

namespace nontype_param_first
{
  template<typename T, T> struct S { };
  template<typename T, T val, template<typename, T> class TT>
  struct W {};

  template<typename T, T val, template<typename, T> typename TT>
  bool f(const W<T, val, TT>&) { return true; }

  bool b = f(W<int, 0, S>{});
}
