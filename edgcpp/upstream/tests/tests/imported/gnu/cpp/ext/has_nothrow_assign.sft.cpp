//type: rp
//options:  --c++17 --c++11
# 0 "./ext/has_nothrow_assign.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/has_nothrow_assign.C"


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
# 4 "./ext/has_nothrow_assign.C" 2


# 5 "./ext/has_nothrow_assign.C"
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
  D& operator=(const D&) throw() { return *this; }
};

struct E
{
  E& operator=(const E&) throw(int) { return *this; }
};

struct E1
{
  E1& operator=(const E1&) throw(int) { throw int(); return *this; }
};

struct F
{
  F() throw(int) { }
};

struct G
{
  G() throw(int) { throw int(); }
};

struct H
{
  H& operator=(H&) throw(int) { return *this; }
};

struct H1
{
  H1& operator=(H1&) throw(int) { throw int(); return *this; }
};

struct I
{
  I& operator=(I&) throw(int) { return *this; }
  I& operator=(const I&) throw() { return *this; }
};

struct I1
{
  I1& operator=(I1&) throw(int) { throw int(); return *this; }
  I1& operator=(const I1&) throw() { return *this; }
};

struct J
{
  J& operator=(J&) throw() { return *this; }
  J& operator=(const J&) throw() { return *this; }
  J& operator=(volatile J&) throw() { return *this; }
  J& operator=(const volatile J&) throw() { return *this; }
};

struct K
{
  K& operator=(K&) throw() { return *this; }
};

struct L
{
  L& operator=(const L&) throw() { return *this; }
};

template<typename T>
  bool
  f()
  { return __has_nothrow_assign(T); }

template<typename T>
  class My
  {
  public:
    bool
    f()
    { return !!__has_nothrow_assign(T); }
  };

template<typename T>
  class My2
  {
  public:
    static const bool trait = __has_nothrow_assign(T);
  };

template<typename T>
  const bool My2<T>::trait;

template<typename T, bool b = __has_nothrow_assign(T)>
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
  
# 138 "./ext/has_nothrow_assign.C" 3 4
 ((
# 138 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())
# 138 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 138 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(int) && f<int>() && My<int>().f() && My2<int>::trait && My3<int>().f())"
# 138 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 138, __PRETTY_FUNCTION__))
# 138 "./ext/has_nothrow_assign.C"
                     ;
  
# 139 "./ext/has_nothrow_assign.C" 3 4
 ((
# 139 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(int (int)) && !f<int (int)>() && !My<int (int)>().f() && !My2<int (int)>::trait && !My3<int (int)>().f())
# 139 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 139 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(int (int)) && !f<int (int)>() && !My<int (int)>().f() && !My2<int (int)>::trait && !My3<int (int)>().f())"
# 139 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 139, __PRETTY_FUNCTION__))
# 139 "./ext/has_nothrow_assign.C"
                           ;
  
# 140 "./ext/has_nothrow_assign.C" 3 4
 ((
# 140 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())
# 140 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 140 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())"
# 140 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 140, __PRETTY_FUNCTION__))
# 140 "./ext/has_nothrow_assign.C"
                      ;
  
# 141 "./ext/has_nothrow_assign.C" 3 4
 ((
# 141 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())
# 141 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 141 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())"
# 141 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 141, __PRETTY_FUNCTION__))
# 141 "./ext/has_nothrow_assign.C"
                   ;
  
# 142 "./ext/has_nothrow_assign.C" 3 4
 ((
# 142 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())
# 142 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 142 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())"
# 142 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 142, __PRETTY_FUNCTION__))
# 142 "./ext/has_nothrow_assign.C"
                   ;
  
# 143 "./ext/has_nothrow_assign.C" 3 4
 ((
# 143 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())
# 143 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 143 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(C) && f<C>() && My<C>().f() && My2<C>::trait && My3<C>().f())"
# 143 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 143, __PRETTY_FUNCTION__))
# 143 "./ext/has_nothrow_assign.C"
                   ;
  
# 144 "./ext/has_nothrow_assign.C" 3 4
 ((
# 144 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())
# 144 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 144 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())"
# 144 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 144, __PRETTY_FUNCTION__))
# 144 "./ext/has_nothrow_assign.C"
                     ;
  
# 145 "./ext/has_nothrow_assign.C" 3 4
 ((
# 145 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(D) && f<D>() && My<D>().f() && My2<D>::trait && My3<D>().f())
# 145 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 145 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(D) && f<D>() && My<D>().f() && My2<D>::trait && My3<D>().f())"
# 145 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 145, __PRETTY_FUNCTION__))
# 145 "./ext/has_nothrow_assign.C"
                   ;
  
# 146 "./ext/has_nothrow_assign.C" 3 4
 ((
# 146 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())
# 146 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 146 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())"
# 146 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 146, __PRETTY_FUNCTION__))
# 146 "./ext/has_nothrow_assign.C"
                   ;
  
# 147 "./ext/has_nothrow_assign.C" 3 4
 ((
# 147 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(E1) && !f<E1>() && !My<E1>().f() && !My2<E1>::trait && !My3<E1>().f())
# 147 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 147 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(E1) && !f<E1>() && !My<E1>().f() && !My2<E1>::trait && !My3<E1>().f())"
# 147 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 147, __PRETTY_FUNCTION__))
# 147 "./ext/has_nothrow_assign.C"
                    ;
  
# 148 "./ext/has_nothrow_assign.C" 3 4
 ((
# 148 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(F) && f<F>() && My<F>().f() && My2<F>::trait && My3<F>().f())
# 148 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 148 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(F) && f<F>() && My<F>().f() && My2<F>::trait && My3<F>().f())"
# 148 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 148, __PRETTY_FUNCTION__))
# 148 "./ext/has_nothrow_assign.C"
                   ;
  
# 149 "./ext/has_nothrow_assign.C" 3 4
 ((
# 149 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(G) && f<G>() && My<G>().f() && My2<G>::trait && My3<G>().f())
# 149 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 149 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(G) && f<G>() && My<G>().f() && My2<G>::trait && My3<G>().f())"
# 149 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 149, __PRETTY_FUNCTION__))
# 149 "./ext/has_nothrow_assign.C"
                   ;
  
# 150 "./ext/has_nothrow_assign.C" 3 4
 ((
# 150 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(H) && !f<H>() && !My<H>().f() && !My2<H>::trait && !My3<H>().f())
# 150 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 150 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(H) && !f<H>() && !My<H>().f() && !My2<H>::trait && !My3<H>().f())"
# 150 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 150, __PRETTY_FUNCTION__))
# 150 "./ext/has_nothrow_assign.C"
                   ;
  
# 151 "./ext/has_nothrow_assign.C" 3 4
 ((
# 151 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(H1) && !f<H1>() && !My<H1>().f() && !My2<H1>::trait && !My3<H1>().f())
# 151 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 151 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(H1) && !f<H1>() && !My<H1>().f() && !My2<H1>::trait && !My3<H1>().f())"
# 151 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 151, __PRETTY_FUNCTION__))
# 151 "./ext/has_nothrow_assign.C"
                    ;
  
# 152 "./ext/has_nothrow_assign.C" 3 4
 ((
# 152 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(I) && !f<I>() && !My<I>().f() && !My2<I>::trait && !My3<I>().f())
# 152 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 152 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(I) && !f<I>() && !My<I>().f() && !My2<I>::trait && !My3<I>().f())"
# 152 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 152, __PRETTY_FUNCTION__))
# 152 "./ext/has_nothrow_assign.C"
                   ;
  
# 153 "./ext/has_nothrow_assign.C" 3 4
 ((
# 153 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(I1) && !f<I1>() && !My<I1>().f() && !My2<I1>::trait && !My3<I1>().f())
# 153 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 153 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(I1) && !f<I1>() && !My<I1>().f() && !My2<I1>::trait && !My3<I1>().f())"
# 153 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 153, __PRETTY_FUNCTION__))
# 153 "./ext/has_nothrow_assign.C"
                    ;
  
# 154 "./ext/has_nothrow_assign.C" 3 4
 ((
# 154 "./ext/has_nothrow_assign.C"
 (__has_nothrow_assign(J) && f<J>() && My<J>().f() && My2<J>::trait && My3<J>().f())
# 154 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 154 "./ext/has_nothrow_assign.C"
 "(__has_nothrow_assign(J) && f<J>() && My<J>().f() && My2<J>::trait && My3<J>().f())"
# 154 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 154, __PRETTY_FUNCTION__))
# 154 "./ext/has_nothrow_assign.C"
                   ;
  
# 155 "./ext/has_nothrow_assign.C" 3 4
 ((
# 155 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(const K) && !f<const K>() && !My<const K>().f() && !My2<const K>::trait && !My3<const K>().f())
# 155 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 155 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(const K) && !f<const K>() && !My<const K>().f() && !My2<const K>::trait && !My3<const K>().f())"
# 155 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 155, __PRETTY_FUNCTION__))
# 155 "./ext/has_nothrow_assign.C"
                         ;
  
# 156 "./ext/has_nothrow_assign.C" 3 4
 ((
# 156 "./ext/has_nothrow_assign.C"
 (!__has_nothrow_assign(const L) && !f<const L>() && !My<const L>().f() && !My2<const L>::trait && !My3<const L>().f())
# 156 "./ext/has_nothrow_assign.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 156 "./ext/has_nothrow_assign.C"
 "(!__has_nothrow_assign(const L) && !f<const L>() && !My<const L>().f() && !My2<const L>::trait && !My3<const L>().f())"
# 156 "./ext/has_nothrow_assign.C" 3 4
 , "./ext/has_nothrow_assign.C", 156, __PRETTY_FUNCTION__))
# 156 "./ext/has_nothrow_assign.C"
                         ;

  return 0;
}
