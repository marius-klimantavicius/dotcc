//type: fp
//options: 
# 0 "./warn/anonymous-namespace-4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/anonymous-namespace-4.C"


# 1 "./warn/anonymous-namespace-4.h" 1
template < typename T > struct integral_c {
  static const T value = 0;
};
struct is_reference:integral_c < bool > { };
template < class > struct is_function_ptr_helper { };
template < bool > struct is_function_chooser;

template <> struct is_function_chooser <0 >
{
  template < typename T > struct result_:is_function_ptr_helper < T * > { };
};

template < typename T > struct is_function_impl:is_function_chooser <
  is_reference::value >::result_ < T > { };
# 4 "./warn/anonymous-namespace-4.C" 2

namespace
{
  class NonCloneable;
  void fn1 ()
  {
    is_function_impl < NonCloneable > i;
  }
}
