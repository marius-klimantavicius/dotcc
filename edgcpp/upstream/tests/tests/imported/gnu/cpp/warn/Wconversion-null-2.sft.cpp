//type: lp
//options: 
# 0 "./warn/Wconversion-null-2.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/Wconversion-null-2.C"



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstddef" 1 3
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstddef" 3
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
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstddef" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 425 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
} max_align_t;






  typedef decltype(nullptr) nullptr_t;
# 53 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstddef" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 56 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstddef" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"

extern "C++"
{

namespace std
{

  using ::max_align_t;
}



namespace std
{


  enum class byte : unsigned char {};

  template<typename _IntegerType> struct __byte_operand { };
  template<> struct __byte_operand<bool> { using __type = byte; };
  template<> struct __byte_operand<char> { using __type = byte; };
  template<> struct __byte_operand<signed char> { using __type = byte; };
  template<> struct __byte_operand<unsigned char> { using __type = byte; };
  template<> struct __byte_operand<wchar_t> { using __type = byte; };



  template<> struct __byte_operand<char16_t> { using __type = byte; };
  template<> struct __byte_operand<char32_t> { using __type = byte; };
  template<> struct __byte_operand<short> { using __type = byte; };
  template<> struct __byte_operand<unsigned short> { using __type = byte; };
  template<> struct __byte_operand<int> { using __type = byte; };
  template<> struct __byte_operand<unsigned int> { using __type = byte; };
  template<> struct __byte_operand<long> { using __type = byte; };
  template<> struct __byte_operand<unsigned long> { using __type = byte; };
  template<> struct __byte_operand<long long> { using __type = byte; };
  template<> struct __byte_operand<unsigned long long> { using __type = byte; };

  template<> struct __byte_operand<__int128>
  { using __type = byte; };
  template<> struct __byte_operand<unsigned __int128>
  { using __type = byte; };
# 114 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstddef" 3
  template<typename _IntegerType>
    struct __byte_operand<const _IntegerType>
    : __byte_operand<_IntegerType> { };
  template<typename _IntegerType>
    struct __byte_operand<volatile _IntegerType>
    : __byte_operand<_IntegerType> { };
  template<typename _IntegerType>
    struct __byte_operand<const volatile _IntegerType>
    : __byte_operand<_IntegerType> { };

  template<typename _IntegerType>
    using __byte_op_t = typename __byte_operand<_IntegerType>::__type;

  template<typename _IntegerType>
    [[__gnu__::__always_inline__]]
    constexpr __byte_op_t<_IntegerType>
    operator<<(byte __b, _IntegerType __shift) noexcept
    { return (byte)(unsigned char)((unsigned)__b << __shift); }

  template<typename _IntegerType>
    [[__gnu__::__always_inline__]]
    constexpr __byte_op_t<_IntegerType>
    operator>>(byte __b, _IntegerType __shift) noexcept
    { return (byte)(unsigned char)((unsigned)__b >> __shift); }

  [[__gnu__::__always_inline__]]
  constexpr byte
  operator|(byte __l, byte __r) noexcept
  { return (byte)(unsigned char)((unsigned)__l | (unsigned)__r); }

  [[__gnu__::__always_inline__]]
  constexpr byte
  operator&(byte __l, byte __r) noexcept
  { return (byte)(unsigned char)((unsigned)__l & (unsigned)__r); }

  [[__gnu__::__always_inline__]]
  constexpr byte
  operator^(byte __l, byte __r) noexcept
  { return (byte)(unsigned char)((unsigned)__l ^ (unsigned)__r); }

  [[__gnu__::__always_inline__]]
  constexpr byte
  operator~(byte __b) noexcept
  { return (byte)(unsigned char)~(unsigned)__b; }

  template<typename _IntegerType>
    [[__gnu__::__always_inline__]]
    constexpr __byte_op_t<_IntegerType>&
    operator<<=(byte& __b, _IntegerType __shift) noexcept
    { return __b = __b << __shift; }

  template<typename _IntegerType>
    [[__gnu__::__always_inline__]]
    constexpr __byte_op_t<_IntegerType>&
    operator>>=(byte& __b, _IntegerType __shift) noexcept
    { return __b = __b >> __shift; }

  [[__gnu__::__always_inline__]]
  constexpr byte&
  operator|=(byte& __l, byte __r) noexcept
  { return __l = __l | __r; }

  [[__gnu__::__always_inline__]]
  constexpr byte&
  operator&=(byte& __l, byte __r) noexcept
  { return __l = __l & __r; }

  [[__gnu__::__always_inline__]]
  constexpr byte&
  operator^=(byte& __l, byte __r) noexcept
  { return __l = __l ^ __r; }

  template<typename _IntegerType>
    [[nodiscard,__gnu__::__always_inline__]]
    constexpr _IntegerType
    to_integer(__byte_op_t<_IntegerType> __b) noexcept
    { return _IntegerType(__b); }


}

}

#pragma GCC diagnostic pop
# 5 "./warn/Wconversion-null-2.C" 2


# 6 "./warn/Wconversion-null-2.C"
void g(int) {}
void g(long) {}
void g(long long) {}



extern void g(void*);

template <int I>
void h() {}

void k(int) {}

template <class T>
void l(T);

template <>
void l(int) {}

template <>
void l(long) {}

template <>
void l(long long) {}






void warn_for_NULL()
{
  int i = 
# 38 "./warn/Wconversion-null-2.C" 3 4
         __null
# 38 "./warn/Wconversion-null-2.C"
             ;
  float z = 
# 39 "./warn/Wconversion-null-2.C" 3 4
           __null
# 39 "./warn/Wconversion-null-2.C"
               ;
  int a[2];

  i != 
# 42 "./warn/Wconversion-null-2.C" 3 4
      __null
# 42 "./warn/Wconversion-null-2.C"
          ;
  
# 43 "./warn/Wconversion-null-2.C" 3 4
 __null 
# 43 "./warn/Wconversion-null-2.C"
      != z;
  k != 
# 44 "./warn/Wconversion-null-2.C" 3 4
      __null
# 44 "./warn/Wconversion-null-2.C"
          ;
  
# 45 "./warn/Wconversion-null-2.C" 3 4
 __null 
# 45 "./warn/Wconversion-null-2.C"
      != a;
  -
# 46 "./warn/Wconversion-null-2.C" 3 4
  __null
# 46 "./warn/Wconversion-null-2.C"
      ;
  +
# 47 "./warn/Wconversion-null-2.C" 3 4
  __null
# 47 "./warn/Wconversion-null-2.C"
      ;
  ~
# 48 "./warn/Wconversion-null-2.C" 3 4
  __null
# 48 "./warn/Wconversion-null-2.C"
      ;
  a[
# 49 "./warn/Wconversion-null-2.C" 3 4
   __null
# 49 "./warn/Wconversion-null-2.C"
       ] = 3;
  i = 
# 50 "./warn/Wconversion-null-2.C" 3 4
     __null
# 50 "./warn/Wconversion-null-2.C"
         ;
  z = 
# 51 "./warn/Wconversion-null-2.C" 3 4
     __null
# 51 "./warn/Wconversion-null-2.C"
         ;
  k(
# 52 "./warn/Wconversion-null-2.C" 3 4
   __null
# 52 "./warn/Wconversion-null-2.C"
       );
  g(
# 53 "./warn/Wconversion-null-2.C" 3 4
   __null
# 53 "./warn/Wconversion-null-2.C"
       );
  h<
# 54 "./warn/Wconversion-null-2.C" 3 4
   __null
# 54 "./warn/Wconversion-null-2.C"
       >();
  l(
# 55 "./warn/Wconversion-null-2.C" 3 4
   __null
# 55 "./warn/Wconversion-null-2.C"
       );
  
# 56 "./warn/Wconversion-null-2.C" 3 4
 __null 
# 56 "./warn/Wconversion-null-2.C"
      && 
# 56 "./warn/Wconversion-null-2.C" 3 4
         __null
# 56 "./warn/Wconversion-null-2.C"
             ;
}

void warn_for___null()
{
  int i = __null;
  float z = __null;
  int a[2];

  i != __null;
  __null != z;
  k != __null;
  __null != a;
  -__null;
  +__null;
  ~__null;
  a[__null] = 3;
  i = __null;
  z = __null;
  k(__null);
  g(__null);
  h<__null>();
  l(__null);
  __null && __null;
}

int main()
{
  warn_for_NULL();
  warn_for___null();
}
