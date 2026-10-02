//type: fp
//options: --c23 --strict_gnu
# 0 "./dfp/bid-non-canonical-d32-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/bid-non-canonical-d32-2.c"




# 1 "./dfp/bid-non-canonical-d32-1.c" 1




extern void abort (void);
extern void exit (int);

union u
{
  _Decimal32 d32;
  unsigned int u32;
};

int
main (void)
{
  union u x;
  _Decimal32 d32;
  x.u32 = 0x6cb89681U;
  d32 = x.d32;
  volatile double d = d32;
  if (d == 0)
    exit (0);
  else
    abort ();
}
# 6 "./dfp/bid-non-canonical-d32-2.c" 2
