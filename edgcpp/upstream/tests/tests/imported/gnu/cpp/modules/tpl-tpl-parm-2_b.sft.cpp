//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-tpl-parm-2_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-tpl-parm-2_b.C"


# 1 "./modules/tpl-tpl-parm-2.h" 1



template<typename _Alloc>
struct allocator_traits
{
  template<template<typename> class _Func>
  struct _Ptr {};

  using rebind_alloc = int;
};

inline void frob ()
{
  allocator_traits<int> _M_unpooled;
}
# 4 "./modules/tpl-tpl-parm-2_b.C" 2
import "tpl-tpl-parm-2_a.H";
