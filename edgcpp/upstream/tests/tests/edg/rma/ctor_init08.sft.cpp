//options_all:-r -x -tused
//options: --strict;cn

struct A {
  const int i;
  int & ri;
  A(int) { }
  A() : i() { }
};

