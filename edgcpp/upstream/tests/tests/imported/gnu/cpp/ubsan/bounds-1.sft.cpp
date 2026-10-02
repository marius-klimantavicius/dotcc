//type: rp
//options: 
# 0 "./ubsan/bounds-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ubsan/bounds-1.C"







# 1 "./ubsan/../../gcc.dg/ubsan/bounds-4.c" 1







struct A { int a; int b[]; };
struct B { int a; int b[0]; };
struct C { int a; int b[1]; };
struct D { int a; int b[2]; };
struct E { int a; int b[42]; };
struct F { int a; int b[0]; int c[2]; };
struct G { int a; int b[15]; int c[2]; };

__attribute__((noipa)) int
foo (struct A *a)
{
  return a->b[14];
}

__attribute__((noipa)) int
bar (struct B *a)
{
  return a->b[0];
}

__attribute__((noipa)) int
baz (struct C *a)
{
  return a->b[1];
}

__attribute__((noipa)) int
qux (struct D *a)
{
  return a->b[2];
}

__attribute__((noipa)) int
corge (struct E *a)
{
  return a->b[14];
}

__attribute__((noipa)) int
freddy (struct F *a)
{
  return a->b[0];
}

__attribute__((noipa)) int
garply (struct G *a)
{
  return a->b[15];
}

__attribute__((noipa)) int
waldo (struct G *a)
{
  return a->b[16];
}

int
main ()
{
  union { struct A a; struct B b; struct C c;
   struct D d; struct E e; struct F f; } u;
  struct G g;
  u.e.a = 42;
  __builtin_memset (u.e.b, 0, sizeof (u.e.b));
  __builtin_memset (&g, 0, sizeof (g));
  int r = garply (&g);
  r += foo (&u.a) + bar (&u.b) + baz (&u.c);
  r += qux (&u.d) + corge (&u.e) + freddy (&u.f);
  r += waldo (&g);
  if (r != 0)
    __builtin_abort ();
}
# 9 "./ubsan/bounds-1.C" 2
