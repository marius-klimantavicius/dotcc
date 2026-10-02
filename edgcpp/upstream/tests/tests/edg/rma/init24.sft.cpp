//options_all:-r -x -tused
//options: --strict;cn:;rp

extern "C" int printf(char *, ...);
struct A {
  int i, j;
  A() : i(5), j(6) { }
  A(int k) : i(k+10), j(k+11) { }
};
struct B {
  int i, j;
  A a;
};
void g() {
  B b1 = { 1 };
  printf("%d %d %d %d\n", b1.i, b1.j, b1.a.i, b1.a.j);
  B b2 = { 1, 2, 3 };
  printf("%d %d %d %d\n", b2.i, b2.j, b2.a.i, b2.a.j);
  B b3[2] = { 1 };
  printf("%d %d %d %d\n", b3[0].i, b3[0].j, b3[0].a.i, b3[0].a.j);
  printf("%d %d %d %d\n", b3[1].i, b3[1].j, b3[1].a.i, b3[1].a.j);
}
main() {
  g();
}



