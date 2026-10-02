//type:fn
//options:--c++11

namespace invalid_nested_name_specifier
{
  template<template<int> typename T, typename U>
  struct A
  {
    typename T<U>::C f();
    typename T<U>::C g();
  };

  template<long>
  struct B
  {
    typedef short C;
  };

  auto v = A<B, long>().g();
}

namespace is_enum_name
{
  namespace ns
  {
    enum class E : unsigned int
    {
      E0, E1, E2
    };
  }

  template<ns::E V = ns::E::E1>
  struct X
  { };

  X<>::type1 x1;                // should show template arg as E::E1
  X<ns::E::E2>::type2 x2;       // should show template arg as E::E2
  X<(ns::E)3>::type3 x3;        // should show template arg using cast
}
