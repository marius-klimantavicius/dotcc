//type: s
//options:  simd-clone-4.cc
# 0 "./vect/simd-clone-5.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/simd-clone-5.cc"





# 1 "./vect/simd-clone-4.h" 1
template <int N>
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

template <int N>
struct T
{
  int t[64];
#pragma omp declare simd aligned(this:32) uniform(this) linear(x)
  int f3 (int x);
};
# 7 "./vect/simd-clone-5.cc" 2

#pragma omp declare simd notinbranch
template <int N>
int
S<N>::f0 (int x)
{
  return x + s;
}

#pragma omp declare simd notinbranch uniform(this)
template <int N>
int
S<N>::f1 (int x)
{
  return x + s;
}

#pragma omp declare simd notinbranch linear(this:sizeof(this)/sizeof(this))
template <int N>
int
S<N>::f2 (int x)
{
  return x + this->S::s;
}

#pragma omp declare simd uniform(this) aligned(this:32) linear(x)
template <int N>
int
T<N>::f3 (int x)
{
  return t[x];
}

template struct S<0>;
template struct T<0>;
