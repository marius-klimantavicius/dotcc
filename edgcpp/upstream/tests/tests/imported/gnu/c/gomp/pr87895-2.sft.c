//type: fp
//options: 
# 0 "./gomp/pr87895-2.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./gomp/pr87895-2.c"




# 1 "./gomp/pr87895-1.c" 1




#pragma omp declare simd
int
foo (int x)
{
  if (x == 0)
    return 0;
}

#pragma omp declare simd
int
bar (int *x, int y)
{
  if ((y == 0) ? (*x = 0) : *x)
    return 0;
}
# 6 "./gomp/pr87895-2.c" 2
