//type: fp
//options:  --c++20 --modules
# 0 "./modules/merge-16_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-16_b.C"



# 1 "./modules/merge-16.h" 1


void f(int*);

template <typename T>
struct S {
  void g(int n) { f(&n); }
};

template struct S<void>;
# 5 "./modules/merge-16_b.C" 2
import merge16;
