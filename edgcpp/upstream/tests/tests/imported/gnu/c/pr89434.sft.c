//type: rp
//options: 
# 0 "./pr89434.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr89434.c"




# 1 "./../gcc.c-torture/execute/pr89434.c" 1



long g = 0;

static inline unsigned long long
foo (unsigned long long u)
{
  unsigned x;
  __builtin_mul_overflow (-1, g, &x);
  u |= (unsigned) u < (unsigned short) x;
  return x - u;
}

int
main ()
{
  unsigned long long x = foo (0x222222222ULL);
  if (x != 0xfffffffddddddddeULL)
    __builtin_abort ();
  return 0;
}
# 6 "./pr89434.c" 2
