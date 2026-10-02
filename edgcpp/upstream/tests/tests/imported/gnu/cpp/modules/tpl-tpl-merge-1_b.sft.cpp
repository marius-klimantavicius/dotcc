//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-tpl-merge-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-tpl-merge-1_b.C"


# 1 "./modules/tpl-tpl-merge-1.h" 1
typedef long unsigned int size_t;

template<typename _Tp, typename _Up>
struct Replace;

template<template<typename> class _Template>
struct Replace<_Template<char>, char>
{
  using type = _Template<char>;
};

template<typename _Tp>
struct TPL;

template<typename _Alloc>
struct Traits
{
  template<typename _Tp>
  using Rebind = typename Replace<_Alloc, _Tp>::type;
};

using tdef = Traits<TPL<char>>::template Rebind<char>;
# 4 "./modules/tpl-tpl-merge-1_b.C" 2
import "tpl-tpl-merge-1_a.H";
