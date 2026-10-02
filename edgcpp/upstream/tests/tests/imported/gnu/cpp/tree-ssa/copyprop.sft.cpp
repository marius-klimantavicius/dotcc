//type: fp
//options:  --no_exceptions --c++03
# 0 "./tree-ssa/copyprop.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./tree-ssa/copyprop.C"







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





#pragma GCC visibility push(default)


  extern "C++" __attribute__ ((__noreturn__, __always_inline__))
  inline void __terminate() throw()
  {
    void terminate() throw() __attribute__ ((__noreturn__,__cold__));
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
  inline bool
  __is_constant_evaluated()
  {







    return false;

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
  ;
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
# 191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cpp_type_traits.h" 3
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
   
    inline bool
    __is_null_pointer(_Type* __ptr)
    { return __ptr == 0; }

  template<typename _Type>
   
    inline bool
    __is_null_pointer(_Type)
    { return false; }
# 175 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/type_traits.h" 3
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
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 260 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _U1, typename _U2> class __pair_base
  {







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
# 973 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
      pair() : first(), second() { }


      pair(const _T1& __a, const _T2& __b)
      : first(__a), second(__b) { }


      template<typename _U1, typename _U2>
 pair(const pair<_U1, _U2>& __p)
 : first(__p.first), second(__p.second)
 {

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wunused-local-typedefs"
   typedef int _DanglingCheck1[
     __reference_constructs_from_temporary(_T1, const _U1&) ? -1 : 1
        ];
   typedef int _DanglingCheck2[
     __reference_constructs_from_temporary(_T2, const _U2&) ? -1 : 1
        ];
#pragma GCC diagnostic pop

 }

    };
# 1039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
   
    inline bool
    operator==(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first == __y.first && __x.second == __y.second; }
# 1052 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
   
    inline bool
    operator<(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __x.first < __y.first
      || (!(__y.first < __x.first) && __x.second < __y.second); }


  template<typename _T1, typename _T2>
   
    inline bool
    operator!=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x == __y); }


  template<typename _T1, typename _T2>
   
    inline bool
    operator>(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return __y < __x; }


  template<typename _T1, typename _T2>
   
    inline bool
    operator<=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__y < __x); }


  template<typename _T1, typename _T2>
   
    inline bool
    operator>=(const pair<_T1, _T2>& __x, const pair<_T1, _T2>& __y)
    { return !(__x < __y); }
# 1154 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_pair.h" 3
  template<typename _T1, typename _T2>
    inline pair<_T1, _T2>
    make_pair(_T1 __x, _T2 __y)
    { return pair<_T1, _T2>(__x, __y); }
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
# 183 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _Iterator>
    struct iterator_traits
    {
      typedef typename _Iterator::iterator_category iterator_category;
      typedef typename _Iterator::value_type value_type;
      typedef typename _Iterator::difference_type difference_type;
      typedef typename _Iterator::pointer pointer;
      typedef typename _Iterator::reference reference;
    };
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
    inline
    typename iterator_traits<_Iter>::iterator_category
    __iterator_category(const _Iter&)
    { return typename iterator_traits<_Iter>::iterator_category(); }
# 271 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_types.h" 3
  template<typename _It, typename _Traits = iterator_traits<_It>,
    typename _Cat = typename _Traits::iterator_category>
    struct __is_random_access_iter
    { enum { __value = __is_base_of(random_access_iterator_tag, _Cat) }; };



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
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator>
    __attribute__((__always_inline__))
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
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3
  template<typename _InputIterator, typename _Distance>
    __attribute__((__always_inline__))
    inline void
    advance(_InputIterator& __i, _Distance __n)
    {

      typename iterator_traits<_InputIterator>::difference_type __d = __n;
      std::__advance(__i, __d, std::__iterator_category(__i));
    }
# 258 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator_base_funcs.h" 3

}
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 1 3
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{







  template<typename _Tp>
    __attribute__((__always_inline__))
    inline _Tp*
    __addressof(_Tp& __r)
    { return __builtin_addressof(__r); }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/move.h" 3
  template<typename _Tp>
   
    inline





    void

    swap(_Tp& __a, _Tp& __b)
   

    {


     

      _Tp __tmp = (__a);
      __a = (__b);
      __b = (__tmp);
    }




  template<typename _Tp, size_t _Nm>
   
    inline



    void

    swap(_Tp (&__a)[_Nm], _Tp (&__b)[_Nm])
   
    {
      for (size_t __n = 0; __n < _Nm; ++__n)
 swap(__a[__n], __b[__n]);
    }



}
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ptr_traits.h" 1 3
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
     
      : current()
      { }




      explicit
      reverse_iterator(iterator_type __x)
     
      : current(__x)
      { }




     
      reverse_iterator(const reverse_iterator& __x)
     
      : current(__x.current)
      { }
# 212 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      template<typename _Iter>




        reverse_iterator(const reverse_iterator<_Iter>& __x)

 : current(__x.current)
 { }
# 241 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      iterator_type
      base() const
     
      { return current; }
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
      reference
      operator*() const
      {
 _Iterator __tmp = current;
 return *--__tmp;
      }






     
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






     
      reverse_iterator
      operator+(difference_type __n) const
      { return reverse_iterator(current - __n); }







      reverse_iterator&
      operator+=(difference_type __n)
      {
 current -= __n;
 return *this;
      }






     
      reverse_iterator
      operator-(difference_type __n) const
      { return reverse_iterator(current + __n); }







      reverse_iterator&
      operator-=(difference_type __n)
      {
 current += __n;
 return *this;
      }






     
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
   
    inline bool
    operator==(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return __x.base() == __y.base(); }

  template<typename _Iterator>
   
    inline bool
    operator<(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y.base() < __x.base(); }

  template<typename _Iterator>
   
    inline bool
    operator!=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x == __y); }

  template<typename _Iterator>
   
    inline bool
    operator>(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y < __x; }

  template<typename _Iterator>
   
    inline bool
    operator<=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__y < __x); }

  template<typename _Iterator>
   
    inline bool
    operator>=(const reverse_iterator<_Iterator>& __x,
        const reverse_iterator<_Iterator>& __y)
    { return !(__x < __y); }




  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator==(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() == __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator<(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __x.base() > __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
    inline bool
    operator!=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() != __y.base(); }

  template<typename _IteratorL, typename _IteratorR>
   
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
   
    inline bool
    operator>=(const reverse_iterator<_IteratorL>& __x,
        const reverse_iterator<_IteratorR>& __y)
    { return __x.base() <= __y.base(); }
# 603 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
    inline typename reverse_iterator<_Iterator>::difference_type
    operator-(const reverse_iterator<_Iterator>& __x,
       const reverse_iterator<_Iterator>& __y)
    { return __y.base() - __x.base(); }

  template<typename _IteratorL, typename _IteratorR>
    inline typename reverse_iterator<_IteratorL>::difference_type
    operator-(const reverse_iterator<_IteratorL>& __x,
       const reverse_iterator<_IteratorR>& __y)
    { return __y.base() - __x.base(); }
# 626 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _Iterator>
   
    inline reverse_iterator<_Iterator>
    operator+(typename reverse_iterator<_Iterator>::difference_type __n,
       const reverse_iterator<_Iterator>& __x)
    { return reverse_iterator<_Iterator>(__x.base() - __n); }
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
# 707 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      back_insert_iterator&
      operator=(typename _Container::const_reference __value)
      {
 container->push_back(__value);
 return *this;
      }
# 732 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
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
# 808 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      front_insert_iterator&
      operator=(typename _Container::const_reference __value)
      {
 container->push_front(__value);
 return *this;
      }
# 833 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
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
# 935 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      insert_iterator&
      operator=(typename _Container::const_reference __value)
      {
 iter = container->insert(iter, __value);
 ++iter;
 return *this;
      }
# 963 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
     
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







    public:
      typedef _Iterator iterator_type;
      typedef typename __traits_type::iterator_category iterator_category;
      typedef typename __traits_type::value_type value_type;
      typedef typename __traits_type::difference_type difference_type;
      typedef typename __traits_type::reference reference;
      typedef typename __traits_type::pointer pointer;





      __attribute__((__always_inline__))
     
      __normal_iterator()
      : _M_current() { }

      __attribute__((__always_inline__))
      explicit
      __normal_iterator(const _Iterator& __i)
      : _M_current(__i) { }
# 1075 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
      template<typename _Iter>
 __attribute__((__always_inline__))
 __normal_iterator(const __normal_iterator<_Iter,
     typename __enable_if<
        (std::__are_same<_Iter, typename _Container::pointer>::__value),
        _Container>::__type>& __i)

        : _M_current(__i.base()) { }



      __attribute__((__always_inline__))
     
      reference
      operator*() const
      { return *_M_current; }

      __attribute__((__always_inline__))
     
      pointer
      operator->() const
      { return _M_current; }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator++()
      {
 ++_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator++(int)
      { return __normal_iterator(_M_current++); }



      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator--()
      {
 --_M_current;
 return *this;
      }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator--(int)
      { return __normal_iterator(_M_current--); }



      __attribute__((__always_inline__))
     
      reference
      operator[](difference_type __n) const
      { return _M_current[__n]; }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator+=(difference_type __n)
      { _M_current += __n; return *this; }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator+(difference_type __n) const
      { return __normal_iterator(_M_current + __n); }

      __attribute__((__always_inline__))
     
      __normal_iterator&
      operator-=(difference_type __n)
      { _M_current -= __n; return *this; }

      __attribute__((__always_inline__))
     
      __normal_iterator
      operator-(difference_type __n) const
      { return __normal_iterator(_M_current - __n); }

      __attribute__((__always_inline__))
     
      const _Iterator&
      base() const
      { return _M_current; }
    };
# 1217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator==(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() == __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator==(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() == __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() != __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() != __rhs.base(); }


  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() < __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() < __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() > __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() > __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() <= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator<=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() <= __rhs.base(); }

  template<typename _IteratorL, typename _IteratorR, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>=(const __normal_iterator<_IteratorL, _Container>& __lhs,
        const __normal_iterator<_IteratorR, _Container>& __rhs)
   
    { return __lhs.base() >= __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline bool
    operator>=(const __normal_iterator<_Iterator, _Container>& __lhs,
        const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() >= __rhs.base(); }






  template<typename _IteratorL, typename _IteratorR, typename _Container>
# 1328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
    inline typename __normal_iterator<_IteratorL, _Container>::difference_type
    operator-(const __normal_iterator<_IteratorL, _Container>& __lhs,
       const __normal_iterator<_IteratorR, _Container>& __rhs)

    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline typename __normal_iterator<_Iterator, _Container>::difference_type
    operator-(const __normal_iterator<_Iterator, _Container>& __lhs,
       const __normal_iterator<_Iterator, _Container>& __rhs)
   
    { return __lhs.base() - __rhs.base(); }

  template<typename _Iterator, typename _Container>
    __attribute__((__always_inline__))
    inline __normal_iterator<_Iterator, _Container>
    operator+(typename __normal_iterator<_Iterator, _Container>::difference_type
       __n, const __normal_iterator<_Iterator, _Container>& __i)
   
    { return __normal_iterator<_Iterator, _Container>(__i.base() + __n); }


}

namespace std __attribute__ ((__visibility__ ("default")))
{

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
    __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(__gnu_cxx::__normal_iterator<_Iterator, _Container> __it)
   
    { return __it.base(); }


  template<typename _Iterator>
    __attribute__((__always_inline__))
   
    inline _Iterator
    __niter_base(_Iterator __it)
   
    { return __it; }




  template<typename _Ite, typename _Seq>
    _Ite
    __niter_base(const ::__gnu_debug::_Safe_iterator<_Ite, _Seq,
   std::random_access_iterator_tag>&);

 template<typename _Ite, typename _Cont, typename _Seq>
    _Ite
    __niter_base(const ::__gnu_debug::_Safe_iterator<
   ::__gnu_cxx::__normal_iterator<_Ite, _Cont>, _Seq,
   std::random_access_iterator_tag>&);
# 3069 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_iterator.h" 3
  template<typename _From, typename _To>
   
   
    inline _From
    __niter_wrap(_From __from, _To __res)
    { return __from + (std::__niter_base(__res) - std::__niter_base(__from)); }


  template<typename _Iterator>
    __attribute__((__always_inline__))
   
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



    _Iter_less_val() { }


   
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



    _Val_less_iter() { }


   
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
 : _M_comp((__comp))
      { }

      template<typename _Iterator1, typename _Iterator2>
       
        bool
        operator()(_Iterator1 __it1, _Iterator2 __it2)
        { return bool(_M_comp(*__it1, *__it2)); }
    };

  template<typename _Compare>
   
    inline _Iter_comp_iter<_Compare>
    __iter_comp_iter(_Compare __comp)
    { return _Iter_comp_iter<_Compare>((__comp)); }

  template<typename _Compare>
    struct _Iter_comp_val
    {
      _Compare _M_comp;

     
      explicit
      _Iter_comp_val(_Compare __comp)
 : _M_comp((__comp))
      { }

     
      explicit
      _Iter_comp_val(const _Iter_comp_iter<_Compare>& __comp)
 : _M_comp(__comp._M_comp)
      { }
# 192 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/predefined_ops.h" 3
      template<typename _Iterator, typename _Value>

 bool
 operator()(_Iterator __it, _Value& __val)
 { return bool(_M_comp(*__it, __val)); }
    };

  template<typename _Compare>
   
    inline _Iter_comp_val<_Compare>
    __iter_comp_val(_Compare __comp)
    { return _Iter_comp_val<_Compare>((__comp)); }

  template<typename _Compare>
   
    inline _Iter_comp_val<_Compare>
    __iter_comp_val(_Iter_comp_iter<_Compare> __comp)
    { return _Iter_comp_val<_Compare>((__comp)); }

  template<typename _Compare>
    struct _Val_comp_iter
    {
      _Compare _M_comp;

     
      explicit
      _Val_comp_iter(_Compare __comp)
 : _M_comp((__comp))
      { }

     
      explicit
      _Val_comp_iter(const _Iter_comp_iter<_Compare>& __comp)
 : _M_comp(__comp._M_comp)
      { }
# 236 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/predefined_ops.h" 3
      template<typename _Value, typename _Iterator>

 bool
 operator()(_Value& __val, _Iterator __it)
 { return bool(_M_comp(__val, *__it)); }
    };

  template<typename _Compare>
   
    inline _Val_comp_iter<_Compare>
    __val_comp_iter(_Compare __comp)
    { return _Val_comp_iter<_Compare>((__comp)); }

  template<typename _Compare>
   
    inline _Val_comp_iter<_Compare>
    __val_comp_iter(_Iter_comp_iter<_Compare> __comp)
    { return _Val_comp_iter<_Compare>((__comp)); }

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
 : _M_pred((__pred))
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return bool(_M_pred(*__it)); }
    };

  template<typename _Predicate>
   
    inline _Iter_pred<_Predicate>
    __pred_iter(_Predicate __pred)
    { return _Iter_pred<_Predicate>((__pred)); }

  template<typename _Compare, typename _Value>
    struct _Iter_comp_to_val
    {
      _Compare _M_comp;
      _Value& _M_value;

     
      _Iter_comp_to_val(_Compare __comp, _Value& __value)
 : _M_comp((__comp)), _M_value(__value)
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
      return _Iter_comp_to_val<_Compare, _Value>((__comp), __val);
    }

  template<typename _Compare, typename _Iterator1>
    struct _Iter_comp_to_iter
    {
      _Compare _M_comp;
      _Iterator1 _M_it1;

     
      _Iter_comp_to_iter(_Compare __comp, _Iterator1 __it1)
 : _M_comp((__comp)), _M_it1(__it1)
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
   (__comp._M_comp), __it);
    }

  template<typename _Predicate>
    struct _Iter_negate
    {
      _Predicate _M_pred;

     
      explicit
      _Iter_negate(_Predicate __pred)
 : _M_pred((__pred))
      { }

      template<typename _Iterator>

 bool
 operator()(_Iterator __it)
 { return !bool(_M_pred(*__it)); }
    };

  template<typename _Predicate>
   
    inline _Iter_negate<_Predicate>
    __negate(_Iter_pred<_Predicate> __pred)
    { return _Iter_negate<_Predicate>((__pred._M_pred)); }

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
# 109 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
 return __builtin_memcmp(__first1, __first2, sizeof(_Tp) * __num);
    }





  template<bool _BoolType>
    struct __iter_swap
    {
      template<typename _ForwardIterator1, typename _ForwardIterator2>
 static void
 iter_swap(_ForwardIterator1 __a, _ForwardIterator2 __b)
 {
   typedef typename iterator_traits<_ForwardIterator1>::value_type
     _ValueType1;
   _ValueType1 __tmp = *__a;
   *__a = *__b;
   *__b = __tmp;
 }
    };

  template<>
    struct __iter_swap<true>
    {
      template<typename _ForwardIterator1, typename _ForwardIterator2>
 static void
 iter_swap(_ForwardIterator1 __a, _ForwardIterator2 __b)
 {
   swap(*__a, *__b);
 }
    };
# 153 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _ForwardIterator1, typename _ForwardIterator2>
   
    inline void
    iter_swap(_ForwardIterator1 __a, _ForwardIterator2 __b)
    {

     

     



      typedef typename iterator_traits<_ForwardIterator1>::value_type
 _ValueType1;
      typedef typename iterator_traits<_ForwardIterator2>::value_type
 _ValueType2;

     

     


      typedef typename iterator_traits<_ForwardIterator1>::reference
 _ReferenceType1;
      typedef typename iterator_traits<_ForwardIterator2>::reference
 _ReferenceType2;
      std::__iter_swap<__are_same<_ValueType1, _ValueType2>::__value
 && __are_same<_ValueType1&, _ReferenceType1>::__value
 && __are_same<_ValueType2&, _ReferenceType2>::__value>::
 iter_swap(__a, __b);





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
   
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b)
    {

     

      if (__b < __a)
 return __b;
      return __a;
    }
# 255 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp>
   
    inline const _Tp&
    max(const _Tp& __a, const _Tp& __b)
    {

     

      if (__a < __b)
 return __b;
      return __a;
    }
# 279 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
   
    inline const _Tp&
    min(const _Tp& __a, const _Tp& __b, _Compare __comp)
    {

      if (__comp(__b, __a))
 return __b;
      return __a;
    }
# 301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Tp, typename _Compare>
   
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
# 391 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _Iter> __attribute__((__always_inline__))
    inline void* __ptr_or_null(_Iter) { return 0; }
  template<typename _Tp> __attribute__((__always_inline__))
  inline void* __ptr_or_null(_Tp* __p) { return (void*)__p; }


  template<typename _Iter> __attribute__((__always_inline__))
    inline void __ptr_advance(_Iter&, ptrdiff_t) { }
  template<typename _Tp> __attribute__((__always_inline__))
    inline void __ptr_advance(_Tp*& __p, ptrdiff_t __n) { __p += __n; }
# 409 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<bool _IsMove, typename _OutIter, typename _InIter>
    __attribute__((__always_inline__))
    inline void
    __assign_one(_OutIter& __out, _InIter& __in)
    {





 *__out = *__in;
    }

  template<bool _IsMove, typename _InIter, typename _Sent, typename _OutIter>
   
    inline _OutIter
    __copy_move_a2(_InIter __first, _Sent __last, _OutIter __result)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if (!__is_trivially_assignable(_OutRef, _InRef))
 { }
      else if (std::__is_constant_evaluated())
 { }
      else if (__memcpyable<_OutIter, _InIter>::__value)
 {
   ptrdiff_t __n = std::distance(__first, __last);
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(std::__ptr_or_null(__result),
    std::__ptr_or_null(__first),
    __n * sizeof(*__first));
       std::__ptr_advance(__result, __n);
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
      if (!__is_trivially_assignable(_OutRef, _InRef))
 { }




      else if (__memcpyable<_OutputIterator,
            _InputIterator>::__value)
 {
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(std::__ptr_or_null(__result),
    std::__ptr_or_null(__first),
    __n * sizeof(*__first));
       std::__ptr_advance(__result, __n);
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
# 695 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<bool _IsMove, typename _BI1, typename _BI2>
   
    inline _BI2
    __copy_move_backward_a2(_BI1 __first, _BI1 __last, _BI2 __result)
    {
      typedef __decltype(*__first) _InRef;
      typedef __decltype(*__result) _OutRef;
      if (!__is_trivially_assignable(_OutRef, _InRef))
       { }




      else if (__memcpyable<_BI2, _BI1>::__value)
 {
   ptrdiff_t __n = std::distance(__first, __last);
   std::advance(__result, -__n);
   if (__builtin_expect(__n > 1, true))
     {
       __builtin_memmove(std::__ptr_or_null(__result),
    std::__ptr_or_null(__first),
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
# 903 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
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

  inline int
  __size_to_integer(int __n) { return __n; }
  inline unsigned
  __size_to_integer(unsigned __n) { return __n; }
  inline long
  __size_to_integer(long __n) { return __n; }
  inline unsigned long
  __size_to_integer(unsigned long __n) { return __n; }
  inline long long
  __size_to_integer(long long __n) { return __n; }
  inline unsigned long long
  __size_to_integer(unsigned long long __n) { return __n; }
# 1065 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  inline long long
  __size_to_integer(float __n) { return (long long)__n; }
  inline long long
  __size_to_integer(double __n) { return (long long)__n; }
  inline long long
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



      return __fill_n_a1(__first, __n, __value);
    }

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::input_iterator_tag)
    {



      return __fill_n_a1(__first, __n, __value);
    }

  template<typename _OutputIterator, typename _Size, typename _Tp>
    __attribute__((__always_inline__))
   
    inline _OutputIterator
    __fill_n_a(_OutputIterator __first, _Size __n, const _Tp& __value,
        std::random_access_iterator_tag)
    {



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
   
    inline _ForwardIterator
    lower_bound(_ForwardIterator __first, _ForwardIterator __last,
  const _Tp& __val)
    {

     
     

      ;

      return std::__lower_bound(__first, __last, __val,
    __gnu_cxx::__ops::__iter_less_val());
    }



  template<typename _Tp>
    inline _Tp
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
   
    inline bool
    equal(_II1 __first1, _II1 __last1, _II2 __first2)
    {

     
     
     


      ;

      return std::__equal_aux(__first1, __last1, __first2);
    }
# 1625 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _IIter1, typename _IIter2, typename _BinaryPredicate>
   
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
# 1781 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3
  template<typename _II1, typename _II2>
   
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
     *__result = (*__first);
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
# 2259 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_algobase.h" 3

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
    exception() throw() { }
    virtual ~exception() throw();
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/exception.h" 3
    virtual const char*
    what() const throw();
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
# 78 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
    virtual ~bad_alloc() throw();


    virtual const char* what() const throw();
  };
# 103 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
  struct nothrow_t
  {



  };

  extern const nothrow_t nothrow;



  typedef void (*new_handler)();



  new_handler set_new_handler(new_handler) throw();





}
# 137 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
 void* operator new(std::size_t)
  throw(std::bad_alloc)
  __attribute__((__externally_visible__, __malloc__));
 void* operator new[](std::size_t)
  throw(std::bad_alloc)
  __attribute__((__externally_visible__, __malloc__));
void operator delete(void*) throw()
  __attribute__((__externally_visible__));
void operator delete[](void*) throw()
  __attribute__((__externally_visible__));
# 155 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
 void* operator new(std::size_t, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
 void* operator new[](std::size_t, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__, __alloc_size__ (1), __malloc__));
void operator delete(void*, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__));
void operator delete[](void*, const std::nothrow_t&)
  throw()
  __attribute__((__externally_visible__));
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/new" 3
 inline
void* operator new(std::size_t, void* __p)
  throw()
{ return __p; }
 inline
void* operator new[](std::size_t, void* __p)
  throw()
{ return __p; }




inline void operator delete (void*, void*)
  throw()
{ }
inline void operator delete[](void*, void*)
  throw()
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
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      __attribute__((__always_inline__))
     
      __new_allocator() throw() { }

      __attribute__((__always_inline__))
     
      __new_allocator(const __new_allocator&) throw() { }

      template<typename _Tp1>
 __attribute__((__always_inline__))

 __new_allocator(const __new_allocator<_Tp1>&) throw() { }






      ~__new_allocator() throw() { }

      pointer
      address(reference __x) const
      { return std::__addressof(__x); }

      const_pointer
      address(const_reference __x) const
      { return std::__addressof(__x); }
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      _Tp*
      allocate(size_type __n, const void* = static_cast<const void*>(0))
      {






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
      max_size() const throw()
      { return _M_max_size(); }
# 202 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/new_allocator.h" 3
      __attribute__((__always_inline__))
      void
      construct(pointer __p, const _Tp& __val)
      { ::new((void *)__p) _Tp(__val); }

      __attribute__((__always_inline__))
      void
      destroy(pointer __p) { __p->~_Tp(); }



      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator==(const __new_allocator&, const __new_allocator<_Up>&)
 throw()
 { return true; }


      template<typename _Up>
 friend __attribute__((__always_inline__)) bool
 operator!=(const __new_allocator&, const __new_allocator<_Up>&)
 throw()
 { return false; }


    private:
      __attribute__((__always_inline__))
      size_type
      _M_max_size() const throw()
      {

 return std::size_t(0x7fffffffffffffffL) / sizeof(_Tp);



      }
    };


}
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++allocator.h" 2 3
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
# 83 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/memoryfwd.h" 3

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
# 120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
    };
# 132 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
  template<typename _Tp>
    class allocator : public __new_allocator<_Tp>
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
# 166 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
      __attribute__((__always_inline__))
     
      allocator() throw() { }

      __attribute__((__always_inline__))
     
      allocator(const allocator& __a) throw()
      : __new_allocator<_Tp>(__a) { }






      template<typename _Tp1>
 __attribute__((__always_inline__))

 allocator(const allocator<_Tp1>&) throw() { }

      __attribute__((__always_inline__))



      ~allocator() throw() { }
# 219 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/allocator.h" 3
      friend __attribute__((__always_inline__))
      bool
      operator==(const allocator&, const allocator&) throw()
      { return true; }


      friend __attribute__((__always_inline__))
      bool
      operator!=(const allocator&, const allocator&) throw()
      { return false; }



    };






  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator==(const allocator<_T1>&, const allocator<_T2>&)
    throw()
    { return true; }


  template<typename _T1, typename _T2>
    __attribute__((__always_inline__))
    inline bool
    operator!=(const allocator<_T1>&, const allocator<_T2>&)
    throw()
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
    } ;





  template<typename _Arg1, typename _Arg2, typename _Result>
    struct binary_function
    {

      typedef _Arg1 first_argument_type;


      typedef _Arg2 second_argument_type;


      typedef _Result result_type;
    } ;
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
      operator()(_Tp* __x, _Tp* __y) const throw()
      {




 return (long unsigned int)__x > (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const throw()
      {




 return (long unsigned int)__x < (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct greater_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const throw()
      {




 return (long unsigned int)__x >= (long unsigned int)__y;
      }
    };


  template<typename _Tp>
    struct less_equal<_Tp*> : public binary_function<_Tp*, _Tp*, bool>
    {
      bool
      operator()(_Tp* __x, _Tp* __y) const throw()
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
    } ;


  template<typename _Arg, typename _Result>
   
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
    } ;


  template<typename _Arg1, typename _Arg2, typename _Result>
   
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
# 1193 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_function.h" 3
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
    } ;


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
    } ;


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
    } ;


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
    } ;


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
    } ;


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
    } ;


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
    } ;


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
    } ;



  template<typename _Ret, typename _Tp>
   
    inline mem_fun_t<_Ret, _Tp>
    mem_fun(_Ret (_Tp::*__f)())
    { return mem_fun_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
   
    inline const_mem_fun_t<_Ret, _Tp>
    mem_fun(_Ret (_Tp::*__f)() const)
    { return const_mem_fun_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
   
    inline mem_fun_ref_t<_Ret, _Tp>
    mem_fun_ref(_Ret (_Tp::*__f)())
    { return mem_fun_ref_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp>
   
    inline const_mem_fun_ref_t<_Ret, _Tp>
    mem_fun_ref(_Ret (_Tp::*__f)() const)
    { return const_mem_fun_ref_t<_Ret, _Tp>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
   
    inline mem_fun1_t<_Ret, _Tp, _Arg>
    mem_fun(_Ret (_Tp::*__f)(_Arg))
    { return mem_fun1_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
   
    inline const_mem_fun1_t<_Ret, _Tp, _Arg>
    mem_fun(_Ret (_Tp::*__f)(_Arg) const)
    { return const_mem_fun1_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
   
    inline mem_fun1_ref_t<_Ret, _Tp, _Arg>
    mem_fun_ref(_Ret (_Tp::*__f)(_Arg))
    { return mem_fun1_ref_t<_Ret, _Tp, _Arg>(__f); }

  template<typename _Ret, typename _Tp, typename _Arg>
   
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
    } ;


  template<typename _Operation, typename _Tp>
   
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
    } ;


  template<typename _Operation, typename _Tp>
   
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

# 136 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
  template<typename _T1, typename _T2>
    inline void
    _Construct(_T1* __p, const _T2& __value)
    {


      ::new(static_cast<void*>(__p)) _T1(__value);
    }


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


  template<bool>
    struct _Destroy_aux
    {
      template<typename _ForwardIterator>
 static void
 __destroy(_ForwardIterator __first, _ForwardIterator __last)
 {
   for (; __first != __last; ++__first)
     std::_Destroy(std::__addressof(*__first));
 }
    };

  template<>
    struct _Destroy_aux<true>
    {
      template<typename _ForwardIterator>
        static void
        __destroy(_ForwardIterator, _ForwardIterator) { }
    };







  template<typename _ForwardIterator>
    inline void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;
# 218 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
      std::_Destroy_aux<__has_trivial_destructor(_Value_type)>::
 __destroy(__first, __last);

    }


  template<bool>
    struct _Destroy_n_aux
    {
      template<typename _ForwardIterator, typename _Size>
 static _ForwardIterator
 __destroy_n(_ForwardIterator __first, _Size __count)
 {
   for (; __count > 0; (void)++__first, --__count)
     std::_Destroy(std::__addressof(*__first));
   return __first;
 }
    };

  template<>
    struct _Destroy_n_aux<true>
    {
      template<typename _ForwardIterator, typename _Size>
        static _ForwardIterator
        __destroy_n(_ForwardIterator __first, _Size __count)
 {
   std::advance(__first, __count);
   return __first;
 }
    };







  template<typename _ForwardIterator, typename _Size>
    inline _ForwardIterator
    _Destroy_n(_ForwardIterator __first, _Size __count)
    {
      typedef typename iterator_traits<_ForwardIterator>::value_type
                       _Value_type;
# 277 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3
      return std::_Destroy_n_aux<__has_trivial_destructor(_Value_type)>::
 __destroy_n(__first, __count);

    }
#pragma GCC diagnostic pop
# 299 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_construct.h" 3

}
# 34 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 2 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 970 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _Alloc, bool = __is_empty(_Alloc)>
    struct __alloc_swap
    { static void _S_do_it(_Alloc&, _Alloc&) { } };

  template<typename _Alloc>
    struct __alloc_swap<_Alloc, false>
    {
      static void
      _S_do_it(_Alloc& __one, _Alloc& __two)
      {

 if (__one != __two)
   swap(__one, __two);
      }
    };
# 1023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/alloc_traits.h" 3
  template<typename _ForwardIterator, typename _Allocator>
   
    void
    _Destroy(_ForwardIterator __first, _ForwardIterator __last,
      _Allocator& __alloc)
    {
      for (; __first != __last; ++__first)

 __alloc.destroy(std::__addressof(*__first));




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



  {
    typedef _Alloc allocator_type;
# 131 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/alloc_traits.h" 3
    typedef typename _Alloc::pointer pointer;
    typedef typename _Alloc::const_pointer const_pointer;
    typedef typename _Alloc::value_type value_type;
    typedef typename _Alloc::reference reference;
    typedef typename _Alloc::const_reference const_reference;
    typedef typename _Alloc::size_type size_type;
    typedef typename _Alloc::difference_type difference_type;

    __attribute__((__always_inline__))
    static pointer
    allocate(_Alloc& __a, size_type __n)
    { return __a.allocate(__n); }

    template<typename _Hint>
      __attribute__((__always_inline__))
      static pointer
      allocate(_Alloc& __a, size_type __n, _Hint __hint)
      { return __a.allocate(__n, __hint); }

    __attribute__((__always_inline__))
    static void deallocate(_Alloc& __a, pointer __p, size_type __n)
    { __a.deallocate(__p, __n); }

    template<typename _Tp>
      __attribute__((__always_inline__))
      static void construct(_Alloc& __a, pointer __p, const _Tp& __arg)
      { __a.construct(__p, __arg); }

    __attribute__((__always_inline__))
    static void destroy(_Alloc& __a, pointer __p)
    { __a.destroy(__p); }

    __attribute__((__always_inline__))
    static size_type max_size(const _Alloc& __a)
    { return __a.max_size(); }

    __attribute__((__always_inline__))
    static const _Alloc& _S_select_on_copy(const _Alloc& __a) { return __a; }

    __attribute__((__always_inline__))
    static void _S_on_swap(_Alloc& __a, _Alloc& __b)
    {


      std::__alloc_swap<_Alloc>::_S_do_it(__a, __b);
    }

    template<typename _Tp>
      struct rebind
      { typedef typename _Alloc::template rebind<_Tp>::other other; };

  };


}
# 71 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 2 3
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
    _S_minimum(_Base_ptr __x)
    {
      while (__x->_M_left != 0) __x = __x->_M_left;
      return __x;
    }

    static _Base_ptr
    _S_maximum(_Base_ptr __x)
    {
      while (__x->_M_right != 0) __x = __x->_M_right;
      return __x;
    }




    _Base_ptr
    _M_base_ptr() const
    { return const_cast<_Rb_tree_node_base*>(this); }
  };


  template<typename _Key_compare>
    struct _Rb_tree_key_compare
    {
      _Key_compare _M_key_compare;

      _Rb_tree_key_compare()
     

      : _M_key_compare()
      { }

      _Rb_tree_key_compare(const _Key_compare& __comp)
      : _M_key_compare(__comp)
      { }
# 163 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
    };


  struct _Rb_tree_header
  {
    _Rb_tree_node_base _M_header;
    size_t _M_node_count;

    _Rb_tree_header()
    {
      _M_header._M_color = _S_red;
      _M_reset();
    }
# 190 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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

      _Val _M_value_field;

      _Val*
      _M_valptr()
      { return std::__addressof(_M_value_field); }

      const _Val*
      _M_valptr() const
      { return std::__addressof(_M_value_field); }
# 238 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      _Rb_tree_node*
      _M_node_ptr()
      { return this; }
    };
# 365 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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

      _Rb_tree_iterator()
      : _M_node() { }

      explicit
      _Rb_tree_iterator(_Base_ptr __x)
      : _M_node(__x) { }

      reference
      operator*() const
      { return *static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      pointer
      operator->() const
      { return static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      _Rb_tree_iterator&
      operator++()
      {
 _M_node = _Rb_tree_increment(_M_node);
 return *this;
      }

      _Rb_tree_iterator
      operator++(int)
      {
 _Rb_tree_iterator __tmp = *this;
 _M_node = _Rb_tree_increment(_M_node);
 return __tmp;
      }

      _Rb_tree_iterator&
      operator--()
      {
 _M_node = _Rb_tree_decrement(_M_node);
 return *this;
      }

      _Rb_tree_iterator
      operator--(int)
      {
 _Rb_tree_iterator __tmp = *this;
 _M_node = _Rb_tree_decrement(_M_node);
 return __tmp;
      }

      friend bool
      operator==(const _Rb_tree_iterator& __x,
   const _Rb_tree_iterator& __y)
      { return __x._M_node == __y._M_node; }


      friend bool
      operator!=(const _Rb_tree_iterator& __x,
   const _Rb_tree_iterator& __y)
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

      _Rb_tree_const_iterator()
      : _M_node() { }

      explicit
      _Rb_tree_const_iterator(_Base_ptr __x)
      : _M_node(__x) { }

      _Rb_tree_const_iterator(const iterator& __it)
      : _M_node(__it._M_node) { }

      reference
      operator*() const
      { return *static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      pointer
      operator->() const
      { return static_cast<_Node_ptr>(_M_node)->_M_valptr(); }

      _Rb_tree_const_iterator&
      operator++()
      {
 _M_node = _Rb_tree_increment(_M_node);
 return *this;
      }

      _Rb_tree_const_iterator
      operator++(int)
      {
 _Rb_tree_const_iterator __tmp = *this;
 _M_node = _Rb_tree_increment(_M_node);
 return __tmp;
      }

      _Rb_tree_const_iterator&
      operator--()
      {
 _M_node = _Rb_tree_decrement(_M_node);
 return *this;
      }

      _Rb_tree_const_iterator
      operator--(int)
      {
 _Rb_tree_const_iterator __tmp = *this;
 _M_node = _Rb_tree_decrement(_M_node);
 return __tmp;
      }

      friend bool
      operator==(const _Rb_tree_const_iterator& __x,
   const _Rb_tree_const_iterator& __y)
      { return __x._M_node == __y._M_node; }


      friend bool
      operator!=(const _Rb_tree_const_iterator& __x,
   const _Rb_tree_const_iterator& __y)
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
# 669 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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
         _Node_base& __header) throw()
      {
 return _Rb_tree_insert_and_rebalance(__insert_left, __x, __p, __header);
      }

      __attribute__((__nonnull__,__returns_nonnull__))
      static _Node_base*
      _S_rebalance_for_erase(_Node_base* const __z,
        _Node_base& __header) throw()
      { return _Rb_tree_rebalance_for_erase(__z, __header); }
    };




  template<typename _Val, typename _Ptr>
    struct _Node_traits
    : _Node_traits<_Val, _Val*>
    { };
# 1011 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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





 ~_Reuse_or_alloc_node()
 {
   if (_M_root)
     _M_t._M_erase(static_cast<_Node&>(*_M_root)._M_node_ptr());
 }

 template<typename _Arg>
   _Node_ptr
   operator()(const _Arg& __arg)
   {
     _Base_ptr __base = _M_extract();
     if (__base)
       {
  _Node_ptr __node = static_cast<_Node&>(*__base)._M_node_ptr();
  _M_t._M_destroy_node(__node);
  _M_t._M_construct_node(__node, (__arg));
  return __node;
       }

     return _M_t._M_create_node((__arg));
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
   operator()(const _Arg& __arg) const
   { return _M_t._M_create_node((__arg)); }

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
      _M_get_Node_allocator()
      { return this->_M_impl; }

      const _Node_allocator&
      _M_get_Node_allocator() const
      { return this->_M_impl; }

      allocator_type
      get_allocator() const
      { return allocator_type(_M_get_Node_allocator()); }

    protected:
      _Node_ptr
      _M_get_node()
      {

 return _Node_alloc_traits::allocate(_M_get_Node_allocator(), 1);
# 1185 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      }

      void
      _M_put_node(_Node_ptr __p)
      {

 _Node_alloc_traits::deallocate(_M_get_Node_allocator(), __p, 1);
# 1207 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      }


      void
      _M_construct_node(_Node_ptr __node, const value_type& __x)
      {
 try
   { get_allocator().construct(__node->_M_valptr(), __x); }
 catch(...)
   {
     _M_put_node(__node);
     throw;
   }
      }

      _Node_ptr
      _M_create_node(const value_type& __x)
      {
 _Node_ptr __tmp = _M_get_node();
 _M_construct_node(__tmp, __x);
 return __tmp;
      }
# 1259 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      void
      _M_destroy_node(_Node_ptr __p)
      {

 get_allocator().destroy(__p->_M_valptr());




      }

      void
      _M_drop_node(_Node_ptr __p)
      {
 _M_destroy_node(__p);
 _M_put_node(__p);
      }

      template<bool _MoveValue, typename _NodeGen>
 _Node_ptr
 _M_clone_node(_Node_ptr __x, _NodeGen& __node_gen)
 {





   _Node_ptr __tmp
     = __node_gen((*__x->_M_valptr()));
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
    


   : _Node_allocator()
   { }

   _Rb_tree_impl(const _Rb_tree_impl& __x)
   : _Node_allocator(_Node_alloc_traits::_S_select_on_copy(__x))
   , _Base_key_compare(__x._M_key_compare)
   , _Header_t()
   { }


   _Rb_tree_impl(const _Key_compare& __comp, const _Node_allocator& __a)
   : _Node_allocator(__a), _Base_key_compare(__comp)
   { }
# 1347 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
 };

      _Rb_tree_impl<_Compare> _M_impl;

    protected:
      _Base_ptr&
      _M_root()
      { return this->_M_impl._M_header._M_parent; }

      _Base_ptr
      _M_root() const
      { return this->_M_impl._M_header._M_parent; }

      _Base_ptr&
      _M_leftmost()
      { return this->_M_impl._M_header._M_left; }

      _Base_ptr
      _M_leftmost() const
      { return this->_M_impl._M_header._M_left; }

      _Base_ptr&
      _M_rightmost()
      { return this->_M_impl._M_header._M_right; }

      _Base_ptr
      _M_rightmost() const
      { return this->_M_impl._M_header._M_right; }

      _Base_ptr
      _M_begin() const
      { return this->_M_impl._M_header._M_parent; }

      _Node_ptr
      _M_begin_node() const
      {
 _Base_ptr __begin = this->_M_impl._M_header._M_parent;
 return __begin
   ? static_cast<_Node&>(*__begin)._M_node_ptr()
   : _Node_ptr();
      }

      _Base_ptr
      _M_end() const
      { return this->_M_impl._M_header._M_base_ptr(); }

      static const _Key&
      _S_key(const _Node& __node)
      {
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
      _S_left(_Base_ptr __x)
      { return __x->_M_left; }

      static _Node_ptr
      _S_left(_Node_ptr __x)
      {
 return __x->_M_left
   ? static_cast<_Node&>(*__x->_M_left)._M_node_ptr()
   : _Node_ptr();
      }

      static _Base_ptr
      _S_right(_Base_ptr __x)
      { return __x->_M_right; }

      static _Node_ptr
      _S_right(_Node_ptr __x)
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
# 1498 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      template<typename _NodeGen>
 iterator
 _M_insert_(_Base_ptr __x, _Base_ptr __y,
     const value_type& __v, _NodeGen&);



      iterator
      _M_insert_lower(_Base_ptr __y, const value_type& __v);

      iterator
      _M_insert_equal_lower(const value_type& __x);


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


      _Rb_tree() { }




      _Rb_tree(const _Compare& __comp,
        const allocator_type& __a = allocator_type())
      : _M_impl(__comp, _Node_allocator(__a)) { }

      _Rb_tree(const _Rb_tree& __x)
      : _M_impl(__x._M_impl)
      {
 if (__x._M_root())
   _M_root() = _M_copy(__x);
      }
# 1608 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      ~_Rb_tree()
      { _M_erase(_M_begin_node()); }

      _Rb_tree&
      operator=(const _Rb_tree& __x);


      _Compare
      key_comp() const
      { return _M_impl._M_key_compare; }

      iterator
      begin()
      { return iterator(this->_M_impl._M_header._M_left); }

      const_iterator
      begin() const
      { return const_iterator(this->_M_impl._M_header._M_left); }

      iterator
      end()
      { return iterator(_M_end()); }

      const_iterator
      end() const
      { return const_iterator(_M_end()); }

      reverse_iterator
      rbegin()
      { return reverse_iterator(end()); }

      const_reverse_iterator
      rbegin() const
      { return const_reverse_iterator(end()); }

      reverse_iterator
      rend()
      { return reverse_iterator(begin()); }

      const_reverse_iterator
      rend() const
      { return const_reverse_iterator(begin()); }

      bool
      empty() const
      { return _M_impl._M_node_count == 0; }

      size_type
      size() const
      { return _M_impl._M_node_count; }

      size_type
      max_size() const
      { return _Node_alloc_traits::max_size(_M_get_Node_allocator()); }

      void
      swap(_Rb_tree& __t)
      ;
# 1755 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      pair<iterator, bool>
      _M_insert_unique(const value_type& __x);

      iterator
      _M_insert_equal(const value_type& __x);

      template<typename _NodeGen>
 iterator
 _M_insert_unique_(const_iterator __pos, const value_type& __x,
     _NodeGen&);

      iterator
      _M_insert_unique_(const_iterator __pos, const value_type& __x)
      {
 _Alloc_node __an(*this);
 return _M_insert_unique_(__pos, __x, __an);
      }

      template<typename _NodeGen>
 iterator
 _M_insert_equal_(const_iterator __pos, const value_type& __x,
    _NodeGen&);
      iterator
      _M_insert_equal_(const_iterator __pos, const value_type& __x)
      {
 _Alloc_node __an(*this);
 return _M_insert_equal_(__pos, __x, __an);
      }

      template<typename _InputIterator>
 void
 _M_insert_range_unique(_InputIterator __first, _InputIterator __last)
 {
   _Alloc_node __an(*this);
   for (; __first != __last; ++__first)
     _M_insert_unique_(end(), *__first, __an);
 }

      template<typename _InputIterator>
 void
 _M_insert_range_equal(_InputIterator __first, _InputIterator __last)
 {
   _Alloc_node __an(*this);
   for (; __first != __last; ++__first)
     _M_insert_equal_(end(), *__first, __an);
 }


    private:
      void
      _M_erase_aux(const_iterator __position);

      void
      _M_erase_aux(const_iterator __first, const_iterator __last);

    public:
# 1837 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      void
      erase(iterator __position)
      {
 do { if (__builtin_expect(!bool(__position != end()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h", 1840, __PRETTY_FUNCTION__, "__position != end()"); } while (false);
 _M_erase_aux(__position);
      }

      void
      erase(const_iterator __position)
      {
 do { if (__builtin_expect(!bool(__position != end()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h", 1847, __PRETTY_FUNCTION__, "__position != end()"); } while (false);
 _M_erase_aux(__position);
      }


      size_type
      erase(const key_type& __x);
# 1866 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
      void
      erase(iterator __first, iterator __last)
      { _M_erase_aux(__first, __last); }

      void
      erase(const_iterator __first, const_iterator __last)
      { _M_erase_aux(__first, __last); }


      void
      clear()
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
# 2322 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
    };

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    inline void
    swap(_Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>& __x,
  _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>& __y)
    { __x.swap(__y); }
# 2424 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>&
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
    operator=(const _Rb_tree& __x)
    {
      if (this != std::__addressof(__x))
 {
# 2449 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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



    template<typename _NodeGen>

      typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_insert_(_Base_ptr __x, _Base_ptr __p,



   const _Val& __v,

   _NodeGen& __node_gen)
      {
 bool __insert_left = (__x || __p == _M_end()
         || _M_impl._M_key_compare(_KeyOfValue()(__v),
       _S_key(__p)));

 _Base_ptr __z =
   __node_gen((__v))->_M_base_ptr();

 _Node_traits::_S_insert_and_rebalance
   (__insert_left, __z, __p, this->_M_impl._M_header);
 ++_M_impl._M_node_count;
 return iterator(__z);
      }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>



    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::



    _M_insert_lower(_Base_ptr __p, const _Val& __v)

    {
      bool __insert_left = (__p == _M_end()
       || !_M_impl._M_key_compare(_S_key(__p),
             _KeyOfValue()(__v)));

      _Base_ptr __z =
 _M_create_node((__v))->_M_base_ptr();
      _Node_traits::_S_insert_and_rebalance
 (__insert_left, __z, __p, this->_M_impl._M_header);
      ++_M_impl._M_node_count;
      return iterator(__z);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>



    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::



    _M_insert_equal_lower(const _Val& __v)

    {
      _Base_ptr __x = _M_begin();
      _Base_ptr __y = _M_end();
      while (__x)
 {
   __y = __x;
   __x = !_M_impl._M_key_compare(_S_key(__x), _KeyOfValue()(__v)) ?
  _S_left(__x) : _S_right(__x);
 }
      return _M_insert_lower(__y, (__v));
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



    pair<typename _Rb_tree<_Key, _Val, _KeyOfValue,
      _Compare, _Alloc>::iterator, bool>
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::



    _M_insert_unique(const _Val& __v)

    {
      typedef pair<iterator, bool> _Res;
      pair<_Base_ptr, _Base_ptr> __res
 = _M_get_insert_unique_pos(_KeyOfValue()(__v));

      if (__res.second)
 {
   _Alloc_node __an(*this);
   return _Res(_M_insert_(__res.first, __res.second,
     (__v), __an),
        true);
 }

      return _Res(iterator(__res.first), false);
    }

  template<typename _Key, typename _Val, typename _KeyOfValue,
    typename _Compare, typename _Alloc>



    typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
    _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::



    _M_insert_equal(const _Val& __v)

    {
      pair<_Base_ptr, _Base_ptr> __res
 = _M_get_insert_equal_pos(_KeyOfValue()(__v));
      _Alloc_node __an(*this);
      return _M_insert_(__res.first, __res.second,
   (__v), __an);
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



    template<typename _NodeGen>

      typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_insert_unique_(const_iterator __position,



   const _Val& __v,

   _NodeGen& __node_gen)
    {
      pair<_Base_ptr, _Base_ptr> __res
 = _M_get_insert_hint_unique_pos(__position, _KeyOfValue()(__v));

      if (__res.second)
 return _M_insert_(__res.first, __res.second,
     (__v),
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



    template<typename _NodeGen>

      typename _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::iterator
      _Rb_tree<_Key, _Val, _KeyOfValue, _Compare, _Alloc>::
      _M_insert_equal_(const_iterator __position,



         const _Val& __v,

         _NodeGen& __node_gen)
      {
 pair<_Base_ptr, _Base_ptr> __res
   = _M_get_insert_hint_equal_pos(__position, _KeyOfValue()(__v));

 if (__res.second)
   return _M_insert_(__res.first, __res.second,
       (__v),
       __node_gen);

 return _M_insert_equal_lower((__v));
      }
# 3106 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_tree.h" 3
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
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
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
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
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
# 195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map() : _M_t() { }
# 205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      explicit
      map(const _Compare& __comp,
   const allocator_type& __a = allocator_type())
      : _M_t(__comp, _Pair_alloc_type(__a)) { }







      map(const map& __x)
      : _M_t(__x._M_t) { }
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
# 323 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      map&
      operator=(const map& __x)
      {
 _M_t = __x._M_t;
 return *this;
      }
# 357 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      allocator_type
      get_allocator() const
      { return allocator_type(_M_t.get_allocator()); }







      iterator
      begin()
      { return _M_t.begin(); }






      const_iterator
      begin() const
      { return _M_t.begin(); }






      iterator
      end()
      { return _M_t.end(); }






      const_iterator
      end() const
      { return _M_t.end(); }






      reverse_iterator
      rbegin()
      { return _M_t.rbegin(); }






      const_reverse_iterator
      rbegin() const
      { return _M_t.rbegin(); }






      reverse_iterator
      rend()
      { return _M_t.rend(); }






      const_reverse_iterator
      rend() const
      { return _M_t.rend(); }
# 476 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      bool
      empty() const
      { return _M_t.empty(); }


      size_type
      size() const
      { return _M_t.size(); }


      size_type
      max_size() const
      { return _M_t.max_size(); }
# 503 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      mapped_type&
      operator[](const key_type& __k)
      {



 iterator __i = lower_bound(__k);

 if (__i == end() || key_comp()(__k, (*__i).first))





   __i = insert(__i, value_type(__k, mapped_type()));

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
# 833 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      std::pair<iterator, bool>
      insert(const value_type& __x)
      { return _M_t._M_insert_unique(__x); }
# 907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      iterator



      insert(iterator __position, const value_type& __x)

      { return _M_t._M_insert_unique_(__position, __x); }
# 940 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      template<typename _InputIterator>
 void
 insert(_InputIterator __first, _InputIterator __last)
 { _M_t._M_insert_range_unique(__first, __last); }
# 1100 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      void
      erase(iterator __position)
      { _M_t.erase(__position); }
# 1116 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      size_type
      erase(const key_type& __x)
      { return _M_t.erase(__x); }
# 1152 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      void
      erase(iterator __first, iterator __last)
      { _M_t.erase(__first, __last); }
# 1170 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_map.h" 3
      void
      swap(map& __x)
     
      { _M_t.swap(__x._M_t); }







      void
      clear()
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
# 129 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
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
# 185 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap() : _M_t() { }
# 195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      explicit
      multimap(const _Compare& __comp,
        const allocator_type& __a = allocator_type())
      : _M_t(__comp, _Pair_alloc_type(__a)) { }







      multimap(const multimap& __x)
      : _M_t(__x._M_t) { }
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
# 312 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      multimap&
      operator=(const multimap& __x)
      {
 _M_t = __x._M_t;
 return *this;
      }
# 346 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      allocator_type
      get_allocator() const
      { return allocator_type(_M_t.get_allocator()); }







      iterator
      begin()
      { return _M_t.begin(); }






      const_iterator
      begin() const
      { return _M_t.begin(); }






      iterator
      end()
      { return _M_t.end(); }






      const_iterator
      end() const
      { return _M_t.end(); }






      reverse_iterator
      rbegin()
      { return _M_t.rbegin(); }






      const_reverse_iterator
      rbegin() const
      { return _M_t.rbegin(); }






      reverse_iterator
      rend()
      { return _M_t.rend(); }






      const_reverse_iterator
      rend() const
      { return _M_t.rend(); }
# 463 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      bool
      empty() const
      { return _M_t.empty(); }


      size_type
      size() const
      { return _M_t.size(); }


      size_type
      max_size() const
      { return _M_t.max_size(); }
# 544 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator
      insert(const value_type& __x)
      { return _M_t._M_insert_equal(__x); }
# 583 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      iterator



      insert(iterator __position, const value_type& __x)

      { return _M_t._M_insert_equal_(__position, __x); }
# 617 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      template<typename _InputIterator>
 void
 insert(_InputIterator __first, _InputIterator __last)
 { _M_t._M_insert_range_equal(__first, __last); }
# 728 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      void
      erase(iterator __position)
      { _M_t.erase(__position); }
# 744 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      size_type
      erase(const key_type& __x)
      { return _M_t.erase(__x); }
# 784 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      void
      erase(iterator __first, iterator __last)
      { _M_t.erase(__first, __last); }
# 802 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3
      void
      swap(multimap& __x)
     
      { _M_t.swap(__x._M_t); }







      void
      clear()
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
   
    { __x.swap(__y); }


# 1236 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_multimap.h" 3

}
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/range_access.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/erase_if.h" 1 3
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 81 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/map" 2 3
# 9 "./tree-ssa/copyprop.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 1 3
# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 1 3
# 71 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
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




  template<typename _Iter,
    typename _Base = __decltype(std::__niter_base(*(_Iter*)0))>
    struct __unwrappable_niter
    { enum { __value = false }; };

  template<typename _Iter, typename _Tp>
    struct __unwrappable_niter<_Iter, _Tp*>
    { enum { __value = true }; };


  template<bool _CanMemcpy>
    struct __uninitialized_copy
    {
      template<typename _InputIterator, typename _ForwardIterator>
        static _ForwardIterator
        __uninit_copy(_InputIterator __first, _InputIterator __last,
        _ForwardIterator __result)
 { return std::__do_uninit_copy(__first, __last, __result); }
    };

  template<>
    struct __uninitialized_copy<true>
    {

      template<typename _InputIterator, typename _ForwardIterator>
        static _ForwardIterator
        __uninit_copy(_InputIterator __first, _InputIterator __last,
        _ForwardIterator __result)
 {
   if (__unwrappable_niter<_InputIterator>::__value
  && __unwrappable_niter<_ForwardIterator>::__value)
     {
       __uninit_copy(std::__niter_base(__first),
       std::__niter_base(__last),
       std::__niter_base(__result));
       std::advance(__result, std::distance(__first, __last));
       return __result;
     }
   else
     return std::__do_uninit_copy(__first, __last, __result);
 }


      template<typename _Tp, typename _Up>
 static _Up*
 __uninit_copy(_Tp* __first, _Tp* __last, _Up* __result)
 {


   typedef __typeof__(static_cast<_Up>(*__first)) __check
     __attribute__((__unused__));

   const ptrdiff_t __n = __last - __first;
   if (__builtin_expect(__n > 0, true))
     {
       __builtin_memcpy(__result, __first, __n * sizeof(_Tp));
       __result += __n;
     }
   return __result;
 }
    };



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _InputIterator, typename _ForwardIterator>
    inline _ForwardIterator
    uninitialized_copy(_InputIterator __first, _InputIterator __last,
         _ForwardIterator __result)
    {
# 299 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      typedef typename iterator_traits<_InputIterator>::value_type
 _ValueType1;
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType2;

      const bool __can_memcpy
 = __memcpyable<_ValueType1*, _ValueType2*>::__value
     && __is_trivially_constructible(_ValueType2, __decltype(*__first));

      return __uninitialized_copy<__can_memcpy>::
        __uninit_copy(__first, __last, __result);

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



  template<bool _CanMemset>
    struct __uninitialized_fill
    {
      template<typename _ForwardIterator, typename _Tp>
 static void
 __uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
        const _Tp& __x)
 { std::__do_uninit_fill(__first, __last, __x); }
    };

  template<>
    struct __uninitialized_fill<true>
    {

      template<typename _ForwardIterator, typename _Tp>
 static void
 __uninit_fill(_ForwardIterator __first, _ForwardIterator __last,
        const _Tp& __x)
 {
   if (__unwrappable_niter<_ForwardIterator>::__value)
     __uninit_fill(std::__niter_base(__first),
     std::__niter_base(__last),
     __x);
   else
     std::__do_uninit_copy(__first, __last, __x);
 }


      template<typename _Up, typename _Tp>
 static void
 __uninit_fill(_Up* __first, _Up* __last, const _Tp& __x)
 {


   typedef __typeof__(static_cast<_Up>(__x)) __check
     __attribute__((__unused__));

   if (__first != __last)
     __builtin_memset(__first, (unsigned char)__x, __last - __first);
 }
    };
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Tp>
    inline void
    uninitialized_fill(_ForwardIterator __first, _ForwardIterator __last,
         const _Tp& __x)
    {
# 397 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;
# 430 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      const bool __can_memset = __is_byte<_ValueType>::__value
      && __is_integer<_Tp>::__value;

      __uninitialized_fill<__can_memset>::__uninit_fill(__first, __last, __x);

    }




  template<typename _ForwardIterator, typename _Size, typename _Tp>
   
    _ForwardIterator
    __do_uninit_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {
      _UninitDestroyGuard<_ForwardIterator> __guard(__first);
# 457 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      for (; __n--; ++__first)
 std::_Construct(std::__addressof(*__first), __x);
      __guard.release();
      return __first;
    }



  template<bool _CanMemset>
    struct __uninitialized_fill_n
    {
      template<typename _ForwardIterator, typename _Size, typename _Tp>
 static _ForwardIterator
        __uninit_fill_n(_ForwardIterator __first, _Size __n,
   const _Tp& __x)
 { return std::__do_uninit_fill_n(__first, __n, __x); }
    };

  template<>
    struct __uninitialized_fill_n<true>
    {

      template<typename _ForwardIterator, typename _Size, typename _Tp>
 static _ForwardIterator
        __uninit_fill_n(_ForwardIterator __first, _Size __n,
   const _Tp& __x)
 {
   if (__unwrappable_niter<_ForwardIterator>::__value)
     {
       _ForwardIterator __last = __first;
       std::advance(__last, __n);
       __uninitialized_fill<true>::__uninit_fill(__first, __last, __x);
       return __last;
     }
   else
     return std::__do_uninit_fill_n(__first, __n, __x);
 }
    };



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
# 511 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
  template<typename _ForwardIterator, typename _Size, typename _Tp>
    inline _ForwardIterator
    uninitialized_fill_n(_ForwardIterator __first, _Size __n, const _Tp& __x)
    {






      typedef typename iterator_traits<_ForwardIterator>::value_type
 _ValueType;
# 555 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3
      const bool __can_memset = __is_byte<_ValueType>::__value
      && __is_integer<_Tp>::__value
      && __is_integer<_Size>::__value;

      return __uninitialized_fill_n<__can_memset>::
 __uninit_fill_n(__first, __n, __x);

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
      return std::__uninitialized_copy_a((__first),
      (__last),
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
 ((__first),
  (__last), __result, __alloc);
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
# 1324 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_uninitialized.h" 3

}
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 1 3
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


 _Vector_impl_data()
 : _M_start(), _M_finish(), _M_end_of_storage()
 { }
# 114 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3

 void
 _M_copy_data(_Vector_impl_data const& __x)
 {
   _M_start = __x._M_start;
   _M_finish = __x._M_finish;
   _M_end_of_storage = __x._M_end_of_storage;
 }


 void
 _M_swap_data(_Vector_impl_data& __x)
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

 _Vector_impl()




 : _Tp_alloc_type()
 { }


 _Vector_impl(_Tp_alloc_type const& __a)
 : _Tp_alloc_type(__a)
 { }
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      };

    public:
      typedef _Alloc allocator_type;

     
      _Tp_alloc_type&
      _M_get_Tp_allocator()
      { return this->_M_impl; }

     
      const _Tp_alloc_type&
      _M_get_Tp_allocator() const
      { return this->_M_impl; }

     
      allocator_type
      get_allocator() const
      { return allocator_type(_M_get_Tp_allocator()); }




      _Vector_base() { }


     
      _Vector_base(const allocator_type& __a)
      : _M_impl(__a) { }



     
      _Vector_base(size_t __n)
      : _M_impl()
      { _M_create_storage(__n); }


     
      _Vector_base(size_t __n, const allocator_type& __a)
      : _M_impl(__a)
      { _M_create_storage(__n); }
# 369 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      ~_Vector_base()
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
# 475 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
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
# 544 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
    protected:
      using _Base::_M_allocate;
      using _Base::_M_deallocate;
      using _Base::_M_impl;
      using _Base::_M_get_Tp_allocator;

    public:
# 560 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      vector() { }






      explicit
     
      vector(const allocator_type& __a)
      : _Base(__a) { }
# 609 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      explicit
      vector(size_type __n, const value_type& __value = value_type(),
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
# 742 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator>
 vector(_InputIterator __first, _InputIterator __last,
        const allocator_type& __a = allocator_type())
 : _Base(__a)
 {

   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_initialize_dispatch(__first, __last, _Integral());
 }
# 800 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      ~vector()
      {
 std::_Destroy(this->_M_impl._M_start, this->_M_impl._M_finish,
        _M_get_Tp_allocator());
 ;
      }
# 817 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      vector&
      operator=(const vector& __x);
# 874 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      assign(size_type __n, const value_type& __val)
      { _M_fill_assign(__n, __val); }
# 899 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator>
 void
 assign(_InputIterator __first, _InputIterator __last)
 {

   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_assign_dispatch(__first, __last, _Integral());
 }
# 987 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      using _Base::get_allocator;







     
      iterator
      begin()
      { return iterator(this->_M_impl._M_start); }






     
      const_iterator
      begin() const
      { return const_iterator(this->_M_impl._M_start); }






     
      iterator
      end()
      { return iterator(this->_M_impl._M_finish); }






     
      const_iterator
      end() const
      { return const_iterator(this->_M_impl._M_finish); }






     
      reverse_iterator
      rbegin()
      { return reverse_iterator(end()); }






     
      const_reverse_iterator
      rbegin() const
      { return const_reverse_iterator(end()); }






     
      reverse_iterator
      rend()
      { return reverse_iterator(begin()); }






     
      const_reverse_iterator
      rend() const
      { return const_reverse_iterator(begin()); }
# 1114 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      size_type
      size() const
      {
 ptrdiff_t __dif = this->_M_impl._M_finish - this->_M_impl._M_start;
 if (__dif < 0)
    __builtin_unreachable ();
 return size_type(__dif);
      }


     
      size_type
      max_size() const
      { return _S_max_size(_M_get_Tp_allocator()); }
# 1182 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      resize(size_type __new_size, value_type __x = value_type())
      {
 if (__new_size > size())
   _M_fill_insert(end(), __new_size - size(), __x);
 else if (__new_size < size())
   _M_erase_at_end(this->_M_impl._M_start + __new_size);
      }
# 1205 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      size_type
      capacity() const
      {
 ptrdiff_t __dif = this->_M_impl._M_end_of_storage
     - this->_M_impl._M_start;
 if (__dif < 0)
    __builtin_unreachable ();
 return size_type(__dif);
      }





     
      bool
      empty() const
      { return begin() == end(); }
# 1242 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      reserve(size_type __n);
# 1258 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      reference
      operator[](size_type __n)
      {
 do { if (__builtin_expect(!bool(__n < this->size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1262, __PRETTY_FUNCTION__, "__n < this->size()"); } while (false);
 return *(this->_M_impl._M_start + __n);
      }
# 1277 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      const_reference
      operator[](size_type __n) const
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
     
      reference
      at(size_type __n)
      {
 _M_range_check(__n);
 return (*this)[__n];
      }
# 1329 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      const_reference
      at(size_type __n) const
      {
 _M_range_check(__n);
 return (*this)[__n];
      }





     
      reference
      front()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1345, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }





     
      const_reference
      front() const
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1357, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }





     
      reference
      back()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1369, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }





     
      const_reference
      back() const
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1381, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }
# 1392 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      _Tp*
      data()
      { return _M_data_ptr(this->_M_impl._M_start); }

     
      const _Tp*
      data() const
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
# 1454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      pop_back()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h", 1458, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 --this->_M_impl._M_finish;
 _Alloc_traits::destroy(this->_M_impl, this->_M_impl._M_finish);
 ;
      }
# 1509 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      iterator
      insert(iterator __position, const value_type& __x);
# 1591 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      void
      insert(iterator __position, size_type __n, const value_type& __x)
      { _M_fill_insert(__position, __n, __x); }
# 1639 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _InputIterator>
 void
 insert(iterator __position, _InputIterator __first,
        _InputIterator __last)
 {

   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_insert_dispatch(__position, __first, __last, _Integral());
 }
# 1700 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator




      erase(iterator __position)
      { return _M_erase(__position); }
# 1728 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      iterator
# 1738 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      erase(iterator __first, iterator __last)
      { return _M_erase(__first, __last); }
# 1753 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
      void
      swap(vector& __x)
      {




 this->_M_impl._M_swap_data(__x._M_impl);
 _Alloc_traits::_S_on_swap(_M_get_Tp_allocator(),
      __x._M_get_Tp_allocator());
      }







     
      void
      clear()
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
# 1835 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _Integer>
 void
 _M_initialize_dispatch(_Integer __int_n, _Integer __value, __true_type)
 {
   const size_type __n = static_cast<size_type>(__int_n);
   pointer __start =
     _M_allocate(_S_check_init_len(__n, _M_get_Tp_allocator()));
   this->_M_impl._M_start = __start;
   this->_M_impl._M_end_of_storage = __start + __n;
   _M_fill_initialize(__n, __value);
 }


      template<typename _InputIterator>
 void
 _M_initialize_dispatch(_InputIterator __first, _InputIterator __last,
          __false_type)
 {
   _M_range_initialize(__first, __last,
         std::__iterator_category(__first));
 }



      template<typename _InputIterator>

 void
 _M_range_initialize(_InputIterator __first, _InputIterator __last,
       std::input_iterator_tag)
 {
   try {
     for (; __first != __last; ++__first)



       push_back(*__first);

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
# 2017 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      void
      _M_insert_aux(iterator __position, const value_type& __x);

      void
      _M_realloc_insert(iterator __position, const value_type& __x);

      void
      _M_realloc_append(const value_type& __x);
# 2098 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
     
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
      _S_max_size(const _Tp_alloc_type& __a)
      {



 const size_t __diffmax
   = __gnu_cxx::__numeric_traits<ptrdiff_t>::__max / sizeof(_Tp);
 const size_t __allocmax = _Alloc_traits::max_size(__a);
 return (std::min)(__diffmax, __allocmax);
      }





     
      void
      _M_erase_at_end(pointer __pos)
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
# 2191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _Up>

 _Up*
 _M_data_ptr(_Up* __ptr) const
 { return __ptr; }
# 2204 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
      template<typename _Ptr>
 value_type*
 _M_data_ptr(_Ptr __ptr) const
 { return empty() ? (value_type*)0 : __ptr.operator->(); }

    };
# 2238 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
  template<typename _Tp, typename _Alloc>
   
    inline bool
    operator==(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return (__x.size() == __y.size()
       && std::equal(__x.begin(), __x.end(), __y.begin())); }
# 2278 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3
  template<typename _Tp, typename _Alloc>
    inline bool
    operator<(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return std::lexicographical_compare(__x.begin(), __x.end(),
       __y.begin(), __y.end()); }


  template<typename _Tp, typename _Alloc>
    inline bool
    operator!=(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return !(__x == __y); }


  template<typename _Tp, typename _Alloc>
    inline bool
    operator>(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return __y < __x; }


  template<typename _Tp, typename _Alloc>
    inline bool
    operator<=(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return !(__y < __x); }


  template<typename _Tp, typename _Alloc>
    inline bool
    operator>=(const vector<_Tp, _Alloc>& __x, const vector<_Tp, _Alloc>& __y)
    { return !(__x < __y); }



  template<typename _Tp, typename _Alloc>
   
    inline void
    swap(vector<_Tp, _Alloc>& __x, vector<_Tp, _Alloc>& __y)
   
    { __x.swap(__y); }


# 2333 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_vector.h" 3

}
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 1 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{


  typedef unsigned long _Bit_type;
  enum { _S_word_bit = int(8 * sizeof(_Bit_type)) };

  __attribute__((__nonnull__))
 
  void
  __fill_bvector_n(_Bit_type*, size_t, bool) ;



  struct _Bit_reference
  {
  private:
    template<typename, typename> friend class vector;
    friend struct _Bit_iterator;
    friend struct _Bit_const_iterator;

   
    _Bit_reference() : _M_p(0), _M_mask(0) { }

    _Bit_type * _M_p;
    _Bit_type _M_mask;

   
    _Bit_reference(_Bit_type * __x, _Bit_type __y)
    : _M_p(__x), _M_mask(__y) { }

  public:




   
    operator bool() const
    { return !!(*_M_p & _M_mask); }

   
    _Bit_reference&
    operator=(bool __x)
    {
      if (__x)
 *_M_p |= _M_mask;
      else
 *_M_p &= ~_M_mask;
      return *this;
    }
# 131 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
   
    _Bit_reference&
    operator=(const _Bit_reference& __x)
    { return *this = bool(__x); }

   
    bool
    operator==(const _Bit_reference& __x) const
    { return bool(*this) == bool(__x); }

   
    bool
    operator<(const _Bit_reference& __x) const
    { return !bool(*this) && bool(__x); }

   
    void
    flip()
    { *_M_p ^= _M_mask; }
# 179 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
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

   
    friend bool
    operator==(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    {
      __x._M_assume_normalized();
      __y._M_assume_normalized();
      return __x._M_p == __y._M_p && __x._M_offset == __y._M_offset;
    }
# 266 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
   
    friend bool
    operator<(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    {
      __x._M_assume_normalized();
      __y._M_assume_normalized();
      return __x._M_p < __y._M_p
     || (__x._M_p == __y._M_p && __x._M_offset < __y._M_offset);
    }

   
    friend bool
    operator!=(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return !(__x == __y); }

   
    friend bool
    operator>(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return __y < __x; }

   
    friend bool
    operator<=(const _Bit_iterator_base& __x, const _Bit_iterator_base& __y)
    { return !(__y < __x); }

   
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

   
    reference
    operator[](difference_type __i) const
    { return *(*this + __i); }

   
    friend iterator
    operator+(const iterator& __x, difference_type __n)
    {
      iterator __tmp = __x;
      __tmp += __n;
      return __tmp;
    }

   
    friend iterator
    operator+(difference_type __n, const iterator& __x)
    { return __x + __n; }

   
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

   
    const_reference
    operator[](difference_type __i) const
    { return *(*this + __i); }

   
    friend const_iterator
    operator+(const const_iterator& __x, difference_type __n)
    {
      const_iterator __tmp = __x;
      __tmp += __n;
      return __tmp;
    }

   
    friend const_iterator
    operator-(const const_iterator& __x, difference_type __n)
    {
      const_iterator __tmp = __x;
      __tmp -= __n;
      return __tmp;
    }

   
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


 _Bvector_impl_data()
 : _M_start(), _M_finish(), _M_end_of_storage()
 { }
# 581 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3

 void
 _M_reset()
 { *this = _Bvector_impl_data(); }


 void
 _M_swap_data(_Bvector_impl_data& __x)
 {


   std::swap(*this, __x);
 }
      };

      struct _Bvector_impl
 : public _Bit_alloc_type, public _Bvector_impl_data
      {

 _Bvector_impl()




 : _Bit_alloc_type()
 { }


 _Bvector_impl(const _Bit_alloc_type& __a)
 : _Bit_alloc_type(__a)
 { }
# 627 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3

 _Bit_type*
 _M_end_addr() const
 {
   if (this->_M_end_of_storage)
     return std::__addressof(this->_M_end_of_storage[-1]) + 1;
   return 0;
 }
      };

    public:
      typedef _Alloc allocator_type;

     
      _Bit_alloc_type&
      _M_get_Bit_allocator()
      { return this->_M_impl; }

     
      const _Bit_alloc_type&
      _M_get_Bit_allocator() const
      { return this->_M_impl; }

     
      allocator_type
      get_allocator() const
      { return allocator_type(_M_get_Bit_allocator()); }




      _Bvector_base() { }


     
      _Bvector_base(const allocator_type& __a)
      : _M_impl(_Bit_alloc_type(__a)) { }
# 674 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
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
# 718 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
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



      vector() { }


     
      explicit
      vector(const allocator_type& __a)
      : _Base(__a) { }
# 804 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      explicit
      vector(size_type __n, const bool& __value = bool(),
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
# 884 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      template<typename _InputIterator>
 vector(_InputIterator __first, _InputIterator __last,
        const allocator_type& __a = allocator_type())
 : _Base(__a)
 {

   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_initialize_dispatch(__first, __last, _Integral());
 }
# 920 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      ~vector() { }

     
      vector&
      operator=(const vector& __x)
      {
 if (&__x == this)
   return *this;
# 944 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
 if (__x.size() > capacity())
   {
     this->_M_deallocate();
     _M_initialize(__x.size());
   }
 this->_M_impl._M_finish = _M_copy_aligned(__x.begin(), __x.end(),
        begin());
 return *this;
      }
# 994 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      assign(size_type __n, const bool& __x)
      { _M_fill_assign(__n, __x); }
# 1007 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      template<typename _InputIterator>
 void
 assign(_InputIterator __first, _InputIterator __last)
 {

   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_assign_dispatch(__first, __last, _Integral());
 }
# 1039 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      iterator
      begin()
      { return iterator(this->_M_impl._M_start._M_p, 0); }

     
      const_iterator
      begin() const
      { return const_iterator(this->_M_impl._M_start._M_p, 0); }

     
      iterator
      end()
      { return this->_M_impl._M_finish; }

     
      const_iterator
      end() const
      { return this->_M_impl._M_finish; }

     
      reverse_iterator
      rbegin()
      { return reverse_iterator(end()); }

     
      const_reverse_iterator
      rbegin() const
      { return const_reverse_iterator(end()); }

     
      reverse_iterator
      rend()
      { return reverse_iterator(begin()); }

     
      const_reverse_iterator
      rend() const
      { return const_reverse_iterator(begin()); }
# 1101 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      size_type
      size() const
      { return size_type(end() - begin()); }

     
      size_type
      max_size() const
      {
 const size_type __isize =
   __gnu_cxx::__numeric_traits<difference_type>::__max
   - int(_S_word_bit) + 1;
 const size_type __asize
   = _Bit_alloc_traits::max_size(_M_get_Bit_allocator());
 return (__asize <= __isize / int(_S_word_bit)
  ? __asize * int(_S_word_bit) : __isize);
      }

     
      size_type
      capacity() const
      { return size_type(const_iterator(this->_M_impl._M_end_addr(), 0)
    - begin()); }

     
      bool
      empty() const
      { return begin() == end(); }

     
      reference
      operator[](size_type __n)
      {
 do { if (__builtin_expect(!bool(__n < this->size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1134, __PRETTY_FUNCTION__, "__n < this->size()"); } while (false);
 return begin()[__n];
      }

     
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
     
      reference
      at(size_type __n)
      {
 _M_range_check(__n);
 return (*this)[__n];
      }

     
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

     
      reference
      front()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1189, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }

     
      const_reference
      front() const
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1197, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *begin();
      }

     
      reference
      back()
      {
 do { if (__builtin_expect(!bool(!this->empty()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h", 1205, __PRETTY_FUNCTION__, "!this->empty()"); } while (false);
 return *(end() - 1);
      }

     
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
      swap(vector& __x)
      {




 this->_M_impl._M_swap_data(__x._M_impl);
 _Bit_alloc_traits::_S_on_swap(_M_get_Bit_allocator(),
          __x._M_get_Bit_allocator());
      }


     
      static void
      swap(reference __x, reference __y)
      {
 bool __tmp = __x;
 __x = __y;
 __y = __tmp;
      }

     
      iterator



      insert(iterator __position, const bool& __x)

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
# 1289 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      template<typename _InputIterator>
 void
 insert(iterator __position,
        _InputIterator __first, _InputIterator __last)
 {

   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_insert_dispatch(__position, __first, __last, _Integral());
 }
# 1310 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      void
      insert(iterator __position, size_type __n, const bool& __x)
      { _M_fill_insert(__position, __n, __x); }
# 1402 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      pop_back()
      { --this->_M_impl._M_finish; }

     
      iterator



      erase(iterator __position)

      { return _M_erase(__position._M_const_cast()); }

     
      iterator



      erase(iterator __first, iterator __last)

      { return _M_erase(__first._M_const_cast(), __last._M_const_cast()); }

     
      void
      resize(size_type __new_size, bool __x = bool())
      {
 if (__new_size < size())
   _M_erase_at_end(begin() + difference_type(__new_size));
 else
   insert(end(), __new_size - size(), __x);
      }
# 1442 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
     
      void
      flip()
      {
 _Bit_type * const __end = this->_M_impl._M_end_addr();
 for (_Bit_type * __p = this->_M_impl._M_start._M_p; __p != __end; ++__p)
   *__p = ~*__p;
      }

     
      void
      clear()
      { _M_erase_at_end(begin()); }
# 1479 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
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
      _M_initialize_value(bool __x)
      {
 if (_Bit_type* __p = this->_M_impl._M_start._M_p)
   __fill_bvector_n(__p, this->_M_impl._M_end_addr() - __p, __x);
      }

     
      void
      _M_reallocate(size_type __n);
# 1526 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      template<typename _Integer>
 void
 _M_initialize_dispatch(_Integer __n, _Integer __x, __true_type)
 {
   _M_initialize(static_cast<size_type>(__n));
   _M_initialize_value(__x);
 }

      template<typename _InputIterator>
 void
 _M_initialize_dispatch(_InputIterator __first, _InputIterator __last,
          __false_type)
 { _M_initialize_range(__first, __last,
         std::__iterator_category(__first)); }


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




      template<typename _Integer>
 void
 _M_assign_dispatch(_Integer __n, _Integer __val, __true_type)
 { _M_fill_assign(__n, __val); }

      template<class _InputIterator>
 void
 _M_assign_dispatch(_InputIterator __first, _InputIterator __last,
      __false_type)
 { _M_assign_aux(__first, __last, std::__iterator_category(__first)); }


     
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




      template<typename _Integer>
 void
 _M_insert_dispatch(iterator __pos, _Integer __n, _Integer __x,
      __true_type)
 { _M_fill_insert(__pos, __n, __x); }

      template<typename _InputIterator>
 void
 _M_insert_dispatch(iterator __pos,
      _InputIterator __first, _InputIterator __last,
      __false_type)
 { _M_insert_range(__pos, __first, __last,
     std::__iterator_category(__first)); }


     
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
# 1705 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3
      void data() { }

    };




 
  inline void
  __fill_bvector(_Bit_type* __v, unsigned int __first, unsigned int __last,
   bool __x)
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
  __fill_bvector_n(_Bit_type* __p, size_t __n, bool __x)
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
# 1777 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stl_bvector.h" 3

}
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/refwrap.h" 1 3
# 71 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3



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
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
     {
       __tmp = _M_allocate_and_copy(__n,
  (this->_M_impl._M_start),
  (this->_M_impl._M_finish));
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
# 130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
  template<typename _Tp, typename _Alloc>
   
    typename vector<_Tp, _Alloc>::iterator
    vector<_Tp, _Alloc>::



    insert(iterator __position, const value_type& __x)

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







       _M_insert_aux(__position, __x);

     }
 }
      else



 _M_realloc_insert(__position, __x);


      return iterator(this->_M_impl._M_start + __n);
    }

  template<typename _Tp, typename _Alloc>
   
    typename vector<_Tp, _Alloc>::iterator
    vector<_Tp, _Alloc>::
    _M_erase(iterator __position)
    {
      if (__position + 1 != end())
 std::copy(__position + 1, end(), __position);
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
     std::copy(__last, end(), __first);
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
# 235 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
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
# 423 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
  template<typename _Tp, typename _Alloc>
    void
    vector<_Tp, _Alloc>::
    _M_insert_aux(iterator __position, const _Tp& __x)

    {
      ;
      _Alloc_traits::construct(this->_M_impl, this->_M_impl._M_finish,
          (*(this->_M_impl._M_finish - 1)));
      ++this->_M_impl._M_finish;
      ;

      _Tp __x_copy = __x;

      std::copy_backward(__position.base(), this->_M_impl._M_finish - 2, this->_M_impl._M_finish - 1)

                                     ;

      *__position = __x_copy;



    }
# 455 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
  template<typename _Tp, typename _Alloc>
    void
    vector<_Tp, _Alloc>::
    _M_realloc_insert(iterator __position, const _Tp& __x)

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
# 485 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
 _Alloc_traits::construct(this->_M_impl,
     __new_start + __elems_before,
     __x);
# 502 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
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
# 560 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
  template<typename _Tp, typename _Alloc>
    void
    vector<_Tp, _Alloc>::
    _M_realloc_append(const _Tp& __x)

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
# 590 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
 _Alloc_traits::construct(this->_M_impl,
     __new_start + __elems,
     __x);
# 605 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
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

       value_type __x_copy = __x;




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
    std::copy_backward(__position.base(), __old_finish - __n, __old_finish)
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
# 856 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3
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
     (__tmp.begin()),
     (__tmp.end()));
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
      std::copy_backward(__position.base(), __old_finish - __n, __old_finish)
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

  if (__len < (__n + (__old_start - __old_finish)))
    __builtin_unreachable();


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
# 1263 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/vector.tcc" 3


}
# 75 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 87 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 88 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/vector" 2 3
# 10 "./tree-ssa/copyprop.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iostream" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iostream" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{








  template<class _CharT>
    struct char_traits;

  template<> struct char_traits<char>;

  template<> struct char_traits<wchar_t>;
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 3
namespace __cxx11 {

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
           typename _Alloc = allocator<_CharT> >
    class basic_string;

}


  typedef basic_string<char> string;


  typedef basic_string<wchar_t> wstring;
# 99 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/stringfwd.h" 3

}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 1 3
# 49 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
# 1 "/usr/include/wchar.h" 1 3 4
# 36 "/usr/include/wchar.h" 3 4
# 1 "/usr/include/stdio.h" 1 3 4
# 44 "/usr/include/stdio.h" 3 4
struct _IO_FILE;



typedef struct _IO_FILE FILE;





# 64 "/usr/include/stdio.h" 3 4
typedef struct _IO_FILE __FILE;
# 37 "/usr/include/wchar.h" 2 3 4


# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 1 3 4
# 40 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdarg.h" 3 4
typedef __builtin_va_list __gnuc_va_list;
# 40 "/usr/include/wchar.h" 2 3 4

# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 42 "/usr/include/wchar.h" 2 3 4
# 51 "/usr/include/wchar.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 359 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef unsigned int wint_t;
# 52 "/usr/include/wchar.h" 2 3 4
# 82 "/usr/include/wchar.h" 3 4
typedef struct
{
  int __count;
  union
  {

    unsigned int __wch;



    char __wchb[4];
  } __value;
} __mbstate_t;
# 104 "/usr/include/wchar.h" 3 4


typedef __mbstate_t mbstate_t;






# 132 "/usr/include/wchar.h" 3 4
extern "C" {




struct tm;









extern wchar_t *wcscpy (wchar_t *__restrict __dest,
   const wchar_t *__restrict __src) throw ();

extern wchar_t *wcsncpy (wchar_t *__restrict __dest,
    const wchar_t *__restrict __src, size_t __n)
     throw ();


extern wchar_t *wcscat (wchar_t *__restrict __dest,
   const wchar_t *__restrict __src) throw ();

extern wchar_t *wcsncat (wchar_t *__restrict __dest,
    const wchar_t *__restrict __src, size_t __n)
     throw ();


extern int wcscmp (const wchar_t *__s1, const wchar_t *__s2)
     throw () __attribute__ ((__pure__));

extern int wcsncmp (const wchar_t *__s1, const wchar_t *__s2, size_t __n)
     throw () __attribute__ ((__pure__));




extern int wcscasecmp (const wchar_t *__s1, const wchar_t *__s2) throw ();


extern int wcsncasecmp (const wchar_t *__s1, const wchar_t *__s2,
   size_t __n) throw ();



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
# 181 "/usr/include/wchar.h" 2 3 4

extern int wcscasecmp_l (const wchar_t *__s1, const wchar_t *__s2,
    __locale_t __loc) throw ();

extern int wcsncasecmp_l (const wchar_t *__s1, const wchar_t *__s2,
     size_t __n, __locale_t __loc) throw ();





extern int wcscoll (const wchar_t *__s1, const wchar_t *__s2) throw ();



extern size_t wcsxfrm (wchar_t *__restrict __s1,
         const wchar_t *__restrict __s2, size_t __n) throw ();








extern int wcscoll_l (const wchar_t *__s1, const wchar_t *__s2,
        __locale_t __loc) throw ();




extern size_t wcsxfrm_l (wchar_t *__s1, const wchar_t *__s2,
    size_t __n, __locale_t __loc) throw ();


extern wchar_t *wcsdup (const wchar_t *__s) throw () __attribute__ ((__malloc__));





extern "C++" wchar_t *wcschr (wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcschr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcschr (const wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcschr") __attribute__ ((__pure__));






extern "C++" wchar_t *wcsrchr (wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcsrchr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcsrchr (const wchar_t *__wcs, wchar_t __wc)
     throw () __asm ("wcsrchr") __attribute__ ((__pure__));









extern wchar_t *wcschrnul (const wchar_t *__s, wchar_t __wc)
     throw () __attribute__ ((__pure__));





extern size_t wcscspn (const wchar_t *__wcs, const wchar_t *__reject)
     throw () __attribute__ ((__pure__));


extern size_t wcsspn (const wchar_t *__wcs, const wchar_t *__accept)
     throw () __attribute__ ((__pure__));


extern "C++" wchar_t *wcspbrk (wchar_t *__wcs, const wchar_t *__accept)
     throw () __asm ("wcspbrk") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcspbrk (const wchar_t *__wcs,
         const wchar_t *__accept)
     throw () __asm ("wcspbrk") __attribute__ ((__pure__));






extern "C++" wchar_t *wcsstr (wchar_t *__haystack, const wchar_t *__needle)
     throw () __asm ("wcsstr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcsstr (const wchar_t *__haystack,
        const wchar_t *__needle)
     throw () __asm ("wcsstr") __attribute__ ((__pure__));






extern wchar_t *wcstok (wchar_t *__restrict __s,
   const wchar_t *__restrict __delim,
   wchar_t **__restrict __ptr) throw ();


extern size_t wcslen (const wchar_t *__s) throw () __attribute__ ((__pure__));





extern "C++" wchar_t *wcswcs (wchar_t *__haystack, const wchar_t *__needle)
     throw () __asm ("wcswcs") __attribute__ ((__pure__));
extern "C++" const wchar_t *wcswcs (const wchar_t *__haystack,
        const wchar_t *__needle)
     throw () __asm ("wcswcs") __attribute__ ((__pure__));
# 306 "/usr/include/wchar.h" 3 4
extern size_t wcsnlen (const wchar_t *__s, size_t __maxlen)
     throw () __attribute__ ((__pure__));






extern "C++" wchar_t *wmemchr (wchar_t *__s, wchar_t __c, size_t __n)
     throw () __asm ("wmemchr") __attribute__ ((__pure__));
extern "C++" const wchar_t *wmemchr (const wchar_t *__s, wchar_t __c,
         size_t __n)
     throw () __asm ("wmemchr") __attribute__ ((__pure__));






extern int wmemcmp (const wchar_t *__s1, const wchar_t *__s2, size_t __n)
     throw () __attribute__ ((__pure__));


extern wchar_t *wmemcpy (wchar_t *__restrict __s1,
    const wchar_t *__restrict __s2, size_t __n) throw ();



extern wchar_t *wmemmove (wchar_t *__s1, const wchar_t *__s2, size_t __n)
     throw ();


extern wchar_t *wmemset (wchar_t *__s, wchar_t __c, size_t __n) throw ();





extern wchar_t *wmempcpy (wchar_t *__restrict __s1,
     const wchar_t *__restrict __s2, size_t __n)
     throw ();






extern wint_t btowc (int __c) throw ();



extern int wctob (wint_t __c) throw ();



extern int mbsinit (const mbstate_t *__ps) throw () __attribute__ ((__pure__));



extern size_t mbrtowc (wchar_t *__restrict __pwc,
         const char *__restrict __s, size_t __n,
         mbstate_t *__restrict __p) throw ();


extern size_t wcrtomb (char *__restrict __s, wchar_t __wc,
         mbstate_t *__restrict __ps) throw ();


extern size_t __mbrlen (const char *__restrict __s, size_t __n,
   mbstate_t *__restrict __ps) throw ();
extern size_t mbrlen (const char *__restrict __s, size_t __n,
        mbstate_t *__restrict __ps) throw ();

# 405 "/usr/include/wchar.h" 3 4



extern size_t mbsrtowcs (wchar_t *__restrict __dst,
    const char **__restrict __src, size_t __len,
    mbstate_t *__restrict __ps) throw ();



extern size_t wcsrtombs (char *__restrict __dst,
    const wchar_t **__restrict __src, size_t __len,
    mbstate_t *__restrict __ps) throw ();






extern size_t mbsnrtowcs (wchar_t *__restrict __dst,
     const char **__restrict __src, size_t __nmc,
     size_t __len, mbstate_t *__restrict __ps) throw ();



extern size_t wcsnrtombs (char *__restrict __dst,
     const wchar_t **__restrict __src,
     size_t __nwc, size_t __len,
     mbstate_t *__restrict __ps) throw ();






extern int wcwidth (wchar_t __c) throw ();



extern int wcswidth (const wchar_t *__s, size_t __n) throw ();






extern double wcstod (const wchar_t *__restrict __nptr,
        wchar_t **__restrict __endptr) throw ();





extern float wcstof (const wchar_t *__restrict __nptr,
       wchar_t **__restrict __endptr) throw ();
extern long double wcstold (const wchar_t *__restrict __nptr,
       wchar_t **__restrict __endptr) throw ();







extern long int wcstol (const wchar_t *__restrict __nptr,
   wchar_t **__restrict __endptr, int __base) throw ();



extern unsigned long int wcstoul (const wchar_t *__restrict __nptr,
      wchar_t **__restrict __endptr, int __base)
     throw ();






__extension__
extern long long int wcstoll (const wchar_t *__restrict __nptr,
         wchar_t **__restrict __endptr, int __base)
     throw ();



__extension__
extern unsigned long long int wcstoull (const wchar_t *__restrict __nptr,
     wchar_t **__restrict __endptr,
     int __base) throw ();






__extension__
extern long long int wcstoq (const wchar_t *__restrict __nptr,
        wchar_t **__restrict __endptr, int __base)
     throw ();



__extension__
extern unsigned long long int wcstouq (const wchar_t *__restrict __nptr,
           wchar_t **__restrict __endptr,
           int __base) throw ();
# 530 "/usr/include/wchar.h" 3 4
extern long int wcstol_l (const wchar_t *__restrict __nptr,
     wchar_t **__restrict __endptr, int __base,
     __locale_t __loc) throw ();

extern unsigned long int wcstoul_l (const wchar_t *__restrict __nptr,
        wchar_t **__restrict __endptr,
        int __base, __locale_t __loc) throw ();

__extension__
extern long long int wcstoll_l (const wchar_t *__restrict __nptr,
    wchar_t **__restrict __endptr,
    int __base, __locale_t __loc) throw ();

__extension__
extern unsigned long long int wcstoull_l (const wchar_t *__restrict __nptr,
       wchar_t **__restrict __endptr,
       int __base, __locale_t __loc)
     throw ();

extern double wcstod_l (const wchar_t *__restrict __nptr,
   wchar_t **__restrict __endptr, __locale_t __loc)
     throw ();

extern float wcstof_l (const wchar_t *__restrict __nptr,
         wchar_t **__restrict __endptr, __locale_t __loc)
     throw ();

extern long double wcstold_l (const wchar_t *__restrict __nptr,
         wchar_t **__restrict __endptr,
         __locale_t __loc) throw ();






extern wchar_t *wcpcpy (wchar_t *__restrict __dest,
   const wchar_t *__restrict __src) throw ();



extern wchar_t *wcpncpy (wchar_t *__restrict __dest,
    const wchar_t *__restrict __src, size_t __n)
     throw ();






extern __FILE *open_wmemstream (wchar_t **__bufloc, size_t *__sizeloc) throw ();






extern int fwide (__FILE *__fp, int __mode) throw ();






extern int fwprintf (__FILE *__restrict __stream,
       const wchar_t *__restrict __format, ...)
                                                           ;




extern int wprintf (const wchar_t *__restrict __format, ...)
                                                           ;

extern int swprintf (wchar_t *__restrict __s, size_t __n,
       const wchar_t *__restrict __format, ...)
     throw () ;





extern int vfwprintf (__FILE *__restrict __s,
        const wchar_t *__restrict __format,
        __gnuc_va_list __arg)
                                                           ;




extern int vwprintf (const wchar_t *__restrict __format,
       __gnuc_va_list __arg)
                                                           ;


extern int vswprintf (wchar_t *__restrict __s, size_t __n,
        const wchar_t *__restrict __format,
        __gnuc_va_list __arg)
     throw () ;






extern int fwscanf (__FILE *__restrict __stream,
      const wchar_t *__restrict __format, ...)
                                                          ;




extern int wscanf (const wchar_t *__restrict __format, ...)
                                                          ;

extern int swscanf (const wchar_t *__restrict __s,
      const wchar_t *__restrict __format, ...)
     throw () ;
# 680 "/usr/include/wchar.h" 3 4









extern int vfwscanf (__FILE *__restrict __s,
       const wchar_t *__restrict __format,
       __gnuc_va_list __arg)
                                                          ;




extern int vwscanf (const wchar_t *__restrict __format,
      __gnuc_va_list __arg)
                                                          ;

extern int vswscanf (const wchar_t *__restrict __s,
       const wchar_t *__restrict __format,
       __gnuc_va_list __arg)
     throw () ;
# 736 "/usr/include/wchar.h" 3 4









extern wint_t fgetwc (__FILE *__stream);
extern wint_t getwc (__FILE *__stream);





extern wint_t getwchar (void);






extern wint_t fputwc (wchar_t __wc, __FILE *__stream);
extern wint_t putwc (wchar_t __wc, __FILE *__stream);





extern wint_t putwchar (wchar_t __wc);







extern wchar_t *fgetws (wchar_t *__restrict __ws, int __n,
   __FILE *__restrict __stream);





extern int fputws (const wchar_t *__restrict __ws,
     __FILE *__restrict __stream);






extern wint_t ungetwc (wint_t __wc, __FILE *__stream);

# 801 "/usr/include/wchar.h" 3 4
extern wint_t getwc_unlocked (__FILE *__stream);
extern wint_t getwchar_unlocked (void);







extern wint_t fgetwc_unlocked (__FILE *__stream);







extern wint_t fputwc_unlocked (wchar_t __wc, __FILE *__stream);
# 827 "/usr/include/wchar.h" 3 4
extern wint_t putwc_unlocked (wchar_t __wc, __FILE *__stream);
extern wint_t putwchar_unlocked (wchar_t __wc);
# 837 "/usr/include/wchar.h" 3 4
extern wchar_t *fgetws_unlocked (wchar_t *__restrict __ws, int __n,
     __FILE *__restrict __stream);







extern int fputws_unlocked (const wchar_t *__restrict __ws,
       __FILE *__restrict __stream);







extern size_t wcsftime (wchar_t *__restrict __s, size_t __maxsize,
   const wchar_t *__restrict __format,
   const struct tm *__restrict __tp) throw ();







extern size_t wcsftime_l (wchar_t *__restrict __s, size_t __maxsize,
     const wchar_t *__restrict __format,
     const struct tm *__restrict __tp,
     __locale_t __loc) throw ();
# 891 "/usr/include/wchar.h" 3 4
}
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 2 3
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
namespace std
{
  using ::mbstate_t;
}
# 137 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
extern "C++"
{
namespace std __attribute__ ((__visibility__ ("default")))
{


  using ::wint_t;

  using ::btowc;
  using ::fgetwc;
  using ::fgetws;
  using ::fputwc;
  using ::fputws;
  using ::fwide;
  using ::fwprintf;
  using ::fwscanf;
  using ::getwc;
  using ::getwchar;
  using ::mbrlen;
  using ::mbrtowc;
  using ::mbsinit;
  using ::mbsrtowcs;
  using ::putwc;
  using ::putwchar;

  using ::swprintf;

  using ::swscanf;
  using ::ungetwc;
  using ::vfwprintf;

  using ::vfwscanf;


  using ::vswprintf;


  using ::vswscanf;

  using ::vwprintf;

  using ::vwscanf;

  using ::wcrtomb;
  using ::wcscat;
  using ::wcscmp;
  using ::wcscoll;
  using ::wcscpy;
  using ::wcscspn;
  using ::wcsftime;
  using ::wcslen;
  using ::wcsncat;
  using ::wcsncmp;
  using ::wcsncpy;
  using ::wcsrtombs;
  using ::wcsspn;
  using ::wcstod;

  using ::wcstof;

  using ::wcstok;
  using ::wcstol;
  using ::wcstoul;
  using ::wcsxfrm;
  using ::wctob;
  using ::wmemcmp;
  using ::wmemcpy;
  using ::wmemmove;
  using ::wmemset;
  using ::wprintf;
  using ::wscanf;
  using ::wcschr;
  using ::wcspbrk;
  using ::wcsrchr;
  using ::wcsstr;
  using ::wmemchr;
# 236 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3

}
}







namespace __gnu_cxx
{





  using ::wcstold;
# 262 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwchar" 3
  using ::wcstoll;
  using ::wcstoull;

}

namespace std
{
  using ::__gnu_cxx::wcstold;
  using ::__gnu_cxx::wcstoll;
  using ::__gnu_cxx::wcstoull;
}
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
 
# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
  typedef long int streamoff;



#pragma GCC diagnostic pop


  typedef ptrdiff_t streamsize;
# 86 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
  template<typename _StateT>
    class fpos
    {
    private:
      streamoff _M_off;
      _StateT _M_state;

    public:




      fpos()
      : _M_off(0), _M_state() { }
# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
      fpos(streamoff __off)
      : _M_off(__off), _M_state() { }
# 118 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3
      operator streamoff() const { return _M_off; }


      void
      state(_StateT __st)
      { _M_state = __st; }


      _StateT
      state() const
      { return _M_state; }





      fpos&
      operator+=(streamoff __off)
      {
 _M_off += __off;
 return *this;
      }





      fpos&
      operator-=(streamoff __off)
      {
 _M_off -= __off;
 return *this;
      }







      fpos
      operator+(streamoff __off) const
      {
 fpos __pos(*this);
 __pos += __off;
 return __pos;
      }







      fpos
      operator-(streamoff __off) const
      {
 fpos __pos(*this);
 __pos -= __off;
 return __pos;
      }






      streamoff
      operator-(const fpos& __other) const
      { return _M_off - __other._M_off; }
    };






  template<typename _StateT>
    inline bool
    operator==(const fpos<_StateT>& __lhs, const fpos<_StateT>& __rhs)
    { return streamoff(__lhs) == streamoff(__rhs); }

  template<typename _StateT>
    inline bool
    operator!=(const fpos<_StateT>& __lhs, const fpos<_StateT>& __rhs)
    { return streamoff(__lhs) != streamoff(__rhs); }





  typedef fpos<mbstate_t> streampos;

  typedef fpos<mbstate_t> wstreampos;
# 225 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/postypes.h" 3

}
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 78 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 3
  class ios_base;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ios;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_streambuf;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_istream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ostream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_iostream;


namespace __cxx11 {

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
     typename _Alloc = allocator<_CharT> >
    class basic_stringbuf;

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
    typename _Alloc = allocator<_CharT> >
    class basic_istringstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
    typename _Alloc = allocator<_CharT> >
    class basic_ostringstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT>,
    typename _Alloc = allocator<_CharT> >
    class basic_stringstream;

}

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_filebuf;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ifstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_ofstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class basic_fstream;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class istreambuf_iterator;

  template<typename _CharT, typename _Traits = char_traits<_CharT> >
    class ostreambuf_iterator;



  typedef basic_ios<char> ios;


  typedef basic_streambuf<char> streambuf;


  typedef basic_istream<char> istream;


  typedef basic_ostream<char> ostream;


  typedef basic_iostream<char> iostream;


  typedef basic_stringbuf<char> stringbuf;


  typedef basic_istringstream<char> istringstream;


  typedef basic_ostringstream<char> ostringstream;


  typedef basic_stringstream<char> stringstream;


  typedef basic_filebuf<char> filebuf;


  typedef basic_ifstream<char> ifstream;


  typedef basic_ofstream<char> ofstream;


  typedef basic_fstream<char> fstream;



  typedef basic_ios<wchar_t> wios;


  typedef basic_streambuf<wchar_t> wstreambuf;


  typedef basic_istream<wchar_t> wistream;


  typedef basic_ostream<wchar_t> wostream;


  typedef basic_iostream<wchar_t> wiostream;


  typedef basic_stringbuf<wchar_t> wstringbuf;


  typedef basic_istringstream<wchar_t> wistringstream;


  typedef basic_ostringstream<wchar_t> wostringstream;


  typedef basic_stringstream<wchar_t> wstringstream;


  typedef basic_filebuf<wchar_t> wfilebuf;


  typedef basic_ifstream<wchar_t> wifstream;


  typedef basic_ofstream<wchar_t> wofstream;


  typedef basic_fstream<wchar_t> wfstream;
# 257 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iosfwd" 3

}
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 2 3

extern "C++" {

namespace std __attribute__ ((__visibility__ ("default")))
{
# 56 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
  class bad_exception : public exception
  {
  public:
    bad_exception() throw() { }



    virtual ~bad_exception() throw();


    virtual const char*
    what() const throw();
  };


  typedef void (*terminate_handler) ();


  terminate_handler set_terminate(terminate_handler) throw();
# 83 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
  void terminate() throw() __attribute__ ((__noreturn__,__cold__));



  typedef void (* unexpected_handler) ();





 
  unexpected_handler set_unexpected(unexpected_handler) throw();
# 111 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
 
  void unexpected() __attribute__ ((__noreturn__,__cold__));
# 126 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
 
  bool uncaught_exception() throw() __attribute__ ((__pure__));
# 138 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
}

namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{

# 160 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/exception" 3
  void __verbose_terminate_handler();


}

}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 1 3
# 66 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{


 
# 70 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wstringop-overflow"
#pragma GCC diagnostic ignored "-Wstringop-overread"
#pragma GCC diagnostic ignored "-Warray-bounds"
# 85 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
  template<typename _CharT>
    struct _Char_types
    {
      typedef unsigned long int_type;

      typedef std::streampos pos_type;
      typedef std::streamoff off_type;
      typedef std::mbstate_t state_type;

    };
# 112 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
  template<typename _CharT>
    struct char_traits
    {
      typedef _CharT char_type;
      typedef typename _Char_types<_CharT>::int_type int_type;

      typedef typename _Char_types<_CharT>::pos_type pos_type;
      typedef typename _Char_types<_CharT>::off_type off_type;
      typedef typename _Char_types<_CharT>::state_type state_type;





      static void
      assign(char_type& __c1, const char_type& __c2)
      {





 __c1 = __c2;
      }

      static bool
      eq(const char_type& __c1, const char_type& __c2)
      { return __c1 == __c2; }

      static bool
      lt(const char_type& __c1, const char_type& __c2)
      { return __c1 < __c2; }

      static int
      compare(const char_type* __s1, const char_type* __s2, std::size_t __n);

      static std::size_t
      length(const char_type* __s);

      static const char_type*
      find(const char_type* __s, std::size_t __n, const char_type& __a);

      static char_type*
      move(char_type* __s1, const char_type* __s2, std::size_t __n);

      static char_type*
      copy(char_type* __s1, const char_type* __s2, std::size_t __n);

      static char_type*
      assign(char_type* __s, std::size_t __n, char_type __a);

      static char_type
      to_char_type(const int_type& __c)
      { return static_cast<char_type>(__c); }

      static int_type
      to_int_type(const char_type& __c)
      { return static_cast<int_type>(__c); }

      static bool
      eq_int_type(const int_type& __c1, const int_type& __c2)
      { return __c1 == __c2; }


      static int_type
      eof()
      { return static_cast<int_type>(-1); }

      static int_type
      not_eof(const int_type& __c)
      { return !eq_int_type(__c, eof()) ? __c : to_int_type(char_type()); }

    };

  template<typename _CharT>
    int
    char_traits<_CharT>::
    compare(const char_type* __s1, const char_type* __s2, std::size_t __n)
    {
      for (std::size_t __i = 0; __i < __n; ++__i)
 if (lt(__s1[__i], __s2[__i]))
   return -1;
 else if (lt(__s2[__i], __s1[__i]))
   return 1;
      return 0;
    }

  template<typename _CharT>
    std::size_t
    char_traits<_CharT>::
    length(const char_type* __p)
    {
      std::size_t __i = 0;
      while (!eq(__p[__i], char_type()))
        ++__i;
      return __i;
    }

  template<typename _CharT>
    const typename char_traits<_CharT>::char_type*
    char_traits<_CharT>::
    find(const char_type* __s, std::size_t __n, const char_type& __a)
    {
      for (std::size_t __i = 0; __i < __n; ++__i)
        if (eq(__s[__i], __a))
          return __s + __i;
      return 0;
    }

  template<typename _CharT>
   
    typename char_traits<_CharT>::char_type*
    char_traits<_CharT>::
    move(char_type* __s1, const char_type* __s2, std::size_t __n)
    {
      if (__n == 0)
 return __s1;
# 248 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
      __builtin_memmove(__s1, __s2, __n * sizeof(char_type));
      return __s1;
    }

  template<typename _CharT>
   
    typename char_traits<_CharT>::char_type*
    char_traits<_CharT>::
    copy(char_type* __s1, const char_type* __s2, std::size_t __n)
    {
      if (__n == 0)
 return __s1;
# 268 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
      __builtin_memcpy(__s1, __s2, __n * sizeof(char_type));
      return __s1;
    }

  template<typename _CharT>
   
    typename char_traits<_CharT>::char_type*
    char_traits<_CharT>::
    assign(char_type* __s, std::size_t __n, char_type __a)
    {
# 287 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
      if (sizeof(_CharT) == 1 && __is_trivial(_CharT))
 {
   if (__n)
     {
       unsigned char __c;
       __builtin_memcpy(&__c, __builtin_addressof(__a), 1);
       __builtin_memset(__s, __c, __n);
     }
 }
      else
 {
   for (std::size_t __i = 0; __i < __n; ++__i)
     __s[__i] = __a;
 }
      return __s;
    }


}

namespace std __attribute__ ((__visibility__ ("default")))
{

# 324 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
  template<typename _CharT>
    struct char_traits : public __gnu_cxx::char_traits<_CharT>
    { };



  template<>
    struct char_traits<char>
    {
      typedef char char_type;
      typedef int int_type;

      typedef streampos pos_type;
      typedef streamoff off_type;
      typedef mbstate_t state_type;





      static void
      assign(char_type& __c1, const char_type& __c2)
      {





 __c1 = __c2;
      }

      static bool
      eq(const char_type& __c1, const char_type& __c2)
      { return __c1 == __c2; }

      static bool
      lt(const char_type& __c1, const char_type& __c2)
      {

 return (static_cast<unsigned char>(__c1)
  < static_cast<unsigned char>(__c2));
      }

      static int
      compare(const char_type* __s1, const char_type* __s2, size_t __n)
      {
 if (__n == 0)
   return 0;
# 383 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3
 return __builtin_memcmp(__s1, __s2, __n);
      }

      static size_t
      length(const char_type* __s)
      {




 return __builtin_strlen(__s);
      }

      static const char_type*
      find(const char_type* __s, size_t __n, const char_type& __a)
      {
 if (__n == 0)
   return 0;




 return static_cast<const char_type*>(__builtin_memchr(__s, __a, __n));
      }

      static char_type*
      move(char_type* __s1, const char_type* __s2, size_t __n)
      {
 if (__n == 0)
   return __s1;




 return static_cast<char_type*>(__builtin_memmove(__s1, __s2, __n));
      }

      static char_type*
      copy(char_type* __s1, const char_type* __s2, size_t __n)
      {
 if (__n == 0)
   return __s1;




 return static_cast<char_type*>(__builtin_memcpy(__s1, __s2, __n));
      }

      static char_type*
      assign(char_type* __s, size_t __n, char_type __a)
      {
 if (__n == 0)
   return __s;




 return static_cast<char_type*>(__builtin_memset(__s, __a, __n));
      }

      static char_type
      to_char_type(const int_type& __c)
      { return static_cast<char_type>(__c); }



      static int_type
      to_int_type(const char_type& __c)
      { return static_cast<int_type>(static_cast<unsigned char>(__c)); }

      static bool
      eq_int_type(const int_type& __c1, const int_type& __c2)
      { return __c1 == __c2; }


      static int_type
      eof()
      { return static_cast<int_type>(-1); }

      static int_type
      not_eof(const int_type& __c)
      { return (__c == eof()) ? 0 : __c; }

  };




  template<>
    struct char_traits<wchar_t>
    {
      typedef wchar_t char_type;
      typedef wint_t int_type;

      typedef streamoff off_type;
      typedef wstreampos pos_type;
      typedef mbstate_t state_type;





      static void
      assign(char_type& __c1, const char_type& __c2)
      {





 __c1 = __c2;
      }

      static bool
      eq(const char_type& __c1, const char_type& __c2)
      { return __c1 == __c2; }

      static bool
      lt(const char_type& __c1, const char_type& __c2)
      { return __c1 < __c2; }

      static int
      compare(const char_type* __s1, const char_type* __s2, size_t __n)
      {
 if (__n == 0)
   return 0;




 return wmemcmp(__s1, __s2, __n);
      }

      static size_t
      length(const char_type* __s)
      {




 return wcslen(__s);
      }

      static const char_type*
      find(const char_type* __s, size_t __n, const char_type& __a)
      {
 if (__n == 0)
   return 0;




 return wmemchr(__s, __a, __n);
      }

      static char_type*
      move(char_type* __s1, const char_type* __s2, size_t __n)
      {
 if (__n == 0)
   return __s1;




 return wmemmove(__s1, __s2, __n);
      }

      static char_type*
      copy(char_type* __s1, const char_type* __s2, size_t __n)
      {
 if (__n == 0)
   return __s1;




 return wmemcpy(__s1, __s2, __n);
      }

      static char_type*
      assign(char_type* __s, size_t __n, char_type __a)
      {
 if (__n == 0)
   return __s;




 return wmemset(__s, __a, __n);
      }

      static char_type
      to_char_type(const int_type& __c)
      { return char_type(__c); }

      static int_type
      to_int_type(const char_type& __c)
      { return int_type(__c); }

      static bool
      eq_int_type(const int_type& __c1, const int_type& __c2)
      { return __c1 == __c2; }


      static int_type
      eof()
      { return static_cast<int_type>((0xffffffffu)); }

      static int_type
      not_eof(const int_type& __c)
      { return eq_int_type(__c, eof()) ? 0 : __c; }

  };
# 731 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/char_traits.h" 3

}
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/localefwd.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/localefwd.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++locale.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++locale.h" 3
       
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++locale.h" 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/clocale" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/clocale" 3
# 1 "/usr/include/locale.h" 1 3 4
# 29 "/usr/include/locale.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 30 "/usr/include/locale.h" 2 3 4
# 1 "/usr/include/bits/locale.h" 1 3 4
# 31 "/usr/include/locale.h" 2 3 4

extern "C" {
# 51 "/usr/include/locale.h" 3 4



struct lconv
{


  char *decimal_point;
  char *thousands_sep;





  char *grouping;





  char *int_curr_symbol;
  char *currency_symbol;
  char *mon_decimal_point;
  char *mon_thousands_sep;
  char *mon_grouping;
  char *positive_sign;
  char *negative_sign;
  char int_frac_digits;
  char frac_digits;

  char p_cs_precedes;

  char p_sep_by_space;

  char n_cs_precedes;

  char n_sep_by_space;






  char p_sign_posn;
  char n_sign_posn;


  char int_p_cs_precedes;

  char int_p_sep_by_space;

  char int_n_cs_precedes;

  char int_n_sep_by_space;






  char int_p_sign_posn;
  char int_n_sign_posn;
# 121 "/usr/include/locale.h" 3 4
};



extern char *setlocale (int __category, const char *__locale) throw ();


extern struct lconv *localeconv (void) throw ();


# 152 "/usr/include/locale.h" 3 4
extern __locale_t newlocale (int __category_mask, const char *__locale,
        __locale_t __base) throw ();
# 187 "/usr/include/locale.h" 3 4
extern __locale_t duplocale (__locale_t __dataset) throw ();



extern void freelocale (__locale_t __dataset) throw ();






extern __locale_t uselocale (__locale_t __dataset) throw ();







}
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/clocale" 2 3





namespace std
{
  using ::lconv;
  using ::setlocale;
  using ::localeconv;
}
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++locale.h" 2 3






namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{


  extern "C" __typeof(uselocale) __uselocale;


}


namespace std __attribute__ ((__visibility__ ("default")))
{


  typedef __locale_t __c_locale;
# 73 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++locale.h" 3
  inline int
  __convert_from_v(const __c_locale& __cloc __attribute__ ((__unused__)),
     char* __out,
     const int __size __attribute__ ((__unused__)),
     const char* __fmt, ...)
  {

    __c_locale __old = __gnu_cxx::__uselocale(__cloc);
# 93 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/c++locale.h" 3
    __builtin_va_list __args;
    __builtin_va_start(__args, __fmt);


    const int __ret = __builtin_vsnprintf(__out, __size, __fmt, __args);




    __builtin_va_end(__args);


    __gnu_cxx::__uselocale(__old);







    return __ret;
  }







}
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/localefwd.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cctype" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cctype" 3
# 1 "/usr/include/ctype.h" 1 3 4
# 27 "/usr/include/ctype.h" 3 4
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
# 28 "/usr/include/ctype.h" 2 3 4

extern "C" {
# 40 "/usr/include/ctype.h" 3 4
# 1 "/usr/include/endian.h" 1 3 4
# 36 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/endian.h" 1 3 4
# 37 "/usr/include/endian.h" 2 3 4
# 60 "/usr/include/endian.h" 3 4
# 1 "/usr/include/bits/byteswap.h" 1 3 4
# 28 "/usr/include/bits/byteswap.h" 3 4
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
# 41 "/usr/include/ctype.h" 2 3 4






enum
{
  _ISupper = ((0) < 8 ? ((1 << (0)) << 8) : ((1 << (0)) >> 8)),
  _ISlower = ((1) < 8 ? ((1 << (1)) << 8) : ((1 << (1)) >> 8)),
  _ISalpha = ((2) < 8 ? ((1 << (2)) << 8) : ((1 << (2)) >> 8)),
  _ISdigit = ((3) < 8 ? ((1 << (3)) << 8) : ((1 << (3)) >> 8)),
  _ISxdigit = ((4) < 8 ? ((1 << (4)) << 8) : ((1 << (4)) >> 8)),
  _ISspace = ((5) < 8 ? ((1 << (5)) << 8) : ((1 << (5)) >> 8)),
  _ISprint = ((6) < 8 ? ((1 << (6)) << 8) : ((1 << (6)) >> 8)),
  _ISgraph = ((7) < 8 ? ((1 << (7)) << 8) : ((1 << (7)) >> 8)),
  _ISblank = ((8) < 8 ? ((1 << (8)) << 8) : ((1 << (8)) >> 8)),
  _IScntrl = ((9) < 8 ? ((1 << (9)) << 8) : ((1 << (9)) >> 8)),
  _ISpunct = ((10) < 8 ? ((1 << (10)) << 8) : ((1 << (10)) >> 8)),
  _ISalnum = ((11) < 8 ? ((1 << (11)) << 8) : ((1 << (11)) >> 8))
};
# 80 "/usr/include/ctype.h" 3 4
extern const unsigned short int **__ctype_b_loc (void)
     throw () __attribute__ ((__const__));
extern const __int32_t **__ctype_tolower_loc (void)
     throw () __attribute__ ((__const__));
extern const __int32_t **__ctype_toupper_loc (void)
     throw () __attribute__ ((__const__));
# 105 "/usr/include/ctype.h" 3 4






extern int isalnum (int) throw ();
extern int isalpha (int) throw ();
extern int iscntrl (int) throw ();
extern int isdigit (int) throw ();
extern int islower (int) throw ();
extern int isgraph (int) throw ();
extern int isprint (int) throw ();
extern int ispunct (int) throw ();
extern int isspace (int) throw ();
extern int isupper (int) throw ();
extern int isxdigit (int) throw ();



extern int tolower (int __c) throw ();


extern int toupper (int __c) throw ();








extern int isblank (int) throw ();






extern int isctype (int __c, int __mask) throw ();






extern int isascii (int __c) throw ();



extern int toascii (int __c) throw ();



extern int _toupper (int) throw ();
extern int _tolower (int) throw ();
# 272 "/usr/include/ctype.h" 3 4
extern int isalnum_l (int, __locale_t) throw ();
extern int isalpha_l (int, __locale_t) throw ();
extern int iscntrl_l (int, __locale_t) throw ();
extern int isdigit_l (int, __locale_t) throw ();
extern int islower_l (int, __locale_t) throw ();
extern int isgraph_l (int, __locale_t) throw ();
extern int isprint_l (int, __locale_t) throw ();
extern int ispunct_l (int, __locale_t) throw ();
extern int isspace_l (int, __locale_t) throw ();
extern int isupper_l (int, __locale_t) throw ();
extern int isxdigit_l (int, __locale_t) throw ();

extern int isblank_l (int, __locale_t) throw ();



extern int __tolower_l (int __c, __locale_t __l) throw ();
extern int tolower_l (int __c, __locale_t __l) throw ();


extern int __toupper_l (int __c, __locale_t __l) throw ();
extern int toupper_l (int __c, __locale_t __l) throw ();
# 348 "/usr/include/ctype.h" 3 4
}
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cctype" 2 3
# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cctype" 3
namespace std
{
  using ::isalnum;
  using ::isalpha;
  using ::iscntrl;
  using ::isdigit;
  using ::isgraph;
  using ::islower;
  using ::isprint;
  using ::ispunct;
  using ::isspace;
  using ::isupper;
  using ::isxdigit;
  using ::tolower;
  using ::toupper;
}
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/localefwd.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 57 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/localefwd.h" 3
  class locale;

  template<typename _Facet>
    bool
    has_facet(const locale&) throw();

  template<typename _Facet>
    const _Facet&
    use_facet(const locale&);


  template<typename _CharT>
    bool
    isspace(_CharT, const locale&);

  template<typename _CharT>
    bool
    isprint(_CharT, const locale&);

  template<typename _CharT>
    bool
    iscntrl(_CharT, const locale&);

  template<typename _CharT>
    bool
    isupper(_CharT, const locale&);

  template<typename _CharT>
    bool
    islower(_CharT, const locale&);

  template<typename _CharT>
    bool
    isalpha(_CharT, const locale&);

  template<typename _CharT>
    bool
    isdigit(_CharT, const locale&);

  template<typename _CharT>
    bool
    ispunct(_CharT, const locale&);

  template<typename _CharT>
    bool
    isxdigit(_CharT, const locale&);

  template<typename _CharT>
    bool
    isalnum(_CharT, const locale&);

  template<typename _CharT>
    bool
    isgraph(_CharT, const locale&);







  template<typename _CharT>
    _CharT
    toupper(_CharT, const locale&);

  template<typename _CharT>
    _CharT
    tolower(_CharT, const locale&);


  struct ctype_base;
  template<typename _CharT>
    class ctype;
  template<> class ctype<char>;

  template<> class ctype<wchar_t>;

  template<typename _CharT>
    class ctype_byname;


  class codecvt_base;
  template<typename _InternT, typename _ExternT, typename _StateT>
    class codecvt;
  template<> class codecvt<char, char, mbstate_t>;

  template<> class codecvt<wchar_t, char, mbstate_t>;
# 153 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/localefwd.h" 3
  template<typename _InternT, typename _ExternT, typename _StateT>
    class codecvt_byname;



  template<typename _CharT, typename _InIter = istreambuf_iterator<_CharT> >
    class num_get;
  template<typename _CharT, typename _OutIter = ostreambuf_iterator<_CharT> >
    class num_put;

namespace __cxx11 {
  template<typename _CharT> class numpunct;
  template<typename _CharT> class numpunct_byname;
}

namespace __cxx11 {

  template<typename _CharT>
    class collate;
  template<typename _CharT>
    class collate_byname;
}


  class time_base;
namespace __cxx11 {
  template<typename _CharT, typename _InIter = istreambuf_iterator<_CharT> >
    class time_get;
  template<typename _CharT, typename _InIter = istreambuf_iterator<_CharT> >
    class time_get_byname;
}
  template<typename _CharT, typename _OutIter = ostreambuf_iterator<_CharT> >
    class time_put;
  template<typename _CharT, typename _OutIter = ostreambuf_iterator<_CharT> >
    class time_put_byname;


  class money_base;
namespace __cxx11 {
  template<typename _CharT, typename _InIter = istreambuf_iterator<_CharT> >
    class money_get;
  template<typename _CharT, typename _OutIter = ostreambuf_iterator<_CharT> >
    class money_put;
}
namespace __cxx11 {
  template<typename _CharT, bool _Intl = false>
    class moneypunct;
  template<typename _CharT, bool _Intl = false>
    class moneypunct_byname;
}


  struct messages_base;
namespace __cxx11 {
  template<typename _CharT>
    class messages;
  template<typename _CharT>
    class messages_byname;
}


}
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 1 3
# 30 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 3
#pragma GCC visibility push(default)
# 157 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 1 3
# 35 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
# 1 "/usr/include/pthread.h" 1 3 4
# 23 "/usr/include/pthread.h" 3 4
# 1 "/usr/include/sched.h" 1 3 4
# 29 "/usr/include/sched.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 30 "/usr/include/sched.h" 2 3 4



# 1 "/usr/include/time.h" 1 3 4
# 73 "/usr/include/time.h" 3 4


typedef __time_t time_t;



# 120 "/usr/include/time.h" 3 4
struct timespec
  {
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
  };
# 34 "/usr/include/sched.h" 2 3 4


typedef __pid_t pid_t;





# 1 "/usr/include/bits/sched.h" 1 3 4
# 73 "/usr/include/bits/sched.h" 3 4
struct sched_param
  {
    int __sched_priority;
  };

extern "C" {



extern int clone (int (*__fn) (void *__arg), void *__child_stack,
    int __flags, void *__arg, ...) throw ();


extern int unshare (int __flags) throw ();


extern int sched_getcpu (void) throw ();


extern int setns (int __fd, int __nstype) throw ();



}







struct __sched_param
  {
    int __sched_priority;
  };
# 119 "/usr/include/bits/sched.h" 3 4
typedef unsigned long int __cpu_mask;






typedef struct
{
  __cpu_mask __bits[1024 / (8 * sizeof (__cpu_mask))];
} cpu_set_t;
# 202 "/usr/include/bits/sched.h" 3 4
extern "C" {

extern int __sched_cpucount (size_t __setsize, const cpu_set_t *__setp)
  throw ();
extern cpu_set_t *__sched_cpualloc (size_t __count) throw () ;
extern void __sched_cpufree (cpu_set_t *__set) throw ();

}
# 43 "/usr/include/sched.h" 2 3 4




extern "C" {


extern int sched_setparam (__pid_t __pid, const struct sched_param *__param)
     throw ();


extern int sched_getparam (__pid_t __pid, struct sched_param *__param) throw ();


extern int sched_setscheduler (__pid_t __pid, int __policy,
          const struct sched_param *__param) throw ();


extern int sched_getscheduler (__pid_t __pid) throw ();


extern int sched_yield (void) throw ();


extern int sched_get_priority_max (int __algorithm) throw ();


extern int sched_get_priority_min (int __algorithm) throw ();


extern int sched_rr_get_interval (__pid_t __pid, struct timespec *__t) throw ();
# 117 "/usr/include/sched.h" 3 4
extern int sched_setaffinity (__pid_t __pid, size_t __cpusetsize,
         const cpu_set_t *__cpuset) throw ();


extern int sched_getaffinity (__pid_t __pid, size_t __cpusetsize,
         cpu_set_t *__cpuset) throw ();


}
# 24 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/time.h" 1 3 4
# 29 "/usr/include/time.h" 3 4
extern "C" {







# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 38 "/usr/include/time.h" 2 3 4



# 1 "/usr/include/bits/time.h" 1 3 4
# 30 "/usr/include/bits/time.h" 3 4
struct timeval
  {
    __time_t tv_sec;
    __suseconds_t tv_usec;
  };
# 88 "/usr/include/bits/time.h" 3 4
# 1 "/usr/include/bits/timex.h" 1 3 4
# 25 "/usr/include/bits/timex.h" 3 4
struct timex
{
  unsigned int modes;
  __syscall_slong_t offset;
  __syscall_slong_t freq;
  __syscall_slong_t maxerror;
  __syscall_slong_t esterror;
  int status;
  __syscall_slong_t constant;
  __syscall_slong_t precision;
  __syscall_slong_t tolerance;
  struct timeval time;
  __syscall_slong_t tick;
  __syscall_slong_t ppsfreq;
  __syscall_slong_t jitter;
  int shift;
  __syscall_slong_t stabil;
  __syscall_slong_t jitcnt;
  __syscall_slong_t calcnt;
  __syscall_slong_t errcnt;
  __syscall_slong_t stbcnt;

  int tai;


  int :32; int :32; int :32; int :32;
  int :32; int :32; int :32; int :32;
  int :32; int :32; int :32;
};
# 89 "/usr/include/bits/time.h" 2 3 4

extern "C" {


extern int clock_adjtime (__clockid_t __clock_id, struct timex *__utx) throw ();

}
# 42 "/usr/include/time.h" 2 3 4
# 57 "/usr/include/time.h" 3 4


typedef __clock_t clock_t;



# 91 "/usr/include/time.h" 3 4
typedef __clockid_t clockid_t;
# 103 "/usr/include/time.h" 3 4
typedef __timer_t timer_t;
# 131 "/usr/include/time.h" 3 4


struct tm
{
  int tm_sec;
  int tm_min;
  int tm_hour;
  int tm_mday;
  int tm_mon;
  int tm_year;
  int tm_wday;
  int tm_yday;
  int tm_isdst;


  long int tm_gmtoff;
  const char *tm_zone;




};








struct itimerspec
  {
    struct timespec it_interval;
    struct timespec it_value;
  };


struct sigevent;
# 186 "/usr/include/time.h" 3 4



extern clock_t clock (void) throw ();


extern time_t time (time_t *__timer) throw ();


extern double difftime (time_t __time1, time_t __time0)
     throw () __attribute__ ((__const__));


extern time_t mktime (struct tm *__tp) throw ();





extern size_t strftime (char *__restrict __s, size_t __maxsize,
   const char *__restrict __format,
   const struct tm *__restrict __tp) throw ();





extern char *strptime (const char *__restrict __s,
         const char *__restrict __fmt, struct tm *__tp)
     throw ();







extern size_t strftime_l (char *__restrict __s, size_t __maxsize,
     const char *__restrict __format,
     const struct tm *__restrict __tp,
     __locale_t __loc) throw ();



extern char *strptime_l (const char *__restrict __s,
    const char *__restrict __fmt, struct tm *__tp,
    __locale_t __loc) throw ();






extern struct tm *gmtime (const time_t *__timer) throw ();



extern struct tm *localtime (const time_t *__timer) throw ();





extern struct tm *gmtime_r (const time_t *__restrict __timer,
       struct tm *__restrict __tp) throw ();



extern struct tm *localtime_r (const time_t *__restrict __timer,
          struct tm *__restrict __tp) throw ();





extern char *asctime (const struct tm *__tp) throw ();


extern char *ctime (const time_t *__timer) throw ();







extern char *asctime_r (const struct tm *__restrict __tp,
   char *__restrict __buf) throw ();


extern char *ctime_r (const time_t *__restrict __timer,
        char *__restrict __buf) throw ();




extern char *__tzname[2];
extern int __daylight;
extern long int __timezone;




extern char *tzname[2];



extern void tzset (void) throw ();



extern int daylight;
extern long int timezone;





extern int stime (const time_t *__when) throw ();
# 319 "/usr/include/time.h" 3 4
extern time_t timegm (struct tm *__tp) throw ();


extern time_t timelocal (struct tm *__tp) throw ();


extern int dysize (int __year) throw () __attribute__ ((__const__));
# 334 "/usr/include/time.h" 3 4
extern int nanosleep (const struct timespec *__requested_time,
        struct timespec *__remaining);



extern int clock_getres (clockid_t __clock_id, struct timespec *__res) throw ();


extern int clock_gettime (clockid_t __clock_id, struct timespec *__tp) throw ();


extern int clock_settime (clockid_t __clock_id, const struct timespec *__tp)
     throw ();






extern int clock_nanosleep (clockid_t __clock_id, int __flags,
       const struct timespec *__req,
       struct timespec *__rem);


extern int clock_getcpuclockid (pid_t __pid, clockid_t *__clock_id) throw ();




extern int timer_create (clockid_t __clock_id,
    struct sigevent *__restrict __evp,
    timer_t *__restrict __timerid) throw ();


extern int timer_delete (timer_t __timerid) throw ();


extern int timer_settime (timer_t __timerid, int __flags,
     const struct itimerspec *__restrict __value,
     struct itimerspec *__restrict __ovalue) throw ();


extern int timer_gettime (timer_t __timerid, struct itimerspec *__value)
     throw ();


extern int timer_getoverrun (timer_t __timerid) throw ();





extern int timespec_get (struct timespec *__ts, int __base)
     throw () __attribute__ ((__nonnull__ (1)));
# 403 "/usr/include/time.h" 3 4
extern int getdate_err;
# 412 "/usr/include/time.h" 3 4
extern struct tm *getdate (const char *__string);
# 426 "/usr/include/time.h" 3 4
extern int getdate_r (const char *__restrict __string,
        struct tm *__restrict __resbufp);


}
# 25 "/usr/include/pthread.h" 2 3 4

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
# 27 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/bits/setjmp.h" 1 3 4
# 26 "/usr/include/bits/setjmp.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 27 "/usr/include/bits/setjmp.h" 2 3 4




typedef long int __jmp_buf[8];
# 28 "/usr/include/pthread.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 29 "/usr/include/pthread.h" 2 3 4



enum
{
  PTHREAD_CREATE_JOINABLE,

  PTHREAD_CREATE_DETACHED

};



enum
{
  PTHREAD_MUTEX_TIMED_NP,
  PTHREAD_MUTEX_RECURSIVE_NP,
  PTHREAD_MUTEX_ERRORCHECK_NP,
  PTHREAD_MUTEX_ADAPTIVE_NP

  ,
  PTHREAD_MUTEX_NORMAL = PTHREAD_MUTEX_TIMED_NP,
  PTHREAD_MUTEX_RECURSIVE = PTHREAD_MUTEX_RECURSIVE_NP,
  PTHREAD_MUTEX_ERRORCHECK = PTHREAD_MUTEX_ERRORCHECK_NP,
  PTHREAD_MUTEX_DEFAULT = PTHREAD_MUTEX_NORMAL



  , PTHREAD_MUTEX_FAST_NP = PTHREAD_MUTEX_TIMED_NP

};




enum
{
  PTHREAD_MUTEX_STALLED,
  PTHREAD_MUTEX_STALLED_NP = PTHREAD_MUTEX_STALLED,
  PTHREAD_MUTEX_ROBUST,
  PTHREAD_MUTEX_ROBUST_NP = PTHREAD_MUTEX_ROBUST
};





enum
{
  PTHREAD_PRIO_NONE,
  PTHREAD_PRIO_INHERIT,
  PTHREAD_PRIO_PROTECT
};
# 125 "/usr/include/pthread.h" 3 4
enum
{
  PTHREAD_RWLOCK_PREFER_READER_NP,
  PTHREAD_RWLOCK_PREFER_WRITER_NP,
  PTHREAD_RWLOCK_PREFER_WRITER_NONRECURSIVE_NP,
  PTHREAD_RWLOCK_DEFAULT_NP = PTHREAD_RWLOCK_PREFER_READER_NP
};
# 166 "/usr/include/pthread.h" 3 4
enum
{
  PTHREAD_INHERIT_SCHED,

  PTHREAD_EXPLICIT_SCHED

};



enum
{
  PTHREAD_SCOPE_SYSTEM,

  PTHREAD_SCOPE_PROCESS

};



enum
{
  PTHREAD_PROCESS_PRIVATE,

  PTHREAD_PROCESS_SHARED

};
# 201 "/usr/include/pthread.h" 3 4
struct _pthread_cleanup_buffer
{
  void (*__routine) (void *);
  void *__arg;
  int __canceltype;
  struct _pthread_cleanup_buffer *__prev;
};


enum
{
  PTHREAD_CANCEL_ENABLE,

  PTHREAD_CANCEL_DISABLE

};
enum
{
  PTHREAD_CANCEL_DEFERRED,

  PTHREAD_CANCEL_ASYNCHRONOUS

};
# 239 "/usr/include/pthread.h" 3 4
extern "C" {




extern int pthread_create (pthread_t *__restrict __newthread,
      const pthread_attr_t *__restrict __attr,
      void *(*__start_routine) (void *),
      void *__restrict __arg) throw () __attribute__ ((__nonnull__ (1, 3)));





extern void pthread_exit (void *__retval) __attribute__ ((__noreturn__));







extern int pthread_join (pthread_t __th, void **__thread_return);




extern int pthread_tryjoin_np (pthread_t __th, void **__thread_return) throw ();







extern int pthread_timedjoin_np (pthread_t __th, void **__thread_return,
     const struct timespec *__abstime);






extern int pthread_detach (pthread_t __th) throw ();



extern pthread_t pthread_self (void) throw () __attribute__ ((__const__));


extern int pthread_equal (pthread_t __thread1, pthread_t __thread2)
  throw () __attribute__ ((__const__));







extern int pthread_attr_init (pthread_attr_t *__attr) throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_destroy (pthread_attr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getdetachstate (const pthread_attr_t *__attr,
     int *__detachstate)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setdetachstate (pthread_attr_t *__attr,
     int __detachstate)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getguardsize (const pthread_attr_t *__attr,
          size_t *__guardsize)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setguardsize (pthread_attr_t *__attr,
          size_t __guardsize)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getschedparam (const pthread_attr_t *__restrict __attr,
           struct sched_param *__restrict __param)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setschedparam (pthread_attr_t *__restrict __attr,
           const struct sched_param *__restrict
           __param) throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_getschedpolicy (const pthread_attr_t *__restrict
     __attr, int *__restrict __policy)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setschedpolicy (pthread_attr_t *__attr, int __policy)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getinheritsched (const pthread_attr_t *__restrict
      __attr, int *__restrict __inherit)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setinheritsched (pthread_attr_t *__attr,
      int __inherit)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getscope (const pthread_attr_t *__restrict __attr,
      int *__restrict __scope)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_attr_setscope (pthread_attr_t *__attr, int __scope)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_attr_getstackaddr (const pthread_attr_t *__restrict
          __attr, void **__restrict __stackaddr)
     throw () __attribute__ ((__nonnull__ (1, 2))) __attribute__ ((__deprecated__));





extern int pthread_attr_setstackaddr (pthread_attr_t *__attr,
          void *__stackaddr)
     throw () __attribute__ ((__nonnull__ (1))) __attribute__ ((__deprecated__));


extern int pthread_attr_getstacksize (const pthread_attr_t *__restrict
          __attr, size_t *__restrict __stacksize)
     throw () __attribute__ ((__nonnull__ (1, 2)));




extern int pthread_attr_setstacksize (pthread_attr_t *__attr,
          size_t __stacksize)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_attr_getstack (const pthread_attr_t *__restrict __attr,
      void **__restrict __stackaddr,
      size_t *__restrict __stacksize)
     throw () __attribute__ ((__nonnull__ (1, 2, 3)));




extern int pthread_attr_setstack (pthread_attr_t *__attr, void *__stackaddr,
      size_t __stacksize) throw () __attribute__ ((__nonnull__ (1)));





extern int pthread_attr_setaffinity_np (pthread_attr_t *__attr,
     size_t __cpusetsize,
     const cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (1, 3)));



extern int pthread_attr_getaffinity_np (const pthread_attr_t *__attr,
     size_t __cpusetsize,
     cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (1, 3)));





extern int pthread_getattr_np (pthread_t __th, pthread_attr_t *__attr)
     throw () __attribute__ ((__nonnull__ (2)));







extern int pthread_setschedparam (pthread_t __target_thread, int __policy,
      const struct sched_param *__param)
     throw () __attribute__ ((__nonnull__ (3)));


extern int pthread_getschedparam (pthread_t __target_thread,
      int *__restrict __policy,
      struct sched_param *__restrict __param)
     throw () __attribute__ ((__nonnull__ (2, 3)));


extern int pthread_setschedprio (pthread_t __target_thread, int __prio)
     throw ();




extern int pthread_getname_np (pthread_t __target_thread, char *__buf,
          size_t __buflen)
     throw () __attribute__ ((__nonnull__ (2)));


extern int pthread_setname_np (pthread_t __target_thread, const char *__name)
     throw () __attribute__ ((__nonnull__ (2)));





extern int pthread_getconcurrency (void) throw ();


extern int pthread_setconcurrency (int __level) throw ();







extern int pthread_yield (void) throw ();




extern int pthread_setaffinity_np (pthread_t __th, size_t __cpusetsize,
       const cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (3)));


extern int pthread_getaffinity_np (pthread_t __th, size_t __cpusetsize,
       cpu_set_t *__cpuset)
     throw () __attribute__ ((__nonnull__ (3)));
# 497 "/usr/include/pthread.h" 3 4
extern int pthread_once (pthread_once_t *__once_control,
    void (*__init_routine) (void)) __attribute__ ((__nonnull__ (1, 2)));
# 509 "/usr/include/pthread.h" 3 4
extern int pthread_setcancelstate (int __state, int *__oldstate);



extern int pthread_setcanceltype (int __type, int *__oldtype);


extern int pthread_cancel (pthread_t __th);




extern void pthread_testcancel (void);




typedef struct
{
  struct
  {
    __jmp_buf __cancel_jmp_buf;
    int __mask_was_saved;
  } __cancel_jmp_buf[1];
  void *__pad[4];
} __pthread_unwind_buf_t __attribute__ ((__aligned__));
# 543 "/usr/include/pthread.h" 3 4
struct __pthread_cleanup_frame
{
  void (*__cancel_routine) (void *);
  void *__cancel_arg;
  int __do_it;
  int __cancel_type;
};




class __pthread_cleanup_class
{
  void (*__cancel_routine) (void *);
  void *__cancel_arg;
  int __do_it;
  int __cancel_type;

 public:
  __pthread_cleanup_class (void (*__fct) (void *), void *__arg)
    : __cancel_routine (__fct), __cancel_arg (__arg), __do_it (1) { }
  ~__pthread_cleanup_class () { if (__do_it) __cancel_routine (__cancel_arg); }
  void __setdoit (int __newval) { __do_it = __newval; }
  void __defer () { pthread_setcanceltype (PTHREAD_CANCEL_DEFERRED,
        &__cancel_type); }
  void __restore () const { pthread_setcanceltype (__cancel_type, 0); }
};
# 745 "/usr/include/pthread.h" 3 4
struct __jmp_buf_tag;
extern int __sigsetjmp (struct __jmp_buf_tag *__env, int __savemask) throw ();





extern int pthread_mutex_init (pthread_mutex_t *__mutex,
          const pthread_mutexattr_t *__mutexattr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_destroy (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_trylock (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutex_lock (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutex_timedlock (pthread_mutex_t *__restrict __mutex,
        const struct timespec *__restrict
        __abstime) throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutex_unlock (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutex_getprioceiling (const pthread_mutex_t *
      __restrict __mutex,
      int *__restrict __prioceiling)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutex_setprioceiling (pthread_mutex_t *__restrict __mutex,
      int __prioceiling,
      int *__restrict __old_ceiling)
     throw () __attribute__ ((__nonnull__ (1, 3)));




extern int pthread_mutex_consistent (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));

extern int pthread_mutex_consistent_np (pthread_mutex_t *__mutex)
     throw () __attribute__ ((__nonnull__ (1)));
# 809 "/usr/include/pthread.h" 3 4
extern int pthread_mutexattr_init (pthread_mutexattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_destroy (pthread_mutexattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_getpshared (const pthread_mutexattr_t *
      __restrict __attr,
      int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_mutexattr_setpshared (pthread_mutexattr_t *__attr,
      int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutexattr_gettype (const pthread_mutexattr_t *__restrict
          __attr, int *__restrict __kind)
     throw () __attribute__ ((__nonnull__ (1, 2)));




extern int pthread_mutexattr_settype (pthread_mutexattr_t *__attr, int __kind)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutexattr_getprotocol (const pthread_mutexattr_t *
       __restrict __attr,
       int *__restrict __protocol)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutexattr_setprotocol (pthread_mutexattr_t *__attr,
       int __protocol)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_mutexattr_getprioceiling (const pthread_mutexattr_t *
          __restrict __attr,
          int *__restrict __prioceiling)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_mutexattr_setprioceiling (pthread_mutexattr_t *__attr,
          int __prioceiling)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_mutexattr_getrobust (const pthread_mutexattr_t *__attr,
     int *__robustness)
     throw () __attribute__ ((__nonnull__ (1, 2)));

extern int pthread_mutexattr_getrobust_np (const pthread_mutexattr_t *__attr,
        int *__robustness)
     throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_mutexattr_setrobust (pthread_mutexattr_t *__attr,
     int __robustness)
     throw () __attribute__ ((__nonnull__ (1)));

extern int pthread_mutexattr_setrobust_np (pthread_mutexattr_t *__attr,
        int __robustness)
     throw () __attribute__ ((__nonnull__ (1)));
# 891 "/usr/include/pthread.h" 3 4
extern int pthread_rwlock_init (pthread_rwlock_t *__restrict __rwlock,
    const pthread_rwlockattr_t *__restrict
    __attr) throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_destroy (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_rdlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_tryrdlock (pthread_rwlock_t *__rwlock)
  throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_rwlock_timedrdlock (pthread_rwlock_t *__restrict __rwlock,
           const struct timespec *__restrict
           __abstime) throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_rwlock_wrlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlock_trywrlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_rwlock_timedwrlock (pthread_rwlock_t *__restrict __rwlock,
           const struct timespec *__restrict
           __abstime) throw () __attribute__ ((__nonnull__ (1, 2)));



extern int pthread_rwlock_unlock (pthread_rwlock_t *__rwlock)
     throw () __attribute__ ((__nonnull__ (1)));





extern int pthread_rwlockattr_init (pthread_rwlockattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlockattr_destroy (pthread_rwlockattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlockattr_getpshared (const pthread_rwlockattr_t *
       __restrict __attr,
       int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_rwlockattr_setpshared (pthread_rwlockattr_t *__attr,
       int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_rwlockattr_getkind_np (const pthread_rwlockattr_t *
       __restrict __attr,
       int *__restrict __pref)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_rwlockattr_setkind_np (pthread_rwlockattr_t *__attr,
       int __pref) throw () __attribute__ ((__nonnull__ (1)));







extern int pthread_cond_init (pthread_cond_t *__restrict __cond,
         const pthread_condattr_t *__restrict __cond_attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_destroy (pthread_cond_t *__cond)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_signal (pthread_cond_t *__cond)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_cond_broadcast (pthread_cond_t *__cond)
     throw () __attribute__ ((__nonnull__ (1)));






extern int pthread_cond_wait (pthread_cond_t *__restrict __cond,
         pthread_mutex_t *__restrict __mutex)
     __attribute__ ((__nonnull__ (1, 2)));
# 1003 "/usr/include/pthread.h" 3 4
extern int pthread_cond_timedwait (pthread_cond_t *__restrict __cond,
       pthread_mutex_t *__restrict __mutex,
       const struct timespec *__restrict __abstime)
     __attribute__ ((__nonnull__ (1, 2, 3)));




extern int pthread_condattr_init (pthread_condattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_condattr_destroy (pthread_condattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_condattr_getpshared (const pthread_condattr_t *
     __restrict __attr,
     int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_condattr_setpshared (pthread_condattr_t *__attr,
     int __pshared) throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_condattr_getclock (const pthread_condattr_t *
          __restrict __attr,
          __clockid_t *__restrict __clock_id)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_condattr_setclock (pthread_condattr_t *__attr,
          __clockid_t __clock_id)
     throw () __attribute__ ((__nonnull__ (1)));
# 1047 "/usr/include/pthread.h" 3 4
extern int pthread_spin_init (pthread_spinlock_t *__lock, int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_destroy (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_lock (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_trylock (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_spin_unlock (pthread_spinlock_t *__lock)
     throw () __attribute__ ((__nonnull__ (1)));






extern int pthread_barrier_init (pthread_barrier_t *__restrict __barrier,
     const pthread_barrierattr_t *__restrict
     __attr, unsigned int __count)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrier_destroy (pthread_barrier_t *__barrier)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrier_wait (pthread_barrier_t *__barrier)
     throw () __attribute__ ((__nonnull__ (1)));



extern int pthread_barrierattr_init (pthread_barrierattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrierattr_destroy (pthread_barrierattr_t *__attr)
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_barrierattr_getpshared (const pthread_barrierattr_t *
        __restrict __attr,
        int *__restrict __pshared)
     throw () __attribute__ ((__nonnull__ (1, 2)));


extern int pthread_barrierattr_setpshared (pthread_barrierattr_t *__attr,
        int __pshared)
     throw () __attribute__ ((__nonnull__ (1)));
# 1114 "/usr/include/pthread.h" 3 4
extern int pthread_key_create (pthread_key_t *__key,
          void (*__destr_function) (void *))
     throw () __attribute__ ((__nonnull__ (1)));


extern int pthread_key_delete (pthread_key_t __key) throw ();


extern void *pthread_getspecific (pthread_key_t __key) throw ();


extern int pthread_setspecific (pthread_key_t __key,
    const void *__pointer) throw () ;




extern int pthread_getcpuclockid (pthread_t __thread_id,
      __clockid_t *__clock_id)
     throw () __attribute__ ((__nonnull__ (2)));
# 1148 "/usr/include/pthread.h" 3 4
extern int pthread_atfork (void (*__prepare) (void),
      void (*__parent) (void),
      void (*__child) (void)) throw ();
# 1162 "/usr/include/pthread.h" 3 4
}
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 2 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
typedef pthread_t __gthread_t;
typedef pthread_key_t __gthread_key_t;
typedef pthread_once_t __gthread_once_t;
typedef pthread_mutex_t __gthread_mutex_t;



typedef pthread_mutex_t __gthread_recursive_mutex_t;
typedef pthread_cond_t __gthread_cond_t;
typedef struct timespec __gthread_time_t;
# 123 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
static __typeof(pthread_once) __gthrw_pthread_once __attribute__ ((__weakref__("pthread_once"), __copy__ (pthread_once)));
static __typeof(pthread_getspecific) __gthrw_pthread_getspecific __attribute__ ((__weakref__("pthread_getspecific"), __copy__ (pthread_getspecific)));
static __typeof(pthread_setspecific) __gthrw_pthread_setspecific __attribute__ ((__weakref__("pthread_setspecific"), __copy__ (pthread_setspecific)));

static __typeof(pthread_create) __gthrw_pthread_create __attribute__ ((__weakref__("pthread_create"), __copy__ (pthread_create)));
static __typeof(pthread_join) __gthrw_pthread_join __attribute__ ((__weakref__("pthread_join"), __copy__ (pthread_join)));
static __typeof(pthread_equal) __gthrw_pthread_equal __attribute__ ((__weakref__("pthread_equal"), __copy__ (pthread_equal)));
static __typeof(pthread_self) __gthrw_pthread_self __attribute__ ((__weakref__("pthread_self"), __copy__ (pthread_self)));
static __typeof(pthread_detach) __gthrw_pthread_detach __attribute__ ((__weakref__("pthread_detach"), __copy__ (pthread_detach)));

static __typeof(pthread_cancel) __gthrw_pthread_cancel __attribute__ ((__weakref__("pthread_cancel"), __copy__ (pthread_cancel)));

static __typeof(sched_yield) __gthrw_sched_yield __attribute__ ((__weakref__("sched_yield"), __copy__ (sched_yield)));

static __typeof(pthread_mutex_lock) __gthrw_pthread_mutex_lock __attribute__ ((__weakref__("pthread_mutex_lock"), __copy__ (pthread_mutex_lock)));
static __typeof(pthread_mutex_trylock) __gthrw_pthread_mutex_trylock __attribute__ ((__weakref__("pthread_mutex_trylock"), __copy__ (pthread_mutex_trylock)));

static __typeof(pthread_mutex_timedlock) __gthrw_pthread_mutex_timedlock __attribute__ ((__weakref__("pthread_mutex_timedlock"), __copy__ (pthread_mutex_timedlock)));

static __typeof(pthread_mutex_unlock) __gthrw_pthread_mutex_unlock __attribute__ ((__weakref__("pthread_mutex_unlock"), __copy__ (pthread_mutex_unlock)));
static __typeof(pthread_mutex_init) __gthrw_pthread_mutex_init __attribute__ ((__weakref__("pthread_mutex_init"), __copy__ (pthread_mutex_init)));
static __typeof(pthread_mutex_destroy) __gthrw_pthread_mutex_destroy __attribute__ ((__weakref__("pthread_mutex_destroy"), __copy__ (pthread_mutex_destroy)));

static __typeof(pthread_cond_init) __gthrw_pthread_cond_init __attribute__ ((__weakref__("pthread_cond_init"), __copy__ (pthread_cond_init)));
static __typeof(pthread_cond_broadcast) __gthrw_pthread_cond_broadcast __attribute__ ((__weakref__("pthread_cond_broadcast"), __copy__ (pthread_cond_broadcast)));
static __typeof(pthread_cond_signal) __gthrw_pthread_cond_signal __attribute__ ((__weakref__("pthread_cond_signal"), __copy__ (pthread_cond_signal)));
static __typeof(pthread_cond_wait) __gthrw_pthread_cond_wait __attribute__ ((__weakref__("pthread_cond_wait"), __copy__ (pthread_cond_wait)));
static __typeof(pthread_cond_timedwait) __gthrw_pthread_cond_timedwait __attribute__ ((__weakref__("pthread_cond_timedwait"), __copy__ (pthread_cond_timedwait)));
static __typeof(pthread_cond_destroy) __gthrw_pthread_cond_destroy __attribute__ ((__weakref__("pthread_cond_destroy"), __copy__ (pthread_cond_destroy)));

static __typeof(pthread_key_create) __gthrw_pthread_key_create __attribute__ ((__weakref__("pthread_key_create"), __copy__ (pthread_key_create)));
static __typeof(pthread_key_delete) __gthrw_pthread_key_delete __attribute__ ((__weakref__("pthread_key_delete"), __copy__ (pthread_key_delete)));
static __typeof(pthread_mutexattr_init) __gthrw_pthread_mutexattr_init __attribute__ ((__weakref__("pthread_mutexattr_init"), __copy__ (pthread_mutexattr_init)));
static __typeof(pthread_mutexattr_settype) __gthrw_pthread_mutexattr_settype __attribute__ ((__weakref__("pthread_mutexattr_settype"), __copy__ (pthread_mutexattr_settype)));
static __typeof(pthread_mutexattr_destroy) __gthrw_pthread_mutexattr_destroy __attribute__ ((__weakref__("pthread_mutexattr_destroy"), __copy__ (pthread_mutexattr_destroy)));
# 274 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
static __typeof(pthread_key_create) __gthrw___pthread_key_create __attribute__ ((__weakref__("__pthread_key_create"), __copy__ (pthread_key_create)));
# 284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
#pragma GCC visibility push(hidden)
inline __attribute__((__always_inline__)) int
__gthread_active_p (void)
{
  static void *const __gthread_active_ptr
    = __extension__ (void *) &__gthrw___pthread_key_create;
  return __gthread_active_ptr != 0;
}
#pragma GCC visibility pop
# 705 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
inline __attribute__((__always_inline__)) int
__gthread_create (__gthread_t *__threadid, void *(*__func) (void*),
    void *__args)
{
  return __gthrw_pthread_create (__threadid, __null, __func, __args);
}

inline __attribute__((__always_inline__)) int
__gthread_join (__gthread_t __threadid, void **__value_ptr)
{
  return __gthrw_pthread_join (__threadid, __value_ptr);
}

inline __attribute__((__always_inline__)) int
__gthread_detach (__gthread_t __threadid)
{
  return __gthrw_pthread_detach (__threadid);
}

inline __attribute__((__always_inline__)) int
__gthread_equal (__gthread_t __t1, __gthread_t __t2)
{
  return __gthrw_pthread_equal (__t1, __t2);
}

inline __attribute__((__always_inline__)) __gthread_t
__gthread_self (void)
{
  return __gthrw_pthread_self ();
}

inline __attribute__((__always_inline__)) int
__gthread_yield (void)
{
  return __gthrw_sched_yield ();
}

inline __attribute__((__always_inline__)) int
__gthread_once (__gthread_once_t *__once, void (*__func) (void))
{
  if (__gthread_active_p ())
    return __gthrw_pthread_once (__once, __func);
  else
    return -1;
}

inline __attribute__((__always_inline__)) int
__gthread_key_create (__gthread_key_t *__key, void (*__dtor) (void *))
{
  return __gthrw_pthread_key_create (__key, __dtor);
}

inline __attribute__((__always_inline__)) int
__gthread_key_delete (__gthread_key_t __key)
{
  return __gthrw_pthread_key_delete (__key);
}

inline __attribute__((__always_inline__)) void *
__gthread_getspecific (__gthread_key_t __key)
{
  return __gthrw_pthread_getspecific (__key);
}

inline __attribute__((__always_inline__)) int
__gthread_setspecific (__gthread_key_t __key, const void *__ptr)
{
  return __gthrw_pthread_setspecific (__key, __ptr);
}

inline __attribute__((__always_inline__)) void
__gthread_mutex_init_function (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    __gthrw_pthread_mutex_init (__mutex, __null);
}

inline __attribute__((__always_inline__)) int
__gthread_mutex_destroy (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_destroy (__mutex);
  else
    return 0;
}

inline __attribute__((__always_inline__)) int
__gthread_mutex_lock (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_lock (__mutex);
  else
    return 0;
}

inline __attribute__((__always_inline__)) int
__gthread_mutex_trylock (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_trylock (__mutex);
  else
    return 0;
}


inline __attribute__((__always_inline__)) int
__gthread_mutex_timedlock (__gthread_mutex_t *__mutex,
      const __gthread_time_t *__abs_timeout)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_timedlock (__mutex, __abs_timeout);
  else
    return 0;
}


inline __attribute__((__always_inline__)) int
__gthread_mutex_unlock (__gthread_mutex_t *__mutex)
{
  if (__gthread_active_p ())
    return __gthrw_pthread_mutex_unlock (__mutex);
  else
    return 0;
}
# 854 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_lock (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_lock (__mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_trylock (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_trylock (__mutex);
}


inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_timedlock (__gthread_recursive_mutex_t *__mutex,
         const __gthread_time_t *__abs_timeout)
{
  return __gthread_mutex_timedlock (__mutex, __abs_timeout);
}


inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_unlock (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_unlock (__mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_recursive_mutex_destroy (__gthread_recursive_mutex_t *__mutex)
{
  return __gthread_mutex_destroy (__mutex);
}
# 896 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr-default.h" 3
inline __attribute__((__always_inline__)) int
__gthread_cond_broadcast (__gthread_cond_t *__cond)
{
  return __gthrw_pthread_cond_broadcast (__cond);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_signal (__gthread_cond_t *__cond)
{
  return __gthrw_pthread_cond_signal (__cond);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_wait (__gthread_cond_t *__cond, __gthread_mutex_t *__mutex)
{
  return __gthrw_pthread_cond_wait (__cond, __mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_timedwait (__gthread_cond_t *__cond, __gthread_mutex_t *__mutex,
     const __gthread_time_t *__abs_timeout)
{
  return __gthrw_pthread_cond_timedwait (__cond, __mutex, __abs_timeout);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_wait_recursive (__gthread_cond_t *__cond,
          __gthread_recursive_mutex_t *__mutex)
{
  return __gthread_cond_wait (__cond, __mutex);
}

inline __attribute__((__always_inline__)) int
__gthread_cond_destroy (__gthread_cond_t* __cond)
{
  return __gthrw_pthread_cond_destroy (__cond);
}
# 158 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/gthr.h" 2 3


#pragma GCC visibility pop
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/atomic_word.h" 1 3
# 32 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/atomic_word.h" 3
typedef int _Atomic_word;
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 2 3




namespace __gnu_cxx __attribute__ ((__visibility__ ("default")))
{


  __attribute__((__always_inline__))
  inline bool
  __is_single_threaded() throw()
  {





    return !__gthread_active_p();

  }






  inline _Atomic_word
  __attribute__((__always_inline__))
  __exchange_and_add(volatile _Atomic_word* __mem, int __val)
  { return __atomic_fetch_add(__mem, __val, 4); }

  inline void
  __attribute__((__always_inline__))
  __atomic_add(volatile _Atomic_word* __mem, int __val)
  { __atomic_fetch_add(__mem, __val, 4); }
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/atomicity.h" 3
  inline _Atomic_word
  __attribute__((__always_inline__))
  __exchange_and_add_single(_Atomic_word* __mem, int __val)
  {
    _Atomic_word __result = *__mem;
    *__mem += __val;
    return __result;
  }

  inline void
  __attribute__((__always_inline__))
  __atomic_add_single(_Atomic_word* __mem, int __val)
  { *__mem += __val; }

  inline _Atomic_word
  __attribute__ ((__always_inline__))
  __exchange_and_add_dispatch(_Atomic_word* __mem, int __val)
  {
    if (__is_single_threaded())
      return __exchange_and_add_single(__mem, __val);
    else
      return __exchange_and_add(__mem, __val);
  }

  inline void
  __attribute__ ((__always_inline__))
  __atomic_add_dispatch(_Atomic_word* __mem, int __val)
  {
    if (__is_single_threaded())
      __atomic_add_single(__mem, __val);
    else
      __atomic_add(__mem, __val);
  }


}
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 1 3
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ostream_insert.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ostream_insert.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_forced.h" 1 3
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/cxxabi_forced.h" 3
#pragma GCC visibility push(default)


namespace __cxxabiv1
{







  class __forced_unwind
  {
    virtual ~__forced_unwind() throw();


    virtual void __pure_dummy() = 0;
  };
}


#pragma GCC visibility pop
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ostream_insert.h" 2 3


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{




  template<typename _CharT, typename _Traits>
    inline void
    __ostream_write(basic_ostream<_CharT, _Traits>& __out,
      const _CharT* __s, streamsize __n)
    {
      typedef basic_ostream<_CharT, _Traits> __ostream_type;
      typedef typename __ostream_type::ios_base __ios_base;

      const streamsize __put = __out.rdbuf()->sputn(__s, __n);
      if (__put != __n)
 __out.setstate(__ios_base::badbit);
    }

  template<typename _CharT, typename _Traits>
    inline void
    __ostream_fill(basic_ostream<_CharT, _Traits>& __out, streamsize __n)
    {
      typedef basic_ostream<_CharT, _Traits> __ostream_type;
      typedef typename __ostream_type::ios_base __ios_base;

      const _CharT __c = __out.fill();
      for (; __n > 0; --__n)
 {
   const typename _Traits::int_type __put = __out.rdbuf()->sputc(__c);
   if (_Traits::eq_int_type(__put, _Traits::eof()))
     {
       __out.setstate(__ios_base::badbit);
       break;
     }
 }
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    __ostream_insert(basic_ostream<_CharT, _Traits>& __out,
       const _CharT* __s, streamsize __n)
    {
      typedef basic_ostream<_CharT, _Traits> __ostream_type;
      typedef typename __ostream_type::ios_base __ios_base;

      typename __ostream_type::sentry __cerb(__out);
      if (__cerb)
 {
   try
     {
       const streamsize __w = __out.width();
       if (__w > __n)
  {
    const bool __left = ((__out.flags()
     & __ios_base::adjustfield)
           == __ios_base::left);
    if (!__left)
      __ostream_fill(__out, __w - __n);
    if (__out.good())
      __ostream_write(__out, __s, __n);
    if (__left && __out.good())
      __ostream_fill(__out, __w - __n);
  }
       else
  __ostream_write(__out, __s, __n);
       __out.width(0);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __out._M_setstate(__ios_base::badbit);
       throw;
     }
   catch(...)
     { __out._M_setstate(__ios_base::badbit); }
 }
      return __out;
    }




  extern template ostream& __ostream_insert(ostream&, const char*, streamsize);


  extern template wostream& __ostream_insert(wostream&, const wchar_t*,
          streamsize);






}

#pragma GCC diagnostic pop
# 49 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 2 3







# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 1 3
# 56 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 57 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 2 3





namespace std __attribute__ ((__visibility__ ("default")))
{

namespace __cxx11 {
# 87 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
    class basic_string
    {
# 98 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      typedef typename __gnu_cxx::__alloc_traits<_Alloc>::template
 rebind<_CharT>::other _Char_alloc_type;


      typedef __gnu_cxx::__alloc_traits<_Char_alloc_type> _Alloc_traits;


    public:
      typedef _Traits traits_type;
      typedef typename _Traits::char_type value_type;
      typedef _Char_alloc_type allocator_type;
      typedef typename _Alloc_traits::size_type size_type;
      typedef typename _Alloc_traits::difference_type difference_type;
      typedef typename _Alloc_traits::reference reference;
      typedef typename _Alloc_traits::const_reference const_reference;
      typedef typename _Alloc_traits::pointer pointer;
      typedef typename _Alloc_traits::const_pointer const_pointer;
      typedef __gnu_cxx::__normal_iterator<pointer, basic_string> iterator;
      typedef __gnu_cxx::__normal_iterator<const_pointer, basic_string>
       const_iterator;
      typedef std::reverse_iterator<const_iterator> const_reverse_iterator;
      typedef std::reverse_iterator<iterator> reverse_iterator;


      static const size_type npos = static_cast<size_type>(-1);

    protected:


      typedef iterator __const_iterator;




    private:
      static pointer
      _S_allocate(_Char_alloc_type& __a, size_type __n)
      {
 pointer __p = _Alloc_traits::allocate(__a, __n);
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
 return __p;
      }
# 191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      struct _Alloc_hider : allocator_type
      {

 _Alloc_hider(pointer __dat, const _Alloc& __a = _Alloc())
 : allocator_type(__a), _M_p(__dat) { }
# 206 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
 pointer _M_p;
      };

      _Alloc_hider _M_dataplus;
      size_type _M_string_length;

      enum { _S_local_capacity = 15 / sizeof(_CharT) };

      union
      {
 _CharT _M_local_buf[_S_local_capacity + 1];
 size_type _M_allocated_capacity;
      };

     
      void
      _M_data(pointer __p)
      { _M_dataplus._M_p = __p; }

     
      void
      _M_length(size_type __length)
      { _M_string_length = __length; }

     
      pointer
      _M_data() const
      { return _M_dataplus._M_p; }

     
      pointer
      _M_local_data()
      {



 return pointer(_M_local_buf);

      }

     
      const_pointer
      _M_local_data() const
      {



 return const_pointer(_M_local_buf);

      }

     
      void
      _M_capacity(size_type __capacity)
      { _M_allocated_capacity = __capacity; }

     
      void
      _M_set_length(size_type __n)
      {
 _M_length(__n);
 traits_type::assign(_M_data()[__n], _CharT());
      }

     
      bool
      _M_is_local() const
      {
 if (_M_data() == _M_local_data())
   {
     if (_M_string_length > _S_local_capacity)
       __builtin_unreachable();
     return true;
   }
 return false;
      }


     
      pointer
      _M_create(size_type&, size_type);

     
      void
      _M_dispose()
      {
 if (!_M_is_local())
   _M_destroy(_M_allocated_capacity);
      }

     
      void
      _M_destroy(size_type __size) throw()
      { _Alloc_traits::deallocate(_M_get_allocator(), _M_data(), __size + 1); }




      template<typename _InIterator>
        void
        _M_construct_aux(_InIterator __beg, _InIterator __end,
    std::__false_type)
 {
          typedef typename iterator_traits<_InIterator>::iterator_category _Tag;
          _M_construct(__beg, __end, _Tag());
 }



      template<typename _Integer>
        void
        _M_construct_aux(_Integer __beg, _Integer __end, std::__true_type)
 { _M_construct_aux_2(static_cast<size_type>(__beg), __end); }

      void
      _M_construct_aux_2(size_type __req, _CharT __c)
      { _M_construct(__req, __c); }



      template<typename _InIterator>

        void
        _M_construct(_InIterator __beg, _InIterator __end,
       std::input_iterator_tag);



      template<typename _FwdIterator>

        void
        _M_construct(_FwdIterator __beg, _FwdIterator __end,
       std::forward_iterator_tag);

     
      void
      _M_construct(size_type __req, _CharT __c);

     
      allocator_type&
      _M_get_allocator()
      { return _M_dataplus; }

     
      const allocator_type&
      _M_get_allocator() const
      { return _M_dataplus; }


      __attribute__((__always_inline__))
     
      void
      _M_init_local_buf()
      {





      }

      __attribute__((__always_inline__))
     
      pointer
      _M_use_local_data()
      {



 return _M_local_data();
      }

    private:
# 394 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      _M_check(size_type __pos, const char* __s) const
      {
 if (__pos > this->size())
   __throw_out_of_range_fmt(("%s: __pos (which is %zu) > " "this->size() (which is %zu)")
                                         ,
       __s, __pos, this->size());
 return __pos;
      }

     
      void
      _M_check_length(size_type __n1, size_type __n2, const char* __s) const
      {
 if (this->max_size() - (this->size() - __n1) < __n2)
   __throw_length_error((__s));
      }



     
      size_type
      _M_limit(size_type __pos, size_type __off) const
      {
 const bool __testoff = __off < this->size() - __pos;
 return __testoff ? __off : this->size() - __pos;
      }


      bool
      _M_disjunct(const _CharT* __s) const
      {
 return (less<const _CharT*>()(__s, _M_data())
  || less<const _CharT*>()(_M_data() + this->size(), __s));
      }



     
      static void
      _S_copy(_CharT* __d, const _CharT* __s, size_type __n)
      {
 if (__n == 1)
   traits_type::assign(*__d, *__s);
 else
   traits_type::copy(__d, __s, __n);
      }

     
      static void
      _S_move(_CharT* __d, const _CharT* __s, size_type __n)
      {
 if (__n == 1)
   traits_type::assign(*__d, *__s);
 else
   traits_type::move(__d, __s, __n);
      }

     
      static void
      _S_assign(_CharT* __d, size_type __n, _CharT __c)
      {
 if (__n == 1)
   traits_type::assign(*__d, __c);
 else
   traits_type::assign(__d, __n, __c);
      }



      template<class _Iterator>

        static void
        _S_copy_chars(_CharT* __p, _Iterator __k1, _Iterator __k2)
        {
   for (; __k1 != __k2; ++__k1, (void)++__p)
     traits_type::assign(*__p, *__k1);
 }

     
      static void
      _S_copy_chars(_CharT* __p, iterator __k1, iterator __k2)
      { _S_copy_chars(__p, __k1.base(), __k2.base()); }

     
      static void
      _S_copy_chars(_CharT* __p, const_iterator __k1, const_iterator __k2)
     
      { _S_copy_chars(__p, __k1.base(), __k2.base()); }

     
      static void
      _S_copy_chars(_CharT* __p, _CharT* __k1, _CharT* __k2)
      { _S_copy(__p, __k1, __k2 - __k1); }

     
      static void
      _S_copy_chars(_CharT* __p, const _CharT* __k1, const _CharT* __k2)
     
      { _S_copy(__p, __k1, __k2 - __k1); }

     
      static int
      _S_compare(size_type __n1, size_type __n2)
      {
 const difference_type __d = difference_type(__n1 - __n2);

 if (__d > __gnu_cxx::__numeric_traits<int>::__max)
   return __gnu_cxx::__numeric_traits<int>::__max;
 else if (__d < __gnu_cxx::__numeric_traits<int>::__min)
   return __gnu_cxx::__numeric_traits<int>::__min;
 else
   return int(__d);
      }

     
      void
      _M_assign(const basic_string&);

     
      void
      _M_mutate(size_type __pos, size_type __len1, const _CharT* __s,
  size_type __len2);

     
      void
      _M_erase(size_type __pos, size_type __n);

    public:







     
      basic_string()
     



      : _M_dataplus(_M_local_data())
      {
 _M_init_local_buf();
 _M_set_length(0);
      }




     
      explicit
      basic_string(const _Alloc& __a)
      : _M_dataplus(_M_local_data(), __a)
      {
 _M_init_local_buf();
 _M_set_length(0);
      }





     
      basic_string(const basic_string& __str)
      : _M_dataplus(_M_local_data(),
      _Alloc_traits::_S_select_on_copy(__str._M_get_allocator()))
      {
 _M_construct(__str._M_data(), __str._M_data() + __str.length(),
       std::forward_iterator_tag());
      }
# 576 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string(const basic_string& __str, size_type __pos,
     const _Alloc& __a = _Alloc())
      : _M_dataplus(_M_local_data(), __a)
      {
 const _CharT* __start = __str._M_data()
   + __str._M_check(__pos, "basic_string::basic_string");
 _M_construct(__start, __start + __str._M_limit(__pos, npos),
       std::forward_iterator_tag());
      }







     
      basic_string(const basic_string& __str, size_type __pos,
     size_type __n)
      : _M_dataplus(_M_local_data())
      {
 const _CharT* __start = __str._M_data()
   + __str._M_check(__pos, "basic_string::basic_string");
 _M_construct(__start, __start + __str._M_limit(__pos, __n),
       std::forward_iterator_tag());
      }
# 611 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string(const basic_string& __str, size_type __pos,
     size_type __n, const _Alloc& __a)
      : _M_dataplus(_M_local_data(), __a)
      {
 const _CharT* __start
   = __str._M_data() + __str._M_check(__pos, "string::string");
 _M_construct(__start, __start + __str._M_limit(__pos, __n),
       std::forward_iterator_tag());
      }
# 631 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string(const _CharT* __s, size_type __n,
     const _Alloc& __a = _Alloc())
      : _M_dataplus(_M_local_data(), __a)
      {

 if (__s == 0 && __n > 0)
   std::__throw_logic_error(("basic_string: " "construction from null is not valid")
                                                 );
 _M_construct(__s, __s + __n, std::forward_iterator_tag());
      }
# 653 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string(const _CharT* __s, const _Alloc& __a = _Alloc())
      : _M_dataplus(_M_local_data(), __a)
      {

 if (__s == 0)
   std::__throw_logic_error(("basic_string: " "construction from null is not valid")
                                                 );
 const _CharT* __end = __s + traits_type::length(__s);
 _M_construct(__s, __end, forward_iterator_tag());
      }
# 676 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string(size_type __n, _CharT __c, const _Alloc& __a = _Alloc())
      : _M_dataplus(_M_local_data(), __a)
      { _M_construct(__n, __c); }
# 770 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      template<typename _InputIterator>


        basic_string(_InputIterator __beg, _InputIterator __end,
       const _Alloc& __a = _Alloc())
 : _M_dataplus(_M_local_data(), __a), _M_string_length(0)
 {



   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   _M_construct_aux(__beg, __end, _Integral());

 }
# 815 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      ~basic_string()
      { _M_dispose(); }





     
      basic_string&
      operator=(const basic_string& __str)
      {
 return this->assign(__str);
      }





     
      basic_string&
      operator=(const _CharT* __s)
      { return this->assign(__s); }
# 846 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      operator=(_CharT __c)
      {
 this->assign(1, __c);
 return *this;
      }
# 966 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      iterator
      begin()
      { return iterator(_M_data()); }





     
      const_iterator
      begin() const
      { return const_iterator(_M_data()); }





     
      iterator
      end()
      { return iterator(_M_data() + this->size()); }





     
      const_iterator
      end() const
      { return const_iterator(_M_data() + this->size()); }






     
      reverse_iterator
      rbegin()
      { return reverse_iterator(this->end()); }






     
      const_reverse_iterator
      rbegin() const
      { return const_reverse_iterator(this->end()); }






     
      reverse_iterator
      rend()
      { return reverse_iterator(this->begin()); }






     
      const_reverse_iterator
      rend() const
      { return const_reverse_iterator(this->begin()); }
# 1078 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
    public:



     
      size_type
      size() const
      {
 size_type __sz = _M_string_length;
 if (__sz > max_size ())
   __builtin_unreachable ();
 return __sz;
      }



     
      size_type
      length() const
      { return size(); }


     
      size_type
      max_size() const
      {
 const size_t __diffmax
   = __gnu_cxx::__numeric_traits<ptrdiff_t>::__max / sizeof(_CharT);
 const size_t __allocmax = _Alloc_traits::max_size(_M_get_allocator());
 return (std::min)(__diffmax, __allocmax) - 1;
      }
# 1120 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      void
      resize(size_type __n, _CharT __c);
# 1134 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      void
      resize(size_type __n)
      { this->resize(__n, _CharT()); }
# 1196 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      capacity() const
      {
 size_t __sz = _M_is_local() ? size_type(_S_local_capacity)
         : _M_allocated_capacity;
 if (__sz < _S_local_capacity || __sz > max_size ())
   __builtin_unreachable ();
 return __sz;
      }
# 1224 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      void
      reserve(size_type __res_arg);







     
      void
      reserve();




     
      void
      clear()
      { _M_set_length(0); }





     
      bool
      empty() const
      { return _M_string_length == 0; }
# 1266 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      const_reference
      operator[] (size_type __pos) const
      {
 do { if (__builtin_expect(!bool(__pos <= size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h", 1270, __PRETTY_FUNCTION__, "__pos <= size()"); } while (false);
 return _M_data()[__pos];
      }
# 1284 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      reference
      operator[](size_type __pos)
      {


 do { if (__builtin_expect(!bool(__pos <= size()), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h", 1290, __PRETTY_FUNCTION__, "__pos <= size()"); } while (false);

 ;
 return _M_data()[__pos];
      }
# 1306 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      const_reference
      at(size_type __n) const
      {
 if (__n >= this->size())
   __throw_out_of_range_fmt(("basic_string::at: __n " "(which is %zu) >= this->size() " "(which is %zu)")

                            ,
       __n, this->size());
 return _M_data()[__n];
      }
# 1328 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      reference
      at(size_type __n)
      {
 if (__n >= size())
   __throw_out_of_range_fmt(("basic_string::at: __n " "(which is %zu) >= this->size() " "(which is %zu)")

                            ,
       __n, this->size());
 return _M_data()[__n];
      }
# 1396 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      operator+=(const basic_string& __str)
      { return this->append(__str); }






     
      basic_string&
      operator+=(const _CharT* __s)
      { return this->append(__s); }






     
      basic_string&
      operator+=(_CharT __c)
      {
 this->push_back(__c);
 return *this;
      }
# 1454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      append(const basic_string& __str)
      { return this->append(__str._M_data(), __str.size()); }
# 1472 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      append(const basic_string& __str, size_type __pos, size_type __n = npos)
      { return this->append(__str._M_data()
       + __str._M_check(__pos, "basic_string::append"),
       __str._M_limit(__pos, __n)); }







     
      basic_string&
      append(const _CharT* __s, size_type __n)
      {
 ;
 _M_check_length(size_type(0), __n, "basic_string::append");
 return _M_append(__s, __n);
      }






     
      basic_string&
      append(const _CharT* __s)
      {
 ;
 const size_type __n = traits_type::length(__s);
 _M_check_length(size_type(0), __n, "basic_string::append");
 return _M_append(__s, __n);
      }
# 1517 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      append(size_type __n, _CharT __c)
      { return _M_replace_aux(this->size(), size_type(0), __n, __c); }
# 1547 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      template<class _InputIterator>

        basic_string&
        append(_InputIterator __first, _InputIterator __last)
        { return this->replace(end(), end(), __first, __last); }
# 1591 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      void
      push_back(_CharT __c)
      {
 const size_type __size = this->size();
 if (__size + 1 > this->capacity())
   this->_M_mutate(__size, size_type(0), 0, size_type(1));
 traits_type::assign(this->_M_data()[__size], __c);
 this->_M_set_length(__size + 1);
      }






     
      basic_string&
      assign(const basic_string& __str)
      {
# 1640 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
 this->_M_assign(__str);
 return *this;
      }
# 1677 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      assign(const basic_string& __str, size_type __pos, size_type __n = npos)
      { return _M_replace(size_type(0), this->size(), __str._M_data()
     + __str._M_check(__pos, "basic_string::assign"),
     __str._M_limit(__pos, __n)); }
# 1694 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      assign(const _CharT* __s, size_type __n)
      {
 ;
 return _M_replace(size_type(0), this->size(), __s, __n);
      }
# 1711 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      assign(const _CharT* __s)
      {
 ;
 return _M_replace(size_type(0), this->size(), __s,
     traits_type::length(__s));
      }
# 1729 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      assign(size_type __n, _CharT __c)
      { return _M_replace_aux(size_type(0), this->size(), __n, __c); }
# 1775 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      template<class _InputIterator>
 basic_string&
 assign(_InputIterator __first, _InputIterator __last)
 { return this->replace(begin(), end(), __first, __last); }
# 1880 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      void
      insert(iterator __p, size_type __n, _CharT __c)
      { this->replace(__p, __p, __n, __c); }
# 1924 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      template<class _InputIterator>
        void
        insert(iterator __p, _InputIterator __beg, _InputIterator __end)
        { this->replace(__p, __p, __beg, __end); }
# 1965 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      insert(size_type __pos1, const basic_string& __str)
      { return this->replace(__pos1, size_type(0),
        __str._M_data(), __str.size()); }
# 1989 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      insert(size_type __pos1, const basic_string& __str,
      size_type __pos2, size_type __n = npos)
      { return this->replace(__pos1, size_type(0), __str._M_data()
        + __str._M_check(__pos2, "basic_string::insert"),
        __str._M_limit(__pos2, __n)); }
# 2013 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      insert(size_type __pos, const _CharT* __s, size_type __n)
      { return this->replace(__pos, size_type(0), __s, __n); }
# 2033 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      insert(size_type __pos, const _CharT* __s)
      {
 ;
 return this->replace(__pos, size_type(0), __s,
        traits_type::length(__s));
      }
# 2058 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      insert(size_type __pos, size_type __n, _CharT __c)
      { return _M_replace_aux(_M_check(__pos, "basic_string::insert"),
         size_type(0), __n, __c); }
# 2077 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      iterator
      insert(__const_iterator __p, _CharT __c)
      {
 ;
 const size_type __pos = __p - begin();
 _M_replace_aux(__pos, size_type(0), size_type(1), __c);
 return iterator(_M_data() + __pos);
      }
# 2140 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      erase(size_type __pos = 0, size_type __n = npos)
      {
 _M_check(__pos, "basic_string::erase");
 if (__n == npos)
   this->_M_set_length(__pos);
 else if (__n != 0)
   this->_M_erase(__pos, _M_limit(__pos, __n));
 return *this;
      }
# 2160 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      iterator
      erase(__const_iterator __position)
      {

                           ;
 const size_type __pos = __position - begin();
 this->_M_erase(__pos, size_type(1));
 return iterator(_M_data() + __pos);
      }
# 2180 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      iterator
      erase(__const_iterator __first, __const_iterator __last)
      {

                        ;
        const size_type __pos = __first - begin();
 if (__last == end())
   this->_M_set_length(__pos);
 else
   this->_M_erase(__pos, __last - __first);
 return iterator(this->_M_data() + __pos);
      }
# 2226 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(size_type __pos, size_type __n, const basic_string& __str)
      { return this->replace(__pos, __n, __str._M_data(), __str.size()); }
# 2249 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(size_type __pos1, size_type __n1, const basic_string& __str,
       size_type __pos2, size_type __n2 = npos)
      { return this->replace(__pos1, __n1, __str._M_data()
        + __str._M_check(__pos2, "basic_string::replace"),
        __str._M_limit(__pos2, __n2)); }
# 2275 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(size_type __pos, size_type __n1, const _CharT* __s,
       size_type __n2)
      {
 ;
 return _M_replace(_M_check(__pos, "basic_string::replace"),
     _M_limit(__pos, __n1), __s, __n2);
      }
# 2301 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(size_type __pos, size_type __n1, const _CharT* __s)
      {
 ;
 return this->replace(__pos, __n1, __s, traits_type::length(__s));
      }
# 2326 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(size_type __pos, size_type __n1, size_type __n2, _CharT __c)
      { return _M_replace_aux(_M_check(__pos, "basic_string::replace"),
         _M_limit(__pos, __n1), __n2, __c); }
# 2345 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2,
       const basic_string& __str)
      { return this->replace(__i1, __i2, __str._M_data(), __str.size()); }
# 2366 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2,
       const _CharT* __s, size_type __n)
      {

                      ;
 return this->replace(__i1 - begin(), __i2 - __i1, __s, __n);
      }
# 2389 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2, const _CharT* __s)
      {
 ;
 return this->replace(__i1, __i2, __s, traits_type::length(__s));
      }
# 2411 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2, size_type __n,
       _CharT __c)
      {

                      ;
 return _M_replace_aux(__i1 - begin(), __i2 - __i1, __n, __c);
      }
# 2451 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      template<class _InputIterator>



        basic_string&

        replace(iterator __i1, iterator __i2,
  _InputIterator __k1, _InputIterator __k2)
        {
  
                        ;
   ;
   typedef typename std::__is_integer<_InputIterator>::__type _Integral;
   return _M_replace_dispatch(__i1, __i2, __k1, __k2, _Integral());
 }




     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2,
       _CharT* __k1, _CharT* __k2)
      {

                      ;
 ;
 return this->replace(__i1 - begin(), __i2 - __i1,
        __k1, __k2 - __k1);
      }

     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2,
       const _CharT* __k1, const _CharT* __k2)
      {

                      ;
 ;
 return this->replace(__i1 - begin(), __i2 - __i1,
        __k1, __k2 - __k1);
      }

     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2,
       iterator __k1, iterator __k2)
      {

                      ;
 ;
 return this->replace(__i1 - begin(), __i2 - __i1,
        __k1.base(), __k2 - __k1);
      }

     
      basic_string&
      replace(__const_iterator __i1, __const_iterator __i2,
       const_iterator __k1, const_iterator __k2)
      {

                      ;
 ;
 return this->replace(__i1 - begin(), __i2 - __i1,
        __k1.base(), __k2 - __k1);
      }
# 2597 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
    private:
      template<class _Integer>

 basic_string&
 _M_replace_dispatch(const_iterator __i1, const_iterator __i2,
       _Integer __n, _Integer __val, __true_type)
        { return _M_replace_aux(__i1 - begin(), __i2 - __i1, __n, __val); }

      template<class _InputIterator>

 basic_string&
 _M_replace_dispatch(const_iterator __i1, const_iterator __i2,
       _InputIterator __k1, _InputIterator __k2,
       __false_type);

     
      basic_string&
      _M_replace_aux(size_type __pos1, size_type __n1, size_type __n2,
       _CharT __c);

      __attribute__((__noinline__, __noclone__, __cold__)) void
      _M_replace_cold(pointer __p, size_type __len1, const _CharT* __s,
        const size_type __len2, const size_type __how_much);

     
      basic_string&
      _M_replace(size_type __pos, size_type __len1, const _CharT* __s,
   const size_type __len2);

     
      basic_string&
      _M_append(const _CharT* __s, size_type __n);

    public:
# 2644 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      copy(_CharT* __s, size_type __n, size_type __pos = 0) const;
# 2655 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      void
      swap(basic_string& __s) ;
# 2666 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      const _CharT*
      c_str() const
      { return _M_data(); }
# 2679 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      const _CharT*
      data() const
      { return _M_data(); }
# 2700 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      allocator_type
      get_allocator() const
      { return _M_get_allocator(); }
# 2717 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find(const _CharT* __s, size_type __pos, size_type __n) const
      ;
# 2732 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find(const basic_string& __str, size_type __pos = 0) const
     
      { return this->find(__str.data(), __pos, __str.size()); }
# 2766 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find(const _CharT* __s, size_type __pos = 0) const
      {
 ;
 return this->find(__s, __pos, traits_type::length(__s));
      }
# 2784 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find(_CharT __c, size_type __pos = 0) const ;
# 2798 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      rfind(const basic_string& __str, size_type __pos = npos) const
     
      { return this->rfind(__str.data(), __pos, __str.size()); }
# 2834 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      rfind(const _CharT* __s, size_type __pos, size_type __n) const
      ;
# 2849 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      rfind(const _CharT* __s, size_type __pos = npos) const
      {
 ;
 return this->rfind(__s, __pos, traits_type::length(__s));
      }
# 2867 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      rfind(_CharT __c, size_type __pos = npos) const ;
# 2882 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_of(const basic_string& __str, size_type __pos = 0) const
     
      { return this->find_first_of(__str.data(), __pos, __str.size()); }
# 2919 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_of(const _CharT* __s, size_type __pos, size_type __n) const
      ;
# 2934 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_of(const _CharT* __s, size_type __pos = 0) const
     
      {
 ;
 return this->find_first_of(__s, __pos, traits_type::length(__s));
      }
# 2955 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_of(_CharT __c, size_type __pos = 0) const
      { return this->find(__c, __pos); }
# 2971 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_of(const basic_string& __str, size_type __pos = npos) const
     
      { return this->find_last_of(__str.data(), __pos, __str.size()); }
# 3008 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_of(const _CharT* __s, size_type __pos, size_type __n) const
      ;
# 3023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_of(const _CharT* __s, size_type __pos = npos) const
     
      {
 ;
 return this->find_last_of(__s, __pos, traits_type::length(__s));
      }
# 3044 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_of(_CharT __c, size_type __pos = npos) const
      { return this->rfind(__c, __pos); }
# 3059 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_not_of(const basic_string& __str, size_type __pos = 0) const
     
      { return this->find_first_not_of(__str.data(), __pos, __str.size()); }
# 3096 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_not_of(const _CharT* __s, size_type __pos,
   size_type __n) const ;
# 3111 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_not_of(const _CharT* __s, size_type __pos = 0) const
     
      {
 ;
 return this->find_first_not_of(__s, __pos, traits_type::length(__s));
      }
# 3130 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_first_not_of(_CharT __c, size_type __pos = 0) const
      ;
# 3146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_not_of(const basic_string& __str, size_type __pos = npos) const
     
      { return this->find_last_not_of(__str.data(), __pos, __str.size()); }
# 3183 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_not_of(const _CharT* __s, size_type __pos,
         size_type __n) const ;
# 3198 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_not_of(const _CharT* __s, size_type __pos = npos) const
     
      {
 ;
 return this->find_last_not_of(__s, __pos, traits_type::length(__s));
      }
# 3217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      size_type
      find_last_not_of(_CharT __c, size_type __pos = npos) const
      ;
# 3234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      basic_string
      substr(size_type __pos = 0, size_type __n = npos) const
      { return basic_string(*this,
       _M_check(__pos, "basic_string::substr"), __n); }
# 3254 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      int
      compare(const basic_string& __str) const
      {
 const size_type __size = this->size();
 const size_type __osize = __str.size();
 const size_type __len = std::min(__size, __osize);

 int __r = traits_type::compare(_M_data(), __str.data(), __len);
 if (!__r)
   __r = _S_compare(__size, __osize);
 return __r;
      }
# 3351 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      int
      compare(size_type __pos, size_type __n, const basic_string& __str) const
      {
 _M_check(__pos, "basic_string::compare");
 __n = _M_limit(__pos, __n);
 const size_type __osize = __str.size();
 const size_type __len = std::min(__n, __osize);
 int __r = traits_type::compare(_M_data() + __pos, __str.data(), __len);
 if (!__r)
   __r = _S_compare(__n, __osize);
 return __r;
      }
# 3388 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      int
      compare(size_type __pos1, size_type __n1, const basic_string& __str,
       size_type __pos2, size_type __n2 = npos) const
      {
 _M_check(__pos1, "basic_string::compare");
 __str._M_check(__pos2, "basic_string::compare");
 __n1 = _M_limit(__pos1, __n1);
 __n2 = __str._M_limit(__pos2, __n2);
 const size_type __len = std::min(__n1, __n2);
 int __r = traits_type::compare(_M_data() + __pos1,
           __str.data() + __pos2, __len);
 if (!__r)
   __r = _S_compare(__n1, __n2);
 return __r;
      }
# 3419 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      int
      compare(const _CharT* __s) const
      {
 ;
 const size_type __size = this->size();
 const size_type __osize = traits_type::length(__s);
 const size_type __len = std::min(__size, __osize);
 int __r = traits_type::compare(_M_data(), __s, __len);
 if (!__r)
   __r = _S_compare(__size, __osize);
 return __r;
      }
# 3454 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      int
      compare(size_type __pos, size_type __n1, const _CharT* __s) const
      {
 ;
 _M_check(__pos, "basic_string::compare");
 __n1 = _M_limit(__pos, __n1);
 const size_type __osize = traits_type::length(__s);
 const size_type __len = std::min(__n1, __osize);
 int __r = traits_type::compare(_M_data() + __pos, __s, __len);
 if (!__r)
   __r = _S_compare(__n1, __osize);
 return __r;
      }
# 3493 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
     
      int
      compare(size_type __pos, size_type __n1, const _CharT* __s,
       size_type __n2) const
      {
 ;
 _M_check(__pos, "basic_string::compare");
 __n1 = _M_limit(__pos, __n1);
 const size_type __len = std::min(__n1, __n2);
 int __r = traits_type::compare(_M_data() + __pos, __s, __len);
 if (!__r)
   __r = _S_compare(__n1, __n2);
 return __r;
      }
# 3558 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
      template<typename, typename, typename> friend class basic_stringbuf;
    };
}

}


namespace std __attribute__ ((__visibility__ ("default")))
{

# 3598 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _Str>
   
    inline _Str
    __str_concat(typename _Str::value_type const* __lhs,
   typename _Str::size_type __lhs_len,
   typename _Str::value_type const* __rhs,
   typename _Str::size_type __rhs_len,
   typename _Str::allocator_type const& __a)
    {
      typedef typename _Str::allocator_type allocator_type;
      typedef __gnu_cxx::__alloc_traits<allocator_type> _Alloc_traits;
      _Str __str(_Alloc_traits::_S_select_on_copy(__a));
      __str.reserve(__lhs_len + __rhs_len);
      __str.append(__lhs, __lhs_len);
      __str.append(__rhs, __rhs_len);
      return __str;
    }
# 3623 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline basic_string<_CharT, _Traits, _Alloc>
    operator+(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
       const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    {
      typedef basic_string<_CharT, _Traits, _Alloc> _Str;
      return std::__str_concat<_Str>(__lhs.c_str(), __lhs.size(),
         __rhs.c_str(), __rhs.size(),
         __lhs.get_allocator());
    }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline basic_string<_CharT,_Traits,_Alloc>
    operator+(const _CharT* __lhs,
       const basic_string<_CharT,_Traits,_Alloc>& __rhs)
    {
      ;
      typedef basic_string<_CharT, _Traits, _Alloc> _Str;
      return std::__str_concat<_Str>(__lhs, _Traits::length(__lhs),
         __rhs.c_str(), __rhs.size(),
         __rhs.get_allocator());
    }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline basic_string<_CharT,_Traits,_Alloc>
    operator+(_CharT __lhs, const basic_string<_CharT,_Traits,_Alloc>& __rhs)
    {
      typedef basic_string<_CharT, _Traits, _Alloc> _Str;
      return std::__str_concat<_Str>(__builtin_addressof(__lhs), 1,
         __rhs.c_str(), __rhs.size(),
         __rhs.get_allocator());
    }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline basic_string<_CharT, _Traits, _Alloc>
    operator+(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
       const _CharT* __rhs)
    {
      ;
      typedef basic_string<_CharT, _Traits, _Alloc> _Str;
      return std::__str_concat<_Str>(__lhs.c_str(), __lhs.size(),
         __rhs, _Traits::length(__rhs),
         __lhs.get_allocator());
    }






  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline basic_string<_CharT, _Traits, _Alloc>
    operator+(const basic_string<_CharT, _Traits, _Alloc>& __lhs, _CharT __rhs)
    {
      typedef basic_string<_CharT, _Traits, _Alloc> _Str;
      return std::__str_concat<_Str>(__lhs.c_str(), __lhs.size(),
         __builtin_addressof(__rhs), 1,
         __lhs.get_allocator());
    }
# 3828 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator==(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    {
      return __lhs.size() == __rhs.size()
        && !_Traits::compare(__lhs.data(), __rhs.data(), __lhs.size());
    }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator==(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const _CharT* __rhs)
    {
      return __lhs.size() == _Traits::length(__rhs)
        && !_Traits::compare(__lhs.data(), __rhs, __lhs.size());
    }
# 3892 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator==(const _CharT* __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    { return __rhs == __lhs; }
# 3906 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator!=(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    { return !(__lhs == __rhs); }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator!=(const _CharT* __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    { return !(__rhs == __lhs); }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator!=(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const _CharT* __rhs)
    { return !(__lhs == __rhs); }
# 3947 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator<(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
       const basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    { return __lhs.compare(__rhs) < 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator<(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
       const _CharT* __rhs)
    { return __lhs.compare(__rhs) < 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator<(const _CharT* __lhs,
       const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    { return __rhs.compare(__lhs) > 0; }
# 3988 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator>(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
       const basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    { return __lhs.compare(__rhs) > 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator>(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
       const _CharT* __rhs)
    { return __lhs.compare(__rhs) > 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator>(const _CharT* __lhs,
       const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    { return __rhs.compare(__lhs) < 0; }
# 4029 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator<=(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    { return __lhs.compare(__rhs) <= 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator<=(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const _CharT* __rhs)
    { return __lhs.compare(__rhs) <= 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator<=(const _CharT* __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    { return __rhs.compare(__lhs) >= 0; }
# 4070 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator>=(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    { return __lhs.compare(__rhs) >= 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator>=(const basic_string<_CharT, _Traits, _Alloc>& __lhs,
        const _CharT* __rhs)
    { return __lhs.compare(__rhs) >= 0; }







  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline bool
    operator>=(const _CharT* __lhs,
      const basic_string<_CharT, _Traits, _Alloc>& __rhs)
    { return __rhs.compare(__lhs) <= 0; }
# 4112 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    inline void
    swap(basic_string<_CharT, _Traits, _Alloc>& __lhs,
  basic_string<_CharT, _Traits, _Alloc>& __rhs)
   
    { __lhs.swap(__rhs); }
# 4133 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
    basic_istream<_CharT, _Traits>&
    operator>>(basic_istream<_CharT, _Traits>& __is,
        basic_string<_CharT, _Traits, _Alloc>& __str);

  template<>
    basic_istream<char>&
    operator>>(basic_istream<char>& __is, basic_string<char>& __str);
# 4151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
    inline basic_ostream<_CharT, _Traits>&
    operator<<(basic_ostream<_CharT, _Traits>& __os,
        const basic_string<_CharT, _Traits, _Alloc>& __str)
    {


      return __ostream_insert(__os, __str.data(), __str.size());
    }
# 4174 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
    basic_istream<_CharT, _Traits>&
    getline(basic_istream<_CharT, _Traits>& __is,
     basic_string<_CharT, _Traits, _Alloc>& __str, _CharT __delim);
# 4191 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
    inline basic_istream<_CharT, _Traits>&
    getline(basic_istream<_CharT, _Traits>& __is,
     basic_string<_CharT, _Traits, _Alloc>& __str)
    { return std::getline(__is, __str, __is.widen('\n')); }
# 4213 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.h" 3
  template<>
    basic_istream<char>&
    getline(basic_istream<char>& __in, basic_string<char>& __str,
     char __delim);


  template<>
    basic_istream<wchar_t>&
    getline(basic_istream<wchar_t>& __in, basic_string<wchar_t>& __str,
     wchar_t __delim);



}
# 57 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.tcc" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"



namespace std __attribute__ ((__visibility__ ("default")))
{




  template<typename _CharT, typename _Traits, typename _Alloc>
    const typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::npos;

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    swap(basic_string& __s)
    {
      if (this == std::__addressof(__s))
 return;

      _Alloc_traits::_S_on_swap(_M_get_allocator(), __s._M_get_allocator());

      if (_M_is_local())
 if (__s._M_is_local())
   {
     if (length() && __s.length())
       {
  _CharT __tmp_data[_S_local_capacity + 1];
  traits_type::copy(__tmp_data, __s._M_local_buf,
      __s.length() + 1);
  traits_type::copy(__s._M_local_buf, _M_local_buf,
      length() + 1);
  traits_type::copy(_M_local_buf, __tmp_data,
      __s.length() + 1);
       }
     else if (__s.length())
       {
  _M_init_local_buf();
  traits_type::copy(_M_local_buf, __s._M_local_buf,
      __s.length() + 1);
  _M_length(__s.length());
  __s._M_set_length(0);
  return;
       }
     else if (length())
       {
  __s._M_init_local_buf();
  traits_type::copy(__s._M_local_buf, _M_local_buf,
      length() + 1);
  __s._M_length(length());
  _M_set_length(0);
  return;
       }
   }
 else
   {
     const size_type __tmp_capacity = __s._M_allocated_capacity;
     __s._M_init_local_buf();
     traits_type::copy(__s._M_local_buf, _M_local_buf,
         length() + 1);
     _M_data(__s._M_data());
     __s._M_data(__s._M_local_buf);
     _M_capacity(__tmp_capacity);
   }
      else
 {
   const size_type __tmp_capacity = _M_allocated_capacity;
   if (__s._M_is_local())
     {
       _M_init_local_buf();
       traits_type::copy(_M_local_buf, __s._M_local_buf,
    __s.length() + 1);
       __s._M_data(_M_data());
       _M_data(_M_local_buf);
     }
   else
     {
       pointer __tmp_ptr = _M_data();
       _M_data(__s._M_data());
       __s._M_data(__tmp_ptr);
       _M_capacity(__s._M_allocated_capacity);
     }
   __s._M_capacity(__tmp_capacity);
 }

      const size_type __tmp_length = length();
      _M_length(__s.length());
      __s._M_length(__tmp_length);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::pointer
    basic_string<_CharT, _Traits, _Alloc>::
    _M_create(size_type& __capacity, size_type __old_capacity)
    {


      if (__capacity > max_size())
 std::__throw_length_error(("basic_string::_M_create"));




      if (__capacity > __old_capacity && __capacity < 2 * __old_capacity)
 {
   __capacity = 2 * __old_capacity;

   if (__capacity > max_size())
     __capacity = max_size();
 }



      return _S_allocate(_M_get_allocator(), __capacity + 1);
    }





  template<typename _CharT, typename _Traits, typename _Alloc>
    template<typename _InIterator>
     
      void
      basic_string<_CharT, _Traits, _Alloc>::
      _M_construct(_InIterator __beg, _InIterator __end,
     std::input_iterator_tag)
      {
 size_type __len = 0;
 size_type __capacity = size_type(_S_local_capacity);

 _M_init_local_buf();

 while (__beg != __end && __len < __capacity)
   {
     _M_local_buf[__len++] = *__beg;
     ++__beg;
   }

 struct _Guard
 {
  
   explicit _Guard(basic_string* __s) : _M_guarded(__s) { }

  
   ~_Guard() { if (_M_guarded) _M_guarded->_M_dispose(); }

   basic_string* _M_guarded;
 } __guard(this);

 while (__beg != __end)
   {
     if (__len == __capacity)
       {

  __capacity = __len + 1;
  pointer __another = _M_create(__capacity, __len);
  this->_S_copy(__another, _M_data(), __len);
  _M_dispose();
  _M_data(__another);
  _M_capacity(__capacity);
       }
     traits_type::assign(_M_data()[__len++], *__beg);
     ++__beg;
   }

 __guard._M_guarded = 0;

 _M_set_length(__len);
      }

  template<typename _CharT, typename _Traits, typename _Alloc>
    template<typename _InIterator>
     
      void
      basic_string<_CharT, _Traits, _Alloc>::
      _M_construct(_InIterator __beg, _InIterator __end,
     std::forward_iterator_tag)
      {
 size_type __dnew = static_cast<size_type>(std::distance(__beg, __end));

 if (__dnew > size_type(_S_local_capacity))
   {
     _M_data(_M_create(__dnew, size_type(0)));
     _M_capacity(__dnew);
   }
 else
   _M_init_local_buf();


 struct _Guard
 {
  
   explicit _Guard(basic_string* __s) : _M_guarded(__s) { }

  
   ~_Guard() { if (_M_guarded) _M_guarded->_M_dispose(); }

   basic_string* _M_guarded;
 } __guard(this);

 this->_S_copy_chars(_M_data(), __beg, __end);

 __guard._M_guarded = 0;

 _M_set_length(__dnew);
      }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    _M_construct(size_type __n, _CharT __c)
    {
      if (__n > size_type(_S_local_capacity))
 {
   _M_data(_M_create(__n, size_type(0)));
   _M_capacity(__n);
 }
      else
 _M_init_local_buf();

      if (__n)
 this->_S_assign(_M_data(), __n, __c);

      _M_set_length(__n);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    _M_assign(const basic_string& __str)
    {
      if (this != std::__addressof(__str))
 {
   const size_type __rsize = __str.length();
   const size_type __capacity = capacity();

   if (__rsize > __capacity)
     {
       size_type __new_capacity = __rsize;
       pointer __tmp = _M_create(__new_capacity, __capacity);
       _M_dispose();
       _M_data(__tmp);
       _M_capacity(__new_capacity);
     }

   if (__rsize)
     this->_S_copy(_M_data(), __str._M_data(), __rsize);

   _M_set_length(__rsize);
 }
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    reserve(size_type __res)
    {
      const size_type __capacity = capacity();




      if (__res <= __capacity)
 return;

      pointer __tmp = _M_create(__res, __capacity);
      this->_S_copy(__tmp, _M_data(), length() + 1);
      _M_dispose();
      _M_data(__tmp);
      _M_capacity(__res);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    _M_mutate(size_type __pos, size_type __len1, const _CharT* __s,
       size_type __len2)
    {
      const size_type __how_much = length() - __pos - __len1;

      size_type __new_capacity = length() + __len2 - __len1;
      pointer __r = _M_create(__new_capacity, capacity());

      if (__pos)
 this->_S_copy(__r, _M_data(), __pos);
      if (__s && __len2)
 this->_S_copy(__r + __pos, __s, __len2);
      if (__how_much)
 this->_S_copy(__r + __pos + __len2,
        _M_data() + __pos + __len1, __how_much);

      _M_dispose();
      _M_data(__r);
      _M_capacity(__new_capacity);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    _M_erase(size_type __pos, size_type __n)
    {
      const size_type __how_much = length() - __pos - __n;

      if (__how_much && __n)
 this->_S_move(_M_data() + __pos, _M_data() + __pos + __n, __how_much);

      _M_set_length(length() - __n);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    reserve()
    {
      if (_M_is_local())
 return;

      const size_type __length = length();
      const size_type __capacity = _M_allocated_capacity;

      if (__length <= size_type(_S_local_capacity))
 {
   _M_init_local_buf();
   this->_S_copy(_M_local_buf, _M_data(), __length + 1);
   _M_destroy(__capacity);
   _M_data(_M_local_data());
 }

      else if (__length < __capacity)
 try
   {
     pointer __tmp = _S_allocate(_M_get_allocator(), __length + 1);
     this->_S_copy(__tmp, _M_data(), __length + 1);
     _M_dispose();
     _M_data(__tmp);
     _M_capacity(__length);
   }
 catch (const __cxxabiv1::__forced_unwind&)
   { throw; }
 catch (...)
   { }

    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    void
    basic_string<_CharT, _Traits, _Alloc>::
    resize(size_type __n, _CharT __c)
    {
      const size_type __size = this->size();
      if (__size < __n)
 this->append(__n - __size, __c);
      else if (__n < __size)
 this->_M_set_length(__n);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    basic_string<_CharT, _Traits, _Alloc>&
    basic_string<_CharT, _Traits, _Alloc>::
    _M_append(const _CharT* __s, size_type __n)
    {
      const size_type __len = __n + this->size();

      if (__len <= this->capacity())
 {
   if (__n)
     this->_S_copy(this->_M_data() + this->size(), __s, __n);
 }
      else
 this->_M_mutate(this->size(), size_type(0), __s, __n);

      this->_M_set_length(__len);
      return *this;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
    template<typename _InputIterator>
     
      basic_string<_CharT, _Traits, _Alloc>&
      basic_string<_CharT, _Traits, _Alloc>::
      _M_replace_dispatch(const_iterator __i1, const_iterator __i2,
     _InputIterator __k1, _InputIterator __k2,
     std::__false_type)
      {


 const basic_string __s(__k1, __k2, this->get_allocator());
 const size_type __n1 = __i2 - __i1;
 return _M_replace(__i1 - begin(), __n1, __s._M_data(),
     __s.size());
      }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    basic_string<_CharT, _Traits, _Alloc>&
    basic_string<_CharT, _Traits, _Alloc>::
    _M_replace_aux(size_type __pos1, size_type __n1, size_type __n2,
     _CharT __c)
    {
      _M_check_length(__n1, __n2, "basic_string::_M_replace_aux");

      const size_type __old_size = this->size();
      const size_type __new_size = __old_size + __n2 - __n1;

      if (__new_size <= this->capacity())
 {
   pointer __p = this->_M_data() + __pos1;

   const size_type __how_much = __old_size - __pos1 - __n1;
   if (__how_much && __n1 != __n2)
     this->_S_move(__p + __n2, __p + __n1, __how_much);
 }
      else
 this->_M_mutate(__pos1, __n1, 0, __n2);

      if (__n2)
 this->_S_assign(this->_M_data() + __pos1, __n2, __c);

      this->_M_set_length(__new_size);
      return *this;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
    __attribute__((__noinline__, __noclone__, __cold__)) void
    basic_string<_CharT, _Traits, _Alloc>::
    _M_replace_cold(pointer __p, size_type __len1, const _CharT* __s,
      const size_type __len2, const size_type __how_much)
    {

      if (__len2 && __len2 <= __len1)
 this->_S_move(__p, __s, __len2);
      if (__how_much && __len1 != __len2)
 this->_S_move(__p + __len2, __p + __len1, __how_much);
      if (__len2 > __len1)
 {
   if (__s + __len2 <= __p + __len1)
     this->_S_move(__p, __s, __len2);
   else if (__s >= __p + __len1)
     {


       const size_type __poff = (__s - __p) + (__len2 - __len1);
       this->_S_copy(__p, __p + __poff, __len2);
     }
   else
     {
       const size_type __nleft = (__p + __len1) - __s;
       this->_S_move(__p, __s, __nleft);
       this->_S_copy(__p + __nleft, __p + __len2, __len2 - __nleft);
     }
 }
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    basic_string<_CharT, _Traits, _Alloc>&
    basic_string<_CharT, _Traits, _Alloc>::
    _M_replace(size_type __pos, size_type __len1, const _CharT* __s,
        const size_type __len2)
    {
      _M_check_length(__len1, __len2, "basic_string::_M_replace");

      const size_type __old_size = this->size();
      const size_type __new_size = __old_size + __len2 - __len1;

      if (__new_size <= this->capacity())
 {
   pointer __p = this->_M_data() + __pos;

   const size_type __how_much = __old_size - __pos - __len1;
# 542 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.tcc" 3
   if (__builtin_expect(_M_disjunct(__s), true))
     {
       if (__how_much && __len1 != __len2)
  this->_S_move(__p + __len2, __p + __len1, __how_much);
       if (__len2)
  this->_S_copy(__p, __s, __len2);
     }
   else
     _M_replace_cold(__p, __len1, __s, __len2, __how_much);
 }
      else
 this->_M_mutate(__pos, __len1, __s, __len2);

      this->_M_set_length(__new_size);
      return *this;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    copy(_CharT* __s, size_type __n, size_type __pos) const
    {
      _M_check(__pos, "basic_string::copy");
      __n = _M_limit(__pos, __n);
      ;
      if (__n)
 _S_copy(__s, _M_data() + __pos, __n);

      return __n;
    }
# 628 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.tcc" 3
  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find(const _CharT* __s, size_type __pos, size_type __n) const
   
    {
      ;
      const size_type __size = this->size();

      if (__n == 0)
 return __pos <= __size ? __pos : npos;
      if (__pos >= __size)
 return npos;

      const _CharT __elem0 = __s[0];
      const _CharT* const __data = data();
      const _CharT* __first = __data + __pos;
      const _CharT* const __last = __data + __size;
      size_type __len = __size - __pos;

      while (__len >= __n)
 {

   __first = traits_type::find(__first, __len - __n + 1, __elem0);
   if (!__first)
     return npos;



   if (traits_type::compare(__first, __s, __n) == 0)
     return __first - __data;
   __len = __last - ++__first;
 }
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find(_CharT __c, size_type __pos) const
    {
      size_type __ret = npos;
      const size_type __size = this->size();
      if (__pos < __size)
 {
   const _CharT* __data = _M_data();
   const size_type __n = __size - __pos;
   const _CharT* __p = traits_type::find(__data + __pos, __n, __c);
   if (__p)
     __ret = __p - __data;
 }
      return __ret;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    rfind(const _CharT* __s, size_type __pos, size_type __n) const
   
    {
      ;
      const size_type __size = this->size();
      if (__n <= __size)
 {
   __pos = std::min(size_type(__size - __n), __pos);
   const _CharT* __data = _M_data();
   do
     {
       if (traits_type::compare(__data + __pos, __s, __n) == 0)
  return __pos;
     }
   while (__pos-- > 0);
 }
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    rfind(_CharT __c, size_type __pos) const
    {
      size_type __size = this->size();
      if (__size)
 {
   if (--__size > __pos)
     __size = __pos;
   for (++__size; __size-- > 0; )
     if (traits_type::eq(_M_data()[__size], __c))
       return __size;
 }
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find_first_of(const _CharT* __s, size_type __pos, size_type __n) const
   
    {
      ;
      for (; __n && __pos < this->size(); ++__pos)
 {
   const _CharT* __p = traits_type::find(__s, __n, _M_data()[__pos]);
   if (__p)
     return __pos;
 }
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find_last_of(const _CharT* __s, size_type __pos, size_type __n) const
   
    {
      ;
      size_type __size = this->size();
      if (__size && __n)
 {
   if (--__size > __pos)
     __size = __pos;
   do
     {
       if (traits_type::find(__s, __n, _M_data()[__size]))
  return __size;
     }
   while (__size-- != 0);
 }
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find_first_not_of(const _CharT* __s, size_type __pos, size_type __n) const
   
    {
      ;
      for (; __pos < this->size(); ++__pos)
 if (!traits_type::find(__s, __n, _M_data()[__pos]))
   return __pos;
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find_first_not_of(_CharT __c, size_type __pos) const
    {
      for (; __pos < this->size(); ++__pos)
 if (!traits_type::eq(_M_data()[__pos], __c))
   return __pos;
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find_last_not_of(const _CharT* __s, size_type __pos, size_type __n) const
   
    {
      ;
      size_type __size = this->size();
      if (__size)
 {
   if (--__size > __pos)
     __size = __pos;
   do
     {
       if (!traits_type::find(__s, __n, _M_data()[__size]))
  return __size;
     }
   while (__size--);
 }
      return npos;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
   
    typename basic_string<_CharT, _Traits, _Alloc>::size_type
    basic_string<_CharT, _Traits, _Alloc>::
    find_last_not_of(_CharT __c, size_type __pos) const
    {
      size_type __size = this->size();
      if (__size)
 {
   if (--__size > __pos)
     __size = __pos;
   do
     {
       if (!traits_type::eq(_M_data()[__size], __c))
  return __size;
     }
   while (__size--);
 }
      return npos;
    }




  template<typename _CharT, typename _Traits, typename _Alloc>
    basic_istream<_CharT, _Traits>&
    operator>>(basic_istream<_CharT, _Traits>& __in,
        basic_string<_CharT, _Traits, _Alloc>& __str)
    {
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef basic_string<_CharT, _Traits, _Alloc> __string_type;
      typedef typename __istream_type::ios_base __ios_base;
      typedef typename __istream_type::int_type __int_type;
      typedef typename __string_type::size_type __size_type;
      typedef ctype<_CharT> __ctype_type;
      typedef typename __ctype_type::ctype_base __ctype_base;

      __size_type __extracted = 0;
      typename __ios_base::iostate __err = __ios_base::goodbit;
      typename __istream_type::sentry __cerb(__in, false);
      if (__cerb)
 {
   try
     {

       __str.erase();
       _CharT __buf[128];
       __size_type __len = 0;
       const streamsize __w = __in.width();
       const __size_type __n = __w > 0 ? static_cast<__size_type>(__w)
                                : __str.max_size();
       const __ctype_type& __ct = use_facet<__ctype_type>(__in.getloc());
       const __int_type __eof = _Traits::eof();
       __int_type __c = __in.rdbuf()->sgetc();

       while (__extracted < __n
       && !_Traits::eq_int_type(__c, __eof)
       && !__ct.is(__ctype_base::space,
     _Traits::to_char_type(__c)))
  {
    if (__len == sizeof(__buf) / sizeof(_CharT))
      {
        __str.append(__buf, sizeof(__buf) / sizeof(_CharT));
        __len = 0;
      }
    __buf[__len++] = _Traits::to_char_type(__c);
    ++__extracted;
    __c = __in.rdbuf()->snextc();
  }
       __str.append(__buf, __len);

       if (__extracted < __n && _Traits::eq_int_type(__c, __eof))
  __err |= __ios_base::eofbit;
       __in.width(0);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __in._M_setstate(__ios_base::badbit);
       throw;
     }
   catch(...)
     {



       __in._M_setstate(__ios_base::badbit);
     }
 }

      if (!__extracted)
 __err |= __ios_base::failbit;
      if (__err)
 __in.setstate(__err);
      return __in;
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
    basic_istream<_CharT, _Traits>&
    getline(basic_istream<_CharT, _Traits>& __in,
     basic_string<_CharT, _Traits, _Alloc>& __str, _CharT __delim)
    {
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef basic_string<_CharT, _Traits, _Alloc> __string_type;
      typedef typename __istream_type::ios_base __ios_base;
      typedef typename __istream_type::int_type __int_type;
      typedef typename __string_type::size_type __size_type;

      __size_type __extracted = 0;
      const __size_type __n = __str.max_size();
      typename __ios_base::iostate __err = __ios_base::goodbit;
      typename __istream_type::sentry __cerb(__in, true);
      if (__cerb)
 {
   try
     {
       __str.erase();
       const __int_type __idelim = _Traits::to_int_type(__delim);
       const __int_type __eof = _Traits::eof();
       __int_type __c = __in.rdbuf()->sgetc();

       while (__extracted < __n
       && !_Traits::eq_int_type(__c, __eof)
       && !_Traits::eq_int_type(__c, __idelim))
  {
    __str += _Traits::to_char_type(__c);
    ++__extracted;
    __c = __in.rdbuf()->snextc();
  }

       if (_Traits::eq_int_type(__c, __eof))
  __err |= __ios_base::eofbit;
       else if (_Traits::eq_int_type(__c, __idelim))
  {
    ++__extracted;
    __in.rdbuf()->sbumpc();
  }
       else
  __err |= __ios_base::failbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __in._M_setstate(__ios_base::badbit);
       throw;
     }
   catch(...)
     {



       __in._M_setstate(__ios_base::badbit);
     }
 }
      if (!__extracted)
 __err |= __ios_base::failbit;
      if (__err)
 __in.setstate(__err);
      return __in;
    }
# 995 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.tcc" 3
  extern template
    basic_istream<char>&
    operator>>(basic_istream<char>&, string&);
  extern template
    basic_ostream<char>&
    operator<<(basic_ostream<char>&, const string&);
  extern template
    basic_istream<char>&
    getline(basic_istream<char>&, string&, char);
  extern template
    basic_istream<char>&
    getline(basic_istream<char>&, string&);
# 1021 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_string.tcc" 3
  extern template
    basic_istream<wchar_t>&
    operator>>(basic_istream<wchar_t>&, wstring&);
  extern template
    basic_ostream<wchar_t>&
    operator<<(basic_ostream<wchar_t>&, const wstring&);
  extern template
    basic_istream<wchar_t>&
    getline(basic_istream<wchar_t>&, wstring&, wchar_t);
  extern template
    basic_istream<wchar_t>&
    getline(basic_istream<wchar_t>&, wstring&);




}

#pragma GCC diagnostic pop
# 58 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 2 3
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 69 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/string" 2 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 2 3






namespace std __attribute__ ((__visibility__ ("default")))
{

# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
  class locale
  {
  public:


    typedef int category;


    class facet;
    class id;
    class _Impl;

    friend class facet;
    friend class _Impl;

    template<typename _Facet>
      friend bool
      has_facet(const locale&) throw();

    template<typename _Facet>
      friend const _Facet&
      use_facet(const locale&);

    template<typename _Facet>
      friend const _Facet*
      __try_use_facet(const locale&) throw();

    template<typename _Cache>
      friend struct __use_cache;
# 108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    static const category none = 0;
    static const category ctype = 1L << 0;
    static const category numeric = 1L << 1;
    static const category collate = 1L << 2;
    static const category time = 1L << 3;
    static const category monetary = 1L << 4;
    static const category messages = 1L << 5;
    static const category all = (ctype | numeric | collate |
        time | monetary | messages);
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    locale() throw();
# 136 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    locale(const locale& __other) throw();
# 146 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    explicit
    locale(const char* __s);
# 161 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    locale(const locale& __base, const char* __s, category __cat);
# 202 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    locale(const locale& __base, const locale& __add, category __cat);
# 215 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    template<typename _Facet>
      locale(const locale& __other, _Facet* __f);


    ~locale() throw();
# 229 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    const locale&
    operator=(const locale& __other) throw();
# 244 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    template<typename _Facet>
     
      locale
      combine(const locale& __other) const;






    __attribute ((__abi_tag__ ("cxx11")))
    string
    name() const;
# 275 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
   
    bool
    operator==(const locale& __other) const throw();
# 286 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
   
    bool
    operator!=(const locale& __other) const throw()
    { return !(this->operator==(__other)); }
# 307 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    template<typename _Char, typename _Traits, typename _Alloc>
     
      bool
      operator()(const basic_string<_Char, _Traits, _Alloc>& __s1,
   const basic_string<_Char, _Traits, _Alloc>& __s2) const;
# 324 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    static locale
    global(const locale& __loc);




   
    static const locale&
    classic();

  private:

    _Impl* _M_impl;


    static _Impl* _S_classic;


    static _Impl* _S_global;





    static const char* const* const _S_categories;
# 360 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    enum { _S_categories_size = 6 + 6 };


    static __gthread_once_t _S_once;


    explicit
    locale(_Impl*) throw();

    static void
    _S_initialize();

    static void
    _S_initialize_once() throw();

    static category
    _S_normalize_category(category);

    void
    _M_coalesce(const locale& __base, const locale& __add, category __cat);


    static const id* const _S_twinned_facets[];

  };
# 404 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
  class locale::facet
  {
  private:
    friend class locale;
    friend class locale::_Impl;

    mutable _Atomic_word _M_refcount;


    static __c_locale _S_c_locale;


    static const char _S_c_name[2];


    static __gthread_once_t _S_once;


    static void
    _S_initialize_once();

  protected:
# 435 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
    explicit
    facet(size_t __refs = 0) throw() : _M_refcount(__refs ? 1 : 0)
    { }


    virtual
    ~facet();

    static void
    _S_create_c_locale(__c_locale& __cloc, const char* __s,
         __c_locale __old = 0);

    static __c_locale
    _S_clone_c_locale(__c_locale& __cloc) throw();

    static void
    _S_destroy_c_locale(__c_locale& __cloc);

    static __c_locale
    _S_lc_ctype_c_locale(__c_locale __cloc, const char* __s);



    static __c_locale
    _S_get_c_locale();

    __attribute__ ((__const__)) static const char*
    _S_get_c_name() throw();


  private:
    facet(const facet&);

    facet&
    operator=(const facet&);







  private:
    void
    _M_add_reference() const throw()
    { __gnu_cxx::__atomic_add_dispatch(&_M_refcount, 1); }

    void
    _M_remove_reference() const throw()
    {

      ;
      if (__gnu_cxx::__exchange_and_add_dispatch(&_M_refcount, -1) == 1)
 {
          ;
   try
     { delete this; }
   catch(...)
     { }
 }
    }

    const facet* _M_sso_shim(const id*) const;
    const facet* _M_cow_shim(const id*) const;

  protected:
    class __shim;
  };
# 516 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
  class locale::id
  {
  private:
    friend class locale;
    friend class locale::_Impl;

    template<typename _Facet>
      friend const _Facet&
      use_facet(const locale&);

    template<typename _Facet>
      friend bool
      has_facet(const locale&) throw();

    template<typename _Facet>
      friend const _Facet*
      __try_use_facet(const locale&) throw();




    mutable size_t _M_index;


    static _Atomic_word _S_refcount;

    void
    operator=(const id&);

    id(const id&);

  public:



    id() { }

    size_t
    _M_id() const throw();
  };



  class locale::_Impl
  {
  public:

    friend class locale;
    friend class locale::facet;

    template<typename _Facet>
      friend bool
      has_facet(const locale&) throw();

    template<typename _Facet>
      friend const _Facet&
      use_facet(const locale&);

    template<typename _Facet>
      friend const _Facet*
      __try_use_facet(const locale&) throw();

    template<typename _Cache>
      friend struct __use_cache;

  private:

    _Atomic_word _M_refcount;
    const facet** _M_facets;
    size_t _M_facets_size;
    const facet** _M_caches;
    char** _M_names;
    static const locale::id* const _S_id_ctype[];
    static const locale::id* const _S_id_numeric[];
    static const locale::id* const _S_id_collate[];
    static const locale::id* const _S_id_time[];
    static const locale::id* const _S_id_monetary[];
    static const locale::id* const _S_id_messages[];
    static const locale::id* const* const _S_facet_categories[];

    void
    _M_add_reference() throw()
    { __gnu_cxx::__atomic_add_dispatch(&_M_refcount, 1); }

    void
    _M_remove_reference() throw()
    {

      ;
      if (__gnu_cxx::__exchange_and_add_dispatch(&_M_refcount, -1) == 1)
 {
          ;
   try
     { delete this; }
   catch(...)
     { }
 }
    }

    _Impl(const _Impl&, size_t);
    _Impl(const char*, size_t);
    _Impl(size_t) throw();

   ~_Impl() throw();

    _Impl(const _Impl&);

    void
    operator=(const _Impl&);

    bool
    _M_check_same_name()
    {
      bool __ret = true;
      if (_M_names[1])

 for (size_t __i = 0; __ret && __i < _S_categories_size - 1; ++__i)
   __ret = __builtin_strcmp(_M_names[__i], _M_names[__i + 1]) == 0;
      return __ret;
    }

    void
    _M_replace_categories(const _Impl*, category);

    void
    _M_replace_category(const _Impl*, const locale::id* const*);

    void
    _M_replace_facet(const _Impl*, const locale::id*);

    void
    _M_install_facet(const locale::id*, const facet*);

    template<typename _Facet>
      void
      _M_init_facet(_Facet* __facet)
      { _M_install_facet(&_Facet::id, __facet); }

    template<typename _Facet>
      void
      _M_init_facet_unchecked(_Facet* __facet)
      {
 __facet->_M_add_reference();
 _M_facets[_Facet::id._M_id()] = __facet;
      }

    void
    _M_install_cache(const facet*, size_t);

    void _M_init_extra(facet**);
    void _M_init_extra(void*, void*, const char*, const char*);




  };
# 686 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
  template<typename _CharT>
    class __cxx11:: collate : public locale::facet
    {
    public:



      typedef _CharT char_type;
      typedef basic_string<_CharT> string_type;


    protected:


      __c_locale _M_c_locale_collate;

    public:

      static locale::id id;
# 713 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      explicit
      collate(size_t __refs = 0)
      : facet(__refs), _M_c_locale_collate(_S_get_c_locale())
      { }
# 727 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      explicit
      collate(__c_locale __cloc, size_t __refs = 0)
      : facet(__refs), _M_c_locale_collate(_S_clone_c_locale(__cloc))
      { }
# 744 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      int
      compare(const _CharT* __lo1, const _CharT* __hi1,
       const _CharT* __lo2, const _CharT* __hi2) const
      { return this->do_compare(__lo1, __hi1, __lo2, __hi2); }
# 763 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      string_type
      transform(const _CharT* __lo, const _CharT* __hi) const
      { return this->do_transform(__lo, __hi); }
# 777 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      long
      hash(const _CharT* __lo, const _CharT* __hi) const
      { return this->do_hash(__lo, __hi); }


      int
      _M_compare(const _CharT*, const _CharT*) const throw();

      size_t
      _M_transform(_CharT*, const _CharT*, size_t) const throw();

  protected:

      virtual
      ~collate()
      { _S_destroy_c_locale(_M_c_locale_collate); }
# 806 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      virtual int
      do_compare(const _CharT* __lo1, const _CharT* __hi1,
   const _CharT* __lo2, const _CharT* __hi2) const;
# 820 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      virtual string_type
      do_transform(const _CharT* __lo, const _CharT* __hi) const;
# 833 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 3
      virtual long
      do_hash(const _CharT* __lo, const _CharT* __hi) const;
    };

  template<typename _CharT>
    locale::id collate<_CharT>::id;


  template<>
    int
    collate<char>::_M_compare(const char*, const char*) const throw();

  template<>
    size_t
    collate<char>::_M_transform(char*, const char*, size_t) const throw();


  template<>
    int
    collate<wchar_t>::_M_compare(const wchar_t*, const wchar_t*) const throw();

  template<>
    size_t
    collate<wchar_t>::_M_transform(wchar_t*, const wchar_t*, size_t) const throw();



  template<typename _CharT>
    class __cxx11:: collate_byname : public collate<_CharT>
    {
    public:


      typedef _CharT char_type;
      typedef basic_string<_CharT> string_type;


      explicit
      collate_byname(const char* __s, size_t __refs = 0)
      : collate<_CharT>(__refs)
      {
 if (__builtin_strcmp(__s, "C") != 0
     && __builtin_strcmp(__s, "POSIX") != 0)
   {
     this->_S_destroy_c_locale(this->_M_c_locale_collate);
     this->_S_create_c_locale(this->_M_c_locale_collate, __s);
   }
      }







    protected:
      virtual
      ~collate_byname() { }
    };


}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.tcc" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.tcc" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cerrno" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cerrno" 3
# 1 "/usr/include/errno.h" 1 3 4
# 31 "/usr/include/errno.h" 3 4
extern "C" {



# 1 "/usr/include/bits/errno.h" 1 3 4
# 24 "/usr/include/bits/errno.h" 3 4
# 1 "/usr/include/linux/errno.h" 1 3 4
# 1 "/usr/include/asm/errno.h" 1 3 4
# 1 "/usr/include/asm-generic/errno.h" 1 3 4



# 1 "/usr/include/asm-generic/errno-base.h" 1 3 4
# 5 "/usr/include/asm-generic/errno.h" 2 3 4
# 2 "/usr/include/asm/errno.h" 2 3 4
# 2 "/usr/include/linux/errno.h" 2 3 4
# 25 "/usr/include/bits/errno.h" 2 3 4
# 50 "/usr/include/bits/errno.h" 3 4
extern int *__errno_location (void) throw () __attribute__ ((__const__));
# 36 "/usr/include/errno.h" 2 3 4
# 54 "/usr/include/errno.h" 3 4
extern char *program_invocation_name, *program_invocation_short_name;



}
# 68 "/usr/include/errno.h" 3 4
typedef int error_t;
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cerrno" 2 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.tcc" 2 3

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wvariadic-macros"

namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _Facet>
    locale::
    locale(const locale& __other, _Facet* __f)
    {


      if (__builtin_expect(!__f, 0))
 {
   _M_impl = __other._M_impl;
   _M_impl->_M_add_reference();
   return;
 }

      _M_impl = new _Impl(*__other._M_impl, 1);

      try
 { _M_impl->_M_install_facet(&_Facet::id, __f); }
      catch(...)
 {
   _M_impl->_M_remove_reference();
   throw;
 }
      delete [] _M_impl->_M_names[0];
      _M_impl->_M_names[0] = 0;
    }

  template<typename _Facet>
    locale
    locale::
    combine(const locale& __other) const
    {




      _Impl* __tmp = new _Impl(*_M_impl, 1);
      try
 {
   __tmp->_M_replace_facet(__other._M_impl, &_Facet::id);
 }
      catch(...)
 {
   __tmp->_M_remove_reference();
   throw;
 }
      delete[] __tmp->_M_names[0];
      __tmp->_M_names[0] = 0;
      return locale(__tmp);
    }

  template<typename _CharT, typename _Traits, typename _Alloc>
    bool
    locale::
    operator()(const basic_string<_CharT, _Traits, _Alloc>& __s1,
        const basic_string<_CharT, _Traits, _Alloc>& __s2) const
    {
      typedef std::collate<_CharT> __collate_type;
      const __collate_type& __collate = use_facet<__collate_type>(*this);
      return (__collate.compare(__s1.data(), __s1.data() + __s1.length(),
    __s2.data(), __s2.data() + __s2.length()) < 0);
    }

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
  template<typename _Facet>
    inline const _Facet*
    __try_use_facet(const locale& __loc) throw()
    {
      const size_t __i = _Facet::id._M_id();
      const locale::facet** __facets = __loc._M_impl->_M_facets;







      if (__is_same(const _Facet, const ctype<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const num_get<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const num_put<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const codecvt<char, char, mbstate_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const collate<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const moneypunct<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const moneypunct<char, true>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const money_get<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const money_put<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const numpunct<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const time_get<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const time_put<char>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const messages<char>)) return static_cast<const _Facet*>(__facets[__i]);


      if (__is_same(const _Facet, const ctype<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const num_get<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const num_put<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const codecvt<wchar_t, char, mbstate_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const collate<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const moneypunct<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const moneypunct<wchar_t, true>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const money_get<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const money_put<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const numpunct<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const time_get<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const time_put<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
      if (__is_same(const _Facet, const messages<wchar_t>)) return static_cast<const _Facet*>(__facets[__i]);
# 164 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.tcc" 3
      if (__i >= __loc._M_impl->_M_facets_size || !__facets[__i])
 return 0;


      return dynamic_cast<const _Facet*>(__facets[__i]);



    }
#pragma GCC diagnostic pop
# 187 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.tcc" 3
  template<typename _Facet>
   
    inline bool
    has_facet(const locale& __loc) throw()
    {




      (void) static_cast<const _Facet*>(static_cast<const locale::facet*>(0));

      return std::__try_use_facet<_Facet>(__loc) != 0;
    }
# 215 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdangling-reference"
  template<typename _Facet>
   
    inline const _Facet&
    use_facet(const locale& __loc)
    {




      (void) static_cast<const _Facet*>(static_cast<const locale::facet*>(0));

      if (const _Facet* __f = std::__try_use_facet<_Facet>(__loc))
 return *__f;
      __throw_bad_cast();
    }
#pragma GCC diagnostic pop



  template<typename _CharT>
    int
    collate<_CharT>::_M_compare(const _CharT*, const _CharT*) const throw ()
    { return 0; }


  template<typename _CharT>
    size_t
    collate<_CharT>::_M_transform(_CharT*, const _CharT*, size_t) const throw ()
    { return 0; }

  template<typename _CharT>
    int
    collate<_CharT>::
    do_compare(const _CharT* __lo1, const _CharT* __hi1,
        const _CharT* __lo2, const _CharT* __hi2) const
    {


      const string_type __one(__lo1, __hi1);
      const string_type __two(__lo2, __hi2);

      const _CharT* __p = __one.c_str();
      const _CharT* __pend = __one.data() + __one.length();
      const _CharT* __q = __two.c_str();
      const _CharT* __qend = __two.data() + __two.length();




      for (;;)
 {
   const int __res = _M_compare(__p, __q);
   if (__res)
     return __res;

   __p += char_traits<_CharT>::length(__p);
   __q += char_traits<_CharT>::length(__q);
   if (__p == __pend && __q == __qend)
     return 0;
   else if (__p == __pend)
     return -1;
   else if (__q == __qend)
     return 1;

   __p++;
   __q++;
 }
    }

  template<typename _CharT>
    typename collate<_CharT>::string_type
    collate<_CharT>::
    do_transform(const _CharT* __lo, const _CharT* __hi) const
    {
      string_type __ret;


      const string_type __str(__lo, __hi);

      const _CharT* __p = __str.c_str();
      const _CharT* __pend = __str.data() + __str.length();

      size_t __len = (__hi - __lo) * 2;

      struct _Buf
      {
 _Buf(size_t __n, void* __buf, int __e)
 : _M_c(__buf ? (_CharT*)__buf : new _CharT[__n]),
   _M_stackbuf(__buf),
   _M_errno(__e)
 { }

 ~_Buf()
 {
   if (_M_c != _M_stackbuf)
     delete[] _M_c;
   if ((*__errno_location ()) == 0)
     (*__errno_location ()) = _M_errno;
 }

 void _M_realloc(size_t __len)
 {
   _CharT* __p = new _CharT[__len];
   if (_M_c != _M_stackbuf)
     delete[] _M_c;
   _M_c = __p;
 }

 _CharT* _M_c;
 void* const _M_stackbuf;
 int _M_errno;
      };

      const size_t __bytes = __len * sizeof(_CharT);
      _Buf __buf(__len, __bytes <= 256 ? __builtin_alloca(__bytes) : 0, (*__errno_location ()));
      (*__errno_location ()) = 0;




      for (;;)
 {

   size_t __res = _M_transform(__buf._M_c, __p, __len);


   if (__res >= __len)
     {
       if (__builtin_expect((*__errno_location ()), 0))
  {

    __throw_system_error((*__errno_location ()));







  }

       __len = __res + 1;
       __buf._M_realloc(__len);
       __res = _M_transform(__buf._M_c, __p, __len);
     }

   __ret.append(__buf._M_c, __res);
   __p += char_traits<_CharT>::length(__p);
   if (__p == __pend)
     break;

   __p++;
   __ret.push_back(_CharT());
 }

      return __ret;
    }

  template<typename _CharT>
    long
    collate<_CharT>::
    do_hash(const _CharT* __lo, const _CharT* __hi) const
    {
      unsigned long __val = 0;
      for (; __lo < __hi; ++__lo)
 __val =
   *__lo + ((__val << 7)
     | (__val >> (__gnu_cxx::__numeric_traits<unsigned long>::
    __digits - 7)));
      return static_cast<long>(__val);
    }




  extern template class collate<char>;
  extern template class collate_byname<char>;

  extern template
    const collate<char>*
    __try_use_facet<collate<char> >(const locale&) throw();

  extern template
    const collate<char>&
    use_facet<collate<char> >(const locale&);

  extern template
    bool
    has_facet<collate<char> >(const locale&);


  extern template class collate<wchar_t>;
  extern template class collate_byname<wchar_t>;

  extern template
    const collate<wchar_t>*
    __try_use_facet<collate<wchar_t> >(const locale&) throw();

  extern template
    const collate<wchar_t>&
    use_facet<collate<wchar_t> >(const locale&);

  extern template
    bool
    has_facet<collate<wchar_t> >(const locale&);




}

#pragma GCC diagnostic pop
# 897 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_classes.h" 2 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/stdexcept" 1 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/stdexcept" 3
namespace std __attribute__ ((__visibility__ ("default")))
{





  struct __cow_string
  {
    union {
      const char* _M_p;
      char _M_bytes[sizeof(const char*)];
    };

    __cow_string();
    __cow_string(const std::string&);
    __cow_string(const char*, size_t);
    __cow_string(const __cow_string&) throw();
    __cow_string& operator=(const __cow_string&) throw();
    ~__cow_string();




  };

  typedef basic_string<char> __sso_string;
# 115 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/stdexcept" 3
  class logic_error : public exception
  {
    __cow_string _M_msg;

  public:

    explicit
    logic_error(const string& __arg) ;
# 133 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/stdexcept" 3
    logic_error(const logic_error&) throw();
    logic_error& operator=(const logic_error&) throw();





    virtual ~logic_error() throw();



    virtual const char*
    what() const throw();





  };



  class domain_error : public logic_error
  {
  public:
    explicit domain_error(const string& __arg) ;







    virtual ~domain_error() throw();
  };


  class invalid_argument : public logic_error
  {
  public:
    explicit invalid_argument(const string& __arg) ;







    virtual ~invalid_argument() throw();
  };



  class length_error : public logic_error
  {
  public:
    explicit length_error(const string& __arg) ;







    virtual ~length_error() throw();
  };



  class out_of_range : public logic_error
  {
  public:
    explicit out_of_range(const string& __arg) ;







    virtual ~out_of_range() throw();
  };






  class runtime_error : public exception
  {
    __cow_string _M_msg;

  public:

    explicit
    runtime_error(const string& __arg) ;
# 239 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/stdexcept" 3
    runtime_error(const runtime_error&) throw();
    runtime_error& operator=(const runtime_error&) throw();





    virtual ~runtime_error() throw();



    virtual const char*
    what() const throw();





  };


  class range_error : public runtime_error
  {
  public:
    explicit range_error(const string& __arg) ;







    virtual ~range_error() throw();
  };


  class overflow_error : public runtime_error
  {
  public:
    explicit overflow_error(const string& __arg) ;







    virtual ~overflow_error() throw();
  };


  class underflow_error : public runtime_error
  {
  public:
    explicit underflow_error(const string& __arg) ;







    virtual ~underflow_error() throw();
  };




}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 2 3




namespace std __attribute__ ((__visibility__ ("default")))
{






  enum _Ios_Fmtflags
    {
      _S_boolalpha = 1L << 0,
      _S_dec = 1L << 1,
      _S_fixed = 1L << 2,
      _S_hex = 1L << 3,
      _S_internal = 1L << 4,
      _S_left = 1L << 5,
      _S_oct = 1L << 6,
      _S_right = 1L << 7,
      _S_scientific = 1L << 8,
      _S_showbase = 1L << 9,
      _S_showpoint = 1L << 10,
      _S_showpos = 1L << 11,
      _S_skipws = 1L << 12,
      _S_unitbuf = 1L << 13,
      _S_uppercase = 1L << 14,
      _S_adjustfield = _S_left | _S_right | _S_internal,
      _S_basefield = _S_dec | _S_oct | _S_hex,
      _S_floatfield = _S_scientific | _S_fixed,
      _S_ios_fmtflags_end = 1L << 16,
      _S_ios_fmtflags_max = 0x7fffffff,
      _S_ios_fmtflags_min = ~0x7fffffff
    };

 
  inline _Ios_Fmtflags
  operator&(_Ios_Fmtflags __a, _Ios_Fmtflags __b) throw()
  { return _Ios_Fmtflags(static_cast<int>(__a) & static_cast<int>(__b)); }

 
  inline _Ios_Fmtflags
  operator|(_Ios_Fmtflags __a, _Ios_Fmtflags __b) throw()
  { return _Ios_Fmtflags(static_cast<int>(__a) | static_cast<int>(__b)); }

 
  inline _Ios_Fmtflags
  operator^(_Ios_Fmtflags __a, _Ios_Fmtflags __b) throw()
  { return _Ios_Fmtflags(static_cast<int>(__a) ^ static_cast<int>(__b)); }

 
  inline _Ios_Fmtflags
  operator~(_Ios_Fmtflags __a) throw()
  { return _Ios_Fmtflags(~static_cast<int>(__a)); }

 
  inline const _Ios_Fmtflags&
  operator|=(_Ios_Fmtflags& __a, _Ios_Fmtflags __b) throw()
  { return __a = __a | __b; }

 
  inline const _Ios_Fmtflags&
  operator&=(_Ios_Fmtflags& __a, _Ios_Fmtflags __b) throw()
  { return __a = __a & __b; }

 
  inline const _Ios_Fmtflags&
  operator^=(_Ios_Fmtflags& __a, _Ios_Fmtflags __b) throw()
  { return __a = __a ^ __b; }
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
  enum __attribute__((__flag_enum__)) _Ios_Openmode
    {
      _S_app = 1L << 0,
      _S_ate = 1L << 1,
      _S_bin = 1L << 2,
      _S_in = 1L << 3,
      _S_out = 1L << 4,
      _S_trunc = 1L << 5,
      _S_noreplace __attribute__((__unused__)) = 1L << 6,
      _S_ios_openmode_end __attribute__((__unused__)) = 1L << 16,
      _S_ios_openmode_max __attribute__((__unused__)) = 0x7fffffff,
      _S_ios_openmode_min __attribute__((__unused__)) = ~0x7fffffff
    };



 
  inline _Ios_Openmode
  operator&(_Ios_Openmode __a, _Ios_Openmode __b) throw()
  { return _Ios_Openmode(static_cast<int>(__a) & static_cast<int>(__b)); }

 
  inline _Ios_Openmode
  operator|(_Ios_Openmode __a, _Ios_Openmode __b) throw()
  { return _Ios_Openmode(static_cast<int>(__a) | static_cast<int>(__b)); }

 
  inline _Ios_Openmode
  operator^(_Ios_Openmode __a, _Ios_Openmode __b) throw()
  { return _Ios_Openmode(static_cast<int>(__a) ^ static_cast<int>(__b)); }

 
  inline _Ios_Openmode
  operator~(_Ios_Openmode __a) throw()
  { return _Ios_Openmode(~static_cast<int>(__a)); }

 
  inline const _Ios_Openmode&
  operator|=(_Ios_Openmode& __a, _Ios_Openmode __b) throw()
  { return __a = __a | __b; }

 
  inline const _Ios_Openmode&
  operator&=(_Ios_Openmode& __a, _Ios_Openmode __b) throw()
  { return __a = __a & __b; }

 
  inline const _Ios_Openmode&
  operator^=(_Ios_Openmode& __a, _Ios_Openmode __b) throw()
  { return __a = __a ^ __b; }


  enum _Ios_Iostate
    {
      _S_goodbit = 0,
      _S_badbit = 1L << 0,
      _S_eofbit = 1L << 1,
      _S_failbit = 1L << 2,
      _S_ios_iostate_end = 1L << 16,
      _S_ios_iostate_max = 0x7fffffff,
      _S_ios_iostate_min = ~0x7fffffff
    };

 
  inline _Ios_Iostate
  operator&(_Ios_Iostate __a, _Ios_Iostate __b) throw()
  { return _Ios_Iostate(static_cast<int>(__a) & static_cast<int>(__b)); }

 
  inline _Ios_Iostate
  operator|(_Ios_Iostate __a, _Ios_Iostate __b) throw()
  { return _Ios_Iostate(static_cast<int>(__a) | static_cast<int>(__b)); }

 
  inline _Ios_Iostate
  operator^(_Ios_Iostate __a, _Ios_Iostate __b) throw()
  { return _Ios_Iostate(static_cast<int>(__a) ^ static_cast<int>(__b)); }

 
  inline _Ios_Iostate
  operator~(_Ios_Iostate __a) throw()
  { return _Ios_Iostate(~static_cast<int>(__a)); }

 
  inline const _Ios_Iostate&
  operator|=(_Ios_Iostate& __a, _Ios_Iostate __b) throw()
  { return __a = __a | __b; }

 
  inline const _Ios_Iostate&
  operator&=(_Ios_Iostate& __a, _Ios_Iostate __b) throw()
  { return __a = __a & __b; }

 
  inline const _Ios_Iostate&
  operator^=(_Ios_Iostate& __a, _Ios_Iostate __b) throw()
  { return __a = __a ^ __b; }


  enum _Ios_Seekdir
    {
      _S_beg = 0,
      _S_cur = 1,
      _S_end = 2,
      _S_ios_seekdir_end = 1L << 16
    };
# 265 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
  class ios_base
  {



    struct system_error : std::runtime_error
    {

      struct error_code
      {
 error_code() { }
      private:
 int _M_value;
 const void* _M_cat;
      } _M_code;
    };


  public:
# 292 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    class __attribute ((__abi_tag__ ("cxx11"))) failure : public system_error
    {
    public:
      explicit
      failure(const string& __str);
# 306 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
      virtual
      ~failure() throw();

      virtual const char*
      what() const throw();
    };
# 378 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    typedef _Ios_Fmtflags fmtflags;


    static const fmtflags boolalpha = _S_boolalpha;


    static const fmtflags dec = _S_dec;


    static const fmtflags fixed = _S_fixed;


    static const fmtflags hex = _S_hex;




    static const fmtflags internal = _S_internal;



    static const fmtflags left = _S_left;


    static const fmtflags oct = _S_oct;



    static const fmtflags right = _S_right;


    static const fmtflags scientific = _S_scientific;



    static const fmtflags showbase = _S_showbase;



    static const fmtflags showpoint = _S_showpoint;


    static const fmtflags showpos = _S_showpos;


    static const fmtflags skipws = _S_skipws;


    static const fmtflags unitbuf = _S_unitbuf;



    static const fmtflags uppercase = _S_uppercase;


    static const fmtflags adjustfield = _S_adjustfield;


    static const fmtflags basefield = _S_basefield;


    static const fmtflags floatfield = _S_floatfield;
# 453 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    typedef _Ios_Iostate iostate;



    static const iostate badbit = _S_badbit;


    static const iostate eofbit = _S_eofbit;




    static const iostate failbit = _S_failbit;


    static const iostate goodbit = _S_goodbit;
# 484 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    typedef _Ios_Openmode openmode;


    static const openmode app = _S_app;


    static const openmode ate = _S_ate;




    static const openmode binary = _S_bin;


    static const openmode in = _S_in;


    static const openmode out = _S_out;


    static const openmode trunc = _S_trunc;

    static const openmode __noreplace = _S_noreplace;
# 523 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    typedef _Ios_Seekdir seekdir;


    static const seekdir beg = _S_beg;


    static const seekdir cur = _S_cur;


    static const seekdir end = _S_end;



    typedef int io_state
      __attribute__ ((__deprecated__ ("use '" "std::iostate" "' instead")));
    typedef int open_mode
      __attribute__ ((__deprecated__ ("use '" "std::openmode" "' instead")));
    typedef int seek_dir
      __attribute__ ((__deprecated__ ("use '" "std::seekdir" "' instead")));

    typedef std::streampos streampos
      __attribute__ ((__deprecated__ ("use '" "std::streampos" "' instead")));
    typedef std::streamoff streamoff
      __attribute__ ((__deprecated__ ("use '" "std::streamoff" "' instead")));
# 556 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    enum event
    {
      erase_event,
      imbue_event,
      copyfmt_event
    };
# 573 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    typedef void (*event_callback) (event __e, ios_base& __b, int __i);
# 585 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    void
    register_callback(event_callback __fn, int __index);

  protected:
    streamsize _M_precision;
    streamsize _M_width;
    fmtflags _M_flags;
    iostate _M_exception;
    iostate _M_streambuf_state;



    struct _Callback_list
    {

      _Callback_list* _M_next;
      ios_base::event_callback _M_fn;
      int _M_index;
      _Atomic_word _M_refcount;

      _Callback_list(ios_base::event_callback __fn, int __index,
       _Callback_list* __cb)
      : _M_next(__cb), _M_fn(__fn), _M_index(__index), _M_refcount(0) { }

      void
      _M_add_reference() { __gnu_cxx::__atomic_add_dispatch(&_M_refcount, 1); }


      int
      _M_remove_reference()
      {

        ;
        int __res = __gnu_cxx::__exchange_and_add_dispatch(&_M_refcount, -1);
        if (__res == 0)
          {
            ;
          }
        return __res;
      }
    };

     _Callback_list* _M_callbacks;

    void
    _M_call_callbacks(event __ev) throw();

    void
    _M_dispose_callbacks(void) throw();


    struct _Words
    {
      void* _M_pword;
      long _M_iword;
      _Words() : _M_pword(0), _M_iword(0) { }
    };


    _Words _M_word_zero;



    enum { _S_local_word_size = 8 };
    _Words _M_local_word[_S_local_word_size];


    int _M_word_size;
    _Words* _M_word;

    _Words&
    _M_grow_words(int __index, bool __iword);


    locale _M_ios_locale;

    void
    _M_init() throw();

  public:





    class Init
    {
      friend class ios_base;
    public:
      Init();
      ~Init();






    private:
      static _Atomic_word _S_refcount;
      static bool _S_synced_with_stdio;
    };






   
    fmtflags
    flags() const
    { return _M_flags; }
# 704 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    fmtflags
    flags(fmtflags __fmtfl)
    {
      fmtflags __old = _M_flags;
      _M_flags = __fmtfl;
      return __old;
    }
# 720 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    fmtflags
    setf(fmtflags __fmtfl)
    {
      fmtflags __old = _M_flags;
      _M_flags |= __fmtfl;
      return __old;
    }
# 737 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    fmtflags
    setf(fmtflags __fmtfl, fmtflags __mask)
    {
      fmtflags __old = _M_flags;
      _M_flags &= ~__mask;
      _M_flags |= (__fmtfl & __mask);
      return __old;
    }







    void
    unsetf(fmtflags __mask)
    { _M_flags &= ~__mask; }
# 763 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
   
    streamsize
    precision() const
    { return _M_precision; }






    streamsize
    precision(streamsize __prec)
    {
      streamsize __old = _M_precision;
      _M_precision = __prec;
      return __old;
    }







   
    streamsize
    width() const
    { return _M_width; }






    streamsize
    width(streamsize __wide)
    {
      streamsize __old = _M_width;
      _M_width = __wide;
      return __old;
    }
# 816 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    static bool
    sync_with_stdio(bool __sync = true);
# 828 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    locale
    imbue(const locale& __loc) throw();
# 839 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
   
    locale
    getloc() const
    { return _M_ios_locale; }
# 851 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    const locale&
    _M_getloc() const
    { return _M_ios_locale; }
# 870 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    static int
    xalloc() throw();
# 886 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    long&
    iword(int __ix)
    {
      _Words& __word = ((unsigned)__ix < (unsigned)_M_word_size)
   ? _M_word[__ix] : _M_grow_words(__ix, true);
      return __word._M_iword;
    }
# 907 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    void*&
    pword(int __ix)
    {
      _Words& __word = ((unsigned)__ix < (unsigned)_M_word_size)
   ? _M_word[__ix] : _M_grow_words(__ix, false);
      return __word._M_pword;
    }
# 924 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
    virtual ~ios_base();

  protected:
    ios_base() throw ();




  private:
    ios_base(const ios_base&);

    ios_base&
    operator=(const ios_base&);
# 951 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3
  };



  inline ios_base&
  boolalpha(ios_base& __base)
  {
    __base.setf(ios_base::boolalpha);
    return __base;
  }


  inline ios_base&
  noboolalpha(ios_base& __base)
  {
    __base.unsetf(ios_base::boolalpha);
    return __base;
  }


  inline ios_base&
  showbase(ios_base& __base)
  {
    __base.setf(ios_base::showbase);
    return __base;
  }


  inline ios_base&
  noshowbase(ios_base& __base)
  {
    __base.unsetf(ios_base::showbase);
    return __base;
  }


  inline ios_base&
  showpoint(ios_base& __base)
  {
    __base.setf(ios_base::showpoint);
    return __base;
  }


  inline ios_base&
  noshowpoint(ios_base& __base)
  {
    __base.unsetf(ios_base::showpoint);
    return __base;
  }


  inline ios_base&
  showpos(ios_base& __base)
  {
    __base.setf(ios_base::showpos);
    return __base;
  }


  inline ios_base&
  noshowpos(ios_base& __base)
  {
    __base.unsetf(ios_base::showpos);
    return __base;
  }


  inline ios_base&
  skipws(ios_base& __base)
  {
    __base.setf(ios_base::skipws);
    return __base;
  }


  inline ios_base&
  noskipws(ios_base& __base)
  {
    __base.unsetf(ios_base::skipws);
    return __base;
  }


  inline ios_base&
  uppercase(ios_base& __base)
  {
    __base.setf(ios_base::uppercase);
    return __base;
  }


  inline ios_base&
  nouppercase(ios_base& __base)
  {
    __base.unsetf(ios_base::uppercase);
    return __base;
  }


  inline ios_base&
  unitbuf(ios_base& __base)
  {
     __base.setf(ios_base::unitbuf);
     return __base;
  }


  inline ios_base&
  nounitbuf(ios_base& __base)
  {
     __base.unsetf(ios_base::unitbuf);
     return __base;
  }



  inline ios_base&
  internal(ios_base& __base)
  {
     __base.setf(ios_base::internal, ios_base::adjustfield);
     return __base;
  }


  inline ios_base&
  left(ios_base& __base)
  {
    __base.setf(ios_base::left, ios_base::adjustfield);
    return __base;
  }


  inline ios_base&
  right(ios_base& __base)
  {
    __base.setf(ios_base::right, ios_base::adjustfield);
    return __base;
  }



  inline ios_base&
  dec(ios_base& __base)
  {
    __base.setf(ios_base::dec, ios_base::basefield);
    return __base;
  }


  inline ios_base&
  hex(ios_base& __base)
  {
    __base.setf(ios_base::hex, ios_base::basefield);
    return __base;
  }


  inline ios_base&
  oct(ios_base& __base)
  {
    __base.setf(ios_base::oct, ios_base::basefield);
    return __base;
  }



  inline ios_base&
  fixed(ios_base& __base)
  {
    __base.setf(ios_base::fixed, ios_base::floatfield);
    return __base;
  }


  inline ios_base&
  scientific(ios_base& __base)
  {
    __base.setf(ios_base::scientific, ios_base::floatfield);
    return __base;
  }
# 1154 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ios_base.h" 3

}
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 1 3
# 49 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
namespace std __attribute__ ((__visibility__ ("default")))
{




  template<typename _CharT, typename _Traits>
    streamsize
    __copy_streambufs_eof(basic_streambuf<_CharT, _Traits>*,
     basic_streambuf<_CharT, _Traits>*, bool&);
# 125 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
  template<typename _CharT, typename _Traits>
    class basic_streambuf
    {
    public:






      typedef _CharT char_type;
      typedef _Traits traits_type;
      typedef typename traits_type::int_type int_type;
      typedef typename traits_type::pos_type pos_type;
      typedef typename traits_type::off_type off_type;




      typedef basic_streambuf<char_type, traits_type> __streambuf_type;


      friend class basic_ios<char_type, traits_type>;
      friend class basic_istream<char_type, traits_type>;
      friend class basic_ostream<char_type, traits_type>;
      friend class istreambuf_iterator<char_type, traits_type>;
      friend class ostreambuf_iterator<char_type, traits_type>;

      friend streamsize
      __copy_streambufs_eof<>(basic_streambuf*, basic_streambuf*, bool&);

      template<bool _IsMove, typename _CharT2>
        friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
            _CharT2*>::__type
        __copy_move_a2(istreambuf_iterator<_CharT2>,
         istreambuf_iterator<_CharT2>, _CharT2*);

      template<typename _CharT2>
        friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
      istreambuf_iterator<_CharT2> >::__type
        find(istreambuf_iterator<_CharT2>, istreambuf_iterator<_CharT2>,
      const _CharT2&);

      template<typename _CharT2, typename _Distance>
        friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
            void>::__type
        advance(istreambuf_iterator<_CharT2>&, _Distance);

      friend void __istream_extract(istream&, char*, streamsize);

      template<typename _CharT2, typename _Traits2, typename _Alloc>
        friend basic_istream<_CharT2, _Traits2>&
        operator>>(basic_istream<_CharT2, _Traits2>&,
     basic_string<_CharT2, _Traits2, _Alloc>&);

      template<typename _CharT2, typename _Traits2, typename _Alloc>
        friend basic_istream<_CharT2, _Traits2>&
        getline(basic_istream<_CharT2, _Traits2>&,
  basic_string<_CharT2, _Traits2, _Alloc>&, _CharT2);

    protected:







      char_type* _M_in_beg;
      char_type* _M_in_cur;
      char_type* _M_in_end;
      char_type* _M_out_beg;
      char_type* _M_out_cur;
      char_type* _M_out_end;


      locale _M_buf_locale;

  public:

      virtual
      ~basic_streambuf()
      { }
# 217 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      locale
      pubimbue(const locale& __loc)
      {
 locale __tmp(this->getloc());
 this->imbue(__loc);
 _M_buf_locale = __loc;
 return __tmp;
      }
# 234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      locale
      getloc() const
      { return _M_buf_locale; }
# 247 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      basic_streambuf*
      pubsetbuf(char_type* __s, streamsize __n)
      { return this->setbuf(__s, __n); }
# 259 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      pos_type
      pubseekoff(off_type __off, ios_base::seekdir __way,
   ios_base::openmode __mode = ios_base::in | ios_base::out)
      { return this->seekoff(__off, __way, __mode); }
# 271 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      pos_type
      pubseekpos(pos_type __sp,
   ios_base::openmode __mode = ios_base::in | ios_base::out)
      { return this->seekpos(__sp, __mode); }




      int
      pubsync() { return this->sync(); }
# 292 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      streamsize
      in_avail()
      {
 const streamsize __ret = this->egptr() - this->gptr();
 return __ret ? __ret : this->showmanyc();
      }
# 306 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      int_type
      snextc()
      {
 int_type __ret = traits_type::eof();
 if (__builtin_expect(!traits_type::eq_int_type(this->sbumpc(),
             __ret), true))
   __ret = this->sgetc();
 return __ret;
      }
# 324 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      int_type
      sbumpc()
      {
 int_type __ret;
 if (__builtin_expect(this->gptr() < this->egptr(), true))
   {
     __ret = traits_type::to_int_type(*this->gptr());
     this->gbump(1);
   }
 else
   __ret = this->uflow();
 return __ret;
      }
# 346 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      int_type
      sgetc()
      {
 int_type __ret;
 if (__builtin_expect(this->gptr() < this->egptr(), true))
   __ret = traits_type::to_int_type(*this->gptr());
 else
   __ret = this->underflow();
 return __ret;
      }
# 365 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      streamsize
      sgetn(char_type* __s, streamsize __n)
      { return this->xsgetn(__s, __n); }
# 380 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      int_type
      sputbackc(char_type __c)
      {
 int_type __ret;
 const bool __testpos = this->eback() < this->gptr();
 if (__builtin_expect(!__testpos ||
        !traits_type::eq(__c, this->gptr()[-1]), false))
   __ret = this->pbackfail(traits_type::to_int_type(__c));
 else
   {
     this->gbump(-1);
     __ret = traits_type::to_int_type(*this->gptr());
   }
 return __ret;
      }
# 405 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      int_type
      sungetc()
      {
 int_type __ret;
 if (__builtin_expect(this->eback() < this->gptr(), true))
   {
     this->gbump(-1);
     __ret = traits_type::to_int_type(*this->gptr());
   }
 else
   __ret = this->pbackfail();
 return __ret;
      }
# 432 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      int_type
      sputc(char_type __c)
      {
 int_type __ret;
 if (__builtin_expect(this->pptr() < this->epptr(), true))
   {
     *this->pptr() = __c;
     this->pbump(1);
     __ret = traits_type::to_int_type(__c);
   }
 else
   __ret = this->overflow(traits_type::to_int_type(__c));
 return __ret;
      }
# 458 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      streamsize
      sputn(const char_type* __s, streamsize __n)
      { return this->xsputn(__s, __n); }

    protected:
# 472 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      basic_streambuf()
      : _M_in_beg(0), _M_in_cur(0), _M_in_end(0),
      _M_out_beg(0), _M_out_cur(0), _M_out_end(0),
      _M_buf_locale(locale())
      { }
# 490 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      char_type*
      eback() const { return _M_in_beg; }

      char_type*
      gptr() const { return _M_in_cur; }

      char_type*
      egptr() const { return _M_in_end; }
# 506 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      void
      gbump(int __n) { _M_in_cur += __n; }
# 517 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      void
      setg(char_type* __gbeg, char_type* __gnext, char_type* __gend)
      {
 _M_in_beg = __gbeg;
 _M_in_cur = __gnext;
 _M_in_end = __gend;
      }
# 537 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      char_type*
      pbase() const { return _M_out_beg; }

      char_type*
      pptr() const { return _M_out_cur; }

      char_type*
      epptr() const { return _M_out_end; }
# 553 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      void
      pbump(int __n) { _M_out_cur += __n; }
# 563 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      void
      setp(char_type* __pbeg, char_type* __pend)
      {
 _M_out_beg = _M_out_cur = __pbeg;
 _M_out_end = __pend;
      }
# 584 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual void
      imbue(const locale& __loc __attribute__ ((__unused__)))
      { }
# 599 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual basic_streambuf<char_type,_Traits>*
      setbuf(char_type*, streamsize)
      { return this; }
# 610 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual pos_type
      seekoff(off_type, ios_base::seekdir,
       ios_base::openmode = ios_base::in | ios_base::out)
      { return pos_type(off_type(-1)); }
# 622 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual pos_type
      seekpos(pos_type,
       ios_base::openmode = ios_base::in | ios_base::out)
      { return pos_type(off_type(-1)); }
# 635 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual int
      sync() { return 0; }
# 657 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual streamsize
      showmanyc() { return 0; }
# 673 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual streamsize
      xsgetn(char_type* __s, streamsize __n);
# 695 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual int_type
      underflow()
      { return traits_type::eof(); }
# 708 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual int_type
      uflow()
      {
 int_type __ret = traits_type::eof();
 const bool __testeof = traits_type::eq_int_type(this->underflow(),
       __ret);
 if (!__testeof)
   {
     __ret = traits_type::to_int_type(*this->gptr());
     this->gbump(1);
   }
 return __ret;
      }
# 732 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual int_type
      pbackfail(int_type __c __attribute__ ((__unused__)) = traits_type::eof())
      { return traits_type::eof(); }
# 750 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual streamsize
      xsputn(const char_type* __s, streamsize __n);
# 776 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      virtual int_type
      overflow(int_type __c __attribute__ ((__unused__)) = traits_type::eof())
      { return traits_type::eof(); }



    public:
# 791 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
      __attribute__ ((__deprecated__ ("use '" "std::basic_streambuf::sbumpc" "' instead")))
      void
      stossc()
      {
 if (this->gptr() < this->egptr())
   this->gbump(1);
 else
   this->uflow();
      }



      void
      __safe_gbump(streamsize __n) { _M_in_cur += __n; }

      void
      __safe_pbump(streamsize __n) { _M_out_cur += __n; }


    private:



      basic_streambuf(const basic_streambuf&);

      basic_streambuf&
      operator=(const basic_streambuf&);
# 832 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
    };
# 846 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 3
  template<>
    streamsize
    __copy_streambufs_eof(basic_streambuf<char>* __sbin,
     basic_streambuf<char>* __sbout, bool& __ineof);

  template<>
    streamsize
    __copy_streambufs_eof(basic_streambuf<wchar_t>* __sbin,
     basic_streambuf<wchar_t>* __sbout, bool& __ineof);





}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf.tcc" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _CharT, typename _Traits>
    streamsize
    basic_streambuf<_CharT, _Traits>::
    xsgetn(char_type* __s, streamsize __n)
    {
      streamsize __ret = 0;
      while (__ret < __n)
 {
   const streamsize __buf_len = this->egptr() - this->gptr();
   if (__buf_len)
     {
       const streamsize __remaining = __n - __ret;
       const streamsize __len = std::min(__buf_len, __remaining);
       traits_type::copy(__s, this->gptr(), __len);
       __ret += __len;
       __s += __len;
       this->__safe_gbump(__len);
     }

   if (__ret < __n)
     {
       const int_type __c = this->uflow();
       if (!traits_type::eq_int_type(__c, traits_type::eof()))
  {
    traits_type::assign(*__s++, traits_type::to_char_type(__c));
    ++__ret;
  }
       else
  break;
     }
 }
      return __ret;
    }

  template<typename _CharT, typename _Traits>
    streamsize
    basic_streambuf<_CharT, _Traits>::
    xsputn(const char_type* __s, streamsize __n)
    {
      streamsize __ret = 0;
      while (__ret < __n)
 {
   const streamsize __buf_len = this->epptr() - this->pptr();
   if (__buf_len)
     {
       const streamsize __remaining = __n - __ret;
       const streamsize __len = std::min(__buf_len, __remaining);
       traits_type::copy(this->pptr(), __s, __len);
       __ret += __len;
       __s += __len;
       this->__safe_pbump(__len);
     }

   if (__ret < __n)
     {
       int_type __c = this->overflow(traits_type::to_int_type(*__s));
       if (!traits_type::eq_int_type(__c, traits_type::eof()))
  {
    ++__ret;
    ++__s;
  }
       else
  break;
     }
 }
      return __ret;
    }




  template<typename _CharT, typename _Traits>
    streamsize
    __copy_streambufs_eof(basic_streambuf<_CharT, _Traits>* __sbin,
     basic_streambuf<_CharT, _Traits>* __sbout,
     bool& __ineof)
    {
      streamsize __ret = 0;
      __ineof = true;
      typename _Traits::int_type __c = __sbin->sgetc();
      while (!_Traits::eq_int_type(__c, _Traits::eof()))
 {
   __c = __sbout->sputc(_Traits::to_char_type(__c));
   if (_Traits::eq_int_type(__c, _Traits::eof()))
     {
       __ineof = false;
       break;
     }
   ++__ret;
   __c = __sbin->snextc();
 }
      return __ret;
    }

  template<typename _CharT, typename _Traits>
    inline streamsize
    __copy_streambufs(basic_streambuf<_CharT, _Traits>* __sbin,
        basic_streambuf<_CharT, _Traits>* __sbout)
    {
      bool __ineof;
      return __copy_streambufs_eof(__sbin, __sbout, __ineof);
    }




  extern template class basic_streambuf<char>;

  extern template
    streamsize
    __copy_streambufs(basic_streambuf<char>*,
        basic_streambuf<char>*);


  extern template class basic_streambuf<wchar_t>;

  extern template
    streamsize
    __copy_streambufs(basic_streambuf<wchar_t>*,
        basic_streambuf<wchar_t>*);




}

#pragma GCC diagnostic pop
# 863 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/streambuf" 2 3
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwctype" 1 3
# 55 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwctype" 3
# 1 "/usr/include/wctype.h" 1 3 4
# 33 "/usr/include/wctype.h" 3 4
# 1 "/usr/include/wchar.h" 1 3 4
# 34 "/usr/include/wctype.h" 2 3 4
# 49 "/usr/include/wctype.h" 3 4



typedef unsigned long int wctype_t;

# 71 "/usr/include/wctype.h" 3 4
enum
{
  __ISwupper = 0,
  __ISwlower = 1,
  __ISwalpha = 2,
  __ISwdigit = 3,
  __ISwxdigit = 4,
  __ISwspace = 5,
  __ISwprint = 6,
  __ISwgraph = 7,
  __ISwblank = 8,
  __ISwcntrl = 9,
  __ISwpunct = 10,
  __ISwalnum = 11,

  _ISwupper = ((__ISwupper) < 8 ? (int) ((1UL << (__ISwupper)) << 24) : ((__ISwupper) < 16 ? (int) ((1UL << (__ISwupper)) << 8) : ((__ISwupper) < 24 ? (int) ((1UL << (__ISwupper)) >> 8) : (int) ((1UL << (__ISwupper)) >> 24)))),
  _ISwlower = ((__ISwlower) < 8 ? (int) ((1UL << (__ISwlower)) << 24) : ((__ISwlower) < 16 ? (int) ((1UL << (__ISwlower)) << 8) : ((__ISwlower) < 24 ? (int) ((1UL << (__ISwlower)) >> 8) : (int) ((1UL << (__ISwlower)) >> 24)))),
  _ISwalpha = ((__ISwalpha) < 8 ? (int) ((1UL << (__ISwalpha)) << 24) : ((__ISwalpha) < 16 ? (int) ((1UL << (__ISwalpha)) << 8) : ((__ISwalpha) < 24 ? (int) ((1UL << (__ISwalpha)) >> 8) : (int) ((1UL << (__ISwalpha)) >> 24)))),
  _ISwdigit = ((__ISwdigit) < 8 ? (int) ((1UL << (__ISwdigit)) << 24) : ((__ISwdigit) < 16 ? (int) ((1UL << (__ISwdigit)) << 8) : ((__ISwdigit) < 24 ? (int) ((1UL << (__ISwdigit)) >> 8) : (int) ((1UL << (__ISwdigit)) >> 24)))),
  _ISwxdigit = ((__ISwxdigit) < 8 ? (int) ((1UL << (__ISwxdigit)) << 24) : ((__ISwxdigit) < 16 ? (int) ((1UL << (__ISwxdigit)) << 8) : ((__ISwxdigit) < 24 ? (int) ((1UL << (__ISwxdigit)) >> 8) : (int) ((1UL << (__ISwxdigit)) >> 24)))),
  _ISwspace = ((__ISwspace) < 8 ? (int) ((1UL << (__ISwspace)) << 24) : ((__ISwspace) < 16 ? (int) ((1UL << (__ISwspace)) << 8) : ((__ISwspace) < 24 ? (int) ((1UL << (__ISwspace)) >> 8) : (int) ((1UL << (__ISwspace)) >> 24)))),
  _ISwprint = ((__ISwprint) < 8 ? (int) ((1UL << (__ISwprint)) << 24) : ((__ISwprint) < 16 ? (int) ((1UL << (__ISwprint)) << 8) : ((__ISwprint) < 24 ? (int) ((1UL << (__ISwprint)) >> 8) : (int) ((1UL << (__ISwprint)) >> 24)))),
  _ISwgraph = ((__ISwgraph) < 8 ? (int) ((1UL << (__ISwgraph)) << 24) : ((__ISwgraph) < 16 ? (int) ((1UL << (__ISwgraph)) << 8) : ((__ISwgraph) < 24 ? (int) ((1UL << (__ISwgraph)) >> 8) : (int) ((1UL << (__ISwgraph)) >> 24)))),
  _ISwblank = ((__ISwblank) < 8 ? (int) ((1UL << (__ISwblank)) << 24) : ((__ISwblank) < 16 ? (int) ((1UL << (__ISwblank)) << 8) : ((__ISwblank) < 24 ? (int) ((1UL << (__ISwblank)) >> 8) : (int) ((1UL << (__ISwblank)) >> 24)))),
  _ISwcntrl = ((__ISwcntrl) < 8 ? (int) ((1UL << (__ISwcntrl)) << 24) : ((__ISwcntrl) < 16 ? (int) ((1UL << (__ISwcntrl)) << 8) : ((__ISwcntrl) < 24 ? (int) ((1UL << (__ISwcntrl)) >> 8) : (int) ((1UL << (__ISwcntrl)) >> 24)))),
  _ISwpunct = ((__ISwpunct) < 8 ? (int) ((1UL << (__ISwpunct)) << 24) : ((__ISwpunct) < 16 ? (int) ((1UL << (__ISwpunct)) << 8) : ((__ISwpunct) < 24 ? (int) ((1UL << (__ISwpunct)) >> 8) : (int) ((1UL << (__ISwpunct)) >> 24)))),
  _ISwalnum = ((__ISwalnum) < 8 ? (int) ((1UL << (__ISwalnum)) << 24) : ((__ISwalnum) < 16 ? (int) ((1UL << (__ISwalnum)) << 8) : ((__ISwalnum) < 24 ? (int) ((1UL << (__ISwalnum)) >> 8) : (int) ((1UL << (__ISwalnum)) >> 24))))
};



extern "C" {








extern int iswalnum (wint_t __wc) throw ();





extern int iswalpha (wint_t __wc) throw ();


extern int iswcntrl (wint_t __wc) throw ();



extern int iswdigit (wint_t __wc) throw ();



extern int iswgraph (wint_t __wc) throw ();




extern int iswlower (wint_t __wc) throw ();


extern int iswprint (wint_t __wc) throw ();




extern int iswpunct (wint_t __wc) throw ();




extern int iswspace (wint_t __wc) throw ();




extern int iswupper (wint_t __wc) throw ();




extern int iswxdigit (wint_t __wc) throw ();





extern int iswblank (wint_t __wc) throw ();
# 171 "/usr/include/wctype.h" 3 4
extern wctype_t wctype (const char *__property) throw ();



extern int iswctype (wint_t __wc, wctype_t __desc) throw ();










typedef const __int32_t *wctrans_t;







extern wint_t towlower (wint_t __wc) throw ();


extern wint_t towupper (wint_t __wc) throw ();


}
# 213 "/usr/include/wctype.h" 3 4
extern "C" {




extern wctrans_t wctrans (const char *__property) throw ();


extern wint_t towctrans (wint_t __wc, wctrans_t __desc) throw ();








extern int iswalnum_l (wint_t __wc, __locale_t __locale) throw ();





extern int iswalpha_l (wint_t __wc, __locale_t __locale) throw ();


extern int iswcntrl_l (wint_t __wc, __locale_t __locale) throw ();



extern int iswdigit_l (wint_t __wc, __locale_t __locale) throw ();



extern int iswgraph_l (wint_t __wc, __locale_t __locale) throw ();




extern int iswlower_l (wint_t __wc, __locale_t __locale) throw ();


extern int iswprint_l (wint_t __wc, __locale_t __locale) throw ();




extern int iswpunct_l (wint_t __wc, __locale_t __locale) throw ();




extern int iswspace_l (wint_t __wc, __locale_t __locale) throw ();




extern int iswupper_l (wint_t __wc, __locale_t __locale) throw ();




extern int iswxdigit_l (wint_t __wc, __locale_t __locale) throw ();




extern int iswblank_l (wint_t __wc, __locale_t __locale) throw ();



extern wctype_t wctype_l (const char *__property, __locale_t __locale)
     throw ();



extern int iswctype_l (wint_t __wc, wctype_t __desc, __locale_t __locale)
     throw ();







extern wint_t towlower_l (wint_t __wc, __locale_t __locale) throw ();


extern wint_t towupper_l (wint_t __wc, __locale_t __locale) throw ();



extern wctrans_t wctrans_l (const char *__property, __locale_t __locale)
     throw ();


extern wint_t towctrans_l (wint_t __wc, wctrans_t __desc,
      __locale_t __locale) throw ();



}
# 56 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwctype" 2 3
# 82 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cwctype" 3
namespace std
{
  using ::wctrans_t;
  using ::wctype_t;
  using ::wint_t;

  using ::iswalnum;
  using ::iswalpha;

  using ::iswblank;

  using ::iswcntrl;
  using ::iswctype;
  using ::iswdigit;
  using ::iswgraph;
  using ::iswlower;
  using ::iswprint;
  using ::iswpunct;
  using ::iswspace;
  using ::iswupper;
  using ::iswxdigit;
  using ::towctrans;
  using ::towlower;
  using ::towupper;
  using ::wctrans;
  using ::wctype;
}
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/ctype_base.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/ctype_base.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{



  struct ctype_base
  {

    typedef const int* __to_type;



    typedef unsigned short mask;
    static const mask upper = _ISupper;
    static const mask lower = _ISlower;
    static const mask alpha = _ISalpha;
    static const mask digit = _ISdigit;
    static const mask xdigit = _ISxdigit;
    static const mask space = _ISspace;
    static const mask print = _ISprint;
    static const mask graph = _ISalpha | _ISdigit | _ISpunct;
    static const mask cntrl = _IScntrl;
    static const mask punct = _ISpunct;
    static const mask alnum = _ISalpha | _ISdigit;



  };


}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 2 3






# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{








 
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"


  template<typename _CharT, typename _Traits>
    class istreambuf_iterator
    : public iterator<input_iterator_tag, _CharT, typename _Traits::off_type,
        _CharT*, _CharT>
    {
    public:




      typedef _CharT& reference;






      typedef _CharT char_type;
      typedef _Traits traits_type;
      typedef typename _Traits::int_type int_type;
      typedef basic_streambuf<_CharT, _Traits> streambuf_type;
      typedef basic_istream<_CharT, _Traits> istream_type;


      template<typename _CharT2>
 friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
        ostreambuf_iterator<_CharT2> >::__type
 copy(istreambuf_iterator<_CharT2>, istreambuf_iterator<_CharT2>,
      ostreambuf_iterator<_CharT2>);

      template<bool _IsMove, typename _CharT2>
 friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
            _CharT2*>::__type
 __copy_move_a2(istreambuf_iterator<_CharT2>,
         istreambuf_iterator<_CharT2>, _CharT2*);

      template<typename _CharT2, typename _Size>
 friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
            _CharT2*>::__type
 __copy_n_a(istreambuf_iterator<_CharT2>, _Size, _CharT2*, bool);

      template<typename _CharT2>
 friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
        istreambuf_iterator<_CharT2> >::__type
 find(istreambuf_iterator<_CharT2>, istreambuf_iterator<_CharT2>,
      const _CharT2&);

      template<typename _CharT2, typename _Distance>
 friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
            void>::__type
 advance(istreambuf_iterator<_CharT2>&, _Distance);

    private:







      mutable streambuf_type* _M_sbuf;
      int_type _M_c;

    public:

      istreambuf_iterator() throw()
      : _M_sbuf(0), _M_c(traits_type::eof()) { }
# 135 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 3
      istreambuf_iterator(istream_type& __s) throw()
      : _M_sbuf(__s.rdbuf()), _M_c(traits_type::eof()) { }


      istreambuf_iterator(streambuf_type* __s) throw()
      : _M_sbuf(__s), _M_c(traits_type::eof()) { }
# 150 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 3
     
      char_type
      operator*() const
      {
 int_type __c = _M_get();
# 163 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 3
 return traits_type::to_char_type(__c);
      }


      istreambuf_iterator&
      operator++()
      {



                        ;

 _M_sbuf->sbumpc();
 _M_c = traits_type::eof();
 return *this;
      }


      istreambuf_iterator
      operator++(int)
      {



                        ;

 istreambuf_iterator __old = *this;
 __old._M_c = _M_sbuf->sbumpc();
 _M_c = traits_type::eof();
 return __old;
      }





     
      bool
      equal(const istreambuf_iterator& __b) const
      { return _M_at_eof() == __b._M_at_eof(); }

    private:
      int_type
      _M_get() const
      {
 int_type __ret = _M_c;
 if (_M_sbuf && _S_is_eof(__ret) && _S_is_eof(__ret = _M_sbuf->sgetc()))
   _M_sbuf = 0;
 return __ret;
      }

      bool
      _M_at_eof() const
      { return _S_is_eof(_M_get()); }

      static bool
      _S_is_eof(int_type __c)
      {
 const int_type __eof = traits_type::eof();
 return traits_type::eq_int_type(__c, __eof);
      }







    };

  template<typename _CharT, typename _Traits>
   
    inline bool
    operator==(const istreambuf_iterator<_CharT, _Traits>& __a,
        const istreambuf_iterator<_CharT, _Traits>& __b)
    { return __a.equal(__b); }


  template<typename _CharT, typename _Traits>
   
    inline bool
    operator!=(const istreambuf_iterator<_CharT, _Traits>& __a,
        const istreambuf_iterator<_CharT, _Traits>& __b)
    { return !__a.equal(__b); }



  template<typename _CharT, typename _Traits>
    class ostreambuf_iterator
    : public iterator<output_iterator_tag, void, void, void, void>
    {
    public:






      typedef _CharT char_type;
      typedef _Traits traits_type;
      typedef basic_streambuf<_CharT, _Traits> streambuf_type;
      typedef basic_ostream<_CharT, _Traits> ostream_type;


      template<typename _CharT2>
 friend typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value,
        ostreambuf_iterator<_CharT2> >::__type
 copy(istreambuf_iterator<_CharT2>, istreambuf_iterator<_CharT2>,
      ostreambuf_iterator<_CharT2>);

    private:
      streambuf_type* _M_sbuf;
      bool _M_failed;

    public:
# 286 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h" 3
      ostreambuf_iterator(ostream_type& __s) throw()
      : _M_sbuf(__s.rdbuf()), _M_failed(!_M_sbuf) { }


      ostreambuf_iterator(streambuf_type* __s) throw()
      : _M_sbuf(__s), _M_failed(!_M_sbuf) { }


      ostreambuf_iterator&
      operator=(_CharT __c)
      {
 if (!_M_failed &&
     _Traits::eq_int_type(_M_sbuf->sputc(__c), _Traits::eof()))
   _M_failed = true;
 return *this;
      }


     
      ostreambuf_iterator&
      operator*()
      { return *this; }


      ostreambuf_iterator&
      operator++(int)
      { return *this; }


      ostreambuf_iterator&
      operator++()
      { return *this; }


     
      bool
      failed() const throw()
      { return _M_failed; }

      ostreambuf_iterator&
      _M_put(const _CharT* __ws, streamsize __len)
      {
 if (__builtin_expect(!_M_failed, true)
     && __builtin_expect(this->_M_sbuf->sputn(__ws, __len) != __len,
    false))
   _M_failed = true;
 return *this;
      }
    };
#pragma GCC diagnostic pop


  template<typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        ostreambuf_iterator<_CharT> >::__type
    copy(istreambuf_iterator<_CharT> __first,
  istreambuf_iterator<_CharT> __last,
  ostreambuf_iterator<_CharT> __result)
    {
      if (__first._M_sbuf && !__last._M_sbuf && !__result._M_failed)
 {
   bool __ineof;
   __copy_streambufs_eof(__first._M_sbuf, __result._M_sbuf, __ineof);
   if (!__ineof)
     __result._M_failed = true;
 }
      return __result;
    }

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        ostreambuf_iterator<_CharT> >::__type
    __copy_move_a2(_CharT* __first, _CharT* __last,
     ostreambuf_iterator<_CharT> __result)
    {
      const streamsize __num = __last - __first;
      if (__num > 0)
 __result._M_put(__first, __num);
      return __result;
    }

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        ostreambuf_iterator<_CharT> >::__type
    __copy_move_a2(const _CharT* __first, const _CharT* __last,
     ostreambuf_iterator<_CharT> __result)
    {
      const streamsize __num = __last - __first;
      if (__num > 0)
 __result._M_put(__first, __num);
      return __result;
    }

  template<bool _IsMove, typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        _CharT*>::__type
    __copy_move_a2(istreambuf_iterator<_CharT> __first,
     istreambuf_iterator<_CharT> __last, _CharT* __result)
    {
      typedef istreambuf_iterator<_CharT> __is_iterator_type;
      typedef typename __is_iterator_type::traits_type traits_type;
      typedef typename __is_iterator_type::streambuf_type streambuf_type;
      typedef typename traits_type::int_type int_type;

      if (__first._M_sbuf && !__last._M_sbuf)
 {
   streambuf_type* __sb = __first._M_sbuf;
   int_type __c = __sb->sgetc();
   while (!traits_type::eq_int_type(__c, traits_type::eof()))
     {
       const streamsize __n = __sb->egptr() - __sb->gptr();
       if (__n > 1)
  {
    traits_type::copy(__result, __sb->gptr(), __n);
    __sb->__safe_gbump(__n);
    __result += __n;
    __c = __sb->underflow();
  }
       else
  {
    *__result++ = traits_type::to_char_type(__c);
    __c = __sb->snextc();
  }
     }
 }
      return __result;
    }

  template<typename _CharT, typename _Size>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        _CharT*>::__type
    __copy_n_a(istreambuf_iterator<_CharT> __it, _Size __n, _CharT* __result,
        bool __strict __attribute__((__unused__)))
    {
      if (__n == 0)
 return __result;

     

                            ;
      _CharT* __beg = __result;
      __result += __it._M_sbuf->sgetn(__beg, __n);
     

                            ;
      return __result;
    }

  template<typename _CharT>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
          istreambuf_iterator<_CharT> >::__type
    find(istreambuf_iterator<_CharT> __first,
  istreambuf_iterator<_CharT> __last, const _CharT& __val)
    {
      typedef istreambuf_iterator<_CharT> __is_iterator_type;
      typedef typename __is_iterator_type::traits_type traits_type;
      typedef typename __is_iterator_type::streambuf_type streambuf_type;
      typedef typename traits_type::int_type int_type;
      const int_type __eof = traits_type::eof();

      if (__first._M_sbuf && !__last._M_sbuf)
 {
   const int_type __ival = traits_type::to_int_type(__val);
   streambuf_type* __sb = __first._M_sbuf;
   int_type __c = __sb->sgetc();
   while (!traits_type::eq_int_type(__c, __eof)
   && !traits_type::eq_int_type(__c, __ival))
     {
       streamsize __n = __sb->egptr() - __sb->gptr();
       if (__n > 1)
  {
    const _CharT* __p = traits_type::find(__sb->gptr(),
       __n, __val);
    if (__p)
      __n = __p - __sb->gptr();
    __sb->__safe_gbump(__n);
    __c = __sb->sgetc();
  }
       else
  __c = __sb->snextc();
     }

   __first._M_c = __eof;
 }

      return __first;
    }

  template<typename _CharT, typename _Distance>
    typename __gnu_cxx::__enable_if<__is_char<_CharT>::__value,
        void>::__type
    advance(istreambuf_iterator<_CharT>& __i, _Distance __n)
    {
      if (__n == 0)
 return;

      do { if (__builtin_expect(!bool(__n > 0), false)) std::__glibcxx_assert_fail("/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/streambuf_iterator.h", 482, __PRETTY_FUNCTION__, "__n > 0"); } while (false);
     

                           ;

      typedef istreambuf_iterator<_CharT> __is_iterator_type;
      typedef typename __is_iterator_type::traits_type traits_type;
      typedef typename __is_iterator_type::streambuf_type streambuf_type;
      typedef typename traits_type::int_type int_type;
      const int_type __eof = traits_type::eof();

      streambuf_type* __sb = __i._M_sbuf;
      while (__n > 0)
 {
   streamsize __size = __sb->egptr() - __sb->gptr();
   if (__size > __n)
     {
       __sb->__safe_gbump(__n);
       break;
     }

   __sb->__safe_gbump(__size);
   __n -= __size;
   if (traits_type::eq_int_type(__sb->underflow(), __eof))
     {
      

                      ;
       break;
     }
 }

      __i._M_c = __eof;
    }




}
# 51 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 76 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _Tp>
    void
    __convert_to_v(const char*, _Tp&, ios_base::iostate&,
     const __c_locale&) throw();


  template<>
    void
    __convert_to_v(const char*, float&, ios_base::iostate&,
     const __c_locale&) throw();

  template<>
    void
    __convert_to_v(const char*, double&, ios_base::iostate&,
     const __c_locale&) throw();

  template<>
    void
    __convert_to_v(const char*, long double&, ios_base::iostate&,
     const __c_locale&) throw();



  template<typename _CharT, typename _Traits>
    struct __pad
    {
      static void
      _S_pad(ios_base& __io, _CharT __fill, _CharT* __news,
      const _CharT* __olds, streamsize __newlen, streamsize __oldlen);
    };






  template<typename _CharT>
    _CharT*
    __add_grouping(_CharT* __s, _CharT __sep,
     const char* __gbeg, size_t __gsize,
     const _CharT* __first, const _CharT* __last);




  template<typename _CharT>
    inline
    ostreambuf_iterator<_CharT>
    __write(ostreambuf_iterator<_CharT> __s, const _CharT* __ws, int __len)
    {
      __s._M_put(__ws, __len);
      return __s;
    }


  template<typename _CharT, typename _OutIter>
    inline
    _OutIter
    __write(_OutIter __s, const _CharT* __ws, int __len)
    {
      for (int __j = 0; __j < __len; __j++, ++__s)
 *__s = __ws[__j];
      return __s;
    }
# 154 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _CharT>
    class __ctype_abstract_base : public locale::facet, public ctype_base
    {
    public:


      typedef _CharT char_type;
# 173 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      bool
      is(mask __m, char_type __c) const
      { return this->do_is(__m, __c); }
# 190 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      is(const char_type *__lo, const char_type *__hi, mask *__vec) const
      { return this->do_is(__lo, __hi, __vec); }
# 206 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      scan_is(mask __m, const char_type* __lo, const char_type* __hi) const
      { return this->do_scan_is(__m, __lo, __hi); }
# 222 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      scan_not(mask __m, const char_type* __lo, const char_type* __hi) const
      { return this->do_scan_not(__m, __lo, __hi); }
# 236 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      toupper(char_type __c) const
      { return this->do_toupper(__c); }
# 251 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      toupper(char_type *__lo, const char_type* __hi) const
      { return this->do_toupper(__lo, __hi); }
# 265 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      tolower(char_type __c) const
      { return this->do_tolower(__c); }
# 280 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      tolower(char_type* __lo, const char_type* __hi) const
      { return this->do_tolower(__lo, __hi); }
# 297 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      widen(char __c) const
      { return this->do_widen(__c); }
# 316 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char*
      widen(const char* __lo, const char* __hi, char_type* __to) const
      { return this->do_widen(__lo, __hi, __to); }
# 335 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char
      narrow(char_type __c, char __dfault) const
      { return this->do_narrow(__c, __dfault); }
# 357 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      narrow(const char_type* __lo, const char_type* __hi,
       char __dfault, char* __to) const
      { return this->do_narrow(__lo, __hi, __dfault, __to); }

    protected:
      explicit
      __ctype_abstract_base(size_t __refs = 0): facet(__refs) { }

      virtual
      ~__ctype_abstract_base() { }
# 382 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual bool
      do_is(mask __m, char_type __c) const = 0;
# 401 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_is(const char_type* __lo, const char_type* __hi,
     mask* __vec) const = 0;
# 420 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_scan_is(mask __m, const char_type* __lo,
   const char_type* __hi) const = 0;
# 439 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_scan_not(mask __m, const char_type* __lo,
    const char_type* __hi) const = 0;
# 457 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_toupper(char_type __c) const = 0;
# 474 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_toupper(char_type* __lo, const char_type* __hi) const = 0;
# 490 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_tolower(char_type __c) const = 0;
# 507 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_tolower(char_type* __lo, const char_type* __hi) const = 0;
# 526 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_widen(char __c) const = 0;
# 547 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char*
      do_widen(const char* __lo, const char* __hi, char_type* __to) const = 0;
# 568 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char
      do_narrow(char_type __c, char __dfault) const = 0;
# 593 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_narrow(const char_type* __lo, const char_type* __hi,
  char __dfault, char* __to) const = 0;
    };
# 616 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _CharT>
    class ctype : public __ctype_abstract_base<_CharT>
    {
    public:

      typedef _CharT char_type;
      typedef typename __ctype_abstract_base<_CharT>::mask mask;


      static locale::id id;

      explicit
      ctype(size_t __refs = 0) : __ctype_abstract_base<_CharT>(__refs) { }

   protected:
      virtual
      ~ctype();

      virtual bool
      do_is(mask __m, char_type __c) const;

      virtual const char_type*
      do_is(const char_type* __lo, const char_type* __hi, mask* __vec) const;

      virtual const char_type*
      do_scan_is(mask __m, const char_type* __lo, const char_type* __hi) const;

      virtual const char_type*
      do_scan_not(mask __m, const char_type* __lo,
    const char_type* __hi) const;

      virtual char_type
      do_toupper(char_type __c) const;

      virtual const char_type*
      do_toupper(char_type* __lo, const char_type* __hi) const;

      virtual char_type
      do_tolower(char_type __c) const;

      virtual const char_type*
      do_tolower(char_type* __lo, const char_type* __hi) const;

      virtual char_type
      do_widen(char __c) const;

      virtual const char*
      do_widen(const char* __lo, const char* __hi, char_type* __dest) const;

      virtual char
      do_narrow(char_type, char __dfault) const;

      virtual const char_type*
      do_narrow(const char_type* __lo, const char_type* __hi,
  char __dfault, char* __to) const;
    };

  template<typename _CharT>
    locale::id ctype<_CharT>::id;



  template<typename _CharT, typename _Traits, typename _Alloc>
    class ctype<basic_string<_CharT, _Traits, _Alloc> >;
# 690 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<>
    class ctype<char> : public locale::facet, public ctype_base
    {
    public:


      typedef char char_type;

    protected:

      __c_locale _M_c_locale_ctype;
      bool _M_del;
      __to_type _M_toupper;
      __to_type _M_tolower;
      const mask* _M_table;
      mutable char _M_widen_ok;
      mutable char _M_widen[1 + static_cast<unsigned char>(-1)];
      mutable char _M_narrow[1 + static_cast<unsigned char>(-1)];
      mutable char _M_narrow_ok;


    public:

      static locale::id id;

      static const size_t table_size = 1 + static_cast<unsigned char>(-1);
# 727 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      ctype(const mask* __table = 0, bool __del = false, size_t __refs = 0);
# 740 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      ctype(__c_locale __cloc, const mask* __table = 0, bool __del = false,
     size_t __refs = 0);
# 753 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      inline bool
      is(mask __m, char __c) const;
# 768 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      inline const char*
      is(const char* __lo, const char* __hi, mask* __vec) const;
# 782 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      inline const char*
      scan_is(mask __m, const char* __lo, const char* __hi) const;
# 796 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      inline const char*
      scan_not(mask __m, const char* __lo, const char* __hi) const;
# 811 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      toupper(char_type __c) const
      { return this->do_toupper(__c); }
# 828 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      toupper(char_type *__lo, const char_type* __hi) const
      { return this->do_toupper(__lo, __hi); }
# 844 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      tolower(char_type __c) const
      { return this->do_tolower(__c); }
# 861 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      tolower(char_type* __lo, const char_type* __hi) const
      { return this->do_tolower(__lo, __hi); }
# 881 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      widen(char __c) const
      {
 if (_M_widen_ok)
   return _M_widen[static_cast<unsigned char>(__c)];
 this->_M_widen_init();
 return this->do_widen(__c);
      }
# 908 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char*
      widen(const char* __lo, const char* __hi, char_type* __to) const
      {
 if (_M_widen_ok == 1)
   {
     if (__builtin_expect(__hi != __lo, true))
       __builtin_memcpy(__to, __lo, __hi - __lo);
     return __hi;
   }
 if (!_M_widen_ok)
   _M_widen_init();
 return this->do_widen(__lo, __hi, __to);
      }
# 940 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char
      narrow(char_type __c, char __dfault) const
      {
 if (_M_narrow[static_cast<unsigned char>(__c)])
   return _M_narrow[static_cast<unsigned char>(__c)];
 const char __t = do_narrow(__c, __dfault);
 if (__t != __dfault)
   _M_narrow[static_cast<unsigned char>(__c)] = __t;
 return __t;
      }
# 973 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      const char_type*
      narrow(const char_type* __lo, const char_type* __hi,
      char __dfault, char* __to) const
      {
 if (__builtin_expect(_M_narrow_ok == 1, true))
   {
     if (__builtin_expect(__hi != __lo, true))
       __builtin_memcpy(__to, __lo, __hi - __lo);
     return __hi;
   }
 if (!_M_narrow_ok)
   _M_narrow_init();
 return this->do_narrow(__lo, __hi, __dfault, __to);
      }





      const mask*
      table() const throw()
      { return _M_table; }


      static const mask*
      classic_table() throw();
    protected:







      virtual
      ~ctype();
# 1023 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_toupper(char_type __c) const;
# 1040 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_toupper(char_type* __lo, const char_type* __hi) const;
# 1056 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_tolower(char_type __c) const;
# 1073 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_tolower(char_type* __lo, const char_type* __hi) const;
# 1093 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_widen(char __c) const
      { return __c; }
# 1116 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char*
      do_widen(const char* __lo, const char* __hi, char_type* __to) const
      {
 if (__builtin_expect(__hi != __lo, true))
   __builtin_memcpy(__to, __lo, __hi - __lo);
 return __hi;
      }
# 1143 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char
      do_narrow(char_type __c, char __dfault __attribute__((__unused__))) const
      { return __c; }
# 1169 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_narrow(const char_type* __lo, const char_type* __hi,
  char __dfault __attribute__((__unused__)), char* __to) const
      {
 if (__builtin_expect(__hi != __lo, true))
   __builtin_memcpy(__to, __lo, __hi - __lo);
 return __hi;
      }

    private:
      void _M_narrow_init() const;
      void _M_widen_init() const;
    };
# 1195 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<>
    class ctype<wchar_t> : public __ctype_abstract_base<wchar_t>
    {
    public:


      typedef wchar_t char_type;
      typedef wctype_t __wmask_type;

    protected:
      __c_locale _M_c_locale_ctype;


      bool _M_narrow_ok;
      char _M_narrow[128];
      wint_t _M_widen[1 + static_cast<unsigned char>(-1)];


      mask _M_bit[16];
      __wmask_type _M_wmask[16];

    public:


      static locale::id id;
# 1228 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      ctype(size_t __refs = 0);
# 1239 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      ctype(__c_locale __cloc, size_t __refs = 0);

    protected:
      __wmask_type
      _M_convert_to_wmask(const mask __m) const throw();


      virtual
      ~ctype();
# 1263 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual bool
      do_is(mask __m, char_type __c) const;
# 1282 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_is(const char_type* __lo, const char_type* __hi, mask* __vec) const;
# 1300 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_scan_is(mask __m, const char_type* __lo, const char_type* __hi) const;
# 1318 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_scan_not(mask __m, const char_type* __lo,
    const char_type* __hi) const;
# 1335 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_toupper(char_type __c) const;
# 1352 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_toupper(char_type* __lo, const char_type* __hi) const;
# 1368 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_tolower(char_type __c) const;
# 1385 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_tolower(char_type* __lo, const char_type* __hi) const;
# 1405 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_widen(char __c) const;
# 1427 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char*
      do_widen(const char* __lo, const char* __hi, char_type* __to) const;
# 1450 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char
      do_narrow(char_type __c, char __dfault) const;
# 1476 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual const char_type*
      do_narrow(const char_type* __lo, const char_type* __hi,
  char __dfault, char* __to) const;


      void
      _M_initialize_ctype() throw();
    };



  template<typename _CharT>
    class ctype_byname : public ctype<_CharT>
    {
    public:
      typedef typename ctype<_CharT>::mask mask;

      explicit
      ctype_byname(const char* __s, size_t __refs = 0);







    protected:
      virtual
      ~ctype_byname() { }
    };


  template<>
    class ctype_byname<char> : public ctype<char>
    {
    public:
      explicit
      ctype_byname(const char* __s, size_t __refs = 0);






    protected:
      virtual
      ~ctype_byname();
    };


  template<>
    class ctype_byname<wchar_t> : public ctype<wchar_t>
    {
    public:
      explicit
      ctype_byname(const char* __s, size_t __refs = 0);






    protected:
      virtual
      ~ctype_byname();
    };



}


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/ctype_inline.h" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/x86_64-pc-linux-gnu/bits/ctype_inline.h" 3
namespace std __attribute__ ((__visibility__ ("default")))
{


  bool
  ctype<char>::
  is(mask __m, char __c) const
  { return _M_table[static_cast<unsigned char>(__c)] & __m; }

  const char*
  ctype<char>::
  is(const char* __low, const char* __high, mask* __vec) const
  {
    while (__low < __high)
      *__vec++ = _M_table[static_cast<unsigned char>(*__low++)];
    return __high;
  }

  const char*
  ctype<char>::
  scan_is(mask __m, const char* __low, const char* __high) const
  {
    while (__low < __high
    && !(_M_table[static_cast<unsigned char>(*__low)] & __m))
      ++__low;
    return __low;
  }

  const char*
  ctype<char>::
  scan_not(mask __m, const char* __low, const char* __high) const
  {
    while (__low < __high
    && (_M_table[static_cast<unsigned char>(*__low)] & __m) != 0)
      ++__low;
    return __low;
  }


}
# 1549 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{



  class __num_base
  {
  public:


    enum
      {
 _S_ominus,
 _S_oplus,
 _S_ox,
 _S_oX,
 _S_odigits,
 _S_odigits_end = _S_odigits + 16,
 _S_oudigits = _S_odigits_end,
 _S_oudigits_end = _S_oudigits + 16,
 _S_oe = _S_odigits + 14,
 _S_oE = _S_oudigits + 14,
 _S_oend = _S_oudigits_end
      };






    static const char* _S_atoms_out;



    static const char* _S_atoms_in;

    enum
    {
      _S_iminus,
      _S_iplus,
      _S_ix,
      _S_iX,
      _S_izero,
      _S_ie = _S_izero + 14,
      _S_iE = _S_izero + 20,
      _S_iend = 26
    };



    static void
    _S_format_float(const ios_base& __io, char* __fptr, char __mod) throw();
  };

  template<typename _CharT>
    struct __numpunct_cache : public locale::facet
    {
      const char* _M_grouping;
      size_t _M_grouping_size;
      bool _M_use_grouping;
      const _CharT* _M_truename;
      size_t _M_truename_size;
      const _CharT* _M_falsename;
      size_t _M_falsename_size;
      _CharT _M_decimal_point;
      _CharT _M_thousands_sep;





      _CharT _M_atoms_out[__num_base::_S_oend];





      _CharT _M_atoms_in[__num_base::_S_iend];

      bool _M_allocated;

      __numpunct_cache(size_t __refs = 0)
      : facet(__refs), _M_grouping(0), _M_grouping_size(0),
 _M_use_grouping(false),
 _M_truename(0), _M_truename_size(0), _M_falsename(0),
 _M_falsename_size(0), _M_decimal_point(_CharT()),
 _M_thousands_sep(_CharT()), _M_allocated(false)
 { }

      ~__numpunct_cache();

      void
      _M_cache(const locale& __loc);

    private:
      __numpunct_cache&
      operator=(const __numpunct_cache&);

      explicit
      __numpunct_cache(const __numpunct_cache&);
    };

  template<typename _CharT>
    __numpunct_cache<_CharT>::~__numpunct_cache()
    {
      if (_M_allocated)
 {
   delete [] _M_grouping;
   delete [] _M_truename;
   delete [] _M_falsename;
 }
    }

namespace __cxx11 {
# 1679 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _CharT>
    class numpunct : public locale::facet
    {
    public:



      typedef _CharT char_type;
      typedef basic_string<_CharT> string_type;

      typedef __numpunct_cache<_CharT> __cache_type;

    protected:
      __cache_type* _M_data;

    public:

      static locale::id id;






      explicit
      numpunct(size_t __refs = 0)
      : facet(__refs), _M_data(0)
      { _M_initialize_numpunct(); }
# 1717 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      numpunct(__cache_type* __cache, size_t __refs = 0)
      : facet(__refs), _M_data(__cache)
      { _M_initialize_numpunct(); }
# 1731 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      numpunct(__c_locale __cloc, size_t __refs = 0)
      : facet(__refs), _M_data(0)
      { _M_initialize_numpunct(__cloc); }
# 1745 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      decimal_point() const
      { return this->do_decimal_point(); }
# 1758 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      char_type
      thousands_sep() const
      { return this->do_thousands_sep(); }
# 1789 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      string
      grouping() const
      { return this->do_grouping(); }
# 1802 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      string_type
      truename() const
      { return this->do_truename(); }
# 1815 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      string_type
      falsename() const
      { return this->do_falsename(); }

    protected:

      virtual
      ~numpunct();
# 1832 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_decimal_point() const
      { return _M_data->_M_decimal_point; }
# 1844 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual char_type
      do_thousands_sep() const
      { return _M_data->_M_thousands_sep; }
# 1857 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual string
      do_grouping() const
      { return _M_data->_M_grouping; }
# 1870 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual string_type
      do_truename() const
      { return _M_data->_M_truename; }
# 1883 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual string_type
      do_falsename() const
      { return _M_data->_M_falsename; }


      void
      _M_initialize_numpunct(__c_locale __cloc = 0);
    };

  template<typename _CharT>
    locale::id numpunct<_CharT>::id;

  template<>
    numpunct<char>::~numpunct();

  template<>
    void
    numpunct<char>::_M_initialize_numpunct(__c_locale __cloc);


  template<>
    numpunct<wchar_t>::~numpunct();

  template<>
    void
    numpunct<wchar_t>::_M_initialize_numpunct(__c_locale __cloc);



  template<typename _CharT>
    class numpunct_byname : public numpunct<_CharT>
    {
    public:
      typedef _CharT char_type;
      typedef basic_string<_CharT> string_type;

      explicit
      numpunct_byname(const char* __s, size_t __refs = 0)
      : numpunct<_CharT>(__refs)
      {
 if (__builtin_strcmp(__s, "C") != 0
     && __builtin_strcmp(__s, "POSIX") != 0)
   {
     __c_locale __tmp;
     this->_S_create_c_locale(__tmp, __s);
     this->_M_initialize_numpunct(__tmp);
     this->_S_destroy_c_locale(__tmp);
   }
      }







    protected:
      virtual
      ~numpunct_byname() { }
    };

}


# 1961 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _CharT, typename _InIter>
    class num_get : public locale::facet
    {
    public:



      typedef _CharT char_type;
      typedef _InIter iter_type;



      static locale::id id;
# 1982 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      num_get(size_t __refs = 0) : facet(__refs) { }
# 2008 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, bool& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }
# 2045 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, long& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, unsigned short& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, unsigned int& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, unsigned long& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, long long& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, unsigned long long& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }
#pragma GCC diagnostic pop
# 2108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, float& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, double& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, long double& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }
# 2151 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      get(iter_type __in, iter_type __end, ios_base& __io,
   ios_base::iostate& __err, void*& __v) const
      { return this->do_get(__in, __end, __io, __err, __v); }

    protected:

      virtual ~num_get() { }

      __attribute ((__abi_tag__ ("cxx11")))
      iter_type
      _M_extract_float(iter_type, iter_type, ios_base&, ios_base::iostate&,
         string&) const;

      template<typename _ValueT>
 __attribute ((__abi_tag__ ("cxx11")))
 iter_type
 _M_extract_int(iter_type, iter_type, ios_base&, ios_base::iostate&,
         _ValueT&) const;

      template<typename _CharT2>
      typename __gnu_cxx::__enable_if<__is_char<_CharT2>::__value, int>::__type
 _M_find(const _CharT2*, size_t __len, _CharT2 __c) const
 {
   int __ret = -1;
   if (__len <= 10)
     {
       if (__c >= _CharT2('0') && __c < _CharT2(_CharT2('0') + __len))
  __ret = __c - _CharT2('0');
     }
   else
     {
       if (__c >= _CharT2('0') && __c <= _CharT2('9'))
  __ret = __c - _CharT2('0');
       else if (__c >= _CharT2('a') && __c <= _CharT2('f'))
  __ret = 10 + (__c - _CharT2('a'));
       else if (__c >= _CharT2('A') && __c <= _CharT2('F'))
  __ret = 10 + (__c - _CharT2('A'));
     }
   return __ret;
 }

      template<typename _CharT2>
      typename __gnu_cxx::__enable_if<!__is_char<_CharT2>::__value,
          int>::__type
 _M_find(const _CharT2* __zero, size_t __len, _CharT2 __c) const
 {
   int __ret = -1;
   const char_type* __q = char_traits<_CharT2>::find(__zero, __len, __c);
   if (__q)
     {
       __ret = __q - __zero;
       if (__ret > 15)
  __ret -= 6;
     }
   return __ret;
 }
# 2224 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual iter_type
      do_get(iter_type, iter_type, ios_base&, ios_base::iostate&, bool&) const;

      virtual iter_type
      do_get(iter_type __beg, iter_type __end, ios_base& __io,
      ios_base::iostate& __err, long& __v) const
      { return _M_extract_int(__beg, __end, __io, __err, __v); }

      virtual iter_type
      do_get(iter_type __beg, iter_type __end, ios_base& __io,
      ios_base::iostate& __err, unsigned short& __v) const
      { return _M_extract_int(__beg, __end, __io, __err, __v); }

      virtual iter_type
      do_get(iter_type __beg, iter_type __end, ios_base& __io,
      ios_base::iostate& __err, unsigned int& __v) const
      { return _M_extract_int(__beg, __end, __io, __err, __v); }

      virtual iter_type
      do_get(iter_type __beg, iter_type __end, ios_base& __io,
      ios_base::iostate& __err, unsigned long& __v) const
      { return _M_extract_int(__beg, __end, __io, __err, __v); }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      virtual iter_type
      do_get(iter_type __beg, iter_type __end, ios_base& __io,
      ios_base::iostate& __err, long long& __v) const
      { return _M_extract_int(__beg, __end, __io, __err, __v); }

      virtual iter_type
      do_get(iter_type __beg, iter_type __end, ios_base& __io,
      ios_base::iostate& __err, unsigned long long& __v) const
      { return _M_extract_int(__beg, __end, __io, __err, __v); }
#pragma GCC diagnostic pop


      virtual iter_type
      do_get(iter_type, iter_type, ios_base&, ios_base::iostate&, float&) const;

      virtual iter_type
      do_get(iter_type, iter_type, ios_base&, ios_base::iostate&,
      double&) const;
# 2279 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual iter_type
      do_get(iter_type, iter_type, ios_base&, ios_base::iostate&,
      long double&) const;


      virtual iter_type
      do_get(iter_type, iter_type, ios_base&, ios_base::iostate&, void*&) const;
# 2307 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
    };

  template<typename _CharT, typename _InIter>
    locale::id num_get<_CharT, _InIter>::id;
# 2325 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _CharT, typename _OutIter>
    class num_put : public locale::facet
    {
    public:



      typedef _CharT char_type;
      typedef _OutIter iter_type;



      static locale::id id;
# 2346 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      explicit
      num_put(size_t __refs = 0) : facet(__refs) { }
# 2364 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill, bool __v) const
      { return this->do_put(__s, __io, __fill, __v); }
# 2406 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill, long __v) const
      { return this->do_put(__s, __io, __fill, __v); }

      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill,
   unsigned long __v) const
      { return this->do_put(__s, __io, __fill, __v); }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill, long long __v) const
      { return this->do_put(__s, __io, __fill, __v); }

      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill,
   unsigned long long __v) const
      { return this->do_put(__s, __io, __fill, __v); }
#pragma GCC diagnostic pop
# 2472 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill, double __v) const
      { return this->do_put(__s, __io, __fill, __v); }

      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill,
   long double __v) const
      { return this->do_put(__s, __io, __fill, __v); }
# 2497 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      iter_type
      put(iter_type __s, ios_base& __io, char_type __fill,
   const void* __v) const
      { return this->do_put(__s, __io, __fill, __v); }

    protected:
      template<typename _ValueT>
 iter_type
 _M_insert_float(iter_type, ios_base& __io, char_type __fill,
   char __mod, _ValueT __v) const;

      void
      _M_group_float(const char* __grouping, size_t __grouping_size,
       char_type __sep, const char_type* __p, char_type* __new,
       char_type* __cs, int& __len) const;

      template<typename _ValueT>
 iter_type
 _M_insert_int(iter_type, ios_base& __io, char_type __fill,
        _ValueT __v) const;

      void
      _M_group_int(const char* __grouping, size_t __grouping_size,
     char_type __sep, ios_base& __io, char_type* __new,
     char_type* __cs, int& __len) const;

      void
      _M_pad(char_type __fill, streamsize __w, ios_base& __io,
      char_type* __new, const char_type* __cs, int& __len) const;


      virtual
      ~num_put() { }
# 2545 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
      virtual iter_type
      do_put(iter_type __s, ios_base& __io, char_type __fill, bool __v) const;

      virtual iter_type
      do_put(iter_type __s, ios_base& __io, char_type __fill, long __v) const
      { return _M_insert_int(__s, __io, __fill, __v); }

      virtual iter_type
      do_put(iter_type __s, ios_base& __io, char_type __fill,
      unsigned long __v) const
      { return _M_insert_int(__s, __io, __fill, __v); }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      virtual iter_type
      do_put(iter_type __s, ios_base& __io, char_type __fill,
      long long __v) const
      { return _M_insert_int(__s, __io, __fill, __v); }

      virtual iter_type
      do_put(iter_type __s, ios_base& __io, char_type __fill,
      unsigned long long __v) const
      { return _M_insert_int(__s, __io, __fill, __v); }
#pragma GCC diagnostic pop


      virtual iter_type
      do_put(iter_type, ios_base&, char_type, double) const;






      virtual iter_type
      do_put(iter_type, ios_base&, char_type, long double) const;


      virtual iter_type
      do_put(iter_type, ios_base&, char_type, const void*) const;
# 2600 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
    };

  template <typename _CharT, typename _OutIter>
    locale::id num_put<_CharT, _OutIter>::id;









  template<typename _CharT>
    inline bool
    isspace(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::space, __c); }


  template<typename _CharT>
    inline bool
    isprint(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::print, __c); }


  template<typename _CharT>
    inline bool
    iscntrl(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::cntrl, __c); }


  template<typename _CharT>
    inline bool
    isupper(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::upper, __c); }


  template<typename _CharT>
    inline bool
    islower(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::lower, __c); }


  template<typename _CharT>
    inline bool
    isalpha(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::alpha, __c); }


  template<typename _CharT>
    inline bool
    isdigit(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::digit, __c); }


  template<typename _CharT>
    inline bool
    ispunct(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::punct, __c); }


  template<typename _CharT>
    inline bool
    isxdigit(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::xdigit, __c); }


  template<typename _CharT>
    inline bool
    isalnum(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::alnum, __c); }


  template<typename _CharT>
    inline bool
    isgraph(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).is(ctype_base::graph, __c); }
# 2687 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 3
  template<typename _CharT>
    inline _CharT
    toupper(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).toupper(__c); }


  template<typename _CharT>
    inline _CharT
    tolower(_CharT __c, const locale& __loc)
    { return use_facet<ctype<_CharT> >(__loc).tolower(__c); }


}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{




  template<typename _Facet>
    struct __use_cache
    {
      const _Facet*
      operator() (const locale& __loc) const;
    };


  template<typename _CharT>
    struct __use_cache<__numpunct_cache<_CharT> >
    {
      const __numpunct_cache<_CharT>*
      operator() (const locale& __loc) const
      {
 const size_t __i = numpunct<_CharT>::id._M_id();
 const locale::facet** __caches = __loc._M_impl->_M_caches;
 if (!__caches[__i])
   {
     __numpunct_cache<_CharT>* __tmp = 0;
     try
       {
  __tmp = new __numpunct_cache<_CharT>;
  __tmp->_M_cache(__loc);
       }
     catch(...)
       {
  delete __tmp;
  throw;
       }
     __loc._M_impl->_M_install_cache(__tmp, __i);
   }
 return static_cast<const __numpunct_cache<_CharT>*>(__caches[__i]);
      }
    };

  template<typename _CharT>
    void
    __numpunct_cache<_CharT>::_M_cache(const locale& __loc)
    {
      const numpunct<_CharT>& __np = use_facet<numpunct<_CharT> >(__loc);

      char* __grouping = 0;
      _CharT* __truename = 0;
      _CharT* __falsename = 0;
      try
 {
   const string& __g = __np.grouping();
   _M_grouping_size = __g.size();
   __grouping = new char[_M_grouping_size];
   __g.copy(__grouping, _M_grouping_size);
   _M_use_grouping = (_M_grouping_size
        && static_cast<signed char>(__grouping[0]) > 0
        && (__grouping[0]
     != __gnu_cxx::__numeric_traits<char>::__max));

   const basic_string<_CharT>& __tn = __np.truename();
   _M_truename_size = __tn.size();
   __truename = new _CharT[_M_truename_size];
   __tn.copy(__truename, _M_truename_size);

   const basic_string<_CharT>& __fn = __np.falsename();
   _M_falsename_size = __fn.size();
   __falsename = new _CharT[_M_falsename_size];
   __fn.copy(__falsename, _M_falsename_size);

   _M_decimal_point = __np.decimal_point();
   _M_thousands_sep = __np.thousands_sep();

   const ctype<_CharT>& __ct = use_facet<ctype<_CharT> >(__loc);
   __ct.widen(__num_base::_S_atoms_out,
       __num_base::_S_atoms_out
       + __num_base::_S_oend, _M_atoms_out);
   __ct.widen(__num_base::_S_atoms_in,
       __num_base::_S_atoms_in
       + __num_base::_S_iend, _M_atoms_in);

   _M_grouping = __grouping;
   _M_truename = __truename;
   _M_falsename = __falsename;
   _M_allocated = true;
 }
      catch(...)
 {
   delete [] __grouping;
   delete [] __truename;
   delete [] __falsename;
   throw;
 }
    }
# 143 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
  __attribute__ ((__pure__)) bool
  __verify_grouping(const char* __grouping, size_t __grouping_size,
      const string& __grouping_tmp) throw ();



  template<typename _CharT, typename _InIter>
    __attribute ((__abi_tag__ ("cxx11")))
    _InIter
    num_get<_CharT, _InIter>::
    _M_extract_float(_InIter __beg, _InIter __end, ios_base& __io,
       ios_base::iostate& __err, string& __xtrc) const
    {
      typedef char_traits<_CharT> __traits_type;
      typedef __numpunct_cache<_CharT> __cache_type;
      __use_cache<__cache_type> __uc;
      const locale& __loc = __io._M_getloc();
      const __cache_type* __lc = __uc(__loc);
      const _CharT* __lit = __lc->_M_atoms_in;
      char_type __c = char_type();


      bool __testeof = __beg == __end;


      if (!__testeof)
 {
   __c = *__beg;
   const bool __plus = __c == __lit[__num_base::_S_iplus];
   if ((__plus || __c == __lit[__num_base::_S_iminus])
       && !(__lc->_M_use_grouping && __c == __lc->_M_thousands_sep)
       && !(__c == __lc->_M_decimal_point))
     {
       __xtrc += __plus ? '+' : '-';
       if (++__beg != __end)
  __c = *__beg;
       else
  __testeof = true;
     }
 }


      bool __found_mantissa = false;
      int __sep_pos = 0;
      while (!__testeof)
 {
   if ((__lc->_M_use_grouping && __c == __lc->_M_thousands_sep)
       || __c == __lc->_M_decimal_point)
     break;
   else if (__c == __lit[__num_base::_S_izero])
     {
       if (!__found_mantissa)
  {
    __xtrc += '0';
    __found_mantissa = true;
  }
       ++__sep_pos;

       if (++__beg != __end)
  __c = *__beg;
       else
  __testeof = true;
     }
   else
     break;
 }


      bool __found_dec = false;
      bool __found_sci = false;
      string __found_grouping;
      if (__lc->_M_use_grouping)
 __found_grouping.reserve(32);
      const char_type* __lit_zero = __lit + __num_base::_S_izero;

      if (!__lc->_M_allocated)

 while (!__testeof)
   {
     const int __digit = _M_find(__lit_zero, 10, __c);
     if (__digit != -1)
       {
  __xtrc += '0' + __digit;
  __found_mantissa = true;
       }
     else if (__c == __lc->_M_decimal_point
       && !__found_dec && !__found_sci)
       {
  __xtrc += '.';
  __found_dec = true;
       }
     else if ((__c == __lit[__num_base::_S_ie]
        || __c == __lit[__num_base::_S_iE])
       && !__found_sci && __found_mantissa)
       {

  __xtrc += 'e';
  __found_sci = true;


  if (++__beg != __end)
    {
      __c = *__beg;
      const bool __plus = __c == __lit[__num_base::_S_iplus];
      if (__plus || __c == __lit[__num_base::_S_iminus])
        __xtrc += __plus ? '+' : '-';
      else
        continue;
    }
  else
    {
      __testeof = true;
      break;
    }
       }
     else
       break;

     if (++__beg != __end)
       __c = *__beg;
     else
       __testeof = true;
   }
      else
 while (!__testeof)
   {


     if (__lc->_M_use_grouping && __c == __lc->_M_thousands_sep)
       {
  if (!__found_dec && !__found_sci)
    {


      if (__sep_pos)
        {
   __found_grouping += static_cast<char>(__sep_pos);
   __sep_pos = 0;
        }
      else
        {


   __xtrc.clear();
   break;
        }
    }
  else
    break;
       }
     else if (__c == __lc->_M_decimal_point)
       {
  if (!__found_dec && !__found_sci)
    {



      if (__found_grouping.size())
        __found_grouping += static_cast<char>(__sep_pos);
      __xtrc += '.';
      __found_dec = true;
    }
  else
    break;
       }
     else
       {
  const char_type* __q =
    __traits_type::find(__lit_zero, 10, __c);
  if (__q)
    {
      __xtrc += '0' + (__q - __lit_zero);
      __found_mantissa = true;
      ++__sep_pos;
    }
  else if ((__c == __lit[__num_base::_S_ie]
     || __c == __lit[__num_base::_S_iE])
    && !__found_sci && __found_mantissa)
    {

      if (__found_grouping.size() && !__found_dec)
        __found_grouping += static_cast<char>(__sep_pos);
      __xtrc += 'e';
      __found_sci = true;


      if (++__beg != __end)
        {
   __c = *__beg;
   const bool __plus = __c == __lit[__num_base::_S_iplus];
   if ((__plus || __c == __lit[__num_base::_S_iminus])
       && !(__lc->_M_use_grouping
     && __c == __lc->_M_thousands_sep)
       && !(__c == __lc->_M_decimal_point))
        __xtrc += __plus ? '+' : '-';
   else
     continue;
        }
      else
        {
   __testeof = true;
   break;
        }
    }
  else
    break;
       }

     if (++__beg != __end)
       __c = *__beg;
     else
       __testeof = true;
   }



      if (__found_grouping.size())
        {

   if (!__found_dec && !__found_sci)
     __found_grouping += static_cast<char>(__sep_pos);

          if (!std::__verify_grouping(__lc->_M_grouping,
          __lc->_M_grouping_size,
          __found_grouping))
     __err = ios_base::failbit;
        }

      return __beg;
    }

  template<typename _CharT, typename _InIter>
    template<typename _ValueT>
      __attribute ((__abi_tag__ ("cxx11")))
      _InIter
      num_get<_CharT, _InIter>::
      _M_extract_int(_InIter __beg, _InIter __end, ios_base& __io,
       ios_base::iostate& __err, _ValueT& __v) const
      {
        typedef char_traits<_CharT> __traits_type;
 using __gnu_cxx::__add_unsigned;
 typedef typename __add_unsigned<_ValueT>::__type __unsigned_type;
 typedef __numpunct_cache<_CharT> __cache_type;
 __use_cache<__cache_type> __uc;
 const locale& __loc = __io._M_getloc();
 const __cache_type* __lc = __uc(__loc);
 const _CharT* __lit = __lc->_M_atoms_in;
 char_type __c = char_type();


 const ios_base::fmtflags __basefield = __io.flags()
                                        & ios_base::basefield;
 const bool __oct = __basefield == ios_base::oct;
 int __base = __oct ? 8 : (__basefield == ios_base::hex ? 16 : 10);


 bool __testeof = __beg == __end;


 bool __negative = false;
 if (!__testeof)
   {
     __c = *__beg;
     __negative = __c == __lit[__num_base::_S_iminus];
     if ((__negative || __c == __lit[__num_base::_S_iplus])
  && !(__lc->_M_use_grouping && __c == __lc->_M_thousands_sep)
  && !(__c == __lc->_M_decimal_point))
       {
  if (++__beg != __end)
    __c = *__beg;
  else
    __testeof = true;
       }
   }



 bool __found_zero = false;
 int __sep_pos = 0;
 while (!__testeof)
   {
     if ((__lc->_M_use_grouping && __c == __lc->_M_thousands_sep)
  || __c == __lc->_M_decimal_point)
       break;
     else if (__c == __lit[__num_base::_S_izero]
       && (!__found_zero || __base == 10))
       {
  __found_zero = true;
  ++__sep_pos;
  if (__basefield == 0)
    __base = 8;
  if (__base == 8)
    __sep_pos = 0;
       }
     else if (__found_zero
       && (__c == __lit[__num_base::_S_ix]
    || __c == __lit[__num_base::_S_iX]))
       {
  if (__basefield == 0)
    __base = 16;
  if (__base == 16)
    {
      __found_zero = false;
      __sep_pos = 0;
    }
  else
    break;
       }
     else
       break;

     if (++__beg != __end)
       {
  __c = *__beg;
  if (!__found_zero)
    break;
       }
     else
       __testeof = true;
   }



 const size_t __len = (__base == 16 ? __num_base::_S_iend
         - __num_base::_S_izero : __base);


 typedef __gnu_cxx::__numeric_traits<_ValueT> __num_traits;
 string __found_grouping;
 if (__lc->_M_use_grouping)
   __found_grouping.reserve(32);
 bool __testfail = false;
 bool __testoverflow = false;
 const __unsigned_type __max =
   (__negative && __num_traits::__is_signed)
   ? -static_cast<__unsigned_type>(__num_traits::__min)
   : __num_traits::__max;
 const __unsigned_type __smax = __max / __base;
 __unsigned_type __result = 0;
 int __digit = 0;
 const char_type* __lit_zero = __lit + __num_base::_S_izero;

 if (!__lc->_M_allocated)

   while (!__testeof)
     {
       __digit = _M_find(__lit_zero, __len, __c);
       if (__digit == -1)
  break;

       if (__result > __smax)
  __testoverflow = true;
       else
  {
    __result *= __base;
    __testoverflow |= __result > __max - __digit;
    __result += __digit;
    ++__sep_pos;
  }

       if (++__beg != __end)
  __c = *__beg;
       else
  __testeof = true;
     }
 else
   while (!__testeof)
     {


       if (__lc->_M_use_grouping && __c == __lc->_M_thousands_sep)
  {


    if (__sep_pos)
      {
        __found_grouping += static_cast<char>(__sep_pos);
        __sep_pos = 0;
      }
    else
      {
        __testfail = true;
        break;
      }
  }
       else if (__c == __lc->_M_decimal_point)
  break;
       else
  {
    const char_type* __q =
      __traits_type::find(__lit_zero, __len, __c);
    if (!__q)
      break;

    __digit = __q - __lit_zero;
    if (__digit > 15)
      __digit -= 6;
    if (__result > __smax)
      __testoverflow = true;
    else
      {
        __result *= __base;
        __testoverflow |= __result > __max - __digit;
        __result += __digit;
        ++__sep_pos;
      }
  }

       if (++__beg != __end)
  __c = *__beg;
       else
  __testeof = true;
     }



 if (__found_grouping.size())
   {

     __found_grouping += static_cast<char>(__sep_pos);

     if (!std::__verify_grouping(__lc->_M_grouping,
     __lc->_M_grouping_size,
     __found_grouping))
       __err = ios_base::failbit;
   }



 if ((!__sep_pos && !__found_zero && !__found_grouping.size())
     || __testfail)
   {
     __v = 0;
     __err = ios_base::failbit;
   }
 else if (__testoverflow)
   {
     if (__negative && __num_traits::__is_signed)
       __v = __num_traits::__min;
     else
       __v = __num_traits::__max;
     __err = ios_base::failbit;
   }
 else
   __v = __negative ? -__result : __result;

 if (__testeof)
   __err |= ios_base::eofbit;
 return __beg;
      }



  template<typename _CharT, typename _InIter>
    _InIter
    num_get<_CharT, _InIter>::
    do_get(iter_type __beg, iter_type __end, ios_base& __io,
           ios_base::iostate& __err, bool& __v) const
    {
      if (!(__io.flags() & ios_base::boolalpha))
        {



   long __l = -1;
          __beg = _M_extract_int(__beg, __end, __io, __err, __l);
   if (__l == 0 || __l == 1)
     __v = bool(__l);
   else
     {


       __v = true;
       __err = ios_base::failbit;
       if (__beg == __end)
  __err |= ios_base::eofbit;
     }
        }
      else
        {

   typedef __numpunct_cache<_CharT> __cache_type;
   __use_cache<__cache_type> __uc;
   const locale& __loc = __io._M_getloc();
   const __cache_type* __lc = __uc(__loc);

   bool __testf = true;
   bool __testt = true;
   bool __donef = __lc->_M_falsename_size == 0;
   bool __donet = __lc->_M_truename_size == 0;
   bool __testeof = false;
   size_t __n = 0;
   while (!__donef || !__donet)
     {
       if (__beg == __end)
  {
    __testeof = true;
    break;
  }

       const char_type __c = *__beg;

       if (!__donef)
  __testf = __c == __lc->_M_falsename[__n];

       if (!__testf && __donet)
  break;

       if (!__donet)
  __testt = __c == __lc->_M_truename[__n];

       if (!__testt && __donef)
  break;

       if (!__testt && !__testf)
  break;

       ++__n;
       ++__beg;

       __donef = !__testf || __n >= __lc->_M_falsename_size;
       __donet = !__testt || __n >= __lc->_M_truename_size;
     }
   if (__testf && __n == __lc->_M_falsename_size && __n)
     {
       __v = false;
       if (__testt && __n == __lc->_M_truename_size)
  __err = ios_base::failbit;
       else
  __err = __testeof ? ios_base::eofbit : ios_base::goodbit;
     }
   else if (__testt && __n == __lc->_M_truename_size && __n)
     {
       __v = true;
       __err = __testeof ? ios_base::eofbit : ios_base::goodbit;
     }
   else
     {


       __v = false;
       __err = ios_base::failbit;
       if (__testeof)
  __err |= ios_base::eofbit;
     }
 }
      return __beg;
    }

  template<typename _CharT, typename _InIter>
    _InIter
    num_get<_CharT, _InIter>::
    do_get(iter_type __beg, iter_type __end, ios_base& __io,
    ios_base::iostate& __err, float& __v) const
    {
      string __xtrc;
      __xtrc.reserve(32);
      __beg = _M_extract_float(__beg, __end, __io, __err, __xtrc);
      std::__convert_to_v(__xtrc.c_str(), __v, __err, _S_get_c_locale());
      if (__beg == __end)
 __err |= ios_base::eofbit;
      return __beg;
    }

  template<typename _CharT, typename _InIter>
    _InIter
    num_get<_CharT, _InIter>::
    do_get(iter_type __beg, iter_type __end, ios_base& __io,
           ios_base::iostate& __err, double& __v) const
    {
      string __xtrc;
      __xtrc.reserve(32);
      __beg = _M_extract_float(__beg, __end, __io, __err, __xtrc);
      std::__convert_to_v(__xtrc.c_str(), __v, __err, _S_get_c_locale());
      if (__beg == __end)
 __err |= ios_base::eofbit;
      return __beg;
    }
# 739 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
  template<typename _CharT, typename _InIter>
    _InIter
    num_get<_CharT, _InIter>::
    do_get(iter_type __beg, iter_type __end, ios_base& __io,
           ios_base::iostate& __err, long double& __v) const
    {
      string __xtrc;
      __xtrc.reserve(32);
      __beg = _M_extract_float(__beg, __end, __io, __err, __xtrc);
      std::__convert_to_v(__xtrc.c_str(), __v, __err, _S_get_c_locale());
      if (__beg == __end)
 __err |= ios_base::eofbit;
      return __beg;
    }

  template<typename _CharT, typename _InIter>
    _InIter
    num_get<_CharT, _InIter>::
    do_get(iter_type __beg, iter_type __end, ios_base& __io,
           ios_base::iostate& __err, void*& __v) const
    {

      typedef ios_base::fmtflags fmtflags;
      const fmtflags __fmt = __io.flags();
      __io.flags((__fmt & ~ios_base::basefield) | ios_base::hex);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      typedef __gnu_cxx::__conditional_type<(sizeof(void*)
          <= sizeof(unsigned long)),
 unsigned long, unsigned long long>::__type _UIntPtrType;
#pragma GCC diagnostic pop

      _UIntPtrType __ul;
      __beg = _M_extract_int(__beg, __end, __io, __err, __ul);


      __io.flags(__fmt);

      __v = reinterpret_cast<void*>(__ul);
      return __beg;
    }
# 802 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
  template<typename _CharT, typename _OutIter>
    void
    num_put<_CharT, _OutIter>::
    _M_pad(_CharT __fill, streamsize __w, ios_base& __io,
    _CharT* __new, const _CharT* __cs, int& __len) const
    {


      __pad<_CharT, char_traits<_CharT> >::_S_pad(__io, __fill, __new,
        __cs, __w, __len);
      __len = static_cast<int>(__w);
    }



  template<typename _CharT, typename _ValueT>
    int
    __int_to_char(_CharT* __bufend, _ValueT __v, const _CharT* __lit,
    ios_base::fmtflags __flags, bool __dec)
    {
      _CharT* __buf = __bufend;
      if (__builtin_expect(__dec, true))
 {

   do
     {
       *--__buf = __lit[(__v % 10) + __num_base::_S_odigits];
       __v /= 10;
     }
   while (__v != 0);
 }
      else if ((__flags & ios_base::basefield) == ios_base::oct)
 {

   do
     {
       *--__buf = __lit[(__v & 0x7) + __num_base::_S_odigits];
       __v >>= 3;
     }
   while (__v != 0);
 }
      else
 {

   const bool __uppercase = __flags & ios_base::uppercase;
   const int __case_offset = __uppercase ? __num_base::_S_oudigits
                                         : __num_base::_S_odigits;
   do
     {
       *--__buf = __lit[(__v & 0xf) + __case_offset];
       __v >>= 4;
     }
   while (__v != 0);
 }
      return __bufend - __buf;
    }



  template<typename _CharT, typename _OutIter>
    void
    num_put<_CharT, _OutIter>::
    _M_group_int(const char* __grouping, size_t __grouping_size, _CharT __sep,
   ios_base&, _CharT* __new, _CharT* __cs, int& __len) const
    {
      _CharT* __p = std::__add_grouping(__new, __sep, __grouping,
     __grouping_size, __cs, __cs + __len);
      __len = __p - __new;
    }

  template<typename _CharT, typename _OutIter>
    template<typename _ValueT>
      _OutIter
      num_put<_CharT, _OutIter>::
      _M_insert_int(_OutIter __s, ios_base& __io, _CharT __fill,
      _ValueT __v) const
      {
 using __gnu_cxx::__add_unsigned;
 typedef typename __add_unsigned<_ValueT>::__type __unsigned_type;
 typedef __numpunct_cache<_CharT> __cache_type;
 __use_cache<__cache_type> __uc;
 const locale& __loc = __io._M_getloc();
 const __cache_type* __lc = __uc(__loc);
 const _CharT* __lit = __lc->_M_atoms_out;
 const ios_base::fmtflags __flags = __io.flags();


 const int __ilen = 5 * sizeof(_ValueT);
 _CharT* __cs = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
            * __ilen));



 const ios_base::fmtflags __basefield = __flags & ios_base::basefield;
 const bool __dec = (__basefield != ios_base::oct
       && __basefield != ios_base::hex);
 const __unsigned_type __u = ((__v > 0 || !__dec)
         ? __unsigned_type(__v)
         : -__unsigned_type(__v));
  int __len = __int_to_char(__cs + __ilen, __u, __lit, __flags, __dec);
 __cs += __ilen - __len;


 if (__lc->_M_use_grouping)
   {


     _CharT* __cs2 = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
          * (__len + 1)
          * 2));
     _M_group_int(__lc->_M_grouping, __lc->_M_grouping_size,
    __lc->_M_thousands_sep, __io, __cs2 + 2, __cs, __len);
     __cs = __cs2 + 2;
   }


 if (__builtin_expect(__dec, true))
   {

     if (__v >= 0)
       {
  if (bool(__flags & ios_base::showpos)
      && __gnu_cxx::__numeric_traits<_ValueT>::__is_signed)
    *--__cs = __lit[__num_base::_S_oplus], ++__len;
       }
     else
       *--__cs = __lit[__num_base::_S_ominus], ++__len;
   }
 else if (bool(__flags & ios_base::showbase) && __v)
   {
     if (__basefield == ios_base::oct)
       *--__cs = __lit[__num_base::_S_odigits], ++__len;
     else
       {

  const bool __uppercase = __flags & ios_base::uppercase;
  *--__cs = __lit[__num_base::_S_ox + __uppercase];

  *--__cs = __lit[__num_base::_S_odigits];
  __len += 2;
       }
   }


 const streamsize __w = __io.width();
 if (__w > static_cast<streamsize>(__len))
   {
     _CharT* __cs3 = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
          * __w));
     _M_pad(__fill, __w, __io, __cs3, __cs, __len);
     __cs = __cs3;
   }
 __io.width(0);



 return std::__write(__s, __cs, __len);
      }

  template<typename _CharT, typename _OutIter>
    void
    num_put<_CharT, _OutIter>::
    _M_group_float(const char* __grouping, size_t __grouping_size,
     _CharT __sep, const _CharT* __p, _CharT* __new,
     _CharT* __cs, int& __len) const
    {



      const int __declen = __p ? __p - __cs : __len;
      _CharT* __p2 = std::__add_grouping(__new, __sep, __grouping,
      __grouping_size,
      __cs, __cs + __declen);


      int __newlen = __p2 - __new;
      if (__p)
 {
   char_traits<_CharT>::copy(__p2, __p, __len - __declen);
   __newlen += __len - __declen;
 }
      __len = __newlen;
    }
# 996 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
  template<typename _CharT, typename _OutIter>
    template<typename _ValueT>
      _OutIter
      num_put<_CharT, _OutIter>::
      _M_insert_float(_OutIter __s, ios_base& __io, _CharT __fill, char __mod,
         _ValueT __v) const
      {
 typedef __numpunct_cache<_CharT> __cache_type;
 __use_cache<__cache_type> __uc;
 const locale& __loc = __io._M_getloc();
 const __cache_type* __lc = __uc(__loc);


 const streamsize __prec = __io.precision() < 0 ? 6 : __io.precision();

 const int __max_digits =
   __gnu_cxx::__numeric_traits<_ValueT>::__digits10;


 int __len;

 char __fbuf[16];
 __num_base::_S_format_float(__io, __fbuf, __mod);



 const bool __use_prec =
   (__io.flags() & ios_base::floatfield) != ios_base::floatfield;



 int __cs_size = __max_digits * 3;
 char* __cs = static_cast<char*>(__builtin_alloca(__cs_size));
 if (__use_prec)
   __len = std::__convert_from_v(_S_get_c_locale(), __cs, __cs_size,
     __fbuf, __prec, __v);
 else
   __len = std::__convert_from_v(_S_get_c_locale(), __cs, __cs_size,
     __fbuf, __v);


 if (__len >= __cs_size)
   {
     __cs_size = __len + 1;
     __cs = static_cast<char*>(__builtin_alloca(__cs_size));
     if (__use_prec)
       __len = std::__convert_from_v(_S_get_c_locale(), __cs, __cs_size,
         __fbuf, __prec, __v);
     else
       __len = std::__convert_from_v(_S_get_c_locale(), __cs, __cs_size,
         __fbuf, __v);
   }
# 1069 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
 const ctype<_CharT>& __ctype = use_facet<ctype<_CharT> >(__loc);

 _CharT* __ws = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
            * __len));
 __ctype.widen(__cs, __cs + __len, __ws);


 _CharT* __wp = 0;
 const char* __p = char_traits<char>::find(__cs, __len, '.');
 if (__p)
   {
     __wp = __ws + (__p - __cs);
     *__wp = __lc->_M_decimal_point;
   }




 if (__lc->_M_use_grouping
     && (__wp || __len < 3 || (__cs[1] <= '9' && __cs[2] <= '9'
          && __cs[1] >= '0' && __cs[2] >= '0')))
   {


     _CharT* __ws2 = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
          * __len * 2));

     streamsize __off = 0;
     if (__cs[0] == '-' || __cs[0] == '+')
       {
  __off = 1;
  __ws2[0] = __ws[0];
  __len -= 1;
       }

     _M_group_float(__lc->_M_grouping, __lc->_M_grouping_size,
      __lc->_M_thousands_sep, __wp, __ws2 + __off,
      __ws + __off, __len);
     __len += __off;

     __ws = __ws2;
   }


 const streamsize __w = __io.width();
 if (__w > static_cast<streamsize>(__len))
   {
     _CharT* __ws3 = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
          * __w));
     _M_pad(__fill, __w, __io, __ws3, __ws, __len);
     __ws = __ws3;
   }
 __io.width(0);



 return std::__write(__s, __ws, __len);
      }

  template<typename _CharT, typename _OutIter>
    _OutIter
    num_put<_CharT, _OutIter>::
    do_put(iter_type __s, ios_base& __io, char_type __fill, bool __v) const
    {
      const ios_base::fmtflags __flags = __io.flags();
      if ((__flags & ios_base::boolalpha) == 0)
        {
          const long __l = __v;
          __s = _M_insert_int(__s, __io, __fill, __l);
        }
      else
        {
   typedef __numpunct_cache<_CharT> __cache_type;
   __use_cache<__cache_type> __uc;
   const locale& __loc = __io._M_getloc();
   const __cache_type* __lc = __uc(__loc);

   const _CharT* __name = __v ? __lc->_M_truename
                              : __lc->_M_falsename;
   int __len = __v ? __lc->_M_truename_size
                   : __lc->_M_falsename_size;

   const streamsize __w = __io.width();
   if (__w > static_cast<streamsize>(__len))
     {
       const streamsize __plen = __w - __len;
       _CharT* __ps
  = static_cast<_CharT*>(__builtin_alloca(sizeof(_CharT)
       * __plen));

       char_traits<_CharT>::assign(__ps, __plen, __fill);
       __io.width(0);

       if ((__flags & ios_base::adjustfield) == ios_base::left)
  {
    __s = std::__write(__s, __name, __len);
    __s = std::__write(__s, __ps, __plen);
  }
       else
  {
    __s = std::__write(__s, __ps, __plen);
    __s = std::__write(__s, __name, __len);
  }
       return __s;
     }
   __io.width(0);
   __s = std::__write(__s, __name, __len);
 }
      return __s;
    }

  template<typename _CharT, typename _OutIter>
    _OutIter
    num_put<_CharT, _OutIter>::
    do_put(iter_type __s, ios_base& __io, char_type __fill, double __v) const
    { return _M_insert_float(__s, __io, __fill, char(), __v); }
# 1194 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
  template<typename _CharT, typename _OutIter>
    _OutIter
    num_put<_CharT, _OutIter>::
    do_put(iter_type __s, ios_base& __io, char_type __fill,
    long double __v) const
    { return _M_insert_float(__s, __io, __fill, 'L', __v); }

  template<typename _CharT, typename _OutIter>
    _OutIter
    num_put<_CharT, _OutIter>::
    do_put(iter_type __s, ios_base& __io, char_type __fill,
           const void* __v) const
    {
      const ios_base::fmtflags __flags = __io.flags();
      const ios_base::fmtflags __fmt = ~(ios_base::basefield
      | ios_base::uppercase);
      __io.flags((__flags & __fmt) | (ios_base::hex | ios_base::showbase));

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      typedef __gnu_cxx::__conditional_type<(sizeof(const void*)
          <= sizeof(unsigned long)),
 unsigned long, unsigned long long>::__type _UIntPtrType;
#pragma GCC diagnostic pop

      __s = _M_insert_int(__s, __io, __fill,
     reinterpret_cast<_UIntPtrType>(__v));
      __io.flags(__flags);
      return __s;
    }
# 1234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3

# 1243 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.tcc" 3
  template<typename _CharT, typename _Traits>
    void
    __pad<_CharT, _Traits>::_S_pad(ios_base& __io, _CharT __fill,
       _CharT* __news, const _CharT* __olds,
       streamsize __newlen, streamsize __oldlen)
    {
      const size_t __plen = static_cast<size_t>(__newlen - __oldlen);
      const ios_base::fmtflags __adjust = __io.flags() & ios_base::adjustfield;


      if (__adjust == ios_base::left)
 {
   _Traits::copy(__news, __olds, __oldlen);
   _Traits::assign(__news + __oldlen, __plen, __fill);
   return;
 }

      size_t __mod = 0;
      if (__adjust == ios_base::internal)
 {



          const locale& __loc = __io._M_getloc();
   const ctype<_CharT>& __ctype = use_facet<ctype<_CharT> >(__loc);

   if (__ctype.widen('-') == __olds[0]
       || __ctype.widen('+') == __olds[0])
     {
       __news[0] = __olds[0];
       __mod = 1;
       ++__news;
     }
   else if (__ctype.widen('0') == __olds[0]
     && __oldlen > 1
     && (__ctype.widen('x') == __olds[1]
         || __ctype.widen('X') == __olds[1]))
     {
       __news[0] = __olds[0];
       __news[1] = __olds[1];
       __mod = 2;
       __news += 2;
     }

 }
      _Traits::assign(__news, __plen, __fill);
      _Traits::copy(__news + __plen, __olds + __mod, __oldlen - __mod);
    }

  template<typename _CharT>
    _CharT*
    __add_grouping(_CharT* __s, _CharT __sep,
     const char* __gbeg, size_t __gsize,
     const _CharT* __first, const _CharT* __last)
    {
      size_t __idx = 0;
      size_t __ctr = 0;

      while (__last - __first > __gbeg[__idx]
      && static_cast<signed char>(__gbeg[__idx]) > 0
      && __gbeg[__idx] != __gnu_cxx::__numeric_traits<char>::__max)
 {
   __last -= __gbeg[__idx];
   __idx < __gsize - 1 ? ++__idx : ++__ctr;
 }

      while (__first != __last)
 *__s++ = *__first++;

      while (__ctr--)
 {
   *__s++ = __sep;
   for (char __i = __gbeg[__idx]; __i > 0; --__i)
     *__s++ = *__first++;
 }

      while (__idx--)
 {
   *__s++ = __sep;
   for (char __i = __gbeg[__idx]; __i > 0; --__i)
     *__s++ = *__first++;
 }

      return __s;
    }




  extern template class __cxx11:: numpunct<char>;
  extern template class __cxx11:: numpunct_byname<char>;
  extern template class num_get<char>;
  extern template class num_put<char>;
  extern template class ctype_byname<char>;

  extern template
    const ctype<char>*
    __try_use_facet<ctype<char> >(const locale&) throw();

  extern template
    const numpunct<char>*
    __try_use_facet<numpunct<char> >(const locale&) throw();

  extern template
    const num_put<char>*
    __try_use_facet<num_put<char> >(const locale&) throw();

  extern template
    const num_get<char>*
    __try_use_facet<num_get<char> >(const locale&) throw();

  extern template
    const ctype<char>&
    use_facet<ctype<char> >(const locale&);

  extern template
    const numpunct<char>&
    use_facet<numpunct<char> >(const locale&);

  extern template
    const num_put<char>&
    use_facet<num_put<char> >(const locale&);

  extern template
    const num_get<char>&
    use_facet<num_get<char> >(const locale&);

  extern template
    bool
    has_facet<ctype<char> >(const locale&);

  extern template
    bool
    has_facet<numpunct<char> >(const locale&);

  extern template
    bool
    has_facet<num_put<char> >(const locale&);

  extern template
    bool
    has_facet<num_get<char> >(const locale&);


  extern template class __cxx11:: numpunct<wchar_t>;
  extern template class __cxx11:: numpunct_byname<wchar_t>;
  extern template class num_get<wchar_t>;
  extern template class num_put<wchar_t>;
  extern template class ctype_byname<wchar_t>;

  extern template
    const ctype<wchar_t>*
    __try_use_facet<ctype<wchar_t> >(const locale&) throw();

  extern template
    const numpunct<wchar_t>*
    __try_use_facet<numpunct<wchar_t> >(const locale&) throw();

  extern template
    const num_put<wchar_t>*
    __try_use_facet<num_put<wchar_t> >(const locale&) throw();

  extern template
    const num_get<wchar_t>*
    __try_use_facet<num_get<wchar_t> >(const locale&) throw();

  extern template
    const ctype<wchar_t>&
    use_facet<ctype<wchar_t> >(const locale&);

  extern template
    const numpunct<wchar_t>&
    use_facet<numpunct<wchar_t> >(const locale&);

  extern template
    const num_put<wchar_t>&
    use_facet<num_put<wchar_t> >(const locale&);

  extern template
    const num_get<wchar_t>&
    use_facet<num_get<wchar_t> >(const locale&);

  extern template
    bool
    has_facet<ctype<wchar_t> >(const locale&);

  extern template
    bool
    has_facet<numpunct<wchar_t> >(const locale&);

  extern template
    bool
    has_facet<num_put<wchar_t> >(const locale&);

  extern template
    bool
    has_facet<num_get<wchar_t> >(const locale&);




}

#pragma GCC diagnostic pop
# 2702 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/locale_facets.h" 2 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 2 3



namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _Facet>
    inline const _Facet&
    __check_facet(const _Facet* __f)
    {
      if (!__f)
 __throw_bad_cast();
      return *__f;
    }
# 68 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
  template<typename _CharT, typename _Traits>
    class basic_ios : public ios_base
    {




    public:






      typedef _CharT char_type;
      typedef typename _Traits::int_type int_type;
      typedef typename _Traits::pos_type pos_type;
      typedef typename _Traits::off_type off_type;
      typedef _Traits traits_type;






      typedef ctype<_CharT> __ctype_type;
      typedef num_put<_CharT, ostreambuf_iterator<_CharT, _Traits> >
           __num_put_type;
      typedef num_get<_CharT, istreambuf_iterator<_CharT, _Traits> >
           __num_get_type;



    protected:
      basic_ostream<_CharT, _Traits>* _M_tie;
      mutable char_type _M_fill;
      mutable bool _M_fill_init;
      basic_streambuf<_CharT, _Traits>* _M_streambuf;


      const __ctype_type* _M_ctype;

      const __num_put_type* _M_num_put;

      const __num_get_type* _M_num_get;

    public:
# 127 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      operator void*() const
      { return this->fail() ? 0 : const_cast<basic_ios*>(this); }


     
      bool
      operator!() const
      { return this->fail(); }
# 144 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
     
      iostate
      rdstate() const
      { return _M_streambuf_state; }
# 156 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      void
      clear(iostate __state = goodbit);







      void
      setstate(iostate __state)
      { this->clear(this->rdstate() | __state); }




      void
      _M_setstate(iostate __state)
      {


 _M_streambuf_state |= __state;
 if (this->exceptions() & __state)
   { throw; }
      }







     
      bool
      good() const
      { return this->rdstate() == 0; }







     
      bool
      eof() const
      { return (this->rdstate() & eofbit) != 0; }
# 211 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
     
      bool
      fail() const
      { return (this->rdstate() & (badbit | failbit)) != 0; }







     
      bool
      bad() const
      { return (this->rdstate() & badbit) != 0; }
# 234 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
     
      iostate
      exceptions() const
      { return _M_exception; }
# 270 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      void
      exceptions(iostate __except)
      {
        _M_exception = __except;
        this->clear(_M_streambuf_state);
      }







      explicit
      basic_ios(basic_streambuf<_CharT, _Traits>* __sb)
      : ios_base(), _M_tie(0), _M_fill(), _M_fill_init(false), _M_streambuf(0),
 _M_ctype(0), _M_num_put(0), _M_num_get(0)
      { this->init(__sb); }







      virtual
      ~basic_ios() { }
# 308 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
     
      basic_ostream<_CharT, _Traits>*
      tie() const
      { return _M_tie; }
# 321 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      basic_ostream<_CharT, _Traits>*
      tie(basic_ostream<_CharT, _Traits>* __tiestr)
      {
        basic_ostream<_CharT, _Traits>* __old = _M_tie;
        _M_tie = __tiestr;
        return __old;
      }







     
      basic_streambuf<_CharT, _Traits>*
      rdbuf() const
      { return _M_streambuf; }
# 362 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      basic_streambuf<_CharT, _Traits>*
      rdbuf(basic_streambuf<_CharT, _Traits>* __sb);
# 376 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      basic_ios&
      copyfmt(const basic_ios& __rhs);







     
      char_type
      fill() const
      {
 if (__builtin_expect(!_M_fill_init, false))
   return this->widen(' ');
 return _M_fill;
      }
# 403 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      char_type
      fill(char_type __ch)
      {
 char_type __old = _M_fill;
 _M_fill = __ch;
 _M_fill_init = true;
 return __old;
      }
# 424 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      locale
      imbue(const locale& __loc);
# 444 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      char
      narrow(char_type __c, char __dfault) const
      { return __check_facet(_M_ctype).narrow(__c, __dfault); }
# 463 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      char_type
      widen(char __c) const
      { return __check_facet(_M_ctype).widen(__c); }

    protected:







      basic_ios()
      : ios_base(), _M_tie(0), _M_fill(char_type()), _M_fill_init(false),
 _M_streambuf(0), _M_ctype(0), _M_num_put(0), _M_num_get(0)
      { }







      void
      init(basic_streambuf<_CharT, _Traits>* __sb);
# 524 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 3
      void
      _M_cache_locale(const locale& __loc);
    };


}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.tcc" 1 3
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"

namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _CharT, typename _Traits>
    void
    basic_ios<_CharT, _Traits>::clear(iostate __state)
    {
      if (this->rdbuf())
 _M_streambuf_state = __state;
      else
 _M_streambuf_state = __state | badbit;
      if (this->exceptions() & this->rdstate())
 __throw_ios_failure(("basic_ios::clear"));
    }

  template<typename _CharT, typename _Traits>
    basic_streambuf<_CharT, _Traits>*
    basic_ios<_CharT, _Traits>::rdbuf(basic_streambuf<_CharT, _Traits>* __sb)
    {
      basic_streambuf<_CharT, _Traits>* __old = _M_streambuf;
      _M_streambuf = __sb;
      this->clear();
      return __old;
    }

  template<typename _CharT, typename _Traits>
    basic_ios<_CharT, _Traits>&
    basic_ios<_CharT, _Traits>::copyfmt(const basic_ios& __rhs)
    {


      if (this != std::__addressof(__rhs))
 {




   _Words* __words = (__rhs._M_word_size <= _S_local_word_size) ?
                      _M_local_word : new _Words[__rhs._M_word_size];


   _Callback_list* __cb = __rhs._M_callbacks;
   if (__cb)
     __cb->_M_add_reference();
   _M_call_callbacks(erase_event);
   if (_M_word != _M_local_word)
     {
       delete [] _M_word;
       _M_word = 0;
     }
   _M_dispose_callbacks();


   _M_callbacks = __cb;
   for (int __i = 0; __i < __rhs._M_word_size; ++__i)
     __words[__i] = __rhs._M_word[__i];
   _M_word = __words;
   _M_word_size = __rhs._M_word_size;

   this->flags(__rhs.flags());
   this->width(__rhs.width());
   this->precision(__rhs.precision());
   this->tie(__rhs.tie());
   this->fill(__rhs.fill());
   _M_ios_locale = __rhs.getloc();
   _M_cache_locale(_M_ios_locale);

   _M_call_callbacks(copyfmt_event);


   this->exceptions(__rhs.exceptions());
 }
      return *this;
    }


  template<typename _CharT, typename _Traits>
    locale
    basic_ios<_CharT, _Traits>::imbue(const locale& __loc)
    {
      locale __old(this->getloc());
      ios_base::imbue(__loc);
      _M_cache_locale(__loc);
      if (this->rdbuf() != 0)
 this->rdbuf()->pubimbue(__loc);
      return __old;
    }

  template<typename _CharT, typename _Traits>
    void
    basic_ios<_CharT, _Traits>::init(basic_streambuf<_CharT, _Traits>* __sb)
    {

      ios_base::_M_init();


      _M_cache_locale(_M_ios_locale);
# 153 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.tcc" 3
      if (_M_ctype)
 {
   _M_fill = _M_ctype->widen(' ');
   _M_fill_init = true;
 }
      else
 _M_fill_init = false;

      _M_tie = 0;
      _M_exception = goodbit;
      _M_streambuf = __sb;
      _M_streambuf_state = __sb ? goodbit : badbit;
    }

  template<typename _CharT, typename _Traits>
    void
    basic_ios<_CharT, _Traits>::_M_cache_locale(const locale& __loc)
    {
      _M_ctype = std::__try_use_facet<__ctype_type>(__loc);
      _M_num_put = std::__try_use_facet<__num_put_type>(__loc);
      _M_num_get = std::__try_use_facet<__num_get_type>(__loc);
    }




  extern template class basic_ios<char>;


  extern template class basic_ios<wchar_t>;




}

#pragma GCC diagnostic pop
# 532 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/basic_ios.h" 2 3
# 49 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ios" 2 3
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 2 3






# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 50 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 67 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
  template<typename _CharT, typename _Traits>
    class basic_ostream : virtual public basic_ios<_CharT, _Traits>
    {
    public:

      typedef _CharT char_type;
      typedef typename _Traits::int_type int_type;
      typedef typename _Traits::pos_type pos_type;
      typedef typename _Traits::off_type off_type;
      typedef _Traits traits_type;


      typedef basic_streambuf<_CharT, _Traits> __streambuf_type;
      typedef basic_ios<_CharT, _Traits> __ios_type;
      typedef basic_ostream<_CharT, _Traits> __ostream_type;
      typedef num_put<_CharT, ostreambuf_iterator<_CharT, _Traits> >
             __num_put_type;
      typedef ctype<_CharT> __ctype_type;
# 93 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      explicit
      basic_ostream(__streambuf_type* __sb)
      { this->init(__sb); }






      virtual
      ~basic_ostream() { }


      class sentry;
      friend class sentry;
# 117 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      operator<<(__ostream_type& (*__pf)(__ostream_type&))
      {



 return __pf(*this);
      }

      __ostream_type&
      operator<<(__ios_type& (*__pf)(__ios_type&))
      {



 __pf(*this);
 return *this;
      }

      __ostream_type&
      operator<<(ios_base& (*__pf) (ios_base&))
      {



 __pf(*this);
 return *this;
      }
# 175 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      operator<<(long __n)
      { return _M_insert(__n); }

      __ostream_type&
      operator<<(unsigned long __n)
      { return _M_insert(__n); }

      __ostream_type&
      operator<<(bool __n)
      { return _M_insert(__n); }

      __ostream_type&
      operator<<(short __n);

      __ostream_type&
      operator<<(unsigned short __n)
      {


 return _M_insert(static_cast<unsigned long>(__n));
      }

      __ostream_type&
      operator<<(int __n);

      __ostream_type&
      operator<<(unsigned int __n)
      {


 return _M_insert(static_cast<unsigned long>(__n));
      }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      __ostream_type&
      operator<<(long long __n)
      { return _M_insert(__n); }

      __ostream_type&
      operator<<(unsigned long long __n)
      { return _M_insert(__n); }
#pragma GCC diagnostic pop
# 232 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      operator<<(double __f)
      { return _M_insert(__f); }

      __ostream_type&
      operator<<(float __f)
      {


 return _M_insert(_S_cast_flt<double>(__f));
      }

      __ostream_type&
      operator<<(long double __f)
      { return _M_insert(__f); }
# 302 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      operator<<(const void* __p)
      { return _M_insert(__p); }
# 340 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      operator<<(__streambuf_type* __sb);
# 373 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      put(char_type __c);
# 392 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      write(const char_type* __s, streamsize __n);
# 405 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      flush();
# 415 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      pos_type
      tellp();
# 426 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      __ostream_type&
      seekp(pos_type);
# 438 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
       __ostream_type&
      seekp(off_type, ios_base::seekdir);

    protected:
      basic_ostream()
      { this->init(0); }
# 471 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      template<typename _ValueT>
 __ostream_type&
 _M_insert(_ValueT __v);

    private:

      void
      _M_write(const char_type* __s, streamsize __n)
      { std::__ostream_insert(*this, __s, __n); }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++17-extensions"
      template<typename _To, typename _From>
 static _To
 _S_cast_flt(_From __f)
 {
   _To __d = static_cast<_To>(__f);
# 509 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
   return __d;
 }
    };
#pragma GCC diagnostic pop
# 521 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
  template <typename _CharT, typename _Traits>
    class basic_ostream<_CharT, _Traits>::sentry
    {

      bool _M_ok;
      basic_ostream<_CharT, _Traits>& _M_os;

    public:
# 540 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      explicit
      sentry(basic_ostream<_CharT, _Traits>& __os);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wdeprecated-declarations"







      ~sentry()
      {

 if (bool(_M_os.flags() & ios_base::unitbuf) && !uncaught_exception())
   {

     if (_M_os.rdbuf() && _M_os.rdbuf()->pubsync() == -1)
       _M_os.setstate(ios_base::badbit);
   }
      }
#pragma GCC diagnostic pop
# 574 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
      operator bool() const
      { return _M_ok; }
    };
# 594 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
  template<typename _CharT, typename _Traits>
    inline basic_ostream<_CharT, _Traits>&
    operator<<(basic_ostream<_CharT, _Traits>& __out, _CharT __c)
    {
      if (__out.width() != 0)
 return __ostream_insert(__out, &__c, 1);
      __out.put(__c);
      return __out;
    }

  template<typename _CharT, typename _Traits>
    inline basic_ostream<_CharT, _Traits>&
    operator<<(basic_ostream<_CharT, _Traits>& __out, char __c)
    { return (__out << __out.widen(__c)); }


  template<typename _Traits>
    inline basic_ostream<char, _Traits>&
    operator<<(basic_ostream<char, _Traits>& __out, char __c)
    {
      if (__out.width() != 0)
 return __ostream_insert(__out, &__c, 1);
      __out.put(__c);
      return __out;
    }


  template<typename _Traits>
    inline basic_ostream<char, _Traits>&
    operator<<(basic_ostream<char, _Traits>& __out, signed char __c)
    { return (__out << static_cast<char>(__c)); }

  template<typename _Traits>
    inline basic_ostream<char, _Traits>&
    operator<<(basic_ostream<char, _Traits>& __out, unsigned char __c)
    { return (__out << static_cast<char>(__c)); }
# 685 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
  template<typename _CharT, typename _Traits>
    inline basic_ostream<_CharT, _Traits>&
    operator<<(basic_ostream<_CharT, _Traits>& __out, const _CharT* __s)
    {
      if (!__s)
 __out.setstate(ios_base::badbit);
      else
 __ostream_insert(__out, __s,
    static_cast<streamsize>(_Traits::length(__s)));
      return __out;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits> &
    operator<<(basic_ostream<_CharT, _Traits>& __out, const char* __s);


  template<typename _Traits>
    inline basic_ostream<char, _Traits>&
    operator<<(basic_ostream<char, _Traits>& __out, const char* __s)
    {
      if (!__s)
 __out.setstate(ios_base::badbit);
      else
 __ostream_insert(__out, __s,
    static_cast<streamsize>(_Traits::length(__s)));
      return __out;
    }


  template<typename _Traits>
    inline basic_ostream<char, _Traits>&
    operator<<(basic_ostream<char, _Traits>& __out, const signed char* __s)
    { return (__out << reinterpret_cast<const char*>(__s)); }

  template<typename _Traits>
    inline basic_ostream<char, _Traits> &
    operator<<(basic_ostream<char, _Traits>& __out, const unsigned char* __s)
    { return (__out << reinterpret_cast<const char*>(__s)); }
# 775 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
  template<typename _CharT, typename _Traits>
    inline basic_ostream<_CharT, _Traits>&
    endl(basic_ostream<_CharT, _Traits>& __os)
    { return flush(__os.put(__os.widen('\n'))); }
# 787 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3
  template<typename _CharT, typename _Traits>
    inline basic_ostream<_CharT, _Traits>&
    ends(basic_ostream<_CharT, _Traits>& __os)
    { return __os.put(_CharT()); }






  template<typename _CharT, typename _Traits>
    inline basic_ostream<_CharT, _Traits>&
    flush(basic_ostream<_CharT, _Traits>& __os)
    { return __os.flush(); }
# 1052 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 3

}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ostream.tcc" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/ostream.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"



namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>::sentry::
    sentry(basic_ostream<_CharT, _Traits>& __os)
    : _M_ok(false), _M_os(__os)
    {

      if (__os.tie() && __os.good())
 __os.tie()->flush();

      if (__os.good())
 _M_ok = true;
      else if (__os.bad())
 __os.setstate(ios_base::failbit);
    }

  template<typename _CharT, typename _Traits>
    template<typename _ValueT>
      basic_ostream<_CharT, _Traits>&
      basic_ostream<_CharT, _Traits>::
      _M_insert(_ValueT __v)
      {
 sentry __cerb(*this);
 if (__cerb)
   {
     ios_base::iostate __err = ios_base::goodbit;
     try
       {

  const __num_put_type& __np = __check_facet(this->_M_num_put);




  if (__np.put(*this, *this, this->fill(), __v).failed())
    __err |= ios_base::badbit;
       }
     catch(__cxxabiv1::__forced_unwind&)
       {
  this->_M_setstate(ios_base::badbit);
  throw;
       }
     catch(...)
       { this->_M_setstate(ios_base::badbit); }
     if (__err)
       this->setstate(__err);
   }
 return *this;
      }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    operator<<(short __n)
    {


      const ios_base::fmtflags __fmt = this->flags() & ios_base::basefield;
      if (__fmt == ios_base::oct || __fmt == ios_base::hex)
 return _M_insert(static_cast<long>(static_cast<unsigned short>(__n)));
      else
 return _M_insert(static_cast<long>(__n));
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    operator<<(int __n)
    {


      const ios_base::fmtflags __fmt = this->flags() & ios_base::basefield;
      if (__fmt == ios_base::oct || __fmt == ios_base::hex)
 return _M_insert(static_cast<long>(static_cast<unsigned int>(__n)));
      else
 return _M_insert(static_cast<long>(__n));
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    operator<<(__streambuf_type* __sbin)
    {
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this);
      if (__cerb && __sbin)
 {
   try
     {
       if (!__copy_streambufs(__sbin, this->rdbuf()))
  __err |= ios_base::failbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::failbit); }
 }
      else if (!__sbin)
 __err |= ios_base::badbit;
      if (__err)
 this->setstate(__err);
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    put(char_type __c)
    {






      sentry __cerb(*this);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       const int_type __put = this->rdbuf()->sputc(__c);
       if (traits_type::eq_int_type(__put, traits_type::eof()))
  __err |= ios_base::badbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    write(const _CharT* __s, streamsize __n)
    {







      sentry __cerb(*this);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       if (this->rdbuf()->sputn(__s, __n) != __n)
  __err = ios_base::badbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(ios_base::badbit);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    flush()
    {





      if (__streambuf_type* __buf = this->rdbuf())
 {
   sentry __cerb(*this);
   if (__cerb)
     {
       ios_base::iostate __err = ios_base::goodbit;
       try
  {
    if (this->rdbuf()->pubsync() == -1)
      __err |= ios_base::badbit;
  }
       catch(__cxxabiv1::__forced_unwind&)
  {
    this->_M_setstate(ios_base::badbit);
    throw;
  }
       catch(...)
  { this->_M_setstate(ios_base::badbit); }
       if (__err)
  this->setstate(__err);
     }
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    typename basic_ostream<_CharT, _Traits>::pos_type
    basic_ostream<_CharT, _Traits>::
    tellp()
    {
      sentry __cerb(*this);
      pos_type __ret = pos_type(-1);
      if (!this->fail())
 __ret = this->rdbuf()->pubseekoff(0, ios_base::cur, ios_base::out);
      return __ret;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    seekp(pos_type __pos)
    {
      sentry __cerb(*this);
      if (!this->fail())
 {


   const pos_type __p = this->rdbuf()->pubseekpos(__pos, ios_base::out);


   if (__p == pos_type(off_type(-1)))
     this->setstate(ios_base::failbit);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    basic_ostream<_CharT, _Traits>::
    seekp(off_type __off, ios_base::seekdir __dir)
    {
      sentry __cerb(*this);
      if (!this->fail())
 {


   const pos_type __p = this->rdbuf()->pubseekoff(__off, __dir,
        ios_base::out);


   if (__p == pos_type(off_type(-1)))
     this->setstate(ios_base::failbit);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_ostream<_CharT, _Traits>&
    operator<<(basic_ostream<_CharT, _Traits>& __out, const char* __s)
    {
      if (!__s)
 __out.setstate(ios_base::badbit);
      else
 {


   const size_t __clen = char_traits<char>::length(__s);
   try
     {
       struct __ptr_guard
       {
  _CharT *__p;
  __ptr_guard (_CharT *__ip): __p(__ip) { }
  ~__ptr_guard() { delete[] __p; }
  _CharT* __get() { return __p; }
       } __pg (new _CharT[__clen]);

       _CharT *__ws = __pg.__get();
       for (size_t __i = 0; __i < __clen; ++__i)
  __ws[__i] = __out.widen(__s[__i]);
       __ostream_insert(__out, __ws, __clen);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __out._M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { __out._M_setstate(ios_base::badbit); }
 }
      return __out;
    }




  extern template class basic_ostream<char>;
  extern template ostream& endl(ostream&);
  extern template ostream& ends(ostream&);
  extern template ostream& flush(ostream&);
  extern template ostream& operator<<(ostream&, char);
  extern template ostream& operator<<(ostream&, unsigned char);
  extern template ostream& operator<<(ostream&, signed char);
  extern template ostream& operator<<(ostream&, const char*);
  extern template ostream& operator<<(ostream&, const unsigned char*);
  extern template ostream& operator<<(ostream&, const signed char*);

  extern template ostream& ostream::_M_insert(long);
  extern template ostream& ostream::_M_insert(unsigned long);
  extern template ostream& ostream::_M_insert(bool);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
  extern template ostream& ostream::_M_insert(long long);
  extern template ostream& ostream::_M_insert(unsigned long long);
#pragma GCC diagnostic pop

  extern template ostream& ostream::_M_insert(double);
  extern template ostream& ostream::_M_insert(long double);
  extern template ostream& ostream::_M_insert(const void*);


  extern template class basic_ostream<wchar_t>;
  extern template wostream& endl(wostream&);
  extern template wostream& ends(wostream&);
  extern template wostream& flush(wostream&);
  extern template wostream& operator<<(wostream&, wchar_t);
  extern template wostream& operator<<(wostream&, char);
  extern template wostream& operator<<(wostream&, const wchar_t*);
  extern template wostream& operator<<(wostream&, const char*);

  extern template wostream& wostream::_M_insert(long);
  extern template wostream& wostream::_M_insert(unsigned long);
  extern template wostream& wostream::_M_insert(bool);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
  extern template wostream& wostream::_M_insert(long long);
  extern template wostream& wostream::_M_insert(unsigned long long);
#pragma GCC diagnostic pop

  extern template wostream& wostream::_M_insert(double);
  extern template wostream& wostream::_M_insert(long double);
  extern template wostream& wostream::_M_insert(const void*);




}

#pragma GCC diagnostic pop
# 1056 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ostream" 2 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iostream" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
namespace std __attribute__ ((__visibility__ ("default")))
{

# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
  template<typename _CharT, typename _Traits>
    class basic_istream : virtual public basic_ios<_CharT, _Traits>
    {
    public:

      typedef _CharT char_type;
      typedef typename _Traits::int_type int_type;
      typedef typename _Traits::pos_type pos_type;
      typedef typename _Traits::off_type off_type;
      typedef _Traits traits_type;


      typedef basic_streambuf<_CharT, _Traits> __streambuf_type;
      typedef basic_ios<_CharT, _Traits> __ios_type;
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef num_get<_CharT, istreambuf_iterator<_CharT, _Traits> >
        __num_get_type;
      typedef ctype<_CharT> __ctype_type;

    protected:





      streamsize _M_gcount;

    public:







      explicit
      basic_istream(__streambuf_type* __sb)
      : _M_gcount(streamsize(0))
      { this->init(__sb); }






      virtual
      ~basic_istream()
      { _M_gcount = streamsize(0); }


      class sentry;
      friend class sentry;
# 123 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      operator>>(__istream_type& (*__pf)(__istream_type&))
      { return __pf(*this); }

      __istream_type&
      operator>>(__ios_type& (*__pf)(__ios_type&))
      {
 __pf(*this);
 return *this;
      }

      __istream_type&
      operator>>(ios_base& (*__pf)(ios_base&))
      {
 __pf(*this);
 return *this;
      }
# 171 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      operator>>(bool& __n)
      { return _M_extract(__n); }

      __istream_type&
      operator>>(short& __n);

      __istream_type&
      operator>>(unsigned short& __n)
      { return _M_extract(__n); }

      __istream_type&
      operator>>(int& __n);

      __istream_type&
      operator>>(unsigned int& __n)
      { return _M_extract(__n); }

      __istream_type&
      operator>>(long& __n)
      { return _M_extract(__n); }

      __istream_type&
      operator>>(unsigned long& __n)
      { return _M_extract(__n); }


#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
      __istream_type&
      operator>>(long long& __n)
      { return _M_extract(__n); }

      __istream_type&
      operator>>(unsigned long long& __n)
      { return _M_extract(__n); }
#pragma GCC diagnostic pop
# 220 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      operator>>(float& __f)
      { return _M_extract(__f); }

      __istream_type&
      operator>>(double& __f)
      { return _M_extract(__f); }

      __istream_type&
      operator>>(long double& __f)
      { return _M_extract(__f); }
# 329 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      operator>>(void*& __p)
      { return _M_extract(__p); }
# 353 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      operator>>(__streambuf_type* __sb);
# 363 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      streamsize
      gcount() const
      { return _M_gcount; }
# 396 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      int_type
      get();
# 410 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      get(char_type& __c);
# 437 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      get(char_type* __s, streamsize __n, char_type __delim);
# 448 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      get(char_type* __s, streamsize __n)
      { return this->get(__s, __n, this->widen('\n')); }
# 471 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      get(__streambuf_type& __sb, char_type __delim);
# 481 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      get(__streambuf_type& __sb)
      { return this->get(__sb, this->widen('\n')); }
# 510 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      getline(char_type* __s, streamsize __n, char_type __delim);
# 521 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      getline(char_type* __s, streamsize __n)
      { return this->getline(__s, __n, this->widen('\n')); }
# 545 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      ignore(streamsize __n, int_type __delim);

      __istream_type&
      ignore(streamsize __n);

      __istream_type&
      ignore();
# 562 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      int_type
      peek();
# 580 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      read(char_type* __s, streamsize __n);
# 599 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      streamsize
      readsome(char_type* __s, streamsize __n);
# 616 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      putback(char_type __c);
# 632 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      unget();
# 650 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      int
      sync();
# 665 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      pos_type
      tellg();
# 680 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      seekg(pos_type);
# 696 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      __istream_type&
      seekg(off_type, ios_base::seekdir);


    protected:
      basic_istream()
      : _M_gcount(streamsize(0))
      { this->init(0); }
# 734 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      template<typename _ValueT>
 __istream_type&
 _M_extract(_ValueT& __v);
    };


  template<>
    basic_istream<char>&
    basic_istream<char>::
    getline(char_type* __s, streamsize __n, char_type __delim);

  template<>
    basic_istream<char>&
    basic_istream<char>::
    ignore(streamsize __n);

  template<>
    basic_istream<char>&
    basic_istream<char>::
    ignore(streamsize __n, int_type __delim);


  template<>
    basic_istream<wchar_t>&
    basic_istream<wchar_t>::
    getline(char_type* __s, streamsize __n, char_type __delim);

  template<>
    basic_istream<wchar_t>&
    basic_istream<wchar_t>::
    ignore(streamsize __n);

  template<>
    basic_istream<wchar_t>&
    basic_istream<wchar_t>::
    ignore(streamsize __n, int_type __delim);
# 780 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
  template<typename _CharT, typename _Traits>
    class basic_istream<_CharT, _Traits>::sentry
    {

      bool _M_ok;

    public:

      typedef _Traits traits_type;
      typedef basic_streambuf<_CharT, _Traits> __streambuf_type;
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef typename __istream_type::__ctype_type __ctype_type;
      typedef typename _Traits::int_type __int_type;
# 816 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      explicit
      sentry(basic_istream<_CharT, _Traits>& __is, bool __noskipws = false);
# 829 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
      operator bool() const
      { return _M_ok; }
    };
# 845 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    operator>>(basic_istream<_CharT, _Traits>& __in, _CharT& __c);

  template<class _Traits>
    inline basic_istream<char, _Traits>&
    operator>>(basic_istream<char, _Traits>& __in, unsigned char& __c)
    { return (__in >> reinterpret_cast<char&>(__c)); }

  template<class _Traits>
    inline basic_istream<char, _Traits>&
    operator>>(basic_istream<char, _Traits>& __in, signed char& __c)
    { return (__in >> reinterpret_cast<char&>(__c)); }



  template<typename _CharT, typename _Traits>
    void
    __istream_extract(basic_istream<_CharT, _Traits>&, _CharT*, streamsize);

  void __istream_extract(istream&, char*, streamsize);
# 895 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
  template<typename _CharT, typename _Traits>
    __attribute__((__nonnull__(2), __access__(__write_only__, 2)))
    inline basic_istream<_CharT, _Traits>&
    operator>>(basic_istream<_CharT, _Traits>& __in, _CharT* __s)
    {
# 929 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
 {

   streamsize __n = __gnu_cxx::__numeric_traits<streamsize>::__max;
   __n /= sizeof(_CharT);
   std::__istream_extract(__in, __s, __n);
 }
      return __in;
    }

  template<class _Traits>
    __attribute__((__nonnull__(2), __access__(__write_only__, 2)))
    inline basic_istream<char, _Traits>&
    operator>>(basic_istream<char, _Traits>& __in, unsigned char* __s)
    { return __in >> reinterpret_cast<char*>(__s); }

  template<class _Traits>
    __attribute__((__nonnull__(2), __access__(__write_only__, 2)))
    inline basic_istream<char, _Traits>&
    operator>>(basic_istream<char, _Traits>& __in, signed char* __s)
    { return __in >> reinterpret_cast<char*>(__s); }
# 984 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
  template<typename _CharT, typename _Traits>
    class basic_iostream
    : public basic_istream<_CharT, _Traits>,
      public basic_ostream<_CharT, _Traits>
    {
    public:



      typedef _CharT char_type;
      typedef typename _Traits::int_type int_type;
      typedef typename _Traits::pos_type pos_type;
      typedef typename _Traits::off_type off_type;
      typedef _Traits traits_type;


      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef basic_ostream<_CharT, _Traits> __ostream_type;







      explicit
      basic_iostream(basic_streambuf<_CharT, _Traits>* __sb)
      : __istream_type(__sb), __ostream_type(__sb) { }




      virtual
      ~basic_iostream() { }

    protected:
      basic_iostream()
      : __istream_type(), __ostream_type() { }
# 1045 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
    };
# 1067 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3
  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    ws(basic_istream<_CharT, _Traits>& __is);
# 1108 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 3

}

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/istream.tcc" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/istream.tcc" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"



namespace std __attribute__ ((__visibility__ ("default")))
{


  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>::sentry::
    sentry(basic_istream<_CharT, _Traits>& __in, bool __noskip) : _M_ok(false)
    {
      ios_base::iostate __err = ios_base::goodbit;
      if (__in.good())
 {
   try
     {
       if (__in.tie())
  __in.tie()->flush();
       if (!__noskip && bool(__in.flags() & ios_base::skipws))
  {
    const __int_type __eof = traits_type::eof();
    __streambuf_type* __sb = __in.rdbuf();
    __int_type __c = __sb->sgetc();

    const __ctype_type& __ct = __check_facet(__in._M_ctype);
    while (!traits_type::eq_int_type(__c, __eof)
    && __ct.is(ctype_base::space,
        traits_type::to_char_type(__c)))
      __c = __sb->snextc();




    if (traits_type::eq_int_type(__c, __eof))
      __err |= ios_base::eofbit;
  }
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __in._M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { __in._M_setstate(ios_base::badbit); }
 }

      if (__in.good() && __err == ios_base::goodbit)
 _M_ok = true;
      else
 {
   __err |= ios_base::failbit;
   __in.setstate(__err);
 }
    }

  template<typename _CharT, typename _Traits>
    template<typename _ValueT>
      basic_istream<_CharT, _Traits>&
      basic_istream<_CharT, _Traits>::
      _M_extract(_ValueT& __v)
      {
 sentry __cerb(*this, false);
 if (__cerb)
   {
     ios_base::iostate __err = ios_base::goodbit;
     try
       {

  const __num_get_type& __ng = __check_facet(this->_M_num_get);




  __ng.get(*this, 0, *this, __err, __v);
       }
     catch(__cxxabiv1::__forced_unwind&)
       {
  this->_M_setstate(ios_base::badbit);
  throw;
       }
     catch(...)
       { this->_M_setstate(ios_base::badbit); }
     if (__err)
       this->setstate(__err);
   }
 return *this;
      }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    operator>>(short& __n)
    {


      sentry __cerb(*this, false);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       long __l;

       const __num_get_type& __ng = __check_facet(this->_M_num_get);




       __ng.get(*this, 0, *this, __err, __l);



       if (__l < __gnu_cxx::__numeric_traits<short>::__min)
  {
    __err |= ios_base::failbit;
    __n = __gnu_cxx::__numeric_traits<short>::__min;
  }
       else if (__l > __gnu_cxx::__numeric_traits<short>::__max)
  {
    __err |= ios_base::failbit;
    __n = __gnu_cxx::__numeric_traits<short>::__max;
  }
       else
  __n = short(__l);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    operator>>(int& __n)
    {


      sentry __cerb(*this, false);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       long __l;

       const __num_get_type& __ng = __check_facet(this->_M_num_get);




       __ng.get(*this, 0, *this, __err, __l);



       if (__l < __gnu_cxx::__numeric_traits<int>::__min)
  {
    __err |= ios_base::failbit;
    __n = __gnu_cxx::__numeric_traits<int>::__min;
  }
       else if (__l > __gnu_cxx::__numeric_traits<int>::__max)
  {
    __err |= ios_base::failbit;
    __n = __gnu_cxx::__numeric_traits<int>::__max;
  }
       else
  __n = int(__l);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    operator>>(__streambuf_type* __sbout)
    {
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this, false);
      if (__cerb && __sbout)
 {
   try
     {
       bool __ineof;
       if (!__copy_streambufs_eof(this->rdbuf(), __sbout, __ineof))
  __err |= ios_base::failbit;
       if (__ineof)
  __err |= ios_base::eofbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::failbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::failbit); }
 }
      else if (!__sbout)
 __err |= ios_base::failbit;
      if (__err)
 this->setstate(__err);
      return *this;
    }

  template<typename _CharT, typename _Traits>
    typename basic_istream<_CharT, _Traits>::int_type
    basic_istream<_CharT, _Traits>::
    get(void)
    {
      const int_type __eof = traits_type::eof();
      int_type __c = __eof;
      _M_gcount = 0;
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   try
     {
       __c = this->rdbuf()->sbumpc();

       if (!traits_type::eq_int_type(__c, __eof))
  _M_gcount = 1;
       else
  __err |= ios_base::eofbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
 }
      if (!_M_gcount)
 __err |= ios_base::failbit;
      if (__err)
 this->setstate(__err);
      return __c;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    get(char_type& __c)
    {
      _M_gcount = 0;
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   try
     {
       const int_type __cb = this->rdbuf()->sbumpc();

       if (!traits_type::eq_int_type(__cb, traits_type::eof()))
  {
    _M_gcount = 1;
    __c = traits_type::to_char_type(__cb);
  }
       else
  __err |= ios_base::eofbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
 }
      if (!_M_gcount)
 __err |= ios_base::failbit;
      if (__err)
 this->setstate(__err);
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    get(char_type* __s, streamsize __n, char_type __delim)
    {
      _M_gcount = 0;
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   try
     {
       const int_type __idelim = traits_type::to_int_type(__delim);
       const int_type __eof = traits_type::eof();
       __streambuf_type* __sb = this->rdbuf();
       int_type __c = __sb->sgetc();

       while (_M_gcount + 1 < __n
       && !traits_type::eq_int_type(__c, __eof)
       && !traits_type::eq_int_type(__c, __idelim))
  {
    *__s++ = traits_type::to_char_type(__c);
    ++_M_gcount;
    __c = __sb->snextc();
  }
       if (traits_type::eq_int_type(__c, __eof))
  __err |= ios_base::eofbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
 }


      if (__n > 0)
 *__s = char_type();
      if (!_M_gcount)
 __err |= ios_base::failbit;
      if (__err)
 this->setstate(__err);
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    get(__streambuf_type& __sb, char_type __delim)
    {
      _M_gcount = 0;
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   try
     {
       const int_type __idelim = traits_type::to_int_type(__delim);
       const int_type __eof = traits_type::eof();
       __streambuf_type* __this_sb = this->rdbuf();
       int_type __c = __this_sb->sgetc();
       char_type __c2 = traits_type::to_char_type(__c);
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
       unsigned long long __gcount = 0;
#pragma GCC diagnostic pop

       while (!traits_type::eq_int_type(__c, __eof)
       && !traits_type::eq_int_type(__c, __idelim)
       && !traits_type::eq_int_type(__sb.sputc(__c2), __eof))
  {
    ++__gcount;
    __c = __this_sb->snextc();
    __c2 = traits_type::to_char_type(__c);
  }
       if (traits_type::eq_int_type(__c, __eof))
  __err |= ios_base::eofbit;


       if (__gcount <= __gnu_cxx::__numeric_traits<streamsize>::__max)
  _M_gcount = __gcount;
       else
  _M_gcount = __gnu_cxx::__numeric_traits<streamsize>::__max;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
 }
      if (!_M_gcount)
 __err |= ios_base::failbit;
      if (__err)
 this->setstate(__err);
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    getline(char_type* __s, streamsize __n, char_type __delim)
    {
      _M_gcount = 0;
      ios_base::iostate __err = ios_base::goodbit;
      sentry __cerb(*this, true);
      if (__cerb)
        {
          try
            {
              const int_type __idelim = traits_type::to_int_type(__delim);
              const int_type __eof = traits_type::eof();
              __streambuf_type* __sb = this->rdbuf();
              int_type __c = __sb->sgetc();

              while (_M_gcount + 1 < __n
                     && !traits_type::eq_int_type(__c, __eof)
                     && !traits_type::eq_int_type(__c, __idelim))
                {
                  *__s++ = traits_type::to_char_type(__c);
                  __c = __sb->snextc();
                  ++_M_gcount;
                }
              if (traits_type::eq_int_type(__c, __eof))
                __err |= ios_base::eofbit;
              else
                {
                  if (traits_type::eq_int_type(__c, __idelim))
                    {
                      __sb->sbumpc();
                      ++_M_gcount;
                    }
                  else
                    __err |= ios_base::failbit;
                }
            }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
          catch(...)
            { this->_M_setstate(ios_base::badbit); }
        }


      if (__n > 0)
 *__s = char_type();
      if (!_M_gcount)
        __err |= ios_base::failbit;
      if (__err)
        this->setstate(__err);
      return *this;
    }




  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    ignore(void)
    {
      _M_gcount = 0;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       const int_type __eof = traits_type::eof();
       __streambuf_type* __sb = this->rdbuf();

       if (traits_type::eq_int_type(__sb->sbumpc(), __eof))
  __err |= ios_base::eofbit;
       else
  _M_gcount = 1;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    ignore(streamsize __n)
    {
      _M_gcount = 0;
      sentry __cerb(*this, true);
      if (__cerb && __n > 0)
        {
          ios_base::iostate __err = ios_base::goodbit;
          try
            {
              const int_type __eof = traits_type::eof();
              __streambuf_type* __sb = this->rdbuf();
              int_type __c = __sb->sgetc();
# 553 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/istream.tcc" 3
       bool __large_ignore = false;
       while (true)
  {
    while (_M_gcount < __n
    && !traits_type::eq_int_type(__c, __eof))
      {
        ++_M_gcount;
        __c = __sb->snextc();
      }
    if (__n == __gnu_cxx::__numeric_traits<streamsize>::__max
        && !traits_type::eq_int_type(__c, __eof))
      {
        _M_gcount =
   __gnu_cxx::__numeric_traits<streamsize>::__min;
        __large_ignore = true;
      }
    else
      break;
  }

       if (__n == __gnu_cxx::__numeric_traits<streamsize>::__max)
  {
    if (__large_ignore)
      _M_gcount = __gnu_cxx::__numeric_traits<streamsize>::__max;

    if (traits_type::eq_int_type(__c, __eof))
      __err |= ios_base::eofbit;
  }
       else if (_M_gcount < __n)
  {
    if (traits_type::eq_int_type(__c, __eof))
      __err |= ios_base::eofbit;
  }
            }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
          catch(...)
            { this->_M_setstate(ios_base::badbit); }
          if (__err)
            this->setstate(__err);
        }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    ignore(streamsize __n, int_type __delim)
    {
      _M_gcount = 0;
      sentry __cerb(*this, true);
      if (__cerb && __n > 0)
        {
          ios_base::iostate __err = ios_base::goodbit;
          try
            {
              const int_type __eof = traits_type::eof();
              __streambuf_type* __sb = this->rdbuf();
              int_type __c = __sb->sgetc();


       bool __large_ignore = false;
       while (true)
  {
    while (_M_gcount < __n
    && !traits_type::eq_int_type(__c, __eof)
    && !traits_type::eq_int_type(__c, __delim))
      {
        ++_M_gcount;
        __c = __sb->snextc();
      }
    if (__n == __gnu_cxx::__numeric_traits<streamsize>::__max
        && !traits_type::eq_int_type(__c, __eof)
        && !traits_type::eq_int_type(__c, __delim))
      {
        _M_gcount =
   __gnu_cxx::__numeric_traits<streamsize>::__min;
        __large_ignore = true;
      }
    else
      break;
  }

       if (__n == __gnu_cxx::__numeric_traits<streamsize>::__max)
  {
    if (__large_ignore)
      _M_gcount = __gnu_cxx::__numeric_traits<streamsize>::__max;

    if (traits_type::eq_int_type(__c, __eof))
      __err |= ios_base::eofbit;
    else
      {
        if (_M_gcount != __n)
   ++_M_gcount;
        __sb->sbumpc();
      }
  }
       else if (_M_gcount < __n)
  {
    if (traits_type::eq_int_type(__c, __eof))
      __err |= ios_base::eofbit;
    else
      {
        ++_M_gcount;
        __sb->sbumpc();
      }
  }
            }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
          catch(...)
            { this->_M_setstate(ios_base::badbit); }
          if (__err)
            this->setstate(__err);
        }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    typename basic_istream<_CharT, _Traits>::int_type
    basic_istream<_CharT, _Traits>::
    peek(void)
    {
      int_type __c = traits_type::eof();
      _M_gcount = 0;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       __c = this->rdbuf()->sgetc();
       if (traits_type::eq_int_type(__c, traits_type::eof()))
  __err |= ios_base::eofbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return __c;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    read(char_type* __s, streamsize __n)
    {
      _M_gcount = 0;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       _M_gcount = this->rdbuf()->sgetn(__s, __n);
       if (_M_gcount != __n)
  __err |= (ios_base::eofbit | ios_base::failbit);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    streamsize
    basic_istream<_CharT, _Traits>::
    readsome(char_type* __s, streamsize __n)
    {
      _M_gcount = 0;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {

       const streamsize __num = this->rdbuf()->in_avail();
       if (__num > 0)
  _M_gcount = this->rdbuf()->sgetn(__s, std::min(__num, __n));
       else if (__num == -1)
  __err |= ios_base::eofbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return _M_gcount;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    putback(char_type __c)
    {


      _M_gcount = 0;

      this->clear(this->rdstate() & ~ios_base::eofbit);
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       const int_type __eof = traits_type::eof();
       __streambuf_type* __sb = this->rdbuf();
       if (!__sb
    || traits_type::eq_int_type(__sb->sputbackc(__c), __eof))
  __err |= ios_base::badbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    unget(void)
    {


      _M_gcount = 0;

      this->clear(this->rdstate() & ~ios_base::eofbit);
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       const int_type __eof = traits_type::eof();
       __streambuf_type* __sb = this->rdbuf();
       if (!__sb
    || traits_type::eq_int_type(__sb->sungetc(), __eof))
  __err |= ios_base::badbit;
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    int
    basic_istream<_CharT, _Traits>::
    sync(void)
    {


      int __ret = -1;
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       __streambuf_type* __sb = this->rdbuf();
       if (__sb)
  {
    if (__sb->pubsync() == -1)
      __err |= ios_base::badbit;
    else
      __ret = 0;
  }
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return __ret;
    }

  template<typename _CharT, typename _Traits>
    typename basic_istream<_CharT, _Traits>::pos_type
    basic_istream<_CharT, _Traits>::
    tellg(void)
    {


      pos_type __ret = pos_type(-1);
      sentry __cerb(*this, true);
      if (__cerb)
 {
   try
     {
       if (!this->fail())
  __ret = this->rdbuf()->pubseekoff(0, ios_base::cur,
        ios_base::in);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
 }
      return __ret;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    seekg(pos_type __pos)
    {



      this->clear(this->rdstate() & ~ios_base::eofbit);
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       if (!this->fail())
  {

    const pos_type __p = this->rdbuf()->pubseekpos(__pos,
         ios_base::in);


    if (__p == pos_type(off_type(-1)))
      __err |= ios_base::failbit;
  }
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }

  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    basic_istream<_CharT, _Traits>::
    seekg(off_type __off, ios_base::seekdir __dir)
    {



      this->clear(this->rdstate() & ~ios_base::eofbit);
      sentry __cerb(*this, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       if (!this->fail())
  {

    const pos_type __p = this->rdbuf()->pubseekoff(__off, __dir,
         ios_base::in);


    if (__p == pos_type(off_type(-1)))
      __err |= ios_base::failbit;
  }
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       this->_M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { this->_M_setstate(ios_base::badbit); }
   if (__err)
     this->setstate(__err);
 }
      return *this;
    }


  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    operator>>(basic_istream<_CharT, _Traits>& __in, _CharT& __c)
    {
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef typename __istream_type::int_type __int_type;

      typename __istream_type::sentry __cerb(__in, false);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       const __int_type __cb = __in.rdbuf()->sbumpc();
       if (!_Traits::eq_int_type(__cb, _Traits::eof()))
  __c = _Traits::to_char_type(__cb);
       else
  __err |= (ios_base::eofbit | ios_base::failbit);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __in._M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { __in._M_setstate(ios_base::badbit); }
   if (__err)
     __in.setstate(__err);
 }
      return __in;
    }

  template<typename _CharT, typename _Traits>
    void
    __istream_extract(basic_istream<_CharT, _Traits>& __in, _CharT* __s,
        streamsize __num)
    {
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef basic_streambuf<_CharT, _Traits> __streambuf_type;
      typedef typename _Traits::int_type int_type;
      typedef _CharT char_type;
      typedef ctype<_CharT> __ctype_type;

      streamsize __extracted = 0;
      ios_base::iostate __err = ios_base::goodbit;
      typename __istream_type::sentry __cerb(__in, false);
      if (__cerb)
 {
   try
     {

       streamsize __width = __in.width();
       if (0 < __width && __width < __num)
  __num = __width;

       const __ctype_type& __ct = use_facet<__ctype_type>(__in.getloc());

       const int_type __eof = _Traits::eof();
       __streambuf_type* __sb = __in.rdbuf();
       int_type __c = __sb->sgetc();

       while (__extracted < __num - 1
       && !_Traits::eq_int_type(__c, __eof)
       && !__ct.is(ctype_base::space,
     _Traits::to_char_type(__c)))
  {
    *__s++ = _Traits::to_char_type(__c);
    ++__extracted;
    __c = __sb->snextc();
  }

       if (__extracted < __num - 1
    && _Traits::eq_int_type(__c, __eof))
  __err |= ios_base::eofbit;



       *__s = char_type();
       __in.width(0);
     }
   catch(__cxxabiv1::__forced_unwind&)
     {
       __in._M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     { __in._M_setstate(ios_base::badbit); }
 }
      if (!__extracted)
 __err |= ios_base::failbit;
      if (__err)
 __in.setstate(__err);
    }


  template<typename _CharT, typename _Traits>
    basic_istream<_CharT, _Traits>&
    ws(basic_istream<_CharT, _Traits>& __in)
    {
      typedef basic_istream<_CharT, _Traits> __istream_type;
      typedef basic_streambuf<_CharT, _Traits> __streambuf_type;
      typedef typename __istream_type::int_type __int_type;
      typedef ctype<_CharT> __ctype_type;



      typename __istream_type::sentry __cerb(__in, true);
      if (__cerb)
 {
   ios_base::iostate __err = ios_base::goodbit;
   try
     {
       const __ctype_type& __ct = use_facet<__ctype_type>(__in.getloc());
       const __int_type __eof = _Traits::eof();
       __streambuf_type* __sb = __in.rdbuf();
       __int_type __c = __sb->sgetc();

       while (true)
  {
    if (_Traits::eq_int_type(__c, __eof))
      {
        __err = ios_base::eofbit;
        break;
      }
    if (!__ct.is(ctype_base::space, _Traits::to_char_type(__c)))
      break;
    __c = __sb->snextc();
  }
     }
   catch(const __cxxabiv1::__forced_unwind&)
     {
       __in._M_setstate(ios_base::badbit);
       throw;
     }
   catch(...)
     {
       __in._M_setstate(ios_base::badbit);
     }
   if (__err)
     __in.setstate(__err);
 }
      return __in;
    }




#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wc++11-extensions"
#pragma GCC diagnostic ignored "-Wlong-long"
  extern template class basic_istream<char>;
  extern template istream& ws(istream&);
  extern template istream& operator>>(istream&, char&);
  extern template istream& operator>>(istream&, unsigned char&);
  extern template istream& operator>>(istream&, signed char&);

  extern template istream& istream::_M_extract(unsigned short&);
  extern template istream& istream::_M_extract(unsigned int&);
  extern template istream& istream::_M_extract(long&);
  extern template istream& istream::_M_extract(unsigned long&);
  extern template istream& istream::_M_extract(bool&);

#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wlong-long"
  extern template istream& istream::_M_extract(long long&);
  extern template istream& istream::_M_extract(unsigned long long&);
#pragma GCC diagnostic pop

  extern template istream& istream::_M_extract(float&);
  extern template istream& istream::_M_extract(double&);
  extern template istream& istream::_M_extract(long double&);
  extern template istream& istream::_M_extract(void*&);

  extern template class basic_iostream<char>;


  extern template class basic_istream<wchar_t>;
  extern template wistream& ws(wistream&);
  extern template wistream& operator>>(wistream&, wchar_t&);
  extern template void __istream_extract(wistream&, wchar_t*, streamsize);

  extern template wistream& wistream::_M_extract(unsigned short&);
  extern template wistream& wistream::_M_extract(unsigned int&);
  extern template wistream& wistream::_M_extract(long&);
  extern template wistream& wistream::_M_extract(unsigned long&);
  extern template wistream& wistream::_M_extract(bool&);

  extern template wistream& wistream::_M_extract(long long&);
  extern template wistream& wistream::_M_extract(unsigned long long&);

  extern template wistream& wistream::_M_extract(float&);
  extern template wistream& wistream::_M_extract(double&);
  extern template wistream& wistream::_M_extract(long double&);
  extern template wistream& wistream::_M_extract(void*&);

  extern template class basic_iostream<wchar_t>;

#pragma GCC diagnostic pop



}

#pragma GCC diagnostic pop
# 1112 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/istream" 2 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iostream" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 64 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iostream" 3
  extern istream cin;
  extern ostream cout;
  extern ostream cerr;
  extern ostream clog;


  extern wistream wcin;
  extern wostream wcout;
  extern wostream wcerr;
  extern wostream wclog;
# 84 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/iostream" 3
  __extension__ __asm (".globl _ZSt21ios_base_library_initv");



}
# 11 "./tree-ssa/copyprop.C" 2
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 1 3
# 80 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdlib" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"

# 1 "/usr/include/stdlib.h" 1 3 4
# 32 "/usr/include/stdlib.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 33 "/usr/include/stdlib.h" 2 3 4

extern "C" {






# 1 "/usr/include/bits/waitflags.h" 1 3 4
# 42 "/usr/include/stdlib.h" 2 3 4
# 1 "/usr/include/bits/waitstatus.h" 1 3 4
# 66 "/usr/include/bits/waitstatus.h" 3 4
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

# 239 "/usr/include/stdlib.h" 3 4
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
# 104 "/usr/include/sys/types.h" 3 4
typedef __id_t id_t;




typedef __ssize_t ssize_t;





typedef __daddr_t daddr_t;
typedef __caddr_t caddr_t;





typedef __key_t key_t;
# 136 "/usr/include/sys/types.h" 3 4
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







# 1 "/usr/include/bits/time.h" 1 3 4
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
# 273 "/usr/include/sys/types.h" 3 4
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
  inline double
  abs(double __x)
  { return __builtin_fabs(__x); }

  inline float
  abs(float __x)
  { return __builtin_fabsf(__x); }

  inline long double
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
  div(long __i, long __j) throw() { return ldiv(__i, __j); }




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
# 12 "./tree-ssa/copyprop.C" 2






# 17 "./tree-ssa/copyprop.C"
using namespace std;
template<typename _FIter, typename _Tp> _FIter lower_bound(_FIter, _FIter, _Tp&);
template<class _Key> struct hash { };
template<class _Val> struct _Hashtable_node {
  _Hashtable_node* _M_next;
  _Val _M_val;
};
static const unsigned long __stl_prime_list[] = { 2, 3, 5 };
inline unsigned long prime(unsigned long __n) {
  const unsigned long* __first = __stl_prime_list;
  const unsigned long* __last = __stl_prime_list + 29;
  const unsigned long* pos = lower_bound(__first, __last, __n);
  return pos == __last ? *(__last - 1) : *pos;
}
template<class _Val, class _Key, class _HashFcn, class _ExtractKey, class _EqualKey, class _Alloc> struct hashtable {
  typedef _Key key_type;
  typedef _Val value_type;
  typedef _HashFcn hasher;
  typedef _EqualKey key_equal;
  typedef size_t size_type;
  typedef value_type& reference;
  typedef _Hashtable_node<_Val> _Node;
  typedef typename _Alloc::template rebind<value_type>::other allocator_type;
  allocator_type get_allocator() const { }
  typedef typename _Alloc::template rebind<_Node>::other _Node_Alloc;
  typedef typename _Alloc::template rebind<_Node*>::other _Nodeptr_Alloc;
  typedef vector<_Node*, _Nodeptr_Alloc> _Vector_type;
  _Node_Alloc _M_node_allocator;
  void _M_put_node(_Node* __p) {
    _M_node_allocator.deallocate(__p, 1);
  }
  hasher _M_hash;
  key_equal _M_equals;
  _ExtractKey _M_get_key;
  _Vector_type _M_buckets;
  size_type _M_num_elements;
  hashtable(size_type __n, const _HashFcn& __hf, const _EqualKey& __eql, const allocator_type& __a = allocator_type()) : _M_num_elements(0) {
    _M_initialize_buckets(__n);
  }
  ~hashtable() { clear(); }
  reference find_or_insert(const value_type& __obj);
  size_type count(const key_type& __key) const {
    const size_type __n = _M_bkt_num_key(__key);
    size_type __result = 0;
    for (const _Node* __cur = _M_buckets[__n]; __cur; __cur = __cur->_M_next)
      if (_M_equals(_M_get_key(__cur->_M_val), __key)) ++__result;
  }
  size_type erase(const key_type& __key);
  void clear();
  size_type _M_next_size(size_type __n) const { return prime(__n); }
  void _M_initialize_buckets(size_type __n) {
    const size_type __n_buckets = _M_next_size(__n);
    _M_buckets.reserve(__n_buckets);
    _M_buckets.insert(_M_buckets.end(), __n_buckets, (_Node*) 0);
  }
  size_type _M_bkt_num_key(const key_type& __key) const {
    return _M_bkt_num_key(__key, _M_buckets.size());
  }
  size_type _M_bkt_num_key(const key_type& __key, size_t __n) const {
    return _M_hash(__key) % __n;
  }
  void _M_delete_node(_Node* __n) {
    this->get_allocator().destroy(&__n->_M_val);
    _M_put_node(__n);
  }
};
template<class _Val, class _Key, class _HF, class _Ex, class _Eq, class _All> typename hashtable<_Val, _Key, _HF, _Ex, _Eq, _All>::size_type hashtable<_Val, _Key, _HF, _Ex, _Eq, _All>:: erase(const key_type& __key) {
  const size_type __n = _M_bkt_num_key(__key);
  _Node* __first = _M_buckets[__n];
  if (__first) _Node* __cur = __first;
}
template<class _Val, class _Key, class _HF, class _Ex, class _Eq, class _All> void hashtable<_Val, _Key, _HF, _Ex, _Eq, _All>:: clear() {
  for (size_type __i = 0; __i < _M_buckets.size(); ++__i) {
    _Node* __cur = _M_buckets[__i];
    while (__cur != 0) { _M_delete_node(__cur); }
  }
}
template<class _Key, class _Tp, class _HashFn = ::hash<_Key>, class _EqualKey = equal_to<_Key>, class _Alloc = allocator<_Tp> > struct hash_map {
  typedef hashtable<pair<const _Key, _Tp>,_Key, _HashFn, _Select1st<pair<const _Key, _Tp> >, _EqualKey, _Alloc> _Ht;
  _Ht _M_ht;
  typedef typename _Ht::key_type key_type;
  typedef typename _Ht::value_type value_type;
  typedef typename _Ht::hasher hasher;
  typedef typename _Ht::key_equal key_equal;
  typedef typename _Ht::size_type size_type;
  typedef typename _Ht::allocator_type allocator_type;
  hash_map() : _M_ht(100, hasher(), key_equal(), allocator_type()) { }
  _Tp& operator[](const key_type& __key) {
    return _M_ht.find_or_insert(value_type(__key, _Tp())).second;
  }
  size_type count(const key_type& __key) const { return _M_ht.count(__key); }
  size_type erase(const key_type& __key) {
    return _M_ht.erase(__key);
  }
};
extern size_t strlen (__const char *__s);
template <class C> struct scoped_ptr {
  explicit scoped_ptr(C* p = __null) : ptr_(p) { delete ptr_; }
  void reset(C* p = __null) {
    if (p != ptr_) { delete ptr_; }
  }
  C& operator*() const {}
  C* operator->() const {}
  bool operator==(C* p) const { return ptr_ == p; }
  bool operator!=(C* p) const { return ptr_ != p; }
  C* ptr_;
};
namespace std {
class strstreambuf : public basic_streambuf<char, char_traits<char> > {
};
class strstream : public basic_iostream<char> {
 public: int pcount() const;
  char* str();
  strstreambuf _M_buf;
};
};
const int INFO = 0, WARNING = 1, ERROR = 2, FATAL = 3, NUM_SEVERITIES = 4;
struct foo_1 {
  foo_1(string* str) : str_(str) { }
  operator bool() const {
    return (__builtin_expect(str_ != __null, 0));
  }
  string* str_;
};
template<class t1, class t2> string* Makefoo_1(const t1& v1, const t2& v2, const char* names) {
  strstream ss;
  ss << names << " (" << v1 << " vs. " << v2 << ")";
  return new string(ss.str(), ss.pcount());
}
template <class t1, class t2> inline string* Check_GTImpl(const t1& v1, const t2& v2, const char* names) {
  if (v1 > v2) return __null;
  else return Makefoo_1(v1, v2, names);
}
struct blah_54 {
  blah_54(const char* file, int line, int severity);
  ~blah_54();
  ostream& stream() { };
};
class blah_0 : public blah_54 {
 public: blah_0(const char* file, int line);
  blah_0(const char* file, int line, const foo_1& result);
};
template <class Value, class Key, class HashFcn, class ExtractKey, class EqualKey, class Alloc> class dense_hashtable;
template <class V, class K, class HF, class ExK, class EqK, class A> struct dense_hashtable_iterator {
  typedef V* pointer;
  dense_hashtable_iterator(const dense_hashtable<V,K,HF,ExK,EqK,A> *h, pointer it, pointer it_end, bool advance) : ht(h), pos(it), end(it_end) {
    if (advance) advance_past_empty_and_deleted();
  }
  pointer operator->() const { }
  void advance_past_empty_and_deleted() {
    while ( pos != end && (ht->test_empty(*this) || ht->test_deleted(*this)) ) ++pos;
  }
  const dense_hashtable<V,K,HF,ExK,EqK,A> *ht;
  pointer pos, end;
};
template <class V, class K, class HF, class ExK, class EqK, class A> struct dense_hashtable_const_iterator {
  typedef dense_hashtable_iterator<V,K,HF,ExK,EqK,A> iterator;
  typedef dense_hashtable_const_iterator<V,K,HF,ExK,EqK,A> const_iterator;
  typedef const V& reference;
  typedef const V* pointer;
  dense_hashtable_const_iterator(const dense_hashtable<V,K,HF,ExK,EqK,A> *h, pointer it, pointer it_end, bool advance) : ht(h), pos(it), end(it_end) {
    if (advance) advance_past_empty_and_deleted();
  }
  dense_hashtable_const_iterator(const iterator &it) : pos(it.pos), end(it.end) {}
  reference operator*() const { return *pos; }
  pointer operator->() const {}
  void advance_past_empty_and_deleted() {
    while ( pos != end && (ht->test_empty(*this) || ht->test_deleted(*this))) ++pos;
  }
  const_iterator& operator++() { }
  bool operator!=(const const_iterator& it) const { }
  const dense_hashtable<V,K,HF,ExK,EqK,A> *ht;
  pointer pos, end;
};
template <class Value, class Key, class HashFcn, class ExtractKey, class EqualKey, class Alloc> class dense_hashtable {
 public: typedef Key key_type;
  typedef Value value_type;
  typedef HashFcn hasher;
  typedef EqualKey key_equal;
  typedef size_t size_type;
  typedef dense_hashtable_iterator<Value, Key, HashFcn, ExtractKey, EqualKey, Alloc> iterator;
  typedef dense_hashtable_const_iterator<Value, Key, HashFcn, ExtractKey, EqualKey, Alloc> const_iterator;
  static const float HT_OCCUPANCY_FLT;
  static const float HT_EMPTY_FLT;
  static const size_t HT_MIN_BUCKETS = 32;
  iterator end() {
    return iterator(this, table + num_buckets, table + num_buckets, true);
  }
  const_iterator end() const {
    return const_iterator(this, table + num_buckets, table+num_buckets,true);
  }
  void set_value(value_type* dst, const value_type& src) {
    new(dst) value_type(src);
  }
  void destroy_buckets(size_type first, size_type last) {
    for (; first != last; ++first) table[first].~value_type();
  }
 private: void squash_deleted() {
    if ( num_deleted ) {
      dense_hashtable tmp(*this);
      swap(tmp);
    }
 }
  public: void set_deleted_key(const value_type &val) { squash_deleted(); }
  bool test_deleted(size_type bucknum) const {
    return (use_deleted && num_deleted > 0 && equals(get_key(delval), get_key(table[bucknum])));
  }
  bool test_deleted(const const_iterator &it) const {
    return (use_deleted && num_deleted > 0 && equals(get_key(delval), get_key(*it)));
  }
  bool set_deleted(const_iterator &it) {
    set_value(const_cast<value_type*>(&(*it)), delval);
  }
  bool test_empty(size_type bucknum) const {
    return equals(get_key(emptyval), get_key(table[bucknum]));
  }
  bool test_empty(const const_iterator &it) const {
    return equals(get_key(emptyval), get_key(*it));
  }
  void fill_range_with_empty(value_type* table_start, value_type* table_end) {
    uninitialized_fill(table_start, table_end, emptyval);
  }
  void set_empty(size_type buckstart, size_type buckend) {
    destroy_buckets(buckstart, buckend);
    fill_range_with_empty(table + buckstart, table + buckend);
  }
  size_type size() const {
    return num_elements - num_deleted;
  }
  size_type bucket_count() const { }
  static const size_type ILLEGAL_BUCKET = size_type(-1);
  size_type min_size(size_type num_elts, size_type min_buckets_wanted) {
    size_type sz = HT_MIN_BUCKETS;
    while ( sz < min_buckets_wanted || num_elts >= sz * enlarge_resize_percent ) sz *= 2;
  }
  void maybe_shrink() {
    if (shrink_threshold > 0 && (num_elements-num_deleted) < shrink_threshold && bucket_count() > HT_MIN_BUCKETS ) {
      size_type sz = bucket_count() / 2;
      sz /= 2;
      dense_hashtable tmp(*this, sz);
      swap(tmp);
    }
  }
  void resize_delta(size_type delta, size_type min_buckets_wanted = 0) {
    if ( consider_shrink ) maybe_shrink();
    const size_type needed_size = min_size(num_elements + delta, min_buckets_wanted);
    if ( needed_size > bucket_count() ) {
      const size_type resize_to = min_size(num_elements - num_deleted + delta, min_buckets_wanted);
      dense_hashtable tmp(*this, resize_to);
      swap(tmp);
    }
  }
  void copy_from(const dense_hashtable &ht, size_type min_buckets_wanted = 0) {
    clear();
    const size_type resize_to = min_size(ht.size(), min_buckets_wanted);
    num_elements++;
  }
  explicit dense_hashtable(size_type n = 0, const HashFcn& hf = HashFcn(), const EqualKey& eql = EqualKey(),const ExtractKey& ext = ExtractKey()) : num_deleted(0), use_deleted(false), use_empty(false), delval(), emptyval(), enlarge_resize_percent(HT_OCCUPANCY_FLT), shrink_resize_percent(HT_EMPTY_FLT), table(__null), num_buckets(min_size(0, n)), num_elements(0) {
    reset_thresholds();
   }
   dense_hashtable(const dense_hashtable& ht, size_type min_buckets_wanted = 0) : num_deleted(0), use_deleted(ht.use_deleted), use_empty(ht.use_empty), delval(ht.delval), emptyval(ht.emptyval), enlarge_resize_percent(ht.enlarge_resize_percent), shrink_resize_percent(ht.shrink_resize_percent), table(__null), num_buckets(0), num_elements(0) {
     reset_thresholds();
     copy_from(ht, min_buckets_wanted);
     set_value(&emptyval, ht.emptyval);
     enlarge_resize_percent = ht.enlarge_resize_percent;
     copy_from(ht);
   }
  ~dense_hashtable() {
    if (table) {
      destroy_buckets(0, num_buckets);
      free(table);
    }
  }
  void swap(dense_hashtable& ht) {
    std::swap(equals, ht.equals);
    {
      value_type tmp;
      set_value(&delval, ht.delval);
      set_value(&ht.delval, tmp);
      set_value(&ht.emptyval, tmp);
    }
    std::swap(table, ht.table);
    std::swap(num_buckets, ht.num_buckets);
    reset_thresholds();
    ht.reset_thresholds();
  }
  void clear() {
    if (table) destroy_buckets(0, num_buckets);
    num_buckets = min_size(0,0);
    set_empty(0, num_buckets);
  }
  pair<size_type, size_type> find_position(const key_type &key) const {
    const size_type bucket_count_minus_one = bucket_count() - 1;
    size_type bucknum = hash(key) & bucket_count_minus_one;
    size_type insert_pos = ILLEGAL_BUCKET;
    while ( 1 ) {
      if ( test_empty(bucknum) ) {
        if ( insert_pos == ILLEGAL_BUCKET ) return pair<size_type,size_type>(ILLEGAL_BUCKET, insert_pos);
      }
      else if ( test_deleted(bucknum) ) {
        if ( insert_pos == ILLEGAL_BUCKET ) insert_pos = bucknum;
      }
      else if ( equals(key, get_key(table[bucknum])) ) {
        return pair<size_type,size_type>(bucknum, ILLEGAL_BUCKET);
      }
    }
  }
  iterator find(const key_type& key) {
    if ( size() == 0 ) return end();
    pair<size_type, size_type> pos = find_position(key);
    if ( pos.first == ILLEGAL_BUCKET ) return end();
    return iterator(this, table + pos.first, table + num_buckets, false);
  }
  const_iterator find(const key_type& key) const {
    if ( size() == 0 ) return end();
    pair<size_type, size_type> pos = find_position(key);
    if ( pos.first == ILLEGAL_BUCKET ) return end();
    return const_iterator(this, table + pos.first, table+num_buckets, false);
  }
  size_type count(const key_type &key) const {
    pair<size_type, size_type> pos = find_position(key); }
  pair<iterator, bool> insert_noresize(const value_type& obj) {
    const pair<size_type,size_type> pos = find_position(get_key(obj));
    if ( pos.first != ILLEGAL_BUCKET) {
      return pair<iterator,bool>(iterator(this, table + pos.first, table + num_buckets, false), false);
    }
    else {
      if ( test_deleted(pos.second) ) { ++num_elements; }
      return pair<iterator,bool>(iterator(this, table + pos.second, table + num_buckets, false), true);
    }
  }
  pair<iterator, bool> insert(const value_type& obj) {
    resize_delta(1);
    return insert_noresize(obj);
  }
  size_type erase(const key_type& key) {
    const_iterator pos = find(key);
    if ( pos != end() ) {
      set_deleted(pos);
    }
  }
  hasher hash;
  key_equal equals;
  ExtractKey get_key;
  size_type num_deleted;
  bool use_deleted;
  bool use_empty;
  value_type delval;
  value_type emptyval;
  float enlarge_resize_percent;
  float shrink_resize_percent;
  size_type shrink_threshold;
  size_type enlarge_threshold;
  value_type *table;
  size_type num_buckets;
  size_type num_elements;
  bool consider_shrink;
  void reset_thresholds() {
    enlarge_threshold = static_cast<size_type>(num_buckets * shrink_resize_percent);
  }
};
template<> struct hash<long> {
  size_t operator()(long x) const {
  }
};
template<> struct hash<unsigned long> {
  size_t operator()(unsigned long x) const {
  }
};
template <class Key, class T, class HashFcn = hash<Key>, class EqualKey = equal_to<Key>, class Alloc = allocator<T> > class dense_hash_map {
  struct SelectKey {
    const Key& operator()(const pair<const Key, T>& p) const {
      return p.first;
    }
  };
  typedef dense_hashtable<pair<const Key, T>, Key, HashFcn, SelectKey, EqualKey, Alloc> ht;
  ht rep;
 public: typedef typename ht::key_type key_type;
  typedef T data_type;
  typedef typename ht::value_type value_type;
  typedef typename ht::size_type size_type;
  typedef typename ht::iterator iterator;
  typedef typename ht::const_iterator const_iterator;
  iterator end() {
    return rep.end();
  }
  iterator find(const key_type& key) { return rep.find(key); }
  data_type& operator[](const key_type& key) {
    iterator it = find(key);
    return insert(value_type(key, data_type())).first->second;
  }
  pair<iterator, bool> insert(const value_type& obj) {
    return rep.insert(obj);
  }
  void set_deleted_key(const key_type& key) {
    rep.set_deleted_key(value_type(key, data_type()));
  }
  size_type erase(const key_type& key) { return rep.erase(key); }
};
template <class Value, class HashFcn = hash<Value>, class EqualKey = equal_to<Value>, class Alloc = allocator<Value> > class dense_hash_set {
  struct Identity {
    const Value& operator()(const Value& v) const { return v; }
  };
  typedef dense_hashtable<Value, Value, HashFcn, Identity, EqualKey, Alloc> ht;
  ht rep;
 public: typedef typename ht::key_type key_type;
  typedef typename ht::value_type value_type;
  typedef typename ht::size_type size_type;
  typedef typename ht::const_iterator iterator;
  size_type count(const key_type& key) const {
    return rep.count(key);
  }
  pair<iterator, bool> insert(const value_type& obj) {
    pair<typename ht::iterator, bool> p = rep.insert(obj);
  }
  size_type erase(const key_type& key) {
    return rep.erase(key);
  }
};
class linked_ptr_internal {
 public: bool depart() { if (next_ == this) return true; }
  mutable linked_ptr_internal const* next_;
};
template <typename T> class linked_ptr {
 public: explicit linked_ptr(T* ptr = __null) {
 }
  ~linked_ptr() { depart(); }
  T& operator*() const { }
  T* value_;
  linked_ptr_internal link_;
  void depart() {
    if (link_.depart()) delete value_;
  }
};
class blah_3 {
  const char* ptr_;
  int length_;
 public: blah_3(const char* str) : ptr_(str), length_((str == __null) ? 0 : static_cast<int>(strlen(str))) { }
};
class blah_5;
class Bitmap {
 public: Bitmap(unsigned int size) : array_size_(RequiredArraySize(size)) { }
  static unsigned int RequiredArraySize(unsigned int num_bits) { return (num_bits + 31) >> 5; }
   unsigned int array_size_;
};
enum blah_31 { CREATIVE_FORMAT_TEXT_NARROW, kNumblah_31s };
enum blah_33 { BLACKLISTED };
template <typename EnumT> class blah_55;
typedef blah_55<blah_31> blah_31Set;
enum blah_36 { APPROVAL_STATUS_APPROVED, APPROVAL_STATUS_UNKNOWN };
enum blah_37 { hahah_INVALID, hahah_KEYWORD };
template<typename EnumT> class blah_55 {
 public: blah_55(int enum_size);
  bool Insert(EnumT x);
  const int enum_size_;
  Bitmap elements_;
};
template<typename EnumT> blah_55<EnumT>::blah_55(int enum_size) :enum_size_(enum_size), elements_(enum_size) {
  while (foo_1 _result = Check_GTImpl(1, 0, "enum_size" " " ">" " " "0")) blah_0(".h", 1902, _result).stream();
};
enum blah_38 {
  ttttttt_9, };
class blah_46 {
 public: blah_46() : hahaha_id_(0), type_(hahah_INVALID), approval_status_(APPROVAL_STATUS_APPROVED) {
 }
  blah_46(long cid) : hahaha_id_(cid), type_(hahah_INVALID), approval_status_(APPROVAL_STATUS_APPROVED) {
  }
  long id() const {
    return (static_cast<long>(hahaha_id_) << 16) >> 16;
  }
  static const blah_46 kBlacklistedID;
  bool operator == (const blah_46& x) const { return id() == x.id(); }
  bool operator < (const blah_46& x) const { return id() < x.id(); }
  long hahaha_id_ : 48;
  blah_37 type_ : 8;
  blah_36 approval_status_ : 4;
};
template <> struct hash<blah_46> {
  size_t operator()(const blah_46 &x) const {
    return size_t(x.id());
  }
};
class blah_57 {
 public: blah_57();
  void AddReason(blah_33 reason, const blah_3& debug_str, const blah_46& hahaha_id, bool );
  void set_collects_multiple_reasons(bool t) { }
 private: struct foo_3 {
   string reject_desc;
 };
  foo_3 first_reason_;
};
template <class T> struct foo_5 : public unary_function<T*, long> {
  long operator()(const T* p) const {
    long id = reinterpret_cast<long>(p);
    if (id < 2) return -id;
  }
};
template <class T> class DensePtrSet : public dense_hashtable<T*, long, hash<long>, foo_5<T>, equal_to<long>, allocator<T*> > {
 public: DensePtrSet() {
   this->set_deleted_key(reinterpret_cast<T*>(1));
 }
  const T* Find(long key) const {
    typename DensePtrSet<T>::const_iterator it = this->find(key);
    return it != this->end() ? *it : __null;
  }
};
struct foo_7 {
  foo_7(bool spell_correction, bool query_broadening, bool previous_query, bool near_aaaaa, bool same_length, float mult, float exp_score) : shengmo_0(spell_correction), shengmo_1(query_broadening), shengmo_2(previous_query), shengmo_3(near_aaaaa), shengmo_4(same_length), multiplier(mult), expansion_score(exp_score) {
  }
  int CompareSameKeywordMatch(const foo_7& compare) const;
  bool shengmo_0, shengmo_1, shengmo_2, shengmo_3, shengmo_4;
  float multiplier, expansion_score;
};
enum blah_41 {
  ACP_ECPM_EARLY = 2 };
struct foo_8 { unsigned int packed_ctr1; };
struct foo_9 { foo_9() {}};
class blah_16;
class blah_17;
class foo_12 { public: foo_12() {}
  unsigned long hahaha_id() const {}
  unsigned int qbb_score() const {}
 private: static const vector<blah_46> hmmmmh_4;
  long hahaha_id_ : 40;
};
class foo_13 {
 public: typedef dense_hash_map<long, int> BestMap;
  foo_13() { best_rrrrrrr_.set_deleted_key(-1); }
  void erase(long ad_group_id) {
    best_rrrrrrr_.erase(ad_group_id);
  }
  typedef BestMap::iterator iterator;
  typedef BestMap::const_iterator const_iterator;
  const_iterator begin() const { }
  iterator end() { return best_rrrrrrr_.end(); }
  iterator find(long ad_group_id) { return best_rrrrrrr_.find(ad_group_id); }
   const foo_12& GetMatch(const_iterator it) const {}
  void hmmmmh_27(long ad_group_id, const foo_12& addme);
 private: BestMap best_rrrrrrr_;
  vector<foo_12> rrrrrrr_buffer_;
};
struct foo_10 : public dense_hash_set<blah_46> {};
class foo_9Set : public DensePtrSet<foo_9> {};
typedef map<blah_46, foo_7*> foo_6Data;
typedef hash_map<long, linked_ptr<blah_57> > RejectedAdGroupMap;
enum blah_43 {};
class foo_14 {
 public: foo_14(const unsigned int, const blah_16*, const int*);
  bool GathersMultipleRejectionReasons() const;
  void hmmmmh_30(blah_46 hahaha_id, blah_38 type);
  const foo_7* Insertfoo_6(const blah_46 hahaha_id, bool shengmo_0, bool shengmo_1, bool shengmo_2, bool shengmo_3, bool shengmo_4_rewrite, float multiplier, float context_score);
  void hmmmmh_7(blah_46 hahaha_id, blah_38 type);
  foo_9* Insertfoo_9();
  bool hmmmmh_8(long ad_group_id, const foo_12 &entry);
  void hmmmmh_9(long ad_group_id);
  foo_13::iterator hmmmmh_0(long ad_group_id);
  bool hmmmmh_8(long ad_group_id, foo_13::iterator best, const foo_12& entry);
  void hmmmmh_5(const blah_46 hahaha_id);
  void hmmmmh_29(const blah_46 hahaha_id);
  bool hmmmmh_12(const blah_46 hahaha_id) const;
  bool hmmmmh_13(const blah_46 hahaha_id) const;
  const foo_9* Getfoo_9(const blah_46 hahaha_id) const;
  bool Gathersfoo_9() const {}
  const foo_10* rrrrrrr_type_data() const {}
  const foo_10* negative_rrrrrrr_type_data() const {}
  const foo_10* positive_rrrrrrr_type_data() const {}
  const foo_9Set* kw_info_set() const { }
   const foo_6Data* rewrite_data() const {}
  const vector<blah_17>& query_rectangles() const {}
   void hmmmmh_14();
   void AddQueryRectangle(const blah_17& query_rectangle);
   void hmmmmh_15(long ad_group_id, const blah_46 hahaha_id, blah_33 reject_class, const char* reject_desc = __null);
   void hmmmmh_16(const vector<long>& rejected_sssr_ids);
   void Copy(const foo_14& cmi);
   void hmmmmh_10();
 private: const blah_16* ad_request_;
  const int* cr_query_;
  blah_43 gather_flags_;
  vector<blah_17> query_rectangles_;
  foo_10 rrrrrrr_type_data_;
  foo_9Set kw_info_set_;
  foo_6Data rewrite_data_;
  scoped_ptr<RejectedAdGroupMap> rejected_sssr_map_;
  foo_13 ad_group_rrrrrrr_data_;
  vector<blah_46> geo_hahaha_;
  bool geo_hahaha_is_sorted_;
  foo_10 negative_rrrrrrr_type_data_, positive_rrrrrrr_type_data_;
  scoped_ptr<foo_10> extra_hahaha_set_;
  int dimension_id_;
  blah_31Set creative_formats_;
  scoped_ptr<dense_hash_set<unsigned long> > near_aaaaa_rrrrrrr_fps_;
  blah_41 comparison_policy_;
  blah_46 next_virtual_hahaha_id_;
  vector<void*>* sub_queries_;
  bool allow_only_whitelisted_customers_, automatic_hahaha_rrrrrrr_;
  scoped_ptr<blah_5> kw_arena_, expanded_rrrrrrr_arena_;
};
class blah_19 {
  void hmmmmh_3();
  enum blah_45 {};
};
void blah_19::hmmmmh_3() {}
class blah_16 {
 public: int near_aaaaa_rrrrrrr_fps_size() const {}
  unsigned long near_aaaaa_rrrrrrr_fps(int i) const {}
};
class blah_21 {
 protected: blah_21(char* first_block, const size_t block_size, bool align_to_page);
  void* GetMemoryFallback(const size_t size, const int align);
  void* GetMemory(const size_t size, const int align) {
    if ( size > 0 && size < remaining_ && align == 1 ) {
      last_alloc_ = freestart_;
    }
    return GetMemoryFallback(size, align);
  }
  char* freestart_;
  char* last_alloc_;
  size_t remaining_;
};
class blah_5 : blah_21 {
 public: char* Alloc(const size_t size) {
   return reinterpret_cast<char*>(GetMemory(size, 1));
 }
};
class blah_25 {
 public: virtual ~blah_25();
};
class blah_17 : blah_25 { };
void Fillfoo_8(const foo_12& x2, struct foo_8* out) {
  out->packed_ctr1 = x2.qbb_score();
}
const vector<blah_46> foo_12::hmmmmh_4;
foo_14::foo_14(const unsigned int gather_flags, const blah_16* ad_request, const int* cr_query): ad_request_(ad_request), cr_query_(cr_query), gather_flags_(static_cast<blah_43>(gather_flags)), geo_hahaha_is_sorted_(false), dimension_id_(0), creative_formats_(kNumblah_31s), comparison_policy_(ACP_ECPM_EARLY), sub_queries_(new vector<void*>()), allow_only_whitelisted_customers_(false), automatic_hahaha_rrrrrrr_(false) {
  hmmmmh_10();
}
void foo_14::hmmmmh_5(const blah_46 hahaha_id) {
  negative_rrrrrrr_type_data_.insert(hahaha_id);
}
void foo_14::hmmmmh_7(blah_46 hahaha_id, blah_38 type) { }
foo_13::iterator foo_14::hmmmmh_0( long ad_group_id) {
  return ad_group_rrrrrrr_data_.find(ad_group_id);
}
bool foo_14::hmmmmh_8(long ad_group_id, foo_13::iterator best, const foo_12& entry) {
  rejected_sssr_map_->erase(ad_group_id);
  ad_group_rrrrrrr_data_.hmmmmh_27(ad_group_id, entry);
}
bool foo_14::hmmmmh_8(long ad_group_id, const foo_12& entry) {
  foo_13::iterator best = hmmmmh_0(ad_group_id);
}
void foo_14::hmmmmh_9(long ad_group_id) {
  ad_group_rrrrrrr_data_.erase(ad_group_id);
}
void foo_14::hmmmmh_10() {
  if (near_aaaaa_rrrrrrr_fps_ != __null) {
    blah_54(".cc", 226, WARNING).stream() << "";
    for (int j = 0;
         j < ad_request_->near_aaaaa_rrrrrrr_fps_size(); j++) {
      near_aaaaa_rrrrrrr_fps_->insert(ad_request_->near_aaaaa_rrrrrrr_fps(j));
    }
  }
}
const foo_7* foo_14::Insertfoo_6(const blah_46 hahaha_id, bool shengmo_0, bool shengmo_1, bool shengmo_2, bool shengmo_3, bool shengmo_4_rewrite, float multiplier, float context_score) {
  if (rrrrrrr_type_data_.count(hahaha_id) > 0) return __null;
  foo_7* new_info = new(expanded_rrrrrrr_arena_->Alloc(sizeof(foo_7))) foo_7(shengmo_0,shengmo_1, shengmo_2, shengmo_3, shengmo_4_rewrite, multiplier, context_score);
  pair<foo_6Data::iterator, bool> status = rewrite_data_.insert( make_pair(hahaha_id, new_info));
  foo_7* inserted = status.first->second;
  if (!status.second) {
    if (inserted->CompareSameKeywordMatch(*new_info) < 0) *inserted = *new_info;
  }
}
foo_9* foo_14::Insertfoo_9() {
  foo_9* info = new(kw_arena_->Alloc(sizeof(foo_9))) foo_9;
  if (Gathersfoo_9()) kw_info_set_.insert(info);
  creative_formats_.Insert(CREATIVE_FORMAT_TEXT_NARROW);
}
bool foo_14::hmmmmh_12(const blah_46 hahaha_id) const {
  if (rrrrrrr_type_data_.count(hahaha_id)) return true;
}
bool foo_14::hmmmmh_13(const blah_46 hahaha_id) const {
  if (positive_rrrrrrr_type_data_.count(hahaha_id)) return true;
}
const foo_9* foo_14::Getfoo_9(const blah_46 hahaha_id) const {
  if (Gathersfoo_9()) return kw_info_set_.Find(hahaha_id.id());
  static int occurrences_383 = 0, occurrences_mod_n_383 = 0;
  if (++occurrences_mod_n_383 > 1000) occurrences_mod_n_383 -= 1000;
}
void foo_14::hmmmmh_15(long ad_group_id, const blah_46 hahaha_id, blah_33 reject_class, const char* reject_desc) {
  if (rejected_sssr_map_ == __null) {
    blah_54("a.cc", 413, ERROR).stream() << "re NULL";
    rejected_sssr_map_.reset(new RejectedAdGroupMap);
  }
  if (rejected_sssr_map_->count(ad_group_id) == 0) {
    blah_57* ad_rejection = new blah_57();
    ad_rejection->set_collects_multiple_reasons( GathersMultipleRejectionReasons());
    (*rejected_sssr_map_)[ad_group_id] = linked_ptr<blah_57>(ad_rejection);
  }
  blah_57& ad_rejection = *(*rejected_sssr_map_)[ad_group_id];
  ad_rejection.AddReason(reject_class, reject_desc, hahaha_id, false);
}
void foo_14::hmmmmh_16(const vector<long>& rejected_sssr_ids) {
  for (vector<long>::const_iterator it = rejected_sssr_ids.begin();
       it != rejected_sssr_ids.end(); ++it) {
    ad_group_rrrrrrr_data_.erase(*it);
    for (foo_13::const_iterator it = ad_group_rrrrrrr_data_.begin();
         it != ad_group_rrrrrrr_data_.end(); ++it) {
      hmmmmh_15(it->first, ad_group_rrrrrrr_data_.GetMatch(it).hahaha_id(), BLACKLISTED);
    }
  }
  hmmmmh_30(blah_46::kBlacklistedID, ttttttt_9);
}
void foo_14::Copy(const foo_14& cmi) {
  rrrrrrr_type_data_ = *cmi.rrrrrrr_type_data();
  negative_rrrrrrr_type_data_ = *cmi.negative_rrrrrrr_type_data();
  positive_rrrrrrr_type_data_ = *cmi.positive_rrrrrrr_type_data();
  if (cmi.Gathersfoo_9()) {
    kw_info_set_ = *cmi.kw_info_set();
    rewrite_data_ = *cmi.rewrite_data();
  }
  hmmmmh_14();
  for (int i = 0; i < cmi.query_rectangles().size();
       ++i) AddQueryRectangle(cmi.query_rectangles()[i]);
}
void foo_13::hmmmmh_27(long ad_group_id, const foo_12& addme) {
  int& best_index = best_rrrrrrr_[ad_group_id];
  rrrrrrr_buffer_.push_back(addme);
}
void foo_14::hmmmmh_29(const blah_46 hahaha_id) {
  if (extra_hahaha_set_ != __null) extra_hahaha_set_->erase(hahaha_id);
}
