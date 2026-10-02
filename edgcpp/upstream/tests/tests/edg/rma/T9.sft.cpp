//options_all:-r -x -tused
//options: --strict;cp

struct A {
  struct B * x;
};
struct B {
  struct A * y;
} xx;

