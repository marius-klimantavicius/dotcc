//options_all:-r -x -tused
//options: --strict;cn:;cn

void f() {
  struct X;
  int Y;
  void Z();
  struct S {
    friend void X();
    friend void Y();
    friend void Z();
    friend void Z(int);
  };
}

