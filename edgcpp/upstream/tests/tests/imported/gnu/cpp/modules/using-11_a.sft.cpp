//type: fp
//options:  --c++20 --modules
# 0 "./modules/using-11_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/using-11_a.C"




module;
# 1 "./modules/using-11.h" 1

inline int foo() { return 42; }
# 7 "./modules/using-11_a.C" 2

export module M;
export using ::foo;
