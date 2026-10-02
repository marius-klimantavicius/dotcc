//type: rp
//options: 
# 0 "./torture/float128x-builtin-issignaling-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float128x-builtin-issignaling-1.c"
# 13 "./torture/float128x-builtin-issignaling-1.c"
# 1 "./torture/builtin-issignaling-1.c" 1
# 70 "./torture/builtin-issignaling-1.c"
int
f1 (void)
{
  return __builtin_issignaling (__builtin_nansf128x (""));
}

int
f2 (void)
{
  return __builtin_issignaling (__builtin_nanf128x (""));
}

int
f3 (void)
{
  return __builtin_issignaling (0.0f128x);
}

int
f4 (_Float128x x)
{
  return __builtin_issignaling (x);
}







_Float128x w;


int
main ()
{
  if (!f1 () || f2 () || f3 ())
    __builtin_abort ();
  asm volatile ("" : : : "memory");
# 132 "./torture/builtin-issignaling-1.c"
  if (f4 (w) || !f4 (__builtin_nansf128x ("0x123")) || f4 (42.0f128x) || f4 (__builtin_nanf128x ("0x234"))
      || f4 (__builtin_inff128x ()) || f4 (-__builtin_inff128x ()) || f4 (-42.0f128x) || f4 (-0.0f128x) || f4 (0.0f128x))
    __builtin_abort ();
  w = __builtin_nansf128x ("");
  asm volatile ("" : : : "memory");
  if (!f4 (w))
    __builtin_abort ();

  return 0;
}
# 14 "./torture/float128x-builtin-issignaling-1.c" 2
