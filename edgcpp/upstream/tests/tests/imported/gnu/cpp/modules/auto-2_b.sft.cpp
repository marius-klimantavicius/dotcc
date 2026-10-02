//type: s
//options:  --c++20 --modules
# 0 "./modules/auto-2_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/auto-2_b.C"


# 1 "./modules/auto-2.h" 1

template<typename _Callable>
struct _RangeAdaptor
{
  constexpr _RangeAdaptor(const _Callable &) { }
};

template<typename _Callable>
_RangeAdaptor(_Callable) -> _RangeAdaptor<_Callable>;

template<unsigned _Nm>
inline constexpr _RangeAdaptor elements = [] (auto&& __r) {};
# 4 "./modules/auto-2_b.C" 2
import "auto-2_a.H";
