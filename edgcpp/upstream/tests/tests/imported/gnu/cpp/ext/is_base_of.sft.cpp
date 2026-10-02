//type: rp
//options: 
# 0 "./ext/is_base_of.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/is_base_of.C"


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
# 941 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/pstl/pstl_config.h" 1 3
# 942 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++config.h" 2 3



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
# 4 "./ext/is_base_of.C" 2


# 5 "./ext/is_base_of.C"
class A1
{
  double a;
  double b;
};

class A2
{
  double a;
  double b;
};

class B
: private A1 { };

class C
: private A1, private A2 { };

union U
{
  double a;
  double b;
};

template<typename T, typename U>
  bool
  f()
  { return __is_base_of(T, U); }

template<typename T, typename U>
  class My
  {
  public:
    bool
    f()
    { return !!__is_base_of(T, U); }
  };

template<typename T, typename U>
  class My2
  {
  public:
    static const bool trait = __is_base_of(T, U);
  };

template<typename T, typename U>
  const bool My2<T, U>::trait;

template<typename T, typename U, bool b = __is_base_of(T, U)>
  struct My3_help
  { static const bool trait = b; };

template<typename T, typename U, bool b>
  const bool My3_help<T, U, b>::trait;

template<typename T, typename U>
  class My3
  {
  public:
    bool
    f()
    { return My3_help<T, U>::trait; }
  };







int main()
{
  
# 77 "./ext/is_base_of.C" 3 4
 ((
# 77 "./ext/is_base_of.C"
 (!__is_base_of(int, A1) && !f<int, A1>() && !My<int, A1>().f() && !My2<int, A1>::trait && !My3<int, A1>().f())
# 77 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 77 "./ext/is_base_of.C"
 "(!__is_base_of(int, A1) && !f<int, A1>() && !My<int, A1>().f() && !My2<int, A1>::trait && !My3<int, A1>().f())"
# 77 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 77, __PRETTY_FUNCTION__))
# 77 "./ext/is_base_of.C"
                         ;
  
# 78 "./ext/is_base_of.C" 3 4
 ((
# 78 "./ext/is_base_of.C"
 (!__is_base_of(A1, void) && !f<A1, void>() && !My<A1, void>().f() && !My2<A1, void>::trait && !My3<A1, void>().f())
# 78 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 78 "./ext/is_base_of.C"
 "(!__is_base_of(A1, void) && !f<A1, void>() && !My<A1, void>().f() && !My2<A1, void>::trait && !My3<A1, void>().f())"
# 78 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 78, __PRETTY_FUNCTION__))
# 78 "./ext/is_base_of.C"
                          ;
  
# 79 "./ext/is_base_of.C" 3 4
 ((
# 79 "./ext/is_base_of.C"
 (__is_base_of(A1, A1) && f<A1, A1>() && My<A1, A1>().f() && My2<A1, A1>::trait && My3<A1, A1>().f())
# 79 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 79 "./ext/is_base_of.C"
 "(__is_base_of(A1, A1) && f<A1, A1>() && My<A1, A1>().f() && My2<A1, A1>::trait && My3<A1, A1>().f())"
# 79 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 79, __PRETTY_FUNCTION__))
# 79 "./ext/is_base_of.C"
                        ;
  
# 80 "./ext/is_base_of.C" 3 4
 ((
# 80 "./ext/is_base_of.C"
 (!__is_base_of(A1*, A1*) && !f<A1*, A1*>() && !My<A1*, A1*>().f() && !My2<A1*, A1*>::trait && !My3<A1*, A1*>().f())
# 80 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 80 "./ext/is_base_of.C"
 "(!__is_base_of(A1*, A1*) && !f<A1*, A1*>() && !My<A1*, A1*>().f() && !My2<A1*, A1*>::trait && !My3<A1*, A1*>().f())"
# 80 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 80, __PRETTY_FUNCTION__))
# 80 "./ext/is_base_of.C"
                          ;
  
# 81 "./ext/is_base_of.C" 3 4
 ((
# 81 "./ext/is_base_of.C"
 (!__is_base_of(A1&, A1&) && !f<A1&, A1&>() && !My<A1&, A1&>().f() && !My2<A1&, A1&>::trait && !My3<A1&, A1&>().f())
# 81 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 81 "./ext/is_base_of.C"
 "(!__is_base_of(A1&, A1&) && !f<A1&, A1&>() && !My<A1&, A1&>().f() && !My2<A1&, A1&>::trait && !My3<A1&, A1&>().f())"
# 81 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 81, __PRETTY_FUNCTION__))
# 81 "./ext/is_base_of.C"
                          ;
  
# 82 "./ext/is_base_of.C" 3 4
 ((
# 82 "./ext/is_base_of.C"
 (__is_base_of(A1, B) && f<A1, B>() && My<A1, B>().f() && My2<A1, B>::trait && My3<A1, B>().f())
# 82 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 82 "./ext/is_base_of.C"
 "(__is_base_of(A1, B) && f<A1, B>() && My<A1, B>().f() && My2<A1, B>::trait && My3<A1, B>().f())"
# 82 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 82, __PRETTY_FUNCTION__))
# 82 "./ext/is_base_of.C"
                       ;
  
# 83 "./ext/is_base_of.C" 3 4
 ((
# 83 "./ext/is_base_of.C"
 (!__is_base_of(B, A1) && !f<B, A1>() && !My<B, A1>().f() && !My2<B, A1>::trait && !My3<B, A1>().f())
# 83 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 83 "./ext/is_base_of.C"
 "(!__is_base_of(B, A1) && !f<B, A1>() && !My<B, A1>().f() && !My2<B, A1>::trait && !My3<B, A1>().f())"
# 83 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 83, __PRETTY_FUNCTION__))
# 83 "./ext/is_base_of.C"
                       ;
  
# 84 "./ext/is_base_of.C" 3 4
 ((
# 84 "./ext/is_base_of.C"
 (__is_base_of(A1, C) && f<A1, C>() && My<A1, C>().f() && My2<A1, C>::trait && My3<A1, C>().f())
# 84 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 84 "./ext/is_base_of.C"
 "(__is_base_of(A1, C) && f<A1, C>() && My<A1, C>().f() && My2<A1, C>::trait && My3<A1, C>().f())"
# 84 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 84, __PRETTY_FUNCTION__))
# 84 "./ext/is_base_of.C"
                       ;
  
# 85 "./ext/is_base_of.C" 3 4
 ((
# 85 "./ext/is_base_of.C"
 (__is_base_of(A2, C) && f<A2, C>() && My<A2, C>().f() && My2<A2, C>::trait && My3<A2, C>().f())
# 85 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 85 "./ext/is_base_of.C"
 "(__is_base_of(A2, C) && f<A2, C>() && My<A2, C>().f() && My2<A2, C>::trait && My3<A2, C>().f())"
# 85 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 85, __PRETTY_FUNCTION__))
# 85 "./ext/is_base_of.C"
                       ;
  
# 86 "./ext/is_base_of.C" 3 4
 ((
# 86 "./ext/is_base_of.C"
 (!__is_base_of(C, A1) && !f<C, A1>() && !My<C, A1>().f() && !My2<C, A1>::trait && !My3<C, A1>().f())
# 86 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 86 "./ext/is_base_of.C"
 "(!__is_base_of(C, A1) && !f<C, A1>() && !My<C, A1>().f() && !My2<C, A1>::trait && !My3<C, A1>().f())"
# 86 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 86, __PRETTY_FUNCTION__))
# 86 "./ext/is_base_of.C"
                       ;
  
# 87 "./ext/is_base_of.C" 3 4
 ((
# 87 "./ext/is_base_of.C"
 (__is_base_of(A1, const B) && f<A1, const B>() && My<A1, const B>().f() && My2<A1, const B>::trait && My3<A1, const B>().f())
# 87 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 87 "./ext/is_base_of.C"
 "(__is_base_of(A1, const B) && f<A1, const B>() && My<A1, const B>().f() && My2<A1, const B>::trait && My3<A1, const B>().f())"
# 87 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 87, __PRETTY_FUNCTION__))
# 87 "./ext/is_base_of.C"
                             ;
  
# 88 "./ext/is_base_of.C" 3 4
 ((
# 88 "./ext/is_base_of.C"
 (!__is_base_of(const B, A1) && !f<const B, A1>() && !My<const B, A1>().f() && !My2<const B, A1>::trait && !My3<const B, A1>().f())
# 88 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 88 "./ext/is_base_of.C"
 "(!__is_base_of(const B, A1) && !f<const B, A1>() && !My<const B, A1>().f() && !My2<const B, A1>::trait && !My3<const B, A1>().f())"
# 88 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 88, __PRETTY_FUNCTION__))
# 88 "./ext/is_base_of.C"
                             ;
  
# 89 "./ext/is_base_of.C" 3 4
 ((
# 89 "./ext/is_base_of.C"
 (__is_base_of(A1, volatile C) && f<A1, volatile C>() && My<A1, volatile C>().f() && My2<A1, volatile C>::trait && My3<A1, volatile C>().f())
# 89 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 89 "./ext/is_base_of.C"
 "(__is_base_of(A1, volatile C) && f<A1, volatile C>() && My<A1, volatile C>().f() && My2<A1, volatile C>::trait && My3<A1, volatile C>().f())"
# 89 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 89, __PRETTY_FUNCTION__))
# 89 "./ext/is_base_of.C"
                                ;
  
# 90 "./ext/is_base_of.C" 3 4
 ((
# 90 "./ext/is_base_of.C"
 (__is_base_of(volatile A2, const C) && f<volatile A2, const C>() && My<volatile A2, const C>().f() && My2<volatile A2, const C>::trait && My3<volatile A2, const C>().f())
# 90 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 90 "./ext/is_base_of.C"
 "(__is_base_of(volatile A2, const C) && f<volatile A2, const C>() && My<volatile A2, const C>().f() && My2<volatile A2, const C>::trait && My3<volatile A2, const C>().f())"
# 90 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 90, __PRETTY_FUNCTION__))
# 90 "./ext/is_base_of.C"
                                      ;
  
# 91 "./ext/is_base_of.C" 3 4
 ((
# 91 "./ext/is_base_of.C"
 (!__is_base_of(const volatile C, A1) && !f<const volatile C, A1>() && !My<const volatile C, A1>().f() && !My2<const volatile C, A1>::trait && !My3<const volatile C, A1>().f())
# 91 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 91 "./ext/is_base_of.C"
 "(!__is_base_of(const volatile C, A1) && !f<const volatile C, A1>() && !My<const volatile C, A1>().f() && !My2<const volatile C, A1>::trait && !My3<const volatile C, A1>().f())"
# 91 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 91, __PRETTY_FUNCTION__))
# 91 "./ext/is_base_of.C"
                                      ;
  
# 92 "./ext/is_base_of.C" 3 4
 ((
# 92 "./ext/is_base_of.C"
 (!__is_base_of(U, U) && !f<U, U>() && !My<U, U>().f() && !My2<U, U>::trait && !My3<U, U>().f())
# 92 "./ext/is_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 92 "./ext/is_base_of.C"
 "(!__is_base_of(U, U) && !f<U, U>() && !My<U, U>().f() && !My2<U, U>::trait && !My3<U, U>().f())"
# 92 "./ext/is_base_of.C" 3 4
 , "./ext/is_base_of.C", 92, __PRETTY_FUNCTION__))
# 92 "./ext/is_base_of.C"
                      ;

  return 0;
}
