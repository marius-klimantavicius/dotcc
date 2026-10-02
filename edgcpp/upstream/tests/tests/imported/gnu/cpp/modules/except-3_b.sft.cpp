//type: fp
//options:  --c++20 --modules
# 0 "./modules/except-3_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/except-3_b.C"


# 1 "./modules/except-3.h" 1

template<typename _Tp>
struct is_nothrow_move_constructible
{
  static constexpr bool value = false;
};

template<typename _Head>
struct _Tuple_impl
{
  _Tuple_impl () noexcept(is_nothrow_move_constructible<_Head>::value)
 { }
};

template<typename T>
void TPL (_Tuple_impl<T> &) noexcept
{
  _Tuple_impl<T> m;
}

inline void foo (_Tuple_impl<int> &p)
{
  TPL<int> (p);
}
# 4 "./modules/except-3_b.C" 2
import "except-3_a.H";
