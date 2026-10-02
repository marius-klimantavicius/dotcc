//options_all:-r -x -tused
//options: --strict;rp:;rp

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
struct C {
  B b;
};
void g() {
  C c1 = { 1, 2, 3 };
  printf("c1:\t%d %d %d %d\n", c1.b.i, c1.b.j, c1.b.a.i, c1.b.a.j);
  C c2 = { 1 };
  printf("c2:\t%d %d %d %d\n", c2.b.i, c2.b.j, c2.b.a.i, c2.b.a.j);
  C c3 = { };
  printf("c3:\t%d %d %d %d\n", c3.b.i, c3.b.j, c3.b.a.i, c3.b.a.j);
  C c4[2] = { c1, 1 };
//  C c4[2];
  printf("c4:\t%d %d %d %d\t", c4[0].b.i, c4[0].b.j, c4[0].b.a.i, c4[0].b.a.j);
  printf("%d %d %d %d\n", c4[1].b.i, c4[1].b.j, c4[1].b.a.i, c4[1].b.a.j);
  C c5[2] = { { 1 } , { 1 } };
  printf("c5:\t%d %d %d %d\t\t", c5[0].b.i, c5[0].b.j, c5[0].b.a.i, c5[0].b.a.j);
  printf("%d %d %d %d\n", c5[1].b.i, c5[1].b.j, c5[1].b.a.i, c5[1].b.a.j);
  C c6[2] = { 1 };
  printf("c6:\t%d %d %d %d\t\t", c6[0].b.i, c6[0].b.j, c6[0].b.a.i, c6[0].b.a.j);
  printf("%d %d %d %d\n", c6[1].b.i, c6[1].b.j, c6[1].b.a.i, c6[1].b.a.j);
}
int main() {
  g();
}



