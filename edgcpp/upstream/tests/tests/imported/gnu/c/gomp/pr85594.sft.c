//type: fp
//options: 
# 0 "./gomp/pr85594.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./gomp/pr85594.c"




# 1 "./gomp/pr81768-2.c" 1



float b[10][15][10];

void
foo (void)
{
  float *i;
#pragma omp target parallel for schedule(static, 32) collapse(3)
  for (i = &b[0][0][0]; i < &b[0][0][10]; i++)
    for (float *j = &b[0][15][0]; j > &b[0][0][0]; j -= 10)
      for (float *k = &b[0][0][10]; k > &b[0][0][0]; --k)
        b[i - &b[0][0][0]][(j - &b[0][0][0]) / 10 - 1][(k - &b[0][0][0]) - 1] -= 3.5;
}
# 6 "./gomp/pr85594.c" 2
