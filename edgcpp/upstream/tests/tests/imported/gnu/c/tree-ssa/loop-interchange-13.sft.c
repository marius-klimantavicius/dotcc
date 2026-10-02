//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-13.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-13.c"
# 11 "./tree-ssa/loop-interchange-13.c"
unsigned u[1024];

static void __attribute__((noinline,noclone,noipa))
foo (int N, int M, unsigned *res)
{
  int i, j;
  unsigned sum = 0;
  if (N > 0)
    for (i = 0; i < M; i++)
      for (j = 0; j < N; j++)
 sum = u[i + 3 * j] - sum;

  *res = sum;
}

extern void abort ();

int
main (void)
{
  int i, j;
  unsigned res;

  u[0] = 1;
  u[1] = 2;
  u[2] = 4;
  u[3] = 5;
  u[4] = 7;
  u[5] = 8;

  foo (2, 3, &res);





  if (res != 13)
    abort ();

  return 0;
}
