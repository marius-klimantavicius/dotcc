//options_all:-r -x -tused
//options: --strict;cn

struct X { X(); };
struct S {
  int & ri;
  X & rx;
  S() : ri(), rx() { }
} s;

