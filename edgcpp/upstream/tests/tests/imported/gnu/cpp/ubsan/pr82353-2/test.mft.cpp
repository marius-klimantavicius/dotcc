//type: rp
//options:  --c++11 -w pr82353-2-aux.cc
# 0 "./ubsan/pr82353-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ubsan/pr82353-2.C"





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
# 7 "./ubsan/pr82353-2.C" 2

unsigned long f, g;
bool h, k, j, i;
unsigned char l, m;
short n;
unsigned o;
F p;

int
main ()
{
  foo ();
  bar ();
}
