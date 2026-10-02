//type: fp
//options: 
# 0 "./other/offsetof5.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./other/offsetof5.C"



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
# 5 "./other/offsetof5.C" 2


# 6 "./other/offsetof5.C"
struct A
{
  char c;
  int &i;
};

int j = 
# 12 "./other/offsetof5.C" 3 4
       __builtin_offsetof (
# 12 "./other/offsetof5.C"
       A
# 12 "./other/offsetof5.C" 3 4
       , 
# 12 "./other/offsetof5.C"
       i
# 12 "./other/offsetof5.C" 3 4
       )
# 12 "./other/offsetof5.C"
                      ;

template <typename T>
struct S
{
  T h;
  T &i;
  static const int j = 
# 19 "./other/offsetof5.C" 3 4
                      __builtin_offsetof (
# 19 "./other/offsetof5.C"
                      S
# 19 "./other/offsetof5.C" 3 4
                      , 
# 19 "./other/offsetof5.C"
                      i
# 19 "./other/offsetof5.C" 3 4
                      )
# 19 "./other/offsetof5.C"
                                     ;
};

int k = S<int>::j;
