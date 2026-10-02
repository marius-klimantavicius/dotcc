//type: fp
//options: 
# 0 "./lto/20081119-1_1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20081119-1_1.C"
# 1 "./lto/20081119-1.h" 1
namespace __gnu_cxx
{
 template < typename _Tp > class new_allocator
 {
 public:
   unsigned max_size () const throw ();
 };
}
# 2 "./lto/20081119-1_1.C" 2

__gnu_cxx::new_allocator<int> X;

int
f (__gnu_cxx::new_allocator<int> a)
{
 return a.max_size ();
}
