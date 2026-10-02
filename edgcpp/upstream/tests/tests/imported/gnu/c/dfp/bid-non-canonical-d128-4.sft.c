//type: rp
//options: --c23 --strict_gnu
# 0 "./dfp/bid-non-canonical-d128-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/bid-non-canonical-d128-4.c"






# 1 "./dfp/bid-non-canonical-d128-2.c" 1






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
  unsigned __int128 i = (((unsigned __int128) 0x1ULL) | (((unsigned __int128) 0x6e79000000000000ULL) << 64));
  union u x;
  _Decimal128 d128;
  x.u128 = i;
  d128 = x.d128;
  volatile double d = d128;
  if (d != 0)
    abort ();

  _Decimal128 t1233 = 0.e1233DL, t1234 = 0.e1234DL, t1235 = 0.e1235DL;
  _Decimal128 dx;
  dx = d128 + t1233;
  if (__builtin_memcmp (&dx, &t1233, 16) != 0)
    abort ();
  dx = d128 + t1234;
  if (__builtin_memcmp (&dx, &t1234, 16) != 0)
    abort ();
  dx = d128 + t1235;
  if (__builtin_memcmp (&dx, &t1234, 16) != 0)
    abort ();
  exit (0);
}
# 8 "./dfp/bid-non-canonical-d128-4.c" 2
