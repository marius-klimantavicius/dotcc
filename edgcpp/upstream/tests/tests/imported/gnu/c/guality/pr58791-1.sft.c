//type: rp
//options: 
# 0 "./guality/pr58791-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr58791-1.c"




# 1 "./guality/../nop.h" 1
# 6 "./guality/pr58791-1.c" 2

__attribute__((noinline, noclone)) int
foo (int x, int y)
{
  _Bool a = x != 0;
  _Bool b = y != 2;
  _Bool c = a & b;
  _Bool d = !c;
  int ret;
  if (c)
    {
      if (y < 3 || y > 4)
 ret = 1;
      else
 ret = 0;
    }
  else
    ret = 0;
  asm volatile ("nop" : : : "memory");
  asm volatile ("nop" : : : "memory");
  return ret;
}

int
main ()
{
  foo (1, 3);
  return 0;
}
