//type: rp
//options: 
# 0 "./guality/asm-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/asm-1.c"



# 1 "./guality/../nop.h" 1
# 5 "./guality/asm-1.c" 2

struct A { int x; unsigned short y; char z[64]; };

void __attribute__((noinline))
foo (struct A *p, char *q)
{
  int f = &p->z[p->y] - q;
  asm volatile ("nop");
  asm volatile ("nop" : : "g" (f));
  asm volatile ("" : : "g" (p), "g" (q));
}

int
main ()
{
  struct A a;
  __builtin_memset (&a, 0, sizeof a);
  a.y = 26;
  a.x = 12;
  asm volatile ("" : : "r" (&a) : "memory");
  foo (&a, &a.z[a.x]);
  return 0;
}
