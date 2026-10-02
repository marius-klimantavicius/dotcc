//type: rp
//options: 
# 0 "./other/offsetof2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./other/offsetof2.C"
# 9 "./other/offsetof2.C"
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 10 "./other/offsetof2.C" 2


# 11 "./other/offsetof2.C"
struct POD1
{
  int m;

  void *operator& () const {return 0;}
};

struct POD2
{
  int m;
};

void *operator& (POD2 const &) {return 0;}

struct POD3
{
  int prefix;

  POD1 m;
};

struct POD4
{
  int prefix;

  POD1 m;
};

int main ()
{
  if (
# 41 "./other/offsetof2.C" 3 4
     __builtin_offsetof (
# 41 "./other/offsetof2.C"
     POD3
# 41 "./other/offsetof2.C" 3 4
     , 
# 41 "./other/offsetof2.C"
     m
# 41 "./other/offsetof2.C" 3 4
     ) 
# 41 "./other/offsetof2.C"
                        != sizeof (int))
    return 1;
  if (
# 43 "./other/offsetof2.C" 3 4
     __builtin_offsetof (
# 43 "./other/offsetof2.C"
     POD4
# 43 "./other/offsetof2.C" 3 4
     , 
# 43 "./other/offsetof2.C"
     m
# 43 "./other/offsetof2.C" 3 4
     ) 
# 43 "./other/offsetof2.C"
                        != sizeof (int))
    return 2;
  return 0;
}
