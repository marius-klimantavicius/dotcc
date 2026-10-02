//type: rp
//options: 
# 0 "./guality/pr58791-5.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr58791-5.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr58791-5.c" 2

__attribute__((noinline, noclone)) unsigned int
foo (unsigned int a0, unsigned int a1, unsigned int a2,
     unsigned int a3, unsigned int a4)
{
  unsigned int b0, b1, b2, b3, b4, e;

  b4 = a4 + a3 + a2 + a1 + a0;
  b3 = a3 + a2 + a1 + a0;
  b2 = a2 + a1 + a0;
  b1 = a1 + a0;

  e = b4 - b3 + b2 - b1 - a4 - a2;
  asm volatile ("nop" : : : "memory");
  asm volatile ("nop" : : : "memory");
  return e;
}

int
main ()
{
  foo (1, 8, 64, 512, 4096);
  return 0;
}
