//type: fp
//options: --c++20
# 0 "./cpp2a/spaceship-sfinae1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/spaceship-sfinae1.C"



template <class T, T x = (T() <=> T()) == 0>
void f(T);
constexpr int f(...) { return 42; }
constexpr int i = f(24);
