//type: fp
//options: 
# 0 "./graphite/uns-interchange-14.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./graphite/uns-interchange-14.c"
# 10 "./graphite/uns-interchange-14.c"
unsigned int A[200][200], B[200][200], C[200][200];

static void __attribute__((noinline))
matmult (void)
{
  int i, j, k;

  for (i = 0; i < 200; i++)
    for (j = 0; j < 200; j++)
      A[i][j] = 0;


  for (i = 0; i < 200; i++)
    for (j = 0; j < 200; j++)
      for (k = 0; k < 200; k++)
 A[i][j] += B[i][k] * C[k][j];
}

extern void abort ();

int
main (void)
{
  int i, j;
  unsigned res = 0;

  for (i = 0; i < 200; i++)
    for (j = 0; j < 200; j++)
      {
 B[i][j] = j;
 C[i][j] = i;
      }

  matmult ();

  for (i = 0; i < 200; i++)
    res += A[i][i];





  if (res != 529340000)
    abort ();

  return 0;
}
