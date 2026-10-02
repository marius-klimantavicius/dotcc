//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-tpl-parm-3_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-tpl-parm-3_b.C"



# 1 "./modules/tpl-tpl-parm-3.h" 1


template <typename P> struct X {};

template<template <typename> typename TT>
struct X<TT<int>> {
  template<template <typename> typename UU>
  void f (X<UU<int>>&);
};

template<template<class> class TT> struct Y;
template<template<class> class UU> struct Y { };
# 5 "./modules/tpl-tpl-parm-3_b.C" 2
import "tpl-tpl-parm-3_a.H";
