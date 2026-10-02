//type: fp
//options:  --c++20 --c++20 --modules
# 0 "./modules/tmpl-part-req-2_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tmpl-part-req-2_b.C"


# 1 "./modules/tmpl-part-req-2.h" 1

template<typename _Iterator, typename>
struct Trait;

template<typename _Iterator>
struct Trait<_Iterator, void> {};

template<typename _Iterator>
requires true && true
struct Trait<_Iterator, void>
{
  template<typename _Iter> struct __cat {};

  template<typename _Iter> requires true struct __cat<_Iter> {};
};

template<typename _Iterator>
requires true
struct Trait<_Iterator, void>
{
  template<typename _Iter> struct __diff {};

  template<typename _Iter> requires true struct __diff<_Iter> {};
};
# 4 "./modules/tmpl-part-req-2_b.C" 2
import "tmpl-part-req-2_a.H";
