//type: s
//options:  --c++20 --modules
# 0 "./modules/merge-13_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-13_b.C"


# 1 "./modules/merge-13.h" 1
template<typename T> class Base;

template<typename U> class Derived : Base<U>
{
  using Base_ = Base<U>;
  using typename Base_::base_member;

public:
  base_member Func ();
};
# 4 "./modules/merge-13_b.C" 2
import "merge-13_a.H";
