//type: s
//options:  --c++20 --modules
# 0 "./modules/concept-6_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/concept-6_b.C"


# 1 "./modules/concept-6.h" 1

template<typename _Callable>
struct Base
{
  Base (const _Callable &)
    requires true
  {}
};

template<typename _Callable>
struct Derived : Base<_Callable>
{
  using Base<_Callable>::Base;
};

template<typename _Callable>
Derived (_Callable) -> Derived<_Callable>;

inline Derived all = [] (auto&& __r) {};
# 4 "./modules/concept-6_b.C" 2
import "concept-6_a.H";
