//options_all:-r -x -tused
//options: --strict;cn

struct A {
  int i;
  (operator int[5])();
};
(A::operator int[5])() { return i; }

