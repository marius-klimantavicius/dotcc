//type: rp
//options:  -w
# 0 "./pr90095-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr90095-2.c"




# 1 "./pr90095-1.c" 1




unsigned long long a;
unsigned int b;

int
main ()
{
  unsigned int c = 255, d = c |= b;
  if (8 != 8 || 4 != 4 || 8 != 8)
    return 0;
  d = __builtin_mul_overflow (-(unsigned long long) d, (unsigned char) - c, &a);
  if (d != 0)
    __builtin_abort ();
  return 0;
}
# 6 "./pr90095-2.c" 2
