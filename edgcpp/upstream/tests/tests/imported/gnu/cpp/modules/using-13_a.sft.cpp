//type: fp
//options:  --c++20 --modules
# 0 "./modules/using-13_a.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/using-13_a.C"



module;
# 1 "./modules/using-13.h" 1


struct A {};

template <typename> struct B {};
template <> struct B<int> { using foo = int; };
template <typename T> struct B<T*> { using bar = T; };

using C = int;

inline int D = 0;
# 6 "./modules/using-13_a.C" 2

export module M;
export using ::A;
export using ::B;
export using ::C;
export using ::D;
