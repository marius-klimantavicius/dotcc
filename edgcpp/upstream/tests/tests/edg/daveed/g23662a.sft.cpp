//remark:ADL and overload set arguments
//options:--c++17;fp

struct S {};

template<typename> struct V {};

namespace X {
  struct SX {};
  template<template<typename ...> class C> int f(SX);
  template<typename T> int operator|(T, int(*)(SX));
}
namespace Y {
  struct SY {};
  template<template<typename ...> class C> int f(SY);
};

namespace Z {
  using X::f;
  using Y::f;
}

  int r = S{} | Z::f<V>;
