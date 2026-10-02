//type: s
//options: --c++17
# 1 "./concepts/explicit-spec5.C"
# 1 "<built-in>"
# 1 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 1 "<command-line>" 2
# 1 "./concepts/explicit-spec5.C"



# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 1 3
# 41 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 3
       
# 42 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 3

# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 252 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3

# 252 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;

}
# 274 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 524 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 525 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 528 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 690 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/pstl/pstl_config.h" 1 3
# 691 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 44 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 2 3
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
# 44 "/mds/gnu/build/gcc-9.3.0/include/c++/9.3.0/cassert" 2 3
# 5 "./concepts/explicit-spec5.C" 2


# 6 "./concepts/explicit-spec5.C"
template<typename T>
  concept bool C() { return __is_class(T); }

template<typename T>
  concept bool D() { return C<T>() && __is_empty(T); }

struct X { } x;
struct Y { int n; } y;

int called = 0;

template<typename T>
  struct S {
    void f() requires C<T>();
  };

template<> void S<int>::f() { called = 1; }
