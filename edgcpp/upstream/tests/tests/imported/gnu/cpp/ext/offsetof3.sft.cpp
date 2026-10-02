//type: fp
//options: --c++11
# 0 "./ext/offsetof3.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/offsetof3.C"




# 1 "./ext/offsetof2.C" 1



struct S { unsigned long x[31]; };
struct T { bool b; S f; };
static_assert (__builtin_offsetof (T, f.x[31 - 1]) == __builtin_offsetof (T, f.x[30]), "");
# 6 "./ext/offsetof3.C" 2
