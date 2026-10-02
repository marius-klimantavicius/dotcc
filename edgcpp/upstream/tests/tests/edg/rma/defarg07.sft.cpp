//options_all:-r -x -tused
//options: --strict;cp

class A {
  int ci, cj, ck;
  A(int i, int j = TWO, int k = THREE) : ci(i), cj(j), ck(k) { };
  void f(int i, int j = TWO, int k = THREE) { };
  enum { TWO=2, THREE=3 };
};

