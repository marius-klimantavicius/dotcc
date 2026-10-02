//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-12.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-12.c"
# 11 "./tree-ssa/loop-interchange-12.c"
unsigned u[1024];

static void __attribute__((noinline,noclone,noipa))
foo (int N, unsigned *res)
{
  int i, j;
  unsigned sum = 1;
  for (i = 0; i < N; i++)
    for (j = 0; j < N; j++)
      sum = u[i + 2 * j] / sum;

  *res = sum;
}

extern void abort ();

int
main (void)
{
  int i, j;
  unsigned res;

  u[0] = 10;
  u[1] = 200;
  u[2] = 10;
  u[3] = 10;

  foo (2, &res);





  if (res != 0)
    abort ();

  return 0;
}
