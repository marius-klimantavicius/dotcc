//type:fn
//options_all:--c++20

template <int I> struct A {
  union B {
    int x;
  };
  B b = { .x . I } ;
};

A<29> a;
