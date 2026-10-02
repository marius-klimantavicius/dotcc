//options_all:-r -x -tused
//options: --strict;cn

struct B;
struct A {
  int i, j;
  static B x[];
};
B A::x[] = { {0,0}, {1,1}, {2,2} };

