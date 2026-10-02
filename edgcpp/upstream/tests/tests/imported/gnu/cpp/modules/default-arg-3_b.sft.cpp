//type: fp
//options:  --c++20 --modules
# 0 "./modules/default-arg-3_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/default-arg-3_b.C"




# 1 "./modules/default-arg-3.h" 1
void f(int a, int b = 123);
template <typename T> void g(T a, T b = 123);

template <typename U = int> struct A;
template <int N = 123> struct B;

struct S {
  template <typename T = int> void x();
  void y(int n = 123);
};

struct nontrivial { nontrivial(int); };
void h(nontrivial p = nontrivial(123));
# 6 "./modules/default-arg-3_b.C" 2
import "default-arg-3_a.H";
