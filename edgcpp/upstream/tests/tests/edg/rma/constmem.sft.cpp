//options_all:-r -x -tused
//options: --strict;cn:;cp

class A {
  const int I = 10;
  static int iarray[I];
  int f();
};
int A::f() { return I; }


