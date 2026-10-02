//options_all:-r -x -tused
//options: --strict;cn:;cp

struct A {
  const float f = 1.0;
  char * const p = "abc";
  A * const pa = (A *)0;
  const int i = 10;
};

