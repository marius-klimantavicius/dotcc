//options_all:-r -x -tused
//options: --strict;cp:;cp

struct S {
  int (*f())() throw (int) { }
};

