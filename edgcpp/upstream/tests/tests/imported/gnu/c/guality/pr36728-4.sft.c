//type: rp
//options: 
# 0 "./guality/pr36728-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr36728-4.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr36728-4.c" 2

int __attribute__((noinline))
foo (int arg1, int arg2, int arg3, int arg4, int arg5, int arg6, int arg7)
{
  char x[30];
  int __attribute__ ((aligned(32))) y;

  y = 2;
  asm ("nop" : "=m" (y) : "m" (y));
  x[0] = 25;
  asm volatile ("nop" : "=m" (x[0]) : "m" (x[0]));
  return y;
}
# 44 "./guality/pr36728-4.c"
int
main ()
{
  int l = 0;
  asm volatile ("" : "=r" (l) : "0" (l));
  foo (l + 1, l + 2, l + 3, l + 4, l + 5, l + 6, l + 30);
  asm volatile ("" :: "r" (l));
  return 0;
}
