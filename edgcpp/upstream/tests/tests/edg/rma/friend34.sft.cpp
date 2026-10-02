//options_all:-r -x -tused
//options: --strict;cp

struct A {
  typedef int INT;
  void f(INT);
};
class B {
  friend void A::f(INT);
};

