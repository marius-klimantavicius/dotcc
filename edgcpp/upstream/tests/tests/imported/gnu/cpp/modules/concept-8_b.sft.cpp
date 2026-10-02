//type: s
//options:  --c++20 --modules --c++20
# 0 "./modules/concept-8_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/concept-8_b.C"



# 1 "./modules/concept-8.h" 1


template<typename _Callable>
struct Base
{
  Base (const _Callable &)
    requires true
  {}
};

template<typename _Callable> requires true
using Derived = Base<_Callable>;

inline Derived all = [] (auto&& __r) {};
# 5 "./modules/concept-8_b.C" 2
import "concept-8_a.H";
