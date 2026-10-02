//type: rp
//options: --c23 --strict_gnu
# 0 "./dfp/bid-non-canonical-d128-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/bid-non-canonical-d128-3.c"





# 1 "./dfp/bid-non-canonical-d128-1.c" 1





extern void abort (void);
extern void exit (int);

union u
{
  _Decimal128 d128;
  unsigned __int128 u128;
};




int
main (void)
{
  unsigned __int128 i = (((unsigned __int128) 0x378d8e6400000001ULL) | (((unsigned __int128) 0x3041ed09bead87c0ULL) << 64));
  union u x;
  _Decimal128 d128;
  x.u128 = i;
  d128 = x.d128;
  volatile double d = d128;
  if (d == 0)
    exit (0);
  else
    abort ();
}
# 7 "./dfp/bid-non-canonical-d128-3.c" 2
