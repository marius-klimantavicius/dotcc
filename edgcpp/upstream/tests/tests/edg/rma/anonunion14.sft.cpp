//options_all:-r -x -tused
//options: --strict;cn:;cn

// Test of anonymous union extension (C++ mode)
typedef union {
  int i, j;
} U1;
typedef union {
  int a, b;
} U2;
typedef union {
  int x, y;
} U3;
typedef union {
  U1;
  union {
    int p;
    U2;
    int q;
    union {
      int m;
      U3;
      int n;
    };
    int r;
  };
} U;
struct S {
  U;
};
main() {
  U u;
  u.i = 0;
  u.j = 0;
  u.p = 0;
  u.a = 0;
  u.b = 0;
  u.q = 0;
  u.m = 0;
  u.x = 0;
  u.y = 0;
  u.n = 0;
  u.r = 0;
  S s;
  s.i = 0;
  s.j = 0;
  s.p = 0;
  s.a = 0;
  s.b = 0;
  s.q = 0;
  s.m = 0;
  s.x = 0;
  s.y = 0;
  s.n = 0;
  s.r = 0;
};


