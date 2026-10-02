//type: rp
//options: 
# 0 "./guality/pr78726.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr78726.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr78726.c" 2

unsigned char b = 36, c = 173;
unsigned int d;

__attribute__((noinline, noclone)) void
foo (void)
{
  unsigned a = ~b;
  unsigned d1 = a * c;
  unsigned d2 = d1 * c;
  unsigned d3 = 1023094746 * a;
  d = d2 + d3;
  unsigned d4 = d1 * 2;
  unsigned d5 = d2 * 2;
  unsigned d6 = d3 * 2;
  asm ("nop" : : : "memory");
}

int
main ()
{
  asm volatile ("" : : "g" (&b), "g" (&c) : "memory");
  foo ();
  return 0;
}
