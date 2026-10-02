//type: fp
//options:  --c++20 --modules
# 0 "./modules/loc-wrapper-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/loc-wrapper-1_b.C"



# 1 "./modules/loc-wrapper-1.h" 1
template<typename _Tp>
struct __is_integer
{
  enum { __value = 0 };
};

template<typename _Tp>
struct __is_integer_nonstrict

{
  using __is_integer<_Tp>::__value;

  enum { __width = __value ? sizeof(_Tp) * 8 : 0 };
};
# 5 "./modules/loc-wrapper-1_b.C" 2
import "loc-wrapper-1_a.H";
