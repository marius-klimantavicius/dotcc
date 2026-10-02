//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-2.c"
# 15 "./tree-ssa/loop-interchange-2.c"
int A[100][1111];

static int __attribute__((noinline))
foo (void)
{
  int i, j;

  for( i = 0; i < 1111; i++)
    for( j = 0; j < 100; j++)
      A[j][i] = 5 * A[j][i];

  return A[0][0] + A[100 -1][1111 -1];
}

extern void abort ();

static void __attribute__((noinline))
init (int i)
{
  int j;

  for (j = 0; j < 1111; j++)
    A[i][j] = 2;
}

int
main (void)
{
  int i, j, res;

  for (i = 0; i < 100; i++)
    init (i);

  res = foo ();





  if (res != 20)
    abort ();

  return 0;
}
