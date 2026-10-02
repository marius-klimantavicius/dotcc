//type: fp
//options:  --c++20 --c++20 --modules
# 0 "./modules/pr99283-7_c.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/pr99283-7_c.C"

import "pr99283-7_b.H";

# 1 "./modules/pr99283-7-swap.h" 1
template<typename _Tp>
constexpr typename remove_reference<_Tp>::type&&
  move(_Tp&& __t) noexcept;

template<typename _Tp>
constexpr inline
typename enable_if<__and_<__not_<__is_tuple_like<_Tp>>,
     is_move_constructible<_Tp>,
     is_move_assignable<_Tp>>::value>::type
  swap(_Tp& __a, _Tp& __b)
  noexcept(__and_<is_nothrow_move_constructible<_Tp>,
    is_nothrow_move_assignable<_Tp>>::value)
{
  _Tp __tmp = move(__a);
  __a = move(__b);
  __b = move(__tmp);
}
# 5 "./modules/pr99283-7_c.C" 2

import "pr99283-7_a.H";

void Xlocale(const string& __s);
