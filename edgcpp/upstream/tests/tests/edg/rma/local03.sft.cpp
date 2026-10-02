//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cp

void f() {
  class A {public: int a,b,c; int ff(int i) { return a+i; } };
  A x;
  x.a = 1;
  x.b = x.ff(3);
}

