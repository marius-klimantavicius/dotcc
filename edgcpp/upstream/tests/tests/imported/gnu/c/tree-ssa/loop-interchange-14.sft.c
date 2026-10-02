//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-14.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-14.c"
# 15 "./tree-ssa/loop-interchange-14.c"
struct S { int a : 3; int b : 17; int c : 12; };
struct S A[100][1111];

static int __attribute__((noinline))
foo (void)
{
  int i, j;

  for( i = 0; i < 1111; i++)
    for( j = 0; j < 100; j++)
      A[j][i].b = 5 * A[j][i].b;

  return A[0][0].b + A[100 -1][1111 -1].b;
}

extern void abort ();

static void __attribute__((noinline))
init (int i)
{
  int j;

  for (j = 0; j < 1111; j++)
    A[i][j].b = 2;
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
