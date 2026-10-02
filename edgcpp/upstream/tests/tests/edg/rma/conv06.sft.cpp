//options_all:-r -x -tused
//options: --strict;cp

struct A {
  int i;
  (operator int)();
};
(A::operator int)() { return i; }

