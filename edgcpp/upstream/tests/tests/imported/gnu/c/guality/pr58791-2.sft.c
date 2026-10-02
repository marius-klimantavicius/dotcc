//type: rp
//options: 
# 0 "./guality/pr58791-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr58791-2.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr58791-2.c" 2

__attribute__((noinline, noclone)) int
foo (unsigned char c)
{
  int ret;
  _Bool a, b, d, e, f;

  a = c == 34;
  b = c == 32;
  d = a | b;
  f = !d;
  if (d)
    ret = 1;
  else
    {
      e = c <= 31;
      ret = e;
    }

  asm volatile ("nop" : : : "memory");
  asm volatile ("nop" : : : "memory");
  return ret;
}


int
main ()
{
  foo (32);
  return 0;
}
