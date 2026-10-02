//options_all:-r -x -tused
//options: --strict;cn

// Bug EDGqa00343
union U {
    int j;
    char *s;
    U() : j(1), s("a") {}
} u;
struct S {
  union {
    int i;
    char *s;
  };
  S() : i(1), s("a") { }
} s;

