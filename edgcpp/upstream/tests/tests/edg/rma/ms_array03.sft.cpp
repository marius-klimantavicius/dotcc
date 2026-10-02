//options_all:-r -x -tused
//options: --microsoft -n;cp

struct S {
  int a, b, c[];
  void f();
};
struct T {
  int a, b, c[];
  struct U { int i, j; };
};

