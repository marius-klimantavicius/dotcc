//type: fp
//options: 
# 0 "./ubsan/pr82353-2-aux.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ubsan/pr82353-2-aux.cc"


# 1 "./ubsan/pr82353-2.h" 1
extern unsigned long f, g;
extern bool h, i, j, k;
extern unsigned char l, m;
extern short n;
extern unsigned o;
struct B {
  short b0 : 27;
  long b1 : 10;
};
struct A {
  int a0 : 5;
};
struct C {
  static B c0;
};
struct D {
  static unsigned d0;
  A d1;
};
struct E {
  B e2;
  D e4;
};
struct F {
  E f2;
  short f4;
};
extern F p;
extern C q;
void foo ();
void bar ();
# 4 "./ubsan/pr82353-2-aux.cc" 2

B a;
E b;
B C::c0;
unsigned D::d0;

void
foo ()
{
  a.b1 = p.f2.e2.b1 = 5;
}

void
bar ()
{
  int c = p.f2.e4.d1.a0 - -~p.f4 * 89;
  q.c0.b0 = i > g * a.b0 * h - k % a.b1;
  if ((~(m * j) && -~p.f4 * 90284000534361) % ~m * j)
    b.e2.b0 << l << f;
  o = -~p.f4 * 89;
  int d = p.f4;
  if (b.e2.b0)
    b.e2.b1 = c;
  bool e = ~-~p.f4;
  a.b1 % e;
  if (k / p.f2.e2.b1)
    b.e4.d0 = g * a.b0 * h;
  n = j;
}
