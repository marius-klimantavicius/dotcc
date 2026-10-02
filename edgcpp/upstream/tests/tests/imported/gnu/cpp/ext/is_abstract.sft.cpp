//type: rp
//options: 
# 0 "./ext/is_abstract.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/is_abstract.C"


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
# 4 "./ext/is_abstract.C" 2


# 5 "./ext/is_abstract.C"
struct A
{
  double a;
  double b;
};

union U
{
  double a;
  double b;
};

class B
{
  B();
};

class C
{
  virtual void rotate(int) = 0;
};

class D
{
  virtual void rotate(int) { }
};

template<typename T>
  bool
  f()
  { return __is_abstract(T); }

template<typename T>
  class My
  {
  public:
    bool
    f()
    { return !!__is_abstract(T); }
  };

template<typename T>
  class My2
  {
  public:
    static const bool trait = __is_abstract(T);
  };

template<typename T>
  const bool My2<T>::trait;

template<typename T, bool b = __is_abstract(T)>
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
  
# 80 "./ext/is_abstract.C" 3 4
 ((
# 80 "./ext/is_abstract.C"
 (!__is_abstract(int) && !f<int>() && !My<int>().f() && !My2<int>::trait && !My3<int>().f())
# 80 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 80 "./ext/is_abstract.C"
 "(!__is_abstract(int) && !f<int>() && !My<int>().f() && !My2<int>::trait && !My3<int>().f())"
# 80 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 80, __PRETTY_FUNCTION__))
# 80 "./ext/is_abstract.C"
                     ;
  
# 81 "./ext/is_abstract.C" 3 4
 ((
# 81 "./ext/is_abstract.C"
 (!__is_abstract(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())
# 81 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 81 "./ext/is_abstract.C"
 "(!__is_abstract(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())"
# 81 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 81, __PRETTY_FUNCTION__))
# 81 "./ext/is_abstract.C"
                      ;
  
# 82 "./ext/is_abstract.C" 3 4
 ((
# 82 "./ext/is_abstract.C"
 (!__is_abstract(A) && !f<A>() && !My<A>().f() && !My2<A>::trait && !My3<A>().f())
# 82 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 82 "./ext/is_abstract.C"
 "(!__is_abstract(A) && !f<A>() && !My<A>().f() && !My2<A>::trait && !My3<A>().f())"
# 82 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 82, __PRETTY_FUNCTION__))
# 82 "./ext/is_abstract.C"
                   ;
  
# 83 "./ext/is_abstract.C" 3 4
 ((
# 83 "./ext/is_abstract.C"
 (!__is_abstract(U) && !f<U>() && !My<U>().f() && !My2<U>::trait && !My3<U>().f())
# 83 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 83 "./ext/is_abstract.C"
 "(!__is_abstract(U) && !f<U>() && !My<U>().f() && !My2<U>::trait && !My3<U>().f())"
# 83 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 83, __PRETTY_FUNCTION__))
# 83 "./ext/is_abstract.C"
                   ;
  
# 84 "./ext/is_abstract.C" 3 4
 ((
# 84 "./ext/is_abstract.C"
 (!__is_abstract(B) && !f<B>() && !My<B>().f() && !My2<B>::trait && !My3<B>().f())
# 84 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 84 "./ext/is_abstract.C"
 "(!__is_abstract(B) && !f<B>() && !My<B>().f() && !My2<B>::trait && !My3<B>().f())"
# 84 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 84, __PRETTY_FUNCTION__))
# 84 "./ext/is_abstract.C"
                   ;
  
# 85 "./ext/is_abstract.C" 3 4
 ((
# 85 "./ext/is_abstract.C"
 (!__is_abstract(B[]) && !f<B[]>() && !My<B[]>().f() && !My2<B[]>::trait && !My3<B[]>().f())
# 85 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 85 "./ext/is_abstract.C"
 "(!__is_abstract(B[]) && !f<B[]>() && !My<B[]>().f() && !My2<B[]>::trait && !My3<B[]>().f())"
# 85 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 85, __PRETTY_FUNCTION__))
# 85 "./ext/is_abstract.C"
                     ;
  
# 86 "./ext/is_abstract.C" 3 4
 ((
# 86 "./ext/is_abstract.C"
 (__is_abstract(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())
# 86 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 86 "./ext/is_abstract.C"
 "(__is_abstract(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())"
# 86 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 86, __PRETTY_FUNCTION__))
# 86 "./ext/is_abstract.C"
                   ;
  
# 87 "./ext/is_abstract.C" 3 4
 ((
# 87 "./ext/is_abstract.C"
 (!__is_abstract(D) && !f<D>() && !My<D>().f() && !My2<D>::trait && !My3<D>().f())
# 87 "./ext/is_abstract.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 87 "./ext/is_abstract.C"
 "(!__is_abstract(D) && !f<D>() && !My<D>().f() && !My2<D>::trait && !My3<D>().f())"
# 87 "./ext/is_abstract.C" 3 4
 , "./ext/is_abstract.C", 87, __PRETTY_FUNCTION__))
# 87 "./ext/is_abstract.C"
                   ;

  return 0;
}
