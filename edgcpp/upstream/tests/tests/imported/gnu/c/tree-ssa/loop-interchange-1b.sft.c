//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-1b.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-1b.c"
# 13 "./tree-ssa/loop-interchange-1b.c"
double u[1782225];

static void __attribute__((noinline))
foo (int N, double *res)
{
  int i, j;
  double sum = 0;
  for (i = 0; i < N; i++)
    for (j = 0; j < N; j++)
      sum = sum + u[i + 1335 * j];

  *res = sum;
}

extern void abort ();

int
main (void)
{
  int i, j;
  double res;

  for (i = 0; i < 1782225; i++)
    u[i] = 0;
  u[0] = ((double)1.79769313486231570814527423731704357e+308L);
  u[1335] = -((double)1.79769313486231570814527423731704357e+308L);
  u[1] = ((double)1.79769313486231570814527423731704357e+308L);
  u[1336] = -((double)1.79769313486231570814527423731704357e+308L);

  foo (1335, &res);





  if (res != 0.0)
    abort ();

  return 0;
}
