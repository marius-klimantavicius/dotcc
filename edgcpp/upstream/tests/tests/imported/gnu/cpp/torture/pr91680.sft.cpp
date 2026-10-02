//type: rp
//options: 
# 0 "./torture/pr91680.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr91680.C"



extern "C" void abort ();

# 1 "./torture/../../gcc.dg/tree-ssa/pr91680.c" 1






__attribute__((noipa)) unsigned long long
foo (unsigned char x)
{
  unsigned long long q = 1 << x;
  return 256 / q;
}

__attribute__((noipa)) unsigned long long
bar (unsigned char x)
{
  unsigned long long q = 1U << x;
  return 256 / q;
}

__attribute__((noipa)) unsigned long long
baz (unsigned char x, unsigned long long y)
{




  unsigned long long q = 1 << x;
  return y / q;
}

__attribute__((noipa)) unsigned long long
qux (unsigned char x, unsigned long long y)
{
  unsigned long long q = 1U << x;
  return y / q;
}
# 7 "./torture/pr91680.C" 2

int
main ()
{
  unsigned char i;
  for (i = 0; i < 4 * 8; i++)
    {
      volatile unsigned long long q = 1 << i;
      if (foo (i) != 256 / q)
 abort ();
      q = 1U << i;
      if (bar (i) != 256 / q)
 abort ();
      q = 1 << i;
      if (baz (i, (1U << i) - 1) != ((1U << i) - 1) / q)
 abort ();
      if (baz (i, 1U << i) != (1U << i) / q)
 abort ();
      if (baz (i, -1) != -1 / q)
 abort ();
      q = 1U << i;
      if (qux (i, (1U << i) - 1) != ((1U << i) - 1) / q)
 abort ();
      if (qux (i, 1U << i) != (1U << i) / q)
 abort ();
      if (qux (i, -1) != -1 / q)
 abort ();
    }
}
