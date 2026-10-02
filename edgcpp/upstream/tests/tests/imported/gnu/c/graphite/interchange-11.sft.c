//type: fp
//options: 
# 0 "./graphite/interchange-11.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./graphite/interchange-11.c"







double u[1782225];

static void __attribute__((noinline))
foo (int N, int *res)
{
  int i, j;
  double sum = 0.0;


  for (i = 0; i < 1335; i++)
    {
      for (j = 0; j < 1335; j++)
 sum = sum + u[i + 1335 * j];

      u[1336 * i] *= 2;
    }
  *res = sum;
}

extern void abort ();

int
main (void)
{
  int i, res;

  for (i = 0; i < 1782225; i++)
    u[i] = 2;

  foo (1335, &res);





  if (res != 3564450)
    abort ();

  return 0;
}
