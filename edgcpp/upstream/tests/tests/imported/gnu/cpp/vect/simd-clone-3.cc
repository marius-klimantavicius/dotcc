//type: s
//options:  simd-clone-2.cc
# 0 "./vect/simd-clone-3.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/simd-clone-3.cc"





# 1 "./vect/simd-clone-2.h" 1
struct S
{
  int s;
#pragma omp declare simd notinbranch
  int f0 (int x);
#pragma omp declare simd notinbranch uniform(this)
  int f1 (int x);
#pragma omp declare simd notinbranch linear(this:sizeof(this)/sizeof(this))
  int f2 (int x);
};

struct T
{
  int t[64];
#pragma omp declare simd aligned(this:32) uniform(this) linear(x)
  int f3 (int x);
};
# 7 "./vect/simd-clone-3.cc" 2

#pragma omp declare simd notinbranch
int
S::f0 (int x)
{
  return x + s;
}

#pragma omp declare simd notinbranch uniform(this)
int
S::f1 (int x)
{
  return x + s;
}

#pragma omp declare simd notinbranch linear(this:sizeof(this)/sizeof(this))
int
S::f2 (int x)
{
  return x + this->S::s;
}

#pragma omp declare simd uniform(this) aligned(this:32) linear(x)
int
T::f3 (int x)
{
  return t[x];
}
