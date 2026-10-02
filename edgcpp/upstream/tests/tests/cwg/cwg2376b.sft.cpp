//type:fn
//options_all:--c++20 -A
  template <class ...T> struct A {
    A(T...) {}
  };
  const A& y{}; // error: no declarator operators allowed
