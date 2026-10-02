//options_all:-r -x -tused
//options: --strict;cn

void f() {
  int X, Y;
  struct Z;
  struct A {
    struct X *p;
    friend struct Y;
    friend void Z();
  } a;
  struct X *px;
  struct Y *py;
  struct Z *pz;
}


