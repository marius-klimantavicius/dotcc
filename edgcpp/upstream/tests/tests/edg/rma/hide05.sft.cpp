//options_all:-r -x -tused
//options: --strict;cn:;cp

// int x;
struct A {
  int x;
  void f();
};
int x;
void A::f() {
  ::x = 1;
};

