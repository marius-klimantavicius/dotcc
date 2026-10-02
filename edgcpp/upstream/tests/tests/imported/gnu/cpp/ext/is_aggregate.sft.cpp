//type: rp
//options: --c++11
# 0 "./ext/is_aggregate.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/is_aggregate.C"


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
# 4 "./ext/is_aggregate.C" 2


# 5 "./ext/is_aggregate.C"
template<typename T>
  bool
  f()
  { return __is_aggregate(T); }

template<typename T>
  class My
  {
  public:
    bool
    f()
    { return !!__is_aggregate(T); }
  };

template<typename T>
  class My2
  {
  public:
    static const bool trait = __is_aggregate(T);
  };

template<typename T>
  const bool My2<T>::trait;

template<typename T, bool b = __is_aggregate(T)>
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







struct A { int a, b, c; };
class B { static int a; private: static int b; public: int c; };
struct C { C () {} int a, b, c; };
struct D { explicit D (int) {} int a, b, c; };
struct E : public A { int d, e, f; };
struct F : public C { using C::C; int d, e, f; };
class G { int a, b; };
struct H { private: int a, b; };
struct I { protected: int a, b; };
struct J { int a, b; void foo (); };
struct K { int a, b; virtual void foo (); };
struct L : virtual public A { int d, e; };
struct M : protected A { int d, e; };
struct N : private A { int d, e; };
struct O { O () = delete; int a, b, c; };
struct P { P () = default; int a, b, c; };
typedef int T;
typedef float U;
typedef int V __attribute__((vector_size (4 * sizeof (int))));
typedef double W __attribute__((vector_size (8 * sizeof (double))));

int
main ()
{
  
# 75 "./ext/is_aggregate.C" 3 4
 ((
# 75 "./ext/is_aggregate.C"
 (!__is_aggregate(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())
# 75 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 75 "./ext/is_aggregate.C"
 "(!__is_aggregate(void) && !f<void>() && !My<void>().f() && !My2<void>::trait && !My3<void>().f())"
# 75 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 75, __PRETTY_FUNCTION__))
# 75 "./ext/is_aggregate.C"
                      ;
  
# 76 "./ext/is_aggregate.C" 3 4
 ((
# 76 "./ext/is_aggregate.C"
 (!__is_aggregate(int) && !f<int>() && !My<int>().f() && !My2<int>::trait && !My3<int>().f())
# 76 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 76 "./ext/is_aggregate.C"
 "(!__is_aggregate(int) && !f<int>() && !My<int>().f() && !My2<int>::trait && !My3<int>().f())"
# 76 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 76, __PRETTY_FUNCTION__))
# 76 "./ext/is_aggregate.C"
                     ;
  
# 77 "./ext/is_aggregate.C" 3 4
 ((
# 77 "./ext/is_aggregate.C"
 (!__is_aggregate(double) && !f<double>() && !My<double>().f() && !My2<double>::trait && !My3<double>().f())
# 77 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 77 "./ext/is_aggregate.C"
 "(!__is_aggregate(double) && !f<double>() && !My<double>().f() && !My2<double>::trait && !My3<double>().f())"
# 77 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 77, __PRETTY_FUNCTION__))
# 77 "./ext/is_aggregate.C"
                        ;
  
# 78 "./ext/is_aggregate.C" 3 4
 ((
# 78 "./ext/is_aggregate.C"
 (!__is_aggregate(T) && !f<T>() && !My<T>().f() && !My2<T>::trait && !My3<T>().f())
# 78 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 78 "./ext/is_aggregate.C"
 "(!__is_aggregate(T) && !f<T>() && !My<T>().f() && !My2<T>::trait && !My3<T>().f())"
# 78 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 78, __PRETTY_FUNCTION__))
# 78 "./ext/is_aggregate.C"
                   ;
  
# 79 "./ext/is_aggregate.C" 3 4
 ((
# 79 "./ext/is_aggregate.C"
 (!__is_aggregate(U) && !f<U>() && !My<U>().f() && !My2<U>::trait && !My3<U>().f())
# 79 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 79 "./ext/is_aggregate.C"
 "(!__is_aggregate(U) && !f<U>() && !My<U>().f() && !My2<U>::trait && !My3<U>().f())"
# 79 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 79, __PRETTY_FUNCTION__))
# 79 "./ext/is_aggregate.C"
                   ;
  
# 80 "./ext/is_aggregate.C" 3 4
 ((
# 80 "./ext/is_aggregate.C"
 (__is_aggregate(V) && f<V>() && My<V>().f() && My2<V>::trait && My3<V>().f())
# 80 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 80 "./ext/is_aggregate.C"
 "(__is_aggregate(V) && f<V>() && My<V>().f() && My2<V>::trait && My3<V>().f())"
# 80 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 80, __PRETTY_FUNCTION__))
# 80 "./ext/is_aggregate.C"
                   ;
  
# 81 "./ext/is_aggregate.C" 3 4
 ((
# 81 "./ext/is_aggregate.C"
 (__is_aggregate(W) && f<W>() && My<W>().f() && My2<W>::trait && My3<W>().f())
# 81 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 81 "./ext/is_aggregate.C"
 "(__is_aggregate(W) && f<W>() && My<W>().f() && My2<W>::trait && My3<W>().f())"
# 81 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 81, __PRETTY_FUNCTION__))
# 81 "./ext/is_aggregate.C"
                   ;
  
# 82 "./ext/is_aggregate.C" 3 4
 ((
# 82 "./ext/is_aggregate.C"
 (__is_aggregate(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())
# 82 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 82 "./ext/is_aggregate.C"
 "(__is_aggregate(A) && f<A>() && My<A>().f() && My2<A>::trait && My3<A>().f())"
# 82 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 82, __PRETTY_FUNCTION__))
# 82 "./ext/is_aggregate.C"
                   ;
  
# 83 "./ext/is_aggregate.C" 3 4
 ((
# 83 "./ext/is_aggregate.C"
 (__is_aggregate(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())
# 83 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 83 "./ext/is_aggregate.C"
 "(__is_aggregate(B) && f<B>() && My<B>().f() && My2<B>::trait && My3<B>().f())"
# 83 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 83, __PRETTY_FUNCTION__))
# 83 "./ext/is_aggregate.C"
                   ;
  
# 84 "./ext/is_aggregate.C" 3 4
 ((
# 84 "./ext/is_aggregate.C"
 (!__is_aggregate(C) && !f<C>() && !My<C>().f() && !My2<C>::trait && !My3<C>().f())
# 84 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 84 "./ext/is_aggregate.C"
 "(!__is_aggregate(C) && !f<C>() && !My<C>().f() && !My2<C>::trait && !My3<C>().f())"
# 84 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 84, __PRETTY_FUNCTION__))
# 84 "./ext/is_aggregate.C"
                   ;
  
# 85 "./ext/is_aggregate.C" 3 4
 ((
# 85 "./ext/is_aggregate.C"
 (!__is_aggregate(D) && !f<D>() && !My<D>().f() && !My2<D>::trait && !My3<D>().f())
# 85 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 85 "./ext/is_aggregate.C"
 "(!__is_aggregate(D) && !f<D>() && !My<D>().f() && !My2<D>::trait && !My3<D>().f())"
# 85 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 85, __PRETTY_FUNCTION__))
# 85 "./ext/is_aggregate.C"
                   ;



  
# 89 "./ext/is_aggregate.C" 3 4
 ((
# 89 "./ext/is_aggregate.C"
 (!__is_aggregate(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())
# 89 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 89 "./ext/is_aggregate.C"
 "(!__is_aggregate(E) && !f<E>() && !My<E>().f() && !My2<E>::trait && !My3<E>().f())"
# 89 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 89, __PRETTY_FUNCTION__))
# 89 "./ext/is_aggregate.C"
                   ;

  
# 91 "./ext/is_aggregate.C" 3 4
 ((
# 91 "./ext/is_aggregate.C"
 (!__is_aggregate(F) && !f<F>() && !My<F>().f() && !My2<F>::trait && !My3<F>().f())
# 91 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 91 "./ext/is_aggregate.C"
 "(!__is_aggregate(F) && !f<F>() && !My<F>().f() && !My2<F>::trait && !My3<F>().f())"
# 91 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 91, __PRETTY_FUNCTION__))
# 91 "./ext/is_aggregate.C"
                   ;
  
# 92 "./ext/is_aggregate.C" 3 4
 ((
# 92 "./ext/is_aggregate.C"
 (!__is_aggregate(G) && !f<G>() && !My<G>().f() && !My2<G>::trait && !My3<G>().f())
# 92 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 92 "./ext/is_aggregate.C"
 "(!__is_aggregate(G) && !f<G>() && !My<G>().f() && !My2<G>::trait && !My3<G>().f())"
# 92 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 92, __PRETTY_FUNCTION__))
# 92 "./ext/is_aggregate.C"
                   ;
  
# 93 "./ext/is_aggregate.C" 3 4
 ((
# 93 "./ext/is_aggregate.C"
 (!__is_aggregate(H) && !f<H>() && !My<H>().f() && !My2<H>::trait && !My3<H>().f())
# 93 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 93 "./ext/is_aggregate.C"
 "(!__is_aggregate(H) && !f<H>() && !My<H>().f() && !My2<H>::trait && !My3<H>().f())"
# 93 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 93, __PRETTY_FUNCTION__))
# 93 "./ext/is_aggregate.C"
                   ;
  
# 94 "./ext/is_aggregate.C" 3 4
 ((
# 94 "./ext/is_aggregate.C"
 (!__is_aggregate(I) && !f<I>() && !My<I>().f() && !My2<I>::trait && !My3<I>().f())
# 94 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 94 "./ext/is_aggregate.C"
 "(!__is_aggregate(I) && !f<I>() && !My<I>().f() && !My2<I>::trait && !My3<I>().f())"
# 94 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 94, __PRETTY_FUNCTION__))
# 94 "./ext/is_aggregate.C"
                   ;
  
# 95 "./ext/is_aggregate.C" 3 4
 ((
# 95 "./ext/is_aggregate.C"
 (__is_aggregate(J) && f<J>() && My<J>().f() && My2<J>::trait && My3<J>().f())
# 95 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 95 "./ext/is_aggregate.C"
 "(__is_aggregate(J) && f<J>() && My<J>().f() && My2<J>::trait && My3<J>().f())"
# 95 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 95, __PRETTY_FUNCTION__))
# 95 "./ext/is_aggregate.C"
                   ;
  
# 96 "./ext/is_aggregate.C" 3 4
 ((
# 96 "./ext/is_aggregate.C"
 (!__is_aggregate(K) && !f<K>() && !My<K>().f() && !My2<K>::trait && !My3<K>().f())
# 96 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 96 "./ext/is_aggregate.C"
 "(!__is_aggregate(K) && !f<K>() && !My<K>().f() && !My2<K>::trait && !My3<K>().f())"
# 96 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 96, __PRETTY_FUNCTION__))
# 96 "./ext/is_aggregate.C"
                   ;
  
# 97 "./ext/is_aggregate.C" 3 4
 ((
# 97 "./ext/is_aggregate.C"
 (!__is_aggregate(L) && !f<L>() && !My<L>().f() && !My2<L>::trait && !My3<L>().f())
# 97 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 97 "./ext/is_aggregate.C"
 "(!__is_aggregate(L) && !f<L>() && !My<L>().f() && !My2<L>::trait && !My3<L>().f())"
# 97 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 97, __PRETTY_FUNCTION__))
# 97 "./ext/is_aggregate.C"
                   ;
  
# 98 "./ext/is_aggregate.C" 3 4
 ((
# 98 "./ext/is_aggregate.C"
 (!__is_aggregate(M) && !f<M>() && !My<M>().f() && !My2<M>::trait && !My3<M>().f())
# 98 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 98 "./ext/is_aggregate.C"
 "(!__is_aggregate(M) && !f<M>() && !My<M>().f() && !My2<M>::trait && !My3<M>().f())"
# 98 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 98, __PRETTY_FUNCTION__))
# 98 "./ext/is_aggregate.C"
                   ;
  
# 99 "./ext/is_aggregate.C" 3 4
 ((
# 99 "./ext/is_aggregate.C"
 (!__is_aggregate(N) && !f<N>() && !My<N>().f() && !My2<N>::trait && !My3<N>().f())
# 99 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 99 "./ext/is_aggregate.C"
 "(!__is_aggregate(N) && !f<N>() && !My<N>().f() && !My2<N>::trait && !My3<N>().f())"
# 99 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 99, __PRETTY_FUNCTION__))
# 99 "./ext/is_aggregate.C"
                   ;




  
# 104 "./ext/is_aggregate.C" 3 4
 ((
# 104 "./ext/is_aggregate.C"
 (__is_aggregate(O) && f<O>() && My<O>().f() && My2<O>::trait && My3<O>().f())
# 104 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 104 "./ext/is_aggregate.C"
 "(__is_aggregate(O) && f<O>() && My<O>().f() && My2<O>::trait && My3<O>().f())"
# 104 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 104, __PRETTY_FUNCTION__))
# 104 "./ext/is_aggregate.C"
                   ;
  
# 105 "./ext/is_aggregate.C" 3 4
 ((
# 105 "./ext/is_aggregate.C"
 (__is_aggregate(P) && f<P>() && My<P>().f() && My2<P>::trait && My3<P>().f())
# 105 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 105 "./ext/is_aggregate.C"
 "(__is_aggregate(P) && f<P>() && My<P>().f() && My2<P>::trait && My3<P>().f())"
# 105 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 105, __PRETTY_FUNCTION__))
# 105 "./ext/is_aggregate.C"
                   ;

  
# 107 "./ext/is_aggregate.C" 3 4
 ((
# 107 "./ext/is_aggregate.C"
 (__is_aggregate(int[]) && f<int[]>() && My<int[]>().f() && My2<int[]>::trait && My3<int[]>().f())
# 107 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 107 "./ext/is_aggregate.C"
 "(__is_aggregate(int[]) && f<int[]>() && My<int[]>().f() && My2<int[]>::trait && My3<int[]>().f())"
# 107 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 107, __PRETTY_FUNCTION__))
# 107 "./ext/is_aggregate.C"
                       ;
  
# 108 "./ext/is_aggregate.C" 3 4
 ((
# 108 "./ext/is_aggregate.C"
 (__is_aggregate(double[]) && f<double[]>() && My<double[]>().f() && My2<double[]>::trait && My3<double[]>().f())
# 108 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 108 "./ext/is_aggregate.C"
 "(__is_aggregate(double[]) && f<double[]>() && My<double[]>().f() && My2<double[]>::trait && My3<double[]>().f())"
# 108 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 108, __PRETTY_FUNCTION__))
# 108 "./ext/is_aggregate.C"
                          ;
  
# 109 "./ext/is_aggregate.C" 3 4
 ((
# 109 "./ext/is_aggregate.C"
 (__is_aggregate(T[2]) && f<T[2]>() && My<T[2]>().f() && My2<T[2]>::trait && My3<T[2]>().f())
# 109 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 109 "./ext/is_aggregate.C"
 "(__is_aggregate(T[2]) && f<T[2]>() && My<T[2]>().f() && My2<T[2]>::trait && My3<T[2]>().f())"
# 109 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 109, __PRETTY_FUNCTION__))
# 109 "./ext/is_aggregate.C"
                      ;
  
# 110 "./ext/is_aggregate.C" 3 4
 ((
# 110 "./ext/is_aggregate.C"
 (__is_aggregate(U[]) && f<U[]>() && My<U[]>().f() && My2<U[]>::trait && My3<U[]>().f())
# 110 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 110 "./ext/is_aggregate.C"
 "(__is_aggregate(U[]) && f<U[]>() && My<U[]>().f() && My2<U[]>::trait && My3<U[]>().f())"
# 110 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 110, __PRETTY_FUNCTION__))
# 110 "./ext/is_aggregate.C"
                     ;
  
# 111 "./ext/is_aggregate.C" 3 4
 ((
# 111 "./ext/is_aggregate.C"
 (__is_aggregate(V[]) && f<V[]>() && My<V[]>().f() && My2<V[]>::trait && My3<V[]>().f())
# 111 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 111 "./ext/is_aggregate.C"
 "(__is_aggregate(V[]) && f<V[]>() && My<V[]>().f() && My2<V[]>::trait && My3<V[]>().f())"
# 111 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 111, __PRETTY_FUNCTION__))
# 111 "./ext/is_aggregate.C"
                     ;
  
# 112 "./ext/is_aggregate.C" 3 4
 ((
# 112 "./ext/is_aggregate.C"
 (__is_aggregate(W[]) && f<W[]>() && My<W[]>().f() && My2<W[]>::trait && My3<W[]>().f())
# 112 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 112 "./ext/is_aggregate.C"
 "(__is_aggregate(W[]) && f<W[]>() && My<W[]>().f() && My2<W[]>::trait && My3<W[]>().f())"
# 112 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 112, __PRETTY_FUNCTION__))
# 112 "./ext/is_aggregate.C"
                     ;
  
# 113 "./ext/is_aggregate.C" 3 4
 ((
# 113 "./ext/is_aggregate.C"
 (__is_aggregate(A[19]) && f<A[19]>() && My<A[19]>().f() && My2<A[19]>::trait && My3<A[19]>().f())
# 113 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 113 "./ext/is_aggregate.C"
 "(__is_aggregate(A[19]) && f<A[19]>() && My<A[19]>().f() && My2<A[19]>::trait && My3<A[19]>().f())"
# 113 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 113, __PRETTY_FUNCTION__))
# 113 "./ext/is_aggregate.C"
                       ;
  
# 114 "./ext/is_aggregate.C" 3 4
 ((
# 114 "./ext/is_aggregate.C"
 (__is_aggregate(B[]) && f<B[]>() && My<B[]>().f() && My2<B[]>::trait && My3<B[]>().f())
# 114 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 114 "./ext/is_aggregate.C"
 "(__is_aggregate(B[]) && f<B[]>() && My<B[]>().f() && My2<B[]>::trait && My3<B[]>().f())"
# 114 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 114, __PRETTY_FUNCTION__))
# 114 "./ext/is_aggregate.C"
                     ;
  
# 115 "./ext/is_aggregate.C" 3 4
 ((
# 115 "./ext/is_aggregate.C"
 (__is_aggregate(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())
# 115 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 115 "./ext/is_aggregate.C"
 "(__is_aggregate(C[]) && f<C[]>() && My<C[]>().f() && My2<C[]>::trait && My3<C[]>().f())"
# 115 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 115, __PRETTY_FUNCTION__))
# 115 "./ext/is_aggregate.C"
                     ;
  
# 116 "./ext/is_aggregate.C" 3 4
 ((
# 116 "./ext/is_aggregate.C"
 (__is_aggregate(D[]) && f<D[]>() && My<D[]>().f() && My2<D[]>::trait && My3<D[]>().f())
# 116 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 116 "./ext/is_aggregate.C"
 "(__is_aggregate(D[]) && f<D[]>() && My<D[]>().f() && My2<D[]>::trait && My3<D[]>().f())"
# 116 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 116, __PRETTY_FUNCTION__))
# 116 "./ext/is_aggregate.C"
                     ;
  
# 117 "./ext/is_aggregate.C" 3 4
 ((
# 117 "./ext/is_aggregate.C"
 (__is_aggregate(E[]) && f<E[]>() && My<E[]>().f() && My2<E[]>::trait && My3<E[]>().f())
# 117 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 117 "./ext/is_aggregate.C"
 "(__is_aggregate(E[]) && f<E[]>() && My<E[]>().f() && My2<E[]>::trait && My3<E[]>().f())"
# 117 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 117, __PRETTY_FUNCTION__))
# 117 "./ext/is_aggregate.C"
                     ;
  
# 118 "./ext/is_aggregate.C" 3 4
 ((
# 118 "./ext/is_aggregate.C"
 (__is_aggregate(F[]) && f<F[]>() && My<F[]>().f() && My2<F[]>::trait && My3<F[]>().f())
# 118 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 118 "./ext/is_aggregate.C"
 "(__is_aggregate(F[]) && f<F[]>() && My<F[]>().f() && My2<F[]>::trait && My3<F[]>().f())"
# 118 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 118, __PRETTY_FUNCTION__))
# 118 "./ext/is_aggregate.C"
                     ;
  
# 119 "./ext/is_aggregate.C" 3 4
 ((
# 119 "./ext/is_aggregate.C"
 (__is_aggregate(G[]) && f<G[]>() && My<G[]>().f() && My2<G[]>::trait && My3<G[]>().f())
# 119 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 119 "./ext/is_aggregate.C"
 "(__is_aggregate(G[]) && f<G[]>() && My<G[]>().f() && My2<G[]>::trait && My3<G[]>().f())"
# 119 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 119, __PRETTY_FUNCTION__))
# 119 "./ext/is_aggregate.C"
                     ;
  
# 120 "./ext/is_aggregate.C" 3 4
 ((
# 120 "./ext/is_aggregate.C"
 (__is_aggregate(H[]) && f<H[]>() && My<H[]>().f() && My2<H[]>::trait && My3<H[]>().f())
# 120 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 120 "./ext/is_aggregate.C"
 "(__is_aggregate(H[]) && f<H[]>() && My<H[]>().f() && My2<H[]>::trait && My3<H[]>().f())"
# 120 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 120, __PRETTY_FUNCTION__))
# 120 "./ext/is_aggregate.C"
                     ;
  
# 121 "./ext/is_aggregate.C" 3 4
 ((
# 121 "./ext/is_aggregate.C"
 (__is_aggregate(I[]) && f<I[]>() && My<I[]>().f() && My2<I[]>::trait && My3<I[]>().f())
# 121 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 121 "./ext/is_aggregate.C"
 "(__is_aggregate(I[]) && f<I[]>() && My<I[]>().f() && My2<I[]>::trait && My3<I[]>().f())"
# 121 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 121, __PRETTY_FUNCTION__))
# 121 "./ext/is_aggregate.C"
                     ;
  
# 122 "./ext/is_aggregate.C" 3 4
 ((
# 122 "./ext/is_aggregate.C"
 (__is_aggregate(J[24]) && f<J[24]>() && My<J[24]>().f() && My2<J[24]>::trait && My3<J[24]>().f())
# 122 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 122 "./ext/is_aggregate.C"
 "(__is_aggregate(J[24]) && f<J[24]>() && My<J[24]>().f() && My2<J[24]>::trait && My3<J[24]>().f())"
# 122 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 122, __PRETTY_FUNCTION__))
# 122 "./ext/is_aggregate.C"
                       ;
  
# 123 "./ext/is_aggregate.C" 3 4
 ((
# 123 "./ext/is_aggregate.C"
 (__is_aggregate(K[]) && f<K[]>() && My<K[]>().f() && My2<K[]>::trait && My3<K[]>().f())
# 123 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 123 "./ext/is_aggregate.C"
 "(__is_aggregate(K[]) && f<K[]>() && My<K[]>().f() && My2<K[]>::trait && My3<K[]>().f())"
# 123 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 123, __PRETTY_FUNCTION__))
# 123 "./ext/is_aggregate.C"
                     ;
  
# 124 "./ext/is_aggregate.C" 3 4
 ((
# 124 "./ext/is_aggregate.C"
 (__is_aggregate(L[]) && f<L[]>() && My<L[]>().f() && My2<L[]>::trait && My3<L[]>().f())
# 124 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 124 "./ext/is_aggregate.C"
 "(__is_aggregate(L[]) && f<L[]>() && My<L[]>().f() && My2<L[]>::trait && My3<L[]>().f())"
# 124 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 124, __PRETTY_FUNCTION__))
# 124 "./ext/is_aggregate.C"
                     ;
  
# 125 "./ext/is_aggregate.C" 3 4
 ((
# 125 "./ext/is_aggregate.C"
 (__is_aggregate(M[6]) && f<M[6]>() && My<M[6]>().f() && My2<M[6]>::trait && My3<M[6]>().f())
# 125 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 125 "./ext/is_aggregate.C"
 "(__is_aggregate(M[6]) && f<M[6]>() && My<M[6]>().f() && My2<M[6]>::trait && My3<M[6]>().f())"
# 125 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 125, __PRETTY_FUNCTION__))
# 125 "./ext/is_aggregate.C"
                      ;
  
# 126 "./ext/is_aggregate.C" 3 4
 ((
# 126 "./ext/is_aggregate.C"
 (__is_aggregate(N[]) && f<N[]>() && My<N[]>().f() && My2<N[]>::trait && My3<N[]>().f())
# 126 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 126 "./ext/is_aggregate.C"
 "(__is_aggregate(N[]) && f<N[]>() && My<N[]>().f() && My2<N[]>::trait && My3<N[]>().f())"
# 126 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 126, __PRETTY_FUNCTION__))
# 126 "./ext/is_aggregate.C"
                     ;
  
# 127 "./ext/is_aggregate.C" 3 4
 ((
# 127 "./ext/is_aggregate.C"
 (__is_aggregate(O[]) && f<O[]>() && My<O[]>().f() && My2<O[]>::trait && My3<O[]>().f())
# 127 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 127 "./ext/is_aggregate.C"
 "(__is_aggregate(O[]) && f<O[]>() && My<O[]>().f() && My2<O[]>::trait && My3<O[]>().f())"
# 127 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 127, __PRETTY_FUNCTION__))
# 127 "./ext/is_aggregate.C"
                     ;
  
# 128 "./ext/is_aggregate.C" 3 4
 ((
# 128 "./ext/is_aggregate.C"
 (__is_aggregate(P[]) && f<P[]>() && My<P[]>().f() && My2<P[]>::trait && My3<P[]>().f())
# 128 "./ext/is_aggregate.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 128 "./ext/is_aggregate.C"
 "(__is_aggregate(P[]) && f<P[]>() && My<P[]>().f() && My2<P[]>::trait && My3<P[]>().f())"
# 128 "./ext/is_aggregate.C" 3 4
 , "./ext/is_aggregate.C", 128, __PRETTY_FUNCTION__))
# 128 "./ext/is_aggregate.C"
                     ;
}
