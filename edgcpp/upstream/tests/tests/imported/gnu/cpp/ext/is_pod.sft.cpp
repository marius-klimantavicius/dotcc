//type: rp
//options: --c++11
# 0 "./ext/is_pod.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/is_pod.C"


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"

#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wc++23-extensions"
# 328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  typedef long unsigned int size_t;
  typedef long int ptrdiff_t;


  typedef decltype(nullptr) nullptr_t;


#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() noexcept
  {
    void terminate() noexcept __attribute__ ((__noreturn__,__cold__));
    terminate();
  }
#pragma GCC visibility pop
}
# 361 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
namespace __gnu_cxx
{
  inline namespace __cxx11 __attribute__((__abi_tag__ ("cxx11"))) { }
}
# 565 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)




  __attribute__((__always_inline__))
  constexpr inline bool
  __is_constant_evaluated() noexcept
  {





    return __builtin_is_constant_evaluated();



  }
#pragma GCC visibility pop
}
# 609 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace std
{
#pragma GCC visibility push(default)

  extern "C++" __attribute__ ((__noreturn__)) __attribute__((__cold__))
  void
  __glibcxx_assert_fail
    (const char* __file, int __line, const char* __function,
     const char* __condition)
  noexcept;
#pragma GCC visibility pop
}
# 719 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 3
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
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/os_defines.h" 2 3
# 720 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/cpu_defines.h" 1 3
# 723 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3
# 879 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
namespace __gnu_cxx
{
  typedef __decltype(0.0bf16) __bfloat16_t;
}
# 945 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
#pragma GCC diagnostic pop
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
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
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 2 3
# 4 "./ext/is_pod.C" 2


# 5 "./ext/is_pod.C"
struct A
{
  double a;
  double b;
};

struct B
{
  B() { }
};

struct C
: public A { };

template<typename T>
  bool
  f()
  { return __is_pod(T); }

template<typename T>
  class My
  {
  public:
    bool
    f()
    { return !!__is_pod(T); }
  };

template<typename T>
  class My2
  {
  public:
    static const bool trait = __is_pod(T);
  };

template<typename T>
  const bool My2<T>::trait;

template<typename T, bool b = __is_pod(T)>
  struct My3_help
  { static const bool trait = b; };

template<typename T, bool b>
  const bool My3_help<T, b>::trait;

template<typename T>
  class My3
  {
  public:
    bool
    f()
    { return My3_help<T>::trait; }
  };







int main()
{
  
# 67 "./ext/is_pod.C" 3 4
 ((
# 67 "./ext/is_pod.C"
 (__is_pod(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())
# 67 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 67 "./ext/is_pod.C"
 "(__is_pod(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())"
# 67 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 67, __PRETTY_FUNCTION__))
# 67 "./ext/is_pod.C"
                     ;
  
# 68 "./ext/is_pod.C" 3 4
 ((
# 68 "./ext/is_pod.C"
 (!__is_pod(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())
# 68 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 68 "./ext/is_pod.C"
 "(!__is_pod(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())"
# 68 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 68, __PRETTY_FUNCTION__))
# 68 "./ext/is_pod.C"
                      ;
  
# 69 "./ext/is_pod.C" 3 4
 ((
# 69 "./ext/is_pod.C"
 (__is_pod(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())
# 69 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 69 "./ext/is_pod.C"
 "(__is_pod(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())"
# 69 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 69, __PRETTY_FUNCTION__))
# 69 "./ext/is_pod.C"
                   ;
  
# 70 "./ext/is_pod.C" 3 4
 ((
# 70 "./ext/is_pod.C"
 (__is_pod(A[]) && f<A[]>() && My<A[]>().f() && My2<A[]>::trait && My3<A[]>().f())
# 70 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 70 "./ext/is_pod.C"
 "(__is_pod(A[]) && f<A[]>() && My<A[]>().f() && My2<A[]>::trait && My3<A[]>().f())"
# 70 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 70, __PRETTY_FUNCTION__))
# 70 "./ext/is_pod.C"
                     ;
  
# 71 "./ext/is_pod.C" 3 4
 ((
# 71 "./ext/is_pod.C"
 (!__is_pod(B) && !f<B>() && !My<B>().f() && !My2<B>::trait && !My3<B>().f())
# 71 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 71 "./ext/is_pod.C"
 "(!__is_pod(B) && !f<B>() && !My<B>().f() && !My2<B>::trait && !My3<B>().f())"
# 71 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 71, __PRETTY_FUNCTION__))
# 71 "./ext/is_pod.C"
                   ;
  
# 72 "./ext/is_pod.C" 3 4
 ((
# 72 "./ext/is_pod.C"
 (__is_pod(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())
# 72 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 72 "./ext/is_pod.C"
 "(__is_pod(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())"
# 72 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 72, __PRETTY_FUNCTION__))
# 72 "./ext/is_pod.C"
                   ;
  
# 73 "./ext/is_pod.C" 3 4
 ((
# 73 "./ext/is_pod.C"
 (__is_pod(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())
# 73 "./ext/is_pod.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 73 "./ext/is_pod.C"
 "(__is_pod(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())"
# 73 "./ext/is_pod.C" 3 4
 , "./ext/is_pod.C", 73, __PRETTY_FUNCTION__))
# 73 "./ext/is_pod.C"
                     ;

  return 0;
}
