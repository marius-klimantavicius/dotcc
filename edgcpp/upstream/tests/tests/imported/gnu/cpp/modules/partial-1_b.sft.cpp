//type: fp
//options:  --c++20 --modules
# 0 "./modules/partial-1_b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./modules/partial-1_b.C"


# 1 "./modules/partial-1.h" 1

template<typename _Tp>
class allocator {};

template<typename _Alloc> struct allocator_traits;

template<typename _Tp>
struct allocator_traits <allocator<_Tp>>
{
  using pointer = _Tp*;
};

struct mutex {};

template<typename _Tp, typename _Alloc>
class Inplace
{
public:
  virtual void _M_dispose() noexcept
  {

    typename allocator_traits<_Alloc>::pointer v;
  }
};

inline void *
allocate_shared()
{
  return new Inplace<mutex, allocator<mutex>> ();
}
# 4 "./modules/partial-1_b.C" 2
import "partial-1_a.H";
