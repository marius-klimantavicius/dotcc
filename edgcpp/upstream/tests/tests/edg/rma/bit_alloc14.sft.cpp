//options_all:-r -x -tused
//options: --strict;cp

enum Z { red, green, blue, brown, violet, purple };
struct A { char a; char b:1; };
struct B { enum Z a; int b:1; };
struct C { int a; int b:1; int c; int d:1; };
struct D { int a; int b:1; int c:1; int d:1; int e:1; int f; };
struct E { unsigned char a; unsigned int b:1; };

#if 0
#ifdef __cplusplus
extern "C" void printf(char*, ...);
#else
extern void printf();
#endif /* __cplusplus */

void disp(char *name, int size, unsigned char x[]) {
  int i, j;
  printf("%s: size = %d", name, size);
  for (i = 0; i < size; i++) {
    if (i % 4 == 0) printf("\n\t");
    for (j = 7; j >= 0; j--) printf("%d", x[i] & (1<<j) ? 1 : 0);
    printf(" ");
  }  /* for */
  printf("\n");
}

struct A a = { 1, 1 };
struct B b = { (enum Z)1, 1 };
struct C c = { 1, 1, 1, 1 };
struct D d = { 1, 1, 1, 1, 1, 1 };
struct E e = { 1, 1 };
int main() {
  disp("A", sizeof(A), (unsigned char *)&a);
  printf("\ta.a = %d, a.b = %d\n", (int)a.a, (int)a.b);
  disp("B", sizeof(B), (unsigned char *)&b);
  printf("\tb.a = %d, b.b = %d\n", (int)b.a, (int)b.b);
  disp("C", sizeof(C), (unsigned char *)&c);
  printf("\tc.a = %d, c.b = %d, c.c = %d, c.d = %d\n",
         (int)c.a, (int)c.b, (int)c.c, (int)c.d);
  printf("\toffset of c.c = %d, size = %d\n",
         (int)&c.c - (int)&c, sizeof(c.c));
  disp("D", sizeof(D), (unsigned char *)&d);
  printf("\td.a = %d, d.b = %d, d.c = %d, d.d = %d, d.e = %d, d.f = %d\n",
         (int)d.a, (int)d.b, (int)d.c, (int)d.d, (int)d.e, (int)d.f);
  printf("\toffset of d.f = %d, size = %d\n",
         (int)&d.f - (int)&d, sizeof(d.f));
  disp("E", sizeof(E), (unsigned char *)&e);
  printf("\te.a = %d, e.b = %d\n", (int)e.a, (int)e.b);
}
#endif /* if 0 */

