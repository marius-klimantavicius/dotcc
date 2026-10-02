//options_all:-r -x -tused
//options: --strict;cn:;cn

void f() {
  extern void g(struct X *);
  extern struct X *px;
  g(px);
}
struct X { int i; } *px;
main() {
  extern void g(struct X *);
  g(px);
}

