//options_all:-r -x -tused
//options: --strict;cn

int y;
void f() {
  int x;
  extern void y();
  struct S {
    friend void x();
  };
}
int z;
struct S {
  friend void z();
};

