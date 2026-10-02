//options_all:-r -x -tused
//options: --strict;cp

int B;
struct B *pb;
int A;
class X {
  friend struct A;
};
struct A *pa;
void f() {
  int S, T;
  class X {
    friend struct S;
  };
  struct S *ps;
  struct T *pt;
}

