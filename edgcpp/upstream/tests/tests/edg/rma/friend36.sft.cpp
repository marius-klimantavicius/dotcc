//options_all:-r -x -tused
//options: --strict;cn:;cn

class A {
  void f(int);
  friend void A::f() { }
};

