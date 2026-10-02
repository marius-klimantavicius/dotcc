//type: fp
//options: --c23 --strict_gnu
# 0 "./dfp/bid-non-canonical-d64-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/bid-non-canonical-d64-2.c"




# 1 "./dfp/bid-non-canonical-d64-1.c" 1




extern void abort (void);
extern void exit (int);

union u
{
  _Decimal64 d64;
  unsigned long long int u64;
};

int
main (void)
{
  union u x;
  _Decimal64 d64;
  x.u64 = 0x6c7386f26fc10001ULL;
  d64 = x.d64;
  volatile double d = d64;
  if (d == 0)
    exit (0);
  else
    abort ();
}
# 6 "./dfp/bid-non-canonical-d64-2.c" 2
