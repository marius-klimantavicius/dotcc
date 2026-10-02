//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

struct A {
  void f();
  void f(int);
};
struct B : public A {
  using A::f;
  void f() { f(0); }
};

