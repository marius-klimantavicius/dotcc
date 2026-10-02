//options_all:-r -x -tused
//options: --strict;cp

struct A {
  int i, j;
  static A x[];
};
A A::x[] = { {0,0}, {1,1}, {2,2} };

