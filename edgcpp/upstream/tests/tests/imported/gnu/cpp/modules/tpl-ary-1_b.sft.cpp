//type: fp
//options:  --c++20 --modules
# 0 "./modules/tpl-ary-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/tpl-ary-1_b.C"


# 1 "./modules/tpl-ary-1.h" 1

inline int ary[4];
extern int unb[];
typedef int z[0];


template<typename _Tp>
struct __aligned_membuf
{
  unsigned char _M_storage[sizeof(_Tp)];
  _Tp bob[5];

  typedef _Tp ary[5];
  typedef const ary c_ary;
};
# 4 "./modules/tpl-ary-1_b.C" 2
import "tpl-ary-1_a.H";
