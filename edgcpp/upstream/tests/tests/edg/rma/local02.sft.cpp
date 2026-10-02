//options_all:-r -x -tused --set_flag=no_checking_pragmas
//options: --strict;cn

class A {};
A x;
void g(A);
void f() {
  class A {public: int a,b,c; int ff(int i) { return a+i; } };
  extern A x;
  extern void g(A);
  x.a = 1;
  x.b = x.ff(3);
  g(x);
}

