//options_all:-r -x -tused
//options: --microsoft -n;cp

struct S {
  int i;
  union { int i, j; };
  int j;
};

