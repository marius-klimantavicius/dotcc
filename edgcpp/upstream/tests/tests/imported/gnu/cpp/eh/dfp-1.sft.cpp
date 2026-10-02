//type: rp
//options: 
# 0 "./eh/dfp-1.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./eh/dfp-1.C"




extern "C" void abort ();

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"

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
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 2 3






namespace std __attribute__ ((__visibility__ ("default")))
{

# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
namespace decimal
{
  class decimal32;
  class decimal64;
  class decimal128;


  static decimal32 make_decimal32(long long __coeff, int __exp);
  static decimal32 make_decimal32(unsigned long long __coeff, int __exp);
  static decimal64 make_decimal64(long long __coeff, int __exp);
  static decimal64 make_decimal64(unsigned long long __coeff, int __exp);
  static decimal128 make_decimal128(long long __coeff, int __exp);
  static decimal128 make_decimal128(unsigned long long __coeff, int __exp);


  long long decimal32_to_long_long(decimal32 __d);
  long long decimal64_to_long_long(decimal64 __d);
  long long decimal128_to_long_long(decimal128 __d);
  long long decimal_to_long_long(decimal32 __d);
  long long decimal_to_long_long(decimal64 __d);
  long long decimal_to_long_long(decimal128 __d);


  float decimal32_to_float(decimal32 __d);
  float decimal64_to_float(decimal64 __d);
  float decimal128_to_float(decimal128 __d);
  float decimal_to_float(decimal32 __d);
  float decimal_to_float(decimal64 __d);
  float decimal_to_float(decimal128 __d);

  double decimal32_to_double(decimal32 __d);
  double decimal64_to_double(decimal64 __d);
  double decimal128_to_double(decimal128 __d);
  double decimal_to_double(decimal32 __d);
  double decimal_to_double(decimal64 __d);
  double decimal_to_double(decimal128 __d);

  long double decimal32_to_long_double(decimal32 __d);
  long double decimal64_to_long_double(decimal64 __d);
  long double decimal128_to_long_double(decimal128 __d);
  long double decimal_to_long_double(decimal32 __d);
  long double decimal_to_long_double(decimal64 __d);
  long double decimal_to_long_double(decimal128 __d);


  decimal32 operator+(decimal32 __rhs);
  decimal64 operator+(decimal64 __rhs);
  decimal128 operator+(decimal128 __rhs);
  decimal32 operator-(decimal32 __rhs);
  decimal64 operator-(decimal64 __rhs);
  decimal128 operator-(decimal128 __rhs);
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
  decimal32 operator +(decimal32 __lhs, decimal32 __rhs);
  decimal32 operator +(decimal32 __lhs, int __rhs); decimal32 operator +(decimal32 __lhs, unsigned int __rhs); decimal32 operator +(decimal32 __lhs, long __rhs); decimal32 operator +(decimal32 __lhs, unsigned long __rhs); decimal32 operator +(decimal32 __lhs, long long __rhs); decimal32 operator +(decimal32 __lhs, unsigned long long __rhs); decimal32 operator +(int __lhs, decimal32 __rhs); decimal32 operator +(unsigned int __lhs, decimal32 __rhs); decimal32 operator +(long __lhs, decimal32 __rhs); decimal32 operator +(unsigned long __lhs, decimal32 __rhs); decimal32 operator +(long long __lhs, decimal32 __rhs); decimal32 operator +(unsigned long long __lhs, decimal32 __rhs);
  decimal64 operator +(decimal32 __lhs, decimal64 __rhs);
  decimal64 operator +(decimal64 __lhs, decimal32 __rhs);
  decimal64 operator +(decimal64 __lhs, decimal64 __rhs);
  decimal64 operator +(decimal64 __lhs, int __rhs); decimal64 operator +(decimal64 __lhs, unsigned int __rhs); decimal64 operator +(decimal64 __lhs, long __rhs); decimal64 operator +(decimal64 __lhs, unsigned long __rhs); decimal64 operator +(decimal64 __lhs, long long __rhs); decimal64 operator +(decimal64 __lhs, unsigned long long __rhs); decimal64 operator +(int __lhs, decimal64 __rhs); decimal64 operator +(unsigned int __lhs, decimal64 __rhs); decimal64 operator +(long __lhs, decimal64 __rhs); decimal64 operator +(unsigned long __lhs, decimal64 __rhs); decimal64 operator +(long long __lhs, decimal64 __rhs); decimal64 operator +(unsigned long long __lhs, decimal64 __rhs);
  decimal128 operator +(decimal32 __lhs, decimal128 __rhs);
  decimal128 operator +(decimal64 __lhs, decimal128 __rhs);
  decimal128 operator +(decimal128 __lhs, decimal32 __rhs);
  decimal128 operator +(decimal128 __lhs, decimal64 __rhs);
  decimal128 operator +(decimal128 __lhs, decimal128 __rhs);
  decimal128 operator +(decimal128 __lhs, int __rhs); decimal128 operator +(decimal128 __lhs, unsigned int __rhs); decimal128 operator +(decimal128 __lhs, long __rhs); decimal128 operator +(decimal128 __lhs, unsigned long __rhs); decimal128 operator +(decimal128 __lhs, long long __rhs); decimal128 operator +(decimal128 __lhs, unsigned long long __rhs); decimal128 operator +(int __lhs, decimal128 __rhs); decimal128 operator +(unsigned int __lhs, decimal128 __rhs); decimal128 operator +(long __lhs, decimal128 __rhs); decimal128 operator +(unsigned long __lhs, decimal128 __rhs); decimal128 operator +(long long __lhs, decimal128 __rhs); decimal128 operator +(unsigned long long __lhs, decimal128 __rhs);

  decimal32 operator -(decimal32 __lhs, decimal32 __rhs);
  decimal32 operator -(decimal32 __lhs, int __rhs); decimal32 operator -(decimal32 __lhs, unsigned int __rhs); decimal32 operator -(decimal32 __lhs, long __rhs); decimal32 operator -(decimal32 __lhs, unsigned long __rhs); decimal32 operator -(decimal32 __lhs, long long __rhs); decimal32 operator -(decimal32 __lhs, unsigned long long __rhs); decimal32 operator -(int __lhs, decimal32 __rhs); decimal32 operator -(unsigned int __lhs, decimal32 __rhs); decimal32 operator -(long __lhs, decimal32 __rhs); decimal32 operator -(unsigned long __lhs, decimal32 __rhs); decimal32 operator -(long long __lhs, decimal32 __rhs); decimal32 operator -(unsigned long long __lhs, decimal32 __rhs);
  decimal64 operator -(decimal32 __lhs, decimal64 __rhs);
  decimal64 operator -(decimal64 __lhs, decimal32 __rhs);
  decimal64 operator -(decimal64 __lhs, decimal64 __rhs);
  decimal64 operator -(decimal64 __lhs, int __rhs); decimal64 operator -(decimal64 __lhs, unsigned int __rhs); decimal64 operator -(decimal64 __lhs, long __rhs); decimal64 operator -(decimal64 __lhs, unsigned long __rhs); decimal64 operator -(decimal64 __lhs, long long __rhs); decimal64 operator -(decimal64 __lhs, unsigned long long __rhs); decimal64 operator -(int __lhs, decimal64 __rhs); decimal64 operator -(unsigned int __lhs, decimal64 __rhs); decimal64 operator -(long __lhs, decimal64 __rhs); decimal64 operator -(unsigned long __lhs, decimal64 __rhs); decimal64 operator -(long long __lhs, decimal64 __rhs); decimal64 operator -(unsigned long long __lhs, decimal64 __rhs);
  decimal128 operator -(decimal32 __lhs, decimal128 __rhs);
  decimal128 operator -(decimal64 __lhs, decimal128 __rhs);
  decimal128 operator -(decimal128 __lhs, decimal32 __rhs);
  decimal128 operator -(decimal128 __lhs, decimal64 __rhs);
  decimal128 operator -(decimal128 __lhs, decimal128 __rhs);
  decimal128 operator -(decimal128 __lhs, int __rhs); decimal128 operator -(decimal128 __lhs, unsigned int __rhs); decimal128 operator -(decimal128 __lhs, long __rhs); decimal128 operator -(decimal128 __lhs, unsigned long __rhs); decimal128 operator -(decimal128 __lhs, long long __rhs); decimal128 operator -(decimal128 __lhs, unsigned long long __rhs); decimal128 operator -(int __lhs, decimal128 __rhs); decimal128 operator -(unsigned int __lhs, decimal128 __rhs); decimal128 operator -(long __lhs, decimal128 __rhs); decimal128 operator -(unsigned long __lhs, decimal128 __rhs); decimal128 operator -(long long __lhs, decimal128 __rhs); decimal128 operator -(unsigned long long __lhs, decimal128 __rhs);

  decimal32 operator *(decimal32 __lhs, decimal32 __rhs);
  decimal32 operator *(decimal32 __lhs, int __rhs); decimal32 operator *(decimal32 __lhs, unsigned int __rhs); decimal32 operator *(decimal32 __lhs, long __rhs); decimal32 operator *(decimal32 __lhs, unsigned long __rhs); decimal32 operator *(decimal32 __lhs, long long __rhs); decimal32 operator *(decimal32 __lhs, unsigned long long __rhs); decimal32 operator *(int __lhs, decimal32 __rhs); decimal32 operator *(unsigned int __lhs, decimal32 __rhs); decimal32 operator *(long __lhs, decimal32 __rhs); decimal32 operator *(unsigned long __lhs, decimal32 __rhs); decimal32 operator *(long long __lhs, decimal32 __rhs); decimal32 operator *(unsigned long long __lhs, decimal32 __rhs);
  decimal64 operator *(decimal32 __lhs, decimal64 __rhs);
  decimal64 operator *(decimal64 __lhs, decimal32 __rhs);
  decimal64 operator *(decimal64 __lhs, decimal64 __rhs);
  decimal64 operator *(decimal64 __lhs, int __rhs); decimal64 operator *(decimal64 __lhs, unsigned int __rhs); decimal64 operator *(decimal64 __lhs, long __rhs); decimal64 operator *(decimal64 __lhs, unsigned long __rhs); decimal64 operator *(decimal64 __lhs, long long __rhs); decimal64 operator *(decimal64 __lhs, unsigned long long __rhs); decimal64 operator *(int __lhs, decimal64 __rhs); decimal64 operator *(unsigned int __lhs, decimal64 __rhs); decimal64 operator *(long __lhs, decimal64 __rhs); decimal64 operator *(unsigned long __lhs, decimal64 __rhs); decimal64 operator *(long long __lhs, decimal64 __rhs); decimal64 operator *(unsigned long long __lhs, decimal64 __rhs);
  decimal128 operator *(decimal32 __lhs, decimal128 __rhs);
  decimal128 operator *(decimal64 __lhs, decimal128 __rhs);
  decimal128 operator *(decimal128 __lhs, decimal32 __rhs);
  decimal128 operator *(decimal128 __lhs, decimal64 __rhs);
  decimal128 operator *(decimal128 __lhs, decimal128 __rhs);
  decimal128 operator *(decimal128 __lhs, int __rhs); decimal128 operator *(decimal128 __lhs, unsigned int __rhs); decimal128 operator *(decimal128 __lhs, long __rhs); decimal128 operator *(decimal128 __lhs, unsigned long __rhs); decimal128 operator *(decimal128 __lhs, long long __rhs); decimal128 operator *(decimal128 __lhs, unsigned long long __rhs); decimal128 operator *(int __lhs, decimal128 __rhs); decimal128 operator *(unsigned int __lhs, decimal128 __rhs); decimal128 operator *(long __lhs, decimal128 __rhs); decimal128 operator *(unsigned long __lhs, decimal128 __rhs); decimal128 operator *(long long __lhs, decimal128 __rhs); decimal128 operator *(unsigned long long __lhs, decimal128 __rhs);

  decimal32 operator /(decimal32 __lhs, decimal32 __rhs);
  decimal32 operator /(decimal32 __lhs, int __rhs); decimal32 operator /(decimal32 __lhs, unsigned int __rhs); decimal32 operator /(decimal32 __lhs, long __rhs); decimal32 operator /(decimal32 __lhs, unsigned long __rhs); decimal32 operator /(decimal32 __lhs, long long __rhs); decimal32 operator /(decimal32 __lhs, unsigned long long __rhs); decimal32 operator /(int __lhs, decimal32 __rhs); decimal32 operator /(unsigned int __lhs, decimal32 __rhs); decimal32 operator /(long __lhs, decimal32 __rhs); decimal32 operator /(unsigned long __lhs, decimal32 __rhs); decimal32 operator /(long long __lhs, decimal32 __rhs); decimal32 operator /(unsigned long long __lhs, decimal32 __rhs);
  decimal64 operator /(decimal32 __lhs, decimal64 __rhs);
  decimal64 operator /(decimal64 __lhs, decimal32 __rhs);
  decimal64 operator /(decimal64 __lhs, decimal64 __rhs);
  decimal64 operator /(decimal64 __lhs, int __rhs); decimal64 operator /(decimal64 __lhs, unsigned int __rhs); decimal64 operator /(decimal64 __lhs, long __rhs); decimal64 operator /(decimal64 __lhs, unsigned long __rhs); decimal64 operator /(decimal64 __lhs, long long __rhs); decimal64 operator /(decimal64 __lhs, unsigned long long __rhs); decimal64 operator /(int __lhs, decimal64 __rhs); decimal64 operator /(unsigned int __lhs, decimal64 __rhs); decimal64 operator /(long __lhs, decimal64 __rhs); decimal64 operator /(unsigned long __lhs, decimal64 __rhs); decimal64 operator /(long long __lhs, decimal64 __rhs); decimal64 operator /(unsigned long long __lhs, decimal64 __rhs);
  decimal128 operator /(decimal32 __lhs, decimal128 __rhs);
  decimal128 operator /(decimal64 __lhs, decimal128 __rhs);
  decimal128 operator /(decimal128 __lhs, decimal32 __rhs);
  decimal128 operator /(decimal128 __lhs, decimal64 __rhs);
  decimal128 operator /(decimal128 __lhs, decimal128 __rhs);
  decimal128 operator /(decimal128 __lhs, int __rhs); decimal128 operator /(decimal128 __lhs, unsigned int __rhs); decimal128 operator /(decimal128 __lhs, long __rhs); decimal128 operator /(decimal128 __lhs, unsigned long __rhs); decimal128 operator /(decimal128 __lhs, long long __rhs); decimal128 operator /(decimal128 __lhs, unsigned long long __rhs); decimal128 operator /(int __lhs, decimal128 __rhs); decimal128 operator /(unsigned int __lhs, decimal128 __rhs); decimal128 operator /(long __lhs, decimal128 __rhs); decimal128 operator /(unsigned long __lhs, decimal128 __rhs); decimal128 operator /(long long __lhs, decimal128 __rhs); decimal128 operator /(unsigned long long __lhs, decimal128 __rhs);
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
  bool operator ==(decimal32 __lhs, decimal32 __rhs); bool operator ==(decimal32 __lhs, decimal64 __rhs); bool operator ==(decimal32 __lhs, decimal128 __rhs); bool operator ==(decimal32 __lhs, int __rhs); bool operator ==(decimal32 __lhs, unsigned int __rhs); bool operator ==(decimal32 __lhs, long __rhs); bool operator ==(decimal32 __lhs, unsigned long __rhs); bool operator ==(decimal32 __lhs, long long __rhs); bool operator ==(decimal32 __lhs, unsigned long long __rhs); bool operator ==(int __lhs, decimal32 __rhs); bool operator ==(unsigned int __lhs, decimal32 __rhs); bool operator ==(long __lhs, decimal32 __rhs); bool operator ==(unsigned long __lhs, decimal32 __rhs); bool operator ==(long long __lhs, decimal32 __rhs); bool operator ==(unsigned long long __lhs, decimal32 __rhs);
  bool operator ==(decimal64 __lhs, decimal32 __rhs); bool operator ==(decimal64 __lhs, decimal64 __rhs); bool operator ==(decimal64 __lhs, decimal128 __rhs); bool operator ==(decimal64 __lhs, int __rhs); bool operator ==(decimal64 __lhs, unsigned int __rhs); bool operator ==(decimal64 __lhs, long __rhs); bool operator ==(decimal64 __lhs, unsigned long __rhs); bool operator ==(decimal64 __lhs, long long __rhs); bool operator ==(decimal64 __lhs, unsigned long long __rhs); bool operator ==(int __lhs, decimal64 __rhs); bool operator ==(unsigned int __lhs, decimal64 __rhs); bool operator ==(long __lhs, decimal64 __rhs); bool operator ==(unsigned long __lhs, decimal64 __rhs); bool operator ==(long long __lhs, decimal64 __rhs); bool operator ==(unsigned long long __lhs, decimal64 __rhs);
  bool operator ==(decimal128 __lhs, decimal32 __rhs); bool operator ==(decimal128 __lhs, decimal64 __rhs); bool operator ==(decimal128 __lhs, decimal128 __rhs); bool operator ==(decimal128 __lhs, int __rhs); bool operator ==(decimal128 __lhs, unsigned int __rhs); bool operator ==(decimal128 __lhs, long __rhs); bool operator ==(decimal128 __lhs, unsigned long __rhs); bool operator ==(decimal128 __lhs, long long __rhs); bool operator ==(decimal128 __lhs, unsigned long long __rhs); bool operator ==(int __lhs, decimal128 __rhs); bool operator ==(unsigned int __lhs, decimal128 __rhs); bool operator ==(long __lhs, decimal128 __rhs); bool operator ==(unsigned long __lhs, decimal128 __rhs); bool operator ==(long long __lhs, decimal128 __rhs); bool operator ==(unsigned long long __lhs, decimal128 __rhs);

  bool operator !=(decimal32 __lhs, decimal32 __rhs); bool operator !=(decimal32 __lhs, decimal64 __rhs); bool operator !=(decimal32 __lhs, decimal128 __rhs); bool operator !=(decimal32 __lhs, int __rhs); bool operator !=(decimal32 __lhs, unsigned int __rhs); bool operator !=(decimal32 __lhs, long __rhs); bool operator !=(decimal32 __lhs, unsigned long __rhs); bool operator !=(decimal32 __lhs, long long __rhs); bool operator !=(decimal32 __lhs, unsigned long long __rhs); bool operator !=(int __lhs, decimal32 __rhs); bool operator !=(unsigned int __lhs, decimal32 __rhs); bool operator !=(long __lhs, decimal32 __rhs); bool operator !=(unsigned long __lhs, decimal32 __rhs); bool operator !=(long long __lhs, decimal32 __rhs); bool operator !=(unsigned long long __lhs, decimal32 __rhs);
  bool operator !=(decimal64 __lhs, decimal32 __rhs); bool operator !=(decimal64 __lhs, decimal64 __rhs); bool operator !=(decimal64 __lhs, decimal128 __rhs); bool operator !=(decimal64 __lhs, int __rhs); bool operator !=(decimal64 __lhs, unsigned int __rhs); bool operator !=(decimal64 __lhs, long __rhs); bool operator !=(decimal64 __lhs, unsigned long __rhs); bool operator !=(decimal64 __lhs, long long __rhs); bool operator !=(decimal64 __lhs, unsigned long long __rhs); bool operator !=(int __lhs, decimal64 __rhs); bool operator !=(unsigned int __lhs, decimal64 __rhs); bool operator !=(long __lhs, decimal64 __rhs); bool operator !=(unsigned long __lhs, decimal64 __rhs); bool operator !=(long long __lhs, decimal64 __rhs); bool operator !=(unsigned long long __lhs, decimal64 __rhs);
  bool operator !=(decimal128 __lhs, decimal32 __rhs); bool operator !=(decimal128 __lhs, decimal64 __rhs); bool operator !=(decimal128 __lhs, decimal128 __rhs); bool operator !=(decimal128 __lhs, int __rhs); bool operator !=(decimal128 __lhs, unsigned int __rhs); bool operator !=(decimal128 __lhs, long __rhs); bool operator !=(decimal128 __lhs, unsigned long __rhs); bool operator !=(decimal128 __lhs, long long __rhs); bool operator !=(decimal128 __lhs, unsigned long long __rhs); bool operator !=(int __lhs, decimal128 __rhs); bool operator !=(unsigned int __lhs, decimal128 __rhs); bool operator !=(long __lhs, decimal128 __rhs); bool operator !=(unsigned long __lhs, decimal128 __rhs); bool operator !=(long long __lhs, decimal128 __rhs); bool operator !=(unsigned long long __lhs, decimal128 __rhs);

  bool operator <(decimal32 __lhs, decimal32 __rhs); bool operator <(decimal32 __lhs, decimal64 __rhs); bool operator <(decimal32 __lhs, decimal128 __rhs); bool operator <(decimal32 __lhs, int __rhs); bool operator <(decimal32 __lhs, unsigned int __rhs); bool operator <(decimal32 __lhs, long __rhs); bool operator <(decimal32 __lhs, unsigned long __rhs); bool operator <(decimal32 __lhs, long long __rhs); bool operator <(decimal32 __lhs, unsigned long long __rhs); bool operator <(int __lhs, decimal32 __rhs); bool operator <(unsigned int __lhs, decimal32 __rhs); bool operator <(long __lhs, decimal32 __rhs); bool operator <(unsigned long __lhs, decimal32 __rhs); bool operator <(long long __lhs, decimal32 __rhs); bool operator <(unsigned long long __lhs, decimal32 __rhs);
  bool operator <(decimal64 __lhs, decimal32 __rhs); bool operator <(decimal64 __lhs, decimal64 __rhs); bool operator <(decimal64 __lhs, decimal128 __rhs); bool operator <(decimal64 __lhs, int __rhs); bool operator <(decimal64 __lhs, unsigned int __rhs); bool operator <(decimal64 __lhs, long __rhs); bool operator <(decimal64 __lhs, unsigned long __rhs); bool operator <(decimal64 __lhs, long long __rhs); bool operator <(decimal64 __lhs, unsigned long long __rhs); bool operator <(int __lhs, decimal64 __rhs); bool operator <(unsigned int __lhs, decimal64 __rhs); bool operator <(long __lhs, decimal64 __rhs); bool operator <(unsigned long __lhs, decimal64 __rhs); bool operator <(long long __lhs, decimal64 __rhs); bool operator <(unsigned long long __lhs, decimal64 __rhs);
  bool operator <(decimal128 __lhs, decimal32 __rhs); bool operator <(decimal128 __lhs, decimal64 __rhs); bool operator <(decimal128 __lhs, decimal128 __rhs); bool operator <(decimal128 __lhs, int __rhs); bool operator <(decimal128 __lhs, unsigned int __rhs); bool operator <(decimal128 __lhs, long __rhs); bool operator <(decimal128 __lhs, unsigned long __rhs); bool operator <(decimal128 __lhs, long long __rhs); bool operator <(decimal128 __lhs, unsigned long long __rhs); bool operator <(int __lhs, decimal128 __rhs); bool operator <(unsigned int __lhs, decimal128 __rhs); bool operator <(long __lhs, decimal128 __rhs); bool operator <(unsigned long __lhs, decimal128 __rhs); bool operator <(long long __lhs, decimal128 __rhs); bool operator <(unsigned long long __lhs, decimal128 __rhs);

  bool operator >=(decimal32 __lhs, decimal32 __rhs); bool operator >=(decimal32 __lhs, decimal64 __rhs); bool operator >=(decimal32 __lhs, decimal128 __rhs); bool operator >=(decimal32 __lhs, int __rhs); bool operator >=(decimal32 __lhs, unsigned int __rhs); bool operator >=(decimal32 __lhs, long __rhs); bool operator >=(decimal32 __lhs, unsigned long __rhs); bool operator >=(decimal32 __lhs, long long __rhs); bool operator >=(decimal32 __lhs, unsigned long long __rhs); bool operator >=(int __lhs, decimal32 __rhs); bool operator >=(unsigned int __lhs, decimal32 __rhs); bool operator >=(long __lhs, decimal32 __rhs); bool operator >=(unsigned long __lhs, decimal32 __rhs); bool operator >=(long long __lhs, decimal32 __rhs); bool operator >=(unsigned long long __lhs, decimal32 __rhs);
  bool operator >=(decimal64 __lhs, decimal32 __rhs); bool operator >=(decimal64 __lhs, decimal64 __rhs); bool operator >=(decimal64 __lhs, decimal128 __rhs); bool operator >=(decimal64 __lhs, int __rhs); bool operator >=(decimal64 __lhs, unsigned int __rhs); bool operator >=(decimal64 __lhs, long __rhs); bool operator >=(decimal64 __lhs, unsigned long __rhs); bool operator >=(decimal64 __lhs, long long __rhs); bool operator >=(decimal64 __lhs, unsigned long long __rhs); bool operator >=(int __lhs, decimal64 __rhs); bool operator >=(unsigned int __lhs, decimal64 __rhs); bool operator >=(long __lhs, decimal64 __rhs); bool operator >=(unsigned long __lhs, decimal64 __rhs); bool operator >=(long long __lhs, decimal64 __rhs); bool operator >=(unsigned long long __lhs, decimal64 __rhs);
  bool operator >=(decimal128 __lhs, decimal32 __rhs); bool operator >=(decimal128 __lhs, decimal64 __rhs); bool operator >=(decimal128 __lhs, decimal128 __rhs); bool operator >=(decimal128 __lhs, int __rhs); bool operator >=(decimal128 __lhs, unsigned int __rhs); bool operator >=(decimal128 __lhs, long __rhs); bool operator >=(decimal128 __lhs, unsigned long __rhs); bool operator >=(decimal128 __lhs, long long __rhs); bool operator >=(decimal128 __lhs, unsigned long long __rhs); bool operator >=(int __lhs, decimal128 __rhs); bool operator >=(unsigned int __lhs, decimal128 __rhs); bool operator >=(long __lhs, decimal128 __rhs); bool operator >=(unsigned long __lhs, decimal128 __rhs); bool operator >=(long long __lhs, decimal128 __rhs); bool operator >=(unsigned long long __lhs, decimal128 __rhs);

  bool operator >(decimal32 __lhs, decimal32 __rhs); bool operator >(decimal32 __lhs, decimal64 __rhs); bool operator >(decimal32 __lhs, decimal128 __rhs); bool operator >(decimal32 __lhs, int __rhs); bool operator >(decimal32 __lhs, unsigned int __rhs); bool operator >(decimal32 __lhs, long __rhs); bool operator >(decimal32 __lhs, unsigned long __rhs); bool operator >(decimal32 __lhs, long long __rhs); bool operator >(decimal32 __lhs, unsigned long long __rhs); bool operator >(int __lhs, decimal32 __rhs); bool operator >(unsigned int __lhs, decimal32 __rhs); bool operator >(long __lhs, decimal32 __rhs); bool operator >(unsigned long __lhs, decimal32 __rhs); bool operator >(long long __lhs, decimal32 __rhs); bool operator >(unsigned long long __lhs, decimal32 __rhs);
  bool operator >(decimal64 __lhs, decimal32 __rhs); bool operator >(decimal64 __lhs, decimal64 __rhs); bool operator >(decimal64 __lhs, decimal128 __rhs); bool operator >(decimal64 __lhs, int __rhs); bool operator >(decimal64 __lhs, unsigned int __rhs); bool operator >(decimal64 __lhs, long __rhs); bool operator >(decimal64 __lhs, unsigned long __rhs); bool operator >(decimal64 __lhs, long long __rhs); bool operator >(decimal64 __lhs, unsigned long long __rhs); bool operator >(int __lhs, decimal64 __rhs); bool operator >(unsigned int __lhs, decimal64 __rhs); bool operator >(long __lhs, decimal64 __rhs); bool operator >(unsigned long __lhs, decimal64 __rhs); bool operator >(long long __lhs, decimal64 __rhs); bool operator >(unsigned long long __lhs, decimal64 __rhs);
  bool operator >(decimal128 __lhs, decimal32 __rhs); bool operator >(decimal128 __lhs, decimal64 __rhs); bool operator >(decimal128 __lhs, decimal128 __rhs); bool operator >(decimal128 __lhs, int __rhs); bool operator >(decimal128 __lhs, unsigned int __rhs); bool operator >(decimal128 __lhs, long __rhs); bool operator >(decimal128 __lhs, unsigned long __rhs); bool operator >(decimal128 __lhs, long long __rhs); bool operator >(decimal128 __lhs, unsigned long long __rhs); bool operator >(int __lhs, decimal128 __rhs); bool operator >(unsigned int __lhs, decimal128 __rhs); bool operator >(long __lhs, decimal128 __rhs); bool operator >(unsigned long __lhs, decimal128 __rhs); bool operator >(long long __lhs, decimal128 __rhs); bool operator >(unsigned long long __lhs, decimal128 __rhs);

  bool operator >=(decimal32 __lhs, decimal32 __rhs); bool operator >=(decimal32 __lhs, decimal64 __rhs); bool operator >=(decimal32 __lhs, decimal128 __rhs); bool operator >=(decimal32 __lhs, int __rhs); bool operator >=(decimal32 __lhs, unsigned int __rhs); bool operator >=(decimal32 __lhs, long __rhs); bool operator >=(decimal32 __lhs, unsigned long __rhs); bool operator >=(decimal32 __lhs, long long __rhs); bool operator >=(decimal32 __lhs, unsigned long long __rhs); bool operator >=(int __lhs, decimal32 __rhs); bool operator >=(unsigned int __lhs, decimal32 __rhs); bool operator >=(long __lhs, decimal32 __rhs); bool operator >=(unsigned long __lhs, decimal32 __rhs); bool operator >=(long long __lhs, decimal32 __rhs); bool operator >=(unsigned long long __lhs, decimal32 __rhs);
  bool operator >=(decimal64 __lhs, decimal32 __rhs); bool operator >=(decimal64 __lhs, decimal64 __rhs); bool operator >=(decimal64 __lhs, decimal128 __rhs); bool operator >=(decimal64 __lhs, int __rhs); bool operator >=(decimal64 __lhs, unsigned int __rhs); bool operator >=(decimal64 __lhs, long __rhs); bool operator >=(decimal64 __lhs, unsigned long __rhs); bool operator >=(decimal64 __lhs, long long __rhs); bool operator >=(decimal64 __lhs, unsigned long long __rhs); bool operator >=(int __lhs, decimal64 __rhs); bool operator >=(unsigned int __lhs, decimal64 __rhs); bool operator >=(long __lhs, decimal64 __rhs); bool operator >=(unsigned long __lhs, decimal64 __rhs); bool operator >=(long long __lhs, decimal64 __rhs); bool operator >=(unsigned long long __lhs, decimal64 __rhs);
  bool operator >=(decimal128 __lhs, decimal32 __rhs); bool operator >=(decimal128 __lhs, decimal64 __rhs); bool operator >=(decimal128 __lhs, decimal128 __rhs); bool operator >=(decimal128 __lhs, int __rhs); bool operator >=(decimal128 __lhs, unsigned int __rhs); bool operator >=(decimal128 __lhs, long __rhs); bool operator >=(decimal128 __lhs, unsigned long __rhs); bool operator >=(decimal128 __lhs, long long __rhs); bool operator >=(decimal128 __lhs, unsigned long long __rhs); bool operator >=(int __lhs, decimal128 __rhs); bool operator >=(unsigned int __lhs, decimal128 __rhs); bool operator >=(long __lhs, decimal128 __rhs); bool operator >=(unsigned long __lhs, decimal128 __rhs); bool operator >=(long long __lhs, decimal128 __rhs); bool operator >=(unsigned long long __lhs, decimal128 __rhs);




  class decimal32
  {
  public:
    typedef float __decfloat32 __attribute__((mode(SD)));


    decimal32() : __val(0.e-101DF) {}


    explicit decimal32(decimal64 __d64);
    explicit decimal32(decimal128 __d128);
    explicit decimal32(float __r) : __val(__r) {}
    explicit decimal32(double __r) : __val(__r) {}
    explicit decimal32(long double __r) : __val(__r) {}


    decimal32(int __z) : __val(__z) {}
    decimal32(unsigned int __z) : __val(__z) {}
    decimal32(long __z) : __val(__z) {}
    decimal32(unsigned long __z) : __val(__z) {}
    decimal32(long long __z) : __val(__z) {}
    decimal32(unsigned long long __z) : __val(__z) {}


    decimal32(__decfloat32 __z) : __val(__z) {}




    explicit operator long long() const { return (long long)__val; }



    decimal32& operator++()
    {
      __val += 1;
      return *this;
    }

    decimal32 operator++(int)
    {
      decimal32 __tmp = *this;
      __val += 1;
      return __tmp;
    }

    decimal32& operator--()
    {
      __val -= 1;
      return *this;
    }

    decimal32 operator--(int)
    {
      decimal32 __tmp = *this;
      __val -= 1;
      return __tmp;
    }
# 303 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
    decimal32& operator +=(decimal32 __rhs); decimal32& operator +=(decimal64 __rhs); decimal32& operator +=(decimal128 __rhs); decimal32& operator +=(int __rhs); decimal32& operator +=(unsigned int __rhs); decimal32& operator +=(long __rhs); decimal32& operator +=(unsigned long __rhs); decimal32& operator +=(long long __rhs); decimal32& operator +=(unsigned long long __rhs);
    decimal32& operator -=(decimal32 __rhs); decimal32& operator -=(decimal64 __rhs); decimal32& operator -=(decimal128 __rhs); decimal32& operator -=(int __rhs); decimal32& operator -=(unsigned int __rhs); decimal32& operator -=(long __rhs); decimal32& operator -=(unsigned long __rhs); decimal32& operator -=(long long __rhs); decimal32& operator -=(unsigned long long __rhs);
    decimal32& operator *=(decimal32 __rhs); decimal32& operator *=(decimal64 __rhs); decimal32& operator *=(decimal128 __rhs); decimal32& operator *=(int __rhs); decimal32& operator *=(unsigned int __rhs); decimal32& operator *=(long __rhs); decimal32& operator *=(unsigned long __rhs); decimal32& operator *=(long long __rhs); decimal32& operator *=(unsigned long long __rhs);
    decimal32& operator /=(decimal32 __rhs); decimal32& operator /=(decimal64 __rhs); decimal32& operator /=(decimal128 __rhs); decimal32& operator /=(int __rhs); decimal32& operator /=(unsigned int __rhs); decimal32& operator /=(long __rhs); decimal32& operator /=(unsigned long __rhs); decimal32& operator /=(long long __rhs); decimal32& operator /=(unsigned long long __rhs);


  private:
    __decfloat32 __val;

  public:
    __decfloat32 __getval(void) { return __val; }
    void __setval(__decfloat32 __x) { __val = __x; }
  };


  class decimal64
  {
  public:
    typedef float __decfloat64 __attribute__((mode(DD)));


    decimal64() : __val(0.e-398dd) {}


      decimal64(decimal32 d32);
    explicit decimal64(decimal128 d128);
    explicit decimal64(float __r) : __val(__r) {}
    explicit decimal64(double __r) : __val(__r) {}
    explicit decimal64(long double __r) : __val(__r) {}


    decimal64(int __z) : __val(__z) {}
    decimal64(unsigned int __z) : __val(__z) {}
    decimal64(long __z) : __val(__z) {}
    decimal64(unsigned long __z) : __val(__z) {}
    decimal64(long long __z) : __val(__z) {}
    decimal64(unsigned long long __z) : __val(__z) {}


    decimal64(__decfloat64 __z) : __val(__z) {}




    explicit operator long long() const { return (long long)__val; }



    decimal64& operator++()
    {
      __val += 1;
      return *this;
    }

    decimal64 operator++(int)
    {
      decimal64 __tmp = *this;
      __val += 1;
      return __tmp;
    }

    decimal64& operator--()
    {
      __val -= 1;
      return *this;
    }

    decimal64 operator--(int)
    {
      decimal64 __tmp = *this;
      __val -= 1;
      return __tmp;
    }
# 389 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
    decimal64& operator +=(decimal32 __rhs); decimal64& operator +=(decimal64 __rhs); decimal64& operator +=(decimal128 __rhs); decimal64& operator +=(int __rhs); decimal64& operator +=(unsigned int __rhs); decimal64& operator +=(long __rhs); decimal64& operator +=(unsigned long __rhs); decimal64& operator +=(long long __rhs); decimal64& operator +=(unsigned long long __rhs);
    decimal64& operator -=(decimal32 __rhs); decimal64& operator -=(decimal64 __rhs); decimal64& operator -=(decimal128 __rhs); decimal64& operator -=(int __rhs); decimal64& operator -=(unsigned int __rhs); decimal64& operator -=(long __rhs); decimal64& operator -=(unsigned long __rhs); decimal64& operator -=(long long __rhs); decimal64& operator -=(unsigned long long __rhs);
    decimal64& operator *=(decimal32 __rhs); decimal64& operator *=(decimal64 __rhs); decimal64& operator *=(decimal128 __rhs); decimal64& operator *=(int __rhs); decimal64& operator *=(unsigned int __rhs); decimal64& operator *=(long __rhs); decimal64& operator *=(unsigned long __rhs); decimal64& operator *=(long long __rhs); decimal64& operator *=(unsigned long long __rhs);
    decimal64& operator /=(decimal32 __rhs); decimal64& operator /=(decimal64 __rhs); decimal64& operator /=(decimal128 __rhs); decimal64& operator /=(int __rhs); decimal64& operator /=(unsigned int __rhs); decimal64& operator /=(long __rhs); decimal64& operator /=(unsigned long __rhs); decimal64& operator /=(long long __rhs); decimal64& operator /=(unsigned long long __rhs);


  private:
    __decfloat64 __val;

  public:
    __decfloat64 __getval(void) { return __val; }
    void __setval(__decfloat64 __x) { __val = __x; }
  };


  class decimal128
  {
  public:
    typedef float __decfloat128 __attribute__((mode(TD)));


    decimal128() : __val(0.e-6176DL) {}


      decimal128(decimal32 d32);
      decimal128(decimal64 d64);
    explicit decimal128(float __r) : __val(__r) {}
    explicit decimal128(double __r) : __val(__r) {}
    explicit decimal128(long double __r) : __val(__r) {}



    decimal128(int __z) : __val(__z) {}
    decimal128(unsigned int __z) : __val(__z) {}
    decimal128(long __z) : __val(__z) {}
    decimal128(unsigned long __z) : __val(__z) {}
    decimal128(long long __z) : __val(__z) {}
    decimal128(unsigned long long __z) : __val(__z) {}


    decimal128(__decfloat128 __z) : __val(__z) {}




    explicit operator long long() const { return (long long)__val; }



    decimal128& operator++()
    {
      __val += 1;
      return *this;
    }

    decimal128 operator++(int)
    {
      decimal128 __tmp = *this;
      __val += 1;
      return __tmp;
    }

    decimal128& operator--()
    {
      __val -= 1;
      return *this;
    }

    decimal128 operator--(int)
    {
      decimal128 __tmp = *this;
      __val -= 1;
      return __tmp;
    }
# 476 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 3
    decimal128& operator +=(decimal32 __rhs); decimal128& operator +=(decimal64 __rhs); decimal128& operator +=(decimal128 __rhs); decimal128& operator +=(int __rhs); decimal128& operator +=(unsigned int __rhs); decimal128& operator +=(long __rhs); decimal128& operator +=(unsigned long __rhs); decimal128& operator +=(long long __rhs); decimal128& operator +=(unsigned long long __rhs);
    decimal128& operator -=(decimal32 __rhs); decimal128& operator -=(decimal64 __rhs); decimal128& operator -=(decimal128 __rhs); decimal128& operator -=(int __rhs); decimal128& operator -=(unsigned int __rhs); decimal128& operator -=(long __rhs); decimal128& operator -=(unsigned long __rhs); decimal128& operator -=(long long __rhs); decimal128& operator -=(unsigned long long __rhs);
    decimal128& operator *=(decimal32 __rhs); decimal128& operator *=(decimal64 __rhs); decimal128& operator *=(decimal128 __rhs); decimal128& operator *=(int __rhs); decimal128& operator *=(unsigned int __rhs); decimal128& operator *=(long __rhs); decimal128& operator *=(unsigned long __rhs); decimal128& operator *=(long long __rhs); decimal128& operator *=(unsigned long long __rhs);
    decimal128& operator /=(decimal32 __rhs); decimal128& operator /=(decimal64 __rhs); decimal128& operator /=(decimal128 __rhs); decimal128& operator /=(int __rhs); decimal128& operator /=(unsigned int __rhs); decimal128& operator /=(long __rhs); decimal128& operator /=(unsigned long __rhs); decimal128& operator /=(long long __rhs); decimal128& operator /=(unsigned long long __rhs);


  private:
    __decfloat128 __val;

  public:
    __decfloat128 __getval(void) { return __val; }
    void __setval(__decfloat128 __x) { __val = __x; }
  };


}



}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{


namespace decimal
{


  inline decimal32::decimal32(decimal64 __r) : __val(__r.__getval()) {}
  inline decimal32::decimal32(decimal128 __r) : __val(__r.__getval()) {}
  inline decimal64::decimal64(decimal32 __r) : __val(__r.__getval()) {}
  inline decimal64::decimal64(decimal128 __r) : __val(__r.__getval()) {}
  inline decimal128::decimal128(decimal32 __r) : __val(__r.__getval()) {}
  inline decimal128::decimal128(decimal64 __r) : __val(__r.__getval()) {}
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal.h" 3
  inline decimal32& decimal32::operator +=(decimal32 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal32& decimal32::operator +=(decimal64 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal32& decimal32::operator +=(decimal128 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal32& decimal32::operator +=(int __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal32& decimal32::operator +=(unsigned int __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal32& decimal32::operator +=(long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal32& decimal32::operator +=(unsigned long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal32& decimal32::operator +=(long long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal32& decimal32::operator +=(unsigned long long __rhs) { __setval(__getval() + __rhs); return *this; }
  inline decimal32& decimal32::operator -=(decimal32 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal32& decimal32::operator -=(decimal64 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal32& decimal32::operator -=(decimal128 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal32& decimal32::operator -=(int __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal32& decimal32::operator -=(unsigned int __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal32& decimal32::operator -=(long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal32& decimal32::operator -=(unsigned long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal32& decimal32::operator -=(long long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal32& decimal32::operator -=(unsigned long long __rhs) { __setval(__getval() - __rhs); return *this; }
  inline decimal32& decimal32::operator *=(decimal32 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal32& decimal32::operator *=(decimal64 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal32& decimal32::operator *=(decimal128 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal32& decimal32::operator *=(int __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal32& decimal32::operator *=(unsigned int __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal32& decimal32::operator *=(long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal32& decimal32::operator *=(unsigned long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal32& decimal32::operator *=(long long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal32& decimal32::operator *=(unsigned long long __rhs) { __setval(__getval() * __rhs); return *this; }
  inline decimal32& decimal32::operator /=(decimal32 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal32& decimal32::operator /=(decimal64 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal32& decimal32::operator /=(decimal128 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal32& decimal32::operator /=(int __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal32& decimal32::operator /=(unsigned int __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal32& decimal32::operator /=(long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal32& decimal32::operator /=(unsigned long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal32& decimal32::operator /=(long long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal32& decimal32::operator /=(unsigned long long __rhs) { __setval(__getval() / __rhs); return *this; }

  inline decimal64& decimal64::operator +=(decimal32 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal64& decimal64::operator +=(decimal64 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal64& decimal64::operator +=(decimal128 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal64& decimal64::operator +=(int __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal64& decimal64::operator +=(unsigned int __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal64& decimal64::operator +=(long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal64& decimal64::operator +=(unsigned long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal64& decimal64::operator +=(long long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal64& decimal64::operator +=(unsigned long long __rhs) { __setval(__getval() + __rhs); return *this; }
  inline decimal64& decimal64::operator -=(decimal32 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal64& decimal64::operator -=(decimal64 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal64& decimal64::operator -=(decimal128 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal64& decimal64::operator -=(int __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal64& decimal64::operator -=(unsigned int __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal64& decimal64::operator -=(long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal64& decimal64::operator -=(unsigned long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal64& decimal64::operator -=(long long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal64& decimal64::operator -=(unsigned long long __rhs) { __setval(__getval() - __rhs); return *this; }
  inline decimal64& decimal64::operator *=(decimal32 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal64& decimal64::operator *=(decimal64 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal64& decimal64::operator *=(decimal128 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal64& decimal64::operator *=(int __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal64& decimal64::operator *=(unsigned int __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal64& decimal64::operator *=(long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal64& decimal64::operator *=(unsigned long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal64& decimal64::operator *=(long long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal64& decimal64::operator *=(unsigned long long __rhs) { __setval(__getval() * __rhs); return *this; }
  inline decimal64& decimal64::operator /=(decimal32 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal64& decimal64::operator /=(decimal64 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal64& decimal64::operator /=(decimal128 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal64& decimal64::operator /=(int __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal64& decimal64::operator /=(unsigned int __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal64& decimal64::operator /=(long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal64& decimal64::operator /=(unsigned long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal64& decimal64::operator /=(long long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal64& decimal64::operator /=(unsigned long long __rhs) { __setval(__getval() / __rhs); return *this; }

  inline decimal128& decimal128::operator +=(decimal32 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal128& decimal128::operator +=(decimal64 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal128& decimal128::operator +=(decimal128 __rhs) { __setval(__getval() + __rhs.__getval()); return *this; } inline decimal128& decimal128::operator +=(int __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal128& decimal128::operator +=(unsigned int __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal128& decimal128::operator +=(long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal128& decimal128::operator +=(unsigned long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal128& decimal128::operator +=(long long __rhs) { __setval(__getval() + __rhs); return *this; } inline decimal128& decimal128::operator +=(unsigned long long __rhs) { __setval(__getval() + __rhs); return *this; }
  inline decimal128& decimal128::operator -=(decimal32 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal128& decimal128::operator -=(decimal64 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal128& decimal128::operator -=(decimal128 __rhs) { __setval(__getval() - __rhs.__getval()); return *this; } inline decimal128& decimal128::operator -=(int __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal128& decimal128::operator -=(unsigned int __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal128& decimal128::operator -=(long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal128& decimal128::operator -=(unsigned long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal128& decimal128::operator -=(long long __rhs) { __setval(__getval() - __rhs); return *this; } inline decimal128& decimal128::operator -=(unsigned long long __rhs) { __setval(__getval() - __rhs); return *this; }
  inline decimal128& decimal128::operator *=(decimal32 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal128& decimal128::operator *=(decimal64 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal128& decimal128::operator *=(decimal128 __rhs) { __setval(__getval() * __rhs.__getval()); return *this; } inline decimal128& decimal128::operator *=(int __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal128& decimal128::operator *=(unsigned int __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal128& decimal128::operator *=(long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal128& decimal128::operator *=(unsigned long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal128& decimal128::operator *=(long long __rhs) { __setval(__getval() * __rhs); return *this; } inline decimal128& decimal128::operator *=(unsigned long long __rhs) { __setval(__getval() * __rhs); return *this; }
  inline decimal128& decimal128::operator /=(decimal32 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal128& decimal128::operator /=(decimal64 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal128& decimal128::operator /=(decimal128 __rhs) { __setval(__getval() / __rhs.__getval()); return *this; } inline decimal128& decimal128::operator /=(int __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal128& decimal128::operator /=(unsigned int __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal128& decimal128::operator /=(long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal128& decimal128::operator /=(unsigned long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal128& decimal128::operator /=(long long __rhs) { __setval(__getval() / __rhs); return *this; } inline decimal128& decimal128::operator /=(unsigned long long __rhs) { __setval(__getval() / __rhs); return *this; }







  inline long long decimal32_to_long_long(decimal32 __d)
  { return (long long)__d.__getval(); }

  inline long long decimal64_to_long_long(decimal64 __d)
  { return (long long)__d.__getval(); }

  inline long long decimal128_to_long_long(decimal128 __d)
  { return (long long)__d.__getval(); }

  inline long long decimal_to_long_long(decimal32 __d)
  { return (long long)__d.__getval(); }

  inline long long decimal_to_long_long(decimal64 __d)
  { return (long long)__d.__getval(); }

  inline long long decimal_to_long_long(decimal128 __d)
  { return (long long)__d.__getval(); }



  static decimal32 make_decimal32(long long __coeff, int __exponent)
  {
    decimal32 __decexp = 1, __multiplier;

    if (__exponent < 0)
      {
 __multiplier = 1.E-1DF;
 __exponent = -__exponent;
      }
    else
      __multiplier = 1.E1DF;

    for (int __i = 0; __i < __exponent; ++__i)
      __decexp *= __multiplier;

    return __coeff * __decexp;
  }

  static decimal32 make_decimal32(unsigned long long __coeff, int __exponent)
  {
    decimal32 __decexp = 1, __multiplier;

    if (__exponent < 0)
      {
 __multiplier = 1.E-1DF;
 __exponent = -__exponent;
      }
    else
      __multiplier = 1.E1DF;

    for (int __i = 0; __i < __exponent; ++__i)
      __decexp *= __multiplier;

    return __coeff * __decexp;
  }

  static decimal64 make_decimal64(long long __coeff, int __exponent)
  {
    decimal64 __decexp = 1, __multiplier;

    if (__exponent < 0)
      {
 __multiplier = 1.E-1DD;
 __exponent = -__exponent;
      }
    else
      __multiplier = 1.E1DD;

    for (int __i = 0; __i < __exponent; ++__i)
      __decexp *= __multiplier;

    return __coeff * __decexp;
  }

  static decimal64 make_decimal64(unsigned long long __coeff, int __exponent)
  {
    decimal64 __decexp = 1, __multiplier;

    if (__exponent < 0)
      {
 __multiplier = 1.E-1DD;
 __exponent = -__exponent;
      }
    else
      __multiplier = 1.E1DD;

    for (int __i = 0; __i < __exponent; ++__i)
      __decexp *= __multiplier;

    return __coeff * __decexp;
  }

  static decimal128 make_decimal128(long long __coeff, int __exponent)
  {
    decimal128 __decexp = 1, __multiplier;

    if (__exponent < 0)
      {
 __multiplier = 1.E-1DL;
 __exponent = -__exponent;
      }
    else
      __multiplier = 1.E1DL;

    for (int __i = 0; __i < __exponent; ++__i)
      __decexp *= __multiplier;

    return __coeff * __decexp;
  }

  static decimal128 make_decimal128(unsigned long long __coeff, int __exponent)
  {
    decimal128 __decexp = 1, __multiplier;

    if (__exponent < 0)
      {
 __multiplier = 1.E-1DL;
 __exponent = -__exponent;
      }
    else
      __multiplier = 1.E1DL;

    for (int __i = 0; __i < __exponent; ++__i)
      __decexp *= __multiplier;

    return __coeff * __decexp;
  }



  inline float decimal32_to_float(decimal32 __d)
  { return (float)__d.__getval(); }

  inline float decimal64_to_float(decimal64 __d)
  { return (float)__d.__getval(); }

  inline float decimal128_to_float(decimal128 __d)
  { return (float)__d.__getval(); }

  inline float decimal_to_float(decimal32 __d)
  { return (float)__d.__getval(); }

  inline float decimal_to_float(decimal64 __d)
  { return (float)__d.__getval(); }

  inline float decimal_to_float(decimal128 __d)
  { return (float)__d.__getval(); }

  inline double decimal32_to_double(decimal32 __d)
  { return (double)__d.__getval(); }

  inline double decimal64_to_double(decimal64 __d)
  { return (double)__d.__getval(); }

  inline double decimal128_to_double(decimal128 __d)
  { return (double)__d.__getval(); }

  inline double decimal_to_double(decimal32 __d)
  { return (double)__d.__getval(); }

  inline double decimal_to_double(decimal64 __d)
  { return (double)__d.__getval(); }

  inline double decimal_to_double(decimal128 __d)
  { return (double)__d.__getval(); }

  inline long double decimal32_to_long_double(decimal32 __d)
  { return (long double)__d.__getval(); }

  inline long double decimal64_to_long_double(decimal64 __d)
  { return (long double)__d.__getval(); }

  inline long double decimal128_to_long_double(decimal128 __d)
  { return (long double)__d.__getval(); }

  inline long double decimal_to_long_double(decimal32 __d)
  { return (long double)__d.__getval(); }

  inline long double decimal_to_long_double(decimal64 __d)
  { return (long double)__d.__getval(); }

  inline long double decimal_to_long_double(decimal128 __d)
  { return (long double)__d.__getval(); }
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal.h" 3
  inline decimal32 operator +(decimal32 __rhs) { decimal32 __tmp; __tmp.__setval(+ __rhs.__getval()); return __tmp; }
  inline decimal64 operator +(decimal64 __rhs) { decimal64 __tmp; __tmp.__setval(+ __rhs.__getval()); return __tmp; }
  inline decimal128 operator +(decimal128 __rhs) { decimal128 __tmp; __tmp.__setval(+ __rhs.__getval()); return __tmp; }
  inline decimal32 operator -(decimal32 __rhs) { decimal32 __tmp; __tmp.__setval(- __rhs.__getval()); return __tmp; }
  inline decimal64 operator -(decimal64 __rhs) { decimal64 __tmp; __tmp.__setval(- __rhs.__getval()); return __tmp; }
  inline decimal128 operator -(decimal128 __rhs) { decimal128 __tmp; __tmp.__setval(- __rhs.__getval()); return __tmp; }
# 354 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal.h" 3
  inline decimal32 operator +(decimal32 __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal32 operator +(decimal32 __lhs, int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal32 operator +(decimal32 __lhs, unsigned int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal32 operator +(decimal32 __lhs, long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal32 operator +(decimal32 __lhs, unsigned long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal32 operator +(decimal32 __lhs, long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal32 operator +(decimal32 __lhs, unsigned long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal32 operator +(int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal32 operator +(unsigned int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal32 operator +(long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal32 operator +(unsigned long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal32 operator +(long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal32 operator +(unsigned long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; }
  inline decimal64 operator +(decimal32 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal64 operator +(decimal64 __lhs, decimal32 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal64 operator +(decimal64 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal64 operator +(decimal64 __lhs, int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal64 operator +(decimal64 __lhs, unsigned int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal64 operator +(decimal64 __lhs, long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal64 operator +(decimal64 __lhs, unsigned long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal64 operator +(decimal64 __lhs, long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal64 operator +(decimal64 __lhs, unsigned long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal64 operator +(int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal64 operator +(unsigned int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal64 operator +(long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal64 operator +(unsigned long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal64 operator +(long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal64 operator +(unsigned long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; }
  inline decimal128 operator +(decimal32 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal128 operator +(decimal64 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal128 operator +(decimal128 __lhs, decimal32 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal128 operator +(decimal128 __lhs, decimal64 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal128 operator +(decimal128 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs.__getval()); return __retval; }
  inline decimal128 operator +(decimal128 __lhs, int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal128 operator +(decimal128 __lhs, unsigned int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal128 operator +(decimal128 __lhs, long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal128 operator +(decimal128 __lhs, unsigned long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal128 operator +(decimal128 __lhs, long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal128 operator +(decimal128 __lhs, unsigned long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() + __rhs); return __retval; } inline decimal128 operator +(int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal128 operator +(unsigned int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal128 operator +(long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal128 operator +(unsigned long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal128 operator +(long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; } inline decimal128 operator +(unsigned long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs + __rhs.__getval()); return __retval; }

  inline decimal32 operator -(decimal32 __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal32 operator -(decimal32 __lhs, int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal32 operator -(decimal32 __lhs, unsigned int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal32 operator -(decimal32 __lhs, long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal32 operator -(decimal32 __lhs, unsigned long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal32 operator -(decimal32 __lhs, long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal32 operator -(decimal32 __lhs, unsigned long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal32 operator -(int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal32 operator -(unsigned int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal32 operator -(long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal32 operator -(unsigned long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal32 operator -(long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal32 operator -(unsigned long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; }
  inline decimal64 operator -(decimal32 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal64 operator -(decimal64 __lhs, decimal32 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal64 operator -(decimal64 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal64 operator -(decimal64 __lhs, int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal64 operator -(decimal64 __lhs, unsigned int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal64 operator -(decimal64 __lhs, long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal64 operator -(decimal64 __lhs, unsigned long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal64 operator -(decimal64 __lhs, long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal64 operator -(decimal64 __lhs, unsigned long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal64 operator -(int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal64 operator -(unsigned int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal64 operator -(long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal64 operator -(unsigned long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal64 operator -(long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal64 operator -(unsigned long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; }
  inline decimal128 operator -(decimal32 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal128 operator -(decimal64 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal128 operator -(decimal128 __lhs, decimal32 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal128 operator -(decimal128 __lhs, decimal64 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal128 operator -(decimal128 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs.__getval()); return __retval; }
  inline decimal128 operator -(decimal128 __lhs, int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal128 operator -(decimal128 __lhs, unsigned int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal128 operator -(decimal128 __lhs, long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal128 operator -(decimal128 __lhs, unsigned long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal128 operator -(decimal128 __lhs, long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal128 operator -(decimal128 __lhs, unsigned long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() - __rhs); return __retval; } inline decimal128 operator -(int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal128 operator -(unsigned int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal128 operator -(long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal128 operator -(unsigned long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal128 operator -(long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; } inline decimal128 operator -(unsigned long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs - __rhs.__getval()); return __retval; }

  inline decimal32 operator *(decimal32 __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal32 operator *(decimal32 __lhs, int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal32 operator *(decimal32 __lhs, unsigned int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal32 operator *(decimal32 __lhs, long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal32 operator *(decimal32 __lhs, unsigned long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal32 operator *(decimal32 __lhs, long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal32 operator *(decimal32 __lhs, unsigned long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal32 operator *(int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal32 operator *(unsigned int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal32 operator *(long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal32 operator *(unsigned long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal32 operator *(long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal32 operator *(unsigned long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; }
  inline decimal64 operator *(decimal32 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal64 operator *(decimal64 __lhs, decimal32 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal64 operator *(decimal64 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal64 operator *(decimal64 __lhs, int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal64 operator *(decimal64 __lhs, unsigned int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal64 operator *(decimal64 __lhs, long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal64 operator *(decimal64 __lhs, unsigned long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal64 operator *(decimal64 __lhs, long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal64 operator *(decimal64 __lhs, unsigned long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal64 operator *(int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal64 operator *(unsigned int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal64 operator *(long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal64 operator *(unsigned long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal64 operator *(long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal64 operator *(unsigned long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; }
  inline decimal128 operator *(decimal32 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal128 operator *(decimal64 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal128 operator *(decimal128 __lhs, decimal32 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal128 operator *(decimal128 __lhs, decimal64 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal128 operator *(decimal128 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs.__getval()); return __retval; }
  inline decimal128 operator *(decimal128 __lhs, int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal128 operator *(decimal128 __lhs, unsigned int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal128 operator *(decimal128 __lhs, long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal128 operator *(decimal128 __lhs, unsigned long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal128 operator *(decimal128 __lhs, long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal128 operator *(decimal128 __lhs, unsigned long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() * __rhs); return __retval; } inline decimal128 operator *(int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal128 operator *(unsigned int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal128 operator *(long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal128 operator *(unsigned long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal128 operator *(long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; } inline decimal128 operator *(unsigned long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs * __rhs.__getval()); return __retval; }

  inline decimal32 operator /(decimal32 __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal32 operator /(decimal32 __lhs, int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal32 operator /(decimal32 __lhs, unsigned int __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal32 operator /(decimal32 __lhs, long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal32 operator /(decimal32 __lhs, unsigned long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal32 operator /(decimal32 __lhs, long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal32 operator /(decimal32 __lhs, unsigned long long __rhs) { decimal32 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal32 operator /(int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal32 operator /(unsigned int __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal32 operator /(long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal32 operator /(unsigned long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal32 operator /(long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal32 operator /(unsigned long long __lhs, decimal32 __rhs) { decimal32 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; }
  inline decimal64 operator /(decimal32 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal64 operator /(decimal64 __lhs, decimal32 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal64 operator /(decimal64 __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal64 operator /(decimal64 __lhs, int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal64 operator /(decimal64 __lhs, unsigned int __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal64 operator /(decimal64 __lhs, long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal64 operator /(decimal64 __lhs, unsigned long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal64 operator /(decimal64 __lhs, long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal64 operator /(decimal64 __lhs, unsigned long long __rhs) { decimal64 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal64 operator /(int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal64 operator /(unsigned int __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal64 operator /(long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal64 operator /(unsigned long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal64 operator /(long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal64 operator /(unsigned long long __lhs, decimal64 __rhs) { decimal64 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; }
  inline decimal128 operator /(decimal32 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal128 operator /(decimal64 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal128 operator /(decimal128 __lhs, decimal32 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal128 operator /(decimal128 __lhs, decimal64 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal128 operator /(decimal128 __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs.__getval()); return __retval; }
  inline decimal128 operator /(decimal128 __lhs, int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal128 operator /(decimal128 __lhs, unsigned int __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal128 operator /(decimal128 __lhs, long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal128 operator /(decimal128 __lhs, unsigned long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal128 operator /(decimal128 __lhs, long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal128 operator /(decimal128 __lhs, unsigned long long __rhs) { decimal128 __retval; __retval.__setval(__lhs.__getval() / __rhs); return __retval; } inline decimal128 operator /(int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal128 operator /(unsigned int __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal128 operator /(long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal128 operator /(unsigned long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal128 operator /(long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; } inline decimal128 operator /(unsigned long long __lhs, decimal128 __rhs) { decimal128 __retval; __retval.__setval(__lhs / __rhs.__getval()); return __retval; }
# 443 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal.h" 3
  inline bool operator ==(decimal32 __lhs, decimal32 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal32 __lhs, decimal64 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal32 __lhs, decimal128 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal32 __lhs, int __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal32 __lhs, unsigned int __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal32 __lhs, long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal32 __lhs, unsigned long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal32 __lhs, long long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal32 __lhs, unsigned long long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(int __lhs, decimal32 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned int __lhs, decimal32 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(long __lhs, decimal32 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned long __lhs, decimal32 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(long long __lhs, decimal32 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned long long __lhs, decimal32 __rhs) { return __lhs == __rhs.__getval(); }
  inline bool operator ==(decimal64 __lhs, decimal32 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal64 __lhs, decimal64 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal64 __lhs, decimal128 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal64 __lhs, int __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal64 __lhs, unsigned int __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal64 __lhs, long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal64 __lhs, unsigned long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal64 __lhs, long long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal64 __lhs, unsigned long long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(int __lhs, decimal64 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned int __lhs, decimal64 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(long __lhs, decimal64 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned long __lhs, decimal64 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(long long __lhs, decimal64 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned long long __lhs, decimal64 __rhs) { return __lhs == __rhs.__getval(); }
  inline bool operator ==(decimal128 __lhs, decimal32 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal128 __lhs, decimal64 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal128 __lhs, decimal128 __rhs) { return __lhs.__getval() == __rhs.__getval(); } inline bool operator ==(decimal128 __lhs, int __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal128 __lhs, unsigned int __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal128 __lhs, long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal128 __lhs, unsigned long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal128 __lhs, long long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(decimal128 __lhs, unsigned long long __rhs) { return __lhs.__getval() == __rhs; } inline bool operator ==(int __lhs, decimal128 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned int __lhs, decimal128 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(long __lhs, decimal128 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned long __lhs, decimal128 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(long long __lhs, decimal128 __rhs) { return __lhs == __rhs.__getval(); } inline bool operator ==(unsigned long long __lhs, decimal128 __rhs) { return __lhs == __rhs.__getval(); }
  inline bool operator !=(decimal32 __lhs, decimal32 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal32 __lhs, decimal64 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal32 __lhs, decimal128 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal32 __lhs, int __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal32 __lhs, unsigned int __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal32 __lhs, long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal32 __lhs, unsigned long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal32 __lhs, long long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal32 __lhs, unsigned long long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(int __lhs, decimal32 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned int __lhs, decimal32 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(long __lhs, decimal32 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned long __lhs, decimal32 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(long long __lhs, decimal32 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned long long __lhs, decimal32 __rhs) { return __lhs != __rhs.__getval(); }
  inline bool operator !=(decimal64 __lhs, decimal32 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal64 __lhs, decimal64 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal64 __lhs, decimal128 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal64 __lhs, int __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal64 __lhs, unsigned int __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal64 __lhs, long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal64 __lhs, unsigned long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal64 __lhs, long long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal64 __lhs, unsigned long long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(int __lhs, decimal64 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned int __lhs, decimal64 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(long __lhs, decimal64 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned long __lhs, decimal64 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(long long __lhs, decimal64 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned long long __lhs, decimal64 __rhs) { return __lhs != __rhs.__getval(); }
  inline bool operator !=(decimal128 __lhs, decimal32 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal128 __lhs, decimal64 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal128 __lhs, decimal128 __rhs) { return __lhs.__getval() != __rhs.__getval(); } inline bool operator !=(decimal128 __lhs, int __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal128 __lhs, unsigned int __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal128 __lhs, long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal128 __lhs, unsigned long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal128 __lhs, long long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(decimal128 __lhs, unsigned long long __rhs) { return __lhs.__getval() != __rhs; } inline bool operator !=(int __lhs, decimal128 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned int __lhs, decimal128 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(long __lhs, decimal128 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned long __lhs, decimal128 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(long long __lhs, decimal128 __rhs) { return __lhs != __rhs.__getval(); } inline bool operator !=(unsigned long long __lhs, decimal128 __rhs) { return __lhs != __rhs.__getval(); }
  inline bool operator <(decimal32 __lhs, decimal32 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal32 __lhs, decimal64 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal32 __lhs, decimal128 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal32 __lhs, int __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal32 __lhs, unsigned int __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal32 __lhs, long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal32 __lhs, unsigned long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal32 __lhs, long long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal32 __lhs, unsigned long long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(int __lhs, decimal32 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned int __lhs, decimal32 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(long __lhs, decimal32 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned long __lhs, decimal32 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(long long __lhs, decimal32 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned long long __lhs, decimal32 __rhs) { return __lhs < __rhs.__getval(); }
  inline bool operator <(decimal64 __lhs, decimal32 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal64 __lhs, decimal64 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal64 __lhs, decimal128 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal64 __lhs, int __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal64 __lhs, unsigned int __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal64 __lhs, long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal64 __lhs, unsigned long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal64 __lhs, long long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal64 __lhs, unsigned long long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(int __lhs, decimal64 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned int __lhs, decimal64 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(long __lhs, decimal64 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned long __lhs, decimal64 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(long long __lhs, decimal64 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned long long __lhs, decimal64 __rhs) { return __lhs < __rhs.__getval(); }
  inline bool operator <(decimal128 __lhs, decimal32 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal128 __lhs, decimal64 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal128 __lhs, decimal128 __rhs) { return __lhs.__getval() < __rhs.__getval(); } inline bool operator <(decimal128 __lhs, int __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal128 __lhs, unsigned int __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal128 __lhs, long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal128 __lhs, unsigned long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal128 __lhs, long long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(decimal128 __lhs, unsigned long long __rhs) { return __lhs.__getval() < __rhs; } inline bool operator <(int __lhs, decimal128 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned int __lhs, decimal128 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(long __lhs, decimal128 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned long __lhs, decimal128 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(long long __lhs, decimal128 __rhs) { return __lhs < __rhs.__getval(); } inline bool operator <(unsigned long long __lhs, decimal128 __rhs) { return __lhs < __rhs.__getval(); }
  inline bool operator <=(decimal32 __lhs, decimal32 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal32 __lhs, decimal64 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal32 __lhs, decimal128 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal32 __lhs, int __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal32 __lhs, unsigned int __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal32 __lhs, long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal32 __lhs, unsigned long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal32 __lhs, long long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal32 __lhs, unsigned long long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(int __lhs, decimal32 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned int __lhs, decimal32 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(long __lhs, decimal32 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned long __lhs, decimal32 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(long long __lhs, decimal32 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned long long __lhs, decimal32 __rhs) { return __lhs <= __rhs.__getval(); }
  inline bool operator <=(decimal64 __lhs, decimal32 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal64 __lhs, decimal64 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal64 __lhs, decimal128 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal64 __lhs, int __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal64 __lhs, unsigned int __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal64 __lhs, long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal64 __lhs, unsigned long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal64 __lhs, long long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal64 __lhs, unsigned long long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(int __lhs, decimal64 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned int __lhs, decimal64 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(long __lhs, decimal64 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned long __lhs, decimal64 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(long long __lhs, decimal64 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned long long __lhs, decimal64 __rhs) { return __lhs <= __rhs.__getval(); }
  inline bool operator <=(decimal128 __lhs, decimal32 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal128 __lhs, decimal64 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal128 __lhs, decimal128 __rhs) { return __lhs.__getval() <= __rhs.__getval(); } inline bool operator <=(decimal128 __lhs, int __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal128 __lhs, unsigned int __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal128 __lhs, long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal128 __lhs, unsigned long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal128 __lhs, long long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(decimal128 __lhs, unsigned long long __rhs) { return __lhs.__getval() <= __rhs; } inline bool operator <=(int __lhs, decimal128 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned int __lhs, decimal128 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(long __lhs, decimal128 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned long __lhs, decimal128 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(long long __lhs, decimal128 __rhs) { return __lhs <= __rhs.__getval(); } inline bool operator <=(unsigned long long __lhs, decimal128 __rhs) { return __lhs <= __rhs.__getval(); }
  inline bool operator >(decimal32 __lhs, decimal32 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal32 __lhs, decimal64 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal32 __lhs, decimal128 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal32 __lhs, int __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal32 __lhs, unsigned int __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal32 __lhs, long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal32 __lhs, unsigned long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal32 __lhs, long long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal32 __lhs, unsigned long long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(int __lhs, decimal32 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned int __lhs, decimal32 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(long __lhs, decimal32 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned long __lhs, decimal32 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(long long __lhs, decimal32 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned long long __lhs, decimal32 __rhs) { return __lhs > __rhs.__getval(); }
  inline bool operator >(decimal64 __lhs, decimal32 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal64 __lhs, decimal64 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal64 __lhs, decimal128 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal64 __lhs, int __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal64 __lhs, unsigned int __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal64 __lhs, long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal64 __lhs, unsigned long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal64 __lhs, long long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal64 __lhs, unsigned long long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(int __lhs, decimal64 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned int __lhs, decimal64 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(long __lhs, decimal64 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned long __lhs, decimal64 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(long long __lhs, decimal64 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned long long __lhs, decimal64 __rhs) { return __lhs > __rhs.__getval(); }
  inline bool operator >(decimal128 __lhs, decimal32 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal128 __lhs, decimal64 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal128 __lhs, decimal128 __rhs) { return __lhs.__getval() > __rhs.__getval(); } inline bool operator >(decimal128 __lhs, int __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal128 __lhs, unsigned int __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal128 __lhs, long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal128 __lhs, unsigned long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal128 __lhs, long long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(decimal128 __lhs, unsigned long long __rhs) { return __lhs.__getval() > __rhs; } inline bool operator >(int __lhs, decimal128 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned int __lhs, decimal128 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(long __lhs, decimal128 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned long __lhs, decimal128 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(long long __lhs, decimal128 __rhs) { return __lhs > __rhs.__getval(); } inline bool operator >(unsigned long long __lhs, decimal128 __rhs) { return __lhs > __rhs.__getval(); }
  inline bool operator >=(decimal32 __lhs, decimal32 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal32 __lhs, decimal64 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal32 __lhs, decimal128 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal32 __lhs, int __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal32 __lhs, unsigned int __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal32 __lhs, long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal32 __lhs, unsigned long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal32 __lhs, long long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal32 __lhs, unsigned long long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(int __lhs, decimal32 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned int __lhs, decimal32 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(long __lhs, decimal32 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned long __lhs, decimal32 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(long long __lhs, decimal32 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned long long __lhs, decimal32 __rhs) { return __lhs >= __rhs.__getval(); }
  inline bool operator >=(decimal64 __lhs, decimal32 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal64 __lhs, decimal64 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal64 __lhs, decimal128 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal64 __lhs, int __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal64 __lhs, unsigned int __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal64 __lhs, long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal64 __lhs, unsigned long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal64 __lhs, long long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal64 __lhs, unsigned long long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(int __lhs, decimal64 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned int __lhs, decimal64 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(long __lhs, decimal64 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned long __lhs, decimal64 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(long long __lhs, decimal64 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned long long __lhs, decimal64 __rhs) { return __lhs >= __rhs.__getval(); }
  inline bool operator >=(decimal128 __lhs, decimal32 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal128 __lhs, decimal64 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal128 __lhs, decimal128 __rhs) { return __lhs.__getval() >= __rhs.__getval(); } inline bool operator >=(decimal128 __lhs, int __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal128 __lhs, unsigned int __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal128 __lhs, long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal128 __lhs, unsigned long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal128 __lhs, long long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(decimal128 __lhs, unsigned long long __rhs) { return __lhs.__getval() >= __rhs; } inline bool operator >=(int __lhs, decimal128 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned int __lhs, decimal128 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(long __lhs, decimal128 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned long __lhs, decimal128 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(long long __lhs, decimal128 __rhs) { return __lhs >= __rhs.__getval(); } inline bool operator >=(unsigned long long __lhs, decimal128 __rhs) { return __lhs >= __rhs.__getval(); }





}


}
# 498 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/decimal/decimal" 2 3

#pragma GCC diagnostic pop
# 8 "./eh/dfp-1.C" 2


# 9 "./eh/dfp-1.C"
using namespace std::decimal;

int
foo (double fp)
{
  if (fp < 32.0)
    throw (decimal32)32;
  if (fp < 64.0)
    throw (decimal64)64;
  if (fp < 128.0)
    throw (decimal128)128;
  return 0;
}

int bar (double fp)
{
  try
    {
      foo (fp);
      abort ();
    }
  catch (decimal32 df)
    {
      if (df != (decimal32)32)
 abort ();
    }
  catch (decimal64 dd)
    {
      if (dd != (decimal64)64)
 abort ();
    }
  catch (decimal128 dl)
    {
      if (dl != (decimal128)128)
 abort ();
    }
  return 0;
}

int
main ()
{
  bar (10.0);
  bar (20.0);
  bar (100.0);
  return 0;
}
