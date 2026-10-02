//type: fp
//options: 
# 0 "./opt/dtor4-aux.cc"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/dtor4-aux.cc"



# 1 "./opt/dtor4.h" 1
# 1 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/cassert" 1 3
# 41 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/cassert" 3
       
# 42 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/cassert" 3

# 1 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 296 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3

# 296 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;


#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() noexcept
  {
    void terminate() noexcept __attribute__ ((__noreturn__));
    terminate();
  }
#pragma GCC visibility pop
}
# 329 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 508 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)




  constexpr inline bool
  __is_constant_evaluated() noexcept
  {





    return __builtin_is_constant_evaluated();



  }
#pragma GCC visibility pop
}
# 655 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
# 1 "/usr/include/features.h" 1 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 40 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 656 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 659 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 841 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/pstl/pstl_config.h" 1 3
# 842 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 44 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/cassert" 2 3
# 1 "/usr/include/assert.h" 1 3 4
# 65 "/usr/include/assert.h" 3 4
extern "C" {


extern void __assert_fail (const char *__assertion, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));


extern void __assert_perror_fail (int __errnum, const char *__file,
      unsigned int __line, const char *__function)
     throw () __attribute__ ((__noreturn__));




extern void __assert (const char *__assertion, const char *__file, int __line)
     throw () __attribute__ ((__noreturn__));


}
# 45 "/mds/gnu/build/gcc-12.1.0/include/c++/12.1.0/cassert" 2 3
# 2 "./opt/dtor4.h" 2


# 3 "./opt/dtor4.h"
struct S
{
  int a, i;
  S () : i(1) {}
  __attribute__((noinline)) ~S () { 
# 7 "./opt/dtor4.h" 3 4
                                   ((
# 7 "./opt/dtor4.h"
                                   i == 1
# 7 "./opt/dtor4.h" 3 4
                                   ) ? static_cast<void> (0) : __assert_fail (
# 7 "./opt/dtor4.h"
                                   "i == 1"
# 7 "./opt/dtor4.h" 3 4
                                   , "./opt/dtor4.h", 7, __PRETTY_FUNCTION__))
# 7 "./opt/dtor4.h"
                                                  ; }
};
# 5 "./opt/dtor4-aux.cc" 2

S s;
