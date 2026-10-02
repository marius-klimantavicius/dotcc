//type: fp
//options:  --c++20 --modules
# 0 "./modules/merge-8_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-8_b.C"


# 1 "./modules/merge-8.h" 1

struct __do_is_destructible_impl
{
  template<typename _Tp, typename = decltype(_Tp().~_Tp())>
  static bool __test(int);

  template<typename>
  static float __test(...);
};

template<typename _Tp>
struct __is_destructible_impl
  : public __do_is_destructible_impl
{

  typedef decltype(__test<_Tp>(0)) type;
};
# 4 "./modules/merge-8_b.C" 2
import "merge-8_a.H";
