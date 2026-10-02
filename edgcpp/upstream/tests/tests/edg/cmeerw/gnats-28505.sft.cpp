//type:fp
//options:--c++11:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950

namespace minimal
{
  template<typename ... Ts>
  struct B { };
  template<typename ... Ts>
  struct C {
    template<typename T, typename U = T>
    struct A;
    B<A<Ts> ...> b;
  };
}

namespace nested_in_class
{
  template<typename ... Ts> void f(Ts ...);

  template<typename ... Ts>
  struct C {
    template<typename T, typename U = typename T::N>
    struct A;

    void g() {
      f(A<Ts>() ...);
    }
  };
}

namespace simple_default_arg
{
  template<typename ... Ts> void f(Ts ...);

  template<typename ... Ts>
  struct C {
    template<typename T, typename U = T>
    struct A;

    void g() {
      f(A<Ts>() ...);
    }
  };
}

namespace sizeof_expr
{
  template<typename ... Ts> int f(Ts ...);

  template<typename ... Ts>
  struct C {
    template<typename T, typename U = T>
    struct A;

    static const int v = sizeof(f<A<Ts> ...>());
  };
}

namespace out_of_class
{
  template<typename T, typename U = typename T::N>
  struct A;

  template<typename ... Ts> void f(Ts ...);

  template<typename ... Ts>
  struct C {
    void g() {
      f(A<Ts>() ...);
    }
  };
}

namespace non_type_pack
{
  template<typename ... Ts> struct A
  {
    template<template<typename, Ts = 0> class ... TTs, TTs<Ts> ... Vs> struct B
    { };
  };

  template<typename T, int> using X = T;
  A<int>::B<X> b;
}
