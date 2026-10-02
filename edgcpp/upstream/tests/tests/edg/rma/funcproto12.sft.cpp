//options_all:-r -x -tused
//options: --strict;cp

void (*(x[2]))(struct A *pa, int i);
typedef void (FT)(struct B *pa, int i);
class X {
  void (*(x[2]))(struct A *pa, int i);
  typedef void (FT)(struct B *pa, int i);
};
void f() {
  void (*(x[2]))(struct A *pa, int i);
  typedef void (FT)(struct B *pa, int i);
}

