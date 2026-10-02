//type:fn
//options:--c++20

namespace type_param
{
  template<typename ...>
  struct C
  {
    C();

    template<typename ... U>
    C(C<U ...>);

    operator int() const;
  };

  namespace expand_fn_param_list
  {
    template<typename ... Ts, typename U>
    int f(Ts ..., U, C<void (U, Ts ...)>);

    int i = f<int>(1, 2, C<void (int, int, int)>());
  }

  namespace expand_tmpl_arg_list
  {
    template<typename ... Ts, typename U>
    int f(Ts ..., U, C<U, Ts ...>);

    int i = f<int>(1, 2, C<int, int, int>());
  }
}

namespace non_type_param
{
  template<int>
  struct B
  { };

  template<typename ...>
  struct C
  {
    C();

    template<typename ... U>
    C(C<U ...>);

    operator int() const;
  };

  namespace expand_fn_param_list
  {
    template<int ... Is, typename U>
    int f(B<Is> ..., U, C<void (U, B<Is> ...)>);

    int i = f<1>(B<1>(), 2, C<void (int, B<1>, B<2>)>());
  }

  namespace expand_tmpl_arg_list
  {
    template<int ... Is, typename U>
    int f(B<Is> ..., U, C<U, B<Is> ...>);

    int i = f<1>(B<1>(), 2, C<int, B<1>, B<2>>());
  }
}

namespace tmpl_param
{
  template<typename>
  struct A
  { };

  template<template<typename> class>
  struct B
  { };

  template<typename ...>
  struct C
  {
    C();

    template<typename ... U>
    C(C<U ...>);

    operator int() const;
  };

  namespace expand_fn_param_list
  {
    template<template<typename> class ... TTs, typename U>
    int f(B<TTs> ..., U, C<void (U, B<TTs> ...)>);

    int i = f<A>(B<A>(), 2, C<void (int, B<A>, B<A>)>());
  }

  namespace expand_tmpl_arg_list
  {
    template<template<typename> class ... TTs, typename U>
    int f(B<TTs> ..., U, C<U, B<TTs> ...>);

    int i = f<A>(B<A>(), 2, C<int, B<A>, B<A>>());
  }
}
