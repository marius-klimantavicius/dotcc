//type: rp
//options:  --c++17 --c++11
# 0 "./ext/has_nothrow_constructor.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/has_nothrow_constructor.C"


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
# 4 "./ext/has_nothrow_constructor.C" 2


# 5 "./ext/has_nothrow_constructor.C"
struct A
{
  double a;
  double b;
};

struct B
{
  A a;
};







struct C
: public A { };

struct D
{
  D() throw() { }
};

struct E
{
  E() throw(int) { }
};

struct E1
{
  E1() throw(int) { throw int(); }
};

struct F
{
  F(const F&) throw() { }
};

struct G
{
  G(const G&) throw(int) { throw int(); }
};

template<typename T>
  bool
  f()
  { return __has_nothrow_constructor(T); }

template<typename T>
  class My
  {
  public:
    bool
    f()
    { return !!__has_nothrow_constructor(T); }
  };

template<typename T>
  class My2
  {
  public:
    static const bool trait = __has_nothrow_constructor(T);
  };

template<typename T>
  const bool My2<T>::trait;


template<typename T, bool b = __has_nothrow_constructor(T)>
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
  
# 99 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 99 "./ext/has_nothrow_constructor.C"
 (__has_nothrow_constructor(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())
# 99 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 99 "./ext/has_nothrow_constructor.C"
 "(__has_nothrow_constructor(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())"
# 99 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 99, __PRETTY_FUNCTION__))
# 99 "./ext/has_nothrow_constructor.C"
                     ;
  
# 100 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 100 "./ext/has_nothrow_constructor.C"
 (!__has_nothrow_constructor(int (int)) && !f<int (int)>() && !My<int (int)>().f() && !My2<int (int)>::trait && !My3<int (int)>().f())
# 100 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 100 "./ext/has_nothrow_constructor.C"
 "(!__has_nothrow_constructor(int (int)) && !f<int (int)>() && !My<int (int)>().f() && !My2<int (int)>::trait && !My3<int (int)>().f())"
# 100 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 100, __PRETTY_FUNCTION__))
# 100 "./ext/has_nothrow_constructor.C"
                           ;
  
# 101 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 101 "./ext/has_nothrow_constructor.C"
 (!__has_nothrow_constructor(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())
# 101 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 101 "./ext/has_nothrow_constructor.C"
 "(!__has_nothrow_constructor(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())"
# 101 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 101, __PRETTY_FUNCTION__))
# 101 "./ext/has_nothrow_constructor.C"
                      ;
  
# 102 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 102 "./ext/has_nothrow_constructor.C"
 (__has_nothrow_constructor(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())
# 102 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 102 "./ext/has_nothrow_constructor.C"
 "(__has_nothrow_constructor(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())"
# 102 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 102, __PRETTY_FUNCTION__))
# 102 "./ext/has_nothrow_constructor.C"
                   ;
  
# 103 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 103 "./ext/has_nothrow_constructor.C"
 (__has_nothrow_constructor(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())
# 103 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 103 "./ext/has_nothrow_constructor.C"
 "(__has_nothrow_constructor(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())"
# 103 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 103, __PRETTY_FUNCTION__))
# 103 "./ext/has_nothrow_constructor.C"
                   ;
  
# 104 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 104 "./ext/has_nothrow_constructor.C"
 (__has_nothrow_constructor(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())
# 104 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 104 "./ext/has_nothrow_constructor.C"
 "(__has_nothrow_constructor(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())"
# 104 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 104, __PRETTY_FUNCTION__))
# 104 "./ext/has_nothrow_constructor.C"
                   ;
  
# 105 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 105 "./ext/has_nothrow_constructor.C"
 (__has_nothrow_constructor(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())
# 105 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 105 "./ext/has_nothrow_constructor.C"
 "(__has_nothrow_constructor(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())"
# 105 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 105, __PRETTY_FUNCTION__))
# 105 "./ext/has_nothrow_constructor.C"
                     ;
  
# 106 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 106 "./ext/has_nothrow_constructor.C"
 (__has_nothrow_constructor(D) && f<D>() && My<D>().f() && My2<D>::trait && My3<D>().f())
# 106 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 106 "./ext/has_nothrow_constructor.C"
 "(__has_nothrow_constructor(D) && f<D>() && My<D>().f() && My2<D>::trait && My3<D>().f())"
# 106 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 106, __PRETTY_FUNCTION__))
# 106 "./ext/has_nothrow_constructor.C"
                   ;
  
# 107 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 107 "./ext/has_nothrow_constructor.C"
 (!__has_nothrow_constructor(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())
# 107 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 107 "./ext/has_nothrow_constructor.C"
 "(!__has_nothrow_constructor(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())"
# 107 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 107, __PRETTY_FUNCTION__))
# 107 "./ext/has_nothrow_constructor.C"
                   ;
  
# 108 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 108 "./ext/has_nothrow_constructor.C"
 (!__has_nothrow_constructor(E1) && !f<E1>() && !My<E1>().f() && !My2<E1>::trait && !My3<E1>().f())
# 108 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 108 "./ext/has_nothrow_constructor.C"
 "(!__has_nothrow_constructor(E1) && !f<E1>() && !My<E1>().f() && !My2<E1>::trait && !My3<E1>().f())"
# 108 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 108, __PRETTY_FUNCTION__))
# 108 "./ext/has_nothrow_constructor.C"
                    ;
  
# 109 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 109 "./ext/has_nothrow_constructor.C"
 (!__has_nothrow_constructor(F) && !f<F>() && !My<F>().f() && !My2<F>::trait && !My3<F>().f())
# 109 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 109 "./ext/has_nothrow_constructor.C"
 "(!__has_nothrow_constructor(F) && !f<F>() && !My<F>().f() && !My2<F>::trait && !My3<F>().f())"
# 109 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 109, __PRETTY_FUNCTION__))
# 109 "./ext/has_nothrow_constructor.C"
                   ;
  
# 110 "./ext/has_nothrow_constructor.C" 3 4
 ((
# 110 "./ext/has_nothrow_constructor.C"
 (!__has_nothrow_constructor(G) && !f<G>() && !My<G>().f() && !My2<G>::trait && !My3<G>().f())
# 110 "./ext/has_nothrow_constructor.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 110 "./ext/has_nothrow_constructor.C"
 "(!__has_nothrow_constructor(G) && !f<G>() && !My<G>().f() && !My2<G>::trait && !My3<G>().f())"
# 110 "./ext/has_nothrow_constructor.C" 3 4
 , "./ext/has_nothrow_constructor.C", 110, __PRETTY_FUNCTION__))
# 110 "./ext/has_nothrow_constructor.C"
                   ;

  return 0;
}
