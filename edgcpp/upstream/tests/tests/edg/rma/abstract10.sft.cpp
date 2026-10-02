//options_all:-r -x -tused
//options: --strict;cp

struct S {
  virtual void f() = 0;
};
template <class T> class A {
  S x(int i);
};
A<int> a;
class B {
  template <class T> S y(T);
};

