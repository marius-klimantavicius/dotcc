//type:fn
//options::--gnu_version 70400:--clang_version 80000
//options_all:--c++17 -tused

template<typename> struct A {
  struct X;
  template <class c> A(c &&, X = X());
};

struct B : A<void> {
  using A::A;
};

B b(0);
