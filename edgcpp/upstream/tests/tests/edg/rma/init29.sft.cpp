//options_all:-r -x -tused
//options: --strict;cp

struct S { int i; ~S(); };
struct X { X(); };
struct SS {
  int i;
  S s;
  S sa[3];
  X x;
};
SS a = { 1 };
SS b = { 1,1,1 };

