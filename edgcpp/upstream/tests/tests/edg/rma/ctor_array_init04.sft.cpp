//options_all:-r -x -tused
//options: --strict;cp

struct S { int i; ~S(); S(int); S(const S&); S(); };
struct SS {
  S a, b;
  S c[3];
  S d;
};
S s;
SS ss = { 1, s, { 1, s } };
S a[2][3][2] = { 1,1,1,1,1,1,1,1,1,1,1 };
S a2[2] = { 1 };

