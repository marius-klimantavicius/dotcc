//type: fp
//options: --c++14
# 0 "./diagnostic/pr65923.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./diagnostic/pr65923.C"



# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 1 3
# 45 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 1 3
# 39 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 1 3
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 1 3
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 3
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
# 41 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/type_traits" 2 3
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
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 2 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 3
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 1 3 4
# 9 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 3 4
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/usr/include/stdint.h" 1 3 4
# 26 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4
# 36 "/usr/include/stdint.h" 3 4
typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 12 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stdint.h" 2 3 4
#pragma GCC diagnostic pop
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 2 3


namespace std
{

  using ::int8_t;
  using ::int16_t;
  using ::int32_t;
  using ::int64_t;

  using ::int_fast8_t;
  using ::int_fast16_t;
  using ::int_fast32_t;
  using ::int_fast64_t;

  using ::int_least8_t;
  using ::int_least16_t;
  using ::int_least32_t;
  using ::int_least64_t;

  using ::intmax_t;
  using ::intptr_t;

  using ::uint8_t;
  using ::uint16_t;
  using ::uint32_t;
  using ::uint64_t;

  using ::uint_fast8_t;
  using ::uint_fast16_t;
  using ::uint_fast32_t;
  using ::uint_fast64_t;

  using ::uint_least8_t;
  using ::uint_least16_t;
  using ::uint_least32_t;
  using ::uint_least64_t;

  using ::uintmax_t;
  using ::uintptr_t;
# 144 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/cstdint" 3
}
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 2 3


# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 3
  template<intmax_t _Pn>
    struct __static_sign
    : integral_constant<intmax_t, (_Pn < 0) ? -1 : 1>
    { };

  template<intmax_t _Pn>
    struct __static_abs
    : integral_constant<intmax_t, _Pn * __static_sign<_Pn>::value>
    { };

  template<intmax_t _Pn, intmax_t _Qn>
    struct __static_gcd
    : __static_gcd<_Qn, (_Pn % _Qn)>
    { };

  template<intmax_t _Pn>
    struct __static_gcd<_Pn, 0>
    : integral_constant<intmax_t, __static_abs<_Pn>::value>
    { };

  template<intmax_t _Qn>
    struct __static_gcd<0, _Qn>
    : integral_constant<intmax_t, __static_abs<_Qn>::value>
    { };







  template<intmax_t _Pn, intmax_t _Qn>
    struct __safe_multiply
    {
    private:
      static const uintmax_t __c = uintmax_t(1) << (sizeof(intmax_t) * 4);

      static const uintmax_t __a0 = __static_abs<_Pn>::value % __c;
      static const uintmax_t __a1 = __static_abs<_Pn>::value / __c;
      static const uintmax_t __b0 = __static_abs<_Qn>::value % __c;
      static const uintmax_t __b1 = __static_abs<_Qn>::value / __c;

      static_assert(__a1 == 0 || __b1 == 0,
      "overflow in multiplication");
      static_assert(__a0 * __b1 + __b0 * __a1 < (__c >> 1),
      "overflow in multiplication");
      static_assert(__b0 * __a0 <= 0x7fffffffffffffffL,
      "overflow in multiplication");
      static_assert((__a0 * __b1 + __b0 * __a1) * __c
      <= 0x7fffffffffffffffL - __b0 * __a0,
      "overflow in multiplication");

    public:
      static const intmax_t value = _Pn * _Qn;
    };



  template<uintmax_t __hi1, uintmax_t __lo1, uintmax_t __hi2, uintmax_t __lo2>
    struct __big_less
    : integral_constant<bool, (__hi1 < __hi2
          || (__hi1 == __hi2 && __lo1 < __lo2))>
    { };

  template<uintmax_t __hi1, uintmax_t __lo1, uintmax_t __hi2, uintmax_t __lo2>
    struct __big_add
    {
      static constexpr uintmax_t __lo = __lo1 + __lo2;
      static constexpr uintmax_t __hi = (__hi1 + __hi2 +
      (__lo1 + __lo2 < __lo1));
    };


  template<uintmax_t __hi1, uintmax_t __lo1, uintmax_t __hi2, uintmax_t __lo2>
    struct __big_sub
    {
      static_assert(!__big_less<__hi1, __lo1, __hi2, __lo2>::value,
      "Internal library error");
      static constexpr uintmax_t __lo = __lo1 - __lo2;
      static constexpr uintmax_t __hi = (__hi1 - __hi2 -
      (__lo1 < __lo2));
    };


  template<uintmax_t __x, uintmax_t __y>
    struct __big_mul
    {
    private:
      static constexpr uintmax_t __c = uintmax_t(1) << (sizeof(intmax_t) * 4);
      static constexpr uintmax_t __x0 = __x % __c;
      static constexpr uintmax_t __x1 = __x / __c;
      static constexpr uintmax_t __y0 = __y % __c;
      static constexpr uintmax_t __y1 = __y / __c;
      static constexpr uintmax_t __x0y0 = __x0 * __y0;
      static constexpr uintmax_t __x0y1 = __x0 * __y1;
      static constexpr uintmax_t __x1y0 = __x1 * __y0;
      static constexpr uintmax_t __x1y1 = __x1 * __y1;
      static constexpr uintmax_t __mix = __x0y1 + __x1y0;
      static constexpr uintmax_t __mix_lo = __mix * __c;
      static constexpr uintmax_t __mix_hi
      = __mix / __c + ((__mix < __x0y1) ? __c : 0);
      typedef __big_add<__mix_hi, __mix_lo, __x1y1, __x0y0> _Res;
    public:
      static constexpr uintmax_t __hi = _Res::__hi;
      static constexpr uintmax_t __lo = _Res::__lo;
    };



  template<uintmax_t __n1, uintmax_t __n0, uintmax_t __d>
    struct __big_div_impl
    {
    private:
      static_assert(__d >= (uintmax_t(1) << (sizeof(intmax_t) * 8 - 1)),
      "Internal library error");
      static_assert(__n1 < __d, "Internal library error");
      static constexpr uintmax_t __c = uintmax_t(1) << (sizeof(intmax_t) * 4);
      static constexpr uintmax_t __d1 = __d / __c;
      static constexpr uintmax_t __d0 = __d % __c;

      static constexpr uintmax_t __q1x = __n1 / __d1;
      static constexpr uintmax_t __r1x = __n1 % __d1;
      static constexpr uintmax_t __m = __q1x * __d0;
      static constexpr uintmax_t __r1y = __r1x * __c + __n0 / __c;
      static constexpr uintmax_t __r1z = __r1y + __d;
      static constexpr uintmax_t __r1
      = ((__r1y < __m) ? ((__r1z >= __d) && (__r1z < __m))
  ? (__r1z + __d) : __r1z : __r1y) - __m;
      static constexpr uintmax_t __q1
      = __q1x - ((__r1y < __m)
   ? ((__r1z >= __d) && (__r1z < __m)) ? 2 : 1 : 0);
      static constexpr uintmax_t __q0x = __r1 / __d1;
      static constexpr uintmax_t __r0x = __r1 % __d1;
      static constexpr uintmax_t __n = __q0x * __d0;
      static constexpr uintmax_t __r0y = __r0x * __c + __n0 % __c;
      static constexpr uintmax_t __r0z = __r0y + __d;
      static constexpr uintmax_t __r0
      = ((__r0y < __n) ? ((__r0z >= __d) && (__r0z < __n))
  ? (__r0z + __d) : __r0z : __r0y) - __n;
      static constexpr uintmax_t __q0
      = __q0x - ((__r0y < __n) ? ((__r0z >= __d)
      && (__r0z < __n)) ? 2 : 1 : 0);

    public:
      static constexpr uintmax_t __quot = __q1 * __c + __q0;
      static constexpr uintmax_t __rem = __r0;

    private:
      typedef __big_mul<__quot, __d> _Prod;
      typedef __big_add<_Prod::__hi, _Prod::__lo, 0, __rem> _Sum;
      static_assert(_Sum::__hi == __n1 && _Sum::__lo == __n0,
      "Internal library error");
  };

  template<uintmax_t __n1, uintmax_t __n0, uintmax_t __d>
    struct __big_div
    {
    private:
      static_assert(__d != 0, "Internal library error");
      static_assert(sizeof (uintmax_t) == sizeof (unsigned long long),
      "This library calls __builtin_clzll on uintmax_t, which "
      "is unsafe on your platform. Please complain to "
      "http://gcc.gnu.org/bugzilla/");
      static constexpr int __shift = __builtin_clzll(__d);
      static constexpr int __coshift_ = sizeof(uintmax_t) * 8 - __shift;
      static constexpr int __coshift = (__shift != 0) ? __coshift_ : 0;
      static constexpr uintmax_t __c1 = uintmax_t(1) << __shift;
      static constexpr uintmax_t __c2 = uintmax_t(1) << __coshift;
      static constexpr uintmax_t __new_d = __d * __c1;
      static constexpr uintmax_t __new_n0 = __n0 * __c1;
      static constexpr uintmax_t __n1_shifted = (__n1 % __d) * __c1;
      static constexpr uintmax_t __n0_top = (__shift != 0) ? (__n0 / __c2) : 0;
      static constexpr uintmax_t __new_n1 = __n1_shifted + __n0_top;
      typedef __big_div_impl<__new_n1, __new_n0, __new_d> _Res;

    public:
      static constexpr uintmax_t __quot_hi = __n1 / __d;
      static constexpr uintmax_t __quot_lo = _Res::__quot;
      static constexpr uintmax_t __rem = _Res::__rem / __c1;

    private:
      typedef __big_mul<__quot_lo, __d> _P0;
      typedef __big_mul<__quot_hi, __d> _P1;
      typedef __big_add<_P0::__hi, _P0::__lo, _P1::__lo, __rem> _Sum;

      static_assert(_P1::__hi == 0, "Internal library error");
      static_assert(_Sum::__hi >= _P0::__hi, "Internal library error");

      static_assert(_Sum::__hi == __n1 && _Sum::__lo == __n0,
      "Internal library error");
      static_assert(__rem < __d, "Internal library error");
    };
# 270 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 3
  template<intmax_t _Num, intmax_t _Den = 1>
    struct ratio
    {
      static_assert(_Den != 0, "denominator cannot be zero");
      static_assert(_Num >= -0x7fffffffffffffffL && _Den >= -0x7fffffffffffffffL,
      "out of range");


      static constexpr intmax_t num =
        _Num * __static_sign<_Den>::value / __static_gcd<_Num, _Den>::value;

      static constexpr intmax_t den =
        __static_abs<_Den>::value / __static_gcd<_Num, _Den>::value;

      typedef ratio<num, den> type;
    };


  template<intmax_t _Num, intmax_t _Den>
    constexpr intmax_t ratio<_Num, _Den>::num;

  template<intmax_t _Num, intmax_t _Den>
    constexpr intmax_t ratio<_Num, _Den>::den;




  template<typename _Tp>
    struct __is_ratio
    : std::false_type
    { };

  template<intmax_t _Num, intmax_t _Den>
    struct __is_ratio<ratio<_Num, _Den>>
    : std::true_type
    { };


  template<typename _Tp>
    constexpr bool __is_ratio_v = false;
  template<intmax_t _Num, intmax_t _Den>
    constexpr bool __is_ratio_v<ratio<_Num, _Den>> = true;


  template<typename _R1, typename _R2>
    constexpr bool
    __are_both_ratios() noexcept
    {






      return __and_<__is_ratio<_R1>, __is_ratio<_R2>>::value;

    }

  template<typename _R1, typename _R2>
    struct __ratio_multiply
    {
      static_assert(std::__are_both_ratios<_R1, _R2>(),
      "both template arguments must be a std::ratio");

    private:
      static const intmax_t __gcd1 =
        __static_gcd<_R1::num, _R2::den>::value;
      static const intmax_t __gcd2 =
        __static_gcd<_R2::num, _R1::den>::value;

    public:
      typedef ratio<
        __safe_multiply<(_R1::num / __gcd1),
                        (_R2::num / __gcd2)>::value,
        __safe_multiply<(_R1::den / __gcd2),
                        (_R2::den / __gcd1)>::value> type;

      static constexpr intmax_t num = type::num;
      static constexpr intmax_t den = type::den;
    };


  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_multiply<_R1, _R2>::num;

  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_multiply<_R1, _R2>::den;





  template<typename _R1, typename _R2>
    using ratio_multiply = typename __ratio_multiply<_R1, _R2>::type;



  template<typename _R1, typename _R2>
    struct __ratio_divide
    {
      static_assert(_R2::num != 0, "division by 0");

      typedef typename __ratio_multiply<
        _R1,
        ratio<_R2::den, _R2::num>>::type type;

      static constexpr intmax_t num = type::num;
      static constexpr intmax_t den = type::den;
    };


  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_divide<_R1, _R2>::num;

  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_divide<_R1, _R2>::den;





  template<typename _R1, typename _R2>
    using ratio_divide = typename __ratio_divide<_R1, _R2>::type;


  template<typename _R1, typename _R2>
    struct ratio_equal
    : integral_constant<bool, _R1::num == _R2::num && _R1::den == _R2::den>
    {
      static_assert(std::__are_both_ratios<_R1, _R2>(),
      "both template arguments must be a std::ratio");
    };


  template<typename _R1, typename _R2>
    struct ratio_not_equal
    : integral_constant<bool, !ratio_equal<_R1, _R2>::value>
    { };




  template<typename _R1, typename _R2,
           typename _Left = __big_mul<_R1::num,_R2::den>,
           typename _Right = __big_mul<_R2::num,_R1::den> >
    struct __ratio_less_impl_1
    : integral_constant<bool, __big_less<_Left::__hi, _Left::__lo,
           _Right::__hi, _Right::__lo>::value>
    { };

  template<typename _R1, typename _R2,
    bool = (_R1::num == 0 || _R2::num == 0
     || (__static_sign<_R1::num>::value
         != __static_sign<_R2::num>::value)),
    bool = (__static_sign<_R1::num>::value == -1
     && __static_sign<_R2::num>::value == -1)>
    struct __ratio_less_impl
    : __ratio_less_impl_1<_R1, _R2>::type
    { };

  template<typename _R1, typename _R2>
    struct __ratio_less_impl<_R1, _R2, true, false>
    : integral_constant<bool, _R1::num < _R2::num>
    { };

  template<typename _R1, typename _R2>
    struct __ratio_less_impl<_R1, _R2, false, true>
    : __ratio_less_impl_1<ratio<-_R2::num, _R2::den>,
           ratio<-_R1::num, _R1::den> >::type
    { };




  template<typename _R1, typename _R2>
    struct ratio_less
    : __ratio_less_impl<_R1, _R2>::type
    {
      static_assert(std::__are_both_ratios<_R1, _R2>(),
      "both template arguments must be a std::ratio");
    };


  template<typename _R1, typename _R2>
    struct ratio_less_equal
    : integral_constant<bool, !ratio_less<_R2, _R1>::value>
    { };


  template<typename _R1, typename _R2>
    struct ratio_greater
    : integral_constant<bool, ratio_less<_R2, _R1>::value>
    { };


  template<typename _R1, typename _R2>
    struct ratio_greater_equal
    : integral_constant<bool, !ratio_less<_R1, _R2>::value>
    { };
# 489 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 3
  template<typename _R1, typename _R2,
      bool = (_R1::num >= 0),
      bool = (_R2::num >= 0),
      bool = ratio_less<ratio<__static_abs<_R1::num>::value, _R1::den>,
        ratio<__static_abs<_R2::num>::value, _R2::den> >::value>
    struct __ratio_add_impl
    {
    private:
      typedef typename __ratio_add_impl<
        ratio<-_R1::num, _R1::den>,
        ratio<-_R2::num, _R2::den> >::type __t;
    public:
      typedef ratio<-__t::num, __t::den> type;
    };


  template<typename _R1, typename _R2, bool __b>
    struct __ratio_add_impl<_R1, _R2, true, true, __b>
    {
    private:
      static constexpr uintmax_t __g = __static_gcd<_R1::den, _R2::den>::value;
      static constexpr uintmax_t __d2 = _R2::den / __g;
      typedef __big_mul<_R1::den, __d2> __d;
      typedef __big_mul<_R1::num, _R2::den / __g> __x;
      typedef __big_mul<_R2::num, _R1::den / __g> __y;
      typedef __big_add<__x::__hi, __x::__lo, __y::__hi, __y::__lo> __n;
      static_assert(__n::__hi >= __x::__hi, "Internal library error");
      typedef __big_div<__n::__hi, __n::__lo, __g> __ng;
      static constexpr uintmax_t __g2 = __static_gcd<__ng::__rem, __g>::value;
      typedef __big_div<__n::__hi, __n::__lo, __g2> __n_final;
      static_assert(__n_final::__rem == 0, "Internal library error");
      static_assert(__n_final::__quot_hi == 0 &&
        __n_final::__quot_lo <= 0x7fffffffffffffffL, "overflow in addition");
      typedef __big_mul<_R1::den / __g2, __d2> __d_final;
      static_assert(__d_final::__hi == 0 &&
        __d_final::__lo <= 0x7fffffffffffffffL, "overflow in addition");
    public:
      typedef ratio<__n_final::__quot_lo, __d_final::__lo> type;
    };

  template<typename _R1, typename _R2>
    struct __ratio_add_impl<_R1, _R2, false, true, true>
    : __ratio_add_impl<_R2, _R1>
    { };


  template<typename _R1, typename _R2>
    struct __ratio_add_impl<_R1, _R2, true, false, false>
    {
    private:
      static constexpr uintmax_t __g = __static_gcd<_R1::den, _R2::den>::value;
      static constexpr uintmax_t __d2 = _R2::den / __g;
      typedef __big_mul<_R1::den, __d2> __d;
      typedef __big_mul<_R1::num, _R2::den / __g> __x;
      typedef __big_mul<-_R2::num, _R1::den / __g> __y;
      typedef __big_sub<__x::__hi, __x::__lo, __y::__hi, __y::__lo> __n;
      typedef __big_div<__n::__hi, __n::__lo, __g> __ng;
      static constexpr uintmax_t __g2 = __static_gcd<__ng::__rem, __g>::value;
      typedef __big_div<__n::__hi, __n::__lo, __g2> __n_final;
      static_assert(__n_final::__rem == 0, "Internal library error");
      static_assert(__n_final::__quot_hi == 0 &&
        __n_final::__quot_lo <= 0x7fffffffffffffffL, "overflow in addition");
      typedef __big_mul<_R1::den / __g2, __d2> __d_final;
      static_assert(__d_final::__hi == 0 &&
        __d_final::__lo <= 0x7fffffffffffffffL, "overflow in addition");
    public:
      typedef ratio<__n_final::__quot_lo, __d_final::__lo> type;
    };

  template<typename _R1, typename _R2>
    struct __ratio_add
    {
      static_assert(std::__are_both_ratios<_R1, _R2>(),
      "both template arguments must be a std::ratio");

      typedef typename __ratio_add_impl<_R1, _R2>::type type;
      static constexpr intmax_t num = type::num;
      static constexpr intmax_t den = type::den;
    };


  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_add<_R1, _R2>::num;

  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_add<_R1, _R2>::den;





  template<typename _R1, typename _R2>
    using ratio_add = typename __ratio_add<_R1, _R2>::type;



  template<typename _R1, typename _R2>
    struct __ratio_subtract
    {
      typedef typename __ratio_add<
        _R1,
        ratio<-_R2::num, _R2::den>>::type type;

      static constexpr intmax_t num = type::num;
      static constexpr intmax_t den = type::den;
    };


  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_subtract<_R1, _R2>::num;

  template<typename _R1, typename _R2>
    constexpr intmax_t __ratio_subtract<_R1, _R2>::den;





  template<typename _R1, typename _R2>
    using ratio_subtract = typename __ratio_subtract<_R1, _R2>::type;
# 620 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 3
  using atto = ratio< 1, 1000000000000000000>;
  using femto = ratio< 1, 1000000000000000>;
  using pico = ratio< 1, 1000000000000>;
  using nano = ratio< 1, 1000000000>;
  using micro = ratio< 1, 1000000>;
  using milli = ratio< 1, 1000>;
  using centi = ratio< 1, 100>;
  using deci = ratio< 1, 10>;
  using deca = ratio< 10, 1>;
  using hecto = ratio< 100, 1>;
  using kilo = ratio< 1000, 1>;
  using mega = ratio< 1000000, 1>;
  using giga = ratio< 1000000000, 1>;
  using tera = ratio< 1000000000000, 1>;
  using peta = ratio< 1000000000000000, 1>;
  using exa = ratio< 1000000000000000000, 1>;
# 648 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ratio" 3

}
# 40 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 1 3
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
#pragma GCC diagnostic ignored "-Wlong-long"
#pragma GCC diagnostic ignored "-Wc++23-extensions"
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
# 1665 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
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


 return double(9.3132257461547852e-10) * _S_1pm16352();



      }

      static constexpr __float128
      max() noexcept
      {



 return (__float128(double(3.4028236692093843e+38))
    + double(3.7778931862957153e+22) + double(8.35584e+6))
   * _S_1p16256();



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


 return double(1.7936620343357659e-43) * _S_1pm16352();



      }

      static constexpr bool is_iec559 = has_signaling_NaN;
      static constexpr bool is_bounded = true;
      static constexpr bool is_modulo = false;

      static constexpr bool traps = false;
      static constexpr bool tinyness_before = false;
      static constexpr float_round_style round_style
 = round_to_nearest;


    private:
      static constexpr __float128
      _S_4p(__float128 __v) noexcept
      { return __v * __v * __v * __v; }

      static constexpr __float128
      _S_1pm4088() noexcept
      { return _S_4p( double(2.2250738585072014e-308)); }

      static constexpr __float128
      _S_1pm16352() noexcept
      { return _S_4p(_S_1pm4088()); }

      static constexpr __float128
      _S_1p4064() noexcept
      { return _S_4p( double(7.0222388080559215e+305)); }

      static constexpr __float128
      _S_1p16256() noexcept
      { return _S_4p(_S_1p4064()); }

    };




}
# 2239 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/limits" 3
#pragma GCC diagnostic pop
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ctime" 1 3
# 47 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ctime" 3
# 1 "/usr/include/time.h" 1 3 4
# 29 "/usr/include/time.h" 3 4
extern "C" {







# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 1 3 4
# 214 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 38 "/usr/include/time.h" 2 3 4



# 1 "/usr/include/bits/time.h" 1 3 4
# 26 "/usr/include/bits/time.h" 3 4
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
# 27 "/usr/include/bits/time.h" 2 3 4



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



# 73 "/usr/include/time.h" 3 4


typedef __time_t time_t;



# 91 "/usr/include/time.h" 3 4
typedef __clockid_t clockid_t;
# 103 "/usr/include/time.h" 3 4
typedef __timer_t timer_t;
# 120 "/usr/include/time.h" 3 4
struct timespec
  {
    __time_t tv_sec;
    __syscall_slong_t tv_nsec;
  };








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





typedef __pid_t pid_t;
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
# 222 "/usr/include/time.h" 2 3 4

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
# 48 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ctime" 2 3
# 60 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ctime" 3
namespace std
{
  using ::clock_t;
  using ::time_t;
  using ::tm;

  using ::clock;
  using ::difftime;
  using ::mktime;
  using ::time;
  using ::asctime;
  using ::ctime;
  using ::gmtime;
  using ::localtime;
  using ::strftime;
}
# 44 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 2 3

# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/parse_numbers.h" 1 3
# 42 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/parse_numbers.h" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 1 3
# 36 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 3
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
# 37 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 2 3
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
# 38 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/ext/numeric_traits.h" 2 3

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
# 43 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/parse_numbers.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{


namespace __parse_int
{
  template<unsigned _Base, char _Dig>
    struct _Digit;

  template<unsigned _Base>
    struct _Digit<_Base, '0'> : integral_constant<unsigned, 0>
    {
      using __valid = true_type;
    };

  template<unsigned _Base>
    struct _Digit<_Base, '1'> : integral_constant<unsigned, 1>
    {
      using __valid = true_type;
    };

  template<unsigned _Base, unsigned _Val>
    struct _Digit_impl : integral_constant<unsigned, _Val>
    {
      static_assert(_Base > _Val, "invalid digit");
      using __valid = true_type;
    };

  template<unsigned _Base>
    struct _Digit<_Base, '2'> : _Digit_impl<_Base, 2>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '3'> : _Digit_impl<_Base, 3>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '4'> : _Digit_impl<_Base, 4>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '5'> : _Digit_impl<_Base, 5>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '6'> : _Digit_impl<_Base, 6>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '7'> : _Digit_impl<_Base, 7>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '8'> : _Digit_impl<_Base, 8>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, '9'> : _Digit_impl<_Base, 9>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'a'> : _Digit_impl<_Base, 0xa>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'A'> : _Digit_impl<_Base, 0xa>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'b'> : _Digit_impl<_Base, 0xb>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'B'> : _Digit_impl<_Base, 0xb>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'c'> : _Digit_impl<_Base, 0xc>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'C'> : _Digit_impl<_Base, 0xc>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'd'> : _Digit_impl<_Base, 0xd>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'D'> : _Digit_impl<_Base, 0xd>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'e'> : _Digit_impl<_Base, 0xe>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'E'> : _Digit_impl<_Base, 0xe>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'f'> : _Digit_impl<_Base, 0xf>
    { };

  template<unsigned _Base>
    struct _Digit<_Base, 'F'> : _Digit_impl<_Base, 0xf>
    { };


  template<unsigned _Base>
    struct _Digit<_Base, '\''> : integral_constant<unsigned, 0>
    {
      using __valid = false_type;
    };



  template<unsigned long long _Val>
    using __ull_constant = integral_constant<unsigned long long, _Val>;

  template<unsigned _Base, char _Dig, char... _Digs>
    struct _Power_help
    {
      using __next = typename _Power_help<_Base, _Digs...>::type;
      using __valid_digit = typename _Digit<_Base, _Dig>::__valid;
      using type
 = __ull_constant<__next::value * (__valid_digit{} ? _Base : 1ULL)>;
    };

  template<unsigned _Base, char _Dig>
    struct _Power_help<_Base, _Dig>
    {
      using __valid_digit = typename _Digit<_Base, _Dig>::__valid;
      using type = __ull_constant<__valid_digit::value>;
    };

  template<unsigned _Base, char... _Digs>
    struct _Power : _Power_help<_Base, _Digs...>::type
    { };

  template<unsigned _Base>
    struct _Power<_Base> : __ull_constant<0>
    { };



  template<unsigned _Base, unsigned long long _Pow, char _Dig, char... _Digs>
    struct _Number_help
    {
      using __digit = _Digit<_Base, _Dig>;
      using __valid_digit = typename __digit::__valid;
      using __next = _Number_help<_Base,
      __valid_digit::value ? _Pow / _Base : _Pow,
      _Digs...>;
      using type = __ull_constant<_Pow * __digit::value + __next::type::value>;
      static_assert((type::value / _Pow) == __digit::value,
      "integer literal does not fit in unsigned long long");
    };


  template<unsigned _Base, unsigned long long _Pow, char _Dig, char..._Digs>
    struct _Number_help<_Base, _Pow, '\'', _Dig, _Digs...>
    : _Number_help<_Base, _Pow, _Dig, _Digs...>
    { };


  template<unsigned _Base, char _Dig>
    struct _Number_help<_Base, 1ULL, _Dig>
    {
      using type = __ull_constant<_Digit<_Base, _Dig>::value>;
    };

  template<unsigned _Base, char... _Digs>
    struct _Number
    : _Number_help<_Base, _Power<_Base, _Digs...>::value, _Digs...>::type
    { };

  template<unsigned _Base>
    struct _Number<_Base>
    : __ull_constant<0>
    { };



  template<char... _Digs>
    struct _Parse_int;

  template<char... _Digs>
    struct _Parse_int<'0', 'b', _Digs...>
    : _Number<2U, _Digs...>::type
    { };

  template<char... _Digs>
    struct _Parse_int<'0', 'B', _Digs...>
    : _Number<2U, _Digs...>::type
    { };

  template<char... _Digs>
    struct _Parse_int<'0', 'x', _Digs...>
    : _Number<16U, _Digs...>::type
    { };

  template<char... _Digs>
    struct _Parse_int<'0', 'X', _Digs...>
    : _Number<16U, _Digs...>::type
    { };

  template<char... _Digs>
    struct _Parse_int<'0', _Digs...>
    : _Number<8U, _Digs...>::type
    { };

  template<char... _Digs>
    struct _Parse_int
    : _Number<10U, _Digs...>::type
    { };

}


namespace __select_int
{
  template<unsigned long long _Val, typename... _Ints>
    struct _Select_int_base;

  template<unsigned long long _Val, typename _IntType, typename... _Ints>
    struct _Select_int_base<_Val, _IntType, _Ints...>
    : __conditional_t<(_Val <= __gnu_cxx::__int_traits<_IntType>::__max),
        integral_constant<_IntType, (_IntType)_Val>,
        _Select_int_base<_Val, _Ints...>>
    { };

  template<unsigned long long _Val>
    struct _Select_int_base<_Val>
    { };

  template<char... _Digs>
    using _Select_int = typename _Select_int_base<
 __parse_int::_Parse_int<_Digs...>::value,
 unsigned char,
 unsigned short,
 unsigned int,
 unsigned long,
 unsigned long long
      >::type;

}


}
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 2 3





# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 52 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{






  namespace chrono
  {




    template<typename _Rep, typename _Period = ratio<1>>
      class duration;


    template<typename _Clock, typename _Dur = typename _Clock::duration>
      class time_point;

  }
# 83 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
  template<typename _CT, typename _Period1, typename _Period2, typename = void>
    struct __duration_common_type
    { };

  template<typename _CT, typename _Period1, typename _Period2>
    struct __duration_common_type<_CT, _Period1, _Period2,
      __void_t<typename _CT::type>>
    {
    private:
      using __gcd_num = __static_gcd<_Period1::num, _Period2::num>;
      using __gcd_den = __static_gcd<_Period1::den, _Period2::den>;
      using __cr = typename _CT::type;
      using __r = ratio<__gcd_num::value,
   (_Period1::den / __gcd_den::value) * _Period2::den>;

    public:
      using type = chrono::duration<__cr, typename __r::type>;
    };







  template<typename _Rep1, typename _Period1, typename _Rep2, typename _Period2>
    struct common_type<chrono::duration<_Rep1, _Period1>,
         chrono::duration<_Rep2, _Period2>>
    : __duration_common_type<common_type<_Rep1, _Rep2>,
        typename _Period1::type,
        typename _Period2::type>
    { };


  template<typename _Rep, typename _Period>
    struct common_type<chrono::duration<_Rep, _Period>,
         chrono::duration<_Rep, _Period>>
    {
      using type = chrono::duration<typename common_type<_Rep>::type,
        typename _Period::type>;
    };


  template<typename _Rep, typename _Period>
    struct common_type<chrono::duration<_Rep, _Period>>
    {
      using type = chrono::duration<typename common_type<_Rep>::type,
        typename _Period::type>;
    };






  template<typename _CT, typename _Clock, typename = void>
    struct __timepoint_common_type
    { };

  template<typename _CT, typename _Clock>
    struct __timepoint_common_type<_CT, _Clock, __void_t<typename _CT::type>>
    {
      using type = chrono::time_point<_Clock, typename _CT::type>;
    };







  template<typename _Clock, typename _Duration1, typename _Duration2>
    struct common_type<chrono::time_point<_Clock, _Duration1>,
         chrono::time_point<_Clock, _Duration2>>
    : __timepoint_common_type<common_type<_Duration1, _Duration2>, _Clock>
    { };


  template<typename _Clock, typename _Duration>
    struct common_type<chrono::time_point<_Clock, _Duration>,
         chrono::time_point<_Clock, _Duration>>
    { using type = chrono::time_point<_Clock, _Duration>; };


  template<typename _Clock, typename _Duration>
    struct common_type<chrono::time_point<_Clock, _Duration>>
    { using type = chrono::time_point<_Clock, _Duration>; };




  namespace chrono
  {






    template<typename _ToDur, typename _CF, typename _CR,
      bool _NumIsOne = false, bool _DenIsOne = false>
      struct __duration_cast_impl
      {
 template<typename _Rep, typename _Period>
   static constexpr _ToDur
   __cast(const duration<_Rep, _Period>& __d)
   {
     typedef typename _ToDur::rep __to_rep;
     return _ToDur(static_cast<__to_rep>(static_cast<_CR>(__d.count())
       * static_cast<_CR>(_CF::num)
       / static_cast<_CR>(_CF::den)));
   }
      };

    template<typename _ToDur, typename _CF, typename _CR>
      struct __duration_cast_impl<_ToDur, _CF, _CR, true, true>
      {
 template<typename _Rep, typename _Period>
   static constexpr _ToDur
   __cast(const duration<_Rep, _Period>& __d)
   {
     typedef typename _ToDur::rep __to_rep;
     return _ToDur(static_cast<__to_rep>(__d.count()));
   }
      };

    template<typename _ToDur, typename _CF, typename _CR>
      struct __duration_cast_impl<_ToDur, _CF, _CR, true, false>
      {
 template<typename _Rep, typename _Period>
   static constexpr _ToDur
   __cast(const duration<_Rep, _Period>& __d)
   {
     typedef typename _ToDur::rep __to_rep;
     return _ToDur(static_cast<__to_rep>(
       static_cast<_CR>(__d.count()) / static_cast<_CR>(_CF::den)));
   }
      };

    template<typename _ToDur, typename _CF, typename _CR>
      struct __duration_cast_impl<_ToDur, _CF, _CR, false, true>
      {
 template<typename _Rep, typename _Period>
   static constexpr _ToDur
   __cast(const duration<_Rep, _Period>& __d)
   {
     typedef typename _ToDur::rep __to_rep;
     return _ToDur(static_cast<__to_rep>(
       static_cast<_CR>(__d.count()) * static_cast<_CR>(_CF::num)));
   }
      };

    template<typename _Tp>
      struct __is_duration
      : std::false_type
      { };

    template<typename _Rep, typename _Period>
      struct __is_duration<duration<_Rep, _Period>>
      : std::true_type
      { };

    template<typename _Tp>
      using __enable_if_is_duration
 = typename enable_if<__is_duration<_Tp>::value, _Tp>::type;

    template<typename _Tp>
      using __disable_if_is_duration
 = typename enable_if<!__is_duration<_Tp>::value, _Tp>::type;
# 276 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _ToDur, typename _Rep, typename _Period>
      [[__nodiscard__]]
      constexpr __enable_if_is_duration<_ToDur>
      duration_cast(const duration<_Rep, _Period>& __d)
      {






   using __to_period = typename _ToDur::period;
   using __to_rep = typename _ToDur::rep;
   using __cf = ratio_divide<_Period, __to_period>;
   using __cr = typename common_type<__to_rep, _Rep, intmax_t>::type;
   using __dc = __duration_cast_impl<_ToDur, __cf, __cr,
         __cf::num == 1, __cf::den == 1>;
   return __dc::__cast(__d);



      }
# 310 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Rep>
      struct treat_as_floating_point
      : is_floating_point<_Rep>
      { };
# 478 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    namespace __detail
    {
      template<typename _Tp, typename _Up>
 constexpr _Tp
 __ceil_impl(const _Tp& __t, const _Up& __u)
 {
   return (__t < __u) ? (__t + _Tp{1}) : __t;
 }


      template<typename _ToDur, typename _Rep, typename _Period>
 constexpr _ToDur
 ceil(const duration<_Rep, _Period>& __d)
 {
   return __detail::__ceil_impl(chrono::duration_cast<_ToDur>(__d), __d);
 }
    }



    template<typename _Rep>
      struct duration_values
      {
 static constexpr _Rep
 zero() noexcept
 { return _Rep(0); }

 static constexpr _Rep
 max() noexcept
 { return numeric_limits<_Rep>::max(); }

 static constexpr _Rep
 min() noexcept
 { return numeric_limits<_Rep>::lowest(); }
      };

    template<typename _Rep, typename _Period>
      class duration
      {
 static_assert(!__is_duration<_Rep>::value,
        "rep cannot be a std::chrono::duration");
 static_assert(__is_ratio<_Period>::value,
        "period must be a specialization of std::ratio");
 static_assert(_Period::num > 0, "period must be positive");

 template<typename _Rep2>
   using __is_float = treat_as_floating_point<_Rep2>;

 static constexpr intmax_t
 _S_gcd(intmax_t __m, intmax_t __n) noexcept
 {



   do
     {
       intmax_t __rem = __m % __n;
       __m = __n;
       __n = __rem;
     }
   while (__n != 0);
   return __m;





 }





 template<typename _R1, typename _R2,
   intmax_t __gcd1 = _S_gcd(_R1::num, _R2::num),
   intmax_t __gcd2 = _S_gcd(_R1::den, _R2::den)>
   using __divide = ratio<(_R1::num / __gcd1) * (_R2::den / __gcd2),
     (_R1::den / __gcd2) * (_R2::num / __gcd1)>;


 template<typename _Period2>
   using __is_harmonic
     = __bool_constant<__divide<_Period2, _Period>::den == 1>;

      public:

 using rep = _Rep;
 using period = typename _Period::type;


 constexpr duration() = default;

 duration(const duration&) = default;



 template<typename _Rep2, typename = _Require<
   is_convertible<const _Rep2&, rep>,
   __or_<__is_float<rep>, __not_<__is_float<_Rep2>>>>>
   constexpr explicit duration(const _Rep2& __rep)
   : __r(static_cast<rep>(__rep)) { }

 template<typename _Rep2, typename _Period2, typename = _Require<
   is_convertible<const _Rep2&, rep>,
   __or_<__is_float<rep>,
         __and_<__is_harmonic<_Period2>,
         __not_<__is_float<_Rep2>>>>>>
   constexpr duration(const duration<_Rep2, _Period2>& __d)
   : __r(duration_cast<duration>(__d).count()) { }

 ~duration() = default;
 duration& operator=(const duration&) = default;


 constexpr rep
 count() const
 { return __r; }



 constexpr duration<typename common_type<rep>::type, period>
 operator+() const
 { return duration<typename common_type<rep>::type, period>(__r); }

 constexpr duration<typename common_type<rep>::type, period>
 operator-() const
 { return duration<typename common_type<rep>::type, period>(-__r); }

 duration&
 operator++()
 {
   ++__r;
   return *this;
 }

 duration
 operator++(int)
 { return duration(__r++); }

 duration&
 operator--()
 {
   --__r;
   return *this;
 }

 duration
 operator--(int)
 { return duration(__r--); }

 duration&
 operator+=(const duration& __d)
 {
   __r += __d.count();
   return *this;
 }

 duration&
 operator-=(const duration& __d)
 {
   __r -= __d.count();
   return *this;
 }

 duration&
 operator*=(const rep& __rhs)
 {
   __r *= __rhs;
   return *this;
 }

 duration&
 operator/=(const rep& __rhs)
 {
   __r /= __rhs;
   return *this;
 }


 template<typename _Rep2 = rep>
  
   __enable_if_t<!treat_as_floating_point<_Rep2>::value, duration&>
   operator%=(const rep& __rhs)
   {
     __r %= __rhs;
     return *this;
   }

 template<typename _Rep2 = rep>
  
   __enable_if_t<!treat_as_floating_point<_Rep2>::value, duration&>
   operator%=(const duration& __d)
   {
     __r %= __d.count();
     return *this;
   }


 static constexpr duration
 zero() noexcept
 { return duration(duration_values<rep>::zero()); }

 static constexpr duration
 min() noexcept
 { return duration(duration_values<rep>::min()); }

 static constexpr duration
 max() noexcept
 { return duration(duration_values<rep>::max()); }

      private:
 rep __r;
      };





    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr typename common_type<duration<_Rep1, _Period1>,
         duration<_Rep2, _Period2>>::type
      operator+(const duration<_Rep1, _Period1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<__dur1,__dur2>::type __cd;
 return __cd(__cd(__lhs).count() + __cd(__rhs).count());
      }


    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr typename common_type<duration<_Rep1, _Period1>,
         duration<_Rep2, _Period2>>::type
      operator-(const duration<_Rep1, _Period1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<__dur1,__dur2>::type __cd;
 return __cd(__cd(__lhs).count() - __cd(__rhs).count());
      }
# 731 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Rep1, typename _Rep2,
      typename _CRep = typename common_type<_Rep1, _Rep2>::type>
      using __common_rep_t = typename
 enable_if<is_convertible<const _Rep2&, _CRep>::value, _CRep>::type;
# 743 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Rep1, typename _Period, typename _Rep2>
      constexpr duration<__common_rep_t<_Rep1, _Rep2>, _Period>
      operator*(const duration<_Rep1, _Period>& __d, const _Rep2& __s)
      {
 typedef duration<typename common_type<_Rep1, _Rep2>::type, _Period>
   __cd;
 return __cd(__cd(__d).count() * __s);
      }

    template<typename _Rep1, typename _Rep2, typename _Period>
      constexpr duration<__common_rep_t<_Rep2, _Rep1>, _Period>
      operator*(const _Rep1& __s, const duration<_Rep2, _Period>& __d)
      { return __d * __s; }

    template<typename _Rep1, typename _Period, typename _Rep2>
      constexpr
      duration<__common_rep_t<_Rep1, __disable_if_is_duration<_Rep2>>, _Period>
      operator/(const duration<_Rep1, _Period>& __d, const _Rep2& __s)
      {
 typedef duration<typename common_type<_Rep1, _Rep2>::type, _Period>
   __cd;
 return __cd(__cd(__d).count() / __s);
      }

    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr typename common_type<_Rep1, _Rep2>::type
      operator/(const duration<_Rep1, _Period1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<__dur1,__dur2>::type __cd;
 return __cd(__lhs).count() / __cd(__rhs).count();
      }


    template<typename _Rep1, typename _Period, typename _Rep2>
      constexpr
      duration<__common_rep_t<_Rep1, __disable_if_is_duration<_Rep2>>, _Period>
      operator%(const duration<_Rep1, _Period>& __d, const _Rep2& __s)
      {
 typedef duration<typename common_type<_Rep1, _Rep2>::type, _Period>
   __cd;
 return __cd(__cd(__d).count() % __s);
      }

    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr typename common_type<duration<_Rep1, _Period1>,
         duration<_Rep2, _Period2>>::type
      operator%(const duration<_Rep1, _Period1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<__dur1,__dur2>::type __cd;
 return __cd(__cd(__lhs).count() % __cd(__rhs).count());
      }
# 811 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr bool
      operator==(const duration<_Rep1, _Period1>& __lhs,
   const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<__dur1,__dur2>::type __ct;
 return __ct(__lhs).count() == __ct(__rhs).count();
      }

    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr bool
      operator<(const duration<_Rep1, _Period1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<__dur1,__dur2>::type __ct;
 return __ct(__lhs).count() < __ct(__rhs).count();
      }
# 848 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr bool
      operator!=(const duration<_Rep1, _Period1>& __lhs,
   const duration<_Rep2, _Period2>& __rhs)
      { return !(__lhs == __rhs); }


    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr bool
      operator<=(const duration<_Rep1, _Period1>& __lhs,
   const duration<_Rep2, _Period2>& __rhs)
      { return !(__rhs < __lhs); }

    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr bool
      operator>(const duration<_Rep1, _Period1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      { return __rhs < __lhs; }

    template<typename _Rep1, typename _Period1,
      typename _Rep2, typename _Period2>
      constexpr bool
      operator>=(const duration<_Rep1, _Period1>& __lhs,
   const duration<_Rep2, _Period2>& __rhs)
      { return !(__lhs < __rhs); }
# 892 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    using nanoseconds = duration<int64_t, nano>;


    using microseconds = duration<int64_t, micro>;


    using milliseconds = duration<int64_t, milli>;


    using seconds = duration<int64_t>;


    using minutes = duration<int64_t, ratio< 60>>;


    using hours = duration<int64_t, ratio<3600>>;
# 925 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Clock, typename _Dur>
      class time_point
      {
 static_assert(__is_duration<_Dur>::value,
     "duration must be a specialization of std::chrono::duration");

      public:
 typedef _Clock clock;
 typedef _Dur duration;
 typedef typename duration::rep rep;
 typedef typename duration::period period;

 constexpr time_point() : __d(duration::zero())
 { }

 constexpr explicit time_point(const duration& __dur)
 : __d(__dur)
 { }


 template<typename _Dur2,
   typename = _Require<is_convertible<_Dur2, _Dur>>>
   constexpr time_point(const time_point<clock, _Dur2>& __t)
   : __d(__t.time_since_epoch())
   { }


 constexpr duration
 time_since_epoch() const
 { return __d; }
# 981 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
 time_point&
 operator+=(const duration& __dur)
 {
   __d += __dur;
   return *this;
 }

 time_point&
 operator-=(const duration& __dur)
 {
   __d -= __dur;
   return *this;
 }


 static constexpr time_point
 min() noexcept
 { return time_point(duration::min()); }

 static constexpr time_point
 max() noexcept
 { return time_point(duration::max()); }

      private:
 duration __d;
      };
# 1020 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _ToDur, typename _Clock, typename _Dur>
      [[__nodiscard__]] constexpr
      __enable_if_t<__is_duration<_ToDur>::value, time_point<_Clock, _ToDur>>
      time_point_cast(const time_point<_Clock, _Dur>& __t)
      {
 typedef time_point<_Clock, _ToDur> __time_point;
 return __time_point(duration_cast<_ToDur>(__t.time_since_epoch()));
      }
# 1101 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Clock, typename _Dur1,
      typename _Rep2, typename _Period2>
      constexpr time_point<_Clock,
 typename common_type<_Dur1, duration<_Rep2, _Period2>>::type>
      operator+(const time_point<_Clock, _Dur1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<_Dur1,__dur2>::type __ct;
 typedef time_point<_Clock, __ct> __time_point;
 return __time_point(__lhs.time_since_epoch() + __rhs);
      }


    template<typename _Rep1, typename _Period1,
      typename _Clock, typename _Dur2>
      constexpr time_point<_Clock,
 typename common_type<duration<_Rep1, _Period1>, _Dur2>::type>
      operator+(const duration<_Rep1, _Period1>& __lhs,
  const time_point<_Clock, _Dur2>& __rhs)
      {
 typedef duration<_Rep1, _Period1> __dur1;
 typedef typename common_type<__dur1,_Dur2>::type __ct;
 typedef time_point<_Clock, __ct> __time_point;
 return __time_point(__rhs.time_since_epoch() + __lhs);
      }


    template<typename _Clock, typename _Dur1,
      typename _Rep2, typename _Period2>
      constexpr time_point<_Clock,
 typename common_type<_Dur1, duration<_Rep2, _Period2>>::type>
      operator-(const time_point<_Clock, _Dur1>& __lhs,
  const duration<_Rep2, _Period2>& __rhs)
      {
 typedef duration<_Rep2, _Period2> __dur2;
 typedef typename common_type<_Dur1,__dur2>::type __ct;
 typedef time_point<_Clock, __ct> __time_point;
 return __time_point(__lhs.time_since_epoch() -__rhs);
      }


    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr typename common_type<_Dur1, _Dur2>::type
      operator-(const time_point<_Clock, _Dur1>& __lhs,
  const time_point<_Clock, _Dur2>& __rhs)
      { return __lhs.time_since_epoch() - __rhs.time_since_epoch(); }







    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr bool
      operator==(const time_point<_Clock, _Dur1>& __lhs,
   const time_point<_Clock, _Dur2>& __rhs)
      { return __lhs.time_since_epoch() == __rhs.time_since_epoch(); }
# 1169 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr bool
      operator!=(const time_point<_Clock, _Dur1>& __lhs,
   const time_point<_Clock, _Dur2>& __rhs)
      { return !(__lhs == __rhs); }


    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr bool
      operator<(const time_point<_Clock, _Dur1>& __lhs,
  const time_point<_Clock, _Dur2>& __rhs)
      { return __lhs.time_since_epoch() < __rhs.time_since_epoch(); }

    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr bool
      operator<=(const time_point<_Clock, _Dur1>& __lhs,
   const time_point<_Clock, _Dur2>& __rhs)
      { return !(__rhs < __lhs); }

    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr bool
      operator>(const time_point<_Clock, _Dur1>& __lhs,
  const time_point<_Clock, _Dur2>& __rhs)
      { return __rhs < __lhs; }

    template<typename _Clock, typename _Dur1, typename _Dur2>
      constexpr bool
      operator>=(const time_point<_Clock, _Dur1>& __lhs,
   const time_point<_Clock, _Dur2>& __rhs)
      { return !(__lhs < __rhs); }
# 1222 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
inline namespace _V2 {







    struct system_clock
    {
      typedef chrono::nanoseconds duration;
      typedef duration::rep rep;
      typedef duration::period period;
      typedef chrono::time_point<system_clock, duration> time_point;

      static_assert(system_clock::duration::min()
      < system_clock::duration::zero(),
      "a clock's minimum duration cannot be less than its epoch");

      static constexpr bool is_steady = false;

      static time_point
      now() noexcept;


      static std::time_t
      to_time_t(const time_point& __t) noexcept
      {
 return std::time_t(duration_cast<chrono::seconds>
      (__t.time_since_epoch()).count());
      }

      static time_point
      from_time_t(std::time_t __t) noexcept
      {
 typedef chrono::time_point<system_clock, seconds> __from;
 return time_point_cast<system_clock::duration>
        (__from(chrono::seconds(__t)));
      }
    };
# 1270 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    struct steady_clock
    {
      typedef chrono::nanoseconds duration;
      typedef duration::rep rep;
      typedef duration::period period;
      typedef chrono::time_point<steady_clock, duration> time_point;

      static constexpr bool is_steady = true;

      static time_point
      now() noexcept;
    };
# 1292 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
    using high_resolution_clock = system_clock;

}
# 1327 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
  }


  inline namespace literals
  {
# 1356 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3
  inline namespace chrono_literals
  {



#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wliteral-suffix"

    template<typename _Dur, char... _Digits>
      constexpr _Dur __check_overflow()
      {
 using _Val = __parse_int::_Parse_int<_Digits...>;
 constexpr typename _Dur::rep __repval = _Val::value;
 static_assert(__repval >= 0 && __repval == _Val::value,
        "literal value cannot be represented by duration type");
 return _Dur(__repval);
      }



    constexpr chrono::duration<long double, ratio<3600,1>>
    operator""h(long double __hours)
    { return chrono::duration<long double, ratio<3600,1>>{__hours}; }


    template <char... _Digits>
      constexpr chrono::hours
      operator""h()
      { return __check_overflow<chrono::hours, _Digits...>(); }


    constexpr chrono::duration<long double, ratio<60,1>>
    operator""min(long double __mins)
    { return chrono::duration<long double, ratio<60,1>>{__mins}; }


    template <char... _Digits>
      constexpr chrono::minutes
      operator""min()
      { return __check_overflow<chrono::minutes, _Digits...>(); }


    constexpr chrono::duration<long double>
    operator""s(long double __secs)
    { return chrono::duration<long double>{__secs}; }


    template <char... _Digits>
      constexpr chrono::seconds
      operator""s()
      { return __check_overflow<chrono::seconds, _Digits...>(); }


    constexpr chrono::duration<long double, milli>
    operator""ms(long double __msecs)
    { return chrono::duration<long double, milli>{__msecs}; }


    template <char... _Digits>
      constexpr chrono::milliseconds
      operator""ms()
      { return __check_overflow<chrono::milliseconds, _Digits...>(); }


    constexpr chrono::duration<long double, micro>
    operator""us(long double __usecs)
    { return chrono::duration<long double, micro>{__usecs}; }


    template <char... _Digits>
      constexpr chrono::microseconds
      operator""us()
      { return __check_overflow<chrono::microseconds, _Digits...>(); }


    constexpr chrono::duration<long double, nano>
    operator""ns(long double __nsecs)
    { return chrono::duration<long double, nano>{__nsecs}; }


    template <char... _Digits>
      constexpr chrono::nanoseconds
      operator""ns()
      { return __check_overflow<chrono::nanoseconds, _Digits...>(); }

#pragma GCC diagnostic pop

  }
  }

  namespace chrono
  {
    using namespace literals::chrono_literals;
  }
# 1516 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/chrono.h" 3

}
# 46 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 2 3
# 61 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 3
# 1 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/bits/version.h" 1 3
# 62 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 2 3

namespace std __attribute__ ((__visibility__ ("default")))
{

# 80 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 3
  namespace chrono
  {
# 3346 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 3
  }
# 3374 "/mds/gnu/build/gcc-15-20250112/include/c++/15.0.0/chrono" 3

}
# 5 "./diagnostic/pr65923.C" 2


# 6 "./diagnostic/pr65923.C"
using std::literals::chrono_literals::operator""s;

struct X
{
  friend constexpr std::chrono::duration<long double> std::literals::chrono_literals::operator""s(long double);
};

struct X2
{
  friend constexpr X operator""foo(long double) {return {};}
};

namespace std
{
  template<> void swap(X&, X&) noexcept
  {
    constexpr std::chrono::duration<long double> operator""s(long double);
  }
}
