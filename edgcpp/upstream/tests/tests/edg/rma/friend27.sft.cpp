//options_all:-r -x -tused
//options: --strict;cn:;cn

struct A {
  void f();
  friend void A::f();
  friend void A::g();
  friend class A;
  friend A;
  friend void f();
};

