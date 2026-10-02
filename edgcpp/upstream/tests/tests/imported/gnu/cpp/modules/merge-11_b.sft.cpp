//type: fp
//options:  --c++20 --modules
# 0 "./modules/merge-11_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/merge-11_b.C"


# 1 "./modules/merge-11.h" 1



template<typename _From, bool>
struct __is_nt_convertible_helper;

template<typename _From>
class __is_nt_convertible_helper<_From, false>
{
  template<typename> static int __test (int);
  template<typename> static void __test(...);

public:
  using type = decltype(__test<_From>(0));
};
# 4 "./modules/merge-11_b.C" 2
import "merge-11_a.H";
