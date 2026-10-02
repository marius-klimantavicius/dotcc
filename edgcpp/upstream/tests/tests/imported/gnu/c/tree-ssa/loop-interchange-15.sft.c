//type: rp
//options: 
# 0 "./tree-ssa/loop-interchange-15.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/loop-interchange-15.c"
# 16 "./tree-ssa/loop-interchange-15.c"
extern void abort ();

static void __attribute__((noipa))
foo (int n)
{
  int i, j;
  struct S { char d[n]; int a : 3; int b : 17; int c : 12; };
  struct S A[100][1111];

  for (i = 0; i < 100; i++)
    {
      asm volatile ("" : : "g" (&A[0][0]) : "memory");
      for (j = 0; j < 1111; j++)
 A[i][j].b = 2;
    }
  asm volatile ("" : : "g" (&A[0][0]) : "memory");

  for (i = 0; i < 1111; i++)
    for (j = 0; j < 100; j++)
      A[j][i].b = 5 * A[j][i].b;

  asm volatile ("" : : "g" (&A[0][0]) : "memory");
  int res = A[0][0].b + A[100 -1][1111 -1].b;





  if (res != 20)
    abort ();
}

int
main (void)
{
  foo (1);
  foo (8);
  return 0;
}
