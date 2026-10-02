//type: rp
//options: 
# 0 "./ext/is_virtual_base_of.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/is_virtual_base_of.C"

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
# 3 "./ext/is_virtual_base_of.C" 2


# 4 "./ext/is_virtual_base_of.C"
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

class VD1
: public virtual A1 { };

class VD2
: protected virtual A1 { };

class VD3
: private virtual A1 { };

class D
: public B, public VD1 { };

class VDMultiple1
: public virtual A1, public A2 { };

class VDMultiple2
: public virtual A1, public virtual A2 { };

template <typename T>
class VDTemplate : public virtual T { };


namespace class_mi
{
class B { int b; };
class X : virtual public B { int x; };
class Y : virtual public B { int y; };
class Z : public B { int z; };
class AA : public X, public Y, public Z { int aa; };
}

union U
{
  double a;
  double b;
};

template<typename T, typename U>
  bool
  f()
  { return __builtin_is_virtual_base_of(T, U); }

template<typename T, typename U>
  class My
  {
  public:
    bool
    f()
    { return !!__builtin_is_virtual_base_of(T, U); }
  };

template<typename T, typename U>
  class My2
  {
  public:
    static const bool trait = __builtin_is_virtual_base_of(T, U);
  };

template<typename T, typename U>
  const bool My2<T, U>::trait;

template<typename T, typename U, bool b = __builtin_is_virtual_base_of(T, U)>
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
  
# 107 "./ext/is_virtual_base_of.C" 3 4
 ((
# 107 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(int, A1) && !f<int, A1>() && !My<int, A1>().f() && !My2<int, A1>::trait && !My3<int, A1>().f())
# 107 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 107 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(int, A1) && !f<int, A1>() && !My<int, A1>().f() && !My2<int, A1>::trait && !My3<int, A1>().f())"
# 107 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 107, __PRETTY_FUNCTION__))
# 107 "./ext/is_virtual_base_of.C"
                         ;
  
# 108 "./ext/is_virtual_base_of.C" 3 4
 ((
# 108 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, void) && !f<A1, void>() && !My<A1, void>().f() && !My2<A1, void>::trait && !My3<A1, void>().f())
# 108 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 108 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, void) && !f<A1, void>() && !My<A1, void>().f() && !My2<A1, void>::trait && !My3<A1, void>().f())"
# 108 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 108, __PRETTY_FUNCTION__))
# 108 "./ext/is_virtual_base_of.C"
                          ;
  
# 109 "./ext/is_virtual_base_of.C" 3 4
 ((
# 109 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, A1) && !f<A1, A1>() && !My<A1, A1>().f() && !My2<A1, A1>::trait && !My3<A1, A1>().f())
# 109 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 109 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, A1) && !f<A1, A1>() && !My<A1, A1>().f() && !My2<A1, A1>::trait && !My3<A1, A1>().f())"
# 109 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 109, __PRETTY_FUNCTION__))
# 109 "./ext/is_virtual_base_of.C"
                        ;
  
# 110 "./ext/is_virtual_base_of.C" 3 4
 ((
# 110 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1*, A1*) && !f<A1*, A1*>() && !My<A1*, A1*>().f() && !My2<A1*, A1*>::trait && !My3<A1*, A1*>().f())
# 110 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 110 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1*, A1*) && !f<A1*, A1*>() && !My<A1*, A1*>().f() && !My2<A1*, A1*>::trait && !My3<A1*, A1*>().f())"
# 110 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 110, __PRETTY_FUNCTION__))
# 110 "./ext/is_virtual_base_of.C"
                          ;
  
# 111 "./ext/is_virtual_base_of.C" 3 4
 ((
# 111 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1&, A1&) && !f<A1&, A1&>() && !My<A1&, A1&>().f() && !My2<A1&, A1&>::trait && !My3<A1&, A1&>().f())
# 111 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 111 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1&, A1&) && !f<A1&, A1&>() && !My<A1&, A1&>().f() && !My2<A1&, A1&>::trait && !My3<A1&, A1&>().f())"
# 111 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 111, __PRETTY_FUNCTION__))
# 111 "./ext/is_virtual_base_of.C"
                          ;
  
# 112 "./ext/is_virtual_base_of.C" 3 4
 ((
# 112 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, B) && !f<A1, B>() && !My<A1, B>().f() && !My2<A1, B>::trait && !My3<A1, B>().f())
# 112 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 112 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, B) && !f<A1, B>() && !My<A1, B>().f() && !My2<A1, B>::trait && !My3<A1, B>().f())"
# 112 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 112, __PRETTY_FUNCTION__))
# 112 "./ext/is_virtual_base_of.C"
                       ;
  
# 113 "./ext/is_virtual_base_of.C" 3 4
 ((
# 113 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(B, A1) && !f<B, A1>() && !My<B, A1>().f() && !My2<B, A1>::trait && !My3<B, A1>().f())
# 113 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 113 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(B, A1) && !f<B, A1>() && !My<B, A1>().f() && !My2<B, A1>::trait && !My3<B, A1>().f())"
# 113 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 113, __PRETTY_FUNCTION__))
# 113 "./ext/is_virtual_base_of.C"
                       ;
  
# 114 "./ext/is_virtual_base_of.C" 3 4
 ((
# 114 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, C) && !f<A1, C>() && !My<A1, C>().f() && !My2<A1, C>::trait && !My3<A1, C>().f())
# 114 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 114 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, C) && !f<A1, C>() && !My<A1, C>().f() && !My2<A1, C>::trait && !My3<A1, C>().f())"
# 114 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 114, __PRETTY_FUNCTION__))
# 114 "./ext/is_virtual_base_of.C"
                       ;
  
# 115 "./ext/is_virtual_base_of.C" 3 4
 ((
# 115 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A2, C) && !f<A2, C>() && !My<A2, C>().f() && !My2<A2, C>::trait && !My3<A2, C>().f())
# 115 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 115 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A2, C) && !f<A2, C>() && !My<A2, C>().f() && !My2<A2, C>::trait && !My3<A2, C>().f())"
# 115 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 115, __PRETTY_FUNCTION__))
# 115 "./ext/is_virtual_base_of.C"
                       ;
  
# 116 "./ext/is_virtual_base_of.C" 3 4
 ((
# 116 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(C, A1) && !f<C, A1>() && !My<C, A1>().f() && !My2<C, A1>::trait && !My3<C, A1>().f())
# 116 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 116 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(C, A1) && !f<C, A1>() && !My<C, A1>().f() && !My2<C, A1>::trait && !My3<C, A1>().f())"
# 116 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 116, __PRETTY_FUNCTION__))
# 116 "./ext/is_virtual_base_of.C"
                       ;
  
# 117 "./ext/is_virtual_base_of.C" 3 4
 ((
# 117 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, const B) && !f<A1, const B>() && !My<A1, const B>().f() && !My2<A1, const B>::trait && !My3<A1, const B>().f())
# 117 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 117 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, const B) && !f<A1, const B>() && !My<A1, const B>().f() && !My2<A1, const B>::trait && !My3<A1, const B>().f())"
# 117 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 117, __PRETTY_FUNCTION__))
# 117 "./ext/is_virtual_base_of.C"
                             ;
  
# 118 "./ext/is_virtual_base_of.C" 3 4
 ((
# 118 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(const B, A1) && !f<const B, A1>() && !My<const B, A1>().f() && !My2<const B, A1>::trait && !My3<const B, A1>().f())
# 118 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 118 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(const B, A1) && !f<const B, A1>() && !My<const B, A1>().f() && !My2<const B, A1>::trait && !My3<const B, A1>().f())"
# 118 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 118, __PRETTY_FUNCTION__))
# 118 "./ext/is_virtual_base_of.C"
                             ;
  
# 119 "./ext/is_virtual_base_of.C" 3 4
 ((
# 119 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, volatile C) && !f<A1, volatile C>() && !My<A1, volatile C>().f() && !My2<A1, volatile C>::trait && !My3<A1, volatile C>().f())
# 119 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 119 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, volatile C) && !f<A1, volatile C>() && !My<A1, volatile C>().f() && !My2<A1, volatile C>::trait && !My3<A1, volatile C>().f())"
# 119 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 119, __PRETTY_FUNCTION__))
# 119 "./ext/is_virtual_base_of.C"
                                ;
  
# 120 "./ext/is_virtual_base_of.C" 3 4
 ((
# 120 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(volatile A2, const C) && !f<volatile A2, const C>() && !My<volatile A2, const C>().f() && !My2<volatile A2, const C>::trait && !My3<volatile A2, const C>().f())
# 120 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 120 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(volatile A2, const C) && !f<volatile A2, const C>() && !My<volatile A2, const C>().f() && !My2<volatile A2, const C>::trait && !My3<volatile A2, const C>().f())"
# 120 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 120, __PRETTY_FUNCTION__))
# 120 "./ext/is_virtual_base_of.C"
                                      ;
  
# 121 "./ext/is_virtual_base_of.C" 3 4
 ((
# 121 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(const volatile C, A1) && !f<const volatile C, A1>() && !My<const volatile C, A1>().f() && !My2<const volatile C, A1>::trait && !My3<const volatile C, A1>().f())
# 121 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 121 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(const volatile C, A1) && !f<const volatile C, A1>() && !My<const volatile C, A1>().f() && !My2<const volatile C, A1>::trait && !My3<const volatile C, A1>().f())"
# 121 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 121, __PRETTY_FUNCTION__))
# 121 "./ext/is_virtual_base_of.C"
                                      ;

  
# 123 "./ext/is_virtual_base_of.C" 3 4
 ((
# 123 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, VD1) && f<A1, VD1>() && My<A1, VD1>().f() && My2<A1, VD1>::trait && My3<A1, VD1>().f())
# 123 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 123 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, VD1) && f<A1, VD1>() && My<A1, VD1>().f() && My2<A1, VD1>::trait && My3<A1, VD1>().f())"
# 123 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 123, __PRETTY_FUNCTION__))
# 123 "./ext/is_virtual_base_of.C"
                         ;
  
# 124 "./ext/is_virtual_base_of.C" 3 4
 ((
# 124 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, VD2) && f<A1, VD2>() && My<A1, VD2>().f() && My2<A1, VD2>::trait && My3<A1, VD2>().f())
# 124 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 124 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, VD2) && f<A1, VD2>() && My<A1, VD2>().f() && My2<A1, VD2>::trait && My3<A1, VD2>().f())"
# 124 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 124, __PRETTY_FUNCTION__))
# 124 "./ext/is_virtual_base_of.C"
                         ;
  
# 125 "./ext/is_virtual_base_of.C" 3 4
 ((
# 125 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, VD3) && f<A1, VD3>() && My<A1, VD3>().f() && My2<A1, VD3>::trait && My3<A1, VD3>().f())
# 125 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 125 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, VD3) && f<A1, VD3>() && My<A1, VD3>().f() && My2<A1, VD3>::trait && My3<A1, VD3>().f())"
# 125 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 125, __PRETTY_FUNCTION__))
# 125 "./ext/is_virtual_base_of.C"
                         ;

  
# 127 "./ext/is_virtual_base_of.C" 3 4
 ((
# 127 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, const VD1) && f<A1, const VD1>() && My<A1, const VD1>().f() && My2<A1, const VD1>::trait && My3<A1, const VD1>().f())
# 127 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 127 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, const VD1) && f<A1, const VD1>() && My<A1, const VD1>().f() && My2<A1, const VD1>::trait && My3<A1, const VD1>().f())"
# 127 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 127, __PRETTY_FUNCTION__))
# 127 "./ext/is_virtual_base_of.C"
                               ;
  
# 128 "./ext/is_virtual_base_of.C" 3 4
 ((
# 128 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, const VD2) && f<A1, const VD2>() && My<A1, const VD2>().f() && My2<A1, const VD2>::trait && My3<A1, const VD2>().f())
# 128 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 128 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, const VD2) && f<A1, const VD2>() && My<A1, const VD2>().f() && My2<A1, const VD2>::trait && My3<A1, const VD2>().f())"
# 128 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 128, __PRETTY_FUNCTION__))
# 128 "./ext/is_virtual_base_of.C"
                               ;
  
# 129 "./ext/is_virtual_base_of.C" 3 4
 ((
# 129 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, const VD3) && f<A1, const VD3>() && My<A1, const VD3>().f() && My2<A1, const VD3>::trait && My3<A1, const VD3>().f())
# 129 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 129 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, const VD3) && f<A1, const VD3>() && My<A1, const VD3>().f() && My2<A1, const VD3>::trait && My3<A1, const VD3>().f())"
# 129 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 129, __PRETTY_FUNCTION__))
# 129 "./ext/is_virtual_base_of.C"
                               ;

  
# 131 "./ext/is_virtual_base_of.C" 3 4
 ((
# 131 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(const A1, VD1) && f<const A1, VD1>() && My<const A1, VD1>().f() && My2<const A1, VD1>::trait && My3<const A1, VD1>().f())
# 131 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 131 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(const A1, VD1) && f<const A1, VD1>() && My<const A1, VD1>().f() && My2<const A1, VD1>::trait && My3<const A1, VD1>().f())"
# 131 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 131, __PRETTY_FUNCTION__))
# 131 "./ext/is_virtual_base_of.C"
                               ;
  
# 132 "./ext/is_virtual_base_of.C" 3 4
 ((
# 132 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(const A1, VD2) && f<const A1, VD2>() && My<const A1, VD2>().f() && My2<const A1, VD2>::trait && My3<const A1, VD2>().f())
# 132 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 132 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(const A1, VD2) && f<const A1, VD2>() && My<const A1, VD2>().f() && My2<const A1, VD2>::trait && My3<const A1, VD2>().f())"
# 132 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 132, __PRETTY_FUNCTION__))
# 132 "./ext/is_virtual_base_of.C"
                               ;
  
# 133 "./ext/is_virtual_base_of.C" 3 4
 ((
# 133 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(const A1, VD3) && f<const A1, VD3>() && My<const A1, VD3>().f() && My2<const A1, VD3>::trait && My3<const A1, VD3>().f())
# 133 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 133 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(const A1, VD3) && f<const A1, VD3>() && My<const A1, VD3>().f() && My2<const A1, VD3>::trait && My3<const A1, VD3>().f())"
# 133 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 133, __PRETTY_FUNCTION__))
# 133 "./ext/is_virtual_base_of.C"
                               ;

  
# 135 "./ext/is_virtual_base_of.C" 3 4
 ((
# 135 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(const A1, const VD1) && f<const A1, const VD1>() && My<const A1, const VD1>().f() && My2<const A1, const VD1>::trait && My3<const A1, const VD1>().f())
# 135 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 135 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(const A1, const VD1) && f<const A1, const VD1>() && My<const A1, const VD1>().f() && My2<const A1, const VD1>::trait && My3<const A1, const VD1>().f())"
# 135 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 135, __PRETTY_FUNCTION__))
# 135 "./ext/is_virtual_base_of.C"
                                     ;
  
# 136 "./ext/is_virtual_base_of.C" 3 4
 ((
# 136 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(const A1, const VD2) && f<const A1, const VD2>() && My<const A1, const VD2>().f() && My2<const A1, const VD2>::trait && My3<const A1, const VD2>().f())
# 136 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 136 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(const A1, const VD2) && f<const A1, const VD2>() && My<const A1, const VD2>().f() && My2<const A1, const VD2>::trait && My3<const A1, const VD2>().f())"
# 136 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 136, __PRETTY_FUNCTION__))
# 136 "./ext/is_virtual_base_of.C"
                                     ;
  
# 137 "./ext/is_virtual_base_of.C" 3 4
 ((
# 137 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(const A1, const VD3) && f<const A1, const VD3>() && My<const A1, const VD3>().f() && My2<const A1, const VD3>::trait && My3<const A1, const VD3>().f())
# 137 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 137 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(const A1, const VD3) && f<const A1, const VD3>() && My<const A1, const VD3>().f() && My2<const A1, const VD3>::trait && My3<const A1, const VD3>().f())"
# 137 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 137, __PRETTY_FUNCTION__))
# 137 "./ext/is_virtual_base_of.C"
                                     ;

  
# 139 "./ext/is_virtual_base_of.C" 3 4
 ((
# 139 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A2, VD1) && !f<A2, VD1>() && !My<A2, VD1>().f() && !My2<A2, VD1>::trait && !My3<A2, VD1>().f())
# 139 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 139 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A2, VD1) && !f<A2, VD1>() && !My<A2, VD1>().f() && !My2<A2, VD1>::trait && !My3<A2, VD1>().f())"
# 139 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 139, __PRETTY_FUNCTION__))
# 139 "./ext/is_virtual_base_of.C"
                         ;
  
# 140 "./ext/is_virtual_base_of.C" 3 4
 ((
# 140 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A2, VD2) && !f<A2, VD2>() && !My<A2, VD2>().f() && !My2<A2, VD2>::trait && !My3<A2, VD2>().f())
# 140 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 140 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A2, VD2) && !f<A2, VD2>() && !My<A2, VD2>().f() && !My2<A2, VD2>::trait && !My3<A2, VD2>().f())"
# 140 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 140, __PRETTY_FUNCTION__))
# 140 "./ext/is_virtual_base_of.C"
                         ;
  
# 141 "./ext/is_virtual_base_of.C" 3 4
 ((
# 141 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A2, VD3) && !f<A2, VD3>() && !My<A2, VD3>().f() && !My2<A2, VD3>::trait && !My3<A2, VD3>().f())
# 141 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 141 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A2, VD3) && !f<A2, VD3>() && !My<A2, VD3>().f() && !My2<A2, VD3>::trait && !My3<A2, VD3>().f())"
# 141 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 141, __PRETTY_FUNCTION__))
# 141 "./ext/is_virtual_base_of.C"
                         ;

  
# 143 "./ext/is_virtual_base_of.C" 3 4
 ((
# 143 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, D) && f<A1, D>() && My<A1, D>().f() && My2<A1, D>::trait && My3<A1, D>().f())
# 143 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 143 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, D) && f<A1, D>() && My<A1, D>().f() && My2<A1, D>::trait && My3<A1, D>().f())"
# 143 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 143, __PRETTY_FUNCTION__))
# 143 "./ext/is_virtual_base_of.C"
                       ;

  
# 145 "./ext/is_virtual_base_of.C" 3 4
 ((
# 145 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, VDMultiple1) && f<A1, VDMultiple1>() && My<A1, VDMultiple1>().f() && My2<A1, VDMultiple1>::trait && My3<A1, VDMultiple1>().f())
# 145 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 145 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, VDMultiple1) && f<A1, VDMultiple1>() && My<A1, VDMultiple1>().f() && My2<A1, VDMultiple1>::trait && My3<A1, VDMultiple1>().f())"
# 145 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 145, __PRETTY_FUNCTION__))
# 145 "./ext/is_virtual_base_of.C"
                                 ;
  
# 146 "./ext/is_virtual_base_of.C" 3 4
 ((
# 146 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, VDMultiple2) && f<A1, VDMultiple2>() && My<A1, VDMultiple2>().f() && My2<A1, VDMultiple2>::trait && My3<A1, VDMultiple2>().f())
# 146 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 146 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, VDMultiple2) && f<A1, VDMultiple2>() && My<A1, VDMultiple2>().f() && My2<A1, VDMultiple2>::trait && My3<A1, VDMultiple2>().f())"
# 146 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 146, __PRETTY_FUNCTION__))
# 146 "./ext/is_virtual_base_of.C"
                                 ;
  
# 147 "./ext/is_virtual_base_of.C" 3 4
 ((
# 147 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A2, VDMultiple1) && !f<A2, VDMultiple1>() && !My<A2, VDMultiple1>().f() && !My2<A2, VDMultiple1>::trait && !My3<A2, VDMultiple1>().f())
# 147 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 147 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A2, VDMultiple1) && !f<A2, VDMultiple1>() && !My<A2, VDMultiple1>().f() && !My2<A2, VDMultiple1>::trait && !My3<A2, VDMultiple1>().f())"
# 147 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 147, __PRETTY_FUNCTION__))
# 147 "./ext/is_virtual_base_of.C"
                                 ;
  
# 148 "./ext/is_virtual_base_of.C" 3 4
 ((
# 148 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A2, VDMultiple2) && f<A2, VDMultiple2>() && My<A2, VDMultiple2>().f() && My2<A2, VDMultiple2>::trait && My3<A2, VDMultiple2>().f())
# 148 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 148 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A2, VDMultiple2) && f<A2, VDMultiple2>() && My<A2, VDMultiple2>().f() && My2<A2, VDMultiple2>::trait && My3<A2, VDMultiple2>().f())"
# 148 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 148, __PRETTY_FUNCTION__))
# 148 "./ext/is_virtual_base_of.C"
                                 ;

  
# 150 "./ext/is_virtual_base_of.C" 3 4
 ((
# 150 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(A1, VDTemplate<A1>) && f<A1, VDTemplate<A1> >() && My<A1, VDTemplate<A1> >().f() && My2<A1, VDTemplate<A1> >::trait && My3<A1, VDTemplate<A1> >().f())
# 150 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 150 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(A1, VDTemplate<A1>) && f<A1, VDTemplate<A1>>() && My<A1, VDTemplate<A1>>().f() && My2<A1, VDTemplate<A1>>::trait && My3<A1, VDTemplate<A1>>().f())"
# 150 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 150, __PRETTY_FUNCTION__))
# 150 "./ext/is_virtual_base_of.C"
                                    ;
  
# 151 "./ext/is_virtual_base_of.C" 3 4
 ((
# 151 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A2, VDTemplate<A1>) && !f<A2, VDTemplate<A1> >() && !My<A2, VDTemplate<A1> >().f() && !My2<A2, VDTemplate<A1> >::trait && !My3<A2, VDTemplate<A1> >().f())
# 151 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 151 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A2, VDTemplate<A1>) && !f<A2, VDTemplate<A1>>() && !My<A2, VDTemplate<A1>>().f() && !My2<A2, VDTemplate<A1>>::trait && !My3<A2, VDTemplate<A1>>().f())"
# 151 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 151, __PRETTY_FUNCTION__))
# 151 "./ext/is_virtual_base_of.C"
                                    ;
  
# 152 "./ext/is_virtual_base_of.C" 3 4
 ((
# 152 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(A1, VDTemplate<A2>) && !f<A1, VDTemplate<A2> >() && !My<A1, VDTemplate<A2> >().f() && !My2<A1, VDTemplate<A2> >::trait && !My3<A1, VDTemplate<A2> >().f())
# 152 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 152 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(A1, VDTemplate<A2>) && !f<A1, VDTemplate<A2>>() && !My<A1, VDTemplate<A2>>().f() && !My2<A1, VDTemplate<A2>>::trait && !My3<A1, VDTemplate<A2>>().f())"
# 152 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 152, __PRETTY_FUNCTION__))
# 152 "./ext/is_virtual_base_of.C"
                                    ;

  
# 154 "./ext/is_virtual_base_of.C" 3 4
 ((
# 154 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(class_mi::B, class_mi::B) && !f<class_mi::B, class_mi::B>() && !My<class_mi::B, class_mi::B>().f() && !My2<class_mi::B, class_mi::B>::trait && !My3<class_mi::B, class_mi::B>().f())
# 154 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 154 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(class_mi::B, class_mi::B) && !f<class_mi::B, class_mi::B>() && !My<class_mi::B, class_mi::B>().f() && !My2<class_mi::B, class_mi::B>::trait && !My3<class_mi::B, class_mi::B>().f())"
# 154 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 154, __PRETTY_FUNCTION__))
# 154 "./ext/is_virtual_base_of.C"
                                          ;
  
# 155 "./ext/is_virtual_base_of.C" 3 4
 ((
# 155 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(class_mi::B, class_mi::X) && f<class_mi::B, class_mi::X>() && My<class_mi::B, class_mi::X>().f() && My2<class_mi::B, class_mi::X>::trait && My3<class_mi::B, class_mi::X>().f())
# 155 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 155 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(class_mi::B, class_mi::X) && f<class_mi::B, class_mi::X>() && My<class_mi::B, class_mi::X>().f() && My2<class_mi::B, class_mi::X>::trait && My3<class_mi::B, class_mi::X>().f())"
# 155 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 155, __PRETTY_FUNCTION__))
# 155 "./ext/is_virtual_base_of.C"
                                          ;
  
# 156 "./ext/is_virtual_base_of.C" 3 4
 ((
# 156 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(class_mi::B, class_mi::Y) && f<class_mi::B, class_mi::Y>() && My<class_mi::B, class_mi::Y>().f() && My2<class_mi::B, class_mi::Y>::trait && My3<class_mi::B, class_mi::Y>().f())
# 156 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 156 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(class_mi::B, class_mi::Y) && f<class_mi::B, class_mi::Y>() && My<class_mi::B, class_mi::Y>().f() && My2<class_mi::B, class_mi::Y>::trait && My3<class_mi::B, class_mi::Y>().f())"
# 156 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 156, __PRETTY_FUNCTION__))
# 156 "./ext/is_virtual_base_of.C"
                                          ;
  
# 157 "./ext/is_virtual_base_of.C" 3 4
 ((
# 157 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(class_mi::B, class_mi::Z) && !f<class_mi::B, class_mi::Z>() && !My<class_mi::B, class_mi::Z>().f() && !My2<class_mi::B, class_mi::Z>::trait && !My3<class_mi::B, class_mi::Z>().f())
# 157 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 157 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(class_mi::B, class_mi::Z) && !f<class_mi::B, class_mi::Z>() && !My<class_mi::B, class_mi::Z>().f() && !My2<class_mi::B, class_mi::Z>::trait && !My3<class_mi::B, class_mi::Z>().f())"
# 157 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 157, __PRETTY_FUNCTION__))
# 157 "./ext/is_virtual_base_of.C"
                                          ;
  
# 158 "./ext/is_virtual_base_of.C" 3 4
 ((
# 158 "./ext/is_virtual_base_of.C"
 (__builtin_is_virtual_base_of(class_mi::B, class_mi::AA) && f<class_mi::B, class_mi::AA>() && My<class_mi::B, class_mi::AA>().f() && My2<class_mi::B, class_mi::AA>::trait && My3<class_mi::B, class_mi::AA>().f())
# 158 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 158 "./ext/is_virtual_base_of.C"
 "(__builtin_is_virtual_base_of(class_mi::B, class_mi::AA) && f<class_mi::B, class_mi::AA>() && My<class_mi::B, class_mi::AA>().f() && My2<class_mi::B, class_mi::AA>::trait && My3<class_mi::B, class_mi::AA>().f())"
# 158 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 158, __PRETTY_FUNCTION__))
# 158 "./ext/is_virtual_base_of.C"
                                           ;

  
# 160 "./ext/is_virtual_base_of.C" 3 4
 ((
# 160 "./ext/is_virtual_base_of.C"
 (!__builtin_is_virtual_base_of(U, U) && !f<U, U>() && !My<U, U>().f() && !My2<U, U>::trait && !My3<U, U>().f())
# 160 "./ext/is_virtual_base_of.C" 3 4
 ) ? static_cast<void> (0) : __assert_fail (
# 160 "./ext/is_virtual_base_of.C"
 "(!__builtin_is_virtual_base_of(U, U) && !f<U, U>() && !My<U, U>().f() && !My2<U, U>::trait && !My3<U, U>().f())"
# 160 "./ext/is_virtual_base_of.C" 3 4
 , "./ext/is_virtual_base_of.C", 160, __PRETTY_FUNCTION__))
# 160 "./ext/is_virtual_base_of.C"
                      ;

  return 0;
}
