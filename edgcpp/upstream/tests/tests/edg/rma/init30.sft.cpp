//options_all:-r -x -tused
//options: --strict;cp

struct A { int i,j,k; };
struct B {
  int i;
  A a;
  int j;
};
B b1 = { 1, { 1 }, 1 };
B b2 = { 1, { 1, 0, 0 }, 1 };

