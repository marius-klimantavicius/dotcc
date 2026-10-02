//type: rp
//options: 
# 0 "./torture/bfloat16-builtin-issignaling-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/bfloat16-builtin-issignaling-1.c"
# 21 "./torture/bfloat16-builtin-issignaling-1.c"
# 1 "./torture/builtin-issignaling-1.c" 1
# 70 "./torture/builtin-issignaling-1.c"
int
f1 (void)
{
  return __builtin_issignaling (__builtin_nansf16b (""));
}

int
f2 (void)
{
  return __builtin_issignaling (((__bf16) __builtin_nanf ("")));
}

int
f3 (void)
{
  return __builtin_issignaling (0.0bf16);
}

int
f4 (__bf16 x)
{
  return __builtin_issignaling (x);
}







__bf16 w;


int
main ()
{
  if (!f1 () || f2 () || f3 ())
    __builtin_abort ();
  asm volatile ("" : : : "memory");
# 132 "./torture/builtin-issignaling-1.c"
  if (f4 (w) || !f4 (__builtin_nansf16b ("0x123")) || f4 (42.0bf16) || f4 (((__bf16) __builtin_nanf ("0x234")))
      || f4 (((__bf16) __builtin_inff ())) || f4 (-((__bf16) __builtin_inff ())) || f4 (-42.0bf16) || f4 (-0.0bf16) || f4 (0.0bf16))
    __builtin_abort ();
  w = __builtin_nansf16b ("");
  asm volatile ("" : : : "memory");
  if (!f4 (w))
    __builtin_abort ();

  return 0;
}
# 22 "./torture/bfloat16-builtin-issignaling-1.c" 2
