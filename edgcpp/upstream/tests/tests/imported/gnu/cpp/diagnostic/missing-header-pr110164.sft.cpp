//type: fn
//options:  --c++11
# 0 "./diagnostic/missing-header-pr110164.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./diagnostic/missing-header-pr110164.C"



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 3
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
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 1 3
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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
# 974 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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
# 1769 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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
# 1815 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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
# 2149 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct remove_extent
    { using type = __remove_extent(_Tp); };
# 2168 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct remove_all_extents
    { using type = __remove_all_extents(_Tp); };
# 2199 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct remove_pointer
    { using type = __remove_pointer(_Tp); };
# 2219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
  template<typename _Tp>
    struct add_pointer
    { using type = __add_pointer(_Tp); };
# 2258 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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
# 142 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
  template<size_t... _Indexes> struct _Index_tuple { };


  template<size_t _Num>
    struct _Build_index_tuple
    {
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/utility.h" 3
      using __type = _Index_tuple<__integer_pack(_Num)...>;

    };
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
    inline
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
    inline
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
    inline void
    __advance(_InputIterator& __i, _Distance __n, input_iterator_tag)
    {

     
      do { if (__builtin_expect(!bool(__n >= 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h", 163, __PRETTY_FUNCTION__, "__n >= 0"); } while (false);
      while (__n--)
 ++__i;
    }

  template<typename _BidirectionalIterator, typename _Distance>
    inline void
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
    inline void
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
     
      __normal_iterator&
      operator++() noexcept
      {
 ++_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator++(int) noexcept
      { return __normal_iterator(_M_current++); }



      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator--() noexcept
      {
 --_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator--(int) noexcept
      { return __normal_iterator(_M_current--); }



      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      reference
      operator[](difference_type __n) const noexcept
      { return _M_current[__n]; }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator+=(difference_type __n) noexcept
      { _M_current += __n; return *this; }

      [[__nodiscard__]] __attribute__((__always_inline__))
      constexpr
      __normal_iterator
      operator+(difference_type __n) const noexcept
      { return __normal_iterator(_M_current + __n); }

      __attribute__((__always_inline__))
     
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
     
      bool
      operator()(_Iterator1 __it1, _Iterator2 __it2) const
      { return *__it1 < *__it2; }
  };

 
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

      explicit
      _Iter_comp_iter(_Compare __comp)
 : _M_comp(std::move(__comp))
      { }

      template<typename _Iterator1, typename _Iterator2>
       
        bool
        operator()(_Iterator1 __it1, _Iterator2 __it2)
        { return bool(_M_comp(*__it1, *__it2)); }
    };

  template<typename _Compare>
   
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
# 83 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{






  template<typename _Tp, typename _Up>
   
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
    [[__nodiscard__]]
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b)
    {

     

      if (__b < __a)
 return __b;
      return __a;
    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp>
    [[__nodiscard__]]
    inline const _Tp&
    max(const _Tp& __a, const _Tp& __b)
    {

     

      if (__a < __b)
 return __b;
      return __a;
    }
# 279 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]]
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {

      if (__comp(__b, __a))
 return __b;
      return __a;
    }
# 301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
    [[__nodiscard__]]
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



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"

      return (sizeof(+__n) * 8 - 1)
        - (sizeof(+__n) == sizeof(long long)
      ? __builtin_clzll(+__n)
      : (sizeof(+__n) == sizeof(long)
    ? __builtin_clzl(+__n)
    : __builtin_clz(+__n)));
#pragma GCC diagnostic pop

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
# 2096 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3



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
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 2 3
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
# 155 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
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
 __builtin_operator_delete((__p));
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
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 2 3
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
# 179 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct plus : public binary_function<_Tp, _Tp, _Tp>
    {

     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x + __y; }
    };


  template<typename _Tp>
    struct minus : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x - __y; }
    };


  template<typename _Tp>
    struct multiplies : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x * __y; }
    };


  template<typename _Tp>
    struct divides : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x / __y; }
    };


  template<typename _Tp>
    struct modulus : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x % __y; }
    };


  template<typename _Tp>
    struct negate : public unary_function<_Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x) const
      { return -__x; }
    };
#pragma GCC diagnostic pop
# 365 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct equal_to : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x == __y; }
    };


  template<typename _Tp>
    struct not_equal_to : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x != __y; }
    };


  template<typename _Tp>
    struct greater : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x > __y; }
    };


  template<typename _Tp>
    struct less : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x < __y; }
    };


  template<typename _Tp>
    struct greater_equal : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x >= __y; }
    };


  template<typename _Tp>
    struct less_equal : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x <= __y; }
    };


  template<typename _Tp>
    struct greater<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {




 return (long unsigned int)__x > (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {




 return (long unsigned int)__x < (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct greater_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {




 return (long unsigned int)__x >= (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const noexcept
      {




 return (long unsigned int)__x <= (long unsigned int)__y;
      }
    };
#pragma GCC diagnostic pop
# 788 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _Tp>
    struct logical_and : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x && __y; }
    };


  template<typename _Tp>
    struct logical_or : public binary_function<_Tp, _Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x || __y; }
    };


  template<typename _Tp>
    struct logical_not : public unary_function<_Tp, bool>
    {
     
      bool
      operator()(const _Tp& __x) const
      { return !__x; }
    };
#pragma GCC diagnostic pop
# 884 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"



  template<typename _Tp>
    struct bit_and : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x & __y; }
    };

  template<typename _Tp>
    struct bit_or : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x | __y; }
    };

  template<typename _Tp>
    struct bit_xor : public binary_function<_Tp, _Tp, _Tp>
    {
     
      _Tp
      operator()(const _Tp& __x, const _Tp& __y) const
      { return __x ^ __y; }
    };

  template<typename _Tp>
    struct bit_not : public unary_function<_Tp, _Tp>
    {
   
      _Tp
      operator()(const _Tp& __x) const
      { return ~__x; }
    };
#pragma GCC diagnostic pop
# 984 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
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
     
      explicit
      unary_negate(const _Predicate& __x) : _M_pred(__x) { }

     
      bool
      operator()(const typename _Predicate::argument_type& __x) const
      { return !_M_pred(__x); }
    };


  template<typename _Predicate>
   
   
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
     
      explicit
      binary_negate(const _Predicate& __x) : _M_pred(__x) { }

     
      bool
      operator()(const typename _Predicate::first_argument_type& __x,
   const typename _Predicate::second_argument_type& __y) const
      { return !_M_pred(__x, __y); }
    };


  template<typename _Predicate>
   
   
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
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 1 3
# 33 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
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
    inline void
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
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 2 3
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
# 524 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
      template<typename _Tp, typename... _Args>
 static constexpr
 __enable_if_t<__has_construct<_Alloc, _Tp, _Args...>, bool>
 _S_nothrow_construct(_Alloc* __a = nullptr, _Tp* __p = nullptr)
 { return noexcept(__a->construct(__p, std::declval<_Args>()...)); }

      template<typename _Tp, typename... _Args>
 static constexpr
 __enable_if_t<!__has_construct<_Alloc, _Tp, _Args...>, bool>
 _S_nothrow_construct(_Alloc* = nullptr, _Tp* __p = nullptr)
 { return __is_nothrow_new_constructible<_Tp, _Args...>; }

      template<typename _Tp>
 static constexpr
 __enable_if_t<__has_destroy<_Alloc, _Tp>, bool>
 _S_nothrow_destroy(_Alloc* __a = nullptr, _Tp* __p = nullptr)
 { return noexcept(__a->destroy(__p)); }

      template<typename _Tp>
 static constexpr
 __enable_if_t<!__has_destroy<_Alloc, _Tp>, bool>
 _S_nothrow_destroy(_Alloc* = nullptr, _Tp* __p = nullptr)
 { return is_nothrow_destructible<_Tp>::value; }

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
    inline void
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
    inline void
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
    inline void
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
      static
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
      static
      std::__enable_if_t<__is_custom_pointer<_Ptr>::value>
      destroy(_Alloc& __a, _Ptr __p)
      noexcept(noexcept(_Base_type::destroy(__a, std::__to_address(__p))))
      { _Base_type::destroy(__a, std::__to_address(__p)); }

    [[__gnu__::__always_inline__]]
    static constexpr _Alloc _S_select_on_copy(const _Alloc& __a)
    { return _Base_type::select_on_container_copy_construction(__a); }

    [[__gnu__::__always_inline__]]
    static void _S_on_swap(_Alloc& __a, _Alloc& __b)
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
# 71 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 3
namespace __gnu_cxx
{




  template<typename _Tp>
    struct __aligned_membuf
    {
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 3
      alignas(_Tp)

 unsigned char _M_storage[sizeof(_Tp)];

      __aligned_membuf() = default;


      __aligned_membuf(std::nullptr_t) { }

      void*
      _M_addr() noexcept
      { return static_cast<void*>(&_M_storage); }

      const void*
      _M_addr() const noexcept
      { return static_cast<const void*>(&_M_storage); }

      _Tp*
      _M_ptr() noexcept
      { return static_cast<_Tp*>(_M_addr()); }

      const _Tp*
      _M_ptr() const noexcept
      { return static_cast<const _Tp*>(_M_addr()); }
    };
# 95 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/aligned_buffer.h" 3
  template<typename _Tp>
    struct __aligned_buffer
    {

      alignas(__alignof__(_Tp)) unsigned char _M_storage[sizeof(_Tp)];

      __aligned_buffer() = default;


      __aligned_buffer(std::nullptr_t) { }

      void*
      _M_addr() noexcept
      {
        return static_cast<void*>(&_M_storage);
      }

      const void*
      _M_addr() const noexcept
      {
        return static_cast<const void*>(&_M_storage);
      }

      _Tp*
      _M_ptr() noexcept
      { return static_cast<_Tp*>(_M_addr()); }

      const _Tp*
      _M_ptr() const noexcept
      { return static_cast<const _Tp*>(_M_addr()); }
    };


}
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 2 3
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 105 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
  enum _Rb_tree_color { _S_red = false, _S_black = true };

  struct _Rb_tree_node_base
  {
    typedef _Rb_tree_node_base* _Base_ptr;

    _Rb_tree_color _M_color;
    _Base_ptr _M_parent;
    _Base_ptr _M_left;
    _Base_ptr _M_right;

    static _Base_ptr
    _S_minimum(_Base_ptr __x) noexcept
    {
      while (__x->_M_left != 0) __x = __x->_M_left;
      return __x;
    }

    static _Base_ptr
    _S_maximum(_Base_ptr __x) noexcept
    {
      while (__x->_M_right != 0) __x = __x->_M_right;
      return __x;
    }




    _Base_ptr
    _M_base_ptr() const noexcept
    { return const_cast<_Rb_tree_node_base*>(this); }
  };


  template<typename _Key_compare>
    struct _Rb_tree_key_compare
    {
      _Key_compare _M_key_compare;

      _Rb_tree_key_compare()
      noexcept(is_nothrow_default_constructible<_Key_compare>::value)

      : _M_key_compare()
      { }

      _Rb_tree_key_compare(const _Key_compare& __comp)
      : _M_key_compare(__comp)
      { }



      _Rb_tree_key_compare(const _Rb_tree_key_compare&) = default;

      _Rb_tree_key_compare(_Rb_tree_key_compare&& __x)
 noexcept(is_nothrow_copy_constructible<_Key_compare>::value)
      : _M_key_compare(__x._M_key_compare)
      { }

    };


  struct _Rb_tree_header
  {
    _Rb_tree_node_base _M_header;
    size_t _M_node_count;

    _Rb_tree_header() noexcept
    {
      _M_header._M_color = _S_red;
      _M_reset();
    }


    _Rb_tree_header(_Rb_tree_header&& __x) noexcept
    {
      if (__x._M_header._M_parent != nullptr)
 _M_move_data(__x);
      else
 {
   _M_header._M_color = _S_red;
   _M_reset();
 }
    }


    void
    _M_move_data(_Rb_tree_header& __from)
    {
      _M_header._M_color = __from._M_header._M_color;
      _M_header._M_parent = __from._M_header._M_parent;
      _M_header._M_left = __from._M_header._M_left;
      _M_header._M_right = __from._M_header._M_right;
      _M_header._M_parent->_M_parent = &_M_header;
      _M_node_count = __from._M_node_count;

      __from._M_reset();
    }

    void
    _M_reset()
    {
      _M_header._M_parent = 0;
      _M_header._M_left = &_M_header;
      _M_header._M_right = &_M_header;
      _M_node_count = 0;
    }
  };

  template<typename _Val>
    struct _Rb_tree_node : public _Rb_tree_node_base
    {
# 227 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      __gnu_cxx::__aligned_membuf<_Val> _M_storage;

      _Val*
      _M_valptr()
      { return _M_storage._M_ptr(); }

      const _Val*
      _M_valptr() const
      { return _M_storage._M_ptr(); }


      _Rb_tree_node*
      _M_node_ptr() noexcept
      { return this; }
    };


namespace __rb_tree
{
  template<typename _VoidPtr>
    struct _Node_base
    {
      using _Base_ptr = __ptr_rebind<_VoidPtr, _Node_base>;

      _Rb_tree_color _M_color;
      _Base_ptr _M_parent;
      _Base_ptr _M_left;
      _Base_ptr _M_right;

      static _Base_ptr
      _S_minimum(_Base_ptr __x) noexcept
      {
 while (__x->_M_left) __x = __x->_M_left;
 return __x;
      }

      static _Base_ptr
      _S_maximum(_Base_ptr __x) noexcept
      {
 while (__x->_M_right) __x = __x->_M_right;
 return __x;
      }




      _Base_ptr
      _M_base_ptr() const noexcept
      {
 return pointer_traits<_Base_ptr>::pointer_to
   (*const_cast<_Node_base*>(this));
      }
    };


  template<typename _NodeBase>
    struct _Header
    {
    private:
      using _Base_ptr = typename _NodeBase::_Base_ptr;

    public:
      _NodeBase _M_header;
      size_t _M_node_count;

      _Header() noexcept
      {
 _M_header._M_color = _S_red;
 _M_reset();
      }

      _Header(_Header&& __x) noexcept
      {
 if (__x._M_header._M_parent)
   _M_move_data(__x);
 else
   {
     _M_header._M_color = _S_red;
     _M_reset();
   }
      }

      void
      _M_move_data(_Header& __from)
      {
 _M_header._M_color = __from._M_header._M_color;
 _M_header._M_parent = __from._M_header._M_parent;
 _M_header._M_left = __from._M_header._M_left;
 _M_header._M_right = __from._M_header._M_right;
 _M_header._M_parent->_M_parent = _M_header._M_base_ptr();
 _M_node_count = __from._M_node_count;

 __from._M_reset();
      }

      void
      _M_reset()
      {
 _M_header._M_parent = nullptr;
 _M_header._M_left = _M_header._M_right = _M_header._M_base_ptr();
 _M_node_count = 0;
      }
    };

  template<typename _ValPtr>
    struct _Node : public __rb_tree::_Node_base<__ptr_rebind<_ValPtr, void>>
    {
      using value_type = typename pointer_traits<_ValPtr>::element_type;
      using _Node_ptr = __ptr_rebind<_ValPtr, _Node>;

      _Node() noexcept { }
      ~_Node() { }
      _Node(_Node&&) = delete;

      union _Uninit_storage
      {
 _Uninit_storage() noexcept { }
 ~_Uninit_storage() { }

 value_type _M_data;
      };
      _Uninit_storage _M_u;

      value_type*
      _M_valptr()
      { return std::addressof(_M_u._M_data); }

      value_type const*
      _M_valptr() const
      { return std::addressof(_M_u._M_data); }

      _Node_ptr
      _M_node_ptr() noexcept
      { return pointer_traits<_Node_ptr>::pointer_to(*this); }
    };
}


  __attribute__ ((__pure__)) _Rb_tree_node_base*
  _Rb_tree_increment(_Rb_tree_node_base* __x) throw ();

  __attribute__ ((__pure__)) _Rb_tree_node_base*
  _Rb_tree_decrement(_Rb_tree_node_base* __x) throw ();

  template<typename _Tp>
    struct _Rb_tree_iterator
    {
      typedef _Tp value_type;
      typedef _Tp& reference;
      typedef _Tp* pointer;

      typedef bidirectional_iterator_tag iterator_category;
      typedef ptrdiff_t difference_type;

      typedef _Rb_tree_node_base::_Base_ptr _Base_ptr;
      typedef _Rb_tree_node<_Tp>* _Node_ptr;

      _Rb_tree_iterator() noexcept
      : _M_node() { }

      explicit
      _Rb_tree_iterator(_Base_ptr __x) noexcept
      : _M_node(__x) { }

      reference
      operator*() const noexcept
      { return *static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      pointer
      operator->() const noexcept
      { return static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      _Rb_tree_iterator&
      operator++() noexcept
      {
 _M_node = _Rb_tree_increment(_M_node);
 return *this;
      }

      _Rb_tree_iterator
      operator++(int) noexcept
      {
 _Rb_tree_iterator __tmp = *this;
 _M_node = _Rb_tree_increment(_M_node);
 return __tmp;
      }

      _Rb_tree_iterator&
      operator--() noexcept
      {
 _M_node = _Rb_tree_decrement(_M_node);
 return *this;
      }

      _Rb_tree_iterator
      operator--(int) noexcept
      {
 _Rb_tree_iterator __tmp = *this;
 _M_node = _Rb_tree_decrement(_M_node);
 return __tmp;
      }

      friend bool
      operator==(const _Rb_tree_iterator& __x,
   const _Rb_tree_iterator& __y) noexcept
      { return __x._M_node == __y._M_node; }


      friend bool
      operator!=(const _Rb_tree_iterator& __x,
   const _Rb_tree_iterator& __y) noexcept
      { return __x._M_node != __y._M_node; }


      _Base_ptr _M_node;
    };

  template<typename _Tp>
    struct _Rb_tree_const_iterator
    {
      typedef _Tp value_type;
      typedef const _Tp& reference;
      typedef const _Tp* pointer;

      typedef _Rb_tree_iterator<_Tp> iterator;

      typedef bidirectional_iterator_tag iterator_category;
      typedef ptrdiff_t difference_type;

      typedef _Rb_tree_node_base::_Base_ptr _Base_ptr;
      typedef const _Rb_tree_node<_Tp>* _Node_ptr;

      _Rb_tree_const_iterator() noexcept
      : _M_node() { }

      explicit
      _Rb_tree_const_iterator(_Base_ptr __x) noexcept
      : _M_node(__x) { }

      _Rb_tree_const_iterator(const iterator& __it) noexcept
      : _M_node(__it._M_node) { }

      reference
      operator*() const noexcept
      { return *static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      pointer
      operator->() const noexcept
      { return static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      _Rb_tree_const_iterator&
      operator++() noexcept
      {
 _M_node = _Rb_tree_increment(_M_node);
 return *this;
      }

      _Rb_tree_const_iterator
      operator++(int) noexcept
      {
 _Rb_tree_const_iterator __tmp = *this;
 _M_node = _Rb_tree_increment(_M_node);
 return __tmp;
      }

      _Rb_tree_const_iterator&
      operator--() noexcept
      {
 _M_node = _Rb_tree_decrement(_M_node);
 return *this;
      }

      _Rb_tree_const_iterator
      operator--(int) noexcept
      {
 _Rb_tree_const_iterator __tmp = *this;
 _M_node = _Rb_tree_decrement(_M_node);
 return __tmp;
      }

      friend bool
      operator==(const _Rb_tree_const_iterator& __x,
   const _Rb_tree_const_iterator& __y) noexcept
      { return __x._M_node == __y._M_node; }


      friend bool
      operator!=(const _Rb_tree_const_iterator& __x,
   const _Rb_tree_const_iterator& __y) noexcept
      { return __x._M_node != __y._M_node; }


      _Base_ptr _M_node;
    };

  __attribute__((__nonnull__))
  void
  _Rb_tree_insert_and_rebalance(const bool __insert_left,
    _Rb_tree_node_base* __x,
    _Rb_tree_node_base* __p,
    _Rb_tree_node_base& __header) throw ();

  __attribute__((__nonnull__,__returns_nonnull__))
  _Rb_tree_node_base*
  _Rb_tree_rebalance_for_erase(_Rb_tree_node_base* const __z,
          _Rb_tree_node_base& __header) throw ();

namespace __rb_tree
{

  template<bool _Const, typename _ValPtr>
    struct _Iterator
    {
      template<typename _Tp>
 using __maybe_const = __conditional_t<_Const, const _Tp, _Tp>;

      using __ptr_traits = pointer_traits<_ValPtr>;
      using value_type = typename __ptr_traits::element_type;
      using reference = __maybe_const<value_type>&;
      using pointer = __maybe_const<value_type>*;

      using iterator_category = bidirectional_iterator_tag;
      using difference_type = ptrdiff_t;

      using _Node = __rb_tree::_Node<_ValPtr>;
      using _Node_base = __rb_tree::_Node_base<__ptr_rebind<_ValPtr, void>>;
      using _Base_ptr = typename _Node_base::_Base_ptr;

      _Iterator() noexcept
      : _M_node() { }

      constexpr explicit
      _Iterator(_Base_ptr __x) noexcept
      : _M_node(__x) { }

      _Iterator(const _Iterator&) = default;
      _Iterator& operator=(const _Iterator&) = default;





      template<bool _OtherConst,
        typename = __enable_if_t<_Const && !_OtherConst>>
 constexpr
 _Iterator(const _Iterator<_OtherConst, _ValPtr>& __it)

 : _M_node(__it._M_node) { }

      [[__nodiscard__]]
      reference
      operator*() const noexcept
      { return *static_cast<_Node&>(*_M_node)._M_valptr(); }

      [[__nodiscard__]]
      pointer
      operator->() const noexcept
      { return static_cast<_Node&>(*_M_node)._M_valptr(); }

      _Iterator&
      operator++() noexcept
      {
 if (_M_node->_M_right)
   {
     _M_node = _M_node->_M_right;
     while (_M_node->_M_left)
       _M_node = _M_node->_M_left;
   }
 else
   {
     _Base_ptr __y = _M_node->_M_parent;
     while (_M_node == __y->_M_right)
       {
  _M_node = __y;
  __y = __y->_M_parent;
       }
     if (_M_node->_M_right != __y)
       _M_node = __y;
   }

 return *this;
      }

      _Iterator
      operator++(int) noexcept
      {
 _Iterator __tmp(this->_M_node);
 ++*this;
 return __tmp;
      }

      _Iterator&
      operator--() noexcept
      {
 if (_M_node->_M_color == _S_red
     && _M_node->_M_parent->_M_parent == _M_node)
   _M_node = _M_node->_M_right;
 else if (_M_node->_M_left)
   {
     _Base_ptr __y = _M_node->_M_left;
     while (__y->_M_right)
       __y = __y->_M_right;
     _M_node = __y;
   }
 else
   {
     _Base_ptr __y = _M_node->_M_parent;
     while (_M_node == __y->_M_left)
       {
  _M_node = __y;
  __y = __y->_M_parent;
       }
     _M_node = __y;
   }
 return *this;
      }

      _Iterator
      operator--(int) noexcept
      {
 _Iterator __tmp(this->_M_node);
 --*this;
 return __tmp;
      }

      [[__nodiscard__]]
      friend bool
      operator==(const _Iterator& __x, const _Iterator& __y) noexcept
      { return __x._M_node == __y._M_node; }


      [[__nodiscard__]]
      friend bool
      operator!=(const _Iterator& __x, const _Iterator& __y) noexcept
      { return __x._M_node != __y._M_node; }


      _Base_ptr _M_node;
    };



  template<typename _Val, typename _Ptr>
    struct _Node_traits;





  template<typename _Val>
    struct _Node_traits<_Val, _Val*>
    {
      typedef _Rb_tree_node<_Val> _Node;
      typedef _Node* _Node_ptr;
      typedef _Rb_tree_node_base _Node_base;
      typedef _Node_base* _Base_ptr;
      typedef _Rb_tree_header _Header_t;
      typedef _Rb_tree_iterator<_Val> _Iterator;
      typedef _Rb_tree_const_iterator<_Val> _Const_iterator;

      __attribute__((__nonnull__))
      static void
      _S_insert_and_rebalance(const bool __insert_left,
         _Node_base* __x, _Node_base* __p,
         _Node_base& __header) noexcept
      {
 return _Rb_tree_insert_and_rebalance(__insert_left, __x, __p, __header);
      }

      __attribute__((__nonnull__,__returns_nonnull__))
      static _Node_base*
      _S_rebalance_for_erase(_Node_base* const __z,
        _Node_base& __header) noexcept
      { return _Rb_tree_rebalance_for_erase(__z, __header); }
    };
# 712 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
  template<typename _Val, typename _ValPtr>
    struct _Node_traits
    {
      using _Node = __rb_tree::_Node<_ValPtr>;
      using _Node_ptr = __ptr_rebind<_ValPtr, _Node>;
      using _Node_base = __rb_tree::_Node_base<__ptr_rebind<_ValPtr, void>>;
      using _Base_ptr = __ptr_rebind<_ValPtr, _Node_base>;
      using _Header_t = __rb_tree::_Header<_Node_base>;
      using _Iterator = __rb_tree::_Iterator<false, _ValPtr>;
      using _Const_iterator = __rb_tree::_Iterator<true, _ValPtr>;

      static void
      _Rotate_left(_Base_ptr __x, _Base_ptr& __root)
      {
 const _Base_ptr __y = __x->_M_right;

 __x->_M_right = __y->_M_left;
 if (__y->_M_left)
   __y->_M_left->_M_parent = __x;
 __y->_M_parent = __x->_M_parent;

 if (__x == __root)
   __root = __y;
 else if (__x == __x->_M_parent->_M_left)
   __x->_M_parent->_M_left = __y;
 else
   __x->_M_parent->_M_right = __y;
 __y->_M_left = __x;
 __x->_M_parent = __y;
      }

      static void
      _Rotate_right(_Base_ptr __x, _Base_ptr& __root)
      {
 const _Base_ptr __y = __x->_M_left;

 __x->_M_left = __y->_M_right;
 if (__y->_M_right)
   __y->_M_right->_M_parent = __x;
 __y->_M_parent = __x->_M_parent;

 if (__x == __root)
   __root = __y;
 else if (__x == __x->_M_parent->_M_right)
   __x->_M_parent->_M_right = __y;
 else
   __x->_M_parent->_M_left = __y;
 __y->_M_right = __x;
 __x->_M_parent = __y;
      }

      static void
      _S_insert_and_rebalance(const bool __insert_left,
         _Base_ptr __x, _Base_ptr __p,
         _Node_base& __header)
      {
 _Base_ptr& __root = __header._M_parent;


 __x->_M_parent = __p;
 __x->_M_left = __x->_M_right = nullptr;
 __x->_M_color = _S_red;





 if (__insert_left)
   {
     __p->_M_left = __x;

     if (std::__to_address(__p) == std::addressof(__header))
       {
  __header._M_parent = __x;
  __header._M_right = __x;
       }
     else if (__p == __header._M_left)
       __header._M_left = __x;
   }
 else
   {
     __p->_M_right = __x;

     if (__p == __header._M_right)
       __header._M_right = __x;
   }

 while (__x != __root
        && __x->_M_parent->_M_color == _S_red)
   {
     const _Base_ptr __xpp = __x->_M_parent->_M_parent;

     if (__x->_M_parent == __xpp->_M_left)
       {
  const _Base_ptr __y = __xpp->_M_right;
  if (__y && __y->_M_color == _S_red)
    {
      __x->_M_parent->_M_color = _S_black;
      __y->_M_color = _S_black;
      __xpp->_M_color = _S_red;
      __x = __xpp;
    }
  else
    {
      if (__x == __x->_M_parent->_M_right)
        {
   __x = __x->_M_parent;
   _Rotate_left(__x, __root);
        }
      __x->_M_parent->_M_color = _S_black;
      __xpp->_M_color = _S_red;
      _Rotate_right(__xpp, __root);
    }
       }
     else
       {
  const _Base_ptr __y = __xpp->_M_left;
  if (__y && __y->_M_color == _S_red)
    {
      __x->_M_parent->_M_color = _S_black;
      __y->_M_color = _S_black;
      __xpp->_M_color = _S_red;
      __x = __xpp;
    }
  else
    {
      if (__x == __x->_M_parent->_M_left)
        {
   __x = __x->_M_parent;
   _Rotate_right(__x, __root);
        }
      __x->_M_parent->_M_color = _S_black;
      __xpp->_M_color = _S_red;
      _Rotate_left(__xpp, __root);
    }
       }
   }
 __root->_M_color = _S_black;
      }

      static _Base_ptr
      _S_rebalance_for_erase(_Base_ptr __z, _Node_base& __header)
      {
 _Base_ptr& __root = __header._M_parent;
 _Base_ptr& __leftmost = __header._M_left;
 _Base_ptr& __rightmost = __header._M_right;
 _Base_ptr __y = __z;
 _Base_ptr __x{};
 _Base_ptr __x_parent{};

 if (!__y->_M_left)
   __x = __y->_M_right;
 else
   if (!__y->_M_right)
     __x = __y->_M_left;
   else
     {

       __y = __y->_M_right;
       while (__y->_M_left)
  __y = __y->_M_left;
       __x = __y->_M_right;
     }
 if (__y != __z)
   {

     __z->_M_left->_M_parent = __y;
     __y->_M_left = __z->_M_left;
     if (__y != __z->_M_right)
       {
  __x_parent = __y->_M_parent;
  if (__x)
    __x->_M_parent = __y->_M_parent;
  __y->_M_parent->_M_left = __x;
  __y->_M_right = __z->_M_right;
  __z->_M_right->_M_parent = __y;
       }
     else
       __x_parent = __y;
     if (__root == __z)
       __root = __y;
     else if (__z->_M_parent->_M_left == __z)
       __z->_M_parent->_M_left = __y;
     else
       __z->_M_parent->_M_right = __y;
     __y->_M_parent = __z->_M_parent;
     std::swap(__y->_M_color, __z->_M_color);
     __y = __z;

   }
 else
   {
     __x_parent = __y->_M_parent;
     if (__x)
       __x->_M_parent = __y->_M_parent;
     if (__root == __z)
       __root = __x;
     else
       if (__z->_M_parent->_M_left == __z)
  __z->_M_parent->_M_left = __x;
       else
  __z->_M_parent->_M_right = __x;
     if (__leftmost == __z)
       {
  if (!__z->_M_right)
    __leftmost = __z->_M_parent;

  else
    __leftmost = _Node_base::_S_minimum(__x);
       }
     if (__rightmost == __z)
       {
  if (__z->_M_left == 0)
    __rightmost = __z->_M_parent;

  else
    __rightmost = _Node_base::_S_maximum(__x);
       }
   }
 if (__y->_M_color != _S_red)
   {
     while (__x != __root && (__x == 0 || __x->_M_color == _S_black))
       if (__x == __x_parent->_M_left)
  {
    _Base_ptr __w = __x_parent->_M_right;
    if (__w->_M_color == _S_red)
      {
        __w->_M_color = _S_black;
        __x_parent->_M_color = _S_red;
        _Rotate_left(__x_parent, __root);
        __w = __x_parent->_M_right;
      }
    if ((!__w->_M_left || __w->_M_left->_M_color == _S_black) &&
        (!__w->_M_right || __w->_M_right->_M_color == _S_black))
      {
        __w->_M_color = _S_red;
        __x = __x_parent;
        __x_parent = __x_parent->_M_parent;
      }
    else
      {
        if (!__w->_M_right || __w->_M_right->_M_color == _S_black)
   {
     __w->_M_left->_M_color = _S_black;
     __w->_M_color = _S_red;
     _Rotate_right(__w, __root);
     __w = __x_parent->_M_right;
   }
        __w->_M_color = __x_parent->_M_color;
        __x_parent->_M_color = _S_black;
        if (__w->_M_right)
   __w->_M_right->_M_color = _S_black;
        _Rotate_left(__x_parent, __root);
        break;
      }
  }
       else
  {

    _Base_ptr __w = __x_parent->_M_left;
    if (__w->_M_color == _S_red)
      {
        __w->_M_color = _S_black;
        __x_parent->_M_color = _S_red;
        _Rotate_right(__x_parent, __root);
        __w = __x_parent->_M_left;
      }
    if ((!__w->_M_right || __w->_M_right->_M_color == _S_black) &&
        (!__w->_M_left || __w->_M_left->_M_color == _S_black))
      {
        __w->_M_color = _S_red;
        __x = __x_parent;
        __x_parent = __x_parent->_M_parent;
      }
    else
      {
        if (!__w->_M_left || __w->_M_left->_M_color == _S_black)
   {
     __w->_M_right->_M_color = _S_black;
     __w->_M_color = _S_red;
     _Rotate_left(__w, __root);
     __w = __x_parent->_M_left;
   }
        __w->_M_color = __x_parent->_M_color;
        __x_parent->_M_color = _S_black;
        if (__w->_M_left)
   __w->_M_left->_M_color = _S_black;
        _Rotate_right(__x_parent, __root);
        break;
      }
  }
     if (__x)
       __x->_M_color = _S_black;
   }

 return __y;
      }
    };

}






  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc = allocator<_Val> >
    class _Rb_tree
    {
      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
 rebind<_Val>::other _Val_alloc_type;

      typedef __gnu_cxx::__alloc_traits<_Val_alloc_type> _Val_alloc_traits;
      typedef typename _Val_alloc_traits::pointer _ValPtr;
      typedef __rb_tree::_Node_traits<_Val, _ValPtr> _Node_traits;

      typedef typename _Node_traits::_Node_base _Node_base;
      typedef typename _Node_traits::_Node _Node;

      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
 rebind<_Node>::other _Node_allocator;

      typedef __gnu_cxx::__alloc_traits<_Node_allocator> _Node_alloc_traits;

    protected:
      typedef typename _Node_traits::_Base_ptr _Base_ptr;
      typedef typename _Node_traits::_Node_ptr _Node_ptr;

    private:


      struct _Reuse_or_alloc_node
      {
 _Reuse_or_alloc_node(_Rb_tree& __t)
 : _M_root(__t._M_root()), _M_nodes(__t._M_rightmost()), _M_t(__t)
 {
   if (_M_root)
     {
       _M_root->_M_parent = _Base_ptr();

       if (_M_nodes->_M_left)
  _M_nodes = _M_nodes->_M_left;
     }
   else
     _M_nodes = _Base_ptr();
 }


 _Reuse_or_alloc_node(const _Reuse_or_alloc_node&) = delete;


 ~_Reuse_or_alloc_node()
 {
   if (_M_root)
     _M_t._M_erase(static_cast<_Node&>(*_M_root)._M_node_ptr());
 }

 template<typename _Arg>
   _Node_ptr
   operator()(_Arg&& __arg)
   {
     _Base_ptr __base = _M_extract();
     if (__base)
       {
  _Node_ptr __node = static_cast<_Node&>(*__base)._M_node_ptr();
  _M_t._M_destroy_node(__node);
  _M_t._M_construct_node(__node, std::forward<_Arg>(__arg));
  return __node;
       }

     return _M_t._M_create_node(std::forward<_Arg>(__arg));
   }

      private:
 _Base_ptr
 _M_extract()
 {
   if (!_M_nodes)
     return _M_nodes;

   _Base_ptr __node = _M_nodes;
   _M_nodes = _M_nodes->_M_parent;
   if (_M_nodes)
     {
       if (_M_nodes->_M_right == __node)
  {
    _M_nodes->_M_right = _Base_ptr();

    if (_M_nodes->_M_left)
      {
        _M_nodes = _M_nodes->_M_left;

        while (_M_nodes->_M_right)
   _M_nodes = _M_nodes->_M_right;

        if (_M_nodes->_M_left)
   _M_nodes = _M_nodes->_M_left;
      }
  }
       else
  _M_nodes->_M_left = _Base_ptr();
     }
   else
     _M_root = _Base_ptr();

   return __node;
 }

 _Base_ptr _M_root;
 _Base_ptr _M_nodes;
 _Rb_tree& _M_t;
      };



      struct _Alloc_node
      {
 _Alloc_node(_Rb_tree& __t)
 : _M_t(__t) { }

 template<typename _Arg>
   _Node_ptr
   operator()(_Arg&& __arg) const
   { return _M_t._M_create_node(std::forward<_Arg>(__arg)); }

      private:
 _Rb_tree& _M_t;
      };

    public:
      typedef _Key key_type;
      typedef _Val value_type;
      typedef value_type* pointer;
      typedef const value_type* const_pointer;
      typedef value_type& reference;
      typedef const value_type& const_reference;
      typedef size_t size_type;
      typedef ptrdiff_t difference_type;
      typedef _Alloc allocator_type;

      _Node_allocator&
      _M_get_Node_allocator() noexcept
      { return this->_M_impl; }

      const _Node_allocator&
      _M_get_Node_allocator() const noexcept
      { return this->_M_impl; }

      allocator_type
      get_allocator() const noexcept
      { return allocator_type(_M_get_Node_allocator()); }

    protected:
      _Node_ptr
      _M_get_node()
      {

 return _Node_alloc_traits::allocate(_M_get_Node_allocator(), 1);
# 1185 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      }

      void
      _M_put_node(_Node_ptr __p) noexcept
      {

 _Node_alloc_traits::deallocate(_M_get_Node_allocator(), __p, 1);
# 1207 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      }
# 1230 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      template<typename... _Args>
 void
 _M_construct_node(_Node_ptr __node, _Args&&... __args)
 {
   try
     {
       ::new(std::addressof(*__node)) _Node;
       _Node_alloc_traits::construct(_M_get_Node_allocator(),
         __node->_M_valptr(),
         std::forward<_Args>(__args)...);
     }
   catch(...)
     {
       __node->~_Node();
       _M_put_node(__node);
       throw;
     }
 }

      template<typename... _Args>
 _Node_ptr
 _M_create_node(_Args&&... __args)
 {
   _Node_ptr __tmp = _M_get_node();
   _M_construct_node(__tmp, std::forward<_Args>(__args)...);
   return __tmp;
 }


      void
      _M_destroy_node(_Node_ptr __p) noexcept
      {



 _Node_alloc_traits::destroy(_M_get_Node_allocator(), __p->_M_valptr());
 __p->~_Node();

      }

      void
      _M_drop_node(_Node_ptr __p) noexcept
      {
 _M_destroy_node(__p);
 _M_put_node(__p);
      }

      template<bool _MoveValue, typename _NodeGen>
 _Node_ptr
 _M_clone_node(_Node_ptr __x, _NodeGen& __node_gen)
 {

   using _Vp = __conditional_t<_MoveValue,
          value_type&&,
          const value_type&>;

   _Node_ptr __tmp
     = __node_gen(std::forward<_Vp>(*__x->_M_valptr()));
   __tmp->_M_color = __x->_M_color;
   __tmp->_M_left = __tmp->_M_right = _Base_ptr();
   return __tmp;
 }

    protected:
      typedef typename _Node_traits::_Header_t _Header_t;





      template<typename _Key_compare,
        bool = __is_pod(_Key_compare)>

 struct _Rb_tree_impl
 : public _Node_allocator
 , public _Rb_tree_key_compare<_Key_compare>
 , public _Header_t
 {
   typedef _Rb_tree_key_compare<_Key_compare> _Base_key_compare;

   _Rb_tree_impl()
     noexcept(is_nothrow_default_constructible<_Node_allocator>::value && is_nothrow_default_constructible<_Base_key_compare>::value)


   : _Node_allocator()
   { }

   _Rb_tree_impl(const _Rb_tree_impl& __x)
   : _Node_allocator(_Node_alloc_traits::_S_select_on_copy(__x))
   , _Base_key_compare(__x._M_key_compare)
   , _Header_t()
   { }






   _Rb_tree_impl(_Rb_tree_impl&&)
     noexcept( is_nothrow_move_constructible<_Base_key_compare>::value )
   = default;

   explicit
   _Rb_tree_impl(_Node_allocator&& __a)
   : _Node_allocator(std::move(__a))
   { }

   _Rb_tree_impl(_Rb_tree_impl&& __x, _Node_allocator&& __a)
   : _Node_allocator(std::move(__a)),
     _Base_key_compare(std::move(__x)),
     _Header_t(std::move(__x))
   { }

   _Rb_tree_impl(const _Key_compare& __comp, _Node_allocator&& __a)
   : _Node_allocator(std::move(__a)), _Base_key_compare(__comp)
   { }

 };

      _Rb_tree_impl<_Compare> _M_impl;

    protected:
      _Base_ptr&
      _M_root() noexcept
      { return this->_M_impl._M_header._M_parent; }

      _Base_ptr
      _M_root() const noexcept
      { return this->_M_impl._M_header._M_parent; }

      _Base_ptr&
      _M_leftmost() noexcept
      { return this->_M_impl._M_header._M_left; }

      _Base_ptr
      _M_leftmost() const noexcept
      { return this->_M_impl._M_header._M_left; }

      _Base_ptr&
      _M_rightmost() noexcept
      { return this->_M_impl._M_header._M_right; }

      _Base_ptr
      _M_rightmost() const noexcept
      { return this->_M_impl._M_header._M_right; }

      _Base_ptr
      _M_begin() const noexcept
      { return this->_M_impl._M_header._M_parent; }

      _Node_ptr
      _M_begin_node() const noexcept
      {
 _Base_ptr __begin = this->_M_impl._M_header._M_parent;
 return __begin
   ? static_cast<_Node&>(*__begin)._M_node_ptr()
   : _Node_ptr();
      }

      _Base_ptr
      _M_end() const noexcept
      { return this->_M_impl._M_header._M_base_ptr(); }

      static const _Key&
      _S_key(const _Node& __node)
      {



 static_assert(__is_invocable<_Compare&, const _Key&, const _Key&>{},
        "comparison object must be invocable "
        "with two arguments of key type");
# 1412 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
 return _KeyOfValue()(*__node._M_valptr());
      }

      static const _Key&
      _S_key(_Base_ptr __x)
      { return _S_key(static_cast<const _Node&>(*__x)); }

      static const _Key&
      _S_key(_Node_ptr __x)
      { return _S_key(*__x); }

      static _Base_ptr
      _S_left(_Base_ptr __x) noexcept
      { return __x->_M_left; }

      static _Node_ptr
      _S_left(_Node_ptr __x)
      {
 return __x->_M_left
   ? static_cast<_Node&>(*__x->_M_left)._M_node_ptr()
   : _Node_ptr();
      }

      static _Base_ptr
      _S_right(_Base_ptr __x) noexcept
      { return __x->_M_right; }

      static _Node_ptr
      _S_right(_Node_ptr __x) noexcept
      {
 return __x->_M_right
   ? static_cast<_Node&>(*__x->_M_right)._M_node_ptr()
   : _Node_ptr();
      }

    public:
      typedef typename _Node_traits::_Iterator iterator;
      typedef typename _Node_traits::_Const_iterator const_iterator;

      typedef std::reverse_iterator<iterator> reverse_iterator;
      typedef std::reverse_iterator<const_iterator> const_reverse_iterator;
# 1461 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      pair<_Base_ptr, _Base_ptr>
      _M_get_insert_unique_pos(const key_type& __k);

      pair<_Base_ptr, _Base_ptr>
      _M_get_insert_equal_pos(const key_type& __k);

      pair<_Base_ptr, _Base_ptr>
      _M_get_insert_hint_unique_pos(const_iterator __pos,
        const key_type& __k);

      pair<_Base_ptr, _Base_ptr>
      _M_get_insert_hint_equal_pos(const_iterator __pos,
       const key_type& __k);

    private:

      template<typename _Arg, typename _NodeGen>
 iterator
 _M_insert_(_Base_ptr __x, _Base_ptr __y, _Arg&& __v, _NodeGen&);

      iterator
      _M_insert_node(_Base_ptr __x, _Base_ptr __y, _Node_ptr __z);

      template<typename _Arg>
 iterator
 _M_insert_lower(_Base_ptr __y, _Arg&& __v);

      template<typename _Arg>
 iterator
 _M_insert_equal_lower(_Arg&& __x);

      iterator
      _M_insert_lower_node(_Base_ptr __p, _Node_ptr __z);

      iterator
      _M_insert_equal_lower_node(_Node_ptr __z);
# 1512 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      enum { __as_lvalue, __as_rvalue };

      template<bool _MoveValues, typename _NodeGen>
 _Base_ptr
 _M_copy(_Node_ptr, _Base_ptr, _NodeGen&);

      template<bool _MoveValues, typename _NodeGen>
 _Base_ptr
 _M_copy(const _Rb_tree& __x, _NodeGen& __gen)
 {
   _Base_ptr __root =
     _M_copy<_MoveValues>(__x._M_begin_node(), _M_end(), __gen);
   _M_leftmost() = _Node_base::_S_minimum(__root);
   _M_rightmost() = _Node_base::_S_maximum(__root);
   _M_impl._M_node_count = __x._M_impl._M_node_count;
   return __root;
 }

      _Base_ptr
      _M_copy(const _Rb_tree& __x)
      {
 _Alloc_node __an(*this);
 return _M_copy<__as_lvalue>(__x, __an);
      }

      void
      _M_erase(_Node_ptr __x);

      _Base_ptr
      _M_lower_bound(_Base_ptr __x, _Base_ptr __y,
       const _Key& __k) const;

      _Base_ptr
      _M_upper_bound(_Base_ptr __x, _Base_ptr __y,
       const _Key& __k) const;

    public:




      _Rb_tree() = default;


      _Rb_tree(const _Compare& __comp,
        const allocator_type& __a = allocator_type())
      : _M_impl(__comp, _Node_allocator(__a)) { }

      _Rb_tree(const _Rb_tree& __x)
      : _M_impl(__x._M_impl)
      {
 if (__x._M_root())
   _M_root() = _M_copy(__x);
      }


      _Rb_tree(const allocator_type& __a)
      : _M_impl(_Node_allocator(__a))
      { }

      _Rb_tree(const _Rb_tree& __x, const allocator_type& __a)
      : _M_impl(__x._M_impl._M_key_compare, _Node_allocator(__a))
      {
 if (__x._M_root())
   _M_root() = _M_copy(__x);
      }

      _Rb_tree(_Rb_tree&&) = default;

      _Rb_tree(_Rb_tree&& __x, const allocator_type& __a)
      : _Rb_tree(std::move(__x), _Node_allocator(__a))
      { }

    private:
      _Rb_tree(_Rb_tree&& __x, _Node_allocator&& __a, true_type)
      noexcept(is_nothrow_default_constructible<_Compare>::value)
      : _M_impl(std::move(__x._M_impl), std::move(__a))
      { }

      _Rb_tree(_Rb_tree&& __x, _Node_allocator&& __a, false_type)
      : _M_impl(__x._M_impl._M_key_compare, std::move(__a))
      {
 if (__x._M_root())
   _M_move_data(__x, false_type{});
      }

    public:
      _Rb_tree(_Rb_tree&& __x, _Node_allocator&& __a)
      noexcept( noexcept(
 _Rb_tree(std::declval<_Rb_tree&&>(), std::declval<_Node_allocator&&>(),
   std::declval<typename _Node_alloc_traits::is_always_equal>())) )
      : _Rb_tree(std::move(__x), std::move(__a),
   typename _Node_alloc_traits::is_always_equal{})
      { }


      ~_Rb_tree() noexcept
      { _M_erase(_M_begin_node()); }

      _Rb_tree&
      operator=(const _Rb_tree& __x);


      _Compare
      key_comp() const
      { return _M_impl._M_key_compare; }

      iterator
      begin() noexcept
      { return iterator(this->_M_impl._M_header._M_left); }

      const_iterator
      begin() const noexcept
      { return const_iterator(this->_M_impl._M_header._M_left); }

      iterator
      end() noexcept
      { return iterator(_M_end()); }

      const_iterator
      end() const noexcept
      { return const_iterator(_M_end()); }

      reverse_iterator
      rbegin() noexcept
      { return reverse_iterator(end()); }

      const_reverse_iterator
      rbegin() const noexcept
      { return const_reverse_iterator(end()); }

      reverse_iterator
      rend() noexcept
      { return reverse_iterator(begin()); }

      const_reverse_iterator
      rend() const noexcept
      { return const_reverse_iterator(begin()); }

      [[__nodiscard__]] bool
      empty() const noexcept
      { return _M_impl._M_node_count == 0; }

      size_type
      size() const noexcept
      { return _M_impl._M_node_count; }

      size_type
      max_size() const noexcept
      { return _Node_alloc_traits::max_size(_M_get_Node_allocator()); }

      void
      swap(_Rb_tree& __t)
      noexcept(__is_nothrow_swappable<_Compare>::value);



      template<typename _Arg>
 pair<iterator, bool>
 _M_insert_unique(_Arg&& __x);

      template<typename _Arg>
 iterator
 _M_insert_equal(_Arg&& __x);

      template<typename _Arg, typename _NodeGen>
 iterator
 _M_insert_unique_(const_iterator __pos, _Arg&& __x, _NodeGen&);

      template<typename _Arg>
 iterator
 _M_insert_unique_(const_iterator __pos, _Arg&& __x)
 {
   _Alloc_node __an(*this);
   return _M_insert_unique_(__pos, std::forward<_Arg>(__x), __an);
 }

      template<typename _Arg, typename _NodeGen>
 iterator
 _M_insert_equal_(const_iterator __pos, _Arg&& __x, _NodeGen&);

      template<typename _Arg>
 iterator
 _M_insert_equal_(const_iterator __pos, _Arg&& __x)
 {
   _Alloc_node __an(*this);
   return _M_insert_equal_(__pos, std::forward<_Arg>(__x), __an);
 }

      template<typename... _Args>
 pair<iterator, bool>
 _M_emplace_unique(_Args&&... __args);

      template<typename... _Args>
 iterator
 _M_emplace_equal(_Args&&... __args);

      template<typename... _Args>
 iterator
 _M_emplace_hint_unique(const_iterator __pos, _Args&&... __args);

      template<typename... _Args>
 iterator
 _M_emplace_hint_equal(const_iterator __pos, _Args&&... __args);

      template<typename _Iter>
 using __same_value_type
   = is_same<value_type, typename iterator_traits<_Iter>::value_type>;

      template<typename _InputIterator>
 __enable_if_t<__same_value_type<_InputIterator>::value>
 _M_insert_range_unique(_InputIterator __first, _InputIterator __last)
 {
   _Alloc_node __an(*this);
   for (; __first != __last; ++__first)
     _M_insert_unique_(end(), *__first, __an);
 }

      template<typename _InputIterator>
 __enable_if_t<!__same_value_type<_InputIterator>::value>
 _M_insert_range_unique(_InputIterator __first, _InputIterator __last)
 {
   for (; __first != __last; ++__first)
     _M_emplace_unique(*__first);
 }

      template<typename _InputIterator>
 __enable_if_t<__same_value_type<_InputIterator>::value>
 _M_insert_range_equal(_InputIterator __first, _InputIterator __last)
 {
   _Alloc_node __an(*this);
   for (; __first != __last; ++__first)
     _M_insert_equal_(end(), *__first, __an);
 }

      template<typename _InputIterator>
 __enable_if_t<!__same_value_type<_InputIterator>::value>
 _M_insert_range_equal(_InputIterator __first, _InputIterator __last)
 {
   for (; __first != __last; ++__first)
     _M_emplace_equal(*__first);
 }
# 1803 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
    private:
      void
      _M_erase_aux(const_iterator __position);

      void
      _M_erase_aux(const_iterator __first, const_iterator __last);

    public:



      __attribute ((__abi_tag__ ("cxx11")))
      iterator
      erase(const_iterator __position)
      {
 do { if (__builtin_expect(!bool(__position != end()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h", 1818, __PRETTY_FUNCTION__, "__position != end()"); } while (false);
 const_iterator __result = __position;
 ++__result;
 _M_erase_aux(__position);
 return iterator(__result._M_node);
      }


      __attribute ((__abi_tag__ ("cxx11")))
      iterator
      erase(iterator __position)
      {
 do { if (__builtin_expect(!bool(__position != end()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h", 1830, __PRETTY_FUNCTION__, "__position != end()"); } while (false);
 iterator __result = __position;
 ++__result;
 _M_erase_aux(__position);
 return __result;
      }
# 1852 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      size_type
      erase(const key_type& __x);




      __attribute ((__abi_tag__ ("cxx11")))
      iterator
      erase(const_iterator __first, const_iterator __last)
      {
 _M_erase_aux(__first, __last);
 return iterator(__last._M_node);
      }
# 1875 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      void
      clear() noexcept
      {
 _M_erase(_M_begin_node());
 _M_impl._M_reset();
      }


      iterator
      find(const key_type& __k);

      const_iterator
      find(const key_type& __k) const;

      size_type
      count(const key_type& __k) const;

      iterator
      lower_bound(const key_type& __k)
      { return iterator(_M_lower_bound(_M_begin(), _M_end(), __k)); }

      const_iterator
      lower_bound(const key_type& __k) const
      {
 return const_iterator
   (_M_lower_bound(_M_begin(), _M_end(), __k));
      }

      iterator
      upper_bound(const key_type& __k)
      { return iterator(_M_upper_bound(_M_begin(), _M_end(), __k)); }

      const_iterator
      upper_bound(const key_type& __k) const
      {
 return const_iterator
   (_M_upper_bound(_M_begin(), _M_end(), __k));
      }

      pair<iterator, iterator>
      equal_range(const key_type& __k);

      pair<const_iterator, const_iterator>
      equal_range(const key_type& __k) const;
# 2012 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      bool
      __rb_verify() const;


      _Rb_tree&
      operator=(_Rb_tree&&)
      noexcept(_Node_alloc_traits::_S_nothrow_move()
        && is_nothrow_move_assignable<_Compare>::value);

      template<typename _Iterator>
 void
 _M_assign_unique(_Iterator, _Iterator);

      template<typename _Iterator>
 void
 _M_assign_equal(_Iterator, _Iterator);

    private:

      void
      _M_move_data(_Rb_tree& __x, true_type)
      { _M_impl._M_move_data(__x._M_impl); }



      void
      _M_move_data(_Rb_tree&, false_type);


      void
      _M_move_assign(_Rb_tree&, true_type);



      void
      _M_move_assign(_Rb_tree&, false_type);
# 2252 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      friend bool
      operator==(const _Rb_tree& __x, const _Rb_tree& __y)
      {
 return __x.size() == __y.size()
   && std::equal(__x.begin(), __x.end(), __y.begin());
      }
# 2269 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      friend bool
      operator<(const _Rb_tree& __x, const _Rb_tree& __y)
      {
 return std::lexicographical_compare(__x.begin(), __x.end(),
         __y.begin(), __y.end());
      }


    private:


      struct _Auto_node
      {
 template<typename... _Args>
   _Auto_node(_Rb_tree& __t, _Args&&... __args)
   : _M_t(__t),
     _M_node(__t._M_create_node(std::forward<_Args>(__args)...))
   { }

 ~_Auto_node()
 {
   if (_M_node)
     _M_t._M_drop_node(_M_node);
 }

 _Auto_node(_Auto_node&& __n)
 : _M_t(__n._M_t), _M_node(__n._M_node)
 { __n._M_node = nullptr; }

 const _Key&
 _M_key() const
 { return _S_key(_M_node); }

 iterator
 _M_insert(pair<_Base_ptr, _Base_ptr> __p)
 {
   auto __it = _M_t._M_insert_node(__p.first, __p.second, _M_node);
   _M_node = nullptr;
   return __it;
 }

 iterator
 _M_insert_equal_lower()
 {
   auto __it = _M_t._M_insert_equal_lower_node(_M_node);
   _M_node = nullptr;
   return __it;
 }

 _Rb_tree& _M_t;
 _Node_ptr _M_node;
      };

    };

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    inline void
    swap(_Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>& __x,
  _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>& __y)
    { __x.swap(__y); }


  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_move_data(_Rb_tree& __x, false_type)
    {
      if (_M_get_Node_allocator() == __x._M_get_Node_allocator())
 _M_move_data(__x, true_type());
      else
 {
   constexpr bool __move = !__move_if_noexcept_cond<value_type>::value;
   _Alloc_node __an(*this);
   _M_root() = _M_copy<__move>(__x, __an);
   if (__move)
     __x.clear();
 }
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    inline void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_move_assign(_Rb_tree& __x, true_type)
    {
      clear();
      if (__x._M_root())
 _M_move_data(__x, true_type());
      std::__alloc_on_move(_M_get_Node_allocator(),
      __x._M_get_Node_allocator());
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_move_assign(_Rb_tree& __x, false_type)
    {
      if (_M_get_Node_allocator() == __x._M_get_Node_allocator())
 return _M_move_assign(__x, true_type{});



      _Reuse_or_alloc_node __roan(*this);
      _M_impl._M_reset();
      if (__x._M_root())
 {
   _M_root() = _M_copy<__as_rvalue>(__x, __roan);
   __x.clear();
 }
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    inline _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>&
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    operator=(_Rb_tree&& __x)
    noexcept(_Node_alloc_traits::_S_nothrow_move()
      && is_nothrow_move_assignable<_Compare>::value)
    {
      _M_impl._M_key_compare = std::move(__x._M_impl._M_key_compare);
      _M_move_assign(__x,
       __bool_constant<_Node_alloc_traits::_S_nothrow_move()>());
      return *this;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    template<typename _Iterator>
      void
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_assign_unique(_Iterator __first, _Iterator __last)
      {
 _Reuse_or_alloc_node __roan(*this);
 _M_impl._M_reset();
 for (; __first != __last; ++__first)
   _M_insert_unique_(end(), *__first, __roan);
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    template<typename _Iterator>
      void
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_assign_equal(_Iterator __first, _Iterator __last)
      {
 _Reuse_or_alloc_node __roan(*this);
 _M_impl._M_reset();
 for (; __first != __last; ++__first)
   _M_insert_equal_(end(), *__first, __roan);
      }


  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>&
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    operator=(const _Rb_tree& __x)
    {
      if (this != std::__addressof(__x))
 {


   if (_Node_alloc_traits::_S_propagate_on_copy_assign())
     {
       auto& __this_alloc = this->_M_get_Node_allocator();
       auto& __that_alloc = __x._M_get_Node_allocator();
       if (!_Node_alloc_traits::_S_always_equal()
    && __this_alloc != __that_alloc)
  {


    clear();
    std::__alloc_on_copy(__this_alloc, __that_alloc);
  }
     }


   _Reuse_or_alloc_node __roan(*this);
   _M_impl._M_reset();
   _M_impl._M_key_compare = __x._M_impl._M_key_compare;
   if (__x._M_root())
     _M_root() = _M_copy<__as_lvalue>(__x, __roan);
 }

      return *this;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg, typename _NodeGen>



      typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_insert_(_Base_ptr __x, _Base_ptr __p,

   _Arg&& __v,



   _NodeGen& __node_gen)
      {
 bool __insert_left = (__x || __p == _M_end()
         || _M_impl._M_key_compare(_KeyOfValue()(__v),
       _S_key(__p)));

 _Base_ptr __z =
   __node_gen(std::forward<_Arg>(__v))->_M_base_ptr();

 _Node_traits::_S_insert_and_rebalance
   (__insert_left, __z, __p, this->_M_impl._M_header);
 ++_M_impl._M_node_count;
 return iterator(__z);
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg>

    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::

    _M_insert_lower(_Base_ptr __p, _Arg&& __v)



    {
      bool __insert_left = (__p == _M_end()
       || !_M_impl._M_key_compare(_S_key(__p),
             _KeyOfValue()(__v)));

      _Base_ptr __z =
 _M_create_node(std::forward<_Arg>(__v))->_M_base_ptr();
      _Node_traits::_S_insert_and_rebalance
 (__insert_left, __z, __p, this->_M_impl._M_header);
      ++_M_impl._M_node_count;
      return iterator(__z);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg>

    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::

    _M_insert_equal_lower(_Arg&& __v)



    {
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      while (__x)
 {
   __y = __x;
   __x = !_M_impl._M_key_compare(_S_key(__x), _KeyOfValue()(__v)) ?
  _S_left(__x) : _S_right(__x);
 }
      return _M_insert_lower(__y, std::forward<_Arg>(__v));
    }

  template<typename _Key, typename _Val, typename _KoV,
    typename _Compare, typename _Alloc>
    template<bool _MoveValues, typename _NodeGen>
      typename _Rb_tree<_Key, _Val, _KoV, _Compare, _Alloc>::_Base_ptr
      _Rb_tree<_Key, _Val, _KoV, _Compare, _Alloc>::
      _M_copy(_Node_ptr __x, _Base_ptr __p, _NodeGen& __node_gen)
      {

 _Node_ptr __top = _M_clone_node<_MoveValues>(__x, __node_gen);
 _Base_ptr __top_base = __top->_M_base_ptr();
 __top->_M_parent = __p;

 try
   {
     if (__x->_M_right)
       __top->_M_right =
  _M_copy<_MoveValues>(_S_right(__x), __top_base, __node_gen);
     __p = __top_base;
     __x = _S_left(__x);

     while (__x)
       {
  _Base_ptr __y =
    _M_clone_node<_MoveValues>(__x, __node_gen)->_M_base_ptr();
  __p->_M_left = __y;
  __y->_M_parent = __p;
  if (__x->_M_right)
    __y->_M_right = _M_copy<_MoveValues>(_S_right(__x),
             __y, __node_gen);
  __p = __y;
  __x = _S_left(__x);
       }
   }
 catch(...)
   {
     _M_erase(__top);
     throw;
   }
 return __top_base;
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_erase(_Node_ptr __x)
    {

      while (__x)
 {
   _M_erase(_S_right(__x));
   _Node_ptr __y = _S_left(__x);
   _M_drop_node(__x);
   __x = __y;
 }
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    typename _Rb_tree<_Key, _Val, _KeyOfValue,
        _Compare, _Alloc>::_Base_ptr
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_lower_bound(_Base_ptr __x, _Base_ptr __y,
     const _Key& __k) const
    {
      while (__x)
 if (!_M_impl._M_key_compare(_S_key(__x), __k))
   __y = __x, __x = _S_left(__x);
 else
   __x = _S_right(__x);
      return __y;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    typename _Rb_tree<_Key, _Val, _KeyOfValue,
        _Compare, _Alloc>::_Base_ptr
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_upper_bound(_Base_ptr __x, _Base_ptr __y,
     const _Key& __k) const
    {
      while (__x)
 if (_M_impl._M_key_compare(__k, _S_key(__x)))
   __y = __x, __x = _S_left(__x);
 else
   __x = _S_right(__x);
      return __y;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::iterator,
  typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::iterator>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    equal_range(const _Key& __k)
    {
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      while (__x)
 {
   if (_M_impl._M_key_compare(_S_key(__x), __k))
     __x = _S_right(__x);
   else if (_M_impl._M_key_compare(__k, _S_key(__x)))
     __y = __x, __x = _S_left(__x);
   else
     {
       _Base_ptr __xu(__x);
       _Base_ptr __yu(__y);
       __y = __x, __x = _S_left(__x);
       __xu = _S_right(__xu);
       return make_pair(iterator(_M_lower_bound(__x, __y, __k)),
          iterator(_M_upper_bound(__xu, __yu, __k)));
     }
 }
      return pair<iterator, iterator>(iterator(__y),
          iterator(__y));
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::const_iterator,
  typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::const_iterator>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    equal_range(const _Key& __k) const
    {
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      while (__x)
 {
   if (_M_impl._M_key_compare(_S_key(__x), __k))
     __x = _S_right(__x);
   else if (_M_impl._M_key_compare(__k, _S_key(__x)))
     __y = __x, __x = _S_left(__x);
   else
     {
       _Base_ptr __xu(__x);
       _Base_ptr __yu(__y);
       __y = __x, __x = _S_left(__x);
       __xu = _S_right(__xu);
       return make_pair(const_iterator(_M_lower_bound(__x, __y, __k)),
          const_iterator(_M_upper_bound(__xu, __yu, __k)));
     }
 }
      return pair<const_iterator, const_iterator>(const_iterator(__y),
        const_iterator(__y));
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    swap(_Rb_tree& __t)
    noexcept(__is_nothrow_swappable<_Compare>::value)
    {
      if (!_M_root())
 {
   if (__t._M_root())
     _M_impl._M_move_data(__t._M_impl);
 }
      else if (!__t._M_root())
 __t._M_impl._M_move_data(_M_impl);
      else
 {
   std::swap(_M_root(),__t._M_root());
   std::swap(_M_leftmost(),__t._M_leftmost());
   std::swap(_M_rightmost(),__t._M_rightmost());

   _M_root()->_M_parent = _M_end();
   __t._M_root()->_M_parent = __t._M_end();
   std::swap(this->_M_impl._M_node_count, __t._M_impl._M_node_count);
 }


      using std::swap;
      swap(this->_M_impl._M_key_compare, __t._M_impl._M_key_compare);

      _Node_alloc_traits::_S_on_swap(_M_get_Node_allocator(),
         __t._M_get_Node_allocator());
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr,
  typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_get_insert_unique_pos(const key_type& __k)
    {
      typedef pair<_Base_ptr, _Base_ptr> _Res;
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      bool __comp = true;
      while (__x)
 {
   __y = __x;
   __comp = _M_impl._M_key_compare(__k, _S_key(__x));
   __x = __comp ? _S_left(__x) : _S_right(__x);
 }
      iterator __j = iterator(__y);
      if (__comp)
 {
   if (__j == begin())
     return _Res(__x, __y);
   else
     --__j;
 }
      if (_M_impl._M_key_compare(_S_key(__j._M_node), __k))
 return _Res(__x, __y);
      return _Res(__j._M_node, _Base_ptr());
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr,
  typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_get_insert_equal_pos(const key_type& __k)
    {
      typedef pair<_Base_ptr, _Base_ptr> _Res;
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      while (__x)
 {
   __y = __x;
   __x = _M_impl._M_key_compare(__k, _S_key(__x)) ?
  _S_left(__x) : _S_right(__x);
 }
      return _Res(__x, __y);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg>

    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::iterator, bool>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::

    _M_insert_unique(_Arg&& __v)



    {
      typedef pair<iterator, bool> _Res;
      pair<_Base_ptr, _Base_ptr> __res
 = _M_get_insert_unique_pos(_KeyOfValue()(__v));

      if (__res.second)
 {
   _Alloc_node __an(*this);
   return _Res(_M_insert_(__res.first, __res.second,
     std::forward<_Arg>(__v), __an),
        true);
 }

      return _Res(iterator(__res.first), false);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg>

    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::

    _M_insert_equal(_Arg&& __v)



    {
      pair<_Base_ptr, _Base_ptr> __res
 = _M_get_insert_equal_pos(_KeyOfValue()(__v));
      _Alloc_node __an(*this);
      return _M_insert_(__res.first, __res.second,
   std::forward<_Arg>(__v), __an);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr,
  typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_get_insert_hint_unique_pos(const_iterator __position,
      const key_type& __k)
    {
      typedef pair<_Base_ptr, _Base_ptr> _Res;


      if (__position._M_node == _M_end())
 {
   if (size() > 0
       && _M_impl._M_key_compare(_S_key(_M_rightmost()), __k))
     return _Res(_Base_ptr(), _M_rightmost());
   else
     return _M_get_insert_unique_pos(__k);
 }
      else if (_M_impl._M_key_compare(__k, _S_key(__position._M_node)))
 {

   iterator __before(__position._M_node);
   if (__position._M_node == _M_leftmost())
     return _Res(_M_leftmost(), _M_leftmost());
   else if (_M_impl._M_key_compare(_S_key((--__before)._M_node), __k))
     {
       if (!_S_right(__before._M_node))
  return _Res(_Base_ptr(), __before._M_node);
       else
  return _Res(__position._M_node, __position._M_node);
     }
   else
     return _M_get_insert_unique_pos(__k);
 }
      else if (_M_impl._M_key_compare(_S_key(__position._M_node), __k))
 {

   iterator __after(__position._M_node);
   if (__position._M_node == _M_rightmost())
     return _Res(_Base_ptr(), _M_rightmost());
   else if (_M_impl._M_key_compare(__k, _S_key((++__after)._M_node)))
     {
       if (!_S_right(__position._M_node))
  return _Res(_Base_ptr(), __position._M_node);
       else
  return _Res(__after._M_node, __after._M_node);
     }
   else
     return _M_get_insert_unique_pos(__k);
 }
      else

 return _Res(__position._M_node, _Base_ptr());
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg, typename _NodeGen>



      typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_insert_unique_(const_iterator __position,

   _Arg&& __v,



   _NodeGen& __node_gen)
    {
      pair<_Base_ptr, _Base_ptr> __res
 = _M_get_insert_hint_unique_pos(__position, _KeyOfValue()(__v));

      if (__res.second)
 return _M_insert_(__res.first, __res.second,
     std::forward<_Arg>(__v),
     __node_gen);
      return iterator(__res.first);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr,
  typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::_Base_ptr>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_get_insert_hint_equal_pos(const_iterator __position, const key_type& __k)
    {
      typedef pair<_Base_ptr, _Base_ptr> _Res;


      if (__position._M_node == _M_end())
 {
   if (size() > 0
       && !_M_impl._M_key_compare(__k, _S_key(_M_rightmost())))
     return _Res(_Base_ptr(), _M_rightmost());
   else
     return _M_get_insert_equal_pos(__k);
 }
      else if (!_M_impl._M_key_compare(_S_key(__position._M_node), __k))
 {

   iterator __before(__position._M_node);
   if (__position._M_node == _M_leftmost())
     return _Res(_M_leftmost(), _M_leftmost());
   else if (!_M_impl._M_key_compare(__k, _S_key((--__before)._M_node)))
     {
       if (!_S_right(__before._M_node))
  return _Res(_Base_ptr(), __before._M_node);
       else
  return _Res(__position._M_node, __position._M_node);
     }
   else
     return _M_get_insert_equal_pos(__k);
 }
      else
 {

   iterator __after(__position._M_node);
   if (__position._M_node == _M_rightmost())
     return _Res(_Base_ptr(), _M_rightmost());
   else if (!_M_impl._M_key_compare(_S_key((++__after)._M_node), __k))
     {
       if (!_S_right(__position._M_node))
  return _Res(_Base_ptr(), __position._M_node);
       else
  return _Res(__after._M_node, __after._M_node);
     }
   else
     return _Res(_Base_ptr(), _Base_ptr());
 }
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>

    template<typename _Arg, typename _NodeGen>



      typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_insert_equal_(const_iterator __position,

         _Arg&& __v,



         _NodeGen& __node_gen)
      {
 pair<_Base_ptr, _Base_ptr> __res
   = _M_get_insert_hint_equal_pos(__position, _KeyOfValue()(__v));

 if (__res.second)
   return _M_insert_(__res.first, __res.second,
       std::forward<_Arg>(__v),
       __node_gen);

 return _M_insert_equal_lower(std::forward<_Arg>(__v));
      }


  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    auto
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_insert_node(_Base_ptr __x, _Base_ptr __p, _Node_ptr __z)
    -> iterator
    {
      bool __insert_left = (__x || __p == _M_end()
       || _M_impl._M_key_compare(_S_key(__z),
            _S_key(__p)));

      _Base_ptr __base_z = __z->_M_base_ptr();
      _Node_traits::_S_insert_and_rebalance
 (__insert_left, __base_z, __p, this->_M_impl._M_header);
      ++_M_impl._M_node_count;
      return iterator(__base_z);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    auto
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_insert_lower_node(_Base_ptr __p, _Node_ptr __z)
    -> iterator
    {
      bool __insert_left = (__p == _M_end()
       || !_M_impl._M_key_compare(_S_key(__p),
             _S_key(__z)));

      _Base_ptr __base_z = __z->_M_base_ptr();
      _Node_traits::_S_insert_and_rebalance
 (__insert_left, __base_z, __p, this->_M_impl._M_header);
      ++_M_impl._M_node_count;
      return iterator(__base_z);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    auto
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_insert_equal_lower_node(_Node_ptr __z)
    -> iterator
    {
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      while (__x)
 {
   __y = __x;
   __x = !_M_impl._M_key_compare(_S_key(__x), _S_key(__z)) ?
  _S_left(__x) : _S_right(__x);
 }
      return _M_insert_lower_node(__y, __z);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    template<typename... _Args>
      auto
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_emplace_unique(_Args&&... __args)
      -> pair<iterator, bool>
      {
 _Auto_node __z(*this, std::forward<_Args>(__args)...);
 auto __res = _M_get_insert_unique_pos(__z._M_key());
 if (__res.second)
   return {__z._M_insert(__res), true};
 return {iterator(__res.first), false};
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    template<typename... _Args>
      auto
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_emplace_equal(_Args&&... __args)
      -> iterator
      {
 _Auto_node __z(*this, std::forward<_Args>(__args)...);
 auto __res = _M_get_insert_equal_pos(__z._M_key());
 return __z._M_insert(__res);
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    template<typename... _Args>
      auto
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_emplace_hint_unique(const_iterator __pos, _Args&&... __args)
      -> iterator
      {
 _Auto_node __z(*this, std::forward<_Args>(__args)...);
 auto __res = _M_get_insert_hint_unique_pos(__pos, __z._M_key());
 if (__res.second)
   return __z._M_insert(__res);
 return iterator(__res.first);
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    template<typename... _Args>
      auto
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_emplace_hint_equal(const_iterator __pos, _Args&&... __args)
      -> iterator
      {
 _Auto_node __z(*this, std::forward<_Args>(__args)...);
 auto __res = _M_get_insert_hint_equal_pos(__pos, __z._M_key());
 if (__res.second)
   return __z._M_insert(__res);
 return __z._M_insert_equal_lower();
      }



  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_erase_aux(const_iterator __position)
    {
      _Base_ptr __y = _Node_traits::_S_rebalance_for_erase
 (__position._M_node, this->_M_impl._M_header);
      _M_drop_node(static_cast<_Node&>(*__y)._M_node_ptr());
      --_M_impl._M_node_count;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    void
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    _M_erase_aux(const_iterator __first, const_iterator __last)
    {
      if (__first == begin() && __last == end())
 clear();
      else
 while (__first != __last)
   _M_erase_aux(__first++);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::size_type
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    erase(const _Key& __x)
    {
      pair<iterator, iterator> __p = equal_range(__x);
      const size_type __old_size = size();
      _M_erase_aux(__p.first, __p.second);
      return __old_size - size();
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    typename _Rb_tree<_Key, _Val, _KeyOfValue,
        _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    find(const _Key& __k)
    {
      iterator __j(_M_lower_bound(_M_begin(), _M_end(), __k));
      return (__j == end()
       || _M_impl._M_key_compare(__k,
     _S_key(__j._M_node))) ? end() : __j;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    typename _Rb_tree<_Key, _Val, _KeyOfValue,
        _Compare, _Alloc>::const_iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    find(const _Key& __k) const
    {
      const_iterator __j(_M_lower_bound(_M_begin(), _M_end(), __k));
      return (__j == end()
       || _M_impl._M_key_compare(__k,
     _S_key(__j._M_node))) ? end() : __j;
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::size_type
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    count(const _Key& __k) const
    {
      pair<const_iterator, const_iterator> __p = equal_range(__k);
      const size_type __n = std::distance(__p.first, __p.second);
      return __n;
    }

  __attribute__ ((__pure__)) unsigned int
  _Rb_tree_black_count(const _Rb_tree_node_base* __node,
         const _Rb_tree_node_base* __root) throw ();

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    bool
    _Rb_tree<_Key,_Val,_KeyOfValue,_Compare,_Alloc>::__rb_verify() const
    {
      if (_M_impl._M_node_count == 0 || begin() == end())
 return _M_impl._M_node_count == 0 && begin() == end()
        && this->_M_impl._M_header._M_left == _M_end()
        && this->_M_impl._M_header._M_right == _M_end();

      unsigned int __len = _Rb_tree_black_count(_M_leftmost(), _M_root());
      for (const_iterator __it = begin(); __it != end(); ++__it)
 {
   _Base_ptr __x = __it._M_node;
   _Base_ptr __L = _S_left(__x);
   _Base_ptr __R = _S_right(__x);

   if (__x->_M_color == _S_red)
     if ((__L && __L->_M_color == _S_red)
  || (__R && __R->_M_color == _S_red))
       return false;

   if (__L && _M_impl._M_key_compare(_S_key(__x), _S_key(__L)))
     return false;
   if (__R && _M_impl._M_key_compare(_S_key(__R), _S_key(__x)))
     return false;

   if (!__L && !__R && _Rb_tree_black_count(__x, _M_root()) != __len)
     return false;
 }

      if (_M_leftmost() != _Node_base::_S_minimum(_M_root()))
 return false;
      if (_M_rightmost() != _Node_base::_S_maximum(_M_root()))
 return false;
      return true;
    }
# 3238 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3

}
# 65 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
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
# 63 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  struct __erased_type { };




  template<typename _Alloc, typename _Tp>
    using __is_erased_or_convertible
      = __or_<is_convertible<_Alloc, _Tp>, is_same<_Tp, __erased_type>>;


  struct allocator_arg_t { explicit allocator_arg_t() = default; };

  constexpr allocator_arg_t allocator_arg =
    allocator_arg_t();

  template<typename _Tp, typename _Alloc, typename = __void_t<>>
    struct __uses_allocator_helper
    : false_type { };

  template<typename _Tp, typename _Alloc>
    struct __uses_allocator_helper<_Tp, _Alloc,
       __void_t<typename _Tp::allocator_type>>
    : __is_erased_or_convertible<_Alloc, typename _Tp::allocator_type>::type
    { };


  template<typename _Tp, typename _Alloc>
    struct uses_allocator
    : __uses_allocator_helper<_Tp, _Alloc>::type
    { };

  struct __uses_alloc_base { };

  struct __uses_alloc0 : __uses_alloc_base
  {
    struct _Sink { void operator=(const void*) { } } _M_a;
  };

  template<typename _Alloc>
    struct __uses_alloc1 : __uses_alloc_base { const _Alloc* _M_a; };

  template<typename _Alloc>
    struct __uses_alloc2 : __uses_alloc_base { const _Alloc* _M_a; };

  template<bool, typename _Tp, typename _Alloc, typename... _Args>
    struct __uses_alloc;

  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __uses_alloc<true, _Tp, _Alloc, _Args...>
    : __conditional_t<
        is_constructible<_Tp, allocator_arg_t, const _Alloc&, _Args...>::value,
        __uses_alloc1<_Alloc>,
        __uses_alloc2<_Alloc>>
    {


      static_assert(__or_<
   is_constructible<_Tp, allocator_arg_t, const _Alloc&, _Args...>,
   is_constructible<_Tp, _Args..., const _Alloc&>>::value,
   "construction with an allocator must be possible"
   " if uses_allocator is true");
    };

  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __uses_alloc<false, _Tp, _Alloc, _Args...>
    : __uses_alloc0 { };

  template<typename _Tp, typename _Alloc, typename... _Args>
    using __uses_alloc_t =
      __uses_alloc<uses_allocator<_Tp, _Alloc>::value, _Tp, _Alloc, _Args...>;

  template<typename _Tp, typename _Alloc, typename... _Args>
   
    inline __uses_alloc_t<_Tp, _Alloc, _Args...>
    __use_alloc(const _Alloc& __a)
    {
      __uses_alloc_t<_Tp, _Alloc, _Args...> __ret;
      __ret._M_a = std::__addressof(__a);
      return __ret;
    }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void
    __use_alloc(const _Alloc&&) = delete;
# 142 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 3
  template<template<typename...> class _Predicate,
    typename _Tp, typename _Alloc, typename... _Args>
    struct __is_uses_allocator_predicate
    : __conditional_t<uses_allocator<_Tp, _Alloc>::value,
      __or_<_Predicate<_Tp, allocator_arg_t, _Alloc, _Args...>,
     _Predicate<_Tp, _Args..., _Alloc>>,
      _Predicate<_Tp, _Args...>> { };

  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __is_uses_allocator_constructible
    : __is_uses_allocator_predicate<is_constructible, _Tp, _Alloc, _Args...>
    { };







  template<typename _Tp, typename _Alloc, typename... _Args>
    struct __is_nothrow_uses_allocator_constructible
    : __is_uses_allocator_predicate<is_nothrow_constructible,
        _Tp, _Alloc, _Args...>
    { };
# 175 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/uses_allocator.h" 3
  template<typename _Tp, typename... _Args>
    void __uses_allocator_construct_impl(__uses_alloc0, _Tp* __ptr,
      _Args&&... __args)
    { ::new ((void*)__ptr) _Tp(std::forward<_Args>(__args)...); }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void __uses_allocator_construct_impl(__uses_alloc1<_Alloc> __a, _Tp* __ptr,
      _Args&&... __args)
    {
      ::new ((void*)__ptr) _Tp(allocator_arg, *__a._M_a,
          std::forward<_Args>(__args)...);
    }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void __uses_allocator_construct_impl(__uses_alloc2<_Alloc> __a, _Tp* __ptr,
      _Args&&... __args)
    { ::new ((void*)__ptr) _Tp(std::forward<_Args>(__args)..., *__a._M_a); }

  template<typename _Tp, typename _Alloc, typename... _Args>
    void __uses_allocator_construct(const _Alloc& __a, _Tp* __ptr,
        _Args&&... __args)
    {
      std::__uses_allocator_construct_impl(
   std::__use_alloc<_Tp, _Alloc, _Args...>(__a), __ptr,
   std::forward<_Args>(__args)...);
    }



}
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3

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
    __enable_if_t<is_void<_Res>::value, _Res>
    __invoke_r(_Callable&& __fn, _Args&&... __args)
    {
      using __result = __invoke_result<_Callable, _Args...>;
      using __type = typename __result::type;
      using __tag = typename __result::__invoke_type;
      std::__invoke_impl<__type>(__tag{}, std::forward<_Callable>(__fn),
     std::forward<_Args>(__args)...);
    }



}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3
# 57 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 58 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename... _Elements>
    class tuple;


  template<typename _Tp>
    struct __is_empty_non_tuple : is_empty<_Tp> { };


  template<typename _El0, typename... _El>
    struct __is_empty_non_tuple<tuple<_El0, _El...>> : false_type { };


  template<typename _Tp>
    using __empty_not_final
    = __conditional_t<__is_final(_Tp), false_type,
        __is_empty_non_tuple<_Tp>>;

  template<size_t _Idx, typename _Head,
    bool = __empty_not_final<_Head>::value>
    struct _Head_base;


  template<size_t _Idx, typename _Head>
    struct _Head_base<_Idx, _Head, true>
    {
      constexpr _Head_base()
      : _M_head_impl() { }

      constexpr _Head_base(const _Head& __h)
      : _M_head_impl(__h) { }

      constexpr _Head_base(const _Head_base&) = default;
      constexpr _Head_base(_Head_base&&) = default;

      template<typename _UHead>
 constexpr _Head_base(_UHead&& __h)
 : _M_head_impl(std::forward<_UHead>(__h)) { }

     
      _Head_base(allocator_arg_t, __uses_alloc0)
      : _M_head_impl() { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc1<_Alloc> __a)
 : _M_head_impl(allocator_arg, *__a._M_a) { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc2<_Alloc> __a)
 : _M_head_impl(*__a._M_a) { }

      template<typename _UHead>

 _Head_base(__uses_alloc0, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead)) { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc1<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(allocator_arg, *__a._M_a, std::forward<_UHead>(__uhead))
 { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc2<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead), *__a._M_a) { }

      static constexpr _Head&
      _M_head(_Head_base& __b) noexcept { return __b._M_head_impl; }

      static constexpr const _Head&
      _M_head(const _Head_base& __b) noexcept { return __b._M_head_impl; }

      [[__no_unique_address__]] _Head _M_head_impl;
    };
# 199 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<size_t _Idx, typename _Head>
    struct _Head_base<_Idx, _Head, false>
    {
      constexpr _Head_base()
      : _M_head_impl() { }

      constexpr _Head_base(const _Head& __h)
      : _M_head_impl(__h) { }

      constexpr _Head_base(const _Head_base&) = default;
      constexpr _Head_base(_Head_base&&) = default;

      template<typename _UHead>
        constexpr _Head_base(_UHead&& __h)
 : _M_head_impl(std::forward<_UHead>(__h)) { }

     
      _Head_base(allocator_arg_t, __uses_alloc0)
      : _M_head_impl() { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc1<_Alloc> __a)
 : _M_head_impl(allocator_arg, *__a._M_a) { }

      template<typename _Alloc>

 _Head_base(allocator_arg_t, __uses_alloc2<_Alloc> __a)
 : _M_head_impl(*__a._M_a) { }

      template<typename _UHead>

 _Head_base(__uses_alloc0, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead)) { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc1<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(allocator_arg, *__a._M_a, std::forward<_UHead>(__uhead))
 { }

      template<typename _Alloc, typename _UHead>

 _Head_base(__uses_alloc2<_Alloc> __a, _UHead&& __uhead)
 : _M_head_impl(std::forward<_UHead>(__uhead), *__a._M_a) { }

      static constexpr _Head&
      _M_head(_Head_base& __b) noexcept { return __b._M_head_impl; }

      static constexpr const _Head&
      _M_head(const _Head_base& __b) noexcept { return __b._M_head_impl; }

      _Head _M_head_impl;
    };
# 272 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<size_t _Idx, typename... _Elements>
    struct _Tuple_impl;






  template<size_t _Idx, typename _Head, typename... _Tail>
    struct _Tuple_impl<_Idx, _Head, _Tail...>
    : public _Tuple_impl<_Idx + 1, _Tail...>,
      private _Head_base<_Idx, _Head>
    {
      template<size_t, typename...> friend struct _Tuple_impl;

      typedef _Tuple_impl<_Idx + 1, _Tail...> _Inherited;
      typedef _Head_base<_Idx, _Head> _Base;

      static constexpr _Head&
      _M_head(_Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      static constexpr const _Head&
      _M_head(const _Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      static constexpr _Inherited&
      _M_tail(_Tuple_impl& __t) noexcept { return __t; }

      static constexpr const _Inherited&
      _M_tail(const _Tuple_impl& __t) noexcept { return __t; }

      constexpr _Tuple_impl()
      : _Inherited(), _Base() { }

      explicit constexpr
      _Tuple_impl(const _Head& __head, const _Tail&... __tail)
      : _Inherited(__tail...), _Base(__head)
      { }

      template<typename _UHead, typename... _UTail,
        typename = __enable_if_t<sizeof...(_Tail) == sizeof...(_UTail)>>
 explicit constexpr
 _Tuple_impl(_UHead&& __head, _UTail&&... __tail)
 : _Inherited(std::forward<_UTail>(__tail)...),
   _Base(std::forward<_UHead>(__head))
 { }

      constexpr _Tuple_impl(const _Tuple_impl&) = default;



      _Tuple_impl& operator=(const _Tuple_impl&) = delete;

      _Tuple_impl(_Tuple_impl&&) = default;

      template<typename... _UElements>
 constexpr
 _Tuple_impl(const _Tuple_impl<_Idx, _UElements...>& __in)
 : _Inherited(_Tuple_impl<_Idx, _UElements...>::_M_tail(__in)),
   _Base(_Tuple_impl<_Idx, _UElements...>::_M_head(__in))
 { }

      template<typename _UHead, typename... _UTails>
 constexpr
 _Tuple_impl(_Tuple_impl<_Idx, _UHead, _UTails...>&& __in)
 : _Inherited(std::move
       (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in))),
   _Base(std::forward<_UHead>
  (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in)))
 { }
# 368 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a),
   _Base(__tag, __use_alloc<_Head>(__a))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Head& __head, const _Tail&... __tail)
 : _Inherited(__tag, __a, __tail...),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a), __head)
 { }

      template<typename _Alloc, typename _UHead, typename... _UTail,
        typename = __enable_if_t<sizeof...(_Tail) == sizeof...(_UTail)>>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _UHead&& __head, _UTail&&... __tail)
 : _Inherited(__tag, __a, std::forward<_UTail>(__tail)...),
   _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(__head))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Tuple_impl& __in)
 : _Inherited(__tag, __a, _M_tail(__in)),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a), _M_head(__in))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _Tuple_impl&& __in)
 : _Inherited(__tag, __a, std::move(_M_tail(__in))),
   _Base(__use_alloc<_Head, _Alloc, _Head>(__a),
  std::forward<_Head>(_M_head(__in)))
 { }

      template<typename _Alloc, typename _UHead, typename... _UTails>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      const _Tuple_impl<_Idx, _UHead, _UTails...>& __in)
 : _Inherited(__tag, __a,
       _Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in)),
   _Base(__use_alloc<_Head, _Alloc, const _UHead&>(__a),
  _Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in))
 { }

      template<typename _Alloc, typename _UHead, typename... _UTails>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a,
      _Tuple_impl<_Idx, _UHead, _UTails...>&& __in)
 : _Inherited(__tag, __a, std::move
       (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in))),
   _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>
  (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in)))
 { }
# 463 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename... _UElements>

 void
 _M_assign(const _Tuple_impl<_Idx, _UElements...>& __in)
 {
   _M_head(*this) = _Tuple_impl<_Idx, _UElements...>::_M_head(__in);
   _M_tail(*this)._M_assign(
       _Tuple_impl<_Idx, _UElements...>::_M_tail(__in));
 }

      template<typename _UHead, typename... _UTails>

 void
 _M_assign(_Tuple_impl<_Idx, _UHead, _UTails...>&& __in)
 {
   _M_head(*this) = std::forward<_UHead>
     (_Tuple_impl<_Idx, _UHead, _UTails...>::_M_head(__in));
   _M_tail(*this)._M_assign(
       std::move(_Tuple_impl<_Idx, _UHead, _UTails...>::_M_tail(__in)));
 }
# 523 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    protected:
     
      void
      _M_swap(_Tuple_impl& __in)
      {
 using std::swap;
 swap(_M_head(*this), _M_head(__in));
 _Inherited::_M_swap(_M_tail(__in));
      }
# 542 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };


  template<size_t _Idx, typename _Head>
    struct _Tuple_impl<_Idx, _Head>
    : private _Head_base<_Idx, _Head>
    {
      template<size_t, typename...> friend struct _Tuple_impl;

      typedef _Head_base<_Idx, _Head> _Base;

      static constexpr _Head&
      _M_head(_Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      static constexpr const _Head&
      _M_head(const _Tuple_impl& __t) noexcept { return _Base::_M_head(__t); }

      constexpr
      _Tuple_impl()
      : _Base() { }

      explicit constexpr
      _Tuple_impl(const _Head& __head)
      : _Base(__head)
      { }

      template<typename _UHead>
 explicit constexpr
 _Tuple_impl(_UHead&& __head)
 : _Base(std::forward<_UHead>(__head))
 { }

      constexpr _Tuple_impl(const _Tuple_impl&) = default;



      _Tuple_impl& operator=(const _Tuple_impl&) = delete;




      constexpr
      _Tuple_impl(_Tuple_impl&& __in)
      noexcept(is_nothrow_move_constructible<_Head>::value)
      : _Base(static_cast<_Base&&>(__in))
      { }


      template<typename _UHead>
 constexpr
 _Tuple_impl(const _Tuple_impl<_Idx, _UHead>& __in)
 : _Base(_Tuple_impl<_Idx, _UHead>::_M_head(__in))
 { }

      template<typename _UHead>
 constexpr
 _Tuple_impl(_Tuple_impl<_Idx, _UHead>&& __in)
 : _Base(std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in)))
 { }
# 624 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t __tag, const _Alloc& __a)
 : _Base(__tag, __use_alloc<_Head>(__a))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Head& __head)
 : _Base(__use_alloc<_Head, _Alloc, const _Head&>(__a), __head)
 { }

      template<typename _Alloc, typename _UHead>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _UHead&& __head)
 : _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(__head))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Tuple_impl& __in)
 : _Base(__use_alloc<_Head, _Alloc, const _Head&>(__a), _M_head(__in))
 { }

      template<typename _Alloc>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _Tuple_impl&& __in)
 : _Base(__use_alloc<_Head, _Alloc, _Head>(__a),
  std::forward<_Head>(_M_head(__in)))
 { }

      template<typename _Alloc, typename _UHead>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      const _Tuple_impl<_Idx, _UHead>& __in)
 : _Base(__use_alloc<_Head, _Alloc, const _UHead&>(__a),
  _Tuple_impl<_Idx, _UHead>::_M_head(__in))
 { }

      template<typename _Alloc, typename _UHead>

 _Tuple_impl(allocator_arg_t, const _Alloc& __a,
      _Tuple_impl<_Idx, _UHead>&& __in)
 : _Base(__use_alloc<_Head, _Alloc, _UHead>(__a),
  std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in)))
 { }
# 703 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _UHead>

 void
 _M_assign(const _Tuple_impl<_Idx, _UHead>& __in)
 {
   _M_head(*this) = _Tuple_impl<_Idx, _UHead>::_M_head(__in);
 }

      template<typename _UHead>

 void
 _M_assign(_Tuple_impl<_Idx, _UHead>&& __in)
 {
   _M_head(*this)
     = std::forward<_UHead>(_Tuple_impl<_Idx, _UHead>::_M_head(__in));
 }
# 749 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    protected:
     
      void
      _M_swap(_Tuple_impl& __in)
      {
 using std::swap;
 swap(_M_head(*this), _M_head(__in));
      }
# 766 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };



  template<bool, typename... _Types>
    struct _TupleConstraints
    {
      template<typename... _UTypes>
 using __constructible = __and_<is_constructible<_Types, _UTypes>...>;

      template<typename... _UTypes>
 using __convertible = __and_<is_convertible<_UTypes, _Types>...>;




      template<typename... _UTypes>
 static constexpr bool __is_implicitly_constructible()
 {
   return __and_<__constructible<_UTypes...>,
   __convertible<_UTypes...>
   >::value;
 }




      template<typename... _UTypes>
 static constexpr bool __is_explicitly_constructible()
 {
   return __and_<__constructible<_UTypes...>,
   __not_<__convertible<_UTypes...>>
   >::value;
 }

      static constexpr bool __is_implicitly_default_constructible()
      {
 return __and_<std::__is_implicitly_default_constructible<_Types>...
        >::value;
      }

      static constexpr bool __is_explicitly_default_constructible()
      {
 return __and_<is_default_constructible<_Types>...,
        __not_<__and_<
   std::__is_implicitly_default_constructible<_Types>...>
        >>::value;
      }
    };



  template<typename... _Types>
    struct _TupleConstraints<false, _Types...>
    {
      template<typename... _UTypes>
 static constexpr bool __is_implicitly_constructible()
 { return false; }

      template<typename... _UTypes>
 static constexpr bool __is_explicitly_constructible()
 { return false; }
    };



  template<typename... _Elements>
    class tuple : public _Tuple_impl<0, _Elements...>
    {
      using _Inherited = _Tuple_impl<0, _Elements...>;
# 1350 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<bool _Cond>
 using _TCC = _TupleConstraints<_Cond, _Elements...>;


      template<bool _Dummy>
 using _ImplicitDefaultCtor = __enable_if_t<
   _TCC<_Dummy>::__is_implicitly_default_constructible(),
   bool>;


      template<bool _Dummy>
 using _ExplicitDefaultCtor = __enable_if_t<
   _TCC<_Dummy>::__is_explicitly_default_constructible(),
   bool>;


      template<bool _Cond, typename... _Args>
 using _ImplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_implicitly_constructible<_Args...>(),
   bool>;


      template<bool _Cond, typename... _Args>
 using _ExplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_explicitly_constructible<_Args...>(),
   bool>;


      template<typename... _UElements>
 static constexpr bool __nothrow_constructible()
 {
   return
     __and_<is_nothrow_constructible<_Elements, _UElements>...>::value;
 }


      template<typename _Up>
 static constexpr bool __valid_args()
 {
   return sizeof...(_Elements) == 1
     && !is_same<tuple, __remove_cvref_t<_Up>>::value;
 }


      template<typename, typename, typename... _Tail>
 static constexpr bool __valid_args()
 { return (sizeof...(_Tail) + 2) == sizeof...(_Elements); }
# 1407 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
      template<typename _Tuple, typename = tuple,
        typename = __remove_cvref_t<_Tuple>>
 struct _UseOtherCtor
 : false_type
 { };


      template<typename _Tuple, typename _Tp, typename _Up>
 struct _UseOtherCtor<_Tuple, tuple<_Tp>, tuple<_Up>>
 : __or_<is_convertible<_Tuple, _Tp>, is_constructible<_Tp, _Tuple>>::type
 { };


      template<typename _Tuple, typename _Tp>
 struct _UseOtherCtor<_Tuple, tuple<_Tp>, tuple<_Tp>>
 : true_type
 { };




      template<typename _Tuple>
 static constexpr bool __use_other_ctor()
 { return _UseOtherCtor<_Tuple>::value; }
# 1453 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    public:
      template<typename _Dummy = void,
        _ImplicitDefaultCtor<is_void<_Dummy>::value> = true>
 constexpr
 tuple()
 noexcept(__and_<is_nothrow_default_constructible<_Elements>...>::value)
 : _Inherited() { }

      template<typename _Dummy = void,
        _ExplicitDefaultCtor<is_void<_Dummy>::value> = false>
 explicit constexpr
 tuple()
 noexcept(__and_<is_nothrow_default_constructible<_Elements>...>::value)
 : _Inherited() { }

      template<bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ImplicitCtor<_NotEmpty, const _Elements&...> = true>
 constexpr
 tuple(const _Elements&... __elements)
 noexcept(__nothrow_constructible<const _Elements&...>())
 : _Inherited(__elements...) { }

      template<bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ExplicitCtor<_NotEmpty, const _Elements&...> = false>
 explicit constexpr
 tuple(const _Elements&... __elements)
 noexcept(__nothrow_constructible<const _Elements&...>())
 : _Inherited(__elements...) { }

      template<typename... _UElements,
        bool _Valid = __valid_args<_UElements...>(),
        _ImplicitCtor<_Valid, _UElements...> = true>
 constexpr
 tuple(_UElements&&... __elements)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(std::forward<_UElements>(__elements)...)
 { ; }

      template<typename... _UElements,
        bool _Valid = __valid_args<_UElements...>(),
        _ExplicitCtor<_Valid, _UElements...> = false>
 explicit constexpr
 tuple(_UElements&&... __elements)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(std::forward<_UElements>(__elements)...)
 { ; }

      constexpr tuple(const tuple&) = default;

      constexpr tuple(tuple&&) = default;

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
      && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ImplicitCtor<_Valid, const _UElements&...> = true>
 constexpr
 tuple(const tuple<_UElements...>& __in)
 noexcept(__nothrow_constructible<const _UElements&...>())
 : _Inherited(static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
      && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ExplicitCtor<_Valid, const _UElements&...> = false>
 explicit constexpr
 tuple(const tuple<_UElements...>& __in)
 noexcept(__nothrow_constructible<const _UElements&...>())
 : _Inherited(static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ImplicitCtor<_Valid, _UElements...> = true>
 constexpr
 tuple(tuple<_UElements...>&& __in)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }

      template<typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ExplicitCtor<_Valid, _UElements...> = false>
 explicit constexpr
 tuple(tuple<_UElements...>&& __in)
 noexcept(__nothrow_constructible<_UElements...>())
 : _Inherited(static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }



      template<typename _Alloc,
        _ImplicitDefaultCtor<is_object<_Alloc>::value> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc,
        _ExplicitDefaultCtor<is_object<_Alloc>::value> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc, bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ImplicitCtor<_NotEmpty, const _Elements&...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _Elements&... __elements)
 : _Inherited(__tag, __a, __elements...) { }

      template<typename _Alloc, bool _NotEmpty = (sizeof...(_Elements) >= 1),
        _ExplicitCtor<_NotEmpty, const _Elements&...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _Elements&... __elements)
 : _Inherited(__tag, __a, __elements...) { }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = __valid_args<_UElements...>(),
        _ImplicitCtor<_Valid, _UElements...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       _UElements&&... __elements)
 : _Inherited(__tag, __a, std::forward<_UElements>(__elements)...)
 { ; }

      template<typename _Alloc, typename... _UElements,
   bool _Valid = __valid_args<_UElements...>(),
        _ExplicitCtor<_Valid, _UElements...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       _UElements&&... __elements)
 : _Inherited(__tag, __a, std::forward<_UElements>(__elements)...)
 { ; }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, const tuple& __in)
 : _Inherited(__tag, __a, static_cast<const _Inherited&>(__in)) { }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple&& __in)
 : _Inherited(__tag, __a, static_cast<_Inherited&&>(__in)) { }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ImplicitCtor<_Valid, const _UElements&...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_UElements...>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<const tuple<_UElements...>&>(),
        _ExplicitCtor<_Valid, const _UElements&...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_UElements...>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _UElements...>&>(__in))
 { ; }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ImplicitCtor<_Valid, _UElements...> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       tuple<_UElements...>&& __in)
 : _Inherited(__tag, __a,
       static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }

      template<typename _Alloc, typename... _UElements,
        bool _Valid = (sizeof...(_Elements) == sizeof...(_UElements))
        && !__use_other_ctor<tuple<_UElements...>&&>(),
        _ExplicitCtor<_Valid, _UElements...> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a,
       tuple<_UElements...>&& __in)
 : _Inherited(__tag, __a,
       static_cast<_Tuple_impl<0, _UElements...>&&>(__in))
 { ; }
# 1885 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    private:
      template<typename... _UElements>
 static constexpr
 __enable_if_t<sizeof...(_UElements) == sizeof...(_Elements), bool>
 __assignable()
 { return __and_<is_assignable<_Elements&, _UElements>...>::value; }


      template<typename... _UElements>
 static constexpr bool __nothrow_assignable()
 {
   return
     __and_<is_nothrow_assignable<_Elements&, _UElements>...>::value;
 }

    public:

     
      tuple&
      operator=(__conditional_t<__assignable<const _Elements&...>(),
    const tuple&,
    const __nonesuch&> __in)
      noexcept(__nothrow_assignable<const _Elements&...>())
      {
 this->_M_assign(__in);
 return *this;
      }

     
      tuple&
      operator=(__conditional_t<__assignable<_Elements...>(),
    tuple&&,
    __nonesuch&&> __in)
      noexcept(__nothrow_assignable<_Elements...>())
      {
 this->_M_assign(std::move(__in));
 return *this;
      }

      template<typename... _UElements>

 __enable_if_t<__assignable<const _UElements&...>(), tuple&>
 operator=(const tuple<_UElements...>& __in)
 noexcept(__nothrow_assignable<const _UElements&...>())
 {
   this->_M_assign(__in);
   return *this;
 }

      template<typename... _UElements>

 __enable_if_t<__assignable<_UElements...>(), tuple&>
 operator=(tuple<_UElements...>&& __in)
 noexcept(__nothrow_assignable<_UElements...>())
 {
   this->_M_assign(std::move(__in));
   return *this;
 }



     
      void
      swap(tuple& __in)
      noexcept(__and_<__is_nothrow_swappable<_Elements>...>::value)
      { _Inherited::_M_swap(__in); }
# 1965 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    };
# 1981 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<>
    class tuple<>
    {
    public:
     
      void swap(tuple&) noexcept { }





      tuple() = default;

      template<typename _Alloc>

 tuple(allocator_arg_t, const _Alloc&) noexcept { }
      template<typename _Alloc>

 tuple(allocator_arg_t, const _Alloc&, const tuple&) noexcept { }
    };




  template<typename _T1, typename _T2>
    class tuple<_T1, _T2> : public _Tuple_impl<0, _T1, _T2>
    {
      typedef _Tuple_impl<0, _T1, _T2> _Inherited;


      template<bool _Dummy, typename _U1, typename _U2>
 using _ImplicitDefaultCtor = __enable_if_t<
   _TupleConstraints<_Dummy, _U1, _U2>::
     __is_implicitly_default_constructible(),
   bool>;


      template<bool _Dummy, typename _U1, typename _U2>
 using _ExplicitDefaultCtor = __enable_if_t<
   _TupleConstraints<_Dummy, _U1, _U2>::
     __is_explicitly_default_constructible(),
   bool>;

      template<bool _Dummy>
 using _TCC = _TupleConstraints<_Dummy, _T1, _T2>;


      template<bool _Cond, typename _U1, typename _U2>
 using _ImplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_implicitly_constructible<_U1, _U2>(),
   bool>;


      template<bool _Cond, typename _U1, typename _U2>
 using _ExplicitCtor = __enable_if_t<
   _TCC<_Cond>::template __is_explicitly_constructible<_U1, _U2>(),
   bool>;

      template<typename _U1, typename _U2>
 static constexpr bool __assignable()
 {
   return __and_<is_assignable<_T1&, _U1>,
   is_assignable<_T2&, _U2>>::value;
 }

      template<typename _U1, typename _U2>
 static constexpr bool __nothrow_assignable()
 {
   return __and_<is_nothrow_assignable<_T1&, _U1>,
   is_nothrow_assignable<_T2&, _U2>>::value;
 }

      template<typename _U1, typename _U2>
 static constexpr bool __nothrow_constructible()
 {
   return __and_<is_nothrow_constructible<_T1, _U1>,
       is_nothrow_constructible<_T2, _U2>>::value;
 }

      static constexpr bool __nothrow_default_constructible()
      {
 return __and_<is_nothrow_default_constructible<_T1>,
        is_nothrow_default_constructible<_T2>>::value;
      }

      template<typename _U1>
 static constexpr bool __is_alloc_arg()
 { return is_same<__remove_cvref_t<_U1>, allocator_arg_t>::value; }
# 2084 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
    public:
      template<bool _Dummy = true,
        _ImplicitDefaultCtor<_Dummy, _T1, _T2> = true>
 constexpr
 tuple()
 noexcept(__nothrow_default_constructible())
 : _Inherited() { }

      template<bool _Dummy = true,
        _ExplicitDefaultCtor<_Dummy, _T1, _T2> = false>
 explicit constexpr
 tuple()
 noexcept(__nothrow_default_constructible())
 : _Inherited() { }

      template<bool _Dummy = true,
        _ImplicitCtor<_Dummy, const _T1&, const _T2&> = true>
 constexpr
 tuple(const _T1& __a1, const _T2& __a2)
 noexcept(__nothrow_constructible<const _T1&, const _T2&>())
 : _Inherited(__a1, __a2) { }

      template<bool _Dummy = true,
        _ExplicitCtor<_Dummy, const _T1&, const _T2&> = false>
 explicit constexpr
 tuple(const _T1& __a1, const _T2& __a2)
 noexcept(__nothrow_constructible<const _T1&, const _T2&>())
 : _Inherited(__a1, __a2) { }

      template<typename _U1, typename _U2,
        _ImplicitCtor<!__is_alloc_arg<_U1>(), _U1, _U2> = true>
 constexpr
 tuple(_U1&& __a1, _U2&& __a2)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__a1), std::forward<_U2>(__a2))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<!__is_alloc_arg<_U1>(), _U1, _U2> = false>
 explicit constexpr
 tuple(_U1&& __a1, _U2&& __a2)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__a1), std::forward<_U2>(__a2))
 { ; }

      constexpr tuple(const tuple&) = default;

      constexpr tuple(tuple&&) = default;

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>
 constexpr
 tuple(const tuple<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit constexpr
 tuple(const tuple<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>
 constexpr
 tuple(tuple<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit constexpr
 tuple(tuple<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>
 constexpr
 tuple(const pair<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(__in.first, __in.second)
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit constexpr
 tuple(const pair<_U1, _U2>& __in)
 noexcept(__nothrow_constructible<const _U1&, const _U2&>())
 : _Inherited(__in.first, __in.second)
 { ; }

      template<typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>
 constexpr
 tuple(pair<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }

      template<typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit constexpr
 tuple(pair<_U1, _U2>&& __in)
 noexcept(__nothrow_constructible<_U1, _U2>())
 : _Inherited(std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }



      template<typename _Alloc,
        _ImplicitDefaultCtor<is_object<_Alloc>::value, _T1, _T2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc,
        _ExplicitDefaultCtor<is_object<_Alloc>::value, _T1, _T2> = false>

 explicit
 tuple(allocator_arg_t __tag, const _Alloc& __a)
 : _Inherited(__tag, __a) { }

      template<typename _Alloc, bool _Dummy = true,
        _ImplicitCtor<_Dummy, const _T1&, const _T2&> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _T1& __a1, const _T2& __a2)
 : _Inherited(__tag, __a, __a1, __a2) { }

      template<typename _Alloc, bool _Dummy = true,
        _ExplicitCtor<_Dummy, const _T1&, const _T2&> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const _T1& __a1, const _T2& __a2)
 : _Inherited(__tag, __a, __a1, __a2) { }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a, _U1&& __a1, _U2&& __a2)
 : _Inherited(__tag, __a, std::forward<_U1>(__a1),
       std::forward<_U2>(__a2))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       _U1&& __a1, _U2&& __a2)
 : _Inherited(__tag, __a, std::forward<_U1>(__a1),
       std::forward<_U2>(__a2))
 { ; }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, const tuple& __in)
 : _Inherited(__tag, __a, static_cast<const _Inherited&>(__in)) { }

      template<typename _Alloc>

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple&& __in)
 : _Inherited(__tag, __a, static_cast<_Inherited&&>(__in)) { }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_U1, _U2>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const tuple<_U1, _U2>& __in)
 : _Inherited(__tag, __a,
       static_cast<const _Tuple_impl<0, _U1, _U2>&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a, tuple<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, static_cast<_Tuple_impl<0, _U1, _U2>&&>(__in))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, const _U1&, const _U2&> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const pair<_U1, _U2>& __in)
 : _Inherited(__tag, __a, __in.first, __in.second)
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, const _U1&, const _U2&> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a,
       const pair<_U1, _U2>& __in)
 : _Inherited(__tag, __a, __in.first, __in.second)
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ImplicitCtor<true, _U1, _U2> = true>

 tuple(allocator_arg_t __tag, const _Alloc& __a, pair<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }

      template<typename _Alloc, typename _U1, typename _U2,
        _ExplicitCtor<true, _U1, _U2> = false>
 explicit

 tuple(allocator_arg_t __tag, const _Alloc& __a, pair<_U1, _U2>&& __in)
 : _Inherited(__tag, __a, std::forward<_U1>(__in.first),
       std::forward<_U2>(__in.second))
 { ; }



     
      tuple&
      operator=(__conditional_t<__assignable<const _T1&, const _T2&>(),
    const tuple&,
    const __nonesuch&> __in)
      noexcept(__nothrow_assignable<const _T1&, const _T2&>())
      {
 this->_M_assign(__in);
 return *this;
      }

     
      tuple&
      operator=(__conditional_t<__assignable<_T1, _T2>(),
    tuple&&,
    __nonesuch&&> __in)
      noexcept(__nothrow_assignable<_T1, _T2>())
      {
 this->_M_assign(std::move(__in));
 return *this;
      }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<const _U1&, const _U2&>(), tuple&>
 operator=(const tuple<_U1, _U2>& __in)
 noexcept(__nothrow_assignable<const _U1&, const _U2&>())
 {
   this->_M_assign(__in);
   return *this;
 }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<_U1, _U2>(), tuple&>
 operator=(tuple<_U1, _U2>&& __in)
 noexcept(__nothrow_assignable<_U1, _U2>())
 {
   this->_M_assign(std::move(__in));
   return *this;
 }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<const _U1&, const _U2&>(), tuple&>
 operator=(const pair<_U1, _U2>& __in)
 noexcept(__nothrow_assignable<const _U1&, const _U2&>())
 {
   this->_M_head(*this) = __in.first;
   this->_M_tail(*this)._M_head(*this) = __in.second;
   return *this;
 }

      template<typename _U1, typename _U2>

 __enable_if_t<__assignable<_U1, _U2>(), tuple&>
 operator=(pair<_U1, _U2>&& __in)
 noexcept(__nothrow_assignable<_U1, _U2>())
 {
   this->_M_head(*this) = std::forward<_U1>(__in.first);
   this->_M_tail(*this)._M_head(*this) = std::forward<_U2>(__in.second);
   return *this;
 }

     
      void
      swap(tuple& __in)
      noexcept(__and_<__is_nothrow_swappable<_T1>,
        __is_nothrow_swappable<_T2>>::value)
      { _Inherited::_M_swap(__in); }
    };



  template<typename... _Elements>
    struct tuple_size<tuple<_Elements...>>
    : public integral_constant<size_t, sizeof...(_Elements)> { };
# 2416 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<size_t __i, typename... _Types>
    struct tuple_element<__i, tuple<_Types...>>
    {
      static_assert(__i < sizeof...(_Types), "tuple index must be in range");

      using type = typename _Nth_type<__i, _Types...>::type;
    };

  template<size_t __i, typename _Head, typename... _Tail>
    constexpr _Head&
    __get_helper(_Tuple_impl<__i, _Head, _Tail...>& __t) noexcept
    { return _Tuple_impl<__i, _Head, _Tail...>::_M_head(__t); }

  template<size_t __i, typename _Head, typename... _Tail>
    constexpr const _Head&
    __get_helper(const _Tuple_impl<__i, _Head, _Tail...>& __t) noexcept
    { return _Tuple_impl<__i, _Head, _Tail...>::_M_head(__t); }


  template<size_t __i, typename... _Types>
    __enable_if_t<(__i >= sizeof...(_Types))>
    __get_helper(const tuple<_Types...>&) = delete;


  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&
    get(tuple<_Elements...>& __t) noexcept
    { return std::__get_helper<__i>(__t); }


  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&
    get(const tuple<_Elements...>& __t) noexcept
    { return std::__get_helper<__i>(__t); }


  template<size_t __i, typename... _Elements>
    constexpr __tuple_element_t<__i, tuple<_Elements...>>&&
    get(tuple<_Elements...>&& __t) noexcept
    {
      typedef __tuple_element_t<__i, tuple<_Elements...>> __element_type;
      return std::forward<__element_type>(std::__get_helper<__i>(__t));
    }


  template<size_t __i, typename... _Elements>
    constexpr const __tuple_element_t<__i, tuple<_Elements...>>&&
    get(const tuple<_Elements...>&& __t) noexcept
    {
      typedef __tuple_element_t<__i, tuple<_Elements...>> __element_type;
      return std::forward<const __element_type>(std::__get_helper<__i>(__t));
    }



  template<size_t __i, typename... _Elements>
    constexpr __enable_if_t<(__i >= sizeof...(_Elements))>
    get(const tuple<_Elements...>&) = delete;
# 2576 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<typename _Tp, typename _Up, size_t __i, size_t __size>
    struct __tuple_compare
    {
      static constexpr bool
      __eq(const _Tp& __t, const _Up& __u)
      {
 return bool(std::get<__i>(__t) == std::get<__i>(__u))
   && __tuple_compare<_Tp, _Up, __i + 1, __size>::__eq(__t, __u);
      }

      static constexpr bool
      __less(const _Tp& __t, const _Up& __u)
      {
 return bool(std::get<__i>(__t) < std::get<__i>(__u))
   || (!bool(std::get<__i>(__u) < std::get<__i>(__t))
       && __tuple_compare<_Tp, _Up, __i + 1, __size>::__less(__t, __u));
      }
    };

  template<typename _Tp, typename _Up, size_t __size>
    struct __tuple_compare<_Tp, _Up, __size, __size>
    {
      static constexpr bool
      __eq(const _Tp&, const _Up&) { return true; }

      static constexpr bool
      __less(const _Tp&, const _Up&) { return false; }
    };

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator==(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    {
      static_assert(sizeof...(_TElements) == sizeof...(_UElements),
   "tuple objects can only be compared if they have equal sizes.");
      using __compare = __tuple_compare<tuple<_TElements...>,
     tuple<_UElements...>,
     0, sizeof...(_TElements)>;
      return __compare::__eq(__t, __u);
    }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator<(const tuple<_TElements...>& __t,
       const tuple<_UElements...>& __u)
    {
      static_assert(sizeof...(_TElements) == sizeof...(_UElements),
   "tuple objects can only be compared if they have equal sizes.");
      using __compare = __tuple_compare<tuple<_TElements...>,
     tuple<_UElements...>,
     0, sizeof...(_TElements)>;
      return __compare::__less(__t, __u);
    }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator!=(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    { return !(__t == __u); }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator>(const tuple<_TElements...>& __t,
       const tuple<_UElements...>& __u)
    { return __u < __t; }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator<=(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    { return !(__u < __t); }

  template<typename... _TElements, typename... _UElements>
    [[__nodiscard__]]
    constexpr bool
    operator>=(const tuple<_TElements...>& __t,
        const tuple<_UElements...>& __u)
    { return !(__t < __u); }




  template<typename... _Elements>
    constexpr tuple<typename __decay_and_strip<_Elements>::__type...>
    make_tuple(_Elements&&... __args)
    {
      typedef tuple<typename __decay_and_strip<_Elements>::__type...>
 __result_type;
      return __result_type(std::forward<_Elements>(__args)...);
    }




  template<typename... _Elements>
    constexpr tuple<_Elements&&...>
    forward_as_tuple(_Elements&&... __args) noexcept
    { return tuple<_Elements&&...>(std::forward<_Elements>(__args)...); }


  template<size_t, typename, typename, size_t>
    struct __make_tuple_impl;

  template<size_t _Idx, typename _Tuple, typename... _Tp, size_t _Nm>
    struct __make_tuple_impl<_Idx, tuple<_Tp...>, _Tuple, _Nm>
    : __make_tuple_impl<_Idx + 1,
   tuple<_Tp..., __tuple_element_t<_Idx, _Tuple>>,
   _Tuple, _Nm>
    { };

  template<size_t _Nm, typename _Tuple, typename... _Tp>
    struct __make_tuple_impl<_Nm, tuple<_Tp...>, _Tuple, _Nm>
    {
      typedef tuple<_Tp...> __type;
    };

  template<typename _Tuple>
    struct __do_make_tuple
    : __make_tuple_impl<0, tuple<>, _Tuple, tuple_size<_Tuple>::value>
    { };


  template<typename _Tuple>
    struct __make_tuple
    : public __do_make_tuple<__remove_cvref_t<_Tuple>>
    { };


  template<typename...>
    struct __combine_tuples;

  template<>
    struct __combine_tuples<>
    {
      typedef tuple<> __type;
    };

  template<typename... _Ts>
    struct __combine_tuples<tuple<_Ts...>>
    {
      typedef tuple<_Ts...> __type;
    };

  template<typename... _T1s, typename... _T2s, typename... _Rem>
    struct __combine_tuples<tuple<_T1s...>, tuple<_T2s...>, _Rem...>
    {
      typedef typename __combine_tuples<tuple<_T1s..., _T2s...>,
     _Rem...>::__type __type;
    };


  template<typename... _Tpls>
    struct __tuple_cat_result
    {
      typedef typename __combine_tuples
        <typename __make_tuple<_Tpls>::__type...>::__type __type;
    };



  template<typename...>
    struct __make_1st_indices;

  template<>
    struct __make_1st_indices<>
    {
      typedef _Index_tuple<> __type;
    };

  template<typename _Tp, typename... _Tpls>
    struct __make_1st_indices<_Tp, _Tpls...>
    {
      typedef typename _Build_index_tuple<tuple_size<
 typename remove_reference<_Tp>::type>::value>::__type __type;
    };




  template<typename _Ret, typename _Indices, typename... _Tpls>
    struct __tuple_concater;

  template<typename _Ret, size_t... _Is, typename _Tp, typename... _Tpls>
    struct __tuple_concater<_Ret, _Index_tuple<_Is...>, _Tp, _Tpls...>
    {
      template<typename... _Us>
        static constexpr _Ret
        _S_do(_Tp&& __tp, _Tpls&&... __tps, _Us&&... __us)
        {
   typedef typename __make_1st_indices<_Tpls...>::__type __idx;
   typedef __tuple_concater<_Ret, __idx, _Tpls...> __next;
   return __next::_S_do(std::forward<_Tpls>(__tps)...,
          std::forward<_Us>(__us)...,
          std::get<_Is>(std::forward<_Tp>(__tp))...);
 }
    };

  template<typename _Ret>
    struct __tuple_concater<_Ret, _Index_tuple<>>
    {
      template<typename... _Us>
 static constexpr _Ret
 _S_do(_Us&&... __us)
        {
   return _Ret(std::forward<_Us>(__us)...);
 }
    };

  template<typename... _Tps>
    struct __is_tuple_like_impl<tuple<_Tps...>> : true_type
    { };






  template<typename... _Tpls, typename = typename
           enable_if<__and_<__is_tuple_like<_Tpls>...>::value>::type>

    constexpr auto
    tuple_cat(_Tpls&&... __tpls)
    -> typename __tuple_cat_result<_Tpls...>::__type
    {
      typedef typename __tuple_cat_result<_Tpls...>::__type __ret;
      typedef typename __make_1st_indices<_Tpls...>::__type __idx;
      typedef __tuple_concater<__ret, __idx, _Tpls...> __concater;
      return __concater::_S_do(std::forward<_Tpls>(__tpls)...);
    }




  template<typename... _Elements>
    constexpr tuple<_Elements&...>
    tie(_Elements&... __args) noexcept
    { return tuple<_Elements&...>(__args...); }


  template<typename... _Elements>
   
    inline





    void

    swap(tuple<_Elements...>& __x, tuple<_Elements...>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }
# 2853 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<typename... _Types, typename _Alloc>
    struct uses_allocator<tuple<_Types...>, _Alloc> : true_type { };
# 2865 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3
  template<class _T1, class _T2>
    template<typename... _Args1, typename... _Args2>
     
      inline
      pair<_T1, _T2>::
      pair(piecewise_construct_t,
    tuple<_Args1...> __first, tuple<_Args2...> __second)
      : pair(__first, __second,
      typename _Build_index_tuple<sizeof...(_Args1)>::__type(),
      typename _Build_index_tuple<sizeof...(_Args2)>::__type())
      { }

  template<class _T1, class _T2>
    template<typename... _Args1, size_t... _Indexes1,
      typename... _Args2, size_t... _Indexes2>
      inline
      pair<_T1, _T2>::
      pair(tuple<_Args1...>& __tuple1, tuple<_Args2...>& __tuple2,
    _Index_tuple<_Indexes1...>, _Index_tuple<_Indexes2...>)
      : first(std::forward<_Args1>(std::get<_Indexes1>(__tuple1))...),
 second(std::forward<_Args2>(std::get<_Indexes2>(__tuple2))...)
      { }
# 3027 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/tuple" 3

}
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 2 3


namespace std __attribute__ ((__visibility__ ("default")))
{



  template <typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    class multimap;
# 100 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
  template <typename _Key, typename _Tp, typename _Compare = std::less<_Key>,
     typename _Alloc = std::allocator<std::pair<const _Key, _Tp> > >
    class map
    {
    public:
      typedef _Key key_type;
      typedef _Tp mapped_type;
      typedef std::pair<const _Key, _Tp> value_type;
      typedef _Compare key_compare;
      typedef _Alloc allocator_type;

    private:
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      static_assert(is_same<typename _Alloc::value_type, value_type>::value,
   "std::map must have the same value_type as its allocator");



    public:
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
      class value_compare
      : public std::binary_function<value_type, value_type, bool>
      {
 friend class map<_Key, _Tp, _Compare, _Alloc>;
      protected:
 _Compare comp;

 value_compare(_Compare __c)
 : comp(__c) { }

      public:
 bool operator()(const value_type& __x, const value_type& __y) const
 { return comp(__x.first, __y.first); }
      };
#pragma GCC diagnostic pop

    private:

      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
 rebind<value_type>::other _Pair_alloc_type;

      typedef _Rb_tree<key_type, value_type, _Select1st<value_type>,
         key_compare, _Pair_alloc_type> _Rep_type;


      _Rep_type _M_t;

      typedef __gnu_cxx::__alloc_traits<_Pair_alloc_type> _Alloc_traits;
# 169 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
    public:


      typedef typename _Alloc_traits::pointer pointer;
      typedef typename _Alloc_traits::const_pointer const_pointer;
      typedef typename _Alloc_traits::reference reference;
      typedef typename _Alloc_traits::const_reference const_reference;
      typedef typename _Rep_type::iterator iterator;
      typedef typename _Rep_type::const_iterator const_iterator;
      typedef typename _Rep_type::size_type size_type;
      typedef typename _Rep_type::difference_type difference_type;
      typedef typename _Rep_type::reverse_iterator reverse_iterator;
      typedef typename _Rep_type::const_reverse_iterator const_reverse_iterator;
# 197 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map() = default;







      explicit
      map(const _Compare& __comp,
   const allocator_type& __a = allocator_type())
      : _M_t(__comp, _Pair_alloc_type(__a)) { }
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map(const map&) = default;







      map(map&&) = default;
# 240 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map(initializer_list<value_type> __l,
   const _Compare& __comp = _Compare(),
   const allocator_type& __a = allocator_type())
      : _M_t(__comp, _Pair_alloc_type(__a))
      { _M_t._M_insert_range_unique(__l.begin(), __l.end()); }


      explicit
      map(const allocator_type& __a)
      : _M_t(_Pair_alloc_type(__a)) { }


      map(const map& __m, const __type_identity_t<allocator_type>& __a)
      : _M_t(__m._M_t, _Pair_alloc_type(__a)) { }


      map(map&& __m, const __type_identity_t<allocator_type>& __a)
      noexcept(is_nothrow_copy_constructible<_Compare>::value
        && _Alloc_traits::_S_always_equal())
      : _M_t(std::move(__m._M_t), _Pair_alloc_type(__a)) { }


      map(initializer_list<value_type> __l, const allocator_type& __a)
      : _M_t(_Pair_alloc_type(__a))
      { _M_t._M_insert_range_unique(__l.begin(), __l.end()); }


      template<typename _InputIterator>
 map(_InputIterator __first, _InputIterator __last,
     const allocator_type& __a)
 : _M_t(_Pair_alloc_type(__a))
 { _M_t._M_insert_range_unique(__first, __last); }
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename _InputIterator>
 map(_InputIterator __first, _InputIterator __last)
 : _M_t()
 { _M_t._M_insert_range_unique(__first, __last); }
# 301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename _InputIterator>
 map(_InputIterator __first, _InputIterator __last,
     const _Compare& __comp,
     const allocator_type& __a = allocator_type())
 : _M_t(__comp, _Pair_alloc_type(__a))
 { _M_t._M_insert_range_unique(__first, __last); }







      ~map() = default;
# 330 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map&
      operator=(const map&) = default;


      map&
      operator=(map&&) = default;
# 348 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map&
      operator=(initializer_list<value_type> __l)
      {
 _M_t._M_assign_unique(__l.begin(), __l.end());
 return *this;
      }



      allocator_type
      get_allocator() const noexcept
      { return allocator_type(_M_t.get_allocator()); }







      iterator
      begin() noexcept
      { return _M_t.begin(); }






      const_iterator
      begin() const noexcept
      { return _M_t.begin(); }






      iterator
      end() noexcept
      { return _M_t.end(); }






      const_iterator
      end() const noexcept
      { return _M_t.end(); }






      reverse_iterator
      rbegin() noexcept
      { return _M_t.rbegin(); }






      const_reverse_iterator
      rbegin() const noexcept
      { return _M_t.rbegin(); }






      reverse_iterator
      rend() noexcept
      { return _M_t.rend(); }






      const_reverse_iterator
      rend() const noexcept
      { return _M_t.rend(); }







      const_iterator
      cbegin() const noexcept
      { return _M_t.begin(); }






      const_iterator
      cend() const noexcept
      { return _M_t.end(); }






      const_reverse_iterator
      crbegin() const noexcept
      { return _M_t.rbegin(); }






      const_reverse_iterator
      crend() const noexcept
      { return _M_t.rend(); }






      [[__nodiscard__]] bool
      empty() const noexcept
      { return _M_t.empty(); }


      size_type
      size() const noexcept
      { return _M_t.size(); }


      size_type
      max_size() const noexcept
      { return _M_t.max_size(); }
# 503 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      mapped_type&
      operator[](const key_type& __k)
      {



 iterator __i = lower_bound(__k);

 if (__i == end() || key_comp()(__k, (*__i).first))

   __i = _M_t._M_emplace_hint_unique(__i, std::piecewise_construct,
         std::tuple<const key_type&>(__k),
         std::tuple<>());



 return (*__i).second;
      }


      mapped_type&
      operator[](key_type&& __k)
      {



 iterator __i = lower_bound(__k);

 if (__i == end() || key_comp()(__k, (*__i).first))
   __i = _M_t._M_emplace_hint_unique(__i, std::piecewise_construct,
     std::forward_as_tuple(std::move(__k)),
     std::tuple<>());
 return (*__i).second;
      }
# 548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      mapped_type&
      at(const key_type& __k)
      {
 iterator __i = lower_bound(__k);
 if (__i == end() || key_comp()(__k, (*__i).first))
   __throw_out_of_range(("map::at"));
 return (*__i).second;
      }

      const mapped_type&
      at(const key_type& __k) const
      {
 const_iterator __i = lower_bound(__k);
 if (__i == end() || key_comp()(__k, (*__i).first))
   __throw_out_of_range(("map::at"));
 return (*__i).second;
      }
# 586 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename... _Args>
 std::pair<iterator, bool>
 emplace(_Args&&... __args)
 {
# 608 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
   return _M_t._M_emplace_unique(std::forward<_Args>(__args)...);
 }
# 636 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename... _Args>
 iterator
 emplace_hint(const_iterator __pos, _Args&&... __args)
 {
   return _M_t._M_emplace_hint_unique(__pos,
          std::forward<_Args>(__args)...);
 }
# 833 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      std::pair<iterator, bool>
      insert(const value_type& __x)
      { return _M_t._M_insert_unique(__x); }




      std::pair<iterator, bool>
      insert(value_type&& __x)
      { return _M_t._M_insert_unique(std::move(__x)); }

      template<typename _Pair>
 __enable_if_t<is_constructible<value_type, _Pair>::value,
        pair<iterator, bool>>
 insert(_Pair&& __x)
 {
# 865 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
   return _M_t._M_emplace_unique(std::forward<_Pair>(__x));
 }
# 878 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      void
      insert(std::initializer_list<value_type> __list)
      { insert(__list.begin(), __list.end()); }
# 907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator

      insert(const_iterator __position, const value_type& __x)



      { return _M_t._M_insert_unique_(__position, __x); }




      iterator
      insert(const_iterator __position, value_type&& __x)
      { return _M_t._M_insert_unique_(__position, std::move(__x)); }

      template<typename _Pair>
 __enable_if_t<is_constructible<value_type, _Pair>::value, iterator>
 insert(const_iterator __position, _Pair&& __x)
 {
   return _M_t._M_emplace_hint_unique(__position,
          std::forward<_Pair>(__x));
 }
# 940 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename _InputIterator>
 void
 insert(_InputIterator __first, _InputIterator __last)
 { _M_t._M_insert_range_unique(__first, __last); }
# 1079 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator
      erase(const_iterator __position)
      { return _M_t.erase(__position); }


      __attribute ((__abi_tag__ ("cxx11")))
      iterator
      erase(iterator __position)
      { return _M_t.erase(__position); }
# 1116 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      size_type
      erase(const key_type& __x)
      { return _M_t.erase(__x); }
# 1136 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator
      erase(const_iterator __first, const_iterator __last)
      { return _M_t.erase(__first, __last); }
# 1170 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      void
      swap(map& __x)
      noexcept(__is_nothrow_swappable<_Compare>::value)
      { _M_t.swap(__x._M_t); }







      void
      clear() noexcept
      { _M_t.clear(); }






      key_compare
      key_comp() const
      { return _M_t.key_comp(); }





      value_compare
      value_comp() const
      { return value_compare(_M_t.key_comp()); }
# 1217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator
      find(const key_type& __x)
      { return _M_t.find(__x); }
# 1242 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      const_iterator
      find(const key_type& __x) const
      { return _M_t.find(__x); }
# 1263 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      size_type
      count(const key_type& __x) const
      { return _M_t.find(__x) == _M_t.end() ? 0 : 1; }
# 1306 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator
      lower_bound(const key_type& __x)
      { return _M_t.lower_bound(__x); }
# 1331 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      const_iterator
      lower_bound(const key_type& __x) const
      { return _M_t.lower_bound(__x); }
# 1351 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator
      upper_bound(const key_type& __x)
      { return _M_t.upper_bound(__x); }
# 1371 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      const_iterator
      upper_bound(const key_type& __x) const
      { return _M_t.upper_bound(__x); }
# 1400 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      std::pair<iterator, iterator>
      equal_range(const key_type& __x)
      { return _M_t.equal_range(__x); }
# 1429 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      std::pair<const_iterator, const_iterator>
      equal_range(const key_type& __x) const
      { return _M_t.equal_range(__x); }
# 1446 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename _K1, typename _T1, typename _C1, typename _A1>
 friend bool
 operator==(const map<_K1, _T1, _C1, _A1>&,
     const map<_K1, _T1, _C1, _A1>&);







      template<typename _K1, typename _T1, typename _C1, typename _A1>
 friend bool
 operator<(const map<_K1, _T1, _C1, _A1>&,
    const map<_K1, _T1, _C1, _A1>&);

    };
# 1510 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator==(const map<_Key, _Tp, _Compare, _Alloc>& __x,
        const map<_Key, _Tp, _Compare, _Alloc>& __y)
    { return __x._M_t == __y._M_t; }
# 1548 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator<(const map<_Key, _Tp, _Compare, _Alloc>& __x,
       const map<_Key, _Tp, _Compare, _Alloc>& __y)
    { return __x._M_t < __y._M_t; }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator!=(const map<_Key, _Tp, _Compare, _Alloc>& __x,
        const map<_Key, _Tp, _Compare, _Alloc>& __y)
    { return !(__x == __y); }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator>(const map<_Key, _Tp, _Compare, _Alloc>& __x,
       const map<_Key, _Tp, _Compare, _Alloc>& __y)
    { return __y < __x; }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator<=(const map<_Key, _Tp, _Compare, _Alloc>& __x,
        const map<_Key, _Tp, _Compare, _Alloc>& __y)
    { return !(__y < __x); }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator>=(const map<_Key, _Tp, _Compare, _Alloc>& __x,
        const map<_Key, _Tp, _Compare, _Alloc>& __y)
    { return !(__x < __y); }



  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline void
    swap(map<_Key, _Tp, _Compare, _Alloc>& __x,
  map<_Key, _Tp, _Compare, _Alloc>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }


# 1614 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3

}
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 1 3
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  template <typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    class map;
# 98 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
  template <typename _Key, typename _Tp,
     typename _Compare = std::less<_Key>,
     typename _Alloc = std::allocator<std::pair<const _Key, _Tp> > >
    class multimap
    {
    public:
      typedef _Key key_type;
      typedef _Tp mapped_type;
      typedef std::pair<const _Key, _Tp> value_type;
      typedef _Compare key_compare;
      typedef _Alloc allocator_type;

    private:
# 124 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      static_assert(is_same<typename _Alloc::value_type, value_type>::value,
   "std::multimap must have the same value_type as its allocator");



    public:
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"
      class value_compare
      : public std::binary_function<value_type, value_type, bool>
      {
 friend class multimap<_Key, _Tp, _Compare, _Alloc>;
      protected:
 _Compare comp;

 value_compare(_Compare __c)
 : comp(__c) { }

      public:
 bool operator()(const value_type& __x, const value_type& __y) const
 { return comp(__x.first, __y.first); }
      };
#pragma GCC diagnostic pop

    private:

      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
 rebind<value_type>::other _Pair_alloc_type;

      typedef _Rb_tree<key_type, value_type, _Select1st<value_type>,
         key_compare, _Pair_alloc_type> _Rep_type;

      _Rep_type _M_t;

      typedef __gnu_cxx::__alloc_traits<_Pair_alloc_type> _Alloc_traits;

    public:


      typedef typename _Alloc_traits::pointer pointer;
      typedef typename _Alloc_traits::const_pointer const_pointer;
      typedef typename _Alloc_traits::reference reference;
      typedef typename _Alloc_traits::const_reference const_reference;
      typedef typename _Rep_type::iterator iterator;
      typedef typename _Rep_type::const_iterator const_iterator;
      typedef typename _Rep_type::size_type size_type;
      typedef typename _Rep_type::difference_type difference_type;
      typedef typename _Rep_type::reverse_iterator reverse_iterator;
      typedef typename _Rep_type::const_reverse_iterator const_reverse_iterator;
# 187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap() = default;







      explicit
      multimap(const _Compare& __comp,
        const allocator_type& __a = allocator_type())
      : _M_t(__comp, _Pair_alloc_type(__a)) { }
# 209 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap(const multimap&) = default;
# 218 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap(multimap&&) = default;
# 230 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap(initializer_list<value_type> __l,
        const _Compare& __comp = _Compare(),
        const allocator_type& __a = allocator_type())
      : _M_t(__comp, _Pair_alloc_type(__a))
      { _M_t._M_insert_range_equal(__l.begin(), __l.end()); }


      explicit
      multimap(const allocator_type& __a)
      : _M_t(_Pair_alloc_type(__a)) { }


      multimap(const multimap& __m,
        const __type_identity_t<allocator_type>& __a)
      : _M_t(__m._M_t, _Pair_alloc_type(__a)) { }


      multimap(multimap&& __m, const __type_identity_t<allocator_type>& __a)
      noexcept(is_nothrow_copy_constructible<_Compare>::value
        && _Alloc_traits::_S_always_equal())
      : _M_t(std::move(__m._M_t), _Pair_alloc_type(__a)) { }


      multimap(initializer_list<value_type> __l, const allocator_type& __a)
      : _M_t(_Pair_alloc_type(__a))
      { _M_t._M_insert_range_equal(__l.begin(), __l.end()); }


      template<typename _InputIterator>
 multimap(_InputIterator __first, _InputIterator __last,
   const allocator_type& __a)
 : _M_t(_Pair_alloc_type(__a))
 { _M_t._M_insert_range_equal(__first, __last); }
# 274 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename _InputIterator>
 multimap(_InputIterator __first, _InputIterator __last)
 : _M_t()
 { _M_t._M_insert_range_equal(__first, __last); }
# 290 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename _InputIterator>
 multimap(_InputIterator __first, _InputIterator __last,
   const _Compare& __comp,
   const allocator_type& __a = allocator_type())
 : _M_t(__comp, _Pair_alloc_type(__a))
 { _M_t._M_insert_range_equal(__first, __last); }







      ~multimap() = default;
# 319 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap&
      operator=(const multimap&) = default;


      multimap&
      operator=(multimap&&) = default;
# 337 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap&
      operator=(initializer_list<value_type> __l)
      {
 _M_t._M_assign_equal(__l.begin(), __l.end());
 return *this;
      }



      allocator_type
      get_allocator() const noexcept
      { return allocator_type(_M_t.get_allocator()); }







      iterator
      begin() noexcept
      { return _M_t.begin(); }






      const_iterator
      begin() const noexcept
      { return _M_t.begin(); }






      iterator
      end() noexcept
      { return _M_t.end(); }






      const_iterator
      end() const noexcept
      { return _M_t.end(); }






      reverse_iterator
      rbegin() noexcept
      { return _M_t.rbegin(); }






      const_reverse_iterator
      rbegin() const noexcept
      { return _M_t.rbegin(); }






      reverse_iterator
      rend() noexcept
      { return _M_t.rend(); }






      const_reverse_iterator
      rend() const noexcept
      { return _M_t.rend(); }







      const_iterator
      cbegin() const noexcept
      { return _M_t.begin(); }






      const_iterator
      cend() const noexcept
      { return _M_t.end(); }






      const_reverse_iterator
      crbegin() const noexcept
      { return _M_t.rbegin(); }






      const_reverse_iterator
      crend() const noexcept
      { return _M_t.rend(); }




      [[__nodiscard__]] bool
      empty() const noexcept
      { return _M_t.empty(); }


      size_type
      size() const noexcept
      { return _M_t.size(); }


      size_type
      max_size() const noexcept
      { return _M_t.max_size(); }
# 495 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename... _Args>
 iterator
 emplace(_Args&&... __args)
 { return _M_t._M_emplace_equal(std::forward<_Args>(__args)...); }
# 522 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename... _Args>
 iterator
 emplace_hint(const_iterator __pos, _Args&&... __args)
 {
   return _M_t._M_emplace_hint_equal(__pos,
         std::forward<_Args>(__args)...);
 }
# 544 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      insert(const value_type& __x)
      { return _M_t._M_insert_equal(__x); }




      iterator
      insert(value_type&& __x)
      { return _M_t._M_insert_equal(std::move(__x)); }

      template<typename _Pair>
 __enable_if_t<is_constructible<value_type, _Pair>::value, iterator>
 insert(_Pair&& __x)
 { return _M_t._M_emplace_equal(std::forward<_Pair>(__x)); }
# 583 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator

      insert(const_iterator __position, const value_type& __x)



      { return _M_t._M_insert_equal_(__position, __x); }




      iterator
      insert(const_iterator __position, value_type&& __x)
      { return _M_t._M_insert_equal_(__position, std::move(__x)); }

      template<typename _Pair>
 __enable_if_t<is_constructible<value_type, _Pair&&>::value, iterator>
 insert(const_iterator __position, _Pair&& __x)
 {
   return _M_t._M_emplace_hint_equal(__position,
         std::forward<_Pair>(__x));
 }
# 617 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename _InputIterator>
 void
 insert(_InputIterator __first, _InputIterator __last)
 { _M_t._M_insert_range_equal(__first, __last); }
# 630 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      void
      insert(initializer_list<value_type> __l)
      { this->insert(__l.begin(), __l.end()); }
# 707 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      erase(const_iterator __position)
      { return _M_t.erase(__position); }


      __attribute ((__abi_tag__ ("cxx11")))
      iterator
      erase(iterator __position)
      { return _M_t.erase(__position); }
# 744 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      size_type
      erase(const key_type& __x)
      { return _M_t.erase(__x); }
# 765 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      erase(const_iterator __first, const_iterator __last)
      { return _M_t.erase(__first, __last); }
# 802 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      void
      swap(multimap& __x)
      noexcept(__is_nothrow_swappable<_Compare>::value)
      { _M_t.swap(__x._M_t); }







      void
      clear() noexcept
      { _M_t.clear(); }






      key_compare
      key_comp() const
      { return _M_t.key_comp(); }





      value_compare
      value_comp() const
      { return value_compare(_M_t.key_comp()); }
# 848 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      find(const key_type& __x)
      { return _M_t.find(__x); }
# 872 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      const_iterator
      find(const key_type& __x) const
      { return _M_t.find(__x); }
# 890 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      size_type
      count(const key_type& __x) const
      { return _M_t.count(__x); }
# 933 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      lower_bound(const key_type& __x)
      { return _M_t.lower_bound(__x); }
# 958 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      const_iterator
      lower_bound(const key_type& __x) const
      { return _M_t.lower_bound(__x); }
# 978 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      upper_bound(const key_type& __x)
      { return _M_t.upper_bound(__x); }
# 998 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      const_iterator
      upper_bound(const key_type& __x) const
      { return _M_t.upper_bound(__x); }
# 1025 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      std::pair<iterator, iterator>
      equal_range(const key_type& __x)
      { return _M_t.equal_range(__x); }
# 1052 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      std::pair<const_iterator, const_iterator>
      equal_range(const key_type& __x) const
      { return _M_t.equal_range(__x); }
# 1069 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename _K1, typename _T1, typename _C1, typename _A1>
 friend bool
 operator==(const multimap<_K1, _T1, _C1, _A1>&,
     const multimap<_K1, _T1, _C1, _A1>&);







      template<typename _K1, typename _T1, typename _C1, typename _A1>
 friend bool
 operator<(const multimap<_K1, _T1, _C1, _A1>&,
    const multimap<_K1, _T1, _C1, _A1>&);

  };
# 1132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator==(const multimap<_Key, _Tp, _Compare, _Alloc>& __x,
        const multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    { return __x._M_t == __y._M_t; }
# 1170 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator<(const multimap<_Key, _Tp, _Compare, _Alloc>& __x,
       const multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    { return __x._M_t < __y._M_t; }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator!=(const multimap<_Key, _Tp, _Compare, _Alloc>& __x,
        const multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    { return !(__x == __y); }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator>(const multimap<_Key, _Tp, _Compare, _Alloc>& __x,
       const multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    { return __y < __x; }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator<=(const multimap<_Key, _Tp, _Compare, _Alloc>& __x,
        const multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    { return !(__y < __x); }


  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline bool
    operator>=(const multimap<_Key, _Tp, _Compare, _Alloc>& __x,
        const multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    { return !(__x < __y); }



  template<typename _Key, typename _Tp, typename _Compare, typename _Alloc>
    inline void
    swap(multimap<_Key, _Tp, _Compare, _Alloc>& __x,
  multimap<_Key, _Tp, _Compare, _Alloc>& __y)
    noexcept(noexcept(__x.swap(__y)))
    { __x.swap(__y); }


# 1236 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3

}
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
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
    inline _Tp*
    begin(_Tp (&__arr)[_Nm]) noexcept
    { return __arr; }






  template<typename _Tp, size_t _Nm>
    [[__nodiscard__, __gnu__::__always_inline__]]
    inline _Tp*
    end(_Tp (&__arr)[_Nm]) noexcept
    { return __arr + _Nm; }
# 371 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/range_access.h" 3

}
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/erase_if.h" 1 3
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 81 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 5 "./diagnostic/missing-header-pr110164.C" 2


# 6 "./diagnostic/missing-header-pr110164.C"
std::array<int, 10> a1;


std::array<int, 10> a2 {5};
