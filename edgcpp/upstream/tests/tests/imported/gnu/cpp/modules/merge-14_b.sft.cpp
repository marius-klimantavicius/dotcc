//type: fp
//options:  --c++20 --modules
# 0 "./modules/merge-14_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-14_b.C"


# 1 "./modules/merge-14.h" 1
template<typename T>
struct TPL
{
  T val = 0;
};

inline TPL<int> x;
# 4 "./modules/merge-14_b.C" 2
import "merge-14_a.H";
