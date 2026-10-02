//options_all:-r -x -tused
//options: --strict;cp

struct A {
  struct B;
} a;
struct A::B {
  int i;
} b;

