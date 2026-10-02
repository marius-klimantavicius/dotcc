//type: fn
//options:  --c++11
# 0 "./warn/Wconversion-null-4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wconversion-null-4.C"



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
# 5 "./warn/Wconversion-null-4.C" 2


# 6 "./warn/Wconversion-null-4.C"
void callee_1 (int, int, int) {}

void caller_1 (void)
{
  callee_1 (0, 
# 10 "./warn/Wconversion-null-4.C" 3 4
              __null
# 10 "./warn/Wconversion-null-4.C"
                  , 2);
# 19 "./warn/Wconversion-null-4.C"
}

void callee_2 (int, void *, int) {}


void caller_2 (void)
{
  callee_2 (0, false, 2);
# 43 "./warn/Wconversion-null-4.C"
}
