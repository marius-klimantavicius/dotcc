//type: fp
//options:  --c++20 --modules
# 0 "./modules/partial-7_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/partial-7_b.C"




module;
# 1 "./modules/partial-7.h" 1

template <typename> struct A;
template <typename T> struct A<T*> {};
template <typename T> struct B { A<T*> f(); };
B<int> inst();
# 7 "./modules/partial-7_b.C" 2
export module B;
import A;
B<int> b;
