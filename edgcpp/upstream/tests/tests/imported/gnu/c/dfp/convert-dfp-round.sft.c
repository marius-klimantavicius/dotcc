//type: fp
//options: 
# 0 "./dfp/convert-dfp-round.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./dfp/convert-dfp-round.c"
# 15 "./dfp/convert-dfp-round.c"
# 1 "./dfp/dfp-dbg.h" 1


int failures;
# 16 "./dfp/convert-dfp-round.c" 2
# 1 "./dfp/dfp-round.h" 1
# 19 "./dfp/dfp-round.h"
extern void __dfp_set_round (int);

extern int __dfp_get_round (void);
# 17 "./dfp/convert-dfp-round.c" 2

_Decimal32 d32;
_Decimal64 d64;
_Decimal128 d128;

_Decimal32 d64_to_d32 (_Decimal64 d) { return d; }
_Decimal64 d128_to_d64 (_Decimal128 d) { return d; }
_Decimal32 d128_to_d32 (_Decimal128 d) { return d; }

int
do_d64_to_d32 (_Decimal64 orig, _Decimal32 exp)
{
  d64 = orig;
  d32 = d64_to_d32 (d64);
  return (d32 == exp);
}

int
do_d128_to_d32 (_Decimal128 orig, _Decimal32 exp)
{
  d128 = orig;
  d32 = d128_to_d32 (d128);
  return (d32 == exp);
}

int
do_d128_to_d64 (_Decimal128 orig, _Decimal64 exp)
{
  d128 = orig;
  d64 = d128_to_d64 (d128);
  return (d64 == exp);
}

int
main ()
{
  __dfp_set_round(1);
  if (!do_d64_to_d32 (1.1111125dd, 1.111112df)) __builtin_abort ();
  if (!do_d64_to_d32 (1.1111135dd, 1.111113df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111125dd, -1.111113df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111135dd, -1.111114df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111125dl, 1.111112df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111135dl, 1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111125dl, -1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111135dl, -1.111114df)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111125dl, 1.111111111111112dd)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111135dl, 1.111111111111113dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111125dl, -1.111111111111113dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111135dl, -1.111111111111114dd)) __builtin_abort ();

  __dfp_set_round(0);
  if (!do_d64_to_d32 (1.1111125dd, 1.111112df)) __builtin_abort ();
  if (!do_d64_to_d32 (1.1111135dd, 1.111114df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111125dd, -1.111112df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111135dd, -1.111114df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111125dl, 1.111112df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111135dl, 1.111114df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111125dl, -1.111112df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111135dl, -1.111114df)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111125dl, 1.111111111111112dd)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111135dl, 1.111111111111114dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111125dl, -1.111111111111112dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111135dl, -1.111111111111114dd)) __builtin_abort ();

  __dfp_set_round(4);
  if (!do_d64_to_d32 (1.1111125dd, 1.111113df)) __builtin_abort ();
  if (!do_d64_to_d32 (1.1111135dd, 1.111114df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111125dd, -1.111113df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111135dd, -1.111114df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111125dl, 1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111135dl, 1.111114df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111125dl, -1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111135dl, -1.111114df)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111125dl, 1.111111111111113dd)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111135dl, 1.111111111111114dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111125dl, -1.111111111111113dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111135dl, -1.111111111111114dd)) __builtin_abort ();

  __dfp_set_round(3);
  if (!do_d64_to_d32 (1.1111125dd, 1.111112df)) __builtin_abort ();
  if (!do_d64_to_d32 (1.1111135dd, 1.111113df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111125dd, -1.111112df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111135dd, -1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111125dl, 1.111112df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111135dl, 1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111125dl, -1.111112df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111135dl, -1.111113df)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111125dl, 1.111111111111112dd)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111135dl, 1.111111111111113dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111125dl, -1.111111111111112dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111135dl, -1.111111111111113dd)) __builtin_abort ();

  __dfp_set_round(2);
  if (!do_d64_to_d32 (1.1111125dd, 1.111113df)) __builtin_abort ();
  if (!do_d64_to_d32 (1.1111135dd, 1.111114df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111125dd, -1.111112df)) __builtin_abort ();
  if (!do_d64_to_d32 (-1.1111135dd, -1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111125dl, 1.111113df)) __builtin_abort ();
  if (!do_d128_to_d32 (1.1111135dl, 1.111114df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111125dl, -1.111112df)) __builtin_abort ();
  if (!do_d128_to_d32 (-1.1111135dl, -1.111113df)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111125dl, 1.111111111111113dd)) __builtin_abort ();
  if (!do_d128_to_d64 (1.1111111111111135dl, 1.111111111111114dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111125dl, -1.111111111111112dd)) __builtin_abort ();
  if (!do_d128_to_d64 (-1.1111111111111135dl, -1.111111111111113dd)) __builtin_abort ();

  return 0;
}
