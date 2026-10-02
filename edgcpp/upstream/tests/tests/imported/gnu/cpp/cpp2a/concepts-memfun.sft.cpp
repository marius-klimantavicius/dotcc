//type: rp
//options: --c++20
# 0 "./cpp2a/concepts-memfun.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp2a/concepts-memfun.C"



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
# 5 "./cpp2a/concepts-memfun.C" 2


# 6 "./cpp2a/concepts-memfun.C"
template<typename T>
  concept C = __is_class(T);

template<typename T>
  concept D = __is_empty(T);

struct X { } x;
struct Y { int n; } y;

int called = 0;


template<typename T>
  struct S1 {
    void f1() requires C<T> { }

    void f2() requires C<T> { called = 1; }
    void f2() requires (not C<T>) { called = 2; }

    void f3() { called = 1; }
    void f3() requires C<T> { called = 2; }
    void f3() requires C<T> and D<T> { called = 3; }

    void g1() requires C<T> and true;

    void g2() requires C<T>;
    void g2() requires (not C<T>);

    void g3();
    void g3() requires C<T>;
    void g3() requires C<T> and D<T>;

    template<C U> void h1(U u) { called = 1; }
    template<C U> void h2(U u);
    template<C U> void h3(U u) requires D<U>;
  };

template<C T>
  struct S2 {
    void f(T) requires D<T>;
  };


int main() {
  S1<X> sx;
  S1<Y> sy;
  S1<int> si;


  sx.f1();
  sx.f2(); 
# 56 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 56 "./cpp2a/concepts-memfun.C"
          called == 1
# 56 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 56 "./cpp2a/concepts-memfun.C"
          "called == 1"
# 56 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 56, __PRETTY_FUNCTION__))
# 56 "./cpp2a/concepts-memfun.C"
                             ;
  sx.f3(); 
# 57 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 57 "./cpp2a/concepts-memfun.C"
          called == 3
# 57 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 57 "./cpp2a/concepts-memfun.C"
          "called == 3"
# 57 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 57, __PRETTY_FUNCTION__))
# 57 "./cpp2a/concepts-memfun.C"
                             ;

  sy.f1();
  sy.f2(); 
# 60 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 60 "./cpp2a/concepts-memfun.C"
          called == 1
# 60 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 60 "./cpp2a/concepts-memfun.C"
          "called == 1"
# 60 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 60, __PRETTY_FUNCTION__))
# 60 "./cpp2a/concepts-memfun.C"
                             ;
  sy.f3(); 
# 61 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 61 "./cpp2a/concepts-memfun.C"
          called == 2
# 61 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 61 "./cpp2a/concepts-memfun.C"
          "called == 2"
# 61 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 61, __PRETTY_FUNCTION__))
# 61 "./cpp2a/concepts-memfun.C"
                             ;

  si.f2(); 
# 63 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 63 "./cpp2a/concepts-memfun.C"
          called == 2
# 63 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 63 "./cpp2a/concepts-memfun.C"
          "called == 2"
# 63 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 63, __PRETTY_FUNCTION__))
# 63 "./cpp2a/concepts-memfun.C"
                             ;
  si.f3(); 
# 64 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 64 "./cpp2a/concepts-memfun.C"
          called == 1
# 64 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 64 "./cpp2a/concepts-memfun.C"
          "called == 1"
# 64 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 64, __PRETTY_FUNCTION__))
# 64 "./cpp2a/concepts-memfun.C"
                             ;


  S1<int> s1i;
  s1i.h1(x); 
# 68 "./cpp2a/concepts-memfun.C" 3 4
            ((
# 68 "./cpp2a/concepts-memfun.C"
            called == 1
# 68 "./cpp2a/concepts-memfun.C" 3 4
            ) ? static_cast<void> (0) : __assert_fail (
# 68 "./cpp2a/concepts-memfun.C"
            "called == 1"
# 68 "./cpp2a/concepts-memfun.C" 3 4
            , "./cpp2a/concepts-memfun.C", 68, __PRETTY_FUNCTION__))
# 68 "./cpp2a/concepts-memfun.C"
                               ;
  s1i.h2(x); 
# 69 "./cpp2a/concepts-memfun.C" 3 4
            ((
# 69 "./cpp2a/concepts-memfun.C"
            called == 2
# 69 "./cpp2a/concepts-memfun.C" 3 4
            ) ? static_cast<void> (0) : __assert_fail (
# 69 "./cpp2a/concepts-memfun.C"
            "called == 2"
# 69 "./cpp2a/concepts-memfun.C" 3 4
            , "./cpp2a/concepts-memfun.C", 69, __PRETTY_FUNCTION__))
# 69 "./cpp2a/concepts-memfun.C"
                               ;
  s1i.h3(x); 
# 70 "./cpp2a/concepts-memfun.C" 3 4
            ((
# 70 "./cpp2a/concepts-memfun.C"
            called == 3
# 70 "./cpp2a/concepts-memfun.C" 3 4
            ) ? static_cast<void> (0) : __assert_fail (
# 70 "./cpp2a/concepts-memfun.C"
            "called == 3"
# 70 "./cpp2a/concepts-memfun.C" 3 4
            , "./cpp2a/concepts-memfun.C", 70, __PRETTY_FUNCTION__))
# 70 "./cpp2a/concepts-memfun.C"
                               ;


  sx.g1();
  sx.g2(); 
# 74 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 74 "./cpp2a/concepts-memfun.C"
          called == 1
# 74 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 74 "./cpp2a/concepts-memfun.C"
          "called == 1"
# 74 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 74, __PRETTY_FUNCTION__))
# 74 "./cpp2a/concepts-memfun.C"
                             ;
  sx.g3(); 
# 75 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 75 "./cpp2a/concepts-memfun.C"
          called == 3
# 75 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 75 "./cpp2a/concepts-memfun.C"
          "called == 3"
# 75 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 75, __PRETTY_FUNCTION__))
# 75 "./cpp2a/concepts-memfun.C"
                             ;

  sy.g1();
  sy.g2(); 
# 78 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 78 "./cpp2a/concepts-memfun.C"
          called == 1
# 78 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 78 "./cpp2a/concepts-memfun.C"
          "called == 1"
# 78 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 78, __PRETTY_FUNCTION__))
# 78 "./cpp2a/concepts-memfun.C"
                             ;
  sy.g3(); 
# 79 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 79 "./cpp2a/concepts-memfun.C"
          called == 2
# 79 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 79 "./cpp2a/concepts-memfun.C"
          "called == 2"
# 79 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 79, __PRETTY_FUNCTION__))
# 79 "./cpp2a/concepts-memfun.C"
                             ;

  si.g2(); 
# 81 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 81 "./cpp2a/concepts-memfun.C"
          called == 2
# 81 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 81 "./cpp2a/concepts-memfun.C"
          "called == 2"
# 81 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 81, __PRETTY_FUNCTION__))
# 81 "./cpp2a/concepts-memfun.C"
                             ;
  si.g3(); 
# 82 "./cpp2a/concepts-memfun.C" 3 4
          ((
# 82 "./cpp2a/concepts-memfun.C"
          called == 1
# 82 "./cpp2a/concepts-memfun.C" 3 4
          ) ? static_cast<void> (0) : __assert_fail (
# 82 "./cpp2a/concepts-memfun.C"
          "called == 1"
# 82 "./cpp2a/concepts-memfun.C" 3 4
          , "./cpp2a/concepts-memfun.C", 82, __PRETTY_FUNCTION__))
# 82 "./cpp2a/concepts-memfun.C"
                             ;
}

template<typename T>
  void S1<T>::g1() requires C<T> and true { }

template<typename T>
  void S1<T>::g2() requires C<T> { called = 1; }

template<typename T>
  void S1<T>::g2() requires (not C<T>) { called = 2; }

template<typename T>
  void S1<T>::g3() { called = 1; }

template<typename T>
  void S1<T>::g3() requires C<T> { called = 2; }

template<typename T>
  void S1<T>::g3() requires C<T> and D<T> { called = 3; }

template<typename T>
  template<C U>
    void S1<T>::h2(U u) { called = 2; }

template<typename T>
  template<C U>
      void S1<T>::h3(U u) requires D<U> { called = 3; }

template<C T>
  void S2<T>::f(T t) requires D<T> { called = 4; }
