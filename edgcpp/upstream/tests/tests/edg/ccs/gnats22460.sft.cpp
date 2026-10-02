//type:fn
//options::--gnu_version 70400;fp:--clang_version 80000
//options_all:--c++17 -tused

struct X;

struct A {
  template <class c> A(c &&, X = X());
};
