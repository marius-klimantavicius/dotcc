//type:fp
//options:--c++17:--c++20:--ms_c++20
//options_all:-tused

namespace minimal
{
  template<int> struct A { };
  template<typename T, T I, template<int> class TT>
  int f(TT<I>);
  int i = f(A<1>{});
}

template<typename, typename>
struct is_same
{
  static constexpr bool value = false;
};

template<typename T>
struct is_same<T, T>
{
  static constexpr bool value = true;
};

namespace deduced_type
{
  template<long>
  struct A { };

  template<typename T, template<T> class TT, T I>
  int f(TT<I>)
  {
    static_assert(is_same<T, long>::value);
    return 0;
  }

  int i = f(A<1>{});
}

namespace duplicate_deduction
{
  template<int>
  struct A { };

  template<int>
  struct B { };

  template<long>
  struct C { };

  template<typename T, T I, template<T> class T1, template<T> class T2>
  int f(T1<I>, T2<I>);

  void *f(...);

  int i = f(A<1>{}, B<1>{});
  void *p1 = f(A<1>{}, B<2>{});
  void *p2 = f(A<1>{}, C<1>{});
}

namespace using_int_in_tmpl_param
{
  template<int>
  struct A { };

  template<typename T, T I, template<int> class TT>
  int f(TT<I>);

  int i = f(A<1>{});
}

namespace using_template_type_in_tmpl_param
{
  template<int>
  struct A { };

  template<typename T, T I, template<T> class TT>
  int f(TT<I>);

  int i = f(A<1>{});
}

namespace using_auto_in_param
{
  template<int>
  struct A { };

  template<typename T, auto I, template<T> class TT>
  int f(TT<I>);

  void *f(...);

  void *p = f(A<1>{});
}

namespace deducible_non_type_in_template
{
  template<typename T, T J>
  struct A
  { };

  template<template<typename T, T> class TT>
  int f();

  int v = f<A>();
}

namespace deducible_non_type_pack_in_template
{
  template<typename T, T J>
  struct A
  { };

  template<template<typename T, T ...> class TT>
  int f();

  int v = f<A>();
}

namespace deducible_non_type_trailing_pack_in_template
{
  template<typename T, T J>
  struct A
  { };

  template<template<typename T, T, T ...> class TT>
  int f();

  int v = f<A>();
}

namespace deducible_non_type_auto_in_template
{
  template<typename T, auto J>
  struct A
  { };

  template<template<typename T, T> class TT>
  int f();

  int v = f<A>();
}
