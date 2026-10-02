//type:fp
//options:--c++14:--c++17:--c++20:--ms_c++14:--ms_c++17:--ms_c++20 --microsoft_version 1936

#if __cpp_concepts
namespace minimal
{
  template<class T> concept C = true;
  template<auto> struct B;
  template<C auto V> struct B<V> { };
}
#endif

namespace templ_templ_parm_matching
{
  template<typename T, T>
  struct C { };

  template<template<typename X, X> class A, typename Y, Y S>
  struct B
  { };

  template<typename T>
  using M = B<C, T, 2>;
}

#if __cpp_concepts
namespace fn_explicit_args
{
  template<class T> concept C1 = true;
  template<class T> concept C2 = C1<T> && true;

  template<C2 auto> struct S
  { };

  template<template<C1 auto> class TT> void f()
  { }

  template void f<S>();
}

namespace constrained_partial_spec
{
  template<class T> concept C = true;

  template<auto> struct B;
  template<C auto V> struct B<V> { };

  B<1> b1;
}

namespace constrained_partial_spec_ref
{
  template<class T> concept C = true;

  template<auto &> struct B;
  template<C auto &V> struct B<V> { };

  int i;
  B<i> b1;
}

namespace partial_spec_requires
{
  template<class T> concept C = true;

  template<auto> struct B;
  template<auto V> requires C<decltype(V)> struct B<V> { };

  B<1> b1;
}
#endif
