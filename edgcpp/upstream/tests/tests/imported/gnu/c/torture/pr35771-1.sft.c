//type: rp
//options: 
# 0 "./torture/pr35771-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./torture/pr35771-1.c"




typedef float __m128 __attribute__ ((__vector_size__ (16), __may_alias__));



# 1 "./torture/pr35771.h" 1
typedef __m128 __attribute__((aligned(1))) unaligned;

extern void abort (void);


__m128 __attribute__((noinline))
foo (__m128 a1, __m128 a2, __m128 a3, __m128 a4,
     __m128 a5, __m128 a6, __m128 a7, __m128 a8,
     int b1, int b2, int b3, int b4, int b5, int b6, int b7, unaligned y)
{
  return y;
}

void
do_test (void)
{
  unaligned x;
  __m128 y = { 0 };
  x = y;
  y = foo (y, y, y, y, y, y, y, y, 1, 2, 3, 4, 5, 6, -1, x);
  if (__builtin_memcmp (&y, &x, sizeof (y)) != 0)
    abort ();
}

int
main (void)
{
  do_test ();
  return 0;
}
# 10 "./torture/pr35771-1.c" 2
