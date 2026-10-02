//options_all:-r -x -tused
//options: --strict;cp

/* Test in -m mode */
struct A {
  struct B;
  int j;
};
struct B {
  int i;
} b;

