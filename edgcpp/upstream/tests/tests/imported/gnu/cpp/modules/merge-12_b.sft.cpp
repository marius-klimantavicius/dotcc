//type: s
//options:  --c++20 --modules
# 0 "./modules/merge-12_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-12_b.C"


# 1 "./modules/merge-12.h" 1

template<typename _Functor, typename _ArgTypes>
struct invoke_result;

template<typename _Fn, typename _ArgTypes>
struct is_invocable;

template<typename _Fn, typename... _Args>
concept invocable = is_invocable<_Fn, _Args...>::value;

template<typename _Fn, typename _Is>
requires invocable<_Fn, _Is>
  using indirect_result_t = typename invoke_result<_Fn, _Is>::type;

template<typename _Tp>
struct remove_cv;

template<typename _Iter, typename _Proj>
struct projected
{
  using value_type = remove_cv<indirect_result_t<_Proj&, _Iter>>;
};
# 4 "./modules/merge-12_b.C" 2
import "merge-12_a.H";
