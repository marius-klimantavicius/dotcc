//type: rp
//options:  --c++14
# 0 "./opt/pr102970.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./opt/pr102970.C"





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/requires_hosted.h" 1 3
# 31 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/requires_hosted.h" 3
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
# 32 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/requires_hosted.h" 2 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 1 3
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception_defines.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{




  void
  __throw_bad_exception(void) __attribute__((__noreturn__));


  void
  __throw_bad_alloc(void) __attribute__((__noreturn__));

  void
  __throw_bad_array_new_length(void) __attribute__((__noreturn__));


  void
  __throw_bad_cast(void) __attribute__((__noreturn__,__cold__));

  void
  __throw_bad_typeid(void) __attribute__((__noreturn__,__cold__));


  void
  __throw_logic_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_domain_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_invalid_argument(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_length_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_out_of_range(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_out_of_range_fmt(const char*, ...) __attribute__((__noreturn__,__cold__))
    __attribute__((__format__(__gnu_printf__, 1, 2)));

  void
  __throw_runtime_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_range_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_overflow_error(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_underflow_error(const char*) __attribute__((__noreturn__,__cold__));


  void
  __throw_ios_failure(const char*) __attribute__((__noreturn__,__cold__));

  void
  __throw_ios_failure(const char*, int) __attribute__((__noreturn__,__cold__));


  void
  __throw_system_error(int) __attribute__((__noreturn__,__cold__));


  void
  __throw_future_error(int) __attribute__((__noreturn__,__cold__));


  void
  __throw_bad_function_call() __attribute__((__noreturn__,__cold__));
# 140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functexcept.h" 3

}
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 2 3




#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{


  struct __true_type { };
  struct __false_type { };

  template<bool>
    struct __truth_type
    { typedef __false_type __type; };

  template<>
    struct __truth_type<true>
    { typedef __true_type __type; };



  template<class _Sp, class _Tp>
    struct __traitor
    {
      enum { __value = bool(_Sp::__value) || bool(_Tp::__value) };
      typedef typename __truth_type<__value>::__type __type;
    };


  template<typename, typename>
    struct __are_same
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<typename _Tp>
    struct __are_same<_Tp, _Tp>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };




  template<typename _Tp>
    struct __is_integer
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<>
    struct __is_integer<bool>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<signed char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_integer<wchar_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 176 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<>
    struct __is_integer<char16_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<char32_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_integer<short>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned short>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<int>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned int>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<long long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_integer<unsigned long long>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 281 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_floating
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };


  template<>
    struct __is_floating<float>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_floating<double>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_floating<long double>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 358 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_arithmetic
    : public __traitor<__is_integer<_Tp>, __is_floating<_Tp> >
    { };




  template<typename _Tp>
    struct __is_char
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<>
    struct __is_char<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<>
    struct __is_char<wchar_t>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };


  template<typename _Tp>
    struct __is_byte
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };

  template<>
    struct __is_byte<char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_byte<signed char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };

  template<>
    struct __is_byte<unsigned char>
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 438 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_nonvolatile_trivially_copyable
    {
      enum { __value = __is_trivially_copyable(_Tp) };
    };




  template<typename _Tp>
    struct __is_nonvolatile_trivially_copyable<volatile _Tp>
    {
      enum { __value = 0 };
    };


  template<typename _OutputIter, typename _InputIter>
    struct __memcpyable
    {
      enum { __value = 0 };
    };


  template<typename _Tp>
    struct __memcpyable<_Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };


  template<typename _Tp>
    struct __memcpyable<_Tp*, const _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp> struct __memcpyable_integer;




  template<typename _Tp, typename _Up>
    struct __memcpyable<_Tp*, _Up*>
    {
      enum {
 __value = __memcpyable_integer<_Tp>::__width != 0
      && ((int)__memcpyable_integer<_Tp>::__width
     == (int)__memcpyable_integer<_Up>::__width)
      };
    };


  template<typename _Tp, typename _Up>
    struct __memcpyable<_Tp*, const _Up*>
    : __memcpyable<_Tp*, _Up*>
    { };

  template<typename _Tp>
    struct __memcpyable_integer
    {
      enum {
 __width = __is_integer<_Tp>::__value ? (sizeof(_Tp) * 8) : 0
      };
    };


  template<typename _Tp>
    struct __memcpyable_integer<volatile _Tp>
    { enum { __width = 0 }; };
# 551 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  __extension__
  template<>
    struct __memcpyable_integer<__int128> { enum { __width = 128 }; };
  __extension__
  template<>
    struct __memcpyable_integer<unsigned __int128> { enum { __width = 128 }; };
# 592 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Iter1, typename _Iter2>
    struct __memcmpable
    {
      enum { __value = 0 };
    };


  template<typename _Tp>
    struct __memcmpable<_Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp>
    struct __memcmpable<const _Tp*, _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };

  template<typename _Tp>
    struct __memcmpable<_Tp*, const _Tp*>
    : __is_nonvolatile_trivially_copyable<_Tp>
    { };







  template<typename _Tp, bool _TreatAsBytes =



 __is_byte<_Tp>::__value

    >
    struct __is_memcmp_ordered
    {
      static const bool __value = _Tp(-1) > _Tp(1);
    };

  template<typename _Tp>
    struct __is_memcmp_ordered<_Tp, false>
    {
      static const bool __value = false;
    };


  template<typename _Tp, typename _Up, bool = sizeof(_Tp) == sizeof(_Up)>
    struct __is_memcmp_ordered_with
    {
      static const bool __value = __is_memcmp_ordered<_Tp>::__value
 && __is_memcmp_ordered<_Up>::__value;
    };

  template<typename _Tp, typename _Up>
    struct __is_memcmp_ordered_with<_Tp, _Up, false>
    {
      static const bool __value = false;
    };
# 686 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
  template<typename _Tp>
    struct __is_move_iterator
    {
      enum { __value = 0 };
      typedef __false_type __type;
    };



  template<typename _Iterator>
   
    inline _Iterator
    __miter_base(_Iterator __it)
    { return __it; }


}
}

#pragma GCC diagnostic pop
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"

extern "C++" {

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{



  template<bool, typename>
    struct __enable_if
    { };

  template<typename _Tp>
    struct __enable_if<true, _Tp>
    { typedef _Tp __type; };



  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    struct __conditional_type
    { typedef _Iftrue __type; };

  template<typename _Iftrue, typename _Iffalse>
    struct __conditional_type<false, _Iftrue, _Iffalse>
    { typedef _Iffalse __type; };



  template<typename _Tp>
    struct __add_unsigned
    {
    private:
      typedef __enable_if<std::__is_integer<_Tp>::__value, _Tp> __if_type;

    public:
      typedef typename __if_type::__type __type;
    };

  template<>
    struct __add_unsigned<char>
    { typedef unsigned char __type; };

  template<>
    struct __add_unsigned<signed char>
    { typedef unsigned char __type; };

  template<>
    struct __add_unsigned<short>
    { typedef unsigned short __type; };

  template<>
    struct __add_unsigned<int>
    { typedef unsigned int __type; };

  template<>
    struct __add_unsigned<long>
    { typedef unsigned long __type; };

  template<>
    struct __add_unsigned<long long>
    { typedef unsigned long long __type; };


  template<>
    struct __add_unsigned<bool>;

  template<>
    struct __add_unsigned<wchar_t>;



  template<typename _Tp>
    struct __remove_unsigned
    {
    private:
      typedef __enable_if<std::__is_integer<_Tp>::__value, _Tp> __if_type;

    public:
      typedef typename __if_type::__type __type;
    };

  template<>
    struct __remove_unsigned<char>
    { typedef signed char __type; };

  template<>
    struct __remove_unsigned<unsigned char>
    { typedef signed char __type; };

  template<>
    struct __remove_unsigned<unsigned short>
    { typedef short __type; };

  template<>
    struct __remove_unsigned<unsigned int>
    { typedef int __type; };

  template<>
    struct __remove_unsigned<unsigned long>
    { typedef long __type; };

  template<>
    struct __remove_unsigned<unsigned long long>
    { typedef long long __type; };


  template<>
    struct __remove_unsigned<bool>;

  template<>
    struct __remove_unsigned<wchar_t>;



  template<typename _Type>
    constexpr
    inline bool
    __is_null_pointer(_Type* __ptr)
    { return __ptr == 0; }

  template<typename _Type>
    constexpr
    inline bool
    __is_null_pointer(_Type)
    { return false; }


  constexpr bool
  __is_null_pointer(std::nullptr_t)
  { return true; }




  template<typename _Tp, bool = std::__is_integer<_Tp>::__value>
    struct __promote
    { typedef double __type; };




  template<typename _Tp>
    struct __promote<_Tp, false>
    { };

  template<>
    struct __promote<long double>
    { typedef long double __type; };

  template<>
    struct __promote<double>
    { typedef double __type; };

  template<>
    struct __promote<float>
    { typedef float __type; };
# 246 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
  template<typename _Tp, typename _Up,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type>
    struct __promote_2
    {
      typedef __typeof__(_Tp2() + _Up2()) __type;
    };

  template<typename _Tp, typename _Up, typename _Vp,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type,
           typename _Vp2 = typename __promote<_Vp>::__type>
    struct __promote_3
    {
      typedef __typeof__(_Tp2() + _Up2() + _Vp2()) __type;
    };

  template<typename _Tp, typename _Up, typename _Vp, typename _Wp,
           typename _Tp2 = typename __promote<_Tp>::__type,
           typename _Up2 = typename __promote<_Up>::__type,
           typename _Vp2 = typename __promote<_Vp>::__type,
           typename _Wp2 = typename __promote<_Wp>::__type>
    struct __promote_4
    {
      typedef __typeof__(_Tp2() + _Up2() + _Vp2() + _Wp2()) __type;
    };



}
}

#pragma GCC diagnostic pop
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  template<typename _Tp>
    struct __is_integer_nonstrict
    : public std::__is_integer<_Tp>
    {
      using std::__is_integer<_Tp>::__value;


      enum { __width = __value ? sizeof(_Tp) * 8 : 0 };
    };

  template<typename _Value>
    struct __numeric_traits_integer
    {

      static_assert(__is_integer_nonstrict<_Value>::__value,
      "invalid specialization");




      static const bool __is_signed = (_Value)(-1) < 0;
      static const int __digits
 = __is_integer_nonstrict<_Value>::__width - __is_signed;


      static const _Value __max = __is_signed
 ? (((((_Value)1 << (__digits - 1)) - 1) << 1) + 1)
 : ~(_Value)0;
      static const _Value __min = __is_signed ? -__max - 1 : (_Value)0;
    };

  template<typename _Value>
    const _Value __numeric_traits_integer<_Value>::__min;

  template<typename _Value>
    const _Value __numeric_traits_integer<_Value>::__max;

  template<typename _Value>
    const bool __numeric_traits_integer<_Value>::__is_signed;

  template<typename _Value>
    const int __numeric_traits_integer<_Value>::__digits;
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  __extension__ template<> struct __is_integer_nonstrict<__int128> { enum { __value = 1 }; typedef std::__true_type __type; enum { __width = 128 }; }; __extension__ template<> struct __is_integer_nonstrict<unsigned __int128> { enum { __value = 1 }; typedef std::__true_type __type; enum { __width = 128 }; };






  template<typename _Tp>
    using __int_traits = __numeric_traits_integer<_Tp>;
# 159 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
  template<typename _Value>
    struct __numeric_traits_floating
    {

      static const int __max_digits10 = (2 + (std::__are_same<_Value, float>::__value ? 24 : std::__are_same<_Value, double>::__value ? 53 : 64) * 643L / 2136);


      static const bool __is_signed = true;
      static const int __digits10 = (std::__are_same<_Value, float>::__value ? 6 : std::__are_same<_Value, double>::__value ? 15 : 18);
      static const int __max_exponent10 = (std::__are_same<_Value, float>::__value ? 38 : std::__are_same<_Value, double>::__value ? 308 : 4932);
    };

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__max_digits10;

  template<typename _Value>
    const bool __numeric_traits_floating<_Value>::__is_signed;

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__digits10;

  template<typename _Value>
    const int __numeric_traits_floating<_Value>::__max_exponent10;






  template<typename _Value>
    struct __numeric_traits
    : public __numeric_traits_integer<_Value>
    { };

  template<>
    struct __numeric_traits<float>
    : public __numeric_traits_floating<float>
    { };

  template<>
    struct __numeric_traits<double>
    : public __numeric_traits_floating<double>
    { };

  template<>
    struct __numeric_traits<long double>
    : public __numeric_traits_floating<long double>
    { };
# 240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3

}
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 1 3
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 1 3
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 2 3

extern "C++"
{
namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _Tp>
    class reference_wrapper;
# 91 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp, _Tp __v>
    struct integral_constant
    {
      static constexpr _Tp value = __v;
      using value_type = _Tp;
      using type = integral_constant<_Tp, __v>;
      constexpr operator value_type() const noexcept { return value; }


      constexpr value_type operator()() const noexcept { return value; }

    };


  template<typename _Tp, _Tp __v>
    constexpr _Tp integral_constant<_Tp, __v>::value;




  template<bool __v>
    using __bool_constant = integral_constant<bool, __v>;



  using true_type = __bool_constant<true>;


  using false_type = __bool_constant<false>;
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<bool, typename _Tp = void>
    struct enable_if
    { };


  template<typename _Tp>
    struct enable_if<true, _Tp>
    { using type = _Tp; };


  template<bool _Cond, typename _Tp = void>
    using __enable_if_t = typename enable_if<_Cond, _Tp>::type;

  template<bool>
    struct __conditional
    {
      template<typename _Tp, typename>
 using type = _Tp;
    };

  template<>
    struct __conditional<false>
    {
      template<typename, typename _Up>
 using type = _Up;
    };


  template<bool _Cond, typename _If, typename _Else>
    using __conditional_t
      = typename __conditional<_Cond>::template type<_If, _Else>;


  template <typename _Type>
    struct __type_identity
    { using type = _Type; };

  template<typename _Tp>
    using __type_identity_t = typename __type_identity<_Tp>::type;

  namespace __detail
  {

    template<typename _Tp, typename...>
      using __first_t = _Tp;


    template<typename... _Bn>
      auto __or_fn(int) -> __first_t<false_type,
         __enable_if_t<!bool(_Bn::value)>...>;

    template<typename... _Bn>
      auto __or_fn(...) -> true_type;

    template<typename... _Bn>
      auto __and_fn(int) -> __first_t<true_type,
          __enable_if_t<bool(_Bn::value)>...>;

    template<typename... _Bn>
      auto __and_fn(...) -> false_type;
  }




  template<typename... _Bn>
    struct __or_
    : decltype(__detail::__or_fn<_Bn...>(0))
    { };

  template<typename... _Bn>
    struct __and_
    : decltype(__detail::__and_fn<_Bn...>(0))
    { };

  template<typename _Pp>
    struct __not_
    : __bool_constant<!bool(_Pp::value)>
    { };
# 282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename>
    struct is_reference;
  template<typename>
    struct is_function;
  template<typename>
    struct is_void;
  template<typename>
    struct remove_cv;
  template<typename>
    struct is_const;


  template<typename>
    struct __is_array_unknown_bounds;




  template <typename _Tp, size_t = sizeof(_Tp)>
    constexpr true_type __is_complete_or_unbounded(__type_identity<_Tp>)
    { return {}; }

  template <typename _TypeIdentity,
      typename _NestedType = typename _TypeIdentity::type>
    constexpr typename __or_<
      is_reference<_NestedType>,
      is_function<_NestedType>,
      is_void<_NestedType>,
      __is_array_unknown_bounds<_NestedType>
    >::type __is_complete_or_unbounded(_TypeIdentity)
    { return {}; }


  template<typename _Tp>
    using __remove_cv_t = typename remove_cv<_Tp>::type;





  template<typename _Tp>
    struct is_void
    : public false_type { };

  template<>
    struct is_void<void>
    : public true_type { };

  template<>
    struct is_void<const void>
    : public true_type { };

  template<>
    struct is_void<volatile void>
    : public true_type { };

  template<>
    struct is_void<const volatile void>
    : public true_type { };


  template<typename>
    struct __is_integral_helper
    : public false_type { };

  template<>
    struct __is_integral_helper<bool>
    : public true_type { };

  template<>
    struct __is_integral_helper<char>
    : public true_type { };

  template<>
    struct __is_integral_helper<signed char>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned char>
    : public true_type { };




  template<>
    struct __is_integral_helper<wchar_t>
    : public true_type { };







  template<>
    struct __is_integral_helper<char16_t>
    : public true_type { };

  template<>
    struct __is_integral_helper<char32_t>
    : public true_type { };

  template<>
    struct __is_integral_helper<short>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned short>
    : public true_type { };

  template<>
    struct __is_integral_helper<int>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned int>
    : public true_type { };

  template<>
    struct __is_integral_helper<long>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned long>
    : public true_type { };

  template<>
    struct __is_integral_helper<long long>
    : public true_type { };

  template<>
    struct __is_integral_helper<unsigned long long>
    : public true_type { };
# 465 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_integral
    : public __is_integral_helper<__remove_cv_t<_Tp>>::type
    { };


  template<typename>
    struct __is_floating_point_helper
    : public false_type { };

  template<>
    struct __is_floating_point_helper<float>
    : public true_type { };

  template<>
    struct __is_floating_point_helper<double>
    : public true_type { };

  template<>
    struct __is_floating_point_helper<long double>
    : public true_type { };
# 525 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_floating_point
    : public __is_floating_point_helper<__remove_cv_t<_Tp>>::type
    { };



  template<typename _Tp>
    struct is_array
    : public __bool_constant<__is_array(_Tp)>
    { };
# 552 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_pointer
    : public __bool_constant<__is_pointer(_Tp)>
    { };
# 579 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename>
    struct is_lvalue_reference
    : public false_type { };

  template<typename _Tp>
    struct is_lvalue_reference<_Tp&>
    : public true_type { };


  template<typename>
    struct is_rvalue_reference
    : public false_type { };

  template<typename _Tp>
    struct is_rvalue_reference<_Tp&&>
    : public true_type { };



  template<typename _Tp>
    struct is_member_object_pointer
    : public __bool_constant<__is_member_object_pointer(_Tp)>
    { };
# 620 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_member_function_pointer
    : public __bool_constant<__is_member_function_pointer(_Tp)>
    { };
# 641 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_enum
    : public __bool_constant<__is_enum(_Tp)>
    { };


  template<typename _Tp>
    struct is_union
    : public __bool_constant<__is_union(_Tp)>
    { };


  template<typename _Tp>
    struct is_class
    : public __bool_constant<__is_class(_Tp)>
    { };



  template<typename _Tp>
    struct is_function
    : public __bool_constant<__is_function(_Tp)>
    { };
# 680 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_null_pointer
    : public false_type { };

  template<>
    struct is_null_pointer<std::nullptr_t>
    : public true_type { };

  template<>
    struct is_null_pointer<const std::nullptr_t>
    : public true_type { };

  template<>
    struct is_null_pointer<volatile std::nullptr_t>
    : public true_type { };

  template<>
    struct is_null_pointer<const volatile std::nullptr_t>
    : public true_type { };



  template<typename _Tp>
    struct __is_nullptr_t
    : public is_null_pointer<_Tp>
    { } __attribute__ ((__deprecated__ ("use '" "std::is_null_pointer" "' instead")));






  template<typename _Tp>
    struct is_reference
    : public __bool_constant<__is_reference(_Tp)>
    { };
# 734 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_arithmetic
    : public __or_<is_integral<_Tp>, is_floating_point<_Tp>>::type
    { };


  template<typename _Tp>
    struct is_fundamental
    : public __or_<is_arithmetic<_Tp>, is_void<_Tp>,
     is_null_pointer<_Tp>>::type
    { };



  template<typename _Tp>
    struct is_object
    : public __bool_constant<__is_object(_Tp)>
    { };
# 760 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename>
    struct is_member_pointer;


  template<typename _Tp>
    struct is_scalar
    : public __or_<is_arithmetic<_Tp>, is_enum<_Tp>, is_pointer<_Tp>,
                   is_member_pointer<_Tp>, is_null_pointer<_Tp>>::type
    { };


  template<typename _Tp>
    struct is_compound
    : public __bool_constant<!is_fundamental<_Tp>::value> { };



  template<typename _Tp>
    struct is_member_pointer
    : public __bool_constant<__is_member_pointer(_Tp)>
    { };
# 798 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename, typename>
    struct is_same;


  template<typename _Tp, typename... _Types>
    using __is_one_of = __or_<is_same<_Tp, _Types>...>;


  __extension__
  template<typename _Tp>
    using __is_signed_integer = __is_one_of<__remove_cv_t<_Tp>,
   signed char, signed short, signed int, signed long,
   signed long long
# 823 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
   >;


  __extension__
  template<typename _Tp>
    using __is_unsigned_integer = __is_one_of<__remove_cv_t<_Tp>,
   unsigned char, unsigned short, unsigned int, unsigned long,
   unsigned long long
# 843 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
   >;


  template<typename _Tp>
    using __is_standard_integer
      = __or_<__is_signed_integer<_Tp>, __is_unsigned_integer<_Tp>>;


  template<typename...> using __void_t = void;






  template<typename _Tp>
    struct is_const
    : public __bool_constant<__is_const(_Tp)>
    { };
# 874 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_volatile
    : public __bool_constant<__is_volatile(_Tp)>
    { };
# 895 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct
   
    is_trivial
    : public __bool_constant<__is_trivial(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_copyable
    : public __bool_constant<__is_trivially_copyable(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_standard_layout
    : public __bool_constant<__is_standard_layout(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };






  template<typename _Tp>
    struct
   
    is_pod
    : public __bool_constant<__is_pod(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };





  template<typename _Tp>
    struct
   
    is_literal_type
    : public __bool_constant<__is_literal_type(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_empty
    : public __bool_constant<__is_empty(_Tp)>
    { };


  template<typename _Tp>
    struct is_polymorphic
    : public __bool_constant<__is_polymorphic(_Tp)>
    { };




  template<typename _Tp>
    struct is_final
    : public __bool_constant<__is_final(_Tp)>
    { };



  template<typename _Tp>
    struct is_abstract
    : public __bool_constant<__is_abstract(_Tp)>
    { };


  template<typename _Tp,
    bool = is_arithmetic<_Tp>::value>
    struct __is_signed_helper
    : public false_type { };

  template<typename _Tp>
    struct __is_signed_helper<_Tp, true>
    : public __bool_constant<_Tp(-1) < _Tp(0)>
    { };



  template<typename _Tp>
    struct is_signed
    : public __is_signed_helper<_Tp>::type
    { };


  template<typename _Tp>
    struct is_unsigned
    : public __and_<is_arithmetic<_Tp>, __not_<is_signed<_Tp>>>::type
    { };


  template<typename _Tp, typename _Up = _Tp&&>
    _Up
    __declval(int);

  template<typename _Tp>
    _Tp
    __declval(long);


  template<typename _Tp>
    auto declval() noexcept -> decltype(__declval<_Tp>(0));

  template<typename>
    struct remove_all_extents;


  template<typename _Tp>
    struct __is_array_known_bounds
    : public false_type
    { };

  template<typename _Tp, size_t _Size>
    struct __is_array_known_bounds<_Tp[_Size]>
    : public true_type
    { };

  template<typename _Tp>
    struct __is_array_unknown_bounds
    : public false_type
    { };

  template<typename _Tp>
    struct __is_array_unknown_bounds<_Tp[]>
    : public true_type
    { };
# 1047 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  struct __do_is_destructible_impl
  {
    template<typename _Tp, typename = decltype(declval<_Tp&>().~_Tp())>
      static true_type __test(int);

    template<typename>
      static false_type __test(...);
  };

  template<typename _Tp>
    struct __is_destructible_impl
    : public __do_is_destructible_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp,
           bool = __or_<is_void<_Tp>,
                        __is_array_unknown_bounds<_Tp>,
                        is_function<_Tp>>::value,
           bool = __or_<is_reference<_Tp>, is_scalar<_Tp>>::value>
    struct __is_destructible_safe;

  template<typename _Tp>
    struct __is_destructible_safe<_Tp, false, false>
    : public __is_destructible_impl<typename
               remove_all_extents<_Tp>::type>::type
    { };

  template<typename _Tp>
    struct __is_destructible_safe<_Tp, true, false>
    : public false_type { };

  template<typename _Tp>
    struct __is_destructible_safe<_Tp, false, true>
    : public true_type { };



  template<typename _Tp>
    struct is_destructible
    : public __is_destructible_safe<_Tp>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };







  struct __do_is_nt_destructible_impl
  {
    template<typename _Tp>
      static __bool_constant<noexcept(declval<_Tp&>().~_Tp())>
      __test(int);

    template<typename>
      static false_type __test(...);
  };

  template<typename _Tp>
    struct __is_nt_destructible_impl
    : public __do_is_nt_destructible_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp,
           bool = __or_<is_void<_Tp>,
                        __is_array_unknown_bounds<_Tp>,
                        is_function<_Tp>>::value,
           bool = __or_<is_reference<_Tp>, is_scalar<_Tp>>::value>
    struct __is_nt_destructible_safe;

  template<typename _Tp>
    struct __is_nt_destructible_safe<_Tp, false, false>
    : public __is_nt_destructible_impl<typename
               remove_all_extents<_Tp>::type>::type
    { };

  template<typename _Tp>
    struct __is_nt_destructible_safe<_Tp, true, false>
    : public false_type { };

  template<typename _Tp>
    struct __is_nt_destructible_safe<_Tp, false, true>
    : public true_type { };



  template<typename _Tp>
    struct is_nothrow_destructible
    : public __is_nt_destructible_safe<_Tp>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename... _Args>
    using __is_constructible_impl
      = __bool_constant<__is_constructible(_Tp, _Args...)>;



  template<typename _Tp, typename... _Args>
    struct is_constructible
      : public __is_constructible_impl<_Tp, _Args...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_default_constructible
    : public __is_constructible_impl<_Tp>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    using __add_lval_ref_t = __add_lvalue_reference(_Tp);
# 1191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_copy_constructible
    : public __is_constructible_impl<_Tp, __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    using __add_rval_ref_t = __add_rvalue_reference(_Tp);
# 1218 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct is_move_constructible
    : public __is_constructible_impl<_Tp, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename... _Args>
    using __is_nothrow_constructible_impl
      = __bool_constant<__is_nothrow_constructible(_Tp, _Args...)>;



  template<typename _Tp, typename... _Args>
    struct is_nothrow_constructible
    : public __is_nothrow_constructible_impl<_Tp, _Args...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_default_constructible
    : public __is_nothrow_constructible_impl<_Tp>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_copy_constructible
    : public __is_nothrow_constructible_impl<_Tp, __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_move_constructible
    : public __is_nothrow_constructible_impl<_Tp, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    using __is_assignable_impl = __bool_constant<__is_assignable(_Tp, _Up)>;



  template<typename _Tp, typename _Up>
    struct is_assignable
    : public __is_assignable_impl<_Tp, _Up>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_copy_assignable
    : public __is_assignable_impl<__add_lval_ref_t<_Tp>,
      __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_move_assignable
    : public __is_assignable_impl<__add_lval_ref_t<_Tp>, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    using __is_nothrow_assignable_impl
      = __bool_constant<__is_nothrow_assignable(_Tp, _Up)>;



  template<typename _Tp, typename _Up>
    struct is_nothrow_assignable
    : public __is_nothrow_assignable_impl<_Tp, _Up>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_copy_assignable
    : public __is_nothrow_assignable_impl<__add_lval_ref_t<_Tp>,
       __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_nothrow_move_assignable
    : public __is_nothrow_assignable_impl<__add_lval_ref_t<_Tp>,
       __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename... _Args>
    using __is_trivially_constructible_impl
      = __bool_constant<__is_trivially_constructible(_Tp, _Args...)>;



  template<typename _Tp, typename... _Args>
    struct is_trivially_constructible
    : public __is_trivially_constructible_impl<_Tp, _Args...>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_default_constructible
    : public __is_trivially_constructible_impl<_Tp>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };
# 1370 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  struct __do_is_implicitly_default_constructible_impl
  {
    template <typename _Tp>
    static void __helper(const _Tp&);

    template <typename _Tp>
    static true_type __test(const _Tp&,
                            decltype(__helper<const _Tp&>({}))* = 0);

    static false_type __test(...);
  };

  template<typename _Tp>
    struct __is_implicitly_default_constructible_impl
    : public __do_is_implicitly_default_constructible_impl
    {
      using type = decltype(__test(declval<_Tp>()));
    };

  template<typename _Tp>
    struct __is_implicitly_default_constructible_safe
    : public __is_implicitly_default_constructible_impl<_Tp>::type
    { };

  template <typename _Tp>
    struct __is_implicitly_default_constructible
    : public __and_<__is_constructible_impl<_Tp>,
      __is_implicitly_default_constructible_safe<_Tp>>::type
    { };



  template<typename _Tp>
    struct is_trivially_copy_constructible
    : public __is_trivially_constructible_impl<_Tp, __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_move_constructible
    : public __is_trivially_constructible_impl<_Tp, __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp, typename _Up>
    using __is_trivially_assignable_impl
      = __bool_constant<__is_trivially_assignable(_Tp, _Up)>;



  template<typename _Tp, typename _Up>
    struct is_trivially_assignable
    : public __is_trivially_assignable_impl<_Tp, _Up>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_copy_assignable
    : public __is_trivially_assignable_impl<__add_lval_ref_t<_Tp>,
         __add_lval_ref_t<const _Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_move_assignable
    : public __is_trivially_assignable_impl<__add_lval_ref_t<_Tp>,
         __add_rval_ref_t<_Tp>>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };


  template<typename _Tp>
    struct is_trivially_destructible
    : public __and_<__is_destructible_safe<_Tp>,
      __bool_constant<__has_trivial_destructor(_Tp)>>::type
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    struct has_virtual_destructor
    : public __bool_constant<__has_virtual_destructor(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };





  template<typename _Tp>
    struct alignment_of
    : public integral_constant<std::size_t, alignof(_Tp)>
    {
      static_assert(std::__is_complete_or_unbounded(__type_identity<_Tp>{}),
 "template argument must be a complete class or an unbounded array");
    };



  template<typename _Tp>
    struct rank
    : public integral_constant<std::size_t, __array_rank(_Tp)> { };
# 1506 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename, unsigned _Uint = 0>
    struct extent
    : public integral_constant<size_t, 0> { };

  template<typename _Tp, size_t _Size>
    struct extent<_Tp[_Size], 0>
    : public integral_constant<size_t, _Size> { };

  template<typename _Tp, unsigned _Uint, size_t _Size>
    struct extent<_Tp[_Size], _Uint>
    : public extent<_Tp, _Uint - 1>::type { };

  template<typename _Tp>
    struct extent<_Tp[], 0>
    : public integral_constant<size_t, 0> { };

  template<typename _Tp, unsigned _Uint>
    struct extent<_Tp[], _Uint>
    : public extent<_Tp, _Uint - 1>::type { };






  template<typename _Tp, typename _Up>
    struct is_same
    : public __bool_constant<__is_same(_Tp, _Up)>
    { };
# 1548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Base, typename _Derived>
    struct is_base_of
    : public __bool_constant<__is_base_of(_Base, _Derived)>
    { };
# 1563 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _From, typename _To>
    struct is_convertible
    : public __bool_constant<__is_convertible(_From, _To)>
    { };
# 1606 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _ToElementType, typename _FromElementType>
    using __is_array_convertible
      = is_convertible<_FromElementType(*)[], _ToElementType(*)[]>;
# 1666 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
  template<typename _Tp, typename... _Args>
    struct __is_nothrow_new_constructible_impl
    : __bool_constant<
 noexcept(::new(std::declval<void*>()) _Tp(std::declval<_Args>()...))
      >
    { };

  template<typename _Tp, typename... _Args>
    constexpr bool __is_nothrow_new_constructible
      = __and_<is_constructible<_Tp, _Args...>,
        __is_nothrow_new_constructible_impl<_Tp, _Args...>>::value;
#pragma GCC diagnostic pop




  template<typename _Tp>
    struct remove_const
    { using type = _Tp; };

  template<typename _Tp>
    struct remove_const<_Tp const>
    { using type = _Tp; };


  template<typename _Tp>
    struct remove_volatile
    { using type = _Tp; };

  template<typename _Tp>
    struct remove_volatile<_Tp volatile>
    { using type = _Tp; };



  template<typename _Tp>
    struct remove_cv
    { using type = __remove_cv(_Tp); };
# 1725 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_const
    { using type = _Tp const; };


  template<typename _Tp>
    struct add_volatile
    { using type = _Tp volatile; };


  template<typename _Tp>
    struct add_cv
    { using type = _Tp const volatile; };



  template<typename _Tp>
    using remove_const_t = typename remove_const<_Tp>::type;


  template<typename _Tp>
    using remove_volatile_t = typename remove_volatile<_Tp>::type;


  template<typename _Tp>
    using remove_cv_t = typename remove_cv<_Tp>::type;


  template<typename _Tp>
    using add_const_t = typename add_const<_Tp>::type;


  template<typename _Tp>
    using add_volatile_t = typename add_volatile<_Tp>::type;


  template<typename _Tp>
    using add_cv_t = typename add_cv<_Tp>::type;






  template<typename _Tp>
    struct remove_reference
    { using type = __remove_reference(_Tp); };
# 1787 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_lvalue_reference
    { using type = __add_lval_ref_t<_Tp>; };


  template<typename _Tp>
    struct add_rvalue_reference
    { using type = __add_rval_ref_t<_Tp>; };



  template<typename _Tp>
    using remove_reference_t = typename remove_reference<_Tp>::type;


  template<typename _Tp>
    using add_lvalue_reference_t = typename add_lvalue_reference<_Tp>::type;


  template<typename _Tp>
    using add_rvalue_reference_t = typename add_rvalue_reference<_Tp>::type;







  template<typename _Unqualified, bool _IsConst, bool _IsVol>
    struct __cv_selector;

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, false, false>
    { using __type = _Unqualified; };

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, false, true>
    { using __type = volatile _Unqualified; };

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, true, false>
    { using __type = const _Unqualified; };

  template<typename _Unqualified>
    struct __cv_selector<_Unqualified, true, true>
    { using __type = const volatile _Unqualified; };

  template<typename _Qualified, typename _Unqualified,
    bool _IsConst = is_const<_Qualified>::value,
    bool _IsVol = is_volatile<_Qualified>::value>
    class __match_cv_qualifiers
    {
      using __match = __cv_selector<_Unqualified, _IsConst, _IsVol>;

    public:
      using __type = typename __match::__type;
    };


  template<typename _Tp>
    struct __make_unsigned
    { using __type = _Tp; };

  template<>
    struct __make_unsigned<char>
    { using __type = unsigned char; };

  template<>
    struct __make_unsigned<signed char>
    { using __type = unsigned char; };

  template<>
    struct __make_unsigned<short>
    { using __type = unsigned short; };

  template<>
    struct __make_unsigned<int>
    { using __type = unsigned int; };

  template<>
    struct __make_unsigned<long>
    { using __type = unsigned long; };

  template<>
    struct __make_unsigned<long long>
    { using __type = unsigned long long; };
# 1900 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp,
    bool _IsInt = is_integral<_Tp>::value,
    bool _IsEnum = __is_enum(_Tp)>
    class __make_unsigned_selector;

  template<typename _Tp>
    class __make_unsigned_selector<_Tp, true, false>
    {
      using __unsigned_type
 = typename __make_unsigned<__remove_cv_t<_Tp>>::__type;

    public:
      using __type
 = typename __match_cv_qualifiers<_Tp, __unsigned_type>::__type;
    };

  class __make_unsigned_selector_base
  {
  protected:
    template<typename...> struct _List { };

    template<typename _Tp, typename... _Up>
      struct _List<_Tp, _Up...> : _List<_Up...>
      { static constexpr size_t __size = sizeof(_Tp); };

    template<size_t _Sz, typename _Tp, bool = (_Sz <= _Tp::__size)>
      struct __select;

    template<size_t _Sz, typename _Uint, typename... _UInts>
      struct __select<_Sz, _List<_Uint, _UInts...>, true>
      { using __type = _Uint; };

    template<size_t _Sz, typename _Uint, typename... _UInts>
      struct __select<_Sz, _List<_Uint, _UInts...>, false>
      : __select<_Sz, _List<_UInts...>>
      { };
  };


  template<typename _Tp>
    class __make_unsigned_selector<_Tp, false, true>
    : __make_unsigned_selector_base
    {

      using _UInts = _List<unsigned char, unsigned short, unsigned int,
      unsigned long, unsigned long long>;

      using __unsigned_type = typename __select<sizeof(_Tp), _UInts>::__type;

    public:
      using __type
 = typename __match_cv_qualifiers<_Tp, __unsigned_type>::__type;
    };





  template<>
    struct __make_unsigned<wchar_t>
    {
      using __type
 = typename __make_unsigned_selector<wchar_t, false, true>::__type;
    };
# 1974 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<>
    struct __make_unsigned<char16_t>
    {
      using __type
 = typename __make_unsigned_selector<char16_t, false, true>::__type;
    };

  template<>
    struct __make_unsigned<char32_t>
    {
      using __type
 = typename __make_unsigned_selector<char32_t, false, true>::__type;
    };






  template<typename _Tp>
    struct make_unsigned
    { using type = typename __make_unsigned_selector<_Tp>::__type; };


  template<> struct make_unsigned<bool>;
  template<> struct make_unsigned<bool const>;
  template<> struct make_unsigned<bool volatile>;
  template<> struct make_unsigned<bool const volatile>;




  template<typename _Tp>
    struct __make_signed
    { using __type = _Tp; };

  template<>
    struct __make_signed<char>
    { using __type = signed char; };

  template<>
    struct __make_signed<unsigned char>
    { using __type = signed char; };

  template<>
    struct __make_signed<unsigned short>
    { using __type = signed short; };

  template<>
    struct __make_signed<unsigned int>
    { using __type = signed int; };

  template<>
    struct __make_signed<unsigned long>
    { using __type = signed long; };

  template<>
    struct __make_signed<unsigned long long>
    { using __type = signed long long; };
# 2060 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp,
    bool _IsInt = is_integral<_Tp>::value,
    bool _IsEnum = __is_enum(_Tp)>
    class __make_signed_selector;

  template<typename _Tp>
    class __make_signed_selector<_Tp, true, false>
    {
      using __signed_type
 = typename __make_signed<__remove_cv_t<_Tp>>::__type;

    public:
      using __type
 = typename __match_cv_qualifiers<_Tp, __signed_type>::__type;
    };


  template<typename _Tp>
    class __make_signed_selector<_Tp, false, true>
    {
      using __unsigned_type = typename __make_unsigned_selector<_Tp>::__type;

    public:
      using __type = typename __make_signed_selector<__unsigned_type>::__type;
    };





  template<>
    struct __make_signed<wchar_t>
    {
      using __type
 = typename __make_signed_selector<wchar_t, false, true>::__type;
    };
# 2106 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<>
    struct __make_signed<char16_t>
    {
      using __type
 = typename __make_signed_selector<char16_t, false, true>::__type;
    };

  template<>
    struct __make_signed<char32_t>
    {
      using __type
 = typename __make_signed_selector<char32_t, false, true>::__type;
    };






  template<typename _Tp>
    struct make_signed
    { using type = typename __make_signed_selector<_Tp>::__type; };


  template<> struct make_signed<bool>;
  template<> struct make_signed<bool const>;
  template<> struct make_signed<bool volatile>;
  template<> struct make_signed<bool const volatile>;



  template<typename _Tp>
    using make_signed_t = typename make_signed<_Tp>::type;


  template<typename _Tp>
    using make_unsigned_t = typename make_unsigned<_Tp>::type;






  template<typename _Tp>
    struct remove_extent
    { using type = __remove_extent(_Tp); };
# 2168 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct remove_all_extents
    { using type = __remove_all_extents(_Tp); };
# 2187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    using remove_extent_t = typename remove_extent<_Tp>::type;


  template<typename _Tp>
    using remove_all_extents_t = typename remove_all_extents<_Tp>::type;






  template<typename _Tp>
    struct remove_pointer
    { using type = __remove_pointer(_Tp); };
# 2219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_pointer
    { using type = __add_pointer(_Tp); };
# 2247 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    using remove_pointer_t = typename remove_pointer<_Tp>::type;


  template<typename _Tp>
    using add_pointer_t = typename add_pointer<_Tp>::type;





  struct __attribute__((__aligned__)) __aligned_storage_max_align_t
  { };

  constexpr size_t
  __aligned_storage_default_alignment([[__maybe_unused__]] size_t __len)
  {
# 2278 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
    return alignof(__aligned_storage_max_align_t);

  }
# 2314 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<size_t _Len,
    size_t _Align = __aligned_storage_default_alignment(_Len)>
    struct
   
    aligned_storage
    {
      struct type
      {
 alignas(_Align) unsigned char __data[_Len];
      };
    };

  template <typename... _Types>
    struct __strictest_alignment
    {
      static const size_t _S_alignment = 0;
      static const size_t _S_size = 0;
    };

  template <typename _Tp, typename... _Types>
    struct __strictest_alignment<_Tp, _Types...>
    {
      static const size_t _S_alignment =
        alignof(_Tp) > __strictest_alignment<_Types...>::_S_alignment
 ? alignof(_Tp) : __strictest_alignment<_Types...>::_S_alignment;
      static const size_t _S_size =
        sizeof(_Tp) > __strictest_alignment<_Types...>::_S_size
 ? sizeof(_Tp) : __strictest_alignment<_Types...>::_S_size;
    };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 2359 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template <size_t _Len, typename... _Types>
    struct
   
    aligned_union
    {
    private:
      static_assert(sizeof...(_Types) != 0, "At least one type is required");

      using __strictest = __strictest_alignment<_Types...>;
      static const size_t _S_len = _Len > __strictest::_S_size
 ? _Len : __strictest::_S_size;
    public:

      static const size_t alignment_value = __strictest::_S_alignment;

      using type = typename aligned_storage<_S_len, alignment_value>::type;
    };

  template <size_t _Len, typename... _Types>
    const size_t aligned_union<_Len, _Types...>::alignment_value;
#pragma GCC diagnostic pop




  template<typename _Tp>
    struct decay
    { using type = __decay(_Tp); };
# 2424 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct __strip_reference_wrapper
    {
      using __type = _Tp;
    };

  template<typename _Tp>
    struct __strip_reference_wrapper<reference_wrapper<_Tp> >
    {
      using __type = _Tp&;
    };


  template<typename _Tp>
    using __decay_t = typename decay<_Tp>::type;

  template<typename _Tp>
    using __decay_and_strip = __strip_reference_wrapper<__decay_t<_Tp>>;





  template<typename... _Cond>
    using _Require = __enable_if_t<__and_<_Cond...>::value>;


  template<typename _Tp>
    using __remove_cvref_t
     = typename remove_cv<typename remove_reference<_Tp>::type>::type;




  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    struct conditional
    { using type = _Iftrue; };


  template<typename _Iftrue, typename _Iffalse>
    struct conditional<false, _Iftrue, _Iffalse>
    { using type = _Iffalse; };


  template<typename... _Tp>
    struct common_type;
# 2480 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct __success_type
    { using type = _Tp; };

  struct __failure_type
  { };

  struct __do_common_type_impl
  {
    template<typename _Tp, typename _Up>
      using __cond_t
 = decltype(true ? std::declval<_Tp>() : std::declval<_Up>());



    template<typename _Tp, typename _Up>
      static __success_type<__decay_t<__cond_t<_Tp, _Up>>>
      _S_test(int);
# 2507 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
    template<typename, typename>
      static __failure_type
      _S_test_2(...);

    template<typename _Tp, typename _Up>
      static decltype(_S_test_2<_Tp, _Up>(0))
      _S_test(...);
  };


  template<>
    struct common_type<>
    { };


  template<typename _Tp0>
    struct common_type<_Tp0>
    : public common_type<_Tp0, _Tp0>
    { };


  template<typename _Tp1, typename _Tp2,
    typename _Dp1 = __decay_t<_Tp1>, typename _Dp2 = __decay_t<_Tp2>>
    struct __common_type_impl
    {


      using type = common_type<_Dp1, _Dp2>;
    };

  template<typename _Tp1, typename _Tp2>
    struct __common_type_impl<_Tp1, _Tp2, _Tp1, _Tp2>
    : private __do_common_type_impl
    {


      using type = decltype(_S_test<_Tp1, _Tp2>(0));
    };


  template<typename _Tp1, typename _Tp2>
    struct common_type<_Tp1, _Tp2>
    : public __common_type_impl<_Tp1, _Tp2>::type
    { };

  template<typename...>
    struct __common_type_pack
    { };

  template<typename, typename, typename = void>
    struct __common_type_fold;


  template<typename _Tp1, typename _Tp2, typename... _Rp>
    struct common_type<_Tp1, _Tp2, _Rp...>
    : public __common_type_fold<common_type<_Tp1, _Tp2>,
    __common_type_pack<_Rp...>>
    { };




  template<typename _CTp, typename... _Rp>
    struct __common_type_fold<_CTp, __common_type_pack<_Rp...>,
         __void_t<typename _CTp::type>>
    : public common_type<typename _CTp::type, _Rp...>
    { };


  template<typename _CTp, typename _Rp>
    struct __common_type_fold<_CTp, _Rp, void>
    { };

  template<typename _Tp, bool = __is_enum(_Tp)>
    struct __underlying_type_impl
    {
      using type = __underlying_type(_Tp);
    };

  template<typename _Tp>
    struct __underlying_type_impl<_Tp, false>
    { };



  template<typename _Tp>
    struct underlying_type
    : public __underlying_type_impl<_Tp>
    { };


  template<typename _Tp>
    struct __declval_protector
    {
      static const bool __stop = false;
    };






  template<typename _Tp>
    auto declval() noexcept -> decltype(__declval<_Tp>(0))
    {
      static_assert(__declval_protector<_Tp>::__stop,
      "declval() must not be used!");
      return __declval<_Tp>(0);
    }


  template<typename _Signature>
    struct result_of;




  struct __invoke_memfun_ref { };
  struct __invoke_memfun_deref { };
  struct __invoke_memobj_ref { };
  struct __invoke_memobj_deref { };
  struct __invoke_other { };


  template<typename _Tp, typename _Tag>
    struct __result_of_success : __success_type<_Tp>
    { using __invoke_type = _Tag; };


  struct __result_of_memfun_ref_impl
  {
    template<typename _Fp, typename _Tp1, typename... _Args>
      static __result_of_success<decltype(
      (std::declval<_Tp1>().*std::declval<_Fp>())(std::declval<_Args>()...)
      ), __invoke_memfun_ref> _S_test(int);

    template<typename...>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_memfun_ref
    : private __result_of_memfun_ref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg, _Args...>(0));
    };


  struct __result_of_memfun_deref_impl
  {
    template<typename _Fp, typename _Tp1, typename... _Args>
      static __result_of_success<decltype(
      ((*std::declval<_Tp1>()).*std::declval<_Fp>())(std::declval<_Args>()...)
      ), __invoke_memfun_deref> _S_test(int);

    template<typename...>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_memfun_deref
    : private __result_of_memfun_deref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg, _Args...>(0));
    };


  struct __result_of_memobj_ref_impl
  {
    template<typename _Fp, typename _Tp1>
      static __result_of_success<decltype(
      std::declval<_Tp1>().*std::declval<_Fp>()
      ), __invoke_memobj_ref> _S_test(int);

    template<typename, typename>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_memobj_ref
    : private __result_of_memobj_ref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg>(0));
    };


  struct __result_of_memobj_deref_impl
  {
    template<typename _Fp, typename _Tp1>
      static __result_of_success<decltype(
      (*std::declval<_Tp1>()).*std::declval<_Fp>()
      ), __invoke_memobj_deref> _S_test(int);

    template<typename, typename>
      static __failure_type _S_test(...);
  };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_memobj_deref
    : private __result_of_memobj_deref_impl
    {
      using type = decltype(_S_test<_MemPtr, _Arg>(0));
    };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_memobj;

  template<typename _Res, typename _Class, typename _Arg>
    struct __result_of_memobj<_Res _Class::*, _Arg>
    {
      using _Argval = __remove_cvref_t<_Arg>;
      using _MemPtr = _Res _Class::*;
      using type = typename __conditional_t<__or_<is_same<_Argval, _Class>,
        is_base_of<_Class, _Argval>>::value,
        __result_of_memobj_ref<_MemPtr, _Arg>,
        __result_of_memobj_deref<_MemPtr, _Arg>
      >::type;
    };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_memfun;

  template<typename _Res, typename _Class, typename _Arg, typename... _Args>
    struct __result_of_memfun<_Res _Class::*, _Arg, _Args...>
    {
      using _Argval = typename remove_reference<_Arg>::type;
      using _MemPtr = _Res _Class::*;
      using type = typename __conditional_t<is_base_of<_Class, _Argval>::value,
        __result_of_memfun_ref<_MemPtr, _Arg, _Args...>,
        __result_of_memfun_deref<_MemPtr, _Arg, _Args...>
      >::type;
    };






  template<typename _Tp, typename _Up = __remove_cvref_t<_Tp>>
    struct __inv_unwrap
    {
      using type = _Tp;
    };

  template<typename _Tp, typename _Up>
    struct __inv_unwrap<_Tp, reference_wrapper<_Up>>
    {
      using type = _Up&;
    };

  template<bool, bool, typename _Functor, typename... _ArgTypes>
    struct __result_of_impl
    {
      using type = __failure_type;
    };

  template<typename _MemPtr, typename _Arg>
    struct __result_of_impl<true, false, _MemPtr, _Arg>
    : public __result_of_memobj<__decay_t<_MemPtr>,
    typename __inv_unwrap<_Arg>::type>
    { };

  template<typename _MemPtr, typename _Arg, typename... _Args>
    struct __result_of_impl<false, true, _MemPtr, _Arg, _Args...>
    : public __result_of_memfun<__decay_t<_MemPtr>,
    typename __inv_unwrap<_Arg>::type, _Args...>
    { };


  struct __result_of_other_impl
  {
    template<typename _Fn, typename... _Args>
      static __result_of_success<decltype(
      std::declval<_Fn>()(std::declval<_Args>()...)
      ), __invoke_other> _S_test(int);

    template<typename...>
      static __failure_type _S_test(...);
  };

  template<typename _Functor, typename... _ArgTypes>
    struct __result_of_impl<false, false, _Functor, _ArgTypes...>
    : private __result_of_other_impl
    {
      using type = decltype(_S_test<_Functor, _ArgTypes...>(0));
    };


  template<typename _Functor, typename... _ArgTypes>
    struct __invoke_result
    : public __result_of_impl<
        is_member_object_pointer<
          typename remove_reference<_Functor>::type
        >::value,
        is_member_function_pointer<
          typename remove_reference<_Functor>::type
        >::value,
 _Functor, _ArgTypes...
      >::type
    { };


  template<typename _Fn, typename... _Args>
    using __invoke_result_t = typename __invoke_result<_Fn, _Args...>::type;


  template<typename _Functor, typename... _ArgTypes>
    struct result_of<_Functor(_ArgTypes...)>
    : public __invoke_result<_Functor, _ArgTypes...>
    { } ;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

  template<size_t _Len,
    size_t _Align = __aligned_storage_default_alignment(_Len)>
    using aligned_storage_t = typename aligned_storage<_Len, _Align>::type;

  template <size_t _Len, typename... _Types>
    using aligned_union_t = typename aligned_union<_Len, _Types...>::type;
#pragma GCC diagnostic pop


  template<typename _Tp>
    using decay_t = typename decay<_Tp>::type;


  template<bool _Cond, typename _Tp = void>
    using enable_if_t = typename enable_if<_Cond, _Tp>::type;


  template<bool _Cond, typename _Iftrue, typename _Iffalse>
    using conditional_t = typename conditional<_Cond, _Iftrue, _Iffalse>::type;


  template<typename... _Tp>
    using common_type_t = typename common_type<_Tp...>::type;


  template<typename _Tp>
    using underlying_type_t = typename underlying_type<_Tp>::type;


  template<typename _Tp>
    using result_of_t = typename result_of<_Tp>::type;
# 2884 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Default, typename _AlwaysVoid,
    template<typename...> class _Op, typename... _Args>
    struct __detector
    {
      using type = _Default;
      using __is_detected = false_type;
    };


  template<typename _Default, template<typename...> class _Op,
     typename... _Args>
    struct __detector<_Default, __void_t<_Op<_Args...>>, _Op, _Args...>
    {
      using type = _Op<_Args...>;
      using __is_detected = true_type;
    };

  template<typename _Default, template<typename...> class _Op,
    typename... _Args>
    using __detected_or = __detector<_Default, void, _Op, _Args...>;



  template<typename _Default, template<typename...> class _Op,
    typename... _Args>
    using __detected_or_t
      = typename __detected_or<_Default, _Op, _Args...>::type;
# 2926 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template <typename _Tp>
    struct __is_swappable;

  template <typename _Tp>
    struct __is_nothrow_swappable;

  template<typename>
    struct __is_tuple_like_impl : false_type
    { };


  template<typename _Tp>
    struct __is_tuple_like
    : public __is_tuple_like_impl<__remove_cvref_t<_Tp>>::type
    { };


  template<typename _Tp>
   
    inline
    _Require<__not_<__is_tuple_like<_Tp>>,
      is_move_constructible<_Tp>,
      is_move_assignable<_Tp>>
    swap(_Tp&, _Tp&)
    noexcept(__and_<is_nothrow_move_constructible<_Tp>,
             is_nothrow_move_assignable<_Tp>>::value);

  template<typename _Tp, size_t _Nm>
   
    inline
    __enable_if_t<__is_swappable<_Tp>::value>
    swap(_Tp (&__a)[_Nm], _Tp (&__b)[_Nm])
    noexcept(__is_nothrow_swappable<_Tp>::value);


  namespace __swappable_details {
    using std::swap;

    struct __do_is_swappable_impl
    {
      template<typename _Tp, typename
               = decltype(swap(std::declval<_Tp&>(), std::declval<_Tp&>()))>
        static true_type __test(int);

      template<typename>
        static false_type __test(...);
    };

    struct __do_is_nothrow_swappable_impl
    {
      template<typename _Tp>
        static __bool_constant<
          noexcept(swap(std::declval<_Tp&>(), std::declval<_Tp&>()))
        > __test(int);

      template<typename>
        static false_type __test(...);
    };

  }

  template<typename _Tp>
    struct __is_swappable_impl
    : public __swappable_details::__do_is_swappable_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp>
    struct __is_nothrow_swappable_impl
    : public __swappable_details::__do_is_nothrow_swappable_impl
    {
      using type = decltype(__test<_Tp>(0));
    };

  template<typename _Tp>
    struct __is_swappable
    : public __is_swappable_impl<_Tp>::type
    { };

  template<typename _Tp>
    struct __is_nothrow_swappable
    : public __is_nothrow_swappable_impl<_Tp>::type
    { };
# 3148 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Result, typename _Ret,
    bool = is_void<_Ret>::value, typename = void>
    struct __is_invocable_impl
    : false_type
    {
      using __nothrow_conv = false_type;
    };


  template<typename _Result, typename _Ret>
    struct __is_invocable_impl<_Result, _Ret,
                                true,
          __void_t<typename _Result::type>>
    : true_type
    {
      using __nothrow_conv = true_type;
    };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wctor-dtor-privacy"

  template<typename _Result, typename _Ret>
    struct __is_invocable_impl<_Result, _Ret,
                                false,
          __void_t<typename _Result::type>>
    {
    private:

      using _Res_t = typename _Result::type;



      static _Res_t _S_get() noexcept;


      template<typename _Tp>
 static void _S_conv(__type_identity_t<_Tp>) noexcept;


      template<typename _Tp,
        bool _Nothrow = noexcept(_S_conv<_Tp>(_S_get())),
        typename = decltype(_S_conv<_Tp>(_S_get())),

        bool _Dangle = __reference_converts_from_temporary(_Tp, _Res_t)



       >
 static __bool_constant<_Nothrow && !_Dangle>
 _S_test(int);

      template<typename _Tp, bool = false>
 static false_type
 _S_test(...);

    public:

      using type = decltype(_S_test<_Ret, true>(1));


      using __nothrow_conv = decltype(_S_test<_Ret>(1));
    };
#pragma GCC diagnostic pop

  template<typename _Fn, typename... _ArgTypes>
    struct __is_invocable
    : __is_invocable_impl<__invoke_result<_Fn, _ArgTypes...>, void>::type
    { };

  template<typename _Fn, typename _Tp, typename... _Args>
    constexpr bool __call_is_nt(__invoke_memfun_ref)
    {
      using _Up = typename __inv_unwrap<_Tp>::type;
      return noexcept((std::declval<_Up>().*std::declval<_Fn>())(
     std::declval<_Args>()...));
    }

  template<typename _Fn, typename _Tp, typename... _Args>
    constexpr bool __call_is_nt(__invoke_memfun_deref)
    {
      return noexcept(((*std::declval<_Tp>()).*std::declval<_Fn>())(
     std::declval<_Args>()...));
    }

  template<typename _Fn, typename _Tp>
    constexpr bool __call_is_nt(__invoke_memobj_ref)
    {
      using _Up = typename __inv_unwrap<_Tp>::type;
      return noexcept(std::declval<_Up>().*std::declval<_Fn>());
    }

  template<typename _Fn, typename _Tp>
    constexpr bool __call_is_nt(__invoke_memobj_deref)
    {
      return noexcept((*std::declval<_Tp>()).*std::declval<_Fn>());
    }

  template<typename _Fn, typename... _Args>
    constexpr bool __call_is_nt(__invoke_other)
    {
      return noexcept(std::declval<_Fn>()(std::declval<_Args>()...));
    }

  template<typename _Result, typename _Fn, typename... _Args>
    struct __call_is_nothrow
    : __bool_constant<
 std::__call_is_nt<_Fn, _Args...>(typename _Result::__invoke_type{})
      >
    { };

  template<typename _Fn, typename... _Args>
    using __call_is_nothrow_
      = __call_is_nothrow<__invoke_result<_Fn, _Args...>, _Fn, _Args...>;


  template<typename _Fn, typename... _Args>
    struct __is_nothrow_invocable
    : __and_<__is_invocable<_Fn, _Args...>,
             __call_is_nothrow_<_Fn, _Args...>>::type
    { };

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wctor-dtor-privacy"
  struct __nonesuchbase {};
  struct __nonesuch : private __nonesuchbase {
    ~__nonesuch() = delete;
    __nonesuch(__nonesuch const&) = delete;
    void operator=(__nonesuch const&) = delete;
  };
#pragma GCC diagnostic pop
# 4189 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3

}
}
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _Tp>
    __attribute__((__always_inline__))
    inline constexpr _Tp*
    __addressof(_Tp& __r) noexcept
    { return __builtin_addressof(__r); }
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr _Tp&&
    forward(typename std::remove_reference<_Tp>::type& __t) noexcept
    { return static_cast<_Tp&&>(__t); }
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr _Tp&&
    forward(typename std::remove_reference<_Tp>::type&& __t) noexcept
    {
      static_assert(!std::is_lvalue_reference<_Tp>::value,
   "std::forward must not be used to convert an rvalue to an lvalue");
      return static_cast<_Tp&&>(__t);
    }
# 135 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr typename std::remove_reference<_Tp>::type&&
    move(_Tp&& __t) noexcept
    { return static_cast<typename std::remove_reference<_Tp>::type&&>(__t); }


  template<typename _Tp>
    struct __move_if_noexcept_cond
    : public __and_<__not_<is_nothrow_move_constructible<_Tp>>,
                    is_copy_constructible<_Tp>>::type { };
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    constexpr
    __conditional_t<__move_if_noexcept_cond<_Tp>::value, const _Tp&, _Tp&&>
    move_if_noexcept(_Tp& __x) noexcept
    { return std::move(__x); }
# 173 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
    [[__nodiscard__,__gnu__::__always_inline__]]
    inline _Tp*
    addressof(_Tp& __r) noexcept
    { return std::__addressof(__r); }



  template<typename _Tp>
    const _Tp* addressof(const _Tp&&) = delete;


  template <typename _Tp, typename _Up = _Tp>
   
    inline _Tp
    __exchange(_Tp& __obj, _Up&& __new_val)
    {
      _Tp __old_val = std::move(__obj);
      __obj = std::forward<_Up>(__new_val);
      return __old_val;
    }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
   
    inline

    typename enable_if<__and_<__not_<__is_tuple_like<_Tp>>,
         is_move_constructible<_Tp>,
         is_move_assignable<_Tp>>::value>::type



    swap(_Tp& __a, _Tp& __b)
    noexcept(__and_<is_nothrow_move_constructible<_Tp>, is_nothrow_move_assignable<_Tp>>::value)

    {




      _Tp __tmp = std::move(__a);
      __a = std::move(__b);
      __b = std::move(__tmp);
    }




  template<typename _Tp, size_t _Nm>
   
    inline

    typename enable_if<__is_swappable<_Tp>::value>::type



    swap(_Tp (&__a)[_Nm], _Tp (&__b)[_Nm])
    noexcept(__is_nothrow_swappable<_Tp>::value)
    {
      for (size_t __n = 0; __n < _Nm; ++__n)
 swap(__a[__n], __b[__n]);
    }



}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  template<typename _Tp>
    struct tuple_size;





  template<typename _Tp,
    typename _Up = typename remove_cv<_Tp>::type,
    typename = typename enable_if<is_same<_Tp, _Up>::value>::type,
    size_t = tuple_size<_Tp>::value>
    using __enable_if_has_tuple_size = _Tp;

  template<typename _Tp>
    struct tuple_size<const __enable_if_has_tuple_size<_Tp>>
    : public tuple_size<_Tp> { };

  template<typename _Tp>
    struct tuple_size<volatile __enable_if_has_tuple_size<_Tp>>
    : public tuple_size<_Tp> { };

  template<typename _Tp>
    struct tuple_size<const volatile __enable_if_has_tuple_size<_Tp>>
    : public tuple_size<_Tp> { };







  template<size_t __i, typename _Tp>
    struct tuple_element;


  template<size_t __i, typename _Tp>
    using __tuple_element_t = typename tuple_element<__i, _Tp>::type;

  template<size_t __i, typename _Tp>
    struct tuple_element<__i, const _Tp>
    {
      using type = const __tuple_element_t<__i, _Tp>;
    };

  template<size_t __i, typename _Tp>
    struct tuple_element<__i, volatile _Tp>
    {
      using type = volatile __tuple_element_t<__i, _Tp>;
    };

  template<size_t __i, typename _Tp>
    struct tuple_element<__i, const volatile _Tp>
    {
      using type = const volatile __tuple_element_t<__i, _Tp>;
    };





  template<typename _Tp, typename... _Types>
    constexpr size_t
    __find_uniq_type_in_pack()
    {
      constexpr size_t __sz = sizeof...(_Types);
      constexpr bool __found[__sz] = { __is_same(_Tp, _Types) ... };
      size_t __n = __sz;
      for (size_t __i = 0; __i < __sz; ++__i)
 {
   if (__found[__i])
     {
       if (__n < __sz)
  return __sz;
       __n = __i;
     }
 }
      return __n;
    }
# 136 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  template<size_t __i, typename _Tp>
    using tuple_element_t = typename tuple_element<__i, _Tp>::type;




  template<size_t... _Indexes> struct _Index_tuple { };


  template<size_t _Num>
    struct _Build_index_tuple
    {
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
      using __type = _Index_tuple<__integer_pack(_Num)...>;

    };




  template<typename _Tp, _Tp... _Idx>
    struct integer_sequence
    {



      typedef _Tp value_type;
      static constexpr size_t size() noexcept { return sizeof...(_Idx); }
    };


  template<typename _Tp, _Tp _Num>
    using make_integer_sequence



      = integer_sequence<_Tp, __integer_pack(_Num)...>;



  template<size_t... _Idx>
    using index_sequence = integer_sequence<size_t, _Idx...>;


  template<size_t _Num>
    using make_index_sequence = make_integer_sequence<size_t, _Num>;


  template<typename... _Types>
    using index_sequence_for = make_index_sequence<sizeof...(_Types)>;
# 234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  template<size_t _Np, typename... _Types>
    struct _Nth_type
    { using type = __type_pack_element<_Np, _Types...>; };
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  struct _Swallow_assign
  {
    template<class _Tp>
      constexpr const _Swallow_assign&
      operator=(const _Tp&) const noexcept
      { return *this; }
  };
# 309 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  constexpr _Swallow_assign ignore{};
# 319 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3

}
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 2 3





namespace std __attribute__ ((__visibility__ ("default")))
{

# 79 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  struct piecewise_construct_t { explicit piecewise_construct_t() = default; };


  constexpr piecewise_construct_t piecewise_construct =
    piecewise_construct_t();




  template<typename _T1, typename _T2>
    struct pair;

  template<typename...>
    class tuple;





  template<typename _Tp, size_t _Nm>
    struct array;

  template<size_t...>
    struct _Index_tuple;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(pair<_Tp1, _Tp2>& __in) noexcept;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(pair<_Tp1, _Tp2>&& __in) noexcept;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(const pair<_Tp1, _Tp2>& __in) noexcept;

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(const pair<_Tp1, _Tp2>&& __in) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&
    get(tuple<_Elements...>& __t) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&
    get(const tuple<_Elements...>& __t) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&&
    get(tuple<_Elements...>&& __t) noexcept;

  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&&
    get(const tuple<_Elements...>&& __t) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr _Tp&
    get(array<_Tp, _Nm>&) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr _Tp&&
    get(array<_Tp, _Nm>&&) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr const _Tp&
    get(const array<_Tp, _Nm>&) noexcept;

  template<size_t _Int, typename _Tp, size_t _Nm>
    constexpr const _Tp&&
    get(const array<_Tp, _Nm>&&) noexcept;







  template <bool, typename _T1, typename _T2>
    struct _PCC
    {
      template <typename _U1, typename _U2>
      static constexpr bool _ConstructiblePair()
      {
 return __and_<is_constructible<_T1, const _U1&>,
        is_constructible<_T2, const _U2&>>::value;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyConvertiblePair()
      {
 return __and_<is_convertible<const _U1&, _T1>,
        is_convertible<const _U2&, _T2>>::value;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _MoveConstructiblePair()
      {
 return __and_<is_constructible<_T1, _U1&&>,
        is_constructible<_T2, _U2&&>>::value;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyMoveConvertiblePair()
      {
 return __and_<is_convertible<_U1&&, _T1>,
        is_convertible<_U2&&, _T2>>::value;
      }
    };

  template <typename _T1, typename _T2>
    struct _PCC<false, _T1, _T2>
    {
      template <typename _U1, typename _U2>
      static constexpr bool _ConstructiblePair()
      {
 return false;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyConvertiblePair()
      {
 return false;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _MoveConstructiblePair()
      {
 return false;
      }

      template <typename _U1, typename _U2>
      static constexpr bool _ImplicitlyMoveConvertiblePair()
      {
 return false;
      }
    };
# 260 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _U1, typename _U2> class __pair_base
  {

    template<typename _T1, typename _T2> friend struct pair;
    __pair_base() = default;
    ~__pair_base() = default;
    __pair_base(const __pair_base&) = default;
    __pair_base& operator=(const __pair_base&) = delete;

  };
# 283 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    struct pair
    : public __pair_base<_T1, _T2>
    {
      typedef _T1 first_type;
      typedef _T2 second_type;

      _T1 first;
      _T2 second;


      constexpr pair(const pair&) = default;
      constexpr pair(pair&&) = default;

      template<typename... _Args1, typename... _Args2>

 pair(piecewise_construct_t, tuple<_Args1...>, tuple<_Args2...>);


      void
      swap(pair& __p)
      noexcept(__and_<__is_nothrow_swappable<_T1>,
        __is_nothrow_swappable<_T2>>::value)
      {
 using std::swap;
 swap(first, __p.first);
 swap(second, __p.second);
      }
# 331 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    private:
      template<typename... _Args1, size_t... _Indexes1,
        typename... _Args2, size_t... _Indexes2>

 pair(tuple<_Args1...>&, tuple<_Args2...>&,
      _Index_tuple<_Indexes1...>, _Index_tuple<_Indexes2...>);
    public:
# 721 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
      template <typename _U1 = _T1,
                typename _U2 = _T2,
                typename enable_if<__and_<
                                     __is_implicitly_default_constructible<_U1>,
                                     __is_implicitly_default_constructible<_U2>>
                                   ::value, bool>::type = true>
      constexpr pair()
      : first(), second() { }

      template <typename _U1 = _T1,
                typename _U2 = _T2,
                typename enable_if<__and_<
                       is_default_constructible<_U1>,
                       is_default_constructible<_U2>,
                       __not_<
                         __and_<__is_implicitly_default_constructible<_U1>,
                                __is_implicitly_default_constructible<_U2>>>>
                                   ::value, bool>::type = false>
      explicit constexpr pair()
      : first(), second() { }



      using _PCCP = _PCC<true, _T1, _T2>;



      template<typename _U1 = _T1, typename _U2=_T2, typename
        enable_if<_PCCP::template
      _ConstructiblePair<_U1, _U2>()
                  && _PCCP::template
      _ImplicitlyConvertiblePair<_U1, _U2>(),
                         bool>::type=true>
      constexpr pair(const _T1& __a, const _T2& __b)
      : first(__a), second(__b) { }


       template<typename _U1 = _T1, typename _U2=_T2, typename
  enable_if<_PCCP::template
       _ConstructiblePair<_U1, _U2>()
                   && !_PCCP::template
       _ImplicitlyConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
      explicit constexpr pair(const _T1& __a, const _T2& __b)
      : first(__a), second(__b) { }



      template <typename _U1, typename _U2>
        using _PCCFP = _PCC<!is_same<_T1, _U1>::value
       || !is_same<_T2, _U2>::value,
       _T1, _T2>;


      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _ConstructiblePair<_U1, _U2>()
                  && _PCCFP<_U1, _U2>::template
      _ImplicitlyConvertiblePair<_U1, _U2>(),
     bool>::type=true>
 constexpr pair(const pair<_U1, _U2>& __p)
 : first(__p.first), second(__p.second)
 { ; }

      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _ConstructiblePair<_U1, _U2>()
    && !_PCCFP<_U1, _U2>::template
      _ImplicitlyConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
 explicit constexpr pair(const pair<_U1, _U2>& __p)
 : first(__p.first), second(__p.second)
 { ; }
# 805 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    private:



      struct __zero_as_null_pointer_constant
      {
 __zero_as_null_pointer_constant(int __zero_as_null_pointer_constant::*)
 { }
 template<typename _Tp,
   typename = __enable_if_t<is_null_pointer<_Tp>::value>>
 __zero_as_null_pointer_constant(_Tp) = delete;
      };

    public:




      template<typename _U1,
        __enable_if_t<__and_<__not_<is_reference<_U1>>,
        is_pointer<_T2>,
        is_constructible<_T1, _U1>,
        __not_<is_constructible<_T1, const _U1&>>,
        is_convertible<_U1, _T1>>::value,
        bool> = true>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 constexpr
 pair(_U1&& __x, __zero_as_null_pointer_constant, ...)
 : first(std::forward<_U1>(__x)), second(nullptr)
 { ; }

      template<typename _U1,
        __enable_if_t<__and_<__not_<is_reference<_U1>>,
        is_pointer<_T2>,
        is_constructible<_T1, _U1>,
        __not_<is_constructible<_T1, const _U1&>>,
        __not_<is_convertible<_U1, _T1>>>::value,
        bool> = false>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 explicit constexpr
 pair(_U1&& __x, __zero_as_null_pointer_constant, ...)
 : first(std::forward<_U1>(__x)), second(nullptr)
 { ; }

      template<typename _U2,
        __enable_if_t<__and_<is_pointer<_T1>,
        __not_<is_reference<_U2>>,
        is_constructible<_T2, _U2>,
        __not_<is_constructible<_T2, const _U2&>>,
        is_convertible<_U2, _T2>>::value,
        bool> = true>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 constexpr
 pair(__zero_as_null_pointer_constant, _U2&& __y, ...)
 : first(nullptr), second(std::forward<_U2>(__y))
 { ; }

      template<typename _U2,
        __enable_if_t<__and_<is_pointer<_T1>,
        __not_<is_reference<_U2>>,
        is_constructible<_T2, _U2>,
        __not_<is_constructible<_T2, const _U2&>>,
        __not_<is_convertible<_U2, _T2>>>::value,
        bool> = false>
 __attribute__ ((__deprecated__ ("use 'nullptr' instead of '0' to " "initialize std::pair of move-only " "type and pointer")))
 explicit constexpr
 pair(__zero_as_null_pointer_constant, _U2&& __y, ...)
 : first(nullptr), second(std::forward<_U2>(__y))
 { ; }



      template<typename _U1, typename _U2, typename
        enable_if<_PCCP::template
      _MoveConstructiblePair<_U1, _U2>()
     && _PCCP::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=true>
 constexpr pair(_U1&& __x, _U2&& __y)
 : first(std::forward<_U1>(__x)), second(std::forward<_U2>(__y))
 { ; }

      template<typename _U1, typename _U2, typename
        enable_if<_PCCP::template
      _MoveConstructiblePair<_U1, _U2>()
     && !_PCCP::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
 explicit constexpr pair(_U1&& __x, _U2&& __y)
 : first(std::forward<_U1>(__x)), second(std::forward<_U2>(__y))
 { ; }


      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _MoveConstructiblePair<_U1, _U2>()
     && _PCCFP<_U1, _U2>::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=true>
 constexpr pair(pair<_U1, _U2>&& __p)
 : first(std::forward<_U1>(__p.first)),
   second(std::forward<_U2>(__p.second))
 { ; }

      template<typename _U1, typename _U2, typename
        enable_if<_PCCFP<_U1, _U2>::template
      _MoveConstructiblePair<_U1, _U2>()
     && !_PCCFP<_U1, _U2>::template
      _ImplicitlyMoveConvertiblePair<_U1, _U2>(),
                         bool>::type=false>
 explicit constexpr pair(pair<_U1, _U2>&& __p)
 : first(std::forward<_U1>(__p.first)),
   second(std::forward<_U2>(__p.second))
 { ; }



      pair&
      operator=(__conditional_t<__and_<is_copy_assignable<_T1>,
           is_copy_assignable<_T2>>::value,
    const pair&, const __nonesuch&> __p)
      {
 first = __p.first;
 second = __p.second;
 return *this;
      }

      pair&
      operator=(__conditional_t<__and_<is_move_assignable<_T1>,
           is_move_assignable<_T2>>::value,
    pair&&, __nonesuch&&> __p)
      noexcept(__and_<is_nothrow_move_assignable<_T1>,
        is_nothrow_move_assignable<_T2>>::value)
      {
 first = std::forward<first_type>(__p.first);
 second = std::forward<second_type>(__p.second);
 return *this;
      }

      template<typename _U1, typename _U2>
 typename enable_if<__and_<is_assignable<_T1&, const _U1&>,
      is_assignable<_T2&, const _U2&>>::value,
      pair&>::type
 operator=(const pair<_U1, _U2>& __p)
 {
   first = __p.first;
   second = __p.second;
   return *this;
 }

      template<typename _U1, typename _U2>
 typename enable_if<__and_<is_assignable<_T1&, _U1&&>,
      is_assignable<_T2&, _U2&&>>::value,
      pair&>::type
 operator=(pair<_U1, _U2>&& __p)
 {
   first = std::forward<_U1>(__p.first);
   second = std::forward<_U2>(__p.second);
   return *this;
 }
# 997 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
    };
# 1039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator==(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first == __y.first && __x.second == __y.second; }
# 1052 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator<(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first < __y.first
      || (!(__y.first < __x.first) && __x.second < __y.second); }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator!=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x == __y); }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator>(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __y < __x; }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator<=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__y < __x); }


  template<typename _T1, typename _T2>
    [[__nodiscard__]]
    inline constexpr bool
    operator>=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x < __y); }
# 1094 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    inline





    void

    swap(pair<_T1, _T2>& __x, pair<_T1, _T2>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }
# 1143 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    constexpr pair<typename __decay_and_strip<_T1>::__type,
                   typename __decay_and_strip<_T2>::__type>
    make_pair(_T1&& __x, _T2&& __y)
    {
      typedef typename __decay_and_strip<_T1>::__type __ds_type1;
      typedef typename __decay_and_strip<_T2>::__type __ds_type2;
      typedef pair<__ds_type1, __ds_type2> __pair_type;
      return __pair_type(std::forward<_T1>(__x), std::forward<_T2>(__y));
    }
# 1166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    struct __is_tuple_like_impl<pair<_T1, _T2>> : true_type
    { };



  template<class _Tp1, class _Tp2>
    struct tuple_size<pair<_Tp1, _Tp2>>
    : public integral_constant<size_t, 2> { };


  template<class _Tp1, class _Tp2>
    struct tuple_element<0, pair<_Tp1, _Tp2>>
    { typedef _Tp1 type; };


  template<class _Tp1, class _Tp2>
    struct tuple_element<1, pair<_Tp1, _Tp2>>
    { typedef _Tp2 type; };
# 1195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _Tp>
    inline constexpr bool __is_pair = false;

  template<typename _Tp, typename _Up>
    inline constexpr bool __is_pair<pair<_Tp, _Up>> = true;
#pragma GCC diagnostic pop



  template<size_t _Int>
    struct __pair_get;

  template<>
    struct __pair_get<0>
    {
      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp1&
 __get(pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.first; }

      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp1&&
 __move_get(pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<_Tp1>(__pair.first); }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp1&
 __const_get(const pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.first; }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp1&&
 __const_move_get(const pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<const _Tp1>(__pair.first); }
    };

  template<>
    struct __pair_get<1>
    {
      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp2&
 __get(pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.second; }

      template<typename _Tp1, typename _Tp2>
 static constexpr _Tp2&&
 __move_get(pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<_Tp2>(__pair.second); }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp2&
 __const_get(const pair<_Tp1, _Tp2>& __pair) noexcept
 { return __pair.second; }

      template<typename _Tp1, typename _Tp2>
 static constexpr const _Tp2&&
 __const_move_get(const pair<_Tp1, _Tp2>&& __pair) noexcept
 { return std::forward<const _Tp2>(__pair.second); }
    };






  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(pair<_Tp1, _Tp2>& __in) noexcept
    { return __pair_get<_Int>::__get(__in); }

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(pair<_Tp1, _Tp2>&& __in) noexcept
    { return __pair_get<_Int>::__move_get(std::move(__in)); }

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&
    get(const pair<_Tp1, _Tp2>& __in) noexcept
    { return __pair_get<_Int>::__const_get(__in); }

  template<size_t _Int, class _Tp1, class _Tp2>
    constexpr const typename tuple_element<_Int, pair<_Tp1, _Tp2>>::type&&
    get(const pair<_Tp1, _Tp2>&& __in) noexcept
    { return __pair_get<_Int>::__const_move_get(std::move(__in)); }



  template <typename _Tp, typename _Up>
    constexpr _Tp&
    get(pair<_Tp, _Up>& __p) noexcept
    { return __p.first; }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&
    get(const pair<_Tp, _Up>& __p) noexcept
    { return __p.first; }

  template <typename _Tp, typename _Up>
    constexpr _Tp&&
    get(pair<_Tp, _Up>&& __p) noexcept
    { return std::move(__p.first); }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&&
    get(const pair<_Tp, _Up>&& __p) noexcept
    { return std::move(__p.first); }

  template <typename _Tp, typename _Up>
    constexpr _Tp&
    get(pair<_Up, _Tp>& __p) noexcept
    { return __p.second; }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&
    get(const pair<_Up, _Tp>& __p) noexcept
    { return __p.second; }

  template <typename _Tp, typename _Up>
    constexpr _Tp&&
    get(pair<_Up, _Tp>&& __p) noexcept
    { return std::move(__p.second); }

  template <typename _Tp, typename _Up>
    constexpr const _Tp&&
    get(const pair<_Up, _Tp>&& __p) noexcept
    { return std::move(__p.second); }
# 1347 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3

}
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 1 3
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 95 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  struct input_iterator_tag { };


  struct output_iterator_tag { };


  struct forward_iterator_tag : public input_iterator_tag { };



  struct bidirectional_iterator_tag : public forward_iterator_tag { };



  struct random_access_iterator_tag : public bidirectional_iterator_tag { };
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Category, typename _Tp, typename _Distance = ptrdiff_t,
           typename _Pointer = _Tp*, typename _Reference = _Tp&>
    struct iterator
    {

      typedef _Category iterator_category;

      typedef _Tp value_type;

      typedef _Distance difference_type;

      typedef _Pointer pointer;

      typedef _Reference reference;
    };
# 151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Iterator>
    struct iterator_traits;




  template<typename _Iterator, typename = __void_t<>>
    struct __iterator_traits { };



  template<typename _Iterator>
    struct __iterator_traits<_Iterator,
        __void_t<typename _Iterator::iterator_category,
          typename _Iterator::value_type,
          typename _Iterator::difference_type,
          typename _Iterator::pointer,
          typename _Iterator::reference>>
    {
      typedef typename _Iterator::iterator_category iterator_category;
      typedef typename _Iterator::value_type value_type;
      typedef typename _Iterator::difference_type difference_type;
      typedef typename _Iterator::pointer pointer;
      typedef typename _Iterator::reference reference;
    };


  template<typename _Iterator>
    struct iterator_traits
    : public __iterator_traits<_Iterator> { };
# 211 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Tp>
    struct iterator_traits<_Tp*>
    {
      typedef random_access_iterator_tag iterator_category;
      typedef _Tp value_type;
      typedef ptrdiff_t difference_type;
      typedef _Tp* pointer;
      typedef _Tp& reference;
    };


  template<typename _Tp>
    struct iterator_traits<const _Tp*>
    {
      typedef random_access_iterator_tag iterator_category;
      typedef _Tp value_type;
      typedef ptrdiff_t difference_type;
      typedef const _Tp* pointer;
      typedef const _Tp& reference;
    };






  template<typename _Iter>
    __attribute__((__always_inline__))
    inline constexpr
    typename iterator_traits<_Iter>::iterator_category
    __iterator_category(const _Iter&)
    { return typename iterator_traits<_Iter>::iterator_category(); }




  template<typename _Iter>
    using __iter_category_t
      = typename iterator_traits<_Iter>::iterator_category;

  template<typename _InIter>
    using _RequireInputIter =
      __enable_if_t<is_convertible<__iter_category_t<_InIter>,
       input_iterator_tag>::value>;







  template<typename _It,
    typename _Cat = __iter_category_t<_It>>
    struct __is_random_access_iter
      : is_base_of<random_access_iterator_tag, _Cat>
    {
      typedef is_base_of<random_access_iterator_tag, _Cat> _Base;
      enum { __value = _Base::value };
    };








}
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 1 3
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wvariadic-macros"
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/concept_check.h" 3
#pragma GCC diagnostic pop
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/assertions.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{




  template <typename> struct _List_iterator;
  template <typename> struct _List_const_iterator;


  template<typename _InputIterator>
    inline constexpr
    typename iterator_traits<_InputIterator>::difference_type
    __distance(_InputIterator __first, _InputIterator __last,
               input_iterator_tag)
    {

     

      typename iterator_traits<_InputIterator>::difference_type __n = 0;
      while (__first != __last)
 {
   ++__first;
   ++__n;
 }
      return __n;
    }

  template<typename _RandomAccessIterator>
    __attribute__((__always_inline__))
    inline constexpr
    typename iterator_traits<_RandomAccessIterator>::difference_type
    __distance(_RandomAccessIterator __first, _RandomAccessIterator __last,
               random_access_iterator_tag)
    {

     

      return __last - __first;
    }



  template<typename _Tp>
    ptrdiff_t
    __distance(std::_List_iterator<_Tp>,
        std::_List_iterator<_Tp>,
        input_iterator_tag);

  template<typename _Tp>
    ptrdiff_t
    __distance(std::_List_const_iterator<_Tp>,
        std::_List_const_iterator<_Tp>,
        input_iterator_tag);




  template<typename _OutputIterator>
    void
    __distance(_OutputIterator, _OutputIterator, output_iterator_tag) = delete;
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
    inline
    typename iterator_traits<_InputIterator>::difference_type
    distance(_InputIterator __first, _InputIterator __last)
    {

      return std::__distance(__first, __last,
        std::__iterator_category(__first));
    }

  template<typename _InputIterator, typename _Distance>
    inline constexpr void
    __advance(_InputIterator& __i, _Distance __n, input_iterator_tag)
    {

     
      do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h", 163, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
      while (__n--)
 ++__i;
    }

  template<typename _BidirectionalIterator, typename _Distance>
    inline constexpr void
    __advance(_BidirectionalIterator& __i, _Distance __n,
       bidirectional_iterator_tag)
    {

     

      if (__n > 0)
        while (__n--)
   ++__i;
      else
        while (__n++)
   --__i;
    }

  template<typename _RandomAccessIterator, typename _Distance>
    inline constexpr void
    __advance(_RandomAccessIterator& __i, _Distance __n,
              random_access_iterator_tag)
    {

     

      if (__builtin_constant_p(__n) && __n == 1)
 ++__i;
      else if (__builtin_constant_p(__n) && __n == -1)
 --__i;
      else
 __i += __n;
    }



  template<typename _OutputIterator, typename _Distance>
    void
    __advance(_OutputIterator&, _Distance, output_iterator_tag) = delete;
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator, typename _Distance>
    __attribute__((__always_inline__))
    inline void
    advance(_InputIterator& __i, _Distance __n)
    {

      typename iterator_traits<_InputIterator>::difference_type __d = __n;
      std::__advance(__i, __d, std::__iterator_category(__i));
    }



  template<typename _InputIterator>
    [[__nodiscard__]] [[__gnu__::__always_inline__]]
    inline _InputIterator
    next(_InputIterator __x, typename
  iterator_traits<_InputIterator>::difference_type __n = 1)
    {

     
      std::advance(__x, __n);
      return __x;
    }

  template<typename _BidirectionalIterator>
    [[__nodiscard__]] [[__gnu__::__always_inline__]]
    inline _BidirectionalIterator
    prev(_BidirectionalIterator __x, typename
  iterator_traits<_BidirectionalIterator>::difference_type __n = 1)
    {

     

      std::advance(__x, -__n);
      return __x;
    }




}
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  class __undefined;



  template<typename _Tp>
    struct __get_first_arg
    { using type = __undefined; };

  template<template<typename, typename...> class _SomeTemplate, typename _Tp,
           typename... _Types>
    struct __get_first_arg<_SomeTemplate<_Tp, _Types...>>
    { using type = _Tp; };



  template<typename _Tp, typename _Up>
    struct __replace_first_arg
    { };

  template<template<typename, typename...> class _SomeTemplate, typename _Up,
           typename _Tp, typename... _Types>
    struct __replace_first_arg<_SomeTemplate<_Tp, _Types...>, _Up>
    { using type = _SomeTemplate<_Up, _Types...>; };


  template<typename _Ptr, typename = void>
    struct __ptr_traits_elem : __get_first_arg<_Ptr>
    { };







  template<typename _Ptr>
    struct __ptr_traits_elem<_Ptr, __void_t<typename _Ptr::element_type>>
    { using type = typename _Ptr::element_type; };


  template<typename _Ptr>
    using __ptr_traits_elem_t = typename __ptr_traits_elem<_Ptr>::type;




  template<typename _Ptr, typename _Elt, bool = is_void<_Elt>::value>
    struct __ptr_traits_ptr_to
    {
      using pointer = _Ptr;
      using element_type = _Elt;







      static pointer
      pointer_to(element_type& __r)





      { return pointer::pointer_to(__r); }
    };


  template<typename _Ptr, typename _Elt>
    struct __ptr_traits_ptr_to<_Ptr, _Elt, true>
    { };


  template<typename _Tp>
    struct __ptr_traits_ptr_to<_Tp*, _Tp, false>
    {
      using pointer = _Tp*;
      using element_type = _Tp;






      static pointer
      pointer_to(element_type& __r) noexcept
      { return std::addressof(__r); }
    };

  template<typename _Ptr, typename _Elt>
    struct __ptr_traits_impl : __ptr_traits_ptr_to<_Ptr, _Elt>
    {
    private:
      template<typename _Tp>
 using __diff_t = typename _Tp::difference_type;

      template<typename _Tp, typename _Up>
 using __rebind = __type_identity<typename _Tp::template rebind<_Up>>;

    public:

      using pointer = _Ptr;


      using element_type = _Elt;


      using difference_type = __detected_or_t<ptrdiff_t, __diff_t, _Ptr>;


      template<typename _Up>
 using rebind = typename __detected_or_t<__replace_first_arg<_Ptr, _Up>,
      __rebind, _Ptr, _Up>::type;
    };



  template<typename _Ptr>
    struct __ptr_traits_impl<_Ptr, __undefined>
    { };







  template<typename _Ptr>
    struct pointer_traits : __ptr_traits_impl<_Ptr, __ptr_traits_elem_t<_Ptr>>
    { };







  template<typename _Tp>
    struct pointer_traits<_Tp*> : __ptr_traits_ptr_to<_Tp*, _Tp>
    {

      typedef _Tp* pointer;

      typedef _Tp element_type;

      typedef ptrdiff_t difference_type;

      template<typename _Up> using rebind = _Up*;
    };


  template<typename _Ptr, typename _Tp>
    using __ptr_rebind = typename pointer_traits<_Ptr>::template rebind<_Tp>;


  template<typename _Tp>
    [[__gnu__::__always_inline__]]
    constexpr _Tp*
    __to_address(_Tp* __ptr) noexcept
    {
      static_assert(!std::is_function<_Tp>::value, "std::to_address argument "
      "must not be a function pointer");
      return __ptr;
    }





  template<typename _Ptr>
    constexpr typename std::pointer_traits<_Ptr>::element_type*
    __to_address(const _Ptr& __ptr)
    { return std::__to_address(__ptr.operator->()); }
# 268 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 3

}
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
 
# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    class reverse_iterator
    : public iterator<typename iterator_traits<_Iterator>::iterator_category,
        typename iterator_traits<_Iterator>::value_type,
        typename iterator_traits<_Iterator>::difference_type,
        typename iterator_traits<_Iterator>::pointer,
                      typename iterator_traits<_Iterator>::reference>
    {
      template<typename _Iter>
 friend class reverse_iterator;
# 149 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    protected:
      _Iterator current;

      typedef iterator_traits<_Iterator> __traits_type;

    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::pointer pointer;

      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
# 180 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      reverse_iterator()
      noexcept(noexcept(_Iterator()))
      : current()
      { }




      explicit
      reverse_iterator(iterator_type __x)
      noexcept(noexcept(_Iterator(__x)))
      : current(__x)
      { }




     
      reverse_iterator(const reverse_iterator& __x)
      noexcept(noexcept(_Iterator(__x.current)))
      : current(__x.current)
      { }


      reverse_iterator& operator=(const reverse_iterator&) = default;






      template<typename _Iter>




        reverse_iterator(const reverse_iterator<_Iter>& __x)
 noexcept(noexcept(_Iterator(__x.current)))
 : current(__x.current)
 { }


      template<typename _Iter>





 reverse_iterator&
 operator=(const reverse_iterator<_Iter>& __x)
 noexcept(noexcept(current = __x.current))
 {
   current = __x.current;
   return *this;
 }





      [[__nodiscard__]]
      iterator_type
      base() const
      noexcept(noexcept(_Iterator(current)))
      { return current; }
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      [[__nodiscard__]]
      reference
      operator*() const
      {
 _Iterator __tmp = current;
 return *--__tmp;
      }






      [[__nodiscard__]]
      pointer
      operator->() const




      {


 _Iterator __tmp = current;
 --__tmp;
 return _S_to_pointer(__tmp);
      }






      reverse_iterator&
      operator++()
      {
 --current;
 return *this;
      }






      reverse_iterator
      operator++(int)
      {
 reverse_iterator __tmp = *this;
 --current;
 return __tmp;
      }






      reverse_iterator&
      operator--()
      {
 ++current;
 return *this;
      }






      reverse_iterator
      operator--(int)
      {
 reverse_iterator __tmp = *this;
 ++current;
 return __tmp;
      }






      [[__nodiscard__]]
      reverse_iterator
      operator+(difference_type __n) const
      { return reverse_iterator(current - __n); }







      reverse_iterator&
      operator+=(difference_type __n)
      {
 current -= __n;
 return *this;
      }






      [[__nodiscard__]]
      reverse_iterator
      operator-(difference_type __n) const
      { return reverse_iterator(current + __n); }







      reverse_iterator&
      operator-=(difference_type __n)
      {
 current += __n;
 return *this;
      }






      [[__nodiscard__]]
      reference
      operator[](difference_type __n) const
      { return *(*this + __n); }
# 417 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    private:
      template<typename _Tp>
 static _Tp*
 _S_to_pointer(_Tp* __p)
        { return __p; }

      template<typename _Tp>
 static pointer
 _S_to_pointer(_Tp __t)
        { return __t.operator->(); }
    };
# 440 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator==(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return __x.base() == __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y.base() < __x.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator!=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x == __y); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y < __x; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__y < __x); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x < __y); }




  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator==(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() == __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator<(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() > __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator!=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() != __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    inline bool
    operator<=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() >= __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() <= __y.base(); }
# 617 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline auto
    operator-(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    -> decltype(__y.base() - __x.base())
    { return __y.base() - __x.base(); }


  template<typename _Iterator>
    [[__nodiscard__]]
    inline reverse_iterator<_Iterator>
    operator+(typename reverse_iterator<_Iterator>::difference_type __n,
       const reverse_iterator<_Iterator>& __x)
    { return reverse_iterator<_Iterator>(__x.base() - __n); }



  template<typename _Iterator>
    inline reverse_iterator<_Iterator>
    __make_reverse_iterator(_Iterator __i)
    { return reverse_iterator<_Iterator>(__i); }





  template<typename _Iterator>
    [[__nodiscard__]]
    inline reverse_iterator<_Iterator>
    make_reverse_iterator(_Iterator __i)
    { return reverse_iterator<_Iterator>(__i); }
# 659 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    struct __is_move_iterator<reverse_iterator<_Iterator> >
      : __is_move_iterator<_Iterator>
    { };
# 676 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class back_insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _Container* container;

    public:

      typedef _Container container_type;





      explicit
      back_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 714 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      back_insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 container->push_back(__value);
 return *this;
      }

     
      back_insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 container->push_back(std::move(__value));
 return *this;
      }



      [[__nodiscard__]]
      back_insert_iterator&
      operator*()
      { return *this; }


     
      back_insert_iterator&
      operator++()
      { return *this; }


     
      back_insert_iterator
      operator++(int)
      { return *this; }
    };
# 761 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]]
    inline back_insert_iterator<_Container>
    back_inserter(_Container& __x)
    { return back_insert_iterator<_Container>(__x); }
# 777 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class front_insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    protected:
      _Container* container;

    public:

      typedef _Container container_type;





      explicit
      front_insert_iterator(_Container& __x)
      : container(std::__addressof(__x)) { }
# 815 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      front_insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 container->push_front(__value);
 return *this;
      }

     
      front_insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 container->push_front(std::move(__value));
 return *this;
      }



      [[__nodiscard__]]
      front_insert_iterator&
      operator*()
      { return *this; }


     
      front_insert_iterator&
      operator++()
      { return *this; }


     
      front_insert_iterator
      operator++(int)
      { return *this; }
    };
# 862 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]]
    inline front_insert_iterator<_Container>
    front_inserter(_Container& __x)
    { return front_insert_iterator<_Container>(__x); }
# 882 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    class insert_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {



      typedef typename _Container::iterator _Iter;

    protected:
      _Container* container;
      _Iter iter;

    public:

      typedef _Container container_type;
# 907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      insert_iterator(_Container& __x, _Iter __i)
      : container(std::__addressof(__x)), iter(__i) {}
# 943 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      insert_iterator&
      operator=(const typename _Container::value_type& __value)
      {
 iter = container->insert(iter, __value);
 ++iter;
 return *this;
      }

     
      insert_iterator&
      operator=(typename _Container::value_type&& __value)
      {
 iter = container->insert(iter, std::move(__value));
 ++iter;
 return *this;
      }



      [[__nodiscard__]]
      insert_iterator&
      operator*()
      { return *this; }


     
      insert_iterator&
      operator++()
      { return *this; }


     
      insert_iterator&
      operator++(int)
      { return *this; }
    };

#pragma GCC diagnostic pop
# 1002 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Container>
    [[__nodiscard__]]
    inline insert_iterator<_Container>
    inserter(_Container& __x, typename _Container::iterator __i)
    { return insert_iterator<_Container>(__x, __i); }





}

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 1025 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator, typename _Container>
    class __normal_iterator
    {
    protected:
      _Iterator _M_current;

      typedef std::iterator_traits<_Iterator> __traits_type;


      template<typename _Iter>
 using __convertible_from
   = std::__enable_if_t<std::is_convertible<_Iter, _Iterator>::value>;


    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::iterator_category iterator_category;
      typedef typename __traits_type::value_type value_type;
      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
      typedef typename __traits_type::pointer pointer;





      __attribute__((__always_inline__))
      constexpr
      __normal_iterator() noexcept
      : _M_current() { }

      __attribute__((__always_inline__))
      explicit constexpr
      __normal_iterator(const _Iterator& __i) noexcept
      : _M_current(__i) { }






      template<typename _Iter, typename = __convertible_from<_Iter>>

 [[__gnu__::__always_inline__]]
 constexpr
 __normal_iterator(const __normal_iterator<_Iter, _Container>& __i)
 noexcept
# 1082 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
        : _M_current(__i.base()) { }



      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      reference
      operator*() const noexcept
      { return *_M_current; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      pointer
      operator->() const noexcept
      { return _M_current; }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator++() noexcept
      {
 ++_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator++(int) noexcept
      { return __normal_iterator(_M_current++); }



      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator--() noexcept
      {
 --_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator--(int) noexcept
      { return __normal_iterator(_M_current--); }



      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      reference
      operator[](difference_type __n) const noexcept
      { return _M_current[__n]; }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator+=(difference_type __n) noexcept
      { _M_current += __n; return *this; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator+(difference_type __n) const noexcept
      { return __normal_iterator(_M_current + __n); }

      __attribute__((__always_inline__))
      constexpr
      __normal_iterator&
      operator-=(difference_type __n) noexcept
      { _M_current -= __n; return *this; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator-(difference_type __n) const noexcept
      { return __normal_iterator(_M_current - __n); }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      const _Iterator&
      base() const noexcept
      { return _M_current; }
    };
# 1217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator==(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() == __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator==(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() == __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator!=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() != __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator!=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() != __rhs.base(); }


  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator<(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() < __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__))
    inline bool
    operator<(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() < __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() > __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() > __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator<=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() <= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator<=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() <= __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
    noexcept
    { return __lhs.base() >= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline bool
    operator>=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() >= __rhs.base(); }






  template<typename _IteratorL, typename _IteratorR, typename _Container>


    [[__nodiscard__, __gnu__::__always_inline__]]
    constexpr auto
    operator-(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs) noexcept
    -> decltype(__lhs.base() - __rhs.base())





    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline typename __normal_iterator<_Iterator, _Container>::difference_type
    operator-(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
    noexcept
    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__)) constexpr
    inline __normal_iterator<_Iterator, _Container>
    operator+(typename __normal_iterator<_Iterator, _Container>::difference_type
       __n, const __normal_iterator<_Iterator, _Container>& __i)
    noexcept
    { return __normal_iterator<_Iterator, _Container>(__i.base() + __n); }


}

namespace std __attribute__ ((__visibility__ ("default")))
{

# 1434 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    class move_iterator



    {
      _Iterator _M_current;

      using __traits_type = iterator_traits<_Iterator>;

      using __base_ref = typename __traits_type::reference;


      template<typename _Iter2>
 friend class move_iterator;
# 1473 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    public:
      using iterator_type = _Iterator;
# 1485 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      typedef typename __traits_type::iterator_category iterator_category;
      typedef typename __traits_type::value_type value_type;
      typedef typename __traits_type::difference_type difference_type;

      typedef _Iterator pointer;


      using reference
 = __conditional_t<is_reference<__base_ref>::value,
     typename remove_reference<__base_ref>::type&&,
     __base_ref>;


     
      move_iterator()
      : _M_current() { }

      explicit
      move_iterator(iterator_type __i)
      : _M_current(std::move(__i)) { }

      template<typename _Iter>




 move_iterator(const move_iterator<_Iter>& __i)
 : _M_current(__i._M_current) { }

      template<typename _Iter>





 move_iterator& operator=(const move_iterator<_Iter>& __i)
 {
   _M_current = __i._M_current;
   return *this;
 }


      [[__nodiscard__]]
      iterator_type
      base() const
      { return _M_current; }
# 1543 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      [[__nodiscard__]]
      reference
      operator*() const



      { return static_cast<reference>(*_M_current); }


      [[__nodiscard__]]
      pointer
      operator->() const
      { return _M_current; }

      move_iterator&
      operator++()
      {
 ++_M_current;
 return *this;
      }

      move_iterator
      operator++(int)
      {
 move_iterator __tmp = *this;
 ++_M_current;
 return __tmp;
      }







      move_iterator&
      operator--()
      {
 --_M_current;
 return *this;
      }

      move_iterator
      operator--(int)
      {
 move_iterator __tmp = *this;
 --_M_current;
 return __tmp;
      }

      [[__nodiscard__]]
      move_iterator
      operator+(difference_type __n) const
      { return move_iterator(_M_current + __n); }

      move_iterator&
      operator+=(difference_type __n)
      {
 _M_current += __n;
 return *this;
      }

      [[__nodiscard__]]
      move_iterator
      operator-(difference_type __n) const
      { return move_iterator(_M_current - __n); }

      move_iterator&
      operator-=(difference_type __n)
      {
 _M_current -= __n;
 return *this;
      }

      [[__nodiscard__]]
      reference
      operator[](difference_type __n) const



      { return std::move(_M_current[__n]); }
# 1657 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    };

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator==(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)



    { return __x.base() == __y.base(); }
# 1678 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator!=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)
    { return !(__x == __y); }


  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator<(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)



    { return __x.base() < __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator<=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)



    { return !(__y < __x); }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)



    { return __y < __x; }

  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline bool
    operator>=(const move_iterator<_IteratorL>& __x,
        const move_iterator<_IteratorR>& __y)



    { return !(__x < __y); }




  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator==(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)

    { return __x.base() == __y.base(); }
# 1745 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator!=(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)
    { return !(__x == __y); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<(const move_iterator<_Iterator>& __x,
       const move_iterator<_Iterator>& __y)
    { return __x.base() < __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator<=(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)
    { return !(__y < __x); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>(const move_iterator<_Iterator>& __x,
       const move_iterator<_Iterator>& __y)
    { return __y < __x; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline bool
    operator>=(const move_iterator<_Iterator>& __x,
        const move_iterator<_Iterator>& __y)
    { return !(__x < __y); }



  template<typename _IteratorL, typename _IteratorR>
    [[__nodiscard__]]
    inline auto
    operator-(const move_iterator<_IteratorL>& __x,
       const move_iterator<_IteratorR>& __y)
    -> decltype(__x.base() - __y.base())
    { return __x.base() - __y.base(); }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline move_iterator<_Iterator>
    operator+(typename move_iterator<_Iterator>::difference_type __n,
       const move_iterator<_Iterator>& __x)



    { return __x + __n; }

  template<typename _Iterator>
    [[__nodiscard__]]
    inline move_iterator<_Iterator>
    make_move_iterator(_Iterator __i)
    { return move_iterator<_Iterator>(std::move(__i)); }

  template<typename _Iterator, typename _ReturnType
    = __conditional_t<__move_if_noexcept_cond
      <typename iterator_traits<_Iterator>::value_type>::value,
  _Iterator, move_iterator<_Iterator>>>
    [[__nodiscard__]]
    constexpr _ReturnType
    __make_move_if_noexcept_iterator(_Iterator __i)
    { return _ReturnType(__i); }



  template<typename _Tp, typename _ReturnType
    = __conditional_t<__move_if_noexcept_cond<_Tp>::value,
        const _Tp*, move_iterator<_Tp*>>>
    [[__nodiscard__]]
    constexpr _ReturnType
    __make_move_if_noexcept_iterator(_Tp* __i)
    { return _ReturnType(__i); }

  template<typename _Iterator>
    struct __is_move_iterator<move_iterator<_Iterator> >
    {
      enum { __value = 1 };
      typedef __true_type __type;
    };
# 2980 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

}

namespace __gnu_debug
{
  template<typename _Iterator, typename _Sequence, typename _Category>
    class _Safe_iterator;
}

namespace std __attribute__ ((__visibility__ ("default")))
{





  template<typename _Iterator, typename _Container>
    [[__nodiscard__]] __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(__gnu_cxx::__normal_iterator<_Iterator, _Container> __it)
    noexcept(std::is_nothrow_copy_constructible<_Iterator>::value)
    { return __it.base(); }


  template<typename _Iterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(_Iterator __it)
    noexcept(std::is_nothrow_copy_constructible<_Iterator>::value)
    { return __it; }
# 3027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Ite, typename _Seq>
   
    decltype(std::__niter_base(std::declval<_Ite>()))
    __niter_base(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq,
   std::random_access_iterator_tag>&)
    noexcept(std::is_nothrow_copy_constructible<_Ite>::value);



  template<typename _Iterator>
   
    inline auto
    __niter_base(reverse_iterator<_Iterator> __it)
    -> decltype(__make_reverse_iterator(__niter_base(__it.base())))
    { return __make_reverse_iterator(__niter_base(__it.base())); }

  template<typename _Iterator>
   
    inline auto
    __niter_base(move_iterator<_Iterator> __it)
    -> decltype(make_move_iterator(__niter_base(__it.base())))
    { return make_move_iterator(__niter_base(__it.base())); }

  template<typename _Iterator>
   
    inline auto
    __miter_base(reverse_iterator<_Iterator> __it)
    -> decltype(__make_reverse_iterator(__miter_base(__it.base())))
    { return __make_reverse_iterator(__miter_base(__it.base())); }

  template<typename _Iterator>
   
    inline auto
    __miter_base(move_iterator<_Iterator> __it)
    -> decltype(__miter_base(__it.base()))
    { return __miter_base(__it.base()); }






  template<typename _From, typename _To>
    [[__nodiscard__]]
   
    inline _From
    __niter_wrap(_From __from, _To __res)
    { return __from + (std::__niter_base(__res) - std::__niter_base(__from)); }


  template<typename _Iterator>
    [[__nodiscard__]] __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_wrap(const _Iterator&, _Iterator __res)
    { return __res; }
# 3113 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3

}
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/debug.h" 1 3
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/debug/debug.h" 3
namespace std
{
  namespace __debug { }
}




namespace __gnu_debug
{
  using namespace std::__debug;

  template<typename _Ite, typename _Seq, typename _Cat>
    struct _Safe_iterator;
}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/predefined_ops.h" 1 3
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/predefined_ops.h" 3
namespace __gnu_cxx
{
namespace __ops
{
  struct _Iter_less_iter
  {
    template<typename _Iterator1, typename _Iterator2>
      constexpr
      bool
      operator()(_Iterator1 __it1, _Iterator2 __it2) const
      { return *__it1 < *__it2; }
  };

  constexpr
  inline _Iter_less_iter
  __iter_less_iter()
  { return _Iter_less_iter(); }

  struct _Iter_less_val
  {

    constexpr _Iter_less_val() = default;




   
    explicit
    _Iter_less_val(_Iter_less_iter) { }

    template<typename _Iterator, typename _Value>
     
      bool
      operator()(_Iterator __it, _Value& __val) const
      { return *__it < __val; }
  };

 
  inline _Iter_less_val
  __iter_less_val()
  { return _Iter_less_val(); }

 
  inline _Iter_less_val
  __iter_comp_val(_Iter_less_iter)
  { return _Iter_less_val(); }

  struct _Val_less_iter
  {

    constexpr _Val_less_iter() = default;




   
    explicit
    _Val_less_iter(_Iter_less_iter) { }

    template<typename _Value, typename _Iterator>
     
      bool
      operator()(_Value& __val, _Iterator __it) const
      { return __val < *__it; }
  };

 
  inline _Val_less_iter
  __val_less_iter()
  { return _Val_less_iter(); }

 
  inline _Val_less_iter
  __val_comp_iter(_Iter_less_iter)
  { return _Val_less_iter(); }

  struct _Iter_equal_to_iter
  {
    template<typename _Iterator1, typename _Iterator2>
     
      bool
      operator()(_Iterator1 __it1, _Iterator2 __it2) const
      { return *__it1 == *__it2; }
  };

 
  inline _Iter_equal_to_iter
  __iter_equal_to_iter()
  { return _Iter_equal_to_iter(); }

  struct _Iter_equal_to_val
  {
    template<typename _Iterator, typename _Value>
     
      bool
      operator()(_Iterator __it, _Value& __val) const
      { return *__it == __val; }
  };

 
  inline _Iter_equal_to_val
  __iter_equal_to_val()
  { return _Iter_equal_to_val(); }

 
  inline _Iter_equal_to_val
  __iter_comp_val(_Iter_equal_to_iter)
  { return _Iter_equal_to_val(); }

  template<typename _Compare>
    struct _Iter_comp_iter
    {
      _Compare _M_comp;

      explicit constexpr
      _Iter_comp_iter(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

      template<typename _Iterator1, typename _Iterator2>
        constexpr
        bool
        operator()(_Iterator1 __it1, _Iterator2 __it2)
        { return bool(_M_comp(*__it1, *__it2)); }
    };

  template<typename _Compare>
    constexpr
    inline _Iter_comp_iter<_Compare>
    __iter_comp_iter(_Compare __comp)
    { return _Iter_comp_iter<_Compare>(std::move(__comp)); }

  template<typename _Compare>
    struct _Iter_comp_val
    {
      _Compare _M_comp;

     
      explicit
      _Iter_comp_val(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

     
      explicit
      _Iter_comp_val(const _Iter_comp_iter<_Compare>& __comp)
 : _M_comp(__comp._M_comp)
      { }


     
      explicit
      _Iter_comp_val(_Iter_comp_iter<_Compare>&& __comp)
 : _M_comp(std::move(__comp._M_comp))
      { }


      template<typename _Iterator, typename _Value>

 bool
 operator()(_Iterator __it, _Value& __val)
 { return bool(_M_comp(*__it, __val)); }
    };

  template<typename _Compare>
   
    inline _Iter_comp_val<_Compare>
    __iter_comp_val(_Compare __comp)
    { return _Iter_comp_val<_Compare>(std::move(__comp)); }

  template<typename _Compare>
   
    inline _Iter_comp_val<_Compare>
    __iter_comp_val(_Iter_comp_iter<_Compare> __comp)
    { return _Iter_comp_val<_Compare>(std::move(__comp)); }

  template<typename _Compare>
    struct _Val_comp_iter
    {
      _Compare _M_comp;

     
      explicit
      _Val_comp_iter(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

     
      explicit
      _Val_comp_iter(const _Iter_comp_iter<_Compare>& __comp)
 : _M_comp(__comp._M_comp)
      { }


     
      explicit
      _Val_comp_iter(_Iter_comp_iter<_Compare>&& __comp)
 : _M_comp(std::move(__comp._M_comp))
      { }


      template<typename _Value, typename _Iterator>

 bool
 operator()(_Value& __val, _Iterator __it)
 { return bool(_M_comp(__val, *__it)); }
    };

  template<typename _Compare>
   
    inline _Val_comp_iter<_Compare>
    __val_comp_iter(_Compare __comp)
    { return _Val_comp_iter<_Compare>(std::move(__comp)); }

  template<typename _Compare>
   
    inline _Val_comp_iter<_Compare>
    __val_comp_iter(_Iter_comp_iter<_Compare> __comp)
    { return _Val_comp_iter<_Compare>(std::move(__comp)); }

  template<typename _Value>
    struct _Iter_equals_val
    {
      _Value& _M_value;

     
      explicit
      _Iter_equals_val(_Value& __value)
 : _M_value(__value)
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return *__it == _M_value; }
    };

  template<typename _Value>
   
    inline _Iter_equals_val<_Value>
    __iter_equals_val(_Value& __val)
    { return _Iter_equals_val<_Value>(__val); }

  template<typename _Iterator1>
    struct _Iter_equals_iter
    {
      _Iterator1 _M_it1;

     
      explicit
      _Iter_equals_iter(_Iterator1 __it1)
 : _M_it1(__it1)
      { }

      template<typename _Iterator2>

 bool
 operator()(_Iterator2 __it2)
 { return *__it2 == *_M_it1; }
    };

  template<typename _Iterator>
   
    inline _Iter_equals_iter<_Iterator>
    __iter_comp_iter(_Iter_equal_to_iter, _Iterator __it)
    { return _Iter_equals_iter<_Iterator>(__it); }

  template<typename _Predicate>
    struct _Iter_pred
    {
      _Predicate _M_pred;

     
      explicit
      _Iter_pred(_Predicate __pred)
 : _M_pred(std::move(__pred))
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return bool(_M_pred(*__it)); }
    };

  template<typename _Predicate>
   
    inline _Iter_pred<_Predicate>
    __pred_iter(_Predicate __pred)
    { return _Iter_pred<_Predicate>(std::move(__pred)); }

  template<typename _Compare, typename _Value>
    struct _Iter_comp_to_val
    {
      _Compare _M_comp;
      _Value& _M_value;

     
      _Iter_comp_to_val(_Compare __comp, _Value& __value)
 : _M_comp(std::move(__comp)), _M_value(__value)
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return bool(_M_comp(*__it, _M_value)); }
    };

  template<typename _Compare, typename _Value>
    _Iter_comp_to_val<_Compare, _Value>
   
    __iter_comp_val(_Compare __comp, _Value &__val)
    {
      return _Iter_comp_to_val<_Compare, _Value>(std::move(__comp), __val);
    }

  template<typename _Compare, typename _Iterator1>
    struct _Iter_comp_to_iter
    {
      _Compare _M_comp;
      _Iterator1 _M_it1;

     
      _Iter_comp_to_iter(_Compare __comp, _Iterator1 __it1)
 : _M_comp(std::move(__comp)), _M_it1(__it1)
      { }

      template<typename _Iterator2>

 bool
 operator()(_Iterator2 __it2)
 { return bool(_M_comp(*__it2, *_M_it1)); }
    };

  template<typename _Compare, typename _Iterator>
   
    inline _Iter_comp_to_iter<_Compare, _Iterator>
    __iter_comp_iter(_Iter_comp_iter<_Compare> __comp, _Iterator __it)
    {
      return _Iter_comp_to_iter<_Compare, _Iterator>(
   std::move(__comp._M_comp), __it);
    }

  template<typename _Predicate>
    struct _Iter_negate
    {
      _Predicate _M_pred;

     
      explicit
      _Iter_negate(_Predicate __pred)
 : _M_pred(std::move(__pred))
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return !bool(_M_pred(*__it)); }
    };

  template<typename _Predicate>
   
    inline _Iter_negate<_Predicate>
    __negate(_Iter_pred<_Predicate> __pred)
    { return _Iter_negate<_Predicate>(std::move(__pred._M_pred)); }

}
}
# 72 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3




# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/concepts" 2 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 2 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3
  template<typename _Tp>
    constexpr _Tp
    __rotl(_Tp __x, int __s) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      if ((_Nd & (_Nd - 1)) == 0)
 {


   constexpr unsigned __uNd = _Nd;
   const unsigned __r = __s;
   return (__x << (__r % __uNd)) | (__x >> ((-__r) % __uNd));
 }
      const int __r = __s % _Nd;
      if (__r == 0)
 return __x;
      else if (__r > 0)
 return (__x << __r) | (__x >> ((_Nd - __r) % _Nd));
      else
 return (__x >> -__r) | (__x << ((_Nd + __r) % _Nd));
    }

  template<typename _Tp>
    constexpr _Tp
    __rotr(_Tp __x, int __s) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      if ((_Nd & (_Nd - 1)) == 0)
 {


   constexpr unsigned __uNd = _Nd;
   const unsigned __r = __s;
   return (__x >> (__r % __uNd)) | (__x << ((-__r) % __uNd));
 }
      const int __r = __s % _Nd;
      if (__r == 0)
 return __x;
      else if (__r > 0)
 return (__x >> __r) | (__x << ((_Nd - __r) % _Nd));
      else
 return (__x << -__r) | (__x >> ((_Nd + __r) % _Nd));
    }

  template<typename _Tp>
    constexpr int
    __countl_zero(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;

      if (__x == 0)
        return _Nd;

      constexpr auto _Nd_ull = __int_traits<unsigned long long>::__digits;
      constexpr auto _Nd_ul = __int_traits<unsigned long>::__digits;
      constexpr auto _Nd_u = __int_traits<unsigned>::__digits;

      if (_Nd <= _Nd_u)
 {
   constexpr int __diff = _Nd_u - _Nd;
   return __builtin_clz(__x) - __diff;
 }
      else if (_Nd <= _Nd_ul)
 {
   constexpr int __diff = _Nd_ul - _Nd;
   return __builtin_clzl(__x) - __diff;
 }
      else if (_Nd <= _Nd_ull)
 {
   constexpr int __diff = _Nd_ull - _Nd;
   return __builtin_clzll(__x) - __diff;
 }
      else
 {
   static_assert(_Nd <= (2 * _Nd_ull),
   "Maximum supported integer size is 128-bit");

   unsigned long long __high = __x >> _Nd_ull;
   if (__high != 0)
     {
       constexpr int __diff = (2 * _Nd_ull) - _Nd;
       return __builtin_clzll(__high) - __diff;
     }
   constexpr auto __max_ull = __int_traits<unsigned long long>::__max;
   unsigned long long __low = __x & __max_ull;
   return (_Nd - _Nd_ull) + __builtin_clzll(__low);
 }
    }

  template<typename _Tp>
    constexpr int
    __countl_one(_Tp __x) noexcept
    {
      return std::__countl_zero<_Tp>((_Tp)~__x);
    }

  template<typename _Tp>
    constexpr int
    __countr_zero(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;

      if (__x == 0)
        return _Nd;

      constexpr auto _Nd_ull = __int_traits<unsigned long long>::__digits;
      constexpr auto _Nd_ul = __int_traits<unsigned long>::__digits;
      constexpr auto _Nd_u = __int_traits<unsigned>::__digits;

      if (_Nd <= _Nd_u)
 return __builtin_ctz(__x);
      else if (_Nd <= _Nd_ul)
 return __builtin_ctzl(__x);
      else if (_Nd <= _Nd_ull)
 return __builtin_ctzll(__x);
      else
 {
   static_assert(_Nd <= (2 * _Nd_ull),
   "Maximum supported integer size is 128-bit");

   constexpr auto __max_ull = __int_traits<unsigned long long>::__max;
   unsigned long long __low = __x & __max_ull;
   if (__low != 0)
     return __builtin_ctzll(__low);
   unsigned long long __high = __x >> _Nd_ull;
   return __builtin_ctzll(__high) + _Nd_ull;
 }
    }

  template<typename _Tp>
    constexpr int
    __countr_one(_Tp __x) noexcept
    {
      return std::__countr_zero((_Tp)~__x);
    }

  template<typename _Tp>
    constexpr int
    __popcount(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;

      constexpr auto _Nd_ull = __int_traits<unsigned long long>::__digits;
      constexpr auto _Nd_ul = __int_traits<unsigned long>::__digits;
      constexpr auto _Nd_u = __int_traits<unsigned>::__digits;

      if (_Nd <= _Nd_u)
 return __builtin_popcount(__x);
      else if (_Nd <= _Nd_ul)
 return __builtin_popcountl(__x);
      else if (_Nd <= _Nd_ull)
 return __builtin_popcountll(__x);
      else
 {
   static_assert(_Nd <= (2 * _Nd_ull),
   "Maximum supported integer size is 128-bit");

   constexpr auto __max_ull = __int_traits<unsigned long long>::__max;
   unsigned long long __low = __x & __max_ull;
   unsigned long long __high = __x >> _Nd_ull;
   return __builtin_popcountll(__low) + __builtin_popcountll(__high);
 }
    }

  template<typename _Tp>
    constexpr bool
    __has_single_bit(_Tp __x) noexcept
    { return std::__popcount(__x) == 1; }

  template<typename _Tp>
    constexpr _Tp
    __bit_ceil(_Tp __x) noexcept
    {
      using __gnu_cxx::__int_traits;
      constexpr auto _Nd = __int_traits<_Tp>::__digits;
      if (__x == 0 || __x == 1)
        return 1;
      auto __shift_exponent = _Nd - std::__countl_zero((_Tp)(__x - 1u));




      if (!std::__is_constant_evaluated())
 {
   do { if (__builtin_expect(!bool(__shift_exponent != __int_traits<_Tp>::__digits), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit", 344, __PRETTY_FUNCTION__, "__shift_exponent != __int_traits<_Tp>::__digits"); } while (false);
 }

      using __promoted_type = decltype(__x << 1);
      if (!is_same<__promoted_type, _Tp>::value)
 {





   const int __extra_exp = sizeof(__promoted_type) / sizeof(_Tp) / 2;
   __shift_exponent |= (__shift_exponent & _Nd) << __extra_exp;
 }
      return (_Tp)1u << __shift_exponent;
    }

  template<typename _Tp>
    constexpr _Tp
    __bit_floor(_Tp __x) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      if (__x == 0)
        return 0;
      return (_Tp)1u << (_Nd - std::__countl_zero((_Tp)(__x >> 1)));
    }

  template<typename _Tp>
    constexpr int
    __bit_width(_Tp __x) noexcept
    {
      constexpr auto _Nd = __gnu_cxx::__int_traits<_Tp>::__digits;
      return _Nd - std::__countl_zero(__x);
    }
# 484 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bit" 3

}
# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3






namespace std __attribute__ ((__visibility__ ("default")))
{






  template<typename _Tp, typename _Up>
    constexpr
    inline int
    __memcmp(const _Tp* __first1, const _Up* __first2, size_t __num)
    {

      static_assert(sizeof(_Tp) == sizeof(_Up), "can be compared with memcmp");
# 109 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
 return __builtin_memcmp(__first1, __first2, sizeof(_Tp) * __num);
    }
# 153 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    inline void
    iter_swap(_ForwardIterator1 __a, _ForwardIterator2 __b)
    {

     

     
# 186 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      swap(*__a, *__b);

    }
# 202 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    _ForwardIterator2
    swap_ranges(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
  _ForwardIterator2 __first2)
    {

     

     

      ;

      for (; __first1 != __last1; ++__first1, (void)++__first2)
 std::iter_swap(__first1, __first2);
      return __first2;
    }
# 231 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b)
    {

     

      if (__b < __a)
 return __b;
      return __a;
    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    max(const _Tp& __a, const _Tp& __b)
    {

     

      if (__a < __b)
 return __b;
      return __a;
    }
# 279 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {

      if (__comp(__b, __a))
 return __b;
      return __a;
    }
# 301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]] constexpr
    inline const _Tp&
    max(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {

      if (__comp(__a, __b))
 return __b;
      return __a;
    }



  template<typename _Tp, typename _Ref, typename _Ptr>
    struct _Deque_iterator;

  struct _Bit_iterator;






  template<typename _CharT>
    struct char_traits;

  template<typename _CharT, typename _Traits>
    class istreambuf_iterator;

  template<typename _CharT, typename _Traits>
    class ostreambuf_iterator;

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
      ostreambuf_iterator<_CharT, char_traits<_CharT> > >::__type
    __copy_move_a2(_CharT*, _CharT*,
     ostreambuf_iterator<_CharT, char_traits<_CharT> >);

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
      ostreambuf_iterator<_CharT, char_traits<_CharT> > >::__type
    __copy_move_a2(const _CharT*, const _CharT*,
     ostreambuf_iterator<_CharT, char_traits<_CharT> >);

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        _CharT*>::__type
    __copy_move_a2(istreambuf_iterator<_CharT, char_traits<_CharT> >,
     istreambuf_iterator<_CharT, char_traits<_CharT> >, _CharT*);

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<
      __is_char<_CharT>::__value,
      std::_Deque_iterator<_CharT, _CharT&, _CharT*> >::__type
    __copy_move_a2(
 istreambuf_iterator<_CharT, char_traits<_CharT> >,
 istreambuf_iterator<_CharT, char_traits<_CharT> >,
 std::_Deque_iterator<_CharT, _CharT&, _CharT*>);
# 409 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<bool _IsMove, typename _OutIter, typename _InIter>
    __attribute__((__always_inline__))
    inline void
    __assign_one(_OutIter& __out, _InIter& __in)
    {

      if constexpr (_IsMove)
 *__out = std::move(*__in);
      else

 *__out = *__in;
    }

  template<bool _IsMove, typename _InIter, typename _Sent, typename _OutIter>
   
    inline _OutIter
    __copy_move_a2(_InIter __first, _Sent __last, _OutIter __result)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if constexpr (!__is_trivially_assignable(_OutRef, _InRef))
 { }
      else if (std::__is_constant_evaluated())
 { }
      else if constexpr (__memcpyable<_OutIter, _InIter>::__value)
 {
   ptrdiff_t __n = std::distance(__first, __last);
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(__result,
    __first,
    __n * sizeof(*__first));
       __result += __n;
     }
   else if (__n == 1)
     {
       std::__assign_one<_IsMove>(__result, __first);
       ++__result;
     }
   return __result;
 }
# 474 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      for (; __first != __last; ++__result, (void)++__first)
 std::__assign_one<_IsMove>(__result, __first);
      return __result;
    }
#pragma GCC diagnostic pop

  template<bool _IsMove,
    typename _Tp, typename _Ref, typename _Ptr, typename _OI>
    _OI
    __copy_move_a1(std::_Deque_iterator<_Tp, _Ref, _Ptr>,
     std::_Deque_iterator<_Tp, _Ref, _Ptr>,
     _OI);

  template<bool _IsMove,
    typename _ITp, typename _IRef, typename _IPtr, typename _OTp>
    std::_Deque_iterator<_OTp, _OTp&, _OTp*>
    __copy_move_a1(std::_Deque_iterator<_ITp, _IRef, _IPtr>,
     std::_Deque_iterator<_ITp, _IRef, _IPtr>,
     std::_Deque_iterator<_OTp, _OTp&, _OTp*>);

  template<bool _IsMove, typename _II, typename _Tp>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value,
      std::_Deque_iterator<_Tp, _Tp&, _Tp*> >::__type
    __copy_move_a1(_II, _II, std::_Deque_iterator<_Tp, _Tp&, _Tp*>);

  template<bool _IsMove, typename _II, typename _OI>
    __attribute__((__always_inline__))
   
    inline _OI
    __copy_move_a1(_II __first, _II __last, _OI __result)
    { return std::__copy_move_a2<_IsMove>(__first, __last, __result); }

  template<bool _IsMove, typename _II, typename _OI>
    __attribute__((__always_inline__))
   
    inline _OI
    __copy_move_a(_II __first, _II __last, _OI __result)
    {
      return std::__niter_wrap(__result,
  std::__copy_move_a1<_IsMove>(std::__niter_base(__first),
          std::__niter_base(__last),
          std::__niter_base(__result)));
    }

  template<bool _IsMove,
    typename _Ite, typename _Seq, typename _Cat, typename _OI>
   
    _OI
    __copy_move_a(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
    const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
    _OI);

  template<bool _IsMove,
    typename _II, typename _Ite, typename _Seq, typename _Cat>
   
    __gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>
    __copy_move_a(_II, _II,
    const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&);

  template<bool _IsMove,
    typename _IIte, typename _ISeq, typename _ICat,
    typename _OIte, typename _OSeq, typename _OCat>
   
    ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>
    __copy_move_a(const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
    const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
    const ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>&);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _InputIterator, typename _Size, typename _OutputIterator>
   
    _OutputIterator
    __copy_n_a(_InputIterator __first, _Size __n, _OutputIterator __result,
        bool)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if constexpr (!__is_trivially_assignable(_OutRef, _InRef))
 { }




      else if constexpr (__memcpyable<_OutputIterator,
            _InputIterator>::__value)
 {
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(__result,
    __first,
    __n * sizeof(*__first));
       __result += __n;
     }
   else if (__n == 1)
     *__result++ = *__first;
   return __result;
 }
# 593 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      if (__n > 0)
 {
   while (true)
     {
       *__result = *__first;
       ++__result;
       if (--__n > 0)
  ++__first;
       else
  break;
     }
 }
      return __result;
    }
#pragma GCC diagnostic pop


  template<typename _CharT, typename _Size>
    typename __gnu_cxx::__enable_if<
      __is_char<_CharT>::__value, _CharT*>::__type
    __copy_n_a(istreambuf_iterator<_CharT, char_traits<_CharT> >,
        _Size, _CharT*, bool);

  template<typename _CharT, typename _Size>
    typename __gnu_cxx::__enable_if<
      __is_char<_CharT>::__value,
      std::_Deque_iterator<_CharT, _CharT&, _CharT*> >::__type
    __copy_n_a(istreambuf_iterator<_CharT, char_traits<_CharT> >, _Size,
        std::_Deque_iterator<_CharT, _CharT&, _CharT*>,
        bool);
# 642 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II, typename _OI>
   
    inline _OI
    copy(_II __first, _II __last, _OI __result)
    {

     
     

      ;

      return std::__copy_move_a<__is_move_iterator<_II>::__value>
      (std::__miter_base(__first), std::__miter_base(__last), __result);
    }
# 675 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II, typename _OI>
   
    inline _OI
    move(_II __first, _II __last, _OI __result)
    {

     
     

      ;

      return std::__copy_move_a<true>(std::__miter_base(__first),
          std::__miter_base(__last), __result);
    }






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<bool _IsMove, typename _BI1, typename _BI2>
   
    inline _BI2
    __copy_move_backward_a2(_BI1 __first, _BI1 __last, _BI2 __result)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if constexpr (!__is_trivially_assignable(_OutRef, _InRef))
       { }




      else if constexpr (__memcpyable<_BI2, _BI1>::__value)
 {
   ptrdiff_t __n = std::distance(__first, __last);
   std::advance(__result, -__n);
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(__result,
    __first,
    __n * sizeof(*__first));
     }
   else if (__n == 1)
     std::__assign_one<_IsMove>(__result, __first);
   return __result;
 }
# 746 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
      while (__first != __last)
 {
   --__last;
   --__result;
   std::__assign_one<_IsMove>(__result, __last);
 }
      return __result;
    }
#pragma GCC diagnostic pop




  template<bool _IsMove, typename _BI1, typename _BI2>
    __attribute__((__always_inline__))
   
    inline _BI2
    __copy_move_backward_a1(_BI1 __first, _BI1 __last, _BI2 __result)
    { return std::__copy_move_backward_a2<_IsMove>(__first, __last, __result); }

  template<bool _IsMove,
    typename _Tp, typename _Ref, typename _Ptr, typename _OI>
    _OI
    __copy_move_backward_a1(std::_Deque_iterator<_Tp, _Ref, _Ptr>,
       std::_Deque_iterator<_Tp, _Ref, _Ptr>,
       _OI);

  template<bool _IsMove,
    typename _ITp, typename _IRef, typename _IPtr, typename _OTp>
    std::_Deque_iterator<_OTp, _OTp&, _OTp*>
    __copy_move_backward_a1(
   std::_Deque_iterator<_ITp, _IRef, _IPtr>,
   std::_Deque_iterator<_ITp, _IRef, _IPtr>,
   std::_Deque_iterator<_OTp, _OTp&, _OTp*>);

  template<bool _IsMove, typename _II, typename _Tp>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value,
      std::_Deque_iterator<_Tp, _Tp&, _Tp*> >::__type
    __copy_move_backward_a1(_II, _II,
       std::_Deque_iterator<_Tp, _Tp&, _Tp*>);

  template<bool _IsMove, typename _II, typename _OI>
    __attribute__((__always_inline__))
   
    inline _OI
    __copy_move_backward_a(_II __first, _II __last, _OI __result)
    {
      return std::__niter_wrap(__result,
  std::__copy_move_backward_a1<_IsMove>
    (std::__niter_base(__first), std::__niter_base(__last),
     std::__niter_base(__result)));
    }

  template<bool _IsMove,
    typename _Ite, typename _Seq, typename _Cat, typename _OI>
   
    _OI
    __copy_move_backward_a(
  const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
  const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
  _OI);

  template<bool _IsMove,
    typename _II, typename _Ite, typename _Seq, typename _Cat>
   
    __gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>
    __copy_move_backward_a(_II, _II,
  const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&);

  template<bool _IsMove,
    typename _IIte, typename _ISeq, typename _ICat,
    typename _OIte, typename _OSeq, typename _OCat>
   
    ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>
    __copy_move_backward_a(
  const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
  const ::__gnu_debug::_Safe_iterator<_IIte, _ISeq, _ICat>&,
  const ::__gnu_debug::_Safe_iterator<_OIte, _OSeq, _OCat>&);
# 844 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _BI1, typename _BI2>
    __attribute__((__always_inline__))
   
    inline _BI2
    copy_backward(_BI1 __first, _BI1 __last, _BI2 __result)
    {

     
     
     

      ;

      return std::__copy_move_backward_a<__is_move_iterator<_BI1>::__value>
      (std::__miter_base(__first), std::__miter_base(__last), __result);
    }
# 880 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _BI1, typename _BI2>
    __attribute__((__always_inline__))
   
    inline _BI2
    move_backward(_BI1 __first, _BI1 __last, _BI2 __result)
    {

     
     
     

      ;

      return std::__copy_move_backward_a<true>(std::__miter_base(__first),
            std::__miter_base(__last),
            __result);
    }






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _ForwardIterator, typename _Tp>
   
    inline void
    __fill_a1(_ForwardIterator __first, _ForwardIterator __last,
       const _Tp& __value)
    {
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"




      const bool __load_outside_loop =


     __is_trivially_constructible(_Tp, const _Tp&)
     && __is_trivially_assignable(__decltype(*__first), const _Tp&)




     && sizeof(_Tp) <= sizeof(long long);
#pragma GCC diagnostic pop



      typedef typename __gnu_cxx::__conditional_type<__load_outside_loop,
           const _Tp,
           const _Tp&>::__type _Up;
      _Up __val(__value);
      for (; __first != __last; ++__first)
 *__first = __val;
    }
#pragma GCC diagnostic pop


  template<typename _Up, typename _Tp>
   
    inline typename
    __gnu_cxx::__enable_if<__is_byte<_Up>::__value
        && (__are_same<_Up, _Tp>::__value
       || __memcpyable_integer<_Tp>::__value),
      void>::__type
    __fill_a1(_Up* __first, _Up* __last, const _Tp& __x)
    {


      const _Up __val = __x;







      if (const size_t __len = __last - __first)
 __builtin_memset(__first, static_cast<unsigned char>(__val), __len);
    }

  template<typename _Ite, typename _Cont, typename _Tp>
    __attribute__((__always_inline__))
   
    inline void
    __fill_a1(::__gnu_cxx::__normal_iterator<_Ite, _Cont> __first,
       ::__gnu_cxx::__normal_iterator<_Ite, _Cont> __last,
       const _Tp& __value)
    { std::__fill_a1(__first.base(), __last.base(), __value); }

  template<typename _Tp, typename _VTp>
    void
    __fill_a1(const std::_Deque_iterator<_Tp, _Tp&, _Tp*>&,
       const std::_Deque_iterator<_Tp, _Tp&, _Tp*>&,
       const _VTp&);

 
  void
  __fill_a1(std::_Bit_iterator, std::_Bit_iterator,
     const bool&);

  template<typename _FIte, typename _Tp>
    __attribute__((__always_inline__))
   
    inline void
    __fill_a(_FIte __first, _FIte __last, const _Tp& __value)
    { std::__fill_a1(__first, __last, __value); }

  template<typename _Ite, typename _Seq, typename _Cat, typename _Tp>
   
    void
    __fill_a(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
      const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>&,
      const _Tp&);
# 1010 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator, typename _Tp>
    __attribute__((__always_inline__))
   
    inline void
    fill(_ForwardIterator __first, _ForwardIterator __last, const _Tp& __value)
    {

     

      ;

      std::__fill_a(__first, __last, __value);
    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"

  inline constexpr int
  __size_to_integer(int __n) { return __n; }
  inline constexpr unsigned
  __size_to_integer(unsigned __n) { return __n; }
  inline constexpr long
  __size_to_integer(long __n) { return __n; }
  inline constexpr unsigned long
  __size_to_integer(unsigned long __n) { return __n; }
  inline constexpr long long
  __size_to_integer(long long __n) { return __n; }
  inline constexpr unsigned long long
  __size_to_integer(unsigned long long __n) { return __n; }
# 1065 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  inline constexpr long long
  __size_to_integer(float __n) { return (long long)__n; }
  inline constexpr long long
  __size_to_integer(double __n) { return (long long)__n; }
  inline constexpr long long
  __size_to_integer(long double __n) { return (long long)__n; }




#pragma GCC diagnostic pop

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
#pragma GCC diagnostic ignored "-Wlong-long"
  template<typename _OutputIterator, typename _Size, typename _Tp>
   
    inline _OutputIterator
    __fill_n_a1(_OutputIterator __first, _Size __n, const _Tp& __value)
    {

      const bool __load_outside_loop =


     __is_trivially_constructible(_Tp, const _Tp&)
     && __is_trivially_assignable(__decltype(*__first), const _Tp&)




     && sizeof(_Tp) <= sizeof(long long);



      typedef typename __gnu_cxx::__conditional_type<__load_outside_loop,
           const _Tp,
           const _Tp&>::__type _Up;
      _Up __val(__value);
      for (; __n > 0; --__n, (void) ++__first)
 *__first = __val;
      return __first;
    }
#pragma GCC diagnostic pop

  template<typename _Ite, typename _Seq, typename _Cat, typename _Size,
    typename _Tp>
   
    ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>
    __fill_n_a(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq, _Cat>& __first,
        _Size __n, const _Tp& __value,
        std::input_iterator_tag);

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::output_iterator_tag)
    {

      static_assert(is_integral<_Size>{}, "fill_n must pass integral size");

      return __fill_n_a1(__first, __n, __value);
    }

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::input_iterator_tag)
    {

      static_assert(is_integral<_Size>{}, "fill_n must pass integral size");

      return __fill_n_a1(__first, __n, __value);
    }

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::random_access_iterator_tag)
    {

      static_assert(is_integral<_Size>{}, "fill_n must pass integral size");

      if (__n <= 0)
 return __first;

      ;

      std::__fill_a(__first, __first + __n, __value);
      return __first + __n;
    }
# 1179 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _OI, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OI
    fill_n(_OI __first, _Size __n, const _Tp& __value)
    {

     

      return std::__fill_n_a(__first, std::__size_to_integer(__n), __value,
          std::__iterator_category(__first));
    }

  template<bool _BoolType>
    struct __equal
    {
      template<typename _II1, typename _II2>

 static bool
 equal(_II1 __first1, _II1 __last1, _II2 __first2)
 {
   for (; __first1 != __last1; ++__first1, (void) ++__first2)
     if (!(*__first1 == *__first2))
       return false;
   return true;
 }
    };

  template<>
    struct __equal<true>
    {
      template<typename _Tp>

 static bool
 equal(const _Tp* __first1, const _Tp* __last1, const _Tp* __first2)
 {
   if (const size_t __len = (__last1 - __first1))
     return !std::__memcmp(__first1, __first2, __len);
   return true;
 }
    };

  template<typename _Tp, typename _Ref, typename _Ptr, typename _II>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value, bool>::__type
    __equal_aux1(std::_Deque_iterator<_Tp, _Ref, _Ptr>,
   std::_Deque_iterator<_Tp, _Ref, _Ptr>,
   _II);

  template<typename _Tp1, typename _Ref1, typename _Ptr1,
    typename _Tp2, typename _Ref2, typename _Ptr2>
    bool
    __equal_aux1(std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
   std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
   std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>);

  template<typename _II, typename _Tp, typename _Ref, typename _Ptr>
    typename __gnu_cxx::__enable_if<
      __is_random_access_iter<_II>::__value, bool>::__type
    __equal_aux1(_II, _II,
  std::_Deque_iterator<_Tp, _Ref, _Ptr>);

  template<typename _II1, typename _II2>
   
    inline bool
    __equal_aux1(_II1 __first1, _II1 __last1, _II2 __first2)
    {
      typedef typename iterator_traits<_II1>::value_type _ValueType1;
      const bool __simple = ((__is_integer<_ValueType1>::__value

    || __is_pointer(_ValueType1)





        ) && __memcmpable<_II1, _II2>::__value);
      return std::__equal<__simple>::equal(__first1, __last1, __first2);
    }

  template<typename _II1, typename _II2>
    __attribute__((__always_inline__))
   
    inline bool
    __equal_aux(_II1 __first1, _II1 __last1, _II2 __first2)
    {
      return std::__equal_aux1(std::__niter_base(__first1),
          std::__niter_base(__last1),
          std::__niter_base(__first2));
    }

  template<typename _II1, typename _Seq1, typename _Cat1, typename _II2>
   
    bool
    __equal_aux(const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  _II2);

  template<typename _II1, typename _II2, typename _Seq2, typename _Cat2>
   
    bool
    __equal_aux(_II1, _II1,
  const ::__gnu_debug::_Safe_iterator<_II2, _Seq2, _Cat2>&);

  template<typename _II1, typename _Seq1, typename _Cat1,
    typename _II2, typename _Seq2, typename _Cat2>
   
    bool
    __equal_aux(const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_II1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_II2, _Seq2, _Cat2>&);

  template<typename, typename>
    struct __lc_rai
    {
      template<typename _II1, typename _II2>

 static _II1
 __newlast1(_II1, _II1 __last1, _II2, _II2)
 { return __last1; }

      template<typename _II>

 static bool
 __cnd2(_II __first, _II __last)
 { return __first != __last; }
    };

  template<>
    struct __lc_rai<random_access_iterator_tag, random_access_iterator_tag>
    {
      template<typename _RAI1, typename _RAI2>

 static _RAI1
 __newlast1(_RAI1 __first1, _RAI1 __last1,
     _RAI2 __first2, _RAI2 __last2)
 {
   const typename iterator_traits<_RAI1>::difference_type
     __diff1 = __last1 - __first1;
   const typename iterator_traits<_RAI2>::difference_type
     __diff2 = __last2 - __first2;
   return __diff2 < __diff1 ? __first1 + __diff2 : __last1;
 }

      template<typename _RAI>
 static bool
 __cnd2(_RAI, _RAI)
 { return true; }
    };

  template<typename _II1, typename _II2, typename _Compare>
   
    bool
    __lexicographical_compare_impl(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2,
       _Compare __comp)
    {
      typedef typename iterator_traits<_II1>::iterator_category _Category1;
      typedef typename iterator_traits<_II2>::iterator_category _Category2;
      typedef std::__lc_rai<_Category1, _Category2> __rai_type;

      __last1 = __rai_type::__newlast1(__first1, __last1, __first2, __last2);
      for (; __first1 != __last1 && __rai_type::__cnd2(__first2, __last2);
    ++__first1, (void)++__first2)
 {
   if (__comp(__first1, __first2))
     return true;
   if (__comp(__first2, __first1))
     return false;
 }
      return __first1 == __last1 && __first2 != __last2;
    }

  template<bool _BoolType>
    struct __lexicographical_compare
    {
      template<typename _II1, typename _II2>

 static bool
 __lc(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
 {
   using __gnu_cxx::__ops::__iter_less_iter;
   return std::__lexicographical_compare_impl(__first1, __last1,
           __first2, __last2,
           __iter_less_iter());
 }

      template<typename _II1, typename _II2>

 static int
 __3way(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
 {
   while (__first1 != __last1)
     {
       if (__first2 == __last2)
  return +1;
       if (*__first1 < *__first2)
  return -1;
       if (*__first2 < *__first1)
  return +1;
       ++__first1;
       ++__first2;
     }
   return int(__first2 == __last2) - 1;
 }
    };

  template<>
    struct __lexicographical_compare<true>
    {
      template<typename _Tp, typename _Up>

 static bool
 __lc(const _Tp* __first1, const _Tp* __last1,
      const _Up* __first2, const _Up* __last2)
 { return __3way(__first1, __last1, __first2, __last2) < 0; }

      template<typename _Tp, typename _Up>

 static ptrdiff_t
 __3way(const _Tp* __first1, const _Tp* __last1,
        const _Up* __first2, const _Up* __last2)
 {
   const size_t __len1 = __last1 - __first1;
   const size_t __len2 = __last2 - __first2;
   if (const size_t __len = std::min(__len1, __len2))
     if (int __result = std::__memcmp(__first1, __first2, __len))
       return __result;
   return ptrdiff_t(__len1 - __len2);
 }
    };

  template<typename _II1, typename _II2>
   
    inline bool
    __lexicographical_compare_aux1(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2)
    {
      typedef typename iterator_traits<_II1>::value_type _ValueType1;
      typedef typename iterator_traits<_II2>::value_type _ValueType2;

      const bool __simple =
 (__is_memcmp_ordered_with<_ValueType1, _ValueType2>::__value
  && __is_pointer(_II1) && __is_pointer(_II2)







  );




      return std::__lexicographical_compare<__simple>::__lc(__first1, __last1,
           __first2, __last2);
    }

  template<typename _Tp1, typename _Ref1, typename _Ptr1,
    typename _Tp2>
    bool
    __lexicographical_compare_aux1(
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 _Tp2*, _Tp2*);

  template<typename _Tp1,
    typename _Tp2, typename _Ref2, typename _Ptr2>
    bool
    __lexicographical_compare_aux1(_Tp1*, _Tp1*,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>);

  template<typename _Tp1, typename _Ref1, typename _Ptr1,
    typename _Tp2, typename _Ref2, typename _Ptr2>
    bool
    __lexicographical_compare_aux1(
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 std::_Deque_iterator<_Tp1, _Ref1, _Ptr1>,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>,
 std::_Deque_iterator<_Tp2, _Ref2, _Ptr2>);

  template<typename _II1, typename _II2>
   
    inline bool
    __lexicographical_compare_aux(_II1 __first1, _II1 __last1,
      _II2 __first2, _II2 __last2)
    {
      return std::__lexicographical_compare_aux1(std::__niter_base(__first1),
       std::__niter_base(__last1),
       std::__niter_base(__first2),
       std::__niter_base(__last2));
    }

  template<typename _Iter1, typename _Seq1, typename _Cat1,
    typename _II2>
   
    bool
    __lexicographical_compare_aux(
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  _II2, _II2);

  template<typename _II1,
    typename _Iter2, typename _Seq2, typename _Cat2>
   
    bool
    __lexicographical_compare_aux(
  _II1, _II1,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&);

  template<typename _Iter1, typename _Seq1, typename _Cat1,
    typename _Iter2, typename _Seq2, typename _Cat2>
   
    bool
    __lexicographical_compare_aux(
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_Iter1, _Seq1, _Cat1>&,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&,
  const ::__gnu_debug::_Safe_iterator<_Iter2, _Seq2, _Cat2>&);

  template<typename _ForwardIterator, typename _Tp, typename _Compare>
   
    _ForwardIterator
    __lower_bound(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __val, _Compare __comp)
    {
      typedef typename iterator_traits<_ForwardIterator>::difference_type
 _DistanceType;

      _DistanceType __len = std::distance(__first, __last);

      while (__len > 0)
 {
   _DistanceType __half = __len >> 1;
   _ForwardIterator __middle = __first;
   std::advance(__middle, __half);
   if (__comp(__middle, __val))
     {
       __first = __middle;
       ++__first;
       __len = __len - __half - 1;
     }
   else
     __len = __half;
 }
      return __first;
    }
# 1542 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator, typename _Tp>
    [[__nodiscard__]]
    inline _ForwardIterator
    lower_bound(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val)
    {

     
     

      ;

      return std::__lower_bound(__first, __last, __val,
    __gnu_cxx::__ops::__iter_less_val());
    }



  template<typename _Tp>
    inline constexpr _Tp
    __lg(_Tp __n)
    {

      return std::__bit_width(make_unsigned_t<_Tp>(__n)) - 1;
# 1578 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
    }


# 1594 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
    [[__nodiscard__]]
    inline bool
    equal(_II1 __first1, _II1 __last1, _II2 __first2)
    {

     
     
     


      ;

      return std::__equal_aux(__first1, __last1, __first2);
    }
# 1625 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
    [[__nodiscard__]]
    inline bool
    equal(_IIter1 __first1, _IIter1 __last1,
   _IIter2 __first2, _BinaryPredicate __binary_pred)
    {

     
     
      ;

      for (; __first1 != __last1; ++__first1, (void)++__first2)
 if (!bool(__binary_pred(*__first1, *__first2)))
   return false;
      return true;
    }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"


  template<typename _II1, typename _II2>
   
    inline bool
    __equal4(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
    {
      using _RATag = random_access_iterator_tag;
      using _Cat1 = typename iterator_traits<_II1>::iterator_category;
      using _Cat2 = typename iterator_traits<_II2>::iterator_category;
      using _RAIters = __and_<is_same<_Cat1, _RATag>, is_same<_Cat2, _RATag>>;
      if constexpr (_RAIters::value)
 {
   if ((__last1 - __first1) != (__last2 - __first2))
     return false;
   return std::equal(__first1, __last1, __first2);
 }
      else
 {
   for (; __first1 != __last1 && __first2 != __last2;
        ++__first1, (void)++__first2)
     if (!(*__first1 == *__first2))
       return false;
   return __first1 == __last1 && __first2 == __last2;
 }
    }


  template<typename _II1, typename _II2, typename _BinaryPredicate>
   
    inline bool
    __equal4(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2,
      _BinaryPredicate __binary_pred)
    {
      using _RATag = random_access_iterator_tag;
      using _Cat1 = typename iterator_traits<_II1>::iterator_category;
      using _Cat2 = typename iterator_traits<_II2>::iterator_category;
      using _RAIters = __and_<is_same<_Cat1, _RATag>, is_same<_Cat2, _RATag>>;
      if constexpr (_RAIters::value)
 {
   if ((__last1 - __first1) != (__last2 - __first2))
     return false;
   return std::equal(__first1, __last1, __first2,
           __binary_pred);
 }
      else
 {
   for (; __first1 != __last1 && __first2 != __last2;
        ++__first1, (void)++__first2)
     if (!bool(__binary_pred(*__first1, *__first2)))
       return false;
   return __first1 == __last1 && __first2 == __last2;
 }
    }
#pragma GCC diagnostic pop
# 1716 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
    [[__nodiscard__]]
    inline bool
    equal(_II1 __first1, _II1 __last1, _II2 __first2, _II2 __last2)
    {

     
     
     


      ;
      ;

      return std::__equal4(__first1, __last1, __first2, __last2);
    }
# 1749 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
    [[__nodiscard__]]
    inline bool
    equal(_IIter1 __first1, _IIter1 __last1,
   _IIter2 __first2, _IIter2 __last2, _BinaryPredicate __binary_pred)
    {

     
     
      ;
      ;

      return std::__equal4(__first1, __last1, __first2, __last2,
          __binary_pred);
    }
# 1781 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
    [[__nodiscard__]]
    inline bool
    lexicographical_compare(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2)
    {





     
     
     
     
      ;
      ;

      return std::__lexicographical_compare_aux(__first1, __last1,
      __first2, __last2);
    }
# 1816 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2, typename _Compare>
    [[__nodiscard__]]
    inline bool
    lexicographical_compare(_II1 __first1, _II1 __last1,
       _II2 __first2, _II2 __last2, _Compare __comp)
    {

     
     
      ;
      ;

      return std::__lexicographical_compare_impl
 (__first1, __last1, __first2, __last2,
  __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 1931 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
   
    pair<_InputIterator1, _InputIterator2>
    __mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _BinaryPredicate __binary_pred)
    {
      while (__first1 != __last1 && __binary_pred(__first1, __first2))
 {
   ++__first1;
   ++__first2;
 }
      return pair<_InputIterator1, _InputIterator2>(__first1, __first2);
    }
# 1959 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2)
    {

     
     
     


      ;

      return std::__mismatch(__first1, __last1, __first2,
        __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 1993 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _BinaryPredicate __binary_pred)
    {

     
     
      ;

      return std::__mismatch(__first1, __last1, __first2,
 __gnu_cxx::__ops::__iter_comp_iter(__binary_pred));
    }


  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
   
    pair<_InputIterator1, _InputIterator2>
    __mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _InputIterator2 __last2,
        _BinaryPredicate __binary_pred)
    {
      while (__first1 != __last1 && __first2 != __last2
      && __binary_pred(__first1, __first2))
 {
   ++__first1;
   ++__first2;
 }
      return pair<_InputIterator1, _InputIterator2>(__first1, __first2);
    }
# 2041 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _InputIterator2 __last2)
    {

     
     
     


      ;
      ;

      return std::__mismatch(__first1, __last1, __first2, __last2,
        __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 2077 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline pair<_InputIterator1, _InputIterator2>
    mismatch(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _InputIterator2 __last2,
      _BinaryPredicate __binary_pred)
    {

     
     
      ;
      ;

      return std::__mismatch(__first1, __last1, __first2, __last2,
        __gnu_cxx::__ops::__iter_comp_iter(__binary_pred));
    }





  template<typename _Iterator, typename _Predicate>
   
    inline _Iterator
    __find_if(_Iterator __first, _Iterator __last, _Predicate __pred)
    {
#pragma GCC unroll 4
      while (__first != __last && !__pred(__first))
 ++__first;
      return __first;
    }

  template<typename _InputIterator, typename _Predicate>
   
    typename iterator_traits<_InputIterator>::difference_type
    __count_if(_InputIterator __first, _InputIterator __last, _Predicate __pred)
    {
      typename iterator_traits<_InputIterator>::difference_type __n = 0;
      for (; __first != __last; ++__first)
 if (__pred(__first))
   ++__n;
      return __n;
    }

  template<typename _ForwardIterator, typename _Predicate>
   
    _ForwardIterator
    __remove_if(_ForwardIterator __first, _ForwardIterator __last,
  _Predicate __pred)
    {
      __first = std::__find_if(__first, __last, __pred);
      if (__first == __last)
 return __first;
      _ForwardIterator __result = __first;
      ++__first;
      for (; __first != __last; ++__first)
 if (!__pred(__first))
   {
     *__result = std::move(*__first);
     ++__result;
   }
      return __result;
    }

  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    _ForwardIterator1
    __search(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
      _ForwardIterator2 __first2, _ForwardIterator2 __last2,
      _BinaryPredicate __predicate)
    {

      if (__first1 == __last1 || __first2 == __last2)
 return __first1;


      _ForwardIterator2 __p1(__first2);
      if (++__p1 == __last2)
 return std::__find_if(__first1, __last1,
  __gnu_cxx::__ops::__iter_comp_iter(__predicate, __first2));


      _ForwardIterator1 __current = __first1;

      for (;;)
 {
   __first1 =
     std::__find_if(__first1, __last1,
  __gnu_cxx::__ops::__iter_comp_iter(__predicate, __first2));

   if (__first1 == __last1)
     return __last1;

   _ForwardIterator2 __p = __p1;
   __current = __first1;
   if (++__current == __last1)
     return __last1;

   while (__predicate(__current, __p))
     {
       if (++__p == __last2)
  return __first1;
       if (++__current == __last1)
  return __last1;
     }
   ++__first1;
 }
      return __first1;
    }


  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    bool
    __is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
       _ForwardIterator2 __first2, _BinaryPredicate __pred)
    {


      for (; __first1 != __last1; ++__first1, (void)++__first2)
 if (!__pred(__first1, __first2))
   break;

      if (__first1 == __last1)
 return true;



      _ForwardIterator2 __last2 = __first2;
      std::advance(__last2, std::distance(__first1, __last1));
      for (_ForwardIterator1 __scan = __first1; __scan != __last1; ++__scan)
 {
   if (__scan != std::__find_if(__first1, __scan,
     __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan)))
     continue;

   auto __matches
     = std::__count_if(__first2, __last2,
   __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan));
   if (0 == __matches ||
       std::__count_if(__scan, __last1,
   __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan))
       != __matches)
     return false;
 }
      return true;
    }
# 2240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    inline bool
    is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
     _ForwardIterator2 __first2)
    {

     
     
     


      ;

      return std::__is_permutation(__first1, __last1, __first2,
       __gnu_cxx::__ops::__iter_equal_to_iter());
    }



# 2282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    inline _ForwardIterator1
    search(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
    _ForwardIterator2 __first2, _ForwardIterator2 __last2,
    _BinaryPredicate __predicate)
    {

     
     
     


      ;
      ;

      return std::__search(__first1, __last1, __first2, __last2,
      __gnu_cxx::__ops::__iter_comp_iter(__predicate));
    }



}
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 1 3
# 33 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 1 3
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
  class exception
  {
  public:
    exception() noexcept { }
    virtual ~exception() noexcept;

    exception(const exception&) = default;
    exception& operator=(const exception&) = default;
    exception(exception&&) = default;
    exception& operator=(exception&&) = default;




    virtual const char*
    what() const noexcept;
  };



}

}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 2 3





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

#pragma GCC visibility push(default)

extern "C++" {

namespace std
{






  class bad_alloc : public exception
  {
  public:
    bad_alloc() throw() { }


    bad_alloc(const bad_alloc&) = default;
    bad_alloc& operator=(const bad_alloc&) = default;




    virtual ~bad_alloc() throw();


    virtual const char* what() const throw();
  };


  class bad_array_new_length : public bad_alloc
  {
  public:
    bad_array_new_length() throw() { }



    virtual ~bad_array_new_length() throw();


    virtual const char* what() const throw();
  };






  struct nothrow_t
  {

    explicit nothrow_t() = default;

  };

  extern const nothrow_t nothrow;



  typedef void (*new_handler)();



  new_handler set_new_handler(new_handler) throw();



  new_handler get_new_handler() noexcept;

}
# 137 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
[[__nodiscard__]] void* operator new(std::size_t)
 
  __attribute__((__externally_visible__, __malloc__));
[[__nodiscard__]] void* operator new[](std::size_t)
 
  __attribute__((__externally_visible__, __malloc__));
void operator delete(void*) noexcept
  __attribute__((__externally_visible__));
void operator delete[](void*) noexcept
  __attribute__((__externally_visible__));

void operator delete(void*, std::size_t)
  noexcept
  __attribute__((__externally_visible__));
void operator delete[](void*, std::size_t)
  noexcept
  __attribute__((__externally_visible__));

[[__nodiscard__]] void* operator new(std::size_t, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
[[__nodiscard__]] void* operator new[](std::size_t, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
void operator delete(void*, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__));
void operator delete[](void*, const std::nothrow_t&)
  noexcept
  __attribute__((__externally_visible__));
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
[[__nodiscard__]] inline
void* operator new(std::size_t, void* __p)
  noexcept
{ return __p; }
[[__nodiscard__]] inline
void* operator new[](std::size_t, void* __p)
  noexcept
{ return __p; }




inline void operator delete (void*, void*)
  noexcept
{ }
inline void operator delete[](void*, void*)
  noexcept
{ }

}

namespace std
{
# 264 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
}

#pragma GCC visibility pop
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 2 3






namespace std __attribute__ ((__visibility__ ("default")))
{

# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
  template<typename _Tp>
    class __new_allocator
    {
    public:
      typedef _Tp value_type;
      typedef std::size_t size_type;
      typedef std::ptrdiff_t difference_type;

      typedef _Tp* pointer;
      typedef const _Tp* const_pointer;
      typedef _Tp& reference;
      typedef const _Tp& const_reference;

      template<typename _Tp1>
 struct rebind
 { typedef __new_allocator<_Tp1> other; };





      typedef std::true_type propagate_on_container_move_assignment;


      __attribute__((__always_inline__))
     
      __new_allocator() noexcept { }

      __attribute__((__always_inline__))
     
      __new_allocator(const __new_allocator&) noexcept { }

      template<typename _Tp1>
 __attribute__((__always_inline__))

 __new_allocator(const __new_allocator<_Tp1>&) noexcept { }


      __new_allocator& operator=(const __new_allocator&) = default;



      ~__new_allocator() noexcept { }

      pointer
      address(reference __x) const noexcept
      { return std::__addressof(__x); }

      const_pointer
      address(const_reference __x) const noexcept
      { return std::__addressof(__x); }
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      [[__nodiscard__]] _Tp*
      allocate(size_type __n, const void* = static_cast<const void*>(0))
      {



 static_assert(sizeof(_Tp) != 0, "cannot allocate incomplete types");


 if (__builtin_expect(__n > this->_M_max_size(), false))
   {


     if (__n > (std::size_t(-1) / sizeof(_Tp)))
       std::__throw_bad_array_new_length();
     std::__throw_bad_alloc();
   }
# 151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
 return static_cast<_Tp*>(__builtin_operator_new(__n * sizeof(_Tp)));
      }


      void
      deallocate(_Tp* __p, size_type __n __attribute__ ((__unused__)))
      {
# 172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
 __builtin_operator_delete((__p), (__n) * sizeof(_Tp));
      }






      __attribute__((__always_inline__))
      size_type
      max_size() const noexcept
      { return _M_max_size(); }


      template<typename _Up, typename... _Args>
 __attribute__((__always_inline__))
 void
 construct(_Up* __p, _Args&&... __args)
 noexcept(__is_nothrow_new_constructible<_Up, _Args...>)
 { ::new((void *)__p) _Up(std::forward<_Args>(__args)...); }

      template<typename _Up>
 __attribute__((__always_inline__))
 void
 destroy(_Up* __p)
 noexcept(std::is_nothrow_destructible<_Up>::value)
 { __p->~_Up(); }
# 213 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator==(const __new_allocator&, const __new_allocator<_Up>&)
 noexcept
 { return true; }


      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator!=(const __new_allocator&, const __new_allocator<_Up>&)
 noexcept
 { return false; }


    private:
      __attribute__((__always_inline__))
      constexpr size_type
      _M_max_size() const noexcept
      {

 return std::size_t(0x7fffffffffffffffL) / sizeof(_Tp);



      }
    };


}
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 2 3


namespace std
{
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 3
  template<typename _Tp>
    using __allocator_base = __new_allocator<_Tp>;
}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 1 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3
  template<typename>
    class allocator;

  template<>
    class allocator<void>;



  template<typename, typename>
    struct uses_allocator;

  template<typename>
    struct allocator_traits;





}
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 2 3




#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<>
    class allocator<void>
    {
    public:
      typedef void value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;



      typedef void* pointer;
      typedef const void* const_pointer;

      template<typename _Tp1>
 struct rebind
 { typedef allocator<_Tp1> other; };





      using propagate_on_container_move_assignment = true_type;


      using is_always_equal

 = true_type;
# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
    };
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<typename _Tp>
    class allocator : public __allocator_base<_Tp>
    {
    public:
      typedef _Tp value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;



      typedef _Tp* pointer;
      typedef const _Tp* const_pointer;
      typedef _Tp& reference;
      typedef const _Tp& const_reference;

      template<typename _Tp1>
 struct rebind
 { typedef allocator<_Tp1> other; };





      using propagate_on_container_move_assignment = true_type;


      using is_always_equal

 = true_type;





      __attribute__((__always_inline__))
     
      allocator() noexcept { }

      __attribute__((__always_inline__))
     
      allocator(const allocator& __a) noexcept
      : __allocator_base<_Tp>(__a) { }



      allocator& operator=(const allocator&) = default;


      template<typename _Tp1>
 __attribute__((__always_inline__))

 allocator(const allocator<_Tp1>&) noexcept { }

      __attribute__((__always_inline__))



      ~allocator() noexcept { }
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
      friend __attribute__((__always_inline__))
      bool
      operator==(const allocator&, const allocator&) noexcept
      { return true; }


      friend __attribute__((__always_inline__))
      bool
      operator!=(const allocator&, const allocator&) noexcept
      { return false; }



    };






  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator==(const allocator<_T1>&, const allocator<_T2>&)
    noexcept
    { return true; }


  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const allocator<_T1>&, const allocator<_T2>&)
    noexcept
    { return false; }






  template<typename _Tp>
    class allocator<const _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };

  template<typename _Tp>
    class allocator<volatile _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };

  template<typename _Tp>
    class allocator<const volatile _Tp>
    {
    public:
      typedef _Tp value_type;
      allocator() { }
      template<typename _Up> allocator(const allocator<_Up>&) { }
    };







  extern template class allocator<char>;
  extern template class allocator<wchar_t>;






}

#pragma GCC diagnostic pop
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 1 3
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _Tp, typename... _Args>
   
    inline void
    _Construct(_Tp* __p, _Args&&... __args)
    {
# 133 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
      ::new(static_cast<void*>(__p)) _Tp(std::forward<_Args>(__args)...);
    }
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _T1>
    inline void
    _Construct_novalue(_T1* __p)
    { ::new(static_cast<void*>(__p)) _T1; }

  template<typename _ForwardIterator>
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last);




  template<typename _Tp>
    constexpr inline void
    _Destroy(_Tp* __pointer)
    {



      __pointer->~_Tp();

    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 199 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _ForwardIterator>
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;


      static_assert(is_destructible<_Value_type>::value,
      "value type is destructible");
      if constexpr (!__has_trivial_destructor(_Value_type))
 for (; __first != __last; ++__first)
   std::_Destroy(std::__addressof(*__first));
# 221 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    _Destroy_n(_ForwardIterator __first, _Size __count)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;


      static_assert(is_destructible<_Value_type>::value,
      "value type is destructible");
      if constexpr (!__has_trivial_destructor(_Value_type))
 for (; __count > 0; (void)++__first, --__count)
   std::_Destroy(std::__addressof(*__first));





      else
 std::advance(__first, __count);
      return __first;




    }
#pragma GCC diagnostic pop
# 299 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3

}
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 1 3
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




 
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
#pragma GCC diagnostic ignored "-Wc++17-extensions"


  struct __allocator_traits_base
  {



    template<typename _Tp, typename _Up, typename = void>

      struct __rebind : __replace_first_arg<_Tp, _Up>
      {
 static_assert(is_same<
   typename __replace_first_arg<_Tp, typename _Tp::value_type>::type,
   _Tp>::value,
   "allocator_traits<A>::rebind_alloc<A::value_type> must be A");
      };

    template<typename _Tp, typename _Up>




      struct __rebind<_Tp, _Up,
        __void_t<typename _Tp::template rebind<_Up>::other>>

      {
 using type = typename _Tp::template rebind<_Up>::other;

 static_assert(is_same<
   typename _Tp::template rebind<typename _Tp::value_type>::other,
   _Tp>::value,
   "allocator_traits<A>::rebind_alloc<A::value_type> must be A");
      };

  protected:
    template<typename _Tp>
      using __pointer = typename _Tp::pointer;
    template<typename _Tp>
      using __c_pointer = typename _Tp::const_pointer;
    template<typename _Tp>
      using __v_pointer = typename _Tp::void_pointer;
    template<typename _Tp>
      using __cv_pointer = typename _Tp::const_void_pointer;
    template<typename _Tp>
      using __pocca = typename _Tp::propagate_on_container_copy_assignment;
    template<typename _Tp>
      using __pocma = typename _Tp::propagate_on_container_move_assignment;
    template<typename _Tp>
      using __pocs = typename _Tp::propagate_on_container_swap;
    template<typename _Tp>
      using __equal = __type_identity<typename _Tp::is_always_equal>;
# 115 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc, typename _Sz, typename _Vp>
      using __allocate_hint_t
 = decltype(std::declval<_Alloc&>()
       .allocate(std::declval<_Sz>(), std::declval<_Vp>()));
    template<typename _Alloc, typename _Sz, typename _Vp, typename = void>
      static constexpr bool __has_allocate_hint = false;
    template<typename _Alloc, typename _Sz, typename _Vp>
      static constexpr bool
      __has_allocate_hint<_Alloc, _Sz, _Vp,
     __void_t<__allocate_hint_t<_Alloc, _Sz, _Vp>>>
 = true;
# 152 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc, typename _Tp, typename... _Args>
      using __construct_t
 = decltype(std::declval<_Alloc&>().construct(std::declval<_Tp*>(),
           std::declval<_Args>()...));
    template<typename _Alloc, typename _Tp, typename, typename... _Args>
      static constexpr bool __has_construct_impl = false;
    template<typename _Alloc, typename _Tp, typename... _Args>
      static constexpr bool
      __has_construct_impl<_Alloc, _Tp,
      __void_t<__construct_t<_Alloc, _Tp, _Args...>>,
      _Args...>
 = true;
    template<typename _Alloc, typename _Tp, typename... _Args>
      static constexpr bool __has_construct
 = __has_construct_impl<_Alloc, _Tp, void, _Args...>;
    template<typename _Tp, typename... _Args>
      using __new_expr_t
 = decltype(::new((void*)0) _Tp(std::declval<_Args>()...));
    template<typename _Tp, typename, typename... _Args>
      static constexpr bool __has_new_expr = false;
    template<typename _Tp, typename... _Args>
      static constexpr bool
      __has_new_expr<_Tp, __void_t<__new_expr_t<_Tp, _Args...>>, _Args...>
 = true;
    template<typename _Alloc, typename _Tp, typename... _Args>
      static constexpr bool __can_construct
 = __has_construct<_Alloc, _Tp, _Args...>
     || __has_new_expr<_Tp, void, _Args...>;
# 189 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc, typename _Tp>
      using __destroy_t
 = decltype(std::declval<_Alloc&>().destroy(std::declval<_Tp*>()));
    template<typename _Alloc, typename _Tp, typename = void>
      static constexpr bool __has_destroy = false;
    template<typename _Alloc, typename _Tp>
      static constexpr bool __has_destroy<_Alloc, _Tp,
       __void_t<__destroy_t<_Alloc, _Tp>>>
 = true;
# 207 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc>
      using __max_size_t = decltype(std::declval<const _Alloc&>().max_size());
    template<typename _Alloc, typename = void>
      static constexpr bool __has_max_size = false;
    template<typename _Alloc>
      static constexpr bool __has_max_size<_Alloc,
        __void_t<__max_size_t<_Alloc>>>
 = true;
# 225 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    template<typename _Alloc>
      using __soccc_t
 = decltype(std::declval<const _Alloc&>()
       .select_on_container_copy_construction());
    template<typename _Alloc, typename = void>
      static constexpr bool __has_soccc = false;
    template<typename _Alloc>
      static constexpr bool __has_soccc<_Alloc, __void_t<__soccc_t<_Alloc>>>
 = true;

  };

  template<typename _Alloc, typename _Up>
    using __alloc_rebind
      = typename __allocator_traits_base::template __rebind<_Alloc, _Up>::type;
# 248 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Alloc>
    struct allocator_traits : __allocator_traits_base
    {

      typedef _Alloc allocator_type;

      typedef typename _Alloc::value_type value_type;






      using pointer = __detected_or_t<value_type*, __pointer, _Alloc>;

    private:

      template<template<typename> class _Func, typename _Tp, typename = void>
 struct _Ptr
 {
   using type = typename pointer_traits<pointer>::template rebind<_Tp>;
 };

      template<template<typename> class _Func, typename _Tp>
 struct _Ptr<_Func, _Tp, __void_t<_Func<_Alloc>>>
 {
   using type = _Func<_Alloc>;
 };


      template<typename _A2, typename _PtrT, typename = void>
 struct _Diff
 { using type = typename pointer_traits<_PtrT>::difference_type; };

      template<typename _A2, typename _PtrT>
 struct _Diff<_A2, _PtrT, __void_t<typename _A2::difference_type>>
 { using type = typename _A2::difference_type; };


      template<typename _A2, typename _DiffT, typename = void>
 struct _Size : make_unsigned<_DiffT> { };

      template<typename _A2, typename _DiffT>
 struct _Size<_A2, _DiffT, __void_t<typename _A2::size_type>>
 { using type = typename _A2::size_type; };

    public:






      using const_pointer = typename _Ptr<__c_pointer, const value_type>::type;







      using void_pointer = typename _Ptr<__v_pointer, void>::type;







      using const_void_pointer = typename _Ptr<__cv_pointer, const void>::type;







      using difference_type = typename _Diff<_Alloc, pointer>::type;







      using size_type = typename _Size<_Alloc, difference_type>::type;







      using propagate_on_container_copy_assignment
 = __detected_or_t<false_type, __pocca, _Alloc>;







      using propagate_on_container_move_assignment
 = __detected_or_t<false_type, __pocma, _Alloc>;







      using propagate_on_container_swap
 = __detected_or_t<false_type, __pocs, _Alloc>;







      using is_always_equal
 = typename __detected_or_t<is_empty<_Alloc>, __equal, _Alloc>::type;

      template<typename _Tp>
 using rebind_alloc = __alloc_rebind<_Alloc, _Tp>;
      template<typename _Tp>
 using rebind_traits = allocator_traits<rebind_alloc<_Tp>>;
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__]] static pointer
      allocate(_Alloc& __a, size_type __n)
      { return __a.allocate(__n); }
# 398 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__]] static pointer
      allocate(_Alloc& __a, size_type __n, const_void_pointer __hint)
      {
 if constexpr (__has_allocate_hint<_Alloc, size_type, const_void_pointer>)
   return __a.allocate(__n, __hint);
 else
   return __a.allocate(__n);
      }
# 415 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      static void
      deallocate(_Alloc& __a, pointer __p, size_type __n)
      { __a.deallocate(__p, __n); }
# 430 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Tp, typename... _Args>




 static __enable_if_t<__can_construct<_Alloc, _Tp, _Args...>>

 construct(_Alloc& __a, _Tp* __p, _Args&&... __args)
 noexcept(_S_nothrow_construct<_Tp, _Args...>())
 {
   if constexpr (__has_construct<_Alloc, _Tp, _Args...>)
     __a.construct(__p, std::forward<_Args>(__args)...);
   else
     std::_Construct(__p, std::forward<_Args>(__args)...);
 }
# 454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Tp>
 static void
 destroy(_Alloc& __a, _Tp* __p)
 noexcept(_S_nothrow_destroy<_Tp>())
 {
   if constexpr (__has_destroy<_Alloc, _Tp>)
     __a.destroy(__p);
   else
     std::_Destroy(__p);
 }
# 473 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      static size_type
      max_size(const _Alloc& __a) noexcept
      {
 if constexpr (__has_max_size<_Alloc>)
   return __a.max_size();
 else


   return __gnu_cxx::__numeric_traits<size_type>::__max
     / sizeof(value_type);
      }
# 493 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      static _Alloc
      select_on_container_copy_construction(const _Alloc& __rhs)
      {
 if constexpr (__has_soccc<_Alloc>)
   return __rhs.select_on_container_copy_construction();
 else
   return __rhs;
      }

    private:

      template<typename _Tp, typename... _Args>
 static constexpr bool
 _S_nothrow_construct(_Alloc* __a = nullptr, _Tp* __p = nullptr)
 {
   if constexpr (__has_construct<_Alloc, _Tp, _Args...>)
     return noexcept(__a->construct(__p, std::declval<_Args>()...));
   else
     return __is_nothrow_new_constructible<_Tp, _Args...>;
 }

      template<typename _Tp>
 static constexpr bool
 _S_nothrow_destroy(_Alloc* __a = nullptr, _Tp* __p = nullptr)
 {
   if constexpr (__has_destroy<_Alloc, _Tp>)
     return noexcept(__a->destroy(__p));
   else
     return is_nothrow_destructible<_Tp>::value;
 }
# 548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
    };
#pragma GCC diagnostic pop
# 559 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Tp>
    struct allocator_traits<allocator<_Tp>>
    {

      using allocator_type = allocator<_Tp>;


      using value_type = _Tp;


      using pointer = _Tp*;


      using const_pointer = const _Tp*;


      using void_pointer = void*;


      using const_void_pointer = const void*;


      using difference_type = std::ptrdiff_t;


      using size_type = std::size_t;


      using propagate_on_container_copy_assignment = false_type;


      using propagate_on_container_move_assignment = true_type;


      using propagate_on_container_swap = false_type;


      using is_always_equal = true_type;

      template<typename _Up>
 using rebind_alloc = allocator<_Up>;

      template<typename _Up>
 using rebind_traits = allocator_traits<allocator<_Up>>;
# 611 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__,__gnu__::__always_inline__]]
      static pointer
      allocate(allocator_type& __a, size_type __n)
      { return __a.allocate(__n); }
# 626 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__nodiscard__,__gnu__::__always_inline__]]
      static pointer
      allocate(allocator_type& __a, size_type __n,
        [[maybe_unused]] const_void_pointer __hint)
      {

 return __a.allocate(__n, __hint);



      }
# 646 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      [[__gnu__::__always_inline__]]
      static void
      deallocate(allocator_type& __a, pointer __p, size_type __n)
      { __a.deallocate(__p, __n); }
# 662 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up, typename... _Args>
 [[__gnu__::__always_inline__]]
 static void
 construct(allocator_type& __a __attribute__((__unused__)),
    _Up* __p, _Args&&... __args)

 noexcept(noexcept(__a.construct(__p, std::forward<_Args>(__args)...)))



 {

   __a.construct(__p, std::forward<_Args>(__args)...);





 }
# 689 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up>
 [[__gnu__::__always_inline__]]
 static void
 destroy(allocator_type& __a __attribute__((__unused__)), _Up* __p)
 noexcept(is_nothrow_destructible<_Up>::value)
 {

   __a.destroy(__p);



 }






      [[__gnu__::__always_inline__]]
      static size_type
      max_size(const allocator_type& __a __attribute__((__unused__))) noexcept
      {

 return __a.max_size();



      }






      [[__gnu__::__always_inline__]]
      static allocator_type
      select_on_container_copy_construction(const allocator_type& __rhs)
      { return __rhs; }
    };
# 736 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<>
    struct allocator_traits<allocator<void>>
    {

      using allocator_type = allocator<void>;


      using value_type = void;


      using pointer = void*;


      using const_pointer = const void*;


      using void_pointer = void*;


      using const_void_pointer = const void*;


      using difference_type = std::ptrdiff_t;


      using size_type = std::size_t;


      using propagate_on_container_copy_assignment = false_type;


      using propagate_on_container_move_assignment = true_type;


      using propagate_on_container_swap = false_type;


      using is_always_equal = true_type;

      template<typename _Up>
 using rebind_alloc = allocator<_Up>;

      template<typename _Up>
 using rebind_traits = allocator_traits<allocator<_Up>>;


      static void*
      allocate(allocator_type&, size_type, const void* = nullptr) = delete;


      static void
      deallocate(allocator_type&, void*, size_type) = delete;
# 800 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up, typename... _Args>
 [[__gnu__::__always_inline__]]
 static void
 construct(allocator_type&, _Up* __p, _Args&&... __args)
 noexcept(__is_nothrow_new_constructible<_Up, _Args...>)
 { std::_Construct(__p, std::forward<_Args>(__args)...); }
# 814 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Up>
 [[__gnu__::__always_inline__]]
 static void
 destroy(allocator_type&, _Up* __p)
 noexcept(is_nothrow_destructible<_Up>::value)
 { std::_Destroy(__p); }


      static size_type
      max_size(const allocator_type&) = delete;






      [[__gnu__::__always_inline__]]
      static allocator_type
      select_on_container_copy_construction(const allocator_type& __rhs)
      { return __rhs; }
    };



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr inline void
    __alloc_on_copy(_Alloc& __one, const _Alloc& __two)
    {
      using __traits = allocator_traits<_Alloc>;
      using __pocca =
 typename __traits::propagate_on_container_copy_assignment::type;
      if constexpr (__pocca::value)
 __one = __two;
    }

  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr _Alloc
    __alloc_on_copy(const _Alloc& __a)
    {
      typedef allocator_traits<_Alloc> __traits;
      return __traits::select_on_container_copy_construction(__a);
    }

  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr inline void
    __alloc_on_move(_Alloc& __one, _Alloc& __two)
    {
      using __traits = allocator_traits<_Alloc>;
      using __pocma
 = typename __traits::propagate_on_container_move_assignment::type;
      if constexpr (__pocma::value)
 __one = std::move(__two);
    }

  template<typename _Alloc>
    [[__gnu__::__always_inline__]]
    constexpr inline void
    __alloc_on_swap(_Alloc& __one, _Alloc& __two)
    {
      using __traits = allocator_traits<_Alloc>;
      using __pocs = typename __traits::propagate_on_container_swap::type;
      if constexpr (__pocs::value)
 {
   using std::swap;
   swap(__one, __two);
 }
    }
#pragma GCC diagnostic pop

  template<typename _Alloc, typename _Tp,
    typename _ValueT = __remove_cvref_t<typename _Alloc::value_type>,
    typename = void>
    struct __is_alloc_insertable_impl
    : false_type
    { };

  template<typename _Alloc, typename _Tp, typename _ValueT>
    struct __is_alloc_insertable_impl<_Alloc, _Tp, _ValueT,
      __void_t<decltype(allocator_traits<_Alloc>::construct(
     std::declval<_Alloc&>(), std::declval<_ValueT*>(),
     std::declval<_Tp>()))>>
    : true_type
    { };




  template<typename _Alloc>
    struct __is_copy_insertable
    : __is_alloc_insertable_impl<_Alloc,
     typename _Alloc::value_type const&>::type
    { };



  template<typename _Tp>
    struct __is_copy_insertable<allocator<_Tp>>
    : is_copy_constructible<_Tp>
    { };





  template<typename _Alloc>
    struct __is_move_insertable
    : __is_alloc_insertable_impl<_Alloc, typename _Alloc::value_type>::type
    { };



  template<typename _Tp>
    struct __is_move_insertable<allocator<_Tp>>
    : is_move_constructible<_Tp>
    { };



  template<typename _Alloc, typename = void>
    struct __is_allocator : false_type { };

  template<typename _Alloc>
    struct __is_allocator<_Alloc,
      __void_t<typename _Alloc::value_type,
        decltype(std::declval<_Alloc&>().allocate(size_t{}))>>
    : true_type { };

  template<typename _Alloc>
    using _RequireAllocator
      = typename enable_if<__is_allocator<_Alloc>::value, _Alloc>::type;

  template<typename _Alloc>
    using _RequireNotAllocator
      = typename enable_if<!__is_allocator<_Alloc>::value, _Alloc>::type;
# 970 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Alloc, bool = __is_empty(_Alloc)>
    struct __alloc_swap
    { static void _S_do_it(_Alloc&, _Alloc&) noexcept { } };

  template<typename _Alloc>
    struct __alloc_swap<_Alloc, false>
    {
      static void
      _S_do_it(_Alloc& __one, _Alloc& __two) noexcept
      {

 if (__one != __two)
   swap(__one, __two);
      }
    };


  template<typename _Tp, bool
    = __or_<is_copy_constructible<typename _Tp::value_type>,
            is_nothrow_move_constructible<typename _Tp::value_type>>::value>
    struct __shrink_to_fit_aux
    { static bool _S_do_it(_Tp&) noexcept { return false; } };

  template<typename _Tp>
    struct __shrink_to_fit_aux<_Tp, true>
    {
     
      static bool
      _S_do_it(_Tp& __c) noexcept
      {

 try
   {
     _Tp(__make_move_if_noexcept_iterator(__c.begin()),
  __make_move_if_noexcept_iterator(__c.end()),
  __c.get_allocator()).swap(__c);
     return true;
   }
 catch(...)
   { return false; }



      }
    };
# 1023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _ForwardIterator, typename _Allocator>
   
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      _Allocator& __alloc)
    {
      for (; __first != __last; ++__first)



 allocator_traits<_Allocator>::destroy(__alloc,
           std::__addressof(*__first));

    }


  template<typename _ForwardIterator, typename _Tp>
    __attribute__((__always_inline__))
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      allocator<_Tp>&)
    {
      std::_Destroy(__first, __last);
    }





}
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 2 3

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{






template<typename _Alloc, typename = typename _Alloc::value_type>
  struct __alloc_traits

  : std::allocator_traits<_Alloc>

  {
    typedef _Alloc allocator_type;

    typedef std::allocator_traits<_Alloc> _Base_type;
    typedef typename _Base_type::value_type value_type;
    typedef typename _Base_type::pointer pointer;
    typedef typename _Base_type::const_pointer const_pointer;
    typedef typename _Base_type::size_type size_type;
    typedef typename _Base_type::difference_type difference_type;

    typedef value_type& reference;
    typedef const value_type& const_reference;
    using _Base_type::allocate;
    using _Base_type::deallocate;
    using _Base_type::construct;
    using _Base_type::destroy;
    using _Base_type::max_size;

  private:
    template<typename _Ptr>
      using __is_custom_pointer
 = std::__and_<std::is_same<pointer, _Ptr>,
        std::__not_<std::is_pointer<_Ptr>>>;

  public:

    template<typename _Ptr, typename... _Args>
      [[__gnu__::__always_inline__]]
      static constexpr
      std::__enable_if_t<__is_custom_pointer<_Ptr>::value>
      construct(_Alloc& __a, _Ptr __p, _Args&&... __args)
      noexcept(noexcept(_Base_type::construct(__a, std::__to_address(__p),
           std::forward<_Args>(__args)...)))
      {
 _Base_type::construct(__a, std::__to_address(__p),
         std::forward<_Args>(__args)...);
      }


    template<typename _Ptr>
      [[__gnu__::__always_inline__]]
      static constexpr
      std::__enable_if_t<__is_custom_pointer<_Ptr>::value>
      destroy(_Alloc& __a, _Ptr __p)
      noexcept(noexcept(_Base_type::destroy(__a, std::__to_address(__p))))
      { _Base_type::destroy(__a, std::__to_address(__p)); }

    [[__gnu__::__always_inline__]]
    static constexpr _Alloc _S_select_on_copy(const _Alloc& __a)
    { return _Base_type::select_on_container_copy_construction(__a); }

    [[__gnu__::__always_inline__]]
    static constexpr void _S_on_swap(_Alloc& __a, _Alloc& __b)
    { std::__alloc_on_swap(__a, __b); }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_propagate_on_copy_assign()
    { return _Base_type::propagate_on_container_copy_assignment::value; }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_propagate_on_move_assign()
    { return _Base_type::propagate_on_container_move_assignment::value; }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_propagate_on_swap()
    { return _Base_type::propagate_on_container_swap::value; }

    [[__gnu__::__always_inline__]]
    static constexpr bool _S_always_equal()
    { return _Base_type::is_always_equal::value; }

    __attribute__((__always_inline__))
    static constexpr bool _S_nothrow_move()
    { return _S_propagate_on_move_assign() || _S_always_equal(); }

    template<typename _Tp>
      struct rebind
      { typedef typename _Base_type::template rebind_alloc<_Tp> other; };
# 182 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
  };


}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{








  template<typename _ForwardIterator, typename _Alloc = void>
    struct _UninitDestroyGuard
    {
     
      explicit
      _UninitDestroyGuard(_ForwardIterator& __first, _Alloc& __a)
      : _M_first(__first), _M_cur(__builtin_addressof(__first)), _M_alloc(__a)
      { }

     
      ~_UninitDestroyGuard()
      {
 if (__builtin_expect(_M_cur != 0, 0))
   std::_Destroy(_M_first, *_M_cur, _M_alloc);
      }

     
      void release() { _M_cur = 0; }

    private:
      _ForwardIterator const _M_first;
      _ForwardIterator* _M_cur;
      _Alloc& _M_alloc;

      _UninitDestroyGuard(const _UninitDestroyGuard&);
    };

  template<typename _ForwardIterator>
    struct _UninitDestroyGuard<_ForwardIterator, void>
    {
     
      explicit
      _UninitDestroyGuard(_ForwardIterator& __first)
      : _M_first(__first), _M_cur(__builtin_addressof(__first))
      { }

     
      ~_UninitDestroyGuard()
      {
 if (__builtin_expect(_M_cur != 0, 0))
   std::_Destroy(_M_first, *_M_cur);
      }

     
      void release() { _M_cur = 0; }

      _ForwardIterator const _M_first;
      _ForwardIterator* _M_cur;

    private:
      _UninitDestroyGuard(const _UninitDestroyGuard&);
    };



  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator>
   
    _ForwardIterator
    __do_uninit_copy(_InputIterator __first, _Sentinel __last,
       _ForwardIterator __result)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __first != __last; ++__first, (void)++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _ForwardIterator>
    inline _ForwardIterator
    uninitialized_copy(_InputIterator __first, _InputIterator __last,
         _ForwardIterator __result)
    {
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      using _Dest = decltype(std::__niter_base(__result));
      using _Src = decltype(std::__niter_base(__first));
      using _ValT = typename iterator_traits<_ForwardIterator>::value_type;

      if constexpr (!__is_trivially_constructible(_ValT, decltype(*__first)))
 return std::__do_uninit_copy(__first, __last, __result);
      else if constexpr (__memcpyable<_Dest, _Src>::__value)
 {
   ptrdiff_t __n = __last - __first;
   if (__n > 0) [[__likely__]]
     {
       using _ValT = typename remove_pointer<_Src>::type;
       __builtin_memcpy(std::__niter_base(__result),
          std::__niter_base(__first),
          __n * sizeof(_ValT));
       __result += __n;
     }
   return __result;
 }
# 296 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      else
 return std::__do_uninit_copy(__first, __last, __result);
# 311 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
    }
#pragma GCC diagnostic pop




  template<typename _ForwardIterator, typename _Tp>
    void
    __do_uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
       const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);
      for (; __first != __last; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
    }
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Tp>
    inline void
    uninitialized_fill(_ForwardIterator __first, _ForwardIterator __last,
         const _Tp& __x)
    {
# 397 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
      if constexpr (__is_byte<_ValueType>::__value)
 if constexpr (is_same<_ValueType, _Tp>::value
   || is_integral<_Tp>::value)
   {
     using _BasePtr = decltype(std::__niter_base(__first));
     if constexpr (is_pointer<_BasePtr>::value)
       {
  void* __dest = std::__niter_base(__first);
  ptrdiff_t __n = __last - __first;
  if (__n > 0) [[__likely__]]
    __builtin_memset(__dest, (unsigned char)__x, __n);
  return;
       }
# 426 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
   }
      std::__do_uninit_fill(__first, __last, __x);
#pragma GCC diagnostic pop






    }




  template<typename _ForwardIterator, typename _Size, typename _Tp>
   
    _ForwardIterator
    __do_uninit_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
      if constexpr (is_integral<_Size>::value)

 do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h", 451, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
      else if constexpr (is_floating_point<_Size>::value)

 do { if (__builtin_expect(!bool(__n >= 0 && static_cast<size_t>(__n) == __n), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h", 454, __PRETTY_FUNCTION__, "__n >= 0 && static_cast<size_t>(__n) == __n"); } while (false);
#pragma GCC diagnostic pop

      for (; __n--; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }
# 498 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 511 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Size, typename _Tp>
    inline _ForwardIterator
    uninitialized_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {






      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;


      if constexpr (__is_byte<_ValueType>::__value)
 if constexpr (is_integral<_Tp>::value)
   if constexpr (is_integral<_Size>::value)
     {
       using _BasePtr = decltype(std::__niter_base(__first));
       if constexpr (is_pointer<_BasePtr>::value)
  {
    void* __dest = std::__niter_base(__first);
    if (__n > 0) [[__likely__]]
      {
        __builtin_memset(__dest, (unsigned char)__x, __n);
        __first += __n;
      }
    return __first;
  }
# 552 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
     }
      return std::__do_uninit_fill_n(__first, __n, __x);
# 562 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
    }
#pragma GCC diagnostic pop
# 573 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator, typename _Allocator>
   
    _ForwardIterator
    __uninitialized_copy_a(_InputIterator __first, _Sentinel __last,
      _ForwardIterator __result, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__result, __alloc);

      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first, (void)++__result)
 __traits::construct(__alloc, std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }


  template<typename _InputIterator, typename _Sentinel,
    typename _ForwardIterator, typename _Tp>
   
    inline _ForwardIterator
    __uninitialized_copy_a(_InputIterator __first, _Sentinel __last,
      _ForwardIterator __result, allocator<_Tp>&)
    {
# 619 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      return std::uninitialized_copy(__first, __last, __result);

    }


  template<typename _InputIterator, typename _ForwardIterator,
    typename _Allocator>
   
    inline _ForwardIterator
    __uninitialized_move_a(_InputIterator __first, _InputIterator __last,
      _ForwardIterator __result, _Allocator& __alloc)
    {
      return std::__uninitialized_copy_a(std::make_move_iterator(__first),
      std::make_move_iterator(__last),
      __result, __alloc);
    }

  template<typename _InputIterator, typename _ForwardIterator,
    typename _Allocator>
   
    inline _ForwardIterator
    __uninitialized_move_if_noexcept_a(_InputIterator __first,
           _InputIterator __last,
           _ForwardIterator __result,
           _Allocator& __alloc)
    {
      return std::__uninitialized_copy_a
 (std::__make_move_if_noexcept_iterator(__first),
  std::__make_move_if_noexcept_iterator(__last), __result, __alloc);
    }

  template<typename _ForwardIterator, typename _Tp, typename _Allocator>
   
    void
    __uninitialized_fill_a(_ForwardIterator __first, _ForwardIterator __last,
      const _Tp& __x, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__first, __alloc);

      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first)
 __traits::construct(__alloc, std::__addressof(*__first), __x);

      __guard.release();
    }


  template<typename _ForwardIterator, typename _Tp, typename _Tp2>
   
    inline void
    __uninitialized_fill_a(_ForwardIterator __first, _ForwardIterator __last,
      const _Tp& __x, allocator<_Tp2>&)
    {




      std::uninitialized_fill(__first, __last, __x);
    }


  template<typename _ForwardIterator, typename _Size, typename _Tp,
    typename _Allocator>
    
    _ForwardIterator
    __uninitialized_fill_n_a(_ForwardIterator __first, _Size __n,
        const _Tp& __x, _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator>
 __guard(__first, __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __n > 0; --__n, (void) ++__first)
 __traits::construct(__alloc, std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }


  template<typename _ForwardIterator, typename _Size, typename _Tp,
    typename _Tp2>
   
    inline _ForwardIterator
    __uninitialized_fill_n_a(_ForwardIterator __first, _Size __n,
        const _Tp& __x, allocator<_Tp2>&)
    {




      return std::uninitialized_fill_n(__first, __n, __x);
    }
# 722 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _ForwardIterator, typename _Allocator>
    inline _ForwardIterator
    __uninitialized_copy_move(_InputIterator1 __first1,
         _InputIterator1 __last1,
         _InputIterator2 __first2,
         _InputIterator2 __last2,
         _ForwardIterator __result,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid = std::__uninitialized_copy_a(__first1, __last1,
          __result, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_move_a(__first2, __last2, __mid, __alloc);
      __guard.release();
      return __result;
    }





  template<typename _InputIterator1, typename _InputIterator2,
    typename _ForwardIterator, typename _Allocator>
    inline _ForwardIterator
    __uninitialized_move_copy(_InputIterator1 __first1,
         _InputIterator1 __last1,
         _InputIterator2 __first2,
         _InputIterator2 __last2,
         _ForwardIterator __result,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid = std::__uninitialized_move_a(__first1, __last1,
          __result, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_copy_a(__first2, __last2, __mid, __alloc);
      __guard.release();
      return __result;
    }




  template<typename _ForwardIterator, typename _Tp, typename _InputIterator,
    typename _Allocator>
    inline _ForwardIterator
    __uninitialized_fill_move(_ForwardIterator __result, _ForwardIterator __mid,
         const _Tp& __x, _InputIterator __first,
         _InputIterator __last, _Allocator& __alloc)
    {
      std::__uninitialized_fill_a(__result, __mid, __x, __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__result,
        __alloc);
      __result = __mid;
      __result = std::__uninitialized_move_a(__first, __last, __mid, __alloc);
      __guard.release();
      return __result;
    }




  template<typename _InputIterator, typename _ForwardIterator, typename _Tp,
    typename _Allocator>
    inline void
    __uninitialized_move_fill(_InputIterator __first1, _InputIterator __last1,
         _ForwardIterator __first2,
         _ForwardIterator __last2, const _Tp& __x,
         _Allocator& __alloc)
    {
      _ForwardIterator __mid2 = std::__uninitialized_move_a(__first1, __last1,
           __first2,
           __alloc);
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first2,
        __alloc);
      __first2 = __mid2;
      std::__uninitialized_fill_a(__mid2, __last2, __x, __alloc);
      __guard.release();
    }
# 814 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<bool _TrivialValueType>
    struct __uninitialized_default_1
    {
      template<typename _ForwardIterator>
        static void
        __uninit_default(_ForwardIterator __first, _ForwardIterator __last)
        {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __first != __last; ++__first)
     std::_Construct(std::__addressof(*__first));
   __guard.release();
 }
    };

  template<>
    struct __uninitialized_default_1<true>
    {
      template<typename _ForwardIterator>
        static void
        __uninit_default(_ForwardIterator __first, _ForwardIterator __last)
        {
   if (__first == __last)
     return;

   typename iterator_traits<_ForwardIterator>::value_type* __val
     = std::__addressof(*__first);
   std::_Construct(__val);
   if (++__first != __last)
     std::fill(__first, __last, *__val);
 }
    };

  template<bool _TrivialValueType>
    struct __uninitialized_default_n_1
    {
      template<typename _ForwardIterator, typename _Size>

        static _ForwardIterator
        __uninit_default_n(_ForwardIterator __first, _Size __n)
        {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __n > 0; --__n, (void) ++__first)
     std::_Construct(std::__addressof(*__first));
   __guard.release();
   return __first;
 }
    };

  template<>
    struct __uninitialized_default_n_1<true>
    {
      template<typename _ForwardIterator, typename _Size>

        static _ForwardIterator
        __uninit_default_n(_ForwardIterator __first, _Size __n)
        {
   if (__n > 0)
     {
       typename iterator_traits<_ForwardIterator>::value_type* __val
  = std::__addressof(*__first);
       std::_Construct(__val);
       ++__first;
       __first = std::fill_n(__first, __n - 1, *__val);
     }
   return __first;
 }
    };



  template<typename _ForwardIterator>
    inline void
    __uninitialized_default(_ForwardIterator __first,
       _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      const bool __assignable = is_copy_assignable<_ValueType>::value;

      std::__uninitialized_default_1<__is_trivial(_ValueType)
         && __assignable>::
 __uninit_default(__first, __last);
    }



  template<typename _ForwardIterator, typename _Size>
   
    inline _ForwardIterator
    __uninitialized_default_n(_ForwardIterator __first, _Size __n)
    {






      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      constexpr bool __can_fill
 = __and_<is_integral<_Size>, is_copy_assignable<_ValueType>>::value;

      return __uninitialized_default_n_1<__is_trivial(_ValueType)
      && __can_fill>::
 __uninit_default_n(__first, __n);
    }





  template<typename _ForwardIterator, typename _Allocator>
    void
    __uninitialized_default_a(_ForwardIterator __first,
         _ForwardIterator __last,
         _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first,
        __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __first != __last; ++__first)
 __traits::construct(__alloc, std::__addressof(*__first));
      __guard.release();
    }


  template<typename _ForwardIterator, typename _Tp>
    inline void
    __uninitialized_default_a(_ForwardIterator __first,
         _ForwardIterator __last,
         allocator<_Tp>&)
    { std::__uninitialized_default(__first, __last); }





  template<typename _ForwardIterator, typename _Size, typename _Allocator>
    _ForwardIterator
    __uninitialized_default_n_a(_ForwardIterator __first, _Size __n,
    _Allocator& __alloc)
    {
      _UninitDestroyGuard<_ForwardIterator, _Allocator> __guard(__first,
        __alloc);
      typedef __gnu_cxx::__alloc_traits<_Allocator> __traits;
      for (; __n > 0; --__n, (void) ++__first)
 __traits::construct(__alloc, std::__addressof(*__first));
      __guard.release();
      return __first;
    }




  template<typename _ForwardIterator, typename _Size, typename _Tp>
   
    inline _ForwardIterator
    __uninitialized_default_n_a(_ForwardIterator __first, _Size __n,
    allocator<_Tp>&)
    { return std::__uninitialized_default_n(__first, __n); }


  template<bool _TrivialValueType>
    struct __uninitialized_default_novalue_1
    {
      template<typename _ForwardIterator>
 static void
 __uninit_default_novalue(_ForwardIterator __first,
     _ForwardIterator __last)
 {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __first != __last; ++__first)
     std::_Construct_novalue(std::__addressof(*__first));
   __guard.release();
 }
    };

  template<>
    struct __uninitialized_default_novalue_1<true>
    {
      template<typename _ForwardIterator>
        static void
        __uninit_default_novalue(_ForwardIterator, _ForwardIterator)
 {
 }
    };

  template<bool _TrivialValueType>
    struct __uninitialized_default_novalue_n_1
    {
      template<typename _ForwardIterator, typename _Size>
 static _ForwardIterator
 __uninit_default_novalue_n(_ForwardIterator __first, _Size __n)
 {
   _UninitDestroyGuard<_ForwardIterator> __guard(__first);
   for (; __n > 0; --__n, (void) ++__first)
     std::_Construct_novalue(std::__addressof(*__first));
   __guard.release();
   return __first;
 }
    };

  template<>
    struct __uninitialized_default_novalue_n_1<true>
    {
      template<typename _ForwardIterator, typename _Size>
 static _ForwardIterator
 __uninit_default_novalue_n(_ForwardIterator __first, _Size __n)
 { return std::next(__first, __n); }
    };



  template<typename _ForwardIterator>
    inline void
    __uninitialized_default_novalue(_ForwardIterator __first,
        _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      std::__uninitialized_default_novalue_1<
 is_trivially_default_constructible<_ValueType>::value>::
 __uninit_default_novalue(__first, __last);
    }



  template<typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    __uninitialized_default_novalue_n(_ForwardIterator __first, _Size __n)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;

      return __uninitialized_default_novalue_n_1<
 is_trivially_default_constructible<_ValueType>::value>::
 __uninit_default_novalue_n(__first, __n);
    }

  template<typename _InputIterator, typename _Size,
    typename _ForwardIterator>
    _ForwardIterator
    __uninitialized_copy_n(_InputIterator __first, _Size __n,
      _ForwardIterator __result, input_iterator_tag)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __n > 0; --__n, (void) ++__first, ++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return __result;
    }

  template<typename _RandomAccessIterator, typename _Size,
    typename _ForwardIterator>
    inline _ForwardIterator
    __uninitialized_copy_n(_RandomAccessIterator __first, _Size __n,
      _ForwardIterator __result,
      random_access_iterator_tag)
    { return std::uninitialized_copy(__first, __first + __n, __result); }

  template<typename _InputIterator, typename _Size,
    typename _ForwardIterator>
    pair<_InputIterator, _ForwardIterator>
    __uninitialized_copy_n_pair(_InputIterator __first, _Size __n,
    _ForwardIterator __result, input_iterator_tag)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__result);
      for (; __n > 0; --__n, (void) ++__first, ++__result)
 std::_Construct(std::__addressof(*__result), *__first);
      __guard.release();
      return {__first, __result};
    }

  template<typename _RandomAccessIterator, typename _Size,
    typename _ForwardIterator>
    inline pair<_RandomAccessIterator, _ForwardIterator>
    __uninitialized_copy_n_pair(_RandomAccessIterator __first, _Size __n,
      _ForwardIterator __result,
      random_access_iterator_tag)
    {
      auto __second_res = uninitialized_copy(__first, __first + __n, __result);
      auto __first_res = std::next(__first, __n);
      return {__first_res, __second_res};
    }
# 1114 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _Size, typename _ForwardIterator>
    inline _ForwardIterator
    uninitialized_copy_n(_InputIterator __first, _Size __n,
    _ForwardIterator __result)
    { return std::__uninitialized_copy_n(__first, __n, __result,
      std::__iterator_category(__first)); }


  template<typename _InputIterator, typename _Size, typename _ForwardIterator>
    inline pair<_InputIterator, _ForwardIterator>
    __uninitialized_copy_n_pair(_InputIterator __first, _Size __n,
         _ForwardIterator __result)
    {
      return
 std::__uninitialized_copy_n_pair(__first, __n, __result,
      std::__iterator_category(__first));
    }
# 1232 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _Tp, typename _Up, typename _Allocator>
   
    inline void
    __relocate_object_a(_Tp* __restrict __dest, _Up* __restrict __orig,
   _Allocator& __alloc)
    noexcept(noexcept(std::allocator_traits<_Allocator>::construct(__alloc,
    __dest, std::move(*__orig)))
      && noexcept(std::allocator_traits<_Allocator>::destroy(
       __alloc, std::__addressof(*__orig))))
    {
      typedef std::allocator_traits<_Allocator> __traits;
      __traits::construct(__alloc, __dest, std::move(*__orig));
      __traits::destroy(__alloc, std::__addressof(*__orig));
    }



  template<typename _Tp, typename = void>
    struct __is_bitwise_relocatable
    : __bool_constant<__is_trivial(_Tp)>
    { };

  template <typename _InputIterator, typename _ForwardIterator,
     typename _Allocator>
   
    inline _ForwardIterator
    __relocate_a_1(_InputIterator __first, _InputIterator __last,
     _ForwardIterator __result, _Allocator& __alloc)
    noexcept(noexcept(std::__relocate_object_a(std::addressof(*__result),
            std::addressof(*__first),
            __alloc)))
    {
      typedef typename iterator_traits<_InputIterator>::value_type
 _ValueType;
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType2;
      static_assert(std::is_same<_ValueType, _ValueType2>::value,
   "relocation is only possible for values of the same type");
      _ForwardIterator __cur = __result;
      for (; __first != __last; ++__first, (void)++__cur)
 std::__relocate_object_a(std::__addressof(*__cur),
     std::__addressof(*__first), __alloc);
      return __cur;
    }


  template <typename _Tp, typename _Up>
   
    inline __enable_if_t<std::__is_bitwise_relocatable<_Tp>::value, _Tp*>
    __relocate_a_1(_Tp* __first, _Tp* __last,
     _Tp* __result,
     [[__maybe_unused__]] allocator<_Up>& __alloc) noexcept
    {
      ptrdiff_t __count = __last - __first;
      if (__count > 0)
 {
# 1298 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
   __builtin_memcpy(__result, __first, __count * sizeof(_Tp));
 }
      return __result + __count;
    }


  template <typename _InputIterator, typename _ForwardIterator,
     typename _Allocator>
   
    inline _ForwardIterator
    __relocate_a(_InputIterator __first, _InputIterator __last,
   _ForwardIterator __result, _Allocator& __alloc)
    noexcept(noexcept(__relocate_a_1(std::__niter_base(__first),
         std::__niter_base(__last),
         std::__niter_base(__result), __alloc)))
    {
      return std::__relocate_a_1(std::__niter_base(__first),
     std::__niter_base(__last),
     std::__niter_base(__result), __alloc);
    }







}
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 1 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/initializer_list" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/initializer_list" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

  template<class _E>
    class initializer_list
    {
    public:
      typedef _E value_type;
      typedef const _E& reference;
      typedef const _E& const_reference;
      typedef size_t size_type;
      typedef const _E* iterator;
      typedef const _E* const_iterator;

    private:
      iterator _M_array;
      size_type _M_len;


      constexpr initializer_list(const_iterator __a, size_type __l)
      : _M_array(__a), _M_len(__l) { }

    public:
      constexpr initializer_list() noexcept
      : _M_array(0), _M_len(0) { }


      constexpr size_type
      size() const noexcept { return _M_len; }


      constexpr const_iterator
      begin() const noexcept { return _M_array; }


      constexpr const_iterator
      end() const noexcept { return begin() + size(); }
    };







  template<class _Tp>
    constexpr const _Tp*
    begin(initializer_list<_Tp> __ils) noexcept
    { return __ils.begin(); }







  template<class _Tp>
    constexpr const _Tp*
    end(initializer_list<_Tp> __ils) noexcept
    { return __ils.end(); }
}
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 2 3
# 81 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  template<typename _Tp, typename _Alloc>
    struct _Vector_base
    {
      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
 rebind<_Tp>::other _Tp_alloc_type;
      typedef typename __gnu_cxx::__alloc_traits<_Tp_alloc_type>::pointer
        pointer;

      struct _Vector_impl_data
      {
 pointer _M_start;
 pointer _M_finish;
 pointer _M_end_of_storage;


 _Vector_impl_data() noexcept
 : _M_start(), _M_finish(), _M_end_of_storage()
 { }



 _Vector_impl_data(_Vector_impl_data&& __x) noexcept
 : _M_start(__x._M_start), _M_finish(__x._M_finish),
   _M_end_of_storage(__x._M_end_of_storage)
 { __x._M_start = __x._M_finish = __x._M_end_of_storage = pointer(); }



 void
 _M_copy_data(_Vector_impl_data const& __x) noexcept
 {
   _M_start = __x._M_start;
   _M_finish = __x._M_finish;
   _M_end_of_storage = __x._M_end_of_storage;
 }


 void
 _M_swap_data(_Vector_impl_data& __x) noexcept
 {


   _Vector_impl_data __tmp;
   __tmp._M_copy_data(*this);
   _M_copy_data(__x);
   __x._M_copy_data(__tmp);
 }
      };

      struct _Vector_impl
 : public _Tp_alloc_type, public _Vector_impl_data
      {

 _Vector_impl() noexcept(is_nothrow_default_constructible<_Tp_alloc_type>::value)




 : _Tp_alloc_type()
 { }


 _Vector_impl(_Tp_alloc_type const& __a) noexcept
 : _Tp_alloc_type(__a)
 { }





 _Vector_impl(_Vector_impl&& __x) noexcept
 : _Tp_alloc_type(std::move(__x)), _Vector_impl_data(std::move(__x))
 { }


 _Vector_impl(_Tp_alloc_type&& __a) noexcept
 : _Tp_alloc_type(std::move(__a))
 { }


 _Vector_impl(_Tp_alloc_type&& __a, _Vector_impl&& __rv) noexcept
 : _Tp_alloc_type(std::move(__a)), _Vector_impl_data(std::move(__rv))
 { }
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      };

    public:
      typedef _Alloc allocator_type;

     
      _Tp_alloc_type&
      _M_get_Tp_allocator() noexcept
      { return this->_M_impl; }

     
      const _Tp_alloc_type&
      _M_get_Tp_allocator() const noexcept
      { return this->_M_impl; }

     
      allocator_type
      get_allocator() const noexcept
      { return allocator_type(_M_get_Tp_allocator()); }


      _Vector_base() = default;




     
      _Vector_base(const allocator_type& __a) noexcept
      : _M_impl(__a) { }



     
      _Vector_base(size_t __n)
      : _M_impl()
      { _M_create_storage(__n); }


     
      _Vector_base(size_t __n, const allocator_type& __a)
      : _M_impl(__a)
      { _M_create_storage(__n); }


      _Vector_base(_Vector_base&&) = default;



     
      _Vector_base(_Tp_alloc_type&& __a) noexcept
      : _M_impl(std::move(__a)) { }

     
      _Vector_base(_Vector_base&& __x, const allocator_type& __a)
      : _M_impl(__a)
      {
 if (__x.get_allocator() == __a)
   this->_M_impl._M_swap_data(__x._M_impl);
 else
   {
     size_t __n = __x._M_impl._M_finish - __x._M_impl._M_start;
     _M_create_storage(__n);
   }
      }


     
      _Vector_base(const allocator_type& __a, _Vector_base&& __x)
      : _M_impl(_Tp_alloc_type(__a), std::move(__x._M_impl))
      { }


     
      ~_Vector_base() noexcept
      {
 _M_deallocate(_M_impl._M_start,
        _M_impl._M_end_of_storage - _M_impl._M_start);
      }

    public:
      _Vector_impl _M_impl;

     
      pointer
      _M_allocate(size_t __n)
      {
 typedef __gnu_cxx::__alloc_traits<_Tp_alloc_type> _Tr;
 return __n != 0 ? _Tr::allocate(_M_impl, __n) : pointer();
      }

     
      void
      _M_deallocate(pointer __p, size_t __n)
      {
 typedef __gnu_cxx::__alloc_traits<_Tp_alloc_type> _Tr;
 if (__p)
   _Tr::deallocate(_M_impl, __p, __n);
      }

    protected:

     
      void
      _M_create_storage(size_t __n)
      {
 this->_M_impl._M_start = this->_M_allocate(__n);
 this->_M_impl._M_finish = this->_M_impl._M_start;
 this->_M_impl._M_end_of_storage = this->_M_impl._M_start + __n;
      }
# 429 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
    };
# 454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
  template<typename _Tp, typename _Alloc = std::allocator<_Tp> >
    class vector : protected _Vector_base<_Tp, _Alloc>
    {
# 467 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      static_assert(is_same<typename remove_cv<_Tp>::type, _Tp>::value,
   "std::vector must have a non-const, non-volatile value_type");

      static_assert(is_same<typename _Alloc::value_type, _Tp>::value,
   "std::vector must have the same value_type as its allocator");



      typedef _Vector_base<_Tp, _Alloc> _Base;
      typedef typename _Base::_Tp_alloc_type _Tp_alloc_type;
      typedef __gnu_cxx::__alloc_traits<_Tp_alloc_type> _Alloc_traits;

    public:
      typedef _Tp value_type;
      typedef typename _Base::pointer pointer;
      typedef typename _Alloc_traits::const_pointer const_pointer;
      typedef typename _Alloc_traits::reference reference;
      typedef typename _Alloc_traits::const_reference const_reference;
      typedef __gnu_cxx::__normal_iterator<pointer, vector> iterator;
      typedef __gnu_cxx::__normal_iterator<const_pointer, vector>
      const_iterator;
      typedef std::reverse_iterator<const_iterator> const_reverse_iterator;
      typedef std::reverse_iterator<iterator> reverse_iterator;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;
      typedef _Alloc allocator_type;

    private:

      static constexpr bool
      _S_nothrow_relocate(true_type)
      {
 return noexcept(std::__relocate_a(std::declval<pointer>(),
       std::declval<pointer>(),
       std::declval<pointer>(),
       std::declval<_Tp_alloc_type&>()));
      }

      static constexpr bool
      _S_nothrow_relocate(false_type)
      { return false; }

      static constexpr bool
      _S_use_relocate()
      {



 return _S_nothrow_relocate(__is_move_insertable<_Tp_alloc_type>{});
      }

      static pointer
      _S_do_relocate(pointer __first, pointer __last, pointer __result,
       _Tp_alloc_type& __alloc, true_type) noexcept
      {
 return std::__relocate_a(__first, __last, __result, __alloc);
      }

      static pointer
      _S_do_relocate(pointer, pointer, pointer __result,
       _Tp_alloc_type&, false_type) noexcept
      { return __result; }

      static pointer
      _S_relocate(pointer __first, pointer __last, pointer __result,
    _Tp_alloc_type& __alloc) noexcept
      {




 using __do_it = __bool_constant<_S_use_relocate()>;
 return _S_do_relocate(__first, __last, __result, __alloc, __do_it{});

      }


    protected:
      using _Base::_M_allocate;
      using _Base::_M_deallocate;
      using _Base::_M_impl;
      using _Base::_M_get_Tp_allocator;

    public:







      vector() = default;
# 567 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      explicit
     
      vector(const allocator_type& __a) noexcept
      : _Base(__a) { }
# 581 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      explicit
     
      vector(size_type __n, const allocator_type& __a = allocator_type())
      : _Base(_S_check_init_len(__n, __a), __a)
      { _M_default_initialize(__n); }
# 595 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector(size_type __n, const value_type& __value,
      const allocator_type& __a = allocator_type())
      : _Base(_S_check_init_len(__n, __a), __a)
      { _M_fill_initialize(__n, __value); }
# 627 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector(const vector& __x)
      : _Base(__x.size(),
 _Alloc_traits::_S_select_on_copy(__x._M_get_Tp_allocator()))
      {
 this->_M_impl._M_finish =
   std::__uninitialized_copy_a(__x.begin(), __x.end(),
          this->_M_impl._M_start,
          _M_get_Tp_allocator());
      }
# 647 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      vector(vector&&) noexcept = default;


     
      vector(const vector& __x, const __type_identity_t<allocator_type>& __a)
      : _Base(__x.size(), __a)
      {
 this->_M_impl._M_finish =
   std::__uninitialized_copy_a(__x.begin(), __x.end(),
          this->_M_impl._M_start,
          _M_get_Tp_allocator());
      }

    private:
     
      vector(vector&& __rv, const allocator_type& __m, true_type) noexcept
      : _Base(__m, std::move(__rv))
      { }

     
      vector(vector&& __rv, const allocator_type& __m, false_type)
      : _Base(__m)
      {
 if (__rv.get_allocator() == __m)
   this->_M_impl._M_swap_data(__rv._M_impl);
 else if (!__rv.empty())
   {
     this->_M_create_storage(__rv.size());
     this->_M_impl._M_finish =
       std::__uninitialized_move_a(__rv.begin(), __rv.end(),
       this->_M_impl._M_start,
       _M_get_Tp_allocator());
     __rv.clear();
   }
      }

    public:

     
      vector(vector&& __rv, const __type_identity_t<allocator_type>& __m)
      noexcept( noexcept(
 vector(std::declval<vector&&>(), std::declval<const allocator_type&>(),
        std::declval<typename _Alloc_traits::is_always_equal>())) )
      : vector(std::move(__rv), __m, typename _Alloc_traits::is_always_equal{})
      { }
# 704 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector(initializer_list<value_type> __l,
      const allocator_type& __a = allocator_type())
      : _Base(__a)
      {
 _M_range_initialize(__l.begin(), __l.end(),
       random_access_iterator_tag());
      }
# 731 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator,
        typename = std::_RequireInputIter<_InputIterator>>

 vector(_InputIterator __first, _InputIterator __last,
        const allocator_type& __a = allocator_type())
 : _Base(__a)
 {
   _M_range_initialize(__first, __last,
         std::__iterator_category(__first));
 }
# 800 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      ~vector() noexcept
      {
 std::_Destroy(this->_M_impl._M_start, this->_M_impl._M_finish,
        _M_get_Tp_allocator());
 ;
      }
# 817 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector&
      operator=(const vector& __x);
# 832 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector&
      operator=(vector&& __x) noexcept(_Alloc_traits::_S_nothrow_move())
      {
 constexpr bool __move_storage =
   _Alloc_traits::_S_propagate_on_move_assign()
   || _Alloc_traits::_S_always_equal();
 _M_move_assign(std::move(__x), __bool_constant<__move_storage>());
 return *this;
      }
# 854 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector&
      operator=(initializer_list<value_type> __l)
      {
 this->_M_assign_aux(__l.begin(), __l.end(),
       random_access_iterator_tag());
 return *this;
      }
# 874 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      assign(size_type __n, const value_type& __val)
      { _M_fill_assign(__n, __val); }
# 892 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator,
        typename = std::_RequireInputIter<_InputIterator>>

 void
 assign(_InputIterator __first, _InputIterator __last)
 { _M_assign_aux(__first, __last, std::__iterator_category(__first)); }
# 921 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      assign(initializer_list<value_type> __l)
      {
 this->_M_assign_aux(__l.begin(), __l.end(),
       random_access_iterator_tag());
      }
# 987 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      using _Base::get_allocator;







      [[__nodiscard__]]
      iterator
      begin() noexcept
      { return iterator(this->_M_impl._M_start); }






      [[__nodiscard__]]
      const_iterator
      begin() const noexcept
      { return const_iterator(this->_M_impl._M_start); }






      [[__nodiscard__]]
      iterator
      end() noexcept
      { return iterator(this->_M_impl._M_finish); }






      [[__nodiscard__]]
      const_iterator
      end() const noexcept
      { return const_iterator(this->_M_impl._M_finish); }






      [[__nodiscard__]]
      reverse_iterator
      rbegin() noexcept
      { return reverse_iterator(end()); }






      [[__nodiscard__]]
      const_reverse_iterator
      rbegin() const noexcept
      { return const_reverse_iterator(end()); }






      [[__nodiscard__]]
      reverse_iterator
      rend() noexcept
      { return reverse_iterator(begin()); }






      [[__nodiscard__]]
      const_reverse_iterator
      rend() const noexcept
      { return const_reverse_iterator(begin()); }







      [[__nodiscard__]]
      const_iterator
      cbegin() const noexcept
      { return const_iterator(this->_M_impl._M_start); }






      [[__nodiscard__]]
      const_iterator
      cend() const noexcept
      { return const_iterator(this->_M_impl._M_finish); }






      [[__nodiscard__]]
      const_reverse_iterator
      crbegin() const noexcept
      { return const_reverse_iterator(end()); }






      [[__nodiscard__]]
      const_reverse_iterator
      crend() const noexcept
      { return const_reverse_iterator(begin()); }




      [[__nodiscard__]]
      size_type
      size() const noexcept
      {
 ptrdiff_t __dif = this->_M_impl._M_finish - this->_M_impl._M_start;
 if (__dif < 0)
    __builtin_unreachable ();
 return size_type(__dif);
      }


      [[__nodiscard__]]
      size_type
      max_size() const noexcept
      { return _S_max_size(_M_get_Tp_allocator()); }
# 1140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      resize(size_type __new_size)
      {
 if (__new_size > size())
   _M_default_append(__new_size - size());
 else if (__new_size < size())
   _M_erase_at_end(this->_M_impl._M_start + __new_size);
      }
# 1161 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      resize(size_type __new_size, const value_type& __x)
      {
 if (__new_size > size())
   _M_fill_insert(end(), __new_size - size(), __x);
 else if (__new_size < size())
   _M_erase_at_end(this->_M_impl._M_start + __new_size);
      }
# 1195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      shrink_to_fit()
      { _M_shrink_to_fit(); }






      [[__nodiscard__]]
      size_type
      capacity() const noexcept
      {
 ptrdiff_t __dif = this->_M_impl._M_end_of_storage
     - this->_M_impl._M_start;
 if (__dif < 0)
    __builtin_unreachable ();
 return size_type(__dif);
      }





      [[__nodiscard__]]
      bool
      empty() const noexcept
      { return begin() == end(); }
# 1242 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      reserve(size_type __n);
# 1258 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      [[__nodiscard__]]
      reference
      operator[](size_type __n) noexcept
      {
 do { if (__builtin_expect(!bool(__n < this->size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1262, __PRETTY_FUNCTION__, "__n < this->size()"); } while (false);
 return *(this->_M_impl._M_start + __n);
      }
# 1277 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      [[__nodiscard__]]
      const_reference
      operator[](size_type __n) const noexcept
      {
 do { if (__builtin_expect(!bool(__n < this->size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1281, __PRETTY_FUNCTION__, "__n < this->size()"); } while (false);
 return *(this->_M_impl._M_start + __n);
      }

    protected:

     
      void
      _M_range_check(size_type __n) const
      {
 if (__n >= this->size())
   __throw_out_of_range_fmt(("vector::_M_range_check: __n " "(which is %zu) >= this->size() " "(which is %zu)")

                            ,
       __n, this->size());
      }

    public:
# 1310 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      [[__nodiscard__]]
      reference
      at(size_type __n)
      {
 _M_range_check(__n);
 return (*this)[__n];
      }
# 1329 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      [[__nodiscard__]]
      const_reference
      at(size_type __n) const
      {
 _M_range_check(__n);
 return (*this)[__n];
      }





      [[__nodiscard__]]
      reference
      front() noexcept
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1345, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }





      [[__nodiscard__]]
      const_reference
      front() const noexcept
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1357, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }





      [[__nodiscard__]]
      reference
      back() noexcept
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1369, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }





      [[__nodiscard__]]
      const_reference
      back() const noexcept
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1381, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }
# 1392 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      [[__nodiscard__]]
      _Tp*
      data() noexcept
      { return _M_data_ptr(this->_M_impl._M_start); }

      [[__nodiscard__]]
      const _Tp*
      data() const noexcept
      { return _M_data_ptr(this->_M_impl._M_start); }
# 1413 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      push_back(const value_type& __x)
      {
 if (this->_M_impl._M_finish != this->_M_impl._M_end_of_storage)
   {
     ;
     _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
         __x);
     ++this->_M_impl._M_finish;
     ;
   }
 else
   _M_realloc_append(__x);
      }


     
      void
      push_back(value_type&& __x)
      { emplace_back(std::move(__x)); }

      template<typename... _Args>




 void

 emplace_back(_Args&&... __args);
# 1454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      pop_back() noexcept
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1458, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 --this->_M_impl._M_finish;
 _Alloc_traits::destroy(this->_M_impl, this->_M_impl._M_finish);
 ;
      }
# 1477 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename... _Args>

 iterator
 emplace(const_iterator __position, _Args&&... __args)
 { return _M_emplace_aux(__position, std::forward<_Args>(__args)...); }
# 1494 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator
      insert(const_iterator __position, const value_type& __x);
# 1525 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator
      insert(const_iterator __position, value_type&& __x)
      { return _M_insert_rval(__position, std::move(__x)); }
# 1543 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator
      insert(const_iterator __position, initializer_list<value_type> __l)
      {
 auto __offset = __position - cbegin();
 _M_range_insert(begin() + __offset, __l.begin(), __l.end(),
   std::random_access_iterator_tag());
 return begin() + __offset;
      }
# 1569 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator
      insert(const_iterator __position, size_type __n, const value_type& __x)
      {
 difference_type __offset = __position - cbegin();
 _M_fill_insert(begin() + __offset, __n, __x);
 return begin() + __offset;
      }
# 1612 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator,
        typename = std::_RequireInputIter<_InputIterator>>

 iterator
 insert(const_iterator __position, _InputIterator __first,
        _InputIterator __last)
 {
   difference_type __offset = __position - cbegin();
   _M_range_insert(begin() + __offset, __first, __last,
     std::__iterator_category(__first));
   return begin() + __offset;
 }
# 1700 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator

      erase(const_iterator __position)
      { return _M_erase(begin() + (__position - cbegin())); }
# 1728 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator

      erase(const_iterator __first, const_iterator __last)
      {
 const auto __beg = begin();
 const auto __cbeg = cbegin();
 return _M_erase(__beg + (__first - __cbeg), __beg + (__last - __cbeg));
      }
# 1753 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      swap(vector& __x) noexcept
      {

 do { if (__builtin_expect(!bool(_Alloc_traits::propagate_on_container_swap::value || _M_get_Tp_allocator() == __x._M_get_Tp_allocator()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1758, __PRETTY_FUNCTION__, "_Alloc_traits::propagate_on_container_swap::value || _M_get_Tp_allocator() == __x._M_get_Tp_allocator()"); } while (false)
                                                          ;

 this->_M_impl._M_swap_data(__x._M_impl);
 _Alloc_traits::_S_on_swap(_M_get_Tp_allocator(),
      __x._M_get_Tp_allocator());
      }







     
      void
      clear() noexcept
      { _M_erase_at_end(this->_M_impl._M_start); }

    private:

      struct _Guard_alloc
      {
 pointer _M_storage;
 size_type _M_len;
 _Base& _M_vect;


 _Guard_alloc(pointer __s, size_type __l, _Base& __vect)
 : _M_storage(__s), _M_len(__l), _M_vect(__vect)
 { }


 ~_Guard_alloc()
 {
   if (_M_storage)
     _M_vect._M_deallocate(_M_storage, _M_len);
 }


 pointer
 _M_release()
 {
   pointer __res = _M_storage;
   _M_storage = pointer();
   return __res;
 }

      private:
 _Guard_alloc(const _Guard_alloc&);
      };

    protected:




      template<typename _ForwardIterator>

 pointer
 _M_allocate_and_copy(size_type __n,
        _ForwardIterator __first, _ForwardIterator __last)
 {
   _Guard_alloc __guard(this->_M_allocate(__n), __n, *this);
   std::__uninitialized_copy_a
     (__first, __last, __guard._M_storage, _M_get_Tp_allocator());
   return __guard._M_release();
 }
# 1859 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator>

 void
 _M_range_initialize(_InputIterator __first, _InputIterator __last,
       std::input_iterator_tag)
 {
   try {
     for (; __first != __last; ++__first)

       emplace_back(*__first);



   } catch(...) {
     clear();
     throw;
   }
 }


      template<typename _ForwardIterator>

 void
 _M_range_initialize(_ForwardIterator __first, _ForwardIterator __last,
       std::forward_iterator_tag)
 {
   const size_type __n = std::distance(__first, __last);
   pointer __start =
     this->_M_allocate(_S_check_init_len(__n, _M_get_Tp_allocator()));
   _Guard_alloc __guard(__start, __n, *this);
   this->_M_impl._M_finish = std::__uninitialized_copy_a
     (__first, __last, __start, _M_get_Tp_allocator());
   this->_M_impl._M_start = __start;
   (void) __guard._M_release();
   this->_M_impl._M_end_of_storage = __start + __n;
 }



     
      void
      _M_fill_initialize(size_type __n, const value_type& __value)
      {
 this->_M_impl._M_finish =
   std::__uninitialized_fill_n_a(this->_M_impl._M_start, __n, __value,
     _M_get_Tp_allocator());
      }



     
      void
      _M_default_initialize(size_type __n)
      {
 this->_M_impl._M_finish =
   std::__uninitialized_default_n_a(this->_M_impl._M_start, __n,
        _M_get_Tp_allocator());
      }
# 1926 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _Integer>

 void
 _M_assign_dispatch(_Integer __n, _Integer __val, __true_type)
 { _M_fill_assign(__n, __val); }


      template<typename _InputIterator>

 void
 _M_assign_dispatch(_InputIterator __first, _InputIterator __last,
      __false_type)
 { _M_assign_aux(__first, __last, std::__iterator_category(__first)); }


      template<typename _InputIterator>

 void
 _M_assign_aux(_InputIterator __first, _InputIterator __last,
        std::input_iterator_tag);


      template<typename _ForwardIterator>

 void
 _M_assign_aux(_ForwardIterator __first, _ForwardIterator __last,
        std::forward_iterator_tag);



     
      void
      _M_fill_assign(size_type __n, const value_type& __val);







      template<typename _Integer>

 void
 _M_insert_dispatch(iterator __pos, _Integer __n, _Integer __val,
      __true_type)
 { _M_fill_insert(__pos, __n, __val); }


      template<typename _InputIterator>

 void
 _M_insert_dispatch(iterator __pos, _InputIterator __first,
      _InputIterator __last, __false_type)
 {
   _M_range_insert(__pos, __first, __last,
     std::__iterator_category(__first));
 }


      template<typename _InputIterator>

 void
 _M_range_insert(iterator __pos, _InputIterator __first,
   _InputIterator __last, std::input_iterator_tag);


      template<typename _ForwardIterator>

 void
 _M_range_insert(iterator __pos, _ForwardIterator __first,
   _ForwardIterator __last, std::forward_iterator_tag);



     
      void
      _M_fill_insert(iterator __pos, size_type __n, const value_type& __x);



     
      void
      _M_default_append(size_type __n);

     
      bool
      _M_shrink_to_fit();
# 2028 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      struct _Temporary_value
      {
 template<typename... _Args>
   explicit
   _Temporary_value(vector* __vec, _Args&&... __args) : _M_this(__vec)
   {
     _Alloc_traits::construct(_M_this->_M_impl, _M_ptr(),
         std::forward<_Args>(__args)...);
   }


 ~_Temporary_value()
 { _Alloc_traits::destroy(_M_this->_M_impl, _M_ptr()); }

 value_type&
 _M_val() noexcept { return _M_storage._M_val; }

      private:
 _Tp*
 _M_ptr() noexcept { return std::__addressof(_M_storage._M_val); }

 union _Storage
 {
   constexpr _Storage() : _M_byte() { }
   ~_Storage() { }
   _Storage& operator=(const _Storage&) = delete;
   unsigned char _M_byte;
   _Tp _M_val;
 };

 vector* _M_this;
 _Storage _M_storage;
      };



      template<typename _Arg>

 void
 _M_insert_aux(iterator __position, _Arg&& __arg);

      template<typename... _Args>

 void
 _M_realloc_insert(iterator __position, _Args&&... __args);

      template<typename... _Args>

 void
 _M_realloc_append(_Args&&... __args);


     
      iterator
      _M_insert_rval(const_iterator __position, value_type&& __v);


      template<typename... _Args>

 iterator
 _M_emplace_aux(const_iterator __position, _Args&&... __args);


     
      iterator
      _M_emplace_aux(const_iterator __position, value_type&& __v)
      { return _M_insert_rval(__position, std::move(__v)); }



     
      size_type
      _M_check_len(size_type __n, const char* __s) const
      {
 if (max_size() - size() < __n)
   __throw_length_error((__s));

 const size_type __len = size() + (std::max)(size(), __n);
 return (__len < size() || __len > max_size()) ? max_size() : __len;
      }


      static size_type
      _S_check_init_len(size_type __n, const allocator_type& __a)
      {
 if (__n > _S_max_size(_Tp_alloc_type(__a)))
   __throw_length_error(
       ("cannot create std::vector larger than max_size()"));
 return __n;
      }

      static size_type
      _S_max_size(const _Tp_alloc_type& __a) noexcept
      {



 const size_t __diffmax
   = __gnu_cxx::__numeric_traits<ptrdiff_t>::__max / sizeof(_Tp);
 const size_t __allocmax = _Alloc_traits::max_size(__a);
 return (std::min)(__diffmax, __allocmax);
      }





     
      void
      _M_erase_at_end(pointer __pos) noexcept
      {
 if (size_type __n = this->_M_impl._M_finish - __pos)
   {
     std::_Destroy(__pos, this->_M_impl._M_finish,
     _M_get_Tp_allocator());
     this->_M_impl._M_finish = __pos;
     ;
   }
      }

     
      iterator
      _M_erase(iterator __position);

     
      iterator
      _M_erase(iterator __first, iterator __last);


    private:



     
      void
      _M_move_assign(vector&& __x, true_type) noexcept
      {
 vector __tmp(get_allocator());
 this->_M_impl._M_swap_data(__x._M_impl);
 __tmp._M_impl._M_swap_data(__x._M_impl);
 std::__alloc_on_move(_M_get_Tp_allocator(), __x._M_get_Tp_allocator());
      }



     
      void
      _M_move_assign(vector&& __x, false_type)
      {
 if (__x._M_get_Tp_allocator() == this->_M_get_Tp_allocator())
   _M_move_assign(std::move(__x), true_type());
 else
   {


     this->_M_assign_aux(std::make_move_iterator(__x.begin()),
           std::make_move_iterator(__x.end()),
    std::random_access_iterator_tag());
     __x.clear();
   }
      }


      template<typename _Up>

 _Up*
 _M_data_ptr(_Up* __ptr) const noexcept
 { return __ptr; }


      template<typename _Ptr>

 typename std::pointer_traits<_Ptr>::element_type*
 _M_data_ptr(_Ptr __ptr) const
 { return empty() ? nullptr : std::__to_address(__ptr); }






    };
# 2238 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
  template<typename _Tp, typename _Alloc>
    [[__nodiscard__]]
    inline bool
    operator==(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return (__x.size() == __y.size()
       && std::equal(__x.begin(), __x.end(), __y.begin())); }
# 2278 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
  template<typename _Tp, typename _Alloc>
    [[__nodiscard__]] inline bool
    operator<(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return std::lexicographical_compare(__x.begin(), __x.end(),
       __y.begin(), __y.end()); }


  template<typename _Tp, typename _Alloc>
    [[__nodiscard__]] inline bool
    operator!=(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return !(__x == __y); }


  template<typename _Tp, typename _Alloc>
    [[__nodiscard__]] inline bool
    operator>(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return __y < __x; }


  template<typename _Tp, typename _Alloc>
    [[__nodiscard__]] inline bool
    operator<=(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return !(__y < __x); }


  template<typename _Tp, typename _Alloc>
    [[__nodiscard__]] inline bool
    operator>=(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return !(__x < __y); }



  template<typename _Tp, typename _Alloc>
   
    inline void
    swap(vector<_Tp, _Alloc>& __x, vector<_Tp, _Alloc>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }


# 2333 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3

}
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 1 3
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/hash_bytes.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/hash_bytes.h" 3
namespace std
{







  size_t
  _Hash_bytes(const void* __ptr, size_t __len, size_t __seed);





  size_t
  _Fnv_hash_bytes(const void* __ptr, size_t __len, size_t __seed);


}
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<typename _Result, typename _Arg>
    struct __hash_base
    {

      typedef _Result result_type ;
      typedef _Arg argument_type ;

    };



  template<typename _Tp> struct __hash_empty_base { };



  template<typename _Tp>
    struct hash;

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++14-extensions"
  template<typename _Tp, typename = void>
    constexpr bool __is_hash_enabled_for = false;

  template<typename _Tp>
    constexpr bool
    __is_hash_enabled_for<_Tp,
     __void_t<decltype(hash<_Tp>()(declval<_Tp>()))>>
      = true;
#pragma GCC diagnostic pop


  template<typename _Tp>
    struct __hash_not_enabled
    {
      __hash_not_enabled(__hash_not_enabled&&) = delete;
      ~__hash_not_enabled() = delete;
    };


  template<typename _Tp, bool = true>
    struct __hash_enum : public __hash_base<size_t, _Tp>
    {
      size_t
      operator()(_Tp __val) const noexcept
      {
       using __type = typename underlying_type<_Tp>::type;
       return hash<__type>{}(static_cast<__type>(__val));
      }
    };


  template<typename _Tp>
    struct hash
    : __conditional_t<__is_enum(_Tp), __hash_enum<_Tp>, __hash_not_enabled<_Tp>>
    { };


  template<typename _Tp>
    struct hash<_Tp*> : public __hash_base<size_t, _Tp*>
    {
      size_t
      operator()(_Tp* __p) const noexcept
      { return reinterpret_cast<size_t>(__p); }
    };
# 128 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<> struct hash<bool> : public __hash_base<size_t, bool> { size_t operator()(bool __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<char> : public __hash_base<size_t, char> { size_t operator()(char __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<signed char> : public __hash_base<size_t, signed char> { size_t operator()(signed char __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned char> : public __hash_base<size_t, unsigned char> { size_t operator()(unsigned char __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<wchar_t> : public __hash_base<size_t, wchar_t> { size_t operator()(wchar_t __val) const noexcept { return static_cast<size_t>(__val); } };







  template<> struct hash<char16_t> : public __hash_base<size_t, char16_t> { size_t operator()(char16_t __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<char32_t> : public __hash_base<size_t, char32_t> { size_t operator()(char32_t __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<short> : public __hash_base<size_t, short> { size_t operator()(short __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<int> : public __hash_base<size_t, int> { size_t operator()(int __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<long> : public __hash_base<size_t, long> { size_t operator()(long __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<long long> : public __hash_base<size_t, long long> { size_t operator()(long long __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned short> : public __hash_base<size_t, unsigned short> { size_t operator()(unsigned short __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned int> : public __hash_base<size_t, unsigned int> { size_t operator()(unsigned int __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned long> : public __hash_base<size_t, unsigned long> { size_t operator()(unsigned long __val) const noexcept { return static_cast<size_t>(__val); } };


  template<> struct hash<unsigned long long> : public __hash_base<size_t, unsigned long long> { size_t operator()(unsigned long long __val) const noexcept { return static_cast<size_t>(__val); } };
# 204 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  struct _Hash_impl
  {
    static size_t
    hash(const void* __ptr, size_t __clength,
  size_t __seed = static_cast<size_t>(0xc70f6907UL))
    { return _Hash_bytes(__ptr, __clength, __seed); }

    template<typename _Tp>
      static size_t
      hash(const _Tp& __val)
      { return hash(&__val, sizeof(__val)); }

    template<typename _Tp>
      static size_t
      __hash_combine(const _Tp& __val, size_t __hash)
      { return hash(&__val, sizeof(__val), __hash); }
  };


  struct _Fnv_hash_impl
  {
    static size_t
    hash(const void* __ptr, size_t __clength,
  size_t __seed = static_cast<size_t>(2166136261UL))
    { return _Fnv_hash_bytes(__ptr, __clength, __seed); }

    template<typename _Tp>
      static size_t
      hash(const _Tp& __val)
      { return hash(&__val, sizeof(__val)); }

    template<typename _Tp>
      static size_t
      __hash_combine(const _Tp& __val, size_t __hash)
      { return hash(&__val, sizeof(__val), __hash); }
  };


  template<>
    struct hash<float> : public __hash_base<size_t, float>
    {
      size_t
      operator()(float __val) const noexcept
      {

 return __val != 0.0f ? std::_Hash_impl::hash(__val) : 0;
      }
    };


  template<>
    struct hash<double> : public __hash_base<size_t, double>
    {
      size_t
      operator()(double __val) const noexcept
      {

 return __val != 0.0 ? std::_Hash_impl::hash(__val) : 0;
      }
    };


  template<>
    struct hash<long double>
    : public __hash_base<size_t, long double>
    {
      __attribute__ ((__pure__)) size_t
      operator()(long double __val) const noexcept;
    };
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/functional_hash.h" 3
  template<typename _Hash>
    struct __is_fast_hash : public std::true_type
    { };

  template<>
    struct __is_fast_hash<hash<long double>> : public std::false_type
    { };


}
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{


  typedef unsigned long _Bit_type;
  enum { _S_word_bit = int(8 * sizeof(_Bit_type)) };

  __attribute__((__nonnull__))
 
  void
  __fill_bvector_n(_Bit_type*, size_t, bool) noexcept;



  struct _Bit_reference
  {
  private:
    template<typename, typename> friend class vector;
    friend struct _Bit_iterator;
    friend struct _Bit_const_iterator;

   
    _Bit_reference() noexcept : _M_p(0), _M_mask(0) { }

    _Bit_type * _M_p;
    _Bit_type _M_mask;

   
    _Bit_reference(_Bit_type * __x, _Bit_type __y)
    : _M_p(__x), _M_mask(__y) { }

  public:

    _Bit_reference(const _Bit_reference&) = default;


    [[__nodiscard__]]
    operator bool() const noexcept
    { return !!(*_M_p & _M_mask); }

   
    _Bit_reference&
    operator=(bool __x) noexcept
    {
      if (__x)
 *_M_p |= _M_mask;
      else
 *_M_p &= ~_M_mask;
      return *this;
    }
# 131 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
   
    _Bit_reference&
    operator=(const _Bit_reference& __x) noexcept
    { return *this = bool(__x); }

    [[__nodiscard__]]
    bool
    operator==(const _Bit_reference& __x) const
    { return bool(*this) == bool(__x); }

    [[__nodiscard__]]
    bool
    operator<(const _Bit_reference& __x) const
    { return !bool(*this) && bool(__x); }

   
    void
    flip() noexcept
    { *_M_p ^= _M_mask; }


   
    friend void
    swap(_Bit_reference __x, _Bit_reference __y) noexcept
    {
      bool __tmp = __x;
      __x = __y;
      __y = __tmp;
    }

   
    friend void
    swap(_Bit_reference __x, bool& __y) noexcept
    {
      bool __tmp = __x;
      __x = __y;
      __y = __tmp;
    }

   
    friend void
    swap(bool& __x, _Bit_reference __y) noexcept
    {
      bool __tmp = __x;
      __x = __y;
      __y = __tmp;
    }

  };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
  struct _Bit_iterator_base
  : public std::iterator<std::random_access_iterator_tag, bool>
  {
    _Bit_type * _M_p;
    unsigned int _M_offset;

    inline __attribute__((__always_inline__))
    void
    _M_assume_normalized() const
    {

      unsigned int __ofst = _M_offset;
      __attribute__ ((__assume__ (__ofst < unsigned(_S_word_bit))));

    }

   
    _Bit_iterator_base(_Bit_type * __x, unsigned int __y)
    : _M_p(__x), _M_offset(__y) { }

   
    void
    _M_bump_up()
    {
      _M_assume_normalized();
      if (_M_offset++ == int(_S_word_bit) - 1)
 {
   _M_offset = 0;
   ++_M_p;
 }
    }

   
    void
    _M_bump_down()
    {
      _M_assume_normalized();
      if (_M_offset-- == 0)
 {
   _M_offset = int(_S_word_bit) - 1;
   --_M_p;
 }
    }

   
    void
    _M_incr(ptrdiff_t __i)
    {
      _M_assume_normalized();
      difference_type __n = __i + _M_offset;
      _M_p += __n / int(_S_word_bit);
      __n = __n % int(_S_word_bit);
      if (__n < 0)
 {
   __n += int(_S_word_bit);
   --_M_p;
 }
      _M_offset = static_cast<unsigned int>(__n);
    }

    [[__nodiscard__]]
    friend bool
    operator==(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    {
      __x._M_assume_normalized();
      __y._M_assume_normalized();
      return __x._M_p == __y._M_p && __x._M_offset == __y._M_offset;
    }
# 266 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
    [[__nodiscard__]]
    friend bool
    operator<(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    {
      __x._M_assume_normalized();
      __y._M_assume_normalized();
      return __x._M_p < __y._M_p
     || (__x._M_p == __y._M_p && __x._M_offset < __y._M_offset);
    }

    [[__nodiscard__]]
    friend bool
    operator!=(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return !(__x == __y); }

    [[__nodiscard__]]
    friend bool
    operator>(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return __y < __x; }

    [[__nodiscard__]]
    friend bool
    operator<=(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return !(__y < __x); }

    [[__nodiscard__]]
    friend bool
    operator>=(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return !(__x < __y); }


    friend ptrdiff_t
    operator-(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    {
      __x._M_assume_normalized();
      __y._M_assume_normalized();
      return (int(_S_word_bit) * (__x._M_p - __y._M_p)
       + __x._M_offset - __y._M_offset);
    }
  };
#pragma GCC diagnostic pop

  struct _Bit_iterator : public _Bit_iterator_base
  {
    typedef _Bit_reference reference;



    typedef _Bit_reference* pointer;

    typedef _Bit_iterator iterator;

   
    _Bit_iterator() : _Bit_iterator_base(0, 0) { }

   
    _Bit_iterator(_Bit_type * __x, unsigned int __y)
    : _Bit_iterator_base(__x, __y) { }

   
    iterator
    _M_const_cast() const
    { return *this; }

    [[__nodiscard__]]
    reference
    operator*() const
    {
      _M_assume_normalized();
      return reference(_M_p, 1UL << _M_offset);
    }

   
    iterator&
    operator++()
    {
      _M_bump_up();
      return *this;
    }

   
    iterator
    operator++(int)
    {
      iterator __tmp = *this;
      _M_bump_up();
      return __tmp;
    }

   
    iterator&
    operator--()
    {
      _M_bump_down();
      return *this;
    }

   
    iterator
    operator--(int)
    {
      iterator __tmp = *this;
      _M_bump_down();
      return __tmp;
    }

   
    iterator&
    operator+=(difference_type __i)
    {
      _M_incr(__i);
      return *this;
    }

   
    iterator&
    operator-=(difference_type __i)
    {
      *this += -__i;
      return *this;
    }

    [[__nodiscard__]]
    reference
    operator[](difference_type __i) const
    { return *(*this + __i); }

    [[__nodiscard__]]
    friend iterator
    operator+(const iterator& __x, difference_type __n)
    {
      iterator __tmp = __x;
      __tmp += __n;
      return __tmp;
    }

    [[__nodiscard__]]
    friend iterator
    operator+(difference_type __n, const iterator& __x)
    { return __x + __n; }

    [[__nodiscard__]]
    friend iterator
    operator-(const iterator& __x, difference_type __n)
    {
      iterator __tmp = __x;
      __tmp -= __n;
      return __tmp;
    }
  };

  struct _Bit_const_iterator : public _Bit_iterator_base
  {
    typedef bool reference;
    typedef bool const_reference;



    typedef const bool* pointer;

    typedef _Bit_const_iterator const_iterator;

   
    _Bit_const_iterator() : _Bit_iterator_base(0, 0) { }

   
    _Bit_const_iterator(_Bit_type * __x, unsigned int __y)
    : _Bit_iterator_base(__x, __y) { }

   
    _Bit_const_iterator(const _Bit_iterator& __x)
    : _Bit_iterator_base(__x._M_p, __x._M_offset) { }

   
    _Bit_iterator
    _M_const_cast() const
    { return _Bit_iterator(_M_p, _M_offset); }

    [[__nodiscard__]]
    const_reference
    operator*() const
    {
      _M_assume_normalized();
      return _Bit_reference(_M_p, 1UL << _M_offset);
    }

   
    const_iterator&
    operator++()
    {
      _M_bump_up();
      return *this;
    }

   
    const_iterator
    operator++(int)
    {
      const_iterator __tmp = *this;
      _M_bump_up();
      return __tmp;
    }

   
    const_iterator&
    operator--()
    {
      _M_bump_down();
      return *this;
    }

   
    const_iterator
    operator--(int)
    {
      const_iterator __tmp = *this;
      _M_bump_down();
      return __tmp;
    }

   
    const_iterator&
    operator+=(difference_type __i)
    {
      _M_incr(__i);
      return *this;
    }

   
    const_iterator&
    operator-=(difference_type __i)
    {
      *this += -__i;
      return *this;
    }

    [[__nodiscard__]]
    const_reference
    operator[](difference_type __i) const
    { return *(*this + __i); }

    [[__nodiscard__]]
    friend const_iterator
    operator+(const const_iterator& __x, difference_type __n)
    {
      const_iterator __tmp = __x;
      __tmp += __n;
      return __tmp;
    }

    [[__nodiscard__]]
    friend const_iterator
    operator-(const const_iterator& __x, difference_type __n)
    {
      const_iterator __tmp = __x;
      __tmp -= __n;
      return __tmp;
    }

    [[__nodiscard__]]
    friend const_iterator
    operator+(difference_type __n, const const_iterator& __x)
    { return __x + __n; }
  };

  template<typename _Alloc>
    struct _Bvector_base
    {
      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
        rebind<_Bit_type>::other _Bit_alloc_type;
      typedef typename __gnu_cxx::__alloc_traits<_Bit_alloc_type>
 _Bit_alloc_traits;
      typedef typename _Bit_alloc_traits::pointer _Bit_pointer;

      struct _Bvector_impl_data
      {

 _Bit_iterator _M_start;
# 553 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
 _Bit_iterator _M_finish;
 _Bit_pointer _M_end_of_storage;


 _Bvector_impl_data() noexcept
 : _M_start(), _M_finish(), _M_end_of_storage()
 { }


 _Bvector_impl_data(const _Bvector_impl_data&) = default;

 _Bvector_impl_data&
 operator=(const _Bvector_impl_data&) = default;


 _Bvector_impl_data(_Bvector_impl_data&& __x) noexcept
 : _Bvector_impl_data(__x)
 { __x._M_reset(); }


 void
 _M_move_data(_Bvector_impl_data&& __x) noexcept
 {
   *this = __x;
   __x._M_reset();
 }



 void
 _M_reset() noexcept
 { *this = _Bvector_impl_data(); }


 void
 _M_swap_data(_Bvector_impl_data& __x) noexcept
 {


   std::swap(*this, __x);
 }
      };

      struct _Bvector_impl
 : public _Bit_alloc_type, public _Bvector_impl_data
      {

 _Bvector_impl() noexcept(is_nothrow_default_constructible<_Bit_alloc_type>::value)




 : _Bit_alloc_type()
 { }


 _Bvector_impl(const _Bit_alloc_type& __a) noexcept
 : _Bit_alloc_type(__a)
 { }





 _Bvector_impl(_Bvector_impl&& __x) noexcept
 : _Bit_alloc_type(std::move(__x)), _Bvector_impl_data(std::move(__x))
 { }


 _Bvector_impl(_Bit_alloc_type&& __a, _Bvector_impl&& __x) noexcept
 : _Bit_alloc_type(std::move(__a)), _Bvector_impl_data(std::move(__x))
 { }



 _Bit_type*
 _M_end_addr() const noexcept
 {
   if (this->_M_end_of_storage)
     return std::__addressof(this->_M_end_of_storage[-1]) + 1;
   return 0;
 }
      };

    public:
      typedef _Alloc allocator_type;

     
      _Bit_alloc_type&
      _M_get_Bit_allocator() noexcept
      { return this->_M_impl; }

     
      const _Bit_alloc_type&
      _M_get_Bit_allocator() const noexcept
      { return this->_M_impl; }

     
      allocator_type
      get_allocator() const noexcept
      { return allocator_type(_M_get_Bit_allocator()); }


      _Bvector_base() = default;




     
      _Bvector_base(const allocator_type& __a)
      : _M_impl(_Bit_alloc_type(__a)) { }


      _Bvector_base(_Bvector_base&&) = default;

     
      _Bvector_base(_Bvector_base&& __x, const allocator_type& __a) noexcept
      : _M_impl(_Bit_alloc_type(__a), std::move(__x._M_impl))
      { }


     
      ~_Bvector_base()
      { this->_M_deallocate(); }

    protected:
      _Bvector_impl _M_impl;

     
      _Bit_pointer
      _M_allocate(size_t __n)
      {
 _Bit_pointer __p = _Bit_alloc_traits::allocate(_M_impl, _S_nword(__n));
# 694 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
 return __p;
      }

     
      void
      _M_deallocate()
      {
 if (_M_impl._M_start._M_p)
   {
     const size_t __n = _M_impl._M_end_addr() - _M_impl._M_start._M_p;
     _Bit_alloc_traits::deallocate(_M_impl,
       _M_impl._M_end_of_storage - __n,
       __n);
     _M_impl._M_reset();
   }
      }


     
      void
      _M_move_data(_Bvector_base&& __x) noexcept
      { _M_impl._M_move_data(std::move(__x._M_impl)); }


      constexpr
      static size_t
      _S_nword(size_t __n)
      { return (__n + int(_S_word_bit) - 1) / int(_S_word_bit); }
    };
# 745 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
  template<typename _Alloc>
    class vector<bool, _Alloc> : protected _Bvector_base<_Alloc>
    {
      typedef _Bvector_base<_Alloc> _Base;
      typedef typename _Base::_Bit_pointer _Bit_pointer;
      typedef typename _Base::_Bit_alloc_traits _Bit_alloc_traits;


      friend struct std::hash<vector>;


    public:
      typedef bool value_type;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;
      typedef _Bit_reference reference;
      typedef bool const_reference;
      typedef _Bit_reference* pointer;
      typedef const bool* const_pointer;
      typedef _Bit_iterator iterator;
      typedef _Bit_const_iterator const_iterator;
      typedef std::reverse_iterator<const_iterator> const_reverse_iterator;
      typedef std::reverse_iterator<iterator> reverse_iterator;
      typedef _Alloc allocator_type;

     
      allocator_type
      get_allocator() const
      { return _Base::get_allocator(); }

    protected:
      using _Base::_M_allocate;
      using _Base::_M_deallocate;
      using _Base::_S_nword;
      using _Base::_M_get_Bit_allocator;

    public:

      vector() = default;




     
      explicit
      vector(const allocator_type& __a) noexcept
      : _Base(__a) { }


     
      explicit
      vector(size_type __n, const allocator_type& __a = allocator_type())
      : vector(__n, false, __a)
      { }

     
      vector(size_type __n, const bool& __value,
      const allocator_type& __a = allocator_type())





      : _Base(__a)
      {
 _M_initialize(__n);
 _M_initialize_value(__value);
      }

     
      vector(const vector& __x)
      : _Base(_Bit_alloc_traits::_S_select_on_copy(__x._M_get_Bit_allocator()))
      {
 const_iterator __xbegin = __x.begin(), __xend = __x.end();
 _M_initialize(__x.size());
 _M_copy_aligned(__xbegin, __xend, begin());
      }


      vector(vector&&) = default;

    private:
     
      vector(vector&& __x, const allocator_type& __a, true_type) noexcept
      : _Base(std::move(__x), __a)
      { }

     
      vector(vector&& __x, const allocator_type& __a, false_type)
      : _Base(__a)
      {
 if (__x.get_allocator() == __a)
   this->_M_move_data(std::move(__x));
 else
   {
     _M_initialize(__x.size());
     _M_copy_aligned(__x.begin(), __x.end(), begin());
     __x.clear();
   }
      }

    public:
     
      vector(vector&& __x, const __type_identity_t<allocator_type>& __a)
      noexcept(_Bit_alloc_traits::_S_always_equal())
      : vector(std::move(__x), __a,
        typename _Bit_alloc_traits::is_always_equal{})
      { }

     
      vector(const vector& __x, const __type_identity_t<allocator_type>& __a)
      : _Base(__a)
      {
 _M_initialize(__x.size());
 _M_copy_aligned(__x.begin(), __x.end(), begin());
      }

     
      vector(initializer_list<bool> __l,
      const allocator_type& __a = allocator_type())
      : _Base(__a)
      {
 _M_initialize_range(__l.begin(), __l.end(),
       random_access_iterator_tag());
      }



      template<typename _InputIterator,
        typename = std::_RequireInputIter<_InputIterator>>

 vector(_InputIterator __first, _InputIterator __last,
        const allocator_type& __a = allocator_type())
 : _Base(__a)
 {
   _M_initialize_range(__first, __last,
         std::__iterator_category(__first));
 }
# 920 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      ~vector() noexcept { }

     
      vector&
      operator=(const vector& __x)
      {
 if (&__x == this)
   return *this;

 if (_Bit_alloc_traits::_S_propagate_on_copy_assign())
   {
     if (this->_M_get_Bit_allocator() != __x._M_get_Bit_allocator())
       {
  this->_M_deallocate();
  std::__alloc_on_copy(_M_get_Bit_allocator(),
         __x._M_get_Bit_allocator());
  _M_initialize(__x.size());
       }
     else
       std::__alloc_on_copy(_M_get_Bit_allocator(),
       __x._M_get_Bit_allocator());
   }

 if (__x.size() > capacity())
   {
     this->_M_deallocate();
     _M_initialize(__x.size());
   }
 this->_M_impl._M_finish = _M_copy_aligned(__x.begin(), __x.end(),
        begin());
 return *this;
      }


     
      vector&
      operator=(vector&& __x) noexcept(_Bit_alloc_traits::_S_nothrow_move())
      {
 if (_Bit_alloc_traits::_S_propagate_on_move_assign()
     || this->_M_get_Bit_allocator() == __x._M_get_Bit_allocator())
   {
     this->_M_deallocate();
     this->_M_move_data(std::move(__x));
     std::__alloc_on_move(_M_get_Bit_allocator(),
     __x._M_get_Bit_allocator());
   }
 else
   {
     if (__x.size() > capacity())
       {
  this->_M_deallocate();
  _M_initialize(__x.size());
       }
     this->_M_impl._M_finish = _M_copy_aligned(__x.begin(), __x.end(),
            begin());
     __x.clear();
   }
 return *this;
      }

     
      vector&
      operator=(initializer_list<bool> __l)
      {
 this->assign(__l.begin(), __l.end());
 return *this;
      }






     
      void
      assign(size_type __n, const bool& __x)
      { _M_fill_assign(__n, __x); }


      template<typename _InputIterator,
        typename = std::_RequireInputIter<_InputIterator>>

 void
 assign(_InputIterator __first, _InputIterator __last)
 { _M_assign_aux(__first, __last, std::__iterator_category(__first)); }
# 1018 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      assign(initializer_list<bool> __l)
      { _M_assign_aux(__l.begin(), __l.end(), random_access_iterator_tag()); }
# 1039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      [[__nodiscard__]]
      iterator
      begin() noexcept
      { return iterator(this->_M_impl._M_start._M_p, 0); }

      [[__nodiscard__]]
      const_iterator
      begin() const noexcept
      { return const_iterator(this->_M_impl._M_start._M_p, 0); }

      [[__nodiscard__]]
      iterator
      end() noexcept
      { return this->_M_impl._M_finish; }

      [[__nodiscard__]]
      const_iterator
      end() const noexcept
      { return this->_M_impl._M_finish; }

      [[__nodiscard__]]
      reverse_iterator
      rbegin() noexcept
      { return reverse_iterator(end()); }

      [[__nodiscard__]]
      const_reverse_iterator
      rbegin() const noexcept
      { return const_reverse_iterator(end()); }

      [[__nodiscard__]]
      reverse_iterator
      rend() noexcept
      { return reverse_iterator(begin()); }

      [[__nodiscard__]]
      const_reverse_iterator
      rend() const noexcept
      { return const_reverse_iterator(begin()); }


      [[__nodiscard__]]
      const_iterator
      cbegin() const noexcept
      { return const_iterator(this->_M_impl._M_start._M_p, 0); }

      [[__nodiscard__]]
      const_iterator
      cend() const noexcept
      { return this->_M_impl._M_finish; }

      [[__nodiscard__]]
      const_reverse_iterator
      crbegin() const noexcept
      { return const_reverse_iterator(end()); }

      [[__nodiscard__]]
      const_reverse_iterator
      crend() const noexcept
      { return const_reverse_iterator(begin()); }


      [[__nodiscard__]]
      size_type
      size() const noexcept
      { return size_type(end() - begin()); }

      [[__nodiscard__]]
      size_type
      max_size() const noexcept
      {
 const size_type __isize =
   __gnu_cxx::__numeric_traits<difference_type>::__max
   - int(_S_word_bit) + 1;
 const size_type __asize
   = _Bit_alloc_traits::max_size(_M_get_Bit_allocator());
 return (__asize <= __isize / int(_S_word_bit)
  ? __asize * int(_S_word_bit) : __isize);
      }

      [[__nodiscard__]]
      size_type
      capacity() const noexcept
      { return size_type(const_iterator(this->_M_impl._M_end_addr(), 0)
    - begin()); }

      [[__nodiscard__]]
      bool
      empty() const noexcept
      { return begin() == end(); }

      [[__nodiscard__]]
      reference
      operator[](size_type __n)
      {
 do { if (__builtin_expect(!bool(__n < this->size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1134, __PRETTY_FUNCTION__, "__n < this->size()"); } while (false);
 return begin()[__n];
      }

      [[__nodiscard__]]
      const_reference
      operator[](size_type __n) const
      {
 do { if (__builtin_expect(!bool(__n < this->size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1142, __PRETTY_FUNCTION__, "__n < this->size()"); } while (false);
 return begin()[__n];
      }

    protected:
     
      void
      _M_range_check(size_type __n) const
      {
 if (__n >= this->size())
   __throw_out_of_range_fmt(("vector<bool>::_M_range_check: __n " "(which is %zu) >= this->size() " "(which is %zu)")

                            ,
       __n, this->size());
      }

    public:
      [[__nodiscard__]]
      reference
      at(size_type __n)
      {
 _M_range_check(__n);
 return (*this)[__n];
      }

      [[__nodiscard__]]
      const_reference
      at(size_type __n) const
      {
 _M_range_check(__n);
 return (*this)[__n];
      }

     
      void
      reserve(size_type __n)
      {
 if (__n > max_size())
   __throw_length_error(("vector::reserve"));
 if (capacity() < __n)
   _M_reallocate(__n);
      }

      [[__nodiscard__]]
      reference
      front()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1189, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }

      [[__nodiscard__]]
      const_reference
      front() const
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1197, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }

      [[__nodiscard__]]
      reference
      back()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1205, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }

      [[__nodiscard__]]
      const_reference
      back() const
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1213, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }

     
      void
      push_back(bool __x)
      {
 if (this->_M_impl._M_finish._M_p != this->_M_impl._M_end_addr())
   *this->_M_impl._M_finish++ = __x;
 else
   _M_insert_aux(end(), __x);
      }

     
      void
      swap(vector& __x) noexcept
      {

 do { if (__builtin_expect(!bool(_Bit_alloc_traits::propagate_on_container_swap::value || _M_get_Bit_allocator() == __x._M_get_Bit_allocator()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1232, __PRETTY_FUNCTION__, "_Bit_alloc_traits::propagate_on_container_swap::value || _M_get_Bit_allocator() == __x._M_get_Bit_allocator()"); } while (false)
                                                            ;

 this->_M_impl._M_swap_data(__x._M_impl);
 _Bit_alloc_traits::_S_on_swap(_M_get_Bit_allocator(),
          __x._M_get_Bit_allocator());
      }


     
      static void
      swap(reference __x, reference __y) noexcept
      {
 bool __tmp = __x;
 __x = __y;
 __y = __tmp;
      }

     
      iterator

      insert(const_iterator __position, const bool& __x)



      {
 const difference_type __n = __position - begin();
 if (this->_M_impl._M_finish._M_p != this->_M_impl._M_end_addr()
     && __position == end())
   *this->_M_impl._M_finish++ = __x;
 else
   _M_insert_aux(__position._M_const_cast(), __x);
 return begin() + __n;
      }


      __attribute__ ((__deprecated__ ("use '" "insert(position, false)" "' instead")))
      iterator
      insert(const_iterator __position)
      { return this->insert(__position._M_const_cast(), false); }



      template<typename _InputIterator,
        typename = std::_RequireInputIter<_InputIterator>>

 iterator
 insert(const_iterator __position,
        _InputIterator __first, _InputIterator __last)
 {
   difference_type __offset = __position - cbegin();
   _M_insert_range(__position._M_const_cast(),
     __first, __last,
     std::__iterator_category(__first));
   return begin() + __offset;
 }
# 1301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      iterator
      insert(const_iterator __position, size_type __n, const bool& __x)
      {
 difference_type __offset = __position - cbegin();
 _M_fill_insert(__position._M_const_cast(), __n, __x);
 return begin() + __offset;
      }







     
      iterator
      insert(const_iterator __p, initializer_list<bool> __l)
      { return this->insert(__p, __l.begin(), __l.end()); }
# 1402 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      pop_back()
      { --this->_M_impl._M_finish; }

     
      iterator

      erase(const_iterator __position)



      { return _M_erase(__position._M_const_cast()); }

     
      iterator

      erase(const_iterator __first, const_iterator __last)



      { return _M_erase(__first._M_const_cast(), __last._M_const_cast()); }

     
      void
      resize(size_type __new_size, bool __x = bool())
      {
 if (__new_size < size())
   _M_erase_at_end(begin() + difference_type(__new_size));
 else
   insert(end(), __new_size - size(), __x);
      }


     
      void
      shrink_to_fit()
      { _M_shrink_to_fit(); }


     
      void
      flip() noexcept
      {
 _Bit_type * const __end = this->_M_impl._M_end_addr();
 for (_Bit_type * __p = this->_M_impl._M_start._M_p; __p != __end; ++__p)
   *__p = ~*__p;
      }

     
      void
      clear() noexcept
      { _M_erase_at_end(begin()); }


      template<typename... _Args>




 void

 emplace_back(_Args&&... __args)
 {
   push_back(bool(std::forward<_Args>(__args)...));



 }

      template<typename... _Args>

 iterator
 emplace(const_iterator __pos, _Args&&... __args)
 { return insert(__pos, bool(std::forward<_Args>(__args)...)); }


    protected:

     
      iterator
      _M_copy_aligned(const_iterator __first, const_iterator __last,
        iterator __result)
      {
 _Bit_type* __q = std::copy(__first._M_p, __last._M_p, __result._M_p);
 return std::copy(const_iterator(__last._M_p, 0), __last,
    iterator(__q, 0));
      }

     
      void
      _M_initialize(size_type __n)
      {
 if (__n)
   {
     _Bit_pointer __q = this->_M_allocate(__n);
     this->_M_impl._M_end_of_storage = __q + _S_nword(__n);
     iterator __start = iterator(std::__addressof(*__q), 0);
     this->_M_impl._M_start = __start;
     this->_M_impl._M_finish = __start + difference_type(__n);
   }
      }

     
      void
      _M_initialize_value(bool __x) noexcept
      {
 if (_Bit_type* __p = this->_M_impl._M_start._M_p)
   __fill_bvector_n(__p, this->_M_impl._M_end_addr() - __p, __x);
      }

     
      void
      _M_reallocate(size_type __n);


     
      bool
      _M_shrink_to_fit();
# 1542 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      template<typename _InputIterator>

 void
 _M_initialize_range(_InputIterator __first, _InputIterator __last,
       std::input_iterator_tag)
 {
   for (; __first != __last; ++__first)
     push_back(*__first);
 }

      template<typename _ForwardIterator>

 void
 _M_initialize_range(_ForwardIterator __first, _ForwardIterator __last,
       std::forward_iterator_tag)
 {
   const size_type __n = std::distance(__first, __last);
   _M_initialize(__n);
   std::copy(__first, __last, begin());
 }
# 1578 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      _M_fill_assign(size_t __n, bool __x)
      {
 if (__n > size())
   {
     _M_initialize_value(__x);
     insert(end(), __n - size(), __x);
   }
 else
   {
     _M_erase_at_end(begin() + __n);
     _M_initialize_value(__x);
   }
      }

      template<typename _InputIterator>

 void
 _M_assign_aux(_InputIterator __first, _InputIterator __last,
        std::input_iterator_tag)
 {
   iterator __cur = begin();
   for (; __first != __last && __cur != end(); ++__cur, (void)++__first)
     *__cur = *__first;
   if (__first == __last)
     _M_erase_at_end(__cur);
   else
     insert(end(), __first, __last);
 }

      template<typename _ForwardIterator>

 void
 _M_assign_aux(_ForwardIterator __first, _ForwardIterator __last,
        std::forward_iterator_tag)
 {
   const size_type __len = std::distance(__first, __last);
   if (__len < size())
     _M_erase_at_end(std::copy(__first, __last, begin()));
   else
     {
       _ForwardIterator __mid = __first;
       std::advance(__mid, size());
       std::copy(__first, __mid, begin());
       insert(end(), __mid, __last);
     }
 }
# 1645 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      _M_fill_insert(iterator __position, size_type __n, bool __x);

      template<typename _InputIterator>

 void
 _M_insert_range(iterator __pos, _InputIterator __first,
   _InputIterator __last, std::input_iterator_tag)
 {
   for (; __first != __last; ++__first)
     {
       __pos = insert(__pos, *__first);
       ++__pos;
     }
 }

      template<typename _ForwardIterator>

 void
 _M_insert_range(iterator __position, _ForwardIterator __first,
   _ForwardIterator __last, std::forward_iterator_tag);

     
      void
      _M_insert_aux(iterator __position, bool __x);

     
      size_type
      _M_check_len(size_type __n, const char* __s) const
      {
 if (max_size() - size() < __n)
   __throw_length_error((__s));

 const size_type __len = size() + std::max(size(), __n);
 return (__len < size() || __len > max_size()) ? max_size() : __len;
      }

     
      void
      _M_erase_at_end(iterator __pos)
      { this->_M_impl._M_finish = __pos; }

     
      iterator
      _M_erase(iterator __pos);

     
      iterator
      _M_erase(iterator __first, iterator __last);

    protected:






      void data() = delete;



    };




 
  inline void
  __fill_bvector(_Bit_type* __v, unsigned int __first, unsigned int __last,
   bool __x) noexcept
  {
    const _Bit_type __fmask = ~0ul << __first;
    const _Bit_type __lmask = ~0ul >> (_S_word_bit - __last);
    const _Bit_type __mask = __fmask & __lmask;

    if (__x)
      *__v |= __mask;
    else
      *__v &= ~__mask;
  }


  __attribute__((__nonnull__))
 
  inline void
  __fill_bvector_n(_Bit_type* __p, size_t __n, bool __x) noexcept
  {
# 1741 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
    __builtin_memset(__p, __x ? ~0 : 0, __n * sizeof(_Bit_type));
  }


 
  inline void
  __fill_a1(std::_Bit_iterator __first,
     std::_Bit_iterator __last, const bool& __x)
  {
    if (__first._M_p != __last._M_p)
      {
 _Bit_type* __first_p = __first._M_p;
 if (__first._M_offset != 0)
   __fill_bvector(__first_p++, __first._M_offset, _S_word_bit, __x);

 __fill_bvector_n(__first_p, __last._M_p - __first_p, __x);

 if (__last._M_offset != 0)
   __fill_bvector(__last._M_p, 0, __last._M_offset, __x);
      }
    else if (__first._M_offset != __last._M_offset)
      __fill_bvector(__first._M_p, __first._M_offset, __last._M_offset, __x);
  }




  template<typename _Alloc>
    struct hash<std::vector<bool, _Alloc>>
    : public __hash_base<size_t, std::vector<bool, _Alloc>>
    {
      size_t
      operator()(const std::vector<bool, _Alloc>&) const noexcept;
    };



}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
  template<typename _Tp, typename _Up = typename __inv_unwrap<_Tp>::type>
    constexpr _Up&&
    __invfwd(typename remove_reference<_Tp>::type& __t) noexcept
    { return static_cast<_Up&&>(__t); }

  template<typename _Res, typename _Fn, typename... _Args>
    constexpr _Res
    __invoke_impl(__invoke_other, _Fn&& __f, _Args&&... __args)
    { return std::forward<_Fn>(__f)(std::forward<_Args>(__args)...); }

  template<typename _Res, typename _MemFun, typename _Tp, typename... _Args>
    constexpr _Res
    __invoke_impl(__invoke_memfun_ref, _MemFun&& __f, _Tp&& __t,
    _Args&&... __args)
    { return (__invfwd<_Tp>(__t).*__f)(std::forward<_Args>(__args)...); }

  template<typename _Res, typename _MemFun, typename _Tp, typename... _Args>
    constexpr _Res
    __invoke_impl(__invoke_memfun_deref, _MemFun&& __f, _Tp&& __t,
    _Args&&... __args)
    {
      return ((*std::forward<_Tp>(__t)).*__f)(std::forward<_Args>(__args)...);
    }

  template<typename _Res, typename _MemPtr, typename _Tp>
    constexpr _Res
    __invoke_impl(__invoke_memobj_ref, _MemPtr&& __f, _Tp&& __t)
    { return __invfwd<_Tp>(__t).*__f; }

  template<typename _Res, typename _MemPtr, typename _Tp>
    constexpr _Res
    __invoke_impl(__invoke_memobj_deref, _MemPtr&& __f, _Tp&& __t)
    { return (*std::forward<_Tp>(__t)).*__f; }


  template<typename _Callable, typename... _Args>
    constexpr typename __invoke_result<_Callable, _Args...>::type
    __invoke(_Callable&& __fn, _Args&&... __args)
    noexcept(__is_nothrow_invocable<_Callable, _Args...>::value)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;
      using __tag = typename __result::__invoke_type;
      return std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }
# 129 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/invoke.h" 3
  template<typename _Res, typename _Callable, typename... _Args>
    constexpr __enable_if_t<!is_void<_Res>::value, _Res>
    __invoke_r(_Callable&& __fn, _Args&&... __args)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;

      static_assert(!__reference_converts_from_temporary(_Res, __type),
      "INVOKE<R> must not create a dangling reference");

      using __tag = typename __result::__invoke_type;
      return std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }


  template<typename _Res, typename _Callable, typename... _Args>
    constexpr __enable_if_t<is_void<_Res>::value, _Res>
    __invoke_r(_Callable&& __fn, _Args&&... __args)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;
      using __tag = typename __result::__invoke_type;
      std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }



}
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 1 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 116 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Arg, typename _Result>
    struct unary_function
    {

      typedef _Arg argument_type;


      typedef _Result result_type;
    } __attribute__ ((__deprecated__));





  template<typename _Arg1, typename _Arg2, typename _Result>
    struct binary_function
    {

      typedef _Arg1 first_argument_type;


      typedef _Arg2 second_argument_type;


      typedef _Result result_type;
    } __attribute__ ((__deprecated__));
# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  struct __is_transparent;

  template<typename _Tp = void>
    struct plus;

  template<typename _Tp = void>
    struct minus;

  template<typename _Tp = void>
    struct multiplies;

  template<typename _Tp = void>
    struct divides;

  template<typename _Tp = void>
    struct modulus;

  template<typename _Tp = void>
    struct negate;



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct plus : public binary_function<_Tp, _Tp, _Tp>
    {

      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x + __y; }
    };


  template<typename _Tp>
    struct minus : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x - __y; }
    };


  template<typename _Tp>
    struct multiplies : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x * __y; }
    };


  template<typename _Tp>
    struct divides : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x / __y; }
    };


  template<typename _Tp>
    struct modulus : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x % __y; }
    };


  template<typename _Tp>
    struct negate : public unary_function<_Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x) const
      { return -__x; }
    };
#pragma GCC diagnostic pop


  template<>
    struct plus<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) + std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) + std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) + std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct minus<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) - std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) - std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) - std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct multiplies<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) * std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) * std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) * std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct divides<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) / std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) / std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) / std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct modulus<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) % std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) % std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) % std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct negate<void>
    {
      template <typename _Tp>
 constexpr
 auto
 operator()(_Tp&& __t) const
 noexcept(noexcept(-std::forward<_Tp>(__t)))
 -> decltype(-std::forward<_Tp>(__t))
 { return -std::forward<_Tp>(__t); }

      typedef __is_transparent is_transparent;
    };
# 346 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Tp = void>
    struct equal_to;

  template<typename _Tp = void>
    struct not_equal_to;

  template<typename _Tp = void>
    struct greater;

  template<typename _Tp = void>
    struct less;

  template<typename _Tp = void>
    struct greater_equal;

  template<typename _Tp = void>
    struct less_equal;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct equal_to : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x == __y; }
    };


  template<typename _Tp>
    struct not_equal_to : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x != __y; }
    };


  template<typename _Tp>
    struct greater : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x > __y; }
    };


  template<typename _Tp>
    struct less : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x < __y; }
    };


  template<typename _Tp>
    struct greater_equal : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x >= __y; }
    };


  template<typename _Tp>
    struct less_equal : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x <= __y; }
    };


  template<typename _Tp>
    struct greater<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x > __y;

 return (long unsigned int)__x > (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x < __y;

 return (long unsigned int)__x < (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct greater_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x >= __y;

 return (long unsigned int)__x >= (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      constexpr bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {

 if (std::__is_constant_evaluated())
   return __x <= __y;

 return (long unsigned int)__x <= (long unsigned int)__y;
      }
    };
#pragma GCC diagnostic pop



  template<>
    struct equal_to<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) == std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) == std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) == std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct not_equal_to<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) != std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) != std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) != std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct greater<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) > std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) > std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return greater<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) > std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return greater<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator>(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator>(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };


  template<>
    struct less<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) < std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) < std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return less<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) < std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return less<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator<(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator<(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };


  template<>
    struct greater_equal<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) >= std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) >= std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return greater_equal<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) >= std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return greater_equal<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator>=(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator>=(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };


  template<>
    struct less_equal<void>
    {
      template <typename _Tp, typename _Up>
 constexpr auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) <= std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) <= std::forward<_Up>(__u))
 {
   return _S_cmp(std::forward<_Tp>(__t), std::forward<_Up>(__u),
   __ptr_cmp<_Tp, _Up>{});
 }

      template<typename _Tp, typename _Up>
 constexpr bool
 operator()(_Tp* __t, _Up* __u) const noexcept
 { return less_equal<common_type_t<_Tp*, _Up*>>{}(__t, __u); }

      typedef __is_transparent is_transparent;

    private:
      template <typename _Tp, typename _Up>
 static constexpr decltype(auto)
 _S_cmp(_Tp&& __t, _Up&& __u, false_type)
 { return std::forward<_Tp>(__t) <= std::forward<_Up>(__u); }

      template <typename _Tp, typename _Up>
 static constexpr bool
 _S_cmp(_Tp&& __t, _Up&& __u, true_type) noexcept
 {
   return less_equal<const volatile void*>{}(
       static_cast<const volatile void*>(std::forward<_Tp>(__t)),
       static_cast<const volatile void*>(std::forward<_Up>(__u)));
 }


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded2 : true_type { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded2<_Tp, _Up, __void_t<
   decltype(std::declval<_Tp>().operator<=(std::declval<_Up>()))>>
 : false_type { };


      template<typename _Tp, typename _Up, typename = void>
 struct __not_overloaded : __not_overloaded2<_Tp, _Up> { };


      template<typename _Tp, typename _Up>
 struct __not_overloaded<_Tp, _Up, __void_t<
   decltype(operator<=(std::declval<_Tp>(), std::declval<_Up>()))>>
 : false_type { };

      template<typename _Tp, typename _Up>
 using __ptr_cmp = __and_<__not_overloaded<_Tp, _Up>,
       is_convertible<_Tp, const volatile void*>,
       is_convertible<_Up, const volatile void*>>;
    };
# 778 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Tp = void>
    struct logical_and;

  template<typename _Tp = void>
    struct logical_or;

  template<typename _Tp = void>
    struct logical_not;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct logical_and : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x && __y; }
    };


  template<typename _Tp>
    struct logical_or : public binary_function<_Tp, _Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x || __y; }
    };


  template<typename _Tp>
    struct logical_not : public unary_function<_Tp, bool>
    {
      constexpr
      bool
      operator()(const _Tp& __x) const
      { return !__x; }
    };
#pragma GCC diagnostic pop



  template<>
    struct logical_and<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) && std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) && std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) && std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct logical_or<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) || std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) || std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) || std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };


  template<>
    struct logical_not<void>
    {
      template <typename _Tp>
 constexpr
 auto
 operator()(_Tp&& __t) const
 noexcept(noexcept(!std::forward<_Tp>(__t)))
 -> decltype(!std::forward<_Tp>(__t))
 { return !std::forward<_Tp>(__t); }

      typedef __is_transparent is_transparent;
    };




  template<typename _Tp = void>
    struct bit_and;

  template<typename _Tp = void>
    struct bit_or;

  template<typename _Tp = void>
    struct bit_xor;

  template<typename _Tp = void>
    struct bit_not;


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"



  template<typename _Tp>
    struct bit_and : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x & __y; }
    };

  template<typename _Tp>
    struct bit_or : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x | __y; }
    };

  template<typename _Tp>
    struct bit_xor : public binary_function<_Tp, _Tp, _Tp>
    {
      constexpr
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x ^ __y; }
    };

  template<typename _Tp>
    struct bit_not : public unary_function<_Tp, _Tp>
    {
    constexpr
      _Tp
      operator()(const _Tp& __x) const
      { return ~__x; }
    };
#pragma GCC diagnostic pop


  template <>
    struct bit_and<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) & std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) & std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) & std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };

  template <>
    struct bit_or<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) | std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) | std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) | std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };

  template <>
    struct bit_xor<void>
    {
      template <typename _Tp, typename _Up>
 constexpr
 auto
 operator()(_Tp&& __t, _Up&& __u) const
 noexcept(noexcept(std::forward<_Tp>(__t) ^ std::forward<_Up>(__u)))
 -> decltype(std::forward<_Tp>(__t) ^ std::forward<_Up>(__u))
 { return std::forward<_Tp>(__t) ^ std::forward<_Up>(__u); }

      typedef __is_transparent is_transparent;
    };

  template <>
    struct bit_not<void>
    {
      template <typename _Tp>
 constexpr
 auto
 operator()(_Tp&& __t) const
 noexcept(noexcept(~std::forward<_Tp>(__t)))
 -> decltype(~std::forward<_Tp>(__t))
 { return ~std::forward<_Tp>(__t); }

      typedef __is_transparent is_transparent;
    };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
# 1020 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Predicate>
    class unary_negate
    : public unary_function<typename _Predicate::argument_type, bool>
    {
    protected:
      _Predicate _M_pred;

    public:
      constexpr
      explicit
      unary_negate(const _Predicate& __x) : _M_pred(__x) { }

      constexpr
      bool
      operator()(const typename _Predicate::argument_type& __x) const
      { return !_M_pred(__x); }
    };


  template<typename _Predicate>
   
    constexpr
    inline unary_negate<_Predicate>
    not1(const _Predicate& __pred)
    { return unary_negate<_Predicate>(__pred); }


  template<typename _Predicate>
    class binary_negate
    : public binary_function<typename _Predicate::first_argument_type,
        typename _Predicate::second_argument_type, bool>
    {
    protected:
      _Predicate _M_pred;

    public:
      constexpr
      explicit
      binary_negate(const _Predicate& __x) : _M_pred(__x) { }

      constexpr
      bool
      operator()(const typename _Predicate::first_argument_type& __x,
   const typename _Predicate::second_argument_type& __y) const
      { return !_M_pred(__x, __y); }
    };


  template<typename _Predicate>
   
    constexpr
    inline binary_negate<_Predicate>
    not2(const _Predicate& __pred)
    { return binary_negate<_Predicate>(__pred); }
# 1101 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Arg, typename _Result>
    class pointer_to_unary_function : public unary_function<_Arg, _Result>
    {
    protected:
      _Result (*_M_ptr)(_Arg);

    public:
      pointer_to_unary_function() { }

      explicit
      pointer_to_unary_function(_Result (*__x)(_Arg))
      : _M_ptr(__x) { }

      _Result
      operator()(_Arg __x) const
      { return _M_ptr(__x); }
    } __attribute__ ((__deprecated__));


  template<typename _Arg, typename _Result>
    __attribute__ ((__deprecated__ ("use '" "std::function" "' instead")))
    inline pointer_to_unary_function<_Arg, _Result>
    ptr_fun(_Result (*__x)(_Arg))
    { return pointer_to_unary_function<_Arg, _Result>(__x); }


  template<typename _Arg1, typename _Arg2, typename _Result>
    class pointer_to_binary_function
    : public binary_function<_Arg1, _Arg2, _Result>
    {
    protected:
      _Result (*_M_ptr)(_Arg1, _Arg2);

    public:
      pointer_to_binary_function() { }

      explicit
      pointer_to_binary_function(_Result (*__x)(_Arg1, _Arg2))
      : _M_ptr(__x) { }

      _Result
      operator()(_Arg1 __x, _Arg2 __y) const
      { return _M_ptr(__x, __y); }
    } __attribute__ ((__deprecated__));


  template<typename _Arg1, typename _Arg2, typename _Result>
    __attribute__ ((__deprecated__ ("use '" "std::function" "' instead")))
    inline pointer_to_binary_function<_Arg1, _Arg2, _Result>
    ptr_fun(_Result (*__x)(_Arg1, _Arg2))
    { return pointer_to_binary_function<_Arg1, _Arg2, _Result>(__x); }


  template<typename _Tp>
    struct _Identity
    : public unary_function<_Tp, _Tp>
    {
      _Tp&
      operator()(_Tp& __x) const
      { return __x; }

      const _Tp&
      operator()(const _Tp& __x) const
      { return __x; }
    };


  template<typename _Tp> struct _Identity<const _Tp> : _Identity<_Tp> { };

  template<typename _Pair>
    struct _Select1st
    : public unary_function<_Pair, typename _Pair::first_type>
    {
      typename _Pair::first_type&
      operator()(_Pair& __x) const
      { return __x.first; }

      const typename _Pair::first_type&
      operator()(const _Pair& __x) const
      { return __x.first; }


      template<typename _Pair2>
        typename _Pair2::first_type&
        operator()(_Pair2& __x) const
        { return __x.first; }

      template<typename _Pair2>
        const typename _Pair2::first_type&
        operator()(const _Pair2& __x) const
        { return __x.first; }

    };

  template<typename _Pair>
    struct _Select2nd
    : public unary_function<_Pair, typename _Pair::second_type>
    {
      typename _Pair::second_type&
      operator()(_Pair& __x) const
      { return __x.second; }

      const typename _Pair::second_type&
      operator()(const _Pair& __x) const
      { return __x.second; }
    };
# 1228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
  template<typename _Ret, typename _Tp>
    class mem_fun_t : public unary_function<_Tp*, _Ret>
    {
    public:
      explicit
      mem_fun_t(_Ret (_Tp::*__pf)())
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp* __p) const
      { return (__p->*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)();
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp>
    class const_mem_fun_t : public unary_function<const _Tp*, _Ret>
    {
    public:
      explicit
      const_mem_fun_t(_Ret (_Tp::*__pf)() const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp* __p) const
      { return (__p->*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)() const;
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp>
    class mem_fun_ref_t : public unary_function<_Tp, _Ret>
    {
    public:
      explicit
      mem_fun_ref_t(_Ret (_Tp::*__pf)())
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp& __r) const
      { return (__r.*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)();
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp>
    class const_mem_fun_ref_t : public unary_function<_Tp, _Ret>
    {
    public:
      explicit
      const_mem_fun_ref_t(_Ret (_Tp::*__pf)() const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp& __r) const
      { return (__r.*_M_f)(); }

    private:
      _Ret (_Tp::*_M_f)() const;
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class mem_fun1_t : public binary_function<_Tp*, _Arg, _Ret>
    {
    public:
      explicit
      mem_fun1_t(_Ret (_Tp::*__pf)(_Arg))
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp* __p, _Arg __x) const
      { return (__p->*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg);
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class const_mem_fun1_t : public binary_function<const _Tp*, _Arg, _Ret>
    {
    public:
      explicit
      const_mem_fun1_t(_Ret (_Tp::*__pf)(_Arg) const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp* __p, _Arg __x) const
      { return (__p->*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg) const;
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class mem_fun1_ref_t : public binary_function<_Tp, _Arg, _Ret>
    {
    public:
      explicit
      mem_fun1_ref_t(_Ret (_Tp::*__pf)(_Arg))
      : _M_f(__pf) { }

      _Ret
      operator()(_Tp& __r, _Arg __x) const
      { return (__r.*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg);
    } __attribute__ ((__deprecated__));


  template<typename _Ret, typename _Tp, typename _Arg>
    class const_mem_fun1_ref_t : public binary_function<_Tp, _Arg, _Ret>
    {
    public:
      explicit
      const_mem_fun1_ref_t(_Ret (_Tp::*__pf)(_Arg) const)
      : _M_f(__pf) { }

      _Ret
      operator()(const _Tp& __r, _Arg __x) const
      { return (__r.*_M_f)(__x); }

    private:
      _Ret (_Tp::*_M_f)(_Arg) const;
    } __attribute__ ((__deprecated__));



  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun_t<_Ret, _Tp>
    mem_fun(_Ret (_Tp::*__f)())
    { return mem_fun_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun_t<_Ret, _Tp>
    mem_fun(_Ret (_Tp::*__f)() const)
    { return const_mem_fun_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun_ref_t<_Ret, _Tp>
    mem_fun_ref(_Ret (_Tp::*__f)())
    { return mem_fun_ref_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun_ref_t<_Ret, _Tp>
    mem_fun_ref(_Ret (_Tp::*__f)() const)
    { return const_mem_fun_ref_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun1_t<_Ret, _Tp, _Arg>
    mem_fun(_Ret (_Tp::*__f)(_Arg))
    { return mem_fun1_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun1_t<_Ret, _Tp, _Arg>
    mem_fun(_Ret (_Tp::*__f)(_Arg) const)
    { return const_mem_fun1_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline mem_fun1_ref_t<_Ret, _Tp, _Arg>
    mem_fun_ref(_Ret (_Tp::*__f)(_Arg))
    { return mem_fun1_ref_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
    __attribute__ ((__deprecated__ ("use '" "std::mem_fn" "' instead")))
    inline const_mem_fun1_ref_t<_Ret, _Tp, _Arg>
    mem_fun_ref(_Ret (_Tp::*__f)(_Arg) const)
    { return const_mem_fun1_ref_t<_Ret, _Tp, _Arg>(__f); }
#pragma GCC diagnostic pop




  template<typename _Func, typename _SfinaeType, typename = __void_t<>>
    struct __has_is_transparent
    { };

  template<typename _Func, typename _SfinaeType>
    struct __has_is_transparent<_Func, _SfinaeType,
    __void_t<typename _Func::is_transparent>>
    { typedef void type; };

  template<typename _Func, typename _SfinaeType>
    using __has_is_transparent_t
      = typename __has_is_transparent<_Func, _SfinaeType>::type;
# 1437 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3

}


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/binders.h" 1 3
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/binders.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 107 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/backward/binders.h" 3
  template<typename _Operation>
    class binder1st
    : public unary_function<typename _Operation::second_argument_type,
       typename _Operation::result_type>
    {
    protected:
      _Operation op;
      typename _Operation::first_argument_type value;

    public:
      binder1st(const _Operation& __x,
  const typename _Operation::first_argument_type& __y)
      : op(__x), value(__y) { }

      typename _Operation::result_type
      operator()(const typename _Operation::second_argument_type& __x) const
      { return op(value, __x); }



      typename _Operation::result_type
      operator()(typename _Operation::second_argument_type& __x) const
      { return op(value, __x); }
    } __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")));


  template<typename _Operation, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")))
    inline binder1st<_Operation>
    bind1st(const _Operation& __fn, const _Tp& __x)
    {
      typedef typename _Operation::first_argument_type _Arg1_type;
      return binder1st<_Operation>(__fn, _Arg1_type(__x));
    }


  template<typename _Operation>
    class binder2nd
    : public unary_function<typename _Operation::first_argument_type,
       typename _Operation::result_type>
    {
    protected:
      _Operation op;
      typename _Operation::second_argument_type value;

    public:
      binder2nd(const _Operation& __x,
  const typename _Operation::second_argument_type& __y)
      : op(__x), value(__y) { }

      typename _Operation::result_type
      operator()(const typename _Operation::first_argument_type& __x) const
      { return op(__x, value); }



      typename _Operation::result_type
      operator()(typename _Operation::first_argument_type& __x) const
      { return op(__x, value); }
    } __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")));


  template<typename _Operation, typename _Tp>
    __attribute__ ((__deprecated__ ("use '" "std::bind" "' instead")))
    inline binder2nd<_Operation>
    bind2nd(const _Operation& __fn, const _Tp& __x)
    {
      typedef typename _Operation::second_argument_type _Arg2_type;
      return binder2nd<_Operation>(__fn, _Arg2_type(__x));
    }



}

#pragma GCC diagnostic pop
# 1442 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 2 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 2 3





namespace std __attribute__ ((__visibility__ ("default")))
{

# 58 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Res, typename... _ArgTypes>
    struct _Maybe_unary_or_binary_function { };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Res, typename _T1>
    struct _Maybe_unary_or_binary_function<_Res, _T1>
    : std::unary_function<_T1, _Res> { };


  template<typename _Res, typename _T1, typename _T2>
    struct _Maybe_unary_or_binary_function<_Res, _T1, _T2>
    : std::binary_function<_T1, _T2, _Res> { };

#pragma GCC diagnostic pop

  template<typename _Signature>
    struct _Mem_fn_traits;

  template<typename _Res, typename _Class, typename... _ArgTypes>
    struct _Mem_fn_traits_base
    {
      using __result_type = _Res;
      using __maybe_type
 = _Maybe_unary_or_binary_function<_Res, _Class*, _ArgTypes...>;
      using __arity = integral_constant<size_t, sizeof...(_ArgTypes)>;
    };
# 109 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) > : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) > : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const > : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const > : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) volatile > : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) volatile > : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const volatile > : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const volatile > : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = true_type; };
template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) &> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) &> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const &> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const &> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) volatile &> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) volatile &> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const volatile &> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const volatile &> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = true_type; };
template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) &&> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) &&> : _Mem_fn_traits_base<_Res, _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const &&> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const &&> : _Mem_fn_traits_base<_Res, const _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) volatile &&> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) volatile &&> : _Mem_fn_traits_base<_Res, volatile _Class, _ArgTypes...> { using __vararg = true_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes...) const volatile &&> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = false_type; }; template<typename _Res, typename _Class, typename... _ArgTypes> struct _Mem_fn_traits<_Res (_Class::*)(_ArgTypes..., ...) const volatile &&> : _Mem_fn_traits_base<_Res, const volatile _Class, _ArgTypes...> { using __vararg = true_type; };
# 123 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Functor, typename = __void_t<>>
    struct _Maybe_get_result_type
    { };

  template<typename _Functor>
    struct _Maybe_get_result_type<_Functor,
      __void_t<typename _Functor::result_type>>
    { typedef typename _Functor::result_type result_type; };





  template<typename _Functor>
    struct _Weak_result_type_impl
    : _Maybe_get_result_type<_Functor>
    { };


  template<typename _Res, typename... _ArgTypes >
    struct _Weak_result_type_impl<_Res(_ArgTypes...) >
    { typedef _Res result_type; };


  template<typename _Res, typename... _ArgTypes >
    struct _Weak_result_type_impl<_Res(_ArgTypes...,
           ...) >
    { typedef _Res result_type; };


  template<typename _Res, typename... _ArgTypes >
    struct _Weak_result_type_impl<_Res(*)(_ArgTypes...) >
    { typedef _Res result_type; };


  template<typename _Res, typename... _ArgTypes >
    struct
    _Weak_result_type_impl<_Res(*)(_ArgTypes..., ...) >
    { typedef _Res result_type; };


  template<typename _Functor,
    bool = is_member_function_pointer<_Functor>::value>
    struct _Weak_result_type_memfun
    : _Weak_result_type_impl<_Functor>
    { };


  template<typename _MemFunPtr>
    struct _Weak_result_type_memfun<_MemFunPtr, true>
    {
      using result_type = typename _Mem_fn_traits<_MemFunPtr>::__result_type;
    };


  template<typename _Func, typename _Class>
    struct _Weak_result_type_memfun<_Func _Class::*, false>
    { };





  template<typename _Functor>
    struct _Weak_result_type
    : _Weak_result_type_memfun<typename remove_cv<_Functor>::type>
    { };



  template<typename _Tp, typename = __void_t<>>
    struct _Refwrap_base_arg1
    { };


  template<typename _Tp>
    struct _Refwrap_base_arg1<_Tp,
         __void_t<typename _Tp::argument_type>>
    {
      typedef typename _Tp::argument_type argument_type;
    };


  template<typename _Tp, typename = __void_t<>>
    struct _Refwrap_base_arg2
    { };


  template<typename _Tp>
    struct _Refwrap_base_arg2<_Tp,
         __void_t<typename _Tp::first_argument_type,
           typename _Tp::second_argument_type>>
    {
      typedef typename _Tp::first_argument_type first_argument_type;
      typedef typename _Tp::second_argument_type second_argument_type;
    };







  template<typename _Tp>
    struct _Reference_wrapper_base
    : _Weak_result_type<_Tp>, _Refwrap_base_arg1<_Tp>, _Refwrap_base_arg2<_Tp>
    { };


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Res, typename _T1 >
    struct _Reference_wrapper_base<_Res(_T1) >
    : unary_function<_T1, _Res>
    { };

  template<typename _Res, typename _T1>
    struct _Reference_wrapper_base<_Res(_T1) const>
    : unary_function<_T1, _Res>
    { };

  template<typename _Res, typename _T1>
    struct _Reference_wrapper_base<_Res(_T1) volatile>
    : unary_function<_T1, _Res>
    { };

  template<typename _Res, typename _T1>
    struct _Reference_wrapper_base<_Res(_T1) const volatile>
    : unary_function<_T1, _Res>
    { };


  template<typename _Res, typename _T1, typename _T2 >
    struct _Reference_wrapper_base<_Res(_T1, _T2) >
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Res, typename _T1, typename _T2>
    struct _Reference_wrapper_base<_Res(_T1, _T2) const>
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Res, typename _T1, typename _T2>
    struct _Reference_wrapper_base<_Res(_T1, _T2) volatile>
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Res, typename _T1, typename _T2>
    struct _Reference_wrapper_base<_Res(_T1, _T2) const volatile>
    : binary_function<_T1, _T2, _Res>
    { };


  template<typename _Res, typename _T1 >
    struct _Reference_wrapper_base<_Res(*)(_T1) >
    : unary_function<_T1, _Res>
    { };


  template<typename _Res, typename _T1, typename _T2 >
    struct _Reference_wrapper_base<_Res(*)(_T1, _T2) >
    : binary_function<_T1, _T2, _Res>
    { };

  template<typename _Tp, bool = is_member_function_pointer<_Tp>::value>
    struct _Reference_wrapper_base_memfun
    : _Reference_wrapper_base<_Tp>
    { };

  template<typename _MemFunPtr>
    struct _Reference_wrapper_base_memfun<_MemFunPtr, true>
    : _Mem_fn_traits<_MemFunPtr>::__maybe_type
    {
      using result_type = typename _Mem_fn_traits<_MemFunPtr>::__result_type;
    };
#pragma GCC diagnostic pop
# 309 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Tp>
    class reference_wrapper



    : public _Reference_wrapper_base_memfun<typename remove_cv<_Tp>::type>

    {
      _Tp* _M_data;

     
      static _Tp* _S_fun(_Tp& __r) noexcept { return std::__addressof(__r); }

      static void _S_fun(_Tp&&) = delete;

      template<typename _Up, typename _Up2 = __remove_cvref_t<_Up>>
 using __not_same
   = typename enable_if<!is_same<reference_wrapper, _Up2>::value>::type;

    public:
      typedef _Tp type;




      template<typename _Up, typename = __not_same<_Up>, typename
  = decltype(reference_wrapper::_S_fun(std::declval<_Up>()))>

 reference_wrapper(_Up&& __uref)
 noexcept(noexcept(reference_wrapper::_S_fun(std::declval<_Up>())))
 : _M_data(reference_wrapper::_S_fun(std::forward<_Up>(__uref)))
 { }

      reference_wrapper(const reference_wrapper&) = default;

      reference_wrapper&
      operator=(const reference_wrapper&) = default;

     
      operator _Tp&() const noexcept
      { return this->get(); }

     
      _Tp&
      get() const noexcept
      { return *_M_data; }

      template<typename... _Args>

 typename __invoke_result<_Tp&, _Args...>::type
 operator()(_Args&&... __args) const
 noexcept(__is_nothrow_invocable<_Tp&, _Args...>::value)
 {




   return std::__invoke(get(), std::forward<_Args>(__args)...);
 }
# 415 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
    };
# 425 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 3
  template<typename _Tp>
   
    inline reference_wrapper<_Tp>
    ref(_Tp& __t) noexcept
    { return reference_wrapper<_Tp>(__t); }


  template<typename _Tp>
   
    inline reference_wrapper<const _Tp>
    cref(const _Tp& __t) noexcept
    { return reference_wrapper<const _Tp>(__t); }

  template<typename _Tp>
    void ref(const _Tp&&) = delete;

  template<typename _Tp>
    void cref(const _Tp&&) = delete;


  template<typename _Tp>
   
    inline reference_wrapper<_Tp>
    ref(reference_wrapper<_Tp> __t) noexcept
    { return __t; }


  template<typename _Tp>
   
    inline reference_wrapper<const _Tp>
    cref(reference_wrapper<_Tp> __t) noexcept
    { return { __t.get() }; }




}
# 71 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/range_access.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/range_access.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    begin(_Container& __cont) noexcept(noexcept(__cont.begin()))
    -> decltype(__cont.begin())
    { return __cont.begin(); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    begin(const _Container& __cont) noexcept(noexcept(__cont.begin()))
    -> decltype(__cont.begin())
    { return __cont.begin(); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    end(_Container& __cont) noexcept(noexcept(__cont.end()))
    -> decltype(__cont.end())
    { return __cont.end(); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    end(const _Container& __cont) noexcept(noexcept(__cont.end()))
    -> decltype(__cont.end())
    { return __cont.end(); }





  template<typename _Tp, size_t _Nm>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline constexpr _Tp*
    begin(_Tp (&__arr)[_Nm]) noexcept
    { return __arr; }






  template<typename _Tp, size_t _Nm>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline constexpr _Tp*
    end(_Tp (&__arr)[_Nm]) noexcept
    { return __arr + _Nm; }



  template<typename _Tp> class valarray;

  template<typename _Tp> _Tp* begin(valarray<_Tp>&) noexcept;
  template<typename _Tp> const _Tp* begin(const valarray<_Tp>&) noexcept;
  template<typename _Tp> _Tp* end(valarray<_Tp>&) noexcept;
  template<typename _Tp> const _Tp* end(const valarray<_Tp>&) noexcept;






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    constexpr auto
    cbegin(const _Container& __cont) noexcept(noexcept(std::begin(__cont)))
      -> decltype(std::begin(__cont))
    { return std::begin(__cont); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    constexpr auto
    cend(const _Container& __cont) noexcept(noexcept(std::end(__cont)))
      -> decltype(std::end(__cont))
    { return std::end(__cont); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    rbegin(_Container& __cont) -> decltype(__cont.rbegin())
    { return __cont.rbegin(); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    rbegin(const _Container& __cont) -> decltype(__cont.rbegin())
    { return __cont.rbegin(); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    rend(_Container& __cont) -> decltype(__cont.rend())
    { return __cont.rend(); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    rend(const _Container& __cont) -> decltype(__cont.rend())
    { return __cont.rend(); }






  template<typename _Tp, size_t _Nm>
    [[__nodiscard__]]
    inline reverse_iterator<_Tp*>
    rbegin(_Tp (&__arr)[_Nm]) noexcept
    { return reverse_iterator<_Tp*>(__arr + _Nm); }






  template<typename _Tp, size_t _Nm>
    [[__nodiscard__]]
    inline reverse_iterator<_Tp*>
    rend(_Tp (&__arr)[_Nm]) noexcept
    { return reverse_iterator<_Tp*>(__arr); }






  template<typename _Tp>
    [[__nodiscard__]]
    inline reverse_iterator<const _Tp*>
    rbegin(initializer_list<_Tp> __il) noexcept
    { return reverse_iterator<const _Tp*>(__il.end()); }






  template<typename _Tp>
    [[__nodiscard__]]
    inline reverse_iterator<const _Tp*>
    rend(initializer_list<_Tp> __il) noexcept
    { return reverse_iterator<const _Tp*>(__il.begin()); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    crbegin(const _Container& __cont) -> decltype(std::rbegin(__cont))
    { return std::rbegin(__cont); }






  template<typename _Container>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline auto
    crend(const _Container& __cont) -> decltype(std::rend(__cont))
    { return std::rend(__cont); }
# 371 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/range_access.h" 3

}
# 72 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 1 3
# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  template<typename _Tp, typename _Alloc>
   
    void
    vector<_Tp, _Alloc>::
    reserve(size_type __n)
    {
      if (__n > this->max_size())
 __throw_length_error(("vector::reserve"));
      if (this->capacity() < __n)
 {
   const size_type __old_size = size();
   pointer __tmp;

   if (_S_use_relocate())
     {
       __tmp = this->_M_allocate(__n);
       _S_relocate(this->_M_impl._M_start, this->_M_impl._M_finish,
     __tmp, _M_get_Tp_allocator());
     }
   else

     {
       __tmp = _M_allocate_and_copy(__n,
  std::__make_move_if_noexcept_iterator(this->_M_impl._M_start),
  std::__make_move_if_noexcept_iterator(this->_M_impl._M_finish));
       std::_Destroy(this->_M_impl._M_start, this->_M_impl._M_finish,
       _M_get_Tp_allocator());
     }
   ;
   _M_deallocate(this->_M_impl._M_start,
   this->_M_impl._M_end_of_storage
   - this->_M_impl._M_start);
   this->_M_impl._M_start = __tmp;
   this->_M_impl._M_finish = __tmp + __old_size;
   this->_M_impl._M_end_of_storage = this->_M_impl._M_start + __n;
 }
    }


  template<typename _Tp, typename _Alloc>
    template<typename... _Args>




      void

      vector<_Tp, _Alloc>::
      emplace_back(_Args&&... __args)
      {
 if (this->_M_impl._M_finish != this->_M_impl._M_end_of_storage)
   {
     ;
     _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
         std::forward<_Args>(__args)...);
     ++this->_M_impl._M_finish;
     ;
   }
 else
   _M_realloc_append(std::forward<_Args>(__args)...);



      }


  template<typename _Tp, typename _Alloc>
   
    typename vector<_Tp, _Alloc>::iterator
    vector<_Tp, _Alloc>::

    insert(const_iterator __position, const value_type& __x)



    {
      const size_type __n = __position - begin();
      if (this->_M_impl._M_finish != this->_M_impl._M_end_of_storage)
 {
   do { if (__builtin_expect(!bool(__position != const_iterator()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc", 143, __PRETTY_FUNCTION__, "__position != const_iterator()"); } while (false);
   if (!(__position != const_iterator()))
     __builtin_unreachable();

   if (__position == end())
     {
       ;
       _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
           __x);
       ++this->_M_impl._M_finish;
       ;
     }
   else
     {

       const auto __pos = begin() + (__position - cbegin());


       _Temporary_value __x_copy(this, __x);
       _M_insert_aux(__pos, std::move(__x_copy._M_val()));



     }
 }
      else

 _M_realloc_insert(begin() + (__position - cbegin()), __x);




      return iterator(this->_M_impl._M_start + __n);
    }

  template<typename _Tp, typename _Alloc>
   
    typename vector<_Tp, _Alloc>::iterator
    vector<_Tp, _Alloc>::
    _M_erase(iterator __position)
    {
      if (__position + 1 != end())
 std::move(__position + 1, end(), __position);
      --this->_M_impl._M_finish;
      _Alloc_traits::destroy(this->_M_impl, this->_M_impl._M_finish);
      ;
      return __position;
    }

  template<typename _Tp, typename _Alloc>
   
    typename vector<_Tp, _Alloc>::iterator
    vector<_Tp, _Alloc>::
    _M_erase(iterator __first, iterator __last)
    {
      if (__first != __last)
 {
   if (__last != end())
     std::move(__last, end(), __first);
   _M_erase_at_end(__first.base() + (end() - __last));
 }
      return __first;
    }

  template<typename _Tp, typename _Alloc>
   
    vector<_Tp, _Alloc>&
    vector<_Tp, _Alloc>::
    operator=(const vector<_Tp, _Alloc>& __x)
    {
      if (std::__addressof(__x) != this)
 {
   ;

   if (_Alloc_traits::_S_propagate_on_copy_assign())
     {
       if (!_Alloc_traits::_S_always_equal()
           && _M_get_Tp_allocator() != __x._M_get_Tp_allocator())
         {

    this->clear();
    _M_deallocate(this->_M_impl._M_start,
    this->_M_impl._M_end_of_storage
    - this->_M_impl._M_start);
    this->_M_impl._M_start = nullptr;
    this->_M_impl._M_finish = nullptr;
    this->_M_impl._M_end_of_storage = nullptr;
  }
       std::__alloc_on_copy(_M_get_Tp_allocator(),
       __x._M_get_Tp_allocator());
     }

   const size_type __xlen = __x.size();
   if (__xlen > capacity())
     {
       pointer __tmp = _M_allocate_and_copy(__xlen, __x.begin(),
         __x.end());
       std::_Destroy(this->_M_impl._M_start, this->_M_impl._M_finish,
       _M_get_Tp_allocator());
       _M_deallocate(this->_M_impl._M_start,
       this->_M_impl._M_end_of_storage
       - this->_M_impl._M_start);
       this->_M_impl._M_start = __tmp;
       this->_M_impl._M_end_of_storage = this->_M_impl._M_start + __xlen;
     }
   else if (size() >= __xlen)
     {
       std::_Destroy(std::copy(__x.begin(), __x.end(), begin()),
       end(), _M_get_Tp_allocator());
     }
   else
     {
       std::copy(__x._M_impl._M_start, __x._M_impl._M_start + size(),
   this->_M_impl._M_start);
       std::__uninitialized_copy_a(__x._M_impl._M_start + size(),
       __x._M_impl._M_finish,
       this->_M_impl._M_finish,
       _M_get_Tp_allocator());
     }
   this->_M_impl._M_finish = this->_M_impl._M_start + __xlen;
 }
      return *this;
    }

  template<typename _Tp, typename _Alloc>
   
    void
    vector<_Tp, _Alloc>::
    _M_fill_assign(size_t __n, const value_type& __val)
    {
      const size_type __sz = size();
      if (__n > capacity())
 {
   if (__n <= __sz)
     __builtin_unreachable();
   vector __tmp(__n, __val, _M_get_Tp_allocator());
   __tmp._M_impl._M_swap_data(this->_M_impl);
 }
      else if (__n > __sz)
 {
   std::fill(begin(), end(), __val);
   const size_type __add = __n - __sz;
   ;
   this->_M_impl._M_finish =
     std::__uninitialized_fill_n_a(this->_M_impl._M_finish,
       __add, __val, _M_get_Tp_allocator());
   ;
 }
      else
        _M_erase_at_end(std::fill_n(this->_M_impl._M_start, __n, __val));
    }

  template<typename _Tp, typename _Alloc>
    template<typename _InputIterator>
     
      void
      vector<_Tp, _Alloc>::
      _M_assign_aux(_InputIterator __first, _InputIterator __last,
      std::input_iterator_tag)
      {
 pointer __cur(this->_M_impl._M_start);
 for (; __first != __last && __cur != this->_M_impl._M_finish;
      ++__cur, (void)++__first)
   *__cur = *__first;
 if (__first == __last)
   _M_erase_at_end(__cur);
 else
   _M_range_insert(end(), __first, __last,
     std::__iterator_category(__first));
      }

  template<typename _Tp, typename _Alloc>
    template<typename _ForwardIterator>
     
      void
      vector<_Tp, _Alloc>::
      _M_assign_aux(_ForwardIterator __first, _ForwardIterator __last,
      std::forward_iterator_tag)
      {
 const size_type __sz = size();
 const size_type __len = std::distance(__first, __last);

 if (__len > capacity())
   {
     if (__len <= __sz)
       __builtin_unreachable();

     _S_check_init_len(__len, _M_get_Tp_allocator());
     pointer __tmp(_M_allocate_and_copy(__len, __first, __last));
     std::_Destroy(this->_M_impl._M_start, this->_M_impl._M_finish,
     _M_get_Tp_allocator());
     ;
     _M_deallocate(this->_M_impl._M_start,
     this->_M_impl._M_end_of_storage
     - this->_M_impl._M_start);
     this->_M_impl._M_start = __tmp;
     this->_M_impl._M_finish = this->_M_impl._M_start + __len;
     this->_M_impl._M_end_of_storage = this->_M_impl._M_finish;
   }
 else if (__sz >= __len)
   _M_erase_at_end(std::copy(__first, __last, this->_M_impl._M_start));
 else
   {
     _ForwardIterator __mid = __first;
     std::advance(__mid, __sz);
     std::copy(__first, __mid, this->_M_impl._M_start);
     const size_type __attribute__((__unused__)) __n = __len - __sz;
     ;
     this->_M_impl._M_finish =
       std::__uninitialized_copy_a(__mid, __last,
       this->_M_impl._M_finish,
       _M_get_Tp_allocator());
     ;
   }
      }


  template<typename _Tp, typename _Alloc>
   
    auto
    vector<_Tp, _Alloc>::
    _M_insert_rval(const_iterator __position, value_type&& __v) -> iterator
    {
      const auto __n = __position - cbegin();
      if (this->_M_impl._M_finish != this->_M_impl._M_end_of_storage)
 if (__position == cend())
   {
     ;
     _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
         std::move(__v));
     ++this->_M_impl._M_finish;
     ;
   }
 else
   _M_insert_aux(begin() + __n, std::move(__v));
      else
 _M_realloc_insert(begin() + __n, std::move(__v));

      return iterator(this->_M_impl._M_start + __n);
    }

  template<typename _Tp, typename _Alloc>
    template<typename... _Args>
     
      auto
      vector<_Tp, _Alloc>::
      _M_emplace_aux(const_iterator __position, _Args&&... __args)
      -> iterator
      {
 const auto __n = __position - cbegin();
 if (this->_M_impl._M_finish != this->_M_impl._M_end_of_storage)
   if (__position == cend())
     {
       ;
       _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
           std::forward<_Args>(__args)...);
       ++this->_M_impl._M_finish;
       ;
     }
   else
     {



       _Temporary_value __tmp(this, std::forward<_Args>(__args)...);
       _M_insert_aux(begin() + __n, std::move(__tmp._M_val()));
     }
 else
   _M_realloc_insert(begin() + __n, std::forward<_Args>(__args)...);

 return iterator(this->_M_impl._M_start + __n);
      }

  template<typename _Tp, typename _Alloc>
    template<typename _Arg>
     
      void
      vector<_Tp, _Alloc>::
      _M_insert_aux(iterator __position, _Arg&& __arg)






    {
      ;
      _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
          std::move(*(this->_M_impl._M_finish - 1)));
      ++this->_M_impl._M_finish;
      ;



      std::move_backward(__position.base(), this->_M_impl._M_finish - 2, this->_M_impl._M_finish - 1)

                                     ;



      *__position = std::forward<_Arg>(__arg);

    }


  template<typename _Tp, typename _Alloc>
    template<typename... _Args>
     
      void
      vector<_Tp, _Alloc>::
      _M_realloc_insert(iterator __position, _Args&&... __args)






    {
      const size_type __len = _M_check_len(1u, "vector::_M_realloc_insert");
      if (__len <= 0)
 __builtin_unreachable ();
      pointer __old_start = this->_M_impl._M_start;
      pointer __old_finish = this->_M_impl._M_finish;
      const size_type __elems_before = __position - begin();
      pointer __new_start(this->_M_allocate(__len));
      pointer __new_finish(__new_start);

      {
 _Guard_alloc __guard(__new_start, __len, *this);
# 481 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
 _Alloc_traits::construct(this->_M_impl,
     std::__to_address(__new_start + __elems_before),
     std::forward<_Args>(__args)...);







 if (_S_use_relocate())
   {

     __new_finish = _S_relocate(__old_start, __position.base(),
           __new_start, _M_get_Tp_allocator());
     ++__new_finish;
     __new_finish = _S_relocate(__position.base(), __old_finish,
           __new_finish, _M_get_Tp_allocator());
   }
 else

   {

     struct _Guard_elts
     {
       pointer _M_first, _M_last;
       _Tp_alloc_type& _M_alloc;

      
       _Guard_elts(pointer __elt, _Tp_alloc_type& __a)
       : _M_first(__elt), _M_last(__elt + 1), _M_alloc(__a)
       { }

      
       ~_Guard_elts()
       { std::_Destroy(_M_first, _M_last, _M_alloc); }

     private:
       _Guard_elts(const _Guard_elts&);
     };


     _Guard_elts __guard_elts(__new_start + __elems_before, _M_impl);

     __new_finish = std::__uninitialized_move_if_noexcept_a(
        __old_start, __position.base(),
        __new_start, _M_get_Tp_allocator());

     ++__new_finish;

     __guard_elts._M_first = __new_start;

     __new_finish = std::__uninitialized_move_if_noexcept_a(
         __position.base(), __old_finish,
         __new_finish, _M_get_Tp_allocator());


     __guard_elts._M_first = __old_start;
     __guard_elts._M_last = __old_finish;
   }
 __guard._M_storage = __old_start;
 __guard._M_len = this->_M_impl._M_end_of_storage - __old_start;
      }



      this->_M_impl._M_start = __new_start;
      this->_M_impl._M_finish = __new_finish;
      this->_M_impl._M_end_of_storage = __new_start + __len;
    }


  template<typename _Tp, typename _Alloc>
    template<typename... _Args>
     
      void
      vector<_Tp, _Alloc>::
      _M_realloc_append(_Args&&... __args)






    {
      const size_type __len = _M_check_len(1u, "vector::_M_realloc_append");
      if (__len <= 0)
 __builtin_unreachable ();
      pointer __old_start = this->_M_impl._M_start;
      pointer __old_finish = this->_M_impl._M_finish;
      const size_type __elems = end() - begin();
      pointer __new_start(this->_M_allocate(__len));
      pointer __new_finish(__new_start);

      {
 _Guard_alloc __guard(__new_start, __len, *this);
# 586 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
 _Alloc_traits::construct(this->_M_impl,
     std::__to_address(__new_start + __elems),
     std::forward<_Args>(__args)...);







 if (_S_use_relocate())
   {

     __new_finish = _S_relocate(__old_start, __old_finish,
           __new_start, _M_get_Tp_allocator());
     ++__new_finish;
   }
 else

   {

     struct _Guard_elts
     {
       pointer _M_first, _M_last;
       _Tp_alloc_type& _M_alloc;

      
       _Guard_elts(pointer __elt, _Tp_alloc_type& __a)
       : _M_first(__elt), _M_last(__elt + 1), _M_alloc(__a)
       { }

      
       ~_Guard_elts()
       { std::_Destroy(_M_first, _M_last, _M_alloc); }

     private:
       _Guard_elts(const _Guard_elts&);
     };


     _Guard_elts __guard_elts(__new_start + __elems, _M_impl);

     __new_finish = std::__uninitialized_move_if_noexcept_a(
        __old_start, __old_finish,
        __new_start, _M_get_Tp_allocator());

     ++__new_finish;


     __guard_elts._M_first = __old_start;
     __guard_elts._M_last = __old_finish;
   }
 __guard._M_storage = __old_start;
 __guard._M_len = this->_M_impl._M_end_of_storage - __old_start;
      }



      this->_M_impl._M_start = __new_start;
      this->_M_impl._M_finish = __new_finish;
      this->_M_impl._M_end_of_storage = __new_start + __len;
    }

  template<typename _Tp, typename _Alloc>
   
    void
    vector<_Tp, _Alloc>::
    _M_fill_insert(iterator __position, size_type __n, const value_type& __x)
    {
      if (__n != 0)
 {
   if (size_type(this->_M_impl._M_end_of_storage
   - this->_M_impl._M_finish) >= __n)
     {



       _Temporary_value __tmp(this, __x);
       value_type& __x_copy = __tmp._M_val();

       const size_type __elems_after = end() - __position;
       pointer __old_finish(this->_M_impl._M_finish);
       if (__elems_after > __n)
  {
    ;
    std::__uninitialized_move_a(__old_finish - __n,
           __old_finish,
           __old_finish,
           _M_get_Tp_allocator());
    this->_M_impl._M_finish += __n;
    ;
    std::move_backward(__position.base(), __old_finish - __n, __old_finish)
                                        ;
    std::fill(__position.base(), __position.base() + __n,
       __x_copy);
  }
       else
  {
    ;
    this->_M_impl._M_finish =
      std::__uninitialized_fill_n_a(__old_finish,
        __n - __elems_after,
        __x_copy,
        _M_get_Tp_allocator());
    ;
    std::__uninitialized_move_a(__position.base(), __old_finish,
           this->_M_impl._M_finish,
           _M_get_Tp_allocator());
    this->_M_impl._M_finish += __elems_after;
    ;
    std::fill(__position.base(), __old_finish, __x_copy);
  }
     }
   else
     {


       pointer __old_start = this->_M_impl._M_start;
       pointer __old_finish = this->_M_impl._M_finish;
       const pointer __pos = __position.base();

       const size_type __len =
  _M_check_len(__n, "vector::_M_fill_insert");
       const size_type __elems_before = __pos - __old_start;
       pointer __new_start(this->_M_allocate(__len));
       pointer __new_finish(__new_start);
       try
  {

    std::__uninitialized_fill_n_a(__new_start + __elems_before,
      __n, __x,
      _M_get_Tp_allocator());
    __new_finish = pointer();

    __new_finish
      = std::__uninitialized_move_if_noexcept_a
      (__old_start, __pos, __new_start, _M_get_Tp_allocator());

    __new_finish += __n;

    __new_finish
      = std::__uninitialized_move_if_noexcept_a
      (__pos, __old_finish, __new_finish, _M_get_Tp_allocator());
  }
       catch(...)
  {
    if (!__new_finish)
      std::_Destroy(__new_start + __elems_before,
      __new_start + __elems_before + __n,
      _M_get_Tp_allocator());
    else
      std::_Destroy(__new_start, __new_finish,
      _M_get_Tp_allocator());
    _M_deallocate(__new_start, __len);
    throw;
  }
       std::_Destroy(__old_start, __old_finish, _M_get_Tp_allocator());
       ;
       _M_deallocate(__old_start,
       this->_M_impl._M_end_of_storage - __old_start);
       this->_M_impl._M_start = __new_start;
       this->_M_impl._M_finish = __new_finish;
       this->_M_impl._M_end_of_storage = __new_start + __len;
     }
 }
    }


  template<typename _Tp, typename _Alloc>
   
    void
    vector<_Tp, _Alloc>::
    _M_default_append(size_type __n)
    {
      if (__n != 0)
 {
   const size_type __size = size();
   size_type __navail = size_type(this->_M_impl._M_end_of_storage
      - this->_M_impl._M_finish);

   if (__size > max_size() || __navail > max_size() - __size)
     __builtin_unreachable();

   if (__navail >= __n)
     {
       ;
       this->_M_impl._M_finish =
  std::__uninitialized_default_n_a(this->_M_impl._M_finish,
       __n, _M_get_Tp_allocator());
       ;
     }
   else
     {


       pointer __old_start = this->_M_impl._M_start;
       pointer __old_finish = this->_M_impl._M_finish;

       const size_type __len =
  _M_check_len(__n, "vector::_M_default_append");
       pointer __new_start(this->_M_allocate(__len));

       {
  _Guard_alloc __guard(__new_start, __len, *this);

  std::__uninitialized_default_n_a(__new_start + __size, __n,
       _M_get_Tp_allocator());

  if (_S_use_relocate())
    {
      _S_relocate(__old_start, __old_finish,
    __new_start, _M_get_Tp_allocator());
    }
  else
    {

      struct _Guard_elts
      {
        pointer _M_first, _M_last;
        _Tp_alloc_type& _M_alloc;

       
        _Guard_elts(pointer __first, size_type __n,
      _Tp_alloc_type& __a)
        : _M_first(__first), _M_last(__first + __n), _M_alloc(__a)
        { }

       
        ~_Guard_elts()
        { std::_Destroy(_M_first, _M_last, _M_alloc); }

      private:
        _Guard_elts(const _Guard_elts&);
      };
      _Guard_elts __guard_elts(__new_start + __size, __n, _M_impl);

      std::__uninitialized_move_if_noexcept_a(
        __old_start, __old_finish, __new_start,
        _M_get_Tp_allocator());

      __guard_elts._M_first = __old_start;
      __guard_elts._M_last = __old_finish;
    }
  ;
  __guard._M_storage = __old_start;
  __guard._M_len = this->_M_impl._M_end_of_storage - __old_start;
       }



       this->_M_impl._M_start = __new_start;
       this->_M_impl._M_finish = __new_start + __size + __n;
       this->_M_impl._M_end_of_storage = __new_start + __len;
     }
 }
    }

  template<typename _Tp, typename _Alloc>
   
    bool
    vector<_Tp, _Alloc>::
    _M_shrink_to_fit()
    {
      if (capacity() == size())
 return false;
      ;
      return std::__shrink_to_fit_aux<vector>::_S_do_it(*this);
    }


  template<typename _Tp, typename _Alloc>
    template<typename _InputIterator>
     
      void
      vector<_Tp, _Alloc>::
      _M_range_insert(iterator __pos, _InputIterator __first,
        _InputIterator __last, std::input_iterator_tag)
      {
 if (__pos == end())
   {
     for (; __first != __last; ++__first)
       insert(end(), *__first);
   }
 else if (__first != __last)
   {
     vector __tmp(__first, __last, _M_get_Tp_allocator());
     insert(__pos,
     std::make_move_iterator(__tmp.begin()),
     std::make_move_iterator(__tmp.end()));
   }
      }

  template<typename _Tp, typename _Alloc>
    template<typename _ForwardIterator>
     
      void
      vector<_Tp, _Alloc>::
      _M_range_insert(iterator __position, _ForwardIterator __first,
        _ForwardIterator __last, std::forward_iterator_tag)
      {
 if (__first != __last)
   {
     const size_type __n = std::distance(__first, __last);
     if (size_type(this->_M_impl._M_end_of_storage
     - this->_M_impl._M_finish) >= __n)
       {
  const size_type __elems_after = end() - __position;
  pointer __old_finish(this->_M_impl._M_finish);
  if (__elems_after > __n)
    {
      ;
      std::__uninitialized_move_a(this->_M_impl._M_finish - __n,
      this->_M_impl._M_finish,
      this->_M_impl._M_finish,
      _M_get_Tp_allocator());
      this->_M_impl._M_finish += __n;
      ;
      std::move_backward(__position.base(), __old_finish - __n, __old_finish)
                                          ;
      std::copy(__first, __last, __position);
    }
  else
    {
      _ForwardIterator __mid = __first;
      std::advance(__mid, __elems_after);
      ;
      std::__uninitialized_copy_a(__mid, __last,
      this->_M_impl._M_finish,
      _M_get_Tp_allocator());
      this->_M_impl._M_finish += __n - __elems_after;
      ;
      std::__uninitialized_move_a(__position.base(),
      __old_finish,
      this->_M_impl._M_finish,
      _M_get_Tp_allocator());
      this->_M_impl._M_finish += __elems_after;
      ;
      std::copy(__first, __mid, __position);
    }
       }
     else
       {



  pointer __old_start = this->_M_impl._M_start;
  pointer __old_finish = this->_M_impl._M_finish;

  const size_type __len =
    _M_check_len(__n, "vector::_M_range_insert");





  pointer __new_start(this->_M_allocate(__len));
  pointer __new_finish(__new_start);
  try
    {
      __new_finish
        = std::__uninitialized_move_if_noexcept_a
        (__old_start, __position.base(),
         __new_start, _M_get_Tp_allocator());
      __new_finish
        = std::__uninitialized_copy_a(__first, __last,
          __new_finish,
          _M_get_Tp_allocator());
      __new_finish
        = std::__uninitialized_move_if_noexcept_a
        (__position.base(), __old_finish,
         __new_finish, _M_get_Tp_allocator());
    }
  catch(...)
    {
      std::_Destroy(__new_start, __new_finish,
      _M_get_Tp_allocator());
      _M_deallocate(__new_start, __len);
      throw;
    }
  std::_Destroy(__old_start, __old_finish,
         _M_get_Tp_allocator());
  ;
  _M_deallocate(__old_start,
         this->_M_impl._M_end_of_storage - __old_start);
  this->_M_impl._M_start = __new_start;
  this->_M_impl._M_finish = __new_finish;
  this->_M_impl._M_end_of_storage = __new_start + __len;
       }
   }
      }
# 1102 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
  template<typename _Alloc>
   
    void
    vector<bool, _Alloc>::
    _M_reallocate(size_type __n)
    {
      _Bit_pointer __q = this->_M_allocate(__n);
      iterator __start(std::__addressof(*__q), 0);
      iterator __finish(_M_copy_aligned(begin(), end(), __start));
      this->_M_deallocate();
      this->_M_impl._M_start = __start;
      this->_M_impl._M_finish = __finish;
      this->_M_impl._M_end_of_storage = __q + _S_nword(__n);
    }

  template<typename _Alloc>
   
    void
    vector<bool, _Alloc>::
    _M_fill_insert(iterator __position, size_type __n, bool __x)
    {
      if (__n == 0)
 return;
      if (capacity() - size() >= __n)
 {
   std::copy_backward(__position, end(),
        this->_M_impl._M_finish + difference_type(__n));
   std::fill(__position, __position + difference_type(__n), __x);
   this->_M_impl._M_finish += difference_type(__n);
 }
      else
 {
   const size_type __len =
     _M_check_len(__n, "vector<bool>::_M_fill_insert");
   _Bit_pointer __q = this->_M_allocate(__len);
   iterator __start(std::__addressof(*__q), 0);
   iterator __i = _M_copy_aligned(begin(), __position, __start);
   std::fill(__i, __i + difference_type(__n), __x);
   iterator __finish = std::copy(__position, end(),
     __i + difference_type(__n));
   this->_M_deallocate();
   this->_M_impl._M_end_of_storage = __q + _S_nword(__len);
   this->_M_impl._M_start = __start;
   this->_M_impl._M_finish = __finish;
 }
    }

  template<typename _Alloc>
    template<typename _ForwardIterator>
     
      void
      vector<bool, _Alloc>::
      _M_insert_range(iterator __position, _ForwardIterator __first,
        _ForwardIterator __last, std::forward_iterator_tag)
      {
 if (__first != __last)
   {
     size_type __n = std::distance(__first, __last);
     if (capacity() - size() >= __n)
       {
  std::copy_backward(__position, end(),
       this->_M_impl._M_finish
       + difference_type(__n));
  std::copy(__first, __last, __position);
  this->_M_impl._M_finish += difference_type(__n);
       }
     else
       {
  const size_type __len =
    _M_check_len(__n, "vector<bool>::_M_insert_range");
  const iterator __begin = begin(), __end = end();
  _Bit_pointer __q = this->_M_allocate(__len);
  iterator __start(std::__addressof(*__q), 0);
  iterator __i = _M_copy_aligned(__begin, __position, __start);
  __i = std::copy(__first, __last, __i);
  iterator __finish = std::copy(__position, __end, __i);
  this->_M_deallocate();
  this->_M_impl._M_end_of_storage = __q + _S_nword(__len);
  this->_M_impl._M_start = __start;
  this->_M_impl._M_finish = __finish;
       }
   }
      }

  template<typename _Alloc>
   
    void
    vector<bool, _Alloc>::
    _M_insert_aux(iterator __position, bool __x)
    {
      if (this->_M_impl._M_finish._M_p != this->_M_impl._M_end_addr())
 {
   std::copy_backward(__position, this->_M_impl._M_finish,
        this->_M_impl._M_finish + 1);
   *__position = __x;
   ++this->_M_impl._M_finish;
 }
      else
 {
   const size_type __len =
     _M_check_len(size_type(1), "vector<bool>::_M_insert_aux");
   _Bit_pointer __q = this->_M_allocate(__len);
   iterator __start(std::__addressof(*__q), 0);
   iterator __i = _M_copy_aligned(begin(), __position, __start);
   *__i++ = __x;
   iterator __finish = std::copy(__position, end(), __i);
   this->_M_deallocate();
   this->_M_impl._M_end_of_storage = __q + _S_nword(__len);
   this->_M_impl._M_start = __start;
   this->_M_impl._M_finish = __finish;
 }
    }

  template<typename _Alloc>
   
    typename vector<bool, _Alloc>::iterator
    vector<bool, _Alloc>::
    _M_erase(iterator __position)
    {
      if (__position + 1 != end())
        std::copy(__position + 1, end(), __position);
      --this->_M_impl._M_finish;
      return __position;
    }

  template<typename _Alloc>
   
    typename vector<bool, _Alloc>::iterator
    vector<bool, _Alloc>::
    _M_erase(iterator __first, iterator __last)
    {
      if (__first != __last)
 _M_erase_at_end(std::copy(__last, end(), __first));
      return __first;
    }


  template<typename _Alloc>
   
    bool
    vector<bool, _Alloc>::
    _M_shrink_to_fit()
    {
      if (capacity() - size() < int(_S_word_bit))
 return false;
      try
 {
   if (size_type __n = size())
     _M_reallocate(__n);
   else
     {
       this->_M_deallocate();
       this->_M_impl._M_reset();
     }
   return true;
 }
      catch(...)
 { return false; }
    }




}



namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _Alloc>
    size_t
    hash<std::vector<bool, _Alloc>>::
    operator()(const std::vector<bool, _Alloc>& __b) const noexcept
    {
      size_t __hash = 0;
      const size_t __words = __b.size() / _S_word_bit;
      if (__words)
 {
   const size_t __clength = __words * sizeof(_Bit_type);
   __hash = std::_Hash_impl::hash(__b._M_impl._M_start._M_p, __clength);
 }

      const size_t __extrabits = __b.size() % _S_word_bit;
      if (__extrabits)
 {
   _Bit_type __hiword = *__b._M_impl._M_finish._M_p;
   __hiword &= ~((~static_cast<_Bit_type>(0)) << __extrabits);

   const size_t __clength
     = (__extrabits + 8 - 1) / 8;
   if (__words)
     __hash = std::_Hash_impl::hash(&__hiword, __clength, __hash);
   else
     __hash = std::_Hash_impl::hash(&__hiword, __clength);
 }

      return __hash;
    }


}
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 87 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 88 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 7 "./opt/pr102970.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/algorithm" 1 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/algorithm" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 1 3
# 59 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/algorithmfwd.h" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/algorithmfwd.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{

# 200 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/algorithmfwd.h" 3
  template<typename _IIter, typename _Predicate>
   
    bool
    all_of(_IIter, _IIter, _Predicate);

  template<typename _IIter, typename _Predicate>
   
    bool
    any_of(_IIter, _IIter, _Predicate);


  template<typename _FIter, typename _Tp >
   
    bool
    binary_search(_FIter, _FIter, const _Tp&);

  template<typename _FIter, typename _Tp ,
    typename _Compare>
   
    bool
    binary_search(_FIter, _FIter, const _Tp&, _Compare);
# 234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/algorithmfwd.h" 3
  template<typename _IIter, typename _OIter>
   
    _OIter
    copy(_IIter, _IIter, _OIter);

  template<typename _BIter1, typename _BIter2>
   
    _BIter2
    copy_backward(_BIter1, _BIter1, _BIter2);


  template<typename _IIter, typename _OIter, typename _Predicate>
   
    _OIter
    copy_if(_IIter, _IIter, _OIter, _Predicate);

  template<typename _IIter, typename _Size, typename _OIter>
   
    _OIter
    copy_n(_IIter, _Size, _OIter);





  template<typename _FIter, typename _Tp >
   
    pair<_FIter, _FIter>
    equal_range(_FIter, _FIter, const _Tp&);

  template<typename _FIter, typename _Tp ,
    typename _Compare>
   
    pair<_FIter, _FIter>
    equal_range(_FIter, _FIter, const _Tp&, _Compare);

  template<typename _FIter, typename _Tp >
   
    void
    fill(_FIter, _FIter, const _Tp&);

  template<typename _OIter, typename _Size,
    typename _Tp >
   
    _OIter
    fill_n(_OIter, _Size, const _Tp&);



  template<typename _FIter1, typename _FIter2>
   
    _FIter1
    find_end(_FIter1, _FIter1, _FIter2, _FIter2);

  template<typename _FIter1, typename _FIter2, typename _BinaryPredicate>
   
    _FIter1
    find_end(_FIter1, _FIter1, _FIter2, _FIter2, _BinaryPredicate);





  template<typename _IIter, typename _Predicate>
   
    _IIter
    find_if_not(_IIter, _IIter, _Predicate);






  template<typename _IIter1, typename _IIter2>
   
    bool
    includes(_IIter1, _IIter1, _IIter2, _IIter2);

  template<typename _IIter1, typename _IIter2, typename _Compare>
   
    bool
    includes(_IIter1, _IIter1, _IIter2, _IIter2, _Compare);

  template<typename _BIter>
    void
    inplace_merge(_BIter, _BIter, _BIter);

  template<typename _BIter, typename _Compare>
    void
    inplace_merge(_BIter, _BIter, _BIter, _Compare);


  template<typename _RAIter>
   
    bool
    is_heap(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    bool
    is_heap(_RAIter, _RAIter, _Compare);

  template<typename _RAIter>
   
    _RAIter
    is_heap_until(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    _RAIter
    is_heap_until(_RAIter, _RAIter, _Compare);

  template<typename _IIter, typename _Predicate>
   
    bool
    is_partitioned(_IIter, _IIter, _Predicate);

  template<typename _FIter1, typename _FIter2>
   
    bool
    is_permutation(_FIter1, _FIter1, _FIter2);

  template<typename _FIter1, typename _FIter2,
    typename _BinaryPredicate>
   
    bool
    is_permutation(_FIter1, _FIter1, _FIter2, _BinaryPredicate);

  template<typename _FIter>
   
    bool
    is_sorted(_FIter, _FIter);

  template<typename _FIter, typename _Compare>
   
    bool
    is_sorted(_FIter, _FIter, _Compare);

  template<typename _FIter>
   
    _FIter
    is_sorted_until(_FIter, _FIter);

  template<typename _FIter, typename _Compare>
   
    _FIter
    is_sorted_until(_FIter, _FIter, _Compare);


  template<typename _FIter1, typename _FIter2>
   
    void
    iter_swap(_FIter1, _FIter2);

  template<typename _FIter, typename _Tp >
   
    _FIter
    lower_bound(_FIter, _FIter, const _Tp&);

  template<typename _FIter, typename _Tp ,
    typename _Compare>
   
    _FIter
    lower_bound(_FIter, _FIter, const _Tp&, _Compare);

  template<typename _RAIter>
   
    void
    make_heap(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    make_heap(_RAIter, _RAIter, _Compare);

  template<typename _Tp>
    constexpr
    const _Tp&
    max(const _Tp&, const _Tp&);

  template<typename _Tp, typename _Compare>
    constexpr
    const _Tp&
    max(const _Tp&, const _Tp&, _Compare);




  template<typename _Tp>
    constexpr
    const _Tp&
    min(const _Tp&, const _Tp&);

  template<typename _Tp, typename _Compare>
    constexpr
    const _Tp&
    min(const _Tp&, const _Tp&, _Compare);




  template<typename _Tp>
    constexpr
    pair<const _Tp&, const _Tp&>
    minmax(const _Tp&, const _Tp&);

  template<typename _Tp, typename _Compare>
    constexpr
    pair<const _Tp&, const _Tp&>
    minmax(const _Tp&, const _Tp&, _Compare);

  template<typename _FIter>
    constexpr
    pair<_FIter, _FIter>
    minmax_element(_FIter, _FIter);

  template<typename _FIter, typename _Compare>
    constexpr
    pair<_FIter, _FIter>
    minmax_element(_FIter, _FIter, _Compare);

  template<typename _Tp>
    constexpr
    _Tp
    min(initializer_list<_Tp>);

  template<typename _Tp, typename _Compare>
    constexpr
    _Tp
    min(initializer_list<_Tp>, _Compare);

  template<typename _Tp>
    constexpr
    _Tp
    max(initializer_list<_Tp>);

  template<typename _Tp, typename _Compare>
    constexpr
    _Tp
    max(initializer_list<_Tp>, _Compare);

  template<typename _Tp>
    constexpr
    pair<_Tp, _Tp>
    minmax(initializer_list<_Tp>);

  template<typename _Tp, typename _Compare>
    constexpr
    pair<_Tp, _Tp>
    minmax(initializer_list<_Tp>, _Compare);




  template<typename _BIter>
   
    bool
    next_permutation(_BIter, _BIter);

  template<typename _BIter, typename _Compare>
   
    bool
    next_permutation(_BIter, _BIter, _Compare);


  template<typename _IIter, typename _Predicate>
   
    bool
    none_of(_IIter, _IIter, _Predicate);





  template<typename _IIter, typename _RAIter>
   
    _RAIter
    partial_sort_copy(_IIter, _IIter, _RAIter, _RAIter);

  template<typename _IIter, typename _RAIter, typename _Compare>
   
    _RAIter
    partial_sort_copy(_IIter, _IIter, _RAIter, _RAIter, _Compare);




  template<typename _IIter, typename _OIter1,
    typename _OIter2, typename _Predicate>
   
    pair<_OIter1, _OIter2>
    partition_copy(_IIter, _IIter, _OIter1, _OIter2, _Predicate);

  template<typename _FIter, typename _Predicate>
   
    _FIter
    partition_point(_FIter, _FIter, _Predicate);


  template<typename _RAIter>
   
    void
    pop_heap(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    pop_heap(_RAIter, _RAIter, _Compare);

  template<typename _BIter>
   
    bool
    prev_permutation(_BIter, _BIter);

  template<typename _BIter, typename _Compare>
   
    bool
    prev_permutation(_BIter, _BIter, _Compare);

  template<typename _RAIter>
   
    void
    push_heap(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    push_heap(_RAIter, _RAIter, _Compare);



  template<typename _FIter, typename _Tp >
   
    _FIter
    remove(_FIter, _FIter, const _Tp&);

  template<typename _FIter, typename _Predicate>
   
    _FIter
    remove_if(_FIter, _FIter, _Predicate);

  template<typename _IIter, typename _OIter,
    typename _Tp >
   
    _OIter
    remove_copy(_IIter, _IIter, _OIter, const _Tp&);

  template<typename _IIter, typename _OIter, typename _Predicate>
   
    _OIter
    remove_copy_if(_IIter, _IIter, _OIter, _Predicate);



  template<typename _IIter, typename _OIter, typename _Tp>
   
    _OIter
    replace_copy(_IIter, _IIter, _OIter, const _Tp&, const _Tp&);

  template<typename _Iter, typename _OIter, typename _Predicate,
    typename _Tp >
   
    _OIter
    replace_copy_if(_Iter, _Iter, _OIter, _Predicate, const _Tp&);



  template<typename _BIter>
   
    void
    reverse(_BIter, _BIter);

  template<typename _BIter, typename _OIter>
   
    _OIter
    reverse_copy(_BIter, _BIter, _OIter);

inline namespace _V2 {

  template<typename _FIter>
   
    _FIter
    rotate(_FIter, _FIter, _FIter);

}

  template<typename _FIter, typename _OIter>
   
    _OIter
    rotate_copy(_FIter, _FIter, _FIter, _OIter);
# 633 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/algorithmfwd.h" 3
  template<typename _RAIter, typename _UGenerator>
    void
    shuffle(_RAIter, _RAIter, _UGenerator&&);


  template<typename _RAIter>
   
    void
    sort_heap(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    sort_heap(_RAIter, _RAIter, _Compare);


  template<typename _BIter, typename _Predicate>
    _BIter
    stable_partition(_BIter, _BIter, _Predicate);
# 668 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/algorithmfwd.h" 3
  template<typename _FIter1, typename _FIter2>
   
    _FIter2
    swap_ranges(_FIter1, _FIter1, _FIter2);



  template<typename _FIter>
   
    _FIter
    unique(_FIter, _FIter);

  template<typename _FIter, typename _BinaryPredicate>
   
    _FIter
    unique(_FIter, _FIter, _BinaryPredicate);



  template<typename _FIter, typename _Tp >
   
    _FIter
    upper_bound(_FIter, _FIter, const _Tp&);

  template<typename _FIter, typename _Tp ,
    typename _Compare>
   
    _FIter
    upper_bound(_FIter, _FIter, const _Tp&, _Compare);



  template<typename _FIter>
   
    _FIter
    adjacent_find(_FIter, _FIter);

  template<typename _FIter, typename _BinaryPredicate>
   
    _FIter
    adjacent_find(_FIter, _FIter, _BinaryPredicate);

  template<typename _IIter, typename _Tp >
   
    typename iterator_traits<_IIter>::difference_type
    count(_IIter, _IIter, const _Tp&);

  template<typename _IIter, typename _Predicate>
   
    typename iterator_traits<_IIter>::difference_type
    count_if(_IIter, _IIter, _Predicate);

  template<typename _IIter1, typename _IIter2>
   
    bool
    equal(_IIter1, _IIter1, _IIter2);

  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
   
    bool
    equal(_IIter1, _IIter1, _IIter2, _BinaryPredicate);

  template<typename _IIter, typename _Tp >
   
    _IIter
    find(_IIter, _IIter, const _Tp&);

  template<typename _FIter1, typename _FIter2>
   
    _FIter1
    find_first_of(_FIter1, _FIter1, _FIter2, _FIter2);

  template<typename _FIter1, typename _FIter2, typename _BinaryPredicate>
   
    _FIter1
    find_first_of(_FIter1, _FIter1, _FIter2, _FIter2, _BinaryPredicate);

  template<typename _IIter, typename _Predicate>
   
    _IIter
    find_if(_IIter, _IIter, _Predicate);

  template<typename _IIter, typename _Funct>
   
    _Funct
    for_each(_IIter, _IIter, _Funct);

  template<typename _FIter, typename _Generator>
   
    void
    generate(_FIter, _FIter, _Generator);

  template<typename _OIter, typename _Size, typename _Generator>
   
    _OIter
    generate_n(_OIter, _Size, _Generator);

  template<typename _IIter1, typename _IIter2>
   
    bool
    lexicographical_compare(_IIter1, _IIter1, _IIter2, _IIter2);

  template<typename _IIter1, typename _IIter2, typename _Compare>
   
    bool
    lexicographical_compare(_IIter1, _IIter1, _IIter2, _IIter2, _Compare);

  template<typename _FIter>
    constexpr
    _FIter
    max_element(_FIter, _FIter);

  template<typename _FIter, typename _Compare>
    constexpr
    _FIter
    max_element(_FIter, _FIter, _Compare);

  template<typename _IIter1, typename _IIter2, typename _OIter>
   
    _OIter
    merge(_IIter1, _IIter1, _IIter2, _IIter2, _OIter);

  template<typename _IIter1, typename _IIter2, typename _OIter,
    typename _Compare>
   
    _OIter
    merge(_IIter1, _IIter1, _IIter2, _IIter2, _OIter, _Compare);

  template<typename _FIter>
    constexpr
    _FIter
    min_element(_FIter, _FIter);

  template<typename _FIter, typename _Compare>
    constexpr
    _FIter
    min_element(_FIter, _FIter, _Compare);

  template<typename _IIter1, typename _IIter2>
   
    pair<_IIter1, _IIter2>
    mismatch(_IIter1, _IIter1, _IIter2);

  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
   
    pair<_IIter1, _IIter2>
    mismatch(_IIter1, _IIter1, _IIter2, _BinaryPredicate);

  template<typename _RAIter>
   
    void
    nth_element(_RAIter, _RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    nth_element(_RAIter, _RAIter, _RAIter, _Compare);

  template<typename _RAIter>
   
    void
    partial_sort(_RAIter, _RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    partial_sort(_RAIter, _RAIter, _RAIter, _Compare);

  template<typename _BIter, typename _Predicate>
   
    _BIter
    partition(_BIter, _BIter, _Predicate);


  template<typename _RAIter>
    __attribute__ ((__deprecated__ ("use '" "std::shuffle" "' instead")))
    void
    random_shuffle(_RAIter, _RAIter);

  template<typename _RAIter, typename _Generator>
    __attribute__ ((__deprecated__ ("use '" "std::shuffle" "' instead")))
    void
    random_shuffle(_RAIter, _RAIter,

     _Generator&&);





  template<typename _FIter, typename _Tp >
   
    void
    replace(_FIter, _FIter, const _Tp&, const _Tp&);

  template<typename _FIter, typename _Predicate,
    typename _Tp >
   
    void
    replace_if(_FIter, _FIter, _Predicate, const _Tp&);

  template<typename _FIter1, typename _FIter2>
   
    _FIter1
    search(_FIter1, _FIter1, _FIter2, _FIter2);

  template<typename _FIter1, typename _FIter2, typename _BinaryPredicate>
   
    _FIter1
    search(_FIter1, _FIter1, _FIter2, _FIter2, _BinaryPredicate);

  template<typename _FIter, typename _Size,
    typename _Tp >
   
    _FIter
    search_n(_FIter, _FIter, _Size, const _Tp&);

  template<typename _FIter, typename _Size,
    typename _Tp ,
    typename _BinaryPredicate>
   
    _FIter
    search_n(_FIter, _FIter, _Size, const _Tp&, _BinaryPredicate);

  template<typename _IIter1, typename _IIter2, typename _OIter>
   
    _OIter
    set_difference(_IIter1, _IIter1, _IIter2, _IIter2, _OIter);

  template<typename _IIter1, typename _IIter2, typename _OIter,
    typename _Compare>
   
    _OIter
    set_difference(_IIter1, _IIter1, _IIter2, _IIter2, _OIter, _Compare);

  template<typename _IIter1, typename _IIter2, typename _OIter>
   
    _OIter
    set_intersection(_IIter1, _IIter1, _IIter2, _IIter2, _OIter);

  template<typename _IIter1, typename _IIter2, typename _OIter,
    typename _Compare>
   
    _OIter
    set_intersection(_IIter1, _IIter1, _IIter2, _IIter2, _OIter, _Compare);

  template<typename _IIter1, typename _IIter2, typename _OIter>
   
    _OIter
    set_symmetric_difference(_IIter1, _IIter1, _IIter2, _IIter2, _OIter);

  template<typename _IIter1, typename _IIter2, typename _OIter,
    typename _Compare>
   
    _OIter
    set_symmetric_difference(_IIter1, _IIter1, _IIter2, _IIter2,
        _OIter, _Compare);

  template<typename _IIter1, typename _IIter2, typename _OIter>
   
    _OIter
    set_union(_IIter1, _IIter1, _IIter2, _IIter2, _OIter);

  template<typename _IIter1, typename _IIter2, typename _OIter,
    typename _Compare>
   
    _OIter
    set_union(_IIter1, _IIter1, _IIter2, _IIter2, _OIter, _Compare);

  template<typename _RAIter>
   
    void
    sort(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
   
    void
    sort(_RAIter, _RAIter, _Compare);

  template<typename _RAIter>
    void
    stable_sort(_RAIter, _RAIter);

  template<typename _RAIter, typename _Compare>
    void
    stable_sort(_RAIter, _RAIter, _Compare);

  template<typename _IIter, typename _OIter, typename _UnaryOperation>
   
    _OIter
    transform(_IIter, _IIter, _OIter, _UnaryOperation);

  template<typename _IIter1, typename _IIter2, typename _OIter,
    typename _BinaryOperation>
   
    _OIter
    transform(_IIter1, _IIter1, _IIter2, _OIter, _BinaryOperation);

  template<typename _IIter, typename _OIter>
   
    _OIter
    unique_copy(_IIter, _IIter, _OIter);

  template<typename _IIter, typename _OIter, typename _BinaryPredicate>
   
    _OIter
    unique_copy(_IIter, _IIter, _OIter, _BinaryPredicate);



}

#pragma GCC diagnostic pop
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 1 3
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _RandomAccessIterator, typename _Distance,
    typename _Compare>
   
    _Distance
    __is_heap_until(_RandomAccessIterator __first, _Distance __n,
      _Compare& __comp)
    {
      _Distance __parent = 0;
      for (_Distance __child = 1; __child < __n; ++__child)
 {
   if (__comp(__first + __parent, __first + __child))
     return __child;
   if ((__child & 1) == 0)
     ++__parent;
 }
      return __n;
    }



  template<typename _RandomAccessIterator, typename _Distance>
   
    inline bool
    __is_heap(_RandomAccessIterator __first, _Distance __n)
    {
      __gnu_cxx::__ops::_Iter_less_iter __comp;
      return std::__is_heap_until(__first, __n, __comp) == __n;
    }

  template<typename _RandomAccessIterator, typename _Compare,
    typename _Distance>
   
    inline bool
    __is_heap(_RandomAccessIterator __first, _Compare __comp, _Distance __n)
    {
      typedef __decltype(__comp) _Cmp;
      __gnu_cxx::__ops::_Iter_comp_iter<_Cmp> __cmp(std::move(__comp));
      return std::__is_heap_until(__first, __n, __cmp) == __n;
    }

  template<typename _RandomAccessIterator>
   
    inline bool
    __is_heap(_RandomAccessIterator __first, _RandomAccessIterator __last)
    { return std::__is_heap(__first, std::distance(__first, __last)); }

  template<typename _RandomAccessIterator, typename _Compare>
   
    inline bool
    __is_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
       _Compare __comp)
    {
      return std::__is_heap(__first, std::move(__comp),
       std::distance(__first, __last));
    }




  template<typename _RandomAccessIterator, typename _Distance, typename _Tp,
    typename _Compare>
   
    void
    __push_heap(_RandomAccessIterator __first,
  _Distance __holeIndex, _Distance __topIndex, _Tp __value,
  _Compare& __comp)
    {
      _Distance __parent = (__holeIndex - 1) / 2;
      while (__holeIndex > __topIndex && __comp(__first + __parent, __value))
 {
   *(__first + __holeIndex) = std::move(*(__first + __parent));
   __holeIndex = __parent;
   __parent = (__holeIndex - 1) / 2;
 }
      *(__first + __holeIndex) = std::move(__value);
    }
# 159 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    push_heap(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {
      typedef typename iterator_traits<_RandomAccessIterator>::value_type
   _ValueType;
      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
   _DistanceType;


     

     
      ;
      ;
      ;

      __gnu_cxx::__ops::_Iter_less_val __comp;
      _ValueType __value = std::move(*(__last - 1));
      std::__push_heap(__first, _DistanceType((__last - __first) - 1),
         _DistanceType(0), std::move(__value), __comp);
    }
# 195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    push_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
       _Compare __comp)
    {
      typedef typename iterator_traits<_RandomAccessIterator>::value_type
   _ValueType;
      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
   _DistanceType;


     

      ;
      ;
      ;

      __decltype(__gnu_cxx::__ops::__iter_comp_val(std::move(__comp)))
 __cmp(std::move(__comp));
      _ValueType __value = std::move(*(__last - 1));
      std::__push_heap(__first, _DistanceType((__last - __first) - 1),
         _DistanceType(0), std::move(__value), __cmp);
    }

  template<typename _RandomAccessIterator, typename _Distance,
    typename _Tp, typename _Compare>
   
    void
    __adjust_heap(_RandomAccessIterator __first, _Distance __holeIndex,
    _Distance __len, _Tp __value, _Compare __comp)
    {
      const _Distance __topIndex = __holeIndex;
      _Distance __secondChild = __holeIndex;
      while (__secondChild < (__len - 1) / 2)
 {
   __secondChild = 2 * (__secondChild + 1);
   if (__comp(__first + __secondChild,
       __first + (__secondChild - 1)))
     __secondChild--;
   *(__first + __holeIndex) = std::move(*(__first + __secondChild));
   __holeIndex = __secondChild;
 }
      if ((__len & 1) == 0 && __secondChild == (__len - 2) / 2)
 {
   __secondChild = 2 * (__secondChild + 1);
   *(__first + __holeIndex) = std::move(*(__first + (__secondChild - 1)))
                                  ;
   __holeIndex = __secondChild - 1;
 }
      __decltype(__gnu_cxx::__ops::__iter_comp_val(std::move(__comp)))
 __cmp(std::move(__comp));
      std::__push_heap(__first, __holeIndex, __topIndex,
         std::move(__value), __cmp);
    }

  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    __pop_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
        _RandomAccessIterator __result, _Compare& __comp)
    {
      typedef typename iterator_traits<_RandomAccessIterator>::value_type
 _ValueType;
      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
 _DistanceType;

      _ValueType __value = std::move(*__result);
      *__result = std::move(*__first);
      std::__adjust_heap(__first, _DistanceType(0),
    _DistanceType(__last - __first),
    std::move(__value), __comp);
    }
# 280 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    pop_heap(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

     

      do { if (__builtin_expect(!bool(__first != __last), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h", 290, __PRETTY_FUNCTION__, "__first != __last"); } while (false);
      ;
      ;
      ;

      if (__last - __first > 1)
 {
   --__last;
   __gnu_cxx::__ops::_Iter_less_iter __comp;
   std::__pop_heap(__first, __last, __last, __comp);
 }
    }
# 314 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    pop_heap(_RandomAccessIterator __first,
      _RandomAccessIterator __last, _Compare __comp)
    {

     

      ;
      ;
      do { if (__builtin_expect(!bool(__first != __last), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h", 325, __PRETTY_FUNCTION__, "__first != __last"); } while (false);
      ;

      if (__last - __first > 1)
 {
   typedef __decltype(__comp) _Cmp;
   __gnu_cxx::__ops::_Iter_comp_iter<_Cmp> __cmp(std::move(__comp));
   --__last;
   std::__pop_heap(__first, __last, __last, __cmp);
 }
    }

  template<typename _RandomAccessIterator, typename _Compare>
   
    void
    __make_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
  _Compare& __comp)
    {
      typedef typename iterator_traits<_RandomAccessIterator>::value_type
   _ValueType;
      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
   _DistanceType;

      if (__last - __first < 2)
 return;

      const _DistanceType __len = __last - __first;
      _DistanceType __parent = (__len - 2) / 2;
      while (true)
 {
   _ValueType __value = std::move(*(__first + __parent));
   std::__adjust_heap(__first, __parent, __len, std::move(__value),
        __comp);
   if (__parent == 0)
     return;
   __parent--;
 }
    }
# 372 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    make_heap(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

     

      ;
      ;

      __gnu_cxx::__ops::_Iter_less_iter __comp;
      std::__make_heap(__first, __last, __comp);
    }
# 399 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    make_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
       _Compare __comp)
    {

     

      ;
      ;

      typedef __decltype(__comp) _Cmp;
      __gnu_cxx::__ops::_Iter_comp_iter<_Cmp> __cmp(std::move(__comp));
      std::__make_heap(__first, __last, __cmp);
    }

  template<typename _RandomAccessIterator, typename _Compare>
   
    void
    __sort_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
  _Compare& __comp)
    {
      while (__last - __first > 1)
 {
   --__last;
   std::__pop_heap(__first, __last, __last, __comp);
 }
    }
# 437 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    sort_heap(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

     

      ;
      ;
      ;

      __gnu_cxx::__ops::_Iter_less_iter __comp;
      std::__sort_heap(__first, __last, __comp);
    }
# 465 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    sort_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
       _Compare __comp)
    {

     

      ;
      ;
      ;

      typedef __decltype(__comp) _Cmp;
      __gnu_cxx::__ops::_Iter_comp_iter<_Cmp> __cmp(std::move(__comp));
      std::__sort_heap(__first, __last, __cmp);
    }
# 494 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator>
    [[__nodiscard__]]
    inline _RandomAccessIterator
    is_heap_until(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

     

      ;
      ;

      __gnu_cxx::__ops::_Iter_less_iter __comp;
      return __first +
 std::__is_heap_until(__first, std::distance(__first, __last), __comp);
    }
# 523 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
    [[__nodiscard__]]
    inline _RandomAccessIterator
    is_heap_until(_RandomAccessIterator __first, _RandomAccessIterator __last,
    _Compare __comp)
    {

     

      ;
      ;

      typedef __decltype(__comp) _Cmp;
      __gnu_cxx::__ops::_Iter_comp_iter<_Cmp> __cmp(std::move(__comp));
      return __first
 + std::__is_heap_until(__first, std::distance(__first, __last), __cmp);
    }
# 548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator>
    [[__nodiscard__]]
    inline bool
    is_heap(_RandomAccessIterator __first, _RandomAccessIterator __last)
    { return std::is_heap_until(__first, __last) == __last; }
# 562 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_heap.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
    [[__nodiscard__]]
    inline bool
    is_heap(_RandomAccessIterator __first, _RandomAccessIterator __last,
     _Compare __comp)
    {

     

      ;
      ;

      const auto __dist = std::distance(__first, __last);
      typedef __decltype(__comp) _Cmp;
      __gnu_cxx::__ops::_Iter_comp_iter<_Cmp> __cmp(std::move(__comp));
      return std::__is_heap_until(__first, __dist, __cmp) == __dist;
    }



}
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 2 3



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h" 3
  namespace __detail
  {



    template<typename _Tp>
      constexpr bool
      _Power_of_2(_Tp __x)
      {
 return ((__x - 1) & __x) == 0;
      }
  }
# 87 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h" 3
  template<typename _IntType = int>
    class uniform_int_distribution
    {
      static_assert(std::is_integral<_IntType>::value,
      "template argument must be an integral type");

    public:

      typedef _IntType result_type;

      struct param_type
      {
 typedef uniform_int_distribution<_IntType> distribution_type;

 param_type() : param_type(0) { }

 explicit
 param_type(_IntType __a,
     _IntType __b = __gnu_cxx::__int_traits<_IntType>::__max)
 : _M_a(__a), _M_b(__b)
 {
   do { if (__builtin_expect(!bool(_M_a <= _M_b), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h", 108, __PRETTY_FUNCTION__, "_M_a <= _M_b"); } while (false);
 }

 result_type
 a() const
 { return _M_a; }

 result_type
 b() const
 { return _M_b; }

 friend bool
 operator==(const param_type& __p1, const param_type& __p2)
 { return __p1._M_a == __p2._M_a && __p1._M_b == __p2._M_b; }

 friend bool
 operator!=(const param_type& __p1, const param_type& __p2)
 { return !(__p1 == __p2); }

      private:
 _IntType _M_a;
 _IntType _M_b;
      };

    public:



      uniform_int_distribution() : uniform_int_distribution(0) { }




      explicit
      uniform_int_distribution(_IntType __a,
          _IntType __b
     = __gnu_cxx::__int_traits<_IntType>::__max)
      : _M_param(__a, __b)
      { }

      explicit
      uniform_int_distribution(const param_type& __p)
      : _M_param(__p)
      { }






      void
      reset() { }

      result_type
      a() const
      { return _M_param.a(); }

      result_type
      b() const
      { return _M_param.b(); }




      param_type
      param() const
      { return _M_param; }





      void
      param(const param_type& __param)
      { _M_param = __param; }




      result_type
      min() const
      { return this->a(); }




      result_type
      max() const
      { return this->b(); }




      template<typename _UniformRandomBitGenerator>
 result_type
 operator()(_UniformRandomBitGenerator& __urng)
        { return this->operator()(__urng, _M_param); }

      template<typename _UniformRandomBitGenerator>
 result_type
 operator()(_UniformRandomBitGenerator& __urng,
     const param_type& __p);

      template<typename _ForwardIterator,
        typename _UniformRandomBitGenerator>
 void
 __generate(_ForwardIterator __f, _ForwardIterator __t,
     _UniformRandomBitGenerator& __urng)
 { this->__generate(__f, __t, __urng, _M_param); }

      template<typename _ForwardIterator,
        typename _UniformRandomBitGenerator>
 void
 __generate(_ForwardIterator __f, _ForwardIterator __t,
     _UniformRandomBitGenerator& __urng,
     const param_type& __p)
 { this->__generate_impl(__f, __t, __urng, __p); }

      template<typename _UniformRandomBitGenerator>
 void
 __generate(result_type* __f, result_type* __t,
     _UniformRandomBitGenerator& __urng,
     const param_type& __p)
 { this->__generate_impl(__f, __t, __urng, __p); }





      friend bool
      operator==(const uniform_int_distribution& __d1,
   const uniform_int_distribution& __d2)
      { return __d1._M_param == __d2._M_param; }

    private:
      template<typename _ForwardIterator,
        typename _UniformRandomBitGenerator>
 void
 __generate_impl(_ForwardIterator __f, _ForwardIterator __t,
   _UniformRandomBitGenerator& __urng,
   const param_type& __p);

      param_type _M_param;




      template<typename _Wp, typename _Urbg, typename _Up>
 static _Up
 _S_nd(_Urbg& __g, _Up __range)
 {
   using _Up_traits = __gnu_cxx::__int_traits<_Up>;
   using _Wp_traits = __gnu_cxx::__int_traits<_Wp>;
   static_assert(!_Up_traits::__is_signed, "U must be unsigned");
   static_assert(!_Wp_traits::__is_signed, "W must be unsigned");
   static_assert(_Wp_traits::__digits == (2 * _Up_traits::__digits),
   "W must be twice as wide as U");




   _Wp __product = _Wp(__g()) * _Wp(__range);
   _Up __low = _Up(__product);
   if (__low < __range)
     {
       _Up __threshold = -__range % __range;
       while (__low < __threshold)
  {
    __product = _Wp(__g()) * _Wp(__range);
    __low = _Up(__product);
  }
     }
   return __product >> _Up_traits::__digits;
 }
    };

  template<typename _IntType>
    template<typename _UniformRandomBitGenerator>
      typename uniform_int_distribution<_IntType>::result_type
      uniform_int_distribution<_IntType>::
      operator()(_UniformRandomBitGenerator& __urng,
   const param_type& __param)
      {
 typedef typename _UniformRandomBitGenerator::result_type _Gresult_type;
 typedef typename make_unsigned<result_type>::type __utype;
 typedef typename common_type<_Gresult_type, __utype>::type __uctype;

 constexpr __uctype __urngmin = _UniformRandomBitGenerator::min();
 constexpr __uctype __urngmax = _UniformRandomBitGenerator::max();
 static_assert( __urngmin < __urngmax,
     "Uniform random bit generator must define min() < max()");
 constexpr __uctype __urngrange = __urngmax - __urngmin;

 const __uctype __urange
   = __uctype(__param.b()) - __uctype(__param.a());

 __uctype __ret;
 if (__urngrange > __urange)
   {


     const __uctype __uerange = __urange + 1;



     if (__urngrange == 0xffffffffffffffffUL)
       {


  long unsigned int __u64erange = __uerange;
  __ret = __extension__ _S_nd<unsigned __int128>(__urng,
              __u64erange);
       }
     else

     if (__urngrange == 0xffffffffU)
       {


  unsigned int __u32erange = __uerange;
  __ret = _S_nd<long unsigned int>(__urng, __u32erange);
       }
     else

       {

  const __uctype __scaling = __urngrange / __uerange;
  const __uctype __past = __uerange * __scaling;
  do
    __ret = __uctype(__urng()) - __urngmin;
  while (__ret >= __past);
  __ret /= __scaling;
       }
   }
 else if (__urngrange < __urange)
   {
# 359 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h" 3
     __uctype __tmp;
     do
       {
  const __uctype __uerngrange = __urngrange + 1;
  __tmp = (__uerngrange * operator()
    (__urng, param_type(0, __urange / __uerngrange)));
  __ret = __tmp + (__uctype(__urng()) - __urngmin);
       }
     while (__ret > __urange || __ret < __tmp);
   }
 else
   __ret = __uctype(__urng()) - __urngmin;

 return __ret + __param.a();
      }


  template<typename _IntType>
    template<typename _ForwardIterator,
      typename _UniformRandomBitGenerator>
      void
      uniform_int_distribution<_IntType>::
      __generate_impl(_ForwardIterator __f, _ForwardIterator __t,
        _UniformRandomBitGenerator& __urng,
        const param_type& __param)
      {

 typedef typename _UniformRandomBitGenerator::result_type _Gresult_type;
 typedef typename make_unsigned<result_type>::type __utype;
 typedef typename common_type<_Gresult_type, __utype>::type __uctype;

 static_assert( __urng.min() < __urng.max(),
     "Uniform random bit generator must define min() < max()");

 constexpr __uctype __urngmin = __urng.min();
 constexpr __uctype __urngmax = __urng.max();
 constexpr __uctype __urngrange = __urngmax - __urngmin;
 const __uctype __urange
   = __uctype(__param.b()) - __uctype(__param.a());

 __uctype __ret;

 if (__urngrange > __urange)
   {
     if (__detail::_Power_of_2(__urngrange + 1)
  && __detail::_Power_of_2(__urange + 1))
       {
  while (__f != __t)
    {
      __ret = __uctype(__urng()) - __urngmin;
      *__f++ = (__ret & __urange) + __param.a();
    }
       }
     else
       {

  const __uctype __uerange = __urange + 1;
  const __uctype __scaling = __urngrange / __uerange;
  const __uctype __past = __uerange * __scaling;
  while (__f != __t)
    {
      do
        __ret = __uctype(__urng()) - __urngmin;
      while (__ret >= __past);
      *__f++ = __ret / __scaling + __param.a();
    }
       }
   }
 else if (__urngrange < __urange)
   {
# 444 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uniform_int_dist.h" 3
     __uctype __tmp;
     while (__f != __t)
       {
  do
    {
      constexpr __uctype __uerngrange = __urngrange + 1;
      __tmp = (__uerngrange * operator()
        (__urng, param_type(0, __urange / __uerngrange)));
      __ret = __tmp + (__uctype(__urng()) - __urngmin);
    }
  while (__ret > __urange || __ret < __tmp);
  *__f++ = __ret;
       }
   }
 else
   while (__f != __t)
     *__f++ = __uctype(__urng()) - __urngmin + __param.a();
      }




}
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 2 3



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 1 3
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 77 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  namespace __detail
  {


    template<typename _Tp>
      inline _Tp*
      __get_temporary_buffer(ptrdiff_t __len) noexcept
      {
 if (__builtin_expect(size_t(__len) > (size_t(-1) / sizeof(_Tp)), 0))
   return 0;







 return (_Tp*) __builtin_operator_new(__len * sizeof(_Tp), nothrow_t());
      }



    template<typename _Tp>
      inline void
      __return_temporary_buffer(_Tp* __p,
    size_t __len __attribute__((__unused__)))
      {
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
 __builtin_operator_delete((__p), (__len) * sizeof(_Tp));
      }

  }
# 140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp>
   
    pair<_Tp*, ptrdiff_t>
    get_temporary_buffer(ptrdiff_t __len) noexcept
    {
      const ptrdiff_t __max =
 __gnu_cxx::__numeric_traits<ptrdiff_t>::__max / sizeof(_Tp);
      if (__len > __max)
 __len = __max;

      while (__len > 0)
 {
   if (_Tp* __tmp = __detail::__get_temporary_buffer<_Tp>(__len))
     return pair<_Tp*, ptrdiff_t>(__tmp, __len);
   __len = __len == 1 ? 0 : ((__len + 1) / 2);
 }
      return pair<_Tp*, ptrdiff_t>();
    }
# 166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp>
   
    inline void
    return_temporary_buffer(_Tp* __p)
    {





      __builtin_operator_delete(__p);
    }
# 187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _ForwardIterator, typename _Tp>
    class _Temporary_buffer
    {

     

    public:
      typedef _Tp value_type;
      typedef value_type* pointer;
      typedef pointer iterator;
      typedef ptrdiff_t size_type;

    protected:
      size_type _M_original_len;
      struct _Impl
      {
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
 explicit
 _Impl(ptrdiff_t __original_len)
 {
   pair<pointer, size_type> __p(
     std::get_temporary_buffer<value_type>(__original_len));
   _M_len = __p.second;
   _M_buffer = __p.first;
 }
#pragma GCC diagnostic pop

 ~_Impl()
 { std::__detail::__return_temporary_buffer(_M_buffer, _M_len); }

 size_type _M_len;
 pointer _M_buffer;
      } _M_impl;

    public:

      size_type
      size() const
      { return _M_impl._M_len; }


      size_type
      requested_size() const
      { return _M_original_len; }


      iterator
      begin()
      { return _M_impl._M_buffer; }


      iterator
      end()
      { return _M_impl._M_buffer + _M_impl._M_len; }





      _Temporary_buffer(_ForwardIterator __seed, size_type __original_len);

      ~_Temporary_buffer()
      { std::_Destroy(_M_impl._M_buffer, _M_impl._M_buffer + _M_impl._M_len); }

    private:

      _Temporary_buffer(const _Temporary_buffer&);

      void
      operator=(const _Temporary_buffer&);
    };


  template<bool>
    struct __uninitialized_construct_buf_dispatch
    {
      template<typename _Pointer, typename _ForwardIterator>
        static void
        __ucr(_Pointer __first, _Pointer __last,
       _ForwardIterator __seed)
        {
   if (__builtin_expect(__first == __last, 0))
     return;

   _Pointer __cur = __first;
   try
     {
       std::_Construct(std::__addressof(*__first),
         std::move(*__seed));
       _Pointer __prev = __cur;
       ++__cur;
       for(; __cur != __last; ++__cur, ++__prev)
  std::_Construct(std::__addressof(*__cur),
    std::move(*__prev));
       *__seed = std::move(*__prev);
     }
   catch(...)
     {
       std::_Destroy(__first, __cur);
       throw;
     }
 }
    };

  template<>
    struct __uninitialized_construct_buf_dispatch<true>
    {
      template<typename _Pointer, typename _ForwardIterator>
        static void
        __ucr(_Pointer, _Pointer, _ForwardIterator) { }
    };
# 311 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tempbuf.h" 3
  template<typename _Tp, typename _ForwardIterator>
    inline void
    __uninitialized_construct_buf(_Tp* __first, _Tp* __last,
      _ForwardIterator __seed)
    {
      std::__uninitialized_construct_buf_dispatch<
 __has_trivial_constructor(_Tp)>::
   __ucr(__first, __last, __seed);
    }

  template<typename _ForwardIterator, typename _Tp>
    _Temporary_buffer<_ForwardIterator, _Tp>::
    _Temporary_buffer(_ForwardIterator __seed, size_type __original_len)
    : _M_original_len(__original_len), _M_impl(__original_len)
    {
      std::__uninitialized_construct_buf(begin(), end(), __seed);
    }


}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 1 3
# 80 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"

# 1 "/usr/include/stdlib.h" 1 3 4
# 32 "/usr/include/stdlib.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 33 "/usr/include/stdlib.h" 2 3 4

extern "C" {






# 1 "/usr/include/bits/waitflags.h" 1 3 4
# 42 "/usr/include/stdlib.h" 2 3 4
# 1 "/usr/include/bits/waitstatus.h" 1 3 4
# 64 "/usr/include/bits/waitstatus.h" 3 4
# 1 "/usr/include/endian.h" 1 3 4
# 36 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/endian.h" 1 3 4
# 37 "/usr/include/endian.h" 2 3 4
# 60 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/byteswap.h" 1 3 4
# 27 "/usr/include/bits/byteswap.h" 3 4
# 1 "/usr/include/bits/types.h" 1 3 4
# 27 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/bits/types.h" 2 3 4


typedef unsigned char __u_char;
typedef unsigned short int __u_short;
typedef unsigned int __u_int;
typedef unsigned long int __u_long;


typedef signed char __int8_t;
typedef unsigned char __uint8_t;
typedef signed short int __int16_t;
typedef unsigned short int __uint16_t;
typedef signed int __int32_t;
typedef unsigned int __uint32_t;

typedef signed long int __int64_t;
typedef unsigned long int __uint64_t;







typedef long int __quad_t;
typedef unsigned long int __u_quad_t;
# 130 "/usr/include/bits/types.h" 3 4
# 1 "/usr/include/bits/typesizes.h" 1 3 4
# 131 "/usr/include/bits/types.h" 2 3 4


typedef unsigned long int __dev_t;
typedef unsigned int __uid_t;
typedef unsigned int __gid_t;
typedef unsigned long int __ino_t;
typedef unsigned long int __ino64_t;
typedef unsigned int __mode_t;
typedef unsigned long int __nlink_t;
typedef long int __off_t;
typedef long int __off64_t;
typedef int __pid_t;
typedef struct { int __val[2]; } __fsid_t;
typedef long int __clock_t;
typedef unsigned long int __rlim_t;
typedef unsigned long int __rlim64_t;
typedef unsigned int __id_t;
typedef long int __time_t;
typedef unsigned int __useconds_t;
typedef long int __suseconds_t;

typedef int __daddr_t;
typedef int __key_t;


typedef int __clockid_t;


typedef void * __timer_t;


typedef long int __blksize_t;




typedef long int __blkcnt_t;
typedef long int __blkcnt64_t;


typedef unsigned long int __fsblkcnt_t;
typedef unsigned long int __fsblkcnt64_t;


typedef unsigned long int __fsfilcnt_t;
typedef unsigned long int __fsfilcnt64_t;


typedef long int __fsword_t;

typedef long int __ssize_t;


typedef long int __syscall_slong_t;

typedef unsigned long int __syscall_ulong_t;



typedef __off64_t __loff_t;
typedef __quad_t *__qaddr_t;
typedef char *__caddr_t;


typedef long int __intptr_t;


typedef unsigned int __socklen_t;
# 28 "/usr/include/bits/byteswap.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 29 "/usr/include/bits/byteswap.h" 2 3 4






# 1 "/usr/include/bits/byteswap-16.h" 1 3 4
# 36 "/usr/include/bits/byteswap.h" 2 3 4
# 44 "/usr/include/bits/byteswap.h" 3 4
static __inline unsigned int
__bswap_32 (unsigned int __bsx)
{
  return __builtin_bswap32 (__bsx);
}
# 108 "/usr/include/bits/byteswap.h" 3 4
static __inline __uint64_t
__bswap_64 (__uint64_t __bsx)
{
  return __builtin_bswap64 (__bsx);
}
# 61 "/usr/include/endian.h" 2 3 4
# 65 "/usr/include/bits/waitstatus.h" 2 3 4

union wait
  {
    int w_status;
    struct
      {

 unsigned int __w_termsig:7;
 unsigned int __w_coredump:1;
 unsigned int __w_retcode:8;
 unsigned int:16;







      } __wait_terminated;
    struct
      {

 unsigned int __w_stopval:8;
 unsigned int __w_stopsig:8;
 unsigned int:16;






      } __wait_stopped;
  };
# 43 "/usr/include/stdlib.h" 2 3 4
# 95 "/usr/include/stdlib.h" 3 4


typedef struct
  {
    int quot;
    int rem;
  } div_t;



typedef struct
  {
    long int quot;
    long int rem;
  } ldiv_t;







__extension__ typedef struct
  {
    long long int quot;
    long long int rem;
  } lldiv_t;


# 139 "/usr/include/stdlib.h" 3 4
extern size_t __ctype_get_mb_cur_max (void) throw () ;




extern double atof (const char *__nptr)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;

extern int atoi (const char *__nptr)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;

extern long int atol (const char *__nptr)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;





__extension__ extern long long int atoll (const char *__nptr)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;





extern double strtod (const char *__restrict __nptr,
        char **__restrict __endptr)
     throw () __attribute__ ((__nonnull__ (1)));





extern float strtof (const char *__restrict __nptr,
       char **__restrict __endptr) throw () __attribute__ ((__nonnull__ (1)));

extern long double strtold (const char *__restrict __nptr,
       char **__restrict __endptr)
     throw () __attribute__ ((__nonnull__ (1)));





extern long int strtol (const char *__restrict __nptr,
   char **__restrict __endptr, int __base)
     throw () __attribute__ ((__nonnull__ (1)));

extern unsigned long int strtoul (const char *__restrict __nptr,
      char **__restrict __endptr, int __base)
     throw () __attribute__ ((__nonnull__ (1)));




__extension__
extern long long int strtoq (const char *__restrict __nptr,
        char **__restrict __endptr, int __base)
     throw () __attribute__ ((__nonnull__ (1)));

__extension__
extern unsigned long long int strtouq (const char *__restrict __nptr,
           char **__restrict __endptr, int __base)
     throw () __attribute__ ((__nonnull__ (1)));





__extension__
extern long long int strtoll (const char *__restrict __nptr,
         char **__restrict __endptr, int __base)
     throw () __attribute__ ((__nonnull__ (1)));

__extension__
extern unsigned long long int strtoull (const char *__restrict __nptr,
     char **__restrict __endptr, int __base)
     throw () __attribute__ ((__nonnull__ (1)));

# 235 "/usr/include/stdlib.h" 3 4
# 1 "/usr/include/xlocale.h" 1 3 4
# 27 "/usr/include/xlocale.h" 3 4
typedef struct __locale_struct
{

  struct __locale_data *__locales[13];


  const unsigned short int *__ctype_b;
  const int *__ctype_tolower;
  const int *__ctype_toupper;


  const char *__names[13];
} *__locale_t;


typedef __locale_t locale_t;
# 236 "/usr/include/stdlib.h" 2 3 4



extern long int strtol_l (const char *__restrict __nptr,
     char **__restrict __endptr, int __base,
     __locale_t __loc) throw () __attribute__ ((__nonnull__ (1, 4)));

extern unsigned long int strtoul_l (const char *__restrict __nptr,
        char **__restrict __endptr,
        int __base, __locale_t __loc)
     throw () __attribute__ ((__nonnull__ (1, 4)));

__extension__
extern long long int strtoll_l (const char *__restrict __nptr,
    char **__restrict __endptr, int __base,
    __locale_t __loc)
     throw () __attribute__ ((__nonnull__ (1, 4)));

__extension__
extern unsigned long long int strtoull_l (const char *__restrict __nptr,
       char **__restrict __endptr,
       int __base, __locale_t __loc)
     throw () __attribute__ ((__nonnull__ (1, 4)));

extern double strtod_l (const char *__restrict __nptr,
   char **__restrict __endptr, __locale_t __loc)
     throw () __attribute__ ((__nonnull__ (1, 3)));

extern float strtof_l (const char *__restrict __nptr,
         char **__restrict __endptr, __locale_t __loc)
     throw () __attribute__ ((__nonnull__ (1, 3)));

extern long double strtold_l (const char *__restrict __nptr,
         char **__restrict __endptr,
         __locale_t __loc)
     throw () __attribute__ ((__nonnull__ (1, 3)));
# 305 "/usr/include/stdlib.h" 3 4
extern char *l64a (long int __n) throw () ;


extern long int a64l (const char *__s)
     throw () __attribute__ ((__pure__)) __attribute__ ((__nonnull__ (1))) ;




# 1 "/usr/include/sys/types.h" 1 3 4
# 27 "/usr/include/sys/types.h" 3 4
extern "C" {





typedef __u_char u_char;
typedef __u_short u_short;
typedef __u_int u_int;
typedef __u_long u_long;
typedef __quad_t quad_t;
typedef __u_quad_t u_quad_t;
typedef __fsid_t fsid_t;




typedef __loff_t loff_t;



typedef __ino_t ino_t;






typedef __ino64_t ino64_t;




typedef __dev_t dev_t;




typedef __gid_t gid_t;




typedef __mode_t mode_t;




typedef __nlink_t nlink_t;




typedef __uid_t uid_t;





typedef __off_t off_t;






typedef __off64_t off64_t;




typedef __pid_t pid_t;





typedef __id_t id_t;




typedef __ssize_t ssize_t;





typedef __daddr_t daddr_t;
typedef __caddr_t caddr_t;





typedef __key_t key_t;
# 132 "/usr/include/sys/types.h" 3 4
# 1 "/usr/include/time.h" 1 3 4
# 57 "/usr/include/time.h" 3 4


typedef __clock_t clock_t;



# 73 "/usr/include/time.h" 3 4


typedef __time_t time_t;



# 91 "/usr/include/time.h" 3 4
typedef __clockid_t clockid_t;
# 103 "/usr/include/time.h" 3 4
typedef __timer_t timer_t;
# 133 "/usr/include/sys/types.h" 2 3 4



typedef __useconds_t useconds_t;



typedef __suseconds_t suseconds_t;





# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 147 "/usr/include/sys/types.h" 2 3 4



typedef unsigned long int ulong;
typedef unsigned short int ushort;
typedef unsigned int uint;
# 194 "/usr/include/sys/types.h" 3 4
typedef int int8_t __attribute__ ((__mode__ (__QI__)));
typedef int int16_t __attribute__ ((__mode__ (__HI__)));
typedef int int32_t __attribute__ ((__mode__ (__SI__)));
typedef int int64_t __attribute__ ((__mode__ (__DI__)));


typedef unsigned int u_int8_t __attribute__ ((__mode__ (__QI__)));
typedef unsigned int u_int16_t __attribute__ ((__mode__ (__HI__)));
typedef unsigned int u_int32_t __attribute__ ((__mode__ (__SI__)));
typedef unsigned int u_int64_t __attribute__ ((__mode__ (__DI__)));

typedef int register_t __attribute__ ((__mode__ (__word__)));
# 219 "/usr/include/sys/types.h" 3 4
# 1 "/usr/include/sys/select.h" 1 3 4
# 30 "/usr/include/sys/select.h" 3 4
# 1 "/usr/include/bits/select.h" 1 3 4
# 22 "/usr/include/bits/select.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/select.h" 2 3 4
# 31 "/usr/include/sys/select.h" 2 3 4


# 1 "/usr/include/bits/sigset.h" 1 3 4
# 23 "/usr/include/bits/sigset.h" 3 4
typedef int __sig_atomic_t;




typedef struct
  {
    unsigned long int __val[(1024 / (8 * sizeof (unsigned long int)))];
  } __sigset_t;
# 34 "/usr/include/sys/select.h" 2 3 4



typedef __sigset_t sigset_t;





# 1 "/usr/include/time.h" 1 3 4
# 120 "/usr/include/time.h" 3 4
struct timespec
  {
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
  };
# 44 "/usr/include/sys/select.h" 2 3 4

# 1 "/usr/include/bits/time.h" 1 3 4
# 30 "/usr/include/bits/time.h" 3 4
struct timeval
  {
    __time_t tv_sec;
    __suseconds_t tv_usec;
  };
# 46 "/usr/include/sys/select.h" 2 3 4
# 54 "/usr/include/sys/select.h" 3 4
typedef long int __fd_mask;
# 64 "/usr/include/sys/select.h" 3 4
typedef struct
  {



    __fd_mask fds_bits[1024 / (8 * (int) sizeof (__fd_mask))];





  } fd_set;






typedef __fd_mask fd_mask;
# 96 "/usr/include/sys/select.h" 3 4
extern "C" {
# 106 "/usr/include/sys/select.h" 3 4
extern int select (int __nfds, fd_set *__restrict __readfds,
     fd_set *__restrict __writefds,
     fd_set *__restrict __exceptfds,
     struct timeval *__restrict __timeout);
# 118 "/usr/include/sys/select.h" 3 4
extern int pselect (int __nfds, fd_set *__restrict __readfds,
      fd_set *__restrict __writefds,
      fd_set *__restrict __exceptfds,
      const struct timespec *__restrict __timeout,
      const __sigset_t *__restrict __sigmask);
# 131 "/usr/include/sys/select.h" 3 4
}
# 220 "/usr/include/sys/types.h" 2 3 4


# 1 "/usr/include/sys/sysmacros.h" 1 3 4
# 29 "/usr/include/sys/sysmacros.h" 3 4
extern "C" {

__extension__
extern unsigned int gnu_dev_major (unsigned long long int __dev)
     throw () __attribute__ ((__const__));
__extension__
extern unsigned int gnu_dev_minor (unsigned long long int __dev)
     throw () __attribute__ ((__const__));
__extension__
extern unsigned long long int gnu_dev_makedev (unsigned int __major,
            unsigned int __minor)
     throw () __attribute__ ((__const__));
# 63 "/usr/include/sys/sysmacros.h" 3 4
}
# 223 "/usr/include/sys/types.h" 2 3 4





typedef __blksize_t blksize_t;






typedef __blkcnt_t blkcnt_t;



typedef __fsblkcnt_t fsblkcnt_t;



typedef __fsfilcnt_t fsfilcnt_t;
# 262 "/usr/include/sys/types.h" 3 4
typedef __blkcnt64_t blkcnt64_t;
typedef __fsblkcnt64_t fsblkcnt64_t;
typedef __fsfilcnt64_t fsfilcnt64_t;





# 1 "/usr/include/bits/pthreadtypes.h" 1 3 4
# 21 "/usr/include/bits/pthreadtypes.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 22 "/usr/include/bits/pthreadtypes.h" 2 3 4
# 60 "/usr/include/bits/pthreadtypes.h" 3 4
typedef unsigned long int pthread_t;


union pthread_attr_t
{
  char __size[56];
  long int __align;
};

typedef union pthread_attr_t pthread_attr_t;





typedef struct __pthread_internal_list
{
  struct __pthread_internal_list *__prev;
  struct __pthread_internal_list *__next;
} __pthread_list_t;
# 90 "/usr/include/bits/pthreadtypes.h" 3 4
typedef union
{
  struct __pthread_mutex_s
  {
    int __lock;
    unsigned int __count;
    int __owner;

    unsigned int __nusers;



    int __kind;

    short __spins;
    short __elision;
    __pthread_list_t __list;
# 124 "/usr/include/bits/pthreadtypes.h" 3 4
  } __data;
  char __size[40];
  long int __align;
} pthread_mutex_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_mutexattr_t;




typedef union
{
  struct
  {
    int __lock;
    unsigned int __futex;
    __extension__ unsigned long long int __total_seq;
    __extension__ unsigned long long int __wakeup_seq;
    __extension__ unsigned long long int __woken_seq;
    void *__mutex;
    unsigned int __nwaiters;
    unsigned int __broadcast_seq;
  } __data;
  char __size[48];
  __extension__ long long int __align;
} pthread_cond_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_condattr_t;



typedef unsigned int pthread_key_t;



typedef int pthread_once_t;





typedef union
{

  struct
  {
    int __lock;
    unsigned int __nr_readers;
    unsigned int __readers_wakeup;
    unsigned int __writer_wakeup;
    unsigned int __nr_readers_queued;
    unsigned int __nr_writers_queued;
    int __writer;
    int __shared;
    unsigned long int __pad1;
    unsigned long int __pad2;


    unsigned int __flags;

  } __data;
# 211 "/usr/include/bits/pthreadtypes.h" 3 4
  char __size[56];
  long int __align;
} pthread_rwlock_t;

typedef union
{
  char __size[8];
  long int __align;
} pthread_rwlockattr_t;





typedef volatile int pthread_spinlock_t;




typedef union
{
  char __size[32];
  long int __align;
} pthread_barrier_t;

typedef union
{
  char __size[4];
  int __align;
} pthread_barrierattr_t;
# 271 "/usr/include/sys/types.h" 2 3 4


}
# 315 "/usr/include/stdlib.h" 2 3 4






extern long int random (void) throw ();


extern void srandom (unsigned int __seed) throw ();





extern char *initstate (unsigned int __seed, char *__statebuf,
   size_t __statelen) throw () __attribute__ ((__nonnull__ (2)));



extern char *setstate (char *__statebuf) throw () __attribute__ ((__nonnull__ (1)));







struct random_data
  {
    int32_t *fptr;
    int32_t *rptr;
    int32_t *state;
    int rand_type;
    int rand_deg;
    int rand_sep;
    int32_t *end_ptr;
  };

extern int random_r (struct random_data *__restrict __buf,
       int32_t *__restrict __result) throw () __attribute__ ((__nonnull__ (1, 2)));

extern int srandom_r (unsigned int __seed, struct random_data *__buf)
     throw () __attribute__ ((__nonnull__ (2)));

extern int initstate_r (unsigned int __seed, char *__restrict __statebuf,
   size_t __statelen,
   struct random_data *__restrict __buf)
     throw () __attribute__ ((__nonnull__ (2, 4)));

extern int setstate_r (char *__restrict __statebuf,
         struct random_data *__restrict __buf)
     throw () __attribute__ ((__nonnull__ (1, 2)));






extern int rand (void) throw ();

extern void srand (unsigned int __seed) throw ();




extern int rand_r (unsigned int *__seed) throw ();







extern double drand48 (void) throw ();
extern double erand48 (unsigned short int __xsubi[3]) throw () __attribute__ ((__nonnull__ (1)));


extern long int lrand48 (void) throw ();
extern long int nrand48 (unsigned short int __xsubi[3])
     throw () __attribute__ ((__nonnull__ (1)));


extern long int mrand48 (void) throw ();
extern long int jrand48 (unsigned short int __xsubi[3])
     throw () __attribute__ ((__nonnull__ (1)));


extern void srand48 (long int __seedval) throw ();
extern unsigned short int *seed48 (unsigned short int __seed16v[3])
     throw () __attribute__ ((__nonnull__ (1)));
extern void lcong48 (unsigned short int __param[7]) throw () __attribute__ ((__nonnull__ (1)));





struct drand48_data
  {
    unsigned short int __x[3];
    unsigned short int __old_x[3];
    unsigned short int __c;
    unsigned short int __init;
    unsigned long long int __a;
  };


extern int drand48_r (struct drand48_data *__restrict __buffer,
        double *__restrict __result) throw () __attribute__ ((__nonnull__ (1, 2)));
extern int erand48_r (unsigned short int __xsubi[3],
        struct drand48_data *__restrict __buffer,
        double *__restrict __result) throw () __attribute__ ((__nonnull__ (1, 2)));


extern int lrand48_r (struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     throw () __attribute__ ((__nonnull__ (1, 2)));
extern int nrand48_r (unsigned short int __xsubi[3],
        struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int mrand48_r (struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     throw () __attribute__ ((__nonnull__ (1, 2)));
extern int jrand48_r (unsigned short int __xsubi[3],
        struct drand48_data *__restrict __buffer,
        long int *__restrict __result)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int srand48_r (long int __seedval, struct drand48_data *__buffer)
     throw () __attribute__ ((__nonnull__ (2)));

extern int seed48_r (unsigned short int __seed16v[3],
       struct drand48_data *__buffer) throw () __attribute__ ((__nonnull__ (1, 2)));

extern int lcong48_r (unsigned short int __param[7],
        struct drand48_data *__buffer)
     throw () __attribute__ ((__nonnull__ (1, 2)));









extern void *malloc (size_t __size) throw () __attribute__ ((__malloc__)) ;

extern void *calloc (size_t __nmemb, size_t __size)
     throw () __attribute__ ((__malloc__)) ;










extern void *realloc (void *__ptr, size_t __size)
     throw () __attribute__ ((__warn_unused_result__));

extern void free (void *__ptr) throw ();




extern void cfree (void *__ptr) throw ();



# 1 "/usr/include/alloca.h" 1 3 4
# 24 "/usr/include/alloca.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 25 "/usr/include/alloca.h" 2 3 4

extern "C" {





extern void *alloca (size_t __size) throw ();





}
# 492 "/usr/include/stdlib.h" 2 3 4





extern void *valloc (size_t __size) throw () __attribute__ ((__malloc__)) ;




extern int posix_memalign (void **__memptr, size_t __alignment, size_t __size)
     throw () __attribute__ ((__nonnull__ (1))) ;




extern void *aligned_alloc (size_t __alignment, size_t __size)
     throw () __attribute__ ((__malloc__, __alloc_size__ (2)));




extern void abort (void) throw () __attribute__ ((__noreturn__));



extern int atexit (void (*__func) (void)) throw () __attribute__ ((__nonnull__ (1)));




extern "C++" int at_quick_exit (void (*__func) (void))
     throw () __asm ("at_quick_exit") __attribute__ ((__nonnull__ (1)));









extern int on_exit (void (*__func) (int __status, void *__arg), void *__arg)
     throw () __attribute__ ((__nonnull__ (1)));






extern void exit (int __status) throw () __attribute__ ((__noreturn__));





extern void quick_exit (int __status) throw () __attribute__ ((__noreturn__));







extern void _Exit (int __status) throw () __attribute__ ((__noreturn__));






extern char *getenv (const char *__name) throw () __attribute__ ((__nonnull__ (1))) ;





extern char *secure_getenv (const char *__name)
     throw () __attribute__ ((__nonnull__ (1))) ;






extern int putenv (char *__string) throw () __attribute__ ((__nonnull__ (1)));





extern int setenv (const char *__name, const char *__value, int __replace)
     throw () __attribute__ ((__nonnull__ (2)));


extern int unsetenv (const char *__name) throw () __attribute__ ((__nonnull__ (1)));






extern int clearenv (void) throw ();
# 605 "/usr/include/stdlib.h" 3 4
extern char *mktemp (char *__template) throw () __attribute__ ((__nonnull__ (1)));
# 619 "/usr/include/stdlib.h" 3 4
extern int mkstemp (char *__template) __attribute__ ((__nonnull__ (1))) ;
# 629 "/usr/include/stdlib.h" 3 4
extern int mkstemp64 (char *__template) __attribute__ ((__nonnull__ (1))) ;
# 641 "/usr/include/stdlib.h" 3 4
extern int mkstemps (char *__template, int __suffixlen) __attribute__ ((__nonnull__ (1))) ;
# 651 "/usr/include/stdlib.h" 3 4
extern int mkstemps64 (char *__template, int __suffixlen)
     __attribute__ ((__nonnull__ (1))) ;
# 662 "/usr/include/stdlib.h" 3 4
extern char *mkdtemp (char *__template) throw () __attribute__ ((__nonnull__ (1))) ;
# 673 "/usr/include/stdlib.h" 3 4
extern int mkostemp (char *__template, int __flags) __attribute__ ((__nonnull__ (1))) ;
# 683 "/usr/include/stdlib.h" 3 4
extern int mkostemp64 (char *__template, int __flags) __attribute__ ((__nonnull__ (1))) ;
# 693 "/usr/include/stdlib.h" 3 4
extern int mkostemps (char *__template, int __suffixlen, int __flags)
     __attribute__ ((__nonnull__ (1))) ;
# 705 "/usr/include/stdlib.h" 3 4
extern int mkostemps64 (char *__template, int __suffixlen, int __flags)
     __attribute__ ((__nonnull__ (1))) ;









extern int system (const char *__command) ;






extern char *canonicalize_file_name (const char *__name)
     throw () __attribute__ ((__nonnull__ (1))) ;
# 733 "/usr/include/stdlib.h" 3 4
extern char *realpath (const char *__restrict __name,
         char *__restrict __resolved) throw () ;






typedef int (*__compar_fn_t) (const void *, const void *);


typedef __compar_fn_t comparison_fn_t;



typedef int (*__compar_d_fn_t) (const void *, const void *, void *);





extern void *bsearch (const void *__key, const void *__base,
        size_t __nmemb, size_t __size, __compar_fn_t __compar)
     __attribute__ ((__nonnull__ (1, 2, 5))) ;



extern void qsort (void *__base, size_t __nmemb, size_t __size,
     __compar_fn_t __compar) __attribute__ ((__nonnull__ (1, 4)));

extern void qsort_r (void *__base, size_t __nmemb, size_t __size,
       __compar_d_fn_t __compar, void *__arg)
  __attribute__ ((__nonnull__ (1, 4)));




extern int abs (int __x) throw () __attribute__ ((__const__)) ;
extern long int labs (long int __x) throw () __attribute__ ((__const__)) ;



__extension__ extern long long int llabs (long long int __x)
     throw () __attribute__ ((__const__)) ;







extern div_t div (int __numer, int __denom)
     throw () __attribute__ ((__const__)) ;
extern ldiv_t ldiv (long int __numer, long int __denom)
     throw () __attribute__ ((__const__)) ;




__extension__ extern lldiv_t lldiv (long long int __numer,
        long long int __denom)
     throw () __attribute__ ((__const__)) ;

# 807 "/usr/include/stdlib.h" 3 4
extern char *ecvt (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign) throw () __attribute__ ((__nonnull__ (3, 4))) ;




extern char *fcvt (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign) throw () __attribute__ ((__nonnull__ (3, 4))) ;




extern char *gcvt (double __value, int __ndigit, char *__buf)
     throw () __attribute__ ((__nonnull__ (3))) ;




extern char *qecvt (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign)
     throw () __attribute__ ((__nonnull__ (3, 4))) ;
extern char *qfcvt (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign)
     throw () __attribute__ ((__nonnull__ (3, 4))) ;
extern char *qgcvt (long double __value, int __ndigit, char *__buf)
     throw () __attribute__ ((__nonnull__ (3))) ;




extern int ecvt_r (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign, char *__restrict __buf,
     size_t __len) throw () __attribute__ ((__nonnull__ (3, 4, 5)));
extern int fcvt_r (double __value, int __ndigit, int *__restrict __decpt,
     int *__restrict __sign, char *__restrict __buf,
     size_t __len) throw () __attribute__ ((__nonnull__ (3, 4, 5)));

extern int qecvt_r (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign,
      char *__restrict __buf, size_t __len)
     throw () __attribute__ ((__nonnull__ (3, 4, 5)));
extern int qfcvt_r (long double __value, int __ndigit,
      int *__restrict __decpt, int *__restrict __sign,
      char *__restrict __buf, size_t __len)
     throw () __attribute__ ((__nonnull__ (3, 4, 5)));







extern int mblen (const char *__s, size_t __n) throw () ;


extern int mbtowc (wchar_t *__restrict __pwc,
     const char *__restrict __s, size_t __n) throw () ;


extern int wctomb (char *__s, wchar_t __wchar) throw () ;



extern size_t mbstowcs (wchar_t *__restrict __pwcs,
   const char *__restrict __s, size_t __n) throw ();

extern size_t wcstombs (char *__restrict __s,
   const wchar_t *__restrict __pwcs, size_t __n)
     throw ();








extern int rpmatch (const char *__response) throw () __attribute__ ((__nonnull__ (1))) ;
# 895 "/usr/include/stdlib.h" 3 4
extern int getsubopt (char **__restrict __optionp,
        char *const *__restrict __tokens,
        char **__restrict __valuep)
     throw () __attribute__ ((__nonnull__ (1, 2, 3))) ;





extern void setkey (const char *__key) throw () __attribute__ ((__nonnull__ (1)));







extern int posix_openpt (int __oflag) ;







extern int grantpt (int __fd) throw ();



extern int unlockpt (int __fd) throw ();




extern char *ptsname (int __fd) throw () ;






extern int ptsname_r (int __fd, char *__buf, size_t __buflen)
     throw () __attribute__ ((__nonnull__ (2)));


extern int getpt (void);






extern int getloadavg (double __loadavg[], int __nelem)
     throw () __attribute__ ((__nonnull__ (1)));


# 1 "/usr/include/bits/stdlib-float.h" 1 3 4
# 952 "/usr/include/stdlib.h" 2 3 4
# 964 "/usr/include/stdlib.h" 3 4
}
# 84 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 2 3

 
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
#pragma GCC diagnostic pop

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/std_abs.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/std_abs.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
#pragma GCC diagnostic ignored "-Wlong-long"
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/std_abs.h" 3
extern "C++"
{
namespace std __attribute__ ((__visibility__ ("default")))
{


  using ::abs;


  inline long
  abs(long __i) { return __builtin_labs(__i); }



  inline long long
  abs(long long __x) { return __builtin_llabs (__x); }
# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/std_abs.h" 3
  inline constexpr double
  abs(double __x)
  { return __builtin_fabs(__x); }

  inline constexpr float
  abs(float __x)
  { return __builtin_fabsf(__x); }

  inline constexpr long double
  abs(long double __x)
  { return __builtin_fabsl(__x); }
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/std_abs.h" 3

}
}

#pragma GCC diagnostic pop
# 88 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 2 3
# 131 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
extern "C++"
{
namespace std __attribute__ ((__visibility__ ("default")))
{


  using ::div_t;
  using ::ldiv_t;

  using ::abort;



  using ::atexit;


  using ::at_quick_exit;


  using ::atof;
  using ::atoi;
  using ::atol;
  using ::bsearch;
  using ::calloc;
  using ::div;
  using ::exit;
  using ::free;
  using ::getenv;
  using ::labs;
  using ::ldiv;
  using ::malloc;

  using ::mblen;
  using ::mbstowcs;
  using ::mbtowc;

  using ::qsort;


  using ::quick_exit;


  using ::rand;
  using ::realloc;
  using ::srand;
  using ::strtod;
  using ::strtol;
  using ::strtoul;
  using ::system;

  using ::wcstombs;
  using ::wctomb;



  inline ldiv_t
  div(long __i, long __j) noexcept { return ldiv(__i, __j); }




}
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{



  using ::lldiv_t;





  using ::_Exit;



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
  using ::llabs;

  inline lldiv_t
  div(long long __n, long long __d)
  { lldiv_t __q; __q.quot = __n / __d; __q.rem = __n % __d; return __q; }

  using ::lldiv;
#pragma GCC diagnostic pop
# 240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
  using ::atoll;
  using ::strtoll;
  using ::strtoull;

  using ::strtof;
  using ::strtold;


}

namespace std
{

  using ::__gnu_cxx::lldiv_t;

  using ::__gnu_cxx::_Exit;

  using ::__gnu_cxx::llabs;
  using ::__gnu_cxx::div;
  using ::__gnu_cxx::lldiv;

  using ::__gnu_cxx::atoll;
  using ::__gnu_cxx::strtof;
  using ::__gnu_cxx::strtoll;
  using ::__gnu_cxx::strtoull;
  using ::__gnu_cxx::strtold;
}
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
}
# 72 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 2 3



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"



namespace std __attribute__ ((__visibility__ ("default")))
{



  template<typename _Iterator, typename _Compare>
   
    void
    __move_median_to_first(_Iterator __result,_Iterator __a, _Iterator __b,
      _Iterator __c, _Compare __comp)
    {
      if (__comp(__a, __b))
 {
   if (__comp(__b, __c))
     std::iter_swap(__result, __b);
   else if (__comp(__a, __c))
     std::iter_swap(__result, __c);
   else
     std::iter_swap(__result, __a);
 }
      else if (__comp(__a, __c))
 std::iter_swap(__result, __a);
      else if (__comp(__b, __c))
 std::iter_swap(__result, __c);
      else
 std::iter_swap(__result, __b);
    }


  template<typename _InputIterator, typename _Predicate>
   
    inline _InputIterator
    __find_if_not(_InputIterator __first, _InputIterator __last,
    _Predicate __pred)
    {
      return std::__find_if(__first, __last,
       __gnu_cxx::__ops::__negate(__pred));
    }




  template<typename _InputIterator, typename _Predicate, typename _Distance>
   
    _InputIterator
    __find_if_not_n(_InputIterator __first, _Distance& __len, _Predicate __pred)
    {
      for (; __len; --__len, (void) ++__first)
 if (!__pred(__first))
   break;
      return __first;
    }
# 150 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Integer,
    typename _UnaryPredicate>
   
    _ForwardIterator
    __search_n_aux(_ForwardIterator __first, _ForwardIterator __last,
     _Integer __count, _UnaryPredicate __unary_pred,
     std::forward_iterator_tag)
    {
      __first = std::__find_if(__first, __last, __unary_pred);
      while (__first != __last)
 {
   typename iterator_traits<_ForwardIterator>::difference_type
     __n = __count;
   _ForwardIterator __i = __first;
   ++__i;
   while (__i != __last && __n != 1 && __unary_pred(__i))
     {
       ++__i;
       --__n;
     }
   if (__n == 1)
     return __first;
   if (__i == __last)
     return __last;
   __first = std::__find_if(++__i, __last, __unary_pred);
 }
      return __last;
    }





  template<typename _RandomAccessIter, typename _Integer,
    typename _UnaryPredicate>
   
    _RandomAccessIter
    __search_n_aux(_RandomAccessIter __first, _RandomAccessIter __last,
     _Integer __count, _UnaryPredicate __unary_pred,
     std::random_access_iterator_tag)
    {
      typedef typename std::iterator_traits<_RandomAccessIter>::difference_type
 _DistanceType;

      _DistanceType __tailSize = __last - __first;
      _DistanceType __remainder = __count;

      while (__remainder <= __tailSize)
 {
   __first += __remainder;
   __tailSize -= __remainder;


   _RandomAccessIter __backTrack = __first;
   while (__unary_pred(--__backTrack))
     {
       if (--__remainder == 0)
  return (__first - __count);
     }
   __remainder = __count + 1 - (__first - __backTrack);
 }
      return __last;
    }

  template<typename _ForwardIterator, typename _Integer,
    typename _UnaryPredicate>
   
    _ForwardIterator
    __search_n(_ForwardIterator __first, _ForwardIterator __last,
        _Integer __count,
        _UnaryPredicate __unary_pred)
    {
      if (__count <= 0)
 return __first;

      if (__count == 1)
 return std::__find_if(__first, __last, __unary_pred);

      return std::__search_n_aux(__first, __last, __count, __unary_pred,
     std::__iterator_category(__first));
    }


  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    _ForwardIterator1
    __find_end(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
        _ForwardIterator2 __first2, _ForwardIterator2 __last2,
        forward_iterator_tag, forward_iterator_tag,
        _BinaryPredicate __comp)
    {
      if (__first2 == __last2)
 return __last1;

      _ForwardIterator1 __result = __last1;
      while (1)
 {
   _ForwardIterator1 __new_result
     = std::__search(__first1, __last1, __first2, __last2, __comp);
   if (__new_result == __last1)
     return __result;
   else
     {
       __result = __new_result;
       __first1 = __new_result;
       ++__first1;
     }
 }
    }


  template<typename _BidirectionalIterator1, typename _BidirectionalIterator2,
    typename _BinaryPredicate>
   
    _BidirectionalIterator1
    __find_end(_BidirectionalIterator1 __first1,
        _BidirectionalIterator1 __last1,
        _BidirectionalIterator2 __first2,
        _BidirectionalIterator2 __last2,
        bidirectional_iterator_tag, bidirectional_iterator_tag,
        _BinaryPredicate __comp)
    {

     

     


      typedef reverse_iterator<_BidirectionalIterator1> _RevIterator1;
      typedef reverse_iterator<_BidirectionalIterator2> _RevIterator2;

      _RevIterator1 __rlast1(__first1);
      _RevIterator2 __rlast2(__first2);
      _RevIterator1 __rresult = std::__search(_RevIterator1(__last1), __rlast1,
           _RevIterator2(__last2), __rlast2,
           __comp);

      if (__rresult == __rlast1)
 return __last1;
      else
 {
   _BidirectionalIterator1 __result = __rresult.base();
   std::advance(__result, -std::distance(__first2, __last2));
   return __result;
 }
    }
# 324 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
    [[__nodiscard__]]
    inline _ForwardIterator1
    find_end(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
      _ForwardIterator2 __first2, _ForwardIterator2 __last2)
    {

     
     
     


      ;
      ;

      return std::__find_end(__first1, __last1, __first2, __last2,
        std::__iterator_category(__first1),
        std::__iterator_category(__first2),
        __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 373 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline _ForwardIterator1
    find_end(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
      _ForwardIterator2 __first2, _ForwardIterator2 __last2,
      _BinaryPredicate __comp)
    {

     
     
     


      ;
      ;

      return std::__find_end(__first1, __last1, __first2, __last2,
        std::__iterator_category(__first1),
        std::__iterator_category(__first2),
        __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 409 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline bool
    all_of(_InputIterator __first, _InputIterator __last, _Predicate __pred)
    { return __last == std::find_if_not(__first, __last, __pred); }
# 427 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline bool
    none_of(_InputIterator __first, _InputIterator __last, _Predicate __pred)
    { return __last == std::find_if(__first, __last, __pred); }
# 446 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline bool
    any_of(_InputIterator __first, _InputIterator __last, _Predicate __pred)
    { return !std::none_of(__first, __last, __pred); }
# 462 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline _InputIterator
    find_if_not(_InputIterator __first, _InputIterator __last,
  _Predicate __pred)
    {

     
     

      ;
      return std::__find_if_not(__first, __last,
    __gnu_cxx::__ops::__pred_iter(__pred));
    }
# 487 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline bool
    is_partitioned(_InputIterator __first, _InputIterator __last,
     _Predicate __pred)
    {
      __first = std::find_if_not(__first, __last, __pred);
      if (__first == __last)
 return true;
      ++__first;
      return std::none_of(__first, __last, __pred);
    }
# 509 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Predicate>
    [[__nodiscard__]]
    _ForwardIterator
    partition_point(_ForwardIterator __first, _ForwardIterator __last,
      _Predicate __pred)
    {

     
     



      ;

      typedef typename iterator_traits<_ForwardIterator>::difference_type
 _DistanceType;

      _DistanceType __len = std::distance(__first, __last);

      while (__len > 0)
 {
   _DistanceType __half = __len >> 1;
   _ForwardIterator __middle = __first;
   std::advance(__middle, __half);
   if (__pred(*__middle))
     {
       __first = __middle;
       ++__first;
       __len = __len - __half - 1;
     }
   else
     __len = __half;
 }
      return __first;
    }


  template<typename _InputIterator, typename _OutputIterator,
    typename _Predicate>
   
    _OutputIterator
    __remove_copy_if(_InputIterator __first, _InputIterator __last,
       _OutputIterator __result, _Predicate __pred)
    {
      for (; __first != __last; ++__first)
 if (!__pred(__first))
   {
     *__result = *__first;
     ++__result;
   }
      return __result;
    }
# 576 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator, typename _Tp>
   
    inline _OutputIterator
    remove_copy(_InputIterator __first, _InputIterator __last,
  _OutputIterator __result, const _Tp& __value)
    {

     
     

     

      ;

      return std::__remove_copy_if(__first, __last, __result,
 __gnu_cxx::__ops::__iter_equals_val(__value));
    }
# 609 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator,
    typename _Predicate>
   
    inline _OutputIterator
    remove_copy_if(_InputIterator __first, _InputIterator __last,
     _OutputIterator __result, _Predicate __pred)
    {

     
     

     

      ;

      return std::__remove_copy_if(__first, __last, __result,
       __gnu_cxx::__ops::__pred_iter(__pred));
    }
# 644 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator,
    typename _Predicate>
   
    _OutputIterator
    copy_if(_InputIterator __first, _InputIterator __last,
     _OutputIterator __result, _Predicate __pred)
    {

     
     

     

      ;

      for (; __first != __last; ++__first)
 if (__pred(*__first))
   {
     *__result = *__first;
     ++__result;
   }
      return __result;
    }
# 681 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Size, typename _OutputIterator>
   
    inline _OutputIterator
    copy_n(_InputIterator __first, _Size __n, _OutputIterator __result)
    {

     
     


      const auto __n2 = std::__size_to_integer(__n);
      if (__n2 <= 0)
 return __result;

      ;
      ;

      auto __res = std::__copy_n_a(std::__niter_base(__first), __n2,
       std::__niter_base(__result), true);
      return std::__niter_wrap(__result, std::move(__res));
    }
# 718 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator1,
    typename _OutputIterator2, typename _Predicate>
   
    pair<_OutputIterator1, _OutputIterator2>
    partition_copy(_InputIterator __first, _InputIterator __last,
     _OutputIterator1 __out_true, _OutputIterator2 __out_false,
     _Predicate __pred)
    {

     
     

     

     

      ;

      for (; __first != __last; ++__first)
 if (__pred(*__first))
   {
     *__out_true = *__first;
     ++__out_true;
   }
 else
   {
     *__out_false = *__first;
     ++__out_false;
   }

      return pair<_OutputIterator1, _OutputIterator2>(__out_true, __out_false);
    }
# 769 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp>
    [[__nodiscard__]]
    inline _ForwardIterator
    remove(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __value)
    {

     

     

      ;

      return std::__remove_if(__first, __last,
  __gnu_cxx::__ops::__iter_equals_val(__value));
    }
# 803 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Predicate>
    [[__nodiscard__]]
    inline _ForwardIterator
    remove_if(_ForwardIterator __first, _ForwardIterator __last,
       _Predicate __pred)
    {

     

     

      ;

      return std::__remove_if(__first, __last,
         __gnu_cxx::__ops::__pred_iter(__pred));
    }

  template<typename _ForwardIterator, typename _BinaryPredicate>
   
    _ForwardIterator
    __adjacent_find(_ForwardIterator __first, _ForwardIterator __last,
      _BinaryPredicate __binary_pred)
    {
      if (__first == __last)
 return __last;
      _ForwardIterator __next = __first;
      while (++__next != __last)
 {
   if (__binary_pred(__first, __next))
     return __first;
   __first = __next;
 }
      return __last;
    }

  template<typename _ForwardIterator, typename _BinaryPredicate>
   
    _ForwardIterator
    __unique(_ForwardIterator __first, _ForwardIterator __last,
      _BinaryPredicate __binary_pred)
    {

      __first = std::__adjacent_find(__first, __last, __binary_pred);
      if (__first == __last)
 return __last;


      _ForwardIterator __dest = __first;
      ++__first;
      while (++__first != __last)
 if (!__binary_pred(__dest, __first))
   *++__dest = std::move(*__first);
      return ++__dest;
    }
# 872 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]]
    inline _ForwardIterator
    unique(_ForwardIterator __first, _ForwardIterator __last)
    {

     

     

      ;

      return std::__unique(__first, __last,
      __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 903 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _BinaryPredicate>
    [[__nodiscard__]]
    inline _ForwardIterator
    unique(_ForwardIterator __first, _ForwardIterator __last,
    _BinaryPredicate __binary_pred)
    {

     

     


      ;

      return std::__unique(__first, __last,
      __gnu_cxx::__ops::__iter_comp_iter(__binary_pred));
    }







  template<typename _ForwardIterator, typename _OutputIterator,
    typename _BinaryPredicate>
   
    _OutputIterator
    __unique_copy(_ForwardIterator __first, _ForwardIterator __last,
    _OutputIterator __result, _BinaryPredicate __binary_pred,
    forward_iterator_tag, output_iterator_tag)
    {

     



      _ForwardIterator __next = __first;
      *__result = *__first;
      while (++__next != __last)
 if (!__binary_pred(__first, __next))
   {
     __first = __next;
     *++__result = *__first;
   }
      return ++__result;
    }







  template<typename _InputIterator, typename _OutputIterator,
    typename _BinaryPredicate>
   
    _OutputIterator
    __unique_copy(_InputIterator __first, _InputIterator __last,
    _OutputIterator __result, _BinaryPredicate __binary_pred,
    input_iterator_tag, output_iterator_tag)
    {

     



      typename iterator_traits<_InputIterator>::value_type __value = *__first;
      __decltype(__gnu_cxx::__ops::__iter_comp_val(__binary_pred))
 __rebound_pred
 = __gnu_cxx::__ops::__iter_comp_val(__binary_pred);
      *__result = __value;
      while (++__first != __last)
 if (!__rebound_pred(__first, __value))
   {
     __value = *__first;
     *++__result = __value;
   }
      return ++__result;
    }







  template<typename _InputIterator, typename _ForwardIterator,
    typename _BinaryPredicate>
   
    _ForwardIterator
    __unique_copy(_InputIterator __first, _InputIterator __last,
    _ForwardIterator __result, _BinaryPredicate __binary_pred,
    input_iterator_tag, forward_iterator_tag)
    {

     


      *__result = *__first;
      while (++__first != __last)
 if (!__binary_pred(__result, __first))
   *++__result = *__first;
      return ++__result;
    }






  template<typename _BidirectionalIterator>
   
    void
    __reverse(_BidirectionalIterator __first, _BidirectionalIterator __last,
       bidirectional_iterator_tag)
    {
      while (true)
 if (__first == __last || __first == --__last)
   return;
 else
   {
     std::iter_swap(__first, __last);
     ++__first;
   }
    }






  template<typename _RandomAccessIterator>
   
    void
    __reverse(_RandomAccessIterator __first, _RandomAccessIterator __last,
       random_access_iterator_tag)
    {
      if (__first == __last)
 return;
      --__last;
      while (__first < __last)
 {
   std::iter_swap(__first, __last);
   ++__first;
   --__last;
 }
    }
# 1064 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator>
   
    inline void
    reverse(_BidirectionalIterator __first, _BidirectionalIterator __last)
    {

     

      ;
      std::__reverse(__first, __last, std::__iterator_category(__first));
    }
# 1092 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator, typename _OutputIterator>
   
    _OutputIterator
    reverse_copy(_BidirectionalIterator __first, _BidirectionalIterator __last,
   _OutputIterator __result)
    {

     

     

      ;

      while (__first != __last)
 {
   --__last;
   *__result = *__last;
   ++__result;
 }
      return __result;
    }





  template<typename _EuclideanRingElement>
   
    _EuclideanRingElement
    __gcd(_EuclideanRingElement __m, _EuclideanRingElement __n)
    {
      while (__n != 0)
 {
   _EuclideanRingElement __t = __m % __n;
   __m = __n;
   __n = __t;
 }
      return __m;
    }

inline namespace _V2 {


  template<typename _ForwardIterator>
   
    _ForwardIterator
    __rotate(_ForwardIterator __first,
      _ForwardIterator __middle,
      _ForwardIterator __last,
      forward_iterator_tag)
    {
      if (__first == __middle)
 return __last;
      else if (__last == __middle)
 return __first;

      _ForwardIterator __first2 = __middle;
      do
 {
   std::iter_swap(__first, __first2);
   ++__first;
   ++__first2;
   if (__first == __middle)
     __middle = __first2;
 }
      while (__first2 != __last);

      _ForwardIterator __ret = __first;

      __first2 = __middle;

      while (__first2 != __last)
 {
   std::iter_swap(__first, __first2);
   ++__first;
   ++__first2;
   if (__first == __middle)
     __middle = __first2;
   else if (__first2 == __last)
     __first2 = __middle;
 }
      return __ret;
    }


  template<typename _BidirectionalIterator>
   
    _BidirectionalIterator
    __rotate(_BidirectionalIterator __first,
      _BidirectionalIterator __middle,
      _BidirectionalIterator __last,
       bidirectional_iterator_tag)
    {

     


      if (__first == __middle)
 return __last;
      else if (__last == __middle)
 return __first;

      std::__reverse(__first, __middle, bidirectional_iterator_tag());
      std::__reverse(__middle, __last, bidirectional_iterator_tag());

      while (__first != __middle && __middle != __last)
 {
   std::iter_swap(__first, --__last);
   ++__first;
 }

      if (__first == __middle)
 {
   std::__reverse(__middle, __last, bidirectional_iterator_tag());
   return __last;
 }
      else
 {
   std::__reverse(__first, __middle, bidirectional_iterator_tag());
   return __first;
 }
    }


  template<typename _RandomAccessIterator>
   
    _RandomAccessIterator
    __rotate(_RandomAccessIterator __first,
      _RandomAccessIterator __middle,
      _RandomAccessIterator __last,
      random_access_iterator_tag)
    {

     


      if (__first == __middle)
 return __last;
      else if (__last == __middle)
 return __first;

      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
 _Distance;
      typedef typename iterator_traits<_RandomAccessIterator>::value_type
 _ValueType;


      typedef typename make_unsigned<_Distance>::type _UDistance;




      _Distance __n = __last - __first;
      _Distance __k = __middle - __first;

      if (__k == __n - __k)
 {
   std::swap_ranges(__first, __middle, __middle);
   return __middle;
 }

      _RandomAccessIterator __p = __first;
      _RandomAccessIterator __ret = __first + (__last - __middle);

      for (;;)
 {
   if (__k < __n - __k)
     {
       if (__is_pod(_ValueType) && __k == 1)
  {
    _ValueType __t = std::move(*__p);
    std::move(__p + 1, __p + __n, __p);
    *(__p + __n - 1) = std::move(__t);
    return __ret;
  }
       _RandomAccessIterator __q = __p + __k;
       for (_Distance __i = 0; __i < __n - __k; ++ __i)
  {
    std::iter_swap(__p, __q);
    ++__p;
    ++__q;
  }
       __n = static_cast<_UDistance>(__n) % static_cast<_UDistance>(__k);
       if (__n == 0)
  return __ret;
       std::swap(__n, __k);
       __k = __n - __k;
     }
   else
     {
       __k = __n - __k;
       if (__is_pod(_ValueType) && __k == 1)
  {
    _ValueType __t = std::move(*(__p + __n - 1));
    std::move_backward(__p, __p + __n - 1, __p + __n);
    *__p = std::move(__t);
    return __ret;
  }
       _RandomAccessIterator __q = __p + __n;
       __p = __q - __k;
       for (_Distance __i = 0; __i < __n - __k; ++ __i)
  {
    --__p;
    --__q;
    std::iter_swap(__p, __q);
  }
       __n = static_cast<_UDistance>(__n) % static_cast<_UDistance>(__k);
       if (__n == 0)
  return __ret;
       std::swap(__n, __k);
     }
 }
    }
# 1329 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
   
    inline _ForwardIterator
    rotate(_ForwardIterator __first, _ForwardIterator __middle,
    _ForwardIterator __last)
    {

     

      ;
      ;

      return std::__rotate(__first, __middle, __last,
      std::__iterator_category(__first));
    }

}
# 1367 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _OutputIterator>
   
    inline _OutputIterator
    rotate_copy(_ForwardIterator __first, _ForwardIterator __middle,
  _ForwardIterator __last, _OutputIterator __result)
    {

     
     

      ;
      ;

      return std::copy(__first, __middle,
         std::copy(__middle, __last, __result));
    }


  template<typename _ForwardIterator, typename _Predicate>
   
    _ForwardIterator
    __partition(_ForwardIterator __first, _ForwardIterator __last,
  _Predicate __pred, forward_iterator_tag)
    {
      if (__first == __last)
 return __first;

      while (__pred(*__first))
 if (++__first == __last)
   return __first;

      _ForwardIterator __next = __first;

      while (++__next != __last)
 if (__pred(*__next))
   {
     std::iter_swap(__first, __next);
     ++__first;
   }

      return __first;
    }


  template<typename _BidirectionalIterator, typename _Predicate>
   
    _BidirectionalIterator
    __partition(_BidirectionalIterator __first, _BidirectionalIterator __last,
  _Predicate __pred, bidirectional_iterator_tag)
    {
      while (true)
 {
   while (true)
     if (__first == __last)
       return __first;
     else if (__pred(*__first))
       ++__first;
     else
       break;
   --__last;
   while (true)
     if (__first == __last)
       return __first;
     else if (!bool(__pred(*__last)))
       --__last;
     else
       break;
   std::iter_swap(__first, __last);
   ++__first;
 }
    }
# 1448 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Pointer, typename _Predicate,
    typename _Distance>
    _ForwardIterator
    __stable_partition_adaptive(_ForwardIterator __first,
    _ForwardIterator __last,
    _Predicate __pred, _Distance __len,
    _Pointer __buffer,
    _Distance __buffer_size)
    {
      if (__len == 1)
 return __first;

      if (__len <= __buffer_size)
 {
   _ForwardIterator __result1 = __first;
   _Pointer __result2 = __buffer;




   *__result2 = std::move(*__first);
   ++__result2;
   ++__first;
   for (; __first != __last; ++__first)
     if (__pred(__first))
       {
  *__result1 = std::move(*__first);
  ++__result1;
       }
     else
       {
  *__result2 = std::move(*__first);
  ++__result2;
       }

   std::move(__buffer, __result2, __result1);
   return __result1;
 }

      _ForwardIterator __middle = __first;
      std::advance(__middle, __len / 2);
      _ForwardIterator __left_split =
 std::__stable_partition_adaptive(__first, __middle, __pred,
      __len / 2, __buffer,
      __buffer_size);



      _Distance __right_len = __len - __len / 2;
      _ForwardIterator __right_split =
 std::__find_if_not_n(__middle, __right_len, __pred);

      if (__right_len)
 __right_split =
   std::__stable_partition_adaptive(__right_split, __last, __pred,
        __right_len,
        __buffer, __buffer_size);

      return std::rotate(__left_split, __middle, __right_split);
    }

  template<typename _ForwardIterator, typename _Predicate>
    _ForwardIterator
    __stable_partition(_ForwardIterator __first, _ForwardIterator __last,
         _Predicate __pred)
    {
      __first = std::__find_if_not(__first, __last, __pred);

      if (__first == __last)
 return __first;

      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;
      typedef typename iterator_traits<_ForwardIterator>::difference_type
 _DistanceType;

      _Temporary_buffer<_ForwardIterator, _ValueType>
 __buf(__first, std::distance(__first, __last));
      return
 std::__stable_partition_adaptive(__first, __last, __pred,
      _DistanceType(__buf.requested_size()),
      __buf.begin(),
      _DistanceType(__buf.size()));
    }
# 1550 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Predicate>
    inline _ForwardIterator
    stable_partition(_ForwardIterator __first, _ForwardIterator __last,
       _Predicate __pred)
    {

     

     

      ;

      return std::__stable_partition(__first, __last,
         __gnu_cxx::__ops::__pred_iter(__pred));
    }





  template<typename _RandomAccessIterator, typename _Compare>
   
    void
    __heap_select(_RandomAccessIterator __first,
    _RandomAccessIterator __middle,
    _RandomAccessIterator __last, _Compare __comp)
    {
      std::__make_heap(__first, __middle, __comp);
      for (_RandomAccessIterator __i = __middle; __i < __last; ++__i)
 if (__comp(__i, __first))
   std::__pop_heap(__first, __middle, __i, __comp);
    }



  template<typename _InputIterator, typename _RandomAccessIterator,
    typename _Compare>
   
    _RandomAccessIterator
    __partial_sort_copy(_InputIterator __first, _InputIterator __last,
   _RandomAccessIterator __result_first,
   _RandomAccessIterator __result_last,
   _Compare __comp)
    {
      typedef typename iterator_traits<_InputIterator>::value_type
 _InputValueType;
      typedef iterator_traits<_RandomAccessIterator> _RItTraits;
      typedef typename _RItTraits::difference_type _DistanceType;

      if (__result_first == __result_last)
 return __result_last;
      _RandomAccessIterator __result_real_last = __result_first;
      while (__first != __last && __result_real_last != __result_last)
 {
   *__result_real_last = *__first;
   ++__result_real_last;
   ++__first;
 }

      std::__make_heap(__result_first, __result_real_last, __comp);
      while (__first != __last)
 {
   if (__comp(__first, __result_first))
     std::__adjust_heap(__result_first, _DistanceType(0),
          _DistanceType(__result_real_last
          - __result_first),
          _InputValueType(*__first), __comp);
   ++__first;
 }
      std::__sort_heap(__result_first, __result_real_last, __comp);
      return __result_real_last;
    }
# 1643 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _RandomAccessIterator>
   
    inline _RandomAccessIterator
    partial_sort_copy(_InputIterator __first, _InputIterator __last,
        _RandomAccessIterator __result_first,
        _RandomAccessIterator __result_last)
    {
# 1658 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
     
     

     

     
      ;
      ;
      ;

      return std::__partial_sort_copy(__first, __last,
          __result_first, __result_last,
          __gnu_cxx::__ops::__iter_less_iter());
    }
# 1693 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _RandomAccessIterator,
    typename _Compare>
   
    inline _RandomAccessIterator
    partial_sort_copy(_InputIterator __first, _InputIterator __last,
        _RandomAccessIterator __result_first,
        _RandomAccessIterator __result_last,
        _Compare __comp)
    {
# 1710 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
     
     

     

     

     

      ;
      ;
      ;

      return std::__partial_sort_copy(__first, __last,
          __result_first, __result_last,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }




  template<typename _RandomAccessIterator, typename _Compare>
   
    void
    __unguarded_linear_insert(_RandomAccessIterator __last,
         _Compare __comp)
    {
      typename iterator_traits<_RandomAccessIterator>::value_type
 __val = std::move(*__last);
      _RandomAccessIterator __next = __last;
      --__next;
      while (__comp(__val, __next))
 {
   *__last = std::move(*__next);
   __last = __next;
   --__next;
 }
      *__last = std::move(__val);
    }


  template<typename _RandomAccessIterator, typename _Compare>
   
    void
    __insertion_sort(_RandomAccessIterator __first,
       _RandomAccessIterator __last, _Compare __comp)
    {
      if (__first == __last) return;

      for (_RandomAccessIterator __i = __first + 1; __i != __last; ++__i)
 {
   if (__comp(__i, __first))
     {
       typename iterator_traits<_RandomAccessIterator>::value_type
  __val = std::move(*__i);
       std::move_backward(__first, __i, __i + 1);
       *__first = std::move(__val);
     }
   else
     std::__unguarded_linear_insert(__i,
    __gnu_cxx::__ops::__val_comp_iter(__comp));
 }
    }


  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    __unguarded_insertion_sort(_RandomAccessIterator __first,
          _RandomAccessIterator __last, _Compare __comp)
    {
      for (_RandomAccessIterator __i = __first; __i != __last; ++__i)
 std::__unguarded_linear_insert(__i,
    __gnu_cxx::__ops::__val_comp_iter(__comp));
    }





  enum { _S_threshold = 16 };


  template<typename _RandomAccessIterator, typename _Compare>
   
    void
    __final_insertion_sort(_RandomAccessIterator __first,
      _RandomAccessIterator __last, _Compare __comp)
    {
      if (__last - __first > int(_S_threshold))
 {
   std::__insertion_sort(__first, __first + int(_S_threshold), __comp);
   std::__unguarded_insertion_sort(__first + int(_S_threshold), __last,
       __comp);
 }
      else
 std::__insertion_sort(__first, __last, __comp);
    }


  template<typename _RandomAccessIterator, typename _Compare>
   
    _RandomAccessIterator
    __unguarded_partition(_RandomAccessIterator __first,
     _RandomAccessIterator __last,
     _RandomAccessIterator __pivot, _Compare __comp)
    {
      while (true)
 {
   while (__comp(__first, __pivot))
     ++__first;
   --__last;
   while (__comp(__pivot, __last))
     --__last;
   if (!(__first < __last))
     return __first;
   std::iter_swap(__first, __last);
   ++__first;
 }
    }


  template<typename _RandomAccessIterator, typename _Compare>
   
    inline _RandomAccessIterator
    __unguarded_partition_pivot(_RandomAccessIterator __first,
    _RandomAccessIterator __last, _Compare __comp)
    {
      _RandomAccessIterator __mid = __first + (__last - __first) / 2;
      std::__move_median_to_first(__first, __first + 1, __mid, __last - 1,
      __comp);
      return std::__unguarded_partition(__first + 1, __last, __first, __comp);
    }

  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    __partial_sort(_RandomAccessIterator __first,
     _RandomAccessIterator __middle,
     _RandomAccessIterator __last,
     _Compare __comp)
    {
      std::__heap_select(__first, __middle, __last, __comp);
      std::__sort_heap(__first, __middle, __comp);
    }


  template<typename _RandomAccessIterator, typename _Size, typename _Compare>
   
    void
    __introsort_loop(_RandomAccessIterator __first,
       _RandomAccessIterator __last,
       _Size __depth_limit, _Compare __comp)
    {
      while (__last - __first > int(_S_threshold))
 {
   if (__depth_limit == 0)
     {
       std::__partial_sort(__first, __last, __last, __comp);
       return;
     }
   --__depth_limit;
   _RandomAccessIterator __cut =
     std::__unguarded_partition_pivot(__first, __last, __comp);
   std::__introsort_loop(__cut, __last, __depth_limit, __comp);
   __last = __cut;
 }
    }



  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    __sort(_RandomAccessIterator __first, _RandomAccessIterator __last,
    _Compare __comp)
    {
      if (__first != __last)
 {
   std::__introsort_loop(__first, __last,
    std::__lg(__last - __first) * 2,
    __comp);
   std::__final_insertion_sort(__first, __last, __comp);
 }
    }

  template<typename _RandomAccessIterator, typename _Size, typename _Compare>
   
    void
    __introselect(_RandomAccessIterator __first, _RandomAccessIterator __nth,
    _RandomAccessIterator __last, _Size __depth_limit,
    _Compare __comp)
    {
      while (__last - __first > 3)
 {
   if (__depth_limit == 0)
     {
       std::__heap_select(__first, __nth + 1, __last, __comp);

       std::iter_swap(__first, __nth);
       return;
     }
   --__depth_limit;
   _RandomAccessIterator __cut =
     std::__unguarded_partition_pivot(__first, __last, __comp);
   if (__cut <= __nth)
     __first = __cut;
   else
     __last = __cut;
 }
      std::__insertion_sort(__first, __last, __comp);
    }
# 1944 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp, typename _Compare>
    [[__nodiscard__]]
    inline _ForwardIterator
    lower_bound(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val, _Compare __comp)
    {

     
     

     
                    ;

      return std::__lower_bound(__first, __last, __val,
    __gnu_cxx::__ops::__iter_comp_val(__comp));
    }

  template<typename _ForwardIterator, typename _Tp, typename _Compare>
   
    _ForwardIterator
    __upper_bound(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __val, _Compare __comp)
    {
      typedef typename iterator_traits<_ForwardIterator>::difference_type
 _DistanceType;

      _DistanceType __len = std::distance(__first, __last);

      while (__len > 0)
 {
   _DistanceType __half = __len >> 1;
   _ForwardIterator __middle = __first;
   std::advance(__middle, __half);
   if (__comp(__val, __middle))
     __len = __half;
   else
     {
       __first = __middle;
       ++__first;
       __len = __len - __half - 1;
     }
 }
      return __first;
    }
# 2000 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp>
    [[__nodiscard__]]
    inline _ForwardIterator
    upper_bound(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val)
    {

     
     

      ;

      return std::__upper_bound(__first, __last, __val,
    __gnu_cxx::__ops::__val_less_iter());
    }
# 2031 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp, typename _Compare>
    [[__nodiscard__]]
    inline _ForwardIterator
    upper_bound(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val, _Compare __comp)
    {

     
     

     
                    ;

      return std::__upper_bound(__first, __last, __val,
    __gnu_cxx::__ops::__val_comp_iter(__comp));
    }

  template<typename _ForwardIterator, typename _Tp,
    typename _CompareItTp, typename _CompareTpIt>
   
    pair<_ForwardIterator, _ForwardIterator>
    __equal_range(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __val,
    _CompareItTp __comp_it_val, _CompareTpIt __comp_val_it)
    {
      typedef typename iterator_traits<_ForwardIterator>::difference_type
 _DistanceType;

      _DistanceType __len = std::distance(__first, __last);

      while (__len > 0)
 {
   _DistanceType __half = __len >> 1;
   _ForwardIterator __middle = __first;
   std::advance(__middle, __half);
   if (__comp_it_val(__middle, __val))
     {
       __first = __middle;
       ++__first;
       __len = __len - __half - 1;
     }
   else if (__comp_val_it(__val, __middle))
     __len = __half;
   else
     {
       _ForwardIterator __left
  = std::__lower_bound(__first, __middle, __val, __comp_it_val);
       std::advance(__first, __len);
       _ForwardIterator __right
  = std::__upper_bound(++__middle, __first, __val, __comp_val_it);
       return pair<_ForwardIterator, _ForwardIterator>(__left, __right);
     }
 }
      return pair<_ForwardIterator, _ForwardIterator>(__first, __first);
    }
# 2104 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp>
    [[__nodiscard__]]
    inline pair<_ForwardIterator, _ForwardIterator>
    equal_range(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val)
    {

     
     

     

      ;
      ;

      return std::__equal_range(__first, __last, __val,
    __gnu_cxx::__ops::__iter_less_val(),
    __gnu_cxx::__ops::__val_less_iter());
    }
# 2141 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp, typename _Compare>
    [[__nodiscard__]]
    inline pair<_ForwardIterator, _ForwardIterator>
    equal_range(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val, _Compare __comp)
    {

     
     

     

     
                    ;
     
                    ;

      return std::__equal_range(__first, __last, __val,
    __gnu_cxx::__ops::__iter_comp_val(__comp),
    __gnu_cxx::__ops::__val_comp_iter(__comp));
    }
# 2175 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp>
    [[__nodiscard__]]
    bool
    binary_search(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __val)
    {

     
     

      ;
      ;

      _ForwardIterator __i
 = std::__lower_bound(__first, __last, __val,
        __gnu_cxx::__ops::__iter_less_val());
      return __i != __last && !(__val < *__i);
    }
# 2209 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp, typename _Compare>
    [[__nodiscard__]]
    bool
    binary_search(_ForwardIterator __first, _ForwardIterator __last,
    const _Tp& __val, _Compare __comp)
    {

     
     

     
                    ;
     
                    ;

      _ForwardIterator __i
 = std::__lower_bound(__first, __last, __val,
        __gnu_cxx::__ops::__iter_comp_val(__comp));
      return __i != __last && !bool(__comp(__val, *__i));
    }




  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
    void
    __move_merge_adaptive(_InputIterator1 __first1, _InputIterator1 __last1,
     _InputIterator2 __first2, _InputIterator2 __last2,
     _OutputIterator __result, _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 {
   if (__comp(__first2, __first1))
     {
       *__result = std::move(*__first2);
       ++__first2;
     }
   else
     {
       *__result = std::move(*__first1);
       ++__first1;
     }
   ++__result;
 }
      if (__first1 != __last1)
 std::move(__first1, __last1, __result);
    }


  template<typename _BidirectionalIterator1, typename _BidirectionalIterator2,
    typename _BidirectionalIterator3, typename _Compare>
    void
    __move_merge_adaptive_backward(_BidirectionalIterator1 __first1,
       _BidirectionalIterator1 __last1,
       _BidirectionalIterator2 __first2,
       _BidirectionalIterator2 __last2,
       _BidirectionalIterator3 __result,
       _Compare __comp)
    {
      if (__first1 == __last1)
 {
   std::move_backward(__first2, __last2, __result);
   return;
 }
      else if (__first2 == __last2)
 return;

      --__last1;
      --__last2;
      while (true)
 {
   if (__comp(__last2, __last1))
     {
       *--__result = std::move(*__last1);
       if (__first1 == __last1)
  {
    std::move_backward(__first2, ++__last2, __result);
    return;
  }
       --__last1;
     }
   else
     {
       *--__result = std::move(*__last2);
       if (__first2 == __last2)
  return;
       --__last2;
     }
 }
    }


  template<typename _BidirectionalIterator1, typename _BidirectionalIterator2,
    typename _Distance>
    _BidirectionalIterator1
    __rotate_adaptive(_BidirectionalIterator1 __first,
        _BidirectionalIterator1 __middle,
        _BidirectionalIterator1 __last,
        _Distance __len1, _Distance __len2,
        _BidirectionalIterator2 __buffer,
        _Distance __buffer_size)
    {
      _BidirectionalIterator2 __buffer_end;
      if (__len1 > __len2 && __len2 <= __buffer_size)
 {
   if (__len2)
     {
       __buffer_end = std::move(__middle, __last, __buffer);
       std::move_backward(__first, __middle, __last);
       return std::move(__buffer, __buffer_end, __first);
     }
   else
     return __first;
 }
      else if (__len1 <= __buffer_size)
 {
   if (__len1)
     {
       __buffer_end = std::move(__first, __middle, __buffer);
       std::move(__middle, __last, __first);
       return std::move_backward(__buffer, __buffer_end, __last);
     }
   else
     return __last;
 }
      else
 return std::rotate(__first, __middle, __last);
    }


  template<typename _BidirectionalIterator, typename _Distance,
    typename _Pointer, typename _Compare>
    void
    __merge_adaptive(_BidirectionalIterator __first,
       _BidirectionalIterator __middle,
       _BidirectionalIterator __last,
       _Distance __len1, _Distance __len2,
       _Pointer __buffer, _Compare __comp)
    {
      if (__len1 <= __len2)
 {
   _Pointer __buffer_end = std::move(__first, __middle, __buffer);
   std::__move_merge_adaptive(__buffer, __buffer_end, __middle, __last,
         __first, __comp);
 }
      else
 {
   _Pointer __buffer_end = std::move(__middle, __last, __buffer);
   std::__move_merge_adaptive_backward(__first, __middle, __buffer,
           __buffer_end, __last, __comp);
 }
    }

  template<typename _BidirectionalIterator, typename _Distance,
    typename _Pointer, typename _Compare>
    void
    __merge_adaptive_resize(_BidirectionalIterator __first,
       _BidirectionalIterator __middle,
       _BidirectionalIterator __last,
       _Distance __len1, _Distance __len2,
       _Pointer __buffer, _Distance __buffer_size,
       _Compare __comp)
    {
      if (__len1 <= __buffer_size || __len2 <= __buffer_size)
 std::__merge_adaptive(__first, __middle, __last,
         __len1, __len2, __buffer, __comp);
      else
 {
   _BidirectionalIterator __first_cut = __first;
   _BidirectionalIterator __second_cut = __middle;
   _Distance __len11 = 0;
   _Distance __len22 = 0;
   if (__len1 > __len2)
     {
       __len11 = __len1 / 2;
       std::advance(__first_cut, __len11);
       __second_cut
  = std::__lower_bound(__middle, __last, *__first_cut,
         __gnu_cxx::__ops::__iter_comp_val(__comp));
       __len22 = std::distance(__middle, __second_cut);
     }
   else
     {
       __len22 = __len2 / 2;
       std::advance(__second_cut, __len22);
       __first_cut
  = std::__upper_bound(__first, __middle, *__second_cut,
         __gnu_cxx::__ops::__val_comp_iter(__comp));
       __len11 = std::distance(__first, __first_cut);
     }

   _BidirectionalIterator __new_middle
     = std::__rotate_adaptive(__first_cut, __middle, __second_cut,
         _Distance(__len1 - __len11), __len22,
         __buffer, __buffer_size);
   std::__merge_adaptive_resize(__first, __first_cut, __new_middle,
           __len11, __len22,
           __buffer, __buffer_size, __comp);
   std::__merge_adaptive_resize(__new_middle, __second_cut, __last,
           _Distance(__len1 - __len11),
           _Distance(__len2 - __len22),
           __buffer, __buffer_size, __comp);
 }
    }


  template<typename _BidirectionalIterator, typename _Distance,
    typename _Compare>
    void
    __merge_without_buffer(_BidirectionalIterator __first,
      _BidirectionalIterator __middle,
      _BidirectionalIterator __last,
      _Distance __len1, _Distance __len2,
      _Compare __comp)
    {
      if (__len1 == 0 || __len2 == 0)
 return;

      if (__len1 + __len2 == 2)
 {
   if (__comp(__middle, __first))
     std::iter_swap(__first, __middle);
   return;
 }

      _BidirectionalIterator __first_cut = __first;
      _BidirectionalIterator __second_cut = __middle;
      _Distance __len11 = 0;
      _Distance __len22 = 0;
      if (__len1 > __len2)
 {
   __len11 = __len1 / 2;
   std::advance(__first_cut, __len11);
   __second_cut
     = std::__lower_bound(__middle, __last, *__first_cut,
     __gnu_cxx::__ops::__iter_comp_val(__comp));
   __len22 = std::distance(__middle, __second_cut);
 }
      else
 {
   __len22 = __len2 / 2;
   std::advance(__second_cut, __len22);
   __first_cut
     = std::__upper_bound(__first, __middle, *__second_cut,
     __gnu_cxx::__ops::__val_comp_iter(__comp));
   __len11 = std::distance(__first, __first_cut);
 }

      _BidirectionalIterator __new_middle
 = std::rotate(__first_cut, __middle, __second_cut);
      std::__merge_without_buffer(__first, __first_cut, __new_middle,
      __len11, __len22, __comp);
      std::__merge_without_buffer(__new_middle, __second_cut, __last,
      __len1 - __len11, __len2 - __len22, __comp);
    }

  template<typename _BidirectionalIterator, typename _Compare>
    void
    __inplace_merge(_BidirectionalIterator __first,
      _BidirectionalIterator __middle,
      _BidirectionalIterator __last,
      _Compare __comp)
    {
      typedef typename iterator_traits<_BidirectionalIterator>::value_type
   _ValueType;
      typedef typename iterator_traits<_BidirectionalIterator>::difference_type
   _DistanceType;

      if (__first == __middle || __middle == __last)
 return;

      const _DistanceType __len1 = std::distance(__first, __middle);
      const _DistanceType __len2 = std::distance(__middle, __last);


      typedef _Temporary_buffer<_BidirectionalIterator, _ValueType> _TmpBuf;


      _TmpBuf __buf(__first, std::min(__len1, __len2));

      if (__builtin_expect(__buf.size() == __buf.requested_size(), true))
 std::__merge_adaptive
   (__first, __middle, __last, __len1, __len2, __buf.begin(), __comp);
      else if (__builtin_expect(__buf.begin() == 0, false))
 std::__merge_without_buffer
   (__first, __middle, __last, __len1, __len2, __comp);
      else
 std::__merge_adaptive_resize
   (__first, __middle, __last, __len1, __len2, __buf.begin(),
    _DistanceType(__buf.size()), __comp);




    }
# 2524 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator>
    inline void
    inplace_merge(_BidirectionalIterator __first,
    _BidirectionalIterator __middle,
    _BidirectionalIterator __last)
    {

     

     

      ;
      ;
      ;

      std::__inplace_merge(__first, __middle, __last,
      __gnu_cxx::__ops::__iter_less_iter());
    }
# 2565 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator, typename _Compare>
    inline void
    inplace_merge(_BidirectionalIterator __first,
    _BidirectionalIterator __middle,
    _BidirectionalIterator __last,
    _Compare __comp)
    {

     

     


      ;
      ;
      ;

      std::__inplace_merge(__first, __middle, __last,
      __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }



  template<typename _InputIterator, typename _OutputIterator,
    typename _Compare>
    _OutputIterator
    __move_merge(_InputIterator __first1, _InputIterator __last1,
   _InputIterator __first2, _InputIterator __last2,
   _OutputIterator __result, _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 {
   if (__comp(__first2, __first1))
     {
       *__result = std::move(*__first2);
       ++__first2;
     }
   else
     {
       *__result = std::move(*__first1);
       ++__first1;
     }
   ++__result;
 }
      return std::move(__first2, __last2, std::move(__first1, __last1, __result))

                  ;
    }

  template<typename _RandomAccessIterator1, typename _RandomAccessIterator2,
    typename _Distance, typename _Compare>
    void
    __merge_sort_loop(_RandomAccessIterator1 __first,
        _RandomAccessIterator1 __last,
        _RandomAccessIterator2 __result, _Distance __step_size,
        _Compare __comp)
    {
      const _Distance __two_step = 2 * __step_size;

      while (__last - __first >= __two_step)
 {
   __result = std::__move_merge(__first, __first + __step_size,
           __first + __step_size,
           __first + __two_step,
           __result, __comp);
   __first += __two_step;
 }
      __step_size = std::min(_Distance(__last - __first), __step_size);

      std::__move_merge(__first, __first + __step_size,
   __first + __step_size, __last, __result, __comp);
    }

  template<typename _RandomAccessIterator, typename _Distance,
    typename _Compare>
   
    void
    __chunk_insertion_sort(_RandomAccessIterator __first,
      _RandomAccessIterator __last,
      _Distance __chunk_size, _Compare __comp)
    {
      while (__last - __first >= __chunk_size)
 {
   std::__insertion_sort(__first, __first + __chunk_size, __comp);
   __first += __chunk_size;
 }
      std::__insertion_sort(__first, __last, __comp);
    }

  enum { _S_chunk_size = 7 };

  template<typename _RandomAccessIterator, typename _Pointer, typename _Compare>
    void
    __merge_sort_with_buffer(_RandomAccessIterator __first,
        _RandomAccessIterator __last,
        _Pointer __buffer, _Compare __comp)
    {
      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
 _Distance;

      const _Distance __len = __last - __first;
      const _Pointer __buffer_last = __buffer + __len;

      _Distance __step_size = _S_chunk_size;
      std::__chunk_insertion_sort(__first, __last, __step_size, __comp);

      while (__step_size < __len)
 {
   std::__merge_sort_loop(__first, __last, __buffer,
     __step_size, __comp);
   __step_size *= 2;
   std::__merge_sort_loop(__buffer, __buffer_last, __first,
     __step_size, __comp);
   __step_size *= 2;
 }
    }

  template<typename _RandomAccessIterator, typename _Pointer, typename _Compare>
    void
    __stable_sort_adaptive(_RandomAccessIterator __first,
      _RandomAccessIterator __middle,
      _RandomAccessIterator __last,
      _Pointer __buffer, _Compare __comp)
    {
      std::__merge_sort_with_buffer(__first, __middle, __buffer, __comp);
      std::__merge_sort_with_buffer(__middle, __last, __buffer, __comp);

      std::__merge_adaptive(__first, __middle, __last,
       __middle - __first, __last - __middle,
       __buffer, __comp);
    }

  template<typename _RandomAccessIterator, typename _Pointer,
    typename _Distance, typename _Compare>
    void
    __stable_sort_adaptive_resize(_RandomAccessIterator __first,
      _RandomAccessIterator __last,
      _Pointer __buffer, _Distance __buffer_size,
      _Compare __comp)
    {
      const _Distance __len = (__last - __first + 1) / 2;
      const _RandomAccessIterator __middle = __first + __len;
      if (__len > __buffer_size)
 {
   std::__stable_sort_adaptive_resize(__first, __middle, __buffer,
          __buffer_size, __comp);
   std::__stable_sort_adaptive_resize(__middle, __last, __buffer,
          __buffer_size, __comp);
   std::__merge_adaptive_resize(__first, __middle, __last,
           _Distance(__middle - __first),
           _Distance(__last - __middle),
           __buffer, __buffer_size,
           __comp);
 }
      else
 std::__stable_sort_adaptive(__first, __middle, __last,
        __buffer, __comp);
    }


  template<typename _RandomAccessIterator, typename _Compare>
    void
    __inplace_stable_sort(_RandomAccessIterator __first,
     _RandomAccessIterator __last, _Compare __comp)
    {
      if (__last - __first < 15)
 {
   std::__insertion_sort(__first, __last, __comp);
   return;
 }
      _RandomAccessIterator __middle = __first + (__last - __first) / 2;
      std::__inplace_stable_sort(__first, __middle, __comp);
      std::__inplace_stable_sort(__middle, __last, __comp);
      std::__merge_without_buffer(__first, __middle, __last,
      __middle - __first,
      __last - __middle,
      __comp);
    }
# 2751 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _Compare>
   
    bool
    __includes(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _InputIterator2 __last2,
        _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 {
   if (__comp(__first2, __first1))
     return false;
   if (!__comp(__first1, __first2))
     ++__first2;
   ++__first1;
 }

      return __first2 == __last2;
    }
# 2789 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2>
    [[__nodiscard__]]
    inline bool
    includes(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _InputIterator2 __last2)
    {

     
     
     


     


      ;
      ;
      ;
      ;

      return std::__includes(__first1, __last1, __first2, __last2,
        __gnu_cxx::__ops::__iter_less_iter());
    }
# 2834 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _Compare>
    [[__nodiscard__]]
    inline bool
    includes(_InputIterator1 __first1, _InputIterator1 __last1,
      _InputIterator2 __first2, _InputIterator2 __last2,
      _Compare __comp)
    {

     
     
     


     


      ;
      ;
      ;
      ;

      return std::__includes(__first1, __last1, __first2, __last2,
        __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 2870 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator, typename _Compare>
   
    bool
    __next_permutation(_BidirectionalIterator __first,
         _BidirectionalIterator __last, _Compare __comp)
    {
      if (__first == __last)
 return false;
      _BidirectionalIterator __i = __first;
      ++__i;
      if (__i == __last)
 return false;
      __i = __last;
      --__i;

      for(;;)
 {
   _BidirectionalIterator __ii = __i;
   --__i;
   if (__comp(__i, __ii))
     {
       _BidirectionalIterator __j = __last;
       while (!__comp(__i, --__j))
  {}
       std::iter_swap(__i, __j);
       std::__reverse(__ii, __last,
        std::__iterator_category(__first));
       return true;
     }
   if (__i == __first)
     {
       std::__reverse(__first, __last,
        std::__iterator_category(__first));
       return false;
     }
 }
    }
# 2920 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator>
   
    inline bool
    next_permutation(_BidirectionalIterator __first,
       _BidirectionalIterator __last)
    {

     

     

      ;
      ;

      return std::__next_permutation
 (__first, __last, __gnu_cxx::__ops::__iter_less_iter());
    }
# 2953 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator, typename _Compare>
   
    inline bool
    next_permutation(_BidirectionalIterator __first,
       _BidirectionalIterator __last, _Compare __comp)
    {

     

     


      ;
      ;

      return std::__next_permutation
 (__first, __last, __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _BidirectionalIterator, typename _Compare>
   
    bool
    __prev_permutation(_BidirectionalIterator __first,
         _BidirectionalIterator __last, _Compare __comp)
    {
      if (__first == __last)
 return false;
      _BidirectionalIterator __i = __first;
      ++__i;
      if (__i == __last)
 return false;
      __i = __last;
      --__i;

      for(;;)
 {
   _BidirectionalIterator __ii = __i;
   --__i;
   if (__comp(__ii, __i))
     {
       _BidirectionalIterator __j = __last;
       while (!__comp(--__j, __i))
  {}
       std::iter_swap(__i, __j);
       std::__reverse(__ii, __last,
        std::__iterator_category(__first));
       return true;
     }
   if (__i == __first)
     {
       std::__reverse(__first, __last,
        std::__iterator_category(__first));
       return false;
     }
 }
    }
# 3023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator>
   
    inline bool
    prev_permutation(_BidirectionalIterator __first,
       _BidirectionalIterator __last)
    {

     

     

      ;
      ;

      return std::__prev_permutation(__first, __last,
         __gnu_cxx::__ops::__iter_less_iter());
    }
# 3056 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _BidirectionalIterator, typename _Compare>
   
    inline bool
    prev_permutation(_BidirectionalIterator __first,
       _BidirectionalIterator __last, _Compare __comp)
    {

     

     


      ;
      ;

      return std::__prev_permutation(__first, __last,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }




  template<typename _InputIterator, typename _OutputIterator,
    typename _Predicate, typename _Tp>
   
    _OutputIterator
    __replace_copy_if(_InputIterator __first, _InputIterator __last,
        _OutputIterator __result,
        _Predicate __pred, const _Tp& __new_value)
    {
      for (; __first != __last; ++__first, (void)++__result)
 if (__pred(__first))
   *__result = __new_value;
 else
   *__result = *__first;
      return __result;
    }
# 3108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator, typename _Tp>
   
    inline _OutputIterator
    replace_copy(_InputIterator __first, _InputIterator __last,
   _OutputIterator __result,
   const _Tp& __old_value, const _Tp& __new_value)
    {

     
     

     

      ;

      return std::__replace_copy_if(__first, __last, __result,
   __gnu_cxx::__ops::__iter_equals_val(__old_value),
           __new_value);
    }
# 3143 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator,
    typename _Predicate, typename _Tp>
   
    inline _OutputIterator
    replace_copy_if(_InputIterator __first, _InputIterator __last,
      _OutputIterator __result,
      _Predicate __pred, const _Tp& __new_value)
    {

     
     

     

      ;

      return std::__replace_copy_if(__first, __last, __result,
    __gnu_cxx::__ops::__pred_iter(__pred),
           __new_value);
    }
# 3172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]]
    inline bool
    is_sorted(_ForwardIterator __first, _ForwardIterator __last)
    { return std::is_sorted_until(__first, __last) == __last; }
# 3187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Compare>
    [[__nodiscard__]]
    inline bool
    is_sorted(_ForwardIterator __first, _ForwardIterator __last,
       _Compare __comp)
    { return std::is_sorted_until(__first, __last, __comp) == __last; }

  template<typename _ForwardIterator, typename _Compare>
   
    _ForwardIterator
    __is_sorted_until(_ForwardIterator __first, _ForwardIterator __last,
        _Compare __comp)
    {
      if (__first == __last)
 return __last;

      _ForwardIterator __next = __first;
      for (++__next; __next != __last; __first = __next, (void)++__next)
 if (__comp(__next, __first))
   return __next;
      return __next;
    }
# 3218 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]]
    inline _ForwardIterator
    is_sorted_until(_ForwardIterator __first, _ForwardIterator __last)
    {

     
     

      ;
      ;

      return std::__is_sorted_until(__first, __last,
        __gnu_cxx::__ops::__iter_less_iter());
    }
# 3243 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Compare>
    [[__nodiscard__]]
    inline _ForwardIterator
    is_sorted_until(_ForwardIterator __first, _ForwardIterator __last,
      _Compare __comp)
    {

     
     


      ;
      ;

      return std::__is_sorted_until(__first, __last,
        __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 3269 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _Tp>
    [[__nodiscard__]] constexpr
    inline pair<const _Tp&, const _Tp&>
    minmax(const _Tp& __a, const _Tp& __b)
    {

     

      return __b < __a ? pair<const _Tp&, const _Tp&>(__b, __a)
         : pair<const _Tp&, const _Tp&>(__a, __b);
    }
# 3290 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]] constexpr
    inline pair<const _Tp&, const _Tp&>
    minmax(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {
      return __comp(__b, __a) ? pair<const _Tp&, const _Tp&>(__b, __a)
         : pair<const _Tp&, const _Tp&>(__a, __b);
    }

  template<typename _ForwardIterator, typename _Compare>
    constexpr
    pair<_ForwardIterator, _ForwardIterator>
    __minmax_element(_ForwardIterator __first, _ForwardIterator __last,
       _Compare __comp)
    {
      _ForwardIterator __next = __first;
      if (__first == __last
   || ++__next == __last)
 return std::make_pair(__first, __first);

      _ForwardIterator __min{}, __max{};
      if (__comp(__next, __first))
 {
   __min = __next;
   __max = __first;
 }
      else
 {
   __min = __first;
   __max = __next;
 }

      __first = __next;
      ++__first;

      while (__first != __last)
 {
   __next = __first;
   if (++__next == __last)
     {
       if (__comp(__first, __min))
  __min = __first;
       else if (!__comp(__first, __max))
  __max = __first;
       break;
     }

   if (__comp(__next, __first))
     {
       if (__comp(__next, __min))
  __min = __next;
       if (!__comp(__first, __max))
  __max = __first;
     }
   else
     {
       if (__comp(__first, __min))
  __min = __first;
       if (!__comp(__next, __max))
  __max = __next;
     }

   __first = __next;
   ++__first;
 }

      return std::make_pair(__min, __max);
    }
# 3370 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]] constexpr
    inline pair<_ForwardIterator, _ForwardIterator>
    minmax_element(_ForwardIterator __first, _ForwardIterator __last)
    {

     
     

      ;
      ;

      return std::__minmax_element(__first, __last,
       __gnu_cxx::__ops::__iter_less_iter());
    }
# 3398 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Compare>
    [[__nodiscard__]] constexpr
    inline pair<_ForwardIterator, _ForwardIterator>
    minmax_element(_ForwardIterator __first, _ForwardIterator __last,
     _Compare __comp)
    {

     
     


      ;
      ;

      return std::__minmax_element(__first, __last,
       __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _Tp>
    [[__nodiscard__]] constexpr
    inline pair<_Tp, _Tp>
    minmax(initializer_list<_Tp> __l)
    {
      ;
      pair<const _Tp*, const _Tp*> __p =
 std::__minmax_element(__l.begin(), __l.end(),
         __gnu_cxx::__ops::__iter_less_iter());
      return std::make_pair(*__p.first, *__p.second);
    }

  template<typename _Tp, typename _Compare>
    [[__nodiscard__]] constexpr
    inline pair<_Tp, _Tp>
    minmax(initializer_list<_Tp> __l, _Compare __comp)
    {
      ;
      pair<const _Tp*, const _Tp*> __p =
 std::__minmax_element(__l.begin(), __l.end(),
         __gnu_cxx::__ops::__iter_comp_iter(__comp));
      return std::make_pair(*__p.first, *__p.second);
    }
# 3454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline bool
    is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
     _ForwardIterator2 __first2, _BinaryPredicate __pred)
    {

     
     
     


      ;

      return std::__is_permutation(__first1, __last1, __first2,
       __gnu_cxx::__ops::__iter_comp_iter(__pred));
    }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
   
    bool
    __is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
       _ForwardIterator2 __first2, _ForwardIterator2 __last2,
       _BinaryPredicate __pred)
    {
      using _Cat1
 = typename iterator_traits<_ForwardIterator1>::iterator_category;
      using _Cat2
 = typename iterator_traits<_ForwardIterator2>::iterator_category;
      using _It1_is_RA = is_same<_Cat1, random_access_iterator_tag>;
      using _It2_is_RA = is_same<_Cat2, random_access_iterator_tag>;
      constexpr bool __ra_iters = __and_<_It1_is_RA, _It2_is_RA>::value;
      if constexpr (__ra_iters)
 {
   if ((__last1 - __first1) != (__last2 - __first2))
     return false;
 }



      for (; __first1 != __last1 && __first2 != __last2;
   ++__first1, (void)++__first2)
 if (!__pred(__first1, __first2))
   break;

      if constexpr (__ra_iters)
 {
   if (__first1 == __last1)
     return true;
 }
      else
 {
   auto __d1 = std::distance(__first1, __last1);
   auto __d2 = std::distance(__first2, __last2);
   if (__d1 == 0 && __d2 == 0)
     return true;
   if (__d1 != __d2)
     return false;
 }

      for (_ForwardIterator1 __scan = __first1; __scan != __last1; ++__scan)
 {
   if (__scan != std::__find_if(__first1, __scan,
   __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan)))
     continue;

   auto __matches = std::__count_if(__first2, __last2,
  __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan));
   if (0 == __matches
       || std::__count_if(__scan, __last1,
   __gnu_cxx::__ops::__iter_comp_iter(__pred, __scan))
       != __matches)
     return false;
 }
      return true;
    }
#pragma GCC diagnostic pop
# 3550 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
    [[__nodiscard__]]
    inline bool
    is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
     _ForwardIterator2 __first2, _ForwardIterator2 __last2)
    {
      ;
      ;

      return
 std::__is_permutation(__first1, __last1, __first2, __last2,
         __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 3578 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline bool
    is_permutation(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
     _ForwardIterator2 __first2, _ForwardIterator2 __last2,
     _BinaryPredicate __pred)
    {
      ;
      ;

      return std::__is_permutation(__first1, __last1, __first2, __last2,
       __gnu_cxx::__ops::__iter_comp_iter(__pred));
    }
# 3656 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _IntType, typename _UniformRandomBitGenerator>
    pair<_IntType, _IntType>
    __gen_two_uniform_ints(_IntType __b0, _IntType __b1,
      _UniformRandomBitGenerator&& __g)
    {
      _IntType __x
 = uniform_int_distribution<_IntType>{0, (__b0 * __b1) - 1}(__g);
      return std::make_pair(__x / __b1, __x % __b1);
    }
# 3678 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator,
    typename _UniformRandomNumberGenerator>
    void
    shuffle(_RandomAccessIterator __first, _RandomAccessIterator __last,
     _UniformRandomNumberGenerator&& __g)
    {

     

      ;

      if (__first == __last)
 return;

      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
 _DistanceType;

      typedef typename std::make_unsigned<_DistanceType>::type __ud_type;
      typedef typename std::uniform_int_distribution<__ud_type> __distr_type;
      typedef typename __distr_type::param_type __p_type;

      typedef typename remove_reference<_UniformRandomNumberGenerator>::type
 _Gen;
      typedef typename common_type<typename _Gen::result_type, __ud_type>::type
 __uc_type;

      const __uc_type __urngrange = __g.max() - __g.min();
      const __uc_type __urange = __uc_type(__last - __first);

      if (__urngrange / __urange >= __urange)

      {
 _RandomAccessIterator __i = __first + 1;





 if ((__urange % 2) == 0)
 {
   __distr_type __d{0, 1};
   std::iter_swap(__i++, __first + __d(__g));
 }





 while (__i != __last)
 {
   const __uc_type __swap_range = __uc_type(__i - __first) + 1;

   const pair<__uc_type, __uc_type> __pospos =
     __gen_two_uniform_ints(__swap_range, __swap_range + 1, __g);

   std::iter_swap(__i++, __first + __pospos.first);
   std::iter_swap(__i++, __first + __pospos.second);
 }

 return;
      }

      __distr_type __d;

      for (_RandomAccessIterator __i = __first + 1; __i != __last; ++__i)
 std::iter_swap(__i, __first + __d(__g, __p_type(0, __i - __first)));
    }



# 3761 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Function>
   
    _Function
    for_each(_InputIterator __first, _InputIterator __last, _Function __f)
    {

     
      ;
      for (; __first != __last; ++__first)
 __f(*__first);
      return __f;
    }
# 3823 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Tp>
    [[__nodiscard__]]
    inline _InputIterator
    find(_InputIterator __first, _InputIterator __last, const _Tp& __val)
    {

     
     

      ;
# 3867 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
      return std::__find_if(__first, __last,
       __gnu_cxx::__ops::__iter_equals_val(__val));
    }
# 3881 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline _InputIterator
    find_if(_InputIterator __first, _InputIterator __last,
     _Predicate __pred)
    {

     
     

      ;

      return std::__find_if(__first, __last,
       __gnu_cxx::__ops::__pred_iter(__pred));
    }
# 3913 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _ForwardIterator>
    [[__nodiscard__]]
    _InputIterator
    find_first_of(_InputIterator __first1, _InputIterator __last1,
    _ForwardIterator __first2, _ForwardIterator __last2)
    {

     
     
     


      ;
      ;

      for (; __first1 != __last1; ++__first1)
 for (_ForwardIterator __iter = __first2; __iter != __last2; ++__iter)
   if (*__first1 == *__iter)
     return __first1;
      return __last1;
    }
# 3954 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _ForwardIterator,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    _InputIterator
    find_first_of(_InputIterator __first1, _InputIterator __last1,
    _ForwardIterator __first2, _ForwardIterator __last2,
    _BinaryPredicate __comp)
    {

     
     
     


      ;
      ;

      for (; __first1 != __last1; ++__first1)
 for (_ForwardIterator __iter = __first2; __iter != __last2; ++__iter)
   if (__comp(*__first1, *__iter))
     return __first1;
      return __last1;
    }
# 3987 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]]
    inline _ForwardIterator
    adjacent_find(_ForwardIterator __first, _ForwardIterator __last)
    {

     
     

      ;

      return std::__adjacent_find(__first, __last,
      __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 4013 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _BinaryPredicate>
    [[__nodiscard__]]
    inline _ForwardIterator
    adjacent_find(_ForwardIterator __first, _ForwardIterator __last,
    _BinaryPredicate __binary_pred)
    {

     
     


      ;

      return std::__adjacent_find(__first, __last,
   __gnu_cxx::__ops::__iter_comp_iter(__binary_pred));
    }
# 4039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Tp>
    [[__nodiscard__]]
    inline typename iterator_traits<_InputIterator>::difference_type
    count(_InputIterator __first, _InputIterator __last, const _Tp& __value)
    {

     
     

      ;

      return std::__count_if(__first, __last,
        __gnu_cxx::__ops::__iter_equals_val(__value));
    }
# 4063 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _Predicate>
    [[__nodiscard__]]
    inline typename iterator_traits<_InputIterator>::difference_type
    count_if(_InputIterator __first, _InputIterator __last, _Predicate __pred)
    {

     
     

      ;

      return std::__count_if(__first, __last,
        __gnu_cxx::__ops::__pred_iter(__pred));
    }
# 4104 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
    [[__nodiscard__]]
    inline _ForwardIterator1
    search(_ForwardIterator1 __first1, _ForwardIterator1 __last1,
    _ForwardIterator2 __first2, _ForwardIterator2 __last2)
    {

     
     
     


      ;
      ;

      return std::__search(__first1, __last1, __first2, __last2,
      __gnu_cxx::__ops::__iter_equal_to_iter());
    }
# 4138 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Integer, typename _Tp>
    [[__nodiscard__]]
    inline _ForwardIterator
    search_n(_ForwardIterator __first, _ForwardIterator __last,
      _Integer __count, const _Tp& __val)
    {

     
     

      ;

      return std::__search_n(__first, __last, __count,
        __gnu_cxx::__ops::__iter_equals_val(__val));
    }
# 4172 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Integer, typename _Tp,
    typename _BinaryPredicate>
    [[__nodiscard__]]
    inline _ForwardIterator
    search_n(_ForwardIterator __first, _ForwardIterator __last,
      _Integer __count, const _Tp& __val,
      _BinaryPredicate __binary_pred)
    {

     
     

      ;

      return std::__search_n(__first, __last, __count,
  __gnu_cxx::__ops::__iter_comp_val(__binary_pred, __val));
    }
# 4222 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator,
    typename _UnaryOperation>
   
    _OutputIterator
    transform(_InputIterator __first, _InputIterator __last,
       _OutputIterator __result, _UnaryOperation __unary_op)
    {

     
     


      ;

      for (; __first != __last; ++__first, (void)++__result)
 *__result = __unary_op(*__first);
      return __result;
    }
# 4260 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _BinaryOperation>
   
    _OutputIterator
    transform(_InputIterator1 __first1, _InputIterator1 __last1,
       _InputIterator2 __first2, _OutputIterator __result,
       _BinaryOperation __binary_op)
    {

     
     
     


      ;

      for (; __first1 != __last1; ++__first1, (void)++__first2, ++__result)
 *__result = __binary_op(*__first1, *__first2);
      return __result;
    }
# 4294 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Tp>
   
    void
    replace(_ForwardIterator __first, _ForwardIterator __last,
     const _Tp& __old_value, const _Tp& __new_value)
    {

     

     

     

      ;

      for (; __first != __last; ++__first)
 if (*__first == __old_value)
   *__first = __new_value;
    }
# 4327 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Predicate, typename _Tp>
   
    void
    replace_if(_ForwardIterator __first, _ForwardIterator __last,
        _Predicate __pred, const _Tp& __new_value)
    {

     

     

     

      ;

      for (; __first != __last; ++__first)
 if (__pred(*__first))
   *__first = __new_value;
    }
# 4359 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Generator>
   
    void
    generate(_ForwardIterator __first, _ForwardIterator __last,
      _Generator __gen)
    {

     
     

      ;

      for (; __first != __last; ++__first)
 *__first = __gen();
    }
# 4392 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _OutputIterator, typename _Size, typename _Generator>
   
    _OutputIterator
    generate_n(_OutputIterator __first, _Size __n, _Generator __gen)
    {

     



      typedef __decltype(std::__size_to_integer(__n)) _IntSize;
      for (_IntSize __niter = std::__size_to_integer(__n);
    __niter > 0; --__niter, (void) ++__first)
 *__first = __gen();
      return __first;
    }
# 4427 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator>
   
    inline _OutputIterator
    unique_copy(_InputIterator __first, _InputIterator __last,
  _OutputIterator __result)
    {

     
     

     

      ;

      if (__first == __last)
 return __result;
      return std::__unique_copy(__first, __last, __result,
    __gnu_cxx::__ops::__iter_equal_to_iter(),
    std::__iterator_category(__first),
    std::__iterator_category(__result));
    }
# 4467 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator, typename _OutputIterator,
    typename _BinaryPredicate>
   
    inline _OutputIterator
    unique_copy(_InputIterator __first, _InputIterator __last,
  _OutputIterator __result,
  _BinaryPredicate __binary_pred)
    {

     
     

      ;

      if (__first == __last)
 return __result;
      return std::__unique_copy(__first, __last, __result,
   __gnu_cxx::__ops::__iter_comp_iter(__binary_pred),
    std::__iterator_category(__first),
    std::__iterator_category(__result));
    }
# 4506 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator>
    __attribute__ ((__deprecated__ ("use '" "std::shuffle" "' instead")))
    inline void
    random_shuffle(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

      ;

      if (__first == __last)
 return;
# 4541 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
      for (_RandomAccessIterator __i = __first + 1; __i != __last; ++__i)
 {

   _RandomAccessIterator __j = __first
     + (std::rand() % ((__i - __first) + 1));
   if (__i != __j)
     std::iter_swap(__i, __j);
 }
    }
# 4569 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator, typename _RandomNumberGenerator>
    __attribute__ ((__deprecated__ ("use '" "std::shuffle" "' instead")))
    void
    random_shuffle(_RandomAccessIterator __first, _RandomAccessIterator __last,

     _RandomNumberGenerator&& __rand)



    {

     

      ;

      if (__first == __last)
 return;
      for (_RandomAccessIterator __i = __first + 1; __i != __last; ++__i)
 {
   _RandomAccessIterator __j = __first + __rand((__i - __first) + 1);
   if (__i != __j)
     std::iter_swap(__i, __j);
 }
    }
# 4611 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Predicate>
   
    inline _ForwardIterator
    partition(_ForwardIterator __first, _ForwardIterator __last,
       _Predicate __pred)
    {

     

     

      ;

      return std::__partition(__first, __last, __pred,
         std::__iterator_category(__first));
    }
# 4646 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    partial_sort(_RandomAccessIterator __first,
   _RandomAccessIterator __middle,
   _RandomAccessIterator __last)
    {

     

     

      ;
      ;
      ;

      std::__partial_sort(__first, __middle, __last,
     __gnu_cxx::__ops::__iter_less_iter());
    }
# 4685 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    partial_sort(_RandomAccessIterator __first,
   _RandomAccessIterator __middle,
   _RandomAccessIterator __last,
   _Compare __comp)
    {

     

     


      ;
      ;
      ;

      std::__partial_sort(__first, __middle, __last,
     __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 4722 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    nth_element(_RandomAccessIterator __first, _RandomAccessIterator __nth,
  _RandomAccessIterator __last)
    {

     

     

      ;
      ;
      ;

      if (__first == __last || __nth == __last)
 return;

      std::__introselect(__first, __nth, __last,
    std::__lg(__last - __first) * 2,
    __gnu_cxx::__ops::__iter_less_iter());
    }
# 4762 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    nth_element(_RandomAccessIterator __first, _RandomAccessIterator __nth,
  _RandomAccessIterator __last, _Compare __comp)
    {

     

     


      ;
      ;
      ;

      if (__first == __last || __nth == __last)
 return;

      std::__introselect(__first, __nth, __last,
    std::__lg(__last - __first) * 2,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }
# 4800 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator>
   
    inline void
    sort(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

     

      ;
      ;

      std::__sort(__first, __last, __gnu_cxx::__ops::__iter_less_iter());
    }
# 4831 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
   
    inline void
    sort(_RandomAccessIterator __first, _RandomAccessIterator __last,
  _Compare __comp)
    {

     

     


      ;
      ;

      std::__sort(__first, __last, __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
   
    _OutputIterator
    __merge(_InputIterator1 __first1, _InputIterator1 __last1,
     _InputIterator2 __first2, _InputIterator2 __last2,
     _OutputIterator __result, _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 {
   if (__comp(__first2, __first1))
     {
       *__result = *__first2;
       ++__first2;
     }
   else
     {
       *__result = *__first1;
       ++__first1;
     }
   ++__result;
 }
      return std::copy(__first2, __last2,
         std::copy(__first1, __last1, __result));
    }
# 4894 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator>
   
    inline _OutputIterator
    merge(_InputIterator1 __first1, _InputIterator1 __last1,
   _InputIterator2 __first2, _InputIterator2 __last2,
   _OutputIterator __result)
    {

     
     
     

     

     


      ;
      ;
      ;
      ;

      return std::__merge(__first1, __last1,
         __first2, __last2, __result,
         __gnu_cxx::__ops::__iter_less_iter());
    }
# 4945 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
   
    inline _OutputIterator
    merge(_InputIterator1 __first1, _InputIterator1 __last1,
   _InputIterator2 __first2, _InputIterator2 __last2,
   _OutputIterator __result, _Compare __comp)
    {

     
     
     

     

     


      ;
      ;
      ;
      ;

      return std::__merge(__first1, __last1,
    __first2, __last2, __result,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _RandomAccessIterator, typename _Compare>
    inline void
    __stable_sort(_RandomAccessIterator __first, _RandomAccessIterator __last,
    _Compare __comp)
    {
      typedef typename iterator_traits<_RandomAccessIterator>::value_type
 _ValueType;
      typedef typename iterator_traits<_RandomAccessIterator>::difference_type
 _DistanceType;

      if (__first == __last)
 return;


      typedef _Temporary_buffer<_RandomAccessIterator, _ValueType> _TmpBuf;


      _TmpBuf __buf(__first, (__last - __first + 1) / 2);

      if (__builtin_expect(__buf.requested_size() == __buf.size(), true))
 std::__stable_sort_adaptive(__first,
        __first + _DistanceType(__buf.size()),
        __last, __buf.begin(), __comp);
      else if (__builtin_expect(__buf.begin() == 0, false))
 std::__inplace_stable_sort(__first, __last, __comp);
      else
 std::__stable_sort_adaptive_resize(__first, __last, __buf.begin(),
        _DistanceType(__buf.size()), __comp);



    }
# 5023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator>
    inline void
    stable_sort(_RandomAccessIterator __first, _RandomAccessIterator __last)
    {

     

     

      ;
      ;

      std::__stable_sort(__first, __last,
        __gnu_cxx::__ops::__iter_less_iter());
    }
# 5057 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _RandomAccessIterator, typename _Compare>
    inline void
    stable_sort(_RandomAccessIterator __first, _RandomAccessIterator __last,
  _Compare __comp)
    {

     

     


      ;
      ;

      std::__stable_sort(__first, __last,
        __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator,
    typename _Compare>
   
    _OutputIterator
    __set_union(_InputIterator1 __first1, _InputIterator1 __last1,
  _InputIterator2 __first2, _InputIterator2 __last2,
  _OutputIterator __result, _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 {
   if (__comp(__first1, __first2))
     {
       *__result = *__first1;
       ++__first1;
     }
   else if (__comp(__first2, __first1))
     {
       *__result = *__first2;
       ++__first2;
     }
   else
     {
       *__result = *__first1;
       ++__first1;
       ++__first2;
     }
   ++__result;
 }
      return std::copy(__first2, __last2,
         std::copy(__first1, __last1, __result));
    }
# 5127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator>
   
    inline _OutputIterator
    set_union(_InputIterator1 __first1, _InputIterator1 __last1,
       _InputIterator2 __first2, _InputIterator2 __last2,
       _OutputIterator __result)
    {

     
     
     

     

     


     


      ;
      ;
      ;
      ;

      return std::__set_union(__first1, __last1,
    __first2, __last2, __result,
    __gnu_cxx::__ops::__iter_less_iter());
    }
# 5178 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
   
    inline _OutputIterator
    set_union(_InputIterator1 __first1, _InputIterator1 __last1,
       _InputIterator2 __first2, _InputIterator2 __last2,
       _OutputIterator __result, _Compare __comp)
    {

     
     
     

     

     


     


      ;
      ;
      ;
      ;

      return std::__set_union(__first1, __last1,
    __first2, __last2, __result,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator,
    typename _Compare>
   
    _OutputIterator
    __set_intersection(_InputIterator1 __first1, _InputIterator1 __last1,
         _InputIterator2 __first2, _InputIterator2 __last2,
         _OutputIterator __result, _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 if (__comp(__first1, __first2))
   ++__first1;
 else if (__comp(__first2, __first1))
   ++__first2;
 else
   {
     *__result = *__first1;
     ++__first1;
     ++__first2;
     ++__result;
   }
      return __result;
    }
# 5251 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator>
   
    inline _OutputIterator
    set_intersection(_InputIterator1 __first1, _InputIterator1 __last1,
       _InputIterator2 __first2, _InputIterator2 __last2,
       _OutputIterator __result)
    {

     
     
     

     


     


      ;
      ;
      ;
      ;

      return std::__set_intersection(__first1, __last1,
         __first2, __last2, __result,
         __gnu_cxx::__ops::__iter_less_iter());
    }
# 5301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
   
    inline _OutputIterator
    set_intersection(_InputIterator1 __first1, _InputIterator1 __last1,
       _InputIterator2 __first2, _InputIterator2 __last2,
       _OutputIterator __result, _Compare __comp)
    {

     
     
     

     


     


      ;
      ;
      ;
      ;

      return std::__set_intersection(__first1, __last1,
    __first2, __last2, __result,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator,
    typename _Compare>
   
    _OutputIterator
    __set_difference(_InputIterator1 __first1, _InputIterator1 __last1,
       _InputIterator2 __first2, _InputIterator2 __last2,
       _OutputIterator __result, _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 if (__comp(__first1, __first2))
   {
     *__result = *__first1;
     ++__first1;
     ++__result;
   }
 else if (__comp(__first2, __first1))
   ++__first2;
 else
   {
     ++__first1;
     ++__first2;
   }
      return std::copy(__first1, __last1, __result);
    }
# 5376 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator>
   
    inline _OutputIterator
    set_difference(_InputIterator1 __first1, _InputIterator1 __last1,
     _InputIterator2 __first2, _InputIterator2 __last2,
     _OutputIterator __result)
    {

     
     
     

     


     


      ;
      ;
      ;
      ;

      return std::__set_difference(__first1, __last1,
       __first2, __last2, __result,
       __gnu_cxx::__ops::__iter_less_iter());
    }
# 5428 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
   
    inline _OutputIterator
    set_difference(_InputIterator1 __first1, _InputIterator1 __last1,
     _InputIterator2 __first2, _InputIterator2 __last2,
     _OutputIterator __result, _Compare __comp)
    {

     
     
     

     


     


      ;
      ;
      ;
      ;

      return std::__set_difference(__first1, __last1,
       __first2, __last2, __result,
       __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator,
    typename _Compare>
   
    _OutputIterator
    __set_symmetric_difference(_InputIterator1 __first1,
          _InputIterator1 __last1,
          _InputIterator2 __first2,
          _InputIterator2 __last2,
          _OutputIterator __result,
          _Compare __comp)
    {
      while (__first1 != __last1 && __first2 != __last2)
 if (__comp(__first1, __first2))
   {
     *__result = *__first1;
     ++__first1;
     ++__result;
   }
 else if (__comp(__first2, __first1))
   {
     *__result = *__first2;
     ++__first2;
     ++__result;
   }
 else
   {
     ++__first1;
     ++__first2;
   }
      return std::copy(__first2, __last2,
         std::copy(__first1, __last1, __result));
    }
# 5509 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator>
   
    inline _OutputIterator
    set_symmetric_difference(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _InputIterator2 __last2,
        _OutputIterator __result)
    {

     
     
     

     

     


     


      ;
      ;
      ;
      ;

      return std::__set_symmetric_difference(__first1, __last1,
     __first2, __last2, __result,
     __gnu_cxx::__ops::__iter_less_iter());
    }
# 5561 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _InputIterator1, typename _InputIterator2,
    typename _OutputIterator, typename _Compare>
   
    inline _OutputIterator
    set_symmetric_difference(_InputIterator1 __first1, _InputIterator1 __last1,
        _InputIterator2 __first2, _InputIterator2 __last2,
        _OutputIterator __result,
        _Compare __comp)
    {

     
     
     

     

     


     


      ;
      ;
      ;
      ;

      return std::__set_symmetric_difference(__first1, __last1,
    __first2, __last2, __result,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _ForwardIterator, typename _Compare>
    constexpr
    _ForwardIterator
    __min_element(_ForwardIterator __first, _ForwardIterator __last,
    _Compare __comp)
    {
      if (__first == __last)
 return __first;
      _ForwardIterator __result = __first;
      while (++__first != __last)
 if (__comp(__first, __result))
   __result = __first;
      return __result;
    }
# 5615 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]] constexpr
    _ForwardIterator
    inline min_element(_ForwardIterator __first, _ForwardIterator __last)
    {

     
     

      ;
      ;

      return std::__min_element(__first, __last,
    __gnu_cxx::__ops::__iter_less_iter());
    }
# 5640 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Compare>
    [[__nodiscard__]] constexpr
    inline _ForwardIterator
    min_element(_ForwardIterator __first, _ForwardIterator __last,
  _Compare __comp)
    {

     
     


      ;
      ;

      return std::__min_element(__first, __last,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _ForwardIterator, typename _Compare>
    constexpr
    _ForwardIterator
    __max_element(_ForwardIterator __first, _ForwardIterator __last,
    _Compare __comp)
    {
      if (__first == __last) return __first;
      _ForwardIterator __result = __first;
      while (++__first != __last)
 if (__comp(__result, __first))
   __result = __first;
      return __result;
    }
# 5679 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator>
    [[__nodiscard__]] constexpr
    inline _ForwardIterator
    max_element(_ForwardIterator __first, _ForwardIterator __last)
    {

     
     

      ;
      ;

      return std::__max_element(__first, __last,
    __gnu_cxx::__ops::__iter_less_iter());
    }
# 5704 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3
  template<typename _ForwardIterator, typename _Compare>
    [[__nodiscard__]] constexpr
    inline _ForwardIterator
    max_element(_ForwardIterator __first, _ForwardIterator __last,
  _Compare __comp)
    {

     
     


      ;
      ;

      return std::__max_element(__first, __last,
    __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }



  template<typename _Tp>
    constexpr
    inline _Tp
    min(initializer_list<_Tp> __l)
    {
      ;
      return *std::__min_element(__l.begin(), __l.end(),
   __gnu_cxx::__ops::__iter_less_iter());
    }

  template<typename _Tp, typename _Compare>
    constexpr
    inline _Tp
    min(initializer_list<_Tp> __l, _Compare __comp)
    {
      ;
      return *std::__min_element(__l.begin(), __l.end(),
   __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }

  template<typename _Tp>
    constexpr
    inline _Tp
    max(initializer_list<_Tp> __l)
    {
      ;
      return *std::__max_element(__l.begin(), __l.end(),
   __gnu_cxx::__ops::__iter_less_iter());
    }

  template<typename _Tp, typename _Compare>
    constexpr
    inline _Tp
    max(initializer_list<_Tp> __l, _Compare __comp)
    {
      ;
      return *std::__max_element(__l.begin(), __l.end(),
   __gnu_cxx::__ops::__iter_comp_iter(__comp));
    }




  template<typename _InputIterator, typename _RandomAccessIterator,
           typename _Size, typename _UniformRandomBitGenerator>
    _RandomAccessIterator
    __sample(_InputIterator __first, _InputIterator __last, input_iterator_tag,
      _RandomAccessIterator __out, random_access_iterator_tag,
      _Size __n, _UniformRandomBitGenerator&& __g)
    {
      using __distrib_type = uniform_int_distribution<_Size>;
      using __param_type = typename __distrib_type::param_type;
      __distrib_type __d{};
      _Size __sample_sz = 0;
      while (__first != __last && __sample_sz != __n)
 {
   __out[__sample_sz++] = *__first;
   ++__first;
 }
      for (auto __pop_sz = __sample_sz; __first != __last;
   ++__first, (void) ++__pop_sz)
 {
   const auto __k = __d(__g, __param_type{0, __pop_sz});
   if (__k < __n)
     __out[__k] = *__first;
 }
      return __out + __sample_sz;
    }


  template<typename _ForwardIterator, typename _OutputIterator, typename _Cat,
           typename _Size, typename _UniformRandomBitGenerator>
    _OutputIterator
    __sample(_ForwardIterator __first, _ForwardIterator __last,
      forward_iterator_tag,
      _OutputIterator __out, _Cat,
      _Size __n, _UniformRandomBitGenerator&& __g)
    {
      using __distrib_type = uniform_int_distribution<_Size>;
      using __param_type = typename __distrib_type::param_type;
      using _USize = make_unsigned_t<_Size>;
      using _Gen = remove_reference_t<_UniformRandomBitGenerator>;
      using __uc_type = common_type_t<typename _Gen::result_type, _USize>;

      if (__first == __last)
 return __out;

      __distrib_type __d{};
      _Size __unsampled_sz = std::distance(__first, __last);
      __n = std::min(__n, __unsampled_sz);




      const __uc_type __urngrange = __g.max() - __g.min();
      if (__urngrange / __uc_type(__unsampled_sz) >= __uc_type(__unsampled_sz))


        {
   while (__n != 0 && __unsampled_sz >= 2)
     {
       const pair<_Size, _Size> __p =
  __gen_two_uniform_ints(__unsampled_sz, __unsampled_sz - 1, __g);

       --__unsampled_sz;
       if (__p.first < __n)
  {
    *__out++ = *__first;
    --__n;
  }

       ++__first;

       if (__n == 0) break;

       --__unsampled_sz;
       if (__p.second < __n)
  {
    *__out++ = *__first;
    --__n;
  }

       ++__first;
     }
        }



      for (; __n != 0; ++__first)
 if (__d(__g, __param_type{0, --__unsampled_sz}) < __n)
   {
     *__out++ = *__first;
     --__n;
   }
      return __out;
    }
# 5892 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algo.h" 3


}

#pragma GCC diagnostic pop
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/algorithm" 2 3
# 80 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/algorithm" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 81 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/algorithm" 2 3
# 8 "./opt/pr102970.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cassert" 3
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
# 9 "./opt/pr102970.C" 2


# 10 "./opt/pr102970.C"
struct box {
   box(int xmin, int xmax, int ymin, int ymax) noexcept
      : m_xmin(xmin),
        m_xmax(xmax),
        m_ymin(ymin),
        m_ymax(ymax) {
   }

   box(box const & o) noexcept
      : m_xmin(o.m_xmin),
        m_xmax(o.m_xmax),
        m_ymin(o.m_ymin),
        m_ymax(o.m_ymax) { }

   int m_xmin;
   int m_xmax;
   int m_ymin;
   int m_ymax;
};


int main() {
    std::vector<box> vRects{
        { 900, 11, 22, 33 },
        { 901, 11, 22, 33 },
        { 902, 11, 22, 33 },
        { 903, 11, 22, 33 },
        { 704, 11, 22, 33 },
        { 705, 11, 22, 33 },
        { 706, 11, 22, 33 },
        { 707, 11, 22, 33 },
        { 808, 11, 22, 33 },
        { 809, 11, 22, 33 },
        { 810, 11, 22, 33 },
        { 811, 11, 22, 33 },
        { 812, 11, 22, 33 },
        { 813, 11, 22, 33 },
        { 814, 11, 22, 33 },
        { 815, 11, 22, 33 },
        { 816, 11, 22, 33 },
        { 817, 11, 22, 33 },
        { 818, 11, 22, 33 },
    };

    std::stable_sort(vRects.begin(), vRects.end(),
       [](auto const &r1, auto const &r2) -> bool
      {
        if (r2.m_xmax==0||r2.m_ymin==0||r2.m_ymax==0){__builtin_abort();}
        return r1.m_xmin < r2.m_xmin;
      });
  return 0;
}
