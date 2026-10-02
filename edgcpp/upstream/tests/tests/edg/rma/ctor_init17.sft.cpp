//options_all:-r -x -tused
//options: --strict;cp

struct S { S(); S(int); } s(0);
struct A { const S s[2]; } a;
struct B { const S s; } b;
struct C { S s[2]; } c;
extern int i;
int j;
void f() {
  S s, ss(0);
  A a;
  int i = 0;
  int j;
}


