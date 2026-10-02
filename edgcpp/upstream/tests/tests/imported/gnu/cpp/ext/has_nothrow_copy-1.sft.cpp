//type: rp
//options:  --c++17 --c++11
# 0 "./ext/has_nothrow_copy-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/has_nothrow_copy-1.C"


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
# 4 "./ext/has_nothrow_copy-1.C" 2


# 5 "./ext/has_nothrow_copy-1.C"
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
  D(const D&) throw() { }
};

struct E
{
  E(const E&) throw(int) { }
};

struct E1
{
  E1(const E1&) throw(int) { throw int(); }
};

struct F
{
  F() throw() { }
};

struct G
{
  G() throw(int) { throw int(); }
};

struct H
{
  H(H&) throw(int) { }
};

struct H1
{
  H1(H1&) throw(int) { throw int(); }
};

struct I
{
  I(I&) throw(int) { }
  I(const I&) throw() { }
};

struct I1
{
  I1(I1&) throw(int) { throw int(); }
  I1(const I1&) throw() { }
};

struct J
{
  J(J&) throw() { }
  J(const J&) throw() { }
  J(volatile J&) throw() { }
  J(const volatile J&) throw() { }
};

template<typename T>
  bool
  f()
  { return __has_nothrow_copy(T); }

template<typename T>
  class My
  {
  public:
    bool
    f()
    { return !!__has_nothrow_copy(T); }
  };

template<typename T>
  class My2
  {
  public:
    static const bool trait = __has_nothrow_copy(T);
  };

template<typename T>
  const bool My2<T>::trait;

template<typename T, bool b = __has_nothrow_copy(T)>
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
  
# 128 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 128 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())
# 128 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 128 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())"
# 128 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 128, __PRETTY_FUNCTION__))
# 128 "./ext/has_nothrow_copy-1.C"
                     ;
  
# 129 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 129 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(int (int)) && !f<int (int)>() && !My<int (int)>().f() && !My2<int (int)>::trait && !My3<int (int)>().f())
# 129 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 129 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(int (int)) && !f<int (int)>() && !My<int (int)>().f() && !My2<int (int)>::trait && !My3<int (int)>().f())"
# 129 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 129, __PRETTY_FUNCTION__))
# 129 "./ext/has_nothrow_copy-1.C"
                           ;
  
# 130 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 130 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())
# 130 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 130 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())"
# 130 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 130, __PRETTY_FUNCTION__))
# 130 "./ext/has_nothrow_copy-1.C"
                      ;
  
# 131 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 131 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())
# 131 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 131 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())"
# 131 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 131, __PRETTY_FUNCTION__))
# 131 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 132 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 132 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())
# 132 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 132 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())"
# 132 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 132, __PRETTY_FUNCTION__))
# 132 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 133 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 133 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())
# 133 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 133 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())"
# 133 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 133, __PRETTY_FUNCTION__))
# 133 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 134 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 134 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())
# 134 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 134 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())"
# 134 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 134, __PRETTY_FUNCTION__))
# 134 "./ext/has_nothrow_copy-1.C"
                     ;
  
# 135 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 135 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(D) && f<D>() && My<D>().f() && My2<D>::trait && My3<D>().f())
# 135 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 135 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(D) && f<D>() && My<D>().f() && My2<D>::trait && My3<D>().f())"
# 135 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 135, __PRETTY_FUNCTION__))
# 135 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 136 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 136 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())
# 136 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 136 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())"
# 136 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 136, __PRETTY_FUNCTION__))
# 136 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 137 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 137 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(E1) && !f<E1>() && !My<E1>().f() && !My2<E1>::trait && !My3<E1>().f())
# 137 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 137 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(E1) && !f<E1>() && !My<E1>().f() && !My2<E1>::trait && !My3<E1>().f())"
# 137 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 137, __PRETTY_FUNCTION__))
# 137 "./ext/has_nothrow_copy-1.C"
                    ;
  
# 138 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 138 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(F) && f<F>() && My<F>().f() && My2<F>::trait && My3<F>().f())
# 138 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 138 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(F) && f<F>() && My<F>().f() && My2<F>::trait && My3<F>().f())"
# 138 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 138, __PRETTY_FUNCTION__))
# 138 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 139 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 139 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(G) && f<G>() && My<G>().f() && My2<G>::trait && My3<G>().f())
# 139 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 139 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(G) && f<G>() && My<G>().f() && My2<G>::trait && My3<G>().f())"
# 139 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 139, __PRETTY_FUNCTION__))
# 139 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 140 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 140 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(H) && !f<H>() && !My<H>().f() && !My2<H>::trait && !My3<H>().f())
# 140 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 140 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(H) && !f<H>() && !My<H>().f() && !My2<H>::trait && !My3<H>().f())"
# 140 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 140, __PRETTY_FUNCTION__))
# 140 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 141 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 141 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(H1) && !f<H1>() && !My<H1>().f() && !My2<H1>::trait && !My3<H1>().f())
# 141 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 141 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(H1) && !f<H1>() && !My<H1>().f() && !My2<H1>::trait && !My3<H1>().f())"
# 141 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 141, __PRETTY_FUNCTION__))
# 141 "./ext/has_nothrow_copy-1.C"
                    ;
  
# 142 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 142 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(I) && !f<I>() && !My<I>().f() && !My2<I>::trait && !My3<I>().f())
# 142 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 142 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(I) && !f<I>() && !My<I>().f() && !My2<I>::trait && !My3<I>().f())"
# 142 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 142, __PRETTY_FUNCTION__))
# 142 "./ext/has_nothrow_copy-1.C"
                   ;
  
# 143 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 143 "./ext/has_nothrow_copy-1.C"
 (!__has_nothrow_copy(I1) && !f<I1>() && !My<I1>().f() && !My2<I1>::trait && !My3<I1>().f())
# 143 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 143 "./ext/has_nothrow_copy-1.C"
 "(!__has_nothrow_copy(I1) && !f<I1>() && !My<I1>().f() && !My2<I1>::trait && !My3<I1>().f())"
# 143 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 143, __PRETTY_FUNCTION__))
# 143 "./ext/has_nothrow_copy-1.C"
                    ;
  
# 144 "./ext/has_nothrow_copy-1.C" 3 4
 ((
# 144 "./ext/has_nothrow_copy-1.C"
 (__has_nothrow_copy(J) && f<J>() && My<J>().f() && My2<J>::trait && My3<J>().f())
# 144 "./ext/has_nothrow_copy-1.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 144 "./ext/has_nothrow_copy-1.C"
 "(__has_nothrow_copy(J) && f<J>() && My<J>().f() && My2<J>::trait && My3<J>().f())"
# 144 "./ext/has_nothrow_copy-1.C" 3 4
 , "./ext/has_nothrow_copy-1.C", 144, __PRETTY_FUNCTION__))
# 144 "./ext/has_nothrow_copy-1.C"
                   ;

  return 0;
}
