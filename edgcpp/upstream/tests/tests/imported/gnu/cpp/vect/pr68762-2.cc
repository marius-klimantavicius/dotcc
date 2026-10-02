//type: fp
//options: 
# 0 "./vect/pr68762-2.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/pr68762-2.cc"



# 1 "./vect/pr68762.h" 1


#pragma omp declare simd
double baz (double x);

#pragma omp declare simd
inline double
foo (double d)
{
  return baz (d);
}
# 5 "./vect/pr68762-2.cc" 2

#pragma omp declare simd
double
baz (double x)
{
  return x;
}

double
fn (double x)
{
  return foo (x);
}
