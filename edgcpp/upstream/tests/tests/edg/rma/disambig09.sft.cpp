//options_all:-r -x -tused
//options: --strict;cp

struct A { A(int); };
void xxx(int *p) {
  A a(int(*p + 1));       // variable declaration
  A b(int(*p));           // function declaration
  A c(int(*p) + 1);       // variable declaration
  A d(int((*p)));         // function declaration
  A e(int((*p + 1)));     // variable declaration
  A f(int((*p) + 1));     // variable declaration
  A g(int((*p)) + 1);     // variable declaration
}

