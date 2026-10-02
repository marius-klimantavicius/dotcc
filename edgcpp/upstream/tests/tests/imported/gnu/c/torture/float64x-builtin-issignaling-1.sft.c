//type: rp
//options: 
# 0 "./torture/float64x-builtin-issignaling-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/float64x-builtin-issignaling-1.c"
# 13 "./torture/float64x-builtin-issignaling-1.c"
# 1 "./torture/builtin-issignaling-1.c" 1
# 70 "./torture/builtin-issignaling-1.c"
int
f1 (void)
{
  return __builtin_issignaling (__builtin_nansf64x (""));
}

int
f2 (void)
{
  return __builtin_issignaling (__builtin_nanf64x (""));
}

int
f3 (void)
{
  return __builtin_issignaling (0.0f64x);
}

int
f4 (_Float64x x)
{
  return __builtin_issignaling (x);
}







_Float64x w;


int
main ()
{
  if (!f1 () || f2 () || f3 ())
    __builtin_abort ();
  asm volatile ("" : : : "memory");
# 132 "./torture/builtin-issignaling-1.c"
  if (f4 (w) || !f4 (__builtin_nansf64x ("0x123")) || f4 (42.0f64x) || f4 (__builtin_nanf64x ("0x234"))
      || f4 (__builtin_inff64x ()) || f4 (-__builtin_inff64x ()) || f4 (-42.0f64x) || f4 (-0.0f64x) || f4 (0.0f64x))
    __builtin_abort ();
  w = __builtin_nansf64x ("");
  asm volatile ("" : : : "memory");
  if (!f4 (w))
    __builtin_abort ();

  return 0;
}
# 14 "./torture/float64x-builtin-issignaling-1.c" 2
