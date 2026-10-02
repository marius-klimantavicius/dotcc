//type: fp
//options:  --c++20 --modules
# 0 "./modules/dguide-6_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/dguide-6_b.C"



module;
# 1 "./modules/dguide-6.h" 1
template <typename T> struct S {
  S(int);
  S(int, int);
};
# 6 "./modules/dguide-6_b.C" 2
export module M;
export import :a;
S(int, int) -> S<double>;
