//type:cp
//options_all:--c++17

struct A {};

A f() {
  typedef decltype(A()) unused;
  A z[ ([x = 1]{}, 1) ];
  return z[0];
};
