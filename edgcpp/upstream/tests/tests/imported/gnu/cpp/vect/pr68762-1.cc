//type: s
//options:  pr68762-2.cc
# 0 "./vect/pr68762-1.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/pr68762-1.cc"






# 1 "./vect/pr68762.h" 1


#pragma omp declare simd
double baz (double x);

#pragma omp declare simd
inline double
foo (double d)
{
  return baz (d);
}
# 8 "./vect/pr68762-1.cc" 2

double v[64];

double
bar ()
{
  double sum = 0.0;
#pragma omp simd reduction (+: sum)
  for (int i = 0; i < 64; i++)
    sum += foo (v[i]);
  return sum;
}

int
main ()
{
  if (bar () != 0.0)
    __builtin_abort ();
}
