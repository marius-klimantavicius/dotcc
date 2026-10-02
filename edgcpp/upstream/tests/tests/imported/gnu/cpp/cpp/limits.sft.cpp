//type: fp
//options: 
# 0 "./cpp/limits.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./cpp/limits.C"



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
#pragma GCC diagnostic ignored "-Wlong-long"
#pragma GCC diagnostic ignored "-Wc++23-extensions"

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
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 2 3
# 165 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
namespace std __attribute__ ((__visibility__ ("default")))
{







  enum float_round_style
  {
    round_indeterminate = -1,
    round_toward_zero = 0,
    round_to_nearest = 1,
    round_toward_infinity = 2,
    round_toward_neg_infinity = 3
  };







  enum float_denorm_style
  {

    denorm_indeterminate = -1,

    denorm_absent = 0,

    denorm_present = 1
  };
# 209 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  struct __numeric_limits_base
  {


    static constexpr bool is_specialized = false;




    static constexpr int digits = 0;


    static constexpr int digits10 = 0;




    static constexpr int max_digits10 = 0;



    static constexpr bool is_signed = false;


    static constexpr bool is_integer = false;




    static constexpr bool is_exact = false;



    static constexpr int radix = 0;



    static constexpr int min_exponent = 0;



    static constexpr int min_exponent10 = 0;




    static constexpr int max_exponent = 0;



    static constexpr int max_exponent10 = 0;


    static constexpr bool has_infinity = false;



    static constexpr bool has_quiet_NaN = false;



    static constexpr bool has_signaling_NaN = false;


    static constexpr float_denorm_style has_denorm = denorm_absent;



    static constexpr bool has_denorm_loss = false;



    static constexpr bool is_iec559 = false;




    static constexpr bool is_bounded = false;
# 295 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
    static constexpr bool is_modulo = false;


    static constexpr bool traps = false;


    static constexpr bool tinyness_before = false;




    static constexpr float_round_style round_style =
          round_toward_zero;
  };
# 318 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  template<typename _Tp>
    struct numeric_limits : public __numeric_limits_base
    {


      static constexpr _Tp
      min() noexcept { return _Tp(); }


      static constexpr _Tp
      max() noexcept { return _Tp(); }




      static constexpr _Tp
      lowest() noexcept { return _Tp(); }




      static constexpr _Tp
      epsilon() noexcept { return _Tp(); }


      static constexpr _Tp
      round_error() noexcept { return _Tp(); }


      static constexpr _Tp
      infinity() noexcept { return _Tp(); }



      static constexpr _Tp
      quiet_NaN() noexcept { return _Tp(); }



      static constexpr _Tp
      signaling_NaN() noexcept { return _Tp(); }




      static constexpr _Tp
      denorm_min() noexcept { return _Tp(); }
    };




  template<typename _Tp>
    struct numeric_limits<const _Tp>
    : public numeric_limits<_Tp> { };

  template<typename _Tp>
    struct numeric_limits<volatile _Tp>
    : public numeric_limits<_Tp> { };

  template<typename _Tp>
    struct numeric_limits<const volatile _Tp>
    : public numeric_limits<_Tp> { };
# 390 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  template<>
    struct numeric_limits<bool>
    {
      static constexpr bool is_specialized = true;

      static constexpr bool
      min() noexcept { return false; }

      static constexpr bool
      max() noexcept { return true; }


      static constexpr bool
      lowest() noexcept { return min(); }

      static constexpr int digits = 1;
      static constexpr int digits10 = 0;

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr bool
      epsilon() noexcept { return false; }

      static constexpr bool
      round_error() noexcept { return false; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr bool
      infinity() noexcept { return false; }

      static constexpr bool
      quiet_NaN() noexcept { return false; }

      static constexpr bool
      signaling_NaN() noexcept { return false; }

      static constexpr bool
      denorm_min() noexcept { return false; }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;




      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<char>
    {
      static constexpr bool is_specialized = true;

      static constexpr char
      min() noexcept { return (((char)(-1) < 0) ? -(((char)(-1) < 0) ? (((((char)1 << ((sizeof(char) * 8 - ((char)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(char)0) - 1 : (char)0); }

      static constexpr char
      max() noexcept { return (((char)(-1) < 0) ? (((((char)1 << ((sizeof(char) * 8 - ((char)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(char)0); }


      static constexpr char
      lowest() noexcept { return min(); }


      static constexpr int digits = (sizeof(char) * 8 - ((char)(-1) < 0));
      static constexpr int digits10 = ((sizeof(char) * 8 - ((char)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = ((char)(-1) < 0);
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr char
      epsilon() noexcept { return 0; }

      static constexpr char
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr
      char infinity() noexcept { return char(); }

      static constexpr char
      quiet_NaN() noexcept { return char(); }

      static constexpr char
      signaling_NaN() noexcept { return char(); }

      static constexpr char
      denorm_min() noexcept { return static_cast<char>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = !is_signed;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<signed char>
    {
      static constexpr bool is_specialized = true;

      static constexpr signed char
      min() noexcept { return -0x7f - 1; }

      static constexpr signed char
      max() noexcept { return 0x7f; }


      static constexpr signed char
      lowest() noexcept { return min(); }


      static constexpr int digits = (sizeof(signed char) * 8 - ((signed char)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(signed char) * 8 - ((signed char)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr signed char
      epsilon() noexcept { return 0; }

      static constexpr signed char
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr signed char
      infinity() noexcept { return static_cast<signed char>(0); }

      static constexpr signed char
      quiet_NaN() noexcept { return static_cast<signed char>(0); }

      static constexpr signed char
      signaling_NaN() noexcept
      { return static_cast<signed char>(0); }

      static constexpr signed char
      denorm_min() noexcept
      { return static_cast<signed char>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<unsigned char>
    {
      static constexpr bool is_specialized = true;

      static constexpr unsigned char
      min() noexcept { return 0; }

      static constexpr unsigned char
      max() noexcept { return 0x7f * 2U + 1; }


      static constexpr unsigned char
      lowest() noexcept { return min(); }


      static constexpr int digits
       = (sizeof(unsigned char) * 8 - ((unsigned char)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(unsigned char) * 8 - ((unsigned char)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr unsigned char
      epsilon() noexcept { return 0; }

      static constexpr unsigned char
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr unsigned char
      infinity() noexcept
      { return static_cast<unsigned char>(0); }

      static constexpr unsigned char
      quiet_NaN() noexcept
      { return static_cast<unsigned char>(0); }

      static constexpr unsigned char
      signaling_NaN() noexcept
      { return static_cast<unsigned char>(0); }

      static constexpr unsigned char
      denorm_min() noexcept
      { return static_cast<unsigned char>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = true;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<wchar_t>
    {
      static constexpr bool is_specialized = true;

      static constexpr wchar_t
      min() noexcept { return (((wchar_t)(-1) < 0) ? -(((wchar_t)(-1) < 0) ? (((((wchar_t)1 << ((sizeof(wchar_t) * 8 - ((wchar_t)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(wchar_t)0) - 1 : (wchar_t)0); }

      static constexpr wchar_t
      max() noexcept { return (((wchar_t)(-1) < 0) ? (((((wchar_t)1 << ((sizeof(wchar_t) * 8 - ((wchar_t)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(wchar_t)0); }


      static constexpr wchar_t
      lowest() noexcept { return min(); }


      static constexpr int digits = (sizeof(wchar_t) * 8 - ((wchar_t)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(wchar_t) * 8 - ((wchar_t)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = ((wchar_t)(-1) < 0);
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr wchar_t
      epsilon() noexcept { return 0; }

      static constexpr wchar_t
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr wchar_t
      infinity() noexcept { return wchar_t(); }

      static constexpr wchar_t
      quiet_NaN() noexcept { return wchar_t(); }

      static constexpr wchar_t
      signaling_NaN() noexcept { return wchar_t(); }

      static constexpr wchar_t
      denorm_min() noexcept { return wchar_t(); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = !is_signed;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };
# 803 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  template<>
    struct numeric_limits<char16_t>
    {
      static constexpr bool is_specialized = true;

      static constexpr char16_t
      min() noexcept { return (((char16_t)(-1) < 0) ? -(((char16_t)(-1) < 0) ? (((((char16_t)1 << ((sizeof(char16_t) * 8 - ((char16_t)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(char16_t)0) - 1 : (char16_t)0); }

      static constexpr char16_t
      max() noexcept { return (((char16_t)(-1) < 0) ? (((((char16_t)1 << ((sizeof(char16_t) * 8 - ((char16_t)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(char16_t)0); }

      static constexpr char16_t
      lowest() noexcept { return min(); }

      static constexpr int digits = (sizeof(char16_t) * 8 - ((char16_t)(-1) < 0));
      static constexpr int digits10 = ((sizeof(char16_t) * 8 - ((char16_t)(-1) < 0)) * 643L / 2136);
      static constexpr int max_digits10 = 0;
      static constexpr bool is_signed = ((char16_t)(-1) < 0);
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr char16_t
      epsilon() noexcept { return 0; }

      static constexpr char16_t
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr char16_t
      infinity() noexcept { return char16_t(); }

      static constexpr char16_t
      quiet_NaN() noexcept { return char16_t(); }

      static constexpr char16_t
      signaling_NaN() noexcept { return char16_t(); }

      static constexpr char16_t
      denorm_min() noexcept { return char16_t(); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = !is_signed;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style = round_toward_zero;
    };


  template<>
    struct numeric_limits<char32_t>
    {
      static constexpr bool is_specialized = true;

      static constexpr char32_t
      min() noexcept { return (((char32_t)(-1) < 0) ? -(((char32_t)(-1) < 0) ? (((((char32_t)1 << ((sizeof(char32_t) * 8 - ((char32_t)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(char32_t)0) - 1 : (char32_t)0); }

      static constexpr char32_t
      max() noexcept { return (((char32_t)(-1) < 0) ? (((((char32_t)1 << ((sizeof(char32_t) * 8 - ((char32_t)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(char32_t)0); }

      static constexpr char32_t
      lowest() noexcept { return min(); }

      static constexpr int digits = (sizeof(char32_t) * 8 - ((char32_t)(-1) < 0));
      static constexpr int digits10 = ((sizeof(char32_t) * 8 - ((char32_t)(-1) < 0)) * 643L / 2136);
      static constexpr int max_digits10 = 0;
      static constexpr bool is_signed = ((char32_t)(-1) < 0);
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr char32_t
      epsilon() noexcept { return 0; }

      static constexpr char32_t
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr char32_t
      infinity() noexcept { return char32_t(); }

      static constexpr char32_t
      quiet_NaN() noexcept { return char32_t(); }

      static constexpr char32_t
      signaling_NaN() noexcept { return char32_t(); }

      static constexpr char32_t
      denorm_min() noexcept { return char32_t(); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = !is_signed;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style = round_toward_zero;
    };



  template<>
    struct numeric_limits<short>
    {
      static constexpr bool is_specialized = true;

      static constexpr short
      min() noexcept { return -0x7fff - 1; }

      static constexpr short
      max() noexcept { return 0x7fff; }


      static constexpr short
      lowest() noexcept { return min(); }


      static constexpr int digits = (sizeof(short) * 8 - ((short)(-1) < 0));
      static constexpr int digits10 = ((sizeof(short) * 8 - ((short)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr short
      epsilon() noexcept { return 0; }

      static constexpr short
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr short
      infinity() noexcept { return short(); }

      static constexpr short
      quiet_NaN() noexcept { return short(); }

      static constexpr short
      signaling_NaN() noexcept { return short(); }

      static constexpr short
      denorm_min() noexcept { return short(); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<unsigned short>
    {
      static constexpr bool is_specialized = true;

      static constexpr unsigned short
      min() noexcept { return 0; }

      static constexpr unsigned short
      max() noexcept { return 0x7fff * 2U + 1; }


      static constexpr unsigned short
      lowest() noexcept { return min(); }


      static constexpr int digits
       = (sizeof(unsigned short) * 8 - ((unsigned short)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(unsigned short) * 8 - ((unsigned short)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr unsigned short
      epsilon() noexcept { return 0; }

      static constexpr unsigned short
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr unsigned short
      infinity() noexcept
      { return static_cast<unsigned short>(0); }

      static constexpr unsigned short
      quiet_NaN() noexcept
      { return static_cast<unsigned short>(0); }

      static constexpr unsigned short
      signaling_NaN() noexcept
      { return static_cast<unsigned short>(0); }

      static constexpr unsigned short
      denorm_min() noexcept
      { return static_cast<unsigned short>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = true;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<int>
    {
      static constexpr bool is_specialized = true;

      static constexpr int
      min() noexcept { return -0x7fffffff - 1; }

      static constexpr int
      max() noexcept { return 0x7fffffff; }


      static constexpr int
      lowest() noexcept { return min(); }


      static constexpr int digits = (sizeof(int) * 8 - ((int)(-1) < 0));
      static constexpr int digits10 = ((sizeof(int) * 8 - ((int)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr int
      epsilon() noexcept { return 0; }

      static constexpr int
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr int
      infinity() noexcept { return static_cast<int>(0); }

      static constexpr int
      quiet_NaN() noexcept { return static_cast<int>(0); }

      static constexpr int
      signaling_NaN() noexcept { return static_cast<int>(0); }

      static constexpr int
      denorm_min() noexcept { return static_cast<int>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<unsigned int>
    {
      static constexpr bool is_specialized = true;

      static constexpr unsigned int
      min() noexcept { return 0; }

      static constexpr unsigned int
      max() noexcept { return 0x7fffffff * 2U + 1; }


      static constexpr unsigned int
      lowest() noexcept { return min(); }


      static constexpr int digits
       = (sizeof(unsigned int) * 8 - ((unsigned int)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(unsigned int) * 8 - ((unsigned int)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr unsigned int
      epsilon() noexcept { return 0; }

      static constexpr unsigned int
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr unsigned int
      infinity() noexcept { return static_cast<unsigned int>(0); }

      static constexpr unsigned int
      quiet_NaN() noexcept
      { return static_cast<unsigned int>(0); }

      static constexpr unsigned int
      signaling_NaN() noexcept
      { return static_cast<unsigned int>(0); }

      static constexpr unsigned int
      denorm_min() noexcept
      { return static_cast<unsigned int>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = true;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<long>
    {
      static constexpr bool is_specialized = true;

      static constexpr long
      min() noexcept { return -0x7fffffffffffffffL - 1; }

      static constexpr long
      max() noexcept { return 0x7fffffffffffffffL; }


      static constexpr long
      lowest() noexcept { return min(); }


      static constexpr int digits = (sizeof(long) * 8 - ((long)(-1) < 0));
      static constexpr int digits10 = ((sizeof(long) * 8 - ((long)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr long
      epsilon() noexcept { return 0; }

      static constexpr long
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr long
      infinity() noexcept { return static_cast<long>(0); }

      static constexpr long
      quiet_NaN() noexcept { return static_cast<long>(0); }

      static constexpr long
      signaling_NaN() noexcept { return static_cast<long>(0); }

      static constexpr long
      denorm_min() noexcept { return static_cast<long>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<unsigned long>
    {
      static constexpr bool is_specialized = true;

      static constexpr unsigned long
      min() noexcept { return 0; }

      static constexpr unsigned long
      max() noexcept { return 0x7fffffffffffffffL * 2UL + 1; }


      static constexpr unsigned long
      lowest() noexcept { return min(); }


      static constexpr int digits
       = (sizeof(unsigned long) * 8 - ((unsigned long)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(unsigned long) * 8 - ((unsigned long)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr unsigned long
      epsilon() noexcept { return 0; }

      static constexpr unsigned long
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr unsigned long
      infinity() noexcept
      { return static_cast<unsigned long>(0); }

      static constexpr unsigned long
      quiet_NaN() noexcept
      { return static_cast<unsigned long>(0); }

      static constexpr unsigned long
      signaling_NaN() noexcept
      { return static_cast<unsigned long>(0); }

      static constexpr unsigned long
      denorm_min() noexcept
      { return static_cast<unsigned long>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = true;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<long long>
    {
      static constexpr bool is_specialized = true;

      static constexpr long long
      min() noexcept { return -0x7fffffffffffffffLL - 1; }

      static constexpr long long
      max() noexcept { return 0x7fffffffffffffffLL; }


      static constexpr long long
      lowest() noexcept { return min(); }


      static constexpr int digits
       = (sizeof(long long) * 8 - ((long long)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(long long) * 8 - ((long long)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr long long
      epsilon() noexcept { return 0; }

      static constexpr long long
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr long long
      infinity() noexcept { return static_cast<long long>(0); }

      static constexpr long long
      quiet_NaN() noexcept { return static_cast<long long>(0); }

      static constexpr long long
      signaling_NaN() noexcept
      { return static_cast<long long>(0); }

      static constexpr long long
      denorm_min() noexcept { return static_cast<long long>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };


  template<>
    struct numeric_limits<unsigned long long>
    {
      static constexpr bool is_specialized = true;

      static constexpr unsigned long long
      min() noexcept { return 0; }

      static constexpr unsigned long long
      max() noexcept { return 0x7fffffffffffffffLL * 2ULL + 1; }


      static constexpr unsigned long long
      lowest() noexcept { return min(); }


      static constexpr int digits
       = (sizeof(unsigned long long) * 8 - ((unsigned long long)(-1) < 0));
      static constexpr int digits10
       = ((sizeof(unsigned long long) * 8 - ((unsigned long long)(-1) < 0)) * 643L / 2136);

      static constexpr int max_digits10 = 0;

      static constexpr bool is_signed = false;
      static constexpr bool is_integer = true;
      static constexpr bool is_exact = true;
      static constexpr int radix = 2;

      static constexpr unsigned long long
      epsilon() noexcept { return 0; }

      static constexpr unsigned long long
      round_error() noexcept { return 0; }

      static constexpr int min_exponent = 0;
      static constexpr int min_exponent10 = 0;
      static constexpr int max_exponent = 0;
      static constexpr int max_exponent10 = 0;

      static constexpr bool has_infinity = false;
      static constexpr bool has_quiet_NaN = false;
      static constexpr bool has_signaling_NaN = false;
      static constexpr float_denorm_style has_denorm
       = denorm_absent;
      static constexpr bool has_denorm_loss = false;

      static constexpr unsigned long long
      infinity() noexcept
      { return static_cast<unsigned long long>(0); }

      static constexpr unsigned long long
      quiet_NaN() noexcept
      { return static_cast<unsigned long long>(0); }

      static constexpr unsigned long long
      signaling_NaN() noexcept
      { return static_cast<unsigned long long>(0); }

      static constexpr unsigned long long
      denorm_min() noexcept
      { return static_cast<unsigned long long>(0); }

      static constexpr bool is_iec559 = false;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = true;

      static constexpr bool traps = true;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
       = round_toward_zero;
    };
# 1644 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  __extension__ template<> struct numeric_limits<__int128> { static constexpr bool is_specialized = true; static constexpr __int128 min() noexcept { return (((__int128)(-1) < 0) ? -(((__int128)(-1) < 0) ? (((((__int128)1 << ((128 - ((__int128)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(__int128)0) - 1 : (__int128)0); } static constexpr __int128 max() noexcept { return (((__int128)(-1) < 0) ? (((((__int128)1 << ((128 - ((__int128)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(__int128)0); } static constexpr int digits = 128 - 1; static constexpr int digits10 = (128 - 1) * 643L / 2136; static constexpr bool is_signed = true; static constexpr bool is_integer = true; static constexpr bool is_exact = true; static constexpr int radix = 2; static constexpr __int128 epsilon() noexcept { return 0; } static constexpr __int128 round_error() noexcept { return 0; } static constexpr __int128 lowest() noexcept { return min(); } static constexpr int max_digits10 = 0; static constexpr int min_exponent = 0; static constexpr int min_exponent10 = 0; static constexpr int max_exponent = 0; static constexpr int max_exponent10 = 0; static constexpr bool has_infinity = false; static constexpr bool has_quiet_NaN = false; static constexpr bool has_signaling_NaN = false; static constexpr float_denorm_style has_denorm = denorm_absent; static constexpr bool has_denorm_loss = false; static constexpr __int128 infinity() noexcept { return static_cast<__int128>(0); } static constexpr __int128 quiet_NaN() noexcept { return static_cast<__int128>(0); } static constexpr __int128 signaling_NaN() noexcept { return static_cast<__int128>(0); } static constexpr __int128 denorm_min() noexcept { return static_cast<__int128>(0); } static constexpr bool is_iec559 = false; static constexpr bool is_bounded = true; static constexpr bool is_modulo = false; static constexpr bool traps = true; static constexpr bool tinyness_before = false; static constexpr float_round_style round_style = round_toward_zero; }; __extension__ template<> struct numeric_limits<unsigned __int128> { static constexpr bool is_specialized = true; static constexpr unsigned __int128 min() noexcept { return 0; } static constexpr unsigned __int128 max() noexcept { return (((unsigned __int128)(-1) < 0) ? (((((unsigned __int128)1 << ((128 - ((unsigned __int128)(-1) < 0)) - 1)) - 1) << 1) + 1) : ~(unsigned __int128)0); } static constexpr unsigned __int128 lowest() noexcept { return min(); } static constexpr int max_digits10 = 0; static constexpr int digits = 128; static constexpr int digits10 = 128 * 643L / 2136; static constexpr bool is_signed = false; static constexpr bool is_integer = true; static constexpr bool is_exact = true; static constexpr int radix = 2; static constexpr unsigned __int128 epsilon() noexcept { return 0; } static constexpr unsigned __int128 round_error() noexcept { return 0; } static constexpr int min_exponent = 0; static constexpr int min_exponent10 = 0; static constexpr int max_exponent = 0; static constexpr int max_exponent10 = 0; static constexpr bool has_infinity = false; static constexpr bool has_quiet_NaN = false; static constexpr bool has_signaling_NaN = false; static constexpr float_denorm_style has_denorm = denorm_absent; static constexpr bool has_denorm_loss = false; static constexpr unsigned __int128 infinity() noexcept { return static_cast<unsigned __int128>(0); } static constexpr unsigned __int128 quiet_NaN() noexcept { return static_cast<unsigned __int128>(0); } static constexpr unsigned __int128 signaling_NaN() noexcept { return static_cast<unsigned __int128>(0); } static constexpr unsigned __int128 denorm_min() noexcept { return static_cast<unsigned __int128>(0); } static constexpr bool is_iec559 = false; static constexpr bool is_bounded = true; static constexpr bool is_modulo = true; static constexpr bool traps = true; static constexpr bool tinyness_before = false; static constexpr float_round_style round_style = round_toward_zero; };
# 1676 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  template<>
    struct numeric_limits<float>
    {
      static constexpr bool is_specialized = true;

      static constexpr float
      min() noexcept { return 1.17549435082228750796873653722224568e-38F; }

      static constexpr float
      max() noexcept { return 3.40282346638528859811704183484516925e+38F; }


      static constexpr float
      lowest() noexcept { return -3.40282346638528859811704183484516925e+38F; }


      static constexpr int digits = 24;
      static constexpr int digits10 = 6;

      static constexpr int max_digits10
  = (2 + (24) * 643L / 2136);

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = false;
      static constexpr bool is_exact = false;
      static constexpr int radix = 2;

      static constexpr float
      epsilon() noexcept { return 1.19209289550781250000000000000000000e-7F; }

      static constexpr float
      round_error() noexcept { return 0.5F; }

      static constexpr int min_exponent = (-125);
      static constexpr int min_exponent10 = (-37);
      static constexpr int max_exponent = 128;
      static constexpr int max_exponent10 = 38;

      static constexpr bool has_infinity = 1;
      static constexpr bool has_quiet_NaN = 1;
      static constexpr bool has_signaling_NaN = has_quiet_NaN;
      static constexpr float_denorm_style has_denorm
 = bool(1) ? denorm_present : denorm_absent;
      static constexpr bool has_denorm_loss
       = false;

      static constexpr float
      infinity() noexcept { return __builtin_huge_valf(); }

      static constexpr float
      quiet_NaN() noexcept { return __builtin_nanf(""); }

      static constexpr float
      signaling_NaN() noexcept { return __builtin_nansf(""); }

      static constexpr float
      denorm_min() noexcept { return 1.40129846432481707092372958328991613e-45F; }

      static constexpr bool is_iec559
 = has_infinity && has_quiet_NaN && has_denorm == denorm_present;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = false;
      static constexpr bool tinyness_before
       = false;
      static constexpr float_round_style round_style
       = round_to_nearest;
    };






  template<>
    struct numeric_limits<double>
    {
      static constexpr bool is_specialized = true;

      static constexpr double
      min() noexcept { return double(2.22507385850720138309023271733240406e-308L); }

      static constexpr double
      max() noexcept { return double(1.79769313486231570814527423731704357e+308L); }


      static constexpr double
      lowest() noexcept { return -double(1.79769313486231570814527423731704357e+308L); }


      static constexpr int digits = 53;
      static constexpr int digits10 = 15;

      static constexpr int max_digits10
  = (2 + (53) * 643L / 2136);

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = false;
      static constexpr bool is_exact = false;
      static constexpr int radix = 2;

      static constexpr double
      epsilon() noexcept { return double(2.22044604925031308084726333618164062e-16L); }

      static constexpr double
      round_error() noexcept { return 0.5; }

      static constexpr int min_exponent = (-1021);
      static constexpr int min_exponent10 = (-307);
      static constexpr int max_exponent = 1024;
      static constexpr int max_exponent10 = 308;

      static constexpr bool has_infinity = 1;
      static constexpr bool has_quiet_NaN = 1;
      static constexpr bool has_signaling_NaN = has_quiet_NaN;
      static constexpr float_denorm_style has_denorm
 = bool(1) ? denorm_present : denorm_absent;
      static constexpr bool has_denorm_loss
        = false;

      static constexpr double
      infinity() noexcept { return __builtin_huge_val(); }

      static constexpr double
      quiet_NaN() noexcept { return __builtin_nan(""); }

      static constexpr double
      signaling_NaN() noexcept { return __builtin_nans(""); }

      static constexpr double
      denorm_min() noexcept { return double(4.94065645841246544176568792868221372e-324L); }

      static constexpr bool is_iec559
 = has_infinity && has_quiet_NaN && has_denorm == denorm_present;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = false;
      static constexpr bool tinyness_before
       = false;
      static constexpr float_round_style round_style
       = round_to_nearest;
    };






  template<>
    struct numeric_limits<long double>
    {
      static constexpr bool is_specialized = true;

      static constexpr long double
      min() noexcept { return 3.36210314311209350626267781732175260e-4932L; }

      static constexpr long double
      max() noexcept { return 1.18973149535723176502126385303097021e+4932L; }


      static constexpr long double
      lowest() noexcept { return -1.18973149535723176502126385303097021e+4932L; }


      static constexpr int digits = 64;
      static constexpr int digits10 = 18;

      static constexpr int max_digits10
  = (2 + (64) * 643L / 2136);

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = false;
      static constexpr bool is_exact = false;
      static constexpr int radix = 2;

      static constexpr long double
      epsilon() noexcept { return 1.08420217248550443400745280086994171e-19L; }

      static constexpr long double
      round_error() noexcept { return 0.5L; }

      static constexpr int min_exponent = (-16381);
      static constexpr int min_exponent10 = (-4931);
      static constexpr int max_exponent = 16384;
      static constexpr int max_exponent10 = 4932;

      static constexpr bool has_infinity = 1;
      static constexpr bool has_quiet_NaN = 1;
      static constexpr bool has_signaling_NaN = has_quiet_NaN;
      static constexpr float_denorm_style has_denorm
 = bool(1) ? denorm_present : denorm_absent;
      static constexpr bool has_denorm_loss
 = false;

      static constexpr long double
      infinity() noexcept { return __builtin_huge_vall(); }

      static constexpr long double
      quiet_NaN() noexcept { return __builtin_nanl(""); }

      static constexpr long double
      signaling_NaN() noexcept { return __builtin_nansl(""); }

      static constexpr long double
      denorm_min() noexcept { return 3.64519953188247460252840593361941982e-4951L; }

      static constexpr bool is_iec559
 = has_infinity && has_quiet_NaN && has_denorm == denorm_present;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = false;
      static constexpr bool tinyness_before =
      false;
      static constexpr float_round_style round_style =
            round_to_nearest;
    };
# 1996 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
__extension__ template<> struct numeric_limits<_Float32> { static constexpr bool is_specialized = true; static constexpr _Float32 min() noexcept { return 1.17549435082228750796873653722224568e-38F32; } static constexpr _Float32 max() noexcept { return 3.40282346638528859811704183484516925e+38F32; } static constexpr _Float32 lowest() noexcept { return -3.40282346638528859811704183484516925e+38F32; } static constexpr int digits = 24; static constexpr int digits10 = 6; static constexpr int max_digits10 = (2 + (24) * 643L / 2136); static constexpr bool is_signed = true; static constexpr bool is_integer = false; static constexpr bool is_exact = false; static constexpr int radix = 2; static constexpr _Float32 epsilon() noexcept { return 1.19209289550781250000000000000000000e-7F32; } static constexpr _Float32 round_error() noexcept { return 0.5F32; } static constexpr int min_exponent = (-125); static constexpr int min_exponent10 = (-37); static constexpr int max_exponent = 128; static constexpr int max_exponent10 = 38; static constexpr bool has_infinity = 1; static constexpr bool has_quiet_NaN = 1; static constexpr bool has_signaling_NaN = has_quiet_NaN; static constexpr float_denorm_style has_denorm = bool(1) ? denorm_present : denorm_absent; static constexpr bool has_denorm_loss = false; static constexpr _Float32 infinity() noexcept { return __builtin_huge_valf32(); } static constexpr _Float32 quiet_NaN() noexcept { return __builtin_nanf32(""); } static constexpr _Float32 signaling_NaN() noexcept { return __builtin_nansf32(""); } static constexpr _Float32 denorm_min() noexcept { return 1.40129846432481707092372958328991613e-45F32; } static constexpr bool is_iec559 = has_infinity && has_quiet_NaN && has_denorm == denorm_present; static constexpr bool is_bounded = true; static constexpr bool is_modulo = false; static constexpr bool traps = false; static constexpr bool tinyness_before = false; static constexpr float_round_style round_style = round_to_nearest; };


__extension__ template<> struct numeric_limits<_Float64> { static constexpr bool is_specialized = true; static constexpr _Float64 min() noexcept { return 2.22507385850720138309023271733240406e-308F64; } static constexpr _Float64 max() noexcept { return 1.79769313486231570814527423731704357e+308F64; } static constexpr _Float64 lowest() noexcept { return -1.79769313486231570814527423731704357e+308F64; } static constexpr int digits = 53; static constexpr int digits10 = 15; static constexpr int max_digits10 = (2 + (53) * 643L / 2136); static constexpr bool is_signed = true; static constexpr bool is_integer = false; static constexpr bool is_exact = false; static constexpr int radix = 2; static constexpr _Float64 epsilon() noexcept { return 2.22044604925031308084726333618164062e-16F64; } static constexpr _Float64 round_error() noexcept { return 0.5F64; } static constexpr int min_exponent = (-1021); static constexpr int min_exponent10 = (-307); static constexpr int max_exponent = 1024; static constexpr int max_exponent10 = 308; static constexpr bool has_infinity = 1; static constexpr bool has_quiet_NaN = 1; static constexpr bool has_signaling_NaN = has_quiet_NaN; static constexpr float_denorm_style has_denorm = bool(1) ? denorm_present : denorm_absent; static constexpr bool has_denorm_loss = false; static constexpr _Float64 infinity() noexcept { return __builtin_huge_valf64(); } static constexpr _Float64 quiet_NaN() noexcept { return __builtin_nanf64(""); } static constexpr _Float64 signaling_NaN() noexcept { return __builtin_nansf64(""); } static constexpr _Float64 denorm_min() noexcept { return 4.94065645841246544176568792868221372e-324F64; } static constexpr bool is_iec559 = has_infinity && has_quiet_NaN && has_denorm == denorm_present; static constexpr bool is_bounded = true; static constexpr bool is_modulo = false; static constexpr bool traps = false; static constexpr bool tinyness_before = false; static constexpr float_round_style round_style = round_to_nearest; };


__extension__ template<> struct numeric_limits<_Float128> { static constexpr bool is_specialized = true; static constexpr _Float128 min() noexcept { return 3.36210314311209350626267781732175260e-4932F128; } static constexpr _Float128 max() noexcept { return 1.18973149535723176508575932662800702e+4932F128; } static constexpr _Float128 lowest() noexcept { return -1.18973149535723176508575932662800702e+4932F128; } static constexpr int digits = 113; static constexpr int digits10 = 33; static constexpr int max_digits10 = (2 + (113) * 643L / 2136); static constexpr bool is_signed = true; static constexpr bool is_integer = false; static constexpr bool is_exact = false; static constexpr int radix = 2; static constexpr _Float128 epsilon() noexcept { return 1.92592994438723585305597794258492732e-34F128; } static constexpr _Float128 round_error() noexcept { return 0.5F128; } static constexpr int min_exponent = (-16381); static constexpr int min_exponent10 = (-4931); static constexpr int max_exponent = 16384; static constexpr int max_exponent10 = 4932; static constexpr bool has_infinity = 1; static constexpr bool has_quiet_NaN = 1; static constexpr bool has_signaling_NaN = has_quiet_NaN; static constexpr float_denorm_style has_denorm = bool(1) ? denorm_present : denorm_absent; static constexpr bool has_denorm_loss = false; static constexpr _Float128 infinity() noexcept { return __builtin_huge_valf128(); } static constexpr _Float128 quiet_NaN() noexcept { return __builtin_nanf128(""); } static constexpr _Float128 signaling_NaN() noexcept { return __builtin_nansf128(""); } static constexpr _Float128 denorm_min() noexcept { return 6.47517511943802511092443895822764655e-4966F128; } static constexpr bool is_iec559 = has_infinity && has_quiet_NaN && has_denorm == denorm_present; static constexpr bool is_bounded = true; static constexpr bool is_modulo = false; static constexpr bool traps = false; static constexpr bool tinyness_before = false; static constexpr float_round_style round_style = round_to_nearest; };
# 2094 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
  __extension__
  template<>
    struct numeric_limits<__float128>
    {
      static constexpr bool is_specialized = true;

      static constexpr __float128
      min() noexcept
      {




 return __extension__ 0x1.0p-16382Q;

      }

      static constexpr __float128
      max() noexcept
      {







 return __extension__ 0x1.ffffffffffffffffffffffffffffp+16383Q;

      }

      static constexpr __float128
      lowest() noexcept
      { return -max(); }

      static constexpr int digits = 113;
      static constexpr int digits10 = 33;

      static constexpr int max_digits10 = 35;

      static constexpr bool is_signed = true;
      static constexpr bool is_integer = false;
      static constexpr bool is_exact = false;
      static constexpr int radix = 2;

      static constexpr __float128
      epsilon() noexcept
      { return double(1.9259299443872359e-34); }

      static constexpr __float128
      round_error() noexcept { return 0.5; }

      static constexpr int min_exponent = -16381;
      static constexpr int min_exponent10 = -4931;
      static constexpr int max_exponent = 16384;
      static constexpr int max_exponent10 = 4932;

      static constexpr bool has_infinity = 1;
      static constexpr bool has_quiet_NaN = 1;


      static constexpr bool has_signaling_NaN = true;



      static constexpr float_denorm_style has_denorm
 = denorm_present;
      static constexpr bool has_denorm_loss = false;

      static constexpr __float128
      infinity() noexcept
      { return __builtin_huge_val(); }

      static constexpr __float128
      quiet_NaN() noexcept
      { return __builtin_nan(""); }

      static constexpr __float128
      signaling_NaN() noexcept
      {

 return __builtin_nansq("");





      }

      static constexpr __float128
      denorm_min() noexcept
      {




 return __extension__ 0x1.0p-16494Q;

      }

      static constexpr bool is_iec559 = has_signaling_NaN;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = false;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
 = round_to_nearest;
# 2225 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
    };




}
# 2239 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
#pragma GCC diagnostic pop
# 5 "./cpp/limits.C" 2
# 17 "./cpp/limits.C"

# 17 "./cpp/limits.C"
int
main ()
{
    return 0;
}
