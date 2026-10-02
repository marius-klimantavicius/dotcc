//remark:ADL and overload set arguments
//options:--c++17;fp

struct S {};

struct SX {};
template<template<typename ...> class C> int f(SX);

namespace X {
  template<typename> struct V {};
  template<typename T> int operator|(T, int(*)(SX));
}
namespace Y {
  struct SY {};
  template<template<typename ...> class C> int f(SY);
};

namespace Z {
  using ::f;
  using Y::f;
}

  int r = S{} | Z::f<X::V>;
