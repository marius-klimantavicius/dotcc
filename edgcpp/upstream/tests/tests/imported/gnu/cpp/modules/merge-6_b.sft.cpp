//type: fp
//options:  --c++20 --modules
# 0 "./modules/merge-6_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-6_b.C"


# 1 "./modules/merge-6.h" 1

template<bool>
struct __truth_type;

template<typename T>
struct __traitor
{
  enum { __value = true };
  typedef typename __truth_type<__value>::__type __type;
};
# 4 "./modules/merge-6_b.C" 2
import "merge-6_a.H";
