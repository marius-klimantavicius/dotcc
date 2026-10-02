//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-3.c"
# 16 "./tree-ssa/loop-interchange-3.c"
static int __attribute__((noinline))
foo (int A[100][200])
{
  int i, j;


  for(j = 0; j < 200; j++)
    for(i = 0; i < 100; i++)
      A[i][j] = A[i][j] + A[i][j];

  return A[0][0] + A[100 -1][200 -1];
}

extern void abort ();

static void __attribute__((noinline))
init (int *arr, int i)
{
  int j;

  for (j = 0; j < 200; j++)
    arr[j] = 2;
}

int
main (void)
{
  int A[100][200];
  int i, j, res;

  for (i = 0; i < 100; i++)
    init (A[i], i);

  res = foo (A);





  if (res != 8)
    abort ();

  return 0;
}
