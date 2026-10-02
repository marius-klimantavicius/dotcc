//type:fp
//options:--c++11:--c++20:--c++20 --gn 140100:--ms_c++20 --microsoft_version 1936:--c++20 --clang_version 190100;fn

namespace minimal
{
  template<typename>
  struct C {
    C(int);
  };
  template<typename ... Ts>
  int f(Ts ..., C<Ts ...>);
  int i = f<int>(1, 2);
}

namespace type_arg
{
  template<typename>
  struct C
  {
    C();

    template<typename U>
    C(C<U>);
  };

  template<typename ... Ts>
  int f(Ts ..., C<Ts ...>);

  int i = f<int>(1, C<void>());
}

namespace nested_type_arg
{
  template<typename>
  struct B
  {
    B();

    template<typename U>
    B(B<U>);
  };

  template<typename ...>
  struct C
  {
    C();

    template<typename ... U>
    C(C<U ...>);
  };

  template<typename ... Ts>
  int f(B<Ts> ..., C<Ts ...>);

  int i = f<int>(B<void>(), C<void>());
}

#ifndef _MSC_VER
namespace nested_fn_type
{
  template<typename ... Ts>
  int f(Ts ..., void(Ts ...));

  int i = f<int>('a', [] (int) { });
}
#endif

namespace nested_type_arg_with_other_param
{
  template<typename, typename>
  struct B
  {
    B();

    template<typename U, typename V>
    B(B<U, V>);
  };

  template<typename, typename>
  struct C
  {
    C();

    template<typename U, typename V>
    C(C<U, V>);
  };

  template<typename ... Ts, typename U>
  int f(B<void, Ts> ..., C<U, Ts> ...);

  template<typename ... Ts, typename U>
  int g(B<U, Ts> ..., C<void, Ts> ...); // MSVC actually complains that it
                                        // won't be able to deduce U, but then
                                        // does actually deduce U in the call
                                        // below

  int i = f<int>(B<void, char>(), C<void, int>());
  int j = g<int>(B<void, int>(), C<void, char>());
}

namespace nested_inner_type_arg_with_other_param
{
  template<typename, typename>
  struct B
  {
    B();

    template<typename U, typename V>
    B(B<U, V>);
  };

  template<typename ...>
  struct C
  {
    C();

    template<typename ... U>
    C(C<U ...>);
  };

  template<typename ... Ts, typename U>
  int f(B<void, Ts> ..., C<U, Ts ...>);

  template<typename ... Ts, typename U>
  int g(B<U, Ts> ..., C<void, Ts ...>); // MSVC actually complains that it
                                        // won't be able to deduce U, but then
                                        // does actually deduce U in the call
                                        // below

  int i = f<int>(B<void, char>(), C<void, int>());
  int j = g<int>(B<void, int>(), C<void, char>());
}

namespace nested_non_type_param
{
  template<int>
  struct B
  {
    B();

    template<int U>
    B(B<U>);
  };

  template<int>
  struct C
  {
    C();

    template<int U>
    C(C<U>);
  };

  template<int ... Is>
  int f(B<Is> ..., C<Is ...>);

  int i = f<1>(B<0>(), C<0>());
}

namespace nested_template_param
{
  template<typename>
  struct A1
  { };

  template<typename>
  struct A2
  { };

  template<template<typename> class>
  struct B
  {
    B();

    template<template<typename> class U>
    B(B<U>);
  };

  template<template<typename> class>
  struct C
  {
    C();

    template<template<typename> class U>
    C(C<U>);
  };

  template<template<typename> class ... TTs>
  int f(B<TTs> ..., C<TTs ...>);

  int i = f<A1>(B<A2>(), C<A2>());
}
