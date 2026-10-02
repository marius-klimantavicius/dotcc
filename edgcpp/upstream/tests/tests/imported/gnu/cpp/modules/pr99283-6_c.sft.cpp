//type: fp
//options:  --c++20 --c++20 --modules
// { dg-additional-options {-std=c++2a -fmodules-ts} }

import  "pr99283-6_b.H";

template<typename _Alloc>
struct __allocated_ptr
{
  using value_type = allocator_traits<_Alloc>;
};

