//type: fp
//options: 
# 0 "./graphite/interchange-9.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./graphite/interchange-9.c"
# 11 "./graphite/interchange-9.c"
static int __attribute__((noinline))
foo (int *x)
{
  int i, j;
  int sum = 0;

  for (j = 0; j < 111; ++j)
    for (i = 0; i < 111; ++i)
      sum += x[111 * i + j];

  return sum;
}

extern void abort ();

int
main (void)
{
  int A[111*111];
  int i, res;

  for (i = 0; i < 111*111; i++)
    A[i] = 2;

  res = foo (A);





  if (res != 24642)
    abort ();

  return 0;
}
